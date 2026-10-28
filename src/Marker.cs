using System.Collections.Generic;
using System.Globalization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
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
                _flat = Options.Current.Omniscient
                    ? Priced.At(_market, _el, _party, true)
                    : LedgerBehavior.Instance?.SeenSellPrice(_el, _site) ?? 0;
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

    internal struct Buyer
    {
        internal Settlement Town;
        internal float Days;
        internal int Purse;
        internal int First;
        internal long Total;
        internal float Score;
        internal Paying Pays;
    }

    internal struct Buyers
    {
        internal Buyer Best;
        internal Buyer Next;

        internal Buyer Besides(Settlement here) => Best.Town != null && Best.Town == here ? Next : Best;
    }

    internal static class Marker
    {
        private static Settlement _tracked;

        private static Settlement _picked;

        private static readonly HashSet<string> _owedAFairLook = new HashSet<string>(System.StringComparer.Ordinal);

        private sealed class Meant
        {
            internal ItemObject Item;
            internal string To;
            internal int Units;
            internal int ProfitEach;
            internal int Price;
            internal int Shelf;
            internal int Landed;
            internal float Days;
            internal float AtHours;
        }

        private static readonly Dictionary<string, Meant> _intended =
            new Dictionary<string, Meant>(System.StringComparer.Ordinal);

        private static readonly Dictionary<string, Meant> _laidOutFor =
            new Dictionary<string, Meant>(System.StringComparer.Ordinal);

        internal static void Intend(ItemObject item, TradeRoute row, int units, bool laidOut)
        {
            if (item == null || row?.To == null || units <= 0) return;
            var into = laidOut ? _laidOutFor : _intended;
            into.TryGetValue(item.StringId, out Meant was);
            into[item.StringId] = new Meant
            {
                Item = item, To = row.To.StringId,
                Units = (was != null && was.To == row.To.StringId ? was.Units : 0) + units,
                ProfitEach = TradeMath.PerUnit(row.TotalProfit, row.Quantity), Price = row.SellPrice,
                Shelf = LedgerBehavior.StockOf(row.To, item), Landed = row.Leg?.LandedAtSell ?? 0,
                Days = row.TravelDays, AtHours = (float)CampaignTime.Now.ToHours
            };
        }

        internal static void TheLaidOutBuyWasTaken(ItemObject item, int units)
        {
            if (item == null || units <= 0 || !_laidOutFor.TryGetValue(item.StringId, out Meant meant)) return;
            _laidOutFor.Remove(item.StringId);
            _intended.TryGetValue(item.StringId, out Meant was);
            meant.Units = System.Math.Min(units, meant.Units) + (was != null && was.To == meant.To ? was.Units : 0);
            _intended[item.StringId] = meant;
        }

        internal static void ForgetTheLaidOutDeal() => _laidOutFor.Clear();

        internal static List<string> WhatWasBoughtFor(Settlement site, SettlementComponent market)
        {
            var lines = new List<string>();
            if (site == null || market == null) return lines;
            float now = (float)CampaignTime.Now.ToHours;
            foreach (var kv in _intended)
            {
                Meant meant = kv.Value;
                if (meant.To != site.StringId || meant.Item == null) continue;
                int pays = Priced.At(market, meant.Item, MobileParty.MainParty, true);
                lines.Add("  " + meant.Units + " " + kv.Key + " bought " +
                          TradeMath.DaysSince(meant.AtHours, now).ToString("0.0", CultureInfo.InvariantCulture) +
                          " day(s) ago for " + site.Name + " at " + meant.Price + " a unit: it pays " + pays +
                          " now; the shelf held " + meant.Shelf + " and holds " + LedgerBehavior.StockOf(site, meant.Item) +
                          (Forecast.On
                              ? ", where Count what is on its way to a market counted goods of that kind worth " +
                                meant.Landed + " denars landing by your arrival"
                              : ""));
            }
            return lines;
        }

        private static Settlement IntendedTown(
            List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> cargo,
            List<(Settlement s, SettlementComponent market, int gold, float days)> reachable, MobileParty party,
            out string goods)
        {
            goods = null;
            if (_intended.Count == 0) return null;
            var carried = new Dictionary<string, int>(System.StringComparer.Ordinal);
            foreach (var one in cargo)
            {
                if (one.item.Item == null) continue;
                carried.TryGetValue(one.item.Item.StringId, out int had);
                carried[one.item.Item.StringId] = had + one.amount;
            }
            var open = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var one in reachable) open.Add(one.s.StringId);
            string inside = party?.CurrentSettlement?.StringId;
            float now = (float)CampaignTime.Now.ToHours;
            var gone = new List<string>();
            var worth = new Dictionary<string, long>(System.StringComparer.Ordinal);
            var named = new Dictionary<string, List<string>>(System.StringComparer.Ordinal);
            foreach (var kv in _intended)
            {
                Meant meant = kv.Value;
                bool stillThere = open.Contains(meant.To) || meant.To == inside;
                if (!carried.TryGetValue(kv.Key, out int held) || held <= 0 || !stillThere ||
                    Scoring.TooOldToSay(meant.Days, meant.AtHours, now, out _))
                {
                    gone.Add(kv.Key);
                    continue;
                }
                if (!open.Contains(meant.To)) continue;
                worth.TryGetValue(meant.To, out long sum);
                worth[meant.To] = sum + (long)System.Math.Min(held, meant.Units) * System.Math.Max(0, meant.ProfitEach);
                if (!named.TryGetValue(meant.To, out var list)) named[meant.To] = list = new List<string>();
                list.Add(kv.Key);
            }
            foreach (string id in gone) _intended.Remove(id);
            string best = null;
            long most = -1L;
            foreach (var kv in worth)
                if (kv.Value > most || (kv.Value == most && string.CompareOrdinal(kv.Key, best) < 0))
                {
                    most = kv.Value;
                    best = kv.Key;
                }
            if (best == null) return null;
            goods = string.Join(", ", named[best].ToArray());
            return Settlement.Find(best);
        }

        private static void TheIntendedTownNext(
            List<(Settlement s, SettlementComponent market, int gold, float days)> reachable, Settlement intended,
            Settlement holder)
        {
            if (intended == null || intended == holder) return;
            int to = holder != null && reachable.Count > 0 && reachable[0].s == holder ? 1 : 0;
            for (int at = to + 1; at < reachable.Count; at++)
            {
                if (reachable[at].s != intended) continue;
                var meant = reachable[at];
                reachable.RemoveAt(at);
                reachable.Insert(to, meant);
                return;
            }
        }

        private static string _walkedInto;
        private static string _markedOnTheWayIn;

        private static int _hour = -1;
        private static Vec2 _at;
        private const float MovedFar = 100f;

        private static Stamp _cargoStamp;
        private static int _cargoVersion = -1;
        private static List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> _cargo;
        private static List<(string good, int amount, bool food)> _cargoHeld;

        private static string _markedId;
        private static long _markedValue;
        private static long _markedEarned;
        private static int _markedUnits;
        private static float _markedRate;
        private static float _markedDays;
        private static int _markedAt = -1;
        private static List<(string good, int amount, bool food)> _markedCargo;
        private static Settlement _walkedOutOf;
        private static bool _justWalkedOut;
        private static string _outMarkId;
        private static long _outValue;
        private static long _outEarned;
        private static int _outUnits;
        private static float _outRate;
        private static float _outDays;
        private static int _outAt = -1;
        private static long _lastValue;
        private static int _lastAt = -1;

        private static long _saidValue = -1L;
        private static float _saidRate = -1f;
        private static long _toldValue = -1L;
        private static int _toldUnits = -1;
        private static bool _toldHeld;
        private static bool _toldSecondLook;
        private static int _lostAt = -1;
        private static List<(string good, int amount, bool food)> _heldCargo;
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
            _intended.Clear();
            _laidOutFor.Clear();
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
            _walkedOutOf = null;
            _justWalkedOut = false;
            _outMarkId = null;
            _outValue = 0L;
            _outEarned = 0L;
            _outUnits = 0;
            _outRate = 0f;
            _outDays = 0f;
            _outAt = -1;
            _lastValue = 0L;
            _lastAt = -1;
            _saidValue = -1L;
            _saidRate = -1f;
            _toldValue = -1L;
            _toldUnits = -1;
            _toldHeld = false;
            _toldSecondLook = false;
            _lostAt = -1;
            _heldCargo = null;
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
            internal long Cost;
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
            internal float Judged;
            internal string Conf;
        }

        private struct Takings
        {
            internal long Value;
            internal long Cost;
            internal float Weight;
            internal int Units;
            internal int Kinds;
            internal bool PurseCapped;
        }

        private struct Reckoning
        {
            internal Settlement Intended;
            internal string IntendedGoods;
            internal float IntendedRate;
            internal float IntendedJudged;
            internal Settlement Best;
            internal Settlement RunnerUp;
            internal bool Held;
            internal long Value;
            internal long RunnerUpValue;
            internal long Cost;
            internal float Rate;
            internal float RunnerUpRate;
            internal float Judged;
            internal float RunnerUpJudged;
            internal float HolderJudged;
            internal float Bar;
            internal float Days;
            internal int Units;
            internal int Kinds;
            internal int Purse;
            internal int Carried;
            internal int Weighed;
            internal int Refused;
            internal int TooPoor;
            internal bool PurseCapped;
            internal int Told;
            internal int Left;
            internal int NoTill;
            internal int Unseen;
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
            internal bool HeldForASecondLook;
            internal bool HolderPriced;
            internal long HolderValue;
            internal long HolderCost;
            internal float HolderRate;
            internal float HolderDays;
            internal int HolderUnits;
            internal int HolderKinds;
            internal int HolderPurse;
            internal bool HolderPurseCapped;
            internal bool HolderTooPoor;
            internal List<Share> HolderBill;
            internal string HolderLost;
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

            int hour = (int)CampaignTime.Now.ToHours;
            Settlement was = _picked;
            int lostBefore = _lostAt;
            bool cargoSame = Marks.OnlyEatenFrom(_heldCargo, _cargoHeld);
            if (_picked != null && target != _picked && target != null &&
                Marks.HoldsForASecondLook(how.HolderPriced, how.Afresh != null, cargoSame, _lostAt, hour))
            {
                if (_lostAt < 0) _lostAt = hour;
                TheHolderStays(ref how);
                target = _picked;
            }
            else
            {
                _lostAt = -1;
                _heldCargo = _cargoHeld;
            }

            if (target != null)
            {
                if (target.IsVillage) EngineTally.MarkedVillage++;
                else EngineTally.MarkedTown++;
            }

            WhatItSaidOnTheWayOut(target, how);
            if (on && how.Intended != null && target != null && target != how.Intended &&
                Options.Current.ExtendedDebugLogging)
                Log.Repeatable("marker intended", target.StringId + ">" + how.Intended.StringId,
                               "map marker chose " + target.Name + " over " + how.Intended.Name +
                               ", the Sell At the TradeLord ledger planned for your " + how.IntendedGoods + ": " +
                               (int)how.Rate + " against " + (int)how.IntendedRate +
                               " profit a day for your whole cargo" +
                               (Options.Current.ConfidenceRanking
                                   ? ", and " + (int)how.Judged + " against " + (int)how.IntendedJudged +
                                     " once each is weighed at the Conf of selling there"
                                   : ""));

            if (target == _picked)
            {
                if (target != null && (_tracked != target || !tracker.CheckTracked(target)))
                {
                    tracker.RegisterObject(target);
                    _tracked = target;
                }
                Marks.OweAFairLook(_owedAFairLook, how.Compared, MobileParty.MainParty?.CurrentSettlement?.StringId,
                                   target == null ? null : TradeActionBehavior.MarketTheMarkerLeavesOut(), target == null);
                if (target != null) SayItWeighedAgain(target, how);
                return;
            }
            if (_tracked != null && tracker.CheckTracked(_tracked))
                tracker.RemoveTrackedObject(_tracked);
            _tracked = null;
            _picked = target;
            Marks.OweAFairLook(_owedAFairLook, how.Compared, MobileParty.MainParty?.CurrentSettlement?.StringId,
                               target == null ? null : TradeActionBehavior.MarketTheMarkerLeavesOut(), true);
            if (target != null)
            {
                tracker.RegisterObject(target);
                _tracked = target;
            }
            string why = on ? Why(how) : "the map marker is switched off";
            string left = on ? LeftBecause(was, target, how, lostBefore, cargoSame) : null;
            Log.Write(target != null
                ? "map marker moved to " + target.Name + (left == null ? "" : " from " + was.Name + " because " + left) +
                  ": " + why
                : left == null
                    ? "map marker taken off the map: " + why
                    : "map marker taken off " + was.Name + " because " + left + ": " + why);
            Ultra(how, board: true);
            Told(how);
            Remember(target, how);
        }

        private static void TheHolderStays(ref Reckoning how)
        {
            how.RunnerUp = how.Best;
            how.RunnerUpValue = how.Value;
            how.RunnerUpRate = how.Rate;
            how.RunnerUpJudged = how.Judged;
            how.Best = _picked;
            how.Value = how.HolderValue;
            how.Cost = how.HolderCost;
            how.Rate = how.HolderRate;
            how.Judged = how.HolderJudged;
            how.Days = how.HolderDays;
            how.Units = how.HolderUnits;
            how.Kinds = how.HolderKinds;
            how.Purse = how.HolderPurse;
            how.PurseCapped = how.HolderPurseCapped;
            how.Bill = how.HolderBill;
            how.Held = false;
            how.HeldForASecondLook = true;
        }

        private static string LeftBecause(Settlement was, Settlement target, in Reckoning how, int lostBefore,
                                             bool cargoSame)
        {
            if (was == null || was == target) return null;
            if (how.Afresh != null && how.Unheld != null)
                return "a market left out while it was marked was weighed again and earns more a day";
            if (how.HolderLost != null) return how.HolderLost;
            if (!how.HolderPriced) return was.Name + " was not priced";
            if (target == null)
                return was.Name + " would earn " + how.HolderRate.ToString("0") + " profit a day on what you carry";
            string against = target.Name + " earns " + how.Rate.ToString("0") + " profit a day against " + was.Name + "'s " +
                             how.HolderRate.ToString("0") +
                             (Options.Current.ConfidenceRanking
                                 ? ", " + how.Judged.ToString("0") + " against " + how.HolderJudged.ToString("0") +
                                   " once each is weighed at the Conf of selling there"
                                 : "");
            if (!cargoSame) return "your cargo changed and " + against;
            return lostBefore >= 0 ? against + " for a second hour in a row" : against;
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
            _toldSecondLook = how.HeldForASecondLook;
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
            if (Marks.WorthSayingAgain(how.Value, how.Units, how.Held, _toldValue, _toldUnits, _toldHeld) ||
                how.HeldForASecondLook != _toldSecondLook)
            {
                Log.Write("map marker weighed your cargo again and stayed on " + target.Name + ": " + Why(how));
                Ultra(how, board: false);
                SayWhatElseMoved(target, how, alongside: true);
                Told(how);
            }
            else if (SayWhatElseMoved(target, how, alongside: false)) Told(how);
            if (how.Value == _saidValue && how.Rate == _saidRate) return;
            Remember(target, how);
        }

        private static bool SayWhatElseMoved(Settlement target, in Reckoning how, bool alongside)
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
                if (one.Where == null || one.Where == target) continue;
                string id = one.Where.StringId;
                bool fresh = joined.Contains(id);
                if (!fresh && !changed.Contains(id)) continue;
                string row = Tongue.Named(one.Where.Name, id) + (fresh ? " newly priced, " : " now ") + one.Units +
                             " unit(s) for " + one.Value + " gold at " + one.Rate.ToString("0") + " profit a day";
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
                      " profit a day for " + how.RunnerUpValue + " gold");
            if (said.Count == 0) return false;
            Log.Write(alongside
                ? "  and of the other markets it priced, " + string.Join("; ", said)
                : "map marker weighed your cargo again and stayed on " + target.Name + ", still " + how.Units +
                  " unit(s) for " + how.Value + " gold, now " + how.Rate.ToString("0") +
                  " profit a day, and only other markets moved: " + string.Join("; ", said));
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

        internal static void NoteTheWayOut(Settlement from)
        {
            if (from == null) return;
            _walkedOutOf = from;
            _justWalkedOut = true;
        }

        private static void WhatItSaidOnTheWayOut(Settlement target, in Reckoning how)
        {
            if (!_justWalkedOut) return;
            _justWalkedOut = false;
            _outMarkId = target == null ? null : target.StringId;
            _outValue = target == null ? 0L : how.Value;
            _outEarned = target == null ? 0L : how.Value - how.Cost;
            _outUnits = how.Units;
            _outRate = how.Rate;
            _outDays = how.Days;
            _outAt = Freshness.Hour;
        }

        internal static void ScoreTheMark(Settlement site, int units, int gold, int profit)
        {
            if (!Options.Current.ExtendedDebugLogging) return;
            ScoreTheFigureItHeld(site, units, gold);
            ScoreWhatItSaidOnTheWayOut(site, units, gold, profit);
        }

        private static void ScoreWhatItSaidOnTheWayOut(Settlement site, int units, int gold, int profit)
        {
            if (site == null || _walkedOutOf == null || _outMarkId == null) return;
            if (site.StringId != _outMarkId || _outValue <= 0L) return;
            long said = _outValue;
            _outValue = 0L;
            float ride = TradeMath.DaysSince(_outAt, Freshness.Hour);
            float held = TradeMath.HeldShare(said > int.MaxValue ? int.MaxValue : (int)said, gold);
            float aDay = TradeMath.PerDay(profit, ride);
            Log.Write("marker check on the way out of " + _walkedOutOf.Name + ": it marked " + site.Name + " for " +
                      _outUnits + " unit(s) worth " + said + " gold, " + _outEarned + " of it profit, " +
                      _outRate.ToString("0") + " profit a day, about " +
                      _outDays.ToString("0.#", CultureInfo.InvariantCulture) + " day(s) away; the ride took " +
                      Scoring.Figure(ride) + " day(s) and you sold " + units + " unit(s) for " + gold + " gold, " +
                      profit + " of it profit, " + aDay.ToString("0") + " profit a day" +
                      (held == TradeMath.NoShareToGive ? "" : ", " + Scoring.Share(held) + " of the gold it said") +
                      (_outRate > 0f ? " and " + Scoring.Share(aDay / _outRate) + " of the profit a day it said" : ""));
        }

        private static void ScoreTheFigureItHeld(Settlement site, int units, int gold)
        {
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
                      " profit a day, about " + _markedDays.ToString("0.#", CultureInfo.InvariantCulture) + " day(s) away; you walked in " +
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
                       "clear Minimum profit margin, " + how.TooPoor +
                       " have too little gold for one unit of any that does, " +
                       how.NoRoad + " have no road it could find, " +
                       how.PastCeiling + " are past your travel ceilings and " + how.NoTill +
                       " have no gold at all" +
                       (how.Unseen > 0
                           ? ", " + how.Unseen + " have no price you have seen for anything you carry"
                           : "") +
                       (how.CameBackTo != null
                           ? ", and it leaves out " + how.CameBackTo.Name + ", " + TheMarketLeftAlone
                           : "");
            return how.Kinds + " of the " + how.Carried + " good(s) you carry clear Minimum profit " +
                   "margin there, " + how.Units + " unit(s) for " + how.Value + " gold, " +
                   (how.Value - how.Cost) + " of it profit" +
                   (how.PurseCapped
                       ? ", which is all that " + MarketKind(how.Best) + "'s purse of " + how.Purse + " can take"
                       : how.Purse == PurseUnread
                           ? ", its purse left unread as Live world prices is off"
                           : " against a " + MarketKind(how.Best) + " purse of " + how.Purse) +
                   ", about " + how.Days.ToString("0.#", CultureInfo.InvariantCulture) + " day(s) away" +
                   ", so " + how.Rate.ToString("0") + " profit a day" + TheNextBest(how) +
                   (how.Afresh != null && how.Unheld != null
                       ? ", marked afresh because " + how.Afresh.Name + ", left out while " + how.Unheld.Name +
                         " was marked, now earns more a day than it"
                       : "");
        }

        private static string MarketKind(Settlement s) => s != null && s.IsVillage ? "village" : "town";

        private static string TheNextBest(in Reckoning how)
        {
            if (how.RunnerUp == null) return ", and no other market it priced would take any of it";
            string next = how.RunnerUp.Name + ", the next best it priced, at " +
                          how.RunnerUpRate.ToString("0") + " profit a day for " +
                          how.RunnerUpValue + " gold";
            if (how.HeldForASecondLook)
                return ", and it holds the mark against " + next +
                       ", until another market earns more in a later hour";
            if (how.Held)
                return ", and it holds the mark against " + next +
                       ", because the marker only moves for a clear gain";
            if (how.RunnerUpRate > how.Rate)
                return ", chosen over " + next + ", because once each is weighed at the Conf of selling there it " +
                       "comes to " + how.Judged.ToString("0") + " against " + how.RunnerUpJudged.ToString("0");
            return ", ahead of " + next;
        }

        private static readonly System.Comparison<Weighing> FastestFirst =
            (x, y) => x.Judged != y.Judged ? y.Judged.CompareTo(x.Judged)
                                           : string.CompareOrdinal(x.Where.StringId, y.Where.StringId);

        private static void Ultra(in Reckoning how, bool board)
        {
            if (!Options.Current.ExtendedDebugLogging) return;
            var said = new List<string>();
            if (how.Best != null)
            {
                said.Add("  ultralog: what it marked " + how.Best.Name + " on");
                said.Add("    " + Travel.TheRideTo(how.Best));
                said.Add("    " + Travel.TheRoadsOthersRide());
                string villages = Forecast.TheVillagesSend();
                if (villages != null) said.Add("    " + villages);
                string unseen = Forecast.TheCaravansNotYetBound();
                if (unseen != null) said.Add("    " + unseen);
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
                                 share.Cost + " gold, profit " + (share.Fetched - share.Cost));
                    }
                said.Add("    " + how.Value + " gold in all, of which " + how.Cost +
                         " is what it cost you, so it marked on the " + (how.Value - how.Cost) +
                         (how.PurseCapped
                             ? ", which is all that " + MarketKind(how.Best) + "'s purse of " + how.Purse +
                               " can take"
                             : ""));
            }
            if (board && how.Board != null && how.Board.Count > 0)
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
                             one.Rate.ToString("0").PadLeft(9) + " profit a day" +
                             (one.Judged.ToString("0") == one.Rate.ToString("0")
                                 ? ""
                                 : ", weighed " + one.Judged.ToString("0")) +
                             (one.Conf == null ? "" : ", " + one.Conf) +
                             (one.Where == how.Best ? "  (marked)" : ""));
                }
            }
            if (how.Carried > 0)
            {
                said.Add("  ultralog: " + how.Weighed + " market(s) weighed, " + how.Told +
                         " priced, " + how.Refused + " would pay too little for anything you carry, " + how.TooPoor +
                         " have too little gold for one unit of anything that clears your margin, " +
                         how.Left + " left unpriced once no price they could pay would beat " +
                         how.Bar.ToString("0") +
                         (how.Bar.ToString("0") == how.Rate.ToString("0")
                             ? " profit a day"
                             : " profit a day as weighed at the Conf of selling there") + ", in " +
                         how.Took.ToString("0.0", CultureInfo.InvariantCulture) + " ms");
                said.Add("  ultralog: left out before pricing, " + how.NoTill +
                         " with nothing in the till, " +
                         (how.Unseen > 0 ? how.Unseen + " with no price you have seen, " : "") +
                         how.PastCeiling + " past your travel ceilings, " +
                         how.NoRoad + " with no road it could find, " + how.Shut +
                         " under siege, raided or shut, " + how.AtWar + " at war with you" +
                         (how.CameBackTo != null
                             ? ", and " + how.CameBackTo.Name + ", " + TheMarketLeftAlone
                             : ""));
            }
            if (said.Count > 0) Log.WriteMany(said);
        }

        private static ItemObject TheStalestYouCarryAt(
            List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> cargo, Settlement s)
        {
            ItemObject stalest = cargo[0].item.Item;
            LedgerBehavior ledger = LedgerBehavior.Instance;
            if (Options.Current.Omniscient || ledger == null) return stalest;
            float oldest = ledger.ObservationAgeDays(stalest, s);
            for (int i = 1; i < cargo.Count; i++)
            {
                ItemObject one = cargo[i].item.Item;
                float age = ledger.ObservationAgeDays(one, s);
                if (age <= oldest) continue;
                oldest = age;
                stalest = one;
            }
            return stalest;
        }

        private static List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> WhatYouCarryToSell(
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
            var cargo = new List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)>();
            float today = (float)CampaignTime.Now.ToDays;
            for (int i = 0; i < party.ItemRoster.Count; i++)
            {
                ItemRosterElement el = party.ItemRoster.GetElementCopyAtIndex(i);
                if (!TradePolicy.MaySell(el, locked, keepBack, awaited, out int keep)) continue;
                if (el.Amount - keep <= 0) continue;
                int worth = TradePolicy.WorthToBeat(el.EquipmentElement);
                Batch[] costs = LedgerBehavior.Instance?.UnitCosts(el.EquipmentElement, el.Amount);
                int covers = Options.Current.CostBasisMode == 0 && costs != null
                    ? TradeMath.WhatTheAverageCovers(worth, Options.Current.MinProfitMargin)
                    : int.MaxValue;
                int unknown = el.Amount - TradeMath.UnitsIn(costs);
                cargo.Add((el.EquipmentElement, el.Amount - keep, worth,
                           Options.Current.CostBasisMode == Options.CostOfEachUnit
                               ? TradeMath.DearFirst.EachAtItsOwnCost(costs, worth, unknown, Options.Current.MinProfitMargin,
                                                                      today, Options.Current.SellAtCostAfterDays)
                               : new TradeMath.DearFirst(costs, covers, worth, unknown)));
            }
            _cargo = cargo;
            _cargoHeld = Held(cargo);
            return cargo;
        }

        private static List<(string good, int amount, bool food)> Held(
            List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> cargo)
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

        internal static Buyers BestBuyersOf(EquipmentElement el, int units)
        {
            Buyers found = default(Buyers);
            MobileParty party = MobileParty.MainParty;
            List<(Settlement town, int price)> markets =
                el.Item == null || units <= 0 || party == null ? null : LedgerBehavior.Instance?.EverySell(el.Item);
            var pressure = markets == null ? null : LedgerBehavior.CaravanPressure();
            for (int i = 0; markets != null && i < markets.Count; i++)
            {
                Settlement town = markets[i].town;
                SettlementComponent market = town?.SettlementComponent;
                if (market == null) continue;
                float ride = Travel.EstimateDaysFromParty(town);
                if (TradeMath.OutOfReach(ride) || !LedgerBehavior.WithinTravelCeiling(town, ride)) continue;
                int purse = PurseOf(town, ride);
                Paying pays = WhatThatMarketPays(town, market, el, party, ride);
                var one = new Buyer
                {
                    Town = town, Days = ride, Purse = purse, First = pays.At(0), Pays = pays,
                    Total = TradeMath.WhatALadderPays(pays.At, units, purse)
                };
                if (one.First <= 0 || one.Total <= 0L) continue;
                one.Score = Plan.SaleScore(el.Item, town, one.Total, (long)one.First * units, units, ride, true, 0,
                                           pressure.TryGetValue(town, out int caravans) ? caravans : 0, out _, out _);
                if (found.Best.Town == null || one.Score > found.Best.Score)
                {
                    found.Next = found.Best;
                    found.Best = one;
                }
                else if (found.Next.Town == null || one.Score > found.Next.Score)
                    found.Next = one;
            }
            return found;
        }

        private static long MostThisCargoCouldFetch(
            Settlement site, SettlementComponent market, MobileParty party,
            List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> cargo)
        {
            long most = 0L;
            for (int at = 0; at < cargo.Count; at++)
            {
                int quoted = Options.Current.Omniscient
                    ? Priced.At(market, cargo[at].item, party, true)
                    : LedgerBehavior.Instance?.SeenSellPrice(cargo[at].item, site) ?? 0;
                if (quoted <= 0) continue;
                most += (long)TradeMath.MostAForecastCanReach(quoted) * cargo[at].amount;
            }
            return most;
        }

        private static Takings WhatItWouldFetch(
            Settlement site, SettlementComponent market, MobileParty party, float ride,
            List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> cargo,
            int gold, List<Share> bill, int[][] keptFor = null, float share = 0f)
        {
            Takings took = default(Takings);
            bool eachAtItsOwnCost = Options.Current.CostBasisMode == Options.CostOfEachUnit;
            var order = new List<(Paying pays, long gain, int at)>(cargo.Count);
            var paying = new Paying[cargo.Count];
            var stacks = new int[cargo.Count];
            var unbought = new int[cargo.Count];
            for (int at = 0; at < cargo.Count; at++)
            {
                Paying pays = WhatThatMarketPays(site, market, cargo[at].item, party, ride);
                int first = pays.At(0);
                int stack = party.ItemRoster.FindIndexOfElement(cargo[at].item);
                int held = stack >= 0 ? party.ItemRoster.GetElementNumber(stack) : cargo[at].amount;
                long gain = first > 0 ? ((long)first - cargo[at].worth) * held : 0L;
                order.Add((pays, gain > 0L ? gain : 0L, at));
                paying[at] = pays;
                stacks[at] = held;
                unbought[at] = System.Math.Max(0, cargo[at].amount - (held - cargo[at].floors.Unknown));
            }
            order.Sort(TheStackThatMakesTheMostFirst);
            var lines = new Share[cargo.Count];
            var sold = new List<int>(cargo.Count);
            foreach (var (pays, _, at) in order)
            {
                if (unbought[at] <= 0) continue;
                int loose = TradePolicy.WorthToBeatUnbought(cargo[at].item);
                long fetched = 0L;
                int moved = 0;
                for (int u = 0; u < unbought[at]; u++)
                {
                    int price = pays.At(u);
                    if (price <= 0) break;
                    if (!TradeMath.ProfitAcceptable(loose, price, Options.Current.MinProfitMargin)) break;
                    if (took.Value + fetched + price > gold) { took.PurseCapped = true; break; }
                    if (moved == 0) lines[at].Price = price;
                    lines[at].Last = price;
                    fetched += price;
                    moved++;
                }
                if (moved == 0) continue;
                long cost = (long)loose * moved;
                took.Value += fetched;
                took.Cost += cost;
                took.Units += moved;
                took.Weight += moved * Carry.Weighs(cargo[at].item.Item);
                lines[at].Moved = moved;
                lines[at].Cost = cost;
                lines[at].Fetched = fetched;
                sold.Add(at);
            }
            var resorted = new List<(Paying pays, long gain, int at)>(order.Count);
            foreach (var (pays, _, at) in order)
            {
                int taken = lines[at].Moved;
                int next = pays.At(taken);
                long gain = next > 0 ? ((long)next - cargo[at].worth) * (stacks[at] - taken) : 0L;
                resorted.Add((pays, gain > 0L ? gain : 0L, at));
            }
            resorted.Sort(TheStackThatMakesTheMostFirst);
            foreach (var (pays, _, at) in resorted)
            {
                var (item, amount, worth, floors) = cargo[at];
                TradeMath.DearFirst walk = floors.WithoutTheUnknown();
                int from = lines[at].Moved;
                int paidFor = amount - unbought[at];
                long fetched = 0L, booked = 0L;
                int moved = 0;
                for (int u = from; u < from + paidFor; u++)
                {
                    int price = pays.At(u);
                    if (price <= 0) break;
                    if (!walk.Clears(price, Options.Current.MinProfitMargin)) break;
                    if (keptFor != null && TradeRules.KeptForTheMark(price, keptFor[at], paidFor - moved, share)) break;
                    if (took.Value + fetched + price > gold) { took.PurseCapped = true; break; }
                    booked += walk.Took();
                    if (from + moved == 0) lines[at].Price = price;
                    lines[at].Last = price;
                    fetched += price;
                    moved++;
                }
                if (moved == 0) continue;
                long cost = eachAtItsOwnCost ? booked : (long)worth * moved;
                took.Value += fetched;
                took.Cost += cost;
                took.Units += moved;
                took.Weight += moved * Carry.Weighs(item.Item);
                if (from == 0) sold.Add(at);
                lines[at].Moved += moved;
                lines[at].Cost += cost;
                lines[at].Fetched += fetched;
            }
            foreach (int at in sold)
            {
                took.Kinds++;
                Paying pays = paying[at];
                Share line = lines[at];
                EquipmentElement item = cargo[at].item;
                bill?.Add(new Share
                {
                    Good = item.Item == null ? "" : Tongue.Named(item.Item.Name, item.Item.StringId),
                    Amount = cargo[at].amount,
                    Moved = line.Moved,
                    Price = line.Price,
                    Last = line.Last,
                    Today = pays.OnItsWay ? pays.Today : 0,
                    Paid = (int)System.Math.Round((double)line.Cost / line.Moved),
                    Cost = line.Cost,
                    Fetched = line.Fetched
                });
            }
            return took;
        }

        private static readonly System.Comparison<(Paying pays, long gain, int at)> TheStackThatMakesTheMostFirst =
            (x, y) => x.gain != y.gain ? y.gain.CompareTo(x.gain) : x.at.CompareTo(y.at);

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
                                       List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> cargo)
        {
            float ride = Travel.EstimateDaysFromParty(one.s);
            int gold = PurseOf(one.s, ride);
            if (gold <= 0) return 0f;
            Takings took = WhatItWouldFetch(one.s, one.market, party, ride, cargo, gold, null);
            if (took.Value <= 0L) return 0f;
            long total = took.Value > gold ? gold : took.Value;
            return TradeMath.PerDay(total - took.Cost, ride);
        }

        private static Settlement OutEarnsTheMark(
            List<(Settlement s, SettlementComponent market, int gold, float days)> reachable, Settlement holder,
            MobileParty party, List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> cargo,
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

        internal const int PurseUnread = int.MaxValue;

        internal static int PurseOf(Settlement s, float days) =>
            Options.Current.Omniscient
                ? TradeRules.WhatTheTillCanPay(Forecast.TillOnArrival(s, days), s.IsVillage)
                : PurseUnread;

        private static int PurseAtMost(Settlement s) =>
            Options.Current.Omniscient
                ? TradeRules.WhatTheTillCanPay(Forecast.TillAtMost(s), s.IsVillage)
                : PurseUnread;

        private static bool SeenAnyOf(
            List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> cargo, Settlement s)
        {
            LedgerBehavior ledger = LedgerBehavior.Instance;
            if (ledger == null) return false;
            foreach (var (item, _, _, _) in cargo)
                if (ledger.SeenSellPrice(item, s) > 0) return true;
            return false;
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
            EngineTally.Marker++;

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
                if (!Options.Current.Omniscient && !SeenAnyOf(cargo, s)) { how.Unseen++; continue; }
                int purse = PurseAtMost(s);
                if (purse <= 0) { how.NoTill++; continue; }
                float cap = LedgerBehavior.TravelCeiling(s);
                float soonest = Travel.StraightDaysFromParty(s);
                if (cap <= 0f) { reachable.Add((s, market, purse, soonest)); continue; }
                if (soonest > cap) { how.PastCeiling++; continue; }
                float ride = Travel.EstimateDaysFromParty(s);
                if (TradeMath.OutOfReach(ride)) { how.NoRoad++; continue; }
                if (!LedgerBehavior.WithinTravelCeiling(s, ride)) { how.PastCeiling++; continue; }
                reachable.Add((s, market, purse, ride));
            }
            reachable.Sort(FastestPurseFirst);
            if (holder != null && _owedAFairLook.Count > 0)
            {
                how.Compared = new List<string>();
                how.Afresh = OutEarnsTheMark(reachable, holder, party, cargo, how.Compared);
            }
            TheMarkedTownFirst(reachable, holder);
            how.Intended = IntendedTown(cargo, reachable, party, out how.IntendedGoods);
            TheIntendedTownNext(reachable, how.Intended, holder);
            bool oneBasis = how.Intended != null && Options.Current.ConfidenceRanking;
            var pressure = oneBasis ? LedgerBehavior.CaravanPressure() : null;
            if (ultra) how.Board = new List<Weighing>();

            float bar = 0f;
            int roadless = 0;
            for (int at = 0; at < reachable.Count; at++)
            {
                var (s, market, gold, ride) = reachable[at];
                if (s != holder && s != how.Intended)
                {
                    if (TradeMath.PerDay(gold, ride) <= bar) break;
                    if (TradeMath.PerDay(MostThisCargoCouldFetch(s, market, party, cargo), ride) <= bar) continue;
                }
                ride = Travel.EstimateDaysFromParty(s);
                if (TradeMath.OutOfReach(ride)) { how.NoRoad++; roadless++; continue; }
                gold = PurseOf(s, ride);
                if (gold <= 0) { how.NoTill++; continue; }
                how.Told++;
                Takings took = WhatItWouldFetch(s, market, party, ride, cargo, gold, null);
                if (took.Value <= 0L && took.PurseCapped)
                {
                    how.TooPoor++;
                    if (s == holder) how.HolderTooPoor = true;
                    continue;
                }
                if (took.Value <= 0L) { how.Refused++; continue; }
                long total = took.Value > gold ? gold : took.Value;
                long earned = total - took.Cost;
                float rate = TradeMath.PerDay(earned, ride);
                if (s == how.Intended) how.IntendedRate = rate;
                int till = 0;
                long worthThere = 1L;
                if (oneBasis && took.PurseCapped)
                {
                    till = gold;
                    worthThere = WhatItWouldFetch(s, market, party, ride, cargo, int.MaxValue, null).Value;
                }
                float judged = rate;
                string confSaid = null;
                if (oneBasis)
                {
                    int caravans = pressure.TryGetValue(s, out int onTheRoad) ? onTheRoad : 0;
                    judged = rate * Plan.SellSide(TheStalestYouCarryAt(cargo, s), s, ride, true, caravans, till,
                                                  worthThere, out Confidence.Parts parts, out float trust);
                    EngineTally.ConfWeighed++;
                    if (parts.Depth < 1f) EngineTally.TillCountedAgainst++;
                    if (parts.Quiet < 1f) EngineTally.TrafficCountedAgainst++;
                    int paysOut = Forecast.TillPaysOutBefore(s, ride);
                    if (paysOut > 0) EngineTally.TillPaidOutFirst++;
                    if (ultra)
                    {
                        confSaid = Confidence.Spelled(parts) + " x trust " + Scoring.Share(trust) +
                                   (till > 0
                                       ? ", its till " + till + " against the " + worthThere +
                                         " your cargo would fetch there"
                                       : ", its till covered the whole sale") +
                                   (paysOut > 0
                                       ? ", and the sellers already on the road draw " + paysOut +
                                         " of that till before you arrive, leaving it " +
                                         Forecast.TillOnArrival(s, ride) + " when you get there"
                                       : "") +
                                   ", " + caravans + " caravan(s) and villager(s) at or heading there";
                    }
                }
                float weighed = TradeMath.RateTheMarkHolds(judged, s == holder);
                if (s == how.Intended) how.IntendedJudged = weighed;
                if (s == holder)
                {
                    how.HolderPriced = true;
                    how.HolderValue = total;
                    how.HolderCost = took.Cost;
                    how.HolderRate = rate;
                    how.HolderJudged = weighed;
                    how.HolderDays = ride;
                    how.HolderUnits = took.Units;
                    how.HolderKinds = took.Kinds;
                    how.HolderPurse = gold;
                    how.HolderPurseCapped = took.PurseCapped;
                }
                how.Board?.Add(new Weighing
                {
                    Where = s, Days = ride, Units = took.Units, Value = total,
                    Earned = earned, Rate = rate, Judged = weighed, Conf = confSaid
                });
                if (weighed > bar)
                {
                    bar = weighed;
                    if (how.Best != null && how.Judged > how.RunnerUpJudged)
                    {
                        how.RunnerUp = how.Best;
                        how.RunnerUpValue = how.Value;
                        how.RunnerUpRate = how.Rate;
                        how.RunnerUpJudged = how.Judged;
                    }
                    how.Rate = rate;
                    how.Judged = weighed;
                    how.Value = total;
                    how.Best = s;
                    how.Days = ride;
                    how.Units = took.Units;
                    how.Kinds = took.Kinds;
                    how.Purse = gold;
                    how.PurseCapped = took.PurseCapped;
                    how.Cost = took.Cost;
                }
                else if (weighed > how.RunnerUpJudged)
                {
                    how.RunnerUpJudged = weighed;
                    how.RunnerUpRate = rate;
                    how.RunnerUpValue = total;
                    how.RunnerUp = s;
                }
            }
            how.Bar = bar;
            if (holder != null && !how.HolderPriced)
                how.HolderLost = TheMarkLeftOut(holder, party, cargo, reachable, ref how);
            how.Held = how.Best != null && how.Best == holder && how.RunnerUpRate > how.Rate;
            how.Left = reachable.Count - how.Told - roadless;
            if (ultra && how.Best != null)
            {
                how.Bill = new List<Share>();
                WhatItWouldFetch(how.Best, how.Best.SettlementComponent, party, how.Days, cargo, how.Purse, how.Bill);
            }
            if (ultra && how.HolderPriced && how.Best != holder)
            {
                how.HolderBill = new List<Share>();
                WhatItWouldFetch(holder, holder.SettlementComponent, party, how.HolderDays, cargo, how.HolderPurse,
                                 how.HolderBill);
            }
            return how.Best;
        }

        private static string TheMarkLeftOut(
            Settlement holder, MobileParty party,
            List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> cargo,
            List<(Settlement s, SettlementComponent market, int gold, float days)> reachable, ref Reckoning how)
        {
            for (int at = 0; at < reachable.Count; at++)
                if (reachable[at].s == holder)
                    return how.HolderTooPoor
                        ? holder.Name + "'s purse of " + reachable[at].gold +
                          " cannot pay for one unit of anything you carry that clears Minimum profit margin"
                        : holder.Name + " would pay too little for anything you carry to clear Minimum profit margin";
            SettlementComponent market = holder.SettlementComponent;
            if (market == null) return holder.Name + " has no market";
            if (holder == party.CurrentSettlement || TradeActionBehavior.StillTheSameArrival(holder))
                return "you walked into " + holder.Name;
            if (!TradeActionBehavior.IsMarket(holder)) return holder.Name + " is a market TradeLord is set not to trade with";
            if (TradeActionBehavior.MarkerLeavesItOut(holder))
                return "TradeLord traded at " + holder.Name + " and leaves it out until you come back to it";
            if (holder.IsUnderSiege)
            {
                MobileParty besieger = holder.SiegeEvent?.BesiegerCamp?.LeaderParty;
                return holder.Name + " is under siege" + (besieger == null ? "" : " by " + besieger.Name);
            }
            if (holder.IsUnderRaid) return holder.Name + " is being raided";
            if (LedgerBehavior.VillageShut(holder))
                return holder.Name + " is shut, its village " +
                       (holder.Village.VillageState == Village.VillageStates.Looted ? "looted" : holder.Village.VillageState.ToString());
            if (Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(holder))
                return holder.Name + " is at war with you";
            if (!Options.Current.Omniscient && !SeenAnyOf(cargo, holder))
                return holder.Name + " has no price you have seen for anything you carry";
            float ride = Travel.EstimateDaysFromParty(holder);
            if (TradeMath.OutOfReach(ride)) return "no road to " + holder.Name + " could be found";
            int purse = PurseOf(holder, ride);
            if (purse <= 0)
                return holder.Name + "'s purse would be empty when you get there, " + market.Gold + " gold now";
            if (LedgerBehavior.WithinTravelCeiling(holder, ride)) return null;
            return holder.Name + " is " + ride.ToString("0.#", CultureInfo.InvariantCulture) + " day(s) away, past your travel " +
                   "ceiling of " + LedgerBehavior.TravelCeiling(holder).ToString("0.#", CultureInfo.InvariantCulture) + " day(s)";
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

        internal static Settlement HoldFor(EquipmentElement el, int units, Settlement here,
                                           System.Func<Settlement, float> rideTo, float lead,
                                           out float ride, out int purse, out bool boughtFor, out NoHold why)
        {
            ride = 0f;
            purse = 0;
            boughtFor = false;
            why = NoHold.NoMarket;
            if (el.Item == null || rideTo == null) return null;
            if (InTheMarkedMarket(here)) { why = NoHold.MarkedMarket; return null; }
            Settlement meant = BoughtFor(el.Item);
            if (meant != null && meant == here) { why = NoHold.ItsSellAt; return null; }
            float meantRide = 0f, markRide = 0f;
            int meantPurse = 0, markPurse = 0;
            bool meantHolds = meant != null && MayHoldFor(meant) && InReach(meant, rideTo, out meantRide, out meantPurse);
            Settlement mark = TheMarkToHoldFor(here);
            bool markHolds = mark != null && mark != meant && InReach(mark, rideTo, out markRide, out markPurse);
            long meantPays = 0L, markPays = 0L;
            if (meantHolds && markHolds)
            {
                meantPays = PaysFor(meant, el, units, lead + meantRide, meantPurse);
                markPays = PaysFor(mark, el, units, lead + markRide, markPurse);
            }
            if (TradeRules.HoldForTheSellAt(meantHolds, markHolds, meantPays, markPays))
            {
                ride = meantRide;
                purse = meantPurse;
                boughtFor = true;
                why = NoHold.None;
                return meant;
            }
            if (markHolds)
            {
                ride = markRide;
                purse = markPurse;
                why = NoHold.None;
                return mark;
            }
            if (meant != null || mark != null) why = NoHold.OutOfReach;
            return null;
        }

        private static long PaysFor(Settlement s, EquipmentElement el, int units, float ride, int purse)
        {
            MobileParty party = MobileParty.MainParty;
            if (s?.SettlementComponent == null || party == null || units <= 0) return 0L;
            Paying pays = WhatThatMarketPays(s, s.SettlementComponent, el, party, ride);
            return TradeMath.WhatALadderPays(pays.At, units, purse);
        }

        private static bool InTheMarkedMarket(Settlement here) =>
            here != null && Options.Current.MarkBestSellTownOnMap &&
            (_picked == here || (here.StringId == _walkedInto && _markedOnTheWayIn == here.StringId));

        private static Settlement BoughtFor(ItemObject item)
        {
            if (item == null || !_intended.TryGetValue(item.StringId, out Meant meant) || meant.To == null) return null;
            if (Scoring.TooOldToSay(meant.Days, meant.AtHours, (float)CampaignTime.Now.ToHours, out _)) return null;
            return Settlement.Find(meant.To);
        }

        private static bool MayHoldFor(Settlement s) =>
            s?.SettlementComponent != null && TradeActionBehavior.IsMarket(s) &&
            !LedgerBehavior.UnderAttack(s) && !LedgerBehavior.VillageShut(s) &&
            !(Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(s));

        private static bool InReach(Settlement s, System.Func<Settlement, float> rideTo, out float ride, out int purse)
        {
            ride = rideTo(s);
            purse = PurseOf(s, ride);
            return LedgerBehavior.WithinTravelCeiling(s, ride) && purse > 0;
        }

        internal static (float room, int gold) WhatYourCargoFrees(Settlement site, float holdShare)
        {
            MobileParty party = MobileParty.MainParty;
            SettlementComponent market = site?.SettlementComponent;
            if (party == null || market == null) return (0f, 0);
            var cargo = WhatYouCarryToSell(party);
            if (cargo.Count == 0) return (0f, 0);
            float ride = Travel.EstimateDaysFromParty(site);
            if (TradeMath.OutOfReach(ride)) return (0f, 0);
            int purse = PurseOf(site, ride);
            if (purse <= 0) return (0f, 0);
            Takings took = WhatItWouldFetch(site, market, party, ride, cargo, purse, null,
                                            WhatTheMarkWouldKeep(site, ride, cargo, holdShare), holdShare);
            long gold = took.Value > purse ? purse : took.Value;
            return (took.Weight, gold > 0L ? (int)gold : 0);
        }

        private static int[][] WhatTheMarkWouldKeep(
            Settlement site, float ride,
            List<(EquipmentElement item, int amount, int worth, TradeMath.DearFirst floors)> cargo, float holdShare)
        {
            if (!(holdShare > 0f)) return null;
            MobileParty party = MobileParty.MainParty;
            SettlementComponent market = site.SettlementComponent;
            if (party == null || market == null) return null;
            var rungs = new int[cargo.Count][];
            var here = new int[cargo.Count][];
            var bought = new int[cargo.Count];
            var heldFor = new int[cargo.Count];
            var purses = new int[cargo.Count];
            var marks = new List<Settlement>();
            System.Func<Settlement, float> onwardTo = to => Travel.EstimateDaysBetween(site, to, ride);
            int till = PurseOf(site, ride);
            for (int at = 0; at < cargo.Count; at++)
            {
                heldFor[at] = -1;
                var (el, amount, worth, floors) = cargo[at];
                int units = System.Math.Max(0, amount - floors.AgedLeft);
                if (units <= 0) continue;
                Settlement mark = HoldFor(el, units, site, onwardTo, ride, out float onward, out int purse, out _, out _);
                if (mark == null) continue;
                int[] ladder = WhatTheMarkTakes(mark, el, units, worth, ride + onward, purse);
                if (ladder.Length == 0) continue;
                Paying pays = WhatThatMarketPays(site, market, el, party, ride);
                here[at] = TradeRules.WhatSellsHere(pays.At, units, worth, Options.Current.MinProfitMargin, ref till);
                rungs[at] = ladder;
                bought[at] = units;
                heldFor[at] = marks.IndexOf(mark);
                if (heldFor[at] < 0)
                {
                    marks.Add(mark);
                    heldFor[at] = marks.Count - 1;
                }
                purses[at] = purse;
            }
            if (marks.Count == 0) return null;
            int[] kept = TradeRules.SharePurses(purses, heldFor, rungs, here, bought, holdShare);
            for (int at = 0; at < cargo.Count; at++)
                if (rungs[at] != null && kept[at] < rungs[at].Length) System.Array.Resize(ref rungs[at], kept[at]);
            return rungs;
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
