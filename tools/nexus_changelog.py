import io, re, sys

HEADING = re.compile(r'^##\s+(.+?)\s*$')
ENTRY = re.compile(r'^-\s+(.*\S)\s*$')
ANY_HEADING = re.compile(r'^#{1,6}\s+(.+?)\s*$')
BOLD = re.compile(r'\*\*(.+?)\*\*')
CODE = re.compile(r'`([^`]+)`')

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

def blocks(text):
    held, kind = [], None
    for line in text.split('\n'):
        bare = line.strip()
        if not bare:
            if held:
                yield kind, held
            held, kind = [], None
            continue
        head = ANY_HEADING.match(bare)
        if head:
            if held:
                yield kind, held
            yield 'heading', [head.group(1)]
            held, kind = [], None
            continue
        entry = ENTRY.match(bare)
        wanted = 'list' if entry else 'text'
        if held and kind != wanted:
            yield kind, held
            held = []
        kind = wanted
        held.append(entry.group(1) if entry else bare)
    if held:
        yield kind, held

def marked(line):
    return BOLD.sub(r'[b]\1[/b]', CODE.sub(r'\1', line))

def listed(lines):
    return '[list]\n' + '\n'.join('[*]' + marked(one) for one in lines) + '\n[/list]'

def markup(text):
    out = []
    for kind, held in blocks(text):
        if kind == 'heading':
            out.append('[size=5][b]' + marked(held[0]) + '[/b][/size]')
        elif kind == 'list':
            out.append(listed(held))
        else:
            out.append(marked(' '.join(held)))
    return '\n\n'.join(out)

def page(version, entries):
    return '\n\n'.join([
        markup(io.open('README.md', encoding='utf-8').read()),
        markup(io.open('COMPARISON.md', encoding='utf-8').read()),
        '[size=5][b]Changelog[/b][/size]',
        '[b]' + version + '[/b]',
        listed(entries),
    ]) + '\n'

def said(text):
    if hasattr(sys.stdout, 'buffer'):
        sys.stdout.buffer.write(text.encode('utf-8'))
    else:
        sys.stdout.write(text)

def main(argv):
    flags = [arg for arg in argv[1:] if arg.startswith('--')]
    rest = [arg for arg in argv[1:] if not arg.startswith('--')]
    for flag in flags:
        if flag not in ('--notes', '--page'):
            sys.stderr.write(flag + ' is not something this tool knows\n')
            return 1
    as_notes = '--notes' in flags
    as_page = '--page' in flags
    if as_notes and as_page:
        sys.stderr.write('--notes writes the release notes and --page writes the mod page - ask for one\n')
        return 1
    wanted = rest[0] if rest else None
    text = io.open('CHANGELOG.md', encoding='utf-8').read()
    found = sections(text)
    if not found:
        sys.stderr.write('CHANGELOG.md carries no version section\n')
        return 1
    if wanted is None and found[0][0].lower() == 'unreleased':
        sys.stderr.write('the top section is Unreleased, which has not shipped - name a version first\n')
        return 1
    found = [(v, said) for v, said in found if v.lower() != 'unreleased']
    shown = [(v, said) for v, said in found if wanted is None or v == wanted]
    if not shown:
        sys.stderr.write('CHANGELOG.md carries no section for ' + wanted + '\n')
        return 1
    if as_page:
        version, entries = shown[0]
        if not entries:
            sys.stderr.write(version + ' has no entries\n')
            return 1
        said(page(version, entries))
        return 0
    for version, entries in shown:
        if not entries:
            sys.stderr.write(version + ' has no entries\n')
            return 1
        if as_notes:
            said('\n'.join('- ' + line for line in entries) + '\n')
            continue
        said('[' + version + ']\n')
        said('\n'.join(entries) + '\n\n')
    return 0

if __name__ == '__main__':
    sys.exit(main(sys.argv))
