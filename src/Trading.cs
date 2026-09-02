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
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TradeLord
{
    internal enum Block
    {
        None, NotMerchandise, NeverList, Locked, CategoryPolicy, Protected,
        MountOrPackAnimal, NotTradable, FoodReserve, TradedHereAlready, NoStock,
        NoResaleMarket, BelowMargin, BelowBestMarket, MerchantTillEmpty, BudgetSpent,
        ItemCountCap, ItemValueCap, CarryWeight, HerdFull, VillageLastUnit
    }

    internal sealed class BlockTally
    {
        private readonly Dictionary<Block, int> _counts = new Dictionary<Block, int>();

        internal void Note(Block reason)
        {
            if (reason == Block.None) return;
            _counts.TryGetValue(reason, out int seen);
            _counts[reason] = seen + 1;
        }

        internal bool Any => _counts.Count > 0;

        internal bool Saw(Block reason) => _counts.ContainsKey(reason);

        private static bool Structural(Block reason) =>
            reason == Block.NotTradable || reason == Block.NotMerchandise;

        internal Block Dominant()
        {
            if (Saw(Block.BudgetSpent)) return Block.BudgetSpent;
            Block top = Block.None;
            int best = 0;
            foreach (var kv in _counts)
                if (!Structural(kv.Key) && (kv.Value > best || (kv.Value == best && kv.Key < top)))
                { best = kv.Value; top = kv.Key; }
            return top;
        }

        internal string Summary()
        {
            var order = new List<KeyValuePair<Block, int>>(_counts);
            order.Sort((x, y) =>
            {
                int byCount = y.Value.CompareTo(x.Value);
                return byCount != 0 ? byCount : x.Key.CompareTo(y.Key);
            });
            var sb = new StringBuilder();
            foreach (var kv in order)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(kv.Key).Append("=").Append(kv.Value);
            }
            return sb.ToString();
        }

        internal static TextObject Phrase(Block reason)
        {
            switch (reason)
            {
                case Block.CategoryPolicy:
                    return Tongue.Text("{=TL41}your category policy excludes it");
                case Block.BelowMargin:
                    return Tongue.Text("{=TL42}prices here miss your margin");
                case Block.BelowBestMarket:
                    return Tongue.Text("{=TL85}you are holding this cargo for a better market");
                case Block.BudgetSpent:
                case Block.ItemCountCap:
                case Block.ItemValueCap:
                    return Tongue.Text("{=TL43}your purse or spending caps are spent");
                case Block.CarryWeight:
                    return Tongue.Text("{=TL44}there is no room to carry more");
                case Block.HerdFull:
                    return Tongue.Text("{=TL86}your party cannot drive any more livestock");
                case Block.MerchantTillEmpty:
                    return Tongue.Text("{=TL46}the merchant has run out of gold");
                case Block.NoResaleMarket:
                    return Tongue.Text("{=TL47}there is nowhere in reach to resell it");
                case Block.TradedHereAlready:
                    return Tongue.Text("{=TL48}you already traded these on this visit");
                case Block.VillageLastUnit:
                    return Tongue.Text("{=TL83}the village is down to its last of each good");
                case Block.NotMerchandise:
                case Block.NeverList:
                case Block.Locked:
                case Block.Protected:
                case Block.MountOrPackAnimal:
                case Block.FoodReserve:
                    return Tongue.Text("{=TL40}your protections held it back");
                default:
                    return Tongue.Text("{=TL45}this market has nothing worth trading");
            }
        }
    }
    public static class TradePolicy
    {
        private static bool IsSmithingMaterial(ItemObject item) =>
            item == DefaultItems.Charcoal || item == DefaultItems.HardWood || item == DefaultItems.IronOre ||
            item == DefaultItems.IronIngot1 || item == DefaultItems.IronIngot2 || item == DefaultItems.IronIngot3 ||
            item == DefaultItems.IronIngot4 || item == DefaultItems.IronIngot5 || item == DefaultItems.IronIngot6;

        internal static bool Listed(ItemList list, ItemObject item) =>
            item != null && (list.HasId(item.StringId) ||
                             (item.Name != null && list.HasName(item.Name.ToString())));

        private static int _auditedGeneration = -1;

        internal static bool ItemListsNameNothing()
        {
            if (_auditedGeneration == Options.Generation) return false;
            _auditedGeneration = Options.Generation;
            Options s = Options.Current;
            if (string.IsNullOrEmpty(s.NeverSellItems) && string.IsNullOrEmpty(s.AlwaysSellItems) &&
                string.IsNullOrEmpty(s.NeverBuyItems)) return false;
            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ItemObject item in Items.All)
            {
                if (item == null) continue;
                ids.Add(item.StringId);
                if (item.Name != null) names.Add(item.Name.ToString());
            }
            bool missed = false;
            missed |= Unmatched("never sell", s.NeverSellItems, ids, names);
            missed |= Unmatched("always sell", s.AlwaysSellItems, ids, names);
            missed |= Unmatched("never buy", s.NeverBuyItems, ids, names);
            return missed;
        }

        internal static void ForgetItemListAudit() => _auditedGeneration = -1;

        private static bool Unmatched(string label, string written, HashSet<string> ids, HashSet<string> names)
        {
            if (string.IsNullOrEmpty(written)) return false;
            var missing = new List<string>();
            foreach (string entry in written.Split(Options.EntryMarks, StringSplitOptions.RemoveEmptyEntries))
            {
                string whole = entry.Trim();
                if (whole.Length == 0 || ids.Contains(whole) || names.Contains(whole)) continue;
                bool everyWordKnown = true;
                foreach (string word in whole.Split(Options.WordMarks, StringSplitOptions.RemoveEmptyEntries))
                    if (!ids.Contains(word)) { everyWordKnown = false; break; }
                if (!everyWordKnown) missing.Add(whole);
            }
            if (missing.Count == 0) return false;
            Log.Write("the " + label + " list names " + missing.Count +
                      " thing(s) no good in this game matches, so those entries do nothing: " +
                      string.Join(", ", missing.ToArray()) +
                      ". Write either the item id TradeLord.log prints, or the name the game shows.");
            return true;
        }

        internal static int PolicyFor(ItemObject item)
        {
            if (item == null) return Options.PolicyBuySell;
            if (IsSmithingMaterial(item)) return Options.Current.CraftingPolicy;
            if (item.HasHorseComponent) return Options.Current.LivestockPolicy;
            if (item.IsFood) return Options.Current.FoodPolicy;
            return Options.PolicyBuySell;
        }

        internal static bool PolicyAllows(int policy, bool buying) =>
            TradeMath.PolicyAllows(policy, buying);

        internal static ISet<string> LockedKeys()
        {
            if (!Options.Current.RespectLocks) return null;
            var tracker = Campaign.Current?.GetCampaignBehavior<IViewDataTracker>();
            var locks = tracker?.GetInventoryLocks();
            return locks == null ? null : new HashSet<string>(locks);
        }

        private static bool IsLocked(ISet<string> lockedKeys, EquipmentElement element) =>
            lockedKeys != null && lockedKeys.Contains(CampaignUIHelper.GetItemLockStringID(element));

        internal static int FoodValue(ItemObject item)
        {
            if (item == null) return 0;
            if (item.HasHorseComponent)
                return IsTradableLivestock(item) ? item.HorseComponent.MeatCount : 0;
            return item.IsFood ? 1 : 0;
        }

        internal static Dictionary<ItemObject, int> FoodKeep(ItemRoster roster)
        {
            var keep = new Dictionary<ItemObject, int>();
            if (Options.Current.KeepFoodDays <= 0 || roster == null) return keep;
            float perDay = -MobileParty.MainParty.FoodChange;
            if (perDay < 1f) perDay = 1f;
            int reserve = (int)Math.Ceiling(perDay * Options.Current.KeepFoodDays);

            var food = new List<ItemRosterElement>();
            for (int i = 0; i < roster.Count; i++)
            {
                ItemRosterElement el = roster.GetElementCopyAtIndex(i);
                ItemObject item = el.EquipmentElement.Item;
                if (el.Amount > 0 && FoodValue(item) > 0 &&
                    !Listed(Options.Current.AlwaysSet, item))
                    food.Add(el);
            }
            food.Sort((x, y) =>
            {
                int lx = IsTradableLivestock(x.EquipmentElement.Item) ? 1 : 0;
                int ly = IsTradableLivestock(y.EquipmentElement.Item) ? 1 : 0;
                return lx != ly ? lx.CompareTo(ly) : CostPerFood(x).CompareTo(CostPerFood(y));
            });

            foreach (ItemRosterElement el in food)
            {
                if (reserve <= 0) break;
                ItemObject item = el.EquipmentElement.Item;
                int perUnit = FoodValue(item);
                int take = Math.Min(el.Amount, (reserve + perUnit - 1) / perUnit);
                reserve -= take * perUnit;
                keep.TryGetValue(item, out int had);
                keep[item] = had + take;
            }
            return keep;
        }

        private static float CostPerFood(ItemRosterElement el)
        {
            ItemObject item = el.EquipmentElement.Item;
            return (float)item.Value / FoodValue(item);
        }

        public static bool MaySell(ItemRosterElement el, ISet<string> lockedKeys,
                                   IDictionary<ItemObject, int> foodKeep, out int keepCount) =>
            MaySell(el, lockedKeys, foodKeep, out keepCount, out _);

        internal static bool MaySell(ItemRosterElement el, ISet<string> lockedKeys,
                                     IDictionary<ItemObject, int> foodKeep, out int keepCount, out Block why)
        {
            keepCount = 0;
            why = Block.None;
            Options s = Options.Current;
            ItemObject item = el.EquipmentElement.Item;

            if (item == null || item.NotMerchandise || el.EquipmentElement.IsQuestItem) { why = Block.NotMerchandise; return false; }
            if (Listed(s.NeverSet, item)) { why = Block.NeverList; return false; }

            if (IsLocked(lockedKeys, el.EquipmentElement)) { why = Block.Locked; return false; }
            if (Listed(s.AlwaysSet, item)) return true;
            if (!PolicyAllows(PolicyFor(item), buying: false)) { why = Block.CategoryPolicy; return false; }

            bool livestock = item.HasHorseComponent;
            if (livestock)
            {
                if (!IsTradableLivestock(item)) { why = Block.MountOrPackAnimal; return false; }
            }
            else if (s.ProtectSpecial && (item.IsUniqueItem || item.IsCraftedByPlayer))
            { why = Block.Protected; return false; }

            bool sellable = livestock || item.IsTradeGood ||
                (s.MaxLootTier > 0 && !item.IsFood && !item.IsAnimal && !item.IsMountable &&
                 (int)item.Tier + 1 <= s.MaxLootTier);
            if (!sellable) { why = Block.NotTradable; return false; }

            if (foodKeep != null && foodKeep.TryGetValue(item, out int reserved) && reserved > 0)
            {
                keepCount = Math.Min(el.Amount, reserved);
                foodKeep[item] = reserved - keepCount;
                if (el.Amount <= keepCount) { why = Block.FoodReserve; return false; }
            }

            return true;
        }

        internal static bool Priced(ItemObject item) =>
            item != null && (item.IsTradeGood || item.HasHorseComponent || item.IsAnimal);

        internal static bool IsTradableLivestock(ItemObject item) =>
            item != null && item.HasHorseComponent && item.HorseComponent.IsLiveStock &&
            !item.HorseComponent.IsMount && !item.HorseComponent.IsPackAnimal;

        public static bool MayBuy(ItemObject item, ISet<string> lockedKeys) =>
            MayBuy(item, lockedKeys, out _);

        internal static bool MayBuy(ItemObject item, ISet<string> lockedKeys, out Block why)
        {
            why = Block.None;
            Options s = Options.Current;
            if (item == null || item.NotMerchandise) { why = Block.NotMerchandise; return false; }
            if (Listed(s.NeverSet, item) || Listed(s.NeverBuySet, item)) { why = Block.NeverList; return false; }
            if (s.NeverBuyGrain && item == DefaultItems.Grain) { why = Block.NeverList; return false; }
            if (IsLocked(lockedKeys, new EquipmentElement(item))) { why = Block.Locked; return false; }

            if (!PolicyAllows(PolicyFor(item), buying: true)) { why = Block.CategoryPolicy; return false; }
            if (item.HasHorseComponent)
            {
                if (IsTradableLivestock(item)) return true;
                why = Block.MountOrPackAnimal;
                return false;
            }
            if (item.IsTradeGood) return true;
            why = Block.NotTradable;
            return false;
        }

        internal static bool MayRoundTrip(ItemObject item, ISet<string> lockedKeys) =>
            MayBuy(item, lockedKeys) &&
            (Listed(Options.Current.AlwaysSet, item) ||
             PolicyAllows(PolicyFor(item), buying: false));

        private static bool HasCostBasis(ItemObject item) =>
            Options.Current.CostBasisMode == 2 ||
            (LedgerBehavior.Instance?.HasPurchaseRecord(item) ?? false);

        internal static int CostBasis(ItemObject item) =>
            HasCostBasis(item) ? (LedgerBehavior.Instance?.GetCostBasis(item) ?? item.Value) : 0;

        internal static int UnpaidWorth(ItemObject item)
        {
            if (item == null) return 0;
            var best = LedgerBehavior.Instance?.BestBuy(item) ?? (null, 0);
            return best.Item2 > 0 ? best.Item2 : item.Value;
        }

        internal static int Credit(int proceeds, int basis, int unpaidWorth) =>
            TradeMath.Credit(proceeds, basis, unpaidWorth);

        public static bool ProfitAcceptable(int costBasis, int townSellPrice) =>
            TradeMath.ProfitAcceptable(costBasis, townSellPrice, Options.Current.MinProfitMargin);

        internal static float Realizable(int farSellPrice) =>
            TradeMath.Realizable(farSellPrice, Options.Current.ResaleSafetyFactor);

        internal static bool BuyAcceptable(int buyPrice, float realizable) =>
            TradeMath.BuyAcceptable(buyPrice, realizable, Options.Current.MinProfitMargin);
    }

    public class TradeActionBehavior : CampaignBehaviorBase
    {
        private Settlement _trackedTown;
        private string _pinnedTowns = "";
        private bool _announcedAutomation;
        private static int _spentThisVisit;
        private static readonly HashSet<string> _soldThisVisit = new HashSet<string>();
        private static readonly Dictionary<string, (int count, int spent)> _boughtThisVisit =
            new Dictionary<string, (int, int)>();
        private static bool _cargoWasFull;

        internal static bool AutomatedTradeInProgress { get; private set; }

        private static int _transactionDepth;
        private static int _silenced;

        internal static bool InGameTransaction => _transactionDepth > 0;

        private static void OpenTransaction() => _transactionDepth++;

        private static void CloseTransaction()
        {
            if (_transactionDepth > 0) _transactionDepth--;
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
            _pendingXp = 0;
            _pendingXpMuted = true;
            AutomatedTradeInProgress = false;
            _herdLookupFailed = false;
            TradePolicy.ForgetItemListAudit();
        }

        public override void SyncData(IDataStore dataStore)
        {
            if (!dataStore.IsLoading) _pinnedTowns = LedgerPanel.PinnedIds();
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
        }

        private void OnDailyTick()
        {
            Guard.Run("Action.DailyTick", UpdateBestSellTownTracker);
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (party != MobileParty.MainParty) return;
            Guard.Run("Action.OnSettlementLeft", UpdateBestSellTownTracker);
        }

        private static void ResetVisit()
        {
            _spentThisVisit = 0;
            _soldThisVisit.Clear();
            _boughtThisVisit.Clear();
            _cargoWasFull = false;
        }

        private static bool NoRoomToCarry()
        {
            MobileParty party = MobileParty.MainParty;
            return party != null && party.InventoryCapacity - party.TotalWeightCarried < 1f;
        }

        private static bool PurseHeldItBack(BlockTally tally) =>
            tally.Any && tally.Dominant() == Block.BudgetSpent;

        private static bool TradedThisVisit() => _soldThisVisit.Count > 0 || _boughtThisVisit.Count > 0;

        private static bool Muted(bool automated) => automated && Options.Current.QuietAutomation;

        private static void WarnUnmatchedItemLists()
        {
            if (!TradePolicy.ItemListsNameNothing()) return;
            Toast(Tongue.Text("{=TL91}An entry on one of your TradeLord item lists matches no good in this game and is doing nothing. TradeLord.log names which."), ToastAlert);
        }

        private static int SpendableGold() =>
            TradeMath.Budget(Hero.MainHero.Gold, Options.Current.GoldReserve,
                             Options.Current.MaxSpendPerVisit, _spentThisVisit, 0);

        private static bool WarnPurseBelowReserve()
        {
            if (TradedThisVisit()) return false;
            if (SpendableGold() > 0) return false;
            TextObject msg = Tongue.Text("{=TL92}Your purse is at {GOLD} denars and your gold reserve is {RESERVE}, so TradeLord will not buy anything here. Sell some cargo, or lower the reserve in its settings.");
            msg.SetTextVariable("GOLD", Hero.MainHero.Gold);
            msg.SetTextVariable("RESERVE", Options.Current.GoldReserve);
            Toast(msg, ToastAlert);
            return true;
        }

        private static void WarnNoRoomToCarry()
        {
            if (TradedThisVisit()) return;
            if (!NoRoomToCarry() && !_cargoWasFull) return;
            Toast(Tongue.Text("{=TL82}Cargo is full - TradeLord cannot buy here until you free up carry weight."),
                  ToastAlert);
        }

        private bool AnnounceAutomation(Settlement settlement)
        {
            if (_announcedAutomation) return false;
            if (!Options.Current.AutoSellOnEntry && !Options.Current.AutoBuyOnEntry) return false;
            if (!CanTradeHere(settlement)) return false;
            _announcedAutomation = true;
            Toast(Tongue.Text("{=TL87}TradeLord buys and sells for you as you enter a market, starting at the next one. Turn auto-sell and auto-buy on entry off in its settings to trade by hand."), ToastAlert);
            Log.Write("automation notice shown - this market is left alone so the campaign can turn it off first");
            return true;
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            Guard.Run("Action.OnSessionLaunched", () =>
            {
                ResetVisit();
                Guard.Run("Action.RestorePins", () => LedgerPanel.RestorePins(_pinnedTowns));
                Guard.Run("Action.RestoreMarker", UpdateBestSellTownTracker);
                Log.Write(Travel.NavalActive
                    ? "naval capability: party can sail - routes and travel times include sea legs"
                    : "naval capability: land-only - land routing in effect");

                void AddOptions(string menu) => Guard.Run(
                    "menu " + menu + " (the other menus are unaffected)", () =>
                {
                    starter.AddGameMenuOption(menu, "tradelord_quicksell",
                        Tongue.Text("{=TL01}Quick-sell trade goods (TradeLord)").ToString(),
                        args =>
                        {
                            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                            return Options.Current.QuickSellMenu && !Options.Current.AutoTradeBoth && CanTradeHere(Settlement.CurrentSettlement);
                        },
                        args => Guard.Run("Action.QuickSellMenu", () => ExecuteQuickSell(Settlement.CurrentSettlement)),
                        false, 4);

                    starter.AddGameMenuOption(menu, "tradelord_quickbuy",
                        Tongue.Text("{=TL10}Quick-buy trade goods (TradeLord)").ToString(),
                        args =>
                        {
                            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                            return Options.Current.EnableBuying && !Options.Current.AutoTradeBoth && CanTradeHere(Settlement.CurrentSettlement);
                        },
                        args => Guard.Run("Action.QuickBuyMenu", () => ExecuteQuickBuy(Settlement.CurrentSettlement)),
                        false, 5);

                    starter.AddGameMenuOption(menu, "tradelord_quicktrade",
                        Tongue.Text("{=TL26}Quick-trade: sell, then buy (TradeLord)").ToString(),
                        args =>
                        {
                            args.optionLeaveType = GameMenuOption.LeaveType.Trade;
                            return Options.Current.QuickSellMenu && Options.Current.EnableBuying && CanTradeHere(Settlement.CurrentSettlement);
                        },
                        args => Guard.Run("Action.QuickTradeMenu", () =>
                        {
                            ExecuteQuickSell(Settlement.CurrentSettlement);
                            ExecuteQuickBuy(Settlement.CurrentSettlement);
                        }),
                        false, 6);

                    starter.AddGameMenuOption(menu, "tradelord_report",
                        Tongue.Text("{=TL11}Consult the TradeLord ledger").ToString(),
                        args =>
                        {
                            args.optionLeaveType = GameMenuOption.LeaveType.Submenu;
                            return true;
                        },
                        args => Guard.Run("Action.LedgerReport", () => ShowLedgerReport()),
                        false, 7);
                });

                AddOptions("town");
                AddOptions("village");

                if (NavalModulePresent())
                    foreach (string port in new[] { "port_menu", "naval_storyline_virtualport" })
                        AddOptions(port);
            });
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party != MobileParty.MainParty) return;
            Guard.Run("Action.OnSettlementEntered", () =>
            {
                ResetVisit();
                WarnUnmatchedItemLists();

                if (!AnnounceAutomation(settlement))
                {
                    if (Options.Current.AutoSellOnEntry) ExecuteQuickSell(settlement, quiet: true);
                    if (Options.Current.AutoBuyOnEntry) ExecuteQuickBuy(settlement, quiet: true);
                }
                if (Options.Current.EnableBuying && CanTradeHere(settlement))
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

        private static bool MarketOpen(Settlement settlement, bool quiet)
        {
            if (!CanTradeHere(settlement)) return false;
            int wait = Options.Current.EconomySettlingDays;
            if (wait <= 0) return true;
            float elapsed = Campaign.Current.Models.CampaignTimeModel.CampaignStartTime.ElapsedDaysUntilNow;
            if (elapsed >= wait) return true;
            if (!quiet)
            {
                TextObject msg = Tongue.Text("{=TL18}The market is still settling ({DAYS} more days).");
                msg.SetTextVariable("DAYS", (int)Math.Ceiling(wait - elapsed));
                Toast(msg);
            }
            return false;
        }

        private static readonly Color ToastGain = new Color(0.40f, 0.90f, 0.40f);
        private static readonly Color ToastSpend = new Color(0.55f, 0.78f, 1f);
        private static readonly Color ToastFlat = new Color(0.85f, 0.75f, 0.45f);
        private static readonly Color ToastNote = new Color(0.75f, 0.75f, 0.75f);
        private static readonly Color ToastXp = new Color(1f, 0.72f, 0.20f);
        private static readonly Color ToastAlert = new Color(0.90f, 0.28f, 0.28f);

        private static readonly List<InformationMessage> _pending = new List<InformationMessage>();
        private static int _pendingXp;
        private static bool _pendingXpMuted = true;

        private static void Toast(TextObject msg) => Toast(msg, ToastNote);

        private static void Toast(TextObject msg, Color color) =>
            _pending.Add(new InformationMessage(msg.ToString(), color));

        internal static void FlushToasts()
        {
            int xp = _pendingXp;
            bool muted = _pendingXpMuted;
            _pendingXp = 0;
            _pendingXpMuted = true;
            if (xp > 0) CreditTradeSkill(xp, muted);
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

        private static void LogDetail(bool selling, bool sim, Dictionary<ItemObject, (int count, int gold)> detail)
        {
            foreach (var kv in detail)
                Log.Write((selling ? "  sold " : "  bought ") + kv.Value.count + " " +
                          kv.Key.StringId + " for " + kv.Value.gold + (sim ? " (simulated)" : ""));
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

        internal static int HerdRoomForLivestock(MobileParty party)
        {
            try
            {
                if (party == null) return 0;
                var model = Campaign.Current?.Models?.PartySpeedCalculatingModel
                    as DefaultPartySpeedCalculatingModel;
                if (model == null)
                {
                    if (!_herdLookupFailed)
                    {
                        _herdLookupFailed = true;
                        Log.Write("herd guard: a mod replaced the party speed model - livestock buying disabled, selling unaffected");
                    }
                    return 0;
                }
                if (_herdModifier == null)
                {
                    if (_herdLookupFailed) return 0;
                    _herdModifier = typeof(DefaultPartySpeedCalculatingModel).GetMethod(
                        "GetHerdingModifier", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (_herdModifier == null)
                    {
                        _herdLookupFailed = true;
                        Log.Write("herd guard: GetHerdingModifier not found on this game version - livestock buying disabled, selling unaffected");
                        return 0;
                    }
                }
                ItemRoster roster = party.ItemRoster;
                int men = party.MemberRoster?.TotalManCount ?? 0;
                if (men <= 0 || roster == null) return 0;
                int herd = roster.NumberOfPackAnimals + roster.NumberOfLivestockAnimals;
                int mounts = roster.NumberOfMounts;
                int foot = party.Party?.NumberOfMenWithoutHorse ?? 0;
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
                herd += Math.Max(0, mounts - foot);
                float neutral = (float)_herdModifier.Invoke(model, new object[] { men, 0 });
                int room = 0;
                while (room < 256)
                {
                    float mod = (float)_herdModifier.Invoke(model,
                        new object[] { men, herd + room + 1 + HerdCushion });
                    if (mod != neutral) break;
                    room++;
                }
                return room;
            }
            catch (Exception e)
            {
                if (!_herdLookupFailed) { _herdLookupFailed = true; Log.Error(e, "herd guard (livestock buying disabled, selling unaffected)"); }
                return 0;
            }
        }

        private static void CoinSound()
        {
            if (!Options.Current.CoinSound) return;
            try { TaleWorlds.Engine.SoundEvent.PlaySound2D("event:/ui/multiplayer/coin_add"); }
            catch {  }
        }

        public static void ExecuteQuickSell(Settlement settlement, bool quiet = false)
        {
            if (!MarketOpen(settlement, quiet)) return;

            SettlementComponent market = settlement.SettlementComponent;
            PartyBase shop = settlement.Party;
            PartyBase me = MobileParty.MainParty.Party;
            ItemRoster roster = MobileParty.MainParty.ItemRoster;
            ISet<string> locked = TradePolicy.LockedKeys();
            bool sim = Options.Current.SimulationMode;

            LedgerBehavior.Instance?.CaptureSettlement(settlement);

            var plan = new List<ItemRosterElement>();
            for (int i = 0; i < roster.Count; i++)
                plan.Add(roster.GetElementCopyAtIndex(i));
            var foodKeep = TradePolicy.FoodKeep(roster);

            int goldBefore = Hero.MainHero.Gold;
            int soldItems = 0, profit = 0, simGold = 0, simTill = market.Gold;
            bool directionError = false;
            var tally = new BlockTally();
            var detail = new Dictionary<ItemObject, (int count, int gold)>();

            AutomatedTradeInProgress = true;
            try
            {
                foreach (ItemRosterElement el in plan)
                {
                    if (directionError) break;
                    ItemObject item = el.EquipmentElement.Item;
                    if (item != null && _boughtThisVisit.ContainsKey(item.StringId)) { tally.Note(Block.TradedHereAlready); continue; }
                    if (!TradePolicy.MaySell(el, locked, foodKeep, out int keep, out Block why)) { tally.Note(why); continue; }

                    int remaining = el.Amount - keep;
                    if (remaining <= 0) { tally.Note(Block.FoodReserve); continue; }

                    int paid = TradePolicy.CostBasis(item);
                    bool basisIsMarket = Options.Current.CostBasisMode == 2;
                    int paidLeft = LedgerBehavior.Instance?.PurchasedUnits(item) ?? 0;

                    int unpaidFloor = 0;
                    bool floorKnown = false;
                    int unpaidWorth = -1;

                    while (remaining > 0)
                    {
                        int basis = basisIsMarket || paidLeft > 0 ? paid : 0;
                        if (basis == 0 && unpaidWorth < 0) unpaidWorth = TradePolicy.UnpaidWorth(item);
                        int holdFloor = 0;
                        if (Options.Current.PreferBestSellTown || basis == 0)
                        {
                            if (!floorKnown)
                            {
                                floorKnown = true;
                                var best = LedgerBehavior.Instance?.BestSell(item) ?? (null, 0);
                                if (best.Item1 != null && best.Item1 != settlement)
                                    unpaidFloor = (int)(best.Item2 * Options.Current.BestSellTownTolerance);
                            }
                            holdFloor = unpaidFloor;
                        }
                        int price = market.GetItemPrice(el.EquipmentElement, MobileParty.MainParty, true);
                        if (price < holdFloor) { tally.Note(Block.BelowBestMarket); break; }
                        if (!TradePolicy.ProfitAcceptable(basis, price))
                        {
                            tally.Note(Block.BelowMargin);
                            if (basisIsMarket || paidLeft <= 0 || remaining <= paidLeft) break;
                            remaining -= paidLeft;
                            paidLeft = 0;
                            continue;
                        }
                        if ((sim ? simTill : market.Gold) < price) { tally.Note(Block.MerchantTillEmpty); break; }

                        if (sim)
                        {
                            simTill -= price;
                            simGold += price;
                            profit += TradePolicy.Credit(price, basis, unpaidWorth);
                            soldItems++;
                            remaining--;
                            if (paidLeft > 0) paidLeft--;
                            Tally(detail, item, 1, price);
                            continue;
                        }

                        int before = Hero.MainHero.Gold;

                        OpenTransaction();
                        try { SellItemsAction.Apply(me, shop, el, 1, settlement); }
                        finally { CloseTransaction(); }
                        int proceeds = Hero.MainHero.Gold - before;
                        if (proceeds < 0)
                        {
                            Log.Write("ERROR: selling removed " + (-proceeds) + " gold - transaction direction changed on this game version. Selling aborted.");
                            directionError = true;
                            break;
                        }
                        if (proceeds == 0) break;

                        if (paidLeft > 0) { paidLeft--; LedgerBehavior.Instance?.RecordSale(item.StringId, 1); }
                        _soldThisVisit.Add(item.StringId);
                        soldItems++;
                        profit += TradePolicy.Credit(proceeds, basis, unpaidWorth);
                        remaining--;
                        Tally(detail, item, 1, proceeds);
                    }
                }
            }
            finally { AutomatedTradeInProgress = false; _transactionDepth = 0; ReportSilenced(); }

            int goldGained = sim ? simGold : Hero.MainHero.Gold - goldBefore;

            if (soldItems > 0)
            {
                if (!sim)
                {
                    LedgerBehavior.Instance?.AddProfit(profit);
                    CoinSound();
                    LedgerBehavior.Instance?.CaptureSettlement(settlement, force: true);
                }
                Log.Write((sim ? "quick-sell (simulated, best case): " : "quick-sell: ") + soldItems +
                          " items, +" + goldGained + " gold, profit " + profit + " at " + settlement.Name);
                LogDetail(selling: true, sim, detail);
                if (tally.Any) Log.Write("  stopped on: " + tally.Summary());
                TextObject msg = Tongue.Text(sim
                    ? "{=TL13}[Simulated, best case] TradeLord would sell {ITEMS} for {GOLD} denars ({PROFIT} profit)."
                    : "{=TL02}TradeLord sold {ITEMS} for {GOLD} denars ({PROFIT} profit).");
                msg.SetTextVariable("ITEMS", ItemSummary(detail, soldItems));
                msg.SetTextVariable("GOLD", goldGained);
                msg.SetTextVariable("PROFIT", profit);
                if (!Muted(quiet)) Toast(msg, profit > 0 ? ToastGain : ToastFlat);
                if (!sim && profit > 0) AwardTradeXp(profit, Muted(quiet));
            }
            else if (!directionError)
            {
                if (tally.Any) Log.Repeatable("quick-sell-empty " + settlement.StringId, tally.Summary(),
                    "quick-sell moved nothing at " + settlement.Name + ": " + tally.Summary());
                if (!quiet)
                {
                    TextObject none = Tongue.Text("{=TL32}Nothing sold here - {REASON}.");
                    none.SetTextVariable("REASON", BlockTally.Phrase(tally.Dominant()));
                    Toast(none);
                }
            }
        }

        public static void ExecuteQuickBuy(Settlement settlement, bool quiet = false)
        {
            if (!Options.Current.EnableBuying) return;
            if (!MarketOpen(settlement, quiet)) return;

            SettlementComponent market = settlement.SettlementComponent;
            PartyBase shop = settlement.Party;
            PartyBase me = MobileParty.MainParty.Party;
            bool sim = Options.Current.SimulationMode;

            LedgerBehavior.Instance?.CaptureSettlement(settlement);

            int goldBefore = Hero.MainHero.Gold;
            int bought = 0, simSpent = 0;
            float simWeight = 0f;
            bool directionError = false;
            var tally = new BlockTally();
            var detail = new Dictionary<ItemObject, (int count, int gold)>();

            int Budget() =>
                TradeMath.Budget(Hero.MainHero.Gold, Options.Current.GoldReserve,
                                 Options.Current.MaxSpendPerVisit, _spentThisVisit, sim ? simSpent : 0);

            var stock = new List<(ItemRosterElement el, float realizable, float margin)>();
            if (Budget() > 0)
            {
                ISet<string> locked = TradePolicy.LockedKeys();
                ItemRoster shopRoster = settlement.ItemRoster;
                for (int i = 0; i < shopRoster.Count; i++)
                {
                    ItemRosterElement el = shopRoster.GetElementCopyAtIndex(i);
                    ItemObject it = el.EquipmentElement.Item;
                    if (el.Amount <= 0) { tally.Note(Block.NoStock); continue; }
                    if (!TradePolicy.MayBuy(it, locked, out Block whyBuy)) { tally.Note(whyBuy); continue; }
                    if (_soldThisVisit.Contains(it.StringId)) { tally.Note(Block.TradedHereAlready); continue; }

                    var elsewhere = LedgerBehavior.Instance?.BestSell(it) ?? (null, 0);
                    if (elsewhere.Item1 == null || elsewhere.Item1 == settlement) { tally.Note(Block.NoResaleMarket); continue; }

                    int here = market.GetItemPrice(el.EquipmentElement, MobileParty.MainParty, false);
                    if (here <= 0) { tally.Note(Block.NoStock); continue; }
                    float realizable = TradePolicy.Realizable(elsewhere.Item2);
                    if (!TradePolicy.BuyAcceptable(here, realizable)) { tally.Note(Block.BelowMargin); continue; }
                    stock.Add((el, realizable, (realizable - here) / here));
                }
                stock.Sort((x, y) => y.margin.CompareTo(x.margin));
            }
            else tally.Note(Block.BudgetSpent);
            int herdRoom = -1;

            AutomatedTradeInProgress = true;
            try
            {
                foreach (var (el, realizable, _) in stock)
                {
                    if (directionError || Budget() <= 0) break;
                    ItemObject item = el.EquipmentElement.Item;
                    bool livestock = TradePolicy.IsTradableLivestock(item);
                    if (livestock)
                    {
                        if (herdRoom < 0) herdRoom = HerdRoomForLivestock(MobileParty.MainParty);
                        if (herdRoom <= 0) { tally.Note(Block.HerdFull); continue; }
                    }

                    _boughtThisVisit.TryGetValue(item.StringId, out var prior);
                    int remaining = el.Amount;
                    int countThis = prior.count, spentThis = prior.spent;

                    while (remaining > 0)
                    {
                        int price = market.GetItemPrice(el.EquipmentElement, MobileParty.MainParty, false);
                        if (!TradePolicy.BuyAcceptable(price, realizable)) { tally.Note(Block.BelowMargin); break; }
                        if (price > Budget()) { tally.Note(Block.BudgetSpent); break; }
                        if (Options.Current.BuyCapPerItem > 0 &&
                            countThis >= Options.Current.BuyCapPerItem) { tally.Note(Block.ItemCountCap); break; }
                        if (Options.Current.BuyValueCapPerItem > 0 &&
                            spentThis + price > Options.Current.BuyValueCapPerItem) { tally.Note(Block.ItemValueCap); break; }
                        if (livestock && herdRoom <= 0) { tally.Note(Block.HerdFull); break; }
                        if (settlement.IsVillage && remaining <= 1) { tally.Note(Block.VillageLastUnit); break; }
                        if (item.Weight > 0.01f && item.Weight > MobileParty.MainParty.InventoryCapacity
                                - MobileParty.MainParty.TotalWeightCarried - simWeight) { tally.Note(Block.CarryWeight); break; }

                        if (sim)
                        {
                            simSpent += price;
                            spentThis += price;
                            countThis++;
                            bought++;
                            remaining--;
                            simWeight += item.Weight;
                            if (livestock) herdRoom--;
                            Tally(detail, item, 1, price);
                            continue;
                        }

                        int before = Hero.MainHero.Gold;
                        OpenTransaction();
                        try { SellItemsAction.Apply(shop, me, el, 1, settlement); }
                        finally { CloseTransaction(); }
                        int cost = before - Hero.MainHero.Gold;
                        if (cost < 0)
                        {
                            Log.Write("ERROR: buying added " + (-cost) + " gold - transaction direction changed on this game version. Buying aborted.");
                            directionError = true;
                            break;
                        }
                        if (cost == 0) break;

                        LedgerBehavior.Instance?.RecordPurchase(item.StringId, 1, cost);
                        _spentThisVisit += cost;
                        spentThis += cost;
                        countThis++;
                        _boughtThisVisit[item.StringId] = (countThis, spentThis);
                        bought++;
                        remaining--;
                        if (livestock) herdRoom--;
                        Tally(detail, item, 1, cost);
                    }
                }
            }
            finally { AutomatedTradeInProgress = false; _transactionDepth = 0; ReportSilenced(); }

            if (tally.Saw(Block.CarryWeight)) _cargoWasFull = true;

            int spent = sim ? simSpent : goldBefore - Hero.MainHero.Gold;
            if (bought > 0)
            {
                Log.Write((sim ? "quick-buy (simulated, best case): " : "quick-buy: ") + bought +
                          " items, -" + spent + " gold at " + settlement.Name);
                LogDetail(selling: false, sim, detail);
                if (tally.Any) Log.Write("  stopped on: " + tally.Summary());
                if (!sim)
                {
                    CoinSound();
                    LedgerBehavior.Instance?.CaptureSettlement(settlement, force: true);
                }
                TextObject msg = Tongue.Text(sim
                    ? "{=TL14}[Simulated, best case] TradeLord would buy {ITEMS} for {GOLD} denars."
                    : "{=TL06}TradeLord bought {ITEMS} for {GOLD} denars.");
                msg.SetTextVariable("ITEMS", ItemSummary(detail, bought));
                msg.SetTextVariable("GOLD", spent);
                if (!Muted(quiet)) Toast(msg, ToastSpend);
            }
            else if (!directionError)
            {
                if (tally.Any) Log.Repeatable("quick-buy-empty " + settlement.StringId, tally.Summary(),
                    "quick-buy moved nothing at " + settlement.Name + ": " + tally.Summary());
                if (!Muted(quiet) && (!quiet || PurseHeldItBack(tally)))
                {
                    TextObject none = Tongue.Text("{=TL33}Nothing bought here - {REASON}.");
                    none.SetTextVariable("REASON", BlockTally.Phrase(tally.Dominant()));
                    Toast(none);
                }
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
            var foodKeep = TradePolicy.FoodKeep(party.ItemRoster);
            var cargo = new List<(EquipmentElement item, int amount)>();
            for (int i = 0; i < party.ItemRoster.Count; i++)
            {
                ItemRosterElement el = party.ItemRoster.GetElementCopyAtIndex(i);
                if (!TradePolicy.MaySell(el, locked, foodKeep, out int keep)) continue;
                if (el.Amount - keep > 0) cargo.Add((el.EquipmentElement, el.Amount - keep));
            }
            if (cargo.Count == 0) return null;

            float cap = Options.Current.MarkerMaxTravelDays;
            Settlement bestTown = null;
            long bestValue = 0;
            foreach (Town town in Town.AllTowns)
            {
                Settlement s = town.Settlement;
                if (s == party.CurrentSettlement) continue;
                if (LedgerBehavior.UnderAttack(s)) continue;
                if (Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(s)) continue;
                if (!LedgerBehavior.WithinRadius(s)) continue;
                if (cap > 0f)
                {
                    if (Travel.StraightDaysFromParty(s) > cap) continue;
                    if (Travel.EstimateDaysFromParty(s) > cap) continue;
                }
                long total = 0;
                foreach (var (item, amount) in cargo)
                    total += (long)town.GetItemPrice(item, party, true) * amount;
                if (town.Gold > 0 && total > town.Gold) total = town.Gold;
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
