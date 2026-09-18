using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class BuyPassTests
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
            internal int Amount = 1;
            internal int Price = 100;
            internal int Step;
            internal int Resale = 200;
            internal int ResaleStep;
            internal int BuyerTill;
            internal bool Elsewhere = true;
            internal int Carried;
        }

        private sealed class FakeMarket : IBuyingMarket
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
            internal bool InAVillage;
            internal readonly Dictionary<int, int> LedgerAsksFor = new Dictionary<int, int>();
            internal bool Halted;
            internal int RefuseAfter = -1;
            internal int PayNothingAfter = -1;
            internal int Described;
            internal int Ranked = -1;

            internal Stall Add(Good good, int amount = 1, int price = 100, int resale = 200)
            {
                var stall = new Stall { Good = good, Amount = amount, Price = price, Resale = resale };
                Stalls.Add(stall);
                return stall;
            }

            public int Count => Stalls.Count;

            public bool Stopped => Halted;

            public bool Village => InAVillage;

            public int AmountAt(int at) => Stalls[at].Amount;

            public Good GoodAt(int at)
            {
                Described++;
                return Stalls[at].Good;
            }

            public bool MayBuy(int at, in Good good, out Block why) =>
                TradeRules.MayBuy(good, false, Rules, default(Says), out why);

            public int TheLedgerAsksFor(int at) =>
                LedgerAsksFor.TryGetValue(at, out int rank) ? rank : 0;

            public int TheirsToSell(int at) =>
                System.Math.Min(Stalls[at].Amount,
                                Stalls[at].Amount - Ledger.Stocked(Sim, Stalls[at].Good.Id));

            public int Carried(int at) => Stalls[at].Carried;

            public void PriceTheMarketsFor(List<Pick> shelf) => Ranked = shelf.Count;

            public bool ResaleMarket(int at, int paid, out int price)
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

            public int PriceToBuy(int at) => Stalls[at].Price + Stalls[at].Step * Stalls[at].Carried;

            public int Spendable() => Purse;

            public float Room() => Cargo;

            public int HerdRoom() => Herd;

            public void Staged(int at, int price) => LaidOut.Add(Stalls[at].Good.Id);

            public bool Take(int at, int price, out int cost)
            {
                cost = 0;
                if (RefuseAfter >= 0 && Taken.Count >= RefuseAfter) { Halted = true; return false; }
                if (PayNothingAfter >= 0 && Taken.Count >= PayNothingAfter) return true;
                cost = price;
                Purse -= price;
                Cargo -= Stalls[at].Good.Weight;
                Stalls[at].Amount--;
                Stalls[at].Carried++;
                Taken.Add(Stalls[at].Good.Id);
                return true;
            }
        }

        private static Good Cargo(string id, float weight = 1f, int value = 100) =>
            new Good { Id = id, Name = id, IsTradeGood = true, Weight = weight, Value = value };

        private static Good Livestock(string id, float weight = 1f) =>
            new Good
            {
                Id = id, Name = id, HasHorse = true, IsLivestock = true, IsAnimal = true,
                Weight = weight, Value = 100
            };

        private sealed class Run
        {
            internal int Units;
            internal BlockTally Tally;
            internal Books Books;
        }

        private static Run Buy(FakeMarket market, float shareCap = 0f, bool sim = false,
                               Books books = null)
        {
            var run = new Run { Tally = new BlockTally(), Books = books ?? new Books() };
            market.Ledger = run.Books;
            market.Sim = sim;
            var stock = new List<Pick>();
            if (market.Spendable() > 0)
                stock = TradePass.WhatToBuy(market, run.Books, sim, shareCap, market.Rules, run.Tally);
            else run.Tally.Note(Block.BudgetSpent);
            run.Units = TradePass.BuyThem(stock, market, run.Books, sim, shareCap, market.Rules,
                                          run.Tally).Units;
            return run;
        }

        [Fact]
        public void A_good_the_buying_pass_took_is_written_into_the_books_for_the_other_half()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 2);
            Run run = Buy(market);
            Assert.Equal(2, run.Units);
            Assert.True(run.Books.Bought(false, "iron"));
            Assert.Equal((2, 200), run.Books.Purchases(false, "iron"));
        }

        [Fact]
        public void A_good_worth_reselling_is_bought_off_the_shelf()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 3);
            Run run = Buy(market);
            Assert.Equal(3, run.Units);
            Assert.Equal(new[] { "iron", "iron", "iron" }, market.Taken);
        }

        [Fact]
        public void A_good_with_nowhere_to_resell_it_is_left_on_the_shelf()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron")).Elsewhere = false;
            Run run = Buy(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.NoResaleMarket));
        }

        [Fact]
        public void A_price_that_misses_your_margin_is_left_on_the_shelf()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), price: 100, resale: 130);
            Run run = Buy(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowMargin));
        }

        [Fact]
        public void A_stack_is_bought_only_while_the_far_market_still_pays_for_one_more()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 10, price: 100, resale: 200).ResaleStep = 20;
            Run run = Buy(market);
            Assert.Equal(4, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowMargin));
        }

        [Fact]
        public void A_far_market_that_cannot_pay_for_the_whole_stack_stops_the_buying()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 10, price: 100, resale: 200).BuyerTill = 500;
            Run run = Buy(market);
            Assert.Equal(2, run.Units);
            Assert.True(run.Tally.Saw(Block.BuyerTillEmpty));
        }

        [Fact]
        public void What_you_already_carry_is_counted_against_the_far_market_before_you_buy_more()
        {
            var market = new FakeMarket();
            Stall stall = market.Add(Cargo("iron"), amount: 10, price: 100, resale: 200);
            stall.ResaleStep = 20;
            stall.Carried = 3;
            Run run = Buy(market);
            Assert.Equal(1, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowMargin));
        }

        [Fact]
        public void The_widest_margin_is_bought_first()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), price: 100, resale: 150);
            market.Add(Cargo("silver"), price: 100, resale: 400);
            market.Add(Cargo("wool"), price: 100, resale: 200);
            Buy(market);
            Assert.Equal(new[] { "silver", "wool", "iron" }, market.Taken);
        }

        [Fact]
        public void The_shelf_is_priced_once_for_every_good_that_survives_it()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"));
            market.Add(Cargo("wool"));
            market.Add(Cargo("stone")).Amount = 0;
            Buy(market);
            Assert.Equal(2, market.Ranked);
        }

        [Fact]
        public void A_good_is_described_once_however_many_units_are_bought()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 8);
            Run run = Buy(market);
            Assert.Equal(8, run.Units);
            Assert.Equal(1, market.Described);
        }

        [Fact]
        public void An_empty_purse_never_reaches_the_shelf()
        {
            var market = new FakeMarket { Purse = 0 };
            market.Add(Cargo("iron"));
            Run run = Buy(market);
            Assert.Equal(0, run.Units);
            Assert.Equal(0, market.Described);
            Assert.Equal(Block.BudgetSpent, run.Tally.Dominant());
        }

        [Fact]
        public void The_purse_stops_the_pass_when_it_runs_down()
        {
            var market = new FakeMarket { Purse = 250 };
            market.Add(Cargo("iron"), amount: 10, price: 100);
            Run run = Buy(market);
            Assert.Equal(2, run.Units);
            Assert.True(run.Tally.Saw(Block.BudgetSpent));
        }

        [Fact]
        public void A_full_cargo_stops_the_pass()
        {
            var market = new FakeMarket { Cargo = 3.5f };
            market.Add(Cargo("iron", weight: 1f), amount: 10);
            Run run = Buy(market);
            Assert.Equal(3, run.Units);
            Assert.True(run.Tally.Saw(Block.CarryWeight));
        }

        [Fact]
        public void A_full_herd_leaves_livestock_where_it_stands()
        {
            var market = new FakeMarket { Herd = 0 };
            market.Add(Livestock("cow"), amount: 4);
            Run run = Buy(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.HerdFull));
        }

        [Fact]
        public void The_herd_takes_only_as_many_head_as_it_has_room_for()
        {
            var market = new FakeMarket { Herd = 2 };
            market.Add(Livestock("cow"), amount: 9);
            Run run = Buy(market);
            Assert.Equal(2, run.Units);
            Assert.True(run.Tally.Saw(Block.HerdFull));
        }

        [Fact]
        public void The_buy_cap_per_item_stops_at_the_count_you_set()
        {
            var market = new FakeMarket();
            market.Rules.BuyCapPerItem = 3;
            market.Add(Cargo("iron"), amount: 20);
            Run run = Buy(market);
            Assert.Equal(3, run.Units);
            Assert.True(run.Tally.Saw(Block.ItemCountCap));
        }

        [Fact]
        public void The_buy_cap_per_item_in_denars_stops_at_the_gold_you_set()
        {
            var market = new FakeMarket();
            market.Rules.BuyValueCapPerItem = 250;
            market.Add(Cargo("iron"), amount: 20, price: 100);
            Run run = Buy(market);
            Assert.Equal(2, run.Units);
            Assert.True(run.Tally.Saw(Block.ItemValueCap));
        }

        [Fact]
        public void A_village_keeps_the_last_of_each_good()
        {
            var market = new FakeMarket { InAVillage = true };
            market.Add(Cargo("grape"), amount: 3);
            Run run = Buy(market);
            Assert.Equal(2, run.Units);
            Assert.True(run.Tally.Saw(Block.VillageLastUnit));
        }

        [Fact]
        public void A_town_buys_the_last_of_a_good()
        {
            var market = new FakeMarket();
            market.Add(Cargo("grape"), amount: 3);
            Assert.Equal(3, Buy(market).Units);
        }

        [Fact]
        public void Carrying_as_many_as_you_allow_keeps_a_good_off_the_shelf()
        {
            var market = new FakeMarket();
            market.Rules.MaxHeldPerItem = 5;
            market.Add(Cargo("iron"), amount: 9).Carried = 5;
            Run run = Buy(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.HeldEnough));
        }

        [Fact]
        public void A_share_of_the_cargo_keeps_a_heavy_good_off_the_shelf()
        {
            var market = new FakeMarket();
            market.Add(Cargo("marble", weight: 10f), amount: 9).Carried = 4;
            Run run = Buy(market, shareCap: 40f);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.HeldEnough));
        }

        [Fact]
        public void A_good_you_already_sold_here_is_not_bought_back()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 4);
            var books = new Books();
            books.NoteSold("iron");
            Run run = Buy(market, books: books);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.TradedHereAlready));
        }

        [Fact]
        public void The_never_buy_list_keeps_a_good_off_the_shelf()
        {
            var market = new FakeMarket();
            market.Rules.NeverBuyItems = "iron";
            market.Add(Cargo("iron"), amount: 4);
            Run run = Buy(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.NeverList));
        }

        [Fact]
        public void A_price_that_climbs_as_you_buy_stops_the_pass_where_it_stops_paying()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 20, price: 100, resale: 200).Step = 20;
            Run run = Buy(market);
            Assert.Equal(3, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowMargin));
        }

        [Fact]
        public void A_trade_that_goes_the_wrong_way_stops_the_whole_pass()
        {
            var market = new FakeMarket { RefuseAfter = 1 };
            market.Add(Cargo("iron"), amount: 5);
            market.Add(Cargo("wool"), amount: 5, resale: 400);
            Run run = Buy(market);
            Assert.Equal(1, run.Units);
            Assert.Single(market.Taken);
        }

        [Fact]
        public void A_trade_that_pays_nothing_leaves_the_rest_of_that_good_alone()
        {
            var market = new FakeMarket { PayNothingAfter = 2 };
            market.Add(Cargo("iron"), amount: 6);
            market.Add(Cargo("wool"), amount: 2, resale: 400);
            Run run = Buy(market);
            Assert.Equal(new[] { "wool", "wool" }, market.Taken.GetRange(0, 2));
            Assert.Equal(2, run.Units);
        }

        [Fact]
        public void A_dry_run_lays_the_deal_out_and_moves_nothing()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 3);
            Run run = Buy(market, sim: true);
            Assert.Equal(3, run.Units);
            Assert.Empty(market.Taken);
            Assert.Equal(new[] { "iron", "iron", "iron" }, market.LaidOut);
            Assert.Equal(100000, market.Purse);
        }

        [Fact]
        public void A_dry_run_books_what_it_would_have_spent_and_carried()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron", weight: 2f), amount: 3, price: 100);
            Run run = Buy(market, sim: true);
            Assert.Equal(3, run.Units);
            Assert.Equal((3, 300), run.Books.Purchases(true, "iron"));
            Assert.Equal(3, run.Books.Held(true, "iron"));
            Assert.Equal(6f, run.Books.Weight(true));
        }

        [Fact]
        public void A_dry_run_fills_the_cargo_as_it_goes()
        {
            var market = new FakeMarket { Cargo = 3.5f };
            market.Add(Cargo("iron", weight: 1f), amount: 10);
            Run run = Buy(market, sim: true);
            Assert.Equal(3, run.Units);
            Assert.True(run.Tally.Saw(Block.CarryWeight));
        }

        [Fact]
        public void A_dry_run_counts_the_head_it_would_have_driven()
        {
            var market = new FakeMarket { Herd = 2 };
            market.Add(Livestock("cow"), amount: 9);
            Run run = Buy(market, sim: true);
            Assert.Equal(2, run.Units);
            Assert.Equal(2, run.Books.HerdTaken(true));
        }

        [Fact]
        public void A_second_dry_run_over_the_same_books_takes_the_first_one_into_account()
        {
            var market = new FakeMarket();
            market.Rules.BuyCapPerItem = 3;
            market.Add(Cargo("iron"), amount: 20);
            var books = new Books();
            Assert.Equal(3, Buy(market, sim: true, books: books).Units);
            Assert.Equal(0, Buy(market, sim: true, books: books).Units);
        }
    }
}
