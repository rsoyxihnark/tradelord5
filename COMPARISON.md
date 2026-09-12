# What TradeLord does differently: all 9 trade mods examined

All nine were decompiled and read, not taken from their pages: what each patches, how it prices, how
it moves a good, what it writes into your save.

**Run only one mod that trades.** Two auto traders fight over the same cargo, purse and Trade XP.
BestTradePrice patches the tooltip and row colour TradeLord uses, so run one or the other.

**135 AutoTrader, Eskalior.** The one everyone has and the closest rival: it stages the vanilla
trade screen and you press Done. Its decision is an average of prices inside a radius, so nothing it
does knows how long the ride is. 34 settings in its own XML file, no MCM, no tooltip, no panel.
TradeLord trades instantly, one unit at a time, with travel days in every decision.

**1490 Latest Trade Rumors, StormLightningSL.** Does not trade: it fills the vanilla rumour list so
vanilla tooltips know more, by replacing `Campaign.UpdateTradeRumors` and reading two private fields
by name. Built against game e1.5.7 and the MCMv3 API, last updated February 2021, and what it gives
is still vanilla rumour data with vanilla's staleness. TradeLord reads every market live, or only
what you have seen in person.

**3206 AutoTrader Fixed, hyper1on.** A 2021 repair of 135 for game 1.6.1: the older feature set,
bundled Harmony, two bugs its page says it could not fix. 135 is maintained again and well ahead of
it.

**8474 BestTradePrice, nabil_dz.** The prettiest tooltip here, five buys and five sells with a
compass arrow. Two things the page understates: distance is a straight line across the map, and the
Market Stability Fix patches the game's price model, on by default at 70 percent, softening prices
for every caravan and AI party in Calradia, not just yours. Towns only, and it never trades.
TradeLord shows travel days through the game's own pathfinder, includes villages, and leaves the
price model alone.

**10369 Trade Advisor, quezzas.** The strongest engineering here and the nearest thing to
TradeLord's ledger panel: unit by unit pricing, caravan snapshots, workshop prediction, sea legs.
Towns only, never trades, keeps no price history, and its wage reserve is hardcoded. TradeLord does
all of it, plus villages, plus the age of your own notes discounting each route, and it trades.

**11607 Trade Optimizer, FriendlyTurtle.** A capable engine with 27 settings and a dry run, built on
someone else's framework: four modules must be installed and load ordered before it runs, and quest
goods are covered only by vanilla's unique and not-for-trade flags. TradeLord is one module, one
dependency.

**11648 ArmouredRay Quick Trade, ArmouredRay.** One Quick Trade entry, 24 settings, a good pack
animal ratio. It does not use the game's trade actions: it moves goods and gold by hand, which is
why its page admits a 2 percent price error and that it sells the item you locked. Every TradeLord
unit goes through the game's own sell action, so the price, the tax and the XP are vanilla's.

**11708 Auto Trader, DarkStyleee.** The leanest honest auto trader: no Harmony patches, a cost
ledger so it never sells below what you paid, a map marker. It trades in chunks of ten and re-prices
only between chunks, so a large sale can walk past your margin; its best sell town scan has no
distance limit at all; and it only touches trade goods, so loot, livestock, pack animals and horses
are outside it.

**11988 Trade Tracker, aslheyairam.** A notebook, and a good one: last seen price, last price paid,
best known buy and sell, in the vanilla tooltip. It only knows what you looked at, has no settings
screen, and is the only one here writing save types of its own into your save. TradeLord's notebook
is the same thing with Live world prices off, and all it saves is three strings, two numbers, a
settlement and a flag, all types vanilla already reads.

## What none of the nine do

Put travel time inside every decision, through the game's own pathfinder, at your real speed with
the cargo you carry. Check the margin on every single unit and stop the moment it fails. Hold back
what a quest is waiting on, read from eleven vanilla quest types by name. Read the game's own
herding model, so a purchase never slows you down and no more animals are sold than it takes to get
your speed back. Trade with caravans and villagers met on the road. Keep your own prices per market,
with their age discounting a route. Keep a settings file and a settings screen as twins, with a
migration ladder so a setting is never quietly reset. Four languages switched with no restart. A
price trace naming any other mod changing your prices.

## Where they are ahead

135 lets you see and edit the deal first. 8474's damping is something some players want, and
TradeLord will not do it, because it would change the economy for everyone else. 10369 predicts
caravan shopping and workshop output. 11988 records what the screen said rather than modelling it.
11607 fits an automation family you may already run.
