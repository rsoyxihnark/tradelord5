using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TradeLord
{
    internal struct Landing
    {
        internal string Item;
        internal string Category;
        internal int Units;
        internal int Worth;
        internal float Days;
    }

    internal static class Forecast
    {
        private const float WorkshopRunDays = 1f;

        private static int _readAtHour = -1;
        private static int _readForGeneration = -1;

        private static readonly Dictionary<string, List<Landing>> _landing =
            new Dictionary<string, List<Landing>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, ItemObject> _standsFor =
            new Dictionary<string, ItemObject>(StringComparer.Ordinal);

        private static bool _saidItCouldNotRead;

        internal static bool On => Options.Current.MarketForecast && Options.Current.Omniscient;

        internal static void Forget()
        {
            _readAtHour = -1;
            _readForGeneration = -1;
            _landing.Clear();
            _standsFor.Clear();
            _saidItCouldNotRead = false;
        }

        internal static int UnitsLanding(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null) return 0;
            List<Landing> listed = Read(site);
            if (listed == null) return 0;
            int units = 0;
            for (int i = 0; i < listed.Count; i++)
            {
                Landing landing = listed[i];
                if (landing.Item != item.StringId) continue;
                if (!TradeMath.LandsInTime(landing.Days, withinDays)) continue;
                units += landing.Units;
            }
            return units;
        }

        internal static int WorthLanding(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null || item.ItemCategory == null) return 0;
            List<Landing> listed = Read(site);
            if (listed == null) return 0;
            string category = item.ItemCategory.StringId;
            int worth = 0;
            for (int i = 0; i < listed.Count; i++)
            {
                Landing landing = listed[i];
                if (landing.Category != category) continue;
                if (!TradeMath.LandsInTime(landing.Days, withinDays)) continue;
                worth = TradeMath.ShelfAfterLanding(worth, landing.Worth);
            }
            return worth;
        }

        internal static string WillMake(Workshop shop)
        {
            if (!On || shop == null) return "";
            string made = "";
            Guard.Run("Forecast.WillMake", () => made = Named(Output(shop)));
            return made;
        }

        private static List<Landing> Read(Settlement site)
        {
            Build();
            return _landing.TryGetValue(site.StringId, out List<Landing> listed) ? listed : null;
        }

        private static void Build()
        {
            int hour = (int)CampaignTime.Now.ToHours;
            if (hour == _readAtHour && Options.Generation == _readForGeneration) return;
            _readAtHour = hour;
            _readForGeneration = Options.Generation;
            _landing.Clear();
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
                if (party == null || !party.IsCaravan) continue;
                if (party.CurrentSettlement != null) continue;
                Settlement bound = party.TargetSettlement;
                if (bound == null || !(bound.IsTown || bound.IsVillage)) continue;
                ItemRoster carried = party.ItemRoster;
                if (carried == null || carried.Count == 0) continue;
                float days = TradeMath.EtaDays(
                    party.GetPosition2D.Distance(bound.GetPosition2D), party.Speed);
                for (int k = 0; k < carried.Count; k++)
                {
                    ItemObject item = carried.GetItemAtIndex(k);
                    int amount = carried.GetElementNumber(k);
                    if (item == null || amount <= 0 || !TradePolicy.Priced(item)) continue;
                    Note(bound, item.StringId, item.ItemCategory, amount,
                         TradeMath.WorthOf(amount, item.Value), days);
                }
            }
        }

        private static void ReadWhatTheShopsWillMake()
        {
            MBReadOnlyList<Town> towns = Town.AllTowns;
            if (towns == null) return;
            for (int i = 0; i < towns.Count; i++)
            {
                Town town = towns[i];
                Workshop[] shops = town?.Workshops;
                if (shops == null) continue;
                Settlement site = town.Settlement;
                if (site == null) continue;
                for (int k = 0; k < shops.Length; k++)
                {
                    foreach (var (category, count) in Output(shops[k]))
                    {
                        ItemObject stands = StandsFor(category);
                        Note(site, null, category, count,
                             TradeMath.WorthOf(count, stands == null ? 0 : stands.Value),
                             WorkshopRunDays);
                    }
                }
            }
        }

        private static List<(ItemCategory category, int count)> Output(Workshop shop)
        {
            var made = new List<(ItemCategory, int)>();
            WorkshopType type = shop?.WorkshopType;
            Settlement site = shop?.Settlement;
            if (type == null || type.Productions == null || site == null) return made;

            Dictionary<string, int> held = WhatTheMarketHolds(site);
            var ready = new List<(bool, float)>();
            for (int i = 0; i < type.Productions.Count; i++)
            {
                WorkshopType.Production production = type.Productions[i];
                var needs = new List<(string, int)>();
                MBReadOnlyList<(ItemCategory, int)> inputs = production.Inputs;
                for (int k = 0; inputs != null && k < inputs.Count; k++)
                {
                    var (category, count) = inputs[k];
                    needs.Add((category == null ? null : category.StringId, count));
                }
                ready.Add((TradeRules.InputsHeld(needs, held), shop.GetProductionProgress(i)));
            }

            int pick = TradeRules.RunsSoonest(ready);
            if (pick < 0) return made;
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
            ItemRoster stock = site.ItemRoster;
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
            if (made.Count == 0) return "";
            var said = new List<string>();
            for (int i = 0; i < made.Count; i++)
            {
                ItemCategory category = made[i].category;
                string name = Spoken(category);
                said.Add(name + " x" + made[i].count);
            }
            return string.Join(", ", said.ToArray());
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
