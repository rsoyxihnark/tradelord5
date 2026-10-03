using System;
using System.Collections.Generic;
using Helpers;
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
            internal int WorthThen;
            internal int[] StockAhead;
            internal int[] WorthAhead;
            internal int StockYours;
            internal int WorthYours;
        }

        private struct Promised
        {
            internal ItemObject Item;
            internal float AtHours;
            internal float WithinDays;
            internal int SellPrice;
            internal int Units;
            internal float Confidence;
            internal bool YourTradeMovedIt;
        }

        private static readonly Keeps<Said> _said = new Keeps<Said>();

        private static readonly Keeps<Promised> _promised = new Keeps<Promised>();

        private static readonly BandTally _bands = new BandTally();

        private static readonly Dictionary<string, Settlement> _nearestTown =
            new Dictionary<string, Settlement>(StringComparer.Ordinal);

        internal static bool Writing => Options.Current.ExtendedDebugLogging;

        internal static bool On => Forecast.On;

        internal static void Forget()
        {
            _said.Forget();
            _promised.Forget();
            _bands.Forget();
            _nearestTown.Clear();
        }

        internal static void Note(TradeRoute route)
        {
            if (route == null || route.Item == null || Counter.Staging) return;
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

        internal static void YouTraded(Settlement site, List<(ItemObject item, int intoTheMarket)> moved)
        {
            if (site == null || moved == null || moved.Count == 0) return;
            Guard.Run("Hindsight.YouTraded", () =>
            {
                LeftOutOfTheForecast(site, moved);
                if (site.IsTown) SetAsideWhatYourTradeMoved(site, moved);
            });
        }

        private static void LeftOutOfTheForecast(Settlement site, List<(ItemObject item, int intoTheMarket)> moved)
        {
            bool worthKept = site.IsTown;
            _said.Rework(site.StringId, kept =>
            {
                if (kept.Item == null) return kept;
                for (int i = 0; i < moved.Count; i++)
                {
                    var (item, into) = moved[i];
                    if (item == null) continue;
                    (kept.StockYours, kept.WorthYours) = Scoring.YoursAdded(
                        kept.StockYours, kept.WorthYours, item == kept.Item,
                        item.ItemCategory != null && item.ItemCategory == kept.Item.ItemCategory,
                        worthKept, into, item.Value);
                }
                return kept;
            });
        }

        private static void SetAsideWhatYourTradeMoved(Settlement town,
                                                       List<(ItemObject item, int intoTheMarket)> moved)
        {
            var kinds = new HashSet<ItemCategory>();
            for (int i = 0; i < moved.Count; i++)
                if (moved[i].item?.ItemCategory != null && moved[i].intoTheMarket != 0)
                    kinds.Add(moved[i].item.ItemCategory);
            if (kinds.Count == 0) return;
            foreach (string where in _promised.Sites())
            {
                Settlement at = where == town.StringId ? town : Settlement.Find(where);
                if (at == null || PricedFrom(at) != town) continue;
                _promised.Rework(where, said =>
                {
                    if (said.Item?.ItemCategory != null && kinds.Contains(said.Item.ItemCategory))
                        said.YourTradeMovedIt = true;
                    return said;
                });
            }
        }

        internal static Settlement PricedFrom(Settlement site)
        {
            if (site.IsTown) return site;
            Village village = site.Village;
            if (village == null) return null;
            if (village.TradeBound != null) return village.TradeBound;
            if (_nearestTown.TryGetValue(site.StringId, out Settlement near)) return near;
            MobileParty villagers = village.VillagerPartyComponent?.MobileParty;
            near = SettlementHelper.FindNearestTownToSettlement(
                site, villagers != null ? villagers.NavigationCapability : MobileParty.NavigationType.All)?.Settlement;
            _nearestTown[site.StringId] = near;
            return near;
        }

        private static void Promise(TradeRoute route)
        {
            Settlement site = route.To;
            if (site == null || route.SellPrice <= 0) return;
            if (_promised.TryGet(site.StringId, route.Item.StringId, out Promised waiting) && waiting.Item != null &&
                !waiting.YourTradeMovedIt &&
                Scoring.StillToBeJudged(waiting.WithinDays, waiting.AtHours, (float)CampaignTime.Now.ToHours))
                return;
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
                one => !one.YourTradeMovedIt && !Scoring.TooOldToSay(one.WithinDays, one.AtHours, now, out _));
        }

        private static bool RoomForOneMoreFigure()
        {
            float now = (float)CampaignTime.Now.ToHours;
            return _said.Prune(
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
            int scored = 0, stale = 0, unpriced = 0, yours = 0, early = 0;
            float keptTotal = 0f;
            var stillToCome = new List<KeyValuePair<string, Promised>>();
            foreach (KeyValuePair<string, Promised> one in here)
            {
                Promised said = one.Value;
                if (said.Item == null) continue;
                if (Scoring.TooOldToSay(said.WithinDays, said.AtHours, now, out float since))
                {
                    stale++;
                    continue;
                }
                if (said.YourTradeMovedIt)
                {
                    yours++;
                    continue;
                }
                if (Scoring.TooSoonToSay(said.WithinDays, since))
                {
                    early++;
                    stillToCome.Add(one);
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
                keptTotal += TradeMath.HowCloseToThePromise(held);
                _bands.Add(said.Confidence, held);
                LedgerBehavior.Instance?.KeepPromiseScore(held);
                lines.Add("  " + Named(said.Item) + ": the panel promised " + said.SellPrice +
                          " a unit for " + said.Units + " unit(s) within " + Figure(said.WithinDays) +
                          " day(s) at Conf " + Share(said.Confidence) + "; you walked in " + Figure(since) +
                          " day(s) later and it pays " + found + ", " + Share(held) + " of what it promised");
            }
            foreach (KeyValuePair<string, Promised> one in stillToCome) _promised.Put(site.StringId, one.Key, one.Value);
            if (scored > 0)
                LedgerBehavior.Instance?.KeepArrival(site.StringId, TradeMath.MeanOf(keptTotal, scored));
            if (!Writing || scored + stale + yours + unpriced + early == 0) return;
            lines.Insert(0, "promise check at " + site.Name + ", " + scored + " promise(s) scored" +
                      (stale == 0 ? "" : ", " + stale + " passed over as too old to say anything") +
                      (yours == 0 ? "" : ", " + yours + " set aside because your own trading has moved the price since it was promised") +
                      (unpriced == 0 ? "" : ", " + unpriced + " the market would put no price on") +
                      (early == 0 ? "" : ", " + early + " kept for a later walk-in as too soon to say anything"));
            if (scored + stale + yours + unpriced == 0)
            {
                Log.Repeatable("promise check " + site.StringId, early.ToString(), lines[0]);
                return;
            }
            if (scored > 0)
            {
                lines.Add("  here: the price came " + Share(TradeMath.MeanOf(keptTotal, scored)) +
                          " close to its promise over " + scored +
                          " price(s) checked on this visit, a price above it missing as much as one below it");
                for (int band = TradeMath.Bands - 1; band >= 0; band--)
                {
                    if (_bands.Scored(band) == 0) continue;
                    lines.Add("  " + Scoring.Banded(band) + ": the price came " +
                              Share(_bands.Held(band)) + " close to its promise over " +
                              _bands.Scored(band) + " price(s) checked this session, a price above it " +
                              "missing as much as one below it");
                }
                if (LedgerBehavior.Instance != null &&
                    LedgerBehavior.Instance.PromiseScore(out int kept, out float overall))
                    lines.Add("  over this campaign: the price has come " + Share(overall) +
                              " close to its promise over " + kept + " price(s) checked, a price above it " +
                              "missing as much as one below it");
                if (LedgerBehavior.Instance != null &&
                    LedgerBehavior.Instance.PromiseScoreAt(site.StringId, out int walkIns, out float hereOverall))
                    lines.Add("  at " + site.Name + ": the price has come " + Share(hereOverall) +
                              " close to its promise over " + walkIns + " walk-in(s) here, a price above " +
                              "its promise missing as much as one below it" + WhatTheRecordDoes(walkIns, hereOverall));
            }
            Log.WriteMany(lines);
        }

        private static string WhatTheRecordDoes(int walkIns, float held)
        {
            Options s = Options.Current;
            switch (Confidence.WhatAMarketsRecordDoes(s.TrustWhatAMarketPaid && s.ConfidenceRanking, walkIns, held))
            {
                case Confidence.RecordAtAMarket.NotYet:
                    return ", which starts to count toward the score of a route selling here once you have " +
                           "walked in " + Confidence.EnoughArrivals + " times";
                case Confidence.RecordAtAMarket.TakesNothingOff:
                    return ", which takes nothing off the score of a route selling here";
                case Confidence.RecordAtAMarket.Lowers:
                    return ", which is what lowers the score of a route selling here";
                default:
                    return "";
            }
        }

        private static void Noted(Settlement site, ItemObject item, float withinDays)
        {
            if (site == null) return;
            if (_said.TryGet(site.StringId, item.StringId, out Said waiting) && waiting.Item != null &&
                Scoring.StillToBeJudged(waiting.WithinDays, waiting.AtHours, (float)CampaignTime.Now.ToHours))
                return;
            if (!_said.Holds(site.StringId, item.StringId) && _said.Full && !RoomForOneMoreFigure())
            {
                Log.Repeatable("forecast check", "full",
                               "forecast check is holding the " + Keeps<Said>.Most + " figures it keeps at once, " +
                               "so newer ones are passed over until one it holds is judged or grows too old to judge");
                return;
            }
            int steps = Scoring.StepsItIsReadOver(withinDays);
            int[] stockAhead = null, worthAhead = null;
            for (int step = Scoring.FirstStepJudged(withinDays); step < steps; step++)
            {
                var (stock, worth) = Forecast.ShiftAt(site, item, Scoring.DaysAtStep(step));
                if (stock == 0 && worth == 0) continue;
                if (stockAhead == null)
                {
                    stockAhead = new int[steps];
                    worthAhead = new int[steps];
                }
                stockAhead[step] = stock;
                worthAhead[step] = worth;
            }
            if (stockAhead == null) return;
            _said.Put(site.StringId, item.StringId, new Said
            {
                Item = item,
                AtHours = (float)CampaignTime.Now.ToHours,
                WithinDays = withinDays,
                StockThen = LedgerBehavior.StockOf(site, item),
                WorthThen = WorthOnTheShelf(site, item),
                StockAhead = stockAhead,
                WorthAhead = worthAhead
            });
        }

        private static void Written(Settlement site)
        {
            Dictionary<string, Said> here = _said.TakeAt(site.StringId);
            if (here == null) return;
            float now = (float)CampaignTime.Now.ToHours;
            var lines = new List<string>();
            int scored = 0, stale = 0, early = 0, landingMiss = 0, shared = 0;
            float weighed = 0f, cameTrue = 0f;
            var stillToCome = new List<KeyValuePair<string, Said>>();
            foreach (KeyValuePair<string, Said> one in here)
            {
                Said kept = one.Value;
                if (kept.Item == null) continue;
                if (Scoring.TooOldToSay(kept.WithinDays, kept.AtHours, now, out float since))
                {
                    stale++;
                    continue;
                }
                if (Scoring.TooSoonToSay(kept.WithinDays, since))
                {
                    early++;
                    stillToCome.Add(one);
                    continue;
                }
                scored++;
                var said = Scoring.AtTheWalkIn(kept.StockAhead, kept.WorthAhead, since,
                                               kept.StockThen, kept.WorthThen);
                Outcome how = Scoring.Weigh(said.stock, kept.StockThen,
                                            LedgerBehavior.StockOf(site, kept.Item),
                                            said.worth, kept.WorthThen,
                                            WorthOnTheShelf(site, kept.Item),
                                            kept.StockYours, kept.WorthYours);
                landingMiss += Math.Abs(how.LandingOff);
                string line = "  " + Named(kept.Item) + ": " + UnitsShifted(said.stock, how.Landed) +
                              Yours(kept.StockYours, false) + "; you walked in " + Figure(since) +
                              " day(s) after it said so, for a ride it put at " + Figure(kept.WithinDays) + " day(s)" +
                              ", held to what it said would move by the day you walked in";
                if (!how.WorthKept)
                {
                    lines.Add(line + "; no worth is kept for a kind of good here, which only a town does");
                    continue;
                }
                if (TradeMath.HowMuchCameTrue(said.worth, how.Moved, out float share))
                {
                    shared++;
                    float size = Math.Abs((float)said.worth);
                    weighed += size;
                    cameTrue += size * share;
                    LedgerBehavior.Instance?.KeepForecastScore(said.worth, how.Moved);
                }
                lines.Add(line + "; " + WorthShifted(said.worth, how.Moved) + Shared(how.Share) +
                          Yours(kept.WorthYours, true));
            }
            foreach (KeyValuePair<string, Said> one in stillToCome) _said.Put(site.StringId, one.Key, one.Value);
            if (!Writing || scored + stale + early == 0) return;
            lines.Insert(0, "forecast check at " + site.Name + ", " + scored + " good(s) it had a figure for" +
                      (stale == 0 ? "" : ", " + stale + " passed over as too old to say anything") +
                      (early == 0 ? "" : ", " + early + " kept for a later walk-in as too soon to say anything") +
                      (scored == 0 ? "" : ":"));
            if (scored > 0)
            {
                lines.Add("  in all: the unit figure was off by " +
                          Figure(TradeMath.MeanOf(landingMiss, scored)) + " unit(s) a good" +
                          (shared == 0
                              ? ", and no worth figure could be held to anything here"
                              : ", and the worth it said would move came " +
                                Share(TradeMath.ShareThatCameTrue(weighed, cameTrue)) + " close to what really moved, over " +
                                shared + " good(s)"));
                if (LedgerBehavior.Instance != null &&
                    LedgerBehavior.Instance.ForecastScore(out int figures, out float held))
                    lines.Add("  over this campaign: the worth it said would move came " + Share(held) +
                              " close to what really moved over " + figures + " figure(s) checked, a move bigger " +
                              "than it said missing as much as a smaller one, so what is on its way is counted at " +
                              Share(TradeMath.TrustInTheForecast(figures, held)) + " of what it says");
            }
            if (scored + stale == 0)
            {
                Log.Repeatable("forecast check " + site.StringId, early.ToString(), lines[0]);
                return;
            }
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

        private static string UnitsShifted(int said, int landed) => Scoring.UnitsShifted(said, landed);

        private static string WorthShifted(int said, int moved) => Scoring.WorthShifted(said, moved);

        private static string Yours(int yours, bool worth) => Scoring.Yours(yours, worth);

        private static string Shared(float share) => Scoring.Shared(share);

        private static string Share(float share) => Scoring.Share(share);

        private static string Figure(float number) => Scoring.Figure(number);
    }
}
