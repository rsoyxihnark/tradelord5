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
which is what a guarded id absent from a plain install should look like.

TaleWorlds.CampaignSystem.dll is the one worth the most. It is the only file that can answer, from
the method bodies themselves rather than from a runtime guard:

- how a market's price moves as its shelf fills and empties, which is what the ledger's whole
  unit by unit price walk rests on
- which way gold moves when a good is sold, which the mod watches for at runtime and writes an
  error about if it ever reverses
- how the herd slows a party, which the mod searches by halving and so needs to be well behaved
- whether buying a workshop takes the gold for it, which the mod pays itself when it does not
- how a party met on the road is priced

## Still wanted

    Modules/NavalDLC/bin/Win64_Shipping_Client/NavalDLC.dll

The War Sails module, for `naval_storyline_virtualport`. Without it the id cannot be told apart
from a typo, only from an id the install does not carry.

It has to be the shipped file. The `Bannerlord.ReferenceAssemblies.NavalDLC` package on nuget
holds a NavalDLC.dll with no user-string heap in it at all, so it carries no menu id and answers
nothing here, and the compatibility tool already has it on disk anyway. Searching one of those
for an id as raw bytes does turn up hits, in the name and blob heaps, and every one of them is a
coincidence. Read the heap, not the file.
