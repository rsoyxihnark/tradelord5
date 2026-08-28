# TradeLord

A trading mod for Mount & Blade II: Bannerlord. It remembers what every market pays, shows you the
best place to buy and sell in the item tooltip, ranks profitable routes on a panel on the campaign
map, and can run the buying and selling for you from the town menu.

Single-player only.

---

## Read this before you install

**Out of the box, TradeLord trades for you.** Auto-sell and auto-buy are both ON by default, so the
first town you walk into, it will sell what your policies allow and spend up to 1000 denars on goods
it can resell. It tells you what it did on screen every time, and it never spends below your gold
reserve of 300 denars, but it does not ask first. The first market you enter in a campaign, it also
says on screen that it trades for you, and names the two settings that stop it.

That is deliberate: the defaults are set up for someone who installed a trading mod to have trading
happen. If you would rather drive it by hand, turn off **Auto-sell on entry** and **Auto-buy on
entry** in the Automation settings, and use the **Quick-sell**, **Quick-buy** and **Quick-trade**
entries in the town menu instead.

If you have no settings menu (see [Requirements](#requirements)), the defaults are what you get and
you cannot change them in game.

---

## What it does

**Insight — knowing where the money is**

- Item tooltips gain a TradeLord block: the five best markets to sell in, the five cheapest to buy
  in, with travel time from where you are standing, and stock counts where the mod can see them.
- Inventory rows are tinted by how this market's price compares with the best one known, so a good
  deal and a bad one are obvious at a glance.
- A ledger panel on the campaign map (hotkey **T**, or the TradeLord button on the right edge) ranks
  profitable routes: what to buy, where, how many, where to sell it, what the trip is worth and how
  many days it takes. Click a town name to jump the camera there and pin a marker on it.
- The panel also tracks your workshops and their recent profit.

**Action — doing something about it**

- **Quick-sell** sells everything your policies allow, one unit at a time, stopping the moment the
  price stops clearing your margin.
- **Quick-buy** buys goods that are worth more somewhere else in reach, inside your spending caps.
- **Quick-trade** does both, selling first so the proceeds fund the buying.
- **Consult the TradeLord ledger** opens the route panel from inside a settlement.

**Automation**

- Either pass can run automatically when you enter a market.
- Profit from automated trades feeds your Trade skill. See [Balance](#balance).

## Requirements

| | |
|---|---|
| Game | Built and tested against Bannerlord **1.4.7**. See [Game versions](#game-versions). |
| Harmony | **Required.** The mod will not load without `Bannerlord.Harmony`. |
| MCM | **Optional**, but needed for the settings screen. This build talks to the **MCM 5** line; a newer line runs the mod on its defaults and says so in the log. |
| War Sails | Optional. If installed, routes and travel estimates account for sea legs, and the menu entries appear in port menus too. |

The settings screen needs **all three** of MCM, ButterLib and UIExtenderEx enabled. With a partial
stack TradeLord runs on its built-in defaults and says so in the log — it does not crash, and
trading works either way. You only lose the ability to change anything.

## Install

1. Install Harmony, and MCM if you want the settings screen.
2. Download the release zip and extract it so you end up with
   `Mount & Blade II Bannerlord/Modules/TradeLord/`.
3. In the launcher, enable **TradeLord**, and make sure Harmony loads before it.

Load order does not otherwise matter. TradeLord loads its settings independently of the order MCM
comes up in.

## Your first five minutes

1. Walk into any town. TradeLord sells what it can and reports it on screen.
2. Open the inventory and hover a trade good — the TradeLord block is at the bottom of the tooltip.
3. Back on the map, press **T**. That is the route panel. If it is empty, you have not seen enough
   markets yet, or every route is outside your travel ceilings.
4. Open the settings and set **Max spend per visit** and **Gold reserve** to numbers you are
   comfortable with before you go shopping.

## Settings

Six groups. Every setting has a hint on hover explaining exactly what it does; this is the map.

- **Knowledge** — where prices come from and how far you are willing to look. `Live world prices` is
  ON by default, which means TradeLord reads prices from markets you have never visited. Turn it OFF
  for observed mode, where only prices you have seen in person count and they go stale after 45 days.
- **Insight** — tooltips, inventory colouring, the map button and the panel hotkey.
- **Action** — what quick-sell may touch: category policies for food, smithing materials and
  livestock, the food reserve, unique and crafted item protection, inventory locks, the never-sell
  and always-sell lists, and the profit margin every trade must clear.
- **Automation** — auto-sell on entry, auto-buy on entry, and the combined auto-trade switch.
- **Buying** — the gold reserve, per-item and per-visit spending caps, the resale safety factor, and
  the never-buy list.
- **General** — villages, simulation mode, the Trade XP multiplier, the map marker, the coin sound.

Settings live in
`Documents/Mount and Blade II Bannerlord/Configs/ModSettings/Global/TradeLord/`.

### The defaults that matter

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
| Panel hotkey | T |

### Never-sell, always-sell and never-buy lists

These take **item ids**, comma separated — the short internal name of a good, like `grain`, `wine`
or `iron_ore`, not the display name. The easiest way to find one is to trade once and read the ids
out of `TradeLord.log`, which names every good it moves.

The never-sell list wins over the always-sell list. An inventory lock wins over both.

## Balance

TradeLord makes money faster than trading by hand, because it does not miss a good deal and it does
not get bored. If that is not what you want, these are the dials:

- **Live world prices OFF** is the big one. In observed mode TradeLord only knows what you have
  personally seen, and forgets it after 45 days. This is the closest thing to an honest-merchant mode.
- **Trade XP multiplier 0%** stops automated profit feeding your Trade skill at all. At the default
  100%, an automated pass earns Trade XP on the profit it made. Goods you never bought — loot, spoils,
  the contents of a raided village — are valued at the cheapest market you know of rather than at
  nothing, so clearing out a pile of loot does not train Trade the way a real buy-low-sell-high run
  does. Selling one below that value earns no profit and no XP.
- **Minimum profit margin** up, **Max spend per visit** down, and **Travel ceiling** down all make it
  pickier and slower.
- **Economy settling delay** keeps TradeLord out of a fresh campaign for a set number of days, since
  prices in a new game have not settled yet.

Nothing here changes the game's economy. TradeLord buys and sells through the game's own trade
actions at the game's own prices — it moves your goods, it does not invent gold.

## Saves and uninstalling

TradeLord stores its price ledger, your purchase records and your lifetime profit **inside your save
file**, written as plain text. A save carries nothing that only TradeLord knows how to read, so:

- **You can remove TradeLord from a campaign and go on playing it.** The save still opens without the
  mod installed. You lose the ledger, the purchase records and the lifetime profit figure. You do not
  lose the campaign.
- **A campaign last saved by 1.6.18 or earlier will not open.** Those saves keep their records in a
  form only TradeLord could read, and TradeLord no longer reads it. Start a fresh campaign.
- Adding TradeLord to an existing campaign is fine. It starts with an empty ledger and fills it in as
  you travel.
- Updating TradeLord over an existing install is fine from 1.6.19 onwards.

Nothing TradeLord saves affects vanilla data. It does not touch your items, your party, your heroes
or the world economy's own records.

## Compatibility

- **Other trade or price mods.** TradeLord reads prices through the game's own price model, so it
  sees whatever another mod has done to prices. Two mods both auto-trading on settlement entry will
  fight over the same goods; run one of them.
- **Vanilla rumour lines.** TradeLord hides the vanilla merchandise rumour block in tooltips so you
  do not get two sets of price hints. Turn off *Suppress vanilla trade-rumor lines* to get it back.
- **On-screen notifications.** While TradeLord is putting a trade through, it holds back the
  individual per-item lines the game raises, so a 40-unit sale does not bury your screen. Other mods
  get first say over their own notifications, and the hold lasts one game transaction. If anything
  ever goes wrong mid-trade, the filter is forced open on the next frame and the log says so.
- **Party speed model.** Livestock *buying* uses the game's own herding calculation so a purchase can
  never push your party into the herd speed penalty. If another mod replaces the party speed model,
  livestock buying switches itself off and writes a line in the log. Selling is unaffected.
- **UI mods.** The ledger panel is its own layer on the map screen. If it fails to build, it disables
  itself and the town-menu popup takes over; the rest of the mod keeps working.

## Performance

The route scan is the only expensive thing TradeLord does, and it runs at most once per in-game hour,
or when you press **Refresh** on the panel. Travel distances are cached per hour and dropped when you
leave the campaign. Tooltips and inventory colouring read from the same cache.

If the panel takes a moment the first time you open it in a session, that is the scan. It will not
repeat within the hour.

## Game versions

The released build is compiled against Bannerlord **1.4.7.117484**, and that is the version it has
been played on.

| Game version | Does the mod fit it | Played on it |
|---|---|---|
| 1.4.7.117484 | yes | yes |
| 1.4.8.119303 | yes | not yet |
| 1.5.1.120547-beta | yes | not yet |

*Fits* means all 103 game types and 201 members the compiled mod binds to still resolve, all four
Harmony patch targets still exist with the same shape and no new overload to make the lookup
ambiguous, both private members the mod reaches for by name are intact, no value of any enum it
reads has moved, the game's assembly identities have not changed, and both projects compile clean
against that version with warnings as errors. The only differences found above 1.4.7 are additive:
1.5.1 adds one unrelated menu action and drops one assembly the mod never touches.

That check reads signatures, not behaviour, so it proves the mod still fits a version, not that it
still behaves the same there. If you are on 1.4.8 or the 1.5.1 beta, the mod should load and run as
it stands, and the log will tell you if a patch failed to attach.

## Troubleshooting

Everything TradeLord does goes into:

```
Documents/Mount and Blade II Bannerlord/TradeLord.log
```

It is rewritten from scratch every time you launch the game, so it is always about the session you
just played. Start at the top.

| Log line | What it means |
|---|---|
| `TradeLord v… loaded \| game …` | The mod loaded. If this line is missing, it did not. |
| `ERROR in patching …` | One feature failed to attach. It names which. The rest of the mod continues. |
| `MCM not detected - running on built-in defaults` | No settings screen. Trading still works. |
| `MCM is installed, but this build of TradeLord was made against MCMv5 …` | A newer line of MCM is installed than this build talks to, so the settings screen cannot be registered. It names what it found. Trading still works; update TradeLord. |
| `MCM is installed but its ButterLib/UIExtenderEx stack is not fully loaded` | Enable all three, or remove the MCM stack entirely. |
| `herd guard: …` | Livestock buying is off because the party speed model was not where it should be. Selling is unaffected. |
| `transaction direction changed on this game version` | TradeLord watched the gold move the wrong way and stopped immediately rather than continue. Report this one. |
| `quick-sell moved nothing at … :` | Followed by the reasons, counted. This is the answer to "why did it not sell my stuff". |
| `ledger panel setup …` | The map panel could not build. The town-menu popup still works. |

**"It sold something I wanted to keep."** Lock it in the inventory screen, or put its item id on the
never-sell list. Unique and player-crafted items, quest items, mounts and pack animals are already
protected.

**"It did not sell anything."** The log line above names the reason. Common ones: the price did not
clear your profit margin, you are holding cargo for a better market, or the merchant is out of gold.

**"The panel is empty."** No route is inside your travel ceilings. Raise **Travel ceiling** in
Knowledge settings, or visit more markets.

**"The map button gets in the way."** Turn off *TradeLord button on the map screen* in Insight
settings. The hotkey still opens the panel.

## Reporting a bug

Open an issue at <https://github.com/rsoyxihnark/tradelord5/issues> with:

1. Your game version and TradeLord version.
2. `TradeLord.log` from the session it happened in — attach the whole file.
3. Your other mods, and their load order.
4. What you did, what you expected, what happened instead.

The log is the useful part. Without it there is usually nothing to go on.

## Building from source

```
dotnet build src/TradeLord.csproj -c Release
dotnet build mcm/TradeLord.MCM.csproj -c Release
```

Game assemblies come from the `Bannerlord.ReferenceAssemblies` NuGet package, so no game install is
needed to build and no game DLLs are in this repository. Both projects build with warnings as errors.

`python3 tools/regression_sweep.py` runs the source checks the build workflow runs.

To ask whether the mod still fits a game version, build both projects in Release and then:

```
dotnet run --project tools/compat -- 1.4.8.119303 1.5.1.120547-beta
```

It fetches the reference assemblies for each version named, compares them against the version in
`src/TradeLord.csproj`, and exits non-zero if anything the mod binds to has moved. This is what the
table under [Game versions](#game-versions) is built from.

## License

MIT. See [LICENSE](LICENSE). Fork it, patch it, ship a compatibility fix — you do not need to ask.
