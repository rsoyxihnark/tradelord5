using System;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Localization;

namespace TradeLord
{
    internal static class Counter
    {
        private const int TicksToWaitForTheScreen = 240;

        private static readonly Action<int> Ours = whatever => { };

        private static InventoryLogic _logic;
        private static int _toSell;
        private static int _toBuy;
        private static int _gained;
        private static int _spent;

        private static InventoryLogic _shown;
        private static bool _totalHandedOver;
        private static int _waited;
        private static int _goldAtOpen;

        internal static bool Staging => _logic != null;

        internal static bool Awaiting => _shown != null;

        internal static string Heading => Staging ? " (laid out): " : " (simulated, best case): ";

        internal static string Aside => Staging ? " (laid out)" : " (simulated)";

        internal static void Forget()
        {
            Drop();
            Unwatch();
        }

        internal static bool HoldsBack() =>
            TradeRules.StagesTheDeal(Options.Current) &&
            (Options.Current.AutoSellOnEntry || Options.Current.AutoBuyOnEntry);

        internal static bool Ready(Settlement site)
        {
            Drop();
            Unwatch();
            if (!TradeRules.StagesTheDeal(Options.Current))
            {
                if (Options.Current.StagedTrading)
                    Log.Repeatable("counter", "dry run",
                                   "the dry run is on, so nothing is laid out on the trade screen: TradeLord " +
                                   "reports what it would trade and moves nothing either way");
                return true;
            }
            bool open = false;
            Guard.Run("Counter.Open", () => open = Opened(site));
            if (open) return true;
            Drop();
            Log.Write("ERROR: the trade screen would not open at " +
                      (site == null ? "this market" : site.Name.ToString()) +
                      ", so nothing was laid out and nothing was traded");
            return false;
        }

        internal static void Stage(ItemRosterElement el, bool selling, int price)
        {
            if (_logic == null || price < 0) return;
            Guard.Run("Counter.Stage", () =>
            {
                InventoryLogic.InventorySide mine = InventoryLogic.InventorySide.PlayerInventory;
                InventoryLogic.InventorySide theirs = InventoryLogic.InventorySide.OtherInventory;
                _logic.AddTransferCommand(TransferCommand.Transfer(
                    1, selling ? mine : theirs, selling ? theirs : mine, el,
                    EquipmentIndex.None, EquipmentIndex.None, CharacterObject.PlayerCharacter));
                if (selling)
                {
                    _toSell++;
                    _gained += price;
                }
                else
                {
                    _toBuy++;
                    _spent += price;
                }
            });
        }

        internal static TextObject Settle()
        {
            if (_logic == null) return null;
            int toSell = _toSell, toBuy = _toBuy, gained = _gained, spent = _spent;
            Drop();
            Log.Write("laid out on the trade screen: " + toSell + " unit(s) to sell for " + gained +
                      " gold and " + toBuy + " to buy for " + spent +
                      ", left for you to take, change or leave");
            if (toSell == 0 && toBuy == 0)
                return Tongue.Text("{=TL402}TradeLord found nothing here worth trading, so it has laid out nothing. Press Cancel to leave the market as it is.");
            TextObject line = Tongue.Text("{=TL401}TradeLord has laid out {SOLD} unit(s) to sell for {GAINED} denars and {BOUGHT} to buy for {SPENT}. Change anything you like, then press Done to trade, or Cancel to leave it.");
            line.SetTextVariable("SOLD", toSell.ToString("N0"));
            line.SetTextVariable("GAINED", gained.ToString("N0"));
            line.SetTextVariable("BOUGHT", toBuy.ToString("N0"));
            line.SetTextVariable("SPENT", spent.ToString("N0"));
            return line;
        }

        private static bool Opened(Settlement site)
        {
            SettlementComponent market = site?.SettlementComponent;
            ItemRoster stock = site?.ItemRoster;
            if (market == null || stock == null) return false;
            InventoryScreenHelper.OpenScreenAsTrade(stock, market);
            InventoryState screen = InventoryScreenHelper.GetActiveInventoryState();
            InventoryLogic logic = screen?.InventoryLogic;
            if (logic == null) return false;
            if (logic.TotalAmountChange == null) logic.TotalAmountChange = Ours;
            if (logic.DonationXpChange == null) logic.DonationXpChange = () => { };
            _logic = logic;
            _shown = logic;
            _totalHandedOver = false;
            _waited = 0;
            _goldAtOpen = Hero.MainHero?.Gold ?? 0;
            TradeActionBehavior.StartAFreshDryRun();
            Log.Write("the trade screen is open at " + site.Name +
                      " and TradeLord is laying its deal out on it rather than trading");
            return true;
        }

        internal static TextObject Watch()
        {
            if (_shown == null) return null;
            if (!_totalHandedOver) HandTheTotalOver();
            if (InventoryScreenHelper.GetActiveInventoryState() != null) return null;
            int moved = (Hero.MainHero?.Gold ?? _goldAtOpen) - _goldAtOpen;
            Unwatch();
            Log.Write("the trade screen is closed and your purse moved " + moved + " gold on it");
            if (moved == 0)
                return Tongue.Text("{=TL423}The deal TradeLord laid out is closed and your purse is where it was.");
            TextObject line = Tongue.Text(moved > 0
                ? "{=TL421}The deal TradeLord laid out is closed: your purse is up {GOLD} denars."
                : "{=TL422}The deal TradeLord laid out is closed: your purse is down {GOLD} denars.");
            line.SetTextVariable("GOLD", Math.Abs(moved).ToString("N0"));
            return line;
        }

        private static void HandTheTotalOver()
        {
            Action<int> reading = _shown.TotalAmountChange;
            if (reading != null && !ReferenceEquals(reading, Ours))
            {
                _totalHandedOver = true;
                reading(_shown.TotalAmount);
                Log.Write("the trade screen was handed the " + _shown.TotalAmount +
                          " gold the laid out deal comes to, so its own running total shows it");
                return;
            }
            if (++_waited < TicksToWaitForTheScreen) return;
            _totalHandedOver = true;
            Log.Write("the trade screen never took a running total of its own, so what the laid out " +
                      "deal comes to is left off it");
        }

        private static void Unwatch()
        {
            _shown = null;
            _totalHandedOver = false;
            _waited = 0;
            _goldAtOpen = 0;
        }

        private static void Drop()
        {
            _logic = null;
            _toSell = 0;
            _toBuy = 0;
            _gained = 0;
            _spent = 0;
        }
    }
}
