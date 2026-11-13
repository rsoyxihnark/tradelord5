using System;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace TradeLord.TestAutopilot
{
    public class AutopilotButtonVM : ViewModel
    {
        private bool _isOn;
        private string _label = "";

        public AutopilotButtonVM() => Show(false);

        internal void Show(bool on)
        {
            IsOn = on;
            Label = on ? "Autopilot ON" : "Autopilot OFF";
        }

        public void ExecuteToggle() => Pilot.Toggle();

        [DataSourceProperty]
        public bool IsOn
        {
            get => _isOn;
            set { if (value != _isOn) { _isOn = value; OnPropertyChangedWithValue(value, "IsOn"); } }
        }

        [DataSourceProperty]
        public string Label
        {
            get => _label;
            set { if (value != _label) { _label = value; OnPropertyChangedWithValue(value, "Label"); } }
        }
    }

    internal static class Button
    {
        private static MapScreen _map;
        private static GauntletLayer _layer;
        private static AutopilotButtonVM _vm;
        private static bool _dead;

        internal static void Tick()
        {
            if (_dead) return;
            try
            {
                MapScreen map = ScreenManager.TopScreen as MapScreen;
                if (map != null && _map == null && Campaign.Current != null) Setup(map);
                if (_map != null && map != null && map != _map) Reset();
                _vm?.Show(Pilot.On);
            }
            catch (Exception e)
            {
                _dead = true;
                Trail.Error(e, "the Autopilot button (it is gone for this session; nothing else is touched)");
                try { Reset(); } catch { }
            }
        }

        private static void Setup(MapScreen map)
        {
            _map = map;
            _vm = new AutopilotButtonVM();
            _layer = new GauntletLayer("TradeLordTestAutopilot", 260);
            _layer.LoadMovie("TradeLordTestAutopilot", _vm);
            _layer.InputRestrictions.SetInputRestrictions(false, InputUsageMask.MouseButtons);
            _map.AddLayer(_layer);
            _vm.Show(Pilot.On);
        }

        internal static void Reset()
        {
            if (_map != null && _layer != null)
            {
                try { _map.RemoveLayer(_layer); } catch { }
            }
            _vm?.OnFinalize();
            _map = null;
            _layer = null;
            _vm = null;
        }
    }
}
