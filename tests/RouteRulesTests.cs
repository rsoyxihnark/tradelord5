using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class BudgetTests
    {
        private const int NoVisitCap = 0;

        [Fact]
        public void The_gold_reserve_is_held_back_from_every_purse()
        {
            Assert.Equal(700, TradeMath.Budget(1000, 300, NoVisitCap, 0, 0));
            Assert.Equal(0, TradeMath.Budget(300, 300, NoVisitCap, 0, 0));
        }

        [Fact]
        public void Below_the_reserve_the_budget_goes_negative_so_the_pass_stops()
        {
            Assert.True(TradeMath.Budget(260, 300, NoVisitCap, 0, 0) < 0);
            Assert.True(TradeMath.Budget(0, 300, NoVisitCap, 0, 0) < 0);
        }

        [Fact]
        public void A_visit_cap_binds_when_it_is_tighter_than_the_purse()
        {
            Assert.Equal(400, TradeMath.Budget(5000, 300, 400, 0, 0));
            Assert.Equal(150, TradeMath.Budget(5000, 300, 400, 250, 0));
            Assert.Equal(0, TradeMath.Budget(5000, 300, 400, 400, 0));
        }

        [Fact]
        public void The_purse_binds_when_it_is_tighter_than_the_visit_cap()
        {
            Assert.Equal(200, TradeMath.Budget(500, 300, 10000, 0, 0));
        }

        [Fact]
        public void A_visit_cap_of_zero_means_no_visit_cap_rather_than_no_spending()
        {
            Assert.Equal(700, TradeMath.Budget(1000, 300, 0, 0, 0));
            Assert.True(TradeMath.Budget(1000, 300, 0, 99999, 0) > 0);
        }

        [Fact]
        public void What_this_pass_has_already_spent_comes_off_both_limits()
        {
            Assert.Equal(500, TradeMath.Budget(1000, 300, NoVisitCap, 0, 200));
            Assert.Equal(200, TradeMath.Budget(5000, 300, 400, 0, 200));
        }

        [Fact]
        public void Spending_never_takes_the_purse_below_the_reserve()
        {
            int gold = 1000, reserve = 300, spentThisPass = 0;
            while (true)
            {
                int budget = TradeMath.Budget(gold, reserve, NoVisitCap, 0, spentThisPass);
                if (budget < 50) break;
                spentThisPass += 50;
            }
            Assert.True(gold - spentThisPass >= reserve,
                $"purse fell to {gold - spentThisPass}, under the {reserve} reserve");
        }
    }

    public class ConfidenceTests
    {
        private static float Ideal() => Confidence.Of(
            simulated: true, flatProfit: 100, simulatedProfit: 100,
            stock: 100, units: 1, travelDays: 0f, caravans: 0, dataAgeDays: -1f);

        [Fact]
        public void A_perfect_route_scores_one_and_nothing_ever_exceeds_it()
        {
            Assert.Equal(1f, Ideal(), 3);
        }

        [Fact]
        public void Confidence_always_lands_between_one_percent_and_certainty()
        {
            foreach (bool sim in new[] { true, false })
                foreach (int flat in new[] { -100, 0, 1, 500 })
                    foreach (int simProfit in new[] { -100, 0, 250, 100000 })
                        foreach (int stock in new[] { 0, 1, 50 })
                            foreach (int units in new[] { 0, 1, 500 })
                                foreach (float days in new[] { -5f, 0f, 3f, 400f })
                                    foreach (int caravans in new[] { -3, 0, 9 })
                                        foreach (float age in new[] { -1f, 0f, 45f })
                                        {
                                            float c = Confidence.Of(sim, flat, simProfit, stock, units, days, caravans, age);
                                            Assert.InRange(c, 0.01f, 1f);
                                        }
        }

        [Fact]
        public void A_longer_haul_is_never_more_confident_than_a_shorter_one()
        {
            float near = Confidence.Of(true, 100, 100, 100, 1, 1f, 0, -1f);
            float far = Confidence.Of(true, 100, 100, 100, 1, 12f, 0, -1f);
            Assert.True(far < near);
        }

        [Fact]
        public void More_caravans_on_the_route_never_raise_confidence()
        {
            float quiet = Confidence.Of(true, 100, 100, 100, 1, 2f, 0, -1f);
            float busy = Confidence.Of(true, 100, 100, 100, 1, 2f, 6, -1f);
            Assert.True(busy < quiet);
        }

        [Fact]
        public void Older_prices_are_trusted_less_and_live_prices_most()
        {
            float live = Confidence.Of(true, 100, 100, 100, 1, 2f, 0, -1f);
            float fresh = Confidence.Of(true, 100, 100, 100, 1, 2f, 0, 0f);
            float stale = Confidence.Of(true, 100, 100, 100, 1, 2f, 0, 40f);
            Assert.Equal(live, fresh, 3);
            Assert.True(stale < live);
        }

        [Fact]
        public void A_thin_shelf_is_trusted_less_than_a_deep_one()
        {
            float deep = Confidence.Of(true, 100, 100, 90, 10, 1f, 0, -1f);
            float thin = Confidence.Of(true, 100, 100, 3, 10, 1f, 0, -1f);
            Assert.True(thin < deep);
        }

        [Fact]
        public void A_simulation_that_holds_up_beats_one_that_collapses()
        {
            float holds = Confidence.Of(true, 100, 95, 100, 1, 1f, 0, -1f);
            float collapses = Confidence.Of(true, 100, 20, 100, 1, 1f, 0, -1f);
            Assert.True(collapses < holds);
        }

        [Fact]
        public void An_unwalkable_route_is_marked_down_but_not_written_off()
        {
            float unsimulated = Confidence.Of(false, 100, 100, 100, 1, 0f, 0, -1f);
            Assert.True(unsimulated < 1f && unsimulated > 0.5f);
        }
    }

    public class ItemListTests
    {
        [Theory]
        [InlineData("grain")]
        [InlineData("grain,hardwood")]
        [InlineData("grain, hardwood")]
        [InlineData("grain;hardwood")]
        [InlineData("grain hardwood")]
        [InlineData("  grain ,, hardwood  ")]
        public void An_item_list_is_read_however_it_is_punctuated(string written)
        {
            var options = new Options { NeverSellItems = written };
            Assert.True(options.NeverSet.HasId("grain"));
        }

        [Fact]
        public void Item_names_are_matched_whatever_the_casing()
        {
            var options = new Options { NeverSellItems = "Grain" };
            Assert.True(options.NeverSet.HasId("grain"));
            Assert.True(options.NeverSet.HasId("GRAIN"));
        }

        [Fact]
        public void An_empty_list_holds_nothing_and_throws_on_nothing()
        {
            Assert.True(new Options { NeverSellItems = "" }.NeverSet.Empty);
            Assert.True(new Options { NeverSellItems = null }.NeverSet.Empty);
            Assert.True(new Options { NeverSellItems = "  ,, ; " }.NeverSet.Empty);
        }

        [Fact]
        public void Editing_the_list_is_seen_at_once_rather_than_served_from_the_last_read()
        {
            var options = new Options { NeverSellItems = "grain" };
            Assert.True(options.NeverSet.HasId("grain"));

            options.NeverSellItems = "hardwood";
            Assert.False(options.NeverSet.HasId("grain"));
            Assert.True(options.NeverSet.HasId("hardwood"));
        }

        [Fact]
        public void The_three_lists_stay_separate()
        {
            var options = new Options
            {
                NeverSellItems = "grain",
                AlwaysSellItems = "hardwood",
                NeverBuyItems = "fish",
            };
            Assert.True(options.NeverSet.HasId("grain"));
            Assert.True(options.AlwaysSet.HasId("hardwood"));
            Assert.True(options.NeverBuySet.HasId("fish"));
            Assert.False(options.AlwaysSet.HasId("grain"));
            Assert.False(options.NeverSet.HasId("fish"));
        }

        [Theory]
        [InlineData("Iron Ore")]
        [InlineData("iron ore")]
        [InlineData(" IRON ORE ")]
        [InlineData("Iron Ore, Hardwood")]
        [InlineData("Hardwood;Iron Ore")]
        public void A_name_with_a_space_in_it_is_kept_whole(string written)
        {
            Assert.True(new Options { NeverSellItems = written }.NeverSet.HasName("Iron Ore"));
        }

        [Fact]
        public void A_word_of_a_written_name_still_stands_for_an_id_of_its_own()
        {
            var options = new Options { NeverSellItems = "Iron Ore" };
            Assert.True(options.NeverSet.HasName("Iron Ore"));
            Assert.True(options.NeverSet.HasId("iron"));
            Assert.True(options.NeverSet.HasId("ore"));
        }

        [Fact]
        public void Naming_one_good_never_catches_another_whose_name_is_a_word_of_it()
        {
            var options = new Options { NeverSellItems = "Iron Ore, Fine Steel" };
            Assert.False(options.NeverSet.HasName("Iron"));
            Assert.False(options.NeverSet.HasName("Ore"));
            Assert.False(options.NeverSet.HasName("Steel"));
            Assert.True(options.NeverSet.HasName("Iron Ore"));
            Assert.True(options.NeverSet.HasName("Fine Steel"));
        }

        [Fact]
        public void Ids_written_the_old_way_with_spaces_between_them_still_read()
        {
            var options = new Options { NeverSellItems = "grain hardwood iron_ore" };
            Assert.True(options.NeverSet.HasId("grain"));
            Assert.True(options.NeverSet.HasId("hardwood"));
            Assert.True(options.NeverSet.HasId("iron_ore"));
        }

        [Fact]
        public void A_multi_word_name_is_not_confused_with_a_neighbouring_entry()
        {
            var options = new Options { NeverSellItems = "Iron Ore, Fine Velvet" };
            Assert.True(options.NeverSet.HasName("Iron Ore"));
            Assert.True(options.NeverSet.HasName("Fine Velvet"));
            Assert.False(options.NeverSet.HasName("Ore, Fine"));
            Assert.False(options.NeverSet.HasName("Iron Ore, Fine Velvet"));
        }

        [Theory]
        [InlineData("grain", "GRAIN")]
        [InlineData("GRAIN", "grain")]
        [InlineData("Grain", "gRaIn")]
        [InlineData("IRON ORE", "iron ore")]
        [InlineData("iron ore", "Iron Ore")]
        [InlineData("Iron_Ore", "iron_ore")]
        public void A_name_is_matched_whatever_its_capitalisation(string written, string looked)
        {
            var list = new Options { NeverSellItems = written }.NeverSet;
            Assert.True(list.HasId(looked) || list.HasName(looked));
        }

        [Fact]
        public void A_list_of_nothing_but_separators_still_holds_nothing()
        {
            Assert.True(new Options { NeverSellItems = " , ; ,, ;; " }.NeverSet.Empty);
            Assert.True(new Options { NeverSellItems = "\t" }.NeverSet.Empty);
        }
    }
}
