using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class SellRulesTests
    {
        private struct Says : IWhatTheGameSays
        {
            internal bool IsLocked;
            internal bool IsSmeltable;
            internal bool Learned;

            public bool Locked() => IsLocked;

            public bool Smeltable() => IsSmeltable;

            public bool PartsAllLearned() => Learned;
        }

        private static Good Cargo(string id = "grain") =>
            new Good { Id = id, Name = id, IsTradeGood = true, IsFood = false, Weight = 1f, Value = 10 };

        private static Good Livestock(string id = "cow") =>
            new Good { Id = id, Name = id, HasHorse = true, IsLivestock = true, IsAnimal = true };

        private static Good Loot(string id = "sword", int tier = 0) =>
            new Good { Id = id, Name = id, Tier = tier, Value = 100 };

        private static SellVerdict Ask(Good good, int amount = 10, SellFacts facts = default(SellFacts),
                                       Options s = null, Says says = default(Says),
                                       bool questsReadable = true)
        {
            facts.QuestsReadable = questsReadable;
            return TradeRules.MaySell(good, amount, facts, s ?? new Options(), says);
        }

        [Fact]
        public void A_plain_trade_good_sells()
        {
            SellVerdict said = Ask(Cargo());
            Assert.True(said.Allowed);
            Assert.Equal(Block.None, said.Why);
            Assert.Equal(0, said.KeepCount);
        }

        [Fact]
        public void Nothing_the_game_refuses_to_trade_is_sold()
        {
            Assert.Equal(Block.NotMerchandise, Ask(default(Good)).Why);
            Assert.Equal(Block.NotMerchandise, Ask(new Good { Id = "x", NotMerchandise = true }).Why);
            Assert.Equal(Block.NotMerchandise,
                Ask(Cargo(), facts: new SellFacts { QuestItem = true }).Why);
        }

        [Fact]
        public void The_never_sell_list_holds_a_good_by_id_or_by_the_name_on_screen()
        {
            var byId = new Options { NeverSellItems = "grain" };
            Assert.Equal(Block.NeverList, Ask(Cargo("grain"), s: byId).Why);

            var byName = new Options { NeverSellItems = "Smoked Fish" };
            Assert.Equal(Block.NeverList,
                Ask(new Good { Id = "fish", Name = "Smoked Fish", IsTradeGood = true }, s: byName).Why);

            var other = new Options { NeverSellItems = "iron" };
            Assert.True(Ask(Cargo("grain"), s: other).Allowed);
        }

        [Fact]
        public void An_inventory_lock_holds_a_good()
        {
            Assert.Equal(Block.Locked, Ask(Cargo(), says: new Says { IsLocked = true }).Why);
        }

        [Fact]
        public void An_animal_a_quest_is_waiting_on_is_kept_back()
        {
            SellVerdict all = Ask(Livestock(), amount: 3,
                facts: new SellFacts { AwaitedHeld = 3 });
            Assert.False(all.Allowed);
            Assert.Equal(Block.QuestAnimal, all.Why);
            Assert.Equal(3, all.KeepCount);
            Assert.Equal(3, all.DrewAwaited);

            SellVerdict some = Ask(Livestock(), amount: 5,
                facts: new SellFacts { AwaitedHeld = 2 });
            Assert.True(some.Allowed);
            Assert.Equal(2, some.KeepCount);
            Assert.Equal(2, some.DrewAwaited);
        }

        [Fact]
        public void The_quest_hold_is_asked_before_the_always_sell_list_and_the_food_reserve_after_it()
        {
            var always = new Options { AlwaysSellItems = "cow" };

            SellVerdict quest = Ask(Livestock(), amount: 2, s: always,
                facts: new SellFacts { AwaitedHeld = 2 });
            Assert.False(quest.Allowed);
            Assert.Equal(Block.QuestAnimal, quest.Why);

            SellVerdict food = Ask(Livestock(), amount: 2, s: always,
                facts: new SellFacts { FoodHeld = 2 });
            Assert.True(food.Allowed);
            Assert.Equal(0, food.DrewFood);
        }

        [Fact]
        public void No_animal_moves_at_all_while_the_quests_cannot_be_read()
        {
            SellVerdict said = Ask(Livestock(), questsReadable: false);
            Assert.False(said.Allowed);
            Assert.Equal(Block.QuestAnimal, said.Why);
        }

        [Fact]
        public void A_category_set_to_leave_alone_stops_the_sale()
        {
            var s = new Options { LivestockPolicy = Options.PolicyIgnore };
            Assert.Equal(Block.CategoryPolicy, Ask(Livestock(), s: s).Why);

            var buyOnly = new Options { FoodPolicy = Options.PolicyBuyOnly };
            var bread = new Good { Id = "bread", Name = "bread", IsTradeGood = true, IsFood = true };
            Assert.Equal(Block.CategoryPolicy, Ask(bread, s: buyOnly).Why);

            var smithing = new Options { CraftingPolicy = Options.PolicySellOnly };
            var ore = new Good { Id = "ore", Name = "ore", IsTradeGood = true, IsSmithingMaterial = true };
            Assert.True(Ask(ore, s: smithing).Allowed);
        }

        [Fact]
        public void A_haul_animal_or_a_spare_mount_is_not_sold_as_livestock()
        {
            var haul = new Good { Id = "mule", Name = "mule", HasHorse = true, IsHaulAnimal = true };
            Assert.Equal(Block.MountOrHaulAnimal, Ask(haul).Why);

            var mount = new Good { Id = "horse", Name = "horse", HasHorse = true, IsSpareMount = true };
            Assert.Equal(Block.MountOrHaulAnimal, Ask(mount).Why);
        }

        [Fact]
        public void The_always_sell_list_releases_a_haul_animal_a_category_policy_would_hold()
        {
            var s = new Options { AlwaysSellItems = "mule" };
            var haul = new Good { Id = "mule", Name = "mule", HasHorse = true, IsHaulAnimal = true };
            Assert.True(Ask(haul, s: s).Allowed);
        }

        [Fact]
        public void Unique_and_crafted_gear_is_protected_while_the_switch_is_on()
        {
            var kept = new Good { Id = "blade", Name = "blade", IsTradeGood = true, IsUnique = true };
            Assert.Equal(Block.Protected, Ask(kept).Why);

            var off = new Options { ProtectSpecial = false };
            Assert.True(Ask(kept, s: off).Allowed);

            var mine = new Good { Id = "mine", Name = "mine", IsTradeGood = true, IsCraftedByPlayer = true };
            Assert.Equal(Block.Protected, Ask(mine).Why);
        }

        [Fact]
        public void Smeltable_weapons_are_sold_kept_or_kept_only_while_unlearned()
        {
            Good weapon = Loot("axe");
            var says = new Says { IsSmeltable = true, Learned = false };

            var sellThem = new Options { KeepSmeltableWeapons = Options.SmeltSellThem, MaxLootTier = 1 };
            Assert.True(Ask(weapon, s: sellThem, says: says).Allowed);

            var keepAll = new Options { KeepSmeltableWeapons = Options.SmeltKeepAll, MaxLootTier = 1 };
            Assert.Equal(Block.Smeltable, Ask(weapon, s: keepAll, says: says).Why);

            var keepUnlearned = new Options
            { KeepSmeltableWeapons = Options.SmeltKeepUnlearned, MaxLootTier = 1 };
            Assert.Equal(Block.Smeltable, Ask(weapon, s: keepUnlearned, says: says).Why);
            Assert.True(Ask(weapon, s: keepUnlearned,
                says: new Says { IsSmeltable = true, Learned = true }).Allowed);
        }

        [Fact]
        public void Loot_goes_only_up_to_the_tier_you_allow()
        {
            var one = new Options { MaxLootTier = 1 };
            Assert.True(Ask(Loot(tier: 0), s: one).Allowed);
            Assert.Equal(Block.NotTradable, Ask(Loot(tier: 1), s: one).Why);

            var three = new Options { MaxLootTier = 3 };
            Assert.True(Ask(Loot(tier: 2), s: three).Allowed);
            Assert.Equal(Block.NotTradable, Ask(Loot(tier: 3), s: three).Why);

            var off = new Options { MaxLootTier = 0 };
            Assert.Equal(Block.NotTradable, Ask(Loot(tier: 0), s: off).Why);
        }

        [Fact]
        public void A_mountable_or_edible_thing_never_goes_out_as_loot()
        {
            var s = new Options { MaxLootTier = 6 };
            var saddle = new Good { Id = "saddle", Name = "saddle", Tier = 0, IsMountable = true };
            Assert.Equal(Block.NotTradable, Ask(saddle, s: s).Why);

            var meal = new Good { Id = "meal", Name = "meal", Tier = 0, IsFood = true };
            Assert.Equal(Block.NotTradable, Ask(meal, s: s).Why);

            var beast = new Good { Id = "beastly", Name = "beastly", Tier = 0, IsAnimal = true };
            Assert.Equal(Block.NotTradable, Ask(beast, s: s).Why);
        }

        [Fact]
        public void Food_the_reserve_is_holding_stays_in_your_bags()
        {
            SellVerdict all = Ask(Cargo("grain"), amount: 4,
                facts: new SellFacts { FoodHeld = 4 });
            Assert.False(all.Allowed);
            Assert.Equal(Block.FoodReserve, all.Why);
            Assert.Equal(4, all.KeepCount);
            Assert.Equal(4, all.DrewFood);

            SellVerdict spare = Ask(Cargo("grain"), amount: 10,
                facts: new SellFacts { FoodHeld = 4 });
            Assert.True(spare.Allowed);
            Assert.Equal(4, spare.KeepCount);
            Assert.Equal(4, spare.DrewFood);
        }

        [Fact]
        public void A_reserve_never_draws_more_than_the_stack_holds()
        {
            SellVerdict said = Ask(Cargo("grain"), amount: 2,
                facts: new SellFacts { FoodHeld = 9 });
            Assert.Equal(2, said.DrewFood);
            Assert.Equal(Block.FoodReserve, said.Why);
        }

        [Fact]
        public void An_animal_held_by_both_a_quest_and_the_food_reserve_reports_the_food_draw_last()
        {
            SellVerdict said = Ask(Livestock(), amount: 10,
                facts: new SellFacts { AwaitedHeld = 2, FoodHeld = 3 });
            Assert.True(said.Allowed);
            Assert.Equal(2, said.DrewAwaited);
            Assert.Equal(3, said.DrewFood);
            Assert.Equal(3, said.KeepCount);
        }

        [Fact]
        public void A_good_you_paid_for_is_measured_against_what_you_paid()
        {
            Assert.Equal(40, TradeRules.WorthToBeat(Cargo(), 40, 90));
            Assert.Equal(40, TradeRules.WorthToBeat(Loot(), 40, 90));
            Assert.Equal(40, TradeRules.WorthToBeat(Livestock(), 40, 90));
        }

        [Fact]
        public void Merchandise_you_never_paid_for_is_measured_against_what_it_is_worth()
        {
            Assert.Equal(90, TradeRules.WorthToBeat(Cargo(), 0, 90));
            Assert.Equal(90, TradeRules.WorthToBeat(Livestock(), 0, 90));
        }

        [Fact]
        public void Looted_gear_you_never_paid_for_still_goes_for_whatever_the_market_pays()
        {
            Assert.Equal(0, TradeRules.WorthToBeat(Loot(), 0, 90));
        }

        [Fact]
        public void Free_merchandise_only_sells_where_the_price_clears_your_margin()
        {
            int worth = TradeRules.WorthToBeat(Cargo(), 0, 100);
            Assert.False(TradeMath.ProfitAcceptable(worth, 100, 0.15f));
            Assert.False(TradeMath.ProfitAcceptable(worth, 114, 0.15f));
            Assert.True(TradeMath.ProfitAcceptable(worth, 115, 0.15f));
        }
    }
}
