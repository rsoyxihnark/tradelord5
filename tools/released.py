import io, json, os, re, subprocess, sys, urllib.request

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from nexus_changelog import sections

REPO = 'rsoyxihnark/tradelord5'

OUTSTANDING = {
    '1.14.1': 'the section and the notes each carry lines the other does not',
    '1.15.0': 'the one entry was reworded on one side only',
    '1.16.0': 'the one entry was reworded on one side only',
    '1.34.0': 'the section gained two entries after the release went out',
    '1.35.0': 'the section gained two entries after the release went out',
    '1.50.0': 'the notes carry four entries the section never got',
    '1.50.1': 'the section was replaced by a summary, and the notes hold the six entries that shipped',
}


def order(version):
    parts = [int(p) for p in version.split('.')]
    while len(parts) < 3:
        parts.append(0)
    return tuple(parts)


def numbered(heading):
    return re.fullmatch(r'\d+\.\d+(\.\d+)?', heading) is not None


def byGh():
    try:
        run = subprocess.run(
            ['gh', 'api', 'repos/{owner}/{repo}/releases', '--paginate', '--jq',
             '.[] | {tag: .tag_name, draft: .draft, body: .body, files: (.assets | length)}'],
            capture_output=True, text=True)
    except OSError as missing:
        return None, str(missing)
    if run.returncode != 0:
        return None, run.stderr.strip().split('\n')[-1]
    return [json.loads(line) for line in run.stdout.split('\n') if line.strip()], None


def byApi():
    found, page = [], 1
    while page <= 10:
        ask = urllib.request.Request(
            'https://api.github.com/repos/' + REPO + '/releases?per_page=100&page=' + str(page),
            headers={'User-Agent': 'tradelord-released'})
        try:
            batch = json.load(urllib.request.urlopen(ask, timeout=30))
        except Exception as unreachable:
            return None, str(unreachable)
        if not batch:
            break
        for one in batch:
            found.append({'tag': one['tag_name'], 'draft': one['draft'],
                          'body': one.get('body') or '', 'files': len(one.get('assets') or [])})
        page += 1
    return found, None


def asked():
    rows, why = byGh()
    if rows is not None:
        return rows, 'gh'
    rows, alsoWhy = byApi()
    if rows is not None:
        return rows, 'the public API'
    print('  skipped  the releases could not be read, so the changelog was not held against them')
    print('           gh said: ' + (why or 'nothing'))
    print('           the API said: ' + (alsoWhy or 'nothing'))
    return None, None


def notes(said):
    return [line.strip() for line in said if line.strip()]


def bulleted(body):
    return [line.strip()[2:].strip() for line in body.split('\n') if line.strip().startswith('- ')]


def main(argv):
    shipping = (argv[1] if len(argv) > 1 else '').lstrip('v')
    rows, how = asked()
    if rows is None:
        return 0

    live = {row['tag'].lstrip('v'): row for row in rows}
    said = {head: entries for head, entries in
            sections(io.open('CHANGELOG.md', encoding='utf-8').read())}
    versions = sorted([head for head in said if numbered(head)], key=order)
    if not live or not versions:
        print('  BROKEN   there are no releases or no versions to compare')
        return 1

    first = min(live, key=order)
    faults, skipped, outstanding = [], 0, []

    for version in sorted(live, key=order):
        row = live[version]
        if row['draft']:
            faults.append(version + ' is still a draft release')
        if row['files'] == 0:
            faults.append(version + ' was published with no file attached')
        if version not in said:
            faults.append(version + ' was published and the changelog carries no section for it')
            continue
        out = bulleted(row['body'])
        if not out:
            skipped += 1
            continue
        if out != notes(said[version]):
            if version in OUTSTANDING:
                outstanding.append(version + ': ' + OUTSTANDING[version])
            else:
                faults.append(version + ' says one thing in the changelog and another in its release notes')

    for version in versions:
        if order(version) < order(first) or version in live or version == shipping:
            continue
        faults.append(version + ' has a changelog section and was never published')

    print('  read ' + str(len(live)) + ' releases through ' + how + ', ' + str(len(versions)) +
          ' versions in the changelog, ' + first + ' the first ever published')
    if shipping:
        print('  ' + shipping + ' is the version this commit ships, so it is not expected out yet')
    if skipped:
        print('  ' + str(skipped) + ' releases predate the rule that notes are written as entries, ' +
              'so their wording was not compared')
    for one in outstanding:
        print('  note     ' + one)
    settled = [version for version in OUTSTANDING if version not in live or
               bulleted(live[version]['body']) == notes(said.get(version, []))]
    for version in sorted(settled, key=order):
        faults.append(version + ' is listed as an outstanding disagreement and no longer disagrees, ' +
                      'so take it out of the list in tools/released.py')
    if outstanding:
        print('  note     those ' + str(len(outstanding)) + ' are from before this check existed. Settle each ' +
              'by making one side match the other; a version that is not on that list and disagrees fails here')
    if not faults:
        print('  ok       every published version has its section, and every section that shipped is out')
        return 0
    for fault in faults:
        print('  BROKEN   ' + fault)
    return 1


if __name__ == '__main__':
    sys.exit(main(sys.argv))
