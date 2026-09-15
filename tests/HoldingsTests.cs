using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class HoldingsTests
    {
        [Fact]
        public void Asking_for_nothing_leaves_the_game_to_say_how_many_you_may_own()
        {
            Assert.Equal(3, Holdings.WorkshopsYouMayOwn(3, 0));
            Assert.Equal(0, Holdings.WorkshopsYouMayOwn(0, 0));
            Assert.Equal(6, Holdings.WorkshopsYouMayOwn(6, -2));
        }

        [Fact]
        public void The_number_you_ask_for_is_the_number_you_get()
        {
            Assert.Equal(200, Holdings.WorkshopsYouMayOwn(3, 200));
            Assert.Equal(1, Holdings.WorkshopsYouMayOwn(3, 1));
        }

        [Fact]
        public void There_is_room_for_one_more_until_you_are_at_the_ceiling()
        {
            Assert.True(Holdings.RoomForOneMore(0, 1));
            Assert.True(Holdings.RoomForOneMore(2, 3));
            Assert.False(Holdings.RoomForOneMore(3, 3));
            Assert.False(Holdings.RoomForOneMore(4, 3));
        }

        [Fact]
        public void A_ceiling_of_nothing_leaves_no_room_at_all()
        {
            Assert.False(Holdings.RoomForOneMore(0, 0));
        }

        [Fact]
        public void What_you_ask_for_never_depends_on_what_the_game_says()
        {
            var roll = new System.Random(3316);
            for (int i = 0; i < 20000; i++)
            {
                int gameSays = roll.Next(0, 12);
                int asked = roll.Next(-5, 201);
                int allowed = Holdings.WorkshopsYouMayOwn(gameSays, asked);
                Assert.Equal(asked > 0 ? asked : gameSays, allowed);
                Assert.Equal(allowed > 0, Holdings.RoomForOneMore(0, allowed));
            }
        }
    }
}
