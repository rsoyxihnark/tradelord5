using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Helpers;
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

        private static float LandSpeed()
        {
            return SpeedByDay(MobileParty.MainParty);
        }

        internal static float SpeedByDay(MobileParty party)
        {
            float now = party.Speed;
            try { return LandSpeedByDay(party, now); } catch { return now; }
        }

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
        private const double LongestForecastHours = 720d;
        private const int MostWeatherTurnsKept = 200000;
        private const byte Wet = 1;
        private const byte Snowy = 2;
        private const string NavalSpeedModel = "NavalDLC.GameComponents.NavalDLCPartySpeedCalculationModel";
        private const string NavalWeatherModel = "NavalDLC.GameComponents.NavalDLCMapWeatherModel";
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
        private static int _walkedBehind, _atlasTaken;
        private static int _othersOnTheirPath, _othersOnAFreshPath, _othersGuessed;
        private static long _walkTicks, _pathTicks, _forecastTicks, _rideTicks, _slowestWalk;

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
            _evenBecause = why;
            _speedHour = hour;
            _pace = pace;
            return pace;
        }

        private static void Speeds(out float land, out float sea)
        {
            TradeMath.Pace pace = PartyPace();
            land = pace.BestOnLand;
            sea = pace.Sea;
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
            _forecastTicks += Stopwatch.GetTimestamp() - started;
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
            if (pace.Even) return TradeMath.DaysAtSpeed(road.Length, road.LandRatio, pace.Base, pace.Sea);
            TradeMath.ISky sky = SkyAt(moment, after);
            long started = Stopwatch.GetTimestamp();
            double start = moment.ToHours + after;
            float days = TradeMath.DaysAlong(road.Runs, pace.After(start - pace.ReadAt), sky, start, CampaignTime.SunRise,
                                             CampaignTime.SunSet, fresh, tally);
            if (tally == null)
            {
                _rideTicks += Stopwatch.GetTimestamp() - started;
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
            _atlasTaken = 0;
            _walkTicks = 0L;
            _pathTicks = 0L;
            _forecastTicks = 0L;
            _rideTicks = 0L;
            _slowestWalk = 0L;
        }

        private static void StopTheMeter(long started, ref int walks)
        {
            long took = Stopwatch.GetTimestamp() - started;
            walks++;
            _walkTicks += took;
            if (took > _slowestWalk) _slowestWalk = took;
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
            if (!atSea && landRatio >= AllOnLand && !_unwalkable.Contains(target.StringId))
            {
                StartTheMeter();
                long started = Stopwatch.GetTimestamp();
                try { WalkFromParty(party, target, road); }
                catch { road.Walked = false; }
                StopTheMeter(started, ref _partyWalks);
                if (!road.Walked) _unwalkable.Add(target.StringId);
            }
            if (!road.Walked)
            {
                road.Length = dist;
                road.LandRatio = landRatio;
                road.Runs = Unwalked(dist, landRatio, atSea);
            }
            return road;
        }

        private static Road RoadBetween(Settlement a, Settlement b)
        {
            float dist = Between(a, b, out float landRatio);
            var road = new Road { Length = dist, LandRatio = landRatio };
            if (landRatio >= AllOnLand && a != b)
            {
                StartTheMeter();
                if (!FromTheAtlas(a, b, road))
                {
                    long started = Stopwatch.GetTimestamp();
                    bool threw = false;
                    try { WalkBetween(a, b, road); }
                    catch { road.Walked = false; threw = true; }
                    StopTheMeter(started, ref _pairWalks);
                    _justWalked = true;
                    if (_behind) _walkedBehind++;
                    if (!threw) IntoTheAtlas(a, b, road);
                }
            }
            if (!road.Walked)
            {
                road.Length = dist;
                road.LandRatio = landRatio;
                road.Runs = Unwalked(dist, landRatio, false);
            }
            return road;
        }

        private const string AtlasFileName = "TradeLord.roads";
        private const double AtlasMsAFrame = 3d;
        private const int AtlasLooksAFrame = 4000;
        private const int AtlasWaitMs = 20000;
        private static readonly TimeSpan AtlasKeptAtMostEvery = TimeSpan.FromMinutes(5);

        private static Dictionary<(string, string), RoadAtlas.Road> _atlas;
        private static RoadAtlas.Key _atlasKey;
        private static string _atlasPath;
        private static Task<(List<RoadAtlas.Road> roads, string why, double ms)> _atlasReading;
        private static Task<string> _atlasWriting;
        private static bool _atlasWritingSaid = true;
        private static bool _atlasOff, _atlasDirty, _behind, _justWalked, _fillBroken;
        private static int _atlasTriedHour = -1;
        private static DateTime _atlasKeptAt = DateTime.MinValue;
        private static List<Settlement> _fillFor, _fillOrder;
        private static int _fillShell, _fillStep, _fillWalked;
        private static long _fillTicks;
        private static bool _fillDone;

        private static double MsOf(long ticks) => ticks * 1000d / Stopwatch.Frequency;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool AtlasKeyNow(out RoadAtlas.Key key)
        {
            key = default(RoadAtlas.Key);
            Campaign campaign = Campaign.Current;
            MobileParty party = MobileParty.MainParty;
            if (campaign?.MapSceneWrapper == null || party == null) return false;
            Grid();
            if (_dim <= 0) return false;
            IMapScene map = campaign.MapSceneWrapper;
            int[] barred = (int[])(campaign.Models.PartyNavigationModel
                .GetInvalidTerrainTypesForNavigationType(MobileParty.NavigationType.Default) ?? new int[0]).Clone();
            Array.Sort(barred);
            key = new RoadAtlas.Key
            {
                NavigationMesh = map.GetSceneNavigationMeshCrc(),
                Scene = map.GetSceneXmlCrc(),
                Grid = _dim,
                CellWide = _cellWide,
                CellHigh = _cellHigh,
                Stride = Stride,
                Way = RoadAtlas.TheWayRoadsAreWalked(MostPathsToOneRoad, ReachedSquared, 0.3f, 2f),
                SeaToLand = party.GetRegionSwitchCostFromSeaToLand(),
                LandToSea = party.GetRegionSwitchCostFromLandToSea(),
                Barred = barred
            };
            return true;
        }

        private static (List<RoadAtlas.Road> roads, string why, double ms) ReadTheAtlas(string path, RoadAtlas.Key key)
        {
            long started = Stopwatch.GetTimestamp();
            try
            {
                if (!File.Exists(path)) return (null, "there is none there yet", 0d);
                using (FileStream file = File.OpenRead(path))
                {
                    bool read = RoadAtlas.Read(file, key, out List<RoadAtlas.Road> roads, out string why);
                    return (read ? roads : null, why, MsOf(Stopwatch.GetTimestamp() - started));
                }
            }
            catch (Exception e)
            {
                return (null, "it could not be opened (" + e.GetType().Name + ")", MsOf(Stopwatch.GetTimestamp() - started));
            }
        }

        private static bool AtlasReady(bool wait)
        {
            if (_atlas != null) return true;
            if (_atlasOff) return false;
            if (_atlasReading == null)
            {
                int hour = (int)CampaignTime.Now.ToHours;
                if (hour == _atlasTriedHour) return false;
                _atlasTriedHour = hour;
                bool keyed;
                RoadAtlas.Key key;
                try { keyed = AtlasKeyNow(out key); }
                catch (Exception e)
                {
                    _atlasOff = true;
                    Log.Write("road atlas: left off for this session, as the map's roads could not be read (" +
                              e.GetType().Name + "), so roads between markets are walked as they are needed");
                    return false;
                }
                if (!keyed) return false;
                string path = Log.Beside(AtlasFileName, mustExist: false);
                if (path == null)
                {
                    _atlasOff = true;
                    Log.Write("road atlas: left off for this session, as no folder for " + AtlasFileName +
                              " could be found next to TradeLord.log");
                    return false;
                }
                _atlasKey = key;
                _atlasPath = path;
                _atlasReading = Task.Run(() => ReadTheAtlas(path, key));
            }
            if (!_atlasReading.IsCompleted)
            {
                if (!wait) return false;
                try { _atlasReading.Wait(AtlasWaitMs); }
                catch { }
                if (!_atlasReading.IsCompleted) return false;
            }
            TakeTheAtlas();
            return _atlas != null;
        }

        private static void TakeTheAtlas()
        {
            (List<RoadAtlas.Road> roads, string why, double ms) read;
            try { read = _atlasReading.Result; }
            catch (Exception e) { read = (null, "it could not be read (" + e.GetType().Name + ")", 0d); }
            _atlas = new Dictionary<(string, string), RoadAtlas.Road>();
            if (read.roads == null)
            {
                Log.Write("road atlas: nothing read from " + _atlasPath + " because " + read.why +
                          ", so roads between markets are walked as they are needed and kept there for next time");
                return;
            }
            foreach (RoadAtlas.Road road in read.roads)
                if (road.From != null && road.To != null) _atlas[(road.From, road.To)] = road;
            Log.Write("road atlas: " + _atlas.Count + " road(s) between markets read from " + _atlasPath + " in " +
                      read.ms.ToString("0", CultureInfo.InvariantCulture) + " ms, so none of them is walked again");
        }

        private static bool FromTheAtlas(Settlement a, Settlement b, Road road)
        {
            if (!AtlasReady(wait: true)) return false;
            var key = (a.StringId, b.StringId);
            if (!_atlas.TryGetValue(key, out RoadAtlas.Road kept)) return false;
            Vec2 from = a.GatePosition.ToVec2(), to = b.GatePosition.ToVec2();
            if (!kept.Between(from.x, from.y, to.x, to.y))
            {
                _atlas.Remove(key);
                _atlasDirty = true;
                return false;
            }
            _atlasTaken++;
            if (!kept.Walked || kept.Runs == null) return true;
            road.Runs = kept.Runs;
            road.Length = kept.Length;
            road.LandRatio = 1f;
            road.Walked = true;
            return true;
        }

        private static void IntoTheAtlas(Settlement a, Settlement b, Road road)
        {
            if (_atlas == null) return;
            Vec2 from = a.GatePosition.ToVec2(), to = b.GatePosition.ToVec2();
            _atlas[(a.StringId, b.StringId)] = new RoadAtlas.Road
            {
                From = a.StringId, To = b.StringId, FromX = from.x, FromY = from.y, ToX = to.x, ToY = to.y,
                Walked = road.Walked, Length = road.Walked ? road.Length : 0f, Runs = road.Walked ? road.Runs : null
            };
            _atlasDirty = true;
        }

        private static string WriteTheAtlas(string path, RoadAtlas.Key key, List<RoadAtlas.Road> roads)
        {
            long started = Stopwatch.GetTimestamp();
            string fresh = path + ".new";
            try
            {
                using (FileStream file = File.Create(fresh)) RoadAtlas.Write(file, key, roads);
                long size = new FileInfo(fresh).Length;
                if (File.Exists(path)) File.Delete(path);
                File.Move(fresh, path);
                return "road atlas: " + roads.Count + " road(s) between markets kept in " + path + ", " + size / 1024 +
                       " KB, in " + MsOf(Stopwatch.GetTimestamp() - started).ToString("0", CultureInfo.InvariantCulture) +
                       " ms, for the next time this map is played";
            }
            catch (Exception e)
            {
                try { if (File.Exists(fresh)) File.Delete(fresh); }
                catch { }
                return "road atlas: the roads between markets could not be kept in " + path + " (" + e.GetType().Name +
                       "), so they are walked again next time";
            }
        }

        private static void SayHowTheAtlasWasKept()
        {
            Task<string> writing = _atlasWriting;
            if (_atlasWritingSaid || writing == null || !writing.IsCompleted) return;
            _atlasWritingSaid = true;
            string said;
            try { said = writing.Result; }
            catch (Exception e) { said = "road atlas: the roads between markets could not be kept (" + e.GetType().Name + ")"; }
            Log.Write(said);
        }

        internal static void KeepTheAtlas(bool wait) => KeepTheAtlas(wait, true);

        private static void KeepTheAtlas(bool wait, bool now)
        {
            if (_atlasWriting != null && !_atlasWriting.IsCompleted)
            {
                if (!wait) return;
                try { _atlasWriting.Wait(AtlasWaitMs); }
                catch { }
            }
            SayHowTheAtlasWasKept();
            if (_atlas == null || !_atlasDirty || _atlasPath == null) return;
            if (_atlasWriting != null && !_atlasWriting.IsCompleted) return;
            if (!now && DateTime.UtcNow - _atlasKeptAt < AtlasKeptAtMostEvery) return;
            _atlasKeptAt = DateTime.UtcNow;
            var known = new HashSet<string>(StringComparer.Ordinal);
            foreach (Settlement s in Settlement.All)
                if (s?.StringId != null) known.Add(s.StringId);
            var roads = new List<RoadAtlas.Road>(_atlas.Count);
            foreach (RoadAtlas.Road one in _atlas.Values)
                if (known.Contains(one.From) && known.Contains(one.To)) roads.Add(one);
            string path = _atlasPath;
            RoadAtlas.Key key = _atlasKey;
            _atlasDirty = false;
            _atlasWritingSaid = false;
            _atlasWriting = Task.Run(() => WriteTheAtlas(path, key, roads));
            if (!wait) return;
            try { _atlasWriting.Wait(AtlasWaitMs); }
            catch { }
            SayHowTheAtlasWasKept();
        }

        private static void ForgetTheAtlas()
        {
            if (_atlasReading != null && !_atlasReading.IsCompleted)
            {
                try { _atlasReading.Wait(AtlasWaitMs); }
                catch { }
            }
            _atlas = null;
            _atlasReading = null;
            _atlasPath = null;
            _atlasOff = false;
            _atlasDirty = false;
            _behind = false;
            _fillBroken = false;
            _atlasTriedHour = -1;
            _fillFor = null;
            _fillOrder = null;
            _fillDone = false;
        }

        private static bool OnTheMap() =>
            Campaign.Current != null && MobileParty.MainParty != null && Mission.Current == null &&
            Game.Current?.GameStateManager?.ActiveState is MapState;

        internal static void FillTheAtlas()
        {
            if (_fillBroken) return;
            try { FillTheAtlasNow(); }
            catch (Exception e)
            {
                _fillBroken = true;
                _behind = false;
                Log.Error(e, "road atlas: the roads between markets are no longer walked in the background this " +
                             "session, and are walked as they are needed instead");
            }
        }

        private static void FillTheAtlasNow()
        {
            SayHowTheAtlasWasKept();
            if (!OnTheMap() || !AtlasReady(wait: false)) return;
            List<Settlement> reach = LedgerBehavior.Instance?.MarketsInReach();
            if (reach == null) return;
            if (!ReferenceEquals(reach, _fillFor))
            {
                _fillFor = reach;
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
            if (_fillDone) return;
            float cap = Options.Current.MaxTravelDaysTown;
            long started = Stopwatch.GetTimestamp();
            long budget = (long)(AtlasMsAFrame * Stopwatch.Frequency / 1000d);
            int n = _fillOrder.Count;
            for (int looks = 0; looks < AtlasLooksAFrame; looks++)
            {
                if (_fillShell >= n)
                {
                    _fillDone = true;
                    if (_fillWalked > 0)
                        Log.Write("road atlas: every road between the " + n + " market(s) within your travel ceilings " +
                                  "is ready, " + _fillWalked + " of them walked in the background in " +
                                  MsOf(_fillTicks).ToString("0", CultureInfo.InvariantCulture) +
                                  " ms spread over the frames while you were on the map");
                    KeepTheAtlas(false, false);
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
                if (from == to || _pairDist.ContainsKey((from.StringId, to.StringId))) continue;
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
            _speedHour = -1;
            _partyDist.Clear();
            _unwalkable.Clear();
            _pairDist.Clear();
            _pairDays.Clear();
            _roadsFound = 0;
            _roadsLost = 0;
        }

        internal static void Forget()
        {
            ForgetTheAtlas();
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
            return StraightDays(Nearest(party.GetPosition2D, target));
        }

        internal static float StraightDaysBetween(Settlement a, Settlement b) =>
            a == null || b == null ? 0f : StraightDays(Nearest(a, b));

        private static float StraightDays(float distance)
        {
            DropIfNavalChanged();
            Speeds(out float land, out float sea);
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
            return inside == null ? TradeMath.FurthestThereIs : RideBetween(PairRoad(inside, target), 0f, false);
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
            float days;
            try { days = AlongItsRoad(party, bound); }
            catch { days = float.NaN; }
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
            long started = Stopwatch.GetTimestamp();
            bool walked = WalkItsRoad(party, bound, road.Runs, !sails, out _, out bool own);
            StopTheMeter(started, ref _otherWalks);
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
                   "; " + sky + "; this hour " +
                   (metered ? _partyWalks : 0) + " road(s) walked from your party, " + (metered ? _pairWalks : 0) +
                   " between markets" +
                   (metered && _walkedBehind > 0 ? ", " + _walkedBehind + " of them in the background" : "") +
                   (metered && _atlasTaken > 0 ? ", " + _atlasTaken + " more between markets taken from the road atlas" : "") +
                   " and " + (metered ? _otherWalks : 0) + " for caravans and villagers, " +
                   (metered ? _paths : 0) + " pathfinding call(s) in " + Ms(metered ? _pathTicks : 0L) + " ms, " +
                   (metered ? _probes : 0) + " ground probe(s) in " +
                   Ms(metered ? _walkTicks - _pathTicks : 0L) + " ms, " +
                   (metered ? _forecasts : 0) + " weather turn(s) worked out in " +
                   Ms(metered ? _forecastTicks : 0L) + " ms, " +
                   (metered ? _rides : 0) + " ride(s) timed in " + Ms(metered ? _rideTicks : 0L) + " ms, the slowest walk " +
                   Ms(metered ? _slowestWalk : 0L) + " ms";
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
