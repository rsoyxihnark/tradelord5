using System;
using System.Collections.Generic;
using System.Globalization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TradeLord
{
    internal static class Hindsight
    {
        private const int NoWorth = -1;

        private const int Most = 600;

        private struct Said
        {
            internal ItemObject Item;
            internal float AtHours;
            internal float WithinDays;
            internal int StockThen;
            internal int StockSaid;
            internal int WorthThen;
            internal int WorthSaid;
        }

        private static readonly Dictionary<string, Dictionary<string, Said>> _said =
            new Dictionary<string, Dictionary<string, Said>>(StringComparer.Ordinal);

        private static int _held;

        internal static bool On => Options.Current.ForecastScore && Forecast.On;

        internal static void Forget()
        {
            _said.Clear();
            _held = 0;
        }

        internal static void Note(TradeRoute route)
        {
            if (!On || route == null || route.Item == null) return;
            Guard.Run("Hindsight.Note", () =>
            {
                Noted(route.From, route.Item, Travel.EstimateDaysFromParty(route.From));
                Noted(route.To, route.Item, route.TravelDays);
            });
        }

        internal static void Score(Settlement site)
        {
            if (!On || site == null) return;
            Guard.Run("Hindsight.Score", () => Written(site));
        }

        private static void Noted(Settlement site, ItemObject item, float withinDays)
        {
            if (site == null) return;
            int stockSaid = Forecast.UnitsLanding(site, item, withinDays);
            int worthSaid = Forecast.WorthShift(site, item, withinDays);
            if (stockSaid == 0 && worthSaid == 0) return;
            if (!_said.TryGetValue(site.StringId, out Dictionary<string, Said> here))
            {
                here = new Dictionary<string, Said>(StringComparer.Ordinal);
                _said[site.StringId] = here;
            }
            if (!here.ContainsKey(item.StringId))
            {
                if (_held >= Most)
                {
                    Log.Repeatable("forecast check", "full",
                                   "forecast check is holding the " + Most + " figures it keeps at once, so newer " +
                                   "ones are passed over until a market it has a figure for is walked into");
                    return;
                }
                _held++;
            }
            here[item.StringId] = new Said
            {
                Item = item,
                AtHours = (float)CampaignTime.Now.ToHours,
                WithinDays = withinDays,
                StockThen = LedgerBehavior.StockOf(site, item),
                StockSaid = stockSaid,
                WorthThen = WorthOnTheShelf(site, item),
                WorthSaid = worthSaid
            };
        }

        private static void Written(Settlement site)
        {
            if (!_said.TryGetValue(site.StringId, out Dictionary<string, Said> here)) return;
            _said.Remove(site.StringId);
            _held -= here.Count;
            if (_held < 0) _held = 0;
            float now = (float)CampaignTime.Now.ToHours;
            var lines = new List<string>();
            int scored = 0, landingMiss = 0, shared = 0;
            float shareTotal = 0f;
            foreach (Said kept in here.Values)
            {
                if (kept.Item == null) continue;
                scored++;
                int landed = TradeMath.MissedBy(kept.StockThen, LedgerBehavior.StockOf(site, kept.Item));
                int landingOff = TradeMath.MissedBy(kept.StockSaid, landed);
                landingMiss += Math.Abs(landingOff);
                string line = "  " + Named(kept.Item) + ": said " + kept.StockSaid + " unit(s) would land within " +
                              Figure(kept.WithinDays) + " day(s) and " + landed + " did, " + Counted(landingOff) +
                              "; you walked in " + Figure(TradeMath.DaysSince(kept.AtHours, now)) +
                              " day(s) after it said so";
                int worthNow = WorthOnTheShelf(site, kept.Item);
                if (kept.WorthThen == NoWorth || worthNow == NoWorth)
                {
                    lines.Add(line + "; no worth is kept for a kind of good here, which only a town does");
                    continue;
                }
                int moved = TradeMath.MissedBy(kept.WorthThen, worthNow);
                float share = TradeMath.OffByShare(kept.WorthSaid, moved);
                if (share != TradeMath.NoShareToGive)
                {
                    shared++;
                    shareTotal += share;
                }
                lines.Add(line + "; said its kind would move " + kept.WorthSaid + " in worth and it moved " +
                          moved + ", " + Counted(TradeMath.MissedBy(kept.WorthSaid, moved)) + Shared(share));
            }
            if (scored == 0) return;
            Log.Write("forecast check at " + site.Name + ", " + scored + " good(s) it had a figure for:");
            for (int i = 0; i < lines.Count; i++) Log.Write(lines[i]);
            Log.Write("  in all: the landing figure was off by " +
                      Figure(TradeMath.MeanOf(landingMiss, scored)) + " unit(s) a good" +
                      (shared == 0
                          ? ", and no worth figure could be held to anything here"
                          : ", the worth figure by " + Share(TradeMath.MeanOf(shareTotal, shared)) +
                            " of what it said would move, over " + shared + " good(s)"));
        }

        private static int WorthOnTheShelf(Settlement site, ItemObject item)
        {
            Town town = site != null && site.IsTown ? site.Town : null;
            if (town == null || item == null || item.ItemCategory == null) return NoWorth;
            return Guard.Read("Hindsight.Worth", town,
                              where => where.MarketData.GetCategoryData(item.ItemCategory).InStoreValue,
                              NoWorth);
        }

        private static string Named(ItemObject item) =>
            item.Name == null ? item.StringId : item.Name.ToString();

        private static string Counted(int off) =>
            off == 0 ? "exactly what it said"
                     : (off > 0 ? off + " more than it said" : -off + " fewer than it said");

        private static string Shared(float share) =>
            share == TradeMath.NoShareToGive ? "" : ", " + Share(share) + " off";

        private static string Share(float share) =>
            (share * 100f).ToString("0", CultureInfo.InvariantCulture) + "%";

        private static string Figure(float number) =>
            number.ToString("0.0", CultureInfo.InvariantCulture);
    }
}
