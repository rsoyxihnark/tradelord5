using System;

namespace TradeLord
{
    public static class Confidence
    {
        private const float Unsimulated = 0.85f;

        public const float NotKnown = -1f;

        private const float Gone = 0.25f;

        private const float Patience = 2f;

        public static float Holds(float runsOutInDays, float daysToTheBuyTown)
        {
            if (runsOutInDays < 0f || float.IsNaN(runsOutInDays)) return 1f;
            float waited = daysToTheBuyTown > 0f && !float.IsNaN(daysToTheBuyTown) ? daysToTheBuyTown : 0f;
            float slack = runsOutInDays - waited;
            if (slack <= 0f) return Gone;
            return Clamp(Gone + (1f - Gone) * (slack / (slack + Patience)));
        }

        public const int EnoughArrivals = 5;

        public const float MostItDiscounts = 0.25f;

        public static float AsPromisesHaveHeld(float score, int arrivals, float held)
        {
            if (score <= 0f || arrivals < EnoughArrivals) return score;
            if (held < 0f || float.IsNaN(held) || float.IsInfinity(held)) return score;
            float trust = held > 1f ? 1f : held;
            float weight = (float)arrivals / (arrivals + EnoughArrivals);
            float factor = 1f - (1f - trust) * weight;
            float floor = 1f - MostItDiscounts;
            if (factor < floor) factor = floor;
            return score * (factor > 1f ? 1f : factor);
        }

        internal enum RecordAtAMarket
        {
            Unused, NotYet, TakesNothingOff, Lowers
        }

        internal static RecordAtAMarket WhatAMarketsRecordDoes(bool counted, int arrivals, float held)
        {
            if (!counted) return RecordAtAMarket.Unused;
            if (arrivals < EnoughArrivals) return RecordAtAMarket.NotYet;
            return AsPromisesHaveHeld(1f, arrivals, held) < 1f
                ? RecordAtAMarket.Lowers
                : RecordAtAMarket.TakesNothingOff;
        }

        public static float Of(bool simulated, int flatProfit, int simulatedProfit,
                                 int stock, int units, float travelDays,
                                 int caravans, float dataAgeDays,
                                 float runsOutInDays = NotKnown, float daysToTheBuyTown = 0f)
        {
            float resilience = Unsimulated;
            if (simulated && flatProfit > 0)
                resilience = Clamp((float)simulatedProfit / flatProfit);

            float depth = 1f;
            if (stock > 0 && units > 0)
                depth = Clamp(stock / (units * 1.5f));

            float haste = 1f / (1f + Math.Max(travelDays, 0f) / 3f);
            float quiet = runsOutInDays >= 0f
                ? Holds(runsOutInDays, daysToTheBuyTown)
                : 1f / (1f + Math.Max(caravans, 0) * 0.15f);
            float fresh = dataAgeDays < 0f ? 1f : 1f / (1f + dataAgeDays / 5f);

            float c = resilience * depth * haste * quiet * fresh;
            return c < 0.01f ? 0.01f : (c > 1f ? 1f : c);
        }

        private static float Clamp(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
