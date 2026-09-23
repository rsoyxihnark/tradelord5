using System;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class DealTests
    {
        [Fact]
        public void The_count_the_game_hands_over_is_taken_when_it_has_one()
        {
            Assert.Equal(29, Deals.UnitsMoved(29, 1742, 60));
            Assert.Equal(1, Deals.UnitsMoved(1, 105, 105));
            Assert.Equal(3, Deals.UnitsMoved(3, 813, 271));
        }

        [Fact]
        public void With_no_count_to_go_on_the_units_come_from_the_gold_and_the_price()
        {
            Assert.Equal(29, Deals.UnitsMoved(0, 1742, 60));
            Assert.Equal(1, Deals.UnitsMoved(0, 105, 105));
            Assert.Equal(3, Deals.UnitsMoved(0, 813, 271));
        }

        [Fact]
        public void A_line_worth_less_than_one_unit_still_counts_as_one()
        {
            Assert.Equal(1, Deals.UnitsMoved(0, 50, 60));
            Assert.Equal(1, Deals.UnitsMoved(0, 1, 9999));
        }

        [Fact]
        public void A_line_with_nothing_on_it_moves_nothing()
        {
            Assert.Equal(0, Deals.UnitsMoved(0, 0, 60));
            Assert.Equal(0, Deals.UnitsMoved(-3, 0, 60));
            Assert.Equal(1, Deals.UnitsMoved(0, 500, 0));
        }

        [Fact]
        public void A_purchase_is_written_down_at_the_gold_it_cost()
        {
            Assert.Equal(300, Deals.PaidForWhatYouKept(300, 10, 10));
            Assert.Equal(300, Deals.PaidForWhatYouKept(300, 10, 12));
        }

        [Fact]
        public void Only_the_share_of_a_purchase_you_still_carry_is_written_down()
        {
            Assert.Equal(150, Deals.PaidForWhatYouKept(300, 10, 5));
            Assert.Equal(33, Deals.PaidForWhatYouKept(100, 3, 1));
        }

        [Fact]
        public void A_purchase_with_nothing_on_it_writes_nothing_down()
        {
            Assert.Equal(0, Deals.PaidForWhatYouKept(300, 10, 0));
            Assert.Equal(0, Deals.PaidForWhatYouKept(0, 10, 5));
            Assert.Equal(0, Deals.PaidForWhatYouKept(300, 0, 5));
        }

        [Fact]
        public void The_deal_that_broke_the_ledger_no_longer_adds_up_and_says_so()
        {
            Assert.False(Deals.AddsUp(96403 - 297341, 1039));
        }

        [Fact]
        public void The_deal_as_it_really_was_adds_up_against_the_purse()
        {
            Assert.True(Deals.AddsUp(2031 - 992, 1039));
            Assert.True(Deals.AddsUp(0, 0));
        }

        [Fact]
        public void A_denar_or_two_of_rounding_is_allowed_and_a_wild_figure_is_not()
        {
            Assert.True(Deals.AddsUp(1041, 1039));
            Assert.True(Deals.AddsUp(1037, 1039));
            Assert.False(Deals.AddsUp(3000, 1039));
            Assert.False(Deals.AddsUp(-1039, 1039));
        }

        [Fact]
        public void A_large_deal_is_allowed_the_same_small_share_of_slack()
        {
            Assert.True(Deals.AddsUp(100_500, 100_000));
            Assert.False(Deals.AddsUp(120_000, 100_000));
        }

        [Fact]
        public void Profit_can_never_be_more_than_the_sale_that_earned_it()
        {
            Assert.Equal(816, Deals.NoMoreThanTheSale(816, 2031));
            Assert.Equal(2031, Deals.NoMoreThanTheSale(20417, 2031));
            Assert.Equal(0, Deals.NoMoreThanTheSale(-5, 2031));
            Assert.Equal(0, Deals.NoMoreThanTheSale(500, 0));
        }

        [Fact]
        public void Nothing_reckoned_from_a_line_ever_outruns_what_the_purse_did()
        {
            var rng = new Random(8812);
            for (int round = 0; round < 20000; round++)
            {
                int gained = rng.Next(0, 50000);
                int spent = rng.Next(0, 50000);
                int purse = gained - spent;
                Assert.True(Deals.AddsUp(gained - spent, purse));
                int profit = Deals.NoMoreThanTheSale(rng.Next(-100, 80000), gained);
                Assert.True(profit >= 0);
                Assert.True(gained == 0 ? profit == 0 : profit <= gained);
            }
        }

        [Fact]
        public void A_trade_from_today_reads_as_today_and_an_older_one_counts_the_days()
        {
            Assert.Equal(0, Recent.DaysAgo(91082f, 91082f));
            Assert.Equal(0, Recent.DaysAgo(91082.9f, 91082f));
            Assert.Equal(3, Recent.DaysAgo(91079f, 91082f));
            Assert.Equal(1, Recent.DaysAgo(91081.2f, 91082.4f));
        }

        [Fact]
        public void A_day_the_game_cannot_work_out_reads_as_today_rather_than_a_wild_number()
        {
            Assert.Equal(0, Recent.DaysAgo(float.NaN, 91082f));
            Assert.Equal(0, Recent.DaysAgo(91082f, float.NaN));
            Assert.Equal(0, Recent.DaysAgo(float.PositiveInfinity, 91082f));
        }
    }
}
