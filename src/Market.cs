using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TradeLord
{
    internal struct RouteQuote
    {
        internal int Units;
        internal int BuyTotal;
        internal int SellTotal;
        internal int OpeningBuyPrice;
        internal int OpeningSellPrice;
        internal bool Simulated;
    }

    internal sealed class Shelf
    {
        private readonly PartyBase _party;
        private readonly ItemObject _item;
        private readonly EquipmentElement _element;
        private readonly bool _selling;
        private readonly int _quoted;
        private readonly bool _walkable;
        private readonly float _supply;
        private readonly float _demand;
        private int _inStoreValue;

        internal bool Walkable => _walkable;

        internal Shelf(Settlement site, ItemObject item, bool selling, int quoted, bool projecting)
        {
            _item = item;
            _element = new EquipmentElement(item);
            _party = site?.Party;
            _selling = selling;
            _quoted = quoted;
            if (projecting && (!Options.Current.Omniscient || !Options.Current.BulkSimulation)) return;
            Town town = site != null && site.IsTown ? site.Town : null;
            if (town == null || item == null || item.ItemCategory == null) return;
            try
            {
                ItemData data = town.MarketData.GetCategoryData(item.ItemCategory);
                _supply = data.Supply;
                _demand = data.Demand;
                _inStoreValue = data.InStoreValue;
                _walkable = true;
            }
            catch (Exception e) { Log.Error(e, "bulk price walk setup"); }
        }

        internal int Price()
        {
            if (!_walkable) return _quoted;
            try
            {
                return Campaign.Current.Models.TradeItemPriceFactorModel.GetPrice(
                    _element, MobileParty.MainParty, _party, _selling,
                    _inStoreValue, _supply, _demand);
            }
            catch (Exception e) { Log.Error(e, "bulk price walk"); return _quoted; }
        }

        internal void Restock(int units)
        {
            if (!_walkable) return;
            _inStoreValue += units * _item.Value;
            if (_inStoreValue < 0) _inStoreValue = 0;
        }
    }

    internal static class Bulk
    {
        internal static RouteQuote Walk(Settlement from, Settlement to, ItemObject item,
                                        int maxUnits, int merchantTill,
                                        int quotedBuyPrice, int quotedSellPrice)
        {
            RouteQuote q = default(RouteQuote);
            if (item == null || from == null || to == null || maxUnits <= 0) return q;

            Shelf buy = new Shelf(from, item, selling: false, quoted: quotedBuyPrice, projecting: true);
            Shelf sell = new Shelf(to, item, selling: true, quoted: quotedSellPrice, projecting: true);
            q.Simulated = buy.Walkable && sell.Walkable;

            for (int u = 0; u < maxUnits; u++)
            {
                int buyPrice = buy.Price();
                int sellPrice = sell.Price();
                if (buyPrice <= 0 || sellPrice <= 0) break;
                if (!TradePolicy.BuyAcceptable(buyPrice, TradePolicy.Realizable(sellPrice))) break;
                if (merchantTill > 0 && q.SellTotal + sellPrice > merchantTill) break;

                if (q.Units == 0) { q.OpeningBuyPrice = buyPrice; q.OpeningSellPrice = sellPrice; }
                q.BuyTotal += buyPrice;
                q.SellTotal += sellPrice;
                q.Units++;

                buy.Restock(-1);
                sell.Restock(1);
            }
            return q;
        }

        internal static int PricePaid(Settlement site, ItemObject item, int units, int quotedUnitPrice)
        {
            if (units <= 0) return 0;
            int flat = quotedUnitPrice * units;
            if (site == null || item == null) return flat;

            Shelf shelf = new Shelf(site, item, selling: false, quoted: quotedUnitPrice, projecting: false);
            if (!shelf.Walkable) return flat;

            shelf.Restock(units);
            int total = 0;
            for (int u = 0; u < units; u++)
            {
                int price = shelf.Price();
                if (price <= 0) return flat;
                total += price;
                shelf.Restock(-1);
            }
            return total;
        }
    }

    internal static class Confidence
    {
        private const float Unsimulated = 0.85f;

        internal static float Of(bool simulated, int flatProfit, int simulatedProfit,
                                 int stock, int units, float travelDays,
                                 int caravans, float dataAgeDays)
        {
            float resilience = Unsimulated;
            if (simulated && flatProfit > 0)
                resilience = Clamp((float)simulatedProfit / flatProfit);

            float depth = 1f;
            if (stock > 0 && units > 0)
                depth = Clamp(stock / (units * 1.5f));

            float haste = 1f / (1f + Math.Max(travelDays, 0f) / 3f);
            float quiet = 1f / (1f + Math.Max(caravans, 0) * 0.15f);
            float fresh = dataAgeDays < 0f ? 1f : 1f / (1f + dataAgeDays / 5f);

            float c = resilience * depth * haste * quiet * fresh;
            return c < 0.01f ? 0.01f : (c > 1f ? 1f : c);
        }

        private static float Clamp(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
    }
}
