using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace TradeLord
{
    internal static class Forecast
    {
        private static Stamp _readStamp;

        private static readonly Dictionary<string, List<Landing>> _landing =
            new Dictionary<string, List<Landing>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, List<Spending>> _spending =
            new Dictionary<string, List<Spending>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, List<Draw>> _drawing =
            new Dictionary<string, List<Draw>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, List<(float days, int gold)>> _tillPays =
            new Dictionary<string, List<(float, int)>>(StringComparer.Ordinal);

        private static readonly Dictionary<(string site, string kind), int> _usedUp =
            new Dictionary<(string, string), int>();

        private static readonly Dictionary<string, (float average, float lowest)> _indexes =
            new Dictionary<string, (float, float)>(StringComparer.Ordinal);

        private static readonly Dictionary<string, (float average, float lowest)> _coastIndexes =
            new Dictionary<string, (float, float)>(StringComparer.Ordinal);

        private static readonly Dictionary<string, float> _averageValues =
            new Dictionary<string, float>(StringComparer.Ordinal);

        private static readonly Dictionary<string, ItemObject> _standsFor =
            new Dictionary<string, ItemObject>(StringComparer.Ordinal);

        private static readonly Dictionary<(string site, string category), ItemObject> _standsForAt =
            new Dictionary<(string, string), ItemObject>();

        private static readonly List<(string, int)> _needs = new List<(string, int)>();

        private static readonly Dictionary<(string site, string kind, float days), int> _shifts =
            new Dictionary<(string, string, float), int>();

        private static readonly Dictionary<(string site, string item, int stockNow, float afterDays),
                                           List<(float days, int shelf)>> _shelfAhead =
            new Dictionary<(string, string, int, float), List<(float, int)>>();

        private static bool _saidItCouldNotRead;

        internal static bool On => Options.Current.MarketForecast && Options.Current.Omniscient;

        internal static void Forget()
        {
            _readStamp.Stale();
            _landing.Clear();
            _spending.Clear();
            _drawing.Clear();
            _tillPays.Clear();
            _usedUp.Clear();
            _indexes.Clear();
            _coastIndexes.Clear();
            _averageValues.Clear();
            _standsFor.Clear();
            _standsForAt.Clear();
            _shelfAhead.Clear();
            _shifts.Clear();
            _saidItCouldNotRead = false;
        }

        internal static int UnitsLanding(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null) return 0;
            return Projection.UnitsLanding(Read(site), item.StringId, withinDays, graded: true);
        }

        internal static int UnitsLeaving(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null) return 0;
            return Projection.UnitsLeaving(WorthLeaving(site, item, withinDays), item.Value);
        }

        internal static float RunsOutIn(Settlement site, ItemObject item, int stockNow,
                                       int wanted, float afterDays)
        {
            if (!On || site == null || item == null || item.ItemCategory == null)
                return Projection.NeverRunsOut;
            Build();
            return Projection.RunsOutOf(ShelfAhead(site, item, stockNow, afterDays), wanted);
        }

        private static List<(float days, int shelf)> ShelfAhead(Settlement site, ItemObject item,
                                                                int stockNow, float afterDays)
        {
            var key = (site.StringId, item.StringId, stockNow, afterDays);
            if (_shelfAhead.TryGetValue(key, out List<(float days, int shelf)> curve)) return curve;
            _landing.TryGetValue(site.StringId, out List<Landing> listed);
            _spending.TryGetValue(site.StringId, out List<Spending> coming);
            _drawing.TryGetValue(site.StringId, out List<Draw> drawn);
            int usedADay = UsedUpADay(site, item.ItemCategory);
            if (listed != null || coming != null || drawn != null || usedADay > 0)
                curve = Projection.ShelfAhead(listed, coming, item.StringId, item.ItemCategory.StringId,
                                              item.Value, stockNow, afterDays, drawn, usedADay);
            _shelfAhead[key] = curve;
            return curve;
        }

        internal static int WorthShift(Settlement site, ItemObject item, float withinDays) =>
            TradeMath.WorthShift(WorthLanding(site, item, withinDays),
                                 WorthLeaving(site, item, withinDays));

        internal static (int stock, int worth) ShiftAt(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null) return (0, 0);
            int leaving = WorthLeaving(site, item, withinDays);
            return (TradeMath.MissedBy(Projection.UnitsLeaving(leaving, item.Value),
                                       UnitsLanding(site, item, withinDays)),
                    TradeMath.WorthShift(WorthLanding(site, item, withinDays), leaving));
        }

        internal static float TrustEarned()
        {
            LedgerBehavior kept = LedgerBehavior.Instance;
            if (kept == null || !kept.ForecastScore(out _, out long weighed, out float cameTrue))
                return TradeMath.TrustInTheForecast(0L, 0f);
            return TradeMath.TrustInTheForecast(weighed, cameTrue);
        }

        internal static int WorthShiftAsItHasHeld(Settlement site, ItemObject item, float withinDays) =>
            !On ? 0
            : TradeMath.WorthShiftTrusted(PriceShift(site != null && site.IsVillage ? Hindsight.PricedFrom(site) : site,
                                                     item, withinDays),
                                          TrustEarned());

        private static int PriceShift(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null || item.ItemCategory == null) return 0;
            Build();
            var key = (site.StringId, item.ItemCategory.StringId, TradeMath.ARideToTheQuarterDay(withinDays));
            if (_shifts.TryGetValue(key, out int shift)) return shift;
            shift = WorthShift(site, item, withinDays);
            _shifts[key] = shift;
            return shift;
        }

        internal static int WorthLanding(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null || item.ItemCategory == null) return 0;
            return Projection.WorthLanding(Read(site), item.ItemCategory.StringId, withinDays, graded: true);
        }

        internal static int WorthLeaving(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null || item.ItemCategory == null) return 0;
            Build();
            int bought = WorthBought(site, item, withinDays);
            Town town = site.IsTown ? site.Town : null;
            if (town == null) return bought;
            int shelf = Guard.Read("Forecast.Shelf", town,
                                   where => where.MarketData.GetCategoryData(item.ItemCategory).InStoreValue, 0);
            int most = TradeMath.AddedUp(shelf, WorthLanding(site, item, withinDays));
            int leaving = TradeMath.AddedUp(bought, WorthUsedUp(site, item, withinDays, most));
            return leaving > most ? most : leaving;
        }

        internal static int UsedUpADay(Settlement site, ItemObject item)
        {
            if (!On || site == null || item == null || item.ItemCategory == null) return 0;
            Build();
            return UsedUpADay(site, item.ItemCategory);
        }

        private static int WorthBought(Settlement site, ItemObject item, float withinDays) =>
            _spending.TryGetValue(site.StringId, out List<Spending> coming)
                ? Projection.WorthBought(coming, item.ItemCategory.StringId, withinDays, graded: true)
                : 0;

        private static int WorthUsedUp(Settlement site, ItemObject item, float withinDays, int most)
        {
            _drawing.TryGetValue(site.StringId, out List<Draw> drawn);
            int usedADay = UsedUpADay(site, item.ItemCategory);
            if (drawn == null && usedADay <= 0) return 0;
            return Projection.WorthUsedUp(drawn, item.ItemCategory.StringId, withinDays, usedADay, most, graded: true);
        }

        private static int UsedUpADay(Settlement site, ItemCategory category)
        {
            Town town = site != null && site.IsTown ? site.Town : null;
            if (town == null || category == null) return 0;
            var key = (site.StringId, category.StringId);
            if (_usedUp.TryGetValue(key, out int worth)) return worth;
            ItemObject stands = StandsForAt(site, category);
            worth = stands == null ? 0 : Guard.Read("Forecast.UsedUp", town, where =>
            {
                var economy = Campaign.Current.Models.SettlementEconomyModel;
                float budget = economy.CalculateDailySettlementBudgetForItemCategory(
                    where, economy.GetDailyDemandForCategory(where, category), category);
                return TradeMath.WorthUsedUpADay(budget, where.MarketData.GetPrice(stands), stands.Value);
            }, 0);
            _usedUp[key] = worth;
            return worth;
        }

        internal static string WillMake(Workshop shop)
        {
            if (!On || shop == null) return "";
            string made = "";
            Guard.Run("Forecast.WillMake",
                      () => made = Named(Output(shop, WhatTheMarketHolds(shop.Settlement), out _, out _)));
            return made;
        }

        private static List<Landing> Read(Settlement site)
        {
            Build();
            return _landing.TryGetValue(site.StringId, out List<Landing> listed) ? listed : null;
        }

        private static void Build()
        {
            if (Freshness.Fresh(ref _readStamp)) return;
            Freshness.Taken(ref _readStamp);
            _landing.Clear();
            _spending.Clear();
            _drawing.Clear();
            _tillPays.Clear();
            _usedUp.Clear();
            _indexes.Clear();
            _coastIndexes.Clear();
            _standsForAt.Clear();
            _shelfAhead.Clear();
            _shifts.Clear();
            Guard.Run("Forecast.PriceIndexes", ReadThePriceIndexes);
            Guard.Run("Forecast.Caravans", ReadWhatIsOnTheRoad);
            Guard.Run("Forecast.Workshops", ReadWhatTheShopsWillMake);
        }

        private static void ReadWhatIsOnTheRoad()
        {
            MBReadOnlyList<MobileParty> all = MobileParty.All;
            if (all == null) return;
            for (int i = 0; i < all.Count; i++)
            {
                MobileParty party = all[i];
                if (party == null) continue;
                bool caravan = party.IsCaravan;
                if (!caravan && !party.IsVillager) continue;
                if (party.CurrentSettlement != null) continue;
                Settlement bound = party.TargetSettlement;
                if (bound == null || !bound.IsTown) continue;
                ItemRoster carried = party.ItemRoster;
                if (carried == null) continue;
                float days = Travel.DaysFor(party, bound);
                RideCheck.Saw(party, bound, days);
                if (caravan)
                {
                    Guard.Run("Forecast.Caravan", bound, site => FollowACaravan(party, site, days));
                    continue;
                }
                ItemObject ridden = ThePackAnimalVillagersKeep(carried);
                Town town = bound.Town;
                int till = town?.Gold ?? 0;
                for (int k = carried.Count - 1; k >= 0 && town != null; k--)
                {
                    ItemObject item = carried.GetItemAtIndex(k);
                    int amount = carried.GetElementNumber(k);
                    if (item == null || amount <= 0) continue;
                    if (item == ridden) amount -= (int)(0.5f * party.MemberRoster.TotalManCount);
                    if (amount <= 0) continue;
                    int price = Priced.At(town, item, party, selling: true);
                    int units = TradeMath.WhatVillagersSell(amount, till, price);
                    if (units <= 0) continue;
                    till -= units * price;
                    NoteTheTillPays(bound, units * price, days);
                    if (TradePolicy.Priced(item))
                        Note(bound, item.StringId, item.ItemCategory, units, TradeMath.WorthOf(units, item.Value), days);
                }
            }
        }

        private static void ReadThePriceIndexes()
        {
            MBReadOnlyList<Town> towns = Town.AllTowns;
            MBReadOnlyList<ItemCategory> kinds = ItemCategories.All;
            if (towns == null || kinds == null) return;
            foreach (ItemCategory kind in kinds)
            {
                if (kind == null || (!kind.IsTradeGood && !kind.IsAnimal)) continue;
                float all = 0f, lowest = 1000f, coast = 0f, coastLowest = 1000f;
                int counted = 0, ports = 0;
                foreach (Town town in towns)
                {
                    if (town?.Settlement == null) continue;
                    float index = town.GetItemCategoryPriceIndex(kind);
                    all += index;
                    counted++;
                    if (index < lowest) lowest = index;
                    if (!town.Settlement.HasPort) continue;
                    coast += index;
                    ports++;
                    if (index < coastLowest) coastLowest = index;
                }
                if (counted > 0) _indexes[kind.StringId] = (all / counted, lowest);
                if (ports > 0) _coastIndexes[kind.StringId] = (coast / ports, coastLowest);
            }
        }

        private static float AverageValueOf(ItemCategory kind)
        {
            if (_averageValues.Count == 0)
            {
                var added = new Dictionary<string, (float sum, int count)>(StringComparer.Ordinal);
                foreach (ItemObject item in Items.All)
                {
                    if (item?.ItemCategory == null || !((MBObjectBase)item).IsReady) continue;
                    added.TryGetValue(item.ItemCategory.StringId, out var had);
                    added[item.ItemCategory.StringId] = (had.sum + Math.Min(500, item.Value), had.count + 1);
                }
                foreach (var one in added) _averageValues[one.Key] = one.Value.sum / one.Value.count;
            }
            return _averageValues.TryGetValue(kind.StringId, out float value) ? value : 1f;
        }

        private static void FollowACaravan(MobileParty caravan, Settlement bound, float days)
        {
            Town town = bound.IsTown ? bound.Town : null;
            ItemRoster carried = caravan.ItemRoster;
            if (town == null || carried == null) return;
            bool convoy = caravan.HasNavalNavigationCapability;
            bool onLand = caravan.HasLandNavigationCapability;
            var indexes = caravan.CaravanPartyComponent != null &&
                          caravan.CaravanPartyComponent.CanHaveNavalNavigationCapability ? _coastIndexes : _indexes;
            int men = caravan.MemberRoster.TotalManCount;
            int hauling = carried.NumberOfPackAnimals, herd = carried.NumberOfLivestockAnimals;
            var held = new Dictionary<string, int>(StringComparer.Ordinal);
            var sold = new int[carried.Count];
            for (int k = 0; k < carried.Count; k++)
            {
                ItemObject item = carried.GetItemAtIndex(k);
                if (item?.ItemCategory == null) continue;
                held.TryGetValue(item.ItemCategory.StringId, out int had);
                held[item.ItemCategory.StringId] = TradeMath.AddedUp(had, TradeMath.WorthOf(carried.GetElementNumber(k),
                                                                                          item.Value + 10));
            }

            var economy = Campaign.Current.Models.SettlementEconomyModel;
            int till = town.Gold;
            long earned = 0L;
            float shed = 0f;
            for (int horses = 0; horses < 2; horses++)
                for (int k = carried.Count - 1; k >= 0; k--)
                {
                    ItemObject item = carried.GetItemAtIndex(k);
                    ItemCategory kind = item?.ItemCategory;
                    int amount = carried.GetElementNumber(k);
                    if (kind == null || amount <= 0) continue;
                    bool haul = kind == DefaultItemCategories.PackAnimal;
                    bool livestock = item.HorseComponent != null && item.HorseComponent.IsLiveStock;
                    if ((horses == 1) != (item.HasHorseComponent || haul)) continue;
                    if (!indexes.TryGetValue(kind.StringId, out var index)) continue;
                    int haulOver = TradeMath.AnimalsACaravanSellsOff(hauling, men);
                    int herdOver = TradeMath.AnimalsACaravanSellsOff(herd, men);
                    if (haul && onLand && haulOver <= 0) continue;
                    int price = Priced.At(town, item, caravan, selling: true);
                    if (price <= 0) continue;
                    held.TryGetValue(kind.StringId, out int worth);
                    int units = onLand && haul ? haulOver
                        : onLand && livestock && herdOver > 0 ? herdOver
                        : TradeMath.WhatACaravanUnloads(
                              amount, town.GetItemCategoryPriceIndex(kind), index.average, index.lowest, worth,
                              economy.CalculateDailySettlementBudgetForItemCategory(town, town.MarketData.GetDemand(kind),
                                                                                    kind),
                              item.Value, price, convoy, haul);
                    units = Math.Min(units, Math.Min(amount, till / price));
                    if (units <= 0) continue;
                    sold[k] = units;
                    till -= units * price;
                    NoteTheTillPays(bound, units * price, days);
                    earned += (long)units * price;
                    shed += units * item.Weight;
                    if (haul) hauling -= units;
                    else if (livestock) herd -= units;
                    if (TradePolicy.Priced(item))
                        Note(bound, item.StringId, kind, units, TradeMath.WorthOf(units, item.Value), days);
                }

            for (int k = 0; k < carried.Count; k++)
            {
                ItemObject item = carried.GetItemAtIndex(k);
                if (item?.ItemCategory == null || sold[k] <= 0) continue;
                held[item.ItemCategory.StringId] -= TradeMath.WorthOf(sold[k], item.Value + 10);
            }
            int gold = (int)Math.Min(int.MaxValue, caravan.PartyTradeGold + earned);
            float weight = caravan.TotalWeightCarried - shed;
            int capacity = caravan.InventoryCapacity;
            float budget = TradeMath.ACaravansBudgetFactor(gold);
            float room = TradeMath.ACaravansRoomFactor(weight, capacity);
            var worthOf = new Dictionary<string, float>(StringComparer.Ordinal);
            var wanted = new List<(ItemCategory kind, float worth, int at)>();
            MBReadOnlyList<ItemCategory> kinds = ItemCategories.All;
            for (int at = 0; kinds != null && at < kinds.Count; at++)
            {
                ItemCategory kind = kinds[at];
                if (kind == null || (!kind.IsTradeGood && !kind.IsAnimal)) continue;
                if (!indexes.TryGetValue(kind.StringId, out var index)) continue;
                if (town.MarketData.GetItemCountOfCategory(kind) <= 0) continue;
                held.TryGetValue(kind.StringId, out int worth);
                float wants = TradeMath.WhatACaravanThinksAKindIsWorth(
                    town.GetItemCategoryPriceIndex(kind), index.average, worth, town.MarketData.GetDemand(kind),
                    AverageValueOf(kind), budget, room, kind.Properties == ItemCategory.Property.BonusToFoodStores,
                    onLand && kind == DefaultItemCategories.PackAnimal);
                worthOf[kind.StringId] = wants;
                wanted.Add((kind, wants, at));
            }
            wanted.Sort((x, y) => x.worth != y.worth ? y.worth.CompareTo(x.worth) : x.at.CompareTo(y.at));

            void Buy(ItemCategory kind)
            {
                if (!worthOf.TryGetValue(kind.StringId, out float wants)) return;
                if (!kind.IsAnimal && weight / capacity > TradeMath.ACaravanBuysNoGoodsPastThisFull) return;
                int stocked = town.MarketData.GetItemCountOfCategory(kind);
                ItemObject stands = StandsForAt(bound, kind);
                if (stocked <= 0 || stands == null) return;
                int spend = TradeMath.WhatACaravanSpendsOnAKind(
                    gold, wants, Campaign.Current.Models.CaravanModel.GetMaxGoldToSpendOnOneItemCategory(caravan, kind));
                int price = Priced.At(town, stands, caravan, selling: false);
                int most = Campaign.Current.Models.CaravanModel.MaxNumberOfItemsToBuyFromSingleCategory;
                bool livestock = stands.HorseComponent != null && stands.HorseComponent.IsLiveStock;
                bool haul = stands.HorseComponent != null && stands.HorseComponent.IsPackAnimal;
                if (onLand && livestock) most = Math.Min(most, (int)(men * TradeMath.ACaravanDrivesThisManyAnimalsAMan) - herd);
                else if (onLand && haul) most = Math.Min(most, men - hauling);
                int units = TradeMath.WhatACaravanTakesOfAKind(spend, price, stocked, most, stands.Weight, weight,
                                                               capacity, kind.IsAnimal && onLand);
                if (units <= 0) return;
                gold -= units * price;
                weight += units * stands.Weight;
                if (onLand && livestock) herd += units;
                else if (onLand && haul) hauling += units;
                NoteWhatACaravanBuys(bound, kind, TradeMath.WorthOf(units, stands.Value), days);
            }

            int kindsBought = convoy ? TradeMath.AConvoyBuysThisManyKinds : TradeMath.ACaravanBuysThisManyKinds;
            for (int i = 0; i < kindsBought && i < wanted.Count; i++) Buy(wanted[i].kind);
            if (convoy)
            {
                Buy(DefaultItemCategories.Grain);
                Buy(DefaultItemCategories.Fish);
            }
            else if (hauling + herd < men * 2f && hauling < men && gold > 1000)
                Buy(DefaultItemCategories.PackAnimal);
        }

        private static ItemObject ThePackAnimalVillagersKeep(ItemRoster carried)
        {
            ItemObject kept = null;
            for (int k = 0; k < carried.Count; k++)
            {
                ItemObject item = carried.GetItemAtIndex(k);
                if (item?.ItemCategory == DefaultItemCategories.PackAnimal && item.Value < (kept?.Value ?? 10000))
                    kept = item;
            }
            return kept;
        }

        private static void ReadWhatTheShopsWillMake()
        {
            MBReadOnlyList<Town> towns = Town.AllTowns;
            if (towns == null) return;
            float watch = WorkshopRuns.Watch(MarketRank.Ceiling(false, Options.Current));
            IWorkshopWarehouseCampaignBehavior warehouse =
                Campaign.Current?.GetCampaignBehavior<IWorkshopWarehouseCampaignBehavior>();
            for (int i = 0; i < towns.Count; i++)
            {
                Town town = towns[i];
                if (town?.Workshops == null || town.Settlement == null || town.InRebelliousState) continue;
                Guard.Run("Forecast.Workshops", town, one => FollowTheShops(one, watch, warehouse));
            }
        }

        private static void FollowTheShops(Town town, float watch, IWorkshopWarehouseCampaignBehavior warehouse)
        {
            Settlement site = town.Settlement;
            Workshop[] shops = town.Workshops;
            var lines = new List<ShopLine>();
            var kinds = new Dictionary<string, ItemCategory>(StringComparer.Ordinal);
            var book = new TownBook
            {
                Gold = town.Gold,
                Capital = new int[shops.Length],
                Expense = new int[shops.Length],
                Warehouse = new int[shops.Length]
            };
            WorkshopModel pace = Campaign.Current.Models.WorkshopModel;
            double lastRun = double.MinValue;
            for (int k = 0; k < shops.Length; k++)
            {
                Workshop shop = shops[k];
                WorkshopType type = shop?.WorkshopType;
                if (type?.Productions == null) continue;
                bool yours = shop.Owner == Hero.MainHero;
                book.Capital[k] = shop.Capital;
                book.Expense[k] = type.IsHidden ? 0 : shop.Expense;
                bool fromWarehouse = yours && warehouse != null && warehouse.IsGettingInputsFromWarehouse(shop);
                if (fromWarehouse) book.Warehouse[k] = warehouse.GetInputCount(shop);
                float toTown = yours && warehouse != null
                    ? 1f - MathF.Clamp(TradeMath.Finite(warehouse.GetStockProductionInWarehouseRatio(shop), 0f), 0f, 1f)
                    : 1f;
                if (shop.LastRunCampaignTime.ToDays > lastRun) lastRun = shop.LastRunCampaignTime.ToDays;
                for (int p = 0; p < type.Productions.Count; p++)
                {
                    WorkshopType.Production production = type.Productions[p];
                    var line = new ShopLine
                    {
                        Shop = k,
                        Progress = shop.GetProductionProgress(p),
                        Speed = pace.GetEffectiveConversionSpeedOfProduction(shop, production.ConversionSpeed, false)
                                    .ResultNumber,
                        Pace = production.ConversionSpeed,
                        Hidden = type.IsHidden,
                        Yours = yours,
                        FromWarehouse = fromWarehouse,
                        ToTown = toTown
                    };
                    if (Listed(production.Inputs, line.Inputs, line, kinds) &&
                        Listed(production.Outputs, line.Outputs, line, kinds))
                        lines.Add(line);
                }
            }
            if (lines.Count == 0) return;

            Dictionary<string, int> held = WhatTheMarketHolds(site);
            var standing = new Dictionary<string, (ItemObject stands, float supply, float demand)>(StringComparer.Ordinal);
            foreach (var one in kinds)
            {
                ItemObject stands = StandsForAt(site, one.Value);
                if (stands == null) continue;
                ItemData data = town.MarketData.GetCategoryData(one.Value);
                held.TryGetValue(one.Key, out int units);
                book.Units[one.Key] = units;
                book.InStore[one.Key] = (int)Math.Min(int.MaxValue, Math.Max(0f, data.InStoreValue));
                book.UnitValue[one.Key] = stands.Value;
                book.UsedADay[one.Key] = UsedUpADay(site, one.Value);
                standing[one.Key] = (stands, data.Supply, data.Demand);
            }
            lines.RemoveAll(line => !EveryKindPriced(line.Inputs, standing) ||
                                    !EveryKindPriced(line.Outputs, standing));
            if (lines.Count == 0) return;

            TradeItemPriceFactorModel priced = Campaign.Current.Models.TradeItemPriceFactorModel;
            Func<string, int, bool, int> price = (kind, store, selling) =>
            {
                var at = standing[kind];
                return priced.GetPrice(new EquipmentElement(at.stands), null, null, selling, store,
                                       at.supply, at.demand);
            };

            _landing.TryGetValue(site.StringId, out List<Landing> arriving);
            var bought = new List<(float days, string category, int units)>();
            if (_spending.TryGetValue(site.StringId, out List<Spending> coming))
                foreach (Spending one in coming)
                    if (one.Category != null && standing.ContainsKey(one.Category))
                        bought.Add((one.Days, one.Category, Projection.UnitsLeaving(one.Worth, book.UnitValue[one.Category])));

            var made = new List<Landing>();
            var taken = new List<Draw>();
            WorkshopRuns.Follow(lines, book, TradeMath.NextDailyTickIn(CampaignTime.Now.ToDays - lastRun), watch,
                                arriving, bought, price, made, taken);
            foreach (Landing one in made)
                Note(site, standing[one.Category].stands.StringId, kinds[one.Category], one.Units, one.Worth, one.Days);
            foreach (Draw one in taken)
                NoteADraw(site, kinds[one.Category], one.Worth, one.Days);
        }

        private static bool Listed(MBReadOnlyList<(ItemCategory, int)> said, List<(string category, int count)> into,
                                   ShopLine line, Dictionary<string, ItemCategory> kinds)
        {
            for (int k = 0; said != null && k < said.Count; k++)
            {
                var (category, count) = said[k];
                if (category == null || count <= 0) return false;
                into.Add((category.StringId, count));
                kinds[category.StringId] = category;
                if (!category.IsTradeGood) line.TradeGoodsOnly = false;
            }
            return true;
        }

        private static bool EveryKindPriced(List<(string category, int count)> said,
                                   Dictionary<string, (ItemObject stands, float supply, float demand)> standing)
        {
            foreach (var (category, _) in said)
                if (!standing.ContainsKey(category)) return false;
            return true;
        }

        private static List<(ItemCategory category, int count)> Output(Workshop shop,
                                                                      Dictionary<string, int> held,
                                                                      out float progress,
                                                                      out List<(ItemCategory, int)> used)
        {
            progress = 0f;
            used = new List<(ItemCategory, int)>();
            var made = new List<(ItemCategory, int)>();
            WorkshopType type = shop?.WorkshopType;
            Settlement site = shop?.Settlement;
            if (type == null || type.Productions == null || site == null) return made;

            var ready = new List<(bool held, float progress)>();
            for (int i = 0; i < type.Productions.Count; i++)
            {
                WorkshopType.Production production = type.Productions[i];
                _needs.Clear();
                MBReadOnlyList<(ItemCategory, int)> inputs = production.Inputs;
                for (int k = 0; inputs != null && k < inputs.Count; k++)
                {
                    var (category, count) = inputs[k];
                    _needs.Add((category == null ? null : category.StringId, count));
                }
                ready.Add((TradeRules.InputsHeld(_needs, held), shop.GetProductionProgress(i)));
            }

            int pick = TradeRules.RunsSoonest(ready);
            if (pick < 0) return made;
            progress = ready[pick].progress;
            MBReadOnlyList<(ItemCategory, int)> taken = type.Productions[pick].Inputs;
            for (int k = 0; taken != null && k < taken.Count; k++)
            {
                var (category, count) = taken[k];
                if (category != null && count > 0) used.Add((category, count));
            }
            MBReadOnlyList<(ItemCategory, int)> outputs = type.Productions[pick].Outputs;
            for (int k = 0; outputs != null && k < outputs.Count; k++)
            {
                var (category, count) = outputs[k];
                if (category != null && count > 0) made.Add((category, count));
            }
            return made;
        }

        private static Dictionary<string, int> WhatTheMarketHolds(Settlement site)
        {
            var held = new Dictionary<string, int>(StringComparer.Ordinal);
            ItemRoster stock = site?.ItemRoster;
            if (stock == null) return held;
            for (int i = 0; i < stock.Count; i++)
            {
                ItemObject item = stock.GetItemAtIndex(i);
                if (item == null || item.ItemCategory == null) continue;
                string category = item.ItemCategory.StringId;
                held.TryGetValue(category, out int have);
                held[category] = have + stock.GetElementNumber(i);
            }
            return held;
        }

        private static ItemObject StandsForAt(Settlement site, ItemCategory category)
        {
            if (site == null || category == null) return StandsFor(category);
            var key = (site.StringId, category.StringId);
            if (_standsForAt.TryGetValue(key, out ItemObject kept)) return kept;
            ItemObject stocked = null;
            int most = 0;
            ItemRoster stock = site.ItemRoster;
            for (int i = 0; stock != null && i < stock.Count; i++)
            {
                ItemObject item = stock.GetItemAtIndex(i);
                if (item == null || item.ItemCategory != category || !TradePolicy.Priced(item)) continue;
                int count = stock.GetElementNumber(i);
                if (!TradeMath.StandsBetter(count, item.Value, most,
                                            stocked == null ? 0 : stocked.Value)) continue;
                stocked = item;
                most = count;
            }
            ItemObject stands = stocked ?? StandsFor(category);
            _standsForAt[key] = stands;
            return stands;
        }

        private static ItemObject StandsFor(ItemCategory category)
        {
            if (category == null) return null;
            if (_standsFor.TryGetValue(category.StringId, out ItemObject kept)) return kept;
            ItemObject cheapest = null;
            foreach (ItemObject item in Items.All)
            {
                if (item == null || item.ItemCategory != category) continue;
                if (!TradePolicy.Priced(item)) continue;
                if (cheapest == null || item.Value < cheapest.Value) cheapest = item;
            }
            _standsFor[category.StringId] = cheapest;
            return cheapest;
        }

        private static string Named(List<(ItemCategory category, int count)> made)
        {
            var said = new List<(string, int)>(made.Count);
            for (int i = 0; i < made.Count; i++)
                said.Add((Spoken(made[i].category), made[i].count));
            return Projection.Named(said);
        }

        private static string Spoken(ItemCategory category)
        {
            string name = null;
            try { name = category.GetName()?.ToString(); }
            catch (Exception e)
            {
                if (!_saidItCouldNotRead)
                {
                    _saidItCouldNotRead = true;
                    Log.Error(e, "reading what a workshop makes by name - " +
                                 "the good it makes is named by its item instead");
                }
            }
            if (!string.IsNullOrEmpty(name)) return name;
            ItemObject stands = StandsFor(category);
            return stands?.Name == null ? category.StringId : stands.Name.ToString();
        }

        private static void NoteTheTillPays(Settlement site, int gold, float days)
        {
            if (site == null || gold <= 0) return;
            if (!_tillPays.TryGetValue(site.StringId, out List<(float days, int gold)> paid))
            {
                paid = new List<(float, int)>();
                _tillPays[site.StringId] = paid;
            }
            paid.Add((days, gold));
        }

        internal static int TillPaysOutBefore(Settlement site, float withinDays)
        {
            if (!On || site == null) return 0;
            Build();
            if (!_tillPays.TryGetValue(site.StringId, out List<(float days, int gold)> paid)) return 0;
            long sum = 0L;
            for (int i = 0; i < paid.Count; i++)
                if (paid[i].days <= withinDays) sum += paid[i].gold;
            return (int)Math.Min(sum, int.MaxValue);
        }

        private static void NoteADraw(Settlement site, ItemCategory category, int worth, float days)
        {
            if (site == null || category == null || worth <= 0) return;
            if (!_drawing.TryGetValue(site.StringId, out List<Draw> drawn))
            {
                drawn = new List<Draw>();
                _drawing[site.StringId] = drawn;
            }
            drawn.Add(new Draw { Category = category.StringId, Worth = worth, Days = days });
        }

        private static void NoteWhatACaravanBuys(Settlement site, ItemCategory category, int worth, float days)
        {
            if (site == null || category == null || worth <= 0) return;
            if (!_spending.TryGetValue(site.StringId, out List<Spending> coming))
            {
                coming = new List<Spending>();
                _spending[site.StringId] = coming;
            }
            coming.Add(new Spending { Category = category.StringId, Worth = worth, Days = days });
        }

        private static void Note(Settlement site, string item, ItemCategory category,
                                 int units, int worth, float days)
        {
            if (site == null || units <= 0) return;
            if (!_landing.TryGetValue(site.StringId, out List<Landing> listed))
            {
                listed = new List<Landing>();
                _landing[site.StringId] = listed;
            }
            listed.Add(new Landing
            {
                Item = item,
                Category = category == null ? null : category.StringId,
                Units = units,
                Worth = worth,
                Days = days
            });
        }
    }
}
