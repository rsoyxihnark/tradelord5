using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace TradeLord
{
    internal static class Config
    {
        internal const string FileName = "TradeLord.ini";

        internal const string ChangedKey = "SettingsChanged";

        internal const string WrittenByKey = "SettingsWrittenBy";

        private static readonly TimeSpan HandTolerance = TimeSpan.FromSeconds(30);

        private static string _path;
        private static bool _dirty;
        private static bool _applying;
        private static Dictionary<string, string> _lastSeen;

        private static readonly string[] Header =
        {
            "TradeLord settings.",
            "",
            "This file and the MCM settings screen are twins. Whichever was saved last wins,",
            "and TradeLord writes the other to match it, so you can install or remove MCM",
            "whenever you like and keep every setting you have made.",
            "",
            "The " + Migration.ShapeKey + " line says which shape this file is in. TradeLord reads it",
            "and brings an older file forward by itself, writing what it changed to TradeLord.log,",
            "so a setting is never lost when TradeLord changes how one works.",
            "",
            "One setting per line, written as name = value. Lines starting with # are",
            "ignored. A name TradeLord does not know, or a value it cannot read, is",
            "reported in TradeLord.log and left at the value shown here.",
            "",
            "Numbers with a decimal point are written with a dot, whatever your language.",
            "The four item lists take item ids or the names the game shows, comma separated.",
        };

        private static FieldInfo[] Fields() =>
            typeof(Options).GetFields(BindingFlags.Public | BindingFlags.Instance);

        internal static void Follow()
        {
            Guard.Run("Config", Read);
            Guard.Run("Config.Away", SayWhatIsAwayFromStock);
            _lastSeen = Snapshot();
            Options.Changed = Noted;
        }

        private static Dictionary<string, string> Snapshot()
        {
            var held = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (FieldInfo field in Fields()) held[field.Name] = Shown(field);
            return held;
        }

        private static void SayWhatIsAwayFromStock()
        {
            var stock = new Options();
            int away = 0;
            foreach (FieldInfo field in Fields())
            {
                string now = Shown(field), ships = Shown(field, stock);
                if (now == ships) continue;
                away++;
                Log.Write("  " + field.Name + " = " + now + " (TradeLord ships with " + ships + ")");
            }
            Log.Write(away == 0
                ? "every setting is at the value TradeLord ships with"
                : away + " setting(s) are away from what TradeLord ships with, listed above");
        }

        private static void SayWhatChanged()
        {
            var now = Snapshot();
            if (_lastSeen == null) { _lastSeen = now; return; }
            var stock = new Options();
            foreach (FieldInfo field in Fields())
            {
                if (!now.TryGetValue(field.Name, out string held)) continue;
                if (_lastSeen.TryGetValue(field.Name, out string before) && before == held) continue;
                string ships = Shown(field, stock);
                Log.Write("setting changed: " + field.Name + " is now " + held +
                          " (it was " + (before ?? "unset") + ", TradeLord ships with " + ships + ")" +
                          (held == ships ? " and is back at what it ships with" : ""));
            }
            _lastSeen = now;
        }

        private static void Noted()
        {
            if (!_applying) _dirty = true;
        }

        internal static void Flush()
        {
            if (!_dirty) return;
            _dirty = false;
            Guard.Run("Config.Changed", SayWhatChanged);
            Guard.Run("Config.Flush", () => Write(_path, "a setting changed"));
        }

        private static bool ChangedByHand(string path, DateTime stamped)
        {
            if (stamped == default(DateTime)) return true;
            try { return File.GetLastWriteTimeUtc(path) > stamped + HandTolerance; }
            catch (Exception e)
            {
                Log.Error(e, "reading when the settings file was last changed (the file is taken as the newer one)");
                return true;
            }
        }

        private static void Read()
        {
            bool screen = McmLoader.SettingsReachable;
            string found = Log.Beside(FileName, mustExist: true);
            _path = found ?? Log.Beside(FileName, mustExist: false);
            if (found == null)
            {
                Write(_path, screen ? "written to match the settings screen" : "no settings file yet");
                return;
            }

            var written = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string raw in File.ReadAllLines(found))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                int mark = line.IndexOf('=');
                if (mark < 0) { Log.Write("settings file: '" + line + "' is not a name = value line, so it is ignored"); continue; }
                written[line.Substring(0, mark).Trim()] = line.Substring(mark + 1).Trim();
            }

            int shape = 1;
            if (written.TryGetValue(Migration.ShapeKey, out string held) &&
                int.TryParse(held, NumberStyles.Integer, CultureInfo.InvariantCulture, out int stored))
                shape = stored;
            DateTime stamped = default(DateTime);
            if (written.TryGetValue(ChangedKey, out string marked))
                DateTime.TryParse(marked, CultureInfo.InvariantCulture,
                                  DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out stamped);
            written.Remove(Migration.ShapeKey);
            written.Remove(ChangedKey);
            written.Remove(WrittenByKey);

            var notes = new List<string>();
            bool lifted = Migration.Lift(shape, written, notes);
            foreach (string note in notes) Log.Write("settings file: " + note);

            if (screen && !ChangedByHand(found, stamped))
            {
                Log.Write("settings file: the settings screen was saved more recently, so this file is written to match it");
                Write(found, "made to match the settings screen");
                return;
            }

            var known = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (FieldInfo field in Fields()) known[field.Name] = field;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int taken = 0;
            _applying = true;
            try
            {
                foreach (var line in written)
                {
                    if (!known.TryGetValue(line.Key, out FieldInfo field))
                    {
                        Log.Write("settings file: TradeLord has no setting called '" + line.Key + "', so that line does nothing");
                        continue;
                    }
                    seen.Add(field.Name);
                    if (Taken(field, line.Value)) taken++;
                }
            }
            finally { _applying = false; }

            Options.Bump();
            Log.Write("settings file read from " + found + ": " + taken + " of " + known.Count + " settings set");
            if (screen)
            {
                Log.Write("settings file: this file was saved more recently than the settings screen, so the screen is set from it");
                McmLoader.Reseat?.Invoke();
            }
            if (screen)
                Write(found, "made the settings screen match it");
            else if (lifted || shape != Migration.Shape)
                Write(found, "brought forward from shape " + shape + " to shape " + Migration.Shape);
            else if (seen.Count < known.Count)
                Write(found, "the file was missing " + (known.Count - seen.Count) + " setting(s) this version knows");
        }

        private static bool Taken(FieldInfo field, string written)
        {
            Options at = Options.Current;
            try
            {
                if (field.FieldType == typeof(bool)) field.SetValue(at, bool.Parse(written));
                else if (field.FieldType == typeof(int)) field.SetValue(at, int.Parse(written, CultureInfo.InvariantCulture));
                else if (field.FieldType == typeof(float)) field.SetValue(at, float.Parse(written, NumberStyles.Float, CultureInfo.InvariantCulture));
                else if (field.FieldType == typeof(string)) field.SetValue(at, written);
                else
                {
                    Log.Write("settings file: " + field.Name + " is not a kind of value this file can carry, so it is left alone");
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                Log.Write("settings file: '" + written + "' is not a value " + field.Name +
                          " can take, so it stays at " + Shown(field));
                return false;
            }
        }

        private static string Shown(FieldInfo field) => Shown(field, Options.Current);

        private static string Shown(FieldInfo field, Options of)
        {
            object held = field.GetValue(of);
            if (held is bool flag) return flag ? "true" : "false";
            if (held is float number) return number.ToString("0.####", CultureInfo.InvariantCulture);
            return Convert.ToString(held, CultureInfo.InvariantCulture) ?? "";
        }

        private static void Write(string path, string why)
        {
            if (path == null)
            {
                Log.Write("settings file: nowhere to write one, so TradeLord runs on its built-in settings");
                return;
            }
            var sb = new StringBuilder();
            foreach (string line in Header)
                sb.AppendLine(line.Length == 0 ? "#" : "# " + line);
            sb.AppendLine();
            sb.Append(Migration.ShapeKey).Append(" = ")
              .AppendLine(Migration.Shape.ToString(CultureInfo.InvariantCulture));
            sb.Append(ChangedKey).Append(" = ")
              .AppendLine(DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            sb.Append(WrittenByKey).Append(" = ")
              .AppendLine(McmLoader.SettingsReachable ? "the settings screen" : "this file");
            foreach (FieldInfo field in Fields())
                sb.Append(field.Name).Append(" = ").AppendLine(Shown(field));
            try
            {
                File.WriteAllText(path, sb.ToString());
                Log.Write("settings file written to " + path + " (" + why + ") - edit it to change how TradeLord trades");
            }
            catch (Exception e) { Log.Error(e, "writing the settings file (TradeLord runs on the settings it has)"); }
        }
    }
}
