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
        QuestGoods, GrainSwitch
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
            if (!always && !TradeMath.PolicyAllows(PolicyFor(good, s), buying: true))
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

        internal static int WorthToBeat(in Good good, int paid, int unpaidWorth) =>
            paid > 0 || !TradedAsMerchandise(good) ? paid : unpaidWorth;

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

            int reserved = DrawKeepBack(amount, facts.FoodHeld, out bool fed);
            if (fed)
            {
                said.DrewFood = reserved;
                if (reserved > said.KeepCount) said.KeepCount = reserved;
                if (amount <= said.KeepCount) { said.Why = Block.FoodReserve; return said; }
            }

            said.Allowed = true;
            return said;
        }
    }
}
