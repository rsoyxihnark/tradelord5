using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace TradeLord
{
    public class TradeRoute
    {
        public ItemObject Item;
        public Settlement From;
        public Settlement To;
        public int BuyPrice;
        public int SellPrice;
        public int Quantity;
        public float TravelDays;
        public int TotalProfit;
        public float ProfitPerDay;

        public float Confidence = 1f;
        public float Score;
        public bool Simulated;
        public int Caravans;

        public float DataAgeDays = -1f;
    }

    public class LedgerBehavior : CampaignBehaviorBase
    {
        public static LedgerBehavior Instance { get; internal set; }

        private Dictionary<string, Dictionary<string, PriceObservation>> _ledger =
            new Dictionary<string, Dictionary<string, PriceObservation>>();
        private List<PurchaseRecord> _purchases = new List<PurchaseRecord>();
        private string _ledgerText = "";
        private string _purchaseText = "";
        private Dictionary<string, PurchaseRecord> _paid;
        private int _lifetimeProfit;
        private ItemRoster _watched;
        private bool _settle;

        public int LifetimeProfit => _lifetimeProfit;
        public void AddProfit(int amount) => _lifetimeProfit += amount;

        public LedgerBehavior() { Instance = this; }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener(this, OnPlayerInventoryExchange);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (!dataStore.IsLoading) PruneExpired();
            if (dataStore.IsSaving)
                Guard.Run("Ledger.WriteForSave", () =>
                {
                    _ledgerText = LedgerCodec.WriteLedger(Listed(_ledger));
                    _purchaseText = LedgerCodec.WritePurchases(_purchases);
                });
            dataStore.SyncData("TradeLord_LedgerText", ref _ledgerText);
            dataStore.SyncData("TradeLord_PurchaseText", ref _purchaseText);
            dataStore.SyncData("TradeLord_LifetimeProfit", ref _lifetimeProfit);
            if (dataStore.IsLoading) ReadSavedText();
            if (dataStore.IsLoading) PruneExpired();
            Guard.Run("Ledger.Reindex", Reindex);
            if (dataStore.IsLoading)
                Log.Write("ledger restored: " + _ledger.Count + " observed items, " +
                          _purchases.Count + " purchase records, lifetime profit " + _lifetimeProfit);
        }

        private void ReadSavedText() => Guard.Run("Ledger.ReadSaved", RestoreSaved);

        private void RestoreSaved()
        {
            _ledger = KeyedByTown(LedgerCodec.ReadLedger(_ledgerText));
            _purchases = LedgerCodec.ReadPurchases(_purchaseText);
        }

        private static Dictionary<string, Dictionary<string, PriceObservation>> KeyedByTown(
            Dictionary<string, List<PriceObservation>> listed)
        {
            var book = new Dictionary<string, Dictionary<string, PriceObservation>>();
            if (listed == null) return book;
            foreach (var kv in listed)
            {
                var byTown = new Dictionary<string, PriceObservation>(StringComparer.Ordinal);
                if (kv.Value != null)
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        PriceObservation o = kv.Value[i];
                        if (o?.TownId != null) byTown[o.TownId] = o;
                    }
                book[kv.Key] = byTown;
            }
            return book;
        }

        private static Dictionary<string, List<PriceObservation>> Listed(
            Dictionary<string, Dictionary<string, PriceObservation>> keyed)
        {
            var book = new Dictionary<string, List<PriceObservation>>();
            if (keyed == null) return book;
            foreach (var kv in keyed)
            {
                var list = new List<PriceObservation>();
                if (kv.Value != null) list.AddRange(kv.Value.Values);
                book[kv.Key] = list;
            }
            return book;
        }

        private void PruneExpired() => Guard.Run("Ledger.Prune", Prune);

        private void Prune()
        {
            PruneObservations();
            PruneSettledPurchases();
        }

        private void PruneObservations()
        {
            if (_ledger == null) return;
            var spent = new List<string>();
            foreach (var kv in _ledger)
            {
                if (kv.Value == null) { spent.Add(kv.Key); continue; }
                var dead = new List<string>();
                foreach (var seen in kv.Value)
                    if (seen.Value == null || seen.Value.TownId == null) dead.Add(seen.Key);
                for (int i = 0; i < dead.Count; i++) kv.Value.Remove(dead[i]);
                if (kv.Value.Count == 0) spent.Add(kv.Key);
            }
            for (int i = 0; i < spent.Count; i++) _ledger.Remove(spent[i]);
        }

        private void PruneSettledPurchases() =>
            _purchases?.RemoveAll(rec => rec == null || rec.ItemId == null || rec.Count <= 0);

        private void MatchPurchasesToWhatIsHeld()
        {
            ItemRoster carried = MobileParty.MainParty?.ItemRoster;
            if (carried == null || _purchases.Count == 0) return;
            var held = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < carried.Count; i++)
            {
                ItemRosterElement el = carried.GetElementCopyAtIndex(i);
                ItemObject item = el.EquipmentElement.Item;
                if (item == null || el.Amount <= 0) continue;
                held.TryGetValue(item.StringId, out int had);
                held[item.StringId] = had + el.Amount;
            }
            int goods = 0, units = 0;
            for (int i = 0; i < _purchases.Count; i++)
            {
                PurchaseRecord rec = _purchases[i];
                if (rec?.ItemId == null || rec.Count <= 0) continue;
                held.TryGetValue(rec.ItemId, out int have);
                if (rec.Count <= have) continue;
                int gone = rec.Count - have;
                goods++;
                units += gone;
                TradeMath.DrainSale(rec, gone);
            }
            if (goods > 0)
                Log.Write("purchase record: " + units + " unit(s) of " + goods + " good(s) left the party " +
                          "without being sold, so what was paid for them is no longer held against a resale");
        }

        private Dictionary<string, PurchaseRecord> Paid
        {
            get { if (_paid == null) Reindex(); return _paid; }
        }

        private void Reindex()
        {
            _paid = new Dictionary<string, PurchaseRecord>();
            for (int i = 0; i < _purchases.Count; i++)
            {
                PurchaseRecord rec = _purchases[i];
                if (rec?.ItemId != null) _paid[rec.ItemId] = rec;
            }
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party != MobileParty.MainParty) return;
            Guard.Run("Ledger.OnSettlementEntered", () => CaptureSettlement(settlement));
        }

        private void OnDailyTick() => Guard.Run("Ledger.OnDailyTick", MatchPurchasesToWhatIsHeld);

        private void OnSessionLaunched(CampaignGameStarter starter) =>
            Guard.Run("Ledger.WatchTheParty", WatchTheParty);

        private void WatchTheParty()
        {
            ItemRoster carried = MobileParty.MainParty?.ItemRoster;
            if (carried == null || carried == _watched) return;
            if (_watched != null) _watched.RosterUpdatedEvent -= WhatLeftTheParty;
            _watched = carried;
            carried.RosterUpdatedEvent += WhatLeftTheParty;
        }

        private void WhatLeftTheParty(ItemRosterElement element, int count)
        {
            if (count < 0) _settle = true;
        }

        private void OnTick(float dt)
        {
            if (!_settle) return;
            _settle = false;
            Guard.Run("Ledger.OnTick", MatchPurchasesToWhatIsHeld);
        }

        private void OnPlayerInventoryExchange(
            List<(ItemRosterElement, int)> purchased,
            List<(ItemRosterElement, int)> sold, bool isTrading)
        {
            Guard.Run("Ledger.OnPlayerInventoryExchange", () =>
            {
                if (!isTrading || TradeActionBehavior.AutomatedTradeInProgress) return;
                Settlement here = Settlement.CurrentSettlement;
                SettlementComponent market = here?.SettlementComponent;
                foreach (var (element, count) in purchased)
                {
                    ItemObject item = element.EquipmentElement.Item;
                    if (item == null || count <= 0) continue;

                    int unit = market != null
                        ? market.GetItemPrice(element.EquipmentElement, MobileParty.MainParty, false)
                        : item.Value;
                    RecordPurchase(item.StringId, count,
                                   Bulk.PricePaid(here, element.EquipmentElement, count, unit));
                }
                foreach (var (element, count) in sold)
                {
                    ItemObject item = element.EquipmentElement.Item;
                    if (item == null || count <= 0) continue;
                    RecordSale(item.StringId, count);
                }
                CaptureSettlement(Settlement.CurrentSettlement, force: true);
            });
        }

        private string _capturedTown;
        private int _capturedHour = -1;
        private int _capturedGen = -1;

        public void CaptureSettlement(Settlement settlement, bool force = false) =>
            CaptureSettlement(settlement, force, null);

        public void CaptureSettlement(Settlement settlement, bool force, ISet<string> moved)
        {
            if (settlement == null || (!settlement.IsTown && !settlement.IsVillage)) return;
            SettlementComponent market = settlement.SettlementComponent;
            if (market == null) return;
            int hour = (int)CampaignTime.Now.ToHours;
            if (!force && hour == _capturedHour && settlement.StringId == _capturedTown &&
                Options.Generation == _capturedGen) return;
            _capturedHour = hour;
            _capturedTown = settlement.StringId;
            _capturedGen = Options.Generation;
            if (force || !Options.Current.Omniscient)
                DropRankings(settlement, Options.Current.Omniscient ? moved : null);
            if (Options.Current.Omniscient) return;
            float day = (float)CampaignTime.Now.ToDays;
            foreach (ItemObject item in Items.AllTradeGoods)
            {
                int buy = market.GetItemPrice(item, MobileParty.MainParty, false);
                int sell = market.GetItemPrice(item, MobileParty.MainParty, true);
                Record(item.StringId, settlement.StringId, buy, sell, day);
            }

            ItemRoster shelf = settlement.ItemRoster;
            if (shelf == null) return;
            for (int i = 0; i < shelf.Count; i++)
            {
                ItemObject item = shelf.GetElementCopyAtIndex(i).EquipmentElement.Item;
                if (item == null || item.IsTradeGood || !TradePolicy.Priced(item)) continue;
                int buy = market.GetItemPrice(item, MobileParty.MainParty, false);
                int sell = market.GetItemPrice(item, MobileParty.MainParty, true);
                Record(item.StringId, settlement.StringId, buy, sell, day);
            }
        }

        private void Record(string itemId, string townId, int buy, int sell, float day)
        {
            if (!_ledger.TryGetValue(itemId, out var byTown))
            {
                byTown = new Dictionary<string, PriceObservation>(StringComparer.Ordinal);
                _ledger[itemId] = byTown;
            }
            if (byTown.TryGetValue(townId, out PriceObservation seen))
            {
                seen.BuyPrice = buy;
                seen.SellPrice = sell;
                seen.CapturedDay = day;
                return;
            }
            byTown[townId] = new PriceObservation
            {
                ItemId = itemId, TownId = townId, BuyPrice = buy, SellPrice = sell, CapturedDay = day
            };
        }

        public void RecordPurchase(string itemId, int count, int totalPaid)
        {
            if (!Paid.TryGetValue(itemId, out var rec))
            {
                rec = new PurchaseRecord { ItemId = itemId, TotalPaid = 0, Count = 0 };
                Paid[itemId] = rec;
                _purchases.Add(rec);
            }
            TradeMath.AddPurchase(rec, count, totalPaid);
        }

        public void RecordSale(string itemId, int count)
        {
            if (Paid.TryGetValue(itemId, out var rec)) TradeMath.DrainSale(rec, count);
        }

        public bool HasPurchaseRecord(ItemObject item) =>
            item != null && Paid.TryGetValue(item.StringId, out var rec) && rec.Count > 0;

        public int PurchasedUnits(ItemObject item) =>
            item != null && Paid.TryGetValue(item.StringId, out var rec) && rec.Count > 0 ? rec.Count : 0;

        public int GetCostBasis(ItemObject item)
        {
            if (item == null) return 0;
            Paid.TryGetValue(item.StringId, out var rec);
            int unit = TradeMath.UnitBasis(rec, Options.Current.CostBasisMode);
            if (unit != TradeMath.NoRecordedBasis) return unit;
            var best = BestBuy(item);
            return best.price > 0 ? best.price : item.Value;
        }

        public (Settlement town, int price) BestSell(ItemObject item) => First(TopMarkets(item, selling: true));
        public (Settlement town, int price) BestBuy(ItemObject item) => First(TopMarkets(item, selling: false));

        public List<(Settlement town, int price)> TopSell(ItemObject item, int n) => TakeN(TopMarkets(item, true), n);
        public List<(Settlement town, int price)> TopBuy(ItemObject item, int n) => TakeN(TopMarkets(item, false), n);

        private static (Settlement, int) First(List<(Settlement, int)> list) =>
            list.Count == 0 ? (null, 0) : list[0];

        private static List<(Settlement, int)> TakeN(List<(Settlement, int)> list, int n) =>
            list.Count <= n ? list : list.GetRange(0, n);

        private List<(Settlement, int)> TopMarkets(ItemObject item, bool selling)
        {
            DropRankingsIfThePartyMoved();
            var key = (item.StringId, selling);
            int hour = (int)CampaignTime.Now.ToHours;
            if (_marketCache.TryGetValue(key, out var hit) && hit.hour == hour && hit.gen == Options.Generation)
                return hit.markets;

            var result = Options.Current.Omniscient
                ? TopLive(item, selling, hour)
                : TopObserved(item, selling);
            _marketCache[key] = (hour, Options.Generation, KindOf(item), result);
            return result;
        }

        internal static bool IsHostile(Settlement s)
        {
            IFaction mine = Hero.MainHero?.MapFaction;
            return mine != null && s.MapFaction != null &&
                   FactionManager.IsAtWarAgainstFaction(s.MapFaction, mine);
        }

        internal static bool UnderAttack(Settlement s) => s.IsUnderSiege || s.IsUnderRaid;

        internal static bool VillageShut(Settlement s)
        {
            Village v = s.Village;
            return v != null && v.VillageState != Village.VillageStates.Normal;
        }

        private const int UncappedBuyProjection = 500;

        internal static bool WithinRadius(Settlement s)
        {
            float radius = Options.Current.ScanRadius;
            if (radius <= 0f) return true;
            return MobileParty.MainParty.GetPosition2D.Distance(s.GetPosition2D) <= radius;
        }

        internal static int StockOf(Settlement s, ItemObject item)
        {
            try { return s.ItemRoster?.GetItemNumber(item) ?? 0; }
            catch { return 0; }
        }

        private static bool Eligible(Settlement s, out float lower)
        {
            lower = 0f;
            if (!TradeActionBehavior.IsMarket(s)) return false;
            if (UnderAttack(s) || VillageShut(s)) return false;
            if (Options.Current.ExcludeHostileTowns && IsHostile(s)) return false;
            if (!WithinRadius(s)) return false;
            lower = Travel.StraightDaysFromParty(s);
            return WithinTravelCeiling(s, lower);
        }

        private static bool WithinTravelCeiling(Settlement s, float days)
        {
            float cap = Options.Current.MaxTravelDays;
            float vcap = Options.Current.MaxVillageTravelDays;
            if (s.IsVillage && vcap > 0f && (cap <= 0f || vcap < cap)) cap = vcap;
            return cap <= 0f || days <= cap;
        }

        private static int Rank(bool selling, (Settlement s, int price, float days) x,
                                              (Settlement s, int price, float days) y)
        {
            int p = selling ? y.price.CompareTo(x.price) : x.price.CompareTo(y.price);
            return p != 0 ? p : x.days.CompareTo(y.days);
        }

        private static readonly Comparison<(Settlement s, int price, float days)> DearestFirst =
            (x, y) => Rank(true, x, y);

        private static readonly Comparison<(Settlement s, int price, float days)> CheapestFirst =
            (x, y) => Rank(false, x, y);

        private const int TopCacheSize = 8;
        private const float MovedFar = 100f;
        private readonly Dictionary<(string item, bool selling), (int hour, int gen, string kind, List<(Settlement, int)> markets)> _marketCache
            = new Dictionary<(string, bool), (int, int, string, List<(Settlement, int)>)>();

        private int _candHour = -1;
        private int _candGen = -1;
        private List<(Settlement s, float days)> _candidates;

        private int _routeHour = -1;
        private int _routeGen = -1;
        private int _routePurse = -1;
        private List<TradeRoute> _routes;

        internal void ForgetMarketRankings()
        {
            ForgetPricedRankings();
            _candidates = null;
        }

        private void ForgetPricedRankings()
        {
            _marketCache.Clear();
            _routes = null;
        }

        internal static string KindOf(ItemObject item) => item?.ItemCategory?.StringId;

        private static bool TillStillOpen(Settlement s) =>
            (s?.SettlementComponent?.Gold ?? 0) > 0;

        private void DropRankings(Settlement settlement, ISet<string> moved)
        {
            if (moved == null || moved.Count == 0 || !TillStillOpen(settlement))
            {
                ForgetPricedRankings();
                return;
            }
            var spent = new List<(string, bool)>();
            foreach (var kv in _marketCache)
                if (kv.Value.kind == null || moved.Contains(kv.Value.kind)) spent.Add(kv.Key);
            for (int i = 0; i < spent.Count; i++) _marketCache.Remove(spent[i]);
            _routes = null;
        }

        private Vec2 _rankedAt;

        private void DropRankingsIfThePartyMoved()
        {
            MobileParty party = MobileParty.MainParty;
            if (party == null) return;
            Vec2 at = party.GetPosition2D;
            if (at.DistanceSquared(_rankedAt) <= MovedFar) return;
            _rankedAt = at;
            ForgetMarketRankings();
        }

        private List<(Settlement s, float days)> LiveCandidates(int hour)
        {
            if (_candidates != null && _candHour == hour && _candGen == Options.Generation) return _candidates;

            var list = new List<(Settlement, float)>();
            foreach (Settlement s in Settlement.All)
            {
                if (s.SettlementComponent == null) continue;
                if (!Eligible(s, out float lower)) continue;
                list.Add((s, lower));
            }
            _candidates = list;
            _candHour = hour;
            _candGen = Options.Generation;
            return list;
        }

        private static List<(Settlement, int)> Rerank(List<(Settlement s, int price, float days)> all, bool selling)
        {
            var kept = new List<(Settlement s, int price, float straight, float days)>(TopCacheSize + 1);
            for (int i = 0; i < all.Count; i++)
            {
                float days = Travel.EstimateDaysFromParty(all[i].s);
                if (!WithinTravelCeiling(all[i].s, days)) continue;
                Keep(kept, (all[i].s, all[i].price, all[i].days, days), selling);
            }
            return Settled(kept, selling);
        }

        private static List<(Settlement, int)> Settled(
            List<(Settlement s, int price, float straight, float days)> kept, bool selling)
        {
            var top = new List<(Settlement s, int price, float days)>(kept.Count);
            for (int i = 0; i < kept.Count; i++) top.Add((kept[i].s, kept[i].price, kept[i].days));
            top.Sort(selling ? DearestFirst : CheapestFirst);
            var result = new List<(Settlement, int)>(top.Count);
            for (int i = 0; i < top.Count; i++)
                result.Add((top[i].s, top[i].price));
            return result;
        }

        private static (Settlement s, int price, float days) ByStraightLine(
            (Settlement s, int price, float straight, float days) one) => (one.s, one.price, one.straight);

        private static void Keep(List<(Settlement s, int price, float straight, float days)> kept,
                                 (Settlement s, int price, float straight, float days) one, bool selling)
        {
            if (kept.Count == TopCacheSize &&
                Rank(selling, ByStraightLine(one), ByStraightLine(kept[TopCacheSize - 1])) >= 0) return;
            int at = kept.Count;
            while (at > 0 && Rank(selling, ByStraightLine(one), ByStraightLine(kept[at - 1])) < 0) at--;
            kept.Insert(at, one);
            if (kept.Count > TopCacheSize) kept.RemoveAt(TopCacheSize);
        }

        private void PrimeLiveRankings(List<ItemObject> wanted, int hour)
        {
            if (!Options.Current.Omniscient || wanted.Count == 0) return;
            List<(Settlement s, float days)> candidates = LiveCandidates(hour);
            if (candidates.Count == 0) return;
            int minStock = Options.Current.MinTownStock;
            MobileParty me = MobileParty.MainParty;
            int n = wanted.Count;
            var sells = new List<(Settlement s, int price, float straight, float days)>[n];
            var buys = new List<(Settlement s, int price, float straight, float days)>[n];
            for (int i = 0; i < n; i++)
            {
                sells[i] = new List<(Settlement, int, float, float)>(TopCacheSize + 1);
                buys[i] = new List<(Settlement, int, float, float)>(TopCacheSize + 1);
            }
            for (int t = 0; t < candidates.Count; t++)
            {
                Settlement town = candidates[t].s;
                float straight = candidates[t].days;
                float days = Travel.EstimateDaysFromParty(town);
                if (!WithinTravelCeiling(town, days)) continue;
                SettlementComponent market = town.SettlementComponent;
                bool tillOpen = market.Gold > 0;
                for (int i = 0; i < n; i++)
                {
                    ItemObject item = wanted[i];
                    if (tillOpen)
                    {
                        int price = market.GetItemPrice(item, me, true);
                        if (price > 0) Keep(sells[i], (town, price, straight, days), true);
                    }
                    if (minStock <= 0 || StockOf(town, item) >= minStock)
                    {
                        int price = market.GetItemPrice(item, me, false);
                        if (price > 0) Keep(buys[i], (town, price, straight, days), false);
                    }
                }
            }
            for (int i = 0; i < n; i++)
            {
                ItemObject item = wanted[i];
                string kind = KindOf(item);
                _marketCache[(item.StringId, true)] = (hour, Options.Generation, kind, Settled(sells[i], true));
                _marketCache[(item.StringId, false)] = (hour, Options.Generation, kind, Settled(buys[i], false));
            }
        }

        private List<(Settlement, int)> TopLive(ItemObject item, bool selling, int hour)
        {
            int minStock = Options.Current.MinTownStock;
            var all = new List<(Settlement s, int price, float days)>();
            foreach (var (s, lower) in LiveCandidates(hour))
            {
                if (selling && s.SettlementComponent.Gold <= 0) continue;
                if (!selling && minStock > 0 && StockOf(s, item) < minStock) continue;
                int price = s.SettlementComponent.GetItemPrice(item, MobileParty.MainParty, selling);
                if (price <= 0) continue;
                all.Add((s, price, lower));
            }
            return Rerank(all, selling);
        }

        public float ObservationAgeDays(ItemObject item, Settlement town)
        {
            if (item == null || town == null) return -1f;
            if (!_ledger.TryGetValue(item.StringId, out var byTown)) return -1f;
            if (byTown.TryGetValue(town.StringId, out PriceObservation seen))
                return (float)CampaignTime.Now.ToDays - seen.CapturedDay;
            return -1f;
        }

        internal static Dictionary<Settlement, int> CaravanPressure()
        {
            var map = new Dictionary<Settlement, int>();
            var all = MobileParty.All;
            for (int i = 0; i < all.Count; i++)
            {
                MobileParty p = all[i];
                if (!p.IsCaravan) continue;
                Settlement at = p.CurrentSettlement, to = p.TargetSettlement;
                if (at != null) Bump(map, at);
                if (to != null && to != at) Bump(map, to);
            }
            return map;
        }

        private static void Bump(Dictionary<Settlement, int> map, Settlement s)
        {
            map.TryGetValue(s, out int n);
            map[s] = n + 1;
        }

        private List<(Settlement, int)> TopObserved(ItemObject item, bool selling)
        {
            if (!_ledger.TryGetValue(item.StringId, out var byTown) || byTown.Count == 0)
                return new List<(Settlement, int)>();
            var found = new List<(Settlement s, int price, float days)>();
            foreach (var o in byTown.Values)
            {
                Settlement town = Settlement.Find(o.TownId);
                if (town == null || !Eligible(town, out float lower)) continue;
                int price = selling ? o.SellPrice : o.BuyPrice;
                if (price <= 0) continue;
                found.Add((town, price, lower));
            }
            return Rerank(found, selling);
        }

        public List<TradeRoute> BestRoutes(int top)
        {
            DropRankingsIfThePartyMoved();
            int hour = (int)CampaignTime.Now.ToHours;
            int purse = TradeActionBehavior.PurseForAVisit();
            if (_routes == null || _routeHour != hour || _routeGen != Options.Generation
                || _routePurse != purse)
            {
                _routes = ScanRoutes(purse);
                _routeHour = hour;
                _routeGen = Options.Generation;
                _routePurse = purse;
            }
            return _routes.Count <= top ? _routes : _routes.GetRange(0, top);
        }

        private List<TradeRoute> ScanRoutes(int purse)
        {
            Bulk.Forget();
            var routes = new List<TradeRoute>();
            if (purse <= 0) return routes;
            ISet<string> locked = TradePolicy.LockedKeys();
            float cap = Options.Current.MaxTravelDays;
            bool rankByScore = Options.Current.ConfidenceRanking;
            var pressure = CaravanPressure();
            int herdRoom = -1;
            var wanted = new List<ItemObject>();
            foreach (ItemObject item in Items.All)
            {
                if (!TradePolicy.Priced(item)) continue;
                if (!TradePolicy.MayRoundTrip(item, locked)) continue;
                wanted.Add(item);
            }
            PrimeLiveRankings(wanted, (int)CampaignTime.Now.ToHours);
            for (int at = 0; at < wanted.Count; at++)
            {
                ItemObject item = wanted[at];

                var buys = TopBuy(item, TopCacheSize);
                var sells = TopSell(item, TopCacheSize);
                if (buys.Count == 0 || sells.Count == 0) continue;

                TradeRoute best = null;
                float bestKey = 0f;
                foreach (var (from, buyPrice) in buys)
                {
                    if (buyPrice <= 0) continue;
                    if (!TradePolicy.BuyAcceptable(buyPrice, TradePolicy.Realizable(sells[0].price))) break;

                    int stocked = Options.Current.BuyCapPerItem > 0
                        ? Options.Current.BuyCapPerItem : UncappedBuyProjection;
                    int spendCap = Options.Current.BuyValueCapPerItem;
                    if (spendCap <= 0 || purse < spendCap) spendCap = purse;
                    if (spendCap > 0) stocked = Math.Min(stocked, spendCap / buyPrice);
                    if (item.HasHorseComponent)
                    {
                        if (herdRoom < 0) herdRoom = TradeActionBehavior.HerdRoomForLivestock(MobileParty.MainParty);
                        stocked = Math.Min(stocked, herdRoom);
                    }
                    int shelf = 0;
                    if (Options.Current.Omniscient)
                    {
                        shelf = StockOf(from, item) - (from.IsVillage ? 1 : 0);
                        stocked = Math.Min(stocked, shelf);
                    }
                    if (stocked <= 0) continue;

                    float toBuy = Travel.EstimateDaysFromParty(from);
                    foreach (var (to, sellPrice) in sells)
                    {
                        float realizable = TradePolicy.Realizable(sellPrice);
                        if (sellPrice <= buyPrice || !TradePolicy.BuyAcceptable(buyPrice, realizable)) break;
                        if (to == from) continue;

                        int till = 0;
                        if (Options.Current.Omniscient)
                        {
                            till = to.SettlementComponent?.Gold ?? 0;
                            if (till <= 0) continue;
                        }
                        int qtyCap = till > 0 ? Math.Min(stocked, till / sellPrice) : stocked;
                        if (qtyCap <= 0) continue;

                        float ceiling = (float)(sellPrice - buyPrice) * qtyCap;
                        float soonest = toBuy + Travel.StraightDaysBetween(from, to);
                        if (cap > 0f && soonest > cap) continue;
                        if (best != null && ceiling / Math.Max(soonest, 0.25f) <= bestKey) continue;

                        float days = toBuy + Travel.EstimateDaysBetween(from, to);
                        if (cap > 0f && days > cap) continue;
                        if (best != null && ceiling / Math.Max(days, 0.25f) <= bestKey) continue;

                        RouteQuote q = Bulk.Walk(from, to, item, qtyCap, till, spendCap, buyPrice, sellPrice);
                        if (q.Units <= 0) continue;

                        int proceeds = Options.Current.ConservativeRouteProjection
                            ? (int)TradePolicy.Realizable(q.SellTotal)
                            : q.SellTotal;
                        int profit = proceeds - q.BuyTotal;
                        if (profit <= 0) continue;

                        float age = Options.Current.Omniscient ? -1f
                            : Math.Max(ObservationAgeDays(item, from), ObservationAgeDays(item, to));
                        int caravans = Pressure(pressure, from) + Pressure(pressure, to);
                        int flatSell = q.OpeningSellPrice * q.Units;
                        int flat = (Options.Current.ConservativeRouteProjection
                                        ? (int)TradePolicy.Realizable(flatSell)
                                        : flatSell) - q.OpeningBuyPrice * q.Units;
                        float confidence = Confidence.Of(q.Simulated, flat, profit, shelf,
                                                         q.Units, days, caravans, age);
                        float perDay = profit / Math.Max(days, 0.25f);
                        float key = rankByScore ? perDay * confidence : perDay;
                        if (best != null && key <= bestKey) continue;

                        bestKey = key;
                        best = new TradeRoute
                        {
                            Item = item, From = from, To = to,
                            BuyPrice = buyPrice, SellPrice = sellPrice,
                            Quantity = q.Units,
                            TravelDays = days, TotalProfit = profit, ProfitPerDay = perDay,
                            Confidence = confidence, Score = perDay * confidence,
                            Simulated = q.Simulated, Caravans = caravans, DataAgeDays = age
                        };
                    }
                }
                if (best == null) continue;
                routes.Add(best);
            }
            routes.Sort((x, y) => rankByScore
                ? y.Score.CompareTo(x.Score)
                : y.ProfitPerDay.CompareTo(x.ProfitPerDay));
            return routes;
        }

        private static int Pressure(Dictionary<Settlement, int> map, Settlement s) =>
            s != null && map.TryGetValue(s, out int n) ? n : 0;
    }
}
