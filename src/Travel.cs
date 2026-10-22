using System;
using System.Collections.Generic;
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

        internal static float DayPace { get { Refresh(); return _landSpeed; } }

        internal static float NightPace { get { Refresh(); return _nightPace; } }

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
            MobileParty party = MobileParty.MainParty;
            float now = party.Speed;
            bool slowedNow = Campaign.Current.IsNight && !AtSea(party);
            if (!AtSea(party)) _nightPace = PaceAtNight(party, _nightPace);
            float byDay = slowedNow ? now / _nightPace : now;
            TradeMath.SpeedsInEffect(byDay, SeaSpeed(), out _landSpeed, out _seaSpeed);
            _speedHour = hour;
        }

        private static bool AtSea(MobileParty party)
        {
            try { return IsAtSea(party); } catch { return false; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsAtSea(MobileParty party) => party != null && party.IsCurrentlyAtSea;

        private static float PaceAtNight(MobileParty party, float known)
        {
            try { return PaceAtNightCore(party); } catch { return known; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static float PaceAtNightCore(MobileParty party)
        {
            PartySpeedModel model = Campaign.Current.Models.PartySpeedCalculatingModel;
            ExplainedNumber speed = model.CalculateFinalSpeed(party, model.CalculateBaseSpeed(party));
            return TradeMath.NightPace(speed.BaseNumber, speed.SumOfFactors, speed.LimitMinValue, speed.LimitMaxValue,
                                       Nightfall.PartyStep, Campaign.Current.IsNight);
        }

        internal static void ReadThePaceAgain() => _speedHour = -1;

        internal static float Days(float distance, float landRatio) =>
            Days(distance, landRatio, 0f, SetOffAtSea(), out _);

        private static float Days(float distance, float landRatio, float afterDays, bool seaFirst, out float nightHours)
        {
            nightHours = 0f;
            if (distance <= 0f) return 0f;
            Refresh();
            float later = TradeMath.OutOfReach(afterDays) ? 0f : Math.Max(0f, TradeMath.Finite(afterDays, 0f));
            float start = CampaignTime.Now.CurrentHourInDay + later * 24f;
            return TradeMath.DaysOnTheClock(distance, landRatio, _landSpeed, _nightPace, _seaSpeed, start,
                                            CampaignTime.SunRise, CampaignTime.SunSet, seaFirst, out nightHours);
        }

        private static bool SetOffAtSea() => AtSea(MobileParty.MainParty);

        internal static float CaravanDays(MobileParty party, Settlement bound, out float nightHours)
        {
            nightHours = 0f;
            float distance = party.GetPosition2D.Distance(bound.GetPosition2D);
            float now = party.Speed;
            if (AtSea(party)) return TradeMath.EtaDays(distance, now);
            float pace = TradeMath.NightPaceKept(Nightfall.CaravanPace);
            float byDay = Campaign.Current.IsNight ? now / pace : now;
            return TradeMath.EtaOnTheClock(distance, byDay, pace, CampaignTime.Now.CurrentHourInDay,
                                           CampaignTime.SunRise, CampaignTime.SunSet, out nightHours);
        }

        internal struct Ride
        {
            internal float Days;
            internal float NightHours;
            internal float Distance;
            internal float LandRatio;
        }

        internal static Ride TheRideFromParty(Settlement target)
        {
            Ride ride = default(Ride);
            if (target == null || MobileParty.MainParty == null) return ride;
            DropIfNavalChanged();
            ride.Distance = FromParty(target, out ride.LandRatio);
            ride.Days = Days(ride.Distance, ride.LandRatio, 0f, SetOffAtSea(), out ride.NightHours);
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
            _roadsFound = 0;
            _roadsLost = 0;
        }

        internal static void Forget()
        {
            _nightPace = TradeMath.NightPaceUnread;
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
                _partyHour = hour;
                _partyAt = at;
                _roadsFound = 0;
                _roadsLost = 0;
            }
            if (!_partyDist.TryGetValue(target.StringId, out var hit))
            {
                float dist = FromParty(target, out float landRatio);
                hit = (dist, landRatio);
                if (TradeMath.OutOfReach(Days(dist, landRatio)))
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
            return Days(hit.dist, hit.landRatio);
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

            return Days(hit.dist, hit.landRatio, afterDays, false, out _);
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
