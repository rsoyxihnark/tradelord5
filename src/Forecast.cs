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
    internal static class Forecast
    {
        private static Stamp _readStamp;

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

        private static readonly Dictionary<(string site, string category), ItemObject> _standsForAt =
            new Dictionary<(string, string), ItemObject>();

        private static readonly List<(string, int)> _needs = new List<(string, int)>();

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
            _pull.Clear();
            _across.Clear();
            _standsFor.Clear();
            _standsForAt.Clear();
            _shelfAhead.Clear();
            _saidItCouldNotRead = false;
        }

        internal static int UnitsLanding(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null) return 0;
            return Projection.UnitsLanding(Read(site), item.StringId, withinDays);
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
            if (listed != null || coming != null)
            {
                PullAt(site, out Dictionary<string, float> pull, out float across);
                curve = Projection.ShelfAhead(listed, coming, pull, across, item.StringId,
                                              item.ItemCategory.StringId, item.Value,
                                              stockNow, afterDays);
            }
            _shelfAhead[key] = curve;
            return curve;
        }

        internal static int WorthShift(Settlement site, ItemObject item, float withinDays) =>
            TradeMath.WorthShift(WorthLanding(site, item, withinDays),
                                 WorthLeaving(site, item, withinDays));

        internal static int WorthLanding(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null || item.ItemCategory == null) return 0;
            return Projection.WorthLanding(Read(site), item.ItemCategory.StringId, withinDays);
        }

        internal static int WorthLeaving(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null || item.ItemCategory == null) return 0;
            Build();
            if (!_spending.TryGetValue(site.StringId, out List<Spending> coming)) return 0;
            int purse = Projection.PurseLanding(coming, withinDays);
            if (purse <= 0) return 0;
            if (!PullAt(site, out Dictionary<string, float> pull, out float across)) return 0;
            return Projection.WorthLeaving(purse, pull, across, item.ItemCategory.StringId);
        }

        private static bool PullAt(Settlement site, out Dictionary<string, float> pull, out float across)
        {
            if (!_pull.TryGetValue(site.StringId, out pull))
            {
                Dictionary<string, float> read = null;
                Guard.Run("Forecast.Pull", () => read = WhatATraderWouldPickAt(site));
                pull = read;
                _pull[site.StringId] = read;
                _across[site.StringId] = Projection.PullAcross(read);
            }
            _across.TryGetValue(site.StringId, out across);
            return Projection.PullReadable(pull, across);
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
            Guard.Run("Forecast.WillMake",
                      () => made = Named(Output(shop, WhatTheMarketHolds(shop.Settlement), out _)));
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
            _pull.Clear();
            _across.Clear();
            _standsForAt.Clear();
            _shelfAhead.Clear();
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
                Dictionary<string, int> held = WhatTheMarketHolds(site);
                for (int k = 0; k < shops.Length; k++)
                {
                    List<(ItemCategory category, int count)> made = Output(shops[k], held, out float progress);
                    float lands = TradeMath.RunLandsIn(progress, Projection.WorkshopRunDays);
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

        private static List<(ItemCategory category, int count)> Output(Workshop shop,
                                                                      Dictionary<string, int> held,
                                                                      out float progress)
        {
            progress = 0f;
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
