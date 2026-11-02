using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class BuyMarkTests
    {
        private static List<(string good, int amount, bool food)> Cargo(params (string good, int amount, bool food)[] held) =>
            new List<(string good, int amount, bool food)>(held);

        private static List<(string from, string to, float profit, float days)> Rows(
            params (string from, string to, float profit, float days)[] rows) =>
            new List<(string from, string to, float profit, float days)>(rows);

        [Fact]
        public void One_silver_ore_at_36_a_day_gives_way_to_a_ledger_route_at_400_a_day()
        {
            Assert.True(Marks.ShowsTheBuyTown(400f, 36f, false));
            Assert.True(Marks.ShowsTheBuyTown(400f, 36f, true));
            Assert.Equal(36f, Marks.SellSideWithItsOwnRoute(36f, 93f, 0f, 0f));
        }

        [Fact]
        public void An_empty_cargo_shows_the_buy_town_whenever_a_route_earns_anything()
        {
            Assert.True(Marks.ShowsTheBuyTown(50f, 0f, false));
            Assert.True(Marks.ShowsTheBuyTown(50f, float.NaN, false));
            Assert.True(Marks.ShowsTheBuyTown(0.1f, -5f, true));
        }

        [Fact]
        public void No_route_worth_anything_never_shows_the_buy_town()
        {
            Assert.False(Marks.ShowsTheBuyTown(0f, 0f, false));
            Assert.False(Marks.ShowsTheBuyTown(0f, 36f, true));
            Assert.False(Marks.ShowsTheBuyTown(-10f, 36f, false));
            Assert.False(Marks.ShowsTheBuyTown(float.NaN, 0f, true));
        }

        [Fact]
        public void The_buy_town_takes_over_only_once_it_earns_past_the_hold_on_the_sell_mark()
        {
            float sell = 100f;
            float atTheHold = sell * TradeMath.TheMarkedTownHoldsBy;
            Assert.False(Marks.ShowsTheBuyTown(atTheHold, sell, false));
            Assert.True(Marks.ShowsTheBuyTown(atTheHold + 1f, sell, false));
            Assert.False(Marks.ShowsTheBuyTown(110f, sell, false));
        }

        [Fact]
        public void The_shown_buy_town_stays_until_the_sell_mark_earns_past_the_hold_on_it()
        {
            float buy = 100f;
            float atTheHold = buy * TradeMath.TheMarkedTownHoldsBy;
            Assert.True(Marks.ShowsTheBuyTown(buy, atTheHold, true));
            Assert.False(Marks.ShowsTheBuyTown(buy, atTheHold + 1f, true));
            Assert.True(Marks.ShowsTheBuyTown(buy, 110f, true));
        }

        [Fact]
        public void Routes_to_different_sell_at_markets_are_never_added_together()
        {
            var best = Marks.BestBuyFrom(Rows(("town_b", "town_x", 600f, 3f), ("town_b", "town_y", 500f, 3f)), null);
            Assert.Equal("town_b", best.from);
            Assert.Equal("town_x", best.to);
            Assert.Equal(600f, best.profit);
            Assert.Equal(200f, best.rate);
            Assert.Equal(1, best.rows);
        }

        [Fact]
        public void Two_goods_from_one_buy_town_to_one_sell_at_add_up_over_the_longer_ride()
        {
            var best = Marks.BestBuyFrom(Rows(("town_b", "town_x", 300f, 2f), ("town_b", "town_x", 200f, 2.5f)), null);
            Assert.Equal("town_b", best.from);
            Assert.Equal("town_x", best.to);
            Assert.Equal(500f, best.profit);
            Assert.Equal(2.5f, best.days);
            Assert.Equal(200f, best.rate);
            Assert.Equal(2, best.rows);
        }

        [Fact]
        public void The_shown_buy_town_keeps_its_hold_and_reports_its_own_rate()
        {
            var rows = Rows(("town_a", "town_x", 300f, 3f), ("town_b", "town_y", 330f, 3f));
            Assert.Equal("town_b", Marks.BestBuyFrom(rows, null).from);
            var held = Marks.BestBuyFrom(rows, "town_a");
            Assert.Equal("town_a", held.from);
            Assert.Equal(100f, held.rate);
            var past = Marks.BestBuyFrom(Rows(("town_a", "town_x", 300f, 3f), ("town_b", "town_y", 400f, 3f)), "town_a");
            Assert.Equal("town_b", past.from);
        }

        [Fact]
        public void A_tie_keeps_the_first_buy_town_seen()
        {
            var best = Marks.BestBuyFrom(Rows(("town_a", "town_x", 300f, 3f), ("town_b", "town_y", 300f, 3f)), null);
            Assert.Equal("town_a", best.from);
        }

        [Fact]
        public void Rows_that_cannot_be_weighed_leave_nothing_to_mark()
        {
            var rows = Rows((null, "town_x", 300f, 3f), ("town_a", null, 300f, 3f), ("town_a", "town_x", 0f, 3f),
                            ("town_a", "town_x", -50f, 3f), ("town_a", "town_x", float.NaN, 3f),
                            ("town_a", "town_x", 300f, float.NaN));
            var best = Marks.BestBuyFrom(rows, "town_a");
            Assert.Null(best.from);
            Assert.Null(best.to);
            Assert.Equal(0f, best.profit);
            Assert.Equal(0f, best.days);
            Assert.Equal(0f, best.rate);
            Assert.Equal(0, best.rows);
            Assert.Null(Marks.BestBuyFrom(null, null).from);
            Assert.Null(Marks.BestBuyFrom(Rows(), null).from);
        }

        [Fact]
        public void A_row_serves_a_day_plus_its_ride_at_scan_time_in_hours()
        {
            Assert.True(Marks.RowStillServes(87, 2.6f));
            Assert.False(Marks.RowStillServes(88, 2.6f));
            Assert.True(Marks.RowStillServes(0, 2.6f));
            Assert.True(Marks.RowStillServes(24, 0f));
            Assert.False(Marks.RowStillServes(25, 0f));
            Assert.False(Marks.RowStillServes(-1, 0f));
        }

        [Fact]
        public void A_ride_that_cannot_be_read_counts_as_none()
        {
            Assert.True(Marks.RowStillServes(24, float.NaN));
            Assert.False(Marks.RowStillServes(25, float.NaN));
            Assert.False(Marks.RowStillServes(25, float.PositiveInfinity));
            Assert.False(Marks.RowStillServes(25, -3f));
        }

        [Fact]
        public void The_scan_serves_while_only_food_was_eaten_and_most_of_the_gold_is_kept()
        {
            var then = Cargo(("silver_ore", 1, false), ("grain", 10, true));
            Assert.True(Marks.ScanStillServes(1000, 950, then, Cargo(("silver_ore", 1, false), ("grain", 7, true))));
            Assert.True(Marks.ScanStillServes(1000, 900, then, Cargo(("silver_ore", 1, false), ("grain", 10, true))));
        }

        [Fact]
        public void The_scan_stops_serving_once_a_good_changed_or_the_gold_fell_past_a_tenth()
        {
            var then = Cargo(("silver_ore", 1, false), ("grain", 10, true));
            Assert.False(Marks.ScanStillServes(1000, 1000, then, Cargo(("silver_ore", 2, false), ("grain", 10, true))));
            Assert.False(Marks.ScanStillServes(1000, 1000, then, Cargo(("grain", 10, true))));
            Assert.False(Marks.ScanStillServes(1000, 890, then, Cargo(("silver_ore", 1, false), ("grain", 10, true))));
            Assert.False(Marks.ScanStillServes(1000, 1000, then, null));
            Assert.False(Marks.ScanStillServes(1000, 1000, null, then));
        }

        [Fact]
        public void A_scan_taken_with_no_gold_is_not_judged_by_the_gold()
        {
            var cargo = Cargo(("silver_ore", 1, false));
            Assert.True(Marks.ScanStillServes(0, 0, cargo, Cargo(("silver_ore", 1, false))));
        }

        [Fact]
        public void A_row_is_retimed_from_where_you_stand_now()
        {
            Assert.Equal(4f, Marks.RetimedDays(1f, 5f, 2f));
            Assert.Equal(5f, Marks.RetimedDays(2f, 5f, 2f));
        }

        [Fact]
        public void A_retimed_row_never_runs_backwards()
        {
            Assert.Equal(1f, Marks.RetimedDays(1f, 2f, 3f));
            Assert.Equal(3f, Marks.RetimedDays(float.NaN, 5f, 2f));
            Assert.Equal(5f, Marks.RetimedDays(-1f, 5f, -2f));
            Assert.Equal(0f, Marks.RetimedDays(-1f, -5f, float.NaN));
            Assert.Equal(2f, Marks.RetimedDays(2f, float.PositiveInfinity, float.PositiveInfinity));
        }

        private static Dictionary<string, (float rate, float earned)> Sold(
            params (string town, float rate, float earned)[] sold)
        {
            var there = new Dictionary<string, (float rate, float earned)>(StringComparer.Ordinal);
            foreach (var (town, rate, earned) in sold) there[town] = (rate, earned);
            return there;
        }

        [Fact]
        public void A_town_that_takes_your_cargo_and_starts_a_route_is_weighed_on_both()
        {
            var rows = Rows(("town_b", "town_x", 110f, 1f));
            var both = Marks.BestPlaceToGo(rows, Sold(("town_b", 90f, 90f)), null);
            Assert.Equal("town_b", both.from);
            Assert.Equal(110f, both.profit);
            Assert.Equal(90f, both.sold);
            Assert.Equal(200f, both.rate);
            Assert.True(Marks.ShowsTheBuyTown(both.rate, 100f, false));
            Assert.False(Marks.ShowsTheBuyTown(Marks.BestPlaceToGo(rows, null, null).rate, 100f, false));
        }

        [Fact]
        public void The_sell_mark_and_a_buy_town_are_weighed_by_one_rule()
        {
            var sold = Sold(("town_m", 100f, 100f), ("town_b", 90f, 90f));
            var mark = Marks.BestPlaceToGo(Rows(("town_m", "town_x", 110f, 1f)), sold, null);
            var other = Marks.BestPlaceToGo(Rows(("town_b", "town_x", 110f, 1f)), sold, null);
            Assert.Equal(210f, mark.rate);
            Assert.Equal(200f, other.rate);
            Assert.Equal(Marks.SellSideWithItsOwnRoute(100f, 100f, 110f, 1f), mark.rate);
            Assert.Equal(Marks.SellSideWithItsOwnRoute(90f, 90f, 110f, 1f), other.rate);
        }

        [Fact]
        public void What_your_cargo_fetches_in_a_town_counts_once_however_many_goods_start_there()
        {
            var best = Marks.BestPlaceToGo(Rows(("town_b", "town_x", 100f, 2f), ("town_b", "town_x", 100f, 2f)),
                                           Sold(("town_b", 50f, 100f)), null);
            Assert.Equal(200f, best.profit);
            Assert.Equal(100f, best.sold);
            Assert.Equal(150f, best.rate);
            Assert.Equal(2, best.rows);
        }

        [Fact]
        public void A_town_never_scores_under_what_your_cargo_alone_earns_there()
        {
            var best = Marks.BestPlaceToGo(Rows(("town_b", "town_x", 10f, 10f)), Sold(("town_b", 80f, 80f)), null);
            Assert.Equal(80f, best.rate);
        }

        [Fact]
        public void With_nothing_sold_anywhere_towns_rank_by_their_routes_alone()
        {
            var rows = Rows(("town_a", "town_x", 300f, 3f), ("town_b", "town_y", 330f, 3f), ("town_b", "town_y", 30f, 2f));
            var plain = Marks.BestBuyFrom(rows, "town_a");
            var weighed = Marks.BestPlaceToGo(rows, Sold(), "town_a");
            Assert.Equal(plain.from, weighed.from);
            Assert.Equal(plain.rate, weighed.rate);
            Assert.Equal(0f, weighed.sold);
        }

        [Fact]
        public void The_sell_mark_counts_its_own_route_only_when_that_earns_more_a_day()
        {
            Assert.Equal(1093f / 3f, Marks.SellSideWithItsOwnRoute(36f, 93f, 1000f, 3f));
            Assert.Equal(500f, Marks.SellSideWithItsOwnRoute(500f, 1000f, 100f, 10f));
            Assert.Equal(100f, Marks.SellSideWithItsOwnRoute(10f, -50f, 300f, 3f));
            Assert.Equal(36f, Marks.SellSideWithItsOwnRoute(36f, 93f, float.NaN, 3f));
            Assert.Equal(36f, Marks.SellSideWithItsOwnRoute(36f, 93f, -20f, 3f));
        }
    }
}
