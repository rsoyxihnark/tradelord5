using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class RankTests
    {
        private static int BandsTrueFor(float rank)
        {
            int said = 0;
            for (int band = 1; band <= Ranks.Bands; band++)
                if (Ranks.BandOf(rank) == band) said++;
            return said;
        }

        [Fact]
        public void The_best_route_ranks_at_the_top_and_the_last_at_the_bottom()
        {
            Assert.Equal(0f, Ranks.Of(0, 30));
            Assert.Equal(1f, Ranks.Of(29, 30));
        }

        [Fact]
        public void A_list_of_one_or_none_ranks_everything_at_the_top()
        {
            Assert.Equal(0f, Ranks.Of(0, 1));
            Assert.Equal(0f, Ranks.Of(0, 0));
            Assert.Equal(0f, Ranks.Of(5, 1));
            Assert.Equal(0f, Ranks.Of(-3, 30));
        }

        [Fact]
        public void The_top_of_the_list_takes_the_first_band_and_the_bottom_the_last()
        {
            Assert.Equal(1, Ranks.BandOf(0f));
            Assert.Equal(5, Ranks.BandOf(1f));
        }

        [Fact]
        public void Every_band_starts_where_the_one_before_it_ends()
        {
            Assert.Equal(1, Ranks.BandOf(0.19f));
            Assert.Equal(2, Ranks.BandOf(0.2f));
            Assert.Equal(2, Ranks.BandOf(0.44f));
            Assert.Equal(3, Ranks.BandOf(0.45f));
            Assert.Equal(3, Ranks.BandOf(0.69f));
            Assert.Equal(4, Ranks.BandOf(0.7f));
            Assert.Equal(4, Ranks.BandOf(0.84f));
            Assert.Equal(5, Ranks.BandOf(0.85f));
        }

        [Fact]
        public void A_row_is_never_two_bands_and_never_none()
        {
            var rng = new System.Random(4409);
            for (int round = 0; round < 20000; round++)
            {
                float rank = (float)(rng.NextDouble() * 3d - 1d);
                Assert.Equal(1, BandsTrueFor(rank));
            }
            Assert.Equal(1, BandsTrueFor(float.NaN));
            Assert.Equal(1, BandsTrueFor(float.PositiveInfinity));
            Assert.Equal(1, BandsTrueFor(float.NegativeInfinity));
        }

        [Fact]
        public void A_row_further_down_the_list_is_never_in_an_earlier_band()
        {
            var rng = new System.Random(3118);
            for (int round = 0; round < 20000; round++)
            {
                int count = rng.Next(1, 40);
                int at = rng.Next(0, count);
                float rank = Ranks.Of(at, count);
                Assert.True(rank >= 0f && rank <= 1f);
                if (at + 1 >= count) continue;
                Assert.True(Ranks.BandOf(Ranks.Of(at + 1, count)) >= Ranks.BandOf(rank));
            }
        }
    }
}
