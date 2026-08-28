# TradeLord

**Bannerlord never tells you where the money is. TradeLord does — and if you like, it goes and gets
it for you.**

It reads what every market in Calradia is paying right now — live, out of the game's own economy,
including the towns you have never set foot in — puts the best place to buy and sell right in the
item tooltip, ranks the trade routes actually worth driving on a panel on the campaign map, and can
run the buying and selling for you the moment you walk into a town.

Nothing to scout, nothing to build up. It knows the whole map from your first minute.

**[Download the latest release](https://github.com/rsoyxihnark/tradelord5/releases/latest)**  ·  Single-player  ·  Bannerlord 1.4.8

---

## Everything it does

**What it tells you**

- ✅ Live prices from every market on the map, read straight from the game's own economy
- ✅ The five best places to sell and the five cheapest to buy, in every item tooltip
- ✅ Travel time to each of them, from wherever you happen to be standing
- ✅ Stock counts, so you know the deal is actually there
- ✅ Inventory rows tinted good to bad against the best price known
- ✅ A ledger panel on the campaign map ranking every profitable route — hotkey **L**, or the map button
- ✅ Every route priced unit by unit through the game's own model, so the profit is what you will really get
- ✅ A confidence score on each route: profit per day, discounted by how likely it is to survive the trip
- ✅ Click any town to jump the camera there and pin a marker on it
- ✅ The best town for the cargo you are carrying, marked on the map for you
- ✅ Your workshops and their recent profit, on the same panel

**What it does for you**

- ✅ Quick-sell, quick-buy and quick-trade in the town menu, whenever you want them
- ✅ Auto-sell and auto-buy the moment you enter a market, if you leave that switched on
- ✅ Sells one unit at a time and stops the moment the price stops clearing your margin
- ✅ Buys only what it can resell at a profit somewhere within reach
- ✅ Credits the profit to your Trade skill
- ✅ Trades in villages too, under their own stricter travel limit
- ✅ Counts sea legs and appears in port menus if you have War Sails
- ✅ Tells you on screen exactly what it moved, and why it moved nothing
- ✅ A dry-run mode that reports every trade it would have made and moves nothing

**What it will never touch**

- ✅ Anything you locked in the inventory screen
- ✅ Unique and player-crafted gear, quest items, mounts and pack animals
- ✅ Your food reserve, and grain
- ✅ Your gold reserve, or anything past your spending cap for the visit
- ✅ Any item id you put on the never-sell or never-buy list
- ✅ The game's economy — it trades at the game's own prices, through the game's own actions

**And the rest**

- ✅ A settings screen with every switch explained on hover, translatable, through MCM
- ✅ One build runs on Bannerlord 1.4.8, 1.4.7 and the 1.5.1 beta
- ✅ Add it or remove it mid-campaign — your save keeps working either way
- ✅ Everything it did goes to `Documents/Mount and Blade II Bannerlord/TradeLord.log`

---

## What it looks like

Hover any trade good, anywhere, and the bottom of the tooltip tells you where it is worth money and
what the trip costs you:

```
Best sell prices
  Danustica        ~2 days   187    Profit: +34%
  Amprela          ~3 days   174    Profit: +25%
* Sargot                     139
Best buy prices
  Zeonica          ~1 day     96    Stock: 41
  Marunath         ~2 days   103    Stock: 26
```

The starred line is the market you are standing in.

Press **L** on the campaign map for the ledger panel — every profitable run TradeLord can see, best
first, with the quantity to buy and the days it takes. It is full before you have entered a single
town:

```
Item        Buy From     Price    Sell At      Qty    Profit    Days
Wine        Zeonica         96    Danustica     24    +2,184     3.1
Iron        Marunath       143    Sargot        18    +1,097     2.4
```

---

## Install

1. Install **Harmony** — the `Bannerlord.Harmony` module, on Nexus Mods. It is the only thing
   TradeLord cannot run without.
2. [Download the release zip](https://github.com/rsoyxihnark/tradelord5/releases/latest) and extract
   it so you end up with `Mount & Blade II Bannerlord/Modules/TradeLord/`.
3. In the launcher, tick **TradeLord**, and make sure Harmony is above it.

That is it. Load order does not otherwise matter, and you can add TradeLord to a campaign you are
already halfway through.

Want the settings screen? Install **MCM** (Mod Configuration Menu) as well, with ButterLib and
UIExtenderEx enabled alongside it. Without them TradeLord runs on the defaults below and says so in
its log — it does not crash, and trading works either way.

## Your first ten minutes

1. **Load your campaign and ride into any town.** TradeLord sells what it can and reports it on
   screen. The first market of a new campaign is left alone on purpose, so you get told what it does
   before it does it.
2. **Open your inventory and hover a trade good.** The TradeLord block is at the bottom of the
   tooltip. That is the whole "where do I sell this" problem, solved.
3. **Back on the map, press L.** That is the ledger panel, and it is already populated — TradeLord
   has read the whole map. Empty means nothing profitable is inside your travel ceilings: raise
   **Travel ceiling** in the Knowledge settings, or ride somewhere with more towns in reach.
4. **Click the most profitable route's buy town.** The camera jumps there and pins a marker. Go.
5. **When you are comfortable, open the settings** and set **Gold reserve** and **Max spend per
   visit** to numbers that suit how you play.

---

## TradeLord trades for you out of the box

Auto-sell and auto-buy are both ON by default, so from your second market onwards TradeLord sells
what your policies allow and spends up to 1000 denars on goods it can resell. It never touches your
last 300 denars, it never sells a locked or unique item, and it tells you what it did every single
time.

That is deliberate — a trading mod should trade. If you would rather drive it by hand, turn off
**Auto-sell on entry** and **Auto-buy on entry** in the Automation settings and use the town-menu
entries instead. TradeLord says so on screen the first time you enter a market in a campaign, and
names those two switches.

### What it does out of the box

| Setting | Default |
|---|---|
| Live world prices | ON |
| Auto-sell on entry / Auto-buy on entry | ON / ON |
| Gold reserve | 300 |
| Max spend per visit | 1000 |
| Buy cap per item | 32 |
| Minimum profit margin | 15% |
| Resale safety factor | 85% |
| Keep food | 5 days |
| Never buy grain | ON |
| Travel ceiling / village ceiling | 3 days / 1 day |
| Trade with villages | ON |
| Sell loot up to tier | OFF |
| Panel hotkey | L |

**Live world prices** is the one worth knowing about. ON, TradeLord reads prices, stock and each
merchant's purse straight from the live world economy, for every market on the map, whether you have
been there or not — which is why the panel works before you have entered a single town. Turn it OFF
for an honest-merchant game: then it records only what you see as you walk a market, only those
prices count, and each one is forgotten after 45 days.

## Add it, remove it, update it

**TradeLord is plug and play.** Put it in a campaign, take it out of a campaign, at any point, and
the campaign still opens. Take it out and you lose your purchase records and the lifetime profit
figure; you keep everything else. Put it back in and it is up to speed at once — on live prices
there is nothing to rebuild. Updates go straight over the top of an existing install.
