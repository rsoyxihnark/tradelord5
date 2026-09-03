using System.Collections.Generic;
using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class TradeMathTests
    {
        private const int AveragePaid = 0, LastPaid = 1, CheapestKnown = 2;

        private static PurchaseRecord Bought(int count, int totalPaid)
        {
            var rec = new PurchaseRecord { ItemId = "grain" };
            TradeMath.AddPurchase(rec, count, totalPaid);
            return rec;
        }

        [Fact]
        public void What_a_lot_cost_per_unit_is_what_you_paid_for_it()
        {
            Assert.Equal(12, TradeMath.UnitBasis(Bought(10, 120), AveragePaid));
            Assert.Equal(12, TradeMath.UnitBasis(Bought(10, 120), LastPaid));
        }

        [Fact]
        public void Buying_again_at_a_new_price_averages_the_lot_and_remembers_the_last()
        {
            var rec = Bought(10, 100);
            TradeMath.AddPurchase(rec, 10, 300);
            Assert.Equal(20, TradeMath.UnitBasis(rec, AveragePaid));
            Assert.Equal(30, TradeMath.UnitBasis(rec, LastPaid));
        }

        [Fact]
        public void Selling_part_of_a_lot_leaves_the_rest_costing_what_it_did()
        {
            var rec = Bought(10, 120);
            TradeMath.DrainSale(rec, 4);
            Assert.Equal(6, rec.Count);
            Assert.Equal(12, TradeMath.UnitBasis(rec, AveragePaid));
        }

        [Fact]
        public void Selling_the_whole_lot_clears_what_it_cost()
        {
            var rec = Bought(10, 120);
            TradeMath.DrainSale(rec, 10);
            Assert.Equal(0, rec.Count);
            Assert.Equal(0, rec.TotalPaid);
            Assert.Equal(TradeMath.NoRecordedBasis, TradeMath.UnitBasis(rec, AveragePaid));
        }

        [Fact]
        public void Selling_more_than_you_hold_never_takes_the_lot_below_nothing()
        {
            var rec = Bought(3, 30);
            TradeMath.DrainSale(rec, 99);
            Assert.Equal(0, rec.Count);
            Assert.Equal(0, rec.TotalPaid);
            TradeMath.DrainSale(rec, 99);
            Assert.Equal(0, rec.Count);
            Assert.Equal(0, rec.TotalPaid);
        }

        [Fact]
        public void A_good_you_never_bought_has_no_price_you_paid()
        {
            Assert.Equal(TradeMath.NoRecordedBasis, TradeMath.UnitBasis(null, AveragePaid));
            Assert.Equal(TradeMath.NoRecordedBasis, TradeMath.UnitBasis(Bought(0, 0), AveragePaid));
        }

        [Fact]
        public void The_cheapest_known_mode_never_reads_what_you_paid()
        {
            Assert.Equal(TradeMath.NoRecordedBasis, TradeMath.UnitBasis(Bought(10, 120), CheapestKnown));
        }

        [Fact]
        public void Buying_and_selling_over_and_over_never_leaves_a_negative_cost()
        {
            var rec = Bought(7, 93);
            for (int round = 1; round <= 200; round++)
            {
                TradeMath.AddPurchase(rec, round % 5 + 1, round * 13 % 97 + 1);
                TradeMath.DrainSale(rec, round % 7 + 1);
                Assert.True(rec.Count >= 0);
                Assert.True(rec.TotalPaid >= 0);
                int unit = TradeMath.UnitBasis(rec, AveragePaid);
                Assert.True(unit == TradeMath.NoRecordedBasis || unit >= 0);
            }
        }

        [Theory]
        [InlineData(Options.PolicyIgnore, true, false)]
        [InlineData(Options.PolicyIgnore, false, false)]
        [InlineData(Options.PolicySellOnly, true, false)]
        [InlineData(Options.PolicySellOnly, false, true)]
        [InlineData(Options.PolicyBuyOnly, true, true)]
        [InlineData(Options.PolicyBuyOnly, false, false)]
        [InlineData(Options.PolicyBuySell, true, true)]
        [InlineData(Options.PolicyBuySell, false, true)]
        public void Each_category_policy_allows_exactly_what_its_name_says(int policy, bool buying, bool allowed)
        {
            Assert.Equal(allowed, TradeMath.PolicyAllows(policy, buying));
        }

        [Fact]
        public void A_policy_value_nobody_defined_allows_nothing()
        {
            foreach (int stray in new[] { -1, 4, 99, int.MaxValue, int.MinValue })
            {
                Assert.False(TradeMath.PolicyAllows(stray, buying: true));
                Assert.False(TradeMath.PolicyAllows(stray, buying: false));
            }
        }

        [Fact]
        public void Goods_you_paid_for_are_credited_against_what_you_paid()
        {
            Assert.Equal(40, TradeMath.Credit(proceeds: 100, basis: 60, unpaidWorth: 999));
            Assert.Equal(0, TradeMath.Credit(proceeds: 60, basis: 60, unpaidWorth: 999));
        }

        [Fact]
        public void A_sale_below_what_you_paid_is_reported_as_the_loss_it_is()
        {
            Assert.Equal(-25, TradeMath.Credit(proceeds: 75, basis: 100, unpaidWorth: 0));
        }

        [Fact]
        public void Loot_is_credited_against_what_it_would_have_cost_never_below_zero()
        {
            Assert.Equal(30, TradeMath.Credit(proceeds: 80, basis: 0, unpaidWorth: 50));
            Assert.Equal(0, TradeMath.Credit(proceeds: 50, basis: 0, unpaidWorth: 50));
            Assert.Equal(0, TradeMath.Credit(proceeds: 20, basis: 0, unpaidWorth: 50));
        }

        [Fact]
        public void A_negative_basis_is_treated_as_no_basis_rather_than_a_bonus()
        {
            Assert.Equal(0, TradeMath.Credit(proceeds: 10, basis: -100, unpaidWorth: 50));
        }

        [Theory]
        [InlineData(100, 115, 0.15f, true)]
        [InlineData(100, 114, 0.15f, false)]
        [InlineData(100, 100, 0f, true)]
        [InlineData(100, 99, 0f, false)]
        [InlineData(100, 200, 1f, true)]
        [InlineData(100, 199, 1f, false)]
        public void A_sale_clears_the_margin_only_at_or_above_the_marked_up_basis(
            int basis, int sellPrice, float margin, bool acceptable)
        {
            Assert.Equal(acceptable, TradeMath.ProfitAcceptable(basis, sellPrice, margin));
        }

        [Fact]
        public void With_no_basis_any_positive_price_clears_and_zero_does_not()
        {
            Assert.True(TradeMath.ProfitAcceptable(costBasis: 0, townSellPrice: 1, margin: 5f));
            Assert.False(TradeMath.ProfitAcceptable(costBasis: 0, townSellPrice: 0, margin: 0f));
            Assert.False(TradeMath.ProfitAcceptable(costBasis: 0, townSellPrice: -5, margin: 0f));
        }

        [Theory]
        [InlineData(100, 0.85f, 85f)]
        [InlineData(100, 1f, 100f)]
        [InlineData(0, 0.85f, 0f)]
        public void The_safety_factor_discounts_a_far_market_price(int far, float factor, float expected)
        {
            Assert.Equal(expected, TradeMath.Realizable(far, factor), 3);
        }

        [Theory]
        [InlineData(100, 115f, 0.15f, true)]
        [InlineData(100, 114f, 0.15f, false)]
        [InlineData(100, 100f, 0f, true)]
        [InlineData(0, 500f, 0f, false)]
        [InlineData(-5, 500f, 0f, false)]
        public void A_purchase_clears_only_when_the_discounted_resale_covers_the_markup(
            int buyPrice, float realizable, float margin, bool acceptable)
        {
            Assert.Equal(acceptable, TradeMath.BuyAcceptable(buyPrice, realizable, margin));
        }

        [Fact]
        public void A_free_good_is_never_bought_however_good_the_resale_looks()
        {
            Assert.False(TradeMath.BuyAcceptable(buyPrice: 0, realizable: float.MaxValue, margin: 0f));
        }

        [Theory]
        [InlineData(0f, 0.85f)]
        [InlineData(0.15f, 0.85f)]
        [InlineData(0.5f, 1f)]
        [InlineData(1f, 0.6f)]
        public void Anything_worth_buying_is_worth_selling_at_the_same_margin(float margin, float safety)
        {
            for (int buy = 1; buy <= 400; buy += 7)
            {
                for (int far = 1; far <= 2000; far += 37)
                {
                    float realizable = TradeMath.Realizable(far, safety);
                    if (!TradeMath.BuyAcceptable(buy, realizable, margin)) continue;

                    Assert.True(TradeMath.ProfitAcceptable(buy, (int)realizable + 1, margin),
                        $"buy {buy} cleared at margin {margin} but selling at {(int)realizable + 1} did not");
                }
            }
        }

        [Fact]
        public void A_stricter_margin_never_admits_a_purchase_a_looser_one_refused()
        {
            foreach (int buy in new[] { 1, 17, 100, 999 })
            {
                foreach (float realizable in new[] { 0f, 50f, 100f, 1500f })
                {
                    bool loose = TradeMath.BuyAcceptable(buy, realizable, 0.10f);
                    bool strict = TradeMath.BuyAcceptable(buy, realizable, 0.50f);
                    Assert.True(loose || !strict);
                }
            }
        }

        [Fact]
        public void A_stricter_margin_never_admits_a_sale_a_looser_one_refused()
        {
            foreach (int basis in new[] { 1, 17, 100, 999 })
            {
                foreach (int sell in new[] { 0, 50, 100, 1500 })
                {
                    bool loose = TradeMath.ProfitAcceptable(basis, sell, 0.10f);
                    bool strict = TradeMath.ProfitAcceptable(basis, sell, 0.50f);
                    Assert.True(loose || !strict);
                }
            }
        }

        [Fact]
        public void The_shipped_defaults_are_the_ones_the_rules_were_written_for()
        {
            var fresh = new Options();
            Assert.Equal(0, fresh.Language);
            Assert.Equal(0.15f, fresh.MinProfitMargin, 3);
            Assert.Equal(0.85f, fresh.ResaleSafetyFactor, 3);
            Assert.Equal(Options.PolicyBuySell, fresh.FoodPolicy);
            Assert.Equal(Options.PolicyBuySell, fresh.CraftingPolicy);
            Assert.Equal(Options.PolicyBuySell, fresh.LivestockPolicy);

            Assert.True(TradeMath.BuyAcceptable(100, TradeMath.Realizable(136, fresh.ResaleSafetyFactor), fresh.MinProfitMargin));
            Assert.False(TradeMath.BuyAcceptable(100, TradeMath.Realizable(135, fresh.ResaleSafetyFactor), fresh.MinProfitMargin));
        }

        [Fact]
        public void A_prohibitive_margin_refuses_every_purchase()
        {
            for (int buy = 1; buy <= 1000; buy += 13)
                Assert.False(TradeMath.BuyAcceptable(buy, buy * 10f, margin: 100f));
        }
    }
}
