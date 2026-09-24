using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace TradeLord
{
    internal static class Config
    {
        internal const string FileName = "TradeLord.ini";

        internal const string ChangedKey = "SettingsChanged";

        internal const string WrittenByKey = "SettingsWrittenBy";

        private const string ByScreen = "the settings screen";

        private const string ByFile = "this file";

        private static string _path;
        private static readonly TimeSpan Settling = TimeSpan.FromMilliseconds(400);

        private static DateTime _stillMoving;

        private static bool _dirty;
        private static bool _applying;
        private static bool _unreadable;
        private static Dictionary<string, string> _lastSeen;
        private static readonly List<KeyValuePair<string, string>> _newerLines =
            new List<KeyValuePair<string, string>>();
        private static int _newerShape;
        private static readonly Dictionary<string, (string taken, string written)> _newerValues =
            new Dictionary<string, (string taken, string written)>(StringComparer.OrdinalIgnoreCase);

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

        private static bool IsASetting(string name)
        {
            foreach (FieldInfo field in Fields())
                if (string.Equals(field.Name, name, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

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

        private static bool SayWhatChanged()
        {
            var now = Snapshot();
            if (_lastSeen == null) { _lastSeen = now; return false; }
            var stock = new Options();
            bool moved = false;
            foreach (FieldInfo field in Fields())
            {
                if (!now.TryGetValue(field.Name, out string held)) continue;
                if (_lastSeen.TryGetValue(field.Name, out string before) && before == held) continue;
                moved = true;
                string ships = Shown(field, stock);
                Log.Write("setting changed: " + field.Name + " is now " + held +
                          " (it was " + (before ?? "unset") + ", TradeLord ships with " + ships + ")" +
                          (held == ships ? " and is back at what it ships with" : ""));
            }
            _lastSeen = now;
            return moved;
        }

        private static void Noted()
        {
            if (_applying) return;
            _dirty = true;
            _stillMoving = DateTime.UtcNow;
        }

        internal static void Flush()
        {
            if (!_dirty) return;
            if (DateTime.UtcNow - _stillMoving < Settling) return;
            Settle();
        }

        internal static void Settle()
        {
            if (!_dirty) return;
            _dirty = false;
            bool moved = Guard.Read("Config.Changed", _path, _ => SayWhatChanged(), true);
            if (!moved) return;
            Guard.Run("Config.Flush", () => Write(_path, "a setting changed"));
        }

        private static string[] Lines(string path)
        {
            try { return File.ReadAllLines(path); }
            catch (Exception e)
            {
                _unreadable = true;
                Log.Error(e, "reading the settings file (it is left exactly as it is, so nothing you set in it " +
                             "is written over, and TradeLord runs on the settings it starts up with)");
                return null;
            }
        }

        private static bool ChangedByHand(string path, DateTime stamped)
        {
            if (stamped == default(DateTime)) return true;
            try { return Twins.ChangedByHand(File.GetLastWriteTimeUtc(path), stamped); }
            catch (Exception e)
            {
                Log.Error(e, "reading when the settings file was last changed (the file is taken as the newer one)");
                return true;
            }
        }

        private static void Read()
        {
            bool screen = McmLoader.SettingsInHand;
            string found = Log.Beside(FileName, mustExist: true);
            _path = found ?? Log.Beside(FileName, mustExist: false);
            if (found == null)
            {
                Write(_path, screen ? "written to match the settings screen" : "no settings file yet");
                return;
            }

            string[] lines = Lines(found);
            if (lines == null) return;

            var ignored = new List<string>();
            var written = SettingsFile.Read(lines, ignored);
            foreach (string line in ignored)
                Log.Write("settings file: '" + line + "' is not a name = value line, so it is ignored");

            int shape = 1;
            if (written.TryGetValue(Migration.ShapeKey, out string held) &&
                int.TryParse(held, NumberStyles.Integer, CultureInfo.InvariantCulture, out int stored))
                shape = stored;
            DateTime stamped = default(DateTime);
            if (written.TryGetValue(ChangedKey, out string marked))
                DateTime.TryParse(marked, CultureInfo.InvariantCulture,
                                  DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out stamped);
            bool screenWroteIt = written.TryGetValue(WrittenByKey, out string wroteIt) &&
                                 string.Equals(wroteIt, ByScreen, StringComparison.Ordinal);
            written.Remove(Migration.ShapeKey);
            written.Remove(ChangedKey);
            written.Remove(WrittenByKey);

            var notes = new List<string>();
            bool lifted = Migration.Lift(shape, written, notes);
            bool newer = shape > Migration.Shape;
            _newerLines.Clear();
            _newerValues.Clear();
            _newerShape = newer ? shape : 0;
            if (newer)
                foreach (var line in written)
                    if (!IsASetting(line.Key)) _newerLines.Add(line);
            bool whipped = Whip.CracksOn(shape);
            if (!whipped)
                foreach (string note in notes) Log.Write("settings file: " + note);
            if (whipped)
            {
                SayWhatYouHadSet(written);
                Whip.Crack(shape, written);
                BackToWhatItShipsWith();
                Log.Write("settings file: this version puts every setting back to the value TradeLord ships with, " +
                          "because the settings it ships with trade better than they used to. Anything you had " +
                          "set is listed above so you can put it back. While TradeLord's trading is settling, " +
                          "an update may do this again.");
                McmLoader.PutBackWhatItShipsWithOnceTheScreenArrives();
            }

            bool handEdited = screen && screenWroteIt && ChangedByHand(found, stamped);
            if (Twins.ScreenWins(screen, screenWroteIt, handEdited))
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
                        if (newer)
                        {
                            Log.Write("settings file: '" + line.Key + "' is a setting from a newer TradeLord, so " +
                                      "this one leaves it in the file for that version to read");
                            continue;
                        }
                        Log.Write("settings file: TradeLord has no setting called '" + line.Key + "', so that line does nothing");
                        continue;
                    }
                    seen.Add(field.Name);
                    if (Taken(field, line.Value)) taken++;
                    if (newer && Shown(field) != line.Value) _newerValues[field.Name] = (Shown(field), line.Value);
                }
            }
            finally { _applying = false; }

            Options.Bump();
            Log.Write("settings file read from " + found + ": " + taken + " of " + known.Count + " settings set");
            if (screen)
            {
                Log.Write(screenWroteIt
                    ? "settings file: this file was saved more recently than the settings screen, so the screen is set from it"
                    : "settings file: this file was last written with no settings screen to write it, so the screen is set from it");
                McmLoader.Reseat?.Invoke();
            }
            if (screen)
                Write(found, "made the settings screen match it");
            else if (whipped)
                Write(found, "every setting put back to what TradeLord ships with");
            else if (newer)
                Log.Write("settings file: it was written by a newer TradeLord, in shape " + shape + " where this " +
                          "one reads shape " + Migration.Shape + ", so it is left exactly as it is");
            else if (lifted || shape != Migration.Shape)
                Write(found, "brought forward from shape " + shape + " to shape " + Migration.Shape);
            else if (seen.Count < known.Count)
                Write(found, "the file was missing " + (known.Count - seen.Count) + " setting(s) this version knows");
        }

        private static void SayWhatYouHadSet(IDictionary<string, string> written)
        {
            var stock = new Options();
            var known = new Dictionary<string, FieldInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (FieldInfo field in Fields()) known[field.Name] = field;
            int away = 0;
            foreach (var line in written)
            {
                if (!known.TryGetValue(line.Key, out FieldInfo field)) continue;
                string ships = Shown(field, stock);
                if (line.Value == ships) continue;
                away++;
                Log.Write("  you had " + field.Name + " = " + line.Value +
                          " (TradeLord ships with " + ships + ")");
            }
            Log.Write(away == 0
                ? "  every setting in your file was already at the value TradeLord ships with"
                : "  " + away + " setting(s) of yours are listed above");
        }

        private static void BackToWhatItShipsWith()
        {
            var stock = new Options();
            foreach (FieldInfo field in Fields()) field.SetValue(Options.Current, field.GetValue(stock));
        }

        private static bool Taken(FieldInfo field, string written)
        {
            Options at = Options.Current;
            try
            {
                if (field.FieldType == typeof(bool)) field.SetValue(at, bool.Parse(written));
                else if (field.FieldType == typeof(int))
                    field.SetValue(at, (int)Within(field, int.Parse(written, CultureInfo.InvariantCulture)));
                else if (field.FieldType == typeof(float))
                    field.SetValue(at, (float)Within(field, float.Parse(written, NumberStyles.Float, CultureInfo.InvariantCulture)));
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

        private static double Within(FieldInfo field, double asked)
        {
            double kept = Limits.Kept(field.Name, asked);
            if (kept != asked)
                Log.Write("settings file: " + field.Name + " has to be between " + Limits.Range(field.Name) +
                          ", so " + Said(asked) + " was taken as " + Said(kept));
            return kept;
        }

        private static string Said(double number) =>
            number.ToString("0.####", CultureInfo.InvariantCulture);

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
            if (_unreadable)
            {
                Log.Repeatable("settings file unreadable", "left alone",
                               "settings file: it could not be read this session, so it is left exactly as it " +
                               "is rather than written over with settings that never came out of it");
                return;
            }
            var lines = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(Migration.ShapeKey,
                    Math.Max(Migration.Shape, _newerShape).ToString(CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>(ChangedKey,
                    DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)),
                new KeyValuePair<string, string>(WrittenByKey,
                    McmLoader.SettingsInHand ? ByScreen : ByFile),
            };
            foreach (FieldInfo field in Fields())
            {
                string shown = Shown(field);
                if (_newerValues.TryGetValue(field.Name, out var kept) && kept.taken == shown) shown = kept.written;
                lines.Add(new KeyValuePair<string, string>(field.Name, shown));
            }
            lines.AddRange(_newerLines);
            try
            {
                File.WriteAllText(path, SettingsFile.Compose(Header, lines));
                Log.Write("settings file written to " + path + " (" + why + ") - edit it to change how TradeLord trades");
            }
            catch (Exception e) { Log.Error(e, "writing the settings file (TradeLord runs on the settings it has)"); }
        }
    }
}
