# TradeLord on game 1.4.8

The mod is built against Bannerlord `1.4.7.117484`. Players are already on `1.4.8.119303`. This is what an inspection of the seam between the two versions found, what it could not reach, and what is left to settle in game.

| | |
|---|---|
| Mod version inspected | 1.6.9 |
| From | 1.4.7.117484 |
| To | 1.4.8.119303 |
| Method | NuGet reference assemblies for both versions, plus the mod's own compiled output. No game install involved. |
| Reproduce it | `dotnet run --project tools/compat -- 1.4.8.119303` |

## Verdict

Nothing found that blocks 1.4.8. Every game type, member, enum value and UI binding the mod touches is unchanged between the two versions, and both projects compile clean against 1.4.8 with warnings as errors.

The reference assemblies carry signatures only, so this proves the mod still *fits* 1.4.8, not that it still *behaves* the same. Five things can only be settled by loading the game; the test pass below is what settles them.

## Verified compatible

**Both projects compile against 1.4.8.** The whole source tree rebuilt against the 1.4.8 reference assemblies in Release, with `TreatWarningsAsErrors` on. No errors, no warnings, and the source checks pass in that tree too.

**The bound API surface is unchanged.** Reading the two compiled assemblies' type and member reference tables gives the exact game surface the mod binds to. All of it resolved in both versions and compared by signature, accessibility and parameter name.

- 100 game types bound, 198 distinct members bound, 100/100 resolved in both versions
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

**Every enum the mod switches on has identical values.** A renumbered enum is the quietest way a game update breaks a mod: it keeps compiling and starts doing the wrong thing. All seven were compared member by member, including each numeric value: `SettlementAction`, `LeaveType` (48 members, naval ones included), `InventorySide`, `NavigationType`, `VillageStates`, `ItemTiers`, `TooltipPropertyFlags`.

**The ledger panel's Gauntlet bindings all resolve.** The panel prefab names widget classes and properties as text, resolved by the UI loader at runtime. All 9 widget types it uses are present, and all 435 attribute bindings resolve through the base-class chain in 1.4.8.

**Assembly identity did not move.** This is why the mod can cross the version line at all. The game's assemblies are unsigned and permanently versioned `1.0.0.0`, so a DLL compiled against 1.4.7 binds to 1.4.8's assemblies with no redirect and no rebuild. All 81 assemblies, zero identity differences.

**Nothing the mod touches changed anywhere in the game.** Diffing the two versions whole, across every type in all 81 assemblies, gives 12,731 -> 12,732 types. The entire difference is three compiler-generated closure classes renumbering inside `NavalDLC.Missions.Objects.MissionShip`, and one new engine delegate for clearing decals.

**Building against 1.4.8 changes nothing in the output.** Because the bound surface and the assembly identities are both unchanged, the compiler emits the same references either way. Both builds come out the same size with the same external reference set - `TradeLord.dll` at 83,968 bytes, `TradeLord.MCM.dll` at 25,088, 32 identical TaleWorlds reference names. The version bump is a statement of which game the mod is tested against, not a change to what ships.

**The naval surface survived, which was the likeliest casualty.** The only churn anywhere in 1.4.8 is inside NavalDLC, and the mod reaches into naval territory twice: the sailing-aware travel estimate and the port menu entries. `NavigationType` kept its values, and so did the naval members of `LeaveType` - `VisitPort=41`, `SetSail=43`, `ManageFleet=44`, `CallFleet=45`, `RepairShips=47`.

## Also checked: the 1.5.1 beta

`1.5.1.120547-beta` was published while this was being written, and the same method reaches the same
verdict on it. Nothing found that blocks it.

- all 103 bound game types and 201 bound members still resolve
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

## What could not be checked without the game

The reference assemblies are signatures without implementation. Every method body in them is `throw null;`, and the string-literal heap is empty - `TaleWorlds.CampaignSystem.dll` contains zero user strings. Anything that depends on what the game's code *does*, or on a string the game holds, is invisible to this method. That is where the remaining risk lives.

| Open question | Why it is out of reach | Settled by |
|---|---|---|
| Whether the patched methods still behave the same | Signature-identical is not behaviour-identical. If the inside of `SetMerchandiseComponentTooltip` or `UpdateProfitType` changed, the patches still attach cleanly and then act on assumptions that no longer hold. | T4, T7, T8 |
| The four game menu ids | `town`, `village`, `port_menu`, `naval_storyline_virtualport` are registered as string literals in game code, unrecoverable from stripped bodies. The two port ids are already wrapped and fail soft; `town` and `village` are not. | T2, T3 |
| Five game brushes and one sprite | `Frame1Brush`, `Popup.Button.Text`, `Popup.Done.Button.NineGrid`, `Recruitment.Popup.Title.Text`, `FaceGen.Scrollbar.Handle`, sprite `BlankWhiteSquare_9`. These live in the game's GUI data files, which the NuGet packages do not ship. A missing brush does not crash the panel - it draws unstyled. | T6 |
| Whether a 1.4.7 campaign still loads | The mod claims a save type-definer id range of its own, base `724,501,000`. Whether 1.4.8 introduced a vanilla definer colliding with it is not something signatures can answer, and a collision shows up as a save that will not open. | T9 |
| Whether the MCM stack has caught up | A separate axis. The settings screen needs MCM, ButterLib and UIExtenderEx to have 1.4.8 builds of their own. The mod already degrades to built-in defaults when the stack is missing or half-loaded, so this affects the settings screen, not trading. | T10 |

## Test pass in game

Install the released v1.6.9 zip on 1.4.8 and change nothing else - it should load as it stands. Nearly all of this lands in `Documents\Mount and Blade II Bannerlord\TradeLord.log`, which starts fresh on every launch, so one clean session covers most of the list.

### T1 - Launch, then read the first line of the log

Before touching anything else. This one line settles whether the 1.4.7-built assembly binds to 1.4.8 at all, and the lines under it say whether each of the four patches attached.

- **Want:** `TradeLord v1.6.9 loaded | game 1.4.8.x`
- **Trouble:** no log file at all, or any line reading `ERROR in patching ...`, which names exactly which patch failed
- **Result:** _not yet run_

### T2 - Walk into a town, then a village

Look for **Consult the TradeLord ledger**. That entry shows unconditionally, so it is the clean test of the menu id - the quick-sell and quick-buy entries hide themselves when auto-trade is on.

- **Want:** the ledger entry in both menus
- **Trouble:** no TradeLord entries in one of them, meaning that menu's id moved in 1.4.8
- **Result:** _not yet run_

### T3 - Walk into a port, if War Sails is installed

Same check. This one already fails soft by design, so a miss costs nothing in play; it just writes a line worth reading.

- **Want:** the ledger entry, and `naval capability: party can sail` in the log
- **Trouble:** `ERROR in port menu port_menu` - harmless, but it means the port id changed
- **Result:** _not yet run_

### T4 - Quick-sell a load of goods

The most important behavioural test on the list. The mod checks the gold actually moved and aborts if it moved the wrong way, so a change inside the game's trade code announces itself rather than quietly robbing the player.

- **Want:** a `quick-sell:` line, gold up by the amount reported, coin sound, and the amber Trade XP line
- **Trouble:** `selling removed N gold - transaction direction changed on this game version`. Stop there.
- **Result:** _not yet run_

### T5 - Quick-buy in a town that stocks livestock

Cattle, sheep or hogs on the shelf specifically. Buying livestock is what makes the mod reach for the game's private herding method by reflection, and that lookup reports itself when it fails.

- **Want:** a `quick-buy:` line, and no herd-guard line at all
- **Trouble:** `herd guard: GetHerdingModifier not found on this game version`, or `buying added N gold`
- **Result:** _not yet run_

### T6 - Open the ledger panel on the map and screenshot it

Press the panel hotkey. The panel borrows five brushes and a sprite from the game's own art, and those are the pieces the inspection had no way to check. A screenshot answers all six at once, faster than any log line.

- **Want:** framed border, styled title, a visible scrollbar handle, and rows that light up under the cursor
- **Trouble:** the panel opens but looks bare, has no frame, or has an invisible scrollbar handle - a borrowed brush is gone
- **Result:** _not yet run_

### T7 - Hover an item in a town's trade screen

Exercises both tooltip patches at once: the one that adds TradeLord's section, and the one that suppresses vanilla's rumour lines so the two do not both appear.

- **Want:** TradeLord's best buy/sell block, with `Stock:` and `~N days`, and vanilla's rumour lines gone
- **Trouble:** TradeLord's block missing, or both sets of price hints showing at once
- **Result:** _not yet run_

### T8 - Glance at the inventory colouring

Open the inventory in a market. Trade goods and livestock should be tinted by how this market's price compares with the best one known. This is the third patch, and the only one with no log line of its own - it can only be seen.

- **Want:** green and red tinting across trade goods, horses and livestock alike
- **Trouble:** everything grey, or only some categories tinted
- **Result:** _not yet run_

### T9 - Load a campaign saved on 1.4.7

One with TradeLord history in it. The only test of whether the mod's saved prices and purchase records survive the version change.

- **Want:** `ledger restored: N observed items...`, and the campaign profit total intact in the panel's top row
- **Trouble:** the save refusing to load, or the ledger coming back empty
- **Result:** _not yet run_

### T10 - Open the settings screen and read the labels closely

With MCM, ButterLib and UIExtenderEx all enabled. This carries a second job beyond 1.4.8: the translation markers that shipped in v1.6.9 were verified from the assemblies but never seen rendering in game, and this is what confirms them.

- **Want:** `MCM detected - settings menu registered`, six headings, and every label reading as plain English
- **Trouble:** any label showing its marker literally, such as `{=TL201}Live world prices`
- **Result:** _not yet run_

### What to collect

- the whole `TradeLord.log` from one session covering T1 to T9
- a screenshot of the ledger panel open on the map
- a screenshot of the settings screen

Every failure above is caught and logged rather than thrown, so none of it should crash the game. If something does, the log's last `ERROR in ...` line names the place.

## Roadmap

1. **Run the test pass on the current build.** Nothing needs changing first. The released v1.6.9 should load on 1.4.8 as it stands, which is what makes this cheap - it answers four of the five open questions without committing the repository to anything.

2. **Fix whatever the pass turns up, if anything.** A failure at T2 or T3 is a menu id to chase; at T6, a brush to replace; at T4, T5 or T7, a patch to re-fit against changed game behaviour. Each ships as its own release, still on 1.4.7 references, because none of them need the bump.

3. **Confirm the MCM stack has a 1.4.8 build, and move 5.12.2 -> 5.12.3 if that is it.** A separate axis from the game version, and only worth doing once 5.12.3's release notes say which game they target.

4. **Then bump the reference assemblies and ship.** `Bannerlord.ReferenceAssemblies` to `1.4.8.119303` in both project files. Since the output is byte-for-byte equivalent, this ships as a statement that 1.4.8 is the tested target - worth saying plainly in the release note, and only honest once the pass has actually been walked.
