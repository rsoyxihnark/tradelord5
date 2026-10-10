using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using HarmonyLib;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Engine;
using TaleWorlds.Engine.GauntletUI;
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
        [DataSourceProperty] public string ItemName =>
            _route.Item == null ? "" : Tongue.Named(_route.Item.Name, _route.Item.StringId);
        [DataSourceProperty] public string BuyTownName =>
            _route.From == null ? "" : Tongue.Named(_route.From.Name, _route.From.StringId);
        [DataSourceProperty] public string BuyPrice => _route.BuyPrice.ToString();
        [DataSourceProperty] public string SellTownName =>
            _route.To == null ? "" : Tongue.Named(_route.To.Name, _route.To.StringId);
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
        private readonly int _aDay;

        public ShopOfferRowVM(Workshop shop, int cost, int aDay, bool affordable, Action bought)
        {
            _shop = shop;
            _bought = bought;
            _cost = cost;
            _aDay = aDay;
            Where = shop.Settlement == null ? "" : Tongue.Named(shop.Settlement.Name, shop.Settlement.StringId);
            What = shop.WorkshopType == null ? "" : Tongue.Named(shop.WorkshopType.Name, shop.WorkshopType.StringId);
            Owner = shop.Owner == null ? "" : Tongue.Named(shop.Owner.Name, shop.Owner.StringId);
            TextObject aDayLine = Tongue.Text("{=TL480}+{GOLD} a day");
            aDayLine.SetTextVariable("GOLD", aDay.ToString("N0"));
            Profit = aDayLine.ToString();
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
            float smoothing = Shops.PayoutSmoothing();
            int days = Holdings.DaysToPayBack(_cost, _aDay, smoothing);
            if (days == Holdings.NeverPaysBack)
                body += Tongue.Text("{=TL482} It pays its owner nothing a day now, so there is no telling when it would pay for itself.").ToString();
            else
            {
                TextObject payback = Tongue.Text("{=TL481} It pays its owner {GOLD} denars a day now and less in its first days with you, so it would pay for itself in about {DAYS} days.");
                payback.SetTextVariable("GOLD", _aDay.ToString("N0"));
                payback.SetTextVariable("DAYS", days.ToString("N0"));
                body += payback.ToString();
                Log.Write("workshop payback: " + Shops.Named(_shop) + " costs " + _cost + " and pays its owner " + _aDay +
                          " a day now; the game hands an owner 1/" +
                          smoothing.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture) +
                          " of what the workshop holds over its starting capital each day, and a workshop you buy " +
                          "starts back at that capital, so it pays for itself in about " + days + " day(s), where " +
                          "its pay now alone would say " + Holdings.DaysToPayBack(_cost, _aDay, Holdings.PaidOutInFull));
            }
            int heldBack = TradeActionBehavior.GoldHeldBack();
            if (Holdings.DipsIntoWhatYouHoldBack(_cost, Hero.MainHero?.Gold ?? 0, heldBack))
            {
                TextObject warned = Tongue.Text("{=TL441} This takes you below the {HELD} denars TradeLord holds back as your gold reserve and wage cover, which it will not spend on goods.");
                warned.SetTextVariable("HELD", heldBack.ToString("N0"));
                body += warned.ToString();
            }
            InformationManager.ShowInquiry(new InquiryData(
                Tongue.Text("{=TL431}Buy Workshops Remotely").ToString(), body,
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
        private string _tradeXpText = "";
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
        public string TradeXpText
        {
            get => _tradeXpText;
            set { if (value != _tradeXpText) { _tradeXpText = value; OnPropertyChangedWithValue(value, "TradeXpText"); } }
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
        [DataSourceProperty] public string HelpLabel => Tongue.Text("{=TL446}?").ToString();
        [DataSourceProperty] public string ShopsLabel => Tongue.Text("{=TL431}Buy Workshops Remotely").ToString();
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

        public void ExecuteClose()
        {
            LedgerPanel.Noted("with its Close button");
            _onClose?.Invoke();
        }

        public void ExecuteCloseFromOutside()
        {
            LedgerPanel.Noted("by a click outside it");
            _onClose?.Invoke();
        }

        public void ExecuteOpenTrades() => Guard.Run("Panel.OpenTrades", () =>
        {
            LedgerPanel.Noted("with the Recent trades button");
            RefreshTrades();
            IsTradesVisible = true;
        });

        public void ExecuteCloseTrades()
        {
            LedgerPanel.Noted("with its Close button");
            IsTradesVisible = false;
        }

        public void ExecuteCloseTradesFromOutside()
        {
            LedgerPanel.Noted("by a click outside it");
            IsTradesVisible = false;
        }

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
            "BrandLabel", "TitleLabel", "RefreshLabel", "TradesLabel", "LegendLabel", "HelpLabel", "ShopsLabel",
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
            long earned = LedgerBehavior.Instance?.LifetimeTradeXp ?? 0L;
            TradeXpText = Line("{=TL448}Trade XP: {AMOUNT}", "AMOUNT", earned.ToString("N0"));

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
                : Line("{=TL68}{COUNT} profitable route(s), best first", "COUNT", rows.Count.ToString());
            LegendText = (empty
                ? Tongue.Text(Options.Current.Omniscient
                    ? "{=TL69}No routes are within your travel ceilings. | Raise the ceilings in the Trade Pool settings, or move nearer to more markets."
                    : "{=TL90}No routes are within your travel ceilings, from the prices you have recorded so far. | Walk more markets, or raise the ceilings in the Trade Pool settings.").ToString()
                : Tongue.Text("{=TL70}Click a market's name to jump to it and pin or unpin it | Days = you -> buy market -> sell market | Carv. = caravans at or heading for those markets").ToString()
                  + Tongue.Text("{=TL476} | Price is the first unit's; Profit prices every unit in turn, so it is less than price x qty | Conf* = flat quote, not priced per unit").ToString()
                  + (Options.Current.ConfidenceRanking
                        ? Tongue.Text("{=TL71} | Score = profit per day discounted by Conf").ToString()
                        : Tongue.Text("{=TL72} | Score = profit per day").ToString())
                  + (Options.Current.ConfidenceRanking && Options.Current.TrustWhatAMarketPaid
                        ? Tongue.Text("{=TL445} | Score is lowered for a market that has paid less than this panel promised").ToString()
                        : "")
                  + (Options.Current.ConservativeRouteProjection
                        ? Tongue.Text("{=TL73} | resale safety factor applied").ToString() : "")
                  + (Forecast.On
                        ? (Options.Current.BulkSimulation
                              ? Tongue.Text("{=TL394} | prices and stock count what caravans, workshops and the town add and take before you arrive").ToString()
                              : Tongue.Text("{=TL447} | stock counts what caravans, workshops and the town add and take before you arrive | prices need Bulk price simulation to count it").ToString())
                          + Tongue.Text("{=TL397} | Qty! = part of that amount is still on the road and lands before you would").ToString()
                          + Tongue.Text("{=TL417} | Left = how long from now that shelf still holds this Qty, and a shelf that empties first lowers Conf").ToString()
                        : ""))
                + HowThePromiseHasHeld()
                + ResaleSafetyInUse()
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
                var (shop, cost, aDay) = offers[i];
                if (cost <= 0) continue;
                rows.Add(new ShopOfferRowVM(shop, cost, aDay, cost <= purse && owned < mayOwn,
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

        private static List<(Workshop shop, int cost, int aDay)> Shops_OnOffer() => TradeLord.Shops.OnOffer();

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

        private static string DayOf(float day)
        {
            int ago = Recent.DaysAgo(day, (float)CampaignTime.Now.ToDays);
            return ago <= 0
                ? Tongue.Text("{=TL442}Today").ToString()
                : Line("{=TL420}{DAYS} day(s) ago", "DAYS", ago.ToString("N0"));
        }

        private static string OneClauseToALine(string said) =>
            said == null ? "" : said.Replace(" | ", "\n");

        private static string HowThePromiseHasHeld()
        {
            LedgerBehavior ledger = LedgerBehavior.Instance;
            if (ledger == null || !ledger.PromiseScore(out int checked_, out float held)) return "";
            TextObject line = Tongue.Text("{=TL399} | the Sell price this panel promised has been {HELD} on target, above or below, over {COUNT} price(s) checked");
            line.SetTextVariable("HELD", ((int)Math.Round(held * 100f)).ToString() + "%");
            line.SetTextVariable("COUNT", checked_.ToString());
            return line.ToString();
        }

        private static string ResaleSafetyInUse()
        {
            float used = TradePolicy.ResaleSafety(out float setting, out bool learn, out int walkIns, out _);
            if (!learn || walkIns <= 0 || Math.Round(used * 100f) == Math.Round(setting * 100f)) return "";
            TextObject line = Tongue.Text("{=TL485} | a price elsewhere counts at {USED}: Resale safety factor set it at {SET} and {COUNT} walk-in(s) moved it");
            line.SetTextVariable("USED", ((int)Math.Round(used * 100f)).ToString() + "%");
            line.SetTextVariable("SET", ((int)Math.Round(setting * 100f)).ToString() + "%");
            line.SetTextVariable("COUNT", walkIns.ToString());
            return line.ToString();
        }

        private static string NothingHereYouCouldBuy(Hero hero)
        {
            int held = TradeActionBehavior.GoldHeldBack(), flat = Options.Current.GoldReserve;
            TextObject line = Tongue.Text(held > flat
                ? "{=TL393}Your purse is at {GOLD} denars and TradeLord holds {RESERVE} of it back, so you could buy nothing here. | That is {FLAT} for Gold reserve and {WAGES} for Keep gold for days of wages. | Sell some cargo, or lower either of those settings."
                : "{=TL377}Your purse is at {GOLD} denars and Gold reserve holds {RESERVE} of it back, so you could buy nothing here. | Sell some cargo, or lower Gold reserve in its settings.");
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
                    Tongue.Named(w.WorkshopType.Name, w.WorkshopType.StringId) + " - " +
                        (w.Settlement == null ? "?" : Tongue.Named(w.Settlement.Name, w.Settlement.StringId)),
                    (w.ProfitMade >= 0 ? "+" : "") + w.ProfitMade,
                    Forecast.WillMake(w),
                    w.Owner == null ? "" : Tongue.Named(w.Owner.Name, w.Owner.StringId)));
            }
            Workshops = rows;
        }
    }

    internal sealed class LedgerPanelEscape : MapView
    {
        protected override bool IsEscaped() => LedgerPanel.CloseOnEscape();
    }

    internal static class LedgerPanel
    {
        private static MapScreen _mapScreen;
        private static GauntletLayer _layer;
        private static MapView _escape;
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
                if (_vm.IsTradesVisible && map.IsEscapeMenuOpened) _vm.IsTradesVisible = false;
                UpdateIdleInput();
                if (!map.IsEscapeMenuOpened && HotkeyReleased() && !TypingOnScreen(map))
                {
                    Noted("with the Ledger panel hotkey (map screen), " + _keyLabel);
                    Guard.Run("Panel.Show", Show);
                }
            }
            else if (map.IsEscapeMenuOpened || (HotkeyReleased() && !TypingOnScreen(map)))
            {
                Noted(map.IsEscapeMenuOpened ? "as the game's menu opened" : "with the Ledger panel hotkey (map screen), " + _keyLabel);
                Hide();
            }
        }

        private static string _how;
        private static float _howAt;
        private const float StillFresh = 1f;

        internal static void Noted(string how)
        {
            _how = how;
            _howAt = TaleWorlds.Engine.Time.ApplicationTime;
        }

        internal static string HowItChanged(float now)
        {
            string how = _how != null && now - _howAt <= StillFresh ? _how : null;
            _how = null;
            return how;
        }

        internal static string OpenWindow =>
            _vm == null ? null : _vm.IsVisible ? "the ledger panel" : _vm.IsTradesVisible ? "the Recent trades window" : null;

        private static bool TypingOnScreen(ScreenBase screen)
        {
            try
            {
                MBReadOnlyList<ScreenLayer> layers = screen?.Layers;
                for (int i = 0; layers != null && i < layers.Count; i++)
                    if (layers[i] != null && layers[i].IsFocusedOnInput()) return true;
                List<ScreenLayer> shown = ScreenManager.SortedLayers;
                for (int i = 0; shown != null && i < shown.Count; i++)
                    if (shown[i] != null && shown[i].IsFocusedOnInput()) return true;
            }
            catch (Exception e) { Log.Error(e, "panel hotkey text-field check (hotkey left as it was)"); }
            return false;
        }

        private static bool _idleMouseActive;

        internal static ScreenLayer OwnLayer => _layer;

        internal static bool AnyWindowOpen => _vm != null && (_vm.IsVisible || _vm.IsTradesVisible);

        private static void UpdateIdleInput()
        {
            bool open = _vm.IsTradesVisible;
            if (open == _idleMouseActive) return;
            _idleMouseActive = open;
            TakeTheMouse(open);
        }

        private static void TakeTheMouse(bool windowOpen)
        {
            var takes = MapButton.LayerTakes(windowOpen);
            if (takes.showsMouse) _layer.ActiveCursor = CursorType.Default;
            _layer.InputRestrictions.SetInputRestrictions(takes.showsMouse,
                takes.takesWheel ? InputUsageMask.Mouse : InputUsageMask.MouseButtons);
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
            TakeTheMouse(false);
        }

        private static void Setup(MapScreen map)
        {
            _mapScreen = map;
            _vm = new LedgerPanelVM(Hide, ShowFromButton, CenterOn);
            _spokenFor = Options.Current.Language;
            _layer = new GauntletLayer("TradeLordPanel", 250);
            _movie = _layer.LoadMovie("TradeLordPanel", _vm);
            _mapScreen.AddLayer(_layer);
            _escape = HearEscape(map);
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

        private static MapView HearEscape(MapScreen map)
        {
            try { return map.AddMapView<LedgerPanelEscape>(); }
            catch (Exception e)
            {
                Log.Error(e, "ledger panel Esc (Esc opens the game's own menu over the panel as it did before)");
                return null;
            }
        }

        internal static bool CloseOnEscape()
        {
            try
            {
                if (_dead || _vm == null || _layer == null) return false;
                if (_vm.IsVisible || _vm.IsTradesVisible) Noted("with Esc");
                if (_vm.IsVisible) { Hide(); return true; }
                if (_vm.IsTradesVisible) { _vm.IsTradesVisible = false; return true; }
                return false;
            }
            catch (Exception e)
            {
                Log.Error(e, "closing the ledger panel on Esc (the game's own menu opens instead)");
                return false;
            }
        }

        private static void ShowFromButton() => Guard.Run("Panel.MapButton", OpenWithTheButton);

        private static void Show()
        {
            if (_vm == null || _layer == null) return;
            _vm.Show();
            _layer.ActiveCursor = CursorType.Default;
            _layer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
        }

        private static void OpenWithTheButton()
        {
            Noted("with the TradeLord button");
            Show();
        }

        internal static bool TryShowFromMenu()
        {
            if (_dead || _vm == null || _layer == null || _mapScreen == null) return false;
            Noted("with Consult the TradeLord ledger");
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
            Noted("by a click on " + (settlement?.Name?.ToString() ?? "a market") + " in it, which moves the camera there");
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
            tracker.RegisterObject(settlement);
        }

        internal static bool Unpin(Settlement settlement)
        {
            if (settlement == null || !_panelPins.Remove(settlement)) return false;
            VisualTrackerManager tracker = Campaign.Current?.VisualTrackerManager;
            if (tracker != null && tracker.CheckTracked(settlement)) tracker.RemoveTrackedObject(settlement);
            return true;
        }

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
            _idleMouseActive = false;
            _keySource = null;
            _how = null;
        }

        internal static void Cleanup()
        {
            MapScreen map = _mapScreen;
            GauntletLayer layer = _layer;
            GauntletMovieIdentifier movie = _movie;
            LedgerPanelVM vm = _vm;
            MapView escape = _escape;
            _mapScreen = null; _layer = null; _movie = null; _vm = null;
            _escape = null;
            if (map != null && escape != null) { try { map.RemoveMapView(escape); } catch { } }
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

    internal static class CursorWatch
    {
        private const float LongEnough = 3f;
        private const float FarEnough = 0.25f;
        private const float JustBefore = 1f;
        private const float LookAgain = 0.5f;
        private const float QuickClick = 0.4f;
        private const float StillEnough = 12f;
        private const float Settle = 0.1f;
        private const float HeldFor = 0.5f;
        private const float AfterTheClick = 0.5f;
        private const float BlindAfter = 10f;
        private const string KeptTheLayer = "nothing, the game keeping the layer it found the frame before";
        private const string NoLayer = "nothing, with no layer under the mouse";

        private struct Run
        {
            internal float From;
            internal ScreenLayer Top;
            internal CursorType Drawn;
            internal CursorType MapAsked;
        }

        private struct Telling
        {
            internal float From;
            internal ScreenLayer By;
            internal CursorType Shape;
            internal string Nothing;
        }

        private struct OnScreen
        {
            internal float From;
            internal bool Over;
            internal bool Showing;
            internal IntPtr Handle;
        }

        private sealed class Ground
        {
            internal string Said;
            internal bool SignIsRight;
        }

        private static readonly Ground Unread = new Ground { Said = "could not be read" };
        private static readonly List<Run> _runs = new List<Run>();
        private static readonly List<Telling> _tellings = new List<Telling>();
        private static readonly List<OnScreen> _onScreen = new List<OnScreen>();
        private static readonly PointerLooks _looks = new PointerLooks();

        private static MapScreen _map;
        private static bool _showing;
        private static bool _told;
        private static bool _dead;
        private static float _since;
        private static float _travelled;
        private static Vec2 _was;
        private static float _lookedAt = -1f;
        private static string _lastButton;
        private static float _lastButtonAt;
        private static float _rightDownAt = -1f;
        private static float _rightMoved;
        private static string _beforeRight;
        private static string _beforeRightOnScreen;
        private static float _hotkeyAt = -1f;
        private static bool _windowOpen;
        private static float _windowAt = -1f;
        private static string _windowWhich;
        private static string _windowSeen;
        private static string _windowHow;
        private static float _afterRightAt = -1f;

        private static bool _inScreens;
        private static bool _keptTheLayer;
        private static bool _tellingDead;
        private static bool _silentSaid;
        private static int _setElsewhere;
        private static CursorType _setElsewhereShape;
        private static float _setElsewhereAt;
        private static string _setElsewhereBy;
        private static string _otherHands;

        private static bool _askedAny;
        private static CursorType _asked;
        private static float _askedSince;
        private static float _signFrom = -1f;
        private static float _signTo = -1f;
        private static string _signBy;
        private static string _signAt;
        private static string _signOver;
        private static float _signLookedAt = -1f;

        private static float _heldFrom = -1f;
        private static float _heldSince;
        private static long _heldLook;
        private static bool _heldSaid;
        private static List<string> _heldBefore;
        private static float _hiddenFor;
        private static float _hiddenAt = -1f;
        private static bool _windowsSeen;
        private static bool _blindSaid;
        private static float _blindFrom = -1f;

        internal static void Forget()
        {
            _runs.Clear();
            _map = null;
            _showing = false;
            _told = false;
            _dead = false;
            _lastButton = null;
            _rightDownAt = -1f;
            _beforeRight = null;
            _beforeRightOnScreen = null;
            _hotkeyAt = -1f;
            _windowOpen = false;
            _windowAt = -1f;
            _windowWhich = null;
            _windowSeen = null;
            _windowHow = null;
            _afterRightAt = -1f;
            _askedAny = false;
            _signFrom = -1f;
            _signTo = -1f;
            _signAt = null;
            _signOver = null;
            _signLookedAt = -1f;
            _heldFrom = -1f;
            _heldBefore = null;
            _hiddenAt = -1f;
            _onScreen.Clear();
        }

        internal static void Tick()
        {
            if (_dead) return;
            try { TickCore(); }
            catch (Exception e)
            {
                _dead = true;
                Log.Error(e, "map cursor watch (stopped for this campaign, the map itself is unaffected)");
            }
        }

        private static void TickCore()
        {
            MapScreen map = ScreenManager.TopScreen as MapScreen;
            if (map == null && _map == null) return;
            float now = TaleWorlds.Engine.Time.ApplicationTime;
            if (map != _map)
            {
                Left(now);
                _map = map;
                _runs.Clear();
                _rightDownAt = -1f;
                if (map == null) return;
            }
            ScreenLayer top = ScreenManager.FirstHitLayer;
            Note(now, top, map.SceneLayer);
            NoteAsked(now, map, top);
            NoteKeys(now, map);
            Watch(now, map, top);
            WatchWhatWindowsShows(now, top);
            WatchTheSilence(now);
            if (_afterRightAt >= 0f && now >= _afterRightAt)
            {
                _afterRightAt = -1f;
                Log.Write(Guard.Read("map cursor watch: after a right click", now, HalfASecondAfter,
                                     "map cursor: half a second after the quick right click the pointer could not be read"));
            }
        }

        internal static void BeforeTheScreens()
        {
            if (_tellingDead) return;
            _inScreens = true;
            _keptTheLayer = ScreenManager.FirstHitLayer != null;
        }

        internal static void AfterTheScreens()
        {
            if (_tellingDead) return;
            _inScreens = false;
            try { NoteTheTelling(TaleWorlds.Engine.Time.ApplicationTime); }
            catch (Exception e) { StopTelling(e); }
        }

        internal static void SetElsewhere(CursorType shape)
        {
            if (_inScreens || _tellingDead) return;
            try { NoteSetElsewhere(TaleWorlds.Engine.Time.ApplicationTime, shape); }
            catch (Exception e) { StopTelling(e); }
        }

        private static void StopTelling(Exception e)
        {
            _tellingDead = true;
            _inScreens = false;
            Log.Error(e, "map cursor watch: the pointer the engine is told (stopped for this session, the game is unaffected)");
        }

        private static void NoteTheTelling(float now)
        {
            ScreenLayer by = _keptTheLayer ? null : ScreenManager.FirstHitLayer;
            string nothing = by != null ? null : _keptTheLayer ? KeptTheLayer : NoLayer;
            CursorType shape = by == null ? CursorType.Default : by.ActiveCursor;
            int last = _tellings.Count - 1;
            if (last >= 0 && _tellings[last].By == by && _tellings[last].Shape == shape && _tellings[last].Nothing == nothing)
                return;
            _tellings.Add(new Telling { From = now, By = by, Shape = shape, Nothing = nothing });
            while (_tellings.Count > 1 && _tellings[1].From <= now - JustBefore)
                _tellings.RemoveAt(0);
        }

        private static void NoteSetElsewhere(float now, CursorType shape)
        {
            bool named = _setElsewhereBy != null && shape == _setElsewhereShape && now - _setElsewhereAt <= JustBefore;
            _setElsewhere++;
            _setElsewhereShape = shape;
            _setElsewhereAt = now;
            if (!named) _setElsewhereBy = Caller();
        }

        private static string Caller()
        {
            var trace = new System.Diagnostics.StackTrace(1, false);
            for (int i = 0; i < trace.FrameCount; i++)
            {
                MethodBase method = trace.GetFrame(i)?.GetMethod();
                Type type = method?.DeclaringType;
                if (type == null || type.Assembly.IsDynamic || type.Assembly == typeof(CursorWatch).Assembly ||
                    type == typeof(MouseManager) || type.Assembly == typeof(Harmony).Assembly)
                    continue;
                return type.FullName + "." + method.Name + " in " + type.Assembly.GetName().Name;
            }
            return "code TradeLord could not name";
        }

        private static void Left(float now)
        {
            if (_showing && _told)
                Log.Write("map cursor: the campaign map was left after " + Seconds(now - _since) +
                          " seconds with the forbidden sign still on, for " +
                          (ScreenManager.TopScreen?.GetType().Name ?? "no screen at all"));
            _showing = false;
            _told = false;
            if (_heldFrom >= 0f && _heldSaid)
                Log.Write("map cursor: the campaign map was left " + Seconds(now - _heldSince) + " seconds after Windows began to show " +
                          Looks(_heldLook) + " where the game asks for " + Shape(_asked) + ", and it still did, for " +
                          (ScreenManager.TopScreen?.GetType().Name ?? "no screen at all"));
            _heldFrom = -1f;
            _hiddenAt = -1f;
            _askedAny = false;
            _onScreen.Clear();
        }

        private static void Note(float now, ScreenLayer top, ScreenLayer map)
        {
            CursorType drawn = top?.ActiveCursor ?? CursorType.Default;
            CursorType asked = map?.ActiveCursor ?? CursorType.Default;
            int last = _runs.Count - 1;
            if (last < 0 || _runs[last].Top != top || _runs[last].Drawn != drawn || _runs[last].MapAsked != asked)
                _runs.Add(new Run { From = now, Top = top, Drawn = drawn, MapAsked = asked });
            while (_runs.Count > 1 && _runs[1].From <= now - JustBefore)
                _runs.RemoveAt(0);
        }

        private static void NoteAsked(float now, MapScreen map, ScreenLayer top)
        {
            if (top == null) return;
            CursorType asked = top.ActiveCursor;
            if (_askedAny && asked == _asked) return;
            if (_askedAny && _asked == CursorType.Disabled) _signTo = now;
            _askedAny = true;
            _asked = asked;
            _askedSince = now;
            if (asked != CursorType.Disabled) return;
            _signFrom = now;
            _signTo = -1f;
            _signBy = Named(top);
            _signAt = Spot(Input.MousePositionPixel);
            if (_signLookedAt >= 0f && now - _signLookedAt < LookAgain)
            {
                _signOver = null;
                return;
            }
            _signLookedAt = now;
            _signOver = Guard.Read("map cursor watch: where the forbidden sign was asked for", map, Look, Unread).Said +
                        Guard.Read("map cursor watch: what the map counts as under the mouse", map, Hovered, "");
        }

        private static string Hovered(MapScreen map)
        {
            var visual = map.CurrentVisualOfTooltip;
            return visual == null
                ? ", with nothing the map counts as a party or a place under the mouse"
                : ", with a " + visual.GetType().Name + " under the mouse that the map says " +
                  (visual.IsInteractable() ? "can" : "cannot") + " be clicked";
        }

        private static void NoteKeys(float now, MapScreen map)
        {
            if (Input.IsKeyReleased(LedgerPanel.PanelKey())) _hotkeyAt = now;
            bool open = LedgerPanel.AnyWindowOpen;
            if (open != _windowOpen)
            {
                _windowOpen = open;
                _windowAt = now;
                _windowWhich = open ? LedgerPanel.OpenWindow : _windowSeen;
                _windowHow = LedgerPanel.HowItChanged(now) ??
                             (!open && map.IsEscapeMenuOpened ? "as the game's menu opened" : null);
            }
            if (open) _windowSeen = LedgerPanel.OpenWindow;
            NoteButton(now, InputKey.LeftMouseButton, "left");
            NoteButton(now, InputKey.MiddleMouseButton, "middle");
            NoteButton(now, InputKey.RightMouseButton, "right");
            if (Input.IsKeyPressed(InputKey.RightMouseButton))
            {
                _rightDownAt = now;
                _rightMoved = 0f;
                _beforeRight = Guard.Read("map cursor watch: before a right click", now, BeforeTheClick, "could not be read");
                _beforeRightOnScreen = Guard.Read("map cursor watch: the pointer before a right click", now, TheSecondOnScreen,
                                                  "the pointer could not be read");
            }
            else if (_rightDownAt >= 0f && Input.IsKeyDown(InputKey.RightMouseButton))
                _rightMoved += Math.Abs(Input.MouseMoveX) + Math.Abs(Input.MouseMoveY);
            else if (_rightDownAt >= 0f)
            {
                float held = now - _rightDownAt;
                _rightDownAt = -1f;
                if (Input.IsKeyReleased(InputKey.RightMouseButton) && held <= QuickClick &&
                    _rightMoved <= StillEnough && !_windowOpen)
                {
                    Log.WriteMany(AQuickRightClick(map, held));
                    _afterRightAt = now + AfterTheClick;
                }
            }
        }

        private static void NoteButton(float now, InputKey button, string name)
        {
            if (Input.IsKeyPressed(button)) _lastButton = name + " mouse button went down";
            else if (Input.IsKeyReleased(button)) _lastButton = name + " mouse button came up";
            else return;
            _lastButtonAt = now;
        }

        private static void Watch(float now, MapScreen map, ScreenLayer top)
        {
            if (top == null || top.ActiveCursor != CursorType.Disabled)
            {
                if (_showing && _told)
                    Log.Write("map cursor: the forbidden sign went away after " + Seconds(now - _since) +
                              " seconds, " + WhatTheButtonsDid(now) + ", and the game now draws " + Drawn(top));
                _showing = false;
                _told = false;
                return;
            }
            Vec2 at = Input.MousePositionPixel;
            if (!_showing)
            {
                _showing = true;
                _told = false;
                _since = now;
                _travelled = 0f;
                _was = at;
                _lookedAt = -1f;
                return;
            }
            _travelled += at.Distance(_was);
            _was = at;
            if (_told) return;
            if (now - _since < LongEnough || _travelled < FarEnough * Input.Resolution.x) return;
            if (_lookedAt >= 0f && now - _lookedAt < LookAgain) return;
            _lookedAt = now;
            Ground ground = Guard.Read("map cursor watch: under the mouse", map, Look, Unread);
            if (ground.SignIsRight) return;
            _told = true;
            Log.WriteMany(WhatTheMapIsDoing(map, top, now, ground));
        }

        private static void WatchWhatWindowsShows(float now, ScreenLayer top)
        {
            if (!WindowsPointer.Read(out bool over, out bool showing, out IntPtr handle)) return;
            int last = _onScreen.Count - 1;
            if (last < 0 || _onScreen[last].Over != over || _onScreen[last].Showing != showing || _onScreen[last].Handle != handle)
                _onScreen.Add(new OnScreen { From = now, Over = over, Showing = showing, Handle = handle });
            while (_onScreen.Count > 1 && _onScreen[1].From <= now - JustBefore)
                _onScreen.RemoveAt(0);
            if (!over || top == null || !_askedAny) return;
            NoteBlind(now, showing);
            if (!showing)
            {
                if (_heldFrom >= 0f && _hiddenAt < 0f) _hiddenAt = now;
                return;
            }
            float shownSince = _onScreen[_onScreen.Count - 1].From;
            long look = handle.ToInt64();
            _looks.See((int)_asked, look, now, _askedSince, shownSince, Settle);
            Hold(now, look, PointerLooks.Settled(now, _askedSince, Settle), shownSince);
        }

        private static void Hold(float now, long look, bool settled, float shownSince)
        {
            if (_hiddenAt >= 0f)
            {
                _hiddenFor += now - _hiddenAt;
                _hiddenAt = -1f;
            }
            bool known = _looks.Known((int)_asked, out long wanted);
            bool wrong = PointerHeld.Wrong(settled, known, look, wanted);
            if (_heldFrom >= 0f)
            {
                if (PointerHeld.Still(look, _heldLook, settled, known, wanted))
                {
                    if (_heldSaid || !wrong || now - _heldFrom < HeldFor) return;
                    _heldSaid = true;
                    Log.WriteMany(Held(now));
                    return;
                }
                if (_heldSaid)
                    Log.Write(Guard.Read("map cursor watch: as Windows let go of a pointer", look, Ended,
                                         "map cursor: Windows let go of a pointer it held, and what it shows now could not be read"));
                _heldFrom = -1f;
            }
            if (!wrong) return;
            _heldFrom = now;
            _heldSince = Math.Max(_askedSince, shownSince);
            _heldLook = look;
            _heldSaid = false;
            _hiddenFor = 0f;
            _hiddenAt = -1f;
            _heldBefore = Guard.Read("map cursor watch: as Windows held on to a pointer", now, HowItBegan, null);
        }

        private static List<string> Held(float now)
        {
            var said = new List<string>
            {
                "map cursor: for " + Seconds(now - _heldSince) + " seconds Windows has shown " + Looks(_heldLook) +
                    " over the campaign map while the game asks for " + Shape(_asked)
            };
            if (_heldBefore != null) said.AddRange(_heldBefore);
            return said;
        }

        private static List<string> HowItBegan(float now) => new List<string>
        {
            "  in the second before it " + TheSecondOnScreen(now),
            "  " + TheSignLastAsked(now),
            "  input: " + WhatTheButtonsDid(now) + ", keyboard on " + Named(ScreenManager.FocusedLayer) + ", " +
                WindowsPointer.WhoHasTheMouse() + ", " + TheHotkey(now),
            "  " + Elsewhere(now) + "; " + OtherHands()
        };

        private static string Ended(long look)
        {
            float now = TaleWorlds.Engine.Time.ApplicationTime;
            bool fits = _looks.Known((int)_asked, out long wanted) && wanted == look;
            return "map cursor: after " + Seconds(now - _heldSince) + " seconds Windows let go of " + Looks(_heldLook) + " " +
                   (look == _heldLook ? "as the game itself now asks for it" : WhatTheButtonsDid(now)) +
                   (_hiddenFor > 0f ? ", once Windows had hidden the pointer for " + Seconds(_hiddenFor, "0.00") + " s" : "") +
                   ", and now shows " + Looks(look) +
                   (fits ? ", the pointer the game asks for" : ", while the game asks for " + Shape(_asked));
        }

        private static string HalfASecondAfter(float now)
        {
            string said = "map cursor: half a second after the quick right click the game asks for " +
                          (_askedAny ? Shape(_asked) : "no pointer TradeLord saw") + ", and Windows shows ";
            if (!WindowsPointer.Read(out bool over, out bool showing, out IntPtr handle))
                return said + "a pointer TradeLord cannot read";
            if (!over) return said + "the pointer of another window, as the mouse is not over the game";
            if (!showing) return said + "no pointer at all";
            long look = handle.ToInt64();
            if (!_askedAny || !_looks.Known((int)_asked, out long wanted))
                return said + Looks(look) + ", which TradeLord has not yet seen the game draw for it";
            return said + Looks(look) + (wanted == look ? ", the same pointer" : ", another pointer");
        }

        private static void WatchTheSilence(float now)
        {
            int last = _tellings.Count - 1;
            if (last < 0 || _tellings[last].Nothing != KeptTheLayer)
            {
                _silentSaid = false;
                return;
            }
            if (_silentSaid || now - _tellings[last].From < HeldFor) return;
            _silentSaid = true;
            Log.Write("map cursor: for " + Seconds(now - _tellings[last].From) + " seconds the game has told the engine no " +
                      "pointer at all, as it kept the layer it found the frame before; " +
                      Guard.Read("map cursor watch: the pointer while the engine is told nothing", now, TheSecondOnScreen,
                                 "the pointer could not be read"));
        }

        private static void NoteBlind(float now, bool showing)
        {
            if (_blindSaid || _windowsSeen) return;
            if (showing)
            {
                _windowsSeen = true;
                return;
            }
            if (!ScreenManager.GetMouseVisibility())
            {
                _blindFrom = -1f;
                return;
            }
            if (_blindFrom < 0f) _blindFrom = now;
            if (now - _blindFrom < BlindAfter) return;
            _blindSaid = true;
            Log.Write("map cursor watch: Windows has shown no pointer of its own over the game for " + Seconds(BlindAfter) +
                      " seconds while the game shows the mouse, so the game draws its pointer itself and " +
                      "TradeLord.log cannot read which one is on the screen");
        }

        private static string WhatTheButtonsDid(float now)
        {
            string held = Input.IsKeyDown(InputKey.RightMouseButton) ? "right"
                : Input.IsKeyDown(InputKey.LeftMouseButton) ? "left"
                : Input.IsKeyDown(InputKey.MiddleMouseButton) ? "middle" : null;
            if (held != null) return "while the " + held + " mouse button was held down";
            return _lastButton != null && now - _lastButtonAt <= JustBefore
                ? "right after the " + _lastButton
                : "with no mouse button touched in the second before";
        }

        private static string Drawn(ScreenLayer top) =>
            top == null ? "no cursor, with the mouse outside the game's window" : top.ActiveCursor + " from " + Named(top);

        private static string BeforeTheClick(float now) =>
            TheSecondBefore(now) + "; the game " + (ScreenManager.GetMouseVisibility() ? "showed" : "hid") +
            " the mouse and the engine " + (EngineShowsTheMouse() ? "showed it" : "hid it") +
            (Input.IsGamepadActive ? ", a gamepad in use" : "") +
            ", keyboard on " + Named(ScreenManager.FocusedLayer) + ", " + TheHotkey(now);

        private static string TheSecondBefore(float now)
        {
            var parts = new List<string>();
            for (int i = 0; i < _runs.Count; i++)
            {
                bool latest = i + 1 == _runs.Count;
                float from = Math.Max(_runs[i].From, now - JustBefore);
                float to = latest ? now : _runs[i + 1].From;
                if (to <= from && !latest) continue;
                Run run = _runs[i];
                parts.Add((run.Top == null
                              ? "no cursor, the mouse outside the game's window"
                              : run.Drawn + " from " + run.Top.Name +
                                (run.MapAsked == run.Drawn ? "" : " while the map's own layer asked for " + run.MapAsked)) +
                          " for " + Seconds(to - from, "0.00") + " s");
            }
            return parts.Count == 0 ? "nothing was seen" : string.Join(", then ", parts.ToArray());
        }

        private static string TheSecondOnScreen(float now) =>
            "the engine was told " + WhatTheEngineWasTold(now) + ", and Windows showed " + WhatWindowsShowed(now);

        private static string WhatTheEngineWasTold(float now)
        {
            if (_tellingDead || !Patcher.Holds(nameof(Patch_ThePointerTheEngineIsTold))) return "a pointer TradeLord cannot see";
            var parts = new List<string>();
            for (int i = 0; i < _tellings.Count; i++)
            {
                bool latest = i + 1 == _tellings.Count;
                float from = Math.Max(_tellings[i].From, now - JustBefore);
                float to = latest ? now : _tellings[i + 1].From;
                if (to <= from && !latest) continue;
                Telling one = _tellings[i];
                parts.Add((one.Nothing ?? Shape(one.Shape) + " by " + Named(one.By)) + " for " + Seconds(to - from, "0.00") + " s");
            }
            return parts.Count == 0 ? "nothing yet" : string.Join(", then ", parts.ToArray());
        }

        private static string WhatWindowsShowed(float now)
        {
            if (!WindowsPointer.Readable) return "a pointer TradeLord cannot read";
            var parts = new List<string>();
            for (int i = 0; i < _onScreen.Count; i++)
            {
                bool latest = i + 1 == _onScreen.Count;
                float from = Math.Max(_onScreen[i].From, now - JustBefore);
                float to = latest ? now : _onScreen[i + 1].From;
                if (to <= from && !latest) continue;
                OnScreen one = _onScreen[i];
                parts.Add((!one.Over ? "the pointer of another window" : !one.Showing ? "no pointer" : Looks(one.Handle.ToInt64())) +
                          " for " + Seconds(to - from, "0.00") + " s");
            }
            return parts.Count == 0 ? "nothing yet" : string.Join(", then ", parts.ToArray());
        }

        private static string Looks(long look) =>
            _looks.ShapeOf(look, out int shape)
                ? Shape((CursorType)shape)
                : WindowsPointer.Named(new IntPtr(look)) ?? "a pointer TradeLord has not yet seen the game ask for";

        private static string Shape(CursorType shape) =>
            shape == CursorType.Default ? "the normal pointer"
            : shape == CursorType.Disabled ? "the forbidden sign"
            : "the " + shape + " pointer";

        private static string TheSignLastAsked(float now) =>
            _signFrom < 0f
                ? "the forbidden sign was not asked for on the campaign map since TradeLord began watching it"
                : "the forbidden sign was last asked for " + Seconds(now - _signFrom) + " seconds before, by " + _signBy +
                  (_signTo < _signFrom ? ", and still is" : ", for " + Seconds(_signTo - _signFrom, "0.00") + " s") +
                  ", with the mouse at " + _signAt + (_signOver == null ? "" : ", over " + _signOver);

        private static string Elsewhere(float now) =>
            !Patcher.Holds(nameof(Patch_APointerSetElsewhere)) || _tellingDead
                ? "a pointer set outside the game's screens cannot be seen"
                : _setElsewhere == 0
                    ? "no pointer was set outside the game's screens this session"
                    : _setElsewhere + " pointer(s) were set outside the game's screens this session, the last " +
                      Shape(_setElsewhereShape) + " " + Seconds(now - _setElsewhereAt) + " seconds before, by " + _setElsewhereBy;

        private static string OtherHands()
        {
            if (_otherHands != null) return _otherHands;
            var others = new List<string>();
            OthersChanging(others, "MapScreen.HandleMouse",
                typeof(MapScreen).GetMethod("HandleMouse", BindingFlags.Instance | BindingFlags.NonPublic));
            OthersChanging(others, "MapScreen.CheckCursorState",
                typeof(MapScreen).GetMethod("CheckCursorState", BindingFlags.Instance | BindingFlags.NonPublic));
            OthersChanging(others, "ScreenManager.EarlyUpdate",
                typeof(ScreenManager).GetMethod("EarlyUpdate", BindingFlags.Static | BindingFlags.Public));
            OthersChanging(others, "ScreenManager.LateUpdate",
                typeof(ScreenManager).GetMethod("LateUpdate", BindingFlags.Static | BindingFlags.NonPublic));
            OthersChanging(others, "MouseManager.ActivateMouseCursor",
                typeof(MouseManager).GetMethod("ActivateMouseCursor", BindingFlags.Static | BindingFlags.Public));
            _otherHands = others.Count == 0
                ? "no other mod changes the game's own pointer code"
                : "the game's own pointer code: " + string.Join("; ", others.ToArray());
            return _otherHands;
        }

        private static void OthersChanging(List<string> others, string what, MethodBase method)
        {
            if (method == null)
            {
                others.Add(what + " could not be found on this game version");
                return;
            }
            Patches found = Harmony.GetPatchInfo(method);
            if (found?.Owners == null) return;
            var owners = new List<string>();
            foreach (string owner in found.Owners)
                if (owner != SubModule.HarmonyId && !owners.Contains(owner)) owners.Add(owner);
            if (owners.Count > 0) others.Add(what + " is changed by " + string.Join(", ", owners.ToArray()));
        }

        private static string TheHotkey(float now) =>
            "the ledger panel hotkey " + LedgerPanel.PanelKey() +
            (_hotkeyAt < 0f ? " not let go on the campaign map yet" : " last let go " + Seconds(now - _hotkeyAt) + " seconds before") +
            (_windowAt < 0f ? "" : ", " + (_windowWhich ?? "a TradeLord window") + (_windowOpen ? " opened " : " closed ") +
                                   Seconds(now - _windowAt) + " seconds before" + (_windowHow == null ? "" : ", " + _windowHow));

        private static bool EngineShowsTheMouse() =>
            ScreenManager.IsMouseCursorActive() || ScreenManager.IsMouseCursorHidden();

        private static List<string> AQuickRightClick(MapScreen map, float held)
        {
            float now = TaleWorlds.Engine.Time.ApplicationTime;
            return new List<string>
            {
                "map cursor: a quick right click of " + Seconds(held, "0.00") + " s at " + Spot(Input.MousePositionPixel) +
                    " with TradeLord's windows closed; in the second before it " + _beforeRight,
                "  in the second before it " + _beforeRightOnScreen,
                "  right after it the game draws " + Drawn(ScreenManager.FirstHitLayer) +
                    ", and under the mouse: " + Guard.Read("map cursor watch: under the mouse", map, Look, Unread).Said,
                "  " + TheSignLastAsked(now),
                "  " + Elsewhere(now) + "; " + OtherHands()
            };
        }

        private static List<string> WhatTheMapIsDoing(MapScreen map, ScreenLayer top, float now, Ground ground)
        {
            return new List<string>
            {
                "map cursor: the forbidden sign has stayed on for " + Seconds(now - _since) + " seconds while the mouse moved " +
                    (int)_travelled + " pixels; the game drew it from " + Named(top) +
                    ", and the campaign map's own layer asks for " + map.SceneLayer?.ActiveCursor,
                "  under the mouse: " + ground.Said,
                "  your party: " + Guard.Read("map cursor watch: your party", map, YourParty, "could not be read"),
                "  the map: " + Guard.Read("map cursor watch: the map", map, TheMap, "could not be read"),
                "  input: " + Guard.Read("map cursor watch: the input", map, WhoHasTheInput, "could not be read") +
                    ", " + TheHotkey(now)
            };
        }

        private static Ground Look(MapScreen map)
        {
            var view = map.SceneLayer?.SceneView;
            MobileParty party = MobileParty.MainParty;
            if (view == null || party == null) return new Ground { Said = "the map has no scene or no party to look from" };
            Vec3 near = Vec3.Zero, far = Vec3.Zero;
            view.TranslateMouse(ref near, ref far, -1f);
            Vec3 along = far - near;
            along.Normalize();
            Vec3 point = Vec3.Zero;
            if (view.RayCastForClosestEntityOrTerrain(near, far, out float distance, out Vec3 _))
                point = near + along * distance;
            var land = new CampaignVec2(point.AsVec2, true);
            bool onLand = land.Face.IsValid();
            CampaignVec2 spot = onLand ? land : new CampaignVec2(point.AsVec2, false);
            bool face = spot.Face.IsValid();
            bool atSea = party.IsCurrentlyAtSea;
            bool? closed = (!face || (!atSea && !onLand)) ? true : atSea ? false : WalkingIsShut(spot);
            bool reach = Helpers.NavigationHelper.CanPlayerNavigateToPosition(spot, out _);
            bool home = Helpers.NavigationHelper.CanPlayerNavigateToPosition(party.Position, out _);
            return new Ground
            {
                Said = "the ground at " + Spot(point.AsVec2) + (onLand ? " on land" : " at sea") +
                       (face ? "" : " with no path face") +
                       (closed == true ? ", which your party cannot cross"
                           : closed == false ? ", which your party can cross" : ", whose kind could not be read") +
                       ", and the game's own check says your party " + (reach ? "can" : "cannot") +
                       " travel there; the same check on the spot your party stands on says " +
                       (home ? "it can be there" : "it cannot"),
                SignIsRight = !reach && closed == true && home
            };
        }

        private static bool? WalkingIsShut(CampaignVec2 spot)
        {
            try { return Shut(spot); }
            catch { return null; }
        }

        private static bool Shut(CampaignVec2 spot)
        {
            int[] shut = Campaign.Current.Models.PartyNavigationModel
                .GetInvalidTerrainTypesForNavigationType(MobileParty.NavigationType.Default);
            return shut != null && Array.IndexOf(shut, spot.Face.FaceGroupIndex) >= 0;
        }

        private static string YourParty(MapScreen map)
        {
            MobileParty party = MobileParty.MainParty;
            if (party == null) return "none";
            return (party.IsCurrentlyAtSea ? "at sea" : "on land") + " at " + Spot(party.Position.ToVec2()) +
                   (party.CurrentSettlement != null ? ", inside " + party.CurrentSettlement.Name : "") +
                   (party.Army != null ? ", in an army" : "") +
                   (Hero.MainHero?.IsPrisoner == true ? ", held prisoner" : "") +
                   (TaleWorlds.CampaignSystem.Encounters.PlayerEncounter.Current != null ? ", in an encounter" : "");
        }

        private static string TheMap(MapScreen map)
        {
            var stopped = new List<string>();
            if (MobileParty.MainParty == null || PartyBase.MainParty?.IsValid != true) stopped.Add("your party is not counted as valid");
            if (map.MapCameraView?.CameraAnimationInProgress == true) stopped.Add("the camera is on a set move");
            if (!map.IsReady) stopped.Add("the map is not ready");
            if (Campaign.Current?.GameStarted != true) stopped.Add("the campaign has not started");
            return (stopped.Count == 0 ? "works out the cursor every frame"
                       : "has stopped working out the cursor, as " + string.Join(", ", stopped.ToArray())) +
                   (map.IsInMenu ? ", a menu is open" : "") +
                   (map.IsEscapeMenuOpened ? ", the escape menu is open" : "") +
                   (TaleWorlds.Core.GameStateManager.Current?.ActiveStateDisabledByUser == true ? ", held still by another window" : "") +
                   (ScreenManager.TopScreen == map ? "" : ", not the screen on top");
        }

        private static string WhoHasTheInput(MapScreen map)
        {
            var under = new List<string>();
            var showing = new List<string>();
            List<ScreenLayer> layers = ScreenManager.SortedLayers;
            for (int i = (layers?.Count ?? 0) - 1; i >= 0; i--)
            {
                ScreenLayer layer = layers[i];
                if (layer == null || !layer.IsActive) continue;
                if (layer.HitTest()) under.Add(layer.Name + " " + layer.ActiveCursor);
                if (layer.InputRestrictions.MouseVisibility) showing.Add(Named(layer));
            }
            ScreenLayer ours = LedgerPanel.OwnLayer;
            ScreenLayer scene = map.SceneLayer;
            return "keyboard on " + Named(ScreenManager.FocusedLayer) +
                   ", under the mouse from the top: " + (under.Count == 0 ? "nothing" : string.Join(", ", under.ToArray())) +
                   ", asking the game to show the mouse: " + (showing.Count == 0 ? "none" : string.Join(", ", showing.ToArray())) +
                   ", the game " + (ScreenManager.GetMouseVisibility() ? "shows" : "hides") + " the mouse and the engine " +
                   (EngineShowsTheMouse() ? "shows it" : "hides it") +
                   (Input.IsGamepadActive ? ", a gamepad in use" : "") +
                   ", the map's own layer " + (scene != null && scene.IsHitThisFrame ? "gets" : "does not get") +
                   " the mouse this frame" +
                   (ours == null ? ", TradeLord's layer is not on the map"
                       : ", TradeLord's layer takes " + ours.InputRestrictions.InputUsageMask +
                         (ours.InputRestrictions.MouseVisibility ? " and asks to show the mouse" : " and leaves the mouse alone"));
        }

        private static string Named(ScreenLayer layer) =>
            layer == null ? "nothing" : layer.Name + " (" + layer.GetType().Name + ", order " + layer.InputRestrictions.Order + ")";

        private static string Spot(Vec2 at) => (int)at.x + ", " + (int)at.y;

        private static string Seconds(float s, string shape = "0.0") =>
            s.ToString(shape, System.Globalization.CultureInfo.InvariantCulture);
    }

    internal static class WindowsPointer
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct Point
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PointerInfo
        {
            public int Size;
            public int Flags;
            public IntPtr Handle;
            public Point At;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Box
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ThreadInput
        {
            public int Size;
            public int Flags;
            public IntPtr Active;
            public IntPtr Focus;
            public IntPtr Capture;
            public IntPtr MenuOwner;
            public IntPtr MoveSize;
            public IntPtr Caret;
            public Box CaretBox;
        }

        private const int Showing = 1;
        private const int Unavailable = 32648;
        private const int Arrow = 32512;
        private static readonly int PointerInfoSize = Marshal.SizeOf(typeof(PointerInfo));
        private static readonly int ThreadInputSize = Marshal.SizeOf(typeof(ThreadInput));

        [DllImport("user32.dll")]
        private static extern bool GetCursorInfo(ref PointerInfo info);

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point at);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr window, out uint process);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetGUIThreadInfo(uint thread, ref ThreadInput info);

        [DllImport("user32.dll", EntryPoint = "LoadCursorW")]
        private static extern IntPtr LoadCursor(IntPtr module, IntPtr name);

        private static bool _gone;
        private static uint _game;
        private static bool _systemRead;
        private static IntPtr _unavailable;
        private static IntPtr _arrow;

        internal static bool Readable => !_gone;

        internal static bool Read(out bool over, out bool showing, out IntPtr handle)
        {
            over = false;
            showing = false;
            handle = IntPtr.Zero;
            if (_gone) return false;
            try
            {
                var info = new PointerInfo { Size = PointerInfoSize };
                if (!GetCursorInfo(ref info)) return false;
                handle = info.Handle;
                showing = (info.Flags & Showing) != 0 && handle != IntPtr.Zero;
                over = TheGames(WindowFromPoint(info.At));
                return true;
            }
            catch (Exception e)
            {
                _gone = true;
                Log.Write("map cursor watch: the pointer Windows shows cannot be read here (" + e.GetType().Name +
                          "), so TradeLord.log leaves it out");
                return false;
            }
        }

        private static bool TheGames(IntPtr window)
        {
            if (window == IntPtr.Zero) return false;
            if (_game == 0) _game = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;
            GetWindowThreadProcessId(window, out uint owner);
            return owner == _game;
        }

        internal static string Named(IntPtr handle)
        {
            if (_gone || handle == IntPtr.Zero) return null;
            try
            {
                if (!_systemRead)
                {
                    _systemRead = true;
                    _unavailable = LoadCursor(IntPtr.Zero, new IntPtr(Unavailable));
                    _arrow = LoadCursor(IntPtr.Zero, new IntPtr(Arrow));
                }
                return handle == _unavailable ? "Windows' own unavailable sign" : handle == _arrow ? "Windows' own arrow" : null;
            }
            catch { return null; }
        }

        internal static string WhoHasTheMouse()
        {
            if (_gone) return "which window has the mouse cannot be read";
            try
            {
                IntPtr front = GetForegroundWindow();
                if (!TheGames(front)) return front == IntPtr.Zero ? "no window is in front" : "another program's window is in front";
                var input = new ThreadInput { Size = ThreadInputSize };
                if (!GetGUIThreadInfo(0, ref input)) return "the game's window is in front";
                return "the game's window is in front and " + (input.Capture == IntPtr.Zero ? "does not hold" : "holds") +
                       " the mouse capture";
            }
            catch { return "which window has the mouse cannot be read"; }
        }
    }

    [HarmonyPatch(typeof(ScreenManager), "EarlyUpdate")]
    internal static class Patch_ThePointerTheEngineIsTold
    {
        private static void Prefix() => CursorWatch.BeforeTheScreens();

        private static void Postfix() => CursorWatch.AfterTheScreens();
    }

    [HarmonyPatch(typeof(MouseManager), "ActivateMouseCursor")]
    internal static class Patch_APointerSetElsewhere
    {
        private static void Prefix(CursorType __0) => CursorWatch.SetElsewhere(__0);
    }
}
