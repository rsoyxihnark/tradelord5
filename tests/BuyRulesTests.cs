using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class BuyRulesTests
    {
        private struct Says : IWhatTheGameSays
        {
            internal bool IsLocked;

            public bool Locked() => IsLocked;

            public bool Smeltable() => false;

            public bool PartsAllLearned() => true;
        }

        private static Good Cargo(string id = "iron") =>
            new Good { Id = id, Name = id, IsTradeGood = true, Weight = 2f, Value = 40 };

        private static Good Grain() =>
            new Good { Id = "grain", Name = "Grain", IsTradeGood = true, IsFood = true, IsGrain = true, Weight = 1f };

        private static Good Livestock(string id = "cow") =>
            new Good { Id = id, Name = id, HasHorse = true, IsLivestock = true, IsAnimal = true };

        private static Good Haul(string id = "mule") =>
            new Good { Id = id, Name = id, HasHorse = true, IsHaulAnimal = true };

        private static bool Buy(Good good, Options s = null, bool toFeed = false,
                                Says says = default(Says)) =>
            TradeRules.MayBuy(good, toFeed, s ?? new Options(), says, out _);

        private static Block WhyNot(Good good, Options s = null, bool toFeed = false,
                                    Says says = default(Says))
        {
            TradeRules.MayBuy(good, toFeed, s ?? new Options(), says, out Block why);
            return why;
        }

        [Fact]
        public void A_plain_trade_good_and_livestock_are_bought()
        {
            Assert.True(Buy(Cargo()));
            Assert.True(Buy(Livestock()));
        }

        [Fact]
        public void Nothing_the_game_refuses_to_trade_is_bought()
        {
            Assert.Equal(Block.NotMerchandise, WhyNot(default(Good)));
            Assert.Equal(Block.NotMerchandise, WhyNot(new Good { Id = "x", NotMerchandise = true }));
        }

        [Fact]
        public void Either_never_list_stops_a_purchase()
        {
            Assert.Equal(Block.NeverList, WhyNot(Cargo("iron"), new Options { NeverSellItems = "iron" }));
            Assert.Equal(Block.NeverList, WhyNot(Cargo("iron"), new Options { NeverBuyItems = "iron" }));
        }

        [Fact]
        public void An_inventory_lock_stops_a_purchase()
        {
            Assert.Equal(Block.Locked, WhyNot(Cargo(), says: new Says { IsLocked = true }));
        }

        [Fact]
        public void Never_buy_grain_names_grain_as_the_reason_and_yields_to_the_larder()
        {
            Assert.Equal(Block.GrainSwitch, WhyNot(Grain()));
            Assert.True(Buy(Grain(), toFeed: true));
            Assert.True(Buy(Grain(), new Options { NeverBuyGrain = false }));
            Assert.True(Buy(Grain(), new Options { AlwaysBuyItems = "grain" }));
            Assert.True(Buy(Cargo(), new Options { NeverBuyGrain = true }));
        }

        [Fact]
        public void The_always_buy_list_clears_a_category_policy_but_never_a_never_list_or_a_lock()
        {
            var leftAlone = new Options { LivestockPolicy = Options.PolicyIgnore };
            Assert.Equal(Block.CategoryPolicy, WhyNot(Livestock(), leftAlone));

            leftAlone.AlwaysBuyItems = "cow";
            Assert.True(Buy(Livestock(), leftAlone));

            var never = new Options { AlwaysBuyItems = "cow", NeverBuyItems = "cow" };
            Assert.Equal(Block.NeverList, WhyNot(Livestock(), never));

            var locked = new Options { AlwaysBuyItems = "cow" };
            Assert.Equal(Block.Locked, WhyNot(Livestock(), locked, says: new Says { IsLocked = true }));
        }

        [Fact]
        public void A_category_set_to_sell_only_is_never_bought_from()
        {
            var ore = new Good { Id = "ore", Name = "ore", IsTradeGood = true, IsSmithingMaterial = true };
            Assert.Equal(Block.CategoryPolicy,
                WhyNot(ore, new Options { CraftingPolicy = Options.PolicySellOnly }));
            Assert.True(Buy(ore, new Options { CraftingPolicy = Options.PolicyBuySell }));

            Assert.True(Buy(Cargo("ore"), new Options { CraftingPolicy = Options.PolicySellOnly }));
        }

        [Fact]
        public void Only_livestock_is_bought_as_an_animal()
        {
            Assert.Equal(Block.MountOrHaulAnimal, WhyNot(Haul()));
            Assert.Equal(Block.MountOrHaulAnimal,
                WhyNot(new Good { Id = "horse", Name = "horse", HasHorse = true, IsSpareMount = true }));
            Assert.True(Buy(Livestock()));
        }

        [Fact]
        public void Something_that_is_no_trade_good_at_all_is_not_bought()
        {
            Assert.Equal(Block.NotTradable, WhyNot(new Good { Id = "helmet", Name = "helmet", Tier = 2 }));
        }

        [Fact]
        public void A_haul_animal_is_bought_for_the_baggage_train_and_nothing_else_is()
        {
            var s = new Options();
            Assert.True(TradeRules.MayHaul(Haul(), s, default(Says)));
            Assert.False(TradeRules.MayHaul(Livestock(), s, default(Says)));
            Assert.False(TradeRules.MayHaul(Cargo(), s, default(Says)));
            Assert.False(TradeRules.MayHaul(default(Good), s, default(Says)));

            Assert.False(TradeRules.MayHaul(Haul(), new Options { NeverSellItems = "mule" }, default(Says)));
            Assert.False(TradeRules.MayHaul(Haul(), new Options { NeverBuyItems = "mule" }, default(Says)));
            Assert.False(TradeRules.MayHaul(Haul(), s, new Says { IsLocked = true }));
        }

        [Fact]
        public void Only_an_animal_you_are_free_to_move_is_shed_to_get_back_up_to_speed()
        {
            var s = new Options();
            Assert.True(TradeRules.MayShedForHerd(Livestock(), false, s, default(Says)));
            Assert.True(TradeRules.MayShedForHerd(Haul(), false, s, default(Says)));
            Assert.False(TradeRules.MayShedForHerd(Cargo(), false, s, default(Says)));
            Assert.False(TradeRules.MayShedForHerd(Livestock(), true, s, default(Says)));
            Assert.False(TradeRules.MayShedForHerd(Livestock(), false, s, new Says { IsLocked = true }));
            Assert.False(TradeRules.MayShedForHerd(
                Livestock(), false, new Options { NeverSellItems = "cow" }, default(Says)));

            var prize = new Good { Id = "war", Name = "war", HasHorse = true, IsUnique = true };
            Assert.False(TradeRules.MayShedForHerd(prize, false, s, default(Says)));
            Assert.True(TradeRules.MayShedForHerd(
                prize, false, new Options { ProtectSpecial = false }, default(Says)));
        }

        [Fact]
        public void A_good_is_only_bought_where_your_own_rules_would_let_it_go_again()
        {
            Assert.True(TradeRules.ResaleAllowed(Cargo(), new Options()));
            Assert.False(TradeRules.ResaleAllowed(
                Livestock(), new Options { LivestockPolicy = Options.PolicyBuyOnly }));
            Assert.True(TradeRules.ResaleAllowed(
                Livestock(), new Options { LivestockPolicy = Options.PolicyBuyOnly, AlwaysSellItems = "cow" }));
        }

        private static Block Capped(Good good, int price = 10, int budget = 1000,
                                    int count = 0, int spent = 0, int held = 0, float shareCap = 0f,
                                    bool livestock = false, int herdRoom = 10,
                                    bool lastInVillage = false, Options s = null) =>
            TradeRules.WhatStopsBuying(good, price, budget, (count, spent), held, shareCap,
                                       livestock, herdRoom, lastInVillage, s ?? new Options());

        [Fact]
        public void Every_cap_on_buying_reports_itself_and_the_purse_is_asked_first()
        {
            Good good = Cargo();
            Assert.Equal(Block.None, Capped(good));
            Assert.Equal(Block.BudgetSpent, Capped(good, price: 50, budget: 10));

            Assert.Equal(Block.ItemCountCap,
                Capped(good, count: 32, s: new Options { BuyCapPerItem = 32 }));
            Assert.Equal(Block.ItemValueCap,
                Capped(good, price: 10, spent: 95, s: new Options { BuyValueCapPerItem = 100 }));
            Assert.Equal(Block.HeldEnough,
                Capped(good, held: 5, s: new Options { MaxHeldPerItem = 5, BuyCapPerItem = 0 }));
            Assert.Equal(Block.HeldEnough, Capped(good, held: 4, shareCap: 9f));
            Assert.Equal(Block.HerdFull, Capped(good, livestock: true, herdRoom: 0));
            Assert.Equal(Block.VillageLastUnit, Capped(good, lastInVillage: true));

            Assert.Equal(Block.BudgetSpent,
                Capped(good, price: 50, budget: 10, count: 99, lastInVillage: true,
                       s: new Options { BuyCapPerItem = 1 }));
        }

        [Fact]
        public void A_cap_set_to_zero_is_off()
        {
            Good good = Cargo();
            var off = new Options { BuyCapPerItem = 0, BuyValueCapPerItem = 0, MaxHeldPerItem = 0 };
            Assert.Equal(Block.None, Capped(good, count: 9999, spent: 9999, held: 9999, s: off));
            Assert.Equal(Block.None, Capped(good, held: 9999, shareCap: 0f, s: off));
        }

        private static Block PerItem(Good good, int price = 10, int count = 0, int spent = 0,
                                     int held = 0, float shareCap = 0f, Options s = null) =>
            TradeRules.WhatCapsAGood(good, price, (count, spent), held, shareCap, s ?? new Options());

        [Fact]
        public void The_per_item_caps_are_one_rule_every_pass_that_buys_can_ask()
        {
            Good good = Cargo();
            var s = new Options { BuyCapPerItem = 32, BuyValueCapPerItem = 100, MaxHeldPerItem = 5 };

            Assert.Equal(Block.None, PerItem(good));
            Assert.Equal(Block.ItemCountCap, PerItem(good, count: 32, s: s));
            Assert.Equal(Block.ItemValueCap, PerItem(good, price: 10, spent: 95, s: s));
            Assert.Equal(Block.HeldEnough,
                PerItem(good, held: 5, s: new Options { MaxHeldPerItem = 5, BuyCapPerItem = 0 }));
            Assert.Equal(Block.HeldEnough, PerItem(good, held: 4, shareCap: 9f));

            foreach (int count in new[] { 0, 31, 32 })
                foreach (int spent in new[] { 0, 95, 100 })
                    foreach (int held in new[] { 0, 4, 5 })
                        foreach (float shareCap in new[] { 0f, 9f, 40f })
                            Assert.Equal(
                                Capped(good, count: count, spent: spent, held: held,
                                       shareCap: shareCap, s: s),
                                PerItem(good, count: count, spent: spent, held: held,
                                        shareCap: shareCap, s: s));
        }

        [Fact]
        public void The_per_item_caps_leave_the_purse_the_herd_and_the_village_to_the_buying_pass()
        {
            Good good = Cargo();
            Assert.Equal(Block.None, PerItem(good, price: 5000));
            Assert.Equal(Block.BudgetSpent, Capped(good, price: 5000, budget: 10));
            Assert.Equal(Block.HerdFull, Capped(Livestock(), livestock: true, herdRoom: 0));
            Assert.Equal(Block.None, PerItem(Livestock()));
            Assert.Equal(Block.VillageLastUnit, Capped(good, lastInVillage: true));
        }

        [Fact]
        public void A_good_with_no_room_left_for_it_is_the_last_thing_asked()
        {
            Assert.True(TradeRules.NoRoomForOneMore(Cargo(), roomLeft: 1f));
            Assert.False(TradeRules.NoRoomForOneMore(Cargo(), roomLeft: 3f));
            Assert.False(TradeRules.NoRoomForOneMore(
                new Good { Id = "note", Name = "note", Weight = 0f }, roomLeft: 0f));
        }
    }
}
