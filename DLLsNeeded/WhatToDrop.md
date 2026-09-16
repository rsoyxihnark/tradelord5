# What to drop here

Shipped Bannerlord assemblies, dropped here so the compatibility tool and a session can read
what reference assemblies cannot answer. Reference assemblies carry signatures only: they have
no string literals and no method bodies, so menu ids, dialog ids and what a game method actually
does are all out of reach without the real files.

Nothing here is packaged into the release. The workflow copies named paths into the download and
this folder is not one of them.

Say which game version each file came from, since the mod is built against 1.4.8.119303 and
checked against 1.5.2.121216-beta.

## First, and worth the most

bin/Win64_Shipping_Client/TaleWorlds.CampaignSystem.dll

This one file answers every assumption the mod currently guards at runtime rather than knows:

- how a market's price moves as its shelf fills and empties, which is what the ledger's whole
  unit by unit price walk rests on
- which way gold moves when a good is sold, which the mod watches for at runtime and writes an
  error about if it ever reverses
- how the herd slows a party, which the mod searches by halving and so needs to be well behaved
- whether buying a workshop takes the gold for it, which the mod pays itself when it does not
- how a party met on the road is priced

## Next, for the ids nothing else can confirm

Modules/SandBox/bin/Win64_Shipping_Client/SandBox.dll
Modules/StoryMode/bin/Win64_Shipping_Client/StoryMode.dll
Modules/NavalDLC/bin/Win64_Shipping_Client/NavalDLC.dll

These carry the town and village menu ids the trade entries hang off, the port menu ids, and the
lines a band of bandits opens with. A wrong id is silent: the entry simply never appears.

## Last, for the inventory colours

Modules/SandBox/bin/Win64_Shipping_Client/SandBox.View.dll

Says what the profit colour values on an inventory row are allowed to be.
