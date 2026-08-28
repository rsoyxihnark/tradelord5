using System;

namespace TradeLord
{
    public static class TradeMath
    {
        public static bool PolicyAllows(int policy, bool buying) =>
            buying ? policy == Options.PolicyBuyOnly || policy == Options.PolicyBuySell
                   : policy == Options.PolicySellOnly || policy == Options.PolicyBuySell;

        public static int Credit(int proceeds, int basis, int unpaidWorth)
        {
            if (basis > 0) return proceeds - basis;
            int gain = proceeds - unpaidWorth;
            return gain > 0 ? gain : 0;
        }

        public static bool ProfitAcceptable(int costBasis, int townSellPrice, float margin) =>
            costBasis > 0
                ? townSellPrice >= costBasis * (1f + margin)
                : townSellPrice > 0;

        public static float Realizable(int farSellPrice, float safetyFactor) =>
            farSellPrice * safetyFactor;

        public static bool BuyAcceptable(int buyPrice, float realizable, float margin) =>
            buyPrice > 0 && realizable >= buyPrice * (1f + margin);

        public static int Budget(int gold, int goldReserve, int maxSpendPerVisit,
                                 int spentThisVisit, int spentThisPass)
        {
            int left = gold - spentThisPass - goldReserve;
            return maxSpendPerVisit > 0
                ? Math.Min(left, maxSpendPerVisit - spentThisVisit - spentThisPass)
                : left;
        }
    }
}
