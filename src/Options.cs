using System;
using System.Collections.Generic;

namespace TradeLord
{
    public sealed class ItemList
    {
        public readonly HashSet<string> Entries = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public readonly HashSet<string> Words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public bool Empty => Entries.Count == 0;

        public bool HasId(string id) => !Empty && (Entries.Contains(id) || Words.Contains(id));

        public bool HasName(string shown) => !Empty && Entries.Contains(shown);
    }

    public class Options
    {
        public static Options Current { get; } = new Options();

        public static int Generation { get; private set; }
        public static void Bump() { Generation++; }

        public int Language = 0;

        public bool Omniscient = true;
        public int ObservationShelfLifeDays = 45;

        public int CostBasisMode = 0;

        public bool ExcludeHostileTowns = true;

        public float ScanRadius = 0f;

        public int MinTownStock = 10;

        public float MaxTravelDays = 3f;
        public float MaxVillageTravelDays = 1f;

        public bool ConservativeRouteProjection = true;

        public bool BulkSimulation = true;

        public bool ConfidenceRanking = true;

        public bool TooltipHints = true;
        public bool SuppressVanillaTradeLines = true;

        public bool ProfitColoring = true;

        public bool ShowMapButton = true;

        public bool QuickSellMenu = true;

        public bool AutoSellOnEntry = true;

        public bool AutoBuyOnEntry = true;

        public bool DetailedTradeSummary = true;
        public bool QuietAutomation = false;
        public float MinProfitMargin = 0.15f;
        public int KeepFoodDays = 5;
        public const int PolicyIgnore = 0, PolicySellOnly = 1, PolicyBuyOnly = 2, PolicyBuySell = 3;

        public int FoodPolicy = PolicyBuySell;
        public int CraftingPolicy = PolicyBuySell;
        public int LivestockPolicy = PolicyBuySell;
        public bool ProtectSpecial = true;
        public bool RespectLocks = true;

        public int MaxLootTier = 1;

        public bool PreferBestSellTown = false;
        public float BestSellTownTolerance = 0.95f;

        public bool EnableBuying = true;
        public int GoldReserve = 300;

        public bool NeverBuyGrain = true;
        public int BuyCapPerItem = 32;

        public int BuyValueCapPerItem = 0;

        public int MaxHeldPerItem = 0;

        public int MaxSpendPerVisit = 1000;

        public float ResaleSafetyFactor = 0.85f;

        public string PanelKey = "T";

        public bool TradeWithVillages = true;
        public bool SimulationMode = false;
        public int EconomySettlingDays = 0;
        public float TradeXpMultiplier = 1f;

        public bool MarkBestSellTownOnMap = true;

        public float MarkerMaxTravelDays = 1.5f;
        public bool CoinSound = true;

        public string NeverSellItems = "";
        public string AlwaysSellItems = "";
        public string NeverBuyItems = "";
        public string AlwaysBuyItems = "";

        private ItemList _never, _always, _neverBuy, _alwaysBuy;
        private string _nSrc, _aSrc, _nbSrc, _abSrc;

        internal static readonly char[] EntryMarks = { ',', ';' };
        internal static readonly char[] WordMarks = { ' ', '\t' };

        private static ItemList Parsed(string src, ref string seen, ref ItemList set)
        {
            if (set == null || seen != src)
            {
                var built = new ItemList();
                foreach (string entry in (src ?? "").Split(EntryMarks, StringSplitOptions.RemoveEmptyEntries))
                {
                    string whole = entry.Trim();
                    if (whole.Length == 0) continue;
                    built.Entries.Add(whole);
                    foreach (string word in whole.Split(WordMarks, StringSplitOptions.RemoveEmptyEntries))
                        built.Words.Add(word);
                }
                set = built;
                seen = src;
            }
            return set;
        }

        public ItemList NeverSet => Parsed(NeverSellItems, ref _nSrc, ref _never);
        public ItemList AlwaysSet => Parsed(AlwaysSellItems, ref _aSrc, ref _always);
        public ItemList NeverBuySet => Parsed(NeverBuyItems, ref _nbSrc, ref _neverBuy);
        public ItemList AlwaysBuySet => Parsed(AlwaysBuyItems, ref _abSrc, ref _alwaysBuy);
    }

}
