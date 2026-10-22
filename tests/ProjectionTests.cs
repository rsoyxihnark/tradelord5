using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class ProjectionTests
    {
        private static Landing Coming(string item, string category, int units, int worth, float days) =>
            new Landing { Item = item, Category = category, Units = units, Worth = worth, Days = days };

        private static Spending Bought(string category, int worth, float days) =>
            new Spending { Category = category, Worth = worth, Days = days };

        private static Draw Taken(string category, int worth, float days) =>
            new Draw { Category = category, Worth = worth, Days = days };

        [Fact]
        public void What_a_town_uses_up_each_day_leaves_its_shelf_for_every_day_of_the_window()
        {
            Assert.Equal(600, Projection.WorthUsedUp(null, "wool", 2f, 300, 10000));
            Assert.Equal(300, Projection.WorthUsedUp(null, "wool", 1.1f, 300, 10000));
            Assert.Equal(0, Projection.WorthUsedUp(null, "wool", 0f, 300, 10000));
        }

        [Fact]
        public void What_a_workshop_takes_leaves_only_the_kind_it_takes_and_only_once_it_runs_in_time()
        {
            var drawn = new List<Draw> { Taken("wool", 200, 0.5f), Taken("wool", 200, 1.5f), Taken("iron", 500, 0.2f) };
            Assert.Equal(200, Projection.WorthUsedUp(drawn, "wool", 1f, 0, 10000));
            Assert.Equal(400, Projection.WorthUsedUp(drawn, "wool", 2f, 0, 10000));
            Assert.Equal(500, Projection.WorthUsedUp(drawn, "iron", 1f, 0, 10000));
            Assert.Equal(0, Projection.WorthUsedUp(drawn, "grain", 1f, 0, 10000));
            Assert.Equal(700, Projection.WorthUsedUp(drawn, "wool", 2f, 150, 10000));
        }

        [Fact]
        public void A_town_never_uses_up_more_than_its_shelf_and_what_lands_on_it_can_give()
        {
            var drawn = new List<Draw> { Taken("wool", 200, 0.5f) };
            Assert.Equal(1000, Projection.WorthUsedUp(null, "wool", 5f, 300, 1000));
            Assert.Equal(150, Projection.WorthUsedUp(drawn, "wool", 2f, 300, 150));
            Assert.Equal(0, Projection.WorthUsedUp(drawn, "wool", 2f, 300, 0));
            Assert.Equal(0, Projection.WorthUsedUp(drawn, null, 2f, 300, 1000));
        }

        [Fact]
        public void A_shelf_read_ahead_takes_off_what_the_town_uses_up_and_what_its_workshops_take()
        {
            var drawn = new List<Draw> { Taken("grain", 100, 1f) };
            var curve = Projection.ShelfAhead(null, null,                       "grain", "grain", 10, 40, 0.5f, drawn, 50);
            Assert.Equal(new[] { (1f, 25), (2f, 20), (3f, 15), (4f, 10), (5f, 5), (6f, 0) },
                         curve.GetRange(0, 6).ToArray());
            Assert.Equal(new[] { 1f }, Projection.Moments(null, null, 0.5f, drawn).ToArray());
            Assert.Equal(3f, Projection.RunsOutOf(curve, 20));
        }

        [Fact]
        public void A_town_that_uses_a_good_up_is_read_a_day_at_a_time_from_the_day_you_arrive()
        {
            Assert.Equal(new[] { 1f, 2f, 3f }, Projection.Moments(null, null, 0.5f, null, 100).GetRange(0, 3).ToArray());
            Assert.Equal(new[] { 6f, 7f }, Projection.Moments(null, null, 5.5f, null, 100).GetRange(0, 2).ToArray());
            Assert.Equal(Projection.DaysUseIsReadFor, Projection.Moments(null, null, 0.5f, null, 100).Count);
            Assert.Equal(Projection.DaysUseIsReadFor, Projection.Moments(null, null, 2f, null, 1).Count);
            Assert.Equal(3f, Projection.Moments(null, null, 2f, null, 1)[0]);
            Assert.Empty(Projection.Moments(null, null, 0.5f, null, 0));
            Assert.Empty(Projection.Moments(null, null, float.NaN, null, 100));
        }

        [Fact]
        public void A_shelf_a_landing_keeps_up_still_runs_out_on_the_day_the_town_has_used_it_up()
        {
            var listed = new List<Landing> { Coming("grain", "grain", 30, 300, 1f) };
            var curve = Projection.ShelfAhead(listed, null,                       "grain", "grain", 10, 40, 0.5f, null, 100);
            Assert.Equal(6f, Projection.RunsOutOf(curve, 20));
            var late = Projection.ShelfAhead(listed, null,                      "grain", "grain", 10, 40, 5.5f, null, 100);
            Assert.Equal(6f, Projection.RunsOutOf(late, 20));
        }

        [Fact]
        public void Units_landing_add_up_only_for_the_good_asked_about()
        {
            var listed = new List<Landing>
            {
                Coming("grain", "grain", 20, 400, 1f),
                Coming("wine", "wine", 5, 500, 1f),
                Coming("grain", "grain", 30, 600, 1f)
            };
            Assert.Equal(50, Projection.UnitsLanding(listed, "grain", 2f));
            Assert.Equal(5, Projection.UnitsLanding(listed, "wine", 2f));
            Assert.Equal(0, Projection.UnitsLanding(listed, "olives", 2f));
        }

        [Fact]
        public void A_load_still_on_the_road_past_the_window_is_left_out()
        {
            var listed = new List<Landing>
            {
                Coming("grain", "grain", 20, 400, 1f),
                Coming("grain", "grain", 30, 600, 5f)
            };
            Assert.Equal(20, Projection.UnitsLanding(listed, "grain", 2f));
            Assert.Equal(50, Projection.UnitsLanding(listed, "grain", 5f));
        }

        [Fact]
        public void On_the_price_path_a_load_landing_near_your_arrival_counts_in_part()
        {
            var listed = new List<Landing>
            {
                Coming("grain", "grain", 10, 400, 0.95f),
                Coming("grain", "grain", 10, 400, 1.1f)
            };
            Assert.Equal(10, Projection.UnitsLanding(listed, "grain", 1f));
            Assert.Equal(9, Projection.UnitsLanding(listed, "grain", 1f, graded: true));
            Assert.Equal(400, Projection.WorthLanding(listed, "grain", 1f));
            Assert.Equal(360, Projection.WorthLanding(listed, "grain", 1f, graded: true));
            var coming = new List<Spending> { Bought("grain", 1000, 1.1f) };
            Assert.Equal(0, Projection.WorthBought(coming, "grain", 1f));
            Assert.Equal(300, Projection.WorthBought(coming, "grain", 1f, graded: true));
        }

        [Fact]
        public void The_window_is_read_to_the_nearest_quarter_day()
        {
            var listed = new List<Landing> { Coming("grain", "grain", 20, 400, 0.6f) };
            Assert.Equal(0, Projection.UnitsLanding(listed, "grain", 0.6f));
            Assert.Equal(20, Projection.UnitsLanding(listed, "grain", 0.7f));
        }

        [Fact]
        public void A_load_an_hour_from_the_shelf_counts_while_you_are_still_minutes_away()
        {
            var listed = new List<Landing> { Coming("jewelry", "jewelry", 10, 8000, 1f / 24f) };
            Assert.Equal(10, Projection.UnitsLanding(listed, "jewelry", 0.12f, graded: true));
            Assert.Equal(8000, Projection.WorthLanding(listed, "jewelry", 0.12f, graded: true));
            Assert.Equal(10, Projection.UnitsLanding(listed, "jewelry", 0.13f, graded: true));
            var coming = new List<Spending> { Bought("jewelry", 1000, 1f / 24f) };
            Assert.Equal(1000, Projection.WorthBought(coming, "jewelry", 0.12f, graded: true));
        }

        [Fact]
        public void Nothing_read_and_nothing_asked_about_land_nothing()
        {
            Assert.Equal(0, Projection.UnitsLanding(null, "grain", 2f));
            Assert.Equal(0, Projection.UnitsLanding(new List<Landing>(), "grain", 2f));
            Assert.Equal(0, Projection.UnitsLanding(new List<Landing> { Coming("grain", "grain", 20, 400, 1f) },
                                                    null, 2f));
        }

        [Fact]
        public void Worth_landing_goes_by_the_kind_of_good_not_the_good()
        {
            var listed = new List<Landing>
            {
                Coming("grain", "grain", 20, 400, 1f),
                Coming(null, "grain", 30, 600, 1f),
                Coming("wine", "wine", 5, 500, 1f)
            };
            Assert.Equal(1000, Projection.WorthLanding(listed, "grain", 2f));
            Assert.Equal(500, Projection.WorthLanding(listed, "wine", 2f));
        }

        [Fact]
        public void A_workshop_run_written_against_the_good_the_town_holds_keeps_that_shelf_up()
        {
            var made = new List<Landing>();
            var unnamed = new List<Landing>();
            for (int day = 1; day <= 6; day++)
            {
                made.Add(Coming("wine", "wine", 2, 100, day));
                unnamed.Add(Coming(null, "wine", 2, 100, day));
            }
            var kept = Projection.ShelfAhead(made, null, "wine", "wine", 50, 12, 0.4f, null, 100);
            Assert.Equal(new[] { (1f, 12), (6f, 12), (7f, 10) }, new[] { kept[0], kept[5], kept[6] });
            Assert.Equal(7f, Projection.RunsOutOf(kept, 11));
            Assert.Equal(1f, Projection.RunsOutOf(
                Projection.ShelfAhead(unnamed, null, "wine", "wine", 50, 12, 0.4f, null, 100), 11));
            Assert.Equal(Projection.WorthLanding(unnamed, "wine", 3f), Projection.WorthLanding(made, "wine", 3f));
        }

        [Fact]
        public void Worth_landing_keeps_the_window_and_the_quarter_day_too()
        {
            var listed = new List<Landing>
            {
                Coming("grain", "grain", 20, 400, 0.6f),
                Coming("grain", "grain", 30, 600, 5f)
            };
            Assert.Equal(0, Projection.WorthLanding(listed, "grain", 0.6f));
            Assert.Equal(400, Projection.WorthLanding(listed, "grain", 0.7f));
            Assert.Equal(1000, Projection.WorthLanding(listed, "grain", 5f));
        }

        [Fact]
        public void Worth_landing_never_comes_back_below_nothing()
        {
            var listed = new List<Landing> { Coming("grain", "grain", 20, -900, 1f) };
            Assert.Equal(0, Projection.WorthLanding(listed, "grain", 2f));
        }

        [Fact]
        public void Nothing_read_and_no_kind_asked_about_land_no_worth()
        {
            Assert.Equal(0, Projection.WorthLanding(null, "grain", 2f));
            Assert.Equal(0, Projection.WorthLanding(new List<Landing> { Coming("grain", "grain", 20, 400, 1f) },
                                                   null, 2f));
        }

        [Fact]
        public void What_caravans_buy_on_the_road_adds_up_within_the_window()
        {
            var coming = new List<Spending> { Bought("grain", 500, 1f), Bought("grain", 300, 5f), Bought("grain", 200, 2f) };
            Assert.Equal(700, Projection.WorthBought(coming, "grain", 2f));
            Assert.Equal(1000, Projection.WorthBought(coming, "grain", 5f));
            Assert.Equal(0, Projection.WorthBought(null, "grain", 5f));
        }

        [Fact]
        public void The_window_for_what_caravans_buy_is_read_to_the_nearest_quarter_day_as_well()
        {
            var coming = new List<Spending> { Bought("grain", 500, 0.6f) };
            Assert.Equal(0, Projection.WorthBought(coming, "grain", 0.6f));
            Assert.Equal(500, Projection.WorthBought(coming, "grain", 0.7f));
        }

        [Fact]
        public void Only_the_kind_a_caravan_buys_leaves_the_shelf()
        {
            var coming = new List<Spending> { Bought("grain", 750, 1f), Bought("wine", 250, 1f) };
            Assert.Equal(750, Projection.WorthBought(coming, "grain", 2f));
            Assert.Equal(250, Projection.WorthBought(coming, "wine", 2f));
            Assert.Equal(0, Projection.WorthBought(coming, "olives", 2f));
            Assert.Equal(0, Projection.WorthBought(coming, null, 2f));
        }

        [Fact]
        public void A_caravan_that_buys_nothing_of_a_kind_takes_none_of_it()
        {
            var coming = new List<Spending> { Bought("grain", 0, 1f), Bought("grain", -500, 1f), Bought(null, 900, 1f) };
            Assert.Equal(0, Projection.WorthBought(coming, "grain", 2f));
        }

        [Fact]
        public void What_a_workshop_will_make_is_named_with_its_count()
        {
            Assert.Equal("", Projection.Named(null));
            Assert.Equal("", Projection.Named(new List<(string, int)>()));
            Assert.Equal("Wine x3", Projection.Named(new List<(string, int)> { ("Wine", 3) }));
            Assert.Equal("Wine x3, Grape x2",
                         Projection.Named(new List<(string, int)> { ("Wine", 3), ("Grape", 2) }));
        }

        [Fact]
        public void A_purse_takes_whole_units_off_the_shelf_and_never_part_of_one()
        {
            Assert.Equal(4, Projection.UnitsLeaving(400, 100));
            Assert.Equal(4, Projection.UnitsLeaving(499, 100));
            Assert.Equal(0, Projection.UnitsLeaving(99, 100));
        }

        [Fact]
        public void A_purse_of_nothing_and_a_good_worth_nothing_take_nothing()
        {
            Assert.Equal(0, Projection.UnitsLeaving(0, 100));
            Assert.Equal(0, Projection.UnitsLeaving(-400, 100));
            Assert.Equal(0, Projection.UnitsLeaving(400, 0));
            Assert.Equal(0, Projection.UnitsLeaving(400, -5));
        }

        [Fact]
        public void What_a_purse_takes_off_the_shelf_is_what_it_takes_off_the_worth()
        {
            var rng = new System.Random(8117);
            for (int round = 0; round < 20000; round++)
            {
                int unitValue = rng.Next(1, 400);
                int units = rng.Next(0, 500);
                int worth = units * unitValue + rng.Next(0, unitValue);
                Assert.Equal(units, Projection.UnitsLeaving(worth, unitValue));
            }
        }

        private static float RunsOut(IList<Landing> listed, IList<Spending> coming,
                                     string item, string category, int unitValue,
                                     int stockNow, int wanted, float afterDays) =>
            Projection.RunsOutOf(Projection.ShelfAhead(listed, coming, item, category,
                                                       unitValue, stockNow, afterDays), wanted);

        [Fact]
        public void A_shelf_nobody_is_coming_for_never_runs_out()
        {
            Assert.Equal(Projection.NeverRunsOut,
                RunsOut(new List<Landing>(), new List<Spending>(),
                        "grain", "grain", 10, 40, 20, 0.5f));
        }

        [Fact]
        public void A_shelf_runs_out_at_the_moment_a_caravan_takes_the_last_of_it()
        {
            var coming = new List<Spending> { Bought("grain", 150, 1f), Bought("grain", 150, 2f) };
            Assert.Equal(2f,
                RunsOut(new List<Landing>(), coming,
                        "grain", "grain", 10, 40, 20, 0.5f));
        }

        [Fact]
        public void A_load_landing_first_holds_the_shelf_up_past_a_caravan_buying()
        {
            var listed = new List<Landing> { Coming("grain", "grain", 30, 300, 0.75f) };
            var coming = new List<Spending> { Bought("grain", 300, 1f) };
            Assert.Equal(Projection.NeverRunsOut,
                RunsOut(listed, coming,
                        "grain", "grain", 10, 40, 20, 0.5f));
        }

        [Fact]
        public void Nothing_that_lands_before_you_arrive_can_expire_the_deal()
        {
            var coming = new List<Spending> { Bought("grain", 5000, 0.25f) };
            Assert.Equal(Projection.NeverRunsOut,
                RunsOut(new List<Landing>(), coming,
                        "grain", "grain", 10, 40, 20, 0.5f));
        }

        [Fact]
        public void A_deal_of_nothing_and_a_good_worth_nothing_never_expire()
        {
            var coming = new List<Spending> { Bought("grain", 5000, 2f) };
            Assert.Equal(Projection.NeverRunsOut,
                RunsOut(null, coming, "grain", "grain", 10, 40, 0, 0.5f));
            Assert.Equal(Projection.NeverRunsOut,
                RunsOut(null, coming, "grain", "grain", 0, 40, 20, 0.5f));
            Assert.Equal(Projection.NeverRunsOut,
                RunsOut(null, coming, null, "grain", 10, 40, 20, 0.5f));
        }

        [Fact]
        public void The_shelf_a_town_will_hold_is_the_same_whatever_size_deal_asks_for_it()
        {
            var listed = new List<Landing> { Coming("grain", "grain", 30, 300, 1.5f) };
            var coming = new List<Spending> { Bought("grain", 150, 1f), Bought("grain", 150, 2f) };
            var once = Projection.ShelfAhead(listed, coming,                      "grain", "grain", 10, 40, 0.5f);
            for (int wanted = 1; wanted <= 60; wanted++)
                Assert.Equal(
                    RunsOut(listed, coming,     "grain", "grain", 10, 40, wanted, 0.5f),
                    Projection.RunsOutOf(once, wanted));
        }

        [Fact]
        public void The_shelf_a_town_will_hold_is_read_once_for_every_moment_it_changes()
        {
            var listed = new List<Landing> { Coming("grain", "grain", 30, 300, 1.5f) };
            var coming = new List<Spending> { Bought("grain", 150, 1f), Bought("grain", 150, 2f) };
            var curve = Projection.ShelfAhead(listed, coming,                       "grain", "grain", 10, 40, 0.5f);
            Assert.Equal(Projection.Moments(listed, coming, 0.5f).Count, curve.Count);
            for (int i = 0; i < curve.Count; i++) Assert.True(curve[i].days > 0.5f);
        }

        [Fact]
        public void A_shelf_read_ahead_that_never_dips_hands_back_no_moment_at_all()
        {
            var curve = Projection.ShelfAhead(new List<Landing>(), new List<Spending>(),
                                              "grain", "grain", 10, 40, 0.5f);
            Assert.Empty(curve);
            Assert.Equal(Projection.NeverRunsOut, Projection.RunsOutOf(curve, 20));
            Assert.Equal(Projection.NeverRunsOut, Projection.RunsOutOf(null, 20));
        }

        [Fact]
        public void A_deal_of_nothing_never_expires_against_a_shelf_read_ahead()
        {
            var coming = new List<Spending> { Bought("grain", 5000, 2f) };
            var curve = Projection.ShelfAhead(null, coming,                       "grain", "grain", 10, 40, 0.5f);
            Assert.Equal(Projection.NeverRunsOut, Projection.RunsOutOf(curve, 0));
            Assert.Equal(Projection.NeverRunsOut, Projection.RunsOutOf(curve, -3));
            Assert.Empty(Projection.ShelfAhead(null, coming,                        "grain", "grain", 0, 40, 0.5f));
            Assert.Empty(Projection.ShelfAhead(null, coming,                        null, "grain", 10, 40, 0.5f));
        }

        [Fact]
        public void Every_moment_counted_is_after_you_arrive_and_on_the_quarter_day()
        {
            var listed = new List<Landing>
            {
                Coming("grain", "grain", 5, 50, 0.1f),
                Coming("grain", "grain", 5, 50, 0.8f),
                Coming("grain", "grain", 5, 50, 2.3f)
            };
            var coming = new List<Spending> { Bought("grain", 100, 0.8f), Bought("grain", 100, 3f) };
            var when = Projection.Moments(listed, coming, 0.5f);
            Assert.Equal(new List<float> { 1f, 2.5f, 3f }, when);
        }

        [Fact]
        public void A_moment_many_things_land_on_is_still_counted_once()
        {
            var listed = new List<Landing>();
            var coming = new List<Spending>();
            for (int i = 0; i < 500; i++)
            {
                listed.Add(Coming("grain", "grain", 1, 10, 1.2f));
                coming.Add(Bought("grain", 10, 1.2f));
            }
            listed.Add(Coming("grain", "grain", 1, 10, 4.1f));
            Assert.Equal(new List<float> { 1.25f, 4.25f }, Projection.Moments(listed, coming, 0.5f));
        }

        [Fact]
        public void A_shelf_that_runs_out_never_reports_a_moment_you_have_already_passed()
        {
            var rng = new System.Random(4471);
            for (int round = 0; round < 20000; round++)
            {
                float after = (float)(rng.NextDouble() * 3d);
                var coming = new List<Spending>();
                int purses = rng.Next(0, 5);
                for (int i = 0; i < purses; i++)
                    coming.Add(Bought("grain", rng.Next(1, 4000), (float)(rng.NextDouble() * 6d)));
                float at = RunsOut(null, coming,            "grain", "grain", rng.Next(1, 200),
                                   rng.Next(0, 200), rng.Next(1, 60), after);
                Assert.True(at == Projection.NeverRunsOut || at > after);
            }
        }

        [Fact]
        public void What_caravans_buy_never_adds_up_to_more_than_they_were_noted_to_buy()
        {
            var rng = new System.Random(3312);
            for (int round = 0; round < 20000; round++)
            {
                var coming = new List<Spending>();
                long noted = 0;
                int kinds = rng.Next(1, 8);
                for (int i = 0; i < rng.Next(0, 12); i++)
                {
                    int worth = rng.Next(1, 200000);
                    noted += worth;
                    coming.Add(Bought("c" + rng.Next(0, kinds), worth, (float)(rng.NextDouble() * 6d)));
                }
                float within = (float)(rng.NextDouble() * 8d);

                long handed = 0;
                for (int i = 0; i < kinds; i++) handed += Projection.WorthBought(coming, "c" + i, within, graded: true);

                Assert.True(handed <= noted);
            }
        }

        private static TownBook Town(int shops, int gold = 50000, int capital = 10000)
        {
            var town = new TownBook
            {
                Gold = gold, Capital = new int[shops], Expense = new int[shops], Warehouse = new int[shops]
            };
            for (int k = 0; k < shops; k++) town.Capital[k] = capital;
            return town;
        }

        private static void Shelve(TownBook town, string kind, int units, int value, int usedADay = 0)
        {
            town.Units[kind] = units;
            town.UnitValue[kind] = value;
            town.InStore[kind] = units * value;
            town.UsedADay[kind] = usedADay;
        }

        private static ShopLine Line(int shop, float progress, float speed, string input, string output,
                                     float pace = 5f)
        {
            var line = new ShopLine { Shop = shop, Progress = progress, Speed = speed, Pace = pace };
            line.Inputs.Add((input, 1));
            line.Outputs.Add((output, 1));
            return line;
        }

        private static int Flat(string kind, int store, bool selling) => kind == "wool" ? 20 : 150;

        private static (List<Landing> made, List<Draw> taken) Follow(List<ShopLine> lines, TownBook town,
                                                                     float first = 0.5f, float watch = 10f,
                                                                     Func<string, int, bool, int> price = null,
                                                                     List<Landing> arriving = null,
                                                                     List<(float, string, int)> bought = null)
        {
            var made = new List<Landing>();
            var taken = new List<Draw>();
            WorkshopRuns.Follow(lines, town, first, watch, arriving, bought, price ?? Flat, made, taken);
            return (made, taken);
        }

        private static int Drawn(List<Draw> taken)
        {
            int worth = 0;
            foreach (Draw one in taken) worth += one.Worth;
            return worth;
        }

        private static int Made(List<Landing> made, string kind)
        {
            int units = 0;
            foreach (Landing one in made) if (one.Category == kind) units += one.Units;
            return units;
        }

        [Fact]
        public void A_workshop_line_runs_at_every_daily_tick_its_pace_reaches_while_the_town_feeds_it()
        {
            TownBook town = Town(1);
            Shelve(town, "wool", 100, 20);
            Shelve(town, "cloth", 0, 150);
            var (made, taken) = Follow(new List<ShopLine> { Line(0, 0.5f, 1f, "wool", "cloth") }, town, 0.5f, 3.5f);
            Assert.Equal(4, Made(made, "cloth"));
            Assert.Equal(new[] { 0.5f, 1.5f, 2.5f, 3.5f }, made.ConvertAll(one => one.Days).ToArray());
            Assert.Equal(4 * 20, Drawn(taken));
        }

        [Fact]
        public void A_faster_line_runs_more_than_once_a_day_and_a_slower_one_skips_days()
        {
            TownBook town = Town(2);
            Shelve(town, "wool", 100, 20);
            Shelve(town, "cloth", 0, 150);
            var lines = new List<ShopLine> { Line(0, 0f, 2f, "wool", "cloth"), Line(1, 0f, 0.5f, "wool", "cloth") };
            var (made, _) = Follow(lines, town, 1f, 4f);
            Assert.Equal(4 * 2 + 2, Made(made, "cloth"));
        }

        [Fact]
        public void A_line_stops_for_the_day_at_the_first_run_it_cannot_feed_and_loses_that_progress()
        {
            TownBook town = Town(1);
            Shelve(town, "wool", 3, 20);
            Shelve(town, "cloth", 0, 150);
            var lines = new List<ShopLine> { Line(0, 0f, 2f, "wool", "cloth") };
            var (made, _) = Follow(lines, town, 1f, 3f);
            Assert.Equal(3, Made(made, "cloth"));
            Assert.Equal(0, town.Units["wool"]);
            Assert.Equal(1f, lines[0].Progress, 3);
        }

        [Fact]
        public void A_run_that_would_not_pay_the_game_its_margin_never_happens()
        {
            TownBook town = Town(1);
            Shelve(town, "wool", 100, 20);
            Shelve(town, "cloth", 0, 150);
            var slow = new List<ShopLine> { Line(0, 0f, 1f, "wool", "cloth", pace: 1f) };
            Assert.Equal(0, Made(Follow(slow, town, 1f, 3f).made, "cloth"));
            var hidden = new List<ShopLine> { Line(0, 0f, 1f, "wool", "cloth", pace: 1f) };
            hidden[0].Hidden = true;
            Assert.Equal(3, Made(Follow(hidden, town, 1f, 3f).made, "cloth"));
        }

        [Fact]
        public void A_town_short_of_gold_or_a_workshop_short_of_capital_runs_nothing_of_trade_goods()
        {
            TownBook poor = Town(1, gold: 100);
            Shelve(poor, "wool", 100, 20);
            Shelve(poor, "cloth", 0, 150);
            Assert.Equal(0, Made(Follow(new List<ShopLine> { Line(0, 0f, 1f, "wool", "cloth") }, poor, 1f, 3f).made, "cloth"));
            TownBook broke = Town(1, capital: 10);
            Shelve(broke, "wool", 100, 20);
            Shelve(broke, "cloth", 0, 150);
            Assert.Equal(0, Made(Follow(new List<ShopLine> { Line(0, 0f, 1f, "wool", "cloth") }, broke, 1f, 3f).made, "cloth"));
        }

        [Fact]
        public void The_first_workshop_in_the_town_gets_the_last_of_an_input_and_what_one_makes_feeds_the_next()
        {
            TownBook town = Town(3);
            Shelve(town, "wool", 1, 20);
            Shelve(town, "cloth", 0, 150);
            Shelve(town, "tunic", 0, 400);
            var lines = new List<ShopLine>
            {
                Line(0, 0f, 1f, "wool", "cloth"), Line(1, 0f, 1f, "wool", "cloth"), Line(2, 0f, 1f, "cloth", "tunic")
            };
            var (made, taken) = Follow(lines, town, 1f, 1f, (kind, store, selling) => kind == "wool" ? 20 : kind == "cloth" ? 150 : 400);
            Assert.Equal(1, Made(made, "cloth"));
            Assert.Equal(1, Made(made, "tunic"));
            Assert.Equal(0, town.Units["cloth"]);
        }

        [Fact]
        public void Goods_on_their_way_in_feed_a_workshop_and_what_caravans_buy_and_the_town_uses_starve_it()
        {
            TownBook fed = Town(1);
            Shelve(fed, "wool", 0, 20);
            Shelve(fed, "cloth", 0, 150);
            var arriving = new List<Landing> { new Landing { Category = "wool", Units = 2, Worth = 40, Days = 1.2f } };
            Assert.Equal(2, Made(Follow(new List<ShopLine> { Line(0, 0f, 1f, "wool", "cloth") }, fed, 1f, 4f,
                                        arriving: arriving).made, "cloth"));
            TownBook starved = Town(1);
            Shelve(starved, "wool", 4, 20, usedADay: 40);
            Shelve(starved, "cloth", 0, 150);
            var bought = new List<(float, string, int)> { (0.5f, "wool", 1) };
            Assert.Equal(1, Made(Follow(new List<ShopLine> { Line(0, 0f, 1f, "wool", "cloth") }, starved, 1f, 4f,
                                        bought: bought).made, "cloth"));
        }

        [Fact]
        public void Your_own_workshop_draws_on_its_warehouse_first_and_leaves_its_share_of_output_in_the_town()
        {
            TownBook town = Town(1);
            Shelve(town, "wool", 10, 20);
            Shelve(town, "cloth", 0, 150);
            town.Warehouse[0] = 2;
            ShopLine line = Line(0, 0f, 1f, "wool", "cloth");
            line.Yours = true;
            line.FromWarehouse = true;
            line.ToTown = 0.5f;
            var (made, taken) = Follow(new List<ShopLine> { line }, town, 1f, 4f);
            Assert.Equal(0, town.Warehouse[0]);
            Assert.Equal(2 * 20, Drawn(taken));
            Assert.Equal(2, Made(made, "cloth"));
        }

        [Fact]
        public void The_daily_expense_comes_off_a_workshop_until_its_capital_cannot_meet_it()
        {
            TownBook town = Town(1, capital: 25);
            town.Expense[0] = 10;
            Shelve(town, "wool", 0, 20);
            Shelve(town, "cloth", 0, 150);
            Follow(new List<ShopLine> { Line(0, 0f, 1f, "wool", "cloth") }, town, 1f, 4f);
            Assert.Equal(5, town.Capital[0]);
        }

        [Fact]
        public void The_workshops_are_followed_for_twice_your_travel_ceiling_and_a_day_within_bounds()
        {
            Assert.Equal(6f, WorkshopRuns.Watch(2.4f), 3);
            Assert.Equal(WorkshopRuns.LongestWatch, WorkshopRuns.Watch(0f), 3);
            Assert.Equal(WorkshopRuns.LongestWatch, WorkshopRuns.Watch(8f), 3);
            Assert.Equal(WorkshopRuns.ShortestWatch, WorkshopRuns.Watch(0.2f), 3);
            Assert.Equal(WorkshopRuns.LongestWatch, WorkshopRuns.Watch(float.NaN), 3);
        }

        [Fact]
        public void Nothing_is_followed_for_a_town_with_no_line_or_no_way_to_price()
        {
            TownBook town = Town(1);
            var (made, taken) = Follow(new List<ShopLine>(), town);
            Assert.Empty(made);
            Assert.Empty(taken);
            var none = new List<Landing>();
            WorkshopRuns.Follow(new List<ShopLine> { Line(0, 0f, 1f, "wool", "cloth") }, town, 1f, 3f, null, null, null,
                                none, new List<Draw>());
            Assert.Empty(none);
        }

        [Fact]
        public void A_workshop_whose_progress_cannot_be_read_is_taken_as_not_started_yet()
        {
            TownBook town = Town(1);
            Shelve(town, "wool", 100, 20);
            Shelve(town, "cloth", 0, 150);
            var lines = new List<ShopLine> { Line(0, float.NaN, 0.5f, "wool", "cloth") };
            var (made, _) = Follow(lines, town, 1f, 2f);
            Assert.Equal(1, Made(made, "cloth"));
            Assert.Equal(2f, made[0].Days, 3);
        }
    }
}
