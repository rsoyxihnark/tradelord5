using System.Collections.Generic;
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
        private Ladder _walk;
        private int _flat;
        private bool _asked;

        internal Paying(Settlement site, SettlementComponent market, EquipmentElement el,
                        MobileParty party)
        {
            _site = site;
            _market = market;
            _el = el;
            _party = party;
        }

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
                Ladder walk = _el.Item == null || _flat <= 0
                    ? null : new Ladder(_site, _el, true, _flat, 0);
                _walk = walk != null && walk.Walkable ? walk : null;
            }
            return _walk != null ? _walk.At(_rungs.Count) : _flat;
        }
    }

    internal static class Marker
    {
        private static Settlement _tracked;

        private static Settlement _picked;

        private static int _hour = -1;
        private static Vec2 _at;
        private const float MovedFar = 100f;

        private static Stamp _cargoStamp;
        private static int _cargoVersion = -1;
        private static List<(EquipmentElement item, int amount, int worth, int floor)> _cargo;
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
        }

        private struct Share
        {
            internal string Good;
            internal int Amount;
            internal int Moved;
            internal int Price;
            internal int Last;
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
            if (on) target = BestSellTownForCargo(out how);

            if (target == _picked)
            {
                if (target != null && !tracker.CheckTracked(target))
                {
                    tracker.RegisterObject(target);
                    _tracked = target;
                }
                if (target != null) SayItWeighedAgain(target, how);
                return;
            }
            if (_tracked != null && !LedgerPanel.IsPinned(_tracked) && tracker.CheckTracked(_tracked))
                tracker.RemoveTrackedObject(_tracked);
            _tracked = null;
            _picked = target;
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
            Remember(target, how);
        }

        private static void SayItWeighedAgain(Settlement target, in Reckoning how)
        {
            if (!Options.Current.ExtendedDebugLogging) return;
            if (how.Value == _saidValue && how.Rate == _saidRate) return;
            Log.Write("map marker weighed your cargo again and stayed on " + target.Name + ": " + Why(how));
            Ultra(how);
            Remember(target, how);
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
            float held = TradeMath.HeldShare(said > int.MaxValue ? int.MaxValue : (int)said, gold);
            float heldLast = TradeMath.HeldShare(last > int.MaxValue ? int.MaxValue : (int)last, gold);
            Log.Write("marker check at " + site.Name + ": it marked this market for " + _markedUnits +
                      " unit(s) worth " + said + " gold, " + _markedEarned + " of it profit, " +
                      _markedRate.ToString("0") +
                      " gold a day, about " + _markedDays.ToString("0.#") + " day(s) away; you walked in " +
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

        private static string Why(in Reckoning how)
        {
            if (how.Carried == 0) return "nothing in your cargo is yours to sell";
            if (how.Best == null)
                return "of the " + how.Weighed + " market(s) it looked at, " + how.Refused +
                       " would pay too little for any of the " + how.Carried + " good(s) you carry to " +
                       "clear Minimum profit margin, and the rest are past your travel ceilings or " +
                       "have no gold at all";
            return how.Kinds + " of the " + how.Carried + " good(s) you carry clear Minimum profit " +
                   "margin there, " + how.Units + " unit(s) for " + how.Value + " gold, " +
                   (how.Value - how.Cost) + " of it profit" +
                   (how.PurseCapped
                       ? ", which is all that town's purse of " + how.Purse + " can take"
                       : " against a town purse of " + how.Purse) +
                   ", about " + how.Days.ToString("0.#") + " day(s) away, so " +
                   how.Rate.ToString("0") + " gold a day" + TheNextBest(how);
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
                             one.Days.ToString("0.00").PadLeft(6) + " day(s) " +
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
                         how.Rate.ToString("0") + " gold a day");
                said.Add("  ultralog: left out before pricing, " + how.NoTill +
                         " with nothing in the till, " + how.PastCeiling + " past your travel ceilings, " +
                         how.NoRoad + " with no road it could find, " + how.Shut +
                         " under siege, raided or shut, " + how.AtWar + " at war with you");
            }
            if (said.Count > 0) Log.WriteMany(said);
        }

        private static List<(EquipmentElement item, int amount, int worth, int floor)> WhatYouCarryToSell(
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
            var cargo = new List<(EquipmentElement item, int amount, int worth, int floor)>();
            for (int i = 0; i < party.ItemRoster.Count; i++)
            {
                ItemRosterElement el = party.ItemRoster.GetElementCopyAtIndex(i);
                if (!TradePolicy.MaySell(el, locked, keepBack, awaited, out int keep)) continue;
                if (el.Amount - keep <= 0) continue;
                ItemObject item = el.EquipmentElement.Item;
                cargo.Add((el.EquipmentElement, el.Amount - keep,
                           TradePolicy.WorthToBeat(item), BestMarketFloor(el.EquipmentElement)));
            }
            _cargo = cargo;
            _cargoHeld = Held(cargo);
            return cargo;
        }

        private static List<(string good, int amount, bool food)> Held(
            List<(EquipmentElement item, int amount, int worth, int floor)> cargo)
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
                                                 EquipmentElement el, MobileParty party) =>
            new Paying(site, market, el, party);

        private static Takings WhatItWouldFetch(
            Settlement site, SettlementComponent market, MobileParty party,
            List<(EquipmentElement item, int amount, int worth, int floor)> cargo,
            int gold, List<Share> bill)
        {
            Takings took = default(Takings);
            foreach (var (item, amount, worth, floor) in cargo)
            {
                Paying pays = WhatThatMarketPays(site, market, item, party);
                long fetched = 0L;
                int moved = 0, opening = 0, last = 0;
                for (int u = 0; u < amount; u++)
                {
                    int price = pays.At(u);
                    if (price <= 0) break;
                    if (price < floor) break;
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
            List<(Settlement s, SettlementComponent market, int gold, float days)> reachable)
        {
            if (_picked == null) return;
            for (int at = 1; at < reachable.Count; at++)
            {
                if (reachable[at].s != _picked) continue;
                var held = reachable[at];
                reachable.RemoveAt(at);
                reachable.Insert(0, held);
                return;
            }
        }

        private static Settlement BestSellTownForCargo(out Reckoning how)
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
                if (LedgerBehavior.UnderAttack(s) || LedgerBehavior.VillageShut(s)) { how.Shut++; continue; }
                if (Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(s))
                { how.AtWar++; continue; }
                how.Weighed++;
                int purse = TradeRules.WhatTheTillCanPay(market.Gold, s.IsVillage);
                if (purse <= 0) { how.NoTill++; continue; }
                float cap = LedgerBehavior.TravelCeiling(s);
                if (cap > 0f && Travel.StraightDaysFromParty(s) > cap) { how.PastCeiling++; continue; }
                float ride = Travel.EstimateDaysFromParty(s);
                if (TradeMath.OutOfReach(ride)) { how.NoRoad++; continue; }
                if (cap > 0f && ride > cap) { how.PastCeiling++; continue; }
                reachable.Add((s, market, purse, ride));
            }
            reachable.Sort(FastestPurseFirst);
            TheMarkedTownFirst(reachable);
            if (ultra) how.Board = new List<Weighing>();

            float bar = 0f;
            for (int at = 0; at < reachable.Count; at++)
            {
                var (s, market, gold, ride) = reachable[at];
                if (TradeMath.PerDay(gold, ride) <= bar) break;
                how.Told++;
                Takings took = WhatItWouldFetch(s, market, party, cargo, gold, null);
                if (took.Value <= 0L) { how.Refused++; continue; }
                long total = took.Value > gold ? gold : took.Value;
                long earned = total - took.Cost;
                float rate = TradeMath.PerDay(earned, ride);
                float weighed = TradeMath.RateTheMarkHolds(rate, s == _picked);
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
                    how.Units = took.Units;
                    how.Kinds = took.Kinds;
                    how.Purse = gold;
                    how.PurseCapped = took.PurseCapped;
                    how.Cost = took.Cost;
                }
                else if (rate > how.RunnerUpRate)
                { how.RunnerUpRate = rate; how.RunnerUpValue = total; how.RunnerUp = s; }
            }
            how.Held = how.Best != null && how.Best == _picked && how.RunnerUpRate > how.Rate;
            how.Left = reachable.Count - how.Told;
            if (ultra && how.Best != null)
            {
                how.Bill = new List<Share>();
                WhatItWouldFetch(how.Best, how.Best.SettlementComponent, party, cargo, how.Purse, how.Bill);
            }
            return how.Best;
        }

        private static int BestMarketFloor(EquipmentElement held)
        {
            if (!Options.Current.PreferBestSellTown) return 0;
            var best = LedgerBehavior.Instance?.BestSell(held.Item) ?? (null, 0);
            return best.Item1 == null ? 0 : TradeRules.BestMarketFloor(
                TradeMath.AtThisQuality(best.Item2, held.Item.Value, held.ItemValue),
                Options.Current.BestSellTownTolerance);
        }
    }
}
