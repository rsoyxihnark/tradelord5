using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class PriceDriftTests
    {
        private static string Written(int buy, int sell, float day,
                                      int wasBuy, int wasSell, float wasDay) =>
            LedgerCodec.WriteLedger(new System.Collections.Generic.Dictionary<
                    string, System.Collections.Generic.List<PriceObservation>>
            {
                ["grain"] = new System.Collections.Generic.List<PriceObservation>
                {
                    new PriceObservation
                    {
                        ItemId = "grain", TownId = "town_S5", BuyPrice = buy, SellPrice = sell,
                        CapturedDay = day, WasBuyPrice = wasBuy, WasSellPrice = wasSell,
                        WasDay = wasDay
                    }
                }
            });

        private static PriceObservation Read(string text) =>
            LedgerCodec.ReadLedger(text)["grain"][0];

        [Fact]
        public void A_price_that_climbed_since_your_last_look_is_rising()
        {
            Assert.Equal(1, TradeMath.Drift(120, 100));
        }

        [Fact]
        public void A_price_that_dropped_since_your_last_look_is_falling()
        {
            Assert.Equal(-1, TradeMath.Drift(80, 100));
        }

        [Fact]
        public void A_wobble_too_small_to_mean_anything_says_nothing()
        {
            Assert.Equal(0, TradeMath.Drift(102, 100));
            Assert.Equal(0, TradeMath.Drift(98, 100));
            Assert.Equal(0, TradeMath.Drift(100, 100));
        }

        [Fact]
        public void A_move_of_exactly_the_share_worth_saying_is_said()
        {
            Assert.Equal(1, TradeMath.Drift(105, 100));
            Assert.Equal(-1, TradeMath.Drift(95, 100));
        }

        [Fact]
        public void A_price_of_nothing_at_either_end_is_never_a_direction()
        {
            Assert.Equal(0, TradeMath.Drift(0, 100));
            Assert.Equal(0, TradeMath.Drift(100, 0));
            Assert.Equal(0, TradeMath.Drift(-5, 100));
        }

        [Fact]
        public void A_second_look_on_the_same_day_is_not_a_second_reading()
        {
            Assert.False(TradeMath.ReadingIsNew(134.25f, 134.25f));
            Assert.False(TradeMath.ReadingIsNew(134.9f, 134.25f));
        }

        [Fact]
        public void A_look_a_day_later_is_a_second_reading()
        {
            Assert.True(TradeMath.ReadingIsNew(135.25f, 134.25f));
            Assert.True(TradeMath.ReadingIsNew(200f, 134.25f));
        }

        [Fact]
        public void A_market_you_have_seen_only_once_carries_no_earlier_reading()
        {
            var fresh = new PriceObservation { ItemId = "grain", TownId = "town_S5" };
            Assert.False(fresh.SeenBefore);
            Assert.Equal(PriceObservation.NoEarlierReading, fresh.WasDay);
        }

        [Fact]
        public void A_campaign_saved_now_carries_the_earlier_reading_back()
        {
            PriceObservation back = Read(Written(17, 23, 134.25f, 14, 19, 121.5f));
            Assert.True(back.SeenBefore);
            Assert.Equal(14, back.WasBuyPrice);
            Assert.Equal(19, back.WasSellPrice);
            Assert.Equal(121.5f, back.WasDay, 3);
            Assert.Equal(1, TradeMath.Drift(back.SellPrice, back.WasSellPrice));
        }

        [Fact]
        public void A_campaign_saved_before_this_version_loads_with_no_earlier_reading()
        {
            PriceObservation back = Read("grain|town_S5|17|23|134.25");
            Assert.Equal(17, back.BuyPrice);
            Assert.Equal(23, back.SellPrice);
            Assert.Equal(134.25f, back.CapturedDay, 3);
            Assert.False(back.SeenBefore);
        }

        [Fact]
        public void An_earlier_reading_that_cannot_be_read_costs_only_the_history()
        {
            PriceObservation back = Read("grain|town_S5|17|23|134.25|rubbish|19|121.5");
            Assert.Equal(17, back.BuyPrice);
            Assert.Equal(23, back.SellPrice);
            Assert.False(back.SeenBefore);
        }

        [Fact]
        public void A_record_a_newer_TradeLord_wrote_keeps_every_field_this_one_understands()
        {
            var back = LedgerCodec.ReadLedger(
                "grain|town_S5|17|23|134.25|14|19|121.5|99|something", out int unreadable);

            Assert.Equal(0, unreadable);
            PriceObservation one = back["grain"][0];
            Assert.Equal(17, one.BuyPrice);
            Assert.Equal(23, one.SellPrice);
            Assert.Equal(134.25f, one.CapturedDay, 3);
            Assert.Equal(14, one.WasBuyPrice);
            Assert.Equal(19, one.WasSellPrice);
            Assert.Equal(121.5f, one.WasDay, 3);
            Assert.True(one.SeenBefore);
        }

        [Fact]
        public void A_record_too_short_to_hold_a_price_keeps_the_current_price_it_does_hold()
        {
            var back = LedgerCodec.ReadLedger("grain|town_S5|17|23|134.25|14", out int unreadable);

            Assert.Equal(0, unreadable);
            PriceObservation one = back["grain"][0];
            Assert.Equal(17, one.BuyPrice);
            Assert.Equal(23, one.SellPrice);
            Assert.False(one.SeenBefore);
        }

        [Fact]
        public void A_record_with_too_few_fields_to_read_at_all_is_dropped_and_counted()
        {
            Assert.Empty(LedgerCodec.ReadLedger("grain|town_S5|17|23", out int unreadable));
            Assert.Equal(1, unreadable);
            Assert.Empty(LedgerCodec.ReadLedger("grain", out unreadable));
            Assert.Equal(1, unreadable);
        }

        [Fact]
        public void A_ledger_that_reads_whole_reports_nothing_it_could_not_read()
        {
            LedgerCodec.ReadLedger(Written(17, 23, 134.25f, 14, 19, 121.5f), out int unreadable);
            Assert.Equal(0, unreadable);
            LedgerCodec.ReadLedger("", out unreadable);
            Assert.Equal(0, unreadable);
            LedgerCodec.ReadLedger(null, out unreadable);
            Assert.Equal(0, unreadable);
        }

        [Fact]
        public void A_stray_separator_in_the_saved_text_is_not_a_price_that_could_not_be_read()
        {
            var back = LedgerCodec.ReadLedger("grain|town_S5|17|23|134.25;", out int unreadable);

            Assert.Equal(0, unreadable);
            Assert.Equal(17, back["grain"][0].BuyPrice);
        }

        [Fact]
        public void Every_record_a_save_holds_that_cannot_be_read_is_counted_on_its_own()
        {
            var back = LedgerCodec.ReadLedger(
                "grain|town_S5|17|23|134.25;broken;wine|town_A1|5|9|2.5;also|broken",
                out int unreadable);

            Assert.Equal(2, unreadable);
            Assert.Equal(2, back.Count);
            Assert.Equal(17, back["grain"][0].BuyPrice);
            Assert.Equal(5, back["wine"][0].BuyPrice);
        }

        [Fact]
        public void The_fields_a_price_needs_and_the_fields_it_is_written_in_are_named()
        {
            Assert.Equal(5, LedgerCodec.FieldsAPriceNeeds);
            Assert.Equal(8, LedgerCodec.FieldsAPriceIsWrittenIn);
        }

        [Fact]
        public void An_earlier_reading_survives_a_save_and_a_load_and_another_save()
        {
            string once = Written(17, 23, 134.25f, 14, 19, 121.5f);
            string twice = LedgerCodec.WriteLedger(LedgerCodec.ReadLedger(once));
            Assert.Equal(once, twice);
        }
    
        [Fact]
        public void A_price_you_recorded_today_is_kept()
        {
            Assert.True(TradeMath.WorthKeeping(100f, 100f, 15));
            Assert.True(TradeMath.WorthKeeping(100f, 100.5f, 15));
        }

        [Fact]
        public void A_price_is_kept_through_its_fifteenth_day_and_forgotten_on_the_sixteenth()
        {
            Assert.True(TradeMath.WorthKeeping(100f, 115f, 15));
            Assert.False(TradeMath.WorthKeeping(100f, 115.5f, 15));
            Assert.False(TradeMath.WorthKeeping(100f, 116f, 15));
            Assert.False(TradeMath.WorthKeeping(100f, 400f, 15));
        }

        [Fact]
        public void A_shelf_life_of_nothing_keeps_every_price_for_as_long_as_the_campaign_lasts()
        {
            Assert.Equal(0, TradeMath.KeptForever);
            Assert.True(TradeMath.WorthKeeping(100f, 100000f, TradeMath.KeptForever));
            Assert.True(TradeMath.WorthKeeping(100f, 100000f, -5));
        }

        [Fact]
        public void A_shelf_life_of_one_day_forgets_a_price_the_day_after_you_saw_it()
        {
            Assert.True(TradeMath.WorthKeeping(100f, 101f, 1));
            Assert.False(TradeMath.WorthKeeping(100f, 101.25f, 1));
        }

        [Fact]
        public void A_price_recorded_later_than_the_clock_says_is_never_thrown_away()
        {
            Assert.True(TradeMath.WorthKeeping(200f, 100f, 15));
        }
    }
}
