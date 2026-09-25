using System.Collections.Generic;

namespace TradeLord
{
    internal sealed class Books
    {
        private readonly Dictionary<string, int> _held = new Dictionary<string, int>();
        private readonly Dictionary<string, (int count, int spent)> _dryBought =
            new Dictionary<string, (int, int)>();
        private readonly HashSet<string> _drySold = new HashSet<string>();
        private readonly Dictionary<string, int> _dryDrawn = new Dictionary<string, int>();
        private readonly Dictionary<string, (int count, int spent)> _bought =
            new Dictionary<string, (int, int)>();
        private readonly HashSet<string> _sold = new HashSet<string>();

        private int _paid;
        private int _spent;
        private int _gained;
        private int _drawn;
        private int _shed;
        private int _mounts;
        private int _hauls;
        private int _herd;
        private int _food;
        private float _weight;
        private float _capacity;
        private int _moves;
        private int _dryMoves;

        internal bool LaidOut;

        private bool OnPaper(bool sim) => sim && !LaidOut;

        internal void Forget()
        {
            ForgetTheDryRun();
            _bought.Clear();
            _sold.Clear();
            _paid = 0;
            _moves = 0;
        }

        internal void ForgetTheDryRun()
        {
            _held.Clear();
            _dryBought.Clear();
            _drySold.Clear();
            _dryDrawn.Clear();
            _spent = 0;
            _gained = 0;
            _drawn = 0;
            _shed = 0;
            _mounts = 0;
            _hauls = 0;
            _herd = 0;
            _food = 0;
            _weight = 0f;
            _capacity = 0f;
            _dryMoves = 0;
        }

        internal int PaidOut(bool sim) => _paid + (sim ? _spent : 0);

        internal int Purse(bool sim) => sim ? _gained - _spent : 0;

        internal int TillDrawn(bool sim) => sim ? _drawn : 0;

        internal float Weight(bool sim) => OnPaper(sim) ? _weight : 0f;

        internal float CapacityAdded(bool sim) => OnPaper(sim) ? _capacity : 0f;

        internal int FoodHeld(bool sim) => OnPaper(sim) ? _food : 0;

        internal int Shed(bool sim) => OnPaper(sim) ? _shed : 0;

        internal int MountsShed(bool sim) => OnPaper(sim) ? _mounts : 0;

        internal int HaulsShed(bool sim) => OnPaper(sim) ? _hauls : 0;

        internal int HerdTaken(bool sim) => OnPaper(sim) ? _herd : 0;

        internal int Moves(bool sim) => _moves + (sim ? _dryMoves : 0);

        internal bool Traded(bool sim) =>
            _sold.Count > 0 || _bought.Count > 0 ||
            (sim && (_drySold.Count > 0 || _dryBought.Count > 0));

        internal int Held(bool sim, string id) =>
            OnPaper(sim) && id != null && _held.TryGetValue(id, out int units) ? units : 0;

        internal int Stocked(bool sim, string id) =>
            OnPaper(sim) && id != null && _dryBought.TryGetValue(id, out var prior) ? prior.count : 0;

        internal int PaidDrawn(bool sim, string id) =>
            sim && id != null && _dryDrawn.TryGetValue(id, out int units) ? units : 0;

        internal bool Sold(bool sim, string id) =>
            id != null && (_sold.Contains(id) || (sim && _drySold.Contains(id)));

        internal bool Bought(bool sim, string id) =>
            id != null && (_bought.ContainsKey(id) || (sim && _dryBought.ContainsKey(id)));

        internal (int count, int spent) Purchases(bool sim, string id)
        {
            if (id == null) return (0, 0);
            _bought.TryGetValue(id, out var real);
            if (!sim || !_dryBought.TryGetValue(id, out var dry)) return real;
            return (real.count + dry.count, real.spent + dry.spent);
        }

        internal void NoteADealTaken(int units)
        {
            if (units > 0) _moves++;
        }

        internal void NoteSold(string id)
        {
            if (id == null) return;
            _sold.Add(id);
            _moves++;
        }

        internal void NotePaidDrawn(string id)
        {
            if (id == null) return;
            _dryDrawn.TryGetValue(id, out int units);
            _dryDrawn[id] = units + 1;
        }

        internal void NoteBought(string id, int price)
        {
            if (id == null) return;
            _moves++;
            _paid += price;
            _bought.TryGetValue(id, out var prior);
            _bought[id] = (prior.count + 1, prior.spent + price);
        }

        internal void NoteSale(string id, int price, float weight, int foodValue)
        {
            if (id == null) return;
            _dryMoves++;
            _drySold.Add(id);
            _gained += price;
            _drawn += price;
            _weight -= weight;
            _food -= foodValue;
            _held.TryGetValue(id, out int units);
            _held[id] = units - 1;
        }

        internal void NotePurchase(string id, int price, float weight, int foodValue)
        {
            if (id == null) return;
            _dryMoves++;
            _spent += price;
            _drawn -= price;
            _weight += weight;
            _food += foodValue;
            _held.TryGetValue(id, out int units);
            _held[id] = units + 1;
            _dryBought.TryGetValue(id, out var prior);
            _dryBought[id] = (prior.count + 1, prior.spent + price);
        }

        internal void NoteShed(bool haulAnimal, bool spareMount)
        {
            _shed++;
            if (haulAnimal) _hauls++;
            else if (spareMount) _mounts++;
        }

        internal void NoteHerdTaken() => _herd++;

        internal void NoteCapacityAdded(float carries)
        {
            if (TradeMath.Finite(carries, 0f) > 0f) _capacity += carries;
        }
    }
}
