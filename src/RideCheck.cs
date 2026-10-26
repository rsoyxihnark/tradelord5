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
            Guard.Run("RideCheck.Tick", dt, RideCheck.Tick);
        }

        private void OnSettlementEntered(MobileParty party, Settlement settlement, Hero hero)
        {
            if (party == null || settlement == null) return;
            Guard.Run("RideCheck.Arrived", () => RideCheck.Arrived(party, settlement));
        }

        private void OnDailyTick()
        {
            Guard.Run("RideCheck.Daily", RideCheck.SayTheCaravans);
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
        }

        private struct Followed
        {
            internal string To;
            internal double At;
            internal float SaidHours;
            internal float StraightHours;
            internal bool Dodged;
        }

        private static Open _ride;
        private static bool _riding;
        private static Settlement _triedTo;
        private static int _judged, _close, _pastCeiling;
        private static float _offTotal;
        private static readonly Dictionary<string, Followed> _followed = new Dictionary<string, Followed>();
        private static readonly List<float> _arrived = new List<float>();
        private static readonly List<float> _arrivedStraight = new List<float>();
        private static readonly List<float> _dodged = new List<float>();

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
            _arrivedStraight.Clear();
            _dodged.Clear();
        }

        internal static bool Dodging(MobileParty party) =>
            party != null && (party.MapEvent != null || party.ShortTermBehavior == AiBehavior.FleeToPoint ||
                              party.ShortTermBehavior == AiBehavior.FleeToGate);

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
                Log.Write("ride check: you turned away from " + Tongue.Named(_ride.To.Name, _ride.To.StringId) +
                          " after " + Hours((float)(now - _ride.From)) + " h, so that ride is not judged");
            }
            if (to != _triedTo && party.CurrentSettlement == null)
            {
                _triedTo = to;
                if (!_riding && to != null) SetOff(to, now);
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

        internal static void Arrived(MobileParty party, Settlement at)
        {
            if (party == MobileParty.MainParty) Reached(at);
            else if (party.IsCaravan || party.IsVillager) ACaravanReached(party, at);
        }

        private static void Reached(Settlement at)
        {
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
