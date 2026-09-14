using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class SellPassTests
    {
        private struct Says : IWhatTheGameSays
        {
            public bool Locked() => false;

            public bool Smeltable() => false;

            public bool PartsAllLearned() => true;
        }

        private sealed class Load
        {
            internal Good Good;
            internal int Amount = 1;
            internal int Price = 200;
            internal int Basis;
            internal int Purchased;
            internal int Worth = 100;
            internal int Resale;
            internal bool Elsewhere;
            internal int Reserved;
        }

        private sealed class FakeMarket : ISellingMarket
        {
            internal readonly List<Load> Cargo = new List<Load>();
            internal readonly List<string> Given = new List<string>();
            internal readonly List<string> LaidOut = new List<string>();
            internal readonly List<string> Recorded = new List<string>();
            internal Options Rules = new Options();
            internal Books Ledger = new Books();
            internal bool Sim;
            internal int Till = 100000;
            internal bool Halted;
            internal int RefuseAfter = -1;
            internal int PayNothingAfter = -1;
            internal int Described;
            internal int WorthAsked;

            internal Load Add(Good good, int amount = 1, int price = 200)
            {
                var load = new Load { Good = good, Amount = amount, Price = price };
                Cargo.Add(load);
                return load;
            }

            public int Count => Cargo.Count;

            public bool Stopped => Halted;

            public string IdAt(int at) => Cargo[at].Good.Id;

            public Good GoodAt(int at)
            {
                Described++;
                return Cargo[at].Good;
            }

            public bool MaySell(int at, in Good good, out int keep, out Block why)
            {
                SellFacts facts = default(SellFacts);
                facts.FoodHeld = Cargo[at].Reserved;
                facts.QuestsReadable = true;
                SellVerdict said = TradeRules.MaySell(good, Cargo[at].Amount, facts, Rules,
                                                      default(Says));
                keep = said.KeepCount;
                why = said.Why;
                return said.Allowed;
            }

            public int YoursToSell(int at) =>
                System.Math.Min(Cargo[at].Amount,
                                Cargo[at].Amount + Ledger.Held(Sim, Cargo[at].Good.Id));

            public int CostBasis(int at) => Cargo[at].Basis;

            public int PurchasedUnits(int at) => Cargo[at].Purchased;

            public int UnpaidWorth(int at)
            {
                WorthAsked++;
                return Cargo[at].Worth;
            }

            public bool ResaleMarket(int at, out int price)
            {
                price = Cargo[at].Resale;
                return Cargo[at].Elsewhere;
            }

            public int PriceToSell(int at) => Cargo[at].Price;

            int ISellingMarket.Till() => Till;

            public int TillNow() => Till;

            public void Staged(int at, int price) => LaidOut.Add(Cargo[at].Good.Id);

            public bool Give(int at, int price, out int proceeds)
            {
                proceeds = 0;
                if (RefuseAfter >= 0 && Given.Count >= RefuseAfter) { Halted = true; return false; }
                if (PayNothingAfter >= 0 && Given.Count >= PayNothingAfter) return true;
                proceeds = price;
                Till -= price;
                Cargo[at].Amount--;
                Given.Add(Cargo[at].Good.Id);
                return true;
            }

            public void RecordedSale(int at) => Recorded.Add(Cargo[at].Good.Id);
        }

        private static Good Cargo(string id, float weight = 1f, int value = 100) =>
            new Good { Id = id, Name = id, IsTradeGood = true, Weight = weight, Value = value };

        private static Good Ration(string id) =>
            new Good { Id = id, Name = id, IsTradeGood = true, IsFood = true, Weight = 1f, Value = 20 };

        private sealed class Run
        {
            internal int Units;
            internal int Profit;
            internal int SimGold;
            internal BlockTally Tally;
            internal Books Books;
        }

        private static Run Sell(FakeMarket market, bool sim = false, Books books = null)
        {
            var run = new Run { Tally = new BlockTally(), Books = books ?? new Books() };
            market.Ledger = run.Books;
            market.Sim = sim;
            Traded moved = TradePass.SellThem(market, run.Books, sim, market.Rules, run.Tally);
            run.Units = moved.Units;
            run.Profit = moved.Profit;
            run.SimGold = moved.SimGold;
            return run;
        }

        [Fact]
        public void Cargo_the_selling_pass_moved_is_written_into_the_books_for_the_other_half()
        {
            var market = new FakeMarket();
            Load load = market.Add(Cargo("iron"), amount: 2, price: 200);
            load.Basis = 100;
            load.Purchased = 2;
            Run run = Sell(market);
            Assert.Equal(2, run.Units);
            Assert.True(run.Books.Sold(false, "iron"));
        }

        [Fact]
        public void A_good_the_buying_pass_took_here_is_left_alone_by_the_selling_pass()
        {
            var books = new Books();
            books.NoteBought("iron", 100);
            var market = new FakeMarket();
            Load load = market.Add(Cargo("iron"), amount: 3, price: 200);
            load.Basis = 100;
            load.Purchased = 3;
            Run run = Sell(market, books: books);
            Assert.Equal(0, run.Units);
            Assert.Empty(market.Given);
            Assert.True(run.Tally.Saw(Block.TradedHereAlready));
        }

        [Fact]
        public void Cargo_that_beats_what_you_paid_for_it_is_sold()
        {
            var market = new FakeMarket();
            Load load = market.Add(Cargo("iron"), amount: 3, price: 200);
            load.Basis = 100;
            load.Purchased = 3;
            Run run = Sell(market);
            Assert.Equal(3, run.Units);
            Assert.Equal(300, run.Profit);
            Assert.Equal(new[] { "iron", "iron", "iron" }, market.Given);
        }

        [Fact]
        public void A_price_that_misses_your_margin_keeps_the_cargo()
        {
            var market = new FakeMarket();
            Load load = market.Add(Cargo("iron"), amount: 3, price: 110);
            load.Basis = 100;
            load.Purchased = 3;
            Run run = Sell(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowMargin));
        }

        [Fact]
        public void The_units_you_paid_for_are_stepped_over_to_reach_the_ones_you_did_not()
        {
            var market = new FakeMarket();
            Load load = market.Add(Cargo("iron"), amount: 5, price: 110);
            load.Basis = 100;
            load.Purchased = 2;
            load.Worth = 80;
            Run run = Sell(market);
            Assert.Equal(3, run.Units);
            Assert.Equal(90, run.Profit);
            Assert.True(run.Tally.Saw(Block.BelowMargin));
        }

        [Fact]
        public void A_merchant_out_of_gold_stops_the_pass()
        {
            var market = new FakeMarket { Till = 250 };
            Load load = market.Add(Cargo("iron"), amount: 5, price: 100);
            load.Worth = 50;
            Run run = Sell(market);
            Assert.Equal(2, run.Units);
            Assert.True(run.Tally.Saw(Block.MerchantTillEmpty));
        }

        [Fact]
        public void Holding_out_for_a_better_market_keeps_the_cargo()
        {
            var market = new FakeMarket();
            market.Rules.PreferBestSellTown = true;
            Load load = market.Add(Cargo("iron"), amount: 3, price: 200);
            load.Elsewhere = true;
            load.Resale = 300;
            Run run = Sell(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowBestMarket));
        }

        [Fact]
        public void A_market_that_pays_within_your_tolerance_is_sold_to()
        {
            var market = new FakeMarket();
            market.Rules.PreferBestSellTown = true;
            Load load = market.Add(Cargo("iron"), amount: 3, price: 290);
            load.Elsewhere = true;
            load.Resale = 300;
            Assert.Equal(3, Sell(market).Units);
        }

        [Fact]
        public void The_best_market_is_looked_up_once_for_a_good_however_many_units_go()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 6, price: 200).Worth = 100;
            Run run = Sell(market);
            Assert.Equal(6, run.Units);
            Assert.Equal(1, market.WorthAsked);
            Assert.Equal(1, market.Described);
        }

        [Fact]
        public void A_good_you_already_bought_here_is_not_sold_back()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 3);
            var books = new Books();
            books.NoteBought("iron", 100);
            Run run = Sell(market, books: books);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.TradedHereAlready));
        }

        [Fact]
        public void The_never_sell_list_keeps_a_good_in_the_cargo()
        {
            var market = new FakeMarket();
            market.Rules.NeverSellItems = "iron";
            market.Add(Cargo("iron"), amount: 3);
            Run run = Sell(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.NeverList));
        }

        [Fact]
        public void The_food_reserve_holds_its_share_back_and_the_rest_goes()
        {
            var market = new FakeMarket();
            market.Add(Ration("grain"), amount: 10, price: 200).Reserved = 4;
            Run run = Sell(market);
            Assert.Equal(6, run.Units);
        }

        [Fact]
        public void A_reserve_that_covers_the_whole_load_leaves_it_alone()
        {
            var market = new FakeMarket();
            market.Add(Ration("grain"), amount: 4, price: 200).Reserved = 4;
            Run run = Sell(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.FoodReserve));
        }

        [Fact]
        public void A_sale_of_a_unit_you_paid_for_is_written_to_the_ledger()
        {
            var market = new FakeMarket();
            Load load = market.Add(Cargo("iron"), amount: 5, price: 200);
            load.Basis = 100;
            load.Purchased = 2;
            load.Worth = 100;
            Run run = Sell(market);
            Assert.Equal(5, run.Units);
            Assert.Equal(2, market.Recorded.Count);
        }

        [Fact]
        public void A_trade_that_goes_the_wrong_way_stops_the_whole_pass()
        {
            var market = new FakeMarket { RefuseAfter = 1 };
            market.Add(Cargo("iron"), amount: 4);
            market.Add(Cargo("wool"), amount: 4);
            Run run = Sell(market);
            Assert.Equal(1, run.Units);
            Assert.Single(market.Given);
        }

        [Fact]
        public void A_trade_that_pays_nothing_leaves_the_rest_of_that_good_alone()
        {
            var market = new FakeMarket { PayNothingAfter = 2 };
            market.Add(Cargo("iron"), amount: 6);
            market.Add(Cargo("wool"), amount: 3);
            Run run = Sell(market);
            Assert.Equal(2, run.Units);
            Assert.Equal(new[] { "iron", "iron" }, market.Given);
        }

        [Fact]
        public void A_dry_run_lays_the_deal_out_and_moves_nothing()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 3, price: 200);
            Run run = Sell(market, sim: true);
            Assert.Equal(3, run.Units);
            Assert.Empty(market.Given);
            Assert.Equal(new[] { "iron", "iron", "iron" }, market.LaidOut);
            Assert.Equal(100000, market.Till);
            Assert.Equal(600, run.SimGold);
        }

        [Fact]
        public void A_dry_run_draws_the_merchant_till_down_as_it_goes()
        {
            var market = new FakeMarket { Till = 250 };
            market.Add(Cargo("iron"), amount: 5, price: 100).Worth = 50;
            Run run = Sell(market, sim: true);
            Assert.Equal(2, run.Units);
            Assert.True(run.Tally.Saw(Block.MerchantTillEmpty));
            Assert.Equal(250, market.Till);
        }

        [Fact]
        public void A_dry_run_books_the_gold_it_would_have_taken()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron", weight: 2f), amount: 3, price: 200);
            Run run = Sell(market, sim: true);
            Assert.Equal(600, run.Books.Purse(true));
            Assert.Equal(600, run.Books.TillDrawn(true));
            Assert.Equal(-6f, run.Books.Weight(true));
        }

        [Fact]
        public void A_second_dry_run_over_the_same_books_does_not_sell_the_same_load_twice()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 3, price: 200);
            var books = new Books();
            Assert.Equal(3, Sell(market, sim: true, books: books).Units);
            Assert.Equal(0, Sell(market, sim: true, books: books).Units);
        }
    }
}
