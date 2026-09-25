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

    internal sealed class ShopLine
    {
        internal int Shop;
        internal float Progress;
        internal float Speed;
        internal float Pace;
        internal bool Hidden;
        internal bool Yours;
        internal bool TradeGoodsOnly = true;
        internal bool FromWarehouse;
        internal float ToTown = 1f;
        internal readonly List<(string category, int count)> Inputs = new List<(string, int)>();
        internal readonly List<(string category, int count)> Outputs = new List<(string, int)>();
    }

    internal sealed class TownBook
    {
        internal readonly Dictionary<string, int> Units = new Dictionary<string, int>(StringComparer.Ordinal);
        internal readonly Dictionary<string, int> InStore = new Dictionary<string, int>(StringComparer.Ordinal);
        internal readonly Dictionary<string, int> UnitValue = new Dictionary<string, int>(StringComparer.Ordinal);
        internal readonly Dictionary<string, int> UsedADay = new Dictionary<string, int>(StringComparer.Ordinal);
        internal int Gold;
        internal int[] Capital = new int[0];
        internal int[] Expense = new int[0];
        internal int[] Warehouse = new int[0];
    }

    internal static class WorkshopRuns
    {
        internal const float PayOverInputsPerPace = 200f;

        internal const int MostPaidForAnOutput = 1000;

        internal const float LongestWatch = 10f;

        internal const float ShortestWatch = 2f;

        internal static float Watch(float travelCeiling)
        {
            float ceiling = TradeMath.Finite(travelCeiling, 0f);
            if (ceiling <= 0f) return LongestWatch;
            float watch = (float)Math.Ceiling(ceiling * 2f) + 1f;
            return watch < ShortestWatch ? ShortestWatch : watch > LongestWatch ? LongestWatch : watch;
        }

        internal static void Follow(IList<ShopLine> lines, TownBook town, float firstTick, float watch,
                                    IList<Landing> arriving, IList<(float days, string category, int units)> bought,
                                    Func<string, int, bool, int> price, List<Landing> made, List<Draw> taken)
        {
            if (lines == null || lines.Count == 0 || town == null || price == null || made == null || taken == null)
                return;
            var kinds = new List<string>();
            foreach (ShopLine line in lines)
            {
                foreach (var (kind, _) in line.Inputs) if (kind != null && !kinds.Contains(kind)) kinds.Add(kind);
                foreach (var (kind, _) in line.Outputs) if (kind != null && !kinds.Contains(kind)) kinds.Add(kind);
            }
            var landing = new List<(float days, string kind, int units)>();
            for (int i = 0; arriving != null && i < arriving.Count; i++)
                if (arriving[i].Units > 0 && kinds.Contains(arriving[i].Category))
                    landing.Add((arriving[i].Days, arriving[i].Category, arriving[i].Units));
            for (int i = 0; bought != null && i < bought.Count; i++)
                if (bought[i].units > 0 && kinds.Contains(bought[i].category))
                    landing.Add((bought[i].days, bought[i].category, -bought[i].units));
            landing.Sort((x, y) => x.days.CompareTo(y.days));

            var buy = new Dictionary<string, int>(StringComparer.Ordinal);
            var sell = new Dictionary<string, int>(StringComparer.Ordinal);
            var madeNow = new Dictionary<string, int>(StringComparer.Ordinal);
            var takenNow = new Dictionary<string, int>(StringComparer.Ordinal);
            var toTown = new float[town.Capital.Length];
            float first = TradeMath.Finite(firstTick, 1f);
            if (first <= 0f) first = 1f;
            int landed = 0;
            for (float at = first; at <= watch; at += 1f)
            {
                for (; landed < landing.Count && landing[landed].days <= at; landed++)
                    Stock(town, landing[landed].kind, landing[landed].units);
                foreach (string kind in kinds)
                {
                    town.InStore.TryGetValue(kind, out int store);
                    buy[kind] = price(kind, store, false);
                    sell[kind] = price(kind, store, true);
                }
                madeNow.Clear();
                takenNow.Clear();
                foreach (ShopLine line in lines)
                {
                    if (!(line.Speed > 0f)) continue;
                    float progress = TradeMath.Finite(line.Progress, 0f);
                    line.Progress = (progress > 1f ? 1f : progress) + line.Speed;
                    while (line.Progress >= 1f)
                    {
                        bool ran = RunOnce(line, town, buy, sell, toTown, madeNow, takenNow);
                        line.Progress -= 1f;
                        if (!ran) break;
                    }
                }
                for (int k = 0; k < town.Capital.Length && k < town.Expense.Length; k++)
                    if (town.Expense[k] > 0 && town.Capital[k] >= town.Expense[k]) town.Capital[k] -= town.Expense[k];
                foreach (string kind in kinds)
                    if (town.UsedADay.TryGetValue(kind, out int worth) && worth > 0 &&
                        town.UnitValue.TryGetValue(kind, out int value) && value > 0)
                        Stock(town, kind, -(int)Math.Round((double)worth / value, MidpointRounding.AwayFromZero));
                foreach (var one in madeNow)
                {
                    town.UnitValue.TryGetValue(one.Key, out int value);
                    made.Add(new Landing
                    {
                        Category = one.Key, Units = one.Value, Worth = TradeMath.WorthOf(one.Value, value), Days = at
                    });
                }
                foreach (var one in takenNow)
                {
                    town.UnitValue.TryGetValue(one.Key, out int value);
                    taken.Add(new Draw { Category = one.Key, Worth = TradeMath.WorthOf(one.Value, value), Days = at });
                }
            }
        }

        private static void Stock(TownBook town, string kind, int units)
        {
            town.Units.TryGetValue(kind, out int have);
            if (have + units < 0) units = -have;
            town.Units[kind] = have + units;
            town.UnitValue.TryGetValue(kind, out int value);
            town.InStore.TryGetValue(kind, out int store);
            long after = (long)store + (long)units * value;
            town.InStore[kind] = after < 0L ? 0 : after > int.MaxValue ? int.MaxValue : (int)after;
        }

        private static void Tally(Dictionary<string, int> tally, string kind, int units)
        {
            tally.TryGetValue(kind, out int had);
            tally[kind] = had + units;
        }

        private static bool RunOnce(ShopLine line, TownBook town, Dictionary<string, int> buy,
                                    Dictionary<string, int> sell, float[] toTown,
                                    Dictionary<string, int> made, Dictionary<string, int> taken)
        {
            int k = line.Shop;
            bool owned = k >= 0 && k < town.Capital.Length;
            int need = 0;
            foreach (var (_, count) in line.Inputs) need += count;
            bool fromWarehouse = line.Yours && line.FromWarehouse && owned && k < town.Warehouse.Length &&
                                 town.Warehouse[k] >= need;
            long cost = 0;
            if (!fromWarehouse)
                foreach (var (kind, count) in line.Inputs)
                {
                    town.Units.TryGetValue(kind, out int have);
                    if (have < count) return false;
                    cost += (long)buy[kind] * count;
                }
            long income = 0;
            foreach (var (kind, count) in line.Outputs) income += (long)sell[kind] * count;
            double bar = line.Hidden || !(line.Pace > 0f) ? cost : cost + PayOverInputsPerPace / line.Pace;
            if (income <= bar) return false;
            if (line.TradeGoodsOnly && town.Gold < income) return false;
            if (owned && town.Capital[k] < cost) return false;

            if (fromWarehouse) town.Warehouse[k] -= need;
            else
                foreach (var (kind, count) in line.Inputs)
                {
                    Stock(town, kind, -count);
                    Tally(taken, kind, count);
                    if (!line.TradeGoodsOnly) continue;
                    if (owned) town.Capital[k] -= buy[kind];
                    town.Gold += buy[kind];
                }
            foreach (var (kind, count) in line.Outputs)
                for (int unit = 0; unit < count; unit++)
                {
                    if (line.Yours && owned)
                    {
                        toTown[k] += line.ToTown;
                        if (toTown[k] < 1f) continue;
                        toTown[k] -= 1f;
                    }
                    Stock(town, kind, 1);
                    Tally(made, kind, 1);
                    if (!line.TradeGoodsOnly) continue;
                    int paid = Math.Min(MostPaidForAnOutput, buy[kind]);
                    if (owned) town.Capital[k] += paid;
                    town.Gold -= paid;
                }
            return true;
        }
    }
}
