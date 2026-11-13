using System.Collections.Generic;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
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
        internal static readonly Color Golden = new Color(0.95f, 0.85f, 0.50f);
        internal static readonly Color Alert = new Color(0.90f, 0.28f, 0.28f);

        private static readonly List<InformationMessage> _pending = new List<InformationMessage>();

        private static bool _profitStyled;

        private static bool _xpStyled;

        internal static string Profit(int profit) => ProfitMark.Shown(profit, _profitStyled);

        internal static string TradeXp(string said) => ProfitMark.XpShown(said, _xpStyled);

        internal static void LetTheTradeXpShow(RichTextWidget line)
        {
            Brush brush = line?.Context?.GetBrush(ProfitMark.ChatBrush);
            if (brush == null || brush.GetStyle(ProfitMark.XpStyle) != null) return;
            var style = new Style(brush.Layers) { Name = ProfitMark.XpStyle, DefaultStyle = brush.DefaultStyle };
            style.FontColor = Alert;
            brush.AddStyle(style);
            if (_xpStyled) return;
            _xpStyled = true;
            Log.Write("the Trade XP in the Sold line shows in red from now on: the chat line brush " + brush.Name +
                      " took the " + ProfitMark.XpStyle + " style");
        }

        internal static void LetTheProfitShow(RichTextWidget line)
        {
            Brush brush = line?.Context?.GetBrush(ProfitMark.ChatBrush);
            if (brush == null || brush.GetStyle(ProfitMark.Style) != null) return;
            var style = new Style(brush.Layers) { Name = ProfitMark.Style, DefaultStyle = brush.DefaultStyle };
            style.FontColor = Golden;
            brush.AddStyle(style);
            if (_profitStyled) return;
            _profitStyled = true;
            Log.Write("the profit in the Sold line shows in golden yellow from now on: the chat line brush " + brush.Name +
                      " took the " + ProfitMark.Style + " style");
        }

        internal static void Forget()
        {
            _pending.Clear();
        }

        internal static bool Speaking { get; private set; }

        internal static void Show(InformationMessage message)
        {
            bool was = Speaking;
            Speaking = true;
            try { InformationManager.DisplayMessage(message); }
            finally { Speaking = was; }
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
                        Show(_pending[i]);
            }
            finally { _pending.Clear(); }
        }
    }

    internal static class News
    {
        private sealed class Held
        {
            internal InformationMessage First;
            internal string Shape;
            internal int More;
            internal float Since;
        }

        internal const float HeldFor = 1f;
        private const float NamesKeptFor = 60f;

        private static readonly List<Held> _held = new List<Held>();
        private static List<string> _names;
        private static float _namesAt;
        private static bool _releasing;
        private static int _grouped;
        private static int _hidden;

        internal static bool Hold(InformationMessage message)
        {
            if (_releasing || Notices.Speaking || message == null || !Options.Current.ExtendedDebugLogging) return false;
            if (Campaign.Current == null || string.IsNullOrEmpty(message.Information)) return false;
            float now = TaleWorlds.Engine.Time.ApplicationTime;
            string shape = GameNews.Shape(message.Information, Names(now));
            for (int i = 0; i < _held.Count; i++)
            {
                if (_held[i].Shape != shape) continue;
                _held[i].More++;
                return true;
            }
            _held.Add(new Held { First = message, Shape = shape, Since = now });
            return true;
        }

        internal static void Release()
        {
            if (_held.Count == 0) return;
            float now = TaleWorlds.Engine.Time.ApplicationTime;
            bool all = !Options.Current.ExtendedDebugLogging || Campaign.Current == null;
            for (int i = 0; i < _held.Count; i++)
            {
                Held one = _held[i];
                if (!all && now - one.Since < HeldFor && now >= one.Since) continue;
                _held.RemoveAt(i--);
                Shown(one);
            }
        }

        private static void Shown(Held one)
        {
            if (one.More > 0)
            {
                TextObject more = Tongue.Text("{=TL518} (+{COUNT} more)");
                more.SetTextVariable("COUNT", one.More);
                string said = one.First.Information;
                one.First.Information = GameNews.WithMore(said, more.ToString());
                _grouped++;
                _hidden += one.More;
                Log.Write("game messages grouped while Extended debug logging is on: " + (one.More + 1) +
                          " of one kind shown as one line, the first of them \"" + said + "\"; " + _grouped +
                          " line(s) stood for " + (_grouped + _hidden) + " message(s) this session");
            }
            _releasing = true;
            try { InformationManager.DisplayMessage(one.First); }
            finally { _releasing = false; }
        }

        private static List<string> Names(float now)
        {
            if (_names != null && now - _namesAt < NamesKeptFor && now >= _namesAt) return _names;
            var seen = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (Hero hero in Hero.AllAliveHeroes) Named(seen, hero);
            foreach (Hero hero in Hero.DeadOrDisabledHeroes) Named(seen, hero);
            foreach (Settlement place in Settlement.All) Named(seen, place.Name);
            foreach (Clan clan in Clan.All) Named(seen, clan.Name);
            foreach (Kingdom kingdom in Kingdom.All)
            {
                Named(seen, kingdom.Name);
                Named(seen, kingdom.InformalName);
            }
            var names = new List<string>(seen);
            names.Sort((x, y) => x.Length != y.Length ? y.Length.CompareTo(x.Length) : string.CompareOrdinal(x, y));
            _names = names;
            _namesAt = now;
            return names;
        }

        private static void Named(HashSet<string> seen, Hero hero)
        {
            if (hero == null) return;
            Named(seen, hero.Name);
            Named(seen, hero.FirstName);
        }

        private static void Named(HashSet<string> seen, TextObject name)
        {
            string said = name?.ToString();
            if (!string.IsNullOrEmpty(said) && said.Length >= GameNews.ShortestName) seen.Add(said);
        }

        internal static void Forget()
        {
            _held.Clear();
            _names = null;
            _grouped = 0;
            _hidden = 0;
        }
    }

    [HarmonyPatch(typeof(InformationManager), "DisplayMessage")]
    internal static class Patch_GroupTheGamesNews
    {
        private static bool Prefix(InformationMessage __0, bool __runOriginal)
        {
            if (!__runOriginal || TradeActionBehavior.InGameTransaction) return __runOriginal;
            return !Guard.Read("News.Hold", __0, News.Hold, false);
        }
    }

    [HarmonyPatch(typeof(ChatLogItemWidget), "OneLineTextWidget", MethodType.Setter)]
    internal static class Patch_ChatLineShowsTheProfit
    {
        private static void Postfix(RichTextWidget __0)
        {
            Guard.Run("ChatLine.Profit", __0, Notices.LetTheProfitShow);
            Guard.Run("ChatLine.TradeXp", __0, Notices.LetTheTradeXpShow);
        }
    }
}
