using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class ShelfOrderTests
    {
        private static List<Pick> Shelved(params float[] keys)
        {
            var stock = new List<Pick>();
            for (int i = 0; i < keys.Length; i++)
                stock.Add(new Pick { At = i, Key = keys[i], Worth = 10f });
            return stock;
        }

        private static float[] KeysOf(List<Pick> stock)
        {
            var said = new float[stock.Count];
            for (int i = 0; i < stock.Count; i++) said[i] = stock[i].Key;
            return said;
        }

        private static float[] WorthOf(List<Pick> stock)
        {
            var said = new float[stock.Count];
            for (int i = 0; i < stock.Count; i++) said[i] = stock[i].Worth;
            return said;
        }

        [Fact]
        public void The_pick_with_the_most_profit_a_day_for_each_unit_of_weight_is_bought_first()
        {
            var stock = Shelved(0.1f, 0.9f, 0.4f);
            Picks.LedgerFirstThenKey(stock);
            Assert.Equal(new[] { 0.9f, 0.4f, 0.1f }, KeysOf(stock));
        }

        [Fact]
        public void A_shelf_already_in_order_is_left_in_it()
        {
            var stock = Shelved(0.9f, 0.4f, 0.1f);
            Picks.LedgerFirstThenKey(stock);
            Assert.Equal(new[] { 0.9f, 0.4f, 0.1f }, KeysOf(stock));
        }

        [Fact]
        public void Two_picks_with_the_same_key_go_by_what_they_would_make()
        {
            var stock = new List<Pick>
            {
                new Pick { At = 0, Key = 5f, Worth = 40f },
                new Pick { At = 1, Key = 5f, Worth = 90f },
                new Pick { At = 2, Key = 7f, Worth = 10f }
            };
            Picks.LedgerFirstThenKey(stock);
            Assert.Equal(new[] { 10f, 90f, 40f }, WorthOf(stock));
        }

        [Fact]
        public void An_empty_shelf_and_no_shelf_at_all_are_both_taken_in_their_stride()
        {
            var stock = new List<Pick>();
            Picks.LedgerFirstThenKey(stock);
            Assert.Empty(stock);
            Picks.LedgerFirstThenKey(null);
        }

        [Fact]
        public void A_light_costly_good_goes_before_a_heavy_cheap_one_whether_or_not_anything_runs_short()
        {
            float velvet = TradeMath.Key(774, 0.37f, 1f, true, 1f, 3, 0.7f);
            float wine = TradeMath.Key(1236, 0.2f, 1f, true, 2f, 9, 0.8f);
            var stock = new List<Pick>
            {
                new Pick { At = 0, Key = wine, Worth = 1236f, Hauls = 18f, Costs = 883 },
                new Pick { At = 1, Key = velvet, Worth = 774f, Hauls = 3f, Costs = 1239 }
            };
            Picks.LedgerFirstThenKey(stock);
            Assert.Equal(1, stock[0].At);
            stock.Reverse();
            Picks.LedgerFirstThenKey(stock);
            Assert.Equal(1, stock[0].At);
        }

        [Fact]
        public void Nothing_is_lost_or_invented_however_the_shelf_arrives()
        {
            var rng = new Random(2276);
            for (int round = 0; round < 20000; round++)
            {
                int count = rng.Next(0, 9);
                var stock = new List<Pick>();
                for (int i = 0; i < count; i++)
                    stock.Add(new Pick { At = i, Key = (float)(rng.NextDouble() * 50d), Worth = rng.Next(0, 9) });
                Picks.LedgerFirstThenKey(stock);
                Assert.Equal(count, stock.Count);
                var seen = new HashSet<int>();
                for (int i = 0; i < stock.Count; i++) Assert.True(seen.Add(stock[i].At));
                for (int i = 1; i < stock.Count; i++)
                    Assert.True(stock[i - 1].Key >= stock[i].Key);
            }
        }

        private static List<Pick> Asked(params (float key, int rank)[] shelf)
        {
            var stock = new List<Pick>();
            for (int i = 0; i < shelf.Length; i++)
                stock.Add(new Pick { At = i, Key = shelf[i].key, LedgerRank = shelf[i].rank });
            return stock;
        }

        [Fact]
        public void The_good_the_ledger_offered_at_this_market_is_bought_before_one_with_a_higher_key()
        {
            var stock = Asked((2.4f, 0), (0.87f, 1), (1.1f, 0));
            Picks.LedgerFirstThenKey(stock);
            Assert.Equal(new[] { 0.87f, 2.4f, 1.1f }, KeysOf(stock));
        }

        [Fact]
        public void The_ledger_rows_keep_the_ledgers_order_and_the_rest_go_by_key()
        {
            var stock = Asked((2.4f, 0), (0.87f, 2), (1.1f, 0), (0.9f, 1));
            Picks.LedgerFirstThenKey(stock);
            Assert.Equal(new[] { 0.9f, 0.87f, 2.4f, 1.1f }, KeysOf(stock));
        }

        [Fact]
        public void A_shelf_the_ledger_says_nothing_about_goes_by_key_alone()
        {
            var stock = Asked((1.1f, 0), (2.4f, 0), (0.4f, 0));
            Picks.LedgerFirstThenKey(stock);
            Assert.Equal(new[] { 2.4f, 1.1f, 0.4f }, KeysOf(stock));
        }

        [Fact]
        public void Nothing_is_lost_or_invented_when_the_ledger_has_its_say()
        {
            var rng = new Random(4471);
            for (int round = 0; round < 20000; round++)
            {
                int count = rng.Next(0, 9);
                var stock = new List<Pick>();
                for (int i = 0; i < count; i++)
                    stock.Add(new Pick
                    {
                        At = i,
                        Key = (float)(rng.NextDouble() * 20d),
                        LedgerRank = rng.Next(0, 3) == 0 ? i + 1 : 0
                    });
                Picks.LedgerFirstThenKey(stock);
                Assert.Equal(count, stock.Count);
                var seen = new HashSet<int>();
                bool past = false;
                for (int i = 0; i < stock.Count; i++)
                {
                    Assert.True(seen.Add(stock[i].At));
                    if (stock[i].LedgerRank == 0) past = true;
                    else Assert.False(past);
                }
                for (int i = 1; i < stock.Count; i++)
                    if (stock[i - 1].LedgerRank == 0 && stock[i].LedgerRank == 0)
                        Assert.True(stock[i - 1].Key >= stock[i].Key);
                    else if (stock[i - 1].LedgerRank > 0 && stock[i].LedgerRank > 0)
                        Assert.True(stock[i - 1].LedgerRank < stock[i].LedgerRank);
            }
        }
    }
}
