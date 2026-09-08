using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
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

        internal Shelf(Settlement site, EquipmentElement stocked, bool selling, int quoted, bool projecting)
        {
            ItemObject item = stocked.Item;
            _item = item;
            _element = stocked;
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
            _shelf = new Shelf(site, new EquipmentElement(item), selling, quoted, projecting: true);
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

        internal static int PricePaid(Settlement site, EquipmentElement bought, int units, int quotedUnitPrice)
        {
            if (units <= 0) return 0;
            int flat = quotedUnitPrice * units;
            if (site == null || bought.Item == null) return flat;

            Shelf shelf = new Shelf(site, bought, selling: false, quoted: quotedUnitPrice, projecting: false);
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
    internal static class Priced
    {
        private static bool _saidItCouldNotAsk;

        internal static IMarketData Kept(Settlement site)
        {
            if (site == null) return null;
            if (site.IsTown) return site.Town == null ? null : (IMarketData)site.Town.MarketData;
            if (site.IsVillage) return site.Village == null ? null : (IMarketData)site.Village.MarketData;
            return null;
        }

        internal static int At(SettlementComponent market, ItemObject item, MobileParty who, bool selling) =>
            item == null ? 0 : At(market, new EquipmentElement(item), who, selling);

        internal static int At(SettlementComponent market, EquipmentElement el, MobileParty who, bool selling)
        {
            if (market == null) return 0;
            Settlement site = market.Settlement;
            IMarketData held = Kept(site);
            if (held != null)
            {
                try { return held.GetPrice(el, who, selling, site.Party); }
                catch (Exception e)
                {
                    if (!_saidItCouldNotAsk)
                    {
                        _saidItCouldNotAsk = true;
                        Log.Error(e, "asking a market its price the way the trade screen asks it, naming " +
                                     "the merchant - TradeLord falls back to asking without one");
                    }
                }
            }
            return market.GetItemPrice(el, who, selling);
        }
    }

    internal static class PriceTrace
    {
        internal static void Say(Settlement site, string when)
        {
            if (!Options.Current.PriceTrace || site == null) return;
            Guard.Run("PriceTrace", () => Written(site, when));
        }

        private static void Written(Settlement site, string when)
        {
            SettlementComponent market = site.SettlementComponent;
            ItemRoster carried = MobileParty.MainParty == null ? null : MobileParty.MainParty.ItemRoster;
            if (market == null || carried == null) return;
            IMarketData kept = Priced.Kept(site);

            Log.Write("price trace (" + when + ") at " + site.Name + ", " + Named(site) +
                      ", prices kept by " +
                      (kept == null ? "nothing TradeLord can read" : kept.GetType().Name) + LeansOn(site));
            Log.Write("  the price model in force is " + ModelName());
            Log.Write("  " + PatchedBy("that model's GetPrice", ModelPrice()));
            MethodBase asked = MarketPrice(market.GetType());
            MethodBase inherited = MarketPrice(typeof(SettlementComponent));
            Log.Write("  " + PatchedBy("this market's own GetItemPrice", asked));
            if (inherited != null && inherited != asked)
                Log.Write("  " + PatchedBy("the GetItemPrice every market inherits", inherited));
            Log.Write("  live world prices are " + (Options.Current.Omniscient ? "on" : "off") +
                      ", and the four readings below are for one unit, before anything is traded");

            for (int i = 0; i < carried.Count; i++)
            {
                EquipmentElement el = carried.GetElementCopyAtIndex(i).EquipmentElement;
                ItemObject item = el.Item;
                if (!TradePolicy.Priced(item)) continue;
                Log.Write("  " + item.StringId + " (" + (item.Name == null ? item.StringId : item.Name.ToString()) +
                          ") worth " + el.ItemValue + ": TradeLord uses " + Uses(market, el) +
                          (kept == null ? "" :
                           "; asked the plain way, which names no merchant, " + Asked(market, el) +
                           "; through the market's own prices with no merchant, " +
                           Read(kept, el, MobileParty.MainParty, null) +
                           "; naming nobody at all, " + Read(kept, el, null, null)) +
                          Noted(item, site));
            }
        }

        private static string Named(Settlement site) =>
            (site.IsTown ? "a town" : site.IsVillage ? "a village" : "neither a town nor a village") +
            " the game holds as " + site.SettlementComponent.GetType().Name;

        private static string LeansOn(Settlement site)
        {
            Village village = site.IsVillage ? site.Village : null;
            Settlement town = village == null ? null : village.TradeBound ?? village.Bound;
            return town == null ? "" : ", which trades through " + town.Name;
        }

        private static string Uses(SettlementComponent market, EquipmentElement el) =>
            "sell " + Priced.At(market, el, MobileParty.MainParty, true) +
            ", buy " + Priced.At(market, el, MobileParty.MainParty, false);

        private static string Asked(SettlementComponent market, EquipmentElement el) =>
            "sell " + market.GetItemPrice(el, MobileParty.MainParty, true) +
            ", buy " + market.GetItemPrice(el, MobileParty.MainParty, false);

        private static string Read(IMarketData kept, EquipmentElement el, MobileParty who, PartyBase merchant) =>
            "sell " + kept.GetPrice(el, who, true, merchant) +
            ", buy " + kept.GetPrice(el, who, false, merchant);

        private static string Noted(ItemObject item, Settlement site)
        {
            if (Options.Current.Omniscient) return "";
            LedgerBehavior ledger = LedgerBehavior.Instance;
            float age = ledger == null ? -1f : ledger.ObservationAgeDays(item, site);
            return age < 0f
                ? "; TradeLord has no note of its own here"
                : "; TradeLord's own note here was taken " + age.ToString("0.0", CultureInfo.InvariantCulture) +
                  " days ago";
        }

        private static string ModelName()
        {
            object model = Campaign.Current == null || Campaign.Current.Models == null
                ? null : Campaign.Current.Models.TradeItemPriceFactorModel;
            return model == null ? "not something TradeLord can read" : model.GetType().FullName;
        }

        private static MethodBase ModelPrice()
        {
            object model = Campaign.Current == null || Campaign.Current.Models == null
                ? null : Campaign.Current.Models.TradeItemPriceFactorModel;
            return model == null ? null : AccessTools.Method(model.GetType(), "GetPrice");
        }

        private static MethodBase MarketPrice(Type owner) =>
            AccessTools.Method(owner, "GetItemPrice",
                new[] { typeof(EquipmentElement), typeof(MobileParty), typeof(bool) });

        private static string PatchedBy(string what, MethodBase method)
        {
            if (method == null) return what + " is not on this game version, so nothing could be read off it";
            Patches found = Harmony.GetPatchInfo(method);
            if (found == null || found.Owners == null || found.Owners.Count == 0)
                return what + " is untouched, so no other mod is changing it";
            return what + " is being changed by " + string.Join(", ", new List<string>(found.Owners).ToArray());
        }
    }
}
