using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;

namespace TradeLord
{
    internal static class Errands
    {
        private static readonly (Type quest, string wanted, string many)[] Named =
        {
            (typeof(HeadmanNeedsToDeliverAHerdIssueBehavior.HeadmanNeedsToDeliverAHerdIssueQuest),
                "_herdTypeToDeliver", "_animalCountToDeliver"),
            (typeof(HeadmanVillageNeedsDraughtAnimalsIssueBehavior.HeadmanVillageNeedsDraughtAnimalsIssueQuest),
                "_requestedAnimal", "_requestedAnimalAmount"),
            (typeof(LordNeedsHorsesIssueBehavior.LordNeedsHorsesIssueQuest),
                "_mountObjectToBeDelivered", "_numMountsToBeDelivered"),
            (typeof(ArtisanOverpricedGoodsIssueBehavior.ArtisanOverpricedGoodsIssueQuest),
                "_requestedTradeGood", "_requestedTradeGoodAmount"),
            (typeof(ArtisanCantSellProductsAtAFairPriceIssueBehavior.ArtisanCantSellProductsAtAFairPriceIssueQuest),
                "_rawMaterialsToBeDelivered", "_amountOfRawGoodsToBeDelivered"),
            (typeof(LandLordTheArtOfTheTradeIssueBehavior.LandLordTheArtOfTheTradeIssueQuest),
                "_selectedItemObject", "_selectedItemObjectCount"),
            (typeof(VillageNeedsToolsIssueBehavior.VillageNeedsToolsIssueQuest),
                "_requestedTradeGood", "_numberOfRequestedGood"),
            (typeof(VillageNeedsCraftingMaterialsIssueBehavior.VillageNeedsCraftingMaterialsIssueQuest),
                "_requestedItem", "_requestedItemAmount"),
        };

        private static readonly (Type quest, string goodId, string many)[] NamedGoods =
        {
            (typeof(ArmyNeedsSuppliesIssueBehavior.ArmyNeedsSuppliesIssueQuest),
                "grain", "_requestedGrainAmount"),
            (typeof(ArmyNeedsSuppliesIssueBehavior.ArmyNeedsSuppliesIssueQuest),
                "wine", "_requestedWineAmount"),
            (typeof(HeadmanNeedsGrainIssueBehavior.HeadmanNeedsGrainIssueQuest),
                "grain", "_neededGrainAmount"),
        };

        private static readonly (Type quest, string many)[] NamedHerds =
        {
            (typeof(ArmyNeedsSuppliesIssueBehavior.ArmyNeedsSuppliesIssueQuest),
                "_requestedLiveStockAmount"),
        };

        private static readonly (Type quest, string kind, string many)[] NamedWeapons =
        {
            (typeof(GangLeaderNeedsWeaponsIssueQuestBehavior.GangLeaderNeedsWeaponsIssueQuest),
                "_requestedWeaponClass", "_requestedWeaponAmount"),
        };

        private static (Type quest, FieldInfo wanted, FieldInfo many)[] _read;
        private static (Type quest, string goodId, FieldInfo many)[] _readGoods;
        private static (Type quest, FieldInfo many)[] _readHerds;
        private static (Type quest, FieldInfo kind, FieldInfo many)[] _readWeapons;
        private static Dictionary<string, ItemObject> _goods;
        private static bool _allRead;
        private static bool _animalsRead;

        internal static void Forget() { _read = null; _readGoods = null; _readHerds = null; _readWeapons = null; _goods = null; }

        private static ItemObject Good(string id)
        {
            if (_goods == null)
            {
                _goods = new Dictionary<string, ItemObject>(StringComparer.Ordinal);
                foreach (ItemObject item in Items.All)
                    if (item?.StringId != null) _goods[item.StringId] = item;
            }
            return _goods.TryGetValue(id, out ItemObject found) ? found : null;
        }

        internal static bool Known { get { Read(); return _allRead; } }

        internal static bool AnimalsKnown { get { Read(); return _animalsRead; } }

        private static void Read()
        {
            if (_read != null) return;
            bool allRead = true, animalsRead = true;
            var found = new List<(Type, FieldInfo, FieldInfo)>();
            for (int i = 0; i < Named.Length; i++)
            {
                FieldInfo wanted = Named[i].quest.GetField(
                    Named[i].wanted, BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo many = Named[i].quest.GetField(
                    Named[i].many, BindingFlags.Instance | BindingFlags.NonPublic);
                if (wanted == null || many == null)
                {
                    allRead = animalsRead = false;
                    Log.Write("quest goods: " + Named[i].quest.Name + " does not say which good it wants " +
                              "or how many on this game version - what it asks for is not held back, and no " +
                              "animal is sold at all, so a quest of yours cannot lose one");
                    continue;
                }
                found.Add((Named[i].quest, wanted, many));
            }
            var byGood = new List<(Type, string, FieldInfo)>();
            for (int i = 0; i < NamedGoods.Length; i++)
            {
                FieldInfo many = NamedGoods[i].quest.GetField(
                    NamedGoods[i].many, BindingFlags.Instance | BindingFlags.NonPublic);
                if (many == null)
                {
                    allRead = false;
                    Log.Write("quest goods: " + NamedGoods[i].quest.Name + " does not say how much " +
                              NamedGoods[i].goodId + " it wants on this game version - that " +
                              NamedGoods[i].goodId + " is not held back for it");
                    continue;
                }
                byGood.Add((NamedGoods[i].quest, NamedGoods[i].goodId, many));
            }
            var byHerd = new List<(Type, FieldInfo)>();
            for (int i = 0; i < NamedHerds.Length; i++)
            {
                FieldInfo many = NamedHerds[i].quest.GetField(
                    NamedHerds[i].many, BindingFlags.Instance | BindingFlags.NonPublic);
                if (many == null)
                {
                    allRead = animalsRead = false;
                    Log.Write("quest goods: " + NamedHerds[i].quest.Name + " does not say how much livestock " +
                              "it wants on this game version - no animal is sold at all, so a quest of yours " +
                              "cannot lose one");
                    continue;
                }
                byHerd.Add((NamedHerds[i].quest, many));
            }
            var byWeapon = new List<(Type, FieldInfo, FieldInfo)>();
            for (int i = 0; i < NamedWeapons.Length; i++)
            {
                FieldInfo kind = NamedWeapons[i].quest.GetField(
                    NamedWeapons[i].kind, BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo many = NamedWeapons[i].quest.GetField(
                    NamedWeapons[i].many, BindingFlags.Instance | BindingFlags.NonPublic);
                if (kind == null || many == null)
                {
                    allRead = false;
                    Log.Write("quest goods: " + NamedWeapons[i].quest.Name + " does not say which kind of weapon " +
                              "it wants or how many on this game version - those weapons are not held back for it");
                    continue;
                }
                byWeapon.Add((NamedWeapons[i].quest, kind, many));
            }
            _readGoods = byGood.ToArray();
            _readHerds = byHerd.ToArray();
            _readWeapons = byWeapon.ToArray();
            _allRead = allRead;
            _animalsRead = animalsRead;
            _read = found.ToArray();
        }

        internal static Dictionary<ItemObject, int> Promised(out int anyLivestock,
                                                            out List<(WeaponClass kind, int many)> weapons)
        {
            anyLivestock = 0;
            weapons = new List<(WeaponClass kind, int many)>();
            Read();
            var promised = new Dictionary<ItemObject, int>();
            var running = Campaign.Current?.QuestManager?.Quests;
            if (running == null) return promised;
            foreach (QuestBase quest in running)
            {
                if (quest == null || quest.IsFinalized) continue;
                for (int i = 0; i < _read.Length; i++)
                {
                    if (!_read[i].quest.IsInstanceOfType(quest)) continue;
                    if (_read[i].wanted.GetValue(quest) is ItemObject wanted &&
                        _read[i].many.GetValue(quest) is int many && many > 0)
                    {
                        promised.TryGetValue(wanted, out int had);
                        promised[wanted] = had + many;
                    }
                    break;
                }
                for (int i = 0; i < _readGoods.Length; i++)
                {
                    if (!_readGoods[i].quest.IsInstanceOfType(quest)) continue;
                    if (!(_readGoods[i].many.GetValue(quest) is int many) || many <= 0) continue;
                    ItemObject wanted = Good(_readGoods[i].goodId);
                    if (wanted == null) continue;
                    promised.TryGetValue(wanted, out int had);
                    promised[wanted] = had + many;
                }
                for (int i = 0; i < _readHerds.Length; i++)
                {
                    if (!_readHerds[i].quest.IsInstanceOfType(quest)) continue;
                    if (_readHerds[i].many.GetValue(quest) is int owed && owed > 0) anyLivestock += owed;
                }
                for (int i = 0; i < _readWeapons.Length; i++)
                {
                    if (!_readWeapons[i].quest.IsInstanceOfType(quest)) continue;
                    if (_readWeapons[i].kind.GetValue(quest) is WeaponClass kind &&
                        _readWeapons[i].many.GetValue(quest) is int many && many > 0)
                    {
                        int at = weapons.FindIndex(owed => owed.kind == kind);
                        if (at < 0) weapons.Add((kind, many));
                        else weapons[at] = (kind, weapons[at].many + many);
                    }
                }
            }
            return promised;
        }
    }


    internal static class Meetings
    {
        private const int GetawayHours = 4;

        private static MobileParty _tradedWith;
        private static object _tradedIn;
        private static object _handledEncounter;
        private static MobileParty _offerTakenFrom;
        private static object _offerTakenIn;

        internal static void Lines(CampaignGameStarter starter)
        {
            AddCaravanLines(starter);
            AddBanditLines(starter);
        }

        private static void AddCaravanLines(CampaignGameStarter starter) => Guard.Run(
            "caravan dialog (trading in a market is unaffected)", () =>
            {
                const string said = "{=TL114}That was a nice trade. [TRADELORD]";
                const string answered = "{=TL115}Agreed. I wish I could use that mod too. Hope you gave a thumbs up endorsement on NexusMods!";
                starter.AddPlayerLine("tradelord_caravan_done", "caravan_talk", "tradelord_caravan_reply",
                    Tongue.Slot(said),
                    () => Tongue.Spoken(said) && CaravanMet(), null, 200);
                starter.AddDialogLine("tradelord_caravan_reply", "tradelord_caravan_reply", "close_window",
                    Tongue.Slot(answered),
                    () => Tongue.Spoken(answered), null, 200);
            });

        private static bool CaravanMet()
        {
            MobileParty caravan = MobileParty.ConversationParty;
            if (!Options.Current.TradeWithCaravans || caravan == null || !caravan.IsCaravan) return false;
            TradeOnce(caravan);
            return true;
        }

        private static void AddBanditLines(CampaignGameStarter starter) => Guard.Run(
            "bandit dialog (meeting a band is otherwise unaffected)", () =>
            {
                const string said = "{=TL387}Let us pass, and we will be on our way. [TRADELORD]";
                const string answered = "{=TL113}Oh, sorry, of course. But do not forget to leave an endorsement thumbs up on NexusMods!";
                ConversationSentence asked = starter.AddPlayerLine(
                    "tradelord_bandit_pass", Parley.OwnState, "tradelord_bandit_pass_reply",
                    Tongue.Slot(said),
                    () => Tongue.Spoken(said) && BanditMet(), null, 200);
                starter.AddDialogLine("tradelord_bandit_pass_reply", "tradelord_bandit_pass_reply", "close_window",
                    Tongue.Slot(answered),
                    () => Tongue.Spoken(answered),
                    () => Guard.Run("Action.Getaway", LetPlayerGo), 200);
                Parley.Remember(asked);
            });

        private static bool BanditMet()
        {
            MobileParty band = MobileParty.ConversationParty;
            return Options.Current.BanditFreePassage && band != null && band.IsBandit;
        }

        private static void TradeOnce(MobileParty met)
        {
            object here = PlayerEncounter.Current;
            if (_tradedWith == met || (here != null && _tradedIn == here)) return;
            _tradedWith = met;
            _tradedIn = here;
            Guard.Run("Action.RoadTrade", () => TradeActionBehavior.ExecuteRoadTrade(met));
        }

        internal static bool IsRoadTrader(MobileParty party) =>
            party != null && (party.IsCaravan || party.IsVillager);

        internal static bool CarriesGoods(MobileParty party)
        {
            ItemRoster goods = party?.ItemRoster;
            for (int at = 0; goods != null && at < goods.Count; at++)
                if (goods.GetElementNumber(at) > 0) return true;
            return false;
        }

        internal static void Watch()
        {
            if (Campaign.Current == null) { _handledEncounter = null; Parley.Forget(); return; }
            object here = PlayerEncounter.Current;
            if (here == null) { _handledEncounter = null; return; }
            if (_handledEncounter == here) return;
            MobileParty met = PlayerEncounter.EncounteredMobileParty;
            if (met == null) return;
            _handledEncounter = here;
            if (IsRoadTrader(met)) TradeOnce(met);
        }

        internal static void ForgetEncounter()
        {
            TradeActionBehavior.ForgetTheMeeting();
            ForgetWhoYouTradedWith();
            _handledEncounter = null;
        }

        internal static void ForgetWhoYouTradedWith()
        {
            _tradedWith = null;
            _tradedIn = null;
            _offerTakenFrom = null;
            _offerTakenIn = null;
        }

        internal static bool TheirOfferIsGuarded() =>
            Patcher.Holds(nameof(Patch_VillagerOfferShown)) && Patcher.Holds(nameof(Patch_VillagerOfferTaken));

        internal static void TookTheirOffer(MobileParty met)
        {
            _offerTakenFrom = met;
            _offerTakenIn = PlayerEncounter.Current;
        }

        internal static bool TheirOfferIsTaken(MobileParty met) =>
            met != null && met == _offerTakenFrom &&
            _offerTakenIn != null && _offerTakenIn == PlayerEncounter.Current;

        internal static List<(string id, int units, int price)> WhatTheyOffer(MobileParty met)
        {
            Village home = met != null && met.IsVillager ? met.HomeSettlement?.Village : null;
            if (home == null || MobileParty.MainParty == null) return null;
            var priced = new TheirOffer(home);
            var offered = new List<(string id, int units, int price)>();
            ItemRoster goods = met.ItemRoster;
            for (int at = 0; at < goods.Count; at++)
            {
                ItemRosterElement el = goods.GetElementCopyAtIndex(at);
                ItemObject item = el.EquipmentElement.Item;
                if (item == null || el.Amount <= 0 || item.ItemCategory == DefaultItemCategories.PackAnimal) continue;
                offered.Add((LedgerBehavior.PaidKey(el.EquipmentElement), el.Amount,
                             priced.GetPrice(el.EquipmentElement, MobileParty.MainParty, isSelling: true, met.Party)));
            }
            return offered;
        }

        internal static void YouTookTheirOffer(List<(string id, int units, int price)> offered, int paid)
        {
            if (offered == null || offered.Count == 0 || paid <= 0) return;
            int asked = 0;
            foreach (var line in offered) asked += TradeMath.WorthOf(line.units, line.price);
            if (!Deals.AddsUp(asked, paid))
            {
                Log.Write("you took the villagers' offer yourself and paid " + paid + " gold where their " +
                          "goods came to " + asked + ", so what you paid is not written down against them");
                return;
            }
            foreach (var line in offered)
                LedgerBehavior.Instance?.RecordPurchase(line.id, line.units, TradeMath.WorthOf(line.units, line.price));
            Log.Write("you took the villagers' offer yourself: " + paid + " gold for " + offered.Count +
                      " kind(s) of goods, written down as what you paid for them");
        }

        internal static void ConversationEnded() => _tradedWith = null;

        private static void LetPlayerGo()
        {
            MobileParty band = MobileParty.ConversationParty;
            Log.Write("free passage taken against " + (band == null ? "an unnamed party" : band.StringId));
            band?.IgnoreForHours(GetawayHours);
            band?.Ai?.SetDoNotAttackMainParty(GetawayHours);
            if (PlayerEncounter.Current != null)
            {
                PlayerEncounter.ProtectPlayerSide();
                PlayerEncounter.LeaveEncounter = true;
            }
            Log.Write("free passage held for " + GetawayHours + " hours: " +
                      (band == null ? "that band" : band.StringId) + " leaves your party alone, while every " +
                      "other party passes you over only for the hour the game gives anyone leaving an encounter");
        }
    }

    [HarmonyPatch(typeof(VillagerCampaignBehavior), "village_farmer_buy_products_on_condition")]
    internal static class Patch_VillagerOfferShown
    {
        private static void Postfix(ref bool __result)
        {
            if (__result && Meetings.TheirOfferIsTaken(PlayerEncounter.EncounteredMobileParty)) __result = false;
        }
    }

    [HarmonyPatch(typeof(VillagerCampaignBehavior), "conversation_player_decided_to_buy_on_consequence")]
    internal static class Patch_VillagerOfferTaken
    {
        private static bool Prefix(out (List<(string id, int units, int price)> offered, int gold) __state)
        {
            __state = (null, 0);
            MobileParty met = MobileParty.ConversationParty;
            if (Meetings.TheirOfferIsTaken(met))
            {
                Log.Write(met.Name + " were asked for their offer again after TradeLord took it, " +
                          "so nothing changed hands the second time");
                if (PlayerEncounter.Current != null) PlayerEncounter.LeaveEncounter = true;
                return false;
            }
            List<(string id, int units, int price)> offered = null;
            Guard.Run("Villagers.Offer", () => offered = Meetings.WhatTheyOffer(met));
            __state = (offered, Hero.MainHero?.Gold ?? 0);
            return true;
        }

        private static void Postfix((List<(string id, int units, int price)> offered, int gold) __state)
        {
            if (__state.offered == null || Hero.MainHero == null) return;
            int paid = __state.gold - Hero.MainHero.Gold;
            Guard.Run("Villagers.Paid", () => Meetings.YouTookTheirOffer(__state.offered, paid));
        }
    }

    internal static class Parley
    {
        internal const string OwnState = "tradelord_bandit_pass_asked";
        private const string BandAsks = "bandit_start_defender_2";
        private const string BandOpens = "bandit_start_defender";
        private const int Attempts = 20;
        private const int TicksApart = 30;

        private static ConversationSentence _asked;
        private static bool _hung;
        private static int _tries;
        private static int _ticks;

        internal static void Remember(ConversationSentence asked)
        {
            _asked = asked;
            _hung = false;
            _tries = 0;
            _ticks = 0;
        }

        internal static void Forget() => Remember(null);

        internal static void HangWhereTheBandAnswers()
        {
            if (_hung || _asked == null || _tries >= Attempts) return;
            if (Campaign.Current == null) return;
            if (_ticks++ % TicksApart != 0) return;
            _tries++;
            ConversationManager talk = Campaign.Current.ConversationManager;
            if (talk == null) { Unhung("the campaign is not talking to anyone yet"); return; }
            var said = typeof(ConversationManager).GetField(
                "_sentences", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(talk)
                as List<ConversationSentence>;
            if (said == null) { Unhung("this game version keeps its lines out of reach"); return; }
            ConversationSentence asks = null, opens = null;
            var named = new List<string>();
            foreach (ConversationSentence line in said)
            {
                string id = line?.Id;
                if (id == null) continue;
                if (id == BandAsks) asks = line;
                else if (id == BandOpens) opens = line;
                else if (named.Count < 8 && id.IndexOf("bandit", StringComparison.OrdinalIgnoreCase) >= 0)
                    named.Add(id);
            }
            int token = asks != null ? asks.InputToken : opens != null ? opens.OutputToken : -1;
            if (token < 0)
            {
                Unhung("none of the " + said.Count + " lines the game has written is the one a band "
                       + "opens with" + (named.Count == 0 ? "" : ", and the bandit lines it does have are "
                       + string.Join(", ", named.ToArray())));
                return;
            }
            MethodInfo hang = typeof(ConversationSentence).GetMethod(
                "set_InputToken", BindingFlags.Instance | BindingFlags.NonPublic);
            if (hang == null) { Unhung("a line cannot be moved on this game version"); return; }
            talk.DisableSentenceSort();
            try { hang.Invoke(_asked, new object[] { token }); }
            finally { talk.EnableSentenceSort(); }
            _hung = true;
            Log.Write("free passage: the option now sits with the answers a band's own talk offers, "
                      + "found on try " + _tries);
        }

        private static void Unhung(string why)
        {
            if (_tries < Attempts) return;
            Log.Write("free passage: no option is offered to a band, because " + why
                      + " - a band is met exactly as the game means it to be");
        }
    }

}
