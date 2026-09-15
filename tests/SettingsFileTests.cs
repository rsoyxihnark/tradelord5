using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class SettingsFileTests
    {
        private static readonly string[] Header =
        {
            "TradeLord settings.",
            "",
            "One setting per line, written as name = value.",
        };

        private static List<KeyValuePair<string, string>> Pairs(params string[] flat)
        {
            var held = new List<KeyValuePair<string, string>>();
            for (int i = 0; i + 1 < flat.Length; i += 2)
                held.Add(new KeyValuePair<string, string>(flat[i], flat[i + 1]));
            return held;
        }

        private static string[] Lines(string text) =>
            text.Replace("\r\n", "\n").Split('\n');

        private static string[] LinesKeepingCarriageReturns(string text) =>
            text.Replace("\r\n", "\n").Replace("\n", "\r\n").Split('\n');

        [Fact]
        public void A_comment_line_and_a_blank_line_carry_no_setting()
        {
            var ignored = new List<string>();
            var read = SettingsFile.Read(
                new[] { "# TradeLord settings.", "", "   ", "#GoldReserve = 900", "GoldReserve = 5000" },
                ignored);
            Assert.Single(read);
            Assert.Equal("5000", read["GoldReserve"]);
            Assert.Empty(ignored);
        }

        [Fact]
        public void The_space_around_a_name_and_its_value_is_not_part_of_either()
        {
            var read = SettingsFile.Read(new[] { "   GoldReserve   =   5000   " }, new List<string>());
            Assert.Equal("5000", read["GoldReserve"]);
        }

        [Fact]
        public void A_line_with_no_equals_sign_is_handed_back_rather_than_read_as_a_setting()
        {
            var ignored = new List<string>();
            var read = SettingsFile.Read(new[] { "GoldReserve 5000", "  what is this  ", "KeepWageDays = 7" }, ignored);
            Assert.Single(read);
            Assert.Equal(new[] { "GoldReserve 5000", "what is this" }, ignored);
        }

        [Fact]
        public void The_last_line_carrying_a_name_is_the_one_that_counts()
        {
            var read = SettingsFile.Read(new[] { "GoldReserve = 1", "GoldReserve = 2", "goldreserve = 3" },
                                         new List<string>());
            Assert.Single(read);
            Assert.Equal("3", read["GoldReserve"]);
        }

        [Fact]
        public void A_name_is_found_whatever_case_it_was_written_in()
        {
            var read = SettingsFile.Read(new[] { "gOlDrEsErVe = 5000" }, new List<string>());
            Assert.Equal("5000", read["GoldReserve"]);
            Assert.True(read.ContainsKey("GOLDRESERVE"));
        }

        [Fact]
        public void A_value_carrying_an_equals_sign_survives_whole()
        {
            var read = SettingsFile.Read(new[] { "NeverSell = sumpter_horse=2, mule=3" }, new List<string>());
            Assert.Equal("sumpter_horse=2, mule=3", read["NeverSell"]);
        }

        [Fact]
        public void A_name_with_nothing_after_the_equals_sign_is_read_as_an_empty_value()
        {
            var read = SettingsFile.Read(new[] { "NeverSell =", "NeverBuy =    " }, new List<string>());
            Assert.Equal("", read["NeverSell"]);
            Assert.Equal("", read["NeverBuy"]);
        }

        [Fact]
        public void Nothing_to_read_and_nothing_to_report_are_both_taken_in_their_stride()
        {
            Assert.Empty(SettingsFile.Read(null, new List<string>()));
            Assert.Empty(SettingsFile.Read(new string[0], null));
            var read = SettingsFile.Read(new[] { null, "GoldReserve = 5000", null }, null);
            Assert.Equal("5000", read["GoldReserve"]);
            Assert.Empty(SettingsFile.Read(new[] { "no equals sign here" }, null));
        }

        [Fact]
        public void The_header_goes_out_behind_the_comment_mark_with_a_blank_line_after_it()
        {
            string[] made = Lines(SettingsFile.Compose(Header, Pairs("GoldReserve", "5000")));
            Assert.Equal("# TradeLord settings.", made[0]);
            Assert.Equal("#", made[1]);
            Assert.Equal("# One setting per line, written as name = value.", made[2]);
            Assert.Equal("", made[3]);
            Assert.Equal("GoldReserve = 5000", made[4]);
        }

        [Fact]
        public void A_file_composed_with_no_header_and_no_settings_is_still_a_file()
        {
            Assert.Equal(Environment.NewLine, SettingsFile.Compose(null, null));
        }

        [Fact]
        public void A_setting_with_no_name_is_not_written_and_one_with_no_value_is()
        {
            string text = SettingsFile.Compose(null, Pairs("", "5000", "NeverSell", null, "KeepWageDays", "7"));
            Assert.DoesNotContain("= 5000", text);
            Assert.Contains("NeverSell = ", text);
            Assert.Contains("KeepWageDays = 7", text);
        }

        [Fact]
        public void What_is_composed_is_read_back_as_what_went_in()
        {
            var went = Pairs("SettingsVersion", "11", "GoldReserve", "5000",
                             "NeverSell", "sumpter_horse, mule", "TradeOnArrival", "true");
            var read = SettingsFile.Read(Lines(SettingsFile.Compose(Header, went)), new List<string>());
            Assert.Equal(went.Count, read.Count);
            foreach (var line in went) Assert.Equal(line.Value, read[line.Key]);
        }

        [Fact]
        public void A_file_stored_with_carriage_returns_reads_the_same_as_one_without()
        {
            string text = SettingsFile.Compose(Header, Pairs("GoldReserve", "5000", "NeverSell", "mule"));
            var plain = SettingsFile.Read(Lines(text), new List<string>());
            var ignored = new List<string>();
            var wound = SettingsFile.Read(LinesKeepingCarriageReturns(text), ignored);
            Assert.Empty(ignored);
            Assert.Equal(plain.Count, wound.Count);
            foreach (var line in plain) Assert.Equal(line.Value, wound[line.Key]);
        }

        [Fact]
        public void Whatever_is_written_comes_back_unchanged_however_it_is_spaced_and_stored()
        {
            var rng = new Random(8823);
            for (int round = 0; round < 20000; round++)
            {
                var went = new List<KeyValuePair<string, string>>();
                var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int count = rng.Next(1, 6);
                for (int i = 0; i < count; i++)
                {
                    string name = Word(rng, 1 + rng.Next(8));
                    if (!names.Add(name)) continue;
                    went.Add(new KeyValuePair<string, string>(name, Value(rng)));
                }
                string text = SettingsFile.Compose(rng.Next(2) == 0 ? Header : null, went);
                var ignored = new List<string>();
                var read = SettingsFile.Read(
                    rng.Next(2) == 0 ? Lines(text) : LinesKeepingCarriageReturns(text), ignored);
                Assert.Empty(ignored);
                Assert.Equal(went.Count, read.Count);
                foreach (var line in went) Assert.Equal(line.Value, read[line.Key]);
            }
        }

        private static string Word(Random rng, int length)
        {
            var made = new char[length];
            for (int i = 0; i < length; i++) made[i] = (char)('a' + rng.Next(26));
            return new string(made);
        }

        private static string Value(Random rng)
        {
            int kind = rng.Next(5);
            if (kind == 0) return "";
            if (kind == 1) return rng.Next(-5000, 100000).ToString();
            if (kind == 2) return rng.Next(2) == 0 ? "true" : "false";
            if (kind == 3) return Word(rng, 3) + "=" + rng.Next(9) + ", " + Word(rng, 4);
            return Word(rng, 2) + " " + Word(rng, 5) + ", " + Word(rng, 3);
        }
    }
}
