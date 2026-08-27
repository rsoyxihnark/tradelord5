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
        private static bool Loaded(string prefix)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Any(a => (a.GetName().Name ?? "").StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        }

        internal static void TryLoad()
        {
            bool mcm = Loaded("MCMv5");
            if (!mcm) { try { mcm = Assembly.Load("MCMv5") != null; } catch { } }
            if (!mcm)
            {
                Log.Write("MCM not detected - running on built-in defaults");
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
            Log.Write("MCM detected - settings menu registered");
        }
    }

    internal static class Log
    {
        private const string FileName = "TradeLord.log";
        private static string _path;
        private static bool _resolved;

        private static List<string> Candidates()
        {
            var paths = new List<string>();
            try
            {
                string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (!string.IsNullOrEmpty(docs))
                    paths.Add(Path.Combine(Path.Combine(docs, "Mount and Blade II Bannerlord"), FileName));
            }
            catch { }
            try
            {
                string own = Path.GetDirectoryName(typeof(Log).Assembly.Location);
                if (!string.IsNullOrEmpty(own)) paths.Add(Path.Combine(own, FileName));
            }
            catch { }
            paths.Add(FileName);
            return paths;
        }

        private static string Resolve()
        {
            foreach (string candidate in Candidates())
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
