# What is here

Shipped Bannerlord assemblies, dropped here so the compatibility tool and a session can read
what reference assemblies cannot answer. Reference assemblies carry signatures only: they have
no string literals and no method bodies, so menu ids, dialog ids and what a game method actually
does are all out of reach without the real files.

Nothing here is packaged into the release. The workflow copies named paths into the download and
this folder is not one of them.

To read them:

    TRADELORD_GAME_BIN=<path to this folder> dotnet run --project tools/compat -c Release -- 1.5.3.122374-beta

## What has been dropped

From build 1.5.3.122374, taken from the owner's install. TaleWorlds stamps every one of these
1.0.0.0, so the build number is what the owner says it is and nothing in the files confirms it.

    TaleWorlds.CampaignSystem.dll
    SandBox.dll
    SandBox.View.dll
    StoryMode.dll

## What they answered

The menu-id check now runs instead of skipping. It found `town` and `village` in SandBox.View.dll,
`port_menu` in TaleWorlds.CampaignSystem.dll, and `naval_storyline_virtualport` in none of them,
which is what a guarded id absent from a plain install should look like. Both of the lines a band
of bandits opens with, `bandit_start_defender` and `bandit_start_defender_2`, are in
TaleWorlds.CampaignSystem.dll.

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
  way out, a tenth on the way in

## Still wanted

    Modules/NavalDLC/bin/Win64_Shipping_Client/NavalDLC.dll

The War Sails module, for `naval_storyline_virtualport`. Without it the id cannot be told apart
from a typo, only from an id the install does not carry.

    bin/Win64_Shipping_Client/TaleWorlds.CampaignSystem.ViewModelCollection.dll

For the profit colour on an inventory row. SPItemVM is in this file, not in SandBox.View.dll where
this note first looked for it, so what those colour values are allowed to be is still unread.

Both have to be the shipped file. The reference assemblies on nuget hold a NavalDLC.dll with no
user-string heap in it at all, so one carries no menu id and answers nothing here, and the
compatibility tool already has it on disk anyway. Searching one of those for an id as raw bytes
does turn up hits, in the name and blob heaps, and every one of them is a coincidence. Read the
heap, not the file.
