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

        private static readonly string[] Header =
        {
            "TradeLord settings.",
            "",
            "This file is read only when MCM is not installed. With MCM installed its",
            "settings screen is in charge and nothing here is read.",
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
            if (McmLoader.SettingsReachable) return;
            Guard.Run("Config", Read);
        }

        private static void Read()
        {
            string found = Log.Beside(FileName, mustExist: true);
            if (found == null)
            {
                Write(Log.Beside(FileName, mustExist: false), "no settings file yet");
                return;
            }

            var known = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (FieldInfo field in Fields()) known[field.Name] = field;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int taken = 0;
            foreach (string raw in File.ReadAllLines(found))
            {
                string line = raw.Trim();
                if (line.Length == 0 || line[0] == '#') continue;
                int mark = line.IndexOf('=');
                if (mark < 0) { Log.Write("settings file: '" + line + "' is not a name = value line, so it is ignored"); continue; }
                string name = line.Substring(0, mark).Trim();
                string written = line.Substring(mark + 1).Trim();
                if (!known.TryGetValue(name, out FieldInfo field))
                {
                    Log.Write("settings file: TradeLord has no setting called '" + name + "', so that line does nothing");
                    continue;
                }
                seen.Add(field.Name);
                if (Taken(field, written)) taken++;
            }

            Options.Bump();
            Log.Write("settings file read from " + found + ": " + taken + " of " + known.Count + " settings set");
            if (seen.Count < known.Count)
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

        private static string Shown(FieldInfo field)
        {
            object held = field.GetValue(Options.Current);
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
