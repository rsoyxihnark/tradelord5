using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace TradeLord
{
    internal static class TooltipHelper
    {
        private const int TopN = 5;

        private const string GoldIcon = "<img src=\"General\\Icons\\Coin@2x\" extend=\"4\">";
        private static readonly Color Title = new Color(0.95f, 0.85f, 0.5f, 1f);
        private static readonly Color Good = new Color(0f, 0.8f, 0f, 1f);
        private static readonly Color Warn = new Color(0.95f, 0.55f, 0.3f, 1f);
        private static readonly Color Bad = new Color(0.85f, 0.25f, 0.25f, 1f);

        private static readonly Color SellLo = new Color(0.6f, 1f, 0.6f, 1f);
        private static readonly Color SellHi = new Color(0f, 0.8f, 0f, 1f);
        private static readonly Color BuyLo = new Color(0.9f, 0.9f, 0.95f, 1f);
        private static readonly Color BuyHi = new Color(0.5f, 0.6f, 0.7f, 1f);

        private static (ItemObject item, List<(Settlement town, int price)> sells,
                        List<(Settlement town, int price)> buys) Markets(ItemVM itemVm)
        {
            if (itemVm == null || !Options.Current.TooltipHints) return (null, null, null);
            ItemObject item = itemVm.ItemRosterElement.EquipmentElement.Item;
            if (!TradePolicy.Priced(item)) return (null, null, null);
            var ledger = LedgerBehavior.Instance;
            if (ledger == null) return (null, null, null);

            var sells = ledger.TopSell(item, TopN);
            var buys = ledger.TopBuy(item, TopN);
            return sells.Count == 0 && buys.Count == 0 ? (null, null, null) : (item, sells, buys);
        }

        internal static bool HasSection(ItemVM itemVm)
        {
            bool has = false;
            Guard.Run("Tooltip.HasSection", () => has = Markets(itemVm).item != null);
            return has;
        }

        internal static void Append(ItemMenuVM vm, ItemVM itemVm)
        {
            if (vm == null || vm.TargetItemProperties == null) return;
            var (item, sells, buys) = Markets(itemVm);
            if (item == null) return;
            var ledger = LedgerBehavior.Instance;

            AddSeparator(vm);

            Settlement here = Settlement.CurrentSettlement;
            SettlementComponent market = here?.SettlementComponent;

            int basis = vm.IsPlayerItem || market == null
                ? (ledger.GetCostBasis(item))
                : market.GetItemPrice(item, MobileParty.MainParty, false);

            if (sells.Count > 0)
            {
                AddLine(vm, new TextObject("{=TL20}Best sell prices").ToString(), "", Title);
                int lo = sells[sells.Count - 1].price, hi = sells[0].price;
                for (int i = 0; i < sells.Count; i++)
                {
                    var (town, price) = sells[i];
                    float t = hi == lo ? 1f : (float)(price - lo) / (hi - lo);
                    string pct = "";
                    if (basis > 0 && price > basis)
                    {
                        TextObject p = new TextObject("{=TL77}Profit: +{PCT}%");
                        p.SetTextVariable("PCT", (int)((price - basis) * 100f / basis));
                        pct = "  " + p.ToString();
                    }
                    AddLine(vm, (town == here ? "* " : "") + town.Name,
                        RowText(town, price) + pct, Lerp(SellLo, SellHi, t));
                }
            }

            if (buys.Count > 0)
            {
                AddLine(vm, new TextObject("{=TL21}Best buy prices").ToString(), "", Title);
                int lo = buys[0].price, hi = buys[buys.Count - 1].price;
                for (int i = 0; i < buys.Count; i++)
                {
                    var (town, price) = buys[i];
                    float t = hi == lo ? 0f : (float)(price - lo) / (hi - lo);
                    int stock = Options.Current.Omniscient ? LedgerBehavior.StockOf(town, item) : 0;
                    string st = "";
                    if (stock > 0)
                    {
                        TextObject k = new TextObject("{=TL78}Stock: {COUNT}");
                        k.SetTextVariable("COUNT", stock);
                        st = "  " + k.ToString();
                    }
                    AddLine(vm, (town == here ? "* " : "") + town.Name,
                        RowText(town, price) + st, Lerp(BuyLo, BuyHi, t));
                }
            }

            if (market != null)
            {
                if (vm.IsPlayerItem && sells.Count > 0)
                {
                    int local = market.GetItemPrice(item, MobileParty.MainParty, true);
                    if (sells[0].town == here || local >= sells[0].price)
                        AddLine(vm, "", new TextObject("{=TL22}* Best market to sell this!").ToString(), Good);
                    else if (local > 0 && sells[0].price > local)
                    {
                        TextObject w = new TextObject("{=TL15}+{PCT}% if sold there instead");
                        w.SetTextVariable("PCT", (int)((sells[0].price - local) * 100f / local));
                        AddLine(vm, "", w.ToString(), Warn);
                    }
                }
                else if (!vm.IsPlayerItem && buys.Count > 0)
                {
                    int local = market.GetItemPrice(item, MobileParty.MainParty, false);
                    if (buys[0].town == here || (local > 0 && local <= buys[0].price))
                        AddLine(vm, "", new TextObject("{=TL23}* Cheapest market to buy this!").ToString(), Good);
                    else if (local > buys[0].price)
                    {
                        TextObject w = new TextObject("{=TL24}Cheaper in {TOWN}: {PRICE}{GOLD}");
                        w.SetTextVariable("TOWN", buys[0].town.Name);
                        w.SetTextVariable("PRICE", buys[0].price);
                        w.SetTextVariable("GOLD", GoldIcon);
                        AddLine(vm, "", w.ToString(), Bad);
                    }
                }
            }
        }

        private static string RowText(Settlement town, int price)
        {
            string d = Travel.EstimateLabel(town);
            return (d.Length == 0 ? "" : d + "  ") + price + GoldIcon;
        }

        private static Color Lerp(Color a, Color b, float t)
        {
            return new Color(a.Red + (b.Red - a.Red) * t, a.Green + (b.Green - a.Green) * t,
                             a.Blue + (b.Blue - a.Blue) * t, 1f);
        }

        private static void AddLine(ItemMenuVM vm, string definition, string value, Color color,
            TooltipProperty.TooltipPropertyFlags flags = TooltipProperty.TooltipPropertyFlags.None)
        {
            vm.TargetItemProperties.Add(new ItemMenuTooltipPropertyVM(
                definition, value, 0, color, false, null, flags));
        }

        private static void AddSeparator(ItemMenuVM vm)
        {
            vm.TargetItemProperties.Add(new ItemMenuTooltipPropertyVM(
                "", "", 0, BuyLo, false, null, TooltipProperty.TooltipPropertyFlags.DefaultSeperator));
        }
    }

    [HarmonyPatch(typeof(ItemMenuVM), "RefreshItemTooltips")]
    internal static class Patch_ItemMenuVM_RefreshItemTooltips
    {
        private static void Postfix(ItemMenuVM __instance, ItemVM item)
        {
            Guard.Run("Tooltip.RefreshItemTooltips", () => TooltipHelper.Append(__instance, item));
        }
    }

    [HarmonyPatch(typeof(ItemMenuVM), "SetMerchandiseComponentTooltip")]
    internal static class Patch_SuppressVanillaTradeLines
    {
        private static bool Prefix(ItemVM ____targetItem) =>
            !Options.Current.SuppressVanillaTradeLines || !TooltipHelper.HasSection(____targetItem);
    }

    [HarmonyPatch(typeof(SPItemVM), "UpdateProfitType")]
    internal static class Patch_SPItemVM_UpdateProfitType
    {
        private static void Postfix(SPItemVM __instance)
        {
            Guard.Run("Tooltip.ProfitColoring", () =>
            {
                if (!Options.Current.ProfitColoring || __instance == null) return;
                var ledger = LedgerBehavior.Instance;
                if (ledger == null) return;
                ItemObject item = __instance.ItemRosterElement.EquipmentElement.Item;
                if (!TradePolicy.Priced(item)) return;
                int cost = __instance.ItemCost;
                if (cost <= 0) return;

                if (__instance.InventorySide == InventoryLogic.InventorySide.OtherInventory)
                {
                    var best = ledger.BestBuy(item);
                    if (best.town == null || best.price <= 0) return;
                    float r = (float)cost / best.price;
                    __instance.ProfitType = r <= 1.02f ? 2 : r <= 1.15f ? 1 : r <= 1.4f ? 0 : r <= 1.7f ? -1 : -2;
                }
                else if (__instance.InventorySide == InventoryLogic.InventorySide.PlayerInventory)
                {
                    var best = ledger.BestSell(item);
                    if (best.town == null || best.price <= 0) return;
                    float r = (float)cost / best.price;
                    __instance.ProfitType = r >= 0.98f ? 2 : r >= 0.9f ? 1 : r >= 0.75f ? 0 : r >= 0.6f ? -1 : -2;
                }
            });
        }
    }
}
