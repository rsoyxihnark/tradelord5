# Commit history

Every commit this repository held before the changelog and the feature list were rewritten, newest first, ending at 975806af4c0a31c8c2a164e8950df572d6bb7705. The message under each heading is the one that commit was made with, which for a version is the note that release published. Every one of them was committed by the repository owner, so no author line is kept.

## [no release] Working rules: a changelog entry is written in the owner's own words

975806af4c0a31c8c2a164e8950df572d6bb7705, 2026-09-18

- The working rules now carry the owner's own wording for a changelog entry, six lines of it word for word, and say to start a fix with Fixed, name what was going wrong and stop.
- A check holds those six lines in the rules file so a later session cannot quietly drop them.

## [1.83.3] The map marker: the market marked is the one that earns the most a day

75c4166c07b0b30b8c4e0ad0ba1361c4947bd56f, 2026-09-18

- The market marked on your map is now the one that would earn the most a day for the cargo you carry rather than the one paying the most in all, so a town a week away no longer wins for paying a little more than one close by.
- TradeLord.log says how much a day the marked market would earn and how much the runner-up would, instead of the gold alone.

## [1.83.2] Buying: the larder is filled after the trading buy, not before it

f641f76438235ec9b3e51f8fcb72e9856006ef93, 2026-09-18

- Food is restocked after the trading buy rather than before it, so your gold and your cargo room go to the goods you came to trade first and the larder takes what is left.

## [1.83.1] Buying: a purchase is judged against the market that earns it back fastest

58281a253c1ad1a2dbaca8be310221138bc3537c, 2026-09-18

- The market a purchase is judged against is now the one that would earn the money back fastest rather than the one paying the most, so a buyer two days further on no longer beats a nearer one paying almost as much.

## [1.83.0] Buying: the shelf is worked through by what each good would really make you

4f1af19d31f47698165a2a68dc480f69a5fdcf96, 2026-09-18

- What TradeLord buys first on a shelf is now whichever good would make you the most for the amount you could really take of it, counting what the shop has, what is left in your purse and what still fits in your cargo, instead of the fattest percentage on a single unit.
- What to buy first is gone from the settings screen, because there is one rule now and nothing left to pick between.
- This version puts every setting back to the value TradeLord ships with, once, and lists what you had set in TradeLord.log so you can put it back.

## [1.82.3] Buying: what to buy first is a choice of two, and the ledger's own order decides

48bd31430c80d2f6cd7c3c6c13ec0ddd9dda97b1, 2026-09-18

- Buy what the ledger sent you for first is now a choice called What to buy first, between What the ledger scores highest and The biggest profit margin, and where the ledger lists more than one route out of a market the one it scores highest is bought first.
- This version puts every setting back to the value TradeLord ships with, once, and lists what you had set in TradeLord.log so you can put it back.

## [1.82.2] Settings: the minimum stock value hint says the rule rather than naming two goods

0f01319b9a352567394cd6dacecf967fc3e28382, 2026-09-18

- The hint under Minimum stock value for buy suggestions now says that the dearer a good is, the fewer of it a market need hold, rather than naming a couple of goods as examples and leaving every other one out.

## [1.82.1] The ledger: a shelf is counted at what its goods are worth, not what the shop asks

508d53f5fd6c3f18eda94a2fb577ebc1443d78cb, 2026-09-18

- Minimum stock value for buy suggestions now counts a shelf at the good's own worth rather than at what the shop is charging for it, so a shop down to its last unit can no longer pass by asking a high price, and the goods it lets through are the costly ones it was meant for.

## [1.82.0] The ledger: rare goods are suggested and what you went for is bought first

8c8aa78660785c8d2f5c113325b1a164aca0dd36, 2026-09-18

- Rare and costly goods such as jewelry, tools and oil are now offered as routes from a market that holds only a few of them, because the new Minimum stock value for buy suggestions counts what is on a shelf in denars, so a shelf worth 500 or more passes even where Minimum stock for buy suggestions asks for ten units.
- Walk into a market the ledger routes a good out of and TradeLord now buys that good first, so your gold and your cargo room go to the trade you travelled for rather than to a cheaper good with a fatter margin, and Buy what the ledger sent you for first turns that off.
- An item tooltip with no best buy prices now names the denars' worth no market in reach could muster as well as the number of units, and points at both settings that ask for them.

## [1.81.13] Tooltips: the line about no market having enough is written a clause to a line

66a5396381710c0f574008c36fb4e0cb9e99349a, 2026-09-17

- The line an item tooltip shows when no market in reach holds enough to suggest is now written a clause to a line and says it in fewer words, so the tooltip no longer stretches across the screen to fit it.

## [1.81.12] The map marker: a town with no road to it is never marked or offered

ad5fcd979bdf79a2add83cb30b8f5335eac5a89e, 2026-09-17

- The best market for your cargo is no longer marked on a town the game can find no road to, and no route is offered through one, however high you have set your travel ceilings.
- An item tooltip with no best buy prices now says that no market in reach holds as much as Minimum stock for buy suggestions asks for, instead of leaving the list out with no word why.
- The forecast check in TradeLord.log now says how many units of the good it named were expected and how many denars of every good of that kind, so the two figures no longer read as one.

## [1.81.11] Prices: a caravan is counted as leaving only what the market it calls at will take

08ad26511837362ac679614c3a3a0bc60c03c65a, 2026-09-17

- A caravan heading for a market is now counted as leaving only what that market will really take off it, and only where it pays above what the goods usually fetch, because a caravan that is merely passing through sells nothing and carries its load onward.
- A party of villagers on the road is now counted too, leaving its whole load at the town its village trades through, which is what it does when it arrives.

## [no release] DLLsNeeded: the note file is removed

e4c233026ffcda16e1235d8aafdbad146ae6155c, 2026-09-17

- The note in DLLsNeeded is gone, as asked; the assemblies it described are untouched and still in place.

## [1.81.10] Prices: what a market is forecast to pay is held within reach of what it pays now

e1b4660286c3a608d8d48bf42c6627e58cb28d1a, 2026-09-17

- What a market is forecast to pay by the time you get there is now held within reach of what it pays today, so a town that pays well can no longer be listed at a price you would never take.
- A caravan on the road is now counted as leaving only part of its cargo at the next market it calls at, because the rest rides on with it, so what a market is forecast to hold when you arrive is closer to what you find there.
- A market the game could not find a road to is no longer offered as a place to sell, and shows no travel time beside it, however high you have set your travel ceilings.

## [1.81.9] Tooltips: the five markets shown are picked after every one has been priced for your arrival

cc3716a6ea6edb3e12c2d5c8ce97624f67cc1331, 2026-09-17

- The best sell and best buy lists in an item tooltip now pick their five markets after every market has been priced for the day you would arrive, so a town the forecast marks down drops out of the list instead of sitting in it at a price you would never take.

## ALL WORK IS DONE, DELETE WhatToDrop.md FILE.

9f0a3cdd74638972e7b3555a58e8f37f7a4a3469, 2026-09-17

ALL WORK IS DONE, DELETE WhatToDrop.md FILE.

## [no release] DLLsNeeded: the War Sails assembly is here, and every menu id is answered

115175e8c32a0a95696b532f527acbfa10c022a8, 2026-09-17

- NavalDLC.dll from the owner's install joins the assemblies in DLLsNeeded, and the note records that all four menu ids the compatibility tool asks about are now answered rather than three.

## [1.81.8] Settings: a file TradeLord could not open is left alone rather than written over

b899a7a41b9617e4510eb970792ed275a0876c4b, 2026-09-17

- A TradeLord.ini that could not be opened is now left exactly as it is, rather than being written over with the settings TradeLord starts up on, so a file locked by something else no longer costs you everything you had set.

## [1.81.7] Villages: an empty village purse is put back, once, so those shops trade again

f0fb659462561d4538e2063f6e1247be47def337, 2026-09-17

- The first time you enter a campaign after this update, TradeLord puts 1000 denars back in every village left with an empty purse, so the villages an older version spent out can be traded with again.
- It says on screen how many village purses it refilled, and TradeLord.log names each village and what went back into it.
- The feature list now says TradeLord puts an empty village purse back, once for a campaign.
- The feature list now counts the switch TradeLord writes into a save alongside the strings and numbers it already listed.

## [1.81.6] Villages: the village shop stays open after TradeLord sells there

a7bee6cd2327987e6622854fc131e9edfa033501, 2026-09-17

- Selling to a village now leaves the last coin in its purse, so Buy products no longer greys out with Village shop is not available right now.
- The map marker and the ledger's routes pass over a village left with nothing but that coin, since there is nothing there for TradeLord to sell.
- The feature list now says a village keeps the last coin in its purse as well as its last of each good.

## [no release] DLLsNeeded: the note says the assemblies are here with permission and stay

fe6286b7d90003054e12052418b4042d50397d2d, 2026-09-17

- The note in DLLsNeeded says plainly that the game assemblies dropped there are permitted and are never to be deleted, so the question is not reopened.

## [no release] Checks: the last three disagreements between the changelog and the release notes are settled

8edbd4b381172521d3f247404352003bedb8e6cf, 2026-09-17

- The 1.14.1, 1.34.0 and 1.35.0 release notes now carry the entries their changelog sections hold, so nothing is left listed as an old disagreement.

## [no release] Checks: the list of old changelog disagreements is read entry by entry

becf38cdea1675353ebbc7fe8618df5c01a47376, 2026-09-17

- The rule that guards the list of old disagreements between the changelog and the release notes now reads each entry as a version, so settling the last of them leaves the rule standing instead of tripping it.

## [no release] The changelog: four old versions now read as the notes they shipped with

73de061928cf597f8b5dd2641fff2a8034d94981, 2026-09-17

- The 1.15.0, 1.16.0, 1.50.0 and 1.50.1 sections now carry the entries those releases were published with, so the changelog and the release notes tell the same story.

## [1.81.5] The ledger: reading what the workshops will make next leaves less behind

be5b91c10588dbc2430544c2d3c068b97f6133c7, 2026-09-17

- TradeLord makes less work for itself each hour as it reads what the workshops will make next, so a campaign left running at speed stays smoother.
- The feature list now says that Write a price trace to the log ships on, rather than off.

## [no release] The feature list: the price trace is described as shipping on

149348bfdacc5f74bc219530a163d035baa12ec9, 2026-09-17

- The feature list now says that Write a price trace to the log ships on, rather than off.

## [1.81.4] The ledger: picking the best markets reads a town's shelves once

934f5587b0b0bfec035752c4e6c427cf9039fb6f, 2026-09-17

- The TradeLord ledger does less work as it picks the best markets for a good: a town's shelves are now read once for the town, rather than searched through again for every good Minimum stock for buy suggestions has it count.

## [1.81.3] The ledger: reading what the workshops will make next costs less

7a2d4e8dd6f315ceb0fac3289df755485074b8f6, 2026-09-17

- The TradeLord ledger does less work as it reads what each town's workshops will make next: a town's shelves are now read once for the town rather than once for every workshop in it.

## [no release] Checks: the test runner is raised to the version NuGet now ships

902cb11a7e6b2fa85a77dba25ca62a303e07b00f, 2026-09-17

- Microsoft.NET.Test.Sdk goes from 18.10.0 to 18.10.1, the version the package manager reports as current, and the 736 tests pass on it.

## [no release] Checks: the settings screen check asks for a control, in one place

0596a1c84bb401a3af2318a59cdd02420fd1eb2f, 2026-09-17

- The check that every setting reaches the settings screen now asks for a control of its own and reads both ways, so a setting with no control on the screen and a control backing no setting each fail it.
- The near duplicate of that check added just before this is gone, along with its claim that a switch or a text box had gone unchecked: every setting was already held to being named on the screen, and what is new is that the naming has to be a control.

## [no release] Checks: a setting TradeLord keeps has to be on the settings screen

320bbef45e35f01ee1c6ce782e16dc72f2f40f94, 2026-09-17

- A source check now holds the settings file and the settings screen to being twins in both directions: every setting TradeLord keeps is on the screen, and the screen carries nothing the file does not keep. A number was already held to this by the check that matches its limits to the screen's slider; a switch or a text box was not, so one could have been added to the file and never shown.

## [no release] Trade skill: the names in the source say which half credits a trade, and the note file is gone

f4ef45050fada4d815ba7adb306ff0444a792b55, 2026-09-17

- The Trade skill path is named after the rule it follows: TradeLord awards the XP for a trade it makes itself, the ledger counts a profit only when the game credited a deal TradeLord laid out, and whether a line earns anything at all is asked as what the game gives XP for.
- OnPurpose.md is removed and the working rules go back to what they said before it, because the source checks already hold every design it listed.
- Two of those checks now say in their own words that the village price and the refusal to fall back are deliberate, so a session reads the design off a check run rather than off a file beside the source.

## [1.81.2] Trade skill: a deal you take on the trade screen is credited once, by the game

ec472f90e7edcfde3748ca4961071ba4f242daca, 2026-09-17

- A deal you take on the trade screen now earns your Trade skill once, from the game, instead of twice.
- The ledger's Trade XP still counts that deal, by listening for what the game credited, so the total covers both the trades TradeLord makes itself and the ones it lays out for you.

## [1.81.1] The trade screen: a deal you take is reported to your game once, not twice

c7186bf4e5a340214104ff74ec25dc82bc3522dc, 2026-09-17

- A deal you take on the trade screen is reported to the rest of your game once rather than twice, because the game already reports it itself.

## [1.81.0] The ledger: the Trade XP TradeLord has earned you, beside the profit

d89c29a01b5ee788763eca4ad4aebeae5541300e, 2026-09-17

- The ledger now shows the Trade XP TradeLord has earned you, beside the profit, and keeps it in your save.
- Selling something that carries a quality, like a Fine sword, no longer earns Trade XP, because selling it yourself at the trade screen never earned any either.
- Anything else in your game that watches what you make from trading now sees what TradeLord trades, the same way it sees what you trade by hand.

## [1.80.14] Prices: TradeLord lets go of a market's prices even when a trade goes wrong

85eba6fdec442e850820d9d2ed2be04a6194c087, 2026-09-17

- A trade that fails to unwind no longer leaves TradeLord's hold on prices in place, which could have moved what a caravan or a villager paid for its own goods.
- A good whose price cannot be read the way the trade screen reads it is now left alone, rather than traded at a price you could not be charged.

## [1.80.13] Settings: everything goes back to what TradeLord ships with, and the price trace ships on

2fdecb67b15e1b5364f80439692bae71f695dbaf, 2026-09-17

- Write a price trace to the log now ships on, so a log sent in carries the four readings for every good you were carrying.
- Every setting goes back to the value TradeLord ships with, once on this update, because the prices it now trades at are not the ones your settings were tuned against. What you had set is listed in TradeLord.log so you can put it back.
- The line that says your settings were put back no longer promises it will not happen again, because while TradeLord's trading is settling an update may do it again.

## [1.80.12] Prices: TradeLord trades at the price you would be charged yourself

4d50be88e8d9653e1776e1a836460603799ef226, 2026-09-17

- TradeLord now buys and sells at the price the trade screen would charge you yourself, so what it earns you is what your own hands would earn.
- A village pays about half for what you sell, the way it always has at the trade screen, so TradeLord goes back to passing over most village sales.
- Trading with a faction you are at war with costs what it costs you by hand, rather than the peacetime price.
- TradeLord trades nothing in a town or a village if it cannot read the price you would be charged, rather than trading at a price you could not get.

## [1.80.11] Recent trades: the list is kept in your save

7d4e4f292ac5f7795015ebf4a4fef6b5f12eae04, 2026-09-16

- Recent trades is now kept in your save, so the last twenty buys and sells are still listed when you load the campaign again instead of starting empty every time.

## [no release] Working rules: a session keeps its chat short and its health checks quiet

65ca630989f756b565e68ef113b0d30af001e012, 2026-09-16

- Added an In chat section: lead with the answer, keep messages plain, run the session's opening checks quietly and say only "starting the work in a healthy repo" when they all come back clean, and write out only what a check actually found.

## [1.80.10] Prices: with Staged Trading on, prices match what the trade screen charges

55199f434d7c61aa989e17b421fdf3d87ceb3a5c, 2026-09-16

- With Staged Trading on, prices now match what the trade screen will charge, so the deal TradeLord lays out is priced the way the screen prices it.

## [no release] DLLsNeeded: the assembly the inventory profit colour lives in

a7019cf6a6f088e73983e08864fe27e092f59125, 2026-09-16

- TaleWorlds.CampaignSystem.ViewModelCollection.dll is in DLLsNeeded, and What is here records that the profit colour values the mod writes are the five the game allows.

## [1.80.9] Prices: a village no longer looks like it pays about half what it does

5e334461b025a7b378ce8d03003c06a702385aac, 2026-09-16

- A village no longer looks like it pays about half what it really does for what you sell, so TradeLord stops passing over village sales worth making.
- Prices in a market whose faction you are at war with no longer carry a penalty that TradeLord's own trade is never charged.
- The feature list now says the price TradeLord shows is the price it pays.

## [no release] DLLsNeeded: what is here records what the dropped assemblies settled

a6c5a7c29ec4ed8d269d8951aaface4de3dd2628, 2026-09-16

- What is here now lists the runtime assumptions the dropped assemblies answered, and corrects where the inventory profit colour actually lives.

## [1.80.8] The forecast score: a market that lost stock is said in words

3fb5f93b2a42556340a6bcfdb464eb64c62c5698, 2026-09-16

- The forecast score in TradeLord.log now says how many units left a market when its stock fell, rather than writing that as a figure below zero.

## [no release] DLLsNeeded: what is here says a reference assembly cannot answer a menu id

b101d6f0e96954653e5e0e31f7a4bb1c70e00326, 2026-09-16

- What is here now warns that the NavalDLC reference assembly on nuget carries no menu id, so only the shipped module file answers for one.

## [1.80.7] What it needs: the mod is supported on 1.4.8.119303 and 1.5.3.122374

43157d5116d318ad67fba1284047a706f5286907, 2026-09-16

- The mod is supported on 1.4.8.119303 and 1.5.3.122374.

## [no release] DLLsNeeded: the four shipped game assemblies the menu-id check needs

d85de36719aa65bd334f355f1861e6d47ba077c2, 2026-09-16

- The four shipped Bannerlord assemblies from build 1.5.3.122374 are in DLLsNeeded, so the menu-id check runs instead of skipping.
- What is here records where each game menu id was really found, and that NavalDLC.dll is still wanted.
- The comment check reads only the file kinds it has a reader for, so a dropped game assembly no longer stops it with a decode error.

## [no release] DLLsNeeded: a folder for the shipped game assemblies the checks cannot read without them

b16d2f03efc4d72b9e6e69c08490b251784c9558, 2026-09-16

- Adds DLLsNeeded, saying which shipped Bannerlord assemblies to drop there and what each one lets the checks answer that reference assemblies cannot.
- Stops the repository ignoring a .dll inside that folder, so a file dropped there is committed rather than passed over.

## [1.80.6] Settings and the ledger: two lines now name the setting they lean on

de4b1bd1f7a791d8236f6f0dde40ed246ba1e870, 2026-09-16

- What this means now says stock alone counts what is on its way to a market while Bulk price simulation is off, since with that off nothing on the way moves a price.
- The Trust a market by what it has paid setting now says it needs Rank routes by confidence, the way TradeLord's other settings name what they need.

## [1.80.5] The ledger: What this means no longer promises a lowered Score that nothing lowers

afceb71db19dea606269df1f5fcccde681d8c1a4, 2026-09-16

- What this means no longer says a market's Score is lowered for paying you less than it promised while Rank routes by confidence is off, because with that off the Score is profit per day and nothing lowers it.

## [no release] Checks: losing a file the repository never deletes is said plainly instead of stopping the run with an error

0b0b61fe508c35be7499171cd4f8b790d6b18c7a, 2026-09-16

- The files this repository never deletes are looked for before anything is read, so removing CHANGELOG.md, CLAUDE.md, the release workflow or either hook now names the missing file instead of ending the run with an error partway through.

## [1.80.4] Recent trades: the line it shows when it is empty says what it really means

bf5bb9d2a8fa5cc836a3bed2a7977010158466d9, 2026-09-16

- Recent trades no longer says nothing has been traded this campaign when it means nothing has been traded since you loaded the game, which is all it ever keeps.

## [no release] Checks: a setting left off the shipped list is caught, and a missing translated line no longer halts the run

a1d1f014df7e7bbfc47ffafc8205a76a3037e119, 2026-09-16

- The check holding every setting that has ever shipped now also refuses a setting that is live in the source and missing from that list, so one can no longer ship, be saved by players, and later be renamed without anything noticing.
- A line missing from one of the language files now fails its own check by name instead of stopping the run partway, so the checks after it still get to run.
- The check on the wording of the grain message asks whether that line is in each language file before it reads what the line says.

## [1.80.3] The ledger: opening it and reading an item tooltip both cost less

d18975ded2a08d16c5bb21d266ac9dfeee3bb445, 2026-09-16

- The TradeLord ledger opens faster: a market is no longer searched from top to bottom for every good it never had on its shelves.
- Item tooltips do less work as you move the mouse over them, because working out whether TradeLord has anything to say about a good no longer builds the lists of best markets twice.

## [no release] Settings file: one line is put back in line with the rest of the file

a3ef4e7fbfe50293affbec781bdfc4448551dea8, 2026-09-16

- One line in the settings file code sat further in than everything around it, and is lined up again.

## [no release] Checks: the route scan's sell price is held to the profit test before the purse is divided by it

2d3ec892646508c66d8722c5c0502bdd41cac118, 2026-09-16

- Added a source check that the opening sell price of a route is put through the profit test before the merchant's purse is divided by it, and that the minimum margin and the resale safety factor keep their floors, since together those are what stop a price of nothing reaching that division.

## [1.80.2] The map: Recent trades can now be scrolled and closed

75eb1eb80eb7b1a2c154f5e9c1c9e54e67af8232, 2026-09-16

- Recent trades opened from the campaign map can now be scrolled and closed.
- Recent trades now closes when you open the game menu over it.

## [1.80.1] Markets: the best ones for a good are picked by today's prices again

9eab2ed485412bf2d9ef238a18988dfe1c34a42d, 2026-09-16

- The best markets for a good are picked again by what each market charges today, the way they were before 1.80.0.
- Restocking your food, buying a haul animal and judging whether a sale clears Minimum profit margin no longer move with the caravans heading for a market you are not going to.

## [1.80.0] Markets: the best ones for a good are picked by what they will charge when you could get there

cf172ac447020e8c91a95f2c8a33af6ee6862d4d, 2026-09-16

- The best markets for a good, in the item tooltips, on the map marker, in the ledger and in what TradeLord buys to sell on, are now picked by what each market will charge by the time you could get there, so one about to be cleared out can beat one that looks dearer today.
- None of that moves unless Count what is on its way to a market is on; with it off every list is picked exactly as it was before.

## [1.79.3] The ledger: a route is priced and judged on what its markets will charge when you get there

9d842723171d3ca346ce4d48b0feea285727dc2a, 2026-09-16

- The two Price columns in the TradeLord ledger now show what you would really pay and be paid once you get there, so they agree with the Profit and Qty beside them.
- A route is no longer left out of the ledger, or its Qty cut short, because of what its markets charge while you are still standing somewhere else.

## [1.79.2] The ledger: a route that should have led the list is no longer dropped from it

b16f102d67789112c2f6d2e7b2548cf1eda0d407, 2026-09-16

- The TradeLord ledger no longer drops a route that should have led the list, which could happen when Count what is on its way to a market raised what the selling market would pay.

## [1.79.1] The ledger: a workshop with no name no longer stops the panel drawing, and the map marker stops repeating itself in the log

2091532f6f3389bcb7184ccadeadc069cf7b42de, 2026-09-16

- The TradeLord ledger no longer stops drawing, and Buy Workshops Remotely no longer fails to open, when a workshop, the town it sits in or its owner has no name of its own, and shows its id instead.
- TradeLord.log no longer repeats the map marker line over and over when the market it marks is one you had already pinned from the ledger.

## [1.79.0] The map: Recent trades opens straight from the campaign map, and the workshop button is renamed

93dd4355b9219bcc6870f43dcacde3810173a4fa, 2026-09-16

- Recent trades now opens from the campaign map, on a button under the TradeLord one, so the last twenty buys and sells are there without opening the ledger; its button has left the row along the bottom of the ledger.
- The Workshops for sale button is now called Buy Workshops Remotely, and so is the window that asks you to confirm a purchase.
- TradeLord button on the map screen now says it puts two buttons on the map rather than one.

## [1.78.5] The ledger: a ? beside the title opens What this means, and the button along the bottom is gone

25a5cdbe9f8a42d96611abd967e7336ea9756273, 2026-09-16

- The What this means button has left the row along the bottom of the TradeLord ledger; a ? beside the title opens the same explanation.

## [1.78.4] Panel: a route whose good or market has no name no longer stops the panel drawing

da8baa0a592c470f4013017ca0670a79292973e7, 2026-09-16

- The TradeLord panel no longer stops drawing when a good or a market on a route has no name of its own, and shows its id instead.
- Recent trades, and the line TradeLord.log writes when the trade screen will not open, name a market or a party by its id where it has no name.

## [no release] Checks: the module file and the names the source reads by string are held to what the project really is

be97264fd222c8f7bfdb79f727a03c1215f4030b, 2026-09-16

- The module file's entry class, both dll names and the folder the mod installs into are now held against what the two projects build and what the release workflow assembles, so renaming any of them in one place alone fails the build.
- Every game type the source reads by name is now held against the game-version check, the settings screen it loads by name is held against the class and method the MCM project really ships, and reading a property by a name nothing holds is refused.

## [1.78.3] Settings file: an item list typed across lines can no longer knock out another setting

9095905bfd9845ad2adc6395e6fc88e9fbd9c8db, 2026-09-16

- A Never sell, Always sell, Never buy or Always buy list saved with a line break in it is now kept on one line in the settings file, so it can no longer lose the goods after the break or land on top of a different setting.

## [1.78.2] Settings: Silence trade messages and Mark a market rising or falling now say what they really do

e11441b064b52626b187c9e4208001e508965b95, 2026-09-16

- Silence trade messages now names the three warnings it still puts on screen, rather than saying it shows nothing at all: a full cargo, a purse below your Gold reserve, and an item list entry that matches no good.
- Mark a market rising or falling now says it needs Live world prices off, since that is when TradeLord records the prices it compares, and with them on it marks nothing.
- The mod description now says the town purse a route is held to is read live, so with Live world prices off a route is planned on the market's stock alone.

## [1.78.1] Trust a market by what it has paid: five walk-ins now means five real visits

0bdc863254f62db4a1b70330406855e211c1e859, 2026-09-15

- Trust a market by what it has paid now counts one walk-in at a market rather than every price it checked there, so the five walk-ins it waits for are five real visits; before this a single arrival at a market the panel had promised several goods at was enough to start lowering that market's score.
- The line under the routes, and TradeLord.log, now say how many prices the panel has checked rather than calling them arrivals, which is what that number has always counted.
- TradeLord.log now says, as you walk into a market, how that market's own record stands and that it is what lowers the score of a route selling there.
- A message raised with nothing in it while TradeLord was moving a good could stop the game showing messages, and no longer can.

## [1.78.0] The ledger: a market that has paid you less than it promised falls down the list

c89187589cb54973a0fd07052efc332cc1666660, 2026-09-15

- The TradeLord ledger now learns from what a market has really paid you: a market that has paid less than the panel promised is scored lower, so routes selling there fall down the list.
- It keeps that record for each market on its own, written into your save, and counts it only once you have walked into that market five times, so one bad arrival never moves the list.
- It can never move a route's Score by more than a quarter, so a market that has disappointed you is pushed down rather than buried.
- Trust a market by what it has paid, in the Knowledge settings, turns it off; it ships on.
- What this means now says when a market's own record is lowering a route's Score.

## [1.77.2] Map marker: riding across Calradia costs far less

d6fe5061c1d404e7dc4dbf699eb02d0d14200858, 2026-09-15

- Auto-mark best sell market on map now works through the richest markets first and stops the moment none of the rest could outpay the one it has found, and it stops pricing a town once that town's own purse is all it could pay, so riding across Calradia costs far less than it did.
- A price the map marker read for a market now keeps for a few hours rather than a single one, so it asks each market its price far less often; the town it points you at can be picked on prices a little older than the ones you walk in on.
- The line TradeLord.log writes about the map marker is now worked out only when the marker actually moves, and it says when a town's own purse is all the cargo could fetch there rather than what the goods are worth.
- Buy to fill the ships now asks the game what your fleet is carrying once for each time your cargo actually moves, instead of once for every single unit it buys.
- TradeLord.log now names the messages it held back while the game was moving a good, rather than only counting them, so a message of another mod's that goes missing during a trade can be found.

## [no release] Trading: the herd, the map marker, the on-screen messages and the road meetings each move to a file of their own

5b917448d73a60d5498d64d8feb2a9618b2ba772, 2026-09-15

- Moves the herd reading, the map marker, the queue of on-screen messages and the caravan and bandit meetings out of Trading.cs into Drove.cs, Marker.cs, Notices.cs and Encounters.cs, changing nothing a player sees or gets.
- Every cache now asks one tested rule whether the hour or a setting has moved, and nothing else in the source may read that setting counter, so a cache added later cannot forget to check it.

## [1.77.1] Map marker: walking into a market no longer prices every other market again

f480ed50dc69646da3de26ac25906caf6adf92b6, 2026-09-15

- Auto-mark best sell market on map no longer asks every market on the map its price all over again each time you walk into a settlement or out of it, since stepping through a gate cannot move what another town pays.
- The feature list now names the two limits TradeLord ships with that it had left unsaid: Buy cap per item stops it at 32 units of one good a visit, and Max spend per visit stops it at 1000 denars in one town.

## [no release] Settings check: a file put back to what TradeLord ships with must be written out again

2634718e02cba03d527163dd85df476f6f244e5d, 2026-09-15

- The rule holding the one-time settings reset now also reads the line that writes the file back out carrying this version's shape, so a reset that stopped doing that and put every setting back on every launch would be refused rather than passed.

## [1.77.0] Settings: every setting goes back to the value TradeLord ships with, once

bfe4d687885fc29dd2ad08599522c3b82d08e8ad, 2026-09-15

- Every setting goes back to the value TradeLord ships with, once, so the values this version ships with are the ones you are running: anything you had set is listed in TradeLord.log first so you can put it back, and it happens only this once.
- Your never-sell, always-sell, never-buy and always-buy lists are emptied by that same reset, so write them in again if you were using them.

## [1.76.9] Saves: what you paid for your goods survives a save written by a newer TradeLord

8f3e6a93070e45a028e0842de33f4dc45f7e9a2c, 2026-09-15

- What you paid for your goods, written into a save by a newer TradeLord than the one you are running, is now read as far as this version understands it rather than dropped, the way the prices it has recorded already were.

## [1.76.8] The log: what TradeLord writes as it trades reaches the file in one go

ad28cd128e242348c84261af3ae2cf4f2f0b7ed4, 2026-09-15

- The list of what TradeLord moved now reaches TradeLord.log in one go rather than a line at a time, so a pass that trades a lot of goods no longer stutters as it finishes.
- The score the panel keeps of what it promised you, and Score the forecast in the log, now reach the log the same way as you walk into a market.

## [1.76.7] Price trace and the trade screen: walking into a market costs less

c22d476170002c7d6bc5228b7242c476eb9237f9, 2026-09-15

- Write a price trace to the log now ships off, so walking into a market costs nothing until a price looks wrong and you turn it on.
- With it turned on, the price trace no longer stutters as you walk into a market: it reaches TradeLord.log in one go instead of a line at a time for every good you carry.
- Show best buy/sell in tooltips and Color prices by world market now work out their markets once for the market you are standing in rather than once for every good on the screen, so the trade screen opens faster.

## [no release] Settings check: Most workshops you may own is now held by the check that guards saved values

feba50f28026ca57f78e0daeda7212446d9880c5, 2026-09-15

- Most workshops you may own is now on the list of every setting that has ever shipped, so the check that refuses a rename or a retype leaving a saved value stranded now covers it.
- That list is held to a floor of 78 in place of 75, matching what it now holds.

## [1.76.6] Workshops and the map marker: the limit you set stays yours, and TradeLord costs less while you ride

c2832be0307de1484301dfb51ac774f95fa2e51b, 2026-09-15

- Most workshops you may own was raising the limit for every other clan in Calradia sitting at your own clan tier, not just for yours, so those clans could buy workshops without limit too; it now only ever lifts the limit while you are the one buying.
- Auto-mark best sell market on map costs far less while you ride, because TradeLord now reads what you are carrying once an hour and asks each market its price once, rather than working both out again every time your party moves.
- The TradeLord ledger opens faster again, because how long a market's shelf holds a quantity is now worked out once for that market and arrival instead of once for every size of deal it tries.
- Filling in Never sell, Always sell, Never buy or Always buy no longer slows every trading pass down, because the name of each good is now read from the game once and kept.

## [1.76.5] Tooltips: a hover no longer moves the prices TradeLord recorded

acdb4e20812459b0c93620a9b55e84de8a420b4e, 2026-09-15

- Hovering a good in a market no longer changes the best buy and sell prices TradeLord has recorded for it. With Show best buy/sell in tooltips and Count what is on its way to a market both on, every hover wrote what that good will fetch once the caravans land back over the recorded prices themselves, and did it again on the next hover, so the towns the tooltip named drifted further from what they really pay the longer you looked at them.
- Those drifting prices are the same ones Color prices by world market colours your inventory by, Hold cargo for the best market measures this town against, and TradeLord itself buys and sells by, so a hover could quietly change what it was willing to pay and what it held back for a better town.
- The TradeLord ledger opens faster, and faster again with Live world prices or Bulk price simulation turned off.

## [1.76.4] Trade screen and workshops: TradeLord no longer takes a trade on trust

b40201a4f1082ecf4bc1088509ca0c007ebf2897, 2026-09-15

- TradeLord now knows the trade screen it laid a deal out on has closed by asking whether that same screen is still the one open, rather than waiting for the game to report no screen at all. If that report had ever not come, TradeLord would have gone on claiming every trade you made by hand afterwards as its own.
- Buying a workshop now checks that the gold actually left your purse. Where the game hands the workshop over without taking anything, TradeLord pays the seller itself, so a workshop is never free; where the game did take the gold, TradeLord never takes it a second time.
- TradeLord.log now says what a workshop cost you and what the game actually took for it.

## [1.76.4] Trade screen and workshops: TradeLord no longer takes a trade on trust

22d07824dfd4b3023165fc5a4b2edbfbf582cf97, 2026-09-15

- TradeLord now knows the trade screen it laid a deal out on has closed by asking whether that same screen is still the one open, rather than waiting for the game to report no screen at all. If that report had ever not come, TradeLord would have gone on claiming every trade you made by hand afterwards as its own.
- Buying a workshop now checks that the gold actually left your purse. Where the game hands the workshop over without taking anything, TradeLord pays the seller itself, so a workshop is never free; where the game did take the gold, TradeLord never takes it a second time.
- TradeLord.log now says what a workshop cost you and what the game actually took for it.

## [1.76.3] Cost basis: a good traded by hand is written down in units again, not in the gold it fetched

bd46d4b30cc968c5bdfba34c2769f7239f972849, 2026-09-15

- Fixed the same fault as 1.76.2 in a second, older place: a good you traded by hand was written into TradeLord's record of what you paid using the gold that line fetched in place of how many of it moved, so selling 29 iron for 1742 denars wiped the record of what all your iron had cost.
- With that record gone, TradeLord no longer knew what your goods had cost you, so Minimum profit margin had nothing to measure against and it could sell a good for less than you paid for it. It now records what actually moved.
- Buying by hand could also be written down as far more of a good than you bought, whenever you were already carrying more of it than the gold that line cost.

## [1.76.2] Staged Trading: the deal you took is no longer read back with prices in place of quantities

74c86c9c04e67c4270369743a60f18886e855f7f, 2026-09-15

- Fixed a serious fault in Staged Trading: the deal you took on the trade screen was read back with each good's price in place of how many of it moved, so a sale of 29 iron was reported as 1742 iron, a purchase of 3 wine as 813 wine, and the gold ran to hundreds of thousands of denars that never left your purse.
- That wrong profit was being fed to your Trade skill and to the TradeLord profit on the ledger, so both were credited with gold you never made. They now take the real figure.
- TradeLord now squares what it read off the trade screen against what your purse actually did on it, and where the two do not agree it says so in TradeLord.log and counts none of it towards your Trade skill or your TradeLord profit.
- Profit from a sale can no longer be reported as more than the sale itself fetched.
- Recent trades dated every row by a running day count that started at the founding of Calradia, so a trade made today read as Day 91,082. Rows now say Today, or how many days ago it was.

## [no release] Hold for a better market: the floor and the test against it moved where a test can reach them

15167fbec9ba756f41cd8b722739b08a46d712cc, 2026-09-15

- Working out the floor a price must clear to be worth selling here, and the test of each unit against it as the lot drains, moved out of the middle of SellThem into TradeRules, with nothing about either one changed.
- The source check that pinned that one line now names the new home and leans on seven tests, one of them a property test over 20,000 lots, so an off-by-one in the floor fails the build instead of passing it.

## [1.76.1] Workshops: the limit you set now only ever lifts your own clan

39db33cfafd5e4cc272e8e06878b8066e4b52770, 2026-09-15

- Most workshops you may own now only ever lifts the limit for your own clan, so no other clan in Calradia can end up owning more workshops because of it.
- Buying a workshop now says so in the confirm box when the price takes you below the gold reserve and wage cover TradeLord holds back, and still lets you buy it.
- The comparison with the other trade mods now names the things TradeLord has gained since it was written: the trades it shows you, the workshops it buys, and the shelf life it ranks a route on.
- The comparison called Staged Trading by the name it had before 1.74.0, and now calls it what the settings screen calls it.

## [1.76.0] Workshops for sale: buy any workshop in Calradia from the ledger, wherever you are

96d03b566cec1e959a826f91dc79023c245a2159, 2026-09-15

- Workshops for sale is a new button below the TradeLord ledger, listing every workshop in Calradia a notable would sell you, with its town, what it makes, who owns it, what it has earned lately and what it would cost, most profitable first.
- You can buy any of those workshops from that window wherever you are standing, without riding to the town or finding the owner, and it asks you to confirm before it spends a denar.
- Most workshops you may own is a new setting under General, at 200: the limit the game puts on how many you may own is lifted out of the box, and a 0 hands the decision back to the game.
- The deal Staged Trading lays out now reports what moved once you press Done, the same way TradeLord reports a trade of its own, and credits the profit to your Trade skill.
- The line that used to run under the routes has left the ledger for a window of its own, on a new What this means button, and now reads one clause to a line.
- Write a price trace to the log and Score the forecast in the log both ship on now, so a price or a forecast that looks wrong is already written down when you come to ask.

## [1.76.0] Workshops: buy any workshop in Calradia from the ledger, wherever you are

0f84327a6c81ab21ebef7df225e1e5b3307780e6, 2026-09-15

- Workshops for sale is a new button below the TradeLord ledger, listing every workshop in Calradia a notable would sell you, with its town, what it makes, who owns it, what it has earned lately and what it would cost, most profitable first.
- You can buy any of those workshops from that window wherever you are standing, without riding to the town or finding the owner, and it asks you to confirm before it spends a denar.
- Most workshops you may own is a new setting under General, at 200: the limit the game puts on how many you may own is lifted out of the box, and a 0 hands the decision back to the game.
- The deal Staged Trading lays out now reports what moved once you press Done, the same way TradeLord reports a trade of its own, and credits the profit to your Trade skill.
- The line that used to run under the routes has left the ledger for a window of its own, on a new What this means button, and now reads one clause to a line.
- Write a price trace to the log and Score the forecast in the log both ship on now, so a price or a forecast that looks wrong is already written down when you come to ask.

## [1.75.1] Log: the map marker now names the market it beat as the next best it priced

d379baaace8932b93adb0585d3101d26bfc1e70f, 2026-09-15

- The line TradeLord.log writes when the map marker moves now names the market it beat as the next best it priced, rather than as the second best on the map, which it never worked out.
- A market whose prices TradeLord could not read says so again in your next campaign, instead of going quiet about it for the rest of the session once it had said it.
- The feature list now says that meeting the same caravan or villagers again keeps the books of what was already traded with them.

## [1.75.0] Map marker: it now counts only what it would really sell in that town

646be532a58d0127847e8adf2a78397caa804f5b, 2026-09-15

- The market marked on your map now counts only the goods it would really sell there, so it no longer sends you to a town that then refuses to sell anything and tells you your cargo is full.
- TradeLord.log now says every time that marker moves, naming the town, what your cargo would fetch there, that town's purse, how far away it is and the market it beat.
- Gold before it buys a haul animal is a new setting under Buying, at 2000 denars: below that TradeLord buys no haul animal however cheap one is, so your early gold goes on goods instead.
- Most it will pay for a haul animal is a new setting under Buying, at 125%: it will now pay up to a quarter more than the cheapest price it has seen for one, where before it would only ever pay the cheapest.

## [1.74.0] Markets: your first market of a campaign now trades like any other

79b30356902465a5795abbf306b89b006c5a08ce, 2026-09-15

- TradeLord now trades at the first market of a campaign like any other, instead of leaving it untraded behind a warning that it trades for you.
- Meeting the same villagers or caravan again no longer buys back what TradeLord has just sold them, and what it spent on them still counts against its caps.
- Staged Trading is the new name of the setting that was called Lay the trade out for you first.
- The trade screen Staged Trading opens now shows the gold the laid out deal comes to in its own running total.
- Once you close that trade screen, TradeLord says what your purse did on it.
- Recent trades now opens in a window of its own, from its own button below the TradeLord ledger.
- The line under the routes in the ledger is now large enough to read and no longer runs off the bottom of the panel.

## [no release] Buy shelf: the order it works through is now held by a test rather than by a line of source

411162d173fa7e11d3392235486bd6a7e57b46b2, 2026-09-15

- The rule putting the best margin first moved out of the middle of WhatToBuy into Picks.BestMarginFirst, with nothing about the order changed.
- The source check that pinned that one line now names the new home and leans on five tests, one of them a property test over 20,000 shelves, so reversing the order fails the build instead of passing it.

## [1.73.0] Routes: how long a shelf lasts now counts towards a route's score

bd377adee43c0965579b2e4816cdb37875e8cbeb, 2026-09-15

- Left now counts towards a route's Conf, so a route whose shelf empties before or soon after you arrive ranks below one that still has the goods waiting for you.
- A route whose shelf the forecast says will hold is no longer marked down for the caravans heading to those towns, because those are the same caravans Left has already counted.
- The line under the routes now says what Left does rather than only what it means.
- Routes are ranked by Left only where Live world prices is on, since that is what works out how long a shelf lasts; with it off, the caravan count still decides as before.

## [1.72.1] Feature list: it now names Recent trades

1b4f4a416c21bc648c4b0f3272d1f5f2834bf000, 2026-09-15

- The feature list now names Recent trades, so the ledger panel's new section is on the mod page with everything else TradeLord does.

## [1.72.0] Ledger panel: it now shows the last trades TradeLord made for you

abd6bc62099532aa00493012c661547fc1e165e0, 2026-09-15

- The ledger panel now ends with Recent trades, the last twenty buys and sells TradeLord made for you, each with the day, the town, what moved and the gold it gained or cost.
- Gold gained reads in green and gold spent in amber, so a buying visit and a selling visit tell themselves apart at a glance.
- Recent trades is kept in memory for the session only and is never written into your save, so it adds nothing to the file and nothing to load.

## [no release] MCM: which line of it the game loaded is worked out where a test can reach it

34c2d5be2f2b4b8ee2b07482ddf0dad9d26b1004, 2026-09-15

- Working out which line of MCM the game has loaded, and the startup line counting the readers that took and the readers that refused, moved into a place the tests can reach, with nothing either one does changed.
- Eighteen tests now cover an MCM name with no number, an odd case, a line this build was not made for, and a tally with nothing applied.

## [no release] Settings file: reading and writing it moved where a test can reach it

b9baad592a06a410bb033e4d848beaeff4249d37, 2026-09-15

- How TradeLord.ini is read and written moved into a place the tests can reach, with nothing about the file itself changed.
- Fourteen tests now cover comment lines, spacing, repeated names, a value carrying an equals sign, and a file stored with carriage returns.

## [no release] Twins: which of the settings file and the settings screen wins moved where a test can reach it

3d925c77bea485fad59017c368adcd67142556fb, 2026-09-15

- The rule deciding whether the settings screen may write over your settings file, and what counts as editing that file by hand, moved out of Config.cs into Twins in Migrate.cs, so tests can hold it to every combination.

## [no release] Map button: where it catches the mouse moved where a test can reach it

350d0234894b1570036f96714e46d75088c000a2, 2026-09-15

- The region the map button reserves the mouse in, and the strip it falls back to when the game cannot measure the button, moved out of Panel.cs into MapButton in Rules.cs, so tests can hold that region to any screen shape.

## [no release] Ranks: which band a route row falls in moved where a test can reach it

7a85fdac6243f9c6909cfc745c52879cdeb5504a, 2026-09-15

- The five bands that shade the ledger panel's rows, and the rank each row is given in the list, moved out of Panel.cs into Ranks in Rules.cs, so a test can prove every row falls in exactly one band.

## [1.71.2] Settling: a campaign the game cannot date no longer leaves every market shut

c5b4e082fec7b129492334e48f53a041bc6acaff, 2026-09-15

- A campaign whose age the game cannot work out no longer leaves every market shut with Economy settling delay set: TradeLord trades as it would on a campaign long past that delay instead.
- The days left of that delay are now never shown as none, and never as more days than the delay you set.
- The feature list now ends with short answers to the questions people ask before installing: whether it is safe on a campaign already running, whether MCM is needed, what it will never sell, whether it changes the economy, and what to do when it trades nothing.

## [no release] Feature list: it now answers what people ask before installing

75974f33ef4682767cd92e27f38870510953a147, 2026-09-15

- The feature list now ends with short answers to the questions people ask before installing: whether it is safe on a campaign already running, whether MCM is needed, what it will never sell, whether it changes the economy, and what to do when it trades nothing.

## [no release] Herding: what the party drives moved where a test can reach it

86681980bea2319ec4466803e40823d457c5c0f9, 2026-09-15

- The rule that a horse one of your men on foot can ride is ridden rather than driven was written out in four places in Trading.cs and tested in none. It is now Herding in Rules.cs, called from all four, with tests of its own.

## [no release] Arrivals: what counts as the same arrival moved where a test can reach it

45071f3c9e9550fc576d039ca46a01ed03b4c979, 2026-09-15

- The three decisions behind "you walked back into the same market" moved out of Trading.cs into Arrivals in Rules.cs, unchanged, so tests can reach them for the first time.

## [1.71.1] Ledger: a long campaign can no longer grow your save without end

26f864cd648b387c3561a1d67f3f02ca21974588, 2026-09-15

- TradeLord now keeps at most 2500 recorded prices, forgetting the oldest first, so a long campaign with Days to keep a price you recorded set to 0 can no longer grow your save without end.
- TradeLord.log now says at every save how many recorded prices and purchase records went into it and how large they are, so a save that has grown can be read rather than guessed at.
- The comparison with the other trade mods is now one short entry a mod, each naming and linking to the mod it weighs, and it no longer credits Trade Advisor with telling you how long a route lasts, which TradeLord now does itself.

## [no release] Comparison: it is now one short entry a mod, each linking to the mod it weighs

ee2cb32338ffd8b0457d308c4374d6fa8e946e84, 2026-09-14

- The comparison with the other trade mods is now one short entry a mod, each naming and linking to the mod it weighs, and it no longer credits Trade Advisor with telling you how long a route lasts, which TradeLord now does itself.

## [no release] Comparison: it no longer credits Trade Advisor with a route's remaining time as something TradeLord lacks

288ead298e67c7e1799184f4adeb705d296192dd, 2026-09-14

- The comparison with the other trade mods no longer says Trade Advisor tells you how long a route lasts and TradeLord does not, now that the Left column does.

## [1.71.0] Ledger panel: it now says how long each route lasts before someone else buys that shelf out

26755248274843973396655ae1d58df9fa029318, 2026-09-14

- The ledger panel has a new Left column saying how long the market you would buy from still holds the amount that route quotes, before the caravans heading there buy it out. It counts what those caravans will spend and what is still landing, and it leaves the column blank when nobody TradeLord can see is coming for that good.
- The panel is a little wider to make room for it.
- The feature list says so too.

## [1.70.0] Trading: it can leave room in your hold, teach your companions and say what you paid

4519b82148c95eb229a125a26ca35d73b3c078a5, 2026-09-14

- Share of the hold TradeLord may fill, a new setting under Buying, now stops it buying once your cargo reaches the share of your capacity you set, so there is room left for what a battle or a quest hands you. It ships at the full hold, so nothing changes until you move it.
- Share of the profit your companions learn from, a new setting under General, now credits every companion riding with you Trade XP worth that share of a trade's profit. It ships at 0, so the XP is yours alone until you ask for it.
- An item tooltip now says what you paid for that good, per unit, once you have bought one, so you can see at a glance whether the market in front of you is beating it.
- The panel no longer quotes more of a good than a market will have when you arrive: the purses the caravans are bringing now come off its shelf as well as off its price.
- The feature list says so too.

## [1.69.3] Log: it now says each day what the price shelf life forgot

cfc231cf44370ec67a96d230c45e48ada14e46ea, 2026-09-14

- TradeLord.log now says each day how many recorded prices it forgot for being older than the days you set, and how many it is still keeping, so you can see what Days to keep a price you recorded is actually doing.

## [1.69.2] Panel: a figure the game cannot put a number to no longer spreads through the panel

0399dce1b5bdc7c17bbb656551fecb64f4103757, 2026-09-14

- A travel time, a price or a workshop's progress that the game cannot put a number to is now read as the safest sensible figure, so a market can never look as though it is next door because its distance would not work out.
- The panel works out how many of a good are worth showing without ever dividing by a price of nothing.
- The feature list now opens with five short lines instead of four and one long paragraph, so what TradeLord is reads at a glance and the detail stays in the list below it.
- The comparison with the other trade mods now says when the nine were read and that anything they have changed since is not in it.

## [no release] Checks: four rules the trading logic must always obey are now run against thousands of random cases

42d2b7d6d1a92d875025bdccbc24f23df4b0da7a, 2026-09-14

- The eight markets kept for a good are now proved to match what a full sort would have picked, a route's confidence is proved never to rise as the news gets worse, another day of food reserve is proved never to hold back less, and a caravan purse split across a market is proved never to hand out more than the purse.

## [no release] Mod page: the opening five lines read at a glance, and the comparison says when it was written

d14fc8d8b16dca9fd44a8561f0774d3370b083bd, 2026-09-14

- The feature list now opens with five short lines instead of four and one long paragraph, so what TradeLord is reads at a glance and the detail stays in the list below it.
- The comparison with the other trade mods now says when the nine were read and that anything they have changed since is not in it.

## [1.69.1] Ledger: a save from a newer TradeLord no longer costs you every recorded price

c8bcca7923b91a5dd7d2cc4856b2cf04b4fea507, 2026-09-14

- A campaign saved by a newer TradeLord than the one you are running now keeps every recorded price that version can still read, instead of losing the lot the moment you go back to an older build.
- When a save does hold a recorded price it cannot read, the log now says how many and why, instead of dropping them without a word.

## [1.69.0] Knowledge: a price you recorded yourself is now forgotten once it is older than the days you set

890bea9b2e5a024a60f7e4f756d67b24f2cf4aa2, 2026-09-14

- Days to keep a price you recorded, a new setting under Knowledge, now forgets a price you wrote down yourself once it is older than the days you set, so a market you have not looked at in a fortnight stops being suggested and leaves your save.
- It is set to 15 days to begin with, and setting it to 0 keeps every price for as long as your campaign lasts, the way it worked before.
- A shelf life you had set under an older TradeLord is read again instead of being thrown away.

## [no release] Checks: a saved price is now held to the eight fields it writes

0c7af59dd820c2de8333d988f7b35b5248c84969, 2026-09-14

- Two tests and a check now hold a saved price to the exact fields it writes, so a ninth cannot be added to every campaign's save without showing up.

## [no release] Checks: the seam lines nothing but a string match was holding are now run by tests

fe3a1ddb0baa59029fbf8524ed09859058837e02, 2026-09-14

- Twelve tests now run what only a text match held before: what each half of a trading pass writes into the books and how the other half reads it, which reason a stalled pass names, and a settings range matching its name whatever the capitalisation.
- These checks now read every test file the build runs, so a new one cannot slip past them.

## [no release] Forecast and promise check: the figures they work out moved where a test can run them

601eb7ba8919e555db0460e5a109ee0035b5a925, 2026-09-14

- What is on the road, what a purse will take off the shelf and how a forecast or a promise held are worked out in two places a test can run, with no change to what TradeLord does.

## [no release] Ledger: which market is best moved where a test can run it

942188abd97488e1679cb7cdb9ab12482ca0a2b0, 2026-09-14

- The rule that decides which market is best, and how far is too far, moved out of Ledger.cs into Ranking.cs, which knows nothing about the game and which the test project compiles.
- Eleven tests now run that rule: the dearest market first when selling and the cheapest when buying, two markets at one price split by the nearer, only ever eight kept, the eight chosen on the straight line but ordered on the real ride, and the tighter of the two travel ceilings holding a village.
- The source checks that read the ranking now read it where it lives, and one of them holds the new file clear of the game.

## [no release] Working rules: which part of the version to raise is judged by what the change does for the player

d7db54bfff6b7e738486f4d19996abaa5e114dd1, 2026-09-14

- The Version section now says that which part to raise is decided by what the change does for the person using the program, never by how much of the source it touched or how much machinery it carried in, and that a setting added to hold back, gate or switch off something already shipped is a correction rather than a feature.

## [1.68.0] Insight: the rising and falling marker is now a switch of its own, and ships off

215e96eda7b3a771ef150f6320dd823d90af5e11, 2026-09-14

- Mark a market rising or falling, a new switch under Insight, now decides whether the tooltip marks which way a market's price has moved, and it ships off, so tooltips read as they did before until you turn it on.
- The feature list says so too.

## [1.67.0] Tooltip: a market whose price has moved since you last looked says which way

633bbf5d645f2d2007a9b1437485fc217ba3ecd1, 2026-09-14

- The item tooltip now marks a market rising or falling when its price has moved 5% or more since the last day you looked there, on the best sell list and the best buy list alike.
- TradeLord now remembers the price it saw on your previous visit to a market as well as the latest one, and a second look on the same day refreshes the price without counting as a new visit.
- A campaign saved before this version keeps every price it had, and starts marking a direction once you have looked at a market on two different days.
- The feature list now says prices are read through each market's own price model, naming the merchant, the way the trade screen asks, rather than calling it the game's own brain.
- The opening line now says the default settings are there to get you earning as fast as possible.
- The feature list says so too.

## [no release] Nexus tool: the page now folds each part of the feature list away behind its heading

b6f1d50308e052fe10da7f726ad68e5f25af4d3e, 2026-09-14

- The mod page now opens with the five lines saying what TradeLord is and the three it needs, and folds each headed run of the feature list into a collapsible section, so seventy eight of its ninety bullet points start out of the way.
- The heading of each folded section stays visible above it, so a reader can open the part they came for.
- What it needs and the comparison of the nine trade mods are left open, since a reader wants those without asking.
- A source check pins the shape: no tick on the summary, one collapsible section for each heading in the feature list, every one of them closed, and nothing folded after the requirements.

## [no release] Feature list: two phrases that said nothing now say what TradeLord does

10c1e70d111c40d117fe709be9b2822cf30ade85, 2026-09-14

- The feature list now says prices are read through each market's own price model, naming the merchant, the way the trade screen asks, rather than calling it the game's own brain.
- The opening line now says the default settings are there to get you earning as fast as possible.
- The tool that writes the Nexus page now falls back to the version that shipped when the changelog has an Unreleased section at the top, rather than refusing to write a page at all.

## [no release] Nexus tool: it now writes the whole mod page, not just the release notes

c8147212f9ff81cf9489b13b5161d913bca187a5, 2026-09-14

- tools/nexus_changelog.py --page now writes the whole Nexus mod page in the markup Nexus reads: the summary, the feature list and what it needs from README.md, the comparison from COMPARISON.md, and the changelog for the version that shipped.
- Every line of that page is read from those three files, so the page cannot drift from the repository and publishing a release stops being an edit in two places.
- The release notes and the version listing the tool already wrote are unchanged, and asking it for both the notes and the page at once is refused.
- Four source checks cover the page: that it carries all four parts, that no line of it is prose the tool invented, that no markdown survives and every tag is closed, and that the two flags are refused against each other.

## [1.66.0] Log: every campaign now opens with one line saying what TradeLord could read

0b1090d9483548094896d68122ae660029066193, 2026-09-14

- TradeLord.log now opens every campaign with one self-check line, saying which of its patches applied and whether it could read the herd penalty, the quest goods, your language file and the game's price model, so a bug report is one line to paste rather than a hunt through the log.
- Anything TradeLord cannot read is now said as your campaign opens rather than the first time it matters, so the herd penalty and your language file report themselves before you reach a market.
- The feature list says so too.

## [no release] Trading: the buying and selling passes moved where a test can run them

238761f5da81e23e9ad8246170b6a5d4a3aff436, 2026-09-14

- The buying and selling passes now ask a market for prices, stock and gold rather than reaching into the game themselves, so a whole pass can be run against a fake market.
- The pass machinery, the cost basis a sale draws down and the tally of what stopped a pass now live in Passes.cs, which the test project compiles alongside the rules.
- Forty five tests run the two passes end to end: the shelf and the order it buys in, the purse, the cargo room, the herd, the per item caps, a village's last unit, the merchant's till, holding out for a better market, and what a dry run books without moving anything.
- The source checks that read the two passes now read them where they live, and the dead helpers the move left behind are gone.

## [1.65.0] Automation: TradeLord can now lay its whole deal on the trade screen for you to approve

d62dab7b135cd0a7dd4b648cf76fe5871bfd63d7, 2026-09-12

- Lay the trade out for you first, a new switch under Automation that ships off: TradeLord opens the trade screen and lays its whole deal on it, everything it would sell on one side and everything it would buy on the other, so you can change what you like and press Done to trade or Cancel to leave it.
- Each unit is laid out at the price that unit really fetches, the same walk the ledger panel quotes, so the total on the screen is the total TradeLord worked out.
- While it is on, nothing is traded as you arrive at a market: a line tells you Trade here now (TradeLord) is what lays the deal out, and a party met on the road still trades as before.
- A deal with nothing in it says so rather than leaving you looking at an empty screen.
- Simulation mode (dry run) still wins where both are on, so a dry run never puts anything on the screen.
- The feature list says so too.

## [1.64.1] Routes: the workshop side of a market's price is read from the game rather than assumed

92457ee3d565f4b8536e871796faa0f52adf3088, 2026-09-12

- What a workshop will make next now lands when the game says that run is due, rather than always a day away, so a market is priced on it at the right time.
- What a workshop will make next is now valued at the good that town actually stocks, rather than the cheapest good of that kind anywhere in Calradia.
- Two routes that reach the same market within the same quarter day are now priced off one walk of that market's prices, so they cannot quote it differently.
- Pricing a route does less work for the same answer: what a market pulls in altogether is added up once an hour rather than once for every good priced against it.

## [1.64.0] Ledger panel: the panel now keeps score of how well its own Sell price holds

d50d7277caaa789e5b21b9dfdc14982e2b6ec1c0, 2026-09-12

- The panel now scores its own promise: every route it shows writes down the Sell price it promised you, and walking into that market near the time it said holds that promise against what the market really pays.
- The line under the routes says how much of the promised Sell price has actually been there and over how many arrivals, and that tally carries on across your campaign.
- With Score the forecast in the log switched on, the log breaks the same thing down by the Conf each route carried, so you can see whether a higher Conf really means a promise that holds.
- A promise you arrive far too late for is dropped rather than scored, so the figure only counts arrivals it can say something about.
- The feature list says so too.

## [1.63.0] Debug: TradeLord can now score its own forecast in the log

0fa355d08472dbfb314ec797cde06bf70cb71383, 2026-09-12

- Score the forecast in the log, a new switch under Debug, writes down what TradeLord expects a market to hold by the time you get there, and then, as you walk in, what it really holds, good by good, with how far off it was.
- It needs Count what is on its way to a market, and it stays off until you turn it on, since it makes the log longer.
- The feature list says so too.

## [1.62.2] Settings: the Russian, Chinese and Turkish notes now name each setting the way the screen names it

a9325758042975ecb686d75dede3a157f4262168, 2026-09-12

- In Russian and Simplified Chinese, the note under Keep gold for days of wages now calls Gold reserve by the name the settings screen gives it.
- In Turkish, Simplified Chinese and Russian, the note under Auto-mark best sell market on map now names the travel ceilings and the ledger panel the way the rest of the mod names them.
- In Russian, the note under Count what is on its way to a market now calls Live world prices by the name the settings screen gives it.

## [no release] Ignore: the Python cache a tool leaves behind is kept out of the repository

7d6caf78e919fe28632df70ecc102ddbcb388077, 2026-09-12

- The last commit carried tools/__pycache__, which Python wrote when the release check imported the changelog reader. It is gone, and __pycache__ is ignored so it cannot come back.

## [no release] Release: the changelog is now held against the releases that actually went out

71101e74805aedf479e38ac8f1f8f3215fe1650b, 2026-09-12

- Nothing compared the changelog with what was published, which is how 1.49.0 and 1.49.1 went missing from it without a single check noticing.
- tools/released.py reads every published release and refuses a build where a published version has no section, a section names a version that never shipped, a section says something different from the notes that went out, a release has no file attached, or a draft was left behind.
- It reads the releases through gh in the workflow and through the public API anywhere else, and says plainly that it was skipped if neither can be reached, rather than passing quietly.
- Running it found seven versions whose changelog section and published notes disagree, from before the check existed. Each is named in the file with what is wrong with it, it reports them on every run, and a new disagreement fails the build.
- A version on that list that stops disagreeing fails the build too, so the list cannot outlive the problem it records.

## [no release] Release: a commit claiming a version that is already out is refused

ef847ca0573b68801077992d7b8b893b996d6206, 2026-09-12

- The publish step treated "this version is already published" as success on a push and exited 0, so a commit that forgot to raise the version built green and shipped nothing, looking exactly like a release that worked.
- It now says for itself that a [no release] commit publishes nothing, and refuses a commit that claims a version already published from another commit, naming the commit that published it and what to do instead.
- A re-run on the very commit that published a version still passes, since the release records that commit as its target.
- The two checks that read the old wording now read the new one, and a new check holds the order of the guards in front of the publish.

## [1.62.1] Ledger: an amount that counts goods still on the road is marked

c12059b69fd8444dc51ff93cc06acf43300aa136, 2026-09-12

- Where a route's amount counts goods still on the road, the ledger panel now marks it Qty!, so an amount larger than what the market holds while you read it is never a surprise.
- The line under the routes says what that mark means, beside the other marks it explains.
- The feature list says so too.

## [1.62.0] Tooltip: a market is now priced as it will be when you get there

fa17d1966658f4baabb1c8e05e0ef8145fa75d2e, 2026-09-12

- The five markets in an item tooltip are now priced as each will be when you get there, counting what the caravans and the workshops will do to it first, so a tooltip and the ledger panel never quote the same market differently.
- Those five are ordered on the prices you will actually be offered, so a market that is about to be picked over no longer sits at the top of the list.
- The explanation under Count what is on its way to a market now says it reaches a tooltip price as well as a route.
- The feature list now says the tooltip prices a market as it will be when you arrive.
- The changelog was missing 1.49.0 and 1.49.1 entirely; both versions are back in it, word for word as they went out.
- The feature list no longer says the ledger panel ranks every profitable route: it ranks the best route it can find for each good, thirty rows of them.
- The feature list now calls the smithing choice Keep the ones you have not learned, the name the settings screen gives it.
- The feature list now says that quiet mode still speaks the notice on your first market and the warning that your cargo is full.
- The feature list now says a route is listed even when your herd is already as large as your men can drive, that a route whose prices could not be walked unit by unit is marked on its confidence figure, and that a dry run is marked a best case.
- The opening summary no longer counts the factors behind a route and now names what is actually weighed.
- The feature list now says that TradeLord tells you once, at the first market of a campaign, that it will trade for you as you arrive, and leaves that market untraded so you can switch it off first.
- The feature list now says that you can tell a caravan it was a good trade once the goods have changed hands, and that the trader answers.
- The feature list now says that the town menu entry falls back to the six best routes written out as text when the ledger panel cannot open.
- The feature list now counts how much of the margin survives unit-by-unit pricing among the things a route's confidence is discounted by, which it had left out.
- The feature list now says the settling delay ships switched off, and says that a trade on the road moves the goods and the gold itself at the price the game quotes off-market, since there is no market there to sell to.

## [no release] Docs: two lost versions restored to the changelog and six claims corrected

b7ad66412a1b7fe34b62d6323493ff72bf36a920, 2026-09-12

- The changelog was missing 1.49.0 and 1.49.1 entirely; both versions are back in it, word for word as they went out.
- The feature list no longer says the ledger panel ranks every profitable route: it ranks the best route it can find for each good, thirty rows of them.
- The feature list now calls the smithing choice Keep the ones you have not learned, the name the settings screen gives it.
- The feature list now says that quiet mode still speaks the notice on your first market and the warning that your cargo is full.
- The feature list now says a route is listed even when your herd is already as large as your men can drive, that a route whose prices could not be walked unit by unit is marked on its confidence figure, and that a dry run is marked a best case.
- The opening summary no longer counts the factors behind a route and now names what is actually weighed.
- The mod comparison now says nine quest types rather than eleven, counts 33 settings in the old AutoTrader's own file rather than 34, and no longer reads as though nine were every trade mod there is.

## [no release] Feature list: five things the list had missing or loosely worded

6af11dba91d47348ca3d8f63699044834e601cfc, 2026-09-12

- The feature list now says that TradeLord tells you once, at the first market of a campaign, that it will trade for you as you arrive, and leaves that market untraded so you can switch it off first.
- The feature list now says that you can tell a caravan it was a good trade once the goods have changed hands, and that the trader answers.
- The feature list now says that the town menu entry falls back to the six best routes written out as text when the ledger panel cannot open.
- The feature list now counts how much of the margin survives unit-by-unit pricing among the things a route's confidence is discounted by, which it had left out.
- The feature list now says the settling delay ships switched off, and says that a trade on the road moves the goods and the gold itself at the price the game quotes off-market, since there is no market there to sell to.

## [no release] Comparison: the note now says TradeLord estimates what a caravan will buy as well

0f93799ff364543c70e0e6474c2e4ce08b0cc65d, 2026-09-12

- COMPARISON.md said Trade Advisor was ahead on predicting what a caravan will buy; TradeLord now estimates that too, so the note says the forecast is in the list of things none of the other nine do, and that the one thing still theirs is the larger market model behind it.

## [1.61.0] Ledger: a route now counts the gold the caravans are bringing to spend at its two markets

f3d99e78593dafe1009e4e5879e5a3a9034a7ab8, 2026-09-12

- A route's price now also counts the gold the caravans on the road are bringing to spend at its two markets, so a market that is about to be picked over prices accordingly instead of looking cheap.
- That purse is spread over the goods that are cheap at the market it is heading for, which is what a trader buys, so the estimate lands on the goods a caravan would actually take off the shelf.
- A purse counts only against the market the caravan is going to, only when it arrives before you would, and never for more gold than the caravan is carrying.
- Count goods on their way to a market is now called Count what is on its way to a market, since it counts the gold coming to spend as well as the goods, and its explanation on the settings screen says so.
- The panel legend now says that prices count what will be bought off a market as well as what will be added to it.
- The feature list now says that a route's price counts the purses the caravans are bringing.

## [no release] Comparison: the note now says which part of the caravan forecast the other panel still has

3d9a7ea039b22aaa4bf1ab87ab83ccf32305b9c5, 2026-09-12

- COMPARISON.md said Trade Advisor was ahead on predicting caravan shopping and workshop output; TradeLord now counts the cargo on the road and what the workshops will make, so the note says the one piece that mod still has, which is what a caravan will buy when it arrives.

## [1.60.0] Ledger: a route is now priced on what the caravans and the workshops will put in a market before you arrive

2de5039efbd01e513d1c45669ab5770f783df19b, 2026-09-12

- A route's price now counts what is still on its way to its two markets: the cargo the caravans on the road will unload, and what that town's workshops will make next, so the panel prices the market you will walk into rather than the one standing there now.
- Goods that would land after you arrive are left out, each end of a route counted against its own travel time.
- The stock shown behind a buy town's confidence now includes the cargo heading there, so a deal that only exists once a caravan unloads is no longer hidden.
- The workshop list on the ledger panel now says what each workshop will make next, beside what it has earned.
- New setting, Count goods on their way to a market, on out of the box, under Knowledge. It follows Live world prices, so turning those off turns this off with them.
- The panel legend now says when prices and stock are counting what is on the way.
- The feature list now says that prices and stock count the caravans on the road and the workshops, and that the workshop list says what each will make next.

## [no release] Comparison: the note no longer names a charge on a sale that the game does not have

11e9895eff6185f1e980e3e13e7537bd19c23c32, 2026-09-12

- COMPARISON.md said a sale made through the game's own action pays the same charge as one you make by hand; the game has no such charge, so the line now says the price and the XP are the game's own.

## [no release] Comparison: a note on how TradeLord stands against the other trade mods

9ae24f86722a39fc45053f97d10c5a1b6a693143, 2026-09-12

- COMPARISON.md is new: a short comparison of TradeLord against the nine other Bannerlord trade mods, written from their own decompiled code.
- Nothing a player downloads changed, so the version, the changelog and the module are untouched.

## [no release] Food reserve: the unused copy of the reserve calculation is gone

b78adabfeb458e8e1011c3a18d2b9cd013adb6c0, 2026-09-11

- TradePolicy.FoodKeep computed the food reserve and nothing called it; KeptBack already does the same two steps before it adds the quest holds, so the duplicate is removed.
- The three source checks that read the removed method now read KeptBack and the pure rule in Rules.cs instead, saying what they said before.

## [no release] Release: the publish now refuses a commit whose subject names the wrong version

c520cd72aa968be1a0aaafec1097d5912c0a6e0e, 2026-09-11

- The step that reads the version out of SubModule.xml now also reads the version out of the commit subject and refuses to publish while the two disagree, or while a commit that is not marked [no release] carries no version in its subject at all.
- A source check holds that gate in place and reads the rule it serves out of the working rules.

## [1.59.0] Map: a village can now carry the best-sell marker

e44f5888380e6c6da2a1c2d4362c8db1f2b50b68, 2026-09-11

- The map marker can now land on a village as well as a town, so a village that pays best for what you are carrying gets the pin, as long as Trade with villages is on.
- A village being raided or rebuilding is never marked, since you could not trade there anyway.
- Auto-mark best sell town on map is now called Auto-mark best sell market on map, and it keeps to the Village travel ceiling when the market it is weighing is a village.
- The feature list now says the map marker can land on a village as well as a town.

## [1.59.0] Map: a village can now carry the best-sell marker

b3414ca652856704e8c8238fc5b2d12baeed61e2, 2026-09-11

- The map marker can now land on a village as well as a town, so a village that pays best for what you are carrying gets the pin, as long as Trade with villages is on.
- A village being raided or rebuilding is never marked, since you could not trade there anyway.
- Auto-mark best sell town on map is now called Auto-mark best sell market on map, and it keeps to the Village travel ceiling when the market it is weighing is a village.

## [1.58.2] Buying: the message about held-back gold now names the settings holding it

8e43a4a451e943e9e3aa8e63eac3c61fcae09a98, 2026-09-11

- When TradeLord will not buy because your gold is held back, the message and the ledger panel now split the figure between Gold reserve and Keep gold for days of wages and name both, instead of calling the whole amount your gold reserve.
- Where only Gold reserve is holding your money back, that message now names Gold reserve by its own name, so you know which setting to lower.
- That message no longer appears when it was Max spend per visit that stopped the buying, since your gold reserve was not what held you back.

## [1.58.1] Trading: the market is still settling notice now shows as you enter a market

9f0e073f84e1e318e240195ab7305338e178a0bf, 2026-09-11

- TradeLord now tells you on screen that the market is still settling as you walk into one, unless Silence trade messages is on.

## [1.58.0] Selling: livestock is no longer food, so a herd trades as ordinary goods

5fb712152cc72a09751fb28ec67771329c31b481, 2026-09-11

- Livestock is no longer counted as food. A herd is not held back towards your days of supply, it is never bought to restock them, and it is bought and sold as ordinary goods with only the herd speed penalty holding it back.
- Keep some of every kind of food now says livestock is left out because TradeLord trades a herd as goods and never as food.
- The feature list now says a herd is never counted as food.

## [1.57.0] Selling: an army waiting on livestock holds back whatever herd you carry

72fe7394b0c456825674b0946dc5f1e7d650b553, 2026-09-11

- An army waiting on livestock now holds back that many head of whatever herd you are carrying, so the last quest that could still lose its supplies to a shopping trip is covered.

## [1.56.0] Selling: the grain and wine an army or a headman is waiting on is held back too

154827e811e9f01fe937a62408fa23c30d6d3cc7, 2026-09-10

- Two more quests are read for what they are waiting on, the army that needs supplies and the headman who needs grain, so the grain and the wine they are owed is held back from every pass that sells.

## [1.55.0] Selling: a good a quest is waiting on is held back whatever it is, not only an animal

e75320d8ea2dc59ac7907606f2014b6bbcc21c6e, 2026-09-10

- TradeLord now holds back any good an active quest of yours is waiting on, a trade good or a raw material as well as an animal, so a shopping trip can no longer sell the supplies a quest needs.
- Four more quests are read for what they are waiting on: the two artisan deliveries, the gang leader's stolen goods and the landlord who asks you to sell his produce.
- Nothing bought here now says it was your Buy cap per item that stopped a good, instead of naming your purse.
- The warning that your purse is under your gold reserve is no longer swallowed by a visit that sold something.
- The town marked on your map is never one TradeLord would walk into and leave alone as the same arrival.
- When TradeLord cannot thin your herd, TradeLord.log now names the animals it is holding back rather than falling silent.
- Always sell now says a good a quest is waiting on still holds, rather than only an animal.
- The feature list now says anything a quest is waiting on is held back from every pass that sells.

## [1.54.0] Buying: Keep gold for days of wages now ships at 0, so a large army no longer stops TradeLord buying

0e80b2f0c236900e27794f8cccc600b1e5282703, 2026-09-10

- Keep gold for days of wages now ships at 0, so out of the box TradeLord holds back only your gold reserve and a large army no longer stops it buying.
- Nothing bought here now names your purse only when nothing else held a good back, so the reason you are shown is the one you can act on.
- With Simulation mode (dry run) on, what a dry run spends at a market goes back into that market's gold, so a sale later in the visit is measured against the till the market would really have.
- The feature list now says TradeLord holds back only the days of your troops' wages you ask it to keep.

## [1.53.0] Log: TradeLord.log now says what is holding your purse when nothing is bought

665b4742277f1fe5e0a0cb27126b60248e95e66d, 2026-09-10

- When TradeLord buys nothing because your purse is under what it holds back, TradeLord.log now says what your purse is, how much is held back, and how much of that is your gold reserve against days of your wage bill.
- With Silence trade messages on, meeting a caravan or a party of villagers on the road during the Economy settling delay no longer puts a line on your screen.

## [1.52.4] Language: a language file that points somewhere else no longer leaves the game waiting

8467b9d28283a122ae774cf4293e3c16d7354c68, 2026-09-10

- TradeLord now reads a language file as it stands, so one that points somewhere else can no longer leave the game waiting while TradeLord fetches it.

## [1.52.3] Simulation mode: a dry run keeps back only the food it still has

073e8fb6e4ad1edc87af6302d9187206e1ac09e6, 2026-09-10

- With Simulation mode (dry run) on, food the dry run has already sold no longer counts towards your food reserve for the rest of the visit, so it now sells the same animals to get you back up to speed that a real pass would.

## [1.52.2] Simulation mode: a dry run no longer counts what you paid twice over

5820bb4da15d455b450740bc6e72be1f4ed00cf4, 2026-09-10

- With Simulation mode (dry run) on, the profit reported for a good that sits in more than one stack in your bags no longer counts what you paid against more units than you actually bought.

## [1.52.1] Settings: Restock and keep food (days of supply) at 0 now keeps no food back at all

ee7dbe7d2fc648f0e2fd4313f10353850537e549, 2026-09-10

- Restock and keep food (days of supply) set to 0 now keeps no food back at all, including the few of each kind Keep some of every kind of food was still holding.
- The hint under Keep some of every kind of food now says it needs Restock and keep food (days of supply) above turned on to do anything.
- The feature list now says keeping some of every kind of food follows the days of supply you set.

## [no release] Checks: the test runner and the game version fit tool move up to their newest versions

c5634462a4de25c810e83aa9a04162e7c55ea405, 2026-09-10

- The test runner moves from Microsoft.NET.Test.Sdk 18.9.0 to 18.10.0.
- The game version fit tool moves from System.Reflection.MetadataLoadContext 10.0.11 to 10.0.12.

## [1.52.0] Buying: the per-item caps now hold while restocking and buying haul animals

c4cc33d95b56bcf8dc014aa875dcc983711a3045, 2026-09-09

- Buy cap per item, in count and in denars, and Stop buying at this many held now hold while TradeLord restocks your food and buys haul animals, not only while it buys for profit.
- Share of the hold one good may fill now holds while TradeLord restocks your food, and is left off haul animals, since one of those adds to the hold rather than filling it.
- The lines TradeLord adds when you meet a caravan or a band of bandits now come out in the language you picked, even when you change it after loading your game.
- The feature list now says which passes the per-item caps hold in, and that a conversation line takes the new language too.

## [1.51.1] Map marker: it now follows the Trade with towns switch

7b6bc9cc1fb4f5b86386bc4241bc378206eee063, 2026-09-09

- Turning Trade with towns off now takes the map marker off the best sell town as well, instead of leaving it pointing at a town TradeLord will not trade in.

## [1.51.0] Settings: a new Trade Pool group, with a switch for trading in towns

1e403f071dde94d97526741cb885dc4d78540aa6, 2026-09-09

- A new switch, Trade with towns, turns trading in town menus off the same way Trade with villages does, and it ships on.
- The settings screen has a new Trade Pool group, holding Trade with towns, Trade with villages and Trade with caravans and villagers, with the town and village travel ceilings and Exclude hostile markets under them.
- The longest hints on the settings screen are shorter, so a hint no longer spills over the settings beneath it.
- The feature list now says trading in towns and trading in villages can be switched off one at a time.

## [1.50.2] Settings: the road trading setting is now called Trade with caravans and villagers

c06467cbb600404be50290674ef1a2e7e131e161, 2026-09-09

- Trade with caravans and villagers you meet is now called Trade with caravans and villagers, a shorter name that makes it clear this setting, rather than Trade with villages, is the one that covers a party of villagers you meet on the road.

## [no release] Changelog: the settings reset entries are cut back to one line on 1.50.1

015eab7cb78403781173968e779b6802ae832057, 2026-09-09

- The entries about the one-time settings reset are taken out of 1.50.0 and 1.50.1.
- The 1.49.0 and 1.49.1 sections, which carried nothing else, are taken out with them.
- 1.50.1 now carries one line about the one-time settings override.

## [1.50.1] Settings: the reset now reaches the copy the settings screen keeps, so it holds

db53f25e27c331676d5e389b2dbf5e45f0e7dd5e, 2026-09-09

- The reset in 1.50.0 did not stick if you have MCM installed, because the settings screen keeps a copy of its own and handed your old settings straight back a few seconds after the game started.
- TradeLord now puts the settings screen's own copy back to what it ships with as well, so the reset holds.
- Because 1.50.0's reset never took, every setting goes back to the value TradeLord ships with once more, the first time you run this version.
- Anything you had set is written into TradeLord.log as it happens, named setting by setting, so you can put back the ones you want.
- Your never sell, always sell, never buy and always buy lists are emptied by that reset along with everything else.
- This happens on this version only and never again on a later one.

## [1.50.0] Settings: every setting goes back to what TradeLord ships with, once, and your food reserve holds when the herd is thinned

dd27e098243f2211de78b1cb5a569e55e01da821, 2026-09-09

- Every setting goes back to the value TradeLord ships with, once, the first time you run this version, because the settings it ships with now trade better out of the box than they used to.
- Anything you had set yourself is written into TradeLord.log as it happens, named setting by setting, so you can put back the ones you want.
- Your never sell, always sell, never buy and always buy lists are emptied by that reset along with everything else.
- This happens on this version only and never again on a later one.
- Getting your party back up to speed no longer sells the animals Restock and keep food (days of supply) is holding back for your men.
- Where your herd is also your food, that can now leave the herd speed penalty in place rather than eat into the reserve, and TradeLord.log says the penalty is still there.
- An animal on your always-sell list is still sold to get you back up to speed, because the food reserve never holds one of those back.
- What TradeLord reckons a good cost you no longer drifts by a denar as it sells a stack down, so a sale no longer stops early with goods left that were worth selling.
- When a quest is waiting on more of an animal than your food reserve holds back, TradeLord now keeps the larger of the two rather than the smaller.

## [no release] Checks: the owner's signature is read out of the working rules rather than written a second time

1bf3389f967e8b686e3620f1b85f9f0144e46736, 2026-09-09

- The check that holds the commit signature in place no longer carries its own copy of the address, and reads the one CLAUDE.md carries instead.
- It now refuses any second address written anywhere in the working rules, rather than watching for one address it knew about.
- It also holds the name in the signature to the name in the address, so the two cannot drift apart.

## [1.49.1] Settings: the one-time reset is over and nothing is put back again

a0c4e16b84cca119f6932fc10323e62b2071a2d9, 2026-09-09

- The one-time reset of every setting is switched off from this version on, so nothing of yours is put back to what TradeLord ships with again.
- That reset ran on 1.49.0 alone: if you came to this version straight from an older one you were never reset, and your settings stand exactly as you left them.

## [1.49.0] Settings: every setting goes back to what TradeLord ships with, once

dcbb11d7254060d0d326c130e22f0886c0c3b5cc, 2026-09-09

- Every setting goes back to the value TradeLord ships with, once, the first time you run this version, because the settings it ships with now trade better out of the box than they used to.
- Anything you had set yourself is written into TradeLord.log as it happens, named setting by setting, so you can put back the ones you want.
- Your four item lists are emptied by that reset along with everything else, so a never-sell or always-buy list you had built up is in the log too.
- This happens on this version only and never again on a later one.

## [1.48.0] Settings: keeping food and restocking it are one setting, and the price trace says what the market really paid

3b635a5190a81887972affab9a855483959283a1, 2026-09-09

- Keeping food back and restocking it are one setting now, Restock and keep food (days of supply), which ships at 3 days: TradeLord holds that much back before it sells any food and tops you back up to the same amount as it trades.
- If you had set Restock food (days of supply) to something of your own, TradeLord now goes by your Keep food number for both and says so in TradeLord.log.
- Town travel ceiling now ships at 2.4 days instead of 3.
- Write a price trace to the log now sits in a Debug group of its own at the foot of the settings screen.
- The price trace is now written as you walk into a market before anything is traded, so the prices in it are the ones TradeLord went on rather than what was left after it traded.
- With the price trace on, every good a pass moves is now written down with the price TradeLord quoted next to what the market actually paid.
- When TradeLord cannot read the game's herd penalty, TradeLord.log now names everything that stops, which is buying livestock, buying haul animals and selling an animal to get you back up to speed, where it used to say only that livestock buying had stopped.
- The herd check in TradeLord.log now says when it could not read the herd penalty at all, instead of reporting that there is none.
- A panel hotkey written with a leading plus, such as +T, no longer reports an empty modifier in TradeLord.log.
- Buying none of a good can no longer leave TradeLord holding a cost against goods you have none of.
- The feature list now says the price trace is written before anything is traded and records what TradeLord quoted against what the market paid.

## [1.47.5] Settings: a hint now names the setting it sends you to the way the screen names it

7373bc3f5586aa45a6b9f177114d8d4137496364, 2026-09-09

- The hint under How many of each kind of food to keep now points you at Keep food (days of supply) by the name the settings screen shows, in every language, instead of a name no setting has.

## [no release] Checks: the food reserve is held to its promise over twenty thousand random bags

41622eb67ece77267119476a6a9aef31ef75eb7d, 2026-09-09

- The food reserve now has a test that fills the bags at random, twenty thousand times over, and holds it to two things every time: it never keeps more of a good than the bags carry, and it never keeps less than the days of supply asked for while there is still food to keep.
- That test fails on the reserve as it stood before the last version, so the fault it found cannot come back unnoticed.

## [no release] Checks: the game is asked whether one good still sits in more than one lot of the bags

85ce2532a7dd32a14fdef7aa14ba6a443c2d8ee7, 2026-09-09

- The game version check now reads the bags the way the mod counts them: an inventory lot still names its good, its quality and its quest hold, and the bags still look a good up both by itself and by the lot it sits in.
- A source check holds that question in place, so the food reserve and the per-lot caps cannot go on resting on a shape nothing verifies.

## [1.47.4] Food reserve: every stack of a good now counts toward what is kept back

f90b6429047780797c5bb7c34e2c6e6233b39e12, 2026-09-09

- Keep food (days of supply) and Keep some of every kind of food now count every stack of a good together, so when your bags hold the same food in two stacks, as they do when a quest hands you some alongside your own, TradeLord no longer sells off part of the reserve it was told to keep.

## [1.47.3] Ledger panel: the running total no longer turns negative once it grows past two billion

4cd47e0e77da4c77ab84bb1f4721e882a705c76e, 2026-09-09

- The running total of what TradeLord has made you, along the top of the ledger panel, no longer turns negative once it passes about 2.1 billion denars, and a campaign saved before this still reads its total back.
- The feature list now says a save carries two of TradeLord's numbers rather than one.

## [1.47.2] Simulation mode: a dry run now counts every stack of a good

8f5eb77a085f48cdf506656b717ea64b4e7d4ac7, 2026-09-09

- Simulation mode (dry run) now counts every stack of a good you carry and every stack a market has, so a dry run over looted gear or horses, which your inventory keeps in a separate stack for each quality, no longer stops short of what TradeLord would really sell and buy.

## [1.47.1] Prices: what a market pays and charges is now read the way the trade screen reads it

56f5857f8195e236c8f63ee87b1d3251773ae0e3, 2026-09-08

- Every price TradeLord quotes you is now read from a market the same way the trade screen reads it, naming the merchant you are trading with, which brings the tooltip, the ledger panel and the routes into line with the price you are actually offered.
- Village prices moved the most, because a village's own shelves were being left out of what TradeLord read there.

## [1.47.0] Log: a price trace you can switch on when a price looks wrong

63477842c054ea09d265ce453cab14a682a2701b, 2026-09-08

- A new setting, Write a price trace to the log, off out of the box: turn it on and TradeLord writes to TradeLord.log what the market you are standing in pays and charges for every good you are carrying, so a price it shows you that the trade screen does not offer can be tracked down.
- That trace names the market, the price model the game is running and any other mod changing either of them, so a mod moving prices behind TradeLord's back shows up by name.
- The feature list now says TradeLord can write a price trace to its log.
- The feature list now says that trading on arrival runs once for each arrival and that waiting in a town or village does not set it trading again, and that leaving a market and walking straight back in counts as the same visit.
- The feature list now says that trade goods and livestock you never paid for wait for a price that clears your margin over what they are worth, and it names the What a good counts as having cost you setting the way the settings screen names it.
- The feature list now says that the town marked on your map keeps up with you as you ride, and that TradeLord.log is kept from one session to the next and is only ever emptied at startup once it has grown past 999 KB.

## [no release] Settings: a session may put the checkout on origin/main without being asked

011bae6d631b31bc9825cbf232d2cec4e7291bbc, 2026-09-08

## [no release] Session hook: it now starts every session on origin/main instead of on whatever the container held

2be2da6d577393eacc51aba85d41befc1b7fe1d1, 2026-09-08

## [no release] Repository: the feature list catches up with what TradeLord does now, and a session starts on the latest source

e2e922aa2ad6f641e5eede21ecf1372a3dc6344e, 2026-09-08

- The feature list now says that trading on arrival runs once for each arrival and that waiting in a town or village does not set it trading again, and that leaving a market and walking straight back in counts as the same visit.
- The feature list now says that trade goods and livestock you never paid for wait for a price that clears your margin over what they are worth, and it names the What a good counts as having cost you setting the way the settings screen names it.
- The feature list now says that the town marked on your map keeps up with you as you ride, and that TradeLord.log is kept from one session to the next and is only ever emptied at startup once it has grown past 999 KB.
- The working rules now put every session on origin/main before it reads or changes a line, say that moving the checkout there is never a question to put to the owner, and treat a remote whose history shares no ancestor with the container as a rewritten remote that still wins.
- The paste tool now answers for a shipped version even while the changelog opens on Unreleased, which the rules allow and it was refusing, and it never hands out Unreleased as a shipped section.

## [1.46.2] Trading: goods you were given now wait for a price worth taking, and waiting no longer trades again

6bc5237229198579e732889d5e893d13c6bef835, 2026-09-08

- Trade goods and livestock you never bought, like the ones a town event hands you, are no longer sold off at the first market that will take them, and instead wait for a price that clears your Minimum profit margin over what the cheapest market you know would have charged for them.
- Nothing sold here and Nothing bought here no longer answer with haul animals and mounts are not traded as livestock, since TradeLord buys and sells those under rules of their own, and they name a reason about your own cargo instead.
- Wait here for some time no longer starts TradeLord trading all over again when the menu comes back: it trades once when you arrive and leaves that market alone until your party has taken to the road, and Trade here now (TradeLord) still trades whenever you ask.

## [1.46.1] Settings: an up to date TradeLord.ini is left as you wrote it

236580a630dd026991545d59afdb73a9ec1c5a13, 2026-09-08

- TradeLord now reads the SettingsVersion line in your TradeLord.ini to decide which of its older settings to carry forward, so a file already up to date is left exactly as you wrote it.
- A value you type into an up to date TradeLord.ini by hand is now held to what that setting takes today, so a wrong one is named in TradeLord.log instead of being quietly turned into something else.

## [1.46.0] Log: TradeLord.log is now kept under 999 KB, and it is only ever emptied as the game starts

0d86b96729157a408e51e399e125153892b81533, 2026-09-08

- TradeLord.log is now emptied as the game starts if it has grown past 999 KB, so a long campaign can no longer leave one file growing without end in your Bannerlord folder.
- It is only ever emptied as the game starts and never as the game closes, so the log you send on after a session still holds everything that session wrote.
- TradeLord.log says when it has been emptied and how large it had grown, so a short log is never a mystery.
- A single line TradeLord could not write to TradeLord.log no longer slows every line after it for the rest of the session, because TradeLord picks the file back up half a minute later instead of leaving it until you restart the game.

## [no release] Buying: what is left to spend drops a running total nothing ever gave it

72eafd6fd18aff2f6d055283c2d12330a565e9d8, 2026-09-08

- What is left to spend was worked out from two running totals, and both places that ask for it always passed the second one as nothing, so it is gone and the arithmetic reads the one total a visit keeps.
- The tests that were the only callers ever passing the second total now push the same money through the path a visit actually takes.
- Nothing a user sees or gets changes, so this ships no version.

## [no release] Checks: the road-trade type and the settings screen's MCM internals are now read from the real assemblies

377adb46a0a20188356cd7de30e8c95a4c6aff96, 2026-09-08

- The compat tool now resolves TaleWorlds.CampaignSystem.Settlements.FakeMarketData, the type a trade with a caravan is priced through, and confirms it still implements IMarketData and can still be made with no arguments; the compiled assembly never names it, so nothing checked it before.
- The compat tool now opens the MCM package as well and reads the seven members and two constructors the settings screen reaches by name, so an MCM rename can no longer take the screen's own language away without the build noticing.
- A commit that ships a version and takes a source file away is now refused, because code that moves or retires gives a player nothing to read; that move ships on its own as [no release] first, then the version on top of it.

## [1.45.0] Settings: how far TradeLord looks is now your two travel ceilings and nothing else

ed86f1ffa31a8760804aec1a177b05f2ae949f58, 2026-09-08

- Town travel ceiling and Village travel ceiling are now the only two things deciding how far TradeLord looks, and everything obeys them: the tooltips, the routes, the town marked on your map, and how far Hold cargo for the best market will wait.
- Hold cargo for the best market now waits only for a town within your Town travel ceiling and a village within your Village travel ceiling, three days and one day out of the box, instead of for the best price anywhere in Calradia.
- The scan radius setting is gone, since those two ceilings now cover what it did, and TradeLord.log names the value it dropped from your TradeLord.ini.
- Your travel ceilings keep everything you had set: TradeLord.ini carries them forward under their new names by itself.
- The feature list drops the scan radius and names the two travel ceilings instead.

## [1.44.0] Marker: the town marked on your map keeps to the same travel ceiling as everything else

ee07e858b200e2786ecda3c3ce896887007b2c19, 2026-09-08

- The town marked on your map now keeps to your Town travel ceiling and Scan radius like everything else, so it can no longer send you to a town TradeLord will not sell in.
- The town marked on your map now follows you as you ride, instead of waiting until you enter or leave a settlement.
- The Travel ceiling setting is now called Town travel ceiling, since that is what it governs.
- The marker's own travel ceiling setting is gone, and TradeLord.log names the value it dropped from your TradeLord.ini.
- A market that sold nothing now says why even when TradeLord bought something there, so Hold cargo for the best market can no longer hold your cargo in silence.
- TradeLord no longer says a market had nothing worth trading when it had nothing to weigh up in the first place.

## [1.43.0] Trading: leaving a market and walking back in is the same visit

3d9d9fbdc20c103e4a6230f4b01c7c01e3f50f4a, 2026-09-08

- Leaving a market and walking straight back in counts as the same visit, so TradeLord no longer buys back the goods it has just sold you there.
- Your Max spend per visit now lasts the whole of that visit, instead of starting again each time you step back inside.
- TradeLord.log is kept from one session to the next, instead of being emptied every time the game starts.
- TradeLord.ini is no longer rewritten when you start the game and none of your settings have changed.
- TradeLord.log now names the goods it stops holding against a resale when they leave your party unsold, rather than only counting them.

## [1.42.4] Buying: a good you buy by hand is written down as what you carry, not thousands of it

16b4a9d83b0bdbf279faff17c578dcb9783ad52c, 2026-09-08

- Buying at a market yourself no longer leaves TradeLord thinking you took thousands more of a good than you did, so it keeps what you really paid and stops selling that cargo under your Minimum profit margin.
- That miscount also made the game work out the price of thousands of units you never bought, which is gone.

## [1.42.3] Road: meeting a caravan and trading nothing now tells you why

20348cabca697528fba7cc73d719b16fd455f025, 2026-09-08

- Meeting a caravan or villagers on the road and trading nothing with them now tells you why, which it was meant to do from 1.42.0 and did not.
- The feature list no longer says a route is quoted against what your purse holds, because since 1.42.0 the ledger lists a route whether or not you could pay for it today.

## [1.42.2] Selling: a market visit costs your game less on the way out too

28988bbef3b5c4cbabfd16c371fcff5cdc2008e7, 2026-09-08

- Selling at a market now works out the best markets for everything in your bags in one go, instead of once for each good, the same way buying already did.

## [no release] Checks: three assertions go back to reading the method they were written to read

9f4df9d761948f78e955d4e973a59c534c47daab, 2026-09-08

- Moving the trading layers into files of their own left three assertions reading a whole file where they had read one method, which would have let the line they look for pass from anywhere in that file.
- Each one reads its own method again: the purse outranking the counted reasons, and the two that pin what happens when the game's crafting record cannot be read.

## [no release] Travel: the arithmetic behind a journey moves where a test can ask it

882071989e6006818f64decab323d7215602db74, 2026-09-08

- Working out how long a journey takes, how a fleet's speed is blended from its ships, and what a party that has stopped counts as, all move into TradeMath.cs, which needs nothing from the game.
- Nothing a user sees or gets changes: the same days, the same fleet speed, the same walking pace when a party has stopped.
- Ten tests now drive that arithmetic directly, which had none at all: a journey all on land, all at sea, and split between the two, one slow ship holding a fleet back, a party with no ships sailing at the speed it walks, and the quick estimate never claiming a journey takes longer than it really does.
- Two source checks now name TradeMath.cs as the place they read, and two more pin that the arithmetic stands clear of the game and that each layer of the trading code has a file of its own.

## [no release] Trading: the stall reasons and the encounter helpers move into files of their own

8eaf396ec066c88a9142d5743f719f0964c9d414, 2026-09-08

- BlockTally moves out of Trading.cs into Reasons.cs, unchanged line for line, so the reasons a pass gives for moving nothing sit together.
- Errands and Parley move into Encounters.cs, unchanged line for line, since both read the campaign by name for something you meet on the road.
- Trading.cs drops the using directive only the quest lookups needed.
- Thirty-four source checks now name the file they read, the two that delimited Parley by the class that used to follow it read its own body instead, and the check on reflected game members scans every source file rather than one.

## [no release] Trading: the rules that read the game move into a file of their own

25e58b5f1fd23f8ffc10afd8ca91b5dafaa7a33c, 2026-09-08

- TradePolicy moves out of Trading.cs into Policy.cs, unchanged line for line, so the seam between the game and the trading rules is a file you can open on its own.
- Trading.cs drops the three using directives only that layer needed.
- Eighty-three source checks now name Policy.cs as the place they read, and the check on the money rules looks for a second copy across both files instead of one.

## [1.42.1] Trading: a market visit and the ledger cost your game less

21bf4446603bf4d3414d7fa498dac3c886b5643d, 2026-09-08

- Walking into a market now works out the best markets for everything on the shelf in one go, instead of once for each good, so a busy market costs your game less.
- Restocking the larder and buying a haul animal do the same.
- The ledger now gives back what it worked the routes out with as soon as it has finished, instead of holding on to it until the next time you open the panel.

## [1.42.0] Ledger: it now lists a route even when you could not take it this second, and the road says why nothing moved

fb58b20f7d09fdf04aa295da9eb3f55c481ddf13, 2026-09-08

- Meeting a caravan or villagers on the road and trading nothing with them now tells you why, the same as walking into a market already did.
- The TradeLord ledger now lists a route even when your purse is empty or your herd is full, so it always tells you where the profit is rather than only what you could do this second.
- When your purse is empty the ledger says so under the routes, instead of showing that message in place of them.

## [1.41.9] Trading: working out the best markets for a good costs your game less

a53bec46be085a19cc1aed8082875759dac2ce16, 2026-09-08

- Working out which markets pay best for a good is quicker, because TradeLord now keeps the best few as it goes instead of putting every town in order first.
- TradeLord no longer asks a town what it pays for a good when that town is already beyond your Travel ceiling.

## [1.41.8] Language: a language file that could not be read is tried again instead of giving up on

84fa69a528b3a935d65b0575411dd5b40aacd865, 2026-09-08

- If TradeLord cannot read its language file for a moment, it now keeps trying and speaks your language as soon as it can, instead of falling back to English until you restart the game.

## [no release] Checks: six source checks retire in favour of the tests that prove the same thing better

9e234a3072fc673041e7c3f39af836df0465a897, 2026-09-08

- Six checks that read the rule bodies as text are gone, each replaced by a test that asserts the same claim on behaviour instead.
- One food reserve in total rather than one per type is now proven by the reserve spilling from grain onto fish, not by counting how often a setting name appears.
- The reserve covering livestock is proven by a cow with five meat counting for five, cheapest-first by caviar losing to grain, and livestock last by a cow worth one still losing to grain worth five hundred.
- A good the game refuses to trade being left unbought, and every cap on buying naming itself, are proven by the reasons the rules actually return.
- Every check that pins how the rules are wired to the game, what the settings screen and the log say, or that the rules stay clear of game types, is left exactly as it was.

## [1.41.7] Settings: the language you pick takes hold again without a restart

8101842bfa81fff9e444ff69920d724e37ed378e, 2026-09-08

- The language you pick, and every other setting you choose from a list, now take hold as you pick them instead of waiting for you to restart the game.
- TradeLord.log now says when the settings screen has taken charge, instead of only saying it was still waiting for it.
- With Live world prices off, writing down what a market charges no longer takes longer the more towns you have visited.

## [no release] Trading: the food reserve is worked out where a test can ask it

3ddb386df015cc283991bee6a8d63690a9a5d998, 2026-09-08

- How much of each food your reserve holds back, and what a good is worth as food, move into the same file as the rules that decide a sale and a purchase.
- Nothing a user sees or gets changes: the same reserve, the same order it fills in, the same floor under every kind of food.
- Restocking the larder and buying a haul animal now describe a good on the shelf once instead of working the same answers out again for each unit.
- Seventeen tests now drive the food reserve directly, which had none at all: the days of supply, the cheapest food going first, livestock counting for its meat and being kept last, the floor under every kind, and what happens when that floor is already larger than the reserve.

## [no release] Trading: the herd order and the animal names now sit with the other rules

56a3289d2eeeb67a8b89b9967d017decdd6d2234, 2026-09-08

- Which animal the herd gives up first, and what each kind of animal is called in the log, move into the same file as the rules that decide a sale and a purchase.
- Nothing a user sees or gets changes: the same order, the same names.
- The selling pass now describes a good once and hands that description to every rule that asks about it, instead of working the same answers out again for each unit it sells.
- Six more tests pin the shedding order the feature list promises, livestock first, then a plain spare mount, then a haul animal, then a war or noble horse.

## [no release] Trading: the rules that decide a purchase now sit beside the ones that decide a sale

90a2919623fb8c26efde214b46ffe00f77ed9255, 2026-09-08

- Deciding whether a good may be bought, whether an animal may be hauled or shed, whether a good may be sold on again, and which cap stops a purchase all move into the same file as the selling rules, reading a description of the good rather than the game's own item.
- Nothing a user sees or gets changes: the same decisions in the same order, refused for the same reasons.
- The buying pass now describes a good on the shelf once instead of working the same answers out twice for it.
- Fifteen more tests drive the buying rules directly, covering both never lists, the grain switch and the way the larder gets past it, every spending cap in the order they are asked, and what may be shed to get back up to speed.

## [no release] Trading: the rules that decide a sale now sit where a test can ask them

e0fbd87537c54d48a849badc70dc8956dac9f7a5, 2026-09-08

- The rules deciding whether a good may be sold move into their own file, reading a description of the good rather than the game's own item, so nothing about them needs a running campaign.
- Nothing a user sees or gets changes: the same decisions in the same order, refused for the same reasons.
- The lock, the smeltable check and the learned-parts lookup stay behind a seam the rules only reach for once the cheaper checks have passed, so a sale costs no more to work out than it did.
- Seventeen tests now drive those rules directly, covering every reason a sale can be refused and the order the reasons are asked in.

## [1.41.6] Trading: what TradeLord knows about the best markets now survives a market visit

4fc72115c1d827f30d6a454f2c82a95febce7fae, 2026-09-08

- Walking into a market no longer makes TradeLord work out the best markets for every good all over again, since walking in moves no prices.
- After TradeLord trades in a market, it works the best markets out again only for the goods its own trading moved the price of, instead of for every good on the map.

## [1.41.5] Feature list: it uses the settings screen's own names and catches up with two changes

4b2d2059a3afd725d8dcbd45fbeabe832ba97ac1, 2026-09-08

- The feature list now calls the setting that reads prices from the whole map by the name the settings screen gives it, Live world prices, instead of calling it honest-merchant mode.
- The feature list now says a pin comes off a town by itself once TradeLord has traded there.
- The feature list now says Share of the hold one good may fill ships at 45%.

## [1.41.4] Inventory: the price lines and the profit colouring cost your game less per row

80e260f0a9c47a938bba6845ff387c830f268de0, 2026-09-08

- The price lines and the profit colouring in your inventory no longer make a small extra piece of work for every row on the screen.
- Working out which markets pay best for a good no longer makes a small extra piece of work every time TradeLord works it out.

## [1.41.3] Buying: a market visit and the campaign map both ask the game less than they did

f07b0578e1a6d8578921157bd5b247ac35339c17, 2026-09-08

- Buying at a market no longer asks twice of every good on the shelf whether it is one you allow TradeLord to buy.
- Buying no longer works out what your party can still carry when your purse or one of your spending caps has already stopped the purchase.
- The campaign map no longer checks every panel on screen for a text box on every frame, only when you press the TradeLord hotkey.

## [1.41.2] Trading: counting your herd, writing the log and marking a town on the map all cost the game less

b1c2a129aa2abcfd2454c35e38572f103cd1f743, 2026-09-08

- Walking into a market, each new day on the road, and buying livestock no longer work out how many animals your party can drive one animal at a time.
- Selling animals to relieve the herd no longer works out how many haul animals your cargo can spare one animal at a time.
- The daily check on the road now works out how many animals must go once instead of twice.
- Whether your animals are slowing you down is now decided on a real difference in your speed, not on the smallest one the game can report.
- TradeLord no longer reopens TradeLord.log for every line it writes, so a busy market visit costs the game less.
- Auto-mark best sell town on map no longer prices your cargo in a town whose gold could never beat the best town found so far.

## [1.41.1] Trading: a market that bought nothing no longer names a setting when grain is why

1616c5ba9bf70b03c9075b2f3718aa88210c578e, 2026-09-07

- When grain is what TradeLord left alone, the line that says why nothing was bought now says grain fills the cargo for little return, instead of naming a setting.

## [1.41.0] Buying: one good is capped at 45% of the hold, and a held-back good names the right reason

108e7ead52b11f54ae4dd65d5dfc4700e50b4517, 2026-09-07

- Share of the hold one good may fill now ships at 45% instead of off, so one cheap good can no longer take your whole cargo.
- A town you pinned on the map loses its pin once TradeLord has traded there, instead of the pins piling up until you clear each one by hand.
- A good the Never buy grain setting is holding back now says so, instead of saying it is on your never-sell or never-buy list when nothing is on those lists.
- Buying a large stack no longer works out what your party can carry all over again for every single unit.

## [1.40.4] Item lists: a good named the way the game shows it no longer covers another good

4249f3868368918b0fad4dba80dcb2b24b2beef1, 2026-09-07

- An entry on one of your item lists that names a good the way the game shows it, such as Iron Ore, no longer also covers a different good whose short name is one of the words in it.
- An entry that mixes a short name with a name the game shows, which TradeLord.log already said matches no good, now really covers nothing instead of quietly covering part of it.

## [1.40.3] Trading: a large stack moves faster on the road, and a dry run names the right reason

042bc1b2ebcdea2538b8212d1a02456223a011bc, 2026-09-07

- Selling or buying a large stack to a caravan or a party of villagers on the road no longer makes the game a small extra piece of work for every single unit, which trading in a market stopped doing in 1.40.2.
- A dry run now says you already traded a good on this visit when that is what stopped it selling more, instead of naming your food reserve.

## [1.40.2] Trading: selling or buying a stack no longer makes extra work for every unit

2954bcb718aee814f1bad44412deb080728a5566, 2026-09-07

- Selling or buying a large stack at a market no longer makes the game a small extra piece of work for every single unit, which it started doing in 1.40.1.

## [1.40.1] Trading: the download carries every change made since 1.40.0

e75d647683959ae5f0b72e702f0daf2c5f53e71c, 2026-09-07

- The download now carries every change made since 1.40.0.
- Quick-sell and quick-buy stop quietly when your party cannot be read, the way restocking food, buying a haul animal and getting your party back up to speed already did, instead of writing an error to TradeLord.log.
- Selling to a caravan or a party of villagers on the road works out what you took in once instead of twice.

## [no release] Trading: what a pass took in and what it paid out are each worked out in one place

a9000db92b1019cfefdd4886e07d419bc1e44358, 2026-09-07

- The sum a pass took in and the sum it paid out are now each worked out in one place, which the five market passes and a meeting on the road all read, instead of the same two sums being spelled out at each of them.
- Selling on the road worked its takings out twice over, once for the log and once for the line on screen; it works them out once now.
- Nothing a user sees or gets changes, so this ships no version and writes no changelog entry.

## [no release] Trading: what a good cost you is carried by one value

1e1e75d35ada9495e0c286787fd0c8c2889b352c, 2026-09-07

- The four figures every sale reads for what a good cost you, the price paid, whether that price is a market quote, how many units are still covered by it and what an unbought unit is worth, are now one value that the selling pass, getting the party back up to speed and a meeting on the road all build the same way, instead of each declaring its own four.
- That value owns the draw-down, so a unit sold reduces the count you paid for in one place rather than at five call sites.
- Nothing a user sees or gets changes, so this ships no version and writes no changelog entry.
- A check that reads a window of source now reports itself broken when the end of that window is gone, instead of silently reading to the end of the file; one window had already lost its end and was put back.

## [no release] Trading: the five market passes are opened and carried by one object

ce272f3e43dd7352c85adfc6c6d266c139286223, 2026-09-07

- The five market passes now open through one Pass object that carries the settlement, the market, the shop, the party, the inventory locks, the dry-run flag and the pass's own books, instead of each pass declaring its own copy of all of it.
- Every unit swap and every price read goes through that object, so the gold-direction guard raises its flag in one place rather than at each call site.
- Nothing a user sees or gets changes, so this ships no version and writes no changelog entry.
- The checks were rewritten to describe where the source moved to, and one was added for the new object; none was loosened.

## [1.40.0] Trading: Silence trade messages now covers a caravan or villagers you meet on the road

efd6e2f9929f3c5a773549ef6671bc9bcf3410bb, 2026-09-07

- Silence trade messages now covers trading with a caravan or a party of villagers you meet on the road, which reported on screen whatever the setting said, and the setting's hint says so.
- The Always sell hint now says an animal a quest is waiting on is still held back, alongside your never-sell list and an inventory lock.
- Trading in a market no longer works out which markets are in reach all over again after each pass that moves goods, so a busy town settles faster.

## [1.39.3] Buying: restocking and haul animals stop before your gold reserve instead of spending down to it

d144de32fca7ec202022b2bfde74dffc83828e39, 2026-09-07

- Restocking food and buying a haul animal stop before your gold reaches your gold reserve again, leaving the reserve whole, and buying for profit is the one pass that spends down to it.

## [1.39.2] Selling: an animal a quest is waiting on is kept even where your always-sell list names it

2c2bc6bfa8453839a6c0104d9c27cae8db01fcbc, 2026-09-07

- An animal a quest is waiting on is now kept back even where your always-sell list names it, instead of being sold with the rest of your cargo.

## [1.39.1] Buying: restocking and haul animals spend down to your gold reserve, the way buying for profit does

39547f025a4d5cd6be077c8cc6ea7363505a7717, 2026-09-07

- Restocking food and buying a haul animal now spend down to your gold reserve, the way buying for profit already did, instead of always leaving a denar above it.

## [1.39.0] Trading: getting back up to speed now counts towards your profit and your Trade skill

0f63cfc61e537012e5d67270a07b205cfacfbf49, 2026-09-07

- Selling an animal to get your party back up to speed now counts towards the profit TradeLord has made you and earns Trade skill, the way its other sales already did.
- A visit that trades nothing now says a quest may be waiting on your animals when that is what held them back, instead of naming your food reserve.
- The best markets and the routes on the panel are worked out again as you ride, instead of waiting for the hour to turn.

## [1.38.5] Language: a line finds its translation whatever your computer is set to, and the feature list says how you ask a band to pass

2c1b3280f7b3ab26227c81e03f1b3ec8d7679798, 2026-09-07

- TradeLord now matches its own lines to their translations the same way whatever language your computer is set to.
- The feature list now says that asking a band to let you pass is a line you say to them, rather than the pop-up it used to be.

## [1.38.4] Selling: a quest item is kept when your herd is thinned, and a band's talk leaves your lines in order

e86bf3749814e4ae32b59ea149a9ae78f78fe17c, 2026-09-06

- Selling animals to get your party back up to speed now leaves a quest item alone, the way the rest of TradeLord's selling already did.
- Fixed the lines you can say in a conversation coming out in the wrong order for the rest of your session, when TradeLord could not place its free passage line among a band's answers.

## [1.38.3] Bandits: the line asking a band to let you pass now appears when you meet one

74be162056aa816ed693b790b9aaaa56b0d94379, 2026-09-06

- Fixed the line asking a band to let you pass never appearing when you met one: TradeLord went looking for the band's own talk before the game had finished writing it, so it never found where to put the line.

## [1.38.2] Saving: a campaign save can no longer be failed by TradeLord's own note-keeping

37fd77526da90147ff7cebc4204f700b63a11fc1, 2026-09-06

- Saving a campaign can no longer be failed by TradeLord's own note-keeping: if its ledger cannot be written down as the game saves, the save goes through and what went wrong is written to TradeLord.log instead.

## [1.38.1] Startup: TradeLord no longer writes an error to its log before a campaign is loaded

3557a498818e04ecc7350d69da3455b401075966, 2026-09-06

- Fixed an error TradeLord wrote to its log on every startup, and went on writing until a campaign was loaded, because it looked for an encounter before there was a game to have one in.

## [1.38.0] Bandits: asking a band to let you pass is a line you say to them, and meeting one no longer closes the game

661fa9c30e2ad42ffc316308657ec1ad5a79f52c, 2026-09-06

- Fixed the game closing itself when you met bandits: TradeLord's offer of free passage arrived as a pop-up over the talk you were already having, and answering the bandits after it had ended the encounter shut the game down.
- Asking a band to let you pass is now a line you say to them, in among your other answers, instead of a pop-up.
- The bandits answer, the talk closes, and your party rides on, so nothing is left half finished behind the conversation.
- Free passage from bandits now says on the settings screen that it adds a line to what you can say to them.

## [1.37.10] Trading: a visit that trades nothing names the protection that stopped it, and the cargo warning says what to do

ecd83402bd2d6477ba39c42b8491c916dd513334, 2026-09-06

- A visit that trades nothing now names the protection that held your goods back, your never-sell or never-buy list, an inventory lock, the unique and crafted protection, an animal a quest may be waiting on, a haul animal or a mount, or your food reserve, instead of saying only that your protections held it back.
- Where more than one protection was in the way, the message names the first one TradeLord met.
- The warning that your cargo is full now tells you what to do about it: recruit more men, buy more horses, or sell goods manually.

## [1.37.9] Selling: an animal a quest is waiting on is held back from every sale

3a830c96ead696722d38476c2e7323368e693e9f, 2026-09-06

- Animals a quest is waiting on are now held back from every sale, not only from thinning the herd, even when TradeLord cannot tell which animals the quest wants.
- The buying pass counts what you already carry afresh for each good, so a good a market stocks twice can no longer slip past your limits on how much of one good to hold.

## [no release] Repo: the source checks hold the cushion the herd guard leaves

e2491c70f1ff5f2594b132363f70257577f477a5, 2026-09-06

- A mutation audit of the source checks found one rule nothing was reading: the cushion the herd guard leaves below the game's speed penalty, which could have been dropped to nothing without a check noticing.
- The checks now hold that cushion, and were proved to catch both ways of removing it.

## [no release] Trading: the passes share one place for the rules they each repeated

5ea71e8e6bf5338ca76e7240a118f5fff55f0cbc, 2026-09-06

- The trading passes now share one place for four rules they were each keeping their own copy of: the automated-trade envelope, the shelf priced cheapest first, the caps that stop a purchase, and how a sale values a unit you paid for.
- Working out which units to stop counting as bought moved in with the arithmetic the tests cover, and is now tested directly.
- The source checks follow those rules to their new home, and each was proved to still catch a break in them.
- Nothing a user sees or gets changes, so this ships no version.

## [1.37.8] Trading: your spending cap now holds when you deal with a party on the road

36fb7c83dc580deaca48ebb670e057f56b3a1afd, 2026-09-06

- Trading with a party on the road now stops at your max spend per visit, instead of buying on until your purse is down to your gold reserve.
- The campaign map no longer works out every trade route the moment it opens, so a long campaign no longer hitches before you have even opened the ledger.

## [1.37.7] Selling: protecting unique and crafted items now covers your animals too

d170ffacf3f00b5dc7cd9848ca876c598dfe48d3, 2026-09-06

- Protect unique and crafted items now covers animals as well as gear, so a unique animal is no longer sold in the trading pass while the setting is on, which is what the setting already did when TradeLord thins your herd.

## [1.37.6] Simulation mode: a dry run now prices a whole visit instead of each pass on its own

f47ee8f0299e7dbec4b0e1c8744d5563bb513a70, 2026-09-06

- Simulation mode now models a visit as one visit rather than each pass on its own, so the merchant's gold, your purse, your carry weight, your larder and every per-item and per-visit cap carry from one pass to the next and a dry run no longer reports more trading than a real visit would do.
- Fixed simulation mode selling the same animals again every time it checked whether your party was back up to speed, and offering to buy goods an earlier pass of the same visit had already taken off the shelf.
- Simulation mode now spends what a meeting on the road just earned, and no longer buys back the goods it has just sold to that party.

## [1.37.5] Selling: animals a quest is waiting on are no longer sold as you walk into a market

167653153caa90bb4883d99d204fe2beef0fc1af, 2026-09-06

- Animals a quest is waiting on are now kept back whenever TradeLord sells, not just when it thins your herd, so the cattle or horses you are carrying to deliver are no longer sold as you walk into a market.

## [1.37.4] Ledger: goods leaving your party are noticed as they go, not once a day

0da3ae248b9783f3ed6b6673579c3ef7814b9e6f, 2026-09-06

- Goods leaving your party are now noticed as they go rather than once a day, so eating the last of a food and looting more of it before the day turns no longer leaves TradeLord counting the looted lot as something you paid for.

## [1.37.3] Ledger: what you paid for a good stops counting once the good has left your party

a775666886c1f3dfffaf0ed5b36af193d132051b, 2026-09-06

- Food your troops eat, and anything else that leaves your party without being sold, no longer counts as still bought, so TradeLord stops holding back looted goods of a kind you once bought and stops selling the next lot you buy too cheaply.

## [1.37.2] Larder: topping your food back up buys grain again, and the lock setting says how each side matches

e14006e00c82b5ef18a1c663fd24da0b31a29210, 2026-09-06

- Topping your food back up now buys grain again, so it works in a farming village where grain is the only cheap food there is. Never buy grain goes back to keeping grain out of trading for profit, which is what it is there for, and putting grain on your never-buy list still keeps it out of everything.
- The inventory lock setting now says how each side matches a lock: selling by item and quality, buying by item alone.

## [1.37.1] Settings: the free passage and herd settings now say what they really do

6927cb9054df114142ee644c95c5060dad099ffc, 2026-09-06

- The free passage setting now describes what it really does, asking you as you meet the band rather than adding a line to the encounter screen.
- The setting that sells animals to get your party back up to speed now says that an animal a quest is waiting on is left alone.
- The feature list now says that getting your party back up to speed keeps back the animals a quest is waiting on.

## [1.37.0] Herd: an animal a quest is waiting on is kept back when your party is thinned

2ddf32ee44138216bb730d59cc90cb61f431b2f4, 2026-09-06

- An animal a quest is waiting on is no longer sold to get your party back up to speed. TradeLord keeps back as many as the quest asks for and thins only the herd beyond them, so delivering a herd, draught animals or horses no longer costs you the animals you gathered for it.

## [no release] Checks: the herd outranking the food reserve is written down as the decision it is

262f9920cbcada1dcb4e7e73da3c5dd601caa84f, 2026-09-06

- Records that thinning a herd that is slowing the party down comes before keeping livestock back as food, so the two settings are never quietly reordered.

## [1.36.2] Trading: a locked animal stays put, and a good you buy by hand is written down at what it cost

de45d1eec3d39484488e6d6630937ea620f0f294, 2026-09-06

- An animal you have locked in your inventory is left alone again when TradeLord sells one to get your party back up to speed. A lock on a horse of a particular quality, such as a spirited or a lame one, was being read as a lock on the plain horse and so was passed over.
- A good you buy by hand is now written down at what its own quality cost you, rather than at the plain good's price, so TradeLord no longer sells a fine one on for less than you paid.

## [1.36.1] Road: a party you have already traded with no longer trades all over again when you talk to it

2438a62f220c6de68b765f18c7bd0fabe8d53945, 2026-09-05

- A caravan or a party of villagers you have just traded with no longer trades with you all over again when you talk to them without riding away first, so your spending limit for that meeting is only spent once.
- Selling to a caravan or villagers on the road now makes the coin sound, the way selling in a town does.
- A Reset on the settings screen now always takes hold at once and is written to TradeLord.ini.

## [1.36.0] Road: trading with a caravan or villagers works at last, and the free passage holds

e1310ac06bcd6c1b98ebbe16215651e11e74725c, 2026-09-05

- Trading with a caravan or a party of villagers on the road now actually happens. It had been failing every time, because the game refuses to sell goods without a market to sell them in, so TradeLord hands the goods and the coin over itself out on the road.
- The free passage line no longer turns up among your battle orders before a fight. It is offered as you meet the band instead of sitting on a menu of the game's own that the game shows elsewhere.
- Bandits who have let you go now leave you be for a few hours, instead of turning round and hitting you again the moment the game unpauses.
- The language you pick now takes hold when you press Done on the settings screen, rather than waiting for you to restart the game, and the ledger panel changes over with it.
- TradeLord.log says how long the free passage holds and which band it holds off you.
- The feature list now says the free passage leaves the band off you afterwards.

## [no release] Changelog: the two feature-list changes that went unrecorded are written into their own sections

b39fb2a5b3c3b5cc9442b0e97ee722b200834065, 2026-09-05

- The 1.34.0 section now records that the feature list was rewritten for meeting a caravan, villagers and bandits, and that TradeLord.log names the band when free passage is offered.
- The 1.35.0 section now records that the feature list dropped the shelf life and took up the language reaching the town menu, and that TradeLord.log says how many settings Reset put back.
- A commit that changes the feature list is now refused unless the changelog says what changed in it, which is what let both of those through.

## [1.35.2] Settings: the language setting leads again, and a slider drag is written down once

9d1ff685cef3f640527e4c2edc2b511fbdf746c5, 2026-09-05

- The language setting is back at the top of the settings screen, with auto sell and auto buy just under it.
- Dragging a slider on the settings screen no longer fills TradeLord.log with every value it passes through, and no longer rewrites TradeLord.ini for each one: your change is written down once, when the slider comes to rest.

## [1.35.1] Bandits: turning their offer down once no longer silences it for good

b1e8b9f06e8ee6a35f3b272fa588128557f3b353, 2026-09-05

- Turning down the bandits' offer of free passage no longer stops them offering it again the next time you run into that same band.

## [1.35.0] Settings: the language reaches the town menu now, automation leads the screen, and the shelf life is gone

692ed9545fab318d58515c67cd3dff6adc3fcacf, 2026-09-05

- The language you pick now reaches the town menu entries as well, so Trade here now and Consult the TradeLord ledger change over the moment you pick it instead of waiting for you to load a campaign again.
- Auto sell and auto buy now sit at the very top of the settings screen, above the language setting.
- Observation shelf life is gone: a price you recorded yourself is kept for as long as you have it, rather than being thrown away once it reached a certain age. Whatever you had set is dropped the next time TradeLord reads your settings.
- Quiet automation is called Silence trade messages now.
- How many of each kind to keep is called How many of each kind of food to keep, and its hint says plainly that it counts the food itself rather than days, because Food reserve (days of supply) is the one that works in days.
- It now starts at two of each kind rather than three.
- The Reset button now puts your settings back at once instead of taking several seconds over it, because it only redraws the settings that had actually been changed.
- The hint under Ledger panel hotkey (map screen) now says that one key name is all it takes, and that a word or an unknown key falls back to T.

## [1.34.0] Road: a caravan or villagers you meet are traded with on the spot, and bandits offer passage as you meet them

6a1d83734ac555f3d653de869ab6c5c31e7fa1db, 2026-09-05

- TradeLord now trades with a caravan the moment you meet it on the road, before anyone says a word, instead of waiting for you to pick a line of dialogue first.
- A party of villagers on the road now trades with you the same way a caravan does.
- Trade with caravans you meet is called Trade with caravans and villagers you meet now, and it governs both.
- Bandits now offer you free passage as soon as you meet them, asking whether to ride on or fight, so the offer no longer waits on a menu that never appeared for some of you.
- TradeLord.log calls these a sale or a purchase on the road now, and names the party it traded with.

## [no release] Build: the compatibility tool now runs on every build

9d6c6d559b47fb6fe711c6ef8a8a18ffc6359512, 2026-09-05

- The build now runs tools/compat against the 1.5.2.121216 beta after both assemblies are built, so a game update that moved a Harmony target, a reflected member or an enum value fails the build instead of failing at load.
- The beta version the checks and the feature list agree on is written in one place in the sweep.

## [1.33.0] Settings: haul animal is the one name now, on the screen, in the log and in your settings file

810168b887ac27c7ce48e6678e5943c1f8cbe4e7, 2026-09-05

- Your setting for buying haul animals is called BuyHaulAnimals in TradeLord.ini now, and whatever you had saved under its old name is carried over the first time this version reads your file.
- The hint under Share of the hold one good may fill now calls them haul animals, the same name the rest of the settings screen already uses.
- TradeLord.log now calls a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse and a Pack Camel a haul animal, rather than an animal that carries for you.
- TradeLord.log no longer opens every campaign with a roll call of every animal in your game, so the log starts on what you actually did.
- The hint under the Language setting now says what really happens: the language takes hold as you pick it, and your town menu entries follow the next time you load a campaign.
- The feature list now describes what TradeLord already does: a caravan on the road held to every rule a market visit is, your herd looked at three times a visit, the Reset button at the top of the settings screen, and TradeLord.ini written whether or not Mod Options is installed.

## [1.32.0] Ledger: the routes it offers you are now priced against the gold in your purse

e1ff4d3b00d8188a98576ffe577cdf79b4102cdd, 2026-09-05

- The ledger panel now counts the gold in your purse when it works out how many of a good a route is worth, so it no longer offers you 32 of something you can only pay for 3 of.
- Your gold reserve is held back from that count too, the same way it is when TradeLord buys for you, and so is your spending cap for the visit.
- When your purse is what is holding the ledger back, the panel now says so and names your gold and your reserve, instead of blaming your travel ceilings.
- The feature list now says your own purse is counted into every route the ledger quotes.

## [1.31.1] Feature list: getting back up to speed now names haul animals the way the settings screen does

a718d11d9ded14adc39886d0532226c90aab100f, 2026-09-05

- The feature list now calls a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse and a Pack Camel haul animals where it describes getting your party back up to speed, the same name the settings screen and the rest of the list already use.

## [1.31.0] Caravans: trading with one on the road now obeys every rule a market visit does

4a76355f8068de3b700c93435c83b7d083464477, 2026-09-05

- Trading with a caravan on the road now sells the goods you looted when the ones you bought fall short of your profit margin, the way a market does, instead of selling nothing at all.
- Hold cargo for the best market now holds your cargo back from a caravan on the road as well, instead of only from a town.
- A caravan can no longer sell you enough livestock to drop your party into the herd speed penalty, because buying from one is now held to the same herding calculation a market is.
- Stop buying at this many held and Share of the hold one good may fill now hold when you buy from a caravan, not only in a market.
- Economy settling delay now holds on the road too, so a caravan will not trade with you before the day you set either.

## [1.30.3] Settings: installing Mod Options no longer replaces what you set in TradeLord.ini

a91086df3ee1a0982370557d92a718de4c62fcc7, 2026-09-05

- Installing Mod Options after playing without it no longer replaces every setting in TradeLord.ini with the ones TradeLord ships with.
- TradeLord.log now says why your settings file was the one TradeLord read, whether it was saved more recently than the settings screen or was written when there was no settings screen at all.

## [1.30.2] Speed: getting your party back up to speed works again after you load a save inside a settlement

d47b76b163c02f0166f41f74aa91fda4199dccba, 2026-09-05

- Loading a saved game while you are inside a town or village no longer stops TradeLord from selling the animals that are slowing your party down as you ride back out.
- TradeLord.log no longer says a trade was turned down as you leave a castle, a hideout or anywhere else TradeLord never trades in the first place.
- With Buy to fill the ships off, getting your party back up to speed now keeps enough haul animals for what your carts are carrying, instead of measuring against your ships.
- TradeLord.log now says once why TradeLord cannot work out your herd, whichever part of it asked first.

## [1.30.1] Feature list: it now says TradeLord runs on the 1.5.2.121216 beta too

1cca5e09e276860e1409acb3b021de8e8196249d, 2026-09-05

- The feature list and what it needs now both say TradeLord runs on the Bannerlord 1.5.2.121216 beta as well as on 1.4.8.119303.

## [1.30.0] Buying: only the animals that carry for you are bought, and never with the last of your gold

e599494d65a0ffadf0c46e7cd22c6c60ae4fc984, 2026-09-05

- TradeLord now buys only the animals that carry for you, a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse or a Pack Camel, so it no longer spends your gold on a Rouncey or a Charger while a Mule is sitting in the same market.
- It never pays more than the cheapest price it knows of for a haul animal, so Most it will pay for a haul animal when your bags are full is gone from the settings screen and the value you had saved for it is dropped the next time TradeLord reads your settings.
- Restocking food now buys only where the market is asking no more than the cheapest price you know of for it, instead of taking the cheapest thing on the shelf whatever it costs.
- Buying a haul animal and restocking food both stop before your gold reaches your reserve, so there is always something left to trade with.
- Buy haul animals and mounts is called Buy haul animals now, because riding horses and camels are no longer among the animals it buys.
- The hints under Buy haul animals and Restock food (days of supply) say what those settings now do, and the feature list follows them.

## [1.29.1] Speed: leaving a settlement only sells where the game would have let you trade there

8cd281ca01a0691dc167c20807857a3386539721, 2026-09-05

- Getting your party back up to speed as you leave a settlement now happens only where the game would have let you trade there in the first place, so TradeLord no longer sells on the way out of a place it refused to trade in on the way in.
- When it holds back for that reason it says so in TradeLord.log, naming the settlement.

## [1.29.0] Speed: the herd is now checked as you arrive, after trading and as you leave, and every animal that moves is written down

ce1c9db1fd455e55435f0bbcbab729c22a1faf31, 2026-09-05

- TradeLord now looks at your herd three times a visit instead of once: as you enter a settlement, again once it has finished trading there, and once more as you leave, so a penalty that arrives while you are in town is caught before you ride out.
- A herd penalty no longer has to come from picking up an animal: losing men in a battle or to desertion shrinks how much your party can drive, and each of those three checks now notices that on its own.
- While you are on the road with no market in reach, TradeLord.log names the herd once a day for as long as it is slowing you down, so you can see when the penalty arrived and what caused it.
- Every herd check is written to TradeLord.log with the numbers behind it: how many men you have and how many are on foot, how many loose mounts nobody is riding, how many pack animals and how much livestock, how many animals are being driven in all, and how many must go.
- Every animal that comes into your party or goes out of it is now named in TradeLord.log with what was paid, why it moved, and what TradeLord counts it as, whether it came from the selling pass, the buying pass, restocking, the baggage train, a caravan on the road, or getting your party back up to speed.

## [1.28.1] Speed: a party that never puts to sea will now part with a spare haul animal to get back up to speed

43a35a74794c6a2e49325ed39d2abca4031b75f7, 2026-09-05

- Getting your party back up to speed now works out how many haul animals your cargo needs the same way TradeLord works out what your party can carry, so a party that never puts to sea will part with a spare haul animal instead of holding on to every one of them.

## [1.28.0] Speed: your party now sheds its livestock and plain horses before its haul animals to get back up to speed

8e9a493be9e0f7e9d3838c22fbe95b2246f3e096, 2026-09-05

- Getting your party back up to speed now sells in a set order: livestock first, then a spare riding horse or camel of the plain horse kind that nobody is riding, then your haul animals, and your war horses and noble horses last of all.
- It never sells a horse one of your men on foot is riding, because that horse is not in the herd and selling it would not speed you up at all.
- Livestock and haul animals can go this way now, so a party that has picked up more mules than its men can drive is no longer stuck slow with nothing TradeLord can do about it.
- It keeps enough haul animals to carry what your party is already carrying, so getting back up to speed can never leave your cargo on the ground.
- The setting that governs all this is now called Sell animals that slow you down, because spare mounts are no longer the only thing it sells.
- A Saddle Horse is now one of the animals TradeLord buys for your baggage train and keeps out of its ordinary selling, alongside the Mule, the Sumpter Horse, the Work Horse and the Pack Camel.
- A Mule or a Pack Camel that cannot be ridden is no longer kept as a haul animal, so it is now sold like any other cargo.
- The hint under the Language setting now says only that a language change needs the game restarted.
- The animal roll call in TradeLord.log now also says whether the game counts each animal as food and whether it can be traded at all.
- TradeLord.log now names both animals when an entry on one of your item lists means two different animals that TradeLord treats differently, such as Saddle Horse, and gives you the item id for each.

## [1.27.5] Settings: the hint under the language setting is plain English again

0197201b8decdb20510a5b53fa208c1f7a76d99d, 2026-09-05

- The hint under the language setting is back to plain English: it names what TradeLord speaks in and says the town menu entries change when you next load a campaign, without the paragraph about the settings screen that 1.27.4 added.

## [1.27.4] Settings: the language you pick now reaches the settings screen itself without a restart

d0c06b6c0a8402673f33a2c4f48a775da453e54a, 2026-09-05

- The language you pick on the TradeLord settings screen now reaches the screen itself: the name and hint of every setting, the headings above them and the Reset button are all spoken in it as you pick it, instead of staying in the old language until you restart the game.
- The words inside each list of choices follow the next time you open the settings screen, and the hint under the language setting now says as much.

## [no release] Working rules: a harness-assigned branch's work moves to main instead of sitting on that branch

6a9b51e94e663b1bd01bc8b1ec7d3e18c1ebd357, 2026-09-05

- The working rules now say to move the checkout to main when the harness assigns a working branch, rather than committing on that branch and living with a tool that counts every commit as unpushed forever.
- The session hook makes that move by itself when the tree is clean, and leaves the checkout alone and says so when it is not.
- The rules no longer name Bannerlord or any of its own words, so the file carries to another repository for another program unchanged.

## [1.27.3] Settings: TradeLord.ini keeps your settings when Mod Options is slow to load

dd8599db01dffad0c5145e62f0524bb081a6c3e3, 2026-09-05

- Your settings in TradeLord.ini are no longer written over with the shipped ones as the game starts, which happened whenever Mod Options had not finished loading yet.
- TradeLord reads the file as it stands in that case, so anything you set by hand survives, and the settings screen takes over as soon as Mod Options is ready.

## [1.27.2] Settings: Bannerlord no longer closes at the main menu when Mod Options is installed

02d3e478468d554760d34858352411a2577df9f5, 2026-09-05

- Bannerlord no longer closes itself as it reaches the main menu when TradeLord and Mod Options are installed together, which it has done since 1.26.0.
- The TradeLord settings screen comes back with it, Reset button and every setting on it.

## [1.27.1] Settings: a language you pick now takes hold on the screen without a restart

3778a93d439bc9a890549b243cd64cafb4fabe3c, 2026-09-05

- Picking a new language on the TradeLord settings screen now takes hold as you set it, so the choices under each setting are spoken in it straight away instead of waiting for you to restart the game.
- The screen is asked to draw itself again at the same moment, so its own names, hints and headings follow the new language too.
- The hint under the language setting no longer says the screen changes language the next time you open it, because that was not what happened.

## [1.27.0] Log: every animal in your game is now named as a campaign starts, with what the game says about each

5f790d7455b806c77dd457fba91178cd250fde94, 2026-09-05

- TradeLord.log now opens every campaign with a roll call of every animal in your game, counted by what TradeLord does with each one: the haul animals it keeps for your baggage train, the mounts it may sell as spares once they slow you down, the livestock it trades, and anything that is none of the three.
- Each animal is listed with the game's own answers about it, whether it is rideable, a pack animal, a mount or livestock, how much meat it is worth and which trade category it sits in, so a mod that adds an animal or changes what an existing one does shows up plainly.

## [1.26.1] Settings: a damaged settings file can no longer set a number the screen would refuse

790b1b3e43ccb472eb14539706724bd729719960, 2026-09-05

- A number in TradeLord.ini is now held to the same limits the settings screen holds it to, so a file that has been damaged or edited to something impossible can no longer put TradeLord into a state you could never have set on the screen.
- A number that has been pulled back inside its limits says so in TradeLord.log, naming the setting, what the file asked for and what was used instead.

## [1.26.0] Settings: a Reset button at the top of the screen, and the log now names every setting you have changed

329aac1c68656247b2fcd995bacf977f6c832be9, 2026-09-05

- A Reset button now sits at the very top of the TradeLord settings screen, and it puts every setting back to the value TradeLord ships with in one click.
- Resetting empties your never sell, always sell, never buy and always buy lists too, and it is written into TradeLord.ini straight away so nothing is left half changed.
- TradeLord.log now lists every setting you have moved away from the value TradeLord ships with, each time you start the game.
- Changing a setting is written to TradeLord.log as it happens, naming what it was, what it is now, and what TradeLord ships with, so a log you send tells the whole story.
- The button on the settings screen is spoken in the language TradeLord is set to, like the rest of that screen.

## [1.25.0] Settings: TradeLord.ini and the settings screen are twins now, so you can add or remove MCM freely

0c1f431ab8b24009d51ee23a2086119498e8f45c, 2026-09-05

- TradeLord.ini and the MCM settings screen are twins now: both are kept up to date, so you can install or remove MCM whenever you like and every setting you have made comes with you.
- Each of them records when it was last saved, and if the two ever disagree the one saved last wins and the other is written to match it.
- Editing TradeLord.ini by hand while MCM is installed now works, because TradeLord sees the file is the newer one and sets the settings screen from it.
- Changing anything on the settings screen writes it straight into TradeLord.ini, so removing MCM never costs you a setting.

## [1.24.0] Settings: an older TradeLord.ini is now carried forward instead of losing what you set

ca2f8a37adb53d47cab0e26cdb857f018eec1a38, 2026-09-05

- TradeLord.ini now says which shape it is in, and TradeLord brings an older file forward by itself when it changes how a setting works, so a setting you set is never quietly lost.
- Anyone whose file still keeps some of every kind of food as a single number is carried over to the switch and the amount that replaced it, and the old number becomes how many of each kind to keep.
- Anyone whose file still has keeping smeltable weapons as a switch is carried over to the choice of three that replaced it, on becoming keep every one and off becoming sell them.
- Everything TradeLord carries over is written to TradeLord.log in plain words, so you can see what it did with a setting you had set.

## [1.23.1] Changelog: the Old Work Horse was the wrong example to give

4812a9f44e566b62d7fb84385bfeb99e109c0624, 2026-09-05

- The last note about an animal that carries nothing for you should not have offered the Old Work Horse as an example, because a work horse carries for you whatever its age and TradeLord keeps it like any other haul animal.

## [1.23.0] Selling: an animal that carries nothing for you is no longer stuck in your bags

b7d13de4cbaa8789757ac69da1c0bcd7e4cc6150, 2026-09-05

- An animal that carries nothing for you and is not livestock, such as an Old Work Horse, is no longer fenced in with your haul animals, so it is sold like any other cargo instead of sitting in your bags for good.
- The setting for how much more TradeLord will pay while your bags are full is now called Most it will pay for a haul animal when your bags are full, and it is explained in plain words.
- The feature list now says where the line falls between a haul animal TradeLord keeps for you and an animal it sells.

## [1.22.0] Selling: spare horses nobody can ride now go when they are slowing you down

35d0de98ec12a527e48bf1daebe8d9149799ffac, 2026-09-05

- TradeLord now sells a spare mount nobody in your party can ride, once those spares are dragging you into the herd speed penalty, so your footmen turning into cavalry no longer leaves the party crawling.
- It sells the cheapest spare first, so your war horses stay in the baggage, it never sells a haul animal that way, and one switch on the settings screen turns the whole thing off.
- A mule, a sumpter horse, a work horse and a pack camel are now called haul animals on the settings screen and in the feature list, and all four are named rather than described.
- The feature list now says TradeLord pays up to the cheapest price it knows of for a haul animal, and up to 1.5 times that while your cargo is full.
- The livestock policy, the unique and crafted protection and the always-sell list no longer say a mount can never be sold, because now one can.

## [1.21.1] Feature list: it now names the animals TradeLord buys for your baggage train

a3ab1c8e2ab81ed54d524dd980481aceeb15ad20, 2026-09-05

- The feature list now names the animals TradeLord buys for your baggage train, a mule, a sumpter horse, a work horse or a pack camel, and says it buys a horse or a camel your men can ride while you still have troops on foot.

## [1.21.0] Selling: looted gear no longer waits for a better market unless you ask it to

759796ce147aadf0322e23dafc4d55801b1d573a, 2026-09-05

- Looted gear now sells to the first market that can pay for it, and Hold cargo for the best market, which is off out of the box, is now the only thing that makes any of your cargo wait for a better town.
- Asking bandits for free passage is no longer labelled a cheat: the line in the encounter screen now reads [TRADELORD] and its setting is called Free passage from bandits.
- The setting that buys mules and horses now calls them animals rather than beasts.
- The feature list now says how much gold the town you would sell to has rather than what is in its till, names your gold reserve as 300 denars, and says what the settings you get out of the box are aimed at.

## [1.20.1] Trading: the pack animal line now comes after your Trade skill line

d483a1b391d14345c192cfcc5ea717b24550cd48, 2026-09-05

- The line saying TradeLord bought pack animals now comes just after the line crediting your Trade skill, rather than before it.
- The feature list now covers what the ledger panel shows along its top, the warnings TradeLord puts on screen, leaving a village its last of each good, and naming a mount on your always-sell list to sell it.

## [no release] Working rules: they now cover the unpushed-commit warning, checks that fail on moved source, a run that looks stuck, and renamed settings

faa3243b0af125efb2fec535a6062d50d82e3183, 2026-09-05

- The Branch rules say what to do when a tool keeps reporting unpushed commits on the assigned branch.
- The Verification rules say to rewrite a check onto the moved source rather than loosen it, and to read the clock before calling a run hung.
- The Changing the source rules say to keep a setting's name and kind of value where the program stores its settings in a file.

## [1.20.0] Trading: it now deals with caravans on the road, and free horses get bought

09d8996d86d9dbe6e6a4c095526c855187406fad, 2026-09-05

- TradeLord now trades with a caravan you meet on the road: talk to it and the deal is already done, and the caravan pays out of its own purse.
- A new setting caps how much of your hold one good may fill as a share rather than a flat count, so the ceiling grows with your carts instead of needing a new number every time your party does.
- A horse one of your unmounted men can ride no longer counts against the herd, so a party driving a full herd still buys the cheap horses its footmen can climb onto.
- The feature list covers trading with caravans and the new share of the hold.

## [1.19.0] Selling: smeltable weapons are now a choice of three, and cheap horses get bought too

8a34148cae21f9ddd88583cad28bb7346707ac88, 2026-09-05

- Keeping smeltable weapons is now a choice of three, and the new one keeps a weapon only while a part of it is still locked in your smithy, so your bags stop filling with loot that can teach you nothing.
- Buying beasts now covers riding horses as well as mules and sumpter horses, so a cheap horse gets bought too and your foot troops have something to ride.
- Asking bandits to let you go is now on out of the box, and one switch turns it off.
- The feature list explains the three ways to play the smeltable weapon setting.

## [1.18.0] Trading: it now buys mules for the baggage train, and bandits can be talked out of a fight

09e7f6b05c5e9a857cc3970a881ae1e098d5d70c, 2026-09-05

- TradeLord now buys mules and sumpter horses when a market is asking no more than one is worth, so your party can carry more, and it will pay half again as much for one while your cargo is full.
- It never buys more pack animals than your party can drive without slowing down, and your gold reserve and your spending limit for the visit still hold.
- Restocking now tops your food back up to three days of supply instead of five.
- A new cheat, off until you switch it on, lets you ask looters, sea raiders and the rest of Calradia's bandits to let you go, and they will, with no fight and no ransom.
- The setting that keeps smeltable weapons now says plainly what it holds back: every weapon built from smithing parts, looted ones included, whatever parts you have already learned.
- The feature list now covers what TradeLord buys for your baggage train and what it leaves alone, and what it needs installed has moved to the foot of it.

## [1.17.0] Trading: it now keeps your party fed, covers the wages and can be set up without MCM

99d500b9863257178f8400f765b88775deec0b6c, 2026-09-05

- TradeLord now tops your food back up to five days of supply as you trade, buying whatever food a market has going cheapest at whatever it asks, before it trades for profit.
- It now holds back three days of your troops' wages on top of your gold reserve, so a shopping trip never eats the payroll.
- A new switch keeps every weapon the smithy can break down for parts, so a smithing playthrough stops selling off its own raw material.
- A new switch sizes what it buys to what your ships can hold rather than what your carts can, for a War Sails fleet.
- The ledger panel's cargo line follows that same capacity, so it counts the ships whenever that switch is on.
- Without MCM installed, TradeLord now writes a TradeLord.ini beside its log that you can edit to change any of its settings.
- The notice about trading as you arrive now points you at that file when MCM is missing, instead of telling you to go and install MCM.
- Default in the settings screen now puts every setting back to the one TradeLord ships with, instead of leaving them exactly as they were.

## [1.16.1] Selling: the food switch now names the food variety morale bonus as the game does

44b61dda77824091baf1311837105d85756feb16, 2026-09-05

- The switch that keeps every kind of food now names the food variety morale bonus the way the game does.

## [1.16.0] Selling: keeping every kind of food is now a switch, with its own amount

9161a82f521881fcaebd5ae1dc2da19ce8f5cf70, 2026-09-05

- Keeping some of every kind of food is now a switch you turn on, with how many to keep set separately and starting at three of each.

## [1.15.0] Selling: it can keep a share of every kind of food so your party keeps its variety bonus

9dced9ec9e851204fe9d9f562286b9c042106ed5, 2026-09-05

- TradeLord can now hold back a few of every kind of food you carry, so selling your stores no longer costs your party the morale bonus it gets from eating a variety of things.

## [no release] Compatibility tool: it reads game assemblies through the current metadata reader

131b45caee27ca48a50a769fd9ae59c26507e2a6, 2026-09-04

- The compatibility tool now uses System.Reflection.MetadataLoadContext 10.0.11 instead of 8.0.0, which the package index reported as out of date.

## [1.14.5] Ledger panel: its headings now change language with the rest of TradeLord

708a664fd392a940e032f59af58dc6a24b08df08, 2026-09-04

- The ledger panel's title, buttons and column headings now follow the language you pick, instead of staying in the one they were first drawn in.

## [1.14.4] Trading: it no longer blames the market when your own cargo was the reason nothing sold

92baee04afbcab928087631e3199ae20cd5dcece, 2026-09-04

- TradeLord no longer says a market has nothing worth trading when the reason nothing sold was your own cargo.

## [1.14.3] Trading: a market that cannot pay is no longer the best place to sell

bdf503adfb5521a6ae1689bbc6f71b45228afc52, 2026-09-04

- A market whose merchant has run out of gold is no longer named as the best place to sell, so TradeLord stops buying cargo for a town that cannot pay for it and stops holding goods back for one.

## [1.14.2] Buying: it no longer buys what your own settings will never let it sell

68bea35dc0fa49935f0011f50b67cc06fd7711f1, 2026-09-04

- TradeLord no longer buys a good that your own settings will never let it sell, so a category you set to buy only no longer fills your cargo with goods it will not move on.
- The ledger panel opens faster, most noticeably in a long campaign with many markets in reach.

## [no release] Release workflow: the notes come from the changelog rather than the last commit

ed47b9adeacfa42557cb69712699214ce0770493, 2026-09-04

- The publish step read the release notes out of whichever commit sat at the head of the push, so pushing a version commit together with a later one published the wrong commit's body as the notes. It now reads the changelog section for the version being published, which is the same text either way.
- The paste tool that turns a changelog section into Nexus entries can now emit that section as release-note bullet points, which is what the workflow uses.
- A rule holds the notes to the changelog section, and the rule that held them to the commit body is gone.

## [no release] Repository: the feature list names what TradeLord needs installed

a3ef0d72c0df92dbd54391b139d5cffe425d254a, 2026-09-04

- The feature list opens with what TradeLord needs: Harmony, which it does not load without, MCM for the settings screen, and the game version it is built for.
- The line about holding cargo for the best market says that goods you never bought are always held back, and only bought goods follow the setting.
- The line about a sale nobody can pay for names the town you would sell to, rather than calling it the buying market.

## [1.14.1] Ledger panel: it no longer opens on its hotkey while you are typing

d2abd3ca3d63b79d6942d885f4155669f0b928bf, 2026-09-04

- The ledger panel no longer opens on its hotkey while you are typing into a box on the campaign map.
- The best-market tolerance hint now says the price floor it sets always holds back goods you never bought, such as looted gear, and not only when the setting above it is on.

## [1.14.0] Language: TradeLord now speaks Simplified Chinese

df0667fcdfbd0b092ed90e9f09ba3ec16f661bba, 2026-09-04

- TradeLord now speaks Simplified Chinese, chosen from the same Language setting that already offered English, Turkish and Russian.

## [1.13.3] Automation: the notice about trading as you arrive is shorter on screen

156d6309a232ff5bacab37b24ce9a079f1cea513, 2026-09-04

- The notice that TradeLord trades as you arrive is shorter, so it sits on screen like its other messages instead of running long.

## [no release] Repo: the source checks now hold the scan, the route walk and the panel

381adee5dd75574a2c8067c9cfb92e7b21429d9a, 2026-09-04

- The check that the dearest market is the one to sell at, and the cheapest the one to buy at, was missing entirely.
- New checks hold the scan to the stock floor, the village travel ceiling and the shelf life it was given.
- New checks hold the unit-by-unit route walk to the merchant's till and the spending cap.
- New checks hold travel time to counting the sea leg, and to refreshing once the party has moved.
- New checks hold the panel to handing back its movie and the mouse, and to honouring the modifier keys.

## [1.13.2] Automation: the first-market notice no longer sends you to settings you do not have

cd24b53c47440be22f127ebe3b07bad3fc445adb, 2026-09-04

- Without MCM installed there is no settings screen, so the notice that TradeLord trades as you arrive now says that plainly and names MCM, instead of sending you to settings you do not have.

## [no release] Repository: the feature list drops the sales pitch and the old save story

59d91fd1eeace58a14beaa7719ed195c970a0834, 2026-09-04

- The opening description says what the route ranking actually weighs, in plain sentences, instead of counting unnamed factors.
- The line about installing and removing the mod no longer compares it to other mods, and no longer retells the save bug it fixed long ago.
- The grain line and the closing line read properly, and the closing line says that changing settings needs MCM.

## [no release] Repo: the source checks now hold the trading guards that were going unread

15e90ac5817f4df1bf365a5149c9a85ceec4c8ff, 2026-09-04

- A rule that claimed to check a dry run kept to the same-stop rule checked nothing of the sort, and now reads both halves of a real pass.
- The rule for markets at war now reads the gate that decides it, rather than any mention of it elsewhere in the file.
- New rules hold selling to the merchant's till, and buying to the purse, the per-item denar cap, the carry weight and the herd.
- A new rule holds both passes to stopping when a trade moves gold the wrong way.
- New rules keep buying to goods a market in reach pays more for, leave unique and player-crafted gear out of a sale, wait out the settling delay, and hold a route's whole trip inside the travel ceiling.

## [1.13.1] Trading: the map marker no longer points at a town that cannot pay

0e791bb27787e0023cb15ceec5f180dd6ffdbf00, 2026-09-03

- The best-market marker no longer points at a town whose merchant has no gold left to pay for your cargo.
- TradeLord no longer warns that it cannot buy at a market when you have turned both auto buy and its town menu trade entry off.
- A panel hotkey with something other than Ctrl, Alt or Shift in front of the key now says so in the log, rather than quietly opening on the bare key.
- Walking into a market is a little quicker when your never-sell, always-sell, never-buy and always-buy lists are all empty.

## [no release] Repository: the feature list catches up with the last four releases

a6e0b8abb3488d5c9cea63e3b0f345fbb6e0f4d0, 2026-09-03

- The save paragraph counts three strings, which is what the source actually writes.
- The ledger panel line names its town menu entry alongside the hotkey and the map button.
- The reporting line says a market that traded nothing answers in one line.
- The hideable list covers the two town menu entries as well as the map button.
- The feature list names the three languages TradeLord speaks and what the Language setting drives.
- The workshops line and the settings heading are reworded.
- A check ties the save paragraph to the fields the source saves, so the count cannot drift again.

## [1.13.0] Settings: selling and buying now sit together at the foot of the screen

36e6d30584e6ae235ccc8abdf5b5196ce50546f4, 2026-09-03

- The settings screen now ends with Selling and Buying, side by side, with every other setting above them.
- The settings that shape both halves of a trade, along with the two town menu entries, have moved into General.
- The two switches that trade as you arrive are now called Auto sell and Auto buy.
- The separate switch that decided whether TradeLord could buy at all is gone, so buying now follows Auto buy and the trade entry in the menu alone.
- The note under the food policy no longer says where on the screen the food reserve sits, since it has moved.

## [1.12.1] Trading: a market that traded nothing now says so in one line

3e663724200e5a4245707852f11440e66706d206, 2026-09-03

- A market where TradeLord traded nothing at all now says so in one line instead of two, naming what stopped it.
- A market where TradeLord did trade no longer also tells you the other half of the pass moved nothing.

## [1.12.0] Trading: a market that traded nothing now says why, and TradeLord speaks Russian

384f996619b93ce62f1dd9e41bf132ee0d91ef51, 2026-09-03

- TradeLord now tells you on screen why it traded nothing at a market, when it trades as you arrive, instead of writing the reason to its log alone.
- Turning quiet automation on keeps those new lines off the screen along with the rest.
- TradeLord now speaks Russian, chosen from the same Language setting that already offered English and Turkish.
- A new setting hides the TradeLord ledger entry in the town and village menus, the way the trade entry could already be hidden.
- The ledger panel has dropped its Data column, and its item, buy town and sell town columns are wider for the room.

## [1.11.1] Routes: markets in reach follow a change to your ships straight away

90536c99cdf78ae07116b33728eda015170943c3, 2026-09-03

- Gaining or losing ships now changes which markets TradeLord counts as being in reach straight away, instead of leaving some of them out until the hour turns.

## [no release] Repo: the paste check reads the tool's output whatever line endings the runner gives it

4b6bd94a4ad1c65b391ea623a84d62068948aa36, 2026-09-03

- The check comparing the paste text against the changelog split the tool's output on newlines alone, so on Windows every line came back carrying a carriage return and the check failed on a run that was otherwise clean.
- It now reads the output a line at a time, which holds whichever line endings the machine running it uses.

## [no release] Repo: the source checks read braces properly, and the changelog can be turned into Nexus entries

b7dd810749e60cf802794f4b3a5bcb11bc0d8132, 2026-09-03

- The source checks no longer mistake a brace inside a string, a character or a comment for the end of a method, so a rule reading such a method reads all of it.
- A new tool prints any version's changelog entries in the shape the Nexus mod page takes, one entry per line with the heading and the bullet marker dropped.
- Three checks cover it: every changelog entry stands on one line, the printed text matches the entries for the shipped version, and a version the changelog does not carry is refused.

## [1.11.0] Settings: named choices instead of number sliders, and an always-buy list

2bc1c31b6934fb099ff0f2309003da33e81b57d1, 2026-09-03

- The food, smithing material and livestock rules are now picked from a list that names each choice, instead of a slider you set to 0, 1, 2 or 3.
- The setting that decides what a good counts as having cost you is picked from a list too, and it and its note are written in plain words now.
- Selling looted gear now starts switched on at tier 1, which is what looters and bandits drop, and you can still raise or switch it off.
- A new always-buy list matches the always-sell one: name a good on it and TradeLord buys it past the category rules and past the never-buy-grain switch.
- Your never-sell and never-buy lists and anything you locked in the inventory still hold, and it still buys only what it can sell on for more somewhere in reach.
- Switching those four settings over to lists keeps whatever you had already picked, so nothing resets.

## [1.10.0] Settings: the screen now reads in the language TradeLord is set to

6e7d50d370992d4d63a66158e9489d3e06e58973, 2026-09-03

- The settings screen now reads in the language you picked in TradeLord's own Language setting, instead of the language the game itself is set to.
- The screen takes a new language the next time you open it, and the note under the Language setting now says so.

## [1.9.1] Ledger panel: it now opens with T, since the game already uses L

cf38cd580f66fdd40c4082ceffa0667beb98e349, 2026-09-03

- The ledger panel now opens with T instead of L, which the game already uses for one of its own screens.
- The key is still yours to set in TradeLord's settings, and one you have already chosen there is left alone.

## [1.9.0] Settings: the switches say plainly when TradeLord sells and buys

0533cb999cf18441646be068449b861333c0c4fa, 2026-09-03

- The switches that decide when TradeLord sells and buys now say plainly what they do, instead of naming quick-sell and quick-buy entries the town menu no longer has.
- Every other setting that still talked about quick-sell and quick-buy now names selling and buying instead, in English and in Turkish.
- The auto-trade switch is gone, since it only read back the two switches above it and turned them on and off together.

## [1.8.0] Buying and menus: a cap on how many of a good you will carry, and one trade entry instead of three

7460781312ae5e8b587231fbb88e32ac53506a76, 2026-09-03

- A new setting stops quick-buy adding to a good once you already carry as many as you allow, so a stock of hundreds no longer grows every time you walk into a market.
- The town and village menus now carry one TradeLord trade entry instead of three, and it sells and then buys in one go.
- Trading done automatically as you arrive is unchanged, and the setting that showed the old entries now shows or hides the single one.

## [no release] Repo: the session signature is pinned to the working rules instead of guessed from the history

8678df047da2baeafe9eb28b4fcf9643c681e662, 2026-09-03

- The hook that sets the signature at the top of a session read it off the newest commit in the log, so the private address the owner no longer uses could come back into a commit any time it sat at the top.
- It now takes the signature from the one line in the working rules that carries it, and reads nothing out of the history at all.
- Where that line is missing it sets nothing and says so, leaving the rule that refuses to commit as Claude to catch it.

## [no release] Repo: the guard reads the git subcommand rather than any later word

abc8c5aab0eb1ab989eb6e60b9351a6ac849937e, 2026-09-03

- The guard that keeps this repository to one branch used to look anywhere after the word git, so a redirection on the end of a command, or the word branch inside a commit message, read as the name of a new branch and turned away work that was going to main.
- It now reads only the subcommand git was actually given, and a table of 57 commands covering both what it must refuse and what it must let through was run against it.

## [no release] Repo: one branch, one signature, and what a lot cost you moves where a check can reach it

0ffc299b54c7afcfadccb7023a7e3b6dea9c183c, 2026-09-03

- The working rules now keep one branch, main, and a git command that would start a second one, locally or on the remote, is refused before it runs.
- Every commit is now made under the owner's noreply address, which the rules name in one place and nowhere else.
- What a lot of goods cost you now sits beside the other money rules, unchanged, where the build can test it, and eight new tests cover it.
- A change that moves working code without altering anything a user sees now says so, ships no version, and the release gate lets exactly that through and nothing else.

## [no release] Source checks: a rule that names source the mod no longer has now reports itself instead of ending the run

f03e38b0eb22b5a3fc73118312d3d690cc142bb5, 2026-09-03

- A source check naming a method the mod no longer carries used to stop the run where it stood, leaving every later check unread.
- Such a check now reports itself as broken, and the run reads every rule to the end.
- A check worded as an absence no longer passes by default when the source it reads has gone.
- A new check holds that behaviour in place.

## [1.7.0] Language: TradeLord speaks Turkish, and a setting at the top of its options picks the language

56397d7a7729d7cb8313987c8b0e9b9458fd2910, 2026-09-02

- TradeLord now speaks Turkish. A new Language setting at the top of its options switches what it says in the game, and it starts on English.
- Playing Bannerlord in Turkish now puts the whole of TradeLord into Turkish, its settings screen included.

## [1.6.33] Settings: the screen is built against a newer Mod Configuration Menu

be2a2b8533eb0fad7ccbf47531df49ef5037744a, 2026-09-02

- The settings screen is now built against Mount and Blade Mod Configuration Menu 5.12.3.

## [1.6.32] Item lists and routes: a list entry catches only the good you named, and route quantities keep to your denar caps

5c84da0c1dbcd4b98dda5aa334d54a87bfd284eb, 2026-09-02

- Naming a good on the never-sell, always-sell or never-buy list no longer quietly catches a second good whose whole name is one of the words you wrote, so putting Iron Ore on a list leaves Iron alone.
- The ledger panel now spends your per-item and per-visit denar caps unit by unit, the way a buying pass spends them, so a route no longer offers a quantity your caps would stop you buying.

## [no release] Repo: the working rules say the version has three parts

4486dfe000b598f45ef486c7fa163cbcc797a4e0, 2026-09-01

- The working rules now say the version has three parts, major.minor.patch, and which part to raise for a rework, for a new feature and for a fix.
- A version in the source with fewer than three parts is written out in full for the next number to ship, and a number already released keeps the form it went out with.
- A first release now starts at 1.0.0 where the source carries no version of its own, rather than 1.0.

## [no release] Repo: the working rules are the new ones, and the release gate and the checks follow them

90387b482bb23490a01f196c0a71827eae29648e, 2026-09-01

- The working rules are replaced with the current version, which adds a verification section, a third release automation state, a section on changing the source, and a commit signature to fall back on.
- The session hook now reads the owner off the most recent commit he authored rather than the first commit in the log, and falls back to the signature the working rules name when the history holds none of his.
- A push is now refused when its commit changes what a user gets and leaves the changelog untouched, and a commit touching only the checks, the workflow and the working rules is let through.
- A version is no longer published while the changelog still carries an Unreleased heading, or carries no section of its own for that version.
- The changelog may now open on an Unreleased heading, and that heading has to say something.
- The rule against comments is now read off every tracked file with a tokenizer rather than the start of a line, so a trailing comment, a block comment, and one in a file that was never looked at before are all caught.

## [no release] Repo: the working rules match the other repositories

d7321aa8812b76c1cac18e55635c82e34544e355, 2026-08-31

- The working rules are now the same file in every repository, covering release automation, the changelog, the version and dependency upgrades.

## [1.6.31] Map markers: the cargo marker names a town that can pay for the load

4b85d7dc7903e6d860c4c2b8f6cf4a1123da8cc4, 2026-08-29

- The town marked on the map for your cargo is now one whose merchants can actually pay for it, instead of one that quotes a high price but has run out of gold.
- A map pin that cannot be put back when a save loads no longer takes the TradeLord entries out of the town menu with it.

## [1.6.30] Messages and travel: a steadier reason when a pass trades nothing, and quicker travel times

73e1fd6ddffae642398e15ec0f52792e10539f85, 2026-08-29

- A pass that trades nothing now gives the same reason every time, instead of naming a different one from visit to visit when two rules held back as much as each other.
- Working out how far away a market is no longer re-reads your party speed for every market measured, so the ledger panel and the price tooltips open with less of a pause.

## [no release] Repository: the feature list says smithing materials can be kept out of trading

f6a6d25afd5cf03090b1c8bc0baac20c08cb22f7, 2026-08-28

- The feature list now says that iron ore, ingots, charcoal and hardwood can be kept out of automated trading, off by default.

## [no release] Repository: the feature list says holding cargo for the best market is off by default

589b51aba7777a45537e3485124ed654e2f20a73, 2026-08-28

- The feature list now says that holding cargo back for the market that pays best is off by default.
- An empty bullet at the end of the settings list is gone.

## [1.6.29] Buying: TradeLord says when your purse is under the gold reserve

b6f8ddc2d464bf309076dabebb536cc48be368ce, 2026-08-28

- TradeLord now tells you as you enter a market when your purse is at or under your gold reserve, so you know why it is buying nothing.
- A pass that bought nothing now names your purse or your spending caps as the reason whenever that is what stopped it, instead of blaming prices or saying nothing at all.
- The cargo-full warning stays quiet when the purse warning has already been given, since selling clears both.

## Revise trade route ranking description in README

8f12eff64c1ca31413700f6263f5f1784aebd39b, 2026-08-29

Updated the wording for clarity and improved readability in the README.

## [no release] Repository: the README opens on five lines and ends on the feature list

1d39ade76119ffca16ef2164b0d269755bad3c3f, 2026-08-28

- The README now opens on five lines saying what TradeLord reads, shows, ranks and does for you, in place of the title, the pitch and the download link.
- The getting started section is gone from the end of it.

## [no release] Repository: the download line stands on its own, and the working rules drop the two one-off cleanses

efdc17602a4c097f52efa957bbe11cef53f46056, 2026-08-28

- The download link at the top of the README no longer carries the single-player and game version notes beside it.
- The two rules added for the version and save-note cleanse are removed from the working rules, since that cleanse is done.

## [no release] Repository: the mod names one game version, and the changelog carries no note about a broken save

0def8d1500abc5c19ad443f222836216bc476b84, 2026-08-28

- TradeLord names Bannerlord 1.4.8.119303 and no other version anywhere, and no longer says it also runs on any other.
- The two changelog notes about a campaign refusing to open are gone, since the mod goes in and comes out of a campaign freely.
- The working rules now hold both of those, and ask for commit bodies written as bullet points.

## [no release] Repository: the log line names the file and nothing else

ba20e2b4d5822fb91190f423bf61cd43ef99cd00, 2026-08-28

The feature list now names TradeLord.log on its own, and asks for it when something happens.

## [no release] Repository: the feature list says how the save was made safe, and points at the log

531d89ecf0b31d69385f3f577149ad3867554d1e, 2026-08-28

The save bullet now names the type definer that used to be written into a save, the two classes it declared, and what is written instead.
The confidence score says the stock it counts is the stock of the market you buy from, and that caravans count whether they are at those towns or heading for them.
The gold counted against a sale is named as the buying market's.
The log line says where the file is and asks for it when something goes wrong.

## [no release] Repository: the feature list leads on the save, and says how travel time and confidence are worked out

954cec3313bcb249b4fdb566812bee4feb5cce1e, 2026-08-28

The feature list now opens on installing and removing the mod at any point in a campaign, and says why a save cannot break: nothing of the mod's own making is written into one.
Travel time now says it comes from the game's own pathfinding at the party's real speed, rather than a straight line across the map.
Livestock trading and the herding check against party size are named.
The confidence score now names what discounts it, including the caravans working the two towns.
The honest-merchant mode says the prices are ones recorded market by market, and the dry run says it shows an estimate.
The list closes on every feature being a switch or a number you can change.

## [1.6.28] Automation: the cargo full warning is given once, on the way into a market

45fbf40eb9c23d7d273458786b9525a535befcad, 2026-08-28

The warning that your cargo is full is now given once, as you enter a market, instead of a second time as you leave it.

## [1.6.27] Item lists: a list that still names nothing after a correction says so again

753e4e6dab9fd0015bd024b91745b853bbc8df11, 2026-08-28

An entry on one of your item lists that still matches no good after you correct it now says so on screen again, instead of going quiet after the first warning.
Changing a setting no longer sends TradeLord through every good in the game when your never-sell, always-sell and never-buy lists are all empty.

## [no release] Repository: the source-check tool now names a rule that breaks instead of stopping at it

8720c810e4435c51fd75c03651307bca10adff96, 2026-08-28

A rule that compares where two pieces of source sit relative to each other used to fail by raising, which ended the run at that point and left every rule after it unread, so one fault could hide the rest.
Such a rule now reports itself as broken and the run carries on to the end.
Removing a fragment one of those comparisons depends on ended the run early in twenty-two of thirty-eight trials before, and in none of them after; the worst case had left all but twenty-nine unread.
Nothing that ships with the mod changed.

## [no release] Repository: the feature list says how to keep a good out of trading, and the working rules ban the em dash

1e4d5a614cefc6e348c133f98249c6de1bfefcb5, 2026-08-28

The feature list now says that a good can be kept out of trading by its item id or by the name the game shows, written in any capitalisation.
The working rules forbid the em dash and the en dash anywhere in the repository, and name what to write instead.

## [1.6.26] Item lists: a good can be named the way the game names it, and a name that matches nothing says so

e91e74df9330391cb7a9453832521d83d41b1cf0, 2026-08-28

The never-sell, always-sell and never-buy lists now take the name a good is shown under, such as Iron Ore, as well as its item id, so a name with a space in it no longer reads as two entries that match nothing.
An entry on one of those lists that matches no good in the game is now named on screen and in the log, instead of quietly doing nothing.
A new quiet automation setting keeps trading done on entry to the log, off your screen.
Starting a second campaign without closing the game now logs the ledger panel's hotkey and map button again, rather than staying silent about the second one.

## [no release] Repository: the licence and the issue template are gone, and nothing is kept for convention

1b80056eba2025fc488f11066aee3996a7c11e9e, 2026-08-28

The MIT licence file and the bug report template are removed from the repository.
The working rules now refuse to add a file because convention expects one, naming the licence, the issue and pull request templates, the contributing guide, the code of conduct and the documentation folder.

## [no release] Repository: the README is a feature list and three lines on getting started

4be8629d64b2bb440b917a91843dea005951bc0e, 2026-08-28

The README now names every feature the mod has, one line each, including the ones it had never mentioned: the merchant's purse, the herd limit, hostile markets, the rumour block, the message filter, the coin sound, the settling delay, the cost basis, the category rules, the buy caps, the scan radius and the loot tier.
The example tooltip and panel, the install steps, the ten-minute walkthrough and the defaults table are gone, replaced by three short paragraphs on installing it, what it does out of the box and where to start.

## [no release] Repository: the README lists every feature and stops sending readers elsewhere

d50d69a1e7d948f785404915e03f7e04eacb11f8, 2026-08-28

The README now opens on every feature the mod has, one line each, and shows what the tooltip and the ledger panel look like straight after.
The requirements table, the reading list, the bug-reporting section and the licence section are gone from it, and the separate settings, troubleshooting, compatibility and build pages are gone with them.

## [1.6.25] Ledger: an empty panel now names what is actually holding your routes back

3b8ca666e4a0096de6efa8f9019896eb35a1a134, 2026-08-28

The message shown when the ledger has no routes for you now points at your travel ceilings, and only asks you to visit more markets when TradeLord is set to use the prices you have seen in person.

## [no release] Repository: the documentation stops saying TradeLord remembers prices it reads live

0f5dc11a4fa3b93d12db7792f222e45eefb409a4, 2026-08-28

The README, the settings guide, the compatibility page and the troubleshooting page said TradeLord builds its knowledge up by visiting markets, when on its default setting it reads every market on the map live, and they now say so.
An empty ledger panel is now explained by your travel ceilings rather than by how few markets you have visited.

## [no release] Repository: the README says which market the starred tooltip line is

c69a674cdbf395934125d3be37d81c6d806961cf, 2026-08-28

The example tooltip now says that the starred line is the market you are standing in.

## [no release] Repository: four settings pages say what the setting actually does

fa66a7dbe8277bdf5a560e826da041043a30d2e4, 2026-08-28

The stock threshold notes that it applies to live prices only, the dry run says to read its result as a best case, the villages switch says what it turns on, and the trade summary says it names the goods rather than counting them.

## [no release] Repository: the README is a welcome, and the reference material moves into docs

2d631ee0ef7f3b26cea200b90cf67731714bb253, 2026-08-28

The README now opens on what TradeLord does for you, shows what the tooltip and the ledger panel look like, and gets you installed and trading in ten minutes.
The settings reference, troubleshooting, compatibility and build instructions each move to a page of their own under docs, linked from the README.
The panel hotkey in the walkthrough is L, the key the panel actually uses.
The note about campaigns saved by older builds is gone.
A bug report template asks for the four things a report needs, and points at the troubleshooting page first.

## [no release] Repository: the purse rule, route confidence and item lists are now tested

b157092c030f6c57496458d8e9c09baf259da234, 2026-08-28

The money path had 37 executable tests over the margin arithmetic and none over the
three rules beside it. The gold-reserve and per-visit budget calculation moves out of
a local function in the buy pass into TradeMath.Budget, which also collapses the two
sim/real branches that were computing the same thing twice. Confidence moves out of
Market.cs into a file of its own; it never touched a game type, only Math.

That makes all three checkable, and 25 new tests cover them: the reserve is held back
from every purse and spending can never cross it, a visit cap of zero means no cap
rather than no spending, the tightest limit binds, confidence stays inside its own
range across every combination of eight inputs and falls with distance, caravans and
stale prices, and an item list is read whatever its punctuation or casing and is
re-read when edited rather than served from a stale cache.

Each of the eight ways these could break was tried against the suite and every one
turns it red. 112 tests, 278 source checks, and the compiled surface is unchanged at
102 types and 200 members. Nothing that ships with the mod changed.

## [1.6.24] Automation: the market that carries the automation notice is left alone

385e9557ccaa75d0bdd23bdca7690b9c497074dd, 2026-08-28

The first market of a campaign is now left alone when TradeLord tells you it trades on entry, so you can turn that off before it does anything, and it starts trading from the next market.

## [1.6.23] Game version: TradeLord is now built against Bannerlord 1.4.8

ae7ba6d68685d7e37cfec63482b88dd4405d3d8c, 2026-08-28

TradeLord is now built against Bannerlord 1.4.8.119303, the version it is played and tested on, and it still runs on 1.4.7 and the 1.5.1 beta.

## [no release] Repository: the compatibility tool can read menu ids from a game install

c14735b06724a4f8967c90ffe294a9cbf0652a7b, 2026-08-28

The four menu ids the mod hangs its entries off are string literals, and the NuGet
reference assemblies ship an empty string heap, so this method could never answer
for them. The shipped assemblies can: TaleWorlds.CampaignSystem.dll for game 1.4.8
carries a user-string heap of 13,694 entries with town, village and port_menu each
in it exactly once.

Point TRADELORD_GAME_BIN at an install and the tool now walks every assembly under
it, reads each user-string heap, and reports which assembly holds each id. An id
the mod does not guard, town or village, appearing in no assembly is a break, since
the entries would silently stop showing; a guarded one is a note, because a menu the
game does not have is skipped and logged by design. With the variable unset the
check is skipped and the run is unaffected, which is how it behaves in CI. A path
that is not a directory fails rather than passing quietly.

The game assemblies stay outside the repository. They are TaleWorlds' to distribute,
so the tool only ever opens them to read, and .gitignore now refuses *.dll to stop a
stray copy being committed by accident. Nothing that ships with the mod changed.

## [1.6.22] Panel and settings: the faults the first play test on game 1.4.8 turned up

bacfcf14a4ea120d2f79a0f34254f395efc9d0b8, 2026-08-28

The ledger panel now opens with L instead of T, because T also opens the game's own message panel and the two fought over the key.
Buy cap per item can now be set to 0 to turn the cap off, the way the two settings beside it already could.
Scan radius, observation shelf life and buy cap per item now say on the setting itself what 0 does.
A market where TradeLord bought nothing because your gold reserve or spending cap was reached now says so, instead of trading quietly and leaving you to guess.
The item tooltip no longer puts a TradeLord heading above the prices it adds.
The line of notes under the ledger is now large enough to read.

## [no release] Repository: the 1.4.8 document carries the result of the test pass

c65f2ead6f000190e7a67ed39e10d912990d3d5f, 2026-08-28

The pass was walked on game 1.4.8.119303 with v1.6.21 and every test passed, so
each test now carries its result instead of "not yet run", and the verdict says
so at the top rather than treating the question as open.

Three of the four remaining questions are settled outright. The patched methods
behave, the panel drew with every borrowed brush present, and the settings screen
registered and rendered. The fourth is settled for the base game from the shipped
TaleWorlds.CampaignSystem.dll, whose user-string heap holds town, village and
port_menu once each; naval_storyline_virtualport belongs to War Sails, and every
NavalDLC reference assembly on NuGet ships an empty string heap, so that one id
can only be confirmed against a real install and stays guarded.

Three faults came out of the pass, none of them compatibility faults. They are
named under the tests that found them and fixed in the release that follows this
commit. Nothing that ships with the mod changed here.

## [no release] Repository: the 1.4.8 document stops quoting figures it no longer measures

113f59a98ff11bb3b462ac7408284d74bf67fb34, 2026-08-28

Four claims in the document were taken at v1.6.9 and had aged. The enum comparison
covers eight enums, not seven, since InputKey is on the list the compatibility tool
walks. The panel finding now reports what is actually re-checked on every run, the
nine widget classes, all of which resolve on 1.4.7, 1.4.8 and the 1.5.1 beta; the
435-attribute walk behind it was a one-off, the prefab now carries 450 attributes,
and the document says so rather than implying the walk still holds. The build-output
comparison keeps its argument but is marked as a v1.6.9 measurement, because the mod
has grown since and the argument never rested on the byte count. The assembly count
now separates the 81 assemblies in the packages from the 75 game assemblies the tool
reports, which had read as a contradiction.

Nothing that ships with the mod changed.

## [1.6.21] Messages: no full-cargo warning after a buy, and one skill line instead of two

b2c38dc40a51c61e567047302df57a5fc89b2e15, 2026-08-28

The cargo-full warning now appears only on a visit where TradeLord traded nothing, instead of following a pass that had just bought until the hold was full.
Profit credited to your Trade skill is reported in one line instead of two, and that line names your new Trade level when the skill rises.

## [no release] Repository: the 1.4.8 document settles the bound-surface count it disagreed with itself on

e6a5cc78179c6c15bfec2e564ce0a0ad0ee0a4b1, 2026-08-28

One section put the mod at 100 game types and 198 members, another at 103 and 201, both describing
the same v1.6.9 build. An earlier version of the compatibility tool counted members differently, and
the two sections were written either side of that change. Rebuilding the v1.6.9 assemblies from the
tag and running the current tool over them gives 100 types and 192 members, resolving on 1.4.7,
1.4.8 and the 1.5.1 beta alike, so both sections now carry that and say where it came from. Nothing
that ships with the mod changed.

## [no release] Repository: the 1.4.8 document records the 1.6.20 compatibility run

f492ef297b8342dd9342ff6b444e5bd2aef118ae, 2026-08-28

The compatibility tool was run again on the 1.6.20 build against 1.4.7.117484,
1.4.8.119303 and 1.5.1.120547-beta, and the document now carries that result
rather than the one taken at 1.6.9. It also records what the save change did to
the bound surface: 103 game types and 202 members became 100 and 198, and
diffing the two builds' reference tables accounts for the whole difference as
the save definer and the saveable-field attribute going.

Dropping the definer closed one of the five open questions outright, since the
mod no longer claims a save type-definer id range for a vanilla definer to
collide with, so that row is struck from the table and the counts around it
follow. The 1.4.7 campaign test now also covers taking the mod back out of a
campaign, which is the behaviour the change was made for.

The readme gains a troubleshooting entry for a campaign that will not load,
which until now was answered only in the saves section. Nothing that ships with
the mod changed.

## [1.6.20] Saves: a campaign saved before 1.6.19 no longer opens

5a40c878e455faf95bc1ff9915d5079e00b97f05, 2026-08-28

A campaign last saved by TradeLord 1.6.18 or earlier no longer opens; campaigns saved by 1.6.19 and later are unaffected.
A price or purchase record for an item whose name carries an unusual character is now left out of the save instead of coming back as a record for an item that does not exist.

## [1.6.19] Saves: removing TradeLord no longer stops a campaign from opening

c32d3b212089c2422efd2dcc0f885882aaf8f588, 2026-08-28

A campaign saved by this version can have TradeLord removed from it later and still open, instead of the save refusing to load without the mod.
The price ledger and your purchase records carry over from a campaign that was saved by an earlier version.

## [1.6.18] Automation: TradeLord now says that it trades for you on entry

b306c378207213d4e89ca7c712b63828ff4c7619, 2026-08-28

TradeLord now says once in each campaign that it buys and sells for you as you enter a market, and names the two settings that turn that off.

## [no release] Repository: the source-check tool now stops on a method it cannot find

424109d342adfd1c23e77368dbc5b31d1e77ac00, 2026-08-28

Looking a method up returned an empty body when the name was not there, so any
rule written as "this text is absent from that method" held whenever the method
had been renamed or removed, which is exactly when it should have spoken up.
Renaming eight methods the tool reads was caught three times out of eight before
and eight times out of eight after. Nothing that ships with the mod changed.

## [1.6.17] Menus: one part of startup failing no longer costs the town and village entries

5ed9ce95b74a20df8f90daf87de8cc230dba9988, 2026-08-28

- The TradeLord entries in the town and village menus no longer go missing when the map marker cannot be restored as your save loads.
- A menu the game does not have no longer costs TradeLord the entries in the menus that it does have.
- Starting another campaign without closing the game no longer leaves livestock buying switched off because of something that happened in the first one.

## [1.6.16] Trading: a refused trade now names the rule that actually stopped it

9afa5fd7913c83d9c80781741123586c799514d6, 2026-08-28

- A market that pays less than the town you are holding your cargo for now says so, instead of blaming your profit margin.
- A herd already as large as your party can drive now says so, instead of reporting that there is no room to carry more.
- The town TradeLord marks as your best place to sell is put back on the map as soon as you load a save, instead of waiting for the next day or the next settlement you enter.

## [1.6.15] Log: every market that trades nothing is now named

1a64ea084ecd39e3b308b15138ff515f4299e4d9, 2026-08-28

- The log now names every market that traded nothing, instead of falling silent after the first market that gave the same reasons.
- Ending a campaign and starting another without closing the game no longer leaves the log silent about problems in the second one.
- A recorded price your save cannot read is now dropped as the ledger loads, instead of stopping the ledger from loading at all.
- With observed prices set never to expire, your save no longer keeps an entry for a good whose recorded prices have all gone.

## [no release] Repository: the compatibility tool no longer stalls on a restore it cannot finish

5dcbc9ba3235630abf45162869a6a3643cb291f6, 2026-08-27

The version fetch redirected the restore's output and then waited for it to exit
without reading either stream, so a restore with more to say than the pipe holds
would leave both processes waiting on each other. It now drains what it asked for,
and reports the restore's own explanation of the failure instead of an empty line.

## [1.6.14] Map: the best-sell marker no longer removes a marker you placed yourself

afae107c96a1c622242f3f755f221df95828bffc, 2026-08-27

- The town TradeLord marks as your best place to sell no longer removes a marker you had placed on that town yourself.
- A town you pinned in the ledger panel is marked on the map again as soon as you load the save.
- Turning live world prices off while you are standing in a market now records that market at once, instead of leaving it blank until you leave and come back.

## [1.6.13] Settings: a newer MCM is named as a mismatch rather than reported missing

c2abd7c3a070a53ff666c9653c65d791156e926d, 2026-08-27

A line of MCM newer than the one this build was made for is now named in the log as exactly that, instead of being reported as MCM missing entirely.

## [1.6.12] Ledger: loot no longer counts its whole sale price as trading profit

9be7713176a86a1a58805980ddb75f2e6194c511, 2026-08-27

Selling goods you never bought, such as loot, now counts profit and Trade XP against what those goods would have cost you at the cheapest market you know of, instead of treating the whole sale price as profit.

## [1.6.11] Ledger: your save stops collecting purchase records for goods you have already sold

74c7429e56a07ecbbb1cee6de52719d883b85388, 2026-08-27

Your save no longer keeps a purchase record for every good you have finished selling, so a long campaign stops collecting a dead entry for each item you have ever traded.

## [no release] Repository: a tool that checks the mod against a game version, and the 1.5.1 beta result

13177cb5ce4586bc95d6c43f99b0f7b01d0546ce, 2026-08-27

Both the 1.4.8 inspection and a new one on the 1.5.1 beta now run from tools/compat, which resolves
every game type, member, patch target, private member and enum value the compiled mod binds to
against any reference-assembly version and exits non-zero if one has moved.

The 1.5.1 beta reaches the same verdict as 1.4.8: nothing found that blocks it. The only differences
are additive - LeaveType gains one unrelated member on the end, and one assembly the mod never
touches is no longer shipped.

## [1.6.10] Panel: the map button no longer reserves map clicks beside it

a4330e3beab38c2a6a3abdf7da914363e1ac8a7b, 2026-08-27

The TradeLord button on the campaign map now reserves only the space the button
itself covers, instead of a fixed strip that reached well past it on ultrawide
and other non-16:9 screens.

## [no release] Repository: a README and an MIT licence

5219d9db13e6fdc9c058dd0ef9a07ebff7161b32, 2026-08-27

The repository had no README and no licence. The README covers what the mod
does, what it needs, how to install it, what the defaults will do the moment
you walk into a town, how the saved data behaves if you remove the mod, and how
to read the log when something goes wrong. The licence is MIT.

Nothing that ships to players changed.

## [no release] Repository: the 1.4.8 roadmap moves to the top level

c1f99e9cba52f2ce445229bb8d59a3c6784a403e, 2026-08-27

The game 1.4.8 document now sits beside the changelog at the root of the repository instead of inside a docs folder, where it did not show on the repository's front page. Nothing that ships with the mod changed.

## [no release] Repository: what the mod needs checked before it moves to game 1.4.8

f1f1760376b8d69b57081b6c47acc3bb03d05ce6, 2026-08-27

The findings from comparing the mod against game 1.4.8, the five questions only the game itself can answer, and the ten-step test pass that answers them, now live in the repository as a document of their own. Nothing that ships with the mod changed.

## [no release] Repository: a document of its own may now live in the repository

a928106d71b3cc267936c2e6e7c821b19c894185, 2026-08-27

The working rules still keep technical detail out of the changelog, the release notes and the commit messages, but no longer bar a document written to be a document from the repository itself. Nothing that ships with the mod changed.

## [no release] Repository: the session hook no longer installs .NET at startup

6a99acb680854e0e453dda80aa04493d3173953d, 2026-08-27

The session hook now only restores the owner's identity and puts .NET on the path, instead of downloading the SDK and restoring packages before every session begins. Nothing that ships with the mod changed.

## [no release] Repository: drop the SDK install rule and the line about documentation being removed

5530b2901cb824d987df0be2cfc53824921e56b9, 2026-08-27

The working rules no longer ask for the .NET SDK to be installed before code work, and the changelog no longer carries the 1.5.6 line saying the project documentation was removed from the repository. Nothing that ships with the mod changed.

## [1.6.9] Settings: the settings screen can now be translated

4678e9a850040966c50dcf4695e6f1fae1edc8dc, 2026-08-27

- The settings screen was the last part of the mod left in English only; every setting name, hint and heading now ships in the language file and can be translated.

## [no release] Repository: the changelog now lists only what changed in the mod

581e8ad799394bd72de0235d1a0151f5f8239f89, 2026-08-27

Every changelog entry that describes something a player can see is unchanged, and the working rules now hold the changelog, the release notes and the commit messages to that. Nothing that ships with the mod changed.

## [no release] Repository: protect the session hook and its settings from deletion

a6be39847ffce61bbf57a2197e8e869c3d181743, 2026-08-27

The working rules now refuse to delete the .claude folder, its settings file and its session hook, the same way they already refuse to delete the changelog and the rules themselves. Nothing that ships with the mod changed.

## [1.6.8] TradeLord: the map button stops swallowing nearby map clicks

12f16fe1254bee4107bf00df655d1745f05da087, 2026-08-27

- The TradeLord button on the campaign map no longer swallows map clicks well above and below the button itself.
- The days of food you asked to keep are no longer partly spent on goods quick-sell was never going to sell.
- Other mods now get first say over their own notifications while TradeLord is trading.
- Added three internal checks. 211 in total.

## [1.6.7] TradeLord: the mod in full, on a fresh history

f46de256172b96d1295cc71314f033267106f356, 2026-08-27

- The Trade XP message now names its number for what it is: the denars of profit credited to your Trade skill.
- Clicking a town in the ledger panel now pins it even when TradeLord is already marking that town as your best place to sell.
- Quick-sell no longer spends part of your food reserve on goods it then passes over because you bought them here on this visit.
- A trade message the game refuses to show no longer leaves the rest of that pass's messages repeating.
- Added four internal checks. 208 in total.
