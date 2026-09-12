# Changelog

## 1.64.0

- The panel now scores its own promise: every route it shows writes down the Sell price it promised you, and walking into that market near the time it said holds that promise against what the market really pays.
- The line under the routes says how much of the promised Sell price has actually been there and over how many arrivals, and that tally carries on across your campaign.
- With Score the forecast in the log switched on, the log breaks the same thing down by the Conf each route carried, so you can see whether a higher Conf really means a promise that holds.
- A promise you arrive far too late for is dropped rather than scored, so the figure only counts arrivals it can say something about.
- The feature list says so too.

## 1.63.0

- Score the forecast in the log, a new switch under Debug, writes down what TradeLord expects a market to hold by the time you get there, and then, as you walk in, what it really holds, good by good, with how far off it was.
- It needs Count what is on its way to a market, and it stays off until you turn it on, since it makes the log longer.
- The feature list says so too.

## 1.62.2

- In Russian and Simplified Chinese, the note under Keep gold for days of wages now calls Gold reserve by the name the settings screen gives it.
- In Turkish, Simplified Chinese and Russian, the note under Auto-mark best sell market on map now names the travel ceilings and the ledger panel the way the rest of the mod names them.
- In Russian, the note under Count what is on its way to a market now calls Live world prices by the name the settings screen gives it.

## 1.62.1

- Where a route's amount counts goods still on the road, the ledger panel now marks it Qty!, so an amount larger than what the market holds while you read it is never a surprise.
- The line under the routes says what that mark means, beside the other marks it explains.
- The feature list says so too.

## 1.62.0

- The five markets in an item tooltip are now priced as each will be when you get there, counting what the caravans and the workshops will do to it first, so a tooltip and the ledger panel never quote the same market differently.
- Those five are ordered on the prices you will actually be offered, so a market that is about to be picked over no longer sits at the top of the list.
- The explanation under Count what is on its way to a market now says it reaches a tooltip price as well as a route.
- The feature list now says the tooltip prices a market as it will be when you arrive.
- The changelog was missing 1.49.0 and 1.49.1 entirely; both versions are back in it, word for word as they went out.
- The feature list no longer says the ledger panel ranks every profitable route: it ranks the best route it can find for each good, thirty rows of them.
- The feature list now calls the smithing choice Keep the ones you have not learned, the name the settings screen gives it.
- The feature list now says that quiet mode still speaks the notice on your first market and the warning that your cargo is full.
- The feature list now says a route is listed even when your herd is already as large as your men can drive, that a route whose prices could not be walked unit by unit is marked on its confidence figure, and that a dry run is marked a best case.
- The opening summary no longer counts the factors behind a route and now names what is actually weighed.
- The feature list now says that TradeLord tells you once, at the first market of a campaign, that it will trade for you as you arrive, and leaves that market untraded so you can switch it off first.
- The feature list now says that you can tell a caravan it was a good trade once the goods have changed hands, and that the trader answers.
- The feature list now says that the town menu entry falls back to the six best routes written out as text when the ledger panel cannot open.
- The feature list now counts how much of the margin survives unit-by-unit pricing among the things a route's confidence is discounted by, which it had left out.
- The feature list now says the settling delay ships switched off, and says that a trade on the road moves the goods and the gold itself at the price the game quotes off-market, since there is no market there to sell to.

## 1.61.0

- A route's price now also counts the gold the caravans on the road are bringing to spend at its two markets, so a market that is about to be picked over prices accordingly instead of looking cheap.
- That purse is spread over the goods that are cheap at the market it is heading for, which is what a trader buys, so the estimate lands on the goods a caravan would actually take off the shelf.
- A purse counts only against the market the caravan is going to, only when it arrives before you would, and never for more gold than the caravan is carrying.
- Count goods on their way to a market is now called Count what is on its way to a market, since it counts the gold coming to spend as well as the goods, and its explanation on the settings screen says so.
- The panel legend now says that prices count what will be bought off a market as well as what will be added to it.
- The feature list now says that a route's price counts the purses the caravans are bringing.

## 1.60.0

- A route's price now counts what is still on its way to its two markets: the cargo the caravans on the road will unload, and what that town's workshops will make next, so the panel prices the market you will walk into rather than the one standing there now.
- Goods that would land after you arrive are left out, each end of a route counted against its own travel time.
- The stock shown behind a buy town's confidence now includes the cargo heading there, so a deal that only exists once a caravan unloads is no longer hidden.
- The workshop list on the ledger panel now says what each workshop will make next, beside what it has earned.
- New setting, Count goods on their way to a market, on out of the box, under Knowledge. It follows Live world prices, so turning those off turns this off with them.
- The panel legend now says when prices and stock are counting what is on the way.
- The feature list now says that prices and stock count the caravans on the road and the workshops, and that the workshop list says what each will make next.

## 1.59.0

- The map marker can now land on a village as well as a town, so a village that pays best for what you are carrying gets the pin, as long as Trade with villages is on.
- A village being raided or rebuilding is never marked, since you could not trade there anyway.
- Auto-mark best sell town on map is now called Auto-mark best sell market on map, and it keeps to the Village travel ceiling when the market it is weighing is a village.
- The feature list now says the map marker can land on a village as well as a town.

## 1.58.2

- When TradeLord will not buy because your gold is held back, the message and the ledger panel now split the figure between Gold reserve and Keep gold for days of wages and name both, instead of calling the whole amount your gold reserve.
- Where only Gold reserve is holding your money back, that message now names Gold reserve by its own name, so you know which setting to lower.
- That message no longer appears when it was Max spend per visit that stopped the buying, since your gold reserve was not what held you back.

## 1.58.1

- TradeLord now tells you on screen that the market is still settling as you walk into one, unless Silence trade messages is on.

## 1.58.0

- Livestock is no longer counted as food. A herd is not held back towards your days of supply, it is never bought to restock them, and it is bought and sold as ordinary goods with only the herd speed penalty holding it back.
- Keep some of every kind of food now says livestock is left out because TradeLord trades a herd as goods and never as food.
- The feature list now says a herd is never counted as food.

## 1.57.0

- An army waiting on livestock now holds back that many head of whatever herd you are carrying, so the last quest that could still lose its supplies to a shopping trip is covered.

## 1.56.0

- Two more quests are read for what they are waiting on, the army that needs supplies and the headman who needs grain, so the grain and the wine they are owed is held back from every pass that sells.

## 1.55.0

- TradeLord now holds back any good an active quest of yours is waiting on, a trade good or a raw material as well as an animal, so a shopping trip can no longer sell the supplies a quest needs.
- Four more quests are read for what they are waiting on: the two artisan deliveries, the gang leader's stolen goods and the landlord who asks you to sell his produce.
- Nothing bought here now says it was your Buy cap per item that stopped a good, instead of naming your purse.
- The warning that your purse is under your gold reserve is no longer swallowed by a visit that sold something.
- The town marked on your map is never one TradeLord would walk into and leave alone as the same arrival.
- When TradeLord cannot thin your herd, TradeLord.log now names the animals it is holding back rather than falling silent.
- Always sell now says a good a quest is waiting on still holds, rather than only an animal.
- The feature list now says anything a quest is waiting on is held back from every pass that sells.

## 1.54.0

- Keep gold for days of wages now ships at 0, so out of the box TradeLord holds back only your gold reserve and a large army no longer stops it buying.
- Nothing bought here now names your purse only when nothing else held a good back, so the reason you are shown is the one you can act on.
- With Simulation mode (dry run) on, what a dry run spends at a market goes back into that market's gold, so a sale later in the visit is measured against the till the market would really have.
- The feature list now says TradeLord holds back only the days of your troops' wages you ask it to keep.

## 1.53.0

- When TradeLord buys nothing because your purse is under what it holds back, TradeLord.log now says what your purse is, how much is held back, and how much of that is your gold reserve against days of your wage bill.
- With Silence trade messages on, meeting a caravan or a party of villagers on the road during the Economy settling delay no longer puts a line on your screen.

## 1.52.4

- TradeLord now reads a language file as it stands, so one that points somewhere else can no longer leave the game waiting while TradeLord fetches it.

## 1.52.3

- With Simulation mode (dry run) on, food the dry run has already sold no longer counts towards your food reserve for the rest of the visit, so it now sells the same animals to get you back up to speed that a real pass would.

## 1.52.2

- With Simulation mode (dry run) on, the profit reported for a good that sits in more than one stack in your bags no longer counts what you paid against more units than you actually bought.

## 1.52.1

- Restock and keep food (days of supply) set to 0 now keeps no food back at all, including the few of each kind Keep some of every kind of food was still holding.
- The hint under Keep some of every kind of food now says it needs Restock and keep food (days of supply) above turned on to do anything.
- The feature list now says keeping some of every kind of food follows the days of supply you set.

## 1.52.0

- Buy cap per item, in count and in denars, and Stop buying at this many held now hold while TradeLord restocks your food and buys haul animals, not only while it buys for profit.
- Share of the hold one good may fill now holds while TradeLord restocks your food, and is left off haul animals, since one of those adds to the hold rather than filling it.
- The lines TradeLord adds when you meet a caravan or a band of bandits now come out in the language you picked, even when you change it after loading your game.
- The feature list now says which passes the per-item caps hold in, and that a conversation line takes the new language too.

## 1.51.1

- Turning Trade with towns off now takes the map marker off the best sell town as well, instead of leaving it pointing at a town TradeLord will not trade in.

## 1.51.0

- A new switch, Trade with towns, turns trading in town menus off the same way Trade with villages does, and it ships on.
- The settings screen has a new Trade Pool group, holding Trade with towns, Trade with villages and Trade with caravans and villagers, with the town and village travel ceilings and Exclude hostile markets under them.
- The longest hints on the settings screen are shorter, so a hint no longer spills over the settings beneath it.
- The feature list now says trading in towns and trading in villages can be switched off one at a time.

## 1.50.2

- Trade with caravans and villagers you meet is now called Trade with caravans and villagers, a shorter name that makes it clear this setting, rather than Trade with villages, is the one that covers a party of villagers you meet on the road.

## 1.50.1

- One-time settings override for this version only; due to so many improvements made into whole Trade Engine and couple defaults tweaked better

## 1.50.0

- Getting your party back up to speed no longer sells the animals Restock and keep food (days of supply) is holding back for your men.
- Where your herd is also your food, that can now leave the herd speed penalty in place rather than eat into the reserve, and TradeLord.log says the penalty is still there.
- An animal on your always-sell list is still sold to get you back up to speed, because the food reserve never holds one of those back.
- What TradeLord reckons a good cost you no longer drifts by a denar as it sells a stack down, so a sale no longer stops early with goods left that were worth selling.
- When a quest is waiting on more of an animal than your food reserve holds back, TradeLord now keeps the larger of the two rather than the smaller.

## 1.49.1

- The one-time reset of every setting is switched off from this version on, so nothing of yours is put back to what TradeLord ships with again.
- That reset ran on 1.49.0 alone: if you came to this version straight from an older one you were never reset, and your settings stand exactly as you left them.

## 1.49.0

- Every setting goes back to the value TradeLord ships with, once, the first time you run this version, because the settings it ships with now trade better out of the box than they used to.
- Anything you had set yourself is written into TradeLord.log as it happens, named setting by setting, so you can put back the ones you want.
- Your four item lists are emptied by that reset along with everything else, so a never-sell or always-buy list you had built up is in the log too.
- This happens on this version only and never again on a later one.

## 1.48.0

- Keeping food back and restocking it are one setting now, Restock and keep food (days of supply), which ships at 3 days: TradeLord holds that much back before it sells any food and tops you back up to the same amount as it trades.
- If you had set Restock food (days of supply) to something of your own, TradeLord now goes by your Keep food number for both and says so in TradeLord.log.
- Town travel ceiling now ships at 2.4 days instead of 3.
- Write a price trace to the log now sits in a Debug group of its own at the foot of the settings screen.
- The price trace is now written as you walk into a market before anything is traded, so the prices in it are the ones TradeLord went on rather than what was left after it traded.
- With the price trace on, every good a pass moves is now written down with the price TradeLord quoted next to what the market actually paid.
- When TradeLord cannot read the game's herd penalty, TradeLord.log now names everything that stops, which is buying livestock, buying haul animals and selling an animal to get you back up to speed, where it used to say only that livestock buying had stopped.
- The herd check in TradeLord.log now says when it could not read the herd penalty at all, instead of reporting that there is none.
- A panel hotkey written with a leading plus, such as +T, no longer reports an empty modifier in TradeLord.log.
- Buying none of a good can no longer leave TradeLord holding a cost against goods you have none of.
- The feature list now says the price trace is written before anything is traded and records what TradeLord quoted against what the market paid.

## 1.47.5

- The hint under How many of each kind of food to keep now points you at Keep food (days of supply) by the name the settings screen shows, in every language, instead of a name no setting has.

## 1.47.4

- Keep food (days of supply) and Keep some of every kind of food now count every stack of a good together, so when your bags hold the same food in two stacks, as they do when a quest hands you some alongside your own, TradeLord no longer sells off part of the reserve it was told to keep.

## 1.47.3

- The running total of what TradeLord has made you, along the top of the ledger panel, no longer turns negative once it passes about 2.1 billion denars, and a campaign saved before this still reads its total back.
- The feature list now says a save carries two of TradeLord's numbers rather than one.

## 1.47.2

- Simulation mode (dry run) now counts every stack of a good you carry and every stack a market has, so a dry run over looted gear or horses, which your inventory keeps in a separate stack for each quality, no longer stops short of what TradeLord would really sell and buy.

## 1.47.1

- Every price TradeLord quotes you is now read from a market the same way the trade screen reads it, naming the merchant you are trading with, which brings the tooltip, the ledger panel and the routes into line with the price you are actually offered.
- Village prices moved the most, because a village's own shelves were being left out of what TradeLord read there.

## 1.47.0

- A new setting, Write a price trace to the log, off out of the box: turn it on and TradeLord writes to TradeLord.log what the market you are standing in pays and charges for every good you are carrying, so a price it shows you that the trade screen does not offer can be tracked down.
- That trace names the market, the price model the game is running and any other mod changing either of them, so a mod moving prices behind TradeLord's back shows up by name.
- The feature list now says TradeLord can write a price trace to its log.
- The feature list now says that trading on arrival runs once for each arrival and that waiting in a town or village does not set it trading again, and that leaving a market and walking straight back in counts as the same visit.
- The feature list now says that trade goods and livestock you never paid for wait for a price that clears your margin over what they are worth, and it names the What a good counts as having cost you setting the way the settings screen names it.
- The feature list now says that the town marked on your map keeps up with you as you ride, and that TradeLord.log is kept from one session to the next and is only ever emptied at startup once it has grown past 999 KB.

## 1.46.2

- Trade goods and livestock you never bought, like the ones a town event hands you, are no longer sold off at the first market that will take them, and instead wait for a price that clears your Minimum profit margin over what the cheapest market you know would have charged for them.
- Nothing sold here and Nothing bought here no longer answer with haul animals and mounts are not traded as livestock, since TradeLord buys and sells those under rules of their own, and they name a reason about your own cargo instead.
- Wait here for some time no longer starts TradeLord trading all over again when the menu comes back: it trades once when you arrive and leaves that market alone until your party has taken to the road, and Trade here now (TradeLord) still trades whenever you ask.

## 1.46.1

- TradeLord now reads the SettingsVersion line in your TradeLord.ini to decide which of its older settings to carry forward, so a file already up to date is left exactly as you wrote it.
- A value you type into an up to date TradeLord.ini by hand is now held to what that setting takes today, so a wrong one is named in TradeLord.log instead of being quietly turned into something else.

## 1.46.0

- TradeLord.log is now emptied as the game starts if it has grown past 999 KB, so a long campaign can no longer leave one file growing without end in your Bannerlord folder.
- It is only ever emptied as the game starts and never as the game closes, so the log you send on after a session still holds everything that session wrote.
- TradeLord.log says when it has been emptied and how large it had grown, so a short log is never a mystery.
- A single line TradeLord could not write to TradeLord.log no longer slows every line after it for the rest of the session, because TradeLord picks the file back up half a minute later instead of leaving it until you restart the game.

## 1.45.0

- Town travel ceiling and Village travel ceiling are now the only two things deciding how far TradeLord looks, and everything obeys them: the tooltips, the routes, the town marked on your map, and how far Hold cargo for the best market will wait.
- Hold cargo for the best market now waits only for a town within your Town travel ceiling and a village within your Village travel ceiling, three days and one day out of the box, instead of for the best price anywhere in Calradia.
- The scan radius setting is gone, since those two ceilings now cover what it did, and TradeLord.log names the value it dropped from your TradeLord.ini.
- Your travel ceilings keep everything you had set: TradeLord.ini carries them forward under their new names by itself.
- The feature list drops the scan radius and names the two travel ceilings instead.

## 1.44.0

- The town marked on your map now keeps to your Town travel ceiling and Scan radius like everything else, so it can no longer send you to a town TradeLord will not sell in.
- The town marked on your map now follows you as you ride, instead of waiting until you enter or leave a settlement.
- The Travel ceiling setting is now called Town travel ceiling, since that is what it governs.
- The marker's own travel ceiling setting is gone, and TradeLord.log names the value it dropped from your TradeLord.ini.
- A market that sold nothing now says why even when TradeLord bought something there, so Hold cargo for the best market can no longer hold your cargo in silence.
- TradeLord no longer says a market had nothing worth trading when it had nothing to weigh up in the first place.

## 1.43.0

- Leaving a market and walking straight back in counts as the same visit, so TradeLord no longer buys back the goods it has just sold you there.
- Your Max spend per visit now lasts the whole of that visit, instead of starting again each time you step back inside.
- TradeLord.log is kept from one session to the next, instead of being emptied every time the game starts.
- TradeLord.ini is no longer rewritten when you start the game and none of your settings have changed.
- TradeLord.log now names the goods it stops holding against a resale when they leave your party unsold, rather than only counting them.

## 1.42.4

- Buying at a market yourself no longer leaves TradeLord thinking you took thousands more of a good than you did, so it keeps what you really paid and stops selling that cargo under your Minimum profit margin.
- That miscount also made the game work out the price of thousands of units you never bought, which is gone.

## 1.42.3

- Meeting a caravan or villagers on the road and trading nothing with them now tells you why, which it was meant to do from 1.42.0 and did not.
- The feature list no longer says a route is quoted against what your purse holds, because since 1.42.0 the ledger lists a route whether or not you could pay for it today.

## 1.42.2

- Selling at a market now works out the best markets for everything in your bags in one go, instead of once for each good, the same way buying already did.

## 1.42.1

- Walking into a market now works out the best markets for everything on the shelf in one go, instead of once for each good, so a busy market costs your game less.
- Restocking the larder and buying a haul animal do the same.
- The ledger now gives back what it worked the routes out with as soon as it has finished, instead of holding on to it until the next time you open the panel.

## 1.42.0

- Meeting a caravan or villagers on the road and trading nothing with them now tells you why, the same as walking into a market already did.
- The TradeLord ledger now lists a route even when your purse is empty or your herd is full, so it always tells you where the profit is rather than only what you could do this second.
- When your purse is empty the ledger says so under the routes, instead of showing that message in place of them.

## 1.41.9

- Working out which markets pay best for a good is quicker, because TradeLord now keeps the best few as it goes instead of putting every town in order first.
- TradeLord no longer asks a town what it pays for a good when that town is already beyond your Travel ceiling.

## 1.41.8

- If TradeLord cannot read its language file for a moment, it now keeps trying and speaks your language as soon as it can, instead of falling back to English until you restart the game.

## 1.41.7

- The language you pick, and every other setting you choose from a list, now take hold as you pick them instead of waiting for you to restart the game.
- TradeLord.log now says when the settings screen has taken charge, instead of only saying it was still waiting for it.
- With Live world prices off, writing down what a market charges no longer takes longer the more towns you have visited.

## 1.41.6

- Walking into a market no longer makes TradeLord work out the best markets for every good all over again, since walking in moves no prices.
- After TradeLord trades in a market, it works the best markets out again only for the goods its own trading moved the price of, instead of for every good on the map.

## 1.41.5

- The feature list now calls the setting that reads prices from the whole map by the name the settings screen gives it, Live world prices, instead of calling it honest-merchant mode.
- The feature list now says a pin comes off a town by itself once TradeLord has traded there.
- The feature list now says Share of the hold one good may fill ships at 45%.

## 1.41.4

- The price lines and the profit colouring in your inventory no longer make a small extra piece of work for every row on the screen.
- Working out which markets pay best for a good no longer makes a small extra piece of work every time TradeLord works it out.

## 1.41.3

- Buying at a market no longer asks twice of every good on the shelf whether it is one you allow TradeLord to buy.
- Buying no longer works out what your party can still carry when your purse or one of your spending caps has already stopped the purchase.
- The campaign map no longer checks every panel on screen for a text box on every frame, only when you press the TradeLord hotkey.

## 1.41.2

- Walking into a market, each new day on the road, and buying livestock no longer work out how many animals your party can drive one animal at a time.
- Selling animals to relieve the herd no longer works out how many haul animals your cargo can spare one animal at a time.
- The daily check on the road now works out how many animals must go once instead of twice.
- Whether your animals are slowing you down is now decided on a real difference in your speed, not on the smallest one the game can report.
- TradeLord no longer reopens TradeLord.log for every line it writes, so a busy market visit costs the game less.
- Auto-mark best sell town on map no longer prices your cargo in a town whose gold could never beat the best town found so far.

## 1.41.1

- When grain is what TradeLord left alone, the line that says why nothing was bought now says grain fills the cargo for little return, instead of naming a setting.

## 1.41.0

- Share of the hold one good may fill now ships at 45% instead of off, so one cheap good can no longer take your whole cargo.
- A town you pinned on the map loses its pin once TradeLord has traded there, instead of the pins piling up until you clear each one by hand.
- A good the Never buy grain setting is holding back now says so, instead of saying it is on your never-sell or never-buy list when nothing is on those lists.
- Buying a large stack no longer works out what your party can carry all over again for every single unit.

## 1.40.4

- An entry on one of your item lists that names a good the way the game shows it, such as Iron Ore, no longer also covers a different good whose short name is one of the words in it.
- An entry that mixes a short name with a name the game shows, which TradeLord.log already said matches no good, now really covers nothing instead of quietly covering part of it.

## 1.40.3

- Selling or buying a large stack to a caravan or a party of villagers on the road no longer makes the game a small extra piece of work for every single unit, which trading in a market stopped doing in 1.40.2.
- A dry run now says you already traded a good on this visit when that is what stopped it selling more, instead of naming your food reserve.

## 1.40.2

- Selling or buying a large stack at a market no longer makes the game a small extra piece of work for every single unit, which it started doing in 1.40.1.

## 1.40.1

- The download now carries every change made since 1.40.0.
- Quick-sell and quick-buy stop quietly when your party cannot be read, the way restocking food, buying a haul animal and getting your party back up to speed already did, instead of writing an error to TradeLord.log.
- Selling to a caravan or a party of villagers on the road works out what you took in once instead of twice.

## 1.40.0

- Silence trade messages now covers trading with a caravan or a party of villagers you meet on the road, which reported on screen whatever the setting said, and the setting's hint says so.
- The Always sell hint now says an animal a quest is waiting on is still held back, alongside your never-sell list and an inventory lock.
- Trading in a market no longer works out which markets are in reach all over again after each pass that moves goods, so a busy town settles faster.

## 1.39.3

- Restocking food and buying a haul animal stop before your gold reaches your gold reserve again, leaving the reserve whole, and buying for profit is the one pass that spends down to it.

## 1.39.2

- An animal a quest is waiting on is now kept back even where your always-sell list names it, instead of being sold with the rest of your cargo.

## 1.39.1

- Restocking food and buying a haul animal now spend down to your gold reserve, the way buying for profit already did, instead of always leaving a denar above it.

## 1.39.0

- Selling an animal to get your party back up to speed now counts towards the profit TradeLord has made you and earns Trade skill, the way its other sales already did.
- A visit that trades nothing now says a quest may be waiting on your animals when that is what held them back, instead of naming your food reserve.
- The best markets and the routes on the panel are worked out again as you ride, instead of waiting for the hour to turn.

## 1.38.5

- TradeLord now matches its own lines to their translations the same way whatever language your computer is set to.
- The feature list now says that asking a band to let you pass is a line you say to them, rather than the pop-up it used to be.

## 1.38.4

- Selling animals to get your party back up to speed now leaves a quest item alone, the way the rest of TradeLord's selling already did.
- Fixed the lines you can say in a conversation coming out in the wrong order for the rest of your session, when TradeLord could not place its free passage line among a band's answers.

## 1.38.3

- Fixed the line asking a band to let you pass never appearing when you met one: TradeLord went looking for the band's own talk before the game had finished writing it, so it never found where to put the line.

## 1.38.2

- Saving a campaign can no longer be failed by TradeLord's own note-keeping: if its ledger cannot be written down as the game saves, the save goes through and what went wrong is written to TradeLord.log instead.

## 1.38.1

- Fixed an error TradeLord wrote to its log on every startup, and went on writing until a campaign was loaded, because it looked for an encounter before there was a game to have one in.

## 1.38.0

- Fixed the game closing itself when you met bandits: TradeLord's offer of free passage arrived as a pop-up over the talk you were already having, and answering the bandits after it had ended the encounter shut the game down.
- Asking a band to let you pass is now a line you say to them, in among your other answers, instead of a pop-up.
- The bandits answer, the talk closes, and your party rides on, so nothing is left half finished behind the conversation.
- Free passage from bandits now says on the settings screen that it adds a line to what you can say to them.

## 1.37.10

- A visit that trades nothing now names the protection that held your goods back, your never-sell or never-buy list, an inventory lock, the unique and crafted protection, an animal a quest may be waiting on, a haul animal or a mount, or your food reserve, instead of saying only that your protections held it back.
- Where more than one protection was in the way, the message names the first one TradeLord met.
- The warning that your cargo is full now tells you what to do about it: recruit more men, buy more horses, or sell goods manually.

## 1.37.9

- Animals a quest is waiting on are now held back from every sale, not only from thinning the herd, even when TradeLord cannot tell which animals the quest wants.
- The buying pass counts what you already carry afresh for each good, so a good a market stocks twice can no longer slip past your limits on how much of one good to hold.

## 1.37.8

- Trading with a party on the road now stops at your max spend per visit, instead of buying on until your purse is down to your gold reserve.
- The campaign map no longer works out every trade route the moment it opens, so a long campaign no longer hitches before you have even opened the ledger.

## 1.37.7

- Protect unique and crafted items now covers animals as well as gear, so a unique animal is no longer sold in the trading pass while the setting is on, which is what the setting already did when TradeLord thins your herd.

## 1.37.6

- Simulation mode now models a visit as one visit rather than each pass on its own, so the merchant's gold, your purse, your carry weight, your larder and every per-item and per-visit cap carry from one pass to the next and a dry run no longer reports more trading than a real visit would do.
- Fixed simulation mode selling the same animals again every time it checked whether your party was back up to speed, and offering to buy goods an earlier pass of the same visit had already taken off the shelf.
- Simulation mode now spends what a meeting on the road just earned, and no longer buys back the goods it has just sold to that party.

## 1.37.5

- Animals a quest is waiting on are now kept back whenever TradeLord sells, not just when it thins your herd, so the cattle or horses you are carrying to deliver are no longer sold as you walk into a market.

## 1.37.4

- Goods leaving your party are now noticed as they go rather than once a day, so eating the last of a food and looting more of it before the day turns no longer leaves TradeLord counting the looted lot as something you paid for.

## 1.37.3

- Food your troops eat, and anything else that leaves your party without being sold, no longer counts as still bought, so TradeLord stops holding back looted goods of a kind you once bought and stops selling the next lot you buy too cheaply.

## 1.37.2

- Topping your food back up now buys grain again, so it works in a farming village where grain is the only cheap food there is. Never buy grain goes back to keeping grain out of trading for profit, which is what it is there for, and putting grain on your never-buy list still keeps it out of everything.
- The inventory lock setting now says how each side matches a lock: selling by item and quality, buying by item alone.

## 1.37.1

- The free passage setting now describes what it really does, asking you as you meet the band rather than adding a line to the encounter screen.
- The setting that sells animals to get your party back up to speed now says that an animal a quest is waiting on is left alone.
- The feature list now says that getting your party back up to speed keeps back the animals a quest is waiting on.

## 1.37.0

- An animal a quest is waiting on is no longer sold to get your party back up to speed. TradeLord keeps back as many as the quest asks for and thins only the herd beyond them, so delivering a herd, draught animals or horses no longer costs you the animals you gathered for it.

## 1.36.2

- An animal you have locked in your inventory is left alone again when TradeLord sells one to get your party back up to speed. A lock on a horse of a particular quality, such as a spirited or a lame one, was being read as a lock on the plain horse and so was passed over.
- A good you buy by hand is now written down at what its own quality cost you, rather than at the plain good's price, so TradeLord no longer sells a fine one on for less than you paid.

## 1.36.1

- A caravan or a party of villagers you have just traded with no longer trades with you all over again when you talk to them without riding away first, so your spending limit for that meeting is only spent once.
- Selling to a caravan or villagers on the road now makes the coin sound, the way selling in a town does.
- A Reset on the settings screen now always takes hold at once and is written to TradeLord.ini.

## 1.36.0

- Trading with a caravan or a party of villagers on the road now actually happens. It had been failing every time, because the game refuses to sell goods without a market to sell them in, so TradeLord hands the goods and the coin over itself out on the road.
- The free passage line no longer turns up among your battle orders before a fight. It is offered as you meet the band instead of sitting on a menu of the game's own that the game shows elsewhere.
- Bandits who have let you go now leave you be for a few hours, instead of turning round and hitting you again the moment the game unpauses.
- The language you pick now takes hold when you press Done on the settings screen, rather than waiting for you to restart the game, and the ledger panel changes over with it.
- TradeLord.log says how long the free passage holds and which band it holds off you.
- The feature list now says the free passage leaves the band off you afterwards.

## 1.35.2

- The language setting is back at the top of the settings screen, with auto sell and auto buy just under it.
- Dragging a slider on the settings screen no longer fills TradeLord.log with every value it passes through, and no longer rewrites TradeLord.ini for each one: your change is written down once, when the slider comes to rest.

## 1.35.1

- Turning down the bandits' offer of free passage no longer stops them offering it again the next time you run into that same band.

## 1.35.0

- The language you pick now reaches the town menu entries as well, so Trade here now and Consult the TradeLord ledger change over the moment you pick it instead of waiting for you to load a campaign again.
- Auto sell and auto buy now sit at the very top of the settings screen, above the language setting.
- Observation shelf life is gone: a price you recorded yourself is kept for as long as you have it, rather than being thrown away once it reached a certain age. Whatever you had set is dropped the next time TradeLord reads your settings.
- Quiet automation is called Silence trade messages now.
- How many of each kind to keep is called How many of each kind of food to keep, and its hint says plainly that it counts the food itself rather than days, because Food reserve (days of supply) is the one that works in days.
- It now starts at two of each kind rather than three.
- The Reset button now puts your settings back at once instead of taking several seconds over it, because it only redraws the settings that had actually been changed.
- The hint under Ledger panel hotkey (map screen) now says that one key name is all it takes, and that a word or an unknown key falls back to T.
- TradeLord.log now says how many of your settings had been changed when you press Reset.
- The feature list no longer offers a shelf life for the prices you recorded yourself, and now says the language reaches your town menu entries too.

## 1.34.0

- TradeLord now trades with a caravan the moment you meet it on the road, before anyone says a word, instead of waiting for you to pick a line of dialogue first.
- A party of villagers on the road now trades with you the same way a caravan does.
- Trade with caravans you meet is called Trade with caravans and villagers you meet now, and it governs both.
- Bandits now offer you free passage as soon as you meet them, asking whether to ride on or fight, so the offer no longer waits on a menu that never appeared for some of you.
- TradeLord.log calls these a sale or a purchase on the road now, and names the party it traded with.
- TradeLord.log names the band each time free passage is offered.
- The feature list now says a caravan or a party of villagers is traded with the moment you meet them, and that bandits offer you free passage as you meet them.

## 1.33.0

- Your setting for buying haul animals is called BuyHaulAnimals in TradeLord.ini now, and whatever you had saved under its old name is carried over the first time this version reads your file.
- The hint under Share of the hold one good may fill now calls them haul animals, the same name the rest of the settings screen already uses.
- TradeLord.log now calls a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse and a Pack Camel a haul animal, rather than an animal that carries for you.
- TradeLord.log no longer opens every campaign with a roll call of every animal in your game, so the log starts on what you actually did.
- The hint under the Language setting now says what really happens: the language takes hold as you pick it, and your town menu entries follow the next time you load a campaign.
- The feature list now describes what TradeLord already does: a caravan on the road held to every rule a market visit is, your herd looked at three times a visit, the Reset button at the top of the settings screen, and TradeLord.ini written whether or not Mod Options is installed.

## 1.32.0

- The ledger panel now counts the gold in your purse when it works out how many of a good a route is worth, so it no longer offers you 32 of something you can only pay for 3 of.
- Your gold reserve is held back from that count too, the same way it is when TradeLord buys for you, and so is your spending cap for the visit.
- When your purse is what is holding the ledger back, the panel now says so and names your gold and your reserve, instead of blaming your travel ceilings.
- The feature list now says your own purse is counted into every route the ledger quotes.

## 1.31.1

- The feature list now calls a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse and a Pack Camel haul animals where it describes getting your party back up to speed, the same name the settings screen and the rest of the list already use.

## 1.31.0

- Trading with a caravan on the road now sells the goods you looted when the ones you bought fall short of your profit margin, the way a market does, instead of selling nothing at all.
- Hold cargo for the best market now holds your cargo back from a caravan on the road as well, instead of only from a town.
- A caravan can no longer sell you enough livestock to drop your party into the herd speed penalty, because buying from one is now held to the same herding calculation a market is.
- Stop buying at this many held and Share of the hold one good may fill now hold when you buy from a caravan, not only in a market.
- Economy settling delay now holds on the road too, so a caravan will not trade with you before the day you set either.

## 1.30.3

- Installing Mod Options after playing without it no longer replaces every setting in TradeLord.ini with the ones TradeLord ships with.
- TradeLord.log now says why your settings file was the one TradeLord read, whether it was saved more recently than the settings screen or was written when there was no settings screen at all.

## 1.30.2

- Loading a saved game while you are inside a town or village no longer stops TradeLord from selling the animals that are slowing your party down as you ride back out.
- TradeLord.log no longer says a trade was turned down as you leave a castle, a hideout or anywhere else TradeLord never trades in the first place.
- With Buy to fill the ships off, getting your party back up to speed now keeps enough haul animals for what your carts are carrying, instead of measuring against your ships.
- TradeLord.log now says once why TradeLord cannot work out your herd, whichever part of it asked first.

## 1.30.1

- The feature list and what it needs now both say TradeLord runs on the Bannerlord 1.5.2.121216 beta as well as on 1.4.8.119303.

## 1.30.0

- TradeLord now buys only the animals that carry for you, a Mule, a Sumpter Horse, a Work Horse, a Saddle Horse or a Pack Camel, so it no longer spends your gold on a Rouncey or a Charger while a Mule is sitting in the same market.
- It never pays more than the cheapest price it knows of for a haul animal, so Most it will pay for a haul animal when your bags are full is gone from the settings screen and the value you had saved for it is dropped the next time TradeLord reads your settings.
- Restocking food now buys only where the market is asking no more than the cheapest price you know of for it, instead of taking the cheapest thing on the shelf whatever it costs.
- Buying a haul animal and restocking food both stop before your gold reaches your reserve, so there is always something left to trade with.
- Buy haul animals and mounts is called Buy haul animals now, because riding horses and camels are no longer among the animals it buys.
- The hints under Buy haul animals and Restock food (days of supply) say what those settings now do, and the feature list follows them.

## 1.29.1

- Getting your party back up to speed as you leave a settlement now happens only where the game would have let you trade there in the first place, so TradeLord no longer sells on the way out of a place it refused to trade in on the way in.
- When it holds back for that reason it says so in TradeLord.log, naming the settlement.

## 1.29.0

- TradeLord now looks at your herd three times a visit instead of once: as you enter a settlement, again once it has finished trading there, and once more as you leave, so a penalty that arrives while you are in town is caught before you ride out.
- A herd penalty no longer has to come from picking up an animal: losing men in a battle or to desertion shrinks how much your party can drive, and each of those three checks now notices that on its own.
- While you are on the road with no market in reach, TradeLord.log names the herd once a day for as long as it is slowing you down, so you can see when the penalty arrived and what caused it.
- Every herd check is written to TradeLord.log with the numbers behind it: how many men you have and how many are on foot, how many loose mounts nobody is riding, how many pack animals and how much livestock, how many animals are being driven in all, and how many must go.
- Every animal that comes into your party or goes out of it is now named in TradeLord.log with what was paid, why it moved, and what TradeLord counts it as, whether it came from the selling pass, the buying pass, restocking, the baggage train, a caravan on the road, or getting your party back up to speed.

## 1.28.1

- Getting your party back up to speed now works out how many haul animals your cargo needs the same way TradeLord works out what your party can carry, so a party that never puts to sea will part with a spare haul animal instead of holding on to every one of them.

## 1.28.0

- Getting your party back up to speed now sells in a set order: livestock first, then a spare riding horse or camel of the plain horse kind that nobody is riding, then your haul animals, and your war horses and noble horses last of all.
- It never sells a horse one of your men on foot is riding, because that horse is not in the herd and selling it would not speed you up at all.
- Livestock and haul animals can go this way now, so a party that has picked up more mules than its men can drive is no longer stuck slow with nothing TradeLord can do about it.
- It keeps enough haul animals to carry what your party is already carrying, so getting back up to speed can never leave your cargo on the ground.
- The setting that governs all this is now called Sell animals that slow you down, because spare mounts are no longer the only thing it sells.
- A Saddle Horse is now one of the animals TradeLord buys for your baggage train and keeps out of its ordinary selling, alongside the Mule, the Sumpter Horse, the Work Horse and the Pack Camel.
- A Mule or a Pack Camel that cannot be ridden is no longer kept as a haul animal, so it is now sold like any other cargo.
- The hint under the Language setting now says only that a language change needs the game restarted.
- The animal roll call in TradeLord.log now also says whether the game counts each animal as food and whether it can be traded at all.
- TradeLord.log now names both animals when an entry on one of your item lists means two different animals that TradeLord treats differently, such as Saddle Horse, and gives you the item id for each.

## 1.27.5

- The hint under the language setting is back to plain English: it names what TradeLord speaks in and says the town menu entries change when you next load a campaign, without the paragraph about the settings screen that 1.27.4 added.

## 1.27.4

- The language you pick on the TradeLord settings screen now reaches the screen itself: the name and hint of every setting, the headings above them and the Reset button are all spoken in it as you pick it, instead of staying in the old language until you restart the game.
- The words inside each list of choices follow the next time you open the settings screen, and the hint under the language setting now says as much.

## 1.27.3

- Your settings in TradeLord.ini are no longer written over with the shipped ones as the game starts, which happened whenever Mod Options had not finished loading yet.
- TradeLord reads the file as it stands in that case, so anything you set by hand survives, and the settings screen takes over as soon as Mod Options is ready.

## 1.27.2

- Bannerlord no longer closes itself as it reaches the main menu when TradeLord and Mod Options are installed together, which it has done since 1.26.0.
- The TradeLord settings screen comes back with it, Reset button and every setting on it.

## 1.27.1

- Picking a new language on the TradeLord settings screen now takes hold as you set it, so the choices under each setting are spoken in it straight away instead of waiting for you to restart the game.
- The screen is asked to draw itself again at the same moment, so its own names, hints and headings follow the new language too.
- The hint under the language setting no longer says the screen changes language the next time you open it, because that was not what happened.

## 1.27.0

- TradeLord.log now opens every campaign with a roll call of every animal in your game, counted by what TradeLord does with each one: the haul animals it keeps for your baggage train, the mounts it may sell as spares once they slow you down, the livestock it trades, and anything that is none of the three.
- Each animal is listed with the game's own answers about it, whether it is rideable, a pack animal, a mount or livestock, how much meat it is worth and which trade category it sits in, so a mod that adds an animal or changes what an existing one does shows up plainly.

## 1.26.1

- A number in TradeLord.ini is now held to the same limits the settings screen holds it to, so a file that has been damaged or edited to something impossible can no longer put TradeLord into a state you could never have set on the screen.
- A number that has been pulled back inside its limits says so in TradeLord.log, naming the setting, what the file asked for and what was used instead.

## 1.26.0

- A Reset button now sits at the very top of the TradeLord settings screen, and it puts every setting back to the value TradeLord ships with in one click.
- Resetting empties your never sell, always sell, never buy and always buy lists too, and it is written into TradeLord.ini straight away so nothing is left half changed.
- TradeLord.log now lists every setting you have moved away from the value TradeLord ships with, each time you start the game.
- Changing a setting is written to TradeLord.log as it happens, naming what it was, what it is now, and what TradeLord ships with, so a log you send tells the whole story.
- The button on the settings screen is spoken in the language TradeLord is set to, like the rest of that screen.

## 1.25.0

- TradeLord.ini and the MCM settings screen are twins now: both are kept up to date, so you can install or remove MCM whenever you like and every setting you have made comes with you.
- Each of them records when it was last saved, and if the two ever disagree the one saved last wins and the other is written to match it.
- Editing TradeLord.ini by hand while MCM is installed now works, because TradeLord sees the file is the newer one and sets the settings screen from it.
- Changing anything on the settings screen writes it straight into TradeLord.ini, so removing MCM never costs you a setting.

## 1.24.0

- TradeLord.ini now says which shape it is in, and TradeLord brings an older file forward by itself when it changes how a setting works, so a setting you set is never quietly lost.
- Anyone whose file still keeps some of every kind of food as a single number is carried over to the switch and the amount that replaced it, and the old number becomes how many of each kind to keep.
- Anyone whose file still has keeping smeltable weapons as a switch is carried over to the choice of three that replaced it, on becoming keep every one and off becoming sell them.
- Everything TradeLord carries over is written to TradeLord.log in plain words, so you can see what it did with a setting you had set.

## 1.23.1

- The last note about an animal that carries nothing for you should not have offered the Old Work Horse as an example, because a work horse carries for you whatever its age and TradeLord keeps it like any other haul animal.

## 1.23.0

- An animal that carries nothing for you and is not livestock, such as an Old Work Horse, is no longer fenced in with your haul animals, so it is sold like any other cargo instead of sitting in your bags for good.
- The setting for how much more TradeLord will pay while your bags are full is now called Most it will pay for a haul animal when your bags are full, and it is explained in plain words.
- The feature list now says where the line falls between a haul animal TradeLord keeps for you and an animal it sells.

## 1.22.0

- TradeLord now sells a spare mount nobody in your party can ride, once those spares are dragging you into the herd speed penalty, so your footmen turning into cavalry no longer leaves the party crawling.
- It sells the cheapest spare first, so your war horses stay in the baggage, it never sells a haul animal that way, and one switch on the settings screen turns the whole thing off.
- A mule, a sumpter horse, a work horse and a pack camel are now called haul animals on the settings screen and in the feature list, and all four are named rather than described.
- The feature list now says TradeLord pays up to the cheapest price it knows of for a haul animal, and up to 1.5 times that while your cargo is full.
- The livestock policy, the unique and crafted protection and the always-sell list no longer say a mount can never be sold, because now one can.

## 1.21.1

- The feature list now names the animals TradeLord buys for your baggage train, a mule, a sumpter horse, a work horse or a pack camel, and says it buys a horse or a camel your men can ride while you still have troops on foot.

## 1.21.0

- Looted gear now sells to the first market that can pay for it, and Hold cargo for the best market, which is off out of the box, is now the only thing that makes any of your cargo wait for a better town.
- Asking bandits for free passage is no longer labelled a cheat: the line in the encounter screen now reads [TRADELORD] and its setting is called Free passage from bandits.
- The setting that buys mules and horses now calls them animals rather than beasts.
- The feature list now says how much gold the town you would sell to has rather than what is in its till, names your gold reserve as 300 denars, and says what the settings you get out of the box are aimed at.

## 1.20.1

- The line saying TradeLord bought pack animals now comes just after the line crediting your Trade skill, rather than before it.
- The feature list now covers what the ledger panel shows along its top, the warnings TradeLord puts on screen, leaving a village its last of each good, and naming a mount on your always-sell list to sell it.

## 1.20.0

- TradeLord now trades with a caravan you meet on the road: talk to it and the deal is already done, and the caravan pays out of its own purse.
- A new setting caps how much of your hold one good may fill as a share rather than a flat count, so the ceiling grows with your carts instead of needing a new number every time your party does.
- A horse one of your unmounted men can ride no longer counts against the herd, so a party driving a full herd still buys the cheap horses its footmen can climb onto.
- The feature list covers trading with caravans and the new share of the hold.

## 1.19.0

- Keeping smeltable weapons is now a choice of three, and the new one keeps a weapon only while a part of it is still locked in your smithy, so your bags stop filling with loot that can teach you nothing.
- Buying beasts now covers riding horses as well as mules and sumpter horses, so a cheap horse gets bought too and your foot troops have something to ride.
- Asking bandits to let you go is now on out of the box, and one switch turns it off.
- The feature list explains the three ways to play the smeltable weapon setting.

## 1.18.0

- TradeLord now buys mules and sumpter horses when a market is asking no more than one is worth, so your party can carry more, and it will pay half again as much for one while your cargo is full.
- It never buys more pack animals than your party can drive without slowing down, and your gold reserve and your spending limit for the visit still hold.
- Restocking now tops your food back up to three days of supply instead of five.
- A new cheat, off until you switch it on, lets you ask looters, sea raiders and the rest of Calradia's bandits to let you go, and they will, with no fight and no ransom.
- The setting that keeps smeltable weapons now says plainly what it holds back: every weapon built from smithing parts, looted ones included, whatever parts you have already learned.
- The feature list now covers what TradeLord buys for your baggage train and what it leaves alone, and what it needs installed has moved to the foot of it.

## 1.17.0

- TradeLord now tops your food back up to five days of supply as you trade, buying whatever food a market has going cheapest at whatever it asks, before it trades for profit.
- It now holds back three days of your troops' wages on top of your gold reserve, so a shopping trip never eats the payroll.
- A new switch keeps every weapon the smithy can break down for parts, so a smithing playthrough stops selling off its own raw material.
- A new switch sizes what it buys to what your ships can hold rather than what your carts can, for a War Sails fleet.
- The ledger panel's cargo line follows that same capacity, so it counts the ships whenever that switch is on.
- Without MCM installed, TradeLord now writes a TradeLord.ini beside its log that you can edit to change any of its settings.
- The notice about trading as you arrive now points you at that file when MCM is missing, instead of telling you to go and install MCM.
- Default in the settings screen now puts every setting back to the one TradeLord ships with, instead of leaving them exactly as they were.

## 1.16.1

- The switch that keeps every kind of food now names the food variety morale bonus the way the game does.

## 1.16.0

- Keeping some of every kind of food is now a switch, with its own setting for how many of each to keep, starting at three.

## 1.15.0

- TradeLord can now hold back a few of every kind of food you carry, so auto-selling no longer costs your party its food variety morale bonus.

## 1.14.5

- The ledger panel's title, buttons and column headings now follow the language you pick, instead of staying in the one they were first drawn in.

## 1.14.4

- TradeLord no longer says a market has nothing worth trading when the reason nothing sold was your own cargo.

## 1.14.3

- A market whose merchant has run out of gold is no longer named as the best place to sell, so TradeLord stops buying cargo for a town that cannot pay for it and stops holding goods back for one.

## 1.14.2

- TradeLord no longer buys a good that your own settings will never let it sell, so a category you set to buy only no longer fills your cargo with goods it will not move on.
- The ledger panel opens faster, most noticeably in a long campaign with many markets in reach.

## 1.14.1

- The ledger panel no longer opens on its hotkey while you are typing into a box on the campaign map.
- The best-market tolerance hint now says the price floor it sets always holds back goods you never bought, such as looted gear, and not only when the setting above it is on.

## 1.14.0

- TradeLord now speaks Simplified Chinese, chosen from the same Language setting that already offered English, Turkish and Russian.

## 1.13.3

- The notice that TradeLord trades as you arrive is shorter, so it sits on screen like its other messages instead of running long.

## 1.13.2

- Without MCM installed there is no settings screen, so the notice that TradeLord trades as you arrive now says that plainly and names MCM, instead of sending you to settings you do not have.

## 1.13.1

- The best-market marker no longer points at a town whose merchant has no gold left to pay for your cargo.
- TradeLord no longer warns that it cannot buy at a market when you have turned both auto buy and its town menu trade entry off.
- A panel hotkey with something other than Ctrl, Alt or Shift in front of the key now says so in the log, rather than quietly opening on the bare key.
- Walking into a market is a little quicker when your never-sell, always-sell, never-buy and always-buy lists are all empty.

## 1.13.0

- The settings screen now ends with Selling and Buying, side by side, with every other setting above them.
- The settings that shape both halves of a trade, along with the two town menu entries, have moved into General.
- The two switches that trade as you arrive are now called Auto sell and Auto buy.
- The separate switch that decided whether TradeLord could buy at all is gone, so buying now follows Auto buy and the trade entry in the menu alone.
- The note under the food policy no longer says where on the screen the food reserve sits, since it has moved.

## 1.12.1

- A market where TradeLord traded nothing at all now says so in one line instead of two, naming what stopped it.
- A market where TradeLord did trade no longer also tells you the other half of the pass moved nothing.

## 1.12.0

- TradeLord now tells you on screen why it traded nothing at a market, when it trades as you arrive, instead of writing the reason to its log alone.
- Turning quiet automation on keeps those new lines off the screen along with the rest.
- TradeLord now speaks Russian, chosen from the same Language setting that already offered English and Turkish.
- A new setting hides the TradeLord ledger entry in the town and village menus, the way the trade entry could already be hidden.
- The ledger panel has dropped its Data column, and its item, buy town and sell town columns are wider for the room.

## 1.11.1

- Gaining or losing ships now changes which markets TradeLord counts as being in reach straight away, instead of leaving some of them out until the hour turns.

## 1.11.0

- The food, smithing material and livestock rules are now picked from a list that names each choice, instead of a slider you set to 0, 1, 2 or 3.
- The setting that decides what a good counts as having cost you is picked from a list too, and it and its note are written in plain words now.
- Selling looted gear now starts switched on at tier 1, which is what looters and bandits drop, and you can still raise or switch it off.
- A new always-buy list matches the always-sell one: name a good on it and TradeLord buys it past the category rules and past the never-buy-grain switch.
- Your never-sell and never-buy lists and anything you locked in the inventory still hold, and it still buys only what it can sell on for more somewhere in reach.
- Switching those four settings over to lists keeps whatever you had already picked, so nothing resets.

## 1.10.0

- The settings screen now reads in the language you picked in TradeLord's own Language setting, instead of the language the game itself is set to.
- The screen takes a new language the next time you open it, and the note under the Language setting now says so.

## 1.9.1

- The ledger panel now opens with T instead of L, which the game already uses for one of its own screens.
- The key is still yours to set in TradeLord's settings, and one you have already chosen there is left alone.

## 1.9.0

- The switches that decide when TradeLord sells and buys now say plainly what they do, instead of naming quick-sell and quick-buy entries the town menu no longer has.
- Every other setting that still talked about quick-sell and quick-buy now names selling and buying instead, in English and in Turkish.
- The auto-trade switch is gone, since it only read back the two switches above it and turned them on and off together.

## 1.8.0

- A new setting stops quick-buy adding to a good once you already carry as many as you allow, so a stock of hundreds no longer grows every time you walk into a market.
- The town and village menus now carry one TradeLord trade entry instead of three, and it sells and then buys in one go.
- Trading done automatically as you arrive is unchanged, and the setting that showed the old entries now shows or hides the single one.

## 1.7.0

- TradeLord now speaks Turkish. A new Language setting at the top of its options switches what it says in the game, and it starts on English.
- Playing Bannerlord in Turkish now puts the whole of TradeLord into Turkish, its settings screen included.

## 1.6.33

- The settings screen is now built against Mount and Blade Mod Configuration Menu 5.12.3.

## 1.6.32

- Naming a good on the never-sell, always-sell or never-buy list no longer quietly catches a second good whose whole name is one of the words you wrote, so putting Iron Ore on a list leaves Iron alone.
- The ledger panel now spends your per-item and per-visit denar caps unit by unit, the way a buying pass spends them, so a route no longer offers a quantity your caps would stop you buying.

## 1.6.31

- The town marked on the map for your cargo is now one whose merchants can actually pay for it, instead of one that quotes a high price but has run out of gold.
- A map pin that cannot be put back when a save loads no longer takes the TradeLord entries out of the town menu with it.

## 1.6.30

- A pass that trades nothing now gives the same reason every time, instead of naming a different one from visit to visit when two rules held back as much as each other.
- Working out how far away a market is no longer re-reads your party speed for every market measured, so the ledger panel and the price tooltips open with less of a pause.

## 1.6.29

- TradeLord now tells you as you enter a market when your purse is at or under your gold reserve, so you know why it is buying nothing.
- A pass that bought nothing now names your purse or your spending caps as the reason whenever that is what stopped it, instead of blaming prices or saying nothing at all.
- The cargo-full warning stays quiet when the purse warning has already been given, since selling clears both.

## 1.6.28

- The warning that your cargo is full is now given once, as you enter a market, instead of a second time as you leave it.

## 1.6.27

- An entry on one of your item lists that still matches no good after you correct it now says so on screen again, instead of going quiet after the first warning.
- Changing a setting no longer sends TradeLord through every good in the game when your never-sell, always-sell and never-buy lists are all empty.

## 1.6.26

- The never-sell, always-sell and never-buy lists now take the name a good is shown under, such as Iron Ore, as well as its item id, so a name with a space in it no longer reads as two entries that match nothing.
- An entry on one of those lists that matches no good in the game is now named on screen and in the log, instead of quietly doing nothing.
- A new quiet automation setting keeps trading done on entry to the log, off your screen.
- Starting a second campaign without closing the game now logs the ledger panel's hotkey and map button again, rather than staying silent about the second one.

## 1.6.25

- The message shown when the ledger has no routes for you now points at your travel ceilings, and only asks you to visit more markets when TradeLord is set to use the prices you have seen in person.

## 1.6.24

- The first market of a campaign is now left alone when TradeLord tells you it trades on entry, so you can turn that off before it does anything, and it starts trading from the next market.

## 1.6.23

- TradeLord is built for Bannerlord 1.4.8.119303, the version it is played and tested on.

## 1.6.22

- The ledger panel now opens with L instead of T, because T also opens the game's own message panel and the two fought over the key.
- Buy cap per item can now be set to 0 to turn the cap off, the way the two settings beside it already could.
- Scan radius, observation shelf life and buy cap per item now say on the setting itself what 0 does.
- A market where TradeLord bought nothing because your gold reserve or spending cap was reached now says so, instead of trading quietly and leaving you to guess.
- The item tooltip no longer puts a TradeLord heading above the prices it adds.
- The line of notes under the ledger is now large enough to read.

## 1.6.21

- The cargo-full warning now appears only on a visit where TradeLord traded nothing, instead of following a pass that had just bought until the hold was full.
- Profit credited to your Trade skill is reported in one line instead of two, and that line names your new Trade level when the skill rises.

## 1.6.20

- A price or purchase record for an item whose name carries an unusual character is now left out of the save instead of coming back as a record for an item that does not exist.

## 1.6.19

- The price ledger and your purchase records carry over from a campaign that was saved by an earlier version.

## 1.6.18

- TradeLord now says once in each campaign that it buys and sells for you as you enter a market, and names the two settings that turn that off.

## 1.6.17

- The TradeLord entries in the town and village menus no longer go missing when the map marker cannot be restored as your save loads.
- A menu the game does not have no longer costs TradeLord the entries in the menus that it does have.
- Starting another campaign without closing the game no longer leaves livestock buying switched off because of something that happened in the first one.

## 1.6.16

- A market that pays less than the town you are holding your cargo for now says so, instead of blaming your profit margin.
- A herd already as large as your party can drive now says so, instead of reporting that there is no room to carry more.
- The town TradeLord marks as your best place to sell is put back on the map as soon as you load a save, instead of waiting for the next day or the next settlement you enter.

## 1.6.15

- The log now names every market that traded nothing, instead of falling silent after the first market that gave the same reasons.
- Ending a campaign and starting another without closing the game no longer leaves the log silent about problems in the second one.
- A recorded price your save cannot read is now dropped as the ledger loads, instead of stopping the ledger from loading at all.
- With observed prices set never to expire, your save no longer keeps an entry for a good whose recorded prices have all gone.

## 1.6.14

- The town TradeLord marks as your best place to sell no longer removes a marker you had placed on that town yourself.
- A town you pinned in the ledger panel is marked on the map again as soon as you load the save.
- Turning live world prices off while you are standing in a market now records that market at once, instead of leaving it blank until you leave and come back.

## 1.6.13

- A line of MCM newer than the one this build was made for is now named in the log as exactly that, instead of being reported as MCM missing entirely.

## 1.6.12

- Selling goods you never bought, such as loot, now counts profit and Trade XP against what those goods would have cost you at the cheapest market you know of, instead of treating the whole sale price as profit.

## 1.6.11

- Your save no longer keeps a purchase record for every good you have finished selling, so a long campaign stops collecting a dead entry for each item you have ever traded.

## 1.6.10

- The TradeLord button on the campaign map now reserves only the space the button itself covers, instead of a fixed strip that reached well past it on ultrawide and other non-16:9 screens.

## 1.6.9

- The settings screen was the last part of the mod left in English only; every setting name, hint and heading now ships in the language file and can be translated.

## 1.6.8

- The TradeLord button on the campaign map no longer swallows map clicks well above and below the button itself.
- The days of food you asked to keep are no longer partly spent on goods quick-sell was never going to sell.
- Other mods now get first say over their own notifications while TradeLord is trading.

## 1.6.7

- The Trade XP message now names its number for what it is: the denars of profit credited to your Trade skill.
- Clicking a town in the ledger panel now pins it even when TradeLord is already marking that town as your best place to sell.
- Quick-sell no longer spends part of your food reserve on goods it then passes over because you bought them here on this visit.
- A trade message the game refuses to show no longer leaves the rest of that pass's messages repeating.

## 1.6.6

- Putting an item on the always-sell list no longer makes quick-buy purchase it against your category policy.
- Loading your settings no longer switches quick-buy back on when your saved settings say it should be off.
- The route lines in the ledger popup are now translatable, like every other line the mod shows.

## 1.6.5

- A village that is down to its last of each good now says so, instead of reporting that it has nothing worth trading.
- The ledger panel now reopens instantly within the same game hour, and the Refresh button rescans whenever you want fresh numbers.
- Trade messages left over from a campaign you have closed no longer appear once you are back at the main menu.
- A damaged never-sell, always-sell or never-buy list in the settings file no longer stops trading outright.

## 1.6.4

- TradeLord now warns you in red when your cargo is full, both as you walk into a market and as you leave it, instead of quietly buying nothing.
- Restored the working-notes file to the repository; nothing that ships with the mod changed.

## 1.6.3

- A release whose commit body is blank is now stopped before it publishes, instead of going out with an empty description as 1.6.1 did.

## 1.6.2

- Every setting now explains itself on hover; keep food, never sell, always sell and the coin sound had no description before.
- The never-sell and always-sell descriptions now say where to find an item id and which of the two lists wins when they disagree.
- The buying settings are now written in the order they appear on screen.
- A fault while showing a trade message, or while closing a campaign, is now logged and stepped over instead of reaching the game.
- Asking the ledger what a good cost when it has never seen that good now answers instead of faulting.
- Tidied the layout of the two trading passes; they behave exactly as before.
- Removed the working-notes file from the repository; nothing that ships with the mod changed.

## 1.6.1

- The Trade XP a trade earns is now reported in amber, right after the sold and bought lines instead of before them.
- Raised the default gold reserve from 100 to 300, enough for two bribes for safe passage and a wage payment. Your own setting is unchanged.

## 1.6.0

- Fixed a trade that failed partway through being able to stop the game showing any notifications at all until you reloaded, including TradeLord's own trade summaries.
- Ending a campaign no longer leaves leftover trade state behind for the next one.
- Rewrote every settings description in plain English.
- The panel and the ledger popup now say "resale safety factor", the same name the settings screen uses, instead of "resale haircut".
- Added this changelog.
- Added zip files and generated release notes to the ignore list.

## 1.5.11

- Replaced the black star in tooltips with a plain asterisk, because the game's font may not have that character and would draw an empty box.
- Fixed the panel's workshop board listing every workshop in Calradia even with live world prices switched off; it now lists your own.
- Renamed the panel's "Total profit" line to "TradeLord profit", which is what it actually counts.
- Removed the mentions of other trade mods from the settings hints.

## 1.5.10

- Fixed a town with no price for a good being called the cheapest place to buy it, because a missing price read as 0 and 0 sorts cheapest.
- Fixed the log setup being able to stop the whole mod loading if your Documents folder path could not be built.
- Moved the build onto checkout v7, setup-dotnet v6 and upload-artifact v7, off the retired Node 20 runtime the old versions needed. The compiler and the DLLs are unchanged.

## 1.5.9

- Fixed a town pinned in the ledger panel not being unpinnable after you reloaded the campaign.
- Fixed the same problem letting the automatic marker delete a pin you had placed by hand.

## 1.5.8

- Fixed the ledger panel having no translatable text at all, so players on any other language got an English panel.
- Fixed the tooltip's "Profit: +N%", "Stock: N" and "~N days" labels not being translatable.
- Every line the mod shows now carries a translation marker and ships in the language file, so the whole mod can be translated or forked without touching the source.

## 1.5.7

- Fixed a stack of part-bought, part-looted goods being refused entirely when the bought units missed the profit margin, stranding the looted units that had no cost to clear.
- The purchase record is now drained against the units it actually covers, in every cost-basis mode.

## 1.5.6

- The build no longer needs a changelog entry to publish a release; the notes come from the commit instead.
- Moved both build projects next to the source they compile, so CI and a local build compile the same thing.
- Added the build output folder, IDE folders and the log to the ignore list.
- Fixed the log being written to the game's program folder, where the write fails silently under Program Files; it now prefers your Bannerlord user folder.
- Fixed live-price mode saving a price note for every good in every settlement you entered, which nothing in that mode ever reads; existing saves shrink.
- Narrowed the message filter so it only silences the game's own trade messages during a pass, not other mods'.
- Fixed a manual purchase being recorded at the price after the trade, which is higher than what you actually paid.
- A failed panel setup is now retried twice more before being given up on.
- A panel hotkey the game cannot name is now reported in the log instead of silently becoming T.
- Fixed the cargo marker picking the first town it scanned even when the cargo was worth nothing anywhere.

## 1.5.5

- Fixed a dry run blocking the real trade that followed it in the same visit.
- Fixed the best-market floor never reaching the loot it exists to protect.
- Fixed a mixed stack charging the looted units a cost they never had.

## 1.5.4

- Fixed the publish step still being able to lose a release to a timeout on retry.
- Fixed three refusals being answered with a message that was not true, such as blaming your goods when the merchant was simply out of money.
- The panel now marks which routes it could not price unit by unit.
- Fixed an English footnote sitting in an otherwise translated dialog.
- Removed every comment from the C# sources and the build workflow.

## 1.5.3

- Corrected the README's count of audit findings.
- Corrected the README's count of releases and the version it named.
- Corrected the Nexus description of what the four Harmony patches do.
- Corrected a wrong check count printed in the 1.5.2 notes.
- Noted that all four wrong numbers were the same failure: a fact written by hand that nothing verified.

## 1.5.2

- Fixed the panel listing round trips it could only half make, when a category was set to buy only.
- Fixed the War Sails port menus being registered by trying and hoping rather than checking.
- Fixed the panel and the tooltip quoting different prices for the same shelf.
- Removed two fields from the internal price quote that nothing ever read.
- Fixed a release being lost when the publish step timed out.

## 1.5.1

- Fixed observed-price mode quoting live prices through the new bulk pricing.
- Fixed a shelf that cannot be priced per unit being asked for its price once per unit, giving the same answer every time.
- Fixed the confidence score partly measuring two of the game's own price functions disagreeing with each other.
- Fixed the "nothing traded" message usually naming the wrong reason, because loot in your inventory drowned out the real one.
- Fixed two settings sharing a position with two others in the Action group.
- The quiet no-trade log line no longer repeats itself at every town gate.

## 1.5.0

- The panel now prices a whole lot one unit at a time, the way you will actually buy it, instead of multiplying one unit's spread by the quantity.
- Routes are now ranked by how likely their profit is to survive the trip, not by profit alone.
- When a pass trades nothing, it now says which rule stopped it.
- Food, smithing materials and livestock each take a policy now - ignore, sell only, buy only, or buy and sell - instead of a plain on/off switch.
- Added the mod's menu entries to the War Sails port menus.

## 1.4.3

- Fixed observed-price mode going on quoting prices it had itself moved.
- Fixed the ledger panel being torn down and rebuilt every time you opened any other screen.
- The cost basis now asks the same livestock question the rest of the mod asks.
- Fixed Trade XP collapsing to almost nothing on goods you had not bought.
- The best-market floor now always guards goods with no cost basis, replacing a cost the mod used to invent for them.

## 1.4.2

- Fixed two more village states the game refuses to trade in still being offered as destinations.
- Fixed the travel fallback returning a straight-line distance paired with a pathfinder's land ratio.
- A visit no longer shows the same "market is still settling" message twice.

## 1.4.1

- Fixed quick-buy pricing the whole shelf before noticing it had no money to spend.
- Fixed a pass stopped by the safety guard blaming your trade policy instead of saying what actually happened.
- Fixed the route planner offering a village's last unit, which the mod would never actually take.
- Auto-trade on entry no longer re-asks what counts as a market.

## 1.4.0

- Fixed the ledger panel not scrolling, because one widget path was missing a level.
- The panel hotkey now accepts a modifier, such as Ctrl+T.
- Added a running campaign profit total to the panel's top row, saved with the campaign.
- Trade summaries are now colored: green for profit, amber for none, blue for spending, grey for notices.
- Trade summaries are no longer buried under the messages the game posts when you enter a town.

## 1.3.35

- Fixed the panel's Refresh button not actually refreshing the routes, because it rebuilt them from the same cache.
- Tidied one redundantly written type name.

## 1.3.34

- Fixed the two travel-distance calculations disagreeing about whether your party can sail.
- Fixed the travel cache storing days instead of distance, so its numbers went stale the moment cargo changed your speed.
- Buying a ship now clears both travel caches at once instead of leaving land-only distances in place for up to an hour.
- Fixed prices going stale after a manual trade in the vanilla trade screen, the same way they did after the mod's own trades.
- Fixed the last price paid being rounded down where the average paid is rounded properly.

## 1.3.33

- Fixed a fully sold stack keeping a denar or two of leftover cost, which nudged the next purchase's average price up.
- Prices are now refreshed after the mod's own trading moves them, instead of the panel and tooltips quoting stale numbers for the rest of the hour.
- Renamed "Protect mounts, unique and crafted items" to "Protect unique and crafted items", because mounts are protected regardless of the setting.
- Removed the leftover chunk-size constant and the two lines that could no longer run.

## 1.3.32

- Corrected a claim in the 1.3.31 notes that simulation mode had become exact; only real trading did.
- Simulation mode now says it is a best case, in the message, the log and the settings hint.

## 1.3.31

- Fixed a sale closing below your minimum profit margin, and sometimes at an outright loss, because trades ran in chunks of ten and the price was only checked once per chunk.

## 1.3.30

- Fixed a damaged purchase record being able to stop a save from loading.
- The purchase index is now rebuilt in one place instead of two.
- Verified that loading MCM settings does not depend on the order the properties are applied in; no change needed.
- Verified that auto-trade on entry does not depend on which handler the game runs first; no change needed.

## 1.3.29

- The buy-side margin rule now has one copy instead of four written two different ways.
- The market-eligibility filter now has one copy instead of one per knowledge mode.
- The panel's column widths are now checked automatically instead of measured by hand.

## 1.3.28

- Fixed the straight-line travel estimate being able to exceed the real one, which would have wrongly hidden reachable markets.
- Tested the faster route search against every possible pair over 300,000 random cases, and it picked the same route every time.

## 1.3.27

- The tooltip hook now declares only the two arguments it actually reads, so a game update has fewer ways to break it.
- Both assemblies now compile with warnings treated as errors.
- Corrected a README claim that both assemblies build clean with every analyzer at maximum level.

## 1.3.26

- Fixed the new pairwise route search asking the game's pathfinder about every pair it considered, instead of filtering with a cheap straight-line estimate first.
- The store page now lists the per-visit spend cap alongside the other route caps.

## 1.3.25

- Fixed the planner throwing away a whole item when its single cheapest buy town and dearest sell town were too far apart, instead of trying other pairs.
- Fixed route quantities ignoring the per-visit spend cap, which is usually the cap that runs out first.
- Quick-buy no longer runs the herd calculation when there is no livestock on the shelf to buy.

## 1.3.24

- Fixed a cow being reserved as food ahead of the grain sitting next to it.
- Confirmed in the field that town names fit the panel's columns; no change needed.

## 1.3.23

- Fixed the food reserve counting a cow as one meal when the game counts it as its meat value.
- Fixed the herd guard under-counting the herd in cavalry parties.
- The mod now asks the game's own trade-permission rules instead of working around them.
- Removed the diagnostics, since decompiling answered the remaining questions more completely than playing could.

## 1.3.22

- Fixed the mod trading at villages the game had closed after a raid.
- Fixed the ledger panel taking the whole keyboard, so space no longer paused and the speed keys stopped working while it was open.
- Settled by diagnostic: the food reserve counting items is correct, because the game counts one unit of food as one food whatever the item.
- Settled by diagnostic: the panel hotkey must not be gated on which layer has focus, because that would have disabled it for everyone.
- Removed the map-wide trade-permission diagnostic, which crashed inside the game's own code when asked about arbitrary settlements.

## 1.3.21

- The trade-permission diagnostic now asks about every town and village at session start, instead of only the ones you walk into.
- The focus diagnostic now also logs what had focus at the exact moment the panel hotkey was pressed.

## 1.3.20

- Added a temporary diagnostic recording the party's food numbers once a day, to settle whether the food reserve should count animals by head or by meat.
- Added a temporary diagnostic recording what the game's own rules say about trading at each settlement you enter.
- Added a temporary diagnostic recording which screen layer has focus while the campaign map is up.

## 1.3.19

- The cargo map marker now also updates when you leave a settlement, so it follows what you actually traded rather than what you walked in with.
- The setting's hint now names all three moments the marker updates.

## 1.3.18

- Fixed the per-item denar cap never reaching the route panel, so a route could show 32 units of a good the cap stops at 5.
- The price-coloring hint now mentions livestock and horses, which 1.3.14 added.
- The "Minimum profit margin" hint now says it applies to buying and to the route panel too, not just selling.
- The "Economy settling delay" hint now says it stops the town-menu buttons as well as automation.
- Corrected two stale README lines about tooltips and per-item caps.

## 1.3.17

- Fixed the scan radius never reaching the cargo map marker.
- Fixed the panel listing routes quick-buy would refuse when conservative route projection was switched off.
- The setting's hint now says which half of it changes the display and which half changes behavior.
- Mapped all five market filters against all four market scans, which is how the two fixes above were found.

## 1.3.16

- Fixed "Hold cargo for the best market" checking the price once and then letting the whole stack go, when selling into a market is exactly what pushes its price down.
- Fixed the food reserve letting an item skip the rest of the sell rules.
- The ledger popup now explains why Profit is not simply sell price minus buy price times quantity.
- Checked the panel layout against the code that fills it, and every price lookup for buy or sell direction.

## 1.3.15

- Fixed the food reserve keeping whichever food came first, so it held the wine and sold the grain; it now keeps the cheapest food.
- Livestock is now reserved last, behind every sack of grain.
- Fixed a failure in a per-frame path writing a full stack trace every time, which could have filled the log at hundreds a second.
- "Minimum stock for buy suggestions" now says it only applies in live-price mode.
- Checked all eighteen numeric defaults against their allowed ranges, and the panel legend against its length limit.

## 1.3.14

- Fixed simulation mode ignoring the merchant's gold, so a dry run happily reported selling more than a town could pay for.
- Fixed the trade screen coloring trade goods only, leaving horses and livestock grey even though the tooltip could price them.
- Fixed the panel never proposing a livestock route even though quick-buy would buy livestock.
- Removed the ledger popup's tip telling you to press T for the panel, since that popup only appears when the panel is unavailable.
- Removed a second catch in the panel teardown that nothing could reach.
- Confirmed town and village gold come from one place, the trade-good list is built fresh each time, and the map button sits clear of the map's own mouse area.

## 1.3.13

- Fixed quick-buy spending the per-visit budget in shelf order, so a 12% margin listed first beat a 60% one further down.
- Fixed the cost basis changing halfway through selling one stack, so your margin was silently two different margins in one sale.
- Fixed pressing Done in the settings screen being able to switch your automation off.
- The panel now counts caravan traffic by walking the party list once instead of once per town shown.
- Entering any settlement anywhere no longer costs work for parties that are not yours.

## 1.3.12

- Fixed towns under siege and villages being raided being ranked as ordinary markets and proposed as destinations.
- A village that has been raided and is rebuilding is deliberately still listed, because thin shelves and high food prices are an opportunity rather than a closed market.
- Fixed quick-buy buying items the selling side had just been taught to refuse.

## 1.3.11

- Fixed quick-sell being able to sell a quest item, which silently failed the quest later.
- Fixed the same for items the game marks as not merchandise, such as tournament prizes and banners.
- Fixed the panel not releasing its mouse and keyboard grab when the screen changed while it was open.
- Re-checked settings and translation coverage: 45 settings to 45 controls, and 23 texts to 23 ids. Ensured nothing orphaned either way.

## 1.3.10

- Fixed a settings combination that switched your automation back off on the next load, with nothing said.
- Fixed "Hold cargo for the best market" ignoring livestock.
- Fixed the ledger popup naming the text in the hotkey setting rather than the key the panel actually listens for.
- Fixed the hotkey setting accepting comma-separated text like "T,Y", which left the panel with no working hotkey at all.
- Confirmed the panel's layer name, the inventory lock key and all 23 translated texts are what the code expects.

## 1.3.9

- Fixed observed mode rebuilding its market rankings on every tooltip and every inventory row, instead of once per game hour.
- Entering a market now clears the cached rankings, so the panel and tooltips no longer serve pre-entry answers for the rest of the hour.
- Fixed the panel proposing routes for locked items that quick-buy would refuse to buy.
- The one-line trade summary now names the six goods worth the most denars, not whichever came first in the roster.
- The startup log no longer claims MCM was registered when the companion file was from a different version.
- Fixed an empty or truncated item-list setting throwing an error and silently stopping a whole trade pass.
- Re-measured the panel layout and wrote the numbers down.

## 1.3.8

- Fixed "Keep food (days of supply)" not covering livestock, so a herder could enter a town with auto-sell on and leave with the whole herd sold and nothing to eat.
- Fixed quick-buy buying goods you had locked in the inventory.
- Fixed "Suppress vanilla trade-rumor lines" still hiding vanilla's hints when TradeLord had no prices to show in their place.
- Quick-buy now stops scanning once the budget is spent, instead of pricing the rest of the shelf first.
- Re-checked that every game method the mod hooks into is still where it expects.

## 1.3.7

- Changed the cargo marker's default travel ceiling from 1 day to 1.5 days, because 1 day rarely had anything to point at.

## 1.3.6

- Fixed quick-buy buying goods quick-sell would never sell, such as smithing materials, which then sat in the cargo forever.
- Fixed "Suppress vanilla trade-rumor lines" hiding vanilla's prices even when TradeLord's own tooltip section was switched off, leaving no price information at all.
- Fixed simulation mode writing its per-item buy caps back to the visit record.
- The cargo marker now values only what quick-sell would actually put on the counter, not locked items, never-sell entries and reserved food.
- Fixed the panel not giving up input focus when its layer was released, which could break input after an encounter on the map.
- A trade pass now reports itself in one line instead of eight, by silencing the game's per-chunk messages for the length of the pass.
- Changed the default: "Protect smithing materials" is now off, since it governs buying as well as selling.

## 1.3.5

- Fixed battle loot, companion transfers and stash moves being recorded as purchases at full market price, which then made the profit margin refuse perfectly good sales.
- Fixed the per-visit trade counters carrying over into the next campaign you loaded in the same session.
- Entering a market now captures prices once per game hour instead of three times per entry.
- The market scan now builds its list of reachable markets once per game hour and shares it, instead of rebuilding it for every item and direction.
- Fixed a sale that paid no gold still handing the goods over and draining the cost record.
- Fixed turning off "Detailed trade summary" also stopping the full item list reaching the log, which its own description promises.
- The log now starts fresh each launch and writes the version banner first, instead of growing forever.
- The panel no longer proposes routes for items on your never-sell list.
- A single failed panel setup no longer disables the panel for the rest of the session.
- The town-to-town distance cache stops building a throwaway text key on every lookup.
- Fixed the panel hotkey firing while the escape menu was open.
- The cargo marker now refreshes on every settlement entry, not only when entry automation is switched on.
- The travel distance caches are now cleared with the campaign, like everything else.
- "Quick-sell option in town menu" now explains that it also hides the quick-trade entry.
- Checked that every setting is reachable from the settings screen and starts inside its allowed range.

## 1.3.4

- Fixed the two per-item buying caps resetting on every quick-buy click instead of lasting the whole visit, so clicking twice bought twice the cap.
- Simulation mode now previews against whatever the caps have left and puts them back where it found them.
- The village last-unit rule now measures against the stock left on this pass rather than the opening shelf count.

## 1.3.3

- Fixed the panel's legend line being far too long for its row, so half of it was invisible, including the note explaining the Profit column.
- Fixed the panel's "no routes" message being cut off, so the advice a new player needs was hidden.

## 1.3.2

- Fixed smithing protection silently switching off in the second campaign of a session.
- Fixed the food reserve being kept per food type instead of in total, which held roughly five times as much food as the setting said.
- Fixed "Exclude hostile markets" doing nothing when turned off, because trading with hostile towns was blocked either way.
- Fixed the scan radius being applied in live-price mode and ignored in observed mode.
- Fixed auto-trade reading as ON when quick-buy was off, which left the town menu with nothing in it at all.
- Fixed profit and Trade XP on livestock you had bought being reported at the full sale price instead of the margin.
- Fixed a purchase that moved no gold still being written to the cost record, which permanently dragged that item's cost basis toward zero.
- Fixed the panel only being able to unpin the most recent town it pinned, stranding every earlier pin on the map.
- Fixed the daily cargo marker removing a marker you had pinned by hand.
- Fixed simulation mode ignoring the rule that stops it selling a good and buying it straight back at the same counter.
- Fixed simulation mode ignoring carry weight, so it reported buying more than the party could hold.
- Observed mode now looks settlements up directly instead of scanning every settlement on the map for every tooltip row.
- The price cache stops building a throwaway text key on every lookup.
- Leaving a campaign no longer keeps the previous campaign's settlements, parties and prices in memory.
- The map panel is rebuilt whenever the map screen is replaced, and logs itself once per session instead of once per inventory screen you close.
- The never-sell, always-sell and never-buy lists now match item ids regardless of capitalization.
- Internal tidying: the duplicated market-settling check became one, and an unused connector class was removed.

## 1.3.1

- Automatic passes on entering a town are silent when nothing traded, instead of reporting "bought 0 items for 0 denars".
- Observed mode now respects the trade-with-villages setting exactly as live mode does.
- The panel hotkey is read once instead of being re-read every frame.
- Removed dead code and all source comments.
- Releases now publish automatically when a push carries a version that has no release yet.
- A push for a version that is already released now skips publishing instead of failing the build.

## 1.3.0

- Grain is no longer bought by default, because it is heavy and low margin and clogged the cargo; a general never-buy list was added alongside it.
- Livestock now trades both ways by default, with buying capped by the game's own herd math so a purchase can never push the party into the herd speed penalty.
- Changed the defaults to a trader-ready set: quick-buy, auto-sell, auto-buy and the cargo marker all on, gold reserve 100.
- The cargo map marker now ignores towns beyond a travel ceiling, 1 day by default.
- Quick-buy never takes a village's last unit of anything, so the vanilla "buy products" option always has stock to show.
- The map button now reads "TradeLord".

## 1.2.0

- Fixed the map button holding your mouse on its invisible layer, which gave a permanent "forbidden" cursor and a stuttering right-drag camera on the world map.
- Trade summaries now name the goods, for example "Sold 8 Olives for 240 denars".
- Added auto-buy on entry, and grouped auto-sell, auto-buy and auto-trade together in the settings.
- Routes must now clear your minimum profit margin after the resale safety factor, so razor-thin routes are no longer listed.

## 1.1.0

- Added quick-trade, one menu option that sells and then buys in a single pass.
- Added livestock selling, off by default; mounts and pack animals are never sold by policy.
- Added a TradeLord button on the right edge of the campaign map that opens the ledger panel.
- Added a workshop tracker, a caravan-traffic column and a data-age column to the panel.
- Observed mode now records the horses and livestock a shelf actually holds, so animal tooltips work without live prices.
- Fixed three things from the 1.0 review: settings changes now clear the price cache immediately, the panel unpins only markers it placed itself, and a number typed as the panel hotkey no longer maps to an arbitrary key.

## 1.0.0

- First release: a price ledger saved with your campaign, best buy and sell prices in item tooltips, a route panel on the campaign map, and quick-sell and quick-buy from the town and village menus.

---

The versions below are the earlier test builds, from the two repositories this one replaced. They were all numbered 1.0 at the time; the numbers here were assigned afterwards to put them in order.

## 0.916Alpha

- The release zip now unpacks straight into the game folder, the layout mod managers expect.
- Updated the install instructions in the README and the release notes to match.

## 0.915Alpha

- Clicking a town in the ledger panel now pins a map marker on it, and clicking again removes it.
- The automatic best-sell-town marker is now off by default, because it hopped between towns with no visible reason.
- Lowered the default buy cap per item from 50 to 32.
- Capped the default spend per visit at 1000 denars, instead of leaving it unlimited.
- Documented how versions are numbered and why releases are never replaced.

## 0.914Alpha

- The version number now lives in one place, and everything else reads it from there.
- Releases became permanent, and the build refuses to publish over one that already exists.
- Added the first changelog, with an entry for every release so far.

## 0.913Alpha

- Travel days now follow real routes instead of straight lines, so a town across a sea no longer reads as a short trip.
- Travel results are cached, so tooltips stay quick to hover.

## 0.912Alpha

- Added travel ceilings, so a market more than three days away, or a village more than one, is no longer offered.
- Both ceilings can be turned off or changed in the settings.
- When two markets tie on price, the nearer one now ranks first.
- Empty lists now name the ceiling that emptied them.
- Added the store description and a feature comparison page alongside the README.
- Brought the README in line with those two pages.

## 0.911Alpha

- Corrected the documentation's account of the research behind the mod.
- Narrowed the "zero hardcoded" claim to price data.
- Rewrote the documentation to say plainly what was researched and what was written from scratch.
- Brought the rest of the documentation back in line with what the mod actually does.
- Marked the mod field-proven in the README, after the first live playthroughs.
- Verdict lines now use a star the game's font can draw.

## 0.910Alpha

- Removed the compass arrows from tooltips because the game's font could not draw them; rows now show travel days and price only.
- Documented a base-game problem that is not TradeLord's and was reproduced with TradeLord disabled: if you enable or remove any module mid-campaign, the MCM stack included, the launcher and main-menu Continue button can hang the game on a save that was made under the old mod list. The save itself is not damaged. Load it once from the Saved Games list instead, click through the module-mismatch warning the game shows, and save again; Continue is safe from that point on.

## 0.909Alpha

- Fixed every colour in the panel being ignored, because the wrong property name was used throughout.

## 0.908Alpha

- Fixed the panel rendering upside down, with the title at the bottom and the worst routes first.
- Column headers are now gold and underlined, so they never read as an entry.
- The profit-per-day cell now carries a green-to-orange gradient across the visible list.
- Renamed the panel's "Best" column to "Profit per day".

## 0.907Alpha

- Route proposals are now capped by the sell town's own gold, so a bulk run no longer promises profit the buyer cannot pay.

## 0.906Alpha

- Removed the diagnostic log lines now that a live game has answered every open question.
- The mod now refuses to touch MCM unless the whole stack is installed, and names the missing piece.
- "Consult the TradeLord ledger" in the town menu now opens the map panel.

## 0.905Alpha

- Added the ledger as a panel on the campaign map, opened by hotkey, with sortable routes and clickable towns.
- Item tooltips now list the top five buy and sell markets, with profit, stock and travel days.
- The route popup now says its day counts are measured from where your party is.
- The panel's artwork now ships inside the release zip.

## 0.904Alpha

- The first successful sale and buy now write a confirmation line to the log.
- The XP line now says its number is profit in denars, not raw XP.

## 0.903Alpha

- Fixed the route report saying there were no profitable routes to anyone whose gold was below the reserve.
- Tooltips now use the game's own coin icon instead of a bare "d".
- Tooltips now spell out stock, denars and days.
- The startup log now reads the game version from the game itself, instead of trusting the number stamped on its files.
- Confirmed that a sale can never be recorded or paid Trade XP twice.

## 0.902Alpha

- Fixed the mod refusing to load at all on any install without MCM.
- The settings menu now lives in its own file, loaded only when MCM is there.
- Removed the launcher's version warning.
- Matched Harmony to the version the game's own Harmony module ships.

## 0.901Alpha

- The mod now writes plain answers to its log for the questions only a live game can settle.
- Added a guard that stops a trade if gold ever moves the wrong way.

## 0.900Alpha

- First test build: a price ledger saved with your campaign, best buy and sell prices in item tooltips, and quick-sell and quick-buy from the town and village menus.
- Added the build that compiles the mod and packs the zip on every push, and publishes a release on demand.
- Fixed a formatting mistake that broke the release notes step in that build.
