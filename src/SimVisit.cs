using System.Collections.Generic;

namespace TradeLord
{
    internal static class SimVisit
    {
        private static readonly Dictionary<string, int> _held = new Dictionary<string, int>();
        private static readonly Dictionary<string, (int count, int spent)> _bought =
            new Dictionary<string, (int, int)>();
        private static readonly HashSet<string> _sold = new HashSet<string>();

        private static int _spent;
        private static int _gained;
        private static int _drawn;
        private static int _shed;
        private static int _mounts;
        private static int _hauls;
        private static int _herd;
        private static int _food;
        private static float _weight;

        internal static void Forget()
        {
            _held.Clear();
            _bought.Clear();
            _sold.Clear();
            _spent = 0;
            _gained = 0;
            _drawn = 0;
            _shed = 0;
            _mounts = 0;
            _hauls = 0;
            _herd = 0;
            _food = 0;
            _weight = 0f;
        }

        internal static int Spent(bool sim) => sim ? _spent : 0;

        internal static int Purse(bool sim) => sim ? _gained - _spent : 0;

        internal static int TillDrawn(bool sim) => sim ? _drawn : 0;

        internal static float Weight(bool sim) => sim ? _weight : 0f;

        internal static int FoodHeld(bool sim) => sim ? _food : 0;

        internal static int Shed(bool sim) => sim ? _shed : 0;

        internal static int MountsShed(bool sim) => sim ? _mounts : 0;

        internal static int HaulsShed(bool sim) => sim ? _hauls : 0;

        internal static int HerdTaken(bool sim) => sim ? _herd : 0;

        internal static bool Traded(bool sim) => sim && (_sold.Count > 0 || _bought.Count > 0);

        internal static int Held(bool sim, string id) =>
            sim && id != null && _held.TryGetValue(id, out int units) ? units : 0;

        internal static int Stocked(bool sim, string id) =>
            sim && id != null && _bought.TryGetValue(id, out var prior) ? prior.count : 0;

        internal static bool Sold(bool sim, string id) => sim && id != null && _sold.Contains(id);

        internal static bool Bought(bool sim, string id) => sim && id != null && _bought.ContainsKey(id);

        internal static (int count, int spent) Purchases(bool sim, string id) =>
            sim && id != null && _bought.TryGetValue(id, out var prior) ? prior : (0, 0);

        internal static void NoteSale(string id, int price, float weight, int foodValue)
        {
            if (id == null) return;
            _sold.Add(id);
            _gained += price;
            _drawn += price;
            _weight -= weight;
            _food -= foodValue;
            _held.TryGetValue(id, out int units);
            _held[id] = units - 1;
        }

        internal static void NotePurchase(string id, int price, float weight, int foodValue)
        {
            if (id == null) return;
            _spent += price;
            _weight += weight;
            _food += foodValue;
            _held.TryGetValue(id, out int units);
            _held[id] = units + 1;
            _bought.TryGetValue(id, out var prior);
            _bought[id] = (prior.count + 1, prior.spent + price);
        }

        internal static void NoteShed(bool haulAnimal, bool spareMount)
        {
            _shed++;
            if (haulAnimal) _hauls++;
            else if (spareMount) _mounts++;
        }

        internal static void NoteHerdTaken() => _herd++;
    }
}
