using System;
using System.Collections.Generic;
using System.Globalization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;

namespace TradeLord
{
    public class RideWatchBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, OnSettlementEntered);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, OnDailyTick);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnTick(float dt)
        {
            Guard.Run("Nightfall.Tick", dt, Nightfall.Tick);
            Guard.Run("RideCheck.Tick", dt, RideCheck.Tick);
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party == null || settlement == null) return;
            Guard.Run("RideCheck.Arrived", () => RideCheck.Arrived(party, settlement));
        }

        private void OnDailyTick() => Guard.Run("RideCheck.Daily", RideCheck.SayTheCaravans);
    }

    internal static class RideCheck
    {
        private const float ShortestJudged = 2f;
        private const float CaravanShortestJudged = 3f;
        private const float CloseEnough = 0.1f;
        private const float CaravanCloseEnough = 0.15f;
        private const double CaravanKeptHours = 15d * 24d;

        private struct Open
        {
            internal Settlement To;
            internal double From;
            internal double LastAt;
            internal float SaidHours;
            internal float SaidNight;
            internal float SaidDistance;
            internal float DayPace;
            internal float NightPace;
            internal float Ceiling;
            internal double Moving;
            internal double Standing;
            internal double NightMoving;
            internal float Moved;
            internal bool Fought;
        }

        private struct Followed
        {
            internal string To;
            internal double At;
            internal float SaidHours;
        }

        private static Open _ride;
        private static bool _riding;
        private static Settlement _triedTo;
        private static int _judged, _close, _pastCeiling;
        private static float _offTotal;
        private static readonly Dictionary<string, Followed> _followed = new Dictionary<string, Followed>();
        private static readonly List<float> _arrived = new List<float>();

        internal static bool Writing => Options.Current.ExtendedDebugLogging;

        internal static void Forget()
        {
            _ride = default(Open);
            _riding = false;
            _triedTo = null;
            _judged = 0;
            _close = 0;
            _pastCeiling = 0;
            _offTotal = 0f;
            _followed.Clear();
            _arrived.Clear();
        }

        internal static void Tick(float dt)
        {
            MobileParty party = MobileParty.MainParty;
            if (!Writing || party == null)
            {
                _riding = false;
                return;
            }
            double now = CampaignTime.Now.ToHours;
            Settlement to = party.DefaultBehavior == AiBehavior.GoToSettlement ? party.TargetSettlement : null;
            if (_riding && to != _ride.To && party.CurrentSettlement == null)
            {
                _riding = false;
                Log.Write("ride check: you turned away from " + _ride.To.Name + " after " + Hours(now - _ride.From) +
                          " h, so that ride is not judged");
            }
            if (to != _triedTo && party.CurrentSettlement == null)
            {
                _triedTo = to;
                if (!_riding && to != null) SetOff(party, to, now);
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
            if (Campaign.Current.IsNight && !party.IsCurrentlyAtSea) _ride.NightMoving += step;
        }

        private static void SetOff(MobileParty party, Settlement to, double now)
        {
            Travel.Ride said = Travel.TheRideFromParty(to);
            if (said.Days <= 0f || TradeMath.OutOfReach(said.Days)) return;
            _ride = new Open
            {
                To = to,
                From = now,
                LastAt = now,
                SaidHours = said.Days * 24f,
                SaidNight = said.NightHours,
                SaidDistance = said.Distance,
                DayPace = Travel.DayPace,
                NightPace = Travel.NightPace,
                Ceiling = MarketRank.Ceiling(to.IsVillage, Options.Current)
            };
            _riding = true;
            Log.Write("ride check: set off for " + to.Name + "; TradeLord says " + Hours(_ride.SaidHours) + " h, " +
                      Hours(_ride.SaidNight) + " h of it at night, over a road of " + Nightfall.Figure(said.Distance) +
                      (said.LandRatio < 0.999f ? " (" + Percent(1f - said.LandRatio) + " of it at sea)" : "") +
                      " at a Speed of " + Nightfall.Figure(_ride.DayPace) + " by day and " +
                      Nightfall.Figure(_ride.DayPace * _ride.NightPace) + " by night, your night step " +
                      Nightfall.PartyStepSaid);
        }

        internal static void Arrived(MobileParty party, Settlement at)
        {
            if (party == MobileParty.MainParty)
            {
                Reached(at);
                return;
            }
            if (!party.IsCaravan && !party.IsVillager) return;
            if (!_followed.TryGetValue(party.StringId, out Followed seen)) return;
            _followed.Remove(party.StringId);
            if (seen.To != at.StringId || !Writing) return;
            float off = TradeMath.RideOffBy(seen.SaidHours, (float)(CampaignTime.Now.ToHours - seen.At));
            if (!float.IsNaN(off)) _arrived.Add(off);
        }

        internal static void Saw(MobileParty party, Settlement bound, float days)
        {
            if (!Writing || party == null || bound == null) return;
            float hours = days * 24f;
            if (hours < CaravanShortestJudged || TradeMath.OutOfReach(days)) return;
            if (_followed.TryGetValue(party.StringId, out Followed seen) && seen.To == bound.StringId) return;
            _followed[party.StringId] = new Followed
            {
                To = bound.StringId,
                At = CampaignTime.Now.ToHours,
                SaidHours = hours
            };
        }

        private static void Reached(Settlement at)
        {
            _triedTo = at;
            if (!_riding) return;
            _riding = false;
            if (at != _ride.To || !Writing) return;
            double now = CampaignTime.Now.ToHours;
            float took = (float)(now - _ride.From);
            float moving = (float)_ride.Moving;
            float off = TradeMath.RideOffBy(_ride.SaidHours, moving);
            var lines = new List<string>
            {
                "ride check: reached " + at.Name + " " + Hours(took) + " h after setting off, " + Hours(moving) +
                " h of it on the move; TradeLord said " + Hours(_ride.SaidHours) + " h, so the ride on the move ran " +
                Longer(off) + " than said",
                "  night: TradeLord counted " + Hours(_ride.SaidNight) + " h of the ride at night at " +
                Nightfall.Share(_ride.NightPace) + " of your day pace, and " + Hours((float)_ride.NightMoving) +
                " h of the ride on the move were at night"
            };
            if (moving > 0f && _ride.Moved > 0f)
            {
                float pace = _ride.Moved * TradeMath.HoursAPartyTakesToCoverItsSpeed / moving;
                lines.Add("  road: TradeLord put the road at " + Nightfall.Figure(_ride.SaidDistance) +
                          " and the game moved you " + Nightfall.Figure(_ride.Moved) + ", " +
                          Longer(TradeMath.RideOffBy(_ride.SaidDistance, _ride.Moved)) + "; your Speed on the move " +
                          "came to " + Nightfall.Figure(pace) + " where TradeLord counted " +
                          Nightfall.Figure(_ride.DayPace) + " by day and " +
                          Nightfall.Figure(_ride.DayPace * _ride.NightPace) + " by night");
            }
            bool overCeiling = false;
            if (_ride.Ceiling > 0f)
            {
                float saidDays = _ride.SaidHours / 24f;
                float tookDays = moving / 24f;
                overCeiling = saidDays <= _ride.Ceiling && tookDays > _ride.Ceiling;
                lines.Add("  ceiling: " + (at.IsVillage ? "Village" : "Town") + " travel ceiling " +
                          Nightfall.Figure(_ride.Ceiling) + " days; TradeLord said " + Nightfall.Figure(saidDays) +
                          " days and the ride on the move took " + Nightfall.Figure(tookDays) +
                          (overCeiling ? ", past the ceiling TradeLord kept it under" : ""));
            }
            if (_ride.Standing > 0.05d || _ride.Fought)
                lines.Add("  left out: " + Hours((float)_ride.Standing) + " h standing still" +
                          (_ride.Fought ? " or fighting, with a fight on the way" : "") +
                          ", which TradeLord's time never counts");
            if (!float.IsNaN(off) && _ride.SaidHours >= ShortestJudged && !_ride.Fought)
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

        internal static void SayTheCaravans()
        {
            double now = CampaignTime.Now.ToHours;
            if (_followed.Count > 0)
            {
                var stale = new List<string>();
                foreach (KeyValuePair<string, Followed> one in _followed)
                    if (now - one.Value.At > CaravanKeptHours) stale.Add(one.Key);
                for (int i = 0; i < stale.Count; i++) _followed.Remove(stale[i]);
            }
            if (_arrived.Count == 0 || !Writing) return;
            _arrived.Sort();
            int late = 0, close = 0;
            for (int i = 0; i < _arrived.Count; i++)
            {
                if (_arrived[i] > 0f) late++;
                if (Math.Abs(_arrived[i]) <= CaravanCloseEnough) close++;
            }
            Log.Write("caravan ride check over the last day: " + _arrived.Count + " caravan(s) and villager(s) " +
                      "reached the town Count what is on its way had them bound for; their ride from where it first " +
                      "timed them ran a median " + Longer(TradeMath.Quantile(_arrived, 0.5f)) +
                      " than it said, the middle half from " + Longer(TradeMath.Quantile(_arrived, 0.25f)) +
                      " to " + Longer(TradeMath.Quantile(_arrived, 0.75f)) + ", " + close + " within " +
                      Percent(CaravanCloseEnough) + " and " + late + " later than said; it counted their nights at " +
                      Nightfall.CaravanPaceSaid + " of their day pace");
            _arrived.Clear();
        }

        private static string Hours(float hours) => Nightfall.Figure(hours);

        private static string Hours(double hours) => Nightfall.Figure((float)hours);

        private static string Percent(float share) =>
            float.IsNaN(share) ? "unknown" : share.ToString("0.0%", CultureInfo.InvariantCulture);

        private static string Longer(float off)
        {
            if (float.IsNaN(off)) return "by an unknown share";
            return Percent(Math.Abs(off)) + (off >= 0f ? " longer" : " shorter");
        }
    }
}
