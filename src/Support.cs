using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace TradeLord
{
    internal static class McmLoader
    {
        private const int McmGeneration = 5;
        private const int GenerationsAhead = 2;

        internal static bool SettingsReachable { get; private set; }

        internal static Action Reseat;

        private static string Named(int generation) => "MCMv" + generation;

        private static bool Loaded(string prefix)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => (a.GetName().Name ?? "").StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        private static string GenerationOf(string assemblyName)
        {
            string name = assemblyName ?? "";
            if (!name.StartsWith("MCMv", StringComparison.OrdinalIgnoreCase)) return null;
            int end = 4;
            while (end < name.Length && char.IsDigit(name[end])) end++;
            return end > 4 ? name.Substring(0, end) : null;
        }

        private static string Detect()
        {
            string other = null;
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                string generation = GenerationOf(a.GetName().Name);
                if (generation == null) continue;
                if (string.Equals(generation, Named(McmGeneration), StringComparison.OrdinalIgnoreCase))
                    return generation;
                if (other == null) other = generation;
            }
            if (other != null) return other;
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
            init.Invoke(null, null);
            SettingsReachable = true;
            Log.Write("MCM detected - settings menu registered");
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
                    File.WriteAllText(candidate, "");
                    return candidate;
                }
                catch { }
            }
            return null;
        }

        internal static void Write(string message)
        {
            if (!_resolved) { _resolved = true; _path = Resolve(); }
            if (_path == null) return;
            string line = DateTime.Now.ToString("s") + "  " + message + Environment.NewLine;
            try { File.AppendAllText(_path, line); }
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

    internal static class Guard
    {
        internal static void Run(string context, Action action)
        {
            try { action(); }
            catch (Exception e) { Log.Error(e, context); }
        }
    }

    internal static class Patcher
    {
        internal static void TryPatch(Harmony harmony, Type patchClass)
        {
            try
            {
                harmony.CreateClassProcessor(patchClass).Patch();
            }
            catch (Exception e)
            {
                Log.Error(e, "patching " + patchClass.Name + " (feature disabled, mod continues)");
            }
        }
    }
}
