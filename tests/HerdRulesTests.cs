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
    }
}
