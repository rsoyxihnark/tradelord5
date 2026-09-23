import io, os, re, sys

NEVER_DELETED = ['CHANGELOG.md', 'CLAUDE.md', '.github/workflows/build.yml',
                 '.claude/settings.json', '.claude/hooks/session-start.sh',
                 '.claude/hooks/no-new-branch.sh']
_gone = [one for one in NEVER_DELETED if not os.path.exists(one)]
if _gone:
    print('  BROKEN  a file this repository never deletes is gone, so nothing else could be checked:')
    for one in _gone:
        print('            ' + one)
    print('\n0/0 source checks passed')
    sys.exit(1)

S = {f: io.open('src/' + f, encoding='utf-8').read() for f in
     ['Trading.cs', 'Drove.cs', 'Marker.cs', 'Notices.cs', 'Passes.cs', 'Ranking.cs', 'Policy.cs', 'Reasons.cs', 'Encounters.cs', 'Ledger.cs', 'LedgerCodec.cs', 'TradeMath.cs', 'Confidence.cs', 'Panel.cs',
      'Travel.cs', 'Support.cs', 'Options.cs', 'TooltipPatches.cs', 'SubModule.cs', 'Market.cs', 'Tongue.cs',
      'Config.cs', 'Migrate.cs', 'Books.cs', 'Rules.cs', 'Forecast.cs', 'Projection.cs', 'Hindsight.cs',
      'Scoring.cs', 'Counter.cs', 'Workshops.cs']}
TESTS = io.open('tests/LedgerCodecTests.cs', encoding='utf-8').read()
MATHTESTS = io.open('tests/TradeMathTests.cs', encoding='utf-8').read()
ROUTETESTS = io.open('tests/RouteRulesTests.cs', encoding='utf-8').read()
MIGRATIONTESTS = io.open('tests/MigrationTests.cs', encoding='utf-8').read()
BOOKTESTS = io.open('tests/BooksTests.cs', encoding='utf-8').read()
SELLTESTS = io.open('tests/SellRulesTests.cs', encoding='utf-8').read()
DRIFTTESTS = io.open('tests/PriceDriftTests.cs', encoding='utf-8').read()
RANKTESTS = io.open('tests/MarketRankTests.cs', encoding='utf-8').read()
PROJECTIONTESTS = io.open('tests/ProjectionTests.cs', encoding='utf-8').read()
SCORINGTESTS = io.open('tests/ScoringTests.cs', encoding='utf-8').read()
FOODTESTS = io.open('tests/FoodReserveTests.cs', encoding='utf-8').read()
STALLTESTS = io.open('tests/StallReasonTests.cs', encoding='utf-8').read()
BUYPASSTESTS = io.open('tests/BuyPassTests.cs', encoding='utf-8').read()
SELLPASSTESTS = io.open('tests/SellPassTests.cs', encoding='utf-8').read()
BUYRULETESTS = io.open('tests/BuyRulesTests.cs', encoding='utf-8').read()
HERDTESTS = io.open('tests/HerdRulesTests.cs', encoding='utf-8').read()
ARRIVALTESTS = io.open('tests/ArrivalTests.cs', encoding='utf-8').read()
SETTLINGTESTS = io.open('tests/SettlingTests.cs', encoding='utf-8').read()
RANKROWTESTS = io.open('tests/RankTests.cs', encoding='utf-8').read()
MAPBUTTONTESTS = io.open('tests/MapButtonTests.cs', encoding='utf-8').read()
TWINSTESTS = io.open('tests/TwinsTests.cs', encoding='utf-8').read()
SETTINGSFILETESTS = io.open('tests/SettingsFileTests.cs', encoding='utf-8').read()
SCREENTESTS = io.open('tests/ScreenTests.cs', encoding='utf-8').read()
RECENTTESTS = io.open('tests/RecentTests.cs', encoding='utf-8').read()
EXPIRYTESTS = io.open('tests/ExpiryTests.cs', encoding='utf-8').read()
SHELFORDERTESTS = io.open('tests/ShelfOrderTests.cs', encoding='utf-8').read()
PROMISETESTS = io.open('tests/PromiseTests.cs', encoding='utf-8').read()
STAMPTESTS = io.open('tests/StampTests.cs', encoding='utf-8').read()
DEALTESTS = io.open('tests/DealTests.cs', encoding='utf-8').read()
LOTTESTS = io.open('tests/LotTests.cs', encoding='utf-8').read()
FLOORTESTS = io.open('tests/BestMarketFloorTests.cs', encoding='utf-8').read()
TALLYTESTS = io.open('tests/TallyTests.cs', encoding='utf-8').read()
HOLDINGTESTS = io.open('tests/HoldingsTests.cs', encoding='utf-8').read()
T = {'LedgerCodecTests.cs': TESTS, 'TradeMathTests.cs': MATHTESTS,
     'HoldingsTests.cs': HOLDINGTESTS,
     'RouteRulesTests.cs': ROUTETESTS, 'MigrationTests.cs': MIGRATIONTESTS,
     'BooksTests.cs': BOOKTESTS, 'SellRulesTests.cs': SELLTESTS,
     'PriceDriftTests.cs': DRIFTTESTS, 'MarketRankTests.cs': RANKTESTS,
     'ProjectionTests.cs': PROJECTIONTESTS, 'ScoringTests.cs': SCORINGTESTS,
     'FoodReserveTests.cs': FOODTESTS, 'StallReasonTests.cs': STALLTESTS,
     'BuyPassTests.cs': BUYPASSTESTS, 'SellPassTests.cs': SELLPASSTESTS,
     'BuyRulesTests.cs': BUYRULETESTS, 'HerdRulesTests.cs': HERDTESTS,
     'ArrivalTests.cs': ARRIVALTESTS, 'SettlingTests.cs': SETTLINGTESTS,
     'RankTests.cs': RANKROWTESTS, 'MapButtonTests.cs': MAPBUTTONTESTS,
     'TwinsTests.cs': TWINSTESTS, 'SettingsFileTests.cs': SETTINGSFILETESTS,
     'ScreenTests.cs': SCREENTESTS, 'TallyTests.cs': TALLYTESTS,
     'RecentTests.cs': RECENTTESTS, 'ExpiryTests.cs': EXPIRYTESTS,
     'ShelfOrderTests.cs': SHELFORDERTESTS,
     'StampTests.cs': STAMPTESTS,
     'PromiseTests.cs': PROMISETESTS,
     'BestMarketFloorTests.cs': FLOORTESTS,
     'DealTests.cs': DEALTESTS,
     'LotTests.cs': LOTTESTS}
TESTPROJ = io.open('tests/TradeLord.Tests.csproj', encoding='utf-8').read()
M = io.open('mcm/Settings.cs', encoding='utf-8').read()
WORKFLOW = io.open('.github/workflows/build.yml', encoding='utf-8').read()
PROJ = [io.open(f, encoding='utf-8').read() for f in
        ['src/TradeLord.csproj', 'mcm/TradeLord.MCM.csproj']]
PREFAB = io.open('TradeLord/GUI/Prefabs/TradeLordPanel.xml', encoding='utf-8').read()
GAME_VERSION_BETA = '1.5.3.122374'
COMPAT = io.open('tools/compat/Program.cs', encoding='utf-8').read()
SWEEP = io.open('tools/regression_sweep.py', encoding='utf-8').read()
NEXUS = io.open('tools/nexus_changelog.py', encoding='utf-8').read()
RELEASED = io.open('tools/released.py', encoding='utf-8').read()

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

def english_string(sid):
    import xml.etree.ElementTree as ET
    for e in ET.parse('TradeLord/ModuleData/Languages/module_strings.xml').getroot().iter('string'):
        if e.get('id') == sid:
            return e.get('text') or ''
    return ''

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
    kept = set(re.findall(r'public\s+(?:bool|int|float|string)\s+(\w+)\s*=(?!>)', S['Options.cs']))
    shown = set(re.findall(r'public\s+(?:bool|int|float|string)\s+(\w+)\s*\{\s*get\s*=>\s*_o\.\w+;', M))
    shown |= set(re.findall(r'to\.(\w+)\)', method_body(M, "private void Bound")))
    return len(kept) >= 70 and shown == kept

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
    found = []
    for t, m, kind in re.findall(
            r'\[HarmonyPatch\(typeof\((\w+)\), "(\w+)"(?:,\s*MethodType\.(\w+))?.*?\)\]', ALL, re.S):
        name = 'get_' + m if kind == 'Getter' else 'set_' + m if kind == 'Setter' else m
        found.append(t + '.' + name)
    return sorted(found)

def every_declared_patch_is_installed():
    declared = sorted(re.findall(
        r'\[HarmonyPatch\(typeof\(\w+\), "\w+"(?:,\s*MethodType\.\w+)?.*?\)\]\s*'
        r'internal static class (\w+)', ALL, re.S))
    installed = sorted(re.findall(
        r'Patcher\.TryPatch\(harmony, typeof\((\w+)\)\)', S['SubModule.cs']))
    return len(declared) > 0 and declared == installed and len(harmony_targets()) == len(declared)

def compat_list(name):
    m = re.search(re.escape(name) + r'\s*=\s*\{(.*?)\};', COMPAT, re.S)
    return None if m is None else sorted(
        t.split('.')[-1] + '.' + member for t, member in
        re.findall(r'\(\s*(?:(?:Inventory|Issues) \+ )?"([\w.+]+)"\s*,\s*"(\w+)"\s*(?:,[^)]*)?\)',
                   m.group(1)))

def compat_checks_every_game_hook():
    reflected = sorted({t.split('.')[-1] + '.' + m for t, m in
                        re.findall(r'typeof\((\w+)\)\.GetMethod\(\s*"(\w+)"', ALL)})
    fields = sorted({'_' + n for n in re.findall(r'____(\w+)', ALL)}
                   | set(re.findall(r'"(_\w+)"', ALL)))
    compat_fields = sorted({p.split('.')[-1] for p in (compat_list('ReflectedFields') or [])})
    return (len(reflected) > 0 and len(fields) > 0
            and compat_list('HarmonyTargets') == harmony_targets()
            and compat_list('ReflectedMethods') == reflected
            and compat_fields == fields)

def refusal_reasons_are_named():
    promised = {'Locked', 'CategoryPolicy', 'FoodReserve', 'BelowMargin',
                'MerchantTillEmpty', 'BudgetSpent', 'CarryWeight', 'HerdFull'}
    phrase = method_body(S['Reasons.cs'], 'internal static TextObject Phrase')
    return promised <= set(re.findall(r'case Block\.(\w+):', phrase))

def projects_pin_one_reference_assembly():
    used = set(re.findall(r'"Bannerlord\.ReferenceAssemblies" Version="([0-9.]+)"', "\n".join(PROJ)))
    return len(PROJ) == 2 and len(used) == 1

def the_readme_names_the_game_versions_the_mod_was_checked_against():
    used = set(re.findall(r'"Bannerlord\.ReferenceAssemblies" Version="([0-9.]+)"', "\n".join(PROJ)))
    if len(used) != 1:
        return False
    built = used.pop()
    also = GAME_VERSION_BETA
    supported = 'The mod is supported on ' + built + ' and ' + also
    return (README.count(built) >= 2
            and README.count(also) >= 2
            and 'Built on Bannerlord ' + built in README
            and README.count(supported) == 2)

def one_hard_dependency():
    required = re.findall(r'<DependedModuleMetadata id="([^"]+)" order="[^"]+" optional="false"/>',
                          MANIFEST)
    return required == ['Bannerlord.Harmony']

_lost = []

_masked = {}

def code_only(src):
    if src in _masked:
        return _masked[src]
    out, i, n = list(src), 0, len(src)

    def blank(start, stop):
        for k in range(start, stop):
            if out[k] != '\n':
                out[k] = ' '

    while i < n:
        c = src[i]
        if c == '"':
            if src.startswith('\"\"\"', i):
                j = src.find('\"\"\"', i + 3)
                j = n if j < 0 else j + 3
            else:
                j = i + 1
                while j < n and src[j] != '"':
                    j += 2 if src[j] == '\\' else 1
                j = min(j + 1, n)
            blank(i, j); i = j; continue
        if c in '@$' and i + 1 < n and src[i + 1] == '"':
            j, depth = i + 2, 0
            while j < n:
                if src[j] == '\\' and c == '$':
                    j += 2; continue
                if src[j] == '{' and c == '$':
                    depth += 1
                elif src[j] == '}' and c == '$' and depth:
                    depth -= 1
                elif src[j] == '"' and not depth:
                    if c == '@' and j + 1 < n and src[j + 1] == '"':
                        j += 2; continue
                    j += 1; break
                j += 1
            blank(i, j); i = j; continue
        if c == "'":
            j = i + 1
            while j < n and src[j] != "'":
                j += 2 if src[j] == '\\' else 1
            j = min(j + 1, n)
            blank(i, j); i = j; continue
        if src.startswith('//', i):
            j = src.find('\n', i)
            j = n if j < 0 else j
            blank(i, j); i = j; continue
        if src.startswith('/*', i):
            j = src.find('*/', i + 2)
            j = n if j < 0 else j + 2
            blank(i, j); i = j; continue
        i += 1
    _masked[src] = ''.join(out)
    return _masked[src]

def method_body(src, signature):
    i = src.find(signature)
    if i >= 0:
        code = code_only(src)
        j = code.find('{', i)
        if j >= 0:
            depth = 0
            for k in range(j, len(code)):
                if code[k] == '{': depth += 1
                elif code[k] == '}':
                    depth -= 1
                    if depth == 0: return src[i:k + 1]
    _lost.append(signature)
    return ''

SHARED_PASS_RULES = {
    "CheapestFirst(": ('Trading.cs',
        "private static List<(ItemRosterElement el, Good good, int price, int ceiling)> CheapestFirst"),
    "WhatStopsBuying(": ('Rules.cs', "internal static Block WhatStopsBuying"),
    "WhatCapsAGood(": ('Rules.cs', "internal static Block WhatCapsAGood"),
    "Basis.For(": ('Passes.cs', "internal struct Basis"),
    "basis.Unit(": ('Passes.cs', "internal struct Basis"),
    "TradeMath.SkipTheUnitsYouPaidFor(": ('TradeMath.cs',
        "public static bool SkipTheUnitsYouPaidFor"),
    "Pass.Open(": ('Trading.cs', "private sealed class Pass"),
    "new BuyingAt(": ('Trading.cs', "private sealed class BuyingAt"),
    "new SellingFrom(": ('Trading.cs', "private sealed class SellingFrom"),
    "TradePass.WhatToBuy(": ('Passes.cs', "internal static List<Pick> WhatToBuy"),
    "TradePass.BuyThem(": ('Passes.cs', "internal static Traded BuyThem"),
    "TradePass.SellThem(": ('Passes.cs', "internal static Traded SellThem"),
}

def rank_rule():
    return method_body(S['Rules.cs'], "internal static int HerdShedRank")

def food_rule():
    return method_body(S['Rules.cs'], "internal static Dictionary<string, int> FoodKeep")

def sell_rule():
    return method_body(S['Rules.cs'], "internal static SellVerdict MaySell<TGame>")

def buy_rule():
    return method_body(S['Rules.cs'], "internal static bool MayBuy<TGame>")

def haul_rule():
    return method_body(S['Rules.cs'], "internal static bool MayHaul<TGame>")

def shed_rule():
    return method_body(S['Rules.cs'], "internal static bool MayShedForHerd<TGame>")

def cap_rule():
    return (method_body(S['Rules.cs'], "internal static Block WhatStopsBuying")
            + "\n" + method_body(S['Rules.cs'], "internal static Block WhatCapsAGood"))

def pass_body(signature):
    body = method_body(S['Trading.cs'], signature)
    read = set()
    while True:
        grew = False
        for call, (where, declared) in SHARED_PASS_RULES.items():
            if call in body and declared not in read:
                read.add(declared)
                body += "\n" + method_body(S[where], declared)
                grew = True
        if not grew:
            return body

def buy_pass():
    return pass_body("private static void BuyPass")

def sell_pass():
    return pass_body("private static void SellPass")

def between(body, opening, closing):
    i = body.find(opening)
    if i < 0:
        _lost.append(opening)
        return ''
    j = body.find(closing, i + len(opening))
    if j < 0:
        _lost.append(closing)
        return ''
    return body[i + len(opening):j]

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

def every_setting_keeps_to_its_own_value():
    named = re.findall(r'public\s+(bool|int|float|string)\s+(\w+)\s*\{', M)
    if len(named) < 40:
        return False
    for kind, name in named:
        body = method_body(M, "public " + kind + " " + name)
        held = re.findall(r'_o\.(\w+)', body)
        if len(held) != 2 or any(one != name for one in held):
            return False
        if "Options.Bump();" not in body or "Options.Current" in body:
            return False
    return "Options.Bump();" in M

def log_prefers_the_user_folder():
    body = method_body(S['Support.cs'], "private static List<string> Candidates")
    docs = body.find('"Mount and Blade II Bannerlord"')
    own = body.find("Assembly.Location")
    cwd = body.rfind("paths.Add(fileName);")
    return (-1 < docs < own < cwd
            and body.count("catch { }") == 2
            and "yield" not in body)

def the_feature_list_calls_a_setting_what_the_settings_screen_calls_it():
    en = spoken(ENGLISH)
    live = en.get('TL201', '').replace(' (default)', '')
    return (live != ''
            and 'With ' + live + ' turned off it shows yours instead' in README
            and '- \u2705 ' + live + ', on out of the box; turn it off and TradeLord uses only the '
                'prices you have seen in person' in README
            and not re.search(r'honest[- ]merchant', README, re.I))

def the_feature_list_says_a_pin_comes_off_a_town_it_traded_in():
    return ('and a pin comes off by itself once TradeLord has traded in that town' in README
            and 'LedgerPanel.Unpin(Site)' in S['Trading.cs'])

def the_selling_rules_stand_clear_of_the_game():
    rules = S['Rules.cs']
    described = between(rules, "internal struct Good", "\n    }")
    fields = re.findall(r'internal (?:string|float|int|bool) (\w+);', described)
    describe = method_body(S['Policy.cs'], "internal static Good Describe")
    return ("TaleWorlds" not in rules
            and rules.count("using ") == 2
            and "using System;" in rules
            and "using System.Collections.Generic;" in rules
            and "Options.Current" not in rules
            and '<Compile Include="..\\src\\Rules.cs" Link="Rules.cs"/>' in TESTPROJ
            and len(fields) >= 15
            and all("good." + one + " =" in describe for one in fields)
            and "where TGame : struct, IWhatTheGameSays" in rules
            and all(said in rules for said in
                    ("bool Locked();", "bool Smeltable();", "bool PartsAllLearned();"))
            and all(asked in sell_rule() for asked in
                    ("game.Locked()", "game.Smeltable()", "game.PartsAllLearned()"))
            and not any(costly in describe for costly in
                        ("IsSmeltable", "PartsAllLearned", "IsLocked"))
            and "if (AnyListNamesAGood(Options.Current))" in describe
            and "public bool Locked() => IsLocked(Locks, What);" in S['Policy.cs'])

def the_food_reserve_is_worked_out_where_a_test_can_ask_it():
    t = S['Trading.cs']
    keep = method_body(S['Policy.cs'], "internal static Dictionary<ItemObject, int> KeptBack")
    return ("internal static Dictionary<string, int> FoodKeep(List<Ration> carried, float perDay, Options s)"
                in S['Rules.cs']
            and "MobileParty" not in S['Rules.cs']
            and "ItemRoster" not in S['Rules.cs']
            and ordered(keep,
                        "List<TradeRules.Ration> carried = Carried(roster, books, sim, byId);",
                        "Named(TradeRules.FoodKeep(carried, AppetitePerDay(), Options.Current), byId);")
            and "byId[item.StringId] = item;" in
                method_body(S['Policy.cs'], "private static List<TradeRules.Ration> Carried")
            and "if (byId.TryGetValue(one.Key, out ItemObject item)) keep[item] = one.Value;" in
                method_body(S['Policy.cs'], "private static Dictionary<ItemObject, int> Named")
            and S['Policy.cs'].count("AppetitePerDay()") == 3
            and "internal static int FoodValue(ItemObject item) =>" in S['Policy.cs']
            and "TradeRules.FoodValue(Describe(item));" in S['Policy.cs']
            and "CostPerFood" not in t
            and "int fed = TradeRules.FoodValue(good);" in
                method_body(t, "public static void ExecuteResupply")
            and "TradeRules.FoodValue(good)" in method_body(t, "public static void ExecuteHaulage")
            and "The_reserve_reaches_past_the_biggest_helping_to_every_other_one" in FOODTESTS
            and "The_variety_floor_counts_every_helping_of_a_kind_together" in FOODTESTS
            and "The_reserve_holds_no_more_than_you_carry_and_no_less_than_it_asked_for" in FOODTESTS
            and "Asking_for_another_day_of_food_never_holds_back_less_of_it" in FOODTESTS
            and "new Random(1447)" in FOODTESTS
            and ("if (at.TryGetValue(held.Good.Id, out int seen))"
                 in method_body(S['Rules.cs'],
                                "internal static Dictionary<string, int> FoodKeep")))

def a_good_you_bought_by_hand_is_never_counted_beyond_what_you_carry():
    body = method_body(S['Ledger.cs'], "private void OnPlayerInventoryExchange")
    return ("ItemRoster carried = MobileParty.MainParty?.ItemRoster;" in body
            and "int bought = Deals.UnitsMoved(element.Amount, said, unit);" in body
            and "int took = Math.Min(bought, InAll(carried, item));" in body
            and "if (took <= 0) continue;" in body
            and ordered(body, "int bought = Deals.UnitsMoved(", "int took = Math.Min(",
                        "if (took <= 0) continue;",
                        "Bulk.PricePaid(here, element.EquipmentElement, took, unit)")
            and ordered(body, "int gone = Deals.UnitsMoved(element.Amount, said, unit);",
                        "RecordSale(item.StringId, gone);")
            and "RecordSale(item.StringId, count);" not in body
            and "RecordPurchase(item.StringId, count," not in body
            and "element.EquipmentElement, count, unit" not in body)

def a_road_trade_that_moved_nothing_says_why():
    road = method_body(S['Trading.cs'], "public static void ExecuteRoadTrade")
    passes = [method_body(S['Trading.cs'], one)
              for one in ("private static void SellPass", "private static void BuyPass")]
    stalled = [one.split("else if (!pass.DirectionError)", 1)[-1] for one in passes]
    return (ordered(road, "SellPass(Pass.Meet(", "BuyPass(Pass.Meet(", "ReportStalledPasses();")
            and road.count("ReportStalledPasses();") == 1
            and all("else if (!pass.DirectionError)" in one for one in passes)
            and all("NoteStalled(" in one and "pass.Site" not in one and "pass.Reports" not in one
                    for one in stalled))

def the_ledger_lists_a_route_you_could_not_take_this_second():
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes()")
    size = method_body(S['Ledger.cs'], "private static int MostWorthShowing")
    return ("purse" not in scan
            and "herdRoom" not in scan
            and "Carry." not in S['Ledger.cs']
            and "Capacity" not in S['Ledger.cs']
            and "HerdRoomForLivestock" not in S['Ledger.cs']
            and "Options.Current.BuyValueCapPerItem" in size
            and "Options.Current.BuyCapPerItem" in size)

def the_best_markets_are_picked_without_sorting_every_town():
    l = S['Ledger.cs']
    rerank = method_body(l, "private static List<(Settlement, int)> Rerank")
    prime = method_body(l, "private void PrimeLiveRankings(List<ItemObject> wanted, int hour)")
    keep = method_body(S['Ranking.cs'], "internal static void Keep<T>")
    return ("all.Sort(" not in l
            and "if (selling) kept.Add(one); else MarketRank.Keep(kept, one, false);" in rerank
            and "Where = all[i].s, Price = all[i].price, Straight = all[i].days, Days = days" in rerank
            and "return Settled(kept, selling);" in rerank
            and "MarketRank.Keep(buys[i], new Reach<Settlement>" in prime
            and "if (kept.Count == TopCacheSize &&" in keep
            and "kept[TopCacheSize - 1].Price, kept[TopCacheSize - 1].Straight) >= 0) return;" in keep
            and "if (kept.Count > TopCacheSize) kept.RemoveAt(TopCacheSize);" in keep
            and "Only_eight_markets_are_ever_kept" in RANKTESTS
            and "A_market_worse_than_the_eight_already_kept_is_turned_away" in RANKTESTS
            and "The_eight_are_chosen_on_the_straight_line_and_ordered_on_the_real_ride" in RANKTESTS
            and "The_eight_it_keeps_are_the_eight_a_full_sort_would_have_picked" in RANKTESTS)


def every_market_a_good_could_be_sold_at_is_offered_to_the_route_scan():
    l = S['Ledger.cs']
    rerank = method_body(l, "private static List<(Settlement, int)> Rerank")
    prime = method_body(l, "private void PrimeLiveRankings(List<ItemObject> wanted, int hour)")
    scan = method_body(l, "private List<TradeRoute> ScanRoutes()")
    return ("if (selling) kept.Add(one); else MarketRank.Keep(kept, one, false);" in rerank
            and "sells[i].Add(new Reach<Settlement>" in prime
            and "MarketRank.Keep(sells[i]" not in prime
            and "public List<(Settlement town, int price)> EverySell(ItemObject item) =>\n"
                "            TakeN(TopMarkets(item, true), int.MaxValue);" in l
            and "var sells = EverySell(item);" in scan
            and "var buys = TopBuy(item, MarketRank.TopCacheSize);" in scan
            and "ledger.TopSell(item, MarketRank.TopCacheSize);" in S['TooltipPatches.cs']
            and "var markets = EverySell(item);" in
                method_body(l, "internal (Settlement town, int price, Ladder rungs) WhereThisEarnsFastest")
            and ordered(scan, "float ceiling = (float)(openingSell - openingBuy) * qtyCap;",
                        "if (best != null && TradeMath.PerDay(ceiling, days) <= bestKey)",
                        "RouteQuote q = Bulk.Walk("))

def a_route_scan_prices_each_town_once_for_every_good_it_wants():
    l = S['Ledger.cs']
    prime = method_body(l, "private void PrimeLiveRankings(List<ItemObject> wanted, int hour)")
    scan = method_body(l, "private List<TradeRoute> ScanRoutes()")
    return ("if (!Options.Current.Omniscient || wanted.Count == 0) return;" in prime
            and prime.find("if (!WithinTravelCeiling(town, days)) continue;") <
                prime.find("Priced.At(market, item, me, true)")
            and "_marketCache[(item.StringId, true)] = (Freshness.At(hour), kind, Settled(sells[i], true));" in prime
            and "_marketCache[(item.StringId, false)] = (Freshness.At(hour), kind, Settled(buys[i], false));" in prime
            and "PrimeLiveRankings(wanted, (int)CampaignTime.Now.ToHours);" in scan
            and scan.find("wanted.Add(item);") < scan.find("PrimeLiveRankings(wanted,")
            and scan.find("PrimeLiveRankings(wanted,") < scan.find("var buys = TopBuy(item, MarketRank.TopCacheSize);"))

def a_language_file_that_could_not_be_read_is_tried_again():
    said = method_body(S['Tongue.cs'], "private static bool Ready")
    failed = said.find("if (read == null)")
    settled = said.find("_saidFor = language;")
    return (0 <= failed < settled
            and "if (_tryingAgainAt > DateTime.UtcNow) return false;" in said
            and "_tryingAgainAt = DateTime.UtcNow + BeforeTryingAgain;" in said
            and "_tryingAgainAt = default(DateTime);" in said
            and "if (_toldItFailedFor != language)" in said
            and "_toldItFailedFor = English - 1;" in said
            and "Guard.Read(\"Tongue.Read\", language, Reading, null)" in said
            and "() =>" not in said
            and "private static Dictionary<string, string> Reading(int language) => Read(Where(language));"
                in S['Tongue.cs']
            and S['Tongue.cs'].count("Ready(") == 3)

def a_price_is_found_by_its_town_rather_than_by_looking_down_the_list():
    led = S['Ledger.cs']
    record = method_body(led, "private void Record")
    aged = method_body(led, "public float ObservationAgeDays")
    return ("private Dictionary<string, Dictionary<string, PriceObservation>> _ledger =" in led
            and "byTown.TryGetValue(townId, out PriceObservation seen)" in record
            and "for (int i = 0" not in record
            and "byTown.TryGetValue(town.StringId, out PriceObservation seen)" in aged
            and "for (int i = 0" not in aged
            and "public static string WriteLedger(Dictionary<string, List<PriceObservation>> ledger)"
                in S['LedgerCodec.cs']
            and "public static Dictionary<string, List<PriceObservation>> ReadLedger(string text) =>"
                in S['LedgerCodec.cs']
            and "LedgerCodec.WriteLedger(Listed(_ledger))" in led
            and "KeyedByTown(LedgerCodec.ReadLedger(_ledgerText, out _unreadable))" in led)

def the_screen_is_asked_for_again_until_mcm_hands_it_over():
    ask = method_body(S['Support.cs'], "internal static void TryHandover")
    tick = method_body(S['SubModule.cs'], "protected override void OnApplicationTick")
    return ("if (SettingsInHand || _handover == null) return;" in ask
            and "if (DateTime.UtcNow - _askedAt < BetweenAsks) return;" in ask
            and "SettingsInHand = true;" in ask
            and ordered(ask, "_handover.Invoke(null, null)", "SettingsInHand = true;", "Log.Write(")
            and "_handover = init;" in method_body(S['Support.cs'], "internal static void TryLoad")
            and 'Guard.Run("Tick.Mcm", McmLoader.TryHandover);' in tick
            and S['Support.cs'].count("SettingsInHand = ") == 2)

def the_sell_pass_describes_a_good_once_and_hands_it_on():
    t = S['Trading.cs']
    sell = sell_pass()
    return ("public Good GoodAt(int at) => TradePolicy.Describe(Item(at));" in sell
            and sell.count("Describe(") == 1
            and "TradePolicy.MaySell(good, _plan[at], _pass.Locked, _keepBack, _awaited," in sell
            and "int herdRank = TradeRules.HerdShedRank(good);" in sell
            and "Drove.ShedRank(item)" not in sell
            and "internal static int HerdShedRank(in Good good)" not in t
            and "internal static string AnimalGroup(ItemObject item) =>" in S['Policy.cs']
            and "TradeRules.AnimalGroup(Describe(item));" in S['Policy.cs'])

def the_buying_rules_stand_clear_of_the_game_too():
    t = S['Trading.cs']
    buy = buy_pass()
    return ("ItemObject" not in S['Rules.cs']
            and all("in Good good" in body for body in
                    (buy_rule(), haul_rule(), shed_rule(), cap_rule()))
            and "public Good GoodAt(int at) => TradePolicy.Describe(Item(at));" in buy
            and buy.count("Describe(") == 1
            and "good.IsGrain = item == DefaultItems.Grain;" in
                method_body(S['Policy.cs'], "internal static Good Describe")
            and "TradeRules.MayBuy(good, toFeed, Options.Current," in S['Policy.cs']
            and "TradeRules.MayHaul(Describe(item), Options.Current," in S['Policy.cs']
            and "TradeRules.MayShedForHerd(Describe(held.Item), held.IsQuestItem, Options.Current," in S['Policy.cs']
            and "TradeRules.WhatStopsBuying(good, price, market.Spendable()," in S['Passes.cs']
            and "TradeRules.NoRoomForOneMore(good, roomLeft);" in t
            and "TradeRules.NoRoomForOneMore(good, market.Room() - simWeight)" in S['Passes.cs'])

def the_tooltip_patches_hand_their_state_over_instead_of_capturing_it():
    g = S['Support.cs']
    t = S['TooltipPatches.cs']
    return ("internal static void Run<T>(string context, T with, Action<T> action)" in g
            and "internal static TAnswer Read<T, TAnswer>(string context, T with, "
                "Func<T, TAnswer> read, TAnswer ifItFails)" in g
            and 'Guard.Run("Tooltip.RefreshItemTooltips", (__instance, item), TooltipHelper.Append);' in t
            and 'Guard.Run("Tooltip.ProfitColoring", __instance, Coloured);' in t
            and 'Guard.Read("Tooltip.HasSection", itemVm, Sectioned, false)' in t
            and "private static void Coloured(SPItemVM shown)" in t
            and "private static bool Sectioned(ItemVM itemVm)" in t
            and "() =>" not in t)

def which_market_is_best_is_worked_out_where_a_test_can_ask_it():
    r = S['Ranking.cs']
    return ("TaleWorlds" not in r and "Settlement" not in r and "ItemObject" not in r
            and "internal struct Reach<T>" in r
            and "internal static class MarketRank" in r
            and "Options s" in r and "Options.Current" not in r
            and 'Ranking.cs' in TESTPROJ
            and "Reach<Settlement>" in S['Ledger.cs']
            and "MarketRank.TopCacheSize" in S['Ledger.cs']
            and "Two_markets_at_the_same_price_are_split_by_the_nearer_one" in RANKTESTS
            and "A_village_is_held_to_its_own_travel_ceiling_when_that_is_the_shorter_one" in RANKTESTS
            and "A_ceiling_of_zero_looks_as_far_as_it_likes" in RANKTESTS)

def what_is_on_the_road_is_added_up_where_a_test_can_ask_it():
    p = S['Projection.cs']
    return ("TaleWorlds" not in p and "Settlement" not in p and "ItemObject" not in p
            and "ItemCategory" not in p and "Options.Current" not in p
            and "internal struct Landing" in p and "internal struct Spending" in p
            and "internal static class Projection" in p
            and "pull != null && across > 0f;" in p
            and 'Projection.cs' in TESTPROJ
            and "Landing" not in S['Forecast.cs'].split("namespace TradeLord")[0]
            and "Projection.UnitsLanding(Read(site), item.StringId, withinDays);" in S['Forecast.cs']
            and "Projection.WorthLanding(Read(site), item.ItemCategory.StringId, withinDays);"
                in S['Forecast.cs']
            and "Units_landing_add_up_only_for_the_good_asked_about" in PROJECTIONTESTS
            and "An_empty_purse_leaves_nothing_whatever_the_pull_says" in PROJECTIONTESTS
            and "A_purse_split_across_a_market_never_hands_out_more_than_the_purse" in PROJECTIONTESTS)

def how_a_forecast_and_a_promise_held_is_scored_where_a_test_can_ask_it():
    c = S['Scoring.cs']
    return ("TaleWorlds" not in c and "Settlement" not in c and "ItemObject" not in c
            and "Options.Current" not in c
            and "internal class Keeps<TRecord>" in c
            and "internal class BandTally" in c
            and "int band = TradeMath.BandOf(confidence);" in method_body(c, "internal void Add")
            and "internal static class Scoring" in c
            and 'Scoring.cs' in TESTPROJ
            and "Keeps<Said>" in S['Hindsight.cs'] and "Keeps<Promised>" in S['Hindsight.cs']
            and "Scoring.NoWorth" in S['Hindsight.cs']
            and "private const int Most" not in S['Hindsight.cs']
            and "A_promise_is_kept_once_however_often_the_panel_repeats_it" in SCORINGTESTS
            and "Fewer_units_landing_than_it_said_reads_as_fewer" in SCORINGTESTS
            and "Shares_and_figures_read_the_same_whatever_the_player_s_own_numbers_look_like"
                in SCORINGTESTS)

def the_log_reads_its_figures_the_same_way_in_every_language():
    c = S['Scoring.cs']
    return ('internal static string Share(float share) =>' in c
            and '(share * 100f).ToString("0", CultureInfo.InvariantCulture) + "%";' in c
            and 'internal static string Figure(float number) =>' in c
            and 'number.ToString("0.0", CultureInfo.InvariantCulture);' in c
            and c.count('CultureInfo.InvariantCulture') == 2
            and 'CultureInfo' not in S['Hindsight.cs']
            and 'NumberDecimalSeparator = ","' in SCORINGTESTS)

def a_market_ranking_sorts_through_one_comparison_for_each_way():
    r = S['Ranking.cs']
    settled = method_body(r, "internal static List<Reach<T>> Settled<T>")
    return ("internal static readonly Comparison<Reach<T>> DearestFirst" in r
            and "internal static readonly Comparison<Reach<T>> CheapestFirst" in r
            and "top.Sort(selling ? Order<T>.DearestFirst : Order<T>.CheapestFirst);" in settled
            and "Sort((x, y) => Rank(" not in r + S['Ledger.cs']
            and r.count("top.Sort(") == 1
            and S['Ledger.cs'].count(".Sort(") == 2
            and "held.Sort((x, y) => y.Scored.CompareTo(x.Scored));" in
                method_body(S['Ledger.cs'], "private void TrimThePromisesKept")
            and "routes.Sort((x, y) => rankByScore" in
                method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes"))

def a_good_on_the_shelf_is_asked_the_buying_questions_once():
    t = S['Trading.cs']
    buy = buy_pass()
    round_trip = method_body(S['Policy.cs'], "internal static bool MayRoundTrip")
    resale = between(S['Rules.cs'], "internal static bool ResaleAllowed", ";")
    return (buy.count("TradePolicy.MayBuy(") == 1
            and "MayRoundTrip" not in buy
            and "MayBuy(good, item, lockedKeys, out _) &&" in round_trip
            and "MayBuy" not in resale
            and "if (!TradePolicy.MayRoundTrip(item, locked)) continue;" in S['Ledger.cs'])

def the_panel_reads_the_key_before_it_walks_the_screen():
    tick = method_body(S['Panel.cs'], "private static void TickCore")
    return (tick.count("HotkeyReleased() && !TypingOnScreen(map)") == 2
            and "TypingOnScreen(map) && HotkeyReleased()" not in tick
            and ordered(tick, "if (!map.IsEscapeMenuOpened && HotkeyReleased() && !TypingOnScreen(map))",
                        "else if (map.IsEscapeMenuOpened || (HotkeyReleased() && !TypingOnScreen(map)))"))

def the_log_is_held_open_and_pushed_out_a_line_at_a_time():
    put = method_body(S['Support.cs'], "private static void Put(string message)")
    held = method_body(S['Support.cs'], "private static StreamWriter Held")
    letgo = method_body(S['Support.cs'], "private static void LetGo")
    resolve = method_body(S['Support.cs'], "private static string Resolve")
    write = method_body(S['Support.cs'], "internal static void Write")
    many = method_body(S['Support.cs'], "internal static void WriteMany")
    pushed = method_body(S['Support.cs'], "private static void Pushed")
    return ("FileMode.Append, FileAccess.Write, FileShare.ReadWrite" in held
            and "{ AutoFlush = false };" in held
            and "if (_open != null || DateTime.UtcNow < _holdAgainAt) return _open;" in held
            and "catch { LetGo(); }" in held
            and "_holdAgainAt = DateTime.UtcNow + BeforeHoldingAgain;" in letgo
            and "_open = null;" in letgo
            and ordered(put, "StreamWriter held = Held();",
                        "try { held.WriteLine(line); return; }",
                        "catch { LetGo(); }",
                        "File.AppendAllText(_path, line + Environment.NewLine);")
            and ordered(write, "Put(message);", "Pushed();")
            and write.count("Pushed();") == 1
            and ordered(many, "for (int i = 0; i < messages.Count; i++) Put(messages[i]);",
                        "Pushed();")
            and many.count("Pushed();") == 1
            and ordered(pushed, "StreamWriter held = _open;", "try { held.Flush(); }",
                        "catch { LetGo(); }")
            and 'File.AppendAllText(candidate, "");' in resolve
            and S['Support.cs'].count("File.AppendAllText(") == 2)

def the_marker_skips_a_town_that_cannot_outpay_the_best_one_yet():
    marker = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    fetch = method_body(S['Marker.cs'], "private static Takings WhatItWouldFetch")
    return (ordered(marker, "reachable.Sort(FastestPurseFirst);",
                    "TheMarkedTownFirst(reachable);",
                    "float bar = 0f;",
                    "if (TradeMath.PerDay(gold, ride) <= bar) break;",
                    "Takings took = WhatItWouldFetch(s, market, party, cargo, gold, null);",
                    "long total = took.Value > gold ? gold : took.Value;",
                    "long earned = total - took.Cost;",
                    "float rate = TradeMath.PerDay(earned, ride);",
                    "float weighed = TradeMath.RateTheMarkHolds(rate, s == _picked);",
                    "if (weighed > bar)")
            and ordered(fetch, "foreach (var (item, amount, worth, floor) in cargo)",
                        "Paying pays = WhatThatMarketPays(site, market, item, party);",
                        "for (int u = 0; u < amount; u++)",
                        "int price = pays.At(u);",
                        "if (took.Value + fetched >= gold) { took.PurseCapped = true; break; }")
            and "float faster = TradeMath.PerDay(x.gold, x.days);" in S['Marker.cs']
            and "return faster != slower ? slower.CompareTo(faster)" in S['Marker.cs']
            and "string.CompareOrdinal(x.s.StringId, y.s.StringId);" in S['Marker.cs']
            and fetch.count("WhatThatMarketPays(") == 1
            and "WhatThatMarketPays(" not in marker
            and "Priced.At(" not in marker
            and "Priced.At(" not in fetch
            and method_body(S['Marker.cs'], "private int Next()")
                   .count("Priced.At(_market, _el, _party, true)") == 1)


def the_marker_counts_only_what_the_selling_rules_would_really_move():
    marker = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    carried = method_body(S['Marker.cs'],
                          "private static List<(EquipmentElement item, int amount, int worth, int floor)> "
                          "WhatYouCarryToSell")
    floor = method_body(S['Marker.cs'], "private static int BestMarketFloor")
    worth = method_body(S['Policy.cs'], "internal static int WorthToBeat(ItemObject item)")
    return ("TradePolicy.WorthToBeat(item), BestMarketFloor(el.EquipmentElement)));" in carried
            and "var cargo = WhatYouCarryToSell(party);" in marker
            and "int paid = CostBasis(item);" in worth
            and "TradeRules.WorthIsWhatYouPaid(good, paid)" in worth
            and "? paid" in worth
            and ": TradeRules.WorthToBeat(good, paid, UnpaidWorth(item));" in worth
            and worth.count("UnpaidWorth(item)") == 1
            and ordered(method_body(S['Marker.cs'], "private static Takings WhatItWouldFetch"),
                        "Paying pays = WhatThatMarketPays(site, market, item, party);",
                        "int price = pays.At(u);",
                        "if (price < floor) break;",
                        "if (!TradeMath.ProfitAcceptable(worth, price, Options.Current.MinProfitMargin)) break;",
                        "took.Value += fetched;")
            and "if (took.Value <= 0L) { how.Refused++; continue; }" in marker
            and ordered(floor, "if (!Options.Current.PreferBestSellTown) return 0;",
                        "LedgerBehavior.Instance?.BestSell(held.Item)",
                        "TradeRules.BestMarketFloor(",
                        "Options.Current.BestSellTownTolerance);")
            and S['Trading.cs'].count("TradePolicy.WorthToBeat(") == 2
            and S['Marker.cs'].count("TradePolicy.WorthToBeat(") == 1)


def the_marker_says_in_the_log_which_town_it_picked_and_why():
    track = method_body(S['Marker.cs'], "internal static void Update")
    marker = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    said = (method_body(S['Marker.cs'], "private static string Why") + "\n" +
            method_body(S['Marker.cs'], "private static string TheNextBest"))
    return (ordered(track, "if (on) target = BestSellTownForCargo(out how);",
                    "if (target == _picked)",
                    'string why = on ? Why(how) : "the map marker is switched off";',
                    'Log.Write(target != null')
            and '"map marker moved to " + target.Name + ": " + why' in track
            and '"map marker taken off the map: " + why' in track
            and "Why(" not in marker
            and 'return "nothing in your cargo is yours to sell";' in said
            and all(one in said for one in
                    ("clear Minimum profit margin", "against a town purse of",
                     "day(s) away", "ahead of ", "past your travel ceilings",
                     "which is all that town's purse of"))
            and "how.Units + \" unit(s) for \" + how.Value" in said)

def a_traded_market_drops_only_the_rankings_its_own_prices_decide():
    ledger = S['Ledger.cs']
    t = S['Trading.cs']
    capture = method_body(ledger,
        "public void CaptureSettlement(Settlement settlement, bool force, ISet<string> moved)")
    drop = method_body(ledger, "private void DropRankings(Settlement settlement, ISet<string> moved)")
    kinds = method_body(t, "private ISet<string> KindsMoved")
    return ("public void CaptureSettlement(Settlement settlement, bool force = false) =>\n"
            "            CaptureSettlement(settlement, force, null);" in ledger
            and "DropRankings(settlement, Options.Current.Omniscient ? moved : null);" in capture
            and "if (moved == null || moved.Count == 0 || !TillStillOpen(settlement))" in drop
            and "ForgetPricedRankings();" in drop
            and "if (kv.Value.kind == null || moved.Contains(kv.Value.kind)) spent.Add(kv.Key);" in drop
            and "_routes = null;" in drop
            and "TradeRules.WhatTheTillCanPay(s?.SettlementComponent?.Gold ?? 0," in
                between(ledger, "private static bool TillStillOpen", ";")
            and "s != null && s.IsVillage) > 0" in
                between(ledger, "private static bool TillStillOpen", ";")
            and "item?.ItemCategory?.StringId" in
                between(ledger, "internal static string KindOf", ";")
            and "_marketCache[key] = (Freshness.At(hour), KindOf(item), result);" in
                method_body(ledger, "private List<(Settlement, int)> TopMarkets")
            and "if (kind == null) return null;" in kinds
            and "LedgerBehavior.KindOf(kv.Key)" in kinds)

def capture_skipped_after_the_caches_are_dropped():
    ledger = S['Ledger.cs']
    body = method_body(ledger,
        "public void CaptureSettlement(Settlement settlement, bool force, ISet<string> moved)")
    dropped = ("if (force || !Options.Current.Omniscient)\n"
               "                DropRankings(settlement, Options.Current.Omniscient ? moved : null);")
    if dropped not in body or "if (Options.Current.Omniscient) return;" not in body:
        return False
    return (ordered(body, dropped, "if (Options.Current.Omniscient) return;")
            and body.count("DropRankings(") == 1
            and "Options.Current.Omniscient" in method_body(ledger,
                    "private List<(Settlement, int)> TopMarkets")
            and "CaptureSettlement(Site, force: true, KindsMoved());" in
                method_body(S['Trading.cs'], "internal void Moved"))

def hotkey_fallback_is_reported():
    body = method_body(S['Panel.cs'], "internal static InputKey PanelKey")
    if 'Log.Write("panel hotkey' not in body or "if (!named)" not in body:
        return False
    return ("_key = InputKey.T;" in body and "named = true;" in body
            and ordered(body, "if (!named)", 'Log.Write("panel hotkey')
            and "if (stray != null)" in body
            and ordered(body, "else if (stray == null && parts[i].Trim().Length > 0) stray =",
                        "if (stray != null)"))

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
    return [b for b in re.split(r'\n\s*(?=\[SettingProperty(?:Bool|Integer|FloatingInteger|Text|Dropdown)\()', M)
            if re.match(r'\s*\[SettingProperty(?:Bool|Integer|FloatingInteger|Text|Dropdown)\(', b)]

def every_setting_has_a_hint():
    blocks = setting_blocks()
    return len(blocks) > 40 and all('HintText' in b for b in blocks)

def every_setting_line_is_translatable():
    lit = r'"((?:[^"\\]|\\.)*)"'
    names = re.findall(r'\[SettingProperty(?:Bool|Integer|FloatingInteger|Text|Dropdown|Button)\(' + lit, M)
    hints = re.findall(r'HintText = ' + lit, M)
    groups = re.findall(r'\[SettingPropertyGroup\(' + lit + r'[^\]]*\)\]', M)
    content = re.findall(r'Content = ' + lit, M)
    return (len(names) > 40 and len(names) == len(hints) == len(groups)
            and len(content) == len(re.findall(r'\[SettingPropertyButton\(', M)) >= 1
            and all(re.match(r'\{=TL\d+\}', text) for text in names + hints + groups + content))

def settings_declared_in_display_order():
    seen = {}
    for b in setting_blocks():
        order = re.search(r'Order\s*=\s*(\d+)', b)
        group = re.search(r'\[SettingPropertyGroup\("([^"]+)"[^\]]*\)\]', b)
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
                 if l.strip().startswith('if ') and 'release-notes.md' in l
                 and '[:space:]' in l), None)
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
COMPARISON = io.open('COMPARISON.md', encoding='utf-8').read()

def option_default(name):
    m = re.search(r'public\s+(?:bool|int|float|string)\s+' + name + r'\s*=\s*([^;]+);', S['Options.cs'])
    return None if m is None else m.group(1).strip()

def the_readme_counts_the_saved_values_right():
    types = saved_field_types()
    tally = {}
    for name in ('Ledger.cs', 'Trading.cs'):
        body = method_body(S[name], "public override void SyncData")
        for field in re.findall(r'dataStore\.SyncData\("[^"]+",\s*ref\s+(\w+)\)', body):
            tally[types.get(field)] = tally.get(types.get(field), 0) + 1
    words = {1: 'one', 2: 'two', 3: 'three', 4: 'four', 5: 'five', 6: 'six',
             7: 'seven', 8: 'eight', 9: 'nine', 10: 'ten'}
    numbers = tally.get('int', 0) + tally.get('long', 0) + tally.get('float', 0)
    counted = 'a number' if numbers == 1 else words.get(numbers, 'no') + ' numbers'
    switches = tally.get('bool', 0)
    marked = 'a switch' if switches == 1 else words.get(switches, 'no') + ' switches'
    said = ('All it puts in a save is ' + words.get(tally.get('string'), 'no') +
            ' strings, ' + counted + ', ' + marked + ' and a settlement reference')
    return (said in README and numbers == 7
            and tally.get('Settlement') == 1 and switches == 1)

def readme_defaults_match_the_shipped_ones():
    def on(name):
        return option_default(name) == 'true'
    spelled = {'1': 'one', '3': 'three', '5': 'five'}
    def said(name):
        held = option_default(name)
        return spelled.get(held, held)
    def counted(src, pattern):
        m = re.search(pattern, src)
        return spelled.get(m.group(1), m.group(1)) if m else ''
    tooltip = counted(S['TooltipPatches.cs'], r'private const int TopN = (\d+);')
    shops = counted(S['Panel.cs'], r'i < best\.Count && i < (\d+);')
    share = str(round(float(option_default('MaxHeldShare').rstrip('f')) * 100))
    claims = ['which ships at ' + share + '% so one cheap good cannot take your whole cargo',
              'hotkey **' + option_default('PanelKey').strip('"') + '**',
              'gold reserve of ' + option_default('GoldReserve') + ' denars',
              'back up to ' + said('KeepFoodDays') + ' days of supply',
              ("the days of your troops' wages you ask it to keep"
               if option_default('KeepWageDays') == '0'
               else said('KeepWageDays') + " days of your troops' wages"),
              'from tier ' + option_default('MaxLootTier') + ' out of the box',
              'The ' + tooltip + ' best places to sell and the ' + tooltip + ' cheapest to buy',
              'The ' + shops + ' workshops in Calradia',
              'raised to ' + option_default('MaxWorkshopsOwned') + ' out of the box']
    shipped = [('Enable extended debug logging', 'ExtendedDebugLogging'),
               ('Count what is on its way to a market', 'MarketForecast'),
               ('Live world prices', 'Omniscient'),
               ('Staged Trading', 'StagedTrading'),
               ('A settling delay', 'EconomySettlingDays'),
               ('Free passage past bandits', 'BanditFreePassage')]
    switched = [lead + ', ' + ('off' if option_default(name) in ('false', '0') else 'on') +
                ' out of the box' for lead, name in shipped]
    return (all(c in README for c in claims)
            and all(c in README for c in switched)
            and on('Omniscient') and on('AutoSellOnEntry') and on('AutoBuyOnEntry')
            and on('NeverBuyGrain') and on('TradeWithVillages')
            and on('ProtectSpecial') and on('RespectLocks') and on('ExcludeHostileTowns')
            and option_default('PreferBestSellTown') == 'false'
            and option_default('CraftingPolicy') == 'PolicyBuySell')

def every_text_variable_is_supplied():
    placeholders = set()
    for text in re.findall(LITERAL, ALL + "\n" + M):
        placeholders |= set(re.findall(r'\{([A-Z][A-Z0-9_]*)\}', unescape(text[1])))
    supplied = set(re.findall(r'"([A-Z][A-Z0-9_]*)"', ALL + "\n" + M))
    return len(placeholders) > 10 and not (placeholders - supplied)

def section_has_entries(head):
    body = CHANGES.split('## ' + head, 1)[1].split('\n## ', 1)[0]
    return any(line.startswith('- ') for line in body.split('\n'))

def changelog_opens_on_the_shipped_version():
    heads = [h.strip() for h in re.findall(r'^## (.+)$', CHANGES, re.M)]
    if not heads:
        return False
    if heads[0].lower() == 'unreleased':
        if not section_has_entries(heads[0]):
            return False
        heads = heads[1:]
    if not heads or heads[0] != module_version():
        return False
    return section_has_entries(heads[0])

PLAIN_SAVED_TYPES = {'string', 'int', 'long', 'bool', 'float', 'Settlement'}

def the_filter_is_armed_only_around_a_game_call_that_talks():
    t = S['Trading.cs']
    armed = re.findall(r'OpenTransaction\(\);\s*try \{ ([\w\.]+)\([^)]*\); \}\s*'
                       r'finally \{ CloseTransaction\(\);(?: _tradingWith = null;)?'
                       r'( ReportSilenced\(\);)? \}', t)
    return (t.count('OpenTransaction();') == len(armed) == 2
            and sorted(c for c, _ in armed) == ['SkillLevelingManager.OnTradeProfitMade', 'swap']
            and sorted(re.findall(r'\(Action\)\(\(\) => ([\w\.]+)\(', t)) ==
                ['SellItemsAction.Apply'] * 2
            and sorted(re.findall(r': \(\) => ([\w\.]+)\(', t)) ==
                ['HandOver', 'TakeDelivery']
            and sorted(re.findall(r'\bSwap\((?:true|false), (_\w+),', t)) ==
                ['_buyUnit', '_sellUnit']
            and t.count("SwapOneUnit(selling, swap, Site == null ? null : Shop, what, named, out gold)") == 1
            and 'InGameTransaction = true' not in t
            and 'if (!TradeActionBehavior.InGameTransaction) return true;' in t
            and 'AutomatedTradeInProgress' not in
                method_body(t, "internal static class Patch_SilenceChunkedTradeLines"))

def the_full_cargo_warning_waits_for_a_visit_that_traded_nothing():
    body = method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry")
    return ('if (TradedThisVisit()) return;' in body
            and ordered(body, 'if (TradedThisVisit()) return;', '!NoRoomToCarry()')
            and 'private static bool TradedThisVisit() => Visit.Traded(Simulating);'
                in S['Trading.cs']
            and (lambda b: "_sold.Count > 0 || _bought.Count > 0 ||" in b
                       and "(sim && (_drySold.Count > 0 || _dryBought.Count > 0))" in b)
                (between(S['Books.cs'], "internal bool Traded(bool sim) =>", ";")))

def the_trade_skill_gain_is_reported_in_one_line():
    body = method_body(S['Trading.cs'], "private static void CreditTradeSkill")
    return ('finally { CloseTransaction(); ReportSilenced(); }' in body
            and ordered(body,
                        'int before = Hero.MainHero.GetSkillValue(DefaultSkills.Trade);',
                        'OpenTransaction();',
                        'SkillLevelingManager.OnTradeProfitMade(Hero.MainHero, xp);',
                        'int now = Hero.MainHero.GetSkillValue(DefaultSkills.Trade);',
                        'Notices.Say(earned, Notices.Xp);')
            and '{=TL88}' in body and '{=TL81}' in body
            and 'earned.SetTextVariable("LEVEL", now);' in body
            and 'SkillLevelingManager' not in
                method_body(S['Trading.cs'], "internal static void FlushToasts"))

def a_zero_cap_never_means_buy_nothing():
    return ("if (s.BuyCapPerItem > 0 && taken.count >= s.BuyCapPerItem) return Block.ItemCountCap;"
                in cap_rule()
            and "WhatStopsBuying(" in buy_pass()
            and "Options.Current.BuyCapPerItem > 0\n                ? Options.Current.BuyCapPerItem : UncappedBuyProjection;"
                in method_body(S['Ledger.cs'], "private static int MostWorthShowing")
            and "private const int UncappedBuyProjection" in S['Ledger.cs'])

def every_numeric_setting_that_switches_off_at_zero_says_so():
    off = {'TL206': 'Town travel ceiling', 'TL425': 'Gold before it buys a haul animal',
           'TL207': 'Village travel ceiling', 'TL228': 'Sell loot up to tier',
           'TL235': 'Buy cap per item (count', 'TL236': 'Buy cap per item (denars',
           'TL237': 'Max spend per visit', 'TL243': 'Economy settling delay'}
    for marker in off:
        label = re.search(r'\{=' + marker + r'\}([^"]*)"', M)
        if label is None or '0 = ' not in label.group(1):
            return False
    return True

def a_silent_pass_still_names_what_stopped_it():
    sell = sell_pass()
    buy = buy_pass()
    return ("if (!pass.Muted)" in sell and "if (!pass.Muted)" in buy
            and "if (!quiet)" not in sell and "if (!quiet)" not in buy
            and "TradeActionBehavior.Muted(Quiet)" in
                between(S['Trading.cs'], "internal bool Muted =>", ";")
            and "PurseHeldItBack" not in S['Trading.cs'])

def a_market_that_traded_nothing_is_reported_once():
    report = method_body(S['Trading.cs'], "private static void ReportStalledPasses")
    entered = method_body(S['Trading.cs'], "private void OnSettlementEntered")
    launched = method_body(S['Trading.cs'], "private void OnSessionLaunched")
    return (report.count("Notices.Say(none);") == 1
            and ordered(report, 'Tongue.Text("{=TL94}Nothing traded here - {REASON}.")',
                        'Tongue.Text("{=TL95}Nothing sold here - {REASON}, '
                        'and nothing bought - {SECOND}.")',
                        'Tongue.Text("{=TL32}', 'Tongue.Text("{=TL33}', 'Notices.Say(none);')
            and 'TL94' in strings_declared() and 'TL95' in strings_declared()
            and S['Trading.cs'].count("ReportStalledPasses();") == 3
            and ordered(entered, "ExecuteQuickSell(settlement, quiet: true)",
                        "ExecuteQuickBuy(settlement, quiet: true)", "ReportStalledPasses();")
            and ordered(launched, "ExecuteQuickSell(Settlement.CurrentSettlement);",
                        "ExecuteQuickBuy(Settlement.CurrentSettlement);", "ReportStalledPasses();"))

def a_market_that_traded_something_drops_the_empty_lines():
    report = method_body(S['Trading.cs'], "private static void ReportStalledPasses")
    sell = sell_pass()
    buy = buy_pass()
    reset = method_body(S['Trading.cs'], "private static void ResetVisit")
    return ("if (!sell.HasValue && !buy.HasValue) return;" in report
            and ordered(report, "Block? sell = _sellStalled;", "Block? buy = _buyStalled;",
                        "_sellStalled = null;", "_buyStalled = null;",
                        "if (!sell.HasValue && !buy.HasValue) return;")
            and "_runMovedGoods" not in S['Trading.cs']
            and "pass.Moved(profit, goldGained, selling: true);" in sell
            and "pass.Moved(gold: spent, selling: false);" in buy
            and sell.count("NoteStalled(") == 1 and buy.count("NoteStalled(") == 1
            and all(one.index("NoteStalled(") > one.index("else if (!pass.DirectionError)")
                    for one in (sell, buy))
            and "if (stopped != Block.None && !pass.Muted) NoteStalled(selling: true, stopped);" in sell
            and "if (stopped != Block.None && !pass.Muted) NoteStalled(selling: false, stopped);" in buy
            and "{=TL32}" not in sell and "{=TL33}" not in buy
            and all(field in reset for field in ("_sellStalled = null;", "_buyStalled = null;")))

def the_item_tooltip_does_not_announce_the_mod():
    body = method_body(S['TooltipPatches.cs'], "internal static void Append")
    return ("{=TL07}" not in body
            and "TooltipProperty.TooltipPropertyFlags.Title" not in body
            and "AddSeparator(vm);" in body)

def the_panel_legend_is_legible():
    m = re.search(r'SuggestedHeight="(\d+)"[^>]*?Brush\.FontSize="(\d+)"\s*\n\s*Brush\.FontColor="#(\w{6})(\w{2})"\s*\n\s*Text="@LegendText"',
                  PREFAB, re.S)
    return (m is not None and int(m.group(1)) >= 130 and int(m.group(2)) >= 16
            and int(m.group(4), 16) >= 0xCC)

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
                    ('PolicyAllows', 'Credit', 'ProfitAcceptable', 'Realizable', 'BuyAcceptable',
                     'AddPurchase', 'DrainSale', 'UnitBasis')))

def the_ledger_keeps_no_second_copy_of_the_cost_basis_rules():
    body = S['Ledger.cs']
    forwards = ('TradeMath.AddPurchase(rec, count, totalPaid);',
                'TradeMath.DrainSale(rec, count);',
                'TradeMath.UnitBasis(rec, Options.Current.CostBasisMode);')
    return (all(f in body for f in forwards)
            and 'rec.LastUnitPaid > 0' not in body
            and 'rec.TotalPaid / rec.Count' not in body
            and 'TradeMath.NoRecordedBasis' in body)

def a_good_you_never_bought_still_falls_through_to_the_market():
    body = method_body(S['Ledger.cs'], "public int GetCostBasis")
    return (ordered(body,
                    "Paid.TryGetValue(item.StringId, out var rec);",
                    "TradeMath.UnitBasis(rec, Options.Current.CostBasisMode);",
                    "if (unit != TradeMath.NoRecordedBasis) return unit;",
                    "BestBuy(item);")
            and "best.price > 0 ? best.price : item.Value;" in body)

def the_cost_basis_rules_are_covered_by_tests_the_build_runs():
    return ('TradeMath.UnitBasis' in MATHTESTS
            and 'TradeMath.AddPurchase' in MATHTESTS
            and 'TradeMath.DrainSale' in MATHTESTS
            and 'TradeMath.NoRecordedBasis' in MATHTESTS
            and 'LedgerCodec.cs' in TESTPROJ)

def the_policy_layer_keeps_no_second_copy_of_the_money_rules():
    body = S['Policy.cs']
    anywhere = S['Policy.cs'] + S['Trading.cs']
    forwards = ('TradeMath.Credit(proceeds, basis, unpaidWorth);',
                'TradeMath.ProfitAcceptable(costBasis, townSellPrice, Options.Current.MinProfitMargin);',
                'TradeMath.Realizable(farSellPrice, Options.Current.ResaleSafetyFactor);',
                'TradeMath.BuyAcceptable(buyPrice, realizable, Options.Current.MinProfitMargin);')
    return (all(f in body for f in forwards)
            and 'TradeMath.PolicyAllows(PolicyFor(good, s), buying: true)' in S['Rules.cs']
            and 'TradeMath.PolicyAllows(PolicyFor(good, s), buying: false)' in S['Rules.cs']
            and 'PolicyAllows' not in anywhere
            and 'gain > 0 ? gain : 0' not in anywhere
            and 'ResaleSafetyFactor;' not in anywhere.replace('Options.Current.ResaleSafetyFactor);', ''))

def the_money_rules_are_covered_by_tests_the_build_runs():
    return ('TradeMath.cs' in TESTPROJ and 'Options.cs' in TESTPROJ
            and MATHTESTS.count('[Fact]') + MATHTESTS.count('[Theory]') >= 12
            and 'TradeMath.Credit' in MATHTESTS and 'TradeMath.BuyAcceptable' in MATHTESTS
            and 'TradeMath.ProfitAcceptable' in MATHTESTS and 'TradeMath.PolicyAllows' in MATHTESTS)

def the_route_rules_need_nothing_from_the_game():
    return ('TaleWorlds' not in S['Confidence.cs']
            and 'public static class Confidence' in S['Confidence.cs']
            and 'Confidence' not in S['Market.cs']
            and ('public static int Budget(int gold, int goldReserve, int maxSpendPerVisit,\n'
                 '                                 int spentThisVisit)') in S['TradeMath.cs']
            and "TradeMath.Budget(Hero.MainHero.Gold, GoldHeldBack()," in S['Trading.cs']
            and 'Options.Current.MaxSpendPerVisit > 0' not in
                buy_pass())

def the_route_rules_are_covered_by_tests_the_build_runs():
    return ('Confidence.cs' in TESTPROJ
            and ROUTETESTS.count('[Fact]') + ROUTETESTS.count('[Theory]') >= 15
            and 'TradeMath.Budget' in ROUTETESTS and 'Confidence.Of' in ROUTETESTS
            and 'Worse_news_never_raises_confidence_whatever_the_route_looks_like' in ROUTETESTS
            and 'NeverSet' in ROUTETESTS)

def saved_field_types():
    types = {}
    for text in (S['Ledger.cs'], S['Trading.cs']):
        for m in re.finditer(r'private\s+([\w<>,\.\[\]\s]+?)\s+(_\w+)\s*(?:=[^;]*)?;', text):
            types[m.group(2)] = ' '.join(m.group(1).split())
        for name in ('Ledger.cs', 'Trading.cs'):
            body = method_body(S[name], "public override void SyncData")
            for m in re.finditer(r'^\s{12}(\w[\w<>,\.\[\]]*)\s+(\w+)\s*=\s*[^=]', body, re.M):
                types.setdefault(m.group(2), m.group(1))
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
    return seen >= 7

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
    return (len(numeric) == 7
            and all('CultureInfo.InvariantCulture' in c for c in numeric)
            and 'NumberStyles.Integer, CultureInfo.InvariantCulture' in codec
            and 'NumberStyles.Float, CultureInfo.InvariantCulture' in codec
            and not re.search(r'\.Append\(\w+\.(?:BuyPrice|SellPrice|CapturedDay|WasBuyPrice'
                              r'|WasSellPrice|WasDay|TotalPaid|Count|LastUnitPaid)\)',
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
    return (ledger.count('continue;') == 6 and purchases.count('continue;') == 3
            and ledger.count('unreadable++;') == 5
            and 'return book;' in ledger and 'return kept;' in purchases
            and 'throw' not in ledger and 'throw' not in purchases)

def the_codec_is_covered_by_tests_the_build_runs():
    return ('dotnet test tests/TradeLord.Tests.csproj' in WORKFLOW
            and 'LedgerCodec.cs' in TESTPROJ
            and TESTS.count('[Fact]') + TESTS.count('[Theory]') >= 12
            and 'InvariantCulture' not in TESTS)

def a_saved_price_costs_the_same_eight_fields_every_time():
    write = method_body(S['LedgerCodec.cs'],
                        "public static string WriteLedger(Dictionary<string, "
                        "List<PriceObservation>> ledger)")
    read = method_body(S['LedgerCodec.cs'],
                       "public static Dictionary<string, List<PriceObservation>> ReadLedger(string text)")
    return (write.count(".Append(FieldMark)") == 7
            and write.count("if (sb.Length > 0) sb.Append(RecordMark);") == 1
            and "parts.Length < FieldsAPriceNeeds" in read
            and "parts.Length >= FieldsAPriceIsWrittenIn" in read
            and "public const int FieldsAPriceIsWrittenIn = 8;" in S['LedgerCodec.cs']
            and "A_saved_price_is_written_out_field_by_field_exactly_as_it_reads_back" in TESTS
            and "Every_saved_price_costs_the_same_eight_fields_however_many_are_kept" in TESTS)

def each_preset_gets_its_own_settings():
    made = method_body(M, "public override BaseSettings CreateNew")
    return ('public Settings() { Bound(Options.Current); }' in M
            and 'private Options _o;' in M
            and 'made.Bound(new Options());' in made
            and 'Options.Current' not in made
            and M.count('Options.Current') == 4
            and 'held.Bound(Options.Current);' in method_body(M, "internal static void Reseat")
            and 'field.SetValue(Options.Current, now);' in
                method_body(M, "internal static void Reset")
            and '_o = to;' in method_body(M, "private void Bound")
            and all('Follows(value, () => _o.' + name in M
                    for name in ('Language', 'FoodPolicy', 'CraftingPolicy',
                                 'LivestockPolicy', 'CostBasisMode', 'KeepSmeltableWeapons')))


def restocking_runs_after_the_trading_buy():
    menu = method_body(S['Trading.cs'], "private void OnSessionLaunched")
    entry = method_body(S['Trading.cs'], "private void OnSettlementEntered")
    body = pass_body("public static void ExecuteResupply")
    return (ordered(menu, "ExecuteQuickSell(Settlement.CurrentSettlement);",
                    "ExecuteHaulage(Settlement.CurrentSettlement);",
                    "ExecuteQuickBuy(Settlement.CurrentSettlement);",
                    "ExecuteResupply(Settlement.CurrentSettlement);")
            and ordered(entry, "ExecuteQuickSell(settlement, quiet: true);",
                        "ExecuteHaulage(settlement, quiet: true);",
                        "ExecuteQuickBuy(settlement, quiet: true);",
                        "ExecuteResupply(settlement, quiet: true);")
            and "if (Options.Current.AutoBuyOnEntry) ExecuteResupply(settlement, quiet: true);" in entry
            and "if (Options.Current.KeepFoodDays <= 0) return;" in body
            and "TradePolicy.IsStorableFood(it) && TradePolicy.MayBuy(it, pass.Locked, out _, toFeed: true)"
                in method_body(S['Trading.cs'], "public static void ExecuteResupply")
            and "if (el.Amount <= 0 || !wanted(it)) continue;" in body
            and "found.Sort((x, y) => x.price.CompareTo(y.price));" in body
            and "pass.WouldReachYourReserve(price)" in body
            and "int worth = TradePolicy.UnpaidWorth(it);" in body
            and body.count("price > ceiling") == 2
            and "int ceiling = TradeMath.MostToPayOverTheCheapest(worth, tolerance);" in body
            and "TradePolicy.IsStorableFood(it) && TradePolicy.MayBuy(it, pass.Locked, out _, toFeed: true));"
                in method_body(S['Trading.cs'], "public static void ExecuteResupply")
            and "settlement.IsVillage && remaining <= 1" in body
            and "pass.Room() - simWeight" in body
            and "pass.Books.Sold(pass.Sim, it.StringId)" in body
            and "BuyAcceptable" not in body
            and "BestSell" not in body)


def the_ships_capacity_is_asked_in_one_place():
    t = S['Trading.cs']
    read = method_body(t, "private static float Read")
    return ("internal static class Carry" in t
            and t.count("party.InventoryCapacity") == 1
            and t.count("party.TotalWeightCarried") == 1
            and "CalculateInventoryCapacity(party, true)" in t
            and "CalculateTotalWeightCarried(party, true)" in t
            and ordered(read, "if (Sailing())",
                        "catch (Exception e) { Log.Error(e, \"fleet capacity (carts counted instead)\"); }",
                        "return capacity ? party.InventoryCapacity : party.TotalWeightCarried;")
            and "Carry.Carried(party)" in S['Panel.cs'] and "Carry.Capacity(party)" in S['Panel.cs']
            and "party.InventoryCapacity" not in S['Panel.cs']
            and "bool atSea = Carry.Sailing();" in method_body(S['Drove.cs'], "internal static int HaulAnimalsCargoCanSpare")
            and "Travel.NavalActive" not in method_body(S['Drove.cs'], "internal static int HaulAnimalsCargoCanSpare")
            and "Carry.Room(party) < 1f" in method_body(t, "private static bool NoRoomToCarry"))


results = []
_read = 0
def chk(ver, claim, ok):
    global _read
    if len(_lost) > _read:
        gone = ', '.join(sorted(set(_lost[_read:])))
        _read = len(_lost)
        ok = False
        claim += ' - this rule reads ' + gone + ', which the source no longer has'
    results.append(ok)
    print(('  ok      ' if ok else '  BROKEN  ') + f"[{ver}] {claim}")

chk("1.3.2", "smithing compares live DefaultItems, no cached static set",
    "item == DefaultItems.Charcoal" in S['Policy.cs']
    and not re.search(r'static\s+(readonly\s+)?HashSet<ItemObject>\s+\w+\s*[=;]', ALL))
chk("1.3.2", "ExcludeHostileTowns blocks trading, not just scans",
    (lambda gate: "IsMarket(s)" in gate
              and "Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(s)" in gate)
    (between(S['Trading.cs'], "private static bool CanTradeHere(Settlement s) =>", ";")) and
    "CanTradeHere(settlement)" in
        between(S['Trading.cs'], "private static bool MarketOpen(Settlement settlement, bool quiet) =>", ";"))
chk("1.3.18", "neither pass trades before the settling delay is served, in a market or on the road",
    (lambda b: ordered(b, "int wait = Options.Current.EconomySettlingDays;", "if (wait <= 0) return false;",
                       "CampaignStartTime.ElapsedDaysUntilNow",
                       "if (!Settling.StillHolding(wait, elapsed, out int daysLeft)) return false;",
                       '{=TL18}', "return true;")
           and "if (elapsed >= waitDays) return false;" in
               method_body(S['Rules.cs'], "internal static bool StillHolding"))
    (method_body(S['Trading.cs'], "private static bool StillSettling")) and
    "CanTradeHere(settlement) && !StillSettling(quiet)" in
        between(S['Trading.cs'], "private static bool MarketOpen(Settlement settlement, bool quiet) =>", ";") and
    "if (StillSettling(Muted(automated: true))) return;" in
        method_body(S['Trading.cs'], "public static void ExecuteRoadTrade") and
    "if (!MarketOpen(site, TradeActionBehavior.Muted(quiet))) return null;" in
        method_body(S['Trading.cs'], "internal static Pass Open") and
    "MarketOpen(site, quiet)" not in S['Trading.cs'] and
    S['Trading.cs'].count("MarketOpen(") == 2 and
    S['Trading.cs'].count("Pass.Open(settlement, quiet)") == 7 and
    S['Trading.cs'].count("Pass.Meet(met, road, books, party)") == 3)
chk("1.3.2", "how far a scan reaches is the two travel ceilings alone, and the scan radius that used to narrow it is gone",
    "WithinRadius" not in S['Ledger.cs'] and "ScanRadius" not in S['Ledger.cs'] and
    "ScanRadius" not in S['Options.cs'] and "ScanRadius" not in M and
    '"ScanRadius"' in S['Migrate.cs'] and
    "return WithinTravelCeiling(s, lower);" in method_body(S['Ledger.cs'], "private static bool Eligible") and
    "!Eligible(town, out float lower)" in S['Ledger.cs'])
chk("1.13.0", "no switch quietly writes another one, so what you set is what is kept",
    "EnableBuying" not in M and "EnableBuying" not in S['Options.cs'] and
    "EnableBuying" not in S['Trading.cs'] and "Loaded" not in M and
    "AutoTradeBoth" not in M and "AutoTradeBoth" not in S['Options.cs'] and
    all("Options.Current." + other not in body
        for name, other in (("AutoBuyOnEntry", "AutoSellOnEntry"), ("AutoSellOnEntry", "AutoBuyOnEntry"))
        for body in [method_body(M, "public bool " + name)]))
chk("1.3.2", "zero-gold purchase not recorded",
    all(re.search(r'if \((?:price|cost) == 0\) break;\s*'
                  r'LedgerBehavior\.Instance\?\.RecordPurchase\(item\.StringId, 1, (?:price|cost)\);\s*'
                  r'pass\.Books\.NoteBought\(item\.StringId, (?:price|cost)\);',
                  method_body(S['Trading.cs'], one)) is not None
        for one in ("public static void ExecuteResupply", "public static void ExecuteHaulage")) and
    re.search(r'if \(cost == 0\) return true;\s*'
              r'LedgerBehavior\.Instance\?\.RecordPurchase\(item\.StringId, 1, cost\);',
              method_body(S['Trading.cs'], "private sealed class BuyingAt")) is not None and
    re.search(r'if \(cost == 0\) break;\s*books\.NoteBought\(good\.Id, cost\);',
              method_body(S['Passes.cs'], "internal static Traded BuyThem")) is not None)
chk("1.3.2", "panel tracks a set of pins", "_panelPins = new HashSet<Settlement>" in S['Panel.cs'])
chk("1.3.2", "marker never removes a panel pin", "LedgerPanel.IsPinned(_tracked)" in S['Marker.cs'])
chk("1.3.2", "a good one half of the pass moved here is left alone by the other half",
    (lambda b: "if (books.Bought(sim, market.IdAt(at))) { tally.Note(Block.TradedHereAlready); continue; }" in b
           and "books.NoteSold(good.Id);" in b)
    (sell_pass()) and
    (lambda b: "if (books.Sold(sim, good.Id)) { tally.Note(Block.TradedHereAlready); continue; }" in b
           and "books.NoteBought(good.Id, cost);" in b)
    (buy_pass()) and
    "_bought[id] = (prior.count + 1, prior.spent + price);" in
        method_body(S['Books.cs'], "internal void NoteBought") and
    (lambda b: "_sold.Contains(id) || (sim && _drySold.Contains(id))" in b)
    (between(S['Books.cs'], "internal bool Sold(bool sim, string id) =>", ";")) and
    "A_good_the_buying_pass_took_is_written_into_the_books_for_the_other_half" in BUYPASSTESTS and
    "Cargo_the_selling_pass_moved_is_written_into_the_books_for_the_other_half" in SELLPASSTESTS and
    "A_good_the_buying_pass_took_here_is_left_alone_by_the_selling_pass" in SELLPASSTESTS and
    (lambda b: "_bought.ContainsKey(id) || (sim && _dryBought.ContainsKey(id))" in b)
    (between(S['Books.cs'], "internal bool Bought(bool sim, string id) =>", ";")))
chk("1.3.2", "a transaction that moves gold the wrong way stops the pass instead of draining the purse",
    (lambda b: ordered(b, "gold = selling ? Hero.MainHero.Gold - before : before - Hero.MainHero.Gold;",
                       "if (gold >= 0) return true;",
                       "transaction direction changed on this game version",
                       "return false;"))
    (method_body(S['Trading.cs'], "private static bool SwapOneUnit")) and
    S['Trading.cs'].count("SwapOneUnit(") == 2 and
    ordered(method_body(S['Trading.cs'], "private bool Swap(bool selling"),
            "if (SwapOneUnit(selling, swap, Site == null ? null : Shop, what, named, out gold)) return true;",
            "DirectionError = true;", "return false;") and
    S['Trading.cs'].count("DirectionError = true;") == 1 and
    all("if (pass.DirectionError" in method_body(S['Trading.cs'], one) for one in
        ("public static void ExecuteResupply",
         "public static void ExecuteHerdRelief", "public static void ExecuteHaulage")) and
    S['Trading.cs'].count("public bool Stopped => _pass.DirectionError;") == 2 and
    "if (market.Stopped) break;" in sell_pass() and
    "if (market.Stopped || market.Spendable() <= 0) break;" in buy_pass() and
    "directionError" not in S['Trading.cs'])
chk("1.3.2", "sim honors carry weight", "simWeight" in S['Trading.cs'])
chk("1.3.2", "observed mode uses Settlement.Find", "Settlement.Find(o.TownId)" in S['Ledger.cs'])
chk("1.3.2", "Instance cleared on game end", "LedgerBehavior.Instance = null" in S['SubModule.cs'])
chk("1.3.2", "panel rebuilt when map screen replaced", "map != _mapScreen" in S['Panel.cs'])
chk("1.3.2", "item lists case-insensitive", "StringComparer.OrdinalIgnoreCase" in S['Options.cs'])
chk("1.3.4", "per-item buy caps persist across clicks",
    "var prior = books.Purchases(sim, good.Id);" in
        buy_pass() and
    "int countThis = prior.count, spentThis = prior.spent;" in
        buy_pass() and
    (lambda b: "_bought.TryGetValue(id, out var real);" in b
           and "return (real.count + dry.count, real.spent + dry.spent);" in b)
    (method_body(S['Books.cs'], "internal (int count, int spent) Purchases")))
chk("1.3.4", "village last-unit clamp leaves one unit on the shelf",
    "if (lastInVillage) return Block.VillageLastUnit;" in
        cap_rule() and
    "market.Village && remaining <= 1, s);" in
        buy_pass() and
    "public bool Village => _pass.Site != null && _pass.Site.IsVillage;" in buy_pass() and
    all("if (settlement.IsVillage && remaining <= 1) break;" in method_body(S['Trading.cs'], one)
        for one in ("public static void ExecuteResupply", "public static void ExecuteHaulage")))
chk("1.3.5", "ledger ignores loot and automated passes",
    "if (!isTrading || TradeActionBehavior.AutomatedTradeInProgress) return;" in S['Ledger.cs'])
chk("1.3.5", "visit counters reset on entry", "ResetVisit();" in S['Trading.cs'])
chk("1.3.5", "capture at most once per hour per town",
    "settlement.StringId == _capturedTown &&" in S['Ledger.cs'])
chk("1.3.5", "sale that moved no gold does not count", "if (proceeds == 0) break;" in sell_pass())
chk("1.3.5", "detailed-summary setting does not gate the log",
    "DetailedTradeSummary" not in method_body(S['Trading.cs'], "private static void LogDetail"))
chk("1.3.5", "log path worked out once per launch, at the first path that accepts the write",
    "foreach (string candidate in Candidates(FileName))" in
        method_body(S['Support.cs'], "private static string Resolve") and
    ordered(method_body(S['Support.cs'], "private static bool Ready"),
            "if (!_resolved)", "_resolved = true;", "_path = Resolve();") and
    S['Support.cs'].count("_path = Resolve();") == 1)
chk("1.3.5", "hotkey blocked while escape menu open", "!map.IsEscapeMenuOpened" in S['Panel.cs'])
chk("1.3.5", "travel caches cleared on game end",
    'Guard.Run("GameEnd.Travel", Travel.Forget)' in S['SubModule.cs'])
chk("1.3.6", "the smithing-material rule still binds buying as well as selling",
    "TradeMath.PolicyAllows(PolicyFor(good, s), buying: true)" in buy_rule() and
    "TradeMath.PolicyAllows(PolicyFor(good, s), buying: false)" in sell_rule() and
    "if (good.IsSmithingMaterial) return s.CraftingPolicy;" in
        method_body(S['Rules.cs'], "internal static int PolicyFor") and
    "good.IsSmithingMaterial = IsSmithingMaterial(item);" in
        method_body(S['Policy.cs'], "internal static Good Describe"))
chk("1.3.6", "vanilla suppression asks the ledger", "TooltipHelper.HasSection(____targetItem)" in S['TooltipPatches.cs'])
chk("1.3.6", "marker respects the sell policy",
    "TradePolicy.MaySell(el, locked, keepBack" in method_body(S['Marker.cs'], "private static List<(EquipmentElement item, int amount, int worth, int floor)> WhatYouCarryToSell"))
chk("1.3.6", "chunked trade lines silenced",
    "AutomatedTradeInProgress" in S['Trading.cs'] and "Patch_SilenceChunkedTradeLines" in S['Trading.cs'])
chk("1.3.6", "smithing materials still ship tradable, as the old switch shipped off",
    "CraftingPolicy = PolicyBuySell" in S['Options.cs'])
chk("1.3.8", "quick-buy respects inventory locks",
    "game.Locked()" in buy_rule() and
    "new AskTheGame { Locks = lockedKeys, What = new EquipmentElement(item) }" in S['Policy.cs'])
chk("1.3.8", "Harmony field injection uses four underscores", "____targetItem" in S['TooltipPatches.cs'])
chk("1.3.8", "quick-buy stops when the budget is spent",
    "if (market.Stopped || market.Spendable() <= 0) break;" in
        buy_pass())
chk("1.3.8", "the buying pass takes only what a market in reach pays more for, and only above the margin",
    (lambda b, far: "if (!market.ResaleMarket(one.At, here, carried + take, out int elsewhere))\n"
                    "                { tally.Note(Block.NoResaleMarket); continue; }" in b
               and "Settlement buyer = elsewhere.town;" in b
               and "if (buyer == null) return false;" in b
               and "if (!TradeMath.BuyAcceptable(price, TradeMath.Realizable(wouldDraw - drawn," in b
               and "{ tally.Note(Block.BelowMargin); break; }" in b
               and "if (town == null || price <= 0 || town == notHere) continue;" in far
               and "if (TradeMath.OutOfReach(days)) continue;" in far)
    (buy_pass(),
     method_body(S['Ledger.cs'], "internal (Settlement town, int price, Ladder rungs) WhereThisEarnsFastest")))
chk("1.3.2", "the buying pass stops at the purse, the per-item denar cap, the carry weight and the herd",
    (lambda b: "if (price > budget) return Block.BudgetSpent;" in b
           and "taken.spent + price > s.BuyValueCapPerItem) return Block.ItemValueCap;" in b
           and "if (livestock && herdRoom <= 0) return Block.HerdFull;" in b
           and "roomLeft" not in b)
    (cap_rule()) and
    "good.Weight > 0.01f && good.Weight > roomLeft;" in
        between(S['Rules.cs'], "internal static bool NoRoomForOneMore", ";") + ";" and
    (lambda b: "Block capped = TradeRules.WhatStopsBuying(" in b
           and "if (capped != Block.None) { tally.Note(capped); break; }" in b
           and ordered(b, "if (capped != Block.None) { tally.Note(capped); break; }",
                       "if (TradeRules.NoRoomForOneMore(good, market.Room() - simWeight))\n"
                       "                    { tally.Note(Block.CarryWeight); break; }"))
    (buy_pass()))
chk("1.3.14", "the selling pass stops when the merchant's till cannot cover the next unit, on a dry run too",
    (lambda sell: "TradeRules.WhatTheTillCanPay(sim ? simTill : market.TillNow()," in sell
              and "market.Village) < price)" in sell
              and "{ tally.Note(Block.MerchantTillEmpty); break; }" in sell)
    (sell_pass()) and
    "internal int TillNow => Site != null ? Market.Gold : Met.PartyTradeGold;" in S['Trading.cs'])
chk("1.3.9", "per-hour cache serves both price modes",
    ordered(S['Ledger.cs'], "_marketCache.TryGetValue", "? TopLive"))
chk("1.3.9", "entering a market drops cached rankings", "ForgetMarketRankings();" in S['Ledger.cs'])
chk("1.3.9", "the best market to sell at is the dearest and the best to buy at is the cheapest",
    "int p = selling ? yPrice.CompareTo(xPrice) : xPrice.CompareTo(yPrice);" in
    method_body(S['Ranking.cs'], "internal static int Rank(bool selling") and
    "Selling_puts_the_market_that_pays_most_first" in RANKTESTS and
    "Buying_puts_the_market_that_charges_least_first" in RANKTESTS)
chk("1.3.9", "the scan keeps to the stock floor and the village ceiling, and passes over no price for its age",
    (lambda live: "if (!selling && !TradeMath.EnoughOnTheShelf(StockOf(s, item), item.Value," in live
              and "minStock, minWorth)) continue;" in live
              and live.find("EnoughOnTheShelf") < live.find("Priced.At"))
    (method_body(S['Ledger.cs'], "private List<(Settlement, int)> TopLive")) and
    "CapturedDay" not in method_body(S['Ledger.cs'], "private List<(Settlement, int)> TopObserved") and
    "if (village && vcap > 0f && (cap <= 0f || vcap < cap)) cap = vcap;" in
    method_body(S['Ranking.cs'], "internal static float Ceiling"))
chk("1.3.26", "the unit-by-unit walk stops at the merchant's till and at the spending cap",
    (lambda b: "if (merchantTill > 0 && q.SellTotal + sellPrice > merchantTill) break;" in b
           and "if (spendCap > 0 && q.BuyTotal + buyPrice > spendCap) break;" in b)
    (method_body(S['Market.cs'], "internal static RouteQuote Walk")))
def a_market_the_game_cannot_reach_is_never_offered_or_timed():
    m = S['TradeMath.cs']
    ceiling = method_body(S['Ranking.cs'], "internal static bool WithinCeiling")
    label = method_body(S['Travel.cs'], "internal static string EstimateLabel")
    return ("public const float LongerThanAnyRide = 1000f;" in m
            and "float.IsNaN(days) || float.IsInfinity(days) || days >= LongerThanAnyRide;" in m
            and "if (TradeMath.OutOfReach(days)) return false;" in ceiling
            and ceiling.find("OutOfReach") < ceiling.find("Ceiling(village, s)")
            and 'if (days < 0.05f || TradeMath.OutOfReach(days)) return "";' in label
            and "OutOfReach(days) ? FurthestThereIs : days;" in
                method_body(m, "public static float DaysAtBestSpeed")
            and "A_ride_longer_than_any_on_the_map_is_out_of_reach" in MATHTESTS
            and "A_market_out_of_reach_is_turned_away_though_no_ceiling_is_set" in RANKTESTS)


def a_forecast_may_not_move_a_shown_price_out_of_all_recognition():
    m = S['TradeMath.cs']
    within = method_body(m, "public static int ForecastWithin")
    first = method_body(S['Market.cs'], "internal static int FirstUnit")
    return ("public const float MostAForecastMayMoveAPrice = 0.5f;" in m
            and "if (live <= 0 || forecast <= 0) return forecast;" in within
            and "int most = (int)(live * (1f + MostAForecastMayMoveAPrice));" in within
            and "int least = (int)(live * (1f - MostAForecastMayMoveAPrice));" in within
            and "return TradeMath.ForecastWithin(quoted, rung.At(0)) : quoted;" in
                first.replace("rung.Walkable ? ", "")
            and "A_forecast_far_above_the_live_price_is_held_to_the_cap" in MATHTESTS
            and "A_forecast_far_below_the_live_price_is_held_to_the_cap" in MATHTESTS)


def a_caravan_leaves_what_the_town_will_take_and_a_villager_leaves_the_lot():
    m = S['TradeMath.cs']
    unloads = method_body(m, "public static int WhatACaravanUnloads")
    road = method_body(S['Forecast.cs'], "private static void ReadWhatIsOnTheRoad")
    leaves = method_body(S['Forecast.cs'], "private static int WhatACaravanLeavesHere")
    return ("public const float ACaravanSellsAbove = 1.1f;" in m
            and "public const float ACaravanSellsThisEagerly = 3f;" in m
            and "float over = Finite(priceFactor, 0f) - ACaravanSellsAbove;" in unloads
            and "if (over <= 0f || budget <= 0f) return 0;" in unloads
            and "float units = budget * over * ACaravanSellsThisEagerly / unitPrice;" in unloads
            and "return units >= carried ? carried : (int)units;" in unloads
            and "WhatACaravanLeavesAtOneStop" not in m
            and "bool caravan = party.IsCaravan;" in road
            and "if (!caravan && !party.IsVillager) continue;" in road
            and "if (caravan) NoteAPurse(bound, party.PartyTradeGold, days);" in road
            and "int landing = caravan ? WhatACaravanLeavesHere(bound, item, amount) : amount;" in road
            and "TradeMath.WorthOf(landing, item.Value), days);" in road
            and "where.MarketData.GetPriceFactor(category)" in leaves
            and "CalculateDailySettlementBudgetForItemCategory(where, demand, category)" in leaves
            and "A_caravan_passing_a_town_that_pays_no_more_than_usual_leaves_nothing" in MATHTESTS
            and "A_caravan_leaves_no_more_than_the_town_daily_purse_for_that_kind_can_pay_for"
                in MATHTESTS)


chk("1.81.10", "a market the game could not find a road to is turned away and shows no travel time, whatever the travel ceilings are set to",
    a_market_the_game_cannot_reach_is_never_offered_or_timed())
chk("1.81.10", "what a market is forecast to pay is held within reach of what it pays today, so a bad forecast can no longer show a price nothing would ever take",
    a_forecast_may_not_move_a_shown_price_out_of_all_recognition())
chk("1.81.11", "a caravan is counted as leaving only what the town it is calling at will actually take off it, and only where that town pays above the usual, while a villager party is counted as leaving its whole load at the town its village trades through",
    a_caravan_leaves_what_the_town_will_take_and_a_villager_leaves_the_lot())

chk("1.3.26", "travel time counts the sea leg, and refreshes when the party has moved",
    "float days = Finite((landLeg / landSpeed + seaLeg / seaSpeed) / 24f, FurthestThereIs);" in
    method_body(S['TradeMath.cs'], "public static float DaysAtSpeed") and
    "return OutOfReach(days) ? FurthestThereIs : days;" in
    method_body(S['TradeMath.cs'], "public static float DaysAtSpeed") and
    "return TradeMath.DaysAtSpeed(distance, landRatio, land, sea);" in
    method_body(S['Travel.cs'], "internal static float Days") and
    "if (hour != _partyHour || at.DistanceSquared(_partyAt) > 100f)" in
    method_body(S['Travel.cs'], "internal static float EstimateDaysFromParty"))
chk("1.3.11", "the panel hands back the movie and the mouse, and honours the modifier keys",
    "layer.ReleaseMovie(movie)" in method_body(S['Panel.cs'], "internal static void Cleanup") and
    "SetInputRestrictions(false, InputUsageMask.All)" in
    method_body(S['Panel.cs'], "private static void ApplyIdleInput") and
    (lambda b: ordered(b, "if (wantMouse)", "_layer.ActiveCursor = CursorType.Default;",
                       "SetInputRestrictions(true, InputUsageMask.MouseButtons)",
                       "else", "SetInputRestrictions(false, InputUsageMask.All)"))
    (method_body(S['Panel.cs'], "private static void UpdateIdleInput")) and
    "SetInputRestrictions(true, InputUsageMask.Mouse)" in
    method_body(S['Panel.cs'], "private static void Show") and
    "if (!Input.IsKeyDown(_modifiers[i].left) && !Input.IsKeyDown(_modifiers[i].right)) return false;" in
    method_body(S['Panel.cs'], "private static bool HotkeyReleased"))
chk("1.3.9", "panel respects locks", "ISet<string> locked = TradePolicy.LockedKeys();" in S['Ledger.cs'])
chk("1.3.9", "summary names the six biggest by gold",
    "byValue.Sort((x, y) => y.Value.gold.CompareTo(x.Value.gold));" in S['Trading.cs'])
chk("1.3.9", "null item lists tolerated", '(src ?? "")' in S['Options.cs'])
chk("1.13.0", "every setting the screen shows reads and writes its own value only, so load order cannot matter",
    every_setting_keeps_to_its_own_value())
chk("1.3.10", "hotkey rejects non-key text", "Enum.IsDefined(typeof(InputKey), k)" in S['Panel.cs'])
def a_quest_item_is_never_sold_by_any_pass():
    sell = method_body(S['Policy.cs'], "internal static bool MaySell(ItemRosterElement el")
    spare = shed_rule()
    return ("el.EquipmentElement.IsQuestItem" in sell
            and "questItem" in spare
            and "held.IsQuestItem, Options.Current," in
                method_body(S['Policy.cs'], "internal static bool MayShedForHerd"))

chk("1.3.11", "a quest item is never sold, by the selling pass or by thinning the herd",
    a_quest_item_is_never_sold_by_any_pass())
chk("1.3.11", "NotMerchandise never sold",
    "good.NotMerchandise" in sell_rule() and
    "good.NotMerchandise = item.NotMerchandise;" in
        method_body(S['Policy.cs'], "internal static Good Describe"))
chk("1.3.33", "a unique or player-crafted good is left alone while the protection is on, an animal along with the rest",
    ordered(sell_rule(),
            "if (livestock && (good.IsHaulAnimal || good.IsSpareMount))\n"
            "            { said.Why = Block.MountOrHaulAnimal; return said; }",
            "if (s.ProtectSpecial && (good.IsUnique || good.IsCraftedByPlayer))\n"
            "            { said.Why = Block.Protected; return said; }") and
    "if (s.ProtectSpecial && (good.IsUnique || good.IsCraftedByPlayer)) return false;" in
        shed_rule())
chk("1.3.11", "panel drops input restrictions on teardown",
    "SetInputRestrictions(false, InputUsageMask.All)" in method_body(S['Panel.cs'], "internal static void Cleanup"))
chk("1.3.12", "sieges/raids excluded from scans",
    "if (UnderAttack(s) || VillageShut(s)) return false;" in method_body(S['Ledger.cs'], "private static bool Eligible") and
    "LedgerBehavior.UnderAttack(s)" in S['Marker.cs'])
chk("1.3.13", "the buy shelf is worked through most money first, and a test holds it to that rather than a line of source",
    "Picks.MostMoneyFirst(stock);" in method_body(S['Passes.cs'], "internal static List<Pick> WhatToBuy")
    and "stock?.Sort((x, y) => y.Worth.CompareTo(x.Worth));" in
        method_body(S['Passes.cs'], "internal static void MostMoneyFirst")
    and 'Passes.cs' in TESTPROJ
    and all(one in SHELFORDERTESTS for one in
            ("The_pick_that_would_make_the_most_is_bought_first",
             "A_shelf_already_in_order_is_left_in_it",
             "A_pick_that_would_make_nothing_goes_behind_one_that_would",
             "An_empty_shelf_and_no_shelf_at_all_are_both_taken_in_their_stride",
             "Nothing_is_lost_or_invented_however_the_shelf_arrives",
             "new Random(2276)")))
chk("1.3.13", "cost basis read once per stack", "ProfitAcceptable(int costBasis, int townSellPrice)" in S['Policy.cs'])
chk("1.13.0", "the automation switches are plain switches like the rest, with nothing behind them",
    "if (value == _o.AutoBuyOnEntry) return;" not in M and
    M.count("set { _o.AutoBuyOnEntry = value; Options.Bump(); } }") == 1 and
    M.count("set { _o.AutoSellOnEntry = value; Options.Bump(); } }") == 1)
chk("1.3.13", "caravan pressure is one pass",
    "internal static Dictionary<Settlement, int> CaravanPressure()" in S['Ledger.cs'])
chk("1.3.13", "main-party check ahead of the guard",
    re.search(r'if \(party != MobileParty\.MainParty\) return;', S['Trading.cs']) is not None)
chk("1.3.14", "sim honors the merchant till", "simTill" in S['Trading.cs'])
chk("1.3.14", "one predicate for ledger-priced items",
    S['Policy.cs'].count("bool Priced(") == 1 and "TradePolicy.Priced" in S['TooltipPatches.cs'] and
    "TradePolicy.Priced" in S['Ledger.cs'])
chk("1.42.0", "a livestock route is listed even when your herd is already full, deliberately, because the ledger says where the profit is and not what you could drive away today",
    "HerdRoomForLivestock" not in S['Ledger.cs'] and
    "herdRoom" not in method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes") and
    "Drove.RoomForLivestock(pass.Party)" in S['Trading.cs'])
chk("1.3.15", "recurring errors reported once", "is recurring - not reporting it again" in S['Support.cs'])
chk("1.3.16", "the hold-for-best-market floor is tested against every unit as the lot drains, and a test holds it to that",
    "internal static int BestMarketFloor(int elsewhere, float tolerance) =>" in S['Rules.cs']
    and "internal static bool BelowTheBestMarket(int price, int holdFloor) => price < holdFloor;" in S['Rules.cs']
    and ordered(method_body(S['Passes.cs'], "internal static Traded SellThem"),
                "bestMarketFloor = TradeRules.BestMarketFloor(elsewhere, s.BestSellTownTolerance);",
                "holdFloor = bestMarketFloor;",
                "if (TradeRules.BelowTheBestMarket(price, holdFloor))")
    and "(int)(elsewhere * s.BestSellTownTolerance)" not in S['Passes.cs']
    and 'Rules.cs' in TESTPROJ and 'Passes.cs' in TESTPROJ
    and all(one in FLOORTESTS for one in
            ("The_floor_is_the_other_market_price_less_the_tolerance_you_set",
             "A_part_denar_floor_is_cut_off_rather_than_rounded_up",
             "No_other_market_worth_naming_leaves_no_floor_to_clear",
             "A_price_under_the_floor_is_held_back_and_one_on_it_is_not",
             "With_no_floor_in_force_nothing_is_ever_held_back_for_a_better_market",
             "Selling_stops_at_the_first_unit_that_falls_under_the_floor",
             "A_lower_tolerance_never_holds_back_more_than_a_higher_one",
             "new Random(5540)")))
chk("1.3.16", "food branch falls through to the sell rules",
    ordered(sell_rule(),
            "int reserved = DrawKeepBack(amount - said.KeepCount, facts.FoodHeld, out bool fed);",
            "if (amount <= said.KeepCount) { said.Why = Block.FoodReserve; return said; }",
            "            said.Allowed = true;\n            return said;") and
    "reserve[item] = held - drawn;" in
        method_body(S['Policy.cs'], "private static void TakeBack"))
chk("1.3.17", "the marker picks its town through the same ceiling as everything else, with nothing of its own",
    "WithinRadius" not in S['Trading.cs'] and
    "float cap = LedgerBehavior.TravelCeiling(s);" in
        method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo"))
chk("1.3.17", "the haircut filters every route pair, on the prices that pair would really open at",
    "float realizable = TradePolicy.Realizable(openingSell);" in S['Ledger.cs'] and
    "if (!TradePolicy.BuyAcceptable(openingBuy, realizable)) { thrownAway++; continue; }" in S['Ledger.cs'] and
    "Realizable(sellPrice)" not in S['Ledger.cs'] and
    "BuyAcceptable(buyPrice" not in S['Ledger.cs'] and
    ordered(method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes"),
            "int openingSell = Bulk.Opening(to, item, true, sellPrice, landedAtSellTown);",
            "if (!TradePolicy.BuyAcceptable(openingBuy, realizable)) { thrownAway++; continue; }",
            "Bulk.Walk(from, to, item"))
chk("1.3.18", "denar cap reaches route quantities",
    "int spendCap = Options.Current.BuyValueCapPerItem;" in S['Ledger.cs'])
chk("1.3.19", "marker re-evaluated on settlement exit", "OnSettlementLeftEvent.AddNonSerializedListener" in S['Trading.cs'])
chk("1.3.22", "looted and raided villages refused and unscanned",
    "VillageShut" in method_body(S['Trading.cs'], "private static bool CanTradeHere") and
    "VillageShut(s)" in method_body(S['Ledger.cs'], "private static bool Eligible"))
chk("1.3.22", "panel takes mouse only",
    "SetInputRestrictions(true, InputUsageMask.Mouse)" in method_body(S['Panel.cs'], "private static void Show") and
    "IsFocusLayer = true" not in method_body(S['Panel.cs'], "private static void Show"))

chk("1.58.0", "a good you can store feeds one, and anything with a horse component feeds nothing at all",
    "return good.IsFood ? 1 : 0;" in
        method_body(S['Rules.cs'], "internal static int FoodValue") and
    "if (good.Id == null || good.HasHorse) return 0;" in
        method_body(S['Rules.cs'], "internal static int FoodValue") and
    "MeatCount" not in method_body(S['Policy.cs'], "internal static Good Describe") and
    "Math.Min(held.Amount - had, (reserve + perUnit - 1) / perUnit)" in food_rule())
chk("1.3.23", "herd surplus counts mounts against unmounted men",
    "Math.Max(0, (mounts < 0 ? 0 : mounts) - (menOnFoot < 0 ? 0 : menOnFoot));" in S['Rules.cs'] and
    "Herding.MountsNobodyRides(mounts, foot)" in S['Drove.cs'] and
    "NumberOfMenWithoutHorse" in S['Drove.cs'])
chk("1.3.23", "herd guard includes attached parties", "party.AttachedParties" in S['Drove.cs'])
chk("1.3.23", "the game's own trade permission gates trading",
    "SettlementAction.Trade, out _, out _" in S['Trading.cs'] and
    "GameAllowsTrade(s)" in method_body(S['Trading.cs'], "private static bool CanTradeHere"))
chk("1.3.23", "the access model is only asked about the settlement in context",
    "if (s != Settlement.CurrentSettlement) return true;" in S['Trading.cs'])


chk("1.3.25", "routes pair every top buy market against every top sell market",
    "foreach (var (to, sellPrice) in sells)" in method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes"))
chk("1.42.0", "your purse never reaches route quantities, deliberately, so the ledger quotes a route the same whether you are rich or broke",
    "PurseForAVisit()" not in S['Ledger.cs'] and
    "purse" not in method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes") and
    "Options.Current.MaxSpendPerVisit, 0);" in S['Trading.cs'] and
    "private static int MostWorthShowing(int buyPrice)" in S['Ledger.cs'])
chk("1.42.0", "a purse with nothing spendable in it still quotes every route, deliberately, so being broke never hides where the profit is",
    "if (purse <= 0) return routes;" not in S['Ledger.cs'] and
    "private List<TradeRoute> ScanRoutes()" in S['Ledger.cs'])
chk("1.42.0", "the route scan is not redone when your gold moves, deliberately, because your gold no longer changes a single thing it quotes",
    "_routePurse" not in S['Ledger.cs'] and
    "_routes = ScanRoutes();" in method_body(S['Ledger.cs'], "public List<TradeRoute> BestRoutes"))
chk("1.42.0", "an empty ledger names the travel ceilings, and an empty purse is said alongside the routes rather than instead of them",
    "TL377" in strings_declared() and "TL393" in strings_declared() and
    (lambda b, said: b.index("TL69") < b.index("NothingHereYouCouldBuy(hero)")
           and 'TradeActionBehavior.PurseForAVisit() > 0 ? "" : " | " + NothingHereYouCouldBuy(hero)' in b
           and "TL377" not in b and "{=TL377}" in said and "{=TL393}" in said)
    (method_body(S['Panel.cs'], "private void Refresh"),
     method_body(S['Panel.cs'], "private static string NothingHereYouCouldBuy")))
chk("1.3.25", "the herd probe runs only once livestock is actually on the shelf",
    "int herdRoom = -1;" in buy_pass() and
    "if (herdRoom < 0)\n                        herdRoom = Math.Max(0, "
    "market.HerdRoom() - books.HerdTaken(sim));" in buy_pass() and
    "public int HerdRoom() => Drove.RoomForLivestock(_pass.Party);" in buy_pass())

chk("1.3.26", "pathfinder calls are gated behind a straight-line lower bound",
    "float soonest = toBuy + Travel.StraightDaysBetween(from, to);" in S['Ledger.cs'] and
    ordered(S['Ledger.cs'], "float soonest = toBuy", "float days = toBuy + Travel.EstimateDaysBetween"))
chk("1.3.26", "the best route so far prunes a pair before it costs a walk through every unit's price",
    "if (best != null && TradeMath.PerDay(ceiling, days) <= bestKey)\n                        { thrownAway++; continue; }" in S['Ledger.cs'] and
    "float perDay = TradeMath.PerDay(profit, days);" in S['Ledger.cs'] and
    "Math.Max(days, 0.25f)" not in S['Ledger.cs'] and
    S['Ledger.cs'].count("TradeMath.PerDay(ceiling, days)") == 1 and
    "Math.Max(soonest, 0.25f)" not in S['Ledger.cs'] and
    ordered(S['Ledger.cs'], "TradeMath.PerDay(ceiling, days)", "Bulk.Walk(from, to, item"))
chk("1.3.26", "a route's whole trip stays inside the travel ceiling, on the straight line and on the real path",
    (lambda b: ordered(b, "if (cap > 0f && soonest > cap) continue;", "if (cap > 0f && days > cap) continue;"))
    (method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")))
chk("1.3.26", "both straight-line estimates share one implementation",
    S['Travel.cs'].count("private static float StraightDays(float distance)") == 1 and
    "return StraightDays(party.GetPosition2D.Distance(target.GetPosition2D));" in S['Travel.cs'] and
    "StraightDays(a.GetPosition2D.Distance(b.GetPosition2D))" in S['Travel.cs'])

chk("1.3.27", "tooltip postfix declares only the arguments it uses",
    "private static void Postfix(ItemMenuVM __instance, ItemVM item)" in S['TooltipPatches.cs'])
chk("1.3.27", "both shipped assemblies compile with warnings as errors",
    all("<TreatWarningsAsErrors>true</TreatWarningsAsErrors>" in p for p in PROJ))

chk("1.3.28", "both travel estimates take their speeds from one definition",
    S['Travel.cs'].count("MobileParty.MainParty.Speed") == 1 and
    S['Travel.cs'].count("TradeMath.SpeedsInEffect(") == 1 and
    S['TradeMath.cs'].count("land = partySpeed <= StandingStill ? WalkingPace : partySpeed;") == 1 and
    S['TradeMath.cs'].count("sea = fleetSpeed <= StandingStill ? land : fleetSpeed;") == 1 and
    "Speeds(out float land, out float sea);" in method_body(S['Travel.cs'], "internal static float Days") and
    "Speeds(out float land, out float sea);" in method_body(S['Travel.cs'], "private static float StraightDays"))

chk("1.3.29", "one buy-side margin rule, for the planner and the executor alike",
    S['Policy.cs'].count("internal static bool BuyAcceptable(int buyPrice, float realizable)") == 1 and
    S['Policy.cs'].count("Options.Current.ResaleSafetyFactor") == 1 and
    S['Ledger.cs'].count("Options.Current.MinProfitMargin") == 0 and
    S['Ledger.cs'].count("Options.Current.ResaleSafetyFactor") == 0 and
    "TradePolicy.BuyAcceptable" in S['Ledger.cs'] and
    "TradeMath.BuyAcceptable(buyPrice, realizable, Options.Current.MinProfitMargin)" in S['Policy.cs'] and
    S['Passes.cs'].count("TradeMath.BuyAcceptable(") == 4 and
    S['Passes.cs'].count("Options.Current") == 0)
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
    'Guard.Run("Ledger.Reindex", Reindex);' in
        method_body(S['Ledger.cs'], "public override void SyncData"))
chk("1.3.30", "a damaged purchase record does not throw during save load",
    "if (rec?.ItemId != null) _paid[rec.ItemId] = rec;" in
    method_body(S['Ledger.cs'], "private void Reindex"))

chk("1.3.31", "the price gate and the transaction it guards share one granularity",
    S['Trading.cs'].count("SellItemsAction.Apply(Me, Shop, _unit, 1, Site)") == 1 and
    S['Trading.cs'].count("SellItemsAction.Apply(Shop, Me, _unit, 1, Site)") == 1 and
    S['Trading.cs'].count("SellItemsAction.Apply(") == 2 and
    "SellItemsAction.Apply(Me, Shop, _unit, 1, Site)" in
        method_body(S['Trading.cs'], "internal bool SellOne(") and
    "SellItemsAction.Apply(Shop, Me, _unit, 1, Site)" in
        method_body(S['Trading.cs'], "internal bool BuyOne(") and
    "SellItemsAction.Apply(" not in method_body(S['Trading.cs'], "public static void ExecuteRoadTrade"))
chk("1.36.0", "a trade on the road moves one unit and its price itself, because the game's own sale needs a market",
    "SellItemsAction" not in method_body(S['Trading.cs'], "public static void ExecuteRoadTrade") and
    (lambda b: "? (Action)(() => SellItemsAction.Apply(Me, Shop, _unit, 1, Site))" in b
           and ": () => HandOver(Me, Shop, _unit.EquipmentElement, _unitPrice);" in b)
        (method_body(S['Trading.cs'], "internal bool SellOne")) and
    (lambda b: "? (Action)(() => SellItemsAction.Apply(Shop, Me, _unit, 1, Site))" in b
           and ": () => TakeDelivery(Shop, Me, _unit.EquipmentElement, _unitPrice);" in b)
        (method_body(S['Trading.cs'], "internal bool BuyOne")) and
    (lambda b: "me.ItemRoster.AddToCounts(what, -1);" in b
           and "shop.ItemRoster.AddToCounts(what, 1);" in b
           and "GiveGoldAction.ApplyForPartyToCharacter(shop, Hero.MainHero, price, true);" in b)
        (method_body(S['Trading.cs'], "private static void HandOver")) and
    (lambda b: "shop.ItemRoster.AddToCounts(what, -1);" in b
           and "me.ItemRoster.AddToCounts(what, 1);" in b
           and "GiveGoldAction.ApplyForCharacterToParty(Hero.MainHero, shop, price, true);" in b)
        (method_body(S['Trading.cs'], "private static void TakeDelivery")) and
    S['Trading.cs'].count("HandOver(") == 2 and S['Trading.cs'].count("TakeDelivery(") == 2)

chk("1.3.32", "a dry run reports itself as a best case, in the toast, the log and the hint",
    S['Trading.cs'].count("[Simulated, best case]") == 8 and
    S['Trading.cs'].count("(simulated, best case): ") == 3 and
    'internal string Headed(string label) => label + (Sim ? Counter.Heading : ": ");'
        in S['Trading.cs'] and
    'internal static string Heading => Staging ? " (laid out): " : " (simulated, best case): ";'
        in S['Counter.cs'] and
    'internal static string Aside => Staging ? " (laid out)" : " (simulated)";'
        in S['Counter.cs'] and
    S['Trading.cs'].count("(sim ? Counter.Aside : \"\")") == 2 and
    S['Trading.cs'].count("Log.Write(pass.Headed(label)") == 3 and
    "best case" in M)

chk("1.3.33", "a fully sold stack clears its cost basis",
    "if (left <= 0) { rec.Count = 0; rec.TotalPaid = 0; return; }" in
    method_body(S['TradeMath.cs'], "public static void DrainSale"))
chk("1.3.33", "automated trading recaptures prices after it moves them",
    all("pass.Moved(" in method_body(S['Trading.cs'], where)
        for where in ("private static void SellPass",
                      "public static void ExecuteHerdRelief",
                      "public static void ExecuteResupply",
                      "public static void ExecuteHaulage",
                      "private static void BuyPass")) and
    "LedgerBehavior.Instance?.CaptureSettlement(Site, force: true, KindsMoved());" in
        method_body(S['Trading.cs'], "internal void Moved") and
    S['Trading.cs'].count("CaptureSettlement(Site, force: true, KindsMoved());") == 1 and
    "internal void ForgetMarketRankings()" in S['Ledger.cs'] and
    "DropRankings(settlement, Options.Current.Omniscient ? moved : null);" in
        method_body(S['Ledger.cs'],
                    "public void CaptureSettlement(Settlement settlement, bool force, ISet<string> moved)"))
_setters = [b for b in re.findall(r'\bset\b\s*(\{[^{}]*(?:\{[^{}]*\}[^{}]*)*\})', M)
            if "Options.Bump();" in b]
chk("1.3.33", "every settings write bumps the generation every cache keys on",
    len(_setters) == len(re.findall(r'\[SettingProperty(?:Bool|Integer|FloatingInteger|Text|Dropdown)\(', M)))

chk("1.3.34", "one naval gate for both map-distance calls",
    S['Travel.cs'].count("HasPort && naval") == 2 and
    "target.HasPort && NavalActive" in S['Travel.cs'])
chk("1.3.34", "both travel caches hold geometry and re-time it at the current speed",
    "return Days(hit.dist, hit.landRatio);" in
    method_body(S['Travel.cs'], "internal static float EstimateDaysFromParty") and
    "return Days(hit.dist, hit.landRatio);" in
    method_body(S['Travel.cs'], "internal static float EstimateDaysBetween"))
chk("1.11.1", "one naval invalidation rule, read by every travel estimate it feeds",
    S['Travel.cs'].count("DropIfNavalChanged();") == 3 and
    "DropIfNavalChanged();" in method_body(S['Travel.cs'], "internal static float EstimateDaysFromParty") and
    "DropIfNavalChanged();" in method_body(S['Travel.cs'], "internal static float EstimateDaysBetween") and
    "DropIfNavalChanged();" in method_body(S['Travel.cs'], "private static float StraightDays") and
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
    "_pending.Add(new InformationMessage(msg.ToString(), color))" in S['Notices.cs'] and
    'Guard.Run("Tick.FlushToasts", TradeActionBehavior.FlushToasts)' in
    method_body(S['SubModule.cs'], "protected override void OnApplicationTick"))
chk("1.4.0", "realized profit is banked for the campaign and shown in the panel",
    'dataStore.SyncData("TradeLord_LifetimeProfit", ref _lifetimeProfitCapped);' in S['Ledger.cs'] and
    'dataStore.SyncData("TradeLord_LifetimeProfitWide", ref _lifetimeProfit);' in S['Ledger.cs'] and
    "LedgerBehavior.Instance?.AddProfit(profit.Value);" in S['Trading.cs'] and
    "LedgerBehavior.Instance?.LifetimeProfit" in S['Panel.cs'])
chk("1.4.0", "the hotkey honors its modifiers on both the open and the close edge",
    S['Panel.cs'].count("HotkeyReleased()") == 3 and
    "Input.IsKeyReleased(key)" not in S['Panel.cs'])

chk("1.4.1", "quick-buy prices the shelf only when there is a budget to spend",
    (lambda b: "if (pass.Spendable() > 0)" in b
           and ordered(b, "if (pass.Spendable() > 0)",
                       "TradePass.WhatToBuy(market, pass.Books, pass.Sim, shareCap,",
                       "ItemRoster shopRoster = _pass.Stock;")
           and "internal ItemRoster Stock => Site != null ? Site.ItemRoster : Met.ItemRoster;"
               in S['Trading.cs']
           and S['Trading.cs'].count("pass.Stock") == 2)
    (buy_pass()))
chk("1.4.1", "a pass the gold-direction guard stopped does not blame the trade policy",
    S['Trading.cs'].count("else if (!pass.DirectionError)") == 3 and
    S['Trading.cs'].count("else if (!quiet && !directionError)") == 0 and
    'Tongue.Text("{=TL32}Nothing sold here - {REASON}.")' in S['Trading.cs'] and
    'Tongue.Text("{=TL33}Nothing bought here - {REASON}.")' in S['Trading.cs'])
chk("1.4.1", "the automatic path asks the same market question the menu does",
    "IsMarket" not in method_body(S['Trading.cs'], "private void OnSettlementEntered"))
chk("1.4.1", "planner and executor apply the same village last-unit clamp",
    "onTheShelfNow = StockOf(from, item) - (from.IsVillage ? 1 : 0);" in S['Ledger.cs'] and
    "TradeMath.StockAfterShift(onTheShelfNow," in S['Ledger.cs'] and
    "if (lastInVillage) return Block.VillageLastUnit;" in S['Rules.cs'] and
    "market.Village && remaining <= 1, s);" in
        buy_pass())

chk("1.4.2", "a village the game will not trade in is not a destination either",
    "v.VillageState != Village.VillageStates.Normal" in
    method_body(S['Ledger.cs'], "internal static bool VillageShut"))
chk("1.4.2", "straight-line distance fallback returns a straight-line land ratio",
    "catch { landRatio = 1f; }" in method_body(S['Travel.cs'], "internal static float FromParty") and
    "catch { landRatio = 1f; }" in method_body(S['Travel.cs'], "internal static float Between"))
chk("1.4.2", "one flush does not post the same line twice",
    "_pending[i].Information != _pending[i - 1].Information" in
    method_body(S['Notices.cs'], "internal static void Drain"))

chk("1.4.3", "an hourly capture can be forced, and only a forced one skips the dedupe",
    "public void CaptureSettlement(Settlement settlement, bool force = false)" in S['Ledger.cs'] and
    "if (!force && settlement.StringId == _capturedTown &&" in S['Ledger.cs'])
chk("1.4.3", "one rule for what the ledger will capture, and it tolerates no settlement at all",
    "if (settlement == null || (!settlement.IsTown && !settlement.IsVillage)) return;" in
    method_body(S['Ledger.cs'], "public void CaptureSettlement") and
    "IsVillage" not in method_body(S['Ledger.cs'], "private void OnSettlementEntered"))
chk("1.4.3", "one definition of the livestock the mod trades",
    S['Policy.cs'].count("&& item.HorseComponent.IsLiveStock") == 1)
chk("1.4.3", "cost basis uses recorded purchase prices, not current market quotes",
    "Options.Current.CostBasisMode == 2 ||" in
    method_body(S['Policy.cs'], "private static bool HasCostBasis") and
    "HasPurchaseRecord(item) ?? false)" in
    method_body(S['Policy.cs'], "private static bool HasCostBasis") and
    "item.IsTradeGood ||" not in method_body(S['Policy.cs'], "private static bool HasCostBasis"))
chk("1.21.0", "the sell-side floor is the hold-for-the-best-market switch and nothing else, so it binds every unit alike or none",
    "if (s.PreferBestSellTown)" in
        sell_pass() and
    "basis == 0)" not in sell_pass() and
    "holdFloor = bestMarketFloor;" in S['Passes.cs'] and
    "TradePolicy.Priced(item)" not in sell_pass())
chk("1.5.5", "a stack pays its purchased basis only for the units that were purchased, and only those units drain the record",
    "public int PurchasedUnits(int at) => LedgerBehavior.Instance?.PurchasedUnits(Item(at)) ?? 0;" in
        method_body(S['Trading.cs'], "private sealed class SellingFrom") and
    "basis.PaidLeft = Math.Max(0, purchased - books.PaidDrawn(sim, id));" in
        method_body(S['Passes.cs'], "internal static Basis For") and
    "int worth = FromMarket || PaidLeft > 0 ? Paid : 0;" in
        method_body(S['Passes.cs'], "internal int Unit(out bool askTheMarket)") and
    ordered(method_body(S['Passes.cs'], "internal bool SoldOne()"),
            "if (PaidLeft <= 0) return false;", "PaidLeft--;", "return true;") and
    S['Passes.cs'].count("if (basis.SoldOne()) market.RecordedSale(at);") == 1 and
    "else LedgerBehavior.Instance?.RecordSale(item.StringId, 1);" in
        method_body(S['Trading.cs'], "public static void ExecuteHerdRelief") and
    S['Passes.cs'].count("PaidLeft--;") == 1 and
    sell_pass().count("RecordSale") == 1)
chk("1.4.3", "the panel is rebuilt for a new map screen, not for every visit to another one",
    "if (_mapScreen != null && map != null && map != _mapScreen)" in S['Panel.cs'])

chk("1.5.0", "the walk gates every unit with the executor's own margin rule",
    "if (!TradePolicy.BuyAcceptable(buyPrice, TradePolicy.Realizable(sellPrice))) break;" in
    method_body(S['Market.cs'], "internal static RouteQuote Walk"))
chk("1.5.0", "one definition of the resale haircut, walk and planner alike",
    S['Market.cs'].count("Options.Current.ResaleSafetyFactor") == 0 and
    S['Ledger.cs'].count("Options.Current.ResaleSafetyFactor") == 0 and
    S['Policy.cs'].count("Options.Current.ResaleSafetyFactor") == 1 and
    "(int)TradePolicy.Realizable(q.SellTotal)" in S['Ledger.cs'])
chk("1.5.0", "observed mode does not read live market supply/demand for projections",
    "if (projecting && (!Options.Current.Omniscient || !Options.Current.BulkSimulation)) return;" in
    method_body(S['Market.cs'],
                "internal Shelf(Settlement site, EquipmentElement stocked, bool selling, int quoted, bool projecting, int landed = 0)"))
chk("1.5.0", "only a town shelf can be advanced, because only a town publishes the inputs",
    "Town town = site != null && site.IsTown ? site.Town : null;" in S['Market.cs'] and
    "if (town == null" in S['Market.cs'] and
    "GetCategoryData" in S['Market.cs'])
chk("1.5.0", "an unwalkable shelf falls back to the quoted price and reports Simulated=false",
    "q.Simulated = buy.Walkable && sell.Walkable;" in S['Market.cs'] and
    "if (!_walkable) return;" in method_body(S['Market.cs'], "internal void Restock(int units)"))
chk("1.5.0", "buying strips the shelf and selling stocks it",
    "if (_selling) _shelf.Restock(1); else _shelf.Restock(-1);" in
        method_body(S['Market.cs'], "internal int At(int taken)") and
    "Restock" not in method_body(S['Market.cs'], "internal static RouteQuote Walk"))
chk("1.5.0", "route pruning is bounded by the very prices the quote opens at, so it cannot discard a viable route",
    "float ceiling = (float)(openingSell - openingBuy) * qtyCap;" in S['Ledger.cs'] and
    "(sellPrice - buyPrice) * qtyCap" not in S['Ledger.cs'] and
    "int openingBuy = Bulk.Opening(from, item, false, buyPrice, landedAtBuyTown);"
        in S['Ledger.cs'] and
    "int openingSell = Bulk.Opening(to, item, true, sellPrice, landedAtSellTown);"
        in S['Ledger.cs'] and
    "int walked = Rung(site, item, selling, quoted, landed).At(0);" in
        method_body(S['Market.cs'], "internal static int Opening") and
    ordered(S['Ledger.cs'], "float ceiling =", "Bulk.Walk(from, to, item"))
chk("1.5.0", "a broke selling town is no destination, in the mode that can see its till",
    "if (till <= 0) continue;" in S['Ledger.cs'])
chk("1.5.0", "the panel is ordered by the column it shows",
    "rankByScore ? score : perDay" in S['Ledger.cs'] and
    "float score = perDay * confidence;" in S['Ledger.cs'] and
    "Options.Current.ConfidenceRanking ? _route.Score : _route.ProfitPerDay" in S['Panel.cs'] and
    "y.Score.CompareTo(x.Score)" in S['Ledger.cs'])
chk("1.5.0", "caravan pressure is counted once, by the planner that scores on it",
    S['Panel.cs'].count("CaravanPressure()") == 0 and
    S['Ledger.cs'].count("var pressure = CaravanPressure();") == 1)
chk("1.5.0", "every confidence factor is a fraction of one",
    "return c < 0.01f ? 0.01f : (c > 1f ? 1f : c);" in
    method_body(S['Confidence.cs'], "public static float Of(bool simulated"))

chk("1.5.0", "one place decides which category policy governs an item",
    S['Rules.cs'].count("internal static int PolicyFor(in Good good, Options s)") == 1 and
    S['Rules.cs'].count("s.FoodPolicy") == 1 and
    S['Rules.cs'].count("s.CraftingPolicy") == 1 and
    S['Rules.cs'].count("s.LivestockPolicy") == 1 and
    not any(named in S['Policy.cs'] for named in
            ("FoodPolicy", "CraftingPolicy", "LivestockPolicy", "PolicyAllows")))
chk("1.5.0", "a head of cattle is asked as livestock, not as food",
    ordered(method_body(S['Rules.cs'], "internal static int PolicyFor"), "LivestockPolicy", "FoodPolicy"))
chk("1.23.0", "the selling fence is the haul animals themselves, so an animal that hauls nothing is not fenced in with them",
    "if (livestock && (good.IsHaulAnimal || good.IsSpareMount))" in sell_rule() and
    "IsLivestock" not in sell_rule() and
    "bool sellable = livestock || good.IsTradeGood ||" in sell_rule() and
    "if (good.IsLivestock) return true;" in buy_rule() and
    "why = Block.MountOrHaulAnimal;" in buy_rule())
chk("1.28.0", "a haul animal is named by all five answers the game gives about it, and the selling fence is those plus the spare mounts",
    "internal static bool IsHaulAnimal(ItemObject item) =>\n"
    "            item != null && item.HasHorseComponent &&\n"
    "            item.HorseComponent.IsRideable && item.HorseComponent.IsPackAnimal &&\n"
    "            !item.HorseComponent.IsMount && !item.HorseComponent.IsLiveStock &&\n"
    "            item.ItemCategory == DefaultItemCategories.PackAnimal;" in S['Policy.cs'] and
    "if (livestock && (good.IsHaulAnimal || good.IsSpareMount))" in sell_rule() and
    "good.IsHaulAnimal = IsHaulAnimal(item);" in
        method_body(S['Policy.cs'], "internal static Good Describe") and
    "good.IsSpareMount = IsSpareMount(item);" in
        method_body(S['Policy.cs'], "internal static Good Describe") and
    S['Policy.cs'].count("item.HorseComponent.IsRideable && item.HorseComponent.IsPackAnimal") == 1 and
    S['Policy.cs'].count("IsHaulAnimal(ItemObject item)") == 1 and
    "IsHaulAnimalOrMount" not in S['Trading.cs'])
chk("1.28.0", "a war horse and a noble horse are the last mounts the herd gives up, told apart by the game's own trade category",
    "internal static bool IsPrizeMount(ItemObject item) =>\n"
    "            IsSpareMount(item) &&\n"
    "            (item.ItemCategory == DefaultItemCategories.WarHorse ||\n"
    "             item.ItemCategory == DefaultItemCategories.NobleHorse);" in S['Policy.cs'] and
    S['Policy.cs'].count("IsPrizeMount(ItemObject item)") == 1)
chk("1.33.0", "the animal roll call is gone from the log and from the campaign opening, every part of it",
    "RollCall" not in ALL and "LogAnimalRollCall" not in ALL and
    "more not listed" not in ALL and
    "AnimalRollCall" not in method_body(S['Trading.cs'], "private void OnSessionLaunched"))
chk("1.5.0", "every category ships trading exactly as it did before the matrix",
    "FoodPolicy = PolicyBuySell" in S['Options.cs'] and
    "CraftingPolicy = PolicyBuySell" in S['Options.cs'] and
    "LivestockPolicy = PolicyBuySell" in S['Options.cs'])
chk("1.5.0", "the food reserve is not a trading policy and is not governed by one",
    "FoodPolicy" not in method_body(S['Rules.cs'], "internal static Dictionary<string, int> FoodKeep") and
    "FoodPolicy" not in method_body(S['Policy.cs'], "internal static Dictionary<ItemObject, int> KeptBack"))

chk("1.5.0", "a pass that moves nothing names the rule that stopped it",
    'Tongue.Text("{=TL32}Nothing sold here - {REASON}.")' in S['Trading.cs'] and
    'Tongue.Text("{=TL33}Nothing bought here - {REASON}.")' in S['Trading.cs'] and
    S['Trading.cs'].count("NoteStalled(selling: ") == 3 and
    method_body(S['Trading.cs'],
                "private static void ReportStalledPasses").count("BlockTally.Phrase(") == 4)
chk("1.5.0", "the reason overloads carry the plain ones, so one rule set decides both",
    "MaySell(el, lockedKeys, foodKeep, awaited, out keepCount, out _);" in S['Policy.cs'] and
    S['Policy.cs'].count("bool MayBuy(ItemObject item, ISet<string> lockedKeys") == 1 and
    "out Block why," in method_body(S['Policy.cs'], "internal static bool MayBuy(ItemObject item") and
    "TradePolicy.MayBuy(it, pass.Locked, out _, toFeed: true)" in S['Trading.cs'] and
    "TradeRules.MaySell(good, el.Amount, facts, Options.Current," in S['Policy.cs'] and
    "TradeRules.MayBuy(good, toFeed, Options.Current," in S['Policy.cs'])
chk("1.5.0", "every stop in the sell pass is counted",
    sell_pass().count("tally.Note(") >= 5)
chk("1.5.0", "every stop in the buy pass is counted",
    buy_pass().count("tally.Note(") >= 10)
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
    "GetItemPrice" not in between(S['Market.cs'], "internal sealed class Shelf",
                                  "internal static class Priced") and
    "if (!_walkable) return _quoted;" in method_body(S['Market.cs'], "internal int Price()") and
    "Bulk.Walk(from, to, item, qtyCap, till, spendCap," in S['Ledger.cs'])
chk("1.5.1", "an unwalkable shelf reads its quote once",
    method_body(S['Market.cs'], "internal int Price()").count("_quoted") == 2)
chk("1.5.1", "confidence measures the walk, not two price APIs disagreeing",
    "int flatSell = q.OpeningSellPrice * q.Units;" in S['Ledger.cs'] and
    "- q.OpeningBuyPrice * q.Units;" in S['Ledger.cs'])
chk("1.5.1", "the no-trade message reports a blocking rule, not a structural exclusion",
    "private static bool Structural(Block reason)" in S['Passes.cs'] and
    "if (!Structural(kv.Key) && kv.Key != Block.BudgetSpent &&" in
    method_body(S['Passes.cs'], "internal Block Dominant") and
    "Structural" not in method_body(S['Passes.cs'], "internal string Summary"))
chk("1.5.1", "no two settings in one MCM group claim the same position",
    mcm_orders_unique())

chk("1.5.2", "a listed route passes both the buy and the sell policy check",
    "internal static bool MayRoundTrip(ItemObject item, ISet<string> lockedKeys)" in S['Policy.cs'] and
    "MayBuy(good, item, lockedKeys, out _) &&" in
    method_body(S['Policy.cs'], "internal static bool MayRoundTrip") and
    "TradeMath.PolicyAllows(PolicyFor(good, s), buying: false)" in
    between(S['Rules.cs'], "internal static bool ResaleAllowed", ";") and
    "if (!TradePolicy.MayRoundTrip(item, locked)) continue;" in S['Ledger.cs'] and
    "TradePolicy.MayBuy(item, locked)" not in S['Ledger.cs'])
chk("1.5.2", "port menus are asked for only where the module that owns them is installed",
    "if (NavalModulePresent())" in S['Trading.cs'] and
    'ModuleHelper.GetModuleInfo("NavalDLC")' in
    method_body(S['Trading.cs'], "private static bool NavalModulePresent"))
chk("1.5.2", "the Price columns the panel shows are the first unit of the very quote behind them, as the tooltip's are",
    "BuyPrice = q.OpeningBuyPrice, SellPrice = q.OpeningSellPrice," in S['Ledger.cs'] and
    "BuyPrice = buyPrice" not in S['Ledger.cs'] and
    "SellPrice = sellPrice" not in S['Ledger.cs'] and
    S['Ledger.cs'].count("q.Opening") == 4 and
    "Bulk.FirstUnit(town, item, selling, price," in
        method_body(S['TooltipPatches.cs'], "private static void AsTheyWillBe"))
chk("1.5.2", "RouteQuote carries no unread field",
    "ClosingBuyPrice" not in S['Market.cs'] and "ClosingSellPrice" not in S['Market.cs'] and
    S['Market.cs'].count("OpeningBuyPrice") == 2 and S['Market.cs'].count("OpeningSellPrice") == 2)

chk("1.5.2", "every setting default lies within its own declared range",
    mcm_defaults_within_range())
chk("1.5.2", "every setting TradeLord keeps has a control of its own on the settings screen, and the screen carries nothing the settings file does not keep",
    every_option_has_a_control())
def a_failed_publish_is_retried_and_nothing_in_the_loop_can_kill_it():
    at = WORKFLOW.find("- name: Publish release")
    step = "" if at < 0 else WORKFLOW[at:]
    tidying = [one.strip() for one in step.split("\n")
               if one.strip().startswith("gh release delete")]
    return ("for attempt in 1 2 3 4 5; do" in WORKFLOW
            and "the request landed despite the error" in WORKFLOW
            and len(tidying) == 2
            and all("||" in one for one in tidying))

chk("1.5.2", "a timed-out publish is retried, and tidying a stranded draft can never end the run before the retry",
    a_failed_publish_is_retried_and_nothing_in_the_loop_can_kill_it())
chk("1.5.2", "a draft release left by a timeout is deleted before republishing",
    "--json isDraft -q .isDraft" in WORKFLOW and
    'gh release delete "$VERSION" --yes' in WORKFLOW and
    "exists only as a draft left by a timed-out attempt" in WORKFLOW)

chk("1.5.3", "every declared Harmony patch is installed",
    every_declared_patch_is_installed())
chk("1.5.3", "both projects pin the same reference-assembly version",
    projects_pin_one_reference_assembly())
chk("1.30.1", "the feature list and what it needs both name the game version the mod is built on and the beta it also runs on",
    the_readme_names_the_game_versions_the_mod_was_checked_against())
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
def csharp_comment_spans(src):
    out, i, n, line = [], 0, len(src), 1
    while i < n:
        c = src[i]
        if c == "\n":
            line += 1; i += 1; continue
        if c == '"':
            if src.startswith('\"\"\"', i):
                j = src.find('\"\"\"', i + 3)
                j = n if j < 0 else j + 3
                line += src.count("\n", i, j); i = j; continue
            i += 1
            while i < n and src[i] != '"':
                if src[i] == "\\": i += 1
                if i < n and src[i] == "\n": line += 1
                i += 1
            i += 1; continue
        if c == "@" and i + 1 < n and src[i + 1] == '"':
            i += 2
            while i < n:
                if src[i] == '"':
                    if i + 1 < n and src[i + 1] == '"': i += 2; continue
                    i += 1; break
                if src[i] == "\n": line += 1
                i += 1
            continue
        if c == "$" and i + 1 < n and src[i + 1] == '"':
            i += 2; depth = 0
            while i < n:
                if src[i] == "\\": i += 2; continue
                if src[i] == "{": depth += 1
                elif src[i] == "}" and depth: depth -= 1
                elif src[i] == '"' and not depth: i += 1; break
                if src[i] == "\n": line += 1
                i += 1
            continue
        if c == "'":
            i += 1
            while i < n and src[i] != "'":
                if src[i] == "\\": i += 1
                i += 1
            i += 1; continue
        if src.startswith("//", i):
            j = src.find("\n", i)
            j = n if j < 0 else j
            out.append(line); i = j; continue
        if src.startswith("/*", i):
            j = src.find("*/", i + 2)
            j = n if j < 0 else j + 2
            out.append(line)
            line += src.count("\n", i, j); i = j; continue
        i += 1
    return out

def tracked_files():
    import subprocess
    listed = subprocess.run(["git", "ls-files", "-z"], capture_output=True)
    if listed.returncode != 0:
        return None
    return [n for n in listed.stdout.decode("utf-8").split("\0") if n]

def no_tracked_source_carries_a_comment():
    import tokenize
    listed = tracked_files()
    if not listed:
        return False
    names = [n for n in listed
             if n.endswith((".cs", ".py", ".xml", ".csproj", ".yml", ".yaml", ".sh"))]
    if not names:
        return False
    found = []
    for name in names:
        try:
            body = io.open(name, encoding="utf-8").read()
        except (IOError, OSError, UnicodeDecodeError):
            return False
        if name.endswith(".cs"):
            found += [(name, ln) for ln in csharp_comment_spans(body)]
        elif name.endswith(".py"):
            try:
                with io.open(name, "rb") as fh:
                    found += [(name, t.start[0]) for t in tokenize.tokenize(fh.readline)
                              if t.type == tokenize.COMMENT]
            except (tokenize.TokenError, IndentationError, SyntaxError):
                return False
        elif name.endswith((".xml", ".csproj")):
            found += [(name, body.count("\n", 0, m.start()) + 1)
                      for m in re.finditer(r"<!--", body)]
        elif name.endswith((".yml", ".yaml", ".sh")):
            found += [(name, i) for i, line in enumerate(body.split("\n"), 1)
                      if line.strip().startswith("#") and not line.strip().startswith("#!")]
    return found == []

chk("1.5.4", "the source carries no comments",
    no_tracked_source_carries_a_comment())

chk("1.5.5", "simulation mode mutates no per-visit state",
    between(sell_pass(),
            "if (sim)", "continue;").count("_soldThisVisit") == 0 and
    between(buy_pass(),
            "if (sim)", "continue;").count("_boughtThisVisit") == 0 and
    "best case" in M and "nothing moves" in M)
chk("1.5.6", "the log prefers the game's user folder over the module folder",
    log_prefers_the_user_folder())
chk("1.5.6", "log path resolution is attempted once, not per line",
    "if (_path == null) return false;" in method_body(S['Support.cs'], "private static bool Ready"))
chk("1.41.6", "a market TradeLord traded in drops only the rankings its own prices decide, and drops them all when its gold has run out",
    a_traded_market_drops_only_the_rankings_its_own_prices_decide())
chk("1.5.6", "live-price mode records no price observations, and a visit that records nothing leaves the market rankings standing",
    capture_skipped_after_the_caches_are_dropped())
chk("1.5.6", "unreadable observations are pruned on both save and load",
    method_body(S['Ledger.cs'], "public override void SyncData").count("PruneExpired();") == 2 and
    "if (!dataStore.IsLoading) PruneExpired();" in S['Ledger.cs'] and
    "if (dataStore.IsLoading) PruneExpired();" in S['Ledger.cs'])
chk("1.5.6", "the message filter is armed only around a game call that talks back",
    the_filter_is_armed_only_around_a_game_call_that_talks())
chk("1.5.6", "every place the filter comes down logs how many messages it suppressed",
    S['Trading.cs'].count("ReportSilenced();") == 3 and
    (lambda b: ordered(b, "AutomatedTradeInProgress = false;", "_transactionDepth = 0;",
                       "_tradingWith = null;", "ReportSilenced();"))
        (method_body(S['Trading.cs'], "private static void InAPass")) and
    "ReportSilenced();" in method_body(S['Trading.cs'], "internal static void ReleaseMessageFilter") and
    "finally { CloseTransaction(); ReportSilenced(); }" in
        method_body(S['Trading.cs'], "private static void CreditTradeSkill") and
    "NoteSilenced(__0?.Information);" in
        method_body(S['Trading.cs'], "internal static class Patch_SilenceChunkedTradeLines") and
    "_silenced[line] = seen + 1;" in method_body(S['Trading.cs'], "internal static void NoteSilenced") and
    'lines.Add("    " + kv.Value + " x " + kv.Key);' in
        method_body(S['Trading.cs'], "private static void ReportSilenced"))
chk("1.6.0", "the message filter uses a depth counter, so nesting cannot disarm it early",
    "internal static bool InGameTransaction => _transactionDepth > 0;" in S['Trading.cs'] and
    "private static void OpenTransaction() => _transactionDepth++;" in S['Trading.cs'] and
    "if (_transactionDepth > 0) _transactionDepth--;" in
        method_body(S['Trading.cs'], "private static void CloseTransaction"))
chk("1.6.0", "an armed message filter is cleared at the start of the next frame",
    'Guard.Run("Tick.ReleaseMessageFilter", TradeActionBehavior.ReleaseMessageFilter)' in S['SubModule.cs'] and
    ordered(S['SubModule.cs'], "TradeActionBehavior.ReleaseMessageFilter", "TradeActionBehavior.FlushToasts") and
    "_transactionDepth = 0;" in method_body(S['Trading.cs'], "internal static void ReleaseMessageFilter"))
chk("1.6.0", "ending a campaign clears the message filter and per-visit state",
    'Guard.Run("GameEnd.Visit", TradeActionBehavior.ForgetVisit)' in S['SubModule.cs'] and
    all(f in method_body(S['Trading.cs'], "internal static void ForgetVisit")
        for f in ("ResetVisit();", "_transactionDepth = 0;", "AutomatedTradeInProgress = false;")))
chk("1.5.6", "a manual purchase is recorded at the price the shelf charged at the time",
    "Bulk.PricePaid(here, element.EquipmentElement, took, unit)" in
        method_body(S['Ledger.cs'], "private void OnPlayerInventoryExchange") and
    "shelf.Restock(units);" in method_body(S['Market.cs'], "internal static int PricePaid") and
    "shelf.Restock(-1);" in method_body(S['Market.cs'], "internal static int PricePaid"))
chk("1.36.2", "the rewind prices the very thing that was bought, quality and all, and a route walk still prices the plain good",
    (lambda paid, shelf, ladder:
        "internal static int PricePaid(Settlement site, EquipmentElement bought, int units, int quotedUnitPrice)"
            in S['Market.cs']
        and "if (site == null || bought.Item == null) return flat;" in paid
        and "new Shelf(site, bought, selling: false, quoted: quotedUnitPrice, projecting: false)" in paid
        and "_element = stocked;" in shelf
        and "new EquipmentElement" not in shelf
        and ": this(site, new EquipmentElement(item), selling, quoted, landed)" in ladder
        and "new Shelf(site, stocked, selling, quoted, projecting: true, landed: landed);" in method_body(
            S['Market.cs'], "internal Ladder(Settlement site, EquipmentElement stocked, bool selling, int quoted, int landed)")
        and S['Market.cs'].count("new EquipmentElement(") == 2
        and "item == null ? 0 : At(market, new EquipmentElement(item), who, selling);" in S['Market.cs'])
    (method_body(S['Market.cs'], "internal static int PricePaid"),
     method_body(S['Market.cs'],
                 "internal Shelf(Settlement site, EquipmentElement stocked, bool selling, int quoted, bool projecting, int landed = 0)"),
     method_body(S['Market.cs'], "internal Ladder(Settlement site, ItemObject item, bool selling, int quoted, int landed)")))
chk("1.5.6", "only the purchase-price rewind reads a shelf outside a projection",
    S['Market.cs'].count("projecting: false") == 1 and
    "projecting: false" in method_body(S['Market.cs'], "internal static int PricePaid") and
    S['Market.cs'].count("projecting: true") == 1 and
    "projecting: true" in method_body(
        S['Market.cs'], "internal Ladder(Settlement site, EquipmentElement stocked, bool selling, int quoted, int landed)"))
chk("1.5.6", "panel setup is retried before being disabled",
    "private const int SetupAttempts = 3;" in S['Panel.cs'] and
    "if (_setupFailures >= SetupAttempts) return false;" in
        method_body(S['Panel.cs'], "private static bool MaySetUp") and
    "if (_setupCooldown > 0) { _setupCooldown--; return false; }" in S['Panel.cs'] and
    "_setupFailures = 0;" in method_body(S['Panel.cs'], "internal static void Reset"))
chk("1.5.6", "an unrecognized hotkey name is logged before falling back to T",
    hotkey_fallback_is_reported())
chk("1.5.6", "the cargo marker only targets a town where the cargo has a price",
    "internal long Value;" in method_body(S['Marker.cs'], "private struct Reckoning") and
    "internal long RunnerUpValue;" in method_body(S['Marker.cs'], "private struct Reckoning") and
    "if (took.Value <= 0L) { how.Refused++; continue; }" in
        method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo"))
chk("1.5.7", "units with no cost basis are still sold when purchased units miss the margin",
    (lambda b: b.count("return false;") == 1 and b.count("return true;") == 1
           and "if (basisIsMarket || paidLeft <= 0 || remaining <= paidLeft) return false;" in b
           and ordered(b, "remaining -= paidLeft;", "paidLeft = 0;", "return true;"))
    (method_body(S['TradeMath.cs'], "public static bool SkipTheUnitsYouPaidFor")) and
    "SkipTheUnitsYouPaidFor" not in S['Ledger.cs'] and
    (lambda b: "if (!basis.SkipTheUnitsYouPaidFor(ref remaining)) break;" in b
           and b.count("tally.Note(Block.BelowMargin)") == 1)
    (sell_pass()) and
    S['Passes.cs'].count("if (!basis.SkipTheUnitsYouPaidFor(ref remaining)) break;") == 1 and
    "TradeMath.SkipTheUnitsYouPaidFor(FromMarket, ref remaining, ref PaidLeft)" in
        between(S['Passes.cs'], "internal bool SkipTheUnitsYouPaidFor(ref int remaining) =>", ";") and
    "Units_you_never_bought_are_still_offered_when_the_bought_ones_miss_the_margin" in MATHTESTS and
    "A_lot_you_paid_for_outright_stops_at_the_margin_and_moves_nothing" in MATHTESTS)
chk("1.5.8", "empty release notes fail the publish, and nothing is appended to the notes",
    "--notes-file release-notes.md" in WORKFLOW and
    empty_release_notes_are_rejected() and
    "Compiled and packaged by CI" not in WORKFLOW and
    "Install: extract the zip" not in WORKFLOW)
chk("1.5.8", "every panel line is localizable",
    prefab_text_is_all_bound() and
    S['Panel.cs'].count('Tongue.Text("{=TL') >= 20)
chk("1.5.8", "tooltip row suffixes carry localization markers",
    'Tongue.Text("{=TL77}Profit: +{PCT}%")' in S['TooltipPatches.cs'] and
    'Tongue.Text("{=TL78}Stock: {COUNT}")' in S['TooltipPatches.cs'] and
    'Tongue.Text("{=TL79}~{DAYS} days")' in S['Travel.cs'])
chk("1.5.8", "each language-file entry matches the source fallback text",
    shipped_text_matches_the_fallback())
chk("1.5.9", "panel-owned map pins survive a save/load cycle",
    'dataStore.SyncData("TradeLord_PanelPins", ref _pinnedTowns);' in S['Trading.cs'] and
    'Guard.Run("Visit.PinsForSave", () => _pinnedTowns = LedgerPanel.PinnedIds());' in
        method_body(S['Trading.cs'], "public override void SyncData") and
    "LedgerPanel.RestorePins(_pinnedTowns)" in
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
    "pass.Moved(profit, goldGained, selling: true);" in
        sell_pass() and
    "if (profit.HasValue) LedgerBehavior.Instance?.AddProfit(profit.Value);" in
        method_body(S['Trading.cs'], "internal void Moved") and
    S['Trading.cs'].count("AddProfit") == 1 and
    "AddProfit" not in buy_pass() and
    "pass.Moved(gold: spent, selling: false);" in
        buy_pass())

chk("1.6.1", "the trade XP the pass earns reaches the game only once the pass is over",
    "_pendingXp += xp;" in method_body(S['Trading.cs'], "private static void AwardTradeXpForOurOwnTrade") and
    "SkillLevelingManager.OnTradeProfitMade" not in
        method_body(S['Trading.cs'], "private static void AwardTradeXpForOurOwnTrade") and
    S['Trading.cs'].count("SkillLevelingManager.OnTradeProfitMade") == 1 and
    "SkillLevelingManager.OnTradeProfitMade" in
        method_body(S['Trading.cs'], "private static void CreditTradeSkill") and
    all("SkillLevelingManager" not in method_body(S['Trading.cs'], m)
        for m in ("public static void ExecuteQuickSell", "public static void ExecuteQuickBuy")))
chk("1.6.1", "the XP line is queued last, in amber, and is translatable",
    'internal static readonly Color Xp = new Color(1f, 0.72f, 0.20f);' in S['Notices.cs'] and
    'Notices.Say(earned, Notices.Xp);' in method_body(S['Trading.cs'], "private static void CreditTradeSkill") and
    ordered(method_body(S['Trading.cs'], "internal static void FlushToasts"),
            "CreditTradeSkill(xp, profit, muted)",
            "Notices.Drain();") and
    ordered(method_body(S['Notices.cs'], "internal static void Drain"),
            "_pending.AddRange(_afterXp);",
            "InformationManager.DisplayMessage") and
    '{=TL81}TradeLord credited {GOLD} denars of profit to your Trade skill.' in S['Trading.cs'] and
    ordered(sell_pass(),
            "Notices.Say(msg, profit > 0",
            "AwardTradeXpForOurOwnTrade(moved.Earned, pass.Muted)"))
chk("1.6.1", "ending a campaign drops trade XP that was queued but not yet handed over",
    "_pendingXp = 0;" in method_body(S['Trading.cs'], "internal static void ForgetVisit"))
chk("1.6.1", "the gold reserve default leaves room for two safe passages and a wage run",
    "public int GoldReserve = 300;" in S['Options.cs'] and
    re.search(r'HintText = "\{=TL\d+\}Never spend below this much gold\. Default 300', M) is not None)

chk("1.6.2", "every per-frame and shutdown call runs inside the crash guard",
    all(f'Guard.Run("{c}"' in S['SubModule.cs'] for c in
        ("Tick.ReleaseMessageFilter", "Tick.FlushToasts", "Tick.Settings",
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
    (lambda b: ordered(b, "Listed(s.NeverSet, good)", "Listed(s.AlwaysSet, good)")
           and ordered(b, "game.Locked()", "Listed(s.AlwaysSet, good)")
           and ordered(b, "Listed(s.AlwaysSet, good)", "PolicyAllows(PolicyFor(good, s)"))
    (sell_rule()))
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
    "if (pass.Reports && tally.Saw(Block.CarryWeight)) _cargoWasFull = true;" in
        buy_pass())
chk("1.6.4", "the full-cargo warning is red, translatable, and not silenced by a quiet pass",
    'internal static readonly Color Alert = new Color(0.90f, 0.28f, 0.28f);' in S['Notices.cs'] and
    'Notices.Say(Tongue.Text("{=TL82}' in method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry") and
    'Notices.Alert)' in method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry") and
    'TL82' in strings_declared() and
    "quiet" not in method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry"))
chk("1.6.4", "the buying warnings are held back where the mod cannot buy, and clear with the visit",
    method_body(S['Trading.cs'], "private void OnSettlementEntered").count("CanTradeHere(settlement)") == 2 and
    method_body(S['Trading.cs'], "private void OnSettlementEntered").count("if (CanTradeHere(settlement) &&") == 1 and
    method_body(S['Trading.cs'], "private void OnSettlementEntered").count(
        "_visitTradeAllowed = CanTradeHere(settlement);") == 1 and
    "(Options.Current.AutoBuyOnEntry || Options.Current.QuickSellMenu)" in
        method_body(S['Trading.cs'], "private void OnSettlementEntered") and
    "_cargoWasFull = false;" in method_body(S['Trading.cs'], "private static void ResetVisit"))

chk("1.6.5", "an item list is parsed once per edit and never left unset",
    "if (set == null || seen != src)" in
        method_body(S['Options.cs'], "private static ItemList Parsed") and
    S['Options.cs'].count("Parsed(") == 5)
chk("1.6.5", "ending a campaign drops trade messages queued but not yet shown",
    "Notices.Forget();" in method_body(S['Trading.cs'], "internal static void ForgetVisit") and
    "_pending.Clear();" in method_body(S['Notices.cs'], "internal static void Forget") and
    "_afterXp.Clear();" in method_body(S['Notices.cs'], "internal static void Forget"))
chk("1.6.5", "the route scan is reused within the hour and dropped with the market rankings",
    "_routes = ScanRoutes();" in method_body(S['Ledger.cs'], "public List<TradeRoute> BestRoutes") and
    "_routes = null;" in method_body(S['Ledger.cs'], "private void ForgetPricedRankings") and
    "ForgetPricedRankings();" in method_body(S['Ledger.cs'], "internal void ForgetMarketRankings") and
    "!Freshness.Fresh(ref _routeStamp, hour)" in S['Ledger.cs'])
chk("1.6.5", "a village keeping its last unit of each good says so",
    "case Block.VillageLastUnit:" in
        method_body(S['Reasons.cs'], "internal static TextObject Phrase") and
    "TL83" in strings_declared())

chk("1.6.6", "the always-sell list governs selling only, never what quick-buy purchases",
    "AlwaysSet," not in buy_rule() and
    "Listed(s.AlwaysSet, good)" in sell_rule() and
    "Listed(s.AlwaysSet, good) ||" in
        between(S['Rules.cs'], "internal static bool ResaleAllowed", ";"))
chk("1.6.6", "the ledger popup builds its route lines from a translatable string",
    '"{=TL84}{ITEM}: buy {FROM}' in S['Trading.cs'] and
    'r.Item.Name + ": buy "' not in S['Trading.cs'] and
    "TL84" in strings_declared())
chk("1.13.0", "a value the settings file carries is never overwritten as the screen loads",
    "internal static bool Loaded;" not in M and "Settings.Loaded" not in M
    and every_setting_keeps_to_its_own_value())

chk("1.6.7", "the Trade XP line reports the denars of profit it hands the skill system, the number it actually passes",
    (lambda b: 'earned.SetTextVariable("GOLD", xp);' in b
           and "SkillLevelingManager.OnTradeProfitMade(Hero.MainHero, xp);" in b)
    (method_body(S['Trading.cs'], "private static void CreditTradeSkill")) and
    "Trade XP." not in S['Trading.cs'] and
    "trade profit fed to the XP system: " in S['Trading.cs'])
chk("1.6.7", "the queued trade messages are dropped even if one of them cannot be shown",
    "finally { _pending.Clear(); }" in method_body(S['Notices.cs'], "internal static void Drain") and
    method_body(S['Notices.cs'], "internal static void Drain").count("_pending.Clear()") == 1)
chk("1.6.7", "a good already bought here is passed over before the food reserve is spent on it",
    (lambda b: ordered(b, "books.Bought(sim, market.IdAt(at))", "market.MaySell(at, good,"))
    (method_body(S['Passes.cs'], "internal static Traded SellThem")))
chk("1.6.7", "the panel's own pin list, not the map's marker state, decides what a click on a town pins and unpins",
    (lambda b: ordered(b, "if (Unpin(settlement)) return;", "_panelPins.Add(settlement)"))
    (method_body(S['Panel.cs'], "private static void ToggleMarker")) and
    (lambda b: ordered(b, "if (settlement == null || !_panelPins.Remove(settlement)) return false;",
                       "tracker.CheckTracked(settlement)) tracker.RemoveTrackedObject(settlement);",
                       "return true;"))
    (method_body(S['Panel.cs'], "internal static bool Unpin")) and
    S['Panel.cs'].count("_panelPins.Remove(") == 1 and
    "LedgerPanel.IsPinned(_tracked)" in S['Marker.cs'])

chk("1.90.1", "the map button reserves the mouse over the button and nowhere else, with no guessed region standing in for it",
    "OverTheStripInstead" not in S['Rules.cs'] and "OverAssumedBounds" not in S['Panel.cs'] and
    "0.90f" not in S['Rules.cs'] and
    "0.90f" not in method_body(S['Panel.cs'], "private static bool OverButtonBounds") and
    "0.90f" not in method_body(S['Panel.cs'], "private static Widget TheMapButton") and
    "if (button == null) return false;" in
        method_body(S['Panel.cs'], "private static bool OverButtonBounds") and
    "No_part_of_the_map_is_reserved_when_the_button_cannot_be_measured" in MAPBUTTONTESTS)
chk("1.6.8", "the food reserve is spent only on goods the sell rules would actually move",
    (lambda b: ordered(b, "said.Why = Block.NotTradable; return said;",
                       "int reserved = DrawKeepBack(amount - said.KeepCount, facts.FoodHeld, out bool fed);"))
    (sell_rule()))
chk("1.6.8", "another mod handles its own notification before TradeLord may hold one back",
    "[HarmonyPriority(Priority.Last)]" in
        method_body(S['Trading.cs'], "internal static class Patch_SilenceChunkedTradeLines"))

chk("1.6.9", "every setting name, hint and group heading carries a translation marker",
    every_setting_line_is_translatable())

chk("1.6.10", "the button's own measured size decides the reserved region, so it holds at any aspect ratio",
    (lambda b: "Screen.RealScreenResolutionWidth" in b and "button.ScaledSuggestedWidth" in b
           and "button.ScaledMarginRight" in b and "0.90f" not in b
           and "return MapButton.Over(m.x, m.y," in b)
    (method_body(S['Panel.cs'], "private static bool OverButtonBounds")) and
    "The_region_holds_at_any_aspect_ratio" in MAPBUTTONTESTS)
chk("1.6.10", "the prefab carries the id the panel looks the button up by",
    'Id="TradeLordMapButton"' in PREFAB and 'MapButtonId = "TradeLordMapButton"' in S['Panel.cs'])
chk("1.6.10", "the button still sits flush right and centred, which is what the reserved region assumes",
    re.search(r'Id="TradeLordMapButton"[\s\S]{0,400}?HorizontalAlignment="Right"', PREFAB) is not None and
    re.search(r'Id="TradeLordMapButton"[\s\S]{0,400}?VerticalAlignment="Center"', PREFAB) is not None)
chk("1.90.1", "a button the panel has not read yet reserves nothing, and the panel keeps looking for it rather than giving up after one try",
    (lambda hunt: hunt
        and "if (_mapButton != null) return _mapButton;" in hunt
        and "if (_huntIn > 0) { _huntIn--; return null; }" in hunt
        and "_huntIn = BetweenButtonHunts;" in hunt
        and "_mapButton = FindMapButton(_layer.UIContext?.Root);" in hunt)
    (method_body(S['Panel.cs'], "private static Widget TheMapButton")) and
    "Widget button = TheMapButton();" in
        method_body(S['Panel.cs'], "private static bool OverButtonBounds") and
    S['Panel.cs'].count("FindMapButton(") == 2 and
    "_mapButton = null; _huntIn = 0;" in method_body(S['Panel.cs'], "internal static void Cleanup"))

chk("1.6.11", "a purchase record with nothing left in it is dropped rather than saved forever",
    re.search(r'PruneSettledPurchases\(\) =>\s*_purchases\?\.RemoveAll\(rec => rec == null \|\| '
              r'rec\.ItemId == null \|\| rec\.Count <= 0\);', S['Ledger.cs']) is not None)
chk("1.6.11", "the purchase index is rebuilt after a prune, never left pointing at dropped records",
    (lambda b: ordered_last(b, "PruneExpired();", 'Guard.Run("Ledger.Reindex", Reindex);'))
    (method_body(S['Ledger.cs'], "public override void SyncData")))
chk("1.6.11", "every reader of a purchase record already requires units left, so dropping a spent one changes nothing",
    all("rec.Count > 0" in line for line in
        [method_body(S['Ledger.cs'], "public bool HasPurchaseRecord"),
         method_body(S['Ledger.cs'], "public int PurchasedUnits")])
    and "rec.Count <= 0) return NoRecordedBasis;" in
        method_body(S['TradeMath.cs'], "public static int UnitBasis")
    and "TradeMath.UnitBasis(rec, Options.Current.CostBasisMode);" in
        method_body(S['Ledger.cs'], "public int GetCostBasis"))
chk("1.6.11", "each pruning step stays independent of the others, so a spent record still goes",
    re.search(r'private void Prune\(\)\s*\{\s*PruneObservations\(\);\s*TrimToWhatItKeeps\(\);'
              r'\s*PruneSettledPurchases\(\);\s*TrimThePromisesKept\(\);\s*\}',
              S['Ledger.cs']) is not None and
    "_promises" not in method_body(S['Ledger.cs'], "private void PruneObservations") and
    "_promises" not in method_body(S['Ledger.cs'], "private void TrimToWhatItKeeps") and
    "Kept.MostPricesKept" not in method_body(S['Ledger.cs'], "private void TrimThePromisesKept") and
    "PruneSettledPurchases" not in method_body(S['Ledger.cs'], "private void PruneObservations") and
    "Kept.MostPricesKept" not in method_body(S['Ledger.cs'], "private void PruneObservations") and
    "ObservationShelfLifeDays" not in method_body(S['Ledger.cs'], "private void TrimToWhatItKeeps") and
    "ObservationShelfLifeDays" not in method_body(S['Ledger.cs'], "private void PruneSettledPurchases"))

def what_left_without_a_sale_stops_counting_as_bought():
    body = method_body(S['Ledger.cs'], "private void MatchPurchasesToWhatIsHeld")
    return ("CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);" in
                method_body(S['Ledger.cs'], "public override void RegisterEvents")
            and 'private void OnDailyTick() => Guard.Run("Ledger.OnDailyTick", () =>' in S['Ledger.cs']
            and "MatchPurchasesToWhatIsHeld();" in method_body(S['Ledger.cs'], "private void OnDailyTick")
            and "ItemRoster carried = MobileParty.MainParty?.ItemRoster;" in body
            and "if (rec.Count <= have) continue;" in body
            and "TradeMath.DrainSale(rec, gone);" in body
            and "rec.TotalPaid" not in body
            and "rec.Count =" not in body)

chk("1.37.3", "a good that left the party without a sale stops counting as bought, so what you paid never outlives what you hold",
    what_left_without_a_sale_stops_counting_as_bought())

def a_good_leaving_the_party_is_noticed_as_it_goes():
    regs = method_body(S['Ledger.cs'], "public override void RegisterEvents")
    watch = method_body(S['Ledger.cs'], "private void WatchTheParty")
    left = method_body(S['Ledger.cs'], "private void WhatLeftTheParty")
    tick = method_body(S['Ledger.cs'], "private void OnTick")
    return ("CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);" in regs
            and "CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);" in regs
            and "carried.RosterUpdatedEvent += WhatLeftTheParty;" in watch
            and "_watched.RosterUpdatedEvent -= WhatLeftTheParty;" in watch
            and "if (count < 0) _settle = true;" in left
            and "MatchPurchasesToWhatIsHeld" not in left
            and "DrainSale" not in left
            and ordered(tick, "if (!_settle) return;", "_settle = false;", "MatchPurchasesToWhatIsHeld")
            and "OnTick" not in S['SubModule.cs'])

chk("1.37.4", "a good leaving the party is noticed as it goes, and the books are settled a frame later so a sale is never counted twice",
    a_good_leaving_the_party_is_noticed_as_it_goes())

chk("1.6.12", "goods with no price paid are credited at what the cheapest market would have charged",
    (lambda b: "BestBuy(item)" in b and "item.Value" in b)
    (method_body(S['Policy.cs'], "internal static int UnpaidWorth")))
chk("1.6.12", "a paid unit is still credited exactly as before, against what was paid for it",
    "if (basis > 0) return proceeds - basis;" in
        method_body(S['TradeMath.cs'], "public static int Credit"))
chk("1.6.12", "an unpaid unit sold below that worth credits nothing rather than a loss",
    "return gain > 0 ? gain : 0;" in
        method_body(S['TradeMath.cs'], "public static int Credit"))
chk("1.6.12", "the simulated pass and the real one credit profit through the same rule",
    S['Trading.cs'].count("TradePolicy.Credit(") == 1 and
    "TradePolicy.Credit(price, worth, basis.UnpaidWorth)" in
        method_body(S['Trading.cs'], "public static void ExecuteHerdRelief") and
    "TradeMath.Credit(proceeds, basis, unpaidWorth);" in S['Policy.cs'] and
    S['Passes.cs'].count("TradeMath.Credit(price, worth, basis.UnpaidWorth)") == 1 and
    S['Passes.cs'].count("TradeMath.Credit(proceeds, worth, basis.UnpaidWorth)") == 1 and
    method_body(S['Passes.cs'], "internal static Traded SellThem").count("TradeMath.Credit(") == 2)
chk("1.6.12", "what quick-sell agrees to sell runs through one margin rule, and the credit still reads the bare basis",
    S['Passes.cs'].count("if (!TradeMath.ProfitAcceptable(mustBeat, price, s.MinProfitMargin))") == 1 and
    S['Passes.cs'].count("int worth = basis.Unit(out bool askTheMarket);") == 1 and
    S['Trading.cs'].count("int worth = basis.Unit(out bool askTheMarket);") == 1 and
    "ProfitAcceptable(basis.UnpaidWorth" not in S['Trading.cs'] + S['Passes.cs'] and
    "TradePolicy.Credit(price, worth, basis.UnpaidWorth)" in S['Trading.cs'] and
    "TradeMath.ProfitAcceptable(costBasis, townSellPrice, Options.Current.MinProfitMargin);" in S['Policy.cs'] and
    re.search(r'ProfitAcceptable\(int costBasis, int townSellPrice, float margin\) =>\s*costBasis > 0\s*\?\s*'
              r'townSellPrice >= costBasis \* \(1f \+ margin\)\s*:\s*'
              r'townSellPrice > 0;', S['TradeMath.cs']) is not None)
chk("1.21.0", "loot with the hold switched off goes to the first market that can pay, since nothing but that switch raises the floor",
    S['Passes.cs'].count("if (s.PreferBestSellTown)") == 1 and
    "if (s.PreferBestSellTown)" in
        sell_pass() and
    "s.BestSellTownTolerance" in
        between(S['Passes.cs'], "if (s.PreferBestSellTown)",
                "int price = market.PriceToSell(at);") and
    "return best.Item1 != null && best.Item1 != _pass.Site;" in S['Trading.cs'] and
    S['Passes.cs'].count("s.BestSellTownTolerance") == 1)
chk("1.6.12", "that worth is looked up once per good, not once per unit sold",
    "askTheMarket = worth == 0 && UnpaidWorth < 0;" in
        method_body(S['Passes.cs'], "internal int Unit(out bool askTheMarket)") and
    "if (askTheMarket) basis.UnpaidWorth = market.UnpaidWorth(at);" in
        method_body(S['Passes.cs'], "internal static Traded SellThem") and
    "basis.UnpaidWorth = -1;" in method_body(S['Passes.cs'], "internal static Basis For") and
    pass_body("public static void ExecuteQuickSell").count("TradePolicy.UnpaidWorth(") == 1 and
    "int worth = basis.Unit(out bool askTheMarket);" in
        sell_pass())
chk("1.6.12", "the tooltip and the sale summary now value an unbought good the same way",
    "var best = BestBuy(item);" in method_body(S['Ledger.cs'], "public int GetCostBasis") and
    "best.price > 0 ? best.price : item.Value" in method_body(S['Ledger.cs'], "public int GetCostBasis") and
    "best.Item2 > 0 ? best.Item2 : item.Value" in method_body(S['Policy.cs'], "internal static int UnpaidWorth"))

def mcm_generation_matches_the_package():
    pkg = re.search(r'Bannerlord\.MCM"\s+Version="(\d+)\.', "\n".join(PROJ))
    code = re.search(r'McmGeneration = (\d+);', S['Support.cs'])
    return bool(pkg) and bool(code) and pkg.group(1) == code.group(1)

chk("1.6.13", "the MCM line the loader expects is the one the settings companion is built against",
    mcm_generation_matches_the_package())
chk("1.6.13", "no MCM line is written into the loader by hand, so bumping the package moves it",
    '"MCMv5"' not in S['Support.cs'] and '"MCMv5"' not in S['Rules.cs']
    and 'internal const string Family = "MCMv";' in S['Rules.cs']
    and 'internal static string Named(int generation) => Family + generation;' in S['Rules.cs']
    and 'private static string Named(int generation) => Screens.Named(generation);' in S['Support.cs'])
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
    (lambda b: "while (end < name.Length && char.IsDigit(name[end])) end++;" in b
           and "return end > Family.Length ? name.Substring(0, end) : null;" in b
           and "int end = Family.Length;" in b)
    (method_body(S['Rules.cs'], "internal static string GenerationOf")))
chk("1.6.13", "an already-loaded usable line wins over a newer one, so the settings screen still opens",
    (lambda b: ordered(b, "return generation;", "if (other == null) other = generation;")
           and "return other;" in b)
    (method_body(S['Rules.cs'], "internal static string Which"))
    and ordered(method_body(S['Support.cs'], "private static string Detect"),
                "string found = Screens.Which(LoadedNames(), Named(McmGeneration));",
                "if (found != null) return found;"))
chk("1.6.13", "a line newer than this build is still found when nothing has loaded it yet",
    "g <= McmGeneration + GenerationsAhead" in method_body(S['Support.cs'], "private static string Detect"))

chk("1.6.14", "the auto-marker claims a town only when it placed the marker itself, so it never removes one you set",
    len(re.findall(r'if \(target != null && !tracker\.CheckTracked\(target\)\)\s*\{\s*'
                   r'tracker\.RegisterObject\(target\);\s*_tracked = target;\s*\}',
                   method_body(S['Marker.cs'], "internal static void Update"))) == 2 and
    "_tracked = target;" not in between(method_body(S['Marker.cs'], "internal static void Update"),
                                        "tracker.RemoveTrackedObject(_tracked);", "if (target != null") and
    "if (_tracked != null && !LedgerPanel.IsPinned(_tracked) && tracker.CheckTracked(_tracked))" in S['Marker.cs'])
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
    (lambda b: "Freshness.Fresh(ref _capturedStamp)) return;" in b
           and ordered(b, "Freshness.Fresh(ref _capturedStamp)", "Freshness.Taken(ref _capturedStamp);"))
    (method_body(S['Ledger.cs'], "public void CaptureSettlement")))

chk("1.6.15", "an unreadable price observation is dropped",
    (lambda b: "if (_ledger == null) return;" in b
           and "seen.Value == null || seen.Value.TownId == null" in b)
    (method_body(S['Ledger.cs'], "private void PruneObservations")))
chk("1.6.15", "an item whose observations have all gone is dropped from the save",
    (lambda b: "if (kv.Value == null) { spent.Add(kv.Key); continue; }" in b
           and "if (kv.Value.Count == 0) spent.Add(kv.Key);" in b
           and "_ledger.Remove(spent[i]);" in b and "shelf <= 0f" not in b)
    (method_body(S['Ledger.cs'], "private void PruneObservations")))
chk("1.6.15", "each market that trades nothing is named in the log, not just the first with those reasons",
    S['Trading.cs'].count('Log.Repeatable(label + "-empty " + pass.Key') == 2 and
    all('Log.Repeatable(label + "-empty " + pass.Key' in
            method_body(S['Trading.cs'], one)
        for one in ("private static void SellPass", "private static void BuyPass")) and
    "internal string Key => Site != null ? Site.StringId : Met.StringId;" in S['Trading.cs'] and
    S['Trading.cs'].count('SellPass(Pass.Open(settlement, quiet), "quick-sell"') == 1 and
    S['Trading.cs'].count('BuyPass(Pass.Open(settlement, quiet), "quick-buy"') == 1)
chk("1.6.15", "ending a campaign clears what the log has already reported, so the next one reports it again",
    "_repeats.Clear();" in method_body(S['Support.cs'], "internal static void Forget") and
    "_errors.Clear();" in method_body(S['Support.cs'], "internal static void Forget") and
    'Guard.Run("GameEnd.Log", Log.Forget)' in S['SubModule.cs'])

chk("1.6.16", "holding cargo for a better market is named as its own reason, not as the profit margin",
    (lambda b: b.count("case Block.BelowBestMarket:") == 1
           and '{=TL85}' in b and '{=TL42}' in b
           and ordered(b, "case Block.BelowMargin:", '{=TL42}', "case Block.BelowBestMarket:"))
    (method_body(S['Reasons.cs'], "internal static TextObject Phrase")) and
    "TL85" in strings_declared())
def the_herd_guard_is_re_armed_and_says_once_why_it_is_off():
    forget = method_body(S['Trading.cs'], "internal static void ForgetVisit")
    model = method_body(S['Drove.cs'], "private static DefaultPartySpeedCalculatingModel Model")
    room = method_body(S['Drove.cs'], "internal static int RoomForLivestock")
    shed = method_body(S['Drove.cs'], "internal static int AnimalsToShed")
    return ("Drove.Forget();" in forget
            and "_modifier = null" not in forget
            and "if (_lookupFailed) return null;" in model
            and model.count("_lookupFailed = true;") == 2
            and "a mod replaced the party speed model" in model
            and "GetHerdingModifier not found on this game version" in model
            and S['Drove.cs'].count(
                'typeof(DefaultPartySpeedCalculatingModel).GetMethod(') == 1
            and "DefaultPartySpeedCalculatingModel model = Model();" in room
            and "DefaultPartySpeedCalculatingModel model = Model();" in shed
            and "if (model == null) return 0;" in room and "if (model == null) return 0;" in shed)

chk("1.6.16", "a campaign starts with the herd guard re-armed, and whichever part of the mod needs it first says once why it is off",
    the_herd_guard_is_re_armed_and_says_once_why_it_is_off())
chk("1.6.16", "a full herd is named as its own reason, not as a full cargo hold",
    (lambda b: b.count("case Block.HerdFull:") == 1
           and '{=TL86}' in b and '{=TL44}' in b
           and ordered(b, "case Block.CarryWeight:", '{=TL44}', "case Block.HerdFull:"))
    (method_body(S['Reasons.cs'], "internal static TextObject Phrase")) and
    "TL86" in strings_declared() and
    "if (pass.Reports && tally.Saw(Block.CarryWeight)) _cargoWasFull = true;" in S['Trading.cs'])
chk("1.6.16", "the auto-marker is put back on the map when a save loads, and cannot cost the menus if it fails",
    (lambda b: 'Guard.Run("Action.RestorePins", () => LedgerPanel.RestorePins(_pinnedTowns));' in b
           and 'Guard.Run("Action.RestoreMarker", Marker.Update);' in b
           and ordered(b, 'Guard.Run("Action.RestorePins"', 'Guard.Run("Action.RestoreMarker"', 'AddOptions("town");'))
    (method_body(S['Trading.cs'], "private void OnSessionLaunched")))
def the_first_market_of_a_campaign_trades_like_every_other_one():
    entered = method_body(S['Trading.cs'], "private void OnSettlementEntered")
    return ("AnnounceAutomation" not in ALL
            and "_announcedAutomation" not in ALL
            and "TradeLord_AutomationNotice" not in ALL
            and "starting at the next one" not in ALL
            and not any(sid in strings_declared() for sid in ("TL87", "TL96"))
            and ordered(entered, "NoteThisArrival(settlement);",
                        "ExecuteQuickSell(settlement, quiet: true);",
                        "ExecuteQuickBuy(settlement, quiet: true);",
                        "WarnNoRoomToCarry()"))

chk("1.74.0", "the first market a campaign walks into trades like every other one, with no notice held over it",
    the_first_market_of_a_campaign_trades_like_every_other_one())
chk("1.6.18", "every variable a shipped line leaves a slot for is filled in by name",
    every_text_variable_is_supplied())
chk("1.13.2", "nothing is written down about MCM that nothing reads back",
    "SettingsReachable" not in ALL and "SettingsReachable" not in M)
chk("1.13.0", "the README counts what goes into a save as the source actually saves it",
    the_readme_counts_the_saved_values_right())
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
                       "_ledgerText = LedgerCodec.WriteLedger(Listed(_ledger));",
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
chk("1.68.0", "a saved price costs the same eight fields every time, so a ninth cannot be added to every campaign's save unnoticed",
    a_saved_price_costs_the_same_eight_fields_every_time())
chk("1.6.21", "a visit that traded something is not then told its cargo is full",
    the_full_cargo_warning_waits_for_a_visit_that_traded_nothing())
chk("1.6.21", "the trade skill gain is reported once, in TradeLord's own line",
    the_trade_skill_gain_is_reported_in_one_line())
chk("1.6.22", "a buy cap of zero turns the cap off instead of stopping every purchase",
    a_zero_cap_never_means_buy_nothing())
chk("1.6.22", "every numeric setting that switches off at zero says so on its own label",
    every_numeric_setting_that_switches_off_at_zero_says_so())
chk("1.12.0", "a pass that moved nothing names the rule that stopped it, on an automatic pass too",
    a_silent_pass_still_names_what_stopped_it())
chk("1.12.1", "a market that traded nothing says so once, after both passes have run",
    a_market_that_traded_nothing_is_reported_once())
chk("1.12.1", "a market that traded something is never also told nothing moved",
    a_market_that_traded_something_drops_the_empty_lines())
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
chk("1.36.0", "a commit that changes the feature list is refused unless the changelog gains an entry saying what changed in it",
    "grep -qx 'README.md'" in WORKFLOW and
    "the changelog gains no entry saying what changed" in WORKFLOW and
    r"grep -q '^+- '" in WORKFLOW and
    r"grep -qi '^+.*feature list'" not in WORKFLOW and
    WORKFLOW.index("grep -qx 'README.md'") < WORKFLOW.index("grep -qx 'CHANGELOG.md'"))
chk("1.34.0", "the build runs the compatibility tool, on the built assemblies, against the other game version the feature list claims",
    "dotnet run --project tools/compat" in WORKFLOW and
    GAME_VERSION_BETA + "-beta" in WORKFLOW and
    WORKFLOW.index("dotnet build mcm/TradeLord.MCM.csproj") <
    WORKFLOW.index("dotnet run --project tools/compat") <
    WORKFLOW.index("Assemble the module folder"))
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

def the_purse_is_the_last_reason_named():
    body = method_body(S['Passes.cs'], "internal Block Dominant")
    return ("kv.Key != Block.BudgetSpent" in body
            and "if (top == Block.None && Saw(Block.BudgetSpent)) return Block.BudgetSpent;" in body
            and ordered(body, "foreach (var kv in _counts)",
                        "if (top == Block.None && Saw(Block.BudgetSpent)) return Block.BudgetSpent;"))

def an_empty_purse_is_reported_on_the_way_into_a_market():
    body = method_body(S['Trading.cs'], "private static bool WarnPurseBelowReserve")
    entered = method_body(S['Trading.cs'], "private void OnSettlementEntered")
    return ("TradedThisVisit()" not in body
            and "int held = GoldHeldBack(), flat = Options.Current.GoldReserve;" in body
            and "if (Hero.MainHero.Gold + Visit.Purse(Simulating) - held > 0) return false;" in body
            and "if (TradedThisVisit()) return;" in
                method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry")
            and "Tongue.Text(held > flat" in body
            and '{=TL92}' in body
            and 'Notices.Say(msg, Notices.Alert);' in body
            and 'TL92' in strings_declared()
            and "quiet" not in body and "Muted(" not in body
            and "if (!WarnPurseBelowReserve()) WarnNoRoomToCarry();" in entered
            and ordered(entered, "ExecuteQuickBuy(settlement, quiet: true)",
                        "WarnPurseBelowReserve()"))

def what_is_left_to_spend_is_worked_out_in_one_place():
    return ("internal static int Spendable(Books books, bool sim) =>\n"
            "            TradeMath.Budget(Hero.MainHero.Gold + books.Purse(sim), GoldHeldBack(),\n"
            "                             Options.Current.MaxSpendPerVisit, books.PaidOut(sim));"
                in S['Trading.cs']
            and "internal static int PurseForAVisit() =>\n"
                "            TradeMath.Budget(Hero.MainHero.Gold, GoldHeldBack(),\n"
                "                             Options.Current.MaxSpendPerVisit, 0);"
                in S['Trading.cs']
            and "internal int Spendable() => TradeActionBehavior.Spendable(Books, Sim);"
                in S['Trading.cs']
            and S['Trading.cs'].count("TradeMath.Budget(") == 2
            and S['Trading.cs'].count("GoldHeldBack()") == 5
            and S['Trading.cs'].count("int Budget() =>") == 0
            and "Hero.MainHero.Gold" not in S['Ledger.cs']
            and "GoldReserve" not in S['Ledger.cs']
            and "Options.Current.GoldReserve" not in
                buy_pass())

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
    return ("StringComparer.OrdinalIgnoreCase" in S['Options.cs'] and
            "A_name_is_matched_whatever_its_capitalisation" in ROUTETESTS)

def an_item_list_is_matched_by_name_as_well_as_by_id():
    listed = between(S['Rules.cs'], "internal static bool Listed(ItemList list, in Good good) =>", ";")
    describe = method_body(S['Policy.cs'], "internal static Good Describe")
    return ("list.HasId(good.Id)" in listed and
            "list.HasName(good.Name)" in listed and
            "good.Name != null" in listed and
            "list.HasId(" not in S['Policy.cs'] and
            "list.HasName(" not in S['Policy.cs'] and
            ordered(describe, "if (AnyListNamesAGood(Options.Current))",
                    "ReadTheGoodsInThisGame();",
                    "good.Name = SpokenName(item);") and
            ordered(method_body(S['Policy.cs'], "private static string SpokenName"),
                    "if (_spoken.TryGetValue(item, out string said)) return said;",
                    "said = item.Name == null ? null : item.Name.ToString();",
                    "_spoken[item] = said;") and
            "_spoken.Clear();" in
                method_body(S['Policy.cs'], "internal static void ForgetItemListAudit"))

def a_written_word_stands_for_an_id_and_never_for_another_goods_name():
    read = method_body(S['Options.cs'], "public void ReadWordsAsIds")
    return ("public bool HasId(string id) => !Empty && (Entries.Contains(id) || _words.Contains(id));"
                in S['Options.cs']
            and "public bool HasName(string shown) => !Empty && Entries.Contains(shown);"
                in S['Options.cs']
            and ordered(read, "_words.Clear();",
                        "if (!knownIds.Contains(word)) { everyWordKnown = false; break; }",
                        "if (!everyWordKnown) continue;",
                        "foreach (string word in words) _words.Add(word);")
            and "if (!ids.Contains(word))" in method_body(S['Policy.cs'], "private static bool Unmatched")
            and S['Policy.cs'].count(".ReadWordsAsIds(_knownIds);") == 4
            and "Naming_one_good_never_catches_another_whose_name_is_a_word_of_it" in ROUTETESTS
            and "A_good_named_the_way_the_game_shows_it_never_catches_another_good" in ROUTETESTS
            and "A_written_name_holds_nothing_until_the_goods_in_this_game_are_read" in ROUTETESTS
            and "Ids_written_the_old_way_with_spaces_between_them_still_read" in ROUTETESTS)

def no_rule_compares_a_list_against_an_id_by_hand():
    return not re.search(r'(NeverSet|AlwaysSet|NeverBuySet)\.Contains\(', S['Trading.cs'])

def a_list_entry_survives_the_space_inside_a_name():
    parsed = method_body(S['Options.cs'], "private static ItemList Parsed")
    return ("Split(EntryMarks" in parsed and "built.Entries.Add(whole)" in parsed and
            "Split(WordMarks" in parsed and
            "if (words.Length > 1) built.Spelled.Add(words);" in parsed and
            "' '" not in parsed)

def a_list_entry_that_names_nothing_is_reported_both_ways():
    audit = method_body(S['Policy.cs'], "internal static bool ItemListsNameNothing")
    warn = method_body(S['Trading.cs'], "private static void WarnUnmatchedItemLists")
    return ("ReadTheGoodsInThisGame();" in audit and audit.count("Unmatched(") == 4 and
            "Items.All" in method_body(S['Policy.cs'], "private static void ReadTheGoodsInThisGame") and
            "Log.Write" in method_body(S['Policy.cs'], "private static bool Unmatched") and
            "TradePolicy.ItemListsNameNothing()" in warn and "Notices.Say(" in warn and
            "WarnUnmatchedItemLists();" in method_body(S['Trading.cs'], "private void OnSettlementEntered"))

def the_list_audit_is_redone_when_the_lists_are_edited():
    audit = method_body(S['Policy.cs'], "internal static bool ItemListsNameNothing")
    return ("Freshness.Fresh(ref _auditStamp, Stamp.Timeless)" in audit and
            "TradePolicy.ForgetItemListAudit();" in method_body(S['Trading.cs'], "internal static void ForgetVisit"))

def a_list_still_naming_nothing_after_an_edit_is_said_again():
    warn = method_body(S['Trading.cs'], "private static void WarnUnmatchedItemLists")
    return ("if (!TradePolicy.ItemListsNameNothing()) return;" in warn
            and "_auditSpoke" not in S['Trading.cs']
            and "AuditShouldSpeak" not in S['Trading.cs'])

def the_audit_reads_the_game_only_for_a_list_with_something_in_it():
    audit = method_body(S['Policy.cs'], "internal static bool ItemListsNameNothing")
    return ordered(audit, "string.IsNullOrEmpty(s.NeverSellItems)",
                   "string.IsNullOrEmpty(s.AlwaysBuyItems)) return false;",
                   "ReadTheGoodsInThisGame();")

def quiet_automation_silences_only_the_automated_lines():
    sell = sell_pass()
    buy = buy_pass()
    credit = method_body(S['Trading.cs'], "private static void CreditTradeSkill")
    return ("automated && Options.Current.QuietAutomation" in
                method_body(S['Trading.cs'], "private static bool Muted") and
            "TradeActionBehavior.Muted(Quiet)" in
                between(S['Trading.cs'], "internal bool Muted =>", ";") and
            "if (!pass.Muted) Notices.Say(msg, profit > 0" in sell and
            "if (!pass.Muted) Notices.Say(msg, Notices.Spend);" in buy and
            "if (!muted) Notices.Say(earned, Notices.Xp);" in credit and
            "AwardTradeXpForOurOwnTrade(moved.Earned, pass.Muted);" in sell)

def quiet_automation_leaves_the_cargo_warning_alone():
    return "Muted(" not in method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry")

def a_second_campaign_starts_the_panel_from_scratch():
    reset = method_body(S['Panel.cs'], "internal static void Reset")
    return all(f in reset for f in
               ("_loggedArmed = false;", "_loggedButtonMissing = false;",
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
chk("1.6.26", "quiet automation leaves the cargo warning speaking",
    quiet_automation_leaves_the_cargo_warning_alone())
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

chk("1.6.29", "an empty purse is the last reason a stalled pass names, so any other rule that held a good back is named ahead of it",
    the_purse_is_the_last_reason_named())
chk("1.6.29", "a purse at or under the reserve is reported on the way into a market, past a quiet pass",
    an_empty_purse_is_reported_on_the_way_into_a_market())
chk("1.6.29", "what is left to spend is worked out in one place for both the warning and the buying",
    what_is_left_to_spend_is_worked_out_in_one_place())

def a_tie_between_reasons_is_broken_the_same_way_every_time():
    dominant = method_body(S['Passes.cs'], "internal Block Dominant")
    summary = method_body(S['Passes.cs'], "internal string Summary")
    return ("kv.Value == best && kv.Key < top" in dominant and
            "x.Key.CompareTo(y.Key)" in summary)

def the_party_speeds_are_read_once_an_hour():
    body = method_body(S['Travel.cs'], "private static void Speeds")
    return ("if (hour == _speedHour) { land = _landSpeed; sea = _seaSpeed; return; }" in body and
            "_speedHour = -1;" in method_body(S['Travel.cs'], "internal static void Forget") and
            "_speedHour = -1;" in method_body(S['Travel.cs'], "private static void DropIfNavalChanged"))

chk("1.6.30", "two reasons that stopped as much as each other are ranked the same way on every pass",
    a_tie_between_reasons_is_broken_the_same_way_every_time())
chk("1.6.30", "the party speed behind every travel estimate is read once an hour, not once per estimate",
    the_party_speeds_are_read_once_an_hour())

def the_cargo_marker_counts_the_town_till():
    return ("long total = took.Value > gold ? gold : took.Value;" in
            method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo"))

chk("1.6.31", "the cargo marker never points at a market that cannot pay for the cargo",
    the_cargo_marker_counts_the_town_till())

def the_spend_cap_is_walked_not_divided():
    walk = method_body(S['Market.cs'], "internal static RouteQuote Walk")
    return ("int maxUnits, int merchantTill, int spendCap," in S['Market.cs']
            and "if (spendCap > 0 && q.BuyTotal + buyPrice > spendCap) break;" in walk
            and ordered(walk, "q.BuyTotal + buyPrice > spendCap", "q.BuyTotal += buyPrice;")
            and "Options.Current.MaxSpendPerVisit / buyPrice" not in S['Ledger.cs']
            and "Options.Current.BuyValueCapPerItem / buyPrice" not in S['Ledger.cs'])

ENGLISH = 'TradeLord/ModuleData/Languages/module_strings.xml'
TRANSLATIONS = {
    'T\u00fcrk\u00e7e': 'TradeLord/ModuleData/Languages/TR/module_strings_tr.xml',
    '\u0420\u0443\u0441\u0441\u043a\u0438\u0439': 'TradeLord/ModuleData/Languages/RU/module_strings_ru.xml',
    '\u7b80\u4f53\u4e2d\u6587': 'TradeLord/ModuleData/Languages/CNs/module_strings_cns.xml',
}

def spoken(path):
    import xml.etree.ElementTree as ET
    return {e.get('id'): e.get('text') for e in ET.parse(path).getroot().iter('string')}

def every_translation_says_everything_the_english_one_does():
    en = spoken(ENGLISH)
    if len(en) <= 150:
        return False
    marks = {k for k, said in en.items()
             if said is not None and len(said) == 1 and not said.isalnum()}
    if len(marks) > 1:
        return False
    for tag, path in TRANSLATIONS.items():
        said = spoken(path)
        if set(en) != set(said):
            return False
        if not all(said[k] and said[k].strip() for k in en):
            return False
        if not all(said[k] != en[k] for k in en if k not in marks):
            return False
        if not all(len(said[k]) == 1 and not said[k].isalnum() for k in marks):
            return False
        if '<tag language="' + tag + '"/>' not in io.open(path, encoding='utf-8').read():
            return False
    return True

def every_translated_line_keeps_its_placeholders():
    en = spoken(ENGLISH)
    holes = lambda text: sorted(re.findall(r'\{([A-Z][A-Z0-9_]*)\}', text or ''))
    for path in TRANSLATIONS.values():
        said = spoken(path)
        for k in en:
            if k not in said or holes(en[k]) != holes(said[k]):
                return False
    return True

def every_language_the_screen_offers_has_a_file_the_mod_reads():
    choices = re.search(r'LanguageWords =\s*\{([^}]*)\}', M)
    return (choices is not None
            and len(choices.group(1).split(',')) == len(TRANSLATIONS) + 1
            and 'internal const int English = 0, Turkish = 1, Russian = 2, Chinese = 3;' in S['Tongue.cs']
            and S['Tongue.cs'].count('module_strings_') == len(TRANSLATIONS)
            and 'if (_saidFor == language) return true;' in
                method_body(S['Tongue.cs'], "private static bool Ready")
            and '_saidFor = language;' in method_body(S['Tongue.cs'], "private static bool Ready")
            and 'if (!Ready(Options.Current.Language)) return null;' in
                method_body(S['Tongue.cs'], "private static string Translated"))

def a_line_is_matched_to_its_translation_the_same_way_in_every_language():
    tongue = S['Tongue.cs']
    label = method_body(tongue, "private static string Id")
    loose = re.findall(r'\.(?:StartsWith|EndsWith|IndexOf)\((?:@?"[^"]*")\s*\)', tongue)
    return ('written.StartsWith("{=", StringComparison.Ordinal)' in label
            and 'id.StartsWith("TL", StringComparison.Ordinal)' in tongue
            and not loose)

chk("1.38.5", "TradeLord matches a line to its translation the same way whatever language the computer is set to",
    a_line_is_matched_to_its_translation_the_same_way_in_every_language())

def every_line_the_mod_says_can_change_language():
    said = "\n".join(v for k, v in S.items() if k != 'Tongue.cs')
    return ('new TextObject(' not in said
            and said.count('Tongue.Text(') > 50
            and 'if (Options.Current.Language == English) return new TextObject(written);'
                in method_body(S['Tongue.cs'], "internal static TextObject Text")
            and (lambda b: b.count('Tongue.Text("{=TL') == 2 * b.count('starter.AddGameMenuOption(') > 0
                       and b.count('args.Text = Tongue.Text("{=TL') == b.count('starter.AddGameMenuOption('))
                (method_body(S['Trading.cs'], "private void OnSessionLaunched"))
            and "AddGameMenuOption" not in method_body(S['Encounters.cs'], "private static void AddBanditLines"))

def the_language_setting_leads_the_screen_and_starts_on_english():
    return ('[SettingPropertyGroup("{=TL100}Language", GroupOrder = 0)]' in M
            and '[SettingPropertyGroup("{=TL104}Automation", GroupOrder = 1)]' in M
            and 'public int Language = 0;' in S['Options.cs']
            and 'Follows(Language, () => _o.Language, picked => _o.Language = picked);' in M
            and 'held.FollowLanguage();' in M
            and 'Settings.Reseat();' in method_body(M, "public static bool Init")
            and all('GroupOrder = ' + str(n) + ')]' in M for n in range(0, 7)))

def the_language_files_reach_the_download():
    return 'cp -r TradeLord/ModuleData dist/Modules/TradeLord/' in WORKFLOW

def the_workflow_gates_the_changelog():
    return ("this commit changes what a user gets and leaves CHANGELOG.md untouched" in WORKFLOW
            and "grep -qx 'CHANGELOG.md'" in WORKFLOW
            and "fetch-depth: 2" in WORKFLOW)

def the_gate_lets_a_checks_only_commit_through():
    return (r"grep -Ev '^(tests/|tools/|\.github/|\.claude/|\.gitignore$|CLAUDE\.md$)'" in WORKFLOW
            and "so it writes no changelog entry" in WORKFLOW)

def the_gate_lets_a_behaviour_neutral_change_through_but_never_a_version():
    return ('"[no release]"*) ;;' in WORKFLOW
            and "a [no release] commit may not ship a version" in WORKFLOW
            and "grep -qx 'TradeLord/SubModule.xml'" in WORKFLOW
            and ordered(WORKFLOW,
                        "the changelog carries this commit's entries",
                        "so it writes no changelog entry",
                        'SUBJECT=$(git log -1 --format=%s "$GITHUB_SHA")',
                        "a [no release] commit may not ship a version"))

def a_good_you_already_hold_enough_of_is_not_bought_again():
    body = pass_body("public static void ExecuteQuickBuy")
    return ("int holdCap = s.MaxHeldPerItem;" in body
            and "if (holdCap > 0 && held >= holdCap) { tally.Note(Block.HeldEnough); continue; }" in body
            and "if (s.MaxHeldPerItem > 0 && held >= s.MaxHeldPerItem) return Block.HeldEnough;" in body
            and body.count("held++;") == 2
            and "Block.HeldEnough" in method_body(S['Reasons.cs'], "internal static TextObject Phrase"))

def the_holding_cap_leaves_selling_alone():
    return ("MaxHeldPerItem" not in sell_pass()
            and "MaxHeldPerItem" not in sell_rule()
            and "MaxHeldPerItem" not in S['Ledger.cs'])

def the_town_menu_carries_one_trade_entry():
    body = method_body(S['Trading.cs'], "private void OnSessionLaunched")
    return (body.count("starter.AddGameMenuOption(") == 2
            and '"tradelord_quicktrade"' in body
            and '"tradelord_report"' in body
            and '"tradelord_quicksell"' not in body
            and '"tradelord_quickbuy"' not in body
            and ordered(body, '"tradelord_quicktrade"',
                        "ExecuteQuickSell(Settlement.CurrentSettlement);",
                        "ExecuteQuickBuy(Settlement.CurrentSettlement);"))

def the_one_entry_still_shows_when_buying_is_off():
    body = method_body(S['Trading.cs'], "private void OnSessionLaunched")
    return ("return (Options.Current.QuickSellMenu || Counter.HoldsBack()) && CanTradeHere(Settlement.CurrentSettlement);" in body
            and "Options.Current.AutoTradeBoth" not in body)

def the_rules_name_the_one_code_change_that_writes_no_entry():
    return ("moves working code without altering a single thing the user sees or gets" in RULES
            and "ships as `[no release]`, and leaves the version alone" in RULES
            and "Never invent an entry to get a commit past a gate" in RULES)

def the_workflow_refuses_to_publish_an_unfinished_changelog():
    return ("still carries an Unreleased heading" in WORKFLOW
            and "carries no section for" in WORKFLOW
            and ordered(WORKFLOW,
                        "this commit is marked [no release], so it publishes nothing",
                        "is already published, from commit $AT",
                        "still carries an Unreleased heading",
                        "carries no section for",
                        "gh release create"))

HOOK = io.open('.claude/hooks/session-start.sh', encoding='utf-8').read()

RULES = io.open('CLAUDE.md', encoding='utf-8').read()
GUARD = io.open('.claude/hooks/no-new-branch.sh', encoding='utf-8').read()
SETTINGS = io.open('.claude/settings.json', encoding='utf-8').read()

def the_hook_never_reads_an_address_out_of_the_history():
    return ("git log" not in HOOK
            and "rev-list" not in HOOK
            and "%ae" not in HOOK
            and "git config --local user.email" in HOOK
            and "git config --local commit.gpgsign false" in HOOK)

def the_hook_falls_back_to_the_signature_the_rules_name():
    return ("commit as `" in RULES
            and r"sed -n 's/.*commit as `\([^`]*\)`.*/\1/p' CLAUDE.md" in HOOK)

def the_signature_is_written_in_one_place_and_is_the_noreply_address():
    written = re.findall(r'commit as `([^`]*)`', RULES)
    shaped = re.match(r'^([A-Za-z0-9-]+) <\1@users\.noreply\.github\.com>$', written[0]) if written else None
    carried = set(re.findall(r'[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}', RULES))
    return (len(written) == 1
            and shaped is not None
            and carried == {written[0].split('<')[1].rstrip('>')}
            and 'never by Claude' in RULES)

def the_rules_keep_one_branch():
    return ('`main` is the only branch this repository keeps' in RULES
            and 'Never start another one' in RULES
            and 'no-new-branch.sh' in RULES
            and 'move the checkout to `main` and commit there' in RULES
            and 'Leave the assigned branch exactly as you found it, and never push it.' in RULES
            and "Read `HEAD` against the branch's own upstream instead." in RULES
            and 'Pushing a second branch to quiet the warning breaks the rule above.' in RULES)

def the_hook_moves_an_assigned_branch_onto_the_one_branch():
    return ('git rev-parse --abbrev-ref HEAD' in HOOK
            and 'git checkout main' in HOOK
            and 'git fetch --quiet origin main' in HOOK
            and 'git reset --hard' in HOOK
            and 'is the source to work from' in HOOK
            and 'still reachable through git reflog' in HOOK
            and 'left exactly as it was' in HOOK
            and 'has uncommitted work' in HOOK
            and '`.claude/hooks/session-start.sh` exists it has already moved the checkout' in RULES
            and 'restores the owner\'s signature for the session and moves the checkout to `main`' in RULES)

def the_rules_work_from_the_remote_rather_than_the_container():
    return ('The source to work from is `origin/main` as it stands right now' in RULES
            and 'git fetch origin main' in RULES
            and 'it is never asked about' in RULES
            and 'Where the two histories share no ancestor at all' in RULES
            and 'Uncommitted changes are the one case that stops.' in RULES
            and 'Never read the source, commit, or push on top of a checkout you have not read '
                'against `origin/main` in this session.' in RULES)

def the_rules_name_no_program_of_their_own():
    return (not re.search(r'Bannerlord|garrison|caravan|morale', RULES)
            and 'Use the words the program itself uses.' in RULES
            and 'Where the program is a mod, the words belong to the thing it is a mod for' in RULES)

def a_git_command_that_would_start_a_branch_is_refused():
    return ('no-new-branch.sh' in SETTINGS
            and '"PreToolUse"' in SETTINGS
            and '"matcher": "Bash"' in SETTINGS
            and 'session-start.sh' in SETTINGS
            and all(f in GUARD for f in ('checkout|switch', 'worktree', 'branch', 'push'))
            and 'exit 2' in GUARD)

def only_main_may_be_pushed():
    return ('main|refs/heads/main' in GUARD
            and 'would create the remote branch' in GUARD
            and 'main is the only place it may go' in GUARD
            and '--delete|-d) DELETING=1' in GUARD)

chk("1.6.32", "a commit that changes what a user gets is refused when it leaves the changelog untouched",
    the_workflow_gates_the_changelog())
chk("1.6.32", "a commit that touches only the checks, the workflow and the working rules is let through",
    the_gate_lets_a_checks_only_commit_through())
chk("1.6.32", "a version is not published while the changelog still says Unreleased or has no section for it",
    the_workflow_refuses_to_publish_an_unfinished_changelog())
chk("1.6.32", "the changelog may open on an Unreleased heading, and that heading has to say something",
    changelog_opens_on_the_shipped_version())
chk("1.7.0", "the session signature never comes from an address left in the history",
    the_hook_never_reads_an_address_out_of_the_history())
chk("1.7.0", "the session signature comes from the one place in the working rules that carries it",
    the_hook_falls_back_to_the_signature_the_rules_name())
chk("1.7.0", "the commit signature is the noreply address, written in one place and nowhere else",
    the_signature_is_written_in_one_place_and_is_the_noreply_address())
chk("1.7.0", "the rules keep one branch and move an assigned branch's work onto it, leaving that branch alone",
    the_rules_keep_one_branch())
chk("1.7.0", "the rules read the checkout against origin/main before anything else, and never make that a question",
    the_rules_work_from_the_remote_rather_than_the_container())
chk("1.7.0", "the session hook moves an assigned branch's checkout onto the one branch, and the rules say it does",
    the_hook_moves_an_assigned_branch_onto_the_one_branch())
chk("1.7.0", "the working rules name no program of their own, so they carry to another repository unchanged",
    the_rules_name_no_program_of_their_own())
chk("1.7.0", "a git command that would start a branch is refused before it runs",
    a_git_command_that_would_start_a_branch_is_refused())
chk("1.7.0", "a push may name main and nothing else, and deleting a branch is still allowed",
    only_main_may_be_pushed())
chk("1.7.0", "the cost-basis arithmetic lives beside the other money rules and needs nothing from the game",
    the_ledger_keeps_no_second_copy_of_the_cost_basis_rules())
chk("1.7.0", "a good with no price you paid still falls through to the cheapest market known",
    a_good_you_never_bought_still_falls_through_to_the_market())
chk("1.7.0", "what a lot cost is covered by tests the build runs",
    the_cost_basis_rules_are_covered_by_tests_the_build_runs())
chk("1.7.0", "a change that alters nothing a user sees may skip the changelog, but may never ship a version",
    the_gate_lets_a_behaviour_neutral_change_through_but_never_a_version())
chk("1.7.0", "the working rules name that one case, and still refuse an invented entry",
    the_rules_name_the_one_code_change_that_writes_no_entry())

chk("1.8.0", "a good you already hold enough of is left alone, on a real pass and a dry run alike",
    a_good_you_already_hold_enough_of_is_not_bought_again())
chk("1.8.0", "the holding cap binds buying only, never selling",
    the_holding_cap_leaves_selling_alone())
chk("1.8.0", "the town menu carries one trade entry, which sells before it buys",
    the_town_menu_carries_one_trade_entry())
chk("1.8.0", "the trade entry shows on its own switch alone, so trading by hand stays reachable",
    the_one_entry_still_shows_when_buying_is_off())

def selling_and_buying_close_the_screen():
    seen = []
    for b in setting_blocks():
        g = re.search(r'\[SettingPropertyGroup\("\{=TL\d+\}([^"]+)",\s*GroupOrder = (\d+)\)\]', b)
        if g is None:
            return False
        pair = (int(g.group(2)), g.group(1))
        if pair not in seen:
            seen.append(pair)
    seen.sort()
    return (len(seen) == 9
            and [name for _, name in seen][-3:] == ['Selling', 'Buying', 'Debug']
            and seen[-2][0] - seen[-3][0] == 1
            and seen[-1][0] - seen[-2][0] == 1
            and 'Debug' == spoken(ENGLISH)['TL107']
            and '{=TL103}Selling' in M and '{=TL103}Action' not in M
            and 'Selling' == spoken(ENGLISH)['TL103'])

def the_switches_say_what_they_do():
    return ("{=TL217}Auto sell" in M and "{=TL218}Auto buy" in M
            and "as well as sell" not in M
            and "quick-buy" not in M and "quick-sell" not in M
            and "Auto-trade" not in M
            and M.count("SettingPropertyGroup(\"{=TL104}Automation\"") == 3)

chk("1.9.0", "the switches name selling and buying plainly, and none names an entry the menu no longer has",
    the_switches_say_what_they_do())
chk("1.13.0", "selling and buying are the last two trading groups on the screen, side by side, with Debug alone below them",
    selling_and_buying_close_the_screen())
chk("1.6.32", "a good named on an item list never drags in a second good whose whole name is one of its words",
    a_written_word_stands_for_an_id_and_never_for_another_goods_name())
chk("1.6.32", "a route's spending caps are spent unit by unit, the way a buying pass spends them",
    the_spend_cap_is_walked_not_divided())

chk("1.12.0", "every translation carries every line the English one does, put into that language, and the one line that is a bare mark is a bare mark in every one of them",
    every_translation_says_everything_the_english_one_does())
chk("1.12.0", "every language the screen offers has a file the mod reads, and switching re-reads it",
    every_language_the_screen_offers_has_a_file_the_mod_reads())
chk("1.7.0", "a translated line keeps every value the English one fills in",
    every_translated_line_keeps_its_placeholders())
chk("1.7.0", "every line the mod says on screen is built where the language is chosen",
    every_line_the_mod_says_can_change_language())
chk("1.7.0", "the language setting opens the screen, auto sell and auto buy come next, and the language starts on English",
    the_language_setting_leads_the_screen_and_starts_on_english())
chk("1.7.0", "the language files are packed into the download",
    the_language_files_reach_the_download())

def the_settings_screen_follows_the_mods_own_language():
    follow = method_body(M, "internal static void Follow")
    spoken = method_body(M, "private static void Spoken")
    return ('Guard.Run("Mcm.ScreenTongue", ScreenTongue.Follow);' in M
            and 'typeof(SettingsPropertyDefinition)' in follow
            and all(field in follow for field in ('"<DisplayName>k__BackingField"',
                                                  '"<HintText>k__BackingField"',
                                                  '"<GroupName>k__BackingField"',
                                                  '"<Content>k__BackingField"'))
            and 'postfix: new HarmonyMethod(typeof(ScreenTongue), nameof(Spoken))' in follow
            and 'Say(__instance, words);' in spoken
            and method_body(M, "private static void Say").count('Said(words[') == 4)

def the_screen_reads_the_translation_the_mod_already_has():
    said = method_body(S['Tongue.cs'], "internal static string Said")
    return ('private static string Said(string word) => Tongue.Said(word) ?? word;' in M
            and 'Translated(Id(written))' in said
            and 'Options.Current.Language == English ? null' in said
            and 'module_strings' not in M)

def a_screen_that_cannot_be_wired_leaves_the_rest_of_the_mod_alone():
    follow = method_body(M, "internal static void Follow")
    once = follow.find('if (_following) return;')
    failed = follow.find('if (_name == null')
    said = follow.find('Log.Write(')
    left = follow.find('return;', said)
    patched = follow.find('new Harmony(SubModule.HarmonyId + ".mcm")')
    return (0 <= once < failed < said < left < patched
            and follow.find('_following = true;') < failed)

chk("1.10.0", "the settings screen is relabelled in the language TradeLord is set to",
    the_settings_screen_follows_the_mods_own_language())
chk("1.10.0", "the screen reads the translation the mod already carries, not a second copy",
    the_screen_reads_the_translation_the_mod_already_has())
chk("1.10.0", "a settings screen that cannot be relabelled says so and leaves the rest alone",
    a_screen_that_cannot_be_wired_leaves_the_rest_of_the_mod_alone())

def a_new_language_reaches_the_screen_without_waiting_for_a_restart():
    setter = between(M, "public Dropdown<string> Language", "internal static void Reset")
    told = method_body(M, "private void Retell")
    again = method_body(M, "private void Relabel")
    return ("Follows(value, () => _o.Language, picked => _o.Language = picked);" in setter
            and "Options.Bump();\n                Retell();" in setter
            and "Relabel();" in told
            and ordered(again, "if (_spokenFor == _o.Language) return;", "bool first = _spokenFor < 0;",
                        "_spokenFor = _o.Language;", "if (first) return;",
                        'Guard.Run("Mcm.Relabel"', "ScreenTongue.Respeak();", "Redrawn();")
            and "private int _spokenFor = -1;" in M)

chk("1.27.1", "a language picked on the screen is spoken there and then, and the screen is asked to draw itself again",
    a_new_language_reaches_the_screen_without_waiting_for_a_restart())

def the_screen_is_spoken_again_where_it_already_stands():
    follow = method_body(M, "internal static void Follow")
    spoken = method_body(M, "private static void Spoken")
    headline = method_body(M, "private static void Headline")
    again = method_body(M, "internal static void Respeak")
    drawn = method_body(M, "private void Redrawn")
    return ('typeof(SettingsPropertyGroupDefinition)' in follow
            and '"_groupNameRaw"' in follow
            and 'postfix: new HarmonyMethod(typeof(ScreenTongue), nameof(Headline))' in follow
            and spoken.count('Tongue.Mine(words[') == 4
            and spoken.count('new WeakReference(__instance)') == 1
            and headline.count('new WeakReference(__instance)') == 1
            and '_rawFor[shown] = words[2];' in M
            and ordered(again, 'Say(one, _held[i].Words);', '_heading(one) = Said(_headed[i].Word);')
            and all('lock (_held)' in body for body in (spoken, headline, again))
            and 'AccessTools.Field(typeof(BaseSettings), "PropertyChanged")' in M
            and 'shown.RefreshValues();' in drawn)

def the_language_hint_keeps_to_what_it_is_for():
    said = [spoken(ENGLISH).get('TL350', '')] + [spoken(path).get('TL350', '') for path in TRANSLATIONS.values()]
    return (all(t for t in said)
            and all('screen' not in t.lower() for t in said)
            and all(len(t) < 280 for t in said))

chk("1.27.4", "a language picked on the screen is spoken again into the names, hints and headings the screen already holds",
    the_screen_is_spoken_again_where_it_already_stands())
chk("1.27.5", "the hint under the language setting names the language and nothing about the settings screen itself",
    the_language_hint_keeps_to_what_it_is_for())

def a_choice_between_named_things_is_picked_from_a_list():
    numbered = re.findall(r'\[SettingPropertyInteger\("\{=TL\d+\}[^"]*", 0, [0-3],', M)
    picked = set(re.findall(r'\[SettingPropertyDropdown\("\{=(TL\d+)\}', M))
    return (numbered == []
            and picked == {'TL250', 'TL222', 'TL223', 'TL224', 'TL227', 'TL264'}
            and all('public Dropdown<string> ' + named in M for named in
                    ('Language', 'FoodPolicy', 'CraftingPolicy', 'LivestockPolicy', 'CostBasisMode',
                     'KeepSmeltableWeapons')))

def the_words_in_a_choice_follow_the_mods_language():
    follow = method_body(M, "internal void FollowLanguage")
    return ('Tongue.Text(words[i]).ToString()' in method_body(M, "private static string[] Spoken")
            and method_body(M, "private void Retell").count('Retold(') == 5
            and 'Language.PropertyChanged += (sender, args) => Retell();' in follow
            and follow.count('Retell();') == 2
            and follow.count('Follows(') == 6)

def a_good_you_always_buy_gets_past_the_policies_but_not_the_never_lists():
    body = buy_rule()
    return (ordered(body, 'Listed(s.NeverSet, good) || Listed(s.NeverBuySet, good)',
                    'game.Locked()',
                    'bool always = Listed(s.AlwaysBuySet, good);',
                    '!always && !toFeed && s.NeverBuyGrain',
                    '!always && !TradeMath.PolicyAllows(PolicyFor(good, s), buying: true)')
            and 'AlwaysBuySet => Parsed(AlwaysBuyItems' in S['Options.cs']
            and 'Unmatched("always buy", s.AlwaysBuyItems, _knownIds, _knownNames);' in S['Policy.cs']
            and '_o.AlwaysBuyItems' in M)

def looted_gear_is_cleared_from_the_first_tier_by_default():
    hint = re.search(r'\{=TL328\}([^"]*)"', M)
    gate = between(S['Rules.cs'], "bool sellable = livestock || good.IsTradeGood ||", ";")
    return (option_default('MaxLootTier') == '1'
            and 'good.Tier + 1 <= s.MaxLootTier' in S['Rules.cs']
            and 's.MaxLootTier > 0' in gate
            and '!good.IsFood' in gate and '!good.IsAnimal' in gate and '!good.IsMountable' in gate
            and 'if (!sellable) { said.Why = Block.NotTradable; return said; }' in sell_rule()
            and hint is not None and 'tier 1' in hint.group(1))

chk("1.11.0", "a setting with named choices is picked from a list rather than typed as a number",
    a_choice_between_named_things_is_picked_from_a_list())
chk("1.11.0", "the choices in those lists are written in the language TradeLord is set to",
    the_words_in_a_choice_follow_the_mods_language())
chk("1.11.0", "a good on the always-buy list clears the policies and the grain switch, never the never lists or a lock",
    a_good_you_always_buy_gets_past_the_policies_but_not_the_never_lists())

def the_grain_switch_keeps_grain_out_of_trading_not_out_of_the_larder():
    buy = buy_rule()
    restock = method_body(S['Trading.cs'], "public static void ExecuteResupply")
    profit = buy_pass()
    road = method_body(S['Trading.cs'], "public static void ExecuteRoadTrade")
    return ("bool toFeed, Options s, TGame game," in buy
            and "!always && !toFeed && s.NeverBuyGrain && good.IsGrain" in buy
            and "toFeed: true" in restock
            and "toFeed" not in profit and "toFeed" not in road
            and S['Trading.cs'].count("toFeed: true") == 1
            and "Listed(s.NeverSet, good) || Listed(s.NeverBuySet, good)" in buy)

chk("1.37.2", "the never buy grain switch keeps grain out of trading for profit without starving the larder",
    the_grain_switch_keeps_grain_out_of_trading_not_out_of_the_larder())
chk("1.11.0", "looted gear is cleared from the first tier out of the box, and the hint says so",
    looted_gear_is_cleared_from_the_first_tier_by_default())

def a_brace_inside_text_is_not_read_as_the_end_of_a_method():
    sample = ("class Sample {\n"
              "    void One() { char c = '}'; string s = \"}\"; int a = 1; }\n"
              "    void Two() { int b = 2; }\n"
              "    void Three() { int d = 3; }\n"
              "}\n")
    mark = len(_lost)
    one = method_body(sample, "void One")
    two = method_body(sample, "void Two")
    lined = "class S {\n    void Four() { int e = 4; // } and /* } too\n    }\n}\n"
    blocked = "class S {\n    void Five() { int f = 5; /* } */ }\n}\n"
    commented = method_body(lined, "void Four")
    spanned = method_body(blocked, "void Five")
    grew = len(_lost) - mark
    return (grew == 0
            and one == "void One() { char c = '}'; string s = \"}\"; int a = 1; }"
            and two == "void Two() { int b = 2; }"
            and commented == "void Four() { int e = 4; // } and /* } too\n    }"
            and spanned == "void Five() { int f = 5; /* } */ }"
            and code_only(sample).count('}') == sample.count('}') - 2)

chk("1.11.0", "a brace inside a string, a character or a comment is not read as the end of a method",
    a_brace_inside_text_is_not_read_as_the_end_of_a_method())

def section_entries(head):
    body = CHANGES.split('## ' + head, 1)[1].split('\n## ', 1)[0]
    return [line[2:] for line in body.split('\n') if line.startswith('- ')]

def every_changelog_entry_stands_on_one_line():
    loose = [line for line in CHANGES.split('\n')
             if line.strip() and not line.startswith('## ') and not line.startswith('- ')
             and line != '# Changelog' and line != '---']
    entries = [line for line in CHANGES.split('\n') if line.startswith('- ')]
    return (len(entries) > 300 and len(loose) == 1
            and loose[0].startswith('The versions below are'))

def the_paste_text_is_the_changelog_without_its_markup():
    import subprocess
    version = module_version()
    made = subprocess.run([sys.executable, 'tools/nexus_changelog.py', version],
                          capture_output=True)
    if made.returncode != 0:
        return False
    out = made.stdout.decode('utf-8').splitlines()
    said = section_entries(version)
    return (len(said) > 0 and out[0] == '[' + version + ']'
            and [line for line in out[1:] if line] == said
            and "sections(text)" in NEXUS)

def the_paste_tool_reads_past_an_unreleased_heading():
    import subprocess
    def run(*args):
        return subprocess.run([sys.executable, 'tools/nexus_changelog.py'] + list(args),
                              capture_output=True)
    named = run('--notes', module_version())
    asked = run('Unreleased')
    return (named.returncode == 0 and named.stdout.startswith(b'- ')
            and asked.returncode != 0
            and b'no section for Unreleased' in asked.stderr
            and "if wanted is None and not as_page and found[0][0].lower() == 'unreleased':" in NEXUS
            and "found = [(v, said) for v, said in found if v.lower() != 'unreleased']" in NEXUS)

def flattened(text):
    said = []
    for line in text.split('\n'):
        bare = re.sub(r'^-\s+', '', re.sub(r'^#{1,6}\s+', '', line.strip()))
        bare = re.sub(r'\[([^\]]+)\]\([^)]+\)', r'\1', bare)
        said.append(re.sub(r'[`*]', '', bare))
    return ' '.join(' '.join(said).split())

def made_page():
    import subprocess
    made = subprocess.run([sys.executable, 'tools/nexus_changelog.py', '--page'],
                          capture_output=True)
    return None if made.returncode != 0 else made.stdout.decode('utf-8')

def a_price_that_moved_since_your_last_look_says_so_in_the_tooltip():
    record = method_body(S['Ledger.cs'], "private void Record")
    drift = method_body(S['Ledger.cs'], "public int PriceDrift")
    tip = method_body(S['TooltipPatches.cs'], "private static string Drifted")
    return (ordered(record, "if (TradeMath.ReadingIsNew(day, seen.CapturedDay))",
                    "seen.WasBuyPrice = seen.BuyPrice;",
                    "seen.WasSellPrice = seen.SellPrice;",
                    "seen.WasDay = seen.CapturedDay;",
                    "seen.BuyPrice = buy;")
            and "!seen.SeenBefore) return 0;" in drift
            and "TradeMath.Drift(seen.SellPrice, seen.WasSellPrice)" in drift
            and "TradeMath.Drift(seen.BuyPrice, seen.WasBuyPrice)" in drift
            and 'if (!Options.Current.MarkPriceDirection) return "";' in tip
            and tip.index('MarkPriceDirection') < tip.index('PriceDrift')
            and "LedgerBehavior.Instance?.PriceDrift(item, town, selling)" in tip
            and "{=TL404}rising" in tip and "{=TL405}falling" in tip
            and S['TooltipPatches.cs'].count("Drifted(item, town, selling: true)") == 1
            and S['TooltipPatches.cs'].count("Drifted(item, town, selling: false)") == 1
            and {'TL404', 'TL405'} <= strings_declared()
            and all({'TL404', 'TL405'} <= set(spoken(f))
                    for f in [ENGLISH] + list(TRANSLATIONS.values())))

def the_price_direction_marker_ships_switched_off_with_a_switch_of_its_own():
    return (option_default('MarkPriceDirection') == 'false'
            and "_o.MarkPriceDirection" in M
            and "{=TL406}Mark a market rising or falling" in M
            and "{=TL407}" in M
            and 'OFF by default.' in spoken(ENGLISH)['TL407']
            and {'TL406', 'TL407'} <= strings_declared()
            and all({'TL406', 'TL407'} <= set(spoken(f))
                    for f in [ENGLISH] + list(TRANSLATIONS.values())))

def which_way_a_price_moved_is_worked_out_where_a_test_can_ask_it():
    rule = method_body(S['TradeMath.cs'], "public static int Drift")
    return ("public const float DriftWorthSaying = 0.05f;" in S['TradeMath.cs']
            and "if (now <= 0 || was <= 0) return 0;" in rule
            and "float moved = (float)(now - was) / was;" in rule
            and "if (moved >= DriftWorthSaying) return 1;" in rule
            and "return moved <= -DriftWorthSaying ? -1 : 0;" in rule
            and "day - lastDay >= DaysBeforeAnotherReading" in
                between(S['TradeMath.cs'],
                        "public static bool ReadingIsNew(float day, float lastDay) =>", ";")
            and "DriftWorthSaying" not in S['Ledger.cs'] + S['TooltipPatches.cs']
            and 'TradeMath.cs' in TESTPROJ and 'LedgerCodec.cs' in TESTPROJ
            and "A_price_that_climbed_since_your_last_look_is_rising" in DRIFTTESTS
            and "A_price_that_dropped_since_your_last_look_is_falling" in DRIFTTESTS
            and "A_second_look_on_the_same_day_is_not_a_second_reading" in DRIFTTESTS)

def a_campaign_saved_before_this_version_keeps_every_price_it_had():
    read = method_body(S['LedgerCodec.cs'],
                       "public static Dictionary<string, List<PriceObservation>> ReadLedger")
    return ("if (parts.Length < FieldsAPriceNeeds) { unreadable++; continue; }" in read
            and "float wasDay = PriceObservation.NoEarlierReading;" in read
            and "public const float NoEarlierReading = -1f;" in S['LedgerCodec.cs']
            and "public bool SeenBefore => WasDay >= 0f;" in S['LedgerCodec.cs']
            and "public float WasDay = NoEarlierReading;" in S['LedgerCodec.cs']
            and "A_campaign_saved_before_this_version_loads_with_no_earlier_reading" in DRIFTTESTS
            and "An_earlier_reading_that_cannot_be_read_costs_only_the_history" in DRIFTTESTS
            and "A_record_too_short_to_hold_a_price_keeps_the_current_price_it_does_hold" in DRIFTTESTS)

def the_feature_list_says_how_a_price_is_read_rather_than_naming_a_brain():
    return ('brain' not in README.lower()
            and 'take off speed' not in README.lower()
            and README.count('so the price it shows is the price it pays') == 2
            and 'return held.GetPrice(el, who, selling, Merchant(site));' in S['Market.cs']
            and 'the way the trade that follows is charged' in S['Market.cs']
            and '(IMarketData)site.Town.MarketData' in S['Market.cs']
            and '(IMarketData)site.Village.MarketData' in S['Market.cs'])

def the_page_carries_the_summary_the_features_the_comparison_and_the_changelog():
    out = made_page()
    version = module_version()
    if out is None or version is None:
        return False
    return ('[*]' + README.split('\n', 1)[0][2:] in out
            and '[size=5][b]Everything it does[/b][/size]' in out
            and '[b]What it tells you[/b]' in out
            and '[b]What it doesn\'t touch[/b]' in out
            and '[size=5][b]What it needs[/b][/size]' in out
            and '[size=5][b]' + COMPARISON.split('\n', 1)[0][2:] + '[/b][/size]' in out
            and '[size=5][b]What none of the nine do[/b][/size]' in out
            and '[size=5][b]Where they are ahead[/b][/size]' in out
            and '[size=5][b]Changelog[/b][/size]' in out
            and '[b]' + version + '[/b]' in out
            and 'Unreleased' not in out
            and all('[*]' + one in out for one in section_entries(version)))

def the_page_leads_with_what_the_mod_is_and_folds_the_long_tail_away():
    out = made_page()
    if out is None:
        return False
    summary = out.split('[size=5]', 1)[0]
    lead = [one for one in README.split('\n## ', 1)[0].split('\n') if one.startswith('- ')]
    headed = [one[2:-2] for one in README.split('\n')
              if one.startswith('**') and one.endswith('**') and one.count('**') == 2]
    return (len(lead) == 5 and len(headed) == 6
            and '\u2705' not in summary
            and not [one for one in lead if one.startswith('- \u2705')]
            and len([one for one in summary.split('\n') if one.startswith('[*]')]) == len(lead)
            and re.findall(r'\[b\]([^\[]+)\[/b\]\n\[spoiler\]', out) == headed
            and out.count('[spoiler]') == out.count('[/spoiler]') == len(headed)
            and all(one.startswith('[list]\n[*]\u2705')
                    for one in out.split('[spoiler]\n')[1:])
            and '[spoiler]' not in out.split('[size=5][b]What it needs', 1)[1])

def the_page_is_written_from_the_repository_rather_than_pasted():
    out = made_page()
    if out is None:
        return False
    sources = flattened(README + '\n' + COMPARISON + '\n' + CHANGES)
    bare = re.sub(r'\[/?(?:b|list|size=5|size|spoiler|\*)\]', '', out)
    bare = re.sub(r'\[url=[^\]]+\]|\[/url\]', '', bare)
    for line in bare.split('\n'):
        said = ' '.join(line.split())
        if said and said != 'Changelog' and said not in sources:
            return False
    return ("io.open('README.md', encoding='utf-8').read()" in NEXUS
            and "io.open('COMPARISON.md', encoding='utf-8').read()" in NEXUS
            and "io.open('CHANGELOG.md', encoding='utf-8').read()" in NEXUS)

def the_page_leaves_no_markdown_behind_and_closes_every_tag():
    out = made_page()
    version = module_version()
    if out is None or version is None:
        return False
    bullets = len([one for one in (README + '\n' + COMPARISON).split('\n')
                   if one.startswith('- ')])
    return ('**' not in out and '`' not in out
            and not [one for one in out.split('\n')
                     if one.startswith('#') or one.startswith('- ')]
            and all(len(re.findall(r'\[' + tag + r'[^\]/]*\]', out)) == out.count('[/' + tag + ']')
                    for tag in ('b', 'list', 'size', 'spoiler'))
            and len(re.findall(r'\[url=[^\]]+\]', out)) == out.count('[/url]')
            and out.count('[/url]') == len(re.findall(r'\[[^\]]+\]\([^)]+\)', COMPARISON))
            and out.count('[*]') == bullets + len(section_entries(version)))

def the_page_and_the_notes_are_asked_for_one_at_a_time():
    import subprocess
    def run(*args):
        return subprocess.run([sys.executable, 'tools/nexus_changelog.py'] + list(args),
                              capture_output=True)
    both = run('--notes', '--page')
    unknown = run('--nope')
    return (both.returncode != 0 and b'ask for one' in both.stderr
            and unknown.returncode != 0
            and b'is not something this tool knows' in unknown.stderr)

def a_version_that_has_not_shipped_is_refused_by_the_paste_tool():
    import subprocess
    made = subprocess.run([sys.executable, 'tools/nexus_changelog.py', '99.99.99'],
                          capture_output=True)
    return made.returncode != 0 and b'no section for 99.99.99' in made.stderr

chk("1.11.0", "every changelog entry stands on one line, which is what a Nexus entry takes",
    every_changelog_entry_stands_on_one_line())
chk("1.11.0", "the paste text for the shipped version is its changelog entries with the markup dropped",
    the_paste_text_is_the_changelog_without_its_markup())
chk("1.11.0", "the paste tool refuses a version the changelog does not carry",
    a_version_that_has_not_shipped_is_refused_by_the_paste_tool())
chk("1.11.0", "a shipped version's notes are still read while the changelog opens on Unreleased, and Unreleased is never handed out as one",
    the_paste_tool_reads_past_an_unreleased_heading())

def a_rule_that_names_missing_source_reports_itself_broken():
    mark = len(_lost)
    lost_body = method_body("class Sample { }", "private static void Absent")
    lost_region = between("private static void Present() { }", "if (absent)", ";")
    lost_end = between("private static void Present() { }", "void Present", "if (absent)")
    grew = len(_lost) - mark
    del _lost[mark:]
    return (lost_body == '' and lost_region == '' and lost_end == '' and grew == 3
            and "if len(_lost) > _read:" in SWEEP
            and "which the source no longer has" in SWEEP)

chk("1.7.0", "a rule naming source that is no longer there reports itself broken, and every later rule is still read",
    a_rule_that_names_missing_source_reports_itself_broken())

chk("1.75.1", "a price model that could not be asked says so again in the next campaign rather than staying quiet for the rest of the session",
    "internal static void Forget() => _saidItCouldNotAsk = false;" in S['Market.cs'] and
    "Priced.Forget();" in method_body(S['Trading.cs'], "internal static void ForgetVisit") and
    S['Trading.cs'].count("Priced.Forget();") == 1)

chk("1.75.1", "the map marker names the market it beat as the next best it priced, never as the second best on the map",
    (lambda said, walk: '", and no other market it priced would take any of it"' in said
           and 'how.RunnerUp.Name + ", the next best it priced, at " +' in said
           and '", ahead of " + next' in said
           and "if (TradeMath.PerDay(gold, ride) <= bar) break;" in walk
           and "else if (rate > how.RunnerUpRate)\n"
               "                { how.RunnerUpRate = rate; how.RunnerUpValue = total; how.RunnerUp = s; }" in walk)
    (method_body(S['Marker.cs'], "private static string TheNextBest"),
     method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")))

chk("1.14.1", "the panel hotkey is ignored while a text field on the map has the keyboard",
    "layers[i].IsFocusedOnInput()" in method_body(S['Panel.cs'], "private static bool TypingOnScreen") and
    S['Panel.cs'].count("HotkeyReleased() && !TypingOnScreen(map)") == 2 and
    S['Panel.cs'].count("!TypingOnScreen(map) && HotkeyReleased()") == 0 and
    "!map.IsEscapeMenuOpened && HotkeyReleased() && !TypingOnScreen(map)" in S['Panel.cs'])
chk("1.14.1", "a text field that cannot be read leaves the hotkey working rather than dead",
    "return false;" in method_body(S['Panel.cs'], "private static bool TypingOnScreen") and
    "catch (Exception e) { Log.Error(e," in method_body(S['Panel.cs'], "private static bool TypingOnScreen"))
chk("1.14.1", "the escape menu still closes the panel whether or not anything is being typed",
    "else if (map.IsEscapeMenuOpened || (HotkeyReleased() && !TypingOnScreen(map)))" in S['Panel.cs'])

def the_hold_hints_say_the_floor_binds_only_while_the_switch_is_on():
    en = spoken(ENGLISH)
    hold, tolerance = en.get('TL329', ''), en.get('TL330', '')
    said = [(spoken(path).get('TL329', ''), spoken(path).get('TL330', ''))
            for path in TRANSLATIONS.values()]
    return ("what you bought and what you looted alike" in hold
            and "OFF by default" in hold
            and "looted gear goes to the first market that can pay for it" in hold
            and "never bought" not in hold and "never bought" not in tolerance
            and "It does nothing while the setting above is OFF." in tolerance
            and hold in M and tolerance in M
            and all(a and b and a != hold and b != tolerance for a, b in said))

def a_pack_animal_is_an_animal_and_a_town_has_gold_not_a_till():
    en = spoken(ENGLISH)
    shipped = list(en.values()) + [README]
    return (not any(re.search(r'\bbeasts?\b', t, re.I) for t in shipped)
            and not any(re.search(r'\btills?\b', t, re.I) for t in shipped)
            and not any(re.search(r'\bpremiums?\b', t, re.I) for t in shipped)
            and 'An animal that carries nothing for you and is not livestock is no haul animal' in README
            and all(named in en.get('TL367', '') and named in README for named in
                    ('a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse or a Pack Camel',))
            and 'Buy any haul animal' in en.get('TL367', '')
            and 'Buys any haul animal' in README
            and all(said in en.get('TL375', '') and said in README for said in
                    ('then your haul animals, and your war horses and noble horses last of all',
                     'it keeps enough haul animals to carry what you are already carrying'))
            and 'How much gold the town you would sell to actually has' in README)

chk("1.23.0", "nothing a player reads calls a haul animal a beast, a town's gold a till, or an overpayment a premium",
    a_pack_animal_is_an_animal_and_a_town_has_gold_not_a_till())

chk("1.21.0", "the hold hints say the floor binds everything alike and does nothing while the switch is off",
    the_hold_hints_say_the_floor_binds_only_while_the_switch_is_on())

def the_panel_relabels_every_line_it_speaks():
    src = S['Panel.cs']
    vm = src[src.index('public class LedgerPanelVM'):src.index('internal static class LedgerPanel')]
    spoken = re.findall(r'\[DataSourceProperty\]\s*public\s+string\s+(\w+)\s*=>', vm)
    listed = re.search(r'private static readonly string\[\] SpokenLabels\s*=\s*\{(.*?)\};', vm, re.S)
    named = re.findall(r'"(\w+)"', listed.group(1)) if listed else []
    body = method_body(S['Panel.cs'], "private void Refresh()")
    raise_all = "for (int i = 0; i < SpokenLabels.Length; i++) OnPropertyChanged(SpokenLabels[i]);"
    return (len(spoken) > 10 and sorted(spoken) == sorted(named) and len(named) == len(set(named))
            and raise_all in body
            and ordered(body, raise_all, "PlayerGold ="))

def the_food_floor_keeps_one_of_every_kind_without_stacking_on_the_days():
    body = food_rule()
    return ("int variety = s.KeepEveryFoodKind ? s.KeepPerFoodKind : 0;" in body
            and "if (s.KeepFoodDays <= 0 || carried == null) return keep;" in body
            and "IsLivestock" not in body
            and "int floor = Math.Min(held.Amount, variety);" in body
            and "reserve -= (floor - had) * FoodValue(held.Good);" in body
            and "if (reserve <= 0) break;" in body
            and ordered(body, "int variety = s.KeepEveryFoodKind ? s.KeepPerFoodKind : 0;",
                        "if (variety > 0)", "int floor = Math.Min(held.Amount, variety);",
                        "reserve -= (floor - had) * FoodValue(held.Good);",
                        "if (reserve <= 0) break;",
                        "Math.Min(held.Amount - had, (reserve + perUnit - 1) / perUnit)"))

def the_notes_are_the_changelog_section_for_the_version():
    import subprocess
    version = module_version()
    made = subprocess.run([sys.executable, 'tools/nexus_changelog.py', '--notes', version],
                          capture_output=True)
    if made.returncode != 0:
        return False
    said = [line for line in made.stdout.decode('utf-8').splitlines() if line]
    wanted = section_entries(version)
    return (len(wanted) > 0 and said == ['- ' + line for line in wanted])

chk("1.14.2", "quick-buy leaves a good its own sell policy would never let it sell again",
    "if (!TradeRules.ResaleAllowed(good, s)) { tally.Note(Block.CategoryPolicy); continue; }" in
        buy_pass() and
    "MayRoundTrip(it," not in S['Trading.cs'] and
    "TradePolicy.MayBuy(good, Item(at), _pass.Locked, out why)" in buy_pass() and
    ordered(method_body(S['Passes.cs'], "internal static List<Pick> WhatToBuy"),
            "market.MayBuy(at, good, out Block whyBuy)",
            "TradeRules.ResaleAllowed(good, s)"))

chk("1.14.2", "a route walk prices each town's ladder once and reads it back for every partner",
    "internal int At(int taken)" in S['Market.cs'] and
    "while (_priced.Count <= taken)" in method_body(S['Market.cs'], "internal int At(int taken)") and
    "int buyPrice = buy.At(u);" in method_body(S['Market.cs'], "internal static RouteQuote Walk") and
    "int sellPrice = sell.At(u);" in method_body(S['Market.cs'], "internal static RouteQuote Walk") and
    "new Shelf(" not in method_body(S['Market.cs'], "internal static RouteQuote Walk") and
    "_rungs.TryGetValue(key, out Ladder rung)" in
        method_body(S['Market.cs'], "private static Ladder Rung"))

chk("1.14.2", "the ladders are dropped when a scan starts, given back when it finishes, and dropped when the campaign ends, so no scan reads stale prices and none is held on to",
    "Bulk.Forget();" in method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes") and
    ordered(method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes"),
            "Bulk.Forget();", "foreach (ItemObject item in Items.All)",
            "Bulk.Forget();\n            return routes;") and
    method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes").count("Bulk.Forget();") == 2 and
    'Guard.Run("GameEnd.Bulk", Bulk.Forget);' in S['SubModule.cs'] and
    "_rungs.Clear();" in method_body(between(S['Market.cs'], "internal static class Bulk",
                                             "internal static class Priced"),
                                     "internal static void Forget()"))

chk("1.14.3", "a market whose merchant has no gold is no destination in any list the mod ranks, not just the route scan",
    (lambda body: "if (selling && TradeRules.WhatTheTillCanPay(s.SettlementComponent.Gold," in body
              and "s.IsVillage) <= 0) continue;" in body
              and ordered(body, "if (selling && TradeRules.WhatTheTillCanPay(s.SettlementComponent.Gold,",
                          "int price = Priced.At(s.SettlementComponent,"))
    (method_body(S['Ledger.cs'], "private List<(Settlement, int)> TopLive")) and
    ".WhereThisEarnsFastest(Item(at), paid, units, _pass.Site) ?? (null, 0, null);" in
        buy_pass() and
    "var markets = EverySell(item);" in
        method_body(S['Ledger.cs'], "internal (Settlement town, int price, Ladder rungs) WhereThisEarnsFastest") and
    "LedgerBehavior.Instance?.BestSell(Item(at)) ?? (null, 0)" in
        sell_pass())

chk("1.14.4", "the sell pass names a stopping rule only when one fired, so a cargo it may not sell never reads as a market with nothing to trade",
    (lambda sell: "Block stopped = tally.Dominant();" in sell
              and "if (stopped != Block.None && !pass.Muted) NoteStalled(selling: true, stopped);" in sell
              and ordered(sell, "Block stopped = tally.Dominant();",
                          "if (stopped != Block.None && !pass.Muted)"))
    (sell_pass()) and
    "if (!Structural(kv.Key) &&" in method_body(S['Passes.cs'], "internal Block Dominant") and
    (lambda buy: "Block stopped = tally.Dominant();" in buy
             and "if (stopped != Block.None && !pass.Muted) NoteStalled(selling: false, stopped);" in buy
             and ordered(buy, "Block stopped = tally.Dominant();",
                         "if (stopped != Block.None && !pass.Muted)"))
    (buy_pass()))

chk("1.14.5", "every line the panel speaks is raised again when it refreshes, so a language change reaches its headings too",
    the_panel_relabels_every_line_it_speaks())

chk("1.15.0", "the food floor holds back every kind of food, counts inside the days of supply and leaves livestock out",
    the_food_floor_keeps_one_of_every_kind_without_stacking_on_the_days())
chk("1.16.0", "the food floor is a switch that ships off, with its own amount that starts at two of each kind",
    option_default('KeepEveryFoodKind') == 'false' and
    option_default('KeepPerFoodKind') == '2' and
    "_o.KeepEveryFoodKind" in M and "_o.KeepPerFoodKind" in M and
    re.search(r'SettingPropertyInteger\("\{=TL262\}[^"]*", 1, 50,', M) is not None)
chk("1.58.0", "a herd is left out of the food floor because TradeLord trades it as goods and never as food, and the hint says so",
    "IsLivestock" not in food_rule() and
    "trades a herd as goods and never as food" in spoken(ENGLISH).get('TL361', ''))

chk("1.17.0", "the settings screen hands every preset its own settings, so Default puts the built-in ones back",
    each_preset_gets_its_own_settings())
chk("1.17.0", "restocking runs after the trading buy, so the purse and the cargo go to trade goods first, and it asks nothing about profit",
    restocking_runs_after_the_trading_buy())
chk("1.17.0", "the food it restocks to is a days-of-supply figure read off the party's own appetite",
    option_default('KeepFoodDays') == '3' and
    "return (int)Math.Ceiling(AppetitePerDay() * days);" in
        method_body(S['Policy.cs'], "internal static int FoodWanted") and
    "float perDay = party == null ? 0f : -party.FoodChange;" in
        method_body(S['Policy.cs'], "private static float AppetitePerDay") and
    "good.IsFood && !good.HasHorse" in
        between(S['Rules.cs'], "internal static bool IsStorableFood", ";") and
    "_o.KeepFoodDays" in M)
chk("1.19.0", "smeltable weapons are a three-way choice that ships on selling them, and an always-sell entry still wins",
    option_default('KeepSmeltableWeapons') == 'SmeltSellThem' and
    "public const int SmeltSellThem = 0, SmeltKeepAll = 1, SmeltKeepUnlearned = 2;" in S['Options.cs'] and
    "item.WeaponDesign != null" in method_body(S['Policy.cs'], "internal static bool IsSmeltable") and
    ordered(sell_rule(),
            "if (Listed(s.AlwaysSet, good)) { said.Allowed = true; return said; }",
            "s.KeepSmeltableWeapons != Options.SmeltSellThem && game.Smeltable()",
            "s.KeepSmeltableWeapons == Options.SmeltKeepAll || !game.PartsAllLearned()") and
    "_o.KeepSmeltableWeapons" in M)
chk("1.17.0", "the purse holds the flat reserve and the days of wages together, worked out without the game",
    option_default('KeepWageDays') == '0' and option_default('GoldReserve') == '300' and
    'public static int Reserve(int goldReserve, int keepWageDays, int totalWage)' in S['TradeMath.cs'] and
    'TaleWorlds' not in S['TradeMath.cs'] and
    'TradeMath.Reserve(Options.Current.GoldReserve, Options.Current.KeepWageDays, wage)' in
        method_body(S['Trading.cs'], "internal static int GoldHeldBack") and
    'MobileParty.MainParty?.TotalWage' in S['Trading.cs'] and '_o.KeepWageDays' in M)
chk("1.17.0", "the ships' capacity is asked for in one place, and the panel and the haul animal floor read that same number",
    option_default('UseFleetCapacity') == 'false' and the_ships_capacity_is_asked_in_one_place())
chk("1.25.0", "the settings file is read whether or not MCM is there, and it carries every option",
    "McmLoader.SettingsReachable" not in
        method_body(S['Config.cs'], "internal static void Follow") and
    ordered(S['SubModule.cs'], 'Guard.Run("McmLoader", McmLoader.TryLoad);', 'Config.Follow();') and
    "typeof(Options).GetFields(BindingFlags.Public | BindingFlags.Instance)" in
        between(S['Config.cs'], "private static FieldInfo[] Fields()", ";") and
    S['Config.cs'].count("CultureInfo.InvariantCulture") >= 3 and
    "Options.Bump();" in S['Config.cs'] and
    'Log.Beside(FileName, mustExist: true)' in S['Config.cs'])

def pack_animals_are_bought_before_the_profit_pass():
    menu = method_body(S['Trading.cs'], "private void OnSessionLaunched")
    entry = method_body(S['Trading.cs'], "private void OnSettlementEntered")
    body = pass_body("public static void ExecuteHaulage")
    return (ordered(menu, "ExecuteHaulage(Settlement.CurrentSettlement);",
                    "ExecuteQuickBuy(Settlement.CurrentSettlement);",
                    "ExecuteResupply(Settlement.CurrentSettlement);")
            and ordered(entry, "ExecuteHaulage(settlement, quiet: true);",
                        "ExecuteQuickBuy(settlement, quiet: true);",
                        "ExecuteResupply(settlement, quiet: true);")
            and "if (Options.Current.AutoBuyOnEntry) ExecuteHaulage(settlement, quiet: true);" in entry
            and "if (!Options.Current.BuyHaulAnimals) return;" in body
            and "it => TradePolicy.MayHaul(it, pass.Locked)" in body
            and "found.Sort((x, y) => x.price.CompareTo(y.price));" in body
            and "pass.WouldReachYourReserve(price)" in body
            and "settlement.IsVillage && remaining <= 1" in body
            and "BuyAcceptable" not in body
            and "BestSell" not in body)

def only_a_carrying_animal_is_hauled_and_the_herd_still_binds():
    haul = haul_rule()
    body = pass_body("public static void ExecuteHaulage")
    fence = "if (livestock && (good.IsHaulAnimal || good.IsSpareMount))" in sell_rule()
    carrying = between(S['Policy.cs'], "internal static bool IsHaulAnimal", ";")
    spare = between(S['Policy.cs'], "internal static bool IsSpareMount", ";")
    return ("if (good.Id == null || !good.IsHaulAnimal || good.NotMerchandise) return false;" in haul
            and "Listed(s.NeverSet, good) || Listed(s.NeverBuySet, good)" in haul
            and "return !game.Locked();" in haul
            and "IsHaulAnimalOrMount" not in haul
            and "IsSpareMount" not in haul + body
            and fence
            and "item.HorseComponent.IsRideable && item.HorseComponent.IsPackAnimal" in carrying
            and "!item.HorseComponent.IsMount && !item.HorseComponent.IsLiveStock" in carrying
            and "item.ItemCategory == DefaultItemCategories.PackAnimal" in carrying
            and "item.HorseComponent.IsMount" in spare
            and "IsLiveStock" not in spare
            and "int herdRoom = Drove.RoomForLivestock(pass.Party);" in body
            and "if (herdRoom <= 0) return;" in body
            and "herdRoom--;" in body
            and "FreeMountRoom" not in ALL
            and "Carry.Room" not in body)

def a_pack_animal_is_bought_only_at_the_cheapest_price_and_never_below_the_reserve():
    body = pass_body("public static void ExecuteHaulage")
    most = method_body(S['TradeMath.cs'], "public static int MostToPayOverTheCheapest")
    return ("int worth = TradePolicy.UnpaidWorth(it);" in body
            and body.count("price > ceiling") == 2
            and "int ceiling = TradeMath.MostToPayOverTheCheapest(worth, tolerance);" in body
            and "Options.Current.HaulAnimalPriceTolerance);" in body
            and ordered(most, "if (cheapest <= 0) return 0;",
                        "if (float.IsNaN(tolerance) || tolerance <= 1f) return cheapest;",
                        "Math.Floor((double)cheapest * tolerance)")
            and option_default('HaulAnimalPriceTolerance') == '1.25f'
            and "_o.HaulAnimalPriceTolerance" in M
            and "CargoIsFull" not in ALL
            and "PackAnimalFullCargoPremium" not in S['Trading.cs']
            and "pass.WouldReachYourReserve(price)" in body
            and option_default('BuyHaulAnimals') == 'true'
            and "_o.BuyHaulAnimals" in M
            and "PackAnimalFullCargoPremium" not in M
            and "PackAnimalFullCargoPremium" not in S['Options.cs']
            and '"PackAnimalFullCargoPremium"' in S['Migrate.cs'])


def no_haul_animal_is_bought_until_the_purse_is_above_its_floor():
    t = S['Trading.cs']
    floor = method_body(t, "private static bool PurseBelowTheHaulAnimalFloor")
    haul = method_body(t, "public static void ExecuteHaulage")
    return (ordered(haul, "if (!Options.Current.BuyHaulAnimals) return;",
                    "Pass pass = Pass.Open(settlement, quiet);",
                    "if (PurseBelowTheHaulAnimalFloor(pass)) return;",
                    "int herdRoom = Drove.RoomForLivestock(pass.Party);")
            and ordered(floor, "int floor = Options.Current.HaulAnimalGoldFloor;",
                        "if (floor <= 0) return false;",
                        "int purse = Hero.MainHero.Gold + pass.Books.Purse(pass.Sim);",
                        "if (purse > floor) return false;",
                        'Log.Repeatable("haul animal floor"',
                        "return true;")
            and t.count("PurseBelowTheHaulAnimalFloor(") == 2
            and option_default('HaulAnimalGoldFloor') == '2000'
            and "_o.HaulAnimalGoldFloor" in M
            and "0 = " in re.search(r'\{=TL425\}([^"]*)"', M).group(1)
            and 'TradeMath.cs' in TESTPROJ
            and all(one in MATHTESTS for one in
                    ("A_tolerance_of_one_pays_no_more_than_the_cheapest_ever_seen",
                     "A_quarter_over_the_cheapest_is_what_the_shipped_tolerance_allows",
                     "A_tolerance_below_one_never_pays_less_than_the_cheapest",
                     "A_cheapest_of_nothing_is_no_ceiling_at_all",
                     "A_tolerance_that_is_not_a_number_falls_back_to_the_cheapest",
                     "The_ceiling_never_overflows_however_large_the_tolerance")))

def the_getaway_ships_on_names_no_cheat_and_only_answers_bandits():
    asked = method_body(S['Encounters.cs'], "private static void AddBanditLines")
    met = method_body(S['Encounters.cs'], "private static bool BanditMet")
    go = method_body(S['Encounters.cs'], "private static void LetPlayerGo")
    return (option_default('BanditFreePassage') == 'true'
            and "_o.BanditFreePassage" in M
            and "AddGetaway" not in S['Trading.cs']
            and "FacingBandits" not in S['Trading.cs']
            and '"encounter"' not in S['Trading.cs']
            and '("encounter", false)' not in COMPAT
            and "Options.Current.BanditFreePassage && band != null && band.IsBandit;" in met
            and "{=TL113}" in asked
            and "band?.IgnoreForHours(GetawayHours);" in go
            and "MobileParty.MainParty?.IgnoreByOtherPartiesTill(CampaignTime.HoursFromNow(GetawayHours));" in go
            and "PlayerEncounter.ProtectPlayerSide(GetawayHours);" in go
            and "PlayerEncounter.LeaveEncounter = true;" in go
            and no_shipped_line_calls_the_free_passage_a_cheat())

def no_shipped_line_calls_the_free_passage_a_cheat():
    en = spoken(ENGLISH)
    lift = re.search(r'Renamed\s*=\s*\{(.*?)\};', S['Migrate.cs'], re.S)
    code = (ALL + "\n" + M).replace(lift.group(0), '') if lift else ALL + "\n" + M
    return (lift is not None
            and '[TRADELORD]' in en.get('TL387', '')
            and en.get('TL269') == 'Free passage from bandits'
            and 'TL112' not in en
            and not any('cheat' in text.lower() for text in en.values())
            and 'cheat' not in code.lower()
            and '(17, "BanditGetawayCheat", "BanditFreePassage"),' in lift.group(1)
            and 'public bool BanditFreePassage = true;' in S['Options.cs'])

def the_smeltable_hint_says_which_weapons_it_holds_back():
    hint = spoken(ENGLISH).get('TL364', '')
    said = [spoken(path).get('TL364', '') for path in TRANSLATIONS.values()]
    words = [spoken(ENGLISH).get(i, '') for i in ('TL270', 'TL271', 'TL272')]
    return ("looted off a bandit" in hint
            and "still locked in your smithy" in hint
            and hint in M and all(t and t != hint for t in said)
            and all(w and w in M for w in words))

def an_unreadable_crafting_record_keeps_the_weapon():
    body = method_body(S['Policy.cs'], "internal static bool PartsAllLearned")
    return ("if (design == null) return true;" in body
            and "Campaign.Current?.GetCampaignBehavior<ICraftingCampaignBehavior>()" in body
            and "crafting.IsOpened(piece, design.Template)" in body
            and body.count("return false;") == 3
            and "_craftingLookupFailed = true;" in body
            and 'Log.Error(e, "learned parts check (the weapon is kept)")' in body
            and "TradePolicy.ForgetCraftingLookup();" in
                method_body(S['Trading.cs'], "internal static void ForgetVisit"))

chk("1.18.0", "pack animals are bought before the goods are, so the room they add is there to fill, and nothing about profit is asked",
    pack_animals_are_bought_before_the_profit_pass())
chk("1.19.0", "only an animal that carries for you is bought that way, the herd guard still binds it and the carry weight never does",
    only_a_carrying_animal_is_hauled_and_the_herd_still_binds())
chk("1.30.0", "a haul animal is bought only within the ceiling over the cheapest TradeLord has seen, and never below the gold reserve",
    a_pack_animal_is_bought_only_at_the_cheapest_price_and_never_below_the_reserve())
chk("1.75.0", "no haul animal is bought at all until your purse is above the floor its own setting names, however cheap one is",
    no_haul_animal_is_bought_until_the_purse_is_above_its_floor())
chk("1.36.0", "the getaway is offered as you meet a band, and leaving holds both sides off each other",
    the_getaway_ships_on_names_no_cheat_and_only_answers_bandits())
chk("1.19.0", "the smeltable hint names all three choices and says looted weapons are held too",
    the_smeltable_hint_says_which_weapons_it_holds_back())
chk("1.19.0", "a weapon is kept whenever the game's crafting record cannot say its parts are all learned",
    an_unreadable_crafting_record_keeps_the_weapon())

def a_horse_a_footman_can_ride_costs_the_herd_nothing():
    tally = method_body(S['Drove.cs'], "private static bool Tally")
    room = method_body(S['Drove.cs'], "internal static int RoomForLivestock")
    spare = method_body(S['Drove.cs'], "internal static int SpareMounts")
    haul = method_body(S['Trading.cs'], "public static void ExecuteHaulage")
    kind = between(S['Policy.cs'], "internal static bool IsSpareMount", ";")
    return ("party.AttachedParties" in tally
            and "NumberOfMenWithoutHorse" in tally
            and "herd = Herding.DrivenInAll(herd, mounts, foot);" in room
            and "Herding.MountsNobodyRides(mounts, foot)" in spare
            and "A_horse_a_man_on_foot_can_ride_is_ridden_rather_than_driven" in HERDTESTS
            and "Putting_a_man_on_foot_never_makes_the_herd_larger" in HERDTESTS
            and "Tally(party, out int men, out int herd, out int mounts, out int foot)" in room
            and "IsMount && !item.HorseComponent.IsPackAnimal" in kind
            and "while (remaining > 0 && herdRoom > 0)" in haul)

def a_share_of_the_hold_caps_one_good_and_ships_off():
    buy = buy_pass()
    return (option_default('MaxHeldShare') == '0.45f'
            and "_o.MaxHeldShare" in M
            and "float shareCap = pass.ShareCap;" in buy
            and buy.count("shareCap > 0f") == 2
            and buy.count("(held + 1) * good.Weight > shareCap") == 2
            and "MaxHeldShare" not in sell_pass()
            and (lambda src: "internal float Capacity => _capacity < 0f ? _capacity = Carry.Capacity(Party) : _capacity;" in src
                         and "TradeMath.RoomToFill(Capacity, Carried(), Options.Current.MaxCargoShare);" in src
                         and "Options.Current.MaxHeldShare > 0f ? Capacity * Options.Current.MaxHeldShare : 0f;" in src
                         and src.count("Options.Current.MaxHeldShare") == 2
                         and src.count("Carry.Capacity(") == 1
                         and src.count("Carry.Room(") == 1
                         and "Carry.Room(party) < 1f;" in method_body(src, "private static bool NoRoomToCarry"))
                (S['Trading.cs'])
            and "MaxHeldShare" not in sell_rule()
            and "MaxHeldShare" not in S['Ledger.cs'])

def a_road_party_is_traded_with_the_moment_it_is_met():
    watch = method_body(S['Encounters.cs'], "internal static void Watch")
    return ('Guard.Run("Tick.Encounter", Meetings.Watch);' in
                method_body(S['SubModule.cs'], "protected override void OnApplicationTick")
            and "Patch_TradeOnMeeting" not in ALL
            and ordered(watch, "if (Campaign.Current == null) { _handledEncounter = null; Parley.Forget(); return; }",
                        "object here = PlayerEncounter.Current;")
            and "if (here == null) { _handledEncounter = null; return; }" in watch
            and "if (_handledEncounter == here) return;" in watch
            and "if (met == null) return;" in watch
            and "if (IsRoadTrader(met)) TradeOnce(met);" in watch
            and "OfferFreePassage" not in ALL
            and "party.IsCaravan || party.IsVillager" in
                between(S['Encounters.cs'], "internal static bool IsRoadTrader", ";"))

def bandits_are_offered_the_getaway_without_a_menu_of_their_own():
    asked = method_body(S['Encounters.cs'], "private static void AddBanditLines")
    return ('starter.AddPlayerLine(\n                    "tradelord_bandit_pass", Parley.OwnState,' in asked
            and '"tradelord_bandit_pass_reply", "tradelord_bandit_pass_reply", "close_window"' in asked
            and "Parley.Remember(asked);" in asked
            and "InformationManager.ShowInquiry" not in asked
            and not any(s in strings_declared() for s in ("TL378", "TL379", "TL380"))
            and "Meetings.ForgetEncounter" in S['SubModule.cs']
            and "_handledEncounter = null;" in
                method_body(S['Encounters.cs'], "internal static void ForgetEncounter"))

def a_caravan_on_the_road_is_priced_by_the_game_not_by_the_mod():
    body = method_body(S['Trading.cs'], "public static void ExecuteRoadTrade")
    market = method_body(S['Trading.cs'], "private static IMarketData RoadMarket")
    met = method_body(S['Encounters.cs'], "private static bool CaravanMet")
    once = method_body(S['Encounters.cs'], "private static void TradeOnce")
    return (option_default('TradeWithCaravans') == 'true' and "_o.TradeWithCaravans" in M
            and '"TaleWorlds.CampaignSystem.Settlements.FakeMarketData"' in market
            and "as IMarketData" in market
            and "_roadMarketFailed = true;" in market
            and "if (!Options.Current.TradeWithCaravans) return;" in body
            and "if (!RoadPartyReachable(met)) return;" in body
            and "IMarketData road = PricedOnTheRoad(met);" in body
            and "if (road == null) return;" in body
            and "Road.GetPrice(what, Party, selling, Shop)" in
                between(S['Trading.cs'], "internal int Price(", ";")
            and "internal int TillNow => Site != null ? Market.Gold : Met.PartyTradeGold;"
                in S['Trading.cs']
            and "if (TradeRules.WhatTheTillCanPay(pass.Sim ? simTill : pass.TillNow," in S['Trading.cs']
            and "TradeOnce(caravan);" in met
            and "object here = PlayerEncounter.Current;" in once
            and "if (_tradedWith == met || (here != null && _tradedIn == here)) return;" in once
            and "_tradedIn = here;" in once
            and "_tradedIn = null;" in method_body(S['Encounters.cs'], "internal static void ForgetWhoYouTradedWith")
            and "ForgetWhoYouTradedWith();" in method_body(S['Encounters.cs'], "internal static void ForgetEncounter")
            and "Meetings.ForgetWhoYouTradedWith();" in method_body(S['Trading.cs'], "internal static void ForgetVisit")
            and "=> Meetings.ConversationEnded()" in between(S['Trading.cs'], "private void OnConversationEnded", ";")
            and "CampaignEvents.ConversationEnded.AddNonSerializedListener(this, OnConversationEnded);"
                in S['Trading.cs']
            and "ForgetRoadMarket();" in method_body(S['Trading.cs'], "internal static void ForgetVisit"))

def a_caravan_trade_obeys_every_rule_a_market_visit_does():
    t = S['Trading.cs']
    road = method_body(t, "public static void ExecuteRoadTrade")
    sell = sell_pass()
    buy = buy_pass()
    return ("if (StillSettling(Muted(automated: true))) return;" in road
            and 'SellPass(Pass.Meet(met, road, books, party),' in road
            and 'BuyPass(Pass.Meet(met, road, books, party),' in road
            and 'SellPass(Pass.Open(settlement, quiet), "quick-sell"' in t
            and 'BuyPass(Pass.Open(settlement, quiet), "quick-buy"' in t
            and t.count("private static void SellPass") == 1
            and t.count("private static void BuyPass") == 1
            and "TradePolicy.MaySell(good, _plan[at], _pass.Locked, _keepBack, _awaited," in sell
            and "TradeMath.ProfitAcceptable(mustBeat, price, s.MinProfitMargin)" in sell
            and "s.PreferBestSellTown" in sell
            and "s.BestSellTownTolerance" in sell
            and ordered(sell, "if (!TradeMath.ProfitAcceptable(mustBeat, price, s.MinProfitMargin))",
                        "if (!basis.SkipTheUnitsYouPaidFor(ref remaining)) break;",
                        "TradeMath.SkipTheUnitsYouPaidFor(FromMarket, ref remaining, ref PaidLeft)",
                        "if (basisIsMarket || paidLeft <= 0 || remaining <= paidLeft) return false;",
                        "remaining -= paidLeft;", "paidLeft = 0;")
            and "TradePolicy.MayBuy(good, Item(at), _pass.Locked, out why)" in buy
            and "TradeRules.ResaleAllowed(good, s)" in buy
            and "int wouldDraw = market.ResaleUpTo(picked.At, held + 1);" in buy
            and "TradeRules.TheBuyerCouldNotPay(wouldDraw, till)" in buy
            and "s.ResaleSafetyFactor" in buy
            and "s.MinProfitMargin" in buy
            and "TradeRules.WhatStopsBuying(good, price, market.Spendable()," in buy
            and "TradeRules.NoRoomForOneMore(good, market.Room() - simWeight)" in buy
            and "s.BuyCapPerItem" in buy
            and "s.BuyValueCapPerItem" in buy
            and "s.MaxHeldPerItem" in buy
            and "(held + 1) * good.Weight > shareCap" in buy
            and "float shareCap = pass.ShareCap;" in buy
            and "herdRoom = Math.Max(0, market.HerdRoom() - books.HerdTaken(sim));" in buy
            and "public int HerdRoom() => Drove.RoomForLivestock(_pass.Party);" in buy
            and "if (livestock && herdRoom <= 0) return Block.HerdFull;" in buy
            and buy.count("if (livestock) herdRoom--;") == 1
            and "Books books = BooksForTheMeeting(met);" in road
            and t.count("private static readonly Books Visit = new Books();") == 1
            and "Visit" not in road)

def the_caravan_line_closes_the_conversation_on_the_caravans_own_answer():
    lines = method_body(S['Encounters.cs'], "private static void AddCaravanLines")
    return ('starter.AddPlayerLine("tradelord_caravan_done", "caravan_talk", "tradelord_caravan_reply",' in lines
            and 'starter.AddDialogLine("tradelord_caravan_reply", "tradelord_caravan_reply", "close_window",'
                in lines
            and "{=TL114}" in lines and "{=TL115}" in lines
            and "&& CaravanMet(), null, 200);" in lines
            and "() => Tongue.Spoken(answered), null, 200);" in lines
            and "TL114" in strings_declared() and "TL115" in strings_declared())

chk("1.20.0", "a horse an unmounted man can ride costs the herd nothing, so a full herd no longer blocks one",
    a_horse_a_footman_can_ride_costs_the_herd_nothing())
chk("1.20.0", "a share of the hold caps one good against the real capacity, ships off and binds buying only",
    a_share_of_the_hold_caps_one_good_and_ships_off())
chk("1.36.0", "a caravan or a party of villagers is traded with as soon as the encounter names it, before any dialog",
    a_road_party_is_traded_with_the_moment_it_is_met())
chk("1.36.0", "bandits offer the getaway on meeting, so it never depends on a menu of the game's own",
    bandits_are_offered_the_getaway_without_a_menu_of_their_own())
chk("1.20.0", "a caravan on the road is priced by the game's own off-market pricing, and is skipped when that is gone",
    a_caravan_on_the_road_is_priced_by_the_game_not_by_the_mod())
chk("1.20.0", "a caravan trade obeys the same rules a market visit does and keeps out of the visit counters",
    a_caravan_trade_obeys_every_rule_a_market_visit_does())
chk("1.20.0", "the caravan's own answer closes the conversation, with no farewell to click after it",
    the_caravan_line_closes_the_conversation_on_the_caravans_own_answer())

def every_pass_that_really_moves_goods_rings_the_coin():
    passes = ["private static void SellPass", "public static void ExecuteResupply",
              "public static void ExecuteHerdRelief", "public static void ExecuteHaulage",
              "private static void BuyPass"]
    return (option_default('CoinSound') == 'true'
            and all("pass.Moved(" in method_body(S['Trading.cs'], one) for one in passes)
            and "CoinSound();" in method_body(S['Trading.cs'], "internal void Moved")
            and S['Trading.cs'].count("CoinSound();") == 1
            and "if (!Options.Current.CoinSound) return;" in
                method_body(S['Trading.cs'], "private static void CoinSound"))

chk("1.36.1", "a pass that really moves goods rings the coin, selling on the road along with the rest",
    every_pass_that_really_moves_goods_rings_the_coin())

def the_pack_animal_line_lands_after_the_trade_skill_line():
    flush = method_body(S['Trading.cs'], "internal static void FlushToasts")
    haul = method_body(S['Trading.cs'], "public static void ExecuteHaulage")
    forget = method_body(S['Trading.cs'], "internal static void ForgetVisit")
    return (ordered(flush, "if (xp > 0) CreditTradeSkill(xp, profit, muted);", "Notices.Drain();")
            and ordered(method_body(S['Notices.cs'], "internal static void Drain"),
                        "_pending.AddRange(_afterXp);", "_afterXp.Clear();",
                        "if (_pending.Count == 0) return;")
            and "{=TL110}" in haul
            and "if (!pass.Muted) Notices.SayAfterXp(msg, Notices.Spend);" in haul
            and "Notices.Say(msg, Notices.Spend);" not in haul
            and S['Trading.cs'].count("Notices.SayAfterXp(") == 1
            and S['Notices.cs'].count("internal static void SayAfterXp(") == 1
            and "Notices.Forget();" in forget)

chk("1.20.1", "the pack animal line waits for the trade skill line and is dropped with the rest when a visit is forgotten",
    the_pack_animal_line_lands_after_the_trade_skill_line())

def a_spare_mount_goes_only_when_it_is_costing_the_party_speed():
    shed = method_body(S['Drove.cs'], "internal static int AnimalsToShed")
    relief = method_body(S['Trading.cs'], "public static void ExecuteHerdRelief")
    spare = shed_rule()
    entered = method_body(S['Trading.cs'], "private void OnSettlementEntered")
    return (option_default('SellSpareMounts') == 'true'
            and "_o.SellSpareMounts" in M
            and "int driven = Herding.DrivenInAll(herd, mounts, foot);" in shed
            and "if (driven <= 0) return 0;" in shed
            and 'float neutral = (float)_modifier.Invoke(model, new object[] { men, 0 });' in shed
            and "return TradeMath.MostThatHolds(driven, shed => shed == 0 || !TradeMath.Unchanged(" in shed
            and "new object[] { men, driven - shed + 1 }), neutral));" in shed
            and "!good.HasHorse || good.NotMerchandise" in spare
            and "Listed(s.NeverSet, good)" in spare
            and "s.ProtectSpecial && (good.IsUnique || good.IsCraftedByPlayer)" in spare
            and "return !game.Locked();" in spare
            and "new AskTheGame { Locks = lockedKeys, What = held }" in S['Policy.cs']
            and "internal static bool MayShedForHerd(EquipmentElement held, ISet<string> lockedKeys)"
                in S['Policy.cs']
            and "!TradePolicy.MayShedForHerd(el.EquipmentElement, pass.Locked)" in relief
            and "if (!Options.Current.SellSpareMounts) return;" in relief
            and "int shed = Drove.AnimalsToShed(pass.Party);" in relief
            and "if (shed <= 0) return;" in relief
            and "pass.Books.NoteSold(item.StringId);" in relief
            and "while (remaining > 0 && shed > 0)" in relief
            and ordered(entered, "ExecuteQuickSell(settlement, quiet: true)",
                        "ExecuteHerdRelief(settlement, quiet: true)",
                        "ExecuteHaulage(settlement, quiet: true)",
                        "ExecuteQuickBuy(settlement, quiet: true)")
            and "if (Options.Current.AutoSellOnEntry) ExecuteHerdRelief(settlement, quiet: true);" in entered)

def the_herd_guard_keeps_a_cushion_below_the_speed_penalty():
    room = method_body(S['Drove.cs'], "internal static int RoomForLivestock")
    cushion = re.search(r'internal const int Cushion = (\d+);', S['Rules.cs'])
    return (cushion is not None and int(cushion.group(1)) > 0
            and "HerdCushion" not in S['Trading.cs']
            and "The_herd_guard_keeps_a_cushion_of_its_own" in HERDTESTS
            and "new object[] { men, herd + room + Herding.Cushion }), neutral));" in room
            and "float neutral = (float)_modifier.Invoke(model, new object[] { men, 0 });" in room
            and "return TradeMath.MostThatHolds(256, room => room == 0 || TradeMath.Unchanged(" in room)

def an_animal_is_held_back_when_the_quests_cannot_be_read():
    sell = sell_rule()
    relief = method_body(S['Trading.cs'], "public static void ExecuteHerdRelief")
    return ("internal static bool Known => Readable();" in S['Encounters.cs']
            and "facts.QuestsReadable = Errands.Known;" in
                method_body(S['Policy.cs'], "internal static bool MaySell(ItemRosterElement el")
            and "if (livestock && !facts.QuestsReadable) { said.Why = Block.QuestGoods; return said; }" in sell
            and ordered(sell, "if (Listed(s.AlwaysSet, good)) { said.Allowed = true; return said; }",
                        "if (livestock && !facts.QuestsReadable) { said.Why = Block.QuestGoods; return said; }")
            and "if (promised == null) return;" in relief
            and "no animal is sold at all" in S['Encounters.cs']
            and "no animal is sold to relieve the herd" not in S['Trading.cs'])

def the_buying_pass_counts_what_you_hold_afresh_for_each_good():
    buy = buy_pass()
    return ("var stock = new List<Pick>();" in buy
            and "alreadyHeld" not in S['Trading.cs'] + S['Passes.cs']
            and "public int Carried(int at) => LedgerBehavior.InAll(_pass.Party.ItemRoster, Item(at));" in buy
            and "int carried = market.Carried(one.At) + books.Held(sim, one.Good.Id);" in buy
            and "int held = market.Carried(picked.At) + books.Held(sim, good.Id);" in buy
            and "Carried" not in method_body(S['Passes.cs'], "internal struct Pick")
            and ordered(method_body(S['Passes.cs'], "internal static Traded BuyThem"),
                        "var prior = books.Purchases(sim, good.Id);",
                        "int held = market.Carried(picked.At) + books.Held(sim, good.Id);",
                        "while (remaining > 0)"))

chk("1.37.9", "an animal is held back from every sale, not just herd thinning, when the quests cannot be read",
    an_animal_is_held_back_when_the_quests_cannot_be_read())
chk("1.37.9", "the buying pass counts what you already carry afresh for each good it prices",
    the_buying_pass_counts_what_you_hold_afresh_for_each_good())
chk("1.22.0", "buying livestock stops a cushion short of the speed penalty, not right at its edge",
    the_herd_guard_keeps_a_cushion_below_the_speed_penalty())
chk("1.22.0", "an animal is sold only while the herd is dragging the party below its speed, and only as many as that takes",
    a_spare_mount_goes_only_when_it_is_costing_the_party_speed())

def the_herd_gives_up_its_animals_in_the_order_the_player_set():
    relief = method_body(S['Trading.cs'], "public static void ExecuteHerdRelief")
    rank = rank_rule()
    spared = method_body(S['Drove.cs'], "internal static int HaulAnimalsCargoCanSpare")
    held = method_body(S['Drove.cs'], "internal static int HaulAnimalsHeld")
    room = method_body(S['Drove.cs'], "internal static int SpareMounts")
    return (all(line in S['Rules.cs'] for line in
                ("internal const int RankLivestock = 0;", "internal const int RankPlainMount = 1;",
                 "internal const int RankHaulAnimal = 2;", "internal const int RankPrizeMount = 3;",
                 "internal const int RankNotAnAnimal = -1;"))
            and "if (good.IsLivestock) return RankLivestock;" in rank
            and "if (good.IsSpareMount) return good.IsPrizeMount ? RankPrizeMount : RankPlainMount;" in rank
            and "if (good.IsHaulAnimal) return RankHaulAnimal;" in rank
            and "return RankNotAnAnimal;" in rank
            and "stable.Sort((x, y) => x.rank != y.rank ? x.rank.CompareTo(y.rank) : x.price.CompareTo(y.price));" in relief
            and "int mountsLeft = Drove.SpareMounts(pass.Party);" in relief
            and "int haulsLeft = -1;" in relief
            and "if (rank == RankHaulAnimal && haulsLeft < 0)\n"
                "                            haulsLeft = Math.Max(0, Drove.HaulAnimalsCargoCanSpare(pass.Party)"
                " - pass.Books.HaulsShed(pass.Sim));" in relief
            and "if (rank != RankLivestock && rank != RankHaulAnimal && mountsLeft <= 0) break;" in relief
            and "if (rank == RankHaulAnimal && haulsLeft <= 0) break;" in relief
            and "if (rank == RankHaulAnimal) haulsLeft--;" in relief
            and "else if (rank != RankLivestock) mountsLeft--;" in relief
            and "Herding.MountsNobodyRides(mounts, foot)" in room
            and "TradePolicy.IsHaulAnimal(el.EquipmentElement.Item)" in held
            and "bool atSea = Carry.Sailing();" in spared
            and "model.CalculateTotalWeightCarried(party, atSea).ResultNumber" in spared
            and "model.CalculateInventoryCapacity(party, atSea, false, 0, 0, -fewer).ResultNumber >= carried);" in spared
            and "return TradeMath.MostThatHolds(held, fewer => fewer == 0 ||" in spared)

chk("1.28.0", "the herd gives up its livestock, then a plain spare mount, then a haul animal, and a war or noble horse last of all",
    the_herd_gives_up_its_animals_in_the_order_the_player_set())

def getting_back_up_to_speed_outranks_the_food_reserve():
    relief = method_body(S['Trading.cs'], "public static void ExecuteHerdRelief")
    spare = shed_rule()
    sell = method_body(S['Policy.cs'], "internal static bool MaySell(in Good good, ItemRosterElement el")
    return ("FoodKeep" not in relief and "foodKeep" not in relief
            and "foodKeep" not in spare and "FoodValue" not in spare
            and "foodKeep" in sell
            and "if (good.IsLivestock) return RankLivestock;" in rank_rule())

chk("1.36.2", "a herd that is slowing the party down is thinned even when its livestock is the food you set aside",
    getting_back_up_to_speed_outranks_the_food_reserve())

def an_animal_a_quest_is_waiting_on_is_counted_out_of_the_herd():
    errands = method_body(S['Encounters.cs'], "internal static class Errands")
    promised = method_body(S['Encounters.cs'], "internal static Dictionary<ItemObject, int> Promised")
    relief = method_body(S['Trading.cs'], "public static void ExecuteHerdRelief")
    quests = ("HeadmanNeedsToDeliverAHerdIssueBehavior.HeadmanNeedsToDeliverAHerdIssueQuest",
              "HeadmanVillageNeedsDraughtAnimalsIssueBehavior.HeadmanVillageNeedsDraughtAnimalsIssueQuest",
              "LordNeedsHorsesIssueBehavior.LordNeedsHorsesIssueQuest")
    named = ("_herdTypeToDeliver", "_animalCountToDeliver", "_requestedAnimal",
             "_requestedAnimalAmount", "_mountObjectToBeDelivered", "_numMountsToBeDelivered")
    return (all("typeof(" + q + ")" in errands for q in quests)
            and all('"' + f + '"' in errands for f in named)
            and "Campaign.Current?.QuestManager?.Quests" in promised
            and "if (quest == null || quest.IsFinalized) continue;" in promised
            and "if (!Readable()) return null;" in promised
            and "TradePolicy.KeptBack(mine, pass.Books, pass.Sim, out Dictionary<ItemObject, int> promised);" in relief
            and "if (promised == null) return;" in relief
            and "if (promised.TryGetValue(item, out int owed) && owed > 0)" in relief
            and "int spare = Math.Min(remaining, owed);" in relief
            and "promised[item] = owed - spare;" in relief
            and "remaining -= spare;" in relief
            and "Errands.Forget();" in method_body(S['Trading.cs'], "internal static void ForgetVisit"))

chk("1.37.0", "as many animals as a quest is waiting on are kept back, and only the herd beyond them is thinned",
    an_animal_a_quest_is_waiting_on_is_counted_out_of_the_herd())

def a_quest_animal_is_held_back_from_every_sale_not_just_the_herd():
    kept = method_body(S['Policy.cs'], "internal static Dictionary<ItemObject, int> KeptBack")
    return ("awaited = Errands.Promised(out int anyLivestock);" in kept
            and "TradeRules.LivestockKeep(carried, anyLivestock)" in kept
            and "if (awaited == null) return keep;" in kept
            and "facts.AwaitedHeld = HeldBack(awaited, item);" in
                method_body(S['Policy.cs'], "internal static bool MaySell(in Good good, ItemRosterElement el")
            and "TradePolicy.MaySell(good, _plan[at], _pass.Locked, _keepBack, _awaited," in S['Trading.cs']
            and "TradePolicy.MaySell(el, locked, keepBack, awaited, out int keep)" in S['Marker.cs']
            and "TradePolicy.FoodKeep(" not in S['Trading.cs']
            and S['Trading.cs'].count("TradePolicy.KeptBack(") == 2
            and S['Marker.cs'].count("TradePolicy.KeptBack(") == 1
            and all("TradePolicy.KeptBack(" in method_body(S['Trading.cs'], where)
                    for where in ("private sealed class SellingFrom",
                                  "public static void ExecuteHerdRelief"))
            and "TradePolicy.KeptBack(" in method_body(S['Marker.cs'],
                    "private static List<(EquipmentElement item, int amount, int worth, int floor)> "
                    "WhatYouCarryToSell"))

chk("1.37.5", "an animal a quest is waiting on is held back from every sale, not only from thinning the herd",
    a_quest_animal_is_held_back_from_every_sale_not_just_the_herd())

def the_herd_is_looked_at_three_times_a_visit():
    entered = method_body(S['Trading.cs'], "private void OnSettlementEntered")
    left = method_body(S['Trading.cs'], "private void OnSettlementLeft")
    launched = method_body(S['Trading.cs'], "private void OnSessionLaunched")
    return ('Drove.LogState("entering " + settlement.Name);' in entered
            and 'Drove.LogState("after trading at " + settlement.Name);' in entered
            and entered.count("ExecuteHerdRelief(settlement, quiet: true)") == 2
            and ordered(entered, "ExecuteQuickSell(settlement, quiet: true)",
                        "ExecuteHerdRelief(settlement, quiet: true)",
                        "ExecuteHaulage(settlement, quiet: true)",
                        "ExecuteQuickBuy(settlement, quiet: true)",
                        "ExecuteResupply(settlement, quiet: true)")
            and ordered_last(entered, "ExecuteQuickBuy(settlement, quiet: true)",
                             "ExecuteHerdRelief(settlement, quiet: true)",
                             'Drove.LogState("after trading at " + settlement.Name);')
            and 'Drove.LogState("leaving " + settlement.Name);' in left
            and "if (Options.Current.AutoSellOnEntry) ExecuteHerdRelief(settlement, quiet: true);" in left
            and "if (!_visitTradeAllowed)" in left
            and ordered(left, 'Drove.LogState("leaving " + settlement.Name);', "if (!_visitTradeAllowed)",
                        "if (Options.Current.AutoSellOnEntry) ExecuteHerdRelief(settlement, quiet: true);")
            and "_visitTradeAllowed = CanTradeHere(settlement);" in
                method_body(S['Trading.cs'], "private void OnSettlementEntered")
            and "_visitTradeAllowed = false;" in method_body(S['Trading.cs'], "private static void ResetVisit")
            and 'Guard.Run("Action.HerdReliefOnLeaving"' in left
            and 'if (shed > 0) Drove.LogState("on the road, no market in reach", shed);' in
                method_body(S['Trading.cs'], "private void OnDailyTick")
            and "int shed = Drove.AnimalsToShed(MobileParty.MainParty);" in
                method_body(S['Trading.cs'], "private void OnDailyTick")
            and method_body(S['Trading.cs'], "private void OnDailyTick").count(
                "Drove.AnimalsToShed(") == 1
            and launched.count("ExecuteHerdRelief(Settlement.CurrentSettlement);") == 2
            and ordered(launched, "ExecuteQuickSell(Settlement.CurrentSettlement);",
                        "ExecuteHerdRelief(Settlement.CurrentSettlement);",
                        "ExecuteQuickBuy(Settlement.CurrentSettlement);")
            and ordered_last(launched, "ExecuteQuickBuy(Settlement.CurrentSettlement);",
                             "ExecuteHerdRelief(Settlement.CurrentSettlement);",
                             'Drove.LogState("after trading by hand at "'))

chk("1.29.0", "the herd is looked at on the way in, after the trading and on the way out, so a penalty from losing men is caught too",
    the_herd_is_looked_at_three_times_a_visit())

def a_loaded_game_inside_a_market_still_gets_back_up_to_speed_on_the_way_out():
    launched = method_body(S['Trading.cs'], "private void OnSessionLaunched")
    left = method_body(S['Trading.cs'], "private void OnSettlementLeft")
    return (ordered(launched, "ResetVisit();",
                    "Settlement inside = MobileParty.MainParty?.CurrentSettlement;",
                    "if (inside != null) _visitTradeAllowed = CanTradeHere(inside);")
            and ordered(left, "if (!_visitTradeAllowed)", "if (IsMarket(settlement))",
                        "herd relief on the way out is skipped")
            and left.count("if (IsMarket(settlement))") == 1)

chk("1.30.2", "a game loaded inside a market still gets the party back up to speed on the way out, and only a market that turned the trade down says so",
    a_loaded_game_inside_a_market_still_gets_back_up_to_speed_on_the_way_out())

def what_the_herd_check_writes_down():
    body = method_body(S['Drove.cs'], "internal static void LogState(string when, int counted)")
    split = method_body(S['Drove.cs'], "private static void Split")
    return ("internal static void LogState(string when) => LogState(when, -1);" in S['Drove.cs']
            and all(needle in body for needle in
                ("Tally(party, out int men, out int herd, out int mounts, out int foot)",
                 "Split(party, out int packs, out int stock)",
                 "int spare = Herding.MountsNobodyRides(mounts, foot);",
                 "int shed = counted >= 0 ? counted : AnimalsToShed(party);",
                 '" men of whom "', '" on foot, "', '" loose mount(s) with "',
                 '" pack animal(s), "', '" livestock, "', '" driven in all, "',
                 '"no herd penalty"'))
            and "roster.NumberOfPackAnimals" in split
            and "roster.NumberOfLivestockAnimals" in split
            and "attached[i]?.ItemRoster" in split
            and 'Log.Error(e, "herd check log (nothing else is affected)")' in body)

chk("1.29.0", "the herd check writes down the men, the mounts nobody rides, the pack animals, the livestock and what it decided",
    what_the_herd_check_writes_down())

def every_animal_that_moves_is_named_with_its_reason():
    moved = method_body(S['Trading.cs'], "private static void LogAnimalMoved")
    detail = method_body(S['Trading.cs'], "private static void LogDetail")
    src = S['Trading.cs']
    reasons = ("the selling pass", "restocking the larder", "trading with a party on the road",
               "herd relief, getting the party back up to speed", "stocking the baggage train",
               "the buying pass", "the deal you took on the trade screen")
    return ("if (item == null || !item.HasHorseComponent) return;" in moved
            and '"  animal " + (selling ? "out: " : "in: ")' in moved
            and '" - " + why + "; TradeLord counts it as " + TradePolicy.AnimalGroup(item)' in moved
            and "LogAnimalMoved(lines, selling, sim, kv.Key, kv.Value.count, kv.Value.gold, why);" in detail
            and 'lines.Add("  animal " + (selling ? "out: " : "in: ")' in moved
            and "LogDetail(selling, Sim, Detail, Quoted, Aimed, why)" in
                between(src, "internal void Logged(", ";")
            and src.count("pass.Logged(selling:") == 8
            and src.count("LogDetail(selling:") == 0
            and all(r in src for r in reasons))

chk("1.29.0", "every animal that comes in or goes out is named in the log with the reason it moved and what TradeLord counts it as",
    every_animal_that_moves_is_named_with_its_reason())
chk("1.28.0", "a horse a man on foot is riding is never sold to relieve the herd, because it is not in the herd",
    "int mountsLeft = Drove.SpareMounts(pass.Party);" in
        method_body(S['Trading.cs'], "public static void ExecuteHerdRelief") and
    "if (rank != RankLivestock && rank != RankHaulAnimal && mountsLeft <= 0) break;" in
        method_body(S['Trading.cs'], "public static void ExecuteHerdRelief"))
chk("1.28.0", "enough haul animals are kept to carry what the party already carries, asked of the game's own capacity model",
    "model.CalculateInventoryCapacity(party, atSea, false, 0, 0, -fewer).ResultNumber >= carried);" in
        method_body(S['Drove.cs'], "internal static int HaulAnimalsCargoCanSpare") and
    'Log.Error(e, "haul animal cargo floor (every haul animal is kept)")' in
        method_body(S['Drove.cs'], "internal static int HaulAnimalsCargoCanSpare"))
chk("1.28.0", "a name that means two animals the mod treats differently is named in the log, with the item id for each",
    "TradePolicy.ItemListsNameTwoAnimals();" in
        method_body(S['Trading.cs'], "private static void WarnUnmatchedItemLists") and
    all(needle in method_body(S['Policy.cs'], "internal static bool ItemListsNameTwoAnimals") for needle in
        ("if (Freshness.Fresh(ref _clashStamp, Stamp.Timeless)) return false;",
         "foreach (ItemObject item in Items.All)",
         "if (groups.Count < 2) continue;",
         'said.Add(item.StringId + " is " + AnimalGroup(item));')) and
    "_clashStamp.Stale();" in method_body(S['Policy.cs'], "internal static void ForgetItemListAudit"))
chk("1.33.0", "the language hint says the change takes hold as it is picked and names the one thing that waits",
    "It takes hold as you pick it" in spoken(ENGLISH).get('TL350', '') and
    "with no restart and no reload" in spoken(ENGLISH).get('TL350', '') and
    all(spoken(path).get('TL350', '') for path in TRANSLATIONS.values()) and
    '[SettingPropertyDropdown("{=TL250}Language", Order = 1, RequireRestart = false,' in M and
    spoken(ENGLISH).get('TL350', '') in M)
chk("1.33.0", "one name reaches the code, the settings screen and the log, and nothing calls these animals anything else",
    "TruckAnimal" not in ALL and "Truck" not in ALL and
    "BuyPackAnimals" not in S['Options.cs'] and "BuyPackAnimals" not in M and
    "Buy haul animals" in M and "_o.BuyHaulAnimals" in M and
    'if (good.IsHaulAnimal) return "a haul animal";' in
        method_body(S['Rules.cs'], "internal static string AnimalGroup") and
    "haul animals" in spoken(ENGLISH)['TL374'] and
    "pack animal" not in spoken(ENGLISH)['TL374'].lower())
chk("1.33.0", "a haul animal setting a player already saved is carried over to its new name rather than stranded",
    '(5, "BuyPackAnimals", "BuyHaulAnimals"),' in S['Migrate.cs'] and
    "BuyPackAnimals" in MIGRATIONTESTS and "BuyHaulAnimals" in MIGRATIONTESTS)

def no_hint_still_claims_a_mount_is_never_sold():
    en = spoken(ENGLISH)
    return ('haul animal' in en.get('TL375', '')
            and 'only way to sell a haul animal for profit' in en.get('TL332', '')
            and 'Mounts and pack animals' not in en.get('TL324', '')
            and 'Mounts and pack animals' not in en.get('TL325', '')
            and all('haul animal' in t.lower() or 'never sold' not in t.lower()
                    for t in (en.get('TL324', ''), en.get('TL325', ''), en.get('TL332', '')))
            and en.get('TL375', '') in M and en.get('TL275', '') in M)

chk("1.22.0", "no hint still tells the player a mount can never be sold",
    no_hint_still_claims_a_mount_is_never_sold())

def a_dropdown_only_ever_gains_choices_at_the_end():
    lists = {}
    for m in re.finditer(r'string\[\]\s+(\w+Words)\s*=\s*\{(.*?)\n\s*\};', M, re.S):
        lists[m.group(1)] = [re.sub(r'\{=\w+\}', '', w)
                             for w in re.findall(r'"((?:[^"\\]|\\.)*)"', m.group(2))]
    shipped = {
        'LanguageWords': ['English', 'T\\u00FCrk\\u00E7e',
                          '\\u0420\\u0443\\u0441\\u0441\\u043A\\u0438\\u0439', '\\u7B80\\u4F53\\u4E2D\\u6587'],
        'PolicyWords': ['Leave alone', 'Sell only', 'Buy only', 'Buy and sell'],
        'SmeltableWords': ['Sell them', 'Keep every one', 'Keep the ones you have not learned'],
        'BasisWords': ['Average of what you paid', 'Last price you paid', 'Cheapest market you know'],
    }
    return (set(lists) == set(shipped)
            and all(lists[k][:len(v)] == v for k, v in shipped.items())
            and 'keep(from.SelectedIndex);' in method_body(M, "private static void Follows"))

def a_settings_file_that_could_not_be_read_is_never_written_over():
    c = S['Config.cs']
    read = method_body(c, "private static void Read()")
    lines = method_body(c, "private static string[] Lines(string path)")
    write = method_body(c, "private static void Write(string path, string why)")
    return ("private static bool _unreadable;" in c
            and "try { return File.ReadAllLines(path); }" in lines
            and "_unreadable = true;" in lines
            and "return null;" in lines
            and "string[] lines = Lines(found);" in read
            and "if (lines == null) return;" in read
            and "File.ReadAllLines" not in read
            and read.find("string[] lines = Lines(found);") < read.find("SettingsFile.Read(lines, ignored)")
            and "if (_unreadable)" in write
            and write.find("if (_unreadable)") < write.find("File.WriteAllText")
            and 'Log.Repeatable("settings file unreadable", "left alone",' in write)


chk("1.81.8", "a settings file TradeLord could not read is left exactly as it is, so a setting changed afterwards never writes what it ships with over what you had",
    a_settings_file_that_could_not_be_read_is_never_written_over())


chk("1.23.1", "a dropdown's choices keep the order they shipped in, so a saved setting never comes back meaning something else",
    a_dropdown_only_ever_gains_choices_at_the_end())

EVER_SHIPPED = {
    "AlwaysBuyItems": "string", "AlwaysSellItems": "string", "AutoBuyOnEntry": "bool",
    "AutoSellOnEntry": "bool", "BanditFreePassage": "bool", "BanditGetawayCheat": "bool",
    "BestSellTownTolerance": "float",
    "BulkSimulation": "bool", "BuyCapPerItem": "int", "BuyHaulAnimals": "bool",
    "BuyPackAnimals": "bool",
    "BuyValueCapPerItem": "int", "CoinSound": "bool", "ConfidenceRanking": "bool",
    "ConservativeRouteProjection": "bool", "CostBasisMode": "int", "CraftingPolicy": "int",
    "TrustWhatAMarketPaid": "bool",
    "DetailedTradeSummary": "bool", "EconomySettlingDays": "int", "ExcludeHostileTowns": "bool",
    "FoodPolicy": "int", "GoldReserve": "int", "KeepEveryFoodKind": "bool", "KeepFoodDays": "int",
    "KeepFoodVariety": "int", "KeepPerFoodKind": "int", "KeepSmeltableWeapons": "bool",
    "KeepWageDays": "int", "Language": "int", "LedgerMenuEntry": "bool", "LivestockPolicy": "int",
    "HaulAnimalGoldFloor": "int", "HaulAnimalPriceTolerance": "float",
    "MarkBestSellTownOnMap": "bool", "MarketForecast": "bool", "MarkerMaxTravelDays": "float", "MaxHeldPerItem": "int",
    "MaxHeldShare": "float", "MaxLootTier": "int", "MaxSpendPerVisit": "int",
    "MaxTravelDays": "float", "MaxTravelDaysTown": "float", "MaxTravelDaysVillage": "float",
    "MaxVillageTravelDays": "float", "MinProfitMargin": "float",
    "MinTownStock": "int", "MinTownStockWorth": "int",
    "FollowTheLedgerFirst": "bool", "WhatToBuyFirst": "int",
    "NeverBuyGrain": "bool", "NeverBuyItems": "string",
    "NeverSellItems": "string", "ObservationShelfLifeDays": "int", "Omniscient": "bool",
    "PackAnimalFullCargoPremium": "float", "PanelKey": "string", "PreferBestSellTown": "bool",
    "ProfitColoring": "bool", "ProtectSpecial": "bool", "QuickSellMenu": "bool",
    "QuietAutomation": "bool", "ResaleSafetyFactor": "float", "RespectLocks": "bool",
    "ResupplyFoodDays": "int", "ScanRadius": "float", "SellSpareMounts": "bool",
    "ShowMapButton": "bool", "SimulationMode": "bool", "SuppressVanillaTradeLines": "bool",
    "TooltipHints": "bool", "TradeWithCaravans": "bool", "TradeWithTowns": "bool",
    "TradeWithVillages": "bool",
    "ForecastScore": "bool", "StagedTrading": "bool",
    "TradeXpMultiplier": "float", "UseFleetCapacity": "bool",
    "MaxCargoShare": "float", "PartyTradeXpShare": "float",
    "MarkPriceDirection": "bool", "PriceTrace": "bool",
    "MaxWorkshopsOwned": "int", "Ultralog": "bool", "ExtendedDebugLogging": "bool",
    "PickTheBuyerOnTheWholeStack": "bool",
}

def settings_now():
    return {m.group(2): m.group(1) for m in
            re.finditer(r'public\s+(bool|int|float|string)\s+(\w+)\s*=(?!>)', S['Options.cs'])}

def no_setting_a_player_ever_saved_is_left_stranded():
    now = settings_now()
    lift = S['Migrate.cs']
    stranded = [name for name, kind in EVER_SHIPPED.items()
                if now.get(name) != kind and '"' + name + '"' not in lift]
    unlisted = [name for name in now if name not in EVER_SHIPPED]
    return not stranded and not unlisted and len(EVER_SHIPPED) >= 86

def a_settings_file_says_which_shape_it_is_in():
    read = method_body(S['Config.cs'], "private static void Read")
    write = method_body(S['Config.cs'], "private static void Write")
    return ('public const int Shape = 17;' in S['Migrate.cs']
            and 'public const string ShapeKey = "SettingsVersion";' in S['Migrate.cs']
            and "written[line.Substring(0, mark).Trim()] = line.Substring(mark + 1).Trim();" in S['Migrate.cs']
            and ordered(read, "string[] lines = Lines(found);",
                        "var written = SettingsFile.Read(lines, ignored);",
                        "written.TryGetValue(Migration.ShapeKey, out string held)",
                        "written.Remove(Migration.ShapeKey);",
                        "bool lifted = Migration.Lift(shape, written, notes);",
                        "foreach (string note in notes) Log.Write",
                        "if (Taken(field, line.Value)) taken++;",
                        "if (lifted || shape != Migration.Shape)")
            and "int shape = 1;" in read
            and "new KeyValuePair<string, string>(Migration.ShapeKey," in write)

def the_file_and_the_screen_are_twins_and_the_newer_one_wins():
    read = method_body(S['Config.cs'], "private static void Read")
    write = method_body(S['Config.cs'], "private static void Write")
    hand = method_body(S['Config.cs'], "private static bool ChangedByHand")
    follow = method_body(S['Config.cs'], "internal static void Follow")
    return ('internal const string ChangedKey = "SettingsChanged";' in S['Config.cs']
            and 'internal const string WrittenByKey = "SettingsWrittenBy";' in S['Config.cs']
            and "new KeyValuePair<string, string>(ChangedKey," in write
            and "DateTime.UtcNow.ToString(\"o\"" in write
            and "new KeyValuePair<string, string>(WrittenByKey," in write
            and 'private const string ByScreen = "the settings screen";' in S['Config.cs']
            and 'private const string ByFile = "this file";' in S['Config.cs']
            and "McmLoader.SettingsInHand ? ByScreen : ByFile" in write
            and "Twins.ChangedByHand(File.GetLastWriteTimeUtc(path), stamped)" in hand
            and "if (stamped == default(DateTime)) return true;" in hand
            and "lastWritten > stamped + HandTolerance;" in S['Migrate.cs']
            and "A_file_touched_after_the_tolerance_is_an_edit_by_hand" in TWINSTESTS
            and ordered(read, "bool screen = McmLoader.SettingsInHand;",
                        "written.Remove(ChangedKey);",
                        "if (Twins.ScreenWins(screen, screenWroteIt, handEdited))",
                        "Write(found, \"made to match the settings screen\");",
                        "McmLoader.Reseat?.Invoke();")
            and "Options.Changed = Noted;" in follow
            and (lambda b: "if (_applying) return;" in b and "_dirty = true;" in b
                       and "_stillMoving = DateTime.UtcNow;" in b)
                (method_body(S['Config.cs'], "private static void Noted"))
            and "finally { _applying = false; }" in read
            and "public static Action Changed;" in S['Options.cs']
            and "Generation++; Changed?.Invoke();" in S['Options.cs']
            and "internal static Action Reseat;" in S['Support.cs']
            and "McmLoader.Reseat = Settings.Reseat;" in M
            and "BaseSettingsProvider.Instance?.SaveSettings(held);" in M
            and "held.Bound(Options.Current);" in M and "held.FollowLanguage();" in M
            and 'Guard.Run("Tick.Settings", Config.Flush);' in
                method_body(S['SubModule.cs'], "protected override void OnApplicationTick"))

chk("1.25.0", "the settings file and the settings screen are twins, the one saved last wins, and the other is written to match",
    the_file_and_the_screen_are_twins_and_the_newer_one_wins())

def the_screen_only_wins_once_it_has_actually_handed_its_settings_over():
    boot = method_body(M, "public static bool Init")
    load = method_body(S['Support.cs'], "internal static void TryLoad")
    read = method_body(S['Config.cs'], "private static void Read")
    write = method_body(S['Config.cs'], "private static void Write")
    return ("internal static bool SettingsInHand { get; private set; }" in S['Support.cs']
            and ordered(boot, "if (Settings.Instance == null) return false;",
                        "Settings.Reseat();", "return true;")
            and ordered(load, "object answered = init.Invoke(null, null);",
                        "SettingsInHand = answered is bool taken && taken;",
                        "Log.Write(SettingsInHand")
            and "bool screen = McmLoader.SettingsInHand;" in read
            and "McmLoader.SettingsReachable" not in S['Config.cs']
            and "McmLoader.SettingsInHand ? ByScreen : ByFile" in write
            and S['Config.cs'].count("McmLoader.SettingsInHand") == 2)

chk("1.27.3", "the settings file is never written from a settings screen that has not handed its settings over",
    the_screen_only_wins_once_it_has_actually_handed_its_settings_over())

def a_file_no_settings_screen_wrote_is_never_written_over_by_one():
    read = method_body(S['Config.cs'], "private static void Read")
    write = method_body(S['Config.cs'], "private static void Write")
    return ('private const string ByScreen = "the settings screen";' in S['Config.cs']
            and 'private const string ByFile = "this file";' in S['Config.cs']
            and "McmLoader.SettingsInHand ? ByScreen : ByFile" in write
            and ordered(read,
                        "bool screenWroteIt = written.TryGetValue(WrittenByKey, out string wroteIt) &&",
                        "string.Equals(wroteIt, ByScreen, StringComparison.Ordinal);",
                        "bool handEdited = screen && screenWroteIt && ChangedByHand(found, stamped);",
                        "if (Twins.ScreenWins(screen, screenWroteIt, handEdited))",
                        "Log.Write(screenWroteIt")
            and "screenInHand && screenWroteIt && !changedByHand;" in S['Migrate.cs']
            and "Nothing_the_screen_never_wrote_is_ever_overwritten_by_it" in TWINSTESTS
            and read.count("screenWroteIt") == 4)

chk("1.30.3", "a settings file no settings screen ever wrote is read rather than written over, however old its stamp looks",
    a_file_no_settings_screen_wrote_is_never_written_over_by_one())

def the_lift_needs_nothing_from_the_game_and_is_covered_by_tests():
    return ('TaleWorlds' not in S['Migrate.cs']
            and 'Log.' not in S['Migrate.cs']
            and 'Migrate.cs' in TESTPROJ
            and MIGRATIONTESTS.count('[Fact]') + MIGRATIONTESTS.count('[Theory]') >= 8
            and 'Migration.Lift' in MIGRATIONTESTS
            and 'KeepFoodVariety' in MIGRATIONTESTS
            and 'KeepSmeltableWeapons' in MIGRATIONTESTS)

chk("1.24.0", "every setting a player could have saved since the last public release is still read, or is named in the lift",
    no_setting_a_player_ever_saved_is_left_stranded())
chk("1.24.0", "the settings file carries its own shape, is lifted before anything is applied, and is written back once lifted",
    a_settings_file_says_which_shape_it_is_in())
chk("1.24.0", "the lift needs nothing from the game, says nothing itself, and is covered by tests the build runs",
    the_lift_needs_nothing_from_the_game_and_is_covered_by_tests())

def the_shape_a_settings_file_declares_gates_every_step_of_the_lift():
    lift = method_body(S['Migrate.cs'], "public static bool Lift")
    rename = method_body(S['Migrate.cs'], "private static bool Rename")
    shape = re.search(r'public const int Shape = (\d+);', S['Migrate.cs'])
    block = re.search(r'Renamed\s*=\s*\{(.*?)\};', S['Migrate.cs'], re.S)
    renamed = block.group(1) if block else ''
    return (shape is not None
            and ordered(lift, "changed |= Rename(from, written, notes);",
                        "if (from < 5)",
                        "changed |= FoodVarietyBecameASwitchAndAnAmount(written, notes);",
                        "changed |= SmeltableWeaponsBecameAChoiceOfThree(written, notes);",
                        "changed |= PayingOverTheOddsForAHaulAnimalIsGone(written, notes);",
                        "if (from < 6) changed |= TheAutoMarkerCeilingIsGone(written, notes);",
                        "if (from < 7) changed |= TheScanRadiusIsGone(written, notes);")
            and lift.count("changed |=") == 9
            and "if (from < 15) changed |= WhatToBuyFirstIsOneRuleNow(written, notes);" in lift
            and (("if (from < " + shape.group(1) + ")") in lift
                 or ("(" + shape.group(1) + ', "') in renamed
                 or ("public const int CracksAt = " + shape.group(1) + ";"
                     in method_body(S['Migrate.cs'], "public static class Whip")))
            and ordered(rename, "foreach (var (arrivedAt, was, now) in Renamed)",
                        "if (from >= arrivedAt) continue;",
                        "if (!written.TryGetValue(was, out string held)) continue;")
            and '(5, "BuyPackAnimals", "BuyHaulAnimals"),' in S['Migrate.cs']
            and '(7, "MaxTravelDays", "MaxTravelDaysTown"),' in S['Migrate.cs']
            and '(7, "MaxVillageTravelDays", "MaxTravelDaysVillage"),' in S['Migrate.cs']
            and '(17, "BanditGetawayCheat", "BanditFreePassage"),' in S['Migrate.cs']
            and "TheFreePassageSettingKeepsItsValueUnderItsNewName" in MIGRATIONTESTS
            and "AStepOnlyRunsOnAFileOlderThanTheShapeItArrivedIn" in MIGRATIONTESTS
            and "ARenameOnlyRunsOnAFileOlderThanTheShapeItArrivedIn" in MIGRATIONTESTS
            and "AFileAlreadyInTheCurrentShapeIsNeverLiftedAtAll" in MIGRATIONTESTS
            and "ASettingWrittenByHandIntoACurrentFileIsLeftForTheFileToReport" in MIGRATIONTESTS)

chk("1.46.1", "the shape a settings file declares decides which steps of the lift run on it, and every step is gated at the shape it arrived in",
    the_shape_a_settings_file_declares_gates_every_step_of_the_lift())

def one_button_puts_every_setting_back_and_sits_at_the_top():
    reset = method_body(M, "internal static void Reset")
    button = re.search(r'\[SettingPropertyButton\("\{=(TL\d+)\}[^"]*", Order = (\d+)[^\]]*?'
                       r'Content = "\{=(TL\d+)\}[^"]*",\s*\n\s*HintText = "\{=(TL\d+)\}[^"]*"\)\]\s*\n'
                       r'\s*\[SettingPropertyGroup\("\{=TL100\}Language", GroupOrder = (\d+)\)\]\s*\n'
                       r'\s*public Action (\w+) \{ get; set; \} = Reset;', M, re.S)
    en = spoken(ENGLISH)
    return (button is not None
            and button.group(2) == "0" and button.group(5) == "0"
            and all(en.get(tag) for tag in (button.group(1), button.group(3), button.group(4)))
            and 'foreach (FieldInfo field in typeof(Options).GetFields(BindingFlags.Public | BindingFlags.Instance))'
                in reset
            and "field.SetValue(Options.Current, now);" in reset
            and "var stock = new Options();" in reset
            and "if (!Equals(field.GetValue(Options.Current), now)) moved.Add(field.Name);" in reset
            and ordered(reset, "var stock = new Options();", "field.SetValue(Options.Current",
                        "Reseat();", "Options.Bump();", "shown.OnPropertyChanged(moved[i]);")
            and re.search(r'SettingPropertyDropdown\("\{=TL250\}Language", Order = 1,', M) is not None)

def every_change_away_from_the_shipped_value_reaches_the_log():
    follow = method_body(S['Config.cs'], "internal static void Follow")
    away = method_body(S['Config.cs'], "private static void SayWhatIsAwayFromStock")
    said = method_body(S['Config.cs'], "private static bool SayWhatChanged")
    flush = method_body(S['Config.cs'], "internal static void Flush")
    return (ordered(follow, "Guard.Run(\"Config\", Read);", "Guard.Run(\"Config.Away\", SayWhatIsAwayFromStock);",
                    "_lastSeen = Snapshot();", "Options.Changed = Noted;")
            and "var stock = new Options();" in away and "var stock = new Options();" in said
            and "TradeLord ships with" in away and "TradeLord ships with" in said
            and "every setting is at the value TradeLord ships with" in away
            and "away from what TradeLord ships with" in away
            and "is back at what it ships with" in said
            and "_lastSeen = now;" in said
            and "bool moved = false;" in said and "moved = true;" in said and "return moved;" in said
            and ordered(method_body(S['Config.cs'], "internal static void Settle"),
                        "_dirty = false;",
                        'bool moved = Guard.Read("Config.Changed", _path, _ => SayWhatChanged(), true);',
                        "if (!moved) return;",
                        'Guard.Run("Config.Flush"')
            and ordered(flush, "if (!_dirty) return;",
                        "if (DateTime.UtcNow - _stillMoving < Settling) return;", "Settle();")
            and "private static string Shown(FieldInfo field) => Shown(field, Options.Current);" in S['Config.cs'])

chk("1.26.0", "one button at the top of the screen puts every setting back to what the module ships with, redrawing only what moved",
    one_button_puts_every_setting_back_and_sits_at_the_top())
chk("1.26.0", "what a player sets away from the shipped value is named in the log, at startup and whenever it changes",
    every_change_away_from_the_shipped_value_reaches_the_log())
chk("1.35.2", "dragging a slider is written down once it comes to rest, not on every frame it passes through",
    "private static readonly TimeSpan Settling = TimeSpan.FromMilliseconds(400);" in S['Config.cs'] and
    'Guard.Run("GameEnd.Settings", Config.Settle);' in
        method_body(S['SubModule.cs'], "public override void OnGameEnd") and
    S['Config.cs'].count("_dirty = false;") == 1)

def unwritable_settings_the_screen_would_refuse_to_build():
    shown = list(re.finditer(r'\[SettingProperty(Bool|Integer|FloatingInteger|Text|Dropdown|Button)\(', M))
    refused = []
    for i, at in enumerate(shown):
        block = M[at.start():shown[i + 1].start() if i + 1 < len(shown) else len(M)]
        declared = re.search(r'\n\s*public\s+[\w<>\[\], .?]+\s+(\w+)\s*(=>|\{)', block)
        if declared is None:
            return [at.group(1) + ' (no property follows it)']
        opener = declared.group(2)
        body = block[declared.end() - len(opener):]
        settable = opener == '{' and re.search(r'(^|[;{\s])set\s*[;{=]', body) is not None
        if not settable and at.group(1) != 'Dropdown':
            refused.append(declared.group(1))
    return refused

chk("1.27.2", "every setting the screen shows can be written back, so the screen can be built at all",
    unwritable_settings_the_screen_would_refuse_to_build() == [] and len(
        re.findall(r'\[SettingProperty(?:Bool|Integer|FloatingInteger|Text|Dropdown|Button)\(', M)) > 50)

def the_file_holds_a_number_to_the_same_limits_the_screen_does():
    def plain(text):
        text = text.rstrip('f')
        return str(int(float(text))) if float(text) == int(float(text)) else str(float(text))
    ranged = {m.group(5): (plain(m.group(2)), plain(m.group(3))) for m in re.finditer(
        r'\[SettingProperty(Integer|FloatingInteger)\("\{=TL\d+\}[^"]*",\s*([-\d.f]+),\s*([-\d.f]+),.*?'
        r'public\s+(int|float)\s+(\w+)\s*\{', M, re.S)}
    words = {m.group(1): len(re.findall(r'"((?:[^"\\]|\\.)*)"', m.group(2))) for m in
             re.finditer(r'string\[\]\s+(\w+Words)\s*=\s*\{(.*?)\n\s*\};', M, re.S)}
    bound = method_body(M, "private void Bound")
    picked = {}
    for m in re.finditer(r'(?:Choice\((\w+Words), to\.(\w+)\)|new Dropdown<string>\((\w+Words), to\.(\w+)\))', bound):
        name, field = (m.group(1), m.group(2)) if m.group(1) else (m.group(3), m.group(4))
        picked[field] = ("0", str(words[name] - 1))
    table = {m.group(1): (plain(m.group(2)), plain(m.group(3))) for m in re.finditer(
        r'\{ "(\w+)", new double\[\] \{ ([-\d.]+), ([-\d.]+) \} \}', S['Migrate.cs'])}
    wanted = dict(ranged)
    wanted.update(picked)
    taken = method_body(S['Config.cs'], "private static bool Taken")
    within = method_body(S['Config.cs'], "private static double Within")
    numeric = set(re.findall(r'^\s*public\s+(?:int|float)\s+(\w+)\s*=', S['Options.cs'], re.M))
    return (len(ranged) >= 18 and len(picked) == 6 and table == wanted
            and numeric and not (numeric - set(table))
            and "(int)Within(field, int.Parse(" in taken
            and "(float)Within(field, float.Parse(" in taken
            and "double kept = Limits.Kept(field.Name, asked);" in within
            and "if (double.IsNaN(asked)) return edge[0];" in
                method_body(S['Migrate.cs'], "public static double Kept")
            and "new Dictionary<string, double[]>(StringComparer.OrdinalIgnoreCase)" in S['Migrate.cs']
            and "ASettingIsHeldInsideItsRangeHoweverItWasCapitalisedInTheFile" in MIGRATIONTESTS
            and 'Limits.Range(field.Name)' in within)

chk("1.26.1", "a number in the settings file is held to the same limits the settings screen holds it to",
    the_file_holds_a_number_to_the_same_limits_the_screen_does())

chk("1.14.1", "the release notes are read out of the changelog section for the version being published",
    'python3 tools/nexus_changelog.py --notes "${VERSION#v}" > release-notes.md' in WORKFLOW and
    'git log -1 --format=%b "$GITHUB_SHA" > release-notes.md' not in WORKFLOW and
    the_notes_are_the_changelog_section_for_the_version())


def a_dry_run_prices_the_whole_visit_and_not_each_pass_on_its_own():
    t = S['Trading.cs']
    sell = sell_pass()
    larder = method_body(t, "public static void ExecuteResupply")
    relief = method_body(t, "public static void ExecuteHerdRelief")
    haul = method_body(t, "public static void ExecuteHaulage")
    buy = buy_pass()
    return ("Visit.Forget();" in method_body(t, "private static void ResetVisit")
            and ("TradeMath.Budget(Hero.MainHero.Gold + books.Purse(sim), GoldHeldBack(),\n"
                 "                             Options.Current.MaxSpendPerVisit, "
                 "books.PaidOut(sim));") in t
            and "private static bool Simulating => Options.Current.SimulationMode;" in t
            and all("pass.WouldReachYourReserve(price)" in b for b in (larder, haul))
            and "pass.Spendable()" in buy
            and "price >= Spendable()" in between(t, "internal bool WouldReachYourReserve", ";")
            and t.count("simTill = pass.Till;") == 1
            and "int simTill = market.Till();" in S['Passes.cs']
            and "TillNow - Books.TillDrawn(Sim)" in between(t, "internal int Till =>", ";")
            and t.count("float simWeight = pass.Books.Weight(pass.Sim);") == 1
            and "float simWeight = books.Weight(sim);" in S['Passes.cs']
            and "TradePolicy.FoodHeld(pass.Party.ItemRoster) - pass.Books.FoodHeld(pass.Sim);" in larder
            and "shed -= pass.Books.Shed(pass.Sim);" in relief
            and "mountsLeft -= pass.Books.MountsShed(pass.Sim);" in relief
            and "Math.Max(0, Drove.HaulAnimalsCargoCanSpare(pass.Party) - pass.Books.HaulsShed(pass.Sim));" in relief
            and "herdRoom -= pass.Books.HerdTaken(pass.Sim);" in haul
            and "herdRoom = Math.Max(0, market.HerdRoom() - books.HerdTaken(sim));" in buy
            and "int remaining = market.YoursToSell(at) - keep;" in sell
            and "int remaining = pass.YoursToSell(el);" in relief
            and t.count("int remaining = pass.TheirsToSell(el);") == 2
            and "int remaining = market.TheirsToSell(picked.At);" in S['Passes.cs']
            and "int held = market.Carried(at) + books.Held(sim, good.Id);" in buy
            and t.count("pass.Books.NoteSale(") == 1
            and S['Passes.cs'].count("books.NoteSale(") == 1
            and t.count("pass.Books.NotePurchase(") == 2
            and S['Passes.cs'].count("books.NotePurchase(") == 2
            and "pass.Books.NoteShed(rank == RankHaulAnimal, rank != RankLivestock);" in relief
            and "books.NoteShed(herdRank == TradeRules.RankHaulAnimal,\n"
                "                                           herdRank != TradeRules.RankLivestock);" in sell
            and t.count("pass.Books.NoteHerdTaken();") == 1
            and S['Passes.cs'].count("books.NoteHerdTaken();") == 2
            and t.count("pass.Books.Sold(pass.Sim,") == 1
            and S['Passes.cs'].count("books.Sold(sim, good.Id)") == 1
            and "pass.Books.Sold(pass.Sim, it.StringId)" in method_body(t,
                    "private static List<(ItemRosterElement el, Good good, int price, int ceiling)> CheapestFirst")
            and "CheapestFirst(" in larder and "CheapestFirst(" in haul
            and S['Passes.cs'].count("books.Bought(sim, market.IdAt(at))") == 1
            and t.count("pass.Books.Purchases(pass.Sim,") == 2
            and S['Passes.cs'].count("books.Purchases(sim, good.Id)") == 2)

def a_dry_run_keeps_its_own_books_and_writes_none_of_the_live_ones():
    ledger = S['Books.cs']
    forget = method_body(ledger, "internal void Forget")
    dry = method_body(ledger, "internal void ForgetTheDryRun")
    fields = set(re.findall(r'^\s*private (?:readonly )?.*?(_\w+)(?: =|;)', ledger, re.M))
    live = {"_bought", "_sold", "_paid"}
    cleared = lambda body: set(re.findall(r'(_\w+)(?:\.Clear\(\)| = 0f?);', body))
    return ("static" not in ledger
            and all(reader in ledger for reader in (
                "internal int PaidOut(bool sim) => _paid + (sim ? _spent : 0);",
                "internal int Purse(bool sim) => sim ? _gained - _spent : 0;",
                "internal int TillDrawn(bool sim) => sim ? _drawn : 0;",
                "private bool OnPaper(bool sim) => sim && !LaidOut;",
                "internal float Weight(bool sim) => OnPaper(sim) ? _weight : 0f;",
                "internal int FoodHeld(bool sim) => OnPaper(sim) ? _food : 0;",
                "internal int Shed(bool sim) => OnPaper(sim) ? _shed : 0;",
                "internal int MountsShed(bool sim) => OnPaper(sim) ? _mounts : 0;",
                "internal int HaulsShed(bool sim) => OnPaper(sim) ? _hauls : 0;",
                "internal int HerdTaken(bool sim) => OnPaper(sim) ? _herd : 0;"))
            and all(dry in ledger for dry in ("_drySold", "_dryBought"))
            and len(fields) == 16
            and "ForgetTheDryRun();" in forget
            and cleared(forget) == live
            and cleared(dry) == fields - live
            and 'Books.cs' in TESTPROJ
            and BOOKTESTS.count("[Fact]") >= 12)

def a_meeting_on_the_road_is_priced_as_one_meeting():
    t = S['Trading.cs']
    body = method_body(t, "public static void ExecuteRoadTrade")
    return ("Books books = BooksForTheMeeting(met);" in body
            and body.count("books, party)") == 3
            and "new Books()" not in body
            and method_body(t, "private static Books BooksForTheMeeting").count("new Books()") == 1
            and t.count("new Books()") == 2
            and "internal static Pass Meet(MobileParty met, IMarketData road, Books books, MobileParty party) =>"
                in t
            and "new Pass(null, met, road, books, party, quiet: true);" in t
            and "new Pass(site, null, null, Visit, party, quiet)" in t)


def meeting_the_same_party_again_keeps_the_books_it_already_wrote():
    t = S['Trading.cs']
    books = method_body(t, "private static Books BooksForTheMeeting")
    passes = S['Passes.cs']
    return (ordered(books, "if (_meetingBooks != null && _meetingBooksFor == met)",
                    "_meetingBooks.ForgetTheDryRun();", "return _meetingBooks;",
                    "_meetingBooks = new Books();", "_meetingBooksFor = met;")
            and "_meetingBooks = null;" in method_body(t, "internal static void ForgetTheMeeting")
            and "ForgetTheMeeting();" in method_body(t, "internal static void ForgetVisit")
            and "TradeActionBehavior.ForgetTheMeeting();" in
                method_body(S['Encounters.cs'], "internal static void ForgetEncounter")
            and "books.Sold(sim, good.Id)" in method_body(passes, "internal static List<Pick> WhatToBuy")
            and "books.Bought(sim, market.IdAt(at))" in method_body(passes, "internal static Traded SellThem")
            and "internal bool Sold(bool sim, string id) =>" in S['Books.cs']
            and "internal bool Bought(bool sim, string id) =>" in S['Books.cs']
            and "_sold.Clear();" in method_body(S['Books.cs'], "internal void Forget")
            and "_sold" not in method_body(S['Books.cs'], "internal void ForgetTheDryRun")
            and "_bought" not in method_body(S['Books.cs'], "internal void ForgetTheDryRun")
            and "ForgettingOnlyTheDryRunKeepsWhatTheSittingReallyMoved" in BOOKTESTS
            and "ForgettingTheWholeVisitClearsWhatTheSittingReallyMovedAsWell" in BOOKTESTS)

chk("1.37.6", "a dry run carries the merchant's gold, the purse, the cargo room and every cap from one pass of a visit to the next",
    a_dry_run_prices_the_whole_visit_and_not_each_pass_on_its_own())
chk("1.37.6", "a dry run keeps its running totals apart from the real ones, empties them with the visit, and is covered by tests the build runs",
    a_dry_run_keeps_its_own_books_and_writes_none_of_the_live_ones())
chk("1.37.6", "a dry run of a meeting on the road spends what the same meeting just earned and never buys back what it just sold",
    a_meeting_on_the_road_is_priced_as_one_meeting())
chk("1.74.0", "meeting the same party again keeps the books already written with them, so nothing sold to them is bought straight back",
    meeting_the_same_party_again_keeps_the_books_it_already_wrote())


def every_pass_hands_one_place_the_trade_and_the_visits_books():
    t = S['Trading.cs']
    swap = method_body(t, "private static bool SwapOneUnit")
    books = method_body(S['Books.cs'], "internal void NoteBought")
    return ("int before = Hero.MainHero.Gold;" in swap
            and t.count("int before = Hero.MainHero.Gold;") == 1
            and t.count("transaction direction changed on this game version") == 1
            and t.count("LedgerBehavior.Instance?.RecordPurchase(item.StringId, 1,") == 3
            and t.count("pass.Books.NoteBought(item.StringId,") == 2
            and S['Passes.cs'].count("books.NoteBought(good.Id, cost);") == 2
            and "_paid += price;" in books
            and S['Books.cs'].count("_paid +=") == 1
            and all(named in t for named in (
                '"quick-sell", "selling", "Selling"', '"restocking", "Restocking"',
                '"sale on the road", "selling on the road", "Road trading"',
                '"purchase on the road", "buying on the road", "Road buying"',
                '"selling an animal to relieve the herd", "Herd relief"',
                '"buying a haul animal", "Haul animal buying"', '"quick-buy", "buying", "Buying"')))

chk("1.37.7", "every pass hands one place the swap of goods for gold, the guard on which way the gold went, and what the visit has spent",
    every_pass_hands_one_place_the_trade_and_the_visits_books())


def a_meeting_on_the_road_counts_what_it_spends_against_the_cap():
    t = S['Trading.cs']
    buy = buy_pass()
    return ("Options.Current.MaxSpendPerVisit, books.PaidOut(sim));" in t
            and "internal int PaidOut(bool sim) => _paid + (sim ? _spent : 0);" in S['Books.cs']
            and buy.count("pass.Spendable()") == 2
            and buy.count("market.Spendable()") == 3
            and ordered(method_body(S['Passes.cs'], "internal static Traded BuyThem"),
                        "if (market.Stopped || market.Spendable() <= 0) break;",
                        "TradeRules.WhatStopsBuying(good, price, market.Spendable(),",
                        "books.NoteBought(good.Id, cost);")
            and "The_spending_cap_counts_what_this_visit_has_already_spent" in MATHTESTS)

def arming_the_panel_costs_no_route_scan():
    src = S['Panel.cs']
    setup = method_body(src, "private static void Setup(MapScreen map)")
    tick = method_body(src, "private static void TickCore")
    return ("private static int _spokenFor = -1;" in src
            and ordered(setup, "_vm = new LedgerPanelVM(", "_spokenFor = Options.Current.Language;")
            and ordered(tick, "if (_spokenFor != Options.Current.Language)",
                        "_spokenFor = Options.Current.Language;",
                        'Guard.Run("Panel.Respeak", _vm.Respeak);')
            and "BestRoutes" not in setup
            and "BestRoutes" in method_body(src, "private void Refresh()"))

chk("1.37.8", "a meeting on the road stops at the spending cap whether it is a dry run or a real one",
    a_meeting_on_the_road_counts_what_it_spends_against_the_cap())
chk("1.37.8", "the panel takes the language it is armed with, so arming it works out no routes",
    arming_the_panel_costs_no_route_scan())


GUARDS = {'NeverList', 'Locked', 'Protected', 'QuestGoods', 'FoodReserve'}

def every_guard_that_holds_a_good_back_says_which_one_it_is():
    phrase = method_body(S['Reasons.cs'], "internal static TextObject Phrase")
    said = []
    for guard in sorted(GUARDS):
        spoken = re.search(r'case Block\.' + guard +
                           r':\s*\n\s*return Tongue\.Text\("\{=(TL\d+)\}([^"]+)"\);', phrase)
        if spoken is None:
            return False
        said.append(spoken.group(2))
    return (len(set(said)) == len(GUARDS)
            and 'TL40' not in strings_declared()
            and 'your protections held it back' not in ALL)

def a_stalled_pass_names_the_first_guard_it_met():
    note = method_body(S['Passes.cs'], "internal void Note")
    dominant = method_body(S['Passes.cs'], "internal Block Dominant")
    guarded = between(S['Passes.cs'], "private static bool Guarded(Block reason) =>",
                      "internal Block Dominant")
    return ("private Block _firstGuard = Block.None;" in S['Passes.cs']
            and ordered(note, "if (reason == Block.None) return;",
                        "if (_firstGuard == Block.None && Guarded(reason)) _firstGuard = reason;",
                        "_counts[reason] = seen + 1;")
            and "return Guarded(top) ? _firstGuard : top;" in dominant
            and set(re.findall(r'Block\.(\w+)', guarded)) == GUARDS
            and all(one in STALLTESTS for one in
                    ("A_pass_that_was_stopped_by_nothing_keeps_no_reason_at_all",
                     "The_first_protection_met_is_named_rather_than_the_one_met_most",
                     "A_reason_that_is_not_a_protection_is_named_on_its_own_count")))

def a_full_cargo_is_told_the_three_ways_out_of_it():
    body = method_body(S['Trading.cs'], "private static void WarnNoRoomToCarry")
    warning = re.search(r'\{=TL82\}([^"]+)"', body)
    return (warning is not None
            and all(way in warning.group(1) for way in
                    ('Recruit more men', 'buy more horses', 'sell goods manually'))
            and 'free up carry weight' not in ALL)

chk("1.37.10", "each protection that holds a good back has a line of its own to say so",
    every_guard_that_holds_a_good_back_says_which_one_it_is())
chk("1.37.10", "a pass that moves nothing names the first protection it met, not the one it met most",
    a_stalled_pass_names_the_first_guard_it_met())
chk("1.37.10", "the full cargo warning names every way out of a full cargo",
    a_full_cargo_is_told_the_three_ways_out_of_it())


def the_free_passage_never_ends_an_encounter_a_band_is_still_talking_through():
    go = method_body(S['Encounters.cs'], "private static void LetPlayerGo")
    return ("PlayerEncounter.Finish" not in ALL
            and "InformationManager.ShowInquiry" not in go
            and ordered(go, "if (PlayerEncounter.Current != null)",
                        "PlayerEncounter.ProtectPlayerSide(GetawayHours);",
                        "PlayerEncounter.LeaveEncounter = true;")
            and "_offeredPassageIn" not in ALL
            and "TL09" in strings_used())

def the_free_passage_is_a_line_in_the_bands_own_talk():
    hang = method_body(S['Encounters.cs'], "internal static void HangWhereTheBandAnswers")
    parley = method_body(S['Encounters.cs'], "internal static class Parley")
    return ('internal const string OwnState = "tradelord_bandit_pass_asked";' in parley
            and 'private const string BandAsks = "bandit_start_defender_2";' in parley
            and 'private const string BandOpens = "bandit_start_defender";' in parley
            and 'typeof(ConversationManager).GetField(' in hang
            and 'typeof(ConversationSentence).GetMethod(' in hang
            and "talk.DisableSentenceSort();\n            try { hang.Invoke(_asked, new object[] { token }); }\n            finally { talk.EnableSentenceSort(); }" in hang
            and hang.count("Unhung(") == 4
            and "a band is met exactly as the game means it to be" in parley)

def the_option_waits_for_the_game_to_finish_writing_its_lines():
    hang = method_body(S['Encounters.cs'], "internal static void HangWhereTheBandAnswers")
    parley = method_body(S['Encounters.cs'], "internal static class Parley")
    unhung = method_body(S['Encounters.cs'], "private static void Unhung")
    return ('Guard.Run("Tick.Parley", Parley.HangWhereTheBandAnswers);' in
                method_body(S['SubModule.cs'], "protected override void OnApplicationTick")
            and "Parley.HangWhereTheBandAnswers(" not in
                method_body(S['Encounters.cs'], "private static void AddBanditLines")
            and ordered(hang, "if (_hung || _asked == null || _tries >= Attempts) return;",
                        "if (Campaign.Current == null) return;",
                        "if (_ticks++ % TicksApart != 0) return;",
                        "_tries++;")
            and "_hung = true;" in hang
            and "int token = asks != null ? asks.InputToken : opens != null ? opens.OutputToken : -1;" in hang
            and "if (_tries < Attempts) return;" in unhung
            and "private const int Attempts = 20;" in parley
            and 'named.Add(id);' in hang)

chk("1.38.0", "asking a band for free passage never ends an encounter the band is still talking through",
    the_free_passage_never_ends_an_encounter_a_band_is_still_talking_through())
chk("1.38.0", "the free passage is asked for as a line in the band's own talk, hung where its own answers hang",
    the_free_passage_is_a_line_in_the_bands_own_talk())
chk("1.38.3", "the option waits for the game to finish writing its own lines, and says what it found if it cannot",
    the_option_waits_for_the_game_to_finish_writing_its_lines())


def every_handler_the_game_calls_guards_its_own_work():
    settled = ("private void OnConversationEnded(IEnumerable<CharacterObject> spoke)"
               " => Meetings.ConversationEnded();")
    held = 0
    for f in ('Trading.cs', 'Ledger.cs'):
        for name in re.findall(r'AddNonSerializedListener\(this, (\w+)\)', S[f]):
            if name == 'OnConversationEnded':
                continue
            body = method_body(S[f], "private void " + name)
            if not body or "Guard.Run" not in body:
                return False
            held += 1
    return held == 11 and settled in S['Trading.cs']

def a_save_is_never_failed_by_the_mods_own_bookkeeping():
    trade = method_body(S['Trading.cs'], "public override void SyncData")
    ledger = method_body(S['Ledger.cs'], "public override void SyncData")
    return ('Guard.Run("Visit.PinsForSave", () => _pinnedTowns = LedgerPanel.PinnedIds());' in trade
            and trade.count("LedgerPanel.PinnedIds()") == 1
            and 'Guard.Run("Ledger.WriteForSave", () =>' in ledger
            and 'Guard.Run("Ledger.Reindex", Reindex);' in ledger
            and ordered(ledger, "LedgerCodec.WriteLedger(Listed(_ledger));",
                        "LedgerCodec.WritePurchases(_purchases);",
                        "LedgerCodec.WritePromises(new List<PromiseRecord>(_promises.Values));",
                        'dataStore.SyncData("TradeLord_LedgerText"')
            and trade.count("dataStore.SyncData(") == 2
            and ledger.count("dataStore.SyncData(") == 12)

def every_choice_the_screen_offers_sits_inside_the_limit_the_file_keeps():
    arrays = dict(re.findall(r'private static readonly string\[\] (\w+) =\s*\{(.*?)\};', M, re.S))
    counted = dict((name, body.count('"') // 2) for name, body in arrays.items())
    bounds = dict((n, (float(low), float(high))) for n, low, high in
                  re.findall(r'\{ "(\w+)", new double\[\] \{ ([\d.]+), ([\d.]+) \} \}', S['Migrate.cs']))
    shown = {'Language': 'LanguageWords', 'FoodPolicy': 'PolicyWords',
             'CraftingPolicy': 'PolicyWords', 'LivestockPolicy': 'PolicyWords',
             'CostBasisMode': 'BasisWords', 'KeepSmeltableWeapons': 'SmeltableWords'}
    if len(bounds) < 20:
        return False
    for setting, array in shown.items():
        if setting not in bounds or counted.get(array, 0) < 3:
            return False
        if bounds[setting] != (0.0, float(counted[array] - 1)):
            return False
    return True

chk("1.38.2", "every handler the game calls into holds its own work behind the guard",
    every_handler_the_game_calls_guards_its_own_work())
chk("1.38.2", "saving a campaign is never failed by TradeLord's own bookkeeping",
    a_save_is_never_failed_by_the_mods_own_bookkeeping())
chk("1.38.2", "every setting shown as a list of choices is held to the number of choices it ships",
    every_choice_the_screen_offers_sits_inside_the_limit_the_file_keeps())


def a_quest_animal_held_back_is_named_as_the_quest_not_the_food_reserve():
    sell = method_body(S['Policy.cs'], "internal static bool MaySell(in Good good, ItemRosterElement el")
    kept = method_body(S['Policy.cs'], "internal static Dictionary<ItemObject, int> KeptBack(ItemRoster roster,")
    quick = sell_pass()
    return ("IDictionary<ItemObject, int> awaited," in sell
            and "facts.AwaitedHeld = HeldBack(awaited, item);" in sell
            and "if (amount <= said.KeepCount) { said.Why = Block.QuestGoods; return said; }" in sell_rule()
            and "if (amount <= said.KeepCount) { said.Why = Block.FoodReserve; return said; }" in sell_rule()
            and "out Dictionary<ItemObject, int> awaited" in kept
            and "TradePolicy.KeptBack(roster, pass.Books, pass.Sim, out _awaited);" in quick
            and "TradePolicy.MaySell(good, _plan[at], _pass.Locked, _keepBack, _awaited," in quick)

def getting_back_up_to_speed_credits_what_it_makes():
    relief = method_body(S['Trading.cs'], "public static void ExecuteHerdRelief")
    return ("Basis basis = Basis.For(TradePolicy.CostBasis(item)," in relief
            and "int worth = basis.Unit(out bool askTheMarket);" in relief
            and "int credited = TradePolicy.Credit(price, worth, basis.UnpaidWorth);" in relief
            and "profit += credited;" in relief
            and "if (el.EquipmentElement.ItemModifier == null) earned += credited;" in relief
            and "pass.Moved(profit, gained, selling: true);" in relief
            and "if (profit.HasValue) LedgerBehavior.Instance?.AddProfit(profit.Value);" in
                method_body(S['Trading.cs'], "internal void Moved")
            and "if (!pass.Sim && earned > 0) AwardTradeXpForOurOwnTrade(earned, pass.Muted);" in relief
            and relief.count("LedgerBehavior.Instance?.RecordSale(item.StringId, 1);") == 1)

def the_rankings_are_dropped_when_the_party_moves_not_only_when_the_hour_turns():
    moved = method_body(S['Ledger.cs'], "private void DropRankingsIfThePartyMoved")
    return ("at.DistanceSquared(_rankedAt) <= MovedFar" in moved
            and "ForgetMarketRankings();" in moved
            and "DropRankingsIfThePartyMoved();" in
                method_body(S['Ledger.cs'], "private List<(Settlement, int)> TopMarkets")
            and "DropRankingsIfThePartyMoved();" in
                method_body(S['Ledger.cs'], "public List<TradeRoute> BestRoutes")
            and "DropRankingsIfThePartyMoved();" in
                method_body(S['Ledger.cs'], "internal void PrimeMarketsFor")
            and S['Ledger.cs'].count("DropRankingsIfThePartyMoved();") == 3)

chk("1.39.0", "an animal a quest is waiting on says so, instead of naming the food reserve",
    a_quest_animal_held_back_is_named_as_the_quest_not_the_food_reserve())
chk("1.39.0", "selling an animal to get back up to speed counts towards your profit and your Trade skill",
    getting_back_up_to_speed_credits_what_it_makes())
chk("1.39.0", "the markets on offer are worked out again as the party moves, not only when the hour turns",
    the_rankings_are_dropped_when_the_party_moves_not_only_when_the_hour_turns())

def the_larder_and_the_stable_leave_the_gold_reserve_whole():
    larder = pass_body("public static void ExecuteResupply")
    stable = pass_body("public static void ExecuteHaulage")
    capped = cap_rule()
    said = "it stops before your gold reaches your reserve"
    reserve = between(S['Trading.cs'], "internal bool WouldReachYourReserve", ";")
    return ("if (pass.WouldReachYourReserve(price)) break;" in larder
            and "if (pass.WouldReachYourReserve(price)) break;" in stable
            and "price >= Spendable()" in reserve
            and "pass.Spendable()" not in larder and "pass.Spendable()" not in stable
            and "if (price > pass.Spendable()) break;" not in S['Trading.cs']
            and "if (price > budget) return Block.BudgetSpent;" in capped
            and said in re.search(r'\{=TL321\}([^"]*)"', M).group(1)
            and said in re.search(r'\{=TL367\}([^"]*)"', M).group(1)
            and said in spoken(ENGLISH).get('TL321', '')
            and said in spoken(ENGLISH).get('TL367', ''))

chk("1.39.3", "restocking and buying a haul animal stop before your gold reaches your reserve, word for word as their own settings promise, and buying for profit is the one pass that may spend down to it",
    the_larder_and_the_stable_leave_the_gold_reserve_whole())

def an_always_sell_entry_cannot_release_an_animal_a_quest_is_waiting_on():
    t = S['Trading.cs']
    sell = method_body(S['Policy.cs'], "internal static bool MaySell(ItemRosterElement el")
    quick = sell_pass()
    carried = method_body(S['Marker.cs'], "private static List<(EquipmentElement item, int amount, int worth, int floor)> WhatYouCarryToSell")
    return (ordered(sell_rule(),
                    "int promised = DrawKeepBack(amount, facts.AwaitedHeld, out bool owed);",
                    "said.KeepCount = promised;",
                    "if (amount <= said.KeepCount) { said.Why = Block.QuestGoods; return said; }",
                    "if (Listed(s.AlwaysSet, good)) { said.Allowed = true; return said; }")
            and ordered(method_body(S['Rules.cs'], "internal static int DrawKeepBack"),
                        "any = held > 0;",
                        "return any ? Math.Min(available, held) : 0;")
            and ordered(method_body(S['Policy.cs'], "private static void TakeBack"),
                        "if (drawn <= 0 || reserve == null) return;",
                        "reserve[item] = held - drawn;")
            and "KeptBack(ItemRoster roster)" not in t
            and "IDictionary<ItemObject, int> foodKeep, out int keepCount)" not in t
            and "_keepBack, _awaited" in quick and "keepBack, awaited" in carried)

chk("1.39.2", "an animal a quest is waiting on is held back even where your always-sell list names it, in every pass that sells",
    an_always_sell_entry_cannot_release_an_animal_a_quest_is_waiting_on())

def every_pass_says_what_it_moved_from_one_place():
    t = S['Trading.cs']
    passes = ("private static void SellPass", "public static void ExecuteResupply",
              "public static void ExecuteHerdRelief", "public static void ExecuteHaulage",
              "private static void BuyPass")
    moved = method_body(t, "internal void Moved")
    said = method_body(t, "private static TextObject PassMessage")
    return (ordered(moved, "if (Sim) return;",
                    "if (profit.HasValue) LedgerBehavior.Instance?.AddProfit(profit.Value);",
                    "CoinSound();", "if (Site == null) return;",
                    "LedgerBehavior.Instance?.CaptureSettlement(Site, force: true, KindsMoved());")
            and ordered(said, "TextObject said = Tongue.Text(sim ? simSaid : realSaid);",
                        'said.SetTextVariable("ITEMS", ItemSummary(detail, items));',
                        'said.SetTextVariable("GOLD", gold);', "return said;")
            and t.count('SetTextVariable("ITEMS"') == 1
            and t.count('SetTextVariable("GOLD", gold);') == 1
            and t.count("PassMessage(") == 2
            and "PassMessage(Sim, simSaid, realSaid, Detail, items, gold)" in
                between(t, "internal TextObject Said(", ";")
            and all("pass.Moved(" in method_body(t, one) for one in passes)
            and all("pass.Said(" in method_body(t, one) for one in passes)
            and "PassMessage" not in method_body(t, "public static void ExecuteRoadTrade"))

def a_meeting_on_the_road_answers_to_the_silence_setting():
    t = S['Trading.cs']
    sell = sell_pass()
    buy = buy_pass()
    named = "when you meet a caravan or a party of villagers on the road"
    return ("quiet: true)" in between(t, "internal static Pass Meet(", ";")
            and "internal bool Muted => Counter.Staging || TradeActionBehavior.Muted(Quiet);" in t
            and "private static bool Muted(bool automated) => automated && Options.Current.QuietAutomation;"
                in t
            and sell.count("if (!pass.Muted) Notices.Say(") == 1
            and buy.count("if (!pass.Muted) Notices.Say(") == 1
            and "if (StillSettling(Muted(automated: true))) return;" in
                method_body(t, "public static void ExecuteRoadTrade")
            and "if (!quiet)" in method_body(t, "private static bool StillSettling")
            and "AwardTradeXpForOurOwnTrade(moved.Earned, pass.Muted);" in sell
            and "AwardTradeXpForOurOwnTrade(profit, false)" not in t
            and named in re.search(r'\{=TL349\}([^"]*)"', M).group(1)
            and named in spoken(ENGLISH)['TL349'])

def a_price_move_keeps_the_markets_in_reach_it_did_not_change():
    ledger = S['Ledger.cs']
    priced = method_body(ledger, "private void ForgetPricedRankings")
    whole = method_body(ledger, "internal void ForgetMarketRankings")
    capture = method_body(ledger, "public void CaptureSettlement")
    return ("_marketCache.Clear();" in priced
            and "_routes = null;" in priced
            and "_candidates = null;" not in priced
            and ordered(whole, "ForgetPricedRankings();", "_candidates = null;")
            and ledger.count("_candidates = null;") == 1
            and "ForgetPricedRankings();" in method_body(ledger, "private void DropRankings(")
            and "ForgetMarketRankings();" not in capture
            and "_candidates" not in method_body(ledger, "private void DropRankings(")
            and "ForgetMarketRankings();" in
                method_body(ledger, "private void DropRankingsIfThePartyMoved")
            and "LedgerBehavior.Instance?.ForgetMarketRankings();" in
                method_body(S['Panel.cs'], "public void ExecuteRefresh")
            and "price" not in method_body(ledger, "private static bool Eligible"))

def the_keep_back_is_drawn_down_in_one_place():
    t = S['Trading.cs']
    draw = method_body(S['Rules.cs'], "internal static int DrawKeepBack")
    sell = method_body(S['Policy.cs'], "internal static bool MaySell(ItemRosterElement el")
    return (ordered(draw, "any = held > 0;",
                    "return any ? Math.Min(available, held) : 0;")
            and S['Rules.cs'].count("DrawKeepBack(") == 3
            and t.count("DrawKeepBack(") == 0
            and "facts.AwaitedHeld = HeldBack(awaited, item);" in sell
            and "facts.FoodHeld = HeldBack(foodKeep, item);" in sell
            and "TakeBack(awaited, item, said.DrewAwaited);" in sell
            and "TakeBack(foodKeep, item, said.DrewFood);" in sell
            and S['Policy.cs'].count("private static void TakeBack(") == 1
            and "awaited[item] =" not in t
            and "foodKeep[item] =" not in t)

def the_always_sell_hint_names_the_good_a_quest_is_waiting_on():
    named = "a good a quest is waiting on still hold"
    words = {
        'TradeLord/ModuleData/Languages/TR/module_strings_tr.xml':
            ('g\u00f6rev', 'mal'),
        'TradeLord/ModuleData/Languages/RU/module_strings_ru.xml':
            ('\u0437\u0430\u0434\u0430\u043d\u0438', '\u0442\u043e\u0432\u0430\u0440'),
        'TradeLord/ModuleData/Languages/CNs/module_strings_cns.xml':
            ('\u4efb\u52a1', '\u8d27\u7269'),
    }
    if named not in re.search(r'\{=TL332\}([^"]*)"', M).group(1):
        return False
    if named not in spoken(ENGLISH)['TL332']:
        return False
    if set(words) != set(TRANSLATIONS.values()):
        return False
    for path, (quest, goods) in words.items():
        said = spoken(path)
        if quest not in said['TL384']:
            return False
        if quest not in said['TL332'] or goods not in said['TL332']:
            return False
    return True

def every_market_pass_is_opened_and_carried_by_one_object():
    t = S['Trading.cs']
    passes = ("private static void SellPass", "public static void ExecuteResupply",
              "public static void ExecuteHerdRelief", "public static void ExecuteHaulage",
              "private static void BuyPass")
    held = method_body(t, "private sealed class Pass")
    opened = method_body(t, "internal static Pass Open")
    alone = ("SettlementComponent market = settlement.SettlementComponent;",
             "PartyBase shop = settlement.Party;",
             "ISet<string> locked = TradePolicy.LockedKeys();",
             "bool sim = Options.Current.SimulationMode;",
             "bool directionError = false;",
             "int goldBefore = Hero.MainHero.Gold;",
             "var detail = new Dictionary<ItemObject, (int count, int gold)>();")
    return (all(field in held for field in (
                "internal readonly Settlement Site;",
                "internal readonly SettlementComponent Market;",
                "internal readonly PartyBase Shop;",
                "internal readonly PartyBase Me;",
                "internal readonly MobileParty Party;",
                "internal readonly MobileParty Met;",
                "internal readonly IMarketData Road;",
                "internal readonly Books Books;",
                "internal readonly bool Sim;",
                "internal readonly bool Quiet;",
                "internal bool DirectionError;"))
            and ordered(opened, "if (!MarketOpen(site, TradeActionBehavior.Muted(quiet))) return null;",
                        "MobileParty party = MobileParty.MainParty;",
                        "return party == null ? null : new Pass(site, null, null, Visit, party, quiet);")
            and t.count("Pass.Open(settlement, quiet)") == 7
            and t.count("if (pass == null) return;") == 5
            and all("if (pass == null) return;" in method_body(t, one) for one in passes)
            and ordered(method_body(t, "internal static void TookTheDeal"),
                        "Pass selling = Pass.Open(settlement, quiet);",
                        "Pass buying = Pass.Open(settlement, quiet);",
                        "if (selling == null || buying == null) return;")
            and not any(one in method_body(t, where) for where in passes for one in alone)
            and "SettlementComponent market = settlement.SettlementComponent;" not in t
            and "PartyBase shop = settlement.Party;" not in t
            and "_locked = TradePolicy.LockedKeys();" in held
            and t.count("Priced.At(Market,") == 1
            and "Priced.At(Market, what, Party, selling)" in
                between(t, "internal int Price(", "Road.GetPrice")
            and t.count("pass.Price(el.EquipmentElement, selling: ") == 6
            and "_pass.Price(_plan[at].EquipmentElement, selling: true)" in t
            and "_pass.Price(Shelf[at].EquipmentElement, selling: false)" in t
            and "TradeActionBehavior.Tally(Detail, item, count, gold)" in
                between(t, "internal void Tally(ItemObject item", ";")
            and t.count("pass.Tally(item, 1, ") == 5
            and t.count("_pass.Tally(Item(at), 1, price);") == 2
            and "CheapestFirst(\n            Pass pass, Func<ItemObject, bool> wanted, float tolerance = 1f)" in t)

chk("1.40.0", "every pass says what it moved and what it did about it from one place, the meeting on the road with them",
    every_pass_says_what_it_moved_from_one_place())
def what_a_good_cost_you_is_carried_by_one_value():
    t = S['Trading.cs']
    sites = (S['Passes.cs'], method_body(t, "public static void ExecuteHerdRelief"))
    held = method_body(S['Passes.cs'], "internal struct Basis")
    made = method_body(S['Passes.cs'], "internal static Basis For")
    return (all(field in held for field in (
                "internal int Paid;", "internal bool FromMarket;",
                "internal int PaidLeft;", "internal int UnpaidWorth;"))
            and ordered(made, "basis.Paid = costBasis;",
                        "basis.FromMarket = s.CostBasisMode == 2;",
                        "basis.PaidLeft = Math.Max(0, purchased - books.PaidDrawn(sim, id));",
                        "basis.UnpaidWorth = -1;")
            and t.count("TradePolicy.CostBasis(item)") == 1
            and t.count("LedgerBehavior.Instance?.PurchasedUnits(item)") == 1
            and t.count("Basis basis = Basis.For(") == 1
            and S['Passes.cs'].count("Basis basis = Basis.For(") == 1
            and all("Basis basis = Basis.For(" in one for one in sites)
            and t.count("int worth = basis.Unit(out bool askTheMarket);") == 1
            and S['Passes.cs'].count("int worth = basis.Unit(out bool askTheMarket);") == 1
            and t.count("basis.SoldOne()") == 1
            and S['Passes.cs'].count("basis.SoldOne()") == 2
            and S['Passes.cs'].count("basis.SkipTheUnitsYouPaidFor(ref remaining)") == 1
            and S['Passes.cs'].count("PaidLeft--;") == 1
            and "UnitWorth" not in t + S['Passes.cs'])

chk("1.40.1", "every market pass is opened the same way and carries the market, the party and its own books in one object",
    every_market_pass_is_opened_and_carried_by_one_object())
def what_a_pass_moved_in_gold_is_worked_out_in_two_places():
    t = S['Trading.cs']
    return ("sim ? simGold : Hero.MainHero.Gold - goldBefore" in
                between(t, "private static int GoldGained(bool sim, int simGold, int goldBefore) =>", ";")
            and "sim ? simSpent : goldBefore - Hero.MainHero.Gold" in
                between(t, "private static int GoldSpent(bool sim, int simSpent, int goldBefore) =>", ";")
            and "GoldGained(Sim, simGold, _goldBefore)" in
                between(t, "internal int Gained(int simGold) =>", ";")
            and "GoldSpent(Sim, simSpent, _goldBefore)" in
                between(t, "internal int Spent(int simSpent) =>", ";")
            and t.count("GoldGained(") == 2
            and t.count("GoldSpent(") == 2
            and t.count("Hero.MainHero.Gold - ") == 2
            and t.count(" - Hero.MainHero.Gold") == 2
            and "gold = selling ? Hero.MainHero.Gold - before : before - Hero.MainHero.Gold;" in
                method_body(t, "private static bool SwapOneUnit")
            and "GoldGained(" not in method_body(t, "public static void ExecuteRoadTrade")
            and "GoldSpent(" not in method_body(t, "public static void ExecuteRoadTrade")
            and "pass.Gained(moved.SimGold)" in method_body(t, "private static void SellPass")
            and "pass.Gained(simGold)" in method_body(t, "public static void ExecuteHerdRelief")
            and "pass.Spent(moved.SimGold)" in method_body(t, "private static void BuyPass")
            and all("pass.Spent(simSpent)" in method_body(t, one) for one in
                    ("public static void ExecuteResupply",
                     "public static void ExecuteHaulage")))

chk("1.40.1", "what a good cost you is one value that every sale reads, draws down and asks what a unit is worth",
    what_a_good_cost_you_is_carried_by_one_value())
def a_market_swap_makes_nothing_new_per_unit():
    t = S['Trading.cs']
    held = method_body(t, "private sealed class Pass")
    sell = method_body(t, "internal bool SellOne(")
    buy = method_body(t, "internal bool BuyOne(")
    return (all(f in held for f in ("private ItemRosterElement _unit;",
                                    "private Action _sellUnit;",
                                    "private Action _buyUnit;"))
            and ordered(sell, "_unit = el;", "if (_sellUnit == null)",
                        "? (Action)(() => SellItemsAction.Apply(Me, Shop, _unit, 1, Site))",
                        "return Swap(true, _sellUnit, what, named, out gold);")
            and ordered(buy, "_unit = el;", "if (_buyUnit == null)",
                        "? (Action)(() => SellItemsAction.Apply(Shop, Me, _unit, 1, Site))",
                        "return Swap(false, _buyUnit, what, named, out gold);")
            and sorted(re.findall(r'(_\w+) =\s*\n?\s*Site != null', held)) == ['_buyUnit', '_sellUnit']
            and held.count("(() =>") == 2
            and all(", () =>" not in method_body(t, one)
                    for one in ("internal bool SellOne(", "internal bool BuyOne(")))

chk("1.40.1", "what a pass took in and what it paid out are each worked out in one place, on a dry run and a real one alike",
    what_a_pass_moved_in_gold_is_worked_out_in_two_places())
chk("1.40.2", "handing one unit over at a market reuses the same two errands rather than making a new one each time",
    a_market_swap_makes_nothing_new_per_unit())
chk("1.40.0", "trading with a caravan or villagers on the road is silenced by the same setting a market visit is, and the setting says so",
    a_meeting_on_the_road_answers_to_the_silence_setting())
chk("1.40.0", "a market whose prices moved drops the rankings prices decide and keeps the markets in reach they do not",
    a_price_move_keeps_the_markets_in_reach_it_did_not_change())
chk("1.40.0", "the food reserve and the animals a quest is waiting on are drawn down in one place, named for what it does",
    the_keep_back_is_drawn_down_in_one_place())
chk("1.40.0", "the always-sell setting says a good a quest is waiting on is still held back",
    the_always_sell_hint_names_the_good_a_quest_is_waiting_on())

def a_road_swap_makes_nothing_new_per_unit():
    t = S['Trading.cs']
    held = method_body(t, "private sealed class Pass")
    road = method_body(t, "public static void ExecuteRoadTrade")
    return ("private int _unitPrice;" in held
            and "_unitPrice = price;" in method_body(t, "internal bool SellOne(")
            and "_unitPrice = price;" in method_body(t, "internal bool BuyOne(")
            and ": () => HandOver(Me, Shop, _unit.EquipmentElement, _unitPrice);" in held
            and ": () => TakeDelivery(Shop, Me, _unit.EquipmentElement, _unitPrice);" in held
            and "() =>" not in road
            and "HandOver" not in road and "TakeDelivery" not in road)

def a_dry_run_names_the_goods_it_already_moved():
    sell = sell_pass()
    return ("int remaining = market.YoursToSell(at) - keep;\n"
            "                if (remaining <= 0) { tally.Note(Block.TradedHereAlready); continue; }" in sell
            and "Block.FoodReserve" not in sell
            and 'case Block.TradedHereAlready:' in method_body(S['Reasons.cs'], "internal static TextObject Phrase")
            and "{=TL48}you already traded these on this visit" in S['Reasons.cs'])

chk("1.40.3", "handing one unit over to a party on the road reuses the same two errands rather than making a new one each time",
    a_road_swap_makes_nothing_new_per_unit())
chk("1.40.3", "a dry run that has already sold a good says so, instead of naming the food reserve",
    a_dry_run_names_the_goods_it_already_moved())

def a_market_and_a_meeting_on_the_road_run_the_same_two_passes():
    t = S['Trading.cs']
    road = method_body(t, "public static void ExecuteRoadTrade")
    sell = sell_pass()
    buy = buy_pass()
    return (t.count("private static void SellPass(Pass pass, string label, string what, string named, string why)") == 1
            and t.count("private static void BuyPass(Pass pass, string label, string what, string named, string why)") == 1
            and t.count("SellPass(") == 3 and t.count("BuyPass(") == 3
            and 'SellPass(Pass.Open(settlement, quiet), "quick-sell", "selling", "Selling", "the selling pass");' in t
            and 'BuyPass(Pass.Open(settlement, quiet), "quick-buy", "buying", "Buying", "the buying pass");' in t
            and 'SellPass(Pass.Meet(met, road, books, party),' in road
            and 'BuyPass(Pass.Meet(met, road, books, party),' in road
            and len(road.splitlines()) < 24
            and all(word not in road for word in
                    ("ItemRoster", "Basis", "TradePolicy.", "WhatStopsBuying", "InAPass",
                     "Notices.Say(", "Log.Write", "SwapOneUnit", "simWeight", "herdRoom"))
            and "internal bool Reports => Site != null;" in t)


def the_venue_is_the_only_thing_a_pass_asks_where_it_is():
    t = S['Trading.cs']
    sell = sell_pass()
    buy = buy_pass()
    held = method_body(t, "private sealed class Pass")
    return (sell.count("pass.Site") == 5 and buy.count("pass.Site") == 4
            and sell.count("pass.Reports") == 0 and buy.count("pass.Reports") == 1
            and sell.count("pass.Key") == 1 and buy.count("pass.Key") == 1
            and "Site" not in method_body(t, "public static void ExecuteRoadTrade")
            and all(one in held for one in (
                "internal bool Reports => Site != null;",
                "internal string Key => Site != null ? Site.StringId : Met.StringId;",
                'internal string Where => Site != null ? "at " + Site.Name : "from " + Met.Name;',
                "internal ItemRoster Stock => Site != null ? Site.ItemRoster : Met.ItemRoster;",
                "internal int TillNow => Site != null ? Market.Gold : Met.PartyTradeGold;"))
            and t.count("pass.Where") == 8)


chk("1.40.4", "a market visit and a meeting on the road sell and buy through the same two passes",
    a_market_and_a_meeting_on_the_road_run_the_same_two_passes())
chk("1.40.4", "where a pass is standing is the only thing the shared selling and buying ask about it",
    the_venue_is_the_only_thing_a_pass_asks_where_it_is())

def the_grain_switch_owns_the_reason_it_holds_a_good_back():
    t = S['Trading.cs']
    buy = buy_rule()
    phrase = method_body(S['Reasons.cs'], "internal static TextObject Phrase")
    return ("if (!always && !toFeed && s.NeverBuyGrain && good.IsGrain)\n"
            "            { why = Block.GrainSwitch; return false; }" in buy
            and S['Reasons.cs'].count("Block.NeverList") + S['Passes.cs'].count("Block.NeverList") == 2
            and S['Rules.cs'].count("Block.NeverList") == 2
            and "if (Listed(s.NeverSet, good) || Listed(s.NeverBuySet, good))\n"
                "            { why = Block.NeverList; return false; }" in buy
            and "case Block.GrainSwitch:" in phrase
            and '{=TL388}grain is left alone, since it fills the cargo for little return' in phrase
            and "TL388" in strings_declared()
            and all("TL388" in spoken(f) for f in [ENGLISH] + list(TRANSLATIONS.values()))
            and all(word not in spoken(f)['TL388'].lower()
                    for f in [ENGLISH] + list(TRANSLATIONS.values())
                    for word in ("setting", "ayar", "настройк", "\u8bbe\u7f6e"))
            and "GrainSwitch" not in method_body(S['Passes.cs'], "private static bool Guarded"))


def a_pin_comes_off_the_map_once_tradelord_has_traded_there():
    t = S['Trading.cs']
    moved = method_body(t, "internal void Moved")
    return (ordered(moved, "if (Site == null) return;",
                    "LedgerBehavior.Instance?.CaptureSettlement(Site, force: true, KindsMoved());",
                    'Guard.Run("Pass.PinCleared", () => LedgerPanel.Unpin(Site));')
            and ordered(moved, "if (Sim) return;", 'Guard.Run("Pass.PinCleared"')
            and t.count("LedgerPanel.Unpin(") == 1
            and "internal static bool Unpin(Settlement settlement)" in S['Panel.cs']
            and "_panelPins.Clear();" in method_body(S['Panel.cs'], "internal static void RestorePins"))


def a_market_visit_prices_each_town_once_for_everything_on_the_shelf():
    l = S['Ledger.cs']
    t = S['Trading.cs']
    prime = method_body(l, "internal void PrimeMarketsFor")
    buy = buy_pass()
    cheapest = method_body(t,
        "private static List<(ItemRosterElement el, Good good, int price, int ceiling)> CheapestFirst")
    sell = sell_pass()
    return ("PrimeLiveRankings(cold, hour);" in prime
            and "if (item == null || !asked.Add(item.StringId)) continue;" in prime
            and "if (Ranked(item.StringId, true, hour) && Ranked(item.StringId, false, hour)) continue;"
                in prime
            and ordered(buy, "goods.Add(Item(one.At));",
                        "LedgerBehavior.Instance?.PrimeMarketsFor(goods);",
                        ".WhereThisEarnsFastest(Item(at), paid, units, _pass.Site)")
            and ordered(method_body(S['Passes.cs'], "internal static List<Pick> WhatToBuy"),
                        "market.PriceTheMarketsFor(shelf);",
                        "market.ResaleMarket(one.At, here, carried + take, out int elsewhere)")
            and ordered(cheapest, "goods.Add(it);",
                        "LedgerBehavior.Instance?.PrimeMarketsFor(goods);",
                        "TradePolicy.UnpaidWorth(it);")
            and ordered(sell, "goods.Add(roster.GetItemAtIndex(at));",
                        "LedgerBehavior.Instance?.PrimeMarketsFor(goods);",
                        "? WhatThisStackWouldMake(pass, held) : 0, at));",
                        "Basis basis = Basis.For(")
            and t.count("PrimeMarketsFor(goods);") == 3
            and l.count("PrimeLiveRankings(") == 3)


def each_layer_of_the_trading_code_has_a_file_of_its_own():
    homes = (('Policy.cs', 'public static class TradePolicy'),
             ('Passes.cs', 'internal sealed partial class BlockTally'),
             ('Passes.cs', 'internal static class TradePass'),
             ('Reasons.cs', 'internal static TextObject Phrase'),
             ('Encounters.cs', 'internal static class Errands'),
             ('Encounters.cs', 'internal static class Parley'))
    return (all(what in S[where] for where, what in homes)
            and all(what not in S['Trading.cs'] for _, what in homes)
            and "public class TradeActionBehavior" in S['Trading.cs'])


def the_travel_arithmetic_is_covered_by_tests_the_build_runs():
    return ('TradeMath.cs' in TESTPROJ
            and all(one in MATHTESTS for one in
                    ('TradeMath.SpeedsInEffect', 'TradeMath.FleetSpeed',
                     'TradeMath.DaysAtSpeed', 'TradeMath.DaysAtBestSpeed'))
            and 'TaleWorlds' not in S['TradeMath.cs']
            and "return TradeMath.FleetSpeed(sum, ships.Count, min);" in
                method_body(S['Travel.cs'], "private static float SeaSpeedCore")
            and "return TradeMath.DaysAtBestSpeed(distance, land, sea);" in
                method_body(S['Travel.cs'], "private static float StraightDays"))


chk("1.41.0", "a good the Never buy grain setting holds back says so, rather than blaming your item lists",
    the_grain_switch_owns_the_reason_it_holds_a_good_back())
chk("1.41.0", "a town you pinned loses its pin once TradeLord has traded there",
    a_pin_comes_off_the_map_once_tradelord_has_traded_there())

def a_market_you_step_back_into_inside_the_hour_is_the_same_visit():
    t = S['Trading.cs']
    sitting = method_body(t, "private static bool StillTheSameSitting")
    reset = method_body(t, "private static void ResetVisit")
    entered = method_body(t, "private void OnSettlementEntered")
    forget = method_body(t, "internal static void ForgetVisit")
    return ("int hour = (int)CampaignTime.Now.ToHours;" in sitting
            and "bool same = Arrivals.StillTheSameSitting(settlement?.StringId, _sittingAt," in sitting
            and "here != null && here == sittingAt && hour == sittingHour;" in S['Rules.cs']
            and "A_sitting_is_the_same_only_at_the_same_market_in_the_same_hour" in ARRIVALTESTS
            and ordered(sitting, "bool same =", "_sittingAt = settlement?.StringId;",
                        "_sittingHour = hour;", "return same;")
            and "private static void ResetVisit(bool sameSitting = false)" in t
            and "if (sameSitting) Visit.ForgetTheDryRun(); else Visit.Forget();" in reset
            and "ResetVisit(StillTheSameSitting(settlement));" in entered
            and t.count("StillTheSameSitting(") == 3
            and "_sittingAt = null;" in forget
            and "_sittingHour = -1;" in forget)

def the_log_is_kept_from_one_launch_to_the_next():
    support = S['Support.cs']
    return ('File.AppendAllText(candidate, "");' in
                method_body(support, "private static string Resolve")
            and 'File.WriteAllText(_path, "");' in
                method_body(support, "private static void EmptyIfItOutgrewItsLimit")
            and support.count("File.WriteAllText(") == 1
            and support.count("EmptyIfItOutgrewItsLimit();") == 1
            and "EmptyIfItOutgrewItsLimit();" in
                method_body(support, "private static bool Ready")
            and "File.Delete(" not in support
            and "FileMode.Create" not in support
            and "FileMode.Truncate" not in support
            and "FileMode.Append, FileAccess.Write, FileShare.ReadWrite" in
                method_body(support, "private static StreamWriter Held"))

def the_settings_file_is_left_alone_when_nothing_moved():
    settle = method_body(S['Config.cs'], "internal static void Settle")
    said = method_body(S['Config.cs'], "private static bool SayWhatChanged")
    return (ordered(settle, "_dirty = false;",
                    'bool moved = Guard.Read("Config.Changed", _path, _ => SayWhatChanged(), true);',
                    "if (!moved) return;", 'Guard.Run("Config.Flush"')
            and "if (_lastSeen == null) { _lastSeen = now; return false; }" in said
            and ordered(said, "bool moved = false;", "moved = true;", "return moved;")
            and ordered_last(said, "_lastSeen = now;", "return moved;")
            and S['Config.cs'].count('Write(_path, "a setting changed")') == 1)

def the_purchase_records_it_drops_are_named():
    body = method_body(S['Ledger.cs'], "private void MatchPurchasesToWhatIsHeld")
    return ("var dropped = new List<string>();" in body
            and ordered(body, "int gone = rec.Count - have;",
                        'dropped.Add(gone + " " + rec.ItemId);',
                        "TradeMath.DrainSale(rec, gone);")
            and 'string.Join(", ", dropped.ToArray())' in body)


def the_log_is_emptied_only_once_it_has_outgrown_its_limit():
    support = S['Support.cs']
    empty = method_body(support, "private static void EmptyIfItOutgrewItsLimit")
    ready = method_body(support, "private static bool Ready")
    write = method_body(support, "internal static void Write")
    return ("private const long MostItHolds = 999 * 1024;" in support
            and ordered(empty, "try { held = new FileInfo(_path).Length; }",
                        "if (held <= MostItHolds) return;",
                        'try { File.WriteAllText(_path, ""); }',
                        "_emptied =")
            and ordered(ready, "if (!_resolved)", "_path = Resolve();",
                        "if (_path != null) EmptyIfItOutgrewItsLimit();",
                        "if (_emptied != null)", "Put(said);")
            and ordered(write, "if (!Ready()) return;", "Put(message);")
            and support.count("MostItHolds") == 3)

def one_line_the_log_would_not_take_never_slows_the_rest_of_the_session():
    support = S['Support.cs']
    held = method_body(support, "private static StreamWriter Held")
    letgo = method_body(support, "private static void LetGo")
    return ("_cannotHold" not in support
            and "private static readonly TimeSpan BeforeHoldingAgain = TimeSpan.FromSeconds(30);" in support
            and "if (_open != null || DateTime.UtcNow < _holdAgainAt) return _open;" in held
            and "catch { LetGo(); }" in held
            and ordered(letgo, "_holdAgainAt = DateTime.UtcNow + BeforeHoldingAgain;",
                        "try { _open?.Dispose(); } catch { }", "_open = null;")
            and support.count("LetGo();") == 3)


chk("1.46.0", "TradeLord.log is emptied only once it has outgrown the size it is allowed, and only from the first line a launch writes",
    the_log_is_emptied_only_once_it_has_outgrown_its_limit())
chk("1.46.0", "a line the log would not take costs the file for half a minute rather than for the rest of the session",
    one_line_the_log_would_not_take_never_slows_the_rest_of_the_session())

chk("1.43.0", "stepping straight back into a market inside the same hour carries the same visit on, so neither what it has already spent nor what it has already traded starts again",
    a_market_you_step_back_into_inside_the_hour_is_the_same_visit())
chk("1.43.0", "TradeLord.log is kept from one launch to the next, and the one thing that empties it is reached from the first line of a launch and nowhere else",
    the_log_is_kept_from_one_launch_to_the_next())
chk("1.43.0", "TradeLord.ini is left alone when a settings handover sets every value to the one it already held",
    the_settings_file_is_left_alone_when_nothing_moved())
chk("1.43.0", "a purchase record dropped because the goods left the party unsold names those goods",
    the_purchase_records_it_drops_are_named())

chk("1.76.3", "a good traded by hand is written down in units, never in the gold it fetched, and a purchase never beyond what your party carries",
    a_good_you_bought_by_hand_is_never_counted_beyond_what_you_carry())
chk("1.42.2", "a market visit asks each town its prices once for everything on the shelf and everything in your bags, through the same priming the route scan uses, and never asks again for a good it has already ranked this hour",
    a_market_visit_prices_each_town_once_for_everything_on_the_shelf())
chk("1.42.1", "each layer of the trading code has a file of its own, so none of them is read out of Trading.cs any more",
    each_layer_of_the_trading_code_has_a_file_of_its_own())
chk("1.42.1", "the travel arithmetic stands clear of the game, so a test can ask it what a journey costs",
    the_travel_arithmetic_is_covered_by_tests_the_build_runs())
chk("1.42.0", "trading with a caravan or villagers on the road says why nothing moved, the same as a market visit does",
    a_road_trade_that_moved_nothing_says_why())
chk("1.42.0", "the ledger works a route out from your settings alone and never from what you are carrying, holding or able to spend right now, deliberately, so nothing you happen to be doing can hide a route from you",
    the_ledger_lists_a_route_you_could_not_take_this_second())
chk("1.41.9", "the markets a good could be bought at are picked by keeping the best few as they come, rather than putting every town in order first",
    the_best_markets_are_picked_without_sorting_every_town())
chk("1.85.0", "every market a good could be sold at is put to the route scan and to the buying pass, so a market that pays less than the dearest eight is no longer out of their reach, while the tooltip still shows the dearest",
    every_market_a_good_could_be_sold_at_is_offered_to_the_route_scan())
chk("1.41.9", "a route scan asks each town its prices once for every good it wants, and never prices a town it has already ruled out as too far",
    a_route_scan_prices_each_town_once_for_every_good_it_wants())
chk("1.41.8", "a language file that could not be read is tried again rather than settled for, without going back to disk for every line",
    a_language_file_that_could_not_be_read_is_tried_again())
chk("1.41.7", "a price already written down is found by its town rather than by looking down every town on the list, and the save is still written the way it always was",
    a_price_is_found_by_its_town_rather_than_by_looking_down_the_list())
chk("1.41.7", "TradeLord keeps asking for the settings screen until MCM hands it over, so what you pick on it takes hold without a restart",
    the_screen_is_asked_for_again_until_mcm_hands_it_over())
chk("1.41.7", "the food reserve is worked out where a test can ask it, and the pass maps its answer back onto the goods you carry",
    the_food_reserve_is_worked_out_where_a_test_can_ask_it())
chk("1.41.7", "the selling pass describes a good once and hands the same description to every rule that asks",
    the_sell_pass_describes_a_good_once_and_hands_it_on())
chk("1.41.7", "the rules that decide a purchase stand clear of the game too, and a good on the shelf is described once",
    the_buying_rules_stand_clear_of_the_game_too())
chk("1.41.7", "the rules that decide a sale stand clear of the game, so a test can ask them, and the costly answers stay behind the seam",
    the_selling_rules_stand_clear_of_the_game())
chk("1.41.5", "the feature list names the live-price setting the way the settings screen names it, and never calls it honest-merchant mode",
    the_feature_list_calls_a_setting_what_the_settings_screen_calls_it())
chk("1.41.5", "the feature list says a pin comes off a town once TradeLord has traded there",
    the_feature_list_says_a_pin_comes_off_a_town_it_traded_in())
chk("1.41.4", "the price tooltip and the profit colouring hand their state to the guard rather than closing over it",
    the_tooltip_patches_hand_their_state_over_instead_of_capturing_it())
chk("1.41.4", "a market ranking sorts through one comparison for selling and one for buying, made once",
    a_market_ranking_sorts_through_one_comparison_for_each_way())
chk("1.68.0", "which market is best, and how far is too far, stand clear of the game so a test can run them",
    which_market_is_best_is_worked_out_where_a_test_can_ask_it())
chk("1.68.0", "what is on the road and what a purse will take off the shelf stand clear of the game so a test can run them",
    what_is_on_the_road_is_added_up_where_a_test_can_ask_it())
chk("1.68.0", "the forecast score, the promise score and the store that holds them stand clear of the game so a test can run them",
    how_a_forecast_and_a_promise_held_is_scored_where_a_test_can_ask_it())
chk("1.68.0", "every figure and share the log writes reads the same whatever numbers the player's own language uses",
    the_log_reads_its_figures_the_same_way_in_every_language())
chk("1.41.3", "a good on the shelf is asked once whether it may be bought, and the resale half of the round-trip question stands on its own",
    a_good_on_the_shelf_is_asked_the_buying_questions_once())
chk("1.41.3", "the panel reads the hotkey before it walks the map's layers looking for a text field",
    the_panel_reads_the_key_before_it_walks_the_screen())
chk("1.41.2", "TradeLord.log is held open, a line written on its own is pushed out at once and a burst of lines when the burst ends, with appending a line at a time left as the fallback",
    the_log_is_held_open_and_pushed_out_a_line_at_a_time())
chk("1.41.2", "the market marked on your map skips one whose purse could not earn faster than the best found so far before it prices your cargo there",
    the_marker_skips_a_town_that_cannot_outpay_the_best_one_yet())
chk("1.75.0", "the market marked on your map counts only the cargo the selling rules would really move there, so the marker never sends you somewhere it will refuse to sell",
    the_marker_counts_only_what_the_selling_rules_would_really_move())
chk("1.75.0", "every move of the map marker is written to the log, with what the cargo would fetch there, the town's purse, the days away and the market it beat",
    the_marker_says_in_the_log_which_town_it_picked_and_why())

def a_good_another_pass_handles_never_speaks_for_a_stalled_pass():
    reasons = S['Reasons.cs']
    structural = between(S['Passes.cs'], "private static bool Structural(Block reason) =>",
                         "private static bool Guarded(Block reason) =>")
    guarded = between(S['Passes.cs'], "private static bool Guarded(Block reason) =>",
                      "internal Block Dominant")
    phrase = method_body(reasons, "internal static TextObject Phrase")
    return (set(re.findall(r'Block\.(\w+)', structural))
                == {'NotTradable', 'NotMerchandise', 'MountOrHaulAnimal'}
            and 'MountOrHaulAnimal' not in guarded
            and "case Block.MountOrHaulAnimal:" not in phrase
            and 'TL385' not in strings_declared()
            and 'not traded as livestock' not in ALL
            and "why = Block.MountOrHaulAnimal;" in buy_rule()
            and "said.Why = Block.MountOrHaulAnimal;" in sell_rule())

def goods_you_were_given_are_held_to_your_margin_like_the_ones_you_bought():
    rules = S['Rules.cs']
    sell = sell_pass()
    relief = method_body(S['Trading.cs'], "public static void ExecuteHerdRelief")
    return ("internal static bool TradedAsMerchandise(in Good good) =>" in rules
            and "good.IsTradeGood || good.IsLivestock;" in rules
            and "internal static bool WorthIsWhatYouPaid(in Good good, int paid) =>" in rules
            and "paid > 0 || !TradedAsMerchandise(good);" in rules
            and "internal static int WorthToBeat(in Good good, int paid, int unpaidWorth) =>" in rules
            and "WorthIsWhatYouPaid(good, paid) ? paid : unpaidWorth;" in rules
            and ordered(sell, "int worth = basis.Unit(out bool askTheMarket);",
                        "int mustBeat = TradeRules.WorthToBeat(good, worth, basis.UnpaidWorth);",
                        "if (!TradeMath.ProfitAcceptable(mustBeat, price, s.MinProfitMargin))")
            and "WorthToBeat" not in relief
            and 'Rules.cs' in TESTPROJ
            and "TradeRules.WorthToBeat" in SELLTESTS
            and "TradeMath.ProfitAcceptable" in SELLTESTS)

def trading_on_arrival_waits_for_the_party_to_take_to_the_road():
    t = S['Trading.cs']
    entered = method_body(t, "private void OnSettlementEntered")
    tick = method_body(t, "private void OnTick")
    left = method_body(t, "private void OnSettlementLeft")
    road = method_body(t, "private static void NoteTheRoadTaken")
    return (ordered(entered, 'Drove.LogState("entering " + settlement.Name);',
                    "if (StillTheSameArrival(settlement))",
                    "NoteThisArrival(settlement);",
                    "ExecuteQuickSell(settlement, quiet: true);")
            and "Arrivals.StillTheSame(settlement?.StringId, _lastArrivalAt, _tookToTheRoad);" in t
            and "here != null && here == lastArrivalAt && !tookToTheRoad;" in S['Rules.cs']
            and "already || (gateKnown && squaredFromTheGate > SetOffFromTheGate);" in S['Rules.cs']
            and "_tookToTheRoad = Arrivals.TakenToTheRoad(" in road
            and "Taking_to_the_road_ends_an_arrival_even_at_the_same_market" in ARRIVALTESTS
            and "The_road_is_taken_once_you_are_further_from_the_gate_than_the_threshold" in ARRIVALTESTS
            and ordered(tick, "if (party == null || party.CurrentSettlement != null) return;",
                        "NoteTheRoadTaken(party);")
            and "NoteTheGateBehind(party);" in left
            and "_gateBehind = party.GetPosition2D;" in
                method_body(t, "private static void NoteTheGateBehind")
            and "_tookToTheRoad, _gateBehindKnown," in road
            and "party.GetPosition2D.DistanceSquared(_gateBehind));" in road
            and "ForgetArrivals();" in method_body(t, "internal static void ForgetVisit"))

def the_price_trace_reads_one_price_four_ways_and_names_what_changes_it():
    trace = method_body(S['Market.cs'], "internal static class PriceTrace")
    say = method_body(S['Market.cs'], "internal static void Say(Settlement site, string when)")
    written = method_body(S['Market.cs'], "private static void Written(Settlement site, string when)")
    ledger = S['Ledger.cs']
    return ("if (!Options.Current.ExtendedDebugLogging || site == null) return;" in say
            and 'Guard.Run("PriceTrace", () => Written(site, when));' in say
            and "Priced.At(market, el, MobileParty.MainParty, true)" in trace
            and "market.GetItemPrice(el, MobileParty.MainParty, true)" in trace
            and "kept.GetPrice(el, who, true, merchant)" in trace
            and "kept.GetPrice(el, who, false, merchant)" in trace
            and ordered(written,
                        "TradeLord uses \" + Uses(market, el)",
                        "Asked(market, el)",
                        "Read(kept, el, MobileParty.MainParty, null)",
                        "Read(kept, el, null, null)")
            and "Campaign.Current.Models.TradeItemPriceFactorModel" in trace
            and "Harmony.GetPatchInfo(method)" in trace
            and "found.Owners" in trace
            and ordered(written, "MarketPrice(market.GetType())",
                        "MarketPrice(typeof(SettlementComponent))",
                        "if (inherited != null && inherited != asked)")
            and 'PriceTrace.Say(settlement, "walked in, before anything was traded");' in S['Trading.cs']
            and ordered(method_body(S['Trading.cs'], "private void OnSettlementEntered"),
                        'PriceTrace.Say(settlement, "walked in, before anything was traded");',
                        "ExecuteQuickSell(settlement, quiet: true);")
            and "PriceTrace" not in method_body(ledger, "private void OnSettlementEntered")
            and 'PriceTrace.Say(Settlement.CurrentSettlement, "traded by hand");' in ledger
            and not any(w in trace for w in ("SellItemsAction", "ChangeGold", "AddToCounts")))


def every_price_is_asked_the_way_the_trade_that_follows_is_charged():
    market = S['Market.cs']
    at = method_body(market, "internal static int At(SettlementComponent market, "
                             "EquipmentElement el, MobileParty who, bool selling)")
    plain = sorted(name for name, text in S.items()
                   if name != 'Market.cs' and "GetItemPrice(" in text)
    compared = between(market, "private static string Asked(", "private static string Read(")
    return (plain == []
            and market.count("GetItemPrice(") == 5
            and compared.count("market.GetItemPrice(") == 2
            and ordered(at, "IMarketData held = Kept(site);",
                        "return held.GetPrice(el, who, selling, Merchant(site));",
                        "return market.GetItemPrice(el, who, selling);")
            and all("Priced.At(" in S[f] for f in ('Ledger.cs', 'TooltipPatches.cs', 'Trading.cs')))


def keeping_food_and_restocking_it_are_one_setting():
    lift = method_body(S['Migrate.cs'], "private static bool KeepingAndRestockingFoodBecameOneSetting")
    return ("ResupplyFoodDays" not in S['Options.cs']
            and "ResupplyFoodDays" not in M
            and "ResupplyFoodDays" not in S['Policy.cs']
            and "ResupplyFoodDays" not in S['Trading.cs']
            and "public int KeepFoodDays = 3;" in S['Options.cs']
            and "int days = Options.Current.KeepFoodDays;" in
                method_body(S['Policy.cs'], "internal static int FoodWanted")
            and "if (Options.Current.KeepFoodDays <= 0) return;" in
                method_body(S['Trading.cs'], "public static void ExecuteResupply")
            and 'const string was = "ResupplyFoodDays";' in lift
            and "written.Remove(was);" in lift
            and "if (from < 8) changed |= KeepingAndRestockingFoodBecameOneSetting(written, notes);"
                in S['Migrate.cs']
            and "TheRestockDaysAreDroppedAndTheKeepDaysStand" in MIGRATIONTESTS)

def the_log_says_what_the_market_charged_against_what_it_was_quoted():
    t = S['Trading.cs']
    quoted = method_body(t, "private static string Quotation")
    detail = method_body(t, "private static void LogDetail")
    return ('"which TradeLord had quoted at "' not in t
            and '", which TradeLord had quoted at " + said.gold' in quoted
            and '" and the market charged the same"' in quoted
            and '" and the market moved " + gold + " instead"' in quoted
            and "if (quoted == null || !quoted.TryGetValue(item, out var said) || said.count <= 0) return \"\";"
                in quoted
            and "Quotation(quoted, kv.Key, kv.Value.gold) +" in detail
            and "if (Options.Current.ExtendedDebugLogging) TradeActionBehavior.Tally(Quoted, item, count, gold);"
                in method_body(t, "internal void Quote(ItemObject item, int count, int gold)")
            and t.count("pass.Quote(item, 1, price);") == 2
            and ordered(t, "pass.Quote(item, 1, price);",
                           "_pass.SellOne(_plan[at], price, _what, _named, out proceeds)")
            and ordered(t, "pass.Quote(item, 1, price);",
                           "_pass.BuyOne(Shelf[at], price, _what, _named, out cost)"))


def the_log_names_everything_that_stops_when_the_herd_cannot_be_read():
    t = S['Trading.cs']
    room = method_body(S['Drove.cs'], "internal static int RoomForLivestock")
    shed = method_body(S['Drove.cs'], "internal static int AnimalsToShed")
    check = method_body(S['Drove.cs'], "internal static void LogState(string when, int counted)")
    stops = ("the herd cannot be counted, so no livestock and no haul animals are bought "
             "and no animal is sold to get you back up to speed; every other trade is unaffected")
    return (S['Drove.cs'].count(stops) == 3
            and "livestock buying disabled" not in t
            and "selling unaffected" not in t
            and "if (model == null) return 0;" in room
            and "if (model == null) return 0;" in shed
            and "Drove.RoomForLivestock(pass.Party)" in pass_body("public static void ExecuteHaulage")
            and "Drove.RoomForLivestock(_pass.Party)" in buy_pass()
            and "Drove.AnimalsToShed(pass.Party)" in pass_body("public static void ExecuteHerdRelief")
            and '"the herd penalty cannot be read on this game version"' in check
            and '"no herd penalty"' in check
            and ordered(check, "_lookupFailed",
                        '"the herd penalty cannot be read on this game version"',
                        '"no herd penalty"'))


def the_reset_whip_is_one_switch_the_lift_can_never_outlive_or_set_off():
    whip = method_body(S['Migrate.cs'], "public static class Whip")
    crack = method_body(S['Migrate.cs'], "public static bool Crack(bool cracks, IDictionary<string, string> written)")
    read = method_body(S['Config.cs'], "private static void Read")
    back = method_body(S['Config.cs'], "private static void BackToWhatItShipsWith")
    said = method_body(S['Config.cs'], "private static void SayWhatYouHadSet")
    return ("public const bool Armed = true;" in whip
            and "public const int CracksAt = " in whip
            and ("public static bool Cracks(bool armed, int cracksAt, int shipped, int shape) =>\n"
                 "            armed && cracksAt > 0 && cracksAt == shipped && shape < cracksAt;") in whip
            and "public static bool CracksOn(int shape) => Cracks(Armed, CracksAt, Migration.Shape, shape);" in whip
            and "if (!cracks || written == null) return false;" in crack
            and "written.Clear();" in crack
            and "Whip" not in method_body(S['Migrate.cs'], "public static bool Lift")
            and ordered(read, "bool lifted = Migration.Lift(shape, written, notes);",
                        "bool whipped = Whip.CracksOn(shape);",
                        "if (!whipped)",
                        "foreach (string note in notes) Log.Write",
                        "SayWhatYouHadSet(written);",
                        "Whip.Crack(shape, written);",
                        "BackToWhatItShipsWith();",
                        "if (Twins.ScreenWins(screen, screenWroteIt, handEdited))",
                        "if (Taken(field, line.Value)) taken++;",
                        "else if (whipped)")
            and "var stock = new Options();" in back
            and "foreach (FieldInfo field in Fields()) field.SetValue(Options.Current, field.GetValue(stock));"
                in back
            and '"  you had " + field.Name + " = " + line.Value' in said
            and all(t in MIGRATIONTESTS for t in
                    ("AnArmedWhipCracksOnAFileOlderThanTheShapeItIsArmedAt",
                     "AnArmedWhipNeverCracksOnAFileAtOrPastTheShapeItIsArmedAt",
                     "ADisarmedWhipNeverCracksOnAnything",
                     "AWhipArmedAtAShapeThisVersionDoesNotShipNeverCracks",
                     "AWhipArmedAtNoShapeAtAllNeverCracks",
                     "NothingTheLiftCarriedForwardSurvivesAWhipThatCracks",
                     "AWhipThatDoesNotCrackLeavesTheLiftsWorkExactlyAsItFoundIt",
                     "ThisVersionShipsTheWhipSpentSoAnOlderFileIsLiftedRatherThanReset",
                     "AFileAlreadyAtTheShapeThisVersionShipsIsNeverResetBySecondTime",
                     "TheWhipIsWiredToTheShapeItWasArmedAtAndIsSpentOnceALaterShapeShips",
                     "TheWhipLeavesTheLiftItselfAlone")))


chk("1.46.2", "a mount or a haul animal is never named as the reason a pass moved nothing, since another pass handles it",
    a_good_another_pass_handles_never_speaks_for_a_stalled_pass())
chk("1.46.2", "trade goods and livestock you never paid for are held to your margin against what they are worth, and looted gear is not",
    goods_you_were_given_are_held_to_your_margin_like_the_ones_you_bought())
chk("1.46.2", "trading on arrival runs once and waits for the party to take to the road before it runs again",
    trading_on_arrival_waits_for_the_party_to_take_to_the_road())

chk("1.47.0", "the price trace reads one market's price four ways, names the price model and any mod changing it, and trades nothing",
    the_price_trace_reads_one_price_four_ways_and_names_what_changes_it())
chk("1.87.0", "the one log switch ships on, so every log carries the price readings, the forecast score and the marker's workings",
    "public bool ExtendedDebugLogging = true;" in S['Options.cs'] and
    "public bool PriceTrace" not in S['Options.cs'] and
    "public bool ForecastScore" not in S['Options.cs'] and
    "public bool Ultralog" not in S['Options.cs'])

chk("1.47.1", "every price TradeLord quotes is asked of the market the way the trade that follows it is charged, and falls back to the plain question only if that cannot be asked",
    every_price_is_asked_the_way_the_trade_that_follows_is_charged())


def one_stack_of_a_good_never_spends_what_another_stack_holds():
    t = S['Trading.cs']
    held = method_body(t, "private sealed class Pass")
    counted = re.findall(r'[^;{}]*pass\.Books\.(?:Held|Stocked)\(pass\.Sim,[^;]*;', t, re.S)
    held_afresh = re.findall(r'[^;{}]*books\.(?:Held|Stocked)\(sim,[^;]*;', S['Passes.cs'], re.S)
    return ("internal int YoursToSell(ItemRosterElement el)" in held
            and "internal int TheirsToSell(ItemRosterElement el)" in held
            and ("return Math.Min(el.Amount,\n"
                 "                                LedgerBehavior.InAll(Party.ItemRoster, item)"
                 " + Books.Held(Sim, item.StringId));") in held
            and ("return Math.Min(el.Amount,\n"
                 "                                LedgerBehavior.InAll(Stock, item)"
                 " - Books.Stocked(Sim, item.StringId));") in held
            and t.count("pass.YoursToSell(el)") == 2
            and t.count("pass.TheirsToSell(el)") == 3
            and "_pass.YoursToSell(_plan[at])" in t
            and "_pass.TheirsToSell(Shelf[at])" in t
            and len(counted) == 2
            and all("LedgerBehavior.InAll(" in one and "el.Amount" not in one for one in counted)
            and len(held_afresh) == 4
            and all("market.Carried(" in one and "AmountAt(" not in one for one in held_afresh))


chk("1.47.2", "a stack of a good counts only its own units against what a dry run has already moved, so another stack of the same good is still offered",
    one_stack_of_a_good_never_spends_what_another_stack_holds())

def every_till_is_read_the_one_way_a_pass_reads_it():
    t = S['Trading.cs']
    return (t.count("pass.TillNow") == 2 and "pass.Market" not in t
            and "internal int TillNow => Site != null ? Market.Gold : Met.PartyTradeGold;" in t)


chk("1.47.2", "every pass asks what the merchant has left to pay with in the one place that knows where the pass is standing",
    every_till_is_read_the_one_way_a_pass_reads_it())


def the_running_total_outgrows_an_int_and_a_save_written_before_it_still_reads():
    ledger = method_body(S['Ledger.cs'], "public override void SyncData")
    capped = between(S['Ledger.cs'], "private static int Capped(long total) =>", ";")
    return ("private long _lifetimeProfit;" in S['Ledger.cs']
            and "public long LifetimeProfit => _lifetimeProfit;" in S['Ledger.cs']
            and "public void AddProfit(int amount) => _lifetimeProfit += amount;" in S['Ledger.cs']
            and "int.MaxValue" in capped and "int.MinValue" in capped
            and ordered(ledger,
                        "_lifetimeProfitCapped = Capped(_lifetimeProfit);",
                        'dataStore.SyncData("TradeLord_LifetimeProfit", ref _lifetimeProfitCapped);',
                        'dataStore.SyncData("TradeLord_LifetimeProfitWide", ref _lifetimeProfit);',
                        "if (dataStore.IsLoading && _lifetimeProfit == 0L)"
                        " _lifetimeProfit = _lifetimeProfitCapped;")
            and "long lifetime = LedgerBehavior.Instance?.LifetimeProfit ?? 0L;" in S['Panel.cs'])


chk("1.47.3", "the running total of what TradeLord made you outgrows what an int holds, and a campaign saved before it still reads its total back",
    the_running_total_outgrows_an_int_and_a_save_written_before_it_still_reads())


def the_lot_shape_the_counting_rests_on_is_held_against_the_game():
    named = re.search(r'LotShape\s*=\s*\{(.*?)\};', COMPAT, re.S)
    if named is None:
        return False
    held = set(kind.split('.')[-1] + '.' + member for kind, member in
               re.findall(r'\(\s*"([\w.]+)"\s*,\s*"(\w+)"\s*,', named.group(1)))
    return ("private static void CheckLotShape" in COMPAT
            and "CheckLotShape(versions);" in COMPAT
            and {"EquipmentElement.Item", "EquipmentElement.ItemModifier",
                 "EquipmentElement.IsQuestItem", "ItemRoster.FindIndexOfItem",
                 "ItemRoster.FindIndexOfElement", "ItemRoster.GetItemAtIndex",
                 "ItemRoster.GetElementNumber", "ItemRoster.GetElementCopyAtIndex"} <= held
            and "at.TryGetValue(held.Good.Id, out int seen)" in S['Rules.cs']
            and S['Trading.cs'].count("Math.Min(el.Amount,") == 2)


chk("1.47.4", "the game is asked, every version, whether one good still sits in more than one lot of the bags, since the food reserve and the per-lot caps are counted on it",
    the_lot_shape_the_counting_rests_on_is_held_against_the_game())


def a_hint_that_sends_you_to_a_setting_names_it_the_way_the_screen_does():
    for path in [ENGLISH] + list(TRANSLATIONS.values()):
        said = spoken(path)
        for cites, named in (('TL362', 'TL221'), ('TL361', 'TL221'), ('TL345', 'TL241')):
            if cites not in said or named not in said:
                return False
            if said[named] not in said[cites]:
                return False
    return "Restock and keep food (days of supply) above is the one that works in days." in M


chk("1.47.5", "a hint that sends you to another setting calls it what the settings screen calls it, in every language",
    a_hint_that_sends_you_to_a_setting_names_it_the_way_the_screen_does())


def the_hints_publish_the_defaults_the_source_ships():
    defaults = dict(re.findall(r'public\s+(?:bool|int|float|string)\s+(\w+)\s*=\s*([^;]+);',
                               S['Options.cs']))
    blocks = re.findall(
        r'\[SettingProperty\w+\("\{=TL\d+\}([^"]+)".*?HintText = "\{=TL\d+\}((?:[^"\\]|\\.)*)"\)\]\s*'
        r'\[SettingPropertyGroup[^\]]*\]\s*public\s+[\w<>]+\s+(\w+)', M, re.S)
    tested = 0
    for label, hint, name in blocks:
        shipped = defaults.get(name, '').strip().rstrip('f').strip('"')
        for said in re.findall(r'[Dd]efault\s+([0-9]+(?:\.[0-9]+)?)', hint):
            tested += 1
            try:
                if abs(float(said) - float(shipped)) > 1e-6:
                    return False
            except ValueError:
                return False
        for state in re.findall(r'\b(ON|OFF) by default\b', hint):
            tested += 1
            if ('true' if state == 'ON' else 'false') != shipped:
                return False
        if '(default)' in label:
            tested += 1
            if shipped != 'true':
                return False
    return len(blocks) > 40 and tested >= 16


chk("1.47.5", "every default the settings screen advertises is the one the source ships",
    the_hints_publish_the_defaults_the_source_ships())

chk("1.48.0", "keeping food back and restocking it are one setting, and a file that carried the old pair is lifted onto it",
    keeping_food_and_restocking_it_are_one_setting())
chk("1.48.0", "with the price trace on, every good a pass moves is logged with what TradeLord quoted next to what the market charged",
    the_log_says_what_the_market_charged_against_what_it_was_quoted())
chk("1.48.0", "the log names everything that stops when the herd cannot be read, and the herd check says it could not read the penalty rather than reporting none",
    the_log_names_everything_that_stops_when_the_herd_cannot_be_read())

def the_food_reserve_holds_against_thinning_the_herd_too():
    relief = method_body(S['Trading.cs'], "public static void ExecuteHerdRelief")
    kept = method_body(S['Policy.cs'], "internal static Dictionary<ItemObject, int> KeptBack(ItemRoster roster,")
    return ("Errands.Promised();" not in relief
            and "TradePolicy.KeptBack(mine, pass.Books, pass.Sim, out Dictionary<ItemObject, int> promised);" in relief
            and ordered(relief,
                        "ItemRoster mine = pass.Party.ItemRoster;",
                        "TradePolicy.KeptBack(mine, pass.Books, pass.Sim, out Dictionary<ItemObject, int> promised);",
                        "if (promised == null) return;",
                        "if (promised.TryGetValue(item, out int owed) && owed > 0)")
            and "Named(TradeRules.FoodKeep(carried, AppetitePerDay(), Options.Current), byId);" in kept
            and "awaited[owed.Key] = had + owed.Value;" in kept
            and "if (good.Id == null || good.HasHorse) return 0;" in
                method_body(S['Rules.cs'], "internal static int FoodValue"))


def both_reserves_are_kept_back_not_just_the_larger_one():
    return ("said.KeepCount += reserved;" in sell_rule()
            and "if (reserved > said.KeepCount) said.KeepCount = reserved;" not in sell_rule()
            and ordered(sell_rule(),
                        "said.KeepCount = promised;",
                        "int reserved = DrawKeepBack(amount - said.KeepCount, facts.FoodHeld, out bool fed);",
                        "said.KeepCount += reserved;")
            and "An_animal_held_by_both_a_quest_and_the_food_reserve_is_kept_back_for_both" in SELLTESTS
            and "A_good_a_quest_wants_and_the_larder_wants_is_held_back_for_both" in SELLTESTS)


def what_a_lot_cost_holds_still_while_the_lot_drains():
    drain = method_body(S['TradeMath.cs'], "public static void DrainSale")
    return (ordered(drain,
                    "int drain = Math.Min(count, rec.Count);",
                    "int left = rec.Count - drain;",
                    "if (left <= 0) { rec.Count = 0; rec.TotalPaid = 0; return; }",
                    "int unit = (int)Math.Round((double)rec.TotalPaid / rec.Count);",
                    "rec.Count = left;",
                    "rec.TotalPaid = unit > 0 ? unit * left : 0;")
            and "rec.TotalPaid -=" not in drain
            and "Selling_a_lot_down_one_at_a_time_never_moves_what_the_rest_cost" in MATHTESTS
            and "Selling_a_lot_in_chunks_leaves_the_rest_costing_the_same_as_selling_it_singly" in MATHTESTS)


chk("1.50.0", "the food reserve holds an animal back from thinning the herd, the same as it does from a sale",
    the_food_reserve_holds_against_thinning_the_herd_too())
chk("1.85.1", "a good a quest is waiting on and the food reserve is holding is kept back for both of them, not for the larger of the two",
    both_reserves_are_kept_back_not_just_the_larger_one())
chk("1.50.0", "what a lot cost a unit holds still as the lot drains, so the price a unit must clear never moves under it",
    what_a_lot_cost_holds_still_while_the_lot_drains())

def a_reset_reaches_the_copy_the_settings_screen_keeps():
    read = method_body(S['Config.cs'], "private static void Read")
    loader = method_body(S['Support.cs'], "internal static class McmLoader")
    arrives = method_body(S['Support.cs'],
                          "internal static void PutBackWhatItShipsWithOnceTheScreenArrives")
    back = method_body(S['Support.cs'], "private static void PutBack()")
    handover = method_body(S['Support.cs'], "internal static void TryHandover")
    reset = method_body(M, "internal static void Reset")
    reseat = method_body(M, "internal static void Reseat")
    return (ordered(read, "Whip.Crack(shape, written);", "BackToWhatItShipsWith();",
                    "McmLoader.PutBackWhatItShipsWithOnceTheScreenArrives();")
            and "internal static Action PutBackWhatItShipsWith;" in loader
            and ordered(arrives, "if (PutBackWhatItShipsWith == null) return;",
                        "if (SettingsInHand) { PutBack(); return; }",
                        "_owedAPutBack = true;")
            and ordered(back, "_owedAPutBack = false;", 'Guard.Run("Mcm.PutBack", PutBackWhatItShipsWith);')
            and ordered(handover, "SettingsInHand = true;", "if (_owedAPutBack) PutBack();")
            and "McmLoader.PutBackWhatItShipsWith = Settings.Reset;" in M
            and ordered(reset, "field.SetValue(Options.Current, now);", "Reseat();", "Options.Bump();")
            and "BaseSettingsProvider.Instance?.SaveSettings(held);" in reseat)


chk("1.50.2", "a reset reaches the copy the settings screen keeps, so a screen that loads late cannot hand the old settings back",
    a_reset_reaches_the_copy_the_settings_screen_keeps())

chk("1.50.2", "the one-time settings reset is one switch, is armed at the shape this version ships, cannot be set off by the lift and wipes everything the lift carried",
    the_reset_whip_is_one_switch_the_lift_can_never_outlive_or_set_off())


def towns_answer_to_their_own_switch_the_same_as_villages():
    gate = between(S['Trading.cs'], "internal static bool IsMarket(Settlement s) =>", ";")
    return ("s.IsTown && Options.Current.TradeWithTowns" in gate
            and "s.IsVillage && Options.Current.TradeWithVillages" in gate
            and option_default('TradeWithTowns') == 'true'
            and "_o.TradeWithTowns" in M
            and EVER_SHIPPED.get('TradeWithTowns') == 'bool')


def the_trade_pool_leads_with_the_three_switches():
    rows = []
    for b in setting_blocks():
        if '[SettingPropertyGroup("{=TL108}Trade Pool", GroupOrder = 2)]' not in b:
            continue
        order = re.search(r'Order = (\d+)', b)
        named = re.search(r'\n\s*public\s+[\w<>]+\s+(\w+)', b)
        if order is None or named is None:
            return False
        rows.append((int(order.group(1)), named.group(1)))
    rows.sort()
    return ([name for _, name in rows] ==
            ['TradeWithTowns', 'TradeWithVillages', 'TradeWithCaravans',
             'MaxTravelDaysTown', 'MaxTravelDaysVillage', 'ExcludeHostileTowns']
            and 'Trade Pool' == spoken(ENGLISH)['TL108'])


def no_hint_runs_past_what_the_screen_can_hold():
    en = spoken(ENGLISH)
    long = [h for h in re.findall(r'HintText = "\{=(TL\d+)\}', M) if len(en.get(h, '')) > 350]
    return not long and len(re.findall(r'HintText = "\{=TL\d+\}', M)) > 40


chk("1.51.0", "trading in towns answers to its own switch, the same as villages",
    towns_answer_to_their_own_switch_the_same_as_villages())

chk("1.51.0", "the Trade Pool group leads with the three switches, then the limits that shape it",
    the_trade_pool_leads_with_the_three_switches())

chk("1.51.0", "no hint runs past what the settings screen can hold",
    no_hint_runs_past_what_the_screen_can_hold())


def the_map_marker_keeps_to_the_same_trade_pool_as_the_scans():
    marker = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    return (ordered(marker, "foreach (Settlement s in Settlement.All)",
                    "if (!TradeActionBehavior.IsMarket(s)) continue;",
                    "if (LedgerBehavior.UnderAttack(s) || LedgerBehavior.VillageShut(s)) { how.Shut++; continue; }",
                    "if (Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(s))")
            and marker.count("if (!TradeActionBehavior.IsMarket(s)) continue;") == 1
            and "Town.AllTowns" not in marker
            and "if (!TradeActionBehavior.IsMarket(s)) return false;" in
                method_body(S['Ledger.cs'], "private static bool Eligible"))


chk("1.51.1", "the market marked on your map is one TradeLord would trade in, so a market kept out of the Trade Pool is kept off the marker too",
    the_map_marker_keeps_to_the_same_trade_pool_as_the_scans())


def the_variety_floor_answers_to_the_days_of_supply():
    body = food_rule()
    en = spoken(ENGLISH)
    return ("if (s.KeepFoodDays <= 0 || carried == null) return keep;" in body
            and "variety <= 0" not in body
            and ordered(body, "if (s.KeepFoodDays <= 0 || carried == null) return keep;",
                        "if (variety > 0)")
            and "Nothing_is_held_back_at_no_days_of_supply_however_the_variety_floor_is_set" in FOODTESTS
            and 'no days of supply, yet " + keep.Count +' in FOODTESTS
            and "It needs Restock and keep food (days of supply) above turned on to do anything"
                in en.get('TL361', '')
            and "0 turns both off." in en.get('TL321', ''))


chk("1.52.1", "no days of supply keeps no food back at all, and the hint under the variety floor says it answers to that setting",
    the_variety_floor_answers_to_the_days_of_supply())


def a_dry_run_draws_the_price_you_paid_down_the_way_a_real_pass_does():
    t = S['Trading.cs']
    basis = method_body(S['Passes.cs'], "internal struct Basis")
    sell = sell_pass()
    herd = method_body(t, "public static void ExecuteHerdRelief")
    return ("internal static Basis For(int costBasis, int purchased, string id, Books books, bool sim,"
                in basis
            and "LedgerBehavior.Instance?.PurchasedUnits(item) ?? 0," in herd
            and "basis.PaidLeft = Math.Max(0, purchased - books.PaidDrawn(sim, id));" in basis
            and t.count("Basis.For(") == 1
            and S['Passes.cs'].count("Basis.For(") == 1
            and "internal static Basis For(" in basis
            and t.count("Basis.For(item)") == 0
            and "if (basis.SoldOne()) books.NotePaidDrawn(good.Id);" in sell
            and "if (basis.SoldOne()) market.RecordedSale(at);" in sell
            and "LedgerBehavior.Instance?.RecordSale(Item(at).StringId, 1);" in sell
            and ordered(herd, "if (basis.SoldOne())",
                        "if (pass.Sim) pass.Books.NotePaidDrawn(item.StringId);",
                        "else LedgerBehavior.Instance?.RecordSale(item.StringId, 1);")
            and t.count("NotePaidDrawn(") == 1
            and S['Passes.cs'].count("NotePaidDrawn(") == 1
            and "internal int PaidDrawn(bool sim, string id) =>" in S['Books.cs']
            and "_dryDrawn.Clear();" in method_body(S['Books.cs'], "internal void ForgetTheDryRun")
            and "OneStackAfterAnotherNeverSpendsThePriceYouPaidTwice" in BOOKTESTS
            and "ARealRunReadsNoPriceYouPaidBackFromTheDryRunBooks" in BOOKTESTS)


chk("1.52.2", "a dry run draws what you paid for a good down across every stack of it, the way a real pass does",
    a_dry_run_draws_the_price_you_paid_down_the_way_a_real_pass_does())


def the_food_reserve_carries_a_dry_run_from_one_pass_to_the_next():
    t = S['Trading.cs']
    keep = method_body(S['Policy.cs'], "private static List<TradeRules.Ration> Carried")
    still = method_body(S['Rules.cs'], "internal static int StillCarried")
    return ("internal static Dictionary<ItemObject, int> KeptBack(ItemRoster roster, Books books, bool sim,"
                in S['Policy.cs']
            and ordered(keep,
                        "soldAlready[item.StringId] = Math.Max(0, -books.Held(sim, item.StringId));",
                        "int amount = TradeRules.StillCarried(soldAlready, item.StringId, el.Amount);",
                        "if (amount <= 0) continue;",
                        "Amount = amount })")
            and ordered(still, "soldAlready[id] = gone - taken;", "return amount - taken;")
            and "int taken = Math.Min(gone, amount);" in still
            and "MobileParty" not in S['Rules.cs'] and "ItemRoster" not in S['Rules.cs']
            and t.count("TradePolicy.KeptBack(") == 2
            and S['Marker.cs'].count("TradePolicy.KeptBack(") == 1
            and "TradePolicy.KeptBack(roster, pass.Books, pass.Sim, out _awaited);" in
                sell_pass()
            and "TradePolicy.KeptBack(mine, pass.Books, pass.Sim, out Dictionary<ItemObject, int> promised);"
                in method_body(t, "public static void ExecuteHerdRelief")
            and "TradePolicy.KeptBack(party.ItemRoster, TradeActionBehavior.TheVisit," in
                method_body(S['Marker.cs'], "private static List<(EquipmentElement item, int amount, int worth, int floor)> WhatYouCarryToSell")
            and "What_a_dry_run_sold_is_taken_off_one_stack_after_another_and_never_twice" in FOODTESTS
            and "A_real_pass_leaves_every_stack_exactly_as_the_party_holds_it" in FOODTESTS
            and "A_reserve_worked_out_after_a_dry_run_reaches_past_what_it_already_sold" in FOODTESTS)


chk("1.52.3", "the food a dry run has already sold is off the reserve for the passes that follow, and the town on your map still reads the party as it stands",
    the_food_reserve_carries_a_dry_run_from_one_pass_to_the_next())


def the_per_item_caps_bind_every_pass_that_buys():
    t = S['Trading.cs']
    larder = method_body(t, "public static void ExecuteResupply")
    haul = method_body(t, "public static void ExecuteHaulage")
    buy = buy_pass()
    caps = method_body(S['Rules.cs'], "internal static Block WhatCapsAGood")
    return ("if (s.BuyCapPerItem > 0 && taken.count >= s.BuyCapPerItem) return Block.ItemCountCap;" in caps
            and "if (s.BuyValueCapPerItem > 0 && taken.spent + price > s.BuyValueCapPerItem) return Block.ItemValueCap;" in caps
            and "if (s.MaxHeldPerItem > 0 && held >= s.MaxHeldPerItem) return Block.HeldEnough;" in caps
            and "Block capped = WhatCapsAGood(good, price, taken, held, shareCap, s);" in
                method_body(S['Rules.cs'], "internal static Block WhatStopsBuying")
            and "WhatCapsAGood(good, price, (countThis, spentThis), held, shareCap)" in larder
            and "WhatCapsAGood(good, price, (countThis, spentThis), held, HoldShareOff)" in haul
            and "private const float HoldShareOff = 0f;" in t
            and "float shareCap = pass.ShareCap;" in larder
            and all("var prior = pass.Books.Purchases(pass.Sim, item.StringId);" in b
                    and "int countThis = prior.count, spentThis = prior.spent;" in b
                    and b.count("countThis++;") == 1 and b.count("spentThis += price;") == 1
                    and b.count("held++;") == 1
                    for b in (larder, haul))
            and "TradeRules.WhatStopsBuying(good, price, market.Spendable(),\n"
                "                                                              (countThis, spentThis), held, shareCap," in buy)


def a_dialogue_line_is_spoken_in_the_language_in_force_when_it_is_offered():
    t = S['Trading.cs']
    tongue = S['Tongue.cs']
    caravan = method_body(S['Encounters.cs'], "private static void AddCaravanLines")
    bandit = method_body(S['Encounters.cs'], "private static void AddBanditLines")
    return ('internal static string Slot(string written) => "{=!}{" + Marker(written) + "}";' in tongue
            and "MBTextManager.SetTextVariable(Marker(written), Text(written), false);" in
                method_body(tongue, "internal static bool Spoken")
            and 'private static string Marker(string written) => "TradeLord_" + Id(written);' in tongue
            and caravan.count("Tongue.Slot(") == 2 and caravan.count("Tongue.Spoken(") == 2
            and bandit.count("Tongue.Slot(") == 2 and bandit.count("Tongue.Spoken(") == 2
            and "Tongue.Text(" not in caravan and "Tongue.Text(" not in bandit
            and S['Encounters.cs'].count("Tongue.Slot(") == 4
            and S['Encounters.cs'].count("Tongue.Spoken(") == 4
            and all(("{=" + said + "}") in caravan + bandit
                    for said in ("TL114", "TL115", "TL387", "TL113")))


chk("1.52.0", "the per-item buy caps bind restocking the larder and buying a haul animal, not just buying for profit",
    the_per_item_caps_bind_every_pass_that_buys())

chk("1.52.0", "a caravan and a bandit line are spoken in the language in force when they are offered, not the one the campaign loaded in",
    a_dialogue_line_is_spoken_in_the_language_in_force_when_it_is_offered())


def a_translation_file_is_read_exactly_as_it_stands():
    tongue = S['Tongue.cs']
    read = method_body(tongue, "private static Dictionary<string, string> Read(string path)")
    return ("var doc = new XmlDocument { XmlResolver = null };" in read
            and "new XmlDocument()" not in ALL
            and ALL.count("new XmlDocument") == 1)


chk("1.52.4", "a translation file is read exactly as it stands, so one naming something outside itself never sends TradeLord fetching it while the game waits",
    a_translation_file_is_read_exactly_as_it_stands())


def a_pass_that_can_spend_nothing_says_what_is_holding_your_purse():
    t = S['Trading.cs']
    buy = buy_pass()
    said = method_body(t, "private static void SayWhatHoldsYourPurse")
    return ("else { tally.Note(Block.BudgetSpent); SayWhatHoldsYourPurse(pass); }" in buy
            and t.count("SayWhatHoldsYourPurse(") == 2
            and ordered(said, "int purse = Hero.MainHero.Gold + pass.Books.Purse(pass.Sim);",
                        "int held = GoldHeldBack(), flat = Options.Current.GoldReserve;",
                        "Log.Repeatable(")
            and "Options.Current.KeepWageDays" in said
            and "Options.Current.MaxSpendPerVisit" in said
            and "pass.Books.PaidOut(pass.Sim)" in said)


chk("1.53.0", "a buying pass with nothing left to spend writes your purse, what is held back and what holds it to the log, rather than only that the budget was spent",
    a_pass_that_can_spend_nothing_says_what_is_holding_your_purse())


def every_quest_that_waits_on_a_good_you_carry_is_read():
    named = between(S['Encounters.cs'], "private static readonly (Type quest, string wanted, string many)[] Named",
                    "private static readonly (Type quest, string goodId, string many)[] NamedGoods")
    wanted = (("HeadmanNeedsToDeliverAHerdIssueQuest", "_herdTypeToDeliver", "_animalCountToDeliver"),
              ("HeadmanVillageNeedsDraughtAnimalsIssueQuest", "_requestedAnimal", "_requestedAnimalAmount"),
              ("LordNeedsHorsesIssueQuest", "_mountObjectToBeDelivered", "_numMountsToBeDelivered"),
              ("ArtisanOverpricedGoodsIssueQuest", "_requestedTradeGood", "_requestedTradeGoodAmount"),
              ("ArtisanCantSellProductsAtAFairPriceIssueQuest", "_rawMaterialsToBeDelivered",
               "_amountOfRawGoodsToBeDelivered"),
              ("GangLeaderNeedsToOffloadStolenGoodsIssueQuest", "_stolenTradeGood", "_stolenTradeGoodAmount"),
              ("LandLordTheArtOfTheTradeIssueQuest", "_selectedItemObject", "_selectedItemObjectCount"))
    goods = between(S['Encounters.cs'], "private static readonly (Type quest, string goodId, string many)[] NamedGoods",
                    "private static readonly (Type quest, string many)[] NamedHerds")
    herds = between(S['Encounters.cs'], "private static readonly (Type quest, string many)[] NamedHerds",
                    "private static (Type quest, FieldInfo wanted, FieldInfo many)[] _read;")
    byGood = (("ArmyNeedsSuppliesIssueQuest", '"grain"', "_requestedGrainAmount"),
              ("ArmyNeedsSuppliesIssueQuest", '"wine"', "_requestedWineAmount"),
              ("HeadmanNeedsGrainIssueQuest", '"grain"', "_neededGrainAmount"))
    return (all(quest in named and item in named and count in named for quest, item, count in wanted)
            and named.count("typeof(") == len(wanted)
            and all(quest in goods and item in goods and count in goods for quest, item, count in byGood)
            and goods.count("typeof(") == len(byGood)
            and all(COMPAT.count('"' + field + '"') == 1 for _, item, count in wanted for field in (item, count))
            and all(COMPAT.count('"' + count + '"') == 1 for _, _item, count in byGood)
            and "ArmyNeedsSuppliesIssueQuest" in herds
            and "_requestedLiveStockAmount" in herds
            and herds.count("typeof(") == 1
            and COMPAT.count('"_requestedLiveStockAmount"') == 1)


def a_quest_holds_back_any_good_it_waits_on_not_only_an_animal():
    sell = sell_rule()
    return ("if (good.HasHorse)\n            {\n                int promised" not in S['Rules.cs']
            and ordered(sell,
                        "if (game.Locked()) { said.Why = Block.Locked; return said; }",
                        "int promised = DrawKeepBack(amount, facts.AwaitedHeld, out bool owed);",
                        "if (amount <= said.KeepCount) { said.Why = Block.QuestGoods; return said; }",
                        "if (Listed(s.AlwaysSet, good)) { said.Allowed = true; return said; }")
            and "A_trade_good_a_quest_is_waiting_on_is_kept_back_the_same_as_an_animal" in SELLTESTS
            and "An_always_sell_entry_cannot_release_a_trade_good_a_quest_is_waiting_on" in SELLTESTS)


def the_per_item_caps_say_which_cap_rather_than_the_purse():
    phrase = method_body(S['Reasons.cs'], "internal static TextObject Phrase")
    return (ordered(phrase,
                    "case Block.BudgetSpent:",
                    'return Tongue.Text("{=TL43}your purse or spending caps are spent");',
                    "case Block.ItemCountCap:",
                    "case Block.ItemValueCap:",
                    'return Tongue.Text("{=TL391}')
            and 'TL391' in strings_declared())


def the_map_marker_leaves_out_a_market_it_would_not_trade_in():
    marker = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    return (ordered(marker, "if (s == party.CurrentSettlement) continue;",
                    "if (TradeActionBehavior.StillTheSameArrival(s)) continue;",
                    "if (!TradeActionBehavior.IsMarket(s)) continue;")
            and "Arrivals.StillTheSame(settlement?.StringId, _lastArrivalAt, _tookToTheRoad)" in
                between(S['Trading.cs'], "internal static bool StillTheSameArrival", ";"))


def a_herd_it_cannot_thin_says_what_it_will_not_give_up():
    relief = method_body(S['Trading.cs'], "public static void ExecuteHerdRelief")
    said = method_body(S['Drove.cs'], "internal static void SayWhatItWillNotGiveUp")
    return ("if (stable.Count == 0) { Drove.SayWhatItWillNotGiveUp(mine, shed, settlement); return; }" in relief
            and S['Trading.cs'].count("Drove.SayWhatItWillNotGiveUp(") == 1
            and S['Drove.cs'].count("internal static void SayWhatItWillNotGiveUp(") == 1
            and "if (ShedRank(it) >= 0) continue;" in said
            and "ordinary cargo" in said
            and "Log.Repeatable(" in said)


chk("1.55.0", "every quest that waits on a good you carry is read, the ones that name the good and the ones that only say how much of it, and the game version fit tool holds every field they are read from",
    every_quest_that_waits_on_a_good_you_carry_is_read())
chk("1.55.0", "a good a quest is waiting on is held back whatever it is, past the always-sell list, not only where it is an animal",
    a_quest_holds_back_any_good_it_waits_on_not_only_an_animal())
chk("1.55.0", "the per-item buy caps say it is those caps that stopped a good, rather than sharing the purse's words",
    the_per_item_caps_say_which_cap_rather_than_the_purse())
chk("1.55.0", "the market marked on your map is never one TradeLord would leave alone as the same arrival",
    the_map_marker_leaves_out_a_market_it_would_not_trade_in())
chk("1.55.0", "a herd it cannot thin says what it is holding back rather than falling silent",
    a_herd_it_cannot_thin_says_what_it_will_not_give_up())


def an_army_waiting_on_livestock_holds_back_whatever_herd_you_carry():
    rule = method_body(S['Rules.cs'], "internal static Dictionary<string, int> LivestockKeep")
    kept = method_body(S['Policy.cs'], "internal static Dictionary<ItemObject, int> KeptBack")
    promised = method_body(S['Encounters.cs'], "internal static Dictionary<ItemObject, int> Promised")
    return (ordered(rule, "if (held.Amount <= 0 || !held.Good.IsLivestock) continue;",
                    "int take = Math.Min(held.Amount, wanted);",
                    "keep[held.Good.Id] = had + take;", "wanted -= take;")
            and "MobileParty" not in S['Rules.cs'] and "ItemRoster" not in S['Rules.cs']
            and ordered(kept, "awaited = Errands.Promised(out int anyLivestock);",
                        "if (awaited == null) return keep;",
                        "Named(TradeRules.LivestockKeep(carried, anyLivestock), byId)",
                        "awaited[owed.Key] = had + owed.Value;")
            and "if (_readHerds[i].many.GetValue(quest) is int owed && owed > 0) anyLivestock += owed;" in promised
            and "An_army_waiting_on_livestock_reserves_it_across_whatever_herd_you_carry" in FOODTESTS
            and "A_livestock_reserve_never_claims_more_than_the_herd_you_are_carrying" in FOODTESTS
            and "One_good_in_two_lots_gives_the_livestock_reserve_both_lots" in FOODTESTS)


chk("1.57.0", "an army waiting on livestock holds back that many head of whatever herd you carry, since the quest counts any livestock rather than naming one",
    an_army_waiting_on_livestock_holds_back_whatever_herd_you_carry())


def a_herd_is_never_food_anywhere_in_the_source():
    value = method_body(S['Rules.cs'], "internal static int FoodValue")
    stored = between(S['Rules.cs'], "internal static bool IsStorableFood", ";")
    larder = pass_body("public static void ExecuteResupply")
    return ("MeatCount" not in ALL
            and "MeatCount" not in M
            and "MeatCount" not in FOODTESTS
            and "IsLivestock" not in value
            and "if (good.Id == null || good.HasHorse) return 0;" in value
            and "IsLivestock" not in food_rule()
            and "good.IsFood && !good.HasHorse" in stored
            and "TradePolicy.IsStorableFood(it)" in larder
            and "A_herd_the_game_calls_food_still_feeds_nobody" in FOODTESTS
            and "A_bag_of_nothing_but_livestock_keeps_no_food_back_at_all" in FOODTESTS
            and "The_days_of_supply_are_met_from_food_alone_and_never_topped_up_with_a_herd" in FOODTESTS
            and "A_herd_never_reaches_the_food_reserve_whatever_else_is_in_the_bags" in FOODTESTS
            and "Livestock_is_never_eaten_at_all_however_cheap_it_is" in FOODTESTS)


chk("1.58.0", "a herd is food to nothing in this repository: the meat count is gone from the source, the food value and the food floor name no livestock, the larder takes nothing with a horse component, and five tests hold it there",
    a_herd_is_never_food_anywhere_in_the_source())


def the_settling_notice_obeys_the_silence_switch_on_both_routes():
    opened = method_body(S['Trading.cs'], "internal static Pass Open")
    road = method_body(S['Trading.cs'], "public static void ExecuteRoadTrade")
    settling = method_body(S['Trading.cs'], "private static bool StillSettling")
    said = "both as you enter a market and when you meet a caravan or a party of villagers on the road"
    return ("if (!MarketOpen(site, TradeActionBehavior.Muted(quiet))) return null;" in opened
            and "if (StillSettling(Muted(automated: true))) return;" in road
            and "MarketOpen(site, quiet)" not in S['Trading.cs']
            and "StillSettling(quiet: false)" not in S['Trading.cs']
            and "private static bool Muted(bool automated) => automated && Options.Current.QuietAutomation;"
                in S['Trading.cs']
            and ordered(settling, "if (!quiet)", '{=TL18}', "Notices.Say(msg);")
            and said in re.search(r'\{=TL349\}([^"]*)"', M).group(1)
            and said in spoken(ENGLISH).get('TL349', ''))


chk("1.58.1", "the market is still settling notice is silenced by Silence trade messages and by nothing else, on entering a market as well as on the road",
    the_settling_notice_obeys_the_silence_switch_on_both_routes())

def a_held_back_purse_names_the_settings_that_hold_it_and_not_the_spending_cap():
    warn = method_body(S['Trading.cs'], "private static bool WarnPurseBelowReserve")
    legend = method_body(S['Panel.cs'], "private static string NothingHereYouCouldBuy")
    en = spoken(ENGLISH)
    flat, wages = en['TL234'], en['TL266']
    split = ("int held = TradeActionBehavior.GoldHeldBack(), flat = Options.Current.GoldReserve;" in legend
             and "int held = GoldHeldBack(), flat = Options.Current.GoldReserve;" in warn
             and warn.count("held > flat") == 1 and legend.count("held > flat") == 1
             and "Spendable(" not in warn and "MaxSpendPerVisit" not in warn
             and "TradeActionBehavior.PurseForAVisit() > 0" in
                 method_body(S['Panel.cs'], "private void Refresh"))
    fed = ('msg.SetTextVariable("RESERVE", held);' in warn
           and 'msg.SetTextVariable("FLAT", flat);' in warn
           and 'msg.SetTextVariable("WAGES", held - flat);' in warn
           and 'line.SetTextVariable("RESERVE", held.ToString("N0"));' in legend
           and 'line.SetTextVariable("FLAT", flat.ToString("N0"));' in legend
           and 'line.SetTextVariable("WAGES", (held - flat).ToString("N0"));' in legend)
    named = all(flat in en[one] and wages in en[one] for one in ('TL392', 'TL393'))
    plain = all(wages not in en[one] and flat in en[one] for one in ('TL92', 'TL377'))
    spelled = all(one in strings_declared() for one in ('TL92', 'TL377', 'TL392', 'TL393'))
    return split and fed and named and plain and spelled


chk("1.58.2", "a purse held below what TradeLord keeps back names Gold reserve on its own, or splits the figure between Gold reserve and Keep gold for days of wages when both hold some, and the buy cap per visit never sets it off",
    a_held_back_purse_names_the_settings_that_hold_it_and_not_the_spending_cap())



def a_village_can_carry_the_map_marker_when_the_trade_pool_holds_villages():
    marker = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    pool = between(S['Trading.cs'], "internal static bool IsMarket(Settlement s) =>", ";")
    en = spoken(ENGLISH)
    walks = (ordered(marker, "foreach (Settlement s in Settlement.All)",
                     "SettlementComponent market = s.SettlementComponent;",
                     "if (market == null) continue;",
                     "if (!TradeActionBehavior.IsMarket(s)) continue;",
                     "if (LedgerBehavior.UnderAttack(s) || LedgerBehavior.VillageShut(s)) { how.Shut++; continue; }",
                     "float cap = LedgerBehavior.TravelCeiling(s);",
                     "if (TradeMath.PerDay(gold, ride) <= bar) break;",
                     "WhatItWouldFetch(s, market, party, cargo, gold, null)")
             and "Town.AllTowns" not in marker
             and "town.Gold" not in marker)
    gated = ("s.IsVillage && Options.Current.TradeWithVillages" in pool
             and "s.IsTown && Options.Current.TradeWithTowns" in pool)
    ceiling = ("if (village && vcap > 0f && (cap <= 0f || vcap < cap)) cap = vcap;" in
               method_body(S['Ranking.cs'], "internal static float Ceiling")
               and "MarketRank.Ceiling(s.IsVillage, Options.Current)" in
                   between(S['Ledger.cs'], "internal static float TravelCeiling(Settlement s) =>", ";"))
    said = "A village is only ever marked while Trade with villages is on." in en['TL345']
    swept = "Town.AllTowns" not in S['Trading.cs']
    return walks and gated and ceiling and said and swept


chk("1.59.0", "a village carries the map marker like any other market, held to the Trade with villages switch, shut out while it is raided or rebuilding, and kept to the tighter of the two travel ceilings",
    a_village_can_carry_the_map_marker_when_the_trade_pool_holds_villages())



def the_release_refuses_a_commit_subject_that_names_another_version():
    step = between(WORKFLOW, "- name: Resolve version from SubModule.xml", "- name: Source checks")
    return (ordered(step,
                    'MODVER=$(sed -n \'s/.*<Version value="\\(v[0-9][^"]*\\)".*/\\1/p\' TradeLord/SubModule.xml)',
                    'SUBJECT=$(git log -1 --format=%s "$GITHUB_SHA")',
                    '"[no release]"*)',
                    "SAID=$(printf '%s' \"$SUBJECT\" | sed -n 's/^\\[\\([0-9][0-9.]*\\)\\].*/\\1/p')",
                    'if [ -z "$SAID" ]; then',
                    'the commit subject carries no version in square brackets and is not marked [no release]:',
                    'if [ "$SAID" != "${MODVER#v}" ]; then',
                    'they have to agree',
                    'echo "version=$MODVER" >> "$GITHUB_OUTPUT"')
            and step.count("exit 1") == 4
            and "Start every commit subject with the version it ships in in square brackets" in RULES
            and "the release workflow refuses to publish while any of them disagree" in RULES)


chk("1.59.0", "the release refuses a commit whose subject names a version other than the one the source declares, and refuses one that names none at all without being marked no release",
    the_release_refuses_a_commit_subject_that_names_another_version())



def the_forecast_reads_the_world_only_while_live_world_prices_are_on():
    return ("internal static bool On => Options.Current.MarketForecast && Options.Current.Omniscient;"
                in S['Forecast.cs']
            and option_default('MarketForecast') == 'true'
            and "_o.MarketForecast" in M
            and EVER_SHIPPED.get('MarketForecast') == 'bool'
            and all("if (!On" in method_body(S['Forecast.cs'], sig)
                    for sig in ("internal static int UnitsLanding",
                                "internal static int WorthLanding",
                                "internal static string WillMake")))

def a_caravan_already_in_the_market_is_not_counted_twice():
    road = method_body(S['Forecast.cs'], "private static void ReadWhatIsOnTheRoad")
    return (ordered(road,
                    "if (!caravan && !party.IsVillager) continue;",
                    "if (party.CurrentSettlement != null) continue;",
                    "Settlement bound = party.TargetSettlement;",
                    "TradeMath.EtaDays(",
                    "if (item == null || amount <= 0 || !TradePolicy.Priced(item)) continue;")
            and "CurrentSettlement" not in method_body(
                    S['Forecast.cs'], "private static void ReadWhatTheShopsWillMake"))

def what_lands_after_you_arrive_is_not_counted():
    units = method_body(S['Projection.cs'], "internal static int UnitsLanding")
    worth = method_body(S['Projection.cs'], "internal static int WorthLanding")
    purse = method_body(S['Projection.cs'], "internal static int PurseLanding")
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
    return ("if (!TradeMath.LandsInTime(landing.Days, withinDays)) continue;" in units
            and "if (!TradeMath.LandsInTime(landing.Days, withinDays)) continue;" in worth
            and "if (!TradeMath.LandsInTime(spending.Days, withinDays)) continue;" in purse
            and "int landedAtBuyTown = Forecast.WorthShiftAsItHasHeld(from, item, toBuy);" in scan
            and "int landedAtSellTown = Forecast.WorthShiftAsItHasHeld(to, item, days);" in scan
            and "Forecast.UnitsLanding(from, item, toBuy)" in scan
            and "A_load_still_on_the_road_past_the_window_is_left_out" in PROJECTIONTESTS
            and "The_purse_on_the_road_adds_up_within_the_window" in PROJECTIONTESTS)

def a_workshop_run_moves_the_price_of_its_kind_and_never_the_stock_of_one_good():
    shops = method_body(S['Forecast.cs'], "private static void ReadWhatTheShopsWillMake")
    return ("Note(site, null, category, count," in shops
            and "if (landing.Item != item) continue;" in
                method_body(S['Projection.cs'], "internal static int UnitsLanding")
            and "if (landing.Category != category) continue;" in
                method_body(S['Projection.cs'], "internal static int WorthLanding")
            and "Worth_landing_goes_by_the_kind_of_good_not_the_good" in PROJECTIONTESTS
            and "TradeRules.InputsHeld(_needs, held)" in S['Forecast.cs']
            and "int pick = TradeRules.RunsSoonest(ready);" in S['Forecast.cs'])

def the_walk_starts_from_what_the_market_will_hold_when_you_get_there():
    shelf = method_body(S['Market.cs'],
        "internal Shelf(Settlement site, EquipmentElement stocked, bool selling, int quoted, bool projecting, int landed = 0)")
    rung = method_body(S['Market.cs'], "private static Ladder Rung")
    return ("_inStoreValue = TradeMath.ShelfAfterLanding(data.InStoreValue, landed);" in shelf
            and "var key = (site.StringId, item.StringId, selling, landed);" in rung
            and "Dictionary<(string site, string item, bool selling, int landed), Ladder>" in S['Market.cs']
            and "int landedAtBuyTown, int landedAtSellTown" in
                method_body(S['Market.cs'], "internal static RouteQuote Walk"))

def the_forecast_is_read_once_an_hour_and_again_when_a_setting_moves():
    build = method_body(S['Forecast.cs'], "private static void Build")
    return (ordered(build,
                    "if (Freshness.Fresh(ref _readStamp)) return;",
                    "_landing.Clear();",
                    'Guard.Run("Forecast.Caravans", ReadWhatIsOnTheRoad);',
                    'Guard.Run("Forecast.Workshops", ReadWhatTheShopsWillMake);')
            and 'Guard.Run("GameEnd.Forecast", Forecast.Forget);' in S['SubModule.cs'])

def the_panel_says_what_each_workshop_will_make_next():
    return ("Forecast.WillMake(w)," in method_body(S['Panel.cs'], "private void RefreshWorkshops")
            and 'Tongue.Text("{=TL395} and what each will make next")' in S['Panel.cs']
            and "{=TL394}" in S['Panel.cs']
            and '[DataSourceProperty] public string Makes { get; }' in S['Panel.cs']
            and 'Text="@Makes"' in PREFAB
            and all(i in strings_declared() for i in ("TL394", "TL395", "TL279", "TL396")))


chk("1.60.0", "goods on the road and in the workshops are read only while live world prices are on, and the switch ships on",
    the_forecast_reads_the_world_only_while_live_world_prices_are_on())
chk("1.60.0", "a caravan standing in a market has already unloaded, so only the ones still on the road are counted",
    a_caravan_already_in_the_market_is_not_counted_twice())
chk("1.60.0", "cargo counts against a market only when it lands before you do, each end to its own travel time",
    what_lands_after_you_arrive_is_not_counted())
chk("1.60.0", "a workshop run moves the price of the kind of good it makes and never the stock of one good",
    a_workshop_run_moves_the_price_of_its_kind_and_never_the_stock_of_one_good())
chk("1.60.0", "the unit-by-unit walk starts from what the market will hold when you arrive, and a ladder is kept per landing",
    the_walk_starts_from_what_the_market_will_hold_when_you_get_there())
chk("1.60.0", "what is on the road is read once an hour, again when a setting moves, and forgotten with the campaign",
    the_forecast_is_read_once_an_hour_and_again_when_a_setting_moves())
chk("1.60.0", "the workshop list says what each one will make next, and the panel legend says the forecast is counted",
    the_panel_says_what_each_workshop_will_make_next())



def a_purse_on_its_way_is_spent_on_what_is_cheap_at_that_market():
    leaving = method_body(S['Forecast.cs'], "internal static int WorthLeaving")
    picked = method_body(S['Forecast.cs'], "private static Dictionary<string, float> WhatATraderWouldPickAt")
    road = method_body(S['Forecast.cs'], "private static void ReadWhatIsOnTheRoad")
    across = method_body(S['Projection.cs'], "internal static float PullAcross")
    share = method_body(S['Projection.cs'], "internal static int WorthLeaving")
    return (ordered(leaving,
                    "int purse = Projection.PurseLanding(coming, withinDays);",
                    "if (!PullAt(site, out Dictionary<string, float> pull, out float across)) return 0;",
                    "return Projection.WorthLeaving(purse, pull, across, item.ItemCategory.StringId);")
            and ordered(share,
                        "if (!pull.TryGetValue(category, out float mine)) return 0;",
                        "return TradeMath.ShareOfAPurse(purse, mine, across);")
            and "foreach (float one in pull.Values) total += one;" in across
            and "NoteAPurse(bound, party.PartyTradeGold, days);" in road
            and "TradeMath.PullOfAPrice(town.MarketData.GetPriceFactor(category))" in picked
            and "Town town = site.IsTown ? site.Town : null;" in picked
            and "What_leaves_the_shelf_is_the_purse_share_the_kind_of_good_pulls" in PROJECTIONTESTS)

def a_purse_is_never_counted_twice_and_never_beyond_what_it_holds():
    shift = method_body(S['Forecast.cs'], "internal static int WorthShift")
    share = method_body(S['TradeMath.cs'], "public static int ShareOfAPurse")
    pull = method_body(S['TradeMath.cs'], "public static float PullOfAPrice")
    return ("TradeMath.WorthShift(WorthLanding(site, item, withinDays)," in shift
            and "WorthLeaving(site, item, withinDays));" in shift
            and "if (purse <= 0 || pull <= 0f || pullAcrossTheMarket <= 0f) return 0;" in share
            and "(double)purse * pull / pullAcrossTheMarket" in share
            and "float pull = 1f - Finite(priceFactor, 1f);" in pull
            and "return pull < 0f ? 0f : (pull > 1f ? 1f : pull);" in pull)

def what_a_market_will_hold_nets_the_buying_off_against_the_landing():
    return ("public static int WorthShift(int landing, int leaving)" in S['TradeMath.cs']
            and "long shift = (long)landing - (leaving < 0 ? 0 : leaving);" in
                method_body(S['TradeMath.cs'], "public static int WorthShift")
            and "_inStoreValue = TradeMath.ShelfAfterLanding(data.InStoreValue, landed);" in S['Market.cs'])


chk("1.61.0", "the gold a caravan brings is aimed at the goods that are cheap at that market, weighted by how cheap each one is",
    a_purse_on_its_way_is_spent_on_what_is_cheap_at_that_market())
chk("1.61.0", "a purse is split across a market rather than counted whole against every good, and never read as more than it holds",
    a_purse_is_never_counted_twice_and_never_beyond_what_it_holds())
chk("1.61.0", "what a market will hold when you get there is what lands there less what the purses take off the shelf",
    what_a_market_will_hold_nets_the_buying_off_against_the_landing())



def a_tooltip_prices_a_market_as_it_will_be_when_you_get_there():
    shift = method_body(S['TooltipPatches.cs'], "private static void AsTheyWillBe")
    picked = between(S['TooltipPatches.cs'], "ScreenMarkets.Prime();", "private static void KeepTheBest")
    first = method_body(S['Market.cs'], "internal static int FirstUnit")
    return (ordered(shift,
                    "if (!Forecast.On || markets == null || markets.Count == 0) return;",
                    "float days = Travel.EstimateDaysFromParty(town);",
                    "Bulk.FirstUnit(town, item, selling, price,",
                    "Forecast.WorthShiftAsItHasHeld(town, item, days)));",
                    "markets.Sort(")
            and "AsTheyWillBe(item, sells, selling: true);" in picked
            and "AsTheyWillBe(item, buys, selling: false);" in picked
            and "if (landed == 0 || site == null || item == null) return quoted;" in first
            and "return rung.Walkable ? TradeMath.ForecastWithin(quoted, rung.At(0)) : quoted;" in first
            and "a price in a tooltip" in spoken(ENGLISH)['TL396'])


def the_tooltip_picks_its_five_after_the_forecast_has_priced_them():
    tip = S['TooltipPatches.cs']
    markets = method_body(tip, "private static (ItemObject item, List<(Settlement town, int price)> sells,")
    keep = method_body(tip, "private static void KeepTheBest")
    append = method_body(tip, "internal static void Append(ItemMenuVM vm, ItemVM itemVm)")
    return ("ledger.TopSell(item, MarketRank.TopCacheSize);" in markets
            and "ledger.TopBuy(item, MarketRank.TopCacheSize);" in markets
            and ordered(markets,
                        "ledger.TopSell(item, MarketRank.TopCacheSize);",
                        "AsTheyWillBe(item, sells, selling: true);",
                        "KeepTheBest(sells);")
            and "if (markets.Count > TopN) markets.RemoveRange(TopN, markets.Count - TopN);" in keep
            and "AsTheyWillBe" not in append
            and "TopSell(item, TopN)" not in tip
            and "TopBuy(item, TopN)" not in tip)


chk("1.81.9", "the item tooltip picks the five markets it shows after the forecast has priced every one it ranked, so a market the forecast marks down loses its place to a better one instead of keeping it",
    the_tooltip_picks_its_five_after_the_forecast_has_priced_them())

chk("1.62.0", "a market in a tooltip is priced as it will be when you get there, through the same forecast the ledger panel reads, and the five are ordered on those prices",
    a_tooltip_prices_a_market_as_it_will_be_when_you_get_there())



def a_quantity_that_leans_on_goods_still_on_the_road_says_so():
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
    rule = method_body(S['TradeMath.cs'], "public static bool StillComing")
    return ("int shelf = 0, onTheShelfNow = int.MaxValue;" in scan
            and "StillComing = TradeMath.StillComing(q.Units, onTheShelfNow)" in scan
            and "public bool StillComing;" in S['Ledger.cs']
            and "units > (onTheShelfNow < 0 ? 0 : onTheShelfNow);" in rule
            and '"x" + _route.Quantity + (_route.StillComing ? "!" : "");' in S['Panel.cs']
            and "{=TL397}" in S['Panel.cs']
            and "Qty! = part of that amount is still on the road" in spoken(ENGLISH)['TL397']
            and "Qty!" in README)


chk("1.62.1", "a route whose quantity leans on goods still on the road is marked on the panel, and the mark is explained where the other marks are",
    a_quantity_that_leans_on_goods_still_on_the_road_says_so())



def a_version_already_out_is_never_claimed_a_second_time():
    step = between(WORKFLOW, "- name: Publish release", "could not publish")
    return (ordered(step,
                    'SUBJECT=$(git log -1 --format=%s "$GITHUB_SHA")',
                    '"[no release]"*)',
                    "this commit is marked [no release], so it publishes nothing",
                    'gh release view "$VERSION" --json tagName',
                    '--json targetCommitish -q .targetCommitish',
                    'if [ "$AT" = "$GITHUB_SHA" ]; then',
                    "was published by this very commit, so there is nothing left to do",
                    "is already published, from commit $AT, and this commit claims it again:",
                    "raise the version in TradeLord/SubModule.xml, or mark this commit [no release]")
            and "nothing to publish for this push" not in step
            and "A version is published once" in RULES)


chk("1.62.2", "a commit claiming a version that is already out is refused rather than quietly publishing nothing, and a no-release commit says so for itself",
    a_version_already_out_is_never_claimed_a_second_time())



def the_changelog_is_held_against_what_was_actually_published():
    step = between(WORKFLOW, "- name: Changelog against the releases", "- name: Codec tests")
    return ('python3 tools/released.py "${{ steps.ver.outputs.version }}"' in step
            and "GH_TOKEN: ${{ github.token }}" in step
            and ordered(WORKFLOW, "- name: Resolve version from SubModule.xml",
                        "- name: Changelog against the releases", "- name: Publish release")
            and "from nexus_changelog import sections" in RELEASED
            and "was published and the changelog carries no section for it" in RELEASED
            and "has a changelog section and was never published" in RELEASED
            and "says one thing in the changelog and another in its release notes" in RELEASED
            and "was published with no file attached" in RELEASED
            and "is still a draft release" in RELEASED
            and "  skipped  the releases could not be read" in RELEASED
            and "version == shipping" in RELEASED)

def an_outstanding_disagreement_is_named_and_cannot_linger_once_settled():
    listed = between(RELEASED, "OUTSTANDING = {", "}")
    named = re.findall(r"'\d+\.\d+(?:\.\d+)?': '", listed)
    return ("OUTSTANDING = {" in RELEASED
            and len(named) == listed.count("': '")
            and "is listed as an outstanding disagreement and no longer disagrees" in RELEASED
            and "from before this check existed" in RELEASED
            and "The entries in a version section and the bullet points in that version's commit body say "
                "the same thing in the same words" in RULES)


chk("1.62.2", "the changelog is held against every release that was actually published, before anything else is published",
    the_changelog_is_held_against_what_was_actually_published())
chk("1.62.2", "a disagreement that predates the check is named one by one, and a settled one has to leave the list",
    an_outstanding_disagreement_is_named_and_cannot_linger_once_settled())



GLOSSARY = {
    'Gold reserve': {'T\u00fcrk\u00e7e': r'alt[\u0131i]n rezerv',
                     '\u0420\u0443\u0441\u0441\u043a\u0438\u0439': r'\u0437\u0430\u043f\u0430\u0441\w* \u0437\u043e\u043b\u043e\u0442\u0430',
                     '\u7b80\u4f53\u4e2d\u6587': r'\u91d1\u94b1\u50a8\u5907'},
    'Live world prices': {'T\u00fcrk\u00e7e': r'canl[\u0131i] d\u00fcnya fiyat',
                          '\u0420\u0443\u0441\u0441\u043a\u0438\u0439': r'\u0436\u0438\u0432\u044b\w* \u0446\u0435\u043d\w* \u043c\u0438\u0440\u0430',
                          '\u7b80\u4f53\u4e2d\u6587': r'\u5b9e\u65f6\u4e16\u754c\u4ef7\u683c'},
    'travel ceiling': {'T\u00fcrk\u00e7e': r'yol s\u00fcresi s[\u0131i]n[\u0131i]r',
                       '\u0420\u0443\u0441\u0441\u043a\u0438\u0439': r'\u043f\u0440\u0435\u0434\u0435\u043b\w* \u043f\u0443\u0442\u0438',
                       '\u7b80\u4f53\u4e2d\u6587': r'\u884c\u7a0b\u4e0a\u9650'},
    'Trade with towns': {'T\u00fcrk\u00e7e': r'\u015fehirlerle ticaret',
                         '\u0420\u0443\u0441\u0441\u043a\u0438\u0439': r'\u0442\u043e\u0440\u0433\u043e\u0432\u0430\u0442\u044c \u0441 \u0433\u043e\u0440\u043e\u0434\u0430\u043c\u0438',
                         '\u7b80\u4f53\u4e2d\u6587': r'\u4e0e\u57ce\u9547\u4ea4\u6613'},
    'Trade with villages': {'T\u00fcrk\u00e7e': r'k\u00f6ylerle ticaret',
                            '\u0420\u0443\u0441\u0441\u043a\u0438\u0439': r'\u0442\u043e\u0440\u0433\u043e\u0432\u0430\u0442\u044c \u0441 \u0434\u0435\u0440\u0435\u0432\u043d\u044f\u043c\u0438',
                            '\u7b80\u4f53\u4e2d\u6587': r'\u4e0e\u6751\u5e84\u4ea4\u6613'},
    'haul animal': {'T\u00fcrk\u00e7e': r'y\u00fck hayvan',
                    '\u0420\u0443\u0441\u0441\u043a\u0438\u0439': r'\u0432\u044c\u044e\u0447\u043d',
                    '\u7b80\u4f53\u4e2d\u6587': r'\u9a6e\u517d'},
    'ledger': {'T\u00fcrk\u00e7e': r'defter',
               '\u0420\u0443\u0441\u0441\u043a\u0438\u0439': r'\u043a\u043d\u0438\u0433',
               '\u7b80\u4f53\u4e2d\u6587': r'\u8d26\u7c3f'},
}

def one_word_for_one_thing_in_every_language():
    en = spoken(ENGLISH)
    astray = []
    for term, byLanguage in GLOSSARY.items():
        lines = [k for k, said in en.items() if term.lower() in said.lower()]
        if not lines:
            astray.append('no English line says ' + term)
        for tag, path in TRANSLATIONS.items():
            said = spoken(path)
            wanted = byLanguage.get(tag)
            if not wanted:
                astray.append(term + ' has no agreed word in ' + tag)
                continue
            for k in lines:
                if not re.search(wanted, said.get(k, ''), re.I):
                    astray.append(tag + ' ' + k + ' says ' + term + ' some other way')
    return astray == []


chk("1.62.2", "a term the settings screen names is translated the same way in every line that names it",
    one_word_for_one_thing_in_every_language())



def the_forecast_is_scored_against_the_market_it_predicted():
    h = S['Hindsight.cs']
    noted = method_body(h, "private static void Noted")
    return ("internal static bool Writing => Options.Current.ExtendedDebugLogging;" in h
            and "internal static bool On => Forecast.On;" in h
            and "Writing" not in method_body(h, "internal static void Note")
            and "Writing" not in method_body(h, "internal static void Score")
            and option_default('ExtendedDebugLogging') == 'true'
            and EVER_SHIPPED.get('ExtendedDebugLogging') == 'bool'
            and "_o.ExtendedDebugLogging" in M
            and all(i in strings_declared() for i in ('TL457', 'TL458'))
            and "Hindsight.Note(best);" in method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
            and "Hindsight.Score(settlement);" in method_body(S['Trading.cs'], "private void OnSettlementEntered")
            and 'Guard.Run("GameEnd.Hindsight", Hindsight.Forget);' in S['SubModule.cs']
            and "Forecast.UnitsLanding(site, item, withinDays)" in noted
            and "Forecast.WorthShift(site, item, withinDays)" in noted
            and "LedgerBehavior.StockOf(site, item)" in noted
            and "WorthOnTheShelf(site, item)" in noted)

def the_score_works_out_how_far_off_it_was_in_the_layer_the_tests_reach():
    weighed = method_body(S['Scoring.cs'], "internal static Outcome Weigh")
    written = method_body(S['Hindsight.cs'], "private static void Written")
    reads = ('TradeMath.MissedBy(', 'TradeMath.OffByShare(')
    return (all(one in weighed for one in reads)
            and all(one.rstrip('(') in MATHTESTS for one in reads)
            and 'TradeMath.MeanOf(' in written
            and 'Scoring.TooOldToSay(kept.WithinDays, kept.AtHours, now, out float since)' in written
            and 'TradeMath.DaysSince(' in method_body(S['Scoring.cs'], "internal static bool TooOldToSay")
            and 'Outcome how = Scoring.Weigh(kept.StockSaid, kept.StockThen,' in written
            and 'TradeMath.NoShareToGive' in MATHTESTS
            and 'public static float OffByShare' in S['TradeMath.cs']
            and 'if (said == 0) return NoShareToGive;' in S['TradeMath.cs']
            and 'How_far_the_worth_figure_was_off_is_scored_as_a_share_of_what_it_said' in SCORINGTESTS
            and 'A_worth_figure_of_nothing_cannot_be_held_to_anything' in SCORINGTESTS)

def a_figure_is_read_once_it_is_walked_into_and_no_more_are_kept_than_it_says():
    h = S['Hindsight.cs']
    noted = method_body(h, "private static void Noted")
    written = method_body(h, "private static void Written")
    taken = method_body(S['Scoring.cs'], "internal Dictionary<string, TRecord> TakeAt")
    return ("internal const int Most = 600;" in S['Scoring.cs']
            and "if (!_said.Holds(site.StringId, item.StringId) && _said.Full && !RoomForOneMoreFigure())" in noted
            and 'Log.Repeatable("forecast check", "full",' in noted
            and "Dictionary<string, Said> here = _said.TakeAt(site.StringId);" in written
            and ordered(taken, "_by.Remove(site);", "_count -= here.Count;", "if (_count < 0) _count = 0;")
            and "if (!Writing || scored + stale + early == 0) return;" in written
            and "no worth is kept for a kind of good here, which only a town does" in written
            and "It_fills_up_at_six_hundred" in SCORINGTESTS
            and "Walking_into_a_market_takes_every_promise_it_held_for_that_market" in SCORINGTESTS)

def the_switch_names_the_setting_it_leans_on():
    said = spoken(ENGLISH)
    return ("TradeLord.log" in said['TL458']
            and "Needs " + said['TL279'] in said['TL458']
            and "ON by default" in said['TL458']
            and said['TL457'] == "Enable extended debug logging")

def every_source_file_is_read_by_these_checks():
    import os
    onDisk = {f for f in os.listdir('src') if f.endswith('.cs')}
    return bool(onDisk) and onDisk == set(S)

def every_test_file_is_read_by_these_checks():
    import os
    onDisk = {f for f in os.listdir('tests') if f.endswith('Tests.cs')}
    return bool(onDisk) and onDisk == set(T)


chk("1.63.0", "what the forecast said a market would hold is written down with the route and read back when you walk in",
    the_forecast_is_scored_against_the_market_it_predicted())
chk("1.63.0", "how far off a figure was is worked out where a test can reach it, and tested there",
    the_score_works_out_how_far_off_it_was_in_the_layer_the_tests_reach())
chk("1.63.0", "a figure is scored once and dropped, and the count held at one time has a ceiling that says when it is reached",
    a_figure_is_read_once_it_is_walked_into_and_no_more_are_kept_than_it_says())
chk("1.63.0", "the switch that writes the score names the setting it needs by that setting's own name",
    the_switch_names_the_setting_it_leans_on())
chk("1.63.0", "every source file the mod ships is read by these checks, so a new one cannot slip past them",
    every_source_file_is_read_by_these_checks())
chk("1.68.0", "every test file the build runs is read by these checks too, so a new one cannot slip past them",
    every_test_file_is_read_by_these_checks())


def the_panel_promise_is_written_down_whatever_the_debug_switch_says():
    h = S['Hindsight.cs']
    note = method_body(h, "internal static void Note")
    score = method_body(h, "internal static void Score")
    promise = method_body(h, "private static void Promise")
    return (ordered(note, 'Guard.Run("Hindsight.Promise", () => Promise(route));', 'if (!On) return;')
            and ordered(score, 'Guard.Run("Hindsight.Kept", () => Kept(site));', 'if (!On) return;')
            and "SellPrice = route.SellPrice," in promise
            and "Units = route.Quantity," in promise
            and "Confidence = route.Confidence" in promise)

def a_promise_is_scored_against_the_price_the_market_actually_pays():
    kept = method_body(S['Hindsight.cs'], "private static void Kept")
    weighed = method_body(S['Scoring.cs'], "internal static Holding Weigh")
    return ("Priced.At(market, said.Item, MobileParty.MainParty, true)" in kept
            and ordered(kept, "Priced.At(market, said.Item, MobileParty.MainParty, true)",
                        "Holding holding = Scoring.Weigh(said.SellPrice, found, out float held);",
                        "if (holding == Holding.NoPrice)", "unpriced++;")
            and ordered(weighed, "if (found <= 0) return Holding.NoPrice;",
                        "held = TradeMath.HeldShare(promised, found);")
            and "Scoring.TooOldToSay(said.WithinDays, said.AtHours, now, out float since)" in kept
            and "TradeMath.WorthScoring(withinDays, since)" in
                method_body(S['Scoring.cs'], "internal static bool TooOldToSay")
            and "LedgerBehavior.Instance?.KeepPromiseScore(held);" in kept
            and "_bands.Add(said.Confidence, held);" in kept
            and "TradeMath.BandOf(confidence)" in method_body(S['Scoring.cs'], "internal void Add")
            and ordered(kept, "LedgerBehavior.Instance?.KeepPromiseScore(held);",
                        "LedgerBehavior.Instance?.KeepArrival(site.StringId, TradeMath.MeanOf(heldTotal, scored));",
                        "if (!Writing || scored + stale + yours + unpriced == 0) return;")
            and kept.index("LedgerBehavior.Instance?.KeepArrival(") >
                kept.rindex("heldTotal += held;")
            and "A_market_that_puts_no_price_on_a_good_scores_nothing" in SCORINGTESTS
            and "What_the_market_pays_is_scored_as_a_share_of_what_was_promised" in SCORINGTESTS)

def how_the_promise_has_held_is_kept_in_the_save_and_shown_on_the_panel():
    ledger = S['Ledger.cs']
    sync = method_body(ledger, "public override void SyncData")
    panel = method_body(S['Panel.cs'], "private static string HowThePromiseHasHeld")
    return ('dataStore.SyncData("TradeLord_PromisesScored", ref _promisesScored);' in sync
            and 'dataStore.SyncData("TradeLord_PromiseHeld", ref _promiseHeld);' in sync
            and "if (held < 0f) return;" in method_body(ledger, "internal void KeepPromiseScore")
            and "TradeMath.AddPromise(" not in method_body(ledger, "internal void KeepPromiseScore")
            and "_promises[" not in method_body(ledger, "internal void KeepPromiseScore")
            and '{=TL399}' in S['Panel.cs'] and "arrival" not in spoken(ENGLISH)['TL399']
            and "held = TradeMath.MeanOf(_promiseHeld, _promisesScored);" in
                method_body(ledger, "internal bool PromiseScore(out int scored, out float held)")
            and 'if (ledger == null || !ledger.PromiseScore(out int checked_, out float held)) return "";' in panel
            and '{=TL399}' in panel
            and 'TL399' in strings_declared()
            and 'HowThePromiseHasHeld()' in method_body(S['Panel.cs'], "private void Refresh"))

def a_promise_too_old_to_say_anything_is_dropped_rather_than_scored():
    h = S['Hindsight.cs']
    room = method_body(h, "private static bool RoomForOneMore")
    pruned = method_body(S['Scoring.cs'], "internal bool Prune(Func<TRecord, bool> stillWorthKeeping)")
    return ("one => !one.YourTradeMovedIt && !Scoring.TooOldToSay(one.WithinDays, one.AtHours, now, out _));" in room
            and "return _promised.Prune(" in room
            and ordered(pruned, "if (!stillWorthKeeping(one.Value)) past.Add(one.Key);",
                        "_count--;", "if (site.Value.Count == 0) emptied.Add(site.Key);",
                        "for (int i = 0; i < emptied.Count; i++) _by.Remove(emptied[i]);",
                        "return !Full;")
            and 'Log.Repeatable("promise check", "full",' in method_body(h, "private static void Promise")
            and all(one in MATHTESTS for one in
                    ('TradeMath.WorthScoring', 'TradeMath.HeldShare', 'TradeMath.BandOf'))
            and "A_promise_stays_worth_scoring_for_twice_the_ride_and_a_day" in SCORINGTESTS
            and "Clearing_out_old_promises_drops_a_market_it_no_longer_holds_any_for" in SCORINGTESTS
            and "A_full_store_that_can_free_nothing_says_there_is_no_room" in SCORINGTESTS)

def every_confidence_sits_in_one_band_and_the_bands_are_counted_in_one_place():
    t = S['TradeMath.cs']
    h = S['Hindsight.cs']
    band = method_body(t, "public static int BandOf")
    cuts = [float(c) for c in re.findall(r'confidence < ([\d.]+)f', band)]
    said = re.search(r'public const int Bands = (\d+);', t)
    returned = [int(n) for n in re.findall(r'return (\d+);', band)]
    for one, other in re.findall(r'\? (\d+) : (\d+)', band):
        returned += [int(one), int(other)]
    returned = sorted(set(returned))
    return (said is not None and cuts == sorted(set(cuts))
            and len(cuts) == int(said.group(1)) - 1
            and cuts[0] > 0.0 and cuts[-1] < 1.0
            and returned == list(range(int(said.group(1))))
            and "new float[TradeMath.Bands]" in S['Scoring.cs']
            and "new int[TradeMath.Bands]" in S['Scoring.cs']
            and "for (int band = TradeMath.Bands - 1; band >= 0; band--)"
                in method_body(h, "private static void Kept")
            and "Every_confidence_falls_in_one_band_and_a_dearer_one_never_falls_lower" in MATHTESTS
            and "Each_confidence_band_keeps_its_own_score" in SCORINGTESTS
            and "The_bands_are_named_by_the_confidence_they_cover" in SCORINGTESTS)


chk("1.64.0", "the promise a route makes is written down whatever the debug switch says, and scored when you walk in",
    the_panel_promise_is_written_down_whatever_the_debug_switch_says())
chk("1.64.0", "a promise is held against the price the market actually pays, and counted before anything is written",
    a_promise_is_scored_against_the_price_the_market_actually_pays())
chk("1.64.0", "how the promise has held is kept in the save and said on the panel, and says nothing until it has an arrival",
    how_the_promise_has_held_is_kept_in_the_save_and_shown_on_the_panel())
chk("1.64.0", "a promise too old to say anything about the panel is dropped rather than scored",
    a_promise_too_old_to_say_anything_is_dropped_rather_than_scored())
chk("1.64.0", "every Conf figure falls in one of the four bands the log splits them into",
    every_confidence_sits_in_one_band_and_the_bands_are_counted_in_one_place())


def the_pull_across_a_market_is_added_up_once_an_hour():
    f = S['Forecast.cs']
    at = method_body(f, "private static bool PullAt")
    leaving = method_body(f, "internal static int WorthLeaving")
    return ("private static readonly Dictionary<string, float> _across =" in f
            and ordered(at, "if (!_pull.TryGetValue(site.StringId, out pull))",
                        "_across[site.StringId] = Projection.PullAcross(read);")
            and "_across.Clear();" in method_body(f, "private static void Build")
            and "_across.Clear();" in method_body(f, "internal static void Forget")
            and "foreach" not in leaving
            and S['Projection.cs'].count("total += one;") == 1
            and "A_market_with_no_pull_at_all_is_not_read" in PROJECTIONTESTS)

def two_routes_that_land_within_a_few_hours_share_one_price_ladder():
    f = S['Projection.cs']
    quartered = "withinDays = TradeMath.ToTheQuarterDay(withinDays);"
    return (f.count(quartered) == 3
            and all(quartered in method_body(f, sig)
                    for sig in ("internal static int UnitsLanding",
                                "internal static int WorthLanding",
                                "internal static int PurseLanding"))
            and "public const float HorizonStep = 0.25f;" in S['TradeMath.cs']
            and "days / HorizonStep + 0.5d" in method_body(S['TradeMath.cs'], "public static float ToTheQuarterDay")
            and "TradeMath.ToTheQuarterDay" in MATHTESTS
            and "The_window_is_read_to_the_nearest_quarter_day" in PROJECTIONTESTS
            and "The_purse_window_is_read_to_the_nearest_quarter_day_as_well" in PROJECTIONTESTS)

def a_workshop_run_lands_by_how_far_along_it_already_is():
    f = S['Forecast.cs']
    shops = method_body(f, "private static void ReadWhatTheShopsWillMake")
    made = method_body(f, "private static List<(ItemCategory category, int count)> Output")
    return ("internal const float WorkshopRunDays = 1f;" in S['Projection.cs']
            and "float lands = TradeMath.RunLandsIn(progress, Projection.WorkshopRunDays);" in shops
            and f.count("WorkshopRunDays") == 1
            and ordered(made, "shop.GetProductionProgress(i)",
                        "int pick = TradeRules.RunsSoonest(ready);",
                        "progress = ready[pick].progress;")
            and "TradeMath.RunLandsIn" in MATHTESTS
            and "A_workshop_run_is_measured_over_one_day" in PROJECTIONTESTS)

def what_a_workshop_makes_is_valued_at_the_good_that_town_stocks():
    f = S['Forecast.cs']
    stands = method_body(f, "private static ItemObject StandsForAt")
    shops = method_body(f, "private static void ReadWhatTheShopsWillMake")
    return ("ItemObject stands = StandsForAt(site, category);" in shops
            and "if (item == null || item.ItemCategory != category || !TradePolicy.Priced(item)) continue;"
                in stands
            and "TradeMath.StandsBetter(count, item.Value, most," in stands
            and "ItemObject stands = stocked ?? StandsFor(category);" in stands
            and "_standsForAt.Clear();" in method_body(f, "private static void Build")
            and "TradeMath.StandsBetter" in MATHTESTS)


def reading_what_the_shops_will_make_leaves_nothing_behind_to_clear_away():
    f = S['Forecast.cs']
    out = method_body(f, "private static List<(ItemCategory category, int count)> Output")
    stands = method_body(f, "private static ItemObject StandsForAt")
    return ("private static readonly List<(string, int)> _needs = new List<(string, int)>();" in f
            and "_needs.Clear();" in out
            and "_needs.Add((category == null ? null : category.StringId, count));" in out
            and "TradeRules.InputsHeld(_needs, held)" in out
            and "new List<(string, int)>()" not in out
            and "private static readonly Dictionary<(string site, string category), ItemObject> _standsForAt =" in f
            and "var key = (site.StringId, category.StringId);" in stands
            and 'site.StringId + "/"' not in f
            and "_standsForAt.Clear();" in method_body(f, "private static void Build"))


chk("1.81.5", "an hour's reading of what the workshops will make builds up no list or key of its own to be cleared away afterwards",
    reading_what_the_shops_will_make_leaves_nothing_behind_to_clear_away())

chk("1.64.1", "how much a market pulls in all is added up once an hour rather than once for every good priced",
    the_pull_across_a_market_is_added_up_once_an_hour())
chk("1.64.1", "the days to a market are read to the nearest quarter day, so two routes that land together share one price ladder",
    two_routes_that_land_within_a_few_hours_share_one_price_ladder())
chk("1.64.1", "a workshop run lands by how far along the game says it already is, within the length of one run",
    a_workshop_run_lands_by_how_far_along_it_already_is())
chk("1.64.1", "what a workshop will make is valued at the good that town actually stocks, and the cheapest of its kind only when it stocks none",
    what_a_workshop_makes_is_valued_at_the_good_that_town_stocks())


def the_deal_is_laid_out_rather_than_traded_only_while_that_is_switched_on():
    c = S['Counter.cs']
    rules = method_body(S['Rules.cs'], "internal static bool StagesTheDeal")
    menu = between(S['Trading.cs'], 'args => Guard.Run("Action.QuickTradeMenu"', 'false, 6);')
    return ("s != null && s.StagedTrading && !s.SimulationMode;" in rules
            and option_default('StagedTrading') == 'false'
            and EVER_SHIPPED.get('StagedTrading') == 'bool'
            and "_o.StagedTrading" in M
            and all(i in strings_declared() for i in ('TL281', 'TL400', 'TL401', 'TL402', 'TL403'))
            and "TradeRules.StagesTheDeal(Options.Current)" in method_body(c, "internal static bool Ready")
            and ordered(menu, "if (!Counter.Ready(Settlement.CurrentSettlement)) return;",
                        "ExecuteQuickSell(Settlement.CurrentSettlement);",
                        "TextObject laid = Counter.Settle();")
            and 'Guard.Run("GameEnd.Counter", Counter.Forget);' in S['SubModule.cs']
            and "The_deal_is_only_laid_out_when_that_is_switched_on_and_no_dry_run_is" in ROUTETESTS
            and "A_dry_run_wins_over_laying_the_deal_out_so_nothing_is_ever_put_on_the_screen_twice"
                in ROUTETESTS)

def nothing_moves_while_the_deal_is_laid_out():
    t = S['Trading.cs']
    return ("Sim = Options.Current.SimulationMode || Counter.Staging;" in t
            and "internal static bool Staging => _logic != null;" in S['Counter.cs']
            and t.count("Counter.Stage(el, selling: true, price);") == 1
            and t.count("Counter.Stage(el, selling: false, price);") == 2
            and t.count("Counter.Stage(_plan[at], selling: true, price);") == 1
            and t.count("Counter.Stage(Shelf[at], selling: false, price);") == 1
            and all("Counter.Stage(" in method_body(t, where)
                    for where in ("private sealed class SellingFrom",
                                  "public static void ExecuteResupply",
                                  "public static void ExecuteHerdRelief",
                                  "public static void ExecuteHaulage",
                                  "private sealed class BuyingAt"))
            and all("market.Staged(" in method_body(S['Passes.cs'], where)
                    for where in ("internal static Traded SellThem",
                                  "internal static Traded BuyThem"))
            and "internal bool Muted => Counter.Staging || TradeActionBehavior.Muted(Quiet);" in t)

def the_deal_is_laid_out_through_the_games_own_trade_screen():
    opened = method_body(S['Counter.cs'], "private static bool Opened")
    staged = method_body(S['Counter.cs'], "internal static void Stage")
    return (ordered(opened, "InventoryScreenHelper.OpenScreenAsTrade(stock, market);",
                    "InventoryScreenHelper.GetActiveInventoryState();",
                    "if (logic == null) return false;",
                    "if (logic.TotalAmountChange == null)",
                    "if (logic.DonationXpChange == null)")
            and "_logic.AddTransferCommand(TransferCommand.Transfer(" in staged
            and "1, selling ? mine : theirs, selling ? theirs : mine, el," in staged
            and "InventoryLogic.InventorySide.PlayerInventory" in staged
            and "InventoryLogic.InventorySide.OtherInventory" in staged
            and "if (_logic == null || price < 0) return;" in staged)

def arriving_at_a_market_holds_its_trade_back_while_the_deal_is_laid_out():
    entered = method_body(S['Trading.cs'], "private void OnSettlementEntered")
    line = method_body(S['Trading.cs'], "private static TextObject TheDealWaitsForYou")
    holds = method_body(S['Counter.cs'], "internal static bool HoldsBack")
    return (ordered(entered, "NoteThisArrival(settlement);", "if (_visitTradeAllowed && Counter.HoldsBack())",
                    "Notices.Say(TheDealWaitsForYou(), Notices.Note);",
                    "if (Options.Current.AutoSellOnEntry) ExecuteQuickSell(settlement, quiet: true);")
            and "TradeRules.StagesTheDeal(Options.Current)" in holds
            and "Options.Current.AutoSellOnEntry || Options.Current.AutoBuyOnEntry" in holds
            and '{=TL26}' in line and '{=TL403}' in line
            and 'line.SetTextVariable("ENTRY", Tongue.Text("{=TL26}' in line
            and "{ENTRY}" in spoken(ENGLISH)['TL403'])

def a_deal_left_half_laid_out_never_leaves_trading_switched_off():
    menu = between(S['Trading.cs'], 'args => Guard.Run("Action.QuickTradeMenu"', 'false, 6);')
    return (ordered(menu, "if (!Counter.Ready(Settlement.CurrentSettlement)) return;", "try", "finally",
                    "TextObject laid = Counter.Settle();")
            and "Counter.Forget();" in method_body(S['Trading.cs'], "private void OnSettlementEntered")
            and "Drop();" in method_body(S['Counter.cs'], "internal static bool Ready")
            and "TradeActionBehavior.StartAFreshDryRun();" in
                method_body(S['Counter.cs'], "private static bool Opened")
            and "internal static void StartAFreshDryRun() => Visit.ForgetTheDryRun();" in S['Trading.cs'])

def an_empty_counter_says_so_rather_than_leaving_you_guessing():
    settle = method_body(S['Counter.cs'], "internal static TextObject Settle")
    said = spoken(ENGLISH)
    return (ordered(settle, 'Log.Write("laid out on the trade screen: "',
                    "if (toSell == 0 && toBuy == 0)", "{=TL402}", "{=TL401}")
            and all(slot in said['TL401'] for slot in ('{SOLD}', '{GAINED}', '{BOUGHT}', '{SPENT}'))
            and all(('line.SetTextVariable("' + slot + '"') in settle
                    for slot in ('SOLD', 'GAINED', 'BOUGHT', 'SPENT'))
            and "Cancel" in said['TL402'] and "Done" in said['TL401'])


def asking_the_herd_early_never_switches_livestock_off_for_the_session():
    herd = method_body(S['Drove.cs'], "private static DefaultPartySpeedCalculatingModel Model")
    return (ordered(herd, "if (_lookupFailed) return null;",
                    "var models = Campaign.Current?.Models;",
                    "if (models == null) return null;",
                    "var model = models.PartySpeedCalculatingModel as DefaultPartySpeedCalculatingModel;",
                    "_lookupFailed = true;")
            and "Campaign.Current?.Models?.PartySpeedCalculatingModel" not in S['Trading.cs']
            and S['Drove.cs'].count("_lookupFailed = false;") == 1
            and "Drove.Forget();" in method_body(S['Trading.cs'], "internal static void ForgetVisit"))

def one_line_says_what_the_mod_could_read_when_your_campaign_opened():
    say = method_body(S['SubModule.cs'], "internal static void Say")
    launched = method_body(S['Trading.cs'], "private void OnSessionLaunched")
    return ('Log.Write("self-check: "' in say
            and 'Guard.Run("SelfCheck"' in say
            and all(one in say for one in (
                "Patcher.Tally()", "Drove.PenaltyRead()", "Errands.Known",
                "Tongue.StringsRead()", "Priced.ModelInForce()"))
            and say.count('" | "') == 4
            and S['Trading.cs'].count("SelfCheck.Say();") == 1
            and "SelfCheck.Say();" in launched
            and ordered(launched, "naval capability", "SelfCheck.Say();"))

def the_startup_line_names_what_each_reader_found():
    patched = method_body(S['Support.cs'], "internal static void TryPatch")
    tally = method_body(S['Rules.cs'], "internal static string Of(int applied, IList<string> refused)")
    strings = method_body(S['Tongue.cs'], "internal static string StringsRead")
    return (ordered(patched, "harmony.CreateClassProcessor(patchClass).Patch();",
                    "Applied.Add(patchClass.Name);")
            and "Refused.Add(patchClass.Name);" in patched
            and 'internal static string Tally() => Tallies.Of(Applied.Count, Refused);' in S['Support.cs']
            and '"patches " + applied + "/" + (applied + turned) + " applied"' in tally
            and '" refused"' in tally
            and '"herd penalty not read" : "herd penalty read"' in
                between(S['Drove.cs'], "internal static string PenaltyRead() =>", ";")
            and '"price model not read" : "prices from " + model.GetType().Name' in
                method_body(S['Market.cs'], "internal static string ModelInForce")
            and 'if (language == English) return "English";' in strings
            and '" strings read"' in strings
            and '" strings not read, speaking English"' in strings)

chk("1.65.0", "the deal is laid out on the trade screen rather than traded, and only while that switch is on and no dry run is",
    the_deal_is_laid_out_rather_than_traded_only_while_that_is_switched_on())
chk("1.65.0", "nothing moves while a deal is laid out: every pass books it as it would a dry run and lays each unit on the screen",
    nothing_moves_while_the_deal_is_laid_out())
chk("1.65.0", "the screen is the game's own trade screen, opened through the game's own helper, one unit to a transfer",
    the_deal_is_laid_out_through_the_games_own_trade_screen())
chk("1.65.0", "arriving at a market holds its own trading back while the deal is laid out, and names the menu entry that lays it out",
    arriving_at_a_market_holds_its_trade_back_while_the_deal_is_laid_out())
chk("1.65.0", "a deal with nothing in it says so, and a deal with something in it counts both sides and their gold",
    an_empty_counter_says_so_rather_than_leaving_you_guessing())
chk("1.65.0", "a deal that breaks off half laid out never leaves trading switched off, and each deal starts from a clean slate",
    a_deal_left_half_laid_out_never_leaves_trading_switched_off())

chk("1.66.0", "one self-check line says what TradeLord could read when your campaign opened, rather than five reports scattered through the log",
    one_line_says_what_the_mod_could_read_when_your_campaign_opened())
chk("1.66.0", "that line names what each reader found, both when it could read and when it could not",
    the_startup_line_names_what_each_reader_found())
chk("1.66.0", "asking the herd penalty as the campaign opens never mistakes a campaign it cannot read yet for a mod that replaced the model",
    asking_the_herd_early_never_switches_livestock_off_for_the_session())

chk("1.66.0", "the mod page is written out whole: the summary, the feature list, the comparison and the changelog for the version that shipped",
    the_page_carries_the_summary_the_features_the_comparison_and_the_changelog())
chk("1.66.0", "every line of that page comes from README.md, COMPARISON.md or CHANGELOG.md, so the page cannot drift from the repository",
    the_page_is_written_from_the_repository_rather_than_pasted())
chk("1.66.0", "the page is handed over in the markup Nexus reads, with no markdown left in it and every tag closed",
    the_page_leaves_no_markdown_behind_and_closes_every_tag())
chk("1.66.0", "the release notes and the mod page are asked for one at a time, and a flag the tool does not know is refused",
    the_page_and_the_notes_are_asked_for_one_at_a_time())
chk("1.66.0", "the feature list says a price is read through that market's own price model, and that the price it shows is the price it pays, rather than calling it the game's brain",
    the_feature_list_says_how_a_price_is_read_rather_than_naming_a_brain())
chk("1.66.0", "the mod page opens with five lines saying what TradeLord is, no ticks on them, and folds each headed run of the feature list away behind its own heading",
    the_page_leads_with_what_the_mod_is_and_folds_the_long_tail_away())

chk("1.67.0", "a market whose price has moved since you last looked says rising or falling next to it, on both sides of the tooltip and in every language",
    a_price_that_moved_since_your_last_look_says_so_in_the_tooltip())
chk("1.67.0", "which way a price moved, and what counts as a second reading, are worked out in one place a test can ask",
    which_way_a_price_moved_is_worked_out_where_a_test_can_ask_it())
chk("1.67.0", "a campaign saved before this version keeps every price it had and simply carries no earlier reading yet",
    a_campaign_saved_before_this_version_keeps_every_price_it_had())
chk("1.68.0", "the rising and falling marker ships switched off, behind a switch of its own on the settings screen, named in every language",
    the_price_direction_marker_ships_switched_off_with_a_switch_of_its_own())


def a_price_you_recorded_is_forgotten_once_it_is_older_than_you_asked():
    prune = method_body(S['Ledger.cs'], "private void PruneObservations")
    keep = between(S['TradeMath.cs'],
                   "public static bool WorthKeeping(float capturedDay, float now, int shelfLifeDays) =>", ";")
    tick = method_body(S['Ledger.cs'], "private void OnDailyTick")
    return (ordered(prune, "float now = (float)CampaignTime.Now.ToDays;",
                    "int shelfLife = Options.Current.ObservationShelfLifeDays;",
                    "if (TradeMath.WorthKeeping(seen.Value.CapturedDay, now, shelfLife)) "
                    "{ stillKept++; continue; }")
            and "shelfLifeDays <= KeptForever || now - capturedDay <= shelfLifeDays" in keep
            and "public const int KeptForever = 0;" in S['TradeMath.cs']
            and "PruneObservations();" in tick
            and option_default('ObservationShelfLifeDays') == '15'
            and EVER_SHIPPED.get('ObservationShelfLifeDays') == 'int'
            and "ObservationShelfLifeDays" in M
            and '{ "ObservationShelfLifeDays", new double[] { 0, 60 } },' in S['Migrate.cs']
            and "TheObservationShelfLifeIsGone" not in S['Migrate.cs']
            and all(one in DRIFTTESTS for one in
                    ("A_price_is_kept_through_its_fifteenth_day_and_forgotten_on_the_sixteenth",
                     "A_shelf_life_of_nothing_keeps_every_price_for_as_long_as_the_campaign_lasts",
                     "A_price_recorded_later_than_the_clock_says_is_never_thrown_away"))
            and "TheObservationShelfLifeASaveAlreadyCarriesIsKept" in MIGRATIONTESTS
            and "TheObservationShelfLifeIsHeldInsideItsRange" in MIGRATIONTESTS)

def the_shelf_life_setting_says_what_it_needs_and_what_nothing_means():
    said = spoken(ENGLISH)
    return (said['TL282'] == "Days to keep a price you recorded"
            and "Live world prices" in said['TL408']
            and "0 keeps every price" in said['TL408']
            and {'TL282', 'TL408'} <= strings_declared()
            and all({'TL282', 'TL408'} <= set(spoken(f))
                    for f in list(TRANSLATIONS.values()) + [ENGLISH])
            and "{=TL282}Days to keep a price you recorded" in M
            and "{=TL408}" in M)

chk("1.69.0", "a price you recorded yourself is forgotten once it is older than the days you asked for, and nothing means keep it forever",
    a_price_you_recorded_is_forgotten_once_it_is_older_than_you_asked())
chk("1.69.0", "the setting that forgets old prices names the setting it needs and says what nothing means, in every language",
    the_shelf_life_setting_says_what_it_needs_and_what_nothing_means())


def a_save_from_a_newer_tradelord_keeps_every_price_this_one_can_read():
    read = method_body(S['LedgerCodec.cs'],
                       "public static Dictionary<string, List<PriceObservation>> ReadLedger(string text,")
    restored = method_body(S['Ledger.cs'], "public override void SyncData")
    return ("if (parts.Length < FieldsAPriceNeeds) { unreadable++; continue; }" in read
            and "if (parts.Length >= FieldsAPriceIsWrittenIn && Whole(parts[5], out int earlierBuy) &&" in read
            and "if (records[i].Length == 0) continue;" in read
            and "public const int FieldsAPriceNeeds = 5;" in S['LedgerCodec.cs']
            and "public const int FieldsAPriceIsWrittenIn = 8;" in S['LedgerCodec.cs']
            and ("public static Dictionary<string, List<PriceObservation>> ReadLedger(string text) =>\n"
                 "            ReadLedger(text, out _);") in S['LedgerCodec.cs']
            and ordered(restored, "(_unreadable == 0", '? ""',
                        'recorded price(s) this version could not read',
                        "written by a newer TradeLord than this one")
            and all(one in DRIFTTESTS for one in
                    ("A_record_a_newer_TradeLord_wrote_keeps_every_field_this_one_understands",
                     "A_record_too_short_to_hold_a_price_keeps_the_current_price_it_does_hold",
                     "A_record_with_too_few_fields_to_read_at_all_is_dropped_and_counted",
                     "Every_record_a_save_holds_that_cannot_be_read_is_counted_on_its_own",
                     "A_ledger_that_reads_whole_reports_nothing_it_could_not_read")))


chk("1.69.1", "a save written by a newer TradeLord keeps every price this one can read, rather than losing the lot, and the log says how many it could not read",
    a_save_from_a_newer_tradelord_keeps_every_price_this_one_can_read())


def the_page_leads_with_lines_short_enough_to_read_at_a_glance():
    lead = [one for one in README.split('\n## ', 1)[0].split('\n') if one.startswith('- ')]
    return (len(lead) == 5
            and all(len(one.split()) <= 35 for one in lead)
            and max(len(one.split()) for one in lead) >= 10)

def the_comparison_says_when_the_nine_were_read():
    opening = COMPARISON.split('\n- ', 1)[0]
    months = ('January', 'February', 'March', 'April', 'May', 'June', 'July',
              'August', 'September', 'October', 'November', 'December')
    return (any(month + ' 20' in opening for month in months)
            and 'decompiled and read in ' in opening
            and 'changed since then is not in here' in opening)


chk("1.69.1", "the five lines the mod page opens with are each short enough to read at a glance",
    the_page_leads_with_lines_short_enough_to_read_at_a_glance())
chk("1.69.1", "the comparison with the other trade mods says when the nine were read, so no claim in it reads as current forever",
    the_comparison_says_when_the_nine_were_read())


def no_travel_or_price_rule_hands_back_a_number_that_is_not_one():
    t = S['TradeMath.cs']
    return ("public const float FurthestThereIs = float.MaxValue;" in t
            and "public static float Finite(float value, float ifNot) =>" in t
            and "float.IsNaN(value) || float.IsInfinity(value) ? ifNot : value;" in t
            and "Finite((landLeg / landSpeed + seaLeg / seaSpeed) / 24f, FurthestThereIs)" in t
            and "Finite(distance / (Math.Max(landSpeed, seaSpeed) * 24f), FurthestThereIs)" in t
            and "Finite(distance / (pace * 24f), FurthestThereIs)" in t
            and "days = Finite(days, 0f);" in method_body(t, "public static float ToTheQuarterDay")
            and "progress = Finite(progress, 0f);" in method_body(t, "public static float RunLandsIn")
            and "float days = Finite((nowHours - thenHours) / 24f, 0f);" in t
            and "Finite(total / counted, 0f)" in t
            and "Finite(farSellPrice * safetyFactor, 0f)" in t
            and "float pull = 1f - Finite(priceFactor, 1f);" in t
            and "TradeMath.Finite(s.MaxTravelDaysTown, 0f)" in S['Ranking.cs']
            and "TradeMath.Finite(s.MaxTravelDaysVillage, 0f)" in S['Ranking.cs']
            and all(one in MATHTESTS for one in
                    ("Every_travel_rule_hands_back_a_real_number_whatever_it_is_handed",
                     "A_distance_the_game_cannot_work_out_reads_as_far_away_rather_than_next_door",
                     "A_travel_time_the_game_can_work_out_is_left_exactly_as_it_was",
                     "A_workshop_whose_progress_cannot_be_read_is_taken_as_not_started_yet"))
            and "A_travel_ceiling_that_is_not_a_number_looks_as_far_as_it_likes" in RANKTESTS)

def a_good_worth_showing_is_counted_without_dividing_by_a_price_of_nothing():
    return ("return spendCap > 0 && buyPrice > 0 ? Math.Min(stocked, spendCap / buyPrice) : stocked;"
            in method_body(S['Ledger.cs'], "private static int MostWorthShowing")
            and "if (buyPrice <= 0) continue;" in method_body(S['Ledger.cs'],
                                                              "private List<TradeRoute> ScanRoutes"))


chk("1.69.2", "no travel time, price factor or workshop run hands back a number that is not a number, whatever the game reports",
    no_travel_or_price_rule_hands_back_a_number_that_is_not_one())
chk("1.69.2", "how many of a good are worth showing is counted without dividing by a price of nothing",
    a_good_worth_showing_is_counted_without_dividing_by_a_price_of_nothing())


def what_the_shelf_life_forgot_is_said_in_the_log():
    prune = method_body(S['Ledger.cs'], "private void PruneObservations")
    return (ordered(prune,
                    "int tooOld = 0, stillKept = 0;",
                    "if (seen.Value == null || seen.Value.TownId == null) { dead.Add(seen.Key); continue; }",
                    "if (TradeMath.WorthKeeping(seen.Value.CapturedDay, now, shelfLife)) "
                    "{ stillKept++; continue; }",
                    "tooOld++;",
                    "if (tooOld > 0)",
                    'Log.Write("prices forgotten: " + tooOld + " older than " + shelfLife +',
                    '" day(s), " + stillKept + " still kept");')
            and prune.count("Log.Write(") == 1
            and "PruneObservations();" in method_body(S['Ledger.cs'], "private void OnDailyTick"))


chk("1.69.3", "the log says how many recorded prices were forgotten for their age and how many are still kept",
    what_the_shelf_life_forgot_is_said_in_the_log())


def said_in_every_language(tid):
    en = spoken(ENGLISH)
    if tid not in en or not en[tid].strip():
        return False
    for path in TRANSLATIONS.values():
        said = spoken(path)
        if tid not in said or not said[tid].strip() or said[tid] == en[tid]:
            return False
    return True

def held_inside_a_range(name):
    return '{ "' + name + '", new double[]' in S['Migrate.cs']

def what_the_purses_on_the_road_take_comes_off_the_shelf_as_well_as_the_price():
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
    leaving = method_body(S['Forecast.cs'], "internal static int UnitsLeaving")
    return ("int landedAtBuyTown = Forecast.WorthShiftAsItHasHeld(from, item, toBuy);" in scan
            and "TradeMath.StockAfterShift(onTheShelfNow," in scan
            and "Forecast.UnitsLanding(from, item, toBuy)," in scan
            and "Forecast.UnitsLeaving(from, item, toBuy));" in scan
            and "return Projection.UnitsLeaving(WorthLeaving(site, item, withinDays), item.Value);"
                in leaving
            and "if (!On" in leaving
            and "return worthLeaving / unitValue;" in
                method_body(S['Projection.cs'], "internal static int UnitsLeaving")
            and "TaleWorlds" not in S['Projection.cs']
            and "What_the_purses_on_the_road_will_take_comes_off_the_shelf_too" in MATHTESTS
            and "What_a_purse_takes_off_the_shelf_is_what_it_takes_off_the_worth" in PROJECTIONTESTS
            and "A_purse_takes_whole_units_off_the_shelf_and_never_part_of_one" in PROJECTIONTESTS)

def the_share_of_the_hold_it_may_fill_ships_at_the_whole_hold_and_binds_buying_only():
    src = S['Trading.cs']
    return (option_default('MaxCargoShare') == '1f'
            and "_o.MaxCargoShare" in M
            and EVER_SHIPPED.get('MaxCargoShare') == 'float'
            and held_inside_a_range('MaxCargoShare')
            and src.count("Options.Current.MaxCargoShare") == 2
            and "TradeMath.RoomToFill(Capacity(party), Carried(party), Options.Current.MaxCargoShare);"
                in src
            and "TradeMath.RoomToFill(Capacity, Carried(), Options.Current.MaxCargoShare);"
                in src
            and "if (_carried >= 0f && version == _carriedAt) return _carried;" in src
            and src.count("Carry.Carried(") == 1
            and "float ceiling = cargoShare > 0f && cargoShare < 1f ? capacity * cargoShare : capacity;"
                in method_body(S['TradeMath.cs'], "public static float RoomToFill")
            and "MaxCargoShare" not in sell_pass()
            and "MaxCargoShare" not in sell_rule()
            and "The_hold_is_filled_no_further_than_the_share_you_allow" in MATHTESTS
            and said_in_every_language('TL410') and said_in_every_language('TL411'))

def a_companion_riding_with_you_earns_a_share_of_the_profit_and_nothing_by_default():
    credit = method_body(S['Trading.cs'], "private static void CreditTheCompanionsWithYou")
    return (option_default('PartyTradeXpShare') == '0f'
            and "_o.PartyTradeXpShare" in M
            and EVER_SHIPPED.get('PartyTradeXpShare') == 'float'
            and held_inside_a_range('PartyTradeXpShare')
            and ordered(credit,
                        "float each = TradeMath.PartyShareOfProfit(xp, Options.Current.PartyTradeXpShare);",
                        "if (each <= 0f) return;",
                        "foreach (Hero companion in Hero.MainHero.CompanionsInParty)",
                        "companion.AddSkillXp(DefaultSkills.Trade, each);")
            and 'Guard.Run("TradeXp.Party", () => CreditTheCompanionsWithYou(xp));' in
                method_body(S['Trading.cs'], "private static void CreditTradeSkill")
            and "SkillLevelingManager.OnTradeProfitMade(Hero.MainHero, xp);" in S['Trading.cs']
            and S['Trading.cs'].count("CompanionsInParty") == 1
            and "A_companion_learns_from_a_share_of_the_profit_and_from_nothing_when_it_is_off"
                in MATHTESTS
            and said_in_every_language('TL412') and said_in_every_language('TL413'))

def the_tooltip_says_what_you_paid_for_a_good_you_have_bought():
    paid = method_body(S['Ledger.cs'], "public int PaidPerUnit")
    add = method_body(S['TooltipPatches.cs'],
                      "internal static void Append(ItemMenuVM vm, ItemVM itemVm)")
    return ("return TradeMath.UnitBasis(rec, 0);" in paid
            and "if (item == null) return TradeMath.NoRecordedBasis;" in paid
            and ordered(add,
                        "int paid = ledger.PaidPerUnit(item);",
                        "if (paid > 0)",
                        'AddLine(vm, Tongue.Text("{=TL409}You paid").ToString(), paid + GoldIcon, Title);',
                        'Tongue.Text("{=TL20}Best sell prices")')
            and said_in_every_language('TL409'))


chk("1.70.0", "what the purses on the road will take comes off the shelf, the same as it already comes off the price",
    what_the_purses_on_the_road_take_comes_off_the_shelf_as_well_as_the_price())
chk("1.70.0", "the share of the hold TradeLord may fill ships at the whole hold and binds buying only",
    the_share_of_the_hold_it_may_fill_ships_at_the_whole_hold_and_binds_buying_only())
chk("1.70.0", "a companion riding with you earns a share of the profit, and nothing until you ask for it",
    a_companion_riding_with_you_earns_a_share_of_the_profit_and_nothing_by_default())
chk("1.70.0", "the tooltip says what you paid for a good you have bought, in every language",
    the_tooltip_says_what_you_paid_for_a_good_you_have_bought())


def how_long_a_shelf_holds_a_deal_is_worked_out_where_a_test_can_ask_it():
    p = S['Projection.cs']
    runs = method_body(p, "internal static float RunsOutAt")
    curve = method_body(p, "internal static List<(float days, int shelf)> ShelfAhead")
    reads = method_body(p, "internal static float RunsOutOf")
    moments = method_body(p, "internal static List<float> Moments")
    note = method_body(p, "private static void Note")
    return ("TaleWorlds" not in p and "Settlement" not in p and "ItemObject" not in p
            and "internal const float NeverRunsOut = -1f;" in p
            and "if (item == null || wanted <= 0 || unitValue <= 0) return NeverRunsOut;" in runs
            and "return RunsOutOf(ShelfAhead(listed, coming, pull, across, item, category," in runs
            and "if (item == null || unitValue <= 0) return curve;" in curve
            and "WorthLeaving(PurseLanding(coming, days), pull, across, category), unitValue);" in curve
            and "UnitsLanding(listed, item, days), taken)));" in curve
            and "wanted" not in curve
            and "if (curve == null || wanted <= 0) return NeverRunsOut;" in reads
            and "if (curve[i].shelf < wanted) return curve[i].days;" in reads
            and "return NeverRunsOut;" in reads
            and "when.Sort();" in moments
            and "float at = TradeMath.UpToTheQuarterDay(days);" in note
            and "if (at <= afterDays || !already.Add(at)) return;" in note
            and "double steps = Math.Ceiling(days / HorizonStep);" in
                method_body(S['TradeMath.cs'], "public static float UpToTheQuarterDay")
            and all(one in PROJECTIONTESTS for one in
                    ("A_shelf_nobody_is_coming_for_never_runs_out",
                     "A_shelf_runs_out_at_the_moment_a_purse_takes_the_last_of_it",
                     "A_load_landing_first_holds_the_shelf_up_past_a_purse",
                     "Nothing_that_lands_before_you_arrive_can_expire_the_deal",
                     "Every_moment_counted_is_after_you_arrive_and_on_the_quarter_day",
                     "A_shelf_that_runs_out_never_reports_a_moment_you_have_already_passed",
                     "The_shelf_a_town_will_hold_is_the_same_whatever_size_deal_asks_for_it",
                     "The_shelf_a_town_will_hold_is_read_once_for_every_moment_it_changes",
                     "A_shelf_read_ahead_that_never_dips_hands_back_no_moment_at_all",
                     "A_deal_of_nothing_never_expires_against_a_shelf_read_ahead"))
            and "What_lands_on_a_day_is_counted_by_the_moment_that_day_rounds_up_to" in MATHTESTS)

def a_route_says_how_long_its_buy_market_holds_that_quantity():
    ask = method_body(S['Forecast.cs'], "internal static float RunsOutIn")
    kept = method_body(S['Forecast.cs'],
                       "private static List<(float days, int shelf)> ShelfAhead")
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
    return ("if (!On || site == null || item == null || item.ItemCategory == null)" in ask
            and "return Projection.RunsOutOf(ShelfAhead(site, item, stockNow, afterDays), wanted);" in ask
            and "if (_shelfAhead.TryGetValue(key, out List<(float days, int shelf)> curve)) return curve;"
                in kept
            and "curve = Projection.ShelfAhead(listed, coming, pull, across, item.StringId," in kept
            and "_shelfAhead.Clear();" in method_body(S['Forecast.cs'], "private static void Build")
            and "public float RunsOutInDays = Projection.NeverRunsOut;" in S['Ledger.cs']
            and "float runsOut = Forecast.RunsOutIn(from, item, onTheShelfNow, q.Units, toBuy);" in scan
            and "RunsOutInDays = runsOut" in scan
            and "float days = _route.RunsOutInDays;" in S['Panel.cs']
            and 'if (days <= 0f) return "";' in S['Panel.cs']
            and '"HeadDays", "HeadRunsOut",' in S['Panel.cs']
            and '{=TL416}Left' in S['Panel.cs']
            and '{=TL417} | Left = how long that shelf still holds this Qty once you arrive' in S['Panel.cs']
            and 'Text="@RunsOut"' in PREFAB and 'Text="@HeadRunsOut"' in PREFAB
            and all(said_in_every_language(one) for one in ("TL414", "TL415", "TL416", "TL417"))
            and panel_columns()[0] is not None and len(panel_columns()[0]) == 12)


chk("1.71.0", "how long a market's shelf holds the quantity a route quotes is worked out where a test can ask it",
    how_long_a_shelf_holds_a_deal_is_worked_out_where_a_test_can_ask_it())
chk("1.71.0", "every route says how long its buy market holds that quantity, and the panel names it in every language",
    a_route_says_how_long_its_buy_market_holds_that_quantity())


def the_comparison_is_one_entry_a_mod_naming_and_linking_to_each():
    lines = [one for one in COMPARISON.split('\n') if one.startswith('- ')]
    wanted = ['135', '1490', '3206', '8474', '10369', '11607', '11648', '11708', '11988']
    if len(lines) != len(wanted):
        return False
    for mod, line in zip(wanted, lines):
        said = re.match(r'- (\d+) - \[([^\]]+)\]'
                        r'\(https://www\.nexusmods\.com/mountandblade2bannerlord/mods/(\d+)\)'
                        r' - ([^:]+): \S', line)
        if not said or said.group(1) != mod or said.group(3) != mod:
            return False
        if not said.group(2).strip() or not said.group(4).strip():
            return False
    out = made_page()
    return (len(COMPARISON) <= 5000
            and out is not None
            and all('[url=https://www.nexusmods.com/mountandblade2bannerlord/mods/'
                    + mod + ']' in out for mod in wanted))


chk("1.71.0", "the comparison is one entry a mod, each naming and linking to the mod it weighs, and short enough for a mod page",
    the_comparison_is_one_entry_a_mod_naming_and_linking_to_each())


def the_recorded_prices_are_held_to_a_ceiling_oldest_forgotten_first():
    trim = method_body(S['Ledger.cs'], "private void TrimToWhatItKeeps")
    rule = method_body(S['Scoring.cs'], "internal static List<Reading> OldestBeyond")
    return ("internal const int MostPricesKept = 2500;" in S['Scoring.cs']
            and "TaleWorlds" not in S['Scoring.cs']
            and "Settlement" not in S['Scoring.cs']
            and "held.Sort((x, y) => x.Day.CompareTo(y.Day));" in rule
            and "if (held == null || cap <= 0 || held.Count <= cap) return dropped;" in rule
            and "int over = held.Count - cap;" in rule
            and "var dropped = Kept.OldestBeyond(held, Kept.MostPricesKept);" in trim
            and "if (dropped.Count == 0) return;" in trim
            and 'Log.Write("prices forgotten: " + dropped.Count + " of the oldest' in trim
            and trim.count("Log.Write(") == 1
            and all(one in SCORINGTESTS for one in
                    ("A_book_inside_what_it_keeps_drops_nothing",
                     "A_book_past_what_it_keeps_drops_the_oldest_first",
                     "A_book_with_no_ceiling_or_nothing_in_it_drops_nothing",
                     "What_is_left_after_a_trim_is_never_more_than_the_ceiling",
                     "Nothing_kept_is_older_than_anything_dropped")))

def the_log_says_what_went_into_the_save_as_well_as_what_came_out():
    sync = method_body(S['Ledger.cs'], "public override void SyncData")
    return ('Log.Write("ledger written into the save: " + RecordedPrices() + " recorded price(s) in "' in sync
            and '_ledgerText.Length + " character(s), and " + _purchases.Count +' in sync
            and 'Log.Write("ledger restored: "' in sync
            and "foreach (var kv in _ledger) held += kv.Value == null ? 0 : kv.Value.Count;" in
                method_body(S['Ledger.cs'], "private int RecordedPrices"))


chk("1.71.1", "the recorded prices are held to a ceiling, the oldest forgotten first, and the log says so",
    the_recorded_prices_are_held_to_a_ceiling_oldest_forgotten_first())
chk("1.71.1", "the log says what went into the save as well as what came out of it",
    the_log_says_what_went_into_the_save_as_well_as_what_came_out())


def what_counts_as_the_same_arrival_is_worked_out_where_a_test_can_ask_it():
    r = S['Rules.cs']
    return ("TaleWorlds" not in r and "Settlement" not in r and "MobileParty" not in r
            and "internal static class Arrivals" in r
            and "internal const float SetOffFromTheGate = 1f;" in r
            and "SetOffFromTheGate" not in S['Trading.cs']
            and 'Rules.cs' in TESTPROJ
            and all(one in ARRIVALTESTS for one in
                    ("Walking_back_through_the_same_gate_is_the_same_arrival",
                     "Taking_to_the_road_ends_an_arrival_even_at_the_same_market",
                     "A_different_market_is_never_the_same_arrival",
                     "The_road_is_taken_once_you_are_further_from_the_gate_than_the_threshold",
                     "A_gate_nobody_wrote_down_never_starts_the_road",
                     "Once_the_road_is_taken_standing_still_does_not_untake_it",
                     "A_sitting_is_the_same_only_at_the_same_market_in_the_same_hour")))


chk("1.71.1", "what counts as the same arrival, the same sitting and taking to the road is worked out where a test can ask it",
    what_counts_as_the_same_arrival_is_worked_out_where_a_test_can_ask_it())


def what_the_party_drives_is_worked_out_in_one_place_a_test_can_ask():
    r = S['Rules.cs']
    return ("internal static class Herding" in r
            and "TaleWorlds" not in r and "MobileParty" not in r
            and "(herd < 0 ? 0 : herd) + MountsNobodyRides(mounts, menOnFoot);" in r
            and S['Drove.cs'].count("Herding.MountsNobodyRides(") == 2
            and S['Drove.cs'].count("Herding.DrivenInAll(") == 3
            and "Math.Max(0, mounts - foot)" not in S['Trading.cs']
            and all(one in HERDTESTS for one in
                    ("A_horse_a_man_on_foot_can_ride_is_ridden_rather_than_driven",
                     "A_party_with_nobody_on_foot_drives_every_loose_mount",
                     "Nothing_counted_below_nothing_ever_shrinks_the_herd",
                     "What_is_driven_is_the_herd_plus_the_mounts_nobody_rides",
                     "Putting_a_man_on_foot_never_makes_the_herd_larger")))


chk("1.71.1", "what the party drives, and the mounts its men on foot ride rather than drive, is worked out in one place a test can ask",
    what_the_party_drives_is_worked_out_in_one_place_a_test_can_ask())


def the_feature_list_answers_what_people_ask_before_installing():
    answers = README.split("**Answers to what people ask**", 1)
    if len(answers) != 2:
        return False
    asked = [one for one in answers[1].split("\n## What it needs", 1)[0].split("\n")
             if one.startswith("- \u2705")]
    out = made_page()
    return (len(asked) >= 6
            and all("?" in one for one in asked)
            and all(len(one.split("?", 1)[1].strip()) > 20 for one in asked)
            and out is not None
            and "[b]Answers to what people ask[/b]\n[spoiler]" in out
            and "[spoiler]" not in out.split("[size=5][b]What it needs", 1)[1])


chk("1.71.1", "the feature list ends with short answers to what people ask before installing, folded away like every other run of it",
    the_feature_list_answers_what_people_ask_before_installing())


def how_long_a_new_campaign_is_left_to_settle_is_worked_out_where_a_test_can_ask():
    rule = method_body(S['Rules.cs'], "internal static bool StillHolding")
    return ("internal static class Settling" in S['Rules.cs']
            and "float elapsed = TradeMath.Finite(elapsedDays, float.MaxValue);" in rule
            and "if (daysLeft < 1) daysLeft = 1;" in rule
            and "if (daysLeft > waitDays) daysLeft = waitDays;" in rule
            and "msg.SetTextVariable(\"DAYS\", daysLeft);" in S['Trading.cs']
            and "Math.Ceiling(wait - elapsed)" not in S['Trading.cs']
            and all(one in SETTLINGTESTS for one in
                    ("A_delay_of_nothing_never_holds_a_market_back",
                     "A_campaign_older_than_the_delay_trades_at_once",
                     "A_young_campaign_is_told_how_many_days_are_left",
                     "A_market_held_back_is_never_told_it_has_no_days_left",
                     "A_campaign_age_the_game_cannot_work_out_never_holds_a_market_back",
                     "The_days_left_are_always_between_one_and_the_delay_you_set",
                     "A_day_further_on_is_never_a_day_further_from_trading")))


chk("1.71.2", "how long a new campaign is left to settle is worked out where a test can ask, and an age the game cannot report never holds a market shut",
    how_long_a_new_campaign_is_left_to_settle_is_worked_out_where_a_test_can_ask())


def which_band_a_route_row_falls_in_is_worked_out_where_a_test_can_ask():
    r = S['Rules.cs']
    p = S['Panel.cs']
    return ("internal static class Ranks" in r
            and "internal const int Bands = 5;" in r
            and "count <= 1 || at < 0 ? 0f : (float)at / (count - 1);" in r
            and "rank < 0.2f ? 1 : rank < 0.45f ? 2 : rank < 0.7f ? 3 : rank < 0.85f ? 4 : Bands;" in r
            and all("Ranks.BandOf(_rank) == " + str(band) + ";" in p
                    for band in range(1, 6))
            and "Ranks.Of(i, routes.Count), _centerMap));" in p
            and "_rank <" not in p and "_rank >=" not in p
            and all(one in RANKROWTESTS for one in
                    ("The_best_route_ranks_at_the_top_and_the_last_at_the_bottom",
                     "A_list_of_one_or_none_ranks_everything_at_the_top",
                     "Every_band_starts_where_the_one_before_it_ends",
                     "A_row_is_never_two_bands_and_never_none",
                     "A_row_further_down_the_list_is_never_in_an_earlier_band")))


chk("1.71.2", "which band a route row falls in, and where in the list it ranks, is worked out where a test can ask",
    which_band_a_route_row_falls_in_is_worked_out_where_a_test_can_ask())


def where_the_map_button_catches_the_mouse_is_worked_out_where_a_test_can_ask():
    r = S['Rules.cs']
    over = method_body(r, "internal static bool Over")
    return ("internal static class MapButton" in r
            and "internal const float Pad = 6f;" in r
            and "TaleWorlds" not in r and "Widget" not in r
            and "if (!BoundsReadable(screenW, screenH, width, height)) return false;" in over
            and "float padX = Pad / screenW, padY = Pad / screenH;" in over
            and "float right = 1f - marginRight / screenW;" in over
            and "6f / screenW" not in S['Panel.cs']
            and all(one in MAPBUTTONTESTS for one in
                    ("The_middle_of_the_button_is_over_the_button",
                     "The_middle_of_the_map_is_not",
                     "The_button_sits_flush_right_and_centred",
                     "A_cursor_just_outside_is_still_caught_and_one_further_out_is_not",
                     "A_screen_or_a_button_the_game_cannot_measure_is_not_readable",
                     "Nothing_is_over_a_button_that_cannot_be_measured",
                     "No_part_of_the_map_is_reserved_when_the_button_cannot_be_measured",
                     "The_region_holds_at_any_aspect_ratio")))


chk("1.71.2", "where the map button catches the mouse is worked out where a test can ask",
    where_the_map_button_catches_the_mouse_is_worked_out_where_a_test_can_ask())


def which_of_the_twins_wins_is_worked_out_where_a_test_can_ask():
    m = S['Migrate.cs']
    return ("public static class Twins" in m
            and "TaleWorlds" not in m
            and "public static readonly TimeSpan HandTolerance = TimeSpan.FromSeconds(30);" in m
            and "stamped == default(DateTime) || lastWritten > stamped + HandTolerance;" in m
            and "HandTolerance" not in S['Config.cs']
            and 'Migrate.cs' in TESTPROJ
            and all(one in TWINSTESTS for one in
                    ("A_file_with_no_stamp_at_all_is_taken_as_edited_by_hand",
                     "A_file_written_when_it_says_it_was_is_not_an_edit_by_hand",
                     "A_file_touched_after_the_tolerance_is_an_edit_by_hand",
                     "The_tolerance_is_generous_enough_for_a_slow_write_and_no_more",
                     "The_screen_wins_only_when_it_is_there_and_wrote_the_file_last",
                     "An_edit_by_hand_always_beats_the_screen",
                     "Nothing_the_screen_never_wrote_is_ever_overwritten_by_it")))


chk("1.71.2", "which of the twins wins, the settings file or the settings screen, is worked out where a test can ask",
    which_of_the_twins_wins_is_worked_out_where_a_test_can_ask())


def how_a_settings_file_is_read_and_written_is_worked_out_where_a_test_can_ask():
    m = S['Migrate.cs']
    read = method_body(m, "public static Dictionary<string, string> Read(IEnumerable<string> lines, ICollection<string> ignored)")
    compose = method_body(m, "public static string Compose(IEnumerable<string> header,")
    config = method_body(S['Config.cs'], "private static void Read")
    write = method_body(S['Config.cs'], "private static void Write")
    return ("public static class SettingsFile" in m
            and "TaleWorlds" not in m and "Log." not in m
            and "public const char Marks = '#';" in m
            and "public const char Splits = '=';" in m
            and ordered(read, "if (lines == null) return written;",
                        "if (raw == null) continue;",
                        "string line = raw.Trim();",
                        "if (line.Length == 0 || line[0] == Marks) continue;",
                        "int mark = line.IndexOf(Splits);",
                        "if (mark < 0) { ignored?.Add(line); continue; }",
                        "written[line.Substring(0, mark).Trim()] = line.Substring(mark + 1).Trim();")
            and "StringComparer.OrdinalIgnoreCase" in read
            and ordered(compose, "if (header != null)", "sb.Append(Marks);",
                        "if (!string.IsNullOrEmpty(line)) sb.Append(' ').Append(OneLine(line));",
                        "sb.AppendLine();", "if (settings != null)",
                        "if (string.IsNullOrEmpty(line.Key)) continue;",
                        "sb.Append(OneLine(line.Key)).Append(' ').Append(Splits).Append(' ')",
                        ".AppendLine(OneLine(line.Value));")
            and ordered(config, "string[] lines = Lines(found);",
                        "var ignored = new List<string>();",
                        "var written = SettingsFile.Read(lines, ignored);",
                        "foreach (string line in ignored)",
                        "is not a name = value line, so it is ignored")
            and "File.WriteAllText(path, SettingsFile.Compose(Header, lines));" in write
            and "StringBuilder" not in S['Config.cs']
            and 'Migrate.cs' in TESTPROJ
            and all(one in SETTINGSFILETESTS for one in
                    ("A_comment_line_and_a_blank_line_carry_no_setting",
                     "The_space_around_a_name_and_its_value_is_not_part_of_either",
                     "A_line_with_no_equals_sign_is_handed_back_rather_than_read_as_a_setting",
                     "The_last_line_carrying_a_name_is_the_one_that_counts",
                     "A_name_is_found_whatever_case_it_was_written_in",
                     "A_value_carrying_an_equals_sign_survives_whole",
                     "A_name_with_nothing_after_the_equals_sign_is_read_as_an_empty_value",
                     "Nothing_to_read_and_nothing_to_report_are_both_taken_in_their_stride",
                     "The_header_goes_out_behind_the_comment_mark_with_a_blank_line_after_it",
                     "A_file_composed_with_no_header_and_no_settings_is_still_a_file",
                     "A_setting_with_no_name_is_not_written_and_one_with_no_value_is",
                     "What_is_composed_is_read_back_as_what_went_in",
                     "A_file_stored_with_carriage_returns_reads_the_same_as_one_without",
                     "Whatever_is_written_comes_back_unchanged_however_it_is_spaced_and_stored",
                     "new Random(8823)")))


chk("1.71.2", "how a settings file is read and written is worked out where a test can ask, carriage returns and all",
    how_a_settings_file_is_read_and_written_is_worked_out_where_a_test_can_ask())



def which_settings_screen_the_game_loaded_is_worked_out_where_a_test_can_ask():
    rules = S['Rules.cs']
    support = S['Support.cs']
    which = method_body(rules, "internal static string Which")
    return ("internal static class Screens" in rules
            and "TaleWorlds" not in rules and "HarmonyLib" not in rules
            and "if (loaded == null) return null;" in which
            and "AppDomain" not in rules and "Assembly" not in rules
            and ordered(method_body(support, "private static IEnumerable<string> LoadedNames"),
                        "foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())",
                        "yield return a.GetName().Name;")
            and "g <= McmGeneration + GenerationsAhead" in method_body(support, "private static string Detect")
            and 'Rules.cs' in TESTPROJ
            and all(one in SCREENTESTS for one in
                    ("The_line_this_build_wants_is_built_from_its_number_alone",
                     "The_line_is_read_off_the_assembly_name_however_long_the_number_is",
                     "An_assembly_that_is_not_an_MCM_line_at_all_is_passed_over",
                     "An_MCM_name_with_no_number_after_it_is_not_a_line",
                     "The_line_is_recognised_whatever_case_the_assembly_was_named_in",
                     "The_line_this_build_was_made_for_wins_wherever_it_sits_in_the_list",
                     "A_line_this_build_was_not_made_for_is_reported_rather_than_passed_over",
                     "The_first_other_line_found_is_the_one_reported",
                     "Nothing_that_looks_like_MCM_at_all_leaves_the_loader_to_go_looking",
                     "The_line_this_build_wants_is_matched_whatever_case_it_loaded_under",
                     "What_the_loader_asks_for_is_always_what_it_takes_when_it_is_there",
                     "new Random(3391)")))


chk("1.71.2", "which line of MCM the game loaded, and whether it is the one this build was made for, is worked out where a test can ask",
    which_settings_screen_the_game_loaded_is_worked_out_where_a_test_can_ask())


def the_startup_tally_is_worked_out_where_a_test_can_ask():
    rules = S['Rules.cs']
    of = method_body(rules, "internal static string Of(int applied, IList<string> refused)")
    return ("internal static class Tallies" in rules
            and "int turned = refused == null ? 0 : refused.Count;" in of
            and "if (applied < 0) applied = 0;" in of
            and "return turned == 0 ? said : said" in of
            and "Applied.Count" in method_body(S['Support.cs'], "internal static string Tally")
            and 'Rules.cs' in TESTPROJ
            and all(one in TALLYTESTS for one in
                    ("Every_reader_applied_is_said_plainly_with_nothing_about_refusals",
                     "A_refused_reader_is_counted_in_the_total_and_named_after_it",
                     "Every_refused_reader_is_named_in_the_order_it_was_refused",
                     "A_reader_with_no_name_still_counts_against_the_total",
                     "Nothing_applied_and_everything_refused_still_reads_as_a_tally",
                     "A_count_that_cannot_be_right_never_reads_as_a_negative_tally",
                     "The_total_is_always_what_was_applied_and_what_was_refused_together",
                     "new Random(6604)")))


chk("1.71.2", "the startup line counting the readers that took and the readers that refused is worked out where a test can ask",
    the_startup_tally_is_worked_out_where_a_test_can_ask())



def the_panel_says_what_it_traded_for_you_lately():
    rules = S['Rules.cs']
    panel = S['Panel.cs']
    ledger = S['Ledger.cs']
    keep = method_body(rules, "internal static void Keep<TRecord>")
    note = method_body(S['Trading.cs'], "private void NoteTrade")
    fill = method_body(panel, "private void RefreshTrades")
    bound, have = panel_bindings()
    return ("internal static class Recent" in rules
            and "internal const int MostKept = 20;" in rules
            and ordered(keep, "if (held == null || most <= 0) return;",
                        "held.Insert(0, one);",
                        "while (held.Count > most) held.RemoveAt(held.Count - 1);")
            and 'internal static string Coins(int gold) => gold > 0 ? "+" + gold : gold.ToString();' in rules
            and "public struct TradeNote" in S['LedgerCodec.cs']
            and "Recent.Keep(_lately, new TradeNote" in
                method_body(ledger, "public void NoteTrade")
            and 'dataStore.SyncData("TradeLord_LatelyText", ref _latelyText);' in ledger
            and "_latelyText = LedgerCodec.WriteTrades(_lately);" in ledger
            and "_lately.AddRange(LedgerCodec.ReadTrades(_latelyText, Recent.MostKept));" in ledger
            and "SyncData" not in method_body(ledger, "public void NoteTrade")
            and ordered(note, "if (gold <= 0 || Detail.Count == 0) return;",
                        "foreach (var kv in Detail) units += kv.Value.count;",
                        "selling ? gold : -gold")
            and 'Guard.Run("Pass.NoteTrade"' in method_body(S['Trading.cs'], "internal void Moved")
            and ordered(fill, "var lately = LedgerBehavior.Instance?.Lately;",
                        "{=TL419}", "{=TL418}", "Recent.Coins(note.Gold), note.Gold > 0")
            and {'Trades', 'TradesHeader', 'When', 'Where', 'What', 'Gold', 'Gained', 'Spent'} <= bound
            and {'Trades', 'TradesHeader', 'When', 'Where', 'What', 'Gold', 'Gained', 'Spent'} <= have
            and 'Rules.cs' in TESTPROJ
            and all(one in RECENTTESTS for one in
                    ("The_newest_trade_is_the_one_at_the_top",
                     "The_oldest_trade_falls_off_once_the_list_is_full",
                     "A_list_that_is_already_too_long_is_brought_back_to_its_ceiling",
                     "A_ceiling_of_nothing_keeps_nothing_and_never_throws",
                     "Nowhere_to_keep_a_trade_is_taken_in_its_stride",
                     "The_number_TradeLord_keeps_is_enough_to_read_and_small_enough_to_hold",
                     "Gold_gained_is_marked_and_gold_spent_keeps_its_own_sign",
                     "The_list_never_outgrows_its_ceiling_and_always_holds_the_latest",
                     "new Random(9142)")))


chk("1.80.11", "the panel ends with the last trades TradeLord made for you, kept in your save as plain text so the list survives loading the campaign again",
    the_panel_says_what_it_traded_for_you_lately())



def how_long_a_shelf_lasts_now_counts_towards_the_route_score():
    conf = S['Confidence.cs']
    holds = method_body(conf, "public static float Holds")
    of = method_body(conf, "public static float Of")
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
    return ('TaleWorlds' not in conf
            and "public const float NotKnown = -1f;" in conf
            and "private const float Gone = 0.25f;" in conf
            and "private const float Patience = 2f;" in conf
            and ordered(holds, "if (runsOutInDays < 0f || float.IsNaN(runsOutInDays)) return 1f;",
                        "float slack = runsOutInDays - waited;",
                        "if (slack <= 0f) return Gone;",
                        "return Clamp(Gone + (1f - Gone) * (slack / (slack + Patience)));")
            and ordered(of, "float quiet = runsOutInDays >= 0f",
                        "? Holds(runsOutInDays, daysToTheBuyTown)",
                        ": 1f / (1f + Math.Max(caravans, 0) * 0.15f);")
            and "float c = resilience * depth * haste * quiet * fresh;" in of
            and ordered(scan, "float runsOut = Forecast.RunsOutIn(from, item, onTheShelfNow, q.Units, toBuy);",
                        "runsOut, toBuy);", "RunsOutInDays = runsOut")
            and scan.count("Forecast.RunsOutIn(") == 1
            and '{=TL417}' in S['Panel.cs']
            and 'lowers Conf' in english_string('TL417')
            and 'Confidence.cs' in TESTPROJ
            and all(one in EXPIRYTESTS for one in
                    ("A_shelf_the_forecast_never_sees_empty_is_not_discounted_at_all",
                     "A_shelf_that_empties_before_you_arrive_is_discounted_hardest",
                     "A_shelf_that_empties_the_moment_you_arrive_counts_as_gone",
                     "The_longer_the_shelf_outlasts_your_arrival_the_less_it_is_discounted",
                     "A_discount_is_never_worse_than_gone_and_never_better_than_untouched",
                     "A_negative_journey_is_read_as_no_journey_rather_than_extra_room",
                     "A_route_whose_shelf_holds_is_no_longer_punished_for_the_caravans_going_there",
                     "A_route_whose_shelf_empties_first_scores_below_one_that_lasts",
                     "The_caravan_count_still_decides_it_wherever_the_forecast_is_off",
                     "new Random(4471)")))


chk("1.73.0", "how long a shelf lasts counts towards a route's score in place of the caravans it already counted, and the panel says so",
    how_long_a_shelf_lasts_now_counts_towards_the_route_score())



def the_recent_trades_open_in_a_window_of_their_own():
    import xml.etree.ElementTree as ET
    tree = ET.parse('TradeLord/GUI/Prefabs/TradeLordPanel.xml')
    parent = {c: p for p in tree.iter() for c in p}
    def within(node, attr, value):
        while node is not None:
            if node.get(attr) == value:
                return True
            node = parent.get(node)
        return False
    def only(attr, value):
        found = [e for e in tree.iter() if e.get(attr) == value]
        return found[0] if len(found) == 1 else None
    trades = only('DataSource', '{Trades}')
    header = only('Text', '@TradesHeader')
    opens = only('Command.Click', 'ExecuteOpenTrades')
    panel = S['Panel.cs']
    bound, have = panel_bindings()
    named = {'IsTradesVisible', 'TradesLabel', 'ExecuteOpenTrades', 'ExecuteCloseTrades'}
    return (trades is not None and header is not None and opens is not None
            and within(trades, 'IsVisible', '@IsTradesVisible')
            and within(header, 'IsVisible', '@IsTradesVisible')
            and not within(opens, 'IsVisible', '@IsTradesVisible')
            and not within(opens, 'IsVisible', '@IsVisible')
            and within(opens, 'IsVisible', '@IsMapButtonVisible')
            and PREFAB.count('Text="@TradesLabel"') == 1
            and PREFAB.count('Command.Click="ExecuteCloseTrades"') == 2
            and 'if (!value) { IsTradesVisible = false; IsLegendVisible = false; IsShopsVisible = false; }'
                in panel
            and 'RefreshTrades();' in method_body(panel, "public void ExecuteOpenTrades")
            and named <= bound and named <= have
            and 'TL424' in strings_declared())

chk("1.74.0", "the recent trades open in a window of their own, from a button on the campaign map under the TradeLord one, and close with the ledger",
    the_recent_trades_open_in_a_window_of_their_own())


def every_panel_frame_holds_everything_it_lays_out():
    import xml.etree.ElementTree as ET
    tree = ET.parse('TradeLord/GUI/Prefabs/TradeLordPanel.xml')
    def slack(frame):
        stack = frame.find('Children/BrushWidget/Children/ListPanel')
        if stack is None or stack.find('Children') is None:
            return None
        used = int(stack.get('MarginTop', 0)) + int(stack.get('MarginBottom', 0))
        for kid in stack.find('Children'):
            used += (int(kid.get('SuggestedHeight', 0)) + int(kid.get('MarginTop', 0))
                     + int(kid.get('MarginBottom', 0)))
        return int(frame.get('SuggestedHeight')) - used
    frames = [e for e in tree.iter('ListPanel')
              if e.get('HeightSizePolicy') == 'Fixed' and e.get('SuggestedHeight')
              and e.find('Children/BrushWidget') is not None]
    room = [slack(f) for f in frames]
    return len(frames) == 4 and all(r is not None and r >= 0 for r in room)

chk("1.74.0", "every frame the panel draws is tall enough for everything stacked inside it, so nothing spills past its edge",
    every_panel_frame_holds_everything_it_lays_out())


def the_laid_out_deal_shows_its_gold_and_says_what_it_came_to():
    c = S['Counter.cs']
    opened = method_body(c, "private static bool Opened")
    watch = method_body(c, "internal static TextObject Watch")
    over = method_body(c, "private static void HandTheTotalOver")
    return ("private static readonly Action<int> Ours = whatever => { };" in c
            and ordered(opened, "if (logic.TotalAmountChange == null) logic.TotalAmountChange = Ours;",
                        "_shown = logic;", "_totalHandedOver = false;",
                        "_goldAtOpen = Hero.MainHero?.Gold ?? 0;")
            and ordered(over, "Action<int> reading = _shown.TotalAmountChange;",
                        "if (reading != null && !ReferenceEquals(reading, Ours))",
                        "reading(_shown.TotalAmount);",
                        "if (++_waited < TicksToWaitForTheScreen) return;")
            and ordered(watch, "if (_shown == null) return null;",
                        "if (!_totalHandedOver) HandTheTotalOver();",
                        "if (TheScreenIsStillOurs()) return null;",
                        "int moved = (Hero.MainHero?.Gold ?? _goldAtOpen) - _goldAtOpen;",
                        "Unwatch();", "{=TL423}", "{=TL421}", "{=TL422}")
            and 'Guard.Run("Tick.Counter", TradeActionBehavior.WatchTheTradeScreen);' in S['SubModule.cs']
            and "TextObject closed = Counter.Watch();" in
                method_body(S['Trading.cs'], "internal static void WatchTheTradeScreen")
            and ordered(method_body(c, "private static bool TheScreenIsStillOurs"),
                        "InventoryState open = InventoryScreenHelper.GetActiveInventoryState();",
                        "return open != null && ReferenceEquals(open.InventoryLogic, _shown);")
            and "Unwatch();" in method_body(c, "internal static void Forget")
            and "Unwatch();" in method_body(c, "internal static bool Ready")
            and all(sid in strings_declared() for sid in ('TL421', 'TL422', 'TL423'))
            and all('{GOLD}' in english_string(sid) for sid in ('TL421', 'TL422')))

chk("1.74.0", "the trade screen is handed what the laid out deal comes to, and the deal says what your purse did once you close it",
    the_laid_out_deal_shows_its_gold_and_says_what_it_came_to())


def the_staged_trading_switch_is_named_the_same_everywhere():
    label = english_string('TL281')
    return (label == 'Staged Trading'
            and '"{=TL281}' + label + '"' in M
            and '- \u2705 ' + label + ', off out of the box:' in README
            and 'Lay the trade out' not in README
            and 'Lay the trade out' not in M
            and 'Lay the trade out' not in ALL)

chk("1.74.0", "the switch that lays the deal out is called Staged Trading on the settings screen and in the feature list alike",
    the_staged_trading_switch_is_named_the_same_everywhere())


def the_workshop_limit_is_lifted_by_one_patch_and_one_setting():
    w = S['Workshops.cs']
    rule = method_body(S['Rules.cs'], "public static int WorkshopsYouMayOwn")
    return ('[HarmonyPatch(typeof(DefaultWorkshopModel), "GetMaxWorkshopCountForClanTier")]' in w
            and "__result = Holdings.WorkshopsYouMayOwn(__result, Options.Current.MaxWorkshopsOwned);" in w
            and ordered(rule, "if (youAsked <= 0) return gameSays;", "return youAsked;")
            and "TaleWorlds" not in method_body(S['Rules.cs'], "public static class Holdings")
            and option_default('MaxWorkshopsOwned') == '200'
            and "_o.MaxWorkshopsOwned" in M
            and 'Patcher.TryPatch(harmony, typeof(Patch_WorkshopLimit));' in S['SubModule.cs']
            and 'Rules.cs' in TESTPROJ
            and all(one in HOLDINGTESTS for one in
                    ("Asking_for_nothing_leaves_the_game_to_say_how_many_you_may_own",
                     "The_number_you_ask_for_is_the_number_you_get",
                     "There_is_room_for_one_more_until_you_are_at_the_ceiling",
                     "A_ceiling_of_nothing_leaves_no_room_at_all",
                     "What_you_ask_for_never_depends_on_what_the_game_says",
                     "new System.Random(3316)")))

chk("1.76.0", "the limit the game puts on how many workshops you may own is lifted by one patch reading one setting, and the rule behind it is proved by tests the build runs",
    the_workshop_limit_is_lifted_by_one_patch_and_one_setting())


def a_workshop_is_bought_from_the_panel_through_the_games_own_action():
    w = S['Workshops.cs']
    buy = method_body(w, "internal static bool Buy")
    stops = method_body(w, "internal static Block WhatStopsBuying")
    row = method_body(S['Panel.cs'], "public void ExecuteBuy")
    bound, have = panel_bindings()
    named = {'IsShopsVisible', 'ShopsLabel', 'ShopsHeader', 'Shops', 'ExecuteOpenShops',
             'ExecuteCloseShops', 'ExecuteBuy', 'BuyLabel', 'Affordable', 'Dear', 'Cost'}
    return (ordered(stops, "if (shop == null || !OnTheMarket(shop)) return Block.NotTradable;",
                    "if (!Holdings.RoomForOneMore(owned, mayOwn)) return Block.HeldEnough;",
                    "if (cost > purse) return Block.BudgetSpent;",
                    "return Block.None;")
            and "owner != null && owner != Hero.MainHero && owner.IsNotable && !owner.IsDead" in
                between(w, "internal static bool OnTheMarket", "\n        }")
            and ordered(buy, "Block why = WhatStopsBuying(shop, cost, purse, owned, mayOwn);",
                        "if (why != Block.None)", "ChangeOwnerOfWorkshopAction.ApplyByPlayerBuying(shop);",
                        "done = shop.Owner == Hero.MainHero;")
            and "model.GetCostForPlayer(shop)" in method_body(w, "internal static int CostOf")
            and "InformationManager.ShowInquiry" in row and "{=TL438}" in row
            and w.count("ChangeOwnerOfWorkshopAction.ApplyByPlayerBuying(") == 1
            and named <= bound and named <= have
            and all(i in strings_declared() for i in
                    ('TL431', 'TL433', 'TL434', 'TL435', 'TL436', 'TL437', 'TL438', 'TL439', 'TL440')))

chk("1.76.0", "a workshop is bought from the panel through the game's own buying action, behind a yes or no, and never past your purse or the ceiling you set",
    a_workshop_is_bought_from_the_panel_through_the_games_own_action())


def the_legend_reads_one_clause_to_a_line_in_a_window_of_its_own():
    import xml.etree.ElementTree as ET
    tree = ET.parse('TradeLord/GUI/Prefabs/TradeLordPanel.xml')
    parent = {c: p for p in tree.iter() for c in p}
    def within(node, attr, value):
        while node is not None:
            if node.get(attr) == value:
                return True
            node = parent.get(node)
        return False
    legend = [e for e in tree.iter() if e.get('Text') == '@LegendText']
    button = [e for e in tree.iter() if e.get('Command.Click') == 'ExecuteOpenLegend']
    panel = S['Panel.cs']
    return (len(legend) == 1 and len(button) == 1
            and within(legend[0], 'IsVisible', '@IsLegendVisible')
            and not within(button[0], 'IsVisible', '@IsLegendVisible')
            and within(button[0], 'IsVisible', '@IsVisible')
            and 'said.Replace(" | ", "\\n")' in
                between(panel, "private static string OneClauseToALine", ";")
            and "LegendText = OneClauseToALine(LegendText);" in
                method_body(panel, "private void Refresh()")
            and 'TL432' in strings_declared())

chk("1.76.0", "the line under the routes is gone from the ledger and reads one clause to a line in a window of its own",
    the_legend_reads_one_clause_to_a_line_in_a_window_of_its_own())


def the_deal_you_took_is_reported_and_credited_like_any_pass():
    t = S['Trading.cs']
    took = method_body(t, "internal static void TookTheDeal")
    sold = method_body(t, "private static void ReportWhatYouSold")
    ledger = method_body(S['Ledger.cs'], "private void OnPlayerInventoryExchange")
    reckon = method_body(t, "private static Took Reckon")
    return (ordered(took, "if (settlement?.SettlementComponent == null) return;",
                    "int purseMoved = Counter.PurseMovedOnTheScreen();",
                    "Took got = Reckon(selling, sold, true);",
                    "Took paid = Reckon(buying, bought, false);",
                    "bool addsUp = Deals.AddsUp(got.Gold - paid.Gold, purseMoved);",
                    "ReportWhatYouSold(selling, got, addsUp);",
                    "ReportWhatYouBought(buying, paid, addsUp);")
            and ordered(reckon, "int count = Deals.UnitsMoved(el.Amount, said, price);",
                        "took.Gold += said;",
                        "TradeMath.Credit(price, TradePolicy.WorthToBeat(item),",
                        "pass.Tally(item, count, said);",
                        "took.Profit = Deals.NoMoreThanTheSale(took.Profit, took.Gold);")
            and "gained += price * count;" not in t and "spent += price * count;" not in t
            and ordered(sold, "pass.Moved(addsUp ? (int?)got.Profit : null, got.Gold, selling: true);",
                        "{=TL02}")
            and "AwardTradeXpForOurOwnTrade(" not in sold
            and ordered(ledger, "if (Counter.Awaiting)",
                        "TradeActionBehavior.TookTheDeal(purchased, sold)",
                        "foreach (var (element, said) in sold)")
            and "internal static bool Awaiting => _shown != null;" in S['Counter.cs']
            and "_shown == null ? 0 : (Hero.MainHero?.Gold ?? _goldAtOpen) - _goldAtOpen;" in S['Counter.cs']
            and t.count("AwardTradeXpForOurOwnTrade(") == 3)

chk("1.76.2", "the deal you took is read in the gold the game hands over, squared against what your purse actually did, and put to your total only when the two agree",
    the_deal_you_took_is_reported_and_credited_like_any_pass())


def the_workshop_limit_is_lifted_for_your_clan_alone():
    rules = S['Rules.cs']
    shops = S['Workshops.cs']
    tier = method_body(shops, "private static void Postfix(int tier, ref int __result)")
    return ("public static bool TheGameIsAskingAboutYou(int askedAboutTier, int yourTier," in rules
            and "bool whileYouBuy) =>" in rules
            and "whileYouBuy && yourTier >= 0 && askedAboutTier == yourTier;" in rules
            and "if (!Holdings.TheGameIsAskingAboutYou(tier, Shops.YourTier(), Shops.ItIsYouBuying)) return;"
                in tier
            and '[HarmonyPatch(typeof(DefaultWorkshopModel), "MaximumWorkshopsPlayerCanHave", MethodType.Getter)]' in shops
            and "Patcher.TryPatch(harmony, typeof(Patch_WorkshopsYouMayHave));" in S['SubModule.cs']
            and '"get_MaximumWorkshopsPlayerCanHave"' in COMPAT
            and 'Rules.cs' in TESTPROJ
            and all(one in HOLDINGTESTS for one in
                    ("The_limit_is_only_lifted_where_the_game_is_asking_about_your_own_clan",
                     "A_clan_the_game_cannot_place_never_has_the_limit_lifted_for_it",
                     "No_tier_but_your_own_is_ever_lifted_whatever_the_game_asks",
                     "new System.Random(7715)")))


chk("1.76.1", "the workshop limit is only ever lifted where the game is asking about your own clan, and through the member that is yours alone",
    the_workshop_limit_is_lifted_for_your_clan_alone())


def buying_a_workshop_says_when_it_eats_into_your_reserve():
    rules = S['Rules.cs']
    buy = method_body(S['Panel.cs'], "public void ExecuteBuy")
    return ("public static bool DipsIntoWhatYouHoldBack(int cost, int purse, int heldBack) =>" in rules
            and "cost > 0 && heldBack > 0 && purse - cost < heldBack;" in rules
            and ordered(buy, "int heldBack = TradeActionBehavior.GoldHeldBack();",
                        "if (Holdings.DipsIntoWhatYouHoldBack(_cost, Hero.MainHero?.Gold ?? 0, heldBack))",
                        '{=TL441}', 'warned.SetTextVariable("HELD"', "body += warned.ToString();")
            and "new TextObject(" not in buy
            and said_in_every_language("TL441")
            and all(one in HOLDINGTESTS for one in
                    ("A_purchase_that_leaves_the_reserve_whole_is_not_warned_about",
                     "A_purchase_that_eats_into_the_reserve_is_warned_about",
                     "Holding_nothing_back_means_there_is_nothing_to_warn_about",
                     "The_warning_follows_what_is_left_rather_than_what_it_costs",
                     "new System.Random(3308)")))


chk("1.76.1", "buying a workshop says so when it takes you below the gold reserve, and still lets you",
    buying_a_workshop_says_when_it_eats_into_your_reserve())



def what_a_trade_moved_is_never_read_as_the_gold_it_fetched():
    rules = S['Rules.cs']
    units = method_body(rules, "public static int UnitsMoved")
    panel = method_body(S['Panel.cs'], "private static string DayOf")
    return ("public static class Deals" in rules
            and ordered(units, "if (amount > 0) return amount;",
                        "if (price > 0 && gold > 0) return Math.Max(1, gold / price);",
                        "return gold > 0 ? 1 : 0;")
            and "Math.Abs((long)reckonedNet - purseMoved) <= Slack + Math.Abs((long)purseMoved) / 100L;" in rules
            and "if (profit <= 0 || gained <= 0) return 0;" in rules
            and "internal static int DaysAgo(float then, float now)" in rules
            and ordered(panel, "int ago = Recent.DaysAgo(day, (float)CampaignTime.Now.ToDays);",
                        "{=TL442}", "{=TL420}")
            and "{=TL420}Day {DAY}" not in S['Panel.cs']
            and said_in_every_language("TL420") and said_in_every_language("TL442")
            and 'Rules.cs' in TESTPROJ
            and all(one in DEALTESTS for one in
                    ("The_count_the_game_hands_over_is_taken_when_it_has_one",
                     "With_no_count_to_go_on_the_units_come_from_the_gold_and_the_price",
                     "A_line_worth_less_than_one_unit_still_counts_as_one",
                     "A_line_with_nothing_on_it_moves_nothing",
                     "The_deal_that_broke_the_ledger_no_longer_adds_up_and_says_so",
                     "The_deal_as_it_really_was_adds_up_against_the_purse",
                     "A_denar_or_two_of_rounding_is_allowed_and_a_wild_figure_is_not",
                     "A_large_deal_is_allowed_the_same_small_share_of_slack",
                     "Profit_can_never_be_more_than_the_sale_that_earned_it",
                     "Nothing_reckoned_from_a_line_ever_outruns_what_the_purse_did",
                     "A_trade_from_today_reads_as_today_and_an_older_one_counts_the_days",
                     "A_day_the_game_cannot_work_out_reads_as_today_rather_than_a_wild_number",
                     "new Random(8812)")))


chk("1.76.2", "what a trade moved is never read as the gold it fetched, and the ledger dates a trade by how long ago it was",
    what_a_trade_moved_is_never_read_as_the_gold_it_fetched())



def nothing_the_game_hands_back_is_read_as_a_count_without_the_one_door():
    t, l = S['Trading.cs'], S['Ledger.cs']
    both = t + l
    lists = re.findall(r'foreach \(var \((\w+), (\w+)\) in (purchased|sold)\)', both)
    return (len(lists) == 2
            and all(named == 'said' for _, named, _ in lists)
            and 'var (el, said) = lines[i];' in t
            and both.count('Deals.UnitsMoved(') == 3
            and ordered(l, 'int gone = Deals.UnitsMoved(element.Amount, said, unit);',
                        'moved.Add((item, gone));', 'RecordSale(item.StringId, gone);')
            and 'int bought = Deals.UnitsMoved(element.Amount, said, unit);' in l
            and 'int count = Deals.UnitsMoved(el.Amount, said, price);' in t
            and 'RecordSale(item.StringId, said)' not in l
            and 'RecordSale(item.StringId, count)' not in l
            and 'RecordPurchase(item.StringId, said' not in l
            and all(one not in both for one in
                    ('foreach (var (element, count) in purchased)',
                     'foreach (var (element, count) in sold)',
                     'var (el, count) = lines[i];')))


chk("1.76.3", "a figure the game hands back for a traded line is read as gold and turned into units by one rule, never taken for a count on the spot",
    nothing_the_game_hands_back_is_read_as_a_count_without_the_one_door())



def a_workshop_the_game_did_not_charge_for_is_still_paid_for():
    rules = S['Rules.cs']
    buy = method_body(S['Workshops.cs'], "internal static bool Buy")
    return ("public static int StillOwedForTheWorkshop(int cost, int paid) =>" in rules
            and "cost <= 0 || paid > 0 ? 0 : cost;" in rules
            and ordered(buy, "Hero seller = shop.Owner;",
                        "int before = Hero.MainHero?.Gold ?? 0;",
                        "ChangeOwnerOfWorkshopAction.ApplyByPlayerBuying(shop);",
                        "int paid = before - (Hero.MainHero?.Gold ?? before);",
                        "int owed = Holdings.StillOwedForTheWorkshop(cost, paid);",
                        "GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, seller, owed, true);")
            and 'Rules.cs' in TESTPROJ
            and all(one in HOLDINGTESTS for one in
                    ("A_workshop_the_game_charged_for_is_never_charged_again",
                     "A_workshop_the_game_handed_over_for_nothing_is_still_paid_for",
                     "A_workshop_with_no_price_on_it_is_never_charged_for",
                     "Nothing_is_ever_charged_twice_however_the_game_behaves",
                     "new System.Random(6193)")))


chk("1.76.4", "a workshop the game hands over without taking the gold for it is paid for all the same, and one it charged for is never charged twice",
    a_workshop_the_game_did_not_charge_for_is_still_paid_for())



def a_half_published_version_never_blocks_the_run_that_would_finish_it():
    at = WORKFLOW.find("- name: Publish release")
    step = "" if at < 0 else WORKFLOW[at:]
    tidying = [one.strip() for one in step.split("\n")
               if one.strip().startswith("gh release delete")]
    return ("if version == shipping and (row['draft'] or row['files'] == 0):" in RELEASED
            and "is part-published from an attempt that did not finish" in RELEASED
            and "' is still a draft release'" in RELEASED
            and "' was published with no file attached'" in RELEASED
            and len(tidying) == 2 and all("||" in one for one in tidying)
            and WORKFLOW.index("released.py") < WORKFLOW.index("- name: Publish release"))


chk("1.76.5", "a version left half published by an attempt that did not finish never blocks the run that would finish it, and tidying it cannot end that run",
    a_half_published_version_never_blocks_the_run_that_would_finish_it())



def the_few_markets_handed_out_are_never_the_ranked_list_itself():
    few = between(S['Ranking.cs'], "internal static List<T> TopFew<T>(", "\n        }")
    taken = between(S['Ledger.cs'], "private static List<(Settlement, int)> TakeN(", ";")
    return ("ranked.GetRange(0, take)" in few
            and "return new List<T>()" in few
            and "most > ranked.Count ? ranked.Count : most" in few
            and "return ranked;" not in few
            and "MarketRank.TopFew(list, n)" in taken
            and "list.Count <= n ? list" not in taken
            and "TopSell" in S['TooltipPatches.cs']
            and "markets[i] = (town," in S['TooltipPatches.cs'])


chk("1.76.5", "the best markets handed to a tooltip are a list of its own, so working the forecast into them never reaches what the ledger recorded",
    the_few_markets_handed_out_are_never_the_ranked_list_itself())


def a_moment_is_looked_up_rather_than_searched_for():
    moments = between(S['Projection.cs'], "internal static List<float> Moments(", "\n        }")
    note = between(S['Projection.cs'], "private static void Note(List<float> when,", "\n        }")
    return ("HashSet<float>" in moments
            and "already" in moments
            and "!already.Add(at)" in note
            and "when.Contains(" not in note
            and "when.Sort();" in moments)


chk("1.76.5", "a moment several things land on is found by looking it up, never by walking every moment already counted",
    a_moment_is_looked_up_rather_than_searched_for())


def a_shelf_that_cannot_be_walked_is_never_walked():
    at = between(S['Market.cs'], "internal int At(int taken)", "\n        }")
    price = between(S['Market.cs'], "internal int Price()", "\n        }")
    restock = between(S['Market.cs'], "internal void Restock(int units)", "\n        }")
    return ("if (!_shelf.Walkable) return _shelf.Price();" in at
            and at.index("if (!_shelf.Walkable)") < at.index("while (_priced.Count <= taken)")
            and "if (!_walkable) return _quoted;" in price
            and "if (!_walkable) return;" in restock)


chk("1.76.5", "a price that cannot move with what is taken off a shelf is read once, not once for every unit",
    a_shelf_that_cannot_be_walked_is_never_walked())

def the_workshop_limit_is_only_lifted_while_it_is_you_buying():
    shops = S['Workshops.cs']
    rule = between(S['Rules.cs'], "public static bool TheGameIsAskingAboutYou", ";")
    tier = method_body(shops, "private static void Postfix(int tier, ref int __result)")
    window = method_body(shops, "internal static void WhileItIsYouBuying")
    return ("whileYouBuy &&" in rule
            and "Shops.ItIsYouBuying" in tier
            and "internal static bool ItIsYouBuying => _youBuying > 0;" in shops
            and ordered(window, "_youBuying++;", "try { work(); }", "finally { _youBuying--; }")
            and "internal static void ForgetWhoIsBuying() => _youBuying = 0;" in shops
            and shops.count("WhileItIsYouBuying(") == 2
            and "WhileItIsYouBuying(" in method_body(shops, "internal static bool Buy")
            and "Shops.ForgetWhoIsBuying();" in
                method_body(S['Trading.cs'], "internal static void ForgetVisit")
            and all(one in HOLDINGTESTS for one in
                    ("A_clan_sitting_at_your_own_tier_keeps_its_own_limit_when_you_are_not_buying",
                     "Nothing_the_game_asks_lifts_a_limit_while_you_are_not_buying",
                     "new System.Random(48802)")))


chk("1.76.6", "the workshop limit is lifted only while it is you buying one, so a clan sitting at your own tier keeps the limit the game gives it",
    the_workshop_limit_is_only_lifted_while_it_is_you_buying())


def the_marker_reads_your_cargo_once_and_prices_each_market_once():
    t = S['Trading.cs']
    carried = method_body(S['Marker.cs'],
                          "private static List<(EquipmentElement item, int amount, int worth, int floor)> "
                          "WhatYouCarryToSell")
    asked = method_body(S['Marker.cs'], "internal int At(int taken)")
    forget = method_body(S['Marker.cs'], "internal static void ForgetTheRead")
    carry = method_body(S['Marker.cs'], "internal static void ForgetWhatYouCarry")
    return (ordered(carried,
                    "int version = party.ItemRoster.VersionNo;",
                    "if (_cargo != null && Freshness.Fresh(ref _cargoStamp) &&",
                    "version == _cargoVersion) return _cargo;",
                    "TradePolicy.KeptBack(party.ItemRoster, TradeActionBehavior.TheVisit,",
                    "_cargo = cargo;")
            and ordered(asked, "while (_rungs.Count <= taken) _rungs.Add(Next());",
                        "return _rungs[taken];")
            and "_flat = Priced.At(_market, _el, _party, true);" in
                method_body(S['Marker.cs'], "private int Next()")
            and "_prices" not in S['Marker.cs']
            and "_priceStamp" not in S['Marker.cs']
            and "PriceShelfHours" not in S['Marker.cs']
            and ordered(carry, "_cargo = null;", "_cargoStamp.Stale();", "_cargoVersion = -1;")
            and forget.count("ForgetWhatYouCarry();") == 1
            and t.count("Marker.ForgetWhatYouCarry();") == 2
            and all("Marker.ForgetWhatYouCarry();" in method_body(t, where)
                    for where in ("private static void ResetVisit",
                                  "private void OnSettlementLeft"))
            and "Marker.Forget();" in method_body(t, "internal static void ForgetVisit")
            and '"VersionNo"' in COMPAT)


chk("1.90.0", "the market marked on your map reads what you carry once an hour and reads it again the moment your cargo can have moved, and asks every market its price live rather than keeping one it read earlier",
    the_marker_reads_your_cargo_once_and_prices_each_market_once())


def the_name_a_good_is_shown_by_is_read_once():
    policy = S['Policy.cs']
    spoken = method_body(policy, "private static string SpokenName")
    describe = method_body(policy, "internal static Good Describe")
    return ("good.Name = SpokenName(item);" in describe
            and "item.Name.ToString()" not in describe
            and ordered(spoken, "if (_spoken.TryGetValue(item, out string said)) return said;",
                        "said = item.Name == null ? null : item.Name.ToString();",
                        "_spoken[item] = said;")
            and "_spoken.Clear();" in
                method_body(policy, "internal static void ForgetItemListAudit"))


chk("1.76.6", "the name a good is shown by is read from the game once and kept, however many times the selling rules ask for it",
    the_name_a_good_is_shown_by_is_read_once())


def a_town_shelf_is_read_ahead_once_for_every_deal_that_asks_it():
    f = S['Forecast.cs']
    kept = method_body(f, "private static List<(float days, int shelf)> ShelfAhead")
    ask = method_body(f, "internal static float RunsOutIn")
    return ("Dictionary<(string site, string item, int stockNow, float afterDays)," in f
            and "_shelfAhead" in f
            and "var key = (site.StringId, item.StringId, stockNow, afterDays);" in kept
            and "if (_shelfAhead.TryGetValue(key, out List<(float days, int shelf)> curve)) return curve;"
                in kept
            and "_shelfAhead[key] = curve;" in kept
            and "return Projection.RunsOutOf(ShelfAhead(site, item, stockNow, afterDays), wanted);" in ask
            and "Projection.RunsOutAt(" not in f
            and f.count("_shelfAhead.Clear();") == 2
            and "_shelfAhead.Clear();" in method_body(f, "private static void Build")
            and "_shelfAhead.Clear();" in method_body(f, "internal static void Forget"))


chk("1.76.6", "how long a buy market holds a quantity is worked out once for that market and arrival, and answered from it for every size of deal the scan tries",
    a_town_shelf_is_read_ahead_once_for_every_deal_that_asks_it())



def the_price_trace_is_written_in_one_burst():
    written = method_body(S['Market.cs'], "private static void Written")
    return ("var lines = new List<string>();" in written
            and "Log.Write(" not in written
            and written.count("lines.Add(") >= 6
            and written.count("Log.WriteMany(lines);") == 1
            and written.index("Log.WriteMany(lines);") > written.rindex("lines.Add("))


chk("1.76.7", "the price trace reaches the log in one burst rather than a line at a time, so a market it reads costs one push of the file and not one for every good",
    the_price_trace_is_written_in_one_burst())


def the_markets_behind_a_screen_are_priced_once_for_the_screen():
    prime = method_body(S['Market.cs'], "internal static void Prime")
    gather = method_body(S['Market.cs'], "private static void Gather")
    tip = S['TooltipPatches.cs']
    markets = method_body(tip, "Markets(ItemVM itemVm)")
    coloured = method_body(tip, "private static void Coloured")
    return (ordered(prime,
                    "if (ledger == null || !Options.Current.Omniscient) return;",
                    "if (at == _primedAt && Freshness.Fresh(ref _primedStamp)) return;",
                    "Gather(MobileParty.MainParty == null ? null : MobileParty.MainParty.ItemRoster, goods);",
                    "Gather(here == null ? null : here.ItemRoster, goods);",
                    "if (goods.Count > 0) ledger.PrimeMarketsFor(goods);")
            and "if (item != null && TradePolicy.Priced(item)) goods.Add(item);" in gather
            and ordered(markets, "ScreenMarkets.Prime();",
                        "ledger.TopSell(item, MarketRank.TopCacheSize)")
            and ordered(coloured, "ScreenMarkets.Prime();", "ledger.BestBuy(item)")
            and ordered(method_body(tip, "private static bool Sectioned"),
                        "ScreenMarkets.Prime();", "ledger.AnyMarketFor(item)")
            and tip.count("ScreenMarkets.Prime();") == 3
            and 'Guard.Run("GameEnd.ScreenMarkets", ScreenMarkets.Forget);' in S['SubModule.cs'])


chk("1.76.7", "the markets behind the item tooltips and the inventory colours are priced once for the market you are standing in, not once for every good on the screen",
    the_markets_behind_a_screen_are_priced_once_for_the_screen())



def the_log_bursts_a_trade_writes_reach_the_file_in_one_go():
    detail = method_body(S['Trading.cs'], "private static void LogDetail")
    promise = method_body(S['Hindsight.cs'], "private static void Kept")
    forecast = method_body(S['Hindsight.cs'], "private static void Written")
    bursts = [detail, promise, forecast]
    return (all("Log.Write(" not in b for b in bursts)
            and all(b.count("Log.WriteMany(lines);") == 1 for b in bursts)
            and all(b.index("Log.WriteMany(lines);") > b.rindex("lines.Add(") for b in bursts)
            and "lines.Insert(0, \"promise check at \"" in promise
            and "lines.Insert(0, \"forecast check at \"" in forecast
            and "for (int i = 0; i < lines.Count; i++) Log.Write(lines[i]);" not in S['Hindsight.cs'])


chk("1.76.8", "the lines TradeLord writes as it trades and as it scores what it promised you reach the log in one go, not one push of the file for every good",
    the_log_bursts_a_trade_writes_reach_the_file_in_one_go())



def a_record_from_a_newer_version_is_read_as_far_as_this_one_understands_it():
    codec = S['LedgerCodec.cs']
    purchases = method_body(codec, "public static List<PurchaseRecord> ReadPurchases")
    return ("public const int FieldsAPurchaseNeeds = 4;" in codec
            and "if (parts.Length < FieldsAPurchaseNeeds || !Storable(parts[0])) continue;" in purchases
            and "parts.Length != 4" not in codec
            and purchases.count("continue;") == 3
            and "A_record_written_by_a_newer_TradeLord_is_read_as_far_as_this_one_understands_it" in TESTS
            and '[InlineData("grain|340|20|17|9")]' not in TESTS)


chk("1.76.9", "a record of what you paid written by a newer TradeLord is read as far as this one understands it rather than dropped, the way a recorded price already was",
    a_record_from_a_newer_version_is_read_as_far_as_this_one_understands_it())



def a_whip_that_cracks_writes_the_file_back_so_it_never_cracks_twice():
    read = method_body(S['Config.cs'], "private static void Read")
    write = method_body(S['Config.cs'], "private static void Write(string path, string why)")
    return (ordered(read, "Whip.Crack(shape, written);",
                    "else if (whipped)",
                    'Write(found, "every setting put back to what TradeLord ships with");')
            and ordered(write, "new KeyValuePair<string, string>(Migration.ShapeKey,",
                        "Migration.Shape.ToString(CultureInfo.InvariantCulture)),")
            and "AFileAlreadyAtTheShapeThisVersionShipsIsNeverResetBySecondTime" in MIGRATIONTESTS
            and "TheWhipIsWiredToTheShapeItWasArmedAtAndIsSpentOnceALaterShapeShips" in MIGRATIONTESTS)


chk("1.77.1", "a settings file put back to what TradeLord ships with is written out again carrying the shape this version ships, so it is put back once and never again",
    a_whip_that_cracks_writes_the_file_back_so_it_never_cracks_twice())



def walking_through_a_gate_keeps_the_prices_the_marker_read():
    t = S['Trading.cs']
    left = method_body(t, "private void OnSettlementLeft")
    reset = method_body(t, "private static void ResetVisit")
    launched = method_body(t, "private void OnSessionLaunched")
    ended = method_body(t, "internal static void ForgetVisit")
    gates = [left, reset]
    return (t.count("Marker.ForgetTheRead();") == 1
            and "Marker.Forget();" in ended
            and ordered(launched, "ResetVisit();", "Marker.ForgetTheRead();")
            and all("Marker.ForgetTheRead();" not in gate for gate in gates)
            and all("Marker.Forget();" not in gate for gate in gates)
            and all("Marker.ForgetWhatYouCarry();" in gate for gate in gates))


chk("1.77.1", "walking into a market or out of it drops what TradeLord read of your cargo and keeps the prices it read of every other market, which only a new campaign or the end of one drops",
    walking_through_a_gate_keeps_the_prices_the_marker_read())



def no_cache_can_forget_to_ask_whether_a_setting_moved():
    stamp = method_body(S['Rules.cs'], "internal struct Stamp")
    fresh = method_body(S['Support.cs'], "internal static class Freshness")
    read = {f: S[f].count("Options.Generation") for f in S}
    kept = [f for f in sorted(read) if read[f] > 0]
    return (kept == ['Support.cs']
            and S['Support.cs'].count("Options.Generation") == read['Support.cs']
            and all(line in fresh for line in
                    ("internal static int Hour => (int)CampaignTime.Now.ToHours;",
                     "internal static bool Fresh(ref Stamp stamp) => stamp.Fresh(Hour, Options.Generation);",
                     "internal static void Taken(ref Stamp stamp) => stamp.Taken(Hour, Options.Generation);",
                     "internal static bool Held(Stamp stamp, int hour) => stamp.Fresh(hour, Options.Generation);"))
            and "Options.Generation" not in stamp
            and "CampaignTime" not in S['Rules.cs']
            and "internal const int Timeless = int.MinValue;" in stamp
            and "_taken && _hour == hour && _generation == generation;" in stamp
            and sum(S[f].count("Stamp _") for f in S) == 9
            and "(Stamp stamp, string kind, List<(Settlement, int)> markets)> _marketCache" in S['Ledger.cs']
            and all(one in STAMPTESTS for one in
                    ("A_stamp_nobody_has_taken_is_never_fresh",
                     "The_hour_moving_on_makes_a_stamp_stale",
                     "A_setting_changing_makes_a_stamp_stale_within_the_same_hour",
                     "A_timeless_stamp_keeps_through_every_hour_and_still_answers_to_a_setting",
                     "Zero_is_a_real_hour_and_a_real_generation_rather_than_nothing_taken_yet")))


chk("1.77.2", "every cache the mod keeps asks one tested rule whether the hour or a setting has moved, and nothing else in the source may read that setting counter at all",
    no_cache_can_forget_to_ask_whether_a_setting_moved())


def the_marker_walks_the_richest_purses_first_and_stops_at_a_town_till():
    marker = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    asked = method_body(S['Marker.cs'], "internal int At(int taken)")
    update = method_body(S['Marker.cs'], "internal static void Update")
    return ("reachable.Sort(FastestPurseFirst);" in marker
            and "if (TradeMath.PerDay(gold, ride) <= bar) break;" in marker
            and "continue;" not in between(marker, "if (TradeMath.PerDay(gold, ride)", "\n")
            and marker.find("float cap = LedgerBehavior.TravelCeiling(s);") <
                marker.find("reachable.Sort(FastestPurseFirst);")
            and "if (took.Value + fetched >= gold) { took.PurseCapped = true; break; }" in
                method_body(S['Marker.cs'], "private static Takings WhatItWouldFetch")
            and "how.PurseCapped = took.PurseCapped;" in marker
            and 'string why = on ? Why(how) : "the map marker is switched off";' in update
            and 0 <= update.find("if (target == _picked)") < update.find("string why = on ?")
            and "_picked = target;" in update
            and "Why(how)" not in between(update, "bool on =", "if (target == _picked)"))


chk("1.77.2", "the map marker walks the purses that would earn fastest first and stops as soon as no market left can beat the best it found, stops pricing a town once its own purse is the ceiling, and works out what to write in the log only when the marker actually moves",
    the_marker_walks_the_richest_purses_first_and_stops_at_a_town_till())


def a_market_is_trusted_by_what_it_has_really_paid():
    ledger = S['Ledger.cs']
    scan = method_body(ledger, "private List<TradeRoute> ScanRoutes()")
    kept = method_body(ledger, "internal void KeepArrival")
    read = method_body(ledger, "internal bool PromiseScoreAt")
    moved = method_body(S['Confidence.cs'], "public static float AsPromisesHaveHeld")
    return (option_default('TrustWhatAMarketPaid') == 'true'
            and "_o.TrustWhatAMarketPaid" in M
            and EVER_SHIPPED.get('TrustWhatAMarketPaid') == 'bool'
            and "TradeMath.AddPromise(rec, held);" in kept
            and 'rec = new PromiseRecord { TownId = townId };' in kept
            and "if (townId == null || held < 0f) return;" in kept
            and "float mean = TradeMath.PromiseMean(rec);" in read
            and ordered(scan, "bool trustWhatItPaid = Options.Current.TrustWhatAMarketPaid;",
                        "float score = perDay * confidence;",
                        "if (trustWhatItPaid &&",
                        "PromiseScoreAt(to.StringId, out int arrivals, out float heldThere))",
                        "score = Confidence.AsPromisesHaveHeld(score, arrivals, heldThere);",
                        "float key = rankByScore ? score : perDay;")
            and "Score = score," in scan
            and "perDay * confidence" not in scan[scan.index("Score = score,"):]
            and ordered(moved, "if (score <= 0f || arrivals < Confidence.EnoughArrivals) return score;".replace("Confidence.", ""),
                        "float trust = held > 1f ? 1f : held;",
                        "float weight = (float)arrivals / (arrivals + EnoughArrivals);",
                        "float factor = 1f - (1f - trust) * weight;",
                        "float floor = 1f - MostItDiscounts;")
            and "public const int EnoughArrivals = 5;" in S['Confidence.cs']
            and "public const float MostItDiscounts = 0.25f;" in S['Confidence.cs']
            and "TaleWorlds" not in S['Confidence.cs']
            and 'dataStore.SyncData("TradeLord_PromiseText", ref _promiseText);' in
                method_body(ledger, "public override void SyncData")
            and "_promises = KeyedByTownId(LedgerCodec.ReadPromises(_promiseText));" in
                method_body(ledger, "private void RestoreSaved")
            and "{=TL445}" in method_body(S['Panel.cs'], "private void Refresh")
            and said_in_every_language('TL443') and said_in_every_language('TL444')
            and said_in_every_language('TL445')
            and all(one in PROMISETESTS for one in
                    ("Too_few_arrivals_leave_a_score_exactly_where_it_was",
                     "Paying_above_the_promise_is_never_a_bonus",
                     "More_arrivals_make_the_same_shortfall_count_for_more",
                     "A_shortfall_can_never_move_a_score_by_more_than_the_ceiling_it_is_held_to",
                     "What_a_market_paid_survives_a_save_and_a_load",
                     "A_record_written_by_a_newer_TradeLord_is_read_as_far_as_this_one_understands_it")))


chk("1.78.0", "a market that has paid less than the panel promised is scored lower, from what it really paid on your own arrivals, kept in the save and never moving a route's score by more than a quarter",
    a_market_is_trusted_by_what_it_has_really_paid())


def a_markets_record_counts_your_walk_ins_not_the_prices_it_checked():
    kept = method_body(S['Hindsight.cs'], "private static void Kept")
    ledger = S['Ledger.cs']
    arrival = method_body(ledger, "internal void KeepArrival")
    tally = method_body(ledger, "internal void KeepPromiseScore")
    enough = re.search(r'public const int EnoughArrivals = (\d+);', S['Confidence.cs'])
    said = spoken(ENGLISH)
    words = {5: 'five', 6: 'six', 7: 'seven', 8: 'eight', 9: 'nine', 10: 'ten'}
    return (enough is not None
            and kept.count("LedgerBehavior.Instance?.KeepArrival(") == 1
            and kept.count("LedgerBehavior.Instance?.KeepPromiseScore(") == 1
            and kept.index("LedgerBehavior.Instance?.KeepArrival(") >
                kept.rindex("heldTotal += held;")
            and kept.index("LedgerBehavior.Instance?.KeepArrival(") <
                kept.index("if (!Writing || scored + stale + yours + unpriced == 0) return;")
            and "TradeMath.MeanOf(heldTotal, scored));" in kept
            and "if (scored > 0)" in kept
            and "TradeMath.AddPromise(rec, held);" in arrival
            and "TradeMath.AddPromise(" not in tally
            and "_promisesScored++;" in tally
            and "_promisesScored" not in arrival
            and "arrival" not in said['TL399']
            and ("walked into that market " + words[int(enough.group(1))] + " times") in said['TL444']
            and ("walked into that market " + words[int(enough.group(1))] + " times") in README)


chk("1.78.1", "a market's own record counts one walk-in rather than one price checked there, and the number of walk-ins it waits for is the same number the settings screen and the feature list name",
    a_markets_record_counts_your_walk_ins_not_the_prices_it_checked())


def closing_clause(said):
    text = said.strip()
    if '\u3002' in text:
        return [p for p in text.split('\u3002') if p][-1] + '\u3002'
    return [p for p in text.split('. ') if p][-1]

def the_rising_or_falling_mark_says_it_needs_live_prices_off():
    captured = method_body(S['Ledger.cs'],
                           "public void CaptureSettlement(Settlement settlement, bool force, ISet<string> moved)")
    written = method_body(S['Ledger.cs'],
                          "private void Record(string itemId, string townId, int buy, int sell, float day)")
    drift = method_body(S['Ledger.cs'], "public int PriceDrift(ItemObject item, Settlement town, bool selling)")
    marked = method_body(S['TooltipPatches.cs'],
                         "private static string Drifted(ItemObject item, Settlement town, bool selling)")
    if not (captured and written and drift and marked):
        return False
    ledger = code_only(S['Ledger.cs'])
    if not (ledger.count("Record(item.StringId") == 2
            and captured.count("Record(item.StringId, settlement.StringId, buy, sell, day);") == 2
            and ledger.count("_ledger[itemId] = byTown;") == 1
            and "_ledger[itemId] = byTown;" in written
            and "if (Options.Current.Omniscient) return;" in captured
            and captured.index("if (Options.Current.Omniscient) return;")
                < captured.index("Record(item.StringId")
            and "_ledger.TryGetValue(item.StringId, out var byTown)) return 0;" in drift
            and 'if (!Options.Current.MarkPriceDirection) return "";' in marked
            and "PriceDrift(item, town, selling)" in marked):
        return False
    for path in [ENGLISH] + list(TRANSLATIONS.values()):
        said = spoken(path)
        if closing_clause(said['TL408']) not in said['TL407']:
            return False
    return ("Only does anything with Live world prices off" in spoken(ENGLISH)['TL407']
            and "{=TL407}" in M
            and "Only does anything with Live world prices off" in M
            and "Mark a market rising or falling needs Live world prices off, since that is when "
                "TradeLord records prices at all" in README
            and "Live world prices, on out of the box" in README
            and "Live world prices off. Off out of the box" not in README)

def quiet_mode_names_the_warnings_it_still_shows():
    entered = method_body(S['Trading.cs'],
                          "private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)")
    if not entered:
        return False
    for name, signature in (("WarnUnmatchedItemLists", "private static void WarnUnmatchedItemLists()"),
                            ("WarnPurseBelowReserve", "private static bool WarnPurseBelowReserve()"),
                            ("WarnNoRoomToCarry", "private static void WarnNoRoomToCarry()")):
        body = method_body(S['Trading.cs'], signature)
        if not body or "Notices.Say(" not in body or "Muted" in body:
            return False
        if (name + "()") not in entered:
            return False
    onscreen = [l for l in code_only(S['Trading.cs']).splitlines()
                if "Notices.Say(" in l or "Notices.SayAfterXp(" in l]
    asked = [l for l in onscreen if "Muted" in l or "muted" in l]
    said = spoken(ENGLISH)
    return (len(onscreen) == 17
            and len(asked) == 7
            and all(("{=TL" + s + "}") in S['Trading.cs'] for s in ("82", "91", "92", "392"))
            and "Warnings still show on screen" in said['TL349']
            and "cargo full" in said['TL349']
            and "Gold reserve" in said['TL349']
            and "matches no good" in said['TL349']
            and "{=TL349}" in M
            and "Warnings still show on screen" in M
            and "apart from three warnings" in README
            and "a purse below your Gold reserve" in README
            and all({'TL249', 'TL349'} <= set(spoken(f))
                    for f in list(TRANSLATIONS.values()) + [ENGLISH]))


chk("1.78.2", "the setting that marks a market rising or falling says it needs live prices off, in every language, and nothing records a price while they are on",
    the_rising_or_falling_mark_says_it_needs_live_prices_off())
chk("1.78.2", "silencing trade messages names the warnings it still puts on screen, in every language, and no further line reaches the screen without asking whether it is silenced",
    quiet_mode_names_the_warnings_it_still_shows())


def the_feature_list_says_which_purse_a_route_is_held_to():
    routes = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes()")
    if not routes:
        return False
    return ("till = TradeRules.WhatTheTillCanPay(to.SettlementComponent?.Gold ?? 0," in routes
            and "if (Options.Current.Omniscient)" in routes
            and routes.index("if (Options.Current.Omniscient)") < routes.index("till = TradeRules.WhatTheTillCanPay(")
            and "int qtyCap = till > 0 ? Math.Min(stocked, till / openingSell) : stocked;" in routes
            and code_only(S['Ledger.cs']).count("to.SettlementComponent?.Gold") == 1
            and "It reads that purse live, so with Live world prices off it plans on the stock alone" in README)


chk("1.78.2", "the feature list says the purse a route is held to is read live, so it names what live prices off takes away",
    the_feature_list_says_which_purse_a_route_is_held_to())



def a_setting_is_written_on_one_line_so_a_file_reads_back_as_what_was_written():
    onto = method_body(S['Migrate.cs'], "public static string OneLine(string value)")
    composed = method_body(S['Migrate.cs'],
                           "public static string Compose(IEnumerable<string> header,")
    if not (onto and composed):
        return False
    migrate = code_only(S['Migrate.cs'])
    return ("value.Replace('\\r', ' ').Replace('\\n', ' ')" in onto
            and "value.IndexOf('\\n') < 0 && value.IndexOf('\\r') < 0" in onto
            and 'if (string.IsNullOrEmpty(value)) return "";' in onto
            and "Append(' ').Append(OneLine(line))" in composed
            and "Append(OneLine(line.Key))" in composed
            and "AppendLine(OneLine(line.Value))" in composed
            and "Append(line.Value" not in composed
            and "AppendLine(line.Value" not in composed
            and "Append(line.Key)" not in composed
            and migrate.count("OneLine(") == 4
            and all(one in SETTINGSFILETESTS for one in
                    ("A_list_typed_across_lines_keeps_to_its_own_line_and_leaves_every_other_setting_alone",
                     "A_list_typed_across_lines_loses_none_of_what_was_typed",
                     "A_value_with_no_line_break_in_it_is_written_exactly_as_it_was_given",
                     "Whatever_is_written_comes_back_whole_even_when_a_value_was_typed_across_lines"))
            and "Assert.Same(one, SettingsFile.OneLine(one));" in SETTINGSFILETESTS
            and "grain\\nGoldReserve = 99999" in SETTINGSFILETESTS)


chk("1.78.3", "every setting goes into the settings file on a line of its own, so a list typed across lines cannot lose part of itself or land on top of another setting",
    a_setting_is_written_on_one_line_so_a_file_reads_back_as_what_was_written())



def every_name_the_source_reaches_for_by_string_is_held_by_something():
    block = re.search(r'ReflectedTypes\s*=\s*\{(.*?)\};', COMPAT, re.S)
    if block is None:
        return False
    resolved = set(re.findall(r'"([\w.+]+)"', block.group(1)))
    asked = sorted(set(re.findall(r'\.GetType\(\s*"([\w.+]+)"', ALL)))
    if not asked:
        return False
    for name in asked:
        if not name.startswith('TradeLord.'):
            if name not in resolved:
                return False
            continue
        space, cls = name.rsplit('.', 1)
        if ('namespace ' + space) not in M or ('class ' + cls) not in M:
            return False
    ours = re.findall(
        r'\.GetType\(\s*"TradeLord\.[\w.]*?(\w+)"\s*\)\s*\?\s*\.GetMethod\(\s*"(\w+)"', ALL)
    if not ours:
        return False
    for cls, called in ours:
        if ('public static class ' + cls) not in M:
            return False
        if ('public static bool ' + called + '()') not in M:
            return False
    return not re.search(r'\.GetProperty\(\s*"', ALL)


chk("1.78.3", "every game type the source reads by name is one the game-version check resolves, the settings screen it loads by name is the class and method the MCM project really ships, and no property is read by a name nothing holds",
    every_name_the_source_reaches_for_by_string_is_held_by_something())



def the_module_manifest_names_what_the_rest_of_the_project_really_is():
    entry = re.search(r'<SubModuleClassType value="([\w.]+)"', MANIFEST)
    dll = re.search(r'<DLLName value="([\w.]+)\.dll"', MANIFEST)
    mod = re.search(r'<Id value="(\w+)"', MANIFEST)
    companion = re.search(r'Path\.Combine\(dir \?\? "", "([\w.]+)\.dll"\)', S['Support.cs'])
    if not (entry and dll and mod and companion):
        return False
    space, named = entry.group(1).rsplit('.', 1)
    core = io.open('src/TradeLord.csproj', encoding='utf-8').read()
    beside = io.open('mcm/TradeLord.MCM.csproj', encoding='utf-8').read()
    return (('namespace ' + space) in S['SubModule.cs']
            and ('public class ' + named + ' : MBSubModuleBase') in S['SubModule.cs']
            and ('<AssemblyName>' + dll.group(1) + '</AssemblyName>') in core
            and ('<AssemblyName>' + companion.group(1) + '</AssemblyName>') in beside
            and ('GetModuleInfo("' + mod.group(1) + '")') in S['SubModule.cs']
            and ('cp TradeLord/SubModule.xml dist/Modules/' + mod.group(1) + '/') in WORKFLOW
            and ('cp -r TradeLord/ModuleData dist/Modules/' + mod.group(1) + '/') in WORKFLOW
            and ('cp -r TradeLord/GUI dist/Modules/' + mod.group(1) + '/') in WORKFLOW
            and ('cp src/bin/Release/net472/' + dll.group(1) + '.dll') in WORKFLOW
            and ('cp mcm/bin/Release/net472/' + companion.group(1) + '.dll') in WORKFLOW)


chk("1.78.3", "the module file names the class the game starts the mod through, the two files it loads and the folder it is installed into, and every one of them is what the projects and the release workflow really build",
    the_module_manifest_names_what_the_rest_of_the_project_really_is())



def nothing_reads_a_name_without_asking_whether_it_has_one():
    named = method_body(S['Tongue.cs'], "internal static string Named(TextObject name, string id)")
    if named is None:
        named = between(S['Tongue.cs'],
                        "internal static string Named(TextObject name, string id) =>", ";")
    if not named or 'name == null ? id ?? "" : name.ToString()' not in named:
        return False
    read = 0
    for where, text in S.items():
        lines = text.split('\n')
        for m in re.finditer(r'\.Name\.ToString\(\)', text):
            read += 1
            at = text[:m.start()].count('\n')
            asked = '\n'.join(lines[max(0, at - 1):at + 1])
            if 'Name == null' in asked or 'Name != null' in asked:
                continue
            return False
    return (read == 7
            and ALL.count("Tongue.Named(") == 16
            and '_route.Item == null ? "" : Tongue.Named(_route.Item.Name, _route.Item.StringId)'
                in S['Panel.cs']
            and '_route.From == null ? "" : Tongue.Named(_route.From.Name, _route.From.StringId)'
                in S['Panel.cs']
            and '_route.To == null ? "" : Tongue.Named(_route.To.Name, _route.To.StringId)'
                in S['Panel.cs']
            and 'shop.Settlement == null ? "" : Tongue.Named(shop.Settlement.Name, shop.Settlement.StringId)'
                in S['Panel.cs']
            and 'shop.WorkshopType == null ? "" : Tongue.Named(shop.WorkshopType.Name, shop.WorkshopType.StringId)'
                in S['Panel.cs']
            and 'shop.Owner == null ? "" : Tongue.Named(shop.Owner.Name, shop.Owner.StringId)'
                in S['Panel.cs']
            and "Tongue.Named(w.WorkshopType.Name, w.WorkshopType.StringId)" in S['Panel.cs']
            and 'w.Settlement == null ? "?" : Tongue.Named(w.Settlement.Name, w.Settlement.StringId)'
                in S['Panel.cs']
            and 'w.Owner == null ? "" : Tongue.Named(w.Owner.Name, w.Owner.StringId)'
                in S['Panel.cs']
            and "Tongue.Named(Site.Name, Site.StringId)" in S['Trading.cs']
            and "Tongue.Named(Met.Name, Met.StringId)" in S['Trading.cs']
            and 'site == null ? "this market" : Tongue.Named(site.Name, site.StringId)'
                in S['Counter.cs'])


chk("1.78.4", "nothing turns a name into text without asking first whether the good, the market or the party has one, so a nameless one is shown by its id rather than stopping the panel",
    nothing_reads_a_name_without_asking_whether_it_has_one())



def the_explanation_is_opened_by_a_mark_beside_the_title():
    import xml.etree.ElementTree as ET
    tree = ET.parse('TradeLord/GUI/Prefabs/TradeLordPanel.xml')
    parent = {c: p for p in tree.iter() for c in p}
    def row(node):
        node = parent.get(node)
        while node is not None and node.tag in ('Children', 'ItemTemplate'):
            node = parent.get(node)
        return node
    def texts(node):
        return {e.get('Text') for e in node.iter() if e.get('Text')}
    opens = [e for e in tree.iter() if e.get('Command.Click') == 'ExecuteOpenLegend']
    closes = [e for e in tree.iter() if e.get('Command.Click') == 'ExecuteClose']
    if len(opens) != 1 or not closes:
        return False
    beside = row(opens[0])
    if beside is None:
        return False
    if '@TitleLabel' not in texts(beside):
        return False
    if any('@CloseLabel' in texts(row(one)) and row(one) is beside for one in closes):
        return False
    said = spoken(ENGLISH)
    return (texts(opens[0]) == {'@HelpLabel'}
            and opens[0].get('SuggestedWidth') == '40'
            and said.get('TL446') == '?'
            and said.get('TL432') == 'What this means'
            and '[DataSourceProperty] public string HelpLabel => Tongue.Text("{=TL446}?").ToString();'
                in S['Panel.cs']
            and '"LegendLabel", "HelpLabel"' in S['Panel.cs']
            and 'Text="@LegendLabel"' in PREFAB
            and PREFAB.count('Text="@LegendLabel"') == 1
            and PREFAB.count('Command.Click="ExecuteOpenLegend"') == 1
            and {'TL432', 'TL446'} <= strings_declared()
            and all({'TL432', 'TL446'} <= set(spoken(f))
                    for f in list(TRANSLATIONS.values()) + [ENGLISH]))


chk("1.78.5", "the explanation is opened by a mark beside the panel title rather than a button in the row along the bottom, and the window it opens is still the one that says what it all means",
    the_explanation_is_opened_by_a_mark_beside_the_title())



def the_map_carries_both_buttons_inside_the_region_it_reserves():
    import xml.etree.ElementTree as ET
    tree = ET.parse('TradeLord/GUI/Prefabs/TradeLordPanel.xml')
    block = [e for e in tree.iter() if e.get('Id') == 'TradeLordMapButton']
    if len(block) != 1:
        return False
    block = block[0]
    stacked = [c for w in block if w.tag == 'Children' for c in w]
    if len(stacked) != 2 or any(b.tag != 'ButtonWidget' for b in stacked):
        return False
    if block.get('StackLayout.LayoutMethod') != 'VerticalTopToBottom':
        return False
    tall = 0
    for b in stacked:
        if b.get('HeightSizePolicy') != 'Fixed' or b.get('WidthSizePolicy') != 'Fixed':
            return False
        if b.get('SuggestedWidth') != block.get('SuggestedWidth'):
            return False
        tall += int(b.get('SuggestedHeight')) + int(b.get('MarginTop') or 0)
    if block.get('HeightSizePolicy') != 'Fixed' or int(block.get('SuggestedHeight')) != tall:
        return False
    named = [[e.get('Text') for e in b.iter() if e.get('Text')][0] for b in stacked]
    opens = [b.get('Command.Click') for b in stacked]
    said = spoken(ENGLISH)
    return (named == ['@BrandLabel', '@TradesLabel']
            and opens == ['ExecuteOpenPanel', 'ExecuteOpenTrades']
            and PREFAB.count('Command.Click="ExecuteOpenTrades"') == 1
            and said.get('TL431') == 'Buy Workshops Remotely'
            and S['Panel.cs'].count('{=TL431}Buy Workshops Remotely') == 2
            and 'Recent trades' in said.get('TL315', '')
            and all('TL315' in set(spoken(f)) for f in list(TRANSLATIONS.values()) + [ENGLISH]))


chk("1.79.0", "the campaign map carries the TradeLord button with Recent trades stacked under it, both inside the one region the map reserves for the mouse, and the ledger's own row no longer repeats either",
    the_map_carries_both_buttons_inside_the_region_it_reserves())


def a_workshop_row_is_drawn_whether_or_not_its_parts_are_named():
    offer = method_body(S['Panel.cs'], "public ShopOfferRowVM")
    listed = method_body(S['Panel.cs'], "private void RefreshWorkshops")
    return (".Name.ToString()" not in offer + listed
            and offer.count("Tongue.Named(") == 3
            and listed.count("Tongue.Named(") == 3
            and "w.WorkshopType.Name + " not in listed
            and "Tongue.Named(w.WorkshopType.Name, w.WorkshopType.StringId)" in listed)


chk("1.79.1", "a workshop, its town or its owner with no name of its own is shown by its id rather than stopping the ledger drawing or the Buy Workshops Remotely window opening",
    a_workshop_row_is_drawn_whether_or_not_its_parts_are_named())


def the_marker_settles_on_a_town_already_on_your_map():
    update = method_body(S['Marker.cs'], "internal static void Update")
    forget = method_body(S['Marker.cs'], "internal static void Forget()")
    held = between(S['Marker.cs'], "internal static Settlement Tracked", "}\n\n")
    return ("private static Settlement _picked;" in S['Marker.cs']
            and "if (target == _picked)" in update
            and "if (target == _tracked)" not in update
            and ordered(update, "_tracked = null;", "_picked = target;")
            and "_picked = null;" in forget
            and ordered(held, "_tracked = value;", "if (value != null) _picked = value;"))


chk("1.79.1", "a market that is already on your map is still taken as the one the marker picked, so the line it writes to TradeLord.log is written once rather than over and over",
    the_marker_settles_on_a_town_already_on_your_map())


def the_ceiling_and_the_quote_read_one_forecast_at_one_arrival():
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
    return (scan.count("Forecast.WorthShiftAsItHasHeld(to, item,") == 1
            and scan.count("Forecast.WorthShiftAsItHasHeld(from, item,") == 1
            and scan.count("Bulk.Opening(") == 2
            and ordered(scan,
                        "int landedAtBuyTown = Forecast.WorthShiftAsItHasHeld(from, item, toBuy);",
                        "Bulk.Opening(from, item, false, buyPrice, landedAtBuyTown)",
                        "float days = toBuy + Travel.EstimateDaysBetween(from, to);",
                        "int landedAtSellTown = Forecast.WorthShiftAsItHasHeld(to, item, days);",
                        "Bulk.Opening(to, item, true, sellPrice, landedAtSellTown)",
                        "float ceiling =",
                        "landedAtBuyTown,\n                                                 landedAtSellTown);"))


chk("1.79.2", "what a route could at best be worth is worked out from the same arrival, and the same count of what reaches that market before you, as the quote the panel then shows",
    the_ceiling_and_the_quote_read_one_forecast_at_one_arrival())


def a_route_is_judged_only_on_what_that_market_will_charge_when_you_reach_it():
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
    return (scan.count("sellPrice") == 3
            and scan.count("buyPrice") == 4
            and "Bulk.Opening(to, item, true, sellPrice, landedAtSellTown);" in scan
            and "Bulk.Opening(from, item, false, buyPrice, landedAtBuyTown);" in scan
            and "int stocked = MostWorthShowing(openingBuy);" in scan
            and "int qtyCap = till > 0 ? Math.Min(stocked, till / openingSell) : stocked;" in scan
            and "float ceiling = (float)(openingSell - openingBuy) * qtyCap;" in scan
            and "BuyPrice = q.OpeningBuyPrice, SellPrice = q.OpeningSellPrice," in scan
            and "sells[0]" not in scan
            and "till / sellPrice" not in scan
            and "MostWorthShowing(buyPrice)" not in scan
            and ") break;" not in between(scan, "foreach (var (to, sellPrice) in sells)",
                                          "Bulk.Walk(from, to, item"))


chk("1.79.3", "a route is weighed, capped and priced on what its two markets will charge when you reach them, never on what they charge while you are still standing somewhere else",
    a_route_is_judged_only_on_what_that_market_will_charge_when_you_reach_it())


def a_window_a_map_button_opens_can_be_reached_with_the_mouse():
    import xml.etree.ElementTree as ET
    tree = ET.parse('TradeLord/GUI/Prefabs/TradeLordPanel.xml')
    block = [e for e in tree.iter() if e.get('Id') == 'TradeLordMapButton']
    if len(block) != 1:
        return False
    opens = [b.get('Command.Click') for b in block[0].iter() if b.get('Command.Click')]
    idle = method_body(S['Panel.cs'], "private static void UpdateIdleInput")
    return ('ExecuteOpenTrades' in opens
            and PREFAB.count('Command.Click="ExecuteOpenTrades"') == 1
            and 'internal static bool TakesTheMouse(bool windowOpen, bool buttonOn, bool overButton) =>\n'
                '            windowOpen || (buttonOn && overButton);' in S['Rules.cs']
            and 'MapButton.TakesTheMouse(\n'
                '                _vm.IsTradesVisible, buttonOn, OverButtonBounds(Input.MousePositionRanged))'
                in idle
            and 'buttonOn && OverButtonBounds' not in S['Panel.cs']
            and S['Panel.cs'].count('MapButton.TakesTheMouse') == 1
            and 'if (_vm.IsTradesVisible && map.IsEscapeMenuOpened) _vm.IsTradesVisible = false;'
                in S['Panel.cs']
            and 'MapButton.TakesTheMouse(windowOpen: true' in T['MapButtonTests.cs']
            and 'MapButton.TakesTheMouse(windowOpen: false' in T['MapButtonTests.cs'])


chk("1.80.2", "a window a button on the campaign map opens holds the mouse while it is up, wherever the cursor is, so it can be scrolled and closed, and it gives the mouse back when the escape menu opens",
    a_window_a_map_button_opens_can_be_reached_with_the_mouse())


def the_till_is_never_divided_by_a_price_nothing_has_tested():
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
    tested = scan.find("if (!TradePolicy.BuyAcceptable(openingBuy, realizable)) { thrownAway++; continue; }")
    divided = scan.find("till / openingSell")
    bounds = {name: (float(lo), float(hi)) for name, lo, hi in re.findall(
        r'\{\s*"(\w+)",\s*new double\[\]\s*\{\s*([-\d.]+),\s*([-\d.]+)\s*\}', S['Migrate.cs'])}
    return (tested > 0 and divided > tested
            and scan.count("till / openingSell") == 1
            and "float realizable = TradePolicy.Realizable(openingSell);" in scan
            and "buyPrice > 0 && realizable >= buyPrice * (1f + margin)" in S['TradeMath.cs']
            and bounds.get("MinProfitMargin", (-1.0, 0.0))[0] >= 0.0
            and bounds.get("ResaleSafetyFactor", (-1.0, 0.0))[0] > 0.0)


chk("1.80.2", "a route's sell price is put to the profit test before the merchant's purse is divided by it, and the margin and the safety factor cannot go low enough to let a price of nothing through, so working out the routes cannot stop the ledger opening",
    the_till_is_never_divided_by_a_price_nothing_has_tested())


def a_market_is_never_searched_for_a_good_it_does_not_stock():
    prime = method_body(S['Ledger.cs'], "private void PrimeLiveRankings(List<ItemObject> wanted, int hour)")
    stocks = method_body(S['Ledger.cs'], "private static Dictionary<ItemObject, int> WhatItStocks")
    return (ordered(prime,
                    "Dictionary<ItemObject, int> onTheShelf = minStock > 0 ? WhatItStocks(town) : null;",
                    "if (onTheShelf == null ||",
                    "(onTheShelf.TryGetValue(item, out stocked) &&",
                    "TradeMath.EnoughOnTheShelf(stocked, item.Value, minStock, minWorth))")
            and prime.find("Priced.At(market, item, me, true)") <
                prime.find("onTheShelf.TryGetValue(item,")
            and prime.count("WhatItStocks(town)") == 1
            and "ItemObject item = shelf.GetItemAtIndex(i);" in stocks
            and "GetItemNumber" not in stocks)


chk("1.80.3", "the TradeLord ledger asks a market how much of a good it holds only where that good is on its shelves, so a market is no longer searched top to bottom for every good it never stocked",
    a_market_is_never_searched_for_a_good_it_does_not_stock())


def one_read_of_a_shelf_answers_both_what_it_holds_and_how_much():
    l = S['Ledger.cs']
    prime = method_body(l, "private void PrimeLiveRankings(List<ItemObject> wanted, int hour)")
    stocks = method_body(l, "private static Dictionary<ItemObject, int> WhatItStocks")
    return (ordered(stocks, "if (item == null) continue;", "held.TryGetValue(item, out int had);",
                    "held[item] = had + shelf.GetElementNumber(i);")
            and "var held = new Dictionary<ItemObject, int>();" in stocks
            and stocks.count("for (int i = 0; shelf != null && i < shelf.Count; i++)") == 1
            and "StockOf(town, item)" not in prime
            and "onTheShelf.Contains(" not in prime
            and "internal static int StockOf(Settlement s, ItemObject item)" in l)


chk("1.81.4", "one read of a town's shelves answers both whether it stocks a good and how much of it, so ranking the markets no longer searches a shelf again for every good already found on it",
    one_read_of_a_shelf_answers_both_what_it_holds_and_how_much())


def asking_whether_a_tooltip_has_a_section_builds_no_list():
    tip = S['TooltipPatches.cs']
    sectioned = method_body(tip, "private static bool Sectioned")
    any_market = method_body(S['Ledger.cs'], "internal bool AnyMarketFor")
    return ("return ledger.AnyMarketFor(item);" in sectioned
            and "Markets(itemVm)" not in sectioned
            and "TopSell" not in sectioned and "TopBuy" not in sectioned
            and "int sells = TopMarkets(item, true).Count;" in any_market
            and "int buys = TopMarkets(item, false).Count;" in any_market
            and "return sells > 0 || buys > 0;" in any_market
            and "TakeN" not in any_market and "TopFew" not in any_market
            and "var (item, sells, buys) = Markets(itemVm);" in
                method_body(tip, "internal static void Append(ItemMenuVM vm, ItemVM itemVm)"))


chk("1.80.3", "the item tooltip works out whether it has anything to say without building the two lists of best markets it would then throw away, and still builds them once when it draws them",
    asking_whether_a_tooltip_has_a_section_builds_no_list())


def a_version_commit_says_in_its_message_what_the_changelog_says():
    held = between(RELEASED, "def theMessageSaysWhatTheChangelogSays", "\ndef main(")
    main = between(RELEASED, "def main(argv):", "\n    first = min(live")
    return ("def thisCommit():" in RELEASED
            and "os.environ.get('GITHUB_SHA') or 'HEAD'" in RELEASED
            and "'--format=%s%x00%b'" in RELEASED
            and "if subject.startswith('[no release]'):" in held
            and "entries = notes(said.get(shipping, []))" in held
            and "written = bulleted(body)" in held
            and "if written != entries:" in held
            and "only in the changelog: " in held
            and "only in the message  : " in held
            and "this commit could not be read" in held
            and ordered(main,
                        "said = {head: entries for head, entries in",
                        "agreed = theMessageSaysWhatTheChangelogSays(shipping, said)",
                        "rows, how = asked()",
                        "return 0 if agreed else 1")
            and RELEASED.count("return 0 if agreed else 1") == 2
            and "The entries in a version section and the bullet points in that version's commit body say "
                "the same thing in the same words" in RULES)


chk("1.80.4", "a commit that ships a version is refused unless its message and that version's changelog section say the same thing, so the history and the published notes can never disagree",
    a_version_commit_says_in_its_message_what_the_changelog_says())


def the_files_this_repository_never_deletes_are_looked_for_first():
    head = SWEEP[:SWEEP.find("S = {f: io.open")]
    return ("NEVER_DELETED = [" in head
            and all("'" + one + "'" in head for one in
                    ('CHANGELOG.md', 'CLAUDE.md', '.github/workflows/build.yml',
                     '.claude/settings.json', '.claude/hooks/session-start.sh',
                     '.claude/hooks/no-new-branch.sh'))
            and "not os.path.exists(one)" in head
            and "a file this repository never deletes is gone" in head
            and "sys.exit(1)" in head
            and all(os.path.exists(one) for one in NEVER_DELETED)
            and "Never delete `CHANGELOG.md` or `CLAUDE.md`" in RULES
            and "Never delete or disable the release workflow" in RULES)


chk("1.80.5", "the files this repository never deletes are looked for before anything is read, so losing one is said plainly rather than stopping the checks with an error",
    the_files_this_repository_never_deletes_are_looked_for_first())


def the_score_is_only_said_to_be_lowered_where_the_lowering_reaches_it():
    refresh = method_body(S['Panel.cs'], "private void Refresh")
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
    return ("(Options.Current.ConfidenceRanking && Options.Current.TrustWhatAMarketPaid" in refresh
            and "{=TL445}" in refresh
            and "float key = rankByScore ? score : perDay;" in scan
            and "score = Confidence.AsPromisesHaveHeld(score, arrivals, heldThere);" in scan
            and "bool rankByScore = Options.Current.ConfidenceRanking;" in scan
            and "Options.Current.ConfidenceRanking ? _route.Score : _route.ProfitPerDay" in S['Panel.cs'])


chk("1.80.5", "the panel says a market's Score is lowered for paying less than it promised only where that lowering reaches the Score at all",
    the_score_is_only_said_to_be_lowered_where_the_lowering_reaches_it())


def a_setting_that_leans_on_another_names_it_in_every_language():
    refresh = method_body(S['Panel.cs'], "private void Refresh")
    shelf = method_body(S['Market.cs'], "internal Shelf(Settlement site, EquipmentElement stocked, "
                                        "bool selling, int quoted, bool projecting, int landed = 0)")
    everywhere = [ENGLISH] + list(TRANSLATIONS.values())
    return ("if (projecting && (!Options.Current.Omniscient || !Options.Current.BulkSimulation)) return;"
                in shelf
            and "(Options.Current.BulkSimulation" in refresh
            and "{=TL394}" in refresh and "{=TL447}" in refresh
            and refresh.index("{=TL394}") < refresh.index("{=TL447}")
            and said_in_every_language('TL447')
            and all(spoken(f)['TL209'] in spoken(f)['TL447'] for f in everywhere)
            and all(spoken(f)['TL210'] in spoken(f)['TL444'] for f in everywhere))


chk("1.80.6", "the panel says a price counts what is on its way only where that can move a price, and a setting that leans on another names it in the reader's own language",
    a_setting_that_leans_on_another_names_it_in_every_language())


def a_price_is_the_one_a_player_could_get_by_hand():
    market = S['Market.cs']
    trading = S['Trading.cs']
    asked = method_body(market,
                        "internal static int At(SettlementComponent market, EquipmentElement el, MobileParty who, bool selling)")
    walk = method_body(market, "internal int Price()")
    prefix = method_body(market, "private static void Prefix(ref PartyBase merchantParty)")
    return ("held.GetPrice(el, who, selling, Merchant(site))" in asked
            and "_element, MobileParty.MainParty, _party, _selling," in walk
            and "_party = Priced.Merchant(site);" in market
            and "internal static PartyBase Merchant(Settlement site) => site?.Party;" in market
            and "StagesTheDeal" not in market
            and "if (merchantParty == null) merchantParty = TradeActionBehavior.TradingWith;" in prefix
            and trading.count("_tradingWith = merchant;") == 1
            and trading.count("_tradingWith = null;") == 4
            and all("_tradingWith = null;" in method_body(trading, one) for one in
                    ("private static bool SwapOneUnit", "private static void InAPass",
                     "internal static void ReleaseMessageFilter", "internal static void ForgetVisit"))
            and "if (!TradeActionBehavior.PricesAreReal()) return null;" in
                method_body(trading, "internal static Pass Open(Settlement site, bool quiet)")
            and "Patcher.Holds(nameof(Patch_TownMarketData_GetPrice))" in trading
            and "return 0;" in method_body(market,
                    "internal static int At(SettlementComponent market, EquipmentElement el, MobileParty who, bool selling)"))


chk("1.80.12", "a price is the one a player could get by hand, in the quote, in the simulated walk and in the trade the mod makes, so naming the settlement as the merchant and wearing the worse village price with it is the point of this and never a fault to undo",
    a_price_is_the_one_a_player_could_get_by_hand())

chk("1.80.12", "the mod trades nothing in a settlement when it could not take the price over, because trading at a price you could not be charged is the thing being avoided and a fallback that traded anyway would undo it",
    (lambda b: ordered(b, "if (!TradeActionBehavior.PricesAreReal()) return null;",
                       "if (!MarketOpen(site"))
    (method_body(S['Trading.cs'], "internal static Pass Open(Settlement site, bool quiet)")) and
    "trading in towns and villages is off" in S['Trading.cs'] and
    S['Trading.cs'].count("PricesAreReal()") == 2)


def whoever_made_the_trade_credits_the_trade_skill_for_it():
    t = S['Trading.cs']
    passes = S['Passes.cs']
    ledger = S['Ledger.cs']
    credit = method_body(t, "private static void CreditTradeSkill")
    heard = method_body(ledger, "private void OnPlayerTradeProfit")
    return ("internal int Earned;" in passes
            and "bool TheGameGivesTradeXpFor(int at);" in passes
            and "public bool TheGameGivesTradeXpFor(int at) => _plan[at].EquipmentElement.ItemModifier == null;" in t
            and passes.count("if (market.TheGameGivesTradeXpFor(at))") == 2
            and t.count("AwardTradeXpForOurOwnTrade(") == 3
            and "AwardTradeXpForOurOwnTrade(" not in method_body(t, "private static void ReportWhatYouSold")
            and "internal int Earned;" not in t
            and "CampaignEventDispatcher.Instance.OnPlayerTradeProfit(profit)" in credit
            and "_tradeLordIsCreditingItsOwnTrade = true;" in credit
            and "internal static bool TradeLordIsCreditingItsOwnTrade => _tradeLordIsCreditingItsOwnTrade;" in t
            and "LedgerBehavior.Instance?.AddTradeXp(xp);" in credit
            and "CampaignEvents.OnPlayerTradeProfitEvent.AddNonSerializedListener(this, OnPlayerTradeProfit);" in ledger
            and "private static bool TheGameCreditedADealTradeLordLaidOut =>" in ledger
            and "!TradeActionBehavior.TradeLordIsCreditingItsOwnTrade && Counter.Awaiting;" in ledger
            and "if (!TheGameCreditedADealTradeLordLaidOut) return;" in heard
            and "AddTradeXp(profit);" in heard)


def the_ledger_keeps_the_trade_xp_it_has_handed_over():
    ledger = S['Ledger.cs']
    panel = S['Panel.cs']
    return ("public long LifetimeTradeXp => _lifetimeTradeXp;" in ledger
            and "public void AddTradeXp(int amount) { if (amount > 0) _lifetimeTradeXp += amount; }" in ledger
            and 'dataStore.SyncData("TradeLord_LifetimeTradeXp", ref _lifetimeTradeXp);' in
                method_body(ledger, "public override void SyncData")
            and ordered(method_body(panel, "private void Refresh"),
                        "LifetimeText = Line(", "TradeXpText = Line(")
            and '{=TL448}Trade XP: {AMOUNT}' in panel
            and said_in_every_language('TL448')
            and ordered(PREFAB, 'Text="@LifetimeText"', 'Text="@TradeXpText"'))


chk("1.81.2", "whoever made the trade credits the Trade skill for it, and each member along that path is named after the half it belongs to, so the two sides read as one rule rather than as a mod that forgot to credit its own screen",
    whoever_made_the_trade_credits_the_trade_skill_for_it())

chk("1.81.0", "the ledger keeps the trade XP it has handed over and shows it beside the profit",
    the_ledger_keeps_the_trade_xp_it_has_handed_over())


def a_village_keeps_the_coin_that_keeps_its_shop_open():
    ledger = S['Ledger.cs']
    rule = between(S['Rules.cs'], "internal static int WhatTheTillCanPay", ";")
    return ("internal const int VillageLastCoin = 1;" in S['Rules.cs']
            and "Math.Max(0, village ? till - VillageLastCoin : till)" in rule
            and "        bool Stopped { get; }\n        bool Village { get; }\n"
                "        string IdAt(int at);" in S['Passes.cs']
            and "public bool Village => _pass.Site != null && _pass.Site.IsVillage;" in sell_pass()
            and "market.Village) < price)" in sell_pass()
            and (lambda herd: "TradeRules.WhatTheTillCanPay(pass.Sim ? simTill : pass.TillNow," in herd
                          and "settlement.IsVillage) < price) break;" in herd)
                (method_body(S['Trading.cs'], "public static void ExecuteHerdRelief"))
            and "till = TradeRules.WhatTheTillCanPay(to.SettlementComponent?.Gold ?? 0," in
                method_body(ledger, "private List<TradeRoute> ScanRoutes()")
            and "TradeRules.WhatTheTillCanPay(market.Gold, town.IsVillage) > 0;" in ledger
            and "int purse = TradeRules.WhatTheTillCanPay(market.Gold, s.IsVillage);" in S['Marker.cs']
            and "reachable.Add((s, market, purse, ride));" in S['Marker.cs']
            and code_only(S['Trading.cs']).count("pass.TillNow") == 2
            and "A_village_is_left_its_last_coin" in SELLPASSTESTS
            and "A_town_spends_its_till_to_the_last_coin" in SELLPASSTESTS
            and "A_dry_run_leaves_a_village_its_last_coin_too" in SELLPASSTESTS
            and "A_village_till_is_read_one_coin_short_and_a_town_till_in_full" in SELLTESTS)


chk("1.81.6", "a village is left the coin that keeps its shop open, in the pass, in the herd relief, in the route scan and on the map marker",
    a_village_keeps_the_coin_that_keeps_its_shop_open())


def a_village_left_with_an_empty_purse_is_put_back_once():
    ledger = S['Ledger.cs']
    put = method_body(ledger, "private void PutBackEmptyVillagePurses")
    if not put:
        return False
    return ("internal const int VillagePurse = 1000;" in S['Rules.cs']
            and "gold <= 0 ? VillagePurse - gold : 0" in
                between(S['Rules.cs'], "internal static int PutBackIntoAnEmptyPurse", ";")
            and 'dataStore.SyncData("TradeLord_VillagePursesPutBack", ref _villagePursesPutBack);' in
                method_body(ledger, "public override void SyncData")
            and 'Guard.Run("Ledger.VillagePurses", PutBackEmptyVillagePurses);' in
                method_body(ledger, "private void OnSessionLaunched")
            and "if (_villagePursesPutBack) return;" in put
            and "_villagePursesPutBack = true;" in put
            and ordered(put, "if (_villagePursesPutBack) return;", "_villagePursesPutBack = true;",
                        "int owed = TradeRules.PutBackIntoAnEmptyPurse(village.Gold);",
                        "village.ChangeGold(owed);")
            and "{=TL449}" in ledger
            and said_in_every_language('TL449')
            and "An_empty_village_purse_is_put_back_to_a_thousand" in SELLTESTS)


chk("1.81.7", "a village left with an empty purse is put back to a thousand denars, once for a campaign and never again",
    a_village_left_with_an_empty_purse_is_put_back_once())



def nothing_out_of_reach_is_marked_on_the_map_or_offered_as_a_route():
    marker = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
    return ("float ride = Travel.EstimateDaysFromParty(s);" in marker
            and "if (TradeMath.OutOfReach(ride)) { how.NoRoad++; continue; }" in marker
            and marker.find("if (TradeMath.OutOfReach(ride)) { how.NoRoad++; continue; }") <
                marker.find("if (cap > 0f && ride > cap) { how.PastCeiling++; continue; }")
            and "if (cap > 0f)\n" not in marker
            and "if (TradeMath.OutOfReach(days)) continue;" in scan
            and scan.find("if (TradeMath.OutOfReach(days)) continue;") <
                scan.find("if (cap > 0f && days > cap) continue;"))


def an_empty_best_buy_list_says_why_it_is_empty():
    append = method_body(S['TooltipPatches.cs'], "internal static void Append(ItemMenuVM vm, ItemVM itemVm)")
    return ("if (buys.Count == 0 && sells.Count > 0 && Options.Current.MinTownStock > 0)" in append
            and "{=TL450}" in append
            and 'none.SetTextVariable("COUNT", Options.Current.MinTownStock);' in append
            and append.find("buys.Count == 0") < append.find("if (buys.Count > 0)")
            and 'private static readonly string[] Clauses = { " | " };' in S['TooltipPatches.cs']
            and "foreach (string clause in none.ToString().Split(Clauses, "
                "StringSplitOptions.RemoveEmptyEntries))" in append
            and 'AddLine(vm, "", clause, Warn);' in append
            and all(len(one.strip()) <= 60
                    for one in spoken(ENGLISH)['TL450'].split(" | "))
            and spoken(ENGLISH)['TL450'].count(" | ") == 2
            and all(said.count(" | ") == 2
                    for said in (spoken(path)['TL450'] for path in TRANSLATIONS.values()))
            and said_in_every_language('TL450'))


def the_forecast_log_says_units_of_a_good_and_denars_of_its_kind():
    written = method_body(S['Hindsight.cs'], "private static void Written")
    return ('" unit(s) of it would land within "' in written
            and '"; said every good of that kind heading there was worth "' in written
            and '" denars in all and "' in written
            and '" denars, "' in written
            and '" in worth and "' not in written)


chk("1.81.12", "nothing the game could find no road to is marked on your map or offered as a route, whatever the travel ceilings are set to",
    nothing_out_of_reach_is_marked_on_the_map_or_offered_as_a_route())
chk("1.81.12", "a best buy list left empty by the minimum stock setting says so rather than vanishing",
    an_empty_best_buy_list_says_why_it_is_empty())
chk("1.81.12", "the forecast check in the log says units of the good it named and denars of every good of that kind, so neither figure reads as the other",
    the_forecast_log_says_units_of_a_good_and_denars_of_its_kind())



def a_costly_good_is_not_filtered_out_for_being_rare():
    m = S['TradeMath.cs']
    rule = method_body(m, "public static bool EnoughOnTheShelf")
    prime = method_body(S['Ledger.cs'], "private void PrimeLiveRankings(List<ItemObject> wanted, int hour)")
    live = method_body(S['Ledger.cs'], "private List<(Settlement, int)> TopLive")
    tip = S['TooltipPatches.cs']
    return ("if (minUnits <= 0) return true;" in rule
            and "if (stocked >= minUnits) return true;" in rule
            and "if (minWorth <= 0 || stocked <= 0 || unitWorth <= 0) return false;" in rule
            and "return (long)stocked * unitWorth >= minWorth;" in rule
            and "public int MinTownStockWorth = 500;" in S['Options.cs']
            and EVER_SHIPPED.get('MinTownStockWorth') == 'int'
            and '{ "MinTownStockWorth", new double[] { 0, 20000 } },' in S['Migrate.cs']
            and "int minWorth = Options.Current.MinTownStockWorth;" in prime
            and "int minWorth = Options.Current.MinTownStockWorth;" in live
            and "TradeMath.EnoughOnTheShelf(stocked, item.Value, minStock, minWorth)" in prime
            and "TradeMath.EnoughOnTheShelf(StockOf(s, item), item.Value," in live
            and "price" not in between(live, "if (!selling", "continue;")
            and "A_shop_down_to_its_last_unit_cannot_pass_by_asking_a_high_price" in MATHTESTS
            and "A_shelf_is_counted_the_way_the_game_counts_one_at_the_goods_own_worth" in MATHTESTS
            and "{=TL452}Minimum stock value for buy suggestions" in M
            and "public int MinTownStockWorth { get => _o.MinTownStockWorth;" in M
            and "{WORTH}" in spoken(ENGLISH)['TL451']
            and "{COUNT}" in spoken(ENGLISH)['TL451']
            and 'none.SetTextVariable("WORTH", worthFloor);' in tip
            and "int worthFloor = Options.Current.MinTownStockWorth;" in tip
            and all(said_in_every_language(one) for one in ("TL451", "TL452", "TL453"))
            and "A_shelf_short_on_units_still_counts_when_what_it_holds_is_worth_enough" in MATHTESTS
            and "A_worth_floor_of_zero_leaves_the_unit_floor_exactly_as_it_was" in MATHTESTS
            and "No_unit_floor_at_all_lets_every_market_through_as_it_always_did" in MATHTESTS)


def the_pass_buys_what_the_ledger_sent_you_for_first():
    order = method_body(S['Passes.cs'], "internal static void WhatTheLedgerAskedForFirst")
    want = method_body(S['Passes.cs'], "internal static List<Pick> WhatToBuy")
    asks = method_body(S['Trading.cs'], "public int TheLedgerAsksFor")
    buys = method_body(S['Ledger.cs'], "public Dictionary<string, int> WhatTheLedgerBuysAt")
    lift = method_body(S['Migrate.cs'], "private static bool WhatToBuyFirstIsOneRuleNow")
    return ("internal int LedgerRank;" in S['Passes.cs']
            and "int TheLedgerAsksFor(int at);" in S['Passes.cs']
            and "LedgerRank = market.TheLedgerAsksFor(at)" in want
            and ordered(want, "Picks.MostMoneyFirst(stock);",
                        "Picks.WhatTheLedgerAskedForFirst(stock);")
            and "foreach (Pick one in stock) (one.LedgerRank > 0 ? asked : rest).Add(one);" in order
            and "if (asked.Count == 0) return;" in order
            and "asked.Sort((x, y) => x.LedgerRank != y.LedgerRank" in order
            and ordered(order, "stock.Clear();", "stock.AddRange(asked);", "stock.AddRange(rest);")
            and "LedgerBehavior.Instance?.WhatTheLedgerBuysAt(_pass.Site)" in asks
            and "asked[route.Item.StringId] = asked.Count + 1;" in buys
            and "BestRoutes(int.MaxValue)" in buys
            and "WhatToBuyFirst" not in S['Options.cs']
            and "FollowTheLedgerFirst" not in S['Options.cs']
            and EVER_SHIPPED.get('FollowTheLedgerFirst') == 'bool'
            and EVER_SHIPPED.get('WhatToBuyFirst') == 'int'
            and 'foreach (string was in new[] { "FollowTheLedgerFirst", "WhatToBuyFirst" })' in lift
            and "public int TheLedgerAsksFor(int at) =>" in BUYPASSTESTS
            and "The_route_the_ledger_scores_highest_is_bought_before_its_lower_ones" in SHELFORDERTESTS
            and "The_good_the_ledger_sent_you_for_is_bought_before_a_fatter_margin" in SHELFORDERTESTS
            and "A_shelf_the_ledger_says_nothing_about_is_left_exactly_as_it_was" in SHELFORDERTESTS)


def the_shelf_is_ranked_by_what_a_pick_would_really_make():
    want = method_body(S['Passes.cs'], "internal static List<Pick> WhatToBuy")
    made = method_body(S['Passes.cs'], "private static float WhatThisPickWouldReallyMake")
    rule = method_body(S['TradeMath.cs'], "public static int MostYouCouldTake")
    return ("long affordable = spendable / unitPrice;" in rule
            and "long fits = (long)(room / unitWeight);" in rule
            and "if (unitWeight > 0.01f)" in rule
            and "if (unitPrice <= 0 || stocked <= 0 || spendable <= 0) return 0;" in rule
            and "return take <= 0 ? 0f : take * profitPerUnit;" in
                method_body(S['TradeMath.cs'], "public static float WhatThisPickWouldMake")
            and ordered(want, "int take = TradeMath.MostYouCouldTake(here, one.Good.Weight,",
                        "market.TheirsToSell(one.At),",
                        "market.Spendable(), market.Room(),",
                        "s.BuyCapPerItem);")
            and "picked.Worth = WhatThisPickWouldReallyMake(market, one.At, carried, here, take, s);"
                in want
            and "if (cap > 0 && cap < take) take = cap;" in rule
            and "The_buy_cap_per_item_holds_the_take_down_to_what_it_allows" in MATHTESTS
            and ordered(made, "int wouldDraw = market.ResaleUpTo(at, carried + u + 1);",
                        "if (TradeRules.TheBuyerCouldNotPay(wouldDraw, till)) break;",
                        "TradeMath.Realizable(wouldDraw - drawn, s.ResaleSafetyFactor)",
                        "made += realizable - here;")
            and "internal float Worth;" in S['Passes.cs']
            and "Margin" not in method_body(S['Passes.cs'], "internal struct Pick")
            and all(one in MATHTESTS for one in
                    ("A_pick_is_worth_what_you_could_actually_take_of_it_not_its_percentage",
                     "A_thin_purse_holds_the_take_down_to_what_it_can_pay_for",
                     "A_full_cargo_holds_the_take_down_to_what_still_fits",
                     "A_good_that_weighs_nothing_is_held_back_by_the_purse_alone")))


chk("1.83.0", "the buy shelf is ranked by what each good would really make you for what you could take of it, so a fat percentage on a cheap good no longer outranks a thinner one on a dear good",
    the_shelf_is_ranked_by_what_a_pick_would_really_make())


chk("1.82.0", "a market holding only a few units of a costly good is offered where what it holds is worth enough, so a rare good is no longer passed over for being rare",
    a_costly_good_is_not_filtered_out_for_being_rare())
chk("1.82.0", "walking into a market the ledger routes a good out of, that good is bought before anything else on the shelf",
    the_pass_buys_what_the_ledger_sent_you_for_first())




TRADE_GOODS = (
    "grain", "grape", "date", "olive", "fish", "flax", "clay", "wool", "hardwood", "butter",
    "cheese", "meat", "salt", "hide", "iron", "silver", "leather", "linen", "tool", "oil",
    "cotton", "silk", "velvet", "jewel", "wine", "pottery", "fur", "spice", "charcoal",
    "ingot", "whale", "beer", "honey", "wax", "amber", "papyrus", "marble",
)

NAMES_A_GOOD_ON_PURPOSE = {
    "TL239": "the switch is the never-buy-grain switch",
    "TL339": "the switch is the never-buy-grain switch",
    "TL352": "it points at the never-buy-grain switch this list overrides",
    "TL388": "the log line is about grain being left alone",
    "TL331": "it shows the shape of an item id",
    "TL348": "it shows the shape of the trade summary line",
    "TL323": "it says what the crafting category holds",
}

def a_hint_says_what_it_does_for_every_good_rather_than_naming_a_few():
    en = spoken(ENGLISH)
    named = set(re.findall(r'HintText = "\{=(TL\d+)\}', M)) | \
            set(re.findall(r'SettingProperty\w+\("\{=(TL\d+)\}', M))
    astray = []
    for sid in sorted(named):
        if sid in NAMES_A_GOOD_ON_PURPOSE:
            continue
        said = en.get(sid, "").lower()
        for good in TRADE_GOODS:
            if re.search(r'\b' + good + r's?\b', said):
                astray.append(sid + " names " + good)
    worth = en.get("TL453", "")
    return (astray == []
            and "The dearer a good is, the fewer of it a market need hold." in worth
            and all("dearer" not in NAMES_A_GOOD_ON_PURPOSE.get(sid, "x") for sid in named)
            and len(NAMES_A_GOOD_ON_PURPOSE) == 7
            and all(sid in en for sid in NAMES_A_GOOD_ON_PURPOSE))


chk("1.82.2", "a setting on the screen says what it does for every good rather than naming a few of them, except where naming the good is the whole point of that setting",
    a_hint_says_what_it_does_for_every_good_rather_than_naming_a_few())




def a_buyer_is_picked_by_what_it_earns_a_day_not_by_its_price_alone():
    far = method_body(S['Ledger.cs'], "internal (Settlement town, int price, Ladder rungs) WhereThisEarnsFastest")
    rate = method_body(S['TradeMath.cs'], "public static float EarnedPerDay")
    want = method_body(S['Passes.cs'], "internal static List<Pick> WhatToBuy")
    return ("public const float NoTripCountsShorterThan = 0.5f;" in S['TradeMath.cs']
            and "if (sellPrice <= 0 || sellPrice <= paid) return 0f;" in rate
            and "float over = days > NoTripCountsShorterThan ? days : NoTripCountsShorterThan;" in
                method_body(S['TradeMath.cs'], "public static float PerDay")
            and "return amount / over;" in method_body(S['TradeMath.cs'], "public static float PerDay")
            and "return PerDay(sellPrice - paid, days);" in rate
            and "float days = Travel.EstimateDaysFromParty(town);" in far
            and "float rate = TradeMath.EarnedPerDay(price, paid, days);" in far
            and "int at = shortlist.Count;" in far
            and "while (at > 0 && rate > shortlist[at - 1].rate) at--;" in far
            and "var markets = EverySell(item);" in far
            and "bool ResaleMarket(int at, int paid, int units, out int price);" in S['Passes.cs']
            and ordered(want, "int here = market.PriceToBuy(one.At);",
                        "market.ResaleMarket(one.At, here, carried + take, out int elsewhere)")
            and "BestSell(" not in method_body(S['Trading.cs'], "private sealed class BuyingAt")
            and all(one in MATHTESTS for one in
                    ("A_nearer_buyer_paying_a_little_less_earns_more_a_day_than_a_far_one",
                     "A_far_buyer_paying_much_more_still_wins",
                     "A_trip_shorter_than_half_a_day_counts_as_half_a_day",
                     "A_town_at_the_door_paying_far_less_no_longer_outpaces_a_richer_one_a_day_away",
                     "A_buyer_paying_no_more_than_you_paid_earns_nothing_a_day")))


chk("1.83.1", "the market a buy is judged against is the one that would earn the money back fastest, so a buyer further down the road no longer wins for paying a little more",
    a_buyer_is_picked_by_what_it_earns_a_day_not_by_its_price_alone())




def the_marker_picks_the_market_that_earns_fastest_not_the_one_paying_most():
    marker = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    why = (method_body(S['Marker.cs'], "private static string Why") + "\n" +
           method_body(S['Marker.cs'], "private static string TheNextBest"))
    rule = method_body(S['TradeMath.cs'], "public static float PerDay")
    return ("if (amount <= 0f || float.IsNaN(amount) || float.IsNaN(days)) return 0f;" in rule
            and "return amount / over;" in rule
            and "internal float Rate;" in S['Marker.cs']
            and "internal float RunnerUpRate;" in S['Marker.cs']
            and "internal float Days;" in S['Marker.cs']
            and "float rate = TradeMath.PerDay(earned, ride);" in marker
            and "float weighed = TradeMath.RateTheMarkHolds(rate, s == _picked);" in marker
            and "if (weighed > bar)" in marker
            and "how.Days = ride;" in marker
            and "DearestPurseFirst" not in S['Marker.cs']
            and "Travel.EstimateDaysFromParty(how.Best)" not in why
            and 'how.Days.ToString("0.#") + " day(s) away, so " +' in why
            and 'how.Rate.ToString("0") + " gold a day"' in why
            and 'how.RunnerUpRate.ToString("0") + " gold a day for " +' in why
            and all(one in MATHTESTS for one in
                    ("A_nearer_market_paying_less_in_all_still_earns_more_a_day",
                     "A_far_market_paying_far_more_still_wins",
                     "Nothing_to_carry_there_and_no_trip_at_all_are_both_handled")))


chk("1.83.3", "the market marked on your map is the one that would earn the most a day for the cargo you carry, not the one paying the most in all, and the log says the rate it picked on",
    the_marker_picks_the_market_that_earns_fastest_not_the_one_paying_most())


def every_extra_line_is_behind_the_one_log_switch_that_ships_on():
    screen = method_body(M, "public bool ExtendedDebugLogging")
    marker = S['Marker.cs']
    lift = method_body(S['Migrate.cs'], "private static bool ThreeLogSwitchesBecameOne")
    return ("public bool ExtendedDebugLogging = true;" in S['Options.cs']
            and EVER_SHIPPED.get("ExtendedDebugLogging") == "bool"
            and re.search(r'\{=TL457\}Enable extended debug logging', M) is not None
            and '[SettingPropertyGroup("{=TL107}Debug", GroupOrder = 8)]\n'
                '        public bool ExtendedDebugLogging' in M
            and M.count("[SettingPropertyGroup(\"{=TL107}Debug\"") == 1
            and screen.count("_o.ExtendedDebugLogging") == 2
            and marker.count("Options.Current.ExtendedDebugLogging") == 4
            and all("if (!Options.Current.ExtendedDebugLogging) return;" in method_body(marker, one)
                    for one in ("private static void Ultra",
                                "private static void SayItWeighedAgain",
                                "internal static void ScoreTheMark"))
            and "bool ultra = Options.Current.ExtendedDebugLogging;" in
                method_body(marker, "private static Settlement BestSellTownForCargo")
            and 'foreach (string was in new[] { "PriceTrace", "ForecastScore", "Ultralog" })' in lift
            and 'written[LogSwitch] = wanted ? "true" : "false";' in lift
            and "if (from < 16) changed |= ThreeLogSwitchesBecameOne(written, notes);" in
                method_body(S['Migrate.cs'], "public static bool Lift")
            and "TheThreeLogSwitchesAreOneSettingNow" in MIGRATIONTESTS)


chk("1.87.0", "the price trace, the forecast score and the marker's own workings are behind one switch that ships on, and a settings file that carried the three is brought forward to it",
    every_extra_line_is_behind_the_one_log_switch_that_ships_on())


def the_marker_goes_on_what_you_keep_not_on_what_the_market_pays():
    marker = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    fetch = method_body(S['Marker.cs'], "private static Takings WhatItWouldFetch")
    why = method_body(S['Marker.cs'], "private static string Why")
    ultra = method_body(S['Marker.cs'], "private static void Ultra")
    return (ordered(marker, "long total = took.Value > gold ? gold : took.Value;",
                    "long earned = total - took.Cost;",
                    "float rate = TradeMath.PerDay(earned, ride);")
            and "TradeMath.PerDay(total, ride)" not in marker
            and "took.Cost += (long)worth * moved;" in fetch
            and "how.Cost = took.Cost;" in marker
            and "internal long Earned;" in method_body(S['Marker.cs'], "private struct Weighing")
            and '" of it profit"' in why
            and '" is what it cost you, so it marked on the "' in ultra
            and '" of it profit, "' in method_body(S['Marker.cs'], "internal static void ScoreTheMark"))


chk("1.85.0", "the market marked on your map is the one that leaves you the most a day after what the cargo cost you, not the one that takes the most off you a day",
    the_marker_goes_on_what_you_keep_not_on_what_the_market_pays())


def the_marker_walks_the_price_down_the_way_a_sale_really_would():
    asked = method_body(S['Marker.cs'], "internal int At(int taken)")
    fetch = method_body(S['Marker.cs'], "private static Takings WhatItWouldFetch")
    step = method_body(S['Marker.cs'], "private int Next()")
    return ("new Ladder(_site, _el, true, _flat, 0);" in step
            and "_walk = walk != null && walk.Walkable ? walk : null;" in step
            and "return _walk != null ? _walk.At(_rungs.Count) : _flat;" in step
            and "while (_rungs.Count <= taken) _rungs.Add(Next());" in asked
            and ordered(fetch, "for (int u = 0; u < amount; u++)",
                        "int price = pays.At(u);", "if (price <= 0) break;",
                        "if (price < floor) break;", "fetched += price;", "moved++;",
                        "if (moved == 0) continue;", "took.Value += fetched;",
                        "took.Units += moved;")
            and "internal int At(int taken)" in S['Market.cs']
            and "if (_selling) _shelf.Restock(1); else _shelf.Restock(-1);" in S['Market.cs'])


chk("1.85.0", "the map marker walks a stack down the price ladder the way a sale really would, and counts only the units the selling rules would move",
    the_marker_walks_the_price_down_the_way_a_sale_really_would())


def a_stack_is_judged_unit_by_unit_against_the_market_that_would_buy_it():
    buy = buy_pass()
    rungs = method_body(S['Market.cs'], "internal int Through(int units)")
    return (ordered(method_body(S['Passes.cs'], "internal static Traded BuyThem"),
                    "int held = market.Carried(picked.At) + books.Held(sim, good.Id);",
                    "int till = market.ResaleTill(picked.At);",
                    "int drawn = market.ResaleUpTo(picked.At, held);",
                    "int wouldDraw = market.ResaleUpTo(picked.At, held + 1);",
                    "if (TradeRules.TheBuyerCouldNotPay(wouldDraw, till))",
                    "{ tally.Note(Block.BuyerTillEmpty); break; }",
                    "TradeMath.Realizable(wouldDraw - drawn,")
            and method_body(S['Passes.cs'], "internal static Traded BuyThem").count("drawn = wouldDraw;") == 2
            and "int ResaleUpTo(int at, int units);" in S['Passes.cs']
            and "int ResaleTill(int at);" in S['Passes.cs']
            and "Realizable" not in method_body(S['Passes.cs'], "internal struct Pick")
            and "Carried" not in method_body(S['Passes.cs'], "internal struct Pick")
            and "elsewhere.rungs ?? new Ladder(buyer, Item(at), true, price, 0)," in buy
            and "far.rungs.Through(units)" in buy
            and "int price = At(_running.Count);" in rungs
            and "if (price <= 0) { _stopsAt = _running.Count; break; }" in rungs
            and "_running.Add((_running.Count == 0 ? 0L : _running[_running.Count - 1]) + price);" in rungs
            and "internal static bool TheBuyerCouldNotPay(int drawnSoFar, int till) =>" in S['Rules.cs']
            and "till > 0 && drawnSoFar > till;" in S['Rules.cs']
            and "the market you would sell them in cannot pay for more" in S['Reasons.cs']
            and "TL456" in strings_declared()
            and all(one in BUYPASSTESTS for one in
                    ("A_stack_is_bought_only_while_the_far_market_still_pays_for_one_more",
                     "A_far_market_that_cannot_pay_for_the_whole_stack_stops_the_buying",
                     "What_you_already_carry_is_counted_against_the_far_market_before_you_buy_more")))


chk("1.85.1", "each unit of a stack is judged against what the market that would buy it pays for that unit, counting what you already carry, and the buying stops where that market runs out of gold",
    a_stack_is_judged_unit_by_unit_against_the_market_that_would_buy_it())


def the_selling_pass_takes_what_makes_the_most_first():
    plan = method_body(S['Trading.cs'], "internal SellingFrom(Pass pass, string what, string named)")
    gain = method_body(S['Trading.cs'], "private static int WhatThisStackWouldMake")
    return ("order.Add((held, TradePolicy.CouldBeSold(held, pass.Locked)" in plan
            and "? WhatThisStackWouldMake(pass, held) : 0, at));" in plan
            and "order.Sort((x, y) => x.gain != y.gain ? y.gain.CompareTo(x.gain)" in plan
            and ": x.at.CompareTo(y.at));" in plan
            and "for (int at = 0; at < order.Count; at++) _plan.Add(order[at].held);" in plan
            and "int price = pass.Price(held.EquipmentElement, selling: true);" in gain
            and "long gain = ((long)price - TradePolicy.WorthToBeat(item)) * held.Amount;" in gain
            and "private ItemObject Item(int at) => _plan[at].EquipmentElement.Item;" in S['Trading.cs'])


chk("1.85.1", "the selling pass works down your cargo by what each good would make you, so a merchant who runs out of gold runs out on the goods that would have made you least",
    the_selling_pass_takes_what_makes_the_most_first())


def the_buyers_gold_is_read_only_where_the_mod_reads_the_world_live():
    resale = method_body(S['Trading.cs'],
                              "public bool ResaleMarket(int at, int paid, int units, out int price)")
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes()")
    return (ordered(resale, "Options.Current.Omniscient",
                    "? TradeRules.WhatTheTillCanPay(buyer.SettlementComponent?.Gold ?? 0,",
                    ": 0);")
            and "if (Options.Current.Omniscient)" in scan
            and "till = TradeRules.WhatTheTillCanPay(to.SettlementComponent?.Gold ?? 0," in scan
            and "internal static bool TheBuyerCouldNotPay(int drawnSoFar, int till) =>" in S['Rules.cs']
            and "till > 0 && drawnSoFar > till;" in S['Rules.cs'])


chk("1.85.2", "how much gold a market you would sell in has is read only while Live world prices is on, the same as the route scan reads it",
    the_buyers_gold_is_read_only_where_the_mod_reads_the_world_live())


def the_log_says_which_market_a_good_was_bought_for():
    t = S['Trading.cs']
    meant = method_body(t, "private static string MeantFor")
    detail = method_body(t, "private static void LogDetail")
    resale = method_body(t,
        "public bool ResaleMarket(int at, int paid, int units, out int price)")
    return ('if (selling || item == null || !Options.Current.ExtendedDebugLogging) return "";' in meant
            and 'if (!aimed.TryGetValue(item, out var far) || far.where == null) return "";' in meant
            and '", meant for " + far.where + " at " + far.price + " a unit";' in meant
            and "MeantFor(selling, aimed, kv.Key));" in detail
            and "internal readonly Dictionary<ItemObject, (string where, int price)> Aimed =" in t
            and ordered(resale, "Settlement buyer = elsewhere.town;",
                        "if (buyer == null) return false;",
                        "_pass.Aimed[good] = (Tongue.Named(buyer.Name, buyer.StringId), price);"))


chk("1.86.0", "the log names the market each good was bought for and what it pays a unit there, so cargo that has not sold can be traced back to the market it was bought for",
    the_log_says_which_market_a_good_was_bought_for())


def a_price_that_rose_is_never_written_up_as_a_fall():
    ultra = method_body(S['Marker.cs'], "private static void Ultra")
    return (ordered(ultra, "share.Last == share.Price", '? share.Price + " a unit"',
                    'share.Last < share.Price ? " a unit down to "', '" a unit up to "')
            and '" a unit down to " : " a unit up to "' not in ultra)


chk("1.86.0", "a market that pays a little more for the next unit than the last is written up as a rise, not as a fall",
    a_price_that_rose_is_never_written_up_as_a_fall())


def the_ultralog_says_what_the_marked_market_was_marked_on():
    ultra = method_body(S['Marker.cs'], "private static void Ultra")
    fetch = method_body(S['Marker.cs'], "private static Takings WhatItWouldFetch")
    scan = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    return (ordered(ultra, '"  ultralog: what it marked "', '" of the "', '" you carry, "',
                    '" a unit down to "', '" gold, cost "', '" a unit = "', '" gold, profit "',
                    '" gold in all, of which "',
                    '" is what it cost you, so it marked on the "')
            and "took.Cost += (long)worth * moved;" in fetch
            and "bill?.Add(new Share" in fetch
            and "how.Bill = new List<Share>();" in scan
            and "if (ultra && how.Best != null)" in scan)


chk("1.84.0", "the ultralog breaks the marked market down good by good, with what each fetches there, what it cost you and which of the two the marker went on",
    the_ultralog_says_what_the_marked_market_was_marked_on())


def the_ultralog_lists_the_markets_the_marker_priced():
    ultra = method_body(S['Marker.cs'], "private static void Ultra")
    scan = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    return ("MostMarketsShown" not in S['Marker.cs']
            and "if (ultra) how.Board = new List<Weighing>();" in scan
            and "how.Board?.Add(new Weighing" in scan
            and "y.Rate.CompareTo(x.Rate)" in S['Marker.cs']
            and ordered(ultra, "how.Board.Sort(FastestFirst);",
                        '" market(s) it priced, best first"',
                        "for (int i = 0; i < how.Board.Count; i++)",
                        '" day(s) "', '" unit(s) "',
                        '" gold "', '" gold a day"', '"  (marked)"'))


chk("1.84.0", "the ultralog lists the markets the marker priced, best first, with the days, the units, the gold and the rate of each",
    the_ultralog_lists_the_markets_the_marker_priced())


def the_ultralog_counts_what_the_marker_left_out_and_why():
    scan = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    ultra = method_body(S['Marker.cs'], "private static void Ultra")
    return (all(one in scan for one in
                ("{ how.Shut++; continue; }", "{ how.AtWar++; continue; }",
                 "{ how.NoTill++; continue; }", "{ how.PastCeiling++; continue; }",
                 "{ how.NoRoad++; continue; }", "how.Told++;"))
            and scan.count("how.PastCeiling++") == 2
            and "how.Left = reachable.Count - how.Told;" in scan
            and ordered(ultra, '" market(s) weighed, "', '" priced, "',
                        '" would pay too little for anything you carry, "',
                        '" left unpriced once no purse left could beat "',
                        '"  ultralog: left out before pricing, "', '" with nothing in the till, "',
                        '" past your travel ceilings, "', '" with no road it could find, "',
                        '" under siege, raided or shut, "', '" at war with you"'))


chk("1.84.0", "the ultralog counts every market the marker left out and says what left each of them out",
    the_ultralog_counts_what_the_marker_left_out_and_why())


def the_marker_says_so_when_it_weighs_again_and_stays_put():
    update = method_body(S['Marker.cs'], "internal static void Update")
    again = method_body(S['Marker.cs'], "private static void SayItWeighedAgain")
    stayed = between(update, "if (target == _picked)", "_tracked = null;")
    return ("if (target != null) SayItWeighedAgain(target, how);" in stayed
            and "if (how.Value == _saidValue && how.Rate == _saidRate) return;" in again
            and '"map marker weighed your cargo again and stayed on "' in again
            and ordered(again, "Log.Write(", "Ultra(how);", "Remember(target, how);")
            and ordered(update, "map marker moved to ", "Ultra(how);", "Remember(target, how);"))


chk("1.84.0", "the ultralog writes a line when the marker weighs your cargo again and stays where it is, and only when the reckoning moved",
    the_marker_says_so_when_it_weighs_again_and_stays_put())


def the_mark_is_scored_against_what_that_market_really_paid():
    score = method_body(S['Marker.cs'], "internal static void ScoreTheMark")
    sell = method_body(S['Trading.cs'], "private static void SellPass")
    return (ordered(score, "site.StringId != _markedId", "if (_markedValue <= 0L) return;",
                    "_markedValue = 0L;", "TradeMath.DaysSince(_markedAt, Freshness.Hour)",
                    "TradeMath.HeldShare(", 'Log.Write("marker check at "')
            and "Scoring.Share(held)" in score
            and '" of what it marked on"' in score
            and "if (pass.Site != null && !pass.Sim)" in sell
            and 'Guard.Run("Marker.Check", () => Marker.ScoreTheMark(pass.Site, soldItems, '
                'goldGained));' in sell)


chk("1.84.0", "the ultralog scores the marked market on arrival, once, against what it really paid you",
    the_mark_is_scored_against_what_that_market_really_paid())




GAMER_WORDED_ENTRIES = (
    "Fixed TradeLord buying other goods just because they had a higher margin percentage "
    "when the ledger had suggested a much better trade",
    "Fixed rare high value goods (e.g. jewelry) not being offered in the ledger, because "
    "they are almost never stocked 10 or more in early game",
    "Fixed TradeLord valuing a purchase against a buyer days away instead of a nearby one "
    "paying almost the same",
    "Fixed the map marker pointing at the town paying the most in total instead of the one "
    "paying the most per day",
    "Food is now restocked after trading, so your gold and cargo space go to trade goods first",
)


def the_owners_own_wording_for_an_entry_is_kept_word_for_word():
    return (all("`" + one + "`" in RULES for one in GAMER_WORDED_ENTRIES)
            and len(GAMER_WORDED_ENTRIES) == 5
            and "start with `Fixed`, name the thing that was going wrong, and stop" in RULES
            and "No clause explaining the mechanism behind it" in RULES)


chk("1.83.3", "the owner's own wording for a changelog entry is kept in the working rules word for word, so a session writes entries in it rather than in wording of its own",
    the_owners_own_wording_for_an_entry_is_kept_word_for_word())


RESET_NEVER_WRITTEN = (
    "resets your settings",
    "settings reset",
    "one time settings reset",
    "one-time settings reset",
    "one-time reset",
    "reset ran on",
    "reset never took",
    "so the reset holds",
    "emptied by that reset",
    "emptied by that same reset",
    "put back on this version only",
    "goes back to the value TradeLord ships with",
    "puts every setting back to the value TradeLord ships with, once",
)


def the_one_time_settings_reset_is_never_written_up_for_the_player():
    said = CHANGES.lower()
    return (not [one for one in RESET_NEVER_WRITTEN if one.lower() in said]
            and "The one time settings reset never reaches the player in writing." in RULES
            and "is never written into a changelog entry, a release note or a commit message" in RULES
            and "no entry naming it is left anywhere in the changelog" in RULES
            and "it ships as `[no release]` under a subject that says the settings "
                "file was touched and no more" in RULES)


chk("1.83.3", "the one time settings reset is never written up for the player, so the changelog carries no entry naming it and the working rules keep it out of the notes and the commits",
    the_one_time_settings_reset_is_never_written_up_for_the_player())


WITHDRAWN_NEVER_WRITTEN = (
    "buy what the ledger sent you for first",
    "what to buy first",
    "what the ledger scores highest",
)


def a_trial_that_was_taken_back_out_is_left_in_no_writing():
    said = CHANGES.lower()
    return (not [one for one in WITHDRAWN_NEVER_WRITTEN if one.lower() in said]
            and "Something tried and then taken back out leaves the player nothing" in RULES
            and "the entry that announced it, the entries that changed it while it was there, "
                "and the entry that took it away" in RULES
            and "Out of the changelog, out of the release notes, out of the commit subjects "
                "and bodies that carry them." in RULES
            and "An entry recording that something was looked into and left as it was is the "
                "same case" in RULES
            and "take its section out too, and throw its release and its tag away with it" in RULES)


chk("1.83.3", "a setting that shipped and was taken away again is left in no changelog entry, and the working rules take the whole run of such a thing out of the notes and the commits too",
    a_trial_that_was_taken_back_out_is_left_in_no_writing())


SYNC = io.open('tools/sync_release_notes.py', encoding='utf-8').read()
NOTESFLOW = io.open('.github/workflows/release-notes.yml', encoding='utf-8').read()


def a_release_is_thrown_away_only_where_the_changelog_says_nothing_about_it():
    return ("still has entries in CHANGELOG.md, so it is not dropped" in SYNC
            and "if book.get(version):" in SYNC
            and "'/releases/' + str(row['id']), tok, None, 'DELETE'" in SYNC
            and "'/git/refs/tags/' + row['tag_name']" in SYNC
            and "would throw away the release and the tag for" in SYNC
            and "--drop" in NOTESFLOW
            and "permissions:\n  contents: write" in NOTESFLOW)


chk("1.83.3", "a release and its tag are thrown away only where the changelog carries no entry for that version, and the dry run says which ones would go",
    a_release_is_thrown_away_only_where_the_changelog_says_nothing_about_it())


COMPAT = io.open('tools/compat/Program.cs', encoding='utf-8').read()


def the_menu_id_check_runs_in_the_build_rather_than_skipping():
    wanted = ('DLLsNeeded/NavalDLC.dll', 'DLLsNeeded/SandBox.dll', 'DLLsNeeded/SandBox.View.dll',
              'DLLsNeeded/StoryMode.dll', 'DLLsNeeded/TaleWorlds.CampaignSystem.dll',
              'DLLsNeeded/TaleWorlds.CampaignSystem.ViewModelCollection.dll')
    return (all(os.path.exists(one) for one in wanted)
            and 'TRADELORD_GAME_BIN: ${{ github.workspace }}/DLLsNeeded' in WORKFLOW
            and WORKFLOW.index('TRADELORD_GAME_BIN') < WORKFLOW.index('tools/compat')
            and '!DLLsNeeded/*.dll' in io.open('.gitignore', encoding='utf-8').read()
            and 'skipped  set " + GameBinVariable' in COMPAT)


chk("1.83.3", "the build points the game menu id check at the shipped assemblies the repository carries, so it runs rather than reporting itself skipped",
    the_menu_id_check_runs_in_the_build_rather_than_skipping())


FROZEN = 'archive/'


def the_kept_copies_are_a_closed_record_and_entries_go_only_to_the_changelog():
    kept = io.open(FROZEN + 'changelog.old.md', encoding='utf-8').read()
    history = io.open(FROZEN + 'commithistory.old.md', encoding='utf-8').read()
    return (kept.startswith('# Changelog\n\n## 1.83.3\n')
            and kept.count('\n## ') == 324
            and history.count('\n## ') == 393
            and 'ending at 975806af4c0a31c8c2a164e8950df572d6bb7705' in history
            and SWEEP.count("'" + FROZEN) == 1
            and 'said = CHANGES.lower()' in SWEEP
            and not [one for one in (RELEASED, NEXUS, SYNC, WORKFLOW, NOTESFLOW) if FROZEN in one]
            and 'The kept copies in `archive/` are a closed record.' in RULES
            and 'goes into `CHANGELOG.md` and nowhere else' in RULES)


chk("1.83.3", "the kept copies in the archive folder are a closed record, so a changelog entry is written into CHANGELOG.md and nowhere else and no check reads an entry back out of the archive",
    the_kept_copies_are_a_closed_record_and_entries_go_only_to_the_changelog())


def the_far_market_ladder_is_walked_once_and_handed_on():
    far = method_body(S['Ledger.cs'],
                      "internal (Settlement town, int price, Ladder rungs) WhereThisEarnsFastest")
    walk = method_body(S['Market.cs'], "internal static Fetched SellWalk")
    buy = buy_pass()
    return (far and walk
            and "internal static Fetched SellWalk(Settlement site, ItemObject item, int units, int quoted,"
                in S['Market.cs']
            and "int paid)" in S['Market.cs']
            and "got.Rungs = new Ladder(site, item, true, quoted, 0);" in walk
            and "int price = got.Rungs.At(u);" in walk
            and S['Market.cs'].count("new Ladder(site, item, true, quoted, 0);") == 1
            and "Ladder deepRungs = null;" in far
            and "deepRungs = got.Rungs;" in far
            and "return (deep.town, deep.price, deepRungs);" in far
            and "return (null, 0, null);" in far
            and "return (flat.town, flat.price, null);" in far
            and "elsewhere.rungs ?? new Ladder(buyer, Item(at), true, price, 0)," in buy
            and buy.count("new Ladder(") == 1)


chk("1.89.0", "the market a buy is aimed at has its price ladder walked once and handed on, rather than walked again from nothing once it has won",
    the_far_market_ladder_is_walked_once_and_handed_on())


def the_town_ceiling_says_what_turning_it_off_costs():
    off = "Set it to 0 and the limit comes off, so every town in Calradia is weighed"
    said = spoken(ENGLISH)
    if off not in said['TL306'] or off not in M:
        return False
    for path in TRANSLATIONS.values():
        if '0' not in spoken(path).get('TL306', ''):
            return False
    live = method_body(S['Ledger.cs'], "private List<(Settlement s, float days)> LiveCandidates")
    warn = method_body(S['Ledger.cs'], "private static void SayIfTheTownCeilingIsOff")
    return (live and warn
            and "SayIfTheTownCeilingIsOff(list.Count);" in live
            and "if (Options.Current.MaxTravelDaysTown > 0f) return;" in warn
            and 'Log.Repeatable("town travel ceiling", weighed.ToString(),' in warn
            and "the town travel ceiling is off" in warn)


chk("1.89.0", "the town travel ceiling says on the settings screen what turning it off costs, in every language, and the log says so too while it is off",
    the_town_ceiling_says_what_turning_it_off_costs())


def the_route_scan_says_what_it_cost():
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes")
    say = method_body(S['Ledger.cs'], "private static void SayWhatTheScanCost")
    return (scan and say
            and "long started = System.DateTime.UtcNow.Ticks;" in scan
            and "int opened = 0, thrownAway = 0;" in scan
            and scan.count("opened++;") == 1
            and scan.count("thrownAway++;") == 3
            and ordered(scan,
                        "int openingSell = Bulk.Opening(to, item, true, sellPrice, landedAtSellTown);",
                        "opened++;")
            and "SayWhatTheScanCost(routes.Count, opened, thrownAway," in scan
            and "if (!Options.Current.ExtendedDebugLogging) return;" in say
            and '"route scan: " + found + " route(s) off " + opened +' in say
            and "thrownAway + \" of them thrown away by a later test, in \"" in say)


chk("1.89.0", "the route scan counts the opening prices it asked for and the ones a later test threw away, and writes both with the time it took",
    the_route_scan_says_what_it_cost())


def the_far_markets_are_asked_only_when_their_answer_is_used():
    worth = method_body(S['Policy.cs'], "internal static int WorthToBeat(ItemObject item)")
    rules = S['Rules.cs']
    return (worth
            and "Good good = Describe(item);" in worth
            and "int paid = CostBasis(item);" in worth
            and ordered(worth, "TradeRules.WorthIsWhatYouPaid(good, paid)", "? paid",
                        ": TradeRules.WorthToBeat(good, paid, UnpaidWorth(item));")
            and worth.count("UnpaidWorth(item)") == 1
            and "internal static bool WorthIsWhatYouPaid(in Good good, int paid) =>" in rules
            and rules.count("paid > 0 || !TradedAsMerchandise(good);") == 1
            and "WorthIsWhatYouPaid(good, paid) ? paid : unpaidWorth;" in rules)


chk("1.89.1", "what a good is worth is read from what you paid for it wherever that settles it, so the far markets are asked only when their price is the answer",
    the_far_markets_are_asked_only_when_their_answer_is_used())


def the_sell_order_prices_nothing_it_could_never_sell():
    plan = method_body(S['Trading.cs'], "internal SellingFrom(Pass pass, string what, string named)")
    peek = method_body(S['Policy.cs'],
                       "internal static bool CouldBeSold(ItemRosterElement el, ISet<string> lockedKeys)")
    real = method_body(S['Policy.cs'],
                       "internal static bool MaySell(in Good good, ItemRosterElement el, ISet<string> lockedKeys,")
    return (plan and peek and real
            and "TradePolicy.CouldBeSold(held, pass.Locked)" in plan
            and "? WhatThisStackWouldMake(pass, held) : 0, at));" in plan
            and "facts.AwaitedHeld = 0;" in peek
            and "facts.FoodHeld = 0;" in peek
            and "TradeRules.MaySell(Describe(item), el.Amount, facts, Options.Current," in peek
            and "TakeBack" not in peek
            and real.count("TakeBack(") == 2
            and "for (int at = 0; at < order.Count; at++) _plan.Add(order[at].held);" in plan
            and plan.count("order.Add(") == 1
            and ordered(method_body(S['Passes.cs'], "internal static Traded SellThem"),
                        "for (int at = 0; at < market.Count; at++)",
                        "if (!market.MaySell(at, good, out int keep, out Block stopped)) "
                        "{ tally.Note(stopped); continue; }"))


chk("1.89.1", "the selling pass asks no price for a row its own rules could never move, and still carries every row into the pass so a stall is explained",
    the_sell_order_prices_nothing_it_could_never_sell())


def every_market_in_reach_is_weighed_for_the_whole_load():
    far = method_body(S['Ledger.cs'],
                      "internal (Settlement town, int price, Ladder rungs) WhereThisEarnsFastest")
    return (far
            and "BuyersWeighedOnTheStack" not in S['Ledger.cs']
            and ordered(far,
                        "var markets = EverySell(item);",
                        "if (town == null || price <= 0 || town == notHere) continue;",
                        "if (TradeMath.OutOfReach(days)) continue;",
                        "float rate = TradeMath.EarnedPerDay(price, paid, days);",
                        "while (at > 0 && rate > shortlist[at - 1].rate) at--;",
                        "shortlist.Insert(at, (town, price, days, rate));")
            and "shortlist.RemoveAt(" not in far
            and far.count("shortlist.Insert(") == 1
            and "Fetched got = Bulk.SellWalk(one.town, item, units, one.price, paid);" in far
            and "if (paid > 0 && !TradePolicy.BuyAcceptable(paid, TradePolicy.Realizable(price))) break;"
                in method_body(S['Market.cs'], "internal static Fetched SellWalk")
            and "float rate = TradeMath.PerDay(got.Total - (long)paid * got.Units, one.days);" in far)


chk("1.90.0", "every market in reach is weighed for the whole load, rather than the five that pay most for one unit",
    every_market_in_reach_is_weighed_for_the_whole_load())


def a_ladder_stops_where_the_margin_goes():
    walk = method_body(S['Market.cs'], "internal static Fetched SellWalk")
    route = method_body(S['Market.cs'], "internal static RouteQuote Walk")
    far = method_body(S['Ledger.cs'],
                      "internal (Settlement town, int price, Ladder rungs) WhereThisEarnsFastest")
    step = method_body(S['Marker.cs'], "private int Next()")
    paid = method_body(S['Marker.cs'], "internal int At(int taken)")
    fetch = method_body(S['Marker.cs'], "private static Takings WhatItWouldFetch")
    return (walk and route and far and step and paid and fetch
            and "if (paid > 0 && !TradePolicy.BuyAcceptable(paid, TradePolicy.Realizable(price))) break;"
                in walk
            and "if (!TradePolicy.BuyAcceptable(buyPrice, TradePolicy.Realizable(sellPrice))) break;"
                in route
            and "got.Units++;" in walk
            and "float rate = TradeMath.PerDay(got.Total - (long)paid * got.Units, one.days);" in far
            and "(long)paid * units" not in far
            and "while (_rungs.Count <= taken) _rungs.Add(Next());" in paid
            and "_asked = true;" in step
            and ordered(fetch, "for (int u = 0; u < amount; u++)", "int price = pays.At(u);",
                        "if (!TradeMath.ProfitAcceptable(worth, price, Options.Current.MinProfitMargin)) break;")
            and "upTo" not in S['Marker.cs'])


chk("1.90.0", "a price ladder is walked no further than the margin lasts, so a market is weighed on the units it would really take rather than on the whole load",
    a_ladder_stops_where_the_margin_goes())


def the_marked_town_is_only_given_up_for_a_clear_gain():
    hold = method_body(S['TradeMath.cs'], "public static float RateTheMarkHolds")
    first = method_body(S['Marker.cs'], "private static void TheMarkedTownFirst")
    marker = method_body(S['Marker.cs'], "private static Settlement BestSellTownForCargo")
    said = method_body(S['Marker.cs'], "private static string TheNextBest")
    held = re.search(r'public const float TheMarkedTownHoldsBy = ([\d.]+)f;', S['TradeMath.cs'])
    return (hold and first and said
            and held is not None and float(held.group(1)) > 1.0
            and "if (!marked || rate <= 0f || float.IsNaN(rate)) return rate;" in hold
            and "float held = rate * TheMarkedTownHoldsBy;" in hold
            and "return float.IsInfinity(held) ? rate : held;" in hold
            and ordered(first, "if (_picked == null) return;",
                        "if (reachable[at].s != _picked) continue;",
                        "reachable.RemoveAt(at);", "reachable.Insert(0, held);")
            and "for (int at = 1; at < reachable.Count; at++)" in first
            and ordered(marker, "TheMarkedTownFirst(reachable);", "float bar = 0f;",
                        "if (TradeMath.PerDay(gold, ride) <= bar) break;",
                        "float weighed = TradeMath.RateTheMarkHolds(rate, s == _picked);",
                        "if (weighed > bar)", "bar = weighed;")
            and "how.Held = how.Best != null && how.Best == _picked && how.RunnerUpRate > how.Rate;"
                in marker
            and "internal bool Held;" in S['Marker.cs']
            and '", and it holds the mark against " + next' in said
            and '", because the marker only moves for a clear gain"' in said
            and all(one in MATHTESTS for one in
                    ("A_market_has_to_beat_the_marked_town_by_a_clear_margin",
                     "A_town_that_is_not_marked_is_weighed_at_what_it_pays",
                     "A_marked_town_that_pays_nothing_holds_on_to_nothing")))


chk("1.90.4", "the map marker gives its town up only to a market that earns clearly more, so it stops swapping back and forth while you ride",
    the_marked_town_is_only_given_up_for_a_clear_gain())


def the_forecast_can_never_empty_a_shelf_it_only_expects_to_empty():
    shelf = method_body(S['TradeMath.cs'], "public static int ShelfAfterLanding")
    floor = re.search(r'public const float ShelfTheForecastMayNotEmptyBelow = ([\d.]+)f;',
                      S['TradeMath.cs'])
    return (shelf
            and floor is not None and 0.0 < float(floor.group(1)) < 1.0
            and "long after = (long)inStoreValue + landingWorth;" in shelf
            and "long floor = inStoreValue > 0" in shelf
            and "? (long)(inStoreValue * ShelfTheForecastMayNotEmptyBelow) : 0L;" in shelf
            and "if (after < floor) after = floor;" in shelf
            and "if (after < 0L) return 0;" in shelf
            and "_inStoreValue = TradeMath.ShelfAfterLanding(data.InStoreValue, landed);"
                in S['Market.cs']
            and "int landedAtSellTown = Forecast.WorthShiftAsItHasHeld(to, item, days);" in S['Ledger.cs']
            and all(one in MATHTESTS for one in
                    ("A_shelf_cannot_be_bought_down_past_half_of_what_is_on_it",
                     "A_shelf_the_forecast_adds_to_is_left_where_the_landing_puts_it")))


chk("1.90.4", "what is still on its way to a market can only move that market's price so far, so the ledger stops quoting a price the shelf standing there would never pay",
    the_forecast_can_never_empty_a_shelf_it_only_expects_to_empty())


def what_is_on_its_way_is_counted_at_the_trust_it_has_earned():
    miss = method_body(S['TradeMath.cs'], "public static float MissThatCounts")
    trust = method_body(S['TradeMath.cs'], "public static float TrustInTheForecast")
    held = method_body(S['TradeMath.cs'], "public static int WorthShiftTrusted")
    earned = method_body(S['Forecast.cs'], "internal static float TrustEarned")
    shift = between(S['Forecast.cs'], "internal static int WorthShiftAsItHasHeld", ";")
    kept = method_body(S['Ledger.cs'], "internal void KeepForecastScore")
    read = method_body(S['Ledger.cs'], "internal bool ForecastScore")
    written = method_body(S['Hindsight.cs'], "private static void Written")
    noted = method_body(S['Hindsight.cs'], "private static void Noted")
    scan = method_body(S['Ledger.cs'], "private List<TradeRoute> ScanRoutes()")
    cap = re.search(r'public const float MostAForecastMissCounts = ([\d.]+)f;', S['TradeMath.cs'])
    enough = re.search(r'public const int EnoughForecasts = (\d+);', S['TradeMath.cs'])
    return (miss and trust and held and earned and shift and kept and read and written and scan
            and cap is not None and float(cap.group(1)) > 0
            and enough is not None and int(enough.group(1)) > 0
            and "return missed > MostAForecastMissCounts ? MostAForecastMissCounts : missed;" in miss
            and "float earned = 1f / (1f + missed);" in trust
            and "float weight = (float)scored / (scored + EnoughForecasts);" in trust
            and "float trust = 1f - (1f - earned) * weight;" in trust
            and "if (scored <= 0 || missed < 0f || float.IsNaN(missed) || float.IsInfinity(missed)) return 1f;"
                in trust
            and "long held = (long)((double)shift * trust);" in held
            and "return TradeMath.TrustInTheForecast(scored, missed);" in earned
            and "TradeMath.WorthShiftTrusted(WorthShift(site, item, withinDays), TrustEarned())" in shift
            and "_forecastsScored++;" in kept and "_forecastMissed += missed;" in kept
            and "missed = TradeMath.MeanOf(_forecastMissed, _forecastsScored);" in read
            and 'dataStore.SyncData("TradeLord_ForecastsScored", ref _forecastsScored);' in S['Ledger.cs']
            and 'dataStore.SyncData("TradeLord_ForecastMissed", ref _forecastMissed);' in S['Ledger.cs']
            and "LedgerBehavior.Instance?.KeepForecastScore(TradeMath.MissThatCounts(how.Share));"
                in written
            and "Forecast.WorthShift(site, item, withinDays)" in noted
            and "WorthShiftAsItHasHeld" not in noted
            and scan.count("Forecast.WorthShiftAsItHasHeld(") == 2
            and scan.count("Forecast.WorthShift(") == 0
            and S['TooltipPatches.cs'].count("Forecast.WorthShiftAsItHasHeld(") == 1
            and "Forecast.WorthShift(" not in S['TooltipPatches.cs']
            and all(one in MATHTESTS for one in
                    ("One_wild_miss_cannot_speak_for_the_whole_forecast",
                     "A_forecast_that_has_never_been_checked_is_taken_at_its_word",
                     "A_forecast_that_keeps_missing_is_believed_less_and_less",
                     "What_is_on_its_way_is_counted_at_the_trust_it_has_earned",
                     "A_forecast_that_has_been_missing_moves_a_shelf_less_than_one_that_has_not")))


chk("1.90.5", "what is still on its way is counted at the trust the forecast has earned, measured from what it said against what really moved, with one wild miss counting no more than the cap",
    what_is_on_its_way_is_counted_at_the_trust_it_has_earned())


def the_ledger_bounds_a_forecast_price_the_way_the_tooltip_already_did():
    opening = method_body(S['Market.cs'], "internal static int Opening")
    first = method_body(S['Market.cs'], "internal static int FirstUnit")
    return (opening and first
            and "int walked = Rung(site, item, selling, quoted, landed).At(0);" in opening
            and "return landed == 0 ? walked : TradeMath.ForecastWithin(quoted, walked);" in opening
            and "return rung.Walkable ? TradeMath.ForecastWithin(quoted, rung.At(0)) : quoted;" in first
            and "int openingBuy = Bulk.Opening(from, item, false, buyPrice, landedAtBuyTown);"
                in S['Ledger.cs']
            and "int openingSell = Bulk.Opening(to, item, true, sellPrice, landedAtSellTown);"
                in S['Ledger.cs'])


chk("1.90.5", "the price the ledger opens a route at is held within reach of the price standing there, the same bound the tooltip already kept",
    the_ledger_bounds_a_forecast_price_the_way_the_tooltip_already_did())


def the_forecast_is_scored_whatever_the_debug_switch_says():
    h = S['Hindsight.cs']
    note = method_body(h, "internal static void Note")
    score = method_body(h, "internal static void Score")
    written = method_body(h, "private static void Written")
    return (note and score and written
            and "internal static bool On => Forecast.On;" in h
            and "Writing" not in note and "Writing" not in score
            and written.find("LedgerBehavior.Instance?.KeepForecastScore(") <
                written.find("if (!Writing || scored + stale + early == 0) return;")
            and "if (!Writing || scored + stale + early == 0) return;" in written
            and "if (!Writing || scored + stale + yours + unpriced == 0) return;" in method_body(h, "private static void Kept"))


chk("1.90.6", "the forecast is held to what really moved whatever the debug switch says, so the share it is counted at is learned by every campaign and not only by one writing a log",
    the_forecast_is_scored_whatever_the_debug_switch_says())


def a_forecast_too_old_to_say_anything_is_passed_over_rather_than_scored():
    h = S['Hindsight.cs']
    written = method_body(h, "private static void Written")
    kept = method_body(h, "private static void Kept")
    return (written and kept
            and ordered(written, "if (kept.Item == null) continue;",
                        "if (Scoring.TooOldToSay(kept.WithinDays, kept.AtHours, now, out float since))",
                        "stale++;", "scored++;",
                        "LedgerBehavior.Instance?.KeepForecastScore(TradeMath.MissThatCounts(how.Share));")
            and written.count("scored++;") == 1
            and '" passed over as too old to say anything"' in written
            and '" passed over as too old to say anything"' in kept)


chk("1.90.9", "a forecast figure the ride outlived is passed over rather than scored, the way an old promise already was, so the share what is on its way is counted at is learned only from figures still worth holding to",
    a_forecast_too_old_to_say_anything_is_passed_over_rather_than_scored())


def a_quest_good_is_held_back_once_and_not_again_as_food():
    kept = method_body(S['Policy.cs'], "internal static Dictionary<ItemObject, int> KeptBack")
    relief = method_body(S['Trading.cs'], "public static void ExecuteHerdRelief")
    return (kept and relief
            and "keep[" not in kept
            and "foreach (var owed in awaited)" not in kept
            and ordered(kept, "Named(TradeRules.FoodKeep(carried, AppetitePerDay(), Options.Current), byId);",
                        "awaited = Errands.Promised(out int anyLivestock);",
                        "awaited[owed.Key] = had + owed.Value;")
            and "heldBack" not in relief
            and "if (promised.TryGetValue(item, out int owed) && owed > 0)" in relief
            and "said.KeepCount += reserved;" in sell_rule()
            and "A_good_a_quest_wants_and_the_larder_wants_is_held_back_for_both" in SELLTESTS)


chk("1.90.10", "a good a quest is waiting on is held back once for the quest, and the food reserve holds back only the food it set aside, so a quest good is never kept a second time as food",
    a_quest_good_is_held_back_once_and_not_again_as_food())


def what_is_on_its_way_moves_a_route_no_further_than_its_first_unit_may_move():
    rung = method_body(S['Market.cs'], "private static Ladder Rung")
    held = method_body(S['Market.cs'], "private static Ladder Held")
    first = method_body(S['Market.cs'], "internal static int FirstUnit")
    opening = method_body(S['Market.cs'], "internal static int Opening")
    walk = method_body(S['Market.cs'], "internal static RouteQuote Walk")
    reach = method_body(S['TradeMath.cs'], "public static int LandingWithinReach")
    return (rung and held and first and opening and walk and reach
            and "rung = Held(site, item, selling, quoted, landed, scanning: true);" in rung
            and "new Ladder(" not in rung
            and "Ladder rung = Held(site, item, selling, quoted, landed, scanning: false);" in first
            and "new Ladder(" not in first
            and "return rung.Walkable ? TradeMath.ForecastWithin(quoted, rung.At(0)) : quoted;" in first
            and "int walked = Rung(site, item, selling, quoted, landed).At(0);" in opening
            and ordered(held, "var rung = new Ladder(site, item, selling, quoted, landed);",
                        "if (landed == 0 || !rung.Walkable) return rung;",
                        "int first = rung.At(0);",
                        "if (TradeMath.ForecastWithin(quoted, first) == first) return rung;",
                        "held = TradeMath.LandingWithinReach(quoted, landed, first,",
                        "shift => new Ladder(site, item, selling, quoted, shift).At(0));",
                        "return new Ladder(site, item, selling, quoted, TradeMath.NoFurtherThan(landed, held));")
            and held.count("new Ladder(") == 3
            and "landed: landedAtBuyTown);" in walk and "landed: landedAtSellTown);" in walk
            and "int buyPrice = buy.At(u);" in walk and "int sellPrice = sell.At(u);" in walk
            and "if (landed == 0 || ForecastWithin(live, firstUnit) == firstUnit) return landed;" in reach
            and "int held = MostThatHolds(most, shift =>" in reach
            and "return ForecastWithin(live, price) == price;" in reach
            and "return landed > 0 ? held : -held;" in reach
            and all(one in MATHTESTS for one in
                    ("A_price_the_forecast_did_not_move_is_left_exactly_where_it_was",
                     "A_landing_that_keeps_the_first_unit_within_reach_is_left_whole",
                     "A_landing_that_would_move_the_first_unit_too_far_is_held_to_the_most_that_keeps_it_within_reach")))


chk("1.90.11", "what is still on its way to a market is held to the most that keeps that market's first unit within reach of what it pays now, so every unit the ledger walks, its price and its profit sit on the bound the tooltip already kept",
    what_is_on_its_way_moves_a_route_no_further_than_its_first_unit_may_move())


def the_halving_search_reaches_the_largest_whole_number_without_wrapping_round():
    most = method_body(S['TradeMath.cs'], "public static int MostThatHolds")
    return (most
            and "int mid = lowest + (int)(((long)highest - lowest + 1) / 2);" in most
            and "The_halving_search_reaches_the_largest_whole_number_there_is" in MATHTESTS)


chk("1.90.11", "the halving search that holds back what is on its way reaches the largest landing there is without wrapping round, so a huge forecast can never freeze the route scan or the tooltip",
    the_halving_search_reaches_the_largest_whole_number_without_wrapping_round())


def how_far_a_landing_may_move_a_market_is_found_once_a_scan():
    m = S['Market.cs']
    held = method_body(m, "private static Ladder Held")
    forget = method_body(between(m, "internal static class Bulk", "internal static class Priced"),
                         "internal static void Forget()")
    further = method_body(S['TradeMath.cs'], "public static int NoFurtherThan")
    return (held and forget
            and "private static readonly Dictionary<(string site, string item, bool selling, bool arriving), int> _reach =" in m
            and ordered(held, "var way = (site.StringId, item.StringId, selling, landed > 0);",
                        "if (!scanning || !_reach.TryGetValue(way, out int held))",
                        "held = TradeMath.LandingWithinReach(",
                        "if (scanning) _reach[way] = held;",
                        "TradeMath.NoFurtherThan(landed, held)")
            and "_rungs.Clear();" in forget and "_reach.Clear();" in forget
            and "public static int NoFurtherThan(int landed, int shift) =>" in S['TradeMath.cs']
            and "landed > 0 ? Math.Min(shift, landed) : Math.Max(shift, landed);" in S['TradeMath.cs']
            and all(one in MATHTESTS for one in
                    ("Every_landing_too_far_the_same_way_is_held_to_the_same_most",
                     "A_held_landing_never_reaches_further_than_the_landing_itself")))


chk("1.90.11", "how far what is on its way may move a market is searched for once per town, good, side and way in a scan and forgotten with the ladders, and never moves a route further than the landing itself, so holding the forecast back costs a scan one search per market rather than one per route",
    how_far_a_landing_may_move_a_market_is_found_once_a_scan())


def every_stack_of_a_good_is_counted_rather_than_the_first():
    l = S['Ledger.cs']
    counted = method_body(l, "internal static int InAll")
    stocks = method_body(l, "private static Dictionary<ItemObject, int> WhatItStocks")
    return (counted and stocks
            and "for (int i = 0; roster != null && i < roster.Count; i++)" in counted
            and "if (roster.GetItemAtIndex(i) == item) held += roster.GetElementNumber(i);" in counted
            and "return InAll(s.ItemRoster, item);" in method_body(l, "internal static int StockOf")
            and "held[item] = had + shelf.GetElementNumber(i);" in stocks
            and "int took = Math.Min(bought, InAll(carried, item));" in l
            and S['Trading.cs'].count("LedgerBehavior.InAll(") == 5
            and "GetItemNumber(" not in ALL + "\n" + M)


chk("1.90.12", "every stack of a good is counted, in your bags and on a market's shelves, since the game's own count stops at the first stack and loot or animals of another quality sit in stacks of their own",
    every_stack_of_a_good_is_counted_rather_than_the_first())


def a_forecast_figure_too_old_to_say_anything_makes_room_for_a_new_one():
    h = S['Hindsight.cs']
    noted = method_body(h, "private static void Noted")
    room = method_body(h, "private static bool RoomForOneMoreFigure")
    return (noted and room
            and "if (!_said.Holds(site.StringId, item.StringId) && _said.Full && !RoomForOneMoreFigure())"
                in noted
            and "return _said.Prune(" in room
            and "one => !Scoring.TooOldToSay(one.WithinDays, one.AtHours, now, out _));" in room
            and 'Log.Repeatable("forecast check", "full",' in noted
            and "Clearing_out_old_promises_drops_a_market_it_no_longer_holds_any_for" in SCORINGTESTS
            and "A_full_store_that_can_free_nothing_says_there_is_no_room" in SCORINGTESTS)


chk("1.90.12", "a forecast figure too old to say anything is cleared out once the store is full, the way an old promise already was, so a long session keeps checking the forecast at every market it reaches",
    a_forecast_figure_too_old_to_say_anything_makes_room_for_a_new_one())


def the_whole_load_switch_says_every_market_in_reach_is_weighed():
    said = spoken(ENGLISH)['TL460']
    return ("ON: every market in reach is weighed again on what it would pay for the whole load" in said
            and "five" not in said
            and said_in_every_language('TL460'))


chk("1.90.12", "the hint under the whole load switch says every market in reach is weighed, as it has been since the five best stopped being the limit",
    the_whole_load_switch_says_every_market_in_reach_is_weighed())


def a_line_that_names_a_setting_names_it_the_way_the_screen_shows_it():
    named = (('TL450', 'TL205'), ('TL451', 'TL205'), ('TL453', 'TL205'),
             ('TL451', 'TL452'), ('TL458', 'TL279'))
    for path in [ENGLISH] + list(TRANSLATIONS.values()):
        said = spoken(path)
        if not all(said[setting] in said[line] for line, setting in named):
            return False
    turkish = spoken(TRANSLATIONS['Türkçe'])
    return all(any(letter in turkish[one] for letter in 'çğıöşü')
               for one in ('TL450', 'TL451', 'TL452', 'TL453'))


chk("1.90.12", "the lines about minimum stock and the debug logging hint name the setting they lean on in every language exactly as the settings screen shows it, and the Turkish stock lines are written with Turkish letters",
    a_line_that_names_a_setting_names_it_the_way_the_screen_shows_it())


def the_panel_legend_names_its_columns_the_way_the_panel_heads_them():
    shown = lambda said: re.sub(r'\s*[(（][^()（）]*[)）]\s*$', '', said)
    named = (('TL397', 'TL54'), ('TL417', 'TL416'), ('TL417', 'TL54'), ('TL417', 'TL59'),
             ('TL71', 'TL59'), ('TL396', 'TL201'))
    for path in [ENGLISH] + list(TRANSLATIONS.values()):
        said = spoken(path)
        if not all(shown(said[head]) in said[line] for line, head in named):
            return False
    turkish = spoken(TRANSLATIONS['Türkçe'])
    return (all('Öntanımlı' not in one for one in turkish.values())
            and turkish['TL260'].split(' ')[0] == turkish['TL216'].split(' ')[0]
            and turkish['TL260'].split(' ')[-1] == turkish['TL216'].split(' ')[-1])


chk("1.90.13", "the panel legend names Qty, Left and Conf, and the forecast hint names Live world prices, in every language the way the panel and the settings screen show them, and the Turkish settings say default and menu entry one way throughout",
    the_panel_legend_names_its_columns_the_way_the_panel_heads_them())


def the_release_check_reads_and_writes_every_letter_the_same_on_every_machine():
    read_gh = between(RELEASED, "def byGh():", "\ndef ")
    read_git = between(RELEASED, "def thisCommit():", "\ndef ")
    main = between(RELEASED, "def main(argv):", "said = {head: entries for head, entries in")
    return (read_gh and read_git and main
            and "capture_output=True, encoding='utf-8')" in read_gh
            and "capture_output=True, encoding='utf-8')" in read_git
            and "text=True" not in RELEASED
            and "sys.stdout.reconfigure(encoding='utf-8')" in main)


chk("1.90.13", "the release check reads the commit message and the published notes as UTF-8 and writes UTF-8, so a Turkish letter in a changelog entry can no longer stop a version going out on the Windows build",
    the_release_check_reads_and_writes_every_letter_the_same_on_every_machine())


def a_release_that_reads_back_with_no_file_is_asked_again_before_it_is_called_bare():
    fault = between(RELEASED, "for version in sorted(live, key=order):", "if version not in said:")
    again = between(RELEASED, "def bare(tag):", "\ndef ")
    one = between(RELEASED, "def filesOn(tag):", "\ndef ")
    return (fault and again and one
            and "if row['files'] == 0 and bare(row['tag']):" in fault
            and "time.sleep(" in again
            and "if filesOn(tag):" in again
            and again.rstrip().endswith("return True")
            and "'/releases/tags/' + tag" in one
            and "capture_output=True, encoding='utf-8')" in one)


chk("1.90.13", "a published version that GitHub lists with no file is asked about again by its tag before the release check calls it bare, because GitHub sometimes lists a release's file and sometimes leaves it out",
    a_release_that_reads_back_with_no_file_is_asked_again_before_it_is_called_bare())


def the_trade_entry_shows_while_staged_trading_holds_trading_back():
    menu = between(S['Trading.cs'], '"tradelord_quicktrade"', 'args => Guard.Run("Action.QuickTradeMenu"')
    entered = method_body(S['Trading.cs'], "private void OnSettlementEntered")
    for path in [ENGLISH] + list(TRANSLATIONS.values()):
        said = spoken(path)
        if said['TL281'] not in said['TL316']:
            return False
    return (menu
            and "return (Options.Current.QuickSellMenu || Counter.HoldsBack()) && CanTradeHere(Settlement.CurrentSettlement);" in menu
            and ordered(entered, "_visitTradeAllowed = CanTradeHere(settlement);",
                        "if (_visitTradeAllowed && Counter.HoldsBack())",
                        "Notices.Say(TheDealWaitsForYou(), Notices.Note);")
            and "While Staged Trading holds that trading back, the entry shows anyway." in M
            and "While Staged Trading holds that trading back, the entry shows anyway." in spoken(ENGLISH)['TL316'])


chk("1.90.14", "the trade entry shows while Staged Trading holds trading back as you arrive, even with Trade entry in town menu off, the notice that names the entry only shows where the entry does, and the hint under Trade entry in town menu says so in every language",
    the_trade_entry_shows_while_staged_trading_holds_trading_back())


def the_turkish_text_speaks_to_you_as_siz_throughout():
    turkish = spoken(TRANSLATIONS['Türkçe'])
    letter = '[\\wçğıöşüÇĞİÖŞÜ]'
    informal = re.compile('(?<!' + letter + ')(sen|senin|sana|seni|senden|sende|kesen|kesende|işyerin|kademen|'
                          'kampanyan|partin|baktığın|yazdığın|girdiğin|taşıdığın|satacağın|olabileceğin|'
                          'kaydettiğin|ödediğin|yoldaşlarının)(?!' + letter + ')', re.I)
    dry = [one for one in turkish.values() if one.startswith('[Benzetim')]
    return (not [one for one in turkish.values() if informal.search(one)]
            and not [one for one in turkish.values() if 'denar' in one]
            and len(dry) == 5 and all(one.startswith('[Benzetim, en iyi durum]') for one in dry)
            and turkish['TL429'].split(' (')[0] in turkish['TL435']
            and turkish['TL403'].endswith('{ENTRY} seçin.'))


chk("1.90.15", "the Turkish text calls the player siz everywhere, writes dinar in every trade message and tags every dry run line the same way",
    the_turkish_text_speaks_to_you_as_siz_throughout())


def your_own_buying_and_selling_is_left_out_of_a_forecast_figure():
    h = S['Hindsight.cs']
    said = method_body(h, "private struct Said")
    left = method_body(h, "private static void LeftOutOfTheForecast")
    written = method_body(h, "private static void Written")
    weighed = method_body(S['Scoring.cs'], "internal static Outcome Weigh")
    worth = method_body(S['TradeMath.cs'], "public static int YourOwnWorth")
    return (said and left and written and weighed and worth
            and "internal int StockYours;" in said and "internal int WorthYours;" in said
            and "_said.Rework(site.StringId, kept =>" in left
            and "bool worthKept = site.IsTown;" in left
            and ordered(left, "(kept.StockYours, kept.WorthYours) = Scoring.YoursAdded(",
                        "kept.StockYours, kept.WorthYours, item == kept.Item,",
                        "item.ItemCategory != null && item.ItemCategory == kept.Item.ItemCategory,",
                        "worthKept, into, item.Value);")
            and "return (sameGood ? TradeMath.AddedUp(stockYours, into) : stockYours," in
                method_body(S['Scoring.cs'], "internal static (int stock, int worth) YoursAdded")
            and "worthKept && sameKind ? TradeMath.AddedUp(worthYours, TradeMath.YourOwnWorth(into, value))" in
                method_body(S['Scoring.cs'], "internal static (int stock, int worth) YoursAdded")
            and "long worth = (long)unitsIn * unitValue;" in worth
            and "kept.StockYours, kept.WorthYours);" in written
            and "how.Landed = TradeMath.WithoutYours(TradeMath.MissedBy(stockThen, stockNow), stockYours);" in weighed
            and "? TradeMath.WithoutYours(TradeMath.MissedBy(worthThen, worthNow), worthYours)" in weighed
            and "int stockYours = 0, int worthYours = 0)" in weighed
            and 'Counted(how.LandingOff) + Yours(kept.StockYours, false) +' in written
            and 'Counted(how.WorthOff) + Shared(how.Share) + Yours(kept.WorthYours, true));' in written
            and all(one in SCORINGTESTS for one in
                    ("What_you_bought_there_yourself_is_not_counted_as_leaving",
                     "What_you_sold_there_yourself_is_not_counted_as_landing",
                     "A_figure_nobody_traded_against_is_weighed_exactly_as_before",
                     "The_log_says_what_it_left_out_of_your_own_trading",
                     "A_trade_in_the_good_itself_counts_its_units_and_its_worth",
                     "A_trade_in_another_good_of_the_kind_counts_only_its_worth",
                     "A_trade_in_a_good_of_another_kind_counts_nothing",
                     "A_market_that_keeps_no_worth_counts_only_the_units_you_moved"))
            and all(one in MATHTESTS for one in
                    ("What_you_put_into_a_market_is_worth_its_units_at_the_good_s_own_value",
                     "Your_own_trades_add_up_without_ever_wrapping_round",
                     "What_moved_leaves_out_what_you_moved_yourself")))


chk("1.90.16", "what you buy or sell in a market yourself is left out of what the forecast is held to there, unit for unit and at each good's own worth, so your own trading never counts against the share what is on its way is counted at",
    your_own_buying_and_selling_is_left_out_of_a_forecast_figure())


def a_promise_your_own_trading_moved_is_set_aside_rather_than_scored():
    h = S['Hindsight.cs']
    promised = method_body(h, "private struct Promised")
    aside = method_body(h, "private static void SetAsideWhatYourTradeMoved")
    priced = method_body(h, "private static Settlement PricedFrom")
    kept = method_body(h, "private static void Kept")
    room = method_body(h, "private static bool RoomForOneMore()")
    traded = method_body(h, "internal static void YouTraded")
    return (promised and aside and priced and kept and room and traded
            and "internal bool YourTradeMovedIt;" in promised
            and "if (site.IsTown) SetAsideWhatYourTradeMoved(site, moved);" in traded
            and "foreach (string where in _promised.Sites())" in aside
            and "if (at == null || PricedFrom(at) != town) continue;" in aside
            and "if (said.Item?.ItemCategory != null && kinds.Contains(said.Item.ItemCategory))" in aside
            and "said.YourTradeMovedIt = true;" in aside
            and ordered(priced, "if (site.IsTown) return site;",
                        "if (village.TradeBound != null) return village.TradeBound;",
                        "if (_nearestTown.TryGetValue(site.StringId, out Settlement near)) return near;",
                        "MobileParty villagers = village.VillagerPartyComponent?.MobileParty;",
                        "SettlementHelper.FindNearestTownToSettlement(",
                        "villagers != null ? villagers.NavigationCapability : MobileParty.NavigationType.All",
                        "_nearestTown[site.StringId] = near;")
            and "_nearestTown.Clear();" in method_body(h, "internal static void Forget")
            and ordered(kept, "if (Scoring.TooOldToSay(said.WithinDays, said.AtHours, now, out float since))",
                        "stale++;", "if (said.YourTradeMovedIt)", "yours++;",
                        "int found = Priced.At(market, said.Item, MobileParty.MainParty, true);")
            and '" set aside because your own trading has moved the price since it was promised"' in kept
            and "one => !one.YourTradeMovedIt && !Scoring.TooOldToSay(" in room
            and all(one in SCORINGTESTS for one in
                    ("Reworking_a_market_changes_only_what_it_holds_for_that_market",
                     "Reworking_a_market_it_holds_nothing_for_changes_nothing",
                     "It_names_every_market_it_holds_something_for")))


chk("1.90.16", "a promise for a market whose price your own trading has since moved, there or in the town a village trades through, is set aside rather than scored, so Trust a market by what it has paid never marks a market down for what you did to it",
    a_promise_your_own_trading_moved_is_set_aside_rather_than_scored())


def every_trade_you_make_at_a_market_is_told_to_the_checks_once():
    t, l = S['Trading.cs'], S['Ledger.cs']
    moved = method_body(t, "internal void Moved(")
    what = method_body(t, "private List<(ItemObject item, int intoTheMarket)> WhatMoved")
    deal = method_body(t, "internal static void TookTheDeal")
    hand = method_body(l, "private void OnPlayerInventoryExchange")
    return (moved and what and deal and hand
            and ordered(moved, "if (Sim) return;", "if (Site == null) return;",
                        "if (!OnTheScreen) Hindsight.YouTraded(Site, WhatMoved(selling));",
                        "LedgerBehavior.Instance?.CaptureSettlement(Site, force: true, KindsMoved());")
            and "moved.Add((kv.Key, selling ? kv.Value.count : -kv.Value.count));" in what
            and ordered(deal, "if (selling == null || buying == null) return;",
                        "selling.OnTheScreen = true;", "buying.OnTheScreen = true;",
                        "Took got = Reckon(selling, sold, true);")
            and ordered(hand, "if (!isTrading || TradeActionBehavior.AutomatedTradeInProgress) return;",
                        "var moved = new List<(ItemObject item, int intoTheMarket)>();",
                        "int bought = Deals.UnitsMoved(element.Amount, said, unit);",
                        "moved.Add((item, -bought));",
                        "int took = Math.Min(bought, InAll(carried, item));",
                        "int gone = Deals.UnitsMoved(element.Amount, said, unit);",
                        "moved.Add((item, gone));",
                        "Hindsight.YouTraded(here, moved);",
                        "CaptureSettlement(Settlement.CurrentSettlement, force: true);")
            and (t + l).count("Hindsight.YouTraded(") == 2)


chk("1.90.16", "every trade you make at a market, by TradeLord or by hand on the trade screen, tells the forecast and promise checks what it moved exactly once, and a deal laid out on the trade screen is told once by the screen rather than again by TradeLord",
    every_trade_you_make_at_a_market_is_told_to_the_checks_once())


def a_forecast_is_not_judged_before_half_its_time_has_passed():
    written = method_body(S['Hindsight.cs'], "private static void Written")
    soon = method_body(S['TradeMath.cs'], "public static bool TooSoonToJudge")
    wait = re.search(r'public const float SoonestAForecastIsJudged = ([\d.]+)f;', S['TradeMath.cs'])
    return (written and soon and wait is not None and float(wait.group(1)) == 0.5
            and ordered(written, "if (Scoring.TooOldToSay(kept.WithinDays, kept.AtHours, now, out float since))",
                        "stale++;", "if (Scoring.TooSoonToSay(kept.WithinDays, since))", "early++;",
                        "scored++;",
                        "LedgerBehavior.Instance?.KeepForecastScore(TradeMath.MissThatCounts(how.Share));")
            and written.count("scored++;") == 1
            and "daysSince < (saidWithinDays > 0f ? saidWithinDays : 0f) * SoonestAForecastIsJudged;" in soon
            and "TradeMath.TooSoonToJudge(withinDays, since);" in S['Scoring.cs']
            and '" passed over as too soon to say anything"' in written
            and "TooSoonToSay" not in method_body(S['Hindsight.cs'], "private static void Kept")
            and all(one in MATHTESTS for one in
                    ("A_forecast_is_judged_only_once_half_its_time_has_passed",
                     "A_forecast_for_no_time_at_all_is_never_too_soon_to_judge",
                     "The_time_a_forecast_must_wait_is_half_of_the_time_it_was_for"))
            and "A_forecast_walked_into_before_half_its_time_is_too_soon_to_say_anything" in SCORINGTESTS)


chk("1.90.16", "a forecast figure walked in on before half of the time it was for is passed over rather than scored, the mirror of one the ride outlived, so a market reached early never counts as the forecast getting it wrong",
    a_forecast_is_not_judged_before_half_its_time_has_passed())


def the_marker_check_weighs_a_sale_against_the_first_figure_for_what_you_carry():
    m = S['Marker.cs']
    remember = method_body(m, "private static void Remember")
    score = method_body(m, "internal static void ScoreTheMark")
    carry = method_body(m, "internal static void ForgetWhatYouCarry")
    carried = method_body(m, "private static List<(EquipmentElement item, int amount, int worth, int floor)> "
                             "WhatYouCarryToSell")
    stands = method_body(S['Rules.cs'], "internal static bool FirstLookStands")
    eaten = method_body(S['Rules.cs'], "internal static bool OnlyEatenFrom")
    return (remember and score and carry and carried and stands and eaten
            and ordered(remember, "_saidValue = how.Value;", "_lastValue = target == null ? 0L : how.Value;",
                        "_lastAt = Freshness.Hour;",
                        "if (Marks.FirstLookStands(_markedId, lookingAt, _markedCargo, _cargoHeld, _markedValue)) return;",
                        "_markedCargo = _cargoHeld;", "_markedId = lookingAt;",
                        "_markedAt = Freshness.Hour;")
            and ordered(carried, "_cargo = cargo;", "_cargoHeld = Held(cargo);", "return cargo;")
            and ordered(carry, "_cargoVersion = -1;", "_cargoHeld = null;")
            and "markedValue > 0L && markedAt != null && markedAt == lookingAt &&" in stands
            and "OnlyEatenFrom(markedCargo, cargoNow);" in stands
            and "if (then == null || now == null) return false;" in eaten
            and ordered(score, "long said = _markedValue;", "long last = _lastValue;",
                        "_markedValue = 0L;", "_lastValue = 0L;",
                        'Log.Write("marker check at "', '" of what it marked on"',
                        '"; its last look, "', '" of that"')
            and all(one in SCORINGTESTS for one in
                    ("The_same_cargo_packed_in_another_order_is_the_same_cargo",
                     "Anything_bought_or_found_since_makes_it_another_cargo",
                     "The_marker_keeps_its_first_figure_while_it_points_at_the_same_town_and_you_have_only_eaten")))


chk("1.90.16", "the marker check weighs a sale against the figure the marker first gave for the town it points at and the cargo you still carry, and names its last look beside it, so it says whether the pick held over the ride rather than reading its own last look back",
    the_marker_check_weighs_a_sale_against_the_first_figure_for_what_you_carry())


def nothing_is_written_down_while_a_deal_is_laid_out_on_the_trade_screen():
    note = method_body(S['Hindsight.cs'], "internal static void Note")
    counter = S['Counter.cs']
    return (note
            and "if (route == null || route.Item == null || Counter.Staging) return;" in note
            and ordered(note, "Counter.Staging) return;", 'Guard.Run("Hindsight.Promise", () => Promise(route));',
                        'Guard.Run("Hindsight.Note", () =>')
            and "internal static bool Staging => _logic != null;" in counter
            and "_logic = logic;" in method_body(counter, "private static bool Opened")
            and "_logic = null;" in method_body(counter, "private static void Drop")
            and "Drop();" in method_body(counter, "internal static TextObject Settle"))


chk("1.90.17", "no forecast figure or promise is written down while TradeLord lays a deal out on the trade screen, because the market it would read already holds goods that may never move, so a laid out deal counts against neither check whether you take it or cancel it",
    nothing_is_written_down_while_a_deal_is_laid_out_on_the_trade_screen())


def the_marker_keeps_its_first_figure_while_your_party_only_eats():
    eaten = method_body(S['Rules.cs'], "internal static bool OnlyEatenFrom")
    held = method_body(S['Marker.cs'], "private static List<(string good, int amount, bool food)> Held")
    return (eaten and held
            and "if (was.food ? one.Value > was.amount : one.Value != was.amount) return false;" in eaten
            and "if (!had.TryGetValue(one.Key, out var was)) return false;" in eaten
            and "if (!one.Value.food && !has.ContainsKey(one.Key)) return false;" in eaten
            and "cargo[i].amount, el.Item.IsFood));" in held
            and all(one in SCORINGTESTS for one in
                    ("Food_your_party_ate_on_the_road_leaves_the_cargo_the_same",
                     "A_good_that_is_not_food_sold_off_on_the_way_makes_it_another_cargo",
                     "The_same_good_split_across_two_lots_is_counted_as_one")))


chk("1.90.17", "the marker check keeps the first figure for the town it points at while your cargo only loses food your party eats, and starts again the moment anything is bought, found or sold off, so a ride of several days is weighed against where it began",
    the_marker_keeps_its_first_figure_while_your_party_only_eats())


def a_check_says_so_when_everything_at_a_market_was_passed_over_or_set_aside():
    h = S['Hindsight.cs']
    kept = method_body(h, "private static void Kept")
    written = method_body(h, "private static void Written")
    yours = method_body(S['Scoring.cs'], "internal static string Yours")
    return (kept and written and yours
            and ordered_last(kept, "if (!Writing || scored + stale + yours + unpriced == 0) return;",
                             'lines.Insert(0, "promise check at "', "if (scored > 0)",
                             '"  here: the price held at "', "Log.WriteMany(lines);")
            and kept.count("if (scored > 0)") == 2
            and ordered(written, "if (!Writing || scored + stale + early == 0) return;",
                        'lines.Insert(0, "forecast check at "', '(scored == 0 ? "" : ":"));',
                        "if (scored > 0)", '"  in all: the landing figure was off by "', "Log.WriteMany(lines);")
            and '" set aside because your own trading has moved the price since it was promised"' in kept
            and '" (not counting "' in yours and '"goods worth "' in yours
            and '" your own trading "' in yours and '"took off"' in yours and '"put on"' in yours
            and '" there yourself"' not in yours
            and "The_log_says_what_it_left_out_of_your_own_trading" in SCORINGTESTS)


chk("1.90.17", "the forecast and promise checks write their line for a market even when every figure there was passed over or set aside, and say goods worth so many denars rather than denars you bought",
    a_check_says_so_when_everything_at_a_market_was_passed_over_or_set_aside())


def staged_trading_sells_no_animal_as_you_leave():
    left = method_body(S['Trading.cs'], "private void OnSettlementLeft")
    held = left.find("if (Options.Current.AutoSellOnEntry && Counter.HoldsBack())")
    sold = left.find("if (Options.Current.AutoSellOnEntry) ExecuteHerdRelief(settlement, quiet: true);")
    return (left and held >= 0 and sold > held
            and "return;" in left[held:sold]
            and '"herd relief on the way out of "' in left[held:sold]
            and ordered(left, "if (!_visitTradeAllowed)",
                        "if (Options.Current.AutoSellOnEntry && Counter.HoldsBack())")
            and "internal static bool HoldsBack() =>" in S['Counter.cs']
            and "TradeRules.StagesTheDeal(Options.Current) &&" in S['Counter.cs'])


chk("1.90.18", "herd relief on the way out waits while Staged Trading holds trading back, the way trading as you arrive already did, so no animal is sold without being laid out on the trade screen first",
    staged_trading_sells_no_animal_as_you_leave())


def a_deal_laid_out_on_the_trade_screen_counts_what_it_moved_once():
    books = S['Books.cs']
    held = method_body(S['Trading.cs'], "private sealed class Pass")
    buy = method_body(S['Passes.cs'], "internal static Traded BuyThem")
    larder = method_body(S['Trading.cs'], "public static void ExecuteResupply")
    return (held and buy and larder
            and "internal bool LaidOut;" in books
            and "private bool OnPaper(bool sim) => sim && !LaidOut;" in books
            and books.count("OnPaper(sim)") == 8
            and "OnPaper(sim) && id != null && _held.TryGetValue(id, out int units) ? units : 0;" in books
            and "OnPaper(sim) && id != null && _dryBought.TryGetValue(id, out var prior) ? prior.count : 0;"
                in books
            and "sim && id != null && _dryDrawn.TryGetValue(id, out int units) ? units : 0;" in books
            and "internal int Purse(bool sim) => sim ? _gained - _spent : 0;" in books
            and ordered(held, "Books = books;", "books.LaidOut = Counter.Staging;",
                        "Sim = Options.Current.SimulationMode || Counter.Staging;")
            and ordered_last(buy, "books.NotePurchase(good.Id, price, good.Weight, TradeRules.FoodValue(good));",
                             "simWeight = books.Weight(sim);", "market.Staged(picked.At, price);")
            and "simWeight +=" not in buy
            and ordered_last(larder, "pass.Books.NotePurchase(item.StringId, price, good.Weight, fed);",
                             "simWeight = pass.Books.Weight(pass.Sim);",
                             "Counter.Stage(el, selling: false, price);")
            and "simWeight +=" not in larder
            and all(one in BOOKTESTS for one in
                    ("ADealLaidOutOnTheTradeScreenLeavesWhatIsCarriedToTheGoodsItMoved",
                     "ADealLaidOutOnTheTradeScreenStillCountsTheGoldAndWhatItTraded",
                     "ADryRunThatIsNotLaidOutCarriesWhatItWouldHaveMoved"))
            and all(one in BUYPASSTESTS for one in
                    ("A_deal_laid_out_on_the_trade_screen_fills_all_the_room_the_screen_has",
                     "A_deal_laid_out_on_the_trade_screen_counts_what_it_laid_out_once"))
            and all(one in SELLPASSTESTS for one in
                    ("A_dry_run_sells_every_quality_of_a_good",
                     "A_deal_laid_out_on_the_trade_screen_sells_every_quality_of_a_good")))


chk("1.90.18", "a deal Staged Trading lays out counts what it has already put on the trade screen once, since the screen moves the goods as they are laid out, while the gold, the caps and what was traded still come from the dry run's own books",
    a_deal_laid_out_on_the_trade_screen_counts_what_it_moved_once())


def an_animal_never_counts_against_the_hold():
    describe = method_body(S['Policy.cs'], "internal static Good Describe")
    relief = method_body(S['Trading.cs'], "public static void ExecuteHerdRelief")
    return (describe and relief
            and "good.Weight = item.HasHorseComponent ? 0f : item.Weight;" in describe
            and describe.count("good.Weight =") == 1
            and "pass.Books.NoteSale(item.StringId, price, 0f, TradePolicy.FoodValue(item));" in relief
            and "item.Weight" not in relief
            and "good.Weight > 0.01f && good.Weight > roomLeft;" in S['Rules.cs'])


chk("1.90.18", "an animal weighs nothing in the hold, the way the game counts it, so livestock is held back by the herd penalty alone and never by the cargo room or the share of the hold",
    an_animal_never_counts_against_the_hold())


def a_purchase_made_away_from_a_market_is_written_down_at_what_it_cost():
    body = method_body(S['Ledger.cs'], "private void OnPlayerInventoryExchange")
    paid = method_body(S['Rules.cs'], "public static int PaidForWhatYouKept")
    return (body and paid
            and ordered(body, "int took = Math.Min(bought, InAll(carried, item));",
                        "here == null", "? Deals.PaidForWhatYouKept(said, bought, took)",
                        ": Bulk.PricePaid(here, element.EquipmentElement, took, unit)")
            and "if (kept >= bought) return gold;" in paid
            and "return (int)((long)gold * kept / bought);" in paid
            and all(one in DEALTESTS for one in
                    ("A_purchase_is_written_down_at_the_gold_it_cost",
                     "Only_the_share_of_a_purchase_you_still_carry_is_written_down",
                     "A_purchase_with_nothing_on_it_writes_nothing_down")))


chk("1.90.18", "a good bought by hand from a caravan on the road is written down at the gold it cost, since there is no shelf to wind back and the good's own worth is not what you paid",
    a_purchase_made_away_from_a_market_is_written_down_at_what_it_cost())


def a_caravan_is_priced_where_the_game_prices_it():
    t = S['Trading.cs']
    priced = method_body(t, "private static IMarketData PricedOnTheRoad")
    road = method_body(t, "public static void ExecuteRoadTrade")
    return (priced and road
            and "IMarketData road = PricedOnTheRoad(met);" in road
            and "if (met == null || !met.IsCaravan) return RoadMarket();" in priced
            and ordered(priced, "me.CurrentSettlement ??",
                        "SettlementHelper.FindNearestTownToMobileParty(me, MobileParty.NavigationType.All)?.Settlement;",
                        "IMarketData kept = Priced.Kept(near);",
                        "if (kept == null) return RoadMarket();", "return kept;")
            and "using Helpers;" in t
            and "Road.GetPrice(what, Party, selling, Shop)" in between(t, "internal int Price(", ";")
            and "new Pass(null, met, road, books, party, quiet: true);" in t)


chk("1.90.19", "a caravan met on the road is priced from the market the game's own trade screen prices a caravan from, the town you are in or else the nearest town, with the caravan as the merchant",
    a_caravan_is_priced_where_the_game_prices_it())


def a_party_of_villagers_is_never_sold_to_and_its_offer_is_taken_whole_or_left():
    t = S['Trading.cs']
    road = method_body(t, "public static void ExecuteRoadTrade")
    lot = method_body(t, "private static void LotPass")
    offer = method_body(t, "private static IMarketData TheirOfferFrom")
    priced = method_body(t, "private static IMarketData PricedOnTheRoad")
    judged = method_body(S['Passes.cs'], "internal static Block WhatStopsTheLot")
    taken = method_body(S['Passes.cs'], "internal static Traded TakeTheLot")
    return (road and lot and offer and priced and judged and taken
            and ordered(road, "if (met.IsVillager)", "LotPass(Pass.Meet(met, road, books, party), why);",
                        "else", "SellPass(Pass.Meet(met, road, books, party),",
                        "BuyPass(Pass.Meet(met, road, books, party),", "ReportStalledPasses();")
            and t.count("LotPass(") == 2
            and ordered(priced, "if (met != null && met.IsVillager) return TheirOfferFrom(met);",
                        "if (met == null || !met.IsCaravan) return RoadMarket();")
            and ordered_last(offer, "Village home = villagers.HomeSettlement?.Village;",
                             "if (Meetings.TheirOfferIsGuarded()) return new TheirOffer(home);", "return null;")
            and "_home.GetItemPrice(itemRosterElement, tradingParty, isSelling: true);" in S['Market.cs']
            and "_home.GetItemPrice(item, tradingParty, isSelling: true);" in S['Market.cs']
            and "!_theirOffer || Item(at)?.ItemCategory != DefaultItemCategories.PackAnimal;" in t
            and "public int AmountAt(int at) => InTheOffer(at) ? Shelf[at].Amount : 0;" in t
            and "public int TheirsToSell(int at) => InTheOffer(at) ? _pass.TheirsToSell(Shelf[at]) : 0;" in t
            and "theirOffer: true" in lot
            and ordered(lot, "TradePass.WhatStopsTheLot(market, pass.Books, pass.Sim, pass.ShareCap,",
                        "if (lot.Units == 0) return;", "if (stops == Block.None)",
                        "InAPass(() => moved = TradePass.TakeTheLot(market, pass.Books, pass.Sim));",
                        "else tally.Note(stops);", "if (!pass.Sim) Meetings.TookTheirOffer(pass.Met);",
                        "pass.Moved(gold: spent, selling: false);",
                        "if (!pass.Muted) Notices.Say(msg, Notices.Spend);",
                        "else if (!pass.DirectionError)",
                        "if (stopped != Block.None && !pass.Muted) NoteStalled(selling: false, stopped);")
            and ordered(judged, "if (!market.MayBuy(at, good, out Block whyBuy)) refused = whyBuy;",
                        "else if (!TradeRules.ResaleAllowed(good, s)) refused = Block.CategoryPolicy;",
                        "if (lot.Units == 0) return Block.NoStock;",
                        "if (refused != Block.None) return refused;",
                        "TradeRules.WhatCapsALot(good,", "if (capped != Block.None) return capped;",
                        "TradeRules.WhatTheBuyerPays(", "s.ResaleSafetyFactor);",
                        "if (!TradeMath.BuyAcceptable(lot.Price, lot.Resale, s.MinProfitMargin)) return Block.BelowMargin;",
                        "if (lot.Price > market.Spendable()) return Block.BudgetSpent;",
                        "return Block.HerdFull;", "return Block.CarryWeight;", "return Block.None;")
            and ordered(taken, "if (market.Stopped) return moved;",
                        "books.NotePurchase(good.Id, price, good.Weight, TradeRules.FoodValue(good));",
                        "market.Staged(at, price);",
                        "if (!market.Take(at, price, out int cost) || cost == 0) return moved;",
                        "books.NoteBought(good.Id, cost);")
            and all(name in LOTTESTS for name in (
                "An_offer_that_sells_on_for_more_than_it_costs_is_taken_whole",
                "One_good_that_loses_money_is_carried_by_the_rest_of_the_offer",
                "An_offer_that_misses_your_margin_as_a_whole_is_left_for_you",
                "One_good_you_never_buy_keeps_the_whole_offer_off",
                "What_the_villagers_keep_out_of_their_offer_is_left_out_of_it_here_too",
                "A_dry_run_writes_the_offer_into_its_own_books_and_takes_nothing",
                "A_trade_the_game_turned_round_stops_the_offer_where_it_stood")))


def the_offer_is_gone_once_tradelord_took_it_and_moves_nothing_if_asked_for_again():
    enc = S['Encounters.cs']
    shown = method_body(enc, "internal static class Patch_VillagerOfferShown")
    taken = method_body(enc, "internal static class Patch_VillagerOfferTaken")
    meetings = method_body(enc, "internal static class Meetings")
    return (shown and taken and meetings
            and '[HarmonyPatch(typeof(VillagerCampaignBehavior), "village_farmer_buy_products_on_condition")]' in enc
            and '[HarmonyPatch(typeof(VillagerCampaignBehavior), '
                '"conversation_player_decided_to_buy_on_consequence")]' in enc
            and "if (__result && Meetings.TheirOfferIsTaken(PlayerEncounter.EncounteredMobileParty)) __result = false;"
                in shown
            and ordered(taken, "if (Meetings.TheirOfferIsTaken(met))",
                        "if (PlayerEncounter.Current != null) PlayerEncounter.LeaveEncounter = true;",
                        "return false;", "Meetings.WhatTheyOffer(met)", "return true;",
                        "Meetings.YouTookTheirOffer(__state.offered, paid)")
            and ordered(method_body(enc, "internal static void TookTheirOffer"),
                        "_offerTakenFrom = met;", "_offerTakenIn = PlayerEncounter.Current;")
            and "_offerTakenIn != null && _offerTakenIn == PlayerEncounter.Current" in meetings
            and all(one in method_body(enc, "internal static void ForgetWhoYouTradedWith")
                    for one in ("_offerTakenFrom = null;", "_offerTakenIn = null;"))
            and "Patcher.Holds(nameof(Patch_VillagerOfferShown)) && Patcher.Holds(nameof(Patch_VillagerOfferTaken))"
                in meetings
            and "item.ItemCategory == DefaultItemCategories.PackAnimal" in meetings
            and "priced.GetPrice(el.EquipmentElement, MobileParty.MainParty, isSelling: true, met.Party)" in meetings
            and ordered_last(method_body(enc, "internal static void YouTookTheirOffer"),
                             "if (!Deals.AddsUp(asked, paid))", "return;",
                             "LedgerBehavior.Instance?.RecordPurchase(line.id, line.units, "
                             "TradeMath.WorthOf(line.units, line.price));")
            and "Patcher.TryPatch(harmony, typeof(Patch_VillagerOfferShown));" in S['SubModule.cs']
            and "Patcher.TryPatch(harmony, typeof(Patch_VillagerOfferTaken));" in S['SubModule.cs'])


def the_hint_and_the_feature_list_say_how_villagers_are_traded_with():
    said = ("Villagers are never sold to: TradeLord takes their whole offer when it clears your margin, "
            "and otherwise leaves it to you.")
    return (said in spoken(ENGLISH)['TL373'] and said in M
            and "Never sells to a party of villagers, and takes their whole offer" in README
            and "so the same goods can never be bought twice" in README
            and "An offer TradeLord leaves is still yours to take in the conversation" in README
            and all(said not in spoken(path)['TL373'] for path in TRANSLATIONS.values()))


chk("1.91.0", "a party of villagers on the road is never sold to, and its whole offer is bought at the game's own price when the lot as a whole clears your margin and every rule a purchase answers to, or else left to you",
    a_party_of_villagers_is_never_sold_to_and_its_offer_is_taken_whole_or_left())
chk("1.91.0", "once TradeLord has bought the villagers' offer the game's own offer is gone for that meeting and moves nothing if it is reached anyway, while an offer you take yourself is written down at what you paid",
    the_offer_is_gone_once_tradelord_took_it_and_moves_nothing_if_asked_for_again())
chk("1.91.0", "the hint under Trade with caravans and villagers and the feature list say how TradeLord trades with villagers",
    the_hint_and_the_feature_list_say_how_villagers_are_traded_with())


def every_best_market_comparison_prices_the_quality_you_carry():
    scale = method_body(S['TradeMath.cs'], "public static int AtThisQuality")
    resale = method_body(S['Trading.cs'], "public bool ResaleMarket(int at, out int price)")
    floor = method_body(S['Marker.cs'], "private static int BestMarketFloor")
    step = method_body(S['Marker.cs'], "private int Next()")
    colour = method_body(S['TooltipPatches.cs'], "private static void Coloured")
    return ("long priced = (long)plainPrice * qualityValue / plainValue;" in scale
            and "price = TradeMath.AtThisQuality(best.Item2, held.Item.Value, held.ItemValue);" in resale
            and "TradeMath.AtThisQuality(best.Item2, held.Item.Value, held.ItemValue)," in floor
            and "new Ladder(_site, _el, true, _flat, 0);" in step
            and colour.count("TradeMath.AtThisQuality(best.price, item.Value, held.ItemValue)") == 2
            and all(one in FLOORTESTS for one in
                    ("The_best_price_is_scaled_to_the_quality_you_carry",
                     "A_worn_good_sells_wherever_a_plain_one_would",
                     "A_price_with_nothing_to_scale_it_by_is_left_as_it_is",
                     "A_scaled_price_stays_between_one_denar_and_what_an_int_holds")))


chk("1.91.1", "Hold cargo for the best market, the map marker and Color prices by world market hold a good of any quality to what the best market pays for that same quality",
    every_best_market_comparison_prices_the_quality_you_carry())

print(f"\n{sum(results)}/{len(results)} source checks passed")
sys.exit(0 if all(results) else 1)
