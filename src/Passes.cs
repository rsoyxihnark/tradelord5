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
    }

    internal struct Pick
    {
        internal int At;
        internal Good Good;
        internal float Realizable;
        internal float Margin;
        internal bool Asked;
    }

    internal static class Picks
    {
        internal static void BestMarginFirst(List<Pick> stock) =>
            stock?.Sort((x, y) => y.Margin.CompareTo(x.Margin));

        internal static void WhatTheLedgerAskedForFirst(List<Pick> stock)
        {
            if (stock == null || stock.Count < 2) return;
            var asked = new List<Pick>();
            var rest = new List<Pick>();
            foreach (Pick one in stock) (one.Asked ? asked : rest).Add(one);
            if (asked.Count == 0 || rest.Count == 0) return;
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
        bool TheLedgerAsksFor(int at);
        int TheirsToSell(int at);
        int Carried(int at);
        void PriceTheMarketsFor(List<Pick> shelf);
        bool ResaleMarket(int at, out int price);
        int PriceToBuy(int at);
        int Spendable();
        float Room();
        int HerdRoom();
        void Staged(int at, int price);
        bool Take(int at, int price, out int cost);
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
        int PurchasedUnits(int at);
        int UnpaidWorth(int at);
        bool ResaleMarket(int at, out int price);
        int PriceToSell(int at);
        bool TheGameGivesTradeXpFor(int at);
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
                if (!TradeRules.ResaleAllowed(good, s)) { tally.Note(Block.CategoryPolicy); continue; }
                if (books.Sold(sim, good.Id)) { tally.Note(Block.TradedHereAlready); continue; }
                if (market.TheirsToSell(at) <= 0) { tally.Note(Block.NoStock); continue; }
                int held = market.Carried(at) + books.Held(sim, good.Id);
                if (holdCap > 0 && held >= holdCap) { tally.Note(Block.HeldEnough); continue; }
                if (shareCap > 0f && (held + 1) * good.Weight > shareCap) { tally.Note(Block.HeldEnough); continue; }
                shelf.Add(new Pick { At = at, Good = good, Asked = market.TheLedgerAsksFor(at) });
            }
            market.PriceTheMarketsFor(shelf);

            var stock = new List<Pick>();
            foreach (Pick one in shelf)
            {
                if (!market.ResaleMarket(one.At, out int elsewhere)) { tally.Note(Block.NoResaleMarket); continue; }
                int here = market.PriceToBuy(one.At);
                if (here <= 0) { tally.Note(Block.NoStock); continue; }
                float realizable = TradeMath.Realizable(elsewhere, s.ResaleSafetyFactor);
                if (!TradeMath.BuyAcceptable(here, realizable, s.MinProfitMargin)) { tally.Note(Block.BelowMargin); continue; }
                Pick picked = one;
                picked.Realizable = realizable;
                picked.Margin = (realizable - here) / here;
                stock.Add(picked);
            }
            Picks.BestMarginFirst(stock);
            if (s.FollowTheLedgerFirst) Picks.WhatTheLedgerAskedForFirst(stock);
            return stock;
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

                while (remaining > 0)
                {
                    int price = market.PriceToBuy(picked.At);
                    if (!TradeMath.BuyAcceptable(price, picked.Realizable, s.MinProfitMargin))
                    { tally.Note(Block.BelowMargin); break; }
                    Block capped = TradeRules.WhatStopsBuying(good, price, market.Spendable(),
                                                              (countThis, spentThis), held, shareCap,
                                                              livestock, herdRoom,
                                                              market.Village && remaining <= 1, s);
                    if (capped != Block.None) { tally.Note(capped); break; }
                    if (TradeRules.NoRoomForOneMore(good, market.Room() - simWeight))
                    { tally.Note(Block.CarryWeight); break; }

                    if (sim)
                    {
                        moved.SimGold += price;
                        spentThis += price;
                        countThis++;
                        held++;
                        moved.Units++;
                        remaining--;
                        simWeight += good.Weight;
                        books.NotePurchase(good.Id, price, good.Weight, TradeRules.FoodValue(good));
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
                    if (livestock) herdRoom--;
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

                Basis basis = Basis.For(market.CostBasis(at), market.PurchasedUnits(at), good.Id,
                                        books, sim, s);

                int bestMarketFloor = 0;
                bool floorKnown = false;

                while (remaining > 0)
                {
                    int worth = basis.Unit(out bool askTheMarket);
                    if (askTheMarket) basis.UnpaidWorth = market.UnpaidWorth(at);
                    int mustBeat = TradeRules.WorthToBeat(good, worth, basis.UnpaidWorth);
                    int holdFloor = 0;
                    if (s.PreferBestSellTown)
                    {
                        if (!floorKnown)
                        {
                            floorKnown = true;
                            if (market.ResaleMarket(at, out int elsewhere))
                                bestMarketFloor = TradeRules.BestMarketFloor(elsewhere, s.BestSellTownTolerance);
                        }
                        holdFloor = bestMarketFloor;
                    }
                    int price = market.PriceToSell(at);
                    if (TradeRules.BelowTheBestMarket(price, holdFloor))
                    { tally.Note(Block.BelowBestMarket); break; }
                    if (!TradeMath.ProfitAcceptable(mustBeat, price, s.MinProfitMargin))
                    {
                        tally.Note(Block.BelowMargin);
                        if (!basis.SkipTheUnitsYouPaidFor(ref remaining)) break;
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
                        if (herdRank >= 0)
                            books.NoteShed(herdRank == TradeRules.RankHaulAnimal,
                                           herdRank != TradeRules.RankLivestock);
                        moved.Units++;
                        remaining--;
                        if (basis.SoldOne()) books.NotePaidDrawn(good.Id);
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
