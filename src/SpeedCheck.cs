using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace TradeLord
{
    public class SpeedWatchBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
        }

        public override void SyncData(IDataStore dataStore) { }

        private void OnTick(float dt)
        {
            Guard.Run("SpeedCheck.Tick", dt, SpeedCheck.Tick);
        }
    }

    internal static class SpeedCheck
    {
        private const float Watch = 0.15f;
        private const float PacesAgreeWithin = 1e-4f;
        private const float ClocksAgreeWithin = 0.01f;

        private struct PartyBefore
        {
            internal bool Held;
            internal float Speed;
            internal float Base;
            internal int Face;
            internal int Weather;
        }

        private struct SlowBefore
        {
            internal float Factors;
            internal float Base;
            internal int Face;
            internal int Sky;
            internal bool Night;
        }

        private struct SkyBefore
        {
            internal bool Held;
            internal int Sky;
            internal float Factors;
            internal float Base;
            internal int Face;
        }

        private static PartyBefore _party;
        private static SlowBefore _slowBefore;
        private static SkyBefore _sky;
        private static bool _wasSlowed, _slowWatching, _watching;
        private static bool? _wasNight;
        private static double _clockFrom = -1d, _movedFor;
        private static float _clockSaid = float.NaN;
        private static int _readHour = -1;

        internal static bool Writing => Options.Current.ExtendedDebugLogging;

        internal static void Forget()
        {
            _party = default(PartyBefore);
            _slowBefore = default(SlowBefore);
            _sky = default(SkyBefore);
            _wasSlowed = false;
            _slowWatching = false;
            _watching = false;
            _wasNight = null;
            _clockFrom = -1d;
            _movedFor = 0d;
            _clockSaid = float.NaN;
            _readHour = -1;
        }

        internal static void Tick(float dt)
        {
            if (Campaign.Current == null || MobileParty.MainParty == null) return;
            WatchTheClock(dt);
            Guard.Run("SpeedCheck.Read", SayWhatWasRead);
            bool night = Campaign.Current.IsNight;
            if (_wasNight.HasValue && night != _wasNight.Value)
            {
                if (_watching) Guard.Run("SpeedCheck.Night", () => ReadTheParty(night));
                else if (Writing)
                    Log.Write("night check at " + Turned(night) +
                              ": the clock ran past the turn too fast to watch it");
            }
            _wasNight = night;
            Guard.Run("SpeedCheck.Disorder", WatchTheDisorder);
            Guard.Run("SpeedCheck.Weather", WatchTheWeather);
            if (NearlyTurning(CampaignTime.Now.CurrentHourInDay))
            {
                Guard.Run("SpeedCheck.Remember", Remember);
                _watching = true;
            }
            else if (_watching)
            {
                _watching = false;
                _party.Held = false;
            }
        }

        private static void SayWhatWasRead()
        {
            if (!Writing) return;
            int hour = (int)CampaignTime.Now.ToHours;
            if (hour == _readHour) return;
            _readHour = hour;
            TradeMath.Pace pace = Travel.PaceNow();
            if (pace == null) return;
            if (pace.Even)
            {
                Log.Write("ground step read: your party's pace could not be read from the game, because " +
                          (Travel.PaceBecause() ?? "it was not read") + ", so every ride is timed at one pace");
                return;
            }
            Log.Write("ground step read: your party's pace moves by " +
                      Step(pace.OnGround[TradeMath.GroundForest]) + " in forest, " +
                      Step(pace.OnGround[TradeMath.GroundWater]) + " on water and fords, " +
                      Step(pace.OnGround[TradeMath.GroundDesert]) + " in desert, " +
                      Step(pace.OnGround[TradeMath.GroundPlain]) + " on plains and " +
                      Step(pace.Night) + " at night, with a snow step of " + Step(pace.Snow) +
                      ", a wet ground step of " + Step(pace.Wet) +
                      " and a Disorganized step of " + Step(pace.Slow));
        }

        private static bool NearlyTurning(float hour)
        {
            float sunrise = Campaign.Current.Models.MapWeatherModel == null ? 6f : CampaignTime.SunRise;
            float sunset = CampaignTime.SunSet;
            return Near(hour, sunrise) || Near(hour, sunset);
        }

        private static bool Near(float hour, float turn) => Math.Abs(hour - turn) <= Watch;

        private static string Turned(bool nightFell) => nightFell ? "nightfall" : "daybreak";

        private static bool Steady(MobileParty party) =>
            party.IsActive && party.Army == null && party.MapEvent == null && !party.IsCurrentlyAtSea;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int WeatherAt(MobileParty party) =>
            (int)Campaign.Current.Models.MapWeatherModel.GetWeatherEventInPosition(party.Position.ToVec2());

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ExplainedNumber Explained(MobileParty party)
        {
            PartySpeedModel model = Campaign.Current.Models.PartySpeedCalculatingModel;
            return model.CalculateFinalSpeed(party, model.CalculateBaseSpeed(party));
        }

        private static void Remember()
        {
            MobileParty main = MobileParty.MainParty;
            _party.Held = Steady(main);
            if (!_party.Held) return;
            _party.Speed = main.Speed;
            _party.Base = main.LastCalculatedBaseSpeed;
            _party.Face = main.CurrentNavigationFace.FaceIndex;
            _party.Weather = WeatherAt(main);
        }

        private static void ReadTheParty(bool nightFell)
        {
            MobileParty main = MobileParty.MainParty;
            string turn = Turned(nightFell);
            TradeMath.Pace pace = Travel.PaceNow();
            string why = null;
            if (pace == null || pace.Even) why = "your party's pace was not read from the game this hour";
            else if (!_party.Held) why = "your party was at sea, in a fight or in an army as it turned";
            else if (!Steady(main)) why = "your party went to sea, into a fight or into an army as it turned";
            else if (main.CurrentNavigationFace.FaceIndex != _party.Face)
                why = "your party crossed onto other ground as it turned";
            else if (Math.Abs(main.LastCalculatedBaseSpeed - _party.Base) > PacesAgreeWithin)
                why = "your party's own pace changed as it turned";
            else if (WeatherAt(main) != _party.Weather)
                why = "the weather changed where your party stands as it turned";
            ExplainedNumber speed = why == null ? Explained(main) : default(ExplainedNumber);
            float after = why == null ? main.Speed : 0f;
            if (why == null && (!TradeMath.ClearOfLimits(after, speed.LimitMinValue, speed.LimitMaxValue) ||
                                !TradeMath.ClearOfLimits(_party.Speed, speed.LimitMinValue, speed.LimitMaxValue)))
                why = "the game held your party's pace at its slowest or fastest";
            float step = why == null
                ? TradeMath.NightStepFrom(_party.Speed, after, speed.SumOfFactors, nightFell)
                : float.NaN;
            if (why == null && !TradeMath.LikeAStep(step)) why = "the change did not look like the night alone";
            if (why != null)
            {
                if (Writing) Log.Write("night check at " + turn + ": the night step was not read, because " + why);
                return;
            }
            float said = TradeMath.PaceAfterTheTurn(_party.Speed, speed.SumOfFactors, after, pace.Night, nightFell);
            if (!Writing) return;
            Log.Write("night check at " + turn + ": the game took your party from a Speed of " +
                      Figure(_party.Speed) + " to " + Figure(after) + ", a night step of " + Step(step) +
                      " of its pace; TradeLord read " + Step(pace.Night) + " from the game, which would have made it " +
                      Figure(said) + ", " + Off(said, after) + " from what the game did");
        }

        private static int SkyBits(MobileParty party)
        {
            Travel.SkyOver(party, out bool wet, out bool snowy);
            return (wet ? 1 : 0) | (snowy ? 2 : 0);
        }

        private static string Turned(bool snowyBefore, bool wetBefore, bool snowyAfter, bool wetAfter)
        {
            if (snowyBefore != snowyAfter) return snowyAfter ? "snow began" : "the snow stopped";
            if (wetBefore != wetAfter) return wetAfter ? "the ground turned wet" : "the ground dried";
            return "the weather turned";
        }

        private static void WatchTheWeather()
        {
            MobileParty main = MobileParty.MainParty;
            bool steady = Steady(main);
            if (_sky.Held && steady)
            {
                int now = SkyBits(main);
                if (now != _sky.Sky) ReadTheTurn(main, now);
            }
            _sky.Held = steady;
            if (!steady) return;
            ExplainedNumber speed = Explained(main);
            _sky.Sky = SkyBits(main);
            _sky.Factors = speed.SumOfFactors;
            _sky.Base = speed.BaseNumber;
            _sky.Face = main.CurrentNavigationFace.FaceIndex;
        }

        private static void ReadTheTurn(MobileParty main, int now)
        {
            TradeMath.Pace pace = Travel.PaceNow();
            string why = null;
            if (pace == null || pace.Even) why = "your party's pace was not read from the game this hour";
            else if (main.CurrentNavigationFace.FaceIndex != _sky.Face)
                why = "your party crossed onto other ground as the weather turned";
            ExplainedNumber speed = why == null ? Explained(main) : default(ExplainedNumber);
            if (why == null && Math.Abs(speed.BaseNumber - _sky.Base) > PacesAgreeWithin)
                why = "your party's own pace changed as the weather turned";
            bool snowyBefore = (_sky.Sky & 2) != 0, wetBefore = (_sky.Sky & 1) != 0;
            bool snowyAfter = (now & 2) != 0, wetAfter = (now & 1) != 0;
            float snow = float.NaN, wet = float.NaN;
            if (why == null)
                TradeMath.WeatherSteps(snowyBefore, wetBefore, snowyAfter, wetAfter, _sky.Factors,
                                       speed.SumOfFactors, pace.Wet, out snow, out wet);
            if (!Writing) return;
            string head = "weather step as " + Turned(snowyBefore, wetBefore, snowyAfter, wetAfter) +
                          " over your party";
            if (why != null)
            {
                Log.Write(head + ": the step was not read, because " + why);
                return;
            }
            string said = head + ": the game took its pace from a sum of " + Step(_sky.Factors) + " to " +
                          Step(speed.SumOfFactors);
            if (!float.IsNaN(snow) && TradeMath.LikeAStep(snow))
                Log.Write(said + ", a snow step of " + Step(snow) + "; TradeLord read " + Step(pace.Snow) +
                          " from the game");
            else if (!float.IsNaN(wet) && TradeMath.LikeAStep(wet))
                Log.Write(said + ", a wet ground step of " + Step(wet) + "; TradeLord read " + Step(pace.Wet) +
                          " from the game");
            else
                Log.Write(said + ", which was not read as a step because snow and wet ground turned together, " +
                          "or the change did not look like the weather alone");
        }

        private static void WatchTheDisorder()
        {
            MobileParty main = MobileParty.MainParty;
            bool slowed = main.IsDisorganized;
            if (_wasSlowed && !slowed && _slowWatching) ReadTheEndOfDisorder(main);
            _wasSlowed = slowed;
            _slowWatching = slowed && main.DisorganizedUntilTime.RemainingHoursFromNow <= Watch &&
                            RememberTheDisorder(main);
        }

        private static bool RememberTheDisorder(MobileParty main)
        {
            if (!Steady(main)) return false;
            ExplainedNumber speed = Explained(main);
            _slowBefore = new SlowBefore
            {
                Factors = speed.SumOfFactors,
                Base = speed.BaseNumber,
                Face = main.CurrentNavigationFace.FaceIndex,
                Sky = WeatherAt(main),
                Night = Campaign.Current.IsNight
            };
            return true;
        }

        private static void ReadTheEndOfDisorder(MobileParty main)
        {
            TradeMath.Pace pace = Travel.PaceNow();
            string why = null;
            if (pace == null || pace.Even) why = "your party's pace was not read from the game this hour";
            else if (!Steady(main)) why = "your party went to sea, into a fight or into an army as it wore off";
            else if (main.CurrentNavigationFace.FaceIndex != _slowBefore.Face)
                why = "your party crossed onto other ground as it wore off";
            else if (Campaign.Current.IsNight != _slowBefore.Night) why = "night fell or day broke as it wore off";
            else if (WeatherAt(main) != _slowBefore.Sky) why = "the weather over your party turned as it wore off";
            ExplainedNumber speed = why == null ? Explained(main) : default(ExplainedNumber);
            if (why == null && Math.Abs(speed.BaseNumber - _slowBefore.Base) > PacesAgreeWithin)
                why = "your party's own pace changed as it wore off";
            float step = why == null ? _slowBefore.Factors - speed.SumOfFactors : float.NaN;
            if (why == null && !TradeMath.LikeAStep(step)) why = "the change did not look like Disorganized alone";
            if (why != null)
            {
                if (Writing)
                    Log.Write("disorder check: Disorganized wore off, but its step was not read, because " + why);
                return;
            }
            if (!Writing) return;
            Log.Write("disorder check: Disorganized wore off; the game took your party's pace from a sum of " +
                      Step(_slowBefore.Factors) + " to " + Step(speed.SumOfFactors) +
                      ", a Disorganized step of " + Step(step) + "; TradeLord read " + Step(pace.Slow) +
                      " from the game");
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
                      (Math.Abs(seen - TradeMath.HoursAPartyTakesToCoverItsSpeed) < ClocksAgreeWithin
                           ? ", so the two agree"
                           : ", so the two disagree and every travel time is off by the difference"));
        }

        private static string Figure(float value) =>
            float.IsNaN(value) ? "unknown" : value.ToString("0.00", CultureInfo.InvariantCulture);

        private static string Step(float value) =>
            float.IsNaN(value) ? "unknown"
                               : (value >= 0f ? "+" : "") + value.ToString("0.000", CultureInfo.InvariantCulture);

        private static string Off(float said, float happened)
        {
            if (float.IsNaN(said) || !(happened > 0f)) return "which could not be weighed";
            return (Math.Abs(said - happened) / happened).ToString("0.0%", CultureInfo.InvariantCulture) + " off";
        }
    }
}
