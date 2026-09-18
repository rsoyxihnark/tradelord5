import io, json, os, subprocess, sys, time, urllib.request

REPO = 'rsoyxihnark/tradelord5'

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nexus_changelog import sections


def token():
    try:
        run = subprocess.run(['gh', 'auth', 'token'], capture_output=True, text=True)
        if run.returncode == 0 and run.stdout.strip():
            return run.stdout.strip(), 'gh'
    except OSError:
        pass
    held = os.environ.get('GH_TOKEN') or os.environ.get('GITHUB_TOKEN')
    if held:
        return held, 'the token in the environment'
    sys.stderr.write('no token: sign in with gh auth login, or set GH_TOKEN\n')
    sys.exit(1)


def ask(url, tok, data=None, method=None):
    req = urllib.request.Request(url, data=data, method=method, headers={
        'User-Agent': 'tradelord-release-notes',
        'Authorization': 'Bearer ' + tok,
        'Accept': 'application/vnd.github+json',
        'Content-Type': 'application/json'})
    with urllib.request.urlopen(req, timeout=60) as got:
        said = got.read()
    return json.loads(said.decode('utf-8')) if said.strip() else None


def releases(tok):
    found, page = [], 1
    while page <= 20:
        batch = ask('https://api.github.com/repos/' + REPO +
                    '/releases?per_page=100&page=' + str(page), tok)
        if not batch:
            break
        found += batch
        page += 1
    return found


def asked(argv):
    out, taking = [], False
    for arg in argv:
        if arg == '--drop':
            taking = True
            continue
        if arg.startswith('--'):
            taking = False
            continue
        if taking:
            out += [one.lstrip('v') for one in arg.replace(',', ' ').split() if one.strip()]
    return out


def drop(rows, book, wanted, tok, apply):
    live = {r['tag_name'].lstrip('v'): r for r in rows}
    for version in wanted:
        if book.get(version):
            sys.stderr.write(version + ' still has entries in CHANGELOG.md, so it is not dropped\n')
            return 1
        row = live.get(version)
        if row is None:
            print('  ' + version + ' has no release to drop')
            continue
        if not apply:
            print('  would throw away the release and the tag for ' + version)
            continue
        ask('https://api.github.com/repos/' + REPO + '/releases/' + str(row['id']), tok, None, 'DELETE')
        try:
            ask('https://api.github.com/repos/' + REPO + '/git/refs/tags/' + row['tag_name'],
                tok, None, 'DELETE')
        except Exception as why:
            print('  ' + version + ': the release is gone and the tag would not delete: ' + str(why))
            continue
        print('  threw away the release and the tag for ' + version)
    return 0


def main(argv):
    if not os.path.exists('CHANGELOG.md'):
        sys.stderr.write('run this from the repository root, where CHANGELOG.md is\n')
        return 1
    apply = '--apply' in argv
    wanted = asked(argv)
    tok, how = token()
    print('reading the releases through ' + how)
    book = dict(sections(io.open('CHANGELOG.md', encoding='utf-8').read()))
    rows = releases(tok)
    if wanted:
        stopped = drop(rows, book, wanted, tok, apply)
        if stopped:
            return stopped
        if apply:
            rows = releases(tok)
    absent = sorted(r['tag_name'] for r in rows if r['tag_name'].lstrip('v') not in book)
    if absent:
        sys.stderr.write('CHANGELOG.md carries no section for: ' + ', '.join(absent) + '\n')
        return 1
    todo = []
    for r in rows:
        want = '\n'.join('- ' + one for one in book[r['tag_name'].lstrip('v')])
        if (r.get('body') or '').strip() != want.strip():
            todo.append((r['id'], r['tag_name'], want))
    print(str(len(rows)) + ' releases read, ' + str(len(todo)) + ' to bring in line')
    if not apply:
        for _, tag, _ in todo[:10]:
            print('  would rewrite ' + tag)
        if len(todo) > 10:
            print('  and ' + str(len(todo) - 10) + ' more')
        print('\nrun it again with --apply to write them')
        return 0
    done, failed = 0, []
    for rid, tag, want in todo:
        for attempt in range(1, 5):
            try:
                ask('https://api.github.com/repos/' + REPO + '/releases/' + str(rid),
                    tok, json.dumps({'body': want}).encode('utf-8'), 'PATCH')
                done += 1
                break
            except Exception as why:
                if attempt == 4:
                    failed.append(tag + ': ' + str(why))
                else:
                    time.sleep(2 ** attempt)
        time.sleep(0.1)
    print('rewritten: ' + str(done) + ', failed: ' + str(len(failed)))
    for one in failed:
        print('  ' + one)
    return 1 if failed else 0


if __name__ == '__main__':
    sys.exit(main(sys.argv))
