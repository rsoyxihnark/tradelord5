# TradeLord

**Bannerlord never tells you where the money is. TradeLord does, and if you like, it goes and gets
it for you.**

It reads what every market in Calradia is paying right now, live, out of the game's own economy,
including the towns you have never set foot in. It puts the best place to buy and sell right in the
item tooltip, ranks the trade routes actually worth driving on a panel on the campaign map, and can
run the buying and selling for you the moment you walk into a town.

Nothing to scout, nothing to build up. It knows the whole map from your first minute.

**[Download the latest release](https://github.com/rsoyxihnark/tradelord5/releases/latest)**  ·  Single-player  ·  Bannerlord 1.4.8

---

## Everything it does

- ✅ Plug and play, unlike similar mods. Install it or remove it before, during or after a campaign, and load a save made with it or without it. It cannot break a save, because it never writes a thing of its own making into one: the whole ledger goes in as ordinary text and numbers the game already knows how to read, so once TradeLord is gone there is nothing left in the save for the game to go looking for.

**What it tells you**

- ✅ Live prices from every market on the map, read directly from the game's own economy brain
- ✅ The five best places to sell and the five cheapest to buy, in every item tooltip
- ✅ Travel time to each of them, from wherever you are
- ✅ That time worked out by the game's own pathfinding, not a straight line drawn across the map, and at your party's real speed with the cargo you are carrying
- ✅ Buys and sells livestock, checking the game's own herding penalty against the men in your party first, so cattle never slow you down
- ✅ Stock counts, so you know the deal is actually there
- ✅ Inventory rows tinted good to bad against the best price known
- ✅ One clean set of price hints, bypassing the vanilla rumour system, which is usually out of date by the time you get there
- ✅ A ledger panel on the campaign map ranking every profitable route, on hotkey **L** or the map button
- ✅ Every route priced unit by unit through the game's own model, so the profit is the one you will really get
- ✅ A confidence score on each route: profit per day, discounted by how deep the seller's stock is, how long the trip is, how old the prices are, and how many NPC caravans are already working those two towns
- ✅ Profit quoted with a safety margin, in case prices drift before you arrive
- ✅ The buyer's own purse counted in, so it never plans a sale nobody can pay for
- ✅ Click any town to jump the camera there and pin a marker on it
- ✅ The best town for the cargo you are carrying, marked on the map for you
- ✅ Your workshops and their recent profit, on the same panel

**What it does for you**

- ✅ Quick-sell, quick-buy and quick-trade in the town menu, whenever you want them
- ✅ Auto-sell and auto-buy the moment you enter a market, if you leave that switched on
- ✅ Sells one unit at a time and stops the moment the price stops clearing your margin, all of it instant, on one click or on its own as you walk in
- ✅ Buys only what it can resell at a profit somewhere within reach
- ✅ Holds cargo back rather than dumping it one town short of the one that pays best
- ✅ Clears looted gear too, up to a tier you choose
- ✅ Credits the profit to your Trade skill, at a rate you set
- ✅ Trades in villages as well, under their own stricter travel limit
- ✅ Counts sea legs and appears in port menus if you have the War Sails DLC
- ✅ Tells you on screen exactly what it moved, and names the reason when it moves nothing
- ✅ Keeps the game's per-item message spam out of a forty-unit sale
- ✅ A coin sound on a trade that lands, silence on one that does not
- ✅ A quiet mode that keeps automated trading to the log and off your screen
- ✅ A dry run that works through every trade it would have made, moves nothing, and shows you the estimate

**What it will never touch**

- ✅ Anything you locked in the inventory screen
- ✅ Unique and player-crafted gear, quest items, mounts and pack animals
- ✅ Your food reserve, and grain
- ✅ Your gold reserve, or anything past your spending cap for the visit
- ✅ Any good you put on the never-sell or never-buy list, named by its item id or by the name on screen
- ✅ More livestock than your party can drive, so a purchase never slows you down
- ✅ Markets belonging to a faction you are at war with
- ✅ The game's economy: it trades at the game's own prices, through the game's own actions

**Dials, when you want them**

- ✅ Travel ceilings, so nothing it suggests is further than you care to ride
- ✅ A scan radius, if you would rather it thought locally
- ✅ A minimum stock before it calls something worth buying
- ✅ Separate rules for food, smithing materials and livestock
- ✅ Caps on one good by count or by denars, and on the whole visit
- ✅ Never-sell, always-sell and never-buy lists, taking item ids or item names, in any capitalisation
- ✅ Profit measured against what you paid: the average, the last, or the cheapest you know of
- ✅ An honest-merchant mode you can switch on, using only the prices you have seen in person, recorded market by market as you walk them
- ✅ A settling delay that keeps it out of a brand new campaign until prices calm down
- ✅ A rebindable panel hotkey, and a map button you can hide

**And**

- ✅ A settings screen with every switch explained on hover, translatable, through MCM
- ✅ One build runs on Bannerlord 1.4.8, 1.4.7 and the 1.5.1 beta
- ✅ Everything it did goes to `Documents/Mount and Blade II Bannerlord/TradeLord.log`

**All of it is yours to change.** Every feature above is a switch or a number on the settings screen.
Turn any one of them off, or set it to whatever you like.

---

## Getting started

**Install.** You need **Harmony**, the `Bannerlord.Harmony` module, and nothing else. Extract the
release zip so you have `Mount & Blade II Bannerlord/Modules/TradeLord/`, tick TradeLord in the
launcher with Harmony above it, and load any campaign, new or halfway through. Add **MCM** as well
if you want the settings screen; without it TradeLord runs on its defaults and says so in its log.

**It trades for you out of the box.** From your second market on, it sells what your rules allow and
spends up to 1000 denars a visit, never touching your last 300 denars, and it tells you what it did
every time. It says as much on screen the first time, and names the two switches that turn it off if
you would rather trade by hand.

**Then just play.** Hover a good to see where it is worth money, press **L** for the routes, and go.
