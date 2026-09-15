using System;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class TwinsTests
    {
        private static readonly DateTime Noon = new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void A_file_with_no_stamp_at_all_is_taken_as_edited_by_hand()
        {
            Assert.True(Twins.ChangedByHand(Noon, default(DateTime)));
        }

        [Fact]
        public void A_file_written_when_it_says_it_was_is_not_an_edit_by_hand()
        {
            Assert.False(Twins.ChangedByHand(Noon, Noon));
            Assert.False(Twins.ChangedByHand(Noon.AddSeconds(29), Noon));
        }

        [Fact]
        public void A_file_touched_after_the_tolerance_is_an_edit_by_hand()
        {
            Assert.True(Twins.ChangedByHand(Noon.AddSeconds(31), Noon));
            Assert.True(Twins.ChangedByHand(Noon.AddHours(3), Noon));
        }

        [Fact]
        public void The_tolerance_is_generous_enough_for_a_slow_write_and_no_more()
        {
            Assert.True(Twins.HandTolerance > TimeSpan.Zero);
            Assert.False(Twins.ChangedByHand(Noon + Twins.HandTolerance, Noon));
            Assert.True(Twins.ChangedByHand(Noon + Twins.HandTolerance + TimeSpan.FromSeconds(1), Noon));
        }

        [Fact]
        public void A_file_older_than_its_own_stamp_is_not_an_edit_by_hand()
        {
            Assert.False(Twins.ChangedByHand(Noon.AddHours(-5), Noon));
        }

        [Fact]
        public void The_screen_wins_only_when_it_is_there_and_wrote_the_file_last()
        {
            Assert.True(Twins.ScreenWins(true, true, false));
            Assert.False(Twins.ScreenWins(false, true, false));
            Assert.False(Twins.ScreenWins(true, false, false));
            Assert.False(Twins.ScreenWins(true, true, true));
        }

        [Fact]
        public void An_edit_by_hand_always_beats_the_screen()
        {
            for (int i = 0; i < 4; i++)
                Assert.False(Twins.ScreenWins((i & 1) != 0, (i & 2) != 0, true));
        }

        [Fact]
        public void Nothing_the_screen_never_wrote_is_ever_overwritten_by_it()
        {
            var rng = new System.Random(5127);
            for (int round = 0; round < 20000; round++)
            {
                bool inHand = rng.Next(2) == 0;
                bool changed = rng.Next(2) == 0;
                Assert.False(Twins.ScreenWins(inHand, false, changed));
            }
        }
    }
}
