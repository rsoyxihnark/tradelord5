using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class TradeMathTests
    {
        private const int AveragePaid = 0, LastPaid = 1, CheapestKnown = 2;

        private static PurchaseRecord Bought(int count, int totalPaid)
        {
            var rec = new PurchaseRecord { ItemId = "grain" };
            TradeMath.AddPurchase(rec, count, totalPaid);
            return rec;
        }

        [Fact]
        public void The_purse_holds_back_the_flat_reserve_and_the_wages_together()
        {
            Assert.Equal(300, TradeMath.Reserve(300, 0, 250));
            Assert.Equal(300, TradeMath.Reserve(300, 3, 0));
            Assert.Equal(1050, TradeMath.Reserve(300, 3, 250));
        }

        [Fact]
        public void The_wage_cover_grows_with_the_army_while_the_flat_reserve_stays_put()
        {
            Assert.Equal(360, TradeMath.Reserve(300, 3, 20));
            Assert.Equal(3300, TradeMath.Reserve(300, 3, 1000));
        }

        [Fact]
        public void A_figure_that_makes_no_sense_is_dropped_and_the_rest_of_the_hold_stands()
        {
            Assert.Equal(750, TradeMath.Reserve(-500, 3, 250));
            Assert.Equal(300, TradeMath.Reserve(300, -3, 250));
            Assert.Equal(300, TradeMath.Reserve(300, 3, -250));
            Assert.Equal(0, TradeMath.Reserve(-500, 0, 250));
        }

        [Fact]
        public void A_wage_bill_too_large_to_count_still_leaves_a_usable_reserve()
        {
            Assert.Equal(int.MaxValue, TradeMath.Reserve(300, 30, int.MaxValue));
            Assert.True(TradeMath.Reserve(300, 30, int.MaxValue) > 0);
        }

        [Fact]
        public void What_is_left_to_spend_is_the_purse_less_everything_held_back()
        {
            int held = TradeMath.Reserve(300, 3, 100);
            Assert.Equal(400, TradeMath.Budget(1000, held, 0, 0));
            Assert.Equal(0, TradeMath.Budget(600, held, 0, 0));
        }

        [Fact]
        public void The_spending_cap_counts_what_this_visit_has_already_spent()
        {
            Assert.Equal(1000, TradeMath.Budget(9700, 300, 1000, 0));
            Assert.Equal(400, TradeMath.Budget(9100, 300, 1000, 600));
            Assert.Equal(0, TradeMath.Budget(8700, 300, 1000, 1000));
        }

        [Fact]
        public void Units_you_never_bought_are_still_offered_when_the_bought_ones_miss_the_margin()
        {
            int remaining = 10, paidLeft = 4;
            Assert.True(TradeMath.SkipTheUnitsYouPaidFor(false, ref remaining, ref paidLeft));
            Assert.Equal(6, remaining);
            Assert.Equal(0, paidLeft);
        }

        [Fact]
        public void A_lot_you_paid_for_outright_stops_at_the_margin_and_moves_nothing()
        {
            int remaining = 4, paidLeft = 10;
            Assert.False(TradeMath.SkipTheUnitsYouPaidFor(false, ref remaining, ref paidLeft));
            Assert.Equal(4, remaining);
            Assert.Equal(10, paidLeft);

            remaining = 10; paidLeft = 0;
            Assert.False(TradeMath.SkipTheUnitsYouPaidFor(false, ref remaining, ref paidLeft));
            Assert.Equal(10, remaining);

            remaining = 10; paidLeft = 4;
            Assert.False(TradeMath.SkipTheUnitsYouPaidFor(true, ref remaining, ref paidLeft));
            Assert.Equal(10, remaining);
            Assert.Equal(4, paidLeft);
        }

        [Fact]
        public void What_a_lot_cost_per_unit_is_what_you_paid_for_it()
        {
            Assert.Equal(12, TradeMath.UnitBasis(Bought(10, 120), AveragePaid));
            Assert.Equal(12, TradeMath.UnitBasis(Bought(10, 120), LastPaid));
        }

        [Fact]
        public void Buying_again_at_a_new_price_averages_the_lot_and_remembers_the_last()
        {
            var rec = Bought(10, 100);
            TradeMath.AddPurchase(rec, 10, 300);
            Assert.Equal(20, TradeMath.UnitBasis(rec, AveragePaid));
            Assert.Equal(30, TradeMath.UnitBasis(rec, LastPaid));
        }

        [Fact]
        public void Selling_part_of_a_lot_leaves_the_rest_costing_what_it_did()
        {
            var rec = Bought(10, 120);
            TradeMath.DrainSale(rec, 4);
            Assert.Equal(6, rec.Count);
            Assert.Equal(12, TradeMath.UnitBasis(rec, AveragePaid));
        }

        [Fact]
        public void Selling_a_lot_down_one_at_a_time_never_moves_what_the_rest_cost()
        {
            foreach (var lot in new[]
                     {
                         (count: 9, paid: 50), (count: 13, paid: 200), (count: 7, paid: 100),
                         (count: 3, paid: 10), (count: 6, paid: 25)
                     })
            {
                var rec = Bought(lot.count, lot.paid);
                int held = TradeMath.UnitBasis(rec, AveragePaid);
                while (rec.Count > 1)
                {
                    TradeMath.DrainSale(rec, 1);
                    Assert.Equal(held, TradeMath.UnitBasis(rec, AveragePaid));
                }
            }
        }

        [Fact]
        public void Selling_a_lot_in_chunks_leaves_the_rest_costing_the_same_as_selling_it_singly()
        {
            var singly = Bought(13, 200);
            for (int i = 0; i < 6; i++) TradeMath.DrainSale(singly, 1);
            var chunked = Bought(13, 200);
            TradeMath.DrainSale(chunked, 6);
            Assert.Equal(singly.Count, chunked.Count);
            Assert.Equal(TradeMath.UnitBasis(singly, AveragePaid),
                         TradeMath.UnitBasis(chunked, AveragePaid));
        }

        [Fact]
        public void Selling_the_whole_lot_clears_what_it_cost()
        {
            var rec = Bought(10, 120);
            TradeMath.DrainSale(rec, 10);
            Assert.Equal(0, rec.Count);
            Assert.Equal(0, rec.TotalPaid);
            Assert.Equal(TradeMath.NoRecordedBasis, TradeMath.UnitBasis(rec, AveragePaid));
        }

        [Fact]
        public void Selling_more_than_you_hold_never_takes_the_lot_below_nothing()
        {
            var rec = Bought(3, 30);
            TradeMath.DrainSale(rec, 99);
            Assert.Equal(0, rec.Count);
            Assert.Equal(0, rec.TotalPaid);
            TradeMath.DrainSale(rec, 99);
            Assert.Equal(0, rec.Count);
            Assert.Equal(0, rec.TotalPaid);
        }

        [Fact]
        public void A_good_you_never_bought_has_no_price_you_paid()
        {
            Assert.Equal(TradeMath.NoRecordedBasis, TradeMath.UnitBasis(null, AveragePaid));
            Assert.Equal(TradeMath.NoRecordedBasis, TradeMath.UnitBasis(Bought(0, 0), AveragePaid));
        }

        [Fact]
        public void The_cheapest_known_mode_never_reads_what_you_paid()
        {
            Assert.Equal(TradeMath.NoRecordedBasis, TradeMath.UnitBasis(Bought(10, 120), CheapestKnown));
        }

        [Fact]
        public void Buying_and_selling_over_and_over_never_leaves_a_negative_cost()
        {
            var rec = Bought(7, 93);
            for (int round = 1; round <= 200; round++)
            {
                TradeMath.AddPurchase(rec, round % 5 + 1, round * 13 % 97 + 1);
                TradeMath.DrainSale(rec, round % 7 + 1);
                Assert.True(rec.Count >= 0);
                Assert.True(rec.TotalPaid >= 0);
                int unit = TradeMath.UnitBasis(rec, AveragePaid);
                Assert.True(unit == TradeMath.NoRecordedBasis || unit >= 0);
            }
        }

        [Theory]
        [InlineData(Options.PolicyIgnore, true, false)]
        [InlineData(Options.PolicyIgnore, false, false)]
        [InlineData(Options.PolicySellOnly, true, false)]
        [InlineData(Options.PolicySellOnly, false, true)]
        [InlineData(Options.PolicyBuyOnly, true, true)]
        [InlineData(Options.PolicyBuyOnly, false, false)]
        [InlineData(Options.PolicyBuySell, true, true)]
        [InlineData(Options.PolicyBuySell, false, true)]
        public void Each_category_policy_allows_exactly_what_its_name_says(int policy, bool buying, bool allowed)
        {
            Assert.Equal(allowed, TradeMath.PolicyAllows(policy, buying));
        }

        [Fact]
        public void A_policy_value_nobody_defined_allows_nothing()
        {
            foreach (int stray in new[] { -1, 4, 99, int.MaxValue, int.MinValue })
            {
                Assert.False(TradeMath.PolicyAllows(stray, buying: true));
                Assert.False(TradeMath.PolicyAllows(stray, buying: false));
            }
        }

        [Fact]
        public void Goods_you_paid_for_are_credited_against_what_you_paid()
        {
            Assert.Equal(40, TradeMath.Credit(proceeds: 100, basis: 60, unpaidWorth: 999));
            Assert.Equal(0, TradeMath.Credit(proceeds: 60, basis: 60, unpaidWorth: 999));
        }

        [Fact]
        public void A_sale_below_what_you_paid_is_reported_as_the_loss_it_is()
        {
            Assert.Equal(-25, TradeMath.Credit(proceeds: 75, basis: 100, unpaidWorth: 0));
        }

        [Fact]
        public void Loot_is_credited_against_what_it_would_have_cost_never_below_zero()
        {
            Assert.Equal(30, TradeMath.Credit(proceeds: 80, basis: 0, unpaidWorth: 50));
            Assert.Equal(0, TradeMath.Credit(proceeds: 50, basis: 0, unpaidWorth: 50));
            Assert.Equal(0, TradeMath.Credit(proceeds: 20, basis: 0, unpaidWorth: 50));
        }

        [Fact]
        public void A_negative_basis_is_treated_as_no_basis_rather_than_a_bonus()
        {
            Assert.Equal(0, TradeMath.Credit(proceeds: 10, basis: -100, unpaidWorth: 50));
        }

        [Theory]
        [InlineData(100, 115, 0.15f, true)]
        [InlineData(100, 114, 0.15f, false)]
        [InlineData(100, 100, 0f, true)]
        [InlineData(100, 99, 0f, false)]
        [InlineData(100, 200, 1f, true)]
        [InlineData(100, 199, 1f, false)]
        public void A_sale_clears_the_margin_only_at_or_above_the_marked_up_basis(
            int basis, int sellPrice, float margin, bool acceptable)
        {
            Assert.Equal(acceptable, TradeMath.ProfitAcceptable(basis, sellPrice, margin));
        }

        [Fact]
        public void With_no_basis_any_positive_price_clears_and_zero_does_not()
        {
            Assert.True(TradeMath.ProfitAcceptable(costBasis: 0, townSellPrice: 1, margin: 5f));
            Assert.False(TradeMath.ProfitAcceptable(costBasis: 0, townSellPrice: 0, margin: 0f));
            Assert.False(TradeMath.ProfitAcceptable(costBasis: 0, townSellPrice: -5, margin: 0f));
        }

        [Theory]
        [InlineData(100, 0.85f, 85f)]
        [InlineData(100, 1f, 100f)]
        [InlineData(0, 0.85f, 0f)]
        public void The_safety_factor_discounts_a_far_market_price(int far, float factor, float expected)
        {
            Assert.Equal(expected, TradeMath.Realizable(far, factor), 3);
        }

        [Theory]
        [InlineData(100, 115f, 0.15f, true)]
        [InlineData(100, 114f, 0.15f, false)]
        [InlineData(100, 100f, 0f, true)]
        [InlineData(0, 500f, 0f, false)]
        [InlineData(-5, 500f, 0f, false)]
        public void A_purchase_clears_only_when_the_discounted_resale_covers_the_markup(
            int buyPrice, float realizable, float margin, bool acceptable)
        {
            Assert.Equal(acceptable, TradeMath.BuyAcceptable(buyPrice, realizable, margin));
        }

        [Fact]
        public void A_free_good_is_never_bought_however_good_the_resale_looks()
        {
            Assert.False(TradeMath.BuyAcceptable(buyPrice: 0, realizable: float.MaxValue, margin: 0f));
        }

        [Theory]
        [InlineData(0f, 0.85f)]
        [InlineData(0.15f, 0.85f)]
        [InlineData(0.5f, 1f)]
        [InlineData(1f, 0.6f)]
        public void Anything_worth_buying_is_worth_selling_at_the_same_margin(float margin, float safety)
        {
            for (int buy = 1; buy <= 400; buy += 7)
            {
                for (int far = 1; far <= 2000; far += 37)
                {
                    float realizable = TradeMath.Realizable(far, safety);
                    if (!TradeMath.BuyAcceptable(buy, realizable, margin)) continue;

                    Assert.True(TradeMath.ProfitAcceptable(buy, (int)realizable + 1, margin),
                        $"buy {buy} cleared at margin {margin} but selling at {(int)realizable + 1} did not");
                }
            }
        }

        [Fact]
        public void A_stricter_margin_never_admits_a_purchase_a_looser_one_refused()
        {
            foreach (int buy in new[] { 1, 17, 100, 999 })
            {
                foreach (float realizable in new[] { 0f, 50f, 100f, 1500f })
                {
                    bool loose = TradeMath.BuyAcceptable(buy, realizable, 0.10f);
                    bool strict = TradeMath.BuyAcceptable(buy, realizable, 0.50f);
                    Assert.True(loose || !strict);
                }
            }
        }

        [Fact]
        public void A_stricter_margin_never_admits_a_sale_a_looser_one_refused()
        {
            foreach (int basis in new[] { 1, 17, 100, 999 })
            {
                foreach (int sell in new[] { 0, 50, 100, 1500 })
                {
                    bool loose = TradeMath.ProfitAcceptable(basis, sell, 0.10f);
                    bool strict = TradeMath.ProfitAcceptable(basis, sell, 0.50f);
                    Assert.True(loose || !strict);
                }
            }
        }

        [Fact]
        public void The_shipped_defaults_are_the_ones_the_rules_were_written_for()
        {
            var fresh = new Options();
            Assert.Equal(0, fresh.Language);
            Assert.Equal(0.15f, fresh.MinProfitMargin, 3);
            Assert.Equal(0.85f, fresh.ResaleSafetyFactor, 3);
            Assert.Equal(Options.PolicyBuySell, fresh.FoodPolicy);
            Assert.Equal(Options.PolicyBuySell, fresh.CraftingPolicy);
            Assert.Equal(Options.PolicyBuySell, fresh.LivestockPolicy);

            Assert.True(TradeMath.BuyAcceptable(100, TradeMath.Realizable(136, fresh.ResaleSafetyFactor), fresh.MinProfitMargin));
            Assert.False(TradeMath.BuyAcceptable(100, TradeMath.Realizable(135, fresh.ResaleSafetyFactor), fresh.MinProfitMargin));
        }

        [Fact]
        public void A_prohibitive_margin_refuses_every_purchase()
        {
            for (int buy = 1; buy <= 1000; buy += 13)
                Assert.False(TradeMath.BuyAcceptable(buy, buy * 10f, margin: 100f));
        }

        private static int WalkedOneByOne(int highest, Func<int, bool> holds)
        {
            int most = 0;
            while (most < highest && holds(most + 1)) most++;
            return most;
        }

        [Fact]
        public void The_halving_search_finds_what_walking_one_at_a_time_finds()
        {
            for (int highest = 0; highest <= 300; highest++)
                for (int threshold = 0; threshold <= highest + 1; threshold++)
                {
                    int stops = threshold;
                    Func<int, bool> holds = step => step <= stops;
                    Assert.Equal(WalkedOneByOne(highest, holds),
                                 TradeMath.MostThatHolds(highest, holds));
                }
        }

        [Fact]
        public void The_halving_search_never_asks_more_than_a_handful_of_times()
        {
            int asked = 0;
            int found = TradeMath.MostThatHolds(256, step => { asked++; return step <= 200; });
            Assert.Equal(200, found);
            Assert.True(asked <= 9, "asked " + asked + " times");
        }

        [Fact]
        public void Nothing_holds_when_the_first_step_already_fails()
        {
            Assert.Equal(0, TradeMath.MostThatHolds(50, step => false));
            Assert.Equal(50, TradeMath.MostThatHolds(50, step => true));
            Assert.Equal(0, TradeMath.MostThatHolds(0, step => true));
        }

        [Fact]
        public void A_modifier_counts_as_unchanged_only_within_a_hair_of_the_neutral_one()
        {
            Assert.True(TradeMath.Unchanged(1f, 1f));
            Assert.True(TradeMath.Unchanged(1f, 1f + TradeMath.SameModifier / 2f));
            Assert.False(TradeMath.Unchanged(1f, 0.99f));
            Assert.False(TradeMath.Unchanged(1f, 1.01f));
        }

        [Fact]
        public void A_party_that_has_stopped_still_counts_as_walking()
        {
            TradeMath.SpeedsInEffect(0f, 0f, out float land, out float sea);
            Assert.Equal(TradeMath.WalkingPace, land);
            Assert.Equal(TradeMath.WalkingPace, sea);
        }

        [Fact]
        public void A_party_with_no_ships_sails_at_the_speed_it_walks()
        {
            TradeMath.SpeedsInEffect(6f, 0f, out float land, out float sea);
            Assert.Equal(6f, land);
            Assert.Equal(6f, sea);
        }

        [Fact]
        public void A_fleet_that_can_sail_keeps_its_own_speed()
        {
            TradeMath.SpeedsInEffect(6f, 9f, out float land, out float sea);
            Assert.Equal(6f, land);
            Assert.Equal(9f, sea);
        }

        [Fact]
        public void A_fleet_sails_between_its_average_and_its_slowest_ship()
        {
            Assert.Equal(5f, TradeMath.FleetSpeed(18f, 3, 4f));
            Assert.Equal(7f, TradeMath.FleetSpeed(21f, 3, 7f));
            Assert.Equal(0f, TradeMath.FleetSpeed(0f, 0, 0f));
        }

        [Fact]
        public void One_slow_ship_holds_the_whole_fleet_back()
        {
            float even = TradeMath.FleetSpeed(21f, 3, 7f);
            float dragging = TradeMath.FleetSpeed(21f, 3, 1f);
            Assert.True(dragging < even);
        }

        [Fact]
        public void A_journey_all_on_land_is_its_distance_at_the_land_speed()
        {
            Assert.Equal(2f, TradeMath.DaysAtSpeed(240f, 1f, 5f, 10f), 4);
        }

        [Fact]
        public void A_journey_all_at_sea_is_its_distance_at_the_sea_speed()
        {
            Assert.Equal(1f, TradeMath.DaysAtSpeed(240f, 0f, 5f, 10f), 4);
        }

        [Fact]
        public void A_journey_with_a_sea_leg_counts_each_leg_at_its_own_speed()
        {
            Assert.Equal(1.5f, TradeMath.DaysAtSpeed(240f, 0.5f, 5f, 10f), 4);
        }

        [Fact]
        public void A_journey_of_no_distance_takes_no_days()
        {
            Assert.Equal(0f, TradeMath.DaysAtSpeed(0f, 1f, 5f, 10f));
            Assert.Equal(0f, TradeMath.DaysAtBestSpeed(0f, 5f, 10f));
        }

        [Fact]
        public void The_quick_estimate_never_says_a_journey_takes_longer_than_it_does()
        {
            foreach (float ratio in new[] { 0f, 0.25f, 0.5f, 0.75f, 1f })
                Assert.True(TradeMath.DaysAtBestSpeed(240f, 5f, 10f)
                            <= TradeMath.DaysAtSpeed(240f, ratio, 5f, 10f) + 1e-4f);
        }

        [Fact]
        public void A_landing_is_worth_its_units_at_the_price_of_one()
        {
            Assert.Equal(0, TradeMath.WorthOf(0, 50));
            Assert.Equal(0, TradeMath.WorthOf(10, 0));
            Assert.Equal(0, TradeMath.WorthOf(-3, 50));
            Assert.Equal(500, TradeMath.WorthOf(10, 50));
        }

        [Fact]
        public void A_landing_worth_more_than_a_number_can_hold_stops_at_the_top()
        {
            Assert.Equal(int.MaxValue, TradeMath.WorthOf(int.MaxValue, 2));
            Assert.Equal(int.MaxValue, TradeMath.ShelfAfterLanding(int.MaxValue, int.MaxValue));
            Assert.Equal(int.MaxValue, TradeMath.StockAfterLanding(int.MaxValue, 5));
        }

        [Fact]
        public void What_lands_is_added_to_what_the_market_already_holds()
        {
            Assert.Equal(1200, TradeMath.ShelfAfterLanding(1000, 200));
            Assert.Equal(1000, TradeMath.ShelfAfterLanding(1000, 0));
            Assert.Equal(0, TradeMath.ShelfAfterLanding(100, -400));
            Assert.Equal(36, TradeMath.StockAfterLanding(12, 24));
        }

        [Fact]
        public void A_landing_of_nothing_leaves_the_stock_where_it_was()
        {
            Assert.Equal(12, TradeMath.StockAfterLanding(12, 0));
            Assert.Equal(12, TradeMath.StockAfterLanding(12, -9));
        }

        [Fact]
        public void What_the_purses_on_the_road_will_take_comes_off_the_shelf_too()
        {
            Assert.Equal(26, TradeMath.StockAfterShift(12, 24, 10));
            Assert.Equal(36, TradeMath.StockAfterShift(12, 24, 0));
            Assert.Equal(36, TradeMath.StockAfterShift(12, 24, -10));
            Assert.Equal(0, TradeMath.StockAfterShift(12, 24, 900));
            Assert.Equal(int.MaxValue, TradeMath.StockAfterShift(int.MaxValue, 5, 0));
        }

        [Fact]
        public void A_moment_is_taken_up_to_the_quarter_day_so_what_lands_then_is_counted()
        {
            Assert.Equal(0.75f, TradeMath.UpToTheQuarterDay(0.75f));
            Assert.Equal(0.75f, TradeMath.UpToTheQuarterDay(0.6f));
            Assert.Equal(1f, TradeMath.UpToTheQuarterDay(0.8f));
            Assert.Equal(0.25f, TradeMath.UpToTheQuarterDay(0.01f));
            Assert.Equal(0f, TradeMath.UpToTheQuarterDay(0f));
            Assert.Equal(0f, TradeMath.UpToTheQuarterDay(-3f));
        }

        [Fact]
        public void What_lands_on_a_day_is_counted_by_the_moment_that_day_rounds_up_to()
        {
            var rng = new System.Random(9021);
            for (int round = 0; round < 20000; round++)
            {
                float days = (float)(rng.NextDouble() * 12d);
                Assert.True(TradeMath.LandsInTime(days, TradeMath.UpToTheQuarterDay(days)));
            }
        }

        [Fact]
        public void Days_are_read_back_as_hours()
        {
            Assert.Equal(24f, TradeMath.HoursOf(1f));
            Assert.Equal(6f, TradeMath.HoursOf(0.25f));
            Assert.Equal(0f, TradeMath.HoursOf(0f));
            Assert.Equal(0f, TradeMath.HoursOf(-2f));
        }

        [Fact]
        public void The_hold_is_filled_no_further_than_the_share_you_allow()
        {
            Assert.Equal(100f, TradeMath.RoomToFill(1000f, 900f, 1f));
            Assert.Equal(0f, TradeMath.RoomToFill(1000f, 900f, 0.9f));
            Assert.Equal(-400f, TradeMath.RoomToFill(1000f, 900f, 0.5f));
            Assert.Equal(100f, TradeMath.RoomToFill(1000f, 900f, 0f));
        }

        [Fact]
        public void A_companion_learns_from_a_share_of_the_profit_and_from_nothing_when_it_is_off()
        {
            Assert.Equal(50f, TradeMath.PartyShareOfProfit(200, 0.25f));
            Assert.Equal(0f, TradeMath.PartyShareOfProfit(200, 0f));
            Assert.Equal(0f, TradeMath.PartyShareOfProfit(200, -1f));
            Assert.Equal(0f, TradeMath.PartyShareOfProfit(0, 0.25f));
            Assert.Equal(0f, TradeMath.PartyShareOfProfit(-200, 0.25f));
        }

        [Fact]
        public void Cargo_counts_only_when_it_arrives_before_you_do()
        {
            Assert.True(TradeMath.LandsInTime(1.5f, 2.4f));
            Assert.True(TradeMath.LandsInTime(2.4f, 2.4f));
            Assert.False(TradeMath.LandsInTime(3f, 2.4f));
            Assert.True(TradeMath.LandsInTime(0f, 0f));
        }

        [Fact]
        public void A_party_crawling_along_is_still_credited_with_a_walking_pace()
        {
            Assert.Equal(0f, TradeMath.EtaDays(0f, 4f));
            Assert.Equal(1f, TradeMath.EtaDays(120f, 5f), 4);
            Assert.Equal(TradeMath.EtaDays(120f, TradeMath.WalkingPace),
                         TradeMath.EtaDays(120f, 0f), 4);
        }

        [Fact]
        public void A_good_that_is_cheap_at_a_market_pulls_a_traders_gold_and_a_dear_one_does_not()
        {
            Assert.Equal(0.4f, TradeMath.PullOfAPrice(0.6f), 4);
            Assert.Equal(0f, TradeMath.PullOfAPrice(1f));
            Assert.Equal(0f, TradeMath.PullOfAPrice(1.8f));
            Assert.Equal(1f, TradeMath.PullOfAPrice(0f));
            Assert.Equal(1f, TradeMath.PullOfAPrice(-3f));
        }

        [Fact]
        public void A_purse_is_split_across_a_market_by_how_cheap_each_good_is()
        {
            Assert.Equal(750, TradeMath.ShareOfAPurse(1000, 0.6f, 0.8f));
            Assert.Equal(250, TradeMath.ShareOfAPurse(1000, 0.2f, 0.8f));
            Assert.Equal(1000, TradeMath.ShareOfAPurse(1000, 0.5f, 0.5f));
        }

        [Fact]
        public void The_shares_of_one_purse_never_add_up_to_more_than_the_purse()
        {
            float[] pulls = { 0.5f, 0.3f, 0.2f };
            float across = 1f;
            int spread = 0;
            foreach (float pull in pulls) spread += TradeMath.ShareOfAPurse(900, pull, across);
            Assert.True(spread <= 900);
        }

        [Fact]
        public void A_purse_nobody_would_spend_here_moves_nothing()
        {
            Assert.Equal(0, TradeMath.ShareOfAPurse(0, 0.5f, 1f));
            Assert.Equal(0, TradeMath.ShareOfAPurse(1000, 0f, 1f));
            Assert.Equal(0, TradeMath.ShareOfAPurse(1000, 0.5f, 0f));
            Assert.Equal(0, TradeMath.ShareOfAPurse(-50, 0.5f, 1f));
        }

        [Fact]
        public void What_a_market_will_hold_is_what_lands_less_what_is_bought_off_it()
        {
            Assert.Equal(400, TradeMath.WorthShift(1000, 600));
            Assert.Equal(-600, TradeMath.WorthShift(0, 600));
            Assert.Equal(1000, TradeMath.WorthShift(1000, 0));
            Assert.Equal(1000, TradeMath.WorthShift(1000, -50));
        }

        [Fact]
        public void A_shelf_cannot_be_bought_down_past_empty()
        {
            Assert.Equal(0, TradeMath.ShelfAfterLanding(500, TradeMath.WorthShift(0, 900)));
            Assert.Equal(100, TradeMath.ShelfAfterLanding(500, TradeMath.WorthShift(0, 400)));
        }

        [Fact]
        public void A_quantity_larger_than_the_shelf_leans_on_what_is_still_coming()
        {
            Assert.True(TradeMath.StillComing(60, 20));
            Assert.False(TradeMath.StillComing(20, 20));
            Assert.False(TradeMath.StillComing(5, 20));
            Assert.True(TradeMath.StillComing(1, 0));
            Assert.True(TradeMath.StillComing(1, -4));
        }

        [Fact]
        public void A_market_nobody_counted_the_stock_of_leans_on_nothing()
        {
            Assert.False(TradeMath.StillComing(999, int.MaxValue));
        }

        [Fact]
        public void What_happened_against_what_was_said_reads_over_as_positive_and_short_as_negative()
        {
            Assert.Equal(0, TradeMath.MissedBy(300, 300));
            Assert.Equal(60, TradeMath.MissedBy(300, 360));
            Assert.Equal(-60, TradeMath.MissedBy(300, 240));
            Assert.Equal(-300, TradeMath.MissedBy(300, 0));
            Assert.Equal(540, TradeMath.MissedBy(-300, 240));
        }

        [Fact]
        public void A_miss_never_runs_off_the_end_of_what_a_number_holds()
        {
            Assert.Equal(int.MaxValue, TradeMath.MissedBy(int.MinValue, int.MaxValue));
            Assert.Equal(int.MinValue, TradeMath.MissedBy(int.MaxValue, int.MinValue));
        }

        [Fact]
        public void How_far_off_a_figure_was_is_measured_against_the_size_of_what_it_said()
        {
            Assert.Equal(0.2f, TradeMath.OffByShare(300, 240), 4);
            Assert.Equal(0.2f, TradeMath.OffByShare(300, 360), 4);
            Assert.Equal(0f, TradeMath.OffByShare(300, 300), 4);
            Assert.Equal(1f, TradeMath.OffByShare(-300, 0), 4);
            Assert.Equal(2f, TradeMath.OffByShare(300, -300), 4);
        }

        [Fact]
        public void A_figure_that_said_nothing_would_move_gets_no_share_to_be_off_by()
        {
            Assert.Equal(TradeMath.NoShareToGive, TradeMath.OffByShare(0, 400));
            Assert.Equal(TradeMath.NoShareToGive, TradeMath.OffByShare(0, 0));
        }

        [Fact]
        public void An_average_over_nothing_is_nothing_rather_than_a_break()
        {
            Assert.Equal(0f, TradeMath.MeanOf(0f, 0), 4);
            Assert.Equal(0f, TradeMath.MeanOf(12f, 0), 4);
            Assert.Equal(4f, TradeMath.MeanOf(12f, 3), 4);
            Assert.Equal(0.25f, TradeMath.MeanOf(1f, 4), 4);
        }

        [Fact]
        public void The_days_to_a_market_are_read_to_the_nearest_quarter_day()
        {
            Assert.Equal(0f, TradeMath.ToTheQuarterDay(0f), 4);
            Assert.Equal(0f, TradeMath.ToTheQuarterDay(0.1f), 4);
            Assert.Equal(0.25f, TradeMath.ToTheQuarterDay(0.125f), 4);
            Assert.Equal(0.25f, TradeMath.ToTheQuarterDay(0.3f), 4);
            Assert.Equal(0.5f, TradeMath.ToTheQuarterDay(0.4f), 4);
            Assert.Equal(2.5f, TradeMath.ToTheQuarterDay(2.4f), 4);
            Assert.Equal(2.5f, TradeMath.ToTheQuarterDay(2.6f), 4);
            Assert.Equal(2.75f, TradeMath.ToTheQuarterDay(2.63f), 4);
            Assert.Equal(0f, TradeMath.ToTheQuarterDay(-3f), 4);
        }

        [Fact]
        public void Two_trips_that_land_within_the_same_quarter_day_are_read_as_one_horizon()
        {
            Assert.Equal(TradeMath.ToTheQuarterDay(2.38f), TradeMath.ToTheQuarterDay(2.45f), 4);
            Assert.NotEqual(TradeMath.ToTheQuarterDay(2.3f), TradeMath.ToTheQuarterDay(2.45f));
        }

        [Fact]
        public void A_workshop_run_lands_sooner_the_further_along_it_already_is()
        {
            Assert.Equal(1f, TradeMath.RunLandsIn(0f, 1f), 4);
            Assert.Equal(0.25f, TradeMath.RunLandsIn(0.75f, 1f), 4);
            Assert.Equal(0f, TradeMath.RunLandsIn(1f, 1f), 4);
        }

        [Fact]
        public void A_run_never_lands_outside_the_length_of_one_run_whatever_the_progress_reads()
        {
            Assert.Equal(0f, TradeMath.RunLandsIn(4f, 1f), 4);
            Assert.Equal(1f, TradeMath.RunLandsIn(-2f, 1f), 4);
            Assert.Equal(0f, TradeMath.RunLandsIn(0.5f, 0f), 4);
            Assert.Equal(0f, TradeMath.RunLandsIn(0.5f, -1f), 4);
            for (int step = 0; step <= 100; step++)
                Assert.InRange(TradeMath.RunLandsIn(step / 100f, 1f), 0f, 1f);
        }

        [Fact]
        public void The_good_a_town_stocks_most_of_stands_for_what_a_workshop_of_that_kind_makes()
        {
            Assert.True(TradeMath.StandsBetter(5, 100, 0, 0));
            Assert.True(TradeMath.StandsBetter(5, 100, 3, 50));
            Assert.False(TradeMath.StandsBetter(2, 10, 3, 50));
            Assert.True(TradeMath.StandsBetter(3, 40, 3, 50));
            Assert.False(TradeMath.StandsBetter(3, 60, 3, 50));
        }

        [Fact]
        public void A_good_the_town_holds_none_of_never_stands_for_anything()
        {
            Assert.False(TradeMath.StandsBetter(0, 10, 0, 0));
            Assert.False(TradeMath.StandsBetter(-2, 10, 0, 0));
            Assert.False(TradeMath.StandsBetter(0, 10, 4, 90));
        }

        [Fact]
        public void A_promise_is_scored_on_the_share_of_it_the_market_still_pays()
        {
            Assert.Equal(1f, TradeMath.HeldShare(140, 140), 4);
            Assert.Equal(0.5f, TradeMath.HeldShare(140, 70), 4);
            Assert.Equal(1.25f, TradeMath.HeldShare(140, 175), 4);
            Assert.Equal(0f, TradeMath.HeldShare(140, 0), 4);
            Assert.Equal(0f, TradeMath.HeldShare(140, -9), 4);
        }

        [Fact]
        public void A_promise_with_no_price_behind_it_is_given_no_share_to_be_scored_on()
        {
            Assert.Equal(TradeMath.NoShareToGive, TradeMath.HeldShare(0, 140));
            Assert.Equal(TradeMath.NoShareToGive, TradeMath.HeldShare(-5, 140));
        }

        [Fact]
        public void A_promise_is_only_worth_scoring_while_you_arrived_near_when_it_said_you_would()
        {
            Assert.True(TradeMath.WorthScoring(2f, 2f));
            Assert.True(TradeMath.WorthScoring(2f, 5f));
            Assert.False(TradeMath.WorthScoring(2f, 5.01f));
            Assert.True(TradeMath.WorthScoring(0f, 1f));
            Assert.False(TradeMath.WorthScoring(0f, 1.5f));
            Assert.True(TradeMath.WorthScoring(-3f, 0.5f));
        }

        [Fact]
        public void Every_confidence_falls_in_one_band_and_a_dearer_one_never_falls_lower()
        {
            var seen = new System.Collections.Generic.HashSet<int>();
            int last = 0;
            for (int step = 0; step <= 100; step++)
            {
                int band = TradeMath.BandOf(step / 100f);
                Assert.InRange(band, 0, TradeMath.Bands - 1);
                Assert.True(band >= last);
                last = band;
                seen.Add(band);
            }
            Assert.Equal(TradeMath.Bands, seen.Count);
            Assert.Equal(0, TradeMath.BandOf(0.24f));
            Assert.Equal(1, TradeMath.BandOf(0.25f));
            Assert.Equal(2, TradeMath.BandOf(0.5f));
            Assert.Equal(3, TradeMath.BandOf(0.75f));
            Assert.Equal(3, TradeMath.BandOf(1f));
        }

        [Fact]
        public void Time_since_a_figure_was_written_down_is_counted_in_days_and_never_backwards()
        {
            Assert.Equal(1f, TradeMath.DaysSince(24f, 48f), 4);
            Assert.Equal(0.5f, TradeMath.DaysSince(0f, 12f), 4);
            Assert.Equal(0f, TradeMath.DaysSince(48f, 24f), 4);
            Assert.Equal(0f, TradeMath.DaysSince(48f, 48f), 4);
        }

        [Fact]
        public void A_number_that_is_not_a_number_is_read_as_the_value_asked_for_instead()
        {
            Assert.Equal(7f, TradeMath.Finite(float.NaN, 7f));
            Assert.Equal(7f, TradeMath.Finite(float.PositiveInfinity, 7f));
            Assert.Equal(7f, TradeMath.Finite(float.NegativeInfinity, 7f));
            Assert.Equal(3.5f, TradeMath.Finite(3.5f, 7f));
            Assert.Equal(0f, TradeMath.Finite(0f, 7f));
        }

        [Fact]
        public void A_distance_the_game_cannot_work_out_reads_as_far_away_rather_than_next_door()
        {
            Assert.Equal(TradeMath.FurthestThereIs, TradeMath.DaysAtSpeed(float.NaN, 1f, 5f, 5f));
            Assert.Equal(TradeMath.FurthestThereIs, TradeMath.DaysAtSpeed(100f, float.NaN, 5f, 5f));
            Assert.Equal(TradeMath.FurthestThereIs, TradeMath.DaysAtBestSpeed(float.NaN, 5f, 5f));
            Assert.Equal(TradeMath.FurthestThereIs, TradeMath.EtaDays(float.NaN, 5f));
            Assert.Equal(100f / (TradeMath.WalkingPace * 24f), TradeMath.EtaDays(100f, float.NaN), 5);
        }

        [Fact]
        public void A_travel_time_the_game_can_work_out_is_left_exactly_as_it_was()
        {
            Assert.Equal(120f / (5f * 24f), TradeMath.DaysAtSpeed(120f, 1f, 5f, 9f), 5);
            Assert.Equal(120f / (9f * 24f), TradeMath.DaysAtBestSpeed(120f, 5f, 9f), 5);
            Assert.Equal(120f / (5f * 24f), TradeMath.EtaDays(120f, 5f), 5);
            Assert.Equal(0f, TradeMath.DaysAtSpeed(0f, 1f, 5f, 5f));
            Assert.Equal(0f, TradeMath.EtaDays(-5f, 5f));
        }

        [Fact]
        public void A_window_a_price_or_a_run_that_is_not_a_number_falls_back_to_the_safe_reading()
        {
            Assert.Equal(0f, TradeMath.ToTheQuarterDay(float.NaN));
            Assert.Equal(0f, TradeMath.ToTheQuarterDay(float.PositiveInfinity));
            Assert.Equal(1f, TradeMath.RunLandsIn(float.NaN, 1f));
            Assert.Equal(0f, TradeMath.RunLandsIn(0f, float.NaN));
            Assert.Equal(0f, TradeMath.PullOfAPrice(float.NaN));
            Assert.Equal(0f, TradeMath.MeanOf(float.NaN, 4));
            Assert.Equal(0f, TradeMath.DaysSince(float.PositiveInfinity, float.PositiveInfinity));
            Assert.Equal(0f, TradeMath.Realizable(100, float.NaN));
            Assert.Equal(0f, TradeMath.FleetSpeed(float.NaN, 2, 5f));
        }

        [Fact]
        public void A_workshop_whose_progress_cannot_be_read_is_taken_as_not_started_yet()
        {
            Assert.Equal(1f, TradeMath.RunLandsIn(float.NaN, 1f));
            Assert.Equal(1f, TradeMath.RunLandsIn(0f, 1f));
            Assert.Equal(0.25f, TradeMath.RunLandsIn(0.75f, 1f), 5);
            Assert.Equal(0f, TradeMath.RunLandsIn(1f, 1f));
        }

        [Fact]
        public void Every_travel_rule_hands_back_a_real_number_whatever_it_is_handed()
        {
            float[] odd = { float.NaN, float.PositiveInfinity, float.NegativeInfinity,
                            float.MaxValue, float.MinValue, 0f, -1f, 1f };
            foreach (float a in odd)
                foreach (float b in odd)
                {
                    Assert.False(float.IsNaN(TradeMath.DaysAtSpeed(a, b, a, b)));
                    Assert.False(float.IsInfinity(TradeMath.DaysAtSpeed(a, b, a, b)));
                    Assert.False(float.IsNaN(TradeMath.DaysAtBestSpeed(a, a, b)));
                    Assert.False(float.IsInfinity(TradeMath.DaysAtBestSpeed(a, a, b)));
                    Assert.False(float.IsNaN(TradeMath.EtaDays(a, b)));
                    Assert.False(float.IsInfinity(TradeMath.EtaDays(a, b)));
                    Assert.False(float.IsNaN(TradeMath.ToTheQuarterDay(a)));
                    Assert.False(float.IsInfinity(TradeMath.ToTheQuarterDay(a)));
                    Assert.False(float.IsNaN(TradeMath.RunLandsIn(a, b)));
                    Assert.False(float.IsNaN(TradeMath.DaysSince(a, b)));
                    Assert.False(float.IsNaN(TradeMath.PullOfAPrice(a)));
                    Assert.False(float.IsNaN(TradeMath.MeanOf(a, 3)));
                }
        }

        [Fact]
        public void A_tolerance_of_one_pays_no_more_than_the_cheapest_ever_seen()
        {
            Assert.Equal(110, TradeMath.MostToPayOverTheCheapest(110, 1f));
        }

        [Fact]
        public void A_quarter_over_the_cheapest_is_what_the_shipped_tolerance_allows()
        {
            Assert.Equal(137, TradeMath.MostToPayOverTheCheapest(110, 1.25f));
            Assert.Equal(500, TradeMath.MostToPayOverTheCheapest(400, 1.25f));
        }

        [Fact]
        public void A_tolerance_below_one_never_pays_less_than_the_cheapest()
        {
            Assert.Equal(110, TradeMath.MostToPayOverTheCheapest(110, 0.5f));
            Assert.Equal(110, TradeMath.MostToPayOverTheCheapest(110, 0f));
            Assert.Equal(110, TradeMath.MostToPayOverTheCheapest(110, -3f));
        }

        [Fact]
        public void A_cheapest_of_nothing_is_no_ceiling_at_all()
        {
            Assert.Equal(0, TradeMath.MostToPayOverTheCheapest(0, 1.25f));
            Assert.Equal(0, TradeMath.MostToPayOverTheCheapest(-40, 1.25f));
        }

        [Fact]
        public void A_tolerance_that_is_not_a_number_falls_back_to_the_cheapest()
        {
            Assert.Equal(110, TradeMath.MostToPayOverTheCheapest(110, float.NaN));
        }

        [Fact]
        public void The_ceiling_never_overflows_however_large_the_tolerance()
        {
            Assert.Equal(int.MaxValue,
                         TradeMath.MostToPayOverTheCheapest(int.MaxValue, float.PositiveInfinity));
            Assert.Equal(int.MaxValue, TradeMath.MostToPayOverTheCheapest(int.MaxValue, 1000f));
        }

        [Fact]
        public void The_ceiling_never_falls_below_the_cheapest_at_any_tolerance()
        {
            var roll = new System.Random(5518);
            for (int i = 0; i < 20000; i++)
            {
                int cheapest = roll.Next(1, 50000);
                float tolerance = (float)(roll.NextDouble() * 4.0);
                int ceiling = TradeMath.MostToPayOverTheCheapest(cheapest, tolerance);
                Assert.True(ceiling >= cheapest);
                if (tolerance > 1f) Assert.True(ceiling <= (long)cheapest * 4L + 1L);
            }
        }

        [Fact]
        public void A_ride_longer_than_any_on_the_map_is_out_of_reach()
        {
            Assert.False(TradeMath.OutOfReach(0f));
            Assert.False(TradeMath.OutOfReach(999f));
            Assert.True(TradeMath.OutOfReach(TradeMath.LongerThanAnyRide));
            Assert.True(TradeMath.OutOfReach(float.MaxValue));
            Assert.True(TradeMath.OutOfReach(float.NaN));
            Assert.True(TradeMath.OutOfReach(float.PositiveInfinity));
            Assert.True(TradeMath.DaysAtSpeed(float.MaxValue, 1f, 4.8f, 4.8f) == TradeMath.FurthestThereIs);
            Assert.True(TradeMath.DaysAtBestSpeed(float.MaxValue, 4.8f, 4.8f) == TradeMath.FurthestThereIs);
            Assert.True(TradeMath.DaysAtSpeed(240f, 1f, 10f, 10f) == 1f);
        }

        [Fact]
        public void A_forecast_far_above_the_live_price_is_held_to_the_cap()
        {
            Assert.Equal(150, TradeMath.ForecastWithin(100, 900));
            Assert.Equal(120, TradeMath.ForecastWithin(100, 120));
        }

        [Fact]
        public void A_forecast_far_below_the_live_price_is_held_to_the_cap()
        {
            Assert.Equal(58, TradeMath.ForecastWithin(117, 15));
            Assert.Equal(80, TradeMath.ForecastWithin(100, 80));
            Assert.Equal(15, TradeMath.ForecastWithin(0, 15));
        }

        [Fact]
        public void A_caravan_passing_a_town_that_pays_no_more_than_usual_leaves_nothing()
        {
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, 1.0f, 500f, 10));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, TradeMath.ACaravanSellsAbove, 500f, 10));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, 0.5f, 500f, 10));
        }

        [Fact]
        public void A_caravan_leaves_no_more_than_the_town_daily_purse_for_that_kind_can_pay_for()
        {
            Assert.Equal(30, TradeMath.WhatACaravanUnloads(100, 1.2f, 1000f, 10));
            Assert.Equal(100, TradeMath.WhatACaravanUnloads(100, 2.0f, 1000f, 10));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, 1.2f, 0f, 10));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(0, 2.0f, 1000f, 10));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, 2.0f, 1000f, 0));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, float.NaN, 1000f, 10));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, 1.2f, float.NaN, 10));
        }

        [Fact]
        public void A_shelf_holding_enough_units_counts_whatever_a_unit_costs()
        {
            Assert.True(TradeMath.EnoughOnTheShelf(10, 19, 10, 500));
            Assert.True(TradeMath.EnoughOnTheShelf(47, 19, 10, 500));
            Assert.False(TradeMath.EnoughOnTheShelf(9, 19, 10, 500));
        }

        [Fact]
        public void A_shelf_short_on_units_still_counts_when_what_it_holds_is_worth_enough()
        {
            Assert.True(TradeMath.EnoughOnTheShelf(2, 250, 10, 500));
            Assert.True(TradeMath.EnoughOnTheShelf(2, 290, 10, 500));
            Assert.True(TradeMath.EnoughOnTheShelf(5, 100, 10, 500));
            Assert.False(TradeMath.EnoughOnTheShelf(1, 250, 10, 500));
            Assert.False(TradeMath.EnoughOnTheShelf(9, 25, 10, 500));
        }

        [Fact]
        public void A_shop_down_to_its_last_unit_cannot_pass_by_asking_a_high_price()
        {
            Assert.False(TradeMath.EnoughOnTheShelf(1, 250, 10, 500));
            Assert.False(TradeMath.EnoughOnTheShelf(2, 25, 10, 500));
            Assert.True(TradeMath.EnoughOnTheShelf(10, 10, 10, 500));
        }

        [Fact]
        public void A_shelf_is_counted_the_way_the_game_counts_one_at_the_goods_own_worth()
        {
            Assert.Equal(500, TradeMath.WorthOf(2, 250));
            Assert.True(TradeMath.EnoughOnTheShelf(2, 250, 10, TradeMath.WorthOf(2, 250)));
            Assert.False(TradeMath.EnoughOnTheShelf(2, 250, 10, TradeMath.WorthOf(2, 250) + 1));
        }

        [Fact]
        public void A_worth_floor_of_zero_leaves_the_unit_floor_exactly_as_it_was()
        {
            Assert.False(TradeMath.EnoughOnTheShelf(2, 250, 10, 0));
            Assert.False(TradeMath.EnoughOnTheShelf(9, 10, 10, 0));
            Assert.True(TradeMath.EnoughOnTheShelf(10, 10, 10, 0));
        }

        [Fact]
        public void No_unit_floor_at_all_lets_every_market_through_as_it_always_did()
        {
            Assert.True(TradeMath.EnoughOnTheShelf(0, 0, 0, 0));
            Assert.True(TradeMath.EnoughOnTheShelf(0, 0, 0, 500));
            Assert.True(TradeMath.EnoughOnTheShelf(1, 19, 0, 500));
        }

        [Fact]
        public void An_empty_shelf_or_an_unpriced_good_never_clears_the_worth_floor()
        {
            Assert.False(TradeMath.EnoughOnTheShelf(0, 250, 10, 500));
            Assert.False(TradeMath.EnoughOnTheShelf(-3, 250, 10, 500));
            Assert.False(TradeMath.EnoughOnTheShelf(2, 0, 10, 500));
            Assert.False(TradeMath.EnoughOnTheShelf(2, -250, 10, 500));
        }

        [Fact]
        public void A_shelf_worth_more_than_an_int_can_hold_still_clears_the_floor()
        {
            Assert.True(TradeMath.EnoughOnTheShelf(100000, 100000, 1000000, 500));
        }

        [Fact]
        public void A_pick_is_worth_what_you_could_actually_take_of_it_not_its_percentage()
        {
            Assert.Equal(60f, TradeMath.WhatThisPickWouldMake(20f, 50, 1f, 3, 100000, 1000f));
            Assert.Equal(100f, TradeMath.WhatThisPickWouldMake(50f, 500, 1f, 2, 100000, 1000f));
        }

        [Fact]
        public void A_thin_purse_holds_the_take_down_to_what_it_can_pay_for()
        {
            Assert.Equal(400f, TradeMath.WhatThisPickWouldMake(20f, 50, 1f, 100, 1000, 1000f));
            Assert.Equal(100f, TradeMath.WhatThisPickWouldMake(50f, 500, 1f, 100, 1000, 1000f));
            Assert.Equal(0f, TradeMath.WhatThisPickWouldMake(50f, 500, 1f, 100, 0, 1000f));
        }

        [Fact]
        public void A_full_cargo_holds_the_take_down_to_what_still_fits()
        {
            Assert.Equal(200f, TradeMath.WhatThisPickWouldMake(20f, 50, 1f, 100, 100000, 10f));
            Assert.Equal(5000f, TradeMath.WhatThisPickWouldMake(50f, 500, 0.1f, 100, 100000, 10f));
            Assert.Equal(0f, TradeMath.WhatThisPickWouldMake(20f, 50, 1f, 100, 100000, 0f));
        }

        [Fact]
        public void A_good_that_weighs_nothing_is_held_back_by_the_purse_alone()
        {
            Assert.Equal(1000f, TradeMath.WhatThisPickWouldMake(50f, 500, 0f, 100, 10000, 0.0001f));
            Assert.Equal(0f, TradeMath.WhatThisPickWouldMake(50f, 500, 1f, 100, 10000, 0.0001f));
        }

        [Fact]
        public void A_pick_that_would_make_nothing_is_worth_nothing()
        {
            Assert.Equal(0f, TradeMath.WhatThisPickWouldMake(0f, 50, 1f, 10, 1000, 100f));
            Assert.Equal(0f, TradeMath.WhatThisPickWouldMake(-5f, 50, 1f, 10, 1000, 100f));
            Assert.Equal(0f, TradeMath.WhatThisPickWouldMake(20f, 0, 1f, 10, 1000, 100f));
            Assert.Equal(0f, TradeMath.WhatThisPickWouldMake(20f, 50, 1f, 0, 1000, 100f));
            Assert.Equal(0f, TradeMath.WhatThisPickWouldMake(20f, 50, 1f, 10, 1000, float.NaN));
            Assert.Equal(0f, TradeMath.WhatThisPickWouldMake(20f, 500, 1f, 10, 100, 100f));
        }

        [Fact]
        public void A_nearer_buyer_paying_a_little_less_earns_more_a_day_than_a_far_one()
        {
            float near = TradeMath.EarnedPerDay(400, 300, 0.3f);
            float far = TradeMath.EarnedPerDay(420, 300, 2.3f);
            Assert.True(near > far);
            Assert.Equal(100f / 0.3f, near, 2);
            Assert.Equal(120f / 2.3f, far, 2);
        }

        [Fact]
        public void A_far_buyer_paying_much_more_still_wins()
        {
            Assert.True(TradeMath.EarnedPerDay(900, 300, 2.3f) >
                        TradeMath.EarnedPerDay(310, 300, 0.3f));
        }

        [Fact]
        public void A_trip_shorter_than_a_quarter_day_counts_as_a_quarter_day()
        {
            Assert.Equal(TradeMath.EarnedPerDay(400, 300, 0f),
                         TradeMath.EarnedPerDay(400, 300, TradeMath.NoTripCountsShorterThan));
            Assert.Equal(400f, TradeMath.EarnedPerDay(400, 300, 0.01f));
        }

        [Fact]
        public void A_buyer_paying_no_more_than_you_paid_earns_nothing_a_day()
        {
            Assert.Equal(0f, TradeMath.EarnedPerDay(300, 300, 1f));
            Assert.Equal(0f, TradeMath.EarnedPerDay(200, 300, 1f));
            Assert.Equal(0f, TradeMath.EarnedPerDay(0, 300, 1f));
            Assert.Equal(0f, TradeMath.EarnedPerDay(400, 300, float.NaN));
        }
    }
}
