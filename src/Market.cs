using System;
using System.Collections.Generic;
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

    internal sealed class Ladder
    {
        private readonly Shelf _shelf;
        private readonly bool _selling;
        private readonly List<int> _priced = new List<int>();

        internal Ladder(Settlement site, ItemObject item, bool selling, int quoted)
        {
            _selling = selling;
            _shelf = new Shelf(site, item, selling, quoted, projecting: true);
        }

        internal bool Walkable => _shelf.Walkable;

        internal int At(int taken)
        {
            while (_priced.Count <= taken)
            {
                _priced.Add(_shelf.Price());
                if (_selling) _shelf.Restock(1); else _shelf.Restock(-1);
            }
            return _priced[taken];
        }
    }

    internal static class Bulk
    {
        private static readonly Dictionary<(string site, string item, bool selling), Ladder> _rungs =
            new Dictionary<(string, string, bool), Ladder>();

        internal static void Forget() => _rungs.Clear();

        private static Ladder Rung(Settlement site, ItemObject item, bool selling, int quoted)
        {
            var key = (site.StringId, item.StringId, selling);
            if (!_rungs.TryGetValue(key, out Ladder rung))
            {
                rung = new Ladder(site, item, selling, quoted);
                _rungs[key] = rung;
            }
            return rung;
        }

        internal static RouteQuote Walk(Settlement from, Settlement to, ItemObject item,
                                        int maxUnits, int merchantTill, int spendCap,
                                        int quotedBuyPrice, int quotedSellPrice)
        {
            RouteQuote q = default(RouteQuote);
            if (item == null || from == null || to == null || maxUnits <= 0) return q;

            Ladder buy = Rung(from, item, selling: false, quoted: quotedBuyPrice);
            Ladder sell = Rung(to, item, selling: true, quoted: quotedSellPrice);
            q.Simulated = buy.Walkable && sell.Walkable;

            for (int u = 0; u < maxUnits; u++)
            {
                int buyPrice = buy.At(u);
                int sellPrice = sell.At(u);
                if (buyPrice <= 0 || sellPrice <= 0) break;
                if (!TradePolicy.BuyAcceptable(buyPrice, TradePolicy.Realizable(sellPrice))) break;
                if (merchantTill > 0 && q.SellTotal + sellPrice > merchantTill) break;
                if (spendCap > 0 && q.BuyTotal + buyPrice > spendCap) break;

                if (q.Units == 0) { q.OpeningBuyPrice = buyPrice; q.OpeningSellPrice = sellPrice; }
                q.BuyTotal += buyPrice;
                q.SellTotal += sellPrice;
                q.Units++;
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
}
