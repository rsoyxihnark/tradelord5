- It reads what every market in Calradia is paying right now, live, straight from the game's own brain.
- It shows you the best ways to buy and sell, right in the item tooltip and in a ledger.
- It ranks the trade routes worth travelling on panel. Each route's profit is evaluated on 9+ factors and travel time on 5+. Then the two are scored together for confidence. Respecting how the price moves because of the NPC caravans enroute there. Profit per travel day is calculated by real pathfinding with party's own speed, with the cargo carrying, from where you are, to the buy town, on to the sell town.
- And it can buy and sell for you automatically as you enter a settlement.
- You can change how it does any of it. The defaults are thoroughly tested and work great.

## Everything it does

- ✅ Plug and play, unlike some other mods. Install or remove it, before, during or after a campaign, and load a save made with it or without it. It does not matter. This mod cannot break a save. It used to register a `SaveableTypeDefiner` declaring classes of its own, `PriceObservation` and `PurchaseRecord`, that only this mod knew how to read, so a save written that way would not open again until you put the mod back. That is gone. All it ever puts in a save now is two strings, a number, a settlement reference and a flag, every one of them something vanilla already knows how to read.

**What it tells you**

- ✅ Live prices from every market on the map, read directly from the game's own economy brain
- ✅ The five best places to sell and the five cheapest to buy, in every item tooltip
- ✅ Travel time to each of them, from wherever you are
- ✅ That time worked out by the game's own pathfinding, not a straight line drawn across the map, and at your party's real speed with the cargo you are carrying
- ✅ Buys and sells livestock, checking the game's own herding penalty against the men in your party first, so cattle never slow you down
- ✅ Stock counts, so you know the deal is actually there
- ✅ Inventory rows tinted good to bad against the best price known
- ✅ One clean set of price hints, bypassing the vanilla rumour system, which is usually out of date by the time you get there
- ✅ A ledger panel on the campaign map ranking every profitable route, on hotkey **T** or the map button
- ✅ Every route priced unit by unit through the game's own model, so the profit is the one you will really get
- ✅ A confidence score on each route: profit per day, discounted by how much of the good the market you are buying from has in stock, how long the trip is, how old the prices are, and how many NPC caravans are sitting at or heading for those two towns
- ✅ Profit quoted with a safety margin, in case prices drift before you arrive
- ✅ The buying market's gold counted in, so it never plans a sale nobody can pay for
- ✅ Click any town to jump the camera there and pin a marker on it
- ✅ The best town for the cargo you are carrying, marked on the map for you
- ✅ Your workshops and their recent profit, on the same panel

**What it does for you**

- ✅ One trade entry in the town menu, selling then buying in one go, whenever you want it
- ✅ Sells and buys the moment you enter a market, if you leave that switched on
- ✅ Sells one unit at a time and stops the moment the price stops clearing your margin, all of it instant, on one click or automatically as you enter a settlement
- ✅ Buys only what it can resell at a profit somewhere within reach
- ✅ Holds cargo back for the market that pays best rather than dumping it one town short, off by default
- ✅ Clears looted gear too, up to a tier you choose
- ✅ Credits the profit to your Trade skill, at a rate you set
- ✅ Trades in villages as well, under their own stricter travel limit
- ✅ Counts sea legs and appears in port menus if you have the War Sails DLC
- ✅ Tells you on screen exactly what it moved, and names the reason when it moves nothing
- ✅ Keeps the game's per-item message spam out of a forty-unit sale
- ✅ A coin sound on a trade that lands, silence on one that does not
- ✅ A quiet mode that keeps automated trading to the log and off your screen
- ✅ A dry run that simulates every trade it would have made, and shows you the estimate (moves nothing)

**What it doesn't touch**

- ✅ Anything you locked in the inventory screen
- ✅ Unique and player-crafted gear, quest items, mounts and pack animals
- ✅ Your food reserve (accounted for the men in your party)
- ✅ Buying grain (its a low profit high weight good to trade)
- ✅ Your gold reserve, or anything past your spending cap for the visit
- ✅ Smithing materials such as iron ore, ingots, charcoal and hardwood, once you switch their policy to leave them alone, off by default
- ✅ Any good you put on the never-sell or never-buy list, named by its item id or by the name on screen
- ✅ More livestock than your party can drive, so a purchase never slows you down
- ✅ Markets belonging to a faction you are at war with
- ✅ The game's economy: it trades at the game's own prices, through the game's own actions

**Dials, when you want them**

- ✅ Travel ceilings, so nothing it suggests is further than you care to ride
- ✅ A scan radius, if you would rather it thought locally
- ✅ A minimum stock before it calls something worth buying
- ✅ Separate rules for food, smithing materials and livestock
- ✅ Caps on one good by count or by denars, on how many of it you will carry, and on the whole visit
- ✅ Never-sell, always-sell and never-buy lists, taking item ids or item names, in any capitalisation
- ✅ Profit measured against what you paid: the average, the last, or the cheapest you know of
- ✅ An honest-merchant mode you can switch on, using only the prices you have seen in person, recorded market by market as you walk them
- ✅ A settling delay that keeps it out of a brand new campaign until prices calm down
- ✅ A rebindable panel hotkey, and a map button you can hide

**And**

- ✅ A settings screen with every switch explained on hover, translatable, through MCM
- ✅ Built on Bannerlord 1.4.8.119303, for Bannerlord 1.4.8.119303
- ✅ Everything it did goes to `TradeLord.log`, which you can read yourself or send to me if something happens so I can debug it

**All of it is yours to change.** Every feature above is a switch or a number on the settings screen.
Turn any one of them off, or set it to whatever you like.
