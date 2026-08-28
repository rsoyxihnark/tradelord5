# Troubleshooting

Almost every question about TradeLord is answered in one place:

```
Documents/Mount and Blade II Bannerlord/TradeLord.log
```

It is rewritten from scratch every time you launch the game, so it is always about the session you
just played. Start at the top.

## The common ones

**"It did not sell anything."**
The log names the reason for every market that traded nothing. The usual ones: the price did not
clear your profit margin, you are holding that cargo for a better market, your spending caps are
spent, or the merchant has run out of gold.

**"It sold something I wanted to keep."**
Lock it in the inventory screen, or put its item id on the never-sell list. Unique and
player-crafted items, quest items, mounts and pack animals are protected already. See
[item id lists](settings.md#item-id-lists).

**"It is spending too much of my gold."**
Raise **Gold reserve**, lower **Max spend per visit**, or turn **Auto-buy on entry** off and buy
through the town menu instead.

**"The ledger panel is empty."**
No route is inside your travel ceilings. On live prices this is never about how much of the map you
have seen — TradeLord reads all of it — so raise **Travel ceiling** in the Knowledge settings, or
move somewhere with more towns in reach. With live prices off, it can also mean you have not walked
enough markets yet to have prices to compare.

**"The map button gets in the way of my clicks."**
Turn off **TradeLord button on the map screen** in the Insight settings. The hotkey still opens the
panel.

**"The hotkey does two things at once."**
TradeLord does not take keys away from the game, so a bare key the game also uses triggers both.
Give the panel a key with a modifier, like `Ctrl+L`.

**"I have no settings screen."**
MCM needs ButterLib and UIExtenderEx enabled alongside it. Enable all three, or remove the MCM stack
entirely. TradeLord runs on its built-in defaults either way and trading is unaffected.

**"I want to watch what it would do without it touching anything."**
Turn on **Simulation mode (dry run)** in the General settings. TradeLord reports every trade it
would have made and moves nothing.

## Reading the log

| Log line | What it means |
|---|---|
| `TradeLord v… loaded \| game …` | The mod loaded. If this line is missing, it did not. |
| `ERROR in patching …` | One feature failed to attach, and it names which. The rest of the mod carries on. |
| `MCM not detected - running on built-in defaults` | No settings screen. Trading still works. |
| `MCM is installed, but this build of TradeLord was made against MCMv5 …` | A newer line of MCM than this build talks to, so the settings screen cannot be registered. It names what it found. Trading still works; update TradeLord. |
| `MCM is installed but its ButterLib/UIExtenderEx stack is not fully loaded` | Enable all three, or remove the MCM stack entirely. |
| `herd guard: …` | Livestock buying is off because the party speed model was not where it should be. Selling is unaffected. |
| `transaction direction changed on this game version` | TradeLord watched the gold move the wrong way and stopped rather than continue. Please report this one. |
| `quick-sell moved nothing at … :` | Followed by the reasons, counted. This is the answer to "why did it not sell my stuff". |
| `ledger panel setup …` | The map panel could not build. The town-menu entry still opens the ledger. |

## Reporting a bug

Open an issue at <https://github.com/rsoyxihnark/tradelord5/issues> with:

1. Your game version and your TradeLord version.
2. `TradeLord.log` from the session it happened in — attach the whole file.
3. Your other mods, and their load order.
4. What you did, what you expected, and what happened instead.

The log is the useful part. Without it there is usually nothing to go on.
