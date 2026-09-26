using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class HerdRulesTests
    {
        [Fact]
        public void One_man_is_a_man_and_any_other_count_is_men()
        {
            Assert.Equal("1 man", Herding.Men(1));
            Assert.Equal("22 men", Herding.Men(22));
            Assert.Equal("0 men", Herding.Men(0));
        }

        private static Good Livestock() =>
            new Good { Id = "cow", Name = "cow", HasHorse = true, IsLivestock = true, IsAnimal = true };

        private static Good PlainMount() =>
            new Good { Id = "saddle horse", Name = "saddle horse", HasHorse = true, IsSpareMount = true };

        private static Good PrizeMount() =>
            new Good { Id = "war horse", Name = "war horse", HasHorse = true, IsSpareMount = true, IsPrizeMount = true };

        private static Good HaulAnimal() =>
            new Good { Id = "mule", Name = "mule", HasHorse = true, IsHaulAnimal = true };

        private static Good Cargo() =>
            new Good { Id = "iron", Name = "iron", IsTradeGood = true };

        [Fact]
        public void A_lame_horse_mule_or_camel_is_not_counted_by_the_game_until_a_load_while_livestock_always_is()
        {
            Assert.True(Herding.TheGameCountsItAtOnce(livestock: false, ofAQuality: false));
            Assert.False(Herding.TheGameCountsItAtOnce(livestock: false, ofAQuality: true));
            Assert.True(Herding.TheGameCountsItAtOnce(livestock: true, ofAQuality: false));
            Assert.True(Herding.TheGameCountsItAtOnce(livestock: true, ofAQuality: true));
        }

        [Fact]
        public void One_haul_animal_carries_its_share_of_what_the_game_says_your_pack_animals_carry()
        {
            Assert.Equal(100f, Herding.CargoAHaulAnimalAdds(200f, 2, 10, 1000f, 1000f), 3);
            Assert.Equal(115f, Herding.CargoAHaulAnimalAdds(230f, 2, 10, 1000f, 1000f), 3);
            Assert.Equal(110f, Herding.CargoAHaulAnimalAdds(200f, 2, 10, 1000f, 1100f), 3);
        }

        [Fact]
        public void With_no_pack_animal_to_read_one_haul_animal_carries_what_the_game_gives_one_before_perks()
        {
            Assert.Equal(100f, Herding.CargoAHaulAnimalAdds(0f, 0, 10, 500f, 500f), 3);
            Assert.Equal(110f, Herding.CargoAHaulAnimalAdds(0f, 0, 10, 500f, 550f), 3);
            Assert.Equal(100f, Herding.CargoAHaulAnimalAdds(0f, 3, 10, 800f, 800f), 3);
            Assert.Equal(100f, Herding.CargoAHaulAnimalAdds(float.NaN, 2, 10, 0f, 0f), 3);
            Assert.Equal(0f, Herding.CargoAHaulAnimalAdds(0f, 0, 0, 500f, 500f), 3);
        }

        private static int HaulAnimalsBought(int budget, int price, float each, float share, float roomLeft,
                                             float weightLeft, int costLeft)
        {
            int hauled = 0;
            while (price < budget &&
                   Herding.AnotherHaulAnimalIsWanted(hauled, each, share, roomLeft,
                       TradeMath.WeightTheBudgetCanStillBuy(weightLeft, costLeft, budget - price)))
            {
                hauled++;
                budget -= price;
            }
            return hauled;
        }

        [Fact]
        public void Haul_animals_are_bought_only_for_the_goods_the_gold_left_after_them_can_still_buy()
        {
            Assert.Equal(1, HaulAnimalsBought(400, 150, 100f, 1f, 0f, 300f, 800));
            Assert.Equal(3, HaulAnimalsBought(5000, 150, 100f, 1f, 0f, 300f, 800));
            Assert.Equal(6, HaulAnimalsBought(5000, 150, 100f, 0.5f, 0f, 300f, 800));
            Assert.Equal(0, HaulAnimalsBought(150, 150, 100f, 1f, 0f, 300f, 800));
            Assert.Equal(0, HaulAnimalsBought(5000, 150, 100f, 1f, 0f, 0f, 800));
        }

        [Fact]
        public void The_room_your_cargo_still_has_counts_before_any_haul_animal_is_bought()
        {
            Assert.Equal(2, HaulAnimalsBought(5000, 150, 100f, 1f, 150f, 300f, 800));
            Assert.Equal(0, HaulAnimalsBought(5000, 150, 100f, 1f, 300f, 300f, 800));
            Assert.Equal(4, HaulAnimalsBought(5000, 150, 100f, 1f, -50f, 300f, 800));
        }

        private static int HaulAnimalsBoughtForGoods(int budget, int price, float each, float share, float roomLeft,
                                                     float weightLeft, int costLeft)
        {
            int hauled = 0;
            while (price < budget &&
                   Herding.AnotherHaulAnimalIsWanted(hauled, each, share, roomLeft,
                       TradeMath.WeightTheBudgetCanStillBuy(weightLeft, costLeft, budget - price),
                       Herding.LeastAHaulAnimalIsFilled))
            {
                hauled++;
                budget -= price;
            }
            return hauled;
        }

        [Fact]
        public void No_haul_animal_is_bought_for_goods_that_would_fill_less_than_half_of_it()
        {
            Assert.Equal(0, HaulAnimalsBoughtForGoods(1000, 944, 100f, 1f, 2f, 100f, 1000));
            Assert.Equal(0, HaulAnimalsBoughtForGoods(1000, 150, 100f, 1f, 2f, 5f, 50));
            Assert.Equal(1, HaulAnimalsBoughtForGoods(1000, 150, 100f, 1f, 2f, 60f, 300));
            Assert.Equal(1, HaulAnimalsBoughtForGoods(400, 150, 100f, 1f, 0f, 300f, 800));
            Assert.Equal(2, HaulAnimalsBoughtForGoods(5000, 150, 100f, 1f, 0f, 240f, 800));
            Assert.Equal(3, HaulAnimalsBoughtForGoods(5000, 150, 100f, 1f, 0f, 250f, 800));
            Assert.Equal(2, HaulAnimalsBoughtForGoods(5000, 150, 100f, 0.5f, 0f, 120f, 800));
        }

        [Fact]
        public void A_haul_animal_asked_to_be_filled_by_nothing_in_particular_is_bought_however_little_it_carries()
        {
            Assert.Equal(1, HaulAnimalsBought(1000, 150, 100f, 1f, 2f, 10f, 60));
            Assert.True(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, 2f, 10f));
            Assert.False(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, 2f, 10f, Herding.LeastAHaulAnimalIsFilled));
            Assert.True(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, 2f, 10f, float.NaN));
        }

        [Fact]
        public void A_haul_animal_is_bought_for_light_goods_that_would_make_more_than_it_costs()
        {
            float half = Herding.LeastAHaulAnimalIsFilled;
            Assert.True(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, 2f, 47f, half, 2000f, 150));
            Assert.False(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, 2f, 47f, half, 100f, 150));
            Assert.True(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, 2f, 47f, half, 160f, 150));
            Assert.False(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, 2f, 47f, half, 2000f, 0));
            Assert.False(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, 2f, 47f, half, float.NaN, 150));
            Assert.False(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, 47f, 47f, half, 2000f, 150));
            Assert.False(Herding.AnotherHaulAnimalIsWanted(1, 100f, 1f, 2f, 130f, half, 600f, 150));
            Assert.True(Herding.AnotherHaulAnimalIsWanted(1, 100f, 1f, 2f, 130f, half, 700f, 150));
        }

        [Fact]
        public void Food_gets_a_haul_animal_it_would_fill_less_than_half_of_only_once_the_party_is_down_to_its_last_day()
        {
            Assert.Equal(0f, Herding.LeastFilledFor(true, 5, 10), 3);
            Assert.Equal(0f, Herding.LeastFilledFor(true, 0, 1), 3);
            Assert.Equal(Herding.LeastAHaulAnimalIsFilled, Herding.LeastFilledFor(true, 10, 10), 3);
            Assert.Equal(Herding.LeastAHaulAnimalIsFilled, Herding.LeastFilledFor(true, 25, 10), 3);
            Assert.Equal(Herding.LeastAHaulAnimalIsFilled, Herding.LeastFilledFor(false, 0, 10), 3);
        }

        [Fact]
        public void A_spare_mount_carries_what_the_game_gives_one_and_its_share_of_a_bonus_on_the_whole_hold()
        {
            Assert.Equal(20f, Herding.CargoASpareMountAdds(10, 500f, 500f), 3);
            Assert.Equal(22f, Herding.CargoASpareMountAdds(10, 500f, 550f), 3);
            Assert.Equal(20f, Herding.CargoASpareMountAdds(10, 0f, 0f), 3);
            Assert.Equal(0f, Herding.CargoASpareMountAdds(0, 500f, 500f), 3);
        }

        [Fact]
        public void A_haul_animal_is_bought_only_while_your_purse_is_above_the_floor_you_set()
        {
            Assert.True(Herding.PurseClearsTheFloor(2100, 2000));
            Assert.False(Herding.PurseClearsTheFloor(2000, 2000));
            Assert.False(Herding.PurseClearsTheFloor(1800, 2000));
            Assert.True(Herding.PurseClearsTheFloor(0, 0));
            Assert.True(Herding.PurseClearsTheFloor(50, -1));
        }

        private static (int hauled, int purse) HaulAnimalsBoughtAboveTheFloor(int purse, int floor, int price,
                                                                              int wanted)
        {
            int hauled = 0;
            while (hauled < wanted && Herding.PurseClearsTheFloor(purse - price, floor))
            {
                hauled++;
                purse -= price;
            }
            return (hauled, purse);
        }

        [Fact]
        public void A_haul_animal_is_bought_only_while_your_purse_stays_above_the_floor_once_it_is_paid_for()
        {
            Assert.Equal((0, 2100), HaulAnimalsBoughtAboveTheFloor(2100, 2000, 300, 3));
            Assert.Equal((1, 2300), HaulAnimalsBoughtAboveTheFloor(2600, 2000, 300, 3));
            Assert.Equal((2, 2001), HaulAnimalsBoughtAboveTheFloor(2601, 2000, 300, 3));
            Assert.Equal((0, 2000), HaulAnimalsBoughtAboveTheFloor(2000, 2000, 300, 3));
            Assert.Equal((3, 1200), HaulAnimalsBoughtAboveTheFloor(2100, 0, 300, 3));
        }

        [Fact]
        public void No_haul_animal_is_wanted_for_nothing_or_by_an_animal_that_carries_nothing()
        {
            Assert.False(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, 0f, 0f));
            Assert.False(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, 0f, float.NaN));
            Assert.False(Herding.AnotherHaulAnimalIsWanted(0, 0f, 1f, 0f, 300f));
            Assert.False(Herding.AnotherHaulAnimalIsWanted(0, float.NaN, 1f, 0f, 300f));
            Assert.True(Herding.AnotherHaulAnimalIsWanted(0, 100f, 1f, float.NaN, 300f));
        }

        [Fact]
        public void A_haul_animal_goes_only_while_what_is_left_still_carries_your_cargo()
        {
            Assert.Equal(2, Herding.HaulAnimalsToSpare(4, 4, 400f, 1000f, 1000f, 750f));
            Assert.Equal(0, Herding.HaulAnimalsToSpare(4, 4, 400f, 1000f, 1000f, 950f));
            Assert.Equal(4, Herding.HaulAnimalsToSpare(4, 4, 400f, 1000f, 1000f, 100f));
        }

        [Fact]
        public void A_party_already_carrying_all_it_can_keeps_every_haul_animal()
        {
            Assert.Equal(0, Herding.HaulAnimalsToSpare(3, 3, 300f, 500f, 500f, 500f));
            Assert.Equal(0, Herding.HaulAnimalsToSpare(3, 3, 300f, 500f, 500f, 700f));
            Assert.Equal(0, Herding.HaulAnimalsToSpare(0, 3, 300f, 500f, 500f, 10f));
        }

        [Fact]
        public void A_bonus_to_the_whole_capacity_counts_against_every_haul_animal_it_would_take_away()
        {
            Assert.Equal(2, Herding.HaulAnimalsToSpare(4, 4, 400f, 1000f, 1000f, 750f));
            Assert.Equal(1, Herding.HaulAnimalsToSpare(4, 4, 400f, 1000f, 1500f, 1250f));
        }

        [Fact]
        public void Haul_animals_that_add_nothing_to_the_capacity_can_all_go()
        {
            Assert.Equal(3, Herding.HaulAnimalsToSpare(3, 0, 0f, 500f, 500f, 100f));
            Assert.Equal(3, Herding.HaulAnimalsToSpare(3, 3, 0f, 500f, 500f, 100f));
        }

        [Fact]
        public void The_herd_gives_up_its_animals_in_the_order_the_feature_list_promises()
        {
            Assert.Equal(TradeRules.RankLivestock, TradeRules.HerdShedRank(Livestock()));
            Assert.Equal(TradeRules.RankPlainMount, TradeRules.HerdShedRank(PlainMount()));
            Assert.Equal(TradeRules.RankHaulAnimal, TradeRules.HerdShedRank(HaulAnimal()));
            Assert.Equal(TradeRules.RankPrizeMount, TradeRules.HerdShedRank(PrizeMount()));

            Assert.True(TradeRules.RankLivestock < TradeRules.RankPlainMount);
            Assert.True(TradeRules.RankPlainMount < TradeRules.RankHaulAnimal);
            Assert.True(TradeRules.RankHaulAnimal < TradeRules.RankPrizeMount);
        }

        [Fact]
        public void Anything_that_is_not_an_animal_has_no_place_in_the_herd_order()
        {
            Assert.Equal(TradeRules.RankNotAnAnimal, TradeRules.HerdShedRank(Cargo()));
            Assert.Equal(TradeRules.RankNotAnAnimal, TradeRules.HerdShedRank(default(Good)));
            Assert.True(TradeRules.RankNotAnAnimal < TradeRules.RankLivestock);
        }

        [Fact]
        public void An_animal_that_hauls_nothing_and_is_no_livestock_is_still_ranked_as_a_mount()
        {
            var odd = new Good { Id = "odd", Name = "odd", HasHorse = true };
            Assert.Equal(TradeRules.RankNotAnAnimal, TradeRules.HerdShedRank(odd));
        }

        [Fact]
        public void Each_kind_of_animal_is_named_for_the_log_and_cargo_is_named_not_at_all()
        {
            Assert.Equal("livestock", TradeRules.AnimalGroup(Livestock()));
            Assert.Equal("a mount", TradeRules.AnimalGroup(PlainMount()));
            Assert.Equal("a mount", TradeRules.AnimalGroup(PrizeMount()));
            Assert.Equal("a haul animal", TradeRules.AnimalGroup(HaulAnimal()));
            Assert.Null(TradeRules.AnimalGroup(Cargo()));

            var odd = new Good { Id = "odd", Name = "odd", HasHorse = true };
            Assert.Equal("an animal TradeLord treats as ordinary cargo", TradeRules.AnimalGroup(odd));
        }

        [Fact]
        public void A_haul_animal_is_named_a_haul_animal_before_anything_else_it_might_also_be()
        {
            var both = new Good
            { Id = "both", Name = "both", HasHorse = true, IsHaulAnimal = true, IsSpareMount = true };
            Assert.Equal("a haul animal", TradeRules.AnimalGroup(both));
            Assert.Equal(TradeRules.RankPlainMount, TradeRules.HerdShedRank(both));
        }
        [Fact]
        public void A_horse_a_man_on_foot_can_ride_is_ridden_rather_than_driven()
        {
            Assert.Equal(0, Herding.MountsNobodyRides(4, 4));
            Assert.Equal(0, Herding.MountsNobodyRides(4, 9));
            Assert.Equal(3, Herding.MountsNobodyRides(7, 4));
        }

        [Fact]
        public void A_party_with_nobody_on_foot_drives_every_loose_mount()
        {
            Assert.Equal(6, Herding.MountsNobodyRides(6, 0));
        }

        [Fact]
        public void Nothing_counted_below_nothing_ever_shrinks_the_herd()
        {
            Assert.Equal(0, Herding.MountsNobodyRides(-3, 2));
            Assert.Equal(4, Herding.MountsNobodyRides(4, -2));
            Assert.Equal(4, Herding.DrivenInAll(-5, 6, 2));
            Assert.Equal(10, Herding.DrivenInAll(10, -1, -1));
        }

        [Fact]
        public void What_is_driven_is_the_herd_plus_the_mounts_nobody_rides()
        {
            Assert.Equal(12, Herding.DrivenInAll(10, 6, 4));
            Assert.Equal(10, Herding.DrivenInAll(10, 4, 4));
            Assert.Equal(10, Herding.DrivenInAll(10, 2, 9));
        }

        [Fact]
        public void Putting_a_man_on_foot_never_makes_the_herd_larger()
        {
            var rng = new System.Random(7741);
            for (int round = 0; round < 20000; round++)
            {
                int herd = rng.Next(0, 200);
                int mounts = rng.Next(0, 200);
                int foot = rng.Next(0, 200);
                int driven = Herding.DrivenInAll(herd, mounts, foot);
                Assert.True(Herding.DrivenInAll(herd, mounts, foot + 1) <= driven);
                Assert.True(driven >= herd);
                Assert.True(driven <= herd + mounts);
            }
        }

        [Fact]
        public void The_herd_guard_keeps_a_cushion_of_its_own()
        {
            Assert.True(Herding.Cushion > 0);
        }
    }
}
