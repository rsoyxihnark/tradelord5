using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Issues;
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
        };

        private static (Type quest, FieldInfo wanted, FieldInfo many)[] _read;
        private static bool _unreadable;

        internal static void Forget() { _read = null; _unreadable = false; }

        internal static bool Known => Readable();

        private static bool Readable()
        {
            if (_unreadable) return false;
            if (_read != null) return true;
            var found = new (Type, FieldInfo, FieldInfo)[Named.Length];
            for (int i = 0; i < Named.Length; i++)
            {
                FieldInfo wanted = Named[i].quest.GetField(
                    Named[i].wanted, BindingFlags.Instance | BindingFlags.NonPublic);
                FieldInfo many = Named[i].quest.GetField(
                    Named[i].many, BindingFlags.Instance | BindingFlags.NonPublic);
                if (wanted == null || many == null)
                {
                    _unreadable = true;
                    Log.Write("quest animals: " + Named[i].quest.Name + " does not say which animal it wants " +
                              "or how many on this game version - no animal is sold at all, so a " +
                              "quest of yours cannot lose one");
                    return false;
                }
                found[i] = (Named[i].quest, wanted, many);
            }
            _read = found;
            return true;
        }

        internal static Dictionary<ItemObject, int> Promised()
        {
            if (!Readable()) return null;
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
            }
            return promised;
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
