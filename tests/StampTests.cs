using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class StampTests
    {
        [Fact]
        public void A_stamp_nobody_has_taken_is_never_fresh()
        {
            Stamp stamp = default(Stamp);
            Assert.False(stamp.Fresh(0, 0));
            Assert.False(stamp.Fresh(5, 3));
        }

        [Fact]
        public void A_stamp_just_taken_is_fresh_for_that_hour_and_that_generation()
        {
            Stamp stamp = default(Stamp);
            stamp.Taken(7, 3);
            Assert.True(stamp.Fresh(7, 3));
        }

        [Fact]
        public void The_hour_moving_on_makes_a_stamp_stale()
        {
            Stamp stamp = default(Stamp);
            stamp.Taken(7, 3);
            Assert.False(stamp.Fresh(8, 3));
        }

        [Fact]
        public void A_setting_changing_makes_a_stamp_stale_within_the_same_hour()
        {
            Stamp stamp = default(Stamp);
            stamp.Taken(7, 3);
            Assert.False(stamp.Fresh(7, 4));
        }

        [Fact]
        public void An_hour_that_comes_round_again_does_not_revive_a_stamp_of_an_older_generation()
        {
            Stamp stamp = default(Stamp);
            stamp.Taken(7, 3);
            Assert.False(stamp.Fresh(7, 4));
            Assert.False(stamp.Fresh(8, 3));
        }

        [Fact]
        public void Going_stale_by_hand_drops_a_stamp_that_was_fresh()
        {
            Stamp stamp = default(Stamp);
            stamp.Taken(7, 3);
            stamp.Stale();
            Assert.False(stamp.Fresh(7, 3));
        }

        [Fact]
        public void A_stamp_gone_stale_can_be_taken_again()
        {
            Stamp stamp = default(Stamp);
            stamp.Taken(7, 3);
            stamp.Stale();
            stamp.Taken(9, 5);
            Assert.True(stamp.Fresh(9, 5));
            Assert.False(stamp.Fresh(7, 3));
        }

        [Fact]
        public void A_timeless_stamp_keeps_through_every_hour_and_still_answers_to_a_setting()
        {
            Stamp stamp = default(Stamp);
            stamp.Taken(Stamp.Timeless, 3);
            Assert.True(stamp.Fresh(Stamp.Timeless, 3));
            Assert.False(stamp.Fresh(Stamp.Timeless, 4));
        }

        [Fact]
        public void Zero_is_a_real_hour_and_a_real_generation_rather_than_nothing_taken_yet()
        {
            Stamp stamp = default(Stamp);
            Assert.False(stamp.Fresh(0, 0));
            stamp.Taken(0, 0);
            Assert.True(stamp.Fresh(0, 0));
        }
    }
}
