import io, re, sys

HEADING = re.compile(r'^##\s+(.+?)\s*$')
ENTRY = re.compile(r'^-\s+(.*\S)\s*$')

def sections(text):
    out, version, said = [], None, []
    for line in text.split('\n'):
        head = HEADING.match(line)
        if head:
            if version is not None:
                out.append((version, said))
            version, said = head.group(1), []
            continue
        entry = ENTRY.match(line)
        if entry and version is not None:
            said.append(entry.group(1))
    if version is not None:
        out.append((version, said))
    return out

def main(argv):
    wanted = argv[1] if len(argv) > 1 else None
    text = io.open('CHANGELOG.md', encoding='utf-8').read()
    found = sections(text)
    if not found:
        sys.stderr.write('CHANGELOG.md carries no version section\n')
        return 1
    if found[0][0].lower() == 'unreleased':
        sys.stderr.write('the top section is Unreleased, which has not shipped - name a version first\n')
        return 1
    shown = [(v, said) for v, said in found if wanted is None or v == wanted]
    if not shown:
        sys.stderr.write('CHANGELOG.md carries no section for ' + wanted + '\n')
        return 1
    for version, said in shown:
        if not said:
            sys.stderr.write(version + ' has no entries\n')
            return 1
        sys.stdout.write('[' + version + ']\n')
        sys.stdout.write('\n'.join(said) + '\n\n')
    return 0

if __name__ == '__main__':
    sys.exit(main(sys.argv))
