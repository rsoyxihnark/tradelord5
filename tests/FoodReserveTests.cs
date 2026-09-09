using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class FoodReserveTests
    {
        private static TradeRules.Ration Food(string id, int amount, int value) =>
            new TradeRules.Ration
            {
                Good = new Good { Id = id, Name = id, IsFood = true, IsTradeGood = true, Value = value },
                Amount = amount
            };

        private static TradeRules.Ration Herd(string id, int amount, int value, int meat) =>
            new TradeRules.Ration
            {
                Good = new Good
                {
                    Id = id, Name = id, HasHorse = true, IsLivestock = true, IsAnimal = true,
                    Value = value, MeatCount = meat
                },
                Amount = amount
            };

        private static TradeRules.Ration Cargo(string id, int amount) =>
            new TradeRules.Ration
            {
                Good = new Good { Id = id, Name = id, IsTradeGood = true, Value = 50 },
                Amount = amount
            };

        private static Dictionary<string, int> Keep(List<TradeRules.Ration> carried, Options s,
                                                    float perDay = 1f) =>
            TradeRules.FoodKeep(carried, perDay, s);

        [Fact]
        public void What_a_good_feeds_is_one_for_food_and_its_meat_for_livestock()
        {
            Assert.Equal(1, TradeRules.FoodValue(Food("grain", 1, 10).Good));
            Assert.Equal(0, TradeRules.FoodValue(Cargo("iron", 1).Good));
            Assert.Equal(6, TradeRules.FoodValue(Herd("cow", 1, 100, 6).Good));
            Assert.Equal(0, TradeRules.FoodValue(default(Good)));

            var mount = new Good { Id = "horse", Name = "horse", HasHorse = true, IsSpareMount = true, MeatCount = 9 };
            Assert.Equal(0, TradeRules.FoodValue(mount));
        }

        [Fact]
        public void Only_food_you_can_store_counts_as_the_larder()
        {
            Assert.True(TradeRules.IsStorableFood(Food("grain", 1, 10).Good));
            Assert.False(TradeRules.IsStorableFood(Herd("cow", 1, 100, 6).Good));
            Assert.False(TradeRules.IsStorableFood(Cargo("iron", 1).Good));
            Assert.False(TradeRules.IsStorableFood(default(Good)));
        }

        [Fact]
        public void No_reserve_is_held_when_both_settings_are_off()
        {
            var off = new Options { KeepFoodDays = 0, KeepEveryFoodKind = false };
            Assert.Empty(Keep(new List<TradeRules.Ration> { Food("grain", 50, 10) }, off));
        }

        [Fact]
        public void The_reserve_is_days_of_supply_times_what_the_party_eats()
        {
            var s = new Options { KeepFoodDays = 5, KeepEveryFoodKind = false };
            var carried = new List<TradeRules.Ration> { Food("grain", 100, 10) };

            Assert.Equal(20, Keep(carried, s, perDay: 4f)["grain"]);
            Assert.Equal(5, Keep(carried, s, perDay: 1f)["grain"]);
        }

        [Fact]
        public void A_party_that_eats_nothing_is_still_fed_one_a_day()
        {
            var s = new Options { KeepFoodDays = 3, KeepEveryFoodKind = false };
            var carried = new List<TradeRules.Ration> { Food("grain", 100, 10) };
            Assert.Equal(3, Keep(carried, s, perDay: 0f)["grain"]);
            Assert.Equal(3, Keep(carried, s, perDay: -2f)["grain"]);
        }

        [Fact]
        public void The_cheapest_food_per_unit_fed_is_kept_first()
        {
            var s = new Options { KeepFoodDays = 4, KeepEveryFoodKind = false };
            var carried = new List<TradeRules.Ration>
            {
                Food("caviar", 50, 200),
                Food("grain", 50, 10),
            };
            Dictionary<string, int> kept = Keep(carried, s, perDay: 1f);
            Assert.Equal(4, kept["grain"]);
            Assert.False(kept.ContainsKey("caviar"));
        }

        [Fact]
        public void Livestock_is_eaten_last_however_cheap_it_is()
        {
            var s = new Options { KeepFoodDays = 4, KeepEveryFoodKind = false };
            var carried = new List<TradeRules.Ration>
            {
                Herd("cow", 10, 1, 1),
                Food("grain", 50, 500),
            };
            Dictionary<string, int> kept = Keep(carried, s, perDay: 1f);
            Assert.Equal(4, kept["grain"]);
            Assert.False(kept.ContainsKey("cow"));
        }

        [Fact]
        public void A_head_of_livestock_counts_for_all_the_meat_it_carries()
        {
            var s = new Options { KeepFoodDays = 10, KeepEveryFoodKind = false };
            var carried = new List<TradeRules.Ration> { Herd("cow", 10, 100, 5) };
            Assert.Equal(2, Keep(carried, s, perDay: 1f)["cow"]);
        }

        [Fact]
        public void The_reserve_never_keeps_more_than_you_are_carrying()
        {
            var s = new Options { KeepFoodDays = 30, KeepEveryFoodKind = false };
            var carried = new List<TradeRules.Ration> { Food("grain", 3, 10) };
            Assert.Equal(3, Keep(carried, s, perDay: 10f)["grain"]);
        }

        [Fact]
        public void The_reserve_spills_onto_the_next_cheapest_food_once_the_first_runs_out()
        {
            var s = new Options { KeepFoodDays = 10, KeepEveryFoodKind = false };
            var carried = new List<TradeRules.Ration>
            {
                Food("grain", 4, 10),
                Food("fish", 50, 20),
            };
            Dictionary<string, int> kept = Keep(carried, s, perDay: 1f);
            Assert.Equal(4, kept["grain"]);
            Assert.Equal(6, kept["fish"]);
        }

        [Fact]
        public void Keeping_some_of_every_kind_holds_a_floor_of_each_before_the_reserve_is_spent()
        {
            var s = new Options { KeepFoodDays = 0, KeepEveryFoodKind = true, KeepPerFoodKind = 2 };
            var carried = new List<TradeRules.Ration>
            {
                Food("grain", 50, 10),
                Food("fish", 50, 20),
                Food("cheese", 1, 30),
            };
            Dictionary<string, int> kept = Keep(carried, s, perDay: 1f);
            Assert.Equal(2, kept["grain"]);
            Assert.Equal(2, kept["fish"]);
            Assert.Equal(1, kept["cheese"]);
        }

        [Fact]
        public void The_variety_floor_is_never_put_on_livestock()
        {
            var s = new Options { KeepFoodDays = 0, KeepEveryFoodKind = true, KeepPerFoodKind = 3 };
            var carried = new List<TradeRules.Ration>
            {
                Food("grain", 50, 10),
                Herd("cow", 50, 100, 5),
            };
            Dictionary<string, int> kept = Keep(carried, s, perDay: 1f);
            Assert.Equal(3, kept["grain"]);
            Assert.False(kept.ContainsKey("cow"));
        }

        [Fact]
        public void What_the_variety_floor_already_holds_comes_off_the_days_of_supply()
        {
            var s = new Options { KeepFoodDays = 8, KeepEveryFoodKind = true, KeepPerFoodKind = 2 };
            var carried = new List<TradeRules.Ration>
            {
                Food("grain", 50, 10),
                Food("fish", 50, 20),
            };
            Dictionary<string, int> kept = Keep(carried, s, perDay: 1f);
            Assert.Equal(2, kept["fish"]);
            Assert.Equal(6, kept["grain"]);
            Assert.Equal(8, kept["grain"] + kept["fish"]);
        }

        [Fact]
        public void A_variety_floor_bigger_than_the_reserve_leaves_nothing_to_top_up()
        {
            var s = new Options { KeepFoodDays = 4, KeepEveryFoodKind = true, KeepPerFoodKind = 3 };
            var carried = new List<TradeRules.Ration>
            {
                Food("grain", 50, 10),
                Food("fish", 50, 20),
            };
            Dictionary<string, int> kept = Keep(carried, s, perDay: 1f);
            Assert.Equal(3, kept["grain"]);
            Assert.Equal(3, kept["fish"]);
        }

        [Fact]
        public void A_good_on_the_always_sell_list_is_never_held_by_the_reserve()
        {
            var s = new Options { KeepFoodDays = 10, AlwaysSellItems = "grain" };
            var carried = new List<TradeRules.Ration>
            {
                Food("grain", 50, 10),
                Food("fish", 50, 20),
            };
            Dictionary<string, int> kept = Keep(carried, s, perDay: 1f);
            Assert.False(kept.ContainsKey("grain"));
            Assert.Equal(10, kept["fish"]);
        }

        [Fact]
        public void Two_helpings_of_the_same_good_are_counted_together()
        {
            var s = new Options { KeepFoodDays = 8, KeepEveryFoodKind = false };
            var carried = new List<TradeRules.Ration>
            {
                Food("grain", 3, 10),
                Food("grain", 20, 10),
            };
            Assert.Equal(8, Keep(carried, s, perDay: 1f)["grain"]);
        }

        [Fact]
        public void The_reserve_reaches_past_the_biggest_helping_to_every_other_one()
        {
            var s = new Options { KeepFoodDays = 15, KeepEveryFoodKind = false };
            var carried = new List<TradeRules.Ration>
            {
                Food("grain", 10, 10),
                Food("grain", 10, 10),
            };
            Assert.Equal(15, Keep(carried, s, perDay: 1f)["grain"]);
        }

        [Fact]
        public void The_variety_floor_counts_every_helping_of_a_kind_together()
        {
            var s = new Options { KeepFoodDays = 0, KeepEveryFoodKind = true, KeepPerFoodKind = 5 };
            var carried = new List<TradeRules.Ration>
            {
                Food("grain", 1, 10),
                Food("grain", 1, 10),
            };
            Assert.Equal(2, Keep(carried, s, perDay: 1f)["grain"]);
        }

        [Fact]
        public void Nothing_you_are_not_carrying_is_reserved()
        {
            var s = new Options { KeepFoodDays = 5, KeepEveryFoodKind = false };
            var carried = new List<TradeRules.Ration>
            {
                Food("grain", 0, 10),
                Cargo("iron", 50),
            };
            Assert.Empty(Keep(carried, s, perDay: 1f));
            Assert.Empty(Keep(null, s, perDay: 1f));
        }
    }
}
