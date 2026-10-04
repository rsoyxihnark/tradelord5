using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Chat;

namespace TradeLord
{
    internal static class Notices
    {
        internal static readonly Color Gain = new Color(0.40f, 0.90f, 0.40f);
        internal static readonly Color Spend = new Color(0.55f, 0.78f, 1f);
        internal static readonly Color Flat = new Color(0.85f, 0.75f, 0.45f);
        internal static readonly Color Note = new Color(0.75f, 0.75f, 0.75f);
        internal static readonly Color Xp = new Color(1f, 0.72f, 0.20f);
        internal static readonly Color Alert = new Color(0.90f, 0.28f, 0.28f);

        private static readonly List<InformationMessage> _pending = new List<InformationMessage>();

        private static bool _profitStyled;

        internal static string Profit(int profit) => ProfitMark.Shown(profit, _profitStyled);

        internal static void LetTheProfitShow(RichTextWidget line)
        {
            Brush brush = line?.Brush;
            if (brush == null || brush.GetStyle(ProfitMark.Style) != null) return;
            var style = new Style(brush.Layers) { Name = ProfitMark.Style, DefaultStyle = brush.DefaultStyle };
            style.FontColor = Xp;
            brush.AddStyle(style);
            if (_profitStyled) return;
            _profitStyled = true;
            Log.Write("the profit in the Sold line shows in orange from now on: the chat line brush " + brush.Name +
                      " took the " + ProfitMark.Style + " style");
        }

        internal static void Forget()
        {
            _pending.Clear();
        }

        internal static void Say(TextObject msg) => Say(msg, Note);

        internal static void Say(TextObject msg, Color color) =>
            _pending.Add(new InformationMessage(msg.ToString(), color));

        internal static void Drain()
        {
            if (_pending.Count == 0) return;
            try
            {
                for (int i = 0; i < _pending.Count; i++)
                    if (i == 0 || _pending[i].Information != _pending[i - 1].Information)
                        InformationManager.DisplayMessage(_pending[i]);
            }
            finally { _pending.Clear(); }
        }
    }

    [HarmonyPatch(typeof(ChatLogItemWidget), "OneLineTextWidget", MethodType.Setter)]
    internal static class Patch_ChatLineShowsTheProfit
    {
        private static void Postfix(RichTextWidget __0) =>
            Guard.Run("ChatLine.Profit", __0, Notices.LetTheProfitShow);
    }
}
