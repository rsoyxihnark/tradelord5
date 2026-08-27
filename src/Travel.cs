using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TaleWorlds.CampaignSystem;
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
            return (sum / ships.Count + min) * 0.5f;
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

        private static void Speeds(out float land, out float sea)
        {
            land = MobileParty.MainParty.Speed;
            if (land <= 0.01f) land = 5f;
            sea = SeaSpeed();
            if (sea <= 0.01f) sea = land;
        }

        internal static float Days(float distance, float landRatio)
        {
            if (distance <= 0f) return 0f;
            Speeds(out float land, out float sea);
            float landDist = distance * landRatio;
            float seaDist = distance * (1f - landRatio);
            return (landDist / land + seaDist / sea) / 24f;
        }

        private static readonly Dictionary<string, (float dist, float landRatio)> _partyDist
            = new Dictionary<string, (float, float)>();
        private static Vec2 _partyAt;
        private static int _partyHour = -1;
        private static readonly Dictionary<(string, string), (float dist, float landRatio)> _pairDist
            = new Dictionary<(string, string), (float, float)>();
        private static bool _cachedNaval;

        private static void DropIfNavalChanged()
        {
            bool naval = NavalActive;
            if (naval == _cachedNaval) return;
            _cachedNaval = naval;
            _partyDist.Clear();
            _pairDist.Clear();
        }

        internal static void Forget()
        {
            _partyDist.Clear();
            _partyHour = -1;
            _pairDist.Clear();
            _cachedNaval = false;
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
            Speeds(out float land, out float sea);
            return distance / (Math.Max(land, sea) * 24f);
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
            }
            if (!_partyDist.TryGetValue(target.StringId, out var hit))
            {
                float dist = FromParty(target, out float landRatio);
                hit = (dist, landRatio);
                _partyDist[target.StringId] = hit;
            }
            return Days(hit.dist, hit.landRatio);
        }

        internal static float EstimateDaysBetween(Settlement a, Settlement b)
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

            return Days(hit.dist, hit.landRatio);
        }

        internal static string EstimateLabel(Settlement target)
        {
            if (target == null || target == Settlement.CurrentSettlement) return "";
            float days = EstimateDaysFromParty(target);
            if (days < 0.05f) return "";
            TextObject label = new TextObject("{=TL79}~{DAYS} days");
            label.SetTextVariable("DAYS", days.ToString("0.#"));
            return label.ToString();
        }
    }
}
