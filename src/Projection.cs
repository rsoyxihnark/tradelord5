using System;
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

    internal struct Draw
    {
        internal string Category;
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

        internal static int WorthUsedUp(IList<Draw> drawn, string category, float withinDays, int usedADay,
                                        int most)
        {
            if (category == null || most <= 0) return 0;
            withinDays = TradeMath.ToTheQuarterDay(withinDays);
            double used = usedADay > 0 ? (double)usedADay * withinDays : 0d;
            for (int i = 0; drawn != null && i < drawn.Count; i++)
            {
                Draw draw = drawn[i];
                if (draw.Category != category || draw.Worth <= 0) continue;
                if (!TradeMath.LandsInTime(draw.Days, withinDays)) continue;
                used += draw.Worth;
            }
            return used >= most ? most : (int)used;
        }

        internal static int UnitsLeaving(int worthLeaving, int unitValue)
        {
            if (worthLeaving <= 0 || unitValue <= 0) return 0;
            return worthLeaving / unitValue;
        }

        internal const float NeverRunsOut = -1f;

        internal const int DaysUseIsReadFor = 30;

        internal static List<float> Moments(IList<Landing> listed, IList<Spending> coming,
                                            float afterDays, IList<Draw> drawn = null, int usedADay = 0)
        {
            var when = new List<float>();
            var already = new HashSet<float>();
            for (int i = 0; listed != null && i < listed.Count; i++) Note(when, already, listed[i].Days, afterDays);
            for (int i = 0; coming != null && i < coming.Count; i++) Note(when, already, coming[i].Days, afterDays);
            for (int i = 0; drawn != null && i < drawn.Count; i++) Note(when, already, drawn[i].Days, afterDays);
            if (usedADay > 0 && !float.IsNaN(afterDays) && afterDays < TradeMath.LongerThanAnyRide)
            {
                int first = afterDays < 0f ? 1 : (int)Math.Floor(afterDays) + 1;
                for (int day = first; day < first + DaysUseIsReadFor; day++) Note(when, already, day, afterDays);
            }
            when.Sort();
            return when;
        }

        private static void Note(List<float> when, HashSet<float> already, float days, float afterDays)
        {
            float at = TradeMath.UpToTheQuarterDay(days);
            if (at <= afterDays || !already.Add(at)) return;
            when.Add(at);
        }

        internal static List<(float days, int shelf)> ShelfAhead(
            IList<Landing> listed, IList<Spending> coming,
            IDictionary<string, float> pull, float across,
            string item, string category, int unitValue, int stockNow, float afterDays,
            IList<Draw> drawn = null, int usedADay = 0)
        {
            var curve = new List<(float, int)>();
            if (item == null || unitValue <= 0) return curve;
            List<float> when = Moments(listed, coming, afterDays, drawn, usedADay);
            int most = TradeMath.WorthOf(stockNow, unitValue);
            for (int i = 0; i < when.Count; i++)
            {
                float days = when[i];
                int used = WorthUsedUp(drawn, category, days, usedADay,
                                       TradeMath.AddedUp(most, WorthLanding(listed, category, days)));
                int taken = UnitsLeaving(
                    TradeMath.AddedUp(WorthLeaving(PurseLanding(coming, days), pull, across, category), used),
                    unitValue);
                curve.Add((days, TradeMath.StockAfterShift(stockNow,
                               UnitsLanding(listed, item, days), taken)));
            }
            return curve;
        }

        internal static float RunsOutOf(IList<(float days, int shelf)> curve, int wanted)
        {
            if (curve == null || wanted <= 0) return NeverRunsOut;
            for (int i = 0; i < curve.Count; i++)
                if (curve[i].shelf < wanted) return curve[i].days;
            return NeverRunsOut;
        }

        internal static float RunsOutAt(IList<Landing> listed, IList<Spending> coming,
                                        IDictionary<string, float> pull, float across,
                                        string item, string category, int unitValue,
                                        int stockNow, int wanted, float afterDays)
        {
            if (item == null || wanted <= 0 || unitValue <= 0) return NeverRunsOut;
            return RunsOutOf(ShelfAhead(listed, coming, pull, across, item, category,
                                        unitValue, stockNow, afterDays), wanted);
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
