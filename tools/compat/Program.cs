using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace TradeLord.Compat
{
    internal static class Program
    {
        private const string Inventory = "TaleWorlds.CampaignSystem.ViewModelCollection.Inventory.";

        private static readonly (string type, string member)[] HarmonyTargets =
        {
            (Inventory + "ItemMenuVM", "RefreshItemTooltips"),
            (Inventory + "ItemMenuVM", "SetMerchandiseComponentTooltip"),
            (Inventory + "SPItemVM", "UpdateProfitType"),
            ("TaleWorlds.Library.InformationManager", "DisplayMessage"),
        };

        private static readonly (string type, string member)[] ReflectedMethods =
        {
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "GetHerdingModifier"),
        };

        private static readonly (string type, string member)[] ReflectedFields =
        {
            (Inventory + "ItemMenuVM", "_targetItem"),
        };

        private static readonly string[] Enums =
        {
            "TaleWorlds.CampaignSystem.ComponentInterfaces.SettlementAccessModel+SettlementAction",
            "TaleWorlds.CampaignSystem.GameMenus.GameMenuOption+LeaveType",
            "TaleWorlds.CampaignSystem.Inventory.InventoryLogic+InventorySide",
            "TaleWorlds.CampaignSystem.Settlements.Village+VillageStates",
            "TaleWorlds.CampaignSystem.Party.MobileParty+NavigationType",
            "TaleWorlds.Core.ItemObject+ItemTiers",
            "TaleWorlds.Core.ViewModelCollection.Information.TooltipProperty+TooltipPropertyFlags",
            "TaleWorlds.InputSystem.InputKey",
        };

        private static readonly string[] ModAssemblies =
        {
            "src/bin/Release/net472/TradeLord.dll",
            "mcm/bin/Release/net472/TradeLord.MCM.dll",
        };

        private static string _repo;
        private static string _nuget;
        private static readonly Dictionary<string, MetadataLoadContext> Loaded = new();
        private static readonly List<string> Failures = new();
        private static readonly List<string> Notes = new();

        private static int Main(string[] args)
        {
            _repo = RepoRoot();
            _nuget = Environment.GetEnvironmentVariable("NUGET_PACKAGES")
                     ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                                     ".nuget", "packages");

            var versions = new List<string> { BuiltAgainst() };
            foreach (string v in args) if (!versions.Contains(v)) versions.Add(v);
            if (versions.Count == 1)
            {
                Console.WriteLine("usage: dotnet run --project tools/compat -- <game version> [more versions]");
                Console.WriteLine("       the version in src/TradeLord.csproj is always the baseline");
                Console.WriteLine();
                Console.WriteLine("example: dotnet run --project tools/compat -- 1.4.8.119303 1.5.1.120547-beta");
                return 2;
            }

            foreach (string dll in ModAssemblies)
                if (!File.Exists(Path.Combine(_repo, dll)))
                {
                    Console.Error.WriteLine("missing " + dll + " - build both projects in Release first");
                    return 2;
                }

            foreach (string v in versions)
            {
                if (!Fetch(v)) return 2;
                Loaded[v] = Open(v);
            }

            Console.WriteLine();
            Console.WriteLine("baseline " + versions[0] + ", compared against "
                              + string.Join(", ", versions.Skip(1)));
            Console.WriteLine();

            CheckAssemblyIdentity(versions);
            CheckHarmonyTargets(versions);
            CheckReflectedMembers(versions);
            CheckEnums(versions);
            CheckBoundSurface(versions);

            Console.WriteLine();
            foreach (string n in Notes) Console.WriteLine("note    " + n);
            if (Failures.Count == 0)
            {
                Console.WriteLine();
                Console.WriteLine("the mod fits every version checked");
                return 0;
            }
            Console.WriteLine();
            foreach (string f in Failures) Console.WriteLine("BROKEN  " + f);
            Console.WriteLine();
            Console.WriteLine(Failures.Count + " break(s) - the mod does not fit every version checked");
            return 1;
        }

        private static string RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "CHANGELOG.md")))
                dir = dir.Parent;
            return dir?.FullName ?? Directory.GetCurrentDirectory();
        }

        private static string BuiltAgainst()
        {
            string proj = File.ReadAllText(Path.Combine(_repo, "src", "TradeLord.csproj"));
            Match m = Regex.Match(proj, @"Bannerlord\.ReferenceAssemblies""\s+Version=""([^""]+)""");
            return m.Success ? m.Groups[1].Value : "";
        }

        private static bool Fetch(string version)
        {
            if (Directory.Exists(Path.Combine(_nuget, "bannerlord.referenceassemblies.core", version)))
            {
                Console.WriteLine("have    " + version);
                return true;
            }
            string work = Path.Combine(Path.GetTempPath(), "tradelord-compat-" + version);
            Directory.CreateDirectory(work);
            File.WriteAllText(Path.Combine(work, "fetch.csproj"),
                "<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
                "  <PropertyGroup><TargetFramework>net472</TargetFramework></PropertyGroup>\n" +
                "  <ItemGroup>\n" +
                "    <PackageReference Include=\"Microsoft.NETFramework.ReferenceAssemblies\" Version=\"1.0.3\" PrivateAssets=\"all\"/>\n" +
                "    <PackageReference Include=\"Bannerlord.ReferenceAssemblies\" Version=\"" + version + "\"/>\n" +
                "  </ItemGroup>\n</Project>\n");
            Console.WriteLine("fetch   " + version);
            var p = Process.Start(new ProcessStartInfo("dotnet", "restore \"" + work + "/fetch.csproj\"")
            { RedirectStandardOutput = true, RedirectStandardError = true });
            p.WaitForExit();
            if (p.ExitCode == 0) return true;
            Console.Error.WriteLine("could not fetch reference assemblies for " + version);
            Console.Error.WriteLine(p.StandardError.ReadToEnd());
            return false;
        }

        private static MetadataLoadContext Open(string version)
        {
            var files = new List<string>();
            foreach (string dir in Directory.GetDirectories(_nuget))
            {
                if (!Path.GetFileName(dir).StartsWith("bannerlord.referenceassemblies")) continue;
                string reference = Path.Combine(dir, version, "ref", "net472");
                if (Directory.Exists(reference)) files.AddRange(Directory.GetFiles(reference, "*.dll"));
            }
            string framework = Path.Combine(_nuget, "microsoft.netframework.referenceassemblies.net472",
                                            "1.0.3", "build", ".NETFramework", "v4.7.2");
            files.AddRange(Directory.GetFiles(framework, "*.dll"));
            files.AddRange(Directory.GetFiles(Path.Combine(framework, "Facades"), "*.dll"));

            var unique = new Dictionary<string, string>();
            foreach (string f in files)
                if (!unique.ContainsKey(Path.GetFileName(f))) unique[Path.GetFileName(f)] = f;
            var context = new MetadataLoadContext(new PathAssemblyResolver(unique.Values), "mscorlib");
            foreach (string f in unique.Values) { try { context.LoadFromAssemblyPath(f); } catch { } }
            return context;
        }

        private static bool IsGame(string name) =>
            name.StartsWith("TaleWorlds") || name.StartsWith("SandBox") || name.StartsWith("StoryMode");

        private static Type Find(string version, string full)
        {
            foreach (Assembly asm in Loaded[version].GetAssemblies())
            {
                Type t = null;
                try { t = asm.GetType(full, false); } catch { }
                if (t != null) return t;
            }
            return null;
        }

        private static List<MethodInfo> Methods(string version, string type, string name)
        {
            var found = new List<MethodInfo>();
            for (Type t = Find(version, type); t != null; t = t.BaseType)
                found.AddRange(t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                            BindingFlags.Static | BindingFlags.DeclaredOnly)
                                .Where(m => m.Name == name));
            return found;
        }

        private static string Signature(MethodInfo m)
        {
            string parameters;
            try
            {
                parameters = string.Join(", ", m.GetParameters()
                    .Select(p => p.ParameterType.Name + " " + p.Name));
            }
            catch { parameters = "?"; }
            return (m.IsPublic ? "public" : m.IsPrivate ? "private" : "protected/internal")
                 + (m.IsStatic ? " static " : " ") + m.ReturnType.Name + "(" + parameters + ")";
        }

        private static void Line(bool ok, string label) =>
            Console.WriteLine((ok ? "  ok      " : "  BROKEN  ") + label);

        private static void CheckAssemblyIdentity(List<string> versions)
        {
            Console.WriteLine("== assembly identity - a build for one version only binds to another if these hold ==");
            foreach (string v in versions)
            {
                var ids = new Dictionary<string, string>();
                foreach (Assembly asm in Loaded[v].GetAssemblies())
                {
                    AssemblyName n = asm.GetName();
                    if (n.Name != null && IsGame(n.Name)) ids[n.Name] = n.Version?.ToString() ?? "";
                }
                if (v == versions[0]) { _baseline = ids; Line(true, v + " - " + ids.Count + " game assemblies"); continue; }
                var moved = ids.Where(kv => _baseline.TryGetValue(kv.Key, out string was) && was != kv.Value)
                               .Select(kv => kv.Key + " " + _baseline[kv.Key] + " -> " + kv.Value).ToList();
                var gone = _baseline.Keys.Where(k => !ids.ContainsKey(k)).ToList();
                Line(moved.Count == 0, v + " - " + ids.Count + " game assemblies, " + moved.Count + " identity change(s)");
                foreach (string m in moved) Failures.Add("assembly identity moved in " + v + ": " + m);
                foreach (string g in gone) Notes.Add(v + " no longer ships " + g);
            }
            Console.WriteLine();
        }

        private static Dictionary<string, string> _baseline = new();

        private static void CheckHarmonyTargets(List<string> versions)
        {
            Console.WriteLine("== Harmony patch targets - resolved by name at runtime, so a clean build proves nothing ==");
            foreach (var (type, member) in HarmonyTargets)
            {
                string label = type.Split('.').Last() + "." + member;
                bool ok = true;
                string first = null;
                foreach (string v in versions)
                {
                    var found = Methods(v, type, member);
                    if (found.Count == 0) { Failures.Add(label + " is gone in " + v); ok = false; continue; }
                    if (found.Count > 1)
                    {
                        Failures.Add(label + " has " + found.Count + " overloads in " + v
                                     + " - the patch lookup turns ambiguous and throws at load");
                        ok = false;
                        continue;
                    }
                    string sig = Signature(found[0]);
                    if (first == null) first = sig;
                    else if (sig != first)
                    {
                        Failures.Add(label + " changed shape in " + v + ": " + first + " -> " + sig);
                        ok = false;
                    }
                }
                Line(ok, label + (first == null ? "" : "  " + first));
            }
            Console.WriteLine();
        }

        private static void CheckReflectedMembers(List<string> versions)
        {
            Console.WriteLine("== members reached by reflection or injected by name ==");
            foreach (var (type, member) in ReflectedMethods)
            {
                string label = type.Split('.').Last() + "." + member;
                string first = null;
                bool ok = true;
                foreach (string v in versions)
                {
                    var found = Methods(v, type, member);
                    if (found.Count != 1) { Failures.Add(label + " is not a single method in " + v); ok = false; continue; }
                    string sig = Signature(found[0]);
                    if (first == null) first = sig;
                    else if (sig != first) { Failures.Add(label + " changed shape in " + v + ": " + first + " -> " + sig); ok = false; }
                }
                Line(ok, label + (first == null ? "" : "  " + first));
            }
            foreach (var (type, member) in ReflectedFields)
            {
                string label = type.Split('.').Last() + "." + member;
                string first = null;
                bool ok = true;
                foreach (string v in versions)
                {
                    string shape = null;
                    for (Type t = Find(v, type); t != null && shape == null; t = t.BaseType)
                        foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.NonPublic |
                                                            BindingFlags.Instance | BindingFlags.Static |
                                                            BindingFlags.DeclaredOnly))
                            if (f.Name == member)
                            { try { shape = f.FieldType.Name + " on " + t.Name; } catch { shape = "on " + t.Name; } break; }
                    if (shape == null) { Failures.Add(label + " is gone in " + v); ok = false; continue; }
                    if (first == null) first = shape;
                    else if (shape != first) { Failures.Add(label + " changed shape in " + v + ": " + first + " -> " + shape); ok = false; }
                }
                Line(ok, label + (first == null ? "" : "  " + first));
            }
            Console.WriteLine();
        }

        private static void CheckEnums(List<string> versions)
        {
            Console.WriteLine("== enum values - a renumbered enum keeps compiling and starts doing the wrong thing ==");
            foreach (string full in Enums)
            {
                string label = full.Split('+', '.').Last();
                var baseline = Values(versions[0], full);
                bool ok = baseline != null;
                if (!ok) Failures.Add(label + " is gone in " + versions[0]);
                foreach (string v in versions.Skip(1))
                {
                    var now = Values(v, full);
                    if (now == null) { Failures.Add(label + " is gone in " + v); ok = false; continue; }
                    if (baseline == null) continue;
                    foreach (var kv in baseline)
                    {
                        if (!now.TryGetValue(kv.Key, out long got))
                        { Failures.Add(label + "." + kv.Key + " is gone in " + v); ok = false; }
                        else if (got != kv.Value)
                        { Failures.Add(label + "." + kv.Key + " moved in " + v + ": " + kv.Value + " -> " + got); ok = false; }
                    }
                    var added = now.Keys.Where(k => !baseline.ContainsKey(k)).ToList();
                    if (added.Count > 0)
                        Notes.Add(v + " adds " + label + " " + string.Join(", ", added.Select(k => k + "=" + now[k]))
                                  + " - additive, nothing the mod reads moves");
                }
                Line(ok, label + (baseline == null ? "" : "  " + baseline.Count + " members held"));
            }
            Console.WriteLine();
        }

        private static Dictionary<string, long> Values(string version, string full)
        {
            Type t = Find(version, full);
            if (t == null || !t.IsEnum) return null;
            var map = new Dictionary<string, long>();
            foreach (FieldInfo f in t.GetFields(BindingFlags.Public | BindingFlags.Static))
                try { map[f.Name] = Convert.ToInt64(f.GetRawConstantValue()); } catch { }
            return map;
        }

        private static void CheckBoundSurface(List<string> versions)
        {
            Console.WriteLine("== every game type and member the compiled mod binds to ==");
            var types = new SortedSet<string>();
            var members = new SortedSet<string>();
            foreach (string rel in ModAssemblies) Read(Path.Combine(_repo, rel), types, members);
            Console.WriteLine("  the mod binds " + types.Count + " game types and " + members.Count + " distinct members");

            foreach (string v in versions)
            {
                var missingTypes = types.Where(t => Find(v, t) == null).ToList();
                var missingMembers = new List<string>();
                foreach (string entry in members)
                {
                    int split = entry.IndexOf("::", StringComparison.Ordinal);
                    string owner = entry.Substring(0, split), name = entry.Substring(split + 2);
                    Type t = Find(v, owner);
                    if (t == null) continue;
                    if (!Has(t, name)) missingMembers.Add(entry);
                }
                Line(missingTypes.Count + missingMembers.Count == 0,
                     v + " - " + missingTypes.Count + " unresolved type(s), " + missingMembers.Count + " unresolved member(s)");
                foreach (string t in missingTypes) Failures.Add(v + " no longer has type " + t);
                foreach (string m in missingMembers) Failures.Add(v + " no longer has member " + m);
            }
            Console.WriteLine();
        }

        private static bool Has(Type type, string name)
        {
            const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic |
                                     BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            for (Type t = type; t != null; t = t.BaseType)
                try
                {
                    if (name == ".ctor" ? t.GetConstructors(All).Length > 0
                                        : t.GetMethods(All).Any(x => x.Name == name)
                                       || t.GetFields(All).Any(x => x.Name == name)
                                       || t.GetProperties(All).Any(x => x.Name == name))
                        return true;
                }
                catch { }
            return false;
        }

        private static void Read(string dll, SortedSet<string> types, SortedSet<string> members)
        {
            using var stream = File.OpenRead(dll);
            using var pe = new PEReader(stream);
            MetadataReader md = pe.GetMetadataReader();

            string Name(TypeReferenceHandle h)
            {
                TypeReference tr = md.GetTypeReference(h);
                string n = md.GetString(tr.Name);
                if (tr.ResolutionScope.Kind == HandleKind.TypeReference)
                    return Name((TypeReferenceHandle)tr.ResolutionScope) + "+" + n;
                string ns = tr.Namespace.IsNil ? "" : md.GetString(tr.Namespace);
                return ns.Length > 0 ? ns + "." + n : n;
            }

            foreach (TypeReferenceHandle h in md.TypeReferences)
            {
                string n = Name(h);
                if (IsGame(n)) types.Add(n);
            }
            foreach (MemberReferenceHandle h in md.MemberReferences)
            {
                MemberReference mr = md.GetMemberReference(h);
                if (mr.Parent.Kind != HandleKind.TypeReference) continue;
                string owner = Name((TypeReferenceHandle)mr.Parent);
                if (IsGame(owner)) members.Add(owner + "::" + md.GetString(mr.Name));
            }
        }
    }
}
