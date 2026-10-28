using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class TradeMathTests
    {
        private const int AveragePaid = 0, LastPaid = 1, CheapestKnown = 2;

        private static long _numbered = TradeMath.FirstUnitNumber;

        private static void Buy(PurchaseRecord rec, int count, int totalPaid) =>
            BuyEach(rec, TradeMath.SpreadOver(count, totalPaid));

        private static void BuyEach(PurchaseRecord rec, params int[] paid)
        {
            TradeMath.AddPurchase(rec, paid, _numbered, 100f);
            _numbered += paid.Length;
        }

        private static PurchaseRecord Bought(int count, int totalPaid)
        {
            var rec = new PurchaseRecord { ItemId = "grain" };
            Buy(rec, count, totalPaid);
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
        public void Selling_what_you_bought_takes_only_the_units_you_bought()
        {
            int remaining = 10;
            Assert.True(TradeMath.KeepToTheBoughtUnits(ref remaining, 4));
            Assert.Equal(4, remaining);
            remaining = 3;
            Assert.True(TradeMath.KeepToTheBoughtUnits(ref remaining, 4));
            Assert.Equal(3, remaining);
            remaining = 10;
            Assert.False(TradeMath.KeepToTheBoughtUnits(ref remaining, 0));
            Assert.Equal(10, remaining);
            remaining = 0;
            Assert.False(TradeMath.KeepToTheBoughtUnits(ref remaining, 4));
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
            Buy(rec, 10, 300);
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
        public void Selling_a_lot_down_one_at_a_time_leaves_the_rest_costing_what_those_units_cost()
        {
            foreach (var lot in new[]
                     {
                         (count: 9, each: 50), (count: 13, each: 200), (count: 7, each: 100),
                         (count: 3, each: 10), (count: 6, each: 25)
                     })
            {
                var rec = new PurchaseRecord { ItemId = "grain" };
                BuyEach(rec, TradeMath.AtOnePrice(lot.count, lot.each));
                while (rec.Count > 1)
                {
                    TradeMath.DrainSale(rec, 1);
                    Assert.Equal(lot.each, TradeMath.UnitBasis(rec, AveragePaid));
                }
            }
            var rising = new PurchaseRecord { ItemId = "wine" };
            BuyEach(rising, 33, 34, 35, 38);
            TradeMath.DrainSale(rising, 1);
            Assert.Equal(34 + 35 + 38, rising.TotalPaid);
            Assert.Equal(36, TradeMath.UnitBasis(rising, AveragePaid));
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
                Buy(rec, round % 5 + 1, round * 13 % 97 + 1);
                TradeMath.DrainSale(rec, round % 7 + 1);
                Assert.True(rec.Count >= 0);
                Assert.True(rec.TotalPaid >= 0);
                int unit = TradeMath.UnitBasis(rec, AveragePaid);
                Assert.True(unit == TradeMath.NoRecordedBasis || unit >= 0);
            }
        }

        [Fact]
        public void A_unit_the_average_would_sell_at_a_loss_keeps_its_own_price()
        {
            var rec = Bought(1, 836);
            Buy(rec, 4, 1114);
            int basis = TradeMath.UnitBasis(rec, AveragePaid);
            Assert.Equal(390, basis);
            Assert.Equal(448, TradeMath.WhatTheAverageCovers(basis, 0.15f));
            Assert.Equal(new[] { 836 }, Dear(rec, TradeMath.WhatTheAverageCovers(basis, 0.15f), rec.Count));
            Assert.Equal(new[] { 278, 278, 279, 279, 836 }, Units(TradeMath.UnitCosts(rec, rec.Count)));
        }

        [Fact]
        public void Units_bought_up_one_rising_price_are_all_held_to_the_average_as_before()
        {
            var rec = new PurchaseRecord { ItemId = "linen" };
            foreach (int paid in new[] { 249, 257, 266, 275, 286, 298 }) Buy(rec, 1, paid);
            int basis = TradeMath.UnitBasis(rec, AveragePaid);
            Assert.Equal(272, basis);
            Assert.Null(Dear(rec, TradeMath.WhatTheAverageCovers(basis, 0.15f), rec.Count));
        }

        [Fact]
        public void The_average_covers_its_margin_and_no_basis_covers_nothing()
        {
            Assert.Equal(115, TradeMath.WhatTheAverageCovers(100, 0.15f));
            Assert.Equal(100, TradeMath.WhatTheAverageCovers(100, 0f));
            Assert.Equal(100, TradeMath.WhatTheAverageCovers(100, float.NaN));
            Assert.Equal(TradeMath.NoRecordedBasis, TradeMath.WhatTheAverageCovers(TradeMath.NoRecordedBasis, 0.15f));
        }

        [Fact]
        public void Goods_bought_at_one_price_have_no_unit_dearer_than_what_they_cost()
        {
            var rec = Bought(6, 600);
            Assert.Null(Dear(rec, TradeMath.UnitBasis(rec, AveragePaid), rec.Count));
            Assert.Equal(new[] { 100, 100, 100, 100, 100, 100 }, Units(TradeMath.UnitCosts(rec, rec.Count)));
            Assert.Null(TradeMath.UnitCosts(null, 6));
        }

        [Fact]
        public void A_sale_is_taken_from_the_cheapest_units_first_and_the_average_is_what_the_units_left_cost()
        {
            var rec = Bought(1, 836);
            BuyEach(rec, 277, 278, 279, 280);
            Assert.Equal(390, TradeMath.UnitBasis(rec, AveragePaid));
            TradeMath.DrainSale(rec, 4);
            Assert.Equal(1, rec.Count);
            Assert.Equal(836, rec.TotalPaid);
            Assert.Equal(836, TradeMath.UnitBasis(rec, AveragePaid));
            TradeMath.DrainSale(rec, 1);
            Assert.Empty(rec.Batches);
            Assert.Null(TradeMath.UnitCosts(rec, 1));
        }

        [Fact]
        public void The_last_price_paid_mode_still_never_lets_a_dearer_unit_go_under_what_it_cost()
        {
            var rec = Bought(1, 836);
            Buy(rec, 4, 1114);
            int basis = TradeMath.UnitBasis(rec, LastPaid);
            Assert.Equal(278, basis);
            Assert.Equal(new[] { 279, 279, 836 }, Dear(rec, basis, rec.Count));
        }

        private static Batch[] Costs(params int[] units)
        {
            var batches = new List<Batch>();
            foreach (int unit in units)
            {
                int last = batches.Count - 1;
                if (last >= 0 && batches[last].Unit == unit)
                {
                    Batch same = batches[last];
                    same.Count++;
                    batches[last] = same;
                }
                else batches.Add(new Batch { Unit = unit, Count = 1 });
            }
            return batches.Count == 0 ? null : batches.ToArray();
        }

        private static int[] Units(Batch[] costs)
        {
            if (costs == null) return null;
            var units = new List<int>();
            foreach (Batch one in costs)
                for (int u = 0; u < one.Count; u++) units.Add(one.Unit);
            return units.ToArray();
        }

        private static int[] Dear(PurchaseRecord rec, int covers, int held)
        {
            int[] costs = Units(TradeMath.UnitCosts(rec, held));
            if (costs == null) return null;
            var dear = new List<int>();
            foreach (int cost in costs) if (cost > covers) dear.Add(cost);
            return dear.Count == 0 ? null : dear.ToArray();
        }

        private static List<int> Walked(TradeMath.DearFirst walk, int units, Func<int, int> priceAt, float margin = 0.15f)
        {
            var sold = new List<int>();
            for (int u = 0; u < units; u++)
            {
                int price = priceAt(u);
                if (!TradeMath.ProfitAcceptable(walk.Floor(price), price, margin)) break;
                sold.Add(walk.Took());
            }
            return sold;
        }

        [Fact]
        public void The_dear_unit_takes_the_best_price_rather_than_the_one_left_after_the_cheap_ones()
        {
            var sold = Walked(new TradeMath.DearFirst(Costs(278, 278, 278, 278, 836), 448, 390), 5, u => 1135 - 109 * u);
            Assert.Equal(new[] { 836, 278, 278, 278, 278 }, sold.ToArray());
        }

        [Fact]
        public void A_price_under_what_the_dear_unit_cost_sells_the_cheap_ones_first_and_the_dear_one_last()
        {
            var sold = Walked(new TradeMath.DearFirst(Costs(278, 278, 278, 278, 836), 448, 390), 5, u => 484);
            Assert.Equal(new[] { 278, 278, 278, 278, 836 }, sold.ToArray());
        }

        [Fact]
        public void Of_several_dear_units_the_dearest_the_price_clears_goes_first()
        {
            var sold = Walked(new TradeMath.DearFirst(Costs(100, 100, 500, 900), 115, 100), 4, u => 700 - 10 * u);
            Assert.Equal(new[] { 500, 100, 100, 900 }, sold.ToArray());
            sold = Walked(new TradeMath.DearFirst(Costs(100, 100, 500, 900), 115, 100), 4, u => 1100 - 10 * u);
            Assert.Equal(new[] { 900, 500, 100, 100 }, sold.ToArray());
        }

        [Fact]
        public void Of_several_dear_units_a_price_covers_the_dearest_is_booked_and_the_dearer_one_waits()
        {
            var sold = Walked(new TradeMath.DearFirst(Costs(100, 400, 500, 900), 115, 100), 4, u => 600);
            Assert.Equal(new[] { 500, 400, 100, 900 }, sold.ToArray());
        }

        [Fact]
        public void Dear_units_a_price_passed_over_are_still_taken_cheapest_first_and_never_twice()
        {
            var walk = new TradeMath.DearFirst(Costs(100, 300, 300, 400, 500, 500, 900), 115, 100);
            Assert.Equal(100, walk.Floor(2000));
            Assert.Equal(900, walk.Took());
            Assert.Equal(100, walk.Floor(510));
            Assert.Equal(500, walk.Took());
            Assert.Equal(100, walk.Floor(350));
            Assert.Equal(300, walk.Took());
            TradeMath.DearFirst copy = walk;
            var rest = new List<int>();
            for (int u = 0; u < 4; u++) rest.Add(walk.Took());
            Assert.Equal(new[] { 100, 300, 400, 500 }, rest.ToArray());
            Assert.Equal(TradeMath.NoUnitThisPriceSells, walk.Floor(5000));
            Assert.Equal(100, copy.Took());
            Assert.Equal(300, copy.Took());
        }

        [Fact]
        public void With_no_dear_unit_the_walk_is_the_average_it_always_was()
        {
            var walk = new TradeMath.DearFirst(null, 115, 100, 3);
            Assert.Equal(100, walk.Floor(116));
            Assert.Equal(100, walk.Took());
            var sold = Walked(new TradeMath.DearFirst(null, 115, 100, 3), 5, u => 116);
            Assert.Equal(3, sold.Count);
        }

        [Fact]
        public void Units_taken_without_asking_a_price_come_off_the_cheap_ones_first()
        {
            var walk = new TradeMath.DearFirst(Costs(100, 500, 900), 115, 100);
            Assert.Equal(100, walk.Took());
            Assert.Equal(500, walk.Took());
            Assert.Equal(900, walk.Took());
            Assert.Equal(100, walk.Took());
        }

        [Fact]
        public void A_copy_of_the_walk_leaves_the_one_it_was_copied_from_where_it_was()
        {
            var first = new TradeMath.DearFirst(Costs(100, 100, 500, 500, 900), 115, 100);
            TradeMath.DearFirst copy = first;
            Assert.Equal(new[] { 900, 500, 500, 100, 100 }, Walked(copy, 5, u => 2000).ToArray());
            Assert.Equal(new[] { 900, 500, 500, 100, 100 }, Walked(first, 5, u => 2000).ToArray());
        }

        [Fact]
        public void Units_taken_from_both_ends_of_one_dear_batch_never_come_to_more_than_it_holds()
        {
            var walk = new TradeMath.DearFirst(Costs(500, 500, 500), 115, 100);
            Assert.Equal(100, walk.Floor(600));
            Assert.Equal(500, walk.Took());
            Assert.Equal(500, walk.Took());
            Assert.Equal(500, walk.Took());
            Assert.Equal(TradeMath.NoUnitThisPriceSells, walk.Floor(600));
            Assert.Equal(100, walk.Took());
        }

        [Fact]
        public void Units_already_drawn_are_left_out_of_the_walk_from_the_cheapest_up()
        {
            Batch[] costs = Costs(500, 900);
            TradeMath.DropTheCheapest(ref costs, 1);
            var sold = Walked(new TradeMath.DearFirst(costs, 115, 100), 2, u => 2000);
            Assert.Equal(new[] { 900 }, sold.ToArray());
            TradeMath.DropTheCheapest(ref costs, 5);
            Assert.Null(costs);
        }

        [Fact]
        public void Every_price_paid_is_kept_apart_however_many_prices_there_are()
        {
            var rec = Bought(1, 5000);
            for (int price = 100; price < 130; price++) Buy(rec, 1, price);
            Assert.Equal(31, rec.Batches.Count);
            Assert.True(TradeMath.BatchesAddUp(rec));
            Assert.Equal(5000, rec.Batches[rec.Batches.Count - 1].Unit);
            Assert.Equal(1, rec.Batches[rec.Batches.Count - 1].Count);
            var paid = new List<int>();
            for (int price = 100; price < 130; price++) paid.Add(price);
            paid.Add(5000);
            Assert.Equal(paid.ToArray(), Units(TradeMath.UnitCosts(rec, rec.Count)));
        }

        [Fact]
        public void The_batches_always_add_up_to_what_the_record_holds_however_goods_come_and_go()
        {
            var rng = new Random(9601);
            var rec = Bought(3, 90);
            for (int round = 0; round < 5000; round++)
            {
                if (rng.Next(3) > 0) Buy(rec, rng.Next(1, 6), rng.Next(1, 4000));
                else TradeMath.DrainSale(rec, rng.Next(0, 9));
                Assert.True(TradeMath.BatchesAddUp(rec));
                int basis = TradeMath.UnitBasis(rec, AveragePaid);
                Batch[] costs = TradeMath.UnitCosts(rec, rec.Count);
                Assert.True(costs == null ? rec.Count == 0 : TradeMath.UnitsIn(costs) == rec.Count);
            }
        }

        [Fact]
        public void A_record_kept_before_batches_were_written_down_counts_as_one_batch_at_its_average()
        {
            var rec = new PurchaseRecord { ItemId = "felt", TotalPaid = 1200, Count = 4, LastUnitPaid = 300 };
            Buy(rec, 1, 900);
            Assert.True(TradeMath.BatchesAddUp(rec));
            Assert.Equal(2, rec.Batches.Count);
            Assert.Equal(300, rec.Batches[0].Unit);
            Assert.Equal(4, rec.Batches[0].Count);
            Assert.Equal(new[] { 900 }, Dear(rec, TradeMath.UnitBasis(rec, AveragePaid), rec.Count));
        }

        [Fact]
        public void Selling_nothing_leaves_the_record_exactly_as_it_was()
        {
            var rec = Bought(4, 1114);
            int paid = rec.TotalPaid;
            TradeMath.DrainSale(rec, 0);
            TradeMath.DrainSale(rec, -3);
            Assert.Equal(4, rec.Count);
            Assert.Equal(paid, rec.TotalPaid);
        }

        [Fact]
        public void With_no_walk_in_judged_yet_the_resale_safety_factor_you_set_is_the_one_used()
        {
            Assert.Equal(0.85f, TradeMath.ResaleSafetyAsPromisesHeld(0.85f, true, 0, TradeMath.NoShareToGive), 4);
            Assert.Equal(0.85f, TradeMath.ResaleSafetyAsPromisesHeld(0.85f, true, 40, TradeMath.NoShareToGive), 4);
            Assert.Equal(0.85f, TradeMath.ResaleSafetyAsPromisesHeld(0.85f, true, 40, float.NaN), 4);
        }

        [Fact]
        public void With_learning_off_the_factor_you_set_is_used_however_the_prices_held()
        {
            Assert.Equal(0.85f, TradeMath.ResaleSafetyAsPromisesHeld(0.85f, false, 5000, 1f), 4);
            Assert.Equal(0.6f, TradeMath.ResaleSafetyAsPromisesHeld(0.6f, false, 5000, 0.95f), 4);
        }

        [Fact]
        public void Prices_that_held_at_their_promise_never_raise_the_safety_above_what_you_set()
        {
            Assert.Equal(0.85f, TradeMath.ResaleSafetyAsPromisesHeld(0.85f, true, TradeMath.EnoughWalkIns, 1f), 4);
            Assert.Equal(0.85f, TradeMath.ResaleSafetyAsPromisesHeld(0.85f, true, 5000, 1f), 4);
            Assert.Equal(0.6f, TradeMath.ResaleSafetyAsPromisesHeld(0.6f, true, 5000, 0.9f), 4);
        }

        [Fact]
        public void The_owners_own_record_of_prices_holding_at_96_percent_leaves_the_safety_as_set()
        {
            Assert.Equal(0.85f, TradeMath.ResaleSafetyAsPromisesHeld(0.85f, true, 51, 0.958f), 4);
        }

        [Fact]
        public void Prices_that_held_under_what_you_set_lower_the_safety_toward_them()
        {
            Assert.Equal(0.85f, TradeMath.ResaleSafetyAsPromisesHeld(0.9f, true, TradeMath.EnoughWalkIns, 0.8f), 3);
        }

        [Fact]
        public void Prices_that_fall_short_move_the_safety_down_but_never_under_half()
        {
            float lower = TradeMath.ResaleSafetyAsPromisesHeld(0.85f, true, 100000, 0.3f);
            Assert.True(lower < 0.52f);
            Assert.True(lower >= TradeMath.LeastResaleSafety);
            Assert.Equal(0.75f, TradeMath.ResaleSafetyAsPromisesHeld(1f, true, TradeMath.EnoughWalkIns, 0.5f), 3);
        }

        [Fact]
        public void A_few_walk_ins_that_fall_short_move_the_safety_only_a_little()
        {
            float after = TradeMath.ResaleSafetyAsPromisesHeld(0.85f, true, 2, 0.6f);
            Assert.True(after > 0.8f && after < 0.85f);
        }

        [Fact]
        public void A_sale_of_a_dear_unit_is_taken_from_its_own_batch()
        {
            var rec = new PurchaseRecord { ItemId = "felt" };
            Buy(rec, 1, 836);
            Buy(rec, 4, 1114);
            TradeMath.DrainSale(rec, 1, 836);
            Assert.Equal(4, rec.Batches.Count);
            Assert.Equal(new[] { 278, 278, 279, 279 }, Units(TradeMath.UnitCosts(rec, rec.Count)));
            TradeMath.DrainSale(rec, 1, 999);
            Assert.Equal(3, rec.Batches.Count);
        }

        [Fact]
        public void Nine_prices_paid_are_nine_prices_kept_and_none_is_averaged_into_another()
        {
            var rec = new PurchaseRecord { ItemId = "felt" };
            foreach (int paid in new[] { 100, 101, 110, 111, 120, 121, 130, 500, 900 }) Buy(rec, 1, paid);
            Assert.Equal(9, rec.Batches.Count);
            Assert.Equal(new[] { 100, 101, 110, 111, 120, 121, 130, 500, 900 },
                         Units(TradeMath.UnitCosts(rec, rec.Count)));
        }

        private static long[] NumbersOf(PurchaseRecord rec)
        {
            var numbers = new List<long>();
            foreach (Batch one in rec.Batches)
                for (int u = 0; u < one.Count; u++) numbers.Add(one.First + u);
            numbers.Sort();
            return numbers.ToArray();
        }

        private static int[] CostsOf(List<TradeMath.SoldUnit> sold)
        {
            var costs = new List<int>();
            foreach (TradeMath.SoldUnit one in sold) if (one.Bought) costs.Add(one.Cost);
            return costs.ToArray();
        }

        private static TradeMath.DearFirst OwnCost(Batch[] costs, int worth = 200, int unknown = 0) =>
            TradeMath.DearFirst.EachAtItsOwnCost(costs, worth, unknown, 0.15f);

        [Fact]
        public void Each_unit_at_its_own_cost_sells_the_dearest_the_price_clears_by_your_margin_first()
        {
            Assert.Equal(new[] { 200, 100 }, Walked(OwnCost(Costs(100, 200, 300)), 3, u => 260).ToArray());
        }

        [Fact]
        public void Each_unit_at_its_own_cost_goes_down_the_ladder_as_the_price_falls()
        {
            Assert.Equal(new[] { 300, 200, 100 }, Walked(OwnCost(Costs(100, 200, 300)), 3, u => 400 - 50 * u).ToArray());
            Assert.Equal(new[] { 300, 100 }, Walked(OwnCost(Costs(100, 200, 300)), 3, u => 345 - 120 * u).ToArray());
        }

        [Fact]
        public void A_unit_whose_own_cost_the_price_cannot_clear_by_your_margin_never_sells()
        {
            var walk = OwnCost(Costs(300));
            Assert.Equal(TradeMath.NoUnitThisPriceSells, walk.Floor(344));
            Assert.Equal(300, walk.Floor(345));
            Assert.Equal(300, walk.Took());
            Assert.Equal(TradeMath.NoUnitThisPriceSells, walk.Floor(5000));
        }

        [Fact]
        public void Units_of_no_known_cost_go_at_the_average_once_no_unit_with_a_cost_clears()
        {
            var walk = OwnCost(Costs(300), worth: 200, unknown: 2);
            Assert.Equal(200, walk.Floor(250));
            Assert.Equal(200, walk.Took());
            Assert.Equal(300, walk.Floor(400));
            Assert.Equal(300, walk.Took());
            Assert.Equal(200, walk.Floor(400));
            Assert.Equal(200, walk.Took());
            Assert.Equal(TradeMath.NoUnitThisPriceSells, walk.Floor(400));
        }

        [Fact]
        public void A_copy_of_an_own_cost_walk_leaves_the_one_it_was_copied_from_where_it_was()
        {
            var first = OwnCost(Costs(100, 200, 200, 300));
            TradeMath.DearFirst copy = first;
            Assert.Equal(new[] { 300, 200, 200, 100 }, Walked(copy, 4, u => 1000).ToArray());
            Assert.Equal(new[] { 300, 200, 200, 100 }, Walked(first, 4, u => 1000).ToArray());
        }

        [Fact]
        public void An_own_cost_walk_never_takes_more_units_than_it_holds_however_they_are_taken()
        {
            var walk = OwnCost(Costs(100, 100, 300));
            Assert.Equal(300, walk.Floor(400));
            Assert.Equal(300, walk.Took());
            Assert.Equal(100, walk.Took());
            Assert.Equal(100, walk.Floor(400));
            Assert.Equal(100, walk.Took());
            Assert.Equal(TradeMath.NoUnitThisPriceSells, walk.Floor(400));
            var one = OwnCost(Costs(500, 500, 500));
            Assert.Equal(500, one.Took());
            Assert.Equal(500, one.Floor(600));
            Assert.Equal(500, one.Took());
            Assert.Equal(500, one.Floor(600));
            Assert.Equal(500, one.Took());
            Assert.Equal(TradeMath.NoUnitThisPriceSells, one.Floor(600));
        }

        [Theory]
        [InlineData(300, 0.15f, 345)]
        [InlineData(100, 0.15f, 115)]
        [InlineData(100, 0f, 100)]
        [InlineData(0, 0.15f, 1)]
        public void The_least_price_that_clears_a_unit_is_the_least_your_margin_accepts(int cost, float margin, int least)
        {
            Assert.Equal(least, TradeMath.LeastThatClears(cost, margin));
            Assert.True(TradeMath.ProfitAcceptable(cost, least, margin));
            Assert.False(TradeMath.ProfitAcceptable(cost, least - 1, margin));
        }

        [Fact]
        public void Every_unit_bought_has_a_number_and_a_sale_takes_the_number_of_the_unit_it_sold()
        {
            var rec = new PurchaseRecord { ItemId = "cloth" };
            TradeMath.AddPurchase(rec, TradeMath.AtOnePrice(1, 100), 1L, 100f);
            TradeMath.AddPurchase(rec, TradeMath.AtOnePrice(1, 200), 2L, 100f);
            TradeMath.AddPurchase(rec, TradeMath.AtOnePrice(1, 300), 3L, 100f);
            Assert.Equal(3L, TradeMath.NumberASaleTakes(rec, 300));
            TradeMath.DrainSale(rec, 1, 300);
            Assert.Equal(new[] { 100, 200 }, Units(TradeMath.UnitCosts(rec, rec.Count)));
            Assert.Equal(2L, TradeMath.NumberASaleTakes(rec, 200));
            Assert.Equal(1L, TradeMath.NumberASaleTakes(rec, 0));
            Assert.Equal(0L, TradeMath.NumberASaleTakes(null, 100));
        }

        [Fact]
        public void Units_bought_one_after_another_at_one_price_keep_a_number_and_a_row_each()
        {
            var rec = new PurchaseRecord { ItemId = "cloth" };
            TradeMath.AddPurchase(rec, TradeMath.AtOnePrice(2, 100), 10L, 100f);
            TradeMath.AddPurchase(rec, TradeMath.AtOnePrice(3, 100), 12L, 100f);
            TradeMath.AddPurchase(rec, TradeMath.AtOnePrice(1, 100), 20L, 100f);
            Assert.Equal(6, rec.Batches.Count);
            Assert.Equal(new[] { 20L, 14L, 13L, 12L, 11L, 10L }, rec.Batches.ConvertAll(one => one.First).ToArray());
            Assert.Equal(10L, TradeMath.NumberASaleTakes(rec, 100));
            TradeMath.DrainSale(rec, 1, 100);
            Assert.Equal(11L, TradeMath.NumberASaleTakes(rec, 100));
            Assert.Equal(5, rec.Batches.Count);
        }

        [Fact]
        public void Units_with_no_number_or_one_another_unit_has_get_new_numbers_past_every_number_in_use()
        {
            var plain = new PurchaseRecord { ItemId = "wine", Count = 2, TotalPaid = 200 };
            plain.Batches = new List<Batch> { new Batch { Unit = 100, Count = 2 } };
            var kept = new PurchaseRecord { ItemId = "oil", Count = 3, TotalPaid = 600 };
            kept.Batches = new List<Batch> { new Batch { Unit = 200, Count = 3, First = 5L } };
            var twice = new PurchaseRecord { ItemId = "salt", Count = 2, TotalPaid = 600 };
            twice.Batches = new List<Batch> { new Batch { Unit = 300, Count = 2, First = 6L } };
            var old = new PurchaseRecord { ItemId = "fur", Count = 4, TotalPaid = 400 };

            long next = TradeMath.NumberEveryUnit(new List<PurchaseRecord> { plain, kept, twice, old }, 1L, out long numbered);

            Assert.Equal(8L, numbered);
            Assert.Equal(16L, next);
            Assert.Equal(5L, kept.Batches[0].First);
            var used = new HashSet<long>();
            foreach (PurchaseRecord rec in new[] { plain, kept, twice, old })
                foreach (Batch one in rec.Batches)
                    for (long n = one.First; n < one.First + one.Count; n++)
                    {
                        Assert.True(n >= TradeMath.FirstUnitNumber && n < next);
                        Assert.True(used.Add(n));
                    }
            Assert.Equal(11, used.Count);
        }

        [Fact]
        public void A_number_given_out_is_never_given_again_even_after_its_unit_is_sold()
        {
            var rec = new PurchaseRecord { ItemId = "cloth" };
            TradeMath.AddPurchase(rec, TradeMath.AtOnePrice(3, 100), 40L, 100f);
            TradeMath.DrainSale(rec, 3, 100);
            Assert.Equal(0, rec.Count);
            Assert.Equal(43L, TradeMath.NumberEveryUnit(new List<PurchaseRecord> { rec }, 43L, out long none));
            Assert.Equal(0L, none);
            var held = new PurchaseRecord { ItemId = "cloth" };
            TradeMath.AddPurchase(held, TradeMath.AtOnePrice(2, 100), 90L, 100f);
            Assert.Equal(92L, TradeMath.NumberEveryUnit(new List<PurchaseRecord> { held }, 0L, out none));
            Assert.Equal(0L, none);
        }

        [Fact]
        public void A_run_of_numbers_that_would_run_past_the_largest_number_is_numbered_afresh()
        {
            var rec = new PurchaseRecord { ItemId = "cloth", Count = 5, TotalPaid = 500 };
            rec.Batches = new List<Batch> { new Batch { Unit = 100, Count = 5, First = long.MaxValue - 1 } };
            long next = TradeMath.NumberEveryUnit(new List<PurchaseRecord> { rec }, 7L, out long numbered);
            Assert.Equal(5L, numbered);
            Assert.Equal(7L, rec.Batches[0].First);
            Assert.Equal(12L, next);
        }

        private static Batch Unit(long number, int paid, float day) =>
            new Batch { Unit = paid, Count = 1, First = number, Day = day };

        private static Batch[] Sorted(params Batch[] rows)
        {
            var list = new List<Batch>(rows);
            list.Sort(TradeMath.CheapestFirstOldestLast);
            return list.ToArray();
        }

        [Fact]
        public void Every_unit_bought_has_a_row_of_its_own_even_at_one_price()
        {
            var rec = new PurchaseRecord { ItemId = "cloth" };
            TradeMath.AddPurchase(rec, TradeMath.AtOnePrice(3, 100), 7L, 40f);
            Assert.Equal(3, rec.Batches.Count);
            Assert.All(rec.Batches, one => Assert.Equal(1, one.Count));
            Assert.All(rec.Batches, one => Assert.Equal(40f, one.Day));
            Assert.Equal(new[] { 9L, 8L, 7L }, rec.Batches.ConvertAll(one => one.First).ToArray());
        }

        [Fact]
        public void Of_two_units_at_one_price_the_older_one_goes_first()
        {
            var rec = new PurchaseRecord { ItemId = "wine" };
            TradeMath.AddPurchase(rec, TradeMath.AtOnePrice(1, 100), 1L, 10f);
            TradeMath.AddPurchase(rec, TradeMath.AtOnePrice(1, 100), 2L, 20f);
            Assert.Equal(1L, TradeMath.NumberASaleTakes(rec, 100));
            Assert.Equal(10f, TradeMath.DayASaleTakes(rec, 100));
            TradeMath.DrainSale(rec, 1, 100);
            Assert.Equal(2L, TradeMath.NumberASaleTakes(rec, 100));
            var walk = OwnCost(Sorted(Unit(1, 100, 10f), Unit(2, 100, 20f), Unit(3, 50, 30f)));
            Assert.Equal(100, walk.Floor(200));
            Assert.Equal(100, walk.Took());
        }

        [Fact]
        public void A_run_kept_by_an_older_TradeLord_is_split_into_one_row_for_each_unit()
        {
            var rec = new PurchaseRecord { ItemId = "cloth", Count = 5, TotalPaid = 700 };
            rec.Batches = new List<Batch> { new Batch { Unit = 100, Count = 3, First = 5L }, new Batch { Unit = 200, Count = 2 } };
            var all = new List<PurchaseRecord> { rec };
            long next = TradeMath.NumberEveryUnit(all, 1L, out long numbered);
            Assert.Equal(0L, TradeMath.KeepEveryUnitApart(all));
            Assert.Equal(2L, numbered);
            Assert.Equal(10L, next);
            Assert.Equal(5, rec.Batches.Count);
            Assert.All(rec.Batches, one => Assert.Equal(1, one.Count));
            Assert.Equal(new[] { 7L, 6L, 5L, 9L, 8L }, rec.Batches.ConvertAll(one => one.First).ToArray());
            Assert.Equal(5, TradeMath.DateEveryUnit(all, 300f));
            Assert.All(rec.Batches, one => Assert.Equal(300f, one.Day));
            Assert.Equal(0, TradeMath.DateEveryUnit(all, 400f));
        }

        [Fact]
        public void A_record_claiming_more_units_than_any_party_carries_is_cut_down_before_it_is_split()
        {
            var rec = new PurchaseRecord { ItemId = "felt", Count = 2000000000, TotalPaid = 2000000000, LastUnitPaid = 1 };
            rec.Batches = new List<Batch> { new Batch { Unit = 999, Count = 2000000000, First = 1L } };
            long trimmed = TradeMath.KeepEveryUnitApart(new List<PurchaseRecord> { rec });
            Assert.Equal(2000000000L - TradeMath.MostUnitsKeptApart, trimmed);
            Assert.Equal(TradeMath.MostUnitsKeptApart, rec.Count);
            Assert.Equal(TradeMath.MostUnitsKeptApart, rec.Batches.Count);
        }

        [Fact]
        public void A_unit_past_its_age_sells_for_what_it_cost_with_no_margin()
        {
            var walk = TradeMath.DearFirst.EachAtItsOwnCost(
                Sorted(Unit(21, 120, 88f), Unit(22, 100, 69f), Unit(23, 70, 95f)), 0, 0, 0.15f, 100f, 30);
            Assert.Equal(1, walk.AgedLeft);
            Assert.Equal(100, walk.Floor(105));
            Assert.True(walk.PickedHasAged);
            Assert.True(walk.Clears(105, 0.15f));
            Assert.Equal(100, walk.Took());
            Assert.Equal(0, walk.AgedLeft);
            Assert.Equal(70, walk.Floor(99));
            Assert.False(walk.PickedHasAged);
        }

        [Fact]
        public void A_unit_past_its_age_never_sells_under_what_it_cost()
        {
            var walk = TradeMath.DearFirst.EachAtItsOwnCost(Sorted(Unit(1, 100, 10f)), 0, 0, 0.15f, 100f, 30);
            Assert.False(walk.Clears(99, 0.15f));
            Assert.True(walk.Clears(100, 0.15f));
        }

        [Fact]
        public void With_no_age_limit_or_no_day_a_unit_always_needs_its_margin()
        {
            var off = TradeMath.DearFirst.EachAtItsOwnCost(Sorted(Unit(1, 100, 10f)), 0, 0, 0.15f, 100f, 0);
            Assert.Equal(0, off.AgedLeft);
            Assert.False(off.Clears(110, 0.15f));
            var undated = TradeMath.DearFirst.EachAtItsOwnCost(Sorted(Unit(1, 100, 0f)), 0, 0, 0.15f, 100f, 30);
            Assert.Equal(0, undated.AgedLeft);
            Assert.False(undated.Clears(110, 0.15f));
        }

        [Fact]
        public void Holding_all_but_the_aged_sells_only_units_past_their_age()
        {
            var walk = TradeMath.DearFirst.EachAtItsOwnCost(
                Sorted(Unit(1, 50, 60f), Unit(2, 60, 99f)), 0, 0, 0.15f, 100f, 30);
            Assert.True(walk.KeepOnlyTheAged());
            Assert.True(walk.HoldingAllButTheAged);
            Assert.Equal(50, walk.Floor(80));
            Assert.Equal(50, walk.Took());
            Assert.Equal(TradeMath.NoUnitThisPriceSells, walk.Floor(80));
            Assert.False(walk.KeepOnlyTheAged());
        }

        [Fact]
        public void Without_the_aged_a_walk_counts_only_units_still_inside_their_age()
        {
            var walk = TradeMath.DearFirst.EachAtItsOwnCost(
                Sorted(Unit(1, 10, 50f), Unit(2, 20, 99f), Unit(3, 30, 99f)), 15, 2, 0.15f, 100f, 30);
            TradeMath.DearFirst fresh = walk.WithoutTheAged();
            Assert.Equal(20, fresh.LeastItAsks());
            Assert.Equal(10, walk.LeastItAsks());
            Assert.Equal(new[] { 32 }, TradeRules.WhatTheWalkTakes(new[] { 32, 28, 24 }, fresh, 0.15f));
            int till = 1000;
            Assert.Equal(new[] { 40, 35 }, TradeRules.WhatSellsHere(u => 40 - 5 * u, 3, fresh, 0.15f, ref till));
            Assert.Equal(925, till);
        }

        [Fact]
        public void A_walk_without_the_unknown_units_sells_only_the_units_with_a_price_paid()
        {
            var walk = new TradeMath.DearFirst(Costs(100, 100), 115, 100, 3);
            TradeMath.DearFirst known = walk.WithoutTheUnknown();
            Assert.Equal(3, walk.Unknown);
            Assert.Equal(0, known.Unknown);
            Assert.Equal(new[] { 100, 100, 100, 100, 100 }, Walked(walk, 5, u => 116).ToArray());
            Assert.Equal(new[] { 100, 100 }, Walked(known, 5, u => 116).ToArray());
        }

        [Fact]
        public void Each_at_its_own_cost_a_walk_without_the_unknown_units_never_sells_one_at_the_worth()
        {
            var walk = TradeMath.DearFirst.EachAtItsOwnCost(Costs(300), 200, 2, 0.15f);
            TradeMath.DearFirst known = walk.WithoutTheUnknown();
            Assert.Equal(2, walk.Unknown);
            Assert.Equal(new[] { 200, 200 }, Walked(walk, 3, u => 250).ToArray());
            Assert.Empty(Walked(known, 3, u => 250));
            Assert.Equal(new[] { 300 }, Walked(known, 3, u => 400).ToArray());
        }

        [Fact]
        public void Dear_units_a_dry_run_already_drew_are_left_out_one_for_one()
        {
            Batch[] dearer = Costs(500, 836, 836);
            Assert.Equal(2, TradeMath.LeaveOut(ref dearer, new List<int> { 836, 500, 999 }));
            Assert.Equal(new[] { 836 }, Units(dearer));
            Assert.Equal(1, TradeMath.LeaveOut(ref dearer, new List<int> { 836 }));
            Assert.Null(dearer);
            Assert.Equal(0, TradeMath.LeaveOut(ref dearer, new List<int> { 836 }));
        }

        [Fact]
        public void A_resale_safety_setting_outside_its_range_is_held_inside_it()
        {
            Assert.Equal(1f, TradeMath.ResaleSafetyAsPromisesHeld(1.5f, true, 0, TradeMath.NoShareToGive));
            Assert.Equal(0.5f, TradeMath.ResaleSafetyAsPromisesHeld(0.2f, true, 0, TradeMath.NoShareToGive));
            Assert.Equal(0.5f, TradeMath.ResaleSafetyAsPromisesHeld(float.NaN, false, 0, TradeMath.NoShareToGive));
        }

        [Fact]
        public void A_dear_unit_sells_once_the_price_beats_the_average_by_your_margin()
        {
            var at = Walked(new TradeMath.DearFirst(Costs(836), 0, 390), 1, u => 836);
            Assert.Equal(new[] { 836 }, at.ToArray());
            var under = Walked(new TradeMath.DearFirst(Costs(836), 0, 390), 1, u => 449);
            Assert.Equal(new[] { 836 }, under.ToArray());
            var belowTheMargin = Walked(new TradeMath.DearFirst(Costs(836), 0, 390), 1, u => 448);
            Assert.Empty(belowTheMargin);
        }

        [Fact]
        public void A_hand_sale_takes_for_each_price_the_dearest_unit_it_covers_and_works_down_from_there()
        {
            var rec = new PurchaseRecord { ItemId = "felt" };
            Buy(rec, 1, 500);
            Buy(rec, 1, 900);
            Buy(rec, 8, 800);
            int covers = TradeMath.WhatTheAverageCovers(TradeMath.UnitBasis(rec, AveragePaid), 0.15f);
            Assert.Equal(253, covers);
            Assert.Equal(new[] { 900, 500 }, CostsOf(TradeMath.WhatAHandSaleTook(rec, new[] { 950, 600 }, covers, null)));
            Assert.Equal(new[] { 500, 100 }, CostsOf(TradeMath.WhatAHandSaleTook(rec, new[] { 600, 400 }, covers, null)));
            Assert.Equal(new[] { 100, 100 }, CostsOf(TradeMath.WhatAHandSaleTook(rec, new[] { 200, 200 }, covers, null)));
        }

        [Fact]
        public void A_hand_sale_with_each_unit_at_its_own_cost_takes_for_each_price_the_dearest_unit_it_covers()
        {
            var rec = new PurchaseRecord { ItemId = "cloth" };
            Buy(rec, 1, 100);
            Buy(rec, 1, 200);
            Buy(rec, 1, 300);
            Assert.Equal(new[] { 200 }, CostsOf(TradeMath.WhatAHandSaleTook(rec, new[] { 250 }, TradeMath.EachUnitApart, null)));
            Assert.Equal(new[] { 300, 200 }, CostsOf(TradeMath.WhatAHandSaleTook(rec, new[] { 300, 200 }, TradeMath.EachUnitApart, null)));
            Assert.Equal(250 - 200, TradeMath.MadeOnAHandSale(rec, new[] { 250 }, TradeMath.EachUnitApart, null));
        }

        [Fact]
        public void A_hand_sale_with_each_unit_at_its_own_cost_is_not_put_off_by_a_dear_unit_left_unsold()
        {
            var rec = new PurchaseRecord { ItemId = "cloth" };
            foreach (int paid in new[] { 50, 120, 130, 1000 }) Buy(rec, 1, paid);
            Assert.Equal(new[] { 130, 120 }, CostsOf(TradeMath.WhatAHandSaleTook(rec, new[] { 150, 150 }, TradeMath.EachUnitApart, null)));
            Assert.Equal(300 - 130 - 120, TradeMath.MadeOnAHandSale(rec, new[] { 150, 150 }, TradeMath.EachUnitApart, null));
            TradeMath.DrainHandSale(rec, new[] { 150, 150 }, TradeMath.EachUnitApart, null);
            Assert.Equal(new[] { 50, 1000 }, Units(TradeMath.UnitCosts(rec, rec.Count)));
        }

        [Fact]
        public void Units_laid_out_for_a_hand_sale_are_taken_at_their_own_cost()
        {
            var rec = new PurchaseRecord { ItemId = "cloth" };
            foreach (int paid in new[] { 10, 290, 300, 3000 }) Buy(rec, 1, paid);
            var laidOut = new List<int> { 300, 290 };
            Assert.Equal(new[] { 300, 290 }, CostsOf(TradeMath.WhatAHandSaleTook(rec, new[] { 350, 345 }, TradeMath.EachUnitApart, laidOut)));
            Assert.Equal(695 - 300 - 290, TradeMath.MadeOnAHandSale(rec, new[] { 350, 345 }, TradeMath.EachUnitApart, laidOut));
        }

        [Fact]
        public void Units_bought_together_on_the_trade_screen_keep_the_price_each_one_cost()
        {
            var rec = new PurchaseRecord { ItemId = "wine" };
            TradeMath.AddPurchase(rec, new[] { 33, 34, 35 }, 40L, 100f);
            Assert.Equal(new[] { 33, 34, 35 }, Units(TradeMath.UnitCosts(rec, rec.Count)));
            Assert.Equal(new[] { 40L, 41L, 42L }, NumbersOf(rec));
            Assert.Equal(102, rec.TotalPaid);
            Assert.Equal(35, rec.LastUnitPaid);
            Assert.Equal(102, TradeMath.WorthOf(new[] { 33, 34, 35 }));
        }

        [Fact]
        public void A_hand_sale_names_each_unit_it_took_with_the_price_that_unit_fetched()
        {
            var rec = new PurchaseRecord { ItemId = "wine" };
            TradeMath.AddPurchase(rec, new[] { 33, 34, 35 }, 40L, 100f);
            List<TradeMath.SoldUnit> sold = TradeMath.DrainHandSale(rec, new[] { 36, 35, 34, 33 }, TradeMath.EachUnitApart, null);
            Assert.Equal(new[] { 35, 34, 33 }, CostsOf(sold));
            Assert.Equal(new[] { 42L, 41L, 40L }, new[] { sold[0].Number, sold[1].Number, sold[2].Number });
            Assert.Equal(new[] { 36, 35, 34, 33 }, new[] { sold[0].Price, sold[1].Price, sold[2].Price, sold[3].Price });
            Assert.False(sold[3].Bought);
            Assert.Equal(0, rec.Count);
            Assert.Equal(0, rec.TotalPaid);
        }

        [Fact]
        public void Units_laid_out_by_Staged_Trading_are_taken_first_and_units_added_by_hand_after_them()
        {
            var rec = new PurchaseRecord { ItemId = "wine" };
            TradeMath.AddPurchase(rec, new[] { 100, 300, 500 }, 1L, 100f);
            var laidOut = new List<int> { 300 };
            List<TradeMath.SoldUnit> sold = TradeMath.WhatAHandSaleTook(rec, new[] { 600, 590 }, TradeMath.EachUnitApart, laidOut);
            Assert.Equal(new[] { 300, 500 }, CostsOf(sold));
            Assert.Equal((600 - 300) + (590 - 500), TradeMath.MadeOnAHandSale(rec, new[] { 600, 590 }, TradeMath.EachUnitApart, laidOut));
            Assert.Equal(3, rec.Count);
        }

        [Fact]
        public void Of_two_units_bought_at_one_price_the_older_one_leaves_first_when_neither_is_named()
        {
            var rec = new PurchaseRecord { ItemId = "wine" };
            TradeMath.AddPurchase(rec, new[] { 50 }, 7L, 90f);
            TradeMath.AddPurchase(rec, new[] { 50 }, 9L, 99f);
            TradeMath.DrainSale(rec, 1);
            Assert.Equal(new[] { 9L }, NumbersOf(rec));
        }

        [Fact]
        public void A_saved_record_carrying_an_old_average_is_added_up_again_from_its_units()
        {
            var rec = new PurchaseRecord { ItemId = "wine" };
            TradeMath.AddPurchase(rec, new[] { 100, 300 }, 1L, 100f);
            rec.TotalPaid = 600;
            var kept = new PurchaseRecord { ItemId = "salt" };
            TradeMath.AddPurchase(kept, new[] { 50 }, 3L, 100f);
            Assert.Equal(1, TradeMath.AddUpEveryRecord(new List<PurchaseRecord> { rec, kept, null }));
            Assert.Equal(400, rec.TotalPaid);
            Assert.Equal(200, TradeMath.UnitBasis(rec, AveragePaid));
            Assert.Equal(50, kept.TotalPaid);
        }

        [Fact]
        public void A_unit_bought_to_trade_keeps_that_mark_when_its_units_are_kept_apart()
        {
            var rec = new PurchaseRecord { ItemId = "cheese" };
            TradeMath.AddPurchase(rec, new[] { 68 }, 1L, 100f, TradeMath.FromAMarket, traded: true);
            TradeMath.AddPurchase(rec, new[] { 40 }, 2L, 100f);
            Assert.Equal(new[] { false, true }, rec.Batches.ConvertAll(one => one.Traded).ToArray());
            rec.Batches = new List<Batch> { new Batch { Unit = 68, Count = 2, First = 5L, Day = 9f, Traded = true } };
            rec.Count = 2;
            rec.TotalPaid = 136;
            TradeMath.KeepEveryUnitApart(new List<PurchaseRecord> { rec });
            Assert.Equal(2, rec.Batches.Count);
            Assert.True(rec.Batches.TrueForAll(one => one.Traded));
        }

        [Fact]
        public void Every_unit_keeps_where_it_came_from_and_only_market_and_caravan_units_count_for_Trade_XP()
        {
            var rec = new PurchaseRecord { ItemId = "wine" };
            TradeMath.AddPurchase(rec, new[] { 30 }, 1L, 100f, TradeMath.FromVillagers);
            TradeMath.AddPurchase(rec, new[] { 40 }, 2L, 100f, TradeMath.FromACaravan);
            TradeMath.AddPurchase(rec, new[] { 50 }, 3L, 100f);
            Assert.Equal(new[] { TradeMath.FromVillagers, TradeMath.FromACaravan, TradeMath.FromAMarket },
                         rec.Batches.ConvertAll(one => one.From).ToArray());
            Assert.Equal(TradeMath.FromVillagers, TradeMath.FromASaleTakes(rec, 30));
            Assert.False(TradeMath.GivesTradeXp(TradeMath.FromVillagers));
            Assert.False(TradeMath.GivesTradeXp(TradeMath.FromLoot));
            Assert.False(TradeMath.GivesTradeXp(TradeMath.FromElsewhere));
            Assert.True(TradeMath.GivesTradeXp(TradeMath.FromACaravan));
            Assert.True(TradeMath.GivesTradeXp(TradeMath.FromAMarket));
            Assert.Equal("source 3 (villager party), bought", TradeMath.WhereFrom(TradeMath.FromVillagers));
            Assert.Equal("source 4 (loot), came without a purchase", TradeMath.WhereFrom(TradeMath.FromLoot));
            List<TradeMath.SoldUnit> sold = TradeMath.DrainHandSale(rec, new[] { 45 }, TradeMath.EachUnitApart, null);
            Assert.Equal(TradeMath.FromACaravan, sold[0].From);
        }

        [Fact]
        public void Where_a_unit_came_from_is_numbered_1_market_2_caravan_3_villager_party_4_loot_5_others()
        {
            int[] sources = { TradeMath.FromAMarket, TradeMath.FromACaravan, TradeMath.FromVillagers,
                              TradeMath.FromLoot, TradeMath.FromElsewhere };
            Assert.Equal(new[] { 1, 2, 3, 4, 5 }, Array.ConvertAll(sources, TradeMath.SourceNumber));
            Assert.Equal(new[] { "market", "caravan", "villager party", "loot", "others" },
                         Array.ConvertAll(sources, TradeMath.SourceNamed));
            Assert.Equal(new[] { false, false, false, true, true }, Array.ConvertAll(sources, TradeMath.CameFree));
            Assert.All(sources, one => Assert.True(TradeMath.IsASource(one)));
            Assert.False(TradeMath.IsASource(5));
            Assert.False(TradeMath.IsASource(-1));
            Assert.Equal("source 1 (market), bought", TradeMath.WhereFrom(TradeMath.FromAMarket));
            Assert.Equal("source 2 (caravan), bought", TradeMath.WhereFrom(TradeMath.FromACaravan));
            Assert.Equal("source 5 (others), came without a purchase", TradeMath.WhereFrom(TradeMath.FromElsewhere));
        }

        [Fact]
        public void What_left_the_party_unsold_comes_off_the_oldest_units_bought_or_not()
        {
            var bought = new PurchaseRecord { ItemId = "grain" };
            var free = new PurchaseRecord { ItemId = "grain" };
            TradeMath.AddPurchase(bought, new[] { 20, 21 }, 10L, 50f);
            TradeMath.AddPurchase(free, new int[3], 20L, 40f, TradeMath.FromLoot);
            TradeMath.AddPurchase(bought, new[] { 22 }, 30L, 60f);
            Assert.Equal(0, TradeMath.DrainTheOldestOf(bought, free, 2));
            Assert.Equal(1, free.Count);
            Assert.Equal(3, bought.Count);
            Assert.Equal(2, TradeMath.DrainTheOldestOf(bought, free, 3));
            Assert.Equal(0, free.Count);
            Assert.Equal(new[] { 22 }, Units(TradeMath.UnitCosts(bought, bought.Count)));
            Assert.Equal(0, free.TotalPaid);
        }

        [Fact]
        public void A_unit_that_came_without_a_purchase_leaves_oldest_first_with_its_number()
        {
            var free = new PurchaseRecord { ItemId = "iron" };
            TradeMath.AddPurchase(free, new int[2], 7L, 90f, TradeMath.FromElsewhere);
            TradeMath.AddPurchase(free, new int[1], 3L, 80f, TradeMath.FromLoot);
            Assert.True(TradeMath.DrainTheOldestUnit(free, out long number, out float day, out int from));
            Assert.Equal(3L, number);
            Assert.Equal(80f, day);
            Assert.Equal(TradeMath.FromLoot, from);
            Assert.Equal(2, free.Count);
            Assert.Equal(0, free.TotalPaid);
            Assert.False(TradeMath.DrainTheOldestUnit(new PurchaseRecord { ItemId = "none" }, out _, out _, out _));
        }

        [Fact]
        public void A_spread_used_only_when_the_screen_cannot_be_read_still_adds_up_to_the_gold()
        {
            Assert.Equal(new[] { 279, 279, 278, 278 }, TradeMath.SpreadOver(4, 1114));
            Assert.Equal(1114, TradeMath.WorthOf(TradeMath.SpreadOver(4, 1114)));
            Assert.Empty(TradeMath.SpreadOver(0, 1114));
        }

        [Fact]
        public void A_deal_shrunk_on_the_trade_screen_takes_only_as_many_dear_units_as_really_moved()
        {
            var rec = new PurchaseRecord { ItemId = "felt" };
            Buy(rec, 1, 500);
            Buy(rec, 1, 900);
            Buy(rec, 8, 800);
            int covers = TradeMath.WhatTheAverageCovers(TradeMath.UnitBasis(rec, AveragePaid), 0.15f);
            var laidOut = new List<int> { 900, 500 };
            Assert.Equal(new[] { 900 }, CostsOf(TradeMath.WhatAHandSaleTook(rec, new[] { 2500 }, covers, laidOut)));
            Assert.Equal(new[] { 900, 500 }, CostsOf(TradeMath.WhatAHandSaleTook(rec, new[] { 2500, 2500 }, covers, laidOut)));
        }

        [Fact]
        public void A_record_claiming_far_more_than_is_held_lists_no_more_dear_units_than_are_held()
        {
            var rec = new PurchaseRecord { ItemId = "felt", Count = 2000000000, TotalPaid = 2000000000, LastUnitPaid = 1 };
            rec.Batches = new List<Batch> { new Batch { Unit = 999, Count = 2000000000 } };
            Batch[] costs = TradeMath.UnitCosts(rec, 5);
            Assert.Equal(5, TradeMath.UnitsIn(costs));
            Assert.All(costs, one => Assert.Equal(999, one.Unit));
            Assert.Null(TradeMath.UnitCosts(rec, 0));
        }

        [Fact]
        public void A_record_claiming_more_than_is_held_drops_the_oldest_units_as_the_daily_check_would()
        {
            var rec = new PurchaseRecord { ItemId = "felt" };
            Buy(rec, 3, 2700);
            Buy(rec, 7, 4200);
            Assert.Equal(new[] { 600, 600, 600, 600, 600 }, Units(TradeMath.UnitCosts(rec, 5)));
            TradeMath.DrainWhatLeftUnsold(rec, 5);
            Assert.Equal(new[] { 600, 600, 600, 600, 600 }, Units(TradeMath.UnitCosts(rec, 5)));
            Assert.Equal(5 * 600, rec.TotalPaid);
        }

        [Fact]
        public void A_unit_bought_dear_keeps_its_own_cost_after_some_of_the_good_is_eaten()
        {
            var rec = new PurchaseRecord { ItemId = "jewelry" };
            Buy(rec, 10, 1000);
            Buy(rec, 1, 836);
            TradeMath.DrainWhatLeftUnsold(rec, 1);
            Assert.Equal(10, rec.Count);
            int covers = TradeMath.WhatTheAverageCovers(TradeMath.UnitBasis(rec, AveragePaid), 0.15f);
            Assert.Equal(new[] { 836 }, Dear(rec, covers, rec.Count));
        }

        [Fact]
        public void Goods_that_left_unsold_come_off_the_oldest_units_first()
        {
            var rec = new PurchaseRecord { ItemId = "felt" };
            Buy(rec, 4, 1112);
            Buy(rec, 1, 836);
            TradeMath.DrainWhatLeftUnsold(rec, 4);
            Assert.Equal(new[] { 836 }, Units(TradeMath.UnitCosts(rec, rec.Count)));
            var even = new PurchaseRecord { ItemId = "felt" };
            Buy(even, 1, 100);
            Buy(even, 1, 900);
            TradeMath.DrainWhatLeftUnsold(even, 1);
            Assert.Equal(new[] { 900 }, Units(TradeMath.UnitCosts(even, even.Count)));
            var dated = new PurchaseRecord { ItemId = "felt" };
            TradeMath.AddPurchase(dated, new[] { 500 }, 50L, 90f);
            TradeMath.AddPurchase(dated, new[] { 100 }, 10L, 95f);
            TradeMath.DrainWhatLeftUnsold(dated, 1);
            Assert.Equal(new[] { 100 }, Units(TradeMath.UnitCosts(dated, dated.Count)));
        }

        [Fact]
        public void Goods_that_left_without_a_sale_drain_a_huge_record_without_listing_every_unit()
        {
            var rec = new PurchaseRecord { ItemId = "felt", Count = 2000000000, TotalPaid = 2000000000, LastUnitPaid = 1 };
            rec.Batches = new List<Batch> { new Batch { Unit = 999, Count = 2000000000 } };
            TradeMath.DrainWhatLeftUnsold(rec, 1999999995);
            Assert.Equal(5, rec.Count);
            Assert.True(TradeMath.BatchesAddUp(rec));
            Assert.Equal(5 * 999, rec.TotalPaid);
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
        public void A_unit_you_bought_makes_what_it_fetched_over_what_that_unit_cost()
        {
            Assert.Equal(40, TradeMath.MadeOnAUnit(price: 100, bought: true, unitCost: 60));
            Assert.Equal(0, TradeMath.MadeOnAUnit(price: 60, bought: true, unitCost: 60));
        }

        [Fact]
        public void A_sale_below_what_the_unit_cost_is_reported_as_the_loss_it_is()
        {
            Assert.Equal(-25, TradeMath.MadeOnAUnit(price: 75, bought: true, unitCost: 100));
        }

        [Fact]
        public void A_unit_you_never_bought_makes_no_profit_whatever_it_fetched()
        {
            Assert.Equal(0, TradeMath.MadeOnAUnit(price: 80, bought: false, unitCost: 0));
            Assert.Equal(0, TradeMath.MadeOnAUnit(price: 5000, bought: false, unitCost: 50));
        }

        [Fact]
        public void A_negative_cost_is_never_a_bonus_on_top_of_the_price()
        {
            Assert.Equal(10, TradeMath.MadeOnAUnit(price: 10, bought: true, unitCost: -100));
        }

        [Fact]
        public void A_hand_sale_makes_what_each_unit_fetched_over_what_that_unit_cost()
        {
            var rec = new PurchaseRecord { ItemId = "felt" };
            Buy(rec, 1, 836);
            Buy(rec, 4, 1112);
            int covers = TradeMath.WhatTheAverageCovers(TradeMath.UnitBasis(rec, AveragePaid), 0.15f);
            Assert.Equal(1135 - 836, TradeMath.MadeOnAHandSale(rec, new[] { 1135 }, covers, null));
            Assert.Equal(484 - 278, TradeMath.MadeOnAHandSale(rec, new[] { 484 }, covers, null));
            Assert.Equal(5 * 1000 - 836 - 4 * 278, TradeMath.MadeOnAHandSale(rec, TradeMath.AtOnePrice(5, 1000), covers, null));
            Assert.Equal(5, rec.Count);
        }

        [Fact]
        public void A_hand_sale_counts_nothing_for_units_you_never_bought()
        {
            var rec = new PurchaseRecord { ItemId = "felt" };
            Buy(rec, 2, 600);
            Assert.Equal(2 * 400 - 600, TradeMath.MadeOnAHandSale(rec, TradeMath.AtOnePrice(4, 400), 345, null));
            Assert.Equal(0, TradeMath.MadeOnAHandSale(null, TradeMath.AtOnePrice(4, 400), 345, null));
            Assert.Equal(0, TradeMath.MadeOnAHandSale(rec, new int[0], 345, null));
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
        public void The_halving_search_reaches_the_largest_whole_number_there_is()
        {
            Assert.Equal(int.MaxValue, TradeMath.MostThatHolds(int.MaxValue, step => step > 0));
            Assert.Equal(1000, TradeMath.MostThatHolds(int.MaxValue, step => step > 0 && step <= 1000));
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
            Assert.Equal(2.4f, TradeMath.DaysAtSpeed(240f, 1f, 5f, 10f), 4);
        }

        [Fact]
        public void A_journey_all_at_sea_is_its_distance_at_the_sea_speed()
        {
            Assert.Equal(1.2f, TradeMath.DaysAtSpeed(240f, 0f, 5f, 10f), 4);
        }

        [Fact]
        public void A_journey_with_a_sea_leg_counts_each_leg_at_its_own_speed()
        {
            Assert.Equal(1.8f, TradeMath.DaysAtSpeed(240f, 0.5f, 5f, 10f), 4);
        }

        [Fact]
        public void A_party_rides_twenty_times_its_speed_in_a_game_day()
        {
            Assert.Equal(1.2f, TradeMath.HoursAPartyTakesToCoverItsSpeed);
            Assert.Equal(1f, TradeMath.DaysAtSpeed(100f, 1f, 5f, 5f), 4);
            Assert.Equal(1f, TradeMath.DaysAtBestSpeed(100f, 5f, 5f), 4);
            Assert.Equal(1f, TradeMath.EtaDays(100f, 5f), 4);
        }

        private const int Dawn = 2, Dusk = 22;

        private sealed class Sky : TradeMath.ISky
        {
            private readonly Func<int, double, (bool snowy, bool wet, double until)> _read;

            internal Sky(Func<int, double, (bool snowy, bool wet, double until)> read)
            {
                _read = read;
            }

            public void Read(int cell, double clockHours, out bool snowy, out bool wet, out double hoursToChange)
            {
                var said = _read(cell, clockHours);
                snowy = said.snowy;
                wet = said.wet;
                hoursToChange = said.until;
            }
        }

        private static float Walked(float start, float land, float sea, float pace, bool seaFirst, out float night)
        {
            night = 0f;
            double clock = seaFirst ? sea : 0d, left = land, step = 0.0005d;
            while (left > 1e-9)
            {
                double hour = ((start + clock) % 24d + 24d) % 24d;
                bool day = hour >= Dawn && hour < Dusk;
                double rate = day ? 1d : pace;
                double spent = Math.Min(step, left / rate);
                clock += spent;
                left -= spent * rate;
                if (!day) night += (float)spent;
            }
            return (float)(clock + (seaFirst ? 0d : sea));
        }

        [Fact]
        public void A_ride_check_weighs_the_time_taken_against_the_time_said()
        {
            Assert.Equal(0.1f, TradeMath.RideOffBy(10f, 11f), 4);
            Assert.Equal(-0.2f, TradeMath.RideOffBy(10f, 8f), 4);
            Assert.True(float.IsNaN(TradeMath.RideOffBy(0f, 8f)));
            var sorted = new List<float> { -0.1f, 0f, 0.1f, 0.3f };
            Assert.Equal(0.05f, TradeMath.Quantile(sorted, 0.5f), 4);
            Assert.Equal(-0.025f, TradeMath.Quantile(sorted, 0.25f), 4);
            Assert.Equal(0.3f, TradeMath.Quantile(sorted, 1f), 4);
            Assert.True(float.IsNaN(TradeMath.Quantile(new List<float>(), 0.5f)));
        }

        [Fact]
        public void A_buyer_is_ranked_on_what_it_pays_for_every_unit_its_purse_can_take()
        {
            Func<int, int> falling = u => 50 - u * 5;
            Assert.Equal(50L + 45L + 40L, TradeMath.WhatALadderPays(falling, 3, int.MaxValue));
            Assert.Equal(50L + 45L, TradeMath.WhatALadderPays(falling, 3, 100));
            Assert.Equal(0L, TradeMath.WhatALadderPays(falling, 3, 49));
            Assert.Equal(50L + 45L + 40L + 35L + 30L + 25L + 20L + 15L + 10L + 5L,
                         TradeMath.WhatALadderPays(falling, 20, int.MaxValue));
            Assert.Equal(0L, TradeMath.WhatALadderPays(null, 3, int.MaxValue));
            Assert.Equal(0L, TradeMath.WhatALadderPays(falling, 0, int.MaxValue));
        }

        [Fact]
        public void Every_log_line_reads_the_campaign_day_and_the_clock_and_says_when_it_is_night()
        {
            Assert.Equal("day 14 20:24".PadRight(TradeMath.GameClockWidth), TradeMath.GameClock(14, 20.4f, false));
            Assert.Equal("day 3 23:59 night".PadRight(TradeMath.GameClockWidth), TradeMath.GameClock(3, 23.9999f, true));
            Assert.Equal("day 0 00:00 night".PadRight(TradeMath.GameClockWidth), TradeMath.GameClock(0, 0f, true));
            Assert.Equal("day 7 00:00".PadRight(TradeMath.GameClockWidth), TradeMath.GameClock(7, float.NaN, false));
            Assert.Equal("day 7 00:00".PadRight(TradeMath.GameClockWidth), TradeMath.GameClock(7, -3f, false));
            Assert.Equal(TradeMath.GameClockWidth, TradeMath.NoGameClock.Length);
            Assert.Equal(TradeMath.GameClockWidth, TradeMath.GameClock(1234, 12.5f, true).Length);
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
            Assert.Equal(0, TradeMath.ShelfAfterLanding(0, -400));
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
        public void The_next_daily_tick_is_read_off_the_last_run_and_falls_back_to_half_a_day()
        {
            Assert.Equal(0.7f, TradeMath.NextDailyTickIn(0.3d), 4);
            Assert.Equal(0.7f, TradeMath.NextDailyTickIn(1.3d), 4);
            Assert.Equal(1f, TradeMath.NextDailyTickIn(0d), 4);
            Assert.Equal(1f, TradeMath.NextDailyTickIn(1d), 4);
            Assert.Equal(TradeMath.TickTimeUnknown, TradeMath.NextDailyTickIn(2.5d), 4);
            Assert.Equal(TradeMath.TickTimeUnknown, TradeMath.NextDailyTickIn(-0.1d), 4);
            Assert.Equal(TradeMath.TickTimeUnknown, TradeMath.NextDailyTickIn(double.NaN), 4);
        }

        [Fact]
        public void A_town_uses_up_as_many_goods_a_day_as_its_budget_for_the_kind_buys_at_its_own_price()
        {
            Assert.Equal(400, TradeMath.WorthUsedUpADay(500f, 50, 40));
            Assert.Equal(0, TradeMath.WorthUsedUpADay(0f, 50, 40));
            Assert.Equal(0, TradeMath.WorthUsedUpADay(float.NaN, 50, 40));
            Assert.Equal(0, TradeMath.WorthUsedUpADay(500f, 0, 40));
            Assert.Equal(0, TradeMath.WorthUsedUpADay(500f, 50, 0));
            Assert.Equal(int.MaxValue, TradeMath.WorthUsedUpADay(float.MaxValue, 1, 1000));
        }

        [Fact]
        public void Goods_left_behind_for_want_of_room_count_only_as_far_as_the_gold_left_would_buy_them()
        {
            Assert.Equal(90f, TradeMath.WeightTheBudgetCanStillBuy(90f, 600, 600), 3);
            Assert.Equal(90f, TradeMath.WeightTheBudgetCanStillBuy(90f, 600, 5000), 3);
            Assert.Equal(30f, TradeMath.WeightTheBudgetCanStillBuy(90f, 600, 200), 3);
            Assert.Equal(0f, TradeMath.WeightTheBudgetCanStillBuy(90f, 600, 0), 3);
            Assert.Equal(0f, TradeMath.WeightTheBudgetCanStillBuy(0f, 600, 600), 3);
            Assert.Equal(0f, TradeMath.WeightTheBudgetCanStillBuy(90f, 0, 600), 3);
            Assert.Equal(0f, TradeMath.WeightTheBudgetCanStillBuy(float.NaN, 600, 600), 3);
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
            Assert.Equal(1.2f, TradeMath.EtaDays(120f, 5f), 4);
            Assert.Equal(TradeMath.EtaDays(120f, TradeMath.WalkingPace),
                         TradeMath.EtaDays(120f, 0f), 4);
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
        public void A_shelf_cannot_be_bought_down_past_half_of_what_is_on_it()
        {
            Assert.Equal(250, TradeMath.ShelfAfterLanding(500, TradeMath.WorthShift(0, 900)));
            Assert.Equal(250, TradeMath.ShelfAfterLanding(500, TradeMath.WorthShift(0, 400)));
            Assert.Equal(300, TradeMath.ShelfAfterLanding(500, TradeMath.WorthShift(0, 200)));
            Assert.Equal(0, TradeMath.ShelfAfterLanding(0, TradeMath.WorthShift(0, 900)));
        }

        [Fact]
        public void A_shelf_the_forecast_adds_to_is_left_where_the_landing_puts_it()
        {
            Assert.Equal(900, TradeMath.ShelfAfterLanding(500, TradeMath.WorthShift(400, 0)));
            Assert.Equal(500, TradeMath.ShelfAfterLanding(500, TradeMath.WorthShift(0, 0)));
            Assert.Equal(int.MaxValue, TradeMath.ShelfAfterLanding(int.MaxValue, int.MaxValue));
        }

        [Fact]
        public void A_move_bigger_than_the_forecast_said_counts_as_all_of_it_and_no_more()
        {
            Assert.True(TradeMath.HowMuchCameTrue(100, 100, out float right));
            Assert.Equal(1f, right);
            Assert.True(TradeMath.HowMuchCameTrue(100, 90, out float under));
            Assert.True(TradeMath.HowMuchCameTrue(100, 110, out float overBy));
            Assert.Equal(0.9f, under, 4);
            Assert.Equal(TradeMath.MostOfAMoveThatCounts, overBy);
            Assert.True(TradeMath.HowMuchCameTrue(-100, -150, out float bigger));
            Assert.Equal(TradeMath.MostOfAMoveThatCounts, bigger);
            Assert.True(TradeMath.HowMuchCameTrue(100, 250, out float wild));
            Assert.Equal(TradeMath.MostOfAMoveThatCounts, wild);
        }

        [Fact]
        public void One_wild_miss_cannot_speak_for_the_whole_forecast()
        {
            Assert.True(TradeMath.HowMuchCameTrue(-76, -2000, out float over));
            Assert.Equal(TradeMath.MostOfAMoveThatCounts, over);
            Assert.True(TradeMath.HowMuchCameTrue(100, -5000, out float against));
            Assert.Equal(TradeMath.LeastOfAMoveThatCounts, against);
            Assert.True(TradeMath.HowMuchCameTrue(-400, -200, out float half));
            Assert.Equal(0.5f, half, 4);
            Assert.True(TradeMath.HowMuchCameTrue(300, 0, out float none));
            Assert.Equal(0f, none);
            Assert.False(TradeMath.HowMuchCameTrue(0, 700, out _));
        }

        [Fact]
        public void What_came_true_counts_each_figure_by_the_size_of_what_it_said()
        {
            TradeMath.HowMuchCameTrue(1000, 500, out float big);
            TradeMath.HowMuchCameTrue(10, -2000, out float small);
            float share = TradeMath.ShareThatCameTrue(1010f, 1000f * big + 10f * small);
            Assert.Equal(490f / 1010f, share, 4);
            Assert.Equal(0f, TradeMath.ShareThatCameTrue(1010f, -300f));
            Assert.Equal(TradeMath.NoShareToGive, TradeMath.ShareThatCameTrue(0f, 0f));
            Assert.Equal(TradeMath.NoShareToGive, TradeMath.ShareThatCameTrue(1010f, float.NaN));
        }

        private static float BySize(params (int said, int moved)[] figures)
        {
            long weighed = 0L, matched = 0L;
            foreach (var (said, moved) in figures)
            {
                if (!TradeMath.HowMuchCameTrue(said, moved, out float share)) continue;
                weighed = TradeMath.AddedUp(weighed, TradeMath.SizeOf(said));
                matched = TradeMath.AddedUp(matched, TradeMath.SizedShareThatCameTrue(said, share));
            }
            return TradeMath.ShareBySize(weighed, matched);
        }

        [Fact]
        public void By_size_a_forecast_that_came_true_every_time_is_trusted_in_full()
        {
            Assert.Equal(1f, BySize((100, 100), (400, 400), (-50, -50)), 4);
        }

        [Fact]
        public void By_size_one_big_miss_counts_for_its_size_and_no_more()
        {
            float share = BySize((1000, 0), (100, 100), (100, 100), (100, 100));
            Assert.Equal(300f / 1300f, share, 4);
            Assert.True(share > 0.2f);
        }

        [Fact]
        public void By_size_no_figure_counts_for_more_than_it_said_either_way()
        {
            Assert.Equal(1f, BySize((100, 5000)), 4);
            Assert.Equal(1f, BySize((100, 110)), 4);
            Assert.Equal(0.5f, BySize((100, 100), (100, -5000), (100, 100), (100, 100)), 4);
            Assert.Equal(0f, BySize((100, -5000)));
        }

        [Fact]
        public void By_size_nothing_checked_says_nothing_and_a_huge_record_never_overflows()
        {
            Assert.Equal(TradeMath.NoShareToGive, BySize());
            Assert.Equal(TradeMath.NoShareToGive, BySize((0, 700)));
            Assert.Equal(long.MaxValue, TradeMath.AddedUp(long.MaxValue - 5L, 10L));
            Assert.Equal(long.MinValue, TradeMath.AddedUp(long.MinValue + 5L, -10L));
            Assert.Equal(2147483648L, TradeMath.SizeOf(int.MinValue));
            Assert.Equal(700L, TradeMath.SizeOf(-700));
        }

        [Fact]
        public void A_forecast_that_has_never_been_checked_is_taken_at_half()
        {
            Assert.Equal(0.5f, TradeMath.TrustBeforeAnyCheck);
            Assert.Equal(TradeMath.TrustBeforeAnyCheck, TradeMath.TrustInTheForecast(0L, 0.9f));
            Assert.Equal(TradeMath.TrustBeforeAnyCheck, TradeMath.TrustInTheForecast(-3L, 0.9f));
            Assert.Equal(TradeMath.TrustBeforeAnyCheck, TradeMath.TrustInTheForecast(15000L, TradeMath.NoShareToGive));
        }

        [Fact]
        public void A_forecast_that_keeps_missing_is_believed_less_and_less()
        {
            float few = TradeMath.TrustInTheForecast(600L, 0.3f);
            float many = TradeMath.TrustInTheForecast(240000L, 0.3f);
            Assert.True(few > many);
            Assert.True(many > 0.3f && many < 0.32f);
            Assert.True(TradeMath.TrustInTheForecast(240000L, 0f) < many);
            Assert.Equal(0.5f, TradeMath.TrustInTheForecast(900L, 0.5f), 4);
        }

        [Fact]
        public void A_forecast_that_keeps_coming_true_is_believed_more_and_more_but_never_past_full()
        {
            float few = TradeMath.TrustInTheForecast(600L, 1f);
            float many = TradeMath.TrustInTheForecast(240000L, 1f);
            Assert.True(few > TradeMath.TrustBeforeAnyCheck);
            Assert.True(many > few);
            Assert.Equal(0.75f, TradeMath.TrustInTheForecast(TradeMath.EnoughForecastWorth, 1f), 4);
            Assert.True(many > 0.95f && many < 1f);
            Assert.Equal(many, TradeMath.TrustInTheForecast(240000L, 1.7f), 4);
        }

        [Fact]
        public void A_landing_counts_in_full_well_before_you_arrive_in_part_near_it_and_not_at_all_well_after()
        {
            Assert.Equal(1f, TradeMath.ShareThatLands(0.75f, 1f));
            Assert.Equal(0.5f, TradeMath.ShareThatLands(1f, 1f), 4);
            Assert.Equal(0.75f, TradeMath.ShareThatLands(0.875f, 1f), 4);
            Assert.Equal(0f, TradeMath.ShareThatLands(1.25f, 1f));
            Assert.Equal(0f, TradeMath.ShareThatLands(3f, 1f));
            Assert.Equal(0f, TradeMath.ShareThatLands(float.NaN, 1f));
        }

        [Fact]
        public void On_a_short_ride_the_part_counted_narrows_to_half_the_ride_and_none_is_a_plain_test()
        {
            Assert.Equal(1f, TradeMath.ShareThatLands(0.125f, 0.25f));
            Assert.Equal(0.5f, TradeMath.ShareThatLands(0.25f, 0.25f), 4);
            Assert.Equal(0f, TradeMath.ShareThatLands(0.375f, 0.25f));
            Assert.Equal(1f, TradeMath.ShareThatLands(0f, 0.25f));
            Assert.Equal(1f, TradeMath.ShareThatLands(0f, 0f));
            Assert.Equal(0f, TradeMath.ShareThatLands(0.01f, 0f));
        }

        [Fact]
        public void A_landing_a_little_later_or_sooner_moves_its_share_only_a_little()
        {
            float hour = 1f / 24f;
            foreach (float horizon in new[] { 0.25f, 0.5f, 1f, 2.75f })
                for (float eta = 0f; eta < horizon + 1f; eta += hour)
                {
                    float jump = Math.Abs(TradeMath.ShareThatLands(eta + hour, horizon) -
                                          TradeMath.ShareThatLands(eta, horizon));
                    Assert.True(jump <= 4f * hour + 0.0001f);
                }
        }

        [Fact]
        public void What_is_on_its_way_is_counted_at_the_trust_it_has_earned()
        {
            Assert.Equal(500, TradeMath.WorthShiftTrusted(1000, 0.5f));
            Assert.Equal(-500, TradeMath.WorthShiftTrusted(-1000, 0.5f));
            Assert.Equal(1000, TradeMath.WorthShiftTrusted(1000, 1f));
            Assert.Equal(0, TradeMath.WorthShiftTrusted(1000, 0f));
            Assert.Equal(0, TradeMath.WorthShiftTrusted(0, 0.5f));
        }

        [Fact]
        public void A_few_small_forecasts_that_came_true_cannot_lift_the_trust_far()
        {
            float small = TradeMath.TrustInTheForecast(447L, 1f);
            Assert.True(small > TradeMath.TrustBeforeAnyCheck && small < 0.6f);
            Assert.True(TradeMath.TrustInTheForecast(25000L, 1f) > 0.9f);
            Assert.True(TradeMath.TrustInTheForecast(1249L, 0f) > 0.3f);
            Assert.True(TradeMath.TrustInTheForecast(25000L, 0f) < 0.1f);
        }

        [Fact]
        public void The_trust_in_a_forecast_never_overflows_however_much_it_has_judged()
        {
            Assert.Equal(1f, TradeMath.TrustInTheForecast(long.MaxValue, 1f), 4);
            Assert.Equal(0f, TradeMath.TrustInTheForecast(long.MaxValue, 0f), 4);
        }

        [Fact]
        public void A_forecast_that_has_been_missing_moves_a_shelf_less_than_one_that_has_not()
        {
            int wild = TradeMath.WorthShift(0, 8000);
            int held = TradeMath.WorthShiftTrusted(wild, TradeMath.TrustInTheForecast(240000L, 0.5f));
            Assert.True(held > wild);
            Assert.Equal(5000, TradeMath.ShelfAfterLanding(10000, wild));
            Assert.True(TradeMath.ShelfAfterLanding(10000, held) > 5000);
        }

        [Fact]
        public void A_market_has_to_beat_the_marked_town_by_a_clear_margin()
        {
            float marked = TradeMath.RateTheMarkHolds(500f, true);
            Assert.True(marked > 500f);
            Assert.False(TradeMath.RateTheMarkHolds(550f, false) > marked);
            Assert.True(TradeMath.RateTheMarkHolds(700f, false) > marked);
        }

        [Fact]
        public void A_town_that_is_not_marked_is_weighed_at_what_it_pays()
        {
            Assert.Equal(500f, TradeMath.RateTheMarkHolds(500f, false));
            Assert.Equal(0f, TradeMath.RateTheMarkHolds(0f, false));
        }

        [Fact]
        public void Villagers_sell_a_town_only_as_many_units_as_its_gold_can_pay_for()
        {
            Assert.Equal(10, TradeMath.WhatVillagersSell(10, 5000, 100));
            Assert.Equal(4, TradeMath.WhatVillagersSell(10, 450, 100));
            Assert.Equal(0, TradeMath.WhatVillagersSell(10, 99, 100));
            Assert.Equal(0, TradeMath.WhatVillagersSell(10, 0, 100));
            Assert.Equal(0, TradeMath.WhatVillagersSell(10, 5000, 0));
            Assert.Equal(0, TradeMath.WhatVillagersSell(0, 5000, 100));
        }

        [Fact]
        public void Wet_ground_takes_three_tenths_of_what_riders_and_footmen_on_horses_add_to_the_pace_as_the_game_counts_it()
        {
            Assert.Equal(-0.09f, TradeMath.WetGroundStep(100, 100, 0, 0), 4);
            Assert.Equal(-0.045f, TradeMath.WetGroundStep(100, 0, 100, 100), 4);
            Assert.Equal(-0.054f, TradeMath.WetGroundStep(100, 50, 50, 20), 4);
            Assert.Equal(-0.0225f, TradeMath.WetGroundStep(100, 0, 100, 50), 4);
        }

        [Fact]
        public void A_party_all_on_foot_or_with_no_men_is_not_slowed_by_wet_ground()
        {
            Assert.Equal(0f, TradeMath.WetGroundStep(100, 0, 100, 0));
            Assert.Equal(0f, TradeMath.WetGroundStep(0, 0, 0, 0));
            Assert.Equal(0f, TradeMath.WetGroundStep(-5, 3, 3, 3));
            Assert.Equal(-0.09f, TradeMath.WetGroundStep(10, 40, 0, 0), 4);
        }

        [Fact]
        public void A_route_carries_only_as_many_units_as_fit_in_the_room_your_cargo_will_have()
        {
            Assert.Equal(32, TradeMath.UnitsThatFit(80f, 2.5f));
            Assert.Equal(1, TradeMath.UnitsThatFit(1.9f, 1f));
            Assert.Equal(0, TradeMath.UnitsThatFit(0.9f, 1f));
            Assert.Equal(0, TradeMath.UnitsThatFit(0f, 1f));
            Assert.Equal(0, TradeMath.UnitsThatFit(-5f, 1f));
            Assert.Equal(0, TradeMath.UnitsThatFit(float.NaN, 1f));
            Assert.Equal(int.MaxValue, TradeMath.UnitsThatFit(10f, 0f));
            Assert.Equal(int.MaxValue, TradeMath.UnitsThatFit(float.PositiveInfinity, 1f));
            Assert.Equal(int.MaxValue, TradeMath.UnitsThatFit(10f, float.NaN));
        }

        [Fact]
        public void A_marked_town_that_pays_nothing_holds_on_to_nothing()
        {
            Assert.Equal(0f, TradeMath.RateTheMarkHolds(0f, true));
            Assert.Equal(float.MaxValue, TradeMath.RateTheMarkHolds(float.MaxValue, true));
            Assert.True(TradeMath.RateTheMarkHolds(1f, false) > TradeMath.RateTheMarkHolds(0f, true));
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
        public void A_ride_shorter_than_three_hours_is_still_a_ride_and_never_reads_as_no_time_at_all()
        {
            var rng = new System.Random(5517);
            for (int round = 0; round < 20000; round++)
            {
                float days = (float)(rng.NextDouble() * 0.125d);
                if (days <= 0f) continue;
                Assert.Equal(0f, TradeMath.ToTheQuarterDay(days), 4);
                Assert.Equal(TradeMath.HorizonStep, TradeMath.ARideToTheQuarterDay(days), 4);
            }
            Assert.Equal(TradeMath.HorizonStep, TradeMath.ARideToTheQuarterDay(float.Epsilon), 4);
            Assert.Equal(0f, TradeMath.ARideToTheQuarterDay(0f), 4);
            Assert.Equal(0f, TradeMath.ARideToTheQuarterDay(-3f), 4);
            Assert.Equal(0f, TradeMath.ARideToTheQuarterDay(float.NaN), 4);
            Assert.Equal(0f, TradeMath.ARideToTheQuarterDay(float.PositiveInfinity), 4);
            Assert.Equal(0.25f, TradeMath.ARideToTheQuarterDay(0.3f), 4);
            Assert.Equal(2.5f, TradeMath.ARideToTheQuarterDay(2.4f), 4);
        }

        [Fact]
        public void Two_trips_that_land_within_the_same_quarter_day_are_read_as_one_horizon()
        {
            Assert.Equal(TradeMath.ToTheQuarterDay(2.38f), TradeMath.ToTheQuarterDay(2.45f), 4);
            Assert.NotEqual(TradeMath.ToTheQuarterDay(2.3f), TradeMath.ToTheQuarterDay(2.45f));
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
            Assert.True(TradeMath.WorthScoring(2f, 3f));
            Assert.False(TradeMath.WorthScoring(2f, 3.01f));
            Assert.False(TradeMath.WorthScoring(2f, 5f));
            Assert.True(TradeMath.WorthScoring(0f, 1f));
            Assert.False(TradeMath.WorthScoring(0f, 1.5f));
            Assert.True(TradeMath.WorthScoring(-3f, 0.5f));
        }

        [Fact]
        public void A_forecast_is_judged_only_once_half_its_time_has_passed()
        {
            Assert.True(TradeMath.TooSoonToJudge(1.8f, 0f));
            Assert.True(TradeMath.TooSoonToJudge(1.8f, 0.2f));
            Assert.True(TradeMath.TooSoonToJudge(1.8f, 0.89f));
            Assert.False(TradeMath.TooSoonToJudge(1.8f, 0.9f));
            Assert.False(TradeMath.TooSoonToJudge(1.8f, 1.8f));
            Assert.False(TradeMath.TooSoonToJudge(1.8f, 4.6f));
        }

        [Fact]
        public void A_forecast_for_no_time_at_all_is_never_too_soon_to_judge()
        {
            Assert.False(TradeMath.TooSoonToJudge(0f, 0f));
            Assert.False(TradeMath.TooSoonToJudge(-2f, 0f));
            Assert.False(TradeMath.TooSoonToJudge(float.NaN, 0f));
            Assert.True(TradeMath.TooSoonToJudge(float.PositiveInfinity, 1000f));
        }

        [Fact]
        public void The_time_a_forecast_must_wait_is_half_of_the_time_it_was_for()
        {
            Assert.Equal(0.5f, TradeMath.SoonestAForecastIsJudged);
            for (float within = 0.1f; within < 6f; within += 0.37f)
            {
                Assert.True(TradeMath.TooSoonToJudge(within, within * 0.49f));
                Assert.False(TradeMath.TooSoonToJudge(within, within * 0.51f));
                Assert.True(TradeMath.WorthScoring(within, within * 0.51f));
            }
        }

        [Fact]
        public void What_you_put_into_a_market_is_worth_its_units_at_the_good_s_own_value()
        {
            Assert.Equal(960, TradeMath.YourOwnWorth(32, 30));
            Assert.Equal(-960, TradeMath.YourOwnWorth(-32, 30));
            Assert.Equal(0, TradeMath.YourOwnWorth(0, 30));
            Assert.Equal(0, TradeMath.YourOwnWorth(5, 0));
            Assert.Equal(0, TradeMath.YourOwnWorth(5, -10));
            Assert.Equal(int.MaxValue, TradeMath.YourOwnWorth(int.MaxValue, 7));
            Assert.Equal(int.MinValue, TradeMath.YourOwnWorth(int.MinValue, 7));
        }

        [Fact]
        public void Your_own_trades_add_up_without_ever_wrapping_round()
        {
            Assert.Equal(-12, TradeMath.AddedUp(-32, 20));
            Assert.Equal(int.MaxValue, TradeMath.AddedUp(int.MaxValue, 1));
            Assert.Equal(int.MinValue, TradeMath.AddedUp(int.MinValue, -1));
            Assert.Equal(0, TradeMath.AddedUp(0, 0));
        }

        [Fact]
        public void What_moved_leaves_out_what_you_moved_yourself()
        {
            Assert.Equal(0, TradeMath.WithoutYours(-32, -32));
            Assert.Equal(-6, TradeMath.WithoutYours(-38, -32));
            Assert.Equal(10, TradeMath.WithoutYours(50, 40));
            Assert.Equal(25, TradeMath.WithoutYours(25, 0));
            Assert.Equal(int.MaxValue, TradeMath.WithoutYours(int.MaxValue, -5));
            Assert.Equal(int.MinValue, TradeMath.WithoutYours(int.MinValue, 5));
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
            Assert.Equal(100f / TradeMath.WalkingPace * TradeMath.HoursAPartyTakesToCoverItsSpeed / 24f,
                         TradeMath.EtaDays(100f, float.NaN), 5);
        }

        [Fact]
        public void A_travel_time_the_game_can_work_out_is_left_exactly_as_it_was()
        {
            float hours = TradeMath.HoursAPartyTakesToCoverItsSpeed;
            Assert.Equal(120f / 5f * hours / 24f, TradeMath.DaysAtSpeed(120f, 1f, 5f, 9f), 5);
            Assert.Equal(120f / 9f * hours / 24f, TradeMath.DaysAtBestSpeed(120f, 5f, 9f), 5);
            Assert.Equal(120f / 5f * hours / 24f, TradeMath.EtaDays(120f, 5f), 5);
            Assert.Equal(0f, TradeMath.DaysAtSpeed(0f, 1f, 5f, 5f));
            Assert.Equal(0f, TradeMath.EtaDays(-5f, 5f));
        }

        [Fact]
        public void A_window_a_price_or_a_run_that_is_not_a_number_falls_back_to_the_safe_reading()
        {
            Assert.Equal(0f, TradeMath.ToTheQuarterDay(float.NaN));
            Assert.Equal(0f, TradeMath.ToTheQuarterDay(float.PositiveInfinity));
            Assert.Equal(TradeMath.TickTimeUnknown, TradeMath.NextDailyTickIn(double.NaN));
            Assert.Equal(TradeMath.TickTimeUnknown, TradeMath.NextDailyTickIn(double.PositiveInfinity));
            Assert.Equal(0f, TradeMath.WhatACaravanThinksAKindIsWorth(1f, float.NaN, 0, 4f, 50f, 1f, 1f, false, false));
            Assert.Equal(0f, TradeMath.MeanOf(float.NaN, 4));
            Assert.Equal(0f, TradeMath.DaysSince(float.PositiveInfinity, float.PositiveInfinity));
            Assert.Equal(0f, TradeMath.Realizable(100, float.NaN));
            Assert.Equal(0f, TradeMath.FleetSpeed(float.NaN, 2, 5f));
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
                    Assert.False(float.IsNaN(TradeMath.NextDailyTickIn(a)));
                    Assert.False(float.IsNaN(TradeMath.DaysSince(a, b)));
                    Assert.False(float.IsNaN(TradeMath.WhatACaravanThinksAKindIsWorth(a, b, 0, a, b, 1f, 1f, false, false)));
                    Assert.False(float.IsNaN(TradeMath.ACaravansRoomFactor(a, 1000)));
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
        public void A_party_short_of_its_days_of_food_may_pay_up_to_half_again_the_cheapest_price()
        {
            Assert.Equal(1.5f, TradeMath.ShortFoodTolerance);
            Assert.Equal(15, TradeMath.MostToPayOverTheCheapest(10, TradeMath.ShortFoodTolerance));
            Assert.True(TradeMath.ShortFoodTolerance < TradeMath.HungryFoodTolerance);
        }

        [Fact]
        public void A_party_short_of_a_day_of_food_may_pay_up_to_twice_the_cheapest_price()
        {
            Assert.Equal(2f, TradeMath.HungryFoodTolerance);
            Assert.Equal(220, TradeMath.MostToPayOverTheCheapest(110, TradeMath.HungryFoodTolerance));
        }

        [Fact]
        public void Only_the_food_short_of_one_day_is_bought_above_the_cheapest_price()
        {
            Assert.Equal(7, TradeMath.FoodShortOfADay(3, 10));
            Assert.Equal(10, TradeMath.FoodShortOfADay(0, 10));
            Assert.Equal(10, TradeMath.FoodShortOfADay(-4, 10));
            Assert.Equal(0, TradeMath.FoodShortOfADay(10, 10));
            Assert.Equal(0, TradeMath.FoodShortOfADay(25, 10));
            Assert.Equal(0, TradeMath.FoodShortOfADay(0, 0));
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
            Assert.True(TradeMath.DaysAtSpeed(240f, 1f, 10f, 10f) ==
                        240f / 10f * TradeMath.HoursAPartyTakesToCoverItsSpeed / 24f);
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
        public void A_price_the_forecast_did_not_move_is_left_exactly_where_it_was()
        {
            foreach (int price in new[] { 1, 2, 3, 7, 99, 100, 101, 5000 })
                Assert.Equal(price, TradeMath.ForecastWithin(price, price));
            foreach (int forecast in new[] { 1, 20, 49, 50, 51, 149, 150, 151, 900 })
            {
                int held = TradeMath.ForecastWithin(100, forecast);
                Assert.Equal(held, TradeMath.ForecastWithin(100, held));
            }
        }

        private static int Flooded(int shift) => Math.Max(1, 100 - shift / 10);

        private static int Drained(int shift) => Math.Max(1, 100 - shift / 5);

        [Fact]
        public void A_landing_that_keeps_the_first_unit_within_reach_is_left_whole()
        {
            Assert.Equal(300, TradeMath.LandingWithinReach(100, 300, Flooded(300), Flooded));
            Assert.Equal(500, TradeMath.LandingWithinReach(100, 500, Flooded(500), Flooded));
            Assert.Equal(-250, TradeMath.LandingWithinReach(100, -250, Drained(-250), Drained));
            Assert.Equal(0, TradeMath.LandingWithinReach(100, 0, 900, Flooded));
        }

        [Fact]
        public void A_landing_that_would_move_the_first_unit_too_far_is_held_to_the_most_that_keeps_it_within_reach()
        {
            int flood = TradeMath.LandingWithinReach(100, 5000, Flooded(5000), Flooded);
            Assert.Equal(509, flood);
            Assert.Equal(50, Flooded(flood));
            Assert.Equal(49, Flooded(flood + 1));

            int drain = TradeMath.LandingWithinReach(100, -4000, Drained(-4000), Drained);
            Assert.Equal(-254, drain);
            Assert.Equal(150, Drained(drain));
            Assert.Equal(151, Drained(drain - 1));
        }

        [Fact]
        public void Every_landing_too_far_the_same_way_is_held_to_the_same_most()
        {
            foreach (int landed in new[] { 510, 600, 1000, 5000, 100000, int.MaxValue })
                Assert.Equal(509, TradeMath.LandingWithinReach(100, landed, Flooded(landed), Flooded));
            foreach (int landed in new[] { -255, -300, -4000, -100000, int.MinValue })
                Assert.Equal(-254, TradeMath.LandingWithinReach(100, landed, Drained(landed), Drained));
        }

        [Fact]
        public void A_held_landing_never_reaches_further_than_the_landing_itself()
        {
            Assert.Equal(300, TradeMath.NoFurtherThan(300, 509));
            Assert.Equal(509, TradeMath.NoFurtherThan(5000, 509));
            Assert.Equal(-100, TradeMath.NoFurtherThan(-100, -254));
            Assert.Equal(-254, TradeMath.NoFurtherThan(-4000, -254));
            Assert.Equal(0, TradeMath.NoFurtherThan(700, 0));
            Assert.Equal(0, TradeMath.NoFurtherThan(-700, 0));
        }

        [Fact]
        public void A_caravan_sells_nothing_where_a_kind_sells_below_its_limit_against_the_average()
        {
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, 1.0f, 1.0f, 0.8f, 0, 1000f, 100, 100, false, false));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, 0.8f, 1.0f, 0.8f, 0, 1000f, 100, 100, false, false));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, 1.2f, 1.2f, 1.0f, 0, 1000f, 100, 100, false, false));
        }

        [Fact]
        public void A_caravan_laden_with_a_kind_sells_it_even_where_it_fetches_no_more_than_usual()
        {
            Assert.Equal(12, TradeMath.WhatACaravanUnloads(100, 1.0f, 1.0f, 0.8f, 5000, 1000f, 100, 100, false, false));
            Assert.Equal(5, TradeMath.WhatACaravanUnloads(100, 1.0f, 1.0f, 0.8f, 1250, 1000f, 100, 100, false, false));
            Assert.Equal(4, TradeMath.WhatACaravanUnloads(100, 0.8f, 1.0f, 0.8f, 5000, 1000f, 100, 100, false, false));
            Assert.Equal(0.4f, TradeMath.WhatHoldingAKindTakesOff(9000), 4);
            Assert.Equal(0f, TradeMath.WhatHoldingAKindTakesOff(0));
        }

        [Fact]
        public void A_convoy_sells_five_thirds_of_what_a_caravan_does_and_haul_animals_half_again_as_much()
        {
            Assert.Equal(6, TradeMath.WhatACaravanUnloads(100, 1.2f, 1.0f, 0.8f, 0, 1000f, 100, 100, false, false));
            Assert.Equal(10, TradeMath.WhatACaravanUnloads(100, 1.2f, 1.0f, 0.8f, 0, 1000f, 100, 100, true, false));
            Assert.Equal(9, TradeMath.WhatACaravanUnloads(100, 1.2f, 1.0f, 0.8f, 0, 1000f, 100, 100, false, true));
        }

        [Fact]
        public void A_caravan_unloads_no_more_than_it_carries_and_reads_nothing_it_cannot()
        {
            Assert.Equal(5, TradeMath.WhatACaravanUnloads(5, 1.5f, 1.0f, 0.8f, 0, 1000f, 100, 100, false, false));
            Assert.Equal(1, TradeMath.WhatACaravanUnloads(100, 1.2f, 1.0f, 0.8f, 0, 0f, 100, 100, false, false));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(0, 2.0f, 1.0f, 0.8f, 0, 1000f, 100, 100, false, false));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, 2.0f, 1.0f, 0.8f, 0, 1000f, 100, 0, false, false));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, float.NaN, 1.0f, 0.8f, 0, 1000f, 100, 100, false, false));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, 1.2f, float.NaN, 0.8f, 0, 1000f, 100, 100, false, false));
            Assert.Equal(0, TradeMath.WhatACaravanUnloads(100, 1.2f, 0f, 0.8f, 0, 1000f, 100, 100, false, false));
        }

        [Fact]
        public void A_caravan_sells_off_the_animals_it_drives_past_three_for_every_five_men()
        {
            Assert.Equal(4, TradeMath.AnimalsACaravanSellsOff(10, 10));
            Assert.Equal(1, TradeMath.AnimalsACaravanSellsOff(7, 10));
            Assert.Equal(0, TradeMath.AnimalsACaravanSellsOff(6, 10));
            Assert.Equal(0, TradeMath.AnimalsACaravanSellsOff(2, 10));
        }

        [Fact]
        public void A_caravan_wants_a_kind_by_how_far_under_the_average_it_sells()
        {
            float budget = TradeMath.ACaravansBudgetFactor(10000), room = TradeMath.ACaravansRoomFactor(0f, 1000);
            Assert.Equal(23.5714f, TradeMath.WhatACaravanThinksAKindIsWorth(0.7f, 1f, 0, 4f, 50f, budget, room, false, false), 3);
            Assert.Equal(25.9286f, TradeMath.WhatACaravanThinksAKindIsWorth(0.7f, 1f, 0, 4f, 50f, budget, room, true, false), 3);
            Assert.Equal(35.3571f, TradeMath.WhatACaravanThinksAKindIsWorth(0.7f, 1f, 0, 4f, 50f, budget, room, false, true), 3);
            Assert.Equal(132f, TradeMath.WhatACaravanThinksAKindIsWorth(0.5f, 1f, 0, 9f, 200f, budget, room, false, false), 3);
            Assert.Equal(0f, TradeMath.WhatACaravanThinksAKindIsWorth(1f, 1f, 0, 4f, 50f, budget, room, false, false));
            Assert.Equal(0f, TradeMath.WhatACaravanThinksAKindIsWorth(0.7f, 1f, 5000, 4f, 50f, budget, room, false, false));
            Assert.Equal(0f, TradeMath.WhatACaravanThinksAKindIsWorth(float.NaN, 1f, 0, 4f, 50f, budget, room, false, false));
        }

        [Fact]
        public void A_caravan_wants_more_with_a_fuller_purse_and_less_with_a_fuller_cargo()
        {
            Assert.Equal(0.1f, TradeMath.ACaravansBudgetFactor(0), 4);
            Assert.Equal(0.6f, TradeMath.ACaravansBudgetFactor(2500), 4);
            Assert.Equal(1.1f, TradeMath.ACaravansBudgetFactor(5000), 4);
            Assert.Equal(1.1f, TradeMath.ACaravansBudgetFactor(20000), 4);
            Assert.Equal(1.1f, TradeMath.ACaravansRoomFactor(0f, 1000), 4);
            Assert.Equal(0.6504f, TradeMath.ACaravansRoomFactor(500f, 1000), 3);
            Assert.Equal(0.1f, TradeMath.ACaravansRoomFactor(5000f, 1000), 4);
            Assert.Equal(1.1f, TradeMath.ACaravansRoomFactor(float.NaN, 1000), 4);
        }

        [Fact]
        public void A_caravan_spends_at_most_half_its_purse_half_again_what_a_kind_is_worth_and_the_most_on_one_kind()
        {
            Assert.Equal(1500, TradeMath.WhatACaravanSpendsOnAKind(10000, 2000f, 1500));
            Assert.Equal(1000, TradeMath.WhatACaravanSpendsOnAKind(2000, 2000f, 1500));
            Assert.Equal(600, TradeMath.WhatACaravanSpendsOnAKind(10000, 400f, 1500));
            Assert.Equal(0, TradeMath.WhatACaravanSpendsOnAKind(10000, 6.9f, 1500));
            Assert.Equal(0, TradeMath.WhatACaravanSpendsOnAKind(0, 2000f, 1500));
            Assert.Equal(0, TradeMath.WhatACaravanSpendsOnAKind(10000, float.NaN, 1500));
            Assert.Equal(0, TradeMath.WhatACaravanSpendsOnAKind(10000, 2000f, 0));
        }

        [Fact]
        public void A_caravan_takes_what_it_spends_for_within_the_shelf_its_limit_and_its_cargo()
        {
            Assert.Equal(30, TradeMath.WhatACaravanTakesOfAKind(1500, 50, 100, 300, 1f, 0f, 1000, false));
            Assert.Equal(20, TradeMath.WhatACaravanTakesOfAKind(1500, 50, 20, 300, 1f, 0f, 1000, false));
            Assert.Equal(300, TradeMath.WhatACaravanTakesOfAKind(1500, 2, 1000, 300, 1f, 0f, 1000, false));
            Assert.Equal(10, TradeMath.WhatACaravanTakesOfAKind(1500, 50, 100, 300, 10f, 800f, 1000, false));
            Assert.Equal(30, TradeMath.WhatACaravanTakesOfAKind(1500, 50, 100, 300, 10f, 800f, 1000, true));
            Assert.Equal(0, TradeMath.WhatACaravanTakesOfAKind(1500, 50, 100, 300, 10f, 950f, 1000, false));
            Assert.Equal(0, TradeMath.WhatACaravanTakesOfAKind(0, 50, 100, 300, 1f, 0f, 1000, false));
            Assert.Equal(0, TradeMath.WhatACaravanTakesOfAKind(1500, 0, 100, 300, 1f, 0f, 1000, false));
            Assert.Equal(0, TradeMath.WhatACaravanTakesOfAKind(1500, 50, 100, -3, 1f, 0f, 1000, false));
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
        public void A_trip_shorter_than_half_a_day_counts_as_half_a_day()
        {
            Assert.Equal(0.5f, TradeMath.NoTripCountsShorterThan);
            Assert.Equal(TradeMath.PerDay(100, 0f), TradeMath.PerDay(100, TradeMath.NoTripCountsShorterThan));
            Assert.Equal(200f, TradeMath.PerDay(100, 0.01f));
        }

        [Fact]
        public void With_the_whole_load_switch_off_a_market_is_weighed_on_its_first_unit_alone()
        {
            Assert.Equal(200f, TradeMath.OneUnitRate(400, 300, 0.3f, false, 0.9f));
            Assert.Equal(100f, TradeMath.OneUnitRate(400, 300, 0.5f, true, 0.875f));
            Assert.Equal(0f, TradeMath.OneUnitRate(300, 300, 1f, false, 1f));
            Assert.Equal(0f, TradeMath.OneUnitRate(0, 0, 1f, false, 1f));
            Assert.Equal(0f, TradeMath.OneUnitRate(400, 300, float.NaN, false, 1f));
        }

        [Fact]
        public void A_town_at_the_door_paying_far_less_no_longer_outpaces_a_richer_one_a_day_away()
        {
            Assert.True(TradeMath.PerDay(349, 1.03f) > TradeMath.PerDay(154, 0.18f));
            Assert.True(TradeMath.PerDay(900, 1.03f) > TradeMath.PerDay(349, 1.03f));
        }

        [Fact]
        public void A_nearer_market_paying_less_in_all_still_earns_more_a_day()
        {
            Assert.True(TradeMath.PerDay(2069f, 3f) > TradeMath.PerDay(2140f, 7.5f));
            Assert.Equal(2140f / 7.5f, TradeMath.PerDay(2140f, 7.5f), 2);
        }

        [Fact]
        public void A_far_market_paying_far_more_still_wins()
        {
            Assert.True(TradeMath.PerDay(9000f, 7.5f) > TradeMath.PerDay(500f, 1f));
        }

        [Fact]
        public void Nothing_to_carry_there_and_no_trip_at_all_are_both_handled()
        {
            Assert.Equal(0f, TradeMath.PerDay(0f, 3f));
            Assert.Equal(0f, TradeMath.PerDay(-5f, 3f));
            Assert.Equal(0f, TradeMath.PerDay(100f, float.NaN));
            Assert.Equal(TradeMath.PerDay(100f, 0f),
                         TradeMath.PerDay(100f, TradeMath.NoTripCountsShorterThan));
        }

        [Fact]
        public void The_adaptive_spend_limit_holds_the_base_until_the_purse_is_five_times_it()
        {
            Assert.Equal(1000, TradeMath.AdaptiveSpendCap(1000, 0, true));
            Assert.Equal(1000, TradeMath.AdaptiveSpendCap(1000, 1000, true));
            Assert.Equal(1000, TradeMath.AdaptiveSpendCap(1000, 5000, true));
            Assert.Equal(1000, TradeMath.AdaptiveSpendCap(1000, -500, true));
        }

        [Fact]
        public void The_adaptive_spend_limit_adds_one_base_each_time_the_purse_doubles_past_that()
        {
            Assert.Equal(2000, TradeMath.AdaptiveSpendCap(1000, 10000, true));
            Assert.Equal(3000, TradeMath.AdaptiveSpendCap(1000, 20000, true));
            Assert.Equal(4000, TradeMath.AdaptiveSpendCap(1000, 40000, true));
            Assert.Equal(1584, TradeMath.AdaptiveSpendCap(1000, 7500, true));
            Assert.Equal(4380, TradeMath.AdaptiveSpendCap(1000, 52068, true));
            Assert.Equal(4000, TradeMath.AdaptiveSpendCap(2000, 20000, true));
            Assert.True(TradeMath.AdaptiveSpendCap(1000, 100000000L, true) < 20000);
            int before = 1000;
            for (long purse = 5000; purse <= 1000000; purse += 2500)
            {
                int now = TradeMath.AdaptiveSpendCap(1000, purse, true);
                Assert.True(now >= before);
                before = now;
            }
        }

        [Fact]
        public void With_the_adaptive_spend_limit_off_or_no_base_the_setting_stands_as_it_is()
        {
            Assert.Equal(1000, TradeMath.AdaptiveSpendCap(1000, 50000, false));
            Assert.Equal(0, TradeMath.AdaptiveSpendCap(0, 50000, true));
            Assert.Equal(int.MaxValue, TradeMath.AdaptiveSpendCap(int.MaxValue, long.MaxValue, true));
        }

        [Fact]
        public void The_fewest_points_that_let_a_skill_learn_again_are_counted_up_from_one()
        {
            Assert.Equal(1, TradeMath.FewestThatLets(5, more => more >= 1));
            Assert.Equal(3, TradeMath.FewestThatLets(5, more => more >= 3));
            Assert.Equal(5, TradeMath.FewestThatLets(5, more => more >= 5));
            Assert.Equal(0, TradeMath.FewestThatLets(5, more => more >= 6));
            Assert.Equal(0, TradeMath.FewestThatLets(0, more => true));
            Assert.Equal(0, TradeMath.FewestThatLets(-2, more => true));
            Assert.Equal(0, TradeMath.FewestThatLets(5, null));
        }

        [Fact]
        public void Trade_one_point_under_its_learning_limit_starts_the_warning_and_two_under_ends_it()
        {
            Assert.False(TradeMath.NearTheLearningLimit(53, 55));
            Assert.True(TradeMath.NearTheLearningLimit(54, 55));
            Assert.True(TradeMath.NearTheLearningLimit(55, 55));
            Assert.True(TradeMath.NearTheLearningLimit(70, 55));
            Assert.False(TradeMath.NearTheLearningLimit(54, 56));
            Assert.True(TradeMath.NearTheLearningLimit(0, 0));
        }

        [Fact]
        public void The_fewest_focus_points_that_end_the_warning_lift_the_limit_two_clear_of_the_skill()
        {
            System.Func<int, int> limit = focus => 25 + 30 * focus;
            Assert.Equal(1, TradeMath.FewestThatLets(5, more => !TradeMath.NearTheLearningLimit(54, limit(1 + more))));
            Assert.Equal(2, TradeMath.FewestThatLets(5, more => !TradeMath.NearTheLearningLimit(113, limit(1 + more))));
            Assert.Equal(3, TradeMath.FewestThatLets(5, more => !TradeMath.NearTheLearningLimit(114, limit(1 + more))));
        }

        [Fact]
        public void A_learning_rate_the_game_floors_at_nothing_is_lifted_by_one_social_point_or_one_focus_point()
        {
            System.Func<int, int, int, float> rate = (social, focus, skill) =>
            {
                int limit = (int)System.Math.Round(System.Math.Max(0f, (social - 1f) * 10f) + focus * 30f);
                float factors = 0.4f * social;
                if (focus != 0) factors += focus * 1f;
                if (skill > limit) factors += -1f - 0.1f * (skill - limit);
                float result = 1.25f + 1.25f * factors;
                return result < 0f ? 0f : result;
            };
            Assert.True(rate(2, 0, 18) > 0f);
            Assert.False(TradeMath.StillLearns(rate(2, 0, 18)));
            Assert.Equal(1, TradeMath.FewestThatLets(5, more => TradeMath.StillLearns(rate(2, more, 18))));
            Assert.Equal(1, TradeMath.FewestThatLets(8, more => TradeMath.StillLearns(rate(2 + more, 0, 18))));
            Assert.True(TradeMath.StillLearns(rate(2, 0, 17)));
            Assert.Equal(0f, rate(2, 0, 19));
        }

        [Fact]
        public void A_learning_rate_left_over_from_rounding_teaches_nothing_and_the_smallest_real_one_still_does()
        {
            Assert.False(TradeMath.StillLearns(0f));
            Assert.False(TradeMath.StillLearns(1.1920929e-7f));
            Assert.False(TradeMath.StillLearns(5.9604645e-7f));
            Assert.False(TradeMath.StillLearns(-0.5f));
            Assert.False(TradeMath.StillLearns(float.NaN));
            Assert.True(TradeMath.StillLearns(0.125f));
            Assert.True(TradeMath.StillLearns(TradeMath.SmallestLearningRateThatTeaches));
            Assert.True(TradeMath.StillLearns(1.25f));
        }

        [Fact]
        public void The_marked_market_takes_the_units_that_clear_your_margin_at_what_it_pays_for_each()
        {
            int[] rungs = { 415, 400, 380, 300, 110 };
            Assert.Equal(new[] { 415, 400, 380, 300 }, TradeMath.WhatTheMarkTakes(u => rungs[u], 5, 100, 0.15f, -1));
        }

        [Fact]
        public void The_marked_market_takes_no_more_than_its_purse_or_than_you_carry()
        {
            int[] rungs = { 415, 400, 380, 300 };
            Assert.Equal(new[] { 415, 400 }, TradeMath.WhatTheMarkTakes(u => rungs[u], 4, 100, 0.15f, 1000));
            Assert.Equal(new[] { 415 }, TradeMath.WhatTheMarkTakes(u => rungs[u], 1, 100, 0.15f, -1));
        }

        [Fact]
        public void A_marked_market_that_pays_nothing_or_misses_your_margin_takes_nothing()
        {
            Assert.Empty(TradeMath.WhatTheMarkTakes(u => 0, 3, 100, 0.15f, -1));
            Assert.Empty(TradeMath.WhatTheMarkTakes(u => 114, 3, 100, 0.15f, -1));
            Assert.Empty(TradeMath.WhatTheMarkTakes(u => 500, 0, 100, 0.15f, -1));
            Assert.Empty(TradeMath.WhatTheMarkTakes(null, 3, 100, 0.15f, -1));
            Assert.Empty(TradeMath.WhatTheMarkTakes(u => 500, 3, 100, 0.15f, 0));
        }
    
        [Fact]
        public void A_unit_from_a_villagers_offer_written_down_at_no_cost_says_so_when_it_sells()
        {
            Assert.Equal("source 3 (villager party), came with a villagers' offer at no cost",
                         TradeMath.WhereFreeFrom(TradeMath.FromVillagers));
            Assert.Equal(TradeMath.WhereFrom(TradeMath.FromLoot), TradeMath.WhereFreeFrom(TradeMath.FromLoot));
            Assert.Equal(TradeMath.WhereFrom(TradeMath.FromElsewhere), TradeMath.WhereFreeFrom(TradeMath.FromElsewhere));
            Assert.Equal("source 3 (villager party), bought", TradeMath.WhereFrom(TradeMath.FromVillagers));
        }

        [Fact]
        public void A_food_is_never_refused_for_price_at_up_to_twice_its_value()
        {
            Assert.Equal(2f, TradeMath.FoodValueTolerance);
            int known = TradeMath.MostToPayOverTheCheapest(10, TradeMath.ShortFoodTolerance);
            Assert.Equal(40, TradeMath.AtLeastTwiceItsValue(known, 20));
            Assert.Equal(15, TradeMath.AtLeastTwiceItsValue(known, 5));
            Assert.Equal(16, TradeMath.AtLeastTwiceItsValue(TradeMath.MostToPayOverTheCheapest(0, TradeMath.ShortFoodTolerance), 8));
            Assert.Equal(20, TradeMath.AtLeastTwiceItsValue(TradeMath.MostToPayOverTheCheapest(10, TradeMath.HungryFoodTolerance), 8));
            Assert.Equal(0, TradeMath.AtLeastTwiceItsValue(0, 0));
        }

        [Fact]
        public void The_dear_first_walk_says_whether_the_unit_it_took_was_bought_to_trade()
        {
            var costs = new[] { new Batch { Unit = 10, Count = 1, Traded = true }, new Batch { Unit = 20, Count = 1 } };
            var walk = TradeMath.DearFirst.EachAtItsOwnCost(costs, 0, 0, 0f);
            walk.Floor(100);
            Assert.Equal(20, walk.Took());
            Assert.False(walk.TookTraded);
            walk.Floor(100);
            Assert.Equal(10, walk.Took());
            Assert.True(walk.TookTraded);

            var average = new TradeMath.DearFirst(new[] { new Batch { Unit = 10, Count = 1, Traded = true } }, 15, 10);
            Assert.Equal(10, average.Took());
            Assert.True(average.TookTraded);
        }

        [Fact]
        public void Units_TradeLord_laid_out_to_trade_are_marked_so_and_units_added_by_hand_are_not()
        {
            var rec = new PurchaseRecord { ItemId = "fish" };
            TradeMath.AddPurchase(rec, new[] { 30, 30 }, 10L, 100f);
            TradeMath.AddPurchase(rec, new[] { 32, 32, 33 }, 12L, 101f);
            Assert.Equal(2, TradeMath.MarkBoughtToTrade(rec, 12L, 2));
            Assert.Equal(2, TradeMath.UnitsBoughtToTrade(rec));
            Assert.Equal(0, TradeMath.MarkBoughtToTrade(rec, 12L, 2));
            Assert.Equal(0, TradeMath.MarkBoughtToTrade(rec, 0L, 2));
            Assert.Equal(5, rec.Count);
        }

        [Fact]
        public void Food_that_left_unsold_comes_off_your_own_units_first_and_food_bought_to_trade_last()
        {
            var bought = new PurchaseRecord { ItemId = "fish" };
            TradeMath.AddPurchase(bought, new[] { 30, 30 }, 10L, 100f, TradeMath.FromAMarket, traded: true);
            TradeMath.AddPurchase(bought, new[] { 20, 20 }, 12L, 101f);
            var free = new PurchaseRecord { ItemId = "fish" };
            TradeMath.AddPurchase(free, new[] { 0 }, 14L, 102f, TradeMath.FromLoot);

            Assert.Equal(2, TradeMath.DrainYourOwnFoodFirst(bought, free, 3));
            Assert.Equal(0, free.Count);
            Assert.Equal(2, bought.Count);
            Assert.Equal(2, TradeMath.UnitsBoughtToTrade(bought));

            Assert.Equal(1, TradeMath.DrainYourOwnFoodFirst(bought, null, 1));
            Assert.Equal(1, TradeMath.UnitsBoughtToTrade(bought));
            Assert.Equal(1, bought.Count);
        }

        [Fact]
        public void A_food_record_claiming_more_than_is_held_drops_your_own_units_first()
        {
            var rec = new PurchaseRecord { ItemId = "fish" };
            TradeMath.AddPurchase(rec, new[] { 30 }, 10L, 100f, TradeMath.FromAMarket, traded: true);
            TradeMath.AddPurchase(rec, new[] { 20 }, 11L, 101f);

            Batch[] food = TradeMath.UnitCostsOfFood(rec, 1);
            Assert.Single(food);
            Assert.True(food[0].Traded);
            Assert.Equal(30, food[0].Unit);

            Batch[] other = TradeMath.UnitCosts(rec, 1);
            Assert.Single(other);
            Assert.Equal(20, other[0].Unit);
        }

        private static bool TodayThrowsAway(int openingBuy, int openingSell, int stocked, int till, float days,
                                            bool beaten, float bestKey, float safety, float margin)
        {
            float realizable = TradeMath.Realizable(openingSell, safety);
            if (!TradeMath.BuyAcceptable(openingBuy, realizable, margin)) return true;
            int qtyCap = till > 0 ? Math.Min(stocked, till / openingSell) : stocked;
            if (qtyCap <= 0) return true;
            float ceiling = (float)(openingSell - openingBuy) * qtyCap;
            return beaten && TradeMath.PerDay(ceiling, days) <= bestKey;
        }

        [Fact]
        public void A_forecast_never_lifts_a_price_past_the_most_the_route_scan_counts_on()
        {
            foreach (int live in new[] { 1, 2, 3, 7, 10, 33, 99, 100, 101, 250, 999, 1000, 4097, 65535 })
            {
                int most = TradeMath.MostAForecastCanReach(live);
                Assert.Equal(most, TradeMath.ForecastWithin(live, int.MaxValue));
                foreach (int forecast in new[] { -5, 0, 1, live / 2, live, most - 1, most, most + 1, live * 3 })
                    Assert.True(TradeMath.ForecastWithin(live, forecast) <= most);
            }
        }

        [Fact]
        public void A_pair_the_route_scan_passes_over_is_one_it_would_throw_away_at_any_opening_price()
        {
            int passed = 0;
            foreach (float safety in new[] { 0.6f, 0.85f, 1f })
                foreach (float margin in new[] { 0f, 0.15f, 0.4f })
                    foreach (int buy in new[] { 0, 1, 40, 99, 250 })
                        foreach (int quoted in new[] { 1, 30, 70, 100, 180, 400 })
                            foreach (int stocked in new[] { 1, 6, 30 })
                                foreach (float days in new[] { 0.2f, 2f, 9f })
                                    foreach (float bestKey in new[] { 0f, 5f, 80f, 900f })
                                        foreach (bool beaten in new[] { false, true })
                                        {
                                            if (!TradeMath.NoOpeningPriceCouldMakeIt(buy, quoted, 3, stocked, days,
                                                                                    beaten, bestKey, safety, margin))
                                                continue;
                                            passed++;
                                            int most = TradeMath.MostAForecastCanReach(quoted);
                                            foreach (int walked in new[] { -3, 0, 1, quoted / 2, quoted, most, most * 2 })
                                                foreach (int till in new[] { 0, 50, 5000 })
                                                    Assert.True(TodayThrowsAway(buy, TradeMath.ForecastWithin(quoted, walked),
                                                                                stocked, till, days, beaten, bestKey,
                                                                                safety, margin));
                                        }
            Assert.True(passed > 0);
            Assert.False(TradeMath.NoOpeningPriceCouldMakeIt(10, 100, 0, 5, 2f, true, 99999f, 0.85f, 0.15f));
            Assert.False(TradeMath.NoOpeningPriceCouldMakeIt(10, 0, 3, 5, 2f, true, 99999f, 0.85f, 0.15f));
            Assert.False(TradeMath.NoOpeningPriceCouldMakeIt(10, 100, 3, 5, 2f, true, 99999f, float.NaN, 0.15f));
        }

        [Fact]
        public void The_route_scan_picks_the_same_routes_with_and_without_passing_over_pairs()
        {
            var rng = new Random(1031);
            int skipped = 0;
            for (int item = 0; item < 400; item++)
            {
                float safety = new[] { 0.6f, 0.85f, 1f }[rng.Next(3)];
                float margin = new[] { 0f, 0.15f, 0.3f }[rng.Next(3)];
                var pairs = new List<(int buy, int quoted, int landed, int walked, int stocked, int till, float days)>();
                int many = rng.Next(1, 60);
                for (int i = 0; i < many; i++)
                    pairs.Add((rng.Next(1, 400), rng.Next(1, 600), rng.Next(3) == 0 ? 0 : rng.Next(-40, 41),
                               rng.Next(-10, 1200), rng.Next(1, 60), rng.Next(3) == 0 ? 0 : rng.Next(0, 20000),
                               0.1f + (float)rng.NextDouble() * 15f));
                var plain = Scan(pairs, false, safety, margin, out int plainWalks, out _);
                var quick = Scan(pairs, true, safety, margin, out int quickWalks, out int passed);
                skipped += passed;
                Assert.Equal(plain, quick);
                Assert.Equal(plainWalks, quickWalks);
            }
            Assert.True(skipped > 0);
        }

        private static (int at, float key) Scan(
            List<(int buy, int quoted, int landed, int walked, int stocked, int till, float days)> pairs,
            bool passOver, float safety, float margin, out int walks, out int passed)
        {
            int bestAt = -1;
            float bestKey = 0f;
            walks = 0;
            passed = 0;
            for (int i = 0; i < pairs.Count; i++)
            {
                var p = pairs[i];
                if (passOver && TradeMath.NoOpeningPriceCouldMakeIt(p.buy, p.quoted, p.landed, p.stocked, p.days,
                                                                    bestAt >= 0, bestKey, safety, margin))
                {
                    passed++;
                    continue;
                }
                int openingSell = p.landed == 0 ? p.walked : TradeMath.ForecastWithin(p.quoted, p.walked);
                if (TodayThrowsAway(p.buy, openingSell, p.stocked, p.till, p.days, bestAt >= 0, bestKey, safety, margin))
                    continue;
                walks++;
                int units = p.till > 0 ? Math.Min(p.stocked, p.till / openingSell) : p.stocked;
                int profit = units * (openingSell - p.buy) - units * (units - 1) / 2;
                if (profit <= 0) continue;
                float key = TradeMath.PerDay(profit, p.days);
                if (bestAt >= 0 && key <= bestKey) continue;
                bestAt = i;
                bestKey = key;
            }
            return (bestAt, bestKey);
        }

        [Fact]
        public void The_key_is_profit_at_its_Conf_for_each_unit_of_weight_a_day()
        {
            Assert.Equal(2160f * 0.2f / (9f * 0.9f), TradeMath.Key(2160, 0.2f, 1f, true, 1f, 9, 0.9f), 3);
            Assert.Equal(2160f / (9f * 0.9f), TradeMath.Key(2160, 0.2f, 1f, false, 1f, 9, 0.9f), 3);
            Assert.Equal(2160f * 0.2f * 0.8f / (9f * 0.9f), TradeMath.Key(2160, 0.2f, 0.8f, true, 1f, 9, 0.9f), 3);
            Assert.Equal(2 * TradeMath.Key(1000, 1f, 1f, true, 2f, 5, 1f), TradeMath.Key(1000, 1f, 1f, true, 1f, 5, 1f), 3);
        }

        [Fact]
        public void The_key_floors_a_short_hop_at_half_a_day_and_a_feather_at_a_tenth_of_a_weight()
        {
            Assert.Equal(TradeMath.Key(100, 1f, 1f, true, 1f, 1, 0.5f), TradeMath.Key(100, 1f, 1f, true, 1f, 1, 0.05f));
            Assert.Equal(TradeMath.Key(100, 1f, 1f, true, 0.1f, 1, 1f), TradeMath.Key(100, 1f, 1f, true, 0f, 1, 1f));
            Assert.Equal(0f, TradeMath.Key(0, 1f, 1f, true, 1f, 1, 1f));
            Assert.Equal(0f, TradeMath.Key(-50, 1f, 1f, true, 1f, 1, 1f));
            Assert.Equal(0f, TradeMath.Key(100, 1f, 1f, true, 1f, 0, 1f));
            Assert.Equal(0f, TradeMath.Key(100, 1f, 1f, true, 1f, 1, float.NaN));
        }

        [Fact]
        public void A_herd_slot_counts_as_the_share_of_the_hold_it_stands_for()
        {
            Assert.Equal(10f, TradeMath.HerdWeight(220f, 2, 20));
            Assert.Equal(0f, TradeMath.HerdWeight(220f, 0, 0));
            Assert.Equal(0f, TradeMath.HerdWeight(0f, 3, 3));
            Assert.Equal(0f, TradeMath.HerdWeight(float.PositiveInfinity, 3, 3));
        }

        [Fact]
        public void The_spend_limit_is_Max_spend_per_visit_or_the_ledgers_expected_profit_whichever_is_more()
        {
            Assert.Equal(2111, TradeMath.SpendLimit(1000, 2644, true, 2111));
            Assert.Equal(1000, TradeMath.SpendLimit(1000, 2644, true, 400));
            Assert.Equal(TradeMath.AdaptiveSpendCap(1000, 40000, true), TradeMath.SpendLimit(1000, 40000, true, 900));
            Assert.Equal(0, TradeMath.SpendLimit(0, 2644, true, 5000));
            Assert.Equal(int.MaxValue, TradeMath.SpendLimit(1000, 2644, false, long.MaxValue));
            Assert.Equal(2077, TradeMath.Budget(2610, 300, TradeMath.SpendLimit(1000, 2644, true, 2111), 34));
        }

        [Fact]
        public void What_an_order_buys_stops_at_the_purse_and_at_the_cargo_room()
        {
            var byWeight = new List<(int units, int cost, int profit, float hauls)>
            {
                (3, 1239, 774, 3f), (9, 1647, 2160, 18f), (9, 882, 1233, 9f)
            };
            var byDenar = new List<(int units, int cost, int profit, float hauls)>
            {
                (9, 882, 1233, 9f), (9, 1647, 2160, 18f), (3, 1239, 774, 3f)
            };
            Assert.Equal(774L + 960L + 137L, TradeMath.WhatThisOrderBuys(byWeight, 2077, -1f));
            Assert.Equal(516L + 137L, TradeMath.WhatThisOrderBuys(byWeight, 966, -1f));
            Assert.Equal(1233L, TradeMath.WhatThisOrderBuys(byDenar, 966, -1f));
            Assert.True(TradeMath.WhatThisOrderBuys(byDenar, 966, -1f) >
                        TradeMath.WhatThisOrderBuys(byWeight, 966, -1f));
            Assert.Equal(774L + 2160L + 1233L, TradeMath.WhatThisOrderBuys(byDenar, 100000, -1f));
            Assert.Equal(0L, TradeMath.WhatThisOrderBuys(byDenar, 0, -1f));
            Assert.Equal(0L, TradeMath.WhatThisOrderBuys(null, 500, -1f));
            Assert.Equal(0L, TradeMath.WhatThisOrderBuys(byDenar, 100000, 0f));
            Assert.Equal(1233L, TradeMath.WhatThisOrderBuys(byDenar, 100000, 9f));
            Assert.Equal(1233L + 2160L, TradeMath.WhatThisOrderBuys(byDenar, 100000, 27f));
        }

        [Fact]
        public void A_figure_priced_for_some_units_is_scaled_to_the_units_it_is_weighed_on()
        {
            Assert.Equal(10000, TradeMath.Scaled(2000, 100, 20));
            Assert.Equal(2000, TradeMath.Scaled(400, 100, 20));
            Assert.Equal(200, TradeMath.Scaled(400, 10, 20));
            Assert.Equal(400, TradeMath.Scaled(400, 20, 20));
            Assert.Equal(400, TradeMath.Scaled(400, 100, 0));
            Assert.Equal(-500, TradeMath.Scaled(-100, 10, 2));
            Assert.Equal(int.MaxValue, TradeMath.Scaled(int.MaxValue, 3, 2));
            Assert.Equal(int.MinValue, TradeMath.Scaled(int.MinValue, 3, 2));
        }

        [Fact]
        public void The_key_by_denar_carries_the_same_numerator_over_what_it_cost()
        {
            Assert.Equal(2160f * 0.2f / (1647f * 0.9f), TradeMath.KeyByDenar(2160, 0.2f, 1f, true, 1647, 0.9f), 4);
            Assert.Equal(2160f / (1647f * 0.9f), TradeMath.KeyByDenar(2160, 0.2f, 1f, false, 1647, 0.9f), 4);
            Assert.Equal(2160f * 0.2f * 0.8f / (1647f * 0.9f),
                         TradeMath.KeyByDenar(2160, 0.2f, 0.8f, true, 1647, 0.9f), 4);
            Assert.Equal(0f, TradeMath.KeyByDenar(2160, 1f, 1f, true, 0, 0.9f));
            Assert.Equal(0f, TradeMath.KeyByDenar(0, 1f, 1f, true, 1647, 0.9f));
            Assert.Equal(TradeMath.KeyByDenar(100, 1f, 1f, true, 500, 0.5f),
                         TradeMath.KeyByDenar(100, 1f, 1f, true, 500, 0.05f));
        }

        [Fact]
        public void A_real_ride_is_never_counted_as_quicker_than_the_rough_guess_it_replaces()
        {
            Assert.Equal(1.25f, TradeMath.HowMuchLongerTheRideIs(0.8f, 1.0f), 4);
            Assert.Equal(1f, TradeMath.HowMuchLongerTheRideIs(1.0f, 1.0f), 4);
            Assert.Equal(1f, TradeMath.HowMuchLongerTheRideIs(1.0f, 0.9f), 4);
            Assert.True(float.IsNaN(TradeMath.HowMuchLongerTheRideIs(0f, 1.0f)));
            Assert.True(float.IsNaN(TradeMath.HowMuchLongerTheRideIs(-1f, 1.0f)));
            Assert.True(float.IsNaN(TradeMath.HowMuchLongerTheRideIs(1.0f, 0f)));
            Assert.True(float.IsNaN(TradeMath.HowMuchLongerTheRideIs(1.0f, float.NaN)));
            Assert.True(float.IsNaN(TradeMath.HowMuchLongerTheRideIs(1.0f, float.PositiveInfinity)));
            Assert.True(float.IsNaN(TradeMath.HowMuchLongerTheRideIs(1f, TradeMath.LongestARideRunsOver * 2f)));
            Assert.Equal(TradeMath.LongestARideRunsOver,
                         TradeMath.HowMuchLongerTheRideIs(1f, TradeMath.LongestARideRunsOver), 4);
        }

        [Fact]
        public void The_stretch_taken_for_unwalked_pairs_is_the_upper_quartile_of_what_was_walked()
        {
            var walked = new List<float> { 1.02f, 1.05f, 1.10f, 1.18f, 1.25f, 1.31f, 1.44f, 1.60f };
            walked.Sort();
            float upper = TradeMath.Quantile(walked, 0.75f);
            float middle = TradeMath.Quantile(walked, 0.5f);
            Assert.True(upper > middle);
            Assert.Equal(1.3425f, upper, 4);
            Assert.True(upper >= 1f);
        }

        [Fact]
        public void A_profit_a_day_for_each_denar_is_the_profit_a_day_over_what_it_cost()
        {
            Assert.Equal(TradeMath.PerDay(960, 0.9f) / 732f, TradeMath.PerDenarADay(960, 732, 0.9f), 5);
            Assert.Equal(0f, TradeMath.PerDenarADay(960, 0, 0.9f));
        }

        [Fact]
        public void A_sell_market_no_price_could_make_clear_your_margin_is_passed_over_before_it_is_priced()
        {
            Assert.True(TradeMath.NoPriceCouldClearTheMargin(100, 60, 1f, 0.1f));
            Assert.False(TradeMath.NoPriceCouldClearTheMargin(100, 200, 1f, 0.1f));
            Assert.False(TradeMath.NoPriceCouldClearTheMargin(100, 0, 1f, 0.1f));
            Assert.False(TradeMath.NoPriceCouldClearTheMargin(100, 60, float.NaN, 0.1f));
            Assert.False(TradeMath.NoPriceCouldClearTheMargin(100, 60, -1f, 0.1f));
            Assert.True(TradeMath.NoPriceCouldClearTheMargin(0, 60, 1f, 0.1f));
        }

        [Fact]
        public void Passing_a_sell_market_over_on_the_most_a_price_could_reach_never_drops_one_its_real_price_would_keep()
        {
            int looked = 0, passed = 0;
            foreach (int openingBuy in new[] { 1, 40, 300, 2000 })
                foreach (int quotedSell in new[] { 1, 50, 310, 2400 })
                    foreach (float safety in new[] { 0.5f, 0.9f, 1f })
                        foreach (float margin in new[] { 0f, 0.1f, 0.5f })
                        {
                            looked++;
                            if (!TradeMath.NoPriceCouldClearTheMargin(openingBuy, quotedSell, safety, margin)) continue;
                            passed++;
                            int upper = TradeMath.MostAForecastCanReach(quotedSell);
                            for (int openingSell = 0; openingSell <= upper; openingSell += Math.Max(1, upper / 7))
                                Assert.False(TradeMath.BuyAcceptable(openingBuy,
                                                                     TradeMath.Realizable(openingSell, safety), margin));
                        }
            Assert.Equal(144, looked);
            Assert.Equal(PassedOverOnTheMargin, passed);
        }

        private const int PassedOverOnTheMargin = 72;

        [Fact]
        public void A_sell_market_the_shortest_road_could_not_make_pay_is_passed_over_before_its_road_is_walked()
        {
            Assert.True(TradeMath.NoRoadCouldMakeIt(100, 120, 4, 6f, true, 100f));
            Assert.False(TradeMath.NoRoadCouldMakeIt(100, 120, 4, 6f, true, 10f));
            Assert.False(TradeMath.NoRoadCouldMakeIt(100, 120, 4, 6f, false, 100f));
            Assert.False(TradeMath.NoRoadCouldMakeIt(100, 0, 4, 6f, true, 100f));
            Assert.False(TradeMath.NoRoadCouldMakeIt(100, 120, 0, 6f, true, 100f));
            Assert.False(TradeMath.NoRoadCouldMakeIt(400, 120, 4, 6f, true, 100f));
            Assert.False(TradeMath.NoRoadCouldMakeIt(100, 120, 4, float.NaN, true, 100f));
            Assert.False(TradeMath.NoRoadCouldMakeIt(100, 120, 4, -1f, true, 100f));
        }

        [Fact]
        public void A_sell_market_close_enough_to_beat_the_best_route_keeps_its_road_walked()
        {
            Assert.False(TradeMath.NoRoadCouldMakeIt(100, 400, 10, 0.5f, true, 1000f));
            Assert.True(TradeMath.NoRoadCouldMakeIt(100, 400, 10, 40f, true, 1000f));
        }

        [Fact]
        public void Passing_a_sell_market_over_on_the_shortest_road_never_drops_one_its_real_road_would_have_kept()
        {
            int looked = 0, passed = 0;
            foreach (int openingBuy in new[] { 1, 40, 300, 2000 })
                foreach (int quotedSell in new[] { 1, 50, 310, 2400 })
                    foreach (int stocked in new[] { 1, 3, 20, 400 })
                        foreach (float soonest in new[] { 0.01f, 0.4f, 2.5f, 19f })
                            foreach (float longer in new[] { 0f, 0.3f, 4f, 55f })
                                foreach (float bestKey in new[] { 0.01f, 7f, 180f, 9000f })
                                {
                                    looked++;
                                    if (!TradeMath.NoRoadCouldMakeIt(openingBuy, quotedSell, stocked, soonest,
                                                                     true, bestKey)) continue;
                                    passed++;
                                    float most = (float)(TradeMath.MostAForecastCanReach(quotedSell) - openingBuy) * stocked;
                                    Assert.True(TradeMath.PerDay(most, soonest + longer) <= bestKey);
                                }
            Assert.Equal(4096, looked);
            Assert.Equal(724, passed);
        }

        [Fact]
        public void A_face_and_a_weather_cell_are_kept_apart_in_one_region()
        {
            foreach (int face in new[] { -1, 0, 7, int.MaxValue })
                foreach (int cell in new[] { -1, 0, 1023, int.MaxValue })
                {
                    long region = TradeMath.RegionOf(face, cell);
                    Assert.Equal(face, TradeMath.FaceOfRegion(region));
                    Assert.Equal(cell, TradeMath.CellOfRegion(region));
                }
        }

        [Fact]
        public void A_leg_that_stays_on_one_face_is_looked_up_once_for_each_step()
        {
            int lookups = 0;
            List<TradeMath.Piece> pieces = PiecesAlong(10f, 1f, at => { lookups++; return 7; });
            Assert.Single(pieces);
            Assert.Equal(10f, pieces[0].Length, 4);
            Assert.Equal(11, lookups);
            Assert.Empty(PiecesAlong(0f, 1f, at => 7));
            Assert.Empty(PiecesAlong(float.NaN, 1f, at => 7));
            Assert.Single(PiecesAlong(0.004f, 1f, at => 7));
        }

        [Fact]
        public void A_party_whose_snow_slowdown_has_not_caught_up_yet_keeps_its_speed_until_the_next_face()
        {
            var road = RoadOf((Else, 1, 0, 10f), (Else, 2, 0, 20f));
            var sky = new SkyOf().Set(0, 0d, true, true);
            TradeMath.Pace pace = PaceOf(6f, snow: -0.125f, wet: -0.09f);
            pace.SnowAtStart = false;
            Assert.Equal(10d / 4.55d + 20d / 3.925d, HoursAlong(road, pace, sky, 4d, null, false), 3);
            Assert.Equal(30d / 3.925d, HoursAlong(road, pace, sky, 4d, null, true), 3);
        }

        [Fact]
        public void A_party_whose_speed_could_not_be_read_rides_at_one_pace_everywhere()
        {
            TradeMath.Pace pace = TradeMath.Pace.AllAt(5f, 8f);
            var road = RoadOf((Forest, 1, 0, 25f), (Sea, 2, 0, 40f));
            var sky = new SkyOf().Set(0, 0d, true, true);
            Assert.Equal(6d + 6d, HoursAlong(road, pace, sky, 20d), 3);
            Assert.Equal(5f, pace.BestOnLand);
        }

        [Fact]
        public void A_ride_into_the_night_slows_down_at_sunset()
        {
            var tally = new TradeMath.Tally();
            var road = RoadOf((Else, 1, 0, 20f));
            Assert.Equal(2d + 10d / 3.75d, HoursAlong(road, PaceOf(6f, night: -0.25f), null, 20d, tally), 3);
            Assert.Equal(10d / 3.75d, tally.Night, 3);
        }

        [Fact]
        public void A_ride_is_priced_at_the_speed_the_party_has_by_day()
        {
            Assert.Equal(5f, TradeMath.SpeedByDay(3.75f, 3.75f, 5f), 4);
            Assert.Equal(6f, TradeMath.SpeedByDay(4.5f, 3.75f, 5f), 4);
            Assert.Equal(4f, TradeMath.SpeedByDay(4f, 4f, 4f));
        }

        [Fact]
        public void A_ride_looked_at_by_night_is_slowed_only_until_sunrise()
        {
            var road = RoadOf((Else, 1, 0, 20f));
            Assert.Equal(2d + 12.5d / 5d, HoursAlong(road, PaceOf(6f, night: -0.25f), null, 0d), 3);
        }

        [Fact]
        public void A_ride_of_several_days_counts_every_night_on_the_way()
        {
            var tally = new TradeMath.Tally();
            var road = RoadOf((Else, 1, 0, 360f));
            Assert.Equal(75d, HoursAlong(road, PaceOf(6f, night: -0.25f), null, 2d, tally), 3);
            Assert.Equal(12d, tally.Night, 3);
        }

        [Fact]
        public void A_ride_set_off_at_a_clock_too_far_ahead_to_tell_the_hours_apart_keeps_one_light_the_whole_way()
        {
            var road = RoadOf((Else, 1, 0, 50f));
            TradeMath.Pace pace = PaceOf(6f, night: -0.25f);
            foreach (double start in new[] { 1e17d, 1e5d + float.MaxValue * 24d })
            {
                double hours = HoursAlong(road, pace, null, start);
                Assert.True(Math.Abs(hours - 10d) < 1e-3 || Math.Abs(hours - 50d / 3.75d) < 1e-3, hours.ToString());
            }
        }

        [Fact]
        public void A_ride_that_ends_before_sunset_is_all_at_the_day_speed()
        {
            var road = RoadOf((Else, 1, 0, 50f));
            Assert.Equal(10d, HoursAlong(road, PaceOf(6f, night: -0.25f), TradeMath.ClearSky.Always, 4d), 3);
        }

        [Fact]
        public void A_ride_that_starts_disorganized_is_slowed_only_while_it_lasts()
        {
            var tally = new TradeMath.Tally();
            var road = RoadOf((Else, 1, 0, 20f));
            TradeMath.Pace pace = PaceOf(6f, night: -0.25f, slow: -0.4f, slowHours: 2d);
            Assert.Equal(2d + 14d / 5d, HoursAlong(road, pace, null, 4d, tally), 3);
            Assert.Equal(2d, tally.Slow, 3);
        }

        [Fact]
        public void A_second_leg_starts_with_only_what_is_left_of_the_disorganized_hours()
        {
            TradeMath.Pace pace = PaceOf(6f, slow: -0.4f, slowHours: 5d);
            Assert.Same(pace, pace.After(0d));
            Assert.Equal(2d, pace.After(3d).SlowHours, 6);
            Assert.Equal(0d, pace.After(9d).SlowHours);
            Assert.Equal(5d, pace.SlowHours);
            Assert.Same(pace.OnGround, pace.After(3d).OnGround);
            Assert.Equal(6d, pace.After(-1d).SlowHours, 6);
            Assert.Same(pace, pace.After(double.NaN));
            TradeMath.Pace calm = PaceOf(6f);
            Assert.Same(calm, calm.After(3d));
        }

        [Fact]
        public void A_weather_turn_handed_to_a_ride_is_always_found_again_at_the_moment_it_was_handed()
        {
            var at = new List<double> { double.NegativeInfinity, 0.1d, 0.7d, 4.1d, 8.1d };
            foreach (double from in new[] { 0d, 0.3d, 1e-7d, 1234.567d, -2.2d })
                for (int k = 1; k < at.Count; k++)
                {
                    double handed = at[k] - from;
                    Assert.Equal(k, TradeMath.LastAtOrBefore(at, from, handed));
                    Assert.Equal(k - 1, TradeMath.LastAtOrBefore(at, from, handed - 1e-6d));
                }
        }

        [Fact]
        public void Every_face_a_straight_leg_crosses_is_found_to_within_a_hundredth_of_a_step()
        {
            long RegionAt(float at) => at < 2.5f ? 1 : at < 2.53f ? 2 : at < 7f ? 3 : 4;
            List<TradeMath.Piece> pieces = PiecesAlong(10f, 1f, RegionAt);
            Assert.Equal(new long[] { 1, 2, 3, 4 }, pieces.ConvertAll(p => p.Region).ToArray());
            Assert.Equal(2.5f, pieces[0].Length, 2);
            Assert.Equal(0.03f, pieces[1].Length, 2);
            Assert.Equal(4.47f, pieces[2].Length, 2);
            Assert.Equal(3f, pieces[3].Length, 2);
            float sum = 0f;
            foreach (TradeMath.Piece piece in pieces) sum += piece.Length;
            Assert.Equal(10f, sum, 4);
        }

        [Fact]
        public void Every_ride_hands_back_a_real_number_whatever_it_is_handed()
        {
            float[] odd = { float.NaN, float.PositiveInfinity, float.NegativeInfinity, -3f, 0f, 1e-30f, 5f };
            foreach (float a in odd)
                foreach (float b in odd)
                {
                    TradeMath.Pace pace = PaceOf(a, night: b, wet: b, snow: a, slow: b, slowHours: a);
                    pace.Sea = b;
                    var road = RoadOf((Else, 1, 0, 10f), (Sea, 2, 1, 5f));
                    road.Add(new TradeMath.Run { Length = a, Ground = Else, Face = 3, Cell = 1 });
                    foreach (double start in new[] { double.NaN, double.PositiveInfinity, -7d, 23.99d })
                    {
                        float days = TradeMath.DaysAlong(road, pace, new SkyOf().Set(0, 1d, true, true), start, 2, 22,
                                                         false, new TradeMath.Tally());
                        Assert.False(float.IsNaN(days));
                        Assert.True(days >= 0f);
                        Assert.True(days == TradeMath.FurthestThereIs || !TradeMath.OutOfReach(days));
                    }
                }
            Assert.Equal(0f, TradeMath.DaysAlong(null, PaceOf(5f), null, 3d, 2, 22, true, null));
            Assert.Equal(0f, TradeMath.DaysAlong(RoadOf((Else, 1, 0, 5f)), null, null, 3d, 2, 22, true, null));
        }

        [Fact]
        public void Night_falls_at_sunset_and_lifts_at_sunrise_as_the_game_has_it()
        {
            Assert.False(TradeMath.DarkAt(21.99, 2, 22));
            Assert.True(TradeMath.DarkAt(22, 2, 22));
            Assert.True(TradeMath.DarkAt(1.99, 2, 22));
            Assert.False(TradeMath.DarkAt(2, 2, 22));
            Assert.True(TradeMath.DarkAt(46, 2, 22));
            Assert.True(TradeMath.DarkAt(10, 22, 2));
            Assert.False(TradeMath.DarkAt(10, 0, 24));
            Assert.Equal(22d, TradeMath.NextTurnOfLight(10, 2, 22));
            Assert.Equal(26d, TradeMath.NextTurnOfLight(22, 2, 22));
            Assert.Equal(2d, TradeMath.NextTurnOfLight(1, 2, 22));
            Assert.Equal(26d, TradeMath.NextTurnOfLight(23.5, 2, 22));
            Assert.Equal(double.PositiveInfinity, TradeMath.NextTurnOfLight(10, 0, 24));
            Assert.Equal(double.PositiveInfinity, TradeMath.NextTurnOfLight(10, 22, 2));
        }

        [Fact]
        public void Rain_that_falls_on_the_road_slows_a_mounted_party_from_the_moment_it_falls()
        {
            var tally = new TradeMath.Tally();
            var road = RoadOf((Else, 1, 0, 30f));
            var sky = new SkyOf().Set(0, 2d, true, false);
            Assert.Equal(2d + 20d / 4.55d, HoursAlong(road, PaceOf(6f, wet: -0.09f), sky, 4d, tally), 3);
            Assert.Equal(20d / 4.55d, tally.Wet, 3);
        }

        [Fact]
        public void Riding_into_a_snowy_stretch_on_the_same_wet_ground_waits_for_the_next_face()
        {
            var road = RoadOf((Else, 1, 0, 10f), (Else, 1, 1, 5f), (Else, 2, 1, 10f));
            var sky = new SkyOf().Set(0, 0d, true, false).Set(1, 0d, true, true);
            TradeMath.Pace pace = PaceOf(6f, snow: -0.125f, wet: -0.09f);
            Assert.Equal(15d / 4.55d + 10d / 3.925d, HoursAlong(road, pace, sky, 4d), 3);
        }

        [Fact]
        public void Riding_out_of_the_rain_lifts_the_wet_ground_slowdown_at_the_edge_of_the_rain()
        {
            var tally = new TradeMath.Tally();
            var road = RoadOf((Else, 1, 0, 9.1f), (Else, 1, 1, 10f));
            var sky = new SkyOf().Set(0, 0d, true, false);
            Assert.Equal(2d + 2d, HoursAlong(road, PaceOf(6f, wet: -0.09f), sky, 4d, tally), 3);
            Assert.Equal(2d, tally.Wet, 3);
        }

        [Fact]
        public void Snow_that_falls_on_dry_ground_slows_the_party_at_once()
        {
            var road = RoadOf((Else, 1, 0, 10f), (Else, 2, 0, 20f));
            var sky = new SkyOf().Set(0, 1d, true, true);
            TradeMath.Pace pace = PaceOf(6f, snow: -0.125f, wet: -0.09f);
            Assert.Equal(1d + 25d / 3.925d, HoursAlong(road, pace, sky, 4d), 3);
        }

        [Fact]
        public void Snow_that_falls_on_ground_already_wet_slows_the_party_from_the_next_face_as_the_game_does()
        {
            var road = RoadOf((Else, 1, 0, 10f), (Else, 2, 0, 20f));
            var sky = new SkyOf().Set(0, 0d, true, false).Set(0, 1d, true, true);
            TradeMath.Pace pace = PaceOf(6f, snow: -0.125f, wet: -0.09f);
            Assert.Equal(10d / 4.55d + 20d / 3.925d, HoursAlong(road, pace, sky, 4d), 3);
        }

        [Fact]
        public void The_forest_slows_the_party_down_only_on_the_stretch_that_runs_through_it()
        {
            var road = RoadOf((Plain, 1, 0, 10f), (Forest, 2, 0, 12f), (Plain, 3, 0, 5f));
            Assert.Equal(2d + 3d + 1d, HoursAlong(road, PaceOf(6f, forest: -0.2f), null, 4d), 3);
        }

        [Fact]
        public void The_ground_on_a_road_is_kept_in_order_with_each_run_of_one_kind_joined_up()
        {
            var road = RoadOf((Plain, 1, 0, 1f), (Plain, 1, 0, 0.5f), (Forest, 2, 0, 2f), (Forest, 2, 1, 1f),
                              (Plain, 3, 1, 0f), (Plain, 3, 1, float.NaN), (Plain, 3, 1, 4f));
            Assert.Equal(4, road.Count);
            Assert.Equal(1.5f, road[0].Length);
            Assert.Equal(TradeMath.GroundForest, road[1].Ground);
            Assert.Equal(1, road[2].Cell);
            Assert.Equal(5.5f, TradeMath.LengthOn(road, TradeMath.GroundPlain));
            Assert.Equal(3f, TradeMath.LengthOn(road, TradeMath.GroundForest));
            Assert.Equal(8.5f, TradeMath.LengthOn(road, -1));
            Assert.Equal(0f, TradeMath.LengthOn(null, -1));
        }

        [Fact]
        public void The_quick_estimate_never_says_a_road_takes_longer_than_riding_it_does()
        {
            TradeMath.Pace pace = PaceOf(6f, night: -0.25f, forest: -0.2f, snow: -0.125f, wet: -0.09f, slow: -0.4f,
                                         slowHours: 3d);
            pace.OnGround[TradeMath.GroundPlain] = 0.05f;
            pace.Day = 0.02f;
            pace.Sea = 4f;
            var sky = new SkyOf().Set(0, 0d, true, false).Set(0, 5d, true, true).Set(1, 2d, false, false);
            var roads = new[]
            {
                RoadOf((Plain, 1, 0, 40f)),
                RoadOf((Else, 1, 0, 10f), (Forest, 2, 1, 8f), (Else, 3, 1, 20f)),
                RoadOf((Forest, 1, 1, 200f), (Sea, 2, 1, 50f))
            };
            foreach (var road in roads)
                for (int start = 0; start < 24; start += 3)
                {
                    float quick = TradeMath.DaysAtBestSpeed(TradeMath.LengthOn(road, -1), pace.BestOnLand, pace.Sea);
                    Assert.True(quick <= TradeMath.DaysAlong(road, pace, sky, start, 2, 22, true, null) + 1e-5f);
                }
        }

        [Fact]
        public void The_sea_is_crossed_at_the_fleet_speed_whatever_the_hour()
        {
            TradeMath.Pace pace = PaceOf(6f, night: -0.25f);
            pace.Sea = 9f;
            var road = RoadOf((Sea, TradeMath.Unread, TradeMath.Unread, 30f));
            Assert.Equal(4d, HoursAlong(road, pace, null, 21d), 3);
        }

        [Fact]
        public void The_speed_by_day_never_slows_a_party_down_or_trusts_a_reading_that_is_not_a_number()
        {
            Assert.Equal(4f, TradeMath.SpeedByDay(4f, 4f, 3f));
            Assert.Equal(4f, TradeMath.SpeedByDay(4f, 0f, 5f));
            Assert.Equal(4f, TradeMath.SpeedByDay(4f, 3.75f, float.NaN));
            Assert.Equal(4f, TradeMath.SpeedByDay(4f, 3.75f, float.PositiveInfinity));
            Assert.Equal(0f, TradeMath.SpeedByDay(0f, 3.75f, 5f));
            Assert.True(float.IsNaN(TradeMath.SpeedByDay(float.NaN, 3.75f, 5f)));
        }

        [Fact]
        public void The_weather_comes_round_to_each_place_in_the_order_the_game_updates_it()
        {
            Assert.Equal(0, TradeMath.TurnsUntil(5, 5, 1024));
            Assert.Equal(3, TradeMath.TurnsUntil(8, 5, 1024));
            Assert.Equal(1021, TradeMath.TurnsUntil(2, 5, 1024));
            Assert.Equal(0, TradeMath.TurnsUntil(2, 5, 0));
            var at = new List<double> { 0d, 4d, 8d };
            Assert.Equal(-1, TradeMath.LastAtOrBefore(at, 0d, -0.5d));
            Assert.Equal(0, TradeMath.LastAtOrBefore(at, 0d, 0d));
            Assert.Equal(1, TradeMath.LastAtOrBefore(at, 0d, 4d));
            Assert.Equal(1, TradeMath.LastAtOrBefore(at, 0d, 7.9d));
            Assert.Equal(2, TradeMath.LastAtOrBefore(at, 0d, 100d));
            Assert.Equal(-1, TradeMath.LastAtOrBefore(at, 0d, double.NaN));
            Assert.Equal(-1, TradeMath.LastAtOrBefore(new List<double>(), 0d, 3d));
            Assert.Equal(2, TradeMath.LastAtOrBefore(at, 3d, 5d));
            Assert.Equal(0, TradeMath.LastAtOrBefore(new List<double> { double.NegativeInfinity, 4d }, 2.5d, -9d));
        }

        [Fact]
        public void Where_the_night_falls_on_the_road_changes_how_long_the_ride_takes()
        {
            TradeMath.Pace pace = PaceOf(6f, night: -0.25f, forest: -0.2f);
            var there = RoadOf((Plain, 1, 0, 10f), (Forest, 2, 0, 8f), (Plain, 3, 0, 20f));
            var back = RoadOf((Plain, 3, 0, 20f), (Forest, 2, 0, 8f), (Plain, 1, 0, 10f));
            Assert.Equal(2d + 8d / 2.75d + (4d - 8d / 2.75d) + (20d - (4d - 8d / 2.75d) * 3.75d) / 5d,
                         HoursAlong(there, pace, null, 20d), 3);
            Assert.Equal(2d + 10d / 3.75d + (4d - 10d / 3.75d) + (8d - (4d - 10d / 3.75d) * 2.75d) / 4d + 2d,
                         HoursAlong(back, pace, null, 20d), 3);
        }

        private const int Else = TradeMath.GroundElse;

        private static double HoursAlong(List<TradeMath.Run> road, TradeMath.Pace pace, TradeMath.ISky sky,
                                         double start, TradeMath.Tally tally = null, bool fresh = true) =>
            TradeMath.DaysAlong(road, pace, sky, start, 2, 22, fresh, tally) * 24d;

        private const int Plain = TradeMath.GroundPlain;

        private sealed class SkyOf : TradeMath.ISky
        {
            private readonly Dictionary<int, List<(double At, bool Wet, bool Snowy)>> _cells =
                new Dictionary<int, List<(double, bool, bool)>>();

            public SkyOf Set(int cell, double at, bool wet, bool snowy)
            {
                if (!_cells.TryGetValue(cell, out var turns)) _cells[cell] = turns = new List<(double, bool, bool)>();
                turns.Add((at, wet, snowy));
                turns.Sort((a, b) => a.At.CompareTo(b.At));
                return this;
            }

            public void Read(int cell, double hours, out bool wet, out bool snowy, out double changes)
            {
                wet = false;
                snowy = false;
                changes = double.PositiveInfinity;
                if (!_cells.TryGetValue(cell, out var turns)) return;
                foreach (var (at, w, s) in turns)
                {
                    if (at <= hours)
                    {
                        wet = w;
                        snowy = s;
                        continue;
                    }
                    changes = at;
                    break;
                }
            }
        }

        private const int Forest = TradeMath.GroundForest;

        private const int Sea = TradeMath.GroundSea;

        private static TradeMath.Pace PaceOf(float speed, float night = 0f, float forest = 0f, float snow = 0f,
                                             float wet = 0f, float slow = 0f, double slowHours = 0d)
        {
            var pace = new TradeMath.Pace
            {
                Base = speed, Night = night, Snow = snow, Wet = wet, Slow = slow, SlowHours = slowHours, Sea = speed
            };
            pace.OnGround[TradeMath.GroundForest] = forest;
            return pace;
        }

        private static List<TradeMath.Run> RoadOf(params (int ground, int face, int cell, float length)[] runs)
        {
            var road = new List<TradeMath.Run>();
            foreach (var (ground, face, cell, length) in runs) TradeMath.AddRun(road, ground, face, cell, length);
            return road;
        }

        private static List<TradeMath.Piece> PiecesAlong(float length, float stride, Func<float, long> regionAt)
        {
            var pieces = new List<TradeMath.Piece>();
            TradeMath.RegionsAlong(length, stride, regionAt, pieces);
            return pieces;
        }

        private static (float x, float y) Heading(float degrees)
        {
            double r = degrees * Math.PI / 180d;
            return ((float)-Math.Sin(r), (float)Math.Cos(r));
        }

        [Fact]
        public void The_wind_on_the_map_is_read_the_way_War_Sails_reads_it_with_no_storm()
        {
            TradeMath.WindOnTheMap(0f, 0f, out float x, out float y);
            Assert.Equal(0f, x);
            Assert.Equal(1f / 15f, y, 6);
            TradeMath.WindOnTheMap(0.01f, 0f, out x, out y);
            Assert.Equal(1f / 15f, x, 6);
            Assert.Equal(0f, y, 6);
            TradeMath.WindOnTheMap(0f, -2f, out x, out y);
            Assert.Equal(0f, x, 6);
            Assert.Equal(-0.46f, y, 6);
            TradeMath.WindOnTheMap(0.18f, 0.24f, out x, out y);
            Assert.Equal(0.18f, x, 5);
            Assert.Equal(0.24f, y, 5);
        }

        [Fact]
        public void A_wind_behind_the_ships_pushes_them_by_up_to_one_and_a_half_times_its_strength()
        {
            var (bx, by) = Heading(30f);
            var (wx, wy) = Heading(30f);
            Assert.Equal(0.46f * 1.5f, TradeMath.WindBehind(bx, by, wx * 0.46f, wy * 0.46f, out bool astern), 4);
            Assert.True(astern);
            var (sx, sy) = Heading(90f);
            Assert.Equal(0.46f * 0.5f * 1.5f, TradeMath.WindBehind(bx, by, sx * 0.46f, sy * 0.46f, out astern), 4);
            Assert.True(astern);
            var (hx, hy) = Heading(150f);
            Assert.Equal(0f, TradeMath.WindBehind(bx, by, hx * 0.46f, hy * 0.46f, out astern));
            Assert.False(astern);
            Assert.Equal(0f, TradeMath.WindBehind(bx, by, 0f, 0f, out astern));
            Assert.False(astern);
            Assert.Equal(0f, TradeMath.WindBehind(bx, by, float.NaN, 0f, out astern));
            Assert.False(astern);
        }

        [Fact]
        public void The_angle_to_the_wind_is_not_wrapped_round_just_as_War_Sails_leaves_it()
        {
            var (bx, by) = Heading(170f);
            var (wx, wy) = Heading(-170f);
            Assert.Equal(0f, TradeMath.WindBehind(bx, by, wx * 0.3f, wy * 0.3f, out bool astern));
            Assert.False(astern);
            var (cx, cy) = Heading(-10f);
            var (dx, dy) = Heading(10f);
            Assert.True(TradeMath.WindBehind(cx, cy, dx * 0.3f, dy * 0.3f, out astern) > 0f);
            Assert.True(astern);
        }

        private static TradeMath.Sail SailOf(float push, float fair, int water = TradeMath.WaterUnread) =>
            new TradeMath.Sail { Push = push, Fair = fair, Water = water };

        private static TradeMath.Pace ShipsOf(float sea)
        {
            var pace = new TradeMath.Pace { Sea = sea, SeaRest = -0.1f, SeaAdd = -0.3f, SeaSnow = -0.125f, SeaSlow = -0.4f };
            pace.SeaLeast = 1f;
            pace.OnWater[TradeMath.WaterOpen] = 0.448f;
            pace.OnWater[TradeMath.WaterRiver] = 0.5f + 0.02f;
            pace.OnWater[TradeMath.WaterCoast] = 0.02f;
            pace.AddOnWater[TradeMath.WaterOpen] = -0.2f;
            pace.AddOnWater[TradeMath.WaterElse] = -0.2f;
            pace.AddOnWater[TradeMath.WaterCoast] = 0.2f;
            pace.AddOnWater[TradeMath.WaterRiver] = 0.2f;
            return pace;
        }

        [Fact]
        public void Ships_sail_faster_with_the_wind_Fair_Winds_and_Shock_and_Awe_but_never_past_ten()
        {
            var pace = new TradeMath.Pace { Sea = 4f };
            Assert.Equal(4f, pace.AtSea(SailOf(0f, 0f), false, false));
            Assert.Equal(6f, pace.AtSea(SailOf(0.5f, 0f), false, false), 4);
            pace.FairWinds = 0.1f;
            Assert.Equal(6.4f, pace.AtSea(SailOf(0.5f, 1f), false, false), 4);
            Assert.Equal(6.2f, pace.AtSea(SailOf(0.5f, 0.5f), false, false), 4);
            Assert.Equal(4f, pace.AtSea(SailOf(0f, 0f), false, false));
            pace.ShockAndAwe = 0.1f;
            Assert.Equal(4.4f, pace.AtSea(SailOf(0f, 0f), false, false), 4);
            Assert.Equal(6.4f, pace.AtSea(SailOf(0.5f, 1f), false, false), 4);
            Assert.Equal(6.4f, pace.AtSea(SailOf(0.5f, 0.5f), false, false), 4);
            Assert.Equal(4f, pace.AtSea(TradeMath.WaterUnread, 0f, false, false));
            Assert.Equal(4f, new TradeMath.Pace { Sea = 4f }.AtSea(SailOf(float.NaN, float.NaN), false, false));
            Assert.Equal(10f, new TradeMath.Pace { Sea = 8f }.AtSea(SailOf(0.69f, 0f), false, false), 4);
            Assert.Equal(10f, new TradeMath.Pace { Sea = 12f }.AtSea(SailOf(0.5f, 0f), false, false), 4);
            Assert.Equal(0f, new TradeMath.Pace { Sea = 0f }.AtSea(SailOf(0.5f, 0f), false, false));
        }

        [Fact]
        public void The_ships_speed_at_sea_is_worked_out_the_way_War_Sails_works_it_out()
        {
            TradeMath.Pace pace = ShipsOf(4f);
            Assert.Equal((4f - 0.3f - 0.2f) * (1f - 0.1f + 0.448f) * 1.25f,
                         pace.AtSea(SailOf(0.25f, 1f, TradeMath.WaterOpen), false, false), 4);
            Assert.Equal((4f - 0.3f + 0.2f) * (1f - 0.1f + 0.52f), pace.AtSea(TradeMath.WaterRiver, 0f, false, false), 4);
            Assert.Equal((4f - 0.3f + 0.2f) * (1f - 0.1f + 0.02f - 0.125f - 0.4f),
                         pace.AtSea(TradeMath.WaterCoast, 0f, true, true), 4);
            Assert.Equal((4f - 0.3f - 0.2f) * (1f - 0.1f), pace.AtSea(TradeMath.WaterElse, 0f, false, false), 4);
            Assert.Equal((4f - 0.3f) * (1f - 0.1f), pace.Afloat, 4);
            Assert.Equal(pace.Afloat, pace.On(Sea, true, false, true, false), 4);
            Assert.Equal((4f - 0.3f) * (1f - 0.1f - 0.4f), pace.On(Sea, false, false, false, true), 4);
            Assert.Equal(1f, ShipsOf(0.5f).AtSea(TradeMath.WaterCoast, 0f, true, true));
            Assert.Equal(10f, ShipsOf(9f).AtSea(SailOf(0.69f, 1f, TradeMath.WaterOpen), false, false));
            Assert.Equal(pace.Afloat, pace.AtSea(99, 0f, false, false));
        }

        [Fact]
        public void The_crew_the_cargo_and_the_size_of_the_fleet_slow_the_ships_as_War_Sails_has_it()
        {
            Assert.Equal(-0.1f, TradeMath.Undermanned(30, 40), 5);
            Assert.Equal(0f, TradeMath.Undermanned(40, 40));
            Assert.Equal(0f, TradeMath.Undermanned(5, 0));
            Assert.Equal(-0.4f, TradeMath.Undermanned(0, 40), 5);
            Assert.Equal(1f / 1.2f - 1f, TradeMath.Overmanned(60, 50), 5);
            Assert.Equal(0f, TradeMath.Overmanned(50, 50));
            Assert.Equal(-1f, TradeMath.Overmanned(10, 0));
            Assert.Equal(0f, TradeMath.OverFleetSize(3, 1f));
            Assert.Equal(-0.2f / (1f + (float)Math.Exp(1d)), TradeMath.OverFleetSize(4, 1f), 5);
            Assert.Equal(-0.1f, TradeMath.OverFleetSize(6, 1f), 5);
            Assert.Equal(-0.2f / (1f + (float)Math.Exp(-1.5d)) * 0.7f * 0.7f, TradeMath.OverFleetSize(9, 0.49f), 5);
            Assert.Equal(-0.4f, TradeMath.Overburdened(150f, 100, 0.8f), 5);
            Assert.Equal(0f, TradeMath.Overburdened(100f, 100, 0.8f));
            Assert.Equal(0f, TradeMath.Overburdened(100f, 0, 1f));
            Assert.Equal(0.165f, TradeMath.ShallowDraft(10f, 4, true), 5);
            Assert.Equal(-0.165f, TradeMath.ShallowDraft(10f, 4, false), 5);
            Assert.Equal(0f, TradeMath.ShallowDraft(10f, 0, true));
            Assert.True(TradeMath.Coastal(TradeMath.WaterCoast) && TradeMath.Coastal(TradeMath.WaterRiver));
            Assert.False(TradeMath.Coastal(TradeMath.WaterOpen) || TradeMath.Coastal(TradeMath.WaterElse) ||
                         TradeMath.Coastal(TradeMath.WaterUnread));
        }

        [Fact]
        public void No_wind_or_water_on_the_map_takes_ships_past_the_soonest_a_ride_at_sea_is_counted_for()
        {
            var paces = new[]
            {
                new TradeMath.Pace { Sea = 4f },
                new TradeMath.Pace { Sea = 4f, FairWinds = 0.1f },
                new TradeMath.Pace { Sea = 4f, ShockAndAwe = 0.1f },
                new TradeMath.Pace { Sea = 4f, FairWinds = 0.1f, ShockAndAwe = 0.1f },
                new TradeMath.Pace { Sea = 4f, ShockAndAwe = 0.9f },
                ShipsOf(4f),
                ShipsOf(2f)
            };
            paces[6].FairWinds = 0.1f;
            Assert.Equal(4f * (1f + 0.46f * 1.5f + 0.1f), paces[1].BestAtSea, 4);
            Assert.Equal(4f * 1.9f, paces[4].BestAtSea, 4);
            Assert.Equal((4f - 0.3f + 0.2f) * (1f - 0.1f + 0.52f) * 1.69f, paces[5].BestAtSea, 4);
            foreach (TradeMath.Pace pace in paces)
            {
                Assert.True(pace.Afloat <= pace.BestAtSea);
                for (float flow = 0f; flow <= 1f; flow += 0.1f)
                    for (float wind = -180f; wind < 180f; wind += 10f)
                        for (float bearing = -180f; bearing < 180f; bearing += 10f)
                        {
                            var (wx, wy) = Heading(wind);
                            TradeMath.WindOnTheMap(wx * flow, wy * flow, out float x, out float y);
                            var (bx, by) = Heading(bearing);
                            float push = TradeMath.WindBehind(bx, by, x, y, out bool astern);
                            for (int water = 0; water < TradeMath.Waters; water++)
                                for (int c = 0; c < 4; c++)
                                    Assert.True(pace.AtSea(SailOf(push, astern ? 1f : 0f, water), (c & 1) != 0,
                                                           (c & 2) != 0) <= pace.BestAtSea + 1E-04f);
                        }
            }
            Assert.Equal(10f, new TradeMath.Pace { Sea = 7f, FairWinds = 0.1f }.BestAtSea, 4);
        }

        [Fact]
        public void Sea_stretches_joined_into_one_run_take_as_long_as_they_did_apart()
        {
            var runs = new List<TradeMath.Run>();
            var sails = new List<TradeMath.Sail>();
            TradeMath.AddRunAtSea(runs, sails, 7, 3, TradeMath.WaterOpen, 10f, 0f, 0f);
            TradeMath.AddRunAtSea(runs, sails, 7, 3, TradeMath.WaterOpen, 10f, 1f, 1f);
            Assert.Single(runs);
            Assert.Equal(Sea, runs[0].Ground);
            Assert.Equal(7, runs[0].Face);
            Assert.Equal(3, runs[0].Cell);
            Assert.Equal(20f, runs[0].Length, 4);
            Assert.Equal(1f / 3f, sails[0].Push, 4);
            Assert.Equal(0.5f, sails[0].Fair, 4);
            Assert.Equal(TradeMath.WaterOpen, sails[0].Water);
            TradeMath.AddRunAtSea(runs, sails, 7, 3, TradeMath.WaterOpen, 5f, 0.2f, 1f);
            TradeMath.AddRunAtSea(runs, sails, 7, 3, TradeMath.WaterOpen, 0f, 9f, 1f);
            TradeMath.AddRunAtSea(runs, sails, 7, 3, TradeMath.WaterOpen, float.NaN, 9f, 1f);
            TradeMath.AddRunAtSea(runs, sails, 7, 3, TradeMath.WaterOpen, float.PositiveInfinity, 9f, 1f);
            Assert.Single(runs);
            Assert.Single(sails);
            Assert.Equal(25f, runs[0].Length, 4);
            Assert.Equal(0.6f, sails[0].Fair, 4);
            TradeMath.Pace pace = ShipsOf(4f);
            pace.Even = true;
            pace.Base = 5f;
            double Hours(List<TradeMath.Run> road, IList<TradeMath.Sail> on) =>
                TradeMath.DaysAlong(road, pace, null, 3d, 2, 22, true, null, on) * 24d;
            double apart = Hours(RoadOf((Sea, 7, 3, 10f)), new[] { SailOf(0f, 0f, TradeMath.WaterOpen) }) +
                           Hours(RoadOf((Sea, 7, 3, 10f)), new[] { SailOf(1f, 1f, TradeMath.WaterOpen) }) +
                           Hours(RoadOf((Sea, 7, 3, 5f)), new[] { SailOf(0.2f, 1f, TradeMath.WaterOpen) });
            Assert.Equal(apart, Hours(runs, sails), 3);
            TradeMath.AddRunAtSea(runs, sails, 8, 3, TradeMath.WaterOpen, 5f, 0f, 0f);
            TradeMath.AddRunAtSea(runs, sails, 8, 4, TradeMath.WaterOpen, 5f, 0f, 0f);
            TradeMath.AddRunAtSea(runs, sails, 8, 4, TradeMath.WaterCoast, 5f, 0f, 0f);
            Assert.Equal(4, runs.Count);
            Assert.Equal(4, sails.Count);
            Assert.Equal(TradeMath.WaterCoast, sails[3].Water);
        }

        [Fact]
        public void A_sea_leg_with_the_wind_behind_takes_less_time_and_one_without_takes_what_it_did()
        {
            List<TradeMath.Run> road = RoadOf((Sea, 4, 0, 60f));
            var pace = PaceOf(4f);
            float calm = TradeMath.DaysAlong(road, pace, null, 3d, 2, 22, true, null);
            Assert.Equal(calm, TradeMath.DaysAlong(road, pace, null, 3d, 2, 22, true, null, new[] { SailOf(0f, 1f) }));
            float pushed = TradeMath.DaysAlong(road, pace, null, 3d, 2, 22, true, null, new[] { SailOf(0.5f, 1f) });
            Assert.Equal(calm / 1.5f, pushed, 4);
            pace.FairWinds = 0.1f;
            Assert.Equal(calm / 1.6f, TradeMath.DaysAlong(road, pace, null, 3d, 2, 22, true, null, new[] { SailOf(0.5f, 1f) }), 4);
            var even = TradeMath.Pace.AllAt(5f, 4f);
            Assert.Equal(TradeMath.DaysAtSpeed(60f, 0f, 5f, 4f),
                         TradeMath.DaysAlong(road, even, null, 3d, 2, 22, true, null, new[] { SailOf(0f, 1f) }), 5);
            Assert.Equal(TradeMath.DaysAtSpeed(60f, 0f, 5f, 4f) / 1.5f,
                         TradeMath.DaysAlong(road, even, null, 3d, 2, 22, true, null, new[] { SailOf(0.5f, 1f) }), 5);
            Assert.Equal(TradeMath.FurthestThereIs,
                         TradeMath.DaysAlong(road, TradeMath.Pace.AllAt(5f, 0f), null, 3d, 2, 22, true, null,
                                             new[] { SailOf(0f, 1f) }));
        }

        [Fact]
        public void Ships_sail_Disorganized_only_while_it_lasts_and_slower_in_snow()
        {
            List<TradeMath.Run> road = RoadOf((Sea, 4, 9, 60f));
            TradeMath.Pace pace = ShipsOf(4f);
            TradeMath.Sail[] coast = { SailOf(0f, 1f, TradeMath.WaterCoast) };
            float fast = pace.AtSea(TradeMath.WaterCoast, 0f, false, false);
            float slow = pace.AtSea(TradeMath.WaterCoast, 0f, false, true);
            Assert.Equal(60f * 1.2f / fast, TradeMath.DaysAlong(road, pace, null, 3d, 2, 22, true, null, coast) * 24f, 3);
            pace.SlowHours = 2d;
            float covered = slow * 2f / 1.2f;
            Assert.Equal(2f + (60f - covered) * 1.2f / fast,
                         TradeMath.DaysAlong(road, pace, null, 3d, 2, 22, true, null, coast) * 24f, 3);
            pace.SlowHours = 0d;
            SkyOf sky = new SkyOf().Set(9, 0d, false, true);
            Assert.Equal(60f * 1.2f / pace.AtSea(TradeMath.WaterCoast, 0f, true, false),
                         TradeMath.DaysAlong(road, pace, sky, 3d, 2, 22, true, null, coast) * 24f, 3);
        }

        [Fact]
        public void The_night_step_is_read_from_the_pace_the_game_gives_either_side_of_the_turn()
        {
            Assert.Equal(-0.25f, TradeMath.NightStepFrom(5f, 3.75f, -0.25f, true), 4);
            Assert.Equal(-0.25f, TradeMath.NightStepFrom(3.75f, 5f, 0f, false), 4);
            Assert.Equal(-0.15f, TradeMath.NightStepFrom(6f, 5.1f, -0.15f, true), 4);
            Assert.True(float.IsNaN(TradeMath.NightStepFrom(0f, 5f, 0f, false)));
            Assert.True(TradeMath.LikeAStep(-0.25f));
            Assert.False(TradeMath.LikeAStep(-0.95f));
            Assert.False(TradeMath.LikeAStep(float.NaN));
            Assert.Equal(3.75f, TradeMath.PaceAfterTheTurn(5f, -0.25f, 3.75f, -0.25f, true), 4);
            Assert.Equal(5f, TradeMath.PaceAfterTheTurn(3.75f, 0f, 5f, -0.25f, false), 4);
            Assert.Equal(4f, TradeMath.PaceAfterTheTurn(5f, -0.25f, 3.75f, -0.2f, true), 4);
            Assert.True(TradeMath.ClearOfLimits(5f, 1f, 100f));
            Assert.False(TradeMath.ClearOfLimits(1f, 1f, 100f));
        }

        [Fact]
        public void A_ride_counts_the_hours_it_spent_on_each_kind_of_ground()
        {
            var tally = new TradeMath.Tally();
            var road = RoadOf((Plain, 1, 0, 6f), (Forest, 2, 0, 6f), (TradeMath.GroundDesert, 3, 0, 6f));
            double hours = HoursAlong(road, PaceOf(6f), null, 10d, tally);
            Assert.Equal(hours, tally.Hours, 3);
            Assert.Equal(hours, tally.OnGround[Plain] + tally.OnGround[Forest] + tally.OnGround[TradeMath.GroundDesert], 3);
            Assert.True(tally.OnGround[Plain] > 0d);
            Assert.True(tally.OnGround[Forest] > 0d);
            Assert.True(tally.OnGround[TradeMath.GroundDesert] > 0d);
            Assert.Equal(0d, tally.OnGround[TradeMath.GroundWater], 6);
        }

        [Fact]
        public void A_weather_step_is_read_only_from_a_turn_that_changed_that_weather_alone()
        {
            TradeMath.WeatherSteps(false, false, false, true, 0.1f, 0.05f, float.NaN, out float snow, out float wet);
            Assert.True(float.IsNaN(snow));
            Assert.Equal(-0.05f, wet, 4);
            TradeMath.WeatherSteps(false, true, true, true, 0.05f, -0.075f, float.NaN, out snow, out wet);
            Assert.Equal(-0.125f, snow, 4);
            Assert.True(float.IsNaN(wet));
            TradeMath.WeatherSteps(true, true, false, false, -0.175f, 0f, -0.05f, out snow, out wet);
            Assert.Equal(-0.125f, snow, 4);
            TradeMath.WeatherSteps(false, false, true, true, 0f, -0.175f, float.NaN, out snow, out wet);
            Assert.True(float.IsNaN(snow) && float.IsNaN(wet));
        }
}
}
