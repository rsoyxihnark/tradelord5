using System.Collections.Generic;

namespace TradeLord
{
    internal struct Spending
    {
        internal int Gold;
        internal float Days;
    }

    internal struct Landing
    {
        internal string Item;
        internal string Category;
        internal int Units;
        internal int Worth;
        internal float Days;
    }

    internal static class Projection
    {
        internal const float WorkshopRunDays = 1f;

        internal static int UnitsLanding(IList<Landing> listed, string item, float withinDays)
        {
            if (listed == null || item == null) return 0;
            withinDays = TradeMath.ToTheQuarterDay(withinDays);
            int units = 0;
            for (int i = 0; i < listed.Count; i++)
            {
                Landing landing = listed[i];
                if (landing.Item != item) continue;
                if (!TradeMath.LandsInTime(landing.Days, withinDays)) continue;
                units += landing.Units;
            }
            return units;
        }

        internal static int WorthLanding(IList<Landing> listed, string category, float withinDays)
        {
            if (listed == null || category == null) return 0;
            withinDays = TradeMath.ToTheQuarterDay(withinDays);
            int worth = 0;
            for (int i = 0; i < listed.Count; i++)
            {
                Landing landing = listed[i];
                if (landing.Category != category) continue;
                if (!TradeMath.LandsInTime(landing.Days, withinDays)) continue;
                worth = TradeMath.ShelfAfterLanding(worth, landing.Worth);
            }
            return worth;
        }

        internal static int PurseLanding(IList<Spending> coming, float withinDays)
        {
            if (coming == null) return 0;
            withinDays = TradeMath.ToTheQuarterDay(withinDays);
            int purse = 0;
            for (int i = 0; i < coming.Count; i++)
            {
                Spending spending = coming[i];
                if (!TradeMath.LandsInTime(spending.Days, withinDays)) continue;
                purse += spending.Gold;
            }
            return purse;
        }

        internal static int WorthLeaving(int purse, IDictionary<string, float> pull, float across,
                                         string category)
        {
            if (purse <= 0 || pull == null || category == null) return 0;
            if (!pull.TryGetValue(category, out float mine)) return 0;
            return TradeMath.ShareOfAPurse(purse, mine, across);
        }

        internal static int UnitsLeaving(int worthLeaving, int unitValue)
        {
            if (worthLeaving <= 0 || unitValue <= 0) return 0;
            return worthLeaving / unitValue;
        }

        internal static float PullAcross(IDictionary<string, float> pull)
        {
            float total = 0f;
            if (pull == null) return total;
            foreach (float one in pull.Values) total += one;
            return total;
        }

        internal static bool PullReadable(IDictionary<string, float> pull, float across) =>
            pull != null && across > 0f;

        internal static string Named(IList<(string name, int count)> made)
        {
            if (made == null || made.Count == 0) return "";
            var said = new List<string>(made.Count);
            for (int i = 0; i < made.Count; i++) said.Add(made[i].name + " x" + made[i].count);
            return string.Join(", ", said.ToArray());
        }
    }
}
