using System;
using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Localization;

namespace TradeLord
{
    internal static class Shops
    {
        internal static int MayOwn()
        {
            int asked = Options.Current.MaxWorkshopsOwned;
            int gameSays = FromTheGame();
            return Holdings.WorkshopsYouMayOwn(gameSays, asked);
        }

        private static int FromTheGame()
        {
            try
            {
                var model = Campaign.Current?.Models?.WorkshopModel;
                Clan mine = Clan.PlayerClan;
                return model == null || mine == null ? 0 : model.GetMaxWorkshopCountForClanTier(mine.Tier);
            }
            catch { return 0; }
        }

        internal static int YourTier()
        {
            try { return Clan.PlayerClan?.Tier ?? -1; }
            catch { return -1; }
        }

        private static int _youBuying;

        internal static bool ItIsYouBuying => _youBuying > 0;

        internal static void WhileItIsYouBuying(Action work)
        {
            _youBuying++;
            try { work(); }
            finally { _youBuying--; }
        }

        internal static void ForgetWhoIsBuying() => _youBuying = 0;

        internal static int Owned()
        {
            try { return Hero.MainHero?.OwnedWorkshops?.Count ?? 0; }
            catch { return 0; }
        }

        internal static int CostOf(Workshop shop)
        {
            try
            {
                var model = Campaign.Current?.Models?.WorkshopModel;
                return model == null || shop == null ? 0 : model.GetCostForPlayer(shop);
            }
            catch { return 0; }
        }

        internal static bool OnTheMarket(Workshop shop)
        {
            if (shop?.Settlement == null || shop.WorkshopType == null) return false;
            Hero owner = shop.Owner;
            return owner != null && owner != Hero.MainHero && owner.IsNotable && !owner.IsDead;
        }

        internal static Block WhatStopsBuying(Workshop shop, int cost, int purse, int owned, int mayOwn)
        {
            if (shop == null || !OnTheMarket(shop)) return Block.NotTradable;
            if (!Holdings.RoomForOneMore(owned, mayOwn)) return Block.HeldEnough;
            if (cost <= 0) return Block.NotTradable;
            if (cost > purse) return Block.BudgetSpent;
            return Block.None;
        }

        internal static List<Workshop> OnOffer()
        {
            var found = new List<Workshop>();
            Guard.Run("Shops.OnOffer", () =>
            {
                foreach (Town town in Town.AllTowns)
                {
                    Workshop[] shops = town?.Workshops;
                    if (shops == null) continue;
                    if (LedgerBehavior.UnderAttack(town.Settlement)) continue;
                    if (Options.Current.ExcludeHostileTowns &&
                        LedgerBehavior.IsHostile(town.Settlement)) continue;
                    for (int i = 0; i < shops.Length; i++)
                        if (OnTheMarket(shops[i])) found.Add(shops[i]);
                }
                found.Sort((x, y) => y.ProfitMade.CompareTo(x.ProfitMade));
            });
            return found;
        }

        internal static bool Buy(Workshop shop, out TextObject said)
        {
            said = null;
            int cost = CostOf(shop);
            int purse = Hero.MainHero?.Gold ?? 0;
            int owned = Owned(), mayOwn = MayOwn();
            Block why = WhatStopsBuying(shop, cost, purse, owned, mayOwn);
            if (why != Block.None)
            {
                said = WhyNot(why, shop, cost, purse, owned, mayOwn);
                Log.Write("workshop not bought: " + Named(shop) + " for " + cost + " gold, purse " + purse +
                          ", owned " + owned + " of " + mayOwn + ", stopped on " + why);
                return false;
            }
            bool done = false;
            Hero seller = shop.Owner;
            int before = Hero.MainHero?.Gold ?? 0;
            Guard.Run("Shops.Buy", () => WhileItIsYouBuying(() =>
            {
                ChangeOwnerOfWorkshopAction.ApplyByPlayerBuying(shop);
                done = shop.Owner == Hero.MainHero;
            }));
            int paid = before - (Hero.MainHero?.Gold ?? before);
            int owed = Holdings.StillOwedForTheWorkshop(cost, paid);
            if (done && owed > 0)
                Guard.Run("Shops.Settle", () =>
                {
                    if (seller != null)
                        GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, seller, owed, true);
                    Log.Write("the game handed over " + Named(shop) + " without taking the " + cost +
                              " gold for it, so TradeLord paid " + Named(shop) + "'s seller itself");
                });
            else if (done)
                Log.Write("the game took " + paid + " gold for " + Named(shop) + ", priced at " + cost);
            if (!done)
            {
                said = Tongue.Text("{=TL437}The game would not hand that workshop over. TradeLord.log says what happened.");
                Log.Write("ERROR: the game refused to hand over " + Named(shop) + " for " + cost + " gold");
                return false;
            }
            Log.Write("workshop bought: " + Named(shop) + " for " + cost + " gold, now " +
                      Owned() + " of " + mayOwn + " owned");
            TextObject line = Tongue.Text("{=TL434}TradeLord bought the {SHOP} in {TOWN} for {GOLD} denars.");
            line.SetTextVariable("SHOP", shop.WorkshopType.Name);
            line.SetTextVariable("TOWN", shop.Settlement.Name);
            line.SetTextVariable("GOLD", cost.ToString("N0"));
            said = line;
            return true;
        }

        private static TextObject WhyNot(Block why, Workshop shop, int cost, int purse,
                                         int owned, int mayOwn)
        {
            if (why == Block.HeldEnough)
            {
                TextObject full = Tongue.Text("{=TL435}You already own {OWNED} workshop(s), which is all Most workshops you may own allows. Raise it in TradeLord's settings to buy another.");
                full.SetTextVariable("OWNED", owned.ToString("N0"));
                return full;
            }
            if (why == Block.BudgetSpent)
            {
                TextObject poor = Tongue.Text("{=TL436}That workshop costs {GOLD} denars and your purse is at {PURSE}.");
                poor.SetTextVariable("GOLD", cost.ToString("N0"));
                poor.SetTextVariable("PURSE", purse.ToString("N0"));
                return poor;
            }
            return Tongue.Text("{=TL437}The game would not hand that workshop over. TradeLord.log says what happened.");
        }

        internal static string Named(Workshop shop) =>
            shop?.Settlement == null || shop.WorkshopType == null
                ? "an unnamed workshop"
                : shop.WorkshopType.Name + " in " + shop.Settlement.Name;
    }

    [HarmonyPatch(typeof(DefaultWorkshopModel), "GetMaxWorkshopCountForClanTier")]
    internal static class Patch_WorkshopLimit
    {
        private static void Postfix(int tier, ref int __result)
        {
            if (!Holdings.TheGameIsAskingAboutYou(tier, Shops.YourTier(), Shops.ItIsYouBuying)) return;
            __result = Holdings.WorkshopsYouMayOwn(__result, Options.Current.MaxWorkshopsOwned);
        }
    }

    [HarmonyPatch(typeof(DefaultWorkshopModel), "MaximumWorkshopsPlayerCanHave", MethodType.Getter)]
    internal static class Patch_WorkshopsYouMayHave
    {
        private static void Postfix(ref int __result)
        {
            __result = Holdings.WorkshopsYouMayOwn(__result, Options.Current.MaxWorkshopsOwned);
        }
    }
}
