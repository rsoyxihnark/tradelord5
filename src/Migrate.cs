using System;
using System.Collections.Generic;
using System.Globalization;

namespace TradeLord
{
    public static class Migration
    {
        public const int Shape = 2;

        public const string ShapeKey = "SettingsVersion";

        private static readonly Dictionary<string, string> Renamed =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
            };

        public static bool Lift(int from, IDictionary<string, string> written, ICollection<string> notes)
        {
            if (written == null) return false;
            bool changed = false;
            changed |= Rename(written, notes);
            changed |= FoodVarietyBecameASwitchAndAnAmount(written, notes);
            changed |= SmeltableWeaponsBecameAChoiceOfThree(written, notes);
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
    }
}
