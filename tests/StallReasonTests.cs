using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class StallReasonTests
    {
        private static BlockTally Tallied(params Block[] met)
        {
            var tally = new BlockTally();
            foreach (Block one in met) tally.Note(one);
            return tally;
        }

        [Fact]
        public void A_pass_that_was_stopped_by_nothing_keeps_no_reason_at_all()
        {
            BlockTally tally = Tallied(Block.None, Block.None);
            Assert.False(tally.Any);
            Assert.False(tally.Saw(Block.None));
            Assert.Equal(Block.None, tally.Dominant());
            Assert.Equal("", tally.Summary());
        }

        [Fact]
        public void The_reason_that_stopped_the_most_goods_is_the_one_named()
        {
            BlockTally tally = Tallied(Block.BelowMargin, Block.NoStock, Block.NoStock);
            Assert.Equal(Block.NoStock, tally.Dominant());
        }

        [Fact]
        public void The_first_protection_met_is_named_rather_than_the_one_met_most()
        {
            BlockTally tally = Tallied(Block.Protected, Block.QuestGoods, Block.QuestGoods,
                                       Block.QuestGoods);
            Assert.Equal(Block.Protected, tally.Dominant());
        }

        [Fact]
        public void A_reason_that_is_not_a_protection_is_named_on_its_own_count()
        {
            BlockTally tally = Tallied(Block.Protected, Block.BelowMargin, Block.BelowMargin);
            Assert.Equal(Block.BelowMargin, tally.Dominant());
        }

        [Fact]
        public void Two_reasons_that_stopped_as_much_as_each_other_are_ranked_the_same_way_round()
        {
            Assert.Equal(Block.BelowMargin,
                         Tallied(Block.BelowMargin, Block.BelowBestMarket).Dominant());
            Assert.Equal(Block.BelowMargin,
                         Tallied(Block.BelowBestMarket, Block.BelowMargin).Dominant());
        }

        [Fact]
        public void What_a_good_simply_is_never_gets_named_as_the_reason_a_pass_stalled()
        {
            Assert.Equal(Block.None, Tallied(Block.NotTradable, Block.NotMerchandise,
                                             Block.MountOrHaulAnimal).Dominant());
            Assert.Equal(Block.BelowMargin,
                         Tallied(Block.NotTradable, Block.NotTradable, Block.BelowMargin).Dominant());
        }

        [Fact]
        public void An_empty_purse_is_the_last_reason_a_stalled_pass_names()
        {
            Assert.Equal(Block.BudgetSpent, Tallied(Block.BudgetSpent).Dominant());
            Assert.Equal(Block.BelowMargin,
                         Tallied(Block.BudgetSpent, Block.BudgetSpent, Block.BelowMargin).Dominant());
        }

        [Fact]
        public void The_summary_lists_every_reason_met_most_first()
        {
            BlockTally tally = Tallied(Block.BelowMargin, Block.NoStock, Block.NoStock);
            Assert.Equal("NoStock=2, BelowMargin=1", tally.Summary());
        }
    }
}
