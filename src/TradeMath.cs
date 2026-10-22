using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;

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

        public static bool PaysMore(long total, int first, float days, long otherTotal, int otherFirst, float otherDays) =>
            total != otherTotal ? total > otherTotal
            : first != otherFirst ? first > otherFirst
            : days < otherDays;

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
                                                     bool beaten, float bestKey, float safety, float margin)
        {
            if (landed == 0 || quotedSell <= 0 || stocked <= 0 || !(safety >= 0f)) return false;
            int upper = MostAForecastCanReach(quotedSell);
            if (!BuyAcceptable(openingBuy, Realizable(upper, safety), margin)) return true;
            return beaten && upper >= openingBuy && PerDay((float)(upper - openingBuy) * stocked, days) <= bestKey;
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

        public const float APurseIsTrustedInFullAtThisTimesTheSale = 2f;

        public static float TrustInAPurse(long sale, int purse)
        {
            if (sale <= 0L || purse <= 0) return 1f;
            double cover = purse / (APurseIsTrustedInFullAtThisTimesTheSale * (double)sale);
            return cover >= 1d ? 1f : (float)cover;
        }

        public static float RateUntrusted(float rate, float trust) =>
            trust > 0f && trust < 1f && !float.IsNaN(rate) ? rate / trust : rate;

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

        public static float SpeedByDay(float now, float atNight, float byDay)
        {
            if (!(now > 0f) || !(atNight > 0f) || !(byDay > atNight)) return now;
            float day = Finite(now * (byDay / atNight), now);
            return day > now ? day : now;
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
            public float FairWinds;
            public bool Even;

            public static Pace AllAt(float land, float sea) => new Pace { Base = land, Sea = sea, Even = true };

            public Pace After(double hours)
            {
                if (hours == 0d || double.IsNaN(hours) || !(SlowHours > 0d)) return this;
                var later = (Pace)MemberwiseClone();
                later.SlowHours = SlowHours > hours ? SlowHours - hours : 0d;
                return later;
            }

            public float AtSea(float push, float fair)
            {
                float sea = Finite(Sea, 0f);
                float lift = Finite(push, 0f) + Finite(fair, 0f) * Finite(FairWinds, 0f);
                if (!(lift > 0f)) return sea;
                float most = sea > ASailIsNeverFasterThan ? sea : ASailIsNeverFasterThan;
                float speed = sea * (1f + lift);
                return speed > most ? most : Finite(speed, sea);
            }

            public float On(int ground, bool dark, bool snow, bool wet, bool slow)
            {
                if (ground == GroundSea) return Finite(Sea, 0f);
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

            internal void Count(double hours, bool dark, bool wet, bool snow, bool slow)
            {
                if (!(hours > 0d)) return;
                Hours += hours;
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
            float s = strength < 1E-05f ? 1E-05f : strength > 1f ? 1f : strength;
            windX = x * s;
            windY = y * s;
        }

        public static float WindBehind(float bearingX, float bearingY, float windX, float windY, out bool astern)
        {
            astern = false;
            float strength = (float)Math.Sqrt(windX * windX + windY * windY);
            if (!(strength > 0f) || float.IsInfinity(strength)) return 0f;
            float angle = Math.Abs((float)Math.Atan2(-bearingX, bearingY) - (float)Math.Atan2(-windX, windY)) * 57.29578f;
            if (!(angle < AWindPushesWithin)) return 0f;
            astern = true;
            float mapped = (angle < 0f ? 0f : angle) * (0f - strength) / AWindPushesWithin + strength;
            float push = mapped * AWindPushesAtMostBy;
            return push < 0f ? 0f : push > AWindPushesAtMostBy ? AWindPushesAtMostBy : push;
        }

        public static void AddRunOnTheWinds(List<Run> road, List<float> push, List<float> fair, int ground, int face,
                                            int cell, float length, float lift, float astern)
        {
            if (road == null || push == null || fair == null || !(length > 0f) || float.IsInfinity(length)) return;
            bool sea = ground == GroundSea;
            float up = sea ? Finite(lift, 0f) : 0f;
            float behind = sea ? Finite(astern, 0f) : 0f;
            int last = road.Count - 1;
            if (last >= 0 && road[last].Ground == ground && road[last].Face == face && road[last].Cell == cell)
            {
                Run joined = road[last];
                float before = joined.Length;
                joined.Length += length;
                road[last] = joined;
                if (sea)
                {
                    double time = before / (1d + push[last]) + length / (1d + up);
                    push[last] = time > 0d ? (float)(joined.Length / time - 1d) : 0f;
                    fair[last] = (before * fair[last] + length * behind) / joined.Length;
                }
                return;
            }
            road.Add(new Run { Length = length, Ground = ground, Face = face, Cell = cell });
            push.Add(up);
            fair.Add(behind);
        }

        public static float DaysOnTheWinds(IList<Run> road, IList<float> push, IList<float> fair, Pace pace)
        {
            if (road == null || pace == null) return 0f;
            double hours = 0d;
            for (int i = 0; i < road.Count; i++)
            {
                float length = road[i].Length;
                if (float.IsNaN(length) || float.IsInfinity(length)) return FurthestThereIs;
                if (!(length > 0f)) continue;
                float speed = road[i].Ground == GroundSea
                    ? pace.AtSea(push != null && i < push.Count ? push[i] : 0f, fair != null && i < fair.Count ? fair[i] : 0f)
                    : Finite(pace.Base, 0f);
                if (!(speed > 0f)) return FurthestThereIs;
                hours += length / speed * HoursAPartyTakesToCoverItsSpeed;
            }
            float days = Finite((float)(hours / 24d), FurthestThereIs);
            return OutOfReach(days) ? FurthestThereIs : days;
        }

        public static float DaysAlong(IList<Run> road, Pace pace, ISky sky, double startClock, int sunrise, int sunset,
                                      bool freshAtStart, Tally tally, IList<float> push = null, IList<float> fair = null)
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
                double perHour = (push != null && road[i].Ground == GroundSea && i < push.Count
                                     ? pace.AtSea(push[i], fair != null && i < fair.Count ? fair[i] : 0f)
                                     : pace.On(road[i].Ground, dark, snow, wet, slow)) / (double)HoursAPartyTakesToCoverItsSpeed;
                if (!(perHour > 0d) || double.IsInfinity(perHour)) return FurthestThereIs;
                double next = Math.Min(turn, Math.Min(change, slowEnds));
                double done = t + left / perHour;
                if (done <= next)
                {
                    tally?.Count(done - t, dark, wet, snow, slow);
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
                    tally?.Count(next - t, dark, wet, snow, slow);
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
    }

    public static class RoadAtlas
    {
        public const int Format = 2;

        public const int MostRoads = 4000000;

        public const int MostRunsOnARoad = 1000000;

        public const int MostBarred = 4096;

        private const uint Mark = 0x41524c54;

        private const int Buffered = 1 << 16;

        public struct Key
        {
            public uint NavigationMesh;
            public uint Scene;
            public int Grid;
            public float CellWide;
            public float CellHigh;
            public float Stride;
            public uint Way;
            public int SeaToLand;
            public int LandToSea;
            public int[] Barred;
            public int[] BarredAtSea;

            private static bool Same(int[] mine, int[] theirs)
            {
                mine = mine ?? new int[0];
                theirs = theirs ?? new int[0];
                if (mine.Length != theirs.Length) return false;
                for (int i = 0; i < mine.Length; i++)
                    if (mine[i] != theirs[i]) return false;
                return true;
            }

            public bool SameAs(Key other)
            {
                if (NavigationMesh != other.NavigationMesh || Scene != other.Scene || Grid != other.Grid ||
                    !CellWide.Equals(other.CellWide) || !CellHigh.Equals(other.CellHigh) ||
                    !Stride.Equals(other.Stride) || Way != other.Way || SeaToLand != other.SeaToLand ||
                    LandToSea != other.LandToSea)
                    return false;
                return Same(Barred, other.Barred) && Same(BarredAtSea, other.BarredAtSea);
            }
        }

        public sealed class Road
        {
            public string From;
            public string To;
            public float FromX, FromY, ToX, ToY;
            public bool Walked;
            public bool Sea;
            public float Length;
            public List<TradeMath.Run> Runs;
            public float[] Push;
            public float[] Fair;

            public bool Between(float fromX, float fromY, float toX, float toY) =>
                FromX.Equals(fromX) && FromY.Equals(fromY) && ToX.Equals(toX) && ToY.Equals(toY);
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
        private struct Bits
        {
            [System.Runtime.InteropServices.FieldOffset(0)] internal float Single;
            [System.Runtime.InteropServices.FieldOffset(0)] internal uint Whole;
        }

        private struct Tally
        {
            internal ulong Sum;

            internal void Add(uint value)
            {
                ulong sum = Sum == 0ul ? 14695981039346656037ul : Sum;
                sum = ((sum << 5) | (sum >> 59)) ^ value;
                Sum = unchecked(sum * 1099511628211ul);
            }

            internal void Add(int value) => Add(unchecked((uint)value));

            internal void Add(float value) => Add(new Bits { Single = value }.Whole);

            internal void Add(string value)
            {
                string text = value ?? "";
                Add(text.Length);
                for (int i = 0; i < text.Length; i++) Add((int)text[i]);
            }
        }

        public const int RoadsAreWalkedThisWay = 2;

        public static uint TheWayRoadsAreWalked(int mostPaths, float reached, float radius, float switchCost)
        {
            var tally = new Tally();
            tally.Add(RoadsAreWalkedThisWay);
            tally.Add(TradeMath.LeastStepOnARoad);
            tally.Add(TradeMath.MostProbesOnALeg);
            tally.Add(TradeMath.MostSplitsOfAStep);
            tally.Add(mostPaths);
            tally.Add(reached);
            tally.Add(radius);
            tally.Add(switchCost);
            tally.Add(TradeMath.WindAtLeast);
            tally.Add(TradeMath.WindAtMost);
            tally.Add(TradeMath.AWindPushesWithin);
            tally.Add(TradeMath.AWindPushesAtMostBy);
            return unchecked((uint)(tally.Sum ^ (tally.Sum >> 32)));
        }

        public static uint ZigZag(int n) => unchecked((uint)((n << 1) ^ (n >> 31)));

        public static int Unzig(uint z) => unchecked((int)(z >> 1) ^ -(int)(z & 1u));

        private static void WriteCount(BinaryWriter w, uint value)
        {
            while (value >= 0x80u)
            {
                w.Write((byte)(value | 0x80u));
                value >>= 7;
            }
            w.Write((byte)value);
        }

        private static uint ReadCount(BinaryReader r)
        {
            uint value = 0u;
            for (int shift = 0; shift < 35; shift += 7)
            {
                byte b = r.ReadByte();
                value |= (uint)(b & 0x7f) << shift;
                if ((b & 0x80) == 0) return value;
            }
            throw new InvalidDataException("a count runs past five bytes");
        }

        private static void Counted(ref Tally tally, Road road, bool walked, bool sea, bool winds)
        {
            tally.Add(road.From);
            tally.Add(road.To);
            tally.Add(road.FromX);
            tally.Add(road.FromY);
            tally.Add(road.ToX);
            tally.Add(road.ToY);
            tally.Add(walked ? 1 : 0);
            tally.Add(sea ? 1 : 0);
            tally.Add(winds ? 1 : 0);
            if (!walked) return;
            tally.Add(road.Length);
            tally.Add(road.Runs.Count);
            for (int k = 0; k < road.Runs.Count; k++)
            {
                tally.Add(road.Runs[k].Ground);
                tally.Add(road.Runs[k].Face);
                tally.Add(road.Runs[k].Cell);
                tally.Add(road.Runs[k].Length);
                if (!winds || road.Runs[k].Ground != TradeMath.GroundSea) continue;
                tally.Add(road.Push[k]);
                tally.Add(road.Fair[k]);
            }
        }

        public static void Write(Stream to, Key key, IList<Road> roads)
        {
            using (var zip = new GZipStream(to, CompressionLevel.Fastest, true))
            using (var buffered = new BufferedStream(zip, Buffered))
            using (var w = new BinaryWriter(buffered, Encoding.UTF8, true))
            {
                w.Write(Mark);
                w.Write(Format);
                w.Write(key.NavigationMesh);
                w.Write(key.Scene);
                w.Write(key.Grid);
                w.Write(key.CellWide);
                w.Write(key.CellHigh);
                w.Write(key.Stride);
                w.Write(key.Way);
                w.Write(key.SeaToLand);
                w.Write(key.LandToSea);
                int[] barred = key.Barred ?? new int[0];
                w.Write(barred.Length);
                for (int i = 0; i < barred.Length; i++) w.Write(barred[i]);
                int[] barredAtSea = key.BarredAtSea ?? new int[0];
                w.Write(barredAtSea.Length);
                for (int i = 0; i < barredAtSea.Length; i++) w.Write(barredAtSea[i]);
                int count = roads?.Count ?? 0;
                w.Write(count);
                var tally = new Tally();
                for (int i = 0; i < count; i++)
                {
                    Road road = roads[i];
                    w.Write(road.From ?? "");
                    w.Write(road.To ?? "");
                    w.Write(road.FromX);
                    w.Write(road.FromY);
                    w.Write(road.ToX);
                    w.Write(road.ToY);
                    bool walked = road.Walked && road.Runs != null;
                    bool sea = road.Sea;
                    bool winds = walked && sea && road.Push != null && road.Fair != null &&
                                 road.Push.Length == road.Runs.Count && road.Fair.Length == road.Runs.Count;
                    w.Write(walked);
                    w.Write(sea);
                    w.Write(winds);
                    Counted(ref tally, road, walked, sea, winds);
                    if (!walked) continue;
                    w.Write(road.Length);
                    WriteCount(w, (uint)road.Runs.Count);
                    int face = 0, cell = 0;
                    for (int k = 0; k < road.Runs.Count; k++)
                    {
                        TradeMath.Run run = road.Runs[k];
                        WriteCount(w, ZigZag(run.Ground));
                        WriteCount(w, ZigZag(unchecked(run.Face - face)));
                        WriteCount(w, ZigZag(unchecked(run.Cell - cell)));
                        w.Write(run.Length);
                        if (winds && run.Ground == TradeMath.GroundSea)
                        {
                            w.Write(road.Push[k]);
                            w.Write(road.Fair[k]);
                        }
                        face = run.Face;
                        cell = run.Cell;
                    }
                }
                w.Write(tally.Sum);
                w.Write(Mark);
            }
        }

        public static bool Read(Stream from, Key expected, out List<Road> roads, out string why)
        {
            roads = null;
            try
            {
                using (var zip = new GZipStream(from, CompressionMode.Decompress, true))
                using (var buffered = new BufferedStream(zip, Buffered))
                using (var r = new BinaryReader(buffered, Encoding.UTF8, true))
                {
                    if (r.ReadUInt32() != Mark)
                    {
                        why = "it is not a road atlas";
                        return false;
                    }
                    int format = r.ReadInt32();
                    if (format != Format)
                    {
                        why = "it was written in another format (" + format + ")";
                        return false;
                    }
                    var key = new Key
                    {
                        NavigationMesh = r.ReadUInt32(), Scene = r.ReadUInt32(), Grid = r.ReadInt32(),
                        CellWide = r.ReadSingle(), CellHigh = r.ReadSingle(), Stride = r.ReadSingle(),
                        Way = r.ReadUInt32(), SeaToLand = r.ReadInt32(), LandToSea = r.ReadInt32()
                    };
                    int barred = r.ReadInt32();
                    if (barred < 0 || barred > MostBarred)
                    {
                        why = "it is damaged";
                        return false;
                    }
                    key.Barred = new int[barred];
                    for (int i = 0; i < barred; i++) key.Barred[i] = r.ReadInt32();
                    int barredAtSea = r.ReadInt32();
                    if (barredAtSea < 0 || barredAtSea > MostBarred)
                    {
                        why = "it is damaged";
                        return false;
                    }
                    key.BarredAtSea = new int[barredAtSea];
                    for (int i = 0; i < barredAtSea; i++) key.BarredAtSea[i] = r.ReadInt32();
                    if (!key.SameAs(expected))
                    {
                        why = "it was made for another map, or for roads walked another way";
                        return false;
                    }
                    int count = r.ReadInt32();
                    if (count < 0 || count > MostRoads)
                    {
                        why = "it is damaged";
                        return false;
                    }
                    var read = new List<Road>(count);
                    var tally = new Tally();
                    for (int i = 0; i < count; i++)
                    {
                        var road = new Road
                        {
                            From = r.ReadString(), To = r.ReadString(),
                            FromX = r.ReadSingle(), FromY = r.ReadSingle(), ToX = r.ReadSingle(), ToY = r.ReadSingle(),
                            Walked = r.ReadBoolean(), Sea = r.ReadBoolean()
                        };
                        bool winds = r.ReadBoolean();
                        if (road.Walked)
                        {
                            road.Length = r.ReadSingle();
                            uint runs = ReadCount(r);
                            if (runs > MostRunsOnARoad)
                            {
                                why = "it is damaged";
                                return false;
                            }
                            road.Runs = new List<TradeMath.Run>((int)runs);
                            if (winds)
                            {
                                road.Push = new float[runs];
                                road.Fair = new float[runs];
                            }
                            int face = 0, cell = 0;
                            for (uint k = 0; k < runs; k++)
                            {
                                int ground = Unzig(ReadCount(r));
                                face = unchecked(face + Unzig(ReadCount(r)));
                                cell = unchecked(cell + Unzig(ReadCount(r)));
                                road.Runs.Add(new TradeMath.Run
                                {
                                    Ground = ground, Face = face, Cell = cell, Length = r.ReadSingle()
                                });
                                if (winds && ground == TradeMath.GroundSea)
                                {
                                    road.Push[k] = r.ReadSingle();
                                    road.Fair[k] = r.ReadSingle();
                                }
                            }
                        }
                        else if (winds)
                        {
                            why = "it is damaged";
                            return false;
                        }
                        Counted(ref tally, road, road.Walked, road.Sea, winds);
                        read.Add(road);
                    }
                    if (r.ReadUInt64() != tally.Sum || r.ReadUInt32() != Mark || buffered.ReadByte() != -1)
                    {
                        why = "it ends early or is damaged";
                        return false;
                    }
                    roads = read;
                    why = null;
                    return true;
                }
            }
            catch (Exception e)
            {
                why = "it could not be read (" + e.GetType().Name + ")";
                return false;
            }
        }
    }
}
