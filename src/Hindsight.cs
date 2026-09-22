using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TradeLord
{
    internal static class Hindsight
    {
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

        private static readonly Keeps<Said> _said = new Keeps<Said>();

        private static readonly Keeps<Promised> _promised = new Keeps<Promised>();

        private static readonly BandTally _bands = new BandTally();

        internal static bool Writing => Options.Current.ExtendedDebugLogging;

        internal static bool On => Forecast.On;

        internal static void Forget()
        {
            _said.Forget();
            _promised.Forget();
            _bands.Forget();
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
            if (!_promised.Holds(site.StringId, route.Item.StringId) &&
                _promised.Full && !RoomForOneMore())
            {
                Log.Repeatable("promise check", "full",
                               "promise check is holding the " + Keeps<Promised>.Most + " promises it keeps at " +
                               "once, so newer ones are passed over until a market it holds a promise for is " +
                               "walked into");
                return;
            }
            _promised.Put(site.StringId, route.Item.StringId, new Promised
            {
                Item = route.Item,
                AtHours = (float)CampaignTime.Now.ToHours,
                WithinDays = route.TravelDays,
                SellPrice = route.SellPrice,
                Units = route.Quantity,
                Confidence = route.Confidence
            });
        }

        private static bool RoomForOneMore()
        {
            float now = (float)CampaignTime.Now.ToHours;
            return _promised.Prune(
                one => !Scoring.TooOldToSay(one.WithinDays, one.AtHours, now, out _));
        }

        private static void Kept(Settlement site)
        {
            Dictionary<string, Promised> here = _promised.TakeAt(site.StringId);
            if (here == null) return;
            SettlementComponent market = site.SettlementComponent;
            if (market == null) return;
            float now = (float)CampaignTime.Now.ToHours;
            var lines = new List<string>();
            int scored = 0, stale = 0, unpriced = 0;
            float heldTotal = 0f;
            foreach (Promised said in here.Values)
            {
                if (said.Item == null) continue;
                if (Scoring.TooOldToSay(said.WithinDays, said.AtHours, now, out float since))
                {
                    stale++;
                    continue;
                }
                int found = Priced.At(market, said.Item, MobileParty.MainParty, true);
                Holding holding = Scoring.Weigh(said.SellPrice, found, out float held);
                if (holding == Holding.NoPrice)
                {
                    unpriced++;
                    continue;
                }
                if (holding != Holding.Scored) continue;
                scored++;
                heldTotal += held;
                _bands.Add(said.Confidence, held);
                LedgerBehavior.Instance?.KeepPromiseScore(held);
                lines.Add("  " + Named(said.Item) + ": the panel promised " + said.SellPrice +
                          " a unit for " + said.Units + " unit(s) within " + Figure(said.WithinDays) +
                          " day(s) at Conf " + Share(said.Confidence) + "; you walked in " + Figure(since) +
                          " day(s) later and it pays " + found + ", " + Share(held) + " of what it promised");
            }
            if (scored > 0)
                LedgerBehavior.Instance?.KeepArrival(site.StringId, TradeMath.MeanOf(heldTotal, scored));
            if (!Writing || scored == 0) return;
            lines.Insert(0, "promise check at " + site.Name + ", " + scored + " promise(s) scored" +
                      (stale == 0 ? "" : ", " + stale + " passed over as too old to say anything") +
                      (unpriced == 0 ? "" : ", " + unpriced + " the market would put no price on"));
            lines.Add("  here: the price held at " + Share(TradeMath.MeanOf(heldTotal, scored)) +
                      " of what the panel promised");
            for (int band = TradeMath.Bands - 1; band >= 0; band--)
            {
                if (_bands.Scored(band) == 0) continue;
                lines.Add("  " + Scoring.Banded(band) + ": held at " +
                          Share(_bands.Held(band)) + " of promise over " +
                          _bands.Scored(band) + " price(s) checked this session");
            }
            if (LedgerBehavior.Instance != null &&
                LedgerBehavior.Instance.PromiseScore(out int kept, out float overall))
                lines.Add("  over this campaign: the price has held at " + Share(overall) +
                          " of promise over " + kept + " price(s) checked");
            if (LedgerBehavior.Instance != null &&
                LedgerBehavior.Instance.PromiseScoreAt(site.StringId, out int walkIns, out float hereOverall))
                lines.Add("  at " + site.Name + ": the price has held at " + Share(hereOverall) +
                          " of promise over " + walkIns + " walk-in(s) here, which is what lowers " +
                          "the score of a route selling here");
            Log.WriteMany(lines);
        }

        private static void Noted(Settlement site, ItemObject item, float withinDays)
        {
            if (site == null) return;
            int stockSaid = Forecast.UnitsLanding(site, item, withinDays);
            int worthSaid = Forecast.WorthShift(site, item, withinDays);
            if (stockSaid == 0 && worthSaid == 0) return;
            if (!_said.Holds(site.StringId, item.StringId) && _said.Full)
            {
                Log.Repeatable("forecast check", "full",
                               "forecast check is holding the " + Keeps<Said>.Most + " figures it keeps at once, " +
                               "so newer ones are passed over until a market it has a figure for is walked into");
                return;
            }
            _said.Put(site.StringId, item.StringId, new Said
            {
                Item = item,
                AtHours = (float)CampaignTime.Now.ToHours,
                WithinDays = withinDays,
                StockThen = LedgerBehavior.StockOf(site, item),
                StockSaid = stockSaid,
                WorthThen = WorthOnTheShelf(site, item),
                WorthSaid = worthSaid
            });
        }

        private static void Written(Settlement site)
        {
            Dictionary<string, Said> here = _said.TakeAt(site.StringId);
            if (here == null) return;
            float now = (float)CampaignTime.Now.ToHours;
            var lines = new List<string>();
            int scored = 0, stale = 0, landingMiss = 0, shared = 0;
            float shareTotal = 0f;
            foreach (Said kept in here.Values)
            {
                if (kept.Item == null) continue;
                if (Scoring.TooOldToSay(kept.WithinDays, kept.AtHours, now, out float since))
                {
                    stale++;
                    continue;
                }
                scored++;
                Outcome how = Scoring.Weigh(kept.StockSaid, kept.StockThen,
                                            LedgerBehavior.StockOf(site, kept.Item),
                                            kept.WorthSaid, kept.WorthThen,
                                            WorthOnTheShelf(site, kept.Item));
                landingMiss += Math.Abs(how.LandingOff);
                string line = "  " + Named(kept.Item) + ": said " + kept.StockSaid + " unit(s) of it would land within " +
                              Figure(kept.WithinDays) + " day(s) and " + Landing(how.Landed) + ", " +
                              Counted(how.LandingOff) +
                              "; you walked in " + Figure(since) +
                              " day(s) after it said so";
                if (!how.WorthKept)
                {
                    lines.Add(line + "; no worth is kept for a kind of good here, which only a town does");
                    continue;
                }
                if (how.Share != TradeMath.NoShareToGive)
                {
                    shared++;
                    shareTotal += how.Share;
                    LedgerBehavior.Instance?.KeepForecastScore(TradeMath.MissThatCounts(how.Share));
                }
                lines.Add(line + "; said every good of that kind heading there was worth " +
                          kept.WorthSaid + " denars in all and " + Moving(how.Moved) + " denars, " +
                          Counted(how.WorthOff) + Shared(how.Share));
            }
            if (!Writing || scored == 0) return;
            lines.Insert(0, "forecast check at " + site.Name + ", " + scored + " good(s) it had a figure for" +
                      (stale == 0 ? "" : ", " + stale + " passed over as too old to say anything") + ":");
            lines.Add("  in all: the landing figure was off by " +
                      Figure(TradeMath.MeanOf(landingMiss, scored)) + " unit(s) a good" +
                      (shared == 0
                          ? ", and no worth figure could be held to anything here"
                          : ", the worth figure by " + Share(TradeMath.MeanOf(shareTotal, shared)) +
                            " of what it said would move, over " + shared + " good(s)"));
            if (LedgerBehavior.Instance != null &&
                LedgerBehavior.Instance.ForecastScore(out int figures, out float missed))
                lines.Add("  over this campaign: the worth figure has been off by " + Share(missed) +
                          " over " + figures + " figure(s) checked, so what is on its way is counted at " +
                          Share(TradeMath.TrustInTheForecast(figures, missed)) + " of what it says");
            Log.WriteMany(lines);
        }

        private static int WorthOnTheShelf(Settlement site, ItemObject item)
        {
            Town town = site != null && site.IsTown ? site.Town : null;
            if (town == null || item == null || item.ItemCategory == null) return Scoring.NoWorth;
            return Guard.Read("Hindsight.Worth", town,
                              where => where.MarketData.GetCategoryData(item.ItemCategory).InStoreValue,
                              Scoring.NoWorth);
        }

        private static string Named(ItemObject item) =>
            item.Name == null ? item.StringId : item.Name.ToString();

        private static string Counted(int off) => Scoring.Counted(off);

        private static string Landing(int landed) => Scoring.Landing(landed);

        private static string Moving(int moved) => Scoring.Moving(moved);

        private static string Shared(float share) => Scoring.Shared(share);

        private static string Share(float share) => Scoring.Share(share);

        private static string Figure(float number) => Scoring.Figure(number);
    }
}
