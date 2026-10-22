using System;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class BestMarketFloorTests
    {
        [Fact]
        public void The_floor_is_the_other_market_price_less_the_tolerance_you_set()
        {
            Assert.Equal(90, TradeRules.BestMarketFloor(100, 0.9f));
            Assert.Equal(100, TradeRules.BestMarketFloor(100, 1f));
            Assert.Equal(50, TradeRules.BestMarketFloor(100, 0.5f));
        }

        [Fact]
        public void A_part_denar_floor_is_cut_off_rather_than_rounded_up()
        {
            Assert.Equal(94, TradeRules.BestMarketFloor(105, 0.9f));
            Assert.Equal(7, TradeRules.BestMarketFloor(15, 0.5f));
        }

        [Fact]
        public void No_other_market_worth_naming_leaves_no_floor_to_clear()
        {
            Assert.Equal(0, TradeRules.BestMarketFloor(0, 0.9f));
            Assert.Equal(0, TradeRules.BestMarketFloor(100, 0f));
        }

        [Fact]
        public void A_price_under_the_floor_is_held_back_and_one_on_it_is_not()
        {
            Assert.True(TradeRules.BelowTheBestMarket(89, 90));
            Assert.False(TradeRules.BelowTheBestMarket(90, 90));
            Assert.False(TradeRules.BelowTheBestMarket(91, 90));
        }

        [Fact]
        public void With_no_floor_in_force_nothing_is_ever_held_back_for_a_better_market()
        {
            for (int price = 0; price <= 500; price += 25)
                Assert.False(TradeRules.BelowTheBestMarket(price, 0));
        }

        [Fact]
        public void Selling_stops_at_the_first_unit_that_falls_under_the_floor()
        {
            int floor = TradeRules.BestMarketFloor(100, 0.9f);
            int[] asTheLotDrains = { 96, 94, 92, 90, 88, 86 };
            int sold = 0;
            foreach (int price in asTheLotDrains)
            {
                if (TradeRules.BelowTheBestMarket(price, floor)) break;
                sold++;
            }
            Assert.Equal(4, sold);
        }

        [Fact]
        public void The_best_price_is_scaled_to_the_quality_you_carry()
        {
            Assert.Equal(140, TradeMath.AtThisQuality(200, 50, 35));
            Assert.Equal(240, TradeMath.AtThisQuality(200, 50, 60));
            Assert.Equal(200, TradeMath.AtThisQuality(200, 50, 50));
        }

        [Fact]
        public void A_worn_good_sells_wherever_a_plain_one_would()
        {
            int bestElsewhere = 200, here = 192, plainValue = 50, wornValue = 35;
            int wornHere = TradeMath.AtThisQuality(here, plainValue, wornValue);
            int wornFloor = TradeRules.BestMarketFloor(
                TradeMath.AtThisQuality(bestElsewhere, plainValue, wornValue), 0.95f);
            Assert.False(TradeRules.BelowTheBestMarket(here, TradeRules.BestMarketFloor(bestElsewhere, 0.95f)));
            Assert.False(TradeRules.BelowTheBestMarket(wornHere, wornFloor));
            Assert.True(TradeRules.BelowTheBestMarket(wornHere, TradeRules.BestMarketFloor(bestElsewhere, 0.95f)));
        }

        [Fact]
        public void A_price_with_nothing_to_scale_it_by_is_left_as_it_is()
        {
            Assert.Equal(0, TradeMath.AtThisQuality(0, 50, 35));
            Assert.Equal(200, TradeMath.AtThisQuality(200, 0, 35));
            Assert.Equal(200, TradeMath.AtThisQuality(200, 50, 0));
        }

        [Fact]
        public void A_scaled_price_stays_between_one_denar_and_what_an_int_holds()
        {
            Assert.Equal(1, TradeMath.AtThisQuality(1, 100, 1));
            Assert.Equal(int.MaxValue, TradeMath.AtThisQuality(int.MaxValue, 1, 3));
        }

        [Fact]
        public void A_lower_tolerance_never_holds_back_more_than_a_higher_one()
        {
            var rng = new Random(5540);
            for (int round = 0; round < 20000; round++)
            {
                int elsewhere = rng.Next(1, 4000);
                float loose = 0.5f + (float)rng.NextDouble() * 0.25f;
                float tight = loose + (float)rng.NextDouble() * 0.25f;
                int price = rng.Next(0, 4000);
                bool heldLoosely = TradeRules.BelowTheBestMarket(price, TradeRules.BestMarketFloor(elsewhere, loose));
                bool heldTightly = TradeRules.BelowTheBestMarket(price, TradeRules.BestMarketFloor(elsewhere, tight));
                if (heldLoosely) Assert.True(heldTightly);
            }
        }

        [Fact]
        public void Only_the_units_the_best_buyer_would_buy_are_held_and_only_below_the_floor()
        {
            Assert.False(TradeRules.HeldForTheMark(price: 150, floor: 0, boughtLeft: 3, holdFor: 3));
            Assert.False(TradeRules.HeldForTheMark(price: 150, floor: 187, boughtLeft: 5, holdFor: 3));
            Assert.True(TradeRules.HeldForTheMark(price: 150, floor: 187, boughtLeft: 3, holdFor: 3));
            Assert.False(TradeRules.HeldForTheMark(price: 187, floor: 187, boughtLeft: 3, holdFor: 3));
            Assert.False(TradeRules.HeldForTheMark(price: 150, floor: 187, boughtLeft: 0, holdFor: 3));
        }

        [Fact]
        public void The_floor_for_the_units_left_is_your_share_of_what_the_best_buyer_pays_for_the_last_of_them()
        {
            int[] rungs = { 300, 280, 260 };
            Assert.Equal(195, TradeRules.FloorForTheLast(rungs, 3, 0.75f));
            Assert.Equal(225, TradeRules.FloorForTheLast(rungs, 1, 0.75f));
            Assert.Equal(0, TradeRules.FloorForTheLast(rungs, 4, 0.75f));
            Assert.Equal(0, TradeRules.FloorForTheLast(rungs, 0, 0.75f));
            Assert.Equal(0, TradeRules.FloorForTheLast(null, 1, 0.75f));
        }

        [Fact]
        public void A_hold_decided_before_turns_only_three_points_past_the_line()
        {
            var choice = new HoldChoice { SoldFrom = 20, SoldTo = 11, HeldFrom = 10 };
            Assert.Equal(0.75f, choice.LineAt(0.75f, 21), 4);
            Assert.Equal(0.72f, choice.LineAt(0.75f, 20), 4);
            Assert.Equal(0.72f, choice.LineAt(0.75f, 11), 4);
            Assert.Equal(0.78f, choice.LineAt(0.75f, 10), 4);
            Assert.Equal(0.78f, choice.LineAt(0.75f, 1), 4);
            Assert.Equal(0.75f, default(HoldChoice).LineAt(0.75f, 5), 4);
            var unsold = new HoldChoice { SoldFrom = 20, SoldTo = 11 };
            Assert.Equal(0.75f, unsold.LineAt(0.75f, 10), 4);
            Assert.Equal(0f, choice.LineAt(0f, 5));
            Assert.Equal(0f, unsold.LineAt(0.02f, 15));
        }

        [Fact]
        public void A_hold_choice_notes_the_units_it_sold_and_the_unit_it_held_from()
        {
            var choice = default(HoldChoice);
            Assert.False(choice.Made);
            choice.Note(0, held: false);
            Assert.False(choice.Made);
            choice.Note(8, held: false);
            choice.Note(7, held: false);
            choice.Note(6, held: true);
            choice.Note(5, held: true);
            Assert.True(choice.Made);
            Assert.Equal((8, 7, 6), (choice.SoldFrom, choice.SoldTo, choice.HeldFrom));
            var held = default(HoldChoice);
            held.Note(4, held: true);
            Assert.True(held.Made);
            Assert.Equal((0, 0, 4), (held.SoldFrom, held.SoldTo, held.HeldFrom));
        }

        [Fact]
        public void A_hold_decision_is_remembered_for_a_day_for_that_good_at_that_market_alone()
        {
            Holds.Forget();
            var held = new HoldChoice { HeldFrom = 5 };
            Holds.Decided("oil", "ortysia", held, 100);
            Assert.Equal(5, Holds.Recalled("oil", "ortysia", 110).HeldFrom);
            Assert.Equal(5, Holds.Recalled("oil", "ortysia", 124).HeldFrom);
            Assert.False(Holds.Recalled("oil", "ortysia", 125).Made);
            Assert.False(Holds.Recalled("oil", "ortysia", 99).Made);
            Assert.False(Holds.Recalled("oil", "poros", 110).Made);
            Assert.False(Holds.Recalled("salt", "ortysia", 110).Made);
            Assert.Equal(0.78f, Holds.LineFor("oil", "ortysia", 0.75f, 5, 110), 4);
            Assert.Equal(0.75f, Holds.LineFor("oil", "ortysia", 0.75f, 6, 110), 4);
            Holds.Decided("oil", "ortysia", new HoldChoice { SoldFrom = 5, SoldTo = 1 }, 111);
            Assert.Equal(0.72f, Holds.LineFor("oil", "ortysia", 0.75f, 5, 112), 4);
            Holds.Decided("oil", "ortysia", default(HoldChoice), 112);
            Assert.Equal(0.72f, Holds.LineFor("oil", "ortysia", 0.75f, 5, 112), 4);
            Holds.Decided(null, "ortysia", held, 111);
            Assert.False(Holds.Recalled(null, "ortysia", 111).Made);
            Assert.Equal(0.75f, Holds.LineFor(null, "ortysia", 0.75f, 5, 111), 4);
            Holds.Forget();
            Assert.False(Holds.Recalled("oil", "ortysia", 112).Made);
        }

        [Fact]
        public void The_hold_memory_lets_old_decisions_go_before_it_grows_too_big()
        {
            Holds.Forget();
            var held = new HoldChoice { HeldFrom = 1 };
            for (int i = 0; i < Holds.MostRemembered - 1; i++) Holds.Decided("good" + i, "market", held, 0);
            Holds.Decided("fresh", "market", held, 100);
            Holds.Decided("fresher", "market", held, 100);
            Assert.False(Holds.Recalled("good0", "market", 10).Made);
            Assert.True(Holds.Recalled("fresh", "market", 100).Made);
            Assert.True(Holds.Recalled("fresher", "market", 100).Made);
            Holds.Forget();
        }

        private static int[] Flat(int price, int units)
        {
            var sold = new int[units];
            for (int u = 0; u < units; u++) sold[u] = price;
            return sold;
        }

        [Fact]
        public void The_purse_of_the_best_buyer_goes_to_the_units_this_market_pays_least_for_against_it()
        {
            var rungs = new[] { new[] { 250, 250, 250, 250 }, new[] { 400, 400 }, null };
            var here = new[] { Flat(180, 4), Flat(120, 2), null };
            Assert.Equal(new[] { 2, 2, 0 }, TradeRules.SharePurse(1300, rungs, here, new[] { 4, 2, 0 }, 0.75f));
        }

        [Fact]
        public void A_unit_this_market_pays_your_share_for_is_the_last_to_draw_on_the_purse()
        {
            var rungs = new[] { new[] { 200 }, new[] { 400 } };
            var here = new[] { Flat(180, 1), Flat(120, 1) };
            Assert.Equal(new[] { 0, 1 }, TradeRules.SharePurse(400, rungs, here, new[] { 1, 1 }, 0.75f));
            Assert.Equal(new[] { 1, 1 }, TradeRules.SharePurse(600, rungs, here, new[] { 1, 1 }, 0.75f));
        }

        [Fact]
        public void Goods_that_ride_to_the_best_buyer_anyway_are_paid_for_first()
        {
            var rungs = new[] { new[] { 250, 250, 250 }, new[] { 400, 400 } };
            var here = new[] { Flat(180, 3), new int[0] };
            Assert.Equal(new[] { 1, 2 }, TradeRules.SharePurse(1050, rungs, here, new[] { 3, 2 }, 0.75f));
        }

        [Fact]
        public void Units_this_market_cannot_take_ride_first_and_the_ones_it_pays_least_for_come_next()
        {
            var rungs = new[] { new[] { 250, 250, 250, 250 }, new[] { 250, 250 } };
            var here = new[] { new[] { 200, 120 }, Flat(150, 2) };
            Assert.Equal(new[] { 3, 1 }, TradeRules.SharePurse(1000, rungs, here, new[] { 4, 2 }, 0.75f));
        }

        [Fact]
        public void A_unit_too_dear_for_what_is_left_shuts_its_good_and_cheaper_ones_still_fit()
        {
            var rungs = new[] { new[] { 400, 400 }, new[] { 100, 100 } };
            var here = new[] { Flat(100, 2), Flat(60, 2) };
            Assert.Equal(new[] { 1, 2 }, TradeRules.SharePurse(650, rungs, here, new[] { 2, 2 }, 0.75f));
        }

        [Fact]
        public void Each_best_buyer_spends_only_its_own_purse_on_the_goods_held_for_it()
        {
            var rungs = new[] { new[] { 250, 250, 250, 250 }, new[] { 400, 400 }, new[] { 300, 300 }, null };
            var here = new[] { Flat(180, 4), Flat(120, 2), Flat(100, 2), null };
            var bought = new[] { 4, 2, 2, 0 };
            Assert.Equal(new[] { 2, 2, 0, 0 },
                         TradeRules.SharePurses(new[] { "a", "a", "b", null }, new[] { 1300, 1300, 0, 0 }, rungs, here,
                                                bought, 0.75f));
            Assert.Equal(new[] { 2, 2, 2, 0 },
                         TradeRules.SharePurses(new[] { "a", "a", "b", null }, new[] { 1300, 1300, 600, 0 }, rungs, here,
                                                bought, 0.75f));
            Assert.Equal(new[] { 4, 2, 0, 0 },
                         TradeRules.SharePurses(new[] { "a", "b", "c", null }, new[] { 1000, 800, 0, 0 }, rungs, here,
                                                bought, 0.75f));
            Assert.Empty(TradeRules.SharePurses(null, null, null, null, null, 0.75f));
        }

        [Fact]
        public void No_purse_or_no_share_holds_nothing()
        {
            var rungs = new[] { new[] { 300 } };
            var here = new[] { Flat(100, 1) };
            Assert.Equal(new[] { 0 }, TradeRules.SharePurse(0, rungs, here, new[] { 1 }, 0.75f));
            Assert.Equal(new[] { 0 }, TradeRules.SharePurse(1000, rungs, here, new[] { 1 }, 0f));
            Assert.Empty(TradeRules.SharePurse(1000, null, null, null, 0.75f));
        }

        [Fact]
        public void This_market_is_walked_until_your_margin_or_its_gold_runs_out()
        {
            int till = 500;
            Assert.Equal(new[] { 200, 180 }, TradeRules.WhatSellsHere(u => 200 - 20 * u, 5, 100, 0.15f, ref till));
            Assert.Equal(120, till);
            till = 10000;
            Assert.Equal(new[] { 200, 180, 160, 140, 120 }, TradeRules.WhatSellsHere(u => 200 - 20 * u, 8, 100, 0.15f, ref till));
        }

        [Fact]
        public void The_best_buyer_is_asked_past_the_units_another_quality_already_took()
        {
            Assert.Equal(new[] { 100, 90 }, TradeRules.PastTheFirst(new[] { 300, 200, 100, 90 }, 2));
            Assert.Empty(TradeRules.PastTheFirst(new[] { 300 }, 1));
            Assert.Equal(new[] { 300 }, TradeRules.PastTheFirst(new[] { 300 }, 0));
        }

        [Fact]
        public void Setting_the_bought_units_aside_leaves_the_looted_ones_to_sell()
        {
            int remaining = 10, paidLeft = 4;
            Assert.True(TradeMath.SetTheBoughtUnitsAside(ref remaining, ref paidLeft));
            Assert.Equal(6, remaining);
            Assert.Equal(0, paidLeft);
            remaining = 4; paidLeft = 4;
            Assert.False(TradeMath.SetTheBoughtUnitsAside(ref remaining, ref paidLeft));
            Assert.Equal(4, remaining);
            remaining = 5; paidLeft = 0;
            Assert.False(TradeMath.SetTheBoughtUnitsAside(ref remaining, ref paidLeft));
            Assert.Equal(5, remaining);
        }
    }
}
