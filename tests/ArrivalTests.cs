using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class ArrivalTests
    {
        [Fact]
        public void Walking_back_through_the_same_gate_is_the_same_arrival()
        {
            Assert.True(Arrivals.StillTheSame("town_ES3", "town_ES3", false));
        }

        [Fact]
        public void Taking_to_the_road_ends_an_arrival_even_at_the_same_market()
        {
            Assert.False(Arrivals.StillTheSame("town_ES3", "town_ES3", true));
        }

        [Fact]
        public void A_different_market_is_never_the_same_arrival()
        {
            Assert.False(Arrivals.StillTheSame("town_ES3", "town_V8", false));
            Assert.False(Arrivals.StillTheSame("town_ES3", null, false));
            Assert.False(Arrivals.StillTheSame(null, "town_ES3", false));
            Assert.False(Arrivals.StillTheSame(null, null, false));
        }

        [Fact]
        public void The_road_is_taken_once_you_are_further_from_the_gate_than_the_threshold()
        {
            Assert.False(Arrivals.TakenToTheRoad(false, true, Arrivals.SetOffFromTheGate));
            Assert.True(Arrivals.TakenToTheRoad(false, true, Arrivals.SetOffFromTheGate + 0.01f));
        }

        [Fact]
        public void A_gate_nobody_wrote_down_never_starts_the_road()
        {
            Assert.False(Arrivals.TakenToTheRoad(false, false, 9000f));
        }

        [Fact]
        public void Once_the_road_is_taken_standing_still_does_not_untake_it()
        {
            Assert.True(Arrivals.TakenToTheRoad(true, true, 0f));
            Assert.True(Arrivals.TakenToTheRoad(true, false, 0f));
        }

        [Fact]
        public void A_sitting_is_the_same_only_at_the_same_market_within_the_hour()
        {
            Assert.True(Arrivals.StillTheSameSitting("town_ES3", "town_ES3", 400, 400));
            Assert.False(Arrivals.StillTheSameSitting("town_ES3", "town_ES3", 401, 400));
            Assert.False(Arrivals.StillTheSameSitting("town_ES3", "town_V8", 400, 400));
            Assert.False(Arrivals.StillTheSameSitting(null, "town_ES3", 400, 400));
            Assert.False(Arrivals.StillTheSameSitting(null, null, 400, 400));
        }

        [Fact]
        public void Straight_back_is_under_an_hour_after_the_last_time_whatever_the_clock_says()
        {
            Assert.True(Arrivals.StraightBack(401.05, 400.97));
            Assert.True(Arrivals.StraightBack(400.97, 400.02));
            Assert.True(Arrivals.StraightBack(400.5, 400.5));
            Assert.False(Arrivals.StraightBack(401.0, 400.0));
            Assert.False(Arrivals.StraightBack(401.5, 400.0));
        }

        [Fact]
        public void Nothing_is_straight_back_before_it_happened_or_when_it_never_did()
        {
            Assert.False(Arrivals.StraightBack(400.0, 400.5));
            Assert.False(Arrivals.StraightBack(0.5, -1d));
        }

        [Fact]
        public void Stepping_back_in_across_the_turn_of_the_hour_is_the_same_sitting()
        {
            Assert.True(Arrivals.StillTheSameSitting("town_ES3", "town_ES3", 401.05, 400.97));
            Assert.False(Arrivals.StillTheSameSitting("town_ES3", "town_V8", 401.05, 400.97));
            Assert.False(Arrivals.StillTheSameSitting("town_ES3", "town_ES3", 402.05, 400.97));
        }

        [Fact]
        public void Coming_back_to_the_market_it_last_traded_at_is_the_first_time_back()
        {
            Assert.True(Arrivals.FirstTimeBack("town_ES3", "town_ES3"));
        }

        [Fact]
        public void Any_other_market_or_none_at_all_is_not_the_first_time_back()
        {
            Assert.False(Arrivals.FirstTimeBack("town_ES3", "town_V8"));
            Assert.False(Arrivals.FirstTimeBack("town_ES3", null));
            Assert.False(Arrivals.FirstTimeBack(null, "town_ES3"));
            Assert.False(Arrivals.FirstTimeBack(null, null));
        }

        [Fact]
        public void Leaving_a_market_it_traded_at_makes_that_market_the_last_one_traded_at()
        {
            Assert.Equal("town_V8", Arrivals.LastTradedAt("town_V8", true, "town_ES3"));
            Assert.Equal("town_V8", Arrivals.LastTradedAt("town_V8", true, null));
        }

        [Fact]
        public void Leaving_without_a_trade_keeps_the_last_market_traded_at()
        {
            Assert.Equal("town_ES3", Arrivals.LastTradedAt("town_V8", false, "town_ES3"));
            Assert.Null(Arrivals.LastTradedAt("town_V8", false, null));
            Assert.Equal("town_ES3", Arrivals.LastTradedAt(null, true, "town_ES3"));
        }

        [Fact]
        public void A_trade_somewhere_else_lets_the_next_arrival_trade_but_a_visit_that_traded_nothing_does_not()
        {
            string last = Arrivals.LastTradedAt("town_ES3", true, null);
            Assert.False(Arrivals.FirstTimeBack("town_ES3", Arrivals.LastTradedAt("town_V8", true, last)));
            Assert.True(Arrivals.FirstTimeBack("town_ES3", Arrivals.LastTradedAt("town_V8", false, last)));
        }

        [Fact]
        public void A_trade_on_the_road_frees_the_market_it_last_traded_at()
        {
            string last = Arrivals.LastTradedAt("town_ES3", true, null);
            Assert.Null(Arrivals.AfterTheRoad(true, last));
            Assert.False(Arrivals.FirstTimeBack("town_ES3", Arrivals.AfterTheRoad(true, last)));
        }

        [Fact]
        public void The_map_marker_leaves_out_the_market_TradeLord_last_traded_at_until_you_come_back_to_it()
        {
            string last = Arrivals.LastTradedAt("town_ES3", true, null);
            Assert.True(Arrivals.FirstTimeBack("town_ES3", Arrivals.LastTradedAt(null, false, last)));
            Assert.False(Arrivals.FirstTimeBack("town_V8", Arrivals.LastTradedAt(null, false, last)));
            Assert.False(Arrivals.FirstTimeBack("town_ES3", Arrivals.LastTradedAt(null, false, null)));
            Assert.False(Arrivals.FirstTimeBack(null, Arrivals.LastTradedAt(null, false, last)));
        }

        [Fact]
        public void A_trade_where_you_stand_frees_the_market_traded_at_before_for_the_map_marker()
        {
            string last = Arrivals.LastTradedAt("town_B", true, null);
            Assert.False(Arrivals.FirstTimeBack("town_B", Arrivals.LastTradedAt("town_A", true, last)));
            Assert.True(Arrivals.FirstTimeBack("town_B", Arrivals.LastTradedAt("town_A", false, last)));
            Assert.True(Arrivals.FirstTimeBack("town_B", Arrivals.LastTradedAt(null, true, last)));
        }

        [Fact]
        public void Meeting_a_party_on_the_road_without_a_trade_keeps_the_market_it_last_traded_at()
        {
            string last = Arrivals.LastTradedAt("town_ES3", true, null);
            Assert.Equal("town_ES3", Arrivals.AfterTheRoad(false, last));
            Assert.True(Arrivals.FirstTimeBack("town_ES3", Arrivals.AfterTheRoad(false, last)));
            Assert.Null(Arrivals.AfterTheRoad(false, null));
        }

        [Fact]
        public void After_leaving_Poros_with_trading_on_arrival_on_its_routes_are_set_aside()
        {
            string lastTraded = Arrivals.LastTradedAt("town_EW1", true, null);
            Assert.True(Arrivals.LeftAloneOnArrival("town_EW1", null, true, lastTraded));
            Assert.False(Arrivals.LeftAloneOnArrival("town_EW1", null, false, lastTraded));
            Assert.False(Arrivals.LeftAloneOnArrival("town_EV4", null, true, lastTraded));
        }

        [Fact]
        public void The_market_you_stand_in_is_never_set_aside_even_after_trading_there()
        {
            Assert.False(Arrivals.LeftAloneOnArrival("town_EW1", "town_EW1", true, "town_EW1"));
            Assert.True(Arrivals.LeftAloneOnArrival("town_EW1", "town_EV4", true, "town_EW1"));
        }

        [Fact]
        public void A_new_campaigns_first_arrival_at_a_market_it_may_trade_at_is_its_first_visit()
        {
            Assert.Equal("town_ES3", Arrivals.FirstVisitAt(true, true, "town_ES3"));
            Assert.False(Arrivals.FirstVisitStillAhead(true, true));
        }

        [Fact]
        public void A_place_TradeLord_may_not_trade_at_leaves_the_first_visit_ahead()
        {
            Assert.Null(Arrivals.FirstVisitAt(true, false, "castle_ES3"));
            Assert.True(Arrivals.FirstVisitStillAhead(true, false));
        }

        [Fact]
        public void The_next_arrival_anywhere_ends_the_first_visit()
        {
            Assert.Null(Arrivals.FirstVisitAt(false, true, "town_ES3"));
            Assert.Null(Arrivals.FirstVisitAt(false, false, "castle_ES3"));
            Assert.False(Arrivals.FirstVisitStillAhead(false, true));
            Assert.False(Arrivals.FirstVisitStillAhead(false, false));
        }

        [Fact]
        public void Food_waits_for_the_next_market_only_while_the_first_visit_lasts()
        {
            Assert.True(Arrivals.FoodWaitsForTheNextMarket("town_ES3", "town_ES3"));
            Assert.False(Arrivals.FoodWaitsForTheNextMarket("town_V8", "town_ES3"));
            Assert.False(Arrivals.FoodWaitsForTheNextMarket("town_ES3", null));
            Assert.False(Arrivals.FoodWaitsForTheNextMarket(null, null));
        }

        [Fact]
        public void A_campaign_loaded_before_any_market_was_visited_still_has_its_first_visit_ahead()
        {
            Assert.True(Arrivals.FirstVisitAheadOnceLoaded(false, null, true));
            Assert.True(Arrivals.FirstVisitAheadOnceLoaded(true, null, true));
            Assert.True(Arrivals.FirstVisitAheadOnceLoaded(true, null, false));
        }

        [Fact]
        public void A_campaign_loaded_after_a_market_was_visited_or_inside_its_first_one_gets_no_second_first_visit()
        {
            Assert.False(Arrivals.FirstVisitAheadOnceLoaded(false, null, false));
            Assert.False(Arrivals.FirstVisitAheadOnceLoaded(false, "town_ES3", true));
            Assert.False(Arrivals.FirstVisitAheadOnceLoaded(false, "town_ES3", false));
        }
    }
}
