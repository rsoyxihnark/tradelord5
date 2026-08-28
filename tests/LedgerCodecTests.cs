using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class LedgerCodecTests
    {
        private static Dictionary<string, List<PriceObservation>> SampleLedger() =>
            new Dictionary<string, List<PriceObservation>>
            {
                ["grain"] = new List<PriceObservation>
                {
                    new PriceObservation { ItemId = "grain", TownId = "town_S5", BuyPrice = 17, SellPrice = 23, CapturedDay = 134.25f },
                    new PriceObservation { ItemId = "grain", TownId = "town_V2", BuyPrice = 9, SellPrice = 41, CapturedDay = 0f }
                },
                ["hardwood"] = new List<PriceObservation>
                {
                    new PriceObservation { ItemId = "hardwood", TownId = "town_A1", BuyPrice = 0, SellPrice = int.MaxValue, CapturedDay = 1234.567f }
                }
            };

        private static List<PurchaseRecord> SamplePurchases() =>
            new List<PurchaseRecord>
            {
                new PurchaseRecord { ItemId = "grain", TotalPaid = 340, Count = 20, LastUnitPaid = 17 },
                new PurchaseRecord { ItemId = "silver", TotalPaid = int.MaxValue, Count = 1, LastUnitPaid = -5 }
            };

        private static void InCulture(string name, Action body)
        {
            CultureInfo was = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(name);
                body();
            }
            finally { Thread.CurrentThread.CurrentCulture = was; }
        }

        [Fact]
        public void A_ledger_survives_a_save_and_a_load_unchanged()
        {
            var back = LedgerCodec.ReadLedger(LedgerCodec.WriteLedger(SampleLedger()));

            Assert.Equal(2, back.Count);
            Assert.Equal(2, back["grain"].Count);
            Assert.Equal("grain", back["grain"][0].ItemId);
            Assert.Equal("town_S5", back["grain"][0].TownId);
            Assert.Equal(17, back["grain"][0].BuyPrice);
            Assert.Equal(23, back["grain"][0].SellPrice);
            Assert.Equal(134.25f, back["grain"][0].CapturedDay, 3);
            Assert.Equal(int.MaxValue, back["hardwood"][0].SellPrice);
            Assert.Equal(0, back["hardwood"][0].BuyPrice);
        }

        [Fact]
        public void Purchase_records_survive_a_save_and_a_load_unchanged()
        {
            var back = LedgerCodec.ReadPurchases(LedgerCodec.WritePurchases(SamplePurchases()));

            Assert.Equal(2, back.Count);
            Assert.Equal("grain", back[0].ItemId);
            Assert.Equal(340, back[0].TotalPaid);
            Assert.Equal(20, back[0].Count);
            Assert.Equal(17, back[0].LastUnitPaid);
            Assert.Equal(int.MaxValue, back[1].TotalPaid);
            Assert.Equal(-5, back[1].LastUnitPaid);
        }

        [Fact]
        public void Saving_the_same_campaign_again_writes_the_same_text()
        {
            string ledger = LedgerCodec.WriteLedger(SampleLedger());
            string purchases = LedgerCodec.WritePurchases(SamplePurchases());

            for (int generation = 0; generation < 3; generation++)
            {
                string nextLedger = LedgerCodec.WriteLedger(LedgerCodec.ReadLedger(ledger));
                string nextPurchases = LedgerCodec.WritePurchases(LedgerCodec.ReadPurchases(purchases));
                Assert.Equal(ledger, nextLedger);
                Assert.Equal(purchases, nextPurchases);
                ledger = nextLedger;
                purchases = nextPurchases;
            }
        }

        [Theory]
        [InlineData("de-DE")]
        [InlineData("fr-FR")]
        [InlineData("tr-TR")]
        [InlineData("ar-SA")]
        [InlineData("fa-IR")]
        [InlineData("hi-IN")]
        [InlineData("sv-SE")]
        [InlineData("ru-RU")]
        public void A_campaign_reads_the_same_in_every_language(string culture)
        {
            string english = LedgerCodec.WriteLedger(SampleLedger());
            string englishPurchases = LedgerCodec.WritePurchases(SamplePurchases());

            InCulture(culture, () =>
            {
                Assert.Equal(english, LedgerCodec.WriteLedger(SampleLedger()));
                Assert.Equal(englishPurchases, LedgerCodec.WritePurchases(SamplePurchases()));

                var back = LedgerCodec.ReadLedger(english);
                Assert.Equal(17, back["grain"][0].BuyPrice);
                Assert.Equal(134.25f, back["grain"][0].CapturedDay, 3);
                Assert.Equal(340, LedgerCodec.ReadPurchases(englishPurchases)[0].TotalPaid);
            });
        }

        [Fact]
        public void A_campaign_written_in_one_language_reads_back_in_another()
        {
            string written = null;
            InCulture("de-DE", () => written = LedgerCodec.WriteLedger(SampleLedger()));

            InCulture("tr-TR", () =>
            {
                var back = LedgerCodec.ReadLedger(written);
                Assert.Equal(2, back.Count);
                Assert.Equal(1234.567f, back["hardwood"][0].CapturedDay, 3);
            });
        }

        [Theory]
        [InlineData("")]
        [InlineData(";")]
        [InlineData(";;;")]
        [InlineData("grain")]
        [InlineData("grain|town")]
        [InlineData("grain|town|1|2")]
        [InlineData("grain|town|1|2|3|4")]
        [InlineData("|town|1|2|3")]
        [InlineData("grain||1|2|3")]
        [InlineData("grain|town|x|2|3")]
        [InlineData("grain|town|1|y|3")]
        [InlineData("grain|town|1|2|z")]
        [InlineData("grain|town|99999999999999999999|2|3")]
        [InlineData("grain|town|1|2|NaN")]
        [InlineData("grain|town|1|2|Infinity")]
        [InlineData("grain|town|1|2|1e400")]
        [InlineData("grain|town|1|2|3.5.5")]
        [InlineData("grain|town|-1|-2|-3")]
        [InlineData("grain|town| 1 | 2 |3")]
        [InlineData("\t\n\r")]
        public void One_unreadable_record_does_not_cost_the_rest_of_the_ledger(string junk)
        {
            const string good = "wine|town_S5|17|23|134.25";

            var back = LedgerCodec.ReadLedger(junk + ";" + good);

            Assert.True(back.ContainsKey("wine"));
            Assert.Single(back["wine"]);
            Assert.Equal(17, back["wine"][0].BuyPrice);
        }

        [Theory]
        [InlineData("")]
        [InlineData(";;")]
        [InlineData("grain|340|20")]
        [InlineData("grain|340|20|17|9")]
        [InlineData("|340|20|17")]
        [InlineData("grain|x|20|17")]
        [InlineData("grain|340|0|17")]
        [InlineData("grain|340|-3|17")]
        [InlineData("grain|99999999999999999999|20|17")]
        public void One_unreadable_purchase_does_not_cost_the_rest(string junk)
        {
            const string good = "wine|500|10|50";

            var back = LedgerCodec.ReadPurchases(junk + ";" + good);

            Assert.Contains(back, r => r.ItemId == "wine" && r.Count == 10 && r.TotalPaid == 500);
        }

        [Fact]
        public void A_very_long_run_of_rubbish_is_read_without_complaint()
        {
            string junk = new string('x', 200_000);

            var back = LedgerCodec.ReadLedger(junk + ";wine|town_S5|17|23|134.25");

            Assert.Single(back);
            Assert.True(back.ContainsKey("wine"));
        }

        [Fact]
        public void An_item_name_holding_a_separator_is_left_out_rather_than_split()
        {
            var ledger = new Dictionary<string, List<PriceObservation>>
            {
                ["od|d"] = new List<PriceObservation> { new PriceObservation { ItemId = "od|d", TownId = "t1", BuyPrice = 5, SellPrice = 6, CapturedDay = 1f } },
                ["se;mi"] = new List<PriceObservation> { new PriceObservation { ItemId = "se;mi", TownId = "t2", BuyPrice = 7, SellPrice = 8, CapturedDay = 2f } },
                ["clean"] = new List<PriceObservation> { new PriceObservation { ItemId = "clean", TownId = "t3", BuyPrice = 9, SellPrice = 10, CapturedDay = 3f } }
            };

            var back = LedgerCodec.ReadLedger(LedgerCodec.WriteLedger(ledger));

            Assert.Single(back);
            Assert.True(back.ContainsKey("clean"));
            Assert.DoesNotContain(back.Keys, k => k == "mi" || k == "se" || k == "od" || k == "d");
        }

        [Fact]
        public void A_town_name_holding_a_separator_is_left_out_rather_than_split()
        {
            var ledger = new Dictionary<string, List<PriceObservation>>
            {
                ["grain"] = new List<PriceObservation>
                {
                    new PriceObservation { ItemId = "grain", TownId = "to;wn", BuyPrice = 5, SellPrice = 6, CapturedDay = 1f },
                    new PriceObservation { ItemId = "grain", TownId = "to|wn", BuyPrice = 7, SellPrice = 8, CapturedDay = 2f },
                    new PriceObservation { ItemId = "grain", TownId = "town_S5", BuyPrice = 9, SellPrice = 10, CapturedDay = 3f }
                }
            };

            var back = LedgerCodec.ReadLedger(LedgerCodec.WriteLedger(ledger));

            Assert.Single(back);
            Assert.Single(back["grain"]);
            Assert.Equal("town_S5", back["grain"][0].TownId);
        }

        [Fact]
        public void A_purchase_for_an_item_whose_name_holds_a_separator_is_left_out()
        {
            var purchases = new List<PurchaseRecord>
            {
                new PurchaseRecord { ItemId = "od|d", TotalPaid = 10, Count = 1, LastUnitPaid = 10 },
                new PurchaseRecord { ItemId = "se;mi", TotalPaid = 20, Count = 2, LastUnitPaid = 10 },
                new PurchaseRecord { ItemId = "clean", TotalPaid = 30, Count = 3, LastUnitPaid = 10 }
            };

            var back = LedgerCodec.ReadPurchases(LedgerCodec.WritePurchases(purchases));

            Assert.Single(back);
            Assert.Equal("clean", back[0].ItemId);
        }

        [Fact]
        public void Everything_written_can_be_read_back_whatever_the_names_are()
        {
            var names = new[] { "grain", "od|d", "se;mi", "", null, "a|b;c", "hardwood", " spaced ", "üñî" };
            var random = new Random(20260828);
            var ledger = new Dictionary<string, List<PriceObservation>>();
            for (int i = 0; i < names.Length; i++)
            {
                var list = new List<PriceObservation>();
                for (int t = 0; t < 4; t++)
                    list.Add(new PriceObservation
                    {
                        ItemId = names[i],
                        TownId = names[(i + t) % names.Length],
                        BuyPrice = random.Next(-5, 5000),
                        SellPrice = random.Next(-5, 5000),
                        CapturedDay = (float)Math.Round(random.NextDouble() * 5000.0, 3)
                    });
                if (names[i] != null) ledger[names[i]] = list;
            }

            string written = LedgerCodec.WriteLedger(ledger);
            var back = LedgerCodec.ReadLedger(written);

            Assert.Equal(written, LedgerCodec.WriteLedger(back));
            Assert.All(back.Keys, k => Assert.DoesNotContain('|', k));
            Assert.All(back.Keys, k => Assert.DoesNotContain(';', k));
            Assert.All(back.Values.SelectMany(v => v), o => Assert.Equal(o.ItemId, back.First(kv => kv.Value.Contains(o)).Key));
        }

        [Fact]
        public void A_day_that_is_not_a_real_number_is_left_out()
        {
            var ledger = new Dictionary<string, List<PriceObservation>>
            {
                ["grain"] = new List<PriceObservation>
                {
                    new PriceObservation { ItemId = "grain", TownId = "t1", BuyPrice = 1, SellPrice = 2, CapturedDay = float.NaN },
                    new PriceObservation { ItemId = "grain", TownId = "t2", BuyPrice = 3, SellPrice = 4, CapturedDay = float.PositiveInfinity },
                    new PriceObservation { ItemId = "grain", TownId = "t3", BuyPrice = 5, SellPrice = 6, CapturedDay = float.NegativeInfinity },
                    new PriceObservation { ItemId = "grain", TownId = "t4", BuyPrice = 7, SellPrice = 8, CapturedDay = 12.5f }
                }
            };

            var back = LedgerCodec.ReadLedger(LedgerCodec.WriteLedger(ledger));

            Assert.Single(back["grain"]);
            Assert.Equal("t4", back["grain"][0].TownId);
        }

        [Fact]
        public void An_empty_campaign_reads_and_writes_as_nothing()
        {
            Assert.Empty(LedgerCodec.ReadLedger(null));
            Assert.Empty(LedgerCodec.ReadLedger(""));
            Assert.Empty(LedgerCodec.ReadPurchases(null));
            Assert.Empty(LedgerCodec.ReadPurchases(""));
            Assert.Equal("", LedgerCodec.WriteLedger(null));
            Assert.Equal("", LedgerCodec.WritePurchases(null));
            Assert.Equal("", LedgerCodec.WriteLedger(new Dictionary<string, List<PriceObservation>>()));
            Assert.Equal("", LedgerCodec.WritePurchases(new List<PurchaseRecord>()));
        }

        [Fact]
        public void Gaps_in_a_campaign_are_written_without_stopping_the_rest()
        {
            var ledger = new Dictionary<string, List<PriceObservation>>
            {
                ["missing"] = null,
                ["holes"] = new List<PriceObservation> { null, new PriceObservation { ItemId = "holes", TownId = null, BuyPrice = 1, SellPrice = 2, CapturedDay = 3f } },
                ["grain"] = new List<PriceObservation> { new PriceObservation { ItemId = "grain", TownId = "town_S5", BuyPrice = 4, SellPrice = 5, CapturedDay = 6f } }
            };
            var purchases = new List<PurchaseRecord>
            {
                null,
                new PurchaseRecord { ItemId = null, TotalPaid = 1, Count = 1, LastUnitPaid = 1 },
                new PurchaseRecord { ItemId = "grain", TotalPaid = 2, Count = 2, LastUnitPaid = 1 }
            };

            var backLedger = LedgerCodec.ReadLedger(LedgerCodec.WriteLedger(ledger));
            var backPurchases = LedgerCodec.ReadPurchases(LedgerCodec.WritePurchases(purchases));

            Assert.Single(backLedger);
            Assert.True(backLedger.ContainsKey("grain"));
            Assert.Single(backPurchases);
            Assert.Equal("grain", backPurchases[0].ItemId);
        }

        [Fact]
        public void A_long_campaign_still_reads_back_exactly()
        {
            var ledger = new Dictionary<string, List<PriceObservation>>();
            for (int i = 0; i < 400; i++)
            {
                var list = new List<PriceObservation>();
                for (int t = 0; t < 60; t++)
                    list.Add(new PriceObservation { ItemId = "item_" + i, TownId = "town_" + t, BuyPrice = 100 + t, SellPrice = 200 + t, CapturedDay = 1234.5f });
                ledger["item_" + i] = list;
            }

            string written = LedgerCodec.WriteLedger(ledger);
            var back = LedgerCodec.ReadLedger(written);

            Assert.Equal(400, back.Count);
            Assert.Equal(24000, back.Values.Sum(v => v.Count));
            Assert.Equal(written, LedgerCodec.WriteLedger(back));
        }
    }
}
