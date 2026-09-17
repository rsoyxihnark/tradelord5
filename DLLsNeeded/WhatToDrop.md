# What is here

Shipped Bannerlord assemblies, dropped here so the compatibility tool and a session can read
what reference assemblies cannot answer. Reference assemblies carry signatures only: they have
no string literals and no method bodies, so menu ids, dialog ids and what a game method actually
does are all out of reach without the real files.

Nothing here is packaged into the release. The workflow copies named paths into the download and
this folder is not one of them.

These files are here with the developer's permission. They stay. Never delete one, never propose
deleting one, and do not raise the question again: it is settled, and this line is the answer.

To read them:

    TRADELORD_GAME_BIN=<path to this folder> dotnet run --project tools/compat -c Release -- 1.5.3.122374-beta

## What has been dropped

From build 1.5.3.122374, taken from the owner's install. TaleWorlds stamps every one of these
1.0.0.0, so the build number is what the owner says it is and nothing in the files confirms it.

    TaleWorlds.CampaignSystem.dll
    TaleWorlds.CampaignSystem.ViewModelCollection.dll
    SandBox.dll
    SandBox.View.dll
    StoryMode.dll
    NavalDLC.dll

## What they answered

The menu-id check now runs instead of skipping, and with the War Sails module here every one of
the four ids is answered. All four, `town`, `village`, `port_menu` and `naval_storyline_virtualport`,
are real user strings in NavalDLC.dll, which is the one the check names because it reads the
assemblies in name order and that one comes first. Without it, `town` and `village` were found in
SandBox.View.dll and `port_menu` in TaleWorlds.CampaignSystem.dll, while
`naval_storyline_virtualport` was in none of them. Both of the lines a band of bandits opens with,
`bandit_start_defender` and `bandit_start_defender_2`, are in TaleWorlds.CampaignSystem.dll.

The shipped NavalDLC.dll carries a user-string heap of 444900 bytes holding 3523 entries. The
NavalDLC.dll on nuget carries no `#US` stream at all, so it holds no menu id and can answer
nothing, which is why the shipped file is the only one worth keeping here.

Read from the method bodies, every runtime assumption the mod was guarding rather than knowing
now has an answer on this build:

- a market's price does move unit by unit. Selling re-reads the price for each single unit, and a
  town's own roster update feeds its supply and demand straight back into the next reading
- a village does not. Nothing updates a store when a village roster changes, so the price there
  stays flat however much is traded
- selling hands gold to the player and buying takes it, so the direction the mod watches for is
  the direction this build moves
- the herd penalty never eases as the herd grows, so searching it by halving is sound
- buying a workshop does take the gold for it, so the fallback that pays the seller itself cannot
  fire here
- a village applies a trade penalty only when the caller names the merchant: a full one on the
  way out, a tenth on the way in. A trade of the mod's own names no merchant, so it is charged
  neither, which is the basis the mod now reads a price on
- the profit colour on an inventory row runs from -2 to 2, and the mod writes every one of the
  five. The game's own reading never returns -2: it tests for a small loss before a large one,
  so the large case cannot be reached

## Still wanted

Nothing. Every id the compatibility tool asks about is answered by the assemblies here.
