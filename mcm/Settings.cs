using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Common;

namespace TradeLord.Mcm
{
    public static class McmSettingsBootstrap
    {
        public static void Init()
        {
            Settings instance = Settings.Instance;
            Settings.Loaded = true;
            instance?.FollowLanguage();
        }
    }

    public class Settings : AttributeGlobalSettings<Settings>
    {
        internal static bool Loaded;

        public override string Id => "TradeLord";

        public override string DisplayName => ("TradeLord " + ModVersion()).TrimEnd();
        public override string FolderName => "TradeLord";
        public override string FormatType => "json2";

        private static string ModVersion()
        {
            try { return TaleWorlds.ModuleManager.ModuleHelper.GetModuleInfo("TradeLord").Version.ToString(); }
            catch { return ""; }
        }

        private Dropdown<string> _language =
            new Dropdown<string>(new[] { "English", "T\u00FCrk\u00E7e" }, Options.Current.Language);

        [SettingPropertyDropdown("{=TL250}Language", Order = 0, RequireRestart = false,
            HintText = "{=TL350}The language TradeLord speaks in the game: its trade messages, the ledger panel, the price tooltips and its town menu entries. English by default. This settings screen follows the language the game itself is set to. Town menu entries take the new language when you next load a campaign.")]
        [SettingPropertyGroup("{=TL100}Language", GroupOrder = 0)]
        public Dropdown<string> Language
        {
            get => _language;
            set { _language = value; Options.Current.Language = value?.SelectedIndex ?? 0; Options.Bump(); }
        }

        internal void FollowLanguage()
        {
            Options.Current.Language = Language.SelectedIndex;
            Language.PropertyChanged += (sender, args) =>
            {
                if (Options.Current.Language == Language.SelectedIndex) return;
                Options.Current.Language = Language.SelectedIndex;
                Options.Bump();
            };
        }

        [SettingPropertyBool("{=TL201}Live world prices (default)", Order = 0, RequireRestart = false,
            HintText = "{=TL301}ON (default): prices are read live from the world economy, including markets you have not visited. OFF: only prices you have seen in person are used.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public bool Omniscient { get => Options.Current.Omniscient; set { Options.Current.Omniscient = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL202}Observation shelf life (days, 0 = never expire)", 0, 200, Order = 1, RequireRestart = false,
            HintText = "{=TL302}Observed prices older than this are ignored. 0 = never expire.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public int ObservationShelfLifeDays { get => Options.Current.ObservationShelfLifeDays; set { Options.Current.ObservationShelfLifeDays = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL203}Exclude hostile markets", Order = 2, RequireRestart = false,
            HintText = "{=TL303}Never scan, suggest or auto-trade with settlements at war with you.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public bool ExcludeHostileTowns { get => Options.Current.ExcludeHostileTowns; set { Options.Current.ExcludeHostileTowns = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL204}Scan radius (map units, 0 = whole map)", 0f, 1000f, "0", Order = 3, RequireRestart = false,
            HintText = "{=TL304}Limit price scans to markets within this straight-line distance. 0 = whole map.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public float ScanRadius { get => Options.Current.ScanRadius; set { Options.Current.ScanRadius = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL205}Minimum stock for buy suggestions", 0, 100, Order = 4, RequireRestart = false,
            HintText = "{=TL305}Best-buy hints require at least this many units in stock. 0 = off. Live-price mode only, because observed mode records prices, not stock levels.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public int MinTownStock { get => Options.Current.MinTownStock; set { Options.Current.MinTownStock = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL206}Travel ceiling (days, 0 = off)", 0f, 20f, "0.0", Order = 5, RequireRestart = false,
            HintText = "{=TL306}Markets farther than this many travel days are hidden from tooltips, and no suggested route's total trip (you -> buy town -> sell town) may exceed it. Default 3.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public float MaxTravelDays { get => Options.Current.MaxTravelDays; set { Options.Current.MaxTravelDays = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL207}Village travel ceiling (days, 0 = off)", 0f, 10f, "0.0", Order = 6, RequireRestart = false,
            HintText = "{=TL307}A separate, stricter travel ceiling for villages. Default 1.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public float MaxVillageTravelDays { get => Options.Current.MaxVillageTravelDays; set { Options.Current.MaxVillageTravelDays = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL208}Conservative route projection", Order = 7, RequireRestart = false,
            HintText = "{=TL308}Apply the resale safety factor to the sell side when ranking and totalling routes, so listed profit allows for prices drifting before you arrive. OFF shows raw margins. Routes must clear the safety factor to be listed either way, since that is the same test quick-buy applies on arrival.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public bool ConservativeRouteProjection { get => Options.Current.ConservativeRouteProjection; set { Options.Current.ConservativeRouteProjection = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL209}Bulk price simulation", Order = 8, RequireRestart = false,
            HintText = "{=TL309}Price a lot unit by unit through the game's own price model, so quantity and profit account for your own buying moving the price. OFF prices every unit at the first unit's price, which reads higher than the trip will pay. Towns and live-price mode only: villages expose no supply or demand data, and observed mode does not read live market internals.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public bool BulkSimulation { get => Options.Current.BulkSimulation; set { Options.Current.BulkSimulation = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL210}Rank routes by confidence", Order = 9, RequireRestart = false,
            HintText = "{=TL310}Order the panel by profit per day, discounted by how likely that profit is to survive the trip. The discount accounts for margin left after the bulk walk, stock depth, trip length, caravan traffic, and in observed mode the age of the prices. OFF ranks on raw profit per day and the Score column shows that instead.")]
        [SettingPropertyGroup("{=TL101}Knowledge", GroupOrder = 1)]
        public bool ConfidenceRanking { get => Options.Current.ConfidenceRanking; set { Options.Current.ConfidenceRanking = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL211}Show best buy/sell in tooltips", Order = 0, RequireRestart = false,
            HintText = "{=TL311}Adds the best known buy and sell markets, with stock and travel time, to item tooltips.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 2)]
        public bool TooltipHints { get => Options.Current.TooltipHints; set { Options.Current.TooltipHints = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL212}Suppress vanilla trade-rumor lines", Order = 1, RequireRestart = false,
            HintText = "{=TL312}Skips the vanilla merchandise rumor block so the tooltip shows one consistent set of price hints.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 2)]
        public bool SuppressVanillaTradeLines { get => Options.Current.SuppressVanillaTradeLines; set { Options.Current.SuppressVanillaTradeLines = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL213}Color prices by world market", Order = 2, RequireRestart = false,
            HintText = "{=TL313}Colors trade-good and livestock rows in the inventory by how this market's price compares with the best known market.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 2)]
        public bool ProfitColoring { get => Options.Current.ProfitColoring; set { Options.Current.ProfitColoring = value; Options.Bump(); } }

        [SettingPropertyText("{=TL214}Ledger panel hotkey (map screen)", Order = 3, RequireRestart = false,
            HintText = "{=TL314}Key that opens the ledger panel on the campaign map. A single key name such as T, Y or F5, optionally with Ctrl, Alt or Shift in front, e.g. \"Ctrl+T\". This mod does not take keys away from the game, so a bare key the game also uses will trigger both actions. Use a modifier to avoid that.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 2)]
        public string PanelKey { get => Options.Current.PanelKey; set { Options.Current.PanelKey = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL215}TradeLord button on the map screen", Order = 4, RequireRestart = false,
            HintText = "{=TL315}A clickable TradeLord button on the right edge of the campaign map that opens the ledger panel. Turn OFF if it interferes with map clicks.")]
        [SettingPropertyGroup("{=TL102}Insight", GroupOrder = 2)]
        public bool ShowMapButton { get => Options.Current.ShowMapButton; set { Options.Current.ShowMapButton = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL216}Quick-sell option in town menu", Order = 0, RequireRestart = false,
            HintText = "{=TL316}Shows the quick-sell entry in town and village menus. It also controls the quick-trade entry, which sells before it buys; with this off, neither appears.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public bool QuickSellMenu { get => Options.Current.QuickSellMenu; set { Options.Current.QuickSellMenu = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL217}Auto-sell on entry", Order = 0, RequireRestart = false,
            HintText = "{=TL317}Sells policy-approved goods automatically when you enter a market. Trade XP is awarded.")]
        [SettingPropertyGroup("{=TL104}Automation", GroupOrder = 4)]
        public bool AutoSellOnEntry { get => Options.Current.AutoSellOnEntry; set { Options.Current.AutoSellOnEntry = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL218}Auto-buy on entry", Order = 1, RequireRestart = false,
            HintText = "{=TL318}Buys profitable goods automatically when you enter a market, after any auto-sell. Turning this on also enables quick-buy.")]
        [SettingPropertyGroup("{=TL104}Automation", GroupOrder = 4)]
        public bool AutoBuyOnEntry
        {
            get => Options.Current.AutoBuyOnEntry;
            set
            {
                if (value == Options.Current.AutoBuyOnEntry) return;
                Options.Current.AutoBuyOnEntry = value;
                if (value && Loaded) Options.Current.EnableBuying = true;
                Options.Bump();
            }
        }

        [SettingPropertyBool("{=TL219}Auto-trade (sell, then buy)", Order = 2, RequireRestart = false,
            HintText = "{=TL319}Full automation. Reads as ON exactly when both toggles above are ON, and switches them together. Selling runs first, so profit and Trade XP are awarded before any buying. With this ON the separate quick-sell and quick-buy menu entries are hidden; quick-trade remains for manual re-runs.")]
        [SettingPropertyGroup("{=TL104}Automation", GroupOrder = 4)]
        public bool AutoTrade
        {
            get => Options.Current.AutoTradeBoth;
            set
            {
                if (!Loaded || value == Options.Current.AutoTradeBoth) return;
                Options.Current.AutoSellOnEntry = value;
                Options.Current.AutoBuyOnEntry = value;
                if (value) Options.Current.EnableBuying = true;
                Options.Bump();
            }
        }

        [SettingPropertyFloatingInteger("{=TL220}Minimum profit margin", 0f, 2f, "#0%", Order = 2, RequireRestart = false,
            HintText = "{=TL320}The margin every trade must clear, in both directions. Sell only if the price exceeds your cost basis by at least this much. Buy, or list a route, only if the far market exceeds the local price by this much after the resale safety factor. Raising it trades less.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public float MinProfitMargin { get => Options.Current.MinProfitMargin; set { Options.Current.MinProfitMargin = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL221}Keep food (days of supply)", 0, 30, Order = 3, RequireRestart = false,
            HintText = "{=TL321}Hold back this many days of food before selling any. The cheapest food per day fed is reserved first, and livestock only if nothing else covers the reserve. 0 sells every scrap of food.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public int KeepFoodDays { get => Options.Current.KeepFoodDays; set { Options.Current.KeepFoodDays = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL222}Food policy (0 ignore, 1 sell only, 2 buy only, 3 buy and sell)", 0, 3, Order = 4, RequireRestart = false,
            HintText = "{=TL322}What automated trading may do with food. The days-of-supply reserve above is separate and still applies.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public int FoodPolicy { get => Options.Current.FoodPolicy; set { Options.Current.FoodPolicy = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL223}Smithing material policy (0 ignore, 1 sell only, 2 buy only, 3 buy and sell)", 0, 3, Order = 5, RequireRestart = false,
            HintText = "{=TL323}What automated trading may do with charcoal, hardwood, iron ore and ingots. Set 0 to keep smithing stock out of automated trading entirely.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public int CraftingPolicy { get => Options.Current.CraftingPolicy; set { Options.Current.CraftingPolicy = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL224}Livestock policy (0 ignore, 1 sell only, 2 buy only, 3 buy and sell)", 0, 3, Order = 6, RequireRestart = false,
            HintText = "{=TL324}What automated trading may do with sheep, cattle and hogs. Buying is capped by the game's own herding calculation, so it will not push the party into the herd speed penalty. Mounts and pack animals are never bought or sold by policy and this setting does not affect them; only an explicit always-sell entry can move one.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public int LivestockPolicy { get => Options.Current.LivestockPolicy; set { Options.Current.LivestockPolicy = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL225}Protect unique and crafted items", Order = 7, RequireRestart = false,
            HintText = "{=TL325}Never auto-trade unique or player-crafted items. Mounts and pack animals are protected by policy regardless of this setting; only an explicit always-sell entry can move those.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public bool ProtectSpecial { get => Options.Current.ProtectSpecial; set { Options.Current.ProtectSpecial = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL226}Respect inventory locks", Order = 8, RequireRestart = false,
            HintText = "{=TL326}Locked items in the inventory screen are never auto-traded. Locks are matched as the game stores them, by item and modifier.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public bool RespectLocks { get => Options.Current.RespectLocks; set { Options.Current.RespectLocks = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL227}Cost basis mode (0 avg paid, 1 last paid, 2 cheapest known)", 0, 2, Order = 9, RequireRestart = false,
            HintText = "{=TL327}How profit is measured, and so how much Trade XP a sale earns. 0: average of what you paid. 1: last price you paid. 2: cheapest known buy price. Goods you never bought have no price you paid, so every mode values those at the cheapest market you know of; selling one below that earns no profit and no Trade XP.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public int CostBasisMode { get => Options.Current.CostBasisMode; set { Options.Current.CostBasisMode = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL228}Sell loot up to tier (0 = off)", 0, 6, Order = 10, RequireRestart = false,
            HintText = "{=TL328}Also quick-sell weapons and armor of this tier and below. Locks and protections still apply.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public int MaxLootTier { get => Options.Current.MaxLootTier; set { Options.Current.MaxLootTier = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL229}Hold cargo for the best market", Order = 11, RequireRestart = false,
            HintText = "{=TL329}Skip selling here when this market pays clearly less than the best known market. Goods you never bought have no cost basis, so the floor always applies to those; this setting controls whether it also applies to goods you bought, which the profit margin already covers.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public bool PreferBestSellTown { get => Options.Current.PreferBestSellTown; set { Options.Current.PreferBestSellTown = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL230}Best-market tolerance", 0.5f, 1f, "#0%", Order = 12, RequireRestart = false,
            HintText = "{=TL330}With the above ON: sell here anyway if this market pays at least this fraction of the best known price.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public float BestSellTownTolerance { get => Options.Current.BestSellTownTolerance; set { Options.Current.BestSellTownTolerance = value; Options.Bump(); } }

        [SettingPropertyText("{=TL231}Never sell (item ids or names, comma separated)", Order = 13, RequireRestart = false,
            HintText = "{=TL331}Goods quick-sell must never sell. Name each one either by the item id, the short internal name such as grain, wine or iron_ore that TradeLord.log prints for every good it moves, or by the name the game shows you, such as Iron Ore. Separate them with commas. An entry matching no good in this game is named in the log and said on screen, rather than passing quietly. Quick-buy leaves these alone too, since it would have no way to sell them on.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public string NeverSellItems { get => Options.Current.NeverSellItems; set { Options.Current.NeverSellItems = value; Options.Bump(); } }

        [SettingPropertyText("{=TL232}Always sell (item ids or names, comma separated)", Order = 14, RequireRestart = false,
            HintText = "{=TL332}Goods quick-sell always sells, past the category policies, the unique and crafted protection and the food reserve. Named by item id or by the name the game shows, comma separated, as above. The never-sell list above and an inventory lock still hold. This is the only way to sell a mount or a pack animal.")]
        [SettingPropertyGroup("{=TL103}Action", GroupOrder = 3)]
        public string AlwaysSellItems { get => Options.Current.AlwaysSellItems; set { Options.Current.AlwaysSellItems = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL233}Enable quick-buy", Order = 0, RequireRestart = false,
            HintText = "{=TL333}Buy goods here that sell for more elsewhere, within the caps below. Only goods actually in stock are bought.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 5)]
        public bool EnableBuying { get => Options.Current.EnableBuying; set { Options.Current.EnableBuying = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL234}Gold reserve", 0, 100000, Order = 1, RequireRestart = false,
            HintText = "{=TL334}Never spend below this much gold. Default 300, which is enough to barter your way out of two hostile encounters and still meet a wage payment after a shopping trip.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 5)]
        public int GoldReserve { get => Options.Current.GoldReserve; set { Options.Current.GoldReserve = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL235}Buy cap per item (count, 0 = off)", 0, 500, Order = 2, RequireRestart = false,
            HintText = "{=TL335}Most units of one item quick-buy takes per visit. 0 = no limit on the count. Default 32.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 5)]
        public int BuyCapPerItem { get => Options.Current.BuyCapPerItem; set { Options.Current.BuyCapPerItem = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL236}Buy cap per item (denars, 0 = off)", 0, 50000, Order = 3, RequireRestart = false,
            HintText = "{=TL336}Also cap spending per item per visit in denars.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 5)]
        public int BuyValueCapPerItem { get => Options.Current.BuyValueCapPerItem; set { Options.Current.BuyValueCapPerItem = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL237}Max spend per visit (0 = unlimited)", 0, 100000, Order = 4, RequireRestart = false,
            HintText = "{=TL337}Total denars quick-buy may spend per settlement visit. Default 1000.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 5)]
        public int MaxSpendPerVisit { get => Options.Current.MaxSpendPerVisit; set { Options.Current.MaxSpendPerVisit = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL238}Resale safety factor", 0.5f, 1f, "#0%", Order = 5, RequireRestart = false,
            HintText = "{=TL338}Assume only this fraction of the best sell price is still available by the time you arrive.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 5)]
        public float ResaleSafetyFactor { get => Options.Current.ResaleSafetyFactor; set { Options.Current.ResaleSafetyFactor = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL239}Never buy grain", Order = 6, RequireRestart = false,
            HintText = "{=TL339}Grain is heavy and low margin, so buying it fills the cargo for little return. Selling and the food reserve are unaffected. ON by default.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 5)]
        public bool NeverBuyGrain { get => Options.Current.NeverBuyGrain; set { Options.Current.NeverBuyGrain = value; Options.Bump(); } }

        [SettingPropertyText("{=TL240}Never buy (item ids or names, comma separated)", Order = 7, RequireRestart = false,
            HintText = "{=TL340}Items quick-buy must never purchase. Named by item id or by the name the game shows, comma separated, as above. Selling them is unaffected.")]
        [SettingPropertyGroup("{=TL105}Buying", GroupOrder = 5)]
        public string NeverBuyItems { get => Options.Current.NeverBuyItems; set { Options.Current.NeverBuyItems = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL241}Trade with villages", Order = 0, RequireRestart = false,
            HintText = "{=TL341}Quick-sell/quick-buy also in village menus; villages join the price scans.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 6)]
        public bool TradeWithVillages { get => Options.Current.TradeWithVillages; set { Options.Current.TradeWithVillages = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL242}Simulation mode (dry run)", Order = 1, RequireRestart = false,
            HintText = "{=TL342}Report what quick-sell and quick-buy would do, without trading. Treat the result as a best case: nothing moves, so the market does not react and every unit is priced at today's opening price. A real pass stops at the unit where the margin runs out, so it usually trades less and gets less per unit. The merchant's gold, your carry weight, the gold reserve and every per-item and per-visit cap are modelled exactly; only your own effect on the price is not.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 6)]
        public bool SimulationMode { get => Options.Current.SimulationMode; set { Options.Current.SimulationMode = value; Options.Bump(); } }

        [SettingPropertyInteger("{=TL243}Economy settling delay (days, 0 = off)", 0, 100, Order = 2, RequireRestart = false,
            HintText = "{=TL343}No TradeLord trading before this campaign day, from the menu or on entry. Prices in a new campaign have not settled yet.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 6)]
        public int EconomySettlingDays { get => Options.Current.EconomySettlingDays; set { Options.Current.EconomySettlingDays = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL244}Trade XP multiplier", 0f, 3f, "#0%", Order = 3, RequireRestart = false,
            HintText = "{=TL344}Scales the Trade XP awarded for automated profit. 0 disables XP.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 6)]
        public float TradeXpMultiplier { get => Options.Current.TradeXpMultiplier; set { Options.Current.TradeXpMultiplier = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL245}Auto-mark best sell town on map", Order = 4, RequireRestart = false,
            HintText = "{=TL345}Moves a map tracker to whichever nearby town pays most for your current cargo. Re-evaluated daily and whenever you enter or leave a settlement, so it follows what you are carrying now. Limited by the travel ceiling below. ON by default; clicking a town in the ledger panel still pins a marker by hand.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 6)]
        public bool MarkBestSellTownOnMap { get => Options.Current.MarkBestSellTownOnMap; set { Options.Current.MarkBestSellTownOnMap = value; Options.Bump(); } }

        [SettingPropertyFloatingInteger("{=TL246}Auto-marker travel ceiling (days, 0 = off)", 0f, 10f, "0.0", Order = 5, RequireRestart = false,
            HintText = "{=TL346}The auto-marker ignores towns farther than this many travel days away. Default 1.5.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 6)]
        public float MarkerMaxTravelDays { get => Options.Current.MarkerMaxTravelDays; set { Options.Current.MarkerMaxTravelDays = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL247}Coin sound on trade", Order = 6, RequireRestart = false,
            HintText = "{=TL347}Play a coin sound when a quick-sell or quick-buy pass actually moves something. A pass that trades nothing stays silent.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 6)]
        public bool CoinSound { get => Options.Current.CoinSound; set { Options.Current.CoinSound = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL248}Detailed trade summary", Order = 7, RequireRestart = false,
            HintText = "{=TL348}Name the goods in the one-line trade summary, e.g. 'TradeLord sold 8 Olives, 3 Wine for 240 denars', instead of a bare item count. The full list is always written to TradeLord.log.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 6)]
        public bool DetailedTradeSummary { get => Options.Current.DetailedTradeSummary; set { Options.Current.DetailedTradeSummary = value; Options.Bump(); } }

        [SettingPropertyBool("{=TL249}Quiet automation", Order = 8, RequireRestart = false,
            HintText = "{=TL349}Trading done automatically as you enter a market reports to TradeLord.log only, with no lines on screen. The quick-sell, quick-buy and quick-trade menu entries always report, since you asked for those. The first-run automation notice, the empty-purse warning and the cargo-full warning are unaffected.")]
        [SettingPropertyGroup("{=TL106}General", GroupOrder = 6)]
        public bool QuietAutomation { get => Options.Current.QuietAutomation; set { Options.Current.QuietAutomation = value; Options.Bump(); } }
    }
}
