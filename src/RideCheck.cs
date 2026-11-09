using System;
using System.Collections.Generic;
using System.Globalization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TradeLord
{
    public class RideWatchBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, OnHourlyTick);
            CampaignEvents.OnSettlementLeftEvent.AddNonSerializedListener(this, OnSettlementLeft);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, OnTownDailyTick);
            CampaignEvents.OnItemProducedEvent.AddNonSerializedListener(this, OnItemProduced);
            CampaignEvents.OnItemConsumedEvent.AddNonSerializedListener(this, OnItemConsumed);
            CampaignEvents.PlayerInventoryExchangeEvent.AddNonSerializedListener(this, OnPlayerInventoryExchange);
        }

        private void OnTownDailyTick(Town town)
        {
            Guard.Run("TownCheck.Before", () => TownCheck.ATownIsAboutToTick(town));
        }

        private void OnItemProduced(ItemObject item, Settlement settlement, int count)
        {
            Guard.Run("TownCheck.Made", () => TownCheck.ItemMade(item, settlement, count));
        }

        private void OnItemConsumed(ItemObject item, Settlement settlement, int count)
        {
            Guard.Run("TownCheck.Used", () => TownCheck.ItemUsed(item, settlement, count));
        }

        private void OnPlayerInventoryExchange(List<(ItemRosterElement, int)> purchased,
                                               List<(ItemRosterElement, int)> sold, bool isTrading)
        {
            if (!isTrading || sold == null) return;
            Guard.Run("RideCheck.YouSold", () => RideCheck.YouSold(sold));
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnHourlyTick()
        {
            Guard.Run("RideCheck.Hourly", RideCheck.Hourly);
        }

        private void OnSettlementLeft(MobileParty party, Settlement settlement)
        {
            if (party == null || settlement == null) return;
            Guard.Run("RideCheck.Left", () => RideCheck.Left(party, settlement));
        }

        private void OnTick(float dt)
        {
            Guard.Run("RideCheck.Tick", dt, RideCheck.Tick);
            Guard.Run("RideCheck.Hour", RideCheck.TheHourOnceWarm);
            Guard.Run("TownCheck.After", TownCheck.Tick);
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party == null || settlement == null) return;
            Guard.Run("RideCheck.Arrived", () => RideCheck.Arrived(party, settlement));
        }

        private void OnDailyTick()
        {
            Guard.Run("RideCheck.Daily", RideCheck.SayTheCaravans);
            Guard.Run("RideCheck.Counted", RideCheck.SayTheCounted);
            Guard.Run("RideCheck.Purses", RideCheck.SayThePurses);
            Guard.Run("RideCheck.Census", RideCheck.SayTheCensus);
            Guard.Run("TownCheck.Daily", TownCheck.SayTheDay);
        }
    }

    internal static class RideCheck
    {
        private const float ShortestJudged = 2f;
        private const float CaravanShortestJudged = 3f;
        private const float CloseEnough = 0.1f;
        private const float CaravanCloseEnough = 0.15f;
        private const double CaravanKeptHours = 15d * 24d;
        private const double StoodStill = 0.05d;

        private struct Open
        {
            internal Settlement To;
            internal double From;
            internal double LastAt;
            internal Travel.Said Said;
            internal float Ceiling;
            internal double Moving;
            internal double Standing;
            internal double NightMoving;
            internal double SnowMoving;
            internal double WetMoving;
            internal double ForestMoving;
            internal double WaterMoving;
            internal double DesertMoving;
            internal float Moved;
            internal bool Fought;
            internal string Carried;
            internal float SlowestSeen;
            internal float QuickestSeen;
            internal double SlowestAt;
            internal bool TillSaid;
            internal int TillThen;
            internal int TillDraw;
            internal int TillTake;
            internal string TillWho;
            internal int TillOnArrival;
            internal int TillPaid;
        }

        private struct Followed
        {
            internal string To;
            internal double At;
            internal float SaidHours;
            internal float StraightHours;
            internal bool Dodged;
        }

        private sealed class Counted
        {
            internal string Id;
            internal string Name;
            internal string Town;
            internal string TownName;
            internal float SaidHours;
            internal int Want;
            internal int Paid;
            internal float Share;
            internal double SetOffAt;
            internal double DueAt;
            internal double YouDueAt;
            internal double ArrivedAt = double.NaN;
            internal string Elsewhere;
            internal int Sold;
            internal bool Villagers;
            internal bool NextTrip;
            internal bool CameWithGoods;
            internal int GoldBefore;
            internal ItemCategory Kind;
            internal float IndexNow = float.NaN;
            internal float IndexSaid = float.NaN;
            internal float IndexMet = float.NaN;
            internal int YourSale;
            internal double YouReachedAt = double.NaN;
            internal bool Aside;
            internal int PurseOnArrival = -1;
            internal double SetOutSaidAt = double.NaN;
            internal double LeftHomeAt = double.NaN;
            internal double ReadyAt = double.NaN;
            internal double BackAt = double.NaN;
            internal float[] Dice;
            internal float DiceSaid = float.NaN;
            internal int StoreAtCount = -1;
            internal int StoreAtLeaving = -1;
            internal int BroughtHome = -1;
            internal bool Brought;
            internal int StoreHolds = -1;
        }

        private struct PurseSaid
        {
            internal string Town;
            internal int Ahead;
            internal int Said;
        }

        private sealed class Visit
        {
            internal string Town;
            internal double At;
            internal readonly Dictionary<ItemObject, int> Held = new Dictionary<ItemObject, int>();
        }

        private static readonly int[] PurseHoursAhead = { 3, 6, 12, 24 };
        private const float PurseCloseEnough = 0.1f;
        private const double VisitKeptHours = 72d;
        private static readonly Dictionary<string, Counted> _counted = new Dictionary<string, Counted>(StringComparer.Ordinal);
        private static int _countedDay, _soldDay, _lateDay, _neverDay;
        private static long _wantSold, _goldSold, _wantLate, _goldLate, _wantNever;
        private static int _indexJudged;
        private static double _indexSaidOff, _indexNowOff;
        private static readonly int[] _groupCount = new int[4];
        private static readonly long[] _groupWant = new long[4], _groupPaid = new long[4], _groupSold = new long[4];
        private static int _asideDay, _purseShortDay;
        private static readonly Dictionary<string, int> _youSoldAt = new Dictionary<string, int>(StringComparer.Ordinal);
        private static readonly List<float> _tripOff = new List<float>();
        private static readonly List<float> _setOffOff = new List<float>();
        private static readonly List<float> _rideOff = new List<float>();
        private static readonly List<float[]> _tripDice = new List<float[]>();
        private static readonly List<string> _beatTheDice = new List<string>();
        private static int _villagersDay, _nextTripsDay, _villagersSold, _villagersLate, _villagersNever;
        private static long _villagersWantSold, _villagersGoldSold, _villagersWantLate, _villagersGoldLate, _villagersWantNever;
        private static readonly Dictionary<int, List<PurseSaid>> _purseDue = new Dictionary<int, List<PurseSaid>>();
        private static readonly Dictionary<int, List<float>> _purseOff = new Dictionary<int, List<float>>();
        private static readonly Dictionary<int, (int over, float worst, string at)> _purseWorst =
            new Dictionary<int, (int, float, string)>();
        private static readonly Dictionary<string, Visit> _visits = new Dictionary<string, Visit>(StringComparer.Ordinal);
        private static int _visitsDay;
        private static long _foodGold, _foodUnits, _horsesInGold, _horsesInUnits, _horsesOutGold, _horsesOutUnits,
                            _lootGold, _lootUnits;
        private static readonly Dictionary<string, long> _censusBy = new Dictionary<string, long>(StringComparer.Ordinal);

        private static Open _ride;
        private static bool _riding;
        private static Settlement _triedTo;
        private static Settlement _brokeOffFrom;
        private static float _brokeOffAfter;
        private static int _judged, _close, _pastCeiling;
        private static float _offTotal;
        private static readonly Dictionary<string, Followed> _followed = new Dictionary<string, Followed>();
        private static readonly List<float> _arrived = new List<float>();
        private static readonly List<float> _arrivedStraight = new List<float>();
        private static readonly List<float> _dodged = new List<float>();

        internal static bool Writing => Options.Current.ExtendedDebugLogging;

        internal static void Forget()
        {
            _hourDue = double.NaN;
            _broughtHome.Clear();
            _ride = default(Open);
            _riding = false;
            _triedTo = null;
            _brokeOffFrom = null;
            _brokeOffAfter = 0f;
            _judged = 0;
            _close = 0;
            _pastCeiling = 0;
            _offTotal = 0f;
            _followed.Clear();
            _arrived.Clear();
            _arrivedStraight.Clear();
            _dodged.Clear();
            _counted.Clear();
            _purseDue.Clear();
            _visits.Clear();
            _youSoldAt.Clear();
            ClearTheCounted();
            ClearThePurses();
            ClearTheCensus();
        }

        internal static bool Dodging(MobileParty party) =>
            party != null && (party.MapEvent != null || party.ShortTermBehavior == AiBehavior.FleeToPoint ||
                              party.ShortTermBehavior == AiBehavior.FleeToGate);

        private static void TurnedAway(Settlement from, float after)
        {
            foreach (Counted one in _counted.Values)
                if (one.Town == from.StringId && double.IsNaN(one.YouReachedAt)) one.Aside = true;
            Marker.NoteATurnAway(after);
            Log.Write("ride check: you turned away from " + Tongue.Named(from.Name, from.StringId) +
                      " after " + Hours(after) + " h, so that ride is not judged");
        }

        private static void TookUpTheRoad(Settlement to)
        {
            Settlement from = _brokeOffFrom;
            if (from == null) return;
            _brokeOffFrom = null;
            if (Marks.TurnedAwayAfterABreak(from.StringId, to.StringId)) TurnedAway(from, _brokeOffAfter);
            else
                Log.Write("ride check: your party took up the road to " + Tongue.Named(from.Name, from.StringId) +
                          " again, so the " + Hours(_brokeOffAfter) + " h before it stopped are not a ride you turned away from");
        }

        internal static void Tick(float dt)
        {
            MobileParty party = MobileParty.MainParty;
            if (!Writing || party == null || Campaign.Current == null)
            {
                _riding = false;
                return;
            }
            double now = CampaignTime.Now.ToHours;
            Settlement to = party.DefaultBehavior == AiBehavior.GoToSettlement ? party.TargetSettlement : null;
            if (_riding && to != _ride.To && party.CurrentSettlement == null)
            {
                _riding = false;
                float after = (float)(now - _ride.From);
                if (to == null)
                {
                    _brokeOffFrom = _ride.To;
                    _brokeOffAfter = after;
                    Log.Write("ride check: your party stopped heading for " + Tongue.Named(_ride.To.Name, _ride.To.StringId) +
                              " after " + Hours(after) + " h, so that ride is not judged");
                }
                else TurnedAway(_ride.To, after);
            }
            if (to != _triedTo && party.CurrentSettlement == null)
            {
                _triedTo = to;
                if (!_riding && to != null)
                {
                    TookUpTheRoad(to);
                    SetOff(to, now);
                }
            }
            if (!_riding) return;
            double step = now - _ride.LastAt;
            _ride.LastAt = now;
            if (step <= 0d) return;
            bool fighting = party.MapEvent != null;
            if (fighting) _ride.Fought = true;
            if (!party.IsMoving || fighting)
            {
                _ride.Standing += step;
                return;
            }
            _ride.Moving += step;
            _ride.Moved += party.Speed * dt;
            float pace = party.Speed;
            if (pace > _ride.QuickestSeen) _ride.QuickestSeen = pace;
            if (pace < _ride.SlowestSeen)
            {
                _ride.SlowestSeen = pace;
                _ride.SlowestAt = now - _ride.From;
            }
            if (party.IsCurrentlyAtSea) return;
            if (Campaign.Current.IsNight) _ride.NightMoving += step;
            Travel.SkyOver(party, out bool wet, out bool snowy);
            if (snowy) _ride.SnowMoving += step;
            if (wet) _ride.WetMoving += step;
            int ground = Travel.GroundUnder(party);
            if (ground == TradeMath.GroundForest) _ride.ForestMoving += step;
            else if (ground == TradeMath.GroundWater) _ride.WaterMoving += step;
            else if (ground == TradeMath.GroundDesert) _ride.DesertMoving += step;
        }

        private static void SetOff(Settlement to, double now)
        {
            Travel.Said said = Travel.WhatWasSaidAbout(to);
            if (!said.Known) return;
            _ride = new Open
            {
                To = to,
                From = now,
                LastAt = now,
                Said = said,
                Ceiling = LedgerBehavior.TravelCeiling(to),
                Carried = Travel.WhatCarriesYou(MobileParty.MainParty),
                SlowestSeen = float.MaxValue
            };
            if (Options.Current.Omniscient && Forecast.On && to.IsTown && to.SettlementComponent != null)
            {
                _ride.TillSaid = true;
                _ride.TillThen = to.SettlementComponent.Gold;
                _ride.TillDraw = Forecast.TillPaysOutBefore(to, said.Days);
                _ride.TillPaid = Forecast.TillPaidOutBefore(to, said.Days);
                _ride.TillTake = Forecast.TillTakesInBefore(to, said.Days);
                _ride.TillWho = Forecast.WhoDrawsTheTill(to, said.Days);
                _ride.TillOnArrival = Forecast.TillOnArrival(to, said.Days);
                CountTheCaravans(to, said.Days, now);
            }
            _riding = true;
            Log.Write("ride check: set off for " + Tongue.Named(to.Name, to.StringId) + "; TradeLord says " +
                      Hours(said.Days * 24f) + " h, " + Hours(said.Night) + " h of it at night, over a road of " +
                      Figure(said.Length) +
                      (said.LandRatio < 0.999f ? " (" + Percent(1f - said.LandRatio) + " of it at sea)" : "") +
                      (said.Walked ? ", measured along the road the game would take" : ", measured by the game's distance") +
                      " at a Speed of " + Figure(said.DayPace) + " by day and " + Figure(said.NightPace) +
                      " by night; " + Hours(said.Snow) + " h of it in snow and " + Hours(said.Wet) +
                      " h on wet ground by the weather forecast, and " + Hours(said.Slow) +
                      " h of it still Disorganized");
            if (_ride.Carried != null) Log.Write("  what carries you: " + _ride.Carried);
        }

        private static readonly Dictionary<string, int> _broughtHome = new Dictionary<string, int>(StringComparer.Ordinal);

        internal static void Arrived(MobileParty party, Settlement at)
        {
            if (Writing && party.IsVillager && party.StringId != null && at == party.HomeSettlement)
                _broughtHome[party.StringId] = UnitsBroughtToSell(party);
            if (party == MobileParty.MainParty) Reached(at);
            else if (party.IsCaravan || party.IsVillager) ACaravanReached(party, at);
            if (party.IsCaravan || party.IsVillager) ACountedCaravanCame(party, at);
            else if (party != MobileParty.MainParty) Entered(party, at);
        }

        private static void CountTheCaravans(Settlement to, float days, double now)
        {
            List<(string id, string name, float days, int want, int paid)> drawing = Forecast.TheCaravansDrawing(to);
            if (drawing == null) return;
            var caravans = new Dictionary<string, MobileParty>(StringComparer.Ordinal);
            foreach (MobileParty party in MobileParty.AllCaravanParties)
                if (party?.StringId != null) caravans[party.StringId] = party;
            foreach (var one in drawing)
            {
                if (one.want <= 0 || _counted.ContainsKey(one.id)) continue;
                _counted[one.id] = new Counted
                {
                    Id = one.id,
                    Name = one.name,
                    Town = to.StringId,
                    TownName = Tongue.Named(to.Name, to.StringId),
                    SaidHours = one.days * 24f,
                    Want = one.want,
                    Paid = one.paid,
                    Share = (float)(TradeMath.TheShareThatCounts(one.want, one.days, days) / one.want),
                    SetOffAt = now,
                    DueAt = now + one.days * 24d,
                    YouDueAt = now + days * 24d,
                    YourSale = Forecast.YourSaleCountedFor(to, one.days, KindsCarried(caravans, one.id))
                };
                var meets = Forecast.WhatACaravanWasCountedToMeet(one.id, to);
                if (meets.HasValue)
                {
                    Counted counted = _counted[one.id];
                    counted.Kind = meets.Value.kind;
                    counted.IndexNow = meets.Value.now;
                    counted.IndexSaid = meets.Value.said;
                }
                _countedDay++;
            }
            CountTheVillagers(to, days, now, TradeMath.TillDraw.Villagers);
            CountTheVillagers(to, days, now, TradeMath.TillDraw.VillagesNextTrips);
        }

        private static HashSet<string> KindsCarried(Dictionary<string, MobileParty> parties, string id) =>
            id != null && parties.TryGetValue(id, out MobileParty party) ? KindsCarried(party, null) : new HashSet<string>();

        private static HashSet<string> KindsCarried(MobileParty party, Settlement store)
        {
            var kinds = new HashSet<string>(StringComparer.Ordinal);
            foreach (ItemRoster roster in new[] { party?.ItemRoster, store?.ItemRoster })
                for (int k = 0; roster != null && k < roster.Count; k++)
                {
                    string kind = roster.GetItemAtIndex(k)?.ItemCategory?.StringId;
                    if (kind != null && roster.GetElementNumber(k) > 0) kinds.Add(kind);
                }
            return kinds;
        }

        private static void CountTheVillagers(Settlement to, float days, double now, TradeMath.TillDraw kind)
        {
            List<(string id, string name, float days, int want, int paid)> drawing = Forecast.TheCaravansDrawing(to, kind);
            if (drawing == null || drawing.Count == 0) return;
            var byHome = new Dictionary<string, MobileParty>(StringComparer.Ordinal);
            foreach (MobileParty party in MobileParty.AllVillagerParties)
                if (party?.StringId != null) byHome[party.HomeSettlement?.StringId ?? party.StringId] = party;
            foreach (var one in drawing)
            {
                if (one.want <= 0 || !byHome.TryGetValue(one.id, out MobileParty party) ||
                    _counted.ContainsKey(party.StringId))
                    continue;
                _counted[party.StringId] = new Counted
                {
                    Id = party.StringId,
                    Name = one.name ?? Tongue.Named(party.Name, party.StringId),
                    Town = to.StringId,
                    TownName = Tongue.Named(to.Name, to.StringId),
                    SaidHours = one.days * 24f,
                    Want = one.want,
                    Paid = one.paid,
                    Share = (float)(TradeMath.TheShareThatCounts(one.want, one.days, days) / one.want),
                    SetOffAt = now,
                    DueAt = now + one.days * 24d,
                    YouDueAt = now + days * 24d,
                    Villagers = true,
                    NextTrip = kind == TradeMath.TillDraw.VillagesNextTrips,
                    GoldBefore = party.PartyTradeGold,
                    YourSale = Forecast.YourSaleCountedFor(to, one.days, KindsCarried(party,
                        kind == TradeMath.TillDraw.VillagesNextTrips ? party.HomeSettlement : null))
                };
                if (kind == TradeMath.TillDraw.VillagesNextTrips &&
                    Forecast.TheNextTripDueIn(one.id, one.days, out float setOut, out float ready, out float back,
                                              out float[] dice, out float diceSaid))
                {
                    Counted trip = _counted[party.StringId];
                    trip.SetOutSaidAt = now + setOut * 24d;
                    trip.ReadyAt = now + ready * 24d;
                    trip.BackAt = now + back * 24d;
                    trip.Dice = dice;
                    trip.DiceSaid = diceSaid * 24f;
                    trip.StoreAtCount = StoreHeld(party.HomeSettlement);
                    trip.StoreHolds = Forecast.TheStoreTradeLordCounts(party.HomeSettlement);
                    trip.Brought = Forecast.TheNextTripCarriesWhatItBroughtHome(one.id, one.days);
                }
                if (kind == TradeMath.TillDraw.VillagesNextTrips) _nextTripsDay++;
                else _villagersDay++;
            }
        }

        private static void ACountedCaravanCame(MobileParty party, Settlement at)
        {
            if (party.StringId == null || !_counted.TryGetValue(party.StringId, out Counted one) ||
                !double.IsNaN(one.ArrivedAt))
                return;
            if (one.NextTrip && at == party.HomeSettlement)
            {
                one.LeftHomeAt = double.NaN;
                one.StoreAtLeaving = -1;
                return;
            }
            if (one.NextTrip && at.StringId == one.Town && double.IsNaN(one.LeftHomeAt))
            {
                if (Writing)
                    Log.Write("villager check: the next trip from " + one.Name + " at " + one.TownName + ": they came in " +
                              (BringsGoods(party) ? "with goods" : "empty") + " " +
                              Hours((float)(CampaignTime.Now.ToHours - one.SetOffAt)) + " h after you set off, before " +
                              "they were seen leaving their village, so that visit is taken as an earlier trip carried " +
                              "over and their next trip is still awaited");
                return;
            }
            if (at.StringId == one.Town)
            {
                one.ArrivedAt = CampaignTime.Now.ToHours;
                if (one.Villagers && party.PartyTradeGold < one.GoldBefore) one.GoldBefore = party.PartyTradeGold;
                if (one.Villagers) one.CameWithGoods = BringsGoods(party);
                if (!one.Villagers && one.Kind != null && at.Town != null)
                    one.IndexMet = at.Town.GetItemCategoryPriceIndex(one.Kind);
                if (at.Town != null) one.PurseOnArrival = at.Town.Gold;
            }
            else if (at.IsTown) one.Elsewhere = Tongue.Named(at.Name, at.StringId);
        }

        private static void VillagersLeft(MobileParty party, Settlement at)
        {
            if (!party.IsVillager || party.StringId == null || !_counted.TryGetValue(party.StringId, out Counted one) ||
                !one.Villagers || one.Town != at.StringId || double.IsNaN(one.ArrivedAt))
                return;
            one.Sold = TradeMath.WhatVillagersSoldFor(one.GoldBefore, party.PartyTradeGold);
            if (!one.NextTrip || one.CameWithGoods) return;
            one.ArrivedAt = double.NaN;
            one.LeftHomeAt = double.NaN;
            one.Sold = 0;
            one.GoldBefore = party.PartyTradeGold;
        }

        private static void VillagersLeftHome(MobileParty party, Settlement at)
        {
            if (!party.IsVillager || party.StringId == null || at != party.HomeSettlement ||
                !_counted.TryGetValue(party.StringId, out Counted one) || !one.NextTrip || !double.IsNaN(one.ArrivedAt))
                return;
            one.LeftHomeAt = CampaignTime.Now.ToHours;
            int store = StoreHeld(at);
            one.StoreAtLeaving = store < 0 ? -1 : store + UnitsBroughtToSell(party);
            one.BroughtHome = _broughtHome.TryGetValue(party.StringId, out int brought) ? brought : -1;
        }

        private static int UnitsBroughtToSell(MobileParty party)
        {
            ItemRoster carried = party?.ItemRoster;
            if (carried == null) return 0;
            ItemObject kept = null;
            for (int k = 0; k < carried.Count; k++)
            {
                ItemObject item = carried.GetItemAtIndex(k);
                if (item?.ItemCategory == DefaultItemCategories.PackAnimal && item.Value < (kept?.Value ?? 10000)) kept = item;
            }
            int keeps = (int)(0.5f * party.MemberRoster.TotalManCount);
            int units = 0;
            for (int k = 0; k < carried.Count; k++)
                units += TradeMath.UnitsVillagersBringToSell(carried.GetElementNumber(k), carried.GetItemAtIndex(k) == kept, keeps);
            return units;
        }

        private static int StoreHeld(Settlement village)
        {
            ItemRoster store = village?.ItemRoster;
            int held = 0;
            for (int k = 0; store != null && k < store.Count; k++) held += store.GetElementNumber(k);
            return store == null ? -1 : held;
        }

        private static string WhatMadeItReady(Counted one) =>
            double.IsNaN(one.ReadyAt) || double.IsNaN(one.BackAt)
                ? ""
                : "; TradeLord had it ready " +
                  (one.Brought ? "with the goods its party brought home unsold"
                   : one.ReadyAt <= one.SetOffAt + 0.01d ? "when it was counted"
                   : one.ReadyAt > one.BackAt + 0.01d ? "once its store filled" : "once its party got home") +
                  (one.StoreHolds >= 0
                      ? ", its store holding " + one.StoreAtCount + " of the " + one.StoreHolds + " TradeLord counts it " +
                        "can hold when counted" +
                        (one.StoreAtLeaving >= 0 ? " and " + one.StoreAtLeaving + " when they left, their load counted in" : "") +
                        (one.BroughtHome > 0
                            ? ", " + one.BroughtHome + " of that load brought back home unsold from their last trip"
                            : "")
                      : "");

        internal static void YouSoldAt(Settlement site, int gold)
        {
            if (!Writing || site?.StringId == null || gold <= 0) return;
            _youSoldAt.TryGetValue(site.StringId, out int had);
            _youSoldAt[site.StringId] = TradeMath.AddedUp(had, gold);
        }

        internal static void YouSold(List<(ItemRosterElement, int)> sold)
        {
            Settlement site = MobileParty.MainParty?.CurrentSettlement;
            if (sold == null || site == null) return;
            foreach (var (_, gold) in sold) YouSoldAt(site, gold);
        }

        private static void YouReached(Settlement at)
        {
            if (at?.StringId == null) return;
            _youSoldAt[at.StringId] = 0;
            double now = CampaignTime.Now.ToHours;
            foreach (Counted one in _counted.Values)
                if (one.Town == at.StringId && double.IsNaN(one.YouReachedAt)) one.YouReachedAt = now;
        }

        private static bool BringsGoods(MobileParty party)
        {
            ItemRoster carried = party?.ItemRoster;
            if (carried == null) return false;
            ItemObject kept = null;
            for (int k = 0; k < carried.Count; k++)
            {
                ItemObject item = carried.GetItemAtIndex(k);
                if (item?.ItemCategory == DefaultItemCategories.PackAnimal && item.Value < (kept?.Value ?? 10000)) kept = item;
            }
            int keeps = (int)(0.5f * party.MemberRoster.TotalManCount);
            for (int k = 0; k < carried.Count; k++)
                if (TradeMath.UnitsVillagersBringToSell(carried.GetElementNumber(k), carried.GetItemAtIndex(k) == kept, keeps) > 0)
                    return true;
            return false;
        }

        internal static void ACaravanTraded(MobileParty caravan, Town town, List<(EquipmentElement, int)> traded)
        {
            if (!Writing || caravan?.StringId == null || town?.Settlement == null || traded == null) return;
            if (!_counted.TryGetValue(caravan.StringId, out Counted one) || one.Town != town.Settlement.StringId) return;
            if (double.IsNaN(one.ArrivedAt)) one.ArrivedAt = CampaignTime.Now.ToHours;
            int[] fetched = Forecast.WhatTheTradesFetched(town, caravan, traded);
            for (int i = 0; i < traded.Count; i++)
            {
                (EquipmentElement element, int amount) = traded[i];
                if (element.Item == null) continue;
                var (units, sold) = TradeMath.WhatACaravanMoved(amount);
                if (!sold || units <= 0) continue;
                one.Sold = TradeMath.AddedUp(one.Sold, fetched != null
                    ? fetched[i] : TradeMath.WorthOf(units, Priced.At(town, element, caravan, selling: true)));
            }
        }

        private static double _hourDue = double.NaN;
        private static int _hourWaited;

        internal static void Hourly()
        {
            if (!Writing || Campaign.Current == null) return;
            if (!double.IsNaN(_hourDue)) TheHour();
            _hourDue = CampaignTime.Now.ToHours;
            _hourWaited = 0;
        }

        internal static void TheHourOnceWarm()
        {
            if (double.IsNaN(_hourDue)) return;
            if (!Writing || Campaign.Current == null)
            {
                _hourDue = double.NaN;
                return;
            }
            if (_hourWaited < Forecast.FramesAWarmUpWaits && Forecast.WarmedAStage())
            {
                _hourWaited++;
                return;
            }
            TheHour();
        }

        private static void TheHour()
        {
            double now = _hourDue;
            _hourDue = double.NaN;
            JudgeTheCounted(now);
            CheckThePurses(now);
        }

        private static void JudgeTheCounted(double now)
        {
            if (_counted.Count == 0) return;
            var alive = new Dictionary<string, MobileParty>(StringComparer.Ordinal);
            foreach (MobileParty party in MobileParty.AllCaravanParties)
                if (party?.StringId != null) alive[party.StringId] = party;
            foreach (MobileParty party in MobileParty.AllVillagerParties)
                if (party?.StringId != null) alive[party.StringId] = party;
            var done = new List<string>();
            var lines = new List<string>();
            foreach (Counted one in _counted.Values)
            {
                alive.TryGetValue(one.Id, out MobileParty party);
                bool gone = party == null || !party.IsActive;
                if (one.NextTrip && !double.IsNaN(one.ArrivedAt) && !gone && party.CurrentSettlement?.StringId == one.Town &&
                    !one.CameWithGoods)
                {
                    one.ArrivedAt = double.NaN;
                    one.LeftHomeAt = double.NaN;
                    one.GoldBefore = party.PartyTradeGold;
                }
                if (!one.Aside && double.IsNaN(one.YouReachedAt) && _riding && _ride.To?.StringId == one.Town &&
                    !(now > one.DueAt + TradeMath.HoursACaravanIsWaitedFor))
                    continue;
                bool left = !double.IsNaN(one.ArrivedAt) && (gone || party.CurrentSettlement?.StringId != one.Town);
                TradeMath.CaravanCame came = TradeMath.WhatACaravanDid(now, one.DueAt, YouGotThereAt(one), one.ArrivedAt,
                                                                       left, one.Elsewhere != null, gone);
                if (came == TradeMath.CaravanCame.Waiting) continue;
                done.Add(one.Id);
                if (one.Villagers && !gone && party.CurrentSettlement?.StringId == one.Town)
                    one.Sold = TradeMath.WhatVillagersSoldFor(one.GoldBefore, party.PartyTradeGold);
                lines.Add(Judged(one, came, gone, party));
                if (one.Aside)
                {
                    _asideDay++;
                    continue;
                }
                if (one.Villagers)
                {
                    TallyTheVillagers(one, came);
                    continue;
                }
                TallyByYourSale(one, came);
                if (!float.IsNaN(one.IndexMet) && !float.IsNaN(one.IndexSaid) && !float.IsNaN(one.IndexNow))
                {
                    _indexJudged++;
                    _indexSaidOff += Math.Abs(one.IndexSaid - one.IndexMet);
                    _indexNowOff += Math.Abs(one.IndexNow - one.IndexMet);
                }
                if (came == TradeMath.CaravanCame.Sold)
                {
                    _soldDay++;
                    _wantSold += one.Want;
                    _goldSold += one.Sold;
                }
                else if (came == TradeMath.CaravanCame.CameAfterYou)
                {
                    _lateDay++;
                    _wantLate += one.Want;
                    _goldLate += one.Sold;
                }
                else
                {
                    _neverDay++;
                    _wantNever += one.Want;
                }
            }
            for (int i = 0; i < done.Count; i++) _counted.Remove(done[i]);
            if (lines.Count > 0) Log.WriteMany(lines);
        }

        private static double YouGotThereAt(Counted one) =>
            double.IsNaN(one.YouReachedAt) ? one.YouDueAt : one.YouReachedAt;

        private static void TallyByYourSale(Counted one, TradeMath.CaravanCame came)
        {
            if (came == TradeMath.CaravanCame.NeverCame) return;
            TradeMath.YourSaleGroup group = TradeMath.GroupedByYourSale(one.YourSale > 0, YouGotThereAt(one), one.ArrivedAt);
            if (group == TradeMath.YourSaleGroup.NotAfterYou) return;
            if (group == TradeMath.YourSaleGroup.Later && one.PurseOnArrival >= 0 && one.PurseOnArrival < one.Want)
            {
                _purseShortDay++;
                return;
            }
            int g = (int)group;
            _groupCount[g]++;
            _groupWant[g] += one.Want;
            _groupPaid[g] += one.Paid;
            _groupSold[g] += one.Sold;
        }

        private static void TallyTheVillagers(Counted one, TradeMath.CaravanCame came)
        {
            if (one.NextTrip && came != TradeMath.CaravanCame.NeverCame && one.Dice != null)
            {
                float off = (float)(one.ArrivedAt - one.DueAt);
                _tripOff.Add(off);
                _tripDice.Add(one.Dice);
                if (!double.IsNaN(one.LeftHomeAt) && !double.IsNaN(one.SetOutSaidAt))
                {
                    _setOffOff.Add((float)(one.LeftHomeAt - one.SetOutSaidAt));
                    _rideOff.Add((float)((one.ArrivedAt - one.LeftHomeAt) - (one.DueAt - one.SetOutSaidAt)));
                }
                if (TradeMath.CameSoonerThanTheDiceAllow(off, one.DiceSaid))
                    _beatTheDice.Add("villager check: the next trip from " + one.Name + " at " + one.TownName + " came " +
                                     Hours(-off) + " h sooner than said, past the " + Hours(one.DiceSaid) +
                                     " h its dice can bring it forward; TradeLord had its village ready to send " +
                                     Hours((float)(one.ReadyAt - one.SetOffAt)) + " h and its party home " +
                                     Hours((float)(one.BackAt - one.SetOffAt)) + " h after you set off, and set it off at " +
                                     Hours((float)(one.SetOutSaidAt - one.SetOffAt)) + " h" +
                                     (double.IsNaN(one.LeftHomeAt)
                                         ? ""
                                         : "; it left its village at " + Hours((float)(one.LeftHomeAt - one.SetOffAt)) +
                                           " h") + WhatMadeItReady(one));
            }
            if (came == TradeMath.CaravanCame.Sold)
            {
                _villagersSold++;
                _villagersWantSold += one.Want;
                _villagersGoldSold += one.Sold;
            }
            else if (came == TradeMath.CaravanCame.CameAfterYou)
            {
                _villagersLate++;
                _villagersWantLate += one.Want;
                _villagersGoldLate += one.Sold;
            }
            else
            {
                _villagersNever++;
                _villagersWantNever += one.Want;
            }
        }

        private static string Judged(Counted one, TradeMath.CaravanCame came, bool gone, MobileParty party)
        {
            if (one.Villagers) return JudgedVillagers(one, came, gone, party);
            string said = "caravan check: " + one.Name + " at " + one.TownName + ": " +
                          (one.Aside ? "set aside, as you did not make the ride there; " : "") +
                          "Count what is on its way counted it " +
                          "drawing " + one.Want + " from the purse " + Hours(one.SaidHours) + " h after you set off, " +
                          Percent(one.Share) + " of that before you got there, and the purse could pay it " + one.Paid +
                          ", with " + one.YourSale + " of your sale counted for it" + YouSoldThere(one) + "; ";
            if (came == TradeMath.CaravanCame.NeverCame)
                return said + (one.Elsewhere != null ? "it never arrived, going to " + one.Elsewhere + " instead"
                               : gone ? "it never arrived, and is gone from the map"
                               : "it never arrived within a day of when it was due");
            return said + "it arrived " + Hours((float)(one.ArrivedAt - one.SetOffAt)) + " h after you set off" +
                   (came == TradeMath.CaravanCame.CameAfterYou
                       ? ", after you got there at " + Hours((float)(YouGotThereAt(one) - one.SetOffAt)) + " h,"
                       : "") +
                   (one.PurseOnArrival >= 0 ? " to a purse holding " + one.PurseOnArrival + "," : "") +
                   " and sold " + one.Sold + IndexMet(one);
        }

        private static string YouSoldThere(Counted one) =>
            double.IsNaN(one.YouReachedAt) || !_youSoldAt.TryGetValue(one.Town, out int sold)
                ? ""
                : ", and you sold " + sold + " there";

        private static string IndexMet(Counted one) =>
            float.IsNaN(one.IndexMet) || float.IsNaN(one.IndexSaid) || one.Kind == null ? ""
            : "; its biggest sale, " + Tongue.Named(one.Kind.GetName(), one.Kind.StringId) + ", was counted at a price " +
              "index of " + Index(one.IndexSaid) + " for its arrival (" + Index(one.IndexNow) + " when it was counted) " +
              "and met " + Index(one.IndexMet);

        private static string Index(float index) =>
            float.IsNaN(index) ? "unknown" : index.ToString("0.000", CultureInfo.InvariantCulture);

        private static string JudgedVillagers(Counted one, TradeMath.CaravanCame came, bool gone, MobileParty party)
        {
            string said = "villager check: " + (one.NextTrip ? "the next trip from " : "villagers of ") + one.Name +
                          " at " + one.TownName + ": " +
                          (one.Aside ? "set aside, as you did not make the ride there; " : "") +
                          "Count what is on its way counted them drawing " + one.Want +
                          " from the purse " + Hours(one.SaidHours) + " h after you set off, " + Percent(one.Share) +
                          " of that before you got there, and the purse could pay them " + one.Paid + "; ";
            if (came == TradeMath.CaravanCame.NeverCame)
                return said + (gone ? "they never arrived, and are gone from the map"
                               : "they never arrived within a day of when they were due" + WhereTheyAre(party)) +
                       (one.NextTrip && !double.IsNaN(one.LeftHomeAt)
                           ? "; they left their village " + Hours((float)(one.LeftHomeAt - one.SetOffAt)) +
                             " h after you set off"
                           : "");
            return said + "they arrived " + Hours((float)(one.ArrivedAt - one.SetOffAt)) + " h after you set off" +
                   (came == TradeMath.CaravanCame.CameAfterYou
                       ? ", after you got there at " + Hours((float)(YouGotThereAt(one) - one.SetOffAt)) + " h,"
                       : "") +
                   " and sold " + one.Sold + SetOffAndRide(one);
        }

        private static string SetOffAndRide(Counted one)
        {
            if (!one.NextTrip || double.IsNaN(one.SetOutSaidAt)) return "";
            string dice = WhatMadeItReady(one) + "; TradeLord said they would leave their village " +
                          Hours((float)(one.SetOutSaidAt - one.SetOffAt)) + " h after you set off, " +
                          Hours(one.DiceSaid) + " h of that waiting on the game's dice";
            if (double.IsNaN(one.LeftHomeAt)) return dice + ", and their leaving was not seen";
            return dice + ", and they left at " + Hours((float)(one.LeftHomeAt - one.SetOffAt)) + " h, so the ride took " +
                   Hours((float)(one.ArrivedAt - one.LeftHomeAt)) + " h where TradeLord said " +
                   Hours((float)(one.DueAt - one.SetOutSaidAt)) + " h";
        }

        private static string WhereTheyAre(MobileParty party)
        {
            if (party == null) return "";
            Settlement home = party.HomeSettlement;
            Village village = home?.Village;
            int held = 0;
            ItemRoster store = village?.Owner?.ItemRoster;
            for (int k = 0; store != null && k < store.Count; k++) held += store.GetElementNumber(k);
            string where = party.CurrentSettlement != null
                ? ", and are at " + Tongue.Named(party.CurrentSettlement.Name, party.CurrentSettlement.StringId)
                : party.TargetSettlement != null
                    ? ", and are on the road to " + Tongue.Named(party.TargetSettlement.Name, party.TargetSettlement.StringId)
                    : ", and are on the map with nowhere set";
            int holds = Forecast.TheStoreTradeLordCounts(home);
            return where + (village == null
                ? ""
                : ", with " + held + " in their village store" +
                  (holds >= 0 ? " of the " + holds + " TradeLord counts it can hold" : ""));
        }

        internal static void SayTheCounted()
        {
            SayTheVillagers();
            int takenIn = Forecast.TimesYourSaleWasTakenIn();
            if (!Writing || _countedDay + _soldDay + _lateDay + _neverDay + _asideDay == 0)
            {
                ClearTheCounted();
                return;
            }
            Log.Write("caravan check today: " + _countedDay + " caravan(s) counted drawing the purse of a market you " +
                      "set off for; " + _soldDay + " arrived before you and sold " + _goldSold + " of the " + _wantSold +
                      " counted for them, " + _lateDay + " arrived after you and sold " + _goldLate + " of the " +
                      _wantLate + " counted for them, and " + _neverDay + " never arrived, counted for " + _wantNever +
                      "; " + CaravansAwaited() + " still awaited" +
                      (_indexJudged > 0
                          ? "; for " + _indexJudged + " of them the price index of their biggest sale counted for their " +
                            "arrival was " + Index((float)(_indexSaidOff / _indexJudged)) + " off what they met on " +
                            "average, and the index when they were counted " + Index((float)(_indexNowOff / _indexJudged)) +
                            " off"
                          : "") +
                      "; of those that arrived after you got there, " + Group(TradeMath.YourSaleGroup.NotCounted) +
                      " with none of your sale counted, " + Group(TradeMath.YourSaleGroup.WithinItsLanding) +
                      " within " + Hours((float)TradeMath.HoursYourSaleTakesToLandInFull) + " h of you with your sale counted, and " +
                      Group(TradeMath.YourSaleGroup.Later) + " later with your sale counted, leaving out " + _purseShortDay +
                      " whose purse on arrival was short of what they would draw; " + _asideDay +
                      " set aside as rides you did not make; a change in your sale was taken in " + takenIn + " time(s)");
            ClearTheCounted();
        }

        private static string Group(TradeMath.YourSaleGroup group)
        {
            int g = (int)group;
            return _groupCount[g] + " sold " + _groupSold[g] + " of the " + _groupWant[g] + " counted for them, of which " +
                   "the purse could pay " + _groupPaid[g];
        }

        private static int CaravansAwaited()
        {
            int awaited = 0;
            foreach (Counted one in _counted.Values) if (!one.Villagers) awaited++;
            return awaited;
        }

        private static void SayTheVillagers()
        {
            if (!Writing || _villagersDay + _nextTripsDay + _villagersSold + _villagersLate + _villagersNever == 0) return;
            int awaited = 0;
            foreach (Counted one in _counted.Values) if (one.Villagers) awaited++;
            Log.Write("villager check today: " + (_villagersDay + _nextTripsDay) + " villager part(ies) counted drawing the " +
                      "purse of a market you set off for, " + _villagersDay + " on the road and " + _nextTripsDay +
                      " on their village's next trip; " + _villagersSold + " arrived before you and sold " +
                      _villagersGoldSold + " of the " + _villagersWantSold + " counted for them, " + _villagersLate +
                      " arrived after you and sold " + _villagersGoldLate + " of the " + _villagersWantLate +
                      " counted for them, and " + _villagersNever + " never arrived, counted for " + _villagersWantNever +
                      "; " + awaited + " still awaited" + TheTrips());
            if (_beatTheDice.Count > 0) Log.WriteMany(new List<string>(_beatTheDice));
            _villagersDay = _nextTripsDay = _villagersSold = _villagersLate = _villagersNever = 0;
            _villagersWantSold = _villagersGoldSold = _villagersWantLate = _villagersGoldLate = _villagersWantNever = 0L;
            ClearTheTrips();
        }

        private static string TheTrips()
        {
            if (_tripOff.Count == 0) return "";
            var offs = new List<float>(_tripOff);
            offs.Sort();
            int early = 0;
            foreach (float off in offs) if (off < 0f) early++;
            TradeMath.DiceAlone dice = TradeMath.WhatTheDiceAloneGive(_tripDice, (float)TradeMath.HoursACaravanIsWaitedFor);
            string said = "; the " + offs.Count + " next trip(s) that arrived came a median " + Signed(TradeMath.Quantile(offs, 0.5f)) +
                          " h off when TradeLord said, " + Percent((float)early / offs.Count) + " of them sooner, from " +
                          Signed(TradeMath.Quantile(offs, 0.1f)) + " to " + Signed(TradeMath.Quantile(offs, 0.9f)) +
                          " h for the middle eight in ten, where the game's dice alone give a median " + Signed(dice.Median) +
                          " h, " + Percent(dice.Early) + " sooner, and " + Signed(dice.Low) + " to " + Signed(dice.High) +
                          " h; " + _beatTheDice.Count + " came sooner than their dice allow";
            if (_setOffOff.Count == 0) return said;
            var setOff = new List<float>(_setOffOff);
            var ride = new List<float>(_rideOff);
            setOff.Sort();
            ride.Sort();
            return said + "; for the " + setOff.Count + " seen leaving their village, they left a median " +
                   Signed(TradeMath.Quantile(setOff, 0.5f)) + " h off when TradeLord said, and their ride ran a median " +
                   Signed(TradeMath.Quantile(ride, 0.5f)) + " h off what it said";
        }

        private static void ClearTheTrips()
        {
            _tripOff.Clear();
            _tripDice.Clear();
            _setOffOff.Clear();
            _rideOff.Clear();
            _beatTheDice.Clear();
        }

        private static string Signed(float hours) =>
            float.IsNaN(hours) ? "unknown" : (hours > 0f ? "+" : "") + Hours(hours);

        private static void ClearTheCounted()
        {
            _villagersDay = _nextTripsDay = _villagersSold = _villagersLate = _villagersNever = 0;
            _villagersWantSold = _villagersGoldSold = _villagersWantLate = _villagersGoldLate = _villagersWantNever = 0L;
            Array.Clear(_groupCount, 0, _groupCount.Length);
            Array.Clear(_groupWant, 0, _groupWant.Length);
            Array.Clear(_groupPaid, 0, _groupPaid.Length);
            Array.Clear(_groupSold, 0, _groupSold.Length);
            _asideDay = _purseShortDay = 0;
            ClearTheTrips();
            _countedDay = _soldDay = _lateDay = _neverDay = 0;
            _wantSold = _goldSold = _wantLate = _goldLate = _wantNever = 0L;
            _indexJudged = 0;
            _indexSaidOff = _indexNowOff = 0d;
        }

        private static void CheckThePurses(double now)
        {
            if (!Options.Current.Omniscient || !Forecast.On)
            {
                _purseDue.Clear();
                return;
            }
            int hour = (int)Math.Floor(now + 0.5d);
            var stale = new List<int>();
            foreach (var due in _purseDue)
            {
                if (due.Key > hour) continue;
                stale.Add(due.Key);
                if (due.Key < hour) continue;
                foreach (PurseSaid one in due.Value)
                {
                    Settlement site = Settlement.Find(one.Town);
                    if (site?.SettlementComponent != null) WeighAPurse(one, site);
                }
            }
            for (int i = 0; i < stale.Count; i++) _purseDue.Remove(stale[i]);
            var towns = Town.AllTowns;
            for (int i = 0; towns != null && i < towns.Count; i++)
            {
                Settlement site = towns[i]?.Settlement;
                if (site?.StringId == null || site.SettlementComponent == null) continue;
                foreach (int ahead in PurseHoursAhead)
                {
                    if (!_purseDue.TryGetValue(hour + ahead, out List<PurseSaid> due))
                    {
                        due = new List<PurseSaid>();
                        _purseDue[hour + ahead] = due;
                    }
                    due.Add(new PurseSaid { Town = site.StringId, Ahead = ahead, Said = Forecast.TillOnArrival(site, ahead / 24f) });
                }
            }
        }

        private static void WeighAPurse(PurseSaid one, Settlement site)
        {
            int held = site.SettlementComponent.Gold;
            float off = TradeMath.PurseMissedBy(one.Said, held);
            if (!_purseOff.TryGetValue(one.Ahead, out List<float> offs))
            {
                offs = new List<float>();
                _purseOff[one.Ahead] = offs;
            }
            offs.Add(off);
            _purseWorst.TryGetValue(one.Ahead, out var worst);
            if (off < 0f) worst.over++;
            if (worst.at == null || Math.Abs(off) > Math.Abs(worst.worst))
            {
                worst.worst = off;
                worst.at = Tongue.Named(site.Name, site.StringId) + " (said " + one.Said + ", held " + held + ")";
            }
            _purseWorst[one.Ahead] = worst;
        }

        internal static void SayThePurses()
        {
            if (!Writing || _purseOff.Count == 0) return;
            var lines = new List<string>();
            foreach (int ahead in PurseHoursAhead)
            {
                if (!_purseOff.TryGetValue(ahead, out List<float> offs) || offs.Count == 0) continue;
                offs.Sort();
                _purseWorst.TryGetValue(ahead, out var worst);
                int close = 0;
                for (int i = 0; i < offs.Count; i++)
                    if (Math.Abs(offs[i]) <= PurseCloseEnough) close++;
                lines.Add("purse check today, " + ahead + " h ahead: " + offs.Count + " forecast(s) of a town's purse; " +
                          "it held a median " + Missed(TradeMath.Quantile(offs, 0.5f)) + " than said, the middle half " +
                          "from " + Missed(TradeMath.Quantile(offs, 0.25f)) + " to " +
                          Missed(TradeMath.Quantile(offs, 0.75f)) + ", " + close + " within " +
                          Percent(PurseCloseEnough) + ", " + worst.over + " holding less than said; the furthest off " +
                          worst.at);
            }
            if (lines.Count > 0) Log.WriteMany(lines);
            ClearThePurses();
        }

        private static string Missed(float off) =>
            float.IsNaN(off) ? "an amount that could not be weighed"
                             : off >= 0f ? Percent(off) + " more" : Percent(-off) + " less";

        private static void ClearThePurses()
        {
            _purseOff.Clear();
            _purseWorst.Clear();
        }

        private static void Entered(MobileParty party, Settlement at)
        {
            if (!Writing || party.StringId == null || !at.IsTown || at.Town == null) return;
            ItemRoster roster = party.ItemRoster;
            if (roster == null) return;
            var visit = new Visit { Town = at.StringId, At = CampaignTime.Now.ToHours };
            for (int k = 0; k < roster.Count; k++)
            {
                ItemObject item = roster.GetItemAtIndex(k);
                int amount = roster.GetElementNumber(k);
                if (item == null || amount == 0) continue;
                visit.Held.TryGetValue(item, out int had);
                visit.Held[item] = had + amount;
            }
            _visits[party.StringId] = visit;
        }

        internal static void Left(MobileParty party, Settlement at)
        {
            VillagersLeft(party, at);
            VillagersLeftHome(party, at);
            if (party.StringId == null || !_visits.TryGetValue(party.StringId, out Visit visit)) return;
            _visits.Remove(party.StringId);
            if (!Writing || visit.Town != at.StringId || at.Town == null) return;
            var now = new Dictionary<ItemObject, int>();
            ItemRoster roster = party.ItemRoster;
            for (int k = 0; roster != null && k < roster.Count; k++)
            {
                ItemObject item = roster.GetItemAtIndex(k);
                int amount = roster.GetElementNumber(k);
                if (item == null || amount == 0) continue;
                now.TryGetValue(item, out int had);
                now[item] = had + amount;
            }
            foreach (ItemObject item in visit.Held.Keys) if (!now.ContainsKey(item)) now[item] = 0;
            long purse = 0L;
            foreach (var one in now)
            {
                visit.Held.TryGetValue(one.Key, out int had);
                int moved = one.Value - had;
                if (moved == 0) continue;
                ItemObject item = one.Key;
                int price = Priced.At(at.Town, item, party, selling: moved < 0);
                long gold = (long)Math.Abs(moved) * Math.Max(0, price);
                if (item.IsFood)
                {
                    if (moved < 0) continue;
                    _foodGold += gold;
                    _foodUnits += moved;
                    purse += gold;
                }
                else if (item.HasHorseComponent)
                {
                    if (moved > 0)
                    {
                        _horsesInGold += gold;
                        _horsesInUnits += moved;
                        purse += gold;
                    }
                    else
                    {
                        _horsesOutGold += gold;
                        _horsesOutUnits -= moved;
                        purse -= gold;
                    }
                }
                else if (moved < 0)
                {
                    _lootGold += gold;
                    _lootUnits -= moved;
                    purse -= gold;
                }
            }
            _visitsDay++;
            string name = Tongue.Named(at.Name, at.StringId);
            _censusBy.TryGetValue(name, out long sum);
            _censusBy[name] = sum + purse;
        }

        internal static void SayTheCensus()
        {
            if (!Writing) return;
            double now = CampaignTime.Now.ToHours;
            var stale = new List<string>();
            foreach (var one in _visits)
                if (now - one.Value.At > VisitKeptHours) stale.Add(one.Key);
            for (int i = 0; i < stale.Count; i++) _visits.Remove(stale[i]);
            if (_visitsDay == 0) return;
            var most = new List<KeyValuePair<string, long>>(_censusBy);
            most.Sort((x, y) => Math.Abs(y.Value).CompareTo(Math.Abs(x.Value)));
            var named = new List<string>();
            for (int i = 0; i < most.Count && i < 3; i++)
                named.Add(most[i].Key + " " + (most[i].Value >= 0L ? "+" : "") + most[i].Value);
            Log.Write("census today: " + _visitsDay + " visit(s) to towns by parties other than yours, caravans and " +
                      "villagers, none of which Count what is on its way counts; they bought food worth " + _foodGold +
                      " (" + _foodUnits + " item(s)) and horses worth " + _horsesInGold + " (" + _horsesInUnits +
                      ") off the towns' shelves, paid into their purses, and sold horses worth " + _horsesOutGold +
                      " (" + _horsesOutUnits + ") and other goods worth " + _lootGold + " (" + _lootUnits +
                      ") onto them, drawn from their purses" +
                      (named.Count > 0 ? "; most to a purse at " + string.Join(", ", named) : ""));
            ClearTheCensus();
        }

        private static void ClearTheCensus()
        {
            _visitsDay = 0;
            _foodGold = _foodUnits = _horsesInGold = _horsesInUnits = _horsesOutGold = _horsesOutUnits = 0L;
            _lootGold = _lootUnits = 0L;
            _censusBy.Clear();
        }

        private static void Reached(Settlement at)
        {
            YouReached(at);
            _triedTo = at;
            if (!_riding) return;
            _riding = false;
            if (at != _ride.To || !Writing) return;
            Travel.Said said = _ride.Said;
            double now = CampaignTime.Now.ToHours;
            float took = (float)(now - _ride.From);
            float moving = (float)_ride.Moving;
            float saidHours = said.Days * 24f;
            float off = TradeMath.RideOffBy(saidHours, moving);
            TradeMath.Pace pace = Travel.PaceNow();
            var lines = new List<string>
            {
                "ride check: reached " + Tongue.Named(at.Name, at.StringId) + " " + Hours(took) +
                " h after setting off, " + Hours(moving) + " h of it on the move; TradeLord said " +
                Hours(saidHours) + " h, so the ride on the move ran " + Longer(off) + " than said",
                "  night: TradeLord counted " + Hours(said.Night) + " h of the ride at night, and " +
                Hours((float)_ride.NightMoving) + " h of the ride on the move were at night; its night step " +
                Step(pace == null ? float.NaN : pace.Night),
                "  weather: TradeLord counted " + Hours(said.Snow) + " h in snow and " + Hours(said.Wet) +
                " h on wet ground, and the ride on the move had " + Hours((float)_ride.SnowMoving) +
                " h in snow and " + Hours((float)_ride.WetMoving) + " h on wet ground; its snow step " +
                Step(pace == null ? float.NaN : pace.Snow) + ", its wet ground step " +
                Step(pace == null ? float.NaN : pace.Wet),
                "  ground: TradeLord counted " + Hours(said.Forest) + " h in forest, " + Hours(said.Water) +
                " h on water and " + Hours(said.Desert) + " h in desert, and the ride on the move had " +
                Hours((float)_ride.ForestMoving) + " h in forest, " + Hours((float)_ride.WaterMoving) +
                " h on water and " + Hours((float)_ride.DesertMoving) + " h in desert; its forest step " +
                Step(OnGround(pace, TradeMath.GroundForest)) + ", its water step " +
                Step(OnGround(pace, TradeMath.GroundWater)) + ", its desert step " +
                Step(OnGround(pace, TradeMath.GroundDesert))
            };
            string carries = Travel.WhatCarriesYou(MobileParty.MainParty);
            if (_ride.Carried != null || carries != null)
            {
                if (string.Equals(carries, _ride.Carried, StringComparison.Ordinal))
                    lines.Add("  what carried you, the same at both ends: " + _ride.Carried);
                else
                {
                    lines.Add("  what carried you as TradeLord gave its time: " +
                              (_ride.Carried ?? "it could not be read"));
                    lines.Add("  what carried you on arrival: " + (carries ?? "it could not be read"));
                }
            }
            if (moving > 0f && _ride.Moved > 0f)
            {
                float ran = _ride.Moved * TradeMath.HoursAPartyTakesToCoverItsSpeed / moving;
                lines.Add("  road: TradeLord put the road at " + Figure(said.Length) +
                          (said.Walked ? ", measured along the road the game would take," : ", measured by the game's distance,") +
                          " and the game moved you " + Figure(_ride.Moved) + ", " +
                          Longer(TradeMath.RideOffBy(said.Length, _ride.Moved)) + "; your Speed on the move came to " +
                          Figure(ran) + " where TradeLord counted " + Figure(said.DayPace) + " by day and " +
                          Figure(said.NightPace) + " by night");
                if (_ride.QuickestSeen > 0f)
                    lines.Add("  pace: the game's own Speed ran from " + Figure(_ride.SlowestSeen) + " to " +
                              Figure(_ride.QuickestSeen) + " while you moved, the slowest reading " +
                              Hours((float)_ride.SlowestAt) + " h into the ride");
            }
            bool overCeiling = false;
            if (_ride.Ceiling > 0f)
            {
                float tookDays = moving / 24f;
                overCeiling = said.Days <= _ride.Ceiling && tookDays > _ride.Ceiling;
                lines.Add("  ceiling: " + (at.IsVillage ? "Village" : "Town") + " travel ceiling " +
                          Figure(_ride.Ceiling) + " days; TradeLord said " + Figure(said.Days) +
                          " days and the ride on the move took " + Figure(tookDays) +
                          (overCeiling ? ", past the ceiling TradeLord kept it under" : ""));
            }
            if (_ride.TillSaid && at.SettlementComponent != null)
            {
                int holds = at.SettlementComponent.Gold;
                int missed = holds - _ride.TillOnArrival;
                lines.Add("  purse: it held " + _ride.TillThen + " when you set off; TradeLord said the sellers " +
                          "already on the road would want to draw " + _ride.TillDraw + " of it, of which it could pay " +
                          "them " + _ride.TillPaid + ", and the caravans already on " +
                          "the road would pay " + _ride.TillTake + " into it, after the game's tax, before you got " +
                          "there, and it would hold " + _ride.TillOnArrival + ", and it holds " + holds + ", " +
                          (missed == 0 ? "exactly what it said" : Math.Abs(missed) + (missed > 0 ? " more" : " less") +
                                                                   " than it said"));
                if (_ride.TillWho != null) lines.Add("  purse draw: " + _ride.TillWho);
            }
            if (_ride.Standing > StoodStill || _ride.Fought)
                lines.Add("  left out: " + Hours((float)_ride.Standing) + " h standing still" +
                          (_ride.Fought ? " or fighting, with a fight on the way" : "") +
                          ", which TradeLord's time never counts");
            if (!float.IsNaN(off) && saidHours >= ShortestJudged && !_ride.Fought)
            {
                _judged++;
                _offTotal += Math.Abs(off);
                if (Math.Abs(off) <= CloseEnough) _close++;
                if (overCeiling) _pastCeiling++;
            }
            if (_judged > 0)
                lines.Add("  this session: " + _judged + " ride(s) of " + Hours(ShortestJudged) + " h or more " +
                          "without a fight judged, TradeLord's time came within " + Percent(_offTotal / _judged) +
                          " on average, " + _close + " of them within " + Percent(CloseEnough) + ", and " +
                          _pastCeiling + " it kept under a travel ceiling ran past it");
            Log.WriteMany(lines);
        }

        internal static void Saw(MobileParty party, Settlement bound, float days)
        {
            if (!Writing || party == null || bound == null || !(days > 0f) || TradeMath.OutOfReach(days)) return;
            string id = party.StringId;
            if (id == null) return;
            if (_followed.TryGetValue(id, out Followed held) && held.To == bound.StringId) return;
            _followed[id] = new Followed
            {
                To = bound.StringId,
                At = CampaignTime.Now.ToHours,
                SaidHours = days * 24f,
                StraightHours = Travel.StraightDaysFor(party, bound) * 24f,
                Dodged = Dodging(party)
            };
        }

        private static void ACaravanReached(MobileParty party, Settlement at)
        {
            string id = party?.StringId;
            if (id == null || at == null || !_followed.TryGetValue(id, out Followed held)) return;
            _followed.Remove(id);
            if (held.To != at.StringId || !(held.SaidHours >= CaravanShortestJudged)) return;
            float took = (float)(CampaignTime.Now.ToHours - held.At);
            float off = TradeMath.RideOffBy(held.SaidHours, took);
            if (float.IsNaN(off)) return;
            if (held.Dodged || Dodging(party))
            {
                _dodged.Add(off);
                return;
            }
            _arrived.Add(off);
            float straight = TradeMath.RideOffBy(held.StraightHours, took);
            if (!float.IsNaN(straight)) _arrivedStraight.Add(straight);
        }

        internal static void SayTheCaravans()
        {
            if (!Writing) return;
            double now = CampaignTime.Now.ToHours;
            var stale = new List<string>();
            foreach (var one in _followed)
                if (now - one.Value.At > CaravanKeptHours) stale.Add(one.Key);
            for (int i = 0; i < stale.Count; i++) _followed.Remove(stale[i]);
            if (_arrived.Count == 0 && _dodged.Count == 0) return;
            _arrived.Sort();
            _arrivedStraight.Sort();
            _dodged.Sort();
            var lines = new List<string>();
            if (_arrived.Count == 0)
                lines.Add("caravan ride check: no caravan or villager arrival was judged yet");
            else
                lines.Add("caravan ride check: " + _arrived.Count + " caravan and villager arrival(s) reached the " +
                          "town Count what is on its way had them bound for without fleeing or fighting on the way; " +
                          "their ride ran a median " + Longer(TradeMath.Quantile(_arrived, 0.5f)) +
                          " than said, the middle half from " + Longer(TradeMath.Quantile(_arrived, 0.25f)) +
                          " to " + Longer(TradeMath.Quantile(_arrived, 0.75f)) + ", " + Within(_arrived) +
                          " within " + Percent(CaravanCloseEnough) + " and " + Later(_arrived) + " later than said");
            if (_arrivedStraight.Count > 0)
                lines.Add("  straight line: timed by a straight line to the town, the same rides would have run a " +
                          "median " + Longer(TradeMath.Quantile(_arrivedStraight, 0.5f)) +
                          " than said, the middle half from " + Longer(TradeMath.Quantile(_arrivedStraight, 0.25f)) +
                          " to " + Longer(TradeMath.Quantile(_arrivedStraight, 0.75f)) + ", " +
                          Within(_arrivedStraight) + " within " + Percent(CaravanCloseEnough) + " and " +
                          Later(_arrivedStraight) + " later than said");
            if (_dodged.Count > 0)
                lines.Add("  set aside: " + _dodged.Count + " caravan(s) and villager(s) were seen fleeing, going " +
                          "around a band or fighting on the way, and ran a median " +
                          Longer(TradeMath.Quantile(_dodged, 0.5f)) + " than said, the middle half from " +
                          Longer(TradeMath.Quantile(_dodged, 0.25f)) + " to " +
                          Longer(TradeMath.Quantile(_dodged, 0.75f)));
            Log.WriteMany(lines);
        }

        private static float OnGround(TradeMath.Pace pace, int ground) =>
            pace == null || pace.Even ? float.NaN : pace.OnGround[ground];

        private static int Within(List<float> offs)
        {
            int close = 0;
            for (int i = 0; i < offs.Count; i++)
                if (Math.Abs(offs[i]) <= CaravanCloseEnough) close++;
            return close;
        }

        private static int Later(List<float> offs)
        {
            int late = 0;
            for (int i = 0; i < offs.Count; i++)
                if (offs[i] > 0f) late++;
            return late;
        }

        private static string Figure(float value) =>
            float.IsNaN(value) ? "unknown" : value.ToString("0.00", CultureInfo.InvariantCulture);

        private static string Hours(float hours) => Figure(hours);

        private static string Step(float value) =>
            float.IsNaN(value) ? "unknown"
                               : (value >= 0f ? "+" : "") + value.ToString("0.000", CultureInfo.InvariantCulture);

        private static string Percent(float share) =>
            float.IsNaN(share) ? "unknown" : share.ToString("0.0%", CultureInfo.InvariantCulture);

        private static string Longer(float off) =>
            float.IsNaN(off) ? "by an amount that could not be weighed"
                             : off >= 0f ? Percent(off) + " longer" : Percent(-off) + " shorter";
    }
}
