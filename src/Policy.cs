using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.ViewModelCollection;
using TaleWorlds.Core;

namespace TradeLord
{
    public static class TradePolicy
    {
        private static bool IsSmithingMaterial(ItemObject item) =>
            item == DefaultItems.Charcoal || item == DefaultItems.HardWood || item == DefaultItems.IronOre ||
            item == DefaultItems.IronIngot1 || item == DefaultItems.IronIngot2 || item == DefaultItems.IronIngot3 ||
            item == DefaultItems.IronIngot4 || item == DefaultItems.IronIngot5 || item == DefaultItems.IronIngot6;

        internal static bool IsSmeltable(ItemObject item)
        {
            try { return item != null && item.WeaponDesign != null; }
            catch (Exception e) { Log.Error(e, "smeltable check"); return false; }
        }

        internal static bool IsHaulAnimal(ItemObject item) =>
            item != null && item.HasHorseComponent &&
            item.HorseComponent.IsRideable && item.HorseComponent.IsPackAnimal &&
            !item.HorseComponent.IsMount && !item.HorseComponent.IsLiveStock &&
            item.ItemCategory == DefaultItemCategories.PackAnimal;

        internal static bool IsSpareMount(ItemObject item) =>
            item != null && item.HasHorseComponent &&
            item.HorseComponent.IsMount && !item.HorseComponent.IsPackAnimal;

        internal static bool IsPrizeMount(ItemObject item) =>
            IsSpareMount(item) &&
            (item.ItemCategory == DefaultItemCategories.WarHorse ||
             item.ItemCategory == DefaultItemCategories.NobleHorse);

        private static bool _craftingLookupFailed;

        internal static void ForgetCraftingLookup() => _craftingLookupFailed = false;

        internal static bool PartsAllLearned(ItemObject item)
        {
            try
            {
                WeaponDesign design = item?.WeaponDesign;
                if (design == null) return true;
                var crafting = Campaign.Current?.GetCampaignBehavior<ICraftingCampaignBehavior>();
                if (crafting == null)
                {
                    if (!_craftingLookupFailed)
                    {
                        _craftingLookupFailed = true;
                        Log.Write("learned parts: the game's crafting record is not reachable - every smeltable weapon is kept instead");
                    }
                    return false;
                }
                WeaponDesignElement[] pieces = design.UsedPieces;
                for (int i = 0; pieces != null && i < pieces.Length; i++)
                {
                    CraftingPiece piece = pieces[i].CraftingPiece;
                    if (piece != null && !crafting.IsOpened(piece, design.Template)) return false;
                }
                return true;
            }
            catch (Exception e) { Log.Error(e, "learned parts check (the weapon is kept)"); return false; }
        }

        private static int _readGeneration = -1;
        private static HashSet<string> _knownIds, _knownNames;

        private static void ReadTheGoodsInThisGame()
        {
            if (_readGeneration == Options.Generation) return;
            _readGeneration = Options.Generation;
            if (_knownIds == null)
            {
                _knownIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _knownNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (ItemObject item in Items.All)
                {
                    if (item == null) continue;
                    _knownIds.Add(item.StringId);
                    if (item.Name != null) _knownNames.Add(item.Name.ToString());
                }
            }
            Options s = Options.Current;
            s.NeverSet.ReadWordsAsIds(_knownIds);
            s.AlwaysSet.ReadWordsAsIds(_knownIds);
            s.NeverBuySet.ReadWordsAsIds(_knownIds);
            s.AlwaysBuySet.ReadWordsAsIds(_knownIds);
        }

        private static int _auditedGeneration = -1;

        internal static bool ItemListsNameNothing()
        {
            if (_auditedGeneration == Options.Generation) return false;
            _auditedGeneration = Options.Generation;
            Options s = Options.Current;
            if (string.IsNullOrEmpty(s.NeverSellItems) && string.IsNullOrEmpty(s.AlwaysSellItems) &&
                string.IsNullOrEmpty(s.NeverBuyItems) && string.IsNullOrEmpty(s.AlwaysBuyItems)) return false;
            ReadTheGoodsInThisGame();
            bool missed = false;
            missed |= Unmatched("never sell", s.NeverSellItems, _knownIds, _knownNames);
            missed |= Unmatched("always sell", s.AlwaysSellItems, _knownIds, _knownNames);
            missed |= Unmatched("never buy", s.NeverBuyItems, _knownIds, _knownNames);
            missed |= Unmatched("always buy", s.AlwaysBuyItems, _knownIds, _knownNames);
            return missed;
        }

        internal static void ForgetItemListAudit()
        {
            _auditedGeneration = -1;
            _clashGeneration = -1;
            _readGeneration = -1;
            _knownIds = null;
            _knownNames = null;
        }

        private static int _clashGeneration = -1;

        internal static string AnimalGroup(ItemObject item) =>
            item == null ? null : TradeRules.AnimalGroup(Describe(item));

        internal static bool ItemListsNameTwoAnimals()
        {
            if (_clashGeneration == Options.Generation) return false;
            _clashGeneration = Options.Generation;
            Options s = Options.Current;
            var written = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string list in new[] { s.NeverSellItems, s.AlwaysSellItems, s.NeverBuyItems, s.AlwaysBuyItems })
            {
                if (string.IsNullOrEmpty(list)) continue;
                foreach (string entry in list.Split(Options.EntryMarks, StringSplitOptions.RemoveEmptyEntries))
                {
                    string whole = entry.Trim();
                    if (whole.Length > 0) written.Add(whole);
                }
            }
            if (written.Count == 0) return false;

            var byName = new Dictionary<string, List<ItemObject>>(StringComparer.OrdinalIgnoreCase);
            foreach (ItemObject item in Items.All)
            {
                if (item == null || !item.HasHorseComponent || item.Name == null) continue;
                string shown = item.Name.ToString();
                if (!written.Contains(shown)) continue;
                if (!byName.TryGetValue(shown, out List<ItemObject> found))
                    byName[shown] = found = new List<ItemObject>();
                found.Add(item);
            }

            bool clashed = false;
            foreach (var pair in byName)
            {
                if (pair.Value.Count < 2) continue;
                var groups = new HashSet<string>();
                foreach (ItemObject item in pair.Value) groups.Add(AnimalGroup(item));
                if (groups.Count < 2) continue;
                clashed = true;
                var said = new List<string>();
                foreach (ItemObject item in pair.Value)
                    said.Add(item.StringId + " is " + AnimalGroup(item));
                Log.Write("your item lists name \"" + pair.Key + "\", and this game has " + pair.Value.Count +
                          " animals by that name which TradeLord treats differently: " +
                          string.Join(", ", said.ToArray()) +
                          ". Write the item id instead of the shown name to reach only the one you mean.");
            }
            return clashed;
        }

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

        internal static ISet<string> LockedKeys()
        {
            if (!Options.Current.RespectLocks) return null;
            var tracker = Campaign.Current?.GetCampaignBehavior<IViewDataTracker>();
            var locks = tracker?.GetInventoryLocks();
            return locks == null ? null : new HashSet<string>(locks);
        }

        private static bool IsLocked(ISet<string> lockedKeys, EquipmentElement element) =>
            lockedKeys != null && lockedKeys.Contains(CampaignUIHelper.GetItemLockStringID(element));

        internal static int FoodValue(ItemObject item) =>
            item == null ? 0 : TradeRules.FoodValue(Describe(item));

        private static float AppetitePerDay()
        {
            MobileParty party = MobileParty.MainParty;
            float perDay = party == null ? 0f : -party.FoodChange;
            return perDay < 1f ? 1f : perDay;
        }

        internal static Dictionary<ItemObject, int> FoodKeep(ItemRoster roster)
        {
            var keep = new Dictionary<ItemObject, int>();
            if (roster == null) return keep;
            var byId = new Dictionary<string, ItemObject>(StringComparer.Ordinal);
            var carried = new List<TradeRules.Ration>();
            for (int i = 0; i < roster.Count; i++)
            {
                ItemRosterElement el = roster.GetElementCopyAtIndex(i);
                ItemObject item = el.EquipmentElement.Item;
                if (item == null || el.Amount <= 0) continue;
                byId[item.StringId] = item;
                carried.Add(new TradeRules.Ration { Good = Describe(item), Amount = el.Amount });
            }
            foreach (var kept in TradeRules.FoodKeep(carried, AppetitePerDay(), Options.Current))
                if (byId.TryGetValue(kept.Key, out ItemObject item)) keep[item] = kept.Value;
            return keep;
        }

        internal static Dictionary<ItemObject, int> KeptBack(ItemRoster roster,
                                                            out Dictionary<ItemObject, int> awaited)
        {
            Dictionary<ItemObject, int> keep = FoodKeep(roster);
            awaited = Errands.Promised();
            if (awaited == null) return keep;
            foreach (var owed in awaited)
            {
                keep.TryGetValue(owed.Key, out int held);
                if (owed.Value > held) keep[owed.Key] = owed.Value;
            }
            return keep;
        }

        internal static bool IsStorableFood(ItemObject item) =>
            item != null && TradeRules.IsStorableFood(Describe(item));

        internal static int FoodHeld(ItemRoster roster)
        {
            if (roster == null) return 0;
            int held = 0;
            for (int i = 0; i < roster.Count; i++)
            {
                ItemRosterElement el = roster.GetElementCopyAtIndex(i);
                if (el.Amount > 0) held += FoodValue(el.EquipmentElement.Item) * el.Amount;
            }
            return held;
        }

        internal static int FoodWanted()
        {
            int days = Options.Current.KeepFoodDays;
            if (days <= 0) return 0;
            return (int)Math.Ceiling(AppetitePerDay() * days);
        }

        public static bool MaySell(ItemRosterElement el, ISet<string> lockedKeys,
                                   IDictionary<ItemObject, int> foodKeep,
                                   IDictionary<ItemObject, int> awaited, out int keepCount) =>
            MaySell(el, lockedKeys, foodKeep, awaited, out keepCount, out _);

        private struct AskTheGame : IWhatTheGameSays
        {
            internal ISet<string> Locks;
            internal EquipmentElement What;

            public bool Locked() => IsLocked(Locks, What);

            public bool Smeltable() => IsSmeltable(What.Item);

            public bool PartsAllLearned() => TradePolicy.PartsAllLearned(What.Item);
        }

        private static bool AnyListNamesAGood(Options s) =>
            !s.NeverSet.Empty || !s.AlwaysSet.Empty || !s.NeverBuySet.Empty || !s.AlwaysBuySet.Empty;

        internal static Good Describe(ItemObject item)
        {
            Good good = default(Good);
            if (item == null) return good;
            good.Id = item.StringId;
            if (AnyListNamesAGood(Options.Current))
            {
                ReadTheGoodsInThisGame();
                good.Name = item.Name == null ? null : item.Name.ToString();
            }
            good.Weight = item.Weight;
            good.Value = item.Value;
            good.Tier = (int)item.Tier;
            good.NotMerchandise = item.NotMerchandise;
            good.HasHorse = item.HasHorseComponent;
            good.IsUnique = item.IsUniqueItem;
            good.IsCraftedByPlayer = item.IsCraftedByPlayer;
            good.IsTradeGood = item.IsTradeGood;
            good.IsFood = item.IsFood;
            good.IsAnimal = item.IsAnimal;
            good.IsMountable = item.IsMountable;
            good.IsHaulAnimal = IsHaulAnimal(item);
            good.IsSpareMount = IsSpareMount(item);
            good.IsLivestock = IsTradableLivestock(item);
            good.IsPrizeMount = IsPrizeMount(item);
            good.MeatCount = item.HasHorseComponent ? item.HorseComponent.MeatCount : 0;
            good.IsSmithingMaterial = IsSmithingMaterial(item);
            good.IsGrain = item == DefaultItems.Grain;
            return good;
        }

        private static int HeldBack(IDictionary<ItemObject, int> reserve, ItemObject item) =>
            reserve != null && reserve.TryGetValue(item, out int held) && held > 0 ? held : 0;

        private static void TakeBack(IDictionary<ItemObject, int> reserve, ItemObject item, int drawn)
        {
            if (drawn <= 0 || reserve == null) return;
            reserve.TryGetValue(item, out int held);
            reserve[item] = held - drawn;
        }

        internal static bool MaySell(ItemRosterElement el, ISet<string> lockedKeys,
                                     IDictionary<ItemObject, int> foodKeep,
                                     IDictionary<ItemObject, int> awaited,
                                     out int keepCount, out Block why) =>
            MaySell(Describe(el.EquipmentElement.Item), el, lockedKeys, foodKeep, awaited,
                    out keepCount, out why);

        internal static bool MaySell(in Good good, ItemRosterElement el, ISet<string> lockedKeys,
                                     IDictionary<ItemObject, int> foodKeep,
                                     IDictionary<ItemObject, int> awaited,
                                     out int keepCount, out Block why)
        {
            ItemObject item = el.EquipmentElement.Item;
            SellFacts facts;
            facts.QuestItem = el.EquipmentElement.IsQuestItem;
            facts.AwaitedHeld = HeldBack(awaited, item);
            facts.FoodHeld = HeldBack(foodKeep, item);
            facts.QuestsReadable = Errands.Known;

            SellVerdict said = TradeRules.MaySell(good, el.Amount, facts, Options.Current,
                new AskTheGame { Locks = lockedKeys, What = el.EquipmentElement });

            TakeBack(awaited, item, said.DrewAwaited);
            TakeBack(foodKeep, item, said.DrewFood);
            keepCount = said.KeepCount;
            why = said.Why;
            return said.Allowed;
        }

        internal static bool Priced(ItemObject item) =>
            item != null && (item.IsTradeGood || item.HasHorseComponent || item.IsAnimal);

        internal static bool IsTradableLivestock(ItemObject item) =>
            item != null && item.HasHorseComponent && item.HorseComponent.IsLiveStock &&
            !item.HorseComponent.IsMount && !item.HorseComponent.IsPackAnimal;

        internal static bool MayBuy(ItemObject item, ISet<string> lockedKeys, out Block why,
                                    bool toFeed = false)
        {
            if (item == null) { why = Block.NotMerchandise; return false; }
            return MayBuy(Describe(item), item, lockedKeys, out why, toFeed);
        }

        internal static bool MayBuy(in Good good, ItemObject item, ISet<string> lockedKeys,
                                    out Block why, bool toFeed = false) =>
            TradeRules.MayBuy(good, toFeed, Options.Current,
                new AskTheGame { Locks = lockedKeys, What = new EquipmentElement(item) }, out why);

        internal static bool MayHaul(ItemObject item, ISet<string> lockedKeys)
        {
            if (item == null) return false;
            return TradeRules.MayHaul(Describe(item), Options.Current,
                new AskTheGame { Locks = lockedKeys, What = new EquipmentElement(item) });
        }

        internal static bool MayShedForHerd(EquipmentElement held, ISet<string> lockedKeys)
        {
            if (held.Item == null) return false;
            return TradeRules.MayShedForHerd(Describe(held.Item), held.IsQuestItem, Options.Current,
                new AskTheGame { Locks = lockedKeys, What = held });
        }

        internal static bool ResaleAllowed(ItemObject item) =>
            item != null && TradeRules.ResaleAllowed(Describe(item), Options.Current);

        internal static bool MayRoundTrip(ItemObject item, ISet<string> lockedKeys)
        {
            if (item == null) return false;
            Good good = Describe(item);
            return MayBuy(good, item, lockedKeys, out _) &&
                   TradeRules.ResaleAllowed(good, Options.Current);
        }

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
}
