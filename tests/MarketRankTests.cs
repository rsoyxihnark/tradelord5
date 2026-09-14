using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class MarketRankTests
    {
        private static Reach<string> At(string town, int price, float straight, float days) =>
            new Reach<string> { Where = town, Price = price, Straight = straight, Days = days };

        private static List<string> Named(List<Reach<string>> reached)
        {
            var said = new List<string>();
            foreach (Reach<string> one in reached) said.Add(one.Where);
            return said;
        }

        private static List<Reach<string>> Kept(bool selling, params Reach<string>[] offered)
        {
            var kept = new List<Reach<string>>(MarketRank.TopCacheSize + 1);
            foreach (Reach<string> one in offered) MarketRank.Keep(kept, one, selling);
            return kept;
        }

        [Fact]
        public void Selling_puts_the_market_that_pays_most_first()
        {
            List<Reach<string>> kept = Kept(true, At("a", 100, 1f, 1f), At("b", 300, 1f, 1f),
                                            At("c", 200, 1f, 1f));
            Assert.Equal(new[] { "b", "c", "a" }, Named(MarketRank.Settled(kept, true)));
        }

        [Fact]
        public void Buying_puts_the_market_that_charges_least_first()
        {
            List<Reach<string>> kept = Kept(false, At("a", 100, 1f, 1f), At("b", 300, 1f, 1f),
                                            At("c", 200, 1f, 1f));
            Assert.Equal(new[] { "a", "c", "b" }, Named(MarketRank.Settled(kept, false)));
        }

        [Fact]
        public void Two_markets_at_the_same_price_are_split_by_the_nearer_one()
        {
            List<Reach<string>> kept = Kept(true, At("far", 200, 9f, 9f), At("near", 200, 2f, 2f));
            Assert.Equal(new[] { "near", "far" }, Named(MarketRank.Settled(kept, true)));
            Assert.Equal(0, MarketRank.Rank(true, 200, 3f, 200, 3f));
        }

        [Fact]
        public void Only_eight_markets_are_ever_kept()
        {
            var offered = new List<Reach<string>>();
            for (int i = 0; i < 40; i++) offered.Add(At("t" + i, 100 + i, 1f, 1f));
            Assert.Equal(MarketRank.TopCacheSize, Kept(false, offered.ToArray()).Count);
            Assert.Equal(8, MarketRank.TopCacheSize);
        }

        [Fact]
        public void A_market_worse_than_the_eight_already_kept_is_turned_away()
        {
            var kept = new List<Reach<string>>(MarketRank.TopCacheSize + 1);
            for (int i = 0; i < 8; i++) MarketRank.Keep(kept, At("t" + i, 100 + i, 1f, 1f), false);
            MarketRank.Keep(kept, At("dear", 500, 1f, 1f), false);
            Assert.DoesNotContain("dear", Named(kept));
        }

        [Fact]
        public void A_market_better_than_the_eight_already_kept_displaces_the_worst()
        {
            var kept = new List<Reach<string>>(MarketRank.TopCacheSize + 1);
            for (int i = 0; i < 8; i++) MarketRank.Keep(kept, At("t" + i, 100 + i, 1f, 1f), false);
            MarketRank.Keep(kept, At("cheap", 1, 1f, 1f), false);
            Assert.Equal("cheap", kept[0].Where);
            Assert.Equal(8, kept.Count);
            Assert.DoesNotContain("t7", Named(kept));
        }

        [Fact]
        public void The_eight_are_chosen_on_the_straight_line_and_ordered_on_the_real_ride()
        {
            List<Reach<string>> kept = Kept(true,
                At("overland", 200, straight: 1f, days: 9f),
                At("coastal", 200, straight: 8f, days: 2f));
            Assert.Equal(new[] { "overland", "coastal" }, Named(kept));
            Assert.Equal(new[] { "coastal", "overland" }, Named(MarketRank.Settled(kept, true)));
        }

        [Fact]
        public void Settling_the_order_leaves_what_was_kept_alone()
        {
            List<Reach<string>> kept = Kept(true, At("a", 100, 1f, 5f), At("b", 300, 1f, 1f));
            List<string> before = Named(kept);
            MarketRank.Settled(kept, true);
            Assert.Equal(before, Named(kept));
        }

        [Fact]
        public void A_village_is_held_to_its_own_travel_ceiling_when_that_is_the_shorter_one()
        {
            var s = new Options { MaxTravelDaysTown = 2.4f, MaxTravelDaysVillage = 1f };
            Assert.Equal(1f, MarketRank.Ceiling(village: true, s: s));
            Assert.Equal(2.4f, MarketRank.Ceiling(village: false, s: s));
            Assert.True(MarketRank.WithinCeiling(true, 0.9f, s));
            Assert.False(MarketRank.WithinCeiling(true, 1.5f, s));
            Assert.True(MarketRank.WithinCeiling(false, 1.5f, s));
        }

        [Fact]
        public void A_village_ceiling_longer_than_the_town_one_never_stretches_a_village()
        {
            var s = new Options { MaxTravelDaysTown = 2f, MaxTravelDaysVillage = 5f };
            Assert.Equal(2f, MarketRank.Ceiling(village: true, s: s));
        }

        [Fact]
        public void A_ceiling_of_zero_looks_as_far_as_it_likes()
        {
            var off = new Options { MaxTravelDaysTown = 0f, MaxTravelDaysVillage = 0f };
            Assert.True(MarketRank.WithinCeiling(false, 500f, off));
            Assert.True(MarketRank.WithinCeiling(true, 500f, off));
            var townOff = new Options { MaxTravelDaysTown = 0f, MaxTravelDaysVillage = 1f };
            Assert.Equal(1f, MarketRank.Ceiling(village: true, s: townOff));
            Assert.True(MarketRank.WithinCeiling(false, 500f, townOff));
        }

        [Fact]
        public void The_eight_it_keeps_are_the_eight_a_full_sort_would_have_picked()
        {
            var rng = new System.Random(4021);
            for (int round = 0; round < 20000; round++)
            {
                bool selling = rng.Next(2) == 0;
                var offered = new List<Reach<string>>();
                for (int i = 0; i < rng.Next(0, 25); i++)
                    offered.Add(At("t" + i, rng.Next(0, 12), rng.Next(0, 6), rng.Next(0, 6)));

                var kept = new List<Reach<string>>(MarketRank.TopCacheSize + 1);
                foreach (Reach<string> one in offered) MarketRank.Keep(kept, one, selling);

                var sorted = new List<Reach<string>>(offered);
                sorted.Sort((x, y) => MarketRank.Rank(selling, x.Price, x.Straight, y.Price, y.Straight));

                int want = offered.Count < MarketRank.TopCacheSize ? offered.Count : MarketRank.TopCacheSize;
                Assert.Equal(want, kept.Count);
                for (int i = 0; i < want; i++)
                    Assert.Equal(0, MarketRank.Rank(selling, kept[i].Price, kept[i].Straight,
                                                    sorted[i].Price, sorted[i].Straight));
            }
        }
    }
}
