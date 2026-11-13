using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using SandBox.View.Map;
using SandBox.ViewModelCollection.Map.Incidents;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Incidents;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;

namespace TradeLord.TestAutopilot
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Trail.Start();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
            if (!(game.GameType is Campaign) || !(gameStarterObject is CampaignGameStarter starter)) return;
            Pilot.Reset("a campaign was started or loaded");
            starter.AddBehavior(new PilotClock());
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            Button.Tick();
            Pilot.Tick();
        }

        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);
            Pilot.Reset("the campaign ended");
            Button.Reset();
        }
    }

    public class PilotClock : CampaignBehaviorBase
    {
        public override void RegisterEvents() =>
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, Pilot.OnTheHour);

        public override void SyncData(IDataStore dataStore)
        {
        }
    }

    internal static class Pilot
    {
        internal const int StopsOnDay = 21;
        internal const string FirstMarket = "Poros";
        internal const string FreePassage = "tradelord_bandit_pass";
        private const float WaitInside = 1f;
        private const float WaitForTheMarker = 5f;
        private const float WaitOutOfAnEncounter = 3f;
        private const float BetweenLines = 0.5f;
        private const float BeforeAskingTheWayAgain = 2f;
        private const int LeaveTries = 3;
        private const int MostLinesInATalk = 12;
        private const double MostDaysToOneMarket = 3d;

        internal static bool On { get; private set; }

        private static int _stopsOnDay = StopsOnDay;
        private static float _eventSince = -1f;
        private static int _eventTries;

        private static bool _firstMarketDone;
        private static Settlement _inside;
        private static float _insideSince;
        private static int _leaveTries;
        private static Settlement _heading;
        private static double _headingSince;
        private static float _askedTheWayAt = -1f;
        private static float _markerMissingSince = -1f;
        private static float _encounterSince = -1f;
        private static float _talkStep = -1f;
        private static bool _askedForPassage;
        private static int _linesInThisTalk;

        internal static void Reset(string why)
        {
            if (On) Trail.Write("Autopilot off: " + why);
            On = false;
            _firstMarketDone = false;
            Forget();
        }

        private static void Forget()
        {
            _inside = null;
            _leaveTries = 0;
            _heading = null;
            _askedTheWayAt = -1f;
            _markerMissingSince = -1f;
            _encounterSince = -1f;
            _talkStep = -1f;
            _askedForPassage = false;
            _linesInThisTalk = 0;
        }

        internal static void Toggle()
        {
            if (On)
            {
                Stop("you turned it off", false);
                return;
            }
            if (Campaign.Current == null || MobileParty.MainParty == null) return;
            Forget();
            _stopsOnDay = DayNow() < StopsOnDay ? StopsOnDay : int.MaxValue;
            On = true;
            Trail.Write("Autopilot on" + (_firstMarketDone ? ", following the map marker" : ", heading for " + FirstMarket + " first") +
                        (_stopsOnDay == StopsOnDay ? ", pausing as day " + StopsOnDay + " begins" : ", with no day to pause on"));
        }

        internal static void OnTheHour()
        {
            if (!On) return;
            if (DayNow() >= _stopsOnDay)
            {
                Stop("day " + StopsOnDay + " 00:00, the game is paused and waits for you", true);
                return;
            }
            try { Focus.OnTradeIfAtTheLimit(); }
            catch (Exception e) { Trail.Error(e, "a focus point on Trade"); }
        }

        internal static void Tick()
        {
            if (!On) return;
            try { Fly(); }
            catch (Exception e)
            {
                Trail.Error(e, "the autopilot");
                Stop("something went wrong, TradeLordTestAutopilot.log says what", true);
            }
        }

        private static void Fly()
        {
            Campaign campaign = Campaign.Current;
            MobileParty me = MobileParty.MainParty;
            if (campaign == null || me == null) return;
            if (DayNow() >= _stopsOnDay)
            {
                Stop("day " + StopsOnDay + " 00:00, the game is paused and waits for you", true);
                return;
            }
            if (!(ScreenManager.TopScreen is MapScreen map)) return;
            if (map.IsMapIncidentActive)
            {
                Answer(map, TaleWorlds.Engine.Time.ApplicationTime);
                return;
            }
            _eventSince = -1f;
            _eventTries = 0;
            if (InformationManager.IsAnyInquiryActive())
            {
                Stop("a window the game opened is waiting for your answer", true);
                return;
            }
            float now = TaleWorlds.Engine.Time.ApplicationTime;
            ConversationManager talk = campaign.ConversationManager;
            if (talk != null && talk.IsConversationInProgress)
            {
                Talk(talk, now);
                return;
            }
            _askedForPassage = false;
            _linesInThisTalk = 0;
            Settlement inside = me.CurrentSettlement;
            if (inside != null)
            {
                Inside(inside, now);
                return;
            }
            _inside = null;
            if (PlayerEncounter.Current != null || me.MapEvent != null)
            {
                Encounter(now);
                return;
            }
            _encounterSince = -1f;
            Road(campaign, me, now);
        }

        private static void Talk(ConversationManager talk, float now)
        {
            MobileParty them = MobileParty.ConversationParty;
            if (them == null || !them.IsBandit)
            {
                Stop("someone other than a band of bandits is talking to you", true);
                return;
            }
            if (_talkStep >= 0f && now - _talkStep < BetweenLines && now >= _talkStep) return;
            _talkStep = now;
            if (++_linesInThisTalk > MostLinesInATalk)
            {
                Stop("the talk with " + them.Name + " did not end", true);
                return;
            }
            var options = talk.CurOptions;
            int count = options == null ? 0 : options.Count;
            if (_askedForPassage || count == 0)
            {
                talk.ContinueConversation();
                return;
            }
            for (int i = 0; i < count; i++)
            {
                if (options[i].Id != FreePassage) continue;
                Trail.Write(them.Name + " stopped you: answering with TradeLord's free passage line");
                _askedForPassage = true;
                talk.DoOption(i);
                return;
            }
            Stop(them.Name + " stopped you and TradeLord's free passage line is not among your answers", true);
        }

        private static void Inside(Settlement here, float now)
        {
            if (_inside != here)
            {
                _inside = here;
                _insideSince = now;
                _heading = null;
                Trail.Write("inside " + here.Name + ", leaving in " + WaitInside + " second(s)");
                return;
            }
            if (now - _insideSince < WaitInside && now >= _insideSince) return;
            if (++_leaveTries > LeaveTries)
            {
                Stop("the party would not leave " + here.Name, true);
                return;
            }
            if (!_firstMarketDone && IsTheFirstMarket(here)) _firstMarketDone = true;
            if (_leaveTries == 1)
            {
                try { Recruits.Fill(here); }
                catch (Exception e) { Trail.Error(e, "recruiting at " + here.Name); }
                try { Focus.OnTradeIfAtTheLimit(); }
                catch (Exception e) { Trail.Error(e, "a focus point on Trade"); }
            }
            Trail.Write("leaving " + here.Name);
            _insideSince = now;
            PlayerEncounter.LeaveSettlement();
            PlayerEncounter.Finish(true);
            if (Campaign.Current.CurrentMenuContext != null) GameMenu.ExitToLast();
            _markerMissingSince = -1f;
        }

        private static void Answer(MapScreen map, float now)
        {
            if (_eventSince < 0f || now < _eventSince) _eventSince = now;
            if (now - _eventSince < BetweenLines) return;
            MapIncidentView view = map.GetMapView<MapIncidentView>();
            Incident incident = view?.Incident;
            MapIncidentVM shown = view == null ? null : Shown(view);
            if (incident == null || shown?.Options == null || shown.Options.Count == 0)
            {
                if (now - _eventSince >= WaitOutOfAnEncounter) Stop("an event the game opened could not be read", true);
                return;
            }
            if (++_eventTries > LeaveTries)
            {
                Stop("the event " + incident.Title + " would not close", true);
                return;
            }
            _eventSince = now;
            string nothing = new TextObject("{=lobJVVWT}Nothing happens").ToString();
            int pick = -1, best = int.MaxValue;
            var told = new List<string>();
            for (int i = 0; i < shown.Options.Count; i++)
            {
                MapIncidentOptionVM option = shown.Options[i];
                var said = new List<string>();
                bool chance = false;
                Read(incident.GetOptionHint(option.Index), said, ref chance);
                int weight = Events.Weight(said, chance, nothing);
                told.Add("\"" + option.Description + "\" (" + string.Join(", ", said.ToArray()) + (chance ? ", by chance" : "") +
                         ", weight " + weight + ")");
                if (weight > best) continue;
                best = weight;
                pick = i;
            }
            Trail.Write("event " + incident.Title + ": " + string.Join("; ", told.ToArray()) + "; chose \"" +
                        shown.Options[pick].Description + "\"");
            shown.Options[pick].ExecuteSelect();
            shown.ExecuteConfirm();
        }

        private static MapIncidentVM Shown(MapIncidentView view)
        {
            for (Type type = view.GetType(); type != null; type = type.BaseType)
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                    if (typeof(MapIncidentVM).IsAssignableFrom(field.FieldType)) return field.GetValue(view) as MapIncidentVM;
            return null;
        }

        private static void Read(IncidentHint hint, List<string> said, ref bool chance)
        {
            if (hint == null) return;
            if (hint.Chance > 0f && hint.Chance < 1f) chance = true;
            if (hint.Type == IncidentHintType.Select || hint.Type == IncidentHintType.SelectBranch) chance = true;
            string text = hint.Text?.ToString();
            if (!string.IsNullOrEmpty(text)) said.Add(text);
            IncidentHint[] children = hint.Children;
            for (int i = 0; children != null && i < children.Length; i++) Read(children[i], said, ref chance);
        }

        private static void Encounter(float now)
        {
            if (_encounterSince < 0f || now < _encounterSince) _encounterSince = now;
            if (now - _encounterSince < WaitOutOfAnEncounter) return;
            Stop("an encounter is waiting for your answer", true);
        }

        private static void Road(Campaign campaign, MobileParty me, float now)
        {
            _leaveTries = 0;
            Settlement to = _firstMarketDone ? Marked(campaign) : TheFirstMarket();
            if (to == null)
            {
                if (_markerMissingSince < 0f || now < _markerMissingSince) _markerMissingSince = now;
                if (now - _markerMissingSince >= WaitForTheMarker)
                    Stop(_firstMarketDone ? "the map marker shows no market to head for" : FirstMarket + " could not be found", true);
                return;
            }
            _markerMissingSince = -1f;
            double today = CampaignTime.Now.ToDays;
            if (to != _heading)
            {
                Trail.Write("heading for " + to.Name + (_heading == null ? "" : ", where the marker had shown " + _heading.Name));
                _heading = to;
                _headingSince = today;
                AskTheWay(me, to, now);
            }
            else if (me.TargetSettlement != to && (now - _askedTheWayAt >= BeforeAskingTheWayAgain || now < _askedTheWayAt))
            {
                Trail.Write("heading for " + to.Name + " again, the party had stopped going there");
                AskTheWay(me, to, now);
            }
            if (today - _headingSince > MostDaysToOneMarket)
            {
                Stop("the party has not reached " + to.Name + " in " + MostDaysToOneMarket + " days", true);
                return;
            }
            if (campaign.TimeControlMode != CampaignTimeControlMode.StoppableFastForward && !campaign.TimeControlModeLock)
                campaign.TimeControlMode = CampaignTimeControlMode.StoppableFastForward;
        }

        private static void AskTheWay(MobileParty me, Settlement to, float now)
        {
            _askedTheWayAt = now;
            me.SetMoveGoToSettlement(to, MobileParty.NavigationType.Default, false);
        }

        private static readonly System.Collections.Generic.HashSet<Settlement> _tracked =
            new System.Collections.Generic.HashSet<Settlement>();

        private static Settlement _marked;

        private static Settlement Marked(Campaign campaign)
        {
            VisualTrackerManager tracker = campaign.VisualTrackerManager;
            if (tracker == null) return null;
            Settlement added = null, any = null;
            int seen = 0;
            foreach (Settlement place in Settlement.All)
            {
                if (!tracker.CheckTracked(place)) continue;
                seen++;
                any = place;
                if (!_tracked.Contains(place)) added = place;
            }
            if (seen != _tracked.Count || added != null)
            {
                _tracked.Clear();
                foreach (Settlement place in Settlement.All)
                    if (tracker.CheckTracked(place)) _tracked.Add(place);
            }
            if (added != null) _marked = added;
            else if (_marked == null || !_tracked.Contains(_marked)) _marked = seen == 1 ? any : null;
            return _marked;
        }

        private static bool IsTheFirstMarket(Settlement place)
        {
            string written = place?.Name?.Value ?? "";
            int id = written.StartsWith("{=", StringComparison.Ordinal) ? written.IndexOf('}') : -1;
            return written.Substring(id + 1) == FirstMarket || place?.Name?.ToString() == FirstMarket;
        }

        private static Settlement TheFirstMarket()
        {
            foreach (Settlement place in Settlement.All)
                if (place.IsTown && IsTheFirstMarket(place)) return place;
            return null;
        }

        private static void Stop(string why, bool pause)
        {
            On = false;
            if (pause)
            {
                try { if (Campaign.Current != null) Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop; }
                catch (Exception e) { Trail.Error(e, "pausing the game"); }
            }
            Trail.Write("Autopilot off: " + why);
            Say("Autopilot off: " + why);
        }

        private static void Say(string said)
        {
            try { InformationManager.DisplayMessage(new InformationMessage("Test Autopilot: " + said, new Color(1f, 0.55f, 0.2f))); }
            catch (Exception e) { Trail.Error(e, "showing a line on screen"); }
        }

        internal static int DayNow()
        {
            CampaignTimeModel model = Campaign.Current?.Models?.CampaignTimeModel;
            if (model == null) return 0;
            return (int)Math.Floor(CampaignTime.Now.ToDays) - (int)Math.Floor(model.CampaignStartTime.ToDays);
        }
    }

    internal static class Events
    {
        private static readonly string[] TouchesTrade =
        {
            "gold", "denar", "price", "trade", "goods", "item", "food", "workshop", "caravan", "merchant", "market"
        };

        internal static int Weight(List<string> said, bool chance, string nothing)
        {
            bool onlyNothing = true;
            int effects = 0;
            bool trade = false;
            foreach (string one in said)
            {
                if (string.IsNullOrEmpty(one) || one == nothing) continue;
                onlyNothing = false;
                effects++;
                string lower = one.ToLowerInvariant();
                foreach (string word in TouchesTrade)
                    if (lower.Contains(word)) trade = true;
            }
            if (onlyNothing && !chance) return said.Contains(nothing) ? 0 : 1;
            return (chance ? 1000 : 0) + (trade ? 100 : 0) + 10 + effects;
        }
    }

    internal static class Recruits
    {
        internal static void Fill(Settlement here)
        {
            MobileParty me = MobileParty.MainParty;
            Hero you = Hero.MainHero;
            VolunteerModel volunteers = Campaign.Current?.Models?.VolunteerModel;
            PartyWageModel wages = Campaign.Current?.Models?.PartyWageModel;
            if (me == null || you == null || volunteers == null || wages == null || here?.Notables == null) return;
            int room = me.Party.PartySizeLimit - me.MemberRoster.TotalManCount;
            if (room <= 0) return;
            var offered = new List<(Hero from, int at, CharacterObject troop, int cost)>();
            foreach (Hero notable in here.Notables)
            {
                if (notable == null || !notable.IsAlive || notable.VolunteerTypes == null) continue;
                int reach = volunteers.MaximumIndexHeroCanRecruitFromHero(you, notable, -101);
                for (int i = 0; i < notable.VolunteerTypes.Length && i < reach; i++)
                {
                    CharacterObject troop = notable.VolunteerTypes[i];
                    if (troop == null) continue;
                    int cost = (int)wages.GetTroopRecruitmentCost(troop, you, false).ResultNumber;
                    offered.Add((notable, i, troop, cost));
                }
            }
            offered.Sort((x, y) => x.troop.Tier != y.troop.Tier ? x.troop.Tier.CompareTo(y.troop.Tier) : x.cost.CompareTo(y.cost));
            var took = new List<string>();
            int paid = 0;
            foreach (var one in offered)
            {
                if (room <= 0) break;
                if (one.cost > you.Gold) continue;
                one.from.VolunteerTypes[one.at] = null;
                me.MemberRoster.AddToCounts(one.troop, 1);
                GiveGoldAction.ApplyBetweenCharacters(you, null, one.cost, true);
                CampaignEventDispatcher.Instance.OnUnitRecruited(one.troop, 1);
                CampaignEventDispatcher.Instance.OnTroopRecruited(you, here, one.from, one.troop, 1);
                room--;
                paid += one.cost;
                took.Add(one.troop.Name + " (tier " + one.troop.Tier + ", " + one.cost + " gold, from " + one.from.Name + ")");
            }
            if (took.Count == 0)
            {
                Trail.Write("room for " + room + " more in the party, and nobody at " + here.Name + " could be recruited");
                return;
            }
            Trail.Write("recruited at " + here.Name + " for " + paid + " gold: " + string.Join(", ", took.ToArray()));
        }
    }

    internal static class Focus
    {
        internal const int WarnsThisClose = 1;

        private static (int skill, int limit, int unspent) _toldStuck = (-1, -1, -1);

        internal static void OnTradeIfAtTheLimit()
        {
            Hero you = Hero.MainHero;
            CharacterDevelopmentModel model = Campaign.Current?.Models?.CharacterDevelopmentModel;
            SkillObject trade = DefaultSkills.Trade;
            HeroDeveloper growing = you?.HeroDeveloper;
            if (model == null || growing == null || trade == null) return;
            int focus = growing.GetFocus(trade);
            int skill = you.GetSkillValue(trade);
            int limit = MathF.Round(model.CalculateLearningLimit(you.CharacterAttributes, focus, trade).ResultNumber);
            if (skill < limit - WarnsThisClose) return;
            if (!growing.CanAddFocusToSkill(trade) || growing.UnspentFocusPoints < growing.GetRequiredFocusPointsToAddFocus(trade))
            {
                var now = (skill, limit, growing.UnspentFocusPoints);
                if (now == _toldStuck) return;
                _toldStuck = now;
                Trail.Write("Trade " + skill + " is at its learning limit of " + limit + ", and no focus point can go on Trade (" +
                            growing.UnspentFocusPoints + " unspent, " + focus + " on Trade)");
                return;
            }
            growing.AddFocus(trade, 1, true);
            Trail.Write("Trade " + skill + " reached its learning limit of " + limit + ": one focus point went on Trade, now " +
                        growing.GetFocus(trade) + ", " + growing.UnspentFocusPoints + " left unspent");
        }
    }

    internal static class Trail
    {
        private const string FileName = "TradeLordTestAutopilot.log";
        private const long MostItHolds = 999 * 1024;
        private static string _path;

        internal static void Start()
        {
            _path = Where();
            if (_path == null) return;
            try { if (File.Exists(_path) && new FileInfo(_path).Length > MostItHolds) File.WriteAllText(_path, ""); }
            catch { }
            Write("TradeLord Test Autopilot loaded");
        }

        private static string Where()
        {
            try
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string game = string.IsNullOrEmpty(docs) ? null : Path.Combine(docs, "Mount and Blade II Bannerlord");
                if (game != null && Directory.Exists(game)) return Path.Combine(game, FileName);
            }
            catch { }
            try
            {
                string own = Path.GetDirectoryName(typeof(Trail).Assembly.Location);
                if (!string.IsNullOrEmpty(own)) return Path.Combine(own, FileName);
            }
            catch { }
            return null;
        }

        internal static void Write(string message)
        {
            if (_path == null) return;
            try { File.AppendAllText(_path, DateTime.Now.ToString("s") + "  " + Clock() + "  " + message + Environment.NewLine); }
            catch { }
        }

        internal static void Error(Exception e, string where) => Write("ERROR in " + where + ": " + e);

        private static string Clock()
        {
            try
            {
                if (Campaign.Current == null) return "no campaign".PadRight(12);
                float hour = CampaignTime.Now.CurrentHourInDay;
                int minutes = hour <= 0f ? 0 : (int)Math.Min(hour * 60f, 24f * 60f - 1f);
                return ("day " + Pilot.DayNow() + " " + (minutes / 60).ToString("00") + ":" +
                        (minutes % 60).ToString("00")).PadRight(12);
            }
            catch { return "no clock".PadRight(12); }
        }
    }
}
