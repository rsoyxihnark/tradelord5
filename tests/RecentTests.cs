using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class RecentTests
    {
        [Fact]
        public void The_newest_trade_is_the_one_at_the_top()
        {
            var held = new List<string>();
            Recent.Keep(held, "first", 5);
            Recent.Keep(held, "second", 5);
            Recent.Keep(held, "third", 5);
            Assert.Equal(new[] { "third", "second", "first" }, held);
        }

        [Fact]
        public void The_oldest_trade_falls_off_once_the_list_is_full()
        {
            var held = new List<int>();
            for (int i = 1; i <= 5; i++) Recent.Keep(held, i, 3);
            Assert.Equal(new[] { 5, 4, 3 }, held);
        }

        [Fact]
        public void A_list_that_is_already_too_long_is_brought_back_to_its_ceiling()
        {
            var held = new List<int> { 9, 8, 7, 6, 5 };
            Recent.Keep(held, 10, 2);
            Assert.Equal(new[] { 10, 9 }, held);
        }

        [Fact]
        public void A_ceiling_of_nothing_keeps_nothing_and_never_throws()
        {
            var held = new List<int>();
            Recent.Keep(held, 1, 0);
            Recent.Keep(held, 2, -3);
            Assert.Empty(held);
        }

        [Fact]
        public void Nowhere_to_keep_a_trade_is_taken_in_its_stride()
        {
            Recent.Keep<string>(null, "one", 5);
            Recent.Keep<string>(null, null, 0);
        }

        [Fact]
        public void The_number_TradeLord_keeps_is_enough_to_read_and_small_enough_to_hold()
        {
            Assert.True(Recent.MostKept > 0);
            Assert.True(Recent.MostKept <= 50);
        }

        [Fact]
        public void Gold_gained_is_marked_and_gold_spent_keeps_its_own_sign()
        {
            Assert.Equal("+500", Recent.Coins(500));
            Assert.Equal("-320", Recent.Coins(-320));
            Assert.Equal("0", Recent.Coins(0));
            Assert.Equal("+1", Recent.Coins(1));
        }

        [Fact]
        public void The_list_never_outgrows_its_ceiling_and_always_holds_the_latest()
        {
            var rng = new Random(9142);
            for (int round = 0; round < 20000; round++)
            {
                int most = rng.Next(1, 8);
                int pushed = rng.Next(0, 25);
                var held = new List<int>();
                for (int i = 0; i < pushed; i++) Recent.Keep(held, i, most);
                Assert.True(held.Count <= most);
                Assert.Equal(Math.Min(pushed, most), held.Count);
                for (int i = 0; i < held.Count; i++) Assert.Equal(pushed - 1 - i, held[i]);
            }
        }
    }
}
