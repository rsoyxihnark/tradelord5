using System.Collections.Generic;
using TaleWorlds.Library;
using TaleWorlds.Localization;

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
        private static readonly List<InformationMessage> _afterXp = new List<InformationMessage>();

        internal static void Forget()
        {
            _pending.Clear();
            _afterXp.Clear();
        }

        internal static void Say(TextObject msg) => Say(msg, Note);

        internal static void Say(TextObject msg, Color color) =>
            _pending.Add(new InformationMessage(msg.ToString(), color));

        internal static void SayAfterXp(TextObject msg, Color color) =>
            _afterXp.Add(new InformationMessage(msg.ToString(), color));

        internal static void Drain()
        {
            if (_afterXp.Count > 0)
            {
                _pending.AddRange(_afterXp);
                _afterXp.Clear();
            }
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
}
