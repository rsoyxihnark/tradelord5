using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class MigrationTests
    {
        private static Dictionary<string, string> File(params string[] pairs)
        {
            var written = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < pairs.Length; i += 2) written[pairs[i]] = pairs[i + 1];
            return written;
        }

        [Fact]
        public void AFileFromTheVersionOnNexusIsCarriedForwardWholesale()
        {
            var written = File("KeepFoodDays", "5", "GoldReserve", "900", "PanelKey", "Ctrl+T",
                               "MinProfitMargin", "0.25", "NeverBuyGrain", "false");
            var notes = new List<string>();
            Assert.False(Migration.Lift(1, written, notes));
            Assert.Equal("900", written["GoldReserve"]);
            Assert.Equal("Ctrl+T", written["PanelKey"]);
            Assert.Equal("0.25", written["MinProfitMargin"]);
            Assert.Equal("false", written["NeverBuyGrain"]);
            Assert.Empty(notes);
        }

        [Theory]
        [InlineData("3", "true", "3")]
        [InlineData("1", "true", "1")]
        [InlineData("0", "false", null)]
        public void KeepingEveryKindOfFoodBecomesASwitchAndAnAmount(string held, string on, string each)
        {
            var written = File("KeepFoodVariety", held);
            var notes = new List<string>();
            Assert.True(Migration.Lift(1, written, notes));
            Assert.False(written.ContainsKey("KeepFoodVariety"));
            Assert.Equal(on, written["KeepEveryFoodKind"]);
            if (each == null) Assert.False(written.ContainsKey("KeepPerFoodKind"));
            else Assert.Equal(each, written["KeepPerFoodKind"]);
            Assert.NotEmpty(notes);
        }

        [Fact]
        public void AnUnreadableFoodVarietyLeavesTheSwitchAloneAndSaysSo()
        {
            var written = File("KeepFoodVariety", "lots");
            var notes = new List<string>();
            Assert.True(Migration.Lift(1, written, notes));
            Assert.False(written.ContainsKey("KeepFoodVariety"));
            Assert.False(written.ContainsKey("KeepEveryFoodKind"));
            Assert.NotEmpty(notes);
        }

        [Theory]
        [InlineData("true", Options.SmeltKeepAll)]
        [InlineData("false", Options.SmeltSellThem)]
        public void KeepingSmeltableWeaponsBecomesAChoiceOfThree(string held, int picked)
        {
            var written = File("KeepSmeltableWeapons", held);
            var notes = new List<string>();
            Assert.True(Migration.Lift(1, written, notes));
            Assert.Equal(picked.ToString(), written["KeepSmeltableWeapons"]);
            Assert.NotEmpty(notes);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("1")]
        [InlineData("2")]
        public void AChoiceAlreadyMadeIsLeftExactlyAsItIs(string held)
        {
            var written = File("KeepSmeltableWeapons", held);
            var notes = new List<string>();
            Assert.False(Migration.Lift(Migration.Shape, written, notes));
            Assert.Equal(held, written["KeepSmeltableWeapons"]);
            Assert.Empty(notes);
        }

        [Theory]
        [InlineData("1.5")]
        [InlineData("1")]
        [InlineData("3")]
        public void PayingOverTheOddsForAHaulAnimalIsDroppedAndNamed(string held)
        {
            var written = File("PackAnimalFullCargoPremium", held, "GoldReserve", "800");
            var notes = new List<string>();
            Assert.True(Migration.Lift(2, written, notes));
            Assert.False(written.ContainsKey("PackAnimalFullCargoPremium"));
            Assert.Equal("800", written["GoldReserve"]);
            Assert.NotEmpty(notes);
        }

        [Fact]
        public void AFileWithoutTheOldHaulAnimalPremiumIsLeftAlone()
        {
            var written = File("GoldReserve", "800");
            var notes = new List<string>();
            Assert.False(Migration.Lift(Migration.Shape, written, notes));
            Assert.Single(written);
            Assert.Empty(notes);
        }

        [Fact]
        public void LiftingTwiceChangesNothingTheSecondTime()
        {
            var written = File("KeepFoodVariety", "4", "KeepSmeltableWeapons", "true");
            Assert.True(Migration.Lift(1, written, new List<string>()));
            var after = new Dictionary<string, string>(written);
            Assert.False(Migration.Lift(Migration.Shape, written, new List<string>()));
            Assert.Equal(after, written);
        }

        [Fact]
        public void AValueTheNewSettingAlreadyCarriesIsNeverOverwritten()
        {
            var written = File("KeepFoodVariety", "9", "KeepEveryFoodKind", "false", "KeepPerFoodKind", "2");
            Assert.True(Migration.Lift(1, written, new List<string>()));
            Assert.Equal("false", written["KeepEveryFoodKind"]);
            Assert.Equal("2", written["KeepPerFoodKind"]);
        }

        [Fact]
        public void TheReservedLinesAreNotSettingsAndNeverReachTheOptions()
        {
            Assert.Equal("SettingsVersion", Migration.ShapeKey);
            Assert.Equal(3, Migration.Shape);
            var written = File(Migration.ShapeKey, "1", "GoldReserve", "700");
            written.Remove(Migration.ShapeKey);
            Assert.False(Migration.Lift(1, written, new List<string>()));
            Assert.Single(written);
            Assert.Equal("700", written["GoldReserve"]);
        }

        [Fact]
        public void AFileFromBeforeTheShapeStampIsStillLiftedWhole()
        {
            var written = File("KeepFoodVariety", "3", "KeepSmeltableWeapons", "true",
                               "GoldReserve", "1200", "PanelKey", "Y");
            var notes = new List<string>();
            Assert.True(Migration.Lift(1, written, notes));
            Assert.Equal("true", written["KeepEveryFoodKind"]);
            Assert.Equal("3", written["KeepPerFoodKind"]);
            Assert.Equal(Options.SmeltKeepAll.ToString(), written["KeepSmeltableWeapons"]);
            Assert.Equal("1200", written["GoldReserve"]);
            Assert.Equal("Y", written["PanelKey"]);
        }

        [Theory]
        [InlineData("MinProfitMargin", -1.0, 0.0)]
        [InlineData("MinProfitMargin", 99.0, 2.0)]
        [InlineData("MinProfitMargin", 0.25, 0.25)]
        [InlineData("GoldReserve", -50000.0, 0.0)]
        [InlineData("GoldReserve", 999999999.0, 100000.0)]
        [InlineData("MaxLootTier", 99.0, 6.0)]
        [InlineData("ResaleSafetyFactor", 0.0, 0.5)]
        [InlineData("KeepPerFoodKind", 0.0, 1.0)]
        [InlineData("Language", 7.0, 3.0)]
        [InlineData("FoodPolicy", -3.0, 0.0)]
        [InlineData("CostBasisMode", 9.0, 2.0)]
        [InlineData("KeepSmeltableWeapons", 5.0, 2.0)]
        public void ANumberOutsideItsLimitsIsBroughtBackInside(string name, double asked, double kept)
        {
            Assert.Equal(kept, Limits.Kept(name, asked));
        }

        [Fact]
        public void ANumberThatIsNotANumberFallsToTheBottomOfItsRange()
        {
            Assert.Equal(0.0, Limits.Kept("MinProfitMargin", double.NaN));
            Assert.Equal(0.5, Limits.Kept("ResaleSafetyFactor", double.NaN));
            Assert.Equal(0.0, Limits.Kept("MaxHeldShare", double.PositiveInfinity * 0 * 0));
        }

        [Fact]
        public void AnEndlessNumberIsPulledToTheEdgeOfItsRange()
        {
            Assert.Equal(2.0, Limits.Kept("MinProfitMargin", double.PositiveInfinity));
            Assert.Equal(0.0, Limits.Kept("MinProfitMargin", double.NegativeInfinity));
        }

        [Fact]
        public void ASettingWithNoLimitsIsLeftExactlyAsItIs()
        {
            Assert.False(Limits.Knows("PanelKey"));
            Assert.False(Limits.Knows("NeverSellItems"));
            Assert.False(Limits.Knows(null));
            Assert.Equal(12345.0, Limits.Kept("PanelKey", 12345.0));
            Assert.Equal(12345.0, Limits.Kept(null, 12345.0));
        }

        [Fact]
        public void EveryRangeReadsBackAsPlainWords()
        {
            Assert.Equal("0 and 2", Limits.Range("MinProfitMargin"));
            Assert.Equal("0.5 and 1", Limits.Range("ResaleSafetyFactor"));
            Assert.Equal("", Limits.Range("PanelKey"));
        }

        [Fact]
        public void AnEmptyFileAndANullFileAreBothSafe()
        {
            Assert.False(Migration.Lift(1, new Dictionary<string, string>(), new List<string>()));
            Assert.False(Migration.Lift(1, null, new List<string>()));
            Assert.True(Migration.Lift(1, File("KeepFoodVariety", "2"), null));
        }
    }
}
