using System;
using System.Collections.Generic;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

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

        public float RunsOutInDays = Projection.NeverRunsOut;

        public TradeLord.Confidence.Parts Parts;
    }

    public class LedgerBehavior : CampaignBehaviorBase
    {
        public static LedgerBehavior Instance { get; internal set; }

        private Dictionary<string, Dictionary<string, PriceObservation>> _ledger =
            new Dictionary<string, Dictionary<string, PriceObservation>>();
        private List<PurchaseRecord> _purchases = new List<PurchaseRecord>();
        private List<PurchaseRecord> _free = new List<PurchaseRecord>();
        private string _ledgerText = "";
        private string _purchaseText = "";
        private string _freeText = "";
        private long _nextUnitNumber = TradeMath.FirstUnitNumber;
        private int _unreadable;
        private Dictionary<string, PurchaseRecord> _paid;
        private Dictionary<string, PurchaseRecord> _freeOf;
        private long _lifetimeProfit;
        private long _lifetimeTradeXp;
        private int _lifetimeProfitCapped;
        private int _promisesScored;
        private float _promiseHeld;
        private int _forecastsJudged;
        private long _forecastWeighed;
        private long _forecastMatched;
        private int _olderForecasts;
        private int _overcountedForecasts;
        private int _unsquaredForecasts;
        private int _squaredForecasts;
        private int _cappedForecasts;
        private int _cappedPromises;
        private int _curvedForecasts;
        private int _olderPromises;
        private int _walkInsKept = -1;
        private float _keptAtWalkIns;
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
            if (held < 0f || float.IsNaN(held)) return;
            _promisesScored++;
            _promiseHeld += TradeMath.HowCloseToThePromise(held);
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
            _walkInsKept = -1;
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

        internal void KeepForecastScore(int said, int moved)
        {
            if (!TradeMath.HowMuchCameTrue(said, moved, out float share)) return;
            _forecastsJudged++;
            _forecastWeighed = TradeMath.AddedUp(_forecastWeighed, TradeMath.SizeOf(said));
            _forecastMatched = TradeMath.AddedUp(_forecastMatched, TradeMath.SizedShareThatCameTrue(said, share));
        }

        internal bool ForecastScore(out int scored, out long weighed, out float cameTrue)
        {
            scored = _forecastsJudged;
            weighed = _forecastWeighed;
            cameTrue = TradeMath.ShareBySize(_forecastWeighed, _forecastMatched);
            return scored > 0;
        }

        internal float ResaleSafety(float setting, bool learn, out int walkIns, out float held)
        {
            if (_walkInsKept < 0) TallyTheWalkIns();
            walkIns = _walkInsKept;
            held = TradeMath.MeanOf(_keptAtWalkIns, _walkInsKept);
            return TradeMath.ResaleSafetyAsPromisesHeld(setting, learn, walkIns, held);
        }

        private void TallyTheWalkIns()
        {
            int walkIns = 0;
            float kept = 0f;
            if (_promises != null)
                foreach (PromiseRecord rec in _promises.Values)
                {
                    if (rec == null || rec.Scored <= 0) continue;
                    walkIns += rec.Scored;
                    kept += rec.Held;
                }
            _walkInsKept = walkIns;
            _keptAtWalkIns = kept;
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
            CampaignEvents.ItemsLooted.AddNonSerializedListener(this, OnItemsLooted);
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (!dataStore.IsLoading) PruneExpired();
            if (dataStore.IsSaving)
                Guard.Run("Ledger.WriteForSave", () =>
                {
                    _ledgerText = LedgerCodec.WriteLedger(Listed(_ledger));
                    _purchaseText = LedgerCodec.WritePurchases(_purchases);
                    _freeText = LedgerCodec.WritePurchases(_free);
                    _promiseText = LedgerCodec.WritePromises(new List<PromiseRecord>(_promises.Values));
                    _latelyText = LedgerCodec.WriteTrades(_lately);
                    Log.Write("ledger written into the save: " + RecordedPrices() + " recorded price(s) in " +
                              _ledgerText.Length + " character(s), and " + _purchases.Count +
                              " purchase record(s) in " + _purchaseText.Length + ", and " + _free.Count +
                              " record(s) of goods that came without a purchase in " + _freeText.Length);
                });
            _lifetimeProfitCapped = Capped(_lifetimeProfit);
            dataStore.SyncData("TradeLord_LedgerText", ref _ledgerText);
            dataStore.SyncData("TradeLord_PurchaseText", ref _purchaseText);
            dataStore.SyncData("TradeLord_CameWithoutAPurchaseText", ref _freeText);
            dataStore.SyncData("TradeLord_NextUnitNumber", ref _nextUnitNumber);
            dataStore.SyncData("TradeLord_LifetimeProfit", ref _lifetimeProfitCapped);
            dataStore.SyncData("TradeLord_LifetimeProfitWide", ref _lifetimeProfit);
            dataStore.SyncData("TradeLord_LifetimeTradeXp", ref _lifetimeTradeXp);
            dataStore.SyncData("TradeLord_PromisesScoredOnTheCurve", ref _promisesScored);
            dataStore.SyncData("TradeLord_PromiseHeldOnTheCurve", ref _promiseHeld);
            dataStore.SyncData("TradeLord_PromiseTextWhenDue", ref _promiseText);
            dataStore.SyncData("TradeLord_ForecastsJudgedUpToTheForecast", ref _forecastsJudged);
            dataStore.SyncData("TradeLord_ForecastSaidUpToTheForecast", ref _forecastWeighed);
            dataStore.SyncData("TradeLord_ForecastCameTrueUpToTheForecast", ref _forecastMatched);
            dataStore.SyncData("TradeLord_LatelyText", ref _latelyText);
            dataStore.SyncData("TradeLord_VillagePursesPutBack", ref _villagePursesPutBack);
            if (dataStore.IsLoading && _lifetimeProfit == 0L) _lifetimeProfit = _lifetimeProfitCapped;
            if (dataStore.IsLoading) Guard.Run("Ledger.OlderRecords", () => ReadTheOlderRecords(dataStore));
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
            if (dataStore.IsLoading && _olderForecasts > 0)
                Log.Write("forecast check: the " + _olderForecasts + " figure(s) an older TradeLord judged at the end " +
                          "of the ride it put, rather than on the day you walked in, were set aside, so how far to " +
                          "trust what is on its way to a market is learned afresh");
            if (dataStore.IsLoading && _overcountedForecasts > 0)
                Log.Write("forecast check: the " + _overcountedForecasts + " figure(s) an older TradeLord judged while " +
                          "counting a move bigger than the forecast said as more than it said were set aside, so how " +
                          "far to trust what is on its way to a market is learned afresh");
            if (dataStore.IsLoading && _unsquaredForecasts > 0)
                Log.Write("forecast check: the " + _unsquaredForecasts + " figure(s) an older TradeLord added up as " +
                          "fractions were set aside, so how far to trust what is on its way to a market is learned " +
                          "afresh, each figure counted by its size");
            if (dataStore.IsLoading && _squaredForecasts > 0)
                Log.Write("forecast check: the " + _squaredForecasts + " figure(s) an older TradeLord weighed by the " +
                          "square of their size were set aside, so how far to trust what is on its way to a market is " +
                          "learned afresh, each figure counted by its size");
            if (dataStore.IsLoading && _olderPromises > 0)
                Log.Write("promise check: the " + _olderPromises + " price(s) checked over this campaign before a price " +
                          "above its promise counted as the promise and no more were set aside, so the campaign " +
                          "figure starts over");
            if (dataStore.IsLoading && _cappedForecasts > 0)
                Log.Write("forecast check: the " + _cappedForecasts + " figure(s) a TradeLord older than 1.101.0 judged " +
                          "were set aside, so how far to trust what is on its way to a market is learned afresh");
            if (dataStore.IsLoading && _curvedForecasts > 0)
                Log.Write("forecast check: the " + _curvedForecasts + " figure(s) an older TradeLord judged while a move " +
                          "bigger than the forecast said missed it as much as a smaller one were set aside, so how far to " +
                          "trust what is on its way to a market is learned afresh, a move bigger than it said counting as " +
                          "all of it");
            if (dataStore.IsLoading && _cappedPromises > 0)
                Log.Write("promise check: the " + _cappedPromises + " price(s) checked over this campaign while a price " +
                          "above its promise counted as the promise were set aside, so the campaign figure starts over, " +
                          "a price above the promise missing it as much as one below it");
        }

        private void ReadTheOlderRecords(IDataStore dataStore)
        {
            int everyRun = 0, onTheDay = 0, upToSaid = 0, squared = 0, whenDue = 0, bySize = 0, upToThePromise = 0,
                onTheCurve = 0;
            dataStore.SyncData("TradeLord_ForecastsScoredEveryRun", ref everyRun);
            dataStore.SyncData("TradeLord_ForecastsJudgedOnTheDay", ref onTheDay);
            dataStore.SyncData("TradeLord_ForecastsJudgedUpToWhatWasSaid", ref upToSaid);
            dataStore.SyncData("TradeLord_ForecastsJudgedByLeastSquares", ref squared);
            dataStore.SyncData("TradeLord_PromisesScoredWhenDue", ref whenDue);
            dataStore.SyncData("TradeLord_ForecastsJudgedBySize", ref bySize);
            dataStore.SyncData("TradeLord_PromisesScoredUpToThePromise", ref upToThePromise);
            dataStore.SyncData("TradeLord_ForecastsJudgedOnTheCurve", ref onTheCurve);
            _olderForecasts = everyRun > 0 ? everyRun : 0;
            _overcountedForecasts = onTheDay > 0 ? onTheDay : 0;
            _unsquaredForecasts = upToSaid > 0 ? upToSaid : 0;
            _squaredForecasts = squared > 0 ? squared : 0;
            _olderPromises = whenDue > 0 ? whenDue : 0;
            _cappedForecasts = bySize > 0 ? bySize : 0;
            _cappedPromises = upToThePromise > 0 ? upToThePromise : 0;
            _curvedForecasts = onTheCurve > 0 ? onTheCurve : 0;
        }

        private void ReadSavedText() => Guard.Run("Ledger.ReadSaved", RestoreSaved);

        private void RestoreSaved()
        {
            _ledger = KeyedByTown(LedgerCodec.ReadLedger(_ledgerText, out _unreadable));
            _purchases = LedgerCodec.ReadPurchases(_purchaseText);
            _free = LedgerCodec.ReadPurchases(_freeText);
            var every = new List<PurchaseRecord>(_purchases);
            every.AddRange(_free);
            _nextUnitNumber = TradeMath.NumberEveryUnit(every, _nextUnitNumber, out long numbered);
            if (numbered > 0L)
                Log.Write("purchase record: " + numbered + " unit(s) you hold had no number of their own, or one " +
                          "another unit already had, so each was given a new one, from #" +
                          (_nextUnitNumber - numbered) + " to #" + (_nextUnitNumber - 1) +
                          ", and no number is ever given out twice");
            long trimmed = TradeMath.KeepEveryUnitApart(_purchases) + TradeMath.KeepEveryUnitApart(_free);
            if (trimmed > 0L)
                Log.Write("purchase record: a record claimed " + trimmed + " unit(s) more than the " +
                          TradeMath.MostUnitsKeptApart + " one good can keep apart, far more than any party " +
                          "carries, so that many of its oldest units were taken off");
            int recounted = TradeMath.AddUpEveryRecord(_purchases);
            if (recounted > 0)
                Log.Write("purchase record: what you paid for " + recounted + " good(s) is now added up from what " +
                          "each unit you hold was bought for, rather than an average carried over from units " +
                          "already sold or gone");
            _promises = KeyedByTownId(LedgerCodec.ReadPromises(_promiseText, out int setAside));
            _walkInsKept = -1;
            if (setAside > 0)
                Log.Write("market records: " + setAside + " market(s) were kept by an older TradeLord, so their record " +
                          "starts over, a price above what the ledger promised counting as the promise and no more, and " +
                          "Learn the resale safety factor starts again from Resale safety factor");
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
            _walkInsKept = -1;
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
            if (carried == null || (_purchases.Count == 0 && _free.Count == 0)) return;
            Dictionary<string, (EquipmentElement el, int units)> held = HeldByKey(carried);
            var keys = new List<string>(Paid.Keys);
            foreach (string key in Free.Keys) if (!Paid.ContainsKey(key)) keys.Add(key);
            int goods = 0, units = 0;
            var dropped = new List<string>();
            foreach (string key in keys)
            {
                Paid.TryGetValue(key, out PurchaseRecord rec);
                Free.TryGetValue(key, out PurchaseRecord free);
                int recorded = (rec?.Count ?? 0) + (free?.Count ?? 0);
                held.TryGetValue(key, out var have);
                if (recorded <= have.units) continue;
                int gone = recorded - have.units;
                goods++;
                units += gone;
                dropped.Add(gone + " " + key);
                if (have.units > 0 && TradePolicy.FoodValue(have.el.Item) > 0)
                    TradeMath.DrainYourOwnFoodFirst(rec, free, gone);
                else
                    TradeMath.DrainTheOldestOf(rec, free, gone);
                if (free != null && free.Count <= 0)
                {
                    _free.Remove(free);
                    _freeOf.Remove(key);
                }
            }
            if (goods > 0)
                Log.Write("purchase record: " + units + " unit(s) of " + goods + " good(s) left the party " +
                          "without being sold, so the oldest units came off, food bought to trade last, and what was paid for them is no " +
                          "longer held against a resale: " +
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
            _freeOf = new Dictionary<string, PurchaseRecord>();
            for (int i = 0; i < _free.Count; i++)
            {
                PurchaseRecord rec = _free[i];
                if (rec?.ItemId != null) _freeOf[rec.ItemId] = rec;
            }
        }

        private Dictionary<string, PurchaseRecord> Free
        {
            get { if (_freeOf == null) Reindex(); return _freeOf; }
        }

        private static Dictionary<string, (EquipmentElement el, int units)> HeldByKey(ItemRoster carried)
        {
            var held = new Dictionary<string, (EquipmentElement el, int units)>(StringComparer.Ordinal);
            for (int i = 0; carried != null && i < carried.Count; i++)
            {
                ItemRosterElement el = carried.GetElementCopyAtIndex(i);
                string key = PaidKey(el.EquipmentElement);
                if (key == null || el.Amount <= 0) continue;
                held.TryGetValue(key, out var had);
                held[key] = (el.EquipmentElement, had.units + el.Amount);
            }
            return held;
        }

        internal void NoteWhatCameWithoutAPurchase()
        {
            ItemRoster carried = MobileParty.MainParty?.ItemRoster;
            if (carried == null) return;
            float today = (float)CampaignTime.Now.ToDays;
            var named = new List<string>();
            foreach (var kv in HeldByKey(carried))
            {
                Paid.TryGetValue(kv.Key, out PurchaseRecord bought);
                Free.TryGetValue(kv.Key, out PurchaseRecord free);
                int surplus = kv.Value.units - (bought?.Count ?? 0) - (free?.Count ?? 0);
                string said = WriteDownFree(kv.Key, kv.Value.el, surplus, TradeMath.FromElsewhere, today);
                if (said != null) named.Add(said);
            }
            if (named.Count > 0)
                Log.Write("came without a purchase: " + string.Join(", ", named.ToArray()) + ", " +
                          TradeMath.WhereFrom(TradeMath.FromElsewhere) + ", written down at no cost, so selling " +
                          "them counts no profit and no Trade XP");
        }

        private void NoteLoot(IEnumerable<ItemRosterElement> taken, string how)
        {
            float today = (float)CampaignTime.Now.ToDays;
            var named = new List<string>();
            Dictionary<string, (EquipmentElement el, int units)> held = HeldByKey(MobileParty.MainParty?.ItemRoster);
            foreach (ItemRosterElement one in taken)
            {
                string key = PaidKey(one.EquipmentElement);
                if (key == null || !held.TryGetValue(key, out var have)) continue;
                Paid.TryGetValue(key, out PurchaseRecord bought);
                Free.TryGetValue(key, out PurchaseRecord free);
                int unexplained = have.units - (bought?.Count ?? 0) - (free?.Count ?? 0);
                string said = WriteDownFree(key, one.EquipmentElement, Math.Min(one.Amount, unexplained),
                                            TradeMath.FromLoot, today);
                if (said != null) named.Add(said);
            }
            if (named.Count > 0)
                Log.Write("loot " + how + ": " + string.Join(", ", named.ToArray()) + ", " +
                          TradeMath.WhereFrom(TradeMath.FromLoot) + ", written down at no cost, so selling them " +
                          "counts no profit and no Trade XP");
        }

        internal string WriteDownTheirGear(string key, EquipmentElement el, int units) =>
            key == null ? null : WriteDownFree(key, el, units, TradeMath.FromVillagers, (float)CampaignTime.Now.ToDays);

        private string WriteDownFree(string key, EquipmentElement el, int units, int from, float today)
        {
            Free.TryGetValue(key, out PurchaseRecord free);
            int kept = free?.Count ?? 0;
            int adding = Math.Min(units, TradeMath.MostUnitsKeptApart - kept);
            if (adding <= 0 || el.Item == null) return null;
            if (free == null)
            {
                free = new PurchaseRecord { ItemId = key };
                _free.Add(free);
                _freeOf[key] = free;
            }
            long first = _nextUnitNumber;
            TradeMath.AddPurchase(free, new int[adding], first, today, from);
            _nextUnitNumber += adding;
            return adding + " " + Tongue.Named(el.Item.Name, el.Item.StringId) + " as #" + first +
                   (adding > 1 ? " to #" + (first + adding - 1) : "");
        }

        private static bool OnALootScreen() =>
            GameStateManager.Current?.ActiveState is InventoryState screen &&
            screen.InventoryMode == InventoryScreenHelper.InventoryMode.Loot;

        private void OnItemsLooted(MobileParty party, ItemRoster items)
        {
            if (party == null || party != MobileParty.MainParty || items == null) return;
            Guard.Run("Ledger.OnItemsLooted", () =>
            {
                var taken = new List<ItemRosterElement>();
                for (int i = 0; i < items.Count; i++) taken.Add(items.GetElementCopyAtIndex(i));
                NoteLoot(taken, "carried off in a raid");
            });
        }

        internal void RecordFreeSale(EquipmentElement el, int price, string how)
        {
            string key = PaidKey(el);
            if (key == null || !Free.TryGetValue(key, out PurchaseRecord free)) return;
            if (!TradeMath.DrainTheOldestUnit(free, out long number, out float day, out int from)) return;
            if (free.Count <= 0)
            {
                _free.Remove(free);
                _freeOf.Remove(key);
            }
            int waited = day > 0f ? (int)Math.Floor((float)CampaignTime.Now.ToDays - day) : -1;
            Log.Write("unit " + (number > 0L ? "#" + number + " " : "") + "of " +
                      Tongue.Named(el.Item.Name, el.Item.StringId) + ", " + TradeMath.WhereFreeFrom(from) +
                      (waited >= 0 ? " " + waited + " day(s) ago" : "") + ", sold for " + price + how +
                      ", counting no profit and no Trade XP");
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party != MobileParty.MainParty) return;
            Guard.Run("Ledger.OnSettlementEntered", () => CaptureSettlement(settlement));
        }

        private void OnDailyTick() => Guard.Run("Ledger.OnDailyTick", () =>
        {
            MatchPurchasesToWhatIsHeld();
            NoteWhatCameWithoutAPurchase();
            PruneObservations();
            SayTheResaleSafety();
        });

        private int _resaleSafetySaid = -1;

        private void SayTheResaleSafety()
        {
            if (!Options.Current.ExtendedDebugLogging) return;
            float used = TradePolicy.ResaleSafety(out float setting, out bool learn, out int walkIns, out float held);
            int percent = (int)Math.Round(used * 100f);
            if (!learn || walkIns <= 0 || percent == _resaleSafetySaid) return;
            _resaleSafetySaid = percent;
            Log.Write("resale safety: the prices at the markets you walked into have held at " + Scoring.Share(held) +
                      " of the Sell price the ledger promised, counting a price above it as the promise, over " +
                      walkIns + " walk-in(s), so a price " +
                      "elsewhere counts at " + percent + "% when TradeLord buys, where Resale safety factor " +
                      "starts it at " + (int)Math.Round(setting * 100f) + "%");
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            Guard.Run("Ledger.WatchTheParty", WatchTheParty);
            Guard.Run("Ledger.MatchPurchases", MatchPurchasesToWhatIsHeld);
            Guard.Run("Ledger.DateTheUnits", DateTheUnits);
            Guard.Run("Ledger.CameWithoutAPurchase", NoteWhatCameWithoutAPurchase);
            Guard.Run("Ledger.VillagePurses", PutBackEmptyVillagePurses);
        }

        private void DateTheUnits()
        {
            int dated = TradeMath.DateEveryUnit(_purchases, (float)CampaignTime.Now.ToDays);
            if (dated > 0)
                Log.Write("purchase record: " + dated + " unit(s) you hold were bought before TradeLord wrote down the " +
                          "day of each purchase, so their age counts from today");
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
                int xp = Counter.TradeXpEarnedOnTheScreen();
                AddTradeXp(xp);
                TradeActionBehavior.NoteScreenXp(xp, Counter.SayWhatTheScreenCredited(profit));
            });
        }

        private void OnPlayerInventoryExchange(
            List<(ItemRosterElement, int)> purchased,
            List<(ItemRosterElement, int)> sold, bool isTrading)
        {
            Guard.Run("Ledger.OnPlayerInventoryExchange", () =>
            {
                if (!isTrading && OnALootScreen())
                {
                    var taken = new List<ItemRosterElement>();
                    foreach (var one in purchased) taken.Add(one.Item1);
                    NoteLoot(taken, "taken on the loot screen");
                    return;
                }
                if (!isTrading || TradeActionBehavior.AutomatedTradeInProgress) return;
                bool laidOut = Counter.Awaiting;
                Settlement here = Settlement.CurrentSettlement;
                SettlementComponent market = here?.SettlementComponent;
                List<(EquipmentElement el, List<int> prices)> each = null;
                Guard.Run("Counter.WhatEachUnitWentFor", () => each = Counter.WhatEachUnitWentFor());
                List<List<int>> boughtAt = WhatEachUnitWentFor(each, purchased, market, selling: false);
                List<List<int>> soldAt = WhatEachUnitWentFor(each, sold, market, selling: true);
                if (laidOut)
                    Guard.Run("Counter.TookTheDeal",
                              () => TradeActionBehavior.TookTheDeal(purchased, boughtAt, sold, soldAt));
                ItemRoster carried = MobileParty.MainParty?.ItemRoster;
                var moved = new List<(ItemObject item, int intoTheMarket)>();
                for (int i = 0; i < purchased.Count; i++)
                {
                    List<int> paid = boughtAt[i];
                    if (paid == null) continue;
                    EquipmentElement el = purchased[i].Item1.EquipmentElement;
                    moved.Add((el.Item, -paid.Count));
                    int took = Math.Min(paid.Count, InAll(carried, el));
                    if (took <= 0) continue;
                    if (el.Item.HasHorseComponent)
                        TradeActionBehavior.TheVisit.NoteHandBought(el.Item.StringId, took);
                    long first = _nextUnitNumber;
                    RecordPurchase(PaidKey(el), paid.GetRange(0, took), BoughtFrom(here));
                    int laidOutToTrade = laidOut ? Math.Min(took, TradeActionBehavior.TheVisit.BoughtToTrade(true, PaidKey(el))) : 0;
                    if (laidOutToTrade > 0 && Paid.TryGetValue(PaidKey(el), out PurchaseRecord rec))
                        TradeMath.MarkBoughtToTrade(rec, first, laidOutToTrade);
                }
                for (int i = 0; i < sold.Count; i++)
                {
                    List<int> fetched = soldAt[i];
                    if (fetched == null) continue;
                    EquipmentElement el = sold[i].Item1.EquipmentElement;
                    moved.Add((el.Item, fetched.Count));
                    string key = PaidKey(el);
                    RecordHandSale(el, fetched, laidOut ? TradeActionBehavior.TheVisit.DearDrawn(true, key) : null,
                                   laidOut);
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

        internal static List<List<int>> WhatEachUnitWentFor(List<(EquipmentElement el, List<int> prices)> each,
                                                            List<(ItemRosterElement, int)> lines,
                                                            SettlementComponent market, bool selling)
        {
            var prices = new List<List<int>>(lines?.Count ?? 0);
            for (int i = 0; lines != null && i < lines.Count; i++)
            {
                var (element, said) = lines[i];
                EquipmentElement el = element.EquipmentElement;
                if (el.Item == null || said <= 0)
                {
                    prices.Add(null);
                    continue;
                }
                int found = -1;
                for (int k = 0; each != null && k < each.Count && found < 0; k++)
                {
                    List<int> read = each[k].prices;
                    if (each[k].el.Item == el.Item && each[k].el.ItemModifier == el.ItemModifier && read.Count > 0 &&
                        (element.Amount <= 0 || read.Count == element.Amount) && TradeMath.WorthOf(read) == said)
                        found = k;
                }
                if (found >= 0)
                {
                    prices.Add(each[found].prices);
                    each.RemoveAt(found);
                    continue;
                }
                int unit = market != null ? Priced.At(market, el, MobileParty.MainParty, selling) : el.Item.Value;
                int units = Deals.UnitsMoved(element.Amount, said, unit);
                prices.Add(new List<int>(TradeMath.SpreadOver(units, said)));
                Log.Write("ERROR: TradeLord could not read on the trade screen what each of the " + units + " " +
                          Tongue.Named(el.Item.Name, el.Item.StringId) + " " + (selling ? "fetched" : "cost") +
                          ", so the " + said + " gold is spread evenly over them");
            }
            return prices;
        }

        public void RecordPurchase(string itemId, IList<int> paid, int from, bool traded = false)
        {
            if (itemId == null || paid == null || paid.Count == 0) return;
            if (!Paid.TryGetValue(itemId, out var rec))
            {
                rec = new PurchaseRecord { ItemId = itemId, TotalPaid = 0, Count = 0 };
                Paid[itemId] = rec;
                _purchases.Add(rec);
            }
            TradeMath.AddPurchase(rec, paid, _nextUnitNumber, (float)CampaignTime.Now.ToDays, from, traded);
            _nextUnitNumber += paid.Count;
        }

        public long RecordSale(string itemId, int count, int unitPaid) => RecordSale(itemId, count, unitPaid, out _);

        public long RecordSale(string itemId, int count, int unitPaid, out float bought) =>
            RecordSale(itemId, count, unitPaid, out bought, out _);

        public long RecordSale(string itemId, int count, int unitPaid, out float bought, out int from)
        {
            bought = 0f;
            from = TradeMath.FromAMarket;
            if (!Paid.TryGetValue(itemId, out var rec)) return 0L;
            long number = TradeMath.NumberASaleTakes(rec, unitPaid);
            bought = TradeMath.DayASaleTakes(rec, unitPaid);
            from = TradeMath.FromASaleTakes(rec, unitPaid);
            TradeMath.DrainSale(rec, count, unitPaid);
            return number;
        }

        private static int BoughtFrom(Settlement here)
        {
            if (here != null) return TradeMath.FromAMarket;
            MobileParty met = PlayerEncounter.EncounteredMobileParty ?? MobileParty.ConversationParty;
            return met != null && met.IsVillager ? TradeMath.FromVillagers : TradeMath.FromACaravan;
        }

        internal static string TradeXpNote(int from, EquipmentElement el) =>
            TradeMath.GivesTradeXp(from) && el.ItemModifier == null
                ? ", a unit the game counts for Trade XP"
                : ", a unit the game gives no Trade XP for";

        private void RecordHandSale(EquipmentElement el, List<int> fetched, List<int> laidOut, bool staged)
        {
            string itemId = PaidKey(el);
            if (itemId == null || fetched == null || fetched.Count == 0) return;
            string how = staged ? " in the deal TradeLord laid out on the trade screen" : " by hand on the trade screen";
            float today = (float)CampaignTime.Now.ToDays;
            var lines = new List<string>();
            var unbought = new List<int>();
            if (Paid.TryGetValue(itemId, out var rec))
                foreach (TradeMath.SoldUnit one in TradeMath.DrainHandSale(rec, fetched, WhatAHandSaleCovers(rec), laidOut))
                {
                    if (!one.Bought)
                    {
                        unbought.Add(one.Price);
                        continue;
                    }
                    int waited = one.Day > 0f ? (int)Math.Floor(today - one.Day) : -1;
                    lines.Add("unit " + (one.Number > 0L ? "#" + one.Number + " " : "") + "of " +
                              Tongue.Named(el.Item.Name, el.Item.StringId) + ", " + TradeMath.WhereFrom(one.From) +
                              " for " + one.Cost + (waited >= 0 ? " " + waited + " day(s) ago" : "") +
                              ", sold for " + one.Price + how + TradeXpNote(one.From, el));
                }
            else unbought.AddRange(fetched);
            Log.WriteMany(lines);
            foreach (int price in unbought) RecordFreeSale(el, price, how);
        }

        internal int MadeOnAHandSale(EquipmentElement el, List<int> fetched, List<int> laidOut)
        {
            if (el.Item == null || fetched == null || !Paid.TryGetValue(PaidKey(el), out var rec)) return 0;
            return TradeMath.MadeOnAHandSale(rec, fetched, WhatAHandSaleCovers(rec), laidOut);
        }

        private static int WhatAHandSaleCovers(PurchaseRecord rec) =>
            Options.Current.CostBasisMode == 0
                ? TradePolicy.WhatTheAverageCovers(TradeMath.UnitBasis(rec, 0))
                : Options.Current.CostBasisMode == Options.CostOfEachUnit
                    ? TradeMath.EachUnitApart
                    : TradeMath.NoRecordedBasis;

        internal static string PaidKey(EquipmentElement el) =>
            el.Item == null ? null : LedgerCodec.PaidKey(el.Item.StringId, el.ItemModifier?.StringId);

        internal int UnitsBoughtToTrade(EquipmentElement el)
        {
            if (el.Item == null || !Paid.TryGetValue(PaidKey(el), out var rec)) return 0;
            return TradeMath.UnitsBoughtToTrade(rec);
        }

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

        public Batch[] UnitCosts(EquipmentElement el, int held) =>
            el.Item != null && Paid.TryGetValue(PaidKey(el), out var rec)
                ? TradePolicy.FoodValue(el.Item) > 0 ? TradeMath.UnitCostsOfFood(rec, held) : TradeMath.UnitCosts(rec, held)
                : null;

        internal static List<(EquipmentElement el, int amount, int traded)> FoodLines(MobileParty party)
        {
            ItemRoster roster = party?.ItemRoster;
            LedgerBehavior kept = Instance;
            if (roster == null || kept == null) return null;
            var lines = new List<(EquipmentElement el, int amount, int traded)>();
            for (int i = 0; i < roster.Count; i++)
            {
                ItemRosterElement el = roster.GetElementCopyAtIndex(i);
                if (el.Amount <= 0 || el.EquipmentElement.Item == null || !el.EquipmentElement.Item.IsFood) continue;
                lines.Add((el.EquipmentElement, el.Amount, kept.UnitsBoughtToTrade(el.EquipmentElement)));
            }
            return lines;
        }

        internal static void YourOwnFoodIsEatenFirst(MobileParty party,
                                                     List<(EquipmentElement el, int amount, int traded)> before)
        {
            ItemRoster roster = party?.ItemRoster;
            if (roster == null || before == null || before.Count == 0) return;
            var lines = new List<(int before, int after, int traded)>(before.Count);
            foreach (var one in before)
            {
                int at = roster.FindIndexOfElement(one.el);
                lines.Add((one.amount, at < 0 ? 0 : roster.GetElementNumber(at), one.traded));
            }
            List<(int back, int take)> swaps = Eating.YourOwnFirst(lines);
            if (swaps.Count == 0) return;
            var said = new List<string>();
            foreach (var (back, take) in swaps)
            {
                roster.AddToCounts(before[back].el, 1);
                roster.AddToCounts(before[take].el, -1);
                said.Add(before[take].el.Item.StringId + " in place of " + before[back].el.Item.StringId);
            }
            Log.Write("your party ate its own food before food bought to trade: " + string.Join(", ", said.ToArray()));
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
            var shortlist = new List<(Settlement town, int price, float days, float rate, int quoted, Ladder rungs)>();
            var markets = EverySell(item);
            for (int i = 0; i < markets.Count; i++)
            {
                var (town, quoted) = markets[i];
                if (town == null || quoted <= 0 || town == notHere) continue;
                float days = Travel.EstimateDaysFromParty(town);
                if (TradeMath.OutOfReach(days)) continue;
                int landed = Forecast.WorthShiftAsItHasHeld(town, item, days);
                Ladder rungs = landed == 0 ? null : Bulk.AsItLands(town, new EquipmentElement(item), true, quoted, landed);
                int price = Bulk.OpeningOn(rungs, town, quoted);
                if (price <= 0) continue;
                float rate = TradeMath.EarnedPerDay(price, paid, days);
                int at = shortlist.Count;
                while (at > 0 && rate > shortlist[at - 1].rate) at--;
                shortlist.Insert(at, (town, price, days, rate, quoted, rungs));
            }
            if (shortlist.Count == 0) return (null, 0, null);
            var flat = shortlist[0];
            if (!Options.Current.PickTheBuyerOnTheWholeStack || units <= 1 || shortlist.Count == 1)
                return (flat.town, flat.price, flat.rungs);

            long started = System.Diagnostics.Stopwatch.GetTimestamp();
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
                Fetched got = Bulk.SellWalk(one.town, item, units, one.quoted, one.rungs, paid, purse);
                BuyerWalks++;
                BuyerRungs += got.Walked;
                float rate = TradeMath.PerDay(got.Total - (long)paid * got.Units, one.days);
                if (rate <= bestRate) continue;
                bestRate = rate;
                deep = one;
                deepRungs = got.Rungs;
                deepGot = got;
            }
            BuyerTicks += System.Diagnostics.Stopwatch.GetTimestamp() - started;
            if (deep.town != flat.town && Options.Current.ExtendedDebugLogging)
                Log.Write("buyer for " + Tongue.Named(item.Name, item.StringId) + ": all " + units +
                          " unit(s) weighed picked " + deep.town.Name + ", which pays " +
                          TradeMath.PerUnit(deepGot.Total, deepGot.Units) + " a unit on average for the " +
                          deepGot.Units + " unit(s) that clear your margin and its purse there, " + deep.price +
                          " for the first, where the first unit alone would have picked " +
                          flat.town.Name + " at " + flat.price +
                          (deep.price != deep.quoted || flat.price != flat.quoted
                              ? ", counting what is on its way to each market by the time you get there"
                              : ""));
            return (deep.town, deep.price, deepRungs);
        }

        public List<(Settlement town, int price)> TopSell(ItemObject item, int n) => TakeN(TopMarkets(item, true), n);
        public List<(Settlement town, int price)> EverySell(ItemObject item) =>
            TakeN(TopMarkets(item, true), int.MaxValue);
        public List<(Settlement town, int price)> TopBuy(ItemObject item, int n) => TakeN(TopMarkets(item, false), n);
        public List<(Settlement town, int price)> EveryBuy(ItemObject item) =>
            TakeN(TopMarkets(item, false), int.MaxValue);

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

        private static bool Eligible(Settlement s)
        {
            if (!TradeActionBehavior.IsMarket(s)) return false;
            if (UnderAttack(s) || VillageShut(s)) return false;
            if (Options.Current.ExcludeHostileTowns && IsHostile(s)) return false;
            float lower = Travel.StraightDaysFromParty(s);
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
        private List<Settlement> _candidates;

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

        private List<Settlement> LiveCandidates(int hour)
        {
            if (_candidates != null && Freshness.Fresh(ref _candStamp, hour)) return _candidates;

            var list = new List<Settlement>();
            foreach (Settlement s in Settlement.All)
            {
                if (s.SettlementComponent == null) continue;
                if (!Eligible(s)) continue;
                list.Add(s);
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

        private static List<(Settlement, int)> Rerank(List<(Settlement s, int price)> all, bool selling)
        {
            var kept = new List<Reach<Settlement>>(MarketRank.TopCacheSize + 1);
            for (int i = 0; i < all.Count; i++)
            {
                float days = Travel.EstimateDaysFromParty(all[i].s);
                if (!WithinTravelCeiling(all[i].s, days)) continue;
                var one = new Reach<Settlement>
                {
                    Where = all[i].s, Price = all[i].price, Days = days
                };
                kept.Add(one);
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
            List<Settlement> candidates = LiveCandidates(hour);
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
                Settlement town = candidates[t];
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
                        { Where = town, Price = price, Days = days });
                    }
                    int stocked = 0;
                    if (onTheShelf == null ||
                        (onTheShelf.TryGetValue(item, out stocked) &&
                         TradeMath.EnoughOnTheShelf(stocked, item.Value, minStock, minWorth)))
                    {
                        int price = Priced.At(market, item, me, false);
                        if (price > 0) buys[i].Add(new Reach<Settlement>
                        { Where = town, Price = price, Days = days });
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
            var all = new List<(Settlement s, int price)>();
            foreach (Settlement s in LiveCandidates(hour))
            {
                if (selling && TradeRules.WhatTheTillCanPay(s.SettlementComponent.Gold,
                                                           s.IsVillage) <= 0) continue;
                if (!selling && !TradeMath.EnoughOnTheShelf(StockOf(s, item), item.Value,
                                                            minStock, minWorth)) continue;
                int price = Priced.At(s.SettlementComponent, item, MobileParty.MainParty, selling);
                if (price <= 0) continue;
                all.Add((s, price));
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

        internal int SeenSellPrice(EquipmentElement el, Settlement town)
        {
            if (el.Item == null || el.ItemModifier != null || town == null) return 0;
            if (!_ledger.TryGetValue(el.Item.StringId, out var byTown)) return 0;
            return byTown.TryGetValue(town.StringId, out PriceObservation seen) && seen.SellPrice > 0
                ? seen.SellPrice
                : 0;
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
            var found = new List<(Settlement s, int price)>();
            foreach (var o in byTown.Values)
            {
                Settlement town = Settlement.Find(o.TownId);
                if (town == null || !Eligible(town)) continue;
                int price = selling ? o.SellPrice : o.BuyPrice;
                if (price <= 0) continue;
                found.Add((town, price));
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
            long started = System.Diagnostics.Stopwatch.GetTimestamp();
            int opened = 0, thrownAway = 0;
            int passedOver = 0;
            bool passOver = Guard.Read("Scan.PricedAsTheGame", 0, none => Priced.PricedAsTheGameItself(), false);
            float safety = TradePolicy.ResaleSafety();
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

                var buys = EveryBuy(item);
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
                        if (passOver && TradePolicy.NoOpeningPriceCouldMakeIt(openingBuy, sellPrice, landedAtSellTown,
                                                                              stocked, days, best != null, bestKey,
                                                                              safety))
                        {
                            passedOver++;
                            continue;
                        }
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
                            Simulated = q.Simulated, Caravans = caravans,
                            Parts = Confidence.PartsOf(q.Simulated, flat, profit, shelf, q.Units, days, caravans, age,
                                                       runsOut, toBuy),
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
            SayWhatTheScanCost(routes.Count, opened, thrownAway, passedOver,
                               System.Diagnostics.Stopwatch.GetTimestamp() - started);
            Bulk.Forget();
            return routes;
        }

        private static void SayWhatTheScanCost(int found, int opened, int thrownAway, int passedOver, long ticks)
        {
            if (!Options.Current.ExtendedDebugLogging) return;
            Log.Write("route scan: " + found + " route(s) off " + opened +
                      " opening price(s), " + thrownAway + " of them thrown away by a later test, in " +
                      (ticks * 1000d / System.Diagnostics.Stopwatch.Frequency).ToString("0.0",
                          System.Globalization.CultureInfo.InvariantCulture) + " ms" +
                      (passedOver > 0
                          ? ", and " + passedOver + " pair(s) passed over unpriced, as no opening price could make them a route"
                          : ""));
        }

        private static int Pressure(Dictionary<Settlement, int> map, Settlement s) =>
            s != null && map.TryGetValue(s, out int n) ? n : 0;
    }

    [HarmonyPatch(typeof(FoodConsumptionBehavior), "MakeFoodConsumption")]
    internal static class Patch_YourOwnFoodIsEatenFirst
    {
        private static void Prefix(MobileParty __0, out List<(EquipmentElement el, int amount, int traded)> __state)
        {
            __state = null;
            if (__0 == null || __0 != MobileParty.MainParty) return;
            __state = Guard.Read("Food.BeforeEating", __0, LedgerBehavior.FoodLines, null);
        }

        private static void Postfix(MobileParty __0, List<(EquipmentElement el, int amount, int traded)> __state)
        {
            if (__state == null) return;
            Guard.Run("Food.YourOwnFirst", () => LedgerBehavior.YourOwnFoodIsEatenFirst(__0, __state));
        }
    }
}
