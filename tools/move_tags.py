import os, subprocess, sys


def git(*args):
    return subprocess.run(['git'] + list(args), capture_output=True, text=True, check=True).stdout


def tags():
    found = {}
    for line in git('ls-remote', '--tags', 'origin').splitlines():
        sha, ref = line.split('\t')
        name = ref[len('refs/tags/'):]
        if name.endswith('^{}'):
            found[name[:-3]] = sha
        else:
            found.setdefault(name, sha)
    return found


def branch():
    name = os.environ.get('GITHUB_REF_NAME') or git('rev-parse', '--abbrev-ref', 'HEAD').strip()
    return name if name and name != 'HEAD' else 'main'


def heads():
    found = []
    for line in git('ls-remote', '--heads', 'origin').splitlines():
        found.append(line.split('\t')[1][len('refs/heads/'):])
    return found


def kept_elsewhere(sha, here, live):
    for name in live:
        if name == here:
            continue
        if subprocess.run(['git', 'merge-base', '--is-ancestor', sha, 'origin/' + name]).returncode == 0:
            return name
    return None


def main_line(here):
    shas, trees, versions = set(), {}, {}
    for line in git('log', '--format=%H %T %s', 'origin/' + here).splitlines():
        sha, tree, subject = line.split(' ', 2)
        shas.add(sha)
        trees.setdefault(tree, []).append((sha, subject))
        if subject.startswith('[') and '] ' in subject:
            versions.setdefault(subject[1:subject.index('] ')], sha)
    return shas, trees, versions


def target(name, sha, trees, versions, here):
    version = name[1:] if name.startswith('v') else name
    same = trees.get(git('rev-parse', sha + '^{tree}').strip(), [])
    for one, subject in same:
        if subject.startswith('[' + version + '] '):
            return one, 'the commit on ' + here + ' with the same source'
    if version in versions:
        return versions[version], 'the commit on ' + here + ' that ships ' + version
    if same:
        return same[0][0], 'the commit on ' + here + ' with the same source'
    return None, ''


def main(argv):
    apply = '--apply' in argv
    here = branch()
    live = heads()
    shas, trees, versions = main_line(here)
    moves, stays, lost, elsewhere = [], 0, [], []
    for name, sha in sorted(tags().items()):
        if sha in shas:
            stays += 1
            continue
        other = kept_elsewhere(sha, here, live)
        if other:
            elsewhere.append((name, other))
            continue
        to, why = target(name, sha, trees, versions, here)
        if to is None:
            lost.append(name)
        else:
            moves.append((name, sha, to, why))
    for name, sha, to, why in moves:
        print('  move    ' + name + ' from ' + sha[:7] + ' to ' + to[:7] + ', ' + why)
    for name, other in elsewhere:
        print('  KEPT    ' + name + ' is on ' + other + ', the branch it was published from, so it stays there')
    for name in lost:
        print('  LOST    ' + name + ' has no commit on ' + here + ' that ships its version, so it stays where it is')
    print(str(stays) + ' tag(s) already on ' + here + ', ' + str(len(moves)) + ' to move, ' + str(len(elsewhere)) +
          ' on another kept branch, ' + str(len(lost)) + ' with nowhere to go')
    if not apply:
        print('dry run: nothing was moved')
        return 0
    if lost:
        print('nothing was moved, because a tag with nowhere to go has to be looked at first')
        return 1
    for at in range(0, len(moves), 50):
        git('push', '--force', 'origin', *[to + ':refs/tags/' + name for name, sha, to, why in moves[at:at + 50]])
    kept = set(name for name, other in elsewhere)
    left = sorted(name for name, sha in tags().items() if sha not in shas and name not in kept)
    print('moved ' + str(len(moves)) + ' tag(s), and ' + str(len(left)) + ' tag(s) still point off ' + here)
    for name in left:
        print('  OFF     ' + name)
    return 0 if not left else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
