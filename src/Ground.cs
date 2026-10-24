using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TradeLord
{
    internal static class Ground
    {
        private const float SampleEvery = 0.5f;
        private const float StepLike = 0.5f;

        private static readonly Dictionary<long, byte> _seen = new Dictionary<long, byte>();
        private static readonly float[] _learned = Unlearned();
        private static readonly List<TradeMath.RoadPiece> _split = new List<TradeMath.RoadPiece>();
        private static readonly Dictionary<string, float[]> _cultures = new Dictionary<string, float[]>(StringComparer.Ordinal);
        private static int _lastGround = -1;
        private static float _lastRest = float.NaN;
        private static int _lastRoster = -1;
        private static int _lastMen = -1;
        private static bool _unreadable;
        private static int _looks, _hits;
        private static long _ticks;

        private static float[] Unlearned()
        {
            var steps = new float[TradeMath.Grounds];
            for (int g = 0; g < steps.Length; g++) steps[g] = float.NaN;
            return steps;
        }

        internal static void Forget()
        {
            _seen.Clear();
            for (int g = 0; g < _learned.Length; g++) _learned[g] = float.NaN;
            _cultures.Clear();
            _lastGround = -1;
            _lastRest = float.NaN;
            _lastRoster = -1;
            _lastMen = -1;
            _unreadable = false;
            ForgetTheDay();
        }

        internal static void ForgetTheDay()
        {
            _looks = 0;
            _hits = 0;
            _ticks = 0L;
        }

        internal static float[] PartySteps
        {
            get
            {
                var steps = new float[TradeMath.Grounds];
                for (int g = 0; g < steps.Length; g++) steps[g] = TradeMath.GroundStep(_learned, g);
                return steps;
            }
        }

        internal static float PartyStep(int ground) => TradeMath.GroundStep(_learned, ground);

        internal static string PartyStepSaid(int ground) =>
            PartyStep(ground).ToString("0.000", CultureInfo.InvariantCulture) +
            (float.IsNaN(_learned[ground]) ? " (the game's usual, not yet read)" : " (read)");

        internal static string Cost =>
            "sampling the ground took " + _looks + " look(s), " + _hits + " of them from memory, in " +
            (_ticks * 1000d / Stopwatch.Frequency).ToString("0.00", CultureInfo.InvariantCulture) + " ms";

        internal static void Split(Vec2 from, Vec2 to, List<TradeMath.RoadPiece> pieces)
        {
            if (_unreadable || pieces == null || pieces.Count == 0) return;
            float length = from.Distance(to);
            if (!(length > 0f)) return;
            long started = Stopwatch.GetTimestamp();
            Vec2 way = to - from;
            TradeMath.GroundsAlong(pieces, length, SampleEvery, t => At(from + way * t), _split);
            pieces.Clear();
            pieces.AddRange(_split);
            _ticks += Stopwatch.GetTimestamp() - started;
        }

        private static int At(Vec2 at)
        {
            long key = ((long)(int)Math.Floor(at.x / SampleEvery) << 32) | (uint)(int)Math.Floor(at.y / SampleEvery);
            _looks++;
            if (_seen.TryGetValue(key, out byte known))
            {
                _hits++;
                return known;
            }
            byte ground = TradeMath.OpenGround;
            try { ground = TradeMath.GroundOf(TerrainAt(at)); }
            catch (Exception e)
            {
                _unreadable = true;
                Log.Error(e, "reading the ground along a road (travel times leave the ground out)");
            }
            _seen[key] = ground;
            return ground;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int TerrainAt(Vec2 at)
        {
            var spot = new CampaignVec2(at, true);
            PathFaceRecord face = Campaign.Current.MapSceneWrapper.GetFaceIndex(in spot);
            return (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(face);
        }

        internal static int PartyOn(MobileParty party)
        {
            try { return party == null || party.IsCurrentlyAtSea ? TradeMath.OpenGround : PartyOnCore(party); }
            catch { return TradeMath.OpenGround; }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int PartyOnCore(MobileParty party) =>
            TradeMath.GroundOf((int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(party.CurrentNavigationFace));

        internal static void Learn(MobileParty party, int ground, float rest)
        {
            int roster = party?.ItemRoster?.VersionNo ?? -1;
            int men = party?.MemberRoster?.TotalManCount ?? -1;
            bool same = roster == _lastRoster && men == _lastMen;
            if (same && _lastGround >= 0 && ground != _lastGround && !float.IsNaN(_lastRest))
            {
                float delta = rest - _lastRest;
                float from = PartyStep(_lastGround);
                bool fromKnown = _lastGround != TradeMath.PlainOrSteppe || !float.IsNaN(_learned[_lastGround]);
                if (fromKnown && Math.Abs(delta) <= StepLike && ground != TradeMath.OpenGround)
                {
                    float step = from + delta;
                    if (Math.Abs(step - PartyStep(ground)) > 0.0005f || float.IsNaN(_learned[ground]))
                    {
                        _learned[ground] = step;
                        if (Options.Current.ExtendedDebugLogging)
                            Log.Write("ground step read: your party's pace moved by " +
                                      delta.ToString("0.000", CultureInfo.InvariantCulture) + " stepping from " +
                                      Named(_lastGround) + " onto " + Named(ground) + ", so its " + Named(ground) +
                                      " step is " + step.ToString("0.000", CultureInfo.InvariantCulture));
                    }
                }
            }
            _lastGround = ground;
            _lastRest = rest;
            _lastRoster = roster;
            _lastMen = men;
        }

        internal static string Named(int ground)
        {
            switch (ground)
            {
                case TradeMath.PlainOrSteppe: return "plain or steppe";
                case TradeMath.ForestGround: return "forest";
                case TradeMath.WaterGround: return "water";
                case TradeMath.DesertGround: return "desert";
                default: return "open ground";
            }
        }

        internal static float[] CaravanShares(MobileParty party)
        {
            string culture = party?.Party?.Culture?.StringId ?? "";
            if (_cultures.TryGetValue(culture, out float[] known)) return known;
            float[] steps = (float[])TradeMath.GameGroundSteps.Clone();
            try
            {
                if (HasFeat(party, DefaultCulturalFeats.BattanianForestSpeedFeat))
                    steps[TradeMath.ForestGround] += DefaultCulturalFeats.BattanianForestSpeedFeat.EffectBonus * 0.2f;
                if (HasFeat(party, DefaultCulturalFeats.AseraiDesertFeat))
                    steps[TradeMath.DesertGround] = 0f;
            }
            catch { steps = (float[])TradeMath.GameGroundSteps.Clone(); }
            float[] shares = TradeMath.GroundSharesFromSteps(steps);
            _cultures[culture] = shares;
            return shares;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool HasFeat(MobileParty party, FeatObject feat) =>
            party?.Party != null && feat != null && PartyBaseHelper.HasFeat(party.Party, feat);
    }
}
