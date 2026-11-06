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
        public void A_purse_that_fell_only_by_the_wages_paid_since_the_scan_still_serves()
        {
            var cargo = Cargo(("silver_ore", 1, false));
            Assert.True(Marks.ScanStillServes(500, 380, cargo, Cargo(("silver_ore", 1, false)), 120));
            Assert.False(Marks.ScanStillServes(500, 379, cargo, Cargo(("silver_ore", 1, false)), 120));
            Assert.False(Marks.ScanStillServes(500, 380, cargo, Cargo(("silver_ore", 1, false))));
            Assert.False(Marks.ScanStillServes(500, 500, cargo, Cargo(("silver_ore", 2, false)), 1000));
        }

        [Fact]
        public void Wages_are_counted_for_every_day_begun_since_the_scan()
        {
            Assert.Equal(0, Marks.WagesPaidSince(0, 50));
            Assert.Equal(50, Marks.WagesPaidSince(1, 50));
            Assert.Equal(50, Marks.WagesPaidSince(24, 50));
            Assert.Equal(100, Marks.WagesPaidSince(25, 50));
            Assert.Equal(0, Marks.WagesPaidSince(-3, 50));
            Assert.Equal(0, Marks.WagesPaidSince(30, 0));
            Assert.Equal(int.MaxValue, Marks.WagesPaidSince(int.MaxValue - 30, int.MaxValue));
            Assert.Equal(89478486, Marks.WagesPaidSince(int.MaxValue, 1));
        }

        [Fact]
        public void A_scan_taken_with_no_gold_is_not_judged_by_the_gold()
        {
            var cargo = Cargo(("silver_ore", 1, false));
            Assert.True(Marks.ScanStillServes(0, 0, cargo, Cargo(("silver_ore", 1, false))));
        }

        [Fact]
        public void A_market_left_at_a_gate_or_as_your_cargo_changed_is_not_held_back()
        {
            var beaten = new Dictionary<string, int>(StringComparer.Ordinal);
            Assert.False(Marks.RememberABeatenMark(beaten, "vostrum", 100, false, true, true));
            Assert.False(Marks.RememberABeatenMark(beaten, "vostrum", 100, true, false, true));
            Assert.False(Marks.RememberABeatenMark(beaten, "vostrum", 100, true, true, false));
            Assert.Empty(beaten);
            Assert.False(Marks.ComesBackTooSoon(beaten, "vostrum", 105, true, 311f, 261f));
        }

        [Fact]
        public void A_market_beaten_on_the_road_needs_a_quarter_more_to_win_the_mark_back_within_the_day()
        {
            var beaten = new Dictionary<string, int>(StringComparer.Ordinal);
            Assert.True(Marks.RememberABeatenMark(beaten, "vostrum", 100, true, true, true));
            Assert.True(Marks.ComesBackTooSoon(beaten, "vostrum", 105, true, 311f, 261f));
            Assert.True(Marks.ComesBackTooSoon(beaten, "vostrum", 105, true, 326.25f, 261f));
            Assert.False(Marks.ComesBackTooSoon(beaten, "vostrum", 105, true, 330f, 261f));
            Assert.False(Marks.ComesBackTooSoon(beaten, "vostrum", 125, true, 311f, 261f));
            Assert.True(Marks.ComesBackTooSoon(beaten, "vostrum", 124, true, 311f, 261f));
            Assert.False(Marks.ComesBackTooSoon(beaten, "vostrum", 99, true, 311f, 261f));
            Assert.True(Marks.LeftOnTheRoadLately(beaten, "vostrum", 124));
            Assert.False(Marks.LeftOnTheRoadLately(beaten, "vostrum", 125));
            Assert.False(Marks.LeftOnTheRoadLately(beaten, "vostrum", 99));
            Assert.False(Marks.LeftOnTheRoadLately(beaten, "atphynia", 105));
            Assert.False(Marks.LeftOnTheRoadLately(null, "vostrum", 105));
            Assert.False(Marks.ComesBackTooSoon(beaten, "vostrum", 105, false, 311f, 261f));
            Assert.False(Marks.ComesBackTooSoon(beaten, "atphynia", 105, true, 311f, 261f));
        }

        [Fact]
        public void A_village_passed_on_the_road_without_entering_keeps_what_the_marker_remembers()
        {
            var beaten = new Dictionary<string, int>(StringComparer.Ordinal);
            Marks.RememberABeatenMark(beaten, "vostrum", 100, true, true, true);
            Marks.RememberABeatenMark(beaten, "atphynia", 102, true, true, true);
            Assert.True(Marks.ComesBackTooSoon(beaten, "vostrum", 103, true, 300f, 261f));
            Assert.True(Marks.ComesBackTooSoon(beaten, "atphynia", 103, true, 300f, 261f));
            Assert.False(Marks.AtAGate(true, false, true));
            Assert.True(Marks.AtAGate(true, true, true));
            Assert.True(Marks.AtAGate(false, false, true));
            Assert.True(Marks.AtAGate(true, false, false));
        }

        [Fact]
        public void A_Buy_From_is_taken_up_only_at_a_gate_and_only_kept_or_dropped_on_the_road()
        {
            Assert.True(Marks.MayTakeUpABuyFrom(true, "jalmarys", null));
            Assert.True(Marks.MayTakeUpABuyFrom(true, "poros", "jalmarys"));
            Assert.False(Marks.MayTakeUpABuyFrom(false, "jalmarys", null));
            Assert.True(Marks.MayTakeUpABuyFrom(false, "jalmarys", "jalmarys"));
            Assert.False(Marks.MayTakeUpABuyFrom(false, "poros", "jalmarys"));
            Assert.False(Marks.MayTakeUpABuyFrom(false, null, null));
        }

        [Fact]
        public void A_Buy_From_held_for_a_gate_counts_as_waiting_only_when_it_earns_half_again_as_much()
        {
            Assert.True(Marks.AGreatBuyFromWaits(151f, 100f));
            Assert.False(Marks.AGreatBuyFromWaits(150f, 100f));
            Assert.False(Marks.AGreatBuyFromWaits(120f, 100f));
            Assert.True(Marks.AGreatBuyFromWaits(10f, 0f));
            Assert.False(Marks.AGreatBuyFromWaits(0f, 0f));
            Assert.False(Marks.AGreatBuyFromWaits(-5f, 0f));
            Assert.Equal(1.5f, Marks.AWaitingBuyFromBeatsWhatYouHaveBy);
        }

        [Fact]
        public void A_Staged_Trading_deal_is_scored_on_the_figure_the_marker_held_as_you_walked_in()
        {
            Assert.True(Marks.TheFigureOnTheWayInScores("lageta", "jaculan", "lageta", 7552L));
            Assert.False(Marks.TheFigureOnTheWayInScores("lageta", "lageta", "lageta", 7552L));
            Assert.False(Marks.TheFigureOnTheWayInScores("lageta", "jaculan", "ortysia", 7552L));
            Assert.False(Marks.TheFigureOnTheWayInScores("lageta", "jaculan", "lageta", 0L));
            Assert.False(Marks.TheFigureOnTheWayInScores("lageta", "jaculan", null, 0L));
            Assert.False(Marks.TheFigureOnTheWayInScores(null, "jaculan", "lageta", 7552L));
        }

        [Fact]
        public void A_ride_broken_off_is_turned_away_from_only_when_you_set_off_for_another_market()
        {
            Assert.False(Marks.TurnedAwayAfterABreak("qasira", "qasira"));
            Assert.True(Marks.TurnedAwayAfterABreak("qasira", "razih"));
            Assert.False(Marks.TurnedAwayAfterABreak(null, "razih"));
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
            Assert.Equal(TradeMath.PerDay(100f + 110f, 1f), mark.rate);
            Assert.Equal(TradeMath.PerDay(90f + 110f, 1f), other.rate);
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
        public void A_buy_town_is_weighed_on_its_route_and_sale_together_never_on_its_sale_alone()
        {
            var best = Marks.BestPlaceToGo(Rows(("town_b", "town_x", 14f, 2.4f)), Sold(("town_b", 400f, 261f)), null);
            Assert.Equal(TradeMath.PerDay(261f + 14f, 2.4f), best.rate);
            Assert.False(Marks.ShowsTheBuyTown(best.rate, 431f, false));
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

        [Fact]
        public void The_shadow_marker_leaves_its_market_on_the_road_only_for_half_as_much_again_and_300_more_profit()
        {
            Assert.True(Marks.AGreatDealBeatsTheShadow(150f, 1300L, 100f, 1000L));
            Assert.False(Marks.AGreatDealBeatsTheShadow(149f, 1300L, 100f, 1000L));
            Assert.False(Marks.AGreatDealBeatsTheShadow(150f, 1299L, 100f, 1000L));
            Assert.False(Marks.AGreatDealBeatsTheShadow(0f, 5000L, 0f, 0L));
            Assert.True(Marks.AGreatDealBeatsTheShadow(10f, 300L, 0f, 0L));
            Assert.Equal(1.5f, Marks.AGreatDealBeatsTheShadowBy);
            Assert.Equal(300L, Marks.AGreatDealEarnsTheShadowAtLeast);
        }
    }
}
