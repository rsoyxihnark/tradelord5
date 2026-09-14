using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class ProjectionTests
    {
        private static Landing Coming(string item, string category, int units, int worth, float days) =>
            new Landing { Item = item, Category = category, Units = units, Worth = worth, Days = days };

        private static Spending Purse(int gold, float days) =>
            new Spending { Gold = gold, Days = days };

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
        public void The_window_is_read_to_the_nearest_quarter_day()
        {
            var listed = new List<Landing> { Coming("grain", "grain", 20, 400, 0.6f) };
            Assert.Equal(0, Projection.UnitsLanding(listed, "grain", 0.6f));
            Assert.Equal(20, Projection.UnitsLanding(listed, "grain", 0.7f));
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
        public void The_purse_on_the_road_adds_up_within_the_window()
        {
            var coming = new List<Spending> { Purse(500, 1f), Purse(300, 5f), Purse(200, 2f) };
            Assert.Equal(700, Projection.PurseLanding(coming, 2f));
            Assert.Equal(1000, Projection.PurseLanding(coming, 5f));
            Assert.Equal(0, Projection.PurseLanding(null, 5f));
        }

        [Fact]
        public void The_purse_window_is_read_to_the_nearest_quarter_day_as_well()
        {
            var coming = new List<Spending> { Purse(500, 0.6f) };
            Assert.Equal(0, Projection.PurseLanding(coming, 0.6f));
            Assert.Equal(500, Projection.PurseLanding(coming, 0.7f));
        }

        [Fact]
        public void What_leaves_the_shelf_is_the_purse_share_the_kind_of_good_pulls()
        {
            var pull = new Dictionary<string, float> { { "grain", 0.6f }, { "wine", 0.2f } };
            float across = Projection.PullAcross(pull);
            Assert.Equal(0.8f, across, 4);
            Assert.Equal(750, Projection.WorthLeaving(1000, pull, across, "grain"));
            Assert.Equal(250, Projection.WorthLeaving(1000, pull, across, "wine"));
        }

        [Fact]
        public void A_kind_of_good_no_trader_would_pick_has_nothing_leaving()
        {
            var pull = new Dictionary<string, float> { { "grain", 0.6f } };
            Assert.Equal(0, Projection.WorthLeaving(1000, pull, 0.6f, "olives"));
            Assert.Equal(0, Projection.WorthLeaving(1000, pull, 0.6f, null));
            Assert.Equal(0, Projection.WorthLeaving(1000, null, 0.6f, "grain"));
        }

        [Fact]
        public void An_empty_purse_leaves_nothing_whatever_the_pull_says()
        {
            var pull = new Dictionary<string, float> { { "grain", 0.6f } };
            Assert.Equal(0, Projection.WorthLeaving(0, pull, 0.6f, "grain"));
            Assert.Equal(0, Projection.WorthLeaving(-500, pull, 0.6f, "grain"));
        }

        [Fact]
        public void A_market_with_no_pull_at_all_is_not_read()
        {
            Assert.Equal(0f, Projection.PullAcross(null));
            Assert.Equal(0f, Projection.PullAcross(new Dictionary<string, float>()));
            Assert.False(Projection.PullReadable(null, 1f));
            Assert.False(Projection.PullReadable(new Dictionary<string, float>(), 0f));
            Assert.True(Projection.PullReadable(new Dictionary<string, float> { { "grain", 0.5f } }, 0.5f));
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
        public void A_workshop_run_is_measured_over_one_day()
        {
            Assert.Equal(1f, Projection.WorkshopRunDays);
            Assert.Equal(0.25f, TradeMath.RunLandsIn(0.75f, Projection.WorkshopRunDays), 4);
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

        [Fact]
        public void A_purse_split_across_a_market_never_hands_out_more_than_the_purse()
        {
            var rng = new System.Random(3312);
            for (int round = 0; round < 20000; round++)
            {
                var pull = new Dictionary<string, float>();
                int kinds = rng.Next(1, 8);
                for (int i = 0; i < kinds; i++) pull["c" + i] = (float)rng.NextDouble();
                float across = Projection.PullAcross(pull);
                int purse = rng.Next(1, 2000000);

                long handed = 0;
                foreach (var kind in pull) handed += Projection.WorthLeaving(purse, pull, across, kind.Key);

                Assert.True(handed <= purse + kinds);
            }
        }
    }
}
