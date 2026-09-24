using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class HerdRulesTests
    {
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
