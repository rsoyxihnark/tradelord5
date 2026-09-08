using System;

namespace TradeLord
{
    internal enum Block
    {
        None, NotMerchandise, NeverList, Locked, CategoryPolicy, Protected,
        MountOrHaulAnimal, NotTradable, FoodReserve, TradedHereAlready, NoStock,
        NoResaleMarket, BelowMargin, BelowBestMarket, MerchantTillEmpty, BudgetSpent,
        ItemCountCap, ItemValueCap, CarryWeight, HerdFull, VillageLastUnit, HeldEnough, Smeltable,
        QuestAnimal, GrainSwitch
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
        internal bool IsSmithingMaterial;
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

        internal static SellVerdict MaySell<TGame>(in Good good, int amount, in SellFacts facts,
                                                   Options s, TGame game)
            where TGame : struct, IWhatTheGameSays
        {
            SellVerdict said = default(SellVerdict);

            if (good.Id == null || good.NotMerchandise || facts.QuestItem)
            { said.Why = Block.NotMerchandise; return said; }
            if (Listed(s.NeverSet, good)) { said.Why = Block.NeverList; return said; }

            if (game.Locked()) { said.Why = Block.Locked; return said; }
            if (good.HasHorse)
            {
                int promised = DrawKeepBack(amount, facts.AwaitedHeld, out bool owed);
                if (owed)
                {
                    said.DrewAwaited = promised;
                    said.KeepCount = promised;
                    if (amount <= said.KeepCount) { said.Why = Block.QuestAnimal; return said; }
                }
            }
            if (Listed(s.AlwaysSet, good)) { said.Allowed = true; return said; }
            if (!TradeMath.PolicyAllows(PolicyFor(good, s), buying: false))
            { said.Why = Block.CategoryPolicy; return said; }

            bool livestock = good.HasHorse;
            if (livestock && (good.IsHaulAnimal || good.IsSpareMount))
            { said.Why = Block.MountOrHaulAnimal; return said; }
            if (livestock && !facts.QuestsReadable) { said.Why = Block.QuestAnimal; return said; }
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
                said.KeepCount = reserved;
                if (amount <= said.KeepCount) { said.Why = Block.FoodReserve; return said; }
            }

            said.Allowed = true;
            return said;
        }
    }
}
