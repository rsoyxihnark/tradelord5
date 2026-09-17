using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TradeLord
{
    internal static class SelfCheck
    {
        internal static void Say() => Guard.Run("SelfCheck", () =>
            Log.Write("self-check: " + Patcher.Tally() +
                      " | " + Drove.PenaltyRead() +
                      " | " + (Errands.Known ? "quest goods read" : "quest goods not read") +
                      " | " + Tongue.StringsRead() +
                      " | " + Priced.ModelInForce()));
    }

    public class SubModule : MBSubModuleBase
    {
        public const string HarmonyId = "mod.tradelord";

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Log.Write("TradeLord " + OwnVersion() + " loaded | game " + GameVersion());

            var harmony = new Harmony(HarmonyId);

            Patcher.TryPatch(harmony, typeof(Patch_TownMarketData_GetPrice));
            Patcher.TryPatch(harmony, typeof(Patch_ItemMenuVM_RefreshItemTooltips));
            Patcher.TryPatch(harmony, typeof(Patch_SuppressVanillaTradeLines));
            Patcher.TryPatch(harmony, typeof(Patch_SPItemVM_UpdateProfitType));
            Patcher.TryPatch(harmony, typeof(Patch_SilenceChunkedTradeLines));
            Patcher.TryPatch(harmony, typeof(Patch_WorkshopLimit));
            Patcher.TryPatch(harmony, typeof(Patch_WorkshopsYouMayHave));

            Guard.Run("McmLoader", McmLoader.TryLoad);
            Config.Follow();
        }

        internal static string OwnVersion()
        {
            try { return TaleWorlds.ModuleManager.ModuleHelper.GetModuleInfo("TradeLord").Version.ToString(); }
            catch { return "(version unknown)"; }
        }

        private static string GameVersion()
        {
            try { return TaleWorlds.ModuleManager.ModuleHelper.GetModuleInfo("Native").Version.ToString(); }
            catch { return "unknown"; }
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (game.GameType is Campaign && gameStarterObject is CampaignGameStarter starter)
            {
                starter.AddBehavior(new LedgerBehavior());
                starter.AddBehavior(new TradeActionBehavior());
            }
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            Guard.Run("Tick.ReleaseMessageFilter", TradeActionBehavior.ReleaseMessageFilter);
            Guard.Run("Tick.FlushToasts", TradeActionBehavior.FlushToasts);
            Guard.Run("Tick.Mcm", McmLoader.TryHandover);
            Guard.Run("Tick.Settings", Config.Flush);
            Guard.Run("Tick.Encounter", Meetings.Watch);
            Guard.Run("Tick.Counter", TradeActionBehavior.WatchTheTradeScreen);
            Guard.Run("Tick.Parley", Parley.HangWhereTheBandAnswers);
            LedgerPanel.Tick();
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);
            Guard.Run("GameEnd.Panel", LedgerPanel.Reset);
            Guard.Run("GameEnd.Travel", Travel.Forget);
            Guard.Run("GameEnd.Bulk", Bulk.Forget);
            Guard.Run("GameEnd.ScreenMarkets", ScreenMarkets.Forget);
            Guard.Run("GameEnd.Forecast", Forecast.Forget);
            Guard.Run("GameEnd.Hindsight", Hindsight.Forget);
            Guard.Run("GameEnd.Counter", Counter.Forget);
            Guard.Run("GameEnd.Visit", TradeActionBehavior.ForgetVisit);
            Guard.Run("GameEnd.Encounter", Meetings.ForgetEncounter);
            LedgerBehavior.Instance = null;
            Guard.Run("GameEnd.Settings", Config.Settle);
            Guard.Run("GameEnd.Log", Log.Forget);
        }
    }
}
