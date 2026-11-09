using System;
using System.Collections.Generic;
using System.Globalization;

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

        public static int MadeOnAHandSale(PurchaseRecord rec, IList<int> prices, int covers, IList<int> laidOut)
        {
            if (rec == null || rec.Count <= 0 || prices == null || prices.Count == 0) return 0;
            long made = 0L;
            foreach (SoldUnit one in WhatAHandSaleTook(rec, prices, covers, laidOut))
                if (one.Bought) made += (long)one.Price - one.Cost;
            return made > int.MaxValue ? int.MaxValue : made < int.MinValue ? int.MinValue : (int)made;
        }

        public struct SoldUnit
        {
            public int Price;
            public bool Bought;
            public int Cost;
            public long Number;
            public float Day;
            public int From;
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

        public static long WhatALadderPays(Func<int, int> rungAt, int units, int purse)
        {
            if (rungAt == null || units <= 0) return 0L;
            long paid = 0L;
            for (int u = 0; u < units; u++)
            {
                int price = rungAt(u);
                if (price <= 0 || paid + price > purse) break;
                paid += price;
            }
            return paid;
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

        public const float HungryFoodTolerance = 2f;

        public const float FoodValueTolerance = 2f;

        public static int AtLeastTwiceItsValue(int ceiling, int value) =>
            Math.Max(ceiling, MostToPayOverTheCheapest(value, FoodValueTolerance));

        public const float ShortFoodTolerance = 1.5f;

        public static int FoodShortOfADay(int held, int day) =>
            day <= 0 || held >= day ? 0 : day - (held < 0 ? 0 : held);

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

        public const int FromAMarket = 0;

        public const int FromACaravan = 1;

        public const int FromVillagers = 2;

        public const int FromElsewhere = 3;

        public const int FromLoot = 4;

        public static bool IsASource(int from) => from >= FromAMarket && from <= FromLoot;

        public static bool GivesTradeXp(int from) => from == FromAMarket || from == FromACaravan;

        public static bool CameFree(int from) => from == FromElsewhere || from == FromLoot;

        public static int SourceNumber(int from) =>
            from == FromACaravan ? 2
            : from == FromVillagers ? 3
            : from == FromLoot ? 4
            : from == FromElsewhere ? 5
            : 1;

        public static string SourceNamed(int from) =>
            from == FromACaravan ? "caravan"
            : from == FromVillagers ? "villager party"
            : from == FromLoot ? "loot"
            : from == FromElsewhere ? "others"
            : "market";

        public static string WhereFrom(int from) =>
            "source " + SourceNumber(from) + " (" + SourceNamed(from) + "), " +
            (CameFree(from) ? "came without a purchase" : "bought");

        public static string WhereFreeFrom(int from) =>
            from == FromVillagers
                ? "source " + SourceNumber(from) + " (" + SourceNamed(from) + "), came with a villagers' offer at no cost"
                : WhereFrom(from);

        public static void AddPurchase(PurchaseRecord rec, IList<int> paid, long first, float day,
                                       int from = FromAMarket, bool traded = false)
        {
            if (rec == null || paid == null || paid.Count == 0) return;
            List<Batch> batches = BatchesOf(rec);
            int source = IsASource(from) ? from : FromAMarket;
            for (int i = 0; i < paid.Count; i++)
                AddBatch(batches, new Batch
                {
                    Unit = paid[i] > 0 ? paid[i] : 0, Count = 1, First = first > 0L ? first + i : 0L, Day = day > 0f ? day : 0f,
                    From = source, Traded = traded
                });
            rec.LastUnitPaid = paid[paid.Count - 1] > 0 ? paid[paid.Count - 1] : 0;
            AddUpTheRows(rec);
        }

        public static int UnitsBoughtToTrade(PurchaseRecord rec)
        {
            if (rec == null || rec.Count <= 0 || !BatchesAddUp(rec)) return 0;
            int units = 0;
            foreach (Batch one in rec.Batches)
                if (one.Traded && one.Count > 0) units += one.Count;
            return units;
        }

        public static int MarkBoughtToTrade(PurchaseRecord rec, long first, int units)
        {
            if (rec == null || rec.Batches == null || units <= 0 || first <= 0L) return 0;
            int marked = 0;
            for (int i = 0; i < rec.Batches.Count; i++)
            {
                Batch one = rec.Batches[i];
                if (one.Traded || one.Count <= 0 || one.First < first || one.First + one.Count > first + units) continue;
                one.Traded = true;
                rec.Batches[i] = one;
                marked += one.Count;
            }
            return marked;
        }

        public static int[] AtOnePrice(int units, int price)
        {
            var each = new int[units > 0 ? units : 0];
            for (int i = 0; i < each.Length; i++) each[i] = price;
            return each;
        }

        public static int[] SpreadOver(int units, int gold)
        {
            var each = new int[units > 0 ? units : 0];
            if (each.Length == 0 || gold <= 0) return each;
            int unit = gold / each.Length;
            int over = gold - unit * each.Length;
            for (int i = 0; i < each.Length; i++) each[i] = i < over ? unit + 1 : unit;
            return each;
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
            Batch taken = batches[at < 0 ? TheOldestOfTheCheapest(batches) : at];
            return taken.First > 0L ? taken.First + taken.Count - 1 : 0L;
        }

        public static int FromASaleTakes(PurchaseRecord rec, int unitPaid)
        {
            if (rec == null || rec.Count <= 0) return FromAMarket;
            List<Batch> batches = BatchesOf(rec);
            if (batches.Count == 0) return FromAMarket;
            int at = unitPaid > 0 ? batches.FindLastIndex(one => one.Unit == unitPaid) : -1;
            return batches[at < 0 ? TheOldestOfTheCheapest(batches) : at].From;
        }

        public static float DayASaleTakes(PurchaseRecord rec, int unitPaid)
        {
            if (rec == null || rec.Count <= 0) return 0f;
            List<Batch> batches = BatchesOf(rec);
            if (batches.Count == 0) return 0f;
            int at = unitPaid > 0 ? batches.FindLastIndex(one => one.Unit == unitPaid) : -1;
            return batches[at < 0 ? TheOldestOfTheCheapest(batches) : at].Day;
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
                            Unit = one.Unit, Count = 1, First = one.First > 0L ? one.First + u : 0L, Day = one.Day,
                            From = one.From, Traded = one.Traded
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
            AddUpTheRows(rec);
        }

        public static List<SoldUnit> DrainHandSale(PurchaseRecord rec, IList<int> prices, int covers, IList<int> laidOut)
        {
            if (rec == null || rec.Count <= 0 || prices == null || prices.Count == 0) return new List<SoldUnit>();
            List<SoldUnit> sold = TakeAHandSale(BatchesOf(rec), prices, covers, laidOut);
            AddUpTheRows(rec);
            return sold;
        }

        public static void DrainWhatLeftUnsold(PurchaseRecord rec, int count)
        {
            if (rec == null || rec.Count <= 0 || count <= 0) return;
            TakeTheOldest(BatchesOf(rec), Math.Min(count, rec.Count));
            AddUpTheRows(rec);
        }

        public static int AddUpEveryRecord(List<PurchaseRecord> purchases)
        {
            int moved = 0;
            for (int r = 0; purchases != null && r < purchases.Count; r++)
            {
                PurchaseRecord rec = purchases[r];
                if (rec == null || rec.Count <= 0 || !BatchesAddUp(rec)) continue;
                int was = rec.TotalPaid;
                AddUpTheRows(rec);
                if (rec.TotalPaid != was) moved++;
            }
            return moved;
        }

        private static void AddUpTheRows(PurchaseRecord rec)
        {
            long units = 0L, paid = 0L;
            for (int i = 0; i < rec.Batches.Count; i++)
            {
                units += rec.Batches[i].Count;
                paid += (long)rec.Batches[i].Unit * rec.Batches[i].Count;
            }
            rec.Count = units > int.MaxValue ? int.MaxValue : (int)units;
            rec.TotalPaid = rec.Count <= 0 ? 0 : paid > int.MaxValue ? int.MaxValue : (int)paid;
        }

        public static List<SoldUnit> WhatAHandSaleTook(PurchaseRecord rec, IList<int> prices, int covers,
                                                       IList<int> laidOut) =>
            rec == null || rec.Count <= 0 || prices == null
                ? new List<SoldUnit>()
                : TakeAHandSale(CopyOfTheBatches(rec), prices, covers, laidOut);

        private static List<SoldUnit> TakeAHandSale(List<Batch> batches, IList<int> prices, int covers,
                                                    IList<int> laidOut)
        {
            var sold = new List<SoldUnit>(prices.Count);
            for (int i = 0; i < prices.Count; i++)
            {
                int price = prices[i];
                if (batches.Count == 0)
                {
                    sold.Add(new SoldUnit { Price = price });
                    continue;
                }
                int at = -1;
                if (laidOut != null && i < laidOut.Count)
                {
                    int named = laidOut[i];
                    at = batches.FindLastIndex(one => one.Unit == named);
                }
                if (at < 0 && covers >= 0)
                    at = batches.FindLastIndex(one => one.Unit > covers && one.Unit <= price);
                if (at < 0) at = TheOldestOfTheCheapest(batches);
                Batch taken = batches[at];
                sold.Add(new SoldUnit
                {
                    Price = price, Bought = true, Cost = taken.Unit, Day = taken.Day, From = taken.From,
                    Number = taken.First > 0L ? taken.First + taken.Count - 1 : 0L
                });
                TakeOne(batches, at);
            }
            return sold;
        }

        private static int TheOldestOfTheCheapest(List<Batch> batches)
        {
            int at = 0;
            while (at + 1 < batches.Count && batches[at + 1].Unit == batches[0].Unit) at++;
            return at;
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
                int at = TheOldestOfTheCheapest(batches);
                Batch cheapest = batches[at];
                int off = Math.Min(units, cheapest.Count);
                cost += (long)off * cheapest.Unit;
                units -= off;
                cheapest.Count -= off;
                if (cheapest.Count <= 0) batches.RemoveAt(at); else batches[at] = cheapest;
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

        public static readonly Comparison<Batch> OldestFirst = (x, y) =>
            x.Day != y.Day ? x.Day.CompareTo(y.Day) : x.First.CompareTo(y.First);

        public static bool DrainTheOldestUnit(PurchaseRecord rec, out long number, out float day, out int from)
        {
            number = 0L;
            day = 0f;
            from = FromElsewhere;
            if (rec == null || rec.Count <= 0) return false;
            List<Batch> batches = BatchesOf(rec);
            if (batches.Count == 0) return false;
            int at = 0;
            for (int i = 1; i < batches.Count; i++)
                if (OldestFirst(batches[i], batches[at]) < 0) at = i;
            number = batches[at].First;
            day = batches[at].Day;
            from = batches[at].From;
            TakeTheOldest(batches, 1);
            AddUpTheRows(rec);
            return true;
        }

        public static int DrainTheOldestOf(PurchaseRecord bought, PurchaseRecord free, int units)
        {
            if (units <= 0) return 0;
            var rows = new List<Batch>();
            var mine = new List<bool>();
            foreach (PurchaseRecord rec in new[] { bought, free })
            {
                if (rec == null || rec.Count <= 0) continue;
                List<Batch> batches = BatchesOf(rec);
                for (int i = 0; i < batches.Count; i++)
                {
                    rows.Add(batches[i]);
                    mine.Add(rec == bought);
                }
            }
            var order = new List<int>(rows.Count);
            for (int i = 0; i < rows.Count; i++) order.Add(i);
            order.Sort((x, y) =>
            {
                int older = OldestFirst(rows[x], rows[y]);
                return older != 0 ? older : x.CompareTo(y);
            });
            int fromBought = 0, fromFree = 0;
            for (int o = 0; o < order.Count && units > 0; o++)
            {
                int gone = Math.Min(units, rows[order[o]].Count);
                if (mine[order[o]]) fromBought += gone; else fromFree += gone;
                units -= gone;
            }
            if (bought != null && fromBought > 0) DrainWhatLeftUnsold(bought, fromBought);
            if (free != null && fromFree > 0) DrainWhatLeftUnsold(free, fromFree);
            return fromBought;
        }

        private static void TakeTheOldest(List<Batch> batches, int units)
        {
            if (units <= 0 || batches.Count == 0) return;
            var order = new List<int>(batches.Count);
            for (int i = 0; i < batches.Count; i++) order.Add(i);
            order.Sort((x, y) =>
            {
                int older = OldestFirst(batches[x], batches[y]);
                return older != 0 ? older : x.CompareTo(y);
            });
            for (int o = 0; o < order.Count && units > 0; o++)
            {
                Batch oldest = batches[order[o]];
                int gone = Math.Min(units, oldest.Count);
                oldest.Count -= gone;
                if (oldest.First > 0L) oldest.First += gone;
                units -= gone;
                batches[order[o]] = oldest;
            }
            batches.RemoveAll(one => one.Count <= 0);
        }

        private static void TakeYourOwnFirst(List<Batch> batches, int units)
        {
            if (units <= 0 || batches.Count == 0) return;
            List<Batch> mine = batches.FindAll(one => !one.Traded);
            List<Batch> traded = batches.FindAll(one => one.Traded);
            int own = 0;
            for (int i = 0; i < mine.Count; i++) own += mine[i].Count;
            int fromMine = Math.Min(units, own);
            TakeTheOldest(mine, fromMine);
            TakeTheOldest(traded, units - fromMine);
            batches.Clear();
            batches.AddRange(mine);
            batches.AddRange(traded);
            batches.Sort(CheapestFirstOldestLast);
        }

        public static void DrainYourOwnFirst(PurchaseRecord rec, int count)
        {
            if (rec == null || rec.Count <= 0 || count <= 0) return;
            TakeYourOwnFirst(BatchesOf(rec), Math.Min(count, rec.Count));
            AddUpTheRows(rec);
        }

        public static int DrainYourOwnFoodFirst(PurchaseRecord bought, PurchaseRecord free, int units)
        {
            if (units <= 0) return 0;
            var rows = new List<Batch>();
            var mine = new List<bool>();
            int traded = 0;
            foreach (PurchaseRecord rec in new[] { bought, free })
            {
                if (rec == null || rec.Count <= 0) continue;
                List<Batch> batches = BatchesOf(rec);
                for (int i = 0; i < batches.Count; i++)
                {
                    if (batches[i].Traded)
                    {
                        traded += batches[i].Count;
                        continue;
                    }
                    rows.Add(batches[i]);
                    mine.Add(rec == bought);
                }
            }
            var order = new List<int>(rows.Count);
            for (int i = 0; i < rows.Count; i++) order.Add(i);
            order.Sort((x, y) =>
            {
                int older = OldestFirst(rows[x], rows[y]);
                return older != 0 ? older : x.CompareTo(y);
            });
            int fromBought = 0, fromFree = 0;
            for (int o = 0; o < order.Count && units > 0; o++)
            {
                int gone = Math.Min(units, rows[order[o]].Count);
                if (mine[order[o]]) fromBought += gone; else fromFree += gone;
                units -= gone;
            }
            fromBought += Math.Min(units, traded);
            if (bought != null && fromBought > 0) DrainYourOwnFirst(bought, fromBought);
            if (free != null && fromFree > 0) DrainWhatLeftUnsold(free, fromFree);
            return fromBought;
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
            if (rec.Count > held) TakeTheOldest(kept, rec.Count - held);
            return kept.Count == 0 ? null : kept.ToArray();
        }

        public static Batch[] UnitCostsOfFood(PurchaseRecord rec, int held)
        {
            if (rec == null || rec.Count <= 0 || held <= 0) return null;
            List<Batch> kept = CopyOfTheBatches(rec);
            if (rec.Count > held) TakeYourOwnFirst(kept, rec.Count - held);
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
            private PassedOver _passedOver;
            private bool _tookTraded;

            private const int NotPicked = 0;
            private const int TheDearest = 1;

            private sealed class PassedOver
            {
                internal readonly int Low;
                internal readonly int High;
                internal readonly int TakenAtLow;
                internal readonly int TakenAtHigh;
                internal readonly PassedOver Next;

                internal PassedOver(int low, int high, int takenAtLow, int takenAtHigh, PassedOver next)
                {
                    Low = low;
                    High = high;
                    TakenAtLow = takenAtLow;
                    TakenAtHigh = takenAtHigh;
                    Next = next;
                }
            }

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
                _passedOver = null;
                _tookTraded = false;
            }

            public bool TookTraded => _tookTraded;

            public int Floor(int price)
            {
                if (_ownCost) return TheDearestThatClears(price);
                _picked = NotPicked;
                for (int at = _top; at >= _dearLow; at--)
                {
                    if (LeftIn(at) <= 0 || price < _costs[at].Unit) continue;
                    _picked = TheDearest;
                    _pickedAt = at;
                    break;
                }
                return _top >= _dearLow || _low < _split || _unknown > 0 || _passedOver != null
                    ? _worth : NoUnitThisPriceSells;
            }

            private static bool HasAged(Batch unit, float today, int agedAfter) =>
                agedAfter > 0 && unit.Day > 0f && today - unit.Day >= agedAfter;

            private bool AgedAt(int at) => HasAged(_costs[at], _today, _agedAfter);

            public bool PickedHasAged => _picked == TheDearest && _pickedAged;

            public int AgedLeft => _aged - _agedTaken > 0 ? _aged - _agedTaken : 0;

            public int Unknown => _unknown;

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

            public DearFirst WithoutTheUnknown()
            {
                DearFirst known = this;
                known._unknown = 0;
                return known;
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
                _tookTraded = false;
                if (picked == TheDearest && _pickedAt >= _dearLow && _pickedAt < _top)
                {
                    if (!_ownCost) _passedOver = new PassedOver(_pickedAt + 1, _top, 0, _topTaken, _passedOver);
                    _top = _pickedAt;
                    _topTaken = 0;
                }
                if (picked == TheDearest)
                {
                    int dearest = _costs[_top].Unit;
                    _tookTraded = _costs[_top].Traded;
                    if (_agedAfter > 0 && AgedAt(_top)) _agedTaken++;
                    _topTaken++;
                    if (LeftIn(_top) <= 0) DropTheTop();
                    return dearest;
                }
                if (picked == NotPicked && _low < _split)
                {
                    int cheapest = _costs[_low].Unit;
                    _tookTraded = _costs[_low].Traded;
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
                if (_top < _dearLow) return _passedOver != null ? TheCheapestPassedOver() : _worth;
                int lowest = _costs[_dearLow].Unit;
                _tookTraded = _costs[_dearLow].Traded;
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

            private int TheCheapestPassedOver()
            {
                PassedOver head = _passedOver;
                int at = head.Low;
                int taken = head.TakenAtLow + 1;
                int left = _costs[at].Count - taken - (at == head.High ? head.TakenAtHigh : 0);
                _passedOver = left > 0 ? new PassedOver(at, head.High, taken, head.TakenAtHigh, head.Next)
                            : at < head.High ? new PassedOver(at + 1, head.High, 0, head.TakenAtHigh, head.Next)
                            : head.Next;
                _tookTraded = _costs[at].Traded;
                return _costs[at].Unit;
            }

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

        public static int MostAForecastCanReach(int live) => (int)(live * (1f + MostAForecastMayMoveAPrice));

        public static bool NoOpeningPriceCouldMakeIt(int openingBuy, int quotedSell, int landed, int stocked, float days,
                                                     bool beaten, float bestKey, float safety, float margin,
                                                     float mostItCounts = 1f)
        {
            if (landed == 0 || quotedSell <= 0 || stocked <= 0 || !(safety >= 0f)) return false;
            int upper = MostAForecastCanReach(quotedSell);
            if (!BuyAcceptable(openingBuy, Realizable(upper, safety), margin)) return true;
            return beaten && upper >= openingBuy &&
                   PerDay((float)(upper - openingBuy) * stocked, days) * NoMoreThanWhole(mostItCounts) <= bestKey;
        }

        public static float NoMoreThanWhole(float share) => share > 0f && share < 1f ? share : 1f;

        public static bool LandingCannotLiftItPastTheMargin(int openingBuy, int quotedSell, int unlanded, int landed,
                                                            float safety, float margin)
        {
            if (landed <= 0 || quotedSell <= 0 || unlanded <= 0 || !(safety >= 0f)) return false;
            return !BuyAcceptable(openingBuy, Realizable(ForecastWithin(quotedSell, unlanded), safety), margin);
        }

        public static bool NoPriceCouldClearTheMargin(int openingBuy, int quotedSell, float safety, float margin)
        {
            if (quotedSell <= 0 || !(safety >= 0f)) return false;
            return !BuyAcceptable(openingBuy, Realizable(MostAForecastCanReach(quotedSell), safety), margin);
        }

        public static bool NoRoadCouldMakeIt(int openingBuy, int quotedSell, int stocked, float soonest,
                                             bool beaten, float bestKey, float mostItCounts = 1f)
        {
            if (!beaten || quotedSell <= 0 || stocked <= 0 || !(soonest >= 0f)) return false;
            int upper = MostAForecastCanReach(quotedSell);
            return upper >= openingBuy &&
                   PerDay((float)(upper - openingBuy) * stocked, soonest) * NoMoreThanWhole(mostItCounts) <= bestKey;
        }

        public static int UnitsARouteMayCarry(int stocked, int fits) => fits > 0 && fits < stocked ? fits : stocked;

        public const float ARideIsNeverShorterThanItsRoadBy = 0.999f;

        public static float ShortestOnLand(IList<Run> runs, float length)
        {
            if (runs == null || !(length > 0f) || float.IsInfinity(length)) return 0f;
            double sum = 0d;
            for (int i = 0; i < runs.Count; i++)
            {
                float piece = runs[i].Length;
                if (float.IsNaN(piece) || float.IsInfinity(piece)) return 0f;
                if (!(piece > 0f)) continue;
                if (runs[i].Ground == GroundSea) return 0f;
                sum += piece;
            }
            float walked = (float)sum;
            return walked < length ? walked : length;
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

        public static float OneUnitRate(int sell, int paid, float days, bool conservative, float safety) =>
            sell <= 0 ? 0f : PerDay((conservative ? (int)Realizable(sell, safety) : sell) - paid, days);

        public const float TheMarkedTownHoldsBy = 1.2f;

        public static float RateTheMarkHolds(float rate, bool marked)
        {
            if (!marked || rate <= 0f || float.IsNaN(rate)) return rate;
            float held = rate * TheMarkedTownHoldsBy;
            return float.IsInfinity(held) ? rate : held;
        }

        public static bool EnoughOnTheShelf(int stocked, int unitWorth, int minUnits, int minWorth)
        {
            if (minUnits <= 0) return true;
            if (stocked >= minUnits) return true;
            if (minWorth <= 0 || stocked <= 0 || unitWorth <= 0) return false;
            return (long)stocked * unitWorth >= minWorth;
        }

        public const float ACaravanSellsAbove = 1.1f;

        public const int ACaravanFeelsLadenAtWorth = 5000;

        public const float ALadenCaravanSellsLowerBy = 0.4f;

        public const float ACaravanLeansOnTheAverageBy = 0.8f;

        public const int ACaravanAddsUnitValuesToTheBudget = 2;

        public const float ACaravanSellsThisEagerly = 3f;

        public const float AConvoySellsThisEagerly = 5f;

        public const float AHaulAnimalSellsThisEagerly = 1.5f;

        public const float ACaravanDrivesThisManyAnimalsAMan = 0.6f;

        public static float WhatHoldingAKindTakesOff(int heldWorth)
        {
            if (heldWorth <= 0) return 0f;
            double laden = Math.Min(heldWorth, ACaravanFeelsLadenAtWorth) / (double)ACaravanFeelsLadenAtWorth;
            return (float)Math.Sqrt(laden) * ALadenCaravanSellsLowerBy;
        }

        public static int WhatACaravanUnloads(int carried, float priceFactor, float averageIndex, float lowestIndex,
                                              int heldWorth, float dailyBudget, int unitValue, int unitPrice,
                                              bool convoy, bool haulAnimal)
        {
            if (carried <= 0 || unitPrice <= 0) return 0;
            float factor = Finite(priceFactor, 0f), average = Finite(averageIndex, 0f);
            if (factor <= 0f || average <= 0f) return 0;
            float limit = ACaravanSellsAbove - WhatHoldingAKindTakesOff(heldWorth);
            float againstTheAverage = factor / average;
            if (againstTheAverage < limit) return 0;
            float floor = (ACaravanLeansOnTheAverageBy * average +
                           (1f - ACaravanLeansOnTheAverageBy) * Finite(lowestIndex, average)) * limit;
            if (factor < floor) return 0;
            double units = (Finite(dailyBudget, 0f) + (double)ACaravanAddsUnitValuesToTheBudget * unitValue) *
                           (factor - floor) * againstTheAverage *
                           (haulAnimal ? AHaulAnimalSellsThisEagerly : 1f) *
                           (convoy ? AConvoySellsThisEagerly : ACaravanSellsThisEagerly) / unitPrice;
            if (!(units >= 0.5d)) return 0;
            return units >= carried ? carried : (int)Math.Round(units, MidpointRounding.AwayFromZero);
        }

        public const float AnIndexReadsTheSameWithin = 1E-4f;

        public static bool SameIndex(float asked, float read) =>
            !float.IsNaN(asked) && !float.IsNaN(read) && Math.Abs(asked - read) <= AnIndexReadsTheSameWithin;

        public static List<int> InTheOrderTheyArrive(IList<float> days)
        {
            var order = new List<int>();
            if (days == null) return order;
            for (int i = 0; i < days.Count; i++) order.Add(i);
            order.Sort((x, y) =>
            {
                float a = Finite(days[x], float.MaxValue), b = Finite(days[y], float.MaxValue);
                return a != b ? a.CompareTo(b) : x.CompareTo(y);
            });
            return order;
        }

        public const float ACaravanLeavesATownAnHourWith = 1f / 3f;

        public static float ACaravanLeavesAnHourWith(float woundedShare)
        {
            float wounded = Finite(woundedShare, 1f);
            float fit = wounded > 0.4f ? 0f : wounded > 0.2f ? 0.1f : wounded > 0.1f ? 0.2f
                : wounded > 0.05f ? 0.3f : wounded > 0.025f ? 0.4f : 1f;
            return ACaravanLeavesATownAnHourWith * fit;
        }

        public const int MostTownTicksFollowed = 64;

        public static List<float> TownTicksBefore(float firstTick, float days)
        {
            var ticks = new List<float>();
            float until = Finite(days, 0f);
            if (!(until > 0f)) return ticks;
            float first = firstTick > 0f && firstTick <= 1f ? firstTick : TickTimeUnknown;
            for (float at = first; at < until && ticks.Count < MostTownTicksFollowed; at += 1f) ticks.Add(at);
            return ticks;
        }

        public static int UnitsVillagersBringToSell(int amount, bool theirOwnPackAnimal, int theyKeep) =>
            Math.Max(0, theirOwnPackAnimal ? amount - Math.Max(0, theyKeep) : amount);

        public static bool ATickMayBeKept(bool following, float tick, float arrivedAt) => !following || tick < arrivedAt;

        public static int TownTicksCountBefore(float firstTick, float days)
        {
            float until = Finite(days, 0f);
            if (!(until > 0f)) return 0;
            float first = firstTick > 0f && firstTick <= 1f ? firstTick : TickTimeUnknown;
            int count = 0;
            for (float at = first; at < until && count < MostTownTicksFollowed; at += 1f) count++;
            return count;
        }

        public static (float demand, float substituteDemand) DemandShiftedToASubstitute(
            float supply, float demand, float substituteSupply, float substituteDemand, float factor)
        {
            if (supply / demand > substituteSupply / substituteDemand && substituteDemand > demand)
            {
                float moved = (substituteDemand - demand) * Finite(factor, 0f);
                return (demand + moved, substituteDemand - moved);
            }
            return (demand, substituteDemand);
        }

        public const float YourSaleMovesItsForecastBy = 0.1f;

        public static int WorthOfAll(IDictionary<string, int> worths)
        {
            long all = 0L;
            if (worths != null) foreach (int worth in worths.Values) if (worth > 0) all += worth;
            return all > int.MaxValue ? int.MaxValue : (int)all;
        }

        public static bool YourSaleMovedEnough(string wasAt, int was, string nowAt, int now)
        {
            if (!string.Equals(wasAt, nowAt, StringComparison.Ordinal)) return true;
            if (nowAt == null) return false;
            long gap = Math.Abs((long)now - was);
            return gap > Math.Max(was, now) * (double)YourSaleMovesItsForecastBy;
        }

        public static float YourSaleCountsFor(float due, float days)
        {
            if (float.IsNaN(due) || float.IsNaN(days) || days < due) return 0f;
            return ShareThatLands(due, days);
        }

        public static float DemandMovedToASubstitute(float supply, float demand, float substituteSupply,
                                                     float substituteDemand, float factor) =>
            Finite(DemandShiftedToASubstitute(supply, demand, substituteSupply, substituteDemand, factor).demand - demand,
                   0f);

        public static float ShelfAtItsPrice(float inStoreValue, int price, int unitValue)
        {
            float shelf = Finite(inStoreValue, 0f);
            if (!(shelf > 0f) || price <= 0 || unitValue <= 0) return shelf;
            return Finite(shelf * price / unitValue, shelf);
        }

        public static void StepSupplyAndDemand(float[] supply, float[] demand, IList<int> inStore, IList<float> wanted,
                                               IList<(int kind, int substitute, float factor)> pairs,
                                               Func<int, float, float, float, float, (float supply, float demand)> step)
        {
            if (supply == null || demand == null || inStore == null || wanted == null || step == null) return;
            int n = Math.Min(Math.Min(supply.Length, demand.Length), Math.Min(inStore.Count, wanted.Count));
            for (int g = 0; g < n; g++) (supply[g], demand[g]) = step(g, inStore[g], wanted[g], supply[g], demand[g]);
            if (pairs == null) return;
            foreach (var (one, other, factor) in pairs)
            {
                if (one < 0 || one >= n || other < 0 || other >= n) continue;
                (demand[one], demand[other]) = DemandShiftedToASubstitute(supply[one], demand[one], supply[other],
                                                                          demand[other], factor);
            }
        }

        public static (float supply, float demand) TheGamesDailySupplyAndDemand(float inStore, float wanted,
                                                                              float supply, float demand) =>
            (Math.Max(0.1f, supply * 0.85f + inStore * 0.15f), demand * 0.85f + wanted * 0.15f);

        public static int WhatAWalkFetches(int units, Func<int, int> priceOfUnit)
        {
            if (units <= 0 || priceOfUnit == null) return 0;
            long gold = 0L;
            for (int i = 0; i < units; i++) gold += Math.Max(0, priceOfUnit(i));
            return gold > int.MaxValue ? int.MaxValue : (int)gold;
        }

        public const int AWalkIsCountedUnitByUnitUpTo = 16;

        public const int AWalkIsSampledAt = 8;

        public static int WhatALongWalkFetches(int units, Func<int, int> priceOfUnit)
        {
            if (units <= AWalkIsCountedUnitByUnitUpTo || priceOfUnit == null) return WhatAWalkFetches(units, priceOfUnit);
            int last = units - 1;
            double step = last / (double)AWalkIsSampledAt;
            double first = Math.Max(0, priceOfUnit(0)), end = Math.Max(0, priceOfUnit(last)), area = first + end;
            for (int k = 1; k < AWalkIsSampledAt; k++)
                area += (k % 2 == 1 ? 4d : 2d) * Math.Max(0, priceOfUnit((int)Math.Round(k * step)));
            double gold = area * step / 3d + (first + end) / 2d;
            return gold >= int.MaxValue ? int.MaxValue : (int)Math.Round(gold);
        }

        public static int ShelfForAUnitOfAWalk(int shelfBefore, int unit, int unitValue, bool soldToTheTown)
        {
            long at = (long)shelfBefore + (soldToTheTown ? 1L : -1L) * unit * Math.Max(0, unitValue);
            return at <= 0L ? 0 : at > int.MaxValue ? int.MaxValue : (int)at;
        }

        public const float OneArrivalSpansAtMost = 1E-4f;

        public static bool OnTheSameArrival(float days, float soonest) =>
            !float.IsNaN(days) && !float.IsNaN(soonest) && Math.Abs(days - soonest) <= OneArrivalSpansAtMost;

        public const float ABudgetFollowsThePriceIndexBy = 0.3f;

        public static float BudgetAtAnotherIndex(float budget, float indexThen, float indexNow)
        {
            if (!(indexThen > 0f) || !(indexNow > 0f) || indexThen == indexNow) return budget;
            return Finite(budget * (float)Math.Pow(indexThen / indexNow, ABudgetFollowsThePriceIndexBy), budget);
        }

        public static int StockAfterAShelfShift(int stock, int inStoreValueNow, int inStoreValueThen, int unitValue)
        {
            long shift = (long)inStoreValueThen - inStoreValueNow;
            long units = unitValue > 0 ? (long)Math.Round(shift / (double)unitValue, MidpointRounding.AwayFromZero) : 0L;
            long after = (long)Math.Max(0, stock) + units;
            return after <= 0L ? 0 : after > int.MaxValue ? int.MaxValue : (int)after;
        }

        public static int AnimalsACaravanSellsOff(int animals, int men) =>
            Math.Max(0, (int)(animals - men * ACaravanDrivesThisManyAnimalsAMan));

        public const int ACaravanBuysThisManyKinds = 5;

        public const int AConvoyBuysThisManyKinds = 10;

        public const int ACaravanFeelsRichAt = 5000;

        public const float ACaravanWeighsDemandBy = 0.1f;

        public const float ACaravanWeighsRoomBy = 200f;

        public const float ACaravanWantsHaulAnimalsBy = 1.5f;

        public const float ACaravanWantsFoodBy = 1.1f;

        public const float ACaravanLeavesAKindWorthUnder = 7f;

        public const float ACaravanSpendsAtMostThisShareOnAKind = 0.5f;

        public const float ACaravanSpendsAtMostThisTimesAKindsWorth = 1.5f;

        public const float ACaravanBuysNoGoodsPastThisFull = 0.9f;

        public const float APoorCaravanStillBuysBy = 0.1f;

        public const float AnEmptyCaravanBuysBy = 1.1f;

        public const float ACaravansLoadWeighsBy = 0.9f;

        public static float ACaravansBudgetFactor(int gold) =>
            APoorCaravanStillBuysBy + (gold <= 0 ? 0f : Math.Min(1f, gold / (float)ACaravanFeelsRichAt));

        public static float ACaravansRoomFactor(float weight, int capacity)
        {
            float load = Finite(weight, 0f) / (capacity + 1f) * ACaravansLoadWeighsBy;
            return AnEmptyCaravanBuysBy - (load <= 0f ? 0f : load >= 1f ? 1f : load);
        }

        public static float WhatACaravanThinksAKindIsWorth(float priceFactor, float averageIndex, int heldWorth,
                                                          float demand, float averageValue, float budgetFactor,
                                                          float roomFactor, bool food, bool haulAnimalOnLand)
        {
            float factor = Finite(priceFactor, 0f), average = Finite(averageIndex, 0f);
            if (factor <= 0f || average <= 0f) return 0f;
            float under = average * (1f - WhatHoldingAKindTakesOff(heldWorth)) - factor;
            if (under < 0f) return 0f;
            double worth = (haulAnimalOnLand ? ACaravanWantsHaulAnimalsBy : 1f) * (food ? ACaravanWantsFoodBy : 1f) *
                           (average / factor) * ACaravanWeighsDemandBy * Math.Sqrt(Math.Max(0f, Finite(demand, 0f))) *
                           (under * (double)Finite(averageValue, 0f) * budgetFactor +
                            under * (double)ACaravanWeighsRoomBy * roomFactor);
            return worth > 0d && worth < float.MaxValue ? (float)worth : 0f;
        }

        public static int WhatACaravanSpendsOnAKind(int gold, float kindWorth, int mostOnAKind)
        {
            if (gold <= 0 || mostOnAKind <= 0 || !(kindWorth >= ACaravanLeavesAKindWorthUnder)) return 0;
            double spend = Math.Min(Math.Min(gold * (double)ACaravanSpendsAtMostThisShareOnAKind,
                                             kindWorth * (double)ACaravanSpendsAtMostThisTimesAKindsWorth),
                                    mostOnAKind);
            return spend >= 1d ? (int)spend : 0;
        }

        public static int WhatACaravanTakesOfAKind(int spend, int unitPrice, int onTheShelf, int mostUnits,
                                                  float unitWeight, float weight, int capacity, bool weightless)
        {
            if (spend <= 0 || unitPrice <= 0 || onTheShelf <= 0 || mostUnits <= 0) return 0;
            int units = (int)Math.Min(Math.Round(spend / (double)unitPrice, MidpointRounding.AwayFromZero),
                                      Math.Min(onTheShelf, mostUnits));
            float carried = Finite(weight, 0f);
            if (!weightless && unitWeight > 0f && carried + units * unitWeight > capacity)
                units = (int)((capacity * ACaravanBuysNoGoodsPastThisFull - carried) / unitWeight);
            return units > 0 ? units : 0;
        }

        public const float StandingStill = 0.01f;

        public const float WalkingPace = 5f;

        // Valuable-Run-Lesson: the game's clock runs 1.2 hours each time it moves a party as far as its Speed, and every ride in a 1.104.17 test ran 25 to 40% past distance over Speed taken as hours.
        public const float HoursAPartyTakesToCoverItsSpeed = 1.2f;

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
            float hours = (landLeg / landSpeed + seaLeg / seaSpeed) * HoursAPartyTakesToCoverItsSpeed;
            float days = Finite(hours / 24f, FurthestThereIs);
            return OutOfReach(days) ? FurthestThereIs : days;
        }

        public static float DaysAtBestSpeed(float distance, float landSpeed, float seaSpeed)
        {
            if (distance <= 0f) return 0f;
            float hours = distance / Math.Max(landSpeed, seaSpeed) * HoursAPartyTakesToCoverItsSpeed;
            float days = Finite(hours / 24f, FurthestThereIs);
            return OutOfReach(days) ? FurthestThereIs : days;
        }

        public static int WhatVillagersSell(int amount, int till, int price) =>
            amount <= 0 || till <= 0 || price <= 0 ? 0 : Math.Min(amount, till / price);

        public const float ATownsPurseClosesItsGapADayBy = 0.25f;

        public static int WhatATownsPurseWouldHold(int gold, int changeToday)
        {
            long held = gold + (long)Math.Round(changeToday / ATownsPurseClosesItsGapADayBy);
            return (int)Math.Max(0L, Math.Min(int.MaxValue, held));
        }

        public static int TillWhenYouArrive(int gold, int wouldHold, IList<(float days, int gold)> paidInOrder,
                                            float days, IList<(float days, int gold)> takenInOrder = null,
                                            float firstTick = float.NaN, int income = 0)
        {
            double till = Math.Max(0, gold);
            float until = Finite(days, 0f);
            if (until <= 0f) return (int)till;
            bool ticks = firstTick >= 0f && !float.IsInfinity(firstTick) && !OutOfReach(until);
            double comes = Math.Max(0, income);
            float reach = until / 2f < HorizonStep ? until / 2f : HorizonStep;
            float end = until + reach;
            float at = 0f;
            double tick = ticks ? firstTick : double.PositiveInfinity;
            int paid = paidInOrder?.Count ?? 0, taken = takenInOrder?.Count ?? 0;
            for (int i = 0, k = 0; ;)
            {
                float paidAt = i < paid ? Math.Max(0f, Finite(paidInOrder[i].days, 0f)) : float.PositiveInfinity;
                float takenAt = k < taken ? Math.Max(0f, Finite(takenInOrder[k].days, 0f)) : float.PositiveInfinity;
                double next = Math.Min(Math.Min(paidAt, takenAt), tick);
                if (!(next < end)) break;
                float share = ShareThatLands((float)next, until);
                if (paidAt <= takenAt && paidAt <= tick)
                {
                    int amount = paidInOrder[i++].gold;
                    if (amount <= 0 || !(share > 0f)) continue;
                    float upTo = paidAt < until ? paidAt : until;
                    if (!ticks) till = APurseClosesItsGap(till, wouldHold, upTo - at);
                    till = Math.Max(0d, till - share * (double)amount);
                    at = Math.Max(at, upTo);
                }
                else if (takenAt <= tick)
                {
                    int amount = takenInOrder[k++].gold;
                    if (amount <= 0 || !(share > 0f)) continue;
                    float upTo = takenAt < until ? takenAt : until;
                    if (!ticks) till = APurseClosesItsGap(till, wouldHold, upTo - at);
                    till += share * (double)amount;
                    at = Math.Max(at, upTo);
                }
                else
                {
                    till += share * (ATownsDailyTick(till, wouldHold, comes) - till);
                    tick += 1d;
                }
            }
            if (!ticks) till = APurseClosesItsGap(till, wouldHold, until - at);
            return (int)Math.Min(int.MaxValue, Math.Round(till));
        }

        public static double ATownsDailyTick(double till, int wouldHold, double income)
        {
            double held = Math.Max(0d, till) + Math.Max(0d, income);
            return wouldHold > 0 ? held + (wouldHold - held) * ATownsPurseClosesItsGapADayBy : held;
        }

        public static int MostATillCanHold(int gold, int wouldHold, IList<(float days, int gold)> takenInOrder,
                                           int income = 0)
        {
            long most = Math.Max(Math.Max(0, gold),
                                 wouldHold > 0 ? (long)wouldHold + 3L * Math.Max(0, income) : 0L);
            if (wouldHold <= 0 && income > 0) most = long.MaxValue / 4;
            for (int k = 0; takenInOrder != null && k < takenInOrder.Count; k++)
                if (takenInOrder[k].gold > 0) most += takenInOrder[k].gold;
            return (int)Math.Min(int.MaxValue, most);
        }

        public enum TillDraw { Caravans, Villagers, VillagesNextTrips }

        public static (long caravans, long villagers, long villages, List<(string who, long gold)> top, List<string> both)
            TillDrawsBySource(IList<(int gold, TillDraw from, string who, string id)> draws, int top)
        {
            long caravans = 0L, villagers = 0L, villages = 0L;
            var byId = new Dictionary<string, (string who, long gold)>(StringComparer.Ordinal);
            var onTheRoad = new HashSet<string>(StringComparer.Ordinal);
            var nextTrip = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; draws != null && i < draws.Count; i++)
            {
                var (gold, from, who, id) = draws[i];
                if (gold <= 0) continue;
                string name = who ?? "?";
                string key = id ?? name;
                if (from == TillDraw.Caravans) caravans += gold;
                else if (from == TillDraw.Villagers) { villagers += gold; onTheRoad.Add(key); }
                else { villages += gold; nextTrip.Add(key); }
                byId.TryGetValue(key, out var had);
                byId[key] = (had.who ?? name, had.gold + gold);
            }
            var ranked = new List<(string who, long gold)>();
            foreach (var one in byId) ranked.Add(one.Value);
            ranked.Sort((x, y) => x.gold != y.gold ? y.gold.CompareTo(x.gold) : string.CompareOrdinal(x.who, y.who));
            if (top >= 0 && ranked.Count > top) ranked.RemoveRange(top, ranked.Count - top);
            var both = new List<string>();
            foreach (string key in onTheRoad) if (nextTrip.Contains(key)) both.Add(byId[key].who);
            both.Sort(StringComparer.Ordinal);
            return (caravans, villagers, villages, ranked, both);
        }

        public const float ASellerIsPaidAcross = 1f / 12f;

        public const double JustBeforeATick = 1E-4d;

        public static double[] WhatAPurseCanPay(int gold, int wouldHold, IList<(float days, int gold)> claims,
                                                IList<(float days, int gold)> takenInOrder, float firstTick,
                                                int income, float horizonDays, IList<float> shopDays = null,
                                                Func<float, double, Func<int, double>, double> shopsRun = null)
        {
            int n = claims?.Count ?? 0;
            var paid = new double[n];
            float horizon = float.IsNaN(horizonDays) || horizonDays < 0f
                ? 0f
                : Math.Min(horizonDays, MostDaysTheUnseenDrawAPurse);
            bool ticks = firstTick >= 0f && !float.IsInfinity(firstTick);
            double comes = Math.Max(0, income);
            float half = ASellerIsPaidAcross * 0.5f;
            var from = new double[n];
            var to = new double[n];
            var rate = new double[n];
            var filledFrom = new double[n];
            var filledTo = new double[n];
            var begun = new bool[n];
            var ended = new bool[n];
            var events = new List<(double at, int kind, int index)>();
            for (int i = 0; i < n; i++)
            {
                int amount = claims[i].gold;
                if (amount <= 0) continue;
                float at = claims[i].days;
                if (float.IsNaN(at) || at < 0f) at = 0f;
                from[i] = Math.Max(0d, (double)at - half);
                to[i] = (double)at + half;
                if (!(from[i] < horizon))
                {
                    paid[i] = amount;
                    continue;
                }
                rate[i] = amount / (to[i] - from[i]);
                events.Add((Math.Min(to[i], horizon), 0, i));
                events.Add((from[i], 1, i));
            }
            for (int k = 0; takenInOrder != null && k < takenInOrder.Count; k++)
            {
                float at = takenInOrder[k].days;
                if (float.IsNaN(at) || at < 0f) at = 0f;
                if (takenInOrder[k].gold > 0 && at <= horizon) events.Add((at, 2, k));
            }
            for (int k = 0; shopsRun != null && shopDays != null && k < shopDays.Count; k++)
            {
                double at = shopDays[k];
                if (!(at >= 0d) || at > horizon) continue;
                double tick = ticks ? firstTick + Math.Round(at - firstTick) : double.NaN;
                events.Add((Math.Abs(at - tick) < JustBeforeATick ? tick : at, 3, k));
            }
            if (ticks)
                for (int k = 0; firstTick + k <= horizon; k++) events.Add(((double)firstTick + k, 4, k));
            if (events.Count == 0) return paid;
            events.Sort((x, y) => x.at != y.at ? x.at.CompareTo(y.at)
                                  : x.kind != y.kind ? x.kind.CompareTo(y.kind) : x.index.CompareTo(y.index));
            double till = Math.Max(0, gold), filled = 0d, asking = 0d, now = 0d;
            int open = 0;
            double SoFar(int i)
            {
                if (i < 0 || i >= n || !begun[i] || !(rate[i] > 0d)) return 1d;
                if (ended[i]) return (filledTo[i] - filledFrom[i]) / (Math.Min(to[i], horizon) - from[i]);
                return now > from[i] ? (filled - filledFrom[i]) / (now - from[i]) : 1d;
            }
            Func<int, double> share = SoFar;
            foreach (var (at, kind, index) in events)
            {
                double span = at - now;
                if (span > 0d)
                {
                    if (!ticks) till = APurseClosesItsGap(till, wouldHold, (float)span);
                    double asked = asking * span;
                    double fill = 1d;
                    if (asked > 0d)
                    {
                        if (till >= asked) till -= asked;
                        else
                        {
                            fill = till / asked;
                            till = 0d;
                        }
                    }
                    filled += fill * span;
                    now = at;
                }
                switch (kind)
                {
                    case 0:
                        ended[index] = true;
                        filledTo[index] = filled;
                        asking -= rate[index];
                        if (--open <= 0 || asking < 0d) asking = open > 0 ? Math.Max(0d, asking) : 0d;
                        break;
                    case 1:
                        begun[index] = true;
                        filledFrom[index] = filled;
                        asking += rate[index];
                        open++;
                        break;
                    case 2:
                        till += takenInOrder[index].gold;
                        break;
                    case 3:
                        double ran = shopsRun(shopDays[index], till, share);
                        if (!double.IsNaN(ran) && !double.IsInfinity(ran)) till = Math.Max(0d, ran);
                        break;
                    default:
                        till = ATownsDailyTick(till, wouldHold, comes);
                        break;
                }
            }
            for (int i = 0; i < n; i++)
            {
                if (!(rate[i] > 0d)) continue;
                double after = to[i] > horizon ? to[i] - horizon : 0d;
                paid[i] = rate[i] * (filledTo[i] - filledFrom[i] + after);
            }
            return paid;
        }

        public enum CaravanCame { Waiting, Sold, CameAfterYou, NeverCame }

        public const double HoursACaravanIsWaitedFor = 24d;

        public const double HoursASaleIsWaitedFor = 1d;

        public static CaravanCame WhatACaravanDid(double nowHours, double dueHours, double youDueHours,
                                                  double arrivedHours, bool left, bool elsewhere, bool gone)
        {
            if (!double.IsNaN(arrivedHours))
            {
                if (!left && nowHours < arrivedHours + HoursASaleIsWaitedFor) return CaravanCame.Waiting;
                return arrivedHours > youDueHours ? CaravanCame.CameAfterYou : CaravanCame.Sold;
            }
            if (elsewhere || gone || !(nowHours <= dueHours + HoursACaravanIsWaitedFor)) return CaravanCame.NeverCame;
            return CaravanCame.Waiting;
        }

        public static int WhatVillagersSoldFor(int goldBefore, int goldNow) =>
            goldNow > goldBefore ? goldNow - goldBefore : 0;

        public static float PurseMissedBy(int said, int held)
        {
            double scale = Math.Max(1d, Math.Max((double)said, held));
            return (float)((held - (double)said) / scale);
        }

        public static double DaysSinceATickSeen(double now, double tickedAt)
        {
            double since = now - tickedAt;
            if (double.IsNaN(since) || double.IsInfinity(since) || since < 0d) return double.NaN;
            return since - Math.Floor(since);
        }

        public static double DaysSinceTheTick(double sinceItsLastRun, double sinceARunSeen)
        {
            if (sinceItsLastRun >= 0d && sinceItsLastRun <= DaysARunTellsTheTickFor) return sinceItsLastRun;
            if (!(sinceARunSeen >= 0d) || double.IsInfinity(sinceARunSeen)) return double.NaN;
            return sinceARunSeen - Math.Floor(sinceARunSeen);
        }

        public static double TheShareThatCounts(int gold, float days, float withinDays)
        {
            float until = Finite(withinDays, 0f);
            if (gold <= 0 || !(until > 0f)) return 0d;
            return gold * (double)ShareThatLands(Math.Max(0f, Finite(days, 0f)), until);
        }

        public static int WhatATownTakesInForWhatItUses(float budget, float inStore)
        {
            float budgeted = Finite(budget, 0f), stocked = Finite(inStore, 0f);
            if (!(budgeted > 0.01f) || !(stocked > 0f)) return 0;
            double takes = Math.Min(budgeted, stocked);
            return (int)Math.Min(int.MaxValue, Math.Round(takes));
        }

        public static int WhatAPurseKeepsOfAPurchase(int paid, float taxRatio)
        {
            if (paid <= 0) return 0;
            double ratio = Finite(taxRatio, 1f);
            ratio = ratio < 0d ? 0d : ratio > 1d ? 1d : ratio;
            return (int)(paid - Math.Round(paid * ratio));
        }

        private static double APurseClosesItsGap(double till, int wouldHold, float days)
        {
            if (wouldHold <= 0 || !(days > 0f)) return till;
            return wouldHold - (wouldHold - till) * Math.Pow(1d - ATownsPurseClosesItsGapADayBy, days);
        }

        public const float VillagersSetOutAnHourWith = 0.15f;

        public const float VillagersLeaveATownAnHourWith = 0.2f;

        public const float VillagersTakeEachPass = 0.2f;

        public const int VillagersLoadInPasses = 4;

        public const float AVillageWarehouseHoldsDays = 5f;

        public const float AVillageStopsMakingPastItsWarehouseBy = 1.5f;

        public const int MostLoadsAVillageIsFollowedFor = 3;

        public const float AVillagesTickUnseen = 0.5f;

        public const float AVillagesLeastHearth = 10f;

        public const float RoomEachVillagerAdds = 20f;

        public struct VillageLoad
        {
            public float Days;
            public float[] Units;
            public float SetOut;
            public float Ready;
            public float Back;
            public bool Unraised;
        }

        public const float TheDiceFillAStoreWithin = 0.5f;

        public static int AVillagesWarehouse(float madeADay) =>
            (int)Math.Ceiling(Math.Max(1f, Finite(madeADay, 0f)) * AVillageWarehouseHoldsDays);

        public static float ShareVillagersLoad() =>
            1f - (float)Math.Pow(1d - VillagersTakeEachPass, VillagersLoadInPasses);

        public static float DaysVillagersWaitFor(float chanceAnHour) =>
            chanceAnHour > 0f ? 1f / chanceAnHour / 24f : float.PositiveInfinity;

        public const float AnHourlyRollComesIn = 0.5f;

        public static float DaysUntilARollTakes(float chanceAnHour) =>
            chanceAnHour > 0f
                ? Math.Max(0f, 1f / chanceAnHour - AnHourlyRollComesIn) / 24f
                : float.PositiveInfinity;

        public const float RoomOfAPartyItself = 10f;
        public const float RoomEachPackAnimalAdds = 100f;
        public const float ANewVillagerPartyLeadsPackAnimalsAMan = 0.5f;

        public static float RoomOfANewParty(float menItSetsOutWith, int ideal, float hearth)
        {
            int men = (int)Math.Round(Math.Max(0f, Finite(menItSetsOutWith, 0f)), MidpointRounding.AwayFromZero);
            int packAnimals = (int)(ANewVillagerPartyLeadsPackAnimalsAMan * men);
            float left = Math.Max(0f, Finite(hearth, 0f) - (men + 1) / 2);
            int added = Math.Min(ideal - men, (int)left);
            if (added > 0) men += added;
            return RoomOfAPartyItself + men * RoomEachVillagerAdds + packAnimals * RoomEachPackAnimalAdds;
        }

        public static int HearthLevelOf(float hearth) => hearth >= 600f ? 2 : hearth >= 200f ? 1 : 0;

        public static float AVillageGrowsItsHearthBy(float hearth) => hearth < 300f ? 4f : hearth < 600f ? 1.2f : 0.2f;

        public static float AVillageHealsADayBy(float factionStrength)
        {
            float strength = Math.Min(7000f, Math.Max(1000f, Finite(factionStrength, 0f)));
            return 0.06f + (7000f - strength) / 100000f;
        }

        public static int TicksAVillageHealsIn(float hitPoints, float aDay)
        {
            float missing = 1f - Math.Max(0f, Math.Min(1f, Finite(hitPoints, 0f)));
            if (missing <= 1E-5f) return 1;
            if (!(Finite(aDay, 0f) > 0f)) return int.MaxValue;
            return Math.Max(1, (int)Math.Ceiling(missing / aDay - 1E-4f));
        }

        public static float DaysToAVillagesNextTick(double now, double tickedAt)
        {
            double since = now - tickedAt;
            if (double.IsNaN(since) || double.IsInfinity(since) || since < 0d) return AVillagesTickUnseen;
            double left = 1d - (since - Math.Floor(since));
            return (float)Math.Max(left, 1E-6d);
        }

        public static float[] WhatAVillageMakesEachTick(float hearth, float hearthChange, bool looted, int healsIn,
                                                        int ticks)
        {
            int count = Math.Max(0, ticks);
            var scale = new float[count + 1];
            float h = Math.Max(AVillagesLeastHearth, Finite(hearth, AVillagesLeastHearth));
            float change = Finite(hearthChange, 0f);
            int levelNow = HearthLevelOf(h);
            float extra = looted ? 0f : change - AVillageGrowsItsHearthBy(h);
            scale[0] = looted ? 0f : 1f;
            for (int j = 1; j <= count; j++)
            {
                bool lootedBefore = looted && j <= healsIn;
                h = Math.Max(AVillagesLeastHearth, h + (lootedBefore ? change : AVillageGrowsItsHearthBy(h) + extra));
                bool normal = !looted || j >= healsIn;
                scale[j] = normal ? (HearthLevelOf(h) + 1f) / (levelNow + 1f) : 0f;
            }
            return scale;
        }

        public static int IdealVillagers(int least, float hearth, float goodsADay)
        {
            float goods = Finite(goodsADay, 0f);
            float each = goods > 10f ? 40f * (1f - (Math.Min(40f, goods) - 10f) / 60f) : 40f;
            return least + (int)(Math.Max(0f, Finite(hearth, 0f)) / each);
        }

        public static float RoomOnceToppedUp(float room, int men, int ideal, float hearth)
        {
            float now = Math.Max(0f, Finite(room, 0f));
            int added = Math.Min(ideal - men, (int)Math.Max(0f, Finite(hearth, 0f)));
            return added > 0 ? now + added * RoomEachVillagerAdds : now;
        }

        public static List<VillageLoad> WhatAVillageSends(float[] held, float[] made, float[] weight, bool[] onHoof,
                                                         float room, float home, bool raised, float ride, float watch,
                                                         float nextTick, float[] scale, float rideBack = float.NaN)
        {
            float rideHome = rideBack >= 0f && !float.IsInfinity(rideBack) ? rideBack : ride;
            var loads = new List<VillageLoad>();
            if (held == null || made == null || weight == null || onHoof == null) return loads;
            int n = held.Length;
            if (made.Length != n || weight.Length != n || onHoof.Length != n) return loads;
            if (!(ride >= 0f) || float.IsInfinity(ride) || !(watch > 0f)) return loads;
            float first = nextTick > 0f && nextTick <= 1f ? nextTick : AVillagesTickUnseen;
            float[] scales = scale != null && scale.Length > 0 ? scale : new[] { 1f };
            float Scale(int j) => Math.Max(0f, Finite(scales[Math.Min(j, scales.Length - 1)], 0f));
            float TickAt(int j) => first + (j - 1);
            var at = new float[n];
            float heldAll = 0f, madeAll = 0f;
            for (int i = 0; i < n; i++)
            {
                at[i] = Math.Max(0f, Finite(held[i], 0f));
                heldAll += at[i];
                madeAll += Math.Max(0f, Finite(made[i], 0f));
            }
            int Warehouse(int j) => AVillagesWarehouse(madeAll * Scale(j));
            int ticked = 0;
            void Make()
            {
                ticked++;
                float s = Scale(ticked);
                if (!(s > 0f) || !(madeAll > 0f) || heldAll >= Warehouse(ticked) * AVillageStopsMakingPastItsWarehouseBy)
                    return;
                for (int i = 0; i < n; i++)
                {
                    float more = Math.Max(0f, Finite(made[i], 0f)) * s;
                    at[i] += more;
                    heldAll += more;
                }
            }
            float share = ShareVillagersLoad();
            float wait = DaysUntilARollTakes(VillagersSetOutAnHourWith);
            float again = DaysVillagersWaitFor(VillagersSetOutAnHourWith);
            float stay = DaysUntilARollTakes(VillagersLeaveATownAnHourWith);
            float since = 0f;
            float back = Math.Max(0f, Finite(home, 0f));
            for (int k = 0; k < MostLoadsAVillageIsFollowedFor; k++)
            {
                float ready = Math.Max(since, back);
                if (float.IsInfinity(ready) || ready > watch) break;
                while (TickAt(ticked + 1) <= ready) Make();
                while (!(Scale(ticked) > 0f && heldAll + TheDiceFillAStoreWithin >= Warehouse(ticked)))
                {
                    if (TickAt(ticked + 1) > watch) return loads;
                    ready = TickAt(ticked + 1);
                    Make();
                }
                float setOut = ready + (k == 0 && !raised ? wait + again : wait);
                if (setOut + ride > watch) break;
                while (TickAt(ticked + 1) <= setOut) Make();
                var units = new float[n];
                float carried = 0f;
                for (int i = 0; i < n; i++)
                {
                    units[i] = at[i] * share;
                    if (!onHoof[i]) carried += units[i] * Math.Max(0f, Finite(weight[i], 0f));
                }
                float cut = room > 0f && carried > room ? room / carried : 1f;
                heldAll = 0f;
                for (int i = 0; i < n; i++)
                {
                    if (!onHoof[i]) units[i] *= cut;
                    at[i] -= units[i];
                    heldAll += at[i];
                }
                loads.Add(new VillageLoad
                {
                    Days = setOut + ride, Units = units, SetOut = setOut, Ready = ready, Back = back,
                    Unraised = k == 0 && !raised
                });
                since = setOut;
                back = setOut + ride + stay + rideHome;
            }
            return loads;
        }

        public const float ACaravansTurnIsKeptFor = 7f;

        public const float LeastDaysATurnIsWeighedOver = 1f;

        public const float MostDaysTheUnseenDrawAPurse = 10f;

        public static float HowOftenSuchATurnComes(float leadDays, float watchedDays)
        {
            float lead = Finite(leadDays, float.NaN);
            if (!(lead >= 0f)) return 0f;
            float span = Math.Min(ACaravansTurnIsKeptFor, Finite(watchedDays, 0f) - lead);
            return span >= LeastDaysATurnIsWeighedOver ? 1f / span : 0f;
        }

        public static float WhatUnseenCaravansBring(IList<(float lead, float perDay)> turns, float withinDays)
        {
            float within = Finite(withinDays, 0f);
            if (turns == null || !(within > 0f)) return 0f;
            double brought = 0d;
            for (int i = 0; i < turns.Count; i++)
            {
                float lead = Finite(turns[i].lead, float.NaN);
                float perDay = Finite(turns[i].perDay, 0f);
                if (!(lead >= 0f) || !(perDay > 0f) || lead >= within) continue;
                brought += (double)perDay * (within - lead);
            }
            return brought > float.MaxValue ? float.MaxValue : (float)brought;
        }

        public static void UnseenDraws(IList<(float lead, float perDay)> draws, float horizonDays,
                                       List<(float days, int gold)> into)
        {
            if (draws == null || into == null) return;
            float horizon = Math.Min(Finite(horizonDays, 0f), MostDaysTheUnseenDrawAPurse);
            float before = 0f;
            for (float at = HorizonStep; at <= horizon + 1E-4f; at += HorizonStep)
            {
                float upTo = WhatUnseenCaravansBring(draws, at);
                int gold = (int)Math.Min(int.MaxValue, Math.Round(upTo - before));
                before = upTo;
                if (gold > 0) into.Add((at - HorizonStep * 0.5f, gold));
            }
        }

        public static float WetGroundStep(int men, int riders, int footmen, int spareMounts)
        {
            if (men <= 0) return 0f;
            float cavalry = riders > 0 ? 0.3f * Math.Min(riders, men) / men : 0f;
            int onHorses = Math.Min(Math.Max(0, footmen), Math.Max(0, spareMounts));
            float footmenOnHorses = onHorses > 0 ? 0.15f * Math.Min(onHorses, men) / men : 0f;
            return -(cavalry * 0.3f) - footmenOnHorses * 0.3f;
        }

        public static float SpeedByDay(float now, float atNight, float byDay)
        {
            if (!(now > 0f) || !(atNight > 0f) || !(byDay > atNight)) return now;
            float day = Finite(now * (byDay / atNight), now);
            return day > now ? day : now;
        }

        public const int GroundElse = 0;
        public const int GroundPlain = 1;
        public const int GroundForest = 2;
        public const int GroundWater = 3;
        public const int GroundDesert = 4;
        public const int GroundSea = 5;
        public const int Grounds = 6;
        public const int Unread = -1;

        public struct Run
        {
            public float Length;
            public int Ground;
            public int Face;
            public int Cell;
        }

        public static void AddRun(List<Run> road, int ground, int face, int cell, float length)
        {
            if (road == null || !(length > 0f) || float.IsInfinity(length)) return;
            int last = road.Count - 1;
            if (last >= 0 && road[last].Ground == ground && road[last].Face == face && road[last].Cell == cell)
            {
                Run joined = road[last];
                joined.Length += length;
                road[last] = joined;
                return;
            }
            road.Add(new Run { Length = length, Ground = ground, Face = face, Cell = cell });
        }

        public static float LengthOn(IList<Run> road, int ground)
        {
            double length = 0d;
            for (int i = 0; road != null && i < road.Count; i++)
                if (ground < 0 || road[i].Ground == ground) length += Finite(road[i].Length, 0f);
            return (float)length;
        }

        public struct Piece
        {
            public float Length;
            public long Region;
        }

        public const float LeastStepOnARoad = 0.01f;
        internal const int MostProbesOnALeg = 100000;
        internal const int MostSplitsOfAStep = 32;

        private static void AddPiece(List<Piece> pieces, float length, long region)
        {
            if (!(length > 0f) || float.IsInfinity(length)) return;
            int last = pieces.Count - 1;
            if (last >= 0 && pieces[last].Region == region)
                pieces[last] = new Piece { Length = pieces[last].Length + length, Region = region };
            else
                pieces.Add(new Piece { Length = length, Region = region });
        }

        public static void RegionsAlong(float length, float stride, Func<float, long> regionAt, List<Piece> pieces)
        {
            if (pieces == null || regionAt == null || !(length > 0f) || float.IsInfinity(length)) return;
            float nudge = Math.Min(LeastStepOnARoad * 0.5f, length * 0.5f);
            int probes = stride > 0f && !float.IsInfinity(stride) ? (int)Math.Ceiling(length / stride) : 1;
            if (probes < 1) probes = 1;
            if (probes > MostProbesOnALeg) probes = MostProbesOnALeg;
            float at = nudge, from = 0f;
            long region = regionAt(at);
            for (int k = 1; k <= probes; k++)
            {
                float next = k == probes ? length - nudge : length * k / probes;
                if (next < at) next = at;
                long there = regionAt(next);
                if (there != region) Edges(at, region, next, there, regionAt, ref from, pieces, 0);
                at = next;
                region = there;
            }
            AddPiece(pieces, length - from, region);
        }

        private static void Edges(float low, long below, float high, long above, Func<float, long> regionAt,
                                  ref float from, List<Piece> pieces, int splits)
        {
            if (high - low <= LeastStepOnARoad || splits >= MostSplitsOfAStep)
            {
                float edge = (low + high) * 0.5f;
                AddPiece(pieces, edge - from, below);
                if (edge > from) from = edge;
                return;
            }
            float middle = (low + high) * 0.5f;
            long between = regionAt(middle);
            if (between != below) Edges(low, below, middle, between, regionAt, ref from, pieces, splits + 1);
            if (between != above) Edges(middle, between, high, above, regionAt, ref from, pieces, splits + 1);
        }

        public static long RegionOf(int face, int cell) => ((long)face << 32) | (uint)cell;

        public static int FaceOfRegion(long region) => (int)(region >> 32);

        public static int CellOfRegion(long region) => (int)(region & 0xFFFFFFFFL);

        public static int TurnsUntil(int slot, int next, int count) =>
            count <= 0 ? 0 : ((slot - next) % count + count) % count;

        public static int LastAtOrBefore(IList<double> at, double from, double hours)
        {
            if (at == null || at.Count == 0 || !(at[0] - from <= hours)) return -1;
            int low = 0, high = at.Count - 1;
            while (low < high)
            {
                int middle = (low + high + 1) / 2;
                if (at[middle] - from <= hours) low = middle;
                else high = middle - 1;
            }
            return low;
        }

        public interface ISky
        {
            void Read(int cell, double hours, out bool wet, out bool snowy, out double changes);
        }

        public sealed class ClearSky : ISky
        {
            public static readonly ClearSky Always = new ClearSky();

            public void Read(int cell, double hours, out bool wet, out bool snowy, out double changes)
            {
                wet = false;
                snowy = false;
                changes = double.PositiveInfinity;
            }
        }

        public sealed class Pace
        {
            public float Base;
            public float Rest;
            public float Least = float.MinValue;
            public float Most = float.MaxValue;
            public readonly float[] OnGround = new float[Grounds];
            public float Day;
            public float Night;
            public float Snow;
            public float Wet;
            public float Slow;
            public double SlowHours;
            public double ReadAt;
            public bool SnowAtStart;
            public float Sea;
            public float SeaAdd;
            public float SeaRest;
            public readonly float[] OnWater = new float[Waters];
            public readonly float[] AddOnWater = new float[Waters];
            public float SeaSnow;
            public float SeaSlow;
            public float SeaLeast;
            public float FairWinds;
            public float ShockAndAwe;
            public bool Even;

            public static Pace AllAt(float land, float sea) => new Pace { Base = land, Sea = sea, Even = true };

            public Pace After(double hours)
            {
                if (hours == 0d || double.IsNaN(hours) || !(SlowHours > 0d)) return this;
                var later = (Pace)MemberwiseClone();
                later.SlowHours = SlowHours > hours ? SlowHours - hours : 0d;
                return later;
            }

            public float LiftOf(Sail sail)
            {
                float fair = Finite(sail.Fair, 0f);
                fair = fair < 0f ? 0f : fair > 1f ? 1f : fair;
                float lift = Finite(sail.Push, 0f) + fair * Finite(FairWinds, 0f) + (1f - fair) * Finite(ShockAndAwe, 0f);
                return lift > 0f ? lift : 0f;
            }

            public float AtSea(int water, float lift, bool snow, bool slow)
            {
                int w = water >= 0 && water < Waters ? water : WaterUnread;
                float hull = Finite(Sea, 0f) + Finite(SeaAdd, 0f) + Finite(AddOnWater[w], 0f);
                float sum = Finite(SeaRest, 0f) + Finite(OnWater[w], 0f) + (snow ? Finite(SeaSnow, 0f) : 0f) +
                            (slow ? Finite(SeaSlow, 0f) : 0f);
                float up = Finite(lift, 0f);
                float speed = hull * (1f + sum) * (1f + (up > 0f ? up : 0f));
                float least = Finite(SeaLeast, 0f);
                speed = speed < least ? least : speed > ASailIsNeverFasterThan ? ASailIsNeverFasterThan : speed;
                return Finite(speed, 0f);
            }

            public float AtSea(Sail sail, bool snow, bool slow) => AtSea(sail.Water, LiftOf(sail), snow, slow);

            public float Afloat => AtSea(WaterUnread, 0f, false, false);

            public float BestAtSea
            {
                get
                {
                    float lift = Math.Max(WindAtMost * AWindPushesAtMostBy + Finite(FairWinds, 0f),
                                          Finite(ShockAndAwe, 0f));
                    float best = 0f;
                    for (int w = 0; w < Waters; w++) best = Math.Max(best, AtSea(w, lift, false, false));
                    return best;
                }
            }

            public float On(int ground, bool dark, bool snow, bool wet, bool slow)
            {
                if (ground == GroundSea) return AtSea(WaterUnread, 0f, snow, slow);
                if (Even) return Finite(Base, 0f);
                int g = ground >= 0 && ground < Grounds ? ground : GroundElse;
                float sum = Rest + OnGround[g] + (dark ? Night : Day) + (snow ? Snow : 0f) + (wet ? Wet : 0f) +
                            (slow ? Slow : 0f);
                float speed = Base + Base * sum;
                speed = speed < Least ? Least : speed > Most ? Most : speed;
                return Finite(speed, 0f);
            }

            public float BestOnLand
            {
                get
                {
                    if (Even) return Finite(Base, 0f);
                    float best = 0f;
                    for (int g = 0; g < Grounds; g++)
                    {
                        if (g == GroundSea) continue;
                        for (int c = 0; c < 16; c++)
                            best = Math.Max(best, On(g, (c & 1) != 0, (c & 2) != 0, (c & 4) != 0, (c & 8) != 0));
                    }
                    return best;
                }
            }
        }

        public sealed class Tally
        {
            public double Hours, Night, Wet, Snow, Slow;
            public readonly double[] OnGround = new double[Grounds];

            internal void Count(double hours, int ground, bool dark, bool wet, bool snow, bool slow)
            {
                if (!(hours > 0d)) return;
                Hours += hours;
                if (ground >= 0 && ground < Grounds) OnGround[ground] += hours;
                if (dark) Night += hours;
                if (wet) Wet += hours;
                if (snow) Snow += hours;
                if (slow) Slow += hours;
            }
        }

        private static void ClampLight(ref int sunrise, ref int sunset)
        {
            sunrise = sunrise < 0 ? 0 : sunrise > 24 ? 24 : sunrise;
            sunset = sunset < 0 ? 0 : sunset > 24 ? 24 : sunset;
        }

        public static bool DarkAt(double clock, int sunrise, int sunset)
        {
            ClampLight(ref sunrise, ref sunset);
            double hour = clock - 24d * Math.Floor(clock / 24d);
            return !(hour >= sunrise && hour < sunset);
        }

        public static double NextTurnOfLight(double clock, int sunrise, int sunset)
        {
            ClampLight(ref sunrise, ref sunset);
            if (sunrise >= sunset || (sunrise == 0 && sunset == 24)) return double.PositiveInfinity;
            double day = 24d * Math.Floor(clock / 24d);
            double hour = clock - day;
            if (hour < sunrise) return day + sunrise;
            if (hour < sunset) return day + sunset;
            return day + 24d + sunrise;
        }

        private const int MostStepsOnARoad = 1000000;

        private static float DaysTaken(double hours)
        {
            float days = Finite((float)(hours / 24d), FurthestThereIs);
            return days < 0f ? 0f : OutOfReach(days) ? FurthestThereIs : days;
        }

        private static int NextRun(IList<Run> road, int i)
        {
            while (i < road.Count && !(road[i].Length > 0f)) i++;
            return i;
        }

        public const int WaterUnread = 0;
        public const int WaterOpen = 1;
        public const int WaterCoast = 2;
        public const int WaterRiver = 3;
        public const int WaterElse = 4;
        public const int Waters = 5;

        public struct Sail
        {
            public float Push;
            public float Fair;
            public int Water;
        }

        public const float TheOpenSeaAdds = 0.448f;

        public const float ARiverAdds = 0.5f;

        public const float AShallowDraftMoves = 0.066f;

        public const float AnUndermannedFleetLosesUpTo = 0.4f;

        public const int AFleetSlowsPast = 3;

        public const float AFleetSlowsAtMostBy = 0.2f;

        public const float AFleetSlowsThisSteeply = 0.5f;

        public const float AFleetSlowsHalfwayAt = 3f;

        public const float DisorganizedAtSea = -0.4f;

        public static float Undermanned(int crew, int needed) =>
            needed <= 0 || crew >= needed ? 0f : -(1f - crew / (float)needed) * AnUndermannedFleetLosesUpTo;

        public static float Overmanned(int men, int most) =>
            men <= most ? 0f : most <= 0 ? -1f : 1f / (men / (float)most) - 1f;

        public static float OverFleetSize(int ships, float eased)
        {
            if (ships <= AFleetSlowsPast) return 0f;
            double over = ships - AFleetSlowsPast;
            double slows = AFleetSlowsAtMostBy / (1d + Math.Exp(-AFleetSlowsThisSteeply * (over - AFleetSlowsHalfwayAt)));
            return Finite((float)(-slows * Finite(eased, 1f)), 0f);
        }

        public static float Overburdened(float weight, int capacity, float eased)
        {
            if (!(weight > capacity) || capacity <= 0) return 0f;
            return Finite(-(weight - capacity) / capacity * Finite(eased, 1f), 0f);
        }

        public static float ShallowDraft(float shallowSpeeds, int ships, bool coastal)
        {
            if (ships <= 0) return 0f;
            float moves = Finite(shallowSpeeds, 0f) * AShallowDraftMoves / ships;
            return coastal ? moves : -moves;
        }

        public static bool Coastal(int water) => water == WaterCoast || water == WaterRiver;

        public const float WindAtLeast = 1f / 15f;

        public const float WindAtMost = 0.46f;

        public const float AWindPushesWithin = 120f;

        public const float AWindPushesAtMostBy = 1.5f;

        public const float ASailIsNeverFasterThan = 10f;

        public static void WindOnTheMap(float flowX, float flowY, out float windX, out float windY)
        {
            float length = (float)Math.Sqrt(flowX * flowX + flowY * flowY);
            float strength = length < WindAtLeast ? WindAtLeast : length > WindAtMost ? WindAtMost : length;
            float x = flowX, y = flowY;
            if (length > 1E-05f)
            {
                x /= length;
                y /= length;
            }
            else
            {
                x = 0f;
                y = 1f;
            }
            windX = x * strength;
            windY = y * strength;
        }

        public static float WindBehind(float bearingX, float bearingY, float windX, float windY, out bool astern)
        {
            astern = false;
            float strength = (float)Math.Sqrt(windX * windX + windY * windY);
            if (!(strength > 0f) || float.IsInfinity(strength)) return 0f;
            float angle = Math.Abs((float)Math.Atan2(-bearingX, bearingY) - (float)Math.Atan2(-windX, windY)) * 57.29578f;
            if (!(angle < AWindPushesWithin)) return 0f;
            astern = true;
            float push = (strength - angle * strength / AWindPushesWithin) * AWindPushesAtMostBy;
            return push < 0f ? 0f : push > AWindPushesAtMostBy ? AWindPushesAtMostBy : push;
        }

        public static void AddRunAtSea(List<Run> road, List<Sail> sails, int face, int cell, int water, float length,
                                       float push, float astern)
        {
            if (road == null || sails == null || !(length > 0f) || float.IsInfinity(length)) return;
            float up = Finite(push, 0f);
            float behind = Finite(astern, 0f);
            int last = road.Count - 1;
            if (last >= 0 && last < sails.Count && road[last].Ground == GroundSea && road[last].Face == face &&
                road[last].Cell == cell && sails[last].Water == water)
            {
                Run joined = road[last];
                Sail held = sails[last];
                float before = joined.Length;
                joined.Length += length;
                road[last] = joined;
                double time = before / (1d + held.Push) + length / (1d + up);
                held.Push = time > 0d ? (float)(joined.Length / time - 1d) : 0f;
                held.Fair = (before * held.Fair + length * behind) / joined.Length;
                sails[last] = held;
                return;
            }
            road.Add(new Run { Length = length, Ground = GroundSea, Face = face, Cell = cell });
            sails.Add(new Sail { Push = up, Fair = behind, Water = water });
        }

        public static float DaysAlong(IList<Run> road, Pace pace, ISky sky, double startClock, int sunrise, int sunset,
                                      bool freshAtStart, Tally tally, IList<Sail> sails = null)
        {
            if (road == null || pace == null) return 0f;
            for (int k = 0; k < road.Count; k++)
                if (float.IsNaN(road[k].Length) || float.IsInfinity(road[k].Length)) return FurthestThereIs;
            int i = NextRun(road, 0);
            if (i >= road.Count) return 0f;
            if (sky == null) sky = ClearSky.Always;
            double start = double.IsNaN(startClock) || double.IsInfinity(startClock) ? 0d : startClock;
            double t = 0d;
            bool dark = DarkAt(start, sunrise, sunset);
            double turnAt = NextTurnOfLight(start, sunrise, sunset);
            if (!(turnAt > start)) turnAt = double.PositiveInfinity;
            double turn = turnAt - start;
            bool slow = pace.SlowHours > 0d;
            double slowEnds = slow ? pace.SlowHours : double.PositiveInfinity;
            int cell = road[i].Cell;
            sky.Read(cell, 0d, out bool wet, out bool snowyHere, out double change);
            bool snow = freshAtStart ? snowyHere : pace.SnowAtStart;
            double left = road[i].Length;
            for (int steps = 0; ; steps++)
            {
                if (steps >= MostStepsOnARoad) return FurthestThereIs;
                float speed = sails != null && road[i].Ground == GroundSea && i < sails.Count
                    ? pace.AtSea(sails[i], snow, slow)
                    : pace.On(road[i].Ground, dark, snow, wet, slow);
                double perHour = speed / (double)HoursAPartyTakesToCoverItsSpeed;
                if (!(perHour > 0d) || double.IsInfinity(perHour)) return FurthestThereIs;
                double next = Math.Min(turn, Math.Min(change, slowEnds));
                double done = t + left / perHour;
                if (done <= next)
                {
                    tally?.Count(done - t, road[i].Ground, dark, wet, snow, slow);
                    t = done;
                    int was = i;
                    i = NextRun(road, i + 1);
                    if (i >= road.Count) break;
                    left = road[i].Length;
                    if (road[i].Cell != cell)
                    {
                        cell = road[i].Cell;
                        sky.Read(cell, t, out bool wetThere, out snowyHere, out change);
                        if (wetThere != wet)
                        {
                            wet = wetThere;
                            snow = snowyHere;
                        }
                    }
                    if (road[i].Face != road[was].Face) snow = snowyHere;
                }
                else
                {
                    tally?.Count(next - t, road[i].Ground, dark, wet, snow, slow);
                    left -= (next - t) * perHour;
                    if (left < 0d) left = 0d;
                    t = next;
                    bool afresh = false;
                    if (t >= turn)
                    {
                        dark = !dark;
                        double turned = turnAt;
                        turnAt = NextTurnOfLight(turned, sunrise, sunset);
                        if (!(turnAt > turned)) turnAt = double.PositiveInfinity;
                        turn = turnAt - start;
                        afresh = true;
                    }
                    if (t >= change)
                    {
                        sky.Read(cell, t, out bool wetNow, out snowyHere, out change);
                        if (wetNow != wet)
                        {
                            wet = wetNow;
                            afresh = true;
                        }
                    }
                    if (t >= slowEnds)
                    {
                        slow = false;
                        slowEnds = double.PositiveInfinity;
                        afresh = true;
                    }
                    if (afresh) snow = snowyHere;
                }
                if (OutOfReach(DaysTaken(t))) return FurthestThereIs;
            }
            return DaysTaken(t);
        }

        public sealed class SkyAfter : ISky
        {
            private readonly ISky _sky;
            private readonly double _after;

            public SkyAfter(ISky sky, double after)
            {
                _sky = sky ?? ClearSky.Always;
                _after = double.IsNaN(after) || double.IsInfinity(after) ? 0d : after;
            }

            public void Read(int cell, double hours, out bool wet, out bool snowy, out double changes)
            {
                _sky.Read(cell, hours + _after, out wet, out snowy, out changes);
                changes -= _after;
            }
        }

        public static float DaysAfloatThenAshore(IList<Run> afloat, IList<Sail> sails, float hoursGoingAshore,
                                                 IList<Run> ashore, Pace pace, ISky sky, double startClock, int sunrise,
                                                 int sunset, bool freshAtStart, Tally tally)
        {
            if (pace == null) return 0f;
            float atSea = DaysAlong(afloat, pace, sky, startClock, sunrise, sunset, freshAtStart, tally, sails);
            if (OutOfReach(atSea)) return FurthestThereIs;
            float going = Finite(hoursGoingAshore, 0f);
            double landed = atSea * 24d + (going > 0f ? going : 0f);
            double start = double.IsNaN(startClock) || double.IsInfinity(startClock) ? 0d : startClock;
            float onLand = DaysAlong(ashore, pace.After(landed), new SkyAfter(sky, landed), start + landed, sunrise,
                                     sunset, true, tally);
            if (OutOfReach(onLand)) return FurthestThereIs;
            return DaysTaken(landed + onLand * 24d);
        }

        public static bool ClearOfLimits(float speed, float least, float most) =>
            speed > 0f && speed > Finite(least, float.MinValue) * 1.001f && speed < Finite(most, float.MaxValue) * 0.999f;

        public static bool LikeAStep(float step) => step > -0.9f && step < 0.5f;

        public static void WeatherSteps(bool snowyBefore, bool wetBefore, bool snowyAfter, bool wetAfter,
                                        float factorsBefore, float factorsAfter, float wetStepKnown,
                                        out float snowStep, out float wetStep)
        {
            snowStep = float.NaN;
            wetStep = float.NaN;
            float change = factorsAfter - factorsBefore;
            if (float.IsNaN(change) || float.IsInfinity(change)) return;
            bool snowTurned = snowyBefore != snowyAfter, wetTurned = wetBefore != wetAfter;
            if (wetTurned && !snowTurned) wetStep = wetAfter ? change : -change;
            else if (snowTurned && !wetTurned) snowStep = snowyAfter ? change : -change;
            else if (snowTurned && wetTurned && snowyAfter == wetAfter && !float.IsNaN(wetStepKnown))
                snowStep = (snowyAfter ? change : -change) - wetStepKnown;
        }

        public static float NightStepFrom(float paceBefore, float paceAfter, float factorsAfter, bool nightFell)
        {
            if (!(paceBefore > 0f) || !(paceAfter > 0f)) return float.NaN;
            float factorsBefore = (1f + factorsAfter) * (paceBefore / paceAfter) - 1f;
            float step = nightFell ? factorsAfter - factorsBefore : factorsBefore - factorsAfter;
            return float.IsNaN(step) || float.IsInfinity(step) ? float.NaN : step;
        }

        public static float PaceAfterTheTurn(float paceBefore, float factorsAfter, float paceAfter,
                                             float nightStep, bool nightFell)
        {
            if (!(paceBefore > 0f) || !(paceAfter > 0f)) return float.NaN;
            float factorsBefore = (1f + factorsAfter) * (paceBefore / paceAfter) - 1f;
            float onePlus = 1f + factorsBefore;
            if (!(onePlus > 0f)) return float.NaN;
            float turned = 1f + factorsBefore + (nightFell ? nightStep : -nightStep);
            return Finite(paceBefore * turned / onePlus, float.NaN);
        }

        public const float LongestARideRunsOver = 10f;

        public static float HowMuchLongerTheRideIs(float guessed, float real)
        {
            if (!(guessed > 0f) || !(real > 0f) || float.IsNaN(real) || float.IsInfinity(real)) return float.NaN;
            float longer = real / guessed;
            if (float.IsNaN(longer) || longer > LongestARideRunsOver) return float.NaN;
            return longer < 1f ? 1f : longer;
        }

        public static float Quantile(List<float> sorted, float share)
        {
            if (sorted == null || sorted.Count == 0) return float.NaN;
            float at = Math.Min(Math.Max(Finite(share, 0.5f), 0f), 1f) * (sorted.Count - 1);
            int below = (int)Math.Floor(at);
            int above = Math.Min(below + 1, sorted.Count - 1);
            return sorted[below] + (sorted[above] - sorted[below]) * (at - below);
        }

        public static float RideOffBy(float saidHours, float tookHours)
        {
            if (!(saidHours > 0f) || float.IsNaN(tookHours) || float.IsInfinity(tookHours)) return float.NaN;
            return Finite((tookHours - saidHours) / saidHours, float.NaN);
        }

        public static int UnitsThatFit(float room, float each)
        {
            if (!(each > 0.01f)) return int.MaxValue;
            if (!(room > 0f)) return 0;
            double fit = Math.Floor((double)room / each);
            return fit >= int.MaxValue ? int.MaxValue : (int)fit;
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

        public static int WorthOf(IList<int> each)
        {
            long worth = 0L;
            for (int i = 0; each != null && i < each.Count; i++) worth += each[i];
            return worth > int.MaxValue ? int.MaxValue : worth < int.MinValue ? int.MinValue : (int)worth;
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

        public static int SpendLimit(int maxSpendPerVisit, long purseBeforeBuying, bool adaptive, long evidence)
        {
            int cap = AdaptiveSpendCap(maxSpendPerVisit, purseBeforeBuying, adaptive);
            if (cap <= 0 || evidence <= cap) return cap;
            return evidence >= int.MaxValue ? int.MaxValue : (int)evidence;
        }

        public const float LightestWeightCounted = 0.1f;

        public static float Key(long profit, float confidence, float trust, bool rankByConfidence,
                                float weightEach, int units, float days)
        {
            if (profit <= 0L || units <= 0 || float.IsNaN(days)) return 0f;
            float weight = weightEach > LightestWeightCounted ? weightEach : LightestWeightCounted;
            float over = days > NoTripCountsShorterThan ? days : NoTripCountsShorterThan;
            double kept = rankByConfidence ? profit * (double)confidence * trust : profit;
            return Finite((float)(kept / ((double)weight * units * over)), 0f);
        }

        public static float KeyRetimed(float key, float scanDays, float days)
        {
            if (!(key > 0f) || float.IsNaN(scanDays) || float.IsNaN(days)) return key;
            float was = scanDays > NoTripCountsShorterThan ? scanDays : NoTripCountsShorterThan;
            float now = days > NoTripCountsShorterThan ? days : NoTripCountsShorterThan;
            return Finite((float)((double)key * was / now), 0f);
        }

        public static List<int> ByShownScore(IList<float> keys, int top)
        {
            var order = new List<int>(keys?.Count ?? 0);
            for (int i = 0; keys != null && i < keys.Count; i++) order.Add(i);
            order.Sort((x, y) =>
            {
                int byKey = keys[y].CompareTo(keys[x]);
                return byKey != 0 ? byKey : x.CompareTo(y);
            });
            if (top >= 0 && order.Count > top) order.RemoveRange(top, order.Count - top);
            return order;
        }

        public static float HerdWeight(float cargoRoom, int herded, int herdRoom)
        {
            long slots = (long)Math.Max(0, herded) + Math.Max(0, herdRoom);
            if (slots <= 0L || !(cargoRoom > 0f) || float.IsInfinity(cargoRoom)) return 0f;
            return (float)(cargoRoom / slots);
        }

        public static float PerDenarADay(long profit, int cost, float days) =>
            cost <= 0 ? 0f : PerDay(profit, days) / cost;

        public static float KeyByDenar(long profit, float confidence, float trust, bool rankByConfidence,
                                       int cost, float days)
        {
            if (profit <= 0L || cost <= 0 || float.IsNaN(days)) return 0f;
            float over = days > NoTripCountsShorterThan ? days : NoTripCountsShorterThan;
            double kept = rankByConfidence ? profit * (double)confidence * trust : profit;
            return Finite((float)(kept / ((double)cost * over)), 0f);
        }

        public static float KeyByProfit(float confidence, float trust, bool rankByConfidence, float days)
        {
            if (float.IsNaN(days)) return 0f;
            float over = days > NoTripCountsShorterThan ? days : NoTripCountsShorterThan;
            double kept = rankByConfidence ? (double)confidence * trust : 1d;
            return Finite((float)(kept / over), 0f);
        }

        public const double ThinLotShare = 0.1d;

        public static (int units, bool sold) WhatACaravanMoved(int amount) =>
            amount > 0 ? (amount, true) : amount < 0 && amount != int.MinValue ? (-amount, false) : (0, false);

        public static bool TheScanHasRested(long sinceItEnded, long itTook, double restsFor) =>
            itTook <= 0L || sinceItEnded >= itTook * restsFor;

        public static bool TooThinBeside(double weighed, double best) =>
            best > 0d && !double.IsNaN(weighed) && weighed < best * ThinLotShare;

        public static long WhatThisOrderBuys(IList<(int units, int cost, int profit, float hauls, float keyByProfit)> plans,
                                             int money, float room) =>
            WhatThisOrderBuys(plans, money, room, out _);

        public static long WhatThisOrderBuys(IList<(int units, int cost, int profit, float hauls, float keyByProfit)> plans,
                                             int money, float room, out double weighed)
        {
            weighed = 0d;
            if (plans == null || money <= 0) return 0L;
            double left = money;
            double space = room < 0f ? double.PositiveInfinity : room;
            long made = 0L;
            for (int i = 0; i < plans.Count; i++)
            {
                if (plans[i].units <= 0 || plans[i].cost <= 0) continue;
                double each = (double)plans[i].cost / plans[i].units;
                double heavy = plans[i].hauls > 0f ? (double)plans[i].hauls / plans[i].units : 0d;
                int take = (int)Math.Min(plans[i].units, Math.Floor(left / each));
                if (heavy > 0d) take = (int)Math.Min(take, Math.Floor(space / heavy));
                if (take <= 0) continue;
                double profit = (double)plans[i].profit * take / plans[i].units;
                made += (long)Math.Round(profit);
                weighed += profit * plans[i].keyByProfit;
                left -= each * take;
                space -= heavy * take;
            }
            return made;
        }

        public const double OrdersWeighTheSameWithin = 1e-6;

        public static bool TheOtherOrderEarnsMore(double weighed, long made, double otherWeighed, long otherMade)
        {
            double apart = otherWeighed - weighed;
            double close = OrdersWeighTheSameWithin * Math.Max(1d, Math.Max(Math.Abs(weighed), Math.Abs(otherWeighed)));
            if (Math.Abs(apart) > close) return apart > 0d;
            return otherMade > made;
        }

        public static int Scaled(int gold, int units, int pricedFor)
        {
            if (pricedFor <= 0 || units == pricedFor) return gold;
            long scaled = (long)gold * units / pricedFor;
            return scaled > int.MaxValue ? int.MaxValue : scaled < int.MinValue ? int.MinValue : (int)scaled;
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

        public static float ShareThatLands(float etaDays, float horizonDays)
        {
            float reach = horizonDays / 2f < HorizonStep ? horizonDays / 2f : HorizonStep;
            if (!(reach > 0f)) return LandsInTime(etaDays, horizonDays) ? 1f : 0f;
            float share = Finite((horizonDays + reach - etaDays) / (2f * reach), 0f);
            return share <= 0f ? 0f : share >= 1f ? 1f : share;
        }

        public const float HorizonStep = 0.25f;

        public static float ToTheQuarterDay(float days)
        {
            days = Finite(days, 0f);
            if (days <= 0f) return 0f;
            double steps = Math.Floor(days / HorizonStep + 0.5d);
            return steps <= 0d ? 0f : Finite((float)(steps * HorizonStep), days);
        }

        public static float TheQuarterDayBelow(float days)
        {
            days = Finite(days, 0f);
            if (!(days > HorizonStep)) return ARideToTheQuarterDay(days);
            double steps = Math.Floor(days / HorizonStep);
            return Finite((float)(steps * HorizonStep), HorizonStep);
        }

        public static float ShareOfTheQuarterDayAbove(float days, float quarterDayBelow)
        {
            days = Finite(days, 0f);
            if (!(days > HorizonStep) || !(days > quarterDayBelow)) return 0f;
            float share = (days - quarterDayBelow) / HorizonStep;
            return share <= 0f ? 0f : share >= 1f ? 1f : share;
        }

        public static void ForecastHorizon(float days, out float low, out float high, out float share)
        {
            float d = Finite(days, 0f);
            if (d < HorizonStep)
            {
                low = d > 0f ? d : 0f;
                high = low;
                share = 0f;
                return;
            }
            low = TheQuarterDayBelow(d);
            high = low + HorizonStep;
            share = ShareOfTheQuarterDayAbove(d, low);
        }

        public static int ShiftOverTheHorizon<T>(float withinDays, T at, Func<T, float, int> cachedAt,
                                                 Func<T, float, int> freshAt)
        {
            ForecastHorizon(withinDays, out float low, out float high, out float share);
            int below = low >= HorizonStep ? cachedAt(at, low) : freshAt(at, low);
            return share > 0f ? BlendedShift(below, cachedAt(at, high), share) : below;
        }

        public static int BlendedShift(int below, int above, float share)
        {
            if (!(share > 0f)) return below;
            if (share >= 1f) return above;
            double blended = below + (above - (double)below) * share;
            return (int)Math.Round(Math.Max(int.MinValue, Math.Min(int.MaxValue, blended)), MidpointRounding.AwayFromZero);
        }

        public static float ARideToTheQuarterDay(float days)
        {
            float quartered = ToTheQuarterDay(days);
            return quartered <= 0f && Finite(days, 0f) > 0f ? HorizonStep : quartered;
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

        public const double ATickRunAgreesWithin = 1d / 24d;

        public static double TheRunThatTellsTheTick(IList<double> runs, double now, double seenRun)
        {
            double earliest = double.NaN;
            for (int i = 0; runs != null && i < runs.Count; i++)
                if (ARecentRun(runs[i], now) && (double.IsNaN(earliest) || runs[i] < earliest)) earliest = runs[i];
            if (double.IsNaN(earliest) || double.IsNaN(seenRun) || double.IsInfinity(seenRun) ||
                OnTheSameHourOfTheDay(earliest, seenRun))
                return earliest;
            int agreeing = 0;
            for (int i = 0; runs != null && i < runs.Count; i++)
                if (ARecentRun(runs[i], now) && OnTheSameHourOfTheDay(runs[i], earliest)) agreeing++;
            return agreeing >= 2 ? earliest : double.NaN;
        }

        private static bool ARecentRun(double run, double now)
        {
            double since = now - run;
            return since >= 0d && since <= DaysARunTellsTheTickFor;
        }

        private static bool OnTheSameHourOfTheDay(double one, double other)
        {
            double apart = Math.Abs((one - Math.Floor(one)) - (other - Math.Floor(other)));
            return Math.Min(apart, 1d - apart) <= ATickRunAgreesWithin;
        }

        public static float ATownTicksIn(double daysSinceTheLastTick)
        {
            if (double.IsNaN(daysSinceTheLastTick) || daysSinceTheLastTick < 0d ||
                daysSinceTheLastTick > DaysARunTellsTheTickFor) return float.NaN;
            double gone = daysSinceTheLastTick - Math.Floor(daysSinceTheLastTick);
            float next = (float)(1d - gone);
            return next > 0f && next <= 1f ? next : 1f;
        }

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

        public static float OnTheCurve(double came)
        {
            if (double.IsNaN(came)) return 0f;
            if (came >= 1d) return came >= 2d ? 0f : (float)(2d - came);
            return came <= LeastOfAMoveThatCounts ? LeastOfAMoveThatCounts : (float)came;
        }

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

        public const long EnoughForecastWorth = 3000L;

        public const float TrustBeforeAnyCheck = 0.5f;

        public static float TrustInTheForecast(long weighed, float cameTrue)
        {
            if (weighed <= 0L || cameTrue < 0f || float.IsNaN(cameTrue) || float.IsInfinity(cameTrue))
                return TrustBeforeAnyCheck;
            float earned = cameTrue > 1f ? 1f : cameTrue;
            float weight = (float)(weighed / ((double)weighed + EnoughForecastWorth));
            float trust = TrustBeforeAnyCheck + (earned - TrustBeforeAnyCheck) * weight;
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

        public static float HowCloseToThePromise(float held) =>
            float.IsNaN(held) || float.IsInfinity(held) || held <= 0f ? 0f : OnTheCurve(held);

        public const float GraceDays = 1f;

        public static bool WorthScoring(float saidWithinDays, float daysSince) =>
            daysSince <= (saidWithinDays < 0f ? 0f : saidWithinDays) + GraceDays;

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

        public const int UnitBands = 4;

        public const float APromiseFallsShortBelow = 0.8f;

        public static int UnitBandOf(int units) => units <= 1 ? 0 : units <= 4 ? 1 : units <= 19 ? 2 : 3;

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

        public const int GameClockWidth = 20;

        public static readonly string NoGameClock = "no campaign".PadRight(GameClockWidth);

        public static string GameClock(int day, float hourInDay, bool night)
        {
            float hour = Finite(hourInDay, 0f);
            int minutes = hour <= 0f ? 0 : (int)Math.Min(hour * 60f, 24f * 60f - 1f);
            string clock = "day " + day.ToString(CultureInfo.InvariantCulture) + " " +
                           (minutes / 60).ToString("00", CultureInfo.InvariantCulture) + ":" +
                           (minutes % 60).ToString("00", CultureInfo.InvariantCulture) + (night ? " night" : "");
            return clock.PadRight(GameClockWidth);
        }

        public static float EtaDays(float distance, float speed)
        {
            if (distance <= 0f) return 0f;
            float pace = Finite(speed, WalkingPace);
            if (pace <= StandingStill) pace = WalkingPace;
            return Finite(distance / pace * HoursAPartyTakesToCoverItsSpeed / 24f, FurthestThereIs);
        }

        public static int Budget(int gold, int goldReserve, int maxSpendPerVisit,
                                 int spentThisVisit)
        {
            int left = gold - goldReserve;
            return maxSpendPerVisit > 0
                ? Math.Min(left, maxSpendPerVisit - spentThisVisit)
                : left;
        }

        public static float[] ShiftsInPairOrder(float[] supply, float[] demand,
                                                IList<(int kind, int substitute, float factor)> pairs)
        {
            var shifts = new float[pairs?.Count ?? 0];
            if (supply == null || demand == null || pairs == null) return shifts;
            int n = Math.Min(supply.Length, demand.Length);
            for (int p = 0; p < pairs.Count; p++)
            {
                var (one, other, factor) = pairs[p];
                if (one < 0 || one >= n || other < 0 || other >= n) continue;
                float was = demand[one];
                (demand[one], demand[other]) = DemandShiftedToASubstitute(supply[one], demand[one], supply[other],
                                                                          demand[other], factor);
                shifts[p] = Finite(demand[one] - was, 0f);
            }
            return shifts;
        }

        public const int DiceHoursFollowed = 240;

        public static double[] HoursTheDiceTake(float chanceAnHour)
        {
            var odds = new double[DiceHoursFollowed + 1];
            if (!(chanceAnHour > 0f) || chanceAnHour > 1f) return odds;
            double left = 1d;
            for (int k = 1; k <= DiceHoursFollowed; k++)
            {
                odds[k] = left * chanceAnHour;
                left *= 1d - chanceAnHour;
            }
            return odds;
        }

        public static double[] HoursAllTheDiceTake(IList<float> chances)
        {
            var odds = new double[DiceHoursFollowed + 1];
            odds[0] = 1d;
            for (int r = 0; chances != null && r < chances.Count; r++)
            {
                double[] one = HoursTheDiceTake(chances[r]);
                var both = new double[DiceHoursFollowed + 1];
                for (int a = 0; a <= DiceHoursFollowed; a++)
                {
                    if (odds[a] <= 0d) continue;
                    for (int b = 1; a + b <= DiceHoursFollowed; b++) both[a + b] += odds[a] * one[b];
                }
                odds = both;
            }
            return odds;
        }

        public static float HoursTheDiceAreSaidToTake(IList<float> chances)
        {
            double said = 0d;
            for (int r = 0; chances != null && r < chances.Count; r++)
                if (chances[r] > 0f) said += 1d / chances[r];
            return (float)said;
        }

        public struct DiceAlone
        {
            public float Median;
            public float Early;
            public float Low;
            public float High;
        }

        public static DiceAlone WhatTheDiceAloneGive(IList<float[]> trips, float lateAtMostHours)
        {
            var spread = new List<(float off, double odds)>();
            int counted = 0;
            for (int t = 0; trips != null && t < trips.Count; t++)
            {
                if (trips[t] == null || trips[t].Length == 0) continue;
                double[] odds = HoursAllTheDiceTake(trips[t]);
                float said = HoursTheDiceAreSaidToTake(trips[t]);
                double kept = 0d;
                for (int h = 0; h < odds.Length; h++) if (odds[h] > 0d && h - said <= lateAtMostHours) kept += odds[h];
                if (!(kept > 0d)) continue;
                counted++;
                for (int h = 0; h < odds.Length; h++)
                    if (odds[h] > 0d && h - said <= lateAtMostHours) spread.Add((h - said, odds[h] / kept));
            }
            var none = new DiceAlone { Median = float.NaN, Early = float.NaN, Low = float.NaN, High = float.NaN };
            if (counted == 0) return none;
            spread.Sort((x, y) => x.off.CompareTo(y.off));
            float At(double share)
            {
                double run = 0d, all = counted;
                foreach (var one in spread)
                {
                    run += one.odds / all;
                    if (run >= share) return one.off;
                }
                return spread[spread.Count - 1].off;
            }
            double early = 0d;
            foreach (var one in spread) if (one.off < 0f) early += one.odds / counted;
            return new DiceAlone { Median = At(0.5d), Early = (float)early, Low = At(0.1d), High = At(0.9d) };
        }

        public static bool CameSoonerThanTheDiceAllow(float offHours, float diceSaidHours) =>
            !float.IsNaN(offHours) && diceSaidHours > 0f && offHours < -diceSaidHours;

        public enum YourSaleGroup { NotAfterYou, NotCounted, WithinItsLanding, Later }

        public const double HoursYourSaleTakesToLandInFull = HorizonStep * 24d;

        public static YourSaleGroup GroupedByYourSale(bool counted, double youArrivedHours, double theyArrivedHours)
        {
            if (double.IsNaN(theyArrivedHours) || double.IsNaN(youArrivedHours) || theyArrivedHours <= youArrivedHours)
                return YourSaleGroup.NotAfterYou;
            if (!counted) return YourSaleGroup.NotCounted;
            return theyArrivedHours - youArrivedHours <= HoursYourSaleTakesToLandInFull
                ? YourSaleGroup.WithinItsLanding
                : YourSaleGroup.Later;
        }

        public const float ShortestRideLearned = 0.5f;

        public const float LongestRideLearned = 2f;

        public const int RidesLearnedPerWay = 3;

        public const int RideRatiosKept = 50;

        public const int LeastRidesForARatio = 5;

        public static bool ARideIsLearned(float tookDays, float flatDays) =>
            tookDays > 0f && flatDays > 0f && !float.IsInfinity(tookDays) && !float.IsInfinity(flatDays) &&
            tookDays >= flatDays * ShortestRideLearned && tookDays <= flatDays * LongestRideLearned;

        public static void KeepTheLast(List<float> kept, float one, int most)
        {
            if (kept == null || float.IsNaN(one)) return;
            kept.Add(one);
            while (kept.Count > most) kept.RemoveAt(0);
        }

        public static float MedianOf(IList<float> values)
        {
            if (values == null || values.Count == 0) return float.NaN;
            var sorted = new List<float>(values);
            sorted.Sort();
            return Quantile(sorted, 0.5f);
        }

        public static float TheRideToCount(IList<float> learned, float flatDays, IList<float> ratios)
        {
            if (learned != null && learned.Count > 0) return MedianOf(learned);
            if (ratios != null && ratios.Count >= LeastRidesForARatio && flatDays >= 0f)
                return Finite(flatDays * MedianOf(ratios), flatDays);
            return flatDays;
        }

        public static float WhatIsLeftOfARide(float rideDays, float leftToGo, float whole)
        {
            if (!(rideDays >= 0f) || !(whole > 0f) || float.IsNaN(leftToGo)) return float.NaN;
            float share = leftToGo / whole;
            return rideDays * (share < 0f ? 0f : share > 1f ? 1f : share);
        }

        public static (double perValue, int value) TheGamesPickWeighed(IList<(int value, float penalty)> items)
        {
            double weights = 0d, perValue = 0d, value = 0d;
            for (int i = 0; items != null && i < items.Count; i++)
            {
                var (each, penalty) = items[i];
                if (each <= 0 || float.IsNaN(penalty) || penalty <= -1f) continue;
                double weight = 1d / (Math.Max(100, each) + 100d);
                weights += weight;
                perValue += weight * each / (1d + penalty);
                value += weight * each;
            }
            if (!(weights > 0d)) return (0d, 0);
            return (perValue / weights, (int)Math.Round(value / weights, MidpointRounding.AwayFromZero));
        }

        public static int ATownPaysForAMake(double perValue, float factor)
        {
            if (!(perValue > 0d) || !(factor > 0f) || float.IsInfinity(factor)) return 0;
            double paid = Math.Floor(perValue * factor);
            return paid >= int.MaxValue ? int.MaxValue : Math.Max(1, (int)paid);
        }

        public sealed class Picks
        {
            public double[] PerValue;
            public double[] Below;
            public float Factor;
        }

        public static (double[] perValue, double[] below) TheGamesPicks(IList<(int value, float penalty)> items)
        {
            var kept = new List<(double perValue, double weight)>();
            double weights = 0d;
            for (int i = 0; items != null && i < items.Count; i++)
            {
                var (each, penalty) = items[i];
                if (each <= 0 || float.IsNaN(penalty) || penalty <= -1f) continue;
                double weight = 1d / (Math.Max(100, each) + 100d);
                kept.Add((each / (1d + penalty), weight));
                weights += weight;
            }
            kept.Sort((x, y) => x.perValue.CompareTo(y.perValue));
            var perValue = new double[kept.Count];
            var below = new double[kept.Count + 1];
            for (int i = 0; i < kept.Count; i++)
            {
                perValue[i] = kept[i].perValue;
                below[i + 1] = below[i] + (weights > 0d ? kept[i].weight / weights : 0d);
            }
            if (kept.Count > 0) below[kept.Count] = 1d;
            return (perValue, below);
        }

        public static double ShareOfPicksClearing(Picks picks, double need)
        {
            if (picks?.PerValue == null || picks.Below == null || picks.PerValue.Length == 0 ||
                picks.Below.Length != picks.PerValue.Length + 1 || double.IsNaN(need))
                return 0d;
            int lo = 0, hi = picks.PerValue.Length;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (ATownPaysForAMake(picks.PerValue[mid], picks.Factor) > need) hi = mid;
                else lo = mid + 1;
            }
            double share = 1d - picks.Below[lo];
            return share < 0d ? 0d : share > 1d ? 1d : share;
        }

        public const float ARunsCreditStartsAt = 0.5f;

        public static float HoursTheWorkshopReadingWasOff(float tickIn)
        {
            if (float.IsNaN(tickIn) || tickIn < 0f || tickIn > 1f) return float.NaN;
            float hours = tickIn * 24f;
            return hours <= 12f ? hours : hours - 24f;
        }

        public static bool MovedPastTwiceWhatWasSaid(int said, int moved) =>
            said != 0 && (double)moved / said > 2d;

        public static float UsedAgainstSaid(int said, int used)
        {
            if (said <= 0 && used <= 0) return float.NaN;
            if (said <= 0) return float.PositiveInfinity;
            return Finite((float)used / said, float.NaN);
        }
    }
}
