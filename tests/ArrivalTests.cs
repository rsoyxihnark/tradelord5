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
        public void A_sitting_is_the_same_only_at_the_same_market_in_the_same_hour()
        {
            Assert.True(Arrivals.StillTheSameSitting("town_ES3", "town_ES3", 400, 400));
            Assert.False(Arrivals.StillTheSameSitting("town_ES3", "town_ES3", 401, 400));
            Assert.False(Arrivals.StillTheSameSitting("town_ES3", "town_V8", 400, 400));
            Assert.False(Arrivals.StillTheSameSitting(null, "town_ES3", 400, 400));
            Assert.False(Arrivals.StillTheSameSitting(null, null, 400, 400));
        }
    }
}
