using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TradeLord.Compat
{
    internal static class Program
    {
        private const string Inventory = "TaleWorlds.CampaignSystem.ViewModelCollection.Inventory.";

        private const string Issues = "TaleWorlds.CampaignSystem.Issues.";

        private static readonly (string type, string member, string[] takes)[] HarmonyTargets =
        {
            (Inventory + "ItemMenuVM", "RefreshItemTooltips", null),
            (Inventory + "ItemMenuVM", "SetMerchandiseComponentTooltip", null),
            (Inventory + "SPItemVM", "UpdateProfitType", null),
            ("TaleWorlds.Library.InformationManager", "DisplayMessage", null),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultWorkshopModel",
                "GetMaxWorkshopCountForClanTier", null),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultWorkshopModel",
                "get_MaximumWorkshopsPlayerCanHave", null),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.WorkshopsCharactersCampaignBehavior",
                "can_player_buy_workshop_clickable_condition", null),
            ("TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.Categories.ClanIncomeVM",
                "RefreshList", null),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultClanTierModel", "HasUpcomingTier", null),
            ("TaleWorlds.CampaignSystem.Settlements.TownMarketData", "GetPrice",
                new[] { "EquipmentElement", "MobileParty", "Boolean", "PartyBase" }),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.VillagerCampaignBehavior",
                "village_farmer_buy_products_on_condition", null),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.VillagerCampaignBehavior",
                "conversation_player_decided_to_buy_on_consequence", null),
            ("TaleWorlds.MountAndBlade.GauntletUI.Widgets.Chat.ChatLogItemWidget", "set_OneLineTextWidget", null),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.FoodConsumptionBehavior", "MakeFoodConsumption", null),
            ("TaleWorlds.ScreenSystem.ScreenManager", "EarlyUpdate", null),
            ("TaleWorlds.Engine.MouseManager", "ActivateMouseCursor", null),
        };

        private static readonly (string type, string member)[] ReflectedMethods =
        {
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "GetHerdingModifier"),
            ("TaleWorlds.CampaignSystem.Conversation.ConversationSentence", "set_InputToken"),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.TradeSkillCampaignBehavior", "ProcessPurchases"),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.TradeSkillCampaignBehavior", "ProcessSales"),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.WorkshopsCampaignBehavior", "GetDataOfWorkshop"),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.WorkshopsCampaignBehavior", "AddNewWorkshopData"),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.WorkshopsCampaignBehavior", "GetWarehouseRoster"),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.WorkshopsCampaignBehavior", "AddNewWarehouseDataIfNeeded"),
            ("SandBox.View.Map.MapScreen", "HandleMouse"),
            ("SandBox.View.Map.MapScreen", "CheckCursorState"),
            ("TaleWorlds.ScreenSystem.ScreenManager", "EarlyUpdate"),
            ("TaleWorlds.ScreenSystem.ScreenManager", "LateUpdate"),
            ("TaleWorlds.Engine.MouseManager", "ActivateMouseCursor"),
            ("TaleWorlds.CampaignSystem.Settlements.VillageMarketData", "get_ClosestTown"),
            ("TaleWorlds.CampaignSystem.ComponentInterfaces.PartySpeedModel", "get_BaseModel"),
            ("TaleWorlds.CampaignSystem.ComponentInterfaces.MapWeatherModel", "get_BaseModel"),
        };

        private static readonly (string type, string member, string why)[] LotShape =
        {
            ("TaleWorlds.Core.EquipmentElement", "Item", "the good a lot holds"),
            ("TaleWorlds.Core.EquipmentElement", "ItemModifier",
             "the quality that keeps one good in lots of its own"),
            ("TaleWorlds.Core.EquipmentElement", "IsQuestItem",
             "the quest hold that keeps one good in lots of its own"),
            ("TaleWorlds.CampaignSystem.Roster.ItemRoster", "FindIndexOfItem",
             "looking a good up by itself"),
            ("TaleWorlds.CampaignSystem.Roster.ItemRoster", "FindIndexOfElement",
             "looking it up by the lot it sits in"),
            ("TaleWorlds.CampaignSystem.Roster.ItemRoster", "GetItemAtIndex",
             "the good a lot holds, read lot by lot"),
            ("TaleWorlds.CampaignSystem.Roster.ItemRoster", "GetElementNumber",
             "how many sit in that lot, added up over every lot of a good"),
            ("TaleWorlds.CampaignSystem.Roster.ItemRoster", "GetElementCopyAtIndex",
             "how many of it sit in the one lot"),
            ("TaleWorlds.CampaignSystem.Roster.ItemRoster", "VersionNo",
             "the mark the game moves whenever what your party carries changes"),
        };

        private static readonly (string type, string contract)[] ReflectedTypes =
        {
            ("TaleWorlds.CampaignSystem.Settlements.FakeMarketData",
             "TaleWorlds.CampaignSystem.Settlements.IMarketData"),
            ("NavalDLC.GameComponents.NavalDLCPartySpeedCalculationModel",
             "TaleWorlds.CampaignSystem.ComponentInterfaces.PartySpeedModel"),
        };

        private static readonly (string type, string member)[] McmMembers =
        {
            ("MCM.Abstractions.SettingsPropertyDefinition", "<DisplayName>k__BackingField"),
            ("MCM.Abstractions.SettingsPropertyDefinition", "<HintText>k__BackingField"),
            ("MCM.Abstractions.SettingsPropertyDefinition", "<GroupName>k__BackingField"),
            ("MCM.Abstractions.SettingsPropertyDefinition", "<Content>k__BackingField"),
            ("MCM.Abstractions.SettingsPropertyGroupDefinition", "_groupNameRaw"),
            ("MCM.Abstractions.Base.BaseSettings", "PropertyChanged"),
            ("MCM.Abstractions.Base.BaseSettings", "SaveTriggered"),
        };

        private static readonly (string type, string[] parameters)[] McmConstructors =
        {
            ("MCM.Abstractions.SettingsPropertyDefinition",
             new[] { "IEnumerable`1", "IPropertyGroupDefinition", "IRef", "Char" }),
            ("MCM.Abstractions.SettingsPropertyGroupDefinition", new[] { "String", "Int32" }),
        };

        private static readonly (string type, string member)[] ReflectedFields =
        {
            (Inventory + "ItemMenuVM", "_targetItem"),
            (Issues + "HeadmanNeedsToDeliverAHerdIssueBehavior+HeadmanNeedsToDeliverAHerdIssueQuest", "_herdTypeToDeliver"),
            (Issues + "HeadmanNeedsToDeliverAHerdIssueBehavior+HeadmanNeedsToDeliverAHerdIssueQuest", "_animalCountToDeliver"),
            (Issues + "HeadmanVillageNeedsDraughtAnimalsIssueBehavior+HeadmanVillageNeedsDraughtAnimalsIssueQuest", "_requestedAnimal"),
            (Issues + "HeadmanVillageNeedsDraughtAnimalsIssueBehavior+HeadmanVillageNeedsDraughtAnimalsIssueQuest", "_requestedAnimalAmount"),
            (Issues + "LordNeedsHorsesIssueBehavior+LordNeedsHorsesIssueQuest", "_mountObjectToBeDelivered"),
            (Issues + "LordNeedsHorsesIssueBehavior+LordNeedsHorsesIssueQuest", "_numMountsToBeDelivered"),
            (Issues + "ArtisanOverpricedGoodsIssueBehavior+ArtisanOverpricedGoodsIssueQuest", "_requestedTradeGood"),
            (Issues + "ArtisanOverpricedGoodsIssueBehavior+ArtisanOverpricedGoodsIssueQuest", "_requestedTradeGoodAmount"),
            (Issues + "ArtisanCantSellProductsAtAFairPriceIssueBehavior+ArtisanCantSellProductsAtAFairPriceIssueQuest", "_rawMaterialsToBeDelivered"),
            (Issues + "ArtisanCantSellProductsAtAFairPriceIssueBehavior+ArtisanCantSellProductsAtAFairPriceIssueQuest", "_amountOfRawGoodsToBeDelivered"),
            (Issues + "LandLordTheArtOfTheTradeIssueBehavior+LandLordTheArtOfTheTradeIssueQuest", "_selectedItemObject"),
            (Issues + "LandLordTheArtOfTheTradeIssueBehavior+LandLordTheArtOfTheTradeIssueQuest", "_selectedItemObjectCount"),
            (Issues + "ArmyNeedsSuppliesIssueBehavior+ArmyNeedsSuppliesIssueQuest", "_requestedGrainAmount"),
            (Issues + "ArmyNeedsSuppliesIssueBehavior+ArmyNeedsSuppliesIssueQuest", "_requestedWineAmount"),
            (Issues + "HeadmanNeedsGrainIssueBehavior+HeadmanNeedsGrainIssueQuest", "_neededGrainAmount"),
            (Issues + "ArmyNeedsSuppliesIssueBehavior+ArmyNeedsSuppliesIssueQuest", "_requestedLiveStockAmount"),
            (Issues + "VillageNeedsToolsIssueBehavior+VillageNeedsToolsIssueQuest", "_requestedTradeGood"),
            (Issues + "VillageNeedsToolsIssueBehavior+VillageNeedsToolsIssueQuest", "_numberOfRequestedGood"),
            (Issues + "VillageNeedsCraftingMaterialsIssueBehavior+VillageNeedsCraftingMaterialsIssueQuest", "_requestedItem"),
            (Issues + "VillageNeedsCraftingMaterialsIssueBehavior+VillageNeedsCraftingMaterialsIssueQuest", "_requestedItemAmount"),
            (Issues + "GangLeaderNeedsWeaponsIssueQuestBehavior+GangLeaderNeedsWeaponsIssueQuest", "_requestedWeaponClass"),
            (Issues + "GangLeaderNeedsWeaponsIssueQuestBehavior+GangLeaderNeedsWeaponsIssueQuest", "_requestedWeaponAmount"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultInventoryCapacityModel", "_textPackAnimals"),
            ("TaleWorlds.CampaignSystem.Conversation.ConversationManager", "_sentences"),
            ("TaleWorlds.CampaignSystem.Inventory.InventoryLogic", "_transactionHistory"),
            ("TaleWorlds.CampaignSystem.Inventory.InventoryLogic+TransactionHistory", "_transactionLogs"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "CavalryEffect"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "DisorganizedEffect"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "MountedFootMenEffect"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "MovingAtForestEffect"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "MovingAtNightEffect"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "MovingAtWaterEffect"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "MovingInDesertEffect"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "MovingOnSnowEffect"),
            ("TaleWorlds.CampaignSystem.MBCampaignEvent", "NextTriggerTime"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "_desert"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "_fordEffect"),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.MapWeatherCampaignBehavior", "_lastUpdatedNodeIndex"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "_movingInForest"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "_night"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "_snow"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "_textCavalry"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "_textCavalryWeatherPenalty"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "_textDisorganized"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel", "_textMountedFootmen"),
            ("TaleWorlds.CampaignSystem.GameComponents.DefaultPartySpeedCalculatingModel",
             "_textMountedFootmenWeatherPenalty"),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.MapWeatherCampaignBehavior",
             "_weatherNodeDataShuffledIndices"),
            ("TaleWorlds.CampaignSystem.CampaignBehaviors.MapWeatherCampaignBehavior", "_weatherTickEvent"),
        };

        private static readonly string[] Enums =
        {
            "TaleWorlds.CampaignSystem.ComponentInterfaces.SettlementAccessModel+SettlementAction",
            "TaleWorlds.CampaignSystem.GameMenus.GameMenuOption+LeaveType",
            "TaleWorlds.CampaignSystem.Inventory.InventoryLogic+InventorySide",
            "Helpers.InventoryScreenHelper+InventoryMode",
            "TaleWorlds.CampaignSystem.Settlements.Village+VillageStates",
            "TaleWorlds.CampaignSystem.Party.MobileParty+NavigationType",
            "TaleWorlds.CampaignSystem.Party.AiBehavior",
            "TaleWorlds.CampaignSystem.ComponentInterfaces.MapWeatherModel+WeatherEvent",
            "TaleWorlds.CampaignSystem.ComponentInterfaces.MapWeatherModel+WeatherEventEffectOnTerrain",
            "TaleWorlds.Core.ItemObject+ItemTiers",
            "TaleWorlds.Core.ViewModelCollection.Information.TooltipProperty+TooltipPropertyFlags",
            "TaleWorlds.InputSystem.InputKey",
        };

        private static readonly (string id, bool guarded)[] MenuIds =
        {
            ("town", false),
            ("village", false),
            ("port_menu", true),
            ("naval_storyline_virtualport", true),
        };

        private const string GameBinVariable = "TRADELORD_GAME_BIN";

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
            if (args.Length == 0)
            {
                Console.WriteLine("usage: dotnet run --project tools/compat -- <game version> [more versions]");
                Console.WriteLine("       the version in src/TradeLord.csproj is always the baseline");
                Console.WriteLine();
                Console.WriteLine("example: dotnet run --project tools/compat -- <a version to compare against>");
                Console.WriteLine();
                Console.WriteLine("set " + GameBinVariable + " to a Bannerlord install to also check the menu ids the");
                Console.WriteLine("mod hangs its entries off. Reference assemblies carry no string literals, so that");
                Console.WriteLine("check needs the shipped assemblies. Nothing is copied and nothing is written.");
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
            Console.WriteLine("baseline " + versions[0] + (versions.Count == 1
                ? ", the one version checked"
                : ", compared against " + string.Join(", ", versions.Skip(1))));
            Console.WriteLine();

            CheckAssemblyIdentity(versions);
            CheckHarmonyTargets(versions);
            CheckReflectedMembers(versions);
            CheckReflectedTypes(versions);
            CheckLotShape(versions);
            CheckMcmSurface();
            CheckEnums(versions);
            CheckBoundSurface(versions);
            CheckMenuIds();

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
            Task<string> errors = p.StandardError.ReadToEndAsync();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode == 0) return true;
            Console.Error.WriteLine("could not fetch reference assemblies for " + version);
            Console.Error.WriteLine(output);
            Console.Error.WriteLine(errors.Result);
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
            name.StartsWith("TaleWorlds") || name.StartsWith("SandBox") || name.StartsWith("StoryMode") ||
            name.StartsWith("Helpers.");

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

        private static List<MethodInfo> WhereHarmonyLooks(string version, string type, string name)
        {
            for (Type t = Find(version, type); t != null; t = t.BaseType)
            {
                var here = t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                        BindingFlags.Static | BindingFlags.DeclaredOnly)
                            .Where(m => m.Name == name).ToList();
                if (here.Count > 0) return here;
            }
            return new List<MethodInfo>();
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

        private static bool Takes(MethodInfo m, string[] takes)
        {
            ParameterInfo[] had = m.GetParameters();
            if (had.Length != takes.Length) return false;
            for (int i = 0; i < takes.Length; i++)
                if (had[i].ParameterType.Name != takes[i]) return false;
            return true;
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

        private static void CheckMenuIds()
        {
            Console.WriteLine("== game menu ids - string literals, so only a real install can answer for them ==");
            string root = Environment.GetEnvironmentVariable(GameBinVariable);
            if (string.IsNullOrWhiteSpace(root))
            {
                Console.WriteLine("  skipped  set " + GameBinVariable + " to a Bannerlord install to run this check");
                Console.WriteLine();
                return;
            }
            if (!Directory.Exists(root))
            {
                Failures.Add(GameBinVariable + " points at " + root + ", which is not a directory");
                Console.WriteLine("  BROKEN  " + GameBinVariable + " points at a directory that is not there");
                Console.WriteLine();
                return;
            }

            var holders = new Dictionary<string, string>();
            int scanned = 0;
            foreach (string dll in Directory.EnumerateFiles(root, "*.dll", SearchOption.AllDirectories))
            {
                HashSet<string> strings;
                try { strings = UserStrings(dll); }
                catch { continue; }
                scanned++;
                foreach (var (id, _) in MenuIds)
                    if (!holders.ContainsKey(id) && strings.Contains(id))
                        holders[id] = Path.GetFileName(dll);
            }
            Console.WriteLine("  read    " + scanned + " assemblies under " + root);

            foreach (var (id, guarded) in MenuIds)
            {
                if (holders.TryGetValue(id, out string where)) { Line(true, id + "  in " + where); continue; }
                if (guarded)
                {
                    Line(true, id + "  not in this install - guarded, the menu is skipped and logged");
                    Notes.Add(id + " is not in this install, which is expected without the module that registers it");
                }
                else
                {
                    Failures.Add(id + " is in no assembly of this install - the menu entries would not appear");
                    Line(false, id + "  in no assembly of this install");
                }
            }
            Console.WriteLine();
        }

        private static HashSet<string> UserStrings(string dll)
        {
            var found = new HashSet<string>(StringComparer.Ordinal);
            using var stream = File.OpenRead(dll);
            using var pe = new PEReader(stream);
            if (!pe.HasMetadata) return found;
            MetadataReader md = pe.GetMetadataReader();
            var handle = MetadataTokens.UserStringHandle(0);
            while (true)
            {
                handle = md.GetNextHandle(handle);
                if (handle.IsNil) break;
                found.Add(md.GetUserString(handle));
            }
            return found;
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
            foreach (var (type, member, takes) in HarmonyTargets)
            {
                string label = type.Split('.').Last() + "." + member
                             + (takes == null ? "" : "(" + string.Join(", ", takes) + ")");
                bool ok = true;
                string first = null;
                foreach (string v in versions)
                {
                    var found = WhereHarmonyLooks(v, type, member);
                    if (takes != null) found = found.FindAll(m => Takes(m, takes));
                    if (found.Count == 0) { Failures.Add(label + " is gone in " + v); ok = false; continue; }
                    if (found.Count > 1)
                    {
                        Failures.Add(label + " has " + found.Count + " overloads on " + found[0].DeclaringType.Name
                                     + " in " + v + " - the patch lookup turns ambiguous and throws at load");
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

        private static void CheckReflectedTypes(List<string> versions)
        {
            Console.WriteLine("== types fetched by name at runtime - the compiled assembly names none of them ==");
            foreach (var (type, contract) in ReflectedTypes)
            {
                string label = type.Split('.').Last();
                bool ok = true;
                foreach (string v in versions)
                {
                    Type found = Find(v, type);
                    if (found == null)
                    {
                        Failures.Add(label + " is gone in " + v);
                        ok = false;
                        continue;
                    }
                    if (Find(v, contract) is Type wanted && !wanted.IsAssignableFrom(found))
                    {
                        Failures.Add(label + " no longer implements " + contract.Split('.').Last() + " in " + v);
                        ok = false;
                    }
                    if (!found.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                              .Any(c => c.GetParameters().Length == 0))
                    {
                        Failures.Add(label + " has no parameterless constructor to create in " + v);
                        ok = false;
                    }
                }
                Line(ok, label + "  " + contract.Split('.').Last() + ", created with no arguments");
            }
            Console.WriteLine();
        }

        private static string LotMember(string version, string type, string member)
        {
            var found = Methods(version, type, member);
            if (found.Count > 1) return "overloaded";
            if (found.Count == 1) return Signature(found[0]);
            for (Type t = Find(version, type); t != null; t = t.BaseType)
            {
                PropertyInfo held = t.GetProperty(member, BindingFlags.Public | BindingFlags.NonPublic |
                                                          BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (held == null) continue;
                try { return held.PropertyType.Name; } catch { return "property"; }
            }
            return null;
        }

        private static void CheckLotShape(List<string> versions)
        {
            Console.WriteLine("== one good sits in more than one lot of the bags - the food reserve "
                              + "and the per-lot caps are counted on that ==");
            foreach (var (type, member, why) in LotShape)
            {
                string label = type.Split('.').Last() + "." + member;
                string first = null;
                bool ok = true;
                foreach (string v in versions)
                {
                    string shape = LotMember(v, type, member);
                    if (shape == null)
                    {
                        Failures.Add(label + " is gone in " + v + ", so " + why + " can no longer be read");
                        ok = false;
                        continue;
                    }
                    if (first == null) first = shape;
                    else if (shape != first)
                    {
                        Failures.Add(label + " changed shape in " + v + ": " + first + " -> " + shape);
                        ok = false;
                    }
                }
                Line(ok, label + "  " + why);
            }
            Console.WriteLine();
        }

        private static string McmVersion()
        {
            string proj = File.ReadAllText(Path.Combine(_repo, "mcm", "TradeLord.MCM.csproj"));
            Match m = Regex.Match(proj, @"Bannerlord\.MCM""\s+Version=""([^""]+)""");
            return m.Success ? m.Groups[1].Value : "";
        }

        private static MetadataLoadContext OpenMcm(string version)
        {
            string package = Path.Combine(_nuget, "bannerlord.mcm", version, "lib", "netstandard2.0");
            if (!Directory.Exists(package)) return null;
            var files = new List<string>(Directory.GetFiles(package, "*.dll"));
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

        private static void CheckMcmSurface()
        {
            string version = McmVersion();
            Console.WriteLine("== MCM internals the settings screen reaches by name - MCM " + version + " ==");
            MetadataLoadContext mcm = version.Length == 0 ? null : OpenMcm(version);
            if (mcm == null)
            {
                Line(false, "MCM " + version + " is not restored, so nothing here was read");
                Failures.Add("MCM " + version + " could not be opened - build mcm/TradeLord.MCM.csproj first");
                Console.WriteLine();
                return;
            }

            Type Named(string full)
            {
                foreach (Assembly asm in mcm.GetAssemblies())
                {
                    Type t = null;
                    try { t = asm.GetType(full, false); } catch { }
                    if (t != null) return t;
                }
                return null;
            }

            const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic |
                                     BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var (type, member) in McmMembers)
            {
                string label = type.Split('.').Last() + "." + member;
                Type owner = Named(type);
                FieldInfo field = null;
                for (Type t = owner; t != null && field == null; t = t.BaseType)
                    field = t.GetFields(All).FirstOrDefault(f => f.Name == member);
                Line(field != null, label + (field == null ? "" : "  " + field.FieldType.Name));
                if (field == null) Failures.Add(label + " is gone in MCM " + version);
            }
            foreach (var (type, parameters) in McmConstructors)
            {
                string label = type.Split('.').Last() + "(" + string.Join(", ", parameters) + ")";
                Type owner = Named(type);
                bool ok = owner != null && owner.GetConstructors(All).Any(c =>
                    c.GetParameters().Select(p => p.ParameterType.Name).SequenceEqual(parameters));
                Line(ok, label);
                if (!ok) Failures.Add(label + " is gone in MCM " + version);
            }
            Console.WriteLine();
        }

        private static void CheckEnums(List<string> versions)
        {
            Console.WriteLine("== enum values - a renumbered enum keeps compiling and starts doing the wrong thing ==");
            var named = new SortedSet<string>(Enums, StringComparer.Ordinal);
            var types = new SortedSet<string>();
            var members = new SortedDictionary<string, Bound>(StringComparer.Ordinal);
            foreach (string rel in ModAssemblies) Read(Path.Combine(_repo, rel), types, members);
            foreach (string full in types)
                try
                {
                    Type found = Find(versions[0], full);
                    if (found != null && found.IsEnum) named.Add(full);
                }
                catch { }
            foreach (string full in named)
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
            var members = new SortedDictionary<string, Bound>(StringComparer.Ordinal);
            foreach (string rel in ModAssemblies) Read(Path.Combine(_repo, rel), types, members);
            Console.WriteLine("  the mod binds " + types.Count + " game types and " + members.Count +
                              " distinct members, each held to the parameters and the type it was built against");

            foreach (string v in versions)
            {
                var missingTypes = types.Where(t => Find(v, t) == null).ToList();
                var missingMembers = new List<string>();
                foreach (Bound bound in members.Values)
                {
                    Type t = Find(v, bound.Owner);
                    if (t == null) continue;
                    if (!Has(t, bound))
                        missingMembers.Add(bound.Said + (Has(t, bound.Name)
                            ? ", whose name is still there with other parameters or another type"
                            : ""));
                }
                Line(missingTypes.Count + missingMembers.Count == 0,
                     v + " - " + missingTypes.Count + " unresolved type(s), " + missingMembers.Count + " unresolved member(s)");
                foreach (string t in missingTypes) Failures.Add(v + " no longer has type " + t);
                foreach (string m in missingMembers) Failures.Add(v + " no longer has member " + m);
            }
            Console.WriteLine();
        }

        private sealed class Bound
        {
            internal string Owner, Name, Returns, Said;
            internal string[] Takes;
            internal int Generic;
            internal bool Field;
        }

        private const BindingFlags Declared = BindingFlags.Public | BindingFlags.NonPublic |
                                              BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        private static bool Has(Type type, string name)
        {
            for (Type t = type; t != null; t = t.BaseType)
                try
                {
                    if (name == ".ctor" ? t.GetConstructors(Declared).Length > 0
                                        : t.GetMethods(Declared).Any(x => x.Name == name)
                                       || t.GetFields(Declared).Any(x => x.Name == name)
                                       || t.GetProperties(Declared).Any(x => x.Name == name))
                        return true;
                }
                catch { }
            return false;
        }

        private static bool Has(Type type, Bound bound)
        {
            for (Type t = type; t != null; t = t.BaseType)
            {
                MemberInfo[] named;
                try { named = t.GetMember(bound.Name, Declared); }
                catch { continue; }
                foreach (MemberInfo one in named)
                    if (Matches(one, bound)) return true;
                if (bound.Name == ".ctor") return false;
            }
            return false;
        }

        private static bool Matches(MemberInfo one, Bound bound)
        {
            try
            {
                switch (one)
                {
                    case FieldInfo f: return bound.Field && Shape(f.FieldType) == bound.Returns;
                    case ConstructorInfo c: return !bound.Field && Takes(c.GetParameters(), bound.Takes);
                    case MethodInfo m:
                        return !bound.Field && (m.IsGenericMethodDefinition ? m.GetGenericArguments().Length : 0) == bound.Generic &&
                               Shape(m.ReturnType) == bound.Returns && Takes(m.GetParameters(), bound.Takes);
                    default: return false;
                }
            }
            catch { return false; }
        }

        private static bool Takes(ParameterInfo[] given, string[] wanted) =>
            given.Length == wanted.Length && given.Select(p => Shape(p.ParameterType)).SequenceEqual(wanted);

        private static string Shape(Type t)
        {
            if (t.IsByRef) return Shape(t.GetElementType()) + "&";
            if (t.IsPointer) return Shape(t.GetElementType()) + "*";
            if (t.IsArray) return Shape(t.GetElementType()) + "[" + new string(',', t.GetArrayRank() - 1) + "]";
            if (t.IsGenericParameter) return (t.DeclaringMethod != null ? "!!" : "!") + t.GenericParameterPosition;
            if (t.IsConstructedGenericType)
                return Shape(t.GetGenericTypeDefinition()) + "<" + string.Join(",", t.GenericTypeArguments.Select(Shape)) + ">";
            return t.FullName ?? t.Name;
        }

        private sealed class Spelled : ISignatureTypeProvider<string, object>
        {
            public string GetPrimitiveType(PrimitiveTypeCode code) => "System." + code;
            public string GetTypeFromDefinition(MetadataReader md, TypeDefinitionHandle h, byte kind)
            {
                TypeDefinition td = md.GetTypeDefinition(h);
                string n = md.GetString(td.Name);
                if (td.IsNested) return GetTypeFromDefinition(md, td.GetDeclaringType(), kind) + "+" + n;
                string ns = md.GetString(td.Namespace);
                return ns.Length > 0 ? ns + "." + n : n;
            }
            public string GetTypeFromReference(MetadataReader md, TypeReferenceHandle h, byte kind)
            {
                TypeReference tr = md.GetTypeReference(h);
                string n = md.GetString(tr.Name);
                if (tr.ResolutionScope.Kind == HandleKind.TypeReference)
                    return GetTypeFromReference(md, (TypeReferenceHandle)tr.ResolutionScope, kind) + "+" + n;
                string ns = tr.Namespace.IsNil ? "" : md.GetString(tr.Namespace);
                return ns.Length > 0 ? ns + "." + n : n;
            }
            public string GetTypeFromSpecification(MetadataReader md, object context, TypeSpecificationHandle h, byte kind) =>
                md.GetTypeSpecification(h).DecodeSignature(this, context);
            public string GetSZArrayType(string element) => element + "[]";
            public string GetArrayType(string element, ArrayShape shape) => element + "[" + new string(',', shape.Rank - 1) + "]";
            public string GetByReferenceType(string element) => element + "&";
            public string GetPointerType(string element) => element + "*";
            public string GetPinnedType(string element) => element;
            public string GetModifiedType(string modifier, string unmodified, bool required) => unmodified;
            public string GetGenericInstantiation(string generic, System.Collections.Immutable.ImmutableArray<string> args) =>
                generic + "<" + string.Join(",", args) + ">";
            public string GetGenericMethodParameter(object context, int index) => "!!" + index;
            public string GetGenericTypeParameter(object context, int index) => "!" + index;
            public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";
        }

        private static void Read(string dll, SortedSet<string> types, SortedDictionary<string, Bound> members)
        {
            using var stream = File.OpenRead(dll);
            using var pe = new PEReader(stream);
            MetadataReader md = pe.GetMetadataReader();
            var spelled = new Spelled();

            string Name(TypeReferenceHandle h) => spelled.GetTypeFromReference(md, h, 0);

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
                if (!IsGame(owner)) continue;
                var bound = new Bound { Owner = owner, Name = md.GetString(mr.Name) };
                if (mr.GetKind() == MemberReferenceKind.Field)
                {
                    bound.Field = true;
                    bound.Returns = mr.DecodeFieldSignature(spelled, null);
                    bound.Takes = new string[0];
                    bound.Said = owner + "::" + bound.Name + ":" + bound.Returns;
                }
                else
                {
                    MethodSignature<string> sig = mr.DecodeMethodSignature(spelled, null);
                    bound.Returns = sig.ReturnType;
                    bound.Takes = sig.ParameterTypes.ToArray();
                    bound.Generic = sig.GenericParameterCount;
                    bound.Said = owner + "::" + bound.Name + "(" + string.Join(",", bound.Takes) + "):" + bound.Returns;
                }
                members[bound.Said] = bound;
            }
        }
    }
}
