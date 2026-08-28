# Compatibility

## Saves

**TradeLord is plug and play.** Add it to a campaign, remove it from a campaign, at any point, and
the campaign keeps working.

- **Remove it whenever you like.** The save opens without the mod installed. You lose your purchase
  records and the lifetime profit figure, and in observed mode the prices you had recorded. You keep
  the campaign.
- **Add it whenever you like.** On live prices it is working the moment the campaign loads: it reads
  the world as it finds it, so there is nothing to build up. In observed mode it starts blank and
  records prices as you walk markets.
- **Update it whenever you like.** Straight over the top of an existing install.

Nothing TradeLord stores affects vanilla data. It does not touch your items, your party, your heroes
or the world economy's own records.

## Other mods

- **Other trade or price mods.** TradeLord reads prices through the game's own price model, so it
  sees whatever another mod has done to prices. Two mods both auto-trading on settlement entry will
  fight over the same goods — run one of them.
- **On-screen notifications.** While a trade is going through, TradeLord holds back the game's
  per-item lines so a 40-unit sale does not bury your screen. Other mods get first say over their own
  notifications, and the hold lasts one transaction. If anything goes wrong mid-trade the hold is
  released on the next frame and the log says so.
- **Party speed model.** Livestock buying uses the game's own herding calculation, so a purchase can
  never push your party into the herd speed penalty. If another mod replaces that model, livestock
  buying switches itself off and writes a line in the log. Selling is unaffected.
- **UI mods.** The ledger panel is its own layer on the map screen. If it fails to build it disables
  itself, the town-menu entry takes over, and the rest of the mod carries on.
- **Vanilla rumour lines.** TradeLord hides the vanilla merchandise rumour block in tooltips so you
  do not get two sets of price hints. Turn off *Suppress vanilla trade-rumor lines* to get it back.

## Game versions

The released build is compiled against Bannerlord **1.4.8.119303**, and that is the version it is
played and tested on.

| Game version | Does the mod fit it | Played on it |
|---|---|---|
| 1.4.8.119303 | yes | yes |
| 1.4.7.117484 | yes | not needed |
| 1.5.1.120547-beta | yes | not yet |

One build covers all three. The game's assemblies are unsigned and permanently versioned 1.0.0.0, so
a 1.4.8 build still runs on 1.4.7, and the only difference above 1.4.8 is additive — 1.5.1 adds one
unrelated menu action and drops one assembly the mod never touches.

*Fits* is a machine answer, not an opinion: every game type and member the compiled mod binds to
resolves, every patch target still exists with the same shape and no new overload to make it
ambiguous, no enum value it reads has moved, the assembly identities match, and both projects
compile clean against that version with warnings as errors. The tool that checks it is in
[docs/building.md](building.md).

That check reads signatures, not behaviour. Behaviour is covered by hand: the full pass — menus,
quick-sell, quick-buy with livestock, both tooltip patches, inventory colouring, the panel, the
settings screen, and a save carried across versions — was walked on 1.4.8 and every step passed. On
the 1.5.1 beta the mod should load and run as it stands, and the log will tell you if a patch failed
to attach.

## Performance

The route scan is the only expensive thing TradeLord does, and it runs at most once per in-game
hour, or when you press **Refresh** on the panel. Travel distances are cached per hour and dropped
when you leave the campaign. Tooltips and inventory colouring read from the same cache.

If the panel takes a moment the first time you open it in a session, that is the scan. It will not
repeat within the hour.
