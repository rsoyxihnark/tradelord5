using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TradeLord
{
    internal static class Carry
    {
        private static bool _warned;

        internal static void Forget() => _warned = false;

        internal static bool Sailing()
        {
            if (!Options.Current.UseFleetCapacity) return false;
            if (Travel.NavalActive) return true;
            if (!_warned)
            {
                _warned = true;
                Log.Write("buy to fill the ships is on but this party cannot sail - its carts are counted instead");
            }
            return false;
        }

        private static float Read(MobileParty party, bool capacity)
        {
            if (party == null) return 0f;
            if (Sailing())
                try
                {
                    InventoryCapacityModel model = Campaign.Current?.Models?.InventoryCapacityModel;
                    if (model != null)
                        return capacity
                            ? model.CalculateInventoryCapacity(party, true).ResultNumber
                            : model.CalculateTotalWeightCarried(party, true).ResultNumber;
                }
                catch (Exception e) { Log.Error(e, "fleet capacity (carts counted instead)"); }
            return capacity ? party.InventoryCapacity : party.TotalWeightCarried;
        }

        internal static float Capacity(MobileParty party) => Read(party, capacity: true);

        internal static float Carried(MobileParty party) => Read(party, capacity: false);

        internal static float Room(MobileParty party) =>
            TradeMath.RoomToFill(Capacity(party), Carried(party), Options.Current.MaxCargoShare);
    }

    internal static class GameTradeBook
    {
        private static bool _read;
        private static MethodInfo _bought;
        private static MethodInfo _sold;

        internal static void Forget()
        {
            _read = false;
            _bought = null;
            _sold = null;
        }

        internal static void Note(bool selling, EquipmentElement what, int gold)
        {
            if (gold <= 0 || what.Item == null) return;
            TradeSkillCampaignBehavior book = Campaign.Current?.GetCampaignBehavior<TradeSkillCampaignBehavior>();
            if (book == null) return;
            if (!_read)
            {
                _read = true;
                _bought = typeof(TradeSkillCampaignBehavior).GetMethod(
                    "ProcessPurchases", BindingFlags.Instance | BindingFlags.NonPublic);
                _sold = typeof(TradeSkillCampaignBehavior).GetMethod(
                    "ProcessSales", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_bought == null || _sold == null)
                    Log.Write("the game's own record of what you paid for your goods could not be reached on " +
                              "this game version, so what TradeLord trades for you is left out of it and the " +
                              "Trade XP the game gives for goods you sell by hand may be off");
            }
            var one = new ItemRosterElement(what, 1);
            if (selling) _sold?.Invoke(book, new object[] { one, gold, true });
            else _bought?.Invoke(book, new object[] { one, gold });
        }
    }

    public class TradeActionBehavior : CampaignBehaviorBase
    {
        private string _pinnedTowns = "";
        private static readonly Books Visit = new Books();

        internal static Books TheVisit => Visit;
        private static bool _cargoWasFull;
        private static (float weight, int cost, float profit, bool food) _unfitted;
        private static Block? _sellStalled;
        private static Block? _buyStalled;

        internal static bool AutomatedTradeInProgress { get; private set; }

        private static int _transactionDepth;

        private const int SilencedNamed = 6;

        private static readonly Dictionary<string, int> _silenced =
            new Dictionary<string, int>(StringComparer.Ordinal);

        internal static bool InGameTransaction => _transactionDepth > 0;

        private static PartyBase _tradingWith;

        internal static PartyBase TradingWith => _tradingWith;

        private static bool _saidPricesAreNotReal;

        internal static bool PricesAreReal()
        {
            if (Patcher.Holds(nameof(Patch_TownMarketData_GetPrice))) return true;
            if (!_saidPricesAreNotReal)
            {
                _saidPricesAreNotReal = true;
                Log.Write("trading in towns and villages is off: TradeLord could not take over the price a " +
                          "trade is charged at, so it would pay you a price you could not get at the trade " +
                          "screen yourself. It trades nothing in a settlement until that is fixed. Trading " +
                          "with a party on the road is unaffected.");
            }
            return false;
        }

        private static void OpenTransaction() => _transactionDepth++;

        private static void CloseTransaction()
        {
            if (_transactionDepth > 0) _transactionDepth--;
        }

        private static bool SwapOneUnit(bool selling, Action swap, PartyBase merchant,
                                        string what, string pass, out int gold)
        {
            int before = Hero.MainHero.Gold;
            _tradingWith = merchant;
            OpenTransaction();
            try { swap(); }
            finally { CloseTransaction(); _tradingWith = null; }
            gold = selling ? Hero.MainHero.Gold - before : before - Hero.MainHero.Gold;
            if (gold >= 0) return true;
            Log.Write("ERROR: " + what + (selling ? " removed " : " added ") + (-gold) +
                      " gold - transaction direction changed on this game version. " + pass + " aborted.");
            return false;
        }

        private static void InAPass(Action work)
        {
            AutomatedTradeInProgress = true;
            try { work(); }
            finally
            {
                AutomatedTradeInProgress = false;
                _transactionDepth = 0;
                _tradingWith = null;
                ReportSilenced();
            }
        }

        internal static void ReleaseMessageFilter()
        {
            if (_transactionDepth == 0) return;
            _transactionDepth = 0;
            _tradingWith = null;
            Log.Write("ERROR: the message filter was still armed at the start of a frame - forced open. " +
                      "A transaction did not unwind; no message is suppressed beyond this frame.");
            ReportSilenced();
        }

        internal static void NoteSilenced(string said)
        {
            string line = said ?? "(a message with no words)";
            _silenced.TryGetValue(line, out int seen);
            _silenced[line] = seen + 1;
        }

        private static void ReportSilenced()
        {
            if (_silenced.Count == 0) return;
            int total = 0, named = 0;
            var lines = new List<string>();
            foreach (var kv in _silenced)
            {
                total += kv.Value;
                if (named++ < SilencedNamed)
                    lines.Add("    " + kv.Value + " x " + kv.Key);
            }
            lines.Insert(0, "  silenced " + total + " message(s) raised inside the game's own " +
                            "transaction, " + _silenced.Count + " of them different:");
            if (_silenced.Count > SilencedNamed)
                lines.Add("    and " + (_silenced.Count - SilencedNamed) + " more not named here");
            _silenced.Clear();
            Log.WriteMany(lines);
        }

        internal static void ForgetVisit()
        {
            ResetVisit();
            _transactionDepth = 0;
            _tradingWith = null;
            _silenced.Clear();
            Notices.Forget();
            _pendingXp = 0;
            _pendingProfit = 0;
            _pendingXpMuted = true;
            AutomatedTradeInProgress = false;
            _sittingAt = null;
            _sittingHours = -1d;
            ForgetArrivals();
            Marker.Forget();
            Shops.ForgetWhoIsBuying();
            Drove.Forget();
            Carry.Forget();
            TradePolicy.ForgetItemListAudit();
            TradePolicy.ForgetCraftingLookup();
            Errands.Forget();
            Priced.Forget();
            GameTradeBook.Forget();
            ForgetRoadMarket();
            ForgetTheMeeting();
            Meetings.ForgetWhoYouTradedWith();
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (!dataStore.IsLoading)
                Guard.Run("Visit.PinsForSave", () => _pinnedTowns = LedgerPanel.PinnedIds());
            Settlement tracked = Marker.Tracked;
            dataStore.SyncData("TradeLord_TrackedTown", ref tracked);
            Marker.Tracked = tracked;
            dataStore.SyncData("TradeLord_PanelPins", ref _pinnedTowns);
            if (_pinnedTowns == null) _pinnedTowns = "";
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
            CampaignEvents.ConversationEnded.AddNonSerializedListener(this, OnConversationEnded);
        }

        private void OnConversationEnded(IEnumerable<CharacterObject> spoke) => Meetings.ConversationEnded();

        private void OnTick(float dt)
        {
            MobileParty party = MobileParty.MainParty;
            if (party == null || party.CurrentSettlement != null) return;
            NoteTheRoadTaken(party);
            if (!Marker.DueAgain(party.GetPosition2D)) return;
            Guard.Run("Action.MarkerAsYouMove", Marker.Update);
        }

        private void OnDailyTick()
        {
            Guard.Run("Action.DailyHerdCheck", () =>
            {
                int shed = Drove.AnimalsToShed(MobileParty.MainParty);
                Settlement here = MobileParty.MainParty?.CurrentSettlement;
                if (shed > 0 && here == null) Drove.LogState("on the road, no market in reach", shed);
                else if (shed > 0)
                    Drove.LogState("at " + here.Name + (HasAMarket(here) ? "" : ", which has no market"), shed);
            });
            Guard.Run("Action.DailyTick", Marker.Update);
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (party != MobileParty.MainParty) return;
            NoteTheGateBehind(party);
            NoteLeavingTheSitting(settlement);
            Marker.ForgetWhatYouCarry();
            Guard.Run("Action.HerdReliefOnLeaving", () =>
            {
                if (HasAMarket(settlement)) Drove.LogState("leaving " + settlement.Name);
                if (!_visitTradeAllowed)
                {
                    if (IsMarket(settlement))
                        Log.Write("herd relief on the way out is skipped: the game would not let you trade at " +
                                  settlement.Name + " when you arrived");
                    return;
                }
                if (Options.Current.AutoSellOnEntry && Counter.HoldsBack())
                {
                    Log.Write("herd relief on the way out of " + settlement.Name + " is held back: Staged Trading " +
                              "lays the deal out on the trade screen from the menu entry instead, so nothing moved");
                    return;
                }
                if (Options.Current.AutoSellOnEntry && Arrivals.FirstTimeBack(settlement.StringId, _heldBackAt))
                {
                    Log.Write("herd relief on the way out of " + settlement.Name + " is held back: this is your " +
                              "first time back since TradeLord made its last trade here, so only Trade here now " +
                              "(TradeLord) trades here this visit");
                    return;
                }
                if (Options.Current.AutoSellOnEntry) ExecuteHerdRelief(settlement, quiet: true);
            });
            Guard.Run("Action.NoteWhereItTraded", () => NoteWhereItTraded(settlement));
            Guard.Run("Action.OnSettlementLeft", Marker.Update);
        }

        private static void NoteWhereItTraded(Settlement settlement)
        {
            bool traded = Visit.Moves(Simulating) > _movesAtArrival;
            _lastTradedAt = Arrivals.LastTradedAt(settlement?.StringId, traded, _lastTradedAt);
            if (traded)
                Log.Write("TradeLord traded at " + settlement?.Name + " this visit, so " +
                          (ArrivalTrades()
                              ? "trading on arrival leaves it alone the first time you come back and the map " +
                                "marker leaves it out until then"
                              : "the map marker leaves it out until you come back to it") +
                          ", unless TradeLord trades somewhere else first");
        }

        private static bool ArrivalTrades() =>
            (Options.Current.AutoSellOnEntry || Options.Current.AutoBuyOnEntry) && !Counter.HoldsBack();

        private static bool _visitTradeAllowed;

        private static string _sittingAt;
        private static double _sittingHours = -1d;

        private static string _lastArrivalAt;
        private static string _lastTradedAt;
        private static string _heldBackAt;
        private static int _movesAtArrival;
        private static Vec2 _gateBehind;
        private static bool _gateBehindKnown;
        private static bool _tookToTheRoad;
        internal static bool StillTheSameArrival(Settlement settlement) =>
            Arrivals.StillTheSame(settlement?.StringId, _lastArrivalAt, _tookToTheRoad);

        internal static bool MarkerLeavesItOut(Settlement settlement) =>
            settlement != null && Arrivals.FirstTimeBack(settlement.StringId, LastTradedAtOnceYouLeave());

        private static string LastTradedAtOnceYouLeave() =>
            Arrivals.LastTradedAt(MobileParty.MainParty?.CurrentSettlement?.StringId,
                                  Visit.Moves(Simulating) > _movesAtArrival, _lastTradedAt);

        private static void NoteARoadTrade(MobileParty met, Books books, int movesBefore)
        {
            string was = _lastTradedAt;
            _lastTradedAt = Arrivals.AfterTheRoad(books.Moves(Simulating) > movesBefore, was);
            if (was != null && _lastTradedAt == null)
                Log.Write("TradeLord traded with " + met.Name + " on the road, so " +
                          (ArrivalTrades()
                              ? "trading on arrival no longer leaves the market it last traded at alone and the " +
                                "map marker no longer leaves it out"
                              : "the map marker no longer leaves out the market it last traded at"));
        }

        private static void NoteThisArrival(Settlement settlement)
        {
            _lastArrivalAt = settlement?.StringId;
            _heldBackAt = null;
            _gateBehindKnown = false;
            _tookToTheRoad = false;
        }

        private static void NoteTheGateBehind(MobileParty party)
        {
            _gateBehind = party.GetPosition2D;
            _gateBehindKnown = true;
        }

        private static void NoteTheRoadTaken(MobileParty party)
        {
            _tookToTheRoad = Arrivals.TakenToTheRoad(
                _tookToTheRoad, _gateBehindKnown,
                party.GetPosition2D.DistanceSquared(_gateBehind));
        }

        private static void ForgetArrivals()
        {
            _lastArrivalAt = null;
            _lastTradedAt = null;
            _heldBackAt = null;
            _movesAtArrival = Visit.Moves(Simulating);
            _gateBehindKnown = false;
            _tookToTheRoad = false;
        }

        private static bool StillTheSameSitting(Settlement settlement)
        {
            double hours = CampaignTime.Now.ToHours;
            bool same = Arrivals.StillTheSameSitting(settlement?.StringId, _sittingAt,
                                                     hours, _sittingHours);
            _sittingAt = settlement?.StringId;
            _sittingHours = hours;
            return same;
        }

        private static void NoteLeavingTheSitting(Settlement settlement)
        {
            if (settlement != null && settlement.StringId == _sittingAt)
                _sittingHours = CampaignTime.Now.ToHours;
        }

        internal static void StartAFreshDryRun() => Visit.ForgetTheDryRun();

        private static void ResetVisit(bool sameSitting = false)
        {
            _visitTradeAllowed = false;
            Marker.ForgetWhatYouCarry();
            if (sameSitting) Visit.ForgetTheDryRun(); else Visit.Forget();
            _cargoWasFull = false;
            _unfitted = default;
            _sellStalled = null;
            _buyStalled = null;
        }

        private static bool NoRoomToCarry()
        {
            MobileParty party = MobileParty.MainParty;
            return party != null && Carry.Room(party) < 1f;
        }

        private static bool Simulating => Options.Current.SimulationMode;

        private static bool TradedThisVisit() => Visit.Traded(Simulating);

        private static TextObject PassMessage(bool sim, string simSaid, string realSaid,
                                              Dictionary<ItemObject, (int count, int gold)> detail,
                                              int items, int gold)
        {
            TextObject said = Tongue.Text(sim ? simSaid : realSaid);
            said.SetTextVariable("ITEMS", ItemSummary(detail, items));
            said.SetTextVariable("GOLD", gold);
            return said;
        }

        private static bool Muted(bool automated) => automated && Options.Current.QuietAutomation;

        private sealed class Pass
        {
            internal readonly Settlement Site;
            internal readonly SettlementComponent Market;
            internal readonly PartyBase Shop;
            internal readonly PartyBase Me;
            internal readonly MobileParty Party;
            internal readonly MobileParty Met;
            internal readonly IMarketData Road;
            internal readonly Books Books;
            internal readonly bool Sim;
            internal readonly bool Quiet;
            internal readonly Dictionary<ItemObject, (int count, int gold)> Detail =
                new Dictionary<ItemObject, (int count, int gold)>();
            internal readonly Dictionary<ItemObject, (int count, int gold)> Quoted =
                new Dictionary<ItemObject, (int count, int gold)>();
            internal readonly Dictionary<ItemObject, (string where, int price)> Aimed =
                new Dictionary<ItemObject, (string where, int price)>();

            internal bool DirectionError;

            internal bool OnTheScreen;

            private ISet<string> _locked;
            private bool _lockedRead;
            private float _capacity = -1f;
            private float _eachHaul = -1f;
            private float _eachMount = -1f;
            private float _carried = -1f;
            private int _carriedAt = -1;
            private int _goldBefore;
            private ItemRosterElement _unit;
            private int _unitPrice;
            private Action _sellUnit;
            private Action _buyUnit;

            private Pass(Settlement site, MobileParty met, IMarketData road, Books books,
                         MobileParty party, bool quiet)
            {
                Site = site;
                Met = met;
                Road = road;
                Market = site?.SettlementComponent;
                Shop = site != null ? site.Party : met.Party;
                Books = books;
                books.LaidOut = Counter.Staging;
                Party = party;
                Me = party.Party;
                Sim = Options.Current.SimulationMode || Counter.Staging;
                Quiet = quiet;
            }

            internal static Pass Open(Settlement site, bool quiet)
            {
                if (!TradeActionBehavior.PricesAreReal()) return null;
                if (!MarketOpen(site, TradeActionBehavior.Muted(quiet))) return null;
                MobileParty party = MobileParty.MainParty;
                return party == null ? null : new Pass(site, null, null, Visit, party, quiet);
            }

            internal static Pass Meet(MobileParty met, IMarketData road, Books books, MobileParty party) =>
                new Pass(null, met, road, books, party, quiet: true);

            internal ISet<string> Locked
            {
                get
                {
                    if (_lockedRead) return _locked;
                    _lockedRead = true;
                    _locked = TradePolicy.LockedKeys();
                    return _locked;
                }
            }

            internal bool Muted => Counter.Staging || TradeActionBehavior.Muted(Quiet);

            internal bool Reports => Site != null;

            internal string Key => Site != null ? Site.StringId : Met.StringId;

            internal string Where => Site != null ? "at " + Site.Name : "from " + Met.Name;

            internal string Place => Site != null
                ? Tongue.Named(Site.Name, Site.StringId)
                : Tongue.Named(Met.Name, Met.StringId);

            internal string Headed(string label) => label + (Sim ? Counter.Heading : ": ");

            internal ItemRoster Stock => Site != null ? Site.ItemRoster : Met.ItemRoster;

            internal int YoursToSell(ItemRosterElement el)
            {
                ItemObject item = el.EquipmentElement.Item;
                return Math.Min(el.Amount,
                                LedgerBehavior.InAll(Party.ItemRoster, item) + Books.Held(Sim, item.StringId));
            }

            internal int TheirsToSell(ItemRosterElement el)
            {
                ItemObject item = el.EquipmentElement.Item;
                return Math.Min(el.Amount,
                                LedgerBehavior.InAll(Stock, item) - Books.Stocked(Sim, item.StringId));
            }

            internal int TillNow => Site != null ? Market.Gold : Met.PartyTradeGold;

            internal int Till => TillNow - Books.TillDrawn(Sim);

            internal int Spendable() => TradeActionBehavior.Spendable(Books, Sim);

            internal bool WouldReachYourReserve(int price) => price >= Spendable();

            internal float Capacity =>
                (_capacity < 0f ? _capacity = Carry.Capacity(Party) : _capacity) + Books.CapacityAdded(Sim) -
                CapacitySoldOnPaper();

            private float CapacitySoldOnPaper()
            {
                int hauls = Books.HaulsShed(Sim), mounts = Books.MountsShed(Sim);
                float sold = 0f;
                if (hauls > 0)
                {
                    if (_eachHaul < 0f) _eachHaul = Drove.CargoAHaulAnimalAdds(Party);
                    sold += hauls * _eachHaul;
                }
                if (mounts > 0)
                {
                    if (_eachMount < 0f) _eachMount = Drove.CargoASpareMountAdds(Party);
                    sold += mounts * _eachMount;
                }
                return sold;
            }

            internal float Carried()
            {
                int version = Party.ItemRoster.VersionNo;
                if (_carried >= 0f && version == _carriedAt) return _carried;
                _carriedAt = version;
                return _carried = Carry.Carried(Party);
            }

            internal float Room() =>
                TradeMath.RoomToFill(Capacity, Carried(), Options.Current.MaxCargoShare);

            internal float ShareCap =>
                Options.Current.MaxHeldShare > 0f ? Capacity * Options.Current.MaxHeldShare : 0f;

            internal void CountFrom() => _goldBefore = Hero.MainHero.Gold;

            internal int Gained(int simGold) => GoldGained(Sim, simGold, _goldBefore);

            internal int Spent(int simSpent) => GoldSpent(Sim, simSpent, _goldBefore);

            internal int Price(EquipmentElement what, bool selling) =>
                Site != null ? Priced.At(Market, what, Party, selling)
                             : Road.GetPrice(what, Party, selling, Shop);

            internal Func<int, int> PricesAhead(EquipmentElement what)
            {
                int now = Price(what, selling: false);
                if (Site == null || now <= 0) return taken => now;
                var rungs = new Ladder(Site, what, false, now, 0);
                int first = rungs.At(0);
                return taken =>
                {
                    int at = rungs.At(taken);
                    return at <= 0 || first <= 0 ? at : now + at - first;
                };
            }

            internal void Tally(ItemObject item, int count, int gold) =>
                TradeActionBehavior.Tally(Detail, item, count, gold);

            internal void Quote(ItemObject item, int count, int gold)
            {
                if (Options.Current.ExtendedDebugLogging) TradeActionBehavior.Tally(Quoted, item, count, gold);
            }

            internal bool SellOne(ItemRosterElement el, int price, string what, string named, out int gold)
            {
                _unit = el;
                _unitPrice = price;
                if (_sellUnit == null)
                    _sellUnit = Site != null
                        ? (Action)(() => SellItemsAction.Apply(Me, Shop, _unit, 1, Site))
                        : () => HandOver(Me, Shop, _unit.EquipmentElement, _unitPrice);
                return Swap(true, _sellUnit, what, named, out gold);
            }

            internal bool BuyOne(ItemRosterElement el, int price, string what, string named, out int gold)
            {
                _unit = el;
                _unitPrice = price;
                if (_buyUnit == null)
                    _buyUnit = Site != null
                        ? (Action)(() => SellItemsAction.Apply(Shop, Me, _unit, 1, Site))
                        : () => TakeDelivery(Shop, Me, _unit.EquipmentElement, _unitPrice);
                return Swap(false, _buyUnit, what, named, out gold);
            }

            private bool Swap(bool selling, Action swap, string what, string named, out int gold)
            {
                if (SwapOneUnit(selling, swap, Site == null ? null : Shop, what, named, out gold))
                {
                    int paid = gold;
                    Guard.Run("GameTradeBook", () => GameTradeBook.Note(selling, _unit.EquipmentElement, paid));
                    return true;
                }
                DirectionError = true;
                return false;
            }

            private ISet<string> KindsMoved()
            {
                var moved = new HashSet<string>(StringComparer.Ordinal);
                foreach (var kv in Detail)
                {
                    string kind = LedgerBehavior.KindOf(kv.Key);
                    if (kind == null) return null;
                    moved.Add(kind);
                }
                return moved.Count > 0 ? moved : null;
            }

            internal void Moved(int? profit = null, int gold = 0, bool selling = true)
            {
                if (Sim) return;
                if (profit.HasValue) LedgerBehavior.Instance?.AddProfit(profit.Value);
                Guard.Run("Pass.NoteTrade", () => NoteTrade(selling, gold));
                CoinSound();
                if (Site == null) return;
                if (!OnTheScreen) Hindsight.YouTraded(Site, WhatMoved(selling));
                LedgerBehavior.Instance?.CaptureSettlement(Site, force: true, KindsMoved());
                Guard.Run("Pass.PinCleared", () => LedgerPanel.Unpin(Site));
            }

            private List<(ItemObject item, int intoTheMarket)> WhatMoved(bool selling)
            {
                var moved = new List<(ItemObject item, int intoTheMarket)>();
                foreach (var kv in Detail)
                    if (kv.Key != null && kv.Value.count > 0)
                        moved.Add((kv.Key, selling ? kv.Value.count : -kv.Value.count));
                return moved;
            }

            private void NoteTrade(bool selling, int gold)
            {
                if (gold <= 0 || Detail.Count == 0) return;
                int units = 0;
                foreach (var kv in Detail) units += kv.Value.count;
                LedgerBehavior.Instance?.NoteTrade(Place, ItemSummary(Detail, units),
                                                   selling ? gold : -gold, (float)CampaignTime.Now.ToDays);
            }

            internal void Capture()
            {
                if (Site != null) LedgerBehavior.Instance?.CaptureSettlement(Site);
            }

            internal TextObject Said(string simSaid, string realSaid, int items, int gold) =>
                PassMessage(Sim, simSaid, realSaid, Detail, items, gold);

            internal void Logged(bool selling, string why) =>
                LogDetail(selling, Sim, Detail, Quoted, Aimed, why);
        }

        private static void WarnUnmatchedItemLists()
        {
            TradePolicy.ItemListsNameTwoAnimals();
            if (!TradePolicy.ItemListsNameNothing()) return;
            Notices.Say(Tongue.Text("{=TL91}An entry on one of your TradeLord item lists matches no good in this game and is doing nothing. TradeLord.log names which."), Notices.Alert);
        }

        internal static int GoldHeldBack()
        {
            int wage = 0;
            try { wage = MobileParty.MainParty?.TotalWage ?? 0; }
            catch (Exception e) { Log.Error(e, "wage cover (the flat reserve still holds)"); }
            return TradeMath.Reserve(Options.Current.GoldReserve, Options.Current.KeepWageDays, wage);
        }

        internal static int Spendable(Books books, bool sim)
        {
            int purse = Hero.MainHero.Gold + books.Purse(sim);
            int held = GoldHeldBack();
            int paid = books.PaidOut(sim);
            return TradeMath.Budget(purse, held, SpendCap((long)purse + paid), paid);
        }

        internal static int SpendCap(long purseBeforeBuying) =>
            TradeMath.AdaptiveSpendCap(Options.Current.MaxSpendPerVisit, purseBeforeBuying,
                                       Options.Current.AdaptiveSpendLimit);

        internal static int PurseForTheirOffer(Books books, bool sim) =>
            TradeMath.Budget(Hero.MainHero.Gold + books.Purse(sim), GoldHeldBack(), 0, 0);

        internal static int PurseForAVisit()
        {
            int purse = Hero.MainHero.Gold;
            int held = GoldHeldBack();
            return TradeMath.Budget(purse, held, SpendCap(purse), 0);
        }

        private static int GoldGained(bool sim, int simGold, int goldBefore) =>
            sim ? simGold : Hero.MainHero.Gold - goldBefore;

        private static int GoldSpent(bool sim, int simSpent, int goldBefore) =>
            sim ? simSpent : goldBefore - Hero.MainHero.Gold;

        private static void SayWhatHoldsYourPurse(Pass pass)
        {
            int purse = Hero.MainHero.Gold + pass.Books.Purse(pass.Sim);
            int held = GoldHeldBack(), flat = Options.Current.GoldReserve;
            int cap = SpendCap((long)purse + pass.Books.PaidOut(pass.Sim));
            int set = Options.Current.MaxSpendPerVisit;
            Log.Repeatable("purse " + pass.Key, purse + "/" + held + "/" + pass.Spendable(),
                           "nothing is bought " + pass.Where + ": your purse is " + purse +
                           " and TradeLord holds " + held + " of it back, " + flat +
                           " as your gold reserve and " + (held - flat) + " as " +
                           Options.Current.KeepWageDays + " day(s) of your wage bill" +
                           (cap > 0 ? ", with " + (cap - pass.Books.PaidOut(pass.Sim)) +
                                      " left of the " + cap + " you allow per visit" +
                                      (cap > set ? " (Max spend per visit " + set + ", raised by Adaptive spend " +
                                                   "limit as your purse grew)" : "")
                                    : ""));
        }

        private static bool WarnPurseBelowReserve()
        {
            int held = GoldHeldBack(), flat = Options.Current.GoldReserve;
            if (Hero.MainHero.Gold + Visit.Purse(Simulating) - held > 0) return false;
            TextObject msg = Tongue.Text(held > flat
                ? "{=TL392}Your purse is at {GOLD} denars and TradeLord holds {RESERVE} of it back, {FLAT} for Gold reserve and {WAGES} for Keep gold for days of wages, so it will not buy anything here. Sell some cargo, or lower either of those in its settings."
                : "{=TL92}Your purse is at {GOLD} denars and your gold reserve is {RESERVE}, so TradeLord will not buy anything here. Sell some cargo, or lower Gold reserve in its settings.");
            msg.SetTextVariable("GOLD", Hero.MainHero.Gold);
            msg.SetTextVariable("RESERVE", held);
            msg.SetTextVariable("FLAT", flat);
            msg.SetTextVariable("WAGES", held - flat);
            Notices.Say(msg, Notices.Alert);
            return true;
        }

        private static void SayWhatPickingABuyerCost()
        {
            if (!Options.Current.ExtendedDebugLogging || LedgerBehavior.BuyerWalks <= 0) return;
            Log.Write("  picking a buyer walked " + LedgerBehavior.BuyerWalks + " price ladder(s), " +
                      LedgerBehavior.BuyerRungs + " rung(s) in all, in " +
                      (LedgerBehavior.BuyerTicks / 10000d).ToString("0.0",
                          System.Globalization.CultureInfo.InvariantCulture) + " ms");
            LedgerBehavior.ForgetWhatPickingABuyerCost();
        }

        private static void WarnNoRoomToCarry()
        {
            if (TradedThisVisit()) return;
            if (!NoRoomToCarry() && !_cargoWasFull) return;
            Notices.Say(Tongue.Text("{=TL82}Cargo is full. Recruit more men, buy more horses, or sell goods manually."),
                  Notices.Alert);
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            Guard.Run("Action.OnSessionLaunched", () =>
            {
                ResetVisit();
                Marker.ForgetTheRead();
                Settlement inside = MobileParty.MainParty?.CurrentSettlement;
                if (inside != null) _visitTradeAllowed = CanTradeHere(inside);
                Guard.Run("Action.RestorePins", () => LedgerPanel.RestorePins(_pinnedTowns));
                Guard.Run("Action.RestoreMarker", Marker.Update);
                Log.Write(Travel.NavalActive
                    ? "naval capability: party can sail - routes and travel times include sea legs"
                    : "naval capability: land-only - land routing in effect");
                SelfCheck.Say();

                void AddOptions(string menu) => Guard.Run(
                    "menu " + menu + " (the other menus are unaffected)", () =>
                {
                    starter.AddGameMenuOption(menu, "tradelord_quicktrade",
                        Tongue.Text("{=TL26}Trade here now (TradeLord)").ToString(),
                        args =>
                        {
                            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                            args.Text = Tongue.Text("{=TL26}Trade here now (TradeLord)");
                            return (Options.Current.QuickSellMenu || Counter.HoldsBack()) && CanTradeHere(Settlement.CurrentSettlement);
                        },
                        args => Guard.Run("Action.QuickTradeMenu", () =>
                        {
                            if (!Counter.Ready(Settlement.CurrentSettlement)) return;
                            try
                            {
                                Drove.LogState("trading by hand at " + Settlement.CurrentSettlement.Name);
                                ExecuteQuickSell(Settlement.CurrentSettlement);
                                ExecuteHerdRelief(Settlement.CurrentSettlement);
                                ExecuteQuickBuy(Settlement.CurrentSettlement);
                                if (ExecuteHaulage(Settlement.CurrentSettlement))
                                    ExecuteQuickBuy(Settlement.CurrentSettlement);
                                ExecuteResupply(Settlement.CurrentSettlement);
                                if (ExecuteHaulage(Settlement.CurrentSettlement))
                                    ExecuteResupply(Settlement.CurrentSettlement);
                                ExecuteHerdRelief(Settlement.CurrentSettlement);
                                Drove.LogState("after trading by hand at " + Settlement.CurrentSettlement.Name);
                                ReportStalledPasses();
                            }
                            finally
                            {
                                TextObject laid = Counter.Settle();
                                if (laid != null) Notices.Say(laid, Notices.Note);
                                Guard.Run("Action.MarkerAfterTradingByHand", Marker.Update);
                            }
                        }),
                        false, 6);

                    starter.AddGameMenuOption(menu, "tradelord_report",
                        Tongue.Text("{=TL11}Consult the TradeLord ledger").ToString(),
                        args =>
                        {
                            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                            args.Text = Tongue.Text("{=TL11}Consult the TradeLord ledger");
                            return Options.Current.LedgerMenuEntry;
                        },
                        args => Guard.Run("Action.LedgerReport", () => ShowLedgerReport()),
                        false, 7);
                });

                AddOptions("town");
                AddOptions("village");

                if (NavalModulePresent())
                    foreach (string port in new[] { "port_menu", "naval_storyline_virtualport" })
                        AddOptions(port);

                Meetings.Lines(starter);
            });
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party != MobileParty.MainParty) return;
            Guard.Run("Action.OnSettlementEntered", () =>
            {
                Counter.Forget();
                PriceTrace.Say(settlement, "walked in, before anything was traded");
                Hindsight.Score(settlement);
                ResetVisit(StillTheSameSitting(settlement));
                _movesAtArrival = Visit.Moves(Simulating);
                _visitTradeAllowed = CanTradeHere(settlement);
                WarnUnmatchedItemLists();
                if (HasAMarket(settlement)) Drove.LogState("entering " + settlement.Name);

                if (StillTheSameArrival(settlement))
                {
                    if (HasAMarket(settlement))
                        Log.Write("trading on arrival at " + settlement.Name + " is left alone: your party has " +
                                  "not taken to the road since it last came in here, so this is the same arrival");
                    Marker.Update();
                    return;
                }
                NoteThisArrival(settlement);

                if (_visitTradeAllowed && Counter.HoldsBack())
                {
                    bool back = Arrivals.FirstTimeBack(settlement.StringId, _lastTradedAt);
                    if (back) _lastTradedAt = null;
                    Notices.Say(TheDealWaitsForYou(), Notices.Note);
                    Log.Write("trading on arrival at " + settlement.Name + " is held back: the deal is laid out " +
                              "on the trade screen from the menu entry instead, so nothing moved" +
                              (back
                                  ? ", and this is your first time back since TradeLord last traded here, so the " +
                                    "map marker no longer leaves it out"
                                  : ""));
                    Marker.Update();
                    return;
                }

                if (_visitTradeAllowed &&
                    (Options.Current.AutoSellOnEntry || Options.Current.AutoBuyOnEntry) &&
                    Arrivals.FirstTimeBack(settlement.StringId, _lastTradedAt))
                {
                    _lastTradedAt = null;
                    _heldBackAt = settlement.StringId;
                    if (Options.Current.QuickSellMenu && !Muted(true)) Notices.Say(FirstTimeBackNote(), Notices.Note);
                    Log.Write("trading on arrival at " + settlement.Name + " is left alone: TradeLord made its last " +
                              "trade here and this is your first time back, so only Trade here now (TradeLord) " +
                              "trades here this visit, and the map marker no longer leaves it out");
                    Marker.Update();
                    return;
                }

                if (_visitTradeAllowed && Arrivals.FirstTimeBack(settlement.StringId, _lastTradedAt))
                {
                    _lastTradedAt = null;
                    Log.Write("this is your first time back at " + settlement.Name + " since TradeLord last traded " +
                              "here, so the map marker no longer leaves it out");
                }

                if (Options.Current.AutoSellOnEntry) ExecuteQuickSell(settlement, quiet: true);
                if (Options.Current.AutoSellOnEntry) ExecuteHerdRelief(settlement, quiet: true);
                if (Options.Current.AutoBuyOnEntry) ExecuteQuickBuy(settlement, quiet: true);
                if (Options.Current.AutoBuyOnEntry && ExecuteHaulage(settlement, quiet: true))
                    ExecuteQuickBuy(settlement, quiet: true);
                if (Options.Current.AutoBuyOnEntry) ExecuteResupply(settlement, quiet: true);
                if (Options.Current.AutoBuyOnEntry && ExecuteHaulage(settlement, quiet: true))
                    ExecuteResupply(settlement, quiet: true);
                if (Options.Current.AutoSellOnEntry) ExecuteHerdRelief(settlement, quiet: true);
                if (HasAMarket(settlement)) Drove.LogState("after trading at " + settlement.Name);
                ReportStalledPasses();
                if (CanTradeHere(settlement) &&
                    (Options.Current.AutoBuyOnEntry || Options.Current.QuickSellMenu))
                {
                    if (!WarnPurseBelowReserve()) WarnNoRoomToCarry();
                }
                Marker.Update();
            });
        }

        private static bool NavalModulePresent()
        {
            try { return TaleWorlds.ModuleManager.ModuleHelper.GetModuleInfo("NavalDLC") != null; }
            catch { return false; }
        }

        internal static bool HasAMarket(Settlement s) => s != null && (s.IsTown || s.IsVillage);

        internal static bool IsMarket(Settlement s) =>
            s != null && ((s.IsTown && Options.Current.TradeWithTowns) ||
                          (s.IsVillage && Options.Current.TradeWithVillages));

        private static bool GameAllowsTrade(Settlement s)
        {
            if (s != Settlement.CurrentSettlement) return true;
            try
            {
                var model = Campaign.Current?.Models?.SettlementAccessModel;
                return model == null || model.CanMainHeroDoSettlementAction(
                    s, SettlementAccessModel.SettlementAction.Trade, out _, out _);
            }
            catch (Exception e)
            {
                Log.Error(e, "asking the game whether you may trade here (TradeLord trades nothing here " +
                             "until the game can answer)");
                return false;
            }
        }

        private static bool CanTradeHere(Settlement s) =>
            IsMarket(s) && !LedgerBehavior.VillageShut(s) && GameAllowsTrade(s) &&
            !(Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(s));

        private static TextObject FirstTimeBackNote()
        {
            TextObject line = Tongue.Text("{=TL475}TradeLord made its last trade here, so it leaves this market alone the first time you come back. Choose {ENTRY} in this menu to trade here now.");
            line.SetTextVariable("ENTRY", Tongue.Text("{=TL26}Trade here now (TradeLord)"));
            return line;
        }

        private static TextObject TheDealWaitsForYou()
        {
            TextObject line = Tongue.Text("{=TL403}TradeLord is holding its trade back until you have seen it. Choose {ENTRY} in this menu to have it laid out on the trade screen.");
            line.SetTextVariable("ENTRY", Tongue.Text("{=TL26}Trade here now (TradeLord)"));
            return line;
        }

        private static bool StillSettling(bool quiet)
        {
            int wait = Options.Current.EconomySettlingDays;
            if (wait <= 0) return false;
            float elapsed = Campaign.Current.Models.CampaignTimeModel.CampaignStartTime.ElapsedDaysUntilNow;
            if (!Settling.StillHolding(wait, elapsed, out int daysLeft)) return false;
            if (!quiet)
            {
                TextObject msg = Tongue.Text("{=TL18}The market is still settling ({DAYS} more days).");
                msg.SetTextVariable("DAYS", daysLeft);
                Notices.Say(msg);
            }
            return true;
        }

        private static bool MarketOpen(Settlement settlement, bool quiet) =>
            CanTradeHere(settlement) && !StillSettling(quiet);

        private static int _pendingXp;
        private static int _pendingProfit;
        private static bool _tradeLordIsCreditingItsOwnTrade;

        internal static bool TradeLordIsCreditingItsOwnTrade => _tradeLordIsCreditingItsOwnTrade;
        private static bool _pendingXpMuted = true;

        private struct Took
        {
            internal int Units;
            internal int Gold;
            internal int Profit;
        }

        private static Took Reckon(Pass pass, List<(ItemRosterElement, int)> lines, bool selling)
        {
            var took = default(Took);
            if (pass == null || lines == null) return took;
            for (int i = 0; i < lines.Count; i++)
            {
                var (el, said) = lines[i];
                ItemObject item = el.EquipmentElement.Item;
                if (item == null || said <= 0) continue;
                int price = pass.Price(el.EquipmentElement, selling: selling);
                if (price <= 0) continue;
                int count = Deals.UnitsMoved(el.Amount, said, price);
                if (count <= 0) continue;
                took.Units += count;
                took.Gold += said;
                if (selling)
                    took.Profit += TradeMath.Credit(TradeMath.PerUnit(said, count),
                                                    TradePolicy.CostBasis(el.EquipmentElement),
                                                    TradePolicy.UnpaidWorth(item)) * count;
                pass.Tally(item, count, said);
            }
            took.Profit = Deals.NoMoreThanTheSale(took.Profit, took.Gold);
            return took;
        }

        internal static void TookTheDeal(List<(ItemRosterElement, int)> bought,
                                        List<(ItemRosterElement, int)> sold, bool quiet = false)
        {
            Settlement settlement = Settlement.CurrentSettlement;
            if (settlement?.SettlementComponent == null) return;
            int purseMoved = Counter.PurseMovedOnTheScreen();
            Pass selling = Pass.Open(settlement, quiet);
            Pass buying = Pass.Open(settlement, quiet);
            if (selling == null || buying == null) return;
            selling.OnTheScreen = true;
            buying.OnTheScreen = true;
            Took got = Reckon(selling, sold, true);
            Took paid = Reckon(buying, bought, false);
            Visit.NoteADealTaken(got.Units + paid.Units);
            bool addsUp = Deals.AddsUp(got.Gold - paid.Gold, purseMoved);
            if (!addsUp)
                Log.Write("ERROR: the deal you took reckons as " + got.Gold + " gold in and " + paid.Gold +
                          " out, a net of " + (got.Gold - paid.Gold) + ", but your purse moved " + purseMoved +
                          " on that screen. TradeLord reports what it read and counts none of it towards your " +
                          "Trade skill or your TradeLord profit, because it cannot square the two.");
            ReportWhatYouSold(selling, got, addsUp);
            ReportWhatYouBought(buying, paid, addsUp);
        }

        private static void ReportWhatYouSold(Pass pass, Took got, bool addsUp)
        {
            if (got.Units <= 0) return;
            pass.Moved(addsUp ? (int?)got.Profit : null, got.Gold, selling: true);
            Log.Write("the deal you took sold " + got.Units + " item(s) for +" + got.Gold +
                      " gold, profit about " + got.Profit + " " + pass.Where +
                      " (the gold is what the trade screen paid, and the profit is worked out on what each good fetched on average)");
            pass.Logged(selling: true, "the deal you took on the trade screen");
            TextObject msg = pass.Said(
                "{=TL13}[Simulated, best case] TradeLord would sell {ITEMS} for {GOLD} denars ({PROFIT} profit).",
                "{=TL02}TradeLord sold {ITEMS} for {GOLD} denars ({PROFIT} profit).", got.Units, got.Gold);
            msg.SetTextVariable("PROFIT", got.Profit);
            Notices.Say(msg, got.Profit > 0 ? Notices.Gain : Notices.Flat);
        }

        private static void ReportWhatYouBought(Pass pass, Took paid, bool addsUp)
        {
            if (paid.Units <= 0) return;
            pass.Moved(gold: paid.Gold, selling: false);
            Log.Write("the deal you took bought " + paid.Units + " item(s) for -" + paid.Gold +
                      " gold " + pass.Where + " (what the trade screen charged)");
            pass.Logged(selling: false, "the deal you took on the trade screen");
            Notices.Say(pass.Said("{=TL14}[Simulated, best case] TradeLord would buy {ITEMS} for {GOLD} denars.",
                            "{=TL06}TradeLord bought {ITEMS} for {GOLD} denars.", paid.Units, paid.Gold), Notices.Spend);
        }

        internal static void WatchTheTradeScreen()
        {
            TextObject closed = Counter.Watch();
            if (closed == null) return;
            Notices.Say(closed, Notices.Note);
            Guard.Run("Action.MarkerAfterTheDeal", Marker.Update);
        }

        internal static void FlushToasts()
        {
            int xp = _pendingXp;
            int profit = _pendingProfit;
            bool muted = _pendingXpMuted;
            _pendingXp = 0;
            _pendingProfit = 0;
            _pendingXpMuted = true;
            if (xp > 0) CreditTradeSkill(xp, profit, muted);
            Notices.Drain();
        }

        private const float GameTradeXpPerDenarOfProfit = 0.5f;

        private static void CreditTheCompanionsWithYou(int xp)
        {
            float each = TradeMath.PartyShareOfProfit(xp, Options.Current.PartyTradeXpShare);
            if (each <= 0f) return;
            int credited = 0;
            foreach (Hero companion in Hero.MainHero.CompanionsInParty)
            {
                if (companion == null) continue;
                companion.AddSkillXp(DefaultSkills.Trade, each * GameTradeXpPerDenarOfProfit);
                credited++;
            }
            if (credited > 0)
                Log.Write("trade skill: " + credited + " companion(s) riding with you were each credited " +
                          ((int)each) + " of the " + xp + " denars of profit");
        }

        private static void CreditTradeSkill(int xp, int profit, bool muted)
        {
            if (Campaign.Current == null || Hero.MainHero == null) return;
            int before = Hero.MainHero.GetSkillValue(DefaultSkills.Trade);
            float xpBefore = Hero.MainHero.HeroDeveloper.GetSkillXp(DefaultSkills.Trade);
            OpenTransaction();
            try { SkillLevelingManager.OnTradeProfitMade(Hero.MainHero, xp); }
            finally { CloseTransaction(); ReportSilenced(); }
            int gained = (int)Math.Round(Hero.MainHero.HeroDeveloper.GetSkillXp(DefaultSkills.Trade) - xpBefore);
            LedgerBehavior.Instance?.AddTradeXp(gained);
            bool learned = gained > 0;
            if (profit > 0)
                Guard.Run("TradeXp.Event", () =>
                {
                    _tradeLordIsCreditingItsOwnTrade = true;
                    try { CampaignEventDispatcher.Instance.OnPlayerTradeProfit(profit); }
                    finally { _tradeLordIsCreditingItsOwnTrade = false; }
                });
            Guard.Run("TradeXp.Party", () => CreditTheCompanionsWithYou(xp));
            int now = Hero.MainHero.GetSkillValue(DefaultSkills.Trade);
            bool rose = now > before;
            if (!learned && xp > 0 && Guard.Read("TradeXp.Limit", muted, SayTheLearningLimit, false))
                return;
            TextObject earned = Tongue.Text(rose
                ? "{=TL88}TradeLord credited {GOLD} denars of profit to your Trade skill, which is now {LEVEL}."
                : "{=TL81}TradeLord credited {GOLD} denars of profit to your Trade skill.");
            earned.SetTextVariable("GOLD", xp);
            if (rose) earned.SetTextVariable("LEVEL", now);
            if (!muted) Notices.Say(earned, Notices.Xp);
            if (rose) Log.Write("trade skill rose to " + now + " - named in TradeLord's own line");
        }

        private static bool SayTheLearningLimit(bool muted)
        {
            Hero hero = Hero.MainHero;
            CharacterDevelopmentModel model = Campaign.Current?.Models?.CharacterDevelopmentModel;
            SkillObject trade = DefaultSkills.Trade;
            if (model == null || hero?.HeroDeveloper == null || trade == null) return false;
            IReadOnlyPropertyOwner<CharacterAttribute> attributes = hero.CharacterAttributes;
            int focus = hero.HeroDeveloper.GetFocus(trade);
            int skill = hero.GetSkillValue(trade);
            if (TradeMath.StillLearns(model.CalculateLearningRate(attributes, focus, skill, trade).ResultNumber)) return false;
            int limit = MathF.Round(model.CalculateLearningLimit(attributes, focus, trade).ResultNumber);
            if (skill <= limit) return false;

            int focusNeeded = TradeMath.FewestThatLets(model.MaxFocusPerSkill - focus,
                more => TradeMath.StillLearns(model.CalculateLearningRate(attributes, focus + more, skill, trade).ResultNumber));
            CharacterAttribute named = null;
            int pointsNeeded = 0;
            foreach (CharacterAttribute attribute in trade.Attributes)
            {
                if (attribute == null) continue;
                if (named == null) named = attribute;
                int needed = TradeMath.FewestThatLets(model.MaxAttribute - attributes.GetPropertyValue(attribute),
                    more => TradeMath.StillLearns(
                        model.CalculateLearningRate(new Raised(attributes, attribute, more), focus, skill, trade).ResultNumber));
                if (needed > 0 && (pointsNeeded == 0 || needed < pointsNeeded))
                {
                    named = attribute;
                    pointsNeeded = needed;
                }
            }

            TextObject said = Tongue.Text(focusNeeded > 0 && pointsNeeded > 0
                ? "{=TL468}Trade XP could not be added: your Trade skill ({SKILL}) is past its learning limit ({LIMIT}). {FOCUS} more focus point(s) in Trade or {POINTS} more point(s) of {ATTRIBUTE} would let it learn again."
                : focusNeeded > 0
                    ? "{=TL469}Trade XP could not be added: your Trade skill ({SKILL}) is past its learning limit ({LIMIT}). {FOCUS} more focus point(s) in Trade would let it learn again."
                    : pointsNeeded > 0
                        ? "{=TL470}Trade XP could not be added: your Trade skill ({SKILL}) is past its learning limit ({LIMIT}). {POINTS} more point(s) of {ATTRIBUTE} would let it learn again."
                        : "{=TL471}Trade XP could not be added: your Trade skill ({SKILL}) is past its learning limit ({LIMIT}). Raise your focus in Trade and your {ATTRIBUTE} together to let it learn again.");
            said.SetTextVariable("SKILL", skill);
            said.SetTextVariable("LIMIT", limit);
            said.SetTextVariable("FOCUS", focusNeeded);
            said.SetTextVariable("POINTS", pointsNeeded);
            said.SetTextVariable("ATTRIBUTE", named?.Name?.ToString() ?? "");

            int focusToSpend = hero.HeroDeveloper.UnspentFocusPoints;
            int pointsToSpend = hero.HeroDeveloper.UnspentAttributePoints;
            Log.Repeatable("trade-xp-limit", skill + "/" + limit + "/" + focus + "/" + focusNeeded + "/" + pointsNeeded,
                           "Trade XP could not be added: Trade is at " + skill + ", past its learning limit of " +
                           limit + " with " + focus + " focus point(s) in it, so the game's learning rate for it " +
                           "is 0; " + (focusNeeded > 0 ? focusNeeded + " more focus point(s)" : "no focus alone") +
                           " or " + (pointsNeeded > 0 ? pointsNeeded + " more point(s) of " + named?.StringId
                                                      : "no attribute alone") +
                           " would let it learn again, with " + focusToSpend + " focus and " + pointsToSpend +
                           " attribute point(s) unspent");

            if (!muted) Notices.Say(said, Notices.Alert);
            if (focusToSpend <= 0 && pointsToSpend <= 0) return true;
            TextObject spend = Tongue.Text("{=TL472}You have {FOCUS} focus point(s) and {POINTS} attribute point(s) left to spend on the character screen.");
            spend.SetTextVariable("FOCUS", focusToSpend);
            spend.SetTextVariable("POINTS", pointsToSpend);
            if (!muted) Notices.Say(spend, Notices.Alert);
            return true;
        }

        private sealed class Raised : IReadOnlyPropertyOwner<CharacterAttribute>
        {
            private readonly IReadOnlyPropertyOwner<CharacterAttribute> _had;
            private readonly CharacterAttribute _raised;
            private readonly int _by;

            internal Raised(IReadOnlyPropertyOwner<CharacterAttribute> had, CharacterAttribute raised, int by)
            {
                _had = had;
                _raised = raised;
                _by = by;
            }

            public int GetPropertyValue(CharacterAttribute attribute) =>
                _had.GetPropertyValue(attribute) + (attribute == _raised ? _by : 0);

            public bool HasProperty(CharacterAttribute attribute) =>
                attribute == _raised || _had.HasProperty(attribute);
        }

        private const int NamedItemCap = 6;

        private static string MeantFor(bool selling, Dictionary<ItemObject, (string where, int price)> aimed,
                                       ItemObject item)
        {
            if (selling || item == null || !Options.Current.ExtendedDebugLogging) return "";
            if (!aimed.TryGetValue(item, out var far) || far.where == null) return "";
            return ", meant for " + far.where + " at " + far.price + " a unit";
        }

        private static void LogDetail(bool selling, bool sim, Dictionary<ItemObject, (int count, int gold)> detail,
                                      Dictionary<ItemObject, (int count, int gold)> quoted,
                                      Dictionary<ItemObject, (string where, int price)> aimed, string why)
        {
            var lines = new List<string>();
            foreach (var kv in detail)
            {
                lines.Add((selling ? "  sold " : "  bought ") + kv.Value.count + " " +
                          kv.Key.StringId + " for " + kv.Value.gold + (sim ? Counter.Aside : "") +
                          Quotation(quoted, kv.Key, kv.Value.gold) +
                          MeantFor(selling, aimed, kv.Key));
                LogAnimalMoved(lines, selling, sim, kv.Key, kv.Value.count, kv.Value.gold, why);
            }
            Log.WriteMany(lines);
        }

        private static string Quotation(Dictionary<ItemObject, (int count, int gold)> quoted,
                                        ItemObject item, int gold)
        {
            if (quoted == null || !quoted.TryGetValue(item, out var said) || said.count <= 0) return "";
            return ", which TradeLord had quoted at " + said.gold +
                   (said.gold == gold ? " and the market charged the same"
                                      : " and the market moved " + gold + " instead");
        }

        private static void LogAnimalMoved(List<string> lines, bool selling, bool sim, ItemObject item, int count, int gold, string why)
        {
            if (item == null || !item.HasHorseComponent) return;
            lines.Add("  animal " + (selling ? "out: " : "in: ") + item.StringId +
                      " (" + (item.Name == null ? item.StringId : item.Name.ToString()) + ") x" + count +
                      (selling ? " for +" : " for -") + gold + " gold" + (sim ? Counter.Aside : "") +
                      " - " + why + "; TradeLord counts it as " + TradePolicy.AnimalGroup(item));
        }

        private static string ItemSummary(Dictionary<ItemObject, (int count, int gold)> detail, int totalItems)
        {
            if (!Options.Current.DetailedTradeSummary)
            {
                TextObject count = Tongue.Text("{=TL31}{COUNT} items");
                count.SetTextVariable("COUNT", totalItems);
                return count.ToString();
            }

            var byValue = new List<KeyValuePair<ItemObject, (int count, int gold)>>(detail);
            byValue.Sort((x, y) => y.Value.gold.CompareTo(x.Value.gold));

            var sb = new StringBuilder();
            int named = 0;
            foreach (var kv in byValue)
            {
                if (named == NamedItemCap) break;
                if (named > 0) sb.Append(", ");
                sb.Append(kv.Value.count).Append(" ").Append(kv.Key.Name);
                named++;
            }
            if (detail.Count > named)
            {
                TextObject more = Tongue.Text("{=TL29}and {COUNT} more");
                more.SetTextVariable("COUNT", detail.Count - named);
                sb.Append(" ").Append(more.ToString());
            }
            return sb.ToString();
        }

        private static void Tally(Dictionary<ItemObject, (int count, int gold)> detail,
                                  ItemObject item, int count, int gold)
        {
            detail.TryGetValue(item, out var t);
            detail[item] = (t.count + count, t.gold + gold);
        }

        private static void CoinSound()
        {
            if (!Options.Current.CoinSound) return;
            try { TaleWorlds.Engine.SoundEvent.PlaySound2D("event:/ui/multiplayer/coin_add"); }
            catch {  }
        }

        private static void NoteStalled(bool selling, Block why)
        {
            if (selling) _sellStalled = why; else _buyStalled = why;
        }

        private static void ReportStalledPasses()
        {
            Block? sell = _sellStalled;
            Block? buy = _buyStalled;
            _sellStalled = null;
            _buyStalled = null;
            if (!sell.HasValue && !buy.HasValue) return;

            TextObject none;
            if (sell.HasValue && buy.HasValue)
            {
                string stoppedSelling = BlockTally.Phrase(sell.Value).ToString();
                string stoppedBuying = BlockTally.Phrase(buy.Value).ToString();
                if (stoppedSelling == stoppedBuying)
                {
                    none = Tongue.Text("{=TL94}Nothing traded here - {REASON}.");
                    none.SetTextVariable("REASON", stoppedSelling);
                }
                else
                {
                    none = Tongue.Text("{=TL95}Nothing sold here - {REASON}, and nothing bought - {SECOND}.");
                    none.SetTextVariable("REASON", stoppedSelling);
                    none.SetTextVariable("SECOND", stoppedBuying);
                }
            }
            else if (sell.HasValue)
            {
                none = Tongue.Text("{=TL32}Nothing sold here - {REASON}.");
                none.SetTextVariable("REASON", BlockTally.Phrase(sell.Value));
            }
            else
            {
                none = Tongue.Text("{=TL33}Nothing bought here - {REASON}.");
                none.SetTextVariable("REASON", BlockTally.Phrase(buy.Value));
            }
            Notices.Say(none);
        }

        public static void ExecuteQuickSell(Settlement settlement, bool quiet = false) =>
            SellPass(Pass.Open(settlement, quiet), "quick-sell", "selling", "Selling", "the selling pass");

        private static void SellPass(Pass pass, string label, string what, string named, string why)
        {
            if (pass == null) return;

            pass.Capture();

            var market = new SellingFrom(pass, what, named);

            pass.CountFrom();
            var tally = new BlockTally();

            Traded moved = default(Traded);
            InAPass(() => moved =
                TradePass.SellThem(market, pass.Books, pass.Sim, Options.Current, tally));

            int soldItems = moved.Units;
            int profit = moved.Profit;
            int goldGained = pass.Gained(moved.SimGold);

            if (pass.Site != null && !pass.Sim)
                Guard.Run("Marker.Check", () => Marker.ScoreTheMark(pass.Site, soldItems, goldGained));

            if (soldItems > 0)
            {
                pass.Moved(profit, goldGained, selling: true);
                Log.Write(pass.Headed(label) + soldItems +
                          " items, +" + goldGained + " gold, profit " + profit + " " + pass.Where);
                pass.Logged(selling: true, why);
                if (tally.Any) Log.Write("  stopped on: " + tally.Summary());
                TextObject msg = pass.Said(
                    "{=TL13}[Simulated, best case] TradeLord would sell {ITEMS} for {GOLD} denars ({PROFIT} profit).",
                    "{=TL02}TradeLord sold {ITEMS} for {GOLD} denars ({PROFIT} profit).",
                    soldItems, goldGained);
                msg.SetTextVariable("PROFIT", profit);
                if (!pass.Muted) Notices.Say(msg, profit > 0 ? Notices.Gain : Notices.Flat);
                if (!pass.Sim && moved.Earned > 0) AwardTradeXpForOurOwnTrade(moved.Earned, pass.Muted);
            }
            else if (!pass.DirectionError)
            {
                if (tally.Any) Log.Repeatable(label + "-empty " + pass.Key, tally.Summary(),
                    label + " moved nothing " + pass.Where + ": " + tally.Summary());
                Block stopped = tally.Dominant();
                if (stopped != Block.None && !pass.Muted) NoteStalled(selling: true, stopped);
            }
        }

        private sealed class SellingFrom : ISellingMarket
        {
            private readonly Pass _pass;
            private readonly List<ItemRosterElement> _plan = new List<ItemRosterElement>();
            private readonly Dictionary<ItemObject, int> _keepBack;
            private readonly Dictionary<ItemObject, int> _awaited;
            private readonly string _what;
            private readonly string _named;

            internal SellingFrom(Pass pass, string what, string named)
            {
                _pass = pass;
                _what = what;
                _named = named;
                ItemRoster roster = pass.Party.ItemRoster;
                var goods = new List<ItemObject>();
                for (int at = 0; at < roster.Count; at++) goods.Add(roster.GetItemAtIndex(at));
                LedgerBehavior.Instance?.PrimeMarketsFor(goods);
                var order = new List<(ItemRosterElement held, int gain, int at)>();
                for (int at = 0; at < roster.Count; at++)
                {
                    ItemRosterElement held = roster.GetElementCopyAtIndex(at);
                    order.Add((held, TradePolicy.CouldBeSold(held, pass.Locked)
                                         ? WhatThisStackWouldMake(pass, held) : 0, at));
                }
                order.Sort((x, y) => x.gain != y.gain ? y.gain.CompareTo(x.gain)
                                                      : x.at.CompareTo(y.at));
                for (int at = 0; at < order.Count; at++) _plan.Add(order[at].held);
                _keepBack = TradePolicy.KeptBack(roster, pass.Books, pass.Sim, out _awaited);
            }

            private static int WhatThisStackWouldMake(Pass pass, ItemRosterElement held)
            {
                ItemObject item = held.EquipmentElement.Item;
                if (item == null || held.Amount <= 0) return 0;
                int price = pass.Price(held.EquipmentElement, selling: true);
                if (price <= 0) return 0;
                long gain = ((long)price - TradePolicy.WorthToBeat(held.EquipmentElement)) * held.Amount;
                if (gain <= 0L) return 0;
                return gain > int.MaxValue ? int.MaxValue : (int)gain;
            }

            private ItemObject Item(int at) => _plan[at].EquipmentElement.Item;

            public int Count => _plan.Count;

            public bool Stopped => _pass.DirectionError;

            public bool Village => _pass.Site != null && _pass.Site.IsVillage;

            public string IdAt(int at)
            {
                ItemObject item = Item(at);
                return item == null ? null : item.StringId;
            }

            public Good GoodAt(int at) => TradePolicy.Describe(Item(at));

            public bool TheGameGivesTradeXpFor(int at) => _plan[at].EquipmentElement.ItemModifier == null;

            public bool OfAQuality(int at) => _plan[at].EquipmentElement.ItemModifier != null;

            public bool MaySell(int at, in Good good, out int keep, out Block why) =>
                TradePolicy.MaySell(good, _plan[at], _pass.Locked, _keepBack, _awaited,
                                    out keep, out why);

            public int YoursToSell(int at) => _pass.YoursToSell(_plan[at]);

            public string PaidKeyAt(int at) => LedgerBehavior.PaidKey(_plan[at].EquipmentElement);

            public int CostBasis(int at) => TradePolicy.CostBasis(_plan[at].EquipmentElement);

            public int PurchasedUnits(int at) => LedgerBehavior.Instance?.PurchasedUnits(_plan[at].EquipmentElement) ?? 0;

            public int UnpaidWorth(int at) => TradePolicy.UnpaidWorth(Item(at));

            public bool ResaleMarket(int at, out int price)
            {
                var best = LedgerBehavior.Instance?.BestSell(Item(at)) ?? (null, 0);
                EquipmentElement held = _plan[at].EquipmentElement;
                price = TradeMath.AtThisQuality(best.Item2, held.Item.Value, held.ItemValue);
                return best.Item1 != null && best.Item1 != _pass.Site;
            }

            public int PriceToSell(int at) => _pass.Price(_plan[at].EquipmentElement, selling: true);

            public int Till() => _pass.Till;

            public int TillNow() => _pass.TillNow;

            public void Staged(int at, int price)
            {
                Counter.Stage(_plan[at], selling: true, price);
                _pass.Tally(Item(at), 1, price);
            }

            public bool Give(int at, int price, out int proceeds)
            {
                ItemObject item = Item(at);
                _pass.Quote(item, 1, price);
                if (!_pass.SellOne(_plan[at], price, _what, _named, out proceeds)) return false;
                if (proceeds == 0) return true;
                _pass.Tally(item, 1, proceeds);
                return true;
            }

            public void RecordedSale(int at) =>
                LedgerBehavior.Instance?.RecordSale(PaidKeyAt(at), 1);
        }

        private const float HoldShareOff = 0f;

        private static Block WhatCapsAGood(in Good good, int price,
                                           (int count, int spent) taken, int held, float shareCap) =>
            TradeRules.WhatCapsAGood(good, price, taken, held, shareCap, Options.Current);

        private static bool NoRoomForOneMore(in Good good, float roomLeft) =>
            TradeRules.NoRoomForOneMore(good, roomLeft);

        private static List<(ItemRosterElement el, Good good, int price, int ceiling)> CheapestFirst(
            Pass pass, Func<ItemObject, bool> wanted, float tolerance = 1f)
        {
            var shelf = new List<(ItemRosterElement el, int price)>();
            var goods = new List<ItemObject>();
            ItemRoster shopRoster = pass.Stock;
            for (int i = 0; i < shopRoster.Count; i++)
            {
                ItemRosterElement el = shopRoster.GetElementCopyAtIndex(i);
                ItemObject it = el.EquipmentElement.Item;
                if (el.Amount <= 0 || !wanted(it)) continue;
                if (pass.Books.Sold(pass.Sim, it.StringId)) continue;
                if (pass.TheirsToSell(el) <= 0) continue;
                int price = pass.Price(el.EquipmentElement, selling: false);
                if (price <= 0) continue;
                shelf.Add((el, price));
                goods.Add(it);
            }
            LedgerBehavior.Instance?.PrimeMarketsFor(goods);

            var found = new List<(ItemRosterElement el, Good good, int price, int ceiling)>();
            foreach (var (el, price) in shelf)
            {
                ItemObject it = el.EquipmentElement.Item;
                int worth = TradePolicy.UnpaidWorth(it);
                int ceiling = TradeMath.MostToPayOverTheCheapest(worth, tolerance);
                if (price > ceiling) continue;
                found.Add((el, TradePolicy.Describe(it), price, ceiling));
            }
            found.Sort((x, y) => x.price.CompareTo(y.price));
            return found;
        }

        public static void ExecuteResupply(Settlement settlement, bool quiet = false)
        {
            _unfitted = default;
            if (Options.Current.KeepFoodDays <= 0) return;
            Pass pass = Pass.Open(settlement, quiet);
            if (pass == null) return;

            int shortfall = TradePolicy.FoodWanted() -
                            TradePolicy.FoodHeld(pass.Party.ItemRoster) - pass.Books.FoodHeld(pass.Sim);
            if (shortfall <= 0) return;

            int stocked = 0, simSpent = 0;
            float simWeight = pass.Books.Weight(pass.Sim);
            float shareCap = pass.ShareCap;
            (ItemRosterElement el, Good good, int fed, int ceiling, int remaining, (int, int) taken, int held)?
                firstLeft = null;

            var larder = CheapestFirst(pass,
                it => TradePolicy.IsStorableFood(it) && TradePolicy.MayBuy(it, pass.Locked, out _, toFeed: true));
            if (larder.Count == 0) return;

            pass.CountFrom();
            InAPass(() =>
            {
                foreach (var (el, good, _, ceiling) in larder)
                {
                    if (pass.DirectionError || shortfall <= 0) break;
                    ItemObject item = el.EquipmentElement.Item;
                    int fed = TradeRules.FoodValue(good);
                    if (fed <= 0) continue;
                    int remaining = pass.TheirsToSell(el);
                    var prior = pass.Books.Purchases(pass.Sim, item.StringId);
                    int countThis = prior.count, spentThis = prior.spent;
                    int held = LedgerBehavior.InAll(pass.Party.ItemRoster, item) +
                               pass.Books.Held(pass.Sim, item.StringId);

                    while (shortfall > 0 && remaining > 0)
                    {
                        int price = pass.Price(el.EquipmentElement, selling: false);
                        if (price <= 0 || price > ceiling) break;
                        if (pass.WouldReachYourReserve(price)) break;
                        if (WhatCapsAGood(good, price, (countThis, spentThis), held, shareCap) != Block.None) break;
                        if (settlement.IsVillage && remaining <= 1) break;
                        if (NoRoomForOneMore(good, pass.Room() - simWeight))
                        {
                            if (firstLeft == null)
                                firstLeft = (el, good, fed, ceiling, remaining, (countThis, spentThis), held);
                            break;
                        }

                        if (pass.Sim)
                        {
                            simSpent += price;
                            pass.Books.NotePurchase(item.StringId, price, good.Weight, fed);
                            simWeight = pass.Books.Weight(pass.Sim);
                            Counter.Stage(el, selling: false, price);
                        }
                        else
                        {
                            if (!pass.BuyOne(el, price, "restocking", "Restocking", out price)) break;
                            if (price == 0) break;
                            LedgerBehavior.Instance?.RecordPurchase(LedgerBehavior.PaidKey(el.EquipmentElement), 1, price);
                            pass.Books.NoteBought(item.StringId, price);
                        }
                        stocked++;
                        remaining--;
                        shortfall -= fed;
                        countThis++;
                        spentThis += price;
                        held++;
                        pass.Tally(item, 1, price);
                    }
                }
            });

            if (shortfall > 0 && firstLeft.HasValue && !pass.DirectionError)
            {
                var left = firstLeft.Value;
                var food = FoodTheHoldLeftBehind(pass, left.el, left.good, left.fed, left.ceiling, shortfall,
                                                 left.remaining, left.taken, left.held, shareCap,
                                                 settlement.IsVillage);
                _unfitted = (food.weight, food.cost, 0f, true);
            }
            if (stocked <= 0) return;

            int spent = pass.Spent(simSpent);
            pass.Moved(gold: spent, selling: false);
            Log.Write((pass.Sim ? "resupply (simulated, best case): " : "resupply: ") + stocked +
                      " items, -" + spent + " gold at " + settlement.Name +
                      ", still short " + (shortfall > 0 ? shortfall : 0) + " unit(s) of food");
            pass.Logged(selling: false, "restocking the larder");
            TextObject msg = pass.Said(
                "{=TL98}[Simulated, best case] TradeLord would restock {ITEMS} for {GOLD} denars.",
                "{=TL97}TradeLord restocked {ITEMS} for {GOLD} denars.",
                stocked, spent);
            if (!pass.Muted) Notices.Say(msg, Notices.Spend);
        }

        private static (float weight, int cost) FoodTheHoldLeftBehind(Pass pass, ItemRosterElement el, in Good good,
                                                                      int fed, int ceiling, int shortfall,
                                                                      int remaining, (int count, int spent) taken,
                                                                      int held, float shareCap, bool village)
        {
            Func<int, int> ahead = pass.PricesAhead(el.EquipmentElement);
            int budget = pass.Spendable();
            float weight = 0f;
            int cost = 0, units = 0;
            while (shortfall > 0 && remaining > 0)
            {
                int price = ahead(units);
                if (price <= 0 || price > ceiling || price >= budget) break;
                if (WhatCapsAGood(good, price, taken, held, shareCap) != Block.None) break;
                if (village && remaining <= 1) break;
                units++;
                remaining--;
                held++;
                shortfall -= fed;
                budget -= price;
                weight += good.Weight;
                cost = TradeMath.AddedUp(cost, price);
                taken = (taken.count + 1, taken.spent + price);
            }
            return (weight, cost);
        }

        private static IMarketData _roadMarket;
        private static bool _roadMarketFailed;

        internal static void ForgetRoadMarket() { _roadMarket = null; _roadMarketFailed = false; }

        private static IMarketData RoadMarket()
        {
            if (_roadMarket != null || _roadMarketFailed) return _roadMarket;
            try
            {
                Type made = typeof(Settlement).Assembly.GetType("TaleWorlds.CampaignSystem.Settlements.FakeMarketData");
                _roadMarket = made == null ? null : Activator.CreateInstance(made, true) as IMarketData;
            }
            catch (Exception e) { Log.Error(e, "road pricing (trading with caravans is off)"); }
            if (_roadMarket == null)
            {
                _roadMarketFailed = true;
                Log.Write("caravan trading: this game version has no off-market pricing TradeLord can read - trading with caravans is disabled, markets are unaffected");
            }
            return _roadMarket;
        }

        private static bool _saidTheOfferIsUnguarded;

        private static IMarketData TheirOfferFrom(MobileParty villagers)
        {
            Village home = villagers.HomeSettlement?.Village;
            if (home == null)
            {
                Log.Write(villagers.Name + " have no home village to price their offer at, " +
                          "so TradeLord leaves their offer to you");
                return null;
            }
            if (Meetings.TheirOfferIsGuarded()) return new TheirOffer(home);
            if (!_saidTheOfferIsUnguarded)
            {
                _saidTheOfferIsUnguarded = true;
                Log.Write("villagers: TradeLord could not take hold of the game's own offer on this game " +
                          "version, so it could not stop the same goods being bought twice. It leaves every " +
                          "villagers' offer to you until that is fixed.");
            }
            return null;
        }

        private static IMarketData PricedOnTheRoad(MobileParty met)
        {
            if (met != null && met.IsVillager) return TheirOfferFrom(met);
            if (met == null || !met.IsCaravan) return RoadMarket();
            MobileParty me = MobileParty.MainParty;
            Settlement near = me == null ? null : me.CurrentSettlement ??
                SettlementHelper.FindNearestTownToMobileParty(me, MobileParty.NavigationType.All)?.Settlement;
            IMarketData kept = Priced.Kept(near);
            if (kept == null) return RoadMarket();
            Log.Write("caravan " + met.Name + " is priced at " + near.Name + ", the market the game's own trade " +
                      "screen prices a caravan from");
            return kept;
        }

        private static void HandOver(PartyBase me, PartyBase shop, EquipmentElement what, int price)
        {
            me.ItemRoster.AddToCounts(what, -1);
            shop.ItemRoster.AddToCounts(what, 1);
            if (price > 0) GiveGoldAction.ApplyForPartyToCharacter(shop, Hero.MainHero, price, true);
        }

        private static void TakeDelivery(PartyBase shop, PartyBase me, EquipmentElement what, int price)
        {
            shop.ItemRoster.AddToCounts(what, -1);
            me.ItemRoster.AddToCounts(what, 1);
            if (price > 0) GiveGoldAction.ApplyForCharacterToParty(Hero.MainHero, shop, price, true);
        }

        private static bool RoadPartyReachable(MobileParty met)
        {
            if (!Meetings.IsRoadTrader(met)) return false;
            if (met.IsCaravan && met.Party?.Owner == Hero.MainHero)
            {
                Log.Write(met.Name + " is your own caravan, and the game never lets you trade with one of " +
                          "your own, so TradeLord leaves it alone");
                return false;
            }
            if (met.IsCaravan && (met.IsInRaftState || !Meetings.CarriesGoods(met)))
            {
                Log.Write(met.Name + (met.IsInRaftState ? " are on a raft" : " carry no goods") +
                          ", and the game only trades with a caravan that is off its raft and has goods to show, " +
                          "so TradeLord leaves it alone");
                return false;
            }
            if (!Options.Current.ExcludeHostileTowns) return true;
            IFaction mine = Hero.MainHero?.MapFaction;
            return mine == null || met.MapFaction == null ||
                   !FactionManager.IsAtWarAgainstFaction(met.MapFaction, mine);
        }

        private static Books _meetingBooks;
        private static MobileParty _meetingBooksFor;
        private static double _meetingHours = -1d;

        internal static void ForgetTheMeeting()
        {
            _meetingBooks = null;
            _meetingBooksFor = null;
            _meetingHours = -1d;
        }

        private static Books BooksForTheMeeting(MobileParty met)
        {
            double hours = CampaignTime.Now.ToHours;
            if (_meetingBooks != null && _meetingBooksFor == met && Arrivals.StraightBack(hours, _meetingHours))
            {
                _meetingHours = hours;
                _meetingBooks.ForgetTheDryRun();
                Log.Write("meeting " + met.Name + " again: what TradeLord already traded with them still " +
                          "stands, so nothing it sold them is bought back and what it spent still counts");
                return _meetingBooks;
            }
            _meetingBooks = new Books();
            _meetingBooksFor = met;
            _meetingHours = hours;
            return _meetingBooks;
        }

        public static void ExecuteRoadTrade(MobileParty met)
        {
            if (!Options.Current.TradeWithCaravans) return;
            if (!RoadPartyReachable(met)) return;
            if (StillSettling(Muted(automated: true))) return;
            IMarketData road = PricedOnTheRoad(met);
            if (road == null) return;
            MobileParty party = MobileParty.MainParty;
            if (party == null) return;

            Books books = BooksForTheMeeting(met);
            int movesBefore = books.Moves(Simulating);
            string why = "trading with a party on the road";
            if (met.IsVillager)
                LotPass(Pass.Meet(met, road, books, party), why);
            else
            {
                SellPass(Pass.Meet(met, road, books, party),
                         "sale on the road", "selling on the road", "Road trading", why);
                BuyPass(Pass.Meet(met, road, books, party),
                        "purchase on the road", "buying on the road", "Road buying", why);
            }
            NoteARoadTrade(met, books, movesBefore);
            ReportStalledPasses();
        }

        private const int RankLivestock = TradeRules.RankLivestock;
        private const int RankHaulAnimal = TradeRules.RankHaulAnimal;

        public static void ExecuteHerdRelief(Settlement settlement, bool quiet = false)
        {
            if (!Options.Current.SellSpareMounts) return;
            Pass pass = Pass.Open(settlement, quiet);
            if (pass == null) return;

            int shed = Drove.AnimalsToShed(pass.Party);
            shed -= pass.Books.Shed(pass.Sim);
            if (shed <= 0) return;

            ItemRoster mine = pass.Party.ItemRoster;
            if (!Errands.AnimalsKnown) return;
            TradePolicy.KeptBack(mine, pass.Books, pass.Sim, out Dictionary<ItemObject, int> promised);

            int mountsLeft = Drove.SpareMounts(pass.Party);
            mountsLeft -= pass.Books.MountsShed(pass.Sim);
            int haulsLeft = -1;

            var stable = new List<(ItemRosterElement el, int rank, int price)>();
            for (int i = 0; i < mine.Count; i++)
            {
                ItemRosterElement el = mine.GetElementCopyAtIndex(i);
                ItemObject it = el.EquipmentElement.Item;
                if (el.Amount <= 0 || !TradePolicy.MayShedForHerd(el.EquipmentElement, pass.Locked)) continue;
                int rank = Drove.ShedRank(it);
                if (rank < 0) continue;
                if (!Herding.TheGameCountsItAtOnce(rank == RankLivestock, el.EquipmentElement.ItemModifier != null)) continue;
                if (pass.YoursToSell(el) <= 0) continue;
                int price = pass.Price(el.EquipmentElement, selling: true);
                if (price <= 0) continue;
                stable.Add((el, rank, price));
            }
            if (stable.Count == 0) { Drove.SayWhatItWillNotGiveUp(mine, shed, settlement); return; }
            stable.Sort((x, y) => x.rank != y.rank ? x.rank.CompareTo(y.rank) : x.price.CompareTo(y.price));

            int sold = 0, profit = 0, earned = 0, simGold = 0, simTill = pass.Till;

            pass.CountFrom();
            InAPass(() =>
            {
                foreach (var (el, rank, _) in stable)
                {
                    if (pass.DirectionError || shed <= 0) break;
                    ItemObject item = el.EquipmentElement.Item;
                    int remaining = pass.YoursToSell(el);
                    if (promised.TryGetValue(item, out int owed) && owed > 0)
                    {
                        int spare = Math.Min(remaining, owed);
                        promised[item] = owed - spare;
                        remaining -= spare;
                    }

                    string paidKey = LedgerBehavior.PaidKey(el.EquipmentElement);
                    Basis basis = Basis.For(TradePolicy.CostBasis(el.EquipmentElement),
                                            LedgerBehavior.Instance?.PurchasedUnits(el.EquipmentElement) ?? 0,
                                            paidKey, pass.Books, pass.Sim, Options.Current);

                    while (remaining > 0 && shed > 0)
                    {
                        if (rank != RankLivestock && rank != RankHaulAnimal && mountsLeft <= 0) break;
                        if (rank == RankHaulAnimal && haulsLeft < 0)
                            haulsLeft = Math.Max(0, Drove.HaulAnimalsCargoCanSpare(pass.Party) - pass.Books.HaulsShed(pass.Sim));
                        if (rank == RankHaulAnimal && haulsLeft <= 0) break;
                        int price = pass.Price(el.EquipmentElement, selling: true);
                        if (price <= 0) break;
                        if (TradeRules.WhatTheTillCanPay(pass.Sim ? simTill : pass.TillNow,
                                                         settlement.IsVillage) < price) break;
                        int worth = basis.Unit(out bool askTheMarket);
                        if (askTheMarket) basis.UnpaidWorth = TradePolicy.UnpaidWorth(item);

                        if (pass.Sim)
                        {
                            simTill -= price;
                            simGold += price;
                            pass.Books.NoteSale(item.StringId, price, 0f, TradePolicy.FoodValue(item));
                            pass.Books.NoteShed(rank == RankHaulAnimal, rank != RankLivestock);
                            Counter.Stage(el, selling: true, price);
                        }
                        else
                        {
                            if (!pass.SellOne(el, price, "selling an animal to relieve the herd", "Herd relief", out price)) break;
                            if (price == 0) break;
                            pass.Books.NoteSold(item.StringId);
                        }
                        if (basis.SoldOne())
                        {
                            if (pass.Sim) pass.Books.NotePaidDrawn(paidKey);
                            else LedgerBehavior.Instance?.RecordSale(paidKey, 1);
                        }
                        int credited = TradePolicy.Credit(price, worth, basis.UnpaidWorth);
                        profit += credited;
                        if (el.EquipmentElement.ItemModifier == null) earned += credited;
                        sold++;
                        remaining--;
                        shed--;
                        if (rank == RankHaulAnimal) haulsLeft--;
                        else if (rank != RankLivestock) mountsLeft--;
                        pass.Tally(item, 1, price);
                    }
                }
            });

            if (sold <= 0) return;

            int gained = pass.Gained(simGold);
            pass.Moved(profit, gained, selling: true);
            Log.Write((pass.Sim ? "herd relief (simulated, best case): " : "herd relief: ") + sold +
                      " sold, +" + gained + " gold, profit " + profit + " at " + settlement.Name);
            pass.Logged(selling: true, "herd relief, getting the party back up to speed");
            TextObject msg = pass.Said(
                "{=TL117}[Simulated, best case] TradeLord would sell {ITEMS} for {GOLD} denars to get your party back up to speed.",
                "{=TL116}TradeLord sold {ITEMS} for {GOLD} denars to get your party back up to speed.",
                sold, gained);
            if (!pass.Muted) Notices.Say(msg, Notices.Gain);
            if (!pass.Sim && earned > 0) AwardTradeXpForOurOwnTrade(earned, pass.Muted);
        }

        private static bool PurseBelowTheHaulAnimalFloor(Pass pass)
        {
            int floor = Options.Current.HaulAnimalGoldFloor;
            if (floor <= 0) return false;
            int purse = Hero.MainHero.Gold + pass.Books.Purse(pass.Sim);
            if (purse > floor) return false;
            Log.Repeatable("haul animal floor", purse + "/" + floor,
                           "haul animals are left alone: your purse is at " + purse +
                           " gold and Gold before it buys a haul animal is " + floor +
                           ", so nothing is bought until you are above it");
            return true;
        }

        public static bool ExecuteHaulage(Settlement settlement, bool quiet = false)
        {
            var unfitted = _unfitted;
            _unfitted = default;
            if (!Options.Current.BuyHaulAnimals || unfitted.weight <= 0f || unfitted.cost <= 0) return false;
            Pass pass = Pass.Open(settlement, quiet);
            if (pass == null) return false;
            if (PurseBelowTheHaulAnimalFloor(pass)) return false;

            int herdRoom = Drove.RoomForLivestock(pass.Party);
            herdRoom -= pass.Books.HerdTaken(pass.Sim);
            if (herdRoom <= 0) return false;

            float each = Drove.CargoAHaulAnimalAdds(pass.Party);
            if (each <= 0f) return false;
            float roomLeft = pass.Room() - pass.Books.Weight(pass.Sim);
            if (roomLeft < 0f)
            {
                Log.Repeatable("haul animal overload", settlement.StringId,
                               "haul animals are left alone at " + settlement.Name + ": your cargo is already " +
                               (-roomLeft).ToString("0", System.Globalization.CultureInfo.InvariantCulture) +
                               " over what TradeLord may fill, so none is bought to carry what it left behind");
                return false;
            }

            int hauled = 0, simSpent = 0;
            bool enough = false, tooLittle = false, floored = false;
            int floor = Options.Current.HaulAnimalGoldFloor, purseAtTheFloor = 0, priceAtTheFloor = 0;
            float leastFilled = Herding.LeastFilledFor(unfitted.food,
                TradePolicy.FoodHeld(pass.Party.ItemRoster) + pass.Books.FoodHeld(pass.Sim),
                TradePolicy.FoodForADay());

            var stable = CheapestFirst(pass, it => TradePolicy.MayHaul(it, pass.Locked),
                                       Options.Current.HaulAnimalPriceTolerance);
            if (stable.Count == 0) return false;

            pass.CountFrom();
            InAPass(() =>
            {
                foreach (var (el, good, _, ceiling) in stable)
                {
                    if (pass.DirectionError || enough) break;
                    if (!Herding.TheGameCountsItAtOnce(false, el.EquipmentElement.ItemModifier != null)) continue;
                    ItemObject item = el.EquipmentElement.Item;
                    int remaining = pass.TheirsToSell(el);
                    var prior = pass.Books.Purchases(pass.Sim, item.StringId);
                    int countThis = prior.count, spentThis = prior.spent;
                    int held = LedgerBehavior.InAll(pass.Party.ItemRoster, item) +
                               pass.Books.Held(pass.Sim, item.StringId);

                    while (remaining > 0 && herdRoom > 0)
                    {
                        int price = pass.Price(el.EquipmentElement, selling: false);
                        if (price <= 0 || price > ceiling) break;
                        int purse = Hero.MainHero.Gold + pass.Books.Purse(pass.Sim);
                        if (!Herding.PurseClearsTheFloor(purse - price, floor))
                        {
                            floored = true;
                            purseAtTheFloor = purse;
                            priceAtTheFloor = price;
                            enough = true;
                            break;
                        }
                        if (pass.WouldReachYourReserve(price)) break;
                        if (WhatCapsAGood(good, price, (countThis, spentThis), held, HoldShareOff) != Block.None) break;
                        if (settlement.IsVillage && remaining <= 1) break;
                        float stillToCarry = TradeMath.WeightTheBudgetCanStillBuy(unfitted.weight, unfitted.cost,
                                                                                   pass.Spendable() - price);
                        float profitToCarry = TradeMath.ProfitTheBudgetCanStillBuy(unfitted.profit, unfitted.cost,
                                                                                    pass.Spendable() - price);
                        if (!Herding.AnotherHaulAnimalIsWanted(hauled, each, Options.Current.MaxCargoShare,
                                                               roomLeft, stillToCarry, leastFilled,
                                                               profitToCarry, price))
                        {
                            tooLittle = hauled == 0 &&
                                        Herding.AnotherHaulAnimalIsWanted(hauled, each, Options.Current.MaxCargoShare,
                                                                          roomLeft, stillToCarry);
                            enough = true;
                            break;
                        }

                        if (pass.Sim)
                        {
                            simSpent += price;
                            pass.Books.NotePurchase(item.StringId, price, 0f, TradeRules.FoodValue(good));
                            pass.Books.NoteHerdTaken();
                            pass.Books.NoteCapacityAdded(each);
                            Counter.Stage(el, selling: false, price);
                        }
                        else
                        {
                            if (!pass.BuyOne(el, price, "buying a haul animal", "Haul animal buying", out price)) break;
                            if (price == 0) break;
                            LedgerBehavior.Instance?.RecordPurchase(LedgerBehavior.PaidKey(el.EquipmentElement), 1, price);
                            pass.Books.NoteBought(item.StringId, price);
                        }
                        hauled++;
                        remaining--;
                        herdRoom--;
                        countThis++;
                        spentThis += price;
                        held++;
                        pass.Tally(item, 1, price);
                    }
                }
            });

            if (hauled <= 0)
            {
                if (floored)
                    Log.Repeatable("haul animal floor after paying", settlement.StringId,
                                   "haul animals are left alone at " + settlement.Name + ": the cheapest, at " +
                                   priceAtTheFloor + ", would leave your purse at " + (purseAtTheFloor - priceAtTheFloor) +
                                   ", not above the " + floor + " set in Gold before it buys a haul animal");
                else if (tooLittle)
                    Log.Repeatable("haul animal too little", settlement.StringId,
                                   "haul animals are left alone at " + settlement.Name + (unfitted.food
                                       ? ": the food your full cargo left behind would fill less than half of one, " +
                                         "and your party still has a day of food or more"
                                       : ": the goods your full cargo left behind that the gold left can buy would " +
                                         "fill less than half of one and make less than it costs"));
                return false;
            }

            int spent = pass.Spent(simSpent);
            pass.Moved(gold: spent, selling: false);
            float carrying = TradeMath.WeightTheBudgetCanStillBuy(unfitted.weight, unfitted.cost, pass.Spendable());
            Log.Write((pass.Sim ? "haul animals (simulated, best case): " : "haul animals: ") + hauled +
                      " bought, -" + spent + " gold at " + settlement.Name + ", for " +
                      (unfitted.food ? "food" : "goods") + " weighing " +
                      carrying.ToString("0", System.Globalization.CultureInfo.InvariantCulture) +
                      " that your full cargo left behind and the gold left can still buy" +
                      (floored
                          ? ", stopping there as the next, at " + priceAtTheFloor + ", would leave your purse at " +
                            (purseAtTheFloor - priceAtTheFloor) + ", not above the " + floor +
                            " set in Gold before it buys a haul animal"
                          : ""));
            pass.Logged(selling: false, "stocking the baggage train");
            TextObject msg = pass.Said(
                "{=TL111}[Simulated, best case] TradeLord would buy {ITEMS} for {GOLD} denars to carry more.",
                "{=TL110}TradeLord bought {ITEMS} for {GOLD} denars to carry more.",
                hauled, spent);
            if (!pass.Muted) Notices.SayAfterXp(msg, Notices.Spend);
            if (!unfitted.food)
            {
                _buyStalled = null;
                _cargoWasFull = false;
            }
            return true;
        }

        public static void ExecuteQuickBuy(Settlement settlement, bool quiet = false) =>
            BuyPass(Pass.Open(settlement, quiet), "quick-buy", "buying", "Buying", "the buying pass");

        private static void BuyPass(Pass pass, string label, string what, string named, string why)
        {
            _unfitted = default;
            if (pass == null) return;

            pass.Capture();

            pass.CountFrom();
            var tally = new BlockTally();
            float shareCap = pass.ShareCap;
            var market = new BuyingAt(pass, what, named);

            LedgerBehavior.ForgetWhatPickingABuyerCost();
            var stock = new List<Pick>();
            if (pass.Spendable() > 0)
                stock = TradePass.WhatToBuy(market, pass.Books, pass.Sim, shareCap,
                                            Options.Current, tally);
            else { tally.Note(Block.BudgetSpent); SayWhatHoldsYourPurse(pass); }

            Traded moved = default(Traded);
            InAPass(() => moved =
                TradePass.BuyThem(stock, market, pass.Books, pass.Sim, shareCap,
                                  Options.Current, tally));

            int bought = moved.Units;
            SayWhatPickingABuyerCost();

            if (pass.Reports && tally.Saw(Block.CarryWeight)) _cargoWasFull = true;
            if (pass.Reports)
                _unfitted = pass.DirectionError
                    ? default
                    : (moved.Unfitted, moved.UnfittedCost, moved.UnfittedProfit, false);

            int spent = pass.Spent(moved.SimGold);
            if (bought > 0)
            {
                pass.Moved(gold: spent, selling: false);
                Log.Write(pass.Headed(label) + bought +
                          " items, -" + spent + " gold " + pass.Where);
                pass.Logged(selling: false, why);
                if (tally.Any) Log.Write("  stopped on: " + tally.Summary());
                TextObject msg = pass.Said(
                    "{=TL14}[Simulated, best case] TradeLord would buy {ITEMS} for {GOLD} denars.",
                    "{=TL06}TradeLord bought {ITEMS} for {GOLD} denars.",
                    bought, spent);
                if (!pass.Muted) Notices.Say(msg, Notices.Spend);
            }
            else if (!pass.DirectionError)
            {
                if (tally.Any) Log.Repeatable(label + "-empty " + pass.Key, tally.Summary(),
                    label + " moved nothing " + pass.Where + ": " + tally.Summary());
                Block stopped = tally.Dominant();
                if (stopped != Block.None && !pass.Muted) NoteStalled(selling: false, stopped);
            }
        }

        private static void LotPass(Pass pass, string why)
        {
            const string label = "villagers' offer taken";
            pass.CountFrom();
            var market = new BuyingAt(pass, "buying the villagers' offer", "Taking the villagers' offer",
                                      theirOffer: true);

            LedgerBehavior.ForgetWhatPickingABuyerCost();
            Block stops = TradePass.WhatStopsTheLot(market, pass.Books, pass.Sim, Options.Current, out Lot lot);
            SayWhatPickingABuyerCost();
            if (lot.Units == 0) return;
            Log.Write(pass.Met.Name + " offer " + lot.Units + " goods for " + lot.Price + " gold" +
                      (lot.Weighed ? ", which TradeLord reckons it can sell on for " + (int)lot.Resale : "") +
                      (stops == Block.None
                          ? ": TradeLord takes the lot"
                          : ": TradeLord leaves the offer to you (" + stops +
                            (lot.Stopper >= 0 ? ", over " + market.IdAt(lot.Stopper) : "") +
                            (stops == Block.BudgetSpent ? ", " + market.Spendable() + " of your purse free to spend" : "") +
                            ")"));

            if (stops != Block.None)
            {
                if (!pass.Muted) Notices.Say(WhyTheOfferIsLeft(market, stops, lot), Notices.Note);
                return;
            }

            Traded moved = default(Traded);
            InAPass(() => moved = TradePass.TakeTheLot(market, pass.Books, pass.Sim));

            int bought = moved.Units;
            int spent = pass.Spent(moved.SimGold);
            if (bought <= 0) return;
            if (!pass.Sim) Meetings.TookTheirOffer(pass.Met);
            pass.Moved(gold: spent, selling: false);
            Log.Write(pass.Headed(label) + bought + " items, -" + spent + " gold " + pass.Where);
            pass.Logged(selling: false, why);
            TextObject msg = pass.Said(
                "{=TL14}[Simulated, best case] TradeLord would buy {ITEMS} for {GOLD} denars.",
                "{=TL06}TradeLord bought {ITEMS} for {GOLD} denars.",
                bought, spent);
            if (!pass.Muted) Notices.Say(msg, Notices.Spend);
        }

        private static TextObject WhyTheOfferIsLeft(BuyingAt market, Block stops, in Lot lot)
        {
            TextObject said;
            if (stops == Block.BelowMargin)
            {
                said = Tongue.Text("{=TL461}TradeLord left the villagers' offer to you: {GOLD} denars for goods it reckons it can sell on for {RESALE}, short of your Minimum profit margin.");
                said.SetTextVariable("GOLD", lot.Price);
                said.SetTextVariable("RESALE", (int)lot.Resale);
            }
            else if (stops == Block.BudgetSpent)
            {
                said = Tongue.Text("{=TL462}TradeLord left the villagers' offer to you: they ask {GOLD} denars, and {SPEND} of your purse is free to spend.");
                said.SetTextVariable("GOLD", lot.Price);
                said.SetTextVariable("SPEND", Math.Max(0, market.Spendable()));
            }
            else if (lot.Stopper >= 0)
            {
                said = Tongue.Text("{=TL463}TradeLord left the villagers' offer to you because of the {GOOD} in it: {REASON}.");
                said.SetTextVariable("GOOD", market.NameAt(lot.Stopper));
                said.SetTextVariable("REASON", WhyOneGoodKeepsTheOfferOff(stops));
            }
            else
            {
                said = Tongue.Text("{=TL464}TradeLord left the villagers' offer to you: {REASON}.");
                said.SetTextVariable("REASON", BlockTally.Phrase(stops));
            }
            return said;
        }

        private static TextObject WhyOneGoodKeepsTheOfferOff(Block stops)
        {
            switch (stops)
            {
                case Block.MountOrHaulAnimal:
                    return Tongue.Text("{=TL465}TradeLord never buys a horse to trade");
                case Block.NotMerchandise:
                case Block.NotTradable:
                    return Tongue.Text("{=TL466}it is not a good TradeLord trades");
                case Block.NoStock:
                    return Tongue.Text("{=TL467}the game puts no price on it");
                default:
                    return BlockTally.Phrase(stops);
            }
        }

        private sealed class BuyingAt : IBuyingMarket
        {
            private readonly Pass _pass;
            private readonly string _what;
            private readonly string _named;
            private readonly bool _theirOffer;
            private ItemRosterElement[] _shelf;
            private Dictionary<string, int> _asked;
            private Dictionary<int, (Settlement where, Ladder rungs, int till)> _resale;
            private Dictionary<ItemObject, (int units, int gold)> _resold;

            internal BuyingAt(Pass pass, string what, string named, bool theirOffer = false)
            {
                _pass = pass;
                _what = what;
                _named = named;
                _theirOffer = theirOffer;
            }

            private bool InTheOffer(int at) =>
                !_theirOffer || Item(at)?.ItemCategory != DefaultItemCategories.PackAnimal;

            private ItemRosterElement[] Shelf
            {
                get
                {
                    if (_shelf != null) return _shelf;
                    ItemRoster shopRoster = _pass.Stock;
                    _shelf = new ItemRosterElement[shopRoster.Count];
                    for (int at = 0; at < _shelf.Length; at++)
                        _shelf[at] = shopRoster.GetElementCopyAtIndex(at);
                    return _shelf;
                }
            }

            private ItemObject Item(int at) => Shelf[at].EquipmentElement.Item;

            public int Count => Shelf.Length;

            public bool Stopped => _pass.DirectionError;

            public bool Village => _pass.Site != null && _pass.Site.IsVillage;

            public int AmountAt(int at) => InTheOffer(at) ? Shelf[at].Amount : 0;

            public Good GoodAt(int at) => TradePolicy.Describe(Item(at));

            public bool MayBuy(int at, in Good good, out Block why) =>
                TradePolicy.MayBuy(good, Item(at), _pass.Locked, out why, wholeOffer: _theirOffer);

            public string IdAt(int at) => Item(at)?.StringId ?? "";

            public string NameAt(int at) => Item(at)?.Name?.ToString() ?? IdAt(at);

            public int TheLedgerAsksFor(int at)
            {
                if (_asked == null)
                    _asked = LedgerBehavior.Instance?.WhatTheLedgerBuysAt(_pass.Site)
                             ?? new Dictionary<string, int>(StringComparer.Ordinal);
                ItemObject item = Item(at);
                return item != null && _asked.TryGetValue(item.StringId, out int rank) ? rank : 0;
            }

            public int TheirsToSell(int at) => InTheOffer(at) ? _pass.TheirsToSell(Shelf[at]) : 0;

            public int Carried(int at) => LedgerBehavior.InAll(_pass.Party.ItemRoster, Item(at));

            public void PriceTheMarketsFor(List<Pick> shelf)
            {
                var goods = new List<ItemObject>();
                foreach (Pick one in shelf) goods.Add(Item(one.At));
                LedgerBehavior.Instance?.PrimeMarketsFor(goods);
            }

            public bool ResaleMarket(int at, int paid, int units, out int price)
            {
                var elsewhere = LedgerBehavior.Instance?
                    .WhereThisEarnsFastest(Item(at), paid, units, _pass.Site) ?? (null, 0, null);
                price = elsewhere.price;
                Settlement buyer = elsewhere.town;
                if (buyer == null) return false;
                ItemObject good = Item(at);
                if (good != null)
                    _pass.Aimed[good] = (Tongue.Named(buyer.Name, buyer.StringId), price);
                if (_resale == null)
                    _resale = new Dictionary<int, (Settlement, Ladder, int)>();
                _resale[at] = (buyer, elsewhere.rungs ?? new Ladder(buyer, Item(at), true, price, 0),
                               Options.Current.Omniscient
                                   ? TradeRules.WhatTheTillCanPay(buyer.SettlementComponent?.Gold ?? 0,
                                                                  buyer.IsVillage)
                                   : 0);
                return true;
            }

            public int ResaleUpTo(int at, int units) =>
                units <= 0 || _resale == null || !_resale.TryGetValue(at, out var far)
                    ? 0 : far.rungs.Through(units);

            public void Resold(int at, int units, int gold)
            {
                ItemObject good = Item(at);
                if (good == null || units <= 0 || !_pass.Aimed.TryGetValue(good, out var aimed)) return;
                if (_resold == null) _resold = new Dictionary<ItemObject, (int units, int gold)>();
                _resold.TryGetValue(good, out var was);
                was = (was.units + units, was.gold + gold);
                _resold[good] = was;
                _pass.Aimed[good] = (aimed.where, TradeMath.PerUnit(was.gold, was.units));
            }

            public int ResaleTill(int at) =>
                _resale != null && _resale.TryGetValue(at, out var far) ? far.till : 0;

            public int PriceToBuy(int at) => _pass.Price(Shelf[at].EquipmentElement, selling: false);

            public Func<int, int> PricesAhead(int at) => _pass.PricesAhead(Shelf[at].EquipmentElement);

            public int Spendable() =>
                _theirOffer ? TradeActionBehavior.PurseForTheirOffer(_pass.Books, _pass.Sim) : _pass.Spendable();

            public float Room() => _pass.Room();

            public int HerdRoom() => Drove.RoomForLivestock(_pass.Party);

            public void Staged(int at, int price)
            {
                Counter.Stage(Shelf[at], selling: false, price);
                _pass.Tally(Item(at), 1, price);
            }

            public bool Take(int at, int price, out int cost)
            {
                ItemObject item = Item(at);
                _pass.Quote(item, 1, price);
                if (!_pass.BuyOne(Shelf[at], price, _what, _named, out cost)) return false;
                if (cost == 0) return true;
                LedgerBehavior.Instance?.RecordPurchase(LedgerBehavior.PaidKey(Shelf[at].EquipmentElement), 1, cost);
                _pass.Tally(item, 1, cost);
                return true;
            }
        }

        public static void ShowLedgerReport()
        {
            if (LedgerPanel.TryShowFromMenu()) return;

            var routes = LedgerBehavior.Instance?.BestRoutes(6);
            Log.Write("ledger report: " + (routes?.Count ?? 0) + " profitable routes");
            string body;
            if (routes == null || routes.Count == 0)
                body = Tongue.Text(Options.Current.Omniscient
                    ? "{=TL08}No profitable routes within your travel ceilings. Raise the ceilings in the Knowledge settings, or move nearer to more markets."
                    : "{=TL89}No profitable routes within your travel ceilings, from the prices you have recorded so far. Walk more markets, or raise the ceilings in the Knowledge settings.").ToString();
            else
            {
                var sb = new StringBuilder();
                foreach (var r in routes)
                {
                    TextObject line = Tongue.Text(
                        "{=TL84}{ITEM}: buy {FROM} ({BUY} denars) -> sell {TO} ({SELL})  x{QTY} = +{PROFIT} denars, ~{DAYS} days from here");
                    line.SetTextVariable("ITEM", r.Item.Name);
                    line.SetTextVariable("FROM", r.From.Name);
                    line.SetTextVariable("BUY", r.BuyPrice);
                    line.SetTextVariable("TO", r.To.Name);
                    line.SetTextVariable("SELL", r.SellPrice);
                    line.SetTextVariable("QTY", r.Quantity);
                    line.SetTextVariable("PROFIT", r.TotalProfit);
                    line.SetTextVariable("DAYS", r.TravelDays.ToString("0.#"));
                    sb.AppendLine(line.ToString());
                }
                if (Options.Current.ConservativeRouteProjection)
                {
                    sb.AppendLine();
                    sb.Append(Tongue.Text("{=TL49}Profit already has the resale safety factor applied, so it is lower than the prices above suggest.").ToString());
                }
                body = sb.ToString();
            }
            InformationManager.ShowInquiry(new InquiryData(
                Tongue.Text("{=TL07}TradeLord ledger").ToString(), body,
                true, false, Tongue.Text("{=TL09}Close").ToString(), "", null, null));
        }


        private static void AwardTradeXpForOurOwnTrade(int profit, bool muted)
        {
            int xp = (int)(profit * Options.Current.TradeXpMultiplier);
            if (xp <= 0) return;
            _pendingXp += xp;
            _pendingProfit += profit;
            if (!muted) _pendingXpMuted = false;
            Log.Write("trade profit fed to the XP system: " + xp + " denars");
        }
    }

    [HarmonyPatch(typeof(InformationManager), "DisplayMessage")]
    internal static class Patch_SilenceChunkedTradeLines
    {
        [HarmonyPriority(Priority.Last)]
        private static bool Prefix(InformationMessage __0)
        {
            if (!TradeActionBehavior.InGameTransaction) return true;
            TradeActionBehavior.NoteSilenced(__0?.Information);
            return false;
        }
    }
}
