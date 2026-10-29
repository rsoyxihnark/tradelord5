using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Helpers;
using SandBox;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

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
            MobileParty party = MobileParty.MainParty;
            float sum = 0f, min = float.MaxValue;
            int count = 0;
            FleetOf(party.Ships, ref sum, ref min, ref count);
            foreach (MobileParty attached in party.AttachedParties) FleetOf(attached.Ships, ref sum, ref min, ref count);
            return count == 0 ? 0f : TradeMath.FleetSpeed(sum, count, min);
        }

        private static void FleetOf(MBReadOnlyList<Ship> ships, ref float sum, ref float min, ref int count)
        {
            if (ships == null) return;
            for (int i = 0; i < ships.Count; i++)
            {
                float sp = ships[i].GetCampaignSpeed();
                sum += sp;
                if (sp < min) min = sp;
                count++;
            }
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

        private static float LandSpeed()
        {
            return SpeedByDay(MobileParty.MainParty);
        }

        internal static float SpeedByDay(MobileParty party)
        {
            float now = party.Speed;
            if (_dayStepUnbound) return now;
            try { return LandSpeedByDay(party, now); }
            catch (Exception e) when (Unbound(e))
            {
                _dayStepUnbound = true;
                Log.Write("binding: the speed a party has by day could not be read from this game version (" +
                          e.GetType().Name + ": " + e.Message + "), so a party's speed is taken as it is now " +
                          "until the game restarts");
                return now;
            }
            catch { return now; }
        }

        private static bool _dayStepUnbound, _othersUnbound;

        private static bool Unbound(Exception e) => e is MissingMemberException || e is TypeLoadException;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static float LandSpeedByDay(MobileParty party, float now)
        {
            if (Campaign.Current == null || !Campaign.Current.IsNight || party.IsCurrentlyAtSea) return now;
            string dark = (typeof(DefaultPartySpeedCalculatingModel).GetField(
                               "_night", BindingFlags.Static | BindingFlags.NonPublic)
                               ?.GetValue(null) as TextObject)?.ToString();
            if (string.IsNullOrEmpty(dark)) return now;
            string runner = DefaultPerks.Scouting.NightRunner?.Name?.ToString();
            PartySpeedModel model = Campaign.Current.Models.PartySpeedCalculatingModel;
            ExplainedNumber speed = model.CalculateFinalSpeed(party, model.CalculateBaseSpeed(party, true));
            float night = 0f;
            foreach (var (name, number) in speed.GetLines())
                if (name == dark || name == runner) night += number;
            float whole = speed.BaseNumber + speed.BaseNumber * speed.SumOfFactors;
            ExplainedNumber daylight = new ExplainedNumber(1f);
            PerkHelper.AddPerkBonusForParty(DefaultPerks.Scouting.DayTraveler, party, true, ref daylight);
            whole += speed.BaseNumber * daylight.SumOfFactors;
            return TradeMath.SpeedByDay(now, speed.ResultNumber,
                                        MathF.Clamp(whole - night, speed.LimitMinValue, speed.LimitMaxValue));
        }


        private sealed class Road
        {
            internal float Length;
            internal float LandRatio;
            internal List<TradeMath.Run> Runs;
            internal TradeMath.Sail[] Sails;
            internal List<TradeMath.Run> Afloat, Ashore;
            internal float GoingAshore;
            internal bool Walked;
            internal int Hour = -1;
            internal float Days;
        }

        private sealed class GroundFactors
        {
            internal float Forest, Culture, Water, Desert, Plain, Night, Day, Snow, Slow, Cavalry, Footmen, Wet;

            internal float On(int ground)
            {
                switch (ground)
                {
                    case TradeMath.GroundPlain: return Plain;
                    case TradeMath.GroundForest: return Forest + Culture;
                    case TradeMath.GroundWater: return Water;
                    case TradeMath.GroundDesert: return Desert;
                    default: return 0f;
                }
            }
        }

        private sealed class SpeedLines
        {
            internal int Hour = -1;
            internal bool Whole;
            internal float Forest, Water, Night, Snow, Desert, Slow, Horses, Spare;
            internal string ForestLine, FordLine, DesertLine, NightLine, SnowLine, CavalryLine, FootmenLine;
            internal string CavalryWetLine, FootmenWetLine, SlowLine;
            internal string Pathfinder, DesertBorn, NightRunner, DayTraveler, Tracker, Culture;
        }

        private sealed class Outlook
        {
            internal readonly List<double> At = new List<double>();
            internal readonly List<byte> Sky = new List<byte>();
            internal int[] Writers;
            internal CampaignTime[] Due;
            internal bool Done;
        }

        private sealed class Skies
        {
            internal CampaignTime Taken;
            internal CampaignTime Next;
            internal CampaignTime Every;
            internal CampaignTime Round;
            internal int Count;
            internal int Last;
            internal WeatherNode[] Nodes;
            internal MapWeatherModel Game;
            internal DefaultMapWeatherModel Ahead;
            internal byte[] Now;
            internal readonly Dictionary<int, Outlook> Outlooks = new Dictionary<int, Outlook>();
            internal int Checked, Agreed;
        }

        private sealed class SkyFrom : TradeMath.ISky
        {
            private readonly double _from;

            internal SkyFrom(double from)
            {
                _from = from;
            }

            public void Read(int cell, double hours, out bool wet, out bool snowy, out double changes)
            {
                try { ReadTheSky(cell, _from, hours, out wet, out snowy, out changes); }
                catch { SkyAsItIs.Held.Read(cell, hours, out wet, out snowy, out changes); }
            }
        }

        private sealed class SkyAsItIs : TradeMath.ISky
        {
            internal static readonly SkyAsItIs Held = new SkyAsItIs();

            public void Read(int cell, double hours, out bool wet, out bool snowy, out double changes)
            {
                byte sky = cell >= 0 && Campaign.Current != null
                    ? SkyNow(Campaign.Current.Models.MapWeatherModel, PlaceIn(cell, null))
                    : (byte)0;
                wet = (sky & Wet) != 0;
                snowy = (sky & Snowy) != 0;
                changes = double.PositiveInfinity;
            }
        }

        private const float AllOnLand = 0.999f;
        private const float Stride = 1f;
        private const int MostPathsToOneRoad = 8;
        private const float ReachedSquared = 0.0001f;
        private const float StoodStill = 0.0001f;
        private const float LinesAgreeWithin = 0.002f;
        private const float SpeedsAgreeWithin = 0.001f;
        private const float WetGroundTakesFromRiders = 0.3f;
        private const float MostHoursGoingAshore = 48f;
        private const float MostUnreadAshore = 2f * Stride;
        private const double LongestForecastHours = 720d;
        private const int MostWeatherTurnsKept = 200000;
        private const byte Wet = 1;
        private const byte Snowy = 2;
        private const string NavalSpeedModel = "NavalDLC.GameComponents.NavalDLCPartySpeedCalculationModel";
        private const string NavalWeatherModel = "NavalDLC.GameComponents.NavalDLCMapWeatherModel";
        private const string FairWindsPerk = "FairWinds";
        private const string ShockAndAwePerk = "ShockAndAwe";
        private const string OldSaltsTouchPerk = "OldSaltsTouch";
        private const string FavorableTidePerk = "FavorableTide";
        private const string RiverRaiderPerk = "RiverRaider";
        private const string FleetCommanderPerk = "FleetCommander";
        private const string ShoreMasterPerk = "ShoreMaster";
        private const string VeteransWisdomPerk = "VeteransWisdom";
        private const string ChainToOarsPerk = "ChainToOars";
        private const string SailingSpeedSkill = "WindBonus";
        private const string NordShipsFeat = "nord_ship_movemenet_increase";
        private const string NavalStoryline = "NavalDLC.Storyline.NavalStorylineData";
        private const float TheFasterMapAdds = 0.25f;
        private const BindingFlags Hidden = BindingFlags.Static | BindingFlags.NonPublic;
        private const BindingFlags Inside = BindingFlags.Instance | BindingFlags.NonPublic;

        private static readonly NavigationPath _path = new NavigationPath();
        private static readonly List<TradeMath.Piece> _pieces = new List<TradeMath.Piece>();
        private static readonly Dictionary<int, int> _groundOfFace = new Dictionary<int, int>();
        private static readonly SpeedLines _lines = new SpeedLines();
        private static readonly Dictionary<(int Node, double Due), byte> _worked = new Dictionary<(int, double), byte>();
        private static int _speedHour = -1;
        private static TradeMath.Pace _pace;
        private static string _evenBecause;
        private static CampaignTime _moment;

        private static int _dim;
        private static float _cellWide, _cellHigh;
        private static int[][] _writers;
        private static int[] _slotOf;
        private static Skies _skies;
        private static int _skyHour = -1;
        private static string _skyBecause;
        private static bool _skySeen;
        private static CampaignTime _firstSky;

        private static int _meterHour = -1;
        private static int _partyWalks, _pairWalks, _otherWalks, _paths, _probes, _forecasts, _rides;
        private static int _probesThisWalk, _pathsThisWalk;
        private static string _slowestWalkSaid;
        private static int _walkedBehind, _seaWalks;
        private static int _othersOnTheirPath, _othersOnAFreshPath, _othersGuessed;
        private static long _walkTicks, _pathTicks, _forecastTicks, _rideTicks, _slowestWalk;
        private static long _frameTicks, _busiestFrame;

        private static int GroundOf(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.Forest:
                    return TradeMath.GroundForest;
                case TerrainType.Water:
                case TerrainType.River:
                case TerrainType.UnderBridge:
                case TerrainType.Bridge:
                case TerrainType.Fording:
                    return TradeMath.GroundWater;
                case TerrainType.Desert:
                case TerrainType.Dune:
                    return TradeMath.GroundDesert;
                case TerrainType.Plain:
                case TerrainType.Steppe:
                    return TradeMath.GroundPlain;
                default:
                    return TradeMath.GroundElse;
            }
        }

        private static float Effect(FieldInfo field) => field?.GetRawConstantValue() is float f ? f : float.NaN;

        private static string LineName(FieldInfo field) => (field?.GetValue(null) as TextObject)?.ToString();

        private static float PerkFactor(PerkObject perk, MobileParty party, bool primary)
        {
            ExplainedNumber bonus = new ExplainedNumber(1f);
            PerkHelper.AddPerkBonusForParty(perk, party, primary, ref bonus);
            return bonus.SumOfFactors;
        }

        private static float Factor(float value) => value.ApproximatelyEqualsTo(0f) ? 0f : value;

        internal static PartySpeedModel UnderWarSails(PartySpeedModel model)
        {
            if (model?.GetType().FullName != NavalSpeedModel) return model;
            MethodInfo under = typeof(PartySpeedModel).GetMethod("get_BaseModel",
                                                                 BindingFlags.Instance | BindingFlags.NonPublic);
            return under?.Invoke(model, null) as PartySpeedModel;
        }

        private static bool TheGamesOwn(PartySpeedModel model) =>
            UnderWarSails(model)?.GetType() == typeof(DefaultPartySpeedCalculatingModel);

        private static MapWeatherModel TheGamesOwnWeather(MapWeatherModel model)
        {
            if (model == null) return null;
            Type type = model.GetType();
            if (type == typeof(DefaultMapWeatherModel)) return model;
            if (type.FullName != NavalWeatherModel) return null;
            MethodInfo under = typeof(MapWeatherModel).GetMethod("get_BaseModel",
                                                                 BindingFlags.Instance | BindingFlags.NonPublic);
            MapWeatherModel game = under?.Invoke(model, null) as MapWeatherModel;
            return game?.GetType() == typeof(DefaultMapWeatherModel) ? game : null;
        }

        private static SpeedLines Lines()
        {
            int hour = (int)CampaignTime.Now.ToHours;
            if (_lines.Hour == hour) return _lines.Whole ? _lines : null;
            _lines.Whole = false;
            _lines.Hour = hour;
            Type own = typeof(DefaultPartySpeedCalculatingModel);
            _lines.Forest = Effect(own.GetField("MovingAtForestEffect", Hidden));
            _lines.Water = Effect(own.GetField("MovingAtWaterEffect", Hidden));
            _lines.Night = Effect(own.GetField("MovingAtNightEffect", Hidden));
            _lines.Snow = Effect(own.GetField("MovingOnSnowEffect", Hidden));
            _lines.Desert = Effect(own.GetField("MovingInDesertEffect", Hidden));
            _lines.Slow = Effect(own.GetField("DisorganizedEffect", Hidden));
            _lines.Horses = Effect(own.GetField("CavalryEffect", Hidden));
            _lines.Spare = Effect(own.GetField("MountedFootMenEffect", Hidden));
            _lines.ForestLine = LineName(own.GetField("_movingInForest", Hidden));
            _lines.FordLine = LineName(own.GetField("_fordEffect", Hidden));
            _lines.DesertLine = LineName(own.GetField("_desert", Hidden));
            _lines.NightLine = LineName(own.GetField("_night", Hidden));
            _lines.SnowLine = LineName(own.GetField("_snow", Hidden));
            _lines.CavalryLine = LineName(own.GetField("_textCavalry", Hidden));
            _lines.FootmenLine = LineName(own.GetField("_textMountedFootmen", Hidden));
            _lines.CavalryWetLine = LineName(own.GetField("_textCavalryWeatherPenalty", Hidden));
            _lines.FootmenWetLine = LineName(own.GetField("_textMountedFootmenWeatherPenalty", Hidden));
            _lines.SlowLine = LineName(own.GetField("_textDisorganized", Hidden));
            _lines.Pathfinder = DefaultPerks.Scouting.Pathfinder?.Name?.ToString();
            _lines.DesertBorn = DefaultPerks.Scouting.DesertBorn?.Name?.ToString();
            _lines.NightRunner = DefaultPerks.Scouting.NightRunner?.Name?.ToString();
            _lines.DayTraveler = DefaultPerks.Scouting.DayTraveler?.Name?.ToString();
            _lines.Tracker = DefaultPerks.Scouting.Tracker?.Name?.ToString();
            _lines.Culture = GameTexts.FindText("str_culture").ToString();
            _lines.Whole = !float.IsNaN(_lines.Forest) && !float.IsNaN(_lines.Water) && !float.IsNaN(_lines.Night) &&
                           !float.IsNaN(_lines.Snow) && !float.IsNaN(_lines.Desert) && !float.IsNaN(_lines.Slow) &&
                           !float.IsNaN(_lines.Horses) && !float.IsNaN(_lines.Spare) && _lines.ForestLine != null &&
                           _lines.FordLine != null && _lines.DesertLine != null && _lines.NightLine != null &&
                           _lines.SnowLine != null && _lines.CavalryLine != null && _lines.FootmenLine != null &&
                           _lines.CavalryWetLine != null && _lines.FootmenWetLine != null && _lines.SlowLine != null;
            return _lines.Whole ? _lines : null;
        }

        private static GroundFactors GroundFor(MobileParty party, SpeedLines lines)
        {
            float forest = lines.Forest, water = lines.Water, night = lines.Night, snow = lines.Snow;
            float desert = lines.Desert, slow = lines.Slow, horses = lines.Horses, spare = lines.Spare;
            TroopRoster roster = party.MemberRoster;
            Hero scout = party.EffectiveScout;
            bool kin = scout != null && scout.GetPerkValue(DefaultPerks.Scouting.ForestKin);
            if (kin)
            {
                float afoot = 0f;
                for (int i = 0; i < roster.Count; i++)
                    if (!roster.GetCharacterAtIndex(i).IsMounted) afoot += roster.GetElementNumber(i);
                kin = afoot / roster.TotalManCount >= 0.75f;
            }
            int men = roster.TotalManCount;
            int mounted = party.Party.NumberOfMenWithHorse;
            int walking = party.Party.NumberOfMenWithoutHorse;
            int mounts = party.ItemRoster.NumberOfMounts;
            foreach (MobileParty attached in party.AttachedParties)
            {
                men += attached.MemberRoster.TotalManCount;
                mounted += attached.Party.NumberOfMenWithHorse;
                walking += attached.Party.NumberOfMenWithoutHorse;
                mounts += attached.ItemRoster.NumberOfMounts;
            }
            float cavalry = men == 0 || mounted == 0 ? 0f : horses * (float)mounted / (float)men;
            int riding = Math.Min(walking, mounts);
            float footmen = men == 0 || riding == 0 ? 0f : spare * (float)riding / (float)men;
            return new GroundFactors
            {
                Forest = kin ? forest * -DefaultPerks.Scouting.ForestKin.PrimaryBonus : forest,
                Culture = PartyBaseHelper.HasFeat(party.Party, DefaultCulturalFeats.BattanianForestSpeedFeat)
                    ? DefaultCulturalFeats.BattanianForestSpeedFeat.EffectBonus * -forest : 0f,
                Water = water,
                Desert = (PartyBaseHelper.HasFeat(party.Party, DefaultCulturalFeats.AseraiDesertFeat) ? 0f : desert) +
                         PerkFactor(DefaultPerks.Scouting.DesertBorn, party, true),
                Plain = PerkFactor(DefaultPerks.Scouting.Pathfinder, party, true),
                Night = night + PerkFactor(DefaultPerks.Scouting.NightRunner, party, true),
                Day = PerkFactor(DefaultPerks.Scouting.DayTraveler, party, true),
                Snow = snow,
                Slow = slow,
                Cavalry = Factor(cavalry),
                Footmen = Factor(footmen),
                Wet = Factor(0f - cavalry * WetGroundTakesFromRiders) + Factor(0f - footmen * WetGroundTakesFromRiders)
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string ReadPace(MobileParty party, TradeMath.Pace pace)
        {
            if (Campaign.Current == null || party == null) return "no campaign is running";
            if (party.IsCurrentlyAtSea) return "the party is at sea";
            if (party.Army != null && party.Army.LeaderParty != party && party.AttachedTo != null)
                return "the party rides with an army that sets its speed";
            PartySpeedModel model = Campaign.Current.Models.PartySpeedCalculatingModel;
            if (!TheGamesOwn(model))
                return "the party speed model is " + (model == null ? "missing" : model.GetType().FullName) +
                       ", not the game's own";
            SpeedLines lines = Lines();
            if (lines == null) return "the game's party speed model has changed shape";
            GroundFactors g = GroundFor(party, lines);
            string forestLine = lines.ForestLine, fordLine = lines.FordLine, desertLine = lines.DesertLine;
            string nightLine = lines.NightLine, snowLine = lines.SnowLine, slowLine = lines.SlowLine;
            string cavalryLine = lines.CavalryLine, footmenLine = lines.FootmenLine;
            string cavalryWetLine = lines.CavalryWetLine, footmenWetLine = lines.FootmenWetLine;
            string pathfinder = lines.Pathfinder, desertBorn = lines.DesertBorn, nightRunner = lines.NightRunner;
            string dayTraveler = lines.DayTraveler, tracker = lines.Tracker, culture = lines.Culture;

            ExplainedNumber speed = model.CalculateFinalSpeed(party, model.CalculateBaseSpeed(party, true));
            float b = speed.BaseNumber;
            if (!(b > 0f) || float.IsInfinity(b)) return "the party's base speed reads " + b;
            float terrain = 0f, time = 0f, snow = 0f, wet = 0f, chase = 0f, slow = 0f, cultureRead = 0f;
            float cavalry = 0f, footmen = 0f;
            foreach (var (name, number) in speed.GetLines())
            {
                float f = number / b;
                if (name == forestLine || name == fordLine || name == desertLine || name == pathfinder ||
                    name == desertBorn) terrain += f;
                else if (name == nightLine || name == nightRunner || name == dayTraveler) time += f;
                else if (name == snowLine) snow += f;
                else if (name == cavalryWetLine || name == footmenWetLine) wet += f;
                else if (name == cavalryLine) cavalry += f;
                else if (name == footmenLine) footmen += f;
                else if (name == tracker) chase += f;
                else if (name == slowLine) slow += f;
                else if (name == culture) cultureRead += f;
            }

            MapWeatherModel weather = Campaign.Current.Models.MapWeatherModel;
            Vec2 at = party.Position.ToVec2();
            int under = GroundOf(Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace));
            float cultureNow = under == TradeMath.GroundForest ? g.Culture : 0f;
            float terrainNow = g.On(under) - cultureNow;
            bool dark = Campaign.Current.IsNight;
            float timeNow = dark ? g.Night : g.Day;
            MapWeatherModel.WeatherEvent sky = weather.GetWeatherEventInPosition(at);
            bool snowyNow = sky == MapWeatherModel.WeatherEvent.Snowy || sky == MapWeatherModel.WeatherEvent.Blizzard;
            bool wetNow = weather.GetWeatherEffectOnTerrainForPosition(at) ==
                          MapWeatherModel.WeatherEventEffectOnTerrain.Wet;
            bool slowNow = party.IsDisorganized;
            float snowNow = snowyNow ? g.Snow : 0f;
            float wetFactorNow = wetNow ? g.Wet : 0f;
            float slowFactorNow = slowNow ? g.Slow : 0f;
            if (Math.Abs(terrain - terrainNow) > LinesAgreeWithin)
                return "the game's terrain lines add " + terrain + " where the ground under the party adds " + terrainNow;
            if (Math.Abs(time - timeNow) > LinesAgreeWithin)
                return "the game's night and day lines add " + time + " where " + timeNow + " was expected";
            if (Math.Abs(snow - snowNow) > LinesAgreeWithin)
                return "the game's snow line adds " + snow + " where " + snowNow + " was expected";
            if (Math.Abs(cavalry - g.Cavalry) > LinesAgreeWithin)
                return "the game's Cavalry line adds " + cavalry + " where " + g.Cavalry + " was expected";
            if (Math.Abs(footmen - g.Footmen) > LinesAgreeWithin)
                return "the game's Footmen on horses line adds " + footmen + " where " + g.Footmen + " was expected";
            if (Math.Abs(wet - wetFactorNow) > LinesAgreeWithin)
                return "the game's weather penalty lines add " + wet + " where " + wetFactorNow + " was expected";
            if (Math.Abs(slow - slowFactorNow) > LinesAgreeWithin)
                return "the game's Disorganized line adds " + slow + " where " + slowFactorNow + " was expected";
            if (cultureRead < cultureNow - LinesAgreeWithin)
                return "the game's culture line adds " + cultureRead + " where at least " + cultureNow + " was expected";
            float chaseNow = 0f;
            if (chase != 0f)
            {
                chaseNow = PerkFactor(DefaultPerks.Scouting.Tracker, party, false);
                if (Math.Abs(chase - chaseNow) > LinesAgreeWithin)
                    return "the game's Tracker line adds " + chase + " where " + chaseNow + " was expected";
            }

            float rest = speed.SumOfFactors - g.On(under) - timeNow - snowNow - wetFactorNow - slowFactorNow - chaseNow;
            pace.Base = b;
            pace.Rest = rest;
            pace.Least = speed.LimitMinValue;
            pace.Most = speed.LimitMaxValue;
            for (int ground = 0; ground < TradeMath.Grounds; ground++)
                pace.OnGround[ground] = ground == TradeMath.GroundSea ? 0f : g.On(ground);
            pace.Day = g.Day;
            pace.Night = g.Night;
            pace.Snow = g.Snow;
            pace.Wet = g.Wet;
            pace.Slow = g.Slow;
            pace.SlowHours = slowNow ? Math.Max(0d, party.DisorganizedUntilTime.RemainingHoursFromNow) : 0d;
            pace.ReadAt = CampaignTime.Now.ToHours;
            pace.Even = false;
            float fresh = speed.ResultNumber;
            float flipped = MathF.Clamp(b + b * (speed.SumOfFactors + (snowyNow ? -g.Snow : g.Snow)),
                                        speed.LimitMinValue, speed.LimitMaxValue);
            float held = party.IsActive ? party.Speed : fresh;
            pace.SnowAtStart = Math.Abs(held - fresh) > SpeedsAgreeWithin && Math.Abs(held - flipped) <= SpeedsAgreeWithin
                ? !snowyNow
                : snowyNow;
            return null;
        }

        internal struct Said
        {
            internal bool Known;
            internal float Days;
            internal float Length;
            internal float LandRatio;
            internal bool Walked;
            internal float Forest, Water, Desert;
            internal float Night, Snow, Wet, Slow;
            internal float DayPace, NightPace;
        }

        internal static Said WhatWasSaidAbout(Settlement target)
        {
            var said = default(Said);
            if (target == null) return said;
            float days = EstimateDaysFromParty(target);
            if (!(days > 0f) || TradeMath.OutOfReach(days)) return said;
            if (!_partyDist.TryGetValue(target.StringId, out Road road)) return said;
            TradeMath.Pace pace = PartyPace();
            var tally = new TradeMath.Tally();
            Ride(road, pace, _moment, 0d, false, tally);
            said.Known = true;
            said.Days = days;
            said.Length = road.Length;
            said.LandRatio = road.LandRatio;
            said.Walked = road.Walked;
            said.Forest = (float)tally.OnGround[TradeMath.GroundForest];
            said.Water = (float)tally.OnGround[TradeMath.GroundWater];
            said.Desert = (float)tally.OnGround[TradeMath.GroundDesert];
            said.Night = (float)tally.Night;
            said.Snow = (float)tally.Snow;
            said.Wet = (float)tally.Wet;
            said.Slow = (float)tally.Slow;
            said.DayPace = pace.On(TradeMath.GroundPlain, false, false, false, false);
            said.NightPace = pace.On(TradeMath.GroundPlain, true, false, false, false);
            return said;
        }

        internal static int GroundUnder(MobileParty party)
        {
            if (party == null || Campaign.Current == null) return TradeMath.Unread;
            try { return GroundOf(Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace)); }
            catch { return TradeMath.Unread; }
        }

        internal static void SkyOver(MobileParty party, out bool wet, out bool snowy)
        {
            wet = false;
            snowy = false;
            if (party == null || Campaign.Current == null) return;
            try
            {
                byte sky = SkyNow(Campaign.Current.Models.MapWeatherModel, party.Position.ToVec2());
                wet = (sky & Wet) != 0;
                snowy = (sky & Snowy) != 0;
            }
            catch { }
        }

        internal static TradeMath.Pace PaceNow()
        {
            try { return PartyPace(); }
            catch { return null; }
        }

        internal static string PaceBecause() => _evenBecause;

        internal static string WhatCarriesYou(MobileParty party)
        {
            try { return TheLoadOnYou(party); }
            catch { return null; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string TheLoadOnYou(MobileParty party)
        {
            if (Campaign.Current == null || party == null) return null;
            TroopRoster roster = party.MemberRoster;
            int men = roster.TotalManCount;
            int mounted = party.Party.NumberOfMenWithHorse;
            int walking = party.Party.NumberOfMenWithoutHorse;
            int mounts = party.ItemRoster.NumberOfMounts;
            int caught = party.PrisonRoster.TotalManCount;
            int hurt = roster.TotalWounded;
            foreach (MobileParty attached in party.AttachedParties)
            {
                men += attached.MemberRoster.TotalManCount;
                mounted += attached.Party.NumberOfMenWithHorse;
                walking += attached.Party.NumberOfMenWithoutHorse;
                mounts += attached.ItemRoster.NumberOfMounts;
                caught += attached.PrisonRoster.TotalManCount;
                hurt += attached.MemberRoster.TotalWounded;
            }
            string head = men + " men, " + mounted + " mounted, " + walking + " afoot, " + mounts +
                          " spare mount(s), " + caught + " prisoner(s), " + hurt + " wounded, morale " +
                          party.Morale.ToString("0", CultureInfo.InvariantCulture);
            PartySpeedModel model = Campaign.Current.Models.PartySpeedCalculatingModel;
            if (model == null) return head;
            ExplainedNumber speed = model.CalculateFinalSpeed(party, model.CalculateBaseSpeed(party, true));
            float b = speed.BaseNumber;
            if (!(b > 0f) || float.IsInfinity(b)) return head;
            SpeedLines lines = Lines();
            var kept = new List<string>();
            foreach (var (name, number) in speed.GetLines())
            {
                if (TheGroundSaysThis(lines, name)) continue;
                kept.Add(name + " " + (number / b).ToString("+0.000;-0.000;0.000", CultureInfo.InvariantCulture));
            }
            return head + "; base speed " + b.ToString("0.00", CultureInfo.InvariantCulture) +
                   (kept.Count == 0 ? ", with no line of its own" : ", " + string.Join(", ", kept));
        }

        private static bool TheGroundSaysThis(SpeedLines lines, string name)
        {
            if (lines == null || name == null) return false;
            return name == lines.ForestLine || name == lines.FordLine || name == lines.DesertLine ||
                   name == lines.NightLine || name == lines.SnowLine || name == lines.SlowLine ||
                   name == lines.CavalryWetLine || name == lines.FootmenWetLine || name == lines.Culture ||
                   name == lines.Pathfinder || name == lines.DesertBorn || name == lines.NightRunner ||
                   name == lines.DayTraveler || name == lines.Tracker;
        }

        private static TradeMath.Pace PartyPace()
        {
            int hour = (int)CampaignTime.Now.ToHours;
            if (hour == _speedHour && _pace != null) return _pace;
            TradeMath.SpeedsInEffect(LandSpeed(), SeaSpeed(), out float land, out float sea);
            TradeMath.Pace pace = TradeMath.Pace.AllAt(land, sea);
            string why;
            try { why = ReadPace(MobileParty.MainParty, pace); }
            catch (Exception e) { why = "the game's party speed could not be read (" + e.GetType().Name + ")"; }
            if (why != null) pace = TradeMath.Pace.AllAt(land, sea);
            pace.FairWinds = NavalPerkAtSea(FairWindsPerk, false);
            pace.ShockAndAwe = NavalPerkAtSea(ShockAndAwePerk, true);
            string seas;
            try { seas = ReadSeas(MobileParty.MainParty, pace); }
            catch (Exception e) { seas = "War Sails' ship speed could not be read (" + e.GetType().Name + ")"; }
            _seasBecause = seas;
            _evenBecause = why;
            _speedHour = hour;
            _pace = pace;
            return pace;
        }

        private static void Speeds(bool sailed, out float land, out float sea)
        {
            TradeMath.Pace pace = PartyPace();
            land = pace.BestOnLand;
            sea = sailed && WindsBlow() ? pace.BestAtSea : pace.Afloat;
        }

        private static int CellOf(Vec2 at)
        {
            if (_dim <= 0) return TradeMath.Unread;
            int x = (int)(at.x / _cellWide);
            int y = (int)(at.y / _cellHigh);
            return x < 0 || y < 0 || x >= _dim || y >= _dim ? TradeMath.Unread : y * _dim + x;
        }

        private static Vec2 PlaceIn(int cell, WeatherNode[] nodes)
        {
            if (_dim <= 0) return Vec2.Zero;
            if (_writers != null && cell >= 0 && cell < _writers.Length && _writers[cell].Length > 0 && nodes != null)
                return nodes[_writers[cell][0]].Position.ToVec2();
            return new Vec2((cell % _dim + 0.5f) * _cellWide, (cell / _dim + 0.5f) * _cellHigh);
        }

        private static void Grid()
        {
            Campaign campaign = Campaign.Current;
            int dim = campaign?.DefaultWeatherNodeDimension ?? 0;
            if (dim == _dim && _dim > 0) return;
            _dim = 0;
            _writers = null;
            _slotOf = null;
            if (dim <= 0 || campaign.MapSceneWrapper == null) return;
            Vec2 size = campaign.MapSceneWrapper.GetTerrainSize();
            float wide = size.X / (float)dim;
            float high = size.Y / (float)dim;
            if (!(wide > 0f) || !(high > 0f) || float.IsInfinity(wide) || float.IsInfinity(high)) return;
            _cellWide = wide;
            _cellHigh = high;
            _dim = dim;
        }

        private static bool Writers(WeatherNode[] nodes, int[] shuffled)
        {
            if (_dim <= 0 || nodes == null || shuffled == null || nodes.Length != _dim * _dim ||
                shuffled.Length != nodes.Length)
                return false;
            if (_writers != null && _slotOf != null && _slotOf.Length == nodes.Length) return true;
            var lists = new List<int>[nodes.Length];
            for (int n = 0; n < nodes.Length; n++)
            {
                if (nodes[n] == null) continue;
                int cell = CellOf(nodes[n].Position.ToVec2());
                if (cell < 0) continue;
                (lists[cell] ?? (lists[cell] = new List<int>())).Add(n);
            }
            var slots = new int[nodes.Length];
            for (int n = 0; n < slots.Length; n++) slots[n] = -1;
            for (int p = 0; p < shuffled.Length; p++)
            {
                int n = shuffled[p];
                if (n < 0 || n >= slots.Length || slots[n] >= 0) return false;
                slots[n] = p;
            }
            var writers = new int[nodes.Length][];
            for (int c = 0; c < writers.Length; c++) writers[c] = lists[c]?.ToArray() ?? new int[0];
            _writers = writers;
            _slotOf = slots;
            return true;
        }

        private static CampaignTime Times(CampaignTime unit, long count)
        {
            CampaignTime sum = CampaignTime.Zero, power = unit;
            while (count > 0)
            {
                if ((count & 1) != 0) sum += power;
                power += power;
                count >>= 1;
            }
            return sum;
        }

        private static byte SkyByte(bool wet, bool snowy) => (byte)((wet ? Wet : 0) | (snowy ? Snowy : 0));

        private static byte SkyNow(MapWeatherModel game, Vec2 at)
        {
            MapWeatherModel.WeatherEvent sky = game.GetWeatherEventInPosition(at);
            return SkyByte(game.GetWeatherEffectOnTerrainForPosition(at) == MapWeatherModel.WeatherEventEffectOnTerrain.Wet,
                           sky == MapWeatherModel.WeatherEvent.Snowy || sky == MapWeatherModel.WeatherEvent.Blizzard);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string BuildTheSkies()
        {
            _skies = null;
            Campaign campaign = Campaign.Current;
            if (campaign == null) return "no campaign is running";
            MapWeatherModel model = campaign.Models.MapWeatherModel;
            MapWeatherModel game = TheGamesOwnWeather(model);
            if (game == null)
                return "the weather model is " + (model == null ? "missing" : model.GetType().FullName) +
                       ", not the game's own";
            Grid();
            if (_dim <= 0) return "the map's weather grid could not be read";
            MapWeatherCampaignBehavior turns = campaign.GetCampaignBehavior<MapWeatherCampaignBehavior>();
            if (turns == null) return "the game's weather turns are not running";
            Type kind = typeof(MapWeatherCampaignBehavior);
            int[] shuffled = kind.GetField("_weatherNodeDataShuffledIndices", Inside)?.GetValue(turns) as int[];
            object last = kind.GetField("_lastUpdatedNodeIndex", Inside)?.GetValue(turns);
            MBCampaignEvent tick = kind.GetField("_weatherTickEvent", Inside)?.GetValue(turns) as MBCampaignEvent;
            object next = tick == null ? null : typeof(MBCampaignEvent).GetField("NextTriggerTime", Inside)?.GetValue(tick);
            WeatherNode[] nodes = turns.AllWeatherNodes;
            if (shuffled == null || !(last is int lastTurn) || tick == null || !(next is CampaignTime nextTurn) ||
                nodes == null)
                return "the game's weather turns could not be read";
            if (!Writers(nodes, shuffled)) return "the game's weather turns do not match the map's weather grid";
            if (lastTurn < 0 || lastTurn >= nodes.Length) return "the game's next weather turn reads " + lastTurn;
            CampaignTime every = tick.TriggerPeriod;
            if (every != model.WeatherUpdateFrequency || !(every.ToHours > 0d))
                return "the game's weather turns come every " + every.ToHours + " hour(s), not as the weather model has it";
            var ahead = new DefaultMapWeatherModel();
            ahead.InitializeCaches();
            var skyNow = new byte[nodes.Length];
            for (int cell = 0; cell < skyNow.Length; cell++) skyNow[cell] = SkyNow(game, PlaceIn(cell, nodes));
            CampaignTime now = CampaignTime.Now;
            if (!_skySeen)
            {
                _skySeen = true;
                _firstSky = now;
            }
            _skies = new Skies
            {
                Taken = now,
                Next = nextTurn,
                Every = every,
                Round = Times(every, nodes.Length),
                Count = nodes.Length,
                Last = lastTurn,
                Nodes = nodes,
                Game = game,
                Ahead = ahead,
                Now = skyNow
            };
            return null;
        }

        private static Skies TheSkies()
        {
            int hour = (int)CampaignTime.Now.ToHours;
            if (hour == _skyHour) return _skies;
            _skyHour = hour;
            string why;
            try { why = BuildTheSkies(); }
            catch (Exception e)
            {
                _skies = null;
                why = "the game's weather could not be read (" + e.GetType().Name + ")";
            }
            _skyBecause = why;
            return _skies;
        }

        private static TradeMath.ISky SkyAt(CampaignTime moment, double after)
        {
            Skies skies = TheSkies();
            if (skies != null) return new SkyFrom((moment - skies.Taken).ToHours + after);
            return _dim > 0 ? (TradeMath.ISky)SkyAsItIs.Held : TradeMath.ClearSky.Always;
        }

        private static byte WorkOut(Skies skies, int node, CampaignTime at)
        {
            var key = (node, at.ToHours);
            if (_worked.TryGetValue(key, out byte known)) return known;
            long started = Stopwatch.GetTimestamp();
            WeatherNode place = skies.Nodes[node];
            MapWeatherModel.WeatherEvent sky = skies.Ahead.UpdateWeatherForPosition(place.Position, at);
            bool wet = skies.Ahead.GetWeatherEffectOnTerrainForPosition(place.Position.ToVec2()) ==
                       MapWeatherModel.WeatherEventEffectOnTerrain.Wet;
            long spent = Stopwatch.GetTimestamp() - started;
            _forecastTicks += spent;
            _frameTicks += spent;
            _forecasts++;
            byte worked = SkyByte(wet, sky == MapWeatherModel.WeatherEvent.Snowy || sky == MapWeatherModel.WeatherEvent.Blizzard);
            if (_worked.Count >= MostWeatherTurnsKept) _worked.Clear();
            _worked[key] = worked;
            return worked;
        }

        private static Outlook OutlookOf(int cell)
        {
            Skies skies = _skies;
            if (skies == null || _writers == null || cell < 0 || cell >= _writers.Length) return null;
            if (skies.Outlooks.TryGetValue(cell, out Outlook outlook)) return outlook;
            outlook = new Outlook();
            int[] writers = _writers[cell];
            byte now = skies.Now[cell];
            outlook.At.Add(double.NegativeInfinity);
            outlook.Sky.Add(now);
            outlook.Writers = writers;
            outlook.Due = new CampaignTime[writers.Length];
            for (int i = 0; i < writers.Length; i++)
            {
                int turns = TradeMath.TurnsUntil(_slotOf[writers[i]], skies.Last, skies.Count);
                outlook.Due[i] = skies.Next + Times(skies.Every, turns);
            }
            if (writers.Length == 1)
            {
                CampaignTime before = outlook.Due[0] - skies.Round;
                if (before >= _firstSky && before <= skies.Taken)
                {
                    skies.Checked++;
                    if (WorkOut(skies, writers[0], before) == now) skies.Agreed++;
                }
            }
            outlook.Done = writers.Length == 0;
            skies.Outlooks[cell] = outlook;
            return outlook;
        }

        private static void Extend(Skies skies, Outlook outlook)
        {
            int soonest = 0;
            for (int i = 1; i < outlook.Due.Length; i++)
                if (outlook.Due[i] < outlook.Due[soonest]) soonest = i;
            CampaignTime due = outlook.Due[soonest];
            double hours = (due - skies.Taken).ToHours;
            if (hours > LongestForecastHours)
            {
                outlook.Done = true;
                return;
            }
            byte sky = WorkOut(skies, outlook.Writers[soonest], due);
            outlook.At.Add(hours);
            outlook.Sky.Add(sky);
            outlook.Due[soonest] = due + skies.Round;
        }

        private static void ReadTheSky(int cell, double from, double hours, out bool wet, out bool snowy,
                                       out double changes)
        {
            wet = false;
            snowy = false;
            changes = double.PositiveInfinity;
            Outlook outlook = OutlookOf(cell);
            if (outlook == null) return;
            while (!outlook.Done && outlook.At[outlook.At.Count - 1] - from <= hours) Extend(_skies, outlook);
            int k = TradeMath.LastAtOrBefore(outlook.At, from, hours);
            if (k < 0) k = 0;
            wet = (outlook.Sky[k] & Wet) != 0;
            snowy = (outlook.Sky[k] & Snowy) != 0;
            if (k + 1 < outlook.At.Count) changes = outlook.At[k + 1] - from;
        }

        private static float Ride(Road road, TradeMath.Pace pace, CampaignTime moment, double after, bool fresh,
                                  TradeMath.Tally tally)
        {
            StartTheMeter();
            if (pace.Even && road.Sails == null)
                return TradeMath.DaysAtSpeed(road.Length, road.LandRatio, pace.Base, pace.Afloat);
            TradeMath.ISky sky = SkyAt(moment, after);
            long started = Stopwatch.GetTimestamp();
            long forecastBefore = _forecastTicks;
            double start = moment.ToHours + after;
            float days = road.Ashore != null
                ? TradeMath.DaysAfloatThenAshore(road.Afloat, road.Sails, road.GoingAshore, road.Ashore,
                                                 pace.After(start - pace.ReadAt), sky, start, CampaignTime.SunRise,
                                                 CampaignTime.SunSet, fresh, tally)
                : TradeMath.DaysAlong(road.Runs, pace.After(start - pace.ReadAt), sky, start, CampaignTime.SunRise,
                                      CampaignTime.SunSet, fresh, tally, road.Sails);
            long spent = Stopwatch.GetTimestamp() - started;
            long inner = _forecastTicks - forecastBefore;
            _frameTicks += spent > inner ? spent - inner : 0L;
            if (tally == null)
            {
                _rideTicks += spent;
                _rides++;
            }
            return days;
        }

        private static float RideFromParty(Road road)
        {
            int hour = (int)CampaignTime.Now.ToHours;
            if (road.Hour == hour) return road.Days;
            road.Days = Ride(road, PartyPace(), _moment, 0d, false, null);
            road.Hour = hour;
            return road.Days;
        }

        private static float RideBetween(Road road, float after, bool fresh)
        {
            double later = TradeMath.Finite(after, 0f) > 0f ? after * 24d : 0d;
            return Ride(road, PartyPace(), _moment, later, fresh, null);
        }

        private static void StartTheMeter()
        {
            int hour = (int)CampaignTime.Now.ToHours;
            if (hour == _meterHour) return;
            _meterHour = hour;
            _partyWalks = 0;
            _pairWalks = 0;
            _otherWalks = 0;
            _paths = 0;
            _probes = 0;
            _forecasts = 0;
            _rides = 0;
            _othersOnTheirPath = 0;
            _othersOnAFreshPath = 0;
            _othersGuessed = 0;
            _walkedBehind = 0;
            _seaWalks = 0;
            _walkTicks = 0L;
            _pathTicks = 0L;
            _forecastTicks = 0L;
            _rideTicks = 0L;
            _slowestWalk = 0L;
            _slowestWalkSaid = null;
        }

        private static float HowLongThatRoadWas(Road road)
        {
            if (road == null) return 0f;
            if (road.Length > 0f) return road.Length;
            return road.Runs == null ? 0f : TradeMath.LengthOn(road.Runs, -1);
        }

        private static long StartAWalk()
        {
            _probesThisWalk = _probes;
            _pathsThisWalk = _paths;
            return Stopwatch.GetTimestamp();
        }

        private static void StopTheMeter(long started, ref int walks, string kind = null, Road road = null)
        {
            long took = Stopwatch.GetTimestamp() - started;
            walks++;
            _walkTicks += took;
            _frameTicks += took;
            if (_scanRoads) _scanRoadTicks += took;
            if (took <= _slowestWalk) return;
            _slowestWalk = took;
            _slowestWalkSaid = kind == null
                ? null
                : kind + " of " + HowLongThatRoadWas(road).ToString("0.0", CultureInfo.InvariantCulture) +
                  " over " + (road?.Runs == null ? 0 : road.Runs.Count) + " stretch(es), " +
                  (_probes - _probesThisWalk) + " ground probe(s) and " + (_paths - _pathsThisWalk) +
                  " pathfinding call(s)" + (road != null && !road.Walked ? ", and it would not walk" : "");
        }

        private static long RegionAt(IMapScene map, Vec2 at)
        {
            _probes++;
            PathFaceRecord face = map.GetFaceIndex(new CampaignVec2(at, true));
            if (!face.IsValid()) return TradeMath.RegionOf(TradeMath.Unread, CellOf(at));
            if (!_groundOfFace.ContainsKey(face.FaceIndex))
                _groundOfFace[face.FaceIndex] = GroundOf(map.GetFaceTerrainType(face));
            return TradeMath.RegionOf(face.FaceIndex, CellOf(at));
        }

        private static float Probe(IMapScene map, Vec2 from, Vec2 to, List<TradeMath.Run> runs)
        {
            float length = from.Distance(to);
            if (!(length > 0f) || float.IsInfinity(length)) return 0f;
            Vec2 step = (to - from) * (1f / length);
            _pieces.Clear();
            TradeMath.RegionsAlong(length, Stride, along => RegionAt(map, from + step * along), _pieces);
            for (int i = 0; i < _pieces.Count; i++)
            {
                int face = TradeMath.FaceOfRegion(_pieces[i].Region);
                int ground = face >= 0 && _groundOfFace.TryGetValue(face, out int known) ? known : TradeMath.GroundElse;
                TradeMath.AddRun(runs, ground, face, TradeMath.CellOfRegion(_pieces[i].Region), _pieces[i].Length);
            }
            return length;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool WalkTheRoad(MobileParty party, PathFaceRecord fromFace, Vec2 from, CampaignVec2 to,
                                        List<TradeMath.Run> runs, ref float length)
        {
            IMapScene map = Campaign.Current.MapSceneWrapper;
            int[] barred = Campaign.Current.Models.PartyNavigationModel
                .GetInvalidTerrainTypesForNavigationType(MobileParty.NavigationType.Default);
            int seaToLand = party.GetRegionSwitchCostFromSeaToLand();
            int landToSea = party.GetRegionSwitchCostFromLandToSea();
            PathFaceRecord toFace = to.Face;
            Vec2 end = to.ToVec2();
            if (!fromFace.IsValid() || !toFace.IsValid()) return false;
            for (int leg = 0; leg < MostPathsToOneRoad; leg++)
            {
                if (fromFace.FaceIndex == toFace.FaceIndex)
                {
                    length += Probe(map, from, end, runs);
                    return true;
                }
                _path.Size = 0;
                long asked = Stopwatch.GetTimestamp();
                bool found = map.GetPathBetweenAIFaces(fromFace, toFace, from, end, 0.3f, _path, barred, 2f, seaToLand,
                                                       landToSea);
                _pathTicks += Stopwatch.GetTimestamp() - asked;
                _paths++;
                if (!found || _path.Size <= 0) return false;
                Vec2 at = from;
                for (int i = 0; i < _path.Size; i++)
                {
                    length += Probe(map, at, _path[i], runs);
                    at = _path[i];
                }
                if (at.DistanceSquared(end) <= ReachedSquared) return true;
                if (at.DistanceSquared(from) <= ReachedSquared) return false;
                fromFace = map.GetFaceIndex(new CampaignVec2(at, true));
                if (!fromFace.IsValid()) return false;
                from = at;
            }
            return false;
        }

        private static List<TradeMath.Run> Unwalked(float length, float landRatio, bool seaFirst)
        {
            var runs = new List<TradeMath.Run>(2);
            float land = landRatio < 0f ? 0f : landRatio > 1f ? 1f : landRatio;
            if (seaFirst) TradeMath.AddRun(runs, TradeMath.GroundSea, TradeMath.Unread, TradeMath.Unread, length * (1f - land));
            TradeMath.AddRun(runs, TradeMath.GroundElse, TradeMath.Unread, TradeMath.Unread, length * land);
            if (!seaFirst) TradeMath.AddRun(runs, TradeMath.GroundSea, TradeMath.Unread, TradeMath.Unread, length * (1f - land));
            if (float.IsNaN(length) || float.IsNaN(land) || float.IsInfinity(length))
                runs.Add(new TradeMath.Run
                {
                    Ground = TradeMath.GroundElse, Face = TradeMath.Unread, Cell = TradeMath.Unread, Length = float.NaN
                });
            return runs;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WalkFromParty(MobileParty party, Settlement target, Road road)
        {
            Grid();
            var runs = new List<TradeMath.Run>();
            if (!WalkItsRoad(party, target, runs, true, out float length, out _)) return;
            road.Runs = runs;
            road.Length = length;
            road.LandRatio = 1f;
            road.Walked = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void WalkBetween(Settlement a, Settlement b, Road road)
        {
            Grid();
            var runs = new List<TradeMath.Run>();
            float length = 0f;
            CampaignVec2 gate = a.GatePosition;
            if (!WalkTheRoad(MobileParty.MainParty, gate.Face, gate.ToVec2(), b.GatePosition, runs, ref length)) return;
            road.Runs = runs;
            road.Length = length;
            road.LandRatio = 1f;
            road.Walked = true;
        }

        private static Road RoadFromParty(MobileParty party, Settlement target)
        {
            float dist = FromParty(target, out float landRatio);
            var road = new Road { Length = dist, LandRatio = landRatio };
            bool atSea = party.IsCurrentlyAtSea;
            if (AllAtSea(landRatio) && target.HasPort && WindsBlow())
            {
                Road sailed = SailedFromParty(party, target, atSea);
                if (sailed != null) return sailed;
            }
            bool mixed = PartlyAtSea(landRatio);
            if (atSea && mixed && !target.HasPort && WindsBlow() && !_unwalkable.Contains(target.StringId))
            {
                Road landed = LandedFromParty(party, target);
                if (landed != null) return landed;
            }
            if (!atSea && (landRatio >= AllOnLand || mixed) && !_unwalkable.Contains(target.StringId))
            {
                StartTheMeter();
                long started = StartAWalk();
                try { WalkFromParty(party, target, road); }
                catch { road.Walked = false; }
                StopTheMeter(started, ref _partyWalks, "a road from your party", road);
                if (road.Walked && landRatio >= AllOnLand) NoteHowRoughTheGuessWas(dist, landRatio, road);
                if (!road.Walked) _unwalkable.Add(target.StringId);
            }
            if (!road.Walked)
            {
                float longer = !atSea && landRatio >= AllOnLand ? TheRoughRoadTakesLongerBy() : 1f;
                road.Length = dist * longer;
                road.LandRatio = landRatio;
                road.Runs = Unwalked(road.Length, landRatio, atSea);
            }
            return road;
        }

        private static Road RoadBetween(Settlement a, Settlement b)
        {
            float dist = Between(a, b, out float landRatio);
            var road = new Road { Length = dist, LandRatio = landRatio };
            bool mixed = PartlyAtSea(landRatio);
            if (a != b && a.HasPort && mixed && !b.HasPort && WindsBlow())
            {
                StartTheMeter();
                long started = StartAWalk();
                try { SailToTheShore(a, b, road); }
                catch { road.Walked = false; }
                StopTheMeter(started, ref _pairWalks, "a course from a port to the shore", road);
                _justWalked = true;
                if (_behind) _walkedBehind++;
                if (road.Walked)
                {
                    _seaWalks++;
                    return road;
                }
                road = new Road { Length = dist, LandRatio = landRatio };
            }
            if (a != b && a.HasPort && b.HasPort && (AllAtSea(landRatio) || mixed) && WindsBlow())
            {
                StartTheMeter();
                long started = StartAWalk();
                try { SailBetween(a, b, road); }
                catch { road.Walked = false; }
                StopTheMeter(started, ref _pairWalks, "a course between ports", road);
                _justWalked = true;
                if (_behind) _walkedBehind++;
                if (road.Walked)
                {
                    _seaWalks++;
                    return road;
                }
                road = new Road { Length = dist, LandRatio = landRatio };
            }
            if ((landRatio >= AllOnLand || mixed) && a != b)
            {
                StartTheMeter();
                long started = StartAWalk();
                try { WalkBetween(a, b, road); }
                catch { road.Walked = false; }
                StopTheMeter(started, ref _pairWalks, "a road between markets", road);
                if (road.Walked && landRatio >= AllOnLand) NoteHowRoughTheGuessWas(dist, landRatio, road);
                _justWalked = true;
                if (_behind) _walkedBehind++;
            }
            if (!road.Walked)
            {
                float longer = landRatio >= AllOnLand ? TheRoughRoadTakesLongerBy() : 1f;
                road.Length = dist * longer;
                road.LandRatio = landRatio;
                road.Runs = Unwalked(road.Length, landRatio, false);
            }
            return road;
        }

        private static bool AllAtSea(float landRatio) => landRatio >= 0f && landRatio <= 1f - AllOnLand;

        private static bool PartlyAtSea(float landRatio) => landRatio > 1f - AllOnLand && landRatio < AllOnLand;

        private static int _windsHour = -1;
        private static bool _windsBlow;
        private static MapScene _windsOf;
        private static string _seasSaid, _seasBecause;
        private static readonly Dictionary<int, int> _waterOfFace = new Dictionary<int, int>();
        private static readonly Dictionary<int, TerrainType> _terrainOfFace = new Dictionary<int, TerrainType>();

        private static bool WindsBlow()
        {
            int hour = (int)CampaignTime.Now.ToHours;
            if (hour == _windsHour) return _windsBlow;
            _windsHour = hour;
            string why;
            try { why = WhyNoWinds(); }
            catch (Exception e) { why = "the War Sails wind could not be read (" + e.GetType().Name + ")"; }
            _windsBlow = why == null;
            Log.Repeatable("sea legs", why ?? "winds",
                           why == null
                               ? "sea legs: a ride all or partly at sea is timed along the course your ships would sail, with the " +
                                 "War Sails wind the map lays out on every stretch of it and storms left out"
                               : "sea legs: timed at your ships' speed alone, as " + why);
            return _windsBlow;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string WhyNoWinds()
        {
            _windsOf = null;
            Campaign campaign = Campaign.Current;
            if (campaign == null || MobileParty.MainParty == null) return "no campaign is running";
            if (!NavalActive) return "your party has no ships";
            Type speed = campaign.Models.PartySpeedCalculatingModel?.GetType();
            if (speed?.FullName != NavalSpeedModel)
                return "the party speed model is " + (speed?.FullName ?? "missing") + ", not War Sails' own";
            Type weather = campaign.Models.MapWeatherModel?.GetType();
            if (weather?.FullName != NavalWeatherModel)
                return "the weather model is " + (weather?.FullName ?? "missing") + ", not War Sails' own";
            string moved = WarSailsMoved(weather, "MinWindWithoutStormOnCampaignMap", TradeMath.WindAtLeast) ??
                           WarSailsMoved(weather, "MaxWindWithoutStormOnCampaignMap", TradeMath.WindAtMost) ??
                           WarSailsMoved(speed, "WindDeadZoneThresholdInDegrees", 180f - TradeMath.AWindPushesWithin) ??
                           WarSailsMoved(speed, "MaximumNavalSpeed", TradeMath.ASailIsNeverFasterThan);
            if (moved != null) return moved;
            if (!(campaign.MapSceneWrapper is MapScene map)) return "the map is not the game's own";
            _windsOf = map;
            return null;
        }

        private static string WarSailsMoved(Type model, string name, float counted)
        {
            float game;
            try
            {
                object raw = model.GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.GetRawConstantValue();
                game = raw is IConvertible held ? held.ToSingle(CultureInfo.InvariantCulture) : float.NaN;
            }
            catch { game = float.NaN; }
            if (Math.Abs(game - counted) <= 1E-05f) return null;
            return "War Sails' own " + name + " is " +
                   (float.IsNaN(game) ? "not there" : game.ToString("0.#####", CultureInfo.InvariantCulture)) +
                   " where TradeLord counts " + counted.ToString("0.#####", CultureInfo.InvariantCulture);
        }

        private static PerkObject NavalPerk(string id) => Game.Current?.ObjectManager?.GetObject<PerkObject>(id);

        private static bool HasNavalPerk(MobileParty party, PerkObject perk, bool secondary)
        {
            if (perk == null) return false;
            var held = new ExplainedNumber();
            return PerkHelper.AddPerkBonusForParty(perk, party, !secondary, ref held, BattleEnvironment.Naval);
        }

        private static void AddNavalPerk(string id, MobileParty party, bool primary, ref ExplainedNumber stat)
        {
            PerkObject perk = NavalPerk(id);
            if (perk != null) PerkHelper.AddPerkBonusForParty(perk, party, primary, ref stat, BattleEnvironment.Naval);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static float NavalPerkAtSea(string id, bool secondary)
        {
            try
            {
                MobileParty party = MobileParty.MainParty;
                PerkObject perk = NavalPerk(id);
                if (party == null || !HasNavalPerk(party, perk, secondary)) return 0f;
                return TradeMath.Finite(secondary ? perk.SecondaryBonus : perk.PrimaryBonus, 0f);
            }
            catch { return 0f; }
        }

        private static void CrewOf(MobileParty party, PerkObject chain, InventoryCapacityModel hold, ref int ships,
                                   ref int needed, ref int most, ref int men, ref int crew, ref float weight)
        {
            foreach (Ship ship in party.Ships)
            {
                ships++;
                needed += ship.SkeletalCrewCapacity;
                most += ship.TotalCrewCapacity;
            }
            int aboard = party.MemberRoster.TotalManCount;
            men += aboard;
            crew += aboard + (HasNavalPerk(party, chain, true) ? party.PrisonRoster.TotalManCount : 0);
            weight += hold.CalculateTotalWeightCarried(party, true).ResultNumber;
        }

        private static bool WithGunnar()
        {
            try
            {
                Type story = Type.GetType(NavalStoryline + ", NavalDLC");
                MethodInfo active = story?.GetMethod("IsNavalStoryLineActive",
                                                     BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                return active?.Invoke(null, null) is bool on && on;
            }
            catch { return false; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string ReadSeas(MobileParty party, TradeMath.Pace pace)
        {
            _seasSaid = null;
            Campaign campaign = Campaign.Current;
            if (campaign == null || party == null) return "no campaign is running";
            if (!NavalActive) return "your party has no ships";
            PartySpeedModel model = campaign.Models.PartySpeedCalculatingModel;
            Type speed = model?.GetType();
            if (speed?.FullName != NavalSpeedModel)
                return "the party speed model is " + (speed?.FullName ?? "missing") + ", not War Sails' own";
            string moved = WarSailsMoved(speed, "OpenSeaBonus", TradeMath.TheOpenSeaAdds) ??
                           WarSailsMoved(speed, "RiverBonus", TradeMath.ARiverAdds) ??
                           WarSailsMoved(speed, "PartyFleetSizeThreshold", TradeMath.AFleetSlowsPast) ??
                           WarSailsMoved(speed, "DisorganizedEffect", TradeMath.DisorganizedAtSea) ??
                           WarSailsMoved(speed, "MaximumNavalSpeed", TradeMath.ASailIsNeverFasterThan);
            if (moved != null) return moved;
            float snow = Effect(typeof(DefaultPartySpeedCalculatingModel).GetField("MovingOnSnowEffect", Hidden));
            if (float.IsNaN(snow)) return "the game's snow step could not be read";
            InventoryCapacityModel hold = campaign.Models.InventoryCapacityModel;
            PerkObject chain = NavalPerk(ChainToOarsPerk);
            int ships = 0, needed = 0, most = 0, men = 0, crew = 0, own = 0;
            float weight = 0f, shallow = 0f;
            CrewOf(party, chain, hold, ref ships, ref needed, ref most, ref men, ref crew, ref weight);
            foreach (MobileParty attached in party.AttachedParties)
                CrewOf(attached, chain, hold, ref ships, ref needed, ref most, ref men, ref crew, ref weight);
            foreach (Ship ship in party.Ships)
            {
                own++;
                if (ship.ShipHull?.CanNavigateShallowWater == true) shallow += ship.GetCampaignSpeed();
            }
            if (ships == 0) return "your party has no ships";

            var skeleton = new ExplainedNumber(needed);
            AddNavalPerk(FleetCommanderPerk, party, false, ref skeleton);
            PerkObject shore = NavalPerk(ShoreMasterPerk), commander = NavalPerk(FleetCommanderPerk);
            float eased = (HasNavalPerk(party, shore, true) ? 1f + shore.SecondaryBonus : 1f) *
                          (HasNavalPerk(party, commander, false) ? 1f + commander.PrimaryBonus : 1f);
            var wisdom = new ExplainedNumber(1f);
            AddNavalPerk(VeteransWisdomPerk, party, false, ref wisdom);
            int capacity = (int)hold.CalculateInventoryCapacity(party, true).ResultNumber;
            float under = TradeMath.Undermanned(crew, skeleton.RoundedResultNumber);
            float over = TradeMath.Overmanned(men, most);
            float fleet = TradeMath.OverFleetSize(ships, eased);
            float burden = TradeMath.Overburdened(weight, capacity, wisdom.ResultNumber);

            var gains = new ExplainedNumber(1f);
            SkillEffect sailing = Game.Current?.ObjectManager?.GetObject<SkillEffect>(SailingSpeedSkill);
            if (sailing != null) SkillHelper.AddSkillBonusForParty(sailing, party, ref gains);
            AddNavalPerk(OldSaltsTouchPerk, party, true, ref gains);
            AddNavalPerk(FavorableTidePerk, party, true, ref gains);
            float faster = campaign.Options.IsIncreasedGlobalMovementSpeedEnabled ? TheFasterMapAdds : 0f;
            PerkObject raider = NavalPerk(RiverRaiderPerk);
            var coast = new ExplainedNumber(1f);
            FeatObject nord = Game.Current?.ObjectManager?.GetObject<FeatObject>(NordShipsFeat);
            if (nord != null) FeatHelper.ApplyCultureFeat(party.Party, nord, ref coast);
            float alongside = coast.SumOfFactors +
                              (HasNavalPerk(party, raider, false) ? -TradeMath.TheOpenSeaAdds * raider.PrimaryBonus : 0f);
            float gunnar = party.IsMainParty && WithGunnar() ? 1f : 0f;
            float inshore = TradeMath.ShallowDraft(shallow, own, true) + (coast.BaseNumber - 1f);
            float offshore = TradeMath.ShallowDraft(shallow, own, false);

            pace.SeaAdd = burden + (gains.BaseNumber - 1f) + gunnar;
            pace.SeaRest = under + over + fleet + faster + gains.SumOfFactors;
            pace.OnWater[TradeMath.WaterOpen] = TradeMath.TheOpenSeaAdds;
            pace.OnWater[TradeMath.WaterRiver] = TradeMath.ARiverAdds + alongside;
            pace.OnWater[TradeMath.WaterCoast] = alongside;
            pace.AddOnWater[TradeMath.WaterOpen] = offshore;
            pace.AddOnWater[TradeMath.WaterElse] = offshore;
            pace.AddOnWater[TradeMath.WaterCoast] = inshore;
            pace.AddOnWater[TradeMath.WaterRiver] = inshore;
            pace.SeaSnow = snow;
            pace.SeaSlow = TradeMath.DisorganizedAtSea;
            pace.SeaLeast = model.MinimumSpeed;
            if (pace.Even && party.IsDisorganized)
            {
                pace.SlowHours = Math.Max(0d, party.DisorganizedUntilTime.RemainingHoursFromNow);
                pace.ReadAt = CampaignTime.Now.ToHours;
            }
            _seasSaid = "sea step read: your ships' base speed " + Figure(pace.Sea) + " with " + Signed(burden + gunnar) +
                        " added to it, Undermanned " + Signed(under) + ", Overmanned " + Signed(over) +
                        ", Over fleet size " + Signed(fleet) + ", skill and perks " + Signed(gains.SumOfFactors) +
                        (faster > 0f ? ", faster map " + Signed(faster) : "") + ", Open Sea " +
                        Signed(pace.OnWater[TradeMath.WaterOpen]) + ", River " + Signed(pace.OnWater[TradeMath.WaterRiver]) +
                        ", coastal waters " + Signed(alongside) + ", Shallow Draft " + Signed(inshore) +
                        " near the shore and " + Signed(offshore) + " away from it, snow " + Signed(snow) +
                        ", Disorganized " + Signed(pace.SeaSlow) + ", Fair Winds " + Signed(pace.FairWinds) +
                        " and Shock and Awe " + Signed(pace.ShockAndAwe) + " with the wind";
            return null;
        }

        private static string Figure(float value) =>
            float.IsNaN(value) ? "unknown" : value.ToString("0.00", CultureInfo.InvariantCulture);

        private static string Signed(float value) =>
            float.IsNaN(value) ? "unknown"
                               : (value >= 0f ? "+" : "") + value.ToString("0.000", CultureInfo.InvariantCulture);

        private static int WaterOf(TerrainType terrain)
        {
            switch (terrain)
            {
                case TerrainType.OpenSea: return TradeMath.WaterOpen;
                case TerrainType.CoastalSea:
                case TerrainType.UnderBridge: return TradeMath.WaterCoast;
                case TerrainType.River: return TradeMath.WaterRiver;
                default: return TradeMath.WaterElse;
            }
        }

        private static int WaterAt(IMapScene map, PathFaceRecord face)
        {
            if (!face.IsValid()) return TradeMath.WaterUnread;
            if (!_waterOfFace.TryGetValue(face.FaceIndex, out int water))
                _waterOfFace[face.FaceIndex] = water = WaterOf(map.GetFaceTerrainType(face));
            return water;
        }

        private static float ProbeAtSea(IMapScene map, MapScene winds, Vec2 from, Vec2 to, List<TradeMath.Run> runs,
                                        List<TradeMath.Sail> sails)
        {
            float length = from.Distance(to);
            if (!(length > 0f) || float.IsInfinity(length)) return 0f;
            Vec2 heading = (to - from) * (1f / length);
            int steps = (int)Math.Ceiling(length / Stride);
            if (steps < 1) steps = 1;
            float each = length / steps;
            for (int k = 0; k < steps; k++)
            {
                _probes++;
                Vec2 at = from + heading * (each * (k + 0.5f));
                PathFaceRecord face = map.GetFaceIndex(new CampaignVec2(at, false));
                Vec2 flow = winds.GetWindAtPosition(at);
                TradeMath.WindOnTheMap(flow.x, flow.y, out float windX, out float windY);
                float push = TradeMath.WindBehind(heading.x, heading.y, windX, windY, out bool astern);
                TradeMath.AddRunAtSea(runs, sails, face.IsValid() ? face.FaceIndex : TradeMath.Unread, CellOf(at),
                                      WaterAt(map, face), each, push, astern ? 1f : 0f);
            }
            return length;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool SailTheSea(PathFaceRecord fromFace, Vec2 from, CampaignVec2 to, Road road)
        {
            MapScene winds = _windsOf;
            MobileParty party = MobileParty.MainParty;
            if (winds == null || party == null) return false;
            Grid();
            IMapScene map = Campaign.Current.MapSceneWrapper;
            int[] barred = Campaign.Current.Models.PartyNavigationModel
                .GetInvalidTerrainTypesForNavigationType(MobileParty.NavigationType.Naval);
            int seaToLand = party.GetRegionSwitchCostFromSeaToLand();
            int landToSea = party.GetRegionSwitchCostFromLandToSea();
            PathFaceRecord toFace = to.Face;
            Vec2 end = to.ToVec2();
            if (!fromFace.IsValid() || !toFace.IsValid()) return false;
            var runs = new List<TradeMath.Run>();
            var sails = new List<TradeMath.Sail>();
            float length = 0f;
            for (int leg = 0; leg < MostPathsToOneRoad; leg++)
            {
                if (fromFace.FaceIndex == toFace.FaceIndex)
                {
                    length += ProbeAtSea(map, winds, from, end, runs, sails);
                    return Sailed(road, runs, sails, length);
                }
                _path.Size = 0;
                long asked = Stopwatch.GetTimestamp();
                bool found = map.GetPathBetweenAIFaces(fromFace, toFace, from, end, 0.3f, _path, barred, 2f, seaToLand,
                                                       landToSea);
                _pathTicks += Stopwatch.GetTimestamp() - asked;
                _paths++;
                if (!found || _path.Size <= 0) return false;
                Vec2 at = from;
                for (int i = 0; i < _path.Size; i++)
                {
                    length += ProbeAtSea(map, winds, at, _path[i], runs, sails);
                    at = _path[i];
                }
                if (at.DistanceSquared(end) <= ReachedSquared) return Sailed(road, runs, sails, length);
                if (at.DistanceSquared(from) <= ReachedSquared) return false;
                fromFace = map.GetFaceIndex(new CampaignVec2(at, false));
                if (!fromFace.IsValid()) return false;
                from = at;
            }
            return false;
        }

        private static bool Sailed(Road road, List<TradeMath.Run> runs, List<TradeMath.Sail> sails, float length)
        {
            if (!(length > 0f)) return false;
            road.Runs = runs;
            road.Sails = sails.ToArray();
            road.Length = length;
            road.LandRatio = 0f;
            road.Walked = true;
            return true;
        }

        private static void SailBetween(Settlement a, Settlement b, Road road)
        {
            CampaignVec2 port = a.PortPosition;
            SailTheSea(port.Face, port.ToVec2(), b.PortPosition, road);
        }

        private static Road SailedFromParty(MobileParty party, Settlement target, bool atSea)
        {
            Settlement inside = party.CurrentSettlement;
            if (!atSea)
            {
                if (inside == null || !inside.HasPort || inside == target) return null;
                Road pair = PairRoad(inside, target);
                return pair.Sails == null || !pair.Walked
                    ? null
                    : new Road { Length = pair.Length, LandRatio = 0f, Runs = pair.Runs, Sails = pair.Sails, Walked = true };
            }
            if (_unwalkable.Contains(target.StringId)) return null;
            StartTheMeter();
            var road = new Road();
            long started = StartAWalk();
            try { SailTheSea(party.CurrentNavigationFace, party.Position.ToVec2(), target.PortPosition, road); }
            catch { road.Walked = false; }
            StopTheMeter(started, ref _partyWalks, "a course from your ships", road);
            if (road.Walked)
            {
                _seaWalks++;
                return road;
            }
            _unwalkable.Add(target.StringId);
            return null;
        }

        private static bool Passable(IMapScene map, PathFaceRecord face, MobileParty.NavigationType way)
        {
            if (!face.IsValid()) return false;
            if (!_terrainOfFace.TryGetValue(face.FaceIndex, out TerrainType terrain))
                _terrainOfFace[face.FaceIndex] = terrain = map.GetFaceTerrainType(face);
            return Campaign.Current.Models.PartyNavigationModel.IsTerrainTypeValidForNavigationType(terrain, way);
        }

        private static float ProbeAfloatThenAshore(IMapScene map, MapScene winds, Vec2 from, Vec2 to,
                                                   List<TradeMath.Run> runs, List<TradeMath.Sail> sails, ref bool afloat)
        {
            if (!afloat) return Probe(map, from, to, runs);
            float length = from.Distance(to);
            if (!(length > 0f) || float.IsInfinity(length)) return 0f;
            Vec2 heading = (to - from) * (1f / length);
            int steps = (int)Math.Ceiling(length / Stride);
            if (steps < 1) steps = 1;
            float each = length / steps;
            for (int k = 0; k < steps; k++)
            {
                _probes++;
                Vec2 at = from + heading * (each * (k + 0.5f));
                PathFaceRecord face = map.GetFaceIndex(new CampaignVec2(at, false));
                if (!Passable(map, face, MobileParty.NavigationType.Naval) &&
                    Passable(map, map.GetFaceIndex(new CampaignVec2(at, true)), MobileParty.NavigationType.Default))
                {
                    afloat = false;
                    Probe(map, from + heading * (each * k), to, runs);
                    return length;
                }
                Vec2 flow = winds.GetWindAtPosition(at);
                TradeMath.WindOnTheMap(flow.x, flow.y, out float windX, out float windY);
                float push = TradeMath.WindBehind(heading.x, heading.y, windX, windY, out bool astern);
                TradeMath.AddRunAtSea(runs, sails, face.IsValid() ? face.FaceIndex : TradeMath.Unread, CellOf(at),
                                      WaterAt(map, face), each, push, astern ? 1f : 0f);
            }
            return length;
        }

        private static float HoursGoingAshore(MobileParty party)
        {
            double hours = Campaign.Current.Models.PartyTransitionModel.GetTransitionTimeDisembarking(party).ToHours;
            return hours >= 0d && hours <= MostHoursGoingAshore ? (float)hours : float.NaN;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool SailAndGoAshore(PathFaceRecord fromFace, Vec2 from, CampaignVec2 to, Road road)
        {
            MapScene winds = _windsOf;
            MobileParty party = MobileParty.MainParty;
            if (winds == null || party == null || !to.IsOnLand) return false;
            float going = HoursGoingAshore(party);
            if (float.IsNaN(going)) return false;
            Grid();
            IMapScene map = Campaign.Current.MapSceneWrapper;
            PartyNavigationModel ways = Campaign.Current.Models.PartyNavigationModel;
            int[] barred = ways.GetInvalidTerrainTypesForNavigationType(MobileParty.NavigationType.All);
            float wide = ways.GetEmbarkDisembarkThresholdDistance();
            int seaToLand = party.GetRegionSwitchCostFromSeaToLand();
            int landToSea = party.GetRegionSwitchCostFromLandToSea();
            PathFaceRecord toFace = to.Face;
            Vec2 end = to.ToVec2();
            if (!fromFace.IsValid() || !toFace.IsValid()) return false;
            var runs = new List<TradeMath.Run>();
            var sails = new List<TradeMath.Sail>();
            bool afloat = true;
            float length = 0f;
            for (int leg = 0; leg < MostPathsToOneRoad; leg++)
            {
                if (fromFace.FaceIndex == toFace.FaceIndex)
                {
                    length += ProbeAfloatThenAshore(map, winds, from, end, runs, sails, ref afloat);
                    return WentAshore(road, runs, sails, length, afloat, going);
                }
                _path.Size = 0;
                long asked = Stopwatch.GetTimestamp();
                bool found = map.GetPathBetweenAIFaces(fromFace, toFace, from, end, afloat ? wide : 0.3f, _path, barred, 2f,
                                                       seaToLand, landToSea);
                _pathTicks += Stopwatch.GetTimestamp() - asked;
                _paths++;
                if (!found || _path.Size <= 0) return false;
                Vec2 at = from;
                for (int i = 0; i < _path.Size; i++)
                {
                    length += ProbeAfloatThenAshore(map, winds, at, _path[i], runs, sails, ref afloat);
                    at = _path[i];
                }
                if (at.DistanceSquared(end) <= ReachedSquared) return WentAshore(road, runs, sails, length, afloat, going);
                if (at.DistanceSquared(from) <= ReachedSquared) return false;
                fromFace = map.GetFaceIndex(new CampaignVec2(at, !afloat));
                if (!fromFace.IsValid()) return false;
                from = at;
            }
            return false;
        }

        private static bool WentAshore(Road road, List<TradeMath.Run> runs, List<TradeMath.Sail> sails, float length,
                                       bool afloat, float going)
        {
            int landfall = sails.Count;
            if (afloat || !(length > 0f) || landfall <= 0 || landfall >= runs.Count) return false;
            float unread = 0f;
            for (int i = landfall; i < runs.Count; i++)
                if (runs[i].Face == TradeMath.Unread) unread += runs[i].Length;
            if (!(unread <= MostUnreadAshore)) return false;
            float sea = TradeMath.LengthOn(runs, TradeMath.GroundSea);
            road.Runs = runs;
            road.Sails = sails.ToArray();
            road.Afloat = runs.GetRange(0, landfall);
            road.Ashore = runs.GetRange(landfall, runs.Count - landfall);
            road.GoingAshore = going;
            road.Length = length;
            road.LandRatio = 1f - sea / length;
            road.Walked = true;
            return true;
        }

        private static void SailToTheShore(Settlement a, Settlement b, Road road)
        {
            CampaignVec2 port = a.PortPosition;
            SailAndGoAshore(port.Face, port.ToVec2(), b.GatePosition, road);
        }

        private static Road LandedFromParty(MobileParty party, Settlement target)
        {
            StartTheMeter();
            var road = new Road();
            long started = StartAWalk();
            try { SailAndGoAshore(party.CurrentNavigationFace, party.Position.ToVec2(), target.GatePosition, road); }
            catch { road.Walked = false; }
            StopTheMeter(started, ref _partyWalks, "a course from your ships to the shore", road);
            if (road.Walked)
            {
                _seaWalks++;
                return road;
            }
            _unwalkable.Add(target.StringId);
            return null;
        }

        private const double RoadsMsAFrame = 3d;
        private const double RoadsMsAScan = 20d;
        private const int RoadsLooksAFrame = 4000;
        private const int RoadsBeforeTheLedgerLooksAgain = 250;
        private const double AScanRestsFor = 9d;
        private static long _scanTook, _scanEnded;
        private static bool _lookAgainOwed;
        private static long _scanRoadTicks, _scanRoadBudget;
        private static bool _scanRoads;
        private static int _walkedForTheLedger;
        private static bool _behind, _justWalked, _fillBroken;
        private static List<Settlement> _fillFor, _fillOrder;
        private static int _fillShell, _fillStep, _fillWalked;
        private static long _fillTicks;
        private static bool _fillDone;
        private static readonly List<(string, string)> _wanted = new List<(string, string)>();
        private static readonly HashSet<(string, string)> _wantedHeld = new HashSet<(string, string)>();
        private static int _wantedAt;

        private static void WantThisRoadWalked((string, string) key)
        {
            if (_wantedHeld.Add(key)) _wanted.Add(key);
        }

        internal static void OpenTheScansRoadBudget()
        {
            _scanRoads = true;
            _scanRoadTicks = 0L;
            _scanRoadBudget = (long)(RoadsMsAScan * Stopwatch.Frequency / 1000d);
        }

        internal static void CloseTheScansRoadBudget() => _scanRoads = false;

        internal static int RoadsStillToWalk() => _wanted.Count - _wantedAt;

        private static bool TheRoadBudgetIsSpent() => _scanRoads && _scanRoadTicks >= _scanRoadBudget;

        private static bool ThePairRoadWaitsForTheBackground(Settlement a, Settlement b)
        {
            if (!TheRoadBudgetIsSpent() || a == null || b == null || a == b) return false;
            var key = (a.StringId, b.StringId);
            if (_pairDist.TryGetValue(key, out Road _)) return false;
            WantThisRoadWalked(key);
            return true;
        }

        private static bool ThePartyRoadWaitsForTheBackground(MobileParty party, Settlement target)
        {
            if (!TheRoadBudgetIsSpent() || party == null || target == null) return false;
            if (target == party.CurrentSettlement || _unwalkable.Contains(target.StringId)) return false;
            WantThisRoadWalked((target.StringId, null));
            return true;
        }

        private static void ForgetWhatTheLedgerWanted()
        {
            _wanted.Clear();
            _wantedHeld.Clear();
            _wantedAt = 0;
            _walkedForTheLedger = 0;
        }

        private static bool TheSameMarkets(List<Settlement> now, List<Settlement> before)
        {
            if (now == null || before == null || now.Count != before.Count) return false;
            for (int i = 0; i < now.Count; i++)
                if (now[i] != before[i]) return false;
            return true;
        }

        private static double MsOf(long ticks) => ticks * 1000d / Stopwatch.Frequency;

        private static void ForgetTheRoadsWalkedBehind()
        {
            _behind = false;
            _fillBroken = false;
            _fillFor = null;
            _fillOrder = null;
            _fillDone = false;
            ForgetWhatTheLedgerWanted();
        }

        private static bool OnTheMap() =>
            Campaign.Current != null && MobileParty.MainParty != null && Mission.Current == null &&
            Game.Current?.GameStateManager?.ActiveState is MapState;

        internal static void NewFrame()
        {
            if (_frameTicks > _busiestFrame) _busiestFrame = _frameTicks;
            _frameTicks = 0L;
        }

        internal static long FrameTicksSoFar() => _frameTicks;

        internal static void SpentOnAScan(long spent, long before)
        {
            long inner = _frameTicks - before;
            if (inner < 0L) inner = 0L;
            _frameTicks += spent > inner ? spent - inner : 0L;
            _scanTook = spent;
            _scanEnded = Stopwatch.GetTimestamp();
        }

        internal static void FillTheRoads()
        {
            if (_fillBroken) return;
            try { FillTheRoadsNow(); }
            catch (Exception e)
            {
                _fillBroken = true;
                _behind = false;
                Log.Error(e, "roads: the roads between markets are no longer walked in the background this " +
                             "session, and are walked as they are needed instead");
            }
        }

        private static bool WalkWhatTheLedgerWanted(long started, long budget)
        {
            while (_wantedAt < _wanted.Count)
            {
                var key = _wanted[_wantedAt++];
                long before = Stopwatch.GetTimestamp();
                bool walked = key.Item2 == null ? WalkThePartyRoadBehind(key.Item1) : WalkThePairRoadBehind(key);
                _fillTicks += Stopwatch.GetTimestamp() - before;
                if (walked && ++_walkedForTheLedger >= RoadsBeforeTheLedgerLooksAgain) TheLedgerMayLookAgain();
                if (Stopwatch.GetTimestamp() - started >= budget) return false;
            }
            if (_wanted.Count > 0)
            {
                ForgetWhatTheLedgerWanted();
                TheLedgerMayLookAgain();
            }
            return true;
        }

        private static bool WalkThePairRoadBehind((string, string) key)
        {
            if (_pairDist.TryGetValue(key, out Road _)) return false;
            Settlement a = Settlement.Find(key.Item1);
            Settlement b = Settlement.Find(key.Item2);
            if (a == null || b == null) return false;
            _justWalked = false;
            _behind = true;
            try { PairRoad(a, b); }
            finally { _behind = false; }
            if (_justWalked) _fillWalked++;
            return true;
        }

        private static bool WalkThePartyRoadBehind(string id)
        {
            MobileParty party = MobileParty.MainParty;
            Settlement target = Settlement.Find(id);
            if (party == null || target == null) return false;
            StandWhereYouAre(party);
            if (_partyDist.TryGetValue(id, out Road _)) return false;
            _behind = true;
            try
            {
                Road road = RoadFromParty(party, target);
                if (!TradeMath.OutOfReach(RideFromParty(road))) _partyDist[id] = road;
            }
            finally { _behind = false; }
            return true;
        }

        private static void TheLedgerMayLookAgain()
        {
            _walkedForTheLedger = 0;
            _lookAgainOwed = !TradeMath.TheScanHasRested(Stopwatch.GetTimestamp() - _scanEnded, _scanTook, AScanRestsFor);
            if (_lookAgainOwed) return;
            try
            {
                LedgerBehavior.Instance?.MoreRoadsAreMeasuredNow();
                LedgerPanel.TheScanIsNew();
            }
            catch { }
        }

        private static void FillTheRoadsNow()
        {
            if (!OnTheMap()) return;
            if (_lookAgainOwed) TheLedgerMayLookAgain();
            long started = Stopwatch.GetTimestamp();
            long budget = (long)(RoadsMsAFrame * Stopwatch.Frequency / 1000d);
            if (!WalkWhatTheLedgerWanted(started, budget)) return;
            List<Settlement> reach = LedgerBehavior.Instance?.MarketsInReach();
            if (reach == null) return;
            if (!ReferenceEquals(reach, _fillFor))
            {
                bool held = TheSameMarkets(reach, _fillFor) && _fillOrder != null;
                _fillFor = reach;
                if (!held)
                {
                    var order = new List<(Settlement s, float days)>(reach.Count);
                    foreach (Settlement s in reach)
                        if (s != null) order.Add((s, StraightDaysFromParty(s)));
                    order.Sort((x, y) => x.days.CompareTo(y.days));
                    _fillOrder = new List<Settlement>(order.Count);
                    foreach (var one in order) _fillOrder.Add(one.s);
                    _fillShell = 0;
                    _fillStep = 0;
                    _fillWalked = 0;
                    _fillTicks = 0L;
                    _fillDone = false;
                }
            }
            if (_fillDone || _fillOrder == null) return;
            float cap = Options.Current.MaxTravelDaysTown;
            int n = _fillOrder.Count;
            for (int looks = 0; looks < RoadsLooksAFrame; looks++)
            {
                if (_fillShell >= n)
                {
                    _fillDone = true;
                    if (_fillWalked > 0)
                        Log.Write("roads: every road between the " + n + " market(s) within your travel ceilings " +
                                  "is ready, " + _fillWalked + " of them walked in the background in " +
                                  MsOf(_fillTicks).ToString("0", CultureInfo.InvariantCulture) +
                                  " ms spread over the frames while you were on the map");
                    return;
                }
                int step = _fillStep;
                Settlement from = step < _fillShell ? _fillOrder[_fillShell] : _fillOrder[step - _fillShell];
                Settlement to = step < _fillShell ? _fillOrder[step] : _fillOrder[_fillShell];
                if (++_fillStep > 2 * _fillShell)
                {
                    _fillShell++;
                    _fillStep = 0;
                }
                if (from == to) continue;
                if (_pairDist.TryGetValue((from.StringId, to.StringId), out Road _)) continue;
                if (cap > 0f && StraightDaysFromParty(from) + StraightDaysBetween(from, to) > cap) continue;
                long before = Stopwatch.GetTimestamp();
                _justWalked = false;
                _behind = true;
                try { PairRoad(from, to); }
                finally { _behind = false; }
                if (_justWalked) _fillWalked++;
                _fillTicks += Stopwatch.GetTimestamp() - before;
                if (Stopwatch.GetTimestamp() - started >= budget) return;
            }
        }

        private static readonly Dictionary<string, Road> _partyDist = new Dictionary<string, Road>();
        private static readonly HashSet<string> _unwalkable = new HashSet<string>();
        private static Vec2 _partyAt;
        private static int _partyHour = -1;
        private static readonly Dictionary<(string, string), Road> _pairDist = new Dictionary<(string, string), Road>();
        private static readonly Dictionary<(string, string), (CampaignTime Moment, float After, float Days)> _pairDays =
            new Dictionary<(string, string), (CampaignTime, float, float)>();
        private static bool _cachedNaval;
        private static int _roadsFound, _roadsLost;

        internal static bool LostTheRoad => _roadsLost > 0 && _roadsFound == 0;

        private static void DropIfNavalChanged()
        {
            bool naval = NavalActive;
            if (naval == _cachedNaval) return;
            _cachedNaval = naval;
            ForgetTheRoadsWalkedBehind();
            _speedHour = -1;
            _windsHour = -1;
            _partyDist.Clear();
            _unwalkable.Clear();
            _pairDist.Clear();
            _pairDays.Clear();
            _roadsFound = 0;
            _roadsLost = 0;
        }

        internal static void Forget()
        {
            ForgetTheRoadsWalkedBehind();
            _windsHour = -1;
            _windsBlow = false;
            _windsOf = null;
            _seasSaid = null;
            _seasBecause = null;
            _waterOfFace.Clear();
            _terrainOfFace.Clear();
            _partyDist.Clear();
            _unwalkable.Clear();
            _partyHour = -1;
            _pairDist.Clear();
            _pairDays.Clear();
            _cachedNaval = false;
            _speedHour = -1;
            _pace = null;
            _evenBecause = null;
            _meterHour = -1;
            _roadsFound = 0;
            _roadsLost = 0;
            _groundOfFace.Clear();
            _dim = 0;
            _writers = null;
            _slotOf = null;
            _skies = null;
            _skyHour = -1;
            _skyBecause = null;
            _skySeen = false;
            _worked.Clear();
            _lines.Hour = -1;
            _lines.Whole = false;
            _frameTicks = 0L;
            _busiestFrame = 0L;
            _ratios.Clear();
            _ratioAt = 0;
            _ratioSeen = 0;
            _ratioFresh = -1;
            _ratioHeld = 1f;
            _scanTook = 0L;
            _scanEnded = 0L;
            _lookAgainOwed = false;
        }

        private static float Nearest(Vec2 from, Settlement to)
        {
            float gate = from.Distance(to.GatePosition.ToVec2());
            return to.HasPort ? Math.Min(gate, from.Distance(to.PortPosition.ToVec2())) : gate;
        }

        private static float Nearest(Settlement a, Settlement b)
        {
            float gate = Nearest(a.GatePosition.ToVec2(), b);
            return a.HasPort ? Math.Min(gate, Nearest(a.PortPosition.ToVec2(), b)) : gate;
        }

        internal static float StraightDaysFromParty(Settlement target)
        {
            MobileParty party = MobileParty.MainParty;
            if (target == null || party == null) return 0f;
            return StraightDays(Nearest(party.GetPosition2D, target), MaySail(party, target));
        }

        internal static float StraightDaysBetween(Settlement a, Settlement b) =>
            a == null || b == null ? 0f : StraightDays(Nearest(a, b), a.HasPort);

        internal static float StraightDaysFor(MobileParty party, Settlement target)
        {
            if (party == null || target == null) return 0f;
            return StraightDays(Nearest(party.GetPosition2D, target), false);
        }

        private static bool MaySail(MobileParty party, Settlement target) =>
            party.IsCurrentlyAtSea || target.HasPort && party.CurrentSettlement?.HasPort == true;

        private static float StraightDays(float distance, bool sailed)
        {
            DropIfNavalChanged();
            Speeds(sailed, out float land, out float sea);
            return TradeMath.DaysAtBestSpeed(distance, land, sea);
        }

        private static void StandWhereYouAre(MobileParty party)
        {
            int hour = (int)CampaignTime.Now.ToHours;
            Vec2 at = party.GetPosition2D;
            if (hour == _partyHour && at.DistanceSquared(_partyAt) <= 100f) return;
            if (at.DistanceSquared(_partyAt) > StoodStill)
            {
                _partyDist.Clear();
                _unwalkable.Clear();
                _speedHour = -1;
            }
            _partyHour = hour;
            _partyAt = at;
            _moment = CampaignTime.Now;
            _roadsFound = 0;
            _roadsLost = 0;
        }

        internal static float EstimateDaysFromParty(Settlement target)
        {
            MobileParty party = MobileParty.MainParty;
            if (target == null || party == null) return 0f;
            DropIfNavalChanged();
            StandWhereYouAre(party);
            if (!_partyDist.TryGetValue(target.StringId, out Road hit))
            {
                if (ThePartyRoadWaitsForTheBackground(party, target)) return TradeMath.FurthestThereIs;
                hit = RoadFromParty(party, target);
                if (TradeMath.OutOfReach(RideFromParty(hit)))
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
            else _roadsFound++;
            return RideFromParty(hit);
        }

        private static float FromWhereYouStand(MobileParty party, Settlement target)
        {
            Settlement inside = party.CurrentSettlement;
            if (inside == null || ThePairRoadWaitsForTheBackground(inside, target)) return TradeMath.FurthestThereIs;
            return RideBetween(PairRoad(inside, target), 0f, false);
        }

        private static Road PairRoad(Settlement a, Settlement b)
        {
            var key = (a.StringId, b.StringId);
            if (!_pairDist.TryGetValue(key, out Road hit))
            {
                hit = RoadBetween(a, b);
                _pairDist[key] = hit;
            }
            return hit;
        }

        private const int RoadRatiosKept = 512;
        private const int LeastRatiosToTrust = 20;
        private const int RoadRatioEveryNth = 16;
        private const float RoadRatioShare = 0.75f;
        private static readonly List<float> _ratios = new List<float>(RoadRatiosKept);
        private static int _ratioAt, _ratioSeen, _ratioFresh = -1;
        private static float _ratioHeld = 1f;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void NoteHowRoughTheGuessWas(float dist, float landRatio, Road walked)
        {
            if (++_ratioSeen % RoadRatioEveryNth != 0 && _ratios.Count >= LeastRatiosToTrust) return;
            try
            {
                TradeMath.Pace pace = PartyPace();
                if (pace == null || pace.Even) return;
                var rough = new Road
                {
                    Length = dist, LandRatio = landRatio, Runs = Unwalked(dist, landRatio, false)
                };
                float longer = TradeMath.HowMuchLongerTheRideIs(Ride(rough, pace, _moment, 0d, false, null),
                                                                Ride(walked, pace, _moment, 0d, false, null));
                if (float.IsNaN(longer)) return;
                if (_ratios.Count < RoadRatiosKept) { _ratios.Add(longer); return; }
                _ratios[_ratioAt] = longer;
                _ratioAt = (_ratioAt + 1) % RoadRatiosKept;
            }
            catch { }
        }

        internal static float TheRoughRoadTakesLongerBy()
        {
            int hour = Campaign.Current == null ? -1 : (int)CampaignTime.Now.ToHours;
            if (hour == _ratioFresh) return _ratioHeld;
            _ratioFresh = hour;
            if (_ratios.Count < LeastRatiosToTrust) { _ratioHeld = 1f; return 1f; }
            var seen = new List<float>(_ratios);
            seen.Sort();
            float at = TradeMath.Quantile(seen, RoadRatioShare);
            _ratioHeld = float.IsNaN(at) || at < 1f ? 1f : at;
            return _ratioHeld;
        }

        internal static float EstimateDaysBetween(Settlement a, Settlement b, float after)
        {
            MobileParty party = MobileParty.MainParty;
            if (a == null || b == null || party == null) return 0f;
            if (TradeMath.OutOfReach(after)) return TradeMath.FurthestThereIs;
            DropIfNavalChanged();
            StandWhereYouAre(party);
            var key = (a.StringId, b.StringId);
            if (_pairDays.TryGetValue(key, out var known) && known.Moment == _moment && known.After == after)
                return known.Days;
            if (ThePairRoadWaitsForTheBackground(a, b)) return TradeMath.FurthestThereIs;
            float days = RideBetween(PairRoad(a, b), after, true);
            _pairDays[key] = (_moment, after, days);
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

        internal static float DaysFor(MobileParty party, Settlement bound)
        {
            if (party == null || bound == null) return 0f;
            float days = float.NaN;
            if (!_othersUnbound)
                try { days = AlongItsRoad(party, bound); }
                catch (Exception e) when (Unbound(e))
                {
                    _othersUnbound = true;
                    Log.Write("binding: a caravan's or villager's speed could not be read from this game version (" +
                              e.GetType().Name + ": " + e.Message + "), so they are timed by straight distance " +
                              "until the game restarts");
                }
                catch { }
            if (!float.IsNaN(days)) return days;
            _othersGuessed++;
            return TradeMath.EtaDays(party.GetPosition2D.Distance(bound.GetPosition2D), SpeedByDay(party));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static float AlongItsRoad(MobileParty party, Settlement bound)
        {
            StartTheMeter();
            if (party.IsCurrentlyAtSea || party.IsTargetingPort) return float.NaN;
            TradeMath.Pace pace = TradeMath.Pace.AllAt(1f, 1f);
            if (ReadPace(party, pace) != null) return float.NaN;
            Grid();
            var road = new Road { Runs = new List<TradeMath.Run>() };
            bool sails = party.NavigationCapability != MobileParty.NavigationType.Default;
            long started = StartAWalk();
            bool walked = WalkItsRoad(party, bound, road.Runs, !sails, out _, out bool own);
            road.Walked = walked;
            StopTheMeter(started, ref _otherWalks, "a road for a caravan or villager", road);
            if (!walked) return float.NaN;
            if (sails)
                foreach (TradeMath.Run run in road.Runs)
                    if (run.Face == TradeMath.Unread) return float.NaN;
            if (own) _othersOnTheirPath++;
            else _othersOnAFreshPath++;
            float days = Ride(road, pace, CampaignTime.Now, 0d, false, null);
            return TradeMath.OutOfReach(days) ? float.NaN : days;
        }

        private static bool WalkItsRoad(MobileParty party, Settlement bound, List<TradeMath.Run> runs, bool mayPathAfresh,
                                        out float length, out bool own)
        {
            own = false;
            length = 0f;
            IMapScene map = Campaign.Current.MapSceneWrapper;
            CampaignVec2 gate = bound.GatePosition;
            PathFaceRecord face = party.CurrentNavigationFace;
            PathFaceRecord gateFace = gate.Face;
            Vec2 at = party.Position.ToVec2();
            if (!face.IsValid() || !gateFace.IsValid()) return false;
            if (face.FaceIndex == gateFace.FaceIndex)
            {
                own = true;
                length += Probe(map, at, gate.ToVec2(), runs);
                return true;
            }
            NavigationPath path = party.Path;
            if (path == null || party.AttachedTo != null || party.PathLastFace.FaceIndex != gateFace.FaceIndex ||
                party.PathBegin < 0 || party.PathBegin >= path.Size ||
                party.NextTargetPosition.ToVec2().DistanceSquared(path[party.PathBegin]) > ReachedSquared)
                return mayPathAfresh && WalkTheRoad(party, face, at, gate, runs, ref length);
            own = true;
            for (int i = party.PathBegin; i < path.Size; i++)
            {
                length += Probe(map, at, path[i], runs);
                at = path[i];
            }
            if (at.DistanceSquared(gate.ToVec2()) <= ReachedSquared) return true;
            face = map.GetFaceIndex(new CampaignVec2(at, true));
            return mayPathAfresh && face.IsValid() && WalkTheRoad(party, face, at, gate, runs, ref length);
        }

        private static string Share(float part, float whole) => Scoring.Share(whole > 0f ? part / whole : 0f);

        private static string SeaOn(Road road, TradeMath.Pace pace)
        {
            if (road.Sails == null || road.Runs == null) return "";
            double sea = 0d, calm = 0d, sailed = 0d;
            var on = new double[TradeMath.Waters];
            for (int i = 0; i < road.Runs.Count && i < road.Sails.Length; i++)
            {
                if (road.Runs[i].Ground != TradeMath.GroundSea || !(road.Runs[i].Length > 0f)) continue;
                TradeMath.Sail sail = road.Sails[i];
                float still = pace.AtSea(sail.Water, 0f, false, false), moving = pace.AtSea(sail, false, false);
                if (!(still > 0f) || !(moving > 0f)) return "";
                float length = road.Runs[i].Length;
                sea += length;
                if (sail.Water >= 0 && sail.Water < TradeMath.Waters) on[sail.Water] += length;
                calm += length / still;
                sailed += length / moving;
            }
            if (!(sea > 0d) || !(sailed > 0d)) return "";
            return "; " + Share((float)sea, road.Length) + " of it at sea, " +
                   Share((float)on[TradeMath.WaterOpen], (float)sea) + " open sea, " +
                   Share((float)on[TradeMath.WaterCoast], (float)sea) + " coastal waters and " +
                   Share((float)on[TradeMath.WaterRiver], (float)sea) + " river, where the War Sails wind adds " +
                   Scoring.Share((float)(calm / sailed - 1d)) + " to your ships' speed on average" +
                   (pace.FairWinds > 0f ? ", Fair Winds included" : "") +
                   (pace.ShockAndAwe > 0f ? ", Shock and Awe included" : "") +
                   (road.Ashore != null
                       ? ", then " + road.GoingAshore.ToString("0.0", CultureInfo.InvariantCulture) +
                         " hour(s) going ashore before the rest of the ride on land"
                       : "");
        }

        internal static string SeasRead()
        {
            if (!NavalActive) return null;
            PartyPace();
            return _seasSaid ?? "sea step read: your ships' speed was not read from War Sails, because " +
                                (_seasBecause ?? "it was not read") + ", so every ride at sea is timed at the ships' own speed";
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string SeaCheck(MobileParty party, float game)
        {
            TradeMath.Pace pace = PartyPace();
            if (_seasSaid == null) return null;
            int water = WaterAt(Campaign.Current.MapSceneWrapper, party.CurrentNavigationFace);
            Vec2 wind = Campaign.Current.Models.MapWeatherModel.GetWindForPosition(party.Position);
            Vec2 bearing = party.Bearing;
            float push = TradeMath.WindBehind(bearing.x, bearing.y, wind.x, wind.y, out bool astern);
            var sail = new TradeMath.Sail { Push = push, Fair = astern ? 1f : 0f, Water = water };
            SkyOver(party, out _, out bool snowy);
            bool slow = party.IsDisorganized;
            float counted = pace.AtSea(sail, snowy, slow);
            float still = pace.AtSea(water, 0f, snowy, slow);
            string where = water == TradeMath.WaterOpen ? "open sea"
                         : water == TradeMath.WaterCoast ? "coastal waters"
                         : water == TradeMath.WaterRiver ? "a river" : "other waters";
            return "sea check: on " + where + " the game gives your ships a Speed of " + Figure(game) +
                   " and TradeLord counts " + Figure(counted) + ", " + Off(counted, game) + ", with the wind " +
                   (astern ? "behind" : "against") + " the ships adding " +
                   (still > 0f ? Scoring.Share(counted / still - 1f) : "unknown") + (snowy ? ", in snow" : "") +
                   (slow ? ", Disorganized" : "");
        }

        private static string Off(float said, float happened)
        {
            if (float.IsNaN(said) || !(happened > 0f)) return "which could not be weighed";
            return (Math.Abs(said - happened) / happened).ToString("0.0%", CultureInfo.InvariantCulture) + " off";
        }

        private static string Ms(long ticks) =>
            (ticks * 1000d / Stopwatch.Frequency).ToString("0.0", CultureInfo.InvariantCulture);

        internal static string TheRideTo(Settlement target)
        {
            if (target == null) return "no market to ride to";
            string name = Tongue.Named(target.Name, target.StringId);
            if (!_partyDist.TryGetValue(target.StringId, out Road road))
                return "the ride to " + name + (MobileParty.MainParty?.CurrentSettlement != null
                    ? " was timed from the market you stand in"
                    : " was not timed from where your party stands this hour");
            TradeMath.Pace pace = PartyPace();
            var tally = new TradeMath.Tally();
            float days = Ride(road, pace, _moment, 0d, false, tally);
            float land = TradeMath.LengthOn(road.Runs, -1) - TradeMath.LengthOn(road.Runs, TradeMath.GroundSea);
            CultureInfo inv = CultureInfo.InvariantCulture;
            Skies skies = _skies;
            var cells = new HashSet<int>();
            foreach (TradeMath.Run run in road.Runs)
                if (run.Cell >= 0) cells.Add(run.Cell);
            bool metered = _meterHour == (int)CampaignTime.Now.ToHours;
            string sky = skies == null
                ? "the weather held as it is now because " + (_skyBecause ?? "it was not read")
                : "the weather worked out ahead for " + cells.Count + " place(s) on the road, the game's weather now " +
                  "matching the forecast in " + skies.Agreed + " of " + skies.Checked + " place(s) checked this hour";
            return "the ride to " + name + ": " + days.ToString("0.00", inv) + " day(s) over " +
                   road.Length.ToString("0.0", inv) +
                   (road.Walked ? " of the road your party would take" : " of road as the game's distance has it") +
                   (pace.Even
                       ? ", at your party's speed by day as it is now because " + _evenBecause
                       : ", base speed " + pace.Base.ToString("0.00", inv) + ", " +
                         Share(TradeMath.LengthOn(road.Runs, TradeMath.GroundForest), land) + " forest, " +
                         Share(TradeMath.LengthOn(road.Runs, TradeMath.GroundWater), land) + " water and fords, " +
                         Share(TradeMath.LengthOn(road.Runs, TradeMath.GroundDesert), land) + " desert, " +
                         tally.Night.ToString("0.0", inv) + " night hour(s), " +
                         tally.Wet.ToString("0.0", inv) + " hour(s) on wet ground, " +
                         tally.Snow.ToString("0.0", inv) + " hour(s) in snow, " +
                         tally.Slow.ToString("0.0", inv) + " hour(s) Disorganized") +
                   SeaOn(road, pace) +
                   "; " + sky + "; this hour " +
                   (metered ? _partyWalks : 0) + " road(s) walked from your party, " + (metered ? _pairWalks : 0) +
                   " between markets" +
                   (metered && _walkedBehind > 0 ? ", " + _walkedBehind + " of them in the background" : "") +
                   (metered && _seaWalks > 0 ? ", " + _seaWalks + " of the roads walked across the sea" : "") +
                   (_ratios.Count > 0
                       ? ", a road the ledger has not walked counted at " +
                         TheRoughRoadTakesLongerBy().ToString("0.00", inv) + " times longer for the ground it " +
                         "cannot read, the figure " + _ratios.Count + " walked road(s) give"
                       : "") +
                   (_wanted.Count > _wantedAt
                       ? ", " + (_wanted.Count - _wantedAt) + " still queued for the background"
                       : "") +
                   " and " + (metered ? _otherWalks : 0) + " for caravans and villagers, " +
                   (metered ? _paths : 0) + " pathfinding call(s) in " + Ms(metered ? _pathTicks : 0L) + " ms, " +
                   (metered ? _probes : 0) + " ground probe(s) in " +
                   Ms(metered ? _walkTicks - _pathTicks : 0L) + " ms, " +
                   (metered ? _forecasts : 0) + " weather turn(s) worked out in " +
                   Ms(metered ? _forecastTicks : 0L) + " ms, " +
                   (metered ? _rides : 0) + " ride(s) timed in " + Ms(metered ? _rideTicks : 0L) + " ms, the slowest walk " +
                   Ms(metered ? _slowestWalk : 0L) + " ms" +
                   (metered && _slowestWalkSaid != null ? " (" + _slowestWalkSaid + ")" : "") +
                   ", the busiest frame this session " + Ms(_busiestFrame) + " ms";
        }

        internal static string TheRoadsOthersRide()
        {
            if (_meterHour != (int)CampaignTime.Now.ToHours) return "no caravan or villager was timed this hour";
            return "caravans and villagers this hour: " + (_othersOnTheirPath + _othersOnAFreshPath) +
                   " timed along their road (" + _othersOnTheirPath + " on the path they follow, " + _othersOnAFreshPath +
                   " on a path found for them) and " + _othersGuessed +
                   " by straight distance, at sea, bound for a port or with a speed that could not be read";
        }
    }
}
