using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class LotTests
    {
        private struct Says : IWhatTheGameSays
        {
            public bool Locked() => false;

            public bool Smeltable() => false;

            public bool PartsAllLearned() => true;
        }

        private sealed class Stall
        {
            internal Good Good;
            internal int Amount;
            internal int Price;
            internal int Resale;
            internal int ResaleStep;
            internal int BuyerTill;
            internal bool Elsewhere = true;
            internal int Carried;
            internal bool Offered = true;
        }

        private sealed class Offer : IBuyingMarket
        {
            internal readonly List<Stall> Stalls = new List<Stall>();
            internal readonly List<string> Taken = new List<string>();
            internal readonly List<string> LaidOut = new List<string>();
            internal Options Rules = new Options();
            internal Books Ledger = new Books();
            internal bool Sim;
            internal int Purse = 100000;
            internal float Cargo = 1000f;
            internal int Herd = 100;
            internal bool Halted;
            internal int RefuseAfter = -1;

            internal Stall Add(Good good, int amount, int price, int resale)
            {
                var stall = new Stall { Good = good, Amount = amount, Price = price, Resale = resale };
                Stalls.Add(stall);
                return stall;
            }

            public int Count => Stalls.Count;

            public bool Stopped => Halted;

            public bool Village => false;

            public int AmountAt(int at) => Stalls[at].Offered ? Stalls[at].Amount : 0;

            public Good GoodAt(int at) => Stalls[at].Good;

            public bool MayBuy(int at, in Good good, out Block why) =>
                TradeRules.MayBuy(good, false, Rules, default(Says), out why, wholeOffer: true);

            public int TheLedgerAsksFor(int at) => 0;

            public int TheirsToSell(int at) =>
                System.Math.Min(Stalls[at].Amount,
                                Stalls[at].Amount - Ledger.Stocked(Sim, Stalls[at].Good.Id));

            public int Carried(int at) => Stalls[at].Carried;

            public void PriceTheMarketsFor(List<Pick> shelf) { }

            public bool ResaleMarket(int at, int paid, int units, out int price)
            {
                price = Stalls[at].Resale;
                return Stalls[at].Elsewhere;
            }

            public int ResaleUpTo(int at, int units)
            {
                if (units <= 0 || !Stalls[at].Elsewhere) return 0;
                int total = 0;
                for (int u = 0; u < units; u++)
                {
                    int price = Stalls[at].Resale - Stalls[at].ResaleStep * u;
                    if (price <= 0) break;
                    total += price;
                }
                return total;
            }

            public int ResaleTill(int at) => Stalls[at].BuyerTill;

            public int PriceToBuy(int at) => Stalls[at].Price;

            public int Spendable() => Purse;

            public float Room() => Cargo;

            public int HerdRoom() => Herd;

            public void Staged(int at, int price) => LaidOut.Add(Stalls[at].Good.Id);

            public bool Take(int at, int price, out int cost)
            {
                cost = 0;
                if (RefuseAfter >= 0 && Taken.Count >= RefuseAfter) { Halted = true; return false; }
                cost = price;
                Purse -= price;
                Cargo -= Stalls[at].Good.Weight;
                Stalls[at].Amount--;
                Stalls[at].Carried++;
                Taken.Add(Stalls[at].Good.Id);
                return true;
            }
        }

        private static Good Cargo(string id, float weight = 1f) =>
            new Good { Id = id, Name = id, IsTradeGood = true, Weight = weight, Value = 100 };

        private static Good Livestock(string id) =>
            new Good { Id = id, Name = id, HasHorse = true, IsLivestock = true, IsAnimal = true, Value = 100 };

        private static Good Mount(string id) =>
            new Good { Id = id, Name = id, HasHorse = true, IsMountable = true, IsAnimal = true, Value = 300 };

        private static Offer Villagers() => new Offer();

        private static Block Judge(Offer offer, out Lot lot) =>
            TradePass.WhatStopsTheLot(offer, offer.Ledger, offer.Sim, offer.Rules, out lot);

        [Fact]
        public void An_offer_that_sells_on_for_more_than_it_costs_is_taken_whole()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides"), amount: 6, price: 40, resale: 90);
            offer.Add(Cargo("clay"), amount: 4, price: 20, resale: 50);
            Assert.Equal(Block.None, Judge(offer, out Lot lot));
            Assert.Equal(10, lot.Units);
            Assert.Equal(6 * 40 + 4 * 20, lot.Price);
            Assert.True(lot.Weighed);

            Traded moved = TradePass.TakeTheLot(offer, offer.Ledger, sim: false);
            Assert.Equal(10, moved.Units);
            Assert.Equal(0, offer.Stalls[0].Amount);
            Assert.Equal(0, offer.Stalls[1].Amount);
            Assert.Equal((6, 240), offer.Ledger.Purchases(false, "hides"));
            Assert.Equal((4, 80), offer.Ledger.Purchases(false, "clay"));
        }

        [Fact]
        public void An_offer_of_a_good_you_buy_only_is_taken_when_it_clears_your_margin()
        {
            var offer = Villagers();
            offer.Rules.LivestockPolicy = Options.PolicyBuyOnly;
            offer.Add(Livestock("cow"), amount: 3, price: 40, resale: 90);
            Assert.Equal(Block.None, Judge(offer, out Lot lot));
            Assert.Equal(3, lot.Units);

            offer.Rules.LivestockPolicy = Options.PolicyIgnore;
            Assert.Equal(Block.CategoryPolicy, Judge(offer, out _));
        }

        [Fact]
        public void One_good_that_loses_money_is_carried_by_the_rest_of_the_offer()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides"), amount: 10, price: 40, resale: 100);
            offer.Add(Cargo("clay"), amount: 5, price: 30, resale: 20);
            Assert.Equal(Block.None, Judge(offer, out Lot lot));
            Assert.Equal(15, lot.Units);
        }

        [Fact]
        public void An_offer_that_misses_your_margin_as_a_whole_is_left_for_you()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides"), amount: 5, price: 40, resale: 60);
            offer.Add(Cargo("clay"), amount: 5, price: 30, resale: 20);
            Assert.Equal(Block.BelowMargin, Judge(offer, out _));
        }

        [Fact]
        public void The_margin_is_counted_after_the_safety_factor()
        {
            var offer = Villagers();
            offer.Rules.MinProfitMargin = 0.15f;
            offer.Rules.ResaleSafetyFactor = 0.85f;
            offer.Add(Cargo("hides"), amount: 10, price: 100, resale: 130);
            Assert.Equal(Block.BelowMargin, Judge(offer, out Lot lot));
            Assert.Equal(1105f, lot.Resale, 3);
            offer.Stalls[0].Resale = 136;
            Assert.Equal(Block.None, Judge(offer, out _));
        }

        [Fact]
        public void A_good_with_nowhere_to_sell_it_counts_for_nothing_in_the_offer()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides"), amount: 5, price: 40, resale: 200);
            Stall dead = offer.Add(Cargo("clay"), amount: 5, price: 40, resale: 200);
            dead.Elsewhere = false;
            Assert.Equal(Block.None, Judge(offer, out Lot lot));
            Assert.Equal(850f, lot.Resale, 3);
            dead.Amount = 30;
            Assert.Equal(Block.BelowMargin, Judge(offer, out _));
        }

        [Fact]
        public void A_buyer_that_pays_less_for_every_unit_is_asked_for_the_whole_offer()
        {
            var offer = Villagers();
            Stall hides = offer.Add(Cargo("hides"), amount: 10, price: 41, resale: 100);
            hides.ResaleStep = 10;
            Assert.Equal(Block.BelowMargin, Judge(offer, out Lot lot));
            Assert.Equal((100 + 90 + 80 + 70 + 60 + 50 + 40 + 30 + 20 + 10) * 0.85f, lot.Resale, 3);
        }

        [Fact]
        public void What_you_already_carry_is_sold_first_so_the_offer_fetches_the_lower_prices()
        {
            var offer = Villagers();
            Stall hides = offer.Add(Cargo("hides"), amount: 2, price: 40, resale: 100);
            hides.ResaleStep = 10;
            hides.Carried = 3;
            Judge(offer, out Lot lot);
            Assert.Equal((70 + 60) * 0.85f, lot.Resale, 3);
        }

        [Fact]
        public void A_buyer_that_cannot_pay_for_it_all_is_counted_only_for_what_it_can_pay()
        {
            var offer = Villagers();
            Stall hides = offer.Add(Cargo("hides"), amount: 10, price: 40, resale: 100);
            hides.BuyerTill = 450;
            Assert.Equal(Block.BelowMargin, Judge(offer, out Lot lot));
            Assert.Equal(450 * 0.85f, lot.Resale, 3);
        }

        [Fact]
        public void An_offer_you_cannot_pay_for_whole_is_left_for_you()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides"), amount: 10, price: 40, resale: 100);
            offer.Purse = 399;
            Assert.Equal(Block.BudgetSpent, Judge(offer, out _));
            offer.Purse = 400;
            Assert.Equal(Block.None, Judge(offer, out _));
        }

        [Fact]
        public void An_offer_that_does_not_fit_in_the_hold_is_left_for_you()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides", weight: 2f), amount: 10, price: 40, resale: 100);
            offer.Cargo = 19f;
            Assert.Equal(Block.CarryWeight, Judge(offer, out _));
            offer.Cargo = 20f;
            Assert.Equal(Block.None, Judge(offer, out _));
        }

        [Fact]
        public void Livestock_in_the_offer_needs_room_in_the_herd_and_none_in_the_hold()
        {
            var offer = Villagers();
            offer.Add(Livestock("cow"), amount: 4, price: 100, resale: 300);
            offer.Cargo = 0f;
            offer.Herd = 3;
            Assert.Equal(Block.HerdFull, Judge(offer, out Lot lot));
            Assert.Equal(4, lot.Herd);
            offer.Herd = 4;
            Assert.Equal(Block.None, Judge(offer, out _));
        }

        [Fact]
        public void One_good_you_never_buy_keeps_the_whole_offer_off()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides"), amount: 10, price: 40, resale: 100);
            offer.Add(Cargo("olives"), amount: 2, price: 20, resale: 50);
            offer.Rules.NeverBuyItems = "olives";
            Assert.Equal(Block.NeverList, Judge(offer, out Lot lot));
            Assert.Equal(12, lot.Units);
            Assert.Equal(10 * 40 + 2 * 20, lot.Price);
            Assert.False(lot.Weighed);
            Assert.Equal(1, lot.Stopper);
            Assert.Empty(offer.Taken);
        }

        [Fact]
        public void An_offer_turned_down_as_a_whole_names_no_good_in_it()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides"), amount: 5, price: 40, resale: 50);
            Assert.Equal(Block.BelowMargin, Judge(offer, out Lot lot));
            Assert.Equal(-1, lot.Stopper);
            offer.Stalls[0].Resale = 200;
            offer.Purse = 10;
            Assert.Equal(Block.BudgetSpent, Judge(offer, out lot));
            Assert.Equal(-1, lot.Stopper);
        }

        [Fact]
        public void Grain_in_the_offer_is_taken_with_the_rest_even_while_grain_is_left_alone()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides"), amount: 10, price: 40, resale: 100);
            Stall grain = offer.Add(new Good { Id = "grain", Name = "grain", IsTradeGood = true, IsFood = true,
                                               IsGrain = true, Weight = 1f, Value = 10 },
                                    amount: 5, price: 10, resale: 30);
            offer.Rules.NeverBuyGrain = true;
            Assert.Equal(Block.None, Judge(offer, out Lot lot));
            Assert.Equal(15, lot.Units);
            TradePass.TakeTheLot(offer, offer.Ledger, sim: false);
            Assert.Equal(0, grain.Amount);
        }

        [Fact]
        public void A_riding_horse_in_the_offer_keeps_it_off_because_TradeLord_never_trades_one()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides"), amount: 10, price: 40, resale: 100);
            offer.Add(Mount("steppe_horse"), amount: 1, price: 200, resale: 400);
            Assert.Equal(Block.MountOrHaulAnimal, Judge(offer, out Lot lot));
            Assert.Equal(1, lot.Stopper);
        }

        [Fact]
        public void What_the_villagers_keep_out_of_their_offer_is_left_out_of_it_here_too()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides"), amount: 10, price: 40, resale: 100);
            Stall mule = offer.Add(Mount("mule"), amount: 3, price: 200, resale: 400);
            mule.Offered = false;
            Assert.Equal(Block.None, Judge(offer, out Lot lot));
            Assert.Equal(10, lot.Units);
            TradePass.TakeTheLot(offer, offer.Ledger, sim: false);
            Assert.Equal(3, mule.Amount);
            Assert.DoesNotContain("mule", offer.Taken);
        }

        [Fact]
        public void The_buy_cap_per_item_never_splits_the_offer_of_one_good()
        {
            var offer = Villagers();
            offer.Rules.BuyCapPerItem = 8;
            offer.Add(Cargo("hides"), amount: 40, price: 40, resale: 100);
            Assert.Equal(Block.None, Judge(offer, out Lot lot));
            Assert.Equal(40, lot.Units);
            Assert.Equal(40, TradePass.TakeTheLot(offer, offer.Ledger, sim: false).Units);
        }

        [Fact]
        public void The_value_cap_per_item_never_holds_the_offer_back()
        {
            var offer = Villagers();
            offer.Rules.BuyValueCapPerItem = 300;
            offer.Add(Cargo("hides"), amount: 8, price: 40, resale: 100);
            Assert.Equal(Block.None, Judge(offer, out _));
        }

        [Fact]
        public void The_most_you_hold_of_one_good_never_holds_the_offer_back()
        {
            var offer = Villagers();
            offer.Rules.MaxHeldPerItem = 10;
            Stall hides = offer.Add(Cargo("hides"), amount: 8, price: 40, resale: 100);
            hides.Carried = 9;
            Assert.Equal(Block.None, Judge(offer, out _));
        }

        [Fact]
        public void The_share_of_the_hold_one_good_may_fill_never_holds_the_offer_back()
        {
            var offer = Villagers();
            offer.Rules.MaxHeldShare = 0.01f;
            offer.Add(Cargo("hides", weight: 2f), amount: 10, price: 40, resale: 100);
            Assert.Equal(Block.None, Judge(offer, out _));
        }

        [Fact]
        public void The_same_good_in_two_lots_runs_on_down_the_buyers_prices()
        {
            var offer = Villagers();
            Stall first = offer.Add(Cargo("hides"), amount: 5, price: 10, resale: 100);
            Stall second = offer.Add(Cargo("hides"), amount: 5, price: 10, resale: 100);
            first.ResaleStep = 10;
            second.ResaleStep = 10;
            Assert.Equal(Block.None, Judge(offer, out Lot lot));
            Assert.Equal((100 + 90 + 80 + 70 + 60 + 50 + 40 + 30 + 20 + 10) * 0.85f, lot.Resale, 3);
        }

        [Fact]
        public void Max_spend_per_visit_never_holds_the_offer_back_while_the_purse_can_pay()
        {
            var offer = Villagers();
            offer.Rules.MaxSpendPerVisit = 100;
            offer.Add(Cargo("fur"), amount: 4, price: 400, resale: 1300);
            offer.Purse = 1600;
            Assert.Equal(Block.None, Judge(offer, out Lot lot));
            Assert.Equal(1600, lot.Price);
        }

        [Fact]
        public void Villagers_with_nothing_to_offer_have_nothing_to_judge()
        {
            var offer = Villagers();
            Stall mule = offer.Add(Mount("mule"), amount: 2, price: 200, resale: 400);
            mule.Offered = false;
            Assert.Equal(Block.NoStock, Judge(offer, out Lot lot));
            Assert.Equal(0, lot.Units);
        }

        [Fact]
        public void A_dry_run_writes_the_offer_into_its_own_books_and_takes_nothing()
        {
            var offer = Villagers();
            offer.Sim = true;
            offer.Add(Cargo("hides", weight: 2f), amount: 3, price: 40, resale: 100);
            offer.Add(Livestock("cow"), amount: 2, price: 100, resale: 300);
            Traded moved = TradePass.TakeTheLot(offer, offer.Ledger, sim: true);
            Assert.Equal(5, moved.Units);
            Assert.Equal(3 * 40 + 2 * 100, moved.SimGold);
            Assert.Empty(offer.Taken);
            Assert.Equal(5, offer.LaidOut.Count);
            Assert.Equal(6f, offer.Ledger.Weight(true), 3);
            Assert.Equal(2, offer.Ledger.HerdTaken(true));
            Assert.Equal(3, offer.Stalls[0].Amount);
        }

        [Fact]
        public void A_trade_the_game_turned_round_stops_the_offer_where_it_stood()
        {
            var offer = Villagers();
            offer.Add(Cargo("hides"), amount: 5, price: 40, resale: 100);
            offer.RefuseAfter = 2;
            Traded moved = TradePass.TakeTheLot(offer, offer.Ledger, sim: false);
            Assert.Equal(2, moved.Units);
            Assert.Equal(3, offer.Stalls[0].Amount);
        }

        [Fact]
        public void The_whole_offer_is_what_the_buyer_pays_for_it_and_never_more_than_the_till()
        {
            Assert.Equal(300, TradeRules.WhatTheBuyerPays(100, 400, 0));
            Assert.Equal(200, TradeRules.WhatTheBuyerPays(100, 400, 300));
            Assert.Equal(0, TradeRules.WhatTheBuyerPays(500, 800, 300));
            Assert.Equal(0, TradeRules.WhatTheBuyerPays(0, 0, 0));
        }
    }
}
