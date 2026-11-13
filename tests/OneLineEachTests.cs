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

        [Fact]
        public void Game_messages_of_one_kind_share_a_shape_once_names_links_and_numbers_are_taken_out()
        {
            var names = new List<string> { "Sorgard the Tanner", "Melya the She-Wolf", "Dogface Yalyr", "Ortysia", "Jalmarys",
                                            "Varys", "Ealdred", "Sorgard" };
            names.Sort((x, y) => y.Length.CompareTo(x.Length));
            string learned = GameNews.Shape("You've learned about Sorgard the Tanner.", names);
            Assert.Equal(learned, GameNews.Shape("You've learned about Dogface Yalyr.", names));
            Assert.Equal(learned, GameNews.Shape("You've learned about Melya the She-Wolf.", names));
            string defended = GameNews.Shape("Lord Varys has defended Ortysia", names);
            Assert.Equal(defended, GameNews.Shape("Lord Ealdred has defended Jalmarys", names));
            Assert.NotEqual(defended, GameNews.Shape("Lord Varys has captured Ortysia", names));
            Assert.NotEqual(learned, defended);
            Assert.Equal(GameNews.Shape("<a style=\"Link.Hero\" href=\"event:Hero:a\"><b>Varys</b></a> has defended Ortysia", names),
                         GameNews.Shape("<a style=\"Link.Hero\" href=\"event:Hero:b\"><b>Somebody</b></a> has defended Jalmarys", names));
            Assert.Equal(GameNews.Shape("You received 112 gold.", names), GameNews.Shape("You received 1,096 gold.", names));
            Assert.Equal("Varysville stands", GameNews.Shape("Varysville stands", names));
            Assert.Equal("", GameNews.Shape(null, names));
        }

        [Fact]
        public void A_party_named_with_s_or_a_bare_apostrophe_gives_one_shape()
        {
            var names = new List<string> { "Shibal Zumr Castle", "Yamina", "Talas" };
            names.Sort((x, y) => y.Length.CompareTo(x.Length));
            string yamina = GameNews.Shape("Shibal Zumr Castle is being defended by Yamina's Party.", names);
            Assert.Equal(yamina, GameNews.Shape("Shibal Zumr Castle is being defended by Talas' Party.", names));
            Assert.Equal(yamina, GameNews.Shape("Shibal Zumr Castle is being defended by Talas\u2019 Party.", names));
            Assert.Equal(yamina, GameNews.Shape("Shibal Zumr Castle is being defended by Yamina\u2019s Party.", names));
            var parties = new List<string> { "Shibal Zumr Castle", "Yamina's Party", "Talas' Party", "Yamina", "Talas" };
            parties.Sort((x, y) => y.Length.CompareTo(x.Length));
            Assert.Equal(GameNews.Shape("Shibal Zumr Castle is being defended by Yamina's Party.", parties),
                         GameNews.Shape("Shibal Zumr Castle is being defended by Talas' Party.", parties));
            Assert.NotEqual(GameNews.Shape("Talas's sword broke.", names), GameNews.Shape("Talas' sword broke.", new List<string>()));
        }

        [Fact]
        public void Game_messages_are_held_only_while_the_clock_runs_and_more_of_their_kind_keep_coming()
        {
            Assert.True(GameNews.StillHeld(10f, 10f, 10.5f, true));
            Assert.False(GameNews.StillHeld(10f, 10f, 10f + GameNews.QuietFor, true));
            Assert.True(GameNews.StillHeld(10f, 11.2f, 12.5f, true));
            Assert.False(GameNews.StillHeld(10f, 13.9f, 10f + GameNews.HeldAtMost, true));
            Assert.False(GameNews.StillHeld(10f, 10f, 9f, true));
            Assert.False(GameNews.StillHeld(10f, 10f, 10.01f, false));
        }

        [Fact]
        public void A_grouped_game_message_says_how_many_more_before_its_full_stop()
        {
            Assert.Equal("You've learned about Sorgard the Tanner (+5 more).",
                         GameNews.WithMore("You've learned about Sorgard the Tanner.", " (+5 more)"));
            Assert.Equal("Lord Varys has defended Ortysia (+19 more)",
                         GameNews.WithMore("Lord Varys has defended Ortysia", " (+19 more)"));
            Assert.Equal("你了解了索尔加德（另有 5 条）。", GameNews.WithMore("你了解了索尔加德。", "（另有 5 条）"));
            Assert.Equal("Peace!", GameNews.WithMore("Peace!", ""));
            Assert.Equal("(+2 more)", GameNews.WithMore("", " (+2 more)"));
        }

        [Fact]
        public void Net_worth_is_your_gold_and_every_stack_you_carry_at_its_value()
        {
            Assert.Equal(690L, Holdings.WorthOfAStack(230, 3));
            Assert.Equal(0L, Holdings.WorthOfAStack(0, 3));
            Assert.Equal(0L, Holdings.WorthOfAStack(230, 0));
            Assert.Equal(4294967294L, Holdings.WorthOfAStack(int.MaxValue, 2));
            Assert.Equal(8605L, Holdings.NetWorth(7915, 690));
            Assert.Equal(7915L, Holdings.NetWorth(7915, -5));
            Assert.Equal(-20L, Holdings.NetWorth(-20, 0));
        }

        [Fact]
        public void The_trade_xp_turns_red_once_the_chat_line_can_show_it_and_keeps_the_space_before_it()
        {
            Assert.Equal(" (+70 Trade XP)", ProfitMark.XpShown(" (+70 Trade XP)", false));
            Assert.Equal(" <span style=\"TradeLord.TradeXp\">(+70 Trade XP)</span>",
                         ProfitMark.XpShown(" (+70 Trade XP)", true));
            Assert.Equal("<span style=\"TradeLord.TradeXp\">（+70 点交易经验）</span>",
                         ProfitMark.XpShown("（+70 点交易经验）", true));
            Assert.Equal("", ProfitMark.XpShown("", true));
            Assert.Equal("", ProfitMark.XpShown(null, true));
            Assert.Equal("  ", ProfitMark.XpShown("  ", true));
        }
    }
}
