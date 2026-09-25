using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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
        public bool StillComing;
        public int Caravans;

        public float DataAgeDays = -1f;

        public float RunsOutInDays = Projection.NeverRunsOut;
    }

    public class LedgerBehavior : CampaignBehaviorBase
    {
        public static LedgerBehavior Instance { get; internal set; }

        private Dictionary<string, Dictionary<string, PriceObservation>> _ledger =
            new Dictionary<string, Dictionary<string, PriceObservation>>();
        private List<PurchaseRecord> _purchases = new List<PurchaseRecord>();
        private string _ledgerText = "";
        private string _purchaseText = "";
        private int _unreadable;
        private Dictionary<string, PurchaseRecord> _paid;
        private long _lifetimeProfit;
        private long _lifetimeTradeXp;
        private int _lifetimeProfitCapped;
        private int _promisesScored;
        private float _promiseHeld;
        private int _forecastsScored;
        private float _forecastMissed;
        private string _promiseText = "";
        private Dictionary<string, PromiseRecord> _promises =
            new Dictionary<string, PromiseRecord>(StringComparer.Ordinal);
        private string _latelyText = "";
        private ItemRoster _watched;
        private bool _settle;
        private bool _villagePursesPutBack;

        private readonly List<TradeNote> _lately = new List<TradeNote>();

        public long LifetimeProfit => _lifetimeProfit;
        public void AddProfit(int amount) => _lifetimeProfit += amount;

        public long LifetimeTradeXp => _lifetimeTradeXp;
        public void AddTradeXp(int amount) { if (amount > 0) _lifetimeTradeXp += amount; }

        public IList<TradeNote> Lately => _lately;

        public void NoteTrade(string where, string what, int gold, float day) =>
            Recent.Keep(_lately, new TradeNote
            {
                Where = where ?? "",
                What = what ?? "",
                Gold = gold,
                Day = day,
            }, Recent.MostKept);

        internal void KeepPromiseScore(float held)
        {
            if (held < 0f) return;
            _promisesScored++;
            _promiseHeld += held;
        }

        internal void KeepArrival(string townId, float held)
        {
            if (townId == null || held < 0f) return;
            if (!_promises.TryGetValue(townId, out PromiseRecord rec))
            {
                rec = new PromiseRecord { TownId = townId };
                _promises[townId] = rec;
            }
            TradeMath.AddPromise(rec, held);
        }

        internal bool PromiseScoreAt(string townId, out int scored, out float held)
        {
            scored = 0;
            held = 0f;
            if (townId == null || !_promises.TryGetValue(townId, out PromiseRecord rec)) return false;
            float mean = TradeMath.PromiseMean(rec);
            if (mean == TradeMath.NoShareToGive) return false;
            scored = rec.Scored;
            held = mean;
            return true;
        }

        internal bool PromiseScore(out int scored, out float held)
        {
            scored = _promisesScored;
            held = TradeMath.MeanOf(_promiseHeld, _promisesScored);
            return scored > 0;
        }

        internal void KeepForecastScore(float missed)
        {
            if (missed < 0f || float.IsNaN(missed) || float.IsInfinity(missed)) return;
            _forecastsScored++;
            _forecastMissed += missed;
        }

        internal bool ForecastScore(out int scored, out float missed)
        {
            scored = _forecastsScored;
            missed = TradeMath.MeanOf(_forecastMissed, _forecastsScored);
            return scored > 0;
        }

        private static int Capped(long total) =>
            total > int.MaxValue ? int.MaxValue : total < int.MinValue ? int.MinValue : (int)total;

        public LedgerBehavior() { Instance = this; }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener(this, OnPlayerInventoryExchange);
            CampaignEvents.OnPlayerTradeProfitEvent.AddNonSerializedListener(this, OnPlayerTradeProfit);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (!dataStore.IsLoading) PruneExpired();
            if (dataStore.IsSaving)
                Guard.Run("Ledger.WriteForSave", () =>
                {
                    _ledgerText = LedgerCodec.WriteLedger(Listed(_ledger));
                    _purchaseText = LedgerCodec.WritePurchases(_purchases);
                    _promiseText = LedgerCodec.WritePromises(new List<PromiseRecord>(_promises.Values));
                    _latelyText = LedgerCodec.WriteTrades(_lately);
                    Log.Write("ledger written into the save: " + RecordedPrices() + " recorded price(s) in " +
                              _ledgerText.Length + " character(s), and " + _purchases.Count +
                              " purchase record(s) in " + _purchaseText.Length);
                });
            _lifetimeProfitCapped = Capped(_lifetimeProfit);
            dataStore.SyncData("TradeLord_LedgerText", ref _ledgerText);
            dataStore.SyncData("TradeLord_PurchaseText", ref _purchaseText);
            dataStore.SyncData("TradeLord_LifetimeProfit", ref _lifetimeProfitCapped);
            dataStore.SyncData("TradeLord_LifetimeProfitWide", ref _lifetimeProfit);
            dataStore.SyncData("TradeLord_LifetimeTradeXp", ref _lifetimeTradeXp);
            dataStore.SyncData("TradeLord_PromisesScored", ref _promisesScored);
            dataStore.SyncData("TradeLord_PromiseHeld", ref _promiseHeld);
            dataStore.SyncData("TradeLord_PromiseText", ref _promiseText);
            dataStore.SyncData("TradeLord_ForecastsScoredWithLeaving", ref _forecastsScored);
            dataStore.SyncData("TradeLord_ForecastMissedWithLeaving", ref _forecastMissed);
            dataStore.SyncData("TradeLord_LatelyText", ref _latelyText);
            dataStore.SyncData("TradeLord_VillagePursesPutBack", ref _villagePursesPutBack);
            if (dataStore.IsLoading && _lifetimeProfit == 0L) _lifetimeProfit = _lifetimeProfitCapped;
            if (dataStore.IsLoading) ReadSavedText();
            if (dataStore.IsLoading) PruneExpired();
            Guard.Run("Ledger.Reindex", Reindex);
            if (dataStore.IsLoading)
                Log.Write("ledger restored: " + _ledger.Count + " observed items, " +
                          _purchases.Count + " purchase records, " + _lately.Count +
                          " recent trade(s), lifetime profit " + _lifetimeProfit +
                          ", lifetime trade XP " + _lifetimeTradeXp +
                          (_unreadable == 0
                               ? ""
                               : ", and " + _unreadable + " recorded price(s) this version could not read, " +
                                 "which happens when a save was written by a newer TradeLord than this one"));
        }

        private void ReadSavedText() => Guard.Run("Ledger.ReadSaved", RestoreSaved);

        private void RestoreSaved()
        {
            _ledger = KeyedByTown(LedgerCodec.ReadLedger(_ledgerText, out _unreadable));
            _purchases = LedgerCodec.ReadPurchases(_purchaseText);
            _promises = KeyedByTownId(LedgerCodec.ReadPromises(_promiseText));
            _lately.Clear();
            _lately.AddRange(LedgerCodec.ReadTrades(_latelyText, Recent.MostKept));
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

        private static Dictionary<string, PromiseRecord> KeyedByTownId(List<PromiseRecord> listed)
        {
            var book = new Dictionary<string, PromiseRecord>(StringComparer.Ordinal);
            for (int i = 0; listed != null && i < listed.Count; i++)
                if (listed[i]?.TownId != null) book[listed[i].TownId] = listed[i];
            return book;
        }

        private void PruneExpired() => Guard.Run("Ledger.Prune", Prune);

        private void Prune()
        {
            PruneObservations();
            TrimToWhatItKeeps();
            PruneSettledPurchases();
            TrimThePromisesKept();
        }

        private void TrimThePromisesKept()
        {
            if (_promises == null || _promises.Count <= Kept.MostPromisesKept) return;
            var held = new List<PromiseRecord>(_promises.Values);
            held.Sort((x, y) => y.Scored.CompareTo(x.Scored));
            int over = held.Count - Kept.MostPromisesKept;
            for (int i = 0; i < over; i++) _promises.Remove(held[held.Count - 1 - i].TownId);
            Log.Write("market records forgotten: " + over + " of the least walked, because TradeLord " +
                      "keeps at most " + Kept.MostPromisesKept + " markets' records of what they paid");
        }

        private int RecordedPrices()
        {
            int held = 0;
            if (_ledger == null) return held;
            foreach (var kv in _ledger) held += kv.Value == null ? 0 : kv.Value.Count;
            return held;
        }

        private void TrimToWhatItKeeps()
        {
            if (_ledger == null) return;
            var held = new List<Reading>();
            foreach (var kv in _ledger)
            {
                if (kv.Value == null) continue;
                foreach (var seen in kv.Value)
                {
                    if (seen.Value == null) continue;
                    held.Add(new Reading
                    {
                        Item = kv.Key, Town = seen.Key, Day = seen.Value.CapturedDay
                    });
                }
            }
            var dropped = Kept.OldestBeyond(held, Kept.MostPricesKept);
            if (dropped.Count == 0) return;
            for (int i = 0; i < dropped.Count; i++)
                if (_ledger.TryGetValue(dropped[i].Item, out var byTown))
                {
                    byTown.Remove(dropped[i].Town);
                    if (byTown.Count == 0) _ledger.Remove(dropped[i].Item);
                }
            Log.Write("prices forgotten: " + dropped.Count + " of the oldest, because TradeLord keeps at most " +
                      Kept.MostPricesKept + " recorded prices so a campaign cannot grow your save without end");
        }

        private void PruneObservations()
        {
            if (_ledger == null) return;
            float now = (float)CampaignTime.Now.ToDays;
            int shelfLife = Options.Current.ObservationShelfLifeDays;
            int tooOld = 0, stillKept = 0;
            var spent = new List<string>();
            foreach (var kv in _ledger)
            {
                if (kv.Value == null) { spent.Add(kv.Key); continue; }
                var dead = new List<string>();
                foreach (var seen in kv.Value)
                {
                    if (seen.Value == null || seen.Value.TownId == null) { dead.Add(seen.Key); continue; }
                    if (TradeMath.WorthKeeping(seen.Value.CapturedDay, now, shelfLife)) { stillKept++; continue; }
                    dead.Add(seen.Key);
                    tooOld++;
                }
                for (int i = 0; i < dead.Count; i++) kv.Value.Remove(dead[i]);
                if (kv.Value.Count == 0) spent.Add(kv.Key);
            }
            for (int i = 0; i < spent.Count; i++) _ledger.Remove(spent[i]);
            if (tooOld > 0)
                Log.Write("prices forgotten: " + tooOld + " older than " + shelfLife +
                          " day(s), " + stillKept + " still kept");
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
                string key = PaidKey(el.EquipmentElement);
                if (key == null || el.Amount <= 0) continue;
                held.TryGetValue(key, out int had);
                held[key] = had + el.Amount;
            }
            int goods = 0, units = 0;
            var dropped = new List<string>();
            for (int i = 0; i < _purchases.Count; i++)
            {
                PurchaseRecord rec = _purchases[i];
                if (rec?.ItemId == null || rec.Count <= 0) continue;
                held.TryGetValue(rec.ItemId, out int have);
                if (rec.Count <= have) continue;
                int gone = rec.Count - have;
                goods++;
                units += gone;
                dropped.Add(gone + " " + rec.ItemId);
                TradeMath.DrainSale(rec, gone);
            }
            if (goods > 0)
                Log.Write("purchase record: " + units + " unit(s) of " + goods + " good(s) left the party " +
                          "without being sold, so what was paid for them is no longer held against a resale: " +
                          string.Join(", ", dropped.ToArray()));
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

        private void OnDailyTick() => Guard.Run("Ledger.OnDailyTick", () =>
        {
            MatchPurchasesToWhatIsHeld();
            PruneObservations();
        });

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            Guard.Run("Ledger.WatchTheParty", WatchTheParty);
            Guard.Run("Ledger.VillagePurses", PutBackEmptyVillagePurses);
        }

        private void PutBackEmptyVillagePurses()
        {
            if (_villagePursesPutBack) return;
            _villagePursesPutBack = true;
            int putBack = 0;
            foreach (Settlement s in Settlement.All)
            {
                Village village = s?.Village;
                if (village == null) continue;
                int owed = TradeRules.PutBackIntoAnEmptyPurse(village.Gold);
                if (owed <= 0) continue;
                village.ChangeGold(owed);
                putBack++;
                Log.Write("village purse: " + s.Name + " had nothing left to trade with, so " + owed +
                          " denars were put back and it holds " + village.Gold + " now");
            }
            if (putBack == 0)
            {
                Log.Write("village purses: none of them were empty, so nothing was put back");
                return;
            }
            Log.Write("village purses: " + putBack + " village(s) were put back to " +
                      TradeRules.VillagePurse + " denars, which is done once for a campaign");
            TextObject line = Tongue.Text("{=TL449}TradeLord has refilled {COUNT} empty village purse(s), so those village shops can trade again.");
            line.SetTextVariable("COUNT", putBack);
            Notices.Say(line);
        }

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

        private static bool TheGameCreditedADealTradeLordLaidOut =>
            !TradeActionBehavior.TradeLordIsCreditingItsOwnTrade && Counter.Awaiting;

        private void OnPlayerTradeProfit(int profit)
        {
            Guard.Run("Ledger.OnPlayerTradeProfit", () =>
            {
                if (!TheGameCreditedADealTradeLordLaidOut) return;
                AddTradeXp(Counter.TradeXpEarnedOnTheScreen());
            });
        }

        private void OnPlayerInventoryExchange(
            List<(ItemRosterElement, int)> purchased,
            List<(ItemRosterElement, int)> sold, bool isTrading)
        {
            Guard.Run("Ledger.OnPlayerInventoryExchange", () =>
            {
                if (!isTrading || TradeActionBehavior.AutomatedTradeInProgress) return;
                if (Counter.Awaiting)
                    Guard.Run("Counter.TookTheDeal",
                              () => TradeActionBehavior.TookTheDeal(purchased, sold));
                Settlement here = Settlement.CurrentSettlement;
                SettlementComponent market = here?.SettlementComponent;
                ItemRoster carried = MobileParty.MainParty?.ItemRoster;
                var moved = new List<(ItemObject item, int intoTheMarket)>();
                foreach (var (element, said) in purchased)
                {
                    ItemObject item = element.EquipmentElement.Item;
                    if (item == null || said <= 0) continue;
                    int unit = market != null
                        ? Priced.At(market, element.EquipmentElement, MobileParty.MainParty, false)
                        : item.Value;
                    int bought = Deals.UnitsMoved(element.Amount, said, unit);
                    moved.Add((item, -bought));
                    int took = Math.Min(bought, InAll(carried, element.EquipmentElement));
                    if (took <= 0) continue;
                    RecordPurchase(PaidKey(element.EquipmentElement), took,
                                   Deals.PaidForWhatYouKept(said, bought, took));
                }
                foreach (var (element, said) in sold)
                {
                    ItemObject item = element.EquipmentElement.Item;
                    if (item == null || said <= 0) continue;
                    int unit = market != null
                        ? Priced.At(market, element.EquipmentElement, MobileParty.MainParty, true)
                        : item.Value;
                    int gone = Deals.UnitsMoved(element.Amount, said, unit);
                    moved.Add((item, gone));
                    RecordSale(PaidKey(element.EquipmentElement), gone);
                }
                Hindsight.YouTraded(here, moved);
                CaptureSettlement(Settlement.CurrentSettlement, force: true);
                PriceTrace.Say(Settlement.CurrentSettlement, "traded by hand");
            });
        }

        private string _capturedTown;
        private Stamp _capturedStamp;

        public void CaptureSettlement(Settlement settlement, bool force = false) =>
            CaptureSettlement(settlement, force, null);

        public void CaptureSettlement(Settlement settlement, bool force, ISet<string> moved)
        {
            if (settlement == null || (!settlement.IsTown && !settlement.IsVillage)) return;
            SettlementComponent market = settlement.SettlementComponent;
            if (market == null) return;
            if (!force && settlement.StringId == _capturedTown &&
                Freshness.Fresh(ref _capturedStamp)) return;
            Freshness.Taken(ref _capturedStamp);
            _capturedTown = settlement.StringId;
            if (force || !Options.Current.Omniscient)
                DropRankings(settlement, Options.Current.Omniscient ? moved : null);
            if (Options.Current.Omniscient) return;
            float day = (float)CampaignTime.Now.ToDays;
            foreach (ItemObject item in Items.AllTradeGoods)
            {
                int buy = Priced.At(market, item, MobileParty.MainParty, false);
                int sell = Priced.At(market, item, MobileParty.MainParty, true);
                Record(item.StringId, settlement.StringId, buy, sell, day);
            }

            ItemRoster shelf = settlement.ItemRoster;
            if (shelf == null) return;
            for (int i = 0; i < shelf.Count; i++)
            {
                ItemObject item = shelf.GetElementCopyAtIndex(i).EquipmentElement.Item;
                if (item == null || item.IsTradeGood || !TradePolicy.Priced(item)) continue;
                int buy = Priced.At(market, item, MobileParty.MainParty, false);
                int sell = Priced.At(market, item, MobileParty.MainParty, true);
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
                if (TradeMath.ReadingIsNew(day, seen.CapturedDay))
                {
                    seen.WasBuyPrice = seen.BuyPrice;
                    seen.WasSellPrice = seen.SellPrice;
                    seen.WasDay = seen.CapturedDay;
                }
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

        internal static string PaidKey(EquipmentElement el) =>
            el.Item == null ? null : LedgerCodec.PaidKey(el.Item.StringId, el.ItemModifier?.StringId);

        public bool HasPurchaseRecord(EquipmentElement el) =>
            el.Item != null && Paid.TryGetValue(PaidKey(el), out var rec) && rec.Count > 0;

        public int PurchasedUnits(EquipmentElement el) =>
            el.Item != null && Paid.TryGetValue(PaidKey(el), out var rec) && rec.Count > 0 ? rec.Count : 0;

        public int GetCostBasis(EquipmentElement el)
        {
            ItemObject item = el.Item;
            if (item == null) return 0;
            Paid.TryGetValue(PaidKey(el), out var rec);
            int unit = TradeMath.UnitBasis(rec, Options.Current.CostBasisMode);
            if (unit != TradeMath.NoRecordedBasis) return unit;
            var best = BestBuy(item);
            return best.price > 0 ? best.price : item.Value;
        }

        public int PaidPerUnit(EquipmentElement el)
        {
            if (el.Item == null) return TradeMath.NoRecordedBasis;
            Paid.TryGetValue(PaidKey(el), out var rec);
            return TradeMath.UnitBasis(rec, 0);
        }

        public (Settlement town, int price) BestSell(ItemObject item) => First(TopMarkets(item, selling: true));
        public (Settlement town, int price) BestBuy(ItemObject item) => First(TopMarkets(item, selling: false));

        internal static int BuyerWalks;
        internal static int BuyerRungs;
        internal static long BuyerTicks;

        internal static void ForgetWhatPickingABuyerCost()
        {
            BuyerWalks = 0;
            BuyerRungs = 0;
            BuyerTicks = 0L;
        }

        internal (Settlement town, int price, Ladder rungs) WhereThisEarnsFastest(ItemObject item, int paid,
                                                                 int units, Settlement notHere)
        {
            var shortlist = new List<(Settlement town, int price, float days, float rate)>();
            var markets = EverySell(item);
            for (int i = 0; i < markets.Count; i++)
            {
                var (town, price) = markets[i];
                if (town == null || price <= 0 || town == notHere) continue;
                float days = Travel.EstimateDaysFromParty(town);
                if (TradeMath.OutOfReach(days)) continue;
                float rate = TradeMath.EarnedPerDay(price, paid, days);
                int at = shortlist.Count;
                while (at > 0 && rate > shortlist[at - 1].rate) at--;
                shortlist.Insert(at, (town, price, days, rate));
            }
            if (shortlist.Count == 0) return (null, 0, null);
            var flat = shortlist[0];
            if (!Options.Current.PickTheBuyerOnTheWholeStack || units <= 1 || shortlist.Count == 1)
                return (flat.town, flat.price, null);

            long started = System.DateTime.UtcNow.Ticks;
            var deep = flat;
            Ladder deepRungs = null;
            Fetched deepGot = default(Fetched);
            float bestRate = float.MinValue;
            for (int i = 0; i < shortlist.Count; i++)
            {
                var one = shortlist[i];
                int purse = Options.Current.Omniscient
                    ? TradeRules.WhatTheTillCanPay(one.town.SettlementComponent?.Gold ?? 0, one.town.IsVillage)
                    : 0;
                Fetched got = Bulk.SellWalk(one.town, item, units, one.price, paid, purse);
                BuyerWalks++;
                BuyerRungs += got.Walked;
                float rate = TradeMath.PerDay(got.Total - (long)paid * got.Units, one.days);
                if (rate <= bestRate) continue;
                bestRate = rate;
                deep = one;
                deepRungs = got.Rungs;
                deepGot = got;
            }
            BuyerTicks += System.DateTime.UtcNow.Ticks - started;
            if (deep.town != flat.town && Options.Current.ExtendedDebugLogging)
                Log.Write("buyer for " + Tongue.Named(item.Name, item.StringId) + ": all " + units +
                          " unit(s) weighed picked " + deep.town.Name + ", which pays " +
                          TradeMath.PerUnit(deepGot.Total, deepGot.Units) + " a unit on average for the " +
                          deepGot.Units + " unit(s) that clear your margin and its purse there, " + deep.price +
                          " for the first, where the first unit alone would have picked " +
                          flat.town.Name + " at " + flat.price);
            return (deep.town, deep.price, deepRungs);
        }

        public List<(Settlement town, int price)> TopSell(ItemObject item, int n) => TakeN(TopMarkets(item, true), n);
        public List<(Settlement town, int price)> EverySell(ItemObject item) =>
            TakeN(TopMarkets(item, true), int.MaxValue);
        public List<(Settlement town, int price)> TopBuy(ItemObject item, int n) => TakeN(TopMarkets(item, false), n);

        internal bool AnyMarketFor(ItemObject item)
        {
            if (item == null) return false;
            int sells = TopMarkets(item, true).Count;
            int buys = TopMarkets(item, false).Count;
            return sells > 0 || buys > 0;
        }

        private static (Settlement, int) First(List<(Settlement, int)> list) =>
            list.Count == 0 ? (null, 0) : list[0];

        private static List<(Settlement, int)> TakeN(List<(Settlement, int)> list, int n) =>
            MarketRank.TopFew(list, n);

        private List<(Settlement, int)> TopMarkets(ItemObject item, bool selling)
        {
            DropRankingsIfThePartyMoved();
            var key = (item.StringId, selling);
            int hour = (int)CampaignTime.Now.ToHours;
            if (_marketCache.TryGetValue(key, out var hit) && Freshness.Held(hit.stamp, hour))
                return hit.markets;

            var result = Options.Current.Omniscient
                ? TopLive(item, selling, hour)
                : TopObserved(item, selling);
            if (!Travel.LostTheRoad) _marketCache[key] = (Freshness.At(hour), KindOf(item), result);
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

        internal static int InAll(ItemRoster roster, ItemObject item)
        {
            int held = 0;
            for (int i = 0; roster != null && i < roster.Count; i++)
                if (roster.GetItemAtIndex(i) == item) held += roster.GetElementNumber(i);
            return held;
        }

        internal static int InAll(ItemRoster roster, EquipmentElement el)
        {
            int held = 0;
            for (int i = 0; roster != null && i < roster.Count; i++)
            {
                EquipmentElement at = roster.GetElementCopyAtIndex(i).EquipmentElement;
                if (at.Item == el.Item && at.ItemModifier == el.ItemModifier) held += roster.GetElementNumber(i);
            }
            return held;
        }

        internal static int StockOf(Settlement s, ItemObject item)
        {
            try { return InAll(s.ItemRoster, item); }
            catch { return 0; }
        }

        private static Dictionary<ItemObject, int> WhatItStocks(Settlement s)
        {
            var held = new Dictionary<ItemObject, int>();
            ItemRoster shelf = s.ItemRoster;
            for (int i = 0; shelf != null && i < shelf.Count; i++)
            {
                ItemObject item = shelf.GetItemAtIndex(i);
                if (item == null) continue;
                held.TryGetValue(item, out int had);
                held[item] = had + shelf.GetElementNumber(i);
            }
            return held;
        }

        private static bool Eligible(Settlement s, out float lower)
        {
            lower = 0f;
            if (!TradeActionBehavior.IsMarket(s)) return false;
            if (UnderAttack(s) || VillageShut(s)) return false;
            if (Options.Current.ExcludeHostileTowns && IsHostile(s)) return false;
            lower = Travel.StraightDaysFromParty(s);
            return WithinTravelCeiling(s, lower);
        }

        internal static float TravelCeiling(Settlement s) =>
            MarketRank.Ceiling(s.IsVillage, Options.Current);

        private static bool WithinTravelCeiling(Settlement s, float days) =>
            MarketRank.WithinCeiling(s.IsVillage, days, Options.Current);

        private const float MovedFar = 100f;
        private readonly Dictionary<(string item, bool selling), (Stamp stamp, string kind, List<(Settlement, int)> markets)> _marketCache
            = new Dictionary<(string, bool), (Stamp, string, List<(Settlement, int)>)>();

        private Stamp _candStamp;
        private List<(Settlement s, float days)> _candidates;

        private Stamp _routeStamp;
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
            TradeRules.WhatTheTillCanPay(s?.SettlementComponent?.Gold ?? 0,
                                         s != null && s.IsVillage) > 0;

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
            if (_candidates != null && Freshness.Fresh(ref _candStamp, hour)) return _candidates;

            var list = new List<(Settlement, float)>();
            foreach (Settlement s in Settlement.All)
            {
                if (s.SettlementComponent == null) continue;
                if (!Eligible(s, out float lower)) continue;
                list.Add((s, lower));
            }
            _candidates = list;
            Freshness.Taken(ref _candStamp, hour);
            SayIfTheTownCeilingIsOff(list.Count);
            return list;
        }

        private static void SayIfTheTownCeilingIsOff(int weighed)
        {
            if (Options.Current.MaxTravelDaysTown > 0f) return;
            Log.Repeatable("town travel ceiling", weighed.ToString(),
                           "the town travel ceiling is off, so nothing is held back by distance and " +
                           weighed + " market(s) are weighed for every good on every pass. That is the " +
                           "slowest TradeLord runs. Set Town travel ceiling above 0 if the map runs " +
                           "roughly.");
        }

        private static List<(Settlement, int)> Rerank(List<(Settlement s, int price, float days)> all, bool selling)
        {
            var kept = new List<Reach<Settlement>>(MarketRank.TopCacheSize + 1);
            for (int i = 0; i < all.Count; i++)
            {
                float days = Travel.EstimateDaysFromParty(all[i].s);
                if (!WithinTravelCeiling(all[i].s, days)) continue;
                var one = new Reach<Settlement>
                {
                    Where = all[i].s, Price = all[i].price, Straight = all[i].days, Days = days
                };
                if (selling) kept.Add(one); else MarketRank.Keep(kept, one, false);
            }
            return Settled(kept, selling);
        }

        private static List<(Settlement, int)> Settled(List<Reach<Settlement>> kept, bool selling)
        {
            List<Reach<Settlement>> top = MarketRank.Settled(kept, selling);
            var result = new List<(Settlement, int)>(top.Count);
            for (int i = 0; i < top.Count; i++)
                result.Add((top[i].Where, top[i].Price));
            return result;
        }

        private void PrimeLiveRankings(List<ItemObject> wanted, int hour)
        {
            if (!Options.Current.Omniscient || wanted.Count == 0) return;
            List<(Settlement s, float days)> candidates = LiveCandidates(hour);
            if (candidates.Count == 0) return;
            int minStock = Options.Current.MinTownStock;
            int minWorth = Options.Current.MinTownStockWorth;
            MobileParty me = MobileParty.MainParty;
            int n = wanted.Count;
            var sells = new List<Reach<Settlement>>[n];
            var buys = new List<Reach<Settlement>>[n];
            for (int i = 0; i < n; i++)
            {
                sells[i] = new List<Reach<Settlement>>(MarketRank.TopCacheSize + 1);
                buys[i] = new List<Reach<Settlement>>(MarketRank.TopCacheSize + 1);
            }
            for (int t = 0; t < candidates.Count; t++)
            {
                Settlement town = candidates[t].s;
                float straight = candidates[t].days;
                float days = Travel.EstimateDaysFromParty(town);
                if (!WithinTravelCeiling(town, days)) continue;
                SettlementComponent market = town.SettlementComponent;
                bool tillOpen = TradeRules.WhatTheTillCanPay(market.Gold, town.IsVillage) > 0;
                Dictionary<ItemObject, int> onTheShelf = minStock > 0 ? WhatItStocks(town) : null;
                for (int i = 0; i < n; i++)
                {
                    ItemObject item = wanted[i];
                    if (tillOpen)
                    {
                        int price = Priced.At(market, item, me, true);
                        if (price > 0) sells[i].Add(new Reach<Settlement>
                        { Where = town, Price = price, Straight = straight, Days = days });
                    }
                    int stocked = 0;
                    if (onTheShelf == null ||
                        (onTheShelf.TryGetValue(item, out stocked) &&
                         TradeMath.EnoughOnTheShelf(stocked, item.Value, minStock, minWorth)))
                    {
                        int price = Priced.At(market, item, me, false);
                        if (price > 0) MarketRank.Keep(buys[i], new Reach<Settlement>
                        { Where = town, Price = price, Straight = straight, Days = days }, false);
                    }
                }
            }
            if (Travel.LostTheRoad) return;
            for (int i = 0; i < n; i++)
            {
                ItemObject item = wanted[i];
                string kind = KindOf(item);
                _marketCache[(item.StringId, true)] = (Freshness.At(hour), kind, Settled(sells[i], true));
                _marketCache[(item.StringId, false)] = (Freshness.At(hour), kind, Settled(buys[i], false));
            }
        }

        internal void PrimeMarketsFor(List<ItemObject> goods)
        {
            if (goods == null || goods.Count == 0 || !Options.Current.Omniscient) return;
            DropRankingsIfThePartyMoved();
            int hour = (int)CampaignTime.Now.ToHours;
            var cold = new List<ItemObject>();
            var asked = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < goods.Count; i++)
            {
                ItemObject item = goods[i];
                if (item == null || !asked.Add(item.StringId)) continue;
                if (Ranked(item.StringId, true, hour) && Ranked(item.StringId, false, hour)) continue;
                cold.Add(item);
            }
            PrimeLiveRankings(cold, hour);
        }

        private bool Ranked(string itemId, bool selling, int hour) =>
            _marketCache.TryGetValue((itemId, selling), out var hit) &&
            Freshness.Held(hit.stamp, hour);

        private List<(Settlement, int)> TopLive(ItemObject item, bool selling, int hour)
        {
            int minStock = Options.Current.MinTownStock;
            int minWorth = Options.Current.MinTownStockWorth;
            var all = new List<(Settlement s, int price, float days)>();
            foreach (var (s, lower) in LiveCandidates(hour))
            {
                if (selling && TradeRules.WhatTheTillCanPay(s.SettlementComponent.Gold,
                                                           s.IsVillage) <= 0) continue;
                if (!selling && !TradeMath.EnoughOnTheShelf(StockOf(s, item), item.Value,
                                                            minStock, minWorth)) continue;
                int price = Priced.At(s.SettlementComponent, item, MobileParty.MainParty, selling);
                if (price <= 0) continue;
                all.Add((s, price, lower));
            }
            return Rerank(all, selling);
        }

        public int PriceDrift(ItemObject item, Settlement town, bool selling)
        {
            if (item == null || town == null ||
                !_ledger.TryGetValue(item.StringId, out var byTown)) return 0;
            if (!byTown.TryGetValue(town.StringId, out PriceObservation seen) ||
                seen == null || !seen.SeenBefore) return 0;
            return selling ? TradeMath.Drift(seen.SellPrice, seen.WasSellPrice)
                           : TradeMath.Drift(seen.BuyPrice, seen.WasBuyPrice);
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
            if (_routes == null || !Freshness.Fresh(ref _routeStamp, hour))
            {
                _routes = ScanRoutes();
                if (Travel.LostTheRoad) _routeStamp.Stale();
                else Freshness.Taken(ref _routeStamp, hour);
            }
            return _routes.Count <= top ? _routes : _routes.GetRange(0, top);
        }

        public Dictionary<string, int> WhatTheLedgerBuysAt(Settlement here)
        {
            var asked = new Dictionary<string, int>(StringComparer.Ordinal);
            if (here == null) return asked;
            List<TradeRoute> routes = BestRoutes(int.MaxValue);
            for (int i = 0; i < routes.Count; i++)
            {
                TradeRoute route = routes[i];
                if (route.From != here || route.Item == null) continue;
                if (!asked.ContainsKey(route.Item.StringId))
                    asked[route.Item.StringId] = asked.Count + 1;
            }
            return asked;
        }

        private static int MostWorthShowing(int buyPrice)
        {
            int stocked = Options.Current.BuyCapPerItem > 0
                ? Options.Current.BuyCapPerItem : UncappedBuyProjection;
            int spendCap = Options.Current.BuyValueCapPerItem;
            return spendCap > 0 && buyPrice > 0 ? Math.Min(stocked, spendCap / buyPrice) : stocked;
        }

        private List<TradeRoute> ScanRoutes()
        {
            Bulk.Forget();
            long started = System.DateTime.UtcNow.Ticks;
            int opened = 0, thrownAway = 0;
            var routes = new List<TradeRoute>();
            ISet<string> locked = TradePolicy.LockedKeys();
            float cap = Options.Current.MaxTravelDaysTown;
            bool rankByScore = Options.Current.ConfidenceRanking;
            bool trustWhatItPaid = Options.Current.TrustWhatAMarketPaid;
            var pressure = CaravanPressure();
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

                var buys = TopBuy(item, MarketRank.TopCacheSize);
                var sells = EverySell(item);
                if (buys.Count == 0 || sells.Count == 0) continue;

                TradeRoute best = null;
                float bestKey = 0f;
                foreach (var (from, buyPrice) in buys)
                {
                    if (buyPrice <= 0) continue;

                    float toBuy = Travel.EstimateDaysFromParty(from);
                    int landedAtBuyTown = Forecast.WorthShiftAsItHasHeld(from, item, toBuy);
                    int openingBuy = Bulk.Opening(from, item, false, buyPrice, landedAtBuyTown);
                    int spendCap = Options.Current.BuyValueCapPerItem;
                    int stocked = MostWorthShowing(openingBuy);
                    int shelf = 0, onTheShelfNow = int.MaxValue;
                    if (Options.Current.Omniscient)
                    {
                        onTheShelfNow = StockOf(from, item) - (from.IsVillage ? 1 : 0);
                        shelf = TradeMath.StockAfterShift(onTheShelfNow,
                                    Forecast.UnitsLanding(from, item, toBuy),
                                    Forecast.UnitsLeaving(from, item, toBuy));
                        stocked = Math.Min(stocked, shelf);
                    }
                    if (stocked <= 0) continue;

                    foreach (var (to, sellPrice) in sells)
                    {
                        if (to == from) continue;

                        int till = 0;
                        if (Options.Current.Omniscient)
                        {
                            till = TradeRules.WhatTheTillCanPay(to.SettlementComponent?.Gold ?? 0,
                                                               to.IsVillage);
                            if (till <= 0) continue;
                        }

                        float soonest = toBuy + Travel.StraightDaysBetween(from, to);
                        if (cap > 0f && soonest > cap) continue;

                        float days = toBuy + Travel.EstimateDaysBetween(from, to);
                        if (TradeMath.OutOfReach(days)) continue;
                        if (cap > 0f && days > cap) continue;

                        int landedAtSellTown = Forecast.WorthShiftAsItHasHeld(to, item, days);
                        int openingSell = Bulk.Opening(to, item, true, sellPrice, landedAtSellTown);
                        opened++;
                        float realizable = TradePolicy.Realizable(openingSell);
                        if (!TradePolicy.BuyAcceptable(openingBuy, realizable)) { thrownAway++; continue; }

                        int qtyCap = till > 0 ? Math.Min(stocked, till / openingSell) : stocked;
                        if (qtyCap <= 0) { thrownAway++; continue; }

                        float ceiling = (float)(openingSell - openingBuy) * qtyCap;
                        if (best != null && TradeMath.PerDay(ceiling, days) <= bestKey)
                        { thrownAway++; continue; }

                        RouteQuote q = Bulk.Walk(from, to, item, qtyCap, till, spendCap,
                                                 buyPrice, sellPrice, landedAtBuyTown,
                                                 landedAtSellTown);
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
                        float runsOut = Forecast.RunsOutIn(from, item, onTheShelfNow, q.Units, toBuy);
                        float confidence = Confidence.Of(q.Simulated, flat, profit, shelf,
                                                         q.Units, days, caravans, age,
                                                         runsOut, toBuy);
                        float perDay = TradeMath.PerDay(profit, days);
                        float score = perDay * confidence;
                        if (trustWhatItPaid &&
                            PromiseScoreAt(to.StringId, out int arrivals, out float heldThere))
                            score = Confidence.AsPromisesHaveHeld(score, arrivals, heldThere);
                        float key = rankByScore ? score : perDay;
                        if (best != null && key <= bestKey) continue;

                        bestKey = key;
                        best = new TradeRoute
                        {
                            Item = item, From = from, To = to,
                            BuyPrice = q.OpeningBuyPrice, SellPrice = q.OpeningSellPrice,
                            Quantity = q.Units,
                            TravelDays = days, TotalProfit = profit, ProfitPerDay = perDay,
                            Confidence = confidence, Score = score,
                            Simulated = q.Simulated, Caravans = caravans, DataAgeDays = age,
                            StillComing = TradeMath.StillComing(q.Units, onTheShelfNow),
                            RunsOutInDays = runsOut
                        };
                    }
                }
                if (best == null) continue;
                routes.Add(best);
                Hindsight.Note(best);
            }
            routes.Sort((x, y) => rankByScore
                ? y.Score.CompareTo(x.Score)
                : y.ProfitPerDay.CompareTo(x.ProfitPerDay));
            SayWhatTheScanCost(routes.Count, opened, thrownAway,
                               System.DateTime.UtcNow.Ticks - started);
            Bulk.Forget();
            return routes;
        }

        private static void SayWhatTheScanCost(int found, int opened, int thrownAway, long ticks)
        {
            if (!Options.Current.ExtendedDebugLogging) return;
            Log.Write("route scan: " + found + " route(s) off " + opened +
                      " opening price(s), " + thrownAway + " of them thrown away by a later test, in " +
                      (ticks / 10000d).ToString("0.0",
                          System.Globalization.CultureInfo.InvariantCulture) + " ms");
        }

        private static int Pressure(Dictionary<Settlement, int> map, Settlement s) =>
            s != null && map.TryGetValue(s, out int n) ? n : 0;
    }
}
