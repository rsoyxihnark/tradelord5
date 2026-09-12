using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MCM.Abstractions;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base;
using MCM.Abstractions.Base.Global;
using MCM.Common;
using TaleWorlds.Library;

namespace TradeLord.Mcm
{
    public static class McmSettingsBootstrap
    {
        public static bool Init()
        {
            Guard.Run("Mcm.ScreenTongue", ScreenTongue.Follow);
            McmLoader.Reseat = Settings.Reseat;
            McmLoader.PutBackWhatItShipsWith = Settings.Reset;
            if (Settings.Instance == null) return false;
            Settings.Reseat();
            return true;
        }
    }

    internal static class ScreenTongue
    {
        private const int Crowd = 256;

        private static AccessTools.FieldRef<SettingsPropertyDefinition, string> _name, _hint, _group, _content;
        private static AccessTools.FieldRef<SettingsPropertyGroupDefinition, string> _heading;

        private sealed class Held
        {
            internal WeakReference At;
            internal string[] Words;
        }

        private sealed class Headed
        {
            internal WeakReference At;
            internal string Word;
        }

        private static readonly List<Held> _held = new List<Held>();
        private static readonly List<Headed> _headed = new List<Headed>();
        private static readonly Dictionary<string, string> _rawFor = new Dictionary<string, string>(StringComparer.Ordinal);

        private static bool _following;

        internal static void Follow()
        {
            if (_following) return;
            _following = true;
            _name = Reaches("<DisplayName>k__BackingField");
            _hint = Reaches("<HintText>k__BackingField");
            _group = Reaches("<GroupName>k__BackingField");
            _content = Reaches("<Content>k__BackingField");
            _heading = Grouped("_groupNameRaw");
            ConstructorInfo built = AccessTools.Constructor(typeof(SettingsPropertyDefinition), new[]
            {
                typeof(IEnumerable<IPropertyDefinitionBase>), typeof(IPropertyGroupDefinition),
                typeof(IRef), typeof(char)
            });
            ConstructorInfo gathered = AccessTools.Constructor(typeof(SettingsPropertyGroupDefinition),
                new[] { typeof(string), typeof(int) });
            if (_name == null || _hint == null || _group == null || _content == null || _heading == null ||
                built == null || gathered == null)
            {
                Log.Write("the settings screen could not be wired to TradeLord's own language - it follows the game's language instead");
                return;
            }
            var harmony = new Harmony(SubModule.HarmonyId + ".mcm");
            harmony.Patch(built, postfix: new HarmonyMethod(typeof(ScreenTongue), nameof(Spoken)));
            harmony.Patch(gathered, postfix: new HarmonyMethod(typeof(ScreenTongue), nameof(Headline)));
        }

        private static AccessTools.FieldRef<SettingsPropertyDefinition, string> Reaches(string field)
        {
            FieldInfo found = AccessTools.Field(typeof(SettingsPropertyDefinition), field);
            return found == null ? null : AccessTools.FieldRefAccess<SettingsPropertyDefinition, string>(found);
        }

        private static AccessTools.FieldRef<SettingsPropertyGroupDefinition, string> Grouped(string field)
        {
            FieldInfo found = AccessTools.Field(typeof(SettingsPropertyGroupDefinition), field);
            return found == null ? null : AccessTools.FieldRefAccess<SettingsPropertyGroupDefinition, string>(found);
        }

        private static void Spoken(SettingsPropertyDefinition __instance)
        {
            var words = new[] { _name(__instance), _hint(__instance), _group(__instance), _content(__instance) };
            if (!Tongue.Mine(words[0]) && !Tongue.Mine(words[1]) &&
                !Tongue.Mine(words[2]) && !Tongue.Mine(words[3])) return;
            lock (_held)
            {
                Forget();
                _held.Add(new Held { At = new WeakReference(__instance), Words = words });
                Say(__instance, words);
            }
        }

        private static void Headline(SettingsPropertyGroupDefinition __instance)
        {
            lock (_held)
            {
                string shown = _heading(__instance);
                if (shown == null || !_rawFor.TryGetValue(shown, out string word)) return;
                Forget();
                _headed.Add(new Headed { At = new WeakReference(__instance), Word = word });
            }
        }

        internal static void Respeak()
        {
            if (_name == null || _heading == null) return;
            lock (_held)
            {
                for (int i = _held.Count - 1; i >= 0; i--)
                {
                    if (_held[i].At.Target is SettingsPropertyDefinition one) Say(one, _held[i].Words);
                    else _held.RemoveAt(i);
                }
                for (int i = _headed.Count - 1; i >= 0; i--)
                {
                    if (_headed[i].At.Target is SettingsPropertyGroupDefinition one) _heading(one) = Said(_headed[i].Word);
                    else _headed.RemoveAt(i);
                }
            }
        }

        private static void Say(SettingsPropertyDefinition of, string[] words)
        {
            _name(of) = Said(words[0]);
            _hint(of) = Said(words[1]);
            _content(of) = Said(words[3]);
            string shown = Said(words[2]);
            _group(of) = shown;
            _rawFor[shown] = words[2];
        }

        private static string Said(string word) => Tongue.Said(word) ?? word;

        private static void Forget()
        {
            if (_held.Count < Crowd && _headed.Count < Crowd) return;
            _held.RemoveAll(one => !one.At.IsAlive);
            _headed.RemoveAll(one => !one.At.IsAlive);
        }
    }

    public class Settings : AttributeGlobalSettings<Settings>
    {
        public override string Id => "TradeLord";

        public override string DisplayName => ("TradeLord " + ModVersion()).TrimEnd();
        public override string FolderName => "TradeLord";
        public override string FormatType => "json2";

        private static string ModVersion()
        {
            try { return TaleWorlds.ModuleManager.ModuleHelper.GetModuleInfo("TradeLord").Version.ToString(); }
            catch { return ""; }
        }

        private static readonly string[] LanguageWords =
        {
            "English", "T\u00FCrk\u00E7e", "\u0420\u0443\u0441\u0441\u043A\u0438\u0439", "\u7B80\u4F53\u4E2D\u6587"
        };

        private Options _o;
        private Dropdown<string> _language;

        private static readonly string[] PolicyWords =
        {
            "{=TL253}Leave alone", "{=TL254}Sell only", "{=TL255}Buy only", "{=TL256}Buy and sell"
        };

        private static readonly string[] SmeltableWords =
        {
            "{=TL270}Sell them", "{=TL271}Keep every one", "{=TL272}Keep the ones you have not learned"
        };

        private static readonly string[] BasisWords =
        {
            "{=TL257}Average of what you paid", "{=TL258}Last price you paid", "{=TL259}Cheapest market you know"
        };

        private Dropdown<string> _foodPolicy;
        private Dropdown<string> _craftingPolicy;
        private Dropdown<string> _livestockPolicy;
        private Dropdown<string> _costBasis;
        private Dropdown<string> _smeltable;

        public Settings() { Bound(Options.Current); }

        private void Bound(Options to)
        {
            _o = to;
            _language = new Dropdown<string>(LanguageWords, to.Language);
            _foodPolicy = Choice(PolicyWords, to.FoodPolicy);
            _craftingPolicy = Choice(PolicyWords, to.CraftingPolicy);
            _livestockPolicy = Choice(PolicyWords, to.LivestockPolicy);
            _costBasis = Choice(BasisWords, to.CostBasisMode);
            _smeltable = Choice(SmeltableWords, to.KeepSmeltableWeapons);
        }

        public override BaseSettings CreateNew()
        {
            var made = new Settings();
            made.Bound(new Options());
            return made;
        }

        private static Dropdown<string> Choice(string[] words, int picked) =>
            new Dropdown<string>(Spoken(words), picked);

        private static string[] Spoken(string[] words)
        {
            var said = new string[words.Length];
            for (int i = 0; i < words.Length; i++) said[i] = Tongue.Plain(words[i]);
            Guard.Run("Mcm.Choice", () =>
            {
                for (int i = 0; i < words.Length; i++) said[i] = Tongue.Text(words[i]).ToString();
            });
            return said;
        }

        [SettingPropertyButton("{=TL276}Put every setting back to how TradeLord ships", Order = 0, RequireRestart = false,
            Content = "{=TL277}Reset",
            HintText = "{=TL376}Puts every TradeLord setting back to the value it ships with, in one go. It takes hold at once and is written to TradeLord.ini as well, so nothing is left half changed. Your never sell, always sell, never buy and always buy lists are emptied too, and what changed is written to TradeLord.log.")]
        [SettingPropertyGroup("{=TL100}Language", GroupOrder = 0)]
        public Action ResetEverything { get; set; } = Reset;

        [SettingPropertyDropdown("{=TL250}Language", Order = 1, RequireRestart = false,
            HintText = "{=TL350}The language TradeLord speaks in the game: its trade messages, the ledger panel, the price tooltips and its town menu entries. English by default. It takes hold as you pick it, with no restart and no reload.")]
        [SettingPropertyGroup("{=TL100}Language", GroupOrder = 0)]
        public Dropdown<string> Language
        {
            get => _language;
            set
            {
                _language = value;
                Follows(value, () => _o.Language, picked => _o.Language = picked);
                if (value != null) value.PropertyChanged += (sender, args) => Retell();
                Options.Bump();
                Retell();
            }
        }

        internal static void Reset()
        {
            Guard.Run("Mcm.Reset", () =>
            {
                var stock = new Options();
                var moved = new List<string>();
                foreach (FieldInfo field in typeof(Options).GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    object now = field.GetValue(stock);
                    if (!Equals(field.GetValue(Options.Current), now)) moved.Add(field.Name);
                    field.SetValue(Options.Current, now);
                }
                Log.Write("settings screen: every setting was put back to the value TradeLord ships with, " +
                          moved.Count + " of them had been changed");
                Reseat();
                Options.Bump();
                Settings shown = Instance;
                if (shown == null) return;
                for (int i = 0; i < moved.Count; i++) shown.OnPropertyChanged(moved[i]);
            });
        }

        internal static void Reseat()
        {
            Guard.Run("Mcm.Reseat", () =>
            {
                Settings held = Instance;
                if (held == null) return;
                held.Bound(Options.Current);
                held.FollowLanguage();
                BaseSettingsProvider.Instance?.SaveSettings(held);
            });
        }

        internal void FollowLanguage()
        {
            Follows(Language, () => _o.Language, picked => _o.Language = picked);
            Follows(FoodPolicy, () => _o.FoodPolicy, picked => _o.FoodPolicy = picked);
            Follows(CraftingPolicy, () => _o.CraftingPolicy, picked => _o.CraftingPolicy = picked);
            Follows(LivestockPolicy, () => _o.LivestockPolicy, picked => _o.LivestockPolicy = picked);
            Follows(CostBasisMode, () => _o.CostBasisMode, picked => _o.CostBasisMode = picked);
            Follows(KeepSmeltableWeapons, () => _o.KeepSmeltableWeapons, picked => _o.KeepSmeltableWeapons = picked);
            Language.PropertyChanged += (sender, args) => Retell();
            if (!_watchingForSave)
            {
                _watchingForSave = true;
                PropertyChanged += (sender, args) =>
                {
                    if (args?.PropertyName == SaveTriggered) Guard.Run("Mcm.Saved", TakeEveryChoice);
                };
            }
            Retell();
        }

        private bool _watchingForSave;

        private void TakeEveryChoice()
        {
            Taken(Language, picked => _o.Language = picked);
            Taken(FoodPolicy, picked => _o.FoodPolicy = picked);
            Taken(CraftingPolicy, picked => _o.CraftingPolicy = picked);
            Taken(LivestockPolicy, picked => _o.LivestockPolicy = picked);
            Taken(CostBasisMode, picked => _o.CostBasisMode = picked);
            Taken(KeepSmeltableWeapons, picked => _o.KeepSmeltableWeapons = picked);
            Options.Bump();
            Retell();
        }

        private static void Taken(Dropdown<string> from, Action<int> keep)
        {
            if (from != null) keep(from.SelectedIndex);
        }

        private static void Follows(Dropdown<string> from, Func<int> held, Action<int> keep)
        {
            if (from == null) return;
            Taken(from, keep);
            from.PropertyChanged += (sender, args) =>
            {
                if (held() == from.SelectedIndex) return;
                keep(from.SelectedIndex);
                Options.Bump();
            };
        }

        private void Retell()
        {
            Retold(FoodPolicy, PolicyWords);
            Retold(CraftingPolicy, PolicyWords);
            Retold(LivestockPolicy, PolicyWords);
            Retold(CostBasisMode, BasisWords);
            Retold(KeepSmeltableWeapons, SmeltableWords);
            Relabel();
        }

        private int _spokenFor = -1;

        private void Relabel()
        {
            if (_spokenFor == _o.Language) return;
            bool first = _spokenFor < 0;
            _spokenFor = _o.Language;
            if (first) return;
            Guard.Run("Mcm.Relabel", () =>
            {
                ScreenTongue.Respeak();
                Redrawn();
                Log.Write("settings screen: the language changed, so every name, hint and heading on it was spoken again");
            });
        }

        private static readonly FieldInfo Listening = AccessTools.Field(typeof(BaseSettings), "PropertyChanged");

        private void Redrawn()
        {
            if (!(Listening?.GetValue(this) is MulticastDelegate told)) return;
            var drawn = new List<ViewModel>();
            foreach (Delegate one in told.GetInvocationList())
            {
                ViewModel shown = Shown(one.Target);
                if (shown != null && !drawn.Exists(had => ReferenceEquals(had, shown))) drawn.Add(shown);
            }
            foreach (ViewModel shown in drawn) shown.RefreshValues();
        }

        private static ViewModel Shown(object listener)
        {
            if (!(listener is ViewModel shown)) return null;
            return AccessTools.Property(shown.GetType(), "Group")?.GetValue(shown) as ViewModel ?? shown;
        }

        private static void Retold(Dropdown<string> shown, string[] words)
        {
            shown.Clear();
            shown.AddRange(Spoken(words));
        }

        [SettingPropertyBool("{=TL201}Live world prices (default)", Order = 0, RequireRestart = false,
            HintText = "{=TL301}ON (default): prices are read live from the world economy, including markets you have not visited. OFF: only prices you have seen in person are used.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 3)]
        public bool Omniscient { get => _o.Omniscient; set { _o.Omniscient = value; Options.Bump(); } }


        [SettingPropertyInteger("{=TL205}Minimum stock for buy suggestions", 0, 100, Order = 3, RequireRestart = false,
            HintText = "{=TL305}Best-buy hints require at least this many units in stock. 0 = off. Live-price mode only, because observed mode records prices, not stock levels.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 3)]
        public int MinTownStock { get => _o.MinTownStock; set { _o.MinTownStock = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL208}Conservative route projection", Order = 6, RequireRestart = false,
            HintText = "{=TL308}Apply the resale safety factor to the sell side when ranking and totalling routes, so listed profit allows for prices drifting before you arrive. OFF shows raw margins. Routes must clear the safety factor to be listed either way, since that is the same test a buying pass applies on arrival.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 3)]
        public bool ConservativeRouteProjection { get => _o.ConservativeRouteProjection; set { _o.ConservativeRouteProjection = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL209}Bulk price simulation", Order = 7, RequireRestart = false,
            HintText = "{=TL309}Price a lot unit by unit through the game's own price model, so quantity and profit account for your own buying moving the price. OFF prices every unit at the first unit's price. Towns and live-price mode only.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 3)]
        public bool BulkSimulation { get => _o.BulkSimulation; set { _o.BulkSimulation = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL210}Rank routes by confidence", Order = 9, RequireRestart = false,
            HintText = "{=TL310}Order the panel by profit per day, discounted by how likely that profit is to survive the trip: margin, stock depth, trip length, caravan traffic and price age. OFF ranks on raw profit per day.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 3)]
        public bool ConfidenceRanking { get => _o.ConfidenceRanking; set { _o.ConfidenceRanking = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL279}Count what is on its way to a market", Order = 10, RequireRestart = false,
            HintText = "{=TL396}Before a route or a price in a tooltip is worked out, count what reaches that market before you do: the cargo the caravans will unload, what the workshops will make next, and the purses those caravans bring, spread over the goods that are cheap there as an estimate of what they will buy. Needs Live world prices. OFF prices each market as it stands.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 3)]
        public bool MarketForecast { get => _o.MarketForecast; set { _o.MarketForecast = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL211}Show best buy/sell in tooltips", Order = 0, RequireRestart = false,
            HintText = "{=TL311}Adds the best known buy and sell markets, with stock and travel time, to item tooltips.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 4)]
        public bool TooltipHints { get => _o.TooltipHints; set { _o.TooltipHints = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL212}Suppress vanilla trade-rumor lines", Order = 1, RequireRestart = false,
            HintText = "{=TL312}Skips the vanilla merchandise rumor block so the tooltip shows one consistent set of price hints.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 4)]
        public bool SuppressVanillaTradeLines { get => _o.SuppressVanillaTradeLines; set { _o.SuppressVanillaTradeLines = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL213}Color prices by world market", Order = 2, RequireRestart = false,
            HintText = "{=TL313}Colors trade-good and livestock rows in the inventory by how this market's price compares with the best known market.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 4)]
        public bool ProfitColoring { get => _o.ProfitColoring; set { _o.ProfitColoring = value; Options.Bump(); } }

        [SettingPropertyText("{=TL214}Ledger panel hotkey (map screen)", Order = 3, RequireRestart = false,
            HintText = "{=TL314}Key that opens the ledger panel on the campaign map. One key name such as T, Y or F5, optionally with Ctrl, Alt or Shift in front, such as Ctrl+T. A bare key the game also uses triggers both actions. Anything unknown falls back to T.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 4)]
        public string PanelKey { get => _o.PanelKey; set { _o.PanelKey = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL215}TradeLord button on the map screen", Order = 4, RequireRestart = false,
            HintText = "{=TL315}A clickable TradeLord button on the right edge of the campaign map that opens the ledger panel. Turn OFF if it interferes with map clicks.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 4)]
        public bool ShowMapButton { get => _o.ShowMapButton; set { _o.ShowMapButton = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL217}Auto sell", Order = 0, RequireRestart = false,
            HintText = "{=TL317}Sells whatever your rules allow the moment you walk into a market, without being asked. Trade XP is awarded. With this off, TradeLord sells only when you pick its trade entry in the menu.")]
        [SettingPropertyGroup("{=TL104}Automation", GroupOrder = 1)]
        public bool AutoSellOnEntry { get => _o.AutoSellOnEntry; set { _o.AutoSellOnEntry = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL218}Auto buy", Order = 1, RequireRestart = false,
            HintText = "{=TL318}Buys the moment you walk into a market, after any selling. With this off, TradeLord buys only when you pick its trade entry in the menu.")]
        [SettingPropertyGroup("{=TL104}Automation", GroupOrder = 1)]
        public bool AutoBuyOnEntry { get => _o.AutoBuyOnEntry; set { _o.AutoBuyOnEntry = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL281}Lay the trade out for you first", Order = 2, RequireRestart = false,
            HintText = "{=TL400}Rather than trading for you, TradeLord opens the trade screen and lays its whole deal on it: all it would sell on one side, all it would buy on the other. Change what you like, then press Done to trade or Cancel to leave it. Nothing is traded as you arrive while this is on, and a party met on the road still trades as before. OFF by default.")]
        [SettingPropertyGroup("{=TL104}Automation", GroupOrder = 1)]
        public bool StagedTrading { get => _o.StagedTrading; set { _o.StagedTrading = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL216}Trade entry in town menu", Order = 0, RequireRestart = false,
            HintText = "{=TL316}Shows the single TradeLord trade entry in town and village menus, which sells and then buys in one go. Turn it off to leave the menu to trading done automatically as you arrive.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public bool QuickSellMenu { get => _o.QuickSellMenu; set { _o.QuickSellMenu = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL260}Ledger entry in town menu", Order = 1, RequireRestart = false,
            HintText = "{=TL360}Shows the TradeLord ledger entry in town and village menus, which opens the route panel. Turn it off if you would rather open the panel with its hotkey or its map button.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public bool LedgerMenuEntry { get => _o.LedgerMenuEntry; set { _o.LedgerMenuEntry = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL278}Trade with towns", Order = 0, RequireRestart = false,
            HintText = "{=TL390}TradeLord trades in town menus and towns join the price scans. ON by default.")]
        [SettingPropertyGroup("{=TL108}Trade Pool", GroupOrder = 2)]
        public bool TradeWithTowns { get => _o.TradeWithTowns; set { _o.TradeWithTowns = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL241}Trade with villages", Order = 1, RequireRestart = false,
            HintText = "{=TL341}TradeLord trades in village menus and villages join the price scans.")]
        [SettingPropertyGroup("{=TL108}Trade Pool", GroupOrder = 2)]
        public bool TradeWithVillages { get => _o.TradeWithVillages; set { _o.TradeWithVillages = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL273}Trade with caravans and villagers", Order = 2, RequireRestart = false,
            HintText = "{=TL373}Meet a caravan or a party of villagers on the road and TradeLord trades with them the moment you meet, selling what clears your margin and buying what it can sell on for more. They pay from their own purse. ON by default.")]
        [SettingPropertyGroup("{=TL108}Trade Pool", GroupOrder = 2)]
        public bool TradeWithCaravans { get => _o.TradeWithCaravans; set { _o.TradeWithCaravans = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL206}Town travel ceiling (days, 0 = off)", 0f, 20f, "0.0", Order = 3, RequireRestart = false,
            HintText = "{=TL306}How far TradeLord looks for a town, in travel days. Towns farther than this are hidden from tooltips, held out of the routes and kept off the map marker, and no suggested route may exceed it. Default 2.4.")]
        [SettingPropertyGroup("{=TL108}Trade Pool", GroupOrder = 2)]
        public float MaxTravelDaysTown { get => _o.MaxTravelDaysTown; set { _o.MaxTravelDaysTown = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL207}Village travel ceiling (days, 0 = off)", 0f, 10f, "0.0", Order = 4, RequireRestart = false,
            HintText = "{=TL307}The same limit for villages, kept separate so you can hold them closer than towns. Default 1.")]
        [SettingPropertyGroup("{=TL108}Trade Pool", GroupOrder = 2)]
        public float MaxTravelDaysVillage { get => _o.MaxTravelDaysVillage; set { _o.MaxTravelDaysVillage = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL203}Exclude hostile markets", Order = 5, RequireRestart = false,
            HintText = "{=TL303}Never scan, suggest or auto-trade with settlements at war with you.")]
        [SettingPropertyGroup("{=TL108}Trade Pool", GroupOrder = 2)]
        public bool ExcludeHostileTowns { get => _o.ExcludeHostileTowns; set { _o.ExcludeHostileTowns = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL220}Minimum profit margin", 0f, 2f, "#0%", Order = 4, RequireRestart = false,
            HintText = "{=TL320}The margin every trade must clear, in both directions. Sell only if the price exceeds your cost basis by at least this much. Buy, or list a route, only if the far market exceeds the local price by this much after the resale safety factor. Raising it trades less.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public float MinProfitMargin { get => _o.MinProfitMargin; set { _o.MinProfitMargin = value; Options.Bump(); } }

        [SettingPropertyDropdown("{=TL222}Food policy", Order = 5, RequireRestart = false,
            HintText = "{=TL322}What automated trading may do with food. The days-of-supply food reserve is separate and still applies.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public Dropdown<string> FoodPolicy
        {
            get => _foodPolicy;
            set { _foodPolicy = value; Follows(value, () => _o.FoodPolicy, picked => _o.FoodPolicy = picked); Options.Bump(); }
        }

        [SettingPropertyDropdown("{=TL223}Smithing material policy", Order = 6, RequireRestart = false,
            HintText = "{=TL323}What automated trading may do with charcoal, hardwood, iron ore and ingots. Pick Leave alone to keep smithing stock out of automated trading entirely.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public Dropdown<string> CraftingPolicy
        {
            get => _craftingPolicy;
            set { _craftingPolicy = value; Follows(value, () => _o.CraftingPolicy, picked => _o.CraftingPolicy = picked); Options.Bump(); }
        }

        [SettingPropertyDropdown("{=TL224}Livestock policy", Order = 7, RequireRestart = false,
            HintText = "{=TL324}What automated trading may do with sheep, cattle and hogs. Buying is capped by the game's own herding calculation, so it will not push the party into the herd speed penalty. Haul animals and riding mounts have their own settings and this one does not affect them.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public Dropdown<string> LivestockPolicy
        {
            get => _livestockPolicy;
            set { _livestockPolicy = value; Follows(value, () => _o.LivestockPolicy, picked => _o.LivestockPolicy = picked); Options.Bump(); }
        }

        [SettingPropertyBool("{=TL226}Respect inventory locks", Order = 8, RequireRestart = false,
            HintText = "{=TL326}Locked items in the inventory screen are never auto-traded. Selling matches a lock the way the game stores it, by item and quality. Buying matches by item alone, so a lock on a good stops TradeLord buying more of it whatever its quality.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public bool RespectLocks { get => _o.RespectLocks; set { _o.RespectLocks = value; Options.Bump(); } }

        [SettingPropertyDropdown("{=TL227}What a good counts as having cost you", Order = 9, RequireRestart = false,
            HintText = "{=TL327}The price a sale is measured against, so it sets both the profit TradeLord reports and the Trade XP the sale earns. Anything you never bought, loot included, is valued at the cheapest market you know of whichever one you pick.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public Dropdown<string> CostBasisMode
        {
            get => _costBasis;
            set { _costBasis = value; Follows(value, () => _o.CostBasisMode, picked => _o.CostBasisMode = picked); Options.Bump(); }
        }

        [SettingPropertyBool("{=TL242}Simulation mode (dry run)", Order = 10, RequireRestart = false,
            HintText = "{=TL342}Reports what TradeLord would sell and buy, without trading. Treat it as a best case: nothing moves, so every unit is priced at today's opening price and a real pass usually trades less. Every cap and the merchant's gold are modelled exactly.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public bool SimulationMode { get => _o.SimulationMode; set { _o.SimulationMode = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL243}Economy settling delay (days, 0 = off)", 0, 100, Order = 11, RequireRestart = false,
            HintText = "{=TL343}No TradeLord trading before this campaign day, from the menu or on entry. Prices in a new campaign have not settled yet.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public int EconomySettlingDays { get => _o.EconomySettlingDays; set { _o.EconomySettlingDays = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL244}Trade XP multiplier", 0f, 3f, "#0%", Order = 12, RequireRestart = false,
            HintText = "{=TL344}Scales the Trade XP awarded for automated profit. 0 disables XP.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public float TradeXpMultiplier { get => _o.TradeXpMultiplier; set { _o.TradeXpMultiplier = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL245}Auto-mark best sell market on map", Order = 13, RequireRestart = false,
            HintText = "{=TL345}Moves a map tracker to whichever market in reach pays most for your current cargo, following you as you ride. A village is only ever marked while Trade with villages is on. It keeps to the travel ceilings. ON by default; clicking a town in the ledger panel still pins a marker by hand.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public bool MarkBestSellTownOnMap { get => _o.MarkBestSellTownOnMap; set { _o.MarkBestSellTownOnMap = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL247}Coin sound on trade", Order = 15, RequireRestart = false,
            HintText = "{=TL347}Play a coin sound when a pass actually moves something. A pass that trades nothing stays silent.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public bool CoinSound { get => _o.CoinSound; set { _o.CoinSound = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL248}Detailed trade summary", Order = 16, RequireRestart = false,
            HintText = "{=TL348}Name the goods in the one-line trade summary, e.g. 'TradeLord sold 8 Olives, 3 Wine for 240 denars', instead of a bare item count. The full list is always written to TradeLord.log.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public bool DetailedTradeSummary { get => _o.DetailedTradeSummary; set { _o.DetailedTradeSummary = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL249}Silence trade messages", Order = 17, RequireRestart = false,
            HintText = "{=TL349}Trading done automatically, both as you enter a market and when you meet a caravan or a party of villagers on the road, reports to TradeLord.log only, with no lines on screen. The trade entry in the menu always reports.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public bool QuietAutomation { get => _o.QuietAutomation; set { _o.QuietAutomation = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL269}Free passage from bandits", Order = 18, RequireRestart = false,
            HintText = "{=TL369}When you run into looters or bandits, TradeLord adds a line asking to be let past. Saying it ends the encounter with no fight and no ransom, and they leave you alone for a few hours. It is ON by default.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 5)]
        public bool BanditGetawayCheat { get => _o.BanditGetawayCheat; set { _o.BanditGetawayCheat = value; Options.Bump(); } }


        [SettingPropertyInteger("{=TL221}Restock and keep food (days of supply)", 0, 30, Order = 0, RequireRestart = false,
            HintText = "{=TL321}The days of food TradeLord keeps you stocked to. It holds this much back before selling food and tops you back up as it trades, buying the cheapest a market has. Your never-buy list still holds, and it stops before your gold reaches your reserve. 0 turns both off. Default 3.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 6)]
        public int KeepFoodDays { get => _o.KeepFoodDays; set { _o.KeepFoodDays = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL261}Keep some of every kind of food", Order = 1, RequireRestart = false,
            HintText = "{=TL361}Hold back a few of every kind of food you carry, so the party keeps its food variety morale bonus. It needs Restock and keep food (days of supply) above turned on to do anything, and what it holds back counts towards those days. Livestock is left out, since TradeLord trades a herd as goods and never as food. OFF by default.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 6)]
        public bool KeepEveryFoodKind { get => _o.KeepEveryFoodKind; set { _o.KeepEveryFoodKind = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL262}How many of each kind of food to keep", 1, 50, Order = 2, RequireRestart = false,
            HintText = "{=TL362}How many of each kind of food the switch above holds back. This is a count of the food itself, not a number of days: Restock and keep food (days of supply) above is the one that works in days. Two is usually enough.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 6)]
        public int KeepPerFoodKind { get => _o.KeepPerFoodKind; set { _o.KeepPerFoodKind = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL225}Protect unique and crafted items", Order = 3, RequireRestart = false,
            HintText = "{=TL325}Never auto-trade unique or player-crafted items. A haul animal is never sold by policy whatever you set here, and only an explicit always-sell entry can move one.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 6)]
        public bool ProtectSpecial { get => _o.ProtectSpecial; set { _o.ProtectSpecial = value; Options.Bump(); } }

        [SettingPropertyDropdown("{=TL264}Smeltable weapons", Order = 4, RequireRestart = false,
            HintText = "{=TL364}What to do with a weapon the smithy can break down for parts. Sell them is the default. Keep every one holds anything built from smithing parts, forged or looted off a bandit alike. Keep the ones you have not learned holds a weapon only while one of its parts is still locked in your smithy.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 6)]
        public Dropdown<string> KeepSmeltableWeapons
        {
            get => _smeltable;
            set { _smeltable = value; Follows(value, () => _o.KeepSmeltableWeapons, picked => _o.KeepSmeltableWeapons = picked); Options.Bump(); }
        }

        [SettingPropertyInteger("{=TL228}Sell loot up to tier (0 = off)", 0, 6, Order = 5, RequireRestart = false,
            HintText = "{=TL328}Also sells weapons and armor of this tier and below. Starts at tier 1, which is the gear looters and bandits drop. Locks and protections still apply.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 6)]
        public int MaxLootTier { get => _o.MaxLootTier; set { _o.MaxLootTier = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL229}Hold cargo for the best market", Order = 6, RequireRestart = false,
            HintText = "{=TL329}Skip selling here when this market pays clearly less than the best known market, so your cargo waits for the town that pays for it. It holds back what you bought and what you looted alike. OFF by default, in which case looted gear goes to the first market that can pay for it.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 6)]
        public bool PreferBestSellTown { get => _o.PreferBestSellTown; set { _o.PreferBestSellTown = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL230}Best-market tolerance", 0.5f, 1f, "#0%", Order = 7, RequireRestart = false,
            HintText = "{=TL330}Sell here anyway if this market pays at least this fraction of the best known price. It does nothing while the setting above is OFF.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 6)]
        public float BestSellTownTolerance { get => _o.BestSellTownTolerance; set { _o.BestSellTownTolerance = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL275}Sell animals that slow you down", Order = 8, RequireRestart = false,
            HintText = "{=TL375}Sells animals to lift the herd speed penalty, only while it is on you and only as many as it takes. Livestock goes first, then a spare mount, then your haul animals, and your war horses and noble horses last of all, and it keeps enough haul animals to carry what you are already carrying. Anything you protected is left alone. ON by default.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 6)]
        public bool SellSpareMounts { get => _o.SellSpareMounts; set { _o.SellSpareMounts = value; Options.Bump(); } }

        [SettingPropertyText("{=TL231}Never sell (item ids or names, comma separated)", Order = 9, RequireRestart = false,
            HintText = "{=TL331}Goods TradeLord must never sell. Name each by item id, by the short name TradeLord.log prints such as grain or iron_ore, or by the name the game shows, comma separated. It leaves these alone when buying too.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 6)]
        public string NeverSellItems { get => _o.NeverSellItems; set { _o.NeverSellItems = value; Options.Bump(); } }

        [SettingPropertyText("{=TL232}Always sell (item ids or names, comma separated)", Order = 10, RequireRestart = false,
            HintText = "{=TL332}Goods TradeLord always sells, past the category policies, the unique and crafted protection and the food reserve. Named as above. The never-sell list, an inventory lock and a good a quest is waiting on still hold. This is the only way to sell a haul animal for profit.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 6)]
        public string AlwaysSellItems { get => _o.AlwaysSellItems; set { _o.AlwaysSellItems = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL234}Gold reserve", 0, 100000, Order = 0, RequireRestart = false,
            HintText = "{=TL334}Never spend below this much gold. Default 300, which is enough to barter your way out of two hostile encounters and still meet a wage payment after a shopping trip.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public int GoldReserve { get => _o.GoldReserve; set { _o.GoldReserve = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL266}Keep gold for days of wages", 0, 30, Order = 1, RequireRestart = false,
            HintText = "{=TL366}Also hold back this many days of your troops' wages on top of the gold reserve above, so a shopping trip never eats the payroll. It is worked out from your real wage bill every time TradeLord trades, so it grows with the army. 0 holds back the flat reserve only.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public int KeepWageDays { get => _o.KeepWageDays; set { _o.KeepWageDays = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL235}Buy cap per item (count, 0 = off)", 0, 500, Order = 2, RequireRestart = false,
            HintText = "{=TL335}Most units of one good TradeLord buys per visit. 0 = no limit on the count. Default 32.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public int BuyCapPerItem { get => _o.BuyCapPerItem; set { _o.BuyCapPerItem = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL236}Buy cap per item (denars, 0 = off)", 0, 50000, Order = 3, RequireRestart = false,
            HintText = "{=TL336}Also cap spending per item per visit in denars.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public int BuyValueCapPerItem { get => _o.BuyValueCapPerItem; set { _o.BuyValueCapPerItem = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL251}Stop buying at this many held (0 = off)", 0, 5000, Order = 4, RequireRestart = false,
            HintText = "{=TL351}Once your party already carries this many of a good, TradeLord leaves it alone and spends on something else. Counts what you are carrying now plus anything bought this visit. Selling is unaffected. 0 = no limit.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public int MaxHeldPerItem { get => _o.MaxHeldPerItem; set { _o.MaxHeldPerItem = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL237}Max spend per visit (0 = unlimited)", 0, 100000, Order = 5, RequireRestart = false,
            HintText = "{=TL337}Total denars TradeLord may spend per settlement visit. Default 1000.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public int MaxSpendPerVisit { get => _o.MaxSpendPerVisit; set { _o.MaxSpendPerVisit = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL238}Resale safety factor", 0.5f, 1f, "#0%", Order = 6, RequireRestart = false,
            HintText = "{=TL338}Assume only this fraction of the best sell price is still available by the time you arrive.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public float ResaleSafetyFactor { get => _o.ResaleSafetyFactor; set { _o.ResaleSafetyFactor = value; Options.Bump(); } }


        [SettingPropertyBool("{=TL265}Buy to fill the ships", Order = 8, RequireRestart = false,
            HintText = "{=TL365}Size purchases to what your ships can hold rather than what your carts can, so you can load the fleet while you are ashore. Your party has to be able to sail; without a fleet TradeLord counts the carts instead and says so in its log. OFF by default.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public bool UseFleetCapacity { get => _o.UseFleetCapacity; set { _o.UseFleetCapacity = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL239}Never buy grain", Order = 9, RequireRestart = false,
            HintText = "{=TL339}Grain is heavy and low margin, so buying it fills the cargo for little return. Selling and the food reserve are unaffected. ON by default.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public bool NeverBuyGrain { get => _o.NeverBuyGrain; set { _o.NeverBuyGrain = value; Options.Bump(); } }

        [SettingPropertyText("{=TL240}Never buy (item ids or names, comma separated)", Order = 10, RequireRestart = false,
            HintText = "{=TL340}Goods TradeLord must never buy. Named by item id or by the name the game shows, comma separated, as above. Selling them is unaffected.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public string NeverBuyItems { get => _o.NeverBuyItems; set { _o.NeverBuyItems = value; Options.Bump(); } }

        [SettingPropertyText("{=TL252}Always buy (item ids or names, comma separated)", Order = 11, RequireRestart = false,
            HintText = "{=TL352}Goods TradeLord always buys, past the category policies and the never-buy-grain switch. Named by item id or by the name the game shows, comma separated, as above. The never lists above and an inventory lock still hold, and it still buys only what it can sell on for more somewhere in reach.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public string AlwaysBuyItems { get => _o.AlwaysBuyItems; set { _o.AlwaysBuyItems = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL267}Buy haul animals", Order = 12, RequireRestart = false,
            HintText = "{=TL367}Buy any haul animal, a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse or a Pack Camel, whenever a market asks no more than one is worth, so your party carries more. It never buys more than you can drive without slowing down, and it stops before your gold reaches your reserve. ON by default.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public bool BuyHaulAnimals { get => _o.BuyHaulAnimals; set { _o.BuyHaulAnimals = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL274}Share of the hold one good may fill (0 = off)", 0f, 1f, "#0%", Order = 14, RequireRestart = false,
            HintText = "{=TL374}Stop buying a good once it would fill more than this share of what your party can carry. It is measured against your real capacity, so the ceiling grows with your carts and haul animals. 0 turns it off. Selling is unaffected.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 7)]
        public float MaxHeldShare { get => _o.MaxHeldShare; set { _o.MaxHeldShare = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL246}Write a price trace to the log", Order = 0, RequireRestart = false,
            HintText = "{=TL389}Writes to TradeLord.log what a market pays and charges for every good you carry, so you can see why a price does not match the trade screen. It names the market and any mod moving prices. OFF by default, since it makes the log much longer.")]
        [SettingPropertyGroup("{=TL107}Debug", GroupOrder = 8)]
        public bool PriceTrace { get => _o.PriceTrace; set { _o.PriceTrace = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL280}Score the forecast in the log", Order = 1, RequireRestart = false,
            HintText = "{=TL398}Writes to TradeLord.log what the forecast said a market would hold and what it held when you walked in, good by good, with how far off it was. Needs Count what is on its way to a market. OFF by default, since it makes the log longer.")]
        [SettingPropertyGroup("{=TL107}Debug", GroupOrder = 8)]
        public bool ForecastScore { get => _o.ForecastScore; set { _o.ForecastScore = value; Options.Bump(); } }
    }
}
