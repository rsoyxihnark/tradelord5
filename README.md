- It reads what every market in Calradia is paying right now, live, through each market's own price model, so the price it shows is the price it pays.
- It shows you the best ways to buy and sell, right in the item tooltip and in a ledger.
- It can buy & sell for you automatically as you enter a settlement.
- It ranks the trade routes worth travelling on a panel, scored on profit per travel day and on how likely that profit is to survive the trip.
- You can change how it works. The default settings are thoroughly tested in game and work great to get you earning as fast as possible.

## Everything it does

- ✅ Install or remove it before, during or after a campaign, and load a save made with it or without it
- ✅ It declares no save types of its own. All it puts in a save is five strings, seven numbers, a switch and a settlement reference, every one of them something vanilla already reads, so a save written with it still opens without it

**What it tells you**

- ✅ Live prices from every market on the map, read through that market's own price model, so the price it shows is the price it pays
- ✅ The five best places to sell and the five cheapest to buy, in every item tooltip
- ✅ Each of those five priced as that market will be when you arrive, not as it stands while you read, so the tooltip and the ledger never disagree about the same market
- ✅ Which way each market is going, marked beside its price once you switch Mark a market rising or falling on, whenever it has moved 5% or more since the last day you looked there
- ✅ Mark a market rising or falling needs Live world prices off, since that is when TradeLord records prices at all, and it ships off itself
- ✅ Travel time to each of them, from wherever you are
- ✅ That time worked out by the game's own pathfinding, not a straight line drawn across the map, and at your party's real speed with the cargo you are carrying
- ✅ Stock counts, so you know the deal is actually there
- ✅ What you paid for a good, per unit, on its own tooltip once you have bought one, so you can see at a glance whether the market in front of you is beating it
- ✅ Inventory rows tinted good to bad against the best price known
- ✅ One clean set of price hints, bypassing the vanilla rumour system, which is usually out of date by the time you get there
- ✅ A ledger panel on the campaign map, ranking the best route it can find for each good, thirty rows of them
- ✅ Open it on hotkey **T**, on the map button, or from its own entry in the town menu, which falls back to the six best routes written out as text if the panel cannot open
- ✅ Esc closes the ledger or Recent trades and leaves the game's own menu shut
- ✅ Every route priced unit by unit through the game's own model, so the profit is the one you will really get
- ✅ A route whose prices could not be walked that way is marked on its confidence figure
- ✅ A confidence score on each route, which starts at profit per day and is marked down by everything that could eat it
- ✅ What marks it down: how much of the margin survives unit by unit pricing, how much of the good the seller has in stock, how long the trip is, how old the prices are, and how many NPC caravans are sitting at or heading for those two towns
- ✅ The panel keeps its own score, writing down the Sell price it promised you and holding it against what that market really pays when you walk in near the time it said
- ✅ What this means says how much of that promised Sell price has actually been there, over how many prices it has checked, and the tally carries on across your campaign
- ✅ The panel learns from that, and a market that has paid less than it promised is scored lower, so routes selling there fall down the list
- ✅ It keeps a record for each market on its own, written into your save, counts it only once you have walked into that market five times, and can never move a route's Score by more than a quarter
- ✅ Trust a market by what it has paid turns that off
- ✅ How long each route lasts, under Left, which says how long the shelf still holds the amount the route quotes before the caravans heading there buy it out or the town and its workshops use it up, in hours, or in days from two days on
- ✅ Left is blank when the shelf holds that amount through everything TradeLord can see taking from it, up to 30 days after you arrive
- ✅ Prices and stock that count what is still on its way: the cargo the caravans will unload, what that town's workshops will make, and the purses those caravans are bringing to spend
- ✅ That purse is spread over the goods that are cheap there, which is what a trader would really take off the shelf
- ✅ What leaves the shelf is counted too: what the town uses up every day and what its workshops take to make their goods
- ✅ Every workshop is followed day by day, every line of it at the game's own pace, and a run counts only while the town holds its inputs and the run pays
- ✅ Your own workshops draw on their warehouse first, and only the share of what they make that you send to the market is counted as landing there
- ✅ A village is priced through the town it trades with, so what lands in that town moves the village's price as well
- ✅ It keeps score of that forecast as well, against what really moved, and counts what is on its way at the share the forecast has actually been right by, so it leans less and less on one that keeps missing
- ✅ Counted at each end of a route, and only what arrives before you would, so a route is priced on the market you will actually walk into
- ✅ Where part of the amount it quotes is still on the road, the panel marks it Qty!, so a number larger than the shelf is never a surprise
- ✅ Profit quoted with a safety margin, in case prices drift before you arrive
- ✅ How much gold the town you would sell to actually has, counted in, so it never plans a sale nobody can pay for
- ✅ It reads that purse live, so with Live world prices off it plans on the stock alone
- ✅ A route listed whether or not your purse could pay for it today, or your herd is already as large as your men can drive, so the panel always shows you where the profit is
- ✅ What this means says when your gold reserve is what is stopping you buying
- ✅ Click any town to jump the camera there and pin a marker on it, and a pin comes off by itself once TradeLord has traded in that town
- ✅ The best market for the cargo you are carrying, a town or a village, marked on the map for you, and the marker keeps up with you as you ride rather than waiting until you next enter or leave a settlement
- ✅ It counts only the goods it would really sell there, the ones that clear Minimum profit margin, so it never sends you somewhere it will then refuse to sell
- ✅ It works through the richest markets first and stops as soon as none of the rest could outpay the one it has found, so riding across Calradia costs it very little
- ✅ It reads every market's price live each time it weighs one, so the marker is never pointing at a town on a price it read hours ago
- ✅ With Count what is on its way to a market and Bulk price simulation on, it prices each market as it will be when you get there, the same way the ledger does
- ✅ It leaves out the market TradeLord made its last trade at while Auto sell would leave that market alone the first time you come back
- ✅ The market already marked keeps its mark until it is a fifth past your travel ceiling, so a brief slow stretch of road does not flick the marker to another market and back
- ✅ TradeLord.log says every time the marker moves, what the cargo would fetch there, that town's purse, how far away it is and the market it beat
- ✅ The five workshops in Calradia earning the most right now, with their town, what each will make next and their owner, on that same panel
- ✅ With Live world prices turned off it shows yours instead
- ✅ Your gold, your cargo against what your party can carry, your party's speed, the running total of what TradeLord has made you and the Trade XP it has earned you, along the top of that same panel
- ✅ A warning on screen when your purse is under your gold reserve, or your cargo is too full to buy anything, so a market that trades nothing is never a mystery
- ✅ A name on one of your item lists that matches no good in this game said on screen and named in the log, instead of quietly doing nothing

**What it does for you**

- ✅ One trade entry in the town menu, selling then buying in one go, whenever you want it
- ✅ Sells and buys the moment you enter a market by default, once for each arrival
- ✅ Wait here for some time, a walk through the lands, or anything else that drops you back at the town or village menu does not set it trading again, and it leaves that market alone until your party has taken to the road
- ✅ The first time you come back to the market TradeLord made its last trade at, it leaves that market alone as you arrive and as you leave
- ✅ Trade here now (TradeLord) still trades whenever you ask
- ✅ A trade with caravans or villagers on the road frees the market TradeLord made its last trade at, so arriving there trades as usual
- ✅ Leaving a market and walking straight back in counts as the same visit, so it never buys back what it has just sold you there, and your spending cap for the visit lasts the whole of it
- ✅ Tops your food back up to three days of supply, buying the cheapest food the market has and never paying more than the cheapest price it knows of for it
- ✅ The larder is filled after it has traded for profit, so your gold and your cargo room go to the goods you came to trade first
- ✅ Buys and sells livestock as ordinary goods, checking the game's own herding penalty against the men in your party first, so cattle never slow you down
- ✅ A herd is never counted as food: it is not held back towards your days of supply, and it is never bought to restock them
- ✅ Buys any haul animal, a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse or a Pack Camel, paying up to a quarter more than the cheapest price it knows of for one, which you can set anywhere from the cheapest to three times it
- ✅ It buys them only when your cargo is full and leaves behind goods worth buying or the food Restock and keep food (days of supply) asks for, only as many as the gold left after them can fill, and then buys what now fits
- ✅ It buys no haul animal for goods that would fill less than half of one, unless those goods would make more than the animal costs
- ✅ It buys one for food only when the food would fill at least half of it, or when your party is down to its last day of food
- ✅ It buys none while your cargo is already fuller than Share of the hold TradeLord may fill allows, since the first animals would only carry what you already have
- ✅ It never buys one that would leave your purse at 2000 denars or less, which you can set too, so early gold goes on goods instead
- ✅ It buys no riding horses or camels at all, and it stops before your gold reaches your reserve
- ✅ It never buys an animal that would push your party into the herd speed penalty, and a horse one of your unmounted men can ride does not count against that, because he rides it rather than driving it
- ✅ Gets you out of the herd speed penalty by selling animals, and only as many as it takes to get out of it
- ✅ Your herd is looked at three times a visit, as you enter a settlement, again once it has finished trading, and once more as you leave, so a penalty that arrives while you are in town is caught before you ride out
- ✅ Losing men in a battle or to desertion is noticed the same way as picking up an animal
- ✅ Livestock goes first, then a spare riding horse or camel of the plain horse kind that nobody is riding, then your haul animals, and your war horses and noble horses last of all, cheapest first at every step
- ✅ It never sells a horse one of your men on foot is riding, because that horse is not slowing you down, it keeps enough haul animals to carry what you are already carrying, and it keeps back as many animals as a quest of yours is waiting on. One switch turns it off
- ✅ A lame horse, mule or camel, or one of any other quality, is never sold to get you back up to speed or bought to carry more, because the game does not count it gone or come until you load again
- ✅ Sells one unit at a time and stops the moment the price stops clearing your margin, all of it instant, on one click or automatically as you enter a settlement
- ✅ Buys only what it can resell at a profit somewhere within reach
- ✅ Adaptive spend limit lets your spending cap for the visit grow with your purse and only ever adds to it: it holds at Max spend per visit until your purse is five times that, then grows by one more Max spend per visit for every doubling of the purse, so 10000 in the purse allows 2000 and 20000 allows 3000. On out of the box
- ✅ Pick where to sell on the whole load, on while it is being tried out: every market in reach is weighed on what it would pay for the whole load rather than on what it pays for one unit
- ✅ The whole load is only what your caps, your gold, your cargo and your herd let it buy, and a market is weighed only on what its own purse can pay for
- ✅ When the gold left after the goods before it buys fewer units than a good was weighed on, the market is picked again for what it can still buy
- ✅ Holds trade goods and livestock you never paid for, the ones a town event hands you or a quest leaves in your bags, until a market beats what the cheapest market it knows would have charged for them by your margin, so they are never given away at the first stall that will take them
- ✅ Holds cargo back for the market that pays best rather than dumping it one town short, once you switch it on. Off out of the box, so looted gear goes to the first market that can pay for it
- ✅ Clears looted gear too, from tier 1 out of the box and up to any tier you choose
- ✅ Credits the profit to your Trade skill, at a rate you set, and says so on screen when the skill goes up a level
- ✅ Says on screen when the profit could add no Trade XP because your Trade skill is past the game's learning limit, and how many focus points in Trade or points of Social would let it learn again
- ✅ Credits every companion riding with you a share of that same profit, which the game turns into Trade XP the same way it does yours, at a share you set, so a trading clan learns from the run as well as its lord. Off out of the box, so the XP is yours alone until you ask for it
- ✅ Trades in villages as well, under their own stricter travel limit, and leaves a village its last of each good and the last coin in its purse, so its shop stays open
- ✅ Puts 1000 denars back in every village left with an empty purse, once for a campaign, the first time you enter it, so a village an older version spent out can be traded with again, and it says on screen how many it refilled
- ✅ Trade with towns and Trade with villages each switch their own kind of market off, so TradeLord can work villages alone, towns alone, or leave both markets to you
- ✅ Trades with a caravan the moment you meet it on the road, before anyone says a word, selling what clears your margin and buying what it can sell on for more somewhere in reach, out of the caravan's own purse
- ✅ Leaves your own caravans alone, and any caravan on a raft or with no goods to show, since the game trades with none of them
- ✅ Never sells to a party of villagers, and takes their whole offer the moment you meet them when it clears your margin, at the price the game's own "What kind of products do you have?" offer charges
- ✅ The villagers' offer is one deal: Max spend per visit, Never buy grain and the caps on one good never hold it back, so it is taken whole whenever it clears your margin and your purse can pay
- ✅ Once TradeLord has taken the villagers' offer, the game's own offer is gone for the rest of that meeting, so the same goods can never be bought twice
- ✅ An offer TradeLord leaves is still yours to take in the conversation, and what you pay for it is written down as what those goods cost you
- ✅ When TradeLord leaves the villagers' offer to you, a message on screen says why: your margin, your purse, your cargo room, your herd, the good in it that kept it off or nowhere in reach to resell it
- ✅ A caravan is traded with at the prices of the town you are in, or else the nearest town, which is what the game's own trade screen charges you with a caravan
- ✅ A caravan on the road is held to every rule a market visit is: your margins, your caps on how many of a good you hold and what share of the hold it may fill, your looted gear, Hold cargo for the best market, the herd speed penalty and the settling delay
- ✅ Meeting that same party again straight away counts as the same meeting, so it never buys back what it has just sold them and what it spent still counts against its caps, while meeting them again later starts afresh
- ✅ Once the goods have changed hands you can say so, and the trader answers
- ✅ Counts sea legs and appears in port menus if you have the War Sails DLC
- ✅ Buys to fill your ships rather than your carts, if you ask it to, so you can load a War Sails fleet from ashore
- ✅ Tells you on screen exactly what it moved, and names what stopped it when it moves nothing, in one line for a market where nothing changed hands at all
- ✅ Keeps the game's per-item message spam out of a forty-unit sale
- ✅ A coin sound on a trade that lands, silence on one that does not
- ✅ A quiet mode that keeps automated trading to the log and off your screen, apart from three warnings: cargo full, a purse below your Gold reserve, and an item list entry that matches no good
- ✅ A dry run that simulates every trade it would have made and shows you the estimate, marked a best case in the message and in the log, moving nothing
- ✅ The dry run counts the cargo room of the haul animals it would buy, so it also shows the goods and food those animals would carry
- ✅ The dry run also takes away the cargo room of the haul animals and spare mounts it would sell
- ✅ Free passage past bandits, on out of the box and one switch to turn off: run into looters, sea raiders or any other bandits and TradeLord adds a line to what you can say to them, asking to be let past
- ✅ They let you go with no fight and no ransom, then leave you be for the next few hours instead of turning round and hitting you again

**What it doesn't touch**

- ✅ Anything you locked in the inventory screen
- ✅ Anything an active quest of yours is waiting on, whether it is an animal, a trade good, a raw material or the weapons a gang leader asked for, held back from every pass that sells until the quest is done with it
- ✅ Unique and player-crafted gear, and quest items
- ✅ Your haul animals are never sold for profit either, though it will buy them for you, and naming one on your always-sell list is the only way to move one by hand
- ✅ An animal that carries nothing for you and is not livestock is no haul animal, so it is sold like any other cargo rather than sitting in your bags for good
- ✅ Weapons the smithy can break down for parts, so a smithing playthrough keeps its raw material. Three ways to play it: sell them, which is what it does out of the box; keep every one, which holds anything built from smithing parts, forged or looted off a bandit alike; or **keep the ones you have not learned**, which holds a weapon while a part of it is still locked in your smithy and sells it once it can teach you nothing, so your bags stop filling with junk you already know
- ✅ Armour, shields, bows and crossbows carry no smithing design and are sold as usual, and a good on your always-sell list still goes
- ✅ Your food reserve (accounted for the men in your party)
- ✅ A share of every kind of food you carry, if you ask for it, so your party keeps its food variety morale bonus. It follows Restock and keep food (days of supply): set that to 0 and TradeLord keeps no food back at all
- ✅ Buying grain, which is heavy and low margin, so buying it fills the cargo for little return
- ✅ Your gold reserve of 300 denars, the days of your troops' wages you ask it to keep on top of that, and anything past your spending cap for the visit, which ships at 1000 denars
- ✅ Smithing materials such as iron ore, ingots, charcoal and hardwood, once you switch their policy to leave them alone, off by default
- ✅ Any good you put on the never-sell or never-buy list, named by its item id or by the name on screen
- ✅ More livestock than your party can drive, so a purchase never slows you down
- ✅ Markets belonging to a faction you are at war with
- ✅ The game's economy: it trades at the game's own prices, through the game's own buying and selling. On the road, where there is no market to sell to, it moves the goods and the gold itself, at the prices the game's own trade screen or the villagers' own offer would charge you

**Settings, when you want to change anything**

- ✅ A town travel ceiling and a village travel ceiling, the only two things deciding how far it looks, so nothing it suggests, newly marks on your map or holds your cargo for is further than you care to ride
- ✅ Either ceiling set to 0 takes that limit off, and the Town travel ceiling at 0 weighs every town in Calradia, which is the slowest TradeLord runs
- ✅ A minimum stock before it calls something worth buying
- ✅ Count what is on its way to a market, on out of the box, which is what puts the caravans on the road, their purses, the workshops and what the town uses up into a route's price, into where TradeLord means to sell what it buys and into the market marked on your map; it follows Live world prices, so turning those off turns this off too
- ✅ The margin every trade has to clear, on the way in and on the way out
- ✅ Separate rules for food, smithing materials and livestock
- ✅ A share of the whole hold TradeLord may fill, so it stops buying with room left for what a battle or a quest hands you. It ships at the full hold, and selling is unaffected
- ✅ Caps on one good by count, which ships at 32 units a visit, or by denars, on how many of it you will carry, on the share of the hold it may fill, which ships at 45% so one cheap good cannot take your whole cargo, and on the whole visit, which ships at 1000 denars so a full purse is never spent in one town
- ✅ They hold whether TradeLord is buying for profit, restocking your food or buying a haul animal, and only the share of the hold is left off an animal, since the game never counts an animal as cargo
- ✅ Never-sell, always-sell, never-buy and always-buy lists, taking item ids or item names, in any capitalisation
- ✅ What a good counts as having cost you: the average of what you paid, the last price you paid, or the cheapest market you know. It sets the profit reported and the Trade XP earned, and for a good you never bought it is what the price has to beat before it sells
- ✅ Live world prices, on out of the box; turn it off and TradeLord uses only the prices you have seen in person, recorded market by market as you walk them
- ✅ A settling delay, off out of the box, that keeps it out of a brand new campaign until prices calm down
- ✅ Staged Trading, off out of the box: instead of trading, TradeLord opens the game's own trade screen and lays its whole deal on it, all it would sell on one side and all it would buy on the other, one unit to a transfer at the price each unit really fetches
- ✅ Change what you like, then press Done to trade or Cancel to leave it. While it is on, nothing is traded as you arrive or as you leave: Trade here now (TradeLord) in the town menu is what lays the deal out, and a party met on the road still trades as before
- ✅ The trade screen's own running total shows what the deal comes to, and once you close it TradeLord says what your purse did. Press Done and it reports what moved the same way it reports a trade of its own, and credits the profit to your Trade skill
- ✅ Buy Workshops Remotely, on its own button below the TradeLord ledger: every workshop in Calradia a notable would sell you, with its town, what it makes, who owns it, what it has earned lately and what it would cost, most profitable first. Buy any of them from there, wherever you are standing, and it asks you to confirm before it spends a denar
- ✅ The most workshops you may own, raised to 200 out of the box, so your clan tier no longer decides how many you can hold. Set it to whatever number you like, and put it back to the game's own limit with a 0
- ✅ A rebindable panel hotkey, and a map button and two town menu entries you can hide

**And**

- ✅ A settings screen with every switch explained on hover, translatable, through MCM, with a Reset button at the top of it that puts every setting back to the value TradeLord ships with and empties your four item lists
- ✅ English, Turkish, Russian and Simplified Chinese, picked in TradeLord's own Language setting. Its trade messages, ledger panel, price tooltips, town menu entries, the lines it adds when you meet a caravan or a band of bandits, and the settings screen itself all take the new language the moment you pick it, with no restart and no reload
- ✅ Built on Bannerlord 1.4.8.119303. The mod is supported on 1.4.8.119303 and 1.5.3.122374
- ✅ Enable extended debug logging, on out of the box, one switch for everything TradeLord writes about its own working in `TradeLord.log`: what the market you are standing in pays and charges for every good you carry, read four ways before anything is traded, named alongside the price model the game is running and any other mod changing it, then what it quoted for every good it traded next to what the market actually paid, and after every route scan how many prices it opened and how long it took
- ✅ The same switch writes down what it expected a market to hold by the time you got there and what it really held when you walked in, good by good, and everything the map marker weighed, each time what your cargo would fetch there changes: every market it priced with the days, the units, the gold and the profit a day, the marked market broken down good by good, how long each weighing took, and how the mark held up against what that market paid you
- ✅ What this means, behind a ? beside the TradeLord ledger title: the line that used to run under the routes, one clause to a line in a window of its own, so the ledger itself stays clean
- ✅ Recent trades, on its own button on the campaign map under the TradeLord one, so it opens without the ledger: the last twenty buys and sells it made for you, newest first, each with the day, the town, the gold it gained or cost, and what moved. Gold gained reads green and gold spent amber. It is kept in your save, so the list is still there when you load the campaign again
- ✅ One self-check line at the top of every campaign in `TradeLord.log`, saying which of its patches applied and whether it could read the herd penalty, the quest goods, your language file and the game's price model, so a bug report is one line to paste
- ✅ Everything it did goes to `TradeLord.log`, which you can read yourself or send to me if something happens so I can debug it. It is kept from one session to the next, and the one thing that ever empties it is starting the game with it already past 999 KB, which it says in the log itself

**All of it is yours to change.** Every feature above is a switch or a number on the settings screen,
which TradeLord puts there through MCM. Install MCM alongside it and you can turn any one of them off,
or set it to whatever you like. The same switches and numbers are also in `TradeLord.ini`, written beside
`TradeLord.log` the first time TradeLord loads. The file and the settings screen are twins: each records when it
was last saved, whichever was saved last wins, and the other is written to match, so you can install or remove
MCM whenever you like and edit the file by hand either way without losing what you set.

**Answers to what people ask**

- ✅ Can I add it to a campaign already running? Yes, and take it out again. It declares no save types of its own, so a save written with it still opens without it
- ✅ Do I need MCM? No. MCM is what the settings screen is built on; without it every setting is in `TradeLord.ini`, written beside the log the first time TradeLord loads, and you edit it there instead
- ✅ Will it sell something I wanted to keep? Not what you locked in the inventory screen, not what a quest of yours is waiting on, not unique or player-crafted gear, and not what you name on the never-sell list
- ✅ Does it change prices or the economy? No. It buys and sells at the game's own prices, through the game's own buying and selling, so every other party in Calradia sees the Calradia it always saw
- ✅ Do I need the War Sails DLC? No. With it TradeLord counts sea legs and appears in port menus, and without it everything else works the same
- ✅ It traded nothing, why? It says so on screen and names what stopped it, with the longer answer in `TradeLord.log`. The usual reasons are your gold reserve holding your purse back, your cargo being too full to buy, and Max spend per visit or Buy cap per item being spent for that town
- ✅ Can I run another trade mod alongside it? Run only one mod that trades, or the two of them fight over the same cargo and the same purse

## What it needs

- **Harmony** (`Bannerlord.Harmony`), required. TradeLord does not load without it. Put it above TradeLord in the launcher's load order.
- **MCM** (`Bannerlord.MBOptionScreen`), optional, and what the settings screen is built on. TradeLord writes a `TradeLord.ini` beside its log whether MCM is there or not, and every setting above can be changed by editing that file instead.
- The mod is supported on 1.4.8.119303 and 1.5.3.122374. War Sails is optional: with it, TradeLord counts sea legs and appears in port menus.
