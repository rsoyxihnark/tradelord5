using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class SettlingTests
    {
        [Fact]
        public void A_delay_of_nothing_never_holds_a_market_back()
        {
            Assert.False(Settling.StillHolding(0, 0f, out int left));
            Assert.Equal(0, left);
            Assert.False(Settling.StillHolding(-5, 0f, out _));
        }

        [Fact]
        public void A_campaign_older_than_the_delay_trades_at_once()
        {
            Assert.False(Settling.StillHolding(50, 50f, out _));
            Assert.False(Settling.StillHolding(50, 500f, out _));
        }

        [Fact]
        public void A_young_campaign_is_told_how_many_days_are_left()
        {
            Assert.True(Settling.StillHolding(50, 0f, out int fresh));
            Assert.Equal(50, fresh);
            Assert.True(Settling.StillHolding(50, 49.1f, out int nearly));
            Assert.Equal(1, nearly);
            Assert.True(Settling.StillHolding(50, 20.5f, out int half));
            Assert.Equal(30, half);
        }

        [Fact]
        public void A_market_held_back_is_never_told_it_has_no_days_left()
        {
            Assert.True(Settling.StillHolding(50, 49.9999f, out int left));
            Assert.Equal(1, left);
        }

        [Fact]
        public void A_campaign_age_the_game_cannot_work_out_never_holds_a_market_back()
        {
            Assert.False(Settling.StillHolding(50, float.NaN, out _));
            Assert.False(Settling.StillHolding(50, float.PositiveInfinity, out _));
            Assert.False(Settling.StillHolding(50, float.NegativeInfinity, out _));
        }

        [Fact]
        public void A_campaign_age_below_nothing_is_read_as_the_very_start()
        {
            Assert.True(Settling.StillHolding(50, -20f, out int left));
            Assert.Equal(50, left);
        }

        [Fact]
        public void The_days_left_are_always_between_one_and_the_delay_you_set()
        {
            var rng = new System.Random(1553);
            for (int round = 0; round < 20000; round++)
            {
                int wait = rng.Next(1, 100);
                float elapsed = (float)(rng.NextDouble() * 120d - 10d);
                if (!Settling.StillHolding(wait, elapsed, out int left)) continue;
                Assert.True(left >= 1 && left <= wait);
                Assert.True(elapsed < wait);
            }
        }

        [Fact]
        public void A_day_further_on_is_never_a_day_further_from_trading()
        {
            var rng = new System.Random(9042);
            for (int round = 0; round < 20000; round++)
            {
                int wait = rng.Next(1, 100);
                float elapsed = (float)(rng.NextDouble() * (double)wait);
                Settling.StillHolding(wait, elapsed, out int now);
                Settling.StillHolding(wait, elapsed + 1f, out int later);
                Assert.True(later <= now);
            }
        }
    }
}
