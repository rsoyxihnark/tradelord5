using System;
using System.Collections.Generic;

namespace TradeLord
{
    internal enum Block
    {
        None, NotMerchandise, NeverList, Locked, CategoryPolicy, Protected,
        MountOrHaulAnimal, NotTradable, FoodReserve, TradedHereAlready, NoStock,
        NoResaleMarket, BelowMargin, BelowBestMarket, MerchantTillEmpty, BudgetSpent,
        ItemCountCap, ItemValueCap, CarryWeight, HerdFull, VillageLastUnit, HeldEnough, Smeltable,
        QuestGoods, GrainSwitch, BuyerTillEmpty
    }

    internal struct Good
    {
        internal string Id;
        internal string Name;
        internal float Weight;
        internal int Value;
        internal int Tier;
        internal bool NotMerchandise;
        internal bool HasHorse;
        internal bool IsUnique;
        internal bool IsCraftedByPlayer;
        internal bool IsTradeGood;
        internal bool IsFood;
        internal bool IsAnimal;
        internal bool IsMountable;
        internal bool IsHaulAnimal;
        internal bool IsSpareMount;
        internal bool IsLivestock;
        internal bool IsPrizeMount;
        internal bool IsSmithingMaterial;
        internal bool IsGrain;
    }

    internal interface IWhatTheGameSays
    {
        bool Locked();
        bool Smeltable();
        bool PartsAllLearned();
    }

    internal struct SellFacts
    {
        internal bool QuestItem;
        internal int AwaitedHeld;
        internal int FoodHeld;
        internal bool QuestsReadable;
    }

    internal struct SellVerdict
    {
        internal bool Allowed;
        internal Block Why;
        internal int KeepCount;
        internal int DrewAwaited;
        internal int DrewFood;
    }

    internal struct Stamp
    {
        internal const int Timeless = int.MinValue;

        private int _hour;
        private int _generation;
        private bool _taken;

        internal bool Fresh(int hour, int generation) =>
            _taken && _hour == hour && _generation == generation;

        internal void Taken(int hour, int generation)
        {
            _taken = true;
            _hour = hour;
            _generation = generation;
        }

        internal void Stale()
        {
            _taken = false;
            _hour = 0;
            _generation = 0;
        }
    }

    internal static class Arrivals
    {
        internal const float SetOffFromTheGate = 1f;

        internal static bool StillTheSame(string here, string lastArrivalAt, bool tookToTheRoad) =>
            here != null && here == lastArrivalAt && !tookToTheRoad;

        internal static bool TakenToTheRoad(bool already, bool gateKnown,
                                            float squaredFromTheGate) =>
            already || (gateKnown && squaredFromTheGate > SetOffFromTheGate);

        internal const double StraightBackWithin = 1d;

        internal static bool StraightBack(double hours, double lastHours) =>
            hours >= lastHours && hours - lastHours < StraightBackWithin;

        internal static bool StillTheSameSitting(string here, string sittingAt,
                                                 double hours, double sittingHours) =>
            here != null && here == sittingAt && StraightBack(hours, sittingHours);
    }

    internal static class Marks
    {
        internal static bool OnlyEatenFrom(List<(string good, int amount, bool food)> then,
                                           List<(string good, int amount, bool food)> now)
        {
            if (then == null || now == null) return false;
            var had = new Dictionary<string, (int amount, bool food)>(StringComparer.Ordinal);
            foreach (var (good, amount, food) in then)
            {
                had.TryGetValue(good, out var was);
                had[good] = (was.amount + amount, food);
            }
            var has = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var (good, amount, _) in now)
            {
                has.TryGetValue(good, out int was);
                has[good] = was + amount;
            }
            foreach (var one in has)
            {
                if (!had.TryGetValue(one.Key, out var was)) return false;
                if (was.food ? one.Value > was.amount : one.Value != was.amount) return false;
            }
            foreach (var one in had)
                if (!one.Value.food && !has.ContainsKey(one.Key)) return false;
            return true;
        }

        internal static bool FirstLookStands(string markedAt, string lookingAt,
                                             List<(string good, int amount, bool food)> markedCargo,
                                             List<(string good, int amount, bool food)> cargoNow,
                                             long markedValue) =>
            markedValue > 0L && markedAt != null && markedAt == lookingAt &&
            OnlyEatenFrom(markedCargo, cargoNow);
    }

    internal static class MapButton
    {
        internal const float Pad = 6f;

        internal static bool BoundsReadable(float screenW, float screenH, float width, float height) =>
            screenW >= 1f && screenH >= 1f && width >= 1f && height >= 1f &&
            width <= screenW && height <= screenH;

        internal static bool Over(float x, float y, float screenW, float screenH,
                                  float width, float height, float marginRight)
        {
            if (!BoundsReadable(screenW, screenH, width, height)) return false;
            float padX = Pad / screenW, padY = Pad / screenH;
            float right = 1f - marginRight / screenW;
            float left = right - width / screenW;
            float half = height / screenH * 0.5f;
            return x >= left - padX && x <= right + padX &&
                   y >= 0.5f - half - padY && y <= 0.5f + half + padY;
        }

        internal static bool TakesTheMouse(bool windowOpen, bool buttonOn, bool overButton) =>
            windowOpen || (buttonOn && overButton);
    }

    internal static class Ranks
    {
        internal const int Bands = 5;

        internal static float Of(int at, int count) =>
            count <= 1 || at < 0 ? 0f : (float)at / (count - 1);

        internal static int BandOf(float rank) =>
            rank < 0.2f ? 1 : rank < 0.45f ? 2 : rank < 0.7f ? 3 : rank < 0.85f ? 4 : Bands;
    }

    internal static class Settling
    {
        internal static bool StillHolding(int waitDays, float elapsedDays, out int daysLeft)
        {
            daysLeft = 0;
            if (waitDays <= 0) return false;
            float elapsed = TradeMath.Finite(elapsedDays, float.MaxValue);
            if (elapsed >= waitDays) return false;
            daysLeft = (int)Math.Ceiling(waitDays - (elapsed < 0f ? 0f : elapsed));
            if (daysLeft < 1) daysLeft = 1;
            if (daysLeft > waitDays) daysLeft = waitDays;
            return true;
        }
    }

    internal static class Herding
    {
        internal const int Cushion = 2;

        internal static int MountsNobodyRides(int mounts, int menOnFoot) =>
            Math.Max(0, (mounts < 0 ? 0 : mounts) - (menOnFoot < 0 ? 0 : menOnFoot));

        internal static int DrivenInAll(int herd, int mounts, int menOnFoot) =>
            (herd < 0 ? 0 : herd) + MountsNobodyRides(mounts, menOnFoot);

        internal static int HaulAnimalsToSpare(int held, int packAnimals, float packCapacity,
                                               float addedUp, float capacity, float carried)
        {
            if (held <= 0) return 0;
            float room = TradeMath.Finite(capacity - carried, 0f);
            if (room <= 0f) return 0;
            if (packAnimals <= 0 || packCapacity <= 0f) return held;
            float each = packCapacity / packAnimals;
            if (addedUp > 0f && capacity > addedUp) each *= capacity / addedUp;
            double spare = Math.Floor(room / each);
            if (double.IsNaN(spare) || spare <= 0d) return 0;
            return spare >= held ? held : (int)spare;
        }
    }

    internal static class Recent
    {
        internal const int MostKept = 20;

        internal static void Keep<TRecord>(IList<TRecord> held, TRecord one, int most)
        {
            if (held == null || most <= 0) return;
            held.Insert(0, one);
            while (held.Count > most) held.RemoveAt(held.Count - 1);
        }

        internal static string Coins(int gold) => gold > 0 ? "+" + gold : gold.ToString();

        internal static int DaysAgo(float then, float now)
        {
            float gap = TradeMath.Finite(now - then, 0f);
            if (gap <= 0f) return 0;
            return (int)gap;
        }
    }

    internal static class Screens
    {
        internal const string Family = "MCMv";

        internal static string Named(int generation) => Family + generation;

        internal static string GenerationOf(string assemblyName)
        {
            string name = assemblyName ?? "";
            if (!name.StartsWith(Family, StringComparison.OrdinalIgnoreCase)) return null;
            int end = Family.Length;
            while (end < name.Length && char.IsDigit(name[end])) end++;
            return end > Family.Length ? name.Substring(0, end) : null;
        }

        internal static string Which(IEnumerable<string> loaded, string wanted)
        {
            if (loaded == null) return null;
            string other = null;
            foreach (string assemblyName in loaded)
            {
                string generation = GenerationOf(assemblyName);
                if (generation == null) continue;
                if (string.Equals(generation, wanted, StringComparison.OrdinalIgnoreCase))
                    return generation;
                if (other == null) other = generation;
            }
            return other;
        }
    }

    internal static class Tallies
    {
        internal static string Of(int applied, IList<string> refused)
        {
            int turned = refused == null ? 0 : refused.Count;
            if (applied < 0) applied = 0;
            string said = "patches " + applied + "/" + (applied + turned) + " applied";
            return turned == 0 ? said : said + ", " + string.Join(", ", ToArray(refused)) + " refused";
        }

        private static string[] ToArray(IList<string> refused)
        {
            var named = new string[refused.Count];
            for (int i = 0; i < refused.Count; i++) named[i] = refused[i] ?? "";
            return named;
        }
    }

    internal static class TradeRules
    {
        internal static bool Listed(ItemList list, in Good good) =>
            !list.Empty &&
            (list.HasId(good.Id) || (good.Name != null && list.HasName(good.Name)));

        internal static int PolicyFor(in Good good, Options s)
        {
            if (good.IsSmithingMaterial) return s.CraftingPolicy;
            if (good.HasHorse) return s.LivestockPolicy;
            if (good.IsFood) return s.FoodPolicy;
            return Options.PolicyBuySell;
        }

        internal static int DrawKeepBack(int available, int held, out bool any)
        {
            any = held > 0;
            return any ? Math.Min(available, held) : 0;
        }

        internal const int RankLivestock = 0;
        internal const int RankPlainMount = 1;
        internal const int RankHaulAnimal = 2;
        internal const int RankPrizeMount = 3;
        internal const int RankNotAnAnimal = -1;

        internal struct Ration
        {
            internal Good Good;
            internal int Amount;
        }

        internal static int FoodValue(in Good good)
        {
            if (good.Id == null || good.HasHorse) return 0;
            return good.IsFood ? 1 : 0;
        }

        internal static bool IsStorableFood(in Good good) =>
            good.Id != null && good.IsFood && !good.HasHorse;

        internal static int StillCarried(IDictionary<string, int> soldAlready, string id, int amount)
        {
            if (soldAlready == null || !soldAlready.TryGetValue(id, out int gone) || gone <= 0)
                return amount;
            int taken = Math.Min(gone, amount);
            soldAlready[id] = gone - taken;
            return amount - taken;
        }

        private static float CostPerFood(in Ration held) =>
            (float)held.Good.Value / FoodValue(held.Good);

        internal static Dictionary<string, int> FoodKeep(List<Ration> carried, float perDay, Options s)
        {
            var keep = new Dictionary<string, int>(StringComparer.Ordinal);
            int variety = s.KeepEveryFoodKind ? s.KeepPerFoodKind : 0;
            if (s.KeepFoodDays <= 0 || carried == null) return keep;
            if (perDay < 1f) perDay = 1f;
            int reserve = (int)Math.Ceiling(perDay * s.KeepFoodDays);

            var food = new List<Ration>();
            var at = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < carried.Count; i++)
            {
                Ration held = carried[i];
                if (held.Amount <= 0 || FoodValue(held.Good) <= 0 ||
                    Listed(s.AlwaysSet, held.Good)) continue;
                if (at.TryGetValue(held.Good.Id, out int seen))
                {
                    Ration had = food[seen];
                    had.Amount += held.Amount;
                    food[seen] = had;
                    continue;
                }
                at[held.Good.Id] = food.Count;
                food.Add(held);
            }

            food.Sort((x, y) => CostPerFood(x).CompareTo(CostPerFood(y)));

            if (variety > 0)
                foreach (Ration held in food)
                {
                    int floor = Math.Min(held.Amount, variety);
                    keep.TryGetValue(held.Good.Id, out int had);
                    if (floor <= had) continue;
                    reserve -= (floor - had) * FoodValue(held.Good);
                    keep[held.Good.Id] = floor;
                }

            foreach (Ration held in food)
            {
                if (reserve <= 0) break;
                int perUnit = FoodValue(held.Good);
                keep.TryGetValue(held.Good.Id, out int had);
                if (had >= held.Amount) continue;
                int take = Math.Min(held.Amount - had, (reserve + perUnit - 1) / perUnit);
                reserve -= take * perUnit;
                keep[held.Good.Id] = had + take;
            }
            return keep;
        }

        internal static Dictionary<string, int> LivestockKeep(List<Ration> carried, int wanted)
        {
            var keep = new Dictionary<string, int>(StringComparer.Ordinal);
            if (carried == null) return keep;
            for (int i = 0; i < carried.Count && wanted > 0; i++)
            {
                Ration held = carried[i];
                if (held.Amount <= 0 || !held.Good.IsLivestock) continue;
                int take = Math.Min(held.Amount, wanted);
                keep.TryGetValue(held.Good.Id, out int had);
                keep[held.Good.Id] = had + take;
                wanted -= take;
            }
            return keep;
        }

        internal static int HerdShedRank(in Good good)
        {
            if (good.IsLivestock) return RankLivestock;
            if (good.IsSpareMount) return good.IsPrizeMount ? RankPrizeMount : RankPlainMount;
            if (good.IsHaulAnimal) return RankHaulAnimal;
            return RankNotAnAnimal;
        }

        internal static string AnimalGroup(in Good good)
        {
            if (!good.HasHorse) return null;
            if (good.IsHaulAnimal) return "a haul animal";
            if (good.IsSpareMount) return "a mount";
            if (good.IsLivestock) return "livestock";
            return "an animal TradeLord treats as ordinary cargo";
        }

        internal static bool InputsHeld(IList<(string category, int needed)> inputs,
                                       IDictionary<string, int> held)
        {
            if (inputs == null || inputs.Count == 0) return false;
            for (int i = 0; i < inputs.Count; i++)
            {
                var (category, needed) = inputs[i];
                if (category == null) return false;
                held.TryGetValue(category, out int have);
                if (have < needed) return false;
            }
            return true;
        }

        internal static int RunsSoonest(IList<(bool ready, float progress)> productions)
        {
            int pick = -1;
            float furthest = -1f;
            if (productions == null) return -1;
            for (int i = 0; i < productions.Count; i++)
            {
                var (ready, progress) = productions[i];
                if (!ready || progress <= furthest) continue;
                furthest = progress;
                pick = i;
            }
            return pick;
        }

        internal static bool StagesTheDeal(Options s) =>
            s != null && s.StagedTrading && !s.SimulationMode;

        internal static bool ResaleAllowed(in Good good, Options s) =>
            Listed(s.AlwaysSet, good) || TradeMath.PolicyAllows(PolicyFor(good, s), buying: false);

        internal static bool MayBuy<TGame>(in Good good, bool toFeed, Options s, TGame game,
                                           out Block why)
            where TGame : struct, IWhatTheGameSays
        {
            why = Block.None;
            if (good.Id == null || good.NotMerchandise) { why = Block.NotMerchandise; return false; }
            if (Listed(s.NeverSet, good) || Listed(s.NeverBuySet, good))
            { why = Block.NeverList; return false; }
            if (game.Locked()) { why = Block.Locked; return false; }

            bool always = Listed(s.AlwaysBuySet, good);
            if (!always && !toFeed && s.NeverBuyGrain && good.IsGrain)
            { why = Block.GrainSwitch; return false; }
            if (!always && !toFeed && !TradeMath.PolicyAllows(PolicyFor(good, s), buying: true))
            { why = Block.CategoryPolicy; return false; }
            if (good.HasHorse)
            {
                if (good.IsLivestock) return true;
                why = Block.MountOrHaulAnimal;
                return false;
            }
            if (good.IsTradeGood) return true;
            why = Block.NotTradable;
            return false;
        }

        internal static bool MayHaul<TGame>(in Good good, Options s, TGame game)
            where TGame : struct, IWhatTheGameSays
        {
            if (good.Id == null || !good.IsHaulAnimal || good.NotMerchandise) return false;
            if (Listed(s.NeverSet, good) || Listed(s.NeverBuySet, good)) return false;
            return !game.Locked();
        }

        internal static bool MayShedForHerd<TGame>(in Good good, bool questItem, Options s, TGame game)
            where TGame : struct, IWhatTheGameSays
        {
            if (good.Id == null || !good.HasHorse || good.NotMerchandise || questItem) return false;
            if (Listed(s.NeverSet, good)) return false;
            if (s.ProtectSpecial && (good.IsUnique || good.IsCraftedByPlayer)) return false;
            return !game.Locked();
        }

        internal static Block WhatCapsAGood(in Good good, int price,
                                            (int count, int spent) taken, int held, float shareCap,
                                            Options s)
        {
            if (s.BuyCapPerItem > 0 && taken.count >= s.BuyCapPerItem) return Block.ItemCountCap;
            if (s.BuyValueCapPerItem > 0 && taken.spent + price > s.BuyValueCapPerItem) return Block.ItemValueCap;
            if (s.MaxHeldPerItem > 0 && held >= s.MaxHeldPerItem) return Block.HeldEnough;
            if (shareCap > 0f && (held + 1) * good.Weight > shareCap) return Block.HeldEnough;
            return Block.None;
        }

        internal static Block WhatCapsALot(in Good good, int cost, int units,
                                           (int count, int spent) taken, int held, float shareCap,
                                           Options s)
        {
            if (s.BuyCapPerItem > 0 && taken.count + units > s.BuyCapPerItem) return Block.ItemCountCap;
            if (s.BuyValueCapPerItem > 0 && (long)taken.spent + cost > s.BuyValueCapPerItem) return Block.ItemValueCap;
            if (s.MaxHeldPerItem > 0 && held + units > s.MaxHeldPerItem) return Block.HeldEnough;
            if (shareCap > 0f && (held + units) * good.Weight > shareCap) return Block.HeldEnough;
            return Block.None;
        }

        internal static Block WhatStopsBuying(in Good good, int price, int budget,
                                              (int count, int spent) taken, int held, float shareCap,
                                              bool livestock, int herdRoom, bool lastInVillage, Options s)
        {
            if (price > budget) return Block.BudgetSpent;
            Block capped = WhatCapsAGood(good, price, taken, held, shareCap, s);
            if (capped != Block.None) return capped;
            if (livestock && herdRoom <= 0) return Block.HerdFull;
            if (lastInVillage) return Block.VillageLastUnit;
            return Block.None;
        }

        internal static bool NoRoomForOneMore(in Good good, float roomLeft) =>
            good.Weight > 0.01f && good.Weight > roomLeft;

        internal static bool TradedAsMerchandise(in Good good) =>
            good.IsTradeGood || good.IsLivestock;

        internal static int BestMarketFloor(int elsewhere, float tolerance) =>
            (int)(elsewhere * tolerance);

        internal static bool BelowTheBestMarket(int price, int holdFloor) => price < holdFloor;

        internal const int VillageLastCoin = 1;

        internal static int WhatTheTillCanPay(int till, bool village) =>
            Math.Max(0, village ? till - VillageLastCoin : till);

        internal static bool TheBuyerCouldNotPay(int drawnSoFar, int till) =>
            till > 0 && drawnSoFar > till;

        internal static int WhatTheBuyerPays(int drawnBefore, int drawnAfter, int till)
        {
            int reach = till > 0 && drawnAfter > till ? till : drawnAfter;
            return Math.Max(0, reach - drawnBefore);
        }

        internal const int VillagePurse = 1000;

        internal static int PutBackIntoAnEmptyPurse(int gold) =>
            gold <= 0 ? VillagePurse - gold : 0;

        internal static bool WorthIsWhatYouPaid(in Good good, int paid) =>
            paid > 0 || !TradedAsMerchandise(good);

        internal static int WorthToBeat(in Good good, int paid, int unpaidWorth) =>
            WorthIsWhatYouPaid(good, paid) ? paid : unpaidWorth;

        internal static SellVerdict MaySell<TGame>(in Good good, int amount, in SellFacts facts,
                                                   Options s, TGame game)
            where TGame : struct, IWhatTheGameSays
        {
            SellVerdict said = default(SellVerdict);

            if (good.Id == null || good.NotMerchandise || facts.QuestItem)
            { said.Why = Block.NotMerchandise; return said; }
            if (Listed(s.NeverSet, good)) { said.Why = Block.NeverList; return said; }

            if (game.Locked()) { said.Why = Block.Locked; return said; }
            int promised = DrawKeepBack(amount, facts.AwaitedHeld, out bool owed);
            if (owed)
            {
                said.DrewAwaited = promised;
                said.KeepCount = promised;
                if (amount <= said.KeepCount) { said.Why = Block.QuestGoods; return said; }
            }
            if (Listed(s.AlwaysSet, good)) { said.Allowed = true; return said; }
            if (!TradeMath.PolicyAllows(PolicyFor(good, s), buying: false))
            { said.Why = Block.CategoryPolicy; return said; }

            bool livestock = good.HasHorse;
            if (livestock && (good.IsHaulAnimal || good.IsSpareMount))
            { said.Why = Block.MountOrHaulAnimal; return said; }
            if (livestock && !facts.QuestsReadable) { said.Why = Block.QuestGoods; return said; }
            if (s.ProtectSpecial && (good.IsUnique || good.IsCraftedByPlayer))
            { said.Why = Block.Protected; return said; }
            if (!livestock && s.KeepSmeltableWeapons != Options.SmeltSellThem && game.Smeltable() &&
                (s.KeepSmeltableWeapons == Options.SmeltKeepAll || !game.PartsAllLearned()))
            { said.Why = Block.Smeltable; return said; }

            bool sellable = livestock || good.IsTradeGood ||
                (s.MaxLootTier > 0 && !good.IsFood && !good.IsAnimal && !good.IsMountable &&
                 good.Tier + 1 <= s.MaxLootTier);
            if (!sellable) { said.Why = Block.NotTradable; return said; }

            int reserved = DrawKeepBack(amount - said.KeepCount, facts.FoodHeld, out bool fed);
            if (fed)
            {
                said.DrewFood = reserved;
                said.KeepCount += reserved;
                if (amount <= said.KeepCount) { said.Why = Block.FoodReserve; return said; }
            }

            said.Allowed = true;
            return said;
        }
    }

    public static class Deals
    {
        public const int Slack = 2;

        public static int UnitsMoved(int amount, int gold, int price)
        {
            if (amount > 0) return amount;
            if (price > 0 && gold > 0) return Math.Max(1, gold / price);
            return gold > 0 ? 1 : 0;
        }

        public static bool AddsUp(int reckonedNet, int purseMoved) =>
            Math.Abs((long)reckonedNet - purseMoved) <= Slack + Math.Abs((long)purseMoved) / 100L;

        public static int NoMoreThanTheSale(int profit, int gained)
        {
            if (profit <= 0 || gained <= 0) return 0;
            return profit > gained ? gained : profit;
        }

        public static int PaidForWhatYouKept(int gold, int bought, int kept)
        {
            if (gold <= 0 || bought <= 0 || kept <= 0) return 0;
            if (kept >= bought) return gold;
            return (int)((long)gold * kept / bought);
        }
    }

    public static class Holdings
    {
        public static int WorkshopsYouMayOwn(int gameSays, int youAsked)
        {
            if (youAsked <= 0) return gameSays;
            return youAsked;
        }

        public static bool RoomForOneMore(int owned, int mayOwn) => owned < mayOwn;

        public static int StillOwedForTheWorkshop(int cost, int paid) =>
            cost <= 0 || paid > 0 ? 0 : cost;

        public static bool TheGameIsAskingAboutYou(int askedAboutTier, int yourTier,
                                                   bool whileYouBuy) =>
            whileYouBuy && yourTier >= 0 && askedAboutTier == yourTier;

        public static bool DipsIntoWhatYouHoldBack(int cost, int purse, int heldBack) =>
            cost > 0 && heldBack > 0 && purse - cost < heldBack;
    }
}
