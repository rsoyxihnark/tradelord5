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
            Assert.EndsWith("5 fewer on the shelf than it said", Scoring.UnitsShifted(20, how.Landed));
        }

        [Fact]
        public void More_units_landing_than_it_said_reads_as_more()
        {
            Outcome how = Scoring.Weigh(20, 10, 40, 0, Scoring.NoWorth, Scoring.NoWorth);
            Assert.Equal(30, how.Landed);
            Assert.Equal(10, how.LandingOff);
            Assert.EndsWith("10 more on the shelf than it said", Scoring.UnitsShifted(20, how.Landed));
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
            Assert.EndsWith("exactly what it said", Scoring.UnitsShifted(20, how.Landed));
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
        private static Reading Seen(string item, string town, float day) =>
            new Reading { Item = item, Town = town, Day = day };

        [Fact]
        public void A_book_inside_what_it_keeps_drops_nothing()
        {
            var held = new List<Reading> { Seen("grain", "a", 1f), Seen("wine", "b", 2f) };
            Assert.Empty(Kept.OldestBeyond(held, 5));
            Assert.Empty(Kept.OldestBeyond(held, 2));
        }

        [Fact]
        public void A_book_past_what_it_keeps_drops_the_oldest_first()
        {
            var held = new List<Reading>
            {
                Seen("grain", "a", 9f), Seen("wine", "b", 2f),
                Seen("olives", "c", 5f), Seen("oil", "d", 1f)
            };
            var dropped = Kept.OldestBeyond(held, 2);
            Assert.Equal(2, dropped.Count);
            Assert.Equal("oil", dropped[0].Item);
            Assert.Equal("wine", dropped[1].Item);
        }

        [Fact]
        public void A_book_with_no_ceiling_or_nothing_in_it_drops_nothing()
        {
            Assert.Empty(Kept.OldestBeyond(null, 10));
            Assert.Empty(Kept.OldestBeyond(new List<Reading>(), 10));
            Assert.Empty(Kept.OldestBeyond(new List<Reading> { Seen("grain", "a", 1f) }, 0));
            Assert.Empty(Kept.OldestBeyond(new List<Reading> { Seen("grain", "a", 1f) }, -4));
        }

        [Fact]
        public void What_is_left_after_a_trim_is_never_more_than_the_ceiling()
        {
            var rng = new System.Random(6613);
            for (int round = 0; round < 20000; round++)
            {
                int count = rng.Next(0, 60);
                int cap = rng.Next(1, 40);
                var held = new List<Reading>();
                for (int i = 0; i < count; i++)
                    held.Add(Seen("i" + i, "t" + i, (float)(rng.NextDouble() * 50d)));
                int dropped = Kept.OldestBeyond(held, cap).Count;
                Assert.True(count - dropped <= cap);
                Assert.True(dropped >= 0 && dropped <= count);
            }
        }

        [Fact]
        public void Nothing_kept_is_older_than_anything_dropped()
        {
            var rng = new System.Random(2288);
            for (int round = 0; round < 20000; round++)
            {
                int count = rng.Next(2, 40);
                int cap = rng.Next(1, count);
                var held = new List<Reading>();
                for (int i = 0; i < count; i++)
                    held.Add(Seen("i" + i, "t" + i, (float)(rng.NextDouble() * 50d)));
                var dropped = Kept.OldestBeyond(held, cap);
                float oldestKept = float.MaxValue;
                var gone = new HashSet<string>();
                foreach (Reading one in dropped) gone.Add(one.Item);
                foreach (Reading one in held)
                    if (!gone.Contains(one.Item) && one.Day < oldestKept) oldestKept = one.Day;
                foreach (Reading one in dropped) Assert.True(one.Day <= oldestKept);
            }
        }

        [Fact]
        public void A_market_that_lost_stock_is_said_in_words_rather_than_as_a_figure_below_zero()
        {
            Assert.Equal("said the shelf would lose 48 unit(s) of it and it lost 48, exactly what it said",
                         Scoring.UnitsShifted(-48, -48));
            Assert.Equal("said the shelf would gain 39 unit(s) of it and it lost 10, 49 fewer on the shelf than it said",
                         Scoring.UnitsShifted(39, -10));
            Assert.Equal("said the shelf would lose 5 unit(s) of it and it gained 3, 8 more on the shelf than it said",
                         Scoring.UnitsShifted(-5, 3));
            Assert.Equal("said the shelf would hold as many of it and it held as many, exactly what it said",
                         Scoring.UnitsShifted(0, 0));
            Assert.Equal("said the shelf would gain 7 unit(s) of it and it gained 6, 1 fewer on the shelf than it said",
                         Scoring.UnitsShifted(7, 6));
        }

        [Fact]
        public void A_town_s_own_use_is_carried_to_the_day_you_walked_in()
        {
            Assert.Equal((-10, -500), Scoring.ToTheWalkIn(-5, -250, 250, 50, 1f, 2f, 40, 2000));
            Assert.Equal((-3, -125), Scoring.ToTheWalkIn(-5, -250, 250, 50, 1f, 0.5f, 40, 2000));
            Assert.Equal((-5, -250), Scoring.ToTheWalkIn(-5, -250, 250, 50, 1f, 1.1f, 40, 2000));
            Assert.Equal((-5, -250), Scoring.ToTheWalkIn(-5, -250, 0, 50, 1f, 3f, 40, 2000));
        }

        [Fact]
        public void A_town_s_own_use_carried_to_your_walk_in_never_takes_more_than_the_shelf_held()
        {
            Assert.Equal((-8, -400), Scoring.ToTheWalkIn(-5, -250, 250, 50, 1f, 3f, 8, 400));
            Assert.Equal((-11, -550), Scoring.ToTheWalkIn(-5, -250, 300, 50, 1f, 2f, 40, Scoring.NoWorth));
        }

        [Fact]
        public void A_forecast_worth_and_what_the_shelf_did_read_the_right_way_round_whichever_way_each_went()
        {
            Assert.Equal("said the shelf would lose goods of that kind worth 1058 denars and it lost goods worth " +
                         "1056 denars, 2 denars more on the shelf than it said",
                         Scoring.WorthShifted(-1058, -1056));
            Assert.Equal("said the shelf would lose goods of that kind worth 418 denars and it lost goods worth " +
                         "3220 denars, 2802 denars less on the shelf than it said",
                         Scoring.WorthShifted(-418, -3220));
            Assert.Equal("said the shelf would lose goods of that kind worth 300 denars and it gained goods worth " +
                         "200 denars, 500 denars more on the shelf than it said",
                         Scoring.WorthShifted(-300, 200));
            Assert.Equal("said the shelf would gain goods of that kind worth 460 denars and it lost goods worth " +
                         "120 denars, 580 denars less on the shelf than it said",
                         Scoring.WorthShifted(460, -120));
            Assert.Equal("said the shelf would gain goods of that kind worth 460 denars and it gained goods worth " +
                         "700 denars, 240 denars more on the shelf than it said",
                         Scoring.WorthShifted(460, 700));
        }

        [Fact]
        public void A_forecast_worth_that_held_or_a_shelf_that_did_not_move_is_said_so_in_words()
        {
            Assert.Equal("said the shelf would gain goods of that kind worth 460 denars and it gained goods worth " +
                         "460 denars, exactly what it said",
                         Scoring.WorthShifted(460, 460));
            Assert.Equal("said the shelf would hold the same worth of goods of that kind and it held the same worth, " +
                         "exactly what it said",
                         Scoring.WorthShifted(0, 0));
            Assert.Equal("said the shelf would lose goods of that kind worth 2147483648 denars and it gained goods " +
                         "worth 2147483647 denars, 4294967295 denars more on the shelf than it said",
                         Scoring.WorthShifted(int.MinValue, int.MaxValue));
        }

        [Fact]
        public void What_you_bought_there_yourself_is_not_counted_as_leaving()
        {
            Outcome how = Scoring.Weigh(0, 40, 8, 30, 1200, 240, -32, -960);
            Assert.Equal(0, how.Landed);
            Assert.Equal(0, how.LandingOff);
            Assert.True(how.WorthKept);
            Assert.Equal(0, how.Moved);
            Assert.Equal(-30, how.WorthOff);
            Assert.Equal(1f, how.Share, 4);
        }

        [Fact]
        public void What_you_sold_there_yourself_is_not_counted_as_landing()
        {
            Outcome how = Scoring.Weigh(10, 5, 55, 400, 1000, 3000, 40, 1600);
            Assert.Equal(10, how.Landed);
            Assert.Equal(0, how.LandingOff);
            Assert.Equal(400, how.Moved);
            Assert.Equal(0, how.WorthOff);
            Assert.Equal(0f, how.Share, 4);
        }

        [Fact]
        public void A_figure_nobody_traded_against_is_weighed_exactly_as_before()
        {
            Outcome before = Scoring.Weigh(20, 10, 25, 100, 1000, 1080);
            Outcome after = Scoring.Weigh(20, 10, 25, 100, 1000, 1080, 0, 0);
            Assert.Equal(before.Landed, after.Landed);
            Assert.Equal(before.Moved, after.Moved);
            Assert.Equal(before.Share, after.Share);
        }

        [Fact]
        public void A_market_that_keeps_no_worth_ignores_the_worth_you_moved()
        {
            Outcome how = Scoring.Weigh(0, 10, 4, 50, Scoring.NoWorth, Scoring.NoWorth, -6, -300);
            Assert.False(how.WorthKept);
            Assert.Equal(0, how.Landed);
            Assert.Equal(0, how.Moved);
            Assert.Equal(TradeMath.NoShareToGive, how.Share);
        }

        [Fact]
        public void The_log_says_what_it_left_out_of_your_own_trading()
        {
            Assert.Equal("", Scoring.Yours(0, false));
            Assert.Equal("", Scoring.Yours(0, true));
            Assert.Equal(" (not counting the 32 unit(s) your own trading took off the shelf)", Scoring.Yours(-32, false));
            Assert.Equal(" (not counting the 12 unit(s) your own trading put on the shelf)", Scoring.Yours(12, false));
            Assert.Equal(" (not counting goods worth 960 denars your own trading took off the shelf)",
                         Scoring.Yours(-960, true));
            Assert.Equal(" (not counting goods worth 2147483648 denars your own trading took off the shelf)",
                         Scoring.Yours(int.MinValue, true));
        }

        [Fact]
        public void A_trade_in_the_good_itself_counts_its_units_and_its_worth()
        {
            var (stock, worth) = Scoring.YoursAdded(0, 0, true, true, true, -32, 30);
            Assert.Equal(-32, stock);
            Assert.Equal(-960, worth);
            (stock, worth) = Scoring.YoursAdded(stock, worth, true, true, true, 12, 30);
            Assert.Equal(-20, stock);
            Assert.Equal(-600, worth);
        }

        [Fact]
        public void A_trade_in_another_good_of_the_kind_counts_only_its_worth()
        {
            var (stock, worth) = Scoring.YoursAdded(-5, -150, false, true, true, 4, 250);
            Assert.Equal(-5, stock);
            Assert.Equal(850, worth);
        }

        [Fact]
        public void A_trade_in_a_good_of_another_kind_counts_nothing()
        {
            var (stock, worth) = Scoring.YoursAdded(-5, -150, false, false, true, 40, 12);
            Assert.Equal(-5, stock);
            Assert.Equal(-150, worth);
        }

        [Fact]
        public void A_market_that_keeps_no_worth_counts_only_the_units_you_moved()
        {
            var (stock, worth) = Scoring.YoursAdded(0, 0, true, true, false, -6, 50);
            Assert.Equal(-6, stock);
            Assert.Equal(0, worth);
        }

        [Fact]
        public void Nothing_moved_leaves_what_you_traded_where_it_was()
        {
            var (stock, worth) = Scoring.YoursAdded(-3, -90, true, true, true, 0, 30);
            Assert.Equal(-3, stock);
            Assert.Equal(-90, worth);
        }

        [Fact]
        public void A_forecast_walked_into_before_half_its_time_is_too_soon_to_say_anything()
        {
            Assert.True(Scoring.TooSoonToSay(1.8f, 0f));
            Assert.False(Scoring.TooSoonToSay(1.8f, 1f));
            Assert.False(Scoring.TooOldToSay(1.8f, 0f, 0f, out float since));
            Assert.True(Scoring.TooSoonToSay(1.8f, since));
        }

        [Fact]
        public void Reworking_a_market_changes_only_what_it_holds_for_that_market()
        {
            var keeps = new Keeps<Promise>();
            keeps.Put("pravend", "grain", Made(0f, 3f));
            keeps.Put("pravend", "wine", Made(0f, 3f));
            keeps.Put("epicrotea", "grain", Made(0f, 3f));
            keeps.Rework("pravend", one => Made(one.AtHours + 24f, one.WithinDays));
            Assert.Equal(3, keeps.Count);
            Dictionary<string, Promise> pravend = keeps.TakeAt("pravend");
            Assert.Equal(24f, pravend["grain"].AtHours);
            Assert.Equal(24f, pravend["wine"].AtHours);
            Assert.Equal(0f, keeps.TakeAt("epicrotea")["grain"].AtHours);
        }

        [Fact]
        public void Reworking_a_market_it_holds_nothing_for_changes_nothing()
        {
            var keeps = new Keeps<Promise>();
            keeps.Put("pravend", "grain", Made(0f, 3f));
            keeps.Rework("epicrotea", one => Made(99f, 99f));
            keeps.Rework(null, one => Made(99f, 99f));
            keeps.Rework("pravend", null);
            Assert.Equal(1, keeps.Count);
            Assert.Equal(0f, keeps.TakeAt("pravend")["grain"].AtHours);
        }

        [Fact]
        public void It_names_every_market_it_holds_something_for()
        {
            var keeps = new Keeps<Promise>();
            Assert.Empty(keeps.Sites());
            keeps.Put("pravend", "grain", Made(0f, 3f));
            keeps.Put("pravend", "wine", Made(0f, 3f));
            keeps.Put("epicrotea", "grain", Made(0f, 3f));
            List<string> sites = keeps.Sites();
            Assert.Equal(2, sites.Count);
            Assert.Contains("pravend", sites);
            Assert.Contains("epicrotea", sites);
            keeps.TakeAt("pravend");
            Assert.Single(keeps.Sites());
        }

        private static List<(string good, int amount, bool food)> Cargo(params (string good, int amount, bool food)[] held) =>
            new List<(string good, int amount, bool food)>(held);

        [Fact]
        public void The_marker_says_it_weighed_again_only_when_the_gold_the_units_or_its_hold_moved()
        {
            Assert.False(Marks.WorthSayingAgain(874, 2, false, 874, 2, false));
            Assert.True(Marks.WorthSayingAgain(834, 2, false, 874, 2, false));
            Assert.True(Marks.WorthSayingAgain(874, 3, false, 874, 2, false));
            Assert.True(Marks.WorthSayingAgain(874, 2, true, 874, 2, false));
            Assert.True(Marks.WorthSayingAgain(874, 2, false, -1, -1, false));
        }

        private static Dictionary<string, (string name, int units, long value)> Told(
            params (string where, string name, int units, long value)[] rows)
        {
            var told = new Dictionary<string, (string name, int units, long value)>();
            foreach (var (where, name, units, value) in rows) told[where] = (name, units, value);
            return told;
        }

        private static List<(string where, int units, long value)> Board(
            params (string where, int units, long value)[] rows) =>
            new List<(string where, int units, long value)>(rows);

        [Fact]
        public void Nothing_moved_on_the_board_when_every_market_shows_the_same_units_and_gold()
        {
            var told = Told(("town_V8", "Vostrum", 8, 1781), ("town_ES3", "Epicrotea", 13, 1946));
            var (joined, changed, gone) = Marks.WhatMovedOnTheBoard(
                Board(("town_ES3", 13, 1946), ("town_V8", 8, 1781)), told);
            Assert.Empty(joined);
            Assert.Empty(changed);
            Assert.Empty(gone);
        }

        [Fact]
        public void A_market_whose_gold_or_units_changed_is_named_as_changed()
        {
            var told = Told(("town_V8", "Vostrum", 8, 1781), ("town_ES3", "Epicrotea", 13, 1946),
                            ("town_M1", "Makeb", 4, 700));
            var (joined, changed, gone) = Marks.WhatMovedOnTheBoard(
                Board(("town_V8", 3, 363), ("town_ES3", 12, 1946), ("town_M1", 4, 690)), told);
            Assert.Empty(joined);
            Assert.Equal(new[] { "town_V8", "town_ES3", "town_M1" }, changed);
            Assert.Empty(gone);
        }

        [Fact]
        public void A_market_newly_priced_joins_the_board_and_one_no_longer_priced_is_gone_from_it()
        {
            var told = Told(("town_V8", "Vostrum", 8, 1781), ("town_ES3", "Epicrotea", 13, 1946),
                            ("town_M1", "Makeb", 4, 700));
            var (joined, changed, gone) = Marks.WhatMovedOnTheBoard(
                Board(("town_R1", 5, 900), ("town_ES3", 13, 1946)), told);
            Assert.Equal(new[] { "town_R1" }, joined);
            Assert.Empty(changed);
            Assert.Equal(new[] { "town_M1", "town_V8" }, gone);
        }

        [Fact]
        public void With_nothing_told_before_every_market_is_new_and_with_nothing_priced_now_every_market_is_gone()
        {
            var (joined, changed, gone) = Marks.WhatMovedOnTheBoard(Board(("town_V8", 8, 1781)), null);
            Assert.Equal(new[] { "town_V8" }, joined);
            Assert.Empty(changed);
            Assert.Empty(gone);
            (joined, changed, gone) = Marks.WhatMovedOnTheBoard(null, Told(("town_V8", "Vostrum", 8, 1781)));
            Assert.Empty(joined);
            Assert.Empty(changed);
            Assert.Equal(new[] { "town_V8" }, gone);
            (joined, changed, gone) = Marks.WhatMovedOnTheBoard(null, null);
            Assert.Empty(joined);
            Assert.Empty(changed);
            Assert.Empty(gone);
        }

        [Fact]
        public void A_market_back_in_the_running_takes_the_mark_only_when_it_earns_more_a_day()
        {
            Assert.True(Marks.OutEarns(110f, 100f));
            Assert.False(Marks.OutEarns(100f, 100f));
            Assert.False(Marks.OutEarns(60f, 100f));
            Assert.False(Marks.OutEarns(0f, -5f));
            Assert.True(Marks.OutEarns(5f, -5f));
        }

        [Fact]
        public void A_market_left_out_while_the_mark_stands_is_owed_a_fair_look_until_it_gets_one()
        {
            var owed = new HashSet<string>();
            Marks.OweAFairLook(owed, null, "town_B", "town_B", true);
            Assert.Equal(new[] { "town_B" }, owed);
            Marks.OweAFairLook(owed, null, null, "town_B", false);
            Assert.Equal(new[] { "town_B" }, owed);
            Marks.OweAFairLook(owed, new List<string>(), "town_A", "town_A", false);
            Assert.Equal(2, owed.Count);
            Assert.Contains("town_B", owed);
            Assert.Contains("town_A", owed);
            Marks.OweAFairLook(owed, new List<string> { "town_B" }, null, null, false);
            Assert.Equal(new[] { "town_A" }, owed);
            Marks.OweAFairLook(owed, new List<string> { "town_A" }, null, "town_C", true);
            Assert.Equal(new[] { "town_C" }, owed);
            Marks.OweAFairLook(owed, null, null, null, true);
            Assert.Empty(owed);
        }

        [Fact]
        public void Coming_back_to_a_market_owed_a_fair_look_counts_as_its_look_unless_TradeLord_trades_there_again()
        {
            var owed = new HashSet<string> { "town_B" };
            Marks.OweAFairLook(owed, null, "town_B", null, false);
            Assert.Empty(owed);
            owed.Add("town_B");
            Marks.OweAFairLook(owed, null, "town_B", "town_B", false);
            Assert.Equal(new[] { "town_B" }, owed);
        }

        [Fact]
        public void The_same_cargo_packed_in_another_order_is_the_same_cargo()
        {
            var then = Cargo(("salt", 13, false), ("flax", 18, false), ("meat", 4, true));
            var now = Cargo(("meat", 4, true), ("salt", 13, false), ("flax", 18, false));
            Assert.True(Marks.OnlyEatenFrom(then, now));
        }

        [Fact]
        public void Food_your_party_ate_on_the_road_leaves_the_cargo_the_same()
        {
            var then = Cargo(("salt", 13, false), ("grain", 30, true), ("meat", 2, true));
            Assert.True(Marks.OnlyEatenFrom(then, Cargo(("salt", 13, false), ("grain", 27, true), ("meat", 2, true))));
            Assert.True(Marks.OnlyEatenFrom(then, Cargo(("salt", 13, false), ("grain", 30, true))));
        }

        [Fact]
        public void Anything_bought_or_found_since_makes_it_another_cargo()
        {
            var then = Cargo(("salt", 13, false), ("grain", 30, true));
            Assert.False(Marks.OnlyEatenFrom(then, Cargo(("salt", 14, false), ("grain", 30, true))));
            Assert.False(Marks.OnlyEatenFrom(then, Cargo(("salt", 13, false), ("grain", 31, true))));
            Assert.False(Marks.OnlyEatenFrom(then, Cargo(("salt", 13, false), ("grain", 30, true), ("flax", 1, false))));
            Assert.False(Marks.OnlyEatenFrom(then, Cargo(("salt", 13, false), ("grain", 30, true), ("fish", 5, true))));
        }

        [Fact]
        public void A_good_that_is_not_food_sold_off_on_the_way_makes_it_another_cargo()
        {
            var then = Cargo(("salt", 13, false), ("flax", 18, false));
            Assert.False(Marks.OnlyEatenFrom(then, Cargo(("salt", 12, false), ("flax", 18, false))));
            Assert.False(Marks.OnlyEatenFrom(then, Cargo(("salt", 13, false))));
            Assert.False(Marks.OnlyEatenFrom(null, then));
            Assert.False(Marks.OnlyEatenFrom(then, null));
        }

        [Fact]
        public void The_same_good_split_across_two_lots_is_counted_as_one()
        {
            var then = Cargo(("salt", 10, false), ("salt", 3, false));
            Assert.True(Marks.OnlyEatenFrom(then, Cargo(("salt", 13, false))));
            Assert.False(Marks.OnlyEatenFrom(then, Cargo(("salt", 10, false))));
        }

        [Fact]
        public void The_marker_keeps_its_first_figure_while_it_points_at_the_same_town_and_you_have_only_eaten()
        {
            var then = Cargo(("salt", 13, false), ("grain", 30, true));
            var eaten = Cargo(("salt", 13, false), ("grain", 28, true));
            var bought = Cargo(("salt", 20, false), ("grain", 30, true));
            Assert.True(Marks.FirstLookStands("lageta", "lageta", then, eaten, 1997L));
            Assert.False(Marks.FirstLookStands("lageta", "ortysia", then, eaten, 1997L));
            Assert.False(Marks.FirstLookStands("lageta", "lageta", then, bought, 1997L));
            Assert.False(Marks.FirstLookStands("lageta", "lageta", then, eaten, 0L));
            Assert.False(Marks.FirstLookStands(null, null, then, eaten, 1997L));
            Assert.False(Marks.FirstLookStands("lageta", "lageta", null, null, 1997L));
        }

        [Fact]
        public void A_figure_reached_too_soon_is_put_back_and_judged_at_a_later_walk_in()
        {
            var keeps = new Keeps<Promise>();
            keeps.Put("lageta", "fish", Made(0f, 1f));
            Dictionary<string, Promise> here = keeps.TakeAt("lageta");
            Promise early = here["fish"];
            Assert.False(Scoring.TooOldToSay(early.WithinDays, early.AtHours, 2f, out float since));
            Assert.True(Scoring.TooSoonToSay(early.WithinDays, since));
            keeps.Put("lageta", "fish", early);
            Assert.True(keeps.TryGet("lageta", "fish", out Promise back));
            Assert.Equal(0f, back.AtHours);
            Assert.False(Scoring.TooOldToSay(back.WithinDays, back.AtHours, 13f, out since));
            Assert.False(Scoring.TooSoonToSay(back.WithinDays, since));
        }

        [Fact]
        public void A_figure_still_to_be_judged_is_not_replaced_by_a_fresher_one()
        {
            Assert.True(Scoring.StillToBeJudged(1f, 0f, 12f));
            Assert.True(Scoring.StillToBeJudged(1f, 0f, 72f));
            Assert.False(Scoring.StillToBeJudged(1f, 0f, 73f));
            var keeps = new Keeps<Promise>();
            Assert.False(keeps.TryGet("lageta", "fish", out _));
            Assert.False(keeps.TryGet(null, "fish", out _));
        }

    }
}
