using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class OneLineEachTests
    {
        private static Dictionary<string, (int count, int gold)> Goods(params (string what, int count, int gold)[] moved)
        {
            var detail = new Dictionary<string, (int count, int gold)>();
            foreach (var one in moved) detail[one.what] = (one.count, one.gold);
            return detail;
        }

        private static int UnitsOf(OneLineEach<string>.Said line)
        {
            int units = 0;
            foreach (var one in line.Detail) units += one.Value.count;
            return units;
        }

        [Fact]
        public void Wine_and_donkeys_bought_then_cheese_after_the_loot_sale_are_one_bought_line()
        {
            var told = new OneLineEach<string>();
            told.Add(Told.Bought, false, Goods(("wine", 10, 500)), 10, 500);
            told.Add(Told.Bought, false, Goods(("donkey", 2, 300)), 2, 300);
            told.Add(Told.Sold, false, Goods(("sword", 3, 900)), 3, 900, 900);
            told.Add(Told.Bought, false, Goods(("cheese", 4, 100)), 4, 100);

            List<OneLineEach<string>.Said> lines = told.Closed();

            Assert.Equal(2, lines.Count);
            OneLineEach<string>.Said bought = lines.Find(line => line.What == Told.Bought);
            Assert.Equal(16, bought.Units);
            Assert.Equal(900, bought.Gold);
            Assert.Equal((10, 500), bought.Detail["wine"]);
            Assert.Equal((2, 300), bought.Detail["donkey"]);
            Assert.Equal((4, 100), bought.Detail["cheese"]);
            Assert.Equal(bought.Units, UnitsOf(bought));
        }

        [Fact]
        public void The_bought_line_comes_out_before_the_sold_line_whatever_order_the_passes_ran_in()
        {
            var first = new OneLineEach<string>();
            first.Add(Told.Sold, false, Goods(("sword", 3, 900)), 3, 900, 900);
            first.Add(Told.Sold, false, Goods(("wine", 10, 800)), 10, 800, 200);
            first.Add(Told.Bought, false, Goods(("wine", 10, 500)), 10, 500);
            first.Add(Told.Sold, false, Goods(("horse", 1, 200)), 1, 200, 50);
            first.Add(Told.Bought, false, Goods(("grain", 5, 50)), 5, 50);
            first.Add(Told.Bought, false, Goods(("mule", 1, 150)), 1, 150);
            first.Add(Told.Bought, false, Goods(("grain", 3, 30)), 3, 30);

            var second = new OneLineEach<string>();
            second.Add(Told.Bought, false, Goods(("grain", 3, 30)), 3, 30);
            second.Add(Told.Bought, false, Goods(("mule", 1, 150)), 1, 150);
            second.Add(Told.Bought, false, Goods(("wine", 10, 500)), 10, 500);
            second.Add(Told.Bought, false, Goods(("grain", 5, 50)), 5, 50);
            second.Add(Told.Sold, false, Goods(("wine", 10, 800)), 10, 800, 200);
            second.Add(Told.Sold, false, Goods(("horse", 1, 200)), 1, 200, 50);
            second.Add(Told.Sold, false, Goods(("sword", 3, 900)), 3, 900, 900);

            List<OneLineEach<string>.Said> one = first.Closed();
            List<OneLineEach<string>.Said> other = second.Closed();

            Assert.Equal(new[] { Told.Bought, Told.Sold },
                         one.ConvertAll(line => line.What).ToArray());
            Assert.Equal(one.Count, other.Count);
            for (int at = 0; at < one.Count; at++)
            {
                Assert.Equal(one[at].What, other[at].What);
                Assert.Equal(one[at].Units, other[at].Units);
                Assert.Equal(one[at].Gold, other[at].Gold);
                Assert.Equal(one[at].Profit, other[at].Profit);
                Assert.Equal(one[at].Detail, other[at].Detail);
            }
            Assert.Equal(1150, one[1].Profit);
            Assert.Equal((1, 200), one[1].Detail["horse"]);
            Assert.Equal((8, 80), one[0].Detail["grain"]);
            Assert.Equal((1, 150), one[0].Detail["mule"]);
        }

        [Fact]
        public void A_good_bought_in_two_passes_is_named_once_with_both_counted()
        {
            var told = new OneLineEach<string>();
            told.Add(Told.Bought, false, Goods(("wine", 6, 300)), 6, 300);
            told.Add(Told.Bought, false, Goods(("wine", 4, 220), ("oil", 2, 90)), 6, 310);

            OneLineEach<string>.Said bought = Assert.Single(told.Closed());

            Assert.Equal(2, bought.Detail.Count);
            Assert.Equal((10, 520), bought.Detail["wine"]);
            Assert.Equal(12, bought.Units);
            Assert.Equal(610, bought.Gold);
        }

        [Fact]
        public void Haul_animals_food_and_goods_bought_on_one_visit_are_one_bought_line()
        {
            var told = new OneLineEach<string>();
            told.Add(Told.Bought, false, Goods(("mule", 2, 300)), 2, 300);
            told.Add(Told.Bought, false, Goods(("camel", 1, 400)), 1, 400);
            told.Add(Told.Bought, false, Goods(("grain", 5, 50)), 5, 50);

            OneLineEach<string>.Said bought = Assert.Single(told.Closed());

            Assert.Equal(Told.Bought, bought.What);
            Assert.Equal(8, bought.Units);
            Assert.Equal(750, bought.Gold);
        }

        [Fact]
        public void A_pass_that_moved_nothing_adds_no_line()
        {
            var told = new OneLineEach<string>();
            told.Add(Told.Bought, false, Goods(), 0, 0);
            told.Add(Told.Sold, false, null, 0, 0, 0);

            Assert.Equal(0, told.Count);
            Assert.Empty(told.Closed());
        }

        [Fact]
        public void A_dry_run_line_never_merges_with_a_real_one()
        {
            var told = new OneLineEach<string>();
            told.Add(Told.Bought, true, Goods(("wine", 10, 500)), 10, 500);
            told.Add(Told.Bought, false, Goods(("wine", 4, 200)), 4, 200);

            List<OneLineEach<string>.Said> lines = told.Closed();

            Assert.Equal(2, lines.Count);
            Assert.False(lines[0].Sim);
            Assert.Equal(4, lines[0].Units);
            Assert.True(lines[1].Sim);
            Assert.Equal(10, lines[1].Units);
        }

        [Fact]
        public void Closing_the_lines_empties_them_so_the_next_visit_starts_fresh()
        {
            var told = new OneLineEach<string>();
            told.Add(Told.Sold, false, Goods(("sword", 1, 300)), 1, 300, 300);
            Assert.Single(told.Closed());

            Assert.Equal(0, told.Count);
            told.Add(Told.Sold, false, Goods(("wine", 2, 100)), 2, 100, 20);
            OneLineEach<string>.Said sold = Assert.Single(told.Closed());
            Assert.Equal(2, sold.Units);
            Assert.False(sold.Detail.ContainsKey("sword"));
        }

        [Fact]
        public void Forgetting_the_lines_drops_what_was_never_said()
        {
            var told = new OneLineEach<string>();
            told.Add(Told.Bought, false, Goods(("grain", 5, 50)), 5, 50);
            told.Forget();

            Assert.Empty(told.Closed());
        }

        [Fact]
        public void A_loss_on_one_sale_is_taken_off_the_profit_of_the_other()
        {
            var told = new OneLineEach<string>();
            told.Add(Told.Sold, false, Goods(("wine", 10, 800)), 10, 800, -150);
            told.Add(Told.Sold, false, Goods(("sword", 1, 300)), 1, 300, 300);

            OneLineEach<string>.Said sold = Assert.Single(told.Closed());

            Assert.Equal(150, sold.Profit);
            Assert.Equal(1100, sold.Gold);
        }

        [Fact]
        public void The_profit_carries_a_denar_coin_and_turns_golden_once_the_chat_line_can_show_it()
        {
            const string coin = "<img src=\"General\\Icons\\Coin@2x\" extend=\"6\">";
            Assert.Equal("2062" + coin, ProfitMark.Shown(2062, false));
            Assert.Equal("<span style=\"TradeLord.Profit\">2062</span>" + coin, ProfitMark.Shown(2062, true));
            Assert.Equal("<span style=\"TradeLord.Profit\">0</span>" + coin, ProfitMark.Shown(0, true));
        }
    }
}
