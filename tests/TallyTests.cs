using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class TallyTests
    {
        [Fact]
        public void Every_reader_applied_is_said_plainly_with_nothing_about_refusals()
        {
            Assert.Equal("patches 12/12 applied", Tallies.Of(12, new List<string>()));
            Assert.Equal("patches 0/0 applied", Tallies.Of(0, new List<string>()));
            Assert.Equal("patches 3/3 applied", Tallies.Of(3, null));
        }

        [Fact]
        public void A_refused_reader_is_counted_in_the_total_and_named_after_it()
        {
            Assert.Equal("patches 11/12 applied, TooltipPatches refused",
                         Tallies.Of(11, new List<string> { "TooltipPatches" }));
        }

        [Fact]
        public void Every_refused_reader_is_named_in_the_order_it_was_refused()
        {
            Assert.Equal("patches 9/12 applied, OnePatch, TwoPatch, ThreePatch refused",
                         Tallies.Of(9, new List<string> { "OnePatch", "TwoPatch", "ThreePatch" }));
        }

        [Fact]
        public void A_reader_with_no_name_still_counts_against_the_total()
        {
            Assert.Equal("patches 1/2 applied,  refused", Tallies.Of(1, new List<string> { null }));
        }

        [Fact]
        public void Nothing_applied_and_everything_refused_still_reads_as_a_tally()
        {
            Assert.Equal("patches 0/2 applied, OnePatch, TwoPatch refused",
                         Tallies.Of(0, new List<string> { "OnePatch", "TwoPatch" }));
        }

        [Fact]
        public void A_count_that_cannot_be_right_never_reads_as_a_negative_tally()
        {
            Assert.Equal("patches 0/0 applied", Tallies.Of(-4, null));
            Assert.Equal("patches 0/1 applied, OnePatch refused", Tallies.Of(-1, new List<string> { "OnePatch" }));
        }

        [Fact]
        public void The_total_is_always_what_was_applied_and_what_was_refused_together()
        {
            var rng = new Random(6604);
            for (int round = 0; round < 20000; round++)
            {
                int applied = rng.Next(0, 40);
                int turned = rng.Next(0, 6);
                var refused = new List<string>();
                for (int i = 0; i < turned; i++) refused.Add("Patch" + i);
                string said = Tallies.Of(applied, refused);
                Assert.StartsWith("patches " + applied + "/" + (applied + turned) + " applied", said);
                Assert.Equal(turned > 0, said.EndsWith(" refused"));
                for (int i = 0; i < turned; i++) Assert.Contains("Patch" + i, said);
            }
        }
    }
}
