using System.Collections.Generic;
using System.Globalization;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class ScoringTests
    {
        private struct Promise
        {
            internal float AtHours;
            internal float WithinDays;
        }

        private static Promise Made(float atHours, float withinDays) =>
            new Promise { AtHours = atHours, WithinDays = withinDays };

        private static bool StillWorthKeeping(Promise one, float nowHours) =>
            !Scoring.TooOldToSay(one.WithinDays, one.AtHours, nowHours, out _);

        [Fact]
        public void A_promise_is_kept_once_however_often_the_panel_repeats_it()
        {
            var keeps = new Keeps<Promise>();
            keeps.Put("pravend", "grain", Made(0f, 3f));
            Assert.Equal(1, keeps.Count);
            keeps.Put("pravend", "grain", Made(24f, 4f));
            Assert.Equal(1, keeps.Count);
            keeps.Put("pravend", "wine", Made(0f, 3f));
            keeps.Put("epicrotea", "grain", Made(0f, 3f));
            Assert.Equal(3, keeps.Count);
        }

        [Fact]
        public void It_knows_which_good_it_already_holds_a_promise_for_and_where()
        {
            var keeps = new Keeps<Promise>();
            keeps.Put("pravend", "grain", Made(0f, 3f));
            Assert.True(keeps.Holds("pravend", "grain"));
            Assert.False(keeps.Holds("pravend", "wine"));
            Assert.False(keeps.Holds("epicrotea", "grain"));
            Assert.False(keeps.Holds(null, "grain"));
            Assert.False(keeps.Holds("pravend", null));
        }

        [Fact]
        public void Walking_into_a_market_takes_every_promise_it_held_for_that_market()
        {
            var keeps = new Keeps<Promise>();
            keeps.Put("pravend", "grain", Made(0f, 3f));
            keeps.Put("pravend", "wine", Made(0f, 3f));
            keeps.Put("epicrotea", "grain", Made(0f, 3f));
            Dictionary<string, Promise> here = keeps.TakeAt("pravend");
            Assert.Equal(2, here.Count);
            Assert.Equal(1, keeps.Count);
            Assert.Null(keeps.TakeAt("pravend"));
            Assert.Equal(1, keeps.Count);
        }

        [Fact]
        public void A_market_it_holds_nothing_for_gives_nothing_back()
        {
            var keeps = new Keeps<Promise>();
            Assert.Null(keeps.TakeAt("pravend"));
            Assert.Null(keeps.TakeAt(null));
            Assert.Equal(0, keeps.Count);
        }

        [Fact]
        public void A_new_campaign_starts_it_empty_again()
        {
            var keeps = new Keeps<Promise>();
            keeps.Put("pravend", "grain", Made(0f, 3f));
            keeps.Forget();
            Assert.Equal(0, keeps.Count);
            Assert.False(keeps.Holds("pravend", "grain"));
            Assert.Null(keeps.TakeAt("pravend"));
        }

        [Fact]
        public void It_fills_up_at_six_hundred()
        {
            var keeps = new Keeps<Promise>();
            Assert.Equal(600, Keeps<Promise>.Most);
            for (int i = 0; i < Keeps<Promise>.Most - 1; i++)
            {
                keeps.Put("pravend", "good" + i, Made(0f, 3f));
                Assert.False(keeps.Full);
            }
            keeps.Put("pravend", "last", Made(0f, 3f));
            Assert.True(keeps.Full);
            Assert.Equal(600, keeps.Count);
        }

        [Fact]
        public void A_full_store_that_can_free_nothing_says_there_is_no_room()
        {
            var keeps = new Keeps<Promise>();
            for (int i = 0; i < Keeps<Promise>.Most; i++) keeps.Put("pravend", "good" + i, Made(0f, 3f));
            Assert.False(keeps.Prune(one => StillWorthKeeping(one, 24f)));
            Assert.Equal(600, keeps.Count);
        }

        [Fact]
        public void A_full_store_that_can_drop_one_old_promise_makes_room()
        {
            var keeps = new Keeps<Promise>();
            for (int i = 0; i < Keeps<Promise>.Most - 1; i++) keeps.Put("pravend", "good" + i, Made(0f, 3f));
            keeps.Put("pravend", "stale", Made(-480f, 3f));
            Assert.True(keeps.Full);
            Assert.True(keeps.Prune(one => StillWorthKeeping(one, 24f)));
            Assert.Equal(599, keeps.Count);
            Assert.False(keeps.Holds("pravend", "stale"));
        }

        [Fact]
        public void Clearing_out_old_promises_drops_a_market_it_no_longer_holds_any_for()
        {
            var keeps = new Keeps<Promise>();
            keeps.Put("pravend", "grain", Made(-480f, 3f));
            keeps.Put("epicrotea", "grain", Made(-480f, 3f));
            keeps.Put("epicrotea", "wine", Made(0f, 3f));
            Assert.True(keeps.Prune(one => StillWorthKeeping(one, 24f)));
            Assert.Equal(1, keeps.Count);
            Assert.Null(keeps.TakeAt("pravend"));
            Dictionary<string, Promise> here = keeps.TakeAt("epicrotea");
            Assert.Single(here);
            Assert.True(here.ContainsKey("wine"));
        }

        [Fact]
        public void A_promise_stays_worth_scoring_for_twice_the_ride_and_a_day()
        {
            Assert.False(Scoring.TooOldToSay(3f, 0f, 168f, out float since));
            Assert.Equal(7f, since, 4);
            Assert.True(Scoring.TooOldToSay(3f, 0f, 192f, out since));
            Assert.Equal(8f, since, 4);
        }

        [Fact]
        public void A_promise_walked_into_before_it_was_made_counts_as_no_days_since()
        {
            Assert.False(Scoring.TooOldToSay(0f, 48f, 24f, out float since));
            Assert.Equal(0f, since);
        }

        [Fact]
        public void A_market_that_puts_no_price_on_a_good_scores_nothing()
        {
            Assert.Equal(Holding.NoPrice, Scoring.Weigh(100, 0, out float held));
            Assert.Equal(TradeMath.NoShareToGive, held);
            Assert.Equal(Holding.NoPrice, Scoring.Weigh(100, -5, out held));
            Assert.Equal(TradeMath.NoShareToGive, held);
        }

        [Fact]
        public void A_promise_of_nothing_leaves_nothing_to_hold_it_to()
        {
            Assert.Equal(Holding.NothingToHold, Scoring.Weigh(0, 100, out float held));
            Assert.Equal(TradeMath.NoShareToGive, held);
            Assert.Equal(Holding.NothingToHold, Scoring.Weigh(-20, 100, out held));
        }

        [Fact]
        public void What_the_market_pays_is_scored_as_a_share_of_what_was_promised()
        {
            Assert.Equal(Holding.Scored, Scoring.Weigh(100, 80, out float held));
            Assert.Equal(0.8f, held, 4);
            Assert.Equal(Holding.Scored, Scoring.Weigh(100, 200, out held));
            Assert.Equal(2f, held, 4);
        }

        [Fact]
        public void Fewer_units_landing_than_it_said_reads_as_fewer()
        {
            Outcome how = Scoring.Weigh(20, 10, 25, 0, Scoring.NoWorth, Scoring.NoWorth);
            Assert.Equal(15, how.Landed);
            Assert.Equal(-5, how.LandingOff);
            Assert.Equal("5 fewer than it said", Scoring.Counted(how.LandingOff));
        }

        [Fact]
        public void More_units_landing_than_it_said_reads_as_more()
        {
            Outcome how = Scoring.Weigh(20, 10, 40, 0, Scoring.NoWorth, Scoring.NoWorth);
            Assert.Equal(30, how.Landed);
            Assert.Equal(10, how.LandingOff);
            Assert.Equal("10 more than it said", Scoring.Counted(how.LandingOff));
        }

        [Fact]
        public void A_market_that_keeps_no_worth_for_a_kind_of_good_is_scored_on_units_alone()
        {
            Outcome how = Scoring.Weigh(20, 10, 25, 100, Scoring.NoWorth, 1080);
            Assert.False(how.WorthKept);
            Assert.Equal(15, how.Landed);
            Assert.Equal(TradeMath.NoShareToGive, how.Share);
            Assert.Equal("", Scoring.Shared(how.Share));
            Assert.False(Scoring.Weigh(20, 10, 25, 100, 1000, Scoring.NoWorth).WorthKept);
        }

        [Fact]
        public void How_far_the_worth_figure_was_off_is_scored_as_a_share_of_what_it_said()
        {
            Outcome how = Scoring.Weigh(20, 10, 25, 100, 1000, 1080);
            Assert.True(how.WorthKept);
            Assert.Equal(80, how.Moved);
            Assert.Equal(-20, how.WorthOff);
            Assert.Equal(0.2f, how.Share, 4);
            Assert.Equal(", 20% off", Scoring.Shared(how.Share));
        }

        [Fact]
        public void A_worth_figure_of_nothing_cannot_be_held_to_anything()
        {
            Outcome how = Scoring.Weigh(20, 10, 25, 0, 1000, 1080);
            Assert.True(how.WorthKept);
            Assert.Equal(80, how.Moved);
            Assert.Equal(TradeMath.NoShareToGive, how.Share);
            Assert.Equal("", Scoring.Shared(how.Share));
        }

        [Fact]
        public void Landing_exactly_what_it_said_reads_that_way()
        {
            Outcome how = Scoring.Weigh(20, 10, 30, 100, 1000, 1100);
            Assert.Equal(0, how.LandingOff);
            Assert.Equal("exactly what it said", Scoring.Counted(how.LandingOff));
            Assert.Equal(0, how.WorthOff);
            Assert.Equal(0f, how.Share);
        }

        [Fact]
        public void Each_confidence_band_keeps_its_own_score()
        {
            var bands = new BandTally();
            bands.Add(0.1f, 0.5f);
            bands.Add(0.3f, 0.9f);
            bands.Add(0.6f, 1f);
            bands.Add(0.9f, 1.1f);
            bands.Add(0.9f, 0.9f);
            Assert.Equal(1, bands.Scored(0));
            Assert.Equal(0.5f, bands.Held(0), 4);
            Assert.Equal(1, bands.Scored(1));
            Assert.Equal(1, bands.Scored(2));
            Assert.Equal(2, bands.Scored(3));
            Assert.Equal(1f, bands.Held(3), 4);
        }

        [Fact]
        public void A_band_nothing_landed_in_holds_nothing()
        {
            var bands = new BandTally();
            Assert.Equal(0, bands.Scored(2));
            Assert.Equal(0f, bands.Held(2));
            bands.Add(0.6f, 1f);
            bands.Forget();
            Assert.Equal(0, bands.Scored(2));
            Assert.Equal(0f, bands.Held(2));
        }

        [Fact]
        public void The_bands_are_named_by_the_confidence_they_cover()
        {
            Assert.Equal(4, TradeMath.Bands);
            Assert.Equal("Conf under 25%", Scoring.Banded(0));
            Assert.Equal("Conf 25% to 49%", Scoring.Banded(1));
            Assert.Equal("Conf 50% to 74%", Scoring.Banded(2));
            Assert.Equal("Conf 75% and over", Scoring.Banded(3));
        }

        [Fact]
        public void Shares_and_figures_read_the_same_whatever_the_player_s_own_numbers_look_like()
        {
            CultureInfo was = CultureInfo.CurrentCulture;
            var comma = (CultureInfo)CultureInfo.InvariantCulture.Clone();
            comma.NumberFormat.NumberDecimalSeparator = ",";
            try
            {
                CultureInfo.CurrentCulture = comma;
                Assert.Equal("1.5", Scoring.Figure(1.5f));
                Assert.Equal("2.0", Scoring.Figure(2f));
                Assert.Equal("0.0", Scoring.Figure(0.04f));
                Assert.Equal("50%", Scoring.Share(0.5f));
                Assert.Equal("81%", Scoring.Share(0.807f));
                Assert.Equal("0%", Scoring.Share(0f));
            }
            finally
            {
                CultureInfo.CurrentCulture = was;
            }
        }
    }
}
