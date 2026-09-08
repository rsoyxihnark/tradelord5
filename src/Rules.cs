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

        internal static Block WhatStopsBuying(in Good good, int price, int budget,
                                              (int count, int spent) taken, int held, float shareCap,
                                              bool livestock, int herdRoom, bool lastInVillage, Options s)
        {
            if (price > budget) return Block.BudgetSpent;
            if (s.BuyCapPerItem > 0 && taken.count >= s.BuyCapPerItem) return Block.ItemCountCap;
            if (s.BuyValueCapPerItem > 0 && taken.spent + price > s.BuyValueCapPerItem) return Block.ItemValueCap;
            if (s.MaxHeldPerItem > 0 && held >= s.MaxHeldPerItem) return Block.HeldEnough;
            if (shareCap > 0f && (held + 1) * good.Weight > shareCap) return Block.HeldEnough;
            if (livestock && herdRoom <= 0) return Block.HerdFull;
            if (lastInVillage) return Block.VillageLastUnit;
            return Block.None;
        }

        internal static bool NoRoomForOneMore(in Good good, float roomLeft) =>
            good.Weight > 0.01f && good.Weight > roomLeft;

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
