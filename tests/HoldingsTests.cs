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

        [Fact]
        public void The_limit_is_only_lifted_where_the_game_is_asking_about_your_own_clan()
        {
            Assert.True(Holdings.TheGameIsAskingAboutYou(3, 3, whileYouBuy: true));
            Assert.True(Holdings.TheGameIsAskingAboutYou(0, 0, whileYouBuy: true));
            Assert.False(Holdings.TheGameIsAskingAboutYou(2, 3, whileYouBuy: true));
            Assert.False(Holdings.TheGameIsAskingAboutYou(4, 3, whileYouBuy: true));
        }

        [Fact]
        public void A_clan_the_game_cannot_place_never_has_the_limit_lifted_for_it()
        {
            Assert.False(Holdings.TheGameIsAskingAboutYou(3, -1, whileYouBuy: true));
            Assert.False(Holdings.TheGameIsAskingAboutYou(-1, -1, whileYouBuy: true));
            Assert.False(Holdings.TheGameIsAskingAboutYou(0, -1, whileYouBuy: true));
        }

        [Fact]
        public void A_clan_sitting_at_your_own_tier_keeps_its_own_limit_when_you_are_not_buying()
        {
            for (int tier = 0; tier <= 6; tier++)
                Assert.False(Holdings.TheGameIsAskingAboutYou(tier, tier, whileYouBuy: false));
        }

        [Fact]
        public void Nothing_the_game_asks_lifts_a_limit_while_you_are_not_buying()
        {
            var rng = new System.Random(48802);
            for (int round = 0; round < 20000; round++)
                Assert.False(Holdings.TheGameIsAskingAboutYou(
                    rng.Next(-2, 9), rng.Next(0, 7), whileYouBuy: false));
        }

        [Fact]
        public void No_tier_but_your_own_is_ever_lifted_whatever_the_game_asks()
        {
            var rng = new System.Random(7715);
            for (int round = 0; round < 20000; round++)
            {
                int yours = rng.Next(0, 7);
                int asked = rng.Next(-2, 9);
                Assert.Equal(asked == yours,
                    Holdings.TheGameIsAskingAboutYou(asked, yours, whileYouBuy: true));
            }
        }

        [Fact]
        public void A_purchase_that_leaves_the_reserve_whole_is_not_warned_about()
        {
            Assert.False(Holdings.DipsIntoWhatYouHoldBack(1000, 6000, 5000));
            Assert.False(Holdings.DipsIntoWhatYouHoldBack(1000, 60000, 5000));
        }

        [Fact]
        public void A_purchase_that_eats_into_the_reserve_is_warned_about()
        {
            Assert.True(Holdings.DipsIntoWhatYouHoldBack(1001, 6000, 5000));
            Assert.True(Holdings.DipsIntoWhatYouHoldBack(6000, 6000, 5000));
        }

        [Fact]
        public void Holding_nothing_back_means_there_is_nothing_to_warn_about()
        {
            Assert.False(Holdings.DipsIntoWhatYouHoldBack(50000, 100, 0));
            Assert.False(Holdings.DipsIntoWhatYouHoldBack(50000, 100, -1));
            Assert.False(Holdings.DipsIntoWhatYouHoldBack(0, 100, 5000));
        }

        [Fact]
        public void The_warning_follows_what_is_left_rather_than_what_it_costs()
        {
            var rng = new System.Random(3308);
            for (int round = 0; round < 20000; round++)
            {
                int cost = rng.Next(1, 40000);
                int purse = rng.Next(0, 80000);
                int held = rng.Next(1, 20000);
                Assert.Equal(purse - cost < held, Holdings.DipsIntoWhatYouHoldBack(cost, purse, held));
            }
        }

        [Fact]
        public void A_workshop_the_game_charged_for_is_never_charged_again()
        {
            Assert.Equal(0, Holdings.StillOwedForTheWorkshop(120000, 120000));
            Assert.Equal(0, Holdings.StillOwedForTheWorkshop(120000, 1));
            Assert.Equal(0, Holdings.StillOwedForTheWorkshop(120000, 999999));
        }

        [Fact]
        public void A_workshop_the_game_handed_over_for_nothing_is_still_paid_for()
        {
            Assert.Equal(120000, Holdings.StillOwedForTheWorkshop(120000, 0));
            Assert.Equal(120000, Holdings.StillOwedForTheWorkshop(120000, -5));
        }

        [Fact]
        public void A_workshop_with_no_price_on_it_is_never_charged_for()
        {
            Assert.Equal(0, Holdings.StillOwedForTheWorkshop(0, 0));
            Assert.Equal(0, Holdings.StillOwedForTheWorkshop(-1, 0));
        }

        [Fact]
        public void Nothing_is_ever_charged_twice_however_the_game_behaves()
        {
            var rng = new System.Random(6193);
            for (int round = 0; round < 20000; round++)
            {
                int cost = rng.Next(-100, 300000);
                int paid = rng.Next(-100, 300000);
                int owed = Holdings.StillOwedForTheWorkshop(cost, paid);
                Assert.True(owed == 0 || (paid <= 0 && owed == cost));
                Assert.True(owed >= 0);
            }
        }
    }
}
