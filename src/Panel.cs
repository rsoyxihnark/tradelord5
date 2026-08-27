using System;
using System.Collections.Generic;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
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

        [DataSourceProperty] public bool Tier1 => _rank < 0.2f;
        [DataSourceProperty] public bool Tier2 => _rank >= 0.2f && _rank < 0.45f;
        [DataSourceProperty] public bool Tier3 => _rank >= 0.45f && _rank < 0.7f;
        [DataSourceProperty] public bool Tier4 => _rank >= 0.7f && _rank < 0.85f;
        [DataSourceProperty] public bool Tier5 => _rank >= 0.85f;
        [DataSourceProperty] public string ItemName => _route.Item.Name.ToString();
        [DataSourceProperty] public string BuyTownName => _route.From.Name.ToString();
        [DataSourceProperty] public string BuyPrice => _route.BuyPrice.ToString();
        [DataSourceProperty] public string SellTownName => _route.To.Name.ToString();
        [DataSourceProperty] public string SellPrice => _route.SellPrice.ToString();
        [DataSourceProperty] public string Quantity => "x" + _route.Quantity;
        [DataSourceProperty] public string Profit => "+" + _route.TotalProfit;
        [DataSourceProperty] public string Days => "~" + _route.TravelDays.ToString("0.#");

        [DataSourceProperty] public string DataAge
        {
            get
            {
                if (_route.DataAgeDays < 0f) return new TextObject("{=TL74}live").ToString();
                if (_route.DataAgeDays < 1f) return new TextObject("{=TL75}today").ToString();
                TextObject age = new TextObject("{=TL76}{DAYS}d old");
                age.SetTextVariable("DAYS", (int)Math.Ceiling(_route.DataAgeDays));
                return age.ToString();
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
        public WorkshopRowVM(string name, string profit, string owner)
        {
            Name = name; Profit = profit; Owner = owner;
        }

        [DataSourceProperty] public string Name { get; }
        [DataSourceProperty] public string Profit { get; }
        [DataSourceProperty] public string Owner { get; }
    }

    public class LedgerPanelVM : ViewModel
    {
        private readonly Action _onClose;
        private readonly Action _onOpen;
        private readonly Action<Settlement> _centerMap;
        private bool _isVisible;
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
                IsMapButtonVisible = Options.Current.ShowMapButton && !value;
            }
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

        [DataSourceProperty] public string BrandLabel => "TradeLord";
        [DataSourceProperty] public string TitleLabel => new TextObject("{=TL07}TradeLord ledger").ToString();
        [DataSourceProperty] public string RefreshLabel => new TextObject("{=TL62}Refresh").ToString();
        [DataSourceProperty] public string CloseLabel => new TextObject("{=TL09}Close").ToString();
        [DataSourceProperty] public string HeadItem => new TextObject("{=TL50}Item").ToString();
        [DataSourceProperty] public string HeadBuyTown => new TextObject("{=TL51}Buy From").ToString();
        [DataSourceProperty] public string HeadPrice => new TextObject("{=TL52}Price").ToString();
        [DataSourceProperty] public string HeadSellTown => new TextObject("{=TL53}Sell At").ToString();
        [DataSourceProperty] public string HeadQuantity => new TextObject("{=TL54}Qty").ToString();
        [DataSourceProperty] public string HeadProfit => new TextObject("{=TL55}Profit").ToString();
        [DataSourceProperty] public string HeadDays => new TextObject("{=TL56}Days").ToString();
        [DataSourceProperty] public string HeadData => new TextObject("{=TL57}Data").ToString();
        [DataSourceProperty] public string HeadCaravans => new TextObject("{=TL58}Carv.").ToString();
        [DataSourceProperty] public string HeadConfidence => new TextObject("{=TL59}Conf").ToString();
        [DataSourceProperty] public string HeadScore => new TextObject("{=TL60}Score").ToString();
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

        public void ExecuteOpenPanel() => _onOpen?.Invoke();

        public void ExecuteRefresh() => Guard.Run("Panel.Refresh", () =>
        {
            LedgerBehavior.Instance?.ForgetMarketRankings();
            Refresh();
        });

        private static string Line(string template, string key, string value)
        {
            TextObject t = new TextObject(template);
            t.SetTextVariable(key, value);
            return t.ToString();
        }

        private static string Line(string template, string k1, string v1, string k2, string v2)
        {
            TextObject t = new TextObject(template);
            t.SetTextVariable(k1, v1);
            t.SetTextVariable(k2, v2);
            return t.ToString();
        }

        private void Refresh()
        {
            var hero = Hero.MainHero;
            var party = MobileParty.MainParty;
            PlayerGold = Line("{=TL63}Gold: {AMOUNT}", "AMOUNT", (hero?.Gold ?? 0).ToString("N0"));
            CapacityText = party == null ? ""
                : Line("{=TL64}Cargo: {CARRIED} / {CAPACITY}", "CARRIED",
                       ((int)party.TotalWeightCarried).ToString(), "CAPACITY",
                       ((int)party.InventoryCapacity).ToString());
            SpeedText = party == null ? "" : Line("{=TL65}Speed: {SPEED}", "SPEED", party.Speed.ToString("0.0"));
            int lifetime = LedgerBehavior.Instance?.LifetimeProfit ?? 0;
            LifetimeText = Line("{=TL66}TradeLord profit: {AMOUNT}", "AMOUNT",
                                (lifetime >= 0 ? "+" : "") + lifetime.ToString("N0"));

            var rows = new MBBindingList<RouteRowVM>();
            var routes = LedgerBehavior.Instance?.BestRoutes(30);
            if (routes != null)
                for (int i = 0; i < routes.Count; i++)
                    rows.Add(new RouteRowVM(routes[i], i % 2 == 1,
                        routes.Count <= 1 ? 0f : (float)i / (routes.Count - 1), _centerMap));
            Routes = rows;
            bool empty = rows.Count == 0;
            StatusText = empty
                ? new TextObject("{=TL67}No profitable routes in reach").ToString()
                : Line("{=TL68}{COUNT} profitable routes, best first", "COUNT", rows.Count.ToString());
            LegendText = empty
                ? new TextObject("{=TL69}No routes are within your travel ceilings yet. Visit more markets, or raise the ceilings in the Knowledge settings.").ToString()
                : new TextObject("{=TL70}Click a town name to jump to it and pin or unpin it | Days = you -> buy town -> sell town | Carv. = caravans at those towns | Price is the first unit's; Profit prices every unit in turn, so it is less than price x qty | Conf* = flat quote, not priced per unit").ToString()
                  + (Options.Current.ConfidenceRanking
                        ? new TextObject("{=TL71} | Score = profit per day discounted by Conf").ToString()
                        : new TextObject("{=TL72} | Score = profit per day").ToString())
                  + (Options.Current.ConservativeRouteProjection
                        ? new TextObject("{=TL73} | resale safety factor applied").ToString() : "");

            RefreshWorkshops();
        }

        private void RefreshWorkshops()
        {
            bool ownedOnly = !Options.Current.Omniscient;
            WorkshopsHeader = ownedOnly
                ? new TextObject("{=TL80}Your workshops (recent profit)").ToString()
                : new TextObject("{=TL61}Most profitable workshops (recent profit)").ToString();
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

            if (!_vm.IsVisible)
            {
                bool button = Options.Current.ShowMapButton;
                if (_vm.IsMapButtonVisible != button)
                    _vm.IsMapButtonVisible = button;
                UpdateIdleInput(button);
                if (!map.IsEscapeMenuOpened && HotkeyReleased())
                    Guard.Run("Panel.Show", Show);
            }
            else if (map.IsEscapeMenuOpened || HotkeyReleased())
            {
                Hide();
            }
        }

        private static bool _idleMouseActive;

        private static void UpdateIdleInput(bool buttonOn)
        {
            Vec2 m = Input.MousePositionRanged;
            bool wantMouse = buttonOn && m.x >= 0.90f && m.y >= 0.42f && m.y <= 0.58f;
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
            for (int i = 0; i < parts.Length - 1; i++)
                if (ParseModifier(parts[i].Trim(), out var pair)) _modifiers.Add(pair);
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
            _layer = new GauntletLayer("TradeLordPanel", 250);
            _movie = _layer.LoadMovie("TradeLordPanel", _vm);
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
            if (_panelPins.Remove(settlement))
            {
                if (tracker.CheckTracked(settlement)) tracker.RemoveTrackedObject(settlement);
                return;
            }
            _panelPins.Add(settlement);
            if (!tracker.CheckTracked(settlement)) tracker.RegisterObject(settlement);
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
            foreach (string id in ids.Split('|'))
            {
                Settlement s = Settlement.Find(id);
                if (s != null) _panelPins.Add(s);
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
        }

        internal static void Cleanup()
        {
            MapScreen map = _mapScreen;
            GauntletLayer layer = _layer;
            GauntletMovieIdentifier movie = _movie;
            LedgerPanelVM vm = _vm;
            _mapScreen = null; _layer = null; _movie = null; _vm = null;
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
