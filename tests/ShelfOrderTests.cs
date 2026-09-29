using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class ShelfOrderTests
    {
        private static List<Pick> Shelved(params float[] margins)
        {
            var stock = new List<Pick>();
            for (int i = 0; i < margins.Length; i++)
                stock.Add(new Pick { At = i, Worth = margins[i] });
            return stock;
        }

        private static float[] WorthOf(List<Pick> stock)
        {
            var said = new float[stock.Count];
            for (int i = 0; i < stock.Count; i++) said[i] = stock[i].Worth;
            return said;
        }

        [Fact]
        public void The_pick_that_would_make_the_most_is_bought_first()
        {
            var stock = Shelved(0.1f, 0.9f, 0.4f);
            Picks.MostMoneyFirst(stock);
            Assert.Equal(new[] { 0.9f, 0.4f, 0.1f }, WorthOf(stock));
        }

        [Fact]
        public void A_shelf_already_in_order_is_left_in_it()
        {
            var stock = Shelved(0.9f, 0.4f, 0.1f);
            Picks.MostMoneyFirst(stock);
            Assert.Equal(new[] { 0.9f, 0.4f, 0.1f }, WorthOf(stock));
        }

        [Fact]
        public void A_pick_that_would_make_nothing_goes_behind_one_that_would()
        {
            var stock = Shelved(-0.2f, 0.05f, -0.9f, 0f);
            Picks.MostMoneyFirst(stock);
            Assert.Equal(new[] { 0.05f, 0f, -0.2f, -0.9f }, WorthOf(stock));
        }

        [Fact]
        public void An_empty_shelf_and_no_shelf_at_all_are_both_taken_in_their_stride()
        {
            var stock = new List<Pick>();
            Picks.MostMoneyFirst(stock);
            Assert.Empty(stock);
            Picks.MostMoneyFirst(null);
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
                    stock.Add(new Pick { At = i, Worth = (float)(rng.NextDouble() * 2d - 1d) });
                int before = stock.Count;
                Picks.MostMoneyFirst(stock);
                Assert.Equal(before, stock.Count);
                for (int i = 1; i < stock.Count; i++)
                    Assert.True(stock[i - 1].Worth >= stock[i].Worth);
            }
        }

        private static List<Pick> Carried(params (float worth, float hauls, int costs)[] shelf)
        {
            var stock = new List<Pick>();
            for (int i = 0; i < shelf.Length; i++)
                stock.Add(new Pick { At = i, Worth = shelf[i].worth, Hauls = shelf[i].hauls, Costs = shelf[i].costs });
            return stock;
        }

        [Fact]
        public void When_the_hold_runs_short_the_most_for_each_unit_of_room_goes_first()
        {
            var stock = Carried((140f, 20f, 200), (72f, 1f, 200));
            Picks.MostForWhatRunsShortFirst(stock, 20f, 100000);
            Assert.Equal(new[] { 72f, 140f }, WorthOf(stock));
        }

        [Fact]
        public void When_the_purse_runs_short_the_most_for_each_denar_goes_first()
        {
            var stock = Carried((133.5f, 3f, 300), (105f, 15f, 150));
            Picks.MostForWhatRunsShortFirst(stock, 1000f, 300);
            Assert.Equal(new[] { 105f, 133.5f }, WorthOf(stock));
        }

        [Fact]
        public void When_both_run_short_each_pick_is_held_to_whichever_it_takes_more_of()
        {
            var stock = Carried((100f, 10f, 100), (90f, 2f, 150), (80f, 1f, 20));
            Picks.MostForWhatRunsShortFirst(stock, 10f, 200);
            Assert.Equal(new[] { 80f, 90f, 100f }, WorthOf(stock));
        }

        [Fact]
        public void When_everything_fits_the_pick_that_makes_the_most_goes_first()
        {
            var stock = Carried((72f, 1f, 200), (140f, 20f, 200));
            Picks.MostForWhatRunsShortFirst(stock, 21f, 400);
            Assert.Equal(new[] { 140f, 72f }, WorthOf(stock));
        }

        [Fact]
        public void A_good_that_takes_no_room_goes_first_when_only_the_hold_runs_short()
        {
            var stock = Carried((500f, 40f, 400), (60f, 0f, 400), (300f, 30f, 400));
            Picks.MostForWhatRunsShortFirst(stock, 50f, 100000);
            Assert.Equal(new[] { 60f, 500f, 300f }, WorthOf(stock));
        }

        [Fact]
        public void A_pick_that_would_make_nothing_still_goes_behind_one_that_would_when_room_runs_short()
        {
            var stock = Carried((-5f, 1f, 10), (20f, 100f, 100), (0f, 0.5f, 5), (30f, 10f, 100));
            Picks.MostForWhatRunsShortFirst(stock, 50f, 100000);
            Assert.Equal(new[] { 30f, 20f, 0f, -5f }, WorthOf(stock));
            Picks.MostForWhatRunsShortFirst(null, 50f, 100);
        }

        [Fact]
        public void Nothing_is_lost_or_invented_whatever_runs_short()
        {
            var rng = new Random(9127);
            for (int round = 0; round < 20000; round++)
            {
                int count = rng.Next(0, 9);
                var stock = new List<Pick>();
                for (int i = 0; i < count; i++)
                    stock.Add(new Pick
                    {
                        At = i,
                        Worth = (float)(rng.NextDouble() * 200d - 50d),
                        Hauls = (float)(rng.NextDouble() * 40d),
                        Costs = rng.Next(0, 500)
                    });
                float room = (float)(rng.NextDouble() * 120d);
                int purse = rng.Next(0, 1500);
                Picks.MostForWhatRunsShortFirst(stock, room, purse);
                Assert.Equal(count, stock.Count);
                var seen = new HashSet<int>();
                bool past = false;
                for (int i = 0; i < stock.Count; i++)
                {
                    Assert.True(seen.Add(stock[i].At));
                    if (!(stock[i].Worth > 0f)) past = true;
                    else Assert.False(past);
                }
            }
        }

        private static List<Pick> Asked(params (float margin, int rank)[] shelf)
        {
            var stock = new List<Pick>();
            for (int i = 0; i < shelf.Length; i++)
                stock.Add(new Pick { At = i, Worth = shelf[i].margin, LedgerRank = shelf[i].rank });
            return stock;
        }

        [Fact]
        public void The_good_the_ledger_sent_you_for_is_bought_before_a_fatter_margin()
        {
            var stock = Asked((2.4f, 0), (0.87f, 1), (1.1f, 0));
            Picks.WhatTheLedgerAskedForFirst(stock);
            Assert.Equal(new[] { 0.87f, 2.4f, 1.1f }, WorthOf(stock));
        }

        [Fact]
        public void Everything_else_keeps_the_order_the_margins_put_it_in()
        {
            var stock = Asked((2.4f, 0), (0.87f, 2), (1.1f, 0), (0.4f, 1));
            Picks.WhatTheLedgerAskedForFirst(stock);
            Assert.Equal(new[] { 0.4f, 0.87f, 2.4f, 1.1f }, WorthOf(stock));
        }

        [Fact]
        public void The_route_the_ledger_scores_highest_is_bought_before_its_lower_ones()
        {
            var stock = Asked((0.2f, 3), (0.9f, 1), (0.5f, 2));
            Picks.WhatTheLedgerAskedForFirst(stock);
            Assert.Equal(new[] { 0.9f, 0.5f, 0.2f }, WorthOf(stock));

            var every = Asked((2.4f, 2), (1.1f, 1));
            Picks.WhatTheLedgerAskedForFirst(every);
            Assert.Equal(new[] { 1.1f, 2.4f }, WorthOf(every));
        }

        [Fact]
        public void A_shelf_the_ledger_says_nothing_about_is_left_exactly_as_it_was()
        {
            var stock = Asked((2.4f, 0), (1.1f, 0), (0.4f, 0));
            Picks.WhatTheLedgerAskedForFirst(stock);
            Assert.Equal(new[] { 2.4f, 1.1f, 0.4f }, WorthOf(stock));

            var none = new List<Pick>();
            Picks.WhatTheLedgerAskedForFirst(none);
            Assert.Empty(none);
            Picks.WhatTheLedgerAskedForFirst(null);
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
                        Worth = (float)(rng.NextDouble() * 2d - 1d),
                        LedgerRank = rng.Next(0, 3) == 0 ? i + 1 : 0
                    });
                Picks.MostMoneyFirst(stock);
                var was = new List<Pick>(stock);
                Picks.WhatTheLedgerAskedForFirst(stock);
                Assert.Equal(was.Count, stock.Count);
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
                        Assert.True(stock[i - 1].Worth >= stock[i].Worth);
                    else if (stock[i - 1].LedgerRank > 0 && stock[i].LedgerRank > 0)
                        Assert.True(stock[i - 1].LedgerRank < stock[i].LedgerRank);
            }
        }
    }
}
