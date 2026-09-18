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
                stock.Add(new Pick { At = i, Margin = margins[i] });
            return stock;
        }

        private static float[] MarginsOf(List<Pick> stock)
        {
            var said = new float[stock.Count];
            for (int i = 0; i < stock.Count; i++) said[i] = stock[i].Margin;
            return said;
        }

        [Fact]
        public void The_best_margin_is_bought_first()
        {
            var stock = Shelved(0.1f, 0.9f, 0.4f);
            Picks.BestMarginFirst(stock);
            Assert.Equal(new[] { 0.9f, 0.4f, 0.1f }, MarginsOf(stock));
        }

        [Fact]
        public void A_shelf_already_in_order_is_left_in_it()
        {
            var stock = Shelved(0.9f, 0.4f, 0.1f);
            Picks.BestMarginFirst(stock);
            Assert.Equal(new[] { 0.9f, 0.4f, 0.1f }, MarginsOf(stock));
        }

        [Fact]
        public void A_losing_margin_goes_behind_a_winning_one()
        {
            var stock = Shelved(-0.2f, 0.05f, -0.9f, 0f);
            Picks.BestMarginFirst(stock);
            Assert.Equal(new[] { 0.05f, 0f, -0.2f, -0.9f }, MarginsOf(stock));
        }

        [Fact]
        public void An_empty_shelf_and_no_shelf_at_all_are_both_taken_in_their_stride()
        {
            var stock = new List<Pick>();
            Picks.BestMarginFirst(stock);
            Assert.Empty(stock);
            Picks.BestMarginFirst(null);
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
                    stock.Add(new Pick { At = i, Margin = (float)(rng.NextDouble() * 2d - 1d) });
                int before = stock.Count;
                Picks.BestMarginFirst(stock);
                Assert.Equal(before, stock.Count);
                for (int i = 1; i < stock.Count; i++)
                    Assert.True(stock[i - 1].Margin >= stock[i].Margin);
            }
        }

        private static List<Pick> Asked(params (float margin, bool asked)[] shelf)
        {
            var stock = new List<Pick>();
            for (int i = 0; i < shelf.Length; i++)
                stock.Add(new Pick { At = i, Margin = shelf[i].margin, Asked = shelf[i].asked });
            return stock;
        }

        [Fact]
        public void The_good_the_ledger_sent_you_for_is_bought_before_a_fatter_margin()
        {
            var stock = Asked((2.4f, false), (0.87f, true), (1.1f, false));
            Picks.WhatTheLedgerAskedForFirst(stock);
            Assert.Equal(new[] { 0.87f, 2.4f, 1.1f }, MarginsOf(stock));
        }

        [Fact]
        public void Everything_else_keeps_the_order_the_margins_put_it_in()
        {
            var stock = Asked((2.4f, false), (0.87f, true), (1.1f, false), (0.4f, true));
            Picks.WhatTheLedgerAskedForFirst(stock);
            Assert.Equal(new[] { 0.87f, 0.4f, 2.4f, 1.1f }, MarginsOf(stock));
        }

        [Fact]
        public void A_shelf_the_ledger_says_nothing_about_is_left_exactly_as_it_was()
        {
            var stock = Asked((2.4f, false), (1.1f, false), (0.4f, false));
            Picks.WhatTheLedgerAskedForFirst(stock);
            Assert.Equal(new[] { 2.4f, 1.1f, 0.4f }, MarginsOf(stock));

            var every = Asked((2.4f, true), (1.1f, true));
            Picks.WhatTheLedgerAskedForFirst(every);
            Assert.Equal(new[] { 2.4f, 1.1f }, MarginsOf(every));

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
                        Margin = (float)(rng.NextDouble() * 2d - 1d),
                        Asked = rng.Next(0, 3) == 0
                    });
                Picks.BestMarginFirst(stock);
                var was = new List<Pick>(stock);
                Picks.WhatTheLedgerAskedForFirst(stock);
                Assert.Equal(was.Count, stock.Count);
                var seen = new HashSet<int>();
                bool past = false;
                for (int i = 0; i < stock.Count; i++)
                {
                    Assert.True(seen.Add(stock[i].At));
                    if (!stock[i].Asked) past = true;
                    else Assert.False(past);
                }
                for (int i = 1; i < stock.Count; i++)
                    if (stock[i - 1].Asked == stock[i].Asked)
                        Assert.True(stock[i - 1].Margin >= stock[i].Margin);
            }
        }
    }
}
