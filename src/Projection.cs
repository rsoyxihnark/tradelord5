using System;
using System.Collections.Generic;

namespace TradeLord
{
    internal struct Spending
    {
        internal string Category;
        internal int Worth;
        internal float Days;
    }

    internal struct Landing
    {
        internal string Item;
        internal string Category;
        internal int Units;
        internal int Worth;
        internal float Days;
        internal int Draw;
        internal int From;
    }

    internal struct Draw
    {
        internal string Category;
        internal int Worth;
        internal float Days;
    }

    internal static class Projection
    {
        internal static int UnitsLanding(IList<Landing> listed, string item, float withinDays, bool graded = false)
        {
            if (listed == null || item == null) return 0;
            TradeMath.ForecastHorizon(withinDays, out float low, out float high, out float share);
            int below = UnitsLandingBy(listed, item, low, graded);
            return share > 0f ? TradeMath.BlendedShift(below, UnitsLandingBy(listed, item, high, graded), share) : below;
        }

        private static int UnitsLandingBy(IList<Landing> listed, string item, float withinDays, bool graded)
        {
            int units = 0;
            for (int i = 0; i < listed.Count; i++)
            {
                Landing landing = listed[i];
                if (landing.Item != item) continue;
                float share = Landed(landing.Days, withinDays, graded);
                if (share <= 0f) continue;
                units += PartOf(landing.Units, share);
            }
            return units;
        }

        internal const int AnySource = -1;

        internal const int FromWorkshops = 3;

        internal const int PartsOfAShift = 10;

        internal static int WorthLanding(IList<Landing> listed, string category, float withinDays,
                                         bool graded = false, int from = AnySource)
        {
            if (listed == null || category == null) return 0;
            TradeMath.ForecastHorizon(withinDays, out float low, out float high, out float share);
            int below = WorthLandingBy(listed, category, low, graded, from);
            return share > 0f
                ? TradeMath.BlendedShift(below, WorthLandingBy(listed, category, high, graded, from), share)
                : below;
        }

        private static int WorthLandingBy(IList<Landing> listed, string category, float withinDays, bool graded,
                                          int from)
        {
            int worth = 0;
            for (int i = 0; i < listed.Count; i++)
            {
                Landing landing = listed[i];
                if (landing.Category != category || (from != AnySource && landing.From != from)) continue;
                float share = Landed(landing.Days, withinDays, graded);
                if (share <= 0f) continue;
                worth = TradeMath.ShelfAfterLanding(worth, PartOf(landing.Worth, share));
            }
            return worth;
        }

        internal static int WorthBought(IList<Spending> coming, string category, float withinDays,
                                        bool graded = false)
        {
            if (coming == null || category == null) return 0;
            TradeMath.ForecastHorizon(withinDays, out float low, out float high, out float share);
            int below = WorthBoughtBy(coming, category, low, graded);
            return share > 0f ? TradeMath.BlendedShift(below, WorthBoughtBy(coming, category, high, graded), share) : below;
        }

        private static int WorthBoughtBy(IList<Spending> coming, string category, float withinDays, bool graded)
        {
            int worth = 0;
            for (int i = 0; i < coming.Count; i++)
            {
                Spending spending = coming[i];
                if (spending.Category != category || spending.Worth <= 0) continue;
                float share = Landed(spending.Days, withinDays, graded);
                if (share <= 0f) continue;
                worth = TradeMath.AddedUp(worth, PartOf(spending.Worth, share));
            }
            return worth;
        }

        private static float Landed(float etaDays, float withinDays, bool graded) =>
            graded ? TradeMath.ShareThatLands(etaDays, withinDays)
            : TradeMath.LandsInTime(etaDays, withinDays) ? 1f : 0f;

        private static int PartOf(int whole, float share) =>
            share >= 1f ? whole : (int)Math.Round(whole * (double)share, MidpointRounding.AwayFromZero);

        internal static int WorthUsedUp(IList<Draw> drawn, string category, float withinDays, int usedADay,
                                        int most, bool graded = false, float firstTick = float.NaN)
        {
            if (category == null || most <= 0) return 0;
            bool ticked = usedADay > 0 && firstTick > 0f && firstTick <= 1f;
            int spread = ticked ? 0 : usedADay;
            TradeMath.ForecastHorizon(withinDays, out float low, out float high, out float share);
            int below = WorthUsedUpBy(drawn, category, low, spread, int.MaxValue, graded, firstTick);
            long used = share > 0f
                ? TradeMath.BlendedShift(below, WorthUsedUpBy(drawn, category, high, spread, int.MaxValue, graded, firstTick),
                                         share)
                : below;
            if (ticked) used += (long)usedADay * TradeMath.TownTicksCountBefore(firstTick, withinDays);
            return used >= most ? most : (int)used;
        }

        internal static float DaysOfUse(float firstTick, float withinDays)
        {
            float within = TradeMath.Finite(withinDays, 0f);
            if (!(within > 0f)) return 0f;
            return firstTick > 0f && firstTick <= 1f ? TradeMath.TownTicksCountBefore(firstTick, within) : within;
        }

        private static int WorthUsedUpBy(IList<Draw> drawn, string category, float withinDays, int usedADay, int most,
                                         bool graded, float firstTick)
        {
            double used = usedADay > 0 ? (double)usedADay * DaysOfUse(firstTick, withinDays) : 0d;
            for (int i = 0; drawn != null && i < drawn.Count; i++)
            {
                Draw draw = drawn[i];
                if (draw.Category != category || draw.Worth <= 0) continue;
                float share = Landed(draw.Days, withinDays, graded);
                if (share <= 0f) continue;
                used += draw.Worth * (double)share;
            }
            return used >= most ? most : (int)used;
        }

        internal static int[] WorthParts(IList<Landing> listed, IList<Spending> coming, IList<Draw> drawn,
                                         string category, float withinDays, int usedADay, int shelf, bool town,
                                         int unseenLanding, int unseenBought, float firstTick, out int figure)
        {
            var parts = new int[PartsOfAShift];
            figure = 0;
            if (category == null) return parts;
            for (int from = 0; from <= FromWorkshops; from++) parts[from] = WorthLanding(listed, category, withinDays, true, from);
            parts[4] = unseenLanding;
            int road = WorthBought(coming, category, withinDays, true);
            parts[5] = -road;
            parts[6] = -unseenBought;
            if (town)
            {
                parts[7] = -WorthUsedUp(drawn, category, withinDays, 0, int.MaxValue, true);
                parts[8] = -WorthUsedUp(null, category, withinDays, usedADay, int.MaxValue, true, firstTick);
            }
            int landing = TradeMath.ShelfAfterLanding(WorthLanding(listed, category, withinDays, true), unseenLanding);
            int leaving = TradeMath.AddedUp(road, unseenBought);
            if (town)
            {
                int most = TradeMath.AddedUp(shelf, landing);
                leaving = TradeMath.AddedUp(leaving, WorthUsedUp(drawn, category, withinDays, usedADay, most, true,
                                                                 firstTick));
                if (leaving > most) leaving = most;
            }
            figure = TradeMath.WorthShift(landing, leaving);
            long sum = 0L;
            for (int k = 0; k < PartsOfAShift - 1; k++) sum += parts[k];
            long held = figure - sum;
            parts[PartsOfAShift - 1] = held > int.MaxValue ? int.MaxValue : held < int.MinValue ? int.MinValue : (int)held;
            return parts;
        }

        internal static float[] SoonestLanding(IList<Landing> listed, string category)
        {
            var soonest = new float[FromWorkshops + 1];
            for (int k = 0; k < soonest.Length; k++) soonest[k] = float.NaN;
            for (int i = 0; listed != null && i < listed.Count; i++)
            {
                Landing landing = listed[i];
                if (landing.Category != category || landing.From < 0 || landing.From > FromWorkshops) continue;
                if (landing.Units <= 0 && landing.Worth <= 0) continue;
                if (float.IsNaN(soonest[landing.From]) || landing.Days < soonest[landing.From])
                    soonest[landing.From] = landing.Days;
            }
            return soonest;
        }

        internal static int UnitsLeaving(int worthLeaving, int unitValue)
        {
            if (worthLeaving <= 0 || unitValue <= 0) return 0;
            return worthLeaving / unitValue;
        }

        internal const float NeverRunsOut = -1f;

        internal const int DaysUseIsReadFor = 30;

        internal const float JustAfterTheTick = 1E-4f;

        internal static List<float> Moments(IList<Landing> listed, IList<Spending> coming,
                                            float afterDays, IList<Draw> drawn = null, int usedADay = 0,
                                            float firstTick = float.NaN)
        {
            var when = new List<float>();
            var already = new HashSet<float>();
            for (int i = 0; listed != null && i < listed.Count; i++) Note(when, already, listed[i].Days, afterDays);
            for (int i = 0; coming != null && i < coming.Count; i++) Note(when, already, coming[i].Days, afterDays);
            for (int i = 0; drawn != null && i < drawn.Count; i++) Note(when, already, drawn[i].Days, afterDays);
            if (usedADay > 0 && !float.IsNaN(afterDays) && afterDays < TradeMath.LongerThanAnyRide)
            {
                if (firstTick > 0f && firstTick <= 1f)
                {
                    float tick = firstTick + JustAfterTheTick;
                    if (tick <= afterDays) tick += (float)Math.Floor(afterDays - tick) + 1f;
                    for (int day = 0; day < DaysUseIsReadFor; day++) Note(when, already, tick + day, afterDays);
                }
                else
                {
                    int first = afterDays < 0f ? 1 : (int)Math.Floor(afterDays) + 1;
                    for (int day = first; day < first + DaysUseIsReadFor; day++) Note(when, already, day, afterDays);
                }
            }
            when.Sort();
            return when;
        }

        private static void Note(List<float> when, HashSet<float> already, float days, float afterDays)
        {
            float at = Math.Max(0f, TradeMath.Finite(days, 0f));
            if (at > afterDays && already.Add(at)) when.Add(at);
            float settled = TradeMath.UpToTheQuarterDay(days);
            if (settled > afterDays && already.Add(settled)) when.Add(settled);
        }

        internal static List<(float days, int shelf)> ShelfAhead(
            IList<Landing> listed, IList<Spending> coming,
            string item, string category, int unitValue, int stockNow, float afterDays,
            IList<Draw> drawn = null, int usedADay = 0, float firstTick = float.NaN)
        {
            var curve = new List<(float, int)>();
            if (item == null || unitValue <= 0) return curve;
            List<float> when = Moments(listed, coming, afterDays, drawn, usedADay, firstTick);
            int most = TradeMath.WorthOf(stockNow, unitValue);
            for (int i = 0; i < when.Count; i++)
            {
                float days = when[i];
                int used = WorthUsedUp(drawn, category, days, usedADay,
                                       TradeMath.AddedUp(most, WorthLanding(listed, category, days)), false, firstTick);
                int taken = UnitsLeaving(TradeMath.AddedUp(WorthBought(coming, category, days), used), unitValue);
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
        internal readonly List<string> Aside = new List<string>();
    }

    internal sealed class TownBook
    {
        internal readonly Dictionary<string, int> Units = new Dictionary<string, int>(StringComparer.Ordinal);
        internal readonly Dictionary<string, int> InStore = new Dictionary<string, int>(StringComparer.Ordinal);
        internal readonly Dictionary<string, int> UnitValue = new Dictionary<string, int>(StringComparer.Ordinal);
        internal readonly Dictionary<string, int> UsedADay = new Dictionary<string, int>(StringComparer.Ordinal);
        internal int Gold;
        internal long TakenIn;
        internal long PaidOut;
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

        internal const int Ran = 0, NoInputs = 1, NotWorthIt = 2, PurseShort = 3, CapitalShort = 4, StopsKnown = 5;

        internal static float Watch(float travelCeiling)
        {
            float ceiling = TradeMath.Finite(travelCeiling, 0f);
            if (ceiling <= 0f) return LongestWatch;
            float watch = (float)Math.Ceiling(ceiling * 2f) + 1f;
            return watch < ShortestWatch ? ShortestWatch : watch > LongestWatch ? LongestWatch : watch;
        }

        internal sealed class Days
        {
            internal IList<ShopLine> Lines;
            internal TownBook Town;
            internal Func<string, int, bool, int> Price;
            internal readonly List<string> Kinds = new List<string>();
            internal readonly List<(float days, string kind, int units, int draw)> Landing =
                new List<(float, string, int, int)>();
            internal int Landed;
            internal readonly Dictionary<string, int> Buy = new Dictionary<string, int>(StringComparer.Ordinal);
            internal readonly Dictionary<string, int> Sell = new Dictionary<string, int>(StringComparer.Ordinal);
            internal readonly Dictionary<string, int> MadeNow = new Dictionary<string, int>(StringComparer.Ordinal);
            internal readonly Dictionary<string, int> TakenNow = new Dictionary<string, int>(StringComparer.Ordinal);
            internal readonly Dictionary<string, int> MadeAside = new Dictionary<string, int>(StringComparer.Ordinal);
            internal readonly Dictionary<string, double> Cleared = new Dictionary<string, double>(StringComparer.Ordinal);
            internal float[] ToTown;
        }

        internal static Days Begin(IList<ShopLine> lines, TownBook town, IList<Landing> arriving,
                                   IList<(float days, string category, int units)> bought,
                                   Func<string, int, bool, int> price)
        {
            if (lines == null || lines.Count == 0 || town == null || price == null) return null;
            var days = new Days { Lines = lines, Town = town, Price = price, ToTown = new float[town.Capital.Length] };
            List<string> kinds = days.Kinds;
            foreach (ShopLine line in lines)
            {
                foreach (var (kind, _) in line.Inputs) if (kind != null && !kinds.Contains(kind)) kinds.Add(kind);
                foreach (var (kind, _) in line.Outputs) if (kind != null && !kinds.Contains(kind)) kinds.Add(kind);
            }
            var landing = days.Landing;
            for (int i = 0; arriving != null && i < arriving.Count; i++)
                if (arriving[i].Units > 0 && kinds.Contains(arriving[i].Category))
                    landing.Add((arriving[i].Days, arriving[i].Category, arriving[i].Units, arriving[i].Draw));
            for (int i = 0; bought != null && i < bought.Count; i++)
                if (bought[i].units > 0 && kinds.Contains(bought[i].category))
                    landing.Add((bought[i].days, bought[i].category, -bought[i].units, 0));
            landing.Sort((x, y) => x.days.CompareTo(y.days));
            return days;
        }

        internal static float FirstDay(float firstTick)
        {
            float first = TradeMath.Finite(firstTick, 1f);
            return first <= 0f ? 1f : first;
        }

        internal static void Follow(IList<ShopLine> lines, TownBook town, float firstTick, float watch,
                                    IList<Landing> arriving, IList<(float days, string category, int units)> bought,
                                    Func<string, int, bool, int> price, List<Landing> made, List<Draw> taken)
        {
            if (made == null || taken == null) return;
            Days days = Begin(lines, town, arriving, bought, price);
            if (days == null) return;
            for (float at = FirstDay(firstTick); at <= watch; at += 1f) Day(days, at, null, made, taken);
        }

        internal static void Day(Days days, float at, Func<int, double> paidShare, List<Landing> made, List<Draw> taken,
                                 int[] stopped = null)
        {
            TownBook town = days.Town;
            town.TakenIn = 0L;
            town.PaidOut = 0L;
            var landing = days.Landing;
            for (; days.Landed < landing.Count && landing[days.Landed].days <= at; days.Landed++)
            {
                var (_, kind, units, draw) = landing[days.Landed];
                if (draw > 0 && paidShare != null)
                {
                    double share = paidShare(draw - 1);
                    if (share < 1d)
                        units = (int)Math.Round(units * Math.Max(0d, share), MidpointRounding.AwayFromZero);
                }
                Stock(town, kind, units);
            }
            foreach (string kind in days.Kinds)
            {
                town.InStore.TryGetValue(kind, out int store);
                days.Buy[kind] = days.Price(kind, store, false);
                days.Sell[kind] = days.Price(kind, store, true);
            }
            days.MadeNow.Clear();
            days.TakenNow.Clear();
            days.MadeAside.Clear();
            foreach (ShopLine line in days.Lines)
            {
                if (!(line.Speed > 0f)) continue;
                float progress = TradeMath.Finite(line.Progress, 0f);
                line.Progress = (progress > 1f ? 1f : progress) + line.Speed;
                while (line.Progress >= 1f)
                {
                    bool ran = RunOnce(line, town, days.Buy, days.Sell, days.ToTown, days.MadeNow, days.TakenNow,
                                       days.MadeAside, out int stoppedBy, out double cleared);
                    line.Progress -= 1f;
                    if (stopped != null && stoppedBy >= 0 && stoppedBy < stopped.Length) stopped[stoppedBy]++;
                    if (!ran) break;
                    foreach (var (kind, _) in line.Outputs) days.Cleared[kind] = cleared;
                    PriceAgain(days, line.Inputs, line.Aside);
                    PriceAgain(days, line.Outputs, line.Aside);
                }
            }
            for (int k = 0; k < town.Capital.Length && k < town.Expense.Length; k++)
                if (town.Expense[k] > 0 && town.Capital[k] >= town.Expense[k]) town.Capital[k] -= town.Expense[k];
            foreach (string kind in days.Kinds)
                if (town.UsedADay.TryGetValue(kind, out int worth) && worth > 0 &&
                    town.UnitValue.TryGetValue(kind, out int value) && value > 0)
                    Stock(town, kind, -(int)Math.Round((double)worth / value, MidpointRounding.AwayFromZero));
            foreach (var one in days.MadeNow)
            {
                town.UnitValue.TryGetValue(one.Key, out int value);
                made?.Add(new Landing
                {
                    Category = one.Key, Units = one.Value, Worth = TradeMath.WorthOf(one.Value, value), Days = at
                });
            }
            foreach (var one in days.TakenNow)
            {
                town.UnitValue.TryGetValue(one.Key, out int value);
                taken?.Add(new Draw { Category = one.Key, Worth = TradeMath.WorthOf(one.Value, value), Days = at });
            }
        }

        private static void PriceAgain(Days days, List<(string category, int count)> kinds, List<string> aside)
        {
            foreach (var (kind, _) in kinds)
            {
                if (kind == null || aside.Contains(kind)) continue;
                days.Town.InStore.TryGetValue(kind, out int store);
                days.Buy[kind] = days.Price(kind, store, false);
                days.Sell[kind] = days.Price(kind, store, true);
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
                                    Dictionary<string, int> made, Dictionary<string, int> taken,
                                    Dictionary<string, int> madeAside, out int stoppedBy, out double cleared)
        {
            stoppedBy = Ran;
            cleared = double.NaN;
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
                    if (have < count)
                    {
                        stoppedBy = NoInputs;
                        return false;
                    }
                    cost += (long)buy[kind] * count;
                }
            long income = 0;
            foreach (var (kind, count) in line.Outputs) income += (long)sell[kind] * count;
            double bar = line.Hidden || !(line.Pace > 0f) ? cost : cost + PayOverInputsPerPace / line.Pace;
            cleared = income - bar;
            if (income <= bar)
            {
                stoppedBy = NotWorthIt;
                return false;
            }
            if (line.TradeGoodsOnly && town.Gold < income)
            {
                stoppedBy = PurseShort;
                return false;
            }
            if (owned && town.Capital[k] < cost)
            {
                stoppedBy = CapitalShort;
                return false;
            }

            if (fromWarehouse) town.Warehouse[k] -= need;
            else
                foreach (var (kind, count) in line.Inputs)
                {
                    Stock(town, kind, -count);
                    Tally(taken, kind, count);
                    if (!line.TradeGoodsOnly) continue;
                    if (owned) town.Capital[k] -= buy[kind];
                    town.Gold += buy[kind];
                    town.TakenIn += buy[kind];
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
                    if (line.Aside.Contains(kind))
                    {
                        Tally(madeAside, kind, 1);
                        continue;
                    }
                    Stock(town, kind, 1);
                    Tally(made, kind, 1);
                    if (!line.TradeGoodsOnly) continue;
                    int paid = Math.Min(MostPaidForAnOutput, buy[kind]);
                    if (owned) town.Capital[k] += paid;
                    town.Gold -= paid;
                    town.PaidOut += paid;
                }
            return true;
        }
    }
}
