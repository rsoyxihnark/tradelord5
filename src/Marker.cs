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

        private static int _hour = -1;
        private static Vec2 _at;
        private const float MovedFar = 100f;

        private static Stamp _cargoStamp;
        private static int _cargoVersion = -1;
        private static List<(EquipmentElement item, int amount, int worth, int floor)> _cargo;

        private static Stamp _priceStamp;

        private static readonly Dictionary<(string site, string good, string quality), int> _prices =
            new Dictionary<(string, string, string), int>();

        internal static Settlement Tracked
        {
            get => _tracked;
            set => _tracked = value;
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
            _hour = -1;
        }

        internal static void Update()
        {
            VisualTrackerManager tracker = Campaign.Current?.VisualTrackerManager;
            if (tracker == null) return;
            Settlement target = null;
            string why = "the map marker is switched off";
            if (Options.Current.MarkBestSellTownOnMap) target = BestSellTownForCargo(out why);

            if (target == _tracked)
            {
                if (target != null && !tracker.CheckTracked(target))
                    tracker.RegisterObject(target);
                return;
            }
            if (_tracked != null && !LedgerPanel.IsPinned(_tracked) && tracker.CheckTracked(_tracked))
                tracker.RemoveTrackedObject(_tracked);
            _tracked = null;
            if (target != null && !tracker.CheckTracked(target))
            {
                tracker.RegisterObject(target);
                _tracked = target;
            }
            Log.Write(_tracked != null
                ? "map marker moved to " + _tracked.Name + ": " + why
                : "map marker taken off the map: " + why);
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
            if (!Freshness.Fresh(ref _priceStamp))
            {
                Freshness.Taken(ref _priceStamp);
                _prices.Clear();
            }
            var key = (site.StringId, good.StringId,
                       el.ItemModifier == null ? "" : el.ItemModifier.StringId);
            if (_prices.TryGetValue(key, out int kept)) return kept;
            int price = Priced.At(market, el, party, true);
            _prices[key] = price;
            return price;
        }

        private static Settlement BestSellTownForCargo(out string why)
        {
            why = "nothing in your cargo is yours to sell";
            MobileParty party = MobileParty.MainParty;
            if (party == null) return null;
            var cargo = WhatYouCarryToSell(party);
            if (cargo.Count == 0) return null;

            Settlement bestTown = null, runnerUp = null;
            long bestValue = 0, runnerUpValue = 0;
            int bestUnits = 0, bestKinds = 0, bestPurse = 0, weighed = 0, refused = 0;
            foreach (Settlement s in Settlement.All)
            {
                SettlementComponent market = s.SettlementComponent;
                if (market == null) continue;
                if (s == party.CurrentSettlement) continue;
                if (TradeActionBehavior.StillTheSameArrival(s)) continue;
                if (!TradeActionBehavior.IsMarket(s)) continue;
                if (LedgerBehavior.UnderAttack(s) || LedgerBehavior.VillageShut(s)) continue;
                if (Options.Current.ExcludeHostileTowns && LedgerBehavior.IsHostile(s)) continue;
                weighed++;
                if (market.Gold <= bestValue) continue;
                float cap = LedgerBehavior.TravelCeiling(s);
                if (cap > 0f)
                {
                    if (Travel.StraightDaysFromParty(s) > cap) continue;
                    if (Travel.EstimateDaysFromParty(s) > cap) continue;
                }
                long total = 0;
                int units = 0, kinds = 0;
                foreach (var (item, amount, worth, floor) in cargo)
                {
                    int price = WhatThatMarketPays(s, market, item, party);
                    if (price < floor) continue;
                    if (!TradeMath.ProfitAcceptable(worth, price, Options.Current.MinProfitMargin)) continue;
                    total += (long)price * amount;
                    units += amount;
                    kinds++;
                }
                if (total <= 0) { refused++; continue; }
                if (total > market.Gold) total = market.Gold;
                if (total > bestValue)
                {
                    runnerUp = bestTown;
                    runnerUpValue = bestValue;
                    bestValue = total;
                    bestTown = s;
                    bestUnits = units;
                    bestKinds = kinds;
                    bestPurse = market.Gold;
                }
                else if (total > runnerUpValue) { runnerUpValue = total; runnerUp = s; }
            }
            why = bestTown == null
                ? "of the " + weighed + " market(s) it looked at, " + refused + " would pay too little for any " +
                  "of the " + cargo.Count + " good(s) you carry to clear Minimum profit margin, and the rest are " +
                  "past your travel ceilings or have no gold at all"
                : bestKinds + " of the " + cargo.Count + " good(s) you carry clear Minimum profit margin there, " +
                  bestUnits + " unit(s) for " + bestValue + " gold against a town purse of " + bestPurse +
                  ", about " + Travel.EstimateDaysFromParty(bestTown).ToString("0.#") + " day(s) away" +
                  (runnerUp == null
                      ? ", and no other market it priced would take any of it"
                      : ", ahead of " + runnerUp.Name + ", the next best it priced, at " +
                        runnerUpValue + " gold");
            return bestTown;
        }

        private static int BestMarketFloor(ItemObject item)
        {
            if (!Options.Current.PreferBestSellTown) return 0;
            var best = LedgerBehavior.Instance?.BestSell(item) ?? (null, 0);
            return best.Item1 == null ? 0 : (int)(best.Item2 * Options.Current.BestSellTownTolerance);
        }
    }
}
