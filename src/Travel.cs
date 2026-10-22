using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TradeLord
{
    internal static class Travel
    {
        internal static bool NavalActive
        {
            get { try { return HasNaval(); } catch { return false; } }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool HasNaval()
        {
            MobileParty p = MobileParty.MainParty;
            return p != null && p.HasNavalNavigationCapability;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static float SeaSpeedCore()
        {
            MBReadOnlyList<Ship> ships = MobileParty.MainParty.Ships;
            if (ships == null || ships.Count == 0) return 0f;
            float sum = 0f, min = float.MaxValue;
            for (int i = 0; i < ships.Count; i++)
            {
                float sp = ships[i].GetCampaignSpeed();
                sum += sp;
                if (sp < min) min = sp;
            }
            return TradeMath.FleetSpeed(sum, ships.Count, min);
        }

        private static float SeaSpeed()
        {
            try { return NavalActive ? SeaSpeedCore() : 0f; } catch { return 0f; }
        }

        internal static float FromParty(Settlement target, out float landRatio)
        {
            landRatio = 1f;
            if (target == null) return 0f;
            try { return FromPartyCore(target, out landRatio); }
            catch { landRatio = 1f; }
            return MobileParty.MainParty.GetPosition2D.Distance(target.GetPosition2D);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static float FromPartyCore(Settlement target, out float landRatio)
        {
            MobileParty party = MobileParty.MainParty;
            return Campaign.Current.Models.MapDistanceModel.GetDistance(
                party, target, target.HasPort && NavalActive, party.NavigationCapability, out landRatio);
        }

        internal static float Between(Settlement from, Settlement to, out float landRatio)
        {
            landRatio = 1f;
            if (from == null || to == null) return 0f;
            try { return BetweenCore(from, to, out landRatio); }
            catch { landRatio = 1f; }
            return from.GetPosition2D.Distance(to.GetPosition2D);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static float BetweenCore(Settlement from, Settlement to, out float landRatio)
        {
            bool naval = NavalActive;
            return Campaign.Current.Models.MapDistanceModel.GetDistance(
                from, to, from.HasPort && naval, to.HasPort && naval,
                MobileParty.MainParty.NavigationCapability, out landRatio);
        }

        private static int _speedHour = -1;
        private static float _landSpeed, _seaSpeed, _nightPace = TradeMath.NightPaceUnread;
        private static float[] _paces = TradeMath.PacesFromShares(TradeMath.NightPaceUnread, TradeMath.SnowShareUnread, 1f);
        private static float[] _caravanPaces = TradeMath.PacesFromShares(TradeMath.NightPaceUnread, TradeMath.SnowShareUnread, 1f);

        internal static float DayPace { get { Refresh(); return _landSpeed; } }

        internal static float NightPace { get { Refresh(); return _nightPace; } }

        internal static float PaceIn(bool night, bool snowy, bool wet)
        {
            Refresh();
            return _paces[TradeMath.WeatherIndex(night, snowy, wet)];
        }

        private static void Speeds(out float land, out float sea)
        {
            Refresh();
            land = _landSpeed;
            sea = _seaSpeed;
        }

        private static void Refresh()
        {
            int hour = (int)CampaignTime.Now.ToHours;
            if (hour == _speedHour) return;
            _rides.Clear();
            _partyRides.Clear();
            MobileParty party = MobileParty.MainParty;
            float now = party.Speed;
            float toClearDay = AtSea(party) ? 1f : ReadThePaces(party);
            TradeMath.SpeedsInEffect(now * toClearDay, SeaSpeed(), out _landSpeed, out _seaSpeed);
            _nightPace = _paces[TradeMath.WeatherIndex(true, false, false)];
            _caravanPaces = TradeMath.PacesFromShares(Nightfall.CaravanPace, Skies.CaravanSnowShare, Skies.CaravanWetShare);
            _speedHour = hour;
        }

        private static bool AtSea(MobileParty party)
        {
            try { return IsAtSea(party); } catch { return false; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsAtSea(MobileParty party) => party != null && party.IsCurrentlyAtSea;

        private static float ReadThePaces(MobileParty party)
        {
            try { return ReadThePacesCore(party); } catch { return 1f; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static float ReadThePacesCore(MobileParty party)
        {
            PartySpeedModel model = Campaign.Current.Models.PartySpeedCalculatingModel;
            ExplainedNumber speed = model.CalculateFinalSpeed(party, model.CalculateBaseSpeed(party));
            bool night = Campaign.Current.IsNight;
            Skies.LiveSkyAt(party.GetPosition2D, out bool snowy, out bool wet);
            float nightStep = Nightfall.PartyStep, snowStep = Skies.PartySnowStep, wetStep = Skies.PartyWetStep;
            float clear = speed.SumOfFactors - (night ? nightStep : 0f) - (snowy ? snowStep : 0f) - (wet ? wetStep : 0f);
            float least = speed.LimitMinValue, most = speed.LimitMaxValue;
            _paces = TradeMath.PacesFromTheGame(speed.BaseNumber, clear, least, most, nightStep, snowStep, wetStep);
            float paceNow = TradeMath.WithinLimits(speed.BaseNumber * (1f + speed.SumOfFactors), least, most);
            float paceClear = TradeMath.WithinLimits(speed.BaseNumber * (1f + clear), least, most);
            return paceNow > 0f && paceClear > 0f ? paceClear / paceNow : 1f;
        }

        internal static void ReadThePaceAgain() => _speedHour = -1;

        private static readonly Dictionary<(string, string, int), float> _rides = new Dictionary<(string, string, int), float>();
        private static readonly Dictionary<string, float> _partyRides = new Dictionary<string, float>();
        private static readonly Dictionary<string, List<TradeMath.RoadPiece>> _partyPieces =
            new Dictionary<string, List<TradeMath.RoadPiece>>();
        private static readonly Dictionary<(string, string), List<TradeMath.RoadPiece>> _pairPieces =
            new Dictionary<(string, string), List<TradeMath.RoadPiece>>();

        private static float Days(float distance, float landRatio, float afterDays, bool seaFirst,
                                  List<TradeMath.RoadPiece> pieces, out TradeMath.RoadTally tally)
        {
            tally = default(TradeMath.RoadTally);
            if (distance <= 0f) return 0f;
            Refresh();
            float later = TradeMath.OutOfReach(afterDays) ? 0f : Math.Max(0f, TradeMath.Finite(afterDays, 0f));
            double startAt = CampaignTime.Now.ToHours + later * 24d;
            float start = CampaignTime.Now.CurrentHourInDay + later * 24f;
            long started = Stopwatch.GetTimestamp();
            float days = TradeMath.DaysAlongTheRoad(distance, landRatio, _landSpeed, _seaSpeed, pieces, _paces,
                                                    Skies.From(startAt), start, CampaignTime.SunRise, CampaignTime.SunSet,
                                                    seaFirst, out tally);
            Skies.Walked(started);
            return days;
        }

        private static bool SetOffAtSea() => AtSea(MobileParty.MainParty);

        private static List<TradeMath.RoadPiece> PartyPieces(MobileParty party, Settlement target)
        {
            if (_partyPieces.TryGetValue(target.StringId, out List<TradeMath.RoadPiece> known)) return known;
            List<TradeMath.RoadPiece> pieces = Skies.CellsBetween(party.GetPosition2D, target.GetPosition2D,
                                                                  new List<TradeMath.RoadPiece>());
            if (pieces.Count > 0) _partyPieces[target.StringId] = pieces;
            return pieces;
        }

        private static List<TradeMath.RoadPiece> PairPieces(Settlement a, Settlement b)
        {
            var key = (a.StringId, b.StringId);
            if (_pairPieces.TryGetValue(key, out List<TradeMath.RoadPiece> known)) return known;
            List<TradeMath.RoadPiece> pieces = Skies.CellsBetween(a.GetPosition2D, b.GetPosition2D,
                                                                  new List<TradeMath.RoadPiece>());
            if (pieces.Count > 0) _pairPieces[key] = pieces;
            return pieces;
        }

        internal static float CaravanDays(MobileParty party, Settlement bound, out TradeMath.RoadTally tally)
        {
            tally = default(TradeMath.RoadTally);
            Vec2 from = party.GetPosition2D, to = bound.GetPosition2D;
            float distance = from.Distance(to);
            float now = party.Speed;
            if (AtSea(party)) return TradeMath.EtaDays(distance, now);
            Refresh();
            Skies.LiveSkyAt(from, out bool snowy, out bool wet);
            float share = _caravanPaces[TradeMath.WeatherIndex(Campaign.Current.IsNight, snowy, wet)];
            float byDay = share > 0f ? now / share : now;
            long started = Stopwatch.GetTimestamp();
            float days = TradeMath.EtaAlongTheRoad(distance, byDay, Skies.CellsBetween(from, to, null), _caravanPaces,
                                                   Skies.From(CampaignTime.Now.ToHours), CampaignTime.Now.CurrentHourInDay,
                                                   CampaignTime.SunRise, CampaignTime.SunSet, out tally);
            Skies.Walked(started);
            return days;
        }

        internal struct Ride
        {
            internal float Days;
            internal float NightHours;
            internal float SnowHours;
            internal float WetHours;
            internal float Distance;
            internal float LandRatio;
        }

        internal static Ride TheRideFromParty(Settlement target)
        {
            Ride ride = default(Ride);
            MobileParty party = MobileParty.MainParty;
            if (target == null || party == null) return ride;
            DropIfNavalChanged();
            ride.Distance = FromParty(target, out ride.LandRatio);
            ride.Days = Days(ride.Distance, ride.LandRatio, 0f, SetOffAtSea(),
                             Skies.CellsBetween(party.GetPosition2D, target.GetPosition2D, new List<TradeMath.RoadPiece>()),
                             out TradeMath.RoadTally tally);
            ride.NightHours = tally.Night;
            ride.SnowHours = tally.Snow;
            ride.WetHours = tally.Wet;
            return ride;
        }

        private static readonly Dictionary<string, (float dist, float landRatio)> _partyDist
            = new Dictionary<string, (float, float)>();
        private static Vec2 _partyAt;
        private static int _partyHour = -1;
        private static readonly Dictionary<(string, string), (float dist, float landRatio)> _pairDist
            = new Dictionary<(string, string), (float, float)>();
        private static bool _cachedNaval;
        private static int _roadsFound, _roadsLost;

        internal static bool LostTheRoad => _roadsLost > 0 && _roadsFound == 0;

        private static void DropIfNavalChanged()
        {
            bool naval = NavalActive;
            if (naval == _cachedNaval) return;
            _cachedNaval = naval;
            _speedHour = -1;
            _partyDist.Clear();
            _pairDist.Clear();
            _partyPieces.Clear();
            _pairPieces.Clear();
            _roadsFound = 0;
            _roadsLost = 0;
        }

        internal static void Forget()
        {
            _nightPace = TradeMath.NightPaceUnread;
            _paces = TradeMath.PacesFromShares(TradeMath.NightPaceUnread, TradeMath.SnowShareUnread, 1f);
            _caravanPaces = TradeMath.PacesFromShares(TradeMath.NightPaceUnread, TradeMath.SnowShareUnread, 1f);
            _rides.Clear();
            _partyRides.Clear();
            _partyPieces.Clear();
            _pairPieces.Clear();
            _partyDist.Clear();
            _partyHour = -1;
            _pairDist.Clear();
            _cachedNaval = false;
            _speedHour = -1;
            _roadsFound = 0;
            _roadsLost = 0;
        }

        internal static float StraightDaysFromParty(Settlement target)
        {
            MobileParty party = MobileParty.MainParty;
            if (target == null || party == null) return 0f;
            return StraightDays(party.GetPosition2D.Distance(target.GetPosition2D));
        }

        internal static float StraightDaysBetween(Settlement a, Settlement b) =>
            a == null || b == null ? 0f : StraightDays(a.GetPosition2D.Distance(b.GetPosition2D));

        private static float StraightDays(float distance)
        {
            DropIfNavalChanged();
            Speeds(out float land, out float sea);
            return TradeMath.DaysAtBestSpeed(distance, land, sea);
        }

        internal static float EstimateDaysFromParty(Settlement target)
        {
            MobileParty party = MobileParty.MainParty;
            if (target == null || party == null) return 0f;
            DropIfNavalChanged();
            int hour = (int)CampaignTime.Now.ToHours;
            Vec2 at = party.GetPosition2D;
            if (hour != _partyHour || at.DistanceSquared(_partyAt) > 100f)
            {
                _partyDist.Clear();
                _partyRides.Clear();
                _partyPieces.Clear();
                _partyHour = hour;
                _partyAt = at;
                _roadsFound = 0;
                _roadsLost = 0;
            }
            Refresh();
            if (_partyRides.TryGetValue(target.StringId, out float known)) return known;
            if (!_partyDist.TryGetValue(target.StringId, out var hit))
            {
                float dist = FromParty(target, out float landRatio);
                hit = (dist, landRatio);
                if (TradeMath.OutOfReach(TradeMath.DaysAtSpeed(dist, landRatio, _landSpeed, _seaSpeed)))
                {
                    float fromHere = FromWhereYouStand(party, target);
                    if (!TradeMath.OutOfReach(fromHere))
                    {
                        _roadsFound++;
                        return fromHere;
                    }
                    _roadsLost++;
                }
                else
                {
                    _roadsFound++;
                    _partyDist[target.StringId] = hit;
                }
            }
            float days = Days(hit.dist, hit.landRatio, 0f, SetOffAtSea(), PartyPieces(party, target), out _);
            _partyRides[target.StringId] = days;
            return days;
        }

        private static float FromWhereYouStand(MobileParty party, Settlement target)
        {
            Settlement inside = party.CurrentSettlement;
            return inside == null ? TradeMath.FurthestThereIs : EstimateDaysBetween(inside, target);
        }

        internal static float EstimateDaysBetween(Settlement a, Settlement b, float afterDays = 0f)
        {
            if (a == null || b == null) return 0f;
            DropIfNavalChanged();
            var key = string.CompareOrdinal(a.StringId, b.StringId) <= 0
                ? (a.StringId, b.StringId) : (b.StringId, a.StringId);
            if (!_pairDist.TryGetValue(key, out var hit))
            {
                float dist = Between(a, b, out float landRatio);
                hit = (dist, landRatio);
                _pairDist[key] = hit;
            }
            Refresh();
            float later = TradeMath.OutOfReach(afterDays) ? 0f : Math.Max(0f, TradeMath.Finite(afterDays, 0f));
            var memo = (a.StringId, b.StringId, (int)Math.Round(later * 1440f));
            if (_rides.TryGetValue(memo, out float known)) return known;
            float days = Days(hit.dist, hit.landRatio, later, false, PairPieces(a, b), out _);
            _rides[memo] = days;
            return days;
        }

        internal static string EstimateLabel(Settlement target)
        {
            if (target == null || target == Settlement.CurrentSettlement) return "";
            float days = EstimateDaysFromParty(target);
            if (days < 0.05f || TradeMath.OutOfReach(days)) return "";
            TextObject label = Tongue.Text("{=TL79}~{DAYS} day(s)");
            label.SetTextVariable("DAYS", days.ToString("0.#"));
            return label.ToString();
        }
    }
}
