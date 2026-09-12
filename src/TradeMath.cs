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
            if (rec == null || count <= 0) return;
            rec.TotalPaid += totalPaid;
            rec.Count += count;
            rec.LastUnitPaid = (int)Math.Round((double)totalPaid / count);
        }

        public static void DrainSale(PurchaseRecord rec, int count)
        {
            if (rec == null || rec.Count <= 0) return;
            int drain = Math.Min(count, rec.Count);
            int left = rec.Count - drain;
            if (left <= 0) { rec.Count = 0; rec.TotalPaid = 0; return; }
            int unit = (int)Math.Round((double)rec.TotalPaid / rec.Count);
            rec.Count = left;
            rec.TotalPaid = unit > 0 ? unit * left : 0;
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

        public static int WorthOf(int units, int unitValue)
        {
            if (units <= 0 || unitValue <= 0) return 0;
            long worth = (long)units * unitValue;
            return worth > int.MaxValue ? int.MaxValue : (int)worth;
        }

        public static int ShelfAfterLanding(int inStoreValue, int landingWorth)
        {
            long after = (long)inStoreValue + landingWorth;
            if (after < 0L) return 0;
            return after > int.MaxValue ? int.MaxValue : (int)after;
        }

        public static int StockAfterLanding(int stock, int landingUnits)
        {
            long after = (long)stock + (landingUnits < 0 ? 0 : landingUnits);
            return after > int.MaxValue ? int.MaxValue : (int)after;
        }

        public static float PullOfAPrice(float priceFactor)
        {
            float pull = 1f - priceFactor;
            return pull < 0f ? 0f : (pull > 1f ? 1f : pull);
        }

        public static int ShareOfAPurse(int purse, float pull, float pullAcrossTheMarket)
        {
            if (purse <= 0 || pull <= 0f || pullAcrossTheMarket <= 0f) return 0;
            double share = (double)purse * pull / pullAcrossTheMarket;
            if (share > int.MaxValue) return int.MaxValue;
            return share < 0d ? 0 : (int)share;
        }

        public static int WorthShift(int landing, int leaving)
        {
            long shift = (long)landing - (leaving < 0 ? 0 : leaving);
            if (shift > int.MaxValue) return int.MaxValue;
            return shift < int.MinValue ? int.MinValue : (int)shift;
        }

        public static bool StillComing(int units, int onTheShelfNow) =>
            units > (onTheShelfNow < 0 ? 0 : onTheShelfNow);

        public static bool LandsInTime(float etaDays, float horizonDays) =>
            etaDays <= horizonDays;

        public const float HorizonStep = 0.25f;

        public static float ToTheQuarterDay(float days)
        {
            if (days <= 0f) return 0f;
            double steps = Math.Floor(days / HorizonStep + 0.5d);
            return steps <= 0d ? 0f : (float)(steps * HorizonStep);
        }

        public static float RunLandsIn(float progress, float runDays)
        {
            float left = 1f - (progress < 0f ? 0f : (progress > 1f ? 1f : progress));
            float days = left * (runDays < 0f ? 0f : runDays);
            return days < 0f ? 0f : days;
        }

        public static bool StandsBetter(int count, int value, int bestCount, int bestValue) =>
            count > 0 && (bestCount <= 0 || count > bestCount ||
                          (count == bestCount && value < bestValue));

        public static int MissedBy(int said, int happened)
        {
            long missed = (long)happened - said;
            if (missed > int.MaxValue) return int.MaxValue;
            return missed < int.MinValue ? int.MinValue : (int)missed;
        }

        public const float NoShareToGive = -1f;

        public static float OffByShare(int said, int happened)
        {
            if (said == 0) return NoShareToGive;
            double off = Math.Abs((double)happened - said) / Math.Abs((double)said);
            return off > float.MaxValue ? float.MaxValue : (float)off;
        }

        public static float MeanOf(float total, int counted) =>
            counted <= 0 ? 0f : total / counted;

        public static float HeldShare(int promised, int found) =>
            promised <= 0 ? NoShareToGive : (found < 0 ? 0f : (float)found / promised);

        public const float GraceDays = 1f;

        public static bool WorthScoring(float saidWithinDays, float daysSince) =>
            daysSince <= (saidWithinDays < 0f ? 0f : saidWithinDays) * 2f + GraceDays;

        public const int Bands = 4;

        public static int BandOf(float confidence)
        {
            if (confidence < 0.25f) return 0;
            if (confidence < 0.5f) return 1;
            return confidence < 0.75f ? 2 : 3;
        }

        public static float DaysSince(float thenHours, float nowHours)
        {
            float days = (nowHours - thenHours) / 24f;
            return days < 0f ? 0f : days;
        }

        public static float EtaDays(float distance, float speed)
        {
            if (distance <= 0f) return 0f;
            float pace = speed <= StandingStill ? WalkingPace : speed;
            return distance / (pace * 24f);
        }

        public static int Budget(int gold, int goldReserve, int maxSpendPerVisit,
                                 int spentThisVisit)
        {
            int left = gold - goldReserve;
            return maxSpendPerVisit > 0
                ? Math.Min(left, maxSpendPerVisit - spentThisVisit)
                : left;
        }
    }
}
