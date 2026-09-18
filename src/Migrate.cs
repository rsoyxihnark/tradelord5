using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TradeLord
{
    public static class Migration
    {
        public const int Shape = 15;

        public const string ShapeKey = "SettingsVersion";

        private static readonly (int arrivedAt, string was, string now)[] Renamed =
        {
            (5, "BuyPackAnimals", "BuyHaulAnimals"),
            (7, "MaxTravelDays", "MaxTravelDaysTown"),
            (7, "MaxVillageTravelDays", "MaxTravelDaysVillage"),
        };

        public static bool Lift(int from, IDictionary<string, string> written, ICollection<string> notes)
        {
            if (written == null) return false;
            bool changed = false;
            changed |= Rename(from, written, notes);
            if (from < 5)
            {
                changed |= FoodVarietyBecameASwitchAndAnAmount(written, notes);
                changed |= SmeltableWeaponsBecameAChoiceOfThree(written, notes);
                changed |= PayingOverTheOddsForAHaulAnimalIsGone(written, notes);
            }
            if (from < 6) changed |= TheAutoMarkerCeilingIsGone(written, notes);
            if (from < 7) changed |= TheScanRadiusIsGone(written, notes);
            if (from < 8) changed |= KeepingAndRestockingFoodBecameOneSetting(written, notes);
            if (from < 15) changed |= WhatToBuyFirstIsOneRuleNow(written, notes);
            if (changed && notes != null)
                notes.Add("your settings were written by an older TradeLord, so they have been brought forward from shape " +
                          from + " to shape " + Shape);
            return changed;
        }

        private static bool Rename(int from, IDictionary<string, string> written, ICollection<string> notes)
        {
            bool changed = false;
            foreach (var (arrivedAt, was, now) in Renamed)
            {
                if (from >= arrivedAt) continue;
                if (!written.TryGetValue(was, out string held)) continue;
                written.Remove(was);
                changed = true;
                if (written.ContainsKey(now)) continue;
                written[now] = held;
                notes?.Add("'" + was + "' is called '" + now + "' now, and your value was carried over");
            }
            return changed;
        }

        private static bool FoodVarietyBecameASwitchAndAnAmount(IDictionary<string, string> written,
                                                               ICollection<string> notes)
        {
            const string was = "KeepFoodVariety";
            if (!written.TryGetValue(was, out string held)) return false;
            written.Remove(was);
            if (!int.TryParse(held, NumberStyles.Integer, CultureInfo.InvariantCulture, out int kept))
            {
                notes?.Add("'" + held + "' could not be read as how many of each kind of food to keep, so keeping " +
                           "some of every kind starts off");
                return true;
            }
            if (!written.ContainsKey("KeepEveryFoodKind"))
                written["KeepEveryFoodKind"] = kept > 0 ? "true" : "false";
            if (kept > 0 && !written.ContainsKey("KeepPerFoodKind"))
                written["KeepPerFoodKind"] = kept.ToString(CultureInfo.InvariantCulture);
            notes?.Add("keeping some of every kind of food is a switch and an amount now, so your setting of " +
                       held + " became " + (kept > 0 ? "on, " + kept + " of each kind" : "off"));
            return true;
        }

        private static bool SmeltableWeaponsBecameAChoiceOfThree(IDictionary<string, string> written,
                                                                ICollection<string> notes)
        {
            const string name = "KeepSmeltableWeapons";
            if (!written.TryGetValue(name, out string held)) return false;
            if (!bool.TryParse(held, out bool kept)) return false;
            int picked = kept ? Options.SmeltKeepAll : Options.SmeltSellThem;
            written[name] = picked.ToString(CultureInfo.InvariantCulture);
            notes?.Add("smeltable weapons are a choice of three now, so your setting of " + held +
                       " became " + (kept ? "keep every one" : "sell them"));
            return true;
        }

        private static bool WhatToBuyFirstIsOneRuleNow(IDictionary<string, string> written,
                                                       ICollection<string> notes)
        {
            bool changed = false;
            foreach (string was in new[] { "FollowTheLedgerFirst", "WhatToBuyFirst" })
            {
                if (!written.TryGetValue(was, out string held)) continue;
                written.Remove(was);
                changed = true;
                notes?.Add("what TradeLord buys first is one rule now, whichever good on the shelf " +
                           "would make you the most for what you could take of it, so your setting of " +
                           held + " is no longer read");
            }
            return changed;
        }

        private static bool KeepingAndRestockingFoodBecameOneSetting(IDictionary<string, string> written,
                                                                     ICollection<string> notes)
        {
            const string was = "ResupplyFoodDays";
            if (!written.TryGetValue(was, out string held)) return false;
            written.Remove(was);
            written.TryGetValue("KeepFoodDays", out string keeps);
            notes?.Add("keeping food back and restocking it are one setting now, so the days of food " +
                       "TradeLord restocks to is the same number it keeps back" +
                       (keeps == null ? "" : ", which is " + keeps) +
                       ", and your restock setting of " + held + " is no longer read");
            return true;
        }

        private static bool TheScanRadiusIsGone(IDictionary<string, string> written,
                                                ICollection<string> notes)
        {
            const string was = "ScanRadius";
            if (!written.TryGetValue(was, out string held)) return false;
            written.Remove(was);
            notes?.Add("how far TradeLord looks is now the town and village travel ceilings alone, " +
                       "so the scan radius is gone and your setting of " + held + " is no longer read");
            return true;
        }

        private static bool TheAutoMarkerCeilingIsGone(IDictionary<string, string> written,
                                                       ICollection<string> notes)
        {
            const string was = "MarkerMaxTravelDays";
            if (!written.TryGetValue(was, out string held)) return false;
            written.Remove(was);
            notes?.Add("the town marked on your map now keeps to the same travel ceiling as everything else, " +
                       "so the marker's own ceiling is gone and your setting of " + held + " is no longer read");
            return true;
        }

        private static bool PayingOverTheOddsForAHaulAnimalIsGone(IDictionary<string, string> written,
                                                                 ICollection<string> notes)
        {
            const string was = "PackAnimalFullCargoPremium";
            if (!written.TryGetValue(was, out string held)) return false;
            written.Remove(was);
            notes?.Add("a haul animal is only ever bought at the cheapest price TradeLord has seen for it now, " +
                       "so paying over the odds for one while your bags are full is gone and your setting of " +
                       held + " is no longer read");
            return true;
        }
    }

    public static class SettingsFile
    {
        public const char Marks = '#';

        public const char Splits = '=';

        public static Dictionary<string, string> Read(IEnumerable<string> lines, ICollection<string> ignored)
        {
            var written = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (lines == null) return written;
            foreach (string raw in lines)
            {
                if (raw == null) continue;
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == Marks) continue;
                int mark = line.IndexOf(Splits);
                if (mark < 0) { ignored?.Add(line); continue; }
                written[line.Substring(0, mark).Trim()] = line.Substring(mark + 1).Trim();
            }
            return written;
        }

        public static string OneLine(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.IndexOf('\n') < 0 && value.IndexOf('\r') < 0
                ? value
                : value.Replace('\r', ' ').Replace('\n', ' ');
        }

        public static string Compose(IEnumerable<string> header,
                                     IEnumerable<KeyValuePair<string, string>> settings)
        {
            var sb = new StringBuilder();
            if (header != null)
                foreach (string line in header)
                {
                    sb.Append(Marks);
                    if (!string.IsNullOrEmpty(line)) sb.Append(' ').Append(OneLine(line));
                    sb.AppendLine();
                }
            sb.AppendLine();
            if (settings != null)
                foreach (var line in settings)
                {
                    if (string.IsNullOrEmpty(line.Key)) continue;
                    sb.Append(OneLine(line.Key)).Append(' ').Append(Splits).Append(' ')
                      .AppendLine(OneLine(line.Value));
                }
            return sb.ToString();
        }
    }

    public static class Twins
    {
        public static readonly TimeSpan HandTolerance = TimeSpan.FromSeconds(30);

        public static bool ChangedByHand(DateTime lastWritten, DateTime stamped) =>
            stamped == default(DateTime) || lastWritten > stamped + HandTolerance;

        public static bool ScreenWins(bool screenInHand, bool screenWroteIt, bool changedByHand) =>
            screenInHand && screenWroteIt && !changedByHand;
    }

    public static class Whip
    {
        public const bool Armed = true;

        public const int CracksAt = 15;

        public static bool Cracks(bool armed, int cracksAt, int shipped, int shape) =>
            armed && cracksAt > 0 && cracksAt == shipped && shape < cracksAt;

        public static bool CracksOn(int shape) => Cracks(Armed, CracksAt, Migration.Shape, shape);

        public static bool Crack(int shape, IDictionary<string, string> written) =>
            Crack(CracksOn(shape), written);

        public static bool Crack(bool cracks, IDictionary<string, string> written)
        {
            if (!cracks || written == null) return false;
            written.Clear();
            return true;
        }
    }

    public static class Limits
    {
        private static readonly Dictionary<string, double[]> Bounds =
            new Dictionary<string, double[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "MinTownStock", new double[] { 0, 100 } },
                { "MinTownStockWorth", new double[] { 0, 20000 } },
                { "ObservationShelfLifeDays", new double[] { 0, 60 } },
                { "MaxTravelDaysTown", new double[] { 0, 20 } },
                { "MaxTravelDaysVillage", new double[] { 0, 10 } },
                { "MinProfitMargin", new double[] { 0, 2 } },
                { "EconomySettlingDays", new double[] { 0, 100 } },
                { "TradeXpMultiplier", new double[] { 0, 3 } },
                { "KeepFoodDays", new double[] { 0, 30 } },
                { "KeepPerFoodKind", new double[] { 1, 50 } },
                { "MaxLootTier", new double[] { 0, 6 } },
                { "BestSellTownTolerance", new double[] { 0.5, 1 } },
                { "GoldReserve", new double[] { 0, 100000 } },
                { "KeepWageDays", new double[] { 0, 30 } },
                { "BuyCapPerItem", new double[] { 0, 500 } },
                { "BuyValueCapPerItem", new double[] { 0, 50000 } },
                { "MaxHeldPerItem", new double[] { 0, 5000 } },
                { "MaxSpendPerVisit", new double[] { 0, 100000 } },
                { "ResaleSafetyFactor", new double[] { 0.5, 1 } },
                { "MaxHeldShare", new double[] { 0, 1 } },
                { "HaulAnimalGoldFloor", new double[] { 0, 100000 } },
                { "MaxWorkshopsOwned", new double[] { 0, 200 } },
                { "HaulAnimalPriceTolerance", new double[] { 1, 3 } },
                { "MaxCargoShare", new double[] { 0.1, 1 } },
                { "PartyTradeXpShare", new double[] { 0, 2 } },
                { "Language", new double[] { 0, 3 } },
                { "FoodPolicy", new double[] { 0, 3 } },
                { "CraftingPolicy", new double[] { 0, 3 } },
                { "LivestockPolicy", new double[] { 0, 3 } },
                { "CostBasisMode", new double[] { 0, 2 } },
                { "KeepSmeltableWeapons", new double[] { 0, 2 } },
            };

        public static bool Knows(string name) => name != null && Bounds.ContainsKey(name);

        public static double Kept(string name, double asked)
        {
            if (name == null || !Bounds.TryGetValue(name, out double[] edge)) return asked;
            if (double.IsNaN(asked)) return edge[0];
            if (asked < edge[0]) return edge[0];
            if (asked > edge[1]) return edge[1];
            return asked;
        }

        public static string Range(string name) =>
            Bounds.TryGetValue(name, out double[] edge)
                ? Said(edge[0]) + " and " + Said(edge[1])
                : "";

        private static string Said(double edge) =>
            edge == Math.Floor(edge)
                ? ((long)edge).ToString(CultureInfo.InvariantCulture)
                : edge.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
