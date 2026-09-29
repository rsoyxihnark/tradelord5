using System;
using System.Collections.Generic;

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

        public static int[] WhatTheMarkTakes(Func<int, int> rungAt, int carried, int worth,
                                             float margin, int purse)
        {
            if (rungAt == null || carried <= 0) return new int[0];
            long fetched = 0L;
            var taken = new int[carried];
            int count = 0;
            for (int u = 0; u < carried; u++)
            {
                int price = rungAt(u);
                if (price <= 0 || !ProfitAcceptable(worth, price, margin)) break;
                if (purse >= 0 && fetched + price > purse) break;
                fetched += price;
                taken[count++] = price;
            }
            Array.Resize(ref taken, count);
            return taken;
        }

        public static int MostToPayOverTheCheapest(int cheapest, float tolerance)
        {
            if (cheapest <= 0) return 0;
            if (float.IsNaN(tolerance) || tolerance <= 1f) return cheapest;
            double most = Math.Floor((double)cheapest * tolerance);
            return most > int.MaxValue ? int.MaxValue : (int)most;
        }

        public const float DriftWorthSaying = 0.05f;

        public const int KeptForever = 0;

        public static bool WorthKeeping(float capturedDay, float now, int shelfLifeDays) =>
            shelfLifeDays <= KeptForever || now - capturedDay <= shelfLifeDays;

        public static bool ReadingIsNew(float day, float lastDay) =>
            Math.Floor(day) > Math.Floor(lastDay);

        public static int Drift(int now, int was)
        {
            if (now <= 0 || was <= 0) return 0;
            float moved = (float)(now - was) / was;
            if (moved >= DriftWorthSaying) return 1;
            return moved <= -DriftWorthSaying ? -1 : 0;
        }

        public static float Realizable(int farSellPrice, float safetyFactor) =>
            Finite(farSellPrice * safetyFactor, 0f);

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
                int mid = lowest + (int)(((long)highest - lowest + 1) / 2);
                if (holds(mid)) lowest = mid; else highest = mid - 1;
            }
            return lowest;
        }

        public static void AddPurchase(PurchaseRecord rec, int count, int totalPaid, int meantEach = 0)
        {
            if (rec == null || count <= 0) return;
            List<Batch> batches = BatchesOf(rec);
            rec.TotalPaid += totalPaid;
            rec.Count += count;
            rec.LastUnitPaid = (int)Math.Round((double)totalPaid / count);
            AddBatch(batches, new Batch
            {
                Unit = rec.LastUnitPaid > 0 ? rec.LastUnitPaid : 0, Count = count, Meant = meantEach > 0 ? meantEach : 0
            });
        }

        public static void DrainSale(PurchaseRecord rec, int count) => DrainSale(rec, count, 0, out _, out _);

        public static void DrainSale(PurchaseRecord rec, int count, int unitPaid, out int planned, out long meant)
        {
            planned = 0;
            meant = 0L;
            if (rec == null || rec.Count <= 0 || count <= 0) return;
            int drain = Math.Min(count, rec.Count);
            TakeFromTheBatches(BatchesOf(rec), drain, unitPaid, ref planned, ref meant);
            int left = rec.Count - drain;
            if (left <= 0) { rec.Count = 0; rec.TotalPaid = 0; return; }
            int unit = (int)Math.Round((double)rec.TotalPaid / rec.Count);
            rec.Count = left;
            rec.TotalPaid = unit > 0 ? unit * left : 0;
        }

        private static void TakeFromTheBatches(List<Batch> batches, int units, int unitPaid, ref int planned,
                                               ref long meant)
        {
            while (units > 0 && batches.Count > 0)
            {
                int at = unitPaid > 0 ? batches.FindIndex(one => one.Unit == unitPaid) : 0;
                if (at < 0) at = 0;
                unitPaid = 0;
                Batch taken = batches[at];
                int off = at > 0 ? 1 : Math.Min(units, taken.Count);
                if (taken.Meant > 0)
                {
                    planned += off;
                    meant += (long)taken.Meant * off;
                }
                units -= off;
                taken.Count -= off;
                if (taken.Count <= 0) batches.RemoveAt(at); else batches[at] = taken;
            }
        }

        public const int MostBatchesKept = 8;

        public static bool BatchesAddUp(PurchaseRecord rec)
        {
            if (rec?.Batches == null) return false;
            long units = 0L;
            for (int i = 0; i < rec.Batches.Count; i++)
            {
                Batch one = rec.Batches[i];
                if (one.Count <= 0 || one.Unit < 0 || one.Meant < 0) return false;
                units += one.Count;
            }
            return units == rec.Count;
        }

        private static List<Batch> BatchesOf(PurchaseRecord rec)
        {
            if (rec.Batches == null) rec.Batches = new List<Batch>();
            if (BatchesAddUp(rec)) return rec.Batches;
            rec.Batches.Clear();
            if (rec.Count > 0)
            {
                int unit = (int)Math.Round((double)rec.TotalPaid / rec.Count);
                rec.Batches.Add(new Batch { Unit = unit > 0 ? unit : 0, Count = rec.Count });
            }
            return rec.Batches;
        }

        private static void AddBatch(List<Batch> batches, Batch added)
        {
            for (int i = 0; i < batches.Count; i++)
            {
                if (batches[i].Unit != added.Unit || batches[i].Meant != added.Meant) continue;
                Batch same = batches[i];
                same.Count += added.Count;
                batches[i] = same;
                return;
            }
            int at = 0;
            while (at < batches.Count && (batches[at].Unit < added.Unit ||
                                          (batches[at].Unit == added.Unit && batches[at].Meant < added.Meant))) at++;
            batches.Insert(at, added);
            while (batches.Count > MostBatchesKept) MergeTheClosest(batches);
        }

        private static void MergeTheClosest(List<Batch> batches)
        {
            int closest = ClosestPair(batches, alike: false);
            int alike = ClosestPair(batches, alike: true);
            if (alike >= 0 && Apart(batches, alike) <= 2L * Apart(batches, closest)) closest = alike;
            Batch low = batches[closest], high = batches[closest + 1];
            long units = (long)low.Count + high.Count;
            Batch merged;
            merged.Count = (int)Math.Min(units, int.MaxValue);
            merged.Unit = (int)Math.Round(((double)low.Unit * low.Count + (double)high.Unit * high.Count) / units);
            merged.Meant = low.Meant > 0 && high.Meant > 0
                ? (int)Math.Round(((double)low.Meant * low.Count + (double)high.Meant * high.Count) / units)
                : 0;
            batches[closest] = merged;
            batches.RemoveAt(closest + 1);
        }

        private static int ClosestPair(List<Batch> batches, bool alike)
        {
            int closest = -1;
            long gap = long.MaxValue;
            for (int i = 0; i + 1 < batches.Count; i++)
            {
                if (alike && (batches[i].Meant > 0) != (batches[i + 1].Meant > 0)) continue;
                long apart = Apart(batches, i);
                if (apart >= gap) continue;
                gap = apart;
                closest = i;
            }
            return closest;
        }

        private static long Apart(List<Batch> batches, int at)
        {
            long apart = (long)batches[at + 1].Unit - batches[at].Unit;
            return apart < 0 ? -apart : apart;
        }

        public static int LeaveOut(ref int[] dearer, List<int> drawn)
        {
            if (dearer == null || dearer.Length == 0 || drawn == null || drawn.Count == 0) return 0;
            var left = new List<int>(dearer);
            int taken = 0;
            for (int i = 0; i < drawn.Count; i++)
                if (left.Remove(drawn[i])) taken++;
            dearer = left.Count == 0 ? null : left.ToArray();
            return taken;
        }

        public static int WhatTheAverageCovers(int basis, float margin)
        {
            if (basis <= 0) return basis;
            float over = Finite(margin, 0f);
            double covered = Math.Floor(basis * (1d + (over > 0f ? over : 0f)));
            return covered >= int.MaxValue ? int.MaxValue : (int)covered;
        }

        public static int[] DearerThan(PurchaseRecord rec, int basis)
        {
            if (rec == null || rec.Count <= 0 || basis < 0 || !BatchesAddUp(rec)) return null;
            long units = 0L;
            for (int i = 0; i < rec.Batches.Count; i++)
                if (rec.Batches[i].Unit > basis) units += rec.Batches[i].Count;
            if (units == 0L || units > rec.Count) return null;
            var each = new int[units];
            int at = 0;
            for (int i = 0; i < rec.Batches.Count; i++)
                if (rec.Batches[i].Unit > basis)
                    for (int u = 0; u < rec.Batches[i].Count; u++) each[at++] = rec.Batches[i].Unit;
            Array.Sort(each);
            return each;
        }

        public const int NoUnitThisPriceSells = int.MaxValue;

        public struct DearFirst
        {
            private readonly int[] _dearer;
            private readonly int _worth;
            private int _lowest;
            private int _top;
            private int _others;
            private int _picked;

            public DearFirst(int[] dearer, int others, int worth, int drawnFromTheDear = 0)
            {
                _dearer = dearer;
                _worth = worth;
                int many = dearer == null ? 0 : dearer.Length;
                _lowest = drawnFromTheDear <= 0 ? 0 : drawnFromTheDear < many ? drawnFromTheDear : many;
                _top = many - 1;
                _others = others > 0 ? others : 0;
                _picked = -1;
            }

            public int Floor(int price, float margin)
            {
                _picked = -1;
                while (_top >= _lowest && !ProfitAcceptable(_dearer[_top], price, margin)) _top--;
                if (_top >= _lowest)
                {
                    _picked = _top;
                    return _dearer[_top];
                }
                return _others > 0 ? _worth : NoUnitThisPriceSells;
            }

            public int Took()
            {
                bool dear = _picked >= 0 && _picked == _top;
                _picked = -1;
                if (dear) return _dearer[_top--];
                if (_others > 0)
                {
                    _others--;
                    return 0;
                }
                return _dearer != null && _lowest <= _top ? _dearer[_lowest++] : 0;
            }
        }

        public static bool SkipTheUnitsYouPaidFor(bool basisIsMarket, ref int remaining, ref int paidLeft)
        {
            if (basisIsMarket || paidLeft <= 0 || remaining <= paidLeft) return false;
            remaining -= paidLeft;
            paidLeft = 0;
            return true;
        }

        public static bool SetTheBoughtUnitsAside(ref int remaining, ref int paidLeft)
        {
            if (paidLeft <= 0 || remaining <= paidLeft) return false;
            remaining -= paidLeft;
            paidLeft = 0;
            return true;
        }

        public static bool KeepToTheBoughtUnits(ref int remaining, int paidLeft)
        {
            if (paidLeft <= 0 || remaining <= 0) return false;
            if (remaining > paidLeft) remaining = paidLeft;
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

        public const float FurthestThereIs = float.MaxValue;

        public static float Finite(float value, float ifNot) =>
            float.IsNaN(value) || float.IsInfinity(value) ? ifNot : value;

        public const float LongerThanAnyRide = 1000f;

        public static bool OutOfReach(float days) =>
            float.IsNaN(days) || float.IsInfinity(days) || days >= LongerThanAnyRide;

        public const float MostAForecastMayMoveAPrice = 0.5f;

        public static int ForecastWithin(int live, int forecast)
        {
            if (live <= 0 || forecast <= 0) return forecast;
            int most = (int)(live * (1f + MostAForecastMayMoveAPrice));
            int least = (int)(live * (1f - MostAForecastMayMoveAPrice));
            if (forecast > most) return most;
            return forecast < least ? least : forecast;
        }

        public static int LandingWithinReach(int live, int landed, int firstUnit, Func<int, int> firstUnitAt)
        {
            if (landed == 0 || ForecastWithin(live, firstUnit) == firstUnit) return landed;
            int most = landed > 0 ? landed : (int)Math.Min(-(long)landed, int.MaxValue);
            int held = MostThatHolds(most, shift =>
            {
                int price = firstUnitAt(landed > 0 ? shift : -shift);
                return ForecastWithin(live, price) == price;
            });
            return landed > 0 ? held : -held;
        }

        public static int NoFurtherThan(int landed, int shift) =>
            landed > 0 ? Math.Min(shift, landed) : Math.Max(shift, landed);

        public const float NoTripCountsShorterThan = 0.5f;

        public static float PerDay(float amount, float days)
        {
            if (amount <= 0f || float.IsNaN(amount) || float.IsNaN(days)) return 0f;
            float over = days > NoTripCountsShorterThan ? days : NoTripCountsShorterThan;
            return amount / over;
        }

        public static float EarnedPerDay(int sellPrice, int paid, float days)
        {
            if (sellPrice <= 0 || sellPrice <= paid) return 0f;
            return PerDay(sellPrice - paid, days);
        }

        public const float TheMarkedTownHoldsBy = 1.2f;

        public static float RateTheMarkHolds(float rate, bool marked)
        {
            if (!marked || rate <= 0f || float.IsNaN(rate)) return rate;
            float held = rate * TheMarkedTownHoldsBy;
            return float.IsInfinity(held) ? rate : held;
        }

        public static float CeilingTheMarkHolds(float cap, bool marked)
        {
            if (!marked || cap <= 0f || float.IsNaN(cap)) return cap;
            float held = cap * TheMarkedTownHoldsBy;
            return float.IsInfinity(held) ? cap : held;
        }

        public static int MostYouCouldTake(int unitPrice, float unitWeight, int stocked,
                                           int spendable, float room, int cap)
        {
            if (unitPrice <= 0 || stocked <= 0 || spendable <= 0) return 0;
            long take = stocked;
            if (cap > 0 && cap < take) take = cap;
            long affordable = spendable / unitPrice;
            if (affordable < take) take = affordable;
            if (unitWeight > 0.01f)
            {
                if (room <= 0f || float.IsNaN(room)) return 0;
                long fits = (long)(room / unitWeight);
                if (fits < take) take = fits;
            }
            if (take <= 0) return 0;
            return take > int.MaxValue ? int.MaxValue : (int)take;
        }

        public static bool EnoughOnTheShelf(int stocked, int unitWorth, int minUnits, int minWorth)
        {
            if (minUnits <= 0) return true;
            if (stocked >= minUnits) return true;
            if (minWorth <= 0 || stocked <= 0 || unitWorth <= 0) return false;
            return (long)stocked * unitWorth >= minWorth;
        }

        public const float ACaravanSellsAbove = 1.1f;

        public const float ACaravanSellsThisEagerly = 3f;

        public static int WhatACaravanUnloads(int carried, float priceFactor, float dailyBudget,
                                              int unitPrice)
        {
            if (carried <= 0 || unitPrice <= 0) return 0;
            float over = Finite(priceFactor, 0f) - ACaravanSellsAbove;
            float budget = Finite(dailyBudget, 0f);
            if (over <= 0f || budget <= 0f) return 0;
            float units = budget * over * ACaravanSellsThisEagerly / unitPrice;
            if (units <= 0f) return 0;
            return units >= carried ? carried : (int)units;
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
            count <= 0 ? 0f : Finite((total / count + slowest) * 0.5f, 0f);

        public static float DaysAtSpeed(float distance, float landRatio,
                                        float landSpeed, float seaSpeed)
        {
            if (distance <= 0f) return 0f;
            float landLeg = distance * landRatio;
            float seaLeg = distance * (1f - landRatio);
            float days = Finite((landLeg / landSpeed + seaLeg / seaSpeed) / 24f, FurthestThereIs);
            return OutOfReach(days) ? FurthestThereIs : days;
        }

        public static float DaysAtBestSpeed(float distance, float landSpeed, float seaSpeed)
        {
            if (distance <= 0f) return 0f;
            float days = Finite(distance / (Math.Max(landSpeed, seaSpeed) * 24f), FurthestThereIs);
            return OutOfReach(days) ? FurthestThereIs : days;
        }

        public static int WorthUsedUpADay(float budget, int price, int unitValue)
        {
            if (float.IsNaN(budget) || float.IsInfinity(budget) || budget <= 0f || price <= 0 || unitValue <= 0)
                return 0;
            double worth = (double)budget / price * unitValue;
            return worth >= int.MaxValue ? int.MaxValue : (int)worth;
        }

        public static float WeightTheBudgetCanStillBuy(float weight, int cost, int budget)
        {
            if (weight <= 0f || float.IsNaN(weight) || cost <= 0 || budget <= 0) return 0f;
            return budget >= cost ? weight : weight * ((float)budget / cost);
        }

        public static float ProfitTheBudgetCanStillBuy(float profit, int cost, int budget) =>
            WeightTheBudgetCanStillBuy(profit, cost, budget);

        public static int WorthOf(int units, int unitValue)
        {
            if (units <= 0 || unitValue <= 0) return 0;
            long worth = (long)units * unitValue;
            return worth > int.MaxValue ? int.MaxValue : (int)worth;
        }

        public const int RichAtThisManyLimits = 5;

        public static int AdaptiveSpendCap(int baseCap, long purseBeforeBuying, bool adaptive)
        {
            if (!adaptive || baseCap <= 0) return baseCap;
            long rich = (long)baseCap * RichAtThisManyLimits;
            if (purseBeforeBuying <= rich) return baseCap;
            double more = baseCap * Math.Log((double)purseBeforeBuying / rich, 2d);
            if (double.IsNaN(more) || more <= 0d) return baseCap;
            double cap = baseCap + Math.Floor(more);
            return cap >= int.MaxValue ? int.MaxValue : (int)cap;
        }

        public static int PerUnit(int gold, int units) =>
            units <= 0 ? 0 : (int)Math.Round((double)gold / units, MidpointRounding.AwayFromZero);

        public const float SmallestLearningRateThatTeaches = 0.001f;

        public static bool StillLearns(float learningRate) => learningRate >= SmallestLearningRateThatTeaches;

        public static int FewestThatLets(int most, Func<int, bool> lets)
        {
            if (lets == null) return 0;
            for (int more = 1; more <= most; more++)
                if (lets(more)) return more;
            return 0;
        }

        public static int AtThisQuality(int plainPrice, int plainValue, int qualityValue)
        {
            if (plainPrice <= 0 || plainValue <= 0 || qualityValue <= 0 || qualityValue == plainValue)
                return plainPrice;
            long priced = (long)plainPrice * qualityValue / plainValue;
            if (priced < 1L) return 1;
            return priced > int.MaxValue ? int.MaxValue : (int)priced;
        }

        public const float ShelfTheForecastMayNotEmptyBelow = 0.5f;

        public static int ShelfAfterLanding(int inStoreValue, int landingWorth)
        {
            long after = (long)inStoreValue + landingWorth;
            long floor = inStoreValue > 0
                ? (long)(inStoreValue * ShelfTheForecastMayNotEmptyBelow) : 0L;
            if (after < floor) after = floor;
            if (after < 0L) return 0;
            return after > int.MaxValue ? int.MaxValue : (int)after;
        }

        public static int StockAfterLanding(int stock, int landingUnits)
        {
            long after = (long)stock + (landingUnits < 0 ? 0 : landingUnits);
            return after > int.MaxValue ? int.MaxValue : (int)after;
        }

        public static float RoomToFill(float capacity, float carried, float cargoShare)
        {
            float ceiling = cargoShare > 0f && cargoShare < 1f ? capacity * cargoShare : capacity;
            return ceiling - carried;
        }

        public static float PartyShareOfProfit(int profit, float share)
        {
            if (profit <= 0 || share <= 0f) return 0f;
            return Finite(profit * share, 0f);
        }

        public static int StockAfterShift(int stock, int landingUnits, int leavingUnits)
        {
            long after = (long)StockAfterLanding(stock, landingUnits) -
                         (leavingUnits < 0 ? 0 : leavingUnits);
            if (after < 0L) return 0;
            return after > int.MaxValue ? int.MaxValue : (int)after;
        }

        public static float PullOfAPrice(float priceFactor)
        {
            float pull = 1f - Finite(priceFactor, 1f);
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
            days = Finite(days, 0f);
            if (days <= 0f) return 0f;
            double steps = Math.Floor(days / HorizonStep + 0.5d);
            return steps <= 0d ? 0f : Finite((float)(steps * HorizonStep), days);
        }

        public static float UpToTheQuarterDay(float days)
        {
            days = Finite(days, 0f);
            if (days <= 0f) return 0f;
            double steps = Math.Ceiling(days / HorizonStep);
            return steps <= 0d ? HorizonStep : Finite((float)(steps * HorizonStep), days);
        }

        public static float HoursOf(float days) =>
            days <= 0f ? 0f : Finite(days * 24f, 0f);

        public const float TickTimeUnknown = 0.5f;

        public const double DaysARunTellsTheTickFor = 2d;

        public static float NextDailyTickIn(double daysSinceTheLastRun)
        {
            if (double.IsNaN(daysSinceTheLastRun) || daysSinceTheLastRun < 0d ||
                daysSinceTheLastRun > DaysARunTellsTheTickFor) return TickTimeUnknown;
            double gone = daysSinceTheLastRun - Math.Floor(daysSinceTheLastRun);
            float next = (float)(1d - gone);
            return next > 0f && next <= 1f ? next : 1f;
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
            counted <= 0 ? 0f : Finite(total / counted, 0f);

        public const float MostOfAMoveThatCounts = 2f;

        public const float LeastOfAMoveThatCounts = -1f;

        public static bool HowMuchCameTrue(int said, int moved, out float share)
        {
            share = 0f;
            if (said == 0) return false;
            double came = (double)moved / said;
            share = came > MostOfAMoveThatCounts ? MostOfAMoveThatCounts
                  : came < LeastOfAMoveThatCounts ? LeastOfAMoveThatCounts
                  : (float)came;
            return true;
        }

        public static float ShareThatCameTrue(float weighed, float cameTrue)
        {
            if (!(weighed > 0f) || float.IsNaN(cameTrue) || float.IsInfinity(cameTrue)) return NoShareToGive;
            float share = Finite(cameTrue / weighed, 0f);
            return share < 0f ? 0f : share;
        }

        public const int EnoughForecasts = 5;

        public static float TrustInTheForecast(int scored, float cameTrue)
        {
            if (scored <= 0 || cameTrue < 0f || float.IsNaN(cameTrue) || float.IsInfinity(cameTrue)) return 1f;
            float earned = cameTrue > 1f ? 1f : cameTrue;
            float weight = (float)scored / (scored + EnoughForecasts);
            float trust = 1f - (1f - earned) * weight;
            if (trust < 0f) return 0f;
            return trust > 1f ? 1f : trust;
        }

        public const float MostASaleCounts = 2f;

        public static long FetchedUpTo(int fetched, long meant)
        {
            if (fetched <= 0 || meant <= 0L) return 0L;
            double most = meant * (double)MostASaleCounts;
            return fetched > most ? (long)most : fetched;
        }

        public static float ShareFetched(long meant, long fetched)
        {
            if (meant <= 0L || fetched < 0L) return NoShareToGive;
            return Finite((float)((double)fetched / meant), NoShareToGive);
        }

        public const int MostResalesKept = 2000;

        public static void ForgetHalfWhenFull(ref int judged, ref long meant, ref long fetched)
        {
            if (judged <= MostResalesKept) return;
            judged /= 2;
            meant /= 2;
            fetched /= 2;
        }

        public const int EnoughResales = 250;

        public const float LeastResaleSafety = 0.5f;

        public const float MostResaleSafety = 1f;

        public static float ResaleSafetyAsSalesWent(float setting, int judged, float fetched)
        {
            float start = Finite(setting, LeastResaleSafety);
            start = start < LeastResaleSafety ? LeastResaleSafety : start > MostResaleSafety ? MostResaleSafety : start;
            if (judged <= 0 || fetched == NoShareToGive || float.IsNaN(fetched) || float.IsInfinity(fetched)) return start;
            float learned = fetched < LeastResaleSafety ? LeastResaleSafety
                          : fetched > MostResaleSafety ? MostResaleSafety : fetched;
            float weight = (float)judged / ((float)judged + EnoughResales);
            return start + (learned - start) * weight;
        }

        public static int WorthShiftTrusted(int shift, float trust)
        {
            if (shift == 0 || float.IsNaN(trust)) return shift;
            if (trust >= 1f) return shift;
            if (trust <= 0f) return 0;
            long held = (long)((double)shift * trust);
            if (held > int.MaxValue) return int.MaxValue;
            return held < int.MinValue ? int.MinValue : (int)held;
        }

        public static void AddPromise(PromiseRecord rec, float held)
        {
            if (rec == null || held < 0f || float.IsNaN(held) || float.IsInfinity(held)) return;
            rec.Held += held;
            rec.Scored++;
        }

        public static float PromiseMean(PromiseRecord rec) =>
            rec == null || rec.Scored <= 0 ? NoShareToGive : MeanOf(rec.Held, rec.Scored);

        public static float HeldShare(int promised, int found) =>
            promised <= 0 ? NoShareToGive : (found < 0 ? 0f : (float)found / promised);

        public static float UpToThePromise(float held) => held > 1f ? 1f : held;

        public const float GraceDays = 1f;

        public static bool WorthScoring(float saidWithinDays, float daysSince) =>
            daysSince <= (saidWithinDays < 0f ? 0f : saidWithinDays) * 2f + GraceDays;

        public const float SoonestAForecastIsJudged = 0.5f;

        public static bool TooSoonToJudge(float saidWithinDays, float daysSince) =>
            daysSince < (saidWithinDays > 0f ? saidWithinDays : 0f) * SoonestAForecastIsJudged;

        public static int YourOwnWorth(int unitsIn, int unitValue)
        {
            if (unitsIn == 0 || unitValue <= 0) return 0;
            long worth = (long)unitsIn * unitValue;
            if (worth > int.MaxValue) return int.MaxValue;
            return worth < int.MinValue ? int.MinValue : (int)worth;
        }

        public static int AddedUp(int kept, int more)
        {
            long sum = (long)kept + more;
            if (sum > int.MaxValue) return int.MaxValue;
            return sum < int.MinValue ? int.MinValue : (int)sum;
        }

        public static int WithoutYours(int moved, int yours) => MissedBy(yours, moved);

        public const int Bands = 4;

        public static int BandOf(float confidence)
        {
            if (confidence < 0.25f) return 0;
            if (confidence < 0.5f) return 1;
            return confidence < 0.75f ? 2 : 3;
        }

        public static float DaysSince(float thenHours, float nowHours)
        {
            float days = Finite((nowHours - thenHours) / 24f, 0f);
            return days < 0f ? 0f : days;
        }

        public static float EtaDays(float distance, float speed)
        {
            if (distance <= 0f) return 0f;
            float pace = Finite(speed, WalkingPace);
            if (pace <= StandingStill) pace = WalkingPace;
            return Finite(distance / (pace * 24f), FurthestThereIs);
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
