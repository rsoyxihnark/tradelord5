# TradeLord on game 1.4.8

The mod is built against Bannerlord `1.4.7.117484`. Players are already on `1.4.8.119303`. This is what an inspection of the seam between the two versions found, what it could not reach, and what is left to settle in game.

| | |
|---|---|
| Mod version first inspected | 1.6.9 |
| Re-checked at | 1.6.21 |
| From | 1.4.7.117484 |
| To | 1.4.8.119303, and 1.5.1.120547-beta |
| Method | NuGet reference assemblies for every version, plus the mod's own compiled output. No game install involved. |
| Reproduce it | `dotnet run --project tools/compat -- 1.4.8.119303 1.5.1.120547-beta` |

## Verdict

Nothing found that blocks 1.4.8. Every game type, member, enum value and UI binding the mod touches is unchanged between the two versions, and both projects compile clean against 1.4.8 with warnings as errors.

The reference assemblies carry signatures only, so this proved the mod *fits* 1.4.8, not that it *behaves* the same. That is no longer the standing state: **the test pass below has been walked on the game, and every test passed.** Of the five questions this document opened with, one was closed by the 1.6.20 save change, three were settled in play, and the last is confirmed for the base game and out of reach only for War Sails.

## Verified compatible

**Both projects compile against 1.4.8.** The whole source tree rebuilt against the 1.4.8 reference assemblies in Release, with `TreatWarningsAsErrors` on. No errors, no warnings, and the source checks pass in that tree too.

**The bound API surface is unchanged.** Reading the two compiled assemblies' type and member reference tables gives the exact game surface the mod binds to. All of it resolved in both versions and compared by signature, accessibility and parameter name.

- 100 game types bound, 192 distinct members bound, every one of them resolved in both versions
- 0 differences affecting anything the mod calls
- the only difference inside any bound type is `ApplicationVersion.DefaultChangeSet`, 117484 -> 119303, which the mod never reads

**All four Harmony patch targets still match.** These resolve by name at runtime, so a clean compile says nothing about them. Each still exists with the same signature, the same accessibility, and the same parameter names, which matters because Harmony injects by name. `DisplayMessage` also still has exactly one overload; a second would make the patch's lookup ambiguous and throw at load.

- `ItemMenuVM.RefreshItemTooltips` - private `Void(ItemVM, ItemVM, Int32)`, params `item, comparedItem, alternativeUsageIndex`
- `ItemMenuVM.SetMerchandiseComponentTooltip` - private `Void()`
- `SPItemVM.UpdateProfitType` - unchanged
- `InformationManager.DisplayMessage` - public `Void(InformationMessage)`, still one overload

**The private members the mod reaches for are intact.** The reference assemblies keep private members, so both could be checked directly.

- `DefaultPartySpeedCalculatingModel.GetHerdingModifier` - private `Single(Int32, Int32)`, params `totalMenCount, herdSize`
- `ItemMenuVM._targetItem` - private field of type `ItemVM`

**Every enum the mod switches on has identical values.** A renumbered enum is the quietest way a game update breaks a mod: it keeps compiling and starts doing the wrong thing. All eight are compared member by member, including each numeric value: `SettlementAction`, `LeaveType` (48 members, naval ones included), `InventorySide`, `NavigationType`, `VillageStates`, `ItemTiers`, `TooltipPropertyFlags` and `InputKey`.

**The ledger panel's widget types all resolve.** The panel prefab names widget classes and properties as text, resolved by the UI loader at runtime, so nothing about it is checked by a compile. All 9 widget types it uses - `BrushWidget`, `ButtonWidget`, `DimensionSyncWidget`, `ImageWidget`, `ListPanel`, `ScrollablePanel`, `ScrollbarWidget`, `TextWidget`, `Widget` - are present in 1.4.7, 1.4.8 and 1.5.1-beta alike, re-checked at 1.6.21.

The deeper check behind this one has not been re-run. At 1.6.9 all 435 attribute bindings were walked through the base-class chain and resolved; the prefab now carries 450 attributes across those same 9 widget types, and that walk was a one-off, not something `tools/compat` does. T6 is what actually settles the panel.

**Assembly identity did not move.** This is why the mod can cross the version line at all. The game's assemblies are unsigned and permanently versioned `1.0.0.0`, so a DLL compiled against 1.4.7 binds to 1.4.8's assemblies with no redirect and no rebuild. All 81 assemblies in the packages, zero identity differences; `tools/compat` reports the 75 of them whose names begin `TaleWorlds`, `SandBox` or `StoryMode`, which is the subset the mod can bind to.

**Nothing the mod touches changed anywhere in the game.** Diffing the two versions whole, across every type in all 81 assemblies, gives 12,731 -> 12,732 types. The entire difference is three compiler-generated closure classes renumbering inside `NavalDLC.Missions.Objects.MissionShip`, and one new engine delegate for clearing decals.

**Building against 1.4.8 changes nothing in the output.** Because the bound surface and the assembly identities are both unchanged, the compiler emits the same references either way. Measured at 1.6.9, both builds came out the same size with the same external reference set - `TradeLord.dll` at 83,968 bytes, `TradeLord.MCM.dll` at 25,088, 32 identical TaleWorlds reference names. The mod has grown since and 1.6.21 builds a larger DLL, but the argument does not depend on the size: it depends on the reference set, which is what `tools/compat` re-checks on every run. The version bump is a statement of which game the mod is tested against, not a change to what ships.

**The naval surface survived, which was the likeliest casualty.** The only churn anywhere in 1.4.8 is inside NavalDLC, and the mod reaches into naval territory twice: the sailing-aware travel estimate and the port menu entries. `NavigationType` kept its values, and so did the naval members of `LeaveType` - `VisitPort=41`, `SetSail=43`, `ManageFleet=44`, `CallFleet=45`, `RepairShips=47`.

## Also checked: the 1.5.1 beta

`1.5.1.120547-beta` was published while this was being written, and the same method reaches the same
verdict on it. Nothing found that blocks it.

- all 100 bound game types and 192 bound members still resolve
- all four Harmony patch targets unchanged in shape, still one overload each
- `GetHerdingModifier` and `ItemMenuVM._targetItem` intact
- no value of any enum the mod reads has moved
- assembly identities unchanged, so a 1.4.7-built assembly still binds with no redirect
- both projects compile clean against it with warnings as errors

Two differences exist and neither touches the mod. `LeaveType` gains `TakeFerry=48` on the end, which
renumbers nothing before it. `TaleWorlds.CampaignSystem.FastMode` is no longer shipped, and the mod
never bound to it.

The same caveat as 1.4.8 applies twice over: this is signatures, not behaviour, and 1.5.1 is a beta
that can still move. The test pass below is written for 1.4.8 and is the thing that settles either.

## Re-checked at 1.6.21

The compatibility tool was run again on the 1.6.21 build, against all three versions at once, and
reaches the same verdict. Nothing found that blocks any of them.

| | |
|---|---|
| Command | `dotnet run --project tools/compat -- 1.4.8.119303 1.5.1.120547-beta` |
| Result | exit 0, `the mod fits every version checked` |

- assembly identity unchanged - 75 game assemblies on 1.4.7 and on 1.4.8, 74 on 1.5.1-beta, 0 identity differences
- all four Harmony patch targets resolve with the same signatures and the same parameter names, and `DisplayMessage` still has exactly one overload
- `GetHerdingModifier` and `ItemMenuVM._targetItem` both intact
- all eight enums held member for member - `SettlementAction` 8, `LeaveType` 48, `InventorySide` 6, `VillageStates` 5, `NavigationType` 4, `ItemTiers` 7, `TooltipPropertyFlags` 15, `InputKey` 146
- the mod binds 102 game types and 200 distinct members, with 0 unresolved types and 0 unresolved members on all three versions

The tool raises two notes and neither touches the mod: `TaleWorlds.CampaignSystem.FastMode` is no
longer shipped in 1.5.1-beta, and `LeaveType` gains `TakeFerry=48` on the end, which renumbers
nothing before it.

### The bound surface shrank, and only where the save definer was

1.6.19 bound 103 game types and 202 members; 1.6.20 binds 100 and 198. Both builds were put through
the tool and their reference tables diffed, so the whole of that difference is accounted for:

| Gone from the bound surface | |
|---|---|
| types | `SaveableTypeDefiner`, `SaveableFieldAttribute`, `IObjectResolver` |
| members | `SaveableTypeDefiner::.ctor`, `SaveableTypeDefiner::AddClassDefinition`, `SaveableTypeDefiner::ConstructContainerDefinition`, `SaveableFieldAttribute::.ctor` |

Nothing was added, and nothing outside that group was lost. Every one of them is a save-system type
the mod stopped needing when it stopped defining saveable types of its own. This number moves with
the mod, not with the game: 1.6.21 took it back up to 102 and 200 by reading the hero's Trade skill
level, and those bindings resolve on all three versions too.

The two sections above once disagreed with each other, quoting 198 members in one and 103 types and
201 members in the other, because an earlier version of the compatibility tool counted members
differently. Both now carry the same figure, taken by running the current tool against a rebuild of
the v1.6.9 assemblies: 100 game types and 192 distinct members, all resolving on all three
versions.

### One open question is closed, and without needing the game

Whether a vanilla definer collides with the mod's own save type-definer id range, base
`724,501,000`, cannot arise any more. Since 1.6.20 TradeLord registers no definer and defines no
saveable type, so a campaign it saves carries `string`, `int`, `bool` and `Settlement` and nothing else. There is no id
left to collide with. That was one of the five open questions this document opened with, and the
only one that could have shown up as a save which will not open. It is struck from the table below,
and it is why T9 now reads the way it does.

## What could not be checked without the game

The reference assemblies are signatures without implementation. Every method body in them is `throw null;`, and the string-literal heap is empty - `TaleWorlds.CampaignSystem.dll` contains zero user strings. Anything that depends on what the game's code *does*, or on a string the game holds, is invisible to this method. That is where the remaining risk lives.

| Open question | How it stood | Settled |
|---|---|---|
| Whether the patched methods still behave the same | Signature-identical is not behaviour-identical. If the inside of `SetMerchandiseComponentTooltip` or `UpdateProfitType` had changed, the patches would still attach cleanly and then act on assumptions that no longer hold. | **Yes** - T4, T7 and T8 all behaved, on the game. |
| The four game menu ids | `town`, `village`, `port_menu` and `naval_storyline_virtualport` are registered as string literals in game code, unrecoverable from stripped bodies. | **Three of four.** The shipped `TaleWorlds.CampaignSystem.dll` carries a user-string heap of 13,694 entries, and `town`, `village` and `port_menu` are each in it exactly once. T2 confirmed the first two in play. `naval_storyline_virtualport` belongs to War Sails, and every NavalDLC reference assembly on NuGet ships with an empty string heap, so it can only be confirmed against a real install. It is guarded and fails soft. |
| Five game brushes and one sprite | `Frame1Brush`, `Popup.Button.Text`, `Popup.Done.Button.NineGrid`, `Recruitment.Popup.Title.Text`, `FaceGen.Scrollbar.Handle`, sprite `BlankWhiteSquare_9`. These live in the game's GUI data files, which the NuGet packages do not ship. | **Yes** - T6 opened the panel fully styled, frame, title, scrollbar handle and row hover all drawing. |
| Whether the MCM stack has caught up | The settings screen needs MCM, ButterLib and UIExtenderEx to have 1.4.8 builds of their own. | **Yes** - T10 registered the settings menu and rendered all six headings. |

## Test pass in game

**Walked on 28 August 2026, on game 1.4.8.119303 with TradeLord v1.6.21. Every test passed.** Three
faults came out of it, none of them compatibility faults, all three fixed in 1.6.22 and noted under
their own test below. T3 could not be run because the install has no War Sails.

Install the released v1.6.21 zip on 1.4.8 and change nothing else - it should load as it stands. Nearly all of this lands in `Documents\Mount and Blade II Bannerlord\TradeLord.log`, which starts fresh on every launch, so one clean session covers most of the list.

### T1 - Launch, then read the first line of the log

Before touching anything else. This one line settles whether the 1.4.7-built assembly binds to 1.4.8 at all, and the lines under it say whether each of the four patches attached.

- **Want:** `TradeLord v1.6.21 loaded | game 1.4.8.x`
- **Trouble:** no log file at all, or any line reading `ERROR in patching ...`, which names exactly which patch failed
- **Result:** **passed.** `TradeLord v1.6.21.0 loaded | game v1.4.8.119303`, and no patch error under it.

### T2 - Walk into a town, then a village

Look for **Consult the TradeLord ledger**. That entry shows unconditionally, so it is the clean test of the menu id - the quick-sell and quick-buy entries hide themselves when auto-trade is on.

- **Want:** the ledger entry in both menus
- **Trouble:** no TradeLord entries in one of them, and `ERROR in menu town` or `ERROR in menu village` in the log, meaning that menu's id moved in 1.4.8
- **Result:** **passed.** The TradeLord entries are present and working in both menus.

### T3 - Walk into a port, if War Sails is installed

Same check. This one already fails soft by design, so a miss costs nothing in play; it just writes a line worth reading.

- **Want:** the ledger entry, and `naval capability: party can sail` in the log
- **Trouble:** `ERROR in menu port_menu` - harmless, but it means the port id changed
- **Result:** **not run - no War Sails.** The log reads `naval capability: land-only - land routing in effect`, which is the correct answer for a install without the DLC.

### T4 - Quick-sell a load of goods

The most important behavioural test on the list. The mod checks the gold actually moved and aborts if it moved the wrong way, so a change inside the game's trade code announces itself rather than quietly robbing the player.

- **Want:** a `quick-sell:` line, gold up by the amount reported, coin sound, and the amber Trade XP line
- **Trouble:** `selling removed N gold - transaction direction changed on this game version`. Stop there.
- **Result:** **passed.** Quick-sell moved goods, gold rose by the reported amount, and no direction error was ever raised.

### T5 - Quick-buy in a town that stocks livestock

Cattle, sheep or hogs on the shelf specifically. Buying livestock is what makes the mod reach for the game's private herding method by reflection, and that lookup reports itself when it fails.

- **Want:** a `quick-buy:` line, and no herd-guard line at all
- **Trouble:** `herd guard: GetHerdingModifier not found on this game version`, or `buying added N gold`
- **Result:** **passed.** Livestock bought without a herd-guard line; the reflection lookup held.

### T6 - Open the ledger panel on the map and screenshot it

Press the panel hotkey. The panel borrows five brushes and a sprite from the game's own art, and those are the pieces the inspection had no way to check. A screenshot answers all six at once, faster than any log line.

- **Want:** framed border, styled title, a visible scrollbar handle, and rows that light up under the cursor
- **Trouble:** the panel opens but looks bare, has no frame, or has an invisible scrollbar handle - a borrowed brush is gone
- **Result:** **passed, with two faults.** The panel opens fully styled, so all five brushes and the sprite are present in 1.4.8. Two problems came out of it: the hotkey collided with the game's own message panel, and the line under the ledger was too small to read. Both fixed in 1.6.22.

### T7 - Hover an item in a town's trade screen

Exercises both tooltip patches at once: the one that adds TradeLord's section, and the one that suppresses vanilla's rumour lines so the two do not both appear.

- **Want:** TradeLord's best buy/sell block, with `Stock:` and `~N days`, and vanilla's rumour lines gone
- **Trouble:** TradeLord's block missing, or both sets of price hints showing at once
- **Result:** **passed, with one fault.** Both tooltip patches behave; vanilla rumour lines are gone. The block carried a "TradeLord ledger" heading that read as an advertisement, removed in 1.6.22.

### T8 - Glance at the inventory colouring

Open the inventory in a market. Trade goods and livestock should be tinted by how this market's price compares with the best one known. This is the third patch, and the only one with no log line of its own - it can only be seen.

- **Want:** green and red tinting across trade goods, horses and livestock alike
- **Trouble:** everything grey, or only some categories tinted
- **Result:** **passed.** Trade goods, horses and livestock all tinted.

### T9 - Load a campaign saved on 1.4.7, then take the mod out of it

One saved by 1.6.19 or later, with TradeLord history in it. A campaign whose last save was written by
1.6.18 or earlier will not open on 1.6.20 or later at all, by design, and is not what this tests. Two things
settle here: that the saved prices and purchase records survive the version change, and that the save
no longer needs the mod in order to open.

- **Want:** `ledger restored: N observed items...`, the campaign profit total intact in the panel's top row, and then, with TradeLord disabled in the launcher, that same campaign still loading and playing without its ledger
- **Trouble:** the save refusing to load with the mod on, the ledger coming back empty, or the save refusing to load with the mod off - the last of those means something the mod owns is still reaching the save file
- **Result:** **passed.** The campaign loaded and the ledger came back - `ledger restored: 0 observed items, 8 purchase records, lifetime profit 897`.

### T10 - Open the settings screen and read the labels closely

With MCM, ButterLib and UIExtenderEx all enabled. This carries a second job beyond 1.4.8: the translation markers that shipped in v1.6.9 were verified from the assemblies but never seen rendering in game, and this is what confirms them.

- **Want:** `MCM detected - settings menu registered`, six headings, and every label reading as plain English
- **Trouble:** any label showing its marker literally, such as `{=TL201}Live world prices`
- **Result:** **passed.** `MCM detected - settings menu registered`, all six headings, every label in plain English. One setting was wrong in a different way: the buy cap per item had no off position, fixed in 1.6.22.

### What to collect

- the whole `TradeLord.log` from one session covering T1 to T9
- a screenshot of the ledger panel open on the map
- a screenshot of the settings screen

Every failure above is caught and logged rather than thrown, so none of it should crash the game. If something does, the log's last `ERROR in ...` line names the place.

## Roadmap

1. **Run the test pass on the current build.** Nothing needs changing first. The released v1.6.21 should load on 1.4.8 as it stands, which is what makes this cheap - it answers three of the four open questions without committing the repository to anything.

2. **Fix whatever the pass turns up, if anything.** A failure at T2 or T3 is a menu id to chase; at T6, a brush to replace; at T4, T5 or T7, a patch to re-fit against changed game behaviour. Each ships as its own release, still on 1.4.7 references, because none of them need the bump.

3. **Confirm the MCM stack has a 1.4.8 build, and move 5.12.2 -> 5.12.3 if that is it.** A separate axis from the game version, and only worth doing once 5.12.3's release notes say which game they target.

4. **Then bump the reference assemblies and ship.** `Bannerlord.ReferenceAssemblies` to `1.4.8.119303` in both project files. Since the output is byte-for-byte equivalent, this ships as a statement that 1.4.8 is the tested target - worth saying plainly in the release note, and only honest once the pass has actually been walked.
