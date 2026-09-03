using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.Localization;

namespace TradeLord
{
    internal static class Tongue
    {
        internal const int English = 0, Turkish = 1, Russian = 2;

        private static Dictionary<string, string> _said;
        private static int _saidFor = English - 1;

        internal static TextObject Text(string written)
        {
            if (Options.Current.Language == English) return new TextObject(written);
            string said = Translated(Id(written));
            return new TextObject(said ?? written);
        }

        internal static string Said(string written)
        {
            return Options.Current.Language == English ? null : Translated(Id(written));
        }

        internal static string Plain(string written)
        {
            string id = Id(written);
            return id == null ? written : written.Substring(id.Length + 3);
        }

        private static string Id(string written)
        {
            if (written == null || !written.StartsWith("{=")) return null;
            int close = written.IndexOf('}');
            return close > 2 ? written.Substring(2, close - 2) : null;
        }

        private static string Translated(string id)
        {
            if (id == null) return null;
            int language = Options.Current.Language;
            if (_saidFor != language)
            {
                _saidFor = language;
                _said = null;
                Guard.Run("Tongue.Read", () => _said = Read(Where(language)));
                if (_said == null || _said.Count == 0)
                    Log.Write("the " + Named(language) + " strings could not be read from the module folder - TradeLord speaks English");
                else
                    Log.Write(Named(language) + " selected - " + _said.Count + " strings read from the module folder");
            }
            return _said != null && _said.TryGetValue(id, out string text) ? text : null;
        }

        private static string Named(int language) => language == Russian ? "Russian" : "Turkish";

        private static string Where(int language)
        {
            string bin = Path.GetDirectoryName(typeof(Tongue).Assembly.Location);
            string module = Path.GetDirectoryName(Path.GetDirectoryName(bin));
            return language == Russian
                ? Path.Combine(module ?? "", "ModuleData", "Languages", "RU", "module_strings_ru.xml")
                : Path.Combine(module ?? "", "ModuleData", "Languages", "TR", "module_strings_tr.xml");
        }

        private static Dictionary<string, string> Read(string path)
        {
            var said = new Dictionary<string, string>(StringComparer.Ordinal);
            if (!File.Exists(path)) return said;
            var doc = new XmlDocument();
            doc.Load(path);
            XmlNodeList lines = doc.SelectNodes("/base/strings/string");
            for (int i = 0; lines != null && i < lines.Count; i++)
            {
                XmlAttributeCollection at = lines[i].Attributes;
                string id = at?["id"]?.Value;
                string text = at?["text"]?.Value;
                if (!string.IsNullOrEmpty(id) && text != null) said[id] = text;
            }
            return said;
        }
    }
}
