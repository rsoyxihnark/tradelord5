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

        public const int NoRecordedBasis = -1;

        public const float SameModifier = 0.0001f;

        public static bool Unchanged(float mod, float neutral) =>
            Math.Abs(mod - neutral) < SameModifier;

        public static int MostThatHolds(int highest, Func<int, bool> holds)
        {
            int lowest = 0;
            while (lowest < highest)
            {
                int mid = lowest + (highest - lowest + 1) / 2;
                if (holds(mid)) lowest = mid; else highest = mid - 1;
            }
            return lowest;
        }

        public static void AddPurchase(PurchaseRecord rec, int count, int totalPaid)
        {
            if (rec == null) return;
            rec.TotalPaid += totalPaid;
            rec.Count += count;
            if (count > 0) rec.LastUnitPaid = (int)Math.Round((double)totalPaid / count);
        }

        public static void DrainSale(PurchaseRecord rec, int count)
        {
            if (rec == null || rec.Count <= 0) return;
            int drain = Math.Min(count, rec.Count);
            rec.TotalPaid -= (int)Math.Round((double)rec.TotalPaid / rec.Count * drain);
            rec.Count -= drain;
            if (rec.Count <= 0) { rec.Count = 0; rec.TotalPaid = 0; }
            else if (rec.TotalPaid < 0) rec.TotalPaid = 0;
        }

        public static bool SkipTheUnitsYouPaidFor(bool basisIsMarket, ref int remaining, ref int paidLeft)
        {
            if (basisIsMarket || paidLeft <= 0 || remaining <= paidLeft) return false;
            remaining -= paidLeft;
            paidLeft = 0;
            return true;
        }

        public static int UnitBasis(PurchaseRecord rec, int mode)
        {
            if (mode == 2 || rec == null || rec.Count <= 0) return NoRecordedBasis;
            return mode == 1 && rec.LastUnitPaid > 0
                ? rec.LastUnitPaid
                : (int)Math.Round((double)rec.TotalPaid / rec.Count);
        }

        public static int Reserve(int goldReserve, int keepWageDays, int totalWage)
        {
            long hold = goldReserve < 0 ? 0 : goldReserve;
            if (keepWageDays > 0 && totalWage > 0) hold += (long)keepWageDays * totalWage;
            return hold > int.MaxValue ? int.MaxValue : (int)hold;
        }

        public const float StandingStill = 0.01f;

        public const float WalkingPace = 5f;

        public static void SpeedsInEffect(float partySpeed, float fleetSpeed,
                                          out float land, out float sea)
        {
            land = partySpeed <= StandingStill ? WalkingPace : partySpeed;
            sea = fleetSpeed <= StandingStill ? land : fleetSpeed;
        }

        public static float FleetSpeed(float total, int count, float slowest) =>
            count <= 0 ? 0f : (total / count + slowest) * 0.5f;

        public static float DaysAtSpeed(float distance, float landRatio,
                                        float landSpeed, float seaSpeed)
        {
            if (distance <= 0f) return 0f;
            float landLeg = distance * landRatio;
            float seaLeg = distance * (1f - landRatio);
            return (landLeg / landSpeed + seaLeg / seaSpeed) / 24f;
        }

        public static float DaysAtBestSpeed(float distance, float landSpeed, float seaSpeed) =>
            distance <= 0f ? 0f : distance / (Math.Max(landSpeed, seaSpeed) * 24f);

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
