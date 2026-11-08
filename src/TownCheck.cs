using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TradeLord
{
    internal static class TownCheck
    {
        private sealed class Heard
        {
            internal Town Town;
            internal string Name;
            internal object SoldBefore;
            internal Forecast.TownTickSaid Said;
            internal readonly Dictionary<string, int> Made = new Dictionary<string, int>(StringComparer.Ordinal);
            internal readonly Dictionary<string, int> Taken = new Dictionary<string, int>(StringComparer.Ordinal);
        }

        private const int UnitsWorthNaming = 5;
        private const float AShiftThatCounts = 0.01f;
        private const int MostGoodsNamed = 4;

        private static readonly Dictionary<string, Heard> _heard = new Dictionary<string, Heard>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> _names = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly List<float> _came = new List<float>();
        private static readonly List<float> _cameStepped = new List<float>();
        private static readonly int[] _runs = new int[WorkshopRuns.StopsKnown];
        private static int _ticks, _afterTheGame, _besieged, _goods, _usedMore, _usedUnsaid, _worstOff;
        private static long _saidUnits, _steppedUnits, _usedUnits;
        private static string _worst;
        private static int _shiftsSeen, _shiftsMissed;
        private static double _shiftNowOff, _shiftSteppedOff, _shiftGame;
        private static long _madeSaid, _madeGame, _takenSaid, _takenGame;
        private static int _shopGoodsOff, _yoursAside;
        private static double _ms, _msMost;

        internal static bool Writing => Options.Current.ExtendedDebugLogging;

        internal static void Forget()
        {
            _heard.Clear();
            _names.Clear();
            ClearTheDay();
        }

        internal static void ATownIsAboutToTick(Town town)
        {
            if (!Writing || !Forecast.On || town?.Settlement?.StringId == null) return;
            if (town.IsUnderSiege)
            {
                _besieged++;
                return;
            }
            Stopwatch watch = Stopwatch.StartNew();
            Forecast.TownTickSaid said = Forecast.WhatATownTickShouldDo(town);
            watch.Stop();
            double ms = watch.Elapsed.TotalMilliseconds;
            _ms += ms;
            if (ms > _msMost) _msMost = ms;
            if (said == null) return;
            foreach (var one in said.Names) _names[one.Key] = one.Value;
            _heard[town.Settlement.StringId] = new Heard
            {
                Town = town,
                Name = Tongue.Named(town.Settlement.Name, town.Settlement.StringId),
                SoldBefore = town.SoldItems,
                Said = said
            };
        }

        internal static void ItemMade(ItemObject item, Settlement at, int count) => Tally(item, at, count, true);

        internal static void ItemUsed(ItemObject item, Settlement at, int count) => Tally(item, at, count, false);

        private static void Tally(ItemObject item, Settlement at, int count, bool made)
        {
            if (_heard.Count == 0 || item?.ItemCategory?.StringId == null || at?.StringId == null || count <= 0) return;
            if (!_heard.TryGetValue(at.StringId, out Heard one)) return;
            string kind = item.ItemCategory.StringId;
            if (!_names.ContainsKey(kind)) _names[kind] = Tongue.Named(item.ItemCategory.GetName(), kind);
            Dictionary<string, int> into = made ? one.Made : one.Taken;
            into.TryGetValue(kind, out int had);
            into[kind] = had + count;
        }

        internal static void Tick()
        {
            if (_heard.Count == 0) return;
            var lines = new List<string>();
            foreach (Heard one in _heard.Values) Judge(one, lines);
            _heard.Clear();
            if (lines.Count > 0) Log.WriteMany(lines);
        }

        private static void Judge(Heard one, List<string> lines)
        {
            Town town = one.Town;
            if (town?.MarketData == null || ReferenceEquals(town.SoldItems, one.SoldBefore))
            {
                _afterTheGame++;
                return;
            }
            _ticks++;
            var used = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (Town.SellLog sold in town.SoldItems)
            {
                string id = sold.Category?.StringId;
                if (id == null) continue;
                used.TryGetValue(id, out int had);
                used[id] = had + sold.Number;
            }
            var off = new List<(int gap, string said)>();
            int saidHere = 0, steppedHere = 0, usedHere = 0;
            foreach (var (kind, _, afterShops, said, stepped) in one.Said.Uses)
            {
                used.TryGetValue(kind.StringId, out int took);
                _goods++;
                saidHere += said;
                steppedHere += stepped;
                usedHere += took;
                if (took > said) _usedMore++;
                if (said > 0) _came.Add(TradeMath.UsedAgainstSaid(said, took));
                else if (took > 0) _usedUnsaid++;
                if (stepped > 0) _cameStepped.Add(TradeMath.UsedAgainstSaid(stepped, took));
                int gap = Math.Abs(took - said);
                string name = Tongue.Named(kind.GetName(), kind.StringId);
                if (gap > _worstOff)
                {
                    _worstOff = gap;
                    _worst = name + " at " + one.Name + ", said " + said + " and used " + took;
                }
                if (gap >= UnitsWorthNaming && gap * 2 >= Math.Max(took, said))
                    off.Add((gap, name + " said " + said + " (" + stepped + " stepped) of " + afterShops +
                                  " on the shelf after the workshops, used " + took));
            }
            _saidUnits += saidHere;
            _steppedUnits += steppedHere;
            _usedUnits += usedHere;

            foreach (var (kind, before, now, stepped) in one.Said.Shifts)
            {
                float game = TradeMath.Finite(town.MarketData.GetCategoryData(kind).Demand - before, 0f);
                _shiftsSeen++;
                _shiftNowOff += Math.Abs(now - game);
                _shiftSteppedOff += Math.Abs(stepped - game);
                _shiftGame += Math.Abs(game);
                if (Math.Abs(game) > AShiftThatCounts && Math.Abs(now) <= AShiftThatCounts) _shiftsMissed++;
            }

            var shopsOff = new List<string>();
            var kinds = new List<string>();
            if (one.Said.Yours) _yoursAside++;
            foreach (string id in one.Said.Made.Keys) if (!kinds.Contains(id)) kinds.Add(id);
            foreach (string id in one.Said.Taken.Keys) if (!kinds.Contains(id)) kinds.Add(id);
            foreach (string id in one.Made.Keys) if (!kinds.Contains(id)) kinds.Add(id);
            foreach (string id in one.Taken.Keys) if (!kinds.Contains(id)) kinds.Add(id);
            foreach (string id in one.Said.Yours ? new List<string>() : kinds)
            {
                one.Said.Made.TryGetValue(id, out int madeSaid);
                one.Said.Taken.TryGetValue(id, out int takenSaid);
                one.Made.TryGetValue(id, out int madeGame);
                one.Taken.TryGetValue(id, out int takenGame);
                _madeSaid += madeSaid;
                _takenSaid += takenSaid;
                _madeGame += madeGame;
                _takenGame += takenGame;
                if (madeSaid == madeGame && takenSaid == takenGame) continue;
                _shopGoodsOff++;
                shopsOff.Add(NameOf(id) + " made " + madeSaid + " said and " + madeGame + " by the game, taken " +
                             takenSaid + " said and " + takenGame + " by the game");
            }
            for (int r = 0; !one.Said.Yours && r < _runs.Length && r < one.Said.Runs.Length; r++) _runs[r] += one.Said.Runs[r];

            if (off.Count == 0 && shopsOff.Count == 0) return;
            off.Sort((x, y) => y.gap.CompareTo(x.gap));
            var named = new List<string>();
            for (int i = 0; i < off.Count && i < MostGoodsNamed; i++) named.Add(off[i].said);
            lines.Add("town tick check at " + one.Name + ": " + one.Said.Uses.Count + " good(s) it had a figure for, " +
                      "the town used " + usedHere + " of the " + saidHere + " unit(s) TradeLord said (" + steppedHere +
                      " with the shift stepped as the game does)" +
                      (named.Count == 0 ? "" : "; furthest off: " + string.Join("; ", named.ToArray())) +
                      (shopsOff.Count == 0 ? "" : "; workshops off: " + string.Join("; ", shopsOff.ToArray())));
        }

        private static string NameOf(string id) => _names.TryGetValue(id, out string name) ? name : id;

        internal static void SayTheDay()
        {
            List<(string town, float off)> first = Forecast.TheFirstTicksSeen();
            if (!Writing)
            {
                ClearTheDay();
                return;
            }
            if (first.Count > 0)
            {
                var said = new List<string>();
                foreach (var (town, off) in first)
                    said.Add(town + (float.IsNaN(off)
                                ? " (no workshop reading, so taken as " +
                                  Signed(TradeMath.HoursTheWorkshopReadingWasOff(TradeMath.TickTimeUnknown)) + " h off)"
                                : " (" + Signed(off) + " h off the workshop reading)"));
                Log.Write("town tick check: first tick seen at " + string.Join(", ", said.ToArray()));
            }
            if (_ticks + _afterTheGame + _besieged == 0)
            {
                ClearTheDay();
                return;
            }
            _came.Sort();
            _cameStepped.Sort();
            int known = Forecast.TownsWhoseHourIsKnown(out int towns);
            Log.Write("town tick check today: " + _ticks + " town tick(s) followed" +
                      (_afterTheGame == 0 ? "" : ", " + _afterTheGame + " set aside as heard after the game's own tick") +
                      (_besieged == 0 ? "" : ", " + _besieged + " set aside as under siege") +
                      "; town use: TradeLord said " + _saidUnits + " unit(s), and " + _steppedUnits +
                      " with the shift stepped as the game does, the towns used " + _usedUnits + ", over " + _goods +
                      " good(s), " + _usedMore + " of them more than said and " + _usedUnsaid +
                      " used where TradeLord said none; used against said, median " + Ratio(_came) + " (" +
                      Ratio(_cameStepped) + " stepped)" + (_worst == null ? "" : "; furthest off: " + _worst) +
                      "; substitute shifts at " + _shiftsSeen + " good(s): TradeLord's shift was off the game's by " +
                      Figure(_shiftNowOff) + " in all and the stepped shift by " + Figure(_shiftSteppedOff) +
                      ", against " + Figure(_shiftGame) + " the game moved, and the game moved " + _shiftsMissed +
                      " where TradeLord counted none; workshops: TradeLord said " + _madeSaid + " unit(s) made and " +
                      _takenSaid + " taken, the game made " + _madeGame + " and took " + _takenGame + ", " +
                      _shopGoodsOff + " good(s) off, leaving out " + _yoursAside +
                      " town(s) with a workshop of yours, as the game counts its warehouse there too; TradeLord's replay ran " +
                      _runs[WorkshopRuns.Ran] +
                      " time(s) and held back " + _runs[WorkshopRuns.NoInputs] + " for no inputs, " +
                      _runs[WorkshopRuns.NotWorthIt] + " as not worth it, " + _runs[WorkshopRuns.PurseShort] +
                      " for the town's purse and " + _runs[WorkshopRuns.CapitalShort] +
                      " for capital; the hour a town uses its goods is known from its own tick for " + known + " of " +
                      towns + " towns; " + Figure(_ms) + " ms in all, " + Figure(_msMost) + " ms at most");
            ClearTheDay();
        }

        private static void ClearTheDay()
        {
            _came.Clear();
            _cameStepped.Clear();
            Array.Clear(_runs, 0, _runs.Length);
            _ticks = _afterTheGame = _besieged = _goods = _usedMore = _usedUnsaid = _worstOff = 0;
            _saidUnits = _steppedUnits = _usedUnits = 0L;
            _worst = null;
            _shiftsSeen = _shiftsMissed = 0;
            _shiftNowOff = _shiftSteppedOff = _shiftGame = 0d;
            _madeSaid = _madeGame = _takenSaid = _takenGame = 0L;
            _shopGoodsOff = _yoursAside = 0;
            _ms = _msMost = 0d;
        }

        private static string Ratio(List<float> sorted) =>
            sorted.Count == 0 ? "unknown" : TradeMath.Quantile(sorted, 0.5f).ToString("0.00", CultureInfo.InvariantCulture);

        private static string Figure(double value) => value.ToString("0.#", CultureInfo.InvariantCulture);

        private static string Signed(float hours) =>
            (hours > 0f ? "+" : "") + hours.ToString("0.#", CultureInfo.InvariantCulture);
    }
}
