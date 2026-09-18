# Changelog

## Unreleased

- The mod description is rewritten in short lines, so it reads at a glance rather than in paragraphs
- Fixed the mod description saying your food is topped up before TradeLord trades for profit, when since 1.83.2 the larder is filled after

## 1.83.3

- Fixed the map marker pointing at the town paying the most in total instead of the one paying the most per day
- TradeLord.log now says what the marked town and the next best one would each earn you per day

## 1.83.2

- Food is now restocked after trading, so your gold and cargo space go to trade goods first

## 1.83.1

- Fixed TradeLord valuing a purchase against a buyer days away instead of a nearby one paying almost the same

## 1.83.0

- Fixed TradeLord buying the good with the fattest margin percentage instead of the one that would actually make you the most gold
- Note: this update resets your settings once, your old ones are listed in TradeLord.log

## 1.82.3

- Where the ledger lists more than one trade out of a market, the one it scores highest is bought first
- Note: this update resets your settings once, your old ones are listed in TradeLord.log

## 1.82.2

- The hint under Minimum stock value for buy suggestions now explains the rule instead of naming a couple of goods as examples

## 1.82.1

- Fixed Minimum stock value for buy suggestions counting a shelf at the shop's asking price instead of what the goods are really worth, which let a shop down to its last unit through

## 1.82.0

- Fixed rare high value goods (e.g. jewelry) not being offered in the ledger, because they are almost never stocked 10 or more in early game
- Added Minimum stock value for buy suggestions, which lets a shelf through on what it is worth in denars rather than on how many units it holds
- TradeLord now buys the good the ledger sent you for first, so your gold and cargo space go to the trade you travelled for
- An item tooltip with no buy prices now says how many units and how many denars' worth no market in reach could manage, and names both settings asking for them

## 1.81.13

- The no buy prices line in an item tooltip is now shorter and set a clause to a line, so tooltips no longer stretch across the screen

## 1.81.12

- Fixed the map marker and the ledger offering towns the game can find no road to
- An item tooltip with no buy prices now tells you no market in reach holds as much as Minimum stock for buy suggestions asks for, instead of just leaving the list out
- The forecast check in TradeLord.log now keeps the units expected and the denars expected apart, so the two no longer read as one number

## 1.81.11

- A caravan heading for a market is now counted as leaving only what that market will really buy off it, since one that is only passing through sells nothing
- Villagers on the road are now counted too, unloading the lot at the town their village trades with

## 1.81.10

- Fixed a town being listed at a forecast price far off what it pays today, which you would never actually get
- A caravan on the road now only drops part of its cargo at its next stop, so the stock forecast is closer to what you find
- A market with no road to it is no longer offered as a place to sell, and shows no travel time

## 1.81.9

- Fixed item tooltips picking their five best markets before pricing them for the day you arrive, which left towns in the list at prices you would never get

## 1.81.8

- Fixed a TradeLord.ini locked by something else being written over with the starting settings, losing everything you had set

## 1.81.7

- TradeLord now puts 1000 denars back into every village left with an empty purse, once, so villages an older version spent out will trade again
- It says on screen how many village purses it refilled, and TradeLord.log names each one
- The mod description now mentions the one time village purse refill
- The mod description now counts the switch TradeLord writes into your save as well as the strings and numbers

## 1.81.6

- Villages now keep the last coin in their purse, so Buy products no longer greys out with Village shop is not available right now
- The map marker and the ledger now skip a village left with nothing but that coin
- The mod description now says a village keeps the last coin in its purse as well as its last of each good

## 1.81.5

- Campaigns left running at speed are smoother, because reading what the workshops will make next costs less each hour
- The mod description now says Write a price trace to the log ships on, not off

## 1.81.4

- The TradeLord ledger opens faster, because a town's shelves are now read once instead of once for every good

## 1.81.3

- The TradeLord ledger opens faster again, because a town's shelves are read once rather than once for every workshop in it

## 1.81.2

- Fixed a deal taken on the trade screen earning your Trade skill twice
- The ledger's Trade XP still counts that deal, so the total covers what TradeLord trades and what you take on the trade screen

## 1.81.1

- Fixed a deal taken on the trade screen being reported twice to the rest of your game

## 1.81.0

- The ledger now shows the Trade XP TradeLord has earned you next to the profit, and keeps it in your save
- Selling something with a quality, like a Fine sword, no longer earns Trade XP, because selling it by hand never did either
- Anything else in your game that watches your trading profit now sees what TradeLord trades, the same as a trade you make by hand

## 1.80.14

- Fixed a failed trade leaving TradeLord's hold on prices in place, which could change what caravans and villagers paid for their own goods
- A good whose trade screen price cannot be read is now left alone, instead of traded at a price you would never be charged

## 1.80.13

- Write a price trace to the log now ships on, so a log you send in carries the price readings for everything you were carrying
- Note: this update resets your settings once, your old ones are listed in TradeLord.log
- The reset message no longer promises it will not happen again, because it might while the trading is still settling

## 1.80.12

- TradeLord now buys and sells at the price the trade screen would charge you, so what it earns is what you would earn by hand
- Villages pay about half for what you sell, the way they always have at the trade screen, so TradeLord passes over most village sales again
- Trading with a faction you are at war with now costs what it costs you by hand, not the peacetime price
- TradeLord now trades nothing in a market where it cannot read the price you would be charged

## 1.80.11

- Recent trades is now kept in your save, so the last twenty buys and sells are still there when you load the campaign

## 1.80.10

- Fixed Staged Trading laying out a deal at prices the trade screen would not charge

## 1.80.9

- Fixed villages looking like they pay half what they really do, which made TradeLord skip sales worth making
- Fixed markets of a faction you are at war with carrying a price penalty TradeLord is never actually charged
- The mod description now says the price TradeLord shows is the price it pays

## 1.80.8

- The forecast score in TradeLord.log now says how many units left a market, instead of writing it as a number below zero

## 1.80.7

- The mod is supported on Bannerlord 1.4.8.119303 and 1.5.3.122374

## 1.80.6

- What this means now explains that with Bulk price simulation off, only stock counts what is on its way to a market
- Trust a market by what it has paid now says it needs Rank routes by confidence, the way the other settings do

## 1.80.5

- What this means no longer claims a market's Score is lowered for underpaying while Rank routes by confidence is off, because nothing lowers it then

## 1.80.4

- Fixed Recent trades saying nothing has been traded this campaign when it meant nothing since you loaded the game

## 1.80.3

- The TradeLord ledger opens faster, because a market is no longer searched top to bottom for goods it never stocked
- Item tooltips are lighter under the mouse, because the best market lists are no longer built twice

## 1.80.2

- Fixed Recent trades opened from the campaign map not scrolling or closing
- Recent trades now closes when you open the game menu over it

## 1.80.1

- The best markets for a good are picked by what each market charges today again, the way they were before 1.80.0
- Fixed food restocking, haul animal buying and Minimum profit margin moving with caravans heading somewhere you are not going

## 1.80.0

- The best markets for a good are now picked by what they will charge by the time you get there, so a market about to be cleared out can beat one that looks dearer today
- None of that changes unless Count what is on its way to a market is on

## 1.79.3

- The two Price columns in the ledger now show what you would really pay and be paid on arrival, so they agree with the Profit and Qty beside them
- Fixed routes being dropped from the ledger, or their Qty cut short, over prices at markets you are not standing in yet

## 1.79.2

- Fixed the ledger dropping a route that should have topped the list when Count what is on its way to a market raised what the buyer would pay

## 1.79.1

- Fixed the ledger and Buy Workshops Remotely breaking when a workshop, its town or its owner has no name, which now shows as an id
- Fixed TradeLord.log repeating the map marker line when the marked town was one you had already pinned from the ledger

## 1.79.0

- Recent trades now opens from its own button on the campaign map, under the TradeLord one, and has left the row along the bottom of the ledger
- Workshops for sale is now called Buy Workshops Remotely, and so is the window that confirms a purchase
- TradeLord button on the map screen now says it puts two buttons on the map, not one

## 1.78.5

- What this means has left the row along the bottom of the ledger, and a ? beside the title opens it instead

## 1.78.4

- Fixed the ledger breaking when a good or a market on a route has no name, which now shows as an id
- Recent trades, and the log line for a trade screen that will not open, now name a market or party by its id where it has no name

## 1.78.3

- Fixed a Never sell, Always sell, Never buy or Always buy list with a line break in it losing everything after the break, or landing on top of another setting

## 1.78.2

- Silence trade messages now names the three warnings it still shows: a full cargo, a purse below your Gold reserve, and an item list entry matching no good
- Mark a market rising or falling now says it needs Live world prices off, since with them on it marks nothing
- The mod description now says a town's purse is read live, so with Live world prices off a route is planned on stock alone

## 1.78.1

- Fixed Trust a market by what it has paid counting every price it checked as a visit, so one arrival could already start lowering a market's score
- The line under the routes and TradeLord.log now say how many prices were checked, rather than calling them arrivals
- TradeLord.log now says how a market's own record stands as you walk in, and that it is what lowers routes selling there
- Fixed an empty message raised mid trade stopping the game showing messages at all

## 1.78.0

- The ledger now learns from what a market has really paid you, and scores a market that pays less than it promised lower
- It keeps a record for each market in your save, and only counts it after five visits, so one bad arrival never moves the list
- It can never move a route's Score by more than a quarter, so a disappointing market is pushed down rather than buried
- Trust a market by what it has paid, in the Knowledge settings, turns it off, and it ships on
- What this means now says when a market's own record is lowering a route's Score

## 1.77.2

- Auto-mark best sell market on map now checks the richest markets first and stops early, so riding across Calradia costs far less
- The map marker now keeps a price it read for a few hours instead of one, so the town it points at can be picked on slightly older prices
- TradeLord.log now writes the map marker line only when the marker moves, and says when a town's purse is all your cargo could fetch there
- Buy to fill the ships now asks what your fleet is carrying once per trade instead of once per unit
- TradeLord.log now names the messages it held back during a trade instead of just counting them, so another mod's missing message can be found

## 1.77.1

- Fixed Auto-mark best sell market on map repricing every market on the map each time you walk in or out of a settlement
- The mod description now names two limits it had left unsaid, Buy cap per item at 32 units a visit and Max spend per visit at 1000 denars

## 1.77.0

- Note: this update resets your settings once, your old ones are listed in TradeLord.log
- Your never sell, always sell, never buy and always buy lists are emptied by that reset, so write them in again

## 1.76.9

- Fixed a save written by a newer TradeLord losing what you paid for your goods when you go back to an older build

## 1.76.8

- A pass that trades a lot of goods no longer stutters as it finishes, because the list reaches TradeLord.log in one go
- The panel's promise score and Score the forecast in the log now reach the log the same way as you walk into a market

## 1.76.7

- Write a price trace to the log now ships off, so walking into a market costs nothing until you turn it on
- With it on, the price trace no longer stutters as you walk in, because it reaches TradeLord.log in one go
- The trade screen opens faster, because Show best buy/sell in tooltips and Color prices by world market work their markets out once instead of once for every good

## 1.76.6

- Fixed Most workshops you may own lifting the limit for every other clan at your clan tier as well as your own
- Auto-mark best sell market on map costs far less while you ride, because your cargo and each market's price are read once an hour rather than every time you move
- The TradeLord ledger opens faster again, because a shelf's life is worked out once for each market rather than once for every size of deal
- Fixed Never sell, Always sell, Never buy and Always buy slowing down every trading pass

## 1.76.5

- Fixed hovering a good drifting the prices TradeLord had recorded for it, further away with every hover
- Those same prices colour your inventory, decide Hold cargo for the best market and set what TradeLord will pay, so a hover could quietly change all three
- The TradeLord ledger opens faster, and faster again with Live world prices or Bulk price simulation off

## 1.76.4

- Fixed TradeLord being able to claim every later trade you made by hand as its own, if the game never reported the staged trade screen closing
- Buying a workshop now checks the gold really left your purse, so one is never free and never paid for twice
- TradeLord.log now says what a workshop cost you and what the game actually took

## 1.76.3

- Fixed the 1.76.2 fault in a second, older place, where selling 29 iron by hand for 1742 denars was written down as 1742 iron and wiped what your iron had cost
- With that record gone Minimum profit margin had nothing to measure against, so TradeLord could sell a good for less than you paid
- Fixed buying by hand being written down as far more of a good than you bought

## 1.76.2

- Fixed Staged Trading reading a deal back with each good's price in place of its count, so 29 iron sold read as 1742 iron and the gold ran to hundreds of thousands of denars
- Your Trade skill and the TradeLord profit on the ledger were both credited with that gold you never made, and now take the real figure
- TradeLord now checks what it read off the trade screen against what your purse really did, and counts nothing when the two disagree
- Profit from a sale can no longer be reported as more than the sale fetched
- Fixed Recent trades dating a trade made today as Day 91,082, which now reads Today or how many days ago it was

## 1.76.1

- Most workshops you may own now lifts the limit for your own clan only
- The confirm box now warns you when a workshop takes you below your Gold reserve and wage cover, and still lets you buy it
- The comparison with the other trade mods now names what TradeLord has gained since it was written
- The comparison called Staged Trading by its old name, and now uses the one on the settings screen

## 1.76.0

- Added Workshops for sale, a button below the TradeLord ledger listing every workshop in Calradia a notable would sell you, most profitable first
- You can buy any of them from that window wherever you are standing, and it asks you to confirm before it spends a denar
- Added Most workshops you may own under General, at 200, which lifts the limit the game puts on how many you may own, and a 0 hands it back to the game
- The deal Staged Trading lays out now reports what moved when you press Done, and credits the profit to your Trade skill
- The line under the routes has moved into a window of its own on a new What this means button, one clause to a line
- Write a price trace to the log and Score the forecast in the log both ship on now, so anything that looks wrong is already written down when you come to ask

## 1.75.1

- The map marker line in TradeLord.log now calls the market it beat the next best it priced, rather than the second best on the map
- Fixed a market whose prices could not be read saying so only once a session, and never again in your next campaign
- The mod description now says meeting the same caravan or villagers again keeps the books of what was already traded

## 1.75.0

- Fixed the map marker sending you to a town that then refuses to sell anything and tells you your cargo is full
- TradeLord.log now writes a line every time the marker moves, with the town, what your cargo would fetch there, its purse, how far away it is and the market it beat
- Added Gold before it buys a haul animal under Buying, at 2000 denars, so your early gold goes on goods instead
- Added Most it will pay for a haul animal under Buying, at 125%, so it no longer holds out for the cheapest price it has ever seen

## 1.74.0

- TradeLord now trades at the first market of a campaign like any other, instead of leaving it untraded behind a warning
- Fixed meeting the same caravan or villagers again buying back what TradeLord had just sold them
- Lay the trade out for you first is now called Staged Trading
- The trade screen Staged Trading opens now shows what the laid out deal comes to in its own running total
- Once you close that screen, TradeLord says what your purse did on it
- Recent trades now opens in a window of its own, from its own button below the TradeLord ledger
- The line under the routes is now large enough to read and no longer runs off the bottom of the panel

## 1.73.0

- Left now counts towards a route's Conf, so a route whose shelf empties before you get there ranks below one that still has the goods waiting
- A route whose shelf the forecast says will hold is no longer marked down twice for the same caravans
- The line under the routes now says what Left does, not just what it means
- Routes are ranked by Left only with Live world prices on, and with it off the caravan count decides as before

## 1.72.1

- The mod description now names Recent trades

## 1.72.0

- Added Recent trades at the end of the ledger, the last twenty buys and sells with the day, the town, what moved and the gold it gained or cost
- Gold gained reads green and gold spent amber, so a buying visit and a selling visit tell apart at a glance
- Recent trades is kept for the session only and never written into your save

## 1.71.2

- Fixed Economy settling delay shutting every market on a campaign whose age the game cannot work out
- The days left of that delay are never shown as none, or as more days than the delay you set
- The mod description now ends with short answers to the questions people ask before installing

## 1.71.1

- TradeLord now keeps at most 2500 recorded prices, oldest forgotten first, so a long campaign cannot grow your save without end
- TradeLord.log now says at every save how many recorded prices and purchase records went into it, and how large they are
- The comparison with the other trade mods is now one short entry a mod, and no longer says another mod alone tells you how long a route lasts

## 1.71.0

- Added a Left column to the ledger, saying how long the market you would buy from still holds what a route quotes before the caravans heading there buy it out, and blank when nobody is coming for it
- The panel is a little wider to make room for it
- The mod description says so too

## 1.70.0

- Added Share of the hold TradeLord may fill under Buying, which leaves room for what a battle or a quest hands you, and ships at the full hold so nothing changes until you move it
- Added Share of the profit your companions learn from under General, giving every companion riding with you Trade XP, and it ships at 0 so the XP is yours alone until you ask
- An item tooltip now says what you paid per unit for a good you own, so you can see at a glance whether the market in front of you beats it
- The panel no longer quotes more of a good than a market will have when you arrive
- The mod description says so too

## 1.69.3

- TradeLord.log now says each day how many recorded prices it forgot and how many it is still keeping, so you can see what Days to keep a price you recorded is doing

## 1.69.2

- Fixed a market looking as though it is next door when the game could not work out how far away it was
- Fixed the panel dividing by a price of nothing when working out how many of a good are worth showing
- The mod description now opens with five short lines instead of four and a long paragraph
- The comparison with the other trade mods now says when they were read, and that anything they have changed since is not in it

## 1.69.1

- Fixed a save written by a newer TradeLord losing every recorded price the moment you go back to an older build
- The log now says how many prices it could not read out of a save and why, instead of dropping them without a word

## 1.69.0

- Added Days to keep a price you recorded under Knowledge, so a market you have not looked at in a fortnight stops being suggested and leaves your save
- It starts at 15 days, and a 0 keeps every price for as long as your campaign lasts, the way it worked before
- Fixed a shelf life you had set under an older TradeLord being thrown away

## 1.68.0

- Added Mark a market rising or falling under Insight, which ships off, so tooltips read as they did before until you turn it on
- The mod description says so too

## 1.67.0

- Item tooltips now mark a market rising or falling when its price has moved 5% or more since the last day you looked there, in the sell list and the buy list alike
- TradeLord now remembers the price from your previous visit to a market as well as the latest one, and a second look on the same day does not count as a new visit
- A campaign saved before this version keeps every price it had, and starts marking a direction once you have looked at a market on two different days
- The mod description now says prices are read through each market's own price model, the way the trade screen asks
- The opening line now says the default settings are there to get you earning as fast as possible
- The mod description says so too

## 1.66.0

- TradeLord.log now opens every campaign with one self check line, so a bug report is one line to paste rather than a hunt through the log
- Anything TradeLord cannot read now says so as your campaign opens rather than the first time it matters
- The mod description says so too

## 1.65.0

- Added Lay the trade out for you first under Automation, which opens the trade screen with the whole deal on it so you can change what you like, and it ships off
- Each unit is laid out at the price that unit really fetches, so the total on the screen is the total TradeLord worked out
- With it on nothing is traded as you arrive, and Trade here now (TradeLord) is what lays the deal out, while a party met on the road trades as before
- A deal with nothing in it says so rather than leaving you looking at an empty screen
- Simulation mode (dry run) still wins where both are on, so a dry run never puts anything on the screen
- The mod description says so too

## 1.64.1

- Fixed what a workshop will make next always landing a day away instead of when the game says the run is due
- What a workshop will make next is now valued at the good that town actually stocks, not the cheapest of its kind in Calradia
- Two routes reaching the same market within the same quarter day can no longer quote it differently
- Pricing a route does less work for the same answer, adding a market's takings up once an hour rather than once for every good

## 1.64.0

- The panel now scores its own promise, holding the Sell price it quoted against what the market really pays when you walk in
- The line under the routes says how much of that promised price has really been there and over how many arrivals, and the tally carries on across your campaign
- With Score the forecast in the log on, the log breaks that down by Conf, so you can see whether a higher Conf really means a promise that holds
- A promise you arrive far too late for is dropped rather than scored
- The mod description says so too

## 1.63.0

- Added Score the forecast in the log under Debug, which writes down what TradeLord expected a market to hold and what it really held, good by good
- It needs Count what is on its way to a market, and stays off until you turn it on, since it makes the log longer
- The mod description says so too

## 1.62.2

- In Russian and Simplified Chinese, the note under Keep gold for days of wages now calls Gold reserve by its name on the settings screen
- In Turkish, Simplified Chinese and Russian, the note under Auto-mark best sell market on map now names the travel ceilings and the ledger the way the rest of the mod does
- In Russian, the note under Count what is on its way to a market now calls Live world prices by its name on the settings screen

## 1.62.1

- The ledger now marks a route Qty! where its amount counts goods still on the road, so a number larger than the shelf is never a surprise
- The line under the routes says what that mark means
- The mod description says so too

## 1.62.0

- The five markets in an item tooltip are now priced as each will be when you get there, so a tooltip and the ledger never quote the same market differently
- Those five are ordered on the prices you will actually be offered, so a market about to be picked over no longer sits at the top
- The note under Count what is on its way to a market now says it reaches a tooltip price as well as a route
- The mod description now says the tooltip prices a market as it will be when you arrive
- The changelog was missing 1.49.0 and 1.49.1 entirely, and both are back in it word for word
- The mod description now says the ledger ranks the best route it can find for each good, thirty rows of them, rather than every profitable route
- The mod description now calls the smithing choice Keep the ones you have not learned, its name on the settings screen
- The mod description now says quiet mode still speaks the notice at your first market and the warning that your cargo is full
- The mod description now says a route is listed even with a full herd, that a route priced without walking every unit is marked on its Conf, and that a dry run is marked a best case
- The opening summary now names what is actually weighed in a route rather than counting the factors behind it
- The mod description now says TradeLord tells you once at the first market of a campaign that it trades for you, and leaves that market untraded so you can switch it off first
- The mod description now says you can tell a caravan it was a good trade once the goods have changed hands, and that the trader answers
- The mod description now says the town menu entry falls back to the six best routes written out as text when the ledger cannot open
- The mod description now counts how much of the margin survives unit by unit pricing among the things a route's confidence is discounted by
- The mod description now says the settling delay ships switched off, and that a trade on the road moves the goods and the gold itself at the price the game quotes off market

## 1.61.0

- A route's price now counts the gold the caravans on the road are bringing to spend at its two markets, so a market about to be picked over no longer looks cheap
- That purse is spread over the goods that are cheap at the market it is heading for, which is what a trader would actually buy
- A purse counts only at the market the caravan is going to, only when it arrives before you would, and never for more gold than the caravan carries
- Count goods on their way to a market is now called Count what is on its way to a market, since it counts the gold coming to spend as well as the goods
- The line under the routes now says prices count what will be bought off a market as well as what will be added to it
- The mod description now says a route's price counts the purses the caravans are bringing

## 1.60.0

- A route's price now counts what is still on its way to its two markets, the cargo the caravans will unload and what the workshops will make next, so the panel prices the market you will walk into
- Goods that would land after you arrive are left out, each end of a route counted against its own travel time
- The stock behind a buy town's confidence now counts the cargo heading there, so a deal that only appears once a caravan unloads is no longer hidden
- The workshop list on the ledger now says what each workshop will make next, beside what it has earned
- Added Count goods on their way to a market under Knowledge, on out of the box, and it follows Live world prices so turning those off turns it off too
- The line under the routes now says when prices and stock are counting what is on the way
- The mod description now says prices and stock count the caravans on the road and the workshops, and that the workshop list says what each will make next

## 1.59.0

- The map marker can now land on a village as well as a town, as long as Trade with villages is on
- A village being raided or rebuilding is never marked, since you could not trade there anyway
- Auto-mark best sell town on map is now called Auto-mark best sell market on map, and keeps to the Village travel ceiling when it is weighing a village
- The mod description now says the map marker can land on a village as well as a town

## 1.58.2

- The message and the ledger now split the gold held back between Gold reserve and Keep gold for days of wages, and name both
- Where Gold reserve alone is holding your money back, that message names it, so you know which setting to lower
- Fixed that message appearing when it was Max spend per visit that stopped the buying

## 1.58.1

- TradeLord now tells you on screen that the market is still settling as you walk in, unless Silence trade messages is on

## 1.58.0

- Livestock is no longer counted as food, so a herd is bought and sold as ordinary goods with only the herd speed penalty holding it back
- Keep some of every kind of food now says livestock is left out, because a herd is traded as goods and never as food
- The mod description now says a herd is never counted as food

## 1.57.0

- An army waiting on livestock now holds back that many head of your herd, so the last quest that could lose its supplies to a shopping trip is covered

## 1.56.0

- Two more quests are read for what they are waiting on, the army that needs supplies and the headman who needs grain, so what they are owed is held back from every sale

## 1.55.0

- TradeLord now holds back any good an active quest of yours is waiting on, a trade good or a raw material as well as an animal
- Four more quests are read for what they are waiting on, the two artisan deliveries, the gang leader's stolen goods and the landlord's produce
- Nothing bought here now says when it was your Buy cap per item that stopped a good, rather than blaming your purse
- Fixed the warning that your purse is under your gold reserve being swallowed by a visit that sold something
- The town marked on your map is never one TradeLord would walk into and then leave alone
- When TradeLord cannot thin your herd, TradeLord.log now names the animals it is holding back rather than going quiet
- Always sell now says a good a quest is waiting on still holds, not only an animal
- The mod description now says anything a quest is waiting on is held back from every sale

## 1.54.0

- Keep gold for days of wages now ships at 0, so a large army no longer stops TradeLord buying out of the box
- Nothing bought here now blames your purse only when nothing else held a good back, so the reason you see is the one you can act on
- With Simulation mode (dry run) on, what the dry run spends goes back into the market's gold, so a later sale is measured against the till it would really have
- The mod description now says TradeLord holds back only the days of your troops' wages you ask it to keep

## 1.53.0

- TradeLord.log now says what your purse holds, how much is held back and how that splits between your gold reserve and your wage bill, when it buys nothing
- Fixed Silence trade messages still putting a line on screen when you met a caravan during the Economy settling delay

## 1.52.4

- Fixed a language file that points somewhere else leaving the game waiting while TradeLord fetched it

## 1.52.3

- Fixed a dry run counting food it had already sold towards your food reserve, so it now sells the same animals a real pass would

## 1.52.2

- Fixed a dry run overstating the profit on a good that sits in more than one stack in your bags

## 1.52.1

- Restock and keep food (days of supply) set to 0 now keeps no food back at all, not even the few of each kind
- The hint under Keep some of every kind of food now says it needs Restock and keep food (days of supply) turned on
- The mod description now says keeping some of every kind of food follows the days of supply you set

## 1.52.0

- Buy cap per item, in count and in denars, and Stop buying at this many held now hold while TradeLord restocks your food and buys haul animals too
- Share of the hold one good may fill now holds while TradeLord restocks your food, and is left off haul animals, which add to the hold rather than filling it
- The lines TradeLord adds when you meet a caravan or a band of bandits now come out in the language you picked, even when you change it mid game
- The mod description now says which passes the per item caps hold in, and that a conversation line takes the new language too

## 1.51.1

- Fixed turning Trade with towns off leaving the map marker on a town TradeLord will not trade in

## 1.51.0

- Added Trade with towns, which turns trading in town menus off the same way Trade with villages does, and it ships on
- The settings screen has a new Trade Pool group, holding who TradeLord trades with, the two travel ceilings and Exclude hostile markets
- The longest hints on the settings screen are shorter, so a hint no longer spills over the settings beneath it
- The mod description now says trading in towns and trading in villages can be switched off one at a time

## 1.50.2

- Trade with caravans and villagers you meet is now called Trade with caravans and villagers, so it is clear this is the setting covering a party you meet on the road

## 1.50.1

- Fixed the 1.50.0 reset not sticking with MCM installed, because the settings screen kept a copy of its own and handed your old settings straight back
- TradeLord now puts the settings screen's own copy back to what it ships with too, so the reset holds
- Note: because the 1.50.0 reset never took, this update resets your settings once more
- Your old ones are listed in TradeLord.log, setting by setting, so you can put back the ones you want
- Your never sell, always sell, never buy and always buy lists are emptied by that reset too
- It happens on this version only and never again on a later one

## 1.50.0

- Note: this update resets your settings once, because the ones TradeLord ships with now trade better out of the box
- Your old ones are listed in TradeLord.log, setting by setting, so you can put back the ones you want
- Your never sell, always sell, never buy and always buy lists are emptied by that reset too
- It happens on this version only and never again on a later one
- Fixed getting your party back up to speed selling the animals Restock and keep food (days of supply) was holding back
- Where your herd is also your food, the herd speed penalty can now stay rather than eat into the reserve, and TradeLord.log says so
- An animal on your always sell list is still sold to get you back up to speed
- Fixed what a good cost you drifting by a denar as a stack sold down, which stopped a sale early with goods left worth selling
- When a quest is waiting on more animals than your food reserve holds back, TradeLord now keeps the larger of the two

## 1.49.1

- The one time settings reset is switched off from this version on, so nothing of yours is put back to what TradeLord ships with again
- It ran on 1.49.0 alone, so coming here straight from an older version leaves your settings exactly as you left them

## 1.49.0

- Note: this update resets your settings once, because the ones TradeLord ships with now trade better out of the box
- Your old ones are listed in TradeLord.log, setting by setting, so you can put back the ones you want
- Your four item lists are emptied by that reset as well, and they are in the log too
- It happens on this version only and never again on a later one

## 1.48.0

- Keeping food back and restocking it are one setting now, Restock and keep food (days of supply), which ships at 3 days
- If you had set Restock food (days of supply) yourself, TradeLord now goes by your Keep food number for both and says so in TradeLord.log
- Town travel ceiling now ships at 2.4 days instead of 3
- Write a price trace to the log now sits in a Debug group of its own at the foot of the settings screen
- The price trace is now written as you walk into a market, before anything is traded, so it holds the prices TradeLord went on
- With the price trace on, every good a pass moves is written down with the price TradeLord quoted next to what the market really paid
- When TradeLord cannot read the game's herd penalty, TradeLord.log now names everything that stops, not just livestock buying
- Fixed the herd check in TradeLord.log reporting no herd penalty when it could not read one at all
- Fixed a panel hotkey written with a leading plus, such as +T, reporting an empty modifier in TradeLord.log
- Fixed buying none of a good leaving TradeLord holding a cost against goods you have none of
- The mod description now says the price trace is written before anything is traded, and records what was quoted against what was paid

## 1.47.5

- The hint under How many of each kind of food to keep now points at Keep food (days of supply) by the name on the settings screen, in every language

## 1.47.4

- Fixed TradeLord selling off part of your food reserve when the same food sat in two stacks in your bags

## 1.47.3

- Fixed the running total of what TradeLord has made you turning negative once it passed about 2.1 billion denars
- The mod description now says a save carries two of TradeLord's numbers rather than one

## 1.47.2

- Fixed a dry run stopping short over looted gear and horses, which your inventory keeps in a separate stack for each quality

## 1.47.1

- Every price TradeLord quotes is now read from a market the way the trade screen reads it, so the tooltip, the ledger and the routes all match what you are offered
- Village prices moved the most, because a village's own shelves were being left out of what TradeLord read there

## 1.47.0

- Added Write a price trace to the log, off out of the box, which writes down what the market you are standing in pays and charges for everything you carry
- It names the market, the price model the game is running and any other mod changing either of them, so a mod moving prices behind TradeLord's back shows up by name
- The mod description now says TradeLord can write a price trace to its log
- The mod description now says trading on arrival runs once for each arrival, that waiting in a town or village does not set it trading again, and that walking straight back in counts as the same visit
- The mod description now says goods you never paid for wait for a price that clears your margin, and it names What a good counts as having cost you the way the settings screen does
- The mod description now says the map marker keeps up with you as you ride, and that TradeLord.log is kept between sessions and only emptied at startup once past 999 KB

## 1.46.2

- Trade goods and livestock you never paid for, like the ones a town event hands you, are no longer given away at the first market that will take them and now wait for a price that clears your Minimum profit margin
- Nothing sold here and Nothing bought here no longer answer with haul animals and mounts are not traded as livestock, and name a reason about your own cargo instead
- Fixed Wait here for some time setting TradeLord trading all over again every time the menu came back

## 1.46.1

- TradeLord now reads the SettingsVersion line in your TradeLord.ini to decide which older settings to carry forward, so a file already up to date is left exactly as you wrote it
- A value you type into an up to date TradeLord.ini by hand is now checked, so a wrong one is named in TradeLord.log instead of being quietly turned into something else

## 1.46.0

- TradeLord.log is now emptied as the game starts once it has grown past 999 KB, so a long campaign cannot leave a file growing without end in your Bannerlord folder
- It is only ever emptied as the game starts and never as it closes, so the log you send on after a session still holds everything that session wrote
- TradeLord.log says when it was emptied and how large it had grown, so a short log is never a mystery
- Fixed one line TradeLord could not write to its log slowing down every line after it for the rest of the session

## 1.45.0

- Town travel ceiling and Village travel ceiling are now the only two things deciding how far TradeLord looks, and the tooltips, the routes, the map marker and Hold cargo for the best market all obey them
- Hold cargo for the best market now waits only for a market inside those two ceilings, three days and one day out of the box, instead of for the best price anywhere in Calradia
- The scan radius setting is gone, since those two ceilings now cover what it did, and TradeLord.log names the value it dropped from your TradeLord.ini
- Your travel ceilings keep everything you had set, carried forward under their new names by TradeLord.ini itself
- The mod description drops the scan radius and names the two travel ceilings instead

## 1.44.0

- Fixed the map marker sending you to a town outside your Town travel ceiling and Scan radius, where TradeLord will not sell
- The town marked on your map now follows you as you ride, instead of waiting until you enter or leave a settlement
- Travel ceiling is now called Town travel ceiling, since that is what it governs
- The marker's own travel ceiling setting is gone, and TradeLord.log names the value it dropped from your TradeLord.ini
- A market that sold nothing now says why even when TradeLord bought something there, so Hold cargo for the best market no longer holds your cargo in silence
- Fixed TradeLord saying a market had nothing worth trading when there was nothing to weigh up in the first place

## 1.43.0

- Leaving a market and walking straight back in counts as the same visit, so TradeLord no longer buys back the goods it has just sold you there
- Your Max spend per visit now lasts the whole of that visit, instead of starting again each time you step back inside
- TradeLord.log is kept from one session to the next, instead of being emptied every time the game starts
- TradeLord.ini is no longer rewritten when you start the game and none of your settings have changed
- TradeLord.log now names the goods it stops holding against a resale when they leave your party unsold, rather than only counting them

## 1.42.4

- Fixed buying at a market yourself leaving TradeLord thinking you took thousands more of a good than you did, which then sold that cargo under your Minimum profit margin
- That miscount also had the game working out the price of thousands of units you never bought, which is gone

## 1.42.3

- Meeting a caravan or villagers on the road and trading nothing with them now really does tell you why, which 1.42.0 meant to do and did not
- The mod description no longer says a route is quoted against what your purse holds, since the ledger now lists a route whether or not you could pay for it today

## 1.42.2

- Selling at a market now works the best markets out for everything in your bags in one go, the way buying already did

## 1.42.1

- Walking into a market now works the best markets out for the whole shelf in one go, so a busy market costs your game less
- Restocking your food and buying a haul animal do the same
- The ledger now lets go of what it worked the routes out with as soon as it has finished, instead of holding on to it until you open the panel again

## 1.42.0

- Meeting a caravan or villagers on the road and trading nothing with them now tells you why, the same as walking into a market already did
- The TradeLord ledger now lists a route even when your purse is empty or your herd is full, so it always tells you where the profit is
- When your purse is empty the ledger says so under the routes, instead of showing that message in place of them

## 1.41.9

- Working out which markets pay best for a good is quicker, because TradeLord keeps the best few as it goes instead of putting every town in order first
- TradeLord no longer asks a town what it pays for a good when that town is already beyond your Travel ceiling

## 1.41.8

- Fixed TradeLord falling back to English until you restart the game, when it could not read its language file for a moment

## 1.41.7

- The language you pick, and every other setting you choose from a list, now take hold as you pick them instead of waiting for a restart
- TradeLord.log now says when the settings screen has taken charge, not only that it is still waiting for it
- With Live world prices off, writing down what a market charges no longer gets slower the more towns you have visited

## 1.41.6

- Walking into a market no longer has TradeLord work the best markets out all over again, since walking in moves no prices
- After it trades, it only works out again the goods whose price its own trading moved, instead of every good on the map

## 1.41.5

- The mod description now calls Live world prices by the name the settings screen gives it
- The mod description now says a pin comes off a town by itself once TradeLord has traded there
- The mod description now says Share of the hold one good may fill ships at 45%

## 1.41.4

- The price lines and the profit colouring in your inventory cost less on every row of the screen
- Working out which markets pay best for a good costs less each time

## 1.41.3

- Buying at a market no longer asks twice of every good on the shelf whether you allow TradeLord to buy it
- Buying no longer works out what your party can still carry when your purse or a spending cap has already stopped the purchase
- The campaign map no longer checks every panel on screen for a text box on every frame, only when you press the TradeLord hotkey

## 1.41.2

- Walking into a market, each new day on the road and buying livestock no longer count how many animals your party can drive one animal at a time
- Thinning your herd no longer counts how many haul animals your cargo can spare one animal at a time
- The daily check on the road now works out how many animals must go once instead of twice
- Whether your animals are slowing you down is now decided on a real difference in your speed, not the smallest one the game can report
- TradeLord no longer reopens its log for every line it writes, so a busy market visit costs the game less
- Auto-mark best sell town on map no longer prices your cargo in a town whose gold could never beat the best one found so far

## 1.41.1

- When grain is what TradeLord left alone, it now says grain fills the cargo for little return instead of naming a setting

## 1.41.0

- Share of the hold one good may fill now ships at 45% instead of off, so one cheap good can no longer take your whole cargo
- A town you pinned on the map loses its pin once TradeLord has traded there, instead of pins piling up until you clear each one by hand
- Fixed a good held back by Never buy grain being reported as on your never sell or never buy list when those lists were empty
- Buying a large stack no longer works out what your party can carry all over again for every single unit

## 1.40.4

- Fixed an item list entry such as Iron Ore also covering a different good whose short name is one of the words in it
- An item list entry that TradeLord.log already said matches no good now really covers nothing, instead of quietly covering part of one

## 1.40.3

- Trading a large stack with a caravan or villagers on the road no longer costs the game extra work on every unit, the way a market stopped doing in 1.40.2
- A dry run now says you already traded a good on this visit when that is what stopped it selling more, instead of blaming your food reserve

## 1.40.2

- Fixed trading a large stack at a market costing the game extra work on every single unit, which started in 1.40.1

## 1.40.1

- The download now carries every change made since 1.40.0
- Quick sell and quick buy now stop quietly when your party cannot be read, instead of writing an error to TradeLord.log
- Selling to a caravan or a party of villagers on the road works out what you took in once instead of twice

## 1.40.0

- Fixed Silence trade messages not covering a trade with a caravan or a party of villagers you meet on the road
- The Always sell hint now says an animal a quest is waiting on is still held back, alongside your never sell list and an inventory lock
- Trading in a market no longer works out which markets are in reach all over again after each pass that moves goods, so a busy town settles faster

## 1.39.3

- Restocking food and buying a haul animal stop before your gold reaches your gold reserve again, and buying for profit is the one pass that spends down to it

## 1.39.2

- An animal a quest is waiting on is now kept back even where your always sell list names it

## 1.39.1

- Restocking food and buying a haul animal now spend down to your gold reserve, the way buying for profit already did, instead of leaving a denar above it

## 1.39.0

- Selling an animal to get your party back up to speed now counts towards the profit TradeLord has made you and earns Trade skill
- A visit that trades nothing now says a quest may be waiting on your animals when that is what held them back, instead of naming your food reserve
- The best markets and the routes on the panel are worked out again as you ride, instead of waiting for the hour to turn

## 1.38.5

- Fixed TradeLord matching its own lines to their translations differently depending on the language your computer is set to
- The mod description now says asking a band to let you pass is a line you say to them, rather than the pop up it used to be

## 1.38.4

- Selling animals to get your party back up to speed now leaves a quest item alone, the way the rest of TradeLord's selling already did
- Fixed the lines you can say in a conversation coming out in the wrong order for the rest of your session, when the free passage line could not be placed

## 1.38.3

- Fixed the line asking a band to let you pass never appearing when you met one

## 1.38.2

- Fixed a campaign save failing over TradeLord's own note keeping, which now lets the save through and writes what went wrong to TradeLord.log

## 1.38.1

- Fixed an error TradeLord wrote to its log on every startup, and went on writing until a campaign was loaded

## 1.38.0

- Fixed the game closing itself when you met bandits, because the offer of free passage arrived as a pop up over the talk you were already having
- Asking a band to let you pass is now a line you say to them, in among your other answers, instead of a pop up
- The bandits answer, the talk closes, and your party rides on, so nothing is left half finished behind the conversation
- Free passage from bandits now says on the settings screen that it adds a line to what you can say to them

## 1.37.10

- A visit that trades nothing now names which protection held your goods back, instead of saying only that your protections did
- Where more than one protection was in the way, the message names the first one TradeLord met
- The warning that your cargo is full now tells you what to do about it, recruit more men, buy more horses, or sell goods yourself

## 1.37.9

- Animals a quest is waiting on are now held back from every sale, not only from thinning the herd, even when TradeLord cannot tell which animals the quest wants
- Fixed a good a market stocks twice slipping past your limits on how much of one good to hold

## 1.37.8

- Trading with a party on the road now stops at your Max spend per visit, instead of buying on until your purse is down to your gold reserve
- The campaign map no longer works out every trade route the moment it opens, so a long campaign no longer hitches before you have even opened the ledger

## 1.37.7

- Protect unique and crafted items now covers animals as well as gear, so a unique animal is no longer sold in the trading pass while the setting is on

## 1.37.6

- Simulation mode now models a visit as one visit rather than each pass on its own, so a dry run no longer reports more trading than a real visit would do
- Fixed simulation mode selling the same animals again every time it checked your speed, and offering to buy goods an earlier pass had already taken off the shelf
- Simulation mode now spends what a meeting on the road just earned, and no longer buys back the goods it has just sold to that party

## 1.37.5

- Fixed the cattle or horses you are carrying to deliver for a quest being sold as you walk into a market

## 1.37.4

- Fixed looted goods being counted as something you paid for, when you had eaten the last of that food earlier the same day

## 1.37.3

- Food your troops eat, and anything else that leaves your party without being sold, no longer counts as still bought, so looted goods stop being held back and the next lot you buy is no longer sold too cheaply

## 1.37.2

- Restocking your food buys grain again, so it works in a farming village where grain is the only cheap food, and Never buy grain goes back to keeping grain out of trading for profit
- The inventory lock setting now says how each side matches a lock, selling by item and quality, buying by item alone

## 1.37.1

- The free passage setting now describes what it really does, asking you as you meet the band rather than adding a line to the encounter screen
- The setting that sells animals to get your party back up to speed now says that an animal a quest is waiting on is left alone
- The mod description now says getting your party back up to speed keeps back the animals a quest is waiting on

## 1.37.0

- Fixed getting your party back up to speed selling the animals a quest is waiting on, which now keeps back as many as the quest asks for and thins only the herd beyond them

## 1.36.2

- Fixed a lock on a horse of a particular quality, such as a spirited or a lame one, being ignored when TradeLord sells one to get your party back up to speed
- A good you buy by hand is now written down at what its own quality cost you, so TradeLord no longer sells a fine one on for less than you paid

## 1.36.1

- Fixed a caravan or a party of villagers you have just traded with trading all over again when you talk to them without riding away first
- Selling to a caravan or villagers on the road now makes the coin sound, the way selling in a town does
- Reset on the settings screen now always takes hold at once and is written to TradeLord.ini

## 1.36.0

- Fixed trading with a caravan or a party of villagers on the road failing every time, since the game will not sell without a market, so TradeLord now hands the goods and the coin over itself
- Fixed the free passage line turning up among your battle orders before a fight
- Bandits who have let you go now leave you be for a few hours, instead of turning round and hitting you again the moment the game unpauses
- The language you pick now takes hold when you press Done on the settings screen, and the ledger panel changes over with it
- TradeLord.log says how long the free passage holds and which band it holds off you
- The mod description now says the free passage leaves the band off you afterwards

## 1.35.2

- The language setting is back at the top of the settings screen, with auto sell and auto buy just under it
- Dragging a slider on the settings screen no longer fills TradeLord.log with every value it passes through, and your change is written down once, when the slider comes to rest

## 1.35.1

- Fixed turning down the bandits' offer of free passage stopping that same band ever offering it again

## 1.35.0

- The language you pick now reaches the town menu entries as well, so Trade here now and Consult the TradeLord ledger change over the moment you pick it
- Auto sell and auto buy now sit at the very top of the settings screen, above the language setting
- Observation shelf life is gone, and a price you recorded yourself is now kept for as long as you have it rather than thrown away at a certain age
- Quiet automation is called Silence trade messages now
- How many of each kind to keep is called How many of each kind of food to keep, and its hint says it counts the food itself rather than days
- It now starts at two of each kind rather than three
- The Reset button now puts your settings back at once instead of taking several seconds over it
- The hint under Ledger panel hotkey (map screen) now says that one key name is all it takes, and that a word or an unknown key falls back to T
- TradeLord.log now says how many of your settings had been changed when you press Reset
- The mod description no longer offers a shelf life for the prices you recorded yourself, and now says the language reaches your town menu entries too

## 1.34.0

- TradeLord now trades with a caravan the moment you meet it on the road, before anyone says a word, instead of waiting for you to pick a line of dialogue
- A party of villagers on the road now trades with you the same way a caravan does
- Trade with caravans you meet is called Trade with caravans and villagers you meet now, and it governs both
- Bandits now offer you free passage as soon as you meet them, asking whether to ride on or fight, so the offer no longer waits on a menu that never appeared for some of you
- TradeLord.log calls these a sale or a purchase on the road now, and names the party it traded with
- TradeLord.log names the band each time free passage is offered
- The mod description now says a caravan or a party of villagers is traded with the moment you meet them, and that bandits offer you free passage as you meet them

## 1.33.0

- Your setting for buying haul animals is called BuyHaulAnimals in TradeLord.ini now, and whatever you had saved under its old name is carried over the first time this version reads your file
- The hint under Share of the hold one good may fill now calls them haul animals, the same name the rest of the settings screen already uses
- TradeLord.log now calls a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse and a Pack Camel a haul animal, rather than an animal that carries for you
- TradeLord.log no longer opens every campaign with a roll call of every animal in your game, so the log starts on what you actually did
- The hint under the Language setting now says what really happens, that the language takes hold as you pick it and your town menu entries follow the next time you load a campaign
- The mod description now covers things TradeLord already did, the rules a caravan on the road is held to, the three herd checks a visit, the Reset button, and TradeLord.ini written with or without Mod Options

## 1.32.0

- The ledger panel now counts the gold in your purse when it works out how many of a good a route is worth, so it no longer offers you 32 of something you can only pay for 3 of
- Your gold reserve is held back from that count too, the same way it is when TradeLord buys for you, and so is your spending cap for the visit
- When your purse is what is holding the ledger back, the panel now says so and names your gold and your reserve, instead of blaming your travel ceilings
- The mod description now says your own purse is counted into every route the ledger quotes

## 1.31.1

- The mod description now calls a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse and a Pack Camel haul animals where it describes getting your party back up to speed

## 1.31.0

- Trading with a caravan on the road now sells the goods you looted when the ones you bought fall short of your profit margin, the way a market does
- Hold cargo for the best market now holds your cargo back from a caravan on the road as well, instead of only from a town
- A caravan can no longer sell you enough livestock to drop your party into the herd speed penalty
- Stop buying at this many held and Share of the hold one good may fill now hold when you buy from a caravan, not only in a market
- Economy settling delay now holds on the road too, so a caravan will not trade with you before the day you set either

## 1.30.3

- Fixed installing Mod Options after playing without it replacing every setting in TradeLord.ini with the ones TradeLord ships with
- TradeLord.log now says why your settings file was the one TradeLord read

## 1.30.2

- Fixed loading a saved game inside a town or village stopping TradeLord selling the animals that are slowing your party down as you ride back out
- TradeLord.log no longer says a trade was turned down as you leave a castle, a hideout or anywhere else TradeLord never trades in the first place
- With Buy to fill the ships off, getting your party back up to speed now keeps enough haul animals for what your carts are carrying, rather than your ships
- TradeLord.log now says once why TradeLord cannot work out your herd, whichever part of it asked first

## 1.30.1

- The mod description and what it needs now both say TradeLord runs on the Bannerlord 1.5.2.121216 beta as well as on 1.4.8.119303

## 1.30.0

- TradeLord now buys only the animals that carry for you, a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse or a Pack Camel, so it no longer spends your gold on a Rouncey or a Charger while a Mule sits in the same market
- It never pays more than the cheapest price it knows of for a haul animal, so Most it will pay for a haul animal when your bags are full is gone from the settings screen
- Restocking food now buys only where the market is asking no more than the cheapest price you know of, instead of taking the cheapest thing on the shelf whatever it costs
- Buying a haul animal and restocking food both stop before your gold reaches your reserve, so there is always something left to trade with
- Buy haul animals and mounts is called Buy haul animals now, because riding horses and camels are no longer among the animals it buys
- The hints under Buy haul animals and Restock food (days of supply) say what those settings now do, and the mod description follows them

## 1.29.1

- Fixed TradeLord selling animals on the way out of a settlement it had refused to trade in on the way in
- When it holds back for that reason it says so in TradeLord.log, naming the settlement

## 1.29.0

- TradeLord now looks at your herd three times a visit, as you enter a settlement, again once it has finished trading, and once more as you leave, so a penalty that arrives while you are in town is caught before you ride out
- Losing men in a battle or to desertion shrinks how much your party can drive, and each of those three checks now notices that on its own
- While you are on the road with no market in reach, TradeLord.log names the herd once a day for as long as it is slowing you down
- Every herd check is written to TradeLord.log with the numbers behind it, your men and how many are on foot, your loose mounts, your pack animals, your livestock, and how many must go
- Every animal that comes into your party or goes out of it is now named in TradeLord.log with what was paid, why it moved, and what TradeLord counts it as

## 1.28.1

- Getting your party back up to speed now works out how many haul animals your cargo needs the same way TradeLord works out what you can carry, so a party that never puts to sea will part with a spare

## 1.28.0

- Getting your party back up to speed now sells in a set order, livestock first, then a spare riding horse or camel nobody is riding, then your haul animals, and your war horses and noble horses last of all
- It never sells a horse one of your men on foot is riding, because that horse is not in the herd and selling it would not speed you up at all
- Livestock and haul animals can go this way now, so a party that has picked up more mules than its men can drive is no longer stuck slow
- It keeps enough haul animals to carry what your party is already carrying, so getting back up to speed can never leave your cargo on the ground
- The setting that governs all this is now called Sell animals that slow you down, because spare mounts are no longer the only thing it sells
- A Saddle Horse is now one of the animals TradeLord buys for your baggage train and keeps out of its ordinary selling
- A Mule or a Pack Camel that cannot be ridden is no longer kept as a haul animal, so it is now sold like any other cargo
- The hint under the Language setting now says only that a language change needs the game restarted
- The animal roll call in TradeLord.log now also says whether the game counts each animal as food and whether it can be traded at all
- TradeLord.log now names both animals, with the item id for each, when an entry on one of your item lists means two that TradeLord treats differently, such as Saddle Horse

## 1.27.5

- The hint under the language setting is back to plain English, without the paragraph about the settings screen that 1.27.4 added

## 1.27.4

- The language you pick now reaches the settings screen itself, the name and hint of every setting, the headings above them and the Reset button, as you pick it
- The words inside each list of choices follow the next time you open the settings screen, and the hint under the language setting now says as much

## 1.27.3

- Fixed your settings in TradeLord.ini being written over with the shipped ones as the game starts, whenever Mod Options had not finished loading yet
- TradeLord reads the file as it stands in that case, so anything you set by hand survives, and the settings screen takes over as soon as Mod Options is ready

## 1.27.2

- Fixed Bannerlord closing itself as it reaches the main menu with TradeLord and Mod Options installed together, which it had done since 1.26.0
- The TradeLord settings screen comes back with it, Reset button and every setting on it

## 1.27.1

- Picking a new language on the settings screen now takes hold as you set it, so the choices under each setting are spoken in it straight away
- The screen is asked to draw itself again at the same moment, so its own names, hints and headings follow the new language too
- The hint under the language setting no longer says the screen changes language the next time you open it, because that was not what happened

## 1.27.0

- TradeLord.log now opens every campaign with a roll call of every animal in your game, counted by what TradeLord does with each one
- Each animal is listed with the game's own answers about it, so a mod that adds an animal or changes what an existing one does shows up plainly

## 1.26.1

- A number in TradeLord.ini is now held to the same limits the settings screen holds it to, so a damaged or hand edited file cannot put TradeLord into a state you could never have set yourself
- A number that has been pulled back inside its limits says so in TradeLord.log, naming the setting, what the file asked for and what was used instead

## 1.26.0

- Added a Reset button at the very top of the settings screen, which puts every setting back to the value TradeLord ships with in one click
- Resetting empties your never sell, always sell, never buy and always buy lists too, and is written into TradeLord.ini straight away so nothing is left half changed
- TradeLord.log now lists every setting you have moved away from the value TradeLord ships with, each time you start the game
- Changing a setting is written to TradeLord.log as it happens, naming what it was, what it is now, and what TradeLord ships with
- The button on the settings screen is spoken in the language TradeLord is set to, like the rest of that screen

## 1.25.0

- TradeLord.ini and the MCM settings screen are twins now, so you can install or remove MCM whenever you like and every setting you have made comes with you
- Each of them records when it was last saved, and if the two ever disagree the one saved last wins and the other is written to match it
- Editing TradeLord.ini by hand while MCM is installed now works, because TradeLord sees the file is the newer one and sets the settings screen from it
- Changing anything on the settings screen writes it straight into TradeLord.ini, so removing MCM never costs you a setting

## 1.24.0

- TradeLord.ini now says which shape it is in, and TradeLord brings an older file forward by itself when it changes how a setting works, so a setting you set is never quietly lost
- A file that still keeps some of every kind of food as a single number is carried over to the switch and the amount that replaced it
- A file that still has keeping smeltable weapons as a switch is carried over to the choice of three that replaced it, on becoming keep every one and off becoming sell them
- Everything TradeLord carries over is written to TradeLord.log in plain words, so you can see what it did with a setting you had set

## 1.23.1

- The note about an animal that carries nothing for you should not have offered the Old Work Horse as an example, because a work horse carries for you whatever its age

## 1.23.0

- An animal that carries nothing for you and is not livestock, such as an Old Work Horse, is no longer kept with your haul animals, and is sold like any other cargo instead of sitting in your bags for good
- The setting for how much more TradeLord will pay while your bags are full is now called Most it will pay for a haul animal when your bags are full, and is explained in plain words
- The mod description now says where the line falls between a haul animal TradeLord keeps for you and an animal it sells

## 1.22.0

- TradeLord now sells a spare mount nobody in your party can ride, once those spares are dragging you into the herd speed penalty, so your footmen turning into cavalry no longer leaves the party crawling
- It sells the cheapest spare first, so your war horses stay in the baggage, it never sells a haul animal that way, and one switch turns the whole thing off
- A mule, a sumpter horse, a work horse and a pack camel are now called haul animals on the settings screen and in the mod description, and all four are named rather than described
- The mod description now says TradeLord pays up to the cheapest price it knows of for a haul animal, and up to 1.5 times that while your cargo is full
- The livestock policy, the unique and crafted protection and the always sell list no longer say a mount can never be sold, because now one can

## 1.21.1

- The mod description now names the animals TradeLord buys for your baggage train, a mule, a sumpter horse, a work horse or a pack camel, and says it buys a mount your men can ride while you still have troops on foot

## 1.21.0

- Looted gear now sells to the first market that can pay for it, and Hold cargo for the best market, which is off out of the box, is now the only thing that makes any of your cargo wait for a better town
- Asking bandits for free passage is no longer labelled a cheat, the line in the encounter screen now reads [TRADELORD] and its setting is called Free passage from bandits
- The setting that buys mules and horses now calls them animals rather than beasts
- The mod description now says how much gold the town you would sell to has rather than what is in its till, names your gold reserve as 300 denars, and says what the settings you get out of the box are aimed at

## 1.20.1

- The line saying TradeLord bought pack animals now comes just after the line crediting your Trade skill, rather than before it
- The mod description now covers what the ledger panel shows along its top, the warnings TradeLord puts on screen, leaving a village its last of each good, and naming a mount on your always sell list to sell it

## 1.20.0

- TradeLord now trades with a caravan you meet on the road, so talk to it and the deal is already done, paid out of the caravan's own purse
- Added a setting capping how much of your hold one good may fill as a share rather than a flat count, so the ceiling grows with your carts
- A horse one of your unmounted men can ride no longer counts against the herd, so a party driving a full herd still buys the cheap horses its footmen can climb onto
- The mod description covers trading with caravans and the new share of the hold

## 1.19.0

- Keeping smeltable weapons is now a choice of three, and the new one keeps a weapon only while a part of it is still locked in your smithy, so your bags stop filling with loot that can teach you nothing
- Buying beasts now covers riding horses as well as mules and sumpter horses, so a cheap horse gets bought too and your foot troops have something to ride
- Asking bandits to let you go is now on out of the box, and one switch turns it off
- The mod description explains the three ways to play the smeltable weapon setting

## 1.18.0

- TradeLord now buys mules and sumpter horses when a market is asking no more than one is worth, so your party can carry more, and it will pay half again as much for one while your cargo is full
- It never buys more pack animals than your party can drive without slowing down, and your gold reserve and your spending limit for the visit still hold
- Restocking now tops your food back up to three days of supply instead of five
- Added a cheat, off until you switch it on, that lets you ask looters, sea raiders and the rest of Calradia's bandits to let you go, with no fight and no ransom
- The setting that keeps smeltable weapons now says plainly what it holds back, every weapon built from smithing parts, looted ones included, whatever parts you have already learned
- The mod description now covers what TradeLord buys for your baggage train and what it leaves alone, and what it needs installed has moved to the foot of it

## 1.17.0

- TradeLord now tops your food back up to five days of supply as you trade, buying whatever food a market has going cheapest, before it trades for profit
- It now holds back three days of your troops' wages on top of your gold reserve, so a shopping trip never eats the payroll
- Added a switch that keeps every weapon the smithy can break down for parts, so a smithing playthrough stops selling off its own raw material
- Added a switch that sizes what it buys to what your ships can hold rather than what your carts can, for a War Sails fleet
- The ledger panel's cargo line follows that same capacity, so it counts the ships whenever that switch is on
- Without MCM installed, TradeLord now writes a TradeLord.ini beside its log that you can edit to change any of its settings
- The notice about trading as you arrive now points you at that file when MCM is missing, instead of telling you to go and install MCM
- Fixed Default on the settings screen leaving every setting exactly as it was, instead of putting each one back to what TradeLord ships with

## 1.16.1

- The switch that keeps every kind of food now names the food variety morale bonus the way the game does

## 1.16.0

- Keeping some of every kind of food is now a switch you turn on, with how many to keep set separately and starting at three of each

## 1.15.0

- TradeLord can now hold back a few of every kind of food you carry, so selling your stores no longer costs your party the morale bonus it gets from eating a variety of things

## 1.14.5

- The ledger panel's title, buttons and column headings now follow the language you pick, instead of staying in the one they were first drawn in

## 1.14.4

- Fixed TradeLord saying a market has nothing worth trading when the reason nothing sold was your own cargo

## 1.14.3

- A market whose merchant has run out of gold is no longer named as the best place to sell, so TradeLord stops buying cargo for a town that cannot pay for it and stops holding goods back for one

## 1.14.2

- TradeLord no longer buys a good that your own settings will never let it sell, so a category you set to buy only no longer fills your cargo with goods it will not move on
- The ledger panel opens faster, most noticeably in a long campaign with many markets in reach

## 1.14.1

- The ledger panel no longer opens on its hotkey while you are typing into a box on the campaign map
- The best market tolerance hint now says the price floor it sets always holds back goods you never bought, such as looted gear, and not only when the setting above it is on

## 1.14.0

- TradeLord now speaks Simplified Chinese, chosen from the same Language setting that already offered English, Turkish and Russian

## 1.13.3

- The notice that TradeLord trades as you arrive is shorter, so it sits on screen like its other messages instead of running long

## 1.13.2

- Without MCM installed there is no settings screen, so the notice that TradeLord trades as you arrive now says that plainly and names MCM, instead of sending you to settings you do not have

## 1.13.1

- Fixed the best market marker pointing at a town whose merchant has no gold left to pay for your cargo
- Fixed TradeLord warning that it cannot buy at a market when you have turned both auto buy and its town menu trade entry off
- A panel hotkey with something other than Ctrl, Alt or Shift in front of the key now says so in the log, rather than quietly opening on the bare key
- Walking into a market is a little quicker when your never sell, always sell, never buy and always buy lists are all empty

## 1.13.0

- The settings screen now ends with Selling and Buying, side by side, with every other setting above them
- The settings that shape both halves of a trade, along with the two town menu entries, have moved into General
- The two switches that trade as you arrive are now called Auto sell and Auto buy
- The separate switch that decided whether TradeLord could buy at all is gone, so buying now follows Auto buy and the trade entry in the menu alone
- The note under the food policy no longer says where on the screen the food reserve sits, since it has moved

## 1.12.1

- A market where TradeLord traded nothing at all now says so in one line instead of two, naming what stopped it
- A market where TradeLord did trade no longer also tells you the other half of the pass moved nothing

## 1.12.0

- TradeLord now tells you on screen why it traded nothing at a market, when it trades as you arrive, instead of writing the reason to its log alone
- Turning quiet automation on keeps those new lines off the screen along with the rest
- TradeLord now speaks Russian, chosen from the same Language setting that already offered English and Turkish
- Added a setting that hides the TradeLord ledger entry in the town and village menus, the way the trade entry could already be hidden
- The ledger panel has dropped its Data column, and its item, buy town and sell town columns are wider for the room

## 1.11.1

- Gaining or losing ships now changes which markets TradeLord counts as being in reach straight away, instead of leaving some of them out until the hour turns

## 1.11.0

- The food, smithing material and livestock rules are now picked from a list that names each choice, instead of a slider you set to 0, 1, 2 or 3
- The setting that decides what a good counts as having cost you is picked from a list too, and it and its note are written in plain words now
- Selling looted gear now starts switched on at tier 1, which is what looters and bandits drop, and you can still raise it or switch it off
- Added an always buy list to match the always sell one, so a good named on it is bought past the category rules and past the never buy grain switch
- Your never sell and never buy lists and anything you locked in the inventory still hold, and it still buys only what it can sell on for more somewhere in reach
- Switching those four settings over to lists keeps whatever you had already picked, so nothing resets

## 1.10.0

- The settings screen now reads in the language you picked in TradeLord's own Language setting, instead of the language the game itself is set to
- The screen takes a new language the next time you open it, and the note under the Language setting now says so

## 1.9.1

- The ledger panel now opens with T instead of L, which the game already uses for one of its own screens
- The key is still yours to set in TradeLord's settings, and one you have already chosen there is left alone

## 1.9.0

- The switches that decide when TradeLord sells and buys now say plainly what they do, instead of naming quick sell and quick buy entries the town menu no longer has
- Every other setting that still talked about quick sell and quick buy now names selling and buying instead, in English and in Turkish
- The auto trade switch is gone, since it only read back the two switches above it and turned them on and off together

## 1.8.0

- Added a setting that stops quick buy adding to a good once you already carry as many as you allow, so a stock of hundreds no longer grows every time you walk into a market
- The town and village menus now carry one TradeLord trade entry instead of three, and it sells and then buys in one go
- Trading done automatically as you arrive is unchanged, and the setting that showed the old entries now shows or hides the single one

## 1.7.0

- TradeLord now speaks Turkish, from a new Language setting at the top of its options, and it starts on English
- Playing Bannerlord in Turkish now puts the whole of TradeLord into Turkish, its settings screen included

## 1.6.33

- The settings screen is now built against Mount and Blade Mod Configuration Menu 5.12.3

## 1.6.32

- Fixed naming a good on the never sell, always sell or never buy list quietly catching a second good whose whole name is one of the words you wrote, so Iron Ore now leaves Iron alone
- The ledger panel now spends your per item and per visit denar caps unit by unit, the way a buying pass spends them, so a route no longer offers a quantity your caps would stop you buying

## 1.6.31

- The town marked on the map for your cargo is now one whose merchants can actually pay for it, instead of one that quotes a high price but has run out of gold
- Fixed a map pin that cannot be put back when a save loads taking the TradeLord entries out of the town menu with it

## 1.6.30

- A pass that trades nothing now gives the same reason every time, instead of naming a different one from visit to visit when two rules held back as much as each other
- The ledger panel and the price tooltips open with less of a pause, because your party speed is no longer read again for every market measured

## 1.6.29

- TradeLord now tells you as you enter a market when your purse is at or under your gold reserve, so you know why it is buying nothing
- A pass that bought nothing now names your purse or your spending caps as the reason whenever that is what stopped it, instead of blaming prices or saying nothing at all
- The cargo full warning stays quiet when the purse warning has already been given, since selling clears both

## 1.6.28

- The warning that your cargo is full is now given once, as you enter a market, instead of a second time as you leave it

## 1.6.27

- An entry on one of your item lists that still matches no good after you correct it now says so on screen again, instead of going quiet after the first warning
- Changing a setting no longer sends TradeLord through every good in the game when your never sell, always sell and never buy lists are all empty

## 1.6.26

- The never sell, always sell and never buy lists now take the name a good is shown under, such as Iron Ore, as well as its item id, so a name with a space in it no longer reads as two entries that match nothing
- An entry on one of those lists that matches no good in the game is now named on screen and in the log, instead of quietly doing nothing
- Added a quiet automation setting that keeps trading done on entry to the log, off your screen
- Starting a second campaign without closing the game now logs the ledger panel's hotkey and map button again, rather than staying silent about the second one

## 1.6.25

- The message shown when the ledger has no routes for you now points at your travel ceilings, and only asks you to visit more markets when TradeLord is set to use the prices you have seen in person

## 1.6.24

- The first market of a campaign is now left alone when TradeLord tells you it trades on entry, so you can turn that off before it does anything, and it starts trading from the next market

## 1.6.23

- TradeLord is now built against Bannerlord 1.4.8.119303, the version it is played and tested on, and it still runs on 1.4.7 and the 1.5.1 beta

## 1.6.22

- The ledger panel now opens with L instead of T, because T also opens the game's own message panel and the two fought over the key
- Buy cap per item can now be set to 0 to turn the cap off, the way the two settings beside it already could
- Scan radius, observation shelf life and buy cap per item now say on the setting itself what 0 does
- A market where TradeLord bought nothing because your gold reserve or spending cap was reached now says so, instead of trading quietly and leaving you to guess
- The item tooltip no longer puts a TradeLord heading above the prices it adds
- The line of notes under the ledger is now large enough to read

## 1.6.21

- The cargo full warning now appears only on a visit where TradeLord traded nothing, instead of following a pass that had just bought until the hold was full
- Profit credited to your Trade skill is reported in one line instead of two, and that line names your new Trade level when the skill rises

## 1.6.20

- Note: a campaign last saved by TradeLord 1.6.18 or earlier no longer opens, and campaigns saved by 1.6.19 and later are unaffected
- A price or purchase record for an item whose name carries an unusual character is now left out of the save instead of coming back as a record for an item that does not exist

## 1.6.19

- A campaign saved by this version can have TradeLord removed from it later and still open, instead of the save refusing to load without the mod
- The price ledger and your purchase records carry over from a campaign that was saved by an earlier version

## 1.6.18

- TradeLord now says once in each campaign that it buys and sells for you as you enter a market, and names the two settings that turn that off

## 1.6.17

- Fixed the TradeLord entries in the town and village menus going missing when the map marker cannot be restored as your save loads
- A menu the game does not have no longer costs TradeLord the entries in the menus that it does have
- Fixed starting another campaign without closing the game leaving livestock buying switched off because of something that happened in the first one

## 1.6.16

- A market that pays less than the town you are holding your cargo for now says so, instead of blaming your profit margin
- A herd already as large as your party can drive now says so, instead of reporting that there is no room to carry more
- The town TradeLord marks as your best place to sell is put back on the map as soon as you load a save, instead of waiting for the next day or the next settlement you enter

## 1.6.15

- The log now names every market that traded nothing, instead of falling silent after the first market that gave the same reasons
- Fixed the log going silent about problems in a second campaign started without closing the game
- A recorded price your save cannot read is now dropped as the ledger loads, instead of stopping the ledger from loading at all
- With observed prices set never to expire, your save no longer keeps an entry for a good whose recorded prices have all gone

## 1.6.14

- The town TradeLord marks as your best place to sell no longer removes a marker you had placed on that town yourself
- A town you pinned in the ledger panel is marked on the map again as soon as you load the save
- Turning live world prices off while you are standing in a market now records that market at once, instead of leaving it blank until you leave and come back

## 1.6.13

- A line of MCM newer than the one this build was made for is now named in the log as exactly that, instead of being reported as MCM missing entirely

## 1.6.12

- Selling goods you never bought, such as loot, now counts profit and Trade XP against what those goods would have cost you at the cheapest market you know of, instead of treating the whole sale price as profit

## 1.6.11

- Your save no longer keeps a purchase record for every good you have finished selling, so a long campaign stops collecting a dead entry for each item you have ever traded

## 1.6.10

- The TradeLord button on the campaign map now reserves only the space the button itself covers, instead of a fixed strip that reached well past it on ultrawide and other non 16:9 screens

## 1.6.9

- Every setting name, hint and heading on the settings screen now ships in the language file and can be translated, which was the last part of the mod left in English only

## 1.6.8

- Fixed the TradeLord button on the campaign map swallowing map clicks well above and below the button itself
- Fixed part of the food you asked TradeLord to keep being spent on goods quick sell was never going to sell
- Other mods now get first say over their own notifications while TradeLord is trading

## 1.6.7

- The Trade XP message now names its number for what it is, the denars of profit credited to your Trade skill
- Clicking a town in the ledger panel now pins it even when TradeLord is already marking that town as your best place to sell
- Quick sell no longer spends part of your food reserve on goods it then passes over because you bought them here on this visit
- Fixed a trade message the game refuses to show leaving the rest of that pass's messages repeating

## 1.6.6

- Fixed putting an item on the always sell list making quick buy purchase it against your category policy
- Fixed loading your settings switching quick buy back on when your saved settings say it should be off
- The route lines in the ledger popup are now translatable, like every other line the mod shows

## 1.6.5

- A village that is down to its last of each good now says so, instead of reporting that it has nothing worth trading
- The ledger panel now reopens instantly within the same game hour, and the Refresh button rescans whenever you want fresh numbers
- Trade messages left over from a campaign you have closed no longer appear once you are back at the main menu
- A damaged never sell, always sell or never buy list in the settings file no longer stops trading outright

## 1.6.4

- TradeLord now warns you in red when your cargo is full, both as you walk into a market and as you leave it, instead of quietly buying nothing

## 1.6.3

- Fixed a release going out with an empty description, the way 1.6.1 did

## 1.6.2

- Every setting now explains itself on hover, where keep food, never sell, always sell and the coin sound had no description before
- The never sell and always sell descriptions now say where to find an item id and which of the two lists wins when they disagree
- The buying settings are now written in the order they appear on screen
- A fault while showing a trade message, or while closing a campaign, is now logged and stepped over instead of reaching the game
- Asking the ledger what a good cost when it has never seen that good now answers instead of faulting
- Tidied the layout of the two trading passes, which behave exactly as before

## 1.6.1

- The Trade XP a trade earns is now reported in amber, right after the sold and bought lines instead of before them
- The gold reserve now starts at 300 denars instead of 100, enough for two bribes for safe passage and a wage payment, and your own setting is unchanged

## 1.6.0

- Fixed a trade that failed partway through being able to stop the game showing any notifications at all until you reloaded, including TradeLord's own trade summaries
- Ending a campaign no longer leaves leftover trade state behind for the next one
- Every settings description is rewritten in plain English
- The panel and the ledger popup now say resale safety factor, the same name the settings screen uses, instead of resale haircut
- Added this changelog

## 1.5.11

- The black star in tooltips is now a plain asterisk, because the game's font may not have that character and would draw an empty box
- Fixed the panel's workshop board listing every workshop in Calradia even with live world prices switched off, where it now lists your own
- The panel's Total profit line is now called TradeLord profit, which is what it actually counts
- The settings hints no longer mention other trade mods

## 1.5.10

- Fixed a town with no price for a good being called the cheapest place to buy it, because a missing price read as 0 and 0 sorts cheapest
- Fixed the mod failing to load at all when your Documents folder path could not be worked out

## 1.5.9

- Fixed a town pinned in the ledger panel not coming unpinned after you reloaded the campaign
- Fixed that same problem letting the automatic marker delete a pin you had placed by hand

## 1.5.8

- Fixed the ledger panel having no translatable text at all, so anyone playing in another language got an English panel
- Fixed the tooltip's Profit, Stock and days labels not being translatable
- Every line the mod shows now carries a translation marker and ships in the language file, so the whole mod can be translated without touching the source

## 1.5.7

- Fixed a stack of part bought, part looted goods being refused entirely when the bought units missed your profit margin, stranding the looted ones that had no cost to clear
- The purchase record is now drawn down against the units it actually covers, in every cost basis mode

## 1.5.6

- Fixed the log being written to the game's program folder, where the write fails silently under Program Files, and it now prefers your Bannerlord user folder
- Fixed live price mode saving a price note for every good in every settlement you entered, which nothing in that mode ever reads, so existing saves shrink
- The message filter now silences only the game's own trade messages during a pass, not other mods'
- Fixed a purchase you made yourself being recorded at the price after the trade, which is higher than what you actually paid
- A failed panel setup is now retried twice more before being given up on
- A panel hotkey the game cannot name is now reported in the log instead of silently becoming T
- Fixed the cargo marker picking the first town it scanned even when the cargo was worth nothing anywhere

## 1.5.5

- Fixed a dry run blocking the real trade that followed it in the same visit
- Fixed the best market floor never reaching the loot it exists to protect
- Fixed a mixed stack charging the looted units a cost they never had

## 1.5.4

- Fixed a release still being able to go missing when it timed out on its way out
- Fixed three refusals being answered with a message that was not true, such as blaming your goods when the merchant was simply out of money
- The panel now marks which routes it could not price unit by unit
- Fixed an English footnote sitting in an otherwise translated window

## 1.5.3

- The mod description's count of releases, and the version it named, are corrected
- The Nexus description of what the four Harmony patches do is corrected

## 1.5.2

- Fixed the panel listing round trips it could only half make, when a category was set to buy only
- Fixed the War Sails port menus being added by trying and hoping rather than checking
- Fixed the panel and the tooltip quoting different prices for the same shelf
- Two fields nothing ever read are gone from the internal price quote
- Fixed a release going missing when it timed out on its way out

## 1.5.1

- Fixed observed price mode quoting live prices through the new bulk pricing
- Fixed a shelf that cannot be priced per unit being asked for its price once per unit, giving the same answer every time
- Fixed the confidence score partly measuring two of the game's own price readings disagreeing with each other
- Fixed the nothing traded message usually naming the wrong reason, because loot in your inventory drowned out the real one
- Fixed two settings sharing a position with two others in the Action group
- The quiet no trade log line no longer repeats itself at every town gate

## 1.5.0

- The panel now prices a whole lot one unit at a time, the way you will actually buy it, instead of multiplying one unit's spread by the quantity
- Routes are now ranked by how likely their profit is to survive the trip, not by profit alone
- When a pass trades nothing, it now says which rule stopped it
- Food, smithing materials and livestock each take a policy now (ignore, sell only, buy only, or buy and sell) instead of a plain on and off switch
- The mod's menu entries are now in the War Sails port menus

## 1.4.3

- Fixed observed price mode going on quoting prices it had moved itself
- Fixed the ledger panel being torn down and rebuilt every time you opened any other screen
- The cost basis now asks the same livestock question the rest of the mod asks
- Fixed Trade XP collapsing to almost nothing on goods you had not bought
- The best market floor now always guards goods with no cost basis, replacing a cost the mod used to invent for them

## 1.4.2

- Fixed two more village states the game refuses to trade in still being offered as destinations
- Fixed the travel fallback pairing a straight line distance with the pathfinder's land ratio
- A visit no longer shows the same market is still settling message twice

## 1.4.1

- Fixed quick buy pricing the whole shelf before noticing it had no money to spend
- Fixed a pass stopped by the safety guard blaming your trade policy instead of saying what actually happened
- Fixed the route planner offering a village's last unit, which the mod would never actually take
- Auto trade on entry no longer asks twice what counts as a market

## 1.4.0

- Fixed the ledger panel not scrolling
- The panel hotkey now accepts a modifier, such as Ctrl+T
- Added a running campaign profit total to the panel's top row, kept in your save
- Trade summaries are now coloured, green for profit, amber for none, blue for spending and grey for notices
- Trade summaries are no longer buried under the messages the game posts when you enter a town

## 1.3.35

- Fixed the panel's Refresh button not actually refreshing the routes, because it rebuilt them from the same cache
- Tidied one redundantly written name in the source

## 1.3.34

- Fixed the two travel distance readings disagreeing about whether your party can sail
- Fixed travel times going stale the moment your cargo changed your speed
- Buying a ship now takes effect at once, instead of leaving land only distances in place for up to an hour
- Fixed prices going stale after a trade you made yourself on the game's own trade screen
- Fixed the last price paid being rounded down where the average paid is rounded properly

## 1.3.33

- Fixed a fully sold stack keeping a denar or two of leftover cost, which nudged the next purchase's average price up
- Prices are now refreshed after the mod's own trading moves them, instead of the panel and tooltips quoting stale numbers for the rest of the hour
- Protect mounts, unique and crafted items is now called Protect unique and crafted items, because mounts are protected whatever that setting says
- A leftover number and two lines of code that could no longer run are gone

## 1.3.32

- The 1.3.31 notes said simulation mode had become exact, and only real trading had
- Simulation mode now says it is a best case, in the message, the log and the settings hint

## 1.3.31

- Fixed a sale closing below your minimum profit margin, and sometimes at an outright loss, because the price was only checked once for every ten units

## 1.3.30

- Fixed a damaged purchase record being able to stop a save from loading
- The record of what you paid is now rebuilt in one place instead of two

## 1.3.29

- The buy side margin rule now has one copy instead of four written two different ways
- The market eligibility filter now has one copy instead of one for each knowledge mode

## 1.3.28

- Fixed the straight line travel estimate being able to exceed the real one, which would have wrongly hidden markets you could reach

## 1.3.27

- The tooltip hook now takes only the two arguments it actually reads, so a game update has fewer ways to break it

## 1.3.26

- Fixed the new route search asking the game's pathfinder about every pair it considered, instead of sifting them with a cheap straight line estimate first
- The store page now lists the per visit spend cap alongside the other route caps

## 1.3.25

- Fixed the planner throwing away a whole good when its single cheapest buy town and dearest sell town were too far apart, instead of trying other pairs
- Fixed route quantities ignoring the per visit spend cap, which is usually the cap that runs out first
- Quick buy no longer works out your herd when there is no livestock on the shelf to buy

## 1.3.24

- Fixed a cow being kept back as food ahead of the grain sitting next to it

## 1.3.23

- Fixed the food reserve counting a cow as one meal, when the game counts it as its meat value
- Fixed the herd guard undercounting the herd in cavalry parties
- The mod now asks the game's own trade permission rules instead of working around them
- The temporary diagnostics are gone

## 1.3.22

- Fixed the mod trading at villages the game had closed after a raid
- Fixed the ledger panel taking the whole keyboard, so space pauses again and the speed keys work while it is open
- The map wide trade permission diagnostic is gone, because it crashed inside the game's own code when asked about any settlement

## 1.3.21

- The trade permission diagnostic now asks about every town and village at session start, instead of only the ones you walk into
- The focus diagnostic now also logs what had focus at the moment the panel hotkey was pressed

## 1.3.20

- Added a temporary diagnostic recording your party's food numbers once a day
- Added a temporary diagnostic recording what the game's own rules say about trading at each settlement you enter
- Added a temporary diagnostic recording which part of the screen has focus while the campaign map is up

## 1.3.19

- The cargo map marker now also updates when you leave a settlement, so it follows what you actually traded rather than what you walked in with
- The setting's hint now names all three moments the marker updates

## 1.3.18

- Fixed the per item denar cap never reaching the route panel, so a route could show 32 units of a good the cap stops at 5
- The price colouring hint now mentions livestock and horses, which 1.3.14 added
- The Minimum profit margin hint now says it applies to buying and to the route panel too, not just selling
- The Economy settling delay hint now says it stops the town menu buttons as well as the automation
- Two stale mod description lines about tooltips and per item caps are corrected

## 1.3.17

- Fixed the scan radius never reaching the cargo map marker
- Fixed the panel listing routes quick buy would refuse when conservative route projection was switched off
- The setting's hint now says which half of it changes what you see and which half changes what it does

## 1.3.16

- Fixed Hold cargo for the best market checking the price once and then letting the whole stack go, when selling into a market is exactly what pushes its price down
- Fixed the food reserve letting a good skip the rest of the sell rules
- The ledger popup now explains why Profit is not simply sell price minus buy price times quantity

## 1.3.15

- Fixed the food reserve keeping whichever food came first, so it held the wine and sold the grain, and it now keeps the cheapest food
- Livestock is now kept back last, behind every sack of grain
- Fixed a fault that could fill the log with hundreds of error reports a second
- Minimum stock for buy suggestions now says it only applies in live price mode

## 1.3.14

- Fixed simulation mode ignoring the merchant's gold, so a dry run happily reported selling more than a town could pay for
- Fixed the trade screen colouring trade goods only, leaving horses and livestock grey even though the tooltip could price them
- Fixed the panel never proposing a livestock route even though quick buy would buy livestock
- The ledger popup no longer tells you to press T for the panel, since that popup only appears when the panel cannot open
- A piece of the panel's closing code that nothing could ever reach is gone

## 1.3.13

- Fixed quick buy spending the per visit budget in shelf order, so a 12% margin listed first beat a 60% one further down
- Fixed the cost basis changing halfway through selling one stack, so your margin was silently two different margins in one sale
- Fixed pressing Done in the settings screen being able to switch your automation off
- The panel now counts caravan traffic by walking the party list once instead of once for every town shown
- Entering any settlement anywhere no longer costs work for parties that are not yours

## 1.3.12

- Fixed towns under siege and villages being raided being ranked as ordinary markets and proposed as destinations
- A village that has been raided and is rebuilding is deliberately still listed, because thin shelves and high food prices are an opportunity rather than a closed market
- Fixed quick buy buying goods the selling side had just been taught to refuse

## 1.3.11

- Fixed quick sell being able to sell a quest item, which silently failed the quest later
- Fixed the same for items the game marks as not merchandise, such as tournament prizes and banners
- Fixed the panel keeping hold of your mouse and keyboard when the screen changed while it was open

## 1.3.10

- Fixed a settings combination that switched your automation back off on the next load, with nothing said
- Fixed Hold cargo for the best market ignoring livestock
- Fixed the ledger popup naming the text in the hotkey setting rather than the key the panel actually listens for
- Fixed the hotkey setting accepting comma separated text like T,Y, which left the panel with no working hotkey at all

## 1.3.9

- Fixed observed mode rebuilding its market rankings on every tooltip and every inventory row, instead of once a game hour
- Entering a market now clears the stored rankings, so the panel and tooltips no longer serve answers from before you walked in
- Fixed the panel proposing routes for locked items that quick buy would refuse to buy
- The one line trade summary now names the six goods worth the most denars, not whichever came first in the list
- The startup log no longer claims MCM was registered when the companion file was from a different version
- Fixed an empty or cut off item list setting throwing an error and silently stopping a whole trade pass

## 1.3.8

- Fixed Keep food (days of supply) not covering livestock, so a herder could enter a town with auto sell on and leave with the whole herd sold and nothing to eat
- Fixed quick buy buying goods you had locked in the inventory
- Fixed Suppress vanilla trade-rumor lines still hiding the game's own hints when TradeLord had no prices to show in their place
- Quick buy now stops looking once the budget is spent, instead of pricing the rest of the shelf first

## 1.3.7

- The cargo marker's travel ceiling now starts at 1.5 days instead of 1, because 1 day rarely had anything to point at

## 1.3.6

- Fixed quick buy buying goods quick sell would never sell, such as smithing materials, which then sat in the cargo forever
- Fixed Suppress vanilla trade-rumor lines hiding the game's own prices even when TradeLord's tooltip section was switched off, leaving no prices at all
- Fixed simulation mode writing its per item buy caps back to the visit record
- The cargo marker now values only what quick sell would actually put on the counter, not locked items, never sell entries and reserved food
- Fixed the panel not giving your keyboard and mouse back when it closed, which could leave the map unresponsive after an encounter
- A trade pass now reports itself in one line instead of eight
- Protect smithing materials now ships off, since it governs buying as well as selling

## 1.3.5

- Fixed battle loot, companion transfers and stash moves being recorded as purchases at full market price, which then made your profit margin refuse perfectly good sales
- Fixed the per visit trade counters carrying over into the next campaign you loaded in the same session
- Entering a market now takes prices once a game hour instead of three times an entry
- The market scan now builds its list of markets in reach once a game hour and shares it, instead of rebuilding it for every good and direction
- Fixed a sale that paid no gold still handing the goods over and draining the cost record
- Fixed turning off Detailed trade summary also stopping the full item list reaching the log, which its own description promises
- The log now starts fresh each launch and writes the version banner first, instead of growing forever
- The panel no longer proposes routes for goods on your never sell list
- A single failed panel setup no longer disables the panel for the rest of the session
- Looking up the distance between two towns no longer builds a throwaway label each time
- Fixed the panel hotkey firing while the escape menu was open
- The cargo marker now refreshes on every settlement entry, not only when entry automation is switched on
- The travel distances are now cleared with the campaign, like everything else
- Quick-sell option in town menu now explains that it also hides the quick trade entry

## 1.3.4

- Fixed the two per item buying caps starting again on every quick buy click instead of lasting the whole visit, so clicking twice bought twice the cap
- Simulation mode now previews against whatever the caps have left and puts them back where it found them
- The village last unit rule now measures against the stock left on this pass rather than the shelf you walked in on

## 1.3.3

- Fixed the panel's legend line being far too long for its row, so half of it was invisible, including the note explaining the Profit column
- Fixed the panel's no routes message being cut off, so the advice a new player needs was hidden

## 1.3.2

- Fixed smithing protection silently switching off in the second campaign of a session
- Fixed the food reserve being kept for each kind of food instead of in total, which held roughly five times as much food as the setting said
- Fixed Exclude hostile markets doing nothing when turned off, because trading with hostile towns was blocked either way
- Fixed the scan radius being applied in live price mode and ignored in observed mode
- Fixed auto trade reading as on when quick buy was off, which left the town menu with nothing in it at all
- Fixed profit and Trade XP on livestock you had bought being reported at the full sale price instead of the margin
- Fixed a purchase that moved no gold still being written to the cost record, which permanently dragged that good's cost basis towards zero
- Fixed the panel only being able to unpin the most recent town it pinned, stranding every earlier pin on the map
- Fixed the daily cargo marker removing a marker you had pinned by hand
- Fixed simulation mode ignoring the rule that stops it selling a good and buying it straight back at the same counter
- Fixed simulation mode ignoring carry weight, so it reported buying more than the party could hold
- Observed mode now looks settlements up directly instead of scanning every settlement on the map for every tooltip row
- Looking up a price no longer builds a throwaway label each time
- Leaving a campaign no longer keeps the previous campaign's settlements, parties and prices in memory
- The map panel is rebuilt whenever the map screen is replaced, and logs itself once a session instead of once for every inventory screen you close
- The never sell, always sell and never buy lists now match item ids whatever the capitalisation
- The duplicated market settling check is one now, and an unused piece of the source is gone

## 1.3.1

- Automatic passes on entering a town are silent when nothing traded, instead of reporting bought 0 items for 0 denars
- Observed mode now respects the trade with villages setting exactly as live mode does
- The panel hotkey is read once instead of being read again every frame
- Dead code and every source comment are gone

## 1.3.0

- Grain is no longer bought out of the box, because it is heavy and low margin and clogged the cargo, and a general never buy list is added alongside it
- Livestock now trades both ways out of the box, with buying capped by the game's own herd maths so a purchase can never push the party into the herd speed penalty
- The settings now ship trader ready, with quick buy, auto sell, auto buy and the cargo marker all on and a gold reserve of 100
- The cargo map marker now ignores towns beyond a travel ceiling, 1 day out of the box
- Quick buy never takes a village's last unit of anything, so the game's own buy products option always has stock to show
- The map button now reads TradeLord

## 1.2.0

- Fixed the map button catching your mouse where you could not see it, which gave a permanent forbidden cursor and a stuttering camera drag on the world map
- Trade summaries now name the goods, for example Sold 8 Olives for 240 denars
- Added auto buy on entry, and auto sell, auto buy and auto trade now sit together in the settings
- Routes must now clear your minimum profit margin after the resale safety factor, so razor thin routes are no longer listed

## 1.1.0

- Added quick trade, one menu option that sells and then buys in a single pass
- Added livestock selling, off out of the box, and mounts and pack animals are never sold
- Added a TradeLord button on the right edge of the campaign map that opens the ledger panel
- Added a workshop tracker, a caravan traffic column and a data age column to the panel
- Observed mode now records the horses and livestock a shelf actually holds, so animal tooltips work without live prices
- Fixed three things from the 1.0 review, settings changes now clear the stored prices at once, the panel unpins only the markers it placed itself, and a number typed as the panel hotkey no longer lands on some other key

## 1.0.0

- First release, with a price ledger saved in your campaign, best buy and sell prices in item tooltips, a route panel on the campaign map, and quick sell and quick buy from the town and village menus

---

The versions below are the earlier test builds, from the two repositories this one replaced. They were all numbered 1.0 at the time, and the numbers here were given to them afterwards to put them in order.

## 0.916Alpha

- The download now unpacks straight into the game folder, the layout mod managers expect
- The install instructions in the mod description and the release notes now match it

## 0.915Alpha

- Clicking a town in the ledger panel now pins a map marker on it, and clicking again takes it off
- The automatic best sell town marker now ships off, because it hopped between towns with no visible reason
- The buy cap per item now starts at 32 instead of 50
- The spend per visit now starts at 1000 denars instead of no limit at all

## 0.914Alpha

- Added the first changelog, with an entry for every release so far

## 0.913Alpha

- Travel days now follow real routes instead of straight lines, so a town across a sea no longer reads as a short trip
- Travel times are now kept, so tooltips stay quick to hover

## 0.912Alpha

- Added travel ceilings, so a market more than three days away, or a village more than one, is no longer offered
- Both ceilings can be turned off or changed in the settings
- When two markets tie on price, the nearer one now ranks first
- An empty list now names the ceiling that emptied it
- Added the store description and a feature comparison page alongside the mod description
- The mod description is brought in line with those two pages

## 0.911Alpha

- The documentation's account of the research behind the mod is corrected
- The zero hardcoded claim is narrowed to price data
- The documentation now says plainly what was researched and what was written from scratch
- The rest of the documentation is brought back in line with what the mod actually does
- The mod description now calls the mod field proven, after the first live playthroughs
- Verdict lines now use a star the game's font can draw

## 0.910Alpha

- The compass arrows are gone from tooltips, because the game's font could not draw them, so rows now show travel days and price only
- Note: adding or removing any mod mid campaign, the MCM stack included, can hang the game when you press Continue on a save made under the old mod list, which is the base game's doing and happens with TradeLord disabled too
- Load that save from the Saved Games list instead, click through the module mismatch warning the game shows, and save again, and Continue is safe from that point on

## 0.909Alpha

- Fixed every colour in the panel being ignored

## 0.908Alpha

- Fixed the panel rendering upside down, with the title at the bottom and the worst routes first
- Column headers are now gold and underlined, so they never read as an entry
- The profit per day cell now carries a green to orange gradient across the visible list
- The panel's Best column is now called Profit per day

## 0.907Alpha

- Route proposals are now capped by the sell town's own gold, so a bulk run no longer promises profit the buyer cannot pay

## 0.906Alpha

- The diagnostic log lines are gone, now that a live game has answered every open question
- The mod now refuses to touch MCM unless the whole stack is installed, and names the missing piece
- Consult the TradeLord ledger in the town menu now opens the map panel

## 0.905Alpha

- Added the ledger as a panel on the campaign map, opened by hotkey, with sortable routes and clickable towns
- Item tooltips now list the top five buy and sell markets, with profit, stock and travel days
- The route popup now says its day counts are measured from where your party is
- The panel's artwork now ships inside the download

## 0.904Alpha

- The first successful sale and buy now write a confirmation line to the log
- The XP line now says its number is profit in denars, not raw XP

## 0.903Alpha

- Fixed the route report telling anyone whose gold was below the reserve that there were no profitable routes
- Tooltips now use the game's own coin icon instead of a bare d
- Tooltips now spell out stock, denars and days
- The startup log now reads the game version from the game itself, instead of trusting the number stamped on its files

## 0.902Alpha

- Fixed the mod refusing to load at all on any install without MCM
- The settings menu now lives in its own file, loaded only when MCM is there
- The launcher's version warning is gone
- Harmony is matched to the version the game's own Harmony module ships

## 0.901Alpha

- The mod now writes plain answers to its log for the questions only a live game can settle
- Added a guard that stops a trade if gold ever moves the wrong way

## 0.900Alpha

- First test build, with a price ledger saved in your campaign, best buy and sell prices in item tooltips, and quick sell and quick buy from the town and village menus
