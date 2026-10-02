using System;
using System.Collections.Generic;

namespace TradeLord
{
    public static class TradeMath
    {
        public static bool PolicyAllows(int policy, bool buying) =>
            buying ? policy == Options.PolicyBuyOnly || policy == Options.PolicyBuySell
                   : policy == Options.PolicySellOnly || policy == Options.PolicyBuySell;

        public static int MadeOnAUnit(int price, bool bought, int unitCost)
        {
            if (!bought) return 0;
            long made = (long)price - (unitCost > 0 ? unitCost : 0);
            return made > int.MaxValue ? int.MaxValue : made < int.MinValue ? int.MinValue : (int)made;
        }

        public static int MadeOnAHandSale(PurchaseRecord rec, int units, long gold, int covers, List<int> laidOut)
        {
            if (rec == null || rec.Count <= 0 || units <= 0 || gold <= 0L) return 0;
            int bought = Math.Min(units, rec.Count);
            List<int> dear = covers >= 0 ? WhatAHandSaleTook(rec, units, gold, covers, laidOut) : null;
            long cost = TakeFromTheBatches(CopyOfTheBatches(rec), bought, dear);
            long made = gold * bought / units - cost;
            return made > int.MaxValue ? int.MaxValue : made < int.MinValue ? int.MinValue : (int)made;
        }

        public static bool ProfitAcceptable(int costBasis, int townSellPrice, float margin) =>
            costBasis > 0
                ? townSellPrice >= costBasis * (1f + margin)
                : townSellPrice > 0;

        public static int LeastThatClears(int costBasis, float margin)
        {
            if (costBasis <= 0) return 1;
            double least = Math.Ceiling(costBasis * (1f + margin));
            if (double.IsNaN(least) || least <= 1d) return 1;
            return least >= int.MaxValue ? int.MaxValue : (int)least;
        }

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

        public static void AddPurchase(PurchaseRecord rec, int count, int totalPaid, long first, float day)
        {
            if (rec == null || count <= 0) return;
            List<Batch> batches = BatchesOf(rec);
            rec.TotalPaid += totalPaid;
            rec.Count += count;
            rec.LastUnitPaid = (int)Math.Round((double)totalPaid / count);
            int unit = rec.LastUnitPaid > 0 ? rec.LastUnitPaid : 0;
            for (int i = 0; i < count; i++)
                AddBatch(batches, new Batch
                {
                    Unit = unit, Count = 1, First = first > 0L ? first + i : 0L, Day = day > 0f ? day : 0f
                });
        }

        public const long FirstUnitNumber = 1L;

        public const int EachUnitApart = 0;

        public const int MostUnitsKeptApart = 100000;

        public static readonly Comparison<Batch> CheapestFirstOldestLast = (x, y) =>
            x.Unit != y.Unit ? x.Unit.CompareTo(y.Unit) : y.First.CompareTo(x.First);

        private static readonly IComparer<Batch> InTheirPlace = Comparer<Batch>.Create(CheapestFirstOldestLast);

        public static long NumberASaleTakes(PurchaseRecord rec, int unitPaid)
        {
            if (rec == null || rec.Count <= 0) return 0L;
            List<Batch> batches = BatchesOf(rec);
            if (batches.Count == 0) return 0L;
            int at = unitPaid > 0 ? batches.FindLastIndex(one => one.Unit == unitPaid) : -1;
            Batch taken = batches[at < 0 ? 0 : at];
            return taken.First > 0L ? taken.First + taken.Count - 1 : 0L;
        }

        public static float DayASaleTakes(PurchaseRecord rec, int unitPaid)
        {
            if (rec == null || rec.Count <= 0) return 0f;
            List<Batch> batches = BatchesOf(rec);
            if (batches.Count == 0) return 0f;
            int at = unitPaid > 0 ? batches.FindLastIndex(one => one.Unit == unitPaid) : -1;
            return batches[at < 0 ? 0 : at].Day;
        }

        public static long KeepEveryUnitApart(List<PurchaseRecord> purchases)
        {
            long trimmed = 0L;
            if (purchases == null) return trimmed;
            for (int r = 0; r < purchases.Count; r++)
            {
                PurchaseRecord rec = purchases[r];
                if (rec == null || rec.Count <= 0) continue;
                if (rec.Count > MostUnitsKeptApart)
                {
                    trimmed += rec.Count - MostUnitsKeptApart;
                    DrainWhatLeftUnsold(rec, rec.Count - MostUnitsKeptApart);
                }
                List<Batch> batches = BatchesOf(rec);
                bool split = false;
                for (int b = 0; b < batches.Count && !split; b++) split = batches[b].Count > 1;
                if (!split) continue;
                var apart = new List<Batch>(rec.Count);
                for (int b = 0; b < batches.Count; b++)
                {
                    Batch one = batches[b];
                    for (int u = 0; u < one.Count; u++)
                        apart.Add(new Batch
                        {
                            Unit = one.Unit, Count = 1, First = one.First > 0L ? one.First + u : 0L, Day = one.Day
                        });
                }
                apart.Sort(CheapestFirstOldestLast);
                rec.Batches = apart;
            }
            return trimmed;
        }

        public static int DateEveryUnit(List<PurchaseRecord> purchases, float today)
        {
            int dated = 0;
            if (purchases == null || today <= 0f) return dated;
            for (int r = 0; r < purchases.Count; r++)
            {
                PurchaseRecord rec = purchases[r];
                if (rec?.Batches == null || rec.Count <= 0) continue;
                for (int b = 0; b < rec.Batches.Count; b++)
                {
                    Batch one = rec.Batches[b];
                    if (one.Day > 0f) continue;
                    one.Day = today;
                    rec.Batches[b] = one;
                    dated += one.Count;
                }
            }
            return dated;
        }

        public static long NumberEveryUnit(List<PurchaseRecord> purchases, long next, out long numbered)
        {
            numbered = 0L;
            long free = next < FirstUnitNumber ? FirstUnitNumber : next;
            if (purchases == null) return free;
            var ranges = new List<(int rec, int batch, long first, long last)>();
            for (int r = 0; r < purchases.Count; r++)
            {
                PurchaseRecord rec = purchases[r];
                if (rec == null || rec.Count <= 0) continue;
                List<Batch> batches = BatchesOf(rec);
                for (int b = 0; b < batches.Count; b++)
                {
                    Batch one = batches[b];
                    if (one.First < FirstUnitNumber || one.First > long.MaxValue - one.Count) continue;
                    ranges.Add((r, b, one.First, one.First + one.Count - 1));
                }
            }
            ranges.Sort((x, y) => x.first.CompareTo(y.first));
            var twice = new HashSet<(int rec, int batch)>();
            long highest = 0L;
            for (int i = 0; i < ranges.Count; i++)
            {
                if (ranges[i].first <= highest) { twice.Add((ranges[i].rec, ranges[i].batch)); continue; }
                highest = ranges[i].last;
            }
            if (highest >= free) free = highest + 1;
            for (int r = 0; r < purchases.Count; r++)
            {
                PurchaseRecord rec = purchases[r];
                if (rec == null || rec.Count <= 0) continue;
                List<Batch> batches = rec.Batches;
                bool moved = false;
                for (int b = 0; b < batches.Count; b++)
                {
                    Batch one = batches[b];
                    if (one.First >= FirstUnitNumber && one.First <= long.MaxValue - one.Count &&
                        !twice.Contains((r, b)))
                        continue;
                    one.First = free;
                    free += one.Count;
                    numbered += one.Count;
                    batches[b] = one;
                    moved = true;
                }
                if (moved) batches.Sort(CheapestFirstOldestLast);
            }
            return free;
        }

        public static void DrainSale(PurchaseRecord rec, int count) => DrainSale(rec, count, null);

        public static void DrainSale(PurchaseRecord rec, int count, int unitPaid) =>
            DrainSale(rec, count, unitPaid > 0 ? new List<int> { unitPaid } : null);

        public static void DrainSale(PurchaseRecord rec, int count, List<int> named)
        {
            if (rec == null || rec.Count <= 0 || count <= 0) return;
            int drain = Math.Min(count, rec.Count);
            TakeFromTheBatches(BatchesOf(rec), drain, named);
            LeaveTheAverage(rec, drain);
        }

        public static void DrainWhatLeftUnsold(PurchaseRecord rec, int count)
        {
            if (rec == null || rec.Count <= 0 || count <= 0) return;
            int drain = Math.Min(count, rec.Count);
            TakeInProportion(BatchesOf(rec), drain);
            LeaveTheAverage(rec, drain);
        }

        private static void LeaveTheAverage(PurchaseRecord rec, int drain)
        {
            int left = rec.Count - drain;
            if (left <= 0) { rec.Count = 0; rec.TotalPaid = 0; return; }
            int unit = (int)Math.Round((double)rec.TotalPaid / rec.Count);
            rec.Count = left;
            rec.TotalPaid = unit > 0 ? (int)Math.Min((long)unit * left, int.MaxValue) : 0;
        }

        public static List<int> WhatAHandSaleTook(PurchaseRecord rec, int units, long gold, int covers,
                                                  List<int> laidOut)
        {
            var took = new List<int>();
            if (rec == null || rec.Count <= 0 || units <= 0 || covers < 0) return took;
            List<Batch> batches = BatchesOf(rec);
            long each = Math.Max(0L, (long)Math.Round((double)rec.TotalPaid / rec.Count));
            var dear = new List<Batch>();
            for (int b = 0; b < batches.Count; b++)
                if (batches[b].Unit > covers) dear.Add(batches[b]);
            long left = gold;
            for (int i = 0; laidOut != null && i < laidOut.Count; i++)
            {
                int at = dear.FindLastIndex(one => one.Unit == laidOut[i]);
                if (at < 0) continue;
                TakeOne(dear, at);
                if (took.Count < units && left >= laidOut[i] + (units - took.Count - 1) * each)
                {
                    took.Add(laidOut[i]);
                    left -= laidOut[i];
                }
            }
            for (int b = dear.Count - 1; b >= 0; b--)
                for (int u = 0; u < dear[b].Count && took.Count < units; u++)
                {
                    if (left < dear[b].Unit + (units - took.Count - 1) * each) break;
                    took.Add(dear[b].Unit);
                    left -= dear[b].Unit;
                }
            return took;
        }

        private static long TakeFromTheBatches(List<Batch> batches, int units, List<int> named)
        {
            long cost = 0L;
            for (int i = 0; named != null && i < named.Count && units > 0; i++)
            {
                int at = batches.FindLastIndex(one => one.Unit == named[i]);
                if (at < 0) continue;
                cost += batches[at].Unit;
                TakeOne(batches, at);
                units--;
            }
            while (units > 0 && batches.Count > 0)
            {
                Batch cheapest = batches[0];
                int off = Math.Min(units, cheapest.Count);
                cost += (long)off * cheapest.Unit;
                units -= off;
                cheapest.Count -= off;
                if (cheapest.Count <= 0) batches.RemoveAt(0); else batches[0] = cheapest;
            }
            return cost;
        }

        private static List<Batch> CopyOfTheBatches(PurchaseRecord rec)
        {
            if (BatchesAddUp(rec))
            {
                var copy = new List<Batch>(rec.Batches);
                copy.Sort(CheapestFirstOldestLast);
                return copy;
            }
            var one = new List<Batch>();
            int unit = UnitBasis(rec, 0);
            if (rec.Count > 0) one.Add(new Batch { Unit = unit > 0 ? unit : 0, Count = rec.Count });
            return one;
        }

        private static void TakeInProportion(List<Batch> batches, int units)
        {
            long all = 0L;
            for (int i = 0; i < batches.Count; i++) all += batches[i].Count;
            if (units <= 0 || all <= 0L) return;
            if (units >= all)
            {
                batches.Clear();
                return;
            }
            var prices = new List<(int start, int end, long count)>();
            for (int i = 0; i < batches.Count; i++)
            {
                int last = prices.Count - 1;
                if (last >= 0 && batches[prices[last].start].Unit == batches[i].Unit)
                    prices[last] = (prices[last].start, i, prices[last].count + batches[i].Count);
                else prices.Add((i, i, batches[i].Count));
            }
            var off = new long[prices.Count];
            var over = new long[prices.Count];
            long taken = 0L;
            for (int p = 0; p < prices.Count; p++)
            {
                long share = prices[p].count * units;
                off[p] = share / all;
                over[p] = share % all;
                taken += off[p];
            }
            for (long left = units - taken; left > 0L; left--)
            {
                int most = -1;
                for (int p = 0; p < prices.Count; p++)
                    if (off[p] < prices[p].count && (most < 0 || over[p] > over[most])) most = p;
                if (most < 0) break;
                off[most]++;
                over[most] = -1L;
            }
            for (int p = 0; p < prices.Count; p++)
                for (int i = prices[p].start; i <= prices[p].end && off[p] > 0L; i++)
                {
                    Batch one = batches[i];
                    int gone = (int)Math.Min(off[p], one.Count);
                    one.Count -= gone;
                    off[p] -= gone;
                    batches[i] = one;
                }
            batches.RemoveAll(one => one.Count <= 0);
        }

        private static void TakeOne(List<Batch> batches, int at)
        {
            Batch taken = batches[at];
            taken.Count--;
            if (taken.Count <= 0) batches.RemoveAt(at); else batches[at] = taken;
        }

        public static bool BatchesAddUp(PurchaseRecord rec)
        {
            if (rec?.Batches == null) return false;
            long units = 0L;
            for (int i = 0; i < rec.Batches.Count; i++)
            {
                Batch one = rec.Batches[i];
                if (one.Count <= 0 || one.Unit < 0) return false;
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
            int at = batches.BinarySearch(added, InTheirPlace);
            batches.Insert(at < 0 ? ~at : at, added);
        }

        public static int LeaveOut(ref Batch[] costs, List<int> drawn)
        {
            if (costs == null || costs.Length == 0 || drawn == null || drawn.Count == 0) return 0;
            var left = new List<Batch>(costs);
            int taken = 0;
            for (int i = 0; i < drawn.Count; i++)
            {
                int at = left.FindLastIndex(one => one.Unit == drawn[i]);
                if (at < 0) continue;
                TakeOne(left, at);
                taken++;
            }
            costs = left.Count == 0 ? null : left.ToArray();
            return taken;
        }

        public static int WhatTheAverageCovers(int basis, float margin)
        {
            if (basis <= 0) return basis;
            float over = Finite(margin, 0f);
            double covered = Math.Floor(basis * (1d + (over > 0f ? over : 0f)));
            return covered >= int.MaxValue ? int.MaxValue : (int)covered;
        }

        public static Batch[] UnitCosts(PurchaseRecord rec, int held)
        {
            if (rec == null || rec.Count <= 0 || held <= 0) return null;
            List<Batch> kept = CopyOfTheBatches(rec);
            if (rec.Count > held) TakeInProportion(kept, rec.Count - held);
            return kept.Count == 0 ? null : kept.ToArray();
        }

        public static int UnitsIn(Batch[] costs)
        {
            long units = 0L;
            for (int i = 0; costs != null && i < costs.Length; i++) units += costs[i].Count;
            return units > int.MaxValue ? int.MaxValue : (int)units;
        }

        public static void DropTheCheapest(ref Batch[] costs, int units)
        {
            if (costs == null || units <= 0) return;
            var left = new List<Batch>(costs);
            TakeFromTheBatches(left, units, null);
            costs = left.Count == 0 ? null : left.ToArray();
        }

        public const int NoUnitThisPriceSells = int.MaxValue;

        public struct DearFirst
        {
            private readonly Batch[] _costs;
            private readonly int _worth;
            private readonly int _split;
            private readonly bool _ownCost;
            private readonly float _margin;
            private readonly float _today;
            private readonly int _agedAfter;
            private readonly int _aged;
            private int _agedTaken;
            private int _only;
            private bool _pickedAged;
            private int _low;
            private int _lowTaken;
            private int _dearLow;
            private int _dearLowTaken;
            private int _top;
            private int _topTaken;
            private int _unknown;
            private int _picked;
            private int _pickedAt;

            private const int NotPicked = 0;
            private const int TheDearest = 1;
            private const int TheLeastDear = 2;

            private const int CoversNoUnit = -1;

            private const int EveryUnit = 0;
            private const int OnlyTheAged = 1;
            private const int NoneOfTheAged = 2;

            public DearFirst(Batch[] costs, int covers, int worth, int unknown = 0)
                : this(costs, covers, worth, unknown, false, 0f, 0f, 0)
            {
            }

            public static DearFirst EachAtItsOwnCost(Batch[] costs, int worth, int unknown, float margin,
                                                     float today = 0f, int agedAfter = 0) =>
                new DearFirst(costs, CoversNoUnit, worth, unknown, true, margin, today, agedAfter);

            private DearFirst(Batch[] costs, int covers, int worth, int unknown, bool ownCost, float margin,
                              float today, int agedAfter)
            {
                _costs = costs;
                _worth = worth;
                _ownCost = ownCost;
                _margin = margin;
                _today = today;
                _agedAfter = ownCost && agedAfter > 0 && today > 0f ? agedAfter : 0;
                _agedTaken = 0;
                _only = EveryUnit;
                _pickedAged = false;
                int aged = 0;
                for (int at = 0; _agedAfter > 0 && costs != null && at < costs.Length; at++)
                    if (HasAged(costs[at], today, _agedAfter)) aged += costs[at].Count;
                _aged = aged;
                int many = costs == null ? 0 : costs.Length;
                int split = 0;
                while (split < many && costs[split].Unit <= covers) split++;
                _split = split;
                _low = 0;
                _lowTaken = 0;
                _dearLow = split;
                _dearLowTaken = 0;
                _top = many - 1;
                _topTaken = 0;
                _unknown = unknown > 0 ? unknown : 0;
                _picked = NotPicked;
                _pickedAt = -1;
            }

            public int Floor(int price)
            {
                if (_ownCost) return TheDearestThatClears(price);
                _picked = _top < _dearLow ? NotPicked
                        : price >= _costs[_top].Unit ? TheDearest
                        : price >= _costs[_dearLow].Unit ? TheLeastDear
                        : NotPicked;
                return _top >= _dearLow || _low < _split || _unknown > 0 ? _worth : NoUnitThisPriceSells;
            }

            private static bool HasAged(Batch unit, float today, int agedAfter) =>
                agedAfter > 0 && unit.Day > 0f && today - unit.Day >= agedAfter;

            private bool AgedAt(int at) => HasAged(_costs[at], _today, _agedAfter);

            public bool PickedHasAged => _picked == TheDearest && _pickedAged;

            public int AgedLeft => _aged - _agedTaken > 0 ? _aged - _agedTaken : 0;

            public bool KeepOnlyTheAged()
            {
                if (AgedLeft <= 0) return false;
                _only = OnlyTheAged;
                return true;
            }

            public bool HoldingAllButTheAged => _only == OnlyTheAged;

            public DearFirst WithoutTheAged()
            {
                DearFirst fresh = this;
                fresh._only = NoneOfTheAged;
                fresh._unknown = 0;
                return fresh;
            }

            public bool Clears(int price, float margin)
            {
                int floor = Floor(price);
                if (floor == NoUnitThisPriceSells) return false;
                return PickedHasAged ? price >= floor : ProfitAcceptable(floor, price, margin);
            }

            public int LeastItAsks()
            {
                for (int at = _dearLow; at <= _top; at++)
                {
                    if (LeftIn(at) <= 0) continue;
                    bool aged = _ownCost && AgedAt(at);
                    if (_only == OnlyTheAged && !aged || _only == NoneOfTheAged && aged) continue;
                    return _costs[at].Unit;
                }
                return _unknown > 0 && _only == EveryUnit ? _worth : NoUnitThisPriceSells;
            }

            private int TheDearestThatClears(int price)
            {
                _picked = NotPicked;
                _pickedAged = false;
                for (int at = _top; at >= _dearLow; at--)
                {
                    if (LeftIn(at) <= 0) continue;
                    bool aged = AgedAt(at);
                    if (_only == OnlyTheAged && !aged || _only == NoneOfTheAged && aged) continue;
                    if (aged ? price < _costs[at].Unit : !ProfitAcceptable(_costs[at].Unit, price, _margin)) continue;
                    _picked = TheDearest;
                    _pickedAt = at;
                    _pickedAged = aged;
                    return _costs[at].Unit;
                }
                return _unknown > 0 && _only == EveryUnit ? _worth : NoUnitThisPriceSells;
            }

            public int Took()
            {
                int picked = _top >= _dearLow ? _picked : NotPicked;
                _picked = NotPicked;
                if (picked == TheDearest && _ownCost && _pickedAt >= _dearLow && _pickedAt < _top)
                {
                    _top = _pickedAt;
                    _topTaken = 0;
                }
                if (picked == TheDearest)
                {
                    int dearest = _costs[_top].Unit;
                    if (_agedAfter > 0 && AgedAt(_top)) _agedTaken++;
                    _topTaken++;
                    if (LeftIn(_top) <= 0) DropTheTop();
                    return dearest;
                }
                if (picked == NotPicked && _low < _split)
                {
                    int cheapest = _costs[_low].Unit;
                    if (++_lowTaken >= _costs[_low].Count)
                    {
                        _low++;
                        _lowTaken = 0;
                    }
                    return cheapest;
                }
                if (picked == NotPicked && _unknown > 0)
                {
                    _unknown--;
                    return _worth;
                }
                if (_top < _dearLow) return _worth;
                int lowest = _costs[_dearLow].Unit;
                if (_agedAfter > 0 && AgedAt(_dearLow)) _agedTaken++;
                _dearLowTaken++;
                if (LeftIn(_dearLow) <= 0)
                {
                    _dearLow++;
                    _dearLowTaken = 0;
                }
                return lowest;
            }

            private int LeftIn(int at) =>
                _costs[at].Count - (at == _top ? _topTaken : 0) - (at == _dearLow ? _dearLowTaken : 0);

            private void DropTheTop()
            {
                _top--;
                _topTaken = 0;
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

        public const int LearningLimitWarnsThisClose = 1;

        public static bool NearTheLearningLimit(int skill, int limit) => skill >= limit - LearningLimitWarnsThisClose;

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

        public const float MostOfAMoveThatCounts = 1f;

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

        public static long SizeOf(int said) => said < 0 ? -(long)said : said;

        public static long SizedShareThatCameTrue(int said, float share) =>
            (long)Math.Round(SizeOf(said) * (double)share);

        public static float ShareBySize(long weighed, long matched)
        {
            if (weighed <= 0L) return NoShareToGive;
            double share = (double)matched / weighed;
            return share <= 0d ? 0f : share >= 1d ? 1f : (float)share;
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

        public const int EnoughWalkIns = 25;

        public const float LeastResaleSafety = 0.5f;

        public const float MostResaleSafety = 1f;

        public static float ResaleSafetyAsPromisesHeld(float setting, bool learn, int walkIns, float held)
        {
            float start = Finite(setting, LeastResaleSafety);
            start = start < LeastResaleSafety ? LeastResaleSafety : start > MostResaleSafety ? MostResaleSafety : start;
            if (!learn || walkIns <= 0 || held == NoShareToGive || float.IsNaN(held) || float.IsInfinity(held))
                return start;
            float learned = held < LeastResaleSafety ? LeastResaleSafety
                          : held > start ? start : held;
            float weight = (float)walkIns / ((float)walkIns + EnoughWalkIns);
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

        public static long AddedUp(long kept, long more)
        {
            if (more > 0L && kept > long.MaxValue - more) return long.MaxValue;
            if (more < 0L && kept < long.MinValue - more) return long.MinValue;
            return kept + more;
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
