import subprocess, sys


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


def main_line():
    shas, trees, versions = set(), {}, {}
    for line in git('log', '--format=%H %T %s', 'origin/main').splitlines():
        sha, tree, subject = line.split(' ', 2)
        shas.add(sha)
        trees.setdefault(tree, []).append((sha, subject))
        if subject.startswith('[') and '] ' in subject:
            versions.setdefault(subject[1:subject.index('] ')], sha)
    return shas, trees, versions


def target(name, sha, trees, versions):
    version = name[1:] if name.startswith('v') else name
    same = trees.get(git('rev-parse', sha + '^{tree}').strip(), [])
    for one, subject in same:
        if subject.startswith('[' + version + '] '):
            return one, 'the commit on main with the same source'
    if version in versions:
        return versions[version], 'the commit on main that ships ' + version
    if same:
        return same[0][0], 'the commit on main with the same source'
    return None, ''


def main(argv):
    apply = '--apply' in argv
    shas, trees, versions = main_line()
    moves, stays, lost = [], 0, []
    for name, sha in sorted(tags().items()):
        if sha in shas:
            stays += 1
            continue
        to, why = target(name, sha, trees, versions)
        if to is None:
            lost.append(name)
        else:
            moves.append((name, sha, to, why))
    for name, sha, to, why in moves:
        print('  move    ' + name + ' from ' + sha[:7] + ' to ' + to[:7] + ', ' + why)
    for name in lost:
        print('  LOST    ' + name + ' has no commit on main that ships its version, so it stays where it is')
    print(str(stays) + ' tag(s) already on main, ' + str(len(moves)) + ' to move, ' + str(len(lost)) + ' with nowhere to go')
    if not apply:
        print('dry run: nothing was moved')
        return 0
    if lost:
        print('nothing was moved, because a tag with nowhere to go has to be looked at first')
        return 1
    for at in range(0, len(moves), 50):
        git('push', '--force', 'origin', *[to + ':refs/tags/' + name for name, sha, to, why in moves[at:at + 50]])
    left = sorted(name for name, sha in tags().items() if sha not in shas)
    print('moved ' + str(len(moves)) + ' tag(s), and ' + str(len(left)) + ' tag(s) still point off main')
    for name in left:
        print('  OFF     ' + name)
    return 0 if not left else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv[1:]))
