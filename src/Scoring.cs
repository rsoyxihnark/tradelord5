using System;
using System.Collections.Generic;
using System.Globalization;

namespace TradeLord
{
    internal class Keeps<TRecord>
    {
        internal const int Most = 600;

        private readonly Dictionary<string, Dictionary<string, TRecord>> _by =
            new Dictionary<string, Dictionary<string, TRecord>>(StringComparer.Ordinal);

        private int _count;

        internal int Count => _count;

        internal bool Full => _count >= Most;

        internal void Forget()
        {
            _by.Clear();
            _count = 0;
        }

        internal bool Holds(string site, string what) =>
            site != null && what != null &&
            _by.TryGetValue(site, out Dictionary<string, TRecord> here) && here.ContainsKey(what);

        internal void Put(string site, string what, TRecord one)
        {
            if (!_by.TryGetValue(site, out Dictionary<string, TRecord> here))
            {
                here = new Dictionary<string, TRecord>(StringComparer.Ordinal);
                _by[site] = here;
            }
            if (!here.ContainsKey(what)) _count++;
            here[what] = one;
        }

        internal bool TryGet(string site, string what, out TRecord one)
        {
            one = default(TRecord);
            return site != null && what != null && _by.TryGetValue(site, out Dictionary<string, TRecord> here) &&
                   here.TryGetValue(what, out one);
        }

        internal Dictionary<string, TRecord> TakeAt(string site)
        {
            if (site == null || !_by.TryGetValue(site, out Dictionary<string, TRecord> here)) return null;
            _by.Remove(site);
            _count -= here.Count;
            if (_count < 0) _count = 0;
            return here;
        }

        internal bool Prune(Func<TRecord, bool> stillWorthKeeping)
        {
            var emptied = new List<string>();
            foreach (var site in _by)
            {
                var past = new List<string>();
                foreach (var one in site.Value)
                    if (!stillWorthKeeping(one.Value)) past.Add(one.Key);
                for (int i = 0; i < past.Count; i++)
                {
                    site.Value.Remove(past[i]);
                    _count--;
                }
                if (site.Value.Count == 0) emptied.Add(site.Key);
            }
            for (int i = 0; i < emptied.Count; i++) _by.Remove(emptied[i]);
            if (_count < 0) _count = 0;
            return !Full;
        }

        internal List<string> Sites() => new List<string>(_by.Keys);

        internal void Rework(string site, Func<TRecord, TRecord> how)
        {
            if (site == null || how == null || !_by.TryGetValue(site, out Dictionary<string, TRecord> here))
                return;
            foreach (string what in new List<string>(here.Keys)) here[what] = how(here[what]);
        }
    }

    internal struct Reading
    {
        internal string Item;
        internal string Town;
        internal float Day;
    }

    internal static class Kept
    {
        internal const int MostPricesKept = 2500;

        internal const int MostPromisesKept = 500;

        internal static List<Reading> OldestBeyond(List<Reading> held, int cap)
        {
            var dropped = new List<Reading>();
            if (held == null || cap <= 0 || held.Count <= cap) return dropped;
            held.Sort((x, y) => x.Day.CompareTo(y.Day));
            int over = held.Count - cap;
            for (int i = 0; i < over; i++) dropped.Add(held[i]);
            return dropped;
        }
    }

    internal class BandTally
    {
        private readonly float[] _held = new float[TradeMath.Bands];

        private readonly int[] _scored = new int[TradeMath.Bands];

        internal void Forget()
        {
            for (int i = 0; i < TradeMath.Bands; i++)
            {
                _held[i] = 0f;
                _scored[i] = 0;
            }
        }

        internal void Add(float confidence, float held)
        {
            int band = TradeMath.BandOf(confidence);
            _held[band] += held;
            _scored[band]++;
        }

        internal int Scored(int band) => _scored[band];

        internal float Held(int band) => TradeMath.MeanOf(_held[band], _scored[band]);
    }

    internal enum Holding
    {
        NoPrice, NothingToHold, Scored
    }

    internal struct Outcome
    {
        internal int Landed;
        internal int LandingOff;
        internal bool WorthKept;
        internal int Moved;
        internal int WorthOff;
        internal float Share;
    }

    internal static class Scoring
    {
        internal const int NoWorth = -1;

        internal static bool TooOldToSay(float withinDays, float atHours, float nowHours,
                                        out float since)
        {
            since = TradeMath.DaysSince(atHours, nowHours);
            return !TradeMath.WorthScoring(withinDays, since);
        }

        internal static bool TooSoonToSay(float withinDays, float since) =>
            TradeMath.TooSoonToJudge(withinDays, since);

        internal static bool StillToBeJudged(float withinDays, float atHours, float nowHours) =>
            !TooOldToSay(withinDays, atHours, nowHours, out _);

        internal static Holding Weigh(int promised, int found, out float held)
        {
            held = TradeMath.NoShareToGive;
            if (found <= 0) return Holding.NoPrice;
            held = TradeMath.HeldShare(promised, found);
            return held == TradeMath.NoShareToGive ? Holding.NothingToHold : Holding.Scored;
        }

        internal static Outcome Weigh(int stockSaid, int stockThen, int stockNow,
                                      int worthSaid, int worthThen, int worthNow,
                                      int stockYours = 0, int worthYours = 0)
        {
            Outcome how;
            how.Landed = TradeMath.WithoutYours(TradeMath.MissedBy(stockThen, stockNow), stockYours);
            how.LandingOff = TradeMath.MissedBy(stockSaid, how.Landed);
            how.WorthKept = worthThen != NoWorth && worthNow != NoWorth;
            how.Moved = how.WorthKept
                ? TradeMath.WithoutYours(TradeMath.MissedBy(worthThen, worthNow), worthYours)
                : 0;
            how.WorthOff = how.WorthKept ? TradeMath.MissedBy(worthSaid, how.Moved) : 0;
            how.Share = how.WorthKept ? TradeMath.OffByShare(worthSaid, how.Moved)
                                      : TradeMath.NoShareToGive;
            return how;
        }

        internal static string Banded(int band)
        {
            if (band == 0) return "Conf under 25%";
            if (band == 1) return "Conf 25% to 49%";
            return band == 2 ? "Conf 50% to 74%" : "Conf 75% and over";
        }

        internal static string Counted(int off) =>
            off == 0 ? "exactly what it said"
                     : (off > 0 ? off + " more than it said" : -off + " fewer than it said");

        internal static string Landing(int landed) =>
            landed < 0 ? -landed + " left instead" : landed + " did";

        internal static string Moving(int moved) =>
            moved < 0 ? -moved + " left instead" : "it moved " + moved;

        internal static string Yours(int yours, bool worth)
        {
            if (yours == 0) return "";
            long size = yours < 0 ? -(long)yours : yours;
            return " (not counting " + (worth ? "goods worth " + size + " denars" : "the " + size + " unit(s)") +
                   " your own trading " + (yours < 0 ? "took off" : "put on") + " the shelf)";
        }

        internal static (int stock, int worth) YoursAdded(int stockYours, int worthYours, bool sameGood,
                                                          bool sameKind, bool worthKept, int into, int value)
        {
            if (into == 0) return (stockYours, worthYours);
            return (sameGood ? TradeMath.AddedUp(stockYours, into) : stockYours,
                    worthKept && sameKind ? TradeMath.AddedUp(worthYours, TradeMath.YourOwnWorth(into, value))
                                          : worthYours);
        }

        internal static string Shared(float share) =>
            share == TradeMath.NoShareToGive ? "" : ", " + Share(share) + " off";

        internal static string Share(float share) =>
            (share * 100f).ToString("0", CultureInfo.InvariantCulture) + "%";

        internal static string Figure(float number) =>
            number.ToString("0.0", CultureInfo.InvariantCulture);
    }
}
