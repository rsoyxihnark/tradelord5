import io, re, sys

S = {f: io.open('src/' + f, encoding='utf-8').read() for f in
     ['Trading.cs', 'Ledger.cs', 'LedgerCodec.cs', 'TradeMath.cs', 'Confidence.cs', 'Panel.cs', 'Travel.cs', 'Support.cs',
      'Options.cs', 'TooltipPatches.cs', 'SubModule.cs', 'Market.cs']}
TESTS = io.open('tests/LedgerCodecTests.cs', encoding='utf-8').read()
MATHTESTS = io.open('tests/TradeMathTests.cs', encoding='utf-8').read()
ROUTETESTS = io.open('tests/RouteRulesTests.cs', encoding='utf-8').read()
TESTPROJ = io.open('tests/TradeLord.Tests.csproj', encoding='utf-8').read()
M = io.open('mcm/Settings.cs', encoding='utf-8').read()
WORKFLOW = io.open('.github/workflows/build.yml', encoding='utf-8').read()
PROJ = [io.open(f, encoding='utf-8').read() for f in
        ['src/TradeLord.csproj', 'mcm/TradeLord.MCM.csproj']]
PREFAB = io.open('TradeLord/GUI/Prefabs/TradeLordPanel.xml', encoding='utf-8').read()
COMPAT = io.open('tools/compat/Program.cs', encoding='utf-8').read()

def panel_columns():
    import xml.etree.ElementTree as ET
    root = ET.parse('TradeLord/GUI/Prefabs/TradeLordPanel.xml').getroot()
    def widths(kids):
        return [k.get('SuggestedWidth') if k.get('WidthSizePolicy') == 'Fixed'
                else k.get('WidthSizePolicy') for k in kids]
    head = next((widths(c) for lp in root.iter('ListPanel')
                 for c in [lp.find('Children')]
                 if c is not None and len(c) == 12 and all(k.tag == 'TextWidget' for k in c)), None)
    row = next((widths(c) for lp in root.find('.//ItemTemplate').iter('ListPanel')
                for c in [lp.find('Children')] if c is not None and len(c) == 12), None)
    return head, row

def panel_paths():
    import xml.etree.ElementTree as ET
    t = ET.parse('TradeLord/GUI/Prefabs/TradeLordPanel.xml')
    parent = {c: p for p in t.iter() for c in p}
    def up(node):
        node = parent.get(node)
        while node is not None and node.tag in ('Children', 'ItemTemplate'):
            node = parent.get(node)
        return node
    def kids(node):
        return [c for w in node if w.tag in ('Children', 'ItemTemplate') for c in w]
    def resolve(node, path):
        cur = node
        for seg in path.split('\\'):
            cur = up(cur) if seg == '..' else next((c for c in kids(cur) if c.get('Id') == seg), None)
            if cur is None:
                return False
        return True
    attrs = ('ClipRect', 'InnerPanel', 'VerticalScrollbar', 'HorizontalScrollbar',
             'WidgetToCopyHeightFrom', 'WidgetToCopyWidthFrom', 'Handle')
    return [e.tag + '.' + a + '=' + e.get(a)
            for e in t.iter() for a in attrs if e.get(a) and not resolve(e, e.get(a))]

def panel_bindings():
    xml = io.open('TradeLord/GUI/Prefabs/TradeLordPanel.xml', encoding='utf-8').read()
    bound = (set(re.findall(r'"@(\w+)"', xml)) | set(re.findall(r'DataSource="\{(\w+)\}"', xml))
             | set(re.findall(r'Command\.\w+="(\w+)"', xml)))
    have = (set(re.findall(r'\[DataSourceProperty\]\s*public\s+[\w<>\.]+\s+(\w+)', S['Panel.cs']))
            | set(re.findall(r'public void (Execute\w+)\(\)', S['Panel.cs'])))
    return bound, have
ALL = "\n".join(S.values())

def strings_declared():
    import xml.etree.ElementTree as ET
    return {e.get('id') for e in
            ET.parse('TradeLord/ModuleData/Languages/module_strings.xml').getroot().iter('string')}

def mcm_orders_unique():
    import collections
    blocks = re.findall(
        r'\[SettingProperty\w+\((.*?)\)\]\s*\[SettingPropertyGroup\("([^"]+)"[^\]]*\)\]\s*public\s+[\w<>]+\s+(\w+)',
        M, re.S)
    groups = collections.defaultdict(list)
    for attrs, group, name in blocks:
        m = re.search(r'Order\s*=\s*(\d+)', attrs)
        groups[group].append(int(m.group(1)) if m else None)
    return all(len(v) == len(set(v)) for v in groups.values()) and len(blocks) > 40

def mcm_defaults_within_range():
    defaults = dict(re.findall(r'public\s+(?:int|float)\s+(\w+)\s*=\s*([0-9.]+)f?;', S['Options.cs']))
    for attrs, name in re.findall(
            r'\[SettingProperty(?:Integer|FloatingInteger)\((.*?)\)\]\s*(?:\[[^\]]*\]\s*)*public\s+\w+\s+(\w+)',
            M, re.S):
        m = re.match(r'\s*"[^"]*"\s*,\s*(-?[0-9.]+)f?\s*,\s*(-?[0-9.]+)f?', attrs)
        if not m or name not in defaults:
            continue
        if not (float(m.group(1)) <= float(defaults[name]) <= float(m.group(2))):
            return False
    return len(defaults) > 10

def every_option_has_a_control():
    declared = set(re.findall(r'public\s+(?:bool|int|float|string)\s+(\w+)\s*=', S['Options.cs']))
    exposed = set(re.findall(r'Options\.Current\.(\w+)', M))
    return not (declared - exposed - {'AutoTradeBoth'})

LITERAL = r'\{=(TL\d+)\}((?:[^"\\]|\\.)*)"'

def unescape(text):
    return text.replace('\\"', '"').replace('\\\\', '\\')

def strings_used():
    return set(re.findall(r'\{=(TL\d+)\}', ALL + "\n" + M))

MANIFEST = io.open('TradeLord/SubModule.xml', encoding='utf-8').read()

def module_version():
    m = re.search(r'<Version value="v([0-9]+\.[0-9]+\.[0-9]+)"', MANIFEST)
    return m.group(1) if m else None

def workflow_reads_the_manifest_version():
    m = re.search(r'<Version value="(v[0-9][^"]*)"', MANIFEST)
    return (m is not None and module_version() is not None
            and m.group(1) == 'v' + module_version()
            and '<Version value="\\(v[0-9][^"]*\\)"' in WORKFLOW)

def harmony_targets():
    return sorted(t + '.' + m for t, m in
                  re.findall(r'\[HarmonyPatch\(typeof\((\w+)\), "(\w+)"\)\]', ALL))

def every_declared_patch_is_installed():
    declared = sorted(re.findall(
        r'\[HarmonyPatch\(typeof\(\w+\), "\w+"\)\]\s*internal static class (\w+)', ALL))
    installed = sorted(re.findall(
        r'Patcher\.TryPatch\(harmony, typeof\((\w+)\)\)', S['SubModule.cs']))
    return len(declared) > 0 and declared == installed and len(harmony_targets()) == len(declared)

def compat_list(name):
    m = re.search(re.escape(name) + r'\s*=\s*\{(.*?)\};', COMPAT, re.S)
    return None if m is None else sorted(
        t.split('.')[-1] + '.' + member for t, member in
        re.findall(r'\(\s*(?:Inventory \+ )?"([\w.]+)"\s*,\s*"(\w+)"\s*\)', m.group(1)))

def compat_checks_every_game_hook():
    reflected = sorted(t.split('.')[-1] + '.' + m for t, m in
                       re.findall(r'typeof\((\w+)\)\.GetMethod\(\s*"(\w+)"', ALL))
    fields = sorted({'_' + n for n in re.findall(r'____(\w+)', ALL)})
    compat_fields = sorted({p.split('.')[-1] for p in (compat_list('ReflectedFields') or [])})
    return (len(reflected) > 0 and len(fields) > 0
            and compat_list('HarmonyTargets') == harmony_targets()
            and compat_list('ReflectedMethods') == reflected
            and compat_fields == fields)

def refusal_reasons_are_named():
    promised = {'Locked', 'CategoryPolicy', 'FoodReserve', 'BelowMargin',
                'MerchantTillEmpty', 'BudgetSpent', 'CarryWeight', 'HerdFull'}
    phrase = method_body(S['Trading.cs'], 'internal static TextObject Phrase')
    return promised <= set(re.findall(r'case Block\.(\w+):', phrase))

def projects_pin_one_reference_assembly():
    used = set(re.findall(r'"Bannerlord\.ReferenceAssemblies" Version="([0-9.]+)"', "\n".join(PROJ)))
    return len(PROJ) == 2 and len(used) == 1

def one_hard_dependency():
    required = re.findall(r'<DependedModuleMetadata id="([^"]+)" order="[^"]+" optional="false"/>',
                          MANIFEST)
    return required == ['Bannerlord.Harmony']

def method_body(src, signature):
    i = src.find(signature)
    if i < 0:
        raise LookupError('no method matching ' + repr(signature)
                          + " - a check names a method that is no longer there")
    j = src.index('{', i)
    depth, k = 0, j
    while True:
        if src[k] == '{': depth += 1
        elif src[k] == '}':
            depth -= 1
            if depth == 0: return src[i:k + 1]
        k += 1

def ordered(text, *needles):
    at = -1
    for needle in needles:
        found = text.find(needle)
        if found < 0 or found <= at:
            return False
        at = found
    return True

def ordered_last(text, *needles):
    at = -1
    for needle in needles:
        found = text.rfind(needle)
        if found < 0 or found <= at:
            return False
        at = found
    return True

def log_prefers_the_user_folder():
    body = method_body(S['Support.cs'], "private static List<string> Candidates")
    docs = body.find('"Mount and Blade II Bannerlord"')
    own = body.find("Assembly.Location")
    cwd = body.rfind("paths.Add(FileName);")
    return (-1 < docs < own < cwd
            and body.count("catch { }") == 2
            and "yield" not in body)

def capture_skipped_after_the_caches_are_dropped():
    body = method_body(S['Ledger.cs'], "public void CaptureSettlement")
    if "ForgetMarketRankings();" not in body or "if (Options.Current.Omniscient) return;" not in body:
        return False
    return (ordered(body, "ForgetMarketRankings();", "if (Options.Current.Omniscient) return;")
            and "Options.Current.Omniscient" in method_body(S['Ledger.cs'],
                    "private List<(Settlement, int)> TopMarkets"))

def hotkey_fallback_is_reported():
    body = method_body(S['Panel.cs'], "internal static InputKey PanelKey")
    if 'Log.Write("panel hotkey' not in body or "if (!named)" not in body:
        return False
    return ("_key = InputKey.T;" in body and "named = true;" in body
            and ordered(body, "if (!named)", 'Log.Write("panel hotkey'))

def prefab_text_is_all_bound():
    xml = io.open('TradeLord/GUI/Prefabs/TradeLordPanel.xml', encoding='utf-8').read()
    return not re.findall(r'Text="[^@"][^"]*"', xml)

def shipped_text_matches_the_fallback():
    import xml.etree.ElementTree as ET
    shipped = {e.get('id'): e.get('text') for e in
               ET.parse('TradeLord/ModuleData/Languages/module_strings.xml').getroot().iter('string')}
    pairs = re.findall(LITERAL, ALL + "\n" + M)
    return len(pairs) > 0 and all(shipped.get(sid) == unescape(text) for sid, text in pairs)

def actions_are_off_the_node20_runtime():
    floors = {'checkout': 5, 'setup-dotnet': 5, 'upload-artifact': 5}
    majors = {n: int(v) for n, v in re.findall(r'uses: actions/([\w-]+)@v(\d+)', WORKFLOW)}
    return (set(majors) == set(floors)
            and all(majors[n] >= f for n, f in floors.items()))

def shipped_text_is_ascii():
    files = ['TradeLord/ModuleData/Languages/module_strings.xml',
             'TradeLord/GUI/Prefabs/TradeLordPanel.xml',
             'TradeLord/GUI/Brushes/TradeLordBrushes.xml',
             'TradeLord/SubModule.xml',
             'mcm/Settings.cs'] + ['src/' + f for f in S]
    return all(ord(c) < 128
               for f in files
               for c in io.open(f, encoding='utf-8').read())

def settings_name_no_other_mod():
    hints = re.findall(r'HintText = "([^"]*)"', M) + re.findall(r'SettingProperty\w+\("([^"]*)"', M)
    foreign = re.compile(r'AutoTrader|BestTradePrice|Trade ?Advisor|Trade Optimizer|QuickTrade', re.I)
    return len(hints) > 40 and not any(foreign.search(h) for h in hints)

def setting_blocks():
    return [b for b in re.split(r'\n\s*(?=\[SettingProperty(?:Bool|Integer|FloatingInteger|Text)\()', M)
            if re.match(r'\s*\[SettingProperty(?:Bool|Integer|FloatingInteger|Text)\(', b)]

def every_setting_has_a_hint():
    blocks = setting_blocks()
    return len(blocks) > 40 and all('HintText' in b for b in blocks)

def every_setting_line_is_translatable():
    lit = r'"((?:[^"\\]|\\.)*)"'
    names = re.findall(r'\[SettingProperty(?:Bool|Integer|FloatingInteger|Text)\(' + lit, M)
    hints = re.findall(r'HintText = ' + lit, M)
    groups = re.findall(r'\[SettingPropertyGroup\(' + lit + r'\)\]', M)
    return (len(names) > 40 and len(names) == len(hints) == len(groups)
            and all(re.match(r'\{=TL\d+\}', text) for text in names + hints + groups))

def settings_declared_in_display_order():
    seen = {}
    for b in setting_blocks():
        order = re.search(r'Order\s*=\s*(\d+)', b)
        group = re.search(r'\[SettingPropertyGroup\("([^"]+)"\)\]', b)
        if not order or not group:
            return False
        seen.setdefault(group.group(1), []).append(int(order.group(1)))
    return bool(seen) and all(v == sorted(v) for v in seen.values())

def indentation_matches_brace_depth():
    off = []
    for name, text in list(S.items()) + [('Settings.cs', M)]:
        depth = 0
        for line in text.split('\n'):
            body = line.strip()
            if body:
                here = depth - (len(body) - len(body.lstrip('}')))
                indent = len(line) - len(line.lstrip(' '))
                if here >= 0 and indent < here * 4:
                    off.append(name)
                    break
            bare = re.sub(r"'(?:[^'\\]|\\.)*'", "''", re.sub(r'"(?:[^"\\]|\\.)*"', '""', line))
            depth += bare.count('{') - bare.count('}')
    return off == []

def working_shell():
    import os, shutil, subprocess
    for cand in (os.environ.get('SHELL'), shutil.which('sh'), shutil.which('bash')):
        if not cand:
            continue
        try:
            probe = subprocess.run([cand, '-c', 'exit 7'], capture_output=True, timeout=30)
        except Exception:
            continue
        if probe.returncode == 7:
            return cand
    return None

def empty_release_notes_are_rejected():
    import subprocess, tempfile
    line = next((l.strip() for l in WORKFLOW.split('\n')
                 if l.strip().startswith('if ') and 'release-notes.md' in l), None)
    if line is None or not line.endswith('then'):
        return False
    cond = line[len('if '):-len('then')].rstrip().rstrip(';')

    shell = working_shell()
    if shell is None:
        return '-s release-notes.md' not in cond and '[:space:]' in cond

    work = tempfile.mkdtemp()
    def blocks(body):
        script = 'printf %s "$1" > release-notes.md\nif ' + cond + '; then exit 0; fi\nexit 1'
        try:
            done = subprocess.run([shell, '-c', script, '_', body], cwd=work,
                                  capture_output=True, timeout=60)
        except Exception:
            return False
        return done.returncode == 0
    return (blocks('') and blocks('\n') and blocks('  \n \t\n')
            and not blocks('- a real release note\n'))

README = io.open('README.md', encoding='utf-8').read()
CHANGES = io.open('CHANGELOG.md', encoding='utf-8').read()

def option_default(name):
    m = re.search(r'public\s+(?:bool|int|float|string)\s+' + name + r'\s*=\s*([^;]+);', S['Options.cs'])
    return None if m is None else m.group(1).strip()

def readme_defaults_match_the_shipped_ones():
    def on(name):
        return option_default(name) == 'true'
    claims = ['hotkey **' + option_default('PanelKey').strip('"') + '**']
    return (all(c in README for c in claims)
            and on('Omniscient') and on('AutoSellOnEntry') and on('AutoBuyOnEntry')
            and on('NeverBuyGrain') and on('TradeWithVillages')
            and on('ProtectSpecial') and on('RespectLocks') and on('ExcludeHostileTowns'))

def every_text_variable_is_supplied():
    placeholders = set()
    for text in re.findall(LITERAL, ALL + "\n" + M):
        placeholders |= set(re.findall(r'\{([A-Z][A-Z0-9_]*)\}', unescape(text[1])))
    supplied = set(re.findall(r'"([A-Z][A-Z0-9_]*)"', ALL + "\n" + M))
    return len(placeholders) > 10 and not (placeholders - supplied)

def changelog_opens_on_the_shipped_version():
    heads = re.findall(r'^## (.+)$', CHANGES, re.M)
    if not heads or heads[0].strip() != module_version():
        return False
    newest = CHANGES.split('## ' + heads[0], 1)[1].split('\n## ', 1)[0]
    return any(line.startswith('- ') for line in newest.split('\n'))

PLAIN_SAVED_TYPES = {'string', 'int', 'bool', 'float', 'Settlement'}

def the_filter_is_armed_only_around_a_game_call_that_talks():
    t = S['Trading.cs']
    armed = re.findall(r'OpenTransaction\(\);\s*try \{ ([\w\.]+)\([^)]*\); \}\s*'
                       r'finally \{ CloseTransaction\(\);( ReportSilenced\(\);)? \}', t)
    return (t.count('OpenTransaction();') == len(armed) == 3
            and sorted(c for c, _ in armed) == ['SellItemsAction.Apply', 'SellItemsAction.Apply',
                                                'SkillLevelingManager.OnTradeProfitMade']
            and 'InGameTransaction = true' not in t
            and 'if (!TradeActionBehavior.InGameTransaction) return true;' in t
            and 'AutomatedTradeInProgress' not in
                method_body(t, "internal static class Patch_SilenceChunkedTradeLines"))

def the_full_cargo_warning_waits_for_a_visit_that_traded_nothing():
    body = method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry")
    return ('if (TradedThisVisit()) return;' in body
            and ordered(body, 'if (TradedThisVisit()) return;', '!NoRoomToCarry()')
            and 'private static bool TradedThisVisit() => _soldThisVisit.Count > 0 || '
                '_boughtThisVisit.Count > 0;' in S['Trading.cs'])

def the_trade_skill_gain_is_reported_in_one_line():
    body = method_body(S['Trading.cs'], "private static void CreditTradeSkill")
    return ('finally { CloseTransaction(); ReportSilenced(); }' in body
            and ordered(body,
                        'int before = Hero.MainHero.GetSkillValue(DefaultSkills.Trade);',
                        'OpenTransaction();',
                        'SkillLevelingManager.OnTradeProfitMade(Hero.MainHero, xp);',
                        'int now = Hero.MainHero.GetSkillValue(DefaultSkills.Trade);',
                        'Toast(earned, ToastXp);')
            and '{=TL88}' in body and '{=TL81}' in body
            and 'earned.SetTextVariable("LEVEL", now);' in body
            and 'SkillLevelingManager' not in
                method_body(S['Trading.cs'], "internal static void FlushToasts"))

def a_zero_cap_never_means_buy_nothing():
    body = method_body(S['Trading.cs'], "public static void ExecuteQuickBuy")
    return ("if (Options.Current.BuyCapPerItem > 0 &&" in body
            and "countThis >= Options.Current.BuyCapPerItem" in body
            and "Options.Current.BuyCapPerItem > 0\n                        ? Options.Current.BuyCapPerItem : UncappedBuyProjection;"
                in S['Ledger.cs']
            and "private const int UncappedBuyProjection" in S['Ledger.cs'])

def every_numeric_setting_that_switches_off_at_zero_says_so():
    off = {'TL202': 'Observation shelf life', 'TL204': 'Scan radius', 'TL206': 'Travel ceiling',
           'TL207': 'Village travel ceiling', 'TL228': 'Sell loot up to tier',
           'TL235': 'Buy cap per item (count', 'TL236': 'Buy cap per item (denars',
           'TL237': 'Max spend per visit', 'TL243': 'Economy settling delay', 'TL246': 'Auto-marker travel ceiling'}
    for marker in off:
        label = re.search(r'\{=' + marker + r'\}([^"]*)"', M)
        if label is None or '0 = ' not in label.group(1):
            return False
    return True

def the_purse_speaks_up_even_on_a_quiet_pass():
    body = method_body(S['Trading.cs'], "public static void ExecuteQuickBuy")
    return ("if (!Muted(quiet) && (!quiet || PurseHeldItBack(tally)))" in body
            and "tally.Any && tally.Dominant() == Block.BudgetSpent;" in S['Trading.cs']
            and "if (!quiet)" in method_body(S['Trading.cs'], "public static void ExecuteQuickSell"))

def the_item_tooltip_does_not_announce_the_mod():
    body = method_body(S['TooltipPatches.cs'], "internal static void Append")
    return ("{=TL07}" not in body
            and "TooltipProperty.TooltipPropertyFlags.Title" not in body
            and "AddSeparator(vm);" in body)

def the_panel_legend_is_legible():
    m = re.search(r'Brush\.FontSize="(\d+)"\s*\n\s*Brush\.FontColor="#(\w{6})(\w{2})"\s*\n\s*Text="@LegendText"',
                  PREFAB)
    return m is not None and int(m.group(1)) >= 14 and int(m.group(3), 16) >= 0xCC

GITIGNORE = io.open('.gitignore', encoding='utf-8').read()

def the_game_assemblies_are_read_from_a_variable_and_never_copied():
    return ('private const string GameBinVariable = "TRADELORD_GAME_BIN";' in COMPAT
            and 'Environment.GetEnvironmentVariable(GameBinVariable)' in COMPAT
            and 'SearchOption.AllDirectories' in COMPAT
            and not re.search(r'File\.(Copy|WriteAllBytes|Move|Delete)\(', COMPAT)
            and COMPAT.count('File.WriteAllText(') == 1
            and 'File.WriteAllText(Path.Combine(work, "fetch.csproj")' in COMPAT
            and 'File.OpenRead(dll)' in method_body(COMPAT, "private static HashSet<string> UserStrings")
            and '*.dll' in GITIGNORE.split())

def a_menu_id_the_mod_does_not_guard_fails_the_run():
    body = method_body(COMPAT, "private static void CheckMenuIds")
    return ('Failures.Add(id + " is in no assembly of this install' in body
            and 'if (guarded)' in body
            and ordered(body, 'if (guarded)', 'Failures.Add(id + " is in no assembly')
            and '("town", false)' in COMPAT and '("village", false)' in COMPAT
            and '("port_menu", true)' in COMPAT
            and '("naval_storyline_virtualport", true)' in COMPAT)

def the_menu_id_check_is_skipped_rather_than_failed_when_unset():
    body = method_body(COMPAT, "private static void CheckMenuIds")
    return ('if (string.IsNullOrWhiteSpace(root))' in body
            and '  skipped  set " + GameBinVariable' in body
            and ordered(body, 'if (string.IsNullOrWhiteSpace(root))', 'if (!Directory.Exists(root))')
            and 'Failures.Add(GameBinVariable + " points at "' in body)

def the_money_rules_need_nothing_from_the_game():
    return ('TaleWorlds' not in S['TradeMath.cs']
            and 'public static class TradeMath' in S['TradeMath.cs']
            and all(m in S['TradeMath.cs'] for m in
                    ('PolicyAllows', 'Credit', 'ProfitAcceptable', 'Realizable', 'BuyAcceptable')))

def the_policy_layer_keeps_no_second_copy_of_the_money_rules():
    body = S['Trading.cs']
    forwards = ('TradeMath.PolicyAllows(policy, buying);',
                'TradeMath.Credit(proceeds, basis, unpaidWorth);',
                'TradeMath.ProfitAcceptable(costBasis, townSellPrice, Options.Current.MinProfitMargin);',
                'TradeMath.Realizable(farSellPrice, Options.Current.ResaleSafetyFactor);',
                'TradeMath.BuyAcceptable(buyPrice, realizable, Options.Current.MinProfitMargin);')
    return (all(f in body for f in forwards)
            and 'gain > 0 ? gain : 0' not in body
            and 'ResaleSafetyFactor;' not in body.replace('Options.Current.ResaleSafetyFactor);', ''))

def the_money_rules_are_covered_by_tests_the_build_runs():
    return ('TradeMath.cs' in TESTPROJ and 'Options.cs' in TESTPROJ
            and MATHTESTS.count('[Fact]') + MATHTESTS.count('[Theory]') >= 12
            and 'TradeMath.Credit' in MATHTESTS and 'TradeMath.BuyAcceptable' in MATHTESTS
            and 'TradeMath.ProfitAcceptable' in MATHTESTS and 'TradeMath.PolicyAllows' in MATHTESTS)

def the_route_rules_need_nothing_from_the_game():
    return ('TaleWorlds' not in S['Confidence.cs']
            and 'public static class Confidence' in S['Confidence.cs']
            and 'Confidence' not in S['Market.cs']
            and 'public static int Budget(' in S['TradeMath.cs']
            and "TradeMath.Budget(Hero.MainHero.Gold, Options.Current.GoldReserve," in S['Trading.cs']
            and 'Options.Current.MaxSpendPerVisit > 0' not in
                method_body(S['Trading.cs'], "public static void ExecuteQuickBuy"))

def the_route_rules_are_covered_by_tests_the_build_runs():
    return ('Confidence.cs' in TESTPROJ
            and ROUTETESTS.count('[Fact]') + ROUTETESTS.count('[Theory]') >= 15
            and 'TradeMath.Budget' in ROUTETESTS and 'Confidence.Of' in ROUTETESTS
            and 'NeverSet' in ROUTETESTS)

def saved_field_types():
    types = {}
    for text in (S['Ledger.cs'], S['Trading.cs']):
        for m in re.finditer(r'private\s+([\w<>,\.\[\]\s]+?)\s+(_\w+)\s*(?:=[^;]*)?;', text):
            types[m.group(2)] = ' '.join(m.group(1).split())
    return types

def every_saved_value_is_a_plain_one():
    types = saved_field_types()
    seen = 0
    for name in ('Ledger.cs', 'Trading.cs'):
        body = method_body(S[name], "public override void SyncData")
        for field in re.findall(r'dataStore\.SyncData\("[^"]+",\s*ref\s+(_\w+)\)', body):
            seen += 1
            if types.get(field) not in PLAIN_SAVED_TYPES:
                return False
    return seen >= 6

def nothing_this_module_defines_is_saveable():
    return all('SaveableTypeDefiner' not in text and 'SaveableField' not in text
               and 'AddClassDefinition' not in text and 'ConstructContainerDefinition' not in text
               for text in S.values())

def no_collection_of_our_own_reaches_a_save():
    body = method_body(S['Ledger.cs'], "public override void SyncData")
    return ('ref _ledger)' not in body and 'ref _purchases)' not in body
            and 'dataStore.SyncData("TradeLord_LedgerText", ref _ledgerText);' in body
            and 'dataStore.SyncData("TradeLord_PurchaseText", ref _purchaseText);' in body)

def the_codec_needs_nothing_from_the_game():
    return ('TaleWorlds' not in S['LedgerCodec.cs']
            and 'public static class LedgerCodec' in S['LedgerCodec.cs'])

def saved_numbers_read_the_same_in_every_language():
    codec = S['LedgerCodec.cs']
    numeric = [c for c in re.findall(
        r'\w+\.ToString\([^)]*\)|\b(?:int|float|double|long)\.(?:Try)?Parse\([^;]*', codec)
        if not c.startswith('sb.ToString')]
    return (len(numeric) == 4
            and all('CultureInfo.InvariantCulture' in c for c in numeric)
            and 'NumberStyles.Integer, CultureInfo.InvariantCulture' in codec
            and 'NumberStyles.Float, CultureInfo.InvariantCulture' in codec
            and not re.search(r'\.Append\(\w+\.(?:BuyPrice|SellPrice|CapturedDay|TotalPaid|Count|LastUnitPaid)\)',
                              codec))

def a_name_that_looks_like_a_separator_is_left_out():
    codec = S['LedgerCodec.cs']
    guard = ("!string.IsNullOrEmpty(id) && id.IndexOf(FieldMark) < 0 && id.IndexOf(RecordMark) < 0"
             in codec)
    ledger = method_body(codec, "public static string WriteLedger")
    purchases = method_body(codec, "public static string WritePurchases")
    return (guard
            and 'Storable(kv.Key)' in ledger and 'Storable(o.TownId)' in ledger
            and 'Storable(o.CapturedDay)' in ledger
            and 'Storable(rec.ItemId)' in purchases)

def a_record_that_cannot_be_read_is_dropped_on_its_own():
    ledger = method_body(S['LedgerCodec.cs'], "public static Dictionary<string, List<PriceObservation>> ReadLedger")
    purchases = method_body(S['LedgerCodec.cs'], "public static List<PurchaseRecord> ReadPurchases")
    return (ledger.count('continue;') == 4 and purchases.count('continue;') == 3
            and 'return book;' in ledger and 'return kept;' in purchases
            and 'throw' not in ledger and 'throw' not in purchases)

def the_codec_is_covered_by_tests_the_build_runs():
    return ('dotnet test tests/TradeLord.Tests.csproj' in WORKFLOW
            and 'LedgerCodec.cs' in TESTPROJ
            and TESTS.count('[Fact]') + TESTS.count('[Theory]') >= 12
            and 'InvariantCulture' not in TESTS)

results = []
def chk(ver, claim, ok):
    results.append(ok)
    print(('  ok      ' if ok else '  BROKEN  ') + f"[{ver}] {claim}")

chk("1.3.2", "smithing compares live DefaultItems, no cached static set",
    "item == DefaultItems.Charcoal" in S['Trading.cs'] and not re.search(r'static.*HashSet<ItemObject>', ALL))
chk("1.3.2", "one food reserve in total, not per type", S['Trading.cs'].count("KeepFoodDays") == 2)
chk("1.3.2", "ExcludeHostileTowns blocks trading, not just scans",
    "CanTradeHere" in S['Trading.cs'] and "ExcludeHostileTowns && LedgerBehavior.IsHostile" in S['Trading.cs'])
chk("1.3.2", "ScanRadius applied in observed mode",
    "if (!WithinRadius(s)) return false;" in method_body(S['Ledger.cs'], "private static bool Eligible") and
    "!Eligible(town, out float lower)" in S['Ledger.cs'])
chk("1.3.2", "AutoTradeBoth includes EnableBuying",
    "AutoSellOnEntry && AutoBuyOnEntry && EnableBuying" in S['Options.cs'])
chk("1.3.2", "zero-gold purchase not recorded",
    re.search(r'if \(cost == 0\) break;[\s\S]{0,80}RecordPurchase', S['Trading.cs']) is not None)
chk("1.3.2", "panel tracks a set of pins", "_panelPins = new HashSet<Settlement>" in S['Panel.cs'])
chk("1.3.2", "marker never removes a panel pin", "LedgerPanel.IsPinned(_trackedTown)" in S['Trading.cs'])
chk("1.3.2", "sim honors the same-stop rule",
    "_soldThisVisit.Add" in S['Trading.cs'] and "_boughtThisVisit[item.StringId] = (countThis, spentThis);" in S['Trading.cs'])
chk("1.3.2", "sim honors carry weight", "simWeight" in S['Trading.cs'])
chk("1.3.2", "observed mode uses Settlement.Find", "Settlement.Find(o.TownId)" in S['Ledger.cs'])
chk("1.3.2", "Instance cleared on game end", "LedgerBehavior.Instance = null" in S['SubModule.cs'])
chk("1.3.2", "panel rebuilt when map screen replaced", "map != _mapScreen" in S['Panel.cs'])
chk("1.3.2", "item lists case-insensitive", "StringComparer.OrdinalIgnoreCase" in S['Options.cs'])
chk("1.3.4", "per-item buy caps persist across clicks",
    "_boughtThisVisit.TryGetValue(item.StringId, out var prior)" in S['Trading.cs'])
chk("1.3.4", "village last-unit clamp leaves one unit on the shelf",
    "if (settlement.IsVillage && remaining <= 1) { tally.Note(Block.VillageLastUnit); break; }" in S['Trading.cs'])
chk("1.3.5", "ledger ignores loot and automated passes",
    "if (!isTrading || TradeActionBehavior.AutomatedTradeInProgress) return;" in S['Ledger.cs'])
chk("1.3.5", "visit counters reset on entry", "ResetVisit();" in S['Trading.cs'])
chk("1.3.5", "capture at most once per hour per town",
    "hour == _capturedHour && settlement.StringId == _capturedTown" in S['Ledger.cs'])
chk("1.3.5", "sale that moved no gold does not count", "if (proceeds == 0) break;" in S['Trading.cs'])
chk("1.3.5", "detailed-summary setting does not gate the log",
    "DetailedTradeSummary" not in method_body(S['Trading.cs'], "private static void LogDetail"))
chk("1.3.5", "log truncated per launch, at the first path that accepts the write",
    'File.WriteAllText(candidate, "");' in method_body(S['Support.cs'], "private static string Resolve") and
    "if (!_resolved) { _resolved = true; _path = Resolve(); }" in S['Support.cs'])
chk("1.3.5", "hotkey blocked while escape menu open", "!map.IsEscapeMenuOpened" in S['Panel.cs'])
chk("1.3.5", "travel caches cleared on game end",
    'Guard.Run("GameEnd.Travel", Travel.Forget)' in S['SubModule.cs'])
chk("1.3.6", "the smithing-material rule still binds buying as well as selling",
    "PolicyAllows(PolicyFor(item), buying: true)" in method_body(S['Trading.cs'], "internal static bool MayBuy") and
    "PolicyAllows(PolicyFor(item), buying: false)" in method_body(S['Trading.cs'], "internal static bool MaySell") and
    "if (IsSmithingMaterial(item)) return Options.Current.CraftingPolicy;" in S['Trading.cs'])
chk("1.3.6", "vanilla suppression asks the ledger", "TooltipHelper.HasSection(____targetItem)" in S['TooltipPatches.cs'])
chk("1.3.6", "marker respects the sell policy",
    "TradePolicy.MaySell(el, locked, foodKeep" in method_body(S['Trading.cs'], "private Settlement FindBestSellTownForCargo"))
chk("1.3.6", "chunked trade lines silenced",
    "AutomatedTradeInProgress" in S['Trading.cs'] and "Patch_SilenceChunkedTradeLines" in S['Trading.cs'])
chk("1.3.6", "smithing materials still ship tradable, as the old switch shipped off",
    "CraftingPolicy = PolicyBuySell" in S['Options.cs'])
chk("1.3.8", "food reserve covers livestock",
    "IsTradableLivestock(item) ? item.HorseComponent.MeatCount : 0" in S['Trading.cs'] and
    ordered(S['Trading.cs'],
            "bool livestock = item.HasHorseComponent",
            "if (foodKeep != null && foodKeep.TryGetValue"))
chk("1.3.8", "quick-buy respects inventory locks",
    "IsLocked(lockedKeys, new EquipmentElement(item))" in S['Trading.cs'])
chk("1.3.8", "Harmony field injection uses four underscores", "____targetItem" in S['TooltipPatches.cs'])
chk("1.3.8", "quick-buy stops when the budget is spent", "if (directionError || Budget() <= 0) break;" in S['Trading.cs'])
chk("1.3.9", "per-hour cache serves both price modes",
    ordered(S['Ledger.cs'], "_marketCache.TryGetValue", "? TopLive"))
chk("1.3.9", "entering a market drops cached rankings", "ForgetMarketRankings();" in S['Ledger.cs'])
chk("1.3.9", "panel respects locks", "ISet<string> locked = TradePolicy.LockedKeys();" in S['Ledger.cs'])
chk("1.3.9", "summary names the six biggest by gold",
    "byValue.Sort((x, y) => y.Value.gold.CompareTo(x.Value.gold));" in S['Trading.cs'])
chk("1.3.9", "null item lists tolerated", '(src ?? "")' in S['Options.cs'])
chk("1.3.10", "automation setters gated on Loaded (what makes an MCM load order-independent)",
    "if (value && Loaded) Options.Current.EnableBuying = true;" in M and
    "if (!Loaded || value == Options.Current.AutoTradeBoth) return;" in M)
chk("1.3.10", "hotkey rejects non-key text", "Enum.IsDefined(typeof(InputKey), k)" in S['Panel.cs'])
chk("1.3.11", "quest items never sold", "el.EquipmentElement.IsQuestItem" in S['Trading.cs'])
chk("1.3.11", "NotMerchandise never sold",
    "item.NotMerchandise" in method_body(S['Trading.cs'], "internal static bool MaySell"))
chk("1.3.11", "panel drops input restrictions on teardown",
    "SetInputRestrictions(false, InputUsageMask.All)" in method_body(S['Panel.cs'], "internal static void Cleanup"))
chk("1.3.12", "sieges/raids excluded from scans",
    "if (UnderAttack(s) || VillageShut(s)) return false;" in method_body(S['Ledger.cs'], "private static bool Eligible") and
    "LedgerBehavior.UnderAttack(s)" in S['Trading.cs'])
chk("1.3.12", "NotMerchandise on the buy side",
    "item.NotMerchandise" in method_body(S['Trading.cs'], "internal static bool MayBuy"))
chk("1.3.13", "buy shelf ordered by margin", "stock.Sort((x, y) => y.margin.CompareTo(x.margin));" in S['Trading.cs'])
chk("1.3.13", "cost basis read once per stack", "ProfitAcceptable(int costBasis, int townSellPrice)" in S['Trading.cs'])
chk("1.3.13", "MCM automation setters idempotent",
    "value == Options.Current.AutoTradeBoth) return;" in M and
    "if (value == Options.Current.AutoBuyOnEntry) return;" in M)
chk("1.3.13", "caravan pressure is one pass",
    "internal static Dictionary<Settlement, int> CaravanPressure()" in S['Ledger.cs'])
chk("1.3.13", "main-party check ahead of the guard",
    re.search(r'if \(party != MobileParty\.MainParty\) return;', S['Trading.cs']) is not None)
chk("1.3.14", "sim honors the merchant till", "simTill" in S['Trading.cs'])
chk("1.3.14", "one predicate for ledger-priced items",
    S['Trading.cs'].count("bool Priced(") == 1 and "TradePolicy.Priced" in S['TooltipPatches.cs'] and
    "TradePolicy.Priced" in S['Ledger.cs'])
chk("1.3.14", "livestock routes listed", "HerdRoomForLivestock(MobileParty.MainParty)" in S['Ledger.cs'])
chk("1.3.15", "food reserve filled cheapest-first",
    "CostPerFood(x).CompareTo(CostPerFood(y))" in
    method_body(S['Trading.cs'], "internal static Dictionary<ItemObject, int> FoodKeep"))
chk("1.3.15", "recurring errors reported once", "is recurring - not reporting it again" in S['Support.cs'])
chk("1.3.16", "hold-for-best-market re-tested per chunk",
    "if (price < holdFloor) { tally.Note(Block.BelowBestMarket); break; }" in S['Trading.cs'])
chk("1.3.16", "food branch falls through to the sell rules",
    "if (el.Amount <= keepCount) { why = Block.FoodReserve; return false; }" in S['Trading.cs'])
chk("1.3.17", "scan radius reaches the marker", "LedgerBehavior.WithinRadius(s)" in S['Trading.cs'])
chk("1.3.17", "haircut always filters routes",
    "float realizable = TradePolicy.Realizable(sellPrice);" in S['Ledger.cs'] and
    "!TradePolicy.BuyAcceptable(buyPrice, realizable)) break;" in S['Ledger.cs'])
chk("1.3.18", "denar cap reaches route quantities", "Options.Current.BuyValueCapPerItem / buyPrice" in S['Ledger.cs'])
chk("1.3.19", "marker re-evaluated on settlement exit", "OnSettlementLeftEvent.AddNonSerializedListener" in S['Trading.cs'])
chk("1.3.22", "looted and raided villages refused and unscanned",
    "VillageShut" in method_body(S['Trading.cs'], "private static bool CanTradeHere") and
    "VillageShut(s)" in method_body(S['Ledger.cs'], "private static bool Eligible"))
chk("1.3.22", "panel takes mouse only",
    "SetInputRestrictions(true, InputUsageMask.Mouse)" in method_body(S['Panel.cs'], "private static void Show") and
    "IsFocusLayer = true" not in method_body(S['Panel.cs'], "private static void Show"))

chk("1.3.23", "food value mirrors ItemRoster.TotalFood (livestock by MeatCount)",
    "return item.IsFood ? 1 : 0;" in S['Trading.cs'] and
    "int take = Math.Min(el.Amount, (reserve + perUnit - 1) / perUnit);" in S['Trading.cs'])
chk("1.3.23", "herd surplus counts mounts against unmounted men",
    "Math.Max(0, mounts - foot)" in S['Trading.cs'] and "NumberOfMenWithoutHorse" in S['Trading.cs'])
chk("1.3.23", "herd guard includes attached parties", "party.AttachedParties" in S['Trading.cs'])
chk("1.3.23", "the game's own trade permission gates trading",
    "SettlementAction.Trade, out _, out _" in S['Trading.cs'] and
    "GameAllowsTrade(s)" in method_body(S['Trading.cs'], "private static bool CanTradeHere"))
chk("1.3.23", "the access model is only asked about the settlement in context",
    "if (s != Settlement.CurrentSettlement) return true;" in S['Trading.cs'])

chk("1.3.24", "livestock reserved only after ordinary food",
    "return lx != ly ? lx.CompareTo(ly) : CostPerFood(x).CompareTo(CostPerFood(y));" in
    method_body(S['Trading.cs'], "internal static Dictionary<ItemObject, int> FoodKeep"))

chk("1.3.25", "routes pair every top buy market against every top sell market",
    "foreach (var (to, sellPrice) in sells)" in method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes"))
chk("1.3.25", "the per-visit spend cap reaches route quantities",
    "Options.Current.MaxSpendPerVisit / buyPrice" in S['Ledger.cs'])
chk("1.3.25", "the herd probe runs only once livestock is actually on the shelf",
    "int herdRoom = -1;" in method_body(S['Trading.cs'], "public static void ExecuteQuickBuy") and
    "if (herdRoom < 0) herdRoom = HerdRoomForLivestock(MobileParty.MainParty);" in
    method_body(S['Trading.cs'], "public static void ExecuteQuickBuy"))

chk("1.3.26", "pathfinder calls are gated behind a straight-line lower bound",
    "float soonest = toBuy + Travel.StraightDaysBetween(from, to);" in S['Ledger.cs'] and
    ordered(S['Ledger.cs'], "float soonest = toBuy", "float days = toBuy + Travel.EstimateDaysBetween"))
chk("1.3.26", "the best route so far prunes pairs before they cost a path query",
    "if (best != null && ceiling / Math.Max(soonest, 0.25f) <= bestKey) continue;" in S['Ledger.cs'] and
    ordered(S['Ledger.cs'], "ceiling / Math.Max(soonest, 0.25f)", "Travel.EstimateDaysBetween(from, to)"))
chk("1.3.26", "both straight-line estimates share one implementation",
    S['Travel.cs'].count("private static float StraightDays(float distance)") == 1 and
    "return StraightDays(party.GetPosition2D.Distance(target.GetPosition2D));" in S['Travel.cs'] and
    "StraightDays(a.GetPosition2D.Distance(b.GetPosition2D))" in S['Travel.cs'])

chk("1.3.27", "tooltip postfix declares only the arguments it uses",
    "private static void Postfix(ItemMenuVM __instance, ItemVM item)" in S['TooltipPatches.cs'])
chk("1.3.27", "both shipped assemblies compile with warnings as errors",
    all("<TreatWarningsAsErrors>true</TreatWarningsAsErrors>" in p for p in PROJ))

chk("1.3.28", "both travel estimates take their speeds from one definition",
    S['Travel.cs'].count("land = MobileParty.MainParty.Speed;") == 1 and
    S['Travel.cs'].count("if (land <= 0.01f) land = 5f;") == 1 and
    "Speeds(out float land, out float sea);" in method_body(S['Travel.cs'], "internal static float Days") and
    "Speeds(out float land, out float sea);" in method_body(S['Travel.cs'], "private static float StraightDays"))

chk("1.3.29", "one buy-side margin rule, for the planner and the executor alike",
    S['Trading.cs'].count("internal static bool BuyAcceptable(int buyPrice, float realizable)") == 1 and
    S['Trading.cs'].count("Options.Current.ResaleSafetyFactor") == 1 and
    S['Ledger.cs'].count("Options.Current.MinProfitMargin") == 0 and
    S['Ledger.cs'].count("Options.Current.ResaleSafetyFactor") == 0 and
    "TradePolicy.BuyAcceptable" in S['Ledger.cs'] and "TradePolicy.BuyAcceptable" in S['Trading.cs'])
chk("1.3.29", "both knowledge modes filter markets through one eligibility rule",
    S['Ledger.cs'].count("private static bool Eligible(Settlement s, out float lower)") == 1 and
    "!Eligible(s, out float lower)" in method_body(S['Ledger.cs'], "private List<(Settlement s, float days)> LiveCandidates") and
    "!Eligible(town, out float lower)" in method_body(S['Ledger.cs'], "private List<(Settlement, int)> TopObserved"))
chk("1.3.29", "one definition of what counts as a market, for scans and for trading",
    S['Trading.cs'].count("internal static bool IsMarket(Settlement s)") == 1 and
    "if (!TradeActionBehavior.IsMarket(s)) return false;" in method_body(S['Ledger.cs'], "private static bool Eligible"))
chk("1.3.29", "the ledger panel's header columns match its row template",
    panel_columns()[0] is not None and panel_columns()[0] == panel_columns()[1])

chk("1.3.30", "the purchase index is rebuilt in one place",
    S['Ledger.cs'].count("private void Reindex()") == 1 and
    "ToDictionary" not in S['Ledger.cs'] and
    "Reindex();" in method_body(S['Ledger.cs'], "public override void SyncData"))
chk("1.3.30", "a damaged purchase record does not throw during save load",
    "if (rec?.ItemId != null) _paid[rec.ItemId] = rec;" in
    method_body(S['Ledger.cs'], "private void Reindex"))

chk("1.3.31", "the price gate and the transaction it guards share one granularity",
    S['Trading.cs'].count("SellItemsAction.Apply(me, shop, el, 1, settlement)") == 1 and
    S['Trading.cs'].count("SellItemsAction.Apply(shop, me, el, 1, settlement)") == 1 and
    S['Trading.cs'].count("SellItemsAction.Apply(") == 2)

chk("1.3.32", "a dry run reports itself as a best case, in the toast, the log and the hint",
    S['Trading.cs'].count("[Simulated, best case]") == 2 and
    S['Trading.cs'].count("(simulated, best case): ") == 2 and
    "best case" in M)

chk("1.3.33", "a fully sold stack clears its cost basis",
    "if (rec.Count <= 0) { rec.Count = 0; rec.TotalPaid = 0; }" in
    method_body(S['Ledger.cs'], "public void RecordSale"))
chk("1.3.33", "automated trading recaptures prices after it moves them",
    S['Trading.cs'].count("LedgerBehavior.Instance?.CaptureSettlement(settlement, force: true);") == 2 and
    "internal void ForgetMarketRankings()" in S['Ledger.cs'] and
    "ForgetMarketRankings();" in method_body(S['Ledger.cs'], "public void CaptureSettlement"))
_setters = [b for b in re.findall(r'\bset\b\s*(\{[^{}]*(?:\{[^{}]*\}[^{}]*)*\})', M)
            if "Options.Bump();" in b]
chk("1.3.33", "every settings write bumps the generation every cache keys on",
    len(_setters) == len(re.findall(r'\[SettingProperty(?:Bool|Integer|FloatingInteger|Text)\(', M)))

chk("1.3.34", "one naval gate for both map-distance calls",
    S['Travel.cs'].count("HasPort && naval") == 2 and
    "target.HasPort && NavalActive" in S['Travel.cs'])
chk("1.3.34", "both travel caches hold geometry and re-time it at the current speed",
    "return Days(hit.dist, hit.landRatio);" in
    method_body(S['Travel.cs'], "internal static float EstimateDaysFromParty") and
    "return Days(hit.dist, hit.landRatio);" in
    method_body(S['Travel.cs'], "internal static float EstimateDaysBetween"))
chk("1.3.34", "one naval invalidation rule, for both travel caches",
    S['Travel.cs'].count("DropIfNavalChanged();") == 2 and
    "DropIfNavalChanged();" in method_body(S['Travel.cs'], "internal static float EstimateDaysFromParty") and
    "DropIfNavalChanged();" in method_body(S['Travel.cs'], "internal static float EstimateDaysBetween") and
    "_partyDist.Clear();" in method_body(S['Travel.cs'], "private static void DropIfNavalChanged") and
    "_pairDist.Clear();" in method_body(S['Travel.cs'], "private static void DropIfNavalChanged"))
chk("1.3.34", "manual trading recaptures prices after it moves them",
    "CaptureSettlement(Settlement.CurrentSettlement, force: true);" in
    method_body(S['Ledger.cs'], "private void OnPlayerInventoryExchange"))
_bound, _have = panel_bindings()
chk("1.3.34", "every panel binding resolves to a view-model member, and none is orphaned",
    len(_bound) > 0 and _bound == _have)

chk("1.3.35", "the panel's Refresh button drops the rankings before rebuilding",
    "LedgerBehavior.Instance?.ForgetMarketRankings();" in
    method_body(S['Panel.cs'], "public void ExecuteRefresh") and
    "ForgetMarketRankings" not in method_body(S['Panel.cs'], "public void Show"))

chk("1.4.0", "every widget path in the panel prefab resolves to a real Id",
    panel_paths() == [])
chk("1.4.0", "trade toasts are queued and flushed a frame later, after the game's own",
    "_pending.Add(new InformationMessage(msg.ToString(), color))" in S['Trading.cs'] and
    'Guard.Run("Tick.FlushToasts", TradeActionBehavior.FlushToasts)' in
    method_body(S['SubModule.cs'], "protected override void OnApplicationTick"))
chk("1.4.0", "realized profit is banked for the campaign and shown in the panel",
    'dataStore.SyncData("TradeLord_LifetimeProfit", ref _lifetimeProfit);' in S['Ledger.cs'] and
    "LedgerBehavior.Instance?.AddProfit(profit);" in S['Trading.cs'] and
    "LedgerBehavior.Instance?.LifetimeProfit" in S['Panel.cs'])
chk("1.4.0", "the hotkey honors its modifiers on both the open and the close edge",
    S['Panel.cs'].count("HotkeyReleased()") == 3 and
    "Input.IsKeyReleased(key)" not in S['Panel.cs'])

chk("1.4.1", "quick-buy prices the shelf only when there is a budget to spend",
    "if (Budget() > 0)" in method_body(S['Trading.cs'], "public static void ExecuteQuickBuy") and
    ordered(S['Trading.cs'], "int Budget()", "ItemRoster shopRoster = settlement.ItemRoster;"))
chk("1.4.1", "a pass the gold-direction guard stopped does not blame the trade policy",
    S['Trading.cs'].count("else if (!directionError)") == 2 and
    S['Trading.cs'].count("else if (!quiet && !directionError)") == 0 and
    'TextObject("{=TL32}Nothing sold here - {REASON}.")' in S['Trading.cs'] and
    'TextObject("{=TL33}Nothing bought here - {REASON}.")' in S['Trading.cs'])
chk("1.4.1", "the automatic path asks the same market question the menu does",
    "IsMarket" not in method_body(S['Trading.cs'], "private void OnSettlementEntered"))
chk("1.4.1", "planner and executor apply the same village last-unit clamp",
    "StockOf(from, item) - (from.IsVillage ? 1 : 0)" in S['Ledger.cs'] and
    "if (settlement.IsVillage && remaining <= 1) { tally.Note(Block.VillageLastUnit); break; }" in S['Trading.cs'])

chk("1.4.2", "a village the game will not trade in is not a destination either",
    "v.VillageState != Village.VillageStates.Normal" in
    method_body(S['Ledger.cs'], "internal static bool VillageShut"))
chk("1.4.2", "straight-line distance fallback returns a straight-line land ratio",
    "catch { landRatio = 1f; }" in method_body(S['Travel.cs'], "internal static float FromParty") and
    "catch { landRatio = 1f; }" in method_body(S['Travel.cs'], "internal static float Between"))
chk("1.4.2", "one flush does not post the same line twice",
    "_pending[i].Information != _pending[i - 1].Information" in
    method_body(S['Trading.cs'], "internal static void FlushToasts"))

chk("1.4.3", "an hourly capture can be forced, and only a forced one skips the dedupe",
    "public void CaptureSettlement(Settlement settlement, bool force = false)" in S['Ledger.cs'] and
    "if (!force && hour == _capturedHour" in S['Ledger.cs'])
chk("1.4.3", "one rule for what the ledger will capture, and it tolerates no settlement at all",
    "if (settlement == null || (!settlement.IsTown && !settlement.IsVillage)) return;" in
    method_body(S['Ledger.cs'], "public void CaptureSettlement") and
    "IsVillage" not in method_body(S['Ledger.cs'], "private void OnSettlementEntered"))
chk("1.4.3", "one definition of the livestock the mod trades",
    S['Trading.cs'].count("item.HorseComponent.IsLiveStock") == 1)
chk("1.4.3", "cost basis uses recorded purchase prices, not current market quotes",
    "Options.Current.CostBasisMode == 2 ||" in
    method_body(S['Trading.cs'], "private static bool HasCostBasis") and
    "HasPurchaseRecord(item) ?? false)" in
    method_body(S['Trading.cs'], "private static bool HasCostBasis") and
    "item.IsTradeGood ||" not in method_body(S['Trading.cs'], "private static bool HasCostBasis"))
chk("1.4.3", "every unit with no cost basis is guarded by the sell-side floor instead - loot, and the unpaid half of a mixed stack",
    "if (Options.Current.PreferBestSellTown || basis == 0)" in
        method_body(S['Trading.cs'], "public static void ExecuteQuickSell") and
    "holdFloor = unpaidFloor;" in S['Trading.cs'] and
    "TradePolicy.Priced(item)" not in method_body(S['Trading.cs'], "public static void ExecuteQuickSell"))
chk("1.5.5", "a stack pays its purchased basis only for the units that were purchased, and only those units drain the record",
    "int paidLeft = LedgerBehavior.Instance?.PurchasedUnits(item) ?? 0;" in S['Trading.cs'] and
    "int basis = basisIsMarket || paidLeft > 0 ? paid : 0;" in S['Trading.cs'] and
    "if (paidLeft > 0) { paidLeft--; LedgerBehavior.Instance?.RecordSale(item.StringId, 1); }" in S['Trading.cs'] and
    method_body(S['Trading.cs'], "public static void ExecuteQuickSell").count("RecordSale") == 1)
chk("1.4.3", "the panel is rebuilt for a new map screen, not for every visit to another one",
    "if (_mapScreen != null && map != null && map != _mapScreen)" in S['Panel.cs'])

chk("1.5.0", "the walk gates every unit with the executor's own margin rule",
    "if (!TradePolicy.BuyAcceptable(buyPrice, TradePolicy.Realizable(sellPrice))) break;" in
    method_body(S['Market.cs'], "internal static RouteQuote Walk"))
chk("1.5.0", "one definition of the resale haircut, walk and planner alike",
    S['Market.cs'].count("Options.Current.ResaleSafetyFactor") == 0 and
    S['Ledger.cs'].count("Options.Current.ResaleSafetyFactor") == 0 and
    S['Trading.cs'].count("Options.Current.ResaleSafetyFactor") == 1 and
    "(int)TradePolicy.Realizable(q.SellTotal)" in S['Ledger.cs'])
chk("1.5.0", "observed mode does not read live market supply/demand for projections",
    "if (projecting && (!Options.Current.Omniscient || !Options.Current.BulkSimulation)) return;" in
    method_body(S['Market.cs'],
                "internal Shelf(Settlement site, ItemObject item, bool selling, int quoted, bool projecting)"))
chk("1.5.0", "only a town shelf can be advanced, because only a town publishes the inputs",
    "Town town = site != null && site.IsTown ? site.Town : null;" in S['Market.cs'] and
    "if (town == null" in S['Market.cs'] and
    "GetCategoryData" in S['Market.cs'])
chk("1.5.0", "an unwalkable shelf falls back to the quoted price and reports Simulated=false",
    "q.Simulated = buy.Walkable && sell.Walkable;" in S['Market.cs'] and
    "if (!_walkable) return;" in method_body(S['Market.cs'], "internal void Restock(int units)"))
chk("1.5.0", "buying strips the shelf and selling stocks it",
    "buy.Restock(-1);" in S['Market.cs'] and "sell.Restock(1);" in S['Market.cs'])
chk("1.5.0", "route pruning uses the flat-quote upper bound, so it cannot discard a viable route",
    "float ceiling = (float)(sellPrice - buyPrice) * qtyCap;" in S['Ledger.cs'] and
    ordered(S['Ledger.cs'], "float ceiling =", "Bulk.Walk(from, to, item"))
chk("1.5.0", "a broke selling town is no destination, in the mode that can see its till",
    "if (till <= 0) continue;" in S['Ledger.cs'])
chk("1.5.0", "the panel is ordered by the column it shows",
    "rankByScore ? perDay * confidence : perDay" in S['Ledger.cs'] and
    "Options.Current.ConfidenceRanking ? _route.Score : _route.ProfitPerDay" in S['Panel.cs'] and
    "y.Score.CompareTo(x.Score)" in S['Ledger.cs'])
chk("1.5.0", "caravan pressure is counted once, by the planner that scores on it",
    S['Panel.cs'].count("CaravanPressure()") == 0 and
    S['Ledger.cs'].count("var pressure = CaravanPressure();") == 1)
chk("1.5.0", "every confidence factor is a fraction of one",
    "return c < 0.01f ? 0.01f : (c > 1f ? 1f : c);" in
    method_body(S['Confidence.cs'], "public static float Of(bool simulated"))

chk("1.5.0", "one place decides which category policy governs an item",
    S['Trading.cs'].count("internal static int PolicyFor(ItemObject item)") == 1 and
    S['Trading.cs'].count("internal static bool PolicyAllows(int policy, bool buying)") == 1 and
    S['Trading.cs'].count("Options.Current.FoodPolicy") == 1 and
    S['Trading.cs'].count("Options.Current.CraftingPolicy") == 1 and
    S['Trading.cs'].count("Options.Current.LivestockPolicy") == 1)
chk("1.5.0", "a head of cattle is asked as livestock, not as food",
    ordered(method_body(S['Trading.cs'], "internal static int PolicyFor"), "LivestockPolicy", "FoodPolicy"))
chk("1.5.0", "the mounts and pack-animal fence outlived the matrix",
    "if (!IsTradableLivestock(item)) { why = Block.MountOrPackAnimal; return false; }" in
    method_body(S['Trading.cs'], "internal static bool MaySell") and
    "if (IsTradableLivestock(item)) return true;" in
    method_body(S['Trading.cs'], "internal static bool MayBuy") and
    "why = Block.MountOrPackAnimal;" in method_body(S['Trading.cs'], "internal static bool MayBuy"))
chk("1.5.0", "every category ships trading exactly as it did before the matrix",
    "FoodPolicy = PolicyBuySell" in S['Options.cs'] and
    "CraftingPolicy = PolicyBuySell" in S['Options.cs'] and
    "LivestockPolicy = PolicyBuySell" in S['Options.cs'])
chk("1.5.0", "the food reserve is not a trading policy and is not governed by one",
    "FoodPolicy" not in method_body(S['Trading.cs'], "internal static Dictionary<ItemObject, int> FoodKeep"))

chk("1.5.0", "a pass that moves nothing names the rule that stopped it",
    'TextObject("{=TL32}Nothing sold here - {REASON}.")' in S['Trading.cs'] and
    'TextObject("{=TL33}Nothing bought here - {REASON}.")' in S['Trading.cs'] and
    S['Trading.cs'].count("BlockTally.Phrase(tally.Dominant())") == 2)
chk("1.5.0", "both gates report a reason whenever they refuse",
    method_body(S['Trading.cs'], "internal static bool MaySell").count("why = Block.") >= 6 and
    method_body(S['Trading.cs'], "internal static bool MayBuy").count("why = Block.") >= 5)
chk("1.5.0", "the reason overloads carry the plain ones, so one rule set decides both",
    "MaySell(el, lockedKeys, foodKeep, out keepCount, out _);" in S['Trading.cs'] and
    "MayBuy(item, lockedKeys, out _);" in S['Trading.cs'])
chk("1.5.0", "every stop in the sell pass is counted",
    method_body(S['Trading.cs'], "public static void ExecuteQuickSell").count("tally.Note(") >= 5)
chk("1.5.0", "every stop in the buy pass is counted",
    method_body(S['Trading.cs'], "public static void ExecuteQuickBuy").count("tally.Note(") >= 10)
chk("1.5.0", "localization ids used in code and declared in the language file match exactly",
    strings_declared() == strings_used())

chk("1.5.0", "a menu the game does not have costs the other menus nothing",
    (lambda b: "void AddOptions(string menu) => Guard.Run(" in b
           and '"menu " + menu + " (the other menus are unaffected)"' in b
           and 'AddOptions("town");' in b and 'AddOptions("village");' in b
           and 'foreach (string port in new[] { "port_menu", "naval_storyline_virtualport" })' in b
           and "try { AddOptions(port); }" not in b)
    (method_body(S['Trading.cs'], "private void OnSessionLaunched")))

chk("1.5.1", "the walk asks no market for a price, so observed mode stays observed",
    "GetItemPrice" not in S['Market.cs'] and
    "if (!_walkable) return _quoted;" in method_body(S['Market.cs'], "internal int Price()") and
    "Bulk.Walk(from, to, item, qtyCap, till, buyPrice, sellPrice)" in S['Ledger.cs'])
chk("1.5.1", "an unwalkable shelf reads its quote once",
    method_body(S['Market.cs'], "internal int Price()").count("_quoted") == 2)
chk("1.5.1", "confidence measures the walk, not two price APIs disagreeing",
    "int flatSell = q.OpeningSellPrice * q.Units;" in S['Ledger.cs'] and
    "- q.OpeningBuyPrice * q.Units;" in S['Ledger.cs'])
chk("1.5.1", "the no-trade message reports a blocking rule, not a structural exclusion",
    "private static bool Structural(Block reason)" in S['Trading.cs'] and
    "if (!Structural(kv.Key) && kv.Value > best)" in
    method_body(S['Trading.cs'], "internal Block Dominant") and
    "Structural" not in method_body(S['Trading.cs'], "internal string Summary"))
chk("1.5.1", "no two settings in one MCM group claim the same position",
    mcm_orders_unique())

chk("1.5.2", "a listed route passes both the buy and the sell policy check",
    "internal static bool MayRoundTrip(ItemObject item, ISet<string> lockedKeys)" in S['Trading.cs'] and
    "PolicyAllows(PolicyFor(item), buying: false)" in
    method_body(S['Trading.cs'], "internal static bool MayRoundTrip") and
    "if (!TradePolicy.MayRoundTrip(item, locked)) continue;" in S['Ledger.cs'] and
    "TradePolicy.MayBuy(item, locked)" not in S['Ledger.cs'])
chk("1.5.2", "port menus are asked for only where the module that owns them is installed",
    "if (NavalModulePresent())" in S['Trading.cs'] and
    'ModuleHelper.GetModuleInfo("NavalDLC")' in
    method_body(S['Trading.cs'], "private static bool NavalModulePresent"))
chk("1.5.2", "panel and tooltip read prices through the same lookup",
    "BuyPrice = buyPrice, SellPrice = sellPrice," in S['Ledger.cs'] and
    "q.OpeningSellPrice" in S['Ledger.cs'] and
    S['Ledger.cs'].count("q.Opening") == 2)
chk("1.5.2", "RouteQuote carries no unread field",
    "ClosingBuyPrice" not in S['Market.cs'] and "ClosingSellPrice" not in S['Market.cs'] and
    S['Market.cs'].count("OpeningBuyPrice") == 2 and S['Market.cs'].count("OpeningSellPrice") == 2)

chk("1.5.2", "every setting default lies within its own declared range",
    mcm_defaults_within_range())
chk("1.5.2", "every option the module reads is exposed in the settings screen",
    every_option_has_a_control())
chk("1.5.2", "a timed-out publish is retried rather than abandoned",
    "for attempt in 1 2 3 4 5; do" in WORKFLOW and
    "the request landed despite the error" in WORKFLOW)
chk("1.5.2", "a draft release left by a timeout is deleted before republishing",
    "--json isDraft -q .isDraft" in WORKFLOW and
    'gh release delete "$VERSION" --yes' in WORKFLOW and
    "is already published - nothing to publish for this push" in WORKFLOW)

chk("1.5.3", "every declared Harmony patch is installed",
    every_declared_patch_is_installed())
chk("1.5.3", "both projects pin the same reference-assembly version",
    projects_pin_one_reference_assembly())
chk("1.5.3", "the manifest declares exactly one required dependency, Harmony",
    one_hard_dependency())
chk("1.5.3", "the release workflow reads its version from SubModule.xml",
    workflow_reads_the_manifest_version())
chk("1.5.4", "every Block reason maps to a player-facing message",
    refusal_reasons_are_named())
chk("1.5.4", "an unsimulated route is marked in the confidence column",
    "_route.Simulated ? " in S['Panel.cs'] and
    'Conf* =' in method_body(S['Panel.cs'], "private void Refresh"))
chk("1.5.4", "publish retries re-check draft state before giving up",
    WORKFLOW.count("--json isDraft -q .isDraft") == 2 and
    "stranded a draft - discarding it before trying again" in WORKFLOW)
chk("1.5.4", "the source carries no comments",
    not any(line.lstrip().startswith("//") for src in list(S.values()) + [M]
            for line in src.split("\n")))

chk("1.5.5", "simulation mode mutates no per-visit state",
    method_body(S['Trading.cs'], "public static void ExecuteQuickSell")
        .split("if (sim)")[1].split("continue;")[0].count("_soldThisVisit") == 0 and
    method_body(S['Trading.cs'], "public static void ExecuteQuickBuy")
        .split("if (sim)")[1].split("continue;")[0].count("_boughtThisVisit") == 0 and
    "best case" in M and "nothing moves" in M)
chk("1.5.6", "the log prefers the game's user folder over the module folder",
    log_prefers_the_user_folder())
chk("1.5.6", "log path resolution is attempted once, not per line",
    "if (_path == null) return;" in method_body(S['Support.cs'], "internal static void Write"))
chk("1.5.6", "live-price mode records no price observations",
    capture_skipped_after_the_caches_are_dropped())
chk("1.5.6", "expired observations are pruned on both save and load",
    method_body(S['Ledger.cs'], "public override void SyncData").count("PruneExpired();") == 2 and
    "if (!dataStore.IsLoading) PruneExpired();" in S['Ledger.cs'] and
    "if (dataStore.IsLoading) PruneExpired();" in S['Ledger.cs'] and
    "(shelf > 0f && now - o.CapturedDay > shelf)" in method_body(S['Ledger.cs'], "private void PruneObservations"))
chk("1.5.6", "the message filter is armed only around a game call that talks back",
    the_filter_is_armed_only_around_a_game_call_that_talks())
chk("1.5.6", "every place the filter comes down logs how many messages it suppressed",
    S['Trading.cs'].count("ReportSilenced();") == 4 and
    "NoteSilenced();" in method_body(S['Trading.cs'], "internal static class Patch_SilenceChunkedTradeLines"))
chk("1.5.12", "the message filter uses a depth counter, so nesting cannot disarm it early",
    "internal static bool InGameTransaction => _transactionDepth > 0;" in S['Trading.cs'] and
    "private static void OpenTransaction() => _transactionDepth++;" in S['Trading.cs'] and
    "if (_transactionDepth > 0) _transactionDepth--;" in
        method_body(S['Trading.cs'], "private static void CloseTransaction"))
chk("1.5.12", "an armed message filter is cleared at the start of the next frame",
    'Guard.Run("Tick.ReleaseMessageFilter", TradeActionBehavior.ReleaseMessageFilter)' in S['SubModule.cs'] and
    ordered(S['SubModule.cs'], "TradeActionBehavior.ReleaseMessageFilter", "TradeActionBehavior.FlushToasts") and
    "_transactionDepth = 0;" in method_body(S['Trading.cs'], "internal static void ReleaseMessageFilter"))
chk("1.5.12", "ending a campaign clears the message filter and per-visit state",
    'Guard.Run("GameEnd.Visit", TradeActionBehavior.ForgetVisit)' in S['SubModule.cs'] and
    all(f in method_body(S['Trading.cs'], "internal static void ForgetVisit")
        for f in ("ResetVisit();", "_transactionDepth = 0;", "AutomatedTradeInProgress = false;")))
chk("1.5.6", "a manual purchase is recorded at the price the shelf charged at the time",
    "Bulk.PricePaid(here, item, count, unit)" in
        method_body(S['Ledger.cs'], "private void OnPlayerInventoryExchange") and
    "shelf.Restock(units);" in method_body(S['Market.cs'], "internal static int PricePaid") and
    "shelf.Restock(-1);" in method_body(S['Market.cs'], "internal static int PricePaid"))
chk("1.5.6", "only the purchase-price rewind reads a shelf outside a projection",
    S['Market.cs'].count("projecting: false") == 1 and
    "projecting: false" in method_body(S['Market.cs'], "internal static int PricePaid") and
    S['Market.cs'].count("projecting: true") == 2)
chk("1.5.6", "panel setup is retried before being disabled",
    "private const int SetupAttempts = 3;" in S['Panel.cs'] and
    "if (_setupFailures >= SetupAttempts) return false;" in
        method_body(S['Panel.cs'], "private static bool MaySetUp") and
    "if (_setupCooldown > 0) { _setupCooldown--; return false; }" in S['Panel.cs'] and
    "_setupFailures = 0;" in method_body(S['Panel.cs'], "internal static void Reset"))
chk("1.5.6", "an unrecognized hotkey name is logged before falling back to T",
    hotkey_fallback_is_reported())
chk("1.5.6", "the cargo marker only targets a town where the cargo has a price",
    "long bestValue = 0;" in method_body(S['Trading.cs'], "private Settlement FindBestSellTownForCargo"))
chk("1.5.7", "units with no cost basis are still sold when purchased units miss the margin",
    "if (basisIsMarket || paidLeft <= 0 || remaining <= paidLeft) break;" in
        method_body(S['Trading.cs'], "public static void ExecuteQuickSell") and
    "remaining -= paidLeft;" in method_body(S['Trading.cs'], "public static void ExecuteQuickSell") and
    method_body(S['Trading.cs'], "public static void ExecuteQuickSell")
        .count("tally.Note(Block.BelowMargin)") == 1)
chk("1.5.8", "release notes come from the commit body, and an empty body fails the publish",
    'git log -1 --format=%b "$GITHUB_SHA" > release-notes.md' in WORKFLOW and
    "--notes-file release-notes.md" in WORKFLOW and
    empty_release_notes_are_rejected() and
    "Compiled and packaged by CI" not in WORKFLOW and
    "Install: extract the zip" not in WORKFLOW)
chk("1.5.8", "every panel line is localizable",
    prefab_text_is_all_bound() and
    S['Panel.cs'].count('new TextObject("{=TL') >= 20)
chk("1.5.8", "tooltip row suffixes carry localization markers",
    'new TextObject("{=TL77}Profit: +{PCT}%")' in S['TooltipPatches.cs'] and
    'new TextObject("{=TL78}Stock: {COUNT}")' in S['TooltipPatches.cs'] and
    'new TextObject("{=TL79}~{DAYS} days")' in S['Travel.cs'])
chk("1.5.8", "each language-file entry matches the source fallback text",
    shipped_text_matches_the_fallback())
chk("1.5.9", "panel-owned map pins survive a save/load cycle",
    'dataStore.SyncData("TradeLord_PanelPins", ref _pinnedTowns);' in S['Trading.cs'] and
    "if (!dataStore.IsLoading) _pinnedTowns = LedgerPanel.PinnedIds();" in
        method_body(S['Trading.cs'], "public override void SyncData") and
    "LedgerPanel.RestorePins(_pinnedTowns);" in
        method_body(S['Trading.cs'], "private void OnSessionLaunched") and
    "internal static void RestorePins(string ids)" in S['Panel.cs'] and
    "internal static string PinnedIds()" in S['Panel.cs'])
chk("1.5.9", "all workflow actions are on a supported runner major version",
    actions_are_off_the_node20_runtime())
chk("1.5.10", "a market quoting zero is excluded from the top-markets lists",
    "if (price <= 0) continue;" in method_body(S['Ledger.cs'], "private List<(Settlement, int)> TopLive") and
    "if (price <= 0) continue;" in method_body(S['Ledger.cs'], "private List<(Settlement, int)> TopObserved"))

chk("1.5.11", "every shipped file is ASCII-only",
    shipped_text_is_ascii())
chk("1.5.11", "no setting hint names another mod",
    settings_name_no_other_mod())
chk("1.5.11", "the workshop board respects the knowledge mode",
    "bool ownedOnly = !Options.Current.Omniscient;" in
        method_body(S['Panel.cs'], "private void RefreshWorkshops") and
    "if (ownedOnly && w.Owner != Hero.MainHero) continue;" in
        method_body(S['Panel.cs'], "private void RefreshWorkshops") and
    '{=TL80}' in S['Panel.cs'])
chk("1.5.11", "the panel profit line counts only profit made by this module",
    '{=TL66}TradeLord profit' in S['Panel.cs'] and
    "LedgerBehavior.Instance?.AddProfit(profit);" in
        method_body(S['Trading.cs'], "public static void ExecuteQuickSell") and
    "AddProfit" not in method_body(S['Trading.cs'], "public static void ExecuteQuickBuy"))

chk("1.6.1", "the trade XP the pass earns reaches the game only once the pass is over",
    "_pendingXp += xp;" in method_body(S['Trading.cs'], "private static void AwardTradeXp") and
    "SkillLevelingManager.OnTradeProfitMade" not in
        method_body(S['Trading.cs'], "private static void AwardTradeXp") and
    S['Trading.cs'].count("SkillLevelingManager.OnTradeProfitMade") == 1 and
    "SkillLevelingManager.OnTradeProfitMade" in
        method_body(S['Trading.cs'], "private static void CreditTradeSkill") and
    all("SkillLevelingManager" not in method_body(S['Trading.cs'], m)
        for m in ("public static void ExecuteQuickSell", "public static void ExecuteQuickBuy")))
chk("1.6.1", "the XP line is queued last, in amber, and is translatable",
    'private static readonly Color ToastXp = new Color(1f, 0.72f, 0.20f);' in S['Trading.cs'] and
    'Toast(earned, ToastXp);' in method_body(S['Trading.cs'], "private static void CreditTradeSkill") and
    ordered(method_body(S['Trading.cs'], "internal static void FlushToasts"),
            "CreditTradeSkill(xp, muted)",
            "InformationManager.DisplayMessage") and
    '{=TL81}TradeLord credited {GOLD} denars of profit to your Trade skill.' in S['Trading.cs'] and
    ordered(method_body(S['Trading.cs'], "public static void ExecuteQuickSell"),
            "Toast(msg, profit > 0",
            "AwardTradeXp(profit, Muted(quiet))"))
chk("1.6.1", "ending a campaign drops trade XP that was queued but not yet handed over",
    "_pendingXp = 0;" in method_body(S['Trading.cs'], "internal static void ForgetVisit"))
chk("1.6.1", "the gold reserve default leaves room for two safe passages and a wage run",
    "public int GoldReserve = 300;" in S['Options.cs'] and
    re.search(r'HintText = "\{=TL\d+\}Never spend below this much gold\. Default 300', M) is not None)

chk("1.6.2", "every per-frame and shutdown call runs inside the crash guard",
    all(f'Guard.Run("{c}"' in S['SubModule.cs'] for c in
        ("Tick.ReleaseMessageFilter", "Tick.FlushToasts",
         "GameEnd.Panel", "GameEnd.Travel", "GameEnd.Visit")) and
    all(re.search(r'^\s+(?!base\.|Guard\.Run|LedgerPanel\.Tick|LedgerBehavior\.Instance = null;)\S.*\(\);',
                  line) is None
        for name in ("protected override void OnApplicationTick", "public override void OnGameEnd")
        for line in method_body(S['SubModule.cs'], name).split('\n')))
chk("1.6.2", "every setting carries a hint",
    every_setting_has_a_hint())
chk("1.6.2", "each settings group is written in the order it is shown",
    settings_declared_in_display_order())
chk("1.6.2", "an always-sell entry still yields to the never-sell list and to an inventory lock",
    (lambda b: ordered(b, "Listed(s.NeverSet, item)", "Listed(s.AlwaysSet, item)")
           and ordered(b, "IsLocked(lockedKeys", "Listed(s.AlwaysSet, item)")
           and ordered(b, "Listed(s.AlwaysSet, item)", "PolicyAllows(PolicyFor(item)"))
    (method_body(S['Trading.cs'], "internal static bool MaySell")))
chk("1.6.2", "the cost basis lookup answers for a good it has never seen instead of throwing",
    "if (item == null) return 0;" in method_body(S['Ledger.cs'], "public int GetCostBasis"))
chk("1.6.2", "every source line is indented to its brace depth",
    indentation_matches_brace_depth())

chk("1.6.28", "a full cargo is reported on the way into a market and not again on the way out",
    "WarnNoRoomToCarry()" in
        method_body(S['Trading.cs'], "private void OnSettlementEntered") and
    "WarnNoRoomToCarry" not in
        method_body(S['Trading.cs'], "private void OnSettlementLeft") and
    S['Trading.cs'].count("WarnNoRoomToCarry()") == 2 and
    "if (tally.Saw(Block.CarryWeight)) _cargoWasFull = true;" in
        method_body(S['Trading.cs'], "public static void ExecuteQuickBuy"))
chk("1.6.4", "the full-cargo warning is red, translatable, and not silenced by a quiet pass",
    'ToastAlert = new Color(0.90f, 0.28f, 0.28f)' in S['Trading.cs'] and
    'Toast(new TextObject("{=TL82}' in method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry") and
    'ToastAlert)' in method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry") and
    'TL82' in strings_declared() and
    "quiet" not in method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry"))
chk("1.6.4", "the warning asks the same trading question the menu asks, and clears with the visit",
    method_body(S['Trading.cs'], "private void OnSettlementEntered").count("CanTradeHere(settlement)") == 1 and
    "_cargoWasFull = false;" in method_body(S['Trading.cs'], "private static void ResetVisit"))

chk("1.6.5", "an item list is parsed once per edit and never left unset",
    "if (set == null || seen != src)" in
        method_body(S['Options.cs'], "private static HashSet<string> Parsed") and
    S['Options.cs'].count("Parsed(") == 4)
chk("1.6.5", "ending a campaign drops trade messages queued but not yet shown",
    "_pending.Clear();" in method_body(S['Trading.cs'], "internal static void ForgetVisit"))
chk("1.6.5", "the route scan is reused within the hour and dropped with the market rankings",
    "_routes = ScanRoutes();" in method_body(S['Ledger.cs'], "public List<TradeRoute> BestRoutes") and
    "_routes = null;" in method_body(S['Ledger.cs'], "internal void ForgetMarketRankings") and
    "_routeGen != Options.Generation" in S['Ledger.cs'])
chk("1.6.5", "a village keeping its last unit of each good says so",
    "case Block.VillageLastUnit:" in
        method_body(S['Trading.cs'], "internal static TextObject Phrase") and
    "TL83" in strings_declared())

chk("1.6.6", "the always-sell list governs selling only, never what quick-buy purchases",
    "AlwaysSet" not in method_body(S['Trading.cs'], "internal static bool MayBuy") and
    "Listed(s.AlwaysSet, item)" in method_body(S['Trading.cs'], "internal static bool MaySell") and
    "Listed(Options.Current.AlwaysSet, item) ||" in
        method_body(S['Trading.cs'], "internal static bool MayRoundTrip"))
chk("1.6.6", "the ledger popup builds its route lines from a translatable string",
    '"{=TL84}{ITEM}: buy {FROM}' in S['Trading.cs'] and
    'r.Item.Name + ": buy "' not in S['Trading.cs'] and
    "TL84" in strings_declared())
chk("1.6.6", "the auto-trade switch lets the settings file speak for itself while it loads",
    "if (!Loaded || value == Options.Current.AutoTradeBoth) return;" in M and
    "Options.Current.AutoSellOnEntry = value;" in M)

chk("1.6.7", "the Trade XP line reports the denars of profit it hands the skill system, the number it actually passes",
    (lambda b: 'earned.SetTextVariable("GOLD", xp);' in b
           and "SkillLevelingManager.OnTradeProfitMade(Hero.MainHero, xp);" in b)
    (method_body(S['Trading.cs'], "private static void CreditTradeSkill")) and
    "Trade XP." not in S['Trading.cs'] and
    "trade profit fed to the XP system: " in S['Trading.cs'])
chk("1.6.7", "the queued trade messages are dropped even if one of them cannot be shown",
    "finally { _pending.Clear(); }" in method_body(S['Trading.cs'], "internal static void FlushToasts") and
    method_body(S['Trading.cs'], "internal static void FlushToasts").count("_pending.Clear()") == 1)
chk("1.6.7", "a good already bought here is passed over before the food reserve is spent on it",
    (lambda b: ordered(b, "_boughtThisVisit.ContainsKey", "TradePolicy.MaySell("))
    (method_body(S['Trading.cs'], "public static void ExecuteQuickSell")))
chk("1.6.7", "the panel's own pin list, not the map's marker state, decides what a click on a town pins and unpins",
    (lambda b: ordered(b, "_panelPins.Remove(settlement)", "tracker.CheckTracked(")
           and ordered(b, "_panelPins.Remove(settlement)", "_panelPins.Add(settlement)"))
    (method_body(S['Panel.cs'], "private static void ToggleMarker")) and
    "LedgerPanel.IsPinned(_trackedTown)" in S['Trading.cs'])

chk("1.6.8", "the map button reserves the mouse over the button, not over the map around it",
    (lambda b: "m.x >= 0.90f" in b and "m.y >= 0.46f && m.y <= 0.54f" in b)
    (method_body(S['Panel.cs'], "private static bool OverAssumedBounds")))
chk("1.6.8", "the food reserve is spent only on goods the sell rules would actually move",
    (lambda b: ordered(b, "why = Block.NotTradable; return false;", "foodKeep[item] = reserved - keepCount;"))
    (method_body(S['Trading.cs'], "internal static bool MaySell")))
chk("1.6.8", "another mod handles its own notification before TradeLord may hold one back",
    "[HarmonyPriority(Priority.Last)]" in
        method_body(S['Trading.cs'], "internal static class Patch_SilenceChunkedTradeLines"))

chk("1.6.9", "every setting name, hint and group heading carries a translation marker",
    every_setting_line_is_translatable())

chk("1.6.10", "the button's own measured size decides the reserved region, so it holds at any aspect ratio",
    (lambda b: "Screen.RealScreenResolutionWidth" in b and "button.ScaledSuggestedWidth" in b
           and "button.ScaledMarginRight" in b and "0.90f" not in b)
    (method_body(S['Panel.cs'], "private static bool OverButtonBounds")))
chk("1.6.10", "the prefab carries the id the panel looks the button up by",
    'Id="TradeLordMapButton"' in PREFAB and 'MapButtonId = "TradeLordMapButton"' in S['Panel.cs'])
chk("1.6.10", "the button still sits flush right and centred, which is what the reserved region assumes",
    re.search(r'Id="TradeLordMapButton"[\s\S]{0,400}?HorizontalAlignment="Right"', PREFAB) is not None and
    re.search(r'Id="TradeLordMapButton"[\s\S]{0,400}?VerticalAlignment="Center"', PREFAB) is not None)
chk("1.6.10", "an unreadable button falls back to the old region instead of reserving nothing",
    "return OverAssumedBounds(m);" in method_body(S['Panel.cs'], "private static bool OverButtonBounds") and
    "_mapButton = null;" in method_body(S['Panel.cs'], "internal static void Cleanup"))

chk("1.6.11", "a purchase record with nothing left in it is dropped rather than saved forever",
    re.search(r'PruneSettledPurchases\(\) =>\s*_purchases\?\.RemoveAll\(rec => rec == null \|\| '
              r'rec\.ItemId == null \|\| rec\.Count <= 0\);', S['Ledger.cs']) is not None)
chk("1.6.11", "the purchase index is rebuilt after a prune, never left pointing at dropped records",
    (lambda b: ordered_last(b, "PruneExpired();", "Reindex();"))
    (method_body(S['Ledger.cs'], "public override void SyncData")))
chk("1.6.11", "every reader of a purchase record already requires units left, so dropping a spent one changes nothing",
    all("rec.Count > 0" in line for line in
        [method_body(S['Ledger.cs'], "public bool HasPurchaseRecord"),
         method_body(S['Ledger.cs'], "public int PurchasedUnits"),
         method_body(S['Ledger.cs'], "public int GetCostBasis")]))
chk("1.6.11", "shelf-life pruning stays independent of purchase pruning, so 'never expire' does not keep spent records",
    "ObservationShelfLifeDays" in method_body(S['Ledger.cs'], "private void PruneObservations") and
    re.search(r'private void Prune\(\)\s*\{\s*PruneObservations\(\);\s*PruneSettledPurchases\(\);\s*\}',
              S['Ledger.cs']) is not None)

chk("1.6.12", "goods with no price paid are credited at what the cheapest market would have charged",
    (lambda b: "BestBuy(item)" in b and "item.Value" in b)
    (method_body(S['Trading.cs'], "internal static int UnpaidWorth")))
chk("1.6.12", "a paid unit is still credited exactly as before, against what was paid for it",
    "if (basis > 0) return proceeds - basis;" in
        method_body(S['TradeMath.cs'], "public static int Credit"))
chk("1.6.12", "an unpaid unit sold below that worth credits nothing rather than a loss",
    "return gain > 0 ? gain : 0;" in
        method_body(S['TradeMath.cs'], "public static int Credit"))
chk("1.6.12", "the simulated pass and the real one credit profit through the same rule",
    S['Trading.cs'].count("TradePolicy.Credit(") == 2 and
    "TradePolicy.Credit(price, basis, unpaidWorth)" in S['Trading.cs'] and
    "TradePolicy.Credit(proceeds, basis, unpaidWorth)" in S['Trading.cs'])
chk("1.6.12", "what quick-sell agrees to sell is unchanged, since the decision still runs on the bare basis",
    "if (!TradePolicy.ProfitAcceptable(basis, price))" in S['Trading.cs'] and
    "TradeMath.ProfitAcceptable(costBasis, townSellPrice, Options.Current.MinProfitMargin);" in S['Trading.cs'] and
    re.search(r'ProfitAcceptable\(int costBasis, int townSellPrice, float margin\) =>\s*costBasis > 0\s*\?\s*'
              r'townSellPrice >= costBasis \* \(1f \+ margin\)\s*:\s*'
              r'townSellPrice > 0;', S['TradeMath.cs']) is not None)
chk("1.6.12", "loot is still held back from a poor market by the same best-market floor",
    "if (Options.Current.PreferBestSellTown || basis == 0)" in S['Trading.cs'])
chk("1.6.12", "that worth is looked up once per good, not once per unit sold",
    "if (basis == 0 && unpaidWorth < 0) unpaidWorth = TradePolicy.UnpaidWorth(item);" in S['Trading.cs'] and
    S['Trading.cs'].count("TradePolicy.UnpaidWorth(") == 1)
chk("1.6.12", "the tooltip and the sale summary now value an unbought good the same way",
    "var best = BestBuy(item);" in method_body(S['Ledger.cs'], "public int GetCostBasis") and
    "best.price > 0 ? best.price : item.Value" in method_body(S['Ledger.cs'], "public int GetCostBasis") and
    "best.Item2 > 0 ? best.Item2 : item.Value" in method_body(S['Trading.cs'], "internal static int UnpaidWorth"))

def mcm_generation_matches_the_package():
    pkg = re.search(r'Bannerlord\.MCM"\s+Version="(\d+)\.', "\n".join(PROJ))
    code = re.search(r'McmGeneration = (\d+);', S['Support.cs'])
    return bool(pkg) and bool(code) and pkg.group(1) == code.group(1)

chk("1.6.13", "the MCM line the loader expects is the one the settings companion is built against",
    mcm_generation_matches_the_package())
chk("1.6.13", "no MCM line is written into the loader by hand, so bumping the package moves it",
    '"MCMv5"' not in S['Support.cs'] and 'Named(int generation) => "MCMv" + generation;' in S['Support.cs'])
chk("1.6.13", "an MCM this build was not made for is reported as a mismatch, not as MCM being absent",
    (lambda b: "MCM not detected" in b and "the game has loaded" in b and
               ordered(b, "MCM not detected", "the game has loaded"))
    (method_body(S['Support.cs'], "internal static void TryLoad")))
chk("1.6.13", "the mismatch line names both the line found and the line this build needs",
    (lambda b: 'Named(McmGeneration) +' in b and '" and the game has loaded " + found' in b)
    (method_body(S['Support.cs'], "internal static void TryLoad")))
chk("1.6.13", "a settings screen is only registered for the line the companion can actually talk to",
    (lambda b: ordered(b, "string.Equals(found, Named(McmGeneration)", "Bannerlord.ButterLib"))
    (method_body(S['Support.cs'], "internal static void TryLoad")))
chk("1.6.13", "detection reads the line off the assembly name rather than testing for one known line",
    (lambda b: "while (end < name.Length && char.IsDigit(name[end])) end++;" in b and "return end > 4" in b)
    (method_body(S['Support.cs'], "private static string GenerationOf")))
chk("1.6.13", "an already-loaded usable line wins over a newer one, so the settings screen still opens",
    (lambda b: ordered(b, "return generation;", "if (other == null) other = generation;"))
    (method_body(S['Support.cs'], "private static string Detect")))
chk("1.6.13", "a line newer than this build is still found when nothing has loaded it yet",
    "g <= McmGeneration + GenerationsAhead" in method_body(S['Support.cs'], "private static string Detect"))

chk("1.6.14", "the auto-marker claims a town only when it placed the marker itself, so it never removes one you set",
    re.search(r'if \(target != null && !tracker\.CheckTracked\(target\)\)\s*\{\s*'
              r'tracker\.RegisterObject\(target\);\s*_trackedTown = target;\s*\}',
              method_body(S['Trading.cs'], "private void UpdateBestSellTownTracker")) is not None and
    "if (_trackedTown != null && !LedgerPanel.IsPinned(_trackedTown) && tracker.CheckTracked(_trackedTown))"
        in S['Trading.cs'])
chk("1.6.14", "a pin restored from a save is put back on the map, so the panel and the map agree",
    (lambda b: "VisualTrackerManager tracker = Campaign.Current?.VisualTrackerManager;" in b
           and "if (tracker != null && !tracker.CheckTracked(s)) tracker.RegisterObject(s);" in b
           and ordered(b, "_panelPins.Add(s);", "tracker.RegisterObject(s);"))
    (method_body(S['Panel.cs'], "internal static void RestorePins")))
chk("1.6.14", "the compatibility tool drains the restore output it redirected, so a noisy restore cannot wedge it",
    (lambda b: "ReadToEndAsync()" in b and "p.StandardOutput.ReadToEnd()" in b
           and ordered(b, "ReadToEndAsync()", "p.WaitForExit()")
           and ordered(b, "p.StandardOutput.ReadToEnd()", "p.WaitForExit()"))
    (method_body(COMPAT, "private static bool Fetch")))
chk("1.6.14", "the compatibility tool checks every game member the mod patches or reaches for by name",
    compat_checks_every_game_hook())
chk("1.6.14", "a settings change reopens the hourly capture, so a market is not left unrecorded for the whole visit",
    (lambda b: "Options.Generation == _capturedGen) return;" in b
           and ordered(b, "Options.Generation == _capturedGen", "_capturedGen = Options.Generation;"))
    (method_body(S['Ledger.cs'], "public void CaptureSettlement")))

chk("1.6.15", "an unreadable price observation is dropped whatever the shelf life is set to",
    (lambda b: "if (_ledger == null) return;" in b
           and "o == null || o.TownId == null" in b
           and ordered(b, "if (_ledger == null) return;", "ObservationShelfLifeDays"))
    (method_body(S['Ledger.cs'], "private void PruneObservations")))
chk("1.6.15", "an item whose observations have all gone is dropped from the save whatever the shelf life is set to",
    (lambda b: "if (kv.Value == null || kv.Value.Count == 0) spent.Add(kv.Key);" in b
           and "_ledger.Remove(spent[i]);" in b and "shelf <= 0f" not in b)
    (method_body(S['Ledger.cs'], "private void PruneObservations")))
chk("1.6.15", "each market that trades nothing is named in the log, not just the first with those reasons",
    S['Trading.cs'].count('Log.Repeatable("quick-sell-empty " + settlement.StringId') == 1 and
    S['Trading.cs'].count('Log.Repeatable("quick-buy-empty " + settlement.StringId') == 1)
chk("1.6.15", "ending a campaign clears what the log has already reported, so the next one reports it again",
    "_repeats.Clear();" in method_body(S['Support.cs'], "internal static void Forget") and
    "_errors.Clear();" in method_body(S['Support.cs'], "internal static void Forget") and
    'Guard.Run("GameEnd.Log", Log.Forget)' in S['SubModule.cs'])

chk("1.6.16", "holding cargo for a better market is named as its own reason, not as the profit margin",
    (lambda b: b.count("case Block.BelowBestMarket:") == 1
           and '{=TL85}' in b and '{=TL42}' in b
           and ordered(b, "case Block.BelowMargin:", '{=TL42}', "case Block.BelowBestMarket:"))
    (method_body(S['Trading.cs'], "internal static TextObject Phrase")) and
    "TL85" in strings_declared())
chk("1.6.16", "a campaign starts with the herd guard re-armed, so an earlier one cannot leave livestock buying off",
    "_herdLookupFailed = false;" in method_body(S['Trading.cs'], "internal static void ForgetVisit") and
    "if (_herdLookupFailed) return 0;" in
        method_body(S['Trading.cs'], "internal static int HerdRoomForLivestock") and
    "_herdModifier = null" not in method_body(S['Trading.cs'], "internal static void ForgetVisit"))
chk("1.6.16", "a full herd is named as its own reason, not as a full cargo hold",
    (lambda b: b.count("case Block.HerdFull:") == 1
           and '{=TL86}' in b and '{=TL44}' in b
           and ordered(b, "case Block.CarryWeight:", '{=TL44}', "case Block.HerdFull:"))
    (method_body(S['Trading.cs'], "internal static TextObject Phrase")) and
    "TL86" in strings_declared() and
    "if (tally.Saw(Block.CarryWeight)) _cargoWasFull = true;" in S['Trading.cs'])
chk("1.6.16", "the auto-marker is put back on the map when a save loads, and cannot cost the menus if it fails",
    (lambda b: "LedgerPanel.RestorePins(_pinnedTowns);" in b
           and 'Guard.Run("Action.RestoreMarker", UpdateBestSellTownTracker);' in b
           and ordered(b, "LedgerPanel.RestorePins", 'Guard.Run("Action.RestoreMarker"', 'AddOptions("town");'))
    (method_body(S['Trading.cs'], "private void OnSessionLaunched")))
chk("1.6.18", "a campaign is told once that entering a market trades for it, before the pass that does so",
    (lambda b: "if (_announcedAutomation) return false;" in b
           and "if (!Options.Current.AutoSellOnEntry && !Options.Current.AutoBuyOnEntry) return false;" in b
           and "if (!CanTradeHere(settlement)) return false;" in b
           and "_announcedAutomation = true;" in b and '{=TL87}' in b and b.rstrip().endswith("return true;\n        }"))
    (method_body(S['Trading.cs'], "private bool AnnounceAutomation")) and
    "TL87" in strings_declared())
chk("1.6.24", "the market that carries the notice is left alone, so a campaign can turn automation off before it runs",
    (lambda b: "if (!AnnounceAutomation(settlement))" in b
           and ordered(b, "if (!AnnounceAutomation(settlement))", "ExecuteQuickSell(settlement, quiet: true)")
           and ordered(b, "ExecuteQuickBuy(settlement, quiet: true)", "WarnNoRoomToCarry()"))
    (method_body(S['Trading.cs'], "private void OnSettlementEntered")) and
    "starting at the next one" in S['Trading.cs'])
chk("1.6.18", "the notice is remembered in the save, so it is shown once per campaign and not once per market",
    'dataStore.SyncData("TradeLord_AutomationNotice", ref _announcedAutomation);' in S['Trading.cs'] and
    "_announcedAutomation" not in method_body(S['Trading.cs'], "internal static void ForgetVisit") and
    "private bool _announcedAutomation;" in S['Trading.cs'])
chk("1.6.18", "every variable a shipped line leaves a slot for is filled in by name",
    every_text_variable_is_supplied())
chk("1.6.18", "the defaults the README publishes are the defaults the module ships",
    readme_defaults_match_the_shipped_ones())
chk("1.6.18", "the changelog opens on the version the manifest ships, and that entry says something",
    changelog_opens_on_the_shipped_version())
chk("1.6.20", "a campaign save carries no type this module defines, so removing the mod cannot cost the save",
    every_saved_value_is_a_plain_one() and nothing_this_module_defines_is_saveable() and
    "private string _ledgerText" in S['Ledger.cs'] and
    "private string _purchaseText" in S['Ledger.cs'])
chk("1.6.20", "no collection of the module's own making is written into a save",
    no_collection_of_our_own_reaches_a_save())
chk("1.6.20", "the ledger a save carries is written and read without the game being involved",
    the_codec_needs_nothing_from_the_game())
chk("1.6.20", "the ledger text is rebuilt from pruned data every time the campaign is saved",
    (lambda b: ordered(b,
                       "if (!dataStore.IsLoading) PruneExpired();",
                       "_ledgerText = LedgerCodec.WriteLedger(_ledger);",
                       'dataStore.SyncData("TradeLord_LedgerText"'))
    (method_body(S['Ledger.cs'], "public override void SyncData")) and
    "if (dataStore.IsSaving)" in S['Ledger.cs'])
chk("1.6.20", "numbers in a saved ledger are written and read the same way in every language",
    saved_numbers_read_the_same_in_every_language())
chk("1.6.20", "an item or town whose name could pass for a separator is left out of the save whole",
    a_name_that_looks_like_a_separator_is_left_out())
chk("1.6.20", "a line of saved text that cannot be read is dropped on its own, not with the whole ledger",
    a_record_that_cannot_be_read_is_dropped_on_its_own())
chk("1.6.20", "the saved ledger is proved to survive a save and a load by tests the build runs",
    the_codec_is_covered_by_tests_the_build_runs())
chk("1.6.21", "a visit that traded something is not then told its cargo is full",
    the_full_cargo_warning_waits_for_a_visit_that_traded_nothing())
chk("1.6.21", "the trade skill gain is reported once, in TradeLord's own line",
    the_trade_skill_gain_is_reported_in_one_line())
chk("1.6.22", "a buy cap of zero turns the cap off instead of stopping every purchase",
    a_zero_cap_never_means_buy_nothing())
chk("1.6.22", "every numeric setting that switches off at zero says so on its own label",
    every_numeric_setting_that_switches_off_at_zero_says_so())
chk("1.6.22", "a pass that bought nothing because of your purse says so, even when it is trading quietly",
    the_purse_speaks_up_even_on_a_quiet_pass())
chk("1.6.22", "the item tooltip adds its prices without announcing the mod by name",
    the_item_tooltip_does_not_announce_the_mod())
chk("1.6.22", "the line under the ledger is set at a size that can be read",
    the_panel_legend_is_legible())
chk("1.6.22", "the tool reads a game install from a variable, copies nothing, and the repository refuses to carry a game assembly",
    the_game_assemblies_are_read_from_a_variable_and_never_copied())
chk("1.6.22", "a menu id the mod does not guard fails the run, and a guarded one does not",
    a_menu_id_the_mod_does_not_guard_fails_the_run())
chk("1.6.22", "with no game install named, the menu-id check is skipped rather than failed",
    the_menu_id_check_is_skipped_rather_than_failed_when_unset())
chk("1.6.24", "the rules that decide what a trade is worth need nothing from the game",
    the_money_rules_need_nothing_from_the_game())
chk("1.6.24", "the policy layer forwards to those rules instead of keeping a second copy",
    the_policy_layer_keeps_no_second_copy_of_the_money_rules())
chk("1.6.24", "margin, resale factor and profit credit are proved by tests the build runs",
    the_money_rules_are_covered_by_tests_the_build_runs())
chk("1.6.24", "the purse rule and the route confidence need nothing from the game",
    the_route_rules_need_nothing_from_the_game())
chk("1.6.24", "the purse rule, route confidence and the item lists are proved by tests the build runs",
    the_route_rules_are_covered_by_tests_the_build_runs())

def the_purse_outranks_the_reasons_that_are_merely_counted():
    body = method_body(S['Trading.cs'], "internal Block Dominant")
    return ("if (Saw(Block.BudgetSpent)) return Block.BudgetSpent;" in body
            and ordered(body, "if (Saw(Block.BudgetSpent)) return Block.BudgetSpent;",
                        "foreach (var kv in _counts)")
            and "tally.Any && tally.Dominant() == Block.BudgetSpent;" in S['Trading.cs'])

def an_empty_purse_is_reported_on_the_way_into_a_market():
    body = method_body(S['Trading.cs'], "private static bool WarnPurseBelowReserve")
    entered = method_body(S['Trading.cs'], "private void OnSettlementEntered")
    return ("if (TradedThisVisit()) return false;" in body
            and ordered(body, "if (TradedThisVisit()) return false;",
                        "if (SpendableGold() > 0) return false;")
            and 'new TextObject("{=TL92}' in body
            and 'Toast(msg, ToastAlert);' in body
            and 'TL92' in strings_declared()
            and "quiet" not in body and "Muted(" not in body
            and "if (!WarnPurseBelowReserve()) WarnNoRoomToCarry();" in entered
            and ordered(entered, "ExecuteQuickBuy(settlement, quiet: true)",
                        "WarnPurseBelowReserve()"))

def what_is_left_to_spend_is_worked_out_in_one_place():
    return ("private static int SpendableGold() =>\n"
            "            TradeMath.Budget(Hero.MainHero.Gold, Options.Current.GoldReserve,\n"
            "                             Options.Current.MaxSpendPerVisit, _spentThisVisit, 0);"
                in S['Trading.cs']
            and S['Trading.cs'].count("TradeMath.Budget(") == 2)

def no_tracked_file_carries_a_machine_written_dash():
    import subprocess
    dashes = "\u2012\u2013\u2014\u2015\u2212\ufe58\uff0d"
    listed = subprocess.run(["git", "ls-files", "-z"], capture_output=True)
    if listed.returncode != 0:
        return False
    for name in listed.stdout.decode("utf-8").split("\0"):
        if not name:
            continue
        try:
            body = io.open(name, encoding="utf-8").read()
        except (IOError, OSError, UnicodeDecodeError):
            continue
        if any(d in body for d in dashes):
            return False
    return True

def a_list_entry_is_matched_whatever_its_capitalisation():
    parsed = method_body(S['Options.cs'], "private static HashSet<string> Parsed")
    return ("StringComparer.OrdinalIgnoreCase" in parsed and
            "A_name_is_matched_whatever_its_capitalisation" in ROUTETESTS)

def an_item_list_is_matched_by_name_as_well_as_by_id():
    listed = method_body(S['Trading.cs'], "internal static bool Listed")
    return ("list.Contains(item.StringId)" in listed and
            "list.Contains(item.Name.ToString())" in listed)

def no_rule_compares_a_list_against_an_id_by_hand():
    return not re.search(r'(NeverSet|AlwaysSet|NeverBuySet)\.Contains\(', S['Trading.cs'])

def a_list_entry_survives_the_space_inside_a_name():
    parsed = method_body(S['Options.cs'], "private static HashSet<string> Parsed")
    return ("Split(EntryMarks" in parsed and "built.Add(whole)" in parsed and
            "Split(WordMarks" in parsed and "' '" not in parsed)

def a_list_entry_that_names_nothing_is_reported_both_ways():
    audit = method_body(S['Trading.cs'], "internal static bool ItemListsNameNothing")
    warn = method_body(S['Trading.cs'], "private static void WarnUnmatchedItemLists")
    return ("Items.All" in audit and audit.count("Unmatched(") == 3 and
            "Log.Write" in method_body(S['Trading.cs'], "private static bool Unmatched") and
            "TradePolicy.ItemListsNameNothing()" in warn and "Toast(" in warn and
            "WarnUnmatchedItemLists();" in method_body(S['Trading.cs'], "private void OnSettlementEntered"))

def the_list_audit_is_redone_when_the_lists_are_edited():
    audit = method_body(S['Trading.cs'], "internal static bool ItemListsNameNothing")
    return ("_auditedGeneration == Options.Generation" in audit and
            "TradePolicy.ForgetItemListAudit();" in method_body(S['Trading.cs'], "internal static void ForgetVisit"))

def a_list_still_naming_nothing_after_an_edit_is_said_again():
    warn = method_body(S['Trading.cs'], "private static void WarnUnmatchedItemLists")
    return ("if (!TradePolicy.ItemListsNameNothing()) return;" in warn
            and "_auditSpoke" not in S['Trading.cs']
            and "AuditShouldSpeak" not in S['Trading.cs'])

def the_audit_reads_the_game_only_for_a_list_with_something_in_it():
    audit = method_body(S['Trading.cs'], "internal static bool ItemListsNameNothing")
    return ordered(audit, "string.IsNullOrEmpty(s.NeverSellItems)",
                   "string.IsNullOrEmpty(s.NeverBuyItems)) return false;", "Items.All")

def quiet_automation_silences_only_the_automated_lines():
    sell = method_body(S['Trading.cs'], "public static void ExecuteQuickSell")
    buy = method_body(S['Trading.cs'], "public static void ExecuteQuickBuy")
    credit = method_body(S['Trading.cs'], "private static void CreditTradeSkill")
    return ("automated && Options.Current.QuietAutomation" in
                method_body(S['Trading.cs'], "private static bool Muted") and
            "if (!Muted(quiet)) Toast(msg, profit > 0" in sell and
            "if (!Muted(quiet)) Toast(msg, ToastSpend);" in buy and
            "if (!muted) Toast(earned, ToastXp);" in credit and
            "AwardTradeXp(profit, Muted(quiet));" in sell)

def quiet_automation_leaves_the_notice_and_the_cargo_warning_alone():
    return ("Muted(" not in method_body(S['Trading.cs'], "private bool AnnounceAutomation") and
            "Muted(" not in method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry"))

def a_second_campaign_starts_the_panel_from_scratch():
    reset = method_body(S['Panel.cs'], "internal static void Reset")
    return all(f in reset for f in
               ("_loggedArmed = false;", "_loggedButtonFallback = false;",
                "_idleMouseActive = false;", "_keySource = null;"))

def the_item_list_reading_is_covered_by_tests_the_build_runs():
    return ("A_name_with_a_space_in_it_is_kept_whole" in ROUTETESTS and
            "Ids_written_the_old_way_with_spaces_between_them_still_read" in ROUTETESTS and
            "A_multi_word_name_is_not_confused_with_a_neighbouring_entry" in ROUTETESTS and
            'Include="..\\src\\Options.cs"' in TESTPROJ)

chk("1.6.26", "a good on an item list is matched by the name the game shows as well as by its id",
    an_item_list_is_matched_by_name_as_well_as_by_id())
chk("1.6.26", "every rule reads the item lists through that one matcher",
    no_rule_compares_a_list_against_an_id_by_hand())
chk("1.6.26", "a name with a space in it is read as one entry rather than split into words",
    a_list_entry_survives_the_space_inside_a_name())
chk("1.6.26", "an entry matching no good is named in the log and said on screen",
    a_list_entry_that_names_nothing_is_reported_both_ways())
chk("1.6.26", "editing a list has it checked again rather than answered from the last check",
    the_list_audit_is_redone_when_the_lists_are_edited())
chk("1.6.26", "quiet automation silences the entry summaries, the buy line and the skill line",
    quiet_automation_silences_only_the_automated_lines())
chk("1.6.26", "quiet automation leaves the first-run notice and the cargo warning speaking",
    quiet_automation_leaves_the_notice_and_the_cargo_warning_alone())
chk("1.6.26", "a second campaign in one sitting starts the panel from scratch",
    a_second_campaign_starts_the_panel_from_scratch())
chk("1.6.26", "the item lists are read the way the tests the build runs say they are",
    the_item_list_reading_is_covered_by_tests_the_build_runs())
chk("1.6.26", "an entry is matched whatever its capitalisation, and the tests the build runs say so",
    a_list_entry_is_matched_whatever_its_capitalisation())
chk("1.6.26", "no file in the repository carries an em dash or an en dash",
    no_tracked_file_carries_a_machine_written_dash())

chk("1.6.27", "a list edited into a state that still names nothing is said on screen again",
    a_list_still_naming_nothing_after_an_edit_is_said_again())
chk("1.6.27", "the audit reads the game's goods only when a list has something to check",
    the_audit_reads_the_game_only_for_a_list_with_something_in_it())

chk("1.6.29", "an empty purse outranks the reasons that are merely counted, so it is named rather than the shop",
    the_purse_outranks_the_reasons_that_are_merely_counted())
chk("1.6.29", "a purse at or under the reserve is reported on the way into a market, past a quiet pass",
    an_empty_purse_is_reported_on_the_way_into_a_market())
chk("1.6.29", "what is left to spend is worked out in one place for both the warning and the buying",
    what_is_left_to_spend_is_worked_out_in_one_place())

print(f"\n{sum(results)}/{len(results)} source checks passed")
sys.exit(0 if all(results) else 1)
