using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class PromiseTests
    {
        private static PromiseRecord After(params float[] held)
        {
            var rec = new PromiseRecord { TownId = "town_A1" };
            foreach (float one in held) TradeMath.AddPromise(rec, one);
            return rec;
        }

        [Fact]
        public void A_market_nobody_has_walked_into_has_no_record_to_give()
        {
            Assert.Equal(TradeMath.NoShareToGive, TradeMath.PromiseMean(After()));
            Assert.Equal(TradeMath.NoShareToGive, TradeMath.PromiseMean(null));
        }

        [Fact]
        public void What_a_market_paid_is_the_mean_of_every_arrival_there()
        {
            Assert.Equal(0.8f, TradeMath.PromiseMean(After(0.6f, 1.0f)), 4);
        }

        [Fact]
        public void A_reading_that_is_not_a_number_is_never_written_down()
        {
            PromiseRecord rec = After(0.5f);
            TradeMath.AddPromise(rec, float.NaN);
            TradeMath.AddPromise(rec, float.PositiveInfinity);
            TradeMath.AddPromise(rec, -1f);
            Assert.Equal(1, rec.Scored);
        }

        [Fact]
        public void Too_few_arrivals_leave_a_score_exactly_where_it_was()
        {
            Assert.Equal(100f, Confidence.AsPromisesHaveHeld(100f, Confidence.EnoughArrivals - 1, 0.5f));
        }

        [Fact]
        public void A_market_that_has_kept_its_promises_leaves_a_score_where_it_was()
        {
            Assert.Equal(100f, Confidence.AsPromisesHaveHeld(100f, 40, 1f), 3);
        }

        [Fact]
        public void Paying_above_the_promise_is_never_a_bonus()
        {
            Assert.Equal(100f, Confidence.AsPromisesHaveHeld(100f, 40, 1.8f), 3);
        }

        [Fact]
        public void A_market_that_has_paid_less_than_promised_scores_lower()
        {
            float moved = Confidence.AsPromisesHaveHeld(100f, 40, 0.6f);
            Assert.True(moved < 100f);
            Assert.True(moved > 0f);
        }

        [Fact]
        public void More_arrivals_make_the_same_shortfall_count_for_more()
        {
            float few = Confidence.AsPromisesHaveHeld(100f, Confidence.EnoughArrivals, 0.6f);
            float many = Confidence.AsPromisesHaveHeld(100f, 200, 0.6f);
            Assert.True(many < few);
        }

        [Fact]
        public void A_shortfall_can_never_move_a_score_by_more_than_the_ceiling_it_is_held_to()
        {
            float floor = 100f * (1f - Confidence.MostItDiscounts);
            Assert.Equal(floor, Confidence.AsPromisesHaveHeld(100f, 5000, 0f), 3);
            Assert.True(Confidence.AsPromisesHaveHeld(100f, 5000, 0.01f) >= floor);
        }

        [Fact]
        public void A_score_of_nothing_stays_nothing()
        {
            Assert.Equal(0f, Confidence.AsPromisesHaveHeld(0f, 40, 0.2f));
        }

        [Fact]
        public void A_reading_that_is_not_a_number_leaves_a_score_alone()
        {
            Assert.Equal(100f, Confidence.AsPromisesHaveHeld(100f, 40, float.NaN));
            Assert.Equal(100f, Confidence.AsPromisesHaveHeld(100f, 40, float.PositiveInfinity));
            Assert.Equal(100f, Confidence.AsPromisesHaveHeld(100f, 40, -1f));
        }

        [Theory]
        [InlineData("")]
        [InlineData("town_A1|3")]
        [InlineData("town_A1|0|2.4")]
        [InlineData("town_A1|3|nonsense")]
        [InlineData("town_A1|3|-4")]
        public void A_record_this_version_cannot_read_is_passed_over(string written)
        {
            Assert.Empty(LedgerCodec.ReadPromises(written));
        }

        [Fact]
        public void What_a_market_paid_survives_a_save_and_a_load()
        {
            var written = new System.Collections.Generic.List<PromiseRecord>
            {
                After(0.6f, 1.0f), new PromiseRecord { TownId = "town_B2", Scored = 7, Held = 5.25f },
            };
            var read = LedgerCodec.ReadPromises(LedgerCodec.WritePromises(written));
            Assert.Equal(2, read.Count);
            Assert.Equal("town_A1", read[0].TownId);
            Assert.Equal(2, read[0].Scored);
            Assert.Equal(1.6f, read[0].Held, 3);
            Assert.Equal(7, read[1].Scored);
            Assert.Equal(5.25f, read[1].Held, 3);
        }

        [Fact]
        public void A_record_written_by_a_newer_TradeLord_is_read_as_far_as_this_one_understands_it()
        {
            var read = LedgerCodec.ReadPromises("town_A1|4|3.2|something|else");
            Assert.Single(read);
            Assert.Equal(4, read[0].Scored);
            Assert.Equal(3.2f, read[0].Held, 3);
        }

        [Fact]
        public void A_town_id_that_would_break_the_record_is_never_written()
        {
            var written = new System.Collections.Generic.List<PromiseRecord>
            {
                new PromiseRecord { TownId = "town|A1", Scored = 2, Held = 1f },
                new PromiseRecord { TownId = "town;A1", Scored = 2, Held = 1f },
                new PromiseRecord { TownId = null, Scored = 2, Held = 1f },
            };
            Assert.Equal("", LedgerCodec.WritePromises(written));
        }
    }
}
