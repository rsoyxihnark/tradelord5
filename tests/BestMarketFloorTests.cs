using System;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class BestMarketFloorTests
    {
        [Fact]
        public void The_floor_is_the_other_market_price_less_the_tolerance_you_set()
        {
            Assert.Equal(90, TradeRules.BestMarketFloor(100, 0.9f));
            Assert.Equal(100, TradeRules.BestMarketFloor(100, 1f));
            Assert.Equal(50, TradeRules.BestMarketFloor(100, 0.5f));
        }

        [Fact]
        public void A_part_denar_floor_is_cut_off_rather_than_rounded_up()
        {
            Assert.Equal(94, TradeRules.BestMarketFloor(105, 0.9f));
            Assert.Equal(7, TradeRules.BestMarketFloor(15, 0.5f));
        }

        [Fact]
        public void No_other_market_worth_naming_leaves_no_floor_to_clear()
        {
            Assert.Equal(0, TradeRules.BestMarketFloor(0, 0.9f));
            Assert.Equal(0, TradeRules.BestMarketFloor(100, 0f));
        }

        [Fact]
        public void A_price_under_the_floor_is_held_back_and_one_on_it_is_not()
        {
            Assert.True(TradeRules.BelowTheBestMarket(89, 90));
            Assert.False(TradeRules.BelowTheBestMarket(90, 90));
            Assert.False(TradeRules.BelowTheBestMarket(91, 90));
        }

        [Fact]
        public void With_no_floor_in_force_nothing_is_ever_held_back_for_a_better_market()
        {
            for (int price = 0; price <= 500; price += 25)
                Assert.False(TradeRules.BelowTheBestMarket(price, 0));
        }

        [Fact]
        public void Selling_stops_at_the_first_unit_that_falls_under_the_floor()
        {
            int floor = TradeRules.BestMarketFloor(100, 0.9f);
            int[] asTheLotDrains = { 96, 94, 92, 90, 88, 86 };
            int sold = 0;
            foreach (int price in asTheLotDrains)
            {
                if (TradeRules.BelowTheBestMarket(price, floor)) break;
                sold++;
            }
            Assert.Equal(4, sold);
        }

        [Fact]
        public void The_best_price_is_scaled_to_the_quality_you_carry()
        {
            Assert.Equal(140, TradeMath.AtThisQuality(200, 50, 35));
            Assert.Equal(240, TradeMath.AtThisQuality(200, 50, 60));
            Assert.Equal(200, TradeMath.AtThisQuality(200, 50, 50));
        }

        [Fact]
        public void A_worn_good_sells_wherever_a_plain_one_would()
        {
            int bestElsewhere = 200, here = 192, plainValue = 50, wornValue = 35;
            int wornHere = TradeMath.AtThisQuality(here, plainValue, wornValue);
            int wornFloor = TradeRules.BestMarketFloor(
                TradeMath.AtThisQuality(bestElsewhere, plainValue, wornValue), 0.95f);
            Assert.False(TradeRules.BelowTheBestMarket(here, TradeRules.BestMarketFloor(bestElsewhere, 0.95f)));
            Assert.False(TradeRules.BelowTheBestMarket(wornHere, wornFloor));
            Assert.True(TradeRules.BelowTheBestMarket(wornHere, TradeRules.BestMarketFloor(bestElsewhere, 0.95f)));
        }

        [Fact]
        public void A_price_with_nothing_to_scale_it_by_is_left_as_it_is()
        {
            Assert.Equal(0, TradeMath.AtThisQuality(0, 50, 35));
            Assert.Equal(200, TradeMath.AtThisQuality(200, 0, 35));
            Assert.Equal(200, TradeMath.AtThisQuality(200, 50, 0));
        }

        [Fact]
        public void A_scaled_price_stays_between_one_denar_and_what_an_int_holds()
        {
            Assert.Equal(1, TradeMath.AtThisQuality(1, 100, 1));
            Assert.Equal(int.MaxValue, TradeMath.AtThisQuality(int.MaxValue, 1, 3));
        }

        [Fact]
        public void A_lower_tolerance_never_holds_back_more_than_a_higher_one()
        {
            var rng = new Random(5540);
            for (int round = 0; round < 20000; round++)
            {
                int elsewhere = rng.Next(1, 4000);
                float loose = 0.5f + (float)rng.NextDouble() * 0.25f;
                float tight = loose + (float)rng.NextDouble() * 0.25f;
                int price = rng.Next(0, 4000);
                bool heldLoosely = TradeRules.BelowTheBestMarket(price, TradeRules.BestMarketFloor(elsewhere, loose));
                bool heldTightly = TradeRules.BelowTheBestMarket(price, TradeRules.BestMarketFloor(elsewhere, tight));
                if (heldLoosely) Assert.True(heldTightly);
            }
        }
    }
}
