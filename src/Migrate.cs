using System;
using System.Collections.Generic;
using System.Globalization;

namespace TradeLord
{
    public static class Migration
    {
        public const int Shape = 4;

        public const string ShapeKey = "SettingsVersion";

        private static readonly Dictionary<string, string> Renamed =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "BuyPackAnimals", "BuyHaulAnimals" },
            };

        public static bool Lift(int from, IDictionary<string, string> written, ICollection<string> notes)
        {
            if (written == null) return false;
            bool changed = false;
            changed |= Rename(written, notes);
            changed |= FoodVarietyBecameASwitchAndAnAmount(written, notes);
            changed |= SmeltableWeaponsBecameAChoiceOfThree(written, notes);
            changed |= PayingOverTheOddsForAHaulAnimalIsGone(written, notes);
            if (changed && notes != null)
                notes.Add("your settings were written by an older TradeLord, so they have been brought forward from shape " +
                          from + " to shape " + Shape);
            return changed;
        }

        private static bool Rename(IDictionary<string, string> written, ICollection<string> notes)
        {
            bool changed = false;
            foreach (var pair in new List<KeyValuePair<string, string>>(Renamed))
            {
                if (!written.TryGetValue(pair.Key, out string held)) continue;
                written.Remove(pair.Key);
                changed = true;
                if (written.ContainsKey(pair.Value)) continue;
                written[pair.Value] = held;
                notes?.Add("'" + pair.Key + "' is called '" + pair.Value + "' now, and your value was carried over");
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

    public static class Limits
    {
        private static readonly Dictionary<string, double[]> Bounds =
            new Dictionary<string, double[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "ObservationShelfLifeDays", new double[] { 0, 200 } },
                { "ScanRadius", new double[] { 0, 1000 } },
                { "MinTownStock", new double[] { 0, 100 } },
                { "MaxTravelDays", new double[] { 0, 20 } },
                { "MaxVillageTravelDays", new double[] { 0, 10 } },
                { "MinProfitMargin", new double[] { 0, 2 } },
                { "EconomySettlingDays", new double[] { 0, 100 } },
                { "TradeXpMultiplier", new double[] { 0, 3 } },
                { "MarkerMaxTravelDays", new double[] { 0, 10 } },
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
                { "ResupplyFoodDays", new double[] { 0, 30 } },
                { "MaxHeldShare", new double[] { 0, 1 } },
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
