using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
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

        private static int _villagesFollowed, _villageLoads, _villageWorth, _villageHoursKnown, _villagesHealing;

        private static readonly List<(MobileParty party, Settlement bound, float days)> _caravansAhead =
            new List<(MobileParty, Settlement, float)>();

        private static readonly List<(MobileParty party, Settlement town)> _caravansInTown =
            new List<(MobileParty, Settlement)>();

        private static readonly List<(MobileParty party, Settlement bound, float days)> _villagersAhead =
            new List<(MobileParty, Settlement, float)>();

        private static readonly List<(Settlement home, Settlement town, MobileParty villagers, List<ItemObject> items,
            TradeMath.VillageLoad load)> _villageLoadsAhead =
            new List<(Settlement, Settlement, MobileParty, List<ItemObject>, TradeMath.VillageLoad)>();

        private static int _caravansWaiting, _villagesLooted;

        private static readonly Dictionary<string, List<(float days, float setOut, float ready, float back, float[] dice,
            float diceSaid)>> _nextTrips = new Dictionary<string, List<(float, float, float, float, float[], float)>>(
            StringComparer.Ordinal);

        private static readonly Dictionary<string, (string site, ItemCategory kind, float now, float said)> _caravanMeets =
            new Dictionary<string, (string, ItemCategory, float, float)>(StringComparer.Ordinal);

        private static int _caravansFollowed, _caravansMet;

        private static readonly Dictionary<(string site, string kind), List<(float[] supply, float[] demand)>> _ticksAhead =
            new Dictionary<(string, string), List<(float[], float[])>>();

        private static List<(ItemCategory kind, ItemCategory substitute, float factor)> _substitutes;

        private static string _yourSaleAt;

        private static double _yourSaleWhen = double.NaN;

        private static readonly Dictionary<string, int> _yourSaleWorth = new Dictionary<string, int>(StringComparer.Ordinal);

        private static long _caravanTicks;

        private static readonly Dictionary<string, double> _villageMadeAt = new Dictionary<string, double>(StringComparer.Ordinal);

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

        private static readonly Dictionary<string, double> _townTickedAt =
            new Dictionary<string, double>(StringComparer.Ordinal);

        private static bool _indexesAwaitTheNewGame, _saidTheIndexesWereRead;

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
            _makePaid.Clear();
            _makeWeighed.Clear();
            _shopsCanMake.Clear();
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
            _townTickedAt.Clear();
            _indexesAwaitTheNewGame = false;
            _tradedLast = null;
            _fetchedLast = null;
            _yourSaleMoved = false;
            _yourSaleTakenInAt = int.MinValue;
            _yourSaleTakenIn = 0;
            _firstTicks.Clear();
            _ridden.Clear();
            _onTheRide.Clear();
            _villageStore.Clear();
            _outRatios.Clear();
            _backRatios.Clear();
            _villageMadeAt.Clear();
            _caravansAhead.Clear();
            _caravansInTown.Clear();
            _villagersAhead.Clear();
            _villageLoadsAhead.Clear();
            _nextTrips.Clear();
            _villagersLike.Clear();
            _caravanMeets.Clear();
            _ticksAhead.Clear();
            _substitutes = null;
            _yourSaleAt = null;
            _yourSaleWhen = double.NaN;
            _yourSaleWorth.Clear();
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
                                              item.Value, stockNow, afterDays, drawn, usedADay, TheTownTicksIn(site));
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
            return TradeMath.ShiftOverTheHorizon(withinDays, (site, item),
                                                 (at, days) => ShiftCachedAt(at.Item1, at.Item2, days),
                                                 (at, days) => WorthShift(at.Item1, at.Item2, days));
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
                                         Unseen(_unseenBought, (site.StringId, kind), withinDays),
                                         town != null ? TheTownTicksIn(site) : float.NaN, out figure);
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
            return Projection.WorthUsedUp(drawn, item.ItemCategory.StringId, withinDays, usedADay, most, graded: true,
                                          firstTick: TheTownTicksIn(site));
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
                var (budget, price) = SteppedUse(where, category, stands, null);
                return TradeMath.WorthUsedUpADay(budget, price, stands.Value);
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
            _caravansAhead.Clear();
            _caravansInTown.Clear();
            _villagersAhead.Clear();
            _villageLoadsAhead.Clear();
            _nextTrips.Clear();
            _villagersLike.Clear();
            _caravanMeets.Clear();
            _ticksAhead.Clear();
            _useTicks = 0L;
            _makePaid.Clear();
            if (_indexes.Count == 0) Guard.Run("Forecast.PriceIndexes", ReadThePriceIndexes);
            Guard.Run("Forecast.Caravans", ReadWhatIsOnTheRoad);
            Guard.Run("Forecast.Villages", ReadWhatTheVillagesWillSend);
            Guard.Run("Forecast.Unseen", ReadWhatUnseenCaravansBring);
            long shopsFrom = Stopwatch.GetTimestamp();
            Guard.Run("Forecast.WorkshopsAhead", WhatTheShopsWillMakeBeforeTheRoadArrives);
            _shopTicks = Stopwatch.GetTimestamp() - shopsFrom;
            Guard.Run("Forecast.CaravansOnArrival", FollowTheCaravans);
            Guard.Run("Forecast.WorkshopsAheadForgotten", ForgetWhatTheShopsWereCountedToMake);
            shopsFrom = Stopwatch.GetTimestamp();
            Guard.Run("Forecast.Workshops", ReadWhatTheShopsWillMake);
            _shopTicks += Stopwatch.GetTimestamp() - shopsFrom;
            long pursesFrom = Stopwatch.GetTimestamp();
            Guard.Run("Forecast.Purses", SettleThePurses);
            _purseTicks = Stopwatch.GetTimestamp() - pursesFrom;
            _ticksAhead.Clear();
            _shifts.Clear();
            _shelfAhead.Clear();
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
                if (party.CurrentSettlement != null)
                {
                    Settlement stays = party.CurrentSettlement;
                    if (caravan && party.IsPartyTradeActive && stays.IsTown && stays.Town != null && !stays.IsUnderSiege)
                        _caravansInTown.Add((party, stays));
                    continue;
                }
                Settlement bound = party.TargetSettlement;
                if (bound == null || !bound.IsTown) continue;
                ItemRoster carried = party.ItemRoster;
                if (carried == null) continue;
                float days = Travel.DaysFor(party, bound);
                RideCheck.Saw(party, bound, days);
                if (caravan)
                {
                    _caravansAhead.Add((party, bound, days));
                    continue;
                }
                if (party.StringId != null) _villagersReach[party.StringId] = days;
                _villagersAhead.Add((party, bound, days));
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
            var fetched = new Dictionary<ItemObject, int>();
            int[] paid = WhatTheTradesFetched(town, caravan, traded);
            for (int i = 0; i < traded.Count; i++)
            {
                (EquipmentElement element, int amount) = traded[i];
                ItemObject item = element.Item;
                if (item?.ItemCategory == null || item.StringId == null || amount == 0) continue;
                moved.TryGetValue(item, out int had);
                moved[item] = had + amount;
                fetched.TryGetValue(item, out int gold);
                fetched[item] = TradeMath.AddedUp(gold, paid == null ? 0 : paid[i]);
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
                    Gold = fetched.TryGetValue(item, out int gold) && gold > 0
                        ? gold : TradeMath.WorthOf(units, Priced.At(town, item, caravan, selling: sold))
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
            _villageHoursKnown = 0;
            _villagesHealing = 0;
            _villagesLooted = 0;
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

        private static float MenANewPartySetsOutWith(Village village)
        {
            PartyTemplateObject template = village.Settlement?.Culture?.VillagerPartyTemplate;
            if (template?.Stacks == null) return 0f;
            Hero governor = village.Bound?.Town?.Governor;
            bool network = governor != null && governor.GetPerkValue(DefaultPerks.Scouting.VillageNetwork);
            float men = 0f;
            foreach (PartyTemplateStack stack in template.Stacks)
            {
                float one = stack.MinValue + (stack.MaxValue - stack.MinValue) * 0.5f;
                if (network) one = (float)Math.Round(one * (1f + DefaultPerks.Scouting.VillageNetwork.SecondaryBonus));
                if (one > 0f) men += one;
            }
            return men;
        }

        private static void FollowAVillage(Village village, float watch, float speed)
        {
            Settlement home = village.Settlement;
            Settlement town = village.TradeBound;
            if (town == null || !town.IsTown || town.Town == null || town.IsUnderSiege) return;
            bool looted = village.VillageState == Village.VillageStates.Looted;
            if ((village.VillageState != Village.VillageStates.Normal && !looted) || home.Party?.MapEvent != null) return;
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
            int level = village.GetHearthLevel();
            float madeADay = 0f, goodsADay = 0f;
            foreach (var (item, each) in village.VillageType.Productions)
            {
                if (item == null) continue;
                float rate = item.IsMountable || looted
                    ? each * (level + 1) * 0.5f
                    : model.CalculateDailyProductionAmount(village, item).ResultNumber;
                rate = TradeMath.Finite(rate, 0f);
                if (rate <= 0f) continue;
                made[At(item)] += rate;
                madeADay += rate;
                goodsADay += rate;
            }
            float food = looted ? level + 1f : TradeMath.Finite(model.CalculateDailyFoodProductionAmount(village), 0f);
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
            _villageStore[home.StringId] = TradeMath.AVillagesWarehouse(madeADay);

            var weight = new float[items.Count];
            var onHoof = new bool[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                weight[i] = items[i].Weight;
                onHoof[i] = items[i].HasHorseComponent;
            }
            float flat = TradeMath.EtaDays(RoadBetween(home, town), speed);
            _ridden.TryGetValue((home.StringId, town.StringId), out Ridden learned);
            float ride = TradeMath.TheRideToCount(learned?.Out, flat, _outRatios);
            float rideBack = TradeMath.TheRideToCount(learned?.Back, flat, _backRatios);
            float back = WhenTheVillagersAreHome(villagers, home, town, ride, rideBack, speed);
            int ideal = TradeMath.IdealVillagers(
                Campaign.Current.Models.PartySizeLimitModel.MinimumNumberOfVillagersAtVillagerParty, village.Hearth, goodsADay);
            float room = villagers != null
                ? TradeMath.RoomOnceToppedUp(villagers.InventoryCapacity, villagers.MemberRoster.TotalManCount, ideal,
                                             village.Hearth)
                : TradeMath.RoomOfANewParty(MenANewPartySetsOutWith(village), ideal, village.Hearth);
            int heals = looted ? TradeMath.TicksAVillageHealsIn(home.SettlementHitPoints, HealsADay(village)) : 0;
            float[] scale = TradeMath.WhatAVillageMakesEachTick(village.Hearth, village.HearthChange, looted, heals,
                                                                (int)Math.Ceiling(watch) + 1);
            bool known = _villageMadeAt.TryGetValue(home.StringId, out double madeAt);
            float next = known
                ? TradeMath.DaysToAVillagesNextTick(CampaignTime.Now.ToDays, madeAt)
                : TradeMath.AVillagesTickUnseen;
            if (looted) _villagesLooted++;
            List<TradeMath.VillageLoad> loads = TradeMath.WhatAVillageSends(
                held.ToArray(), made.ToArray(), weight, onHoof, room, back, villagers != null, ride, watch, next, scale,
                rideBack);
            if (loads.Count == 0) return;

            _villagesFollowed++;
            if (known) _villageHoursKnown++;
            if (looted) _villagesHealing++;
            foreach (TradeMath.VillageLoad load in loads) _villageLoadsAhead.Add((home, town, villagers, items, load));
            NoteTheNextTrips(home, villagers != null && (villagers.CurrentSettlement == town || villagers.TargetSettlement == town),
                             loads);
        }

        private static void NoteTheNextTrips(Settlement home, bool inTown, List<TradeMath.VillageLoad> loads)
        {
            if (home?.StringId == null || loads == null) return;
            float stay = TradeMath.DaysUntilARollTakes(TradeMath.VillagersLeaveATownAnHourWith);
            var trips = new List<(float, float, float, float, float[], float)>();
            for (int k = 0; k < loads.Count; k++)
            {
                TradeMath.VillageLoad load = loads[k];
                bool stayed = k == 0 && inTown && Math.Abs(load.Ready - load.Back) < 0.0001f;
                float[] dice = k > 0
                    ? null
                    : load.Unraised
                        ? new[] { TradeMath.VillagersSetOutAnHourWith, TradeMath.VillagersSetOutAnHourWith }
                        : stayed
                            ? new[] { TradeMath.VillagersLeaveATownAnHourWith, TradeMath.VillagersSetOutAnHourWith }
                            : new[] { TradeMath.VillagersSetOutAnHourWith };
                float said = k > 0 ? float.NaN : load.SetOut - load.Ready + (stayed ? stay : 0f);
                trips.Add((load.Days, load.SetOut, load.Ready, load.Back, dice, said));
            }
            _nextTrips[home.StringId] = trips;
        }

        internal static bool TheNextTripDueIn(string homeId, float days, out float setOut, out float ready, out float back,
                                              out float[] dice, out float diceSaid)
        {
            setOut = ready = back = diceSaid = float.NaN;
            dice = null;
            if (homeId == null || !_nextTrips.TryGetValue(homeId, out var trips) || trips.Count == 0) return false;
            int best = 0;
            for (int k = 1; k < trips.Count; k++)
                if (Math.Abs(trips[k].days - days) < Math.Abs(trips[best].days - days)) best = k;
            (_, setOut, ready, back, dice, diceSaid) = trips[best];
            return true;
        }

        private static void ARoadVillagersSale(MobileParty party, Settlement bound, float days, float trust)
        {
            ItemRoster carried = party.ItemRoster;
            if (bound.Town == null || carried == null) return;
            ItemObject ridden = ThePackAnimalVillagersKeep(carried);
            var load = new List<(ItemObject item, int units)>();
            for (int k = carried.Count - 1; k >= 0; k--)
            {
                ItemObject item = carried.GetItemAtIndex(k);
                int amount = carried.GetElementNumber(k);
                if (item == null || amount <= 0) continue;
                if (item == ridden) amount -= (int)(0.5f * party.MemberRoster.TotalManCount);
                if (amount > 0) load.Add((item, amount));
            }
            SellWhatVillagersBring(bound, party, days, trust, load, TradeMath.TillDraw.Villagers,
                                   Naming ? party.HomeSettlement?.Name?.ToString() ?? party.Name?.ToString() : null,
                                   party.HomeSettlement?.StringId ?? party.StringId);
        }

        private static void ANextTripsSale(Settlement home, Settlement town, MobileParty villagers, List<ItemObject> items,
                                           TradeMath.VillageLoad load, float trust)
        {
            var sold = new List<(ItemObject item, int units)>();
            for (int i = items.Count - 1; i >= 0; i--)
            {
                int units = (int)Math.Round(load.Units[i], MidpointRounding.AwayFromZero);
                if (items[i]?.ItemCategory != null && units > 0) sold.Add((items[i], units));
            }
            int worth = SellWhatVillagersBring(town, VillagersToPriceBy(home, villagers), load.Days, trust, sold,
                                               TradeMath.TillDraw.VillagesNextTrips,
                                               Naming ? home.Name?.ToString() : null, home.StringId);
            if (worth <= 0) return;
            _villageWorth = TradeMath.AddedUp(_villageWorth, worth);
            _villageLoads++;
            if (load.Days < _villageSoonest) _villageSoonest = load.Days;
        }

        private static readonly Dictionary<string, MobileParty> _villagersLike =
            new Dictionary<string, MobileParty>(StringComparer.Ordinal);

        private static MobileParty VillagersToPriceBy(Settlement home, MobileParty villagers)
        {
            string faction = home?.MapFaction?.StringId;
            if (villagers != null || faction == null) return villagers;
            if (_villagersLike.TryGetValue(faction, out MobileParty like)) return like;
            MBReadOnlyList<MobileParty> all = MobileParty.AllVillagerParties;
            for (int i = 0; all != null && i < all.Count && like == null; i++)
                if (all[i] != null && all[i].IsActive && all[i].MapFaction?.StringId == faction) like = all[i];
            for (int i = 0; all != null && i < all.Count && like == null; i++)
                if (all[i] != null && all[i].IsActive && all[i].MapFaction != null &&
                    !all[i].MapFaction.IsAtWarWith(home.MapFaction))
                    like = all[i];
            _villagersLike[faction] = like;
            return like;
        }

        private static int SellWhatVillagersBring(Settlement bound, MobileParty party, float days, float trust,
                                                  List<(ItemObject item, int units)> load, TradeMath.TillDraw from,
                                                  string who, string id)
        {
            Town town = bound.Town;
            if (town == null) return 0;
            var arriving = new Dictionary<string, (int inStore, float supply, float demand)>(StringComparer.Ordinal);
            int till = town.Gold;
            int landed = 0;
            foreach (var (item, units) in load)
            {
                ItemCategory kind = item?.ItemCategory;
                if (kind == null || units <= 0) continue;
                if (!arriving.TryGetValue(kind.StringId, out var at))
                    arriving[kind.StringId] = at = CategoryAt(town, bound, kind, days, trust, null, 0f);
                int price = PriceAt(town, item, party, true, at, Priced.At(town, item, party, selling: true));
                if (price <= 0) continue;
                int sold = TradeMath.WhatVillagersSell(units, till, price);
                if (sold <= 0) continue;
                arriving[kind.StringId] = (TradeMath.ShelfForAUnitOfAWalk(at.inStore, sold, item.Value, true),
                                           at.supply, at.demand);
                till -= sold * price;
                int draw = NoteTheTillPays(bound, sold * price, days, from, who, id);
                if (!TradePolicy.Priced(item)) continue;
                int worth = TradeMath.WorthOf(sold, item.Value);
                Note(bound, item.StringId, kind, sold, worth, days, draw, (int)from);
                landed = TradeMath.AddedUp(landed, worth);
            }
            return landed;
        }

        private static float HealsADay(Village village)
        {
            Settlement home = village.Settlement;
            var heal = new ExplainedNumber(TradeMath.AVillageHealsADayBy(home.MapFaction?.CurrentTotalStrength ?? 0f));
            if (village.TradeBound?.Town != null)
                PerkHelper.AddPerkBonusForTown(DefaultPerks.Medicine.CleanInfrastructure, village.TradeBound.Town, false,
                                               ref heal);
            Hero leader = home.OwnerClan?.Leader;
            if (leader != null && leader.GetPerkValue(DefaultPerks.Roguery.InBestLight))
                heal.AddFactor(DefaultPerks.Roguery.InBestLight.SecondaryBonus);
            return heal.ResultNumber;
        }

        internal static void ATownTicked(Town town)
        {
            if (town?.Settlement?.StringId == null) return;
            if (Options.Current.ExtendedDebugLogging && !_townTickedAt.ContainsKey(town.Settlement.StringId))
                _firstTicks.Add((Tongue.Named(town.Settlement.Name, town.Settlement.StringId),
                                 TradeMath.HoursTheWorkshopReadingWasOff(TheTownTicksIn(town.Settlement))));
            _townTickedAt[town.Settlement.StringId] = CampaignTime.Now.ToDays;
        }

        private static readonly List<(string town, float off)> _firstTicks = new List<(string, float)>();

        internal static List<(string town, float off)> TheFirstTicksSeen()
        {
            var seen = new List<(string town, float off)>(_firstTicks);
            _firstTicks.Clear();
            return seen;
        }

        internal static int TownsWhoseHourIsKnown(out int towns)
        {
            towns = 0;
            int known = 0;
            MBReadOnlyList<Town> all = Town.AllTowns;
            double now = CampaignTime.Now.ToDays;
            for (int i = 0; all != null && i < all.Count; i++)
            {
                string id = all[i]?.Settlement?.StringId;
                if (id == null) continue;
                towns++;
                if (_townTickedAt.TryGetValue(id, out double at) && !double.IsNaN(TradeMath.DaysSinceATickSeen(now, at)))
                    known++;
            }
            return known;
        }

        internal static void ASettlementTicked(Settlement settlement)
        {
            if (settlement == null || !settlement.IsVillage || settlement.StringId == null) return;
            _villageMadeAt[settlement.StringId] = CampaignTime.Now.ToDays;
        }

        private static float WhenTheVillagersAreHome(MobileParty villagers, Settlement home, Settlement town, float ride,
                                                     float rideBack, float speed)
        {
            if (villagers == null || villagers.CurrentSettlement == home) return 0f;
            float stay = TradeMath.DaysUntilARollTakes(TradeMath.VillagersLeaveATownAnHourWith);
            if (villagers.CurrentSettlement == town) return stay + rideBack;
            if (villagers.TargetSettlement == town)
            {
                if (villagers.StringId == null || !_villagersReach.TryGetValue(villagers.StringId, out float reach) ||
                    !(reach >= 0f) || TradeMath.OutOfReach(reach))
                    reach = TradeMath.TheRideToCount(null,
                        TradeMath.EtaDays(villagers.GetPosition2D.Distance(town.GetPosition2D), speed), _outRatios);
                return reach + stay + rideBack;
            }
            if (villagers.TargetSettlement == home)
            {
                float left = villagers.GetPosition2D.Distance(home.GetPosition2D);
                float rest = TradeMath.WhatIsLeftOfARide(rideBack, left, town.GetPosition2D.Distance(home.GetPosition2D));
                return float.IsNaN(rest) ? TradeMath.TheRideToCount(null, TradeMath.EtaDays(left, speed), _backRatios) : rest;
            }
            return ride;
        }

        private sealed class Ridden
        {
            internal readonly List<float> Out = new List<float>();
            internal readonly List<float> Back = new List<float>();
        }

        private sealed class OnTheRide
        {
            internal string Home;
            internal string Town;
            internal bool Out;
            internal double LeftAt;
            internal bool Spoiled;
        }

        private static readonly Dictionary<(string home, string town), Ridden> _ridden =
            new Dictionary<(string, string), Ridden>();

        private static readonly Dictionary<string, int> _villageStore = new Dictionary<string, int>(StringComparer.Ordinal);

        internal static int TheStoreTradeLordCounts(Settlement village) =>
            village?.StringId != null && _villageStore.TryGetValue(village.StringId, out int holds) ? holds : -1;

        private static readonly Dictionary<string, OnTheRide> _onTheRide =
            new Dictionary<string, OnTheRide>(StringComparer.Ordinal);

        private static readonly List<float> _outRatios = new List<float>();

        private static readonly List<float> _backRatios = new List<float>();

        private const double DaysARideIsWatchedFor = 5d;

        internal static void VillagersLeft(MobileParty party, Settlement left)
        {
            if (party == null || !party.IsVillager || party.StringId == null || left?.StringId == null) return;
            Settlement home = party.HomeSettlement;
            Settlement town = home?.Village?.TradeBound;
            if (home == null || town == null)
            {
                _onTheRide.Remove(party.StringId);
                return;
            }
            if (left == home || left == town)
            {
                _onTheRide[party.StringId] = new OnTheRide
                {
                    Home = home.StringId, Town = town.StringId, Out = left == home, LeftAt = CampaignTime.Now.ToDays
                };
                return;
            }
            if (_onTheRide.TryGetValue(party.StringId, out OnTheRide riding)) riding.Spoiled = true;
        }

        internal static void VillagersEntered(MobileParty party, Settlement entered)
        {
            if (party == null || !party.IsVillager || party.StringId == null || entered?.StringId == null) return;
            if (!_onTheRide.TryGetValue(party.StringId, out OnTheRide riding)) return;
            if (entered.StringId != (riding.Out ? riding.Town : riding.Home))
            {
                riding.Spoiled = true;
                return;
            }
            _onTheRide.Remove(party.StringId);
            if (riding.Spoiled) return;
            Settlement home = Settlement.Find(riding.Home), town = Settlement.Find(riding.Town);
            if (home == null || town == null || Campaign.Current == null) return;
            float flat = TradeMath.EtaDays(RoadBetween(home, town), Campaign.Current.EstimatedAverageVillagerPartySpeed);
            float took = (float)(CampaignTime.Now.ToDays - riding.LeftAt);
            if (!TradeMath.ARideIsLearned(took, flat)) return;
            var key = (riding.Home, riding.Town);
            if (!_ridden.TryGetValue(key, out Ridden learned)) _ridden[key] = learned = new Ridden();
            TradeMath.KeepTheLast(riding.Out ? learned.Out : learned.Back, took, TradeMath.RidesLearnedPerWay);
            TradeMath.KeepTheLast(riding.Out ? _outRatios : _backRatios, took / flat, TradeMath.RideRatiosKept);
        }

        internal static void WatchTheVillagersRide()
        {
            if (_onTheRide.Count == 0 || Campaign.Current == null) return;
            double now = CampaignTime.Now.ToDays;
            var stale = new List<string>();
            foreach (var one in _onTheRide)
                if (now - one.Value.LeftAt > DaysARideIsWatchedFor) stale.Add(one.Key);
            foreach (string id in stale) _onTheRide.Remove(id);
            MBReadOnlyList<MobileParty> all = MobileParty.AllVillagerParties;
            for (int i = 0; all != null && i < all.Count; i++)
            {
                MobileParty party = all[i];
                if (party?.StringId == null || !_onTheRide.TryGetValue(party.StringId, out OnTheRide riding)) continue;
                if (RideCheck.Dodging(party)) riding.Spoiled = true;
            }
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
                   " denars for the towns they sell to; the hour of the day a village makes its goods known for " +
                   _villageHoursKnown + " of them; " + _villagesLooted + " looted village(s) followed until they recover, " +
                   _villagesHealing + " of them sending a load in time" +
                   (_villageLoads > 0
                       ? ", the soonest landing in " +
                         _villageSoonest.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " day(s)"
                       : "") +
                   ", worked out in " +
                   (_villageTicks * 1000d / Stopwatch.Frequency)
                       .ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " ms";
        }

        internal static void ReadThePriceIndexesAsTheCaravansDo()
        {
            ReadThePriceIndexes();
            _readStamp.Stale();
        }

        internal static void ReadThePriceIndexesOnceTheNewGameIsMade() => _indexesAwaitTheNewGame = true;

        internal static void TakeTheCaravansOwnPriceIndexes()
        {
            if (!_indexesAwaitTheNewGame) return;
            _indexesAwaitTheNewGame = false;
            if (!CopyTheCaravansOwnPriceIndexes())
            {
                ReadThePriceIndexes();
                if (!_saidTheIndexesWereRead)
                {
                    _saidTheIndexesWereRead = true;
                    Log.Write("the caravans' own price averages could not be read on this game version, so " +
                              "TradeLord works them out from the towns' prices as the new game starts");
                }
            }
            _readStamp.Stale();
        }

        private static bool CopyTheCaravansOwnPriceIndexes()
        {
            CaravansCampaignBehavior caravans = Campaign.Current?.GetCampaignBehavior<CaravansCampaignBehavior>();
            Type data = HarmonyLib.AccessTools.Inner(typeof(CaravansCampaignBehavior), "PriceIndexData");
            System.Reflection.FieldInfo land = HarmonyLib.AccessTools.Field(typeof(CaravansCampaignBehavior), "_priceDictionary");
            System.Reflection.FieldInfo coast =
                HarmonyLib.AccessTools.Field(typeof(CaravansCampaignBehavior), "_coastalPriceDictionary");
            System.Reflection.FieldInfo average = data == null ? null : HarmonyLib.AccessTools.Field(data, "AverageBuySellPriceIndex");
            System.Reflection.FieldInfo lowest = data == null ? null : HarmonyLib.AccessTools.Field(data, "MinBuySellPriceIndex");
            if (caravans == null || land == null || coast == null || average == null || lowest == null) return false;
            var onLand = new Dictionary<string, (float average, float lowest)>(StringComparer.Ordinal);
            var onTheCoast = new Dictionary<string, (float average, float lowest)>(StringComparer.Ordinal);
            if (!CopyIndexes(land.GetValue(caravans) as System.Collections.IDictionary, average, lowest, onLand) ||
                !CopyIndexes(coast.GetValue(caravans) as System.Collections.IDictionary, average, lowest, onTheCoast))
                return false;
            _indexes.Clear();
            _coastIndexes.Clear();
            foreach (var one in onLand) _indexes[one.Key] = one.Value;
            foreach (var one in onTheCoast) _coastIndexes[one.Key] = one.Value;
            return true;
        }

        private static bool CopyIndexes(System.Collections.IDictionary held, System.Reflection.FieldInfo average,
                                        System.Reflection.FieldInfo lowest,
                                        Dictionary<string, (float average, float lowest)> into)
        {
            if (held == null || held.Count == 0) return false;
            foreach (System.Collections.DictionaryEntry one in held)
            {
                if (!(one.Key is ItemCategory kind) || kind.StringId == null || one.Value == null) continue;
                if (!kind.IsTradeGood && !kind.IsAnimal) continue;
                if (!(average.GetValue(one.Value) is float mean) || !(lowest.GetValue(one.Value) is float least))
                    return false;
                into[kind.StringId] = (mean, least);
            }
            return into.Count > 0;
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

        private static void FollowTheCaravans()
        {
            _caravansFollowed = _caravansAhead.Count;
            _caravansWaiting = 0;
            _caravansMet = 0;
            _caravanTicks = 0L;
            int inTown = _caravansAhead.Count, onFoot = inTown + _caravansInTown.Count,
                fromHome = onFoot + _villagersAhead.Count;
            var arrive = new List<float>(fromHome + _villageLoadsAhead.Count);
            foreach (var one in _caravansAhead) arrive.Add(one.days);
            foreach (var (party, _) in _caravansInTown)
            {
                float leaves = TradeMath.DaysUntilARollTakes(TradeMath.ACaravanLeavesAnHourWith(
                    party.MemberRoster.TotalManCount > 0
                        ? party.MemberRoster.TotalWounded / (float)party.MemberRoster.TotalManCount : 1f));
                if (!float.IsInfinity(leaves)) _caravansWaiting++;
                arrive.Add(leaves);
            }
            foreach (var one in _villagersAhead) arrive.Add(one.days);
            foreach (var one in _villageLoadsAhead) arrive.Add(one.load.Days);
            float trust = TrustEarned();
            foreach (int i in TradeMath.InTheOrderTheyArrive(arrive))
            {
                long started = Stopwatch.GetTimestamp();
                bool caravan = i < onFoot;
                if (i < inTown)
                {
                    var (party, bound, days) = _caravansAhead[i];
                    Guard.Run("Forecast.Caravan", bound, site => FollowACaravan(party, site, days, trust, false));
                }
                else if (i < onFoot)
                {
                    var (party, town) = _caravansInTown[i - inTown];
                    Guard.Run("Forecast.CaravanInTown", town, site => FollowACaravan(party, site, 0f, trust, true));
                }
                else if (i < fromHome)
                {
                    var (party, bound, days) = _villagersAhead[i - onFoot];
                    Guard.Run("Forecast.VillagersOnArrival", bound, site => ARoadVillagersSale(party, site, days, trust));
                }
                else
                {
                    var (home, town, villagers, items, load) = _villageLoadsAhead[i - fromHome];
                    Guard.Run("Forecast.NextTripOnArrival", town,
                              site => ANextTripsSale(home, site, villagers, items, load, trust));
                }
                long took = Stopwatch.GetTimestamp() - started;
                if (caravan) _caravanTicks += took;
                else _villageTicks += took;
            }
        }

        internal static string TheCaravansMeet()
        {
            if (!On) return null;
            Build();
            return "caravans this hour: " + _caravansFollowed + " caravan(s) on the road followed to the market they " +
                   "are bound for, " + _caravansMet + " of them selling at other prices there than today's, by what " +
                   "lands first, the town's daily supply and demand and your own sale, and " + _caravansWaiting +
                   " in a town still to buy before they leave, " +
                   "worked out in " + (_caravanTicks * 1000d / Stopwatch.Frequency)
                       .ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " ms; the workshops took " +
                   Milliseconds(_shopTicks) + " ms and the purses " + Milliseconds(_purseTicks) + " ms, and the " +
                   "towns' daily use stepped as the game steps it took " + Milliseconds(_useTicks) + " ms in all";
        }

        private static bool _yourSaleMoved;

        private static int _yourSaleTakenInAt = int.MinValue;

        internal static void TakeYourSale(Settlement site, float ride, List<(ItemObject item, int units)> sold)
        {
            string wasAt = _yourSaleAt;
            int was = TradeMath.WorthOfAll(_yourSaleWorth);
            _yourSaleAt = null;
            _yourSaleWhen = double.NaN;
            _yourSaleWorth.Clear();
            if (site?.StringId != null && sold != null && ride >= 0f && !TradeMath.OutOfReach(ride))
            {
                foreach (var (item, units) in sold)
                {
                    if (item?.ItemCategory == null || units <= 0) continue;
                    _yourSaleWorth.TryGetValue(item.ItemCategory.StringId, out int had);
                    _yourSaleWorth[item.ItemCategory.StringId] = TradeMath.AddedUp(had, TradeMath.WorthOf(units, item.Value));
                }
                if (_yourSaleWorth.Count > 0)
                {
                    _yourSaleAt = site.StringId;
                    _yourSaleWhen = CampaignTime.Now.ToDays + ride;
                }
            }
            if (TradeMath.YourSaleMovedEnough(wasAt, was, _yourSaleAt, TradeMath.WorthOfAll(_yourSaleWorth)))
                _yourSaleMoved = true;
        }

        internal static void TakeInYourNewSale()
        {
            if (!_yourSaleMoved) return;
            int hour = (int)CampaignTime.Now.ToHours;
            if (hour == _yourSaleTakenInAt) return;
            _yourSaleMoved = false;
            _yourSaleTakenInAt = hour;
            _yourSaleTakenIn++;
            _readStamp.Stale();
        }

        private static int _yourSaleTakenIn;

        internal static int TimesYourSaleWasTakenIn()
        {
            int taken = _yourSaleTakenIn;
            _yourSaleTakenIn = 0;
            return taken;
        }

        internal static int YourSaleCountedFor(Settlement site, float days, ICollection<string> kinds)
        {
            if (!On || _yourSaleAt == null || site?.StringId != _yourSaleAt || kinds == null) return 0;
            if (MobileParty.MainParty?.CurrentSettlement == site) return 0;
            float due = (float)Math.Max(0d, _yourSaleWhen - CampaignTime.Now.ToDays);
            long worth = 0L;
            foreach (var one in _yourSaleWorth) if (one.Value > 0 && kinds.Contains(one.Key)) worth += one.Value;
            return (int)Math.Min(int.MaxValue, Math.Round(worth * (double)TradeMath.YourSaleCountsFor(due, days),
                                                          MidpointRounding.AwayFromZero));
        }

        private static int YourSaleBy(Settlement site, string kind, float days)
        {
            if (_yourSaleAt == null || site?.StringId != _yourSaleAt || kind == null) return 0;
            if (MobileParty.MainParty?.CurrentSettlement == site) return 0;
            if (!_yourSaleWorth.TryGetValue(kind, out int worth)) return 0;
            float due = (float)Math.Max(0d, _yourSaleWhen - CampaignTime.Now.ToDays);
            return (int)Math.Round(worth * (double)TradeMath.YourSaleCountsFor(due, days), MidpointRounding.AwayFromZero);
        }

        private static List<(ItemCategory kind, ItemCategory substitute, float factor)> Substitutes()
        {
            if (_substitutes != null) return _substitutes;
            _substitutes = new List<(ItemCategory, ItemCategory, float)>();
            MBReadOnlyList<ItemCategory> kinds = ItemCategories.All;
            for (int i = 0; kinds != null && i < kinds.Count; i++)
            {
                ItemCategory kind = kinds[i];
                if (kind?.CanSubstitute != null) _substitutes.Add((kind, kind.CanSubstitute, kind.SubstitutionFactor));
            }
            return _substitutes;
        }

        private static int ShelfShiftAt(Settlement site, ItemCategory kind, float days, float trust)
        {
            ItemObject stands = StandsForAt(site, kind);
            int shift = stands == null ? 0 : TradeMath.WorthShiftTrusted(WorthShift(site, stands, days), trust);
            return TradeMath.AddedUp(shift, YourSaleBy(site, kind.StringId, days));
        }

        private static (int inStore, float supply, float demand) CategoryAt(Town town, Settlement site, ItemCategory kind,
                                                                            float days, float trust,
                                                                            Dictionary<string, int> own, float ownAt)
        {
            var economy = Campaign.Current.Models.SettlementEconomyModel;
            var group = new List<ItemCategory> { kind };
            var pairs = new List<(int kind, int substitute, float factor)>();
            foreach (var (one, other, factor) in Substitutes())
            {
                if (one != kind && other != kind) continue;
                if (!group.Contains(one)) group.Add(one);
                if (!group.Contains(other)) group.Add(other);
                pairs.Add((group.IndexOf(one), group.IndexOf(other), factor));
            }
            int n = group.Count;
            var data = new ItemData[n];
            var supply = new float[n];
            var demand = new float[n];
            for (int g = 0; g < n; g++)
            {
                data[g] = town.MarketData.GetCategoryData(group[g]);
                supply[g] = data[g].Supply;
                demand[g] = data[g].Demand;
            }
            int InStoreAt(int g, float at)
            {
                int mine = own != null && ownAt <= at && own.TryGetValue(group[g].StringId, out int sold) ? sold : 0;
                return TradeMath.ShelfAfterLanding(data[g].InStoreValue,
                                                   TradeMath.AddedUp(ShelfShiftAt(site, group[g], at, trust), mine));
            }
            bool ownedHere = false;
            for (int g = 0; own != null && g < n; g++) ownedHere |= own.ContainsKey(group[g].StringId);
            var wanted = new float[n];
            for (int g = 0; g < n; g++) wanted[g] = economy.GetEstimatedDemandForCategory(town, data[g], group[g]);
            var shelves = new int[n];
            List<float> ticks = TradeMath.TownTicksBefore(TheTownTicksIn(site), days);
            var key = (site.StringId, kind.StringId);
            _ticksAhead.TryGetValue(key, out List<(float[] supply, float[] demand)> kept);
            for (int j = 0; j < ticks.Count; j++)
            {
                bool mine = ownedHere && ownAt <= ticks[j];
                if (!mine && kept != null && j < kept.Count)
                {
                    Array.Copy(kept[j].supply, supply, n);
                    Array.Copy(kept[j].demand, demand, n);
                    continue;
                }
                for (int g = 0; g < n; g++) shelves[g] = InStoreAt(g, ticks[j]);
                TradeMath.StepSupplyAndDemand(supply, demand, shelves, wanted, pairs,
                                              (g, shelf, want, was, wasWanted) =>
                                                  economy.GetSupplyDemandForCategory(town, group[g], shelf, want, was,
                                                                                     wasWanted));
                if (!TradeMath.ATickMayBeKept(own != null, ticks[j], ownAt)) continue;
                if (kept == null) _ticksAhead[key] = kept = new List<(float[], float[])>();
                if (kept.Count == j) kept.Add(((float[])supply.Clone(), (float[])demand.Clone()));
            }
            return (InStoreAt(0, days), supply[0], demand[0]);
        }

        private static float IndexAt(Town town, ItemCategory kind, (int inStore, float supply, float demand) at)
        {
            float now = town.GetItemCategoryPriceIndex(kind);
            ItemData data = town.MarketData.GetCategoryData(kind);
            if (at.inStore == data.InStoreValue && at.supply == data.Supply && at.demand == data.Demand) return now;
            TradeItemPriceFactorModel model = Campaign.Current.Models.TradeItemPriceFactorModel;
            if (!TradeMath.SameIndex(model.GetBasePriceFactor(kind, data.InStoreValue, data.Supply, data.Demand, false, 0),
                                     now))
                return now;
            return model.GetBasePriceFactor(kind, at.inStore, at.supply, at.demand, false, 0);
        }

        private static int PriceAt(Town town, ItemObject item, MobileParty caravan, bool selling,
                                   (int inStore, float supply, float demand) at, int now)
        {
            if (now <= 0 || item?.ItemCategory == null) return now;
            ItemData data = town.MarketData.GetCategoryData(item.ItemCategory);
            if (at.inStore == data.InStoreValue && at.supply == data.Supply && at.demand == data.Demand) return now;
            TradeItemPriceFactorModel model = Campaign.Current.Models.TradeItemPriceFactorModel;
            var el = new EquipmentElement(item);
            PartyBase merchant = Priced.Merchant(town.Settlement);
            if (model.GetPrice(el, caravan, merchant, selling, data.InStoreValue, data.Supply, data.Demand) != now)
                return now;
            int then = model.GetPrice(el, caravan, merchant, selling, at.inStore, at.supply, at.demand);
            return then > 0 ? then : now;
        }

        private static bool Moved(Town town, ItemCategory kind, (int inStore, float supply, float demand) at)
        {
            ItemData data = town.MarketData.GetCategoryData(kind);
            return at.inStore != data.InStoreValue || at.supply != data.Supply || at.demand != data.Demand;
        }

        internal static (ItemCategory kind, float now, float said)? WhatACaravanWasCountedToMeet(string caravan,
                                                                                                Settlement site)
        {
            if (!On || caravan == null || site == null) return null;
            Build();
            if (!_caravanMeets.TryGetValue(caravan, out var met) || met.site != site.StringId || met.kind == null)
                return null;
            return (met.kind, met.now, met.said);
        }

        private static bool PricedByTheModel(Town town, ItemObject item, MobileParty party, bool selling, int now)
        {
            if (now <= 0 || item?.ItemCategory == null) return false;
            ItemData data = town.MarketData.GetCategoryData(item.ItemCategory);
            return Campaign.Current.Models.TradeItemPriceFactorModel.GetPrice(
                       new EquipmentElement(item), party, Priced.Merchant(town.Settlement), selling, data.InStoreValue,
                       data.Supply, data.Demand) == now;
        }

        private static int WhatAWalkFetchesThere(Town town, ItemObject item, MobileParty party, bool selling,
                                                 (int inStore, float supply, float demand) at, int units, int first)
        {
            if (units <= 0) return 0;
            if (!PricedByTheModel(town, item, party, selling, Priced.At(town, item, party, selling)))
                return TradeMath.WorthOf(units, first);
            TradeItemPriceFactorModel model = Campaign.Current.Models.TradeItemPriceFactorModel;
            var el = new EquipmentElement(item);
            PartyBase merchant = Priced.Merchant(town.Settlement);
            return TradeMath.WhatALongWalkFetches(units, i => i == 0 ? first
                : model.GetPrice(el, party, merchant, selling,
                                 TradeMath.ShelfForAUnitOfAWalk(at.inStore, i, item.Value, selling), at.supply, at.demand));
        }

        private static List<(EquipmentElement el, int amount)> _tradedLast;

        private static int[] _fetchedLast;

        internal static int[] WhatTheTradesFetched(Town town, MobileParty party, List<(EquipmentElement el, int amount)> traded)
        {
            if (town?.MarketData == null || traded == null) return null;
            if (ReferenceEquals(traded, _tradedLast) && _fetchedLast != null && _fetchedLast.Length == traded.Count)
                return _fetchedLast;
            var fetched = new int[traded.Count];
            TradeItemPriceFactorModel model = Campaign.Current.Models.TradeItemPriceFactorModel;
            var shelf = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = traded.Count - 1; i >= 0; i--)
            {
                var (el, amount) = traded[i];
                ItemObject item = el.Item;
                var (units, sold) = TradeMath.WhatACaravanMoved(amount);
                if (item?.ItemCategory == null || units <= 0) continue;
                ItemData data = town.MarketData.GetCategoryData(item.ItemCategory);
                if (!shelf.TryGetValue(item.ItemCategory.StringId, out int after)) after = data.InStoreValue;
                int before = TradeMath.ShelfForAUnitOfAWalk(after, units, item.Value, !sold);
                int now = Priced.AsTheGameChargesACaravan(town, el, party, sold);
                fetched[i] = model.GetPrice(el, party, null, sold, data.InStoreValue, data.Supply, data.Demand) != now
                    ? TradeMath.WorthOf(units, now)
                    : TradeMath.WhatAWalkFetches(units, u => model.GetPrice(
                          el, party, null, sold, TradeMath.ShelfForAUnitOfAWalk(before, u, item.Value, sold), data.Supply,
                          data.Demand));
                shelf[item.ItemCategory.StringId] = before;
            }
            _tradedLast = traded;
            _fetchedLast = fetched;
            return fetched;
        }

        private static void FollowACaravan(MobileParty caravan, Settlement bound, float days, float trust, bool inTown)
        {
            Town town = bound.IsTown ? bound.Town : null;
            ItemRoster carried = caravan.ItemRoster;
            if (town == null || carried == null) return;
            var arriving = new Dictionary<string, (int inStore, float supply, float demand)>(StringComparer.Ordinal);
            (int inStore, float supply, float demand) OnArrival(ItemCategory kind)
            {
                if (!arriving.TryGetValue(kind.StringId, out var at))
                    arriving[kind.StringId] = at = CategoryAt(town, bound, kind, days, trust, null, 0f);
                return at;
            }
            bool moved = false;
            ItemCategory biggest = null;
            long biggestGold = 0L;
            float biggestNow = 0f, biggestSaid = 0f;
            bool convoy = caravan.HasNavalNavigationCapability;
            bool onLand = caravan.HasLandNavigationCapability;
            var indexes = caravan.CaravanPartyComponent != null &&
                          caravan.CaravanPartyComponent.CanHaveNavalNavigationCapability ? _coastIndexes : _indexes;
            int men = caravan.MemberRoster.TotalManCount;
            int hauling = carried.NumberOfPackAnimals, herd = carried.NumberOfLivestockAnimals;
            var held = new Dictionary<string, int>(StringComparer.Ordinal);
            var sold = new int[carried.Count];
            var soldHere = new Dictionary<string, int>(StringComparer.Ordinal);
            var landing = new List<(ItemObject item, int units, int draw)>();
            for (int k = 0; k < carried.Count; k++)
            {
                ItemObject item = carried.GetItemAtIndex(k);
                if (item?.ItemCategory == null) continue;
                held.TryGetValue(item.ItemCategory.StringId, out int had);
                held[item.ItemCategory.StringId] = TradeMath.AddedUp(had, TradeMath.WorthOf(carried.GetElementNumber(k),
                                                                                          item.Value + 10));
            }

            var economy = Campaign.Current.Models.SettlementEconomyModel;
            bool budgetFollowsTheIndex = economy.GetType() == typeof(DefaultSettlementEconomyModel);
            int till = town.Gold;
            long earned = 0L;
            float shed = 0f;
            for (int horses = 0; horses < 2 && !inTown; horses++)
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
                    var at = OnArrival(kind);
                    int price = PriceAt(town, item, caravan, true, at, Priced.At(town, item, caravan, selling: true));
                    if (price <= 0) continue;
                    held.TryGetValue(kind.StringId, out int worth);
                    float now = town.GetItemCategoryPriceIndex(kind);
                    float factor = IndexAt(town, kind, at);
                    float budget = economy.CalculateDailySettlementBudgetForItemCategory(town, at.demand, kind);
                    if (budgetFollowsTheIndex) budget = TradeMath.BudgetAtAnotherIndex(budget, factor, now);
                    int units = onLand && haul ? haulOver
                        : onLand && livestock && herdOver > 0 ? herdOver
                        : TradeMath.WhatACaravanUnloads(
                              amount, factor, index.average, index.lowest, worth, budget,
                              item.Value, price, convoy, haul);
                    units = Math.Min(units, Math.Min(amount, till / price));
                    if (units <= 0) continue;
                    moved |= !TradeMath.SameIndex(factor, now);
                    int gold = WhatAWalkFetchesThere(town, item, caravan, true, at, units, price);
                    sold[k] = units;
                    arriving[kind.StringId] = (TradeMath.ShelfForAUnitOfAWalk(at.inStore, units, item.Value, true),
                                               at.supply, at.demand);
                    till -= gold;
                    soldHere.TryGetValue(kind.StringId, out int soldBefore);
                    soldHere[kind.StringId] = TradeMath.AddedUp(soldBefore, TradeMath.WorthOf(units, item.Value));
                    if (gold > biggestGold)
                    {
                        biggestGold = gold;
                        biggest = kind;
                        biggestNow = now;
                        biggestSaid = factor;
                    }
                    int draw = NoteTheTillPays(bound, gold, days, TradeMath.TillDraw.Caravans,
                                               Naming ? caravan.Name + " (" + caravan.StringId + ")" : null,
                                               caravan.StringId);
                    earned += gold;
                    shed += units * item.Weight;
                    if (haul) hauling -= units;
                    else if (livestock) herd -= units;
                    if (TradePolicy.Priced(item)) landing.Add((item, units, draw));
                }

            void Land()
            {
                foreach (var (item, units, draw) in landing)
                    Note(bound, item.StringId, item.ItemCategory, units, TradeMath.WorthOf(units, item.Value), days, draw,
                         (int)TradeMath.TillDraw.Caravans);
            }
            if (biggest != null && caravan.StringId != null)
                _caravanMeets[caravan.StringId] = (bound.StringId, biggest, biggestNow, biggestSaid);
            if (moved) _caravansMet++;
            for (int k = 0; k < carried.Count; k++)
            {
                ItemObject item = carried.GetItemAtIndex(k);
                if (item?.ItemCategory == null || sold[k] <= 0) continue;
                held[item.ItemCategory.StringId] -= TradeMath.WorthOf(sold[k], item.Value + 10);
            }

            float wounded = men > 0 ? caravan.MemberRoster.TotalWounded / (float)men : 1f;
            float buyAt = days + TradeMath.DaysUntilARollTakes(TradeMath.ACaravanLeavesAnHourWith(wounded));
            if (float.IsInfinity(buyAt) || float.IsNaN(buyAt))
            {
                Land();
                return;
            }
            int purse = (int)Math.Min(int.MaxValue, caravan.PartyTradeGold + earned);
            float weight = caravan.TotalWeightCarried - shed;
            int capacity = caravan.InventoryCapacity;
            float budgetFactor = TradeMath.ACaravansBudgetFactor(purse);
            float room = TradeMath.ACaravansRoomFactor(weight, capacity);
            var worthOf = new Dictionary<string, float>(StringComparer.Ordinal);
            var leaving = new Dictionary<string, ((int inStore, float supply, float demand) at, int stock)>(StringComparer.Ordinal);
            var wanted = new List<(ItemCategory kind, float worth, int at)>();
            MBReadOnlyList<ItemCategory> kinds = ItemCategories.All;
            for (int at = 0; kinds != null && at < kinds.Count; at++)
            {
                ItemCategory kind = kinds[at];
                if (kind == null || (!kind.IsTradeGood && !kind.IsAnimal)) continue;
                if (!indexes.TryGetValue(kind.StringId, out var index)) continue;
                ItemObject stands = StandsForAt(bound, kind);
                var then = CategoryAt(town, bound, kind, buyAt, trust, soldHere, days);
                int stock = TradeMath.StockAfterAShelfShift(town.MarketData.GetItemCountOfCategory(kind),
                                                            town.MarketData.GetCategoryData(kind).InStoreValue,
                                                            then.inStore, stands?.Value ?? 0);
                if (stock <= 0) continue;
                leaving[kind.StringId] = (then, stock);
                held.TryGetValue(kind.StringId, out int worth);
                float wants = TradeMath.WhatACaravanThinksAKindIsWorth(
                    IndexAt(town, kind, then), index.average, worth, then.demand,
                    AverageValueOf(kind), budgetFactor, room, kind.Properties == ItemCategory.Property.BonusToFoodStores,
                    onLand && kind == DefaultItemCategories.PackAnimal);
                worthOf[kind.StringId] = wants;
                wanted.Add((kind, wants, at));
            }
            wanted.Sort((x, y) => x.worth != y.worth ? y.worth.CompareTo(x.worth) : x.at.CompareTo(y.at));

            void Buy(ItemCategory kind)
            {
                if (!worthOf.TryGetValue(kind.StringId, out float wants)) return;
                if (!leaving.TryGetValue(kind.StringId, out var shelf)) return;
                if (!kind.IsAnimal && weight / capacity > TradeMath.ACaravanBuysNoGoodsPastThisFull) return;
                int stocked = shelf.stock;
                ItemObject stands = StandsForAt(bound, kind);
                if (stocked <= 0 || stands == null) return;
                int spend = TradeMath.WhatACaravanSpendsOnAKind(
                    purse, wants, Campaign.Current.Models.CaravanModel.GetMaxGoldToSpendOnOneItemCategory(caravan, kind));
                int price = PriceAt(town, stands, caravan, false, shelf.at, Priced.At(town, stands, caravan, selling: false));
                int most = Campaign.Current.Models.CaravanModel.MaxNumberOfItemsToBuyFromSingleCategory;
                bool livestock = stands.HorseComponent != null && stands.HorseComponent.IsLiveStock;
                bool haul = stands.HorseComponent != null && stands.HorseComponent.IsPackAnimal;
                if (onLand && livestock) most = Math.Min(most, (int)(men * TradeMath.ACaravanDrivesThisManyAnimalsAMan) - herd);
                else if (onLand && haul) most = Math.Min(most, men - hauling);
                int units = TradeMath.WhatACaravanTakesOfAKind(spend, price, stocked, most, stands.Weight, weight,
                                                               capacity, kind.IsAnimal && onLand);
                if (units <= 0) return;
                int cost = WhatAWalkFetchesThere(town, stands, caravan, false, shelf.at, units, price);
                purse -= cost;
                NoteTheTillTakesIn(bound, cost, buyAt);
                weight += units * stands.Weight;
                if (onLand && livestock) herd += units;
                else if (onLand && haul) hauling += units;
                NoteWhatACaravanBuys(bound, kind, TradeMath.WorthOf(units, stands.Value), buyAt);
            }

            int kindsBought = convoy ? TradeMath.AConvoyBuysThisManyKinds : TradeMath.ACaravanBuysThisManyKinds;
            for (int i = 0; i < kindsBought && i < wanted.Count; i++) Buy(wanted[i].kind);
            if (convoy)
            {
                Buy(DefaultItemCategories.Grain);
                Buy(DefaultItemCategories.Fish);
            }
            else if (hauling + herd < men * 2f && hauling < men && purse > 1000)
                Buy(DefaultItemCategories.PackAnimal);
            Land();
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

        private static void WhatTheShopsWillMakeBeforeTheRoadArrives()
        {
            ReadWhatTheShopsWillMake();
            foreach (KeyValuePair<string, ShopsAhead> one in _shopsAhead)
            {
                Settlement site = Settlement.Find(one.Key);
                ShopsAhead shops = one.Value;
                if (site == null || shops?.Days == null) continue;
                var made = new List<Landing>();
                var taken = new List<Draw>();
                for (float at = shops.First; at <= shops.Watch; at += 1f) WorkshopRuns.Day(shops.Days, at, null, made, taken);
                foreach (Landing landing in made)
                    Note(site, shops.Standing[landing.Category].stands.StringId, shops.Kinds[landing.Category], landing.Units,
                         landing.Worth, landing.Days, 0, Projection.FromWorkshops);
                foreach (Draw draw in taken) NoteADraw(site, shops.Kinds[draw.Category], draw.Worth, draw.Days);
            }
            _shopsAhead.Clear();
        }

        private static void ForgetWhatTheShopsWereCountedToMake()
        {
            foreach (List<Landing> listed in _landing.Values) listed.RemoveAll(one => one.From == Projection.FromWorkshops);
            _drawing.Clear();
            _shopsAhead.Clear();
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

        private static ShopBook ReadTheShops(Town town, IWorkshopWarehouseCampaignBehavior warehouse,
                                             Func<ItemCategory, ItemObject> standsFor, Func<ItemCategory, int> usedADay,
                                             bool live)
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
            if (lines.Count == 0) return null;

            Dictionary<string, int> held = WhatTheMarketHolds(site);
            var standing = new Dictionary<string, (ItemObject stands, float supply, float demand)>(StringComparer.Ordinal);
            foreach (var one in kinds)
            {
                ItemObject stands = standsFor(one.Value);
                if (stands == null) continue;
                ItemData data = town.MarketData.GetCategoryData(one.Value);
                held.TryGetValue(one.Key, out int units);
                book.Units[one.Key] = units;
                book.InStore[one.Key] = (int)Math.Min(int.MaxValue, Math.Max(0f, data.InStoreValue));
                book.UnitValue[one.Key] = stands.Value;
                book.UsedADay[one.Key] = usedADay(one.Value);
                standing[one.Key] = (stands, data.Supply, data.Demand);
            }
            var expected = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (ShopLine line in lines)
                foreach (var (kind, _) in line.Outputs)
                {
                    if (standing.ContainsKey(kind) || expected.ContainsKey(kind) ||
                        !kinds.TryGetValue(kind, out ItemCategory category))
                        continue;
                    int paid = WhatTheTownPaysForWhatItsShopsMake(town, category, live);
                    if (paid > 0) expected[kind] = paid;
                }
            foreach (ShopLine line in lines)
                foreach (var (kind, _) in line.Outputs)
                    if (!standing.ContainsKey(kind) && expected.ContainsKey(kind) && !line.Aside.Contains(kind))
                        line.Aside.Add(kind);
            lines.RemoveAll(line => !EveryKindPriced(line.Inputs, standing) ||
                                    !EveryOutputPriced(line.Outputs, standing, expected));
            if (lines.Count == 0) return null;

            TradeItemPriceFactorModel priced = Campaign.Current.Models.TradeItemPriceFactorModel;
            Func<string, int, bool, int> price = (kind, store, selling) =>
            {
                if (!standing.TryGetValue(kind, out var at)) return expected.TryGetValue(kind, out int paid) ? paid : 0;
                return priced.GetPrice(new EquipmentElement(at.stands), null, null, selling, store,
                                       at.supply, at.demand);
            };
            return new ShopBook { Lines = lines, Book = book, Standing = standing, Kinds = kinds, Price = price };
        }

        private sealed class ShopBook
        {
            internal List<ShopLine> Lines;
            internal TownBook Book;
            internal Dictionary<string, (ItemObject stands, float supply, float demand)> Standing;
            internal Dictionary<string, ItemCategory> Kinds;
            internal Func<string, int, bool, int> Price;
        }

        private static void FollowTheShops(Town town, float watch, IWorkshopWarehouseCampaignBehavior warehouse)
        {
            Settlement site = town.Settlement;
            ShopBook read = ReadTheShops(town, warehouse, kind => StandsForAt(site, kind), kind => UsedUpADay(site, kind),
                                         false);
            if (read == null) return;
            List<ShopLine> lines = read.Lines;
            TownBook book = read.Book;
            Dictionary<string, (ItemObject stands, float supply, float demand)> standing = read.Standing;
            Dictionary<string, ItemCategory> kinds = read.Kinds;
            Func<string, int, bool, int> price = read.Price;

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

        private static bool EveryOutputPriced(List<(string category, int count)> said,
                                              Dictionary<string, (ItemObject stands, float supply, float demand)> standing,
                                              Dictionary<string, int> expected)
        {
            foreach (var (category, _) in said)
                if (!standing.ContainsKey(category) && !expected.ContainsKey(category)) return false;
            return true;
        }

        private static readonly Dictionary<string, List<ItemObject>> _shopsCanMake =
            new Dictionary<string, List<ItemObject>>(StringComparer.Ordinal);

        private static readonly Dictionary<(string kind, string culture), (double perValue, int value)> _makeWeighed =
            new Dictionary<(string, string), (double, int)>();

        private static readonly Dictionary<(string site, string kind), int> _makePaid =
            new Dictionary<(string, string), int>();

        private static int WhatTheTownPaysForWhatItsShopsMake(Town town, ItemCategory category, bool live)
        {
            if (town?.Settlement?.StringId == null || category?.StringId == null) return 0;
            var key = (town.Settlement.StringId, category.StringId);
            if (!live && _makePaid.TryGetValue(key, out int known)) return known;
            var (perValue, value) = TheMakeWeighed(category, town.Culture);
            int paid = 0;
            if (perValue > 0d)
            {
                ItemData data = town.MarketData.GetCategoryData(category);
                float factor = Campaign.Current.Models.TradeItemPriceFactorModel.GetBasePriceFactor(
                    category, data.InStoreValue, data.Supply, data.Demand, true, value);
                paid = TradeMath.ATownPaysForAMake(perValue, factor);
            }
            if (!live) _makePaid[key] = paid;
            return paid;
        }

        private static (double perValue, int value) TheMakeWeighed(ItemCategory category, CultureObject culture)
        {
            var key = (category.StringId, culture?.StringId ?? "");
            if (_makeWeighed.TryGetValue(key, out var kept)) return kept;
            if (_shopsCanMake.Count == 0)
                foreach (ItemObject item in Items.All)
                {
                    if (item?.ItemCategory?.StringId == null || item.MultiplayerItem || item.NotMerchandise ||
                        item.IsCraftedByPlayer)
                        continue;
                    if (!_shopsCanMake.TryGetValue(item.ItemCategory.StringId, out List<ItemObject> listed))
                        _shopsCanMake[item.ItemCategory.StringId] = listed = new List<ItemObject>();
                    listed.Add(item);
                }
            var preferred = new List<(int value, float penalty)>();
            var every = new List<(int value, float penalty)>();
            TradeItemPriceFactorModel model = Campaign.Current.Models.TradeItemPriceFactorModel;
            if (_shopsCanMake.TryGetValue(category.StringId, out List<ItemObject> items))
                foreach (ItemObject item in items)
                {
                    var one = (item.Value, model.GetTradePenalty(item, null, null, true, 0f, 0f, 0f));
                    every.Add(one);
                    if (item.Culture == null || item.Culture.StringId == "neutral_culture" || item.Culture == culture)
                        preferred.Add(one);
                }
            kept = TradeMath.TheGamesPickWeighed(preferred.Count > 0 ? preferred : every);
            _makeWeighed[key] = kept;
            return kept;
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
            ItemObject stands = WhatStandsForAt(site, category);
            _standsForAt[key] = stands;
            return stands;
        }

        private static ItemObject WhatStandsForAt(Settlement site, ItemCategory category)
        {
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
            return stocked ?? StandsFor(category);
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

        private static readonly List<double> _runsRead = new List<double>();

        private static double DaysSinceTheTick(Settlement site)
        {
            if (site?.Town == null || site.StringId == null) return double.NaN;
            if (_townTickedAt.TryGetValue(site.StringId, out double ticked))
            {
                double sinceTheTick = TradeMath.DaysSinceATickSeen(CampaignTime.Now.ToDays, ticked);
                if (!double.IsNaN(sinceTheTick)) return sinceTheTick;
            }
            _runsRead.Clear();
            Workshop[] shops = site.Town.Workshops;
            for (int k = 0; shops != null && k < shops.Length; k++)
            {
                Workshop shop = shops[k];
                if (shop?.WorkshopType != null) _runsRead.Add(shop.LastRunCampaignTime.ToDays);
            }
            double now = CampaignTime.Now.ToDays;
            double lastRun = TradeMath.TheRunThatTellsTheTick(_runsRead, now,
                                                              _runSeen.TryGetValue(site.StringId, out double had)
                                                                  ? had
                                                                  : double.NaN);
            double since = double.IsNaN(lastRun) ? double.NaN : now - lastRun;
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
                long sum = 0L;
                MBReadOnlyList<ItemCategory> kinds = ItemCategories.All;
                for (int i = 0; kinds != null && i < kinds.Count; i++)
                {
                    ItemCategory kind = kinds[i];
                    if (kind == null) continue;
                    float inStore = town.MarketData.GetCategoryData(kind).InStoreValue;
                    if (!(inStore > 0f)) continue;
                    ItemObject stands = StandsForAt(site, kind);
                    var (budget, price) = SteppedUse(town, kind, stands, null);
                    sum += TradeMath.WhatATownTakesInForWhatItUses(
                        budget, TradeMath.ShelfAtItsPrice(inStore, price, stands?.Value ?? 0));
                }
                return (int)Math.Min(int.MaxValue, sum);
            }, 0);
            _income[site.StringId] = income;
            return income;
        }

        private static long _useTicks, _shopTicks, _purseTicks;

        private static string Milliseconds(long ticks) =>
            (ticks * 1000d / Stopwatch.Frequency).ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);

        private static (float budget, int price) SteppedUse(Town town, ItemCategory kind, ItemObject stands, TownBook book)
        {
            long started = Stopwatch.GetTimestamp();
            var economy = Campaign.Current.Models.SettlementEconomyModel;
            var at = SteppedAtTheTick(town, kind, book, out _, out float shift);
            float budget = economy.CalculateDailySettlementBudgetForItemCategory(
                town, TradeMath.Finite(economy.GetDailyDemandForCategory(town, kind) + shift, 0f), kind);
            budget = TradeMath.BudgetAtAnotherIndex(budget, IndexAt(town, kind, at), town.GetItemCategoryPriceIndex(kind));
            int price = stands == null ? 0 : PriceAt(town, stands, null, false, at, town.MarketData.GetPrice(stands));
            _useTicks += Stopwatch.GetTimestamp() - started;
            return (budget, price);
        }

        internal sealed class TownTickSaid
        {
            internal readonly List<(ItemCategory kind, int shelf, int afterShops, int said, int stepped)> Uses =
                new List<(ItemCategory, int, int, int, int)>();
            internal readonly List<(ItemCategory kind, float steppedBefore, float now, float stepped)> Shifts =
                new List<(ItemCategory, float, float, float)>();
            internal readonly Dictionary<string, int> Made = new Dictionary<string, int>(StringComparer.Ordinal);
            internal readonly Dictionary<string, int> Taken = new Dictionary<string, int>(StringComparer.Ordinal);
            internal readonly int[] Runs = new int[WorkshopRuns.StopsKnown];
            internal readonly Dictionary<string, string> Names = new Dictionary<string, string>(StringComparer.Ordinal);
            internal readonly Dictionary<string, double> Cleared = new Dictionary<string, double>(StringComparer.Ordinal);
            internal bool Shops;
            internal bool Yours;
        }

        internal static TownTickSaid WhatATownTickShouldDo(Town town)
        {
            Settlement site = town?.Settlement;
            if (!On || site?.StringId == null || town.MarketData == null || Campaign.Current == null) return null;
            long useTicksBefore = _useTicks;
            try { return WhatATownTickWillDo(town, site); }
            finally { _useTicks = useTicksBefore; }
        }

        private static TownTickSaid WhatATownTickWillDo(Town town, Settlement site)
        {
            var said = new TownTickSaid();
            TownBook book = null;
            Workshop[] shopsHere = town.Workshops;
            for (int k = 0; shopsHere != null && k < shopsHere.Length; k++)
                if (shopsHere[k]?.Owner == Hero.MainHero) said.Yours = true;
            if (!town.InRebelliousState && town.Workshops != null)
            {
                IWorkshopWarehouseCampaignBehavior warehouse =
                    Campaign.Current.GetCampaignBehavior<IWorkshopWarehouseCampaignBehavior>();
                ShopBook read = ReadTheShops(town, warehouse, kind => WhatStandsForAt(site, kind), kind => 0, true);
                WorkshopRuns.Days days = read == null ? null : WorkshopRuns.Begin(read.Lines, read.Book, null, null, read.Price);
                if (days != null)
                {
                    WorkshopRuns.Day(days, 0f, null, null, null, said.Runs);
                    foreach (var one in days.MadeNow) said.Made[one.Key] = one.Value;
                    foreach (var one in days.MadeAside)
                    {
                        said.Made.TryGetValue(one.Key, out int had);
                        said.Made[one.Key] = had + one.Value;
                    }
                    foreach (var one in days.TakenNow) said.Taken[one.Key] = one.Value;
                    foreach (var one in days.Cleared) said.Cleared[one.Key] = one.Value;
                    book = read.Book;
                    said.Shops = true;
                    foreach (var one in read.Kinds) said.Names[one.Key] = Tongue.Named(one.Value.GetName(), one.Key);
                }
            }
            Dictionary<string, int> held = WhatTheMarketHolds(site);
            MBReadOnlyList<ItemCategory> all = ItemCategories.All;
            for (int i = 0; all != null && i < all.Count; i++)
            {
                ItemCategory kind = all[i];
                if (kind == null) continue;
                ItemObject stands = WhatStandsForAt(site, kind);
                if (stands == null || stands.Value <= 0 || !TradePolicy.Priced(stands)) continue;
                held.TryGetValue(kind.StringId, out int shelf);
                int afterShops = book != null && book.Units.TryGetValue(kind.StringId, out int left) ? left : shelf;
                if (shelf <= 0 && afterShops <= 0) continue;
                var (budget, price) = SteppedUse(town, kind, stands, book);
                int counted = UnitsUsedUp(TradeMath.WorthUsedUpADay(budget, price, stands.Value), stands.Value, afterShops);
                said.Uses.Add((kind, shelf, afterShops, counted, counted));
            }
            var paired = new List<ItemCategory>();
            foreach (var (one, other, _) in Substitutes())
            {
                if (one != null && !paired.Contains(one)) paired.Add(one);
                if (other != null && !paired.Contains(other)) paired.Add(other);
            }
            foreach (ItemCategory kind in paired)
            {
                SteppedAtTheTick(town, kind, book, out float steppedBefore, out float shift);
                said.Shifts.Add((kind, steppedBefore, shift, shift));
            }
            return said;
        }

        private static int UnitsUsedUp(int worth, int unitValue, int have)
        {
            if (worth <= 0 || unitValue <= 0 || have <= 0) return 0;
            int units = (int)Math.Round((double)worth / unitValue, MidpointRounding.AwayFromZero);
            return units > have ? have : units;
        }

        private static (int inStore, float supply, float demand) SteppedAtTheTick(Town town, ItemCategory kind, TownBook book,
                                                                                 out float steppedBefore, out float shift)
        {
            var economy = Campaign.Current.Models.SettlementEconomyModel;
            var group = new List<ItemCategory> { kind };
            List<(ItemCategory kind, ItemCategory substitute, float factor)> all = Substitutes();
            for (bool grew = true; grew;)
            {
                grew = false;
                foreach (var (one, other, _) in all)
                {
                    bool mine = group.Contains(one), theirs = group.Contains(other);
                    if (mine == theirs) continue;
                    group.Add(mine ? other : one);
                    grew = true;
                }
            }
            var pairs = new List<(int kind, int substitute, float factor)>();
            foreach (var (one, other, factor) in all)
                if (group.Contains(one) && group.Contains(other))
                    pairs.Add((group.IndexOf(one), group.IndexOf(other), factor));
            int n = group.Count;
            var supply = new float[n];
            var demand = new float[n];
            var shelves = new int[n];
            var wanted = new float[n];
            for (int g = 0; g < n; g++)
            {
                ItemData data = town.MarketData.GetCategoryData(group[g]);
                supply[g] = data.Supply;
                demand[g] = data.Demand;
                shelves[g] = book != null && book.InStore.TryGetValue(group[g].StringId, out int store)
                    ? store
                    : (int)Math.Min(int.MaxValue, Math.Max(0f, data.InStoreValue));
                wanted[g] = economy.GetEstimatedDemandForCategory(town, data, group[g]);
            }
            TradeMath.StepSupplyAndDemand(supply, demand, shelves, wanted, null,
                                          (g, shelf, want, was, wasWanted) =>
                                              economy.GetSupplyDemandForCategory(town, group[g], shelf, want, was,
                                                                                 wasWanted));
            steppedBefore = demand[0];
            TradeMath.ShiftsInPairOrder(supply, demand, pairs);
            shift = TradeMath.Finite(demand[0] - steppedBefore, 0f);
            return (shelves[0], supply[0], demand[0]);
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

        internal static List<(string id, string name, float days, int want, int paid)> TheCaravansDrawing(Settlement site,
            TradeMath.TillDraw kind = TradeMath.TillDraw.Caravans)
        {
            if (!On || site == null) return null;
            Build();
            if (!_tillPaysBy.TryGetValue(site.StringId,
                                         out List<(float days, int gold, TradeMath.TillDraw from, string who, string id)> by))
                return null;
            _tillPaid.TryGetValue(site.StringId, out List<(float days, int gold)> paid);
            bool aligned = paid != null && paid.Count == by.Count;
            var first = new Dictionary<string, float>(StringComparer.Ordinal);
            foreach (var one in by)
                if (one.from == kind && one.id != null && one.gold > 0)
                    first[one.id] = first.TryGetValue(one.id, out float soonest) ? Math.Min(soonest, one.days) : one.days;
            var at = new Dictionary<string, int>(StringComparer.Ordinal);
            var drawing = new List<(string id, string name, float days, int want, int paid)>();
            for (int i = 0; i < by.Count; i++)
            {
                var one = by[i];
                if (one.from != kind || one.id == null || one.gold <= 0) continue;
                if (!TradeMath.OnTheSameArrival(one.days, first[one.id])) continue;
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
