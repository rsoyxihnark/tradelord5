using System;
using System.Collections.Generic;

namespace TradeLord
{
    internal struct Reach<T>
    {
        internal T Where;
        internal int Price;
        internal float Straight;
        internal float Days;
    }

    internal static class MarketRank
    {
        internal const int TopCacheSize = 8;

        private static class Order<T>
        {
            internal static readonly Comparison<Reach<T>> DearestFirst =
                (x, y) => Rank(true, x.Price, x.Days, y.Price, y.Days);

            internal static readonly Comparison<Reach<T>> CheapestFirst =
                (x, y) => Rank(false, x.Price, x.Days, y.Price, y.Days);
        }

        internal static float Ceiling(bool village, Options s)
        {
            float cap = s.MaxTravelDaysTown;
            float vcap = s.MaxTravelDaysVillage;
            if (village && vcap > 0f && (cap <= 0f || vcap < cap)) cap = vcap;
            return cap;
        }

        internal static bool WithinCeiling(bool village, float days, Options s)
        {
            float cap = Ceiling(village, s);
            return cap <= 0f || days <= cap;
        }

        internal static int Rank(bool selling, int xPrice, float xDays, int yPrice, float yDays)
        {
            int p = selling ? yPrice.CompareTo(xPrice) : xPrice.CompareTo(yPrice);
            return p != 0 ? p : xDays.CompareTo(yDays);
        }

        internal static void Keep<T>(List<Reach<T>> kept, Reach<T> one, bool selling)
        {
            if (kept.Count == TopCacheSize &&
                Rank(selling, one.Price, one.Straight,
                     kept[TopCacheSize - 1].Price, kept[TopCacheSize - 1].Straight) >= 0) return;
            int at = kept.Count;
            while (at > 0 && Rank(selling, one.Price, one.Straight,
                                  kept[at - 1].Price, kept[at - 1].Straight) < 0) at--;
            kept.Insert(at, one);
            if (kept.Count > TopCacheSize) kept.RemoveAt(TopCacheSize);
        }

        internal static List<Reach<T>> Settled<T>(List<Reach<T>> kept, bool selling)
        {
            var top = new List<Reach<T>>(kept);
            top.Sort(selling ? Order<T>.DearestFirst : Order<T>.CheapestFirst);
            return top;
        }
    }
}
