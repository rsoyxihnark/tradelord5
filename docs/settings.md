# Settings

Every setting has a hint on hover that explains exactly what it does. This page is the map: what
each group is for, and which switches are worth reaching for first.

The settings screen needs **MCM**, and MCM wants **ButterLib** and **UIExtenderEx** enabled
alongside it. Without all three, TradeLord runs on its built-in defaults and says so in the log.
Trading works either way — you only lose the ability to change anything.

Your settings live in
`Documents/Mount and Blade II Bannerlord/Configs/ModSettings/Global/TradeLord/`.

## The five that matter most

If you change nothing else, change these.

| Want | Setting |
|---|---|
| It to stop trading without being asked | **Auto-sell on entry** and **Auto-buy on entry** OFF |
| It to spend less of your gold | **Max spend per visit** down, **Gold reserve** up |
| It to be pickier about deals | **Minimum profit margin** up |
| It to stay near you | **Travel ceiling** down |
| An honest-merchant game | **Live world prices** OFF |

Defaults for these and the rest are in the [README](../README.md#what-it-does-out-of-the-box).

## Knowledge — where prices come from

Where TradeLord's prices come from, and how far it is willing to look.

- **Live world prices** — ON, TradeLord reads prices, stock and merchant gold live from the world
  economy, for every market on the map, including ones you have never set foot in; nothing has to be
  scouted first. OFF, it records only what you see as you walk a market, and only those prices count.
  This is the single biggest switch in the mod.
- **Observation shelf life** — with live prices OFF, how long a price you saw stays trustworthy
  before it is forgotten. 0 means never forget.
- **Exclude hostile markets** — never scan, suggest or trade with settlements at war with you.
- **Scan radius** — limit price scanning to markets within a straight-line distance. 0 is the whole
  map.
- **Minimum stock for buy suggestions** — do not suggest buying somewhere that only has a handful.
  Live prices only: observed mode records prices, not stock levels.
- **Travel ceiling** and **Village travel ceiling** — how many days of travel a route may cost.
  Markets beyond the ceiling vanish from tooltips, and no suggested trip may exceed it. Villages get
  their own, stricter ceiling.
- **Conservative route projection** — price the sell side as though the market has drifted a little
  by the time you arrive, so the panel promises less than the best case.
- **Bulk price simulation** — price a whole lot unit by unit, so quantity and profit account for
  your own buying pushing the price up. OFF reads higher than the trip will actually pay.
- **Rank routes by confidence** — order the panel by profit per day discounted by how likely that
  profit is to survive the trip, rather than by raw profit per day.

## Insight — what you see

- **Show best buy/sell in tooltips** — the TradeLord block at the bottom of an item tooltip.
- **Suppress vanilla trade-rumor lines** — hides the game's own merchandise rumours so you get one
  set of price hints rather than two.
- **Color prices by world market** — tints inventory rows by how this market's price compares with
  the best one known.
- **Ledger panel hotkey** — the key that opens the panel on the campaign map. A single key such as
  `L`, `Y` or `F5`, optionally with `Ctrl`, `Alt` or `Shift` in front. TradeLord does not take keys
  away from the game, so a bare key the game also uses will do both things; add a modifier to avoid
  that.
- **TradeLord button on the map screen** — the clickable button on the right edge of the map. Turn
  it off if it gets in the way of your map clicks; the hotkey still works.

## Action — what quick-sell may touch

- **Quick-sell option in town menu** — shows the quick-sell entry, and with it quick-trade.
- **Minimum profit margin** — the margin every sale has to clear. Raise it to make TradeLord pickier
  and slower.
- **Keep food (days of supply)** — days of food to hold back for your party before any of it is sold.
- **Food / Smithing material / Livestock policy** — per category: ignore it, sell only, buy only, or
  buy and sell.
- **Protect unique and crafted items** — keeps unique and player-crafted gear out of every sale.
  Quest items, mounts and pack animals are always protected.
- **Respect inventory locks** — a lock in the inventory screen wins over every other rule.
- **Cost basis mode** — what "what you paid" means when profit is worked out: the average you paid,
  the last price you paid, or the cheapest market you know of.
- **Sell loot up to tier** — how far up the tiers looted gear may be sold. OFF by default.
- **Hold cargo for the best market** and **Best-market tolerance** — hold goods back rather than sell
  them just short of the town that pays best.
- **Never sell** and **Always sell** lists — see [Item id lists](#item-id-lists) below.

## Automation — whether it trades for you

- **Auto-sell on entry** — sells what your policies allow when you enter a market. Trade XP is
  awarded.
- **Auto-buy on entry** — buys goods worth more elsewhere in reach, inside your spending caps.
- **Auto-trade** — both of the above together, selling first so the proceeds fund the buying. With
  this on, the separate quick-sell and quick-buy menu entries are hidden and quick-trade stays for
  manual re-runs.

The first market of a new campaign is always left alone, so you get told what TradeLord does before
it does it.

## Buying — how much of your gold it may spend

- **Enable quick-buy** — the buying side, on or off entirely.
- **Gold reserve** — denars TradeLord will never spend down past.
- **Buy cap per item (count)** and **(denars)** — the most of one good it will buy on a visit, by
  count or by value. 0 turns either cap off.
- **Max spend per visit** — the ceiling on a single market visit. 0 is unlimited.
- **Resale safety factor** — assume you will only get this much of the price you are counting on
  when you resell. Lower is more cautious.
- **Never buy grain** — grain is your party's food, so it is left alone by default.
- **Never buy** list — see below.

## General

- **Trade with villages** — villages join the price scans and get the quick-sell and quick-buy
  entries in their menus. They keep their own, stricter travel ceiling.
- **Simulation mode (dry run)** — TradeLord reports what it would have done and moves nothing. The
  safest way to watch it work before you trust it with your purse. Read the result as a best case:
  nothing moves, so the market never reacts, and a real pass usually trades a little less.
- **Economy settling delay** — keep TradeLord out of a brand new campaign for a number of days,
  since prices have not settled yet.
- **Trade XP multiplier** — how much of the profit from automated trades feeds your Trade skill.
  0% stops it entirely. See [Balance](#balance).
- **Auto-mark best sell town on map** and **Auto-marker travel ceiling** — pin the best place to
  sell your cargo on the campaign map.
- **Coin sound on trade** — the coin noise on a completed trade.
- **Detailed trade summary** — name the goods in the on-screen trade summary rather than counting
  them. The full list goes to the log either way.

## Item id lists

The never-sell, always-sell and never-buy lists take **item ids**, comma separated — the short
internal name of a good, like `grain`, `wine` or `iron_ore`, not the display name.

The easiest way to find one: trade once, then read the ids out of `TradeLord.log`, which names every
good it moves.

Never-sell beats always-sell. An inventory lock beats both.

## Balance

TradeLord makes money faster than trading by hand, because it does not miss a good deal and it does
not get bored. If that is more than you want:

- **Live world prices OFF** is the big one. TradeLord then only knows what you have personally seen,
  and forgets each price once it goes stale. This is the closest thing to an honest-merchant mode.
- **Trade XP multiplier 0%** stops automated profit feeding your Trade skill. At the default 100%, an
  automated pass earns Trade XP on the profit it made. Goods you never bought — loot, spoils, the
  contents of a raided village — are valued at the cheapest market you know of rather than at
  nothing, so clearing out a pile of loot does not train Trade the way a real buy-low-sell-high run
  does.
- **Minimum profit margin** up, **Max spend per visit** down and **Travel ceiling** down all make it
  pickier and slower.
- **Economy settling delay** keeps it out of a fresh campaign until prices have settled.

None of this changes the game's economy. TradeLord buys and sells through the game's own trade
actions at the game's own prices.
