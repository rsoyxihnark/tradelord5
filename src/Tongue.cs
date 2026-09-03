using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TaleWorlds.Localization;

namespace TradeLord
{
    internal static class Tongue
    {
        internal const int English = 0, Turkish = 1;

        private static Dictionary<string, string> _said;
        private static bool _read;

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

        private static string Id(string written)
        {
            if (written == null || !written.StartsWith("{=")) return null;
            int close = written.IndexOf('}');
            return close > 2 ? written.Substring(2, close - 2) : null;
        }

        private static string Translated(string id)
        {
            if (id == null) return null;
            if (!_read)
            {
                _read = true;
                Guard.Run("Tongue.Read", () => _said = Read(Where()));
                if (_said == null || _said.Count == 0)
                    Log.Write("the Turkish strings could not be read from the module folder - TradeLord speaks English");
                else
                    Log.Write("Turkish selected - " + _said.Count + " strings read from the module folder");
            }
            return _said != null && _said.TryGetValue(id, out string text) ? text : null;
        }

        private static string Where()
        {
            string bin = Path.GetDirectoryName(typeof(Tongue).Assembly.Location);
            string module = Path.GetDirectoryName(Path.GetDirectoryName(bin));
            return Path.Combine(module ?? "", "ModuleData", "Languages", "TR", "module_strings_tr.xml");
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
