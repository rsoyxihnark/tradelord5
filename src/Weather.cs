using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Library;

namespace TradeLord
{
    internal static class Skies
    {
        private const byte Wet = 1;
        private const byte Snowy = 2;
        private const int MostWorkedKept = 200000;
        private const int MostSharesKept = 200;
        private const int FewestShares = 10;
        private const int CheckEvery = 4;

        internal struct Felt
        {
            internal MobileParty Party;
            internal float Factors;
            internal float Base;
            internal byte Sky;
        }

        internal struct Turn
        {
            internal bool Live;
            internal int Cell;
            internal bool TwinRead;
            internal byte Twin;
            internal bool PartyRead;
            internal Felt Party;
            internal List<Felt> Caravans;
        }

        private sealed class SkyFrom : TradeMath.ISky
        {
            private readonly double _from;

            internal SkyFrom(double from)
            {
                _from = from;
            }

            public void Read(int cell, double clockHours, out bool snowy, out bool wet, out double hoursToChange) =>
                Skies.Read(cell, _from + clockHours, out snowy, out wet, out hoursToChange);
        }

        private static DefaultMapWeatherModel _twin, _live;
        private static string _why = "the weather has not been read yet";
        private static int _dim, _builtFor = -1;
        private static float _cellWide, _cellHigh;
        private static WeatherNode[] _nodes;
        private static double[] _refreshed, _phase;
        private static double _period = 4d;
        private static bool _ticking;
        private static readonly Dictionary<long, byte> _worked = new Dictionary<long, byte>();
        private static readonly HashSet<long> _told = new HashSet<long>();
        private static readonly Dictionary<int, List<MobileParty>> _onTheRoad = new Dictionary<int, List<MobileParty>>();
        private static int _roadHour = -1;
        private static readonly List<float> _snowShares = new List<float>(), _wetShares = new List<float>();
        private static readonly List<TradeMath.RoadPiece> _scratch = new List<TradeMath.RoadPiece>();

        internal static float PartySnowStep { get; private set; } = TradeMath.UsualSnowStep;
        internal static float PartyWetStep => _wetFrom.Length == 0 ? _wetWorkedOut : _wetRead;
        private static float _wetRead, _wetWorkedOut;
        private static string _snowFrom = "", _wetFrom = "";

        private static int _refreshes, _twinChecked, _twinAgreed, _forecastSeen, _cameTrue, _turned;
        private static int _twinWorked, _walks, _looks, _hits, _listens, _partyReads, _caravanReads;
        private static long _twinTicks, _twinSlowest, _walkTicks, _walkSlowest, _listenTicks, _readTicks;
        private static long _frameTicks, _busiestFrame;
        private static double _costFrom = -1d;

        internal static bool Writing => Options.Current.ExtendedDebugLogging;

        internal static float CaravanSnowShare => Middle(_snowShares, TradeMath.SnowShareUnread);

        internal static float CaravanWetShare => Middle(_wetShares, 1f);

        internal static string PartySnowSaid =>
            Step(PartySnowStep) + (_snowFrom.Length == 0 ? " (the game's usual snow, not yet read this session)" : " (read " + _snowFrom + ")");

        internal static string PartyWetSaid =>
            Step(PartyWetStep) + (_wetFrom.Length == 0
                ? " (not yet read this session, so it is worked out from your riders and footmen on horses the way the game does)"
                : " (read " + _wetFrom + ")");

        internal static float PartyWetStepFor(MobileParty party)
        {
            if (_wetFrom.Length == 0) _wetWorkedOut = Drove.WetGroundStep(party);
            return PartyWetStep;
        }

        internal static void Forget()
        {
            _twin = null;
            _live = null;
            _why = "the weather has not been read yet";
            _dim = 0;
            _builtFor = -1;
            _nodes = null;
            _refreshed = null;
            _phase = null;
            _ticking = false;
            _worked.Clear();
            _told.Clear();
            _onTheRoad.Clear();
            _roadHour = -1;
            _snowShares.Clear();
            _wetShares.Clear();
            PartySnowStep = TradeMath.UsualSnowStep;
            _wetRead = 0f;
            _wetWorkedOut = 0f;
            _snowFrom = "";
            _wetFrom = "";
            ForgetTheDay();
            _costFrom = -1d;
        }

        private static void ForgetTheDay()
        {
            _refreshes = _twinChecked = _twinAgreed = _forecastSeen = _cameTrue = _turned = 0;
            _twinWorked = _walks = _looks = _hits = _listens = _partyReads = _caravanReads = 0;
            _twinTicks = _twinSlowest = _walkTicks = _walkSlowest = _listenTicks = _readTicks = 0L;
            _busiestFrame = 0L;
        }

        internal static void Tick()
        {
            _ticking = true;
            if (_frameTicks > _busiestFrame) _busiestFrame = _frameTicks;
            _frameTicks = 0L;
            if (_costFrom < 0d) _costFrom = CampaignTime.Now.ToHours;
        }

        private static void Grid()
        {
            Campaign campaign = Campaign.Current;
            int dim = campaign?.DefaultWeatherNodeDimension ?? 0;
            if (dim == _dim && _dim > 0) return;
            _dim = 0;
            if (dim <= 0 || campaign.MapSceneWrapper == null) return;
            Vec2 size = campaign.MapSceneWrapper.GetTerrainSize();
            float wide = size.X / dim, high = size.Y / dim;
            if (!(wide > 0f) || !(high > 0f) || float.IsInfinity(wide) || float.IsInfinity(high)) return;
            _cellWide = wide;
            _cellHigh = high;
            _dim = dim;
            _refreshed = new double[dim * dim];
            _phase = new double[dim * dim];
            for (int i = 0; i < _phase.Length; i++)
            {
                _refreshed[i] = double.NaN;
                _phase[i] = double.NaN;
            }
        }

        private static bool Ready()
        {
            if (_twin != null && _nodes != null && _dim > 0) return true;
            int hour = (int)CampaignTime.Now.ToHours;
            if (hour == _builtFor) return false;
            _builtFor = hour;
            try { _why = Build(); }
            catch (Exception e) { _why = "the game's weather could not be read (" + e.GetType().Name + ")"; }
            if (_why != null)
            {
                _twin = null;
                if (Writing) Log.Repeatable("weather forecast", _why, "weather forecast: off, because " + _why + ", so every ride is timed under a clear sky");
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string Build()
        {
            Campaign campaign = Campaign.Current;
            if (campaign == null) return "no campaign is running";
            Grid();
            if (_dim <= 0) return "the map's weather grid could not be read";
            WeatherNode[] nodes = campaign.GetCampaignBehavior<MapWeatherCampaignBehavior>()?.AllWeatherNodes;
            if (nodes == null || nodes.Length != _dim * _dim) return "the game's weather cells could not be read";
            double period = campaign.Models.MapWeatherModel.WeatherUpdatePeriod.ToHours;
            if (!(period > 0d)) return "the game says its weather turns every " + period + " hour(s)";
            var twin = new DefaultMapWeatherModel();
            twin.InitializeCaches();
            _nodes = nodes;
            _period = period;
            _twin = twin;
            return null;
        }

        internal static TradeMath.ISky From(double startHours) =>
            Ready() ? new SkyFrom(startHours) : (TradeMath.ISky)TradeMath.ClearSky.Always;

        internal static int CellAt(Vec2 at) =>
            TradeMath.CellOf(at.x, _cellWide, _dim) * _dim + TradeMath.CellOf(at.y, _cellHigh, _dim);

        internal static List<TradeMath.RoadPiece> CellsBetween(Vec2 from, Vec2 to, List<TradeMath.RoadPiece> into)
        {
            List<TradeMath.RoadPiece> pieces = into ?? _scratch;
            pieces.Clear();
            if (!Ready()) return pieces;
            TradeMath.CellsAlong(from.x, from.y, to.x, to.y, _cellWide, _cellHigh, _dim, pieces);
            return pieces;
        }

        private static long Key(int cell, double hours) =>
            ((long)cell << 32) | (uint)(long)Math.Round(hours * 60d);

        private static byte SkyByte(MapWeatherModel.WeatherEvent sky, MapWeatherModel.WeatherEventEffectOnTerrain ground) =>
            (byte)((sky == MapWeatherModel.WeatherEvent.Snowy || sky == MapWeatherModel.WeatherEvent.Blizzard ? Snowy : 0) |
                   (ground == MapWeatherModel.WeatherEventEffectOnTerrain.Wet ? Wet : 0));

        internal static void LiveSkyAt(Vec2 at, out bool snowy, out bool wet)
        {
            MapWeatherModel weather = Campaign.Current.Models.MapWeatherModel;
            byte sky = SkyByte(weather.GetWeatherEventInPosition(at), weather.GetWeatherEffectOnTerrainForPosition(at));
            snowy = (sky & Snowy) != 0;
            wet = (sky & Wet) != 0;
        }

        private static byte LiveSky(Vec2 at)
        {
            MapWeatherModel weather = Campaign.Current.Models.MapWeatherModel;
            return SkyByte(weather.GetWeatherEventInPosition(at), weather.GetWeatherEffectOnTerrainForPosition(at));
        }

        internal static int StormsAlong(Vec2 from, Vec2 to)
        {
            if (!Ready()) return 0;
            MapWeatherModel weather = Campaign.Current.Models.MapWeatherModel;
            List<TradeMath.RoadPiece> pieces = CellsBetween(from, to, null);
            int storms = 0;
            float along = 0f;
            for (int i = 0; i < pieces.Count; i++)
            {
                Vec2 at = from + (to - from) * (along + pieces[i].Share * 0.5f);
                along += pieces[i].Share;
                if (weather.GetWeatherEventInPosition(at) == MapWeatherModel.WeatherEvent.Storm) storms++;
            }
            return storms;
        }

        internal static void Read(int cell, double hours, out bool snowy, out bool wet, out double hoursToChange)
        {
            snowy = false;
            wet = false;
            hoursToChange = double.PositiveInfinity;
            if (_nodes == null || cell < 0 || cell >= _nodes.Length) return;
            WeatherNode node = _nodes[cell];
            if (node == null) return;
            long started = Stopwatch.GetTimestamp();
            _looks++;
            double now = CampaignTime.Now.ToHours;
            double phase = _phase[cell];
            bool known = !double.IsNaN(phase);
            byte sky;
            if (hours <= now + 1e-6 && _live != null)
            {
                Vec2 at = node.Position.ToVec2();
                sky = SkyByte(_live.GetWeatherEventInPosition(at), _live.GetWeatherEffectOnTerrainForPosition(at));
                hoursToChange = known ? phase + (Math.Floor((hours - phase) / _period) + 1d) * _period - hours : 1d;
            }
            else
            {
                double from;
                if (known)
                {
                    double turns = Math.Floor((hours - phase) / _period);
                    from = phase + turns * _period;
                    if (!double.IsNaN(_refreshed[cell]) && _refreshed[cell] > from) from = _refreshed[cell];
                    hoursToChange = phase + (turns + 1d) * _period - hours;
                }
                else
                {
                    from = Math.Floor(hours);
                    hoursToChange = 1d - (hours - from);
                }
                long key = Key(cell, from);
                if (_worked.TryGetValue(key, out sky)) _hits++;
                else
                {
                    sky = WorkOut(cell, from);
                    if (_worked.Count >= MostWorkedKept) _worked.Clear();
                    _worked[key] = sky;
                }
                if (known && hours > now)
                {
                    if (_told.Count >= MostWorkedKept) _told.Clear();
                    _told.Add(key);
                }
            }
            snowy = (sky & Snowy) != 0;
            wet = (sky & Wet) != 0;
            _readTicks += Stopwatch.GetTimestamp() - started;
        }

        private static byte WorkOut(int cell, double hours)
        {
            WeatherNode node = _nodes[cell];
            if (node == null || _twin == null) return 0;
            long started = Stopwatch.GetTimestamp();
            byte sky = Twin(node.Position, hours);
            long took = Stopwatch.GetTimestamp() - started;
            _twinWorked++;
            _twinTicks += took;
            if (took > _twinSlowest) _twinSlowest = took;
            return sky;
        }

        private static byte Twin(CampaignVec2 position, double hours) =>
            Twin(position, CampaignTime.Milliseconds((long)Math.Round(hours * 3600000d)));

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static byte Twin(CampaignVec2 position, CampaignTime at)
        {
            MapWeatherModel.WeatherEvent sky = _twin.UpdateWeatherForPosition(position, at);
            return SkyByte(sky, _twin.GetWeatherEffectOnTerrainForPosition(position.ToVec2()));
        }

        internal static void Walked(long started)
        {
            long took = Stopwatch.GetTimestamp() - started;
            _walks++;
            _walkTicks += took;
            _frameTicks += took;
            if (took > _walkSlowest) _walkSlowest = took;
        }

        internal static void Before(DefaultMapWeatherModel model, CampaignVec2 position, CampaignTime ct, out Turn turn)
        {
            turn = default(Turn);
            if (model == null || model == _twin) return;
            long started = Stopwatch.GetTimestamp();
            _live = model;
            Grid();
            if (_dim <= 0) return;
            turn.Live = true;
            turn.Cell = CellAt(position.ToVec2());
            if (!_ticking || !Ready())
            {
                Listened(started);
                return;
            }
            bool told = !double.IsNaN(_phase[turn.Cell]) && _told.Contains(Key(turn.Cell, Due(turn.Cell, ct.ToHours)));
            if (Writing && (told || (_refreshes % CheckEvery) == 0))
            {
                long twinStarted = Stopwatch.GetTimestamp();
                turn.Twin = Twin(position, ct);
                long took = Stopwatch.GetTimestamp() - twinStarted;
                _twinWorked++;
                _twinTicks += took;
                if (took > _twinSlowest) _twinSlowest = took;
                turn.TwinRead = true;
            }
            MobileParty main = MobileParty.MainParty;
            if (main != null && Steady(main) && CellAt(main.GetPosition2D) == turn.Cell)
            {
                turn.Party = Feel(main);
                turn.PartyRead = turn.Party.Party != null;
                _partyReads++;
            }
            List<MobileParty> here = OnTheRoadIn(turn.Cell);
            if (here != null)
                for (int i = 0; i < here.Count; i++)
                {
                    MobileParty one = here[i];
                    if (one == null || !Steady(one) || CellAt(one.GetPosition2D) != turn.Cell) continue;
                    Felt felt = Feel(one);
                    if (felt.Party == null) continue;
                    (turn.Caravans ?? (turn.Caravans = new List<Felt>())).Add(felt);
                    _caravanReads++;
                }
            Listened(started);
        }

        internal static void After(DefaultMapWeatherModel model, CampaignVec2 position, CampaignTime ct,
                                   MapWeatherModel.WeatherEvent result, Turn turn)
        {
            if (!turn.Live || _refreshed == null || turn.Cell < 0 || turn.Cell >= _refreshed.Length) return;
            long started = Stopwatch.GetTimestamp();
            double at = ct.ToHours;
            byte after = SkyByte(result, model.GetWeatherEffectOnTerrainForPosition(position.ToVec2()));
            bool corner = _nodes != null && _nodes[turn.Cell] != null &&
                          _nodes[turn.Cell].Position.ToVec2().DistanceSquared(position.ToVec2()) < 1e-6f;
            if (_ticking && Ready())
            {
                _refreshes++;
                if (turn.TwinRead)
                {
                    _twinChecked++;
                    if (turn.Twin == after) _twinAgreed++;
                }
                if (corner && !double.IsNaN(_phase[turn.Cell]))
                {
                    long key = Key(turn.Cell, Due(turn.Cell, at));
                    if (_told.Contains(key))
                    {
                        _forecastSeen++;
                        if (_worked.TryGetValue(key, out byte said) && said == after) _cameTrue++;
                        _told.Remove(key);
                    }
                }
                if (corner) _phase[turn.Cell] = at;
                if (turn.PartyRead) Guard.Run("Skies.PartyTurned", () => PartyTurned(turn.Party));
                if (turn.Caravans != null) Guard.Run("Skies.CaravansTurned", () => CaravansTurned(turn.Caravans));
            }
            _refreshed[turn.Cell] = at;
            Listened(started);
        }

        private static double Due(int cell, double hours)
        {
            double phase = _phase[cell];
            return phase + Math.Round((hours - phase) / _period) * _period;
        }

        private static void Listened(long started)
        {
            long took = Stopwatch.GetTimestamp() - started;
            _listens++;
            _listenTicks += took;
            _frameTicks += took;
        }

        private static bool Steady(MobileParty party) =>
            party.IsActive && party.Army == null && party.MapEvent == null && !party.IsCurrentlyAtSea;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Felt Feel(MobileParty party)
        {
            PartySpeedModel model = Campaign.Current.Models.PartySpeedCalculatingModel;
            ExplainedNumber speed = model.CalculateFinalSpeed(party, model.CalculateBaseSpeed(party));
            return new Felt { Party = party, Factors = speed.SumOfFactors, Base = speed.BaseNumber, Sky = LiveSky(party.GetPosition2D) };
        }

        private static List<MobileParty> OnTheRoadIn(int cell)
        {
            int hour = (int)CampaignTime.Now.ToHours;
            if (hour != _roadHour)
            {
                _roadHour = hour;
                foreach (List<MobileParty> list in _onTheRoad.Values) list.Clear();
                MBReadOnlyList<MobileParty> all = MobileParty.All;
                if (all != null)
                    for (int i = 0; i < all.Count; i++)
                    {
                        MobileParty one = all[i];
                        if (one == null || (!one.IsCaravan && !one.IsVillager) || !one.IsActive ||
                            one.CurrentSettlement != null || one.IsCurrentlyAtSea) continue;
                        int at = CellAt(one.GetPosition2D);
                        if (!_onTheRoad.TryGetValue(at, out List<MobileParty> list))
                            _onTheRoad[at] = list = new List<MobileParty>();
                        list.Add(one);
                    }
            }
            return _onTheRoad.TryGetValue(cell, out List<MobileParty> found) && found.Count > 0 ? found : null;
        }

        private static void PartyTurned(Felt before)
        {
            Felt now = Feel(before.Party);
            if (now.Sky == before.Sky || Math.Abs(now.Base - before.Base) > 1e-4f) return;
            _turned++;
            bool snowyBefore = (before.Sky & Snowy) != 0, wetBefore = (before.Sky & Wet) != 0;
            bool snowyAfter = (now.Sky & Snowy) != 0, wetAfter = (now.Sky & Wet) != 0;
            TradeMath.WeatherSteps(snowyBefore, wetBefore, snowyAfter, wetAfter, before.Factors, now.Factors,
                                   _wetFrom.Length == 0 ? float.NaN : PartyWetStep, out float snow, out float wet);
            string turned = Turned(snowyBefore, wetBefore, snowyAfter, wetAfter);
            string said = "weather step as " + turned + " over your party: the game took its pace from a sum of " +
                          Step(before.Factors) + " to " + Step(now.Factors);
            if (!float.IsNaN(snow) && TradeMath.LikeAStep(snow))
            {
                if (Writing)
                    Log.Write(said + ", a snow step of " + Step(snow) + "; TradeLord had counted on " + PartySnowSaid);
                PartySnowStep = snow;
                _snowFrom = "as " + turned + " on " + Today();
                Travel.ReadThePaceAgain();
            }
            else if (!float.IsNaN(wet) && TradeMath.LikeAStep(wet))
            {
                if (Writing)
                    Log.Write(said + ", a wet ground step of " + Step(wet) + "; TradeLord had counted on " + PartyWetSaid);
                _wetRead = wet;
                _wetFrom = "as " + turned + " on " + Today();
                Travel.ReadThePaceAgain();
            }
            else if (Writing)
                Log.Write(said + ", which was not read as a step because snow and wet ground turned together before " +
                          "the wet ground step was known, or the change did not look like the weather alone");
        }

        private static void CaravansTurned(List<Felt> before)
        {
            for (int i = 0; i < before.Count; i++)
            {
                Felt was = before[i];
                Felt now = Feel(was.Party);
                if (now.Sky == was.Sky || Math.Abs(now.Base - was.Base) > 1e-4f) continue;
                bool snowTurned = ((was.Sky ^ now.Sky) & Snowy) != 0, wetTurned = ((was.Sky ^ now.Sky) & Wet) != 0;
                if (snowTurned == wetTurned) continue;
                bool into = (now.Sky & (snowTurned ? Snowy : Wet)) != 0;
                float share = TradeMath.WeatherShare(into, was.Factors, now.Factors);
                if (float.IsNaN(share) || share <= TradeMath.SlowestNightPace || share >= TradeMath.FastestNightPace) continue;
                List<float> kept = snowTurned ? _snowShares : _wetShares;
                if (kept.Count >= MostSharesKept) kept.RemoveAt(0);
                kept.Add(share);
            }
        }

        private static float Middle(List<float> shares, float unread)
        {
            if (shares.Count < FewestShares) return unread;
            var sorted = new List<float>(shares);
            sorted.Sort();
            return TradeMath.Quantile(sorted, 0.5f);
        }

        private static string Turned(bool snowyBefore, bool wetBefore, bool snowyAfter, bool wetAfter)
        {
            if (snowyBefore != snowyAfter) return snowyAfter ? "snow began" : "the snow stopped";
            if (wetBefore != wetAfter) return wetAfter ? "the ground turned wet" : "the ground dried";
            return "the weather turned";
        }

        internal static void SayTheDay()
        {
            if (!Writing) return;
            var lines = new List<string>();
            int cells = 0, known = 0;
            if (_nodes != null)
                for (int i = 0; i < _nodes.Length; i++)
                {
                    if (_nodes[i] == null) continue;
                    cells++;
                    if (!double.IsNaN(_phase[i])) known++;
                }
            lines.Add("weather check over the last day: " + (_why == null
                ? "the game refreshed " + _refreshes + " weather cell(s); TradeLord's own copy of the game's weather worked out " +
                  "the same weather for " + _twinAgreed + " of the " + _twinChecked + " it checked; " + _forecastSeen +
                  " refresh(es) had been forecast for a ride and " + _cameTrue + " came true; the refresh time of " + known +
                  " of the " + cells + " weather cells on land is known"
                : "the forecast is off, because " + _why));
            lines.Add("  weather steps: your party " + PartySnowSaid + " for snow and " + PartyWetSaid +
                      " for wet ground; caravans and villagers keep " + Nightfall.Share(CaravanSnowShare) + " of their pace in snow (" +
                      _snowShares.Count + " read) and " + Nightfall.Share(CaravanWetShare) + " on wet ground (" + _wetShares.Count +
                      " read); " + _turned + " turn(s) of the weather over your party were seen");
            double hours = _costFrom < 0d ? 0d : CampaignTime.Now.ToHours - _costFrom;
            lines.Add("weather cost over the last " + Nightfall.Figure((float)hours) + " hour(s): the copy of the game's weather " +
                      "worked out " + _twinWorked + " cell(s) in " + Ms(_twinTicks) + " (slowest " + Ms(_twinSlowest) + "); " +
                      _walks + " ride time(s) walked their weather cells in " + Ms(_walkTicks) + " (slowest " + Ms(_walkSlowest) +
                      "), looking up " + _looks + " cell(s), " + _hits + " of them from memory, in " + Ms(_readTicks) +
                      "; listening to " + _listens + " of the game's weather refreshes took " + Ms(_listenTicks) +
                      ", reading the pace of your party " + _partyReads + " time(s) and of caravans and villagers " + _caravanReads +
                      " time(s); the busiest frame spent " + Ms(_busiestFrame) + " on the weather");
            Log.WriteMany(lines);
            ForgetTheDay();
            _costFrom = CampaignTime.Now.ToHours;
        }

        private static string Ms(long ticks) =>
            (ticks * 1000d / Stopwatch.Frequency).ToString("0.00", CultureInfo.InvariantCulture) + " ms";

        private static string Step(float value) =>
            (value >= 0f ? "+" : "") + value.ToString("0.000", CultureInfo.InvariantCulture);

        private static string Today()
        {
            CampaignTime now = CampaignTime.Now;
            CampaignTimeModel model = Campaign.Current?.Models?.CampaignTimeModel;
            int day = model == null ? (int)Math.Floor(now.ToDays)
                                    : (int)Math.Floor(now.ToDays) - (int)Math.Floor(model.CampaignStartTime.ToDays);
            return "day " + day.ToString(CultureInfo.InvariantCulture);
        }
    }

    [HarmonyPatch(typeof(DefaultMapWeatherModel), "UpdateWeatherForPosition")]
    internal static class Patch_WeatherCellRefreshed
    {
        private static void Prefix(DefaultMapWeatherModel __instance, CampaignVec2 position, CampaignTime ct, out Skies.Turn __state)
        {
            try { Skies.Before(__instance, position, ct, out __state); }
            catch (Exception e)
            {
                __state = default(Skies.Turn);
                Log.Error(e, "Skies.Before");
            }
        }

        private static void Postfix(DefaultMapWeatherModel __instance, CampaignVec2 position, CampaignTime ct,
                                    MapWeatherModel.WeatherEvent __result, Skies.Turn __state)
        {
            try { Skies.After(__instance, position, ct, __result, __state); }
            catch (Exception e) { Log.Error(e, "Skies.After"); }
        }
    }
}
