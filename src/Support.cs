using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;

namespace TradeLord
{
    internal static class McmLoader
    {
        private const int McmGeneration = 5;
        private const int GenerationsAhead = 2;

        internal static bool SettingsInHand { get; private set; }

        internal static Action Reseat;

        internal static Action PutBackWhatItShipsWith;

        private static bool _owedAPutBack;

        internal static void PutBackWhatItShipsWithOnceTheScreenArrives()
        {
            if (PutBackWhatItShipsWith == null) return;
            if (SettingsInHand) { PutBack(); return; }
            _owedAPutBack = true;
            Log.Write("settings file: MCM has not handed its settings over yet, and it keeps a copy of its own, " +
                      "so putting every setting back waits until it does");
        }

        private static void PutBack()
        {
            _owedAPutBack = false;
            Log.Write("settings file: the settings screen was holding the settings you had, so they are put back " +
                      "to what TradeLord ships with there too, and the list above still stands");
            Guard.Run("Mcm.PutBack", PutBackWhatItShipsWith);
        }

        private static MethodInfo _handover;
        private static DateTime _askedAt;
        private static readonly TimeSpan BetweenAsks = TimeSpan.FromSeconds(1);

        private static string Named(int generation) => Screens.Named(generation);

        private static bool Loaded(string prefix)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => (a.GetName().Name ?? "").StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<string> LoadedNames()
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                yield return a.GetName().Name;
        }

        private static string Detect()
        {
            string found = Screens.Which(LoadedNames(), Named(McmGeneration));
            if (found != null) return found;
            for (int g = McmGeneration; g <= McmGeneration + GenerationsAhead; g++)
                try { if (Assembly.Load(Named(g)) != null) return Named(g); }
                catch { }
            return null;
        }

        internal static void TryLoad()
        {
            string found = Detect();
            if (found == null)
            {
                Log.Write("MCM not detected - running on built-in defaults");
                return;
            }
            if (!string.Equals(found, Named(McmGeneration), StringComparison.OrdinalIgnoreCase))
            {
                Log.Write("MCM is installed, but this build of TradeLord was made against " + Named(McmGeneration) +
                          " and the game has loaded " + found + " - TradeLord runs on built-in defaults. Trading is " +
                          "unaffected; only the settings screen is missing. Update TradeLord to a build made for " +
                          found + ", or run the " + Named(McmGeneration) + " line of MCM alongside it.");
                return;
            }

            if (!Loaded("Bannerlord.ButterLib") || !Loaded("Bannerlord.UIExtenderEx"))
            {
                Log.Write("MCM is installed but its ButterLib/UIExtenderEx stack is not fully loaded - TradeLord runs on defaults. Enable Bannerlord.ButterLib AND Bannerlord.UIExtenderEx alongside MCM, or remove the MCM stack entirely.");
                return;
            }
            string dir = Path.GetDirectoryName(typeof(McmLoader).Assembly.Location);
            string companion = Path.Combine(dir ?? "", "TradeLord.MCM.dll");
            if (!File.Exists(companion))
            {
                Log.Write("MCM detected but TradeLord.MCM.dll is missing - defaults in effect");
                return;
            }
            Assembly asm = Assembly.LoadFrom(companion);
            MethodInfo init = asm.GetType("TradeLord.Mcm.McmSettingsBootstrap")?.GetMethod("Init");
            if (init == null)
            {
                Log.Write("TradeLord.MCM.dll loaded but TradeLord.Mcm.McmSettingsBootstrap.Init is missing - defaults in effect. The companion DLL is from a different TradeLord version; reinstall the module.");
                return;
            }
            _handover = init;
            object answered = init.Invoke(null, null);
            SettingsInHand = answered is bool taken && taken;
            Log.Write(SettingsInHand
                ? "MCM detected - settings menu registered"
                : "MCM detected, but it has not handed over its settings yet - TradeLord.ini is read as it stands, and the settings screen takes over once MCM has loaded");
        }

        internal static void TryHandover()
        {
            if (SettingsInHand || _handover == null) return;
            if (DateTime.UtcNow - _askedAt < BetweenAsks) return;
            _askedAt = DateTime.UtcNow;
            if (!(_handover.Invoke(null, null) is bool taken) || !taken) return;
            SettingsInHand = true;
            Log.Write("MCM has handed its settings over - the settings screen is in charge from here, and what you pick on it takes hold as you pick it");
            if (_owedAPutBack) PutBack();
        }
    }

    internal static class Log
    {
        private const string FileName = "TradeLord.log";
        private static string _path;
        private static bool _resolved;

        private static List<string> Candidates(string fileName)
        {
            var paths = new List<string>();
            try
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (!string.IsNullOrEmpty(docs))
                    paths.Add(Path.Combine(Path.Combine(docs, "Mount and Blade II Bannerlord"), fileName));
            }
            catch { }
            try
            {
                string own = Path.GetDirectoryName(typeof(Log).Assembly.Location);
                if (!string.IsNullOrEmpty(own)) paths.Add(Path.Combine(own, fileName));
            }
            catch { }
            paths.Add(fileName);
            return paths;
        }

        internal static string Beside(string fileName, bool mustExist)
        {
            foreach (string candidate in Candidates(fileName))
            {
                try
                {
                    if (mustExist)
                    {
                        if (File.Exists(candidate)) return candidate;
                        continue;
                    }
                    string dir = Path.GetDirectoryName(candidate);
                    if (string.IsNullOrEmpty(dir) || Directory.Exists(dir)) return candidate;
                }
                catch { }
            }
            return null;
        }

        private static string Resolve()
        {
            foreach (string candidate in Candidates(FileName))
            {
                try
                {
                    string dir = Path.GetDirectoryName(candidate);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) continue;
                    File.AppendAllText(candidate, "");
                    return candidate;
                }
                catch { }
            }
            return null;
        }

        private const long MostItHolds = 999 * 1024;

        private static StreamWriter _open;
        private static DateTime _holdAgainAt;
        private static readonly TimeSpan BeforeHoldingAgain = TimeSpan.FromSeconds(30);
        private static string _emptied;

        private static StreamWriter Held()
        {
            if (_open != null || DateTime.UtcNow < _holdAgainAt) return _open;
            try
            {
                _open = new StreamWriter(new FileStream(
                    _path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                { AutoFlush = false };
            }
            catch { LetGo(); }
            return _open;
        }

        private static void LetGo()
        {
            _holdAgainAt = DateTime.UtcNow + BeforeHoldingAgain;
            try { _open?.Dispose(); } catch { }
            _open = null;
        }

        private static void EmptyIfItOutgrewItsLimit()
        {
            long held;
            try { held = new FileInfo(_path).Length; }
            catch { return; }
            if (held <= MostItHolds) return;
            try { File.WriteAllText(_path, ""); }
            catch { return; }
            _emptied = "TradeLord.log had grown to " + held / 1024 + " KB, past the " + MostItHolds / 1024 +
                       " KB it is allowed, so it was emptied as the game started. It is only ever emptied " +
                       "as the game starts, never as the game closes, so everything this session writes " +
                       "stays in the file for you to send on.";
        }

        internal static void Write(string message)
        {
            if (!Ready()) return;
            Put(message);
            Pushed();
        }

        internal static void WriteMany(List<string> messages)
        {
            if (messages == null || messages.Count == 0 || !Ready()) return;
            for (int i = 0; i < messages.Count; i++) Put(messages[i]);
            Pushed();
        }

        private static bool Ready()
        {
            if (!_resolved)
            {
                _resolved = true;
                _path = Resolve();
                if (_path != null) EmptyIfItOutgrewItsLimit();
            }
            if (_path == null) return false;
            if (_emptied != null)
            {
                string said = _emptied;
                _emptied = null;
                Put(said);
            }
            return true;
        }

        private static void Pushed()
        {
            StreamWriter held = _open;
            if (held == null) return;
            try { held.Flush(); }
            catch { LetGo(); }
        }

        private static void Put(string message)
        {
            string line = DateTime.Now.ToString("s") + "  " + message;
            StreamWriter held = Held();
            if (held != null)
            {
                try { held.WriteLine(line); return; }
                catch { LetGo(); }
            }
            try { File.AppendAllText(_path, line + Environment.NewLine); }
            catch { }
        }

        private static readonly Dictionary<string, string> _repeats = new Dictionary<string, string>();

        internal static void Repeatable(string key, string signature, string message)
        {
            _repeats.TryGetValue(key, out string seen);
            if (seen == signature) return;
            _repeats[key] = signature;
            Write(message);
        }

        private static readonly Dictionary<string, int> _errors = new Dictionary<string, int>();

        internal static void Error(Exception e, string context)
        {
            _errors.TryGetValue(context, out int seen);
            _errors[context] = seen + 1;
            if (seen == 0) Write("ERROR in " + context + ": " + e);
            else if (seen == 1) Write("ERROR in " + context + " is recurring - not reporting it again");
        }

        internal static void Forget()
        {
            _repeats.Clear();
            _errors.Clear();
        }
    }

    internal static class Freshness
    {
        internal static int Hour => (int)CampaignTime.Now.ToHours;

        internal static bool Fresh(ref Stamp stamp) => stamp.Fresh(Hour, Options.Generation);

        internal static bool Fresh(ref Stamp stamp, int hour) => stamp.Fresh(hour, Options.Generation);

        internal static void Taken(ref Stamp stamp) => stamp.Taken(Hour, Options.Generation);

        internal static void Taken(ref Stamp stamp, int hour) => stamp.Taken(hour, Options.Generation);

        internal static Stamp At(int hour)
        {
            Stamp stamp = default(Stamp);
            stamp.Taken(hour, Options.Generation);
            return stamp;
        }

        internal static bool Held(Stamp stamp, int hour) => stamp.Fresh(hour, Options.Generation);
    }

    internal static class Guard
    {
        internal static void Run(string context, Action action)
        {
            try { action(); }
            catch (Exception e) { Log.Error(e, context); }
        }

        internal static void Run<T>(string context, T with, Action<T> action)
        {
            try { action(with); }
            catch (Exception e) { Log.Error(e, context); }
        }

        internal static TAnswer Read<T, TAnswer>(string context, T with, Func<T, TAnswer> read, TAnswer ifItFails)
        {
            try { return read(with); }
            catch (Exception e) { Log.Error(e, context); return ifItFails; }
        }
    }

    internal static class Patcher
    {
        private static readonly List<string> Applied = new List<string>();
        private static readonly List<string> Refused = new List<string>();

        internal static string Tally() => Tallies.Of(Applied.Count, Refused);

        internal static bool Holds(string patchClass) => Applied.Contains(patchClass);

        internal static void TryPatch(Harmony harmony, Type patchClass)
        {
            try
            {
                harmony.CreateClassProcessor(patchClass).Patch();
                Applied.Add(patchClass.Name);
            }
            catch (Exception e)
            {
                Refused.Add(patchClass.Name);
                Log.Error(e, "patching " + patchClass.Name + " (feature disabled, mod continues)");
            }
        }
    }
}
