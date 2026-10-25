using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TradeLord
{
    internal sealed class Leg
    {
        internal ItemObject Item;
        internal Settlement From;
        internal Settlement To;
        internal int BuyQuoted;
        internal int SellQuoted;
        internal int LandedAtBuy;
        internal int LandedAtSell;
        internal int Till;
        internal int Units;
        internal int ValueCap;
        internal float Each;
        internal float Weighs;
        internal float ToBuy;
        internal float Days;
        internal int Shelf;
        internal int OnTheShelfNow;
        internal int Held;
        internal float Age;
        internal int Caravans;
        internal float Trust = 1f;
        internal Func<int, int> PaidAhead;
    }

    internal sealed class BuyFrom
    {
        internal Settlement Site;
        internal bool Here;
        internal float Room;
        internal int Gold;
        internal int HeldBack;
        internal long PurseBefore;
        internal int Paid;
        internal long Evidence;
        internal long EvidenceAtConf;
        internal int Routes;
        internal int Limit;

        internal int Purse => Math.Max(0, TradeMath.Budget(Gold, HeldBack, 0, 0));

        internal int Money => Math.Max(0, TradeMath.Budget(Gold, HeldBack, Limit, Paid));

        internal bool LimitBinds => Limit > 0 && Limit - Paid < Purse;
    }

    internal sealed class RouteScan
    {
        internal int Opened;
        internal int ThrownAway;
        internal int PassedOver;
        internal int CargoShut;
        internal int SetAsideRoutes;

        private readonly LedgerBehavior _ledger;
        private readonly bool _passOver;
        private readonly float _safety;
        private readonly bool _rankByScore;
        private readonly bool _wholeLoad;
        private readonly bool _conservative;
        private readonly bool _trustWhatItPaid;
        private readonly Dictionary<Settlement, int> _pressure;
        private readonly MobileParty _party;
        private readonly Settlement _here;
        private readonly Books _books;
        private readonly bool _sim;
        private readonly bool _soldHere;
        private readonly float _roomNow;
        private readonly int _purseNow;
        private readonly int _heldBack;
        private readonly float _shareCap;
        private readonly int _herdRoom;
        private readonly float _herdWeight;
        private readonly Dictionary<Settlement, BuyFrom> _from = new Dictionary<Settlement, BuyFrom>();
        private readonly Dictionary<ItemObject, Good> _goods = new Dictionary<ItemObject, Good>();
        private readonly Dictionary<Settlement, float> _trust = new Dictionary<Settlement, float>();

        internal RouteScan(LedgerBehavior ledger, MobileParty party, Settlement here)
        {
            _ledger = ledger;
            _party = party;
            _here = here?.SettlementComponent != null ? here : null;
            _books = TradeActionBehavior.TheVisit;
            _sim = TradeActionBehavior.OnPaper;
            _soldHere = _here != null && TradeActionBehavior.SoldHereThisVisit;
            _passOver = Guard.Read("Scan.PricedAsTheGame", 0, none => Priced.PricedAsTheGameItself(), false);
            _safety = TradePolicy.ResaleSafety();
            _rankByScore = Options.Current.ConfidenceRanking;
            _wholeLoad = Options.Current.PickTheBuyerOnTheWholeStack;
            _conservative = Options.Current.ConservativeRouteProjection;
            _trustWhatItPaid = Options.Current.TrustWhatAMarketPaid;
            _pressure = LedgerBehavior.CaravanPressure();
            _roomNow = party == null ? float.PositiveInfinity : Carry.Room(party);
            _purseNow = Hero.MainHero?.Gold ?? 0;
            _heldBack = TradeActionBehavior.GoldHeldBack();
            float capacity = party == null ? 0f : Carry.Capacity(party);
            _shareCap = Options.Current.MaxHeldShare > 0f ? capacity * Options.Current.MaxHeldShare : 0f;
            _herdRoom = party == null ? 0 : Drove.RoomForLivestock(party);
            _herdWeight = TradeMath.HerdWeight(capacity * Options.Current.MaxCargoShare,
                                               party == null ? 0 : Drove.DrivenNow(party), _herdRoom);
        }

        internal Settlement Here => _here;

        internal bool RankByScore => _rankByScore;

        internal IEnumerable<BuyFrom> Markets => _from.Values;

        internal bool SetAside(Settlement from) =>
            from != null && Arrivals.LeftAloneOnArrival(from.StringId, _here?.StringId,
                                                        TradeActionBehavior.TradesOnArrival,
                                                        TradeActionBehavior.MarketTheMarkerLeavesOut());

        internal BuyFrom At(Settlement from)
        {
            if (_from.TryGetValue(from, out BuyFrom at)) return at;
            at = new BuyFrom { Site = from, Here = from == _here };
            var freed = at.Here && _soldHere
                ? (room: 0f, gold: 0)
                : Guard.Read("Plan.WhatYourCargoFrees", from,
                             site => Marker.WhatYourCargoFrees(site, Options.Current.HoldCargoForBestMarket),
                             (room: 0f, gold: 0));
            long purse = (long)_purseNow + freed.gold;
            if (at.Here)
            {
                at.Room = _roomNow - _books.Weight(_sim) + _books.CapacityAdded(_sim) + freed.room;
                at.Paid = _books.PaidOut(_sim);
                purse += _books.Purse(_sim);
                at.PurseBefore = purse + at.Paid;
            }
            else
            {
                at.Room = _roomNow + freed.room;
                at.PurseBefore = purse;
            }
            at.Gold = purse >= int.MaxValue ? int.MaxValue : (int)Math.Max(0L, purse);
            at.HeldBack = _heldBack;
            _from[from] = at;
            return at;
        }

        private Good GoodOf(ItemObject item)
        {
            if (!_goods.TryGetValue(item, out Good good)) _goods[item] = good = TradePolicy.Describe(item);
            return good;
        }

        private float TrustAt(Settlement to)
        {
            if (!_trustWhatItPaid) return 1f;
            if (_trust.TryGetValue(to, out float trust)) return trust;
            trust = _ledger.PromiseScoreAt(to.StringId, out int arrivals, out float heldThere)
                ? Confidence.AsPromisesHaveHeld(1f, arrivals, heldThere)
                : 1f;
            _trust[to] = trust;
            return trust;
        }

        private float ScoreOf(TradeRoute route)
        {
            if (_wholeLoad) return _rankByScore ? route.Score : route.ProfitPerDay;
            float rate = TradeMath.OneUnitRate(route.SellPrice, route.BuyPrice, route.TravelDays, _conservative, _safety);
            return _rankByScore ? rate * route.Confidence * route.Trust : rate;
        }

        internal TradeRoute Best(ItemObject item, Settlement from, int buyPrice, bool beaten, float bar,
                                 out Block why)
        {
            why = Block.NoResaleMarket;
            if (item == null || from == null || buyPrice <= 0) { why = Block.NoStock; return null; }
            BuyFrom at = At(from);
            Good good = GoodOf(item);
            float each = Carry.Weighs(item);
            float toBuy = Travel.EstimateDaysFromParty(from);
            if (!at.Here && !LedgerBehavior.WithinTravelCeiling(from, toBuy)) return null;
            string id = item.StringId;
            int held = at.Here ? LedgerBehavior.InAll(_party?.ItemRoster, item) + _books.Held(_sim, id) : 0;
            var taken = at.Here ? _books.Purchases(_sim, id) : (0, 0);
            if (at.Here && _books.Sold(_sim, id)) { why = Block.TradedHereAlready; return null; }
            int herdRoom = int.MaxValue;
            if (good.IsLivestock)
            {
                herdRoom = at.Here ? _herdRoom - _books.HerdTaken(_sim) : _herdRoom;
                if (herdRoom <= 0) { why = Block.HerdFull; return null; }
            }
            if (at.Purse <= 0) { why = Block.BudgetSpent; return null; }
            int fits = TradeMath.UnitsThatFit(at.Room, each);
            int landedAtBuyTown = Forecast.WorthShiftAsItHasHeld(from, item, toBuy);
            int openingBuy = Bulk.Opening(from, item, false, buyPrice, landedAtBuyTown);
            Options s = Options.Current;
            int caps = TradeRules.UnitsTheCapsAllow(good, openingBuy, taken, held, _shareCap, s);
            if (caps <= 0)
            {
                why = TradeRules.WhatCapsAGood(good, openingBuy, taken, held, _shareCap, s);
                if (why == Block.None) why = Block.HeldEnough;
                return null;
            }
            int stocked = TradeRules.UnitsAPlanMayTake(good, openingBuy, taken, held, _shareCap, herdRoom,
                                                       s.BuyCapPerItem > 0 ? int.MaxValue
                                                                           : LedgerBehavior.UncappedBuyProjection, s);
            int shelf = 0, onTheShelfNow = int.MaxValue;
            if (s.Omniscient || at.Here)
            {
                onTheShelfNow = LedgerBehavior.StockOf(from, item) - (at.Here ? _books.Stocked(_sim, id) : 0) -
                                (from.IsVillage ? 1 : 0);
                shelf = TradeMath.StockAfterShift(onTheShelfNow,
                            Forecast.UnitsLanding(from, item, toBuy),
                            Forecast.UnitsLeaving(from, item, toBuy));
                stocked = Math.Min(stocked, shelf);
            }
            if (stocked <= 0) { why = Block.NoStock; return null; }
            int valueCap = s.BuyValueCapPerItem > 0 ? Math.Max(0, s.BuyValueCapPerItem - taken.Item2) : 0;
            float weighs = each > 0.01f ? each : good.IsLivestock ? _herdWeight : each;

            TradeRoute best = null;
            float key = bar;
            var sells = _ledger.EverySell(item);
            foreach (var (to, sellPrice) in sells)
            {
                if (to == from) continue;

                int till = 0;
                if (s.Omniscient)
                {
                    till = TradeRules.WhatTheTillCanPay(to.SettlementComponent?.Gold ?? 0, to.IsVillage);
                    if (till <= 0) continue;
                }

                float soonest = toBuy + Travel.StraightDaysBetween(from, to);
                if (!LedgerBehavior.WithinTravelCeiling(to, soonest)) continue;
                if (_passOver && TradeMath.NoPriceCouldClearTheMargin(openingBuy, sellPrice, _safety, s.MinProfitMargin))
                {
                    PassedOver++;
                    continue;
                }
                if (_passOver && TradeMath.NoRoadCouldMakeIt(openingBuy, sellPrice, stocked, soonest, beaten, key))
                {
                    PassedOver++;
                    continue;
                }

                // Intended-Behavior: Days runs from where your party stands to Buy From and on to Sell At, as the developer chose; it is not to be raised again.
                float days = toBuy + Travel.EstimateDaysBetween(from, to, toBuy);
                if (TradeMath.OutOfReach(days)) continue;
                if (!LedgerBehavior.WithinTravelCeiling(to, days)) continue;

                int landedAtSellTown = Forecast.WorthShiftAsItHasHeld(to, item, days);
                if (_passOver && TradePolicy.NoOpeningPriceCouldMakeIt(openingBuy, sellPrice, landedAtSellTown,
                                                                      stocked, days, beaten, key, _safety))
                {
                    PassedOver++;
                    continue;
                }
                int openingSell = Bulk.Opening(to, item, true, sellPrice, landedAtSellTown);
                Opened++;
                float realizable = TradePolicy.Realizable(openingSell);
                if (!TradePolicy.BuyAcceptable(openingBuy, realizable)) { ThrownAway++; continue; }

                int qtyCap = till > 0 ? Math.Min(stocked, till / openingSell) : stocked;
                if (qtyCap <= 0) { ThrownAway++; continue; }
                if (fits <= 0) { ThrownAway++; CargoShut++; continue; }

                float ceiling = (float)(openingSell - openingBuy) * Math.Min(qtyCap, fits);
                if (beaten && TradeMath.PerDay(ceiling, days) <= key) { ThrownAway++; continue; }

                var leg = new Leg
                {
                    Item = item, From = from, To = to,
                    BuyQuoted = buyPrice, SellQuoted = sellPrice,
                    LandedAtBuy = landedAtBuyTown, LandedAtSell = landedAtSellTown,
                    Till = till, Units = qtyCap, ValueCap = valueCap,
                    Each = each, Weighs = weighs, ToBuy = toBuy, Days = days,
                    Shelf = shelf, OnTheShelfNow = onTheShelfNow, Held = held,
                    Age = s.Omniscient ? -1f
                        : Math.Max(_ledger.ObservationAgeDays(item, from), _ledger.ObservationAgeDays(item, to)),
                    Caravans = Pressure(from) + Pressure(to),
                    Trust = TrustAt(to)
                };
                TradeRoute route = Sized(leg, fits, at.Purse, Bound.Purse);
                if (route == null) continue;
                float score = ScoreOf(route);
                if (beaten && score <= key) continue;

                key = score;
                beaten = true;
                best = route;
            }
            if (best == null) return null;
            why = Block.None;
            best.QtyUncut = Uncut(best.Leg, at.Purse);
            return best;
        }

        internal float ScoreToBeat(TradeRoute route) => route == null ? 0f : ScoreOf(route);

        private int Pressure(Settlement s) => s != null && _pressure.TryGetValue(s, out int n) ? n : 0;

        internal TradeRoute Sized(Leg leg, int fits, int money, Bound moneyBound)
        {
            int units = Math.Min(leg.Units, fits);
            if (units <= 0 || money <= 0) return null;
            int spendCap = leg.ValueCap > 0 ? Math.Min(leg.ValueCap, money) : money;
            RouteQuote q = Bulk.Walk(leg.From, leg.To, leg.Item, units, leg.Till, spendCap,
                                     leg.BuyQuoted, leg.SellQuoted, leg.LandedAtBuy, leg.LandedAtSell, leg.Held);
            if (q.Units <= 0) return null;

            bool conservative = Options.Current.ConservativeRouteProjection;
            int proceeds = conservative ? (int)TradePolicy.Realizable(q.SellTotal) : q.SellTotal;
            int profit = proceeds - q.BuyTotal;
            if (profit <= 0) return null;

            int flatSell = q.OpeningSellPrice * q.Units;
            int flat = (conservative ? (int)TradePolicy.Realizable(flatSell) : flatSell) - q.OpeningBuyPrice * q.Units;
            float runsOut = Forecast.RunsOutIn(leg.From, leg.Item, leg.OnTheShelfNow, q.Units, leg.ToBuy);
            float lasts = Confidence.Lasts(runsOut, Forecast.On);
            float confidence = Confidence.Of(q.Simulated, flat, profit, leg.Shelf, q.Units, leg.Days,
                                             leg.Caravans, leg.Age, lasts, leg.ToBuy);
            float perDay = TradeMath.PerDay(profit, leg.Days);
            bool sized = fits < leg.Units;
            bool moneyCapped = q.SpendCapped && (leg.ValueCap <= 0 || money < leg.ValueCap);
            return new TradeRoute
            {
                Item = leg.Item, From = leg.From, To = leg.To,
                BuyPrice = q.OpeningBuyPrice, SellPrice = q.OpeningSellPrice,
                Quantity = q.Units,
                TravelDays = leg.Days, TotalProfit = profit, ProfitPerDay = perDay,
                Confidence = confidence, Score = perDay * confidence * leg.Trust, Trust = leg.Trust,
                Simulated = q.Simulated, Caravans = leg.Caravans,
                Parts = Confidence.PartsOf(q.Simulated, flat, profit, leg.Shelf, q.Units, leg.Days, leg.Caravans,
                                           leg.Age, lasts, leg.ToBuy),
                StillComing = TradeMath.StillComing(q.Units, leg.OnTheShelfNow),
                RunsOutInDays = runsOut,
                SizedToCargo = sized && q.Units >= fits,
                HeldBackBy = sized && q.Units >= fits ? Bound.Cargo : moneyCapped ? moneyBound : Bound.None,
                Key = TradeMath.Key(profit, confidence, leg.Trust, _rankByScore, leg.Weighs, q.Units, leg.Days),
                Cost = q.BuyTotal,
                Leg = leg
            };
        }

        internal int Uncut(Leg leg, int money)
        {
            if (leg == null || money <= 0) return 0;
            int spendCap = leg.ValueCap > 0 ? Math.Min(leg.ValueCap, money) : money;
            return Bulk.Walk(leg.From, leg.To, leg.Item, leg.Units, leg.Till, spendCap,
                             leg.BuyQuoted, leg.SellQuoted, leg.LandedAtBuy, leg.LandedAtSell, leg.Held).Units;
        }

        internal TradeRoute Resized(TradeRoute row, float roomLeft, int moneyLeft, Bound moneyBound)
        {
            if (row?.Leg == null) return null;
            TradeRoute sized = Sized(row.Leg, TradeMath.UnitsThatFit(roomLeft, row.Leg.Each), moneyLeft, moneyBound);
            if (sized == null) return null;
            sized.Key = row.Key;
            sized.QtyUncut = row.QtyUncut;
            return sized;
        }

        internal int LimitAt(BuyFrom at)
        {
            int limit = TradeMath.SpendLimit(Options.Current.MaxSpendPerVisit, at.PurseBefore,
                                             Options.Current.AdaptiveSpendLimit, at.Evidence);
            return at.Here && limit > 0 ? Math.Max(limit, TradeActionBehavior.LimitThisVisit) : limit;
        }
    }

    internal static class EngineTally
    {
        internal static int Scans;
        internal static int Planned;
        internal static int OnTheRoad;
        internal static int Marker;
        internal static int HoldCargo;
        internal static int Tooltips;
        internal static int OffTheLedger;
        internal static double SlowestScan;

        internal static void SayTheDay()
        {
            if (Options.Current.ExtendedDebugLogging && Scans + Planned + OnTheRoad + Marker + HoldCargo + Tooltips > 0)
                Log.Write("one engine today: " + Scans + " route scan(s), " + Planned + " good(s) the buying pass " +
                          "planned from the scan, " + OnTheRoad + " on the road, " + Marker + " map marker weighing(s), " +
                          HoldCargo + " Hold cargo for the best market weighing(s) and " + Tooltips +
                          " tooltip(s); buying went against the TradeLord ledger's Sell At " + OffTheLedger +
                          " time(s); slowest route scan " +
                          SlowestScan.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " ms");
            Scans = Planned = OnTheRoad = Marker = HoldCargo = Tooltips = OffTheLedger = 0;
            SlowestScan = 0d;
        }
    }

    internal static class Plan
    {
        internal static Ladder SellLadder(Settlement to, ItemObject item, int quoted, int landed) =>
            to == null || item == null
                ? null
                : Bulk.AsItLands(to, new EquipmentElement(item), true, quoted, landed) ??
                  new Ladder(to, item, true, quoted, 0);

        internal static Ladder SellLadder(Leg leg) =>
            leg == null ? null : SellLadder(leg.To, leg.Item, leg.SellQuoted, leg.LandedAtSell);

        internal static int TillAt(Settlement to) =>
            to != null && Options.Current.Omniscient
                ? TradeRules.WhatTheTillCanPay(to.SettlementComponent?.Gold ?? 0, to.IsVillage)
                : 0;

        internal static float SellSide(ItemObject item, Settlement to, float days, bool simulated, int caravans)
        {
            if (to == null || !Options.Current.ConfidenceRanking) return 1f;
            LedgerBehavior ledger = LedgerBehavior.Instance;
            float age = Options.Current.Omniscient || ledger == null ? -1f
                : ledger.ObservationAgeDays(item, to);
            float conf = Confidence.Of(simulated, 1, 1, 0, 1, days, caravans, age);
            float trust = 1f;
            if (Options.Current.TrustWhatAMarketPaid && ledger != null &&
                ledger.PromiseScoreAt(to.StringId, out int arrivals, out float held))
                trust = Confidence.AsPromisesHaveHeld(1f, arrivals, held);
            return conf * trust;
        }

        internal static float SaleScore(ItemObject item, Settlement to, long profit, long flat, int units, float days,
                                        bool simulated, int stock, int caravans, out float confidence, out float trust)
        {
            LedgerBehavior ledger = LedgerBehavior.Instance;
            Options s = Options.Current;
            float age = s.Omniscient || ledger == null ? -1f : ledger.ObservationAgeDays(item, to);
            int whole = profit >= int.MaxValue ? int.MaxValue : (int)Math.Max(0L, profit);
            int level = flat >= int.MaxValue ? int.MaxValue : (int)Math.Max(0L, flat);
            confidence = Confidence.Of(simulated, level, whole, stock, units, days, caravans, age);
            trust = 1f;
            if (s.TrustWhatAMarketPaid && ledger != null && to != null &&
                ledger.PromiseScoreAt(to.StringId, out int arrivals, out float heldThere))
                trust = Confidence.AsPromisesHaveHeld(1f, arrivals, heldThere);
            float perDay = TradeMath.PerDay(profit, days);
            return s.ConfidenceRanking ? perDay * confidence * trust : perDay;
        }

        internal static TradeRoute BestSale(ItemObject item, Func<int, int> paidAhead, int units, int held,
                                            int money, int stock, Settlement notHere, out Ladder rungs, out int till)
        {
            rungs = null;
            till = 0;
            LedgerBehavior ledger = LedgerBehavior.Instance;
            if (item == null || units <= 0 || ledger == null) return null;
            Options s = Options.Current;
            bool rank = s.ConfidenceRanking;
            bool conservative = s.ConservativeRouteProjection;
            var pressure = LedgerBehavior.CaravanPressure();
            float each = Carry.Weighs(item);
            TradeRoute best = null;
            float bestKey = 0f;
            var sells = ledger.EverySell(item);
            foreach (var (to, quoted) in sells)
            {
                if (to == null || quoted <= 0 || to == notHere) continue;
                float days = Travel.EstimateDaysFromParty(to);
                if (TradeMath.OutOfReach(days) || !LedgerBehavior.WithinTravelCeiling(to, days)) continue;
                int landed = Forecast.WorthShiftAsItHasHeld(to, item, days);
                Ladder ladder = SellLadder(to, item, quoted, landed);
                int purse = TillAt(to);
                int drawn = held > 0 ? ladder.Through(held) : 0;
                int bought = 0, cost = 0, sold = 0, firstSell = 0, firstPaid = 0;
                while (bought < units)
                {
                    int paid = paidAhead == null ? 0 : paidAhead(bought);
                    int price = ladder.At(held + bought);
                    if (price <= 0) break;
                    if (paid > 0 && !TradePolicy.BuyAcceptable(paid, TradePolicy.Realizable(price))) break;
                    if (purse > 0 && (long)drawn + sold + price > purse) break;
                    if (money > 0 && paid > 0 && (long)cost + paid > money) break;
                    if (bought == 0) { firstSell = price; firstPaid = paid; }
                    cost += paid;
                    sold += price;
                    bought++;
                }
                if (bought <= 0) continue;
                int proceeds = conservative ? (int)TradePolicy.Realizable(sold) : sold;
                int profit = proceeds - cost;
                if (profit <= 0) continue;
                int flatSell = firstSell * bought;
                int flat = (conservative ? (int)TradePolicy.Realizable(flatSell) : flatSell) - firstPaid * bought;
                int caravans = pressure.TryGetValue(to, out int n) ? n : 0;
                float key = SaleScore(item, to, profit, flat, bought, days, ladder.Walkable, stock, caravans,
                                      out float confidence, out float trust);
                if (!s.PickTheBuyerOnTheWholeStack)
                {
                    key = TradeMath.OneUnitRate(firstSell, firstPaid, days, conservative, TradePolicy.ResaleSafety());
                    if (rank) key *= confidence * trust;
                }
                float perDay = TradeMath.PerDay(profit, days);
                float score = perDay * confidence * trust;
                if (best != null && key <= bestKey) continue;
                bestKey = key;
                rungs = ladder;
                till = purse;
                best = new TradeRoute
                {
                    Item = item, To = to, BuyPrice = firstPaid, SellPrice = firstSell,
                    Quantity = bought, QtyUncut = bought, TravelDays = days,
                    TotalProfit = profit, ProfitPerDay = perDay, Confidence = confidence, Trust = trust,
                    Score = score, Simulated = ladder.Walkable, Caravans = caravans, Cost = cost,
                    Key = TradeMath.Key(profit, confidence, trust, rank, each, bought, days),
                    Leg = new Leg
                    {
                        Item = item, To = to, SellQuoted = quoted, LandedAtSell = landed, Till = purse,
                        Units = units, Each = each, Weighs = each, Days = days, Held = held,
                        Caravans = caravans, Trust = trust, PaidAhead = paidAhead
                    }
                };
            }
            return best;
        }
    }
}
