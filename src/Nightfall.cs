using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace TradeLord
{
    internal static class Nightfall
    {
        private const float Watch = 0.15f;
        private const int FewestCaravans = 10;

        private struct Seen
        {
            internal float Speed;
            internal int Face;
        }

        private struct PartyBefore
        {
            internal bool Held;
            internal float Speed;
            internal float Base;
            internal int Face;
            internal int Weather;
        }

        internal static float PartyStep { get; private set; } = TradeMath.UsualNightStep;

        internal static float CaravanPace { get; private set; } = TradeMath.NightPaceUnread;

        private static string _partyStepFrom = "", _caravanPaceFrom = "";
        private static bool? _wasNight;
        private static PartyBefore _party;
        private static readonly Dictionary<MobileParty, Seen> _caravans = new Dictionary<MobileParty, Seen>();
        private static bool _watching;
        private static double _clockFrom = -1d, _movedFor;
        private static float _clockSaid = float.NaN;

        internal static bool Writing => Options.Current.ExtendedDebugLogging;

        internal static string PartyStepSaid =>
            Step(PartyStep) + (_partyStepFrom.Length == 0 ? " (the game's usual night, not yet read this session)"
                                                             : " (read " + _partyStepFrom + ")");

        internal static string CaravanPaceSaid =>
            Share(CaravanPace) + (_caravanPaceFrom.Length == 0 ? " (the game's usual night, not yet read this session)"
                                                               : " (read " + _caravanPaceFrom + ")");

        internal static void Forget()
        {
            PartyStep = TradeMath.UsualNightStep;
            CaravanPace = TradeMath.NightPaceUnread;
            _partyStepFrom = "";
            _caravanPaceFrom = "";
            _wasNight = null;
            _party = default(PartyBefore);
            _caravans.Clear();
            _watching = false;
            _clockFrom = -1d;
            _movedFor = 0d;
            _clockSaid = float.NaN;
        }

        internal static void Tick(float dt)
        {
            if (Campaign.Current == null || MobileParty.MainParty == null) return;
            WatchTheClock(dt);
            bool night = Campaign.Current.IsNight;
            if (_wasNight.HasValue && night != _wasNight.Value)
            {
                if (_watching)
                {
                    Guard.Run("Nightfall.Party", () => ReadTheParty(night));
                    Guard.Run("Nightfall.Caravans", () => ReadTheCaravans(night));
                }
                else if (Writing)
                    Log.Write("night check at " + Turned(night) + ": the clock ran past the turn too fast to watch " +
                              "it, so TradeLord keeps " + PartyStepSaid + " for your party and " + CaravanPaceSaid +
                              " of the day pace at night for caravans and villagers");
                Travel.ReadThePaceAgain();
            }
            _wasNight = night;
            float hour = CampaignTime.Now.CurrentHourInDay;
            if (NearlyTurning(hour))
            {
                Guard.Run("Nightfall.Watch", Remember);
                _watching = true;
            }
            else if (_watching)
            {
                _watching = false;
                _party.Held = false;
                _caravans.Clear();
            }
        }

        private static bool NearlyTurning(float hour)
        {
            float toDawn = CampaignTime.SunRise - hour;
            float toDusk = CampaignTime.SunSet - hour;
            if (toDawn < 0f) toDawn += 24f;
            if (toDusk < 0f) toDusk += 24f;
            return (toDawn > 0f && toDawn <= Watch) || (toDusk > 0f && toDusk <= Watch);
        }

        private static void Remember()
        {
            MobileParty main = MobileParty.MainParty;
            _party.Held = Steady(main);
            if (_party.Held)
            {
                _party.Speed = main.Speed;
                _party.Base = main.LastCalculatedBaseSpeed;
                _party.Face = main.CurrentNavigationFace.FaceIndex;
                _party.Weather = WeatherAt(main);
            }
            _caravans.Clear();
            MBReadOnlyList<MobileParty> all = MobileParty.All;
            if (all == null) return;
            for (int i = 0; i < all.Count; i++)
            {
                MobileParty one = all[i];
                if (one == null || (!one.IsCaravan && !one.IsVillager) || !Steady(one)) continue;
                _caravans[one] = new Seen { Speed = one.Speed, Face = one.CurrentNavigationFace.FaceIndex };
            }
        }

        private static bool Steady(MobileParty party) =>
            party.IsActive && party.Army == null && party.MapEvent == null && !party.IsCurrentlyAtSea;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int WeatherAt(MobileParty party) =>
            (int)Campaign.Current.Models.MapWeatherModel.GetWeatherEventInPosition(party.Position.ToVec2());

        private static string Turned(bool nightFell) => nightFell ? "nightfall" : "daybreak";

        private static void ReadTheParty(bool nightFell)
        {
            MobileParty main = MobileParty.MainParty;
            string turn = Turned(nightFell);
            string why = null;
            if (!_party.Held) why = "your party was at sea, in a fight or in an army as it turned";
            else if (!Steady(main)) why = "your party went to sea, into a fight or into an army as it turned";
            else if (main.CurrentNavigationFace.FaceIndex != _party.Face) why = "your party crossed onto other ground as it turned";
            else if (Math.Abs(main.LastCalculatedBaseSpeed - _party.Base) > 1e-4f) why = "your party's own pace changed as it turned";
            else if (WeatherAt(main) != _party.Weather) why = "the weather changed where your party stands as it turned";
            PartySpeedModel model = Campaign.Current.Models.PartySpeedCalculatingModel;
            ExplainedNumber speed = why == null ? model.CalculateFinalSpeed(main, model.CalculateBaseSpeed(main)) : default(ExplainedNumber);
            float after = why == null ? main.Speed : 0f;
            if (why == null && (!TradeMath.ClearOfLimits(after, speed.LimitMinValue, speed.LimitMaxValue) ||
                                !TradeMath.ClearOfLimits(_party.Speed, speed.LimitMinValue, speed.LimitMaxValue)))
                why = "the game held your party's pace at its slowest or fastest";
            float step = why == null ? TradeMath.NightStepFrom(_party.Speed, after, speed.SumOfFactors, nightFell) : float.NaN;
            if (why == null && !TradeMath.LikeANightStep(step)) why = "the change did not look like the night alone";
            if (why != null)
            {
                if (Writing)
                    Log.Write("night check at " + turn + ": the night step was not read, because " + why +
                              ", so TradeLord keeps " + PartyStepSaid);
                return;
            }
            float said = TradeMath.PaceAfterTheTurn(_party.Speed, speed.SumOfFactors, after, PartyStep, nightFell);
            if (Writing)
                Log.Write("night check at " + turn + ": the game took your party from a Speed of " + Figure(_party.Speed) +
                          " to " + Figure(after) + ", a night step of " + Step(step) + " of its pace; TradeLord had counted " +
                          "on " + PartyStepSaid + ", which would have made it " + Figure(said) + ", " +
                          Off(said, after) + " from what the game did");
            PartyStep = step;
            _partyStepFrom = "at " + turn + " on " + Today();
        }

        private static void ReadTheCaravans(bool nightFell)
        {
            string turn = Turned(nightFell);
            var kept = new List<float>();
            int moved = 0;
            foreach (KeyValuePair<MobileParty, Seen> one in _caravans)
            {
                MobileParty party = one.Key;
                if (party == null || !Steady(party) || party.CurrentNavigationFace.FaceIndex != one.Value.Face)
                {
                    moved++;
                    continue;
                }
                float after = party.Speed;
                float before = one.Value.Speed;
                if (!(after > 0f) || !(before > 0f)) continue;
                kept.Add(nightFell ? after / before : before / after);
            }
            if (kept.Count < FewestCaravans)
            {
                if (Writing)
                    Log.Write("night check at " + turn + ": only " + kept.Count + " caravan(s) and villager(s) stayed on " +
                              "the same ground as it turned, too few to read, so Count what is on its way keeps " +
                              CaravanPaceSaid + " of their day pace at night");
                return;
            }
            kept.Sort();
            float middle = TradeMath.Quantile(kept, 0.5f);
            if (Writing)
                Log.Write("night check at " + turn + ": " + kept.Count + " caravan(s) and villager(s) on land kept " +
                          Share(middle) + " of their day pace at night, the middle half of them " +
                          Share(TradeMath.Quantile(kept, 0.25f)) + " to " + Share(TradeMath.Quantile(kept, 0.75f)) +
                          " (" + moved + " set aside for changing ground or stopping as it turned); Count what is on " +
                          "its way had counted on " + CaravanPaceSaid);
            CaravanPace = TradeMath.NightPaceKept(middle);
            _caravanPaceFrom = "at " + turn + " on " + Today();
        }

        private static void WatchTheClock(float dt)
        {
            double now = CampaignTime.Now.ToHours;
            if (_clockFrom < 0d || now < _clockFrom)
            {
                _clockFrom = now;
                _movedFor = 0d;
                return;
            }
            if (dt > 0f) _movedFor += dt;
            double hours = now - _clockFrom;
            if (hours < 24d || _movedFor <= 0d) return;
            float seen = (float)(hours / _movedFor);
            _clockFrom = now;
            _movedFor = 0d;
            if (!float.IsNaN(_clockSaid) && Math.Abs(seen - _clockSaid) < 0.001f) return;
            _clockSaid = seen;
            if (!Writing) return;
            Log.Write("game clock check: over the last " + Figure((float)hours) + " hours the game moved every party " +
                      "as far as its Speed each " + seen.ToString("0.000", CultureInfo.InvariantCulture) +
                      " hours, and TradeLord times every ride at " +
                      TradeMath.HoursAPartyTakesToCoverItsSpeed.ToString("0.000", CultureInfo.InvariantCulture) +
                      (Math.Abs(seen - TradeMath.HoursAPartyTakesToCoverItsSpeed) < 0.01f
                           ? ", so the two agree"
                           : ", so the two disagree and every travel time is off by the difference"));
        }

        private static string Today()
        {
            CampaignTime now = CampaignTime.Now;
            CampaignTimeModel model = Campaign.Current?.Models?.CampaignTimeModel;
            int day = model == null ? (int)Math.Floor(now.ToDays)
                                    : (int)Math.Floor(now.ToDays) - (int)Math.Floor(model.CampaignStartTime.ToDays);
            return "day " + day.ToString(CultureInfo.InvariantCulture);
        }

        internal static string Figure(float value) =>
            float.IsNaN(value) ? "unknown" : value.ToString("0.00", CultureInfo.InvariantCulture);

        internal static string Share(float value) =>
            float.IsNaN(value) ? "unknown" : value.ToString("0.000", CultureInfo.InvariantCulture);

        private static string Step(float value) =>
            (value >= 0f ? "+" : "") + value.ToString("0.000", CultureInfo.InvariantCulture);

        private static string Off(float said, float happened)
        {
            if (float.IsNaN(said) || !(happened > 0f)) return "which could not be weighed";
            float off = Math.Abs(said - happened) / happened;
            return off.ToString("0.0%", CultureInfo.InvariantCulture) + " off";
        }
    }
}
