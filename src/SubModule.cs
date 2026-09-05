using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TradeLord
{
    public class SubModule : MBSubModuleBase
    {
        public const string HarmonyId = "mod.tradelord";

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Log.Write("TradeLord " + OwnVersion() + " loaded | game " + GameVersion());

            var harmony = new Harmony(HarmonyId);

            Patcher.TryPatch(harmony, typeof(Patch_ItemMenuVM_RefreshItemTooltips));
            Patcher.TryPatch(harmony, typeof(Patch_SuppressVanillaTradeLines));
            Patcher.TryPatch(harmony, typeof(Patch_SPItemVM_UpdateProfitType));
            Patcher.TryPatch(harmony, typeof(Patch_SilenceChunkedTradeLines));

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
            Guard.Run("Tick.Settings", Config.Flush);
            LedgerPanel.Tick();
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);
            Guard.Run("GameEnd.Panel", LedgerPanel.Reset);
            Guard.Run("GameEnd.Travel", Travel.Forget);
            Guard.Run("GameEnd.Bulk", Bulk.Forget);
            Guard.Run("GameEnd.Visit", TradeActionBehavior.ForgetVisit);
            LedgerBehavior.Instance = null;
            Guard.Run("GameEnd.Log", Log.Forget);
        }
    }
}
