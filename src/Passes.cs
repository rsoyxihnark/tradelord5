using System;
using System.Collections.Generic;
using System.Text;

namespace TradeLord
{
    internal sealed partial class BlockTally
    {
        private readonly Dictionary<Block, int> _counts = new Dictionary<Block, int>();
        private Block _firstGuard = Block.None;

        internal void Note(Block reason)
        {
            if (reason == Block.None) return;
            if (_firstGuard == Block.None && Guarded(reason)) _firstGuard = reason;
            _counts.TryGetValue(reason, out int seen);
            _counts[reason] = seen + 1;
        }

        internal bool Any => _counts.Count > 0;

        internal bool Saw(Block reason) => _counts.ContainsKey(reason);

        private static bool Structural(Block reason) =>
            reason == Block.NotTradable || reason == Block.NotMerchandise ||
            reason == Block.MountOrHaulAnimal;

        private static bool Guarded(Block reason) =>
            reason == Block.NeverList || reason == Block.Locked || reason == Block.Protected ||
            reason == Block.QuestGoods || reason == Block.FoodReserve;

        internal Block Dominant()
        {
            Block top = Block.None;
            int best = 0;
            foreach (var kv in _counts)
                if (!Structural(kv.Key) && kv.Key != Block.BudgetSpent &&
                    (kv.Value > best || (kv.Value == best && kv.Key < top)))
                { best = kv.Value; top = kv.Key; }
            if (top == Block.None && Saw(Block.BudgetSpent)) return Block.BudgetSpent;
            return Guarded(top) ? _firstGuard : top;
        }

        internal string Summary()
        {
            var order = new List<KeyValuePair<Block, int>>(_counts);
            order.Sort((x, y) =>
            {
                int byCount = y.Value.CompareTo(x.Value);
                return byCount != 0 ? byCount : x.Key.CompareTo(y.Key);
            });
            var sb = new StringBuilder();
            foreach (var kv in order)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(kv.Key).Append("=").Append(kv.Value);
            }
            return sb.ToString();
        }
    }

    internal struct Traded
    {
        internal int Units;
        internal int SimGold;
        internal int Profit;
        internal int Earned;
        internal float Unfitted;
        internal int UnfittedCost;
        internal float UnfittedProfit;
    }

    internal struct Pick
    {
        internal int At;
        internal Good Good;
        internal float Worth;
        internal int LedgerRank;
        internal int Weighed;
    }

    internal struct Lot
    {
        internal int Units;
        internal int Price;
        internal float Resale;
        internal float Weight;
        internal int Herd;
        internal bool Weighed;
        internal int Stopper;
    }

    internal static class Picks
    {
        internal static void MostMoneyFirst(List<Pick> stock) =>
            stock?.Sort((x, y) => y.Worth.CompareTo(x.Worth));

        internal static void WhatTheLedgerAskedForFirst(List<Pick> stock)
        {
            if (stock == null || stock.Count < 2) return;
            var asked = new List<Pick>();
            var rest = new List<Pick>();
            foreach (Pick one in stock) (one.LedgerRank > 0 ? asked : rest).Add(one);
            if (asked.Count == 0) return;
            asked.Sort((x, y) => x.LedgerRank != y.LedgerRank
                ? x.LedgerRank.CompareTo(y.LedgerRank)
                : y.Worth.CompareTo(x.Worth));
            stock.Clear();
            stock.AddRange(asked);
            stock.AddRange(rest);
        }
    }

    internal interface IBuyingMarket
    {
        int Count { get; }
        bool Stopped { get; }
        bool Village { get; }
        int AmountAt(int at);
        Good GoodAt(int at);
        bool MayBuy(int at, in Good good, out Block why);
        int TheLedgerAsksFor(int at);
        int TheirsToSell(int at);
        int Carried(int at);
        void PriceTheMarketsFor(List<Pick> shelf);
        bool ResaleMarket(int at, int paid, int units, out int price);
        int ResaleUpTo(int at, int units);
        int ResaleTill(int at);
        int PriceToBuy(int at);
        Func<int, int> PricesAhead(int at);
        int Spendable();
        float Room();
        int HerdRoom();
        void Staged(int at, int price);
        bool Take(int at, int price, out int cost);
        void Resold(int at, int units, int gold);
    }

    internal interface ISellingMarket
    {
        int Count { get; }
        bool Stopped { get; }
        bool Village { get; }
        string IdAt(int at);
        Good GoodAt(int at);
        bool MaySell(int at, in Good good, out int keep, out Block why);
        int YoursToSell(int at);
        int CostBasis(int at);
        string PaidKeyAt(int at);
        int PurchasedUnits(int at);
        int UnpaidWorth(int at);
        bool ResaleMarket(int at, int units, int worth, out int price, out int takes);
        int PriceToSell(int at);
        bool TheGameGivesTradeXpFor(int at);
        bool OfAQuality(int at);
        int Till();
        int TillNow();
        void Staged(int at, int price);
        bool Give(int at, int price, out int proceeds);
        void RecordedSale(int at);
    }

    internal struct Basis
    {
        internal int Paid;
        internal bool FromMarket;
        internal int PaidLeft;
        internal int UnpaidWorth;

        internal static Basis For(int costBasis, int purchased, string id, Books books, bool sim,
                                  Options s)
        {
            Basis basis;
            basis.Paid = costBasis;
            basis.FromMarket = s.CostBasisMode == 2;
            basis.PaidLeft = Math.Max(0, purchased - books.PaidDrawn(sim, id));
            basis.UnpaidWorth = -1;
            return basis;
        }

        internal int Unit(out bool askTheMarket)
        {
            int worth = FromMarket || PaidLeft > 0 ? Paid : 0;
            askTheMarket = worth == 0 && UnpaidWorth < 0;
            return worth;
        }

        internal bool SoldOne()
        {
            if (PaidLeft <= 0) return false;
            PaidLeft--;
            return true;
        }

        internal bool SkipTheUnitsYouPaidFor(ref int remaining) =>
            TradeMath.SkipTheUnitsYouPaidFor(FromMarket, ref remaining, ref PaidLeft);

        internal bool SetTheBoughtUnitsAside(ref int remaining) =>
            TradeMath.SetTheBoughtUnitsAside(ref remaining, ref PaidLeft);
    }

    internal static class TradePass
    {
        internal static List<Pick> WhatToBuy(IBuyingMarket market, Books books, bool sim,
                                             float shareCap, Options s, BlockTally tally)
        {
            int holdCap = s.MaxHeldPerItem;
            var shelf = new List<Pick>();
            for (int at = 0; at < market.Count; at++)
            {
                if (market.AmountAt(at) <= 0) { tally.Note(Block.NoStock); continue; }
                Good good = market.GoodAt(at);
                if (!market.MayBuy(at, good, out Block whyBuy)) { tally.Note(whyBuy); continue; }
                if (books.Sold(sim, good.Id)) { tally.Note(Block.TradedHereAlready); continue; }
                if (market.TheirsToSell(at) <= 0) { tally.Note(Block.NoStock); continue; }
                int held = market.Carried(at) + books.Held(sim, good.Id);
                if (holdCap > 0 && held >= holdCap) { tally.Note(Block.HeldEnough); continue; }
                if (shareCap > 0f && (held + 1) * good.Weight > shareCap) { tally.Note(Block.HeldEnough); continue; }
                shelf.Add(new Pick { At = at, Good = good, LedgerRank = market.TheLedgerAsksFor(at) });
            }
            market.PriceTheMarketsFor(shelf);

            var stock = new List<Pick>();
            int herdRoom = -1;
            foreach (Pick one in shelf)
            {
                int here = market.PriceToBuy(one.At);
                if (here <= 0) { tally.Note(Block.NoStock); continue; }
                int carried = market.Carried(one.At) + books.Held(sim, one.Good.Id);
                if (one.Good.IsLivestock && herdRoom < 0)
                    herdRoom = Math.Max(0, market.HerdRoom() - books.HerdTaken(sim));
                int take = UnitsItCouldTake(market, one.At, one.Good, here, books.Purchases(sim, one.Good.Id),
                                            carried, market.Room() - books.Weight(sim), herdRoom, shareCap, s);
                if (!market.ResaleMarket(one.At, here, carried + take, out int elsewhere))
                { tally.Note(Block.NoResaleMarket); continue; }
                float realizable = TradeMath.Realizable(
                    market.ResaleUpTo(one.At, carried + 1) - market.ResaleUpTo(one.At, carried),
                    s.ResaleSafetyFactor);
                if (!TradeMath.BuyAcceptable(here, realizable, s.MinProfitMargin)) { tally.Note(Block.BelowMargin); continue; }
                Pick picked = one;
                picked.Worth = WhatThisPickWouldReallyMake(market, one.At, carried, here, take, s);
                picked.Weighed = take;
                stock.Add(picked);
            }
            Picks.MostMoneyFirst(stock);
            Picks.WhatTheLedgerAskedForFirst(stock);
            return stock;
        }

        private static int UnitsItCouldTake(IBuyingMarket market, int at, in Good good, int price,
                                            (int count, int spent) taken, int held, float room, int herdRoom,
                                            float shareCap, Options s)
        {
            int allowed = TradeRules.UnitsTheCapsAllow(good, price, taken, held, shareCap, s);
            if (good.IsLivestock && herdRoom < allowed) allowed = herdRoom;
            return allowed <= 0
                ? 0
                : TradeMath.MostYouCouldTake(price, good.Weight, market.TheirsToSell(at), market.Spendable(),
                                             room, allowed);
        }

        private static float WhatThisPickWouldReallyMake(IBuyingMarket market, int at, int carried,
                                                         int here, int take, Options s)
        {
            if (take <= 0 || here <= 0) return 0f;
            int till = market.ResaleTill(at);
            int drawn = market.ResaleUpTo(at, carried);
            float made = 0f;
            for (int u = 0; u < take; u++)
            {
                int wouldDraw = market.ResaleUpTo(at, carried + u + 1);
                if (TradeRules.TheBuyerCouldNotPay(wouldDraw, till)) break;
                float realizable = TradeMath.Realizable(wouldDraw - drawn, s.ResaleSafetyFactor);
                if (!TradeMath.BuyAcceptable(here, realizable, s.MinProfitMargin)) break;
                made += realizable - here;
                drawn = wouldDraw;
            }
            return made;
        }

        internal static Traded BuyThem(List<Pick> stock, IBuyingMarket market, Books books, bool sim,
                                       float shareCap, Options s, BlockTally tally)
        {
            Traded moved = default(Traded);
            float simWeight = books.Weight(sim);
            int herdRoom = -1;

            foreach (Pick picked in stock)
            {
                if (market.Stopped || market.Spendable() <= 0) break;
                Good good = picked.Good;
                bool livestock = good.IsLivestock;
                if (livestock)
                {
                    if (herdRoom < 0)
                        herdRoom = Math.Max(0, market.HerdRoom() - books.HerdTaken(sim));
                    if (herdRoom <= 0) { tally.Note(Block.HerdFull); continue; }
                }

                var prior = books.Purchases(sim, good.Id);
                int remaining = market.TheirsToSell(picked.At);
                int countThis = prior.count, spentThis = prior.spent;
                int held = market.Carried(picked.At) + books.Held(sim, good.Id);
                if (s.PickTheBuyerOnTheWholeStack && picked.Weighed > 0)
                {
                    int now = market.PriceToBuy(picked.At);
                    int take = UnitsItCouldTake(market, picked.At, good, now, prior, held,
                                                market.Room() - simWeight, herdRoom, shareCap, s);
                    if (take > 0 && take < picked.Weighed &&
                        !market.ResaleMarket(picked.At, now, held + take, out _))
                    { tally.Note(Block.NoResaleMarket); continue; }
                }
                int till = market.ResaleTill(picked.At);
                int drawn = market.ResaleUpTo(picked.At, held);
                int unitsBefore = moved.Units;
                int drawnBefore = drawn;

                while (remaining > 0)
                {
                    int price = market.PriceToBuy(picked.At);
                    int wouldDraw = market.ResaleUpTo(picked.At, held + 1);
                    if (TradeRules.TheBuyerCouldNotPay(wouldDraw, till))
                    { tally.Note(Block.BuyerTillEmpty); break; }
                    if (!TradeMath.BuyAcceptable(price, TradeMath.Realizable(wouldDraw - drawn,
                                                                            s.ResaleSafetyFactor),
                                                 s.MinProfitMargin))
                    { tally.Note(Block.BelowMargin); break; }
                    Block capped = TradeRules.WhatStopsBuying(good, price, market.Spendable(),
                                                              (countThis, spentThis), held, shareCap,
                                                              livestock, herdRoom,
                                                              market.Village && remaining <= 1, s);
                    if (capped != Block.None) { tally.Note(capped); break; }
                    if (TradeRules.NoRoomForOneMore(good, market.Room() - simWeight))
                    {
                        tally.Note(Block.CarryWeight);
                        var left = UnitsTheHoldLeftBehind(market, picked.At, good, remaining, held, drawn,
                                                          till, (countThis, spentThis), shareCap, s);
                        moved.Unfitted += left.units * good.Weight;
                        moved.UnfittedCost = TradeMath.AddedUp(moved.UnfittedCost, left.cost);
                        moved.UnfittedProfit += left.profit;
                        break;
                    }

                    if (sim)
                    {
                        moved.SimGold += price;
                        spentThis += price;
                        countThis++;
                        held++;
                        moved.Units++;
                        remaining--;
                        drawn = wouldDraw;
                        books.NotePurchase(good.Id, price, good.Weight, TradeRules.FoodValue(good));
                        simWeight = books.Weight(sim);
                        if (livestock) { herdRoom--; books.NoteHerdTaken(); }
                        market.Staged(picked.At, price);
                        continue;
                    }

                    if (!market.Take(picked.At, price, out int cost)) break;
                    if (cost == 0) break;

                    books.NoteBought(good.Id, cost);
                    spentThis += cost;
                    countThis++;
                    held++;
                    moved.Units++;
                    remaining--;
                    drawn = wouldDraw;
                    if (livestock) herdRoom--;
                }
                if (moved.Units > unitsBefore)
                    market.Resold(picked.At, moved.Units - unitsBefore, drawn - drawnBefore);
            }
            return moved;
        }

        private static (int units, int cost, float profit) UnitsTheHoldLeftBehind(IBuyingMarket market, int at,
                                                                                  in Good good, int remaining,
                                                                                  int held, int drawn, int till,
                                                                                  (int count, int spent) taken,
                                                                                  float shareCap, Options s)
        {
            Func<int, int> ahead = market.PricesAhead(at);
            int budget = market.Spendable();
            int units = 0, cost = 0;
            float profit = 0f;
            while (remaining > 0)
            {
                int price = ahead(units);
                if (price <= 0) break;
                int wouldDraw = market.ResaleUpTo(at, held + 1);
                if (TradeRules.TheBuyerCouldNotPay(wouldDraw, till)) break;
                float realizable = TradeMath.Realizable(wouldDraw - drawn, s.ResaleSafetyFactor);
                if (!TradeMath.BuyAcceptable(price, realizable, s.MinProfitMargin)) break;
                if (TradeRules.WhatStopsBuying(good, price, budget, taken, held, shareCap, false, 0,
                                               market.Village && remaining <= 1, s) != Block.None) break;
                units++;
                remaining--;
                held++;
                drawn = wouldDraw;
                budget -= price;
                cost = TradeMath.AddedUp(cost, price);
                profit += realizable - price;
                taken = (taken.count + 1, taken.spent + price);
            }
            return (units, cost, profit);
        }

        internal static Block WhatStopsTheLot(IBuyingMarket market, Books books, bool sim, Options s,
                                              out Lot lot)
        {
            lot = default(Lot);
            lot.Stopper = -1;
            Block refused = Block.None;
            var shelf = new List<Pick>();
            for (int at = 0; at < market.Count; at++)
            {
                if (market.AmountAt(at) <= 0) continue;
                int units = market.TheirsToSell(at);
                if (units <= 0) continue;
                Good good = market.GoodAt(at);
                int unit = market.PriceToBuy(at);
                lot.Units += units;
                lot.Price = (int)Math.Min(int.MaxValue, (long)lot.Price + TradeMath.WorthOf(units, unit));
                lot.Weight += units * good.Weight;
                if (good.IsLivestock) lot.Herd += units;
                if (refused != Block.None) continue;
                if (unit <= 0) refused = Block.NoStock;
                else if (!market.MayBuy(at, good, out Block whyBuy)) refused = whyBuy;
                else
                {
                    shelf.Add(new Pick { At = at, Good = good });
                    continue;
                }
                lot.Stopper = at;
            }
            if (lot.Units == 0) return Block.NoStock;
            if (refused != Block.None) return refused;
            market.PriceTheMarketsFor(shelf);

            var inTheLot = new Dictionary<string, int>();
            int sellable = 0;
            foreach (Pick one in shelf)
            {
                Good good = one.Good;
                int units = market.TheirsToSell(one.At);
                int unit = market.PriceToBuy(one.At);
                inTheLot.TryGetValue(good.Id, out int before);
                inTheLot[good.Id] = before + units;

                int from = market.Carried(one.At) + books.Held(sim, good.Id) + before;
                if (!market.ResaleMarket(one.At, unit, from + units, out _)) continue;
                sellable++;
                int pays = TradeRules.WhatTheBuyerPays(market.ResaleUpTo(one.At, from),
                                                       market.ResaleUpTo(one.At, from + units),
                                                       market.ResaleTill(one.At));
                lot.Resale += TradeMath.Realizable(pays, s.ResaleSafetyFactor);
                market.Resold(one.At, units, pays);
            }
            lot.Weighed = true;

            if (sellable == 0) return Block.NoResaleMarket;
            if (!TradeMath.BuyAcceptable(lot.Price, lot.Resale, s.MinProfitMargin)) return Block.BelowMargin;
            if (lot.Price > market.Spendable()) return Block.BudgetSpent;
            if (lot.Herd > 0 && lot.Herd > market.HerdRoom() - books.HerdTaken(sim)) return Block.HerdFull;
            if (lot.Weight > 0.01f && lot.Weight > market.Room() - books.Weight(sim)) return Block.CarryWeight;
            return Block.None;
        }

        internal static Traded TakeTheLot(IBuyingMarket market, Books books, bool sim)
        {
            Traded moved = default(Traded);
            for (int at = 0; at < market.Count; at++)
            {
                if (market.AmountAt(at) <= 0) continue;
                Good good = market.GoodAt(at);
                for (int left = market.TheirsToSell(at); left > 0; left--)
                {
                    if (market.Stopped) return moved;
                    int price = market.PriceToBuy(at);
                    if (sim)
                    {
                        moved.SimGold += price;
                        moved.Units++;
                        books.NotePurchase(good.Id, price, good.Weight, TradeRules.FoodValue(good));
                        if (good.IsLivestock) books.NoteHerdTaken();
                        market.Staged(at, price);
                        continue;
                    }

                    if (!market.Take(at, price, out int cost) || cost == 0) return moved;
                    books.NoteBought(good.Id, cost);
                    moved.Units++;
                }
            }
            return moved;
        }

        internal static Traded SellThem(ISellingMarket market, Books books, bool sim, Options s,
                                        BlockTally tally)
        {
            Traded moved = default(Traded);
            int simTill = market.Till();

            for (int at = 0; at < market.Count; at++)
            {
                if (market.Stopped) break;
                if (books.Bought(sim, market.IdAt(at))) { tally.Note(Block.TradedHereAlready); continue; }
                Good good = market.GoodAt(at);
                if (!market.MaySell(at, good, out int keep, out Block stopped)) { tally.Note(stopped); continue; }

                int remaining = market.YoursToSell(at) - keep;
                if (remaining <= 0) { tally.Note(Block.TradedHereAlready); continue; }

                Basis basis = Basis.For(market.CostBasis(at), market.PurchasedUnits(at), market.PaidKeyAt(at),
                                        books, sim, s);

                int bestMarketFloor = 0, holdFor = 0;
                bool floorKnown = false;

                while (remaining > 0)
                {
                    int worth = basis.Unit(out bool askTheMarket);
                    if (askTheMarket) basis.UnpaidWorth = market.UnpaidWorth(at);
                    int mustBeat = TradeRules.WorthToBeat(good, worth, basis.UnpaidWorth);
                    int price = market.PriceToSell(at);
                    if (!TradeMath.ProfitAcceptable(mustBeat, price, s.MinProfitMargin))
                    {
                        tally.Note(Block.BelowMargin);
                        if (!basis.SkipTheUnitsYouPaidFor(ref remaining)) break;
                        continue;
                    }
                    if (!floorKnown)
                    {
                        floorKnown = true;
                        int bought = Math.Min(remaining, basis.PaidLeft);
                        if (s.HoldCargoForBestMarket > 0f && bought > 0 &&
                            market.ResaleMarket(at, bought, mustBeat, out int there, out int theyTake))
                        {
                            bestMarketFloor = TradeRules.BestMarketFloor(there, s.HoldCargoForBestMarket);
                            holdFor = Math.Min(theyTake, bought);
                        }
                    }
                    if (TradeRules.HeldForTheMark(price, bestMarketFloor, Math.Min(remaining, basis.PaidLeft), holdFor))
                    {
                        tally.Note(Block.BelowBestMarket);
                        if (!basis.SetTheBoughtUnitsAside(ref remaining)) break;
                        continue;
                    }
                    if (TradeRules.WhatTheTillCanPay(sim ? simTill : market.TillNow(),
                                                     market.Village) < price)
                    { tally.Note(Block.MerchantTillEmpty); break; }

                    if (sim)
                    {
                        simTill -= price;
                        moved.SimGold += price;
                        int credited = TradeMath.Credit(price, worth, basis.UnpaidWorth);
                        moved.Profit += credited;
                        if (market.TheGameGivesTradeXpFor(at)) moved.Earned += credited;
                        int herdRank = TradeRules.HerdShedRank(good);
                        books.NoteSale(good.Id, price,
                                       herdRank == TradeRules.RankHaulAnimal ? 0f : good.Weight,
                                       TradeRules.FoodValue(good));
                        if (herdRank >= 0 &&
                            Herding.TheGameCountsItAtOnce(herdRank == TradeRules.RankLivestock, market.OfAQuality(at)))
                            books.NoteShed(herdRank == TradeRules.RankHaulAnimal,
                                           herdRank != TradeRules.RankLivestock);
                        moved.Units++;
                        remaining--;
                        if (basis.SoldOne()) books.NotePaidDrawn(market.PaidKeyAt(at));
                        market.Staged(at, price);
                        continue;
                    }

                    if (!market.Give(at, price, out int proceeds)) break;
                    if (proceeds == 0) break;

                    if (basis.SoldOne()) market.RecordedSale(at);
                    books.NoteSold(good.Id);
                    moved.Units++;
                    int earned = TradeMath.Credit(proceeds, worth, basis.UnpaidWorth);
                    moved.Profit += earned;
                    if (market.TheGameGivesTradeXpFor(at)) moved.Earned += earned;
                    remaining--;
                }
            }
            return moved;
        }
    }
}
