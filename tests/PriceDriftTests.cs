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
        public void A_record_of_a_length_this_version_never_wrote_is_dropped()
        {
            Assert.Empty(LedgerCodec.ReadLedger("grain|town_S5|17|23|134.25|14"));
            Assert.Empty(LedgerCodec.ReadLedger("grain|town_S5|17|23|134.25|14|19"));
        }

        [Fact]
        public void An_earlier_reading_survives_a_save_and_a_load_and_another_save()
        {
            string once = Written(17, 23, 134.25f, 14, 19, 121.5f);
            string twice = LedgerCodec.WriteLedger(LedgerCodec.ReadLedger(once));
            Assert.Equal(once, twice);
        }
    }
}
