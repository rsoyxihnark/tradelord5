using System;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class ExpiryTests
    {
        [Fact]
        public void A_shelf_the_forecast_never_sees_empty_is_not_discounted_at_all()
        {
            Assert.Equal(1f, Confidence.Holds(Confidence.NotKnown, 3f));
            Assert.Equal(1f, Confidence.Holds(-5f, 3f));
            Assert.Equal(1f, Confidence.Holds(float.NaN, 3f));
        }

        [Fact]
        public void A_shelf_that_empties_before_you_arrive_is_discounted_hardest()
        {
            float gone = Confidence.Holds(2f, 5f);
            Assert.Equal(gone, Confidence.Holds(0f, 0f));
            Assert.True(gone > 0f && gone < 0.5f);
        }

        [Fact]
        public void A_shelf_that_empties_the_moment_you_arrive_counts_as_gone()
        {
            Assert.Equal(Confidence.Holds(0f, 0f), Confidence.Holds(4f, 4f));
        }

        [Fact]
        public void The_longer_the_shelf_outlasts_your_arrival_the_less_it_is_discounted()
        {
            float a = Confidence.Holds(3f, 2f);
            float b = Confidence.Holds(6f, 2f);
            float c = Confidence.Holds(30f, 2f);
            Assert.True(a < b);
            Assert.True(b < c);
            Assert.True(c < 1f);
        }

        [Fact]
        public void A_discount_is_never_worse_than_gone_and_never_better_than_untouched()
        {
            var rng = new Random(4471);
            for (int round = 0; round < 20000; round++)
            {
                float left = (float)(rng.NextDouble() * 40d);
                float travel = (float)(rng.NextDouble() * 20d);
                float held = Confidence.Holds(left, travel);
                Assert.True(held >= Confidence.Holds(0f, 0f));
                Assert.True(held <= 1f);
            }
        }

        [Fact]
        public void A_negative_journey_is_read_as_no_journey_rather_than_extra_room()
        {
            Assert.Equal(Confidence.Holds(5f, 0f), Confidence.Holds(5f, -3f));
            Assert.Equal(Confidence.Holds(5f, 0f), Confidence.Holds(5f, float.NaN));
        }

        [Fact]
        public void A_route_whose_shelf_holds_is_no_longer_punished_for_the_caravans_going_there()
        {
            float withCaravansAndNoForecast =
                Confidence.Of(false, 0, 0, 100, 10, 2f, 6, -1f);
            float withCaravansAndAShelfThatHolds =
                Confidence.Of(false, 0, 0, 100, 10, 2f, 6, -1f, Confidence.NotKnown, 1f);
            Assert.Equal(withCaravansAndNoForecast, withCaravansAndAShelfThatHolds);

            float shelfHolds = Confidence.Of(false, 0, 0, 100, 10, 2f, 6, -1f, 400f, 1f);
            Assert.True(shelfHolds > withCaravansAndNoForecast);
        }

        [Fact]
        public void A_route_whose_shelf_empties_first_scores_below_one_that_lasts()
        {
            float dies = Confidence.Of(false, 0, 0, 100, 10, 2f, 0, -1f, 1f, 3f);
            float lasts = Confidence.Of(false, 0, 0, 100, 10, 2f, 0, -1f, 20f, 3f);
            Assert.True(dies < lasts);
        }

        [Fact]
        public void The_caravan_count_still_decides_it_wherever_the_forecast_is_off()
        {
            float few = Confidence.Of(false, 0, 0, 100, 10, 2f, 1, -1f);
            float many = Confidence.Of(false, 0, 0, 100, 10, 2f, 9, -1f);
            Assert.True(many < few);
        }
    }
}
