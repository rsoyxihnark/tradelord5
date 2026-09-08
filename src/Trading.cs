using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
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

        internal static float Room(MobileParty party) => Capacity(party) - Carried(party);
    }

    public class TradeActionBehavior : CampaignBehaviorBase
    {
        private Settlement _trackedTown;
        private string _pinnedTowns = "";
        private bool _announcedAutomation;
        private static readonly Books Visit = new Books();
        private static bool _cargoWasFull;
        private static Block? _sellStalled;
        private static Block? _buyStalled;

        internal static bool AutomatedTradeInProgress { get; private set; }

        private static int _transactionDepth;
        private static int _silenced;

        internal static bool InGameTransaction => _transactionDepth > 0;

        private static void OpenTransaction() => _transactionDepth++;

        private static void CloseTransaction()
        {
            if (_transactionDepth > 0) _transactionDepth--;
        }

        private static bool SwapOneUnit(bool selling, Action swap, string what, string pass, out int gold)
        {
            int before = Hero.MainHero.Gold;
            OpenTransaction();
            try { swap(); }
            finally { CloseTransaction(); }
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
            finally { AutomatedTradeInProgress = false; _transactionDepth = 0; ReportSilenced(); }
        }

        internal static void ReleaseMessageFilter()
        {
            if (_transactionDepth == 0) return;
            _transactionDepth = 0;
            Log.Write("ERROR: the message filter was still armed at the start of a frame - forced open. " +
                      "A transaction did not unwind; no message is suppressed beyond this frame.");
            ReportSilenced();
        }

        internal static void NoteSilenced() => _silenced++;

        private static void ReportSilenced()
        {
            if (_silenced == 0) return;
            Log.Write("  silenced " + _silenced + " message(s) raised inside the game's own transaction");
            _silenced = 0;
        }

        internal static void ForgetVisit()
        {
            ResetVisit();
            _transactionDepth = 0;
            _silenced = 0;
            _pending.Clear();
            _pendingAfterXp.Clear();
            _pendingXp = 0;
            _pendingXpMuted = true;
            AutomatedTradeInProgress = false;
            _sittingAt = null;
            _sittingHour = -1;
            _markerHour = -1;
            _herdLookupFailed = false;
            Carry.Forget();
            TradePolicy.ForgetItemListAudit();
            TradePolicy.ForgetCraftingLookup();
            Errands.Forget();
            ForgetRoadMarket();
            _tradedWith = null;
            _tradedIn = null;
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (!dataStore.IsLoading)
                Guard.Run("Visit.PinsForSave", () => _pinnedTowns = LedgerPanel.PinnedIds());
            dataStore.SyncData("TradeLord_TrackedTown", ref _trackedTown);
            dataStore.SyncData("TradeLord_PanelPins", ref _pinnedTowns);
            dataStore.SyncData("TradeLord_AutomationNotice", ref _announcedAutomation);
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

        private static int _markerHour = -1;
        private static Vec2 _markerAt;
        private const float MarkerMovedFar = 100f;

        private void OnTick(float dt)
        {
            MobileParty party = MobileParty.MainParty;
            if (party == null || party.CurrentSettlement != null) return;
            int hour = (int)CampaignTime.Now.ToHours;
            Vec2 at = party.GetPosition2D;
            if (hour == _markerHour && at.DistanceSquared(_markerAt) <= MarkerMovedFar) return;
            _markerHour = hour;
            _markerAt = at;
            Guard.Run("Action.MarkerAsYouMove", UpdateBestSellTownTracker);
        }

        private void OnDailyTick()
        {
            Guard.Run("Action.DailyHerdCheck", () =>
            {
                int shed = DrivenAnimalsToShed(MobileParty.MainParty);
                if (shed > 0) LogHerdState("on the road, no market in reach", shed);
            });
            Guard.Run("Action.DailyTick", UpdateBestSellTownTracker);
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (party != MobileParty.MainParty) return;
            Guard.Run("Action.HerdReliefOnLeaving", () =>
            {
                LogHerdState("leaving " + settlement.Name);
                if (!_visitTradeAllowed)
                {
                    if (IsMarket(settlement))
                        Log.Write("herd relief on the way out is skipped: the game would not let you trade at " +
                                  settlement.Name + " when you arrived");
                    return;
                }
                if (Options.Current.AutoSellOnEntry) ExecuteHerdRelief(settlement, quiet: true);
            });
            Guard.Run("Action.OnSettlementLeft", UpdateBestSellTownTracker);
        }

        private static bool _visitTradeAllowed;

        private static string _sittingAt;
        private static int _sittingHour = -1;

        private static bool StillTheSameSitting(Settlement settlement)
        {
            int hour = (int)CampaignTime.Now.ToHours;
            bool same = settlement != null && settlement.StringId == _sittingAt && hour == _sittingHour;
            _sittingAt = settlement?.StringId;
            _sittingHour = hour;
            return same;
        }

        private static void ResetVisit(bool sameSitting = false)
        {
            _visitTradeAllowed = false;
            if (sameSitting) Visit.ForgetTheDryRun(); else Visit.Forget();
            _cargoWasFull = false;
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

            internal bool DirectionError;

            private ISet<string> _locked;
            private bool _lockedRead;
            private float _capacity = -1f;
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
                Party = party;
                Me = party.Party;
                Sim = Options.Current.SimulationMode;
                Quiet = quiet;
            }

            internal static Pass Open(Settlement site, bool quiet)
            {
                if (!MarketOpen(site, quiet)) return null;
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

            internal bool Muted => TradeActionBehavior.Muted(Quiet);

            internal bool Reports => Site != null;

            internal string Key => Site != null ? Site.StringId : Met.StringId;

            internal string Where => Site != null ? "at " + Site.Name : "from " + Met.Name;

            internal string Headed(string label) => label + (Sim ? " (simulated, best case): " : ": ");

            internal ItemRoster Stock => Site != null ? Site.ItemRoster : Met.ItemRoster;

            internal int TillNow => Site != null ? Market.Gold : Met.PartyTradeGold;

            internal int Till => TillNow - Books.TillDrawn(Sim);

            internal int Spendable() => TradeActionBehavior.Spendable(Books, Sim);

            internal float Capacity => _capacity < 0f ? _capacity = Carry.Capacity(Party) : _capacity;

            internal float Room() => Capacity - Carry.Carried(Party);

            internal void CountFrom() => _goldBefore = Hero.MainHero.Gold;

            internal int Gained(int simGold) => GoldGained(Sim, simGold, _goldBefore);

            internal int Spent(int simSpent) => GoldSpent(Sim, simSpent, _goldBefore);

            internal int Price(EquipmentElement what, bool selling) =>
                Site != null ? Market.GetItemPrice(what, Party, selling)
                             : Road.GetPrice(what, Party, selling, Shop);

            internal void Tally(ItemObject item, int count, int gold) =>
                TradeActionBehavior.Tally(Detail, item, count, gold);

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
                if (SwapOneUnit(selling, swap, what, named, out gold)) return true;
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

            internal void Moved(int? profit = null)
            {
                if (Sim) return;
                if (profit.HasValue) LedgerBehavior.Instance?.AddProfit(profit.Value);
                CoinSound();
                if (Site == null) return;
                LedgerBehavior.Instance?.CaptureSettlement(Site, force: true, KindsMoved());
                Guard.Run("Pass.PinCleared", () => LedgerPanel.Unpin(Site));
            }

            internal void Capture()
            {
                if (Site != null) LedgerBehavior.Instance?.CaptureSettlement(Site);
            }

            internal TextObject Said(string simSaid, string realSaid, int items, int gold) =>
                PassMessage(Sim, simSaid, realSaid, Detail, items, gold);

            internal void Logged(bool selling, string why) => LogDetail(selling, Sim, Detail, why);
        }

        private static void WarnUnmatchedItemLists()
        {
            TradePolicy.ItemListsNameTwoAnimals();
            if (!TradePolicy.ItemListsNameNothing()) return;
            Toast(Tongue.Text("{=TL91}An entry on one of your TradeLord item lists matches no good in this game and is doing nothing. TradeLord.log names which."), ToastAlert);
        }

        internal static int GoldHeldBack()
        {
            int wage = 0;
            try { wage = MobileParty.MainParty?.TotalWage ?? 0; }
            catch (Exception e) { Log.Error(e, "wage cover (the flat reserve still holds)"); }
            return TradeMath.Reserve(Options.Current.GoldReserve, Options.Current.KeepWageDays, wage);
        }

        internal static int Spendable(Books books, bool sim) =>
            TradeMath.Budget(Hero.MainHero.Gold + books.Purse(sim), GoldHeldBack(),
                             Options.Current.MaxSpendPerVisit, books.PaidOut(sim));

        internal static int PurseForAVisit() =>
            TradeMath.Budget(Hero.MainHero.Gold, GoldHeldBack(),
                             Options.Current.MaxSpendPerVisit, 0);

        private static int GoldGained(bool sim, int simGold, int goldBefore) =>
            sim ? simGold : Hero.MainHero.Gold - goldBefore;

        private static int GoldSpent(bool sim, int simSpent, int goldBefore) =>
            sim ? simSpent : goldBefore - Hero.MainHero.Gold;

        private static bool WarnPurseBelowReserve()
        {
            if (TradedThisVisit()) return false;
            if (Spendable(Visit, Simulating) > 0) return false;
            TextObject msg = Tongue.Text("{=TL92}Your purse is at {GOLD} denars and your gold reserve is {RESERVE}, so TradeLord will not buy anything here. Sell some cargo, or lower the reserve in its settings.");
            msg.SetTextVariable("GOLD", Hero.MainHero.Gold);
            msg.SetTextVariable("RESERVE", GoldHeldBack());
            Toast(msg, ToastAlert);
            return true;
        }

        private static void WarnNoRoomToCarry()
        {
            if (TradedThisVisit()) return;
            if (!NoRoomToCarry() && !_cargoWasFull) return;
            Toast(Tongue.Text("{=TL82}Cargo is full. Recruit more men, buy more horses, or sell goods manually."),
                  ToastAlert);
        }

        private bool AnnounceAutomation(Settlement settlement)
        {
            if (_announcedAutomation) return false;
            if (!Options.Current.AutoSellOnEntry && !Options.Current.AutoBuyOnEntry) return false;
            if (!CanTradeHere(settlement)) return false;
            _announcedAutomation = true;
            Toast(McmLoader.SettingsReachable
                ? Tongue.Text("{=TL87}TradeLord buys and sells for you as you enter a market, starting at the next one. Turn auto-sell and auto-buy on entry off in its settings to trade by hand.")
                : Tongue.Text("{=TL96}TradeLord buys and sells for you as you enter a market, starting at the next one. MCM is not installed, so its settings live in TradeLord.ini, beside TradeLord.log in your Bannerlord folder in Documents."), ToastAlert);
            Log.Write("automation notice shown - this market is left alone so the campaign can turn it off first"
                      + (McmLoader.SettingsReachable ? "" : "; MCM is absent, so the notice names the settings file instead"));
            return true;
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            Guard.Run("Action.OnSessionLaunched", () =>
            {
                ResetVisit();
                Settlement inside = MobileParty.MainParty?.CurrentSettlement;
                if (inside != null) _visitTradeAllowed = CanTradeHere(inside);
                Guard.Run("Action.RestorePins", () => LedgerPanel.RestorePins(_pinnedTowns));
                Guard.Run("Action.RestoreMarker", UpdateBestSellTownTracker);
                Log.Write(Travel.NavalActive
                    ? "naval capability: party can sail - routes and travel times include sea legs"
                    : "naval capability: land-only - land routing in effect");

                void AddOptions(string menu) => Guard.Run(
                    "menu " + menu + " (the other menus are unaffected)", () =>
                {
                    starter.AddGameMenuOption(menu, "tradelord_quicktrade",
                        Tongue.Text("{=TL26}Trade here now (TradeLord)").ToString(),
                        args =>
                        {
                            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                            args.Text = Tongue.Text("{=TL26}Trade here now (TradeLord)");
                            return Options.Current.QuickSellMenu && CanTradeHere(Settlement.CurrentSettlement);
                        },
                        args => Guard.Run("Action.QuickTradeMenu", () =>
                        {
                            LogHerdState("trading by hand at " + Settlement.CurrentSettlement.Name);
                            ExecuteQuickSell(Settlement.CurrentSettlement);
                            ExecuteHerdRelief(Settlement.CurrentSettlement);
                            ExecuteResupply(Settlement.CurrentSettlement);
                            ExecuteHaulage(Settlement.CurrentSettlement);
                            ExecuteQuickBuy(Settlement.CurrentSettlement);
                            ExecuteHerdRelief(Settlement.CurrentSettlement);
                            LogHerdState("after trading by hand at " + Settlement.CurrentSettlement.Name);
                            ReportStalledPasses();
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

                AddCaravanLines(starter);
                AddBanditLines(starter);
            });
        }

        private static MobileParty _tradedWith;
        private static object _tradedIn;

        private void AddCaravanLines(CampaignGameStarter starter) => Guard.Run(
            "caravan dialog (trading in a market is unaffected)", () =>
            {
                starter.AddPlayerLine("tradelord_caravan_done", "caravan_talk", "tradelord_caravan_reply",
                    Tongue.Text("{=TL114}That was a nice trade. [TRADELORD]").ToString(),
                    CaravanMet, null, 200);
                starter.AddDialogLine("tradelord_caravan_reply", "tradelord_caravan_reply", "close_window",
                    Tongue.Text("{=TL115}Agreed. I wish I could use that mod too. Hope you gave a thumbs up endorsement on NexusMods!").ToString(),
                    null, null, 200);
            });

        private static bool CaravanMet()
        {
            MobileParty caravan = MobileParty.ConversationParty;
            if (!Options.Current.TradeWithCaravans || caravan == null || !caravan.IsCaravan) return false;
            TradeOnce(caravan);
            return true;
        }

        private void AddBanditLines(CampaignGameStarter starter) => Guard.Run(
            "bandit dialog (meeting a band is otherwise unaffected)", () =>
            {
                ConversationSentence asked = starter.AddPlayerLine(
                    "tradelord_bandit_pass", Parley.OwnState, "tradelord_bandit_pass_reply",
                    Tongue.Text("{=TL387}Let us pass, and we will be on our way. [TRADELORD]").ToString(),
                    BanditMet, null, 200);
                starter.AddDialogLine("tradelord_bandit_pass_reply", "tradelord_bandit_pass_reply", "close_window",
                    Tongue.Text("{=TL113}Oh, sorry, of course. But do not forget to leave an endorsement thumbs up on NexusMods!").ToString(),
                    null, () => Guard.Run("Action.Getaway", LetPlayerGo), 200);
                Parley.Remember(asked);
            });

        private static bool BanditMet()
        {
            MobileParty band = MobileParty.ConversationParty;
            return Options.Current.BanditGetawayCheat && band != null && band.IsBandit;
        }

        private static void TradeOnce(MobileParty met)
        {
            object here = PlayerEncounter.Current;
            if (_tradedWith == met || (here != null && _tradedIn == here)) return;
            _tradedWith = met;
            _tradedIn = here;
            Guard.Run("Action.RoadTrade", () => ExecuteRoadTrade(met));
        }

        internal static bool IsRoadTrader(MobileParty party) =>
            party != null && (party.IsCaravan || party.IsVillager);

        private static object _handledEncounter;

        internal static void WatchEncounter()
        {
            if (Campaign.Current == null) { _handledEncounter = null; Parley.Forget(); return; }
            object here = PlayerEncounter.Current;
            if (here == null) { _handledEncounter = null; return; }
            if (_handledEncounter == here) return;
            MobileParty met = PlayerEncounter.EncounteredMobileParty;
            if (met == null) return;
            _handledEncounter = here;
            if (IsRoadTrader(met)) TradeOnce(met);
        }

        internal static void ForgetEncounter()
        {
            _tradedWith = null;
            _tradedIn = null;
            _handledEncounter = null;
        }

        private void OnConversationEnded(IEnumerable<CharacterObject> spoke) => _tradedWith = null;

        private static void LetPlayerGo()
        {
            MobileParty band = MobileParty.ConversationParty;
            Log.Write("free passage taken against " + (band == null ? "an unnamed party" : band.StringId));
            band?.IgnoreForHours(GetawayHours);
            MobileParty.MainParty?.IgnoreByOtherPartiesTill(CampaignTime.HoursFromNow(GetawayHours));
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.ProtectPlayerSide(GetawayHours);
                PlayerEncounter.LeaveEncounter = true;
            }
            Log.Write("free passage held for " + GetawayHours + " hours: your party is passed over by other parties, " +
                      "and " + (band == null ? "that band" : band.StringId) + " is passed over by yours");
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party != MobileParty.MainParty) return;
            Guard.Run("Action.OnSettlementEntered", () =>
            {
                ResetVisit(StillTheSameSitting(settlement));
                _visitTradeAllowed = CanTradeHere(settlement);
                WarnUnmatchedItemLists();
                LogHerdState("entering " + settlement.Name);

                if (!AnnounceAutomation(settlement))
                {
                    if (Options.Current.AutoSellOnEntry) ExecuteQuickSell(settlement, quiet: true);
                    if (Options.Current.AutoSellOnEntry) ExecuteHerdRelief(settlement, quiet: true);
                    if (Options.Current.AutoBuyOnEntry) ExecuteResupply(settlement, quiet: true);
                    if (Options.Current.AutoBuyOnEntry) ExecuteHaulage(settlement, quiet: true);
                    if (Options.Current.AutoBuyOnEntry) ExecuteQuickBuy(settlement, quiet: true);
                    if (Options.Current.AutoSellOnEntry) ExecuteHerdRelief(settlement, quiet: true);
                    LogHerdState("after trading at " + settlement.Name);
                    ReportStalledPasses();
                }
                if (CanTradeHere(settlement) &&
                    (Options.Current.AutoBuyOnEntry || Options.Current.QuickSellMenu))
                {
                    if (!WarnPurseBelowReserve()) WarnNoRoomToCarry();
                }
                UpdateBestSellTownTracker();
            });
        }

        private static bool NavalModulePresent()
        {
            try { return TaleWorlds.ModuleManager.ModuleHelper.GetModuleInfo("NavalDLC") != null; }
            catch { return false; }
        }

        internal static bool IsMarket(Settlement s) =>
            s != null && (s.IsTown || (s.IsVillage && Options.Current.TradeWithVillages));

        private static bool GameAllowsTrade(Settlement s)
        {
            if (s != Settlement.CurrentSettlement) return true;
            try
            {
                var model = Campaign.Current?.Models?.SettlementAccessModel;
                return model == null || model.CanMainHeroDoSettlementAction(
                    s, SettlementAccessModel.SettlementAction.Trade, out _, out _);
            }
            catch { return true; }
        }

        private static bool CanTradeHere(Settlement s) =>
            IsMarket(s) && !LedgerBehavior.VillageShut(s) && GameAllowsTrade(s) &&
            !(Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(s));

        private static bool StillSettling(bool quiet)
        {
            int wait = Options.Current.EconomySettlingDays;
            if (wait <= 0) return false;
            float elapsed = Campaign.Current.Models.CampaignTimeModel.CampaignStartTime.ElapsedDaysUntilNow;
            if (elapsed >= wait) return false;
            if (!quiet)
            {
                TextObject msg = Tongue.Text("{=TL18}The market is still settling ({DAYS} more days).");
                msg.SetTextVariable("DAYS", (int)Math.Ceiling(wait - elapsed));
                Toast(msg);
            }
            return true;
        }

        private static bool MarketOpen(Settlement settlement, bool quiet) =>
            CanTradeHere(settlement) && !StillSettling(quiet);

        private static readonly Color ToastGain = new Color(0.40f, 0.90f, 0.40f);
        private static readonly Color ToastSpend = new Color(0.55f, 0.78f, 1f);
        private static readonly Color ToastFlat = new Color(0.85f, 0.75f, 0.45f);
        private static readonly Color ToastNote = new Color(0.75f, 0.75f, 0.75f);
        private static readonly Color ToastXp = new Color(1f, 0.72f, 0.20f);
        private static readonly Color ToastAlert = new Color(0.90f, 0.28f, 0.28f);

        private static readonly List<InformationMessage> _pending = new List<InformationMessage>();
        private static readonly List<InformationMessage> _pendingAfterXp = new List<InformationMessage>();
        private static int _pendingXp;
        private static bool _pendingXpMuted = true;

        private static void Toast(TextObject msg) => Toast(msg, ToastNote);

        private static void Toast(TextObject msg, Color color) =>
            _pending.Add(new InformationMessage(msg.ToString(), color));

        private static void ToastAfterXp(TextObject msg, Color color) =>
            _pendingAfterXp.Add(new InformationMessage(msg.ToString(), color));

        internal static void FlushToasts()
        {
            int xp = _pendingXp;
            bool muted = _pendingXpMuted;
            _pendingXp = 0;
            _pendingXpMuted = true;
            if (xp > 0) CreditTradeSkill(xp, muted);
            if (_pendingAfterXp.Count > 0)
            {
                _pending.AddRange(_pendingAfterXp);
                _pendingAfterXp.Clear();
            }
            if (_pending.Count > 0)
            {
                try
                {
                    for (int i = 0; i < _pending.Count; i++)
                        if (i == 0 || _pending[i].Information != _pending[i - 1].Information)
                            InformationManager.DisplayMessage(_pending[i]);
                }
                finally { _pending.Clear(); }
            }
        }

        private static void CreditTradeSkill(int xp, bool muted)
        {
            if (Campaign.Current == null || Hero.MainHero == null) return;
            int before = Hero.MainHero.GetSkillValue(DefaultSkills.Trade);
            OpenTransaction();
            try { SkillLevelingManager.OnTradeProfitMade(Hero.MainHero, xp); }
            finally { CloseTransaction(); ReportSilenced(); }
            int now = Hero.MainHero.GetSkillValue(DefaultSkills.Trade);
            bool rose = now > before;
            TextObject earned = Tongue.Text(rose
                ? "{=TL88}TradeLord credited {GOLD} denars of profit to your Trade skill, which is now {LEVEL}."
                : "{=TL81}TradeLord credited {GOLD} denars of profit to your Trade skill.");
            earned.SetTextVariable("GOLD", xp);
            if (rose) earned.SetTextVariable("LEVEL", now);
            if (!muted) Toast(earned, ToastXp);
            if (rose) Log.Write("trade skill rose to " + now + " - named in TradeLord's own line");
        }

        private const int NamedItemCap = 6;

        private const float GetawayHours = 4f;

        private static void LogDetail(bool selling, bool sim, Dictionary<ItemObject, (int count, int gold)> detail,
                                      string why)
        {
            foreach (var kv in detail)
            {
                Log.Write((selling ? "  sold " : "  bought ") + kv.Value.count + " " +
                          kv.Key.StringId + " for " + kv.Value.gold + (sim ? " (simulated)" : ""));
                LogAnimalMoved(selling, sim, kv.Key, kv.Value.count, kv.Value.gold, why);
            }
        }

        private static void LogAnimalMoved(bool selling, bool sim, ItemObject item, int count, int gold, string why)
        {
            if (item == null || !item.HasHorseComponent) return;
            Log.Write("  animal " + (selling ? "out: " : "in: ") + item.StringId +
                      " (" + (item.Name == null ? item.StringId : item.Name.ToString()) + ") x" + count +
                      (selling ? " for +" : " for -") + gold + " gold" + (sim ? " (simulated)" : "") +
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

        private static MethodInfo _herdModifier;
        private static bool _herdLookupFailed;
        private const int HerdCushion = 2;

        private static bool HerdTally(MobileParty party, out int men, out int herd,
                                     out int mounts, out int foot)
        {
            men = 0; herd = 0; mounts = 0; foot = 0;
            ItemRoster roster = party?.ItemRoster;
            if (roster == null) return false;
            men = party.MemberRoster?.TotalManCount ?? 0;
            herd = roster.NumberOfPackAnimals + roster.NumberOfLivestockAnimals;
            mounts = roster.NumberOfMounts;
            foot = party.Party?.NumberOfMenWithoutHorse ?? 0;
            var attached = party.AttachedParties;
            for (int i = 0; attached != null && i < attached.Count; i++)
            {
                MobileParty a = attached[i];
                if (a?.ItemRoster == null) continue;
                herd += a.ItemRoster.NumberOfPackAnimals + a.ItemRoster.NumberOfLivestockAnimals;
                mounts += a.ItemRoster.NumberOfMounts;
                men += a.MemberRoster?.TotalManCount ?? 0;
                foot += a.Party?.NumberOfMenWithoutHorse ?? 0;
            }
            return men > 0;
        }

        internal static int HerdRoomForLivestock(MobileParty party)
        {
            try
            {
                if (party == null) return 0;
                DefaultPartySpeedCalculatingModel model = HerdModel();
                if (model == null) return 0;
                if (!HerdTally(party, out int men, out int herd, out int mounts, out int foot)) return 0;
                herd += Math.Max(0, mounts - foot);
                float neutral = (float)_herdModifier.Invoke(model, new object[] { men, 0 });
                return TradeMath.MostThatHolds(256, room => room == 0 || TradeMath.Unchanged(
                    (float)_herdModifier.Invoke(model,
                        new object[] { men, herd + room + HerdCushion }), neutral));
            }
            catch (Exception e)
            {
                if (!_herdLookupFailed) { _herdLookupFailed = true; Log.Error(e, "herd guard (livestock buying disabled, selling unaffected)"); }
                return 0;
            }
        }

        private static DefaultPartySpeedCalculatingModel HerdModel()
        {
            if (_herdLookupFailed) return null;
            var model = Campaign.Current?.Models?.PartySpeedCalculatingModel
                as DefaultPartySpeedCalculatingModel;
            if (model == null)
            {
                _herdLookupFailed = true;
                Log.Write("herd guard: a mod replaced the party speed model - livestock buying disabled, selling unaffected");
                return null;
            }
            if (_herdModifier == null)
            {
                _herdModifier = typeof(DefaultPartySpeedCalculatingModel).GetMethod(
                    "GetHerdingModifier", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_herdModifier == null)
                {
                    _herdLookupFailed = true;
                    Log.Write("herd guard: GetHerdingModifier not found on this game version - livestock buying disabled, selling unaffected");
                    return null;
                }
            }
            return model;
        }

        internal static int DrivenAnimalsToShed(MobileParty party)
        {
            try
            {
                if (party == null) return 0;
                DefaultPartySpeedCalculatingModel model = HerdModel();
                if (model == null) return 0;
                if (!HerdTally(party, out int men, out int herd, out int mounts, out int foot)) return 0;
                int spare = Math.Max(0, mounts - foot);
                int driven = herd + spare;
                if (driven <= 0) return 0;
                float neutral = (float)_herdModifier.Invoke(model, new object[] { men, 0 });
                return TradeMath.MostThatHolds(driven, shed => shed == 0 || !TradeMath.Unchanged(
                    (float)_herdModifier.Invoke(model, new object[] { men, driven - shed + 1 }), neutral));
            }
            catch (Exception e)
            {
                if (!_herdLookupFailed)
                {
                    _herdLookupFailed = true;
                    Log.Error(e, "herd relief check (no animal is sold)");
                }
                return 0;
            }
        }

        private static void HerdSplit(MobileParty party, out int packs, out int stock)
        {
            packs = 0; stock = 0;
            ItemRoster roster = party?.ItemRoster;
            if (roster == null) return;
            packs = roster.NumberOfPackAnimals;
            stock = roster.NumberOfLivestockAnimals;
            var attached = party.AttachedParties;
            for (int i = 0; attached != null && i < attached.Count; i++)
            {
                ItemRoster other = attached[i]?.ItemRoster;
                if (other == null) continue;
                packs += other.NumberOfPackAnimals;
                stock += other.NumberOfLivestockAnimals;
            }
        }

        internal static void LogHerdState(string when) => LogHerdState(when, -1);

        internal static void LogHerdState(string when, int counted)
        {
            try
            {
                MobileParty party = MobileParty.MainParty;
                if (party == null) return;
                if (!HerdTally(party, out int men, out int herd, out int mounts, out int foot)) return;
                HerdSplit(party, out int packs, out int stock);
                int spare = Math.Max(0, mounts - foot);
                int shed = counted >= 0 ? counted : DrivenAnimalsToShed(party);
                Log.Write("herd check (" + when + "): " + men + " men of whom " + foot + " on foot, " +
                          mounts + " loose mount(s) with " + spare + " nobody rides, " +
                          packs + " pack animal(s), " + stock + " livestock, " +
                          (herd + spare) + " driven in all, " +
                          (shed > 0
                              ? "the herd is slowing you down and " + shed + " must go"
                              : "no herd penalty"));
            }
            catch (Exception e) { Log.Error(e, "herd check log (nothing else is affected)"); }
        }

        internal static int SpareMountRoom(MobileParty party)
        {
            try
            {
                return HerdTally(party, out _, out _, out int mounts, out int foot)
                    ? Math.Max(0, mounts - foot) : 0;
            }
            catch (Exception e)
            {
                Log.Error(e, "spare mount count (no mount is sold to relieve the herd)");
                return 0;
            }
        }

        internal static int HaulAnimalsHeld(MobileParty party)
        {
            ItemRoster roster = party?.ItemRoster;
            if (roster == null) return 0;
            int held = 0;
            for (int i = 0; i < roster.Count; i++)
            {
                ItemRosterElement el = roster.GetElementCopyAtIndex(i);
                if (el.Amount > 0 && TradePolicy.IsHaulAnimal(el.EquipmentElement.Item)) held += el.Amount;
            }
            return held;
        }

        internal static int HaulAnimalsCargoCanSpare(MobileParty party)
        {
            int held = HaulAnimalsHeld(party);
            if (held <= 0) return 0;
            try
            {
                InventoryCapacityModel model = Campaign.Current?.Models?.InventoryCapacityModel;
                if (model == null) return 0;
                bool atSea = Carry.Sailing();
                float carried = model.CalculateTotalWeightCarried(party, atSea).ResultNumber;
                return TradeMath.MostThatHolds(held, fewer => fewer == 0 ||
                    model.CalculateInventoryCapacity(party, atSea, false, 0, 0, -fewer).ResultNumber >= carried);
            }
            catch (Exception e)
            {
                Log.Error(e, "haul animal cargo floor (every haul animal is kept)");
                return 0;
            }
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
            Toast(none);
        }

        public static void ExecuteQuickSell(Settlement settlement, bool quiet = false) =>
            SellPass(Pass.Open(settlement, quiet), "quick-sell", "selling", "Selling", "the selling pass");

        private static void SellPass(Pass pass, string label, string what, string named, string why)
        {
            if (pass == null) return;

            ItemRoster roster = pass.Party.ItemRoster;

            pass.Capture();

            var plan = new List<ItemRosterElement>();
            var goods = new List<ItemObject>();
            for (int i = 0; i < roster.Count; i++)
            {
                ItemRosterElement held = roster.GetElementCopyAtIndex(i);
                plan.Add(held);
                goods.Add(held.EquipmentElement.Item);
            }
            LedgerBehavior.Instance?.PrimeMarketsFor(goods);
            var keepBack = TradePolicy.KeptBack(roster, out var awaited);

            pass.CountFrom();
            int soldItems = 0, profit = 0, simGold = 0, simTill = pass.Till;
            var tally = new BlockTally();

            InAPass(() =>
            {
                foreach (ItemRosterElement el in plan)
                {
                    if (pass.DirectionError) break;
                    ItemObject item = el.EquipmentElement.Item;
                    if (item != null && pass.Books.Bought(pass.Sim, item.StringId)) { tally.Note(Block.TradedHereAlready); continue; }
                    Good good = TradePolicy.Describe(item);
                    if (!TradePolicy.MaySell(good, el, pass.Locked, keepBack, awaited, out int keep, out Block stopped)) { tally.Note(stopped); continue; }

                    int remaining = el.Amount - keep + pass.Books.Held(pass.Sim, item.StringId);
                    if (remaining <= 0) { tally.Note(Block.TradedHereAlready); continue; }

                    Basis basis = Basis.For(item);

                    int bestMarketFloor = 0;
                    bool floorKnown = false;

                    while (remaining > 0)
                    {
                        int worth = basis.Unit(item);
                        int holdFloor = 0;
                        if (Options.Current.PreferBestSellTown)
                        {
                            if (!floorKnown)
                            {
                                floorKnown = true;
                                var best = LedgerBehavior.Instance?.BestSell(item) ?? (null, 0);
                                if (best.Item1 != null && best.Item1 != pass.Site)
                                    bestMarketFloor = (int)(best.Item2 * Options.Current.BestSellTownTolerance);
                            }
                            holdFloor = bestMarketFloor;
                        }
                        int price = pass.Price(el.EquipmentElement, selling: true);
                        if (price < holdFloor) { tally.Note(Block.BelowBestMarket); break; }
                        if (!TradePolicy.ProfitAcceptable(worth, price))
                        {
                            tally.Note(Block.BelowMargin);
                            if (!basis.SkipTheUnitsYouPaidFor(ref remaining)) break;
                            continue;
                        }
                        if ((pass.Sim ? simTill : pass.TillNow) < price) { tally.Note(Block.MerchantTillEmpty); break; }

                        if (pass.Sim)
                        {
                            simTill -= price;
                            simGold += price;
                            profit += TradePolicy.Credit(price, worth, basis.UnpaidWorth);
                            int herdRank = HerdShedRank(good);
                            pass.Books.NoteSale(item.StringId, price,
                                                herdRank == RankHaulAnimal ? 0f : item.Weight,
                                                TradePolicy.FoodValue(item));
                            if (herdRank >= 0)
                                pass.Books.NoteShed(herdRank == RankHaulAnimal, herdRank != RankLivestock);
                            soldItems++;
                            remaining--;
                            basis.SoldOne();
                            pass.Tally(item, 1, price);
                            continue;
                        }

                        if (!pass.SellOne(el, price, what, named, out int proceeds)) break;
                        if (proceeds == 0) break;

                        if (basis.SoldOne()) LedgerBehavior.Instance?.RecordSale(item.StringId, 1);
                        pass.Books.NoteSold(item.StringId);
                        soldItems++;
                        profit += TradePolicy.Credit(proceeds, worth, basis.UnpaidWorth);
                        remaining--;
                        pass.Tally(item, 1, proceeds);
                    }
                }
            });

            int goldGained = pass.Gained(simGold);

            if (soldItems > 0)
            {
                pass.Moved(profit);
                Log.Write(pass.Headed(label) + soldItems +
                          " items, +" + goldGained + " gold, profit " + profit + " " + pass.Where);
                pass.Logged(selling: true, why);
                if (tally.Any) Log.Write("  stopped on: " + tally.Summary());
                TextObject msg = pass.Said(
                    "{=TL13}[Simulated, best case] TradeLord would sell {ITEMS} for {GOLD} denars ({PROFIT} profit).",
                    "{=TL02}TradeLord sold {ITEMS} for {GOLD} denars ({PROFIT} profit).",
                    soldItems, goldGained);
                msg.SetTextVariable("PROFIT", profit);
                if (!pass.Muted) Toast(msg, profit > 0 ? ToastGain : ToastFlat);
                if (!pass.Sim && profit > 0) AwardTradeXp(profit, pass.Muted);
            }
            else if (!pass.DirectionError)
            {
                if (tally.Any) Log.Repeatable(label + "-empty " + pass.Key, tally.Summary(),
                    label + " moved nothing " + pass.Where + ": " + tally.Summary());
                Block stopped = tally.Dominant();
                if (stopped != Block.None && !pass.Muted) NoteStalled(selling: true, stopped);
            }
        }

        private struct Basis
        {
            internal int Paid;
            internal bool FromMarket;
            internal int PaidLeft;
            internal int UnpaidWorth;

            internal static Basis For(ItemObject item)
            {
                Basis basis;
                basis.Paid = TradePolicy.CostBasis(item);
                basis.FromMarket = Options.Current.CostBasisMode == 2;
                basis.PaidLeft = LedgerBehavior.Instance?.PurchasedUnits(item) ?? 0;
                basis.UnpaidWorth = -1;
                return basis;
            }

            internal int Unit(ItemObject item)
            {
                int worth = FromMarket || PaidLeft > 0 ? Paid : 0;
                if (worth == 0 && UnpaidWorth < 0) UnpaidWorth = TradePolicy.UnpaidWorth(item);
                return worth;
            }

            internal bool SoldOne()
            {
                if (PaidLeft <= 0) return false;
                PaidLeft--;
                return true;
            }

            internal bool SkipTheUnitsYouPaidFor(ref int remaining) =>
                TradeMath.SkipTheUnitsYouPaidFor(FromMarket, ref remaining, ref PaidLeft);
        }

        private static Block WhatStopsBuying(in Good good, int price, int budget,
                                             (int count, int spent) taken, int held, float shareCap,
                                             bool livestock, int herdRoom, bool lastInVillage) =>
            TradeRules.WhatStopsBuying(good, price, budget, taken, held, shareCap,
                                       livestock, herdRoom, lastInVillage, Options.Current);

        private static bool NoRoomForOneMore(in Good good, float roomLeft) =>
            TradeRules.NoRoomForOneMore(good, roomLeft);

        private static List<(ItemRosterElement el, Good good, int price, int worth)> CheapestFirst(
            Pass pass, Func<ItemObject, bool> wanted)
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
                if (el.Amount - pass.Books.Stocked(pass.Sim, it.StringId) <= 0) continue;
                int price = pass.Price(el.EquipmentElement, selling: false);
                if (price <= 0) continue;
                shelf.Add((el, price));
                goods.Add(it);
            }
            LedgerBehavior.Instance?.PrimeMarketsFor(goods);

            var found = new List<(ItemRosterElement el, Good good, int price, int worth)>();
            foreach (var (el, price) in shelf)
            {
                ItemObject it = el.EquipmentElement.Item;
                int worth = TradePolicy.UnpaidWorth(it);
                if (price > worth) continue;
                found.Add((el, TradePolicy.Describe(it), price, worth));
            }
            found.Sort((x, y) => x.price.CompareTo(y.price));
            return found;
        }

        public static void ExecuteResupply(Settlement settlement, bool quiet = false)
        {
            if (Options.Current.ResupplyFoodDays <= 0) return;
            Pass pass = Pass.Open(settlement, quiet);
            if (pass == null) return;

            int shortfall = TradePolicy.FoodWanted() -
                            TradePolicy.FoodHeld(pass.Party.ItemRoster) - pass.Books.FoodHeld(pass.Sim);
            if (shortfall <= 0) return;

            int stocked = 0, simSpent = 0;
            float simWeight = pass.Books.Weight(pass.Sim);

            var larder = CheapestFirst(pass,
                it => TradePolicy.IsStorableFood(it) && TradePolicy.MayBuy(it, pass.Locked, out _, toFeed: true));
            if (larder.Count == 0) return;

            pass.CountFrom();
            InAPass(() =>
            {
                foreach (var (el, good, _, worth) in larder)
                {
                    if (pass.DirectionError || shortfall <= 0) break;
                    ItemObject item = el.EquipmentElement.Item;
                    int fed = TradeRules.FoodValue(good);
                    if (fed <= 0) continue;
                    int remaining = el.Amount - pass.Books.Stocked(pass.Sim, item.StringId);

                    while (shortfall > 0 && remaining > 0)
                    {
                        int price = pass.Price(el.EquipmentElement, selling: false);
                        if (price <= 0 || price > worth) break;
                        if (price >= pass.Spendable()) break;
                        if (settlement.IsVillage && remaining <= 1) break;
                        if (item.Weight > 0.01f && item.Weight > pass.Room() - simWeight) break;

                        if (pass.Sim)
                        {
                            simSpent += price;
                            simWeight += item.Weight;
                            pass.Books.NotePurchase(item.StringId, price, good.Weight, fed);
                        }
                        else
                        {
                            if (!pass.BuyOne(el, price, "restocking", "Restocking", out price)) break;
                            if (price == 0) break;
                            LedgerBehavior.Instance?.RecordPurchase(item.StringId, 1, price);
                            pass.Books.NoteBought(item.StringId, price);
                        }
                        stocked++;
                        remaining--;
                        shortfall -= fed;
                        pass.Tally(item, 1, price);
                    }
                }
            });

            if (stocked <= 0) return;

            int spent = pass.Spent(simSpent);
            pass.Moved();
            Log.Write((pass.Sim ? "resupply (simulated, best case): " : "resupply: ") + stocked +
                      " items, -" + spent + " gold at " + settlement.Name +
                      ", still short " + (shortfall > 0 ? shortfall : 0) + " unit(s) of food");
            pass.Logged(selling: false, "restocking the larder");
            TextObject msg = pass.Said(
                "{=TL98}[Simulated, best case] TradeLord would restock {ITEMS} for {GOLD} denars.",
                "{=TL97}TradeLord restocked {ITEMS} for {GOLD} denars.",
                stocked, spent);
            if (!pass.Muted) Toast(msg, ToastSpend);
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
            if (!IsRoadTrader(met)) return false;
            if (!Options.Current.ExcludeHostileTowns) return true;
            IFaction mine = Hero.MainHero?.MapFaction;
            return mine == null || met.MapFaction == null ||
                   !FactionManager.IsAtWarAgainstFaction(met.MapFaction, mine);
        }

        public static void ExecuteRoadTrade(MobileParty met)
        {
            if (!Options.Current.TradeWithCaravans) return;
            if (!RoadPartyReachable(met)) return;
            if (StillSettling(quiet: false)) return;
            IMarketData road = RoadMarket();
            if (road == null) return;
            MobileParty party = MobileParty.MainParty;
            if (party == null) return;

            var books = new Books();
            string why = "trading with a party on the road";
            SellPass(Pass.Meet(met, road, books, party),
                     "sale on the road", "selling on the road", "Road trading", why);
            BuyPass(Pass.Meet(met, road, books, party),
                    "purchase on the road", "buying on the road", "Road buying", why);
            ReportStalledPasses();
        }

        private const int RankLivestock = TradeRules.RankLivestock;
        private const int RankHaulAnimal = TradeRules.RankHaulAnimal;

        private static int HerdShedRank(ItemObject item) =>
            item == null ? TradeRules.RankNotAnAnimal
                         : TradeRules.HerdShedRank(TradePolicy.Describe(item));

        private static int HerdShedRank(in Good good) => TradeRules.HerdShedRank(good);

        public static void ExecuteHerdRelief(Settlement settlement, bool quiet = false)
        {
            if (!Options.Current.SellSpareMounts) return;
            Pass pass = Pass.Open(settlement, quiet);
            if (pass == null) return;

            int shed = DrivenAnimalsToShed(pass.Party);
            shed -= pass.Books.Shed(pass.Sim);
            if (shed <= 0) return;

            Dictionary<ItemObject, int> promised = Errands.Promised();
            if (promised == null) return;

            int mountsLeft = SpareMountRoom(pass.Party);
            mountsLeft -= pass.Books.MountsShed(pass.Sim);
            int haulsLeft = -1;

            var stable = new List<(ItemRosterElement el, int rank, int price)>();
            ItemRoster mine = pass.Party.ItemRoster;
            for (int i = 0; i < mine.Count; i++)
            {
                ItemRosterElement el = mine.GetElementCopyAtIndex(i);
                ItemObject it = el.EquipmentElement.Item;
                if (el.Amount <= 0 || !TradePolicy.MayShedForHerd(el.EquipmentElement, pass.Locked)) continue;
                int rank = HerdShedRank(it);
                if (rank < 0) continue;
                if (el.Amount + pass.Books.Held(pass.Sim, it.StringId) <= 0) continue;
                int price = pass.Price(el.EquipmentElement, selling: true);
                if (price <= 0) continue;
                stable.Add((el, rank, price));
            }
            if (stable.Count == 0) return;
            stable.Sort((x, y) => x.rank != y.rank ? x.rank.CompareTo(y.rank) : x.price.CompareTo(y.price));

            int sold = 0, profit = 0, simGold = 0, simTill = pass.Till;

            pass.CountFrom();
            InAPass(() =>
            {
                foreach (var (el, rank, _) in stable)
                {
                    if (pass.DirectionError || shed <= 0) break;
                    ItemObject item = el.EquipmentElement.Item;
                    int remaining = el.Amount + pass.Books.Held(pass.Sim, item.StringId);
                    if (promised.TryGetValue(item, out int owed) && owed > 0)
                    {
                        int spare = Math.Min(remaining, owed);
                        promised[item] = owed - spare;
                        remaining -= spare;
                    }

                    Basis basis = Basis.For(item);

                    while (remaining > 0 && shed > 0)
                    {
                        if (rank != RankLivestock && rank != RankHaulAnimal && mountsLeft <= 0) break;
                        if (rank == RankHaulAnimal && haulsLeft < 0)
                            haulsLeft = Math.Max(0, HaulAnimalsCargoCanSpare(pass.Party) - pass.Books.HaulsShed(pass.Sim));
                        if (rank == RankHaulAnimal && haulsLeft <= 0) break;
                        int price = pass.Price(el.EquipmentElement, selling: true);
                        if (price <= 0) break;
                        if ((pass.Sim ? simTill : pass.Market.Gold) < price) break;
                        int worth = basis.Unit(item);

                        if (pass.Sim)
                        {
                            simTill -= price;
                            simGold += price;
                            pass.Books.NoteSale(item.StringId, price,
                                                rank == RankHaulAnimal ? 0f : item.Weight,
                                                TradePolicy.FoodValue(item));
                            pass.Books.NoteShed(rank == RankHaulAnimal, rank != RankLivestock);
                        }
                        else
                        {
                            if (!pass.SellOne(el, price, "selling an animal to relieve the herd", "Herd relief", out price)) break;
                            if (price == 0) break;
                            pass.Books.NoteSold(item.StringId);
                        }
                        if (basis.SoldOne() && !pass.Sim) LedgerBehavior.Instance?.RecordSale(item.StringId, 1);
                        profit += TradePolicy.Credit(price, worth, basis.UnpaidWorth);
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
            pass.Moved(profit);
            Log.Write((pass.Sim ? "herd relief (simulated, best case): " : "herd relief: ") + sold +
                      " sold, +" + gained + " gold, profit " + profit + " at " + settlement.Name);
            pass.Logged(selling: true, "herd relief, getting the party back up to speed");
            TextObject msg = pass.Said(
                "{=TL117}[Simulated, best case] TradeLord would sell {ITEMS} for {GOLD} denars to get your party back up to speed.",
                "{=TL116}TradeLord sold {ITEMS} for {GOLD} denars to get your party back up to speed.",
                sold, gained);
            if (!pass.Muted) Toast(msg, ToastGain);
            if (!pass.Sim && profit > 0) AwardTradeXp(profit, pass.Muted);
        }

        public static void ExecuteHaulage(Settlement settlement, bool quiet = false)
        {
            if (!Options.Current.BuyHaulAnimals) return;
            Pass pass = Pass.Open(settlement, quiet);
            if (pass == null) return;

            int herdRoom = HerdRoomForLivestock(pass.Party);
            herdRoom -= pass.Books.HerdTaken(pass.Sim);
            if (herdRoom <= 0) return;

            int hauled = 0, simSpent = 0;

            var stable = CheapestFirst(pass, it => TradePolicy.MayHaul(it, pass.Locked));
            if (stable.Count == 0) return;

            pass.CountFrom();
            InAPass(() =>
            {
                foreach (var (el, good, _, worth) in stable)
                {
                    if (pass.DirectionError) break;
                    ItemObject item = el.EquipmentElement.Item;
                    int remaining = el.Amount - pass.Books.Stocked(pass.Sim, item.StringId);

                    while (remaining > 0 && herdRoom > 0)
                    {
                        int price = pass.Price(el.EquipmentElement, selling: false);
                        if (price <= 0 || price > worth) break;
                        if (price >= pass.Spendable()) break;
                        if (settlement.IsVillage && remaining <= 1) break;

                        if (pass.Sim)
                        {
                            simSpent += price;
                            pass.Books.NotePurchase(item.StringId, price, 0f, TradeRules.FoodValue(good));
                            pass.Books.NoteHerdTaken();
                        }
                        else
                        {
                            if (!pass.BuyOne(el, price, "buying a haul animal", "Haul animal buying", out price)) break;
                            if (price == 0) break;
                            LedgerBehavior.Instance?.RecordPurchase(item.StringId, 1, price);
                            pass.Books.NoteBought(item.StringId, price);
                        }
                        hauled++;
                        remaining--;
                        herdRoom--;
                        pass.Tally(item, 1, price);
                    }
                }
            });

            if (hauled <= 0) return;

            int spent = pass.Spent(simSpent);
            pass.Moved();
            Log.Write((pass.Sim ? "haul animals (simulated, best case): " : "haul animals: ") + hauled +
                      " bought, -" + spent + " gold at " + settlement.Name);
            pass.Logged(selling: false, "stocking the baggage train");
            TextObject msg = pass.Said(
                "{=TL111}[Simulated, best case] TradeLord would buy {ITEMS} for {GOLD} denars to carry more.",
                "{=TL110}TradeLord bought {ITEMS} for {GOLD} denars to carry more.",
                hauled, spent);
            if (!pass.Muted) ToastAfterXp(msg, ToastSpend);
        }

        public static void ExecuteQuickBuy(Settlement settlement, bool quiet = false) =>
            BuyPass(Pass.Open(settlement, quiet), "quick-buy", "buying", "Buying", "the buying pass");

        private static void BuyPass(Pass pass, string label, string what, string named, string why)
        {
            if (pass == null) return;

            pass.Capture();

            pass.CountFrom();
            int bought = 0, simSpent = 0;
            float simWeight = pass.Books.Weight(pass.Sim);
            var tally = new BlockTally();

            float shareCap = Options.Current.MaxHeldShare > 0f
                ? pass.Capacity * Options.Current.MaxHeldShare : 0f;

            var stock = new List<(ItemRosterElement el, Good good, float realizable, float margin)>();
            if (pass.Spendable() > 0)
            {
                ItemRoster shopRoster = pass.Stock;
                ItemRoster mine = pass.Party.ItemRoster;
                int holdCap = Options.Current.MaxHeldPerItem;
                var shelf = new List<(ItemRosterElement el, Good good)>();
                var goods = new List<ItemObject>();
                for (int i = 0; i < shopRoster.Count; i++)
                {
                    ItemRosterElement el = shopRoster.GetElementCopyAtIndex(i);
                    ItemObject it = el.EquipmentElement.Item;
                    if (el.Amount <= 0) { tally.Note(Block.NoStock); continue; }
                    Good good = TradePolicy.Describe(it);
                    if (!TradePolicy.MayBuy(good, it, pass.Locked, out Block whyBuy)) { tally.Note(whyBuy); continue; }
                    if (!TradeRules.ResaleAllowed(good, Options.Current)) { tally.Note(Block.CategoryPolicy); continue; }
                    if (pass.Books.Sold(pass.Sim, it.StringId)) { tally.Note(Block.TradedHereAlready); continue; }
                    if (el.Amount - pass.Books.Stocked(pass.Sim, it.StringId) <= 0) { tally.Note(Block.NoStock); continue; }
                    int held = mine.GetItemNumber(it) + pass.Books.Held(pass.Sim, it.StringId);
                    if (holdCap > 0 && held >= holdCap) { tally.Note(Block.HeldEnough); continue; }
                    if (shareCap > 0f && (held + 1) * good.Weight > shareCap) { tally.Note(Block.HeldEnough); continue; }
                    shelf.Add((el, good));
                    goods.Add(it);
                }
                LedgerBehavior.Instance?.PrimeMarketsFor(goods);
                foreach (var (el, good) in shelf)
                {
                    ItemObject it = el.EquipmentElement.Item;
                    var elsewhere = LedgerBehavior.Instance?.BestSell(it) ?? (null, 0);
                    if (elsewhere.Item1 == null || elsewhere.Item1 == pass.Site) { tally.Note(Block.NoResaleMarket); continue; }

                    int here = pass.Price(el.EquipmentElement, selling: false);
                    if (here <= 0) { tally.Note(Block.NoStock); continue; }
                    float realizable = TradePolicy.Realizable(elsewhere.Item2);
                    if (!TradePolicy.BuyAcceptable(here, realizable)) { tally.Note(Block.BelowMargin); continue; }
                    stock.Add((el, good, realizable, (realizable - here) / here));
                }
                stock.Sort((x, y) => y.margin.CompareTo(x.margin));
            }
            else tally.Note(Block.BudgetSpent);
            int herdRoom = -1;

            InAPass(() =>
            {
                foreach (var (el, good, realizable, _) in stock)
                {
                    if (pass.DirectionError || pass.Spendable() <= 0) break;
                    ItemObject item = el.EquipmentElement.Item;
                    bool livestock = good.IsLivestock;
                    if (livestock)
                    {
                        if (herdRoom < 0)
                            herdRoom = Math.Max(0, HerdRoomForLivestock(pass.Party) - pass.Books.HerdTaken(pass.Sim));
                        if (herdRoom <= 0) { tally.Note(Block.HerdFull); continue; }
                    }

                    var prior = pass.Books.Purchases(pass.Sim, item.StringId);
                    int remaining = el.Amount - pass.Books.Stocked(pass.Sim, item.StringId);
                    int countThis = prior.count, spentThis = prior.spent;
                    int held = pass.Party.ItemRoster.GetItemNumber(item) +
                               pass.Books.Held(pass.Sim, item.StringId);

                    while (remaining > 0)
                    {
                        int price = pass.Price(el.EquipmentElement, selling: false);
                        if (!TradePolicy.BuyAcceptable(price, realizable)) { tally.Note(Block.BelowMargin); break; }
                        Block capped = WhatStopsBuying(good, price, pass.Spendable(), (countThis, spentThis), held,
                                                       shareCap, livestock, herdRoom,
                                                       pass.Site != null && pass.Site.IsVillage && remaining <= 1);
                        if (capped != Block.None) { tally.Note(capped); break; }
                        if (NoRoomForOneMore(good, pass.Room() - simWeight)) { tally.Note(Block.CarryWeight); break; }

                        if (pass.Sim)
                        {
                            simSpent += price;
                            spentThis += price;
                            countThis++;
                            held++;
                            bought++;
                            remaining--;
                            simWeight += item.Weight;
                            pass.Books.NotePurchase(item.StringId, price, item.Weight,
                                                    TradePolicy.FoodValue(item));
                            if (livestock) { herdRoom--; pass.Books.NoteHerdTaken(); }
                            pass.Tally(item, 1, price);
                            continue;
                        }

                        if (!pass.BuyOne(el, price, what, named, out int cost)) break;
                        if (cost == 0) break;

                        LedgerBehavior.Instance?.RecordPurchase(item.StringId, 1, cost);
                        pass.Books.NoteBought(item.StringId, cost);
                        spentThis += cost;
                        countThis++;
                        held++;
                        bought++;
                        remaining--;
                        if (livestock) herdRoom--;
                        pass.Tally(item, 1, cost);
                    }
                }
            });

            if (pass.Reports && tally.Saw(Block.CarryWeight)) _cargoWasFull = true;

            int spent = pass.Spent(simSpent);
            if (bought > 0)
            {
                pass.Moved();
                Log.Write(pass.Headed(label) + bought +
                          " items, -" + spent + " gold " + pass.Where);
                pass.Logged(selling: false, why);
                if (tally.Any) Log.Write("  stopped on: " + tally.Summary());
                TextObject msg = pass.Said(
                    "{=TL14}[Simulated, best case] TradeLord would buy {ITEMS} for {GOLD} denars.",
                    "{=TL06}TradeLord bought {ITEMS} for {GOLD} denars.",
                    bought, spent);
                if (!pass.Muted) Toast(msg, ToastSpend);
            }
            else if (!pass.DirectionError)
            {
                if (tally.Any) Log.Repeatable(label + "-empty " + pass.Key, tally.Summary(),
                    label + " moved nothing " + pass.Where + ": " + tally.Summary());
                Block stopped = tally.Dominant();
                if (stopped != Block.None && !pass.Muted) NoteStalled(selling: false, stopped);
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

        private void UpdateBestSellTownTracker()
        {
            VisualTrackerManager tracker = Campaign.Current?.VisualTrackerManager;
            if (tracker == null) return;
            Settlement target = Options.Current.MarkBestSellTownOnMap ? FindBestSellTownForCargo() : null;

            if (target == _trackedTown)
            {
                if (target != null && !tracker.CheckTracked(target))
                    tracker.RegisterObject(target);
                return;
            }
            if (_trackedTown != null && !LedgerPanel.IsPinned(_trackedTown) && tracker.CheckTracked(_trackedTown))
                tracker.RemoveTrackedObject(_trackedTown);
            _trackedTown = null;
            if (target != null && !tracker.CheckTracked(target))
            {
                tracker.RegisterObject(target);
                _trackedTown = target;
            }
        }

        private Settlement FindBestSellTownForCargo()
        {
            MobileParty party = MobileParty.MainParty;
            if (party == null) return null;
            ISet<string> locked = TradePolicy.LockedKeys();
            var keepBack = TradePolicy.KeptBack(party.ItemRoster, out var awaited);
            var cargo = new List<(EquipmentElement item, int amount)>();
            for (int i = 0; i < party.ItemRoster.Count; i++)
            {
                ItemRosterElement el = party.ItemRoster.GetElementCopyAtIndex(i);
                if (!TradePolicy.MaySell(el, locked, keepBack, awaited, out int keep)) continue;
                if (el.Amount - keep > 0) cargo.Add((el.EquipmentElement, el.Amount - keep));
            }
            if (cargo.Count == 0) return null;

            Settlement bestTown = null;
            long bestValue = 0;
            foreach (Town town in Town.AllTowns)
            {
                Settlement s = town.Settlement;
                if (s == party.CurrentSettlement) continue;
                if (LedgerBehavior.UnderAttack(s)) continue;
                if (Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(s)) continue;
                if (town.Gold <= bestValue) continue;
                float cap = LedgerBehavior.TravelCeiling(s);
                if (cap > 0f)
                {
                    if (Travel.StraightDaysFromParty(s) > cap) continue;
                    if (Travel.EstimateDaysFromParty(s) > cap) continue;
                }
                long total = 0;
                foreach (var (item, amount) in cargo)
                    total += (long)town.GetItemPrice(item, party, true) * amount;
                if (total > town.Gold) total = town.Gold;
                if (total > bestValue) { bestValue = total; bestTown = s; }
            }
            return bestTown;
        }

        private static void AwardTradeXp(int profit, bool muted)
        {
            int xp = (int)(profit * Options.Current.TradeXpMultiplier);
            if (xp <= 0) return;
            _pendingXp += xp;
            if (!muted) _pendingXpMuted = false;
            Log.Write("trade profit fed to the XP system: " + xp + " denars");
        }
    }

    [HarmonyPatch(typeof(InformationManager), "DisplayMessage")]
    internal static class Patch_SilenceChunkedTradeLines
    {
        [HarmonyPriority(Priority.Last)]
        private static bool Prefix()
        {
            if (!TradeActionBehavior.InGameTransaction) return true;
            TradeActionBehavior.NoteSilenced();
            return false;
        }
    }
}
