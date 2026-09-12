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
    internal struct Spending
    {
        internal int Gold;
        internal float Days;
    }

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

        private static readonly Dictionary<string, List<Spending>> _spending =
            new Dictionary<string, List<Spending>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, Dictionary<string, float>> _pull =
            new Dictionary<string, Dictionary<string, float>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, float> _across =
            new Dictionary<string, float>(StringComparer.Ordinal);

        private static readonly Dictionary<string, ItemObject> _standsFor =
            new Dictionary<string, ItemObject>(StringComparer.Ordinal);

        private static readonly Dictionary<string, ItemObject> _standsForAt =
            new Dictionary<string, ItemObject>(StringComparer.Ordinal);

        private static bool _saidItCouldNotRead;

        internal static bool On => Options.Current.MarketForecast && Options.Current.Omniscient;

        internal static void Forget()
        {
            _readAtHour = -1;
            _readForGeneration = -1;
            _landing.Clear();
            _spending.Clear();
            _pull.Clear();
            _across.Clear();
            _standsFor.Clear();
            _standsForAt.Clear();
            _saidItCouldNotRead = false;
        }

        internal static int UnitsLanding(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null) return 0;
            withinDays = TradeMath.ToTheQuarterDay(withinDays);
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

        internal static int WorthShift(Settlement site, ItemObject item, float withinDays) =>
            TradeMath.WorthShift(WorthLanding(site, item, withinDays),
                                 WorthLeaving(site, item, withinDays));

        internal static int WorthLanding(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null || item.ItemCategory == null) return 0;
            withinDays = TradeMath.ToTheQuarterDay(withinDays);
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

        internal static int WorthLeaving(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null || item.ItemCategory == null) return 0;
            withinDays = TradeMath.ToTheQuarterDay(withinDays);
            Build();
            if (!_spending.TryGetValue(site.StringId, out List<Spending> coming)) return 0;
            int purse = 0;
            for (int i = 0; i < coming.Count; i++)
            {
                Spending spending = coming[i];
                if (!TradeMath.LandsInTime(spending.Days, withinDays)) continue;
                purse += spending.Gold;
            }
            if (purse <= 0) return 0;
            if (!PullAt(site, out Dictionary<string, float> pull, out float across)) return 0;
            if (!pull.TryGetValue(item.ItemCategory.StringId, out float mine)) return 0;
            return TradeMath.ShareOfAPurse(purse, mine, across);
        }

        private static bool PullAt(Settlement site, out Dictionary<string, float> pull, out float across)
        {
            if (!_pull.TryGetValue(site.StringId, out pull))
            {
                Dictionary<string, float> read = null;
                Guard.Run("Forecast.Pull", () => read = WhatATraderWouldPickAt(site));
                float total = 0f;
                if (read != null) foreach (float one in read.Values) total += one;
                pull = read;
                _pull[site.StringId] = read;
                _across[site.StringId] = total;
            }
            _across.TryGetValue(site.StringId, out across);
            return pull != null && across > 0f;
        }

        private static Dictionary<string, float> WhatATraderWouldPickAt(Settlement site)
        {
            Town town = site.IsTown ? site.Town : null;
            ItemRoster stock = site.ItemRoster;
            if (town == null || stock == null) return null;
            var pull = new Dictionary<string, float>(StringComparer.Ordinal);
            for (int i = 0; i < stock.Count; i++)
            {
                ItemObject item = stock.GetItemAtIndex(i);
                ItemCategory category = item?.ItemCategory;
                if (category == null || stock.GetElementNumber(i) <= 0) continue;
                if (!TradePolicy.Priced(item) || pull.ContainsKey(category.StringId)) continue;
                float appeal = TradeMath.PullOfAPrice(town.MarketData.GetPriceFactor(category));
                if (appeal > 0f) pull[category.StringId] = appeal;
            }
            return pull;
        }

        internal static string WillMake(Workshop shop)
        {
            if (!On || shop == null) return "";
            string made = "";
            Guard.Run("Forecast.WillMake", () => made = Named(Output(shop, out _)));
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
            _spending.Clear();
            _pull.Clear();
            _across.Clear();
            _standsForAt.Clear();
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
                if (carried == null) continue;
                float days = TradeMath.EtaDays(
                    party.GetPosition2D.Distance(bound.GetPosition2D), party.Speed);
                NoteAPurse(bound, party.PartyTradeGold, days);
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
                    List<(ItemCategory category, int count)> made = Output(shops[k], out float progress);
                    float lands = TradeMath.RunLandsIn(progress, WorkshopRunDays);
                    for (int at = 0; at < made.Count; at++)
                    {
                        var (category, count) = made[at];
                        ItemObject stands = StandsForAt(site, category);
                        Note(site, null, category, count,
                             TradeMath.WorthOf(count, stands == null ? 0 : stands.Value),
                             lands);
                    }
                }
            }
        }

        private static List<(ItemCategory category, int count)> Output(Workshop shop, out float progress)
        {
            progress = 0f;
            var made = new List<(ItemCategory, int)>();
            WorkshopType type = shop?.WorkshopType;
            Settlement site = shop?.Settlement;
            if (type == null || type.Productions == null || site == null) return made;

            Dictionary<string, int> held = WhatTheMarketHolds(site);
            var ready = new List<(bool held, float progress)>();
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
            progress = ready[pick].progress;
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

        private static ItemObject StandsForAt(Settlement site, ItemCategory category)
        {
            if (site == null || category == null) return StandsFor(category);
            string key = site.StringId + "/" + category.StringId;
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

        private static void NoteAPurse(Settlement site, int gold, float days)
        {
            if (site == null || gold <= 0) return;
            if (!_spending.TryGetValue(site.StringId, out List<Spending> coming))
            {
                coming = new List<Spending>();
                _spending[site.StringId] = coming;
            }
            coming.Add(new Spending { Gold = gold, Days = days });
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
