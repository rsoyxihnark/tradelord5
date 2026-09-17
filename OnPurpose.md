# Designs that look like faults

Every entry here reads as a bug to someone meeting it cold. Each one is deliberate, and each was argued out with the owner. Read this before calling one of them a fault, and never undo one without asking him first.

Where a check in `tools/regression_sweep.py` names one of these, the check is the guard. It is there to stop the design being undone, so rewrite it only to follow the source, never to let the design go.

## The mod names the settlement as the merchant, which makes its own prices worse

A market gives a different price depending on whether the caller names the settlement as the merchant. Named, a village pays about half for what you sell, and a faction you are at war with charges more. Not named, none of that applies.

The game's trade screen always names it. The trade action the mod uses never does. So the mod asks with it named, and takes over the price its own trade is charged so the two agree.

This looks like the mod handing itself a penalty for no reason. It is the mod getting what the player gets by hand, which is the whole point. Taking the merchant out would hand the player roughly double what a village really pays.

## The mod refuses to trade rather than fall back

Where that takeover cannot be installed, the mod trades nothing in a town or a village and writes why into its log. Where a price cannot be read the way the trade screen reads it, that good is left alone.

Both refusals are deliberate. Trading at a price the player could not be charged is the thing being avoided, so a fallback that trades anyway would undo the fix rather than improve it.

## Selling something with a quality on it earns no Trade skill

The game gives no trade experience for an item carrying a modifier. The mod does the same. It is not an oversight in the mod's accounting.

## Whoever made the trade credits the Trade skill

A trade the mod makes itself is credited by the mod, because the game never sees that trade and would credit nothing. A deal the mod lays out on the game's own trade screen is credited by the game, and the mod stays out of it, because the game already credits that one.

Side by side the two halves look inconsistent. They are one rule: the one who traded credits it. Making both sides credit was the bug, and it was fixed by taking the mod out of the second.

One cost was accepted knowingly: cargo bought by the mod's own trading and then sold on the trade screen earns nothing, because the game never recorded the purchase. It is a cost that only appears when the player switches between the two ways of trading, and it errs towards giving less rather than more.

## The lifetime Trade XP counts both halves

What the mod credits it knows directly. What the game credits it hears through the game's own trade profit event, and only while a deal of the mod's own is on the screen, so a trade the player made alone is never counted as the mod's.

## Trading with a party on the road is priced at the plain value

The game has no way for a player to trade with a caravan or a party of villagers by hand. There is no price to match, so there is nothing here to bring into line, and the mod moves the gold itself at exactly the price it quoted.

## Reading distant markets is a feature, not a leak

Prices in markets the party has not visited, and what is on the road towards them, are things a player cannot know. Both are advertised on the mod's own page and both can be switched off. They are not in the same class as a price the player could not be charged.
