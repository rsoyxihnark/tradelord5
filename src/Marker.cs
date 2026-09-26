using System.Collections.Generic;
using System.Globalization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TradeLord
{
    internal sealed class Paying
    {
        private readonly List<int> _rungs = new List<int>();
        private readonly Settlement _site;
        private readonly SettlementComponent _market;
        private readonly EquipmentElement _el;
        private readonly MobileParty _party;
        private readonly float _ride;
        private Ladder _walk;
        private int _flat;
        private int _landed;
        private bool _asked;

        internal Paying(Settlement site, SettlementComponent market, EquipmentElement el,
                        MobileParty party, float ride)
        {
            _site = site;
            _market = market;
            _el = el;
            _party = party;
            _ride = ride;
        }

        internal int Today => _flat;

        internal bool OnItsWay => _landed != 0;

        internal int At(int taken)
        {
            while (_rungs.Count <= taken) _rungs.Add(Next());
            return _rungs[taken];
        }

        private int Next()
        {
            if (!_asked)
            {
                _asked = true;
                _flat = Priced.At(_market, _el, _party, true);
                int landed = _el.Item == null || _flat <= 0
                    ? 0 : Forecast.WorthShiftAsItHasHeld(_site, _el.Item, _ride);
                _landed = landed;
                Ladder walk = _el.Item == null || _flat <= 0
                    ? null : Bulk.AsItLands(_site, _el, true, _flat, landed);
                _walk = walk != null && (walk.Walkable || landed != 0) ? walk : null;
            }
            return _walk != null ? _walk.At(_rungs.Count) : _flat;
        }
    }

    internal static class Marker
    {
        private static Settlement _tracked;

        private static Settlement _picked;

        private static readonly HashSet<string> _owedAFairLook = new HashSet<string>(System.StringComparer.Ordinal);

        private static string _walkedInto;
        private static string _markedOnTheWayIn;

        private static int _hour = -1;
        private static Vec2 _at;
        private const float MovedFar = 100f;

        private static Stamp _cargoStamp;
        private static int _cargoVersion = -1;
        private static List<(EquipmentElement item, int amount, int worth)> _cargo;
        private static List<(string good, int amount, bool food)> _cargoHeld;

        private static string _markedId;
        private static long _markedValue;
        private static long _markedEarned;
        private static int _markedUnits;
        private static float _markedRate;
        private static float _markedDays;
        private static int _markedAt = -1;
        private static List<(string good, int amount, bool food)> _markedCargo;
        private static long _lastValue;
        private static int _lastAt = -1;

        private static long _saidValue = -1L;
        private static float _saidRate = -1f;
        private static long _toldValue = -1L;
        private static int _toldUnits = -1;
        private static bool _toldHeld;
        private static string _toldNext;
        private static readonly Dictionary<string, (string name, int units, long value)> _toldBoard =
            new Dictionary<string, (string name, int units, long value)>(System.StringComparer.Ordinal);

        internal static Settlement Tracked
        {
            get => _tracked;
            set
            {
                _tracked = value;
                if (value != null) _picked = value;
            }
        }

        internal static bool DueAgain(Vec2 at)
        {
            int hour = (int)CampaignTime.Now.ToHours;
            if (hour == _hour && at.DistanceSquared(_at) <= MovedFar) return false;
            _hour = hour;
            _at = at;
            return true;
        }

        internal static void ForgetWhatYouCarry()
        {
            _cargo = null;
            _cargoStamp.Stale();
            _cargoVersion = -1;
            _cargoHeld = null;
        }

        internal static void ForgetTheRead()
        {
            ForgetWhatYouCarry();
        }

        internal static void Forget()
        {
            ForgetTheRead();
            _tracked = null;
            _picked = null;
            _owedAFairLook.Clear();
            _walkedInto = null;
            _markedOnTheWayIn = null;
            _hour = -1;
            _markedId = null;
            _markedValue = 0L;
            _markedEarned = 0L;
            _markedUnits = 0;
            _markedRate = 0f;
            _markedDays = 0f;
            _markedAt = -1;
            _markedCargo = null;
            _lastValue = 0L;
            _lastAt = -1;
            _saidValue = -1L;
            _saidRate = -1f;
            _toldValue = -1L;
            _toldUnits = -1;
            _toldHeld = false;
            _toldNext = null;
            _toldBoard.Clear();
        }

        private struct Share
        {
            internal string Good;
            internal int Amount;
            internal int Moved;
            internal int Price;
            internal int Last;
            internal int Today;
            internal int Paid;
            internal long Fetched;
        }

        private struct Weighing
        {
            internal Settlement Where;
            internal float Days;
            internal int Units;
            internal long Value;
            internal long Earned;
            internal float Rate;
        }

        private struct Takings
        {
            internal long Value;
            internal long Cost;
            internal int Units;
            internal int Kinds;
            internal bool PurseCapped;
        }

        private struct Reckoning
        {
            internal Settlement Best;
            internal Settlement RunnerUp;
            internal bool Held;
            internal long Value;
            internal long RunnerUpValue;
            internal long Cost;
            internal float Rate;
            internal float RunnerUpRate;
            internal float Days;
            internal float CeilingPassed;
            internal int Units;
            internal int Kinds;
            internal int Purse;
            internal int Carried;
            internal int Weighed;
            internal int Refused;
            internal bool PurseCapped;
            internal int Told;
            internal int Left;
            internal int NoTill;
            internal int PastCeiling;
            internal int NoRoad;
            internal int Shut;
            internal int AtWar;
            internal Settlement CameBackTo;
            internal Settlement Afresh;
            internal Settlement Unheld;
            internal List<string> Compared;
            internal double Took;
            internal List<Share> Bill;
            internal List<Weighing> Board;
        }

        internal static void Update()
        {
            VisualTrackerManager tracker = Campaign.Current?.VisualTrackerManager;
            if (tracker == null) return;
            Settlement target = null;
            Reckoning how = default(Reckoning);
            bool on = Options.Current.MarkBestSellTownOnMap;
            long started = System.Diagnostics.Stopwatch.GetTimestamp();
            if (on) target = TheMarkFairlyWeighed(out how);
            how.Took = (System.Diagnostics.Stopwatch.GetTimestamp() - started) * 1000d /
                       System.Diagnostics.Stopwatch.Frequency;

            if (target == _picked)
            {
                if (target != null && !tracker.CheckTracked(target))
                {
                    tracker.RegisterObject(target);
                    _tracked = target;
                }
                Marks.OweAFairLook(_owedAFairLook, how.Compared, MobileParty.MainParty?.CurrentSettlement?.StringId,
                                   target == null ? null : TradeActionBehavior.MarketTheMarkerLeavesOut(), target == null);
                if (target != null) SayItWeighedAgain(target, how);
                return;
            }
            if (_tracked != null && !LedgerPanel.IsPinned(_tracked) && tracker.CheckTracked(_tracked))
                tracker.RemoveTrackedObject(_tracked);
            _tracked = null;
            _picked = target;
            Marks.OweAFairLook(_owedAFairLook, how.Compared, MobileParty.MainParty?.CurrentSettlement?.StringId,
                               target == null ? null : TradeActionBehavior.MarketTheMarkerLeavesOut(), true);
            if (target != null && !tracker.CheckTracked(target))
            {
                tracker.RegisterObject(target);
                _tracked = target;
            }
            string why = on ? Why(how) : "the map marker is switched off";
            Log.Write(target != null
                ? "map marker moved to " + target.Name + ": " + why
                : "map marker taken off the map: " + why);
            Ultra(how);
            Told(how);
            Remember(target, how);
        }

        private static List<(string where, int units, long value)> OnTheBoard(in Reckoning how)
        {
            if (how.Board == null) return null;
            var board = new List<(string where, int units, long value)>(how.Board.Count);
            for (int i = 0; i < how.Board.Count; i++)
            {
                Weighing one = how.Board[i];
                if (one.Where != null) board.Add((one.Where.StringId, one.Units, one.Value));
            }
            return board;
        }

        private static void Told(in Reckoning how)
        {
            _toldValue = how.Board == null ? -1L : how.Value;
            _toldUnits = how.Units;
            _toldHeld = how.Held;
            _toldNext = how.RunnerUp?.StringId;
            _toldBoard.Clear();
            for (int i = 0; how.Board != null && i < how.Board.Count; i++)
            {
                Weighing one = how.Board[i];
                if (one.Where != null)
                    _toldBoard[one.Where.StringId] =
                        (Tongue.Named(one.Where.Name, one.Where.StringId), one.Units, one.Value);
            }
        }

        private static void SayItWeighedAgain(Settlement target, in Reckoning how)
        {
            if (!Options.Current.ExtendedDebugLogging) return;
            if (Marks.WorthSayingAgain(how.Value, how.Units, how.Held, _toldValue, _toldUnits, _toldHeld))
            {
                Log.Write("map marker weighed your cargo again and stayed on " + target.Name + ": " + Why(how));
                Ultra(how);
                Told(how);
            }
            else if (SayWhatElseMoved(target, how)) Told(how);
            if (how.Value == _saidValue && how.Rate == _saidRate) return;
            Remember(target, how);
        }

        private static bool SayWhatElseMoved(Settlement target, in Reckoning how)
        {
            if (how.Board == null) return false;
            how.Board.Sort(FastestFirst);
            var (joined, changed, gone) = Marks.WhatMovedOnTheBoard(OnTheBoard(how), _toldBoard);
            bool nextMoved = !string.Equals(how.RunnerUp?.StringId, _toldNext, System.StringComparison.Ordinal);
            if (joined.Count == 0 && changed.Count == 0 && gone.Count == 0 && !nextMoved) return false;
            var said = new List<string>();
            for (int i = 0; i < how.Board.Count; i++)
            {
                Weighing one = how.Board[i];
                if (one.Where == null) continue;
                string id = one.Where.StringId;
                bool fresh = joined.Contains(id);
                if (!fresh && !changed.Contains(id)) continue;
                string row = Tongue.Named(one.Where.Name, id) + (fresh ? " newly priced, " : " now ") + one.Units +
                             " unit(s) for " + one.Value + " gold at " + one.Rate.ToString("0") + " gold a day";
                if (!fresh) row += ", was " + _toldBoard[id].units + " unit(s) for " + _toldBoard[id].value + " gold";
                said.Add(row);
            }
            for (int i = 0; i < gone.Count; i++)
                said.Add(_toldBoard[gone[i]].name + " no longer priced, was " + _toldBoard[gone[i]].units +
                         " unit(s) for " + _toldBoard[gone[i]].value + " gold");
            if (nextMoved)
                said.Add(how.RunnerUp == null
                    ? "no other market it priced would take any of it now"
                    : "the next best is now " + how.RunnerUp.Name + " at " + how.RunnerUpRate.ToString("0") +
                      " gold a day for " + how.RunnerUpValue + " gold");
            Log.Write("map marker weighed your cargo again and stayed on " + target.Name + ", still " + how.Units +
                      " unit(s) for " + how.Value + " gold, now " + how.Rate.ToString("0") +
                      " gold a day, and only other markets moved: " + string.Join("; ", said));
            return true;
        }

        private static void Remember(Settlement target, in Reckoning how)
        {
            _saidValue = how.Value;
            _saidRate = how.Rate;
            string lookingAt = target == null ? null : target.StringId;
            _lastValue = target == null ? 0L : how.Value;
            _lastAt = Freshness.Hour;
            if (Marks.FirstLookStands(_markedId, lookingAt, _markedCargo, _cargoHeld, _markedValue)) return;
            _markedCargo = _cargoHeld;
            _markedId = lookingAt;
            _markedValue = target == null ? 0L : how.Value;
            _markedEarned = target == null ? 0L : how.Value - how.Cost;
            _markedUnits = how.Units;
            _markedRate = how.Rate;
            _markedDays = how.Days;
            _markedAt = Freshness.Hour;
        }

        internal static void ScoreTheMark(Settlement site, int units, int gold)
        {
            if (!Options.Current.ExtendedDebugLogging) return;
            if (site == null || _markedId == null || site.StringId != _markedId) return;
            if (_markedValue <= 0L) return;
            long said = _markedValue;
            long last = _lastValue;
            int lastAt = _lastAt;
            _markedValue = 0L;
            _lastValue = 0L;
            float since = TradeMath.DaysSince(_markedAt, Freshness.Hour);
            if (Scoring.TooSoonToSay(_markedDays, since))
            {
                Log.Write("marker check at " + site.Name + ": it marked this market about " +
                          _markedDays.ToString("0.#", CultureInfo.InvariantCulture) + " day(s) away and you walked in " +
                          Scoring.Figure(since) + " day(s) later, too soon to say anything about what it marked on");
                return;
            }
            float held = TradeMath.HeldShare(said > int.MaxValue ? int.MaxValue : (int)said, gold);
            float heldLast = TradeMath.HeldShare(last > int.MaxValue ? int.MaxValue : (int)last, gold);
            Log.Write("marker check at " + site.Name + ": it marked this market for " + _markedUnits +
                      " unit(s) worth " + said + " gold, " + _markedEarned + " of it profit, " +
                      _markedRate.ToString("0") +
                      " gold a day, about " + _markedDays.ToString("0.#", CultureInfo.InvariantCulture) + " day(s) away; you walked in " +
                      Scoring.Figure(since) + " day(s) later and sold " + units + " unit(s) for " + gold +
                      (held == TradeMath.NoShareToGive
                          ? ""
                          : ", " + Scoring.Share(held) + " of what it marked on") +
                      (last == said || heldLast == TradeMath.NoShareToGive
                          ? ""
                          : "; its last look, " + Scoring.Figure(TradeMath.DaysSince(lastAt, Freshness.Hour)) +
                            " day(s) before you walked in, came to " + last + " gold, and the sale was " +
                            Scoring.Share(heldLast) + " of that"));
        }

        private const string TheMarketLeftAlone =
            "the market TradeLord last traded at, until you come back to it";

        private static string Why(in Reckoning how)
        {
            if (how.Carried == 0) return "nothing in your cargo is yours to sell";
            if (how.Best == null)
                return "of the " + how.Weighed + " market(s) it looked at, " + how.Refused +
                       " would pay too little for any of the " + how.Carried + " good(s) you carry to " +
                       "clear Minimum profit margin, " + how.NoRoad + " have no road it could find, " +
                       how.PastCeiling + " are past your travel ceilings and " + how.NoTill +
                       " have no gold at all" +
                       (how.CameBackTo != null
                           ? ", and it leaves out " + how.CameBackTo.Name + ", " + TheMarketLeftAlone
                           : "");
            return how.Kinds + " of the " + how.Carried + " good(s) you carry clear Minimum profit " +
                   "margin there, " + how.Units + " unit(s) for " + how.Value + " gold, " +
                   (how.Value - how.Cost) + " of it profit" +
                   (how.PurseCapped
                       ? ", which is all that town's purse of " + how.Purse + " can take"
                       : " against a town purse of " + how.Purse) +
                   ", about " + how.Days.ToString("0.#", CultureInfo.InvariantCulture) + " day(s) away" +
                   (how.CeilingPassed > 0f
                       ? ", past your travel ceiling of " +
                         how.CeilingPassed.ToString("0.#", CultureInfo.InvariantCulture) +
                         " day(s) (the market already marked may pass it by " +
                         ((TradeMath.TheMarkedTownHoldsBy - 1f) * 100f).ToString("0", CultureInfo.InvariantCulture) +
                         "%)"
                       : "") +
                   ", so " + how.Rate.ToString("0") + " gold a day" + TheNextBest(how) +
                   (how.Afresh != null && how.Unheld != null
                       ? ", marked afresh because " + how.Afresh.Name + ", left out while " + how.Unheld.Name +
                         " was marked, now earns more a day than it"
                       : "");
        }

        private static string TheNextBest(in Reckoning how)
        {
            if (how.RunnerUp == null) return ", and no other market it priced would take any of it";
            string next = how.RunnerUp.Name + ", the next best it priced, at " +
                          how.RunnerUpRate.ToString("0") + " gold a day for " +
                          how.RunnerUpValue + " gold";
            return how.Held
                ? ", and it holds the mark against " + next +
                  ", because the marker only moves for a clear gain"
                : ", ahead of " + next;
        }

        private static readonly System.Comparison<Weighing> FastestFirst =
            (x, y) => x.Rate != y.Rate ? y.Rate.CompareTo(x.Rate)
                                       : string.CompareOrdinal(x.Where.StringId, y.Where.StringId);

        private static void Ultra(in Reckoning how)
        {
            if (!Options.Current.ExtendedDebugLogging) return;
            var said = new List<string>();
            if (how.Best != null)
            {
                said.Add("  ultralog: what it marked " + how.Best.Name + " on");
                if (how.Bill != null)
                    for (int i = 0; i < how.Bill.Count; i++)
                    {
                        Share share = how.Bill[i];
                        said.Add("    " + share.Good + ": " + share.Moved + " of the " + share.Amount +
                                 " you carry, " + (share.Last == share.Price
                                     ? share.Price + " a unit"
                                     : share.Price + (share.Last < share.Price ? " a unit down to "
                                                                               : " a unit up to ") +
                                       share.Last) +
                                 (share.Today > 0 && share.Today != share.Price
                                     ? ", " + share.Today + " a unit today before what is on its way lands"
                                     : "") +
                                 ", " + share.Fetched + " gold, cost " + share.Paid + " a unit = " +
                                 (long)share.Paid * share.Moved + " gold, profit " +
                                 (share.Fetched - (long)share.Paid * share.Moved));
                    }
                said.Add("    " + how.Value + " gold in all, of which " + how.Cost +
                         " is what it cost you, so it marked on the " + (how.Value - how.Cost) +
                         (how.PurseCapped
                             ? ", which is all that town's purse of " + how.Purse +
                               " can take, so the lines above come to more"
                             : ""));
            }
            if (how.Board != null && how.Board.Count > 0)
            {
                how.Board.Sort(FastestFirst);
                said.Add("  ultralog: the " + how.Board.Count + " market(s) it priced, best first");
                for (int i = 0; i < how.Board.Count; i++)
                {
                    Weighing one = how.Board[i];
                    said.Add("    " + Tongue.Named(one.Where.Name, one.Where.StringId).PadRight(18) +
                             one.Days.ToString("0.00", CultureInfo.InvariantCulture).PadLeft(6) + " day(s) " +
                             one.Units.ToString().PadLeft(5) + " unit(s) " +
                             one.Value.ToString().PadLeft(9) + " gold " +
                             one.Earned.ToString().PadLeft(9) + " profit " +
                             one.Rate.ToString("0").PadLeft(9) + " gold a day" +
                             (one.Where == how.Best ? "  (marked)" : ""));
                }
            }
            if (how.Carried > 0)
            {
                said.Add("  ultralog: " + how.Weighed + " market(s) weighed, " + how.Told +
                         " priced, " + how.Refused + " would pay too little for anything you carry, " +
                         how.Left + " left unpriced once no purse left could beat " +
                         how.Rate.ToString("0") + " gold a day, in " +
                         how.Took.ToString("0.0", CultureInfo.InvariantCulture) + " ms");
                said.Add("  ultralog: left out before pricing, " + how.NoTill +
                         " with nothing in the till, " + how.PastCeiling + " past your travel ceilings, " +
                         how.NoRoad + " with no road it could find, " + how.Shut +
                         " under siege, raided or shut, " + how.AtWar + " at war with you" +
                         (how.CameBackTo != null
                             ? ", and " + how.CameBackTo.Name + ", " + TheMarketLeftAlone
                             : ""));
            }
            if (said.Count > 0) Log.WriteMany(said);
        }

        private static List<(EquipmentElement item, int amount, int worth)> WhatYouCarryToSell(
            MobileParty party)
        {
            int version = party.ItemRoster.VersionNo;
            if (_cargo != null && Freshness.Fresh(ref _cargoStamp) &&
                version == _cargoVersion) return _cargo;
            Freshness.Taken(ref _cargoStamp);
            _cargoVersion = version;
            ISet<string> locked = TradePolicy.LockedKeys();
            var keepBack = TradePolicy.KeptBack(party.ItemRoster, TradeActionBehavior.TheVisit,
                                                sim: false, out var awaited);
            var cargo = new List<(EquipmentElement item, int amount, int worth)>();
            for (int i = 0; i < party.ItemRoster.Count; i++)
            {
                ItemRosterElement el = party.ItemRoster.GetElementCopyAtIndex(i);
                if (!TradePolicy.MaySell(el, locked, keepBack, awaited, out int keep)) continue;
                if (el.Amount - keep <= 0) continue;
                ItemObject item = el.EquipmentElement.Item;
                cargo.Add((el.EquipmentElement, el.Amount - keep, TradePolicy.WorthToBeat(el.EquipmentElement)));
            }
            _cargo = cargo;
            _cargoHeld = Held(cargo);
            return cargo;
        }

        private static List<(string good, int amount, bool food)> Held(
            List<(EquipmentElement item, int amount, int worth)> cargo)
        {
            var held = new List<(string good, int amount, bool food)>(cargo.Count);
            for (int i = 0; i < cargo.Count; i++)
            {
                EquipmentElement el = cargo[i].item;
                if (el.Item == null) continue;
                held.Add((el.ItemModifier == null ? el.Item.StringId : el.Item.StringId + "@" + el.ItemModifier.StringId,
                          cargo[i].amount, el.Item.IsFood));
            }
            return held;
        }

        private static Paying WhatThatMarketPays(Settlement site, SettlementComponent market,
                                                 EquipmentElement el, MobileParty party, float ride) =>
            new Paying(site, market, el, party, ride);

        private static Takings WhatItWouldFetch(
            Settlement site, SettlementComponent market, MobileParty party, float ride,
            List<(EquipmentElement item, int amount, int worth)> cargo,
            int gold, List<Share> bill)
        {
            Takings took = default(Takings);
            foreach (var (item, amount, worth) in cargo)
            {
                Paying pays = WhatThatMarketPays(site, market, item, party, ride);
                long fetched = 0L;
                int moved = 0, opening = 0, last = 0;
                for (int u = 0; u < amount; u++)
                {
                    int price = pays.At(u);
                    if (price <= 0) break;
                    if (!TradeMath.ProfitAcceptable(worth, price, Options.Current.MinProfitMargin)) break;
                    if (moved == 0) opening = price;
                    last = price;
                    fetched += price;
                    moved++;
                    if (took.Value + fetched >= gold) { took.PurseCapped = true; break; }
                }
                if (moved == 0) continue;
                took.Value += fetched;
                took.Cost += (long)worth * moved;
                took.Units += moved;
                took.Kinds++;
                bill?.Add(new Share
                {
                    Good = item.Item == null ? "" : Tongue.Named(item.Item.Name, item.Item.StringId),
                    Amount = amount,
                    Moved = moved,
                    Price = opening,
                    Last = last,
                    Today = pays.OnItsWay ? pays.Today : 0,
                    Paid = worth,
                    Fetched = fetched
                });
                if (took.PurseCapped) break;
            }
            return took;
        }

        private static readonly
            System.Comparison<(Settlement s, SettlementComponent market, int gold, float days)>
            FastestPurseFirst = (x, y) =>
            {
                float faster = TradeMath.PerDay(x.gold, x.days);
                float slower = TradeMath.PerDay(y.gold, y.days);
                return faster != slower ? slower.CompareTo(faster)
                                        : string.CompareOrdinal(x.s.StringId, y.s.StringId);
            };

        private static void TheMarkedTownFirst(
            List<(Settlement s, SettlementComponent market, int gold, float days)> reachable, Settlement holder)
        {
            if (holder == null) return;
            for (int at = 1; at < reachable.Count; at++)
            {
                if (reachable[at].s != holder) continue;
                var held = reachable[at];
                reachable.RemoveAt(at);
                reachable.Insert(0, held);
                return;
            }
        }

        private static Settlement TheMarkFairlyWeighed(out Reckoning how)
        {
            Settlement best = BestSellTownForCargo(out how, _picked);
            if (how.Afresh == null || best == null || best != _picked)
            {
                how.Afresh = null;
                return best;
            }
            Settlement back = how.Afresh;
            List<string> compared = how.Compared;
            best = BestSellTownForCargo(out how, null);
            how.Afresh = back;
            how.Unheld = _picked;
            how.Compared = compared;
            return best;
        }

        private static float RateThere((Settlement s, SettlementComponent market, int gold, float days) one,
                                       MobileParty party,
                                       List<(EquipmentElement item, int amount, int worth)> cargo)
        {
            Takings took = WhatItWouldFetch(one.s, one.market, party, one.days, cargo, one.gold, null);
            if (took.Value <= 0L) return 0f;
            long total = took.Value > one.gold ? one.gold : took.Value;
            return TradeMath.PerDay(total - took.Cost, one.days);
        }

        private static Settlement OutEarnsTheMark(
            List<(Settlement s, SettlementComponent market, int gold, float days)> reachable, Settlement holder,
            MobileParty party, List<(EquipmentElement item, int amount, int worth)> cargo,
            List<string> compared)
        {
            int mine = -1;
            for (int at = 0; at < reachable.Count; at++)
                if (reachable[at].s == holder) { mine = at; break; }
            Settlement back = null;
            float marked = 0f;
            float best = 0f;
            bool priced = false;
            for (int at = 0; at < reachable.Count; at++)
            {
                Settlement s = reachable[at].s;
                if (!_owedAFairLook.Contains(s.StringId)) continue;
                compared.Add(s.StringId);
                if (mine < 0) continue;
                if (!priced) { marked = RateThere(reachable[mine], party, cargo); priced = true; }
                float rate = RateThere(reachable[at], party, cargo);
                if (!Marks.OutEarns(rate, marked) || (back != null && rate <= best)) continue;
                best = rate;
                back = s;
            }
            return back;
        }

        private static Settlement BestSellTownForCargo(out Reckoning how, Settlement holder)
        {
            how = default(Reckoning);
            MobileParty party = MobileParty.MainParty;
            if (party == null) return null;
            bool ultra = Options.Current.ExtendedDebugLogging;
            var cargo = WhatYouCarryToSell(party);
            how.Carried = cargo.Count;
            if (cargo.Count == 0) return null;

            var reachable = new List<(Settlement s, SettlementComponent market, int gold, float days)>();
            foreach (Settlement s in Settlement.All)
            {
                SettlementComponent market = s.SettlementComponent;
                if (market == null) continue;
                if (s == party.CurrentSettlement) continue;
                if (TradeActionBehavior.StillTheSameArrival(s)) continue;
                if (!TradeActionBehavior.IsMarket(s)) continue;
                if (TradeActionBehavior.MarkerLeavesItOut(s)) { how.CameBackTo = s; continue; }
                if (LedgerBehavior.UnderAttack(s) || LedgerBehavior.VillageShut(s)) { how.Shut++; continue; }
                if (Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(s))
                { how.AtWar++; continue; }
                how.Weighed++;
                int purse = TradeRules.WhatTheTillCanPay(market.Gold, s.IsVillage);
                if (purse <= 0) { how.NoTill++; continue; }
                float cap = TradeMath.CeilingTheMarkHolds(LedgerBehavior.TravelCeiling(s), s == holder);
                if (cap > 0f && Travel.StraightDaysFromParty(s) > cap) { how.PastCeiling++; continue; }
                float ride = Travel.EstimateDaysFromParty(s);
                if (TradeMath.OutOfReach(ride)) { how.NoRoad++; continue; }
                if (cap > 0f && ride > cap) { how.PastCeiling++; continue; }
                reachable.Add((s, market, purse, ride));
            }
            reachable.Sort(FastestPurseFirst);
            if (holder != null && _owedAFairLook.Count > 0)
            {
                how.Compared = new List<string>();
                how.Afresh = OutEarnsTheMark(reachable, holder, party, cargo, how.Compared);
            }
            TheMarkedTownFirst(reachable, holder);
            if (ultra) how.Board = new List<Weighing>();

            float bar = 0f;
            for (int at = 0; at < reachable.Count; at++)
            {
                var (s, market, gold, ride) = reachable[at];
                if (TradeMath.PerDay(gold, ride) <= bar) break;
                how.Told++;
                Takings took = WhatItWouldFetch(s, market, party, ride, cargo, gold, null);
                if (took.Value <= 0L) { how.Refused++; continue; }
                long total = took.Value > gold ? gold : took.Value;
                long earned = total - took.Cost;
                float rate = TradeMath.PerDay(earned, ride);
                float weighed = TradeMath.RateTheMarkHolds(rate, s == holder);
                how.Board?.Add(new Weighing
                {
                    Where = s, Days = ride, Units = took.Units, Value = total,
                    Earned = earned, Rate = rate
                });
                if (weighed > bar)
                {
                    bar = weighed;
                    how.RunnerUp = how.Best;
                    how.RunnerUpValue = how.Value;
                    how.RunnerUpRate = how.Rate;
                    how.Rate = rate;
                    how.Value = total;
                    how.Best = s;
                    how.Days = ride;
                    float plain = LedgerBehavior.TravelCeiling(s);
                    how.CeilingPassed = plain > 0f && ride > plain ? plain : 0f;
                    how.Units = took.Units;
                    how.Kinds = took.Kinds;
                    how.Purse = gold;
                    how.PurseCapped = took.PurseCapped;
                    how.Cost = took.Cost;
                }
                else if (rate > how.RunnerUpRate)
                { how.RunnerUpRate = rate; how.RunnerUpValue = total; how.RunnerUp = s; }
            }
            how.Held = how.Best != null && how.Best == holder && how.RunnerUpRate > how.Rate;
            how.Left = reachable.Count - how.Told;
            if (ultra && how.Best != null)
            {
                how.Bill = new List<Share>();
                WhatItWouldFetch(how.Best, how.Best.SettlementComponent, party, how.Days, cargo, how.Purse, how.Bill);
            }
            return how.Best;
        }

        internal static void NoteTheWalkIn(Settlement at)
        {
            _walkedInto = at?.StringId;
            _markedOnTheWayIn = _picked?.StringId;
        }

        internal static void NoteALoadInside(Settlement at)
        {
            _walkedInto = at?.StringId;
            _markedOnTheWayIn = at?.StringId;
        }

        internal static Settlement TheMarkToHoldFor(Settlement here)
        {
            Settlement mark = _picked;
            if (!Options.Current.MarkBestSellTownOnMap || mark?.SettlementComponent == null) return null;
            if (!TradeActionBehavior.IsMarket(mark)) return null;
            if (!Marks.HoldsFor(mark.StringId, here?.StringId, _walkedInto, _markedOnTheWayIn)) return null;
            if (LedgerBehavior.UnderAttack(mark) || LedgerBehavior.VillageShut(mark)) return null;
            if (Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(mark)) return null;
            return mark;
        }

        internal static int[] WhatTheMarkTakes(Settlement mark, EquipmentElement el, int carried,
                                               int worth, float ride, int purse)
        {
            MobileParty party = MobileParty.MainParty;
            if (mark?.SettlementComponent == null || party == null || el.Item == null) return new int[0];
            Paying pays = WhatThatMarketPays(mark, mark.SettlementComponent, el, party, ride);
            return TradeMath.WhatTheMarkTakes(pays.At, carried, worth, Options.Current.MinProfitMargin, purse);
        }
    }
}
