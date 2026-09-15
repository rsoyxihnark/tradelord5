using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class ScreenTests
    {
        [Fact]
        public void The_line_this_build_wants_is_built_from_its_number_alone()
        {
            Assert.Equal("MCMv5", Screens.Named(5));
            Assert.Equal("MCMv6", Screens.Named(6));
            Assert.Equal("MCMv12", Screens.Named(12));
        }

        [Fact]
        public void The_line_is_read_off_the_assembly_name_however_long_the_number_is()
        {
            Assert.Equal("MCMv5", Screens.GenerationOf("MCMv5"));
            Assert.Equal("MCMv5", Screens.GenerationOf("MCMv5.UI"));
            Assert.Equal("MCMv12", Screens.GenerationOf("MCMv12.Abstractions.Base"));
        }

        [Fact]
        public void An_assembly_that_is_not_an_MCM_line_at_all_is_passed_over()
        {
            Assert.Null(Screens.GenerationOf(null));
            Assert.Null(Screens.GenerationOf(""));
            Assert.Null(Screens.GenerationOf("Bannerlord.ButterLib"));
            Assert.Null(Screens.GenerationOf("TaleWorlds.CampaignSystem"));
            Assert.Null(Screens.GenerationOf("MCM"));
        }

        [Fact]
        public void An_MCM_name_with_no_number_after_it_is_not_a_line()
        {
            Assert.Null(Screens.GenerationOf("MCMv"));
            Assert.Null(Screens.GenerationOf("MCMv.Something"));
            Assert.Null(Screens.GenerationOf("MCMvNext"));
        }

        [Fact]
        public void The_line_is_recognised_whatever_case_the_assembly_was_named_in()
        {
            Assert.Equal("mcmv5", Screens.GenerationOf("mcmv5.ui"));
            Assert.Equal("McMv5", Screens.GenerationOf("McMv5"));
            Assert.Equal("MCMV5", Screens.GenerationOf("MCMV5.Abstractions"));
        }

        [Fact]
        public void The_line_this_build_was_made_for_wins_wherever_it_sits_in_the_list()
        {
            var loaded = new List<string>
            {
                "TaleWorlds.Core", "MCMv6.Abstractions", "Bannerlord.ButterLib", "MCMv5.UI", "MCMv4"
            };
            Assert.Equal("MCMv5", Screens.Which(loaded, "MCMv5"));
        }

        [Fact]
        public void A_line_this_build_was_not_made_for_is_reported_rather_than_passed_over()
        {
            var loaded = new List<string> { "TaleWorlds.Core", "MCMv6.Abstractions", "MCMv7.UI" };
            Assert.Equal("MCMv6", Screens.Which(loaded, "MCMv5"));
        }

        [Fact]
        public void The_first_other_line_found_is_the_one_reported()
        {
            Assert.Equal("MCMv7", Screens.Which(new[] { "MCMv7.UI", "MCMv6" }, "MCMv5"));
            Assert.Equal("MCMv6", Screens.Which(new[] { "MCMv6", "MCMv7.UI" }, "MCMv5"));
        }

        [Fact]
        public void Nothing_that_looks_like_MCM_at_all_leaves_the_loader_to_go_looking()
        {
            Assert.Null(Screens.Which(null, "MCMv5"));
            Assert.Null(Screens.Which(new string[0], "MCMv5"));
            Assert.Null(Screens.Which(new[] { "TaleWorlds.Core", "Bannerlord.UIExtenderEx", null }, "MCMv5"));
        }

        [Fact]
        public void The_line_this_build_wants_is_matched_whatever_case_it_loaded_under()
        {
            Assert.Equal("mcmv5", Screens.Which(new[] { "mcmv5.ui" }, "MCMv5"));
            Assert.Equal("MCMv5", Screens.Which(new[] { "MCMv5" }, "mcmv5"));
        }

        [Fact]
        public void What_the_loader_asks_for_is_always_what_it_takes_when_it_is_there()
        {
            var rng = new Random(3391);
            for (int round = 0; round < 20000; round++)
            {
                int wanted = rng.Next(1, 20);
                var loaded = new List<string>();
                int count = rng.Next(0, 6);
                bool there = false;
                for (int i = 0; i < count; i++)
                {
                    int line = rng.Next(1, 20);
                    if (line == wanted) there = true;
                    loaded.Add(Screens.Named(line) + (rng.Next(2) == 0 ? ".UI" : ""));
                }
                loaded.Add("TaleWorlds.CampaignSystem");
                string found = Screens.Which(loaded, Screens.Named(wanted));
                if (there) Assert.Equal(Screens.Named(wanted), Screens.GenerationOf(found));
                else if (count == 0) Assert.Null(found);
                else Assert.NotEqual(Screens.Named(wanted), found);
            }
        }
    }
}
