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

namespace TradeLord.Mcm
{
    public static class McmSettingsBootstrap
    {
        public static void Init()
        {
            Settings instance = Settings.Instance;
            instance?.FollowLanguage();
            Guard.Run("Mcm.ScreenTongue", ScreenTongue.Follow);
        }
    }

    internal static class ScreenTongue
    {
        private static AccessTools.FieldRef<SettingsPropertyDefinition, string> _name, _hint, _group;

        internal static void Follow()
        {
            _name = Reaches("<DisplayName>k__BackingField");
            _hint = Reaches("<HintText>k__BackingField");
            _group = Reaches("<GroupName>k__BackingField");
            ConstructorInfo built = AccessTools.Constructor(typeof(SettingsPropertyDefinition), new[]
            {
                typeof(IEnumerable<IPropertyDefinitionBase>), typeof(IPropertyGroupDefinition),
                typeof(IRef), typeof(char)
            });
            if (_name == null || _hint == null || _group == null || built == null)
            {
                Log.Write("the settings screen could not be wired to TradeLord's own language - it follows the game's language instead");
                return;
            }
            new Harmony(SubModule.HarmonyId + ".mcm").Patch(built,
                postfix: new HarmonyMethod(typeof(ScreenTongue), nameof(Spoken)));
        }

        private static AccessTools.FieldRef<SettingsPropertyDefinition, string> Reaches(string field)
        {
            FieldInfo found = AccessTools.Field(typeof(SettingsPropertyDefinition), field);
            return found == null ? null : AccessTools.FieldRefAccess<SettingsPropertyDefinition, string>(found);
        }

        private static void Spoken(SettingsPropertyDefinition __instance)
        {
            Say(_name, __instance);
            Say(_hint, __instance);
            Say(_group, __instance);
        }

        private static void Say(AccessTools.FieldRef<SettingsPropertyDefinition, string> at, SettingsPropertyDefinition of)
        {
            string said = Tongue.Said(at(of));
            if (said != null) at(of) = said;
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

        [SettingPropertyDropdown("{=TL250}Language", Order = 0, RequireRestart = false,
            HintText = "{=TL350}The language TradeLord speaks in the game: its trade messages, the ledger panel, the price tooltips and its town menu entries. English by default. This settings screen takes the new language the next time you open it. Town menu entries take it when you next load a campaign.")]
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
            }
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
            Retell();
        }

        private static void Follows(Dropdown<string> from, Func<int> held, Action<int> keep)
        {
            if (from == null) return;
            keep(from.SelectedIndex);
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
        }

        private static void Retold(Dropdown<string> shown, string[] words)
        {
            shown.Clear();
            shown.AddRange(Spoken(words));
        }

        [SettingPropertyBool("{=TL201}Live world prices (default)", Order = 0, RequireRestart = false,
            HintText = "{=TL301}ON (default): prices are read live from the world economy, including markets you have not visited. OFF: only prices you have seen in person are used.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public bool Omniscient { get => _o.Omniscient; set { _o.Omniscient = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL202}Observation shelf life (days, 0 = never expire)", 0, 200, Order = 1, RequireRestart = false,
            HintText = "{=TL302}Observed prices older than this are ignored. 0 = never expire.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public int ObservationShelfLifeDays { get => _o.ObservationShelfLifeDays; set { _o.ObservationShelfLifeDays = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL203}Exclude hostile markets", Order = 2, RequireRestart = false,
            HintText = "{=TL303}Never scan, suggest or auto-trade with settlements at war with you.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public bool ExcludeHostileTowns { get => _o.ExcludeHostileTowns; set { _o.ExcludeHostileTowns = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL204}Scan radius (map units, 0 = whole map)", 0f, 1000f, "0", Order = 3, RequireRestart = false,
            HintText = "{=TL304}Limit price scans to markets within this straight-line distance. 0 = whole map.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public float ScanRadius { get => _o.ScanRadius; set { _o.ScanRadius = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL205}Minimum stock for buy suggestions", 0, 100, Order = 4, RequireRestart = false,
            HintText = "{=TL305}Best-buy hints require at least this many units in stock. 0 = off. Live-price mode only, because observed mode records prices, not stock levels.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public int MinTownStock { get => _o.MinTownStock; set { _o.MinTownStock = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL206}Travel ceiling (days, 0 = off)", 0f, 20f, "0.0", Order = 5, RequireRestart = false,
            HintText = "{=TL306}Markets farther than this many travel days are hidden from tooltips, and no suggested route's total trip (you -> buy town -> sell town) may exceed it. Default 3.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public float MaxTravelDays { get => _o.MaxTravelDays; set { _o.MaxTravelDays = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL207}Village travel ceiling (days, 0 = off)", 0f, 10f, "0.0", Order = 6, RequireRestart = false,
            HintText = "{=TL307}A separate, stricter travel ceiling for villages. Default 1.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public float MaxVillageTravelDays { get => _o.MaxVillageTravelDays; set { _o.MaxVillageTravelDays = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL208}Conservative route projection", Order = 7, RequireRestart = false,
            HintText = "{=TL308}Apply the resale safety factor to the sell side when ranking and totalling routes, so listed profit allows for prices drifting before you arrive. OFF shows raw margins. Routes must clear the safety factor to be listed either way, since that is the same test a buying pass applies on arrival.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public bool ConservativeRouteProjection { get => _o.ConservativeRouteProjection; set { _o.ConservativeRouteProjection = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL209}Bulk price simulation", Order = 8, RequireRestart = false,
            HintText = "{=TL309}Price a lot unit by unit through the game's own price model, so quantity and profit account for your own buying moving the price. OFF prices every unit at the first unit's price, which reads higher than the trip will pay. Towns and live-price mode only: villages expose no supply or demand data, and observed mode does not read live market internals.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public bool BulkSimulation { get => _o.BulkSimulation; set { _o.BulkSimulation = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL210}Rank routes by confidence", Order = 9, RequireRestart = false,
            HintText = "{=TL310}Order the panel by profit per day, discounted by how likely that profit is to survive the trip. The discount accounts for margin left after the bulk walk, stock depth, trip length, caravan traffic, and in observed mode the age of the prices. OFF ranks on raw profit per day and the Score column shows that instead.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public bool ConfidenceRanking { get => _o.ConfidenceRanking; set { _o.ConfidenceRanking = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL211}Show best buy/sell in tooltips", Order = 0, RequireRestart = false,
            HintText = "{=TL311}Adds the best known buy and sell markets, with stock and travel time, to item tooltips.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 2)]
        public bool TooltipHints { get => _o.TooltipHints; set { _o.TooltipHints = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL212}Suppress vanilla trade-rumor lines", Order = 1, RequireRestart = false,
            HintText = "{=TL312}Skips the vanilla merchandise rumor block so the tooltip shows one consistent set of price hints.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 2)]
        public bool SuppressVanillaTradeLines { get => _o.SuppressVanillaTradeLines; set { _o.SuppressVanillaTradeLines = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL213}Color prices by world market", Order = 2, RequireRestart = false,
            HintText = "{=TL313}Colors trade-good and livestock rows in the inventory by how this market's price compares with the best known market.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 2)]
        public bool ProfitColoring { get => _o.ProfitColoring; set { _o.ProfitColoring = value; Options.Bump(); } }

        [SettingPropertyText("{=TL214}Ledger panel hotkey (map screen)", Order = 3, RequireRestart = false,
            HintText = "{=TL314}Key that opens the ledger panel on the campaign map. A single key name such as T, Y or F5, optionally with Ctrl, Alt or Shift in front, e.g. \"Ctrl+T\". This mod does not take keys away from the game, so a bare key the game also uses will trigger both actions. Use a modifier to avoid that.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 2)]
        public string PanelKey { get => _o.PanelKey; set { _o.PanelKey = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL215}TradeLord button on the map screen", Order = 4, RequireRestart = false,
            HintText = "{=TL315}A clickable TradeLord button on the right edge of the campaign map that opens the ledger panel. Turn OFF if it interferes with map clicks.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 2)]
        public bool ShowMapButton { get => _o.ShowMapButton; set { _o.ShowMapButton = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL217}Auto sell", Order = 0, RequireRestart = false,
            HintText = "{=TL317}Sells whatever your rules allow the moment you walk into a market, without being asked. Trade XP is awarded. With this off, TradeLord sells only when you pick its trade entry in the menu.")]
        [SettingPropertyGroup("{=TL104}Automation", GroupOrder = 3)]
        public bool AutoSellOnEntry { get => _o.AutoSellOnEntry; set { _o.AutoSellOnEntry = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL218}Auto buy", Order = 1, RequireRestart = false,
            HintText = "{=TL318}Buys the moment you walk into a market, after any selling. With this off, TradeLord buys only when you pick its trade entry in the menu.")]
        [SettingPropertyGroup("{=TL104}Automation", GroupOrder = 3)]
        public bool AutoBuyOnEntry { get => _o.AutoBuyOnEntry; set { _o.AutoBuyOnEntry = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL216}Trade entry in town menu", Order = 0, RequireRestart = false,
            HintText = "{=TL316}Shows the single TradeLord trade entry in town and village menus, which sells and then buys in one go. Turn it off to leave the menu to trading done automatically as you arrive.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public bool QuickSellMenu { get => _o.QuickSellMenu; set { _o.QuickSellMenu = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL260}Ledger entry in town menu", Order = 1, RequireRestart = false,
            HintText = "{=TL360}Shows the TradeLord ledger entry in town and village menus, which opens the route panel. Turn it off if you would rather open the panel with its hotkey or its map button.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public bool LedgerMenuEntry { get => _o.LedgerMenuEntry; set { _o.LedgerMenuEntry = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL241}Trade with villages", Order = 2, RequireRestart = false,
            HintText = "{=TL341}TradeLord trades in village menus as well as town menus, and villages join the price scans.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public bool TradeWithVillages { get => _o.TradeWithVillages; set { _o.TradeWithVillages = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL273}Trade with caravans you meet", Order = 3, RequireRestart = false,
            HintText = "{=TL373}Talk to a caravan on the road and TradeLord trades with it there and then, selling what clears your margin and buying what it can sell on for more somewhere in reach. The caravan pays out of its own purse, so it stops when that purse runs dry. Every rule a market visit obeys still holds here. ON by default.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public bool TradeWithCaravans { get => _o.TradeWithCaravans; set { _o.TradeWithCaravans = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL220}Minimum profit margin", 0f, 2f, "#0%", Order = 4, RequireRestart = false,
            HintText = "{=TL320}The margin every trade must clear, in both directions. Sell only if the price exceeds your cost basis by at least this much. Buy, or list a route, only if the far market exceeds the local price by this much after the resale safety factor. Raising it trades less.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public float MinProfitMargin { get => _o.MinProfitMargin; set { _o.MinProfitMargin = value; Options.Bump(); } }

        [SettingPropertyDropdown("{=TL222}Food policy", Order = 5, RequireRestart = false,
            HintText = "{=TL322}What automated trading may do with food. The days-of-supply food reserve is separate and still applies.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public Dropdown<string> FoodPolicy
        {
            get => _foodPolicy;
            set { _foodPolicy = value; Follows(value, () => _o.FoodPolicy, picked => _o.FoodPolicy = picked); Options.Bump(); }
        }

        [SettingPropertyDropdown("{=TL223}Smithing material policy", Order = 6, RequireRestart = false,
            HintText = "{=TL323}What automated trading may do with charcoal, hardwood, iron ore and ingots. Pick Leave alone to keep smithing stock out of automated trading entirely.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public Dropdown<string> CraftingPolicy
        {
            get => _craftingPolicy;
            set { _craftingPolicy = value; Follows(value, () => _o.CraftingPolicy, picked => _o.CraftingPolicy = picked); Options.Bump(); }
        }

        [SettingPropertyDropdown("{=TL224}Livestock policy", Order = 7, RequireRestart = false,
            HintText = "{=TL324}What automated trading may do with sheep, cattle and hogs. Buying is capped by the game's own herding calculation, so it will not push the party into the herd speed penalty. Haul animals and riding mounts have their own settings and this one does not affect them.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public Dropdown<string> LivestockPolicy
        {
            get => _livestockPolicy;
            set { _livestockPolicy = value; Follows(value, () => _o.LivestockPolicy, picked => _o.LivestockPolicy = picked); Options.Bump(); }
        }

        [SettingPropertyBool("{=TL226}Respect inventory locks", Order = 8, RequireRestart = false,
            HintText = "{=TL326}Locked items in the inventory screen are never auto-traded. Locks are matched as the game stores them, by item and modifier.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public bool RespectLocks { get => _o.RespectLocks; set { _o.RespectLocks = value; Options.Bump(); } }

        [SettingPropertyDropdown("{=TL227}What a good counts as having cost you", Order = 9, RequireRestart = false,
            HintText = "{=TL327}The price a sale is measured against, so it sets both the profit TradeLord reports and the Trade XP the sale earns. Anything you never bought, loot included, is valued at the cheapest market you know of whichever one you pick.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public Dropdown<string> CostBasisMode
        {
            get => _costBasis;
            set { _costBasis = value; Follows(value, () => _o.CostBasisMode, picked => _o.CostBasisMode = picked); Options.Bump(); }
        }

        [SettingPropertyBool("{=TL242}Simulation mode (dry run)", Order = 10, RequireRestart = false,
            HintText = "{=TL342}Report what TradeLord would sell and buy, without trading. Treat the result as a best case: nothing moves, so the market does not react and every unit is priced at today's opening price. A real pass stops at the unit where the margin runs out, so it usually trades less and gets less per unit. The merchant's gold, your carry weight, the gold reserve and every per-item and per-visit cap are modelled exactly; only your own effect on the price is not.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public bool SimulationMode { get => _o.SimulationMode; set { _o.SimulationMode = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL243}Economy settling delay (days, 0 = off)", 0, 100, Order = 11, RequireRestart = false,
            HintText = "{=TL343}No TradeLord trading before this campaign day, from the menu or on entry. Prices in a new campaign have not settled yet.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public int EconomySettlingDays { get => _o.EconomySettlingDays; set { _o.EconomySettlingDays = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL244}Trade XP multiplier", 0f, 3f, "#0%", Order = 12, RequireRestart = false,
            HintText = "{=TL344}Scales the Trade XP awarded for automated profit. 0 disables XP.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public float TradeXpMultiplier { get => _o.TradeXpMultiplier; set { _o.TradeXpMultiplier = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL245}Auto-mark best sell town on map", Order = 13, RequireRestart = false,
            HintText = "{=TL345}Moves a map tracker to whichever nearby town pays most for your current cargo. Re-evaluated daily and whenever you enter or leave a settlement, so it follows what you are carrying now. Limited by the travel ceiling below. ON by default; clicking a town in the ledger panel still pins a marker by hand.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public bool MarkBestSellTownOnMap { get => _o.MarkBestSellTownOnMap; set { _o.MarkBestSellTownOnMap = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL246}Auto-marker travel ceiling (days, 0 = off)", 0f, 10f, "0.0", Order = 14, RequireRestart = false,
            HintText = "{=TL346}The auto-marker ignores towns farther than this many travel days away. Default 1.5.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public float MarkerMaxTravelDays { get => _o.MarkerMaxTravelDays; set { _o.MarkerMaxTravelDays = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL247}Coin sound on trade", Order = 15, RequireRestart = false,
            HintText = "{=TL347}Play a coin sound when a pass actually moves something. A pass that trades nothing stays silent.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public bool CoinSound { get => _o.CoinSound; set { _o.CoinSound = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL248}Detailed trade summary", Order = 16, RequireRestart = false,
            HintText = "{=TL348}Name the goods in the one-line trade summary, e.g. 'TradeLord sold 8 Olives, 3 Wine for 240 denars', instead of a bare item count. The full list is always written to TradeLord.log.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public bool DetailedTradeSummary { get => _o.DetailedTradeSummary; set { _o.DetailedTradeSummary = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL249}Quiet automation", Order = 17, RequireRestart = false,
            HintText = "{=TL349}Trading done automatically as you enter a market reports to TradeLord.log only, with no lines on screen. The trade entry in the menu always reports, since you asked for it. The first-run automation notice, the empty-purse warning and the cargo-full warning are unaffected.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public bool QuietAutomation { get => _o.QuietAutomation; set { _o.QuietAutomation = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL269}Free passage from bandits", Order = 18, RequireRestart = false,
            HintText = "{=TL369}Adds a line to the encounter screen when you run into looters, sea raiders, forest bandits, mountain bandits, steppe bandits or desert bandits. Taking it ends the encounter with no fight and no ransom, and they leave you alone for a few hours afterwards. It is ON by default; switch it off for a campaign you want to fight your own way out of.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 4)]
        public bool BanditGetawayCheat { get => _o.BanditGetawayCheat; set { _o.BanditGetawayCheat = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL221}Keep food (days of supply)", 0, 30, Order = 0, RequireRestart = false,
            HintText = "{=TL321}Hold back this many days of food before selling any. The cheapest food per day fed is reserved first, and livestock only if nothing else covers the reserve. 0 sells every scrap of food.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 5)]
        public int KeepFoodDays { get => _o.KeepFoodDays; set { _o.KeepFoodDays = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL261}Keep some of every kind of food", Order = 1, RequireRestart = false,
            HintText = "{=TL361}Hold back a few of every kind of food you carry, so the party keeps its food variety morale bonus. What it holds back counts towards the days of supply above rather than adding to it. Livestock is left out, since a herd is slaughtered for meat rather than eaten as a kind of food in its own right. OFF by default.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 5)]
        public bool KeepEveryFoodKind { get => _o.KeepEveryFoodKind; set { _o.KeepEveryFoodKind = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL262}How many of each kind to keep", 1, 50, Order = 2, RequireRestart = false,
            HintText = "{=TL362}How many of every kind of food the switch above holds back. Three is enough that a day of eating does not wipe a kind out, and the bonus counts the kinds you carry rather than how much of them, so there is little gained by going higher.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 5)]
        public int KeepPerFoodKind { get => _o.KeepPerFoodKind; set { _o.KeepPerFoodKind = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL225}Protect unique and crafted items", Order = 3, RequireRestart = false,
            HintText = "{=TL325}Never auto-trade unique or player-crafted items. A haul animal is never sold by policy whatever you set here, and only an explicit always-sell entry can move one.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 5)]
        public bool ProtectSpecial { get => _o.ProtectSpecial; set { _o.ProtectSpecial = value; Options.Bump(); } }

        [SettingPropertyDropdown("{=TL264}Smeltable weapons", Order = 4, RequireRestart = false,
            HintText = "{=TL364}What to do with a weapon the smithy can break down for parts, so a smithing playthrough keeps its raw material. Sell them is the default. Keep every one holds back anything built from smithing parts, forged or looted off a bandit alike, so cheap loot piles up with the rest. Keep the ones you have not learned holds a weapon only while one of its parts is still locked in your smithy, and sells the rest once it can teach you nothing. Armour, shields, bows and crossbows carry no smithing design, so those are always sold, and an always-sell entry still wins.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 5)]
        public Dropdown<string> KeepSmeltableWeapons
        {
            get => _smeltable;
            set { _smeltable = value; Follows(value, () => _o.KeepSmeltableWeapons, picked => _o.KeepSmeltableWeapons = picked); Options.Bump(); }
        }

        [SettingPropertyInteger("{=TL228}Sell loot up to tier (0 = off)", 0, 6, Order = 5, RequireRestart = false,
            HintText = "{=TL328}Also sells weapons and armor of this tier and below. Starts at tier 1, which is the gear looters and bandits drop. Locks and protections still apply.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 5)]
        public int MaxLootTier { get => _o.MaxLootTier; set { _o.MaxLootTier = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL229}Hold cargo for the best market", Order = 6, RequireRestart = false,
            HintText = "{=TL329}Skip selling here when this market pays clearly less than the best known market, so your cargo waits for the town that pays for it. It holds back everything you carry, what you bought and what you looted alike. OFF by default, in which case a good you bought still has to clear your profit margin, and looted gear goes to the first market that can pay for it.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 5)]
        public bool PreferBestSellTown { get => _o.PreferBestSellTown; set { _o.PreferBestSellTown = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL230}Best-market tolerance", 0.5f, 1f, "#0%", Order = 7, RequireRestart = false,
            HintText = "{=TL330}Sell here anyway if this market pays at least this fraction of the best known price. It does nothing while the setting above is OFF.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 5)]
        public float BestSellTownTolerance { get => _o.BestSellTownTolerance; set { _o.BestSellTownTolerance = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL275}Sell spare mounts that slow you down", Order = 8, RequireRestart = false,
            HintText = "{=TL375}Sell a riding horse or camel nobody in your party can ride, but only while those spare mounts are dragging you into the herd speed penalty, and only as many as it takes to get out of it. It sells the cheapest ones first, so your war horses stay in the baggage. A haul animal is never sold this way, and neither is anything you locked, put on your never-sell list, or that the unique and crafted protection covers. ON by default.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 5)]
        public bool SellSpareMounts { get => _o.SellSpareMounts; set { _o.SellSpareMounts = value; Options.Bump(); } }

        [SettingPropertyText("{=TL231}Never sell (item ids or names, comma separated)", Order = 9, RequireRestart = false,
            HintText = "{=TL331}Goods TradeLord must never sell. Name each one either by the item id, the short internal name such as grain, wine or iron_ore that TradeLord.log prints for every good it moves, or by the name the game shows you, such as Iron Ore. Separate them with commas. An entry matching no good in this game is named in the log and said on screen, rather than passing quietly. It leaves these alone when buying too, since it would have no way to sell them on.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 5)]
        public string NeverSellItems { get => _o.NeverSellItems; set { _o.NeverSellItems = value; Options.Bump(); } }

        [SettingPropertyText("{=TL232}Always sell (item ids or names, comma separated)", Order = 10, RequireRestart = false,
            HintText = "{=TL332}Goods TradeLord always sells, past the category policies, the unique and crafted protection and the food reserve. Named by item id or by the name the game shows, comma separated, as above. The never-sell list above and an inventory lock still hold. This is the only way to sell a haul animal.")]
        [SettingPropertyGroup("{=TL103}Selling", GroupOrder = 5)]
        public string AlwaysSellItems { get => _o.AlwaysSellItems; set { _o.AlwaysSellItems = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL234}Gold reserve", 0, 100000, Order = 0, RequireRestart = false,
            HintText = "{=TL334}Never spend below this much gold. Default 300, which is enough to barter your way out of two hostile encounters and still meet a wage payment after a shopping trip.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public int GoldReserve { get => _o.GoldReserve; set { _o.GoldReserve = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL266}Keep gold for days of wages", 0, 30, Order = 1, RequireRestart = false,
            HintText = "{=TL366}Also hold back this many days of your troops' wages on top of the gold reserve above, so a shopping trip never eats the payroll. It is worked out from your real wage bill every time TradeLord trades, so it grows with the army. 0 holds back the flat reserve only.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public int KeepWageDays { get => _o.KeepWageDays; set { _o.KeepWageDays = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL235}Buy cap per item (count, 0 = off)", 0, 500, Order = 2, RequireRestart = false,
            HintText = "{=TL335}Most units of one good TradeLord buys per visit. 0 = no limit on the count. Default 32.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public int BuyCapPerItem { get => _o.BuyCapPerItem; set { _o.BuyCapPerItem = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL236}Buy cap per item (denars, 0 = off)", 0, 50000, Order = 3, RequireRestart = false,
            HintText = "{=TL336}Also cap spending per item per visit in denars.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public int BuyValueCapPerItem { get => _o.BuyValueCapPerItem; set { _o.BuyValueCapPerItem = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL251}Stop buying at this many held (0 = off)", 0, 5000, Order = 4, RequireRestart = false,
            HintText = "{=TL351}Once your party already carries this many of a good, TradeLord leaves it alone and spends on something else. Counts what you are carrying now plus anything bought this visit. Selling is unaffected. 0 = no limit.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public int MaxHeldPerItem { get => _o.MaxHeldPerItem; set { _o.MaxHeldPerItem = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL237}Max spend per visit (0 = unlimited)", 0, 100000, Order = 5, RequireRestart = false,
            HintText = "{=TL337}Total denars TradeLord may spend per settlement visit. Default 1000.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public int MaxSpendPerVisit { get => _o.MaxSpendPerVisit; set { _o.MaxSpendPerVisit = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL238}Resale safety factor", 0.5f, 1f, "#0%", Order = 6, RequireRestart = false,
            HintText = "{=TL338}Assume only this fraction of the best sell price is still available by the time you arrive.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public float ResaleSafetyFactor { get => _o.ResaleSafetyFactor; set { _o.ResaleSafetyFactor = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL263}Restock food (days of supply)", 0, 30, Order = 7, RequireRestart = false,
            HintText = "{=TL363}When your party carries fewer than this many days of food, TradeLord buys the cheapest food here to top it back up, at whatever the market asks, before it trades for profit. Your gold reserve, your spending limit for the visit, your never-buy list and your food policy all still hold, and it leaves a village its last of each good. 0 turns restocking off. As you arrive at a market it runs only when auto buy is on.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public int ResupplyFoodDays { get => _o.ResupplyFoodDays; set { _o.ResupplyFoodDays = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL265}Buy to fill the ships", Order = 8, RequireRestart = false,
            HintText = "{=TL365}Size purchases to what your ships can hold rather than what your carts can, so you can load the fleet while you are ashore. Your party has to be able to sail; without a fleet TradeLord counts the carts instead and says so in its log. OFF by default.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public bool UseFleetCapacity { get => _o.UseFleetCapacity; set { _o.UseFleetCapacity = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL239}Never buy grain", Order = 9, RequireRestart = false,
            HintText = "{=TL339}Grain is heavy and low margin, so buying it fills the cargo for little return. Selling and the food reserve are unaffected. ON by default.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public bool NeverBuyGrain { get => _o.NeverBuyGrain; set { _o.NeverBuyGrain = value; Options.Bump(); } }

        [SettingPropertyText("{=TL240}Never buy (item ids or names, comma separated)", Order = 10, RequireRestart = false,
            HintText = "{=TL340}Goods TradeLord must never buy. Named by item id or by the name the game shows, comma separated, as above. Selling them is unaffected.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public string NeverBuyItems { get => _o.NeverBuyItems; set { _o.NeverBuyItems = value; Options.Bump(); } }

        [SettingPropertyText("{=TL252}Always buy (item ids or names, comma separated)", Order = 11, RequireRestart = false,
            HintText = "{=TL352}Goods TradeLord always buys, past the category policies and the never-buy-grain switch. Named by item id or by the name the game shows, comma separated, as above. The never lists above and an inventory lock still hold, and it still buys only what it can sell on for more somewhere in reach.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public string AlwaysBuyItems { get => _o.AlwaysBuyItems; set { _o.AlwaysBuyItems = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL267}Buy haul animals and mounts", Order = 12, RequireRestart = false,
            HintText = "{=TL367}Buy any haul animal, a mule, a sumpter horse, a work horse or a pack camel, whenever a market is asking no more than one is worth, so your party can carry more, and a horse or a camel your men can ride while you still have troops on foot. Worth here is the cheapest price you know of for that animal, or its own value where you know none. While your cargo is full it will go over that by the premium below. Livestock is not included, it has its own policy. It never buys more than your party can drive without slowing down, and your gold reserve, your spending limit for the visit and your never-buy list all still hold. ON by default.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public bool BuyPackAnimals { get => _o.BuyPackAnimals; set { _o.BuyPackAnimals = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL268}Haul animal premium while the cargo is full", 1f, 3f, "#0%", Order = 13, RequireRestart = false,
            HintText = "{=TL368}How far over the going rate TradeLord will go for a haul animal or a mount while your cargo is full and the extra capacity is worth paying for. 150% is half again as much. 100% turns the premium off, so it only ever buys at the going rate.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public float PackAnimalFullCargoPremium { get => _o.PackAnimalFullCargoPremium; set { _o.PackAnimalFullCargoPremium = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL274}Share of the hold one good may fill (0 = off)", 0f, 1f, "#0%", Order = 14, RequireRestart = false,
            HintText = "{=TL374}Stop buying a good once it would fill more than this share of what your party can carry. It is measured against your real capacity, so the ceiling grows with your carts and your pack animals instead of needing a new number every time the party grows. 0 turns it off. Selling is unaffected, and the flat per-item caps above still hold.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 6)]
        public float MaxHeldShare { get => _o.MaxHeldShare; set { _o.MaxHeldShare = value; Options.Bump(); } }
    }
}
