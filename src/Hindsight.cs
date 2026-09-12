using System;
using System.Collections.Generic;
using System.Globalization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TradeLord
{
    internal static class Hindsight
    {
        private const int NoWorth = -1;

        private const int Most = 600;

        private struct Said
        {
            internal ItemObject Item;
            internal float AtHours;
            internal float WithinDays;
            internal int StockThen;
            internal int StockSaid;
            internal int WorthThen;
            internal int WorthSaid;
        }

        private struct Promised
        {
            internal ItemObject Item;
            internal float AtHours;
            internal float WithinDays;
            internal int SellPrice;
            internal int Units;
            internal float Confidence;
        }

        private static readonly Dictionary<string, Dictionary<string, Said>> _said =
            new Dictionary<string, Dictionary<string, Said>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, Dictionary<string, Promised>> _promised =
            new Dictionary<string, Dictionary<string, Promised>>(StringComparer.Ordinal);

        private static readonly float[] _bandHeld = new float[TradeMath.Bands];

        private static readonly int[] _bandScored = new int[TradeMath.Bands];

        private static int _held;

        private static int _promises;

        internal static bool Writing => Options.Current.ForecastScore;

        internal static bool On => Writing && Forecast.On;

        internal static void Forget()
        {
            _said.Clear();
            _promised.Clear();
            _held = 0;
            _promises = 0;
            for (int i = 0; i < TradeMath.Bands; i++)
            {
                _bandHeld[i] = 0f;
                _bandScored[i] = 0;
            }
        }

        internal static void Note(TradeRoute route)
        {
            if (route == null || route.Item == null) return;
            Guard.Run("Hindsight.Promise", () => Promise(route));
            if (!On) return;
            Guard.Run("Hindsight.Note", () =>
            {
                Noted(route.From, route.Item, Travel.EstimateDaysFromParty(route.From));
                Noted(route.To, route.Item, route.TravelDays);
            });
        }

        internal static void Score(Settlement site)
        {
            if (site == null) return;
            Guard.Run("Hindsight.Kept", () => Kept(site));
            if (!On) return;
            Guard.Run("Hindsight.Score", () => Written(site));
        }

        private static void Promise(TradeRoute route)
        {
            Settlement site = route.To;
            if (site == null || route.SellPrice <= 0) return;
            if (!_promised.TryGetValue(site.StringId, out Dictionary<string, Promised> here))
            {
                here = new Dictionary<string, Promised>(StringComparer.Ordinal);
                _promised[site.StringId] = here;
            }
            if (!here.ContainsKey(route.Item.StringId))
            {
                if (_promises >= Most && !RoomForOneMore())
                {
                    Log.Repeatable("promise check", "full",
                                   "promise check is holding the " + Most + " promises it keeps at once, so newer " +
                                   "ones are passed over until a market it holds a promise for is walked into");
                    return;
                }
                _promises++;
            }
            here[route.Item.StringId] = new Promised
            {
                Item = route.Item,
                AtHours = (float)CampaignTime.Now.ToHours,
                WithinDays = route.TravelDays,
                SellPrice = route.SellPrice,
                Units = route.Quantity,
                Confidence = route.Confidence
            };
        }

        private static bool RoomForOneMore()
        {
            float now = (float)CampaignTime.Now.ToHours;
            var emptied = new List<string>();
            foreach (var site in _promised)
            {
                var past = new List<string>();
                foreach (var one in site.Value)
                    if (!TradeMath.WorthScoring(one.Value.WithinDays,
                                                TradeMath.DaysSince(one.Value.AtHours, now)))
                        past.Add(one.Key);
                for (int i = 0; i < past.Count; i++)
                {
                    site.Value.Remove(past[i]);
                    _promises--;
                }
                if (site.Value.Count == 0) emptied.Add(site.Key);
            }
            for (int i = 0; i < emptied.Count; i++) _promised.Remove(emptied[i]);
            if (_promises < 0) _promises = 0;
            return _promises < Most;
        }

        private static void Kept(Settlement site)
        {
            if (!_promised.TryGetValue(site.StringId, out Dictionary<string, Promised> here)) return;
            _promised.Remove(site.StringId);
            _promises -= here.Count;
            if (_promises < 0) _promises = 0;
            SettlementComponent market = site.SettlementComponent;
            if (market == null) return;
            float now = (float)CampaignTime.Now.ToHours;
            var lines = new List<string>();
            int scored = 0, stale = 0, unpriced = 0;
            float heldTotal = 0f;
            foreach (Promised said in here.Values)
            {
                if (said.Item == null) continue;
                float since = TradeMath.DaysSince(said.AtHours, now);
                if (!TradeMath.WorthScoring(said.WithinDays, since))
                {
                    stale++;
                    continue;
                }
                int found = Priced.At(market, said.Item, MobileParty.MainParty, true);
                if (found <= 0)
                {
                    unpriced++;
                    continue;
                }
                float held = TradeMath.HeldShare(said.SellPrice, found);
                if (held == TradeMath.NoShareToGive) continue;
                scored++;
                heldTotal += held;
                int band = TradeMath.BandOf(said.Confidence);
                _bandHeld[band] += held;
                _bandScored[band]++;
                LedgerBehavior.Instance?.KeepPromiseScore(held);
                lines.Add("  " + Named(said.Item) + ": the panel promised " + said.SellPrice +
                          " a unit for " + said.Units + " unit(s) within " + Figure(said.WithinDays) +
                          " day(s) at Conf " + Share(said.Confidence) + "; you walked in " + Figure(since) +
                          " day(s) later and it pays " + found + ", " + Share(held) + " of what it promised");
            }
            if (!Writing || scored == 0) return;
            Log.Write("promise check at " + site.Name + ", " + scored + " promise(s) scored" +
                      (stale == 0 ? "" : ", " + stale + " passed over as too old to say anything") +
                      (unpriced == 0 ? "" : ", " + unpriced + " the market would put no price on"));
            for (int i = 0; i < lines.Count; i++) Log.Write(lines[i]);
            Log.Write("  here: the price held at " + Share(TradeMath.MeanOf(heldTotal, scored)) +
                      " of what the panel promised");
            for (int band = TradeMath.Bands - 1; band >= 0; band--)
            {
                if (_bandScored[band] == 0) continue;
                Log.Write("  " + Banded(band) + ": held at " +
                          Share(TradeMath.MeanOf(_bandHeld[band], _bandScored[band])) + " of promise over " +
                          _bandScored[band] + " arrival(s) this session");
            }
            if (LedgerBehavior.Instance != null &&
                LedgerBehavior.Instance.PromiseScore(out int kept, out float overall))
                Log.Write("  over this campaign: the price has held at " + Share(overall) +
                          " of promise over " + kept + " arrival(s)");
        }

        private static string Banded(int band)
        {
            if (band == 0) return "Conf under 25%";
            if (band == 1) return "Conf 25% to 49%";
            return band == 2 ? "Conf 50% to 74%" : "Conf 75% and over";
        }

        private static void Noted(Settlement site, ItemObject item, float withinDays)
        {
            if (site == null) return;
            int stockSaid = Forecast.UnitsLanding(site, item, withinDays);
            int worthSaid = Forecast.WorthShift(site, item, withinDays);
            if (stockSaid == 0 && worthSaid == 0) return;
            if (!_said.TryGetValue(site.StringId, out Dictionary<string, Said> here))
            {
                here = new Dictionary<string, Said>(StringComparer.Ordinal);
                _said[site.StringId] = here;
            }
            if (!here.ContainsKey(item.StringId))
            {
                if (_held >= Most)
                {
                    Log.Repeatable("forecast check", "full",
                                   "forecast check is holding the " + Most + " figures it keeps at once, so newer " +
                                   "ones are passed over until a market it has a figure for is walked into");
                    return;
                }
                _held++;
            }
            here[item.StringId] = new Said
            {
                Item = item,
                AtHours = (float)CampaignTime.Now.ToHours,
                WithinDays = withinDays,
                StockThen = LedgerBehavior.StockOf(site, item),
                StockSaid = stockSaid,
                WorthThen = WorthOnTheShelf(site, item),
                WorthSaid = worthSaid
            };
        }

        private static void Written(Settlement site)
        {
            if (!_said.TryGetValue(site.StringId, out Dictionary<string, Said> here)) return;
            _said.Remove(site.StringId);
            _held -= here.Count;
            if (_held < 0) _held = 0;
            float now = (float)CampaignTime.Now.ToHours;
            var lines = new List<string>();
            int scored = 0, landingMiss = 0, shared = 0;
            float shareTotal = 0f;
            foreach (Said kept in here.Values)
            {
                if (kept.Item == null) continue;
                scored++;
                int landed = TradeMath.MissedBy(kept.StockThen, LedgerBehavior.StockOf(site, kept.Item));
                int landingOff = TradeMath.MissedBy(kept.StockSaid, landed);
                landingMiss += Math.Abs(landingOff);
                string line = "  " + Named(kept.Item) + ": said " + kept.StockSaid + " unit(s) would land within " +
                              Figure(kept.WithinDays) + " day(s) and " + landed + " did, " + Counted(landingOff) +
                              "; you walked in " + Figure(TradeMath.DaysSince(kept.AtHours, now)) +
                              " day(s) after it said so";
                int worthNow = WorthOnTheShelf(site, kept.Item);
                if (kept.WorthThen == NoWorth || worthNow == NoWorth)
                {
                    lines.Add(line + "; no worth is kept for a kind of good here, which only a town does");
                    continue;
                }
                int moved = TradeMath.MissedBy(kept.WorthThen, worthNow);
                float share = TradeMath.OffByShare(kept.WorthSaid, moved);
                if (share != TradeMath.NoShareToGive)
                {
                    shared++;
                    shareTotal += share;
                }
                lines.Add(line + "; said its kind would move " + kept.WorthSaid + " in worth and it moved " +
                          moved + ", " + Counted(TradeMath.MissedBy(kept.WorthSaid, moved)) + Shared(share));
            }
            if (scored == 0) return;
            Log.Write("forecast check at " + site.Name + ", " + scored + " good(s) it had a figure for:");
            for (int i = 0; i < lines.Count; i++) Log.Write(lines[i]);
            Log.Write("  in all: the landing figure was off by " +
                      Figure(TradeMath.MeanOf(landingMiss, scored)) + " unit(s) a good" +
                      (shared == 0
                          ? ", and no worth figure could be held to anything here"
                          : ", the worth figure by " + Share(TradeMath.MeanOf(shareTotal, shared)) +
                            " of what it said would move, over " + shared + " good(s)"));
        }

        private static int WorthOnTheShelf(Settlement site, ItemObject item)
        {
            Town town = site != null && site.IsTown ? site.Town : null;
            if (town == null || item == null || item.ItemCategory == null) return NoWorth;
            return Guard.Read("Hindsight.Worth", town,
                              where => where.MarketData.GetCategoryData(item.ItemCategory).InStoreValue,
                              NoWorth);
        }

        private static string Named(ItemObject item) =>
            item.Name == null ? item.StringId : item.Name.ToString();

        private static string Counted(int off) =>
            off == 0 ? "exactly what it said"
                     : (off > 0 ? off + " more than it said" : -off + " fewer than it said");

        private static string Shared(float share) =>
            share == TradeMath.NoShareToGive ? "" : ", " + Share(share) + " off";

        private static string Share(float share) =>
            (share * 100f).ToString("0", CultureInfo.InvariantCulture) + "%";

        private static string Figure(float number) =>
            number.ToString("0.0", CultureInfo.InvariantCulture);
    }
}
