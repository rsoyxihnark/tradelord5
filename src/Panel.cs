using System;
using System.Collections.Generic;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace TradeLord
{
    public class RouteRowVM : ViewModel
    {
        private readonly TradeRoute _route;
        private readonly Action<Settlement> _centerMap;
        private readonly float _rank;

        public RouteRowVM(TradeRoute route, bool alternate, float rank, Action<Settlement> centerMap)
        {
            _route = route;
            _centerMap = centerMap;
            _rank = rank;
            IsAlternateRow = alternate;
        }

        [DataSourceProperty] public bool IsAlternateRow { get; }

        [DataSourceProperty] public bool Tier1 => Ranks.BandOf(_rank) == 1;
        [DataSourceProperty] public bool Tier2 => Ranks.BandOf(_rank) == 2;
        [DataSourceProperty] public bool Tier3 => Ranks.BandOf(_rank) == 3;
        [DataSourceProperty] public bool Tier4 => Ranks.BandOf(_rank) == 4;
        [DataSourceProperty] public bool Tier5 => Ranks.BandOf(_rank) == 5;
        [DataSourceProperty] public string ItemName => _route.Item.Name.ToString();
        [DataSourceProperty] public string BuyTownName => _route.From.Name.ToString();
        [DataSourceProperty] public string BuyPrice => _route.BuyPrice.ToString();
        [DataSourceProperty] public string SellTownName => _route.To.Name.ToString();
        [DataSourceProperty] public string SellPrice => _route.SellPrice.ToString();
        [DataSourceProperty] public string Quantity =>
            "x" + _route.Quantity + (_route.StillComing ? "!" : "");
        [DataSourceProperty] public string Profit => "+" + _route.TotalProfit;
        [DataSourceProperty] public string Days => "~" + _route.TravelDays.ToString("0.#");

        [DataSourceProperty] public string RunsOut
        {
            get
            {
                float days = _route.RunsOutInDays;
                if (days <= 0f) return "";
                float hours = TradeMath.HoursOf(days);
                return hours < 48f
                    ? (int)Math.Round(hours) + Tongue.Text("{=TL414}h").ToString()
                    : days.ToString("0.#") + Tongue.Text("{=TL415}d").ToString();
            }
        }

        [DataSourceProperty] public string Caravans => _route.Caravans.ToString();

        [DataSourceProperty] public string Confidence =>
            (int)Math.Round(_route.Confidence * 100f) + (_route.Simulated ? "%" : "%*");

        [DataSourceProperty] public string Score =>
            ((int)(Options.Current.ConfidenceRanking ? _route.Score : _route.ProfitPerDay)).ToString();

        public void ExecuteClickBuyTown() => _centerMap?.Invoke(_route.From);
        public void ExecuteClickSellTown() => _centerMap?.Invoke(_route.To);
    }

    public class WorkshopRowVM : ViewModel
    {
        public WorkshopRowVM(string name, string profit, string makes, string owner)
        {
            Name = name; Profit = profit; Makes = makes; Owner = owner;
        }

        [DataSourceProperty] public string Name { get; }
        [DataSourceProperty] public string Profit { get; }
        [DataSourceProperty] public string Makes { get; }
        [DataSourceProperty] public string Owner { get; }
    }

    public class TradeRowVM : ViewModel
    {
        public TradeRowVM(string when, string where, string what, string gold, bool gained)
        {
            When = when;
            Where = where;
            What = what;
            Gold = gold;
            Gained = gained;
        }

        [DataSourceProperty] public string When { get; }
        [DataSourceProperty] public string Where { get; }
        [DataSourceProperty] public string What { get; }
        [DataSourceProperty] public string Gold { get; }
        [DataSourceProperty] public bool Gained { get; }
        [DataSourceProperty] public bool Spent => !Gained;
    }

    public class ShopOfferRowVM : ViewModel
    {
        private readonly Workshop _shop;
        private readonly Action _bought;
        private readonly int _cost;

        public ShopOfferRowVM(Workshop shop, int cost, bool affordable, Action bought)
        {
            _shop = shop;
            _bought = bought;
            _cost = cost;
            Where = shop.Settlement?.Name.ToString() ?? "";
            What = shop.WorkshopType?.Name.ToString() ?? "";
            Owner = shop.Owner?.Name.ToString() ?? "";
            Profit = (shop.ProfitMade >= 0 ? "+" : "") + shop.ProfitMade;
            Cost = cost.ToString("N0");
            Affordable = affordable;
        }

        [DataSourceProperty] public string Where { get; }
        [DataSourceProperty] public string What { get; }
        [DataSourceProperty] public string Owner { get; }
        [DataSourceProperty] public string Profit { get; }
        [DataSourceProperty] public string Cost { get; }
        [DataSourceProperty] public bool Affordable { get; }
        [DataSourceProperty] public bool Dear => !Affordable;
        [DataSourceProperty] public string BuyLabel => Tongue.Text("{=TL433}Buy").ToString();

        public void ExecuteBuy() => Guard.Run("Panel.BuyWorkshop", () =>
        {
            TextObject asked = Tongue.Text("{=TL438}Buy the {SHOP} in {TOWN} from {OWNER} for {GOLD} denars?");
            asked.SetTextVariable("SHOP", What);
            asked.SetTextVariable("TOWN", Where);
            asked.SetTextVariable("OWNER", Owner);
            asked.SetTextVariable("GOLD", Cost);
            string body = asked.ToString();
            int heldBack = TradeActionBehavior.GoldHeldBack();
            if (Holdings.DipsIntoWhatYouHoldBack(_cost, Hero.MainHero?.Gold ?? 0, heldBack))
            {
                TextObject warned = Tongue.Text("{=TL441} This takes you below the {HELD} denars TradeLord holds back as your gold reserve and wage cover, which it will not spend on goods.");
                warned.SetTextVariable("HELD", heldBack.ToString("N0"));
                body += warned.ToString();
            }
            InformationManager.ShowInquiry(new InquiryData(
                Tongue.Text("{=TL431}Workshops for sale").ToString(), body,
                true, true, Tongue.Text("{=TL433}Buy").ToString(),
                Tongue.Text("{=TL09}Close").ToString(),
                () => Guard.Run("Panel.BuyWorkshopTaken", Take), null));
        });

        private void Take()
        {
            if (Shops.Buy(_shop, out TextObject said) && said != null)
                InformationManager.DisplayMessage(new InformationMessage(said.ToString(), Told));
            else if (said != null)
                InformationManager.DisplayMessage(new InformationMessage(said.ToString(), Refused));
            _bought?.Invoke();
        }

        private static readonly Color Told = new Color(0.40f, 0.90f, 0.40f);
        private static readonly Color Refused = new Color(0.90f, 0.28f, 0.28f);
    }

    public class LedgerPanelVM : ViewModel
    {
        private readonly Action _onClose;
        private readonly Action _onOpen;
        private readonly Action<Settlement> _centerMap;
        private bool _isVisible;
        private bool _isTradesVisible;
        private bool _isLegendVisible;
        private bool _isShopsVisible;
        private bool _isMapButtonVisible;
        private string _playerGold = "";
        private string _capacityText = "";
        private string _speedText = "";
        private string _lifetimeText = "";
        private string _statusText = "";
        private string _legendText = "";
        private string _workshopsHeader = "";
        private MBBindingList<RouteRowVM> _routes = new MBBindingList<RouteRowVM>();
        private MBBindingList<WorkshopRowVM> _workshops = new MBBindingList<WorkshopRowVM>();
        private MBBindingList<TradeRowVM> _trades = new MBBindingList<TradeRowVM>();
        private MBBindingList<ShopOfferRowVM> _shops = new MBBindingList<ShopOfferRowVM>();
        private string _tradesHeader = "";
        private string _shopsHeader = "";

        public LedgerPanelVM(Action onClose, Action onOpen, Action<Settlement> centerMap)
        {
            _onClose = onClose;
            _onOpen = onOpen;
            _centerMap = centerMap;
            _isMapButtonVisible = Options.Current.ShowMapButton;
        }

        [DataSourceProperty]
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (value != _isVisible) { _isVisible = value; OnPropertyChangedWithValue(value, "IsVisible"); }
                if (!value) { IsTradesVisible = false; IsLegendVisible = false; IsShopsVisible = false; }
                IsMapButtonVisible = Options.Current.ShowMapButton && !value;
            }
        }

        [DataSourceProperty]
        public bool IsTradesVisible
        {
            get => _isTradesVisible;
            set { if (value != _isTradesVisible) { _isTradesVisible = value; OnPropertyChangedWithValue(value, "IsTradesVisible"); } }
        }

        [DataSourceProperty]
        public bool IsLegendVisible
        {
            get => _isLegendVisible;
            set { if (value != _isLegendVisible) { _isLegendVisible = value; OnPropertyChangedWithValue(value, "IsLegendVisible"); } }
        }

        [DataSourceProperty]
        public bool IsShopsVisible
        {
            get => _isShopsVisible;
            set { if (value != _isShopsVisible) { _isShopsVisible = value; OnPropertyChangedWithValue(value, "IsShopsVisible"); } }
        }

        [DataSourceProperty]
        public MBBindingList<ShopOfferRowVM> Shops
        {
            get => _shops;
            set { if (value != _shops) { _shops = value; OnPropertyChangedWithValue(value, "Shops"); } }
        }

        [DataSourceProperty]
        public string ShopsHeader
        {
            get => _shopsHeader;
            set { if (value != _shopsHeader) { _shopsHeader = value; OnPropertyChangedWithValue(value, "ShopsHeader"); } }
        }

        [DataSourceProperty]
        public bool IsMapButtonVisible
        {
            get => _isMapButtonVisible;
            set { if (value != _isMapButtonVisible) { _isMapButtonVisible = value; OnPropertyChangedWithValue(value, "IsMapButtonVisible"); } }
        }

        [DataSourceProperty]
        public string PlayerGold
        {
            get => _playerGold;
            set { if (value != _playerGold) { _playerGold = value; OnPropertyChangedWithValue(value, "PlayerGold"); } }
        }

        [DataSourceProperty]
        public string CapacityText
        {
            get => _capacityText;
            set { if (value != _capacityText) { _capacityText = value; OnPropertyChangedWithValue(value, "CapacityText"); } }
        }

        [DataSourceProperty]
        public string SpeedText
        {
            get => _speedText;
            set { if (value != _speedText) { _speedText = value; OnPropertyChangedWithValue(value, "SpeedText"); } }
        }

        [DataSourceProperty]
        public string LifetimeText
        {
            get => _lifetimeText;
            set { if (value != _lifetimeText) { _lifetimeText = value; OnPropertyChangedWithValue(value, "LifetimeText"); } }
        }

        [DataSourceProperty]
        public string StatusText
        {
            get => _statusText;
            set { if (value != _statusText) { _statusText = value; OnPropertyChangedWithValue(value, "StatusText"); } }
        }

        [DataSourceProperty]
        public string LegendText
        {
            get => _legendText;
            set { if (value != _legendText) { _legendText = value; OnPropertyChangedWithValue(value, "LegendText"); } }
        }

        [DataSourceProperty]
        public MBBindingList<RouteRowVM> Routes
        {
            get => _routes;
            set { if (value != _routes) { _routes = value; OnPropertyChangedWithValue(value, "Routes"); } }
        }

        [DataSourceProperty]
        public MBBindingList<WorkshopRowVM> Workshops
        {
            get => _workshops;
            set { if (value != _workshops) { _workshops = value; OnPropertyChangedWithValue(value, "Workshops"); } }
        }

        [DataSourceProperty]
        public MBBindingList<TradeRowVM> Trades
        {
            get => _trades;
            set { if (value != _trades) { _trades = value; OnPropertyChangedWithValue(value, "Trades"); } }
        }

        [DataSourceProperty]
        public string TradesHeader
        {
            get => _tradesHeader;
            set { if (value != _tradesHeader) { _tradesHeader = value; OnPropertyChangedWithValue(value, "TradesHeader"); } }
        }

        [DataSourceProperty] public string BrandLabel => "TradeLord";
        [DataSourceProperty] public string TitleLabel => Tongue.Text("{=TL07}TradeLord ledger").ToString();
        [DataSourceProperty] public string RefreshLabel => Tongue.Text("{=TL62}Refresh").ToString();
        [DataSourceProperty] public string TradesLabel => Tongue.Text("{=TL424}Recent trades").ToString();
        [DataSourceProperty] public string LegendLabel => Tongue.Text("{=TL432}What this means").ToString();
        [DataSourceProperty] public string ShopsLabel => Tongue.Text("{=TL431}Workshops for sale").ToString();
        [DataSourceProperty] public string CloseLabel => Tongue.Text("{=TL09}Close").ToString();
        [DataSourceProperty] public string HeadItem => Tongue.Text("{=TL50}Item").ToString();
        [DataSourceProperty] public string HeadBuyTown => Tongue.Text("{=TL51}Buy From").ToString();
        [DataSourceProperty] public string HeadPrice => Tongue.Text("{=TL52}Price").ToString();
        [DataSourceProperty] public string HeadSellTown => Tongue.Text("{=TL53}Sell At").ToString();
        [DataSourceProperty] public string HeadQuantity => Tongue.Text("{=TL54}Qty").ToString();
        [DataSourceProperty] public string HeadProfit => Tongue.Text("{=TL55}Profit").ToString();
        [DataSourceProperty] public string HeadDays => Tongue.Text("{=TL56}Days").ToString();
        [DataSourceProperty] public string HeadRunsOut => Tongue.Text("{=TL416}Left").ToString();
        [DataSourceProperty] public string HeadCaravans => Tongue.Text("{=TL58}Carv.").ToString();
        [DataSourceProperty] public string HeadConfidence => Tongue.Text("{=TL59}Conf").ToString();
        [DataSourceProperty] public string HeadScore => Tongue.Text("{=TL60}Score").ToString();
        [DataSourceProperty]
        public string WorkshopsHeader
        {
            get => _workshopsHeader;
            set { if (value != _workshopsHeader) { _workshopsHeader = value; OnPropertyChangedWithValue(value, "WorkshopsHeader"); } }
        }

        public void Show()
        {
            Refresh();
            IsVisible = true;
        }

        public void Hide() => IsVisible = false;

        public void ExecuteClose() => _onClose?.Invoke();

        public void ExecuteOpenTrades() => Guard.Run("Panel.OpenTrades", () =>
        {
            RefreshTrades();
            IsTradesVisible = true;
        });

        public void ExecuteCloseTrades() => IsTradesVisible = false;

        public void ExecuteOpenLegend() => IsLegendVisible = true;

        public void ExecuteCloseLegend() => IsLegendVisible = false;

        public void ExecuteOpenShops() => Guard.Run("Panel.OpenShops", () =>
        {
            RefreshShops();
            IsShopsVisible = true;
        });

        public void ExecuteCloseShops() => IsShopsVisible = false;

        public void ExecuteOpenPanel() => _onOpen?.Invoke();

        public void ExecuteRefresh() => Guard.Run("Panel.Refresh", () =>
        {
            LedgerBehavior.Instance?.ForgetMarketRankings();
            Refresh();
        });

        private static string Line(string template, string key, string value)
        {
            TextObject t = Tongue.Text(template);
            t.SetTextVariable(key, value);
            return t.ToString();
        }

        private static string Line(string template, string k1, string v1, string k2, string v2)
        {
            TextObject t = Tongue.Text(template);
            t.SetTextVariable(k1, v1);
            t.SetTextVariable(k2, v2);
            return t.ToString();
        }

        private static readonly string[] SpokenLabels =
        {
            "BrandLabel", "TitleLabel", "RefreshLabel", "TradesLabel", "LegendLabel", "ShopsLabel",
            "CloseLabel", "HeadItem", "HeadBuyTown",
            "HeadPrice", "HeadSellTown", "HeadQuantity", "HeadProfit", "HeadDays", "HeadRunsOut",
            "HeadCaravans", "HeadConfidence", "HeadScore"
        };

        internal void Respeak() => Refresh();

        private void Refresh()
        {
            for (int i = 0; i < SpokenLabels.Length; i++) OnPropertyChanged(SpokenLabels[i]);
            var hero = Hero.MainHero;
            var party = MobileParty.MainParty;
            PlayerGold = Line("{=TL63}Gold: {AMOUNT}", "AMOUNT", (hero?.Gold ?? 0).ToString("N0"));
            CapacityText = party == null ? ""
                : Line("{=TL64}Cargo: {CARRIED} / {CAPACITY}", "CARRIED",
                       ((int)Carry.Carried(party)).ToString(), "CAPACITY",
                       ((int)Carry.Capacity(party)).ToString());
            SpeedText = party == null ? "" : Line("{=TL65}Speed: {SPEED}", "SPEED", party.Speed.ToString("0.0"));
            long lifetime = LedgerBehavior.Instance?.LifetimeProfit ?? 0L;
            LifetimeText = Line("{=TL66}TradeLord profit: {AMOUNT}", "AMOUNT",
                                (lifetime >= 0 ? "+" : "") + lifetime.ToString("N0"));

            var rows = new MBBindingList<RouteRowVM>();
            var routes = LedgerBehavior.Instance?.BestRoutes(30);
            if (routes != null)
                for (int i = 0; i < routes.Count; i++)
                    rows.Add(new RouteRowVM(routes[i], i % 2 == 1,
                        Ranks.Of(i, routes.Count), _centerMap));
            Routes = rows;
            bool empty = rows.Count == 0;
            StatusText = empty
                ? Tongue.Text("{=TL67}No profitable routes in reach").ToString()
                : Line("{=TL68}{COUNT} profitable routes, best first", "COUNT", rows.Count.ToString());
            LegendText = (empty
                ? Tongue.Text(Options.Current.Omniscient
                    ? "{=TL69}No routes are within your travel ceilings. Raise the ceilings in the Knowledge settings, or move nearer to more markets."
                    : "{=TL90}No routes are within your travel ceilings, from the prices you have recorded so far. Walk more markets, or raise the ceilings in the Knowledge settings.").ToString()
                : Tongue.Text("{=TL70}Click a town name to jump to it and pin or unpin it | Days = you -> buy town -> sell town | Carv. = caravans at those towns | Price is the first unit's; Profit prices every unit in turn, so it is less than price x qty | Conf* = flat quote, not priced per unit").ToString()
                  + (Options.Current.ConfidenceRanking
                        ? Tongue.Text("{=TL71} | Score = profit per day discounted by Conf").ToString()
                        : Tongue.Text("{=TL72} | Score = profit per day").ToString())
                  + (Options.Current.ConservativeRouteProjection
                        ? Tongue.Text("{=TL73} | resale safety factor applied").ToString() : "")
                  + (Forecast.On
                        ? Tongue.Text("{=TL394} | prices and stock count what the caravans and the workshops will add to a market before you arrive, less an estimate of what the caravans' purses will buy there").ToString()
                          + Tongue.Text("{=TL397} | Qty! = part of that amount is still on the road and lands before you would").ToString()
                          + Tongue.Text("{=TL417} | Left = how long that shelf still holds this Qty once you arrive, and a shelf that empties first lowers Conf").ToString()
                        : ""))
                + HowThePromiseHasHeld()
                + (TradeActionBehavior.PurseForAVisit() > 0 ? "" : " | " + NothingHereYouCouldBuy(hero));
            LegendText = OneClauseToALine(LegendText);

            RefreshWorkshops();
            RefreshTrades();
        }

        private void RefreshShops()
        {
            var offers = Shops_OnOffer();
            int purse = Hero.MainHero?.Gold ?? 0;
            int owned = TradeLord.Shops.Owned(), mayOwn = TradeLord.Shops.MayOwn();
            var rows = new MBBindingList<ShopOfferRowVM>();
            for (int i = 0; i < offers.Count; i++)
            {
                Workshop shop = offers[i];
                int cost = TradeLord.Shops.CostOf(shop);
                if (cost <= 0) continue;
                rows.Add(new ShopOfferRowVM(shop, cost, cost <= purse && owned < mayOwn,
                                            () => Guard.Run("Panel.ShopsAgain", RefreshShops)));
            }
            Shops = rows;
            if (rows.Count == 0)
            {
                ShopsHeader = Tongue.Text("{=TL440}No workshop in reach is a notable's to sell").ToString();
                return;
            }
            TextObject head = Tongue.Text("{=TL439}Workshops for sale: {COUNT}, and you own {OWNED} of {MOST}");
            head.SetTextVariable("COUNT", rows.Count.ToString());
            head.SetTextVariable("OWNED", owned.ToString());
            head.SetTextVariable("MOST", mayOwn.ToString());
            ShopsHeader = head.ToString();
        }

        private static List<Workshop> Shops_OnOffer() => TradeLord.Shops.OnOffer();

        private void RefreshTrades()
        {
            var lately = LedgerBehavior.Instance?.Lately;
            int count = lately?.Count ?? 0;
            TradesHeader = count == 0
                ? Tongue.Text("{=TL419}Recent trades: nothing traded yet this campaign").ToString()
                : Line("{=TL418}Recent trades (last {COUNT})", "COUNT", count.ToString());
            var rows = new MBBindingList<TradeRowVM>();
            for (int i = 0; i < count; i++)
            {
                TradeNote note = lately[i];
                rows.Add(new TradeRowVM(DayOf(note.Day), note.Where, note.What,
                                        Recent.Coins(note.Gold), note.Gold > 0));
            }
            Trades = rows;
        }

        private static string DayOf(float day) =>
            Line("{=TL420}Day {DAY}", "DAY", ((int)day).ToString("N0"));

        private static string OneClauseToALine(string said) =>
            said == null ? "" : said.Replace(" | ", "\n");

        private static string HowThePromiseHasHeld()
        {
            LedgerBehavior ledger = LedgerBehavior.Instance;
            if (ledger == null || !ledger.PromiseScore(out int arrivals, out float held)) return "";
            TextObject line = Tongue.Text("{=TL399} | the Sell price has held at {HELD} of what this panel promised, over {COUNT} arrival(s) so far");
            line.SetTextVariable("HELD", ((int)Math.Round(held * 100f)).ToString() + "%");
            line.SetTextVariable("COUNT", arrivals.ToString());
            return line.ToString();
        }

        private static string NothingHereYouCouldBuy(Hero hero)
        {
            int held = TradeActionBehavior.GoldHeldBack(), flat = Options.Current.GoldReserve;
            TextObject line = Tongue.Text(held > flat
                ? "{=TL393}Your purse is at {GOLD} denars and TradeLord holds {RESERVE} of it back, {FLAT} for Gold reserve and {WAGES} for Keep gold for days of wages, so there is nothing here you could buy. Sell some cargo, or lower either of those in its settings."
                : "{=TL377}Your purse is at {GOLD} denars and your gold reserve holds {RESERVE} of it back, so there is nothing here you could buy. Sell some cargo, or lower Gold reserve in its settings.");
            line.SetTextVariable("GOLD", (hero?.Gold ?? 0).ToString("N0"));
            line.SetTextVariable("RESERVE", held.ToString("N0"));
            line.SetTextVariable("FLAT", flat.ToString("N0"));
            line.SetTextVariable("WAGES", (held - flat).ToString("N0"));
            return line.ToString();
        }

        private void RefreshWorkshops()
        {
            bool ownedOnly = !Options.Current.Omniscient;
            WorkshopsHeader = ownedOnly
                ? Tongue.Text("{=TL80}Your workshops (recent profit)").ToString()
                : Tongue.Text("{=TL61}Most profitable workshops (recent profit)").ToString();
            if (Forecast.On)
                WorkshopsHeader += Tongue.Text("{=TL395} and what each will make next").ToString();
            var best = new List<Workshop>();
            foreach (Town town in Town.AllTowns)
            {
                Workshop[] shops = town.Workshops;
                if (shops == null) continue;
                for (int i = 0; i < shops.Length; i++)
                {
                    Workshop w = shops[i];
                    if (w?.WorkshopType == null) continue;
                    if (ownedOnly && w.Owner != Hero.MainHero) continue;
                    best.Add(w);
                }
            }
            best.Sort((x, y) => y.ProfitMade.CompareTo(x.ProfitMade));
            var rows = new MBBindingList<WorkshopRowVM>();
            for (int i = 0; i < best.Count && i < 5; i++)
            {
                Workshop w = best[i];
                rows.Add(new WorkshopRowVM(
                    w.WorkshopType.Name + " - " + (w.Settlement?.Name.ToString() ?? "?"),
                    (w.ProfitMade >= 0 ? "+" : "") + w.ProfitMade,
                    Forecast.WillMake(w),
                    w.Owner?.Name.ToString() ?? ""));
            }
            Workshops = rows;
        }
    }

    internal static class LedgerPanel
    {
        private static MapScreen _mapScreen;
        private static GauntletLayer _layer;
        private static GauntletMovieIdentifier _movie;
        private static LedgerPanelVM _vm;
        private static int _setupFailures;
        private static int _setupCooldown;
        private const int SetupAttempts = 3;
        private const int SetupCooldownTicks = 120;
        private static bool _loggedArmed;
        private static int _spokenFor = -1;
        private static bool _dead;

        private static readonly HashSet<Settlement> _panelPins = new HashSet<Settlement>();

        internal static void Tick()
        {
            if (_dead) return;
            try { TickCore(); }
            catch (Exception e)
            {
                _dead = true;
                Log.Error(e, "ledger panel tick (panel disabled; town-menu popup unaffected)");
                try { Cleanup(); } catch { }
            }
        }

        private static void TickCore()
        {
            ScreenBase top = ScreenManager.TopScreen;
            MapScreen map = top as MapScreen;

            if (map != null && _mapScreen == null && Campaign.Current != null && MaySetUp())
            {
                try { Setup(map); }
                catch (Exception e)
                {
                    _setupFailures++;
                    _setupCooldown = SetupCooldownTicks;
                    Log.Error(e, _setupFailures < SetupAttempts
                        ? "ledger panel setup (attempt " + _setupFailures + ", retrying shortly)"
                        : "ledger panel setup (panel disabled for this campaign; the town-menu popup still works)");
                    Cleanup();
                }
            }
            if (_mapScreen != null && map != null && map != _mapScreen)
                Cleanup();

            if (_layer == null || _vm == null || map == null) return;

            if (_spokenFor != Options.Current.Language)
            {
                _spokenFor = Options.Current.Language;
                Guard.Run("Panel.Respeak", _vm.Respeak);
            }

            if (!_vm.IsVisible)
            {
                bool button = Options.Current.ShowMapButton;
                if (_vm.IsMapButtonVisible != button)
                    _vm.IsMapButtonVisible = button;
                UpdateIdleInput(button);
                if (!map.IsEscapeMenuOpened && HotkeyReleased() && !TypingOnScreen(map))
                    Guard.Run("Panel.Show", Show);
            }
            else if (map.IsEscapeMenuOpened || (HotkeyReleased() && !TypingOnScreen(map)))
            {
                Hide();
            }
        }

        private static bool TypingOnScreen(ScreenBase screen)
        {
            try
            {
                MBReadOnlyList<ScreenLayer> layers = screen?.Layers;
                for (int i = 0; layers != null && i < layers.Count; i++)
                    if (layers[i] != null && layers[i].IsFocusedOnInput()) return true;
            }
            catch (Exception e) { Log.Error(e, "panel hotkey text-field check (hotkey left as it was)"); }
            return false;
        }

        private static bool _idleMouseActive;
        private static Widget _mapButton;
        private const string MapButtonId = "TradeLordMapButton";
        private static bool _loggedButtonFallback;

        private static Widget FindMapButton(Widget root)
        {
            if (root == null) return null;
            var all = root.GetAllChildrenAndThisRecursive();
            for (int i = 0; i < all.Count; i++)
                if (all[i] != null && all[i].Id == MapButtonId) return all[i];
            return null;
        }

        private static bool OverButtonBounds(Vec2 m)
        {
            Widget button = _mapButton;
            if (button == null) return OverAssumedBounds(m);
            float screenW = TaleWorlds.Engine.Screen.RealScreenResolutionWidth;
            float screenH = TaleWorlds.Engine.Screen.RealScreenResolutionHeight;
            float width = button.ScaledSuggestedWidth;
            float height = button.ScaledSuggestedHeight;
            if (!MapButton.BoundsReadable(screenW, screenH, width, height))
                return OverAssumedBounds(m);
            return MapButton.Over(m.x, m.y, screenW, screenH, width, height,
                                  button.ScaledMarginRight);
        }

        private static bool OverAssumedBounds(Vec2 m)
        {
            if (!_loggedButtonFallback)
            {
                _loggedButtonFallback = true;
                Log.Write("map button bounds unreadable - falling back to an assumed strip on the right edge. " +
                          "Map clicks near that edge may be taken by the button; turn the map button off if it gets in the way.");
            }
            return MapButton.OverTheStripInstead(m.x, m.y);
        }

        private static void UpdateIdleInput(bool buttonOn)
        {
            bool wantMouse = buttonOn && OverButtonBounds(Input.MousePositionRanged);
            if (wantMouse == _idleMouseActive) return;
            _idleMouseActive = wantMouse;
            if (wantMouse)
                _layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
            else
                _layer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.All);
        }

        private static string _keySource;
        private static InputKey _key = InputKey.T;
        private static string _keyLabel = "T";
        private static readonly List<(InputKey left, InputKey right)> _modifiers =
            new List<(InputKey, InputKey)>();

        private static bool ParseModifier(string name, out (InputKey, InputKey) pair)
        {
            switch (name.ToLowerInvariant())
            {
                case "ctrl": case "control": pair = (InputKey.LeftControl, InputKey.RightControl); return true;
                case "shift": pair = (InputKey.LeftShift, InputKey.RightShift); return true;
                case "alt": pair = (InputKey.LeftAlt, InputKey.RightAlt); return true;
            }
            pair = default;
            return false;
        }

        internal static InputKey PanelKey()
        {
            string source = Options.Current.PanelKey;
            if (source == _keySource) return _key;
            _keySource = source;
            _modifiers.Clear();
            string[] parts = (source ?? "").Split('+');
            string stray = null;
            for (int i = 0; i < parts.Length - 1; i++)
                if (ParseModifier(parts[i].Trim(), out var pair)) _modifiers.Add(pair);
                else if (stray == null && parts[i].Trim().Length > 0) stray = parts[i].Trim();
            string raw = parts[parts.Length - 1].Trim();
            bool numeric = raw.Length > 0;
            for (int i = 0; i < raw.Length; i++)
                if (!char.IsDigit(raw[i])) { numeric = false; break; }
            _key = InputKey.T;
            bool named = false;
            if (!numeric && Enum.TryParse(raw, ignoreCase: true, out InputKey k) &&
                Enum.IsDefined(typeof(InputKey), k))
            {
                _key = k;
                named = true;
            }
            if (!named)
                Log.Write("panel hotkey \"" + source + "\" names no key this game knows - falling back to T. " +
                          "Number keys are named D1 through D0.");
            if (stray != null)
                Log.Write("panel hotkey \"" + source + "\" puts \"" + stray + "\" in front of the key, which is " +
                          "not Ctrl, Alt or Shift - that part is dropped and the panel opens on the key alone.");
            _keyLabel = "";
            for (int i = 0; i < _modifiers.Count; i++)
                _keyLabel += _modifiers[i].left.ToString().Replace("Left", "") + "+";
            _keyLabel += _key.ToString();
            return _key;
        }

        private static bool HotkeyReleased()
        {
            if (!Input.IsKeyReleased(PanelKey())) return false;
            for (int i = 0; i < _modifiers.Count; i++)
                if (!Input.IsKeyDown(_modifiers[i].left) && !Input.IsKeyDown(_modifiers[i].right)) return false;
            return true;
        }

        private static void ApplyIdleInput()
        {
            if (_layer == null) return;
            _idleMouseActive = false;
            _layer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.All);
        }

        private static void Setup(MapScreen map)
        {
            _mapScreen = map;
            _vm = new LedgerPanelVM(Hide, ShowFromButton, CenterOn);
            _spokenFor = Options.Current.Language;
            _layer = new GauntletLayer("TradeLordPanel", 250);
            _movie = _layer.LoadMovie("TradeLordPanel", _vm);
            _mapButton = FindMapButton(_layer.UIContext?.Root);
            _mapScreen.AddLayer(_layer);
            _vm.IsVisible = false;
            ApplyIdleInput();
            if (!_loggedArmed)
            {
                _loggedArmed = true;
                PanelKey();
                Log.Write("ledger panel armed on map screen (hotkey " + _keyLabel +
                          (Options.Current.ShowMapButton ? ", map button on)" : ")"));
            }
        }

        private static void ShowFromButton() => Guard.Run("Panel.MapButton", Show);

        private static void Show()
        {
            if (_vm == null || _layer == null) return;
            _vm.Show();
            _layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
        }

        internal static bool TryShowFromMenu()
        {
            if (_dead || _vm == null || _layer == null || _mapScreen == null) return false;
            try { Show(); return true; }
            catch (Exception e)
            {
                Log.Error(e, "ledger panel from town menu (falling back to popup)");
                return false;
            }
        }

        internal static void Hide()
        {
            if (_vm == null || _layer == null) return;
            _vm.Hide();
            ApplyIdleInput();
        }

        private static void CenterOn(Settlement settlement)
        {
            Hide();
            if (_mapScreen == null || settlement == null) return;
            try { _mapScreen.FastMoveCameraToPosition(new CampaignVec2(settlement.GetPosition2D, true)); }
            catch (Exception e) { Log.Error(e, "panel camera jump"); }
            Guard.Run("Panel.ToggleMarker", () => ToggleMarker(settlement));
        }

        private static void ToggleMarker(Settlement settlement)
        {
            VisualTrackerManager tracker = Campaign.Current?.VisualTrackerManager;
            if (tracker == null) return;
            if (Unpin(settlement)) return;
            _panelPins.Add(settlement);
            if (!tracker.CheckTracked(settlement)) tracker.RegisterObject(settlement);
        }

        internal static bool Unpin(Settlement settlement)
        {
            if (settlement == null || !_panelPins.Remove(settlement)) return false;
            VisualTrackerManager tracker = Campaign.Current?.VisualTrackerManager;
            if (tracker != null && tracker.CheckTracked(settlement)) tracker.RemoveTrackedObject(settlement);
            return true;
        }

        internal static bool IsPinned(Settlement s) => _panelPins.Contains(s);

        internal static string PinnedIds()
        {
            var ids = new List<string>();
            foreach (Settlement s in _panelPins)
                if (s != null && s.StringId != null) ids.Add(s.StringId);
            return string.Join("|", ids.ToArray());
        }

        internal static void RestorePins(string ids)
        {
            _panelPins.Clear();
            if (string.IsNullOrEmpty(ids)) return;
            VisualTrackerManager tracker = Campaign.Current?.VisualTrackerManager;
            foreach (string id in ids.Split('|'))
            {
                Settlement s = Settlement.Find(id);
                if (s == null) continue;
                _panelPins.Add(s);
                if (tracker != null && !tracker.CheckTracked(s)) tracker.RegisterObject(s);
            }
        }

        private static bool MaySetUp()
        {
            if (_setupFailures >= SetupAttempts) return false;
            if (_setupCooldown > 0) { _setupCooldown--; return false; }
            return true;
        }

        internal static void Reset()
        {
            Cleanup();
            _panelPins.Clear();
            _setupFailures = 0;
            _setupCooldown = 0;
            _dead = false;
            _loggedArmed = false;
            _loggedButtonFallback = false;
            _idleMouseActive = false;
            _keySource = null;
        }

        internal static void Cleanup()
        {
            MapScreen map = _mapScreen;
            GauntletLayer layer = _layer;
            GauntletMovieIdentifier movie = _movie;
            LedgerPanelVM vm = _vm;
            _mapScreen = null; _layer = null; _movie = null; _vm = null; _mapButton = null;
            if (layer != null)
            {
                try
                {
                    layer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.All);
                    layer.IsFocusLayer = false;
                    ScreenManager.TryLoseFocus(layer);
                }
                catch { }
                if (movie != null) { try { layer.ReleaseMovie(movie); } catch { } }
                if (map != null) { try { map.RemoveLayer(layer); } catch { } }
            }
            if (vm != null) { try { vm.OnFinalize(); } catch { } }
        }
    }
}
