using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TradeLord
{
    internal static class Drove
    {
        private static MethodInfo _modifier;
        private static bool _lookupFailed;

        internal static void Forget()
        {
            _lookupFailed = false;
            _packLineUnread = false;
            _eachUnread = false;
        }

        private static bool Tally(MobileParty party, out int men, out int herd,
                                  out int mounts, out int foot)
        {
            men = 0; herd = 0; mounts = 0; foot = 0;
            ItemRoster roster = party?.ItemRoster;
            if (roster == null) return false;
            men = party.MemberRoster?.TotalManCount ?? 0;
            herd = roster.NumberOfPackAnimals + roster.NumberOfLivestockAnimals;
            mounts = roster.NumberOfMounts;
            foot = party.Party?.NumberOfMenWithoutHorse ?? 0;
            var attached = party.AttachedParties;
            for (int i = 0; attached != null && i < attached.Count; i++)
            {
                MobileParty a = attached[i];
                if (a?.ItemRoster == null) continue;
                herd += a.ItemRoster.NumberOfPackAnimals + a.ItemRoster.NumberOfLivestockAnimals;
                mounts += a.ItemRoster.NumberOfMounts;
                men += a.MemberRoster?.TotalManCount ?? 0;
                foot += a.Party?.NumberOfMenWithoutHorse ?? 0;
            }
            return men > 0;
        }

        internal static int RoomForLivestock(MobileParty party)
        {
            try
            {
                if (party == null) return 0;
                DefaultPartySpeedCalculatingModel model = Model();
                if (model == null) return 0;
                if (!Tally(party, out int men, out int herd, out int mounts, out int foot)) return 0;
                herd = Herding.DrivenInAll(herd, mounts, foot);
                float neutral = (float)_modifier.Invoke(model, new object[] { men, 0 });
                return TradeMath.MostThatHolds(256, room => room == 0 || TradeMath.Unchanged(
                    (float)_modifier.Invoke(model,
                        new object[] { men, herd + room + Herding.Cushion }), neutral));
            }
            catch (Exception e)
            {
                if (!_lookupFailed) { _lookupFailed = true; Log.Error(e, "herd guard (the herd cannot be counted, so no livestock and no haul animals are bought and no animal is sold to get you back up to speed; every other trade is unaffected)"); }
                return 0;
            }
        }

        internal static string PenaltyRead() =>
            Model() == null ? "herd penalty not read" : "herd penalty read";

        private static DefaultPartySpeedCalculatingModel Model()
        {
            if (_lookupFailed) return null;
            var models = Campaign.Current?.Models;
            if (models == null) return null;
            var model = models.PartySpeedCalculatingModel as DefaultPartySpeedCalculatingModel;
            if (model == null)
            {
                _lookupFailed = true;
                Log.Write("herd guard: a mod replaced the party speed model - " +
                          "the herd cannot be counted, so no livestock and no haul animals are bought and no animal is sold to get you back up to speed; every other trade is unaffected");
                return null;
            }
            if (_modifier == null)
            {
                _modifier = typeof(DefaultPartySpeedCalculatingModel).GetMethod(
                    "GetHerdingModifier", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_modifier == null)
                {
                    _lookupFailed = true;
                    Log.Write("herd guard: GetHerdingModifier not found on this game version - " +
                              "the herd cannot be counted, so no livestock and no haul animals are bought and no animal is sold to get you back up to speed; every other trade is unaffected");
                    return null;
                }
            }
            return model;
        }

        internal static int AnimalsToShed(MobileParty party)
        {
            try
            {
                if (party == null) return 0;
                DefaultPartySpeedCalculatingModel model = Model();
                if (model == null) return 0;
                if (!Tally(party, out int men, out int herd, out int mounts, out int foot)) return 0;
                int driven = Herding.DrivenInAll(herd, mounts, foot);
                if (driven <= 0) return 0;
                float neutral = (float)_modifier.Invoke(model, new object[] { men, 0 });
                return TradeMath.MostThatHolds(driven, shed => shed == 0 || !TradeMath.Unchanged(
                    (float)_modifier.Invoke(model, new object[] { men, driven - shed + 1 }), neutral));
            }
            catch (Exception e)
            {
                if (!_lookupFailed)
                {
                    _lookupFailed = true;
                    Log.Error(e, "herd relief check (no animal is sold)");
                }
                return 0;
            }
        }

        private static void Split(MobileParty party, out int packs, out int stock)
        {
            packs = 0; stock = 0;
            ItemRoster roster = party?.ItemRoster;
            if (roster == null) return;
            packs = roster.NumberOfPackAnimals;
            stock = roster.NumberOfLivestockAnimals;
            var attached = party.AttachedParties;
            for (int i = 0; attached != null && i < attached.Count; i++)
            {
                ItemRoster other = attached[i]?.ItemRoster;
                if (other == null) continue;
                packs += other.NumberOfPackAnimals;
                stock += other.NumberOfLivestockAnimals;
            }
        }

        internal static void LogState(string when) => LogState(when, -1);

        internal static void LogState(string when, int counted)
        {
            try
            {
                MobileParty party = MobileParty.MainParty;
                if (party == null) return;
                if (!Tally(party, out int men, out int herd, out int mounts, out int foot)) return;
                Split(party, out int packs, out int stock);
                int spare = Herding.MountsNobodyRides(mounts, foot);
                int shed = counted >= 0 ? counted : AnimalsToShed(party);
                Log.Write("herd check (" + when + "): " + men + " men of whom " + foot + " on foot, " +
                          mounts + " loose mount(s) with " + spare + " nobody rides, " +
                          packs + " pack animal(s), " + stock + " livestock, " +
                          Herding.DrivenInAll(herd, mounts, foot) + " driven in all, " +
                          (shed > 0
                              ? "the herd is slowing you down and " + shed + " must go"
                              : _lookupFailed
                                  ? "the herd penalty cannot be read on this game version"
                                  : "no herd penalty"));
            }
            catch (Exception e) { Log.Error(e, "herd check log (nothing else is affected)"); }
        }

        internal static int SpareMounts(MobileParty party)
        {
            try
            {
                return Tally(party, out _, out _, out int mounts, out int foot)
                    ? Herding.MountsNobodyRides(mounts, foot) : 0;
            }
            catch (Exception e)
            {
                Log.Error(e, "spare mount count (no mount is sold to relieve the herd)");
                return 0;
            }
        }

        internal static int HaulAnimalsHeld(MobileParty party)
        {
            ItemRoster roster = party?.ItemRoster;
            if (roster == null) return 0;
            int held = 0;
            for (int i = 0; i < roster.Count; i++)
            {
                ItemRosterElement el = roster.GetElementCopyAtIndex(i);
                if (el.Amount > 0 && TradePolicy.IsHaulAnimal(el.EquipmentElement.Item)) held += el.Amount;
            }
            return held;
        }

        private static bool _packLineUnread;
        private static bool _eachUnread;

        private static bool PackAnimalsLine(ExplainedNumber capacity, out float carries)
        {
            carries = 0f;
            string named = (typeof(DefaultInventoryCapacityModel).GetField(
                                "_textPackAnimals", BindingFlags.Static | BindingFlags.NonPublic)
                                ?.GetValue(null) as TextObject)?.ToString();
            if (string.IsNullOrEmpty(named)) return false;
            foreach (var (name, number) in capacity.GetLines())
                if (name == named) carries += number;
            return carries > 0f;
        }

        internal static float CargoAHaulAnimalAdds(MobileParty party)
        {
            if (party == null || Carry.Sailing()) return 0f;
            try
            {
                InventoryCapacityModel model = Campaign.Current?.Models?.InventoryCapacityModel;
                if (model == null) return 0f;
                ExplainedNumber capacity = model.CalculateInventoryCapacity(party, false, true);
                int packAnimals = party.ItemRoster.NumberOfPackAnimals;
                float carries = 0f;
                if (packAnimals > 0 && !PackAnimalsLine(capacity, out carries) && !_eachUnread)
                {
                    _eachUnread = true;
                    Log.Write("haul animal cargo: the game did not say how much its pack animals carry, " +
                              "so one is counted as carrying what the game gives a pack animal before perks");
                }
                return Herding.CargoAHaulAnimalAdds(carries, packAnimals, model.GetItemAverageWeight(),
                                                    capacity.BaseNumber, capacity.ResultNumber);
            }
            catch (Exception e)
            {
                Log.Error(e, "cargo a haul animal adds (no haul animal is bought)");
                return 0f;
            }
        }

        internal static float CargoASpareMountAdds(MobileParty party)
        {
            if (party == null || Carry.Sailing()) return 0f;
            try
            {
                InventoryCapacityModel model = Campaign.Current?.Models?.InventoryCapacityModel;
                if (model == null) return 0f;
                ExplainedNumber capacity = model.CalculateInventoryCapacity(party, false);
                return Herding.CargoASpareMountAdds(model.GetItemAverageWeight(), capacity.BaseNumber,
                                                    capacity.ResultNumber);
            }
            catch (Exception e)
            {
                Log.Error(e, "cargo a spare mount adds (a dry run counts no room lost to a mount it would sell)");
                return 0f;
            }
        }

        internal static int HaulAnimalsCargoCanSpare(MobileParty party)
        {
            int held = HaulAnimalsHeld(party);
            if (held <= 0) return 0;
            try
            {
                InventoryCapacityModel model = Campaign.Current?.Models?.InventoryCapacityModel;
                if (model == null) return 0;
                bool atSea = Carry.Sailing();
                if (atSea) return held;
                ExplainedNumber capacity = model.CalculateInventoryCapacity(party, atSea, true);
                float carried = model.CalculateTotalWeightCarried(party, atSea).ResultNumber;
                int packAnimals = party.ItemRoster.NumberOfPackAnimals;
                if (!PackAnimalsLine(capacity, out float carries) && !atSea && packAnimals > 0)
                {
                    if (!_packLineUnread)
                    {
                        _packLineUnread = true;
                        Log.Write("haul animal cargo floor: the game did not say how much its pack animals carry, " +
                                  "so every haul animal is kept");
                    }
                    return 0;
                }
                return Herding.HaulAnimalsToSpare(held, packAnimals, carries, capacity.BaseNumber,
                                                  capacity.ResultNumber, carried);
            }
            catch (Exception e)
            {
                Log.Error(e, "haul animal cargo floor (every haul animal is kept)");
                return 0;
            }
        }

        internal static int ShedRank(ItemObject item) =>
            item == null ? TradeRules.RankNotAnAnimal
                         : TradeRules.HerdShedRank(TradePolicy.Describe(item));

        internal static void SayWhatItWillNotGiveUp(ItemRoster mine, int shed, Settlement settlement)
        {
            var kinds = new List<string>();
            var marked = new List<string>();
            for (int i = 0; i < mine.Count; i++)
            {
                ItemRosterElement el = mine.GetElementCopyAtIndex(i);
                ItemObject it = el.EquipmentElement.Item;
                if (el.Amount <= 0 || it == null || !it.HasHorseComponent) continue;
                int rank = ShedRank(it);
                if (rank >= 0)
                {
                    if (!Herding.TheGameCountsItAtOnce(rank == TradeRules.RankLivestock,
                                                       el.EquipmentElement.ItemModifier != null))
                        marked.Add(LedgerBehavior.PaidKey(el.EquipmentElement) + " x" + el.Amount);
                    continue;
                }
                kinds.Add(it.StringId + " x" + el.Amount);
            }
            Log.Repeatable("herd-stuck " + settlement.StringId, shed + "/" + kinds.Count + "/" + marked.Count,
                           "herd relief at " + settlement.Name + " has " + shed +
                           " animal(s) to shed and nothing it may sell" +
                           (kinds.Count == 0 && marked.Count == 0
                               ? ", because every animal you drive is held back by your own rules"
                               : "") +
                           (kinds.Count == 0
                               ? ""
                               : "; these are driven but TradeLord counts them as ordinary cargo, so it never sells them: " +
                                 string.Join(", ", kinds.ToArray())) +
                           (marked.Count == 0
                               ? ""
                               : "; these are lame or of another quality, and the game does not count one gone until " +
                                 "you load again, so selling it would not get you back up to speed: " +
                                 string.Join(", ", marked.ToArray())));
        }
    }
}
