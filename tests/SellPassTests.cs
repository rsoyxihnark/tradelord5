using System;
using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class SellPassTests
    {
        private const int AveragePaid = 0;

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
            internal int[] Dearer;
            internal Batch[] Rows;
            internal int Cheap = -1;
            internal int Worth = 100;
            internal int Resale;
            internal int Takes = 1000;
            internal int[] Ladder;
            internal bool Elsewhere;
            internal bool Modified;
            internal int Reserved;
            internal int Falls;
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
            internal bool OnTheScreen;
            internal int Till = 100000;
            internal bool AtAVillage;
            internal bool Halted;
            internal int RefuseAfter = -1;
            internal int PayNothingAfter = -1;
            internal int Described;
            internal int WorthAsked;
            internal int AskedFor;
            internal int AskedWorth;
            internal int MarkPurse = 1000000;
            internal readonly List<(string good, int units, int there, int here)> Held =
                new List<(string good, int units, int there, int here)>();

            internal Load Add(Good good, int amount = 1, int price = 200)
            {
                var load = new Load { Good = good, Amount = amount, Price = price };
                Cargo.Add(load);
                return load;
            }

            public int Count => Cargo.Count;

            public bool Stopped => Halted;

            public bool Village => AtAVillage;

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
                System.Math.Min(Cargo[at].Amount - Ledger.SoldFrom(Sim, PaidKeyAt(at)),
                                Carrying(Cargo[at].Good.Id) + Ledger.Held(Sim, Cargo[at].Good.Id));

            private int Carrying(string id)
            {
                int held = 0;
                foreach (Load load in Cargo)
                    if (load.Good.Id == id) held += load.Amount;
                return held;
            }

            public int CostBasis(int at) => Cargo[at].Basis;

            public string PaidKeyAt(int at) =>
                LedgerCodec.PaidKey(Cargo[at].Good.Id, Cargo[at].Modified ? "fine" : null);

            public int PurchasedUnits(int at) => Cargo[at].Purchased;

            public Batch[] UnitCosts(int at)
            {
                Load load = Cargo[at];
                if (load.Purchased <= 0) return null;
                if (load.Rows != null)
                {
                    var rows = new List<Batch>(load.Rows);
                    rows.Sort(TradeMath.CheapestFirstOldestLast);
                    return rows.ToArray();
                }
                int dear = load.Dearer == null ? 0 : load.Dearer.Length;
                var costs = new List<Batch>();
                if (load.Purchased > dear)
                    costs.Add(new Batch { Unit = load.Cheap >= 0 ? load.Cheap : load.Basis, Count = load.Purchased - dear });
                if (load.Dearer != null)
                    foreach (int unit in load.Dearer) costs.Add(new Batch { Unit = unit, Count = 1 });
                costs.Sort((x, y) => x.Unit.CompareTo(y.Unit));
                return costs.ToArray();
            }

            public int UnpaidWorth(int at)
            {
                WorthAsked++;
                return Cargo[at].Worth;
            }

            public int HoldableUnits(int at)
            {
                SellFacts facts = default(SellFacts);
                facts.FoodHeld = Cargo[at].Reserved;
                facts.QuestsReadable = true;
                SellVerdict said = TradeRules.MaySell(Cargo[at].Good, Cargo[at].Amount, facts, Rules,
                                                      default(Says));
                return said.Allowed ? Math.Max(0, YoursToSell(at) - said.KeepCount) : 0;
            }

            public int ResalePurse() => MarkPurse;

            public bool ResaleMarket(int at, int units, int worth, out int[] rungs)
            {
                AskedFor = units;
                AskedWorth = worth;
                var took = new List<int>();
                long paid = 0L;
                for (int u = 0; u < units; u++)
                {
                    int price = Cargo[at].Ladder != null
                        ? (u < Cargo[at].Ladder.Length ? Cargo[at].Ladder[u] : 0)
                        : (u < Cargo[at].Takes ? Cargo[at].Resale : 0);
                    if (price <= 0 || paid + price > MarkPurse) break;
                    paid += price;
                    took.Add(price);
                }
                rungs = took.ToArray();
                return Cargo[at].Elsewhere && rungs.Length > 0;
            }

            public Func<int, int> PricesHere(int at)
            {
                int price = Cargo[at].Price, falls = Cargo[at].Falls;
                return u => price - falls * u;
            }

            public void HeldBack(int at, int units, int there, int here) =>
                Held.Add((Cargo[at].Good.Id, units, there, here));

            public int PriceToSell(int at) => Cargo[at].Price;

            public bool OfAQuality(int at) => Cargo[at].Modified;

            int ISellingMarket.Till() => Till - Ledger.TillDrawn(Sim);

            public int TillNow() => Till;

            public void Staged(int at, int price)
            {
                LaidOut.Add(Cargo[at].Good.Id);
                if (OnTheScreen) Cargo[at].Amount--;
            }

            internal readonly List<int> SoldFor = new List<int>();

            public bool Give(int at, int price, out int proceeds)
            {
                proceeds = 0;
                if (RefuseAfter >= 0 && Given.Count >= RefuseAfter) { Halted = true; return false; }
                if (PayNothingAfter >= 0 && Given.Count >= PayNothingAfter) return true;
                proceeds = price;
                SoldFor.Add(price);
                Till -= price;
                Cargo[at].Amount--;
                Cargo[at].Price -= Cargo[at].Falls;
                Given.Add(Cargo[at].Good.Id);
                return true;
            }

            internal float Day;

            public float Today => Day;

            internal readonly List<int> RecordedPaid = new List<int>();
            internal readonly List<int> RecordedBest = new List<int>();

            public void RecordedSale(int at, int unitPaid, int price, int bestPays)
            {
                Recorded.Add(Cargo[at].Good.Id);
                RecordedPaid.Add(unitPaid);
                RecordedBest.Add(bestPays);
                Cargo[at].Purchased--;
            }

            internal readonly List<int> RecordedFree = new List<int>();

            public void RecordedFreeSale(int at, int price) => RecordedFree.Add(price);
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
            foreach (bool loot in new[] { true, false }) OnePass(market, run, sim, loot);
            return run;
        }

        private static Run OnePass(FakeMarket market, bool loot, bool sim = false, Books books = null)
        {
            var run = new Run { Tally = new BlockTally(), Books = books ?? new Books() };
            market.Ledger = run.Books;
            market.Sim = sim;
            return OnePass(market, run, sim, loot);
        }

        private static Run OnePass(FakeMarket market, Run run, bool sim, bool loot)
        {
            Traded moved = TradePass.SellThem(market, run.Books, sim, market.Rules, run.Tally, loot);
            run.Units += moved.Units;
            run.Profit += moved.Profit;
            run.SimGold += moved.SimGold;
            return run;
        }

        [Fact]
        public void A_dry_run_keeps_what_you_paid_for_each_quality_of_a_good_apart()
        {
            foreach (bool sim in new[] { false, true })
            {
                var market = new FakeMarket();
                Load plain = market.Add(Cargo("iron"), amount: 1, price: 200);
                plain.Basis = 100;
                plain.Purchased = 1;
                Load fine = market.Add(Cargo("iron"), amount: 1, price: 200);
                fine.Basis = 300;
                fine.Purchased = 1;
                fine.Modified = true;

                Run run = Sell(market, sim);

                Assert.Equal(1, run.Units);
            }
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
            Assert.Equal(0, run.Profit);
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

        private static Load Bought(FakeMarket market, int amount, int price, int paid, int there)
        {
            Load load = market.Add(Cargo("iron"), amount, price);
            load.Basis = paid;
            load.Purchased = amount;
            load.Elsewhere = true;
            load.Resale = there;
            return load;
        }

        [Fact]
        public void Holding_out_for_the_marked_market_keeps_the_cargo_you_bought()
        {
            var market = new FakeMarket();
            Bought(market, 3, price: 200, paid: 100, there: 300);
            Run run = Sell(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowBestMarket));
        }

        [Fact]
        public void A_market_that_pays_within_your_share_is_sold_to()
        {
            var market = new FakeMarket();
            Bought(market, 3, price: 290, paid: 100, there: 300);
            Assert.Equal(3, Sell(market).Units);
        }

        [Fact]
        public void Three_quarters_of_what_the_marked_market_pays_is_where_the_hold_lets_go()
        {
            Assert.Equal(0.75f, new Options().HoldCargoForBestMarket);
            var at = new FakeMarket();
            Bought(at, 1, price: 187, paid: 100, there: 250);
            Assert.Equal(1, Sell(at).Units);
            var under = new FakeMarket();
            Bought(under, 1, price: 186, paid: 100, there: 250);
            Assert.Equal(0, Sell(under).Units);
        }

        [Fact]
        public void A_share_of_nothing_holds_nothing()
        {
            var market = new FakeMarket();
            market.Rules.HoldCargoForBestMarket = 0f;
            Bought(market, 3, price: 200, paid: 100, there: 1000);
            Assert.Equal(3, Sell(market).Units);
        }

        [Fact]
        public void Loot_sold_is_booked_against_what_came_without_a_purchase_and_makes_no_profit()
        {
            var market = new FakeMarket();
            Load load = market.Add(Cargo("iron"), amount: 3, price: 200);
            load.Worth = 100;
            Run run = Sell(market);
            Assert.Equal(3, run.Units);
            Assert.Equal(new[] { 200, 200, 200 }, market.RecordedFree.ToArray());
            Assert.Empty(market.RecordedPaid);
            Assert.Equal(0, run.Profit);
        }

        [Fact]
        public void Loot_is_never_held_for_the_marked_market()
        {
            var market = new FakeMarket();
            Load load = market.Add(Cargo("iron"), amount: 3, price: 200);
            load.Worth = 100;
            load.Elsewhere = true;
            load.Resale = 1000;
            Assert.Equal(3, Sell(market).Units);
        }

        [Fact]
        public void Only_as_many_as_the_marked_market_would_buy_are_held()
        {
            var market = new FakeMarket();
            Bought(market, 5, price: 200, paid: 100, there: 300).Takes = 2;
            Run run = Sell(market);
            Assert.Equal(3, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowBestMarket));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public void In_a_stack_of_bought_and_looted_units_the_bought_ones_are_held_and_the_loot_goes(int costBasisMode)
        {
            var market = new FakeMarket();
            market.Rules.CostBasisMode = costBasisMode;
            Load load = Bought(market, 10, price: 200, paid: 100, there: 300);
            load.Purchased = 4;
            load.Takes = 8;
            load.Worth = 100;
            Run run = Sell(market);
            Assert.Equal(6, run.Units);
            Assert.Equal(4, market.AskedFor);
            Assert.True(run.Tally.Saw(Block.BelowBestMarket));
        }

        [Fact]
        public void Units_kept_back_from_a_bought_stack_never_let_the_rest_go_under_the_floor()
        {
            var market = new FakeMarket();
            Load load = market.Add(Ration("grain"), amount: 10, price: 200);
            load.Basis = 100;
            load.Purchased = 10;
            load.Elsewhere = true;
            load.Resale = 300;
            load.Reserved = 3;
            Run run = Sell(market);
            Assert.Equal(0, run.Units);
            Assert.Equal(7, market.AskedFor);
            Assert.True(run.Tally.Saw(Block.BelowBestMarket));
        }

        [Fact]
        public void The_marked_market_is_asked_about_the_bought_units_at_the_margin_they_are_sold_against()
        {
            var market = new FakeMarket();
            Bought(market, 10, price: 200, paid: 100, there: 300).Takes = 6;
            Run run = Sell(market);
            Assert.Equal(4, run.Units);
            Assert.Equal(10, market.AskedFor);
            Assert.Equal(100, market.AskedWorth);
        }

        private static Load BoughtAs(FakeMarket market, string id, int amount, int price, int paid, int there)
        {
            Load load = market.Add(Cargo(id), amount, price);
            load.Basis = paid;
            load.Purchased = amount;
            load.Elsewhere = true;
            load.Resale = there;
            return load;
        }

        [Fact]
        public void The_units_the_marked_market_pays_most_for_are_held_and_the_rest_sell_here()
        {
            var market = new FakeMarket();
            Bought(market, 10, price: 160, paid: 100, there: 0).Ladder =
                new[] { 300, 280, 260, 240, 220, 200, 180, 160, 140, 120 };
            Run run = Sell(market);
            Assert.Equal(5, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowBestMarket));
            Assert.Equal(("iron", 5, 220, 160), market.Held[0]);
        }

        [Fact]
        public void A_price_that_falls_as_the_stack_sells_is_held_once_it_drops_under_your_share()
        {
            var market = new FakeMarket();
            Bought(market, 20, price: 200, paid: 100, there: 260).Falls = 5;
            Run run = Sell(market);
            Assert.Equal(2, run.Units);
            Assert.Equal(("iron", 18, 260, 190), market.Held[0]);
        }

        [Fact]
        public void The_units_the_marked_market_pays_little_for_sell_here_even_when_its_average_is_high()
        {
            var market = new FakeMarket();
            Bought(market, 10, price: 180, paid: 100, there: 0).Ladder =
                new[] { 400, 400, 400, 400, 400, 100, 100, 100, 100, 100 };
            Run run = Sell(market);
            Assert.Equal(5, run.Units);
            Assert.Equal(("iron", 5, 400, 180), market.Held[0]);
        }

        [Fact]
        public void The_marked_market_spends_its_gold_first_on_the_good_it_pays_most_for_over_this_market()
        {
            var market = new FakeMarket { MarkPurse = 5000 };
            BoughtAs(market, "iron", 20, price: 180, paid: 100, there: 250);
            BoughtAs(market, "velvet", 10, price: 120, paid: 100, there: 400);
            Run run = Sell(market);
            Assert.Equal(16, run.Units);
            Assert.Contains(("iron", 4, 250, 180), market.Held);
            Assert.Contains(("velvet", 10, 400, 120), market.Held);
        }

        [Fact]
        public void Goods_that_miss_your_margin_here_draw_first_on_the_gold_of_the_marked_market()
        {
            var market = new FakeMarket { MarkPurse = 5000 };
            BoughtAs(market, "iron", 20, price: 180, paid: 100, there: 250);
            BoughtAs(market, "velvet", 10, price: 110, paid: 100, there: 400);
            Run run = Sell(market);
            Assert.Equal(16, run.Units);
            Assert.Contains(("iron", 4, 250, 180), market.Held);
            Assert.True(run.Tally.Saw(Block.BelowMargin));
        }

        [Fact]
        public void No_more_is_held_than_the_marked_market_has_the_gold_to_buy()
        {
            var market = new FakeMarket { MarkPurse = 3000 };
            BoughtAs(market, "iron", 10, price: 150, paid: 100, there: 250);
            BoughtAs(market, "tools", 10, price: 150, paid: 100, there: 250);
            Run run = Sell(market);
            Assert.Equal(8, run.Units);
            int held = 0;
            foreach (var one in market.Held) held += one.units;
            Assert.Equal(12, held);
        }

        [Fact]
        public void A_marked_market_with_no_gold_holds_nothing()
        {
            var market = new FakeMarket { MarkPurse = 0 };
            Bought(market, 3, price: 200, paid: 100, there: 300);
            Assert.Equal(3, Sell(market).Units);
            Assert.Empty(market.Held);
        }

        [Fact]
        public void Units_this_market_cannot_take_count_against_the_gold_of_the_marked_market()
        {
            var market = new FakeMarket { MarkPurse = 2500 };
            BoughtAs(market, "iron", 10, price: 200, paid: 100, there: 250).Falls = 20;
            BoughtAs(market, "tools", 10, price: 150, paid: 100, there: 250);
            Run run = Sell(market);
            Assert.Equal(10, run.Units);
            Assert.Contains(("iron", 7, 250, 140), market.Held);
            Assert.Contains(("tools", 3, 250, 150), market.Held);
        }

        [Fact]
        public void Goods_bought_here_this_visit_count_against_the_gold_of_the_marked_market()
        {
            var books = new Books();
            books.NoteBought("velvet", 100);
            var market = new FakeMarket { MarkPurse = 3000 };
            BoughtAs(market, "velvet", 10, price: 120, paid: 100, there: 300);
            BoughtAs(market, "iron", 10, price: 150, paid: 100, there: 250);
            Run run = Sell(market, books: books);
            Assert.Equal(10, run.Units);
            Assert.Empty(market.Held);
        }

        [Fact]
        public void Two_qualities_of_one_good_share_what_the_marked_market_would_buy()
        {
            var market = new FakeMarket();
            int[] ladder = { 300, 300, 300, 300, 300, 100, 100, 100, 100, 100 };
            Load plain = market.Add(Cargo("iron"), 5, 160);
            plain.Basis = 100; plain.Purchased = 5; plain.Elsewhere = true; plain.Ladder = ladder;
            Load fine = market.Add(Cargo("iron"), 5, 160);
            fine.Basis = 100; fine.Purchased = 5; fine.Elsewhere = true; fine.Ladder = ladder; fine.Modified = true;
            Run run = Sell(market);
            Assert.Equal(5, run.Units);
            Assert.Single(market.Held);
            Assert.Equal(("iron", 5, 300, 160), market.Held[0]);
        }

        [Fact]
        public void Selling_what_you_bought_leaves_what_you_did_not_buy_for_the_loot_sale()
        {
            var market = new FakeMarket();
            Load load = market.Add(Cargo("iron"), amount: 5, price: 200);
            load.Basis = 100;
            load.Purchased = 3;
            load.Worth = 50;
            var books = new Books();
            Run bought = OnePass(market, loot: false, books: books);
            Assert.Equal(3, bought.Units);
            Assert.Equal(300, bought.Profit);
            Assert.Equal(2, load.Amount);
            Run looted = OnePass(market, loot: true, books: books);
            Assert.Equal(2, looted.Units);
            Assert.Equal(0, looted.Profit);
            Assert.Equal(0, load.Amount);
        }

        [Fact]
        public void The_loot_sale_leaves_the_units_you_bought_where_they_are()
        {
            var market = new FakeMarket();
            Load load = market.Add(Cargo("iron"), amount: 5, price: 110);
            load.Basis = 100;
            load.Purchased = 3;
            load.Worth = 50;
            Run looted = OnePass(market, loot: true);
            Assert.Equal(2, looted.Units);
            Assert.Equal(3, load.Amount);
        }

        [Fact]
        public void What_you_bought_is_sold_only_with_the_gold_this_market_has_left_after_the_loot()
        {
            var market = new FakeMarket { MarkPurse = 1250, Till = 2190 };
            Load loot = market.Add(Cargo("sword"), 10, 200);
            loot.Worth = 100;
            Load iron = BoughtAs(market, "iron", 10, price: 200, paid: 100, there: 250);
            Load tools = BoughtAs(market, "tools", 5, price: 150, paid: 100, there: 250);
            Run run = Sell(market);
            Assert.Equal(11, run.Units);
            Assert.Equal(0, loot.Amount);
            Assert.Equal(10, iron.Amount);
            Assert.Equal(4, tools.Amount);
            Assert.Equal(40, market.Till);
            Assert.Empty(market.Held);
            Assert.True(run.Tally.Saw(Block.MerchantTillEmpty));
        }

        [Fact]
        public void A_good_you_bought_that_the_marked_market_does_not_want_counts_against_this_market_s_gold()
        {
            var market = new FakeMarket { MarkPurse = 1250, Till = 2190 };
            BoughtAs(market, "grain", 10, price: 200, paid: 100, there: 250).Elsewhere = false;
            BoughtAs(market, "iron", 10, price: 200, paid: 100, there: 250);
            Load tools = BoughtAs(market, "tools", 5, price: 150, paid: 100, there: 250);
            Run run = Sell(market);
            Assert.Empty(market.Held);
            Assert.Equal(11, run.Units);
            Assert.Equal(4, tools.Amount);
        }

        [Fact]
        public void A_dry_run_leaves_the_sale_of_what_you_bought_only_the_gold_the_loot_sale_left()
        {
            var market = new FakeMarket { Till = 500 };
            Load iron = market.Add(Cargo("iron"), amount: 2, price: 200);
            iron.Basis = 100;
            iron.Purchased = 2;
            market.Add(Cargo("sword"), amount: 1, price: 200).Worth = 100;
            Run run = Sell(market, sim: true);
            Assert.Equal(2, run.Units);
            Assert.Equal(400, run.SimGold);
            Assert.Equal(1, run.Books.SoldFrom(true, "sword"));
            Assert.Equal(1, run.Books.SoldFrom(true, "iron"));
        }

        [Fact]
        public void Your_margin_still_binds_where_the_hold_would_let_a_sale_through()
        {
            var market = new FakeMarket();
            Bought(market, 3, price: 110, paid: 100, there: 120);
            Run run = Sell(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowMargin));
            Assert.False(run.Tally.Saw(Block.BelowBestMarket));
        }

        [Fact]
        public void A_unit_that_misses_both_is_named_as_missing_your_margin()
        {
            var market = new FakeMarket();
            Bought(market, 3, price: 110, paid: 100, there: 300);
            Run run = Sell(market);
            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowMargin));
            Assert.False(run.Tally.Saw(Block.BelowBestMarket));
        }

        [Fact]
        public void The_best_market_is_looked_up_once_for_a_good_however_many_units_go()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 6, price: 200).Worth = 100;
            Run run = Sell(market);
            Assert.Equal(6, run.Units);
            Assert.Equal(1, market.WorthAsked);
            Assert.Equal(2, market.Described);
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

        [Fact]
        public void A_good_with_a_modifier_is_sold_and_shows_its_profit()
        {
            var market = new FakeMarket();
            Load iron = market.Add(Cargo("iron"), price: 200);
            iron.Basis = 100;
            iron.Purchased = 1;
            Load sword = market.Add(Cargo("sword"), price: 200);
            sword.Modified = true;
            sword.Basis = 100;
            sword.Purchased = 1;
            Run run = Sell(market);
            Assert.Equal(2, run.Units);
            Assert.Equal(200, run.Profit);
        }

        [Fact]
        public void Units_bought_at_different_prices_each_count_at_what_they_cost()
        {
            var market = new FakeMarket();
            Load grain = market.Add(Cargo("grain"), amount: 3, price: 200);
            grain.Basis = 150;
            grain.Purchased = 3;
            grain.Cheap = 100;
            grain.Dearer = new[] { 190 };
            market.Rules.CostBasisMode = 1;
            Run run = Sell(market);
            Assert.Equal(3, run.Units);
            Assert.Equal(100 + 100 + 10, run.Profit);
        }

        [Fact]
        public void A_dear_unit_sold_at_what_it_cost_shows_no_profit_and_the_rest_count_at_their_own_cost()
        {
            var market = new FakeMarket();
            market.Rules.CostBasisMode = AveragePaid;
            Load felt = market.Add(Cargo("felt"), amount: 5, price: 836);
            felt.Falls = 400;
            felt.Basis = 390;
            felt.Purchased = 5;
            felt.Cheap = 278;
            felt.Dearer = new[] { 836 };

            Run first = Sell(market);

            Assert.Equal(1, first.Units);
            Assert.Equal(0, first.Profit);

            felt.Price = 484;
            felt.Falls = 0;
            felt.Dearer = null;
            Run second = Sell(market);

            Assert.Equal(4, second.Units);
            Assert.Equal(4 * (484 - 278), second.Profit);
        }

        [Fact]
        public void A_village_is_left_its_last_coin()
        {
            var market = new FakeMarket { Till = 400, AtAVillage = true };
            market.Add(Cargo("iron"), amount: 3, price: 200).Worth = 50;
            Run run = Sell(market);
            Assert.Equal(1, run.Units);
            Assert.Equal(200, market.Till);
            Assert.True(run.Tally.Saw(Block.MerchantTillEmpty));
        }

        [Fact]
        public void A_town_spends_its_till_to_the_last_coin()
        {
            var market = new FakeMarket { Till = 400 };
            market.Add(Cargo("iron"), amount: 3, price: 200).Worth = 50;
            Run run = Sell(market);
            Assert.Equal(2, run.Units);
            Assert.Equal(0, market.Till);
            Assert.True(run.Tally.Saw(Block.MerchantTillEmpty));
        }

        [Fact]
        public void A_dry_run_leaves_a_village_its_last_coin_too()
        {
            var market = new FakeMarket { Till = 400, AtAVillage = true };
            market.Add(Cargo("iron"), amount: 3, price: 200).Worth = 50;
            Run run = Sell(market, sim: true);
            Assert.Equal(1, run.Units);
            Assert.Equal(200, run.SimGold);
        }

        [Fact]
        public void A_dry_run_counts_only_the_sound_horses_it_would_sell_towards_the_herd()
        {
            var market = new FakeMarket();
            market.Rules.AlwaysSellItems = "horse";
            var horse = new Good { Id = "horse", Name = "horse", HasHorse = true, IsSpareMount = true, IsMountable = true };
            market.Add(horse, amount: 1, price: 200);
            market.Add(horse, amount: 1, price: 150).Modified = true;
            Run run = Sell(market, sim: true);
            Assert.Equal(2, run.Units);
            Assert.Equal(1, run.Books.Shed(true));
            Assert.Equal(1, run.Books.MountsShed(true));
        }

        [Fact]
        public void A_dry_run_sells_every_quality_of_a_good()
        {
            var market = new FakeMarket();
            market.Add(Cargo("iron"), amount: 3, price: 200).Worth = 50;
            Load fine = market.Add(Cargo("iron"), amount: 2, price: 200);
            fine.Worth = 50;
            fine.Modified = true;
            Run run = Sell(market, sim: true);
            Assert.Equal(5, run.Units);
        }

        [Fact]
        public void A_deal_laid_out_on_the_trade_screen_sells_every_quality_of_a_good()
        {
            var market = new FakeMarket { OnTheScreen = true };
            market.Add(Cargo("iron"), amount: 3, price: 200).Worth = 50;
            Load fine = market.Add(Cargo("iron"), amount: 2, price: 200);
            fine.Worth = 50;
            fine.Modified = true;
            Run run = Sell(market, sim: true, books: new Books { LaidOut = true });
            Assert.Equal(5, run.Units);
            Assert.Equal(1000, run.SimGold);
        }

        [Fact]
        public void A_unit_bought_dear_sells_with_the_rest_of_its_good_once_the_average_is_beaten()
        {
            foreach (bool sim in new[] { false, true })
            {
                var market = new FakeMarket();
                market.Rules.CostBasisMode = AveragePaid;
                Load felt = market.Add(Cargo("felt"), amount: 5, price: 484);
                felt.Basis = 390;
                felt.Purchased = 5;
                felt.Dearer = new[] { 836 };

                Run run = Sell(market, sim);

                Assert.Equal(5, run.Units);
                Assert.Equal(5 * 484 - 4 * 390 - 836, run.Profit);
            }
        }

        [Fact]
        public void A_unit_bought_dear_is_sold_once_a_market_pays_what_it_cost()
        {
            var market = new FakeMarket();
            Load felt = market.Add(Cargo("felt"), amount: 5, price: 1135);
            felt.Basis = 390;
            felt.Purchased = 5;
            felt.Cheap = 278;
            felt.Dearer = new[] { 836 };

            Run run = Sell(market);

            Assert.Equal(5, run.Units);
            Assert.Equal(5 * 1135 - 836 - 4 * 278, run.Profit);
        }

        [Fact]
        public void A_dry_run_that_already_drew_the_cheap_units_still_sells_the_dear_one_at_the_average()
        {
            var market = new FakeMarket();
            market.Rules.CostBasisMode = AveragePaid;
            Load felt = market.Add(Cargo("felt"), amount: 2, price: 484);
            felt.Basis = 390;
            felt.Purchased = 5;
            felt.Dearer = new[] { 836 };
            var books = new Books();
            for (int i = 0; i < 3; i++) books.NotePaidDrawn(market.PaidKeyAt(0));

            Run run = Sell(market, sim: true, books: books);

            Assert.Equal(2, run.Units);
            Assert.Equal(2 * 484 - 390 - 836, run.Profit);
        }

        [Fact]
        public void The_price_you_set_for_the_market_basis_is_never_raised_by_what_you_paid()
        {
            var market = new FakeMarket();
            market.Rules.CostBasisMode = 2;
            Load felt = market.Add(Cargo("felt"), amount: 5, price: 484);
            felt.Basis = 390;
            felt.Purchased = 5;
            felt.Dearer = new[] { 836 };

            Run run = Sell(market);

            Assert.Equal(5, run.Units);
        }

        [Fact]
        public void A_unit_bought_dear_takes_the_best_price_before_the_cheap_ones_pull_it_down()
        {
            var market = new FakeMarket();
            Load felt = market.Add(Cargo("felt"), amount: 5, price: 1135);
            felt.Falls = 109;
            felt.Basis = 390;
            felt.Purchased = 5;
            felt.Cheap = 278;
            felt.Dearer = new[] { 836 };

            Run run = Sell(market);

            Assert.Equal(5, run.Units);
            Assert.Equal(new[] { 1135, 1026, 917, 808, 699 }, market.SoldFor.ToArray());
            Assert.Equal(new[] { 836, 278, 278, 278, 278 }, market.RecordedPaid.ToArray());
            Assert.Equal(1135 + 1026 + 917 + 808 + 699 - 836 - 4 * 278, run.Profit);
        }

        [Theory]
        [InlineData(836, 1)]
        [InlineData(835, 1)]
        [InlineData(449, 1)]
        [InlineData(448, 0)]
        public void A_unit_bought_dear_sells_once_the_price_beats_the_average_by_your_margin(int price, int sold)
        {
            var market = new FakeMarket();
            market.Rules.CostBasisMode = AveragePaid;
            Load felt = market.Add(Cargo("felt"), amount: 1, price: price);
            felt.Basis = 390;
            felt.Purchased = 1;
            felt.Dearer = new[] { 836 };

            Run run = Sell(market);

            Assert.Equal(sold, run.Units);
            Assert.Equal(sold == 1 ? new[] { 836 } : new int[0], market.RecordedPaid.ToArray());
        }

        [Fact]
        public void Each_unit_at_its_own_cost_sells_the_dearest_the_price_clears_by_your_margin_and_keeps_the_rest()
        {
            var market = new FakeMarket();
            Assert.Equal(Options.CostOfEachUnit, market.Rules.CostBasisMode);
            Load cloth = market.Add(Cargo("cloth"), amount: 3, price: 260);
            cloth.Basis = 200;
            cloth.Purchased = 3;
            cloth.Cheap = 100;
            cloth.Dearer = new[] { 200, 300 };

            Run run = Sell(market);

            Assert.Equal(2, run.Units);
            Assert.Equal(new[] { 200, 100 }, market.RecordedPaid.ToArray());
            Assert.Equal((260 - 200) + (260 - 100), run.Profit);
            Assert.True(run.Tally.Saw(Block.BelowMargin));
        }

        [Fact]
        public void Each_unit_at_its_own_cost_goes_down_the_ladder_from_the_dearest_as_the_price_falls()
        {
            foreach (bool sim in new[] { false, true })
            {
                var market = new FakeMarket();
                Load cloth = market.Add(Cargo("cloth"), amount: 3, price: 400);
                cloth.Falls = 50;
                cloth.Basis = 200;
                cloth.Purchased = 3;
                cloth.Cheap = 100;
                cloth.Dearer = new[] { 200, 300 };

                Run run = Sell(market, sim);

                Assert.Equal(3, run.Units);
                Assert.Equal((400 - 300) + (sim ? 400 - 200 : 350 - 200) + (sim ? 400 - 100 : 300 - 100), run.Profit);
                if (!sim) Assert.Equal(new[] { 300, 200, 100 }, market.RecordedPaid.ToArray());
            }
        }

        [Fact]
        public void Each_unit_at_its_own_cost_sells_nothing_when_no_unit_clears_your_margin()
        {
            var market = new FakeMarket();
            Load cloth = market.Add(Cargo("cloth"), amount: 3, price: 114);
            cloth.Basis = 200;
            cloth.Purchased = 3;
            cloth.Cheap = 100;
            cloth.Dearer = new[] { 200, 300 };

            Run run = Sell(market);

            Assert.Equal(0, run.Units);
            Assert.Empty(market.RecordedPaid);
            Assert.True(run.Tally.Saw(Block.BelowMargin));
        }

        [Fact]
        public void Each_unit_at_its_own_cost_also_has_to_fetch_three_quarters_of_what_the_marked_market_pays()
        {
            var market = new FakeMarket();
            Load cloth = Bought(market, 3, price: 240, paid: 100, there: 300);
            cloth.Takes = 3;

            Run run = Sell(market);

            Assert.Equal(3, run.Units);
            Assert.Equal(new[] { 300, 300, 300 }, market.RecordedBest.ToArray());

            var held = new FakeMarket();
            Bought(held, 3, price: 224, paid: 100, there: 300).Takes = 3;
            Run kept = Sell(held);

            Assert.Equal(0, kept.Units);
            Assert.True(kept.Tally.Saw(Block.BelowBestMarket));
        }

        private static Batch Row(long number, int paid, float day) =>
            new Batch { Unit = paid, Count = 1, First = number, Day = day };

        [Fact]
        public void Wine_bought_at_five_prices_sells_down_the_ladder_and_one_past_its_age_sells_at_what_it_cost()
        {
            var market = new FakeMarket { Day = 100f };
            Load wine = market.Add(Cargo("wine"), amount: 5, price: 105);
            wine.Falls = 3;
            wine.Basis = 78;
            wine.Purchased = 5;
            wine.Elsewhere = true;
            wine.Ladder = new[] { 130, 125, 120, 115, 110 };
            wine.Rows = new[] { Row(21, 120, 88f), Row(22, 100, 69f), Row(23, 70, 95f), Row(24, 70, 95f), Row(25, 30, 98f) };

            Run run = Sell(market);

            Assert.Equal(4, run.Units);
            Assert.Equal(new[] { 100, 70, 70, 30 }, market.RecordedPaid.ToArray());
            Assert.Equal(new[] { 0, 0, 120, 125 }, market.RecordedBest.ToArray());
            Assert.Equal((105 - 100) + (102 - 70) + (99 - 70) + (96 - 30), run.Profit);
        }

        [Fact]
        public void A_unit_past_its_age_sells_at_what_it_cost_while_Hold_cargo_for_the_best_market_keeps_the_rest()
        {
            var market = new FakeMarket { Day = 100f };
            Load wine = market.Add(Cargo("wine"), amount: 2, price: 80);
            wine.Basis = 55;
            wine.Purchased = 2;
            wine.Elsewhere = true;
            wine.Ladder = new[] { 130, 130 };
            wine.Rows = new[] { Row(1, 50, 60f), Row(2, 60, 99f) };

            Run run = Sell(market);

            Assert.Equal(1, run.Units);
            Assert.Equal(new[] { 50 }, market.RecordedPaid.ToArray());
            Assert.True(run.Tally.Saw(Block.BelowBestMarket));
        }

        [Fact]
        public void With_Sell_at_cost_after_at_0_a_unit_never_skips_its_margin_however_long_it_waited()
        {
            var market = new FakeMarket { Day = 500f };
            market.Rules.SellAtCostAfterDays = 0;
            Load wine = market.Add(Cargo("wine"), amount: 2, price: 80);
            wine.Basis = 55;
            wine.Purchased = 2;
            wine.Elsewhere = true;
            wine.Ladder = new[] { 130, 130 };
            wine.Rows = new[] { Row(1, 50, 60f), Row(2, 60, 99f) };

            Run run = Sell(market);

            Assert.Equal(0, run.Units);
            Assert.True(run.Tally.Saw(Block.BelowBestMarket));
        }

        [Fact]
        public void A_unit_past_its_age_still_never_sells_for_less_than_it_cost()
        {
            var market = new FakeMarket { Day = 100f };
            Load wine = market.Add(Cargo("wine"), amount: 1, price: 99);
            wine.Basis = 100;
            wine.Purchased = 1;
            wine.Rows = new[] { Row(1, 100, 10f) };

            Assert.Equal(0, Sell(market).Units);
            wine.Price = 100;
            Assert.Equal(1, Sell(market).Units);
            Assert.Equal(new[] { 100 }, market.RecordedPaid.ToArray());
        }

        [Fact]
        public void The_marked_market_is_counted_unit_by_unit_so_a_unit_it_would_not_take_at_a_profit_is_never_held_for_it()
        {
            var market = new FakeMarket { Day = 100f };
            Load lemonade = market.Add(Cargo("lemonade"), amount: 3, price: 20);
            lemonade.Basis = 20;
            lemonade.Purchased = 3;
            lemonade.Elsewhere = true;
            lemonade.Ladder = new[] { 32, 28, 24, 20 };
            lemonade.Rows = new[] { Row(1, 10, 99f), Row(2, 20, 99f), Row(3, 30, 99f) };

            ForTheMark[] holding = TradePass.WhatTheMarkKeeps(market, new Books(), false, market.Rules);

            Assert.Equal(new[] { 32, 28 }, holding[0].Rungs);
            Assert.Equal(10, market.AskedWorth);
        }

        [Fact]
        public void Food_kept_back_with_units_past_their_age_sells_the_aged_units_and_holds_the_fresh_ones_for_the_marked_market()
        {
            var market = new FakeMarket { Day = 100f };
            Load cheese = market.Add(Ration("cheese"), amount: 10, price: 200);
            cheese.Basis = 130;
            cheese.Purchased = 10;
            cheese.Elsewhere = true;
            cheese.Resale = 300;
            cheese.Reserved = 7;
            cheese.Rows = new[]
            {
                Row(1, 100, 10f), Row(2, 100, 10f), Row(3, 100, 10f), Row(4, 100, 10f),
                Row(5, 150, 99f), Row(6, 150, 99f), Row(7, 150, 99f), Row(8, 150, 99f), Row(9, 150, 99f), Row(10, 150, 99f)
            };

            Run run = Sell(market);

            Assert.Equal(new[] { 100, 100, 100 }, market.RecordedPaid.ToArray());
            Assert.True(run.Tally.Saw(Block.BelowBestMarket));
        }

        [Fact]
        public void A_unit_past_its_age_is_never_held_for_the_marked_market()
        {
            var market = new FakeMarket { Day = 100f };
            Load lemonade = market.Add(Cargo("lemonade"), amount: 3, price: 20);
            lemonade.Basis = 20;
            lemonade.Purchased = 3;
            lemonade.Elsewhere = true;
            lemonade.Ladder = new[] { 32, 28, 24, 20 };
            lemonade.Rows = new[] { Row(1, 10, 50f), Row(2, 20, 99f), Row(3, 30, 99f) };

            ForTheMark[] holding = TradePass.WhatTheMarkKeeps(market, new Books(), false, market.Rules);

            Assert.Equal(new[] { 32 }, holding[0].Rungs);
            Assert.Equal(2, market.AskedFor);
        }

        [Fact]
        public void Goods_with_no_unit_the_average_would_sell_at_a_loss_sell_as_they_always_did()
        {
            var market = new FakeMarket();
            Load linen = market.Add(Cargo("linen"), amount: 6, price: 320);
            linen.Basis = 272;
            linen.Purchased = 6;

            Run run = Sell(market);

            Assert.Equal(6, run.Units);
        }

        [Fact]
        public void A_second_dry_run_pass_knows_the_dear_unit_already_went_in_the_first()
        {
            var market = new FakeMarket { Till = 3 * 1135 };
            Load felt = market.Add(Cargo("felt"), amount: 5, price: 1135);
            felt.Basis = 390;
            felt.Purchased = 5;
            felt.Dearer = new[] { 836 };
            var books = new Books();

            Assert.Equal(3, OnePass(market, loot: false, sim: true, books: books).Units);

            felt.Price = 500;
            market.Till = 100000;
            Assert.Equal(2, OnePass(market, loot: false, sim: true, books: books).Units);
        }

        private static SalesOnOneScreen<string> Screen() =>
            new SalesOnOneScreen<string>((one, other) => one == other);

        [Fact]
        public void One_good_sold_a_unit_at_a_time_is_one_sale_as_the_trade_screen_counts_it()
        {
            SalesOnOneScreen<string> screen = Screen();
            screen.Sold("felt", 836, muted: false);
            screen.Sold("felt", 748, muted: false);
            screen.Sold("grain", 20, muted: false);
            screen.Sold("felt", 660, muted: false);

            SalesOnOneScreen<string>.Line[] lines = screen.Closed(out bool muted);

            Assert.False(muted);
            Assert.Equal(2, lines.Length);
            Assert.Equal("felt", lines[0].What);
            Assert.Equal(3, lines[0].Units);
            Assert.Equal(836 + 748 + 660, lines[0].Gold);
            Assert.Equal("grain", lines[1].What);
            Assert.Equal(1, lines[1].Units);
            Assert.Equal(20, lines[1].Gold);
        }

        [Fact]
        public void Closing_the_screen_empties_it_so_the_next_visit_starts_a_new_one()
        {
            SalesOnOneScreen<string> screen = Screen();
            screen.Sold("felt", 836, muted: false);
            Assert.Single(screen.Closed(out _));

            Assert.Equal(0, screen.Count);
            Assert.Empty(screen.Closed(out bool muted));
            Assert.True(muted);
        }

        [Fact]
        public void A_sale_that_paid_nothing_is_left_off_the_screen()
        {
            SalesOnOneScreen<string> screen = Screen();
            screen.Sold("felt", 0, muted: false);
            screen.Sold("felt", -5, muted: false);

            Assert.Empty(screen.Closed(out bool muted));
            Assert.True(muted);
        }

        [Fact]
        public void The_screen_is_quiet_only_when_every_sale_on_it_was_quiet()
        {
            SalesOnOneScreen<string> screen = Screen();
            screen.Sold("felt", 836, muted: true);
            screen.Sold("grain", 20, muted: true);
            screen.Closed(out bool allQuiet);
            Assert.True(allQuiet);

            screen.Sold("felt", 836, muted: true);
            screen.Sold("grain", 20, muted: false);
            screen.Closed(out bool oneSpoke);
            Assert.False(oneSpoke);
        }

        [Fact]
        public void The_gold_on_one_line_stops_at_the_largest_number_rather_than_turning_negative()
        {
            SalesOnOneScreen<string> screen = Screen();
            screen.Sold("jewelry", int.MaxValue, muted: false);
            screen.Sold("jewelry", int.MaxValue, muted: false);

            SalesOnOneScreen<string>.Line[] lines = screen.Closed(out _);

            Assert.Equal(2, lines[0].Units);
            Assert.Equal(int.MaxValue, lines[0].Gold);
        }

        [Fact]
        public void Forgetting_the_screen_drops_what_was_on_it()
        {
            SalesOnOneScreen<string> screen = Screen();
            screen.Sold("felt", 836, muted: false);
            screen.Forget();

            Assert.Empty(screen.Closed(out bool muted));
            Assert.True(muted);
        }
    }
}
