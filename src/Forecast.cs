using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

        private static readonly Dictionary<string, List<(float days, int gold)>> _tillAhead =
            new Dictionary<string, List<(float, int)>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, List<(float days, int gold)>> _tillTakes =
            new Dictionary<string, List<(float, int)>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, List<(float days, int gold)>> _tillTakesAhead =
            new Dictionary<string, List<(float, int)>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, float> _taxOnASale = new Dictionary<string, float>(StringComparer.Ordinal);

        private static readonly Dictionary<string, int> _income = new Dictionary<string, int>(StringComparer.Ordinal);

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

        private static readonly Dictionary<string, float> _villagersReach =
            new Dictionary<string, float>(StringComparer.Ordinal);

        private static int _villagesFollowed, _villageLoads, _villageWorth;

        private static float _villageSoonest;

        private static long _villageTicks;

        private static readonly Dictionary<string, int> _wouldHold = new Dictionary<string, int>(StringComparer.Ordinal);

        private sealed class ShopsAhead
        {
            internal WorkshopRuns.Days Days;
            internal float First;
            internal float Watch;
            internal Dictionary<string, (ItemObject stands, float supply, float demand)> Standing;
            internal Dictionary<string, ItemCategory> Kinds;
        }

        private static readonly Dictionary<string, ShopsAhead> _shopsAhead =
            new Dictionary<string, ShopsAhead>(StringComparer.Ordinal);

        private static readonly Dictionary<string, List<(float days, int gold)>> _tillPaid =
            new Dictionary<string, List<(float, int)>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, double> _runSeen = new Dictionary<string, double>(StringComparer.Ordinal);

        private const float JustBeforeTheTick = 1E-4f;

        private sealed class Turn
        {
            internal string Town;
            internal double Since;
            internal int Seen;
        }

        private struct Came
        {
            internal double At;
            internal float Lead;
            internal bool Sold;
            internal bool Priced;
            internal string Item;
            internal string Category;
            internal int Units;
            internal int Worth;
            internal int Gold;
        }

        private const int HoursATurnIsKeptUnseen = 48;

        private static readonly Dictionary<string, Turn> _turns = new Dictionary<string, Turn>(StringComparer.Ordinal);

        private static readonly Dictionary<string, List<Came>> _came =
            new Dictionary<string, List<Came>>(StringComparer.Ordinal);

        private static double _watchedFrom = double.NaN;

        private static int _turnsPrunedAt;

        private static readonly Dictionary<(string site, string item), List<(float lead, float perDay)>> _unseenUnits =
            new Dictionary<(string, string), List<(float, float)>>();

        private static readonly Dictionary<(string site, string kind), List<(float lead, float perDay)>> _unseenWorth =
            new Dictionary<(string, string), List<(float, float)>>();

        private static readonly Dictionary<(string site, string kind), List<(float lead, float perDay)>> _unseenBought =
            new Dictionary<(string, string), List<(float, float)>>();

        private static int _unseenSales, _unseenBuys;

        private static float _unseenWatched;

        internal static bool On => Options.Current.MarketForecast && Options.Current.Omniscient;

        internal static void Forget()
        {
            _readStamp.Stale();
            _landing.Clear();
            _spending.Clear();
            _drawing.Clear();
            _tillPays.Clear();
            _tillAhead.Clear();
            _tillTakes.Clear();
            _tillTakesAhead.Clear();
            _taxOnASale.Clear();
            _income.Clear();
            _tillPaysBy.Clear();
            _unseenPaysOut.Clear();
            _unseenTakesIn.Clear();
            _usedUp.Clear();
            _indexes.Clear();
            _coastIndexes.Clear();
            _averageValues.Clear();
            _standsFor.Clear();
            _standsForAt.Clear();
            _shelfAhead.Clear();
            _shifts.Clear();
            _villagersReach.Clear();
            _wouldHold.Clear();
            _unseenUnits.Clear();
            _unseenWorth.Clear();
            _unseenBought.Clear();
            _shopsAhead.Clear();
            _tillPaid.Clear();
            _runSeen.Clear();
            ForgetTheTurns();
            _saidItCouldNotRead = false;
        }

        private static void ForgetTheTurns()
        {
            _turns.Clear();
            _came.Clear();
            _watchedFrom = double.NaN;
            _turnsPrunedAt = 0;
        }

        internal static int UnitsLanding(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null) return 0;
            int seen = Projection.UnitsLanding(Read(site), item.StringId, withinDays, graded: true);
            return TradeMath.AddedUp(seen, Unseen(_unseenUnits, (site.StringId, item.StringId), withinDays));
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
            TradeMath.ForecastHorizon(withinDays, out float low, out float high, out float share);
            int below = low >= TradeMath.HorizonStep ? ShiftCachedAt(site, item, low) : WorthShift(site, item, low);
            return share > 0f ? TradeMath.BlendedShift(below, ShiftCachedAt(site, item, high), share) : below;
        }

        private static int ShiftCachedAt(Settlement site, ItemObject item, float quarterDay)
        {
            var key = (site.StringId, item.ItemCategory.StringId, quarterDay);
            if (_shifts.TryGetValue(key, out int shift)) return shift;
            shift = WorthShift(site, item, quarterDay);
            _shifts[key] = shift;
            return shift;
        }

        internal static int WorthLanding(Settlement site, ItemObject item, float withinDays)
        {
            if (!On || site == null || item == null || item.ItemCategory == null) return 0;
            int seen = Projection.WorthLanding(Read(site), item.ItemCategory.StringId, withinDays, graded: true);
            return TradeMath.ShelfAfterLanding(seen, Unseen(_unseenWorth, (site.StringId, item.ItemCategory.StringId),
                                                            withinDays));
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

        internal static int[] WorthParts(Settlement site, ItemObject item, float withinDays, out int figure)
        {
            figure = 0;
            if (!On || site == null || item == null || item.ItemCategory == null) return null;
            Build();
            string kind = item.ItemCategory.StringId;
            _landing.TryGetValue(site.StringId, out List<Landing> listed);
            _spending.TryGetValue(site.StringId, out List<Spending> coming);
            _drawing.TryGetValue(site.StringId, out List<Draw> drawn);
            Town town = site.IsTown ? site.Town : null;
            int shelf = town == null ? 0 : Guard.Read("Forecast.Shelf", town,
                                                      where => where.MarketData.GetCategoryData(item.ItemCategory).InStoreValue, 0);
            return Projection.WorthParts(listed, coming, drawn, kind, withinDays, UsedUpADay(site, item.ItemCategory), shelf,
                                         town != null, Unseen(_unseenWorth, (site.StringId, kind), withinDays),
                                         Unseen(_unseenBought, (site.StringId, kind), withinDays), out figure);
        }

        internal static float[] SoonestLanding(Settlement site, ItemObject item)
        {
            if (!On || site == null || item?.ItemCategory == null) return null;
            Build();
            _landing.TryGetValue(site.StringId, out List<Landing> listed);
            return Projection.SoonestLanding(listed, item.ItemCategory.StringId);
        }

        internal static int UsedUpADay(Settlement site, ItemObject item)
        {
            if (!On || site == null || item == null || item.ItemCategory == null) return 0;
            Build();
            return UsedUpADay(site, item.ItemCategory);
        }

        private static int WorthBought(Settlement site, ItemObject item, float withinDays) =>
            TradeMath.AddedUp(_spending.TryGetValue(site.StringId, out List<Spending> coming)
                                  ? Projection.WorthBought(coming, item.ItemCategory.StringId, withinDays, graded: true)
                                  : 0,
                              Unseen(_unseenBought, (site.StringId, item.ItemCategory.StringId), withinDays));

        private static int Unseen<TKey>(Dictionary<TKey, List<(float lead, float perDay)>> from, TKey key,
                                        float withinDays)
        {
            if (!from.TryGetValue(key, out List<(float lead, float perDay)> turns)) return 0;
            float within = TradeMath.Finite(withinDays, 0f);
            float brought = TradeMath.WhatUnseenCaravansBring(turns, within);
            return brought >= int.MaxValue ? int.MaxValue : (int)Math.Round(brought);
        }

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
            _tillAhead.Clear();
            _tillTakes.Clear();
            _tillTakesAhead.Clear();
            _taxOnASale.Clear();
            _income.Clear();
            _tillPaysBy.Clear();
            _unseenPaysOut.Clear();
            _unseenTakesIn.Clear();
            _usedUp.Clear();
            _indexes.Clear();
            _coastIndexes.Clear();
            _standsForAt.Clear();
            _shelfAhead.Clear();
            _shifts.Clear();
            _villagersReach.Clear();
            _wouldHold.Clear();
            _unseenUnits.Clear();
            _unseenWorth.Clear();
            _unseenBought.Clear();
            _shopsAhead.Clear();
            _tillPaid.Clear();
            Guard.Run("Forecast.PriceIndexes", ReadThePriceIndexes);
            Guard.Run("Forecast.Caravans", ReadWhatIsOnTheRoad);
            Guard.Run("Forecast.Villages", ReadWhatTheVillagesWillSend);
            Guard.Run("Forecast.Workshops", ReadWhatTheShopsWillMake);
            Guard.Run("Forecast.Unseen", ReadWhatUnseenCaravansBring);
            Guard.Run("Forecast.Purses", SettleThePurses);
            foreach (List<(float days, int gold)> paid in _tillPays.Values) paid.Sort((x, y) => x.days.CompareTo(y.days));
            foreach (List<(float days, int gold)> paid in _tillAhead.Values) paid.Sort((x, y) => x.days.CompareTo(y.days));
            foreach (List<(float days, int gold)> taken in _tillTakes.Values) taken.Sort((x, y) => x.days.CompareTo(y.days));
            foreach (List<(float days, int gold)> taken in _tillTakesAhead.Values)
                taken.Sort((x, y) => x.days.CompareTo(y.days));
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
                if (party.StringId != null) _villagersReach[party.StringId] = days;
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
                    int draw = NoteTheTillPays(bound, units * price, days, TradeMath.TillDraw.Villagers,
                                               Naming ? party.HomeSettlement?.Name?.ToString() ?? party.Name?.ToString()
                                                      : null,
                                               party.HomeSettlement?.StringId ?? party.StringId);
                    if (TradePolicy.Priced(item))
                        Note(bound, item.StringId, item.ItemCategory, units, TradeMath.WorthOf(units, item.Value), days, draw,
                             (int)TradeMath.TillDraw.Villagers);
                }
            }
        }

        internal static void WatchTheCaravansTurn()
        {
            if (!On || Campaign.Current == null)
            {
                if (!double.IsNaN(_watchedFrom)) ForgetTheTurns();
                return;
            }
            double now = CampaignTime.Now.ToHours;
            bool watching = !double.IsNaN(_watchedFrom);
            int hour = (int)now;
            if (!watching)
            {
                _watchedFrom = now;
                _turnsPrunedAt = hour;
            }
            MBReadOnlyList<MobileParty> all = MobileParty.All;
            for (int i = 0; all != null && i < all.Count; i++)
            {
                MobileParty party = all[i];
                string id = party?.StringId;
                if (id == null || !party.IsCaravan) continue;
                _turns.TryGetValue(id, out Turn turn);
                if (party.CurrentSettlement != null)
                {
                    if (turn != null) turn.Seen = hour;
                    continue;
                }
                Settlement bound = party.TargetSettlement;
                if (bound == null || !bound.IsTown)
                {
                    if (turn != null) _turns.Remove(id);
                    continue;
                }
                if (turn != null && turn.Town == bound.StringId)
                {
                    turn.Seen = hour;
                    continue;
                }
                _turns[id] = new Turn
                {
                    Town = bound.StringId,
                    Since = watching ? now : double.NaN,
                    Seen = hour
                };
            }
            if (hour - _turnsPrunedAt < HoursATurnIsKeptUnseen) return;
            _turnsPrunedAt = hour;
            var gone = new List<string>();
            foreach (KeyValuePair<string, Turn> one in _turns)
                if (hour - one.Value.Seen > HoursATurnIsKeptUnseen) gone.Add(one.Key);
            foreach (string id in gone) _turns.Remove(id);
        }

        internal static void ACaravanTraded(MobileParty caravan, Town town, List<(EquipmentElement, int)> traded)
        {
            if (!On || double.IsNaN(_watchedFrom) || caravan?.StringId == null || town?.Settlement == null ||
                traded == null)
                return;
            Settlement site = town.Settlement;
            if (!_turns.TryGetValue(caravan.StringId, out Turn turn) || turn.Town != site.StringId) return;
            var moved = new Dictionary<ItemObject, int>();
            foreach ((EquipmentElement element, int amount) in traded)
            {
                ItemObject item = element.Item;
                if (item?.ItemCategory == null || item.StringId == null || amount == 0) continue;
                moved.TryGetValue(item, out int had);
                moved[item] = had + amount;
            }
            if (double.IsNaN(turn.Since)) return;
            double at = CampaignTime.Now.ToHours;
            float lead = (float)((at - turn.Since) / 24d);
            if (!_came.TryGetValue(site.StringId, out List<Came> came))
            {
                came = new List<Came>();
                _came[site.StringId] = came;
            }
            foreach (KeyValuePair<ItemObject, int> one in moved)
            {
                ItemObject item = one.Key;
                var (units, sold) = TradeMath.WhatACaravanMoved(one.Value);
                if (units <= 0) continue;
                came.Add(new Came
                {
                    At = at,
                    Lead = lead,
                    Sold = sold,
                    Priced = TradePolicy.Priced(item),
                    Item = item.StringId,
                    Category = item.ItemCategory.StringId,
                    Units = units,
                    Worth = TradeMath.WorthOf(units, item.Value),
                    Gold = TradeMath.WorthOf(units, Priced.At(town, item, caravan, selling: sold))
                });
            }
        }

        private static void ReadWhatUnseenCaravansBring()
        {
            _unseenSales = 0;
            _unseenBuys = 0;
            _unseenWatched = 0f;
            if (double.IsNaN(_watchedFrom)) return;
            double now = CampaignTime.Now.ToHours;
            float watched = (float)((now - _watchedFrom) / 24d);
            _unseenWatched = watched;
            double oldest = now - TradeMath.ACaravansTurnIsKeptFor * 24d;
            var draws = new Dictionary<string, List<(float lead, float perDay)>>(StringComparer.Ordinal);
            var takes = new Dictionary<string, List<(float lead, float perDay)>>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, List<Came>> one in _came)
            {
                List<Came> came = one.Value;
                came.RemoveAll(e => e.At < oldest);
                float kept = 1f - TaxOnASaleAt(Settlement.Find(one.Key));
                for (int i = 0; i < came.Count; i++)
                {
                    Came e = came[i];
                    float perDay = TradeMath.HowOftenSuchATurnComes(e.Lead, watched);
                    if (!(perDay > 0f)) continue;
                    if (!e.Sold)
                    {
                        _unseenBuys++;
                        NoteUnseen(_unseenBought, (one.Key, e.Category), e.Lead, e.Worth * perDay);
                        NoteUnseen(takes, one.Key, e.Lead, e.Gold * perDay * kept);
                        continue;
                    }
                    _unseenSales++;
                    if (e.Priced)
                    {
                        NoteUnseen(_unseenUnits, (one.Key, e.Item), e.Lead, e.Units * perDay);
                        NoteUnseen(_unseenWorth, (one.Key, e.Category), e.Lead, e.Worth * perDay);
                    }
                    NoteUnseen(draws, one.Key, e.Lead, e.Gold * perDay);
                }
            }
            foreach (KeyValuePair<string, List<(float lead, float perDay)>> one in draws)
            {
                var unseen = new List<(float days, int gold)>();
                TradeMath.UnseenDraws(one.Value, TradeMath.MostDaysTheUnseenDrawAPurse, unseen);
                _unseenPaysOut[one.Key] = unseen;
            }
            foreach (KeyValuePair<string, List<(float lead, float perDay)>> one in takes)
            {
                var unseen = new List<(float days, int gold)>();
                TradeMath.UnseenDraws(one.Value, TradeMath.MostDaysTheUnseenDrawAPurse, unseen);
                _unseenTakesIn[one.Key] = unseen;
            }
        }

        private static void SettleThePurses()
        {
            MBReadOnlyList<Town> towns = Town.AllTowns;
            for (int i = 0; towns != null && i < towns.Count; i++)
            {
                Town town = towns[i];
                if (town?.Settlement?.StringId == null) continue;
                Guard.Run("Forecast.Purse", town, SettleThePurse);
            }
        }

        private static void SettleThePurse(Town town)
        {
            Settlement site = town.Settlement;
            string id = site.StringId;
            _tillPays.TryGetValue(id, out List<(float days, int gold)> claims);
            _tillTakes.TryGetValue(id, out List<(float days, int gold)> takes);
            _shopsAhead.TryGetValue(id, out ShopsAhead shops);
            var draws = new List<(float days, int gold)>();
            var takings = takes != null ? new List<(float, int)>(takes) : new List<(float, int)>();
            if ((claims != null && claims.Count > 0) || shops != null)
            {
                var made = new List<Landing>();
                var taken = new List<Draw>();
                var shopDays = new List<float>();
                Func<float, double, Func<int, double>, double> run = null;
                if (shops != null)
                {
                    for (float at = shops.First; at <= shops.Watch; at += 1f) shopDays.Add(at);
                    run = (at, till, share) =>
                    {
                        TownBook book = shops.Days.Town;
                        book.Gold = (int)Math.Min(int.MaxValue, Math.Round(till));
                        WorkshopRuns.Day(shops.Days, at, share, made, taken);
                        if (book.TakenIn > 0L)
                            takings.Add((at - 2f * JustBeforeTheTick, (int)Math.Min(int.MaxValue, book.TakenIn)));
                        if (book.PaidOut > 0L)
                            draws.Add((at - JustBeforeTheTick, (int)Math.Min(int.MaxValue, book.PaidOut)));
                        return book.Gold;
                    };
                }
                int gold = town.Gold;
                double[] paid = TradeMath.WhatAPurseCanPay(gold, WouldHold(site, gold), claims, takes,
                                                           TheTownTicksIn(site), ConsumptionIncome(site),
                                                           TradeMath.MostDaysTheUnseenDrawAPurse, shopDays, run);
                if (claims != null && claims.Count > 0)
                {
                    var settled = new List<(float days, int gold)>(claims.Count);
                    for (int i = 0; i < claims.Count; i++)
                    {
                        int pays = (int)Math.Min(int.MaxValue, Math.Round(paid[i]));
                        settled.Add((claims[i].days, pays));
                        if (pays > 0) draws.Add((claims[i].days, pays));
                    }
                    _tillPaid[id] = settled;
                    if (_landing.TryGetValue(id, out List<Landing> listed)) PaidFor(listed, claims, paid);
                }
                if (shops != null)
                {
                    foreach (Landing one in made)
                        Note(site, shops.Standing[one.Category].stands.StringId, shops.Kinds[one.Category], one.Units, one.Worth, one.Days,
                             0, Projection.FromWorkshops);
                    foreach (Draw one in taken)
                        NoteADraw(site, shops.Kinds[one.Category], one.Worth, one.Days);
                }
            }
            if (_unseenPaysOut.TryGetValue(id, out List<(float days, int gold)> unseenDraws)) draws.AddRange(unseenDraws);
            if (_unseenTakesIn.TryGetValue(id, out List<(float days, int gold)> unseenTakes)) takings.AddRange(unseenTakes);
            _tillAhead[id] = draws;
            _tillTakesAhead[id] = takings;
        }

        private static void PaidFor(List<Landing> listed, List<(float days, int gold)> claims, double[] paid)
        {
            for (int k = listed.Count - 1; k >= 0; k--)
            {
                Landing one = listed[k];
                int at = one.Draw - 1;
                if (at < 0 || at >= claims.Count || claims[at].gold <= 0) continue;
                double share = paid[at] / claims[at].gold;
                if (!(share < 1d)) continue;
                if (share < 0d) share = 0d;
                one.Units = (int)Math.Round(one.Units * share, MidpointRounding.AwayFromZero);
                one.Worth = (int)Math.Round(one.Worth * share, MidpointRounding.AwayFromZero);
                if (one.Units <= 0 && one.Worth <= 0) listed.RemoveAt(k);
                else listed[k] = one;
            }
        }

        private static void NoteUnseen<TKey>(Dictionary<TKey, List<(float lead, float perDay)>> into, TKey key,
                                             float lead, float perDay)
        {
            if (!(perDay > 0f)) return;
            if (!into.TryGetValue(key, out List<(float lead, float perDay)> turns))
            {
                turns = new List<(float, float)>();
                into[key] = turns;
            }
            turns.Add((lead, perDay));
        }

        internal static string TheCaravansNotYetBound()
        {
            if (!On) return null;
            Build();
            return "caravans not bound yet: worked out from " + _unseenSales + " good(s) sold and " + _unseenBuys +
                   " bought by caravans after they chose a town, over " +
                   _unseenWatched.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) +
                   " day(s) watched, the last " +
                   TradeMath.ACaravansTurnIsKeptFor.ToString("0", System.Globalization.CultureInfo.InvariantCulture) +
                   " day(s) of them kept";
        }

        private static void ReadWhatTheVillagesWillSend()
        {
            _villagesFollowed = 0;
            _villageLoads = 0;
            _villageWorth = 0;
            _villageSoonest = float.PositiveInfinity;
            _villageTicks = 0L;
            MBReadOnlyList<Village> villages = Village.All;
            if (villages == null) return;
            long started = Stopwatch.GetTimestamp();
            float watch = WorkshopRuns.Watch(MarketRank.Ceiling(false, Options.Current));
            float speed = Campaign.Current.EstimatedAverageVillagerPartySpeed;
            for (int i = 0; i < villages.Count; i++)
            {
                Village village = villages[i];
                if (village?.Settlement == null) continue;
                Guard.Run("Forecast.Village", village, one => FollowAVillage(one, watch, speed));
            }
            _villageTicks = Stopwatch.GetTimestamp() - started;
        }

        private static void FollowAVillage(Village village, float watch, float speed)
        {
            Settlement home = village.Settlement;
            Settlement town = village.TradeBound;
            if (town == null || !town.IsTown || town.Town == null || town.IsUnderSiege) return;
            if (village.VillageState != Village.VillageStates.Normal || home.Party?.MapEvent != null) return;
            if (FactionManager.IsAtWarAgainstFaction(home.MapFaction, town.MapFaction)) return;
            MobileParty villagers = village.VillagerPartyComponent?.MobileParty;
            if (villagers != null && (villagers.MapEvent != null || villagers.IsInRaftState)) return;
            if (villagers == null &&
                village.Hearth <= Campaign.Current.Models.PartySizeLimitModel.MinimumNumberOfVillagersAtVillagerParty)
                return;
            ItemRoster roster = home.ItemRoster;
            if (roster == null) return;

            var items = new List<ItemObject>();
            var held = new List<float>();
            var made = new List<float>();
            int At(ItemObject item)
            {
                int at = items.IndexOf(item);
                if (at >= 0) return at;
                items.Add(item);
                held.Add(0f);
                made.Add(0f);
                return items.Count - 1;
            }
            for (int k = 0; k < roster.Count; k++)
            {
                ItemObject item = roster.GetItemAtIndex(k);
                int amount = roster.GetElementNumber(k);
                if (item != null && amount > 0) held[At(item)] += amount;
            }
            VillageProductionCalculatorModel model = Campaign.Current.Models.VillageProductionCalculatorModel;
            float madeADay = 0f;
            foreach (var (item, each) in village.VillageType.Productions)
            {
                if (item == null) continue;
                float rate = item.IsMountable
                    ? each * (village.GetHearthLevel() + 1) * 0.5f
                    : model.CalculateDailyProductionAmount(village, item).ResultNumber;
                rate = TradeMath.Finite(rate, 0f);
                if (rate <= 0f) continue;
                made[At(item)] += rate;
                madeADay += rate;
            }
            float food = TradeMath.Finite(model.CalculateDailyFoodProductionAmount(village), 0f);
            IList<ItemObject> foods = Campaign.Current.DefaultVillageTypes?.ConsumableRawItems;
            if (food > 0f && foods != null)
            {
                float rarity = 0f;
                foreach (ItemObject one in foods) if (one != null && one.Value > 0) rarity += 1f / one.Value;
                if (rarity > 0f)
                    foreach (ItemObject one in foods)
                        if (one != null && one.Value > 0) made[At(one)] += food * (1f / one.Value) / rarity;
                madeADay += food;
            }
            if (items.Count == 0) return;

            var weight = new float[items.Count];
            var onHoof = new bool[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                weight[i] = items[i].Weight;
                onHoof[i] = items[i].HasHorseComponent;
            }
            float ride = TradeMath.EtaDays(RoadBetween(home, town), speed);
            float back = WhenTheVillagersAreHome(villagers, home, town, ride, speed);
            float room = villagers != null ? villagers.InventoryCapacity : 0f;
            List<TradeMath.VillageLoad> loads = TradeMath.WhatAVillageSends(
                held.ToArray(), made.ToArray(), weight, onHoof, TradeMath.AVillagesWarehouse(madeADay), room, back,
                villagers != null, ride, watch);
            if (loads.Count == 0) return;

            _villagesFollowed++;
            Town market = town.Town;
            foreach (TradeMath.VillageLoad load in loads)
            {
                int till = market.Gold;
                bool landed = false;
                for (int i = 0; i < items.Count; i++)
                {
                    ItemObject item = items[i];
                    if (item.IsMountable || item.ItemCategory == null) continue;
                    int units = (int)Math.Round(load.Units[i], MidpointRounding.AwayFromZero);
                    if (units <= 0) continue;
                    int price = Priced.At(market, item, villagers, selling: true);
                    int sold = TradeMath.WhatVillagersSell(units, till, price);
                    if (sold <= 0) continue;
                    till -= sold * price;
                    int draw = NoteTheTillPays(town, sold * price, load.Days, TradeMath.TillDraw.VillagesNextTrips,
                                               Naming ? home.Name?.ToString() : null, home.StringId);
                    if (!TradePolicy.Priced(item)) continue;
                    int worth = TradeMath.WorthOf(sold, item.Value);
                    Note(town, item.StringId, item.ItemCategory, sold, worth, load.Days, draw,
                         (int)TradeMath.TillDraw.VillagesNextTrips);
                    _villageWorth = TradeMath.AddedUp(_villageWorth, worth);
                    landed = true;
                }
                if (!landed) continue;
                _villageLoads++;
                if (load.Days < _villageSoonest) _villageSoonest = load.Days;
            }
        }

        private static float WhenTheVillagersAreHome(MobileParty villagers, Settlement home, Settlement town, float ride,
                                                     float speed)
        {
            if (villagers == null || villagers.CurrentSettlement == home) return 0f;
            float stay = TradeMath.DaysVillagersWaitFor(TradeMath.VillagersLeaveATownAnHourWith);
            if (villagers.CurrentSettlement == town) return stay + ride;
            if (villagers.TargetSettlement == town)
            {
                if (villagers.StringId == null || !_villagersReach.TryGetValue(villagers.StringId, out float reach) ||
                    !(reach >= 0f) || TradeMath.OutOfReach(reach))
                    reach = TradeMath.EtaDays(villagers.GetPosition2D.Distance(town.GetPosition2D), speed);
                return reach + stay + ride;
            }
            if (villagers.TargetSettlement == home)
                return TradeMath.EtaDays(villagers.GetPosition2D.Distance(home.GetPosition2D), speed);
            return ride;
        }

        private static float RoadBetween(Settlement from, Settlement to)
        {
            try { return MappedRoadBetween(from, to); }
            catch { return from.GetPosition2D.Distance(to.GetPosition2D); }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static float MappedRoadBetween(Settlement from, Settlement to) =>
            Campaign.Current.Models.MapDistanceModel.GetDistance(from, to, false, false,
                                                                 MobileParty.NavigationType.Default, out _);

        internal static string TheVillagesSend()
        {
            if (!On) return null;
            Build();
            return "villages this hour: " + _villageLoads + " load(s) from " + _villagesFollowed +
                   " village(s) whose villagers have not set out yet, goods worth " + _villageWorth +
                   " denars for the towns they sell to" +
                   (_villageLoads > 0
                       ? ", the soonest landing in " +
                         _villageSoonest.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " day(s)"
                       : "") +
                   ", worked out in " +
                   (_villageTicks * 1000d / Stopwatch.Frequency)
                       .ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " ms";
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
                    int draw = NoteTheTillPays(bound, units * price, days, TradeMath.TillDraw.Caravans,
                                               Naming ? caravan.Name + " (" + caravan.StringId + ")" : null,
                                               caravan.StringId);
                    earned += (long)units * price;
                    shed += units * item.Weight;
                    if (haul) hauling -= units;
                    else if (livestock) herd -= units;
                    if (TradePolicy.Priced(item))
                        Note(bound, item.StringId, kind, units, TradeMath.WorthOf(units, item.Value), days, draw,
                             (int)TradeMath.TillDraw.Caravans);
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
                NoteTheTillTakesIn(bound, units * price, days);
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

            WorkshopRuns.Days days = WorkshopRuns.Begin(lines, book, arriving, bought, price);
            if (days == null) return;
            _shopsAhead[site.StringId] = new ShopsAhead
            {
                Days = days,
                First = WorkshopRuns.FirstDay(TradeMath.NextDailyTickIn(DaysSinceTheTick(site))),
                Watch = watch,
                Standing = standing,
                Kinds = kinds
            };
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

        private static float TaxOnASaleAt(Settlement site)
        {
            if (site?.Town == null) return 1f;
            if (_taxOnASale.TryGetValue(site.StringId, out float tax)) return tax;
            tax = Guard.Read("Forecast.TownTax", site.Town,
                             town => Campaign.Current.Models.SettlementTaxModel.GetTownTaxRatio(town), 1f);
            tax = float.IsNaN(tax) || tax > 1f ? 1f : tax < 0f ? 0f : tax;
            _taxOnASale[site.StringId] = tax;
            return tax;
        }

        private static void NoteTheTillTakesIn(Settlement site, int paid, float days)
        {
            int kept = TradeMath.WhatAPurseKeepsOfAPurchase(paid, TaxOnASaleAt(site));
            if (site == null || kept <= 0) return;
            if (!_tillTakes.TryGetValue(site.StringId, out List<(float days, int gold)> taken))
            {
                taken = new List<(float, int)>();
                _tillTakes[site.StringId] = taken;
            }
            taken.Add((days, kept));
        }

        private static readonly Dictionary<string, List<(float days, int gold, TradeMath.TillDraw from, string who, string id)>>
            _tillPaysBy = new Dictionary<string, List<(float, int, TradeMath.TillDraw, string, string)>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, List<(float days, int gold)>> _unseenPaysOut =
            new Dictionary<string, List<(float, int)>>(StringComparer.Ordinal);

        private static readonly Dictionary<string, List<(float days, int gold)>> _unseenTakesIn =
            new Dictionary<string, List<(float, int)>>(StringComparer.Ordinal);

        private static bool Naming => Options.Current.ExtendedDebugLogging;

        private static int NoteTheTillPays(Settlement site, int gold, float days, TradeMath.TillDraw from, string who,
                                           string id)
        {
            if (site == null || gold <= 0) return 0;
            if (!_tillPays.TryGetValue(site.StringId, out List<(float days, int gold)> paid))
            {
                paid = new List<(float, int)>();
                _tillPays[site.StringId] = paid;
            }
            paid.Add((days, gold));
            if (!Naming) return paid.Count;
            if (!_tillPaysBy.TryGetValue(site.StringId,
                                         out List<(float days, int gold, TradeMath.TillDraw from, string who, string id)> by))
            {
                by = new List<(float, int, TradeMath.TillDraw, string, string)>();
                _tillPaysBy[site.StringId] = by;
            }
            by.Add((days, gold, from, who ?? "?", id));
            return paid.Count;
        }

        internal static string WhoDrawsTheTill(Settlement site, float withinDays)
        {
            if (!On || site == null) return null;
            Build();
            int unseenDraw = WhatCounts(_unseenPaysOut, site.StringId, withinDays);
            int unseenTake = WhatCounts(_unseenTakesIn, site.StringId, withinDays);
            var draws = new List<(int gold, TradeMath.TillDraw from, string who, string id)>();
            if (_tillPaysBy.TryGetValue(site.StringId,
                                        out List<(float days, int gold, TradeMath.TillDraw from, string who, string id)> by))
                foreach (var one in by)
                {
                    double counts = TradeMath.TheShareThatCounts(one.gold, one.days, withinDays);
                    if (counts >= 0.5d)
                        draws.Add(((int)Math.Min(int.MaxValue, Math.Round(counts)), one.from, one.who, one.id));
                }
            if (draws.Count == 0 && unseenDraw == 0 && unseenTake == 0) return null;
            var split = TradeMath.TillDrawsBySource(draws, TopDrawers);
            string most = "";
            foreach (var (who, gold) in split.top) most += (most.Length > 0 ? ", " : "") + who + " " + gold;
            return "caravans on the road " + split.caravans + ", villagers on the road " + split.villagers +
                   ", the villages' next trips " + split.villages + (most.Length > 0 ? "; most from " + most : "") +
                   (split.both.Count > 0
                       ? "; counted both as villagers on the road and for their next trip: " + string.Join(", ", split.both)
                       : "") +
                   "; caravans not bound yet would draw " + unseenDraw + " and pay " + unseenTake +
                   " into it, after the game's tax";
        }

        private static int WhatCounts(Dictionary<string, List<(float days, int gold)>> from, string id, float withinDays)
        {
            if (!from.TryGetValue(id, out List<(float days, int gold)> listed)) return 0;
            double sum = 0d;
            for (int i = 0; i < listed.Count; i++) sum += TradeMath.TheShareThatCounts(listed[i].gold, listed[i].days, withinDays);
            return (int)Math.Min(Math.Round(sum), int.MaxValue);
        }

        private const int TopDrawers = 3;

        internal static int TillOnArrival(Settlement site, float withinDays)
        {
            SettlementComponent market = site?.SettlementComponent;
            if (market == null) return 0;
            int gold = market.Gold;
            if (!On || !site.IsTown || site.Town == null) return gold;
            Build();
            if (!_tillAhead.TryGetValue(site.StringId, out List<(float days, int gold)> paid))
                _tillPays.TryGetValue(site.StringId, out paid);
            return TradeMath.TillWhenYouArrive(gold, WouldHold(site, gold), paid, withinDays, TakenAt(site),
                                               TheTownTicksIn(site), ConsumptionIncome(site));
        }

        internal static float TheTownTicksIn(Settlement site) => TradeMath.ATownTicksIn(DaysSinceTheTick(site));

        private static double DaysSinceTheTick(Settlement site)
        {
            if (site?.Town == null || site.StringId == null) return double.NaN;
            double lastRun = double.MinValue;
            Workshop[] shops = site.Town.Workshops;
            for (int k = 0; shops != null && k < shops.Length; k++)
            {
                Workshop shop = shops[k];
                if (shop?.WorkshopType != null && shop.LastRunCampaignTime.ToDays > lastRun)
                    lastRun = shop.LastRunCampaignTime.ToDays;
            }
            double now = CampaignTime.Now.ToDays;
            double since = lastRun > double.MinValue ? now - lastRun : double.NaN;
            if (since >= 0d && since <= TradeMath.DaysARunTellsTheTickFor) _runSeen[site.StringId] = lastRun;
            return TradeMath.DaysSinceTheTick(since, _runSeen.TryGetValue(site.StringId, out double seen)
                                                         ? now - seen
                                                         : double.NaN);
        }

        private static int ConsumptionIncome(Settlement site)
        {
            if (site?.Town == null) return 0;
            if (_income.TryGetValue(site.StringId, out int known)) return known;
            int income = Guard.Read("Forecast.TownIncome", site.Town, town =>
            {
                var economy = Campaign.Current.Models.SettlementEconomyModel;
                long sum = 0L;
                MBReadOnlyList<ItemCategory> kinds = ItemCategories.All;
                for (int i = 0; kinds != null && i < kinds.Count; i++)
                {
                    ItemCategory kind = kinds[i];
                    if (kind == null) continue;
                    float inStore = town.MarketData.GetCategoryData(kind).InStoreValue;
                    if (!(inStore > 0f)) continue;
                    float budget = economy.CalculateDailySettlementBudgetForItemCategory(
                        town, economy.GetDailyDemandForCategory(town, kind), kind);
                    sum += TradeMath.WhatATownTakesInForWhatItUses(budget, inStore);
                }
                return (int)Math.Min(int.MaxValue, sum);
            }, 0);
            _income[site.StringId] = income;
            return income;
        }

        private static List<(float days, int gold)> TakenAt(Settlement site)
        {
            if (!_tillTakesAhead.TryGetValue(site.StringId, out List<(float days, int gold)> taken))
                _tillTakes.TryGetValue(site.StringId, out taken);
            return taken;
        }

        internal static int TillAtMost(Settlement site)
        {
            SettlementComponent market = site?.SettlementComponent;
            if (market == null) return 0;
            int gold = market.Gold;
            if (!On || !site.IsTown || site.Town == null) return gold;
            Build();
            return TradeMath.MostATillCanHold(gold, WouldHold(site, gold), TakenAt(site), ConsumptionIncome(site));
        }

        private static int WouldHold(Settlement site, int gold)
        {
            if (_wouldHold.TryGetValue(site.StringId, out int wouldHold)) return wouldHold;
            int change = Guard.Read("Forecast.TownGold", site.Town,
                                    town => Campaign.Current.Models.SettlementEconomyModel.GetTownGoldChange(town), 0);
            wouldHold = TradeMath.WhatATownsPurseWouldHold(gold, change);
            _wouldHold[site.StringId] = wouldHold;
            return wouldHold;
        }

        internal static int TillTakesInBefore(Settlement site, float withinDays)
        {
            if (!On || site == null) return 0;
            Build();
            if (!_tillTakes.TryGetValue(site.StringId, out List<(float days, int gold)> taken)) return 0;
            double sum = 0d;
            for (int i = 0; i < taken.Count; i++) sum += TradeMath.TheShareThatCounts(taken[i].gold, taken[i].days, withinDays);
            return (int)Math.Min(Math.Round(sum), int.MaxValue);
        }

        internal static List<(string id, string name, float days, int want, int paid)> TheCaravansDrawing(Settlement site)
        {
            if (!On || site == null) return null;
            Build();
            if (!_tillPaysBy.TryGetValue(site.StringId,
                                         out List<(float days, int gold, TradeMath.TillDraw from, string who, string id)> by))
                return null;
            _tillPaid.TryGetValue(site.StringId, out List<(float days, int gold)> paid);
            bool aligned = paid != null && paid.Count == by.Count;
            var at = new Dictionary<string, int>(StringComparer.Ordinal);
            var drawing = new List<(string id, string name, float days, int want, int paid)>();
            for (int i = 0; i < by.Count; i++)
            {
                var one = by[i];
                if (one.from != TradeMath.TillDraw.Caravans || one.id == null || one.gold <= 0) continue;
                int gets = aligned ? paid[i].gold : one.gold;
                if (!at.TryGetValue(one.id, out int k))
                {
                    at[one.id] = drawing.Count;
                    drawing.Add((one.id, one.who, one.days, one.gold, gets));
                    continue;
                }
                var had = drawing[k];
                drawing[k] = (had.id, had.name, Math.Min(had.days, one.days), TradeMath.AddedUp(had.want, one.gold),
                              TradeMath.AddedUp(had.paid, gets));
            }
            return drawing;
        }

        internal static int TillPaidOutBefore(Settlement site, float withinDays)
        {
            if (!On || site == null) return 0;
            Build();
            if (!_tillPaid.TryGetValue(site.StringId, out List<(float days, int gold)> paid)) return 0;
            double sum = 0d;
            for (int i = 0; i < paid.Count; i++) sum += TradeMath.TheShareThatCounts(paid[i].gold, paid[i].days, withinDays);
            return (int)Math.Min(Math.Round(sum), int.MaxValue);
        }

        internal static int TillPaysOutBefore(Settlement site, float withinDays)
        {
            if (!On || site == null) return 0;
            Build();
            if (!_tillPays.TryGetValue(site.StringId, out List<(float days, int gold)> paid)) return 0;
            double sum = 0d;
            for (int i = 0; i < paid.Count; i++) sum += TradeMath.TheShareThatCounts(paid[i].gold, paid[i].days, withinDays);
            return (int)Math.Min(Math.Round(sum), int.MaxValue);
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
                                 int units, int worth, float days, int draw, int from)
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
                Days = days,
                Draw = draw,
                From = from
            });
        }
    }
}
