using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace TradeLord
{
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

        private static Stamp _priceStamp;

        private const int PriceShelfHours = 3;

        private static readonly Dictionary<(string site, string good, string quality), int> _prices =
            new Dictionary<(string, string, string), int>();

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
        }

        internal static void ForgetTheRead()
        {
            ForgetWhatYouCarry();
            _prices.Clear();
            _priceStamp.Stale();
        }

        internal static void Forget()
        {
            ForgetTheRead();
            _tracked = null;
            _picked = null;
            _hour = -1;
        }

        private struct Reckoning
        {
            internal Settlement Best;
            internal Settlement RunnerUp;
            internal long Value;
            internal long RunnerUpValue;
            internal int Units;
            internal int Kinds;
            internal int Purse;
            internal int Carried;
            internal int Weighed;
            internal int Refused;
            internal bool PurseCapped;
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
                   "margin there, " + how.Units + " unit(s) for " + how.Value + " gold" +
                   (how.PurseCapped
                       ? ", which is all that town's purse of " + how.Purse + " can take"
                       : " against a town purse of " + how.Purse) +
                   ", about " + Travel.EstimateDaysFromParty(how.Best).ToString("0.#") + " day(s) away" +
                   (how.RunnerUp == null
                       ? ", and no other market it priced would take any of it"
                       : ", ahead of " + how.RunnerUp.Name + ", the next best it priced, at " +
                         how.RunnerUpValue + " gold");
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
                           TradePolicy.WorthToBeat(item), BestMarketFloor(item)));
            }
            _cargo = cargo;
            return cargo;
        }

        private static int WhatThatMarketPays(Settlement site, SettlementComponent market,
                                              EquipmentElement el, MobileParty party)
        {
            ItemObject good = el.Item;
            if (good == null) return Priced.At(market, el, party, true);
            int shelf = Freshness.Hour / PriceShelfHours;
            if (!Freshness.Fresh(ref _priceStamp, shelf))
            {
                Freshness.Taken(ref _priceStamp, shelf);
                _prices.Clear();
            }
            var key = (site.StringId, good.StringId,
                       el.ItemModifier == null ? "" : el.ItemModifier.StringId);
            if (_prices.TryGetValue(key, out int kept)) return kept;
            int price = Priced.At(market, el, party, true);
            _prices[key] = price;
            return price;
        }

        private static readonly System.Comparison<(Settlement s, SettlementComponent market, int gold)>
            DearestPurseFirst = (x, y) =>
                x.gold != y.gold ? y.gold.CompareTo(x.gold)
                                 : string.CompareOrdinal(x.s.StringId, y.s.StringId);

        private static Settlement BestSellTownForCargo(out Reckoning how)
        {
            how = default(Reckoning);
            MobileParty party = MobileParty.MainParty;
            if (party == null) return null;
            var cargo = WhatYouCarryToSell(party);
            how.Carried = cargo.Count;
            if (cargo.Count == 0) return null;

            var reachable = new List<(Settlement s, SettlementComponent market, int gold)>();
            foreach (Settlement s in Settlement.All)
            {
                SettlementComponent market = s.SettlementComponent;
                if (market == null) continue;
                if (s == party.CurrentSettlement) continue;
                if (TradeActionBehavior.StillTheSameArrival(s)) continue;
                if (!TradeActionBehavior.IsMarket(s)) continue;
                if (LedgerBehavior.UnderAttack(s) || LedgerBehavior.VillageShut(s)) continue;
                if (Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(s)) continue;
                how.Weighed++;
                int purse = TradeRules.WhatTheTillCanPay(market.Gold, s.IsVillage);
                if (purse <= 0) continue;
                reachable.Add((s, market, purse));
            }
            reachable.Sort(DearestPurseFirst);

            for (int at = 0; at < reachable.Count; at++)
            {
                var (s, market, gold) = reachable[at];
                if (gold <= how.Value) break;
                float cap = LedgerBehavior.TravelCeiling(s);
                if (cap > 0f && Travel.StraightDaysFromParty(s) > cap) continue;
                float ride = Travel.EstimateDaysFromParty(s);
                if (TradeMath.OutOfReach(ride)) continue;
                if (cap > 0f && ride > cap) continue;
                long total = 0;
                int units = 0, kinds = 0;
                bool capped = false;
                foreach (var (item, amount, worth, floor) in cargo)
                {
                    int price = WhatThatMarketPays(s, market, item, party);
                    if (price < floor) continue;
                    if (!TradeMath.ProfitAcceptable(worth, price, Options.Current.MinProfitMargin)) continue;
                    total += (long)price * amount;
                    units += amount;
                    kinds++;
                    if (total >= gold) { capped = true; break; }
                }
                if (total <= 0) { how.Refused++; continue; }
                if (total > gold) total = gold;
                if (total > how.Value)
                {
                    how.RunnerUp = how.Best;
                    how.RunnerUpValue = how.Value;
                    how.Value = total;
                    how.Best = s;
                    how.Units = units;
                    how.Kinds = kinds;
                    how.Purse = gold;
                    how.PurseCapped = capped;
                }
                else if (total > how.RunnerUpValue) { how.RunnerUpValue = total; how.RunnerUp = s; }
            }
            return how.Best;
        }

        private static int BestMarketFloor(ItemObject item)
        {
            if (!Options.Current.PreferBestSellTown) return 0;
            var best = LedgerBehavior.Instance?.BestSell(item) ?? (null, 0);
            return best.Item1 == null ? 0 : (int)(best.Item2 * Options.Current.BestSellTownTolerance);
        }
    }
}
