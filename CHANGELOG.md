# Changelog

## 1.6.9

- The settings screen was the last part of the mod left in English only; every setting name, hint and heading now ships in the language file and can be translated.

## 1.6.8

- The TradeLord button on the campaign map no longer swallows map clicks well above and below the button itself.
- The days of food you asked to keep are no longer partly spent on goods quick-sell was never going to sell.
- Other mods now get first say over their own notifications while TradeLord is trading.

## 1.6.7

- The Trade XP message now names its number for what it is: the denars of profit credited to your Trade skill.
- Clicking a town in the ledger panel now pins it even when TradeLord is already marking that town as your best place to sell.
- Quick-sell no longer spends part of your food reserve on goods it then passes over because you bought them here on this visit.
- A trade message the game refuses to show no longer leaves the rest of that pass's messages repeating.

## 1.6.6

- Putting an item on the always-sell list no longer makes quick-buy purchase it against your category policy.
- Loading your settings no longer switches quick-buy back on when your saved settings say it should be off.
- The route lines in the ledger popup are now translatable, like every other line the mod shows.

## 1.6.5

- A village that is down to its last of each good now says so, instead of reporting that it has nothing worth trading.
- The ledger panel now reopens instantly within the same game hour, and the Refresh button rescans whenever you want fresh numbers.
- Trade messages left over from a campaign you have closed no longer appear once you are back at the main menu.
- A damaged never-sell, always-sell or never-buy list in the settings file no longer stops trading outright.

## 1.6.4

- TradeLord now warns you in red when your cargo is full, both as you walk into a market and as you leave it, instead of quietly buying nothing.
- Restored the working-notes file to the repository; nothing that ships with the mod changed.

## 1.6.3

- A release whose commit body is blank is now stopped before it publishes, instead of going out with an empty description as 1.6.1 did.

## 1.6.2

- Every setting now explains itself on hover; keep food, never sell, always sell and the coin sound had no description before.
- The never-sell and always-sell descriptions now say where to find an item id and which of the two lists wins when they disagree.
- The buying settings are now written in the order they appear on screen.
- A fault while showing a trade message, or while closing a campaign, is now logged and stepped over instead of reaching the game.
- Asking the ledger what a good cost when it has never seen that good now answers instead of faulting.
- Tidied the layout of the two trading passes; they behave exactly as before.
- Removed the working-notes file from the repository; nothing that ships with the mod changed.

## 1.6.1

- The Trade XP a trade earns is now reported in amber, right after the sold and bought lines instead of before them.
- Raised the default gold reserve from 100 to 300, enough for two bribes for safe passage and a wage payment. Your own setting is unchanged.

## 1.6.0

- Fixed a trade that failed partway through being able to stop the game showing any notifications at all until you reloaded, including TradeLord's own trade summaries.
- Ending a campaign no longer leaves leftover trade state behind for the next one.
- Rewrote every settings description in plain English.
- The panel and the ledger popup now say "resale safety factor", the same name the settings screen uses, instead of "resale haircut".
- Added this changelog.
- Added zip files and generated release notes to the ignore list.

## 1.5.11

- Replaced the black star in tooltips with a plain asterisk, because the game's font may not have that character and would draw an empty box.
- Fixed the panel's workshop board listing every workshop in Calradia even with live world prices switched off; it now lists your own.
- Renamed the panel's "Total profit" line to "TradeLord profit", which is what it actually counts.
- Removed the mentions of other trade mods from the settings hints.

## 1.5.10

- Fixed a town with no price for a good being called the cheapest place to buy it, because a missing price read as 0 and 0 sorts cheapest.
- Fixed the log setup being able to stop the whole mod loading if your Documents folder path could not be built.
- Moved the build onto checkout v7, setup-dotnet v6 and upload-artifact v7, off the retired Node 20 runtime the old versions needed. The compiler and the DLLs are unchanged.

## 1.5.9

- Fixed a town pinned in the ledger panel not being unpinnable after you reloaded the campaign.
- Fixed the same problem letting the automatic marker delete a pin you had placed by hand.

## 1.5.8

- Fixed the ledger panel having no translatable text at all, so players on any other language got an English panel.
- Fixed the tooltip's "Profit: +N%", "Stock: N" and "~N days" labels not being translatable.
- Every line the mod shows now carries a translation marker and ships in the language file, so the whole mod can be translated or forked without touching the source.

## 1.5.7

- Fixed a stack of part-bought, part-looted goods being refused entirely when the bought units missed the profit margin, stranding the looted units that had no cost to clear.
- The purchase record is now drained against the units it actually covers, in every cost-basis mode.

## 1.5.6

- Removed the project documentation from the repository.
- The build no longer needs a changelog entry to publish a release; the notes come from the commit instead.
- Moved both build projects next to the source they compile, so CI and a local build compile the same thing.
- Added the build output folder, IDE folders and the log to the ignore list.
- Fixed the log being written to the game's program folder, where the write fails silently under Program Files; it now prefers your Bannerlord user folder.
- Fixed live-price mode saving a price note for every good in every settlement you entered, which nothing in that mode ever reads; existing saves shrink.
- Narrowed the message filter so it only silences the game's own trade messages during a pass, not other mods'.
- Fixed a manual purchase being recorded at the price after the trade, which is higher than what you actually paid.
- A failed panel setup is now retried twice more before being given up on.
- A panel hotkey the game cannot name is now reported in the log instead of silently becoming T.
- Fixed the cargo marker picking the first town it scanned even when the cargo was worth nothing anywhere.

## 1.5.5

- Fixed a dry run blocking the real trade that followed it in the same visit.
- Fixed the best-market floor never reaching the loot it exists to protect.
- Fixed a mixed stack charging the looted units a cost they never had.

## 1.5.4

- Fixed the publish step still being able to lose a release to a timeout on retry.
- Fixed three refusals being answered with a message that was not true, such as blaming your goods when the merchant was simply out of money.
- The panel now marks which routes it could not price unit by unit.
- Fixed an English footnote sitting in an otherwise translated dialog.
- Removed every comment from the C# sources and the build workflow.

## 1.5.3

- Corrected the README's count of audit findings.
- Corrected the README's count of releases and the version it named.
- Corrected the Nexus description of what the four Harmony patches do.
- Corrected a wrong check count printed in the 1.5.2 notes.
- Noted that all four wrong numbers were the same failure: a fact written by hand that nothing verified.

## 1.5.2

- Fixed the panel listing round trips it could only half make, when a category was set to buy only.
- Fixed the War Sails port menus being registered by trying and hoping rather than checking.
- Fixed the panel and the tooltip quoting different prices for the same shelf.
- Removed two fields from the internal price quote that nothing ever read.
- Fixed a release being lost when the publish step timed out.

## 1.5.1

- Fixed observed-price mode quoting live prices through the new bulk pricing.
- Fixed a shelf that cannot be priced per unit being asked for its price once per unit, giving the same answer every time.
- Fixed the confidence score partly measuring two of the game's own price functions disagreeing with each other.
- Fixed the "nothing traded" message usually naming the wrong reason, because loot in your inventory drowned out the real one.
- Fixed two settings sharing a position with two others in the Action group.
- The quiet no-trade log line no longer repeats itself at every town gate.

## 1.5.0

- The panel now prices a whole lot one unit at a time, the way you will actually buy it, instead of multiplying one unit's spread by the quantity.
- Routes are now ranked by how likely their profit is to survive the trip, not by profit alone.
- When a pass trades nothing, it now says which rule stopped it.
- Food, smithing materials and livestock each take a policy now - ignore, sell only, buy only, or buy and sell - instead of a plain on/off switch.
- Added the mod's menu entries to the War Sails port menus.

## 1.4.3

- Fixed observed-price mode going on quoting prices it had itself moved.
- Fixed the ledger panel being torn down and rebuilt every time you opened any other screen.
- The cost basis now asks the same livestock question the rest of the mod asks.
- Fixed Trade XP collapsing to almost nothing on goods you had not bought.
- The best-market floor now always guards goods with no cost basis, replacing a cost the mod used to invent for them.

## 1.4.2

- Fixed two more village states the game refuses to trade in still being offered as destinations.
- Fixed the travel fallback returning a straight-line distance paired with a pathfinder's land ratio.
- A visit no longer shows the same "market is still settling" message twice.

## 1.4.1

- Fixed quick-buy pricing the whole shelf before noticing it had no money to spend.
- Fixed a pass stopped by the safety guard blaming your trade policy instead of saying what actually happened.
- Fixed the route planner offering a village's last unit, which the mod would never actually take.
- Auto-trade on entry no longer re-asks what counts as a market.

## 1.4.0

- Fixed the ledger panel not scrolling, because one widget path was missing a level.
- The panel hotkey now accepts a modifier, such as Ctrl+T.
- Added a running campaign profit total to the panel's top row, saved with the campaign.
- Trade summaries are now colored: green for profit, amber for none, blue for spending, grey for notices.
- Trade summaries are no longer buried under the messages the game posts when you enter a town.

## 1.3.35

- Fixed the panel's Refresh button not actually refreshing the routes, because it rebuilt them from the same cache.
- Tidied one redundantly written type name.

## 1.3.34

- Fixed the two travel-distance calculations disagreeing about whether your party can sail.
- Fixed the travel cache storing days instead of distance, so its numbers went stale the moment cargo changed your speed.
- Buying a ship now clears both travel caches at once instead of leaving land-only distances in place for up to an hour.
- Fixed prices going stale after a manual trade in the vanilla trade screen, the same way they did after the mod's own trades.
- Fixed the last price paid being rounded down where the average paid is rounded properly.

## 1.3.33

- Fixed a fully sold stack keeping a denar or two of leftover cost, which nudged the next purchase's average price up.
- Prices are now refreshed after the mod's own trading moves them, instead of the panel and tooltips quoting stale numbers for the rest of the hour.
- Renamed "Protect mounts, unique and crafted items" to "Protect unique and crafted items", because mounts are protected regardless of the setting.
- Removed the leftover chunk-size constant and the two lines that could no longer run.

## 1.3.32

- Corrected a claim in the 1.3.31 notes that simulation mode had become exact; only real trading did.
- Simulation mode now says it is a best case, in the message, the log and the settings hint.

## 1.3.31

- Fixed a sale closing below your minimum profit margin, and sometimes at an outright loss, because trades ran in chunks of ten and the price was only checked once per chunk.

## 1.3.30

- Fixed a damaged purchase record being able to stop a save from loading.
- The purchase index is now rebuilt in one place instead of two.
- Verified that loading MCM settings does not depend on the order the properties are applied in; no change needed.
- Verified that auto-trade on entry does not depend on which handler the game runs first; no change needed.

## 1.3.29

- The buy-side margin rule now has one copy instead of four written two different ways.
- The market-eligibility filter now has one copy instead of one per knowledge mode.
- The panel's column widths are now checked automatically instead of measured by hand.

## 1.3.28

- Fixed the straight-line travel estimate being able to exceed the real one, which would have wrongly hidden reachable markets.
- Tested the faster route search against every possible pair over 300,000 random cases, and it picked the same route every time.

## 1.3.27

- The tooltip hook now declares only the two arguments it actually reads, so a game update has fewer ways to break it.
- Both assemblies now compile with warnings treated as errors.
- Corrected a README claim that both assemblies build clean with every analyzer at maximum level.

## 1.3.26

- Fixed the new pairwise route search asking the game's pathfinder about every pair it considered, instead of filtering with a cheap straight-line estimate first.
- The store page now lists the per-visit spend cap alongside the other route caps.

## 1.3.25

- Fixed the planner throwing away a whole item when its single cheapest buy town and dearest sell town were too far apart, instead of trying other pairs.
- Fixed route quantities ignoring the per-visit spend cap, which is usually the cap that runs out first.
- Quick-buy no longer runs the herd calculation when there is no livestock on the shelf to buy.

## 1.3.24

- Fixed a cow being reserved as food ahead of the grain sitting next to it.
- Confirmed in the field that town names fit the panel's columns; no change needed.

## 1.3.23

- Fixed the food reserve counting a cow as one meal when the game counts it as its meat value.
- Fixed the herd guard under-counting the herd in cavalry parties.
- The mod now asks the game's own trade-permission rules instead of working around them.
- Removed the diagnostics, since decompiling answered the remaining questions more completely than playing could.

## 1.3.22

- Fixed the mod trading at villages the game had closed after a raid.
- Fixed the ledger panel taking the whole keyboard, so space no longer paused and the speed keys stopped working while it was open.
- Settled by diagnostic: the food reserve counting items is correct, because the game counts one unit of food as one food whatever the item.
- Settled by diagnostic: the panel hotkey must not be gated on which layer has focus, because that would have disabled it for everyone.
- Removed the map-wide trade-permission diagnostic, which crashed inside the game's own code when asked about arbitrary settlements.

## 1.3.21

- The trade-permission diagnostic now asks about every town and village at session start, instead of only the ones you walk into.
- The focus diagnostic now also logs what had focus at the exact moment the panel hotkey was pressed.

## 1.3.20

- Added a temporary diagnostic recording the party's food numbers once a day, to settle whether the food reserve should count animals by head or by meat.
- Added a temporary diagnostic recording what the game's own rules say about trading at each settlement you enter.
- Added a temporary diagnostic recording which screen layer has focus while the campaign map is up.

## 1.3.19

- The cargo map marker now also updates when you leave a settlement, so it follows what you actually traded rather than what you walked in with.
- The setting's hint now names all three moments the marker updates.

## 1.3.18

- Fixed the per-item denar cap never reaching the route panel, so a route could show 32 units of a good the cap stops at 5.
- The price-coloring hint now mentions livestock and horses, which 1.3.14 added.
- The "Minimum profit margin" hint now says it applies to buying and to the route panel too, not just selling.
- The "Economy settling delay" hint now says it stops the town-menu buttons as well as automation.
- Corrected two stale README lines about tooltips and per-item caps.

## 1.3.17

- Fixed the scan radius never reaching the cargo map marker.
- Fixed the panel listing routes quick-buy would refuse when conservative route projection was switched off.
- The setting's hint now says which half of it changes the display and which half changes behavior.
- Mapped all five market filters against all four market scans, which is how the two fixes above were found.

## 1.3.16

- Fixed "Hold cargo for the best market" checking the price once and then letting the whole stack go, when selling into a market is exactly what pushes its price down.
- Fixed the food reserve letting an item skip the rest of the sell rules.
- The ledger popup now explains why Profit is not simply sell price minus buy price times quantity.
- Checked the panel layout against the code that fills it, and every price lookup for buy or sell direction.

## 1.3.15

- Fixed the food reserve keeping whichever food came first, so it held the wine and sold the grain; it now keeps the cheapest food.
- Livestock is now reserved last, behind every sack of grain.
- Fixed a failure in a per-frame path writing a full stack trace every time, which could have filled the log at hundreds a second.
- "Minimum stock for buy suggestions" now says it only applies in live-price mode.
- Checked all eighteen numeric defaults against their allowed ranges, and the panel legend against its length limit.

## 1.3.14

- Fixed simulation mode ignoring the merchant's gold, so a dry run happily reported selling more than a town could pay for.
- Fixed the trade screen coloring trade goods only, leaving horses and livestock grey even though the tooltip could price them.
- Fixed the panel never proposing a livestock route even though quick-buy would buy livestock.
- Removed the ledger popup's tip telling you to press T for the panel, since that popup only appears when the panel is unavailable.
- Removed a second catch in the panel teardown that nothing could reach.
- Confirmed town and village gold come from one place, the trade-good list is built fresh each time, and the map button sits clear of the map's own mouse area.

## 1.3.13

- Fixed quick-buy spending the per-visit budget in shelf order, so a 12% margin listed first beat a 60% one further down.
- Fixed the cost basis changing halfway through selling one stack, so your margin was silently two different margins in one sale.
- Fixed pressing Done in the settings screen being able to switch your automation off.
- The panel now counts caravan traffic by walking the party list once instead of once per town shown.
- Entering any settlement anywhere no longer costs work for parties that are not yours.

## 1.3.12

- Fixed towns under siege and villages being raided being ranked as ordinary markets and proposed as destinations.
- A village that has been raided and is rebuilding is deliberately still listed, because thin shelves and high food prices are an opportunity rather than a closed market.
- Fixed quick-buy buying items the selling side had just been taught to refuse.

## 1.3.11

- Fixed quick-sell being able to sell a quest item, which silently failed the quest later.
- Fixed the same for items the game marks as not merchandise, such as tournament prizes and banners.
- Fixed the panel not releasing its mouse and keyboard grab when the screen changed while it was open.
- Re-checked settings and translation coverage: 45 settings to 45 controls, and 23 texts to 23 ids. Ensured nothing orphaned either way.

## 1.3.10

- Fixed a settings combination that switched your automation back off on the next load, with nothing said.
- Fixed "Hold cargo for the best market" ignoring livestock.
- Fixed the ledger popup naming the text in the hotkey setting rather than the key the panel actually listens for.
- Fixed the hotkey setting accepting comma-separated text like "T,Y", which left the panel with no working hotkey at all.
- Confirmed the panel's layer name, the inventory lock key and all 23 translated texts are what the code expects.

## 1.3.9

- Fixed observed mode rebuilding its market rankings on every tooltip and every inventory row, instead of once per game hour.
- Entering a market now clears the cached rankings, so the panel and tooltips no longer serve pre-entry answers for the rest of the hour.
- Fixed the panel proposing routes for locked items that quick-buy would refuse to buy.
- The one-line trade summary now names the six goods worth the most denars, not whichever came first in the roster.
- The startup log no longer claims MCM was registered when the companion file was from a different version.
- Fixed an empty or truncated item-list setting throwing an error and silently stopping a whole trade pass.
- Re-measured the panel layout and wrote the numbers down.

## 1.3.8

- Fixed "Keep food (days of supply)" not covering livestock, so a herder could enter a town with auto-sell on and leave with the whole herd sold and nothing to eat.
- Fixed quick-buy buying goods you had locked in the inventory.
- Fixed "Suppress vanilla trade-rumor lines" still hiding vanilla's hints when TradeLord had no prices to show in their place.
- Quick-buy now stops scanning once the budget is spent, instead of pricing the rest of the shelf first.
- Re-checked that every game method the mod hooks into is still where it expects.

## 1.3.7

- Changed the cargo marker's default travel ceiling from 1 day to 1.5 days, because 1 day rarely had anything to point at.

## 1.3.6

- Fixed quick-buy buying goods quick-sell would never sell, such as smithing materials, which then sat in the cargo forever.
- Fixed "Suppress vanilla trade-rumor lines" hiding vanilla's prices even when TradeLord's own tooltip section was switched off, leaving no price information at all.
- Fixed simulation mode writing its per-item buy caps back to the visit record.
- The cargo marker now values only what quick-sell would actually put on the counter, not locked items, never-sell entries and reserved food.
- Fixed the panel not giving up input focus when its layer was released, which could break input after an encounter on the map.
- A trade pass now reports itself in one line instead of eight, by silencing the game's per-chunk messages for the length of the pass.
- Changed the default: "Protect smithing materials" is now off, since it governs buying as well as selling.

## 1.3.5

- Fixed battle loot, companion transfers and stash moves being recorded as purchases at full market price, which then made the profit margin refuse perfectly good sales.
- Fixed the per-visit trade counters carrying over into the next campaign you loaded in the same session.
- Entering a market now captures prices once per game hour instead of three times per entry.
- The market scan now builds its list of reachable markets once per game hour and shares it, instead of rebuilding it for every item and direction.
- Fixed a sale that paid no gold still handing the goods over and draining the cost record.
- Fixed turning off "Detailed trade summary" also stopping the full item list reaching the log, which its own description promises.
- The log now starts fresh each launch and writes the version banner first, instead of growing forever.
- The panel no longer proposes routes for items on your never-sell list.
- A single failed panel setup no longer disables the panel for the rest of the session.
- The town-to-town distance cache stops building a throwaway text key on every lookup.
- Fixed the panel hotkey firing while the escape menu was open.
- The cargo marker now refreshes on every settlement entry, not only when entry automation is switched on.
- The travel distance caches are now cleared with the campaign, like everything else.
- "Quick-sell option in town menu" now explains that it also hides the quick-trade entry.
- Checked that every setting is reachable from the settings screen and starts inside its allowed range.

## 1.3.4

- Fixed the two per-item buying caps resetting on every quick-buy click instead of lasting the whole visit, so clicking twice bought twice the cap.
- Simulation mode now previews against whatever the caps have left and puts them back where it found them.
- The village last-unit rule now measures against the stock left on this pass rather than the opening shelf count.

## 1.3.3

- Fixed the panel's legend line being far too long for its row, so half of it was invisible, including the note explaining the Profit column.
- Fixed the panel's "no routes" message being cut off, so the advice a new player needs was hidden.

## 1.3.2

- Fixed smithing protection silently switching off in the second campaign of a session.
- Fixed the food reserve being kept per food type instead of in total, which held roughly five times as much food as the setting said.
- Fixed "Exclude hostile markets" doing nothing when turned off, because trading with hostile towns was blocked either way.
- Fixed the scan radius being applied in live-price mode and ignored in observed mode.
- Fixed auto-trade reading as ON when quick-buy was off, which left the town menu with nothing in it at all.
- Fixed profit and Trade XP on livestock you had bought being reported at the full sale price instead of the margin.
- Fixed a purchase that moved no gold still being written to the cost record, which permanently dragged that item's cost basis toward zero.
- Fixed the panel only being able to unpin the most recent town it pinned, stranding every earlier pin on the map.
- Fixed the daily cargo marker removing a marker you had pinned by hand.
- Fixed simulation mode ignoring the rule that stops it selling a good and buying it straight back at the same counter.
- Fixed simulation mode ignoring carry weight, so it reported buying more than the party could hold.
- Observed mode now looks settlements up directly instead of scanning every settlement on the map for every tooltip row.
- The price cache stops building a throwaway text key on every lookup.
- Leaving a campaign no longer keeps the previous campaign's settlements, parties and prices in memory.
- The map panel is rebuilt whenever the map screen is replaced, and logs itself once per session instead of once per inventory screen you close.
- The never-sell, always-sell and never-buy lists now match item ids regardless of capitalization.
- Internal tidying: the duplicated market-settling check became one, and an unused connector class was removed.

## 1.3.1

- Automatic passes on entering a town are silent when nothing traded, instead of reporting "bought 0 items for 0 denars".
- Observed mode now respects the trade-with-villages setting exactly as live mode does.
- The panel hotkey is read once instead of being re-read every frame.
- Removed dead code and all source comments.
- Releases now publish automatically when a push carries a version that has no release yet.
- A push for a version that is already released now skips publishing instead of failing the build.

## 1.3.0

- Grain is no longer bought by default, because it is heavy and low margin and clogged the cargo; a general never-buy list was added alongside it.
- Livestock now trades both ways by default, with buying capped by the game's own herd math so a purchase can never push the party into the herd speed penalty.
- Changed the defaults to a trader-ready set: quick-buy, auto-sell, auto-buy and the cargo marker all on, gold reserve 100.
- The cargo map marker now ignores towns beyond a travel ceiling, 1 day by default.
- Quick-buy never takes a village's last unit of anything, so the vanilla "buy products" option always has stock to show.
- The map button now reads "TradeLord".

## 1.2.0

- Fixed the map button holding your mouse on its invisible layer, which gave a permanent "forbidden" cursor and a stuttering right-drag camera on the world map.
- Trade summaries now name the goods, for example "Sold 8 Olives for 240 denars".
- Added auto-buy on entry, and grouped auto-sell, auto-buy and auto-trade together in the settings.
- Routes must now clear your minimum profit margin after the resale safety factor, so razor-thin routes are no longer listed.

## 1.1.0

- Added quick-trade, one menu option that sells and then buys in a single pass.
- Added livestock selling, off by default; mounts and pack animals are never sold by policy.
- Added a TradeLord button on the right edge of the campaign map that opens the ledger panel.
- Added a workshop tracker, a caravan-traffic column and a data-age column to the panel.
- Observed mode now records the horses and livestock a shelf actually holds, so animal tooltips work without live prices.
- Fixed three things from the 1.0 review: settings changes now clear the price cache immediately, the panel unpins only markers it placed itself, and a number typed as the panel hotkey no longer maps to an arbitrary key.

## 1.0.0

- First release: a price ledger saved with your campaign, best buy and sell prices in item tooltips, a route panel on the campaign map, and quick-sell and quick-buy from the town and village menus.

---

The versions below are the earlier test builds, from the two repositories this one replaced. They were all numbered 1.0 at the time; the numbers here were assigned afterwards to put them in order.

## 0.916Alpha

- The release zip now unpacks straight into the game folder, the layout mod managers expect.
- Updated the install instructions in the README and the release notes to match.

## 0.915Alpha

- Clicking a town in the ledger panel now pins a map marker on it, and clicking again removes it.
- The automatic best-sell-town marker is now off by default, because it hopped between towns with no visible reason.
- Lowered the default buy cap per item from 50 to 32.
- Capped the default spend per visit at 1000 denars, instead of leaving it unlimited.
- Documented how versions are numbered and why releases are never replaced.

## 0.914Alpha

- The version number now lives in one place, and everything else reads it from there.
- Releases became permanent, and the build refuses to publish over one that already exists.
- Added the first changelog, with an entry for every release so far.

## 0.913Alpha

- Travel days now follow real routes instead of straight lines, so a town across a sea no longer reads as a short trip.
- Travel results are cached, so tooltips stay quick to hover.

## 0.912Alpha

- Added travel ceilings, so a market more than three days away, or a village more than one, is no longer offered.
- Both ceilings can be turned off or changed in the settings.
- When two markets tie on price, the nearer one now ranks first.
- Empty lists now name the ceiling that emptied them.
- Added the store description and a feature comparison page alongside the README.
- Brought the README in line with those two pages.

## 0.911Alpha

- Corrected the documentation's account of the research behind the mod.
- Narrowed the "zero hardcoded" claim to price data.
- Rewrote the documentation to say plainly what was researched and what was written from scratch.
- Brought the rest of the documentation back in line with what the mod actually does.
- Marked the mod field-proven in the README, after the first live playthroughs.
- Verdict lines now use a star the game's font can draw.

## 0.910Alpha

- Removed the compass arrows from tooltips because the game's font could not draw them; rows now show travel days and price only.
- Documented a base-game problem that is not TradeLord's and was reproduced with TradeLord disabled: if you enable or remove any module mid-campaign, the MCM stack included, the launcher and main-menu Continue button can hang the game on a save that was made under the old mod list. The save itself is not damaged. Load it once from the Saved Games list instead, click through the module-mismatch warning the game shows, and save again; Continue is safe from that point on.

## 0.909Alpha

- Fixed every colour in the panel being ignored, because the wrong property name was used throughout.

## 0.908Alpha

- Fixed the panel rendering upside down, with the title at the bottom and the worst routes first.
- Column headers are now gold and underlined, so they never read as an entry.
- The profit-per-day cell now carries a green-to-orange gradient across the visible list.
- Renamed the panel's "Best" column to "Profit per day".

## 0.907Alpha

- Route proposals are now capped by the sell town's own gold, so a bulk run no longer promises profit the buyer cannot pay.

## 0.906Alpha

- Removed the diagnostic log lines now that a live game has answered every open question.
- The mod now refuses to touch MCM unless the whole stack is installed, and names the missing piece.
- "Consult the TradeLord ledger" in the town menu now opens the map panel.

## 0.905Alpha

- Added the ledger as a panel on the campaign map, opened by hotkey, with sortable routes and clickable towns.
- Item tooltips now list the top five buy and sell markets, with profit, stock and travel days.
- The route popup now says its day counts are measured from where your party is.
- The panel's artwork now ships inside the release zip.

## 0.904Alpha

- The first successful sale and buy now write a confirmation line to the log.
- The XP line now says its number is profit in denars, not raw XP.

## 0.903Alpha

- Fixed the route report saying there were no profitable routes to anyone whose gold was below the reserve.
- Tooltips now use the game's own coin icon instead of a bare "d".
- Tooltips now spell out stock, denars and days.
- The startup log now reads the game version from the game itself, instead of trusting the number stamped on its files.
- Confirmed that a sale can never be recorded or paid Trade XP twice.

## 0.902Alpha

- Fixed the mod refusing to load at all on any install without MCM.
- The settings menu now lives in its own file, loaded only when MCM is there.
- Removed the launcher's version warning.
- Matched Harmony to the version the game's own Harmony module ships.

## 0.901Alpha

- The mod now writes plain answers to its log for the questions only a live game can settle.
- Added a guard that stops a trade if gold ever moves the wrong way.

## 0.900Alpha

- First test build: a price ledger saved with your campaign, best buy and sell prices in item tooltips, and quick-sell and quick-buy from the town and village menus.
- Added the build that compiles the mod and packs the zip on every push, and publishes a release on demand.
- Fixed a formatting mistake that broke the release notes step in that build.
