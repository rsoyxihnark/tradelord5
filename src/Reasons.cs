using System.Collections.Generic;
using System.Text;
using TaleWorlds.Localization;

namespace TradeLord
{
    internal sealed class BlockTally
    {
        private readonly Dictionary<Block, int> _counts = new Dictionary<Block, int>();
        private Block _firstGuard = Block.None;

        internal void Note(Block reason)
        {
            if (reason == Block.None) return;
            if (_firstGuard == Block.None && Guarded(reason)) _firstGuard = reason;
            _counts.TryGetValue(reason, out int seen);
            _counts[reason] = seen + 1;
        }

        internal bool Any => _counts.Count > 0;

        internal bool Saw(Block reason) => _counts.ContainsKey(reason);

        private static bool Structural(Block reason) =>
            reason == Block.NotTradable || reason == Block.NotMerchandise ||
            reason == Block.MountOrHaulAnimal;

        private static bool Guarded(Block reason) =>
            reason == Block.NeverList || reason == Block.Locked || reason == Block.Protected ||
            reason == Block.QuestGoods || reason == Block.FoodReserve;

        internal Block Dominant()
        {
            Block top = Block.None;
            int best = 0;
            foreach (var kv in _counts)
                if (!Structural(kv.Key) && kv.Key != Block.BudgetSpent &&
                    (kv.Value > best || (kv.Value == best && kv.Key < top)))
                { best = kv.Value; top = kv.Key; }
            if (top == Block.None && Saw(Block.BudgetSpent)) return Block.BudgetSpent;
            return Guarded(top) ? _firstGuard : top;
        }

        internal string Summary()
        {
            var order = new List<KeyValuePair<Block, int>>(_counts);
            order.Sort((x, y) =>
            {
                int byCount = y.Value.CompareTo(x.Value);
                return byCount != 0 ? byCount : x.Key.CompareTo(y.Key);
            });
            var sb = new StringBuilder();
            foreach (var kv in order)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(kv.Key).Append("=").Append(kv.Value);
            }
            return sb.ToString();
        }

        internal static TextObject Phrase(Block reason)
        {
            switch (reason)
            {
                case Block.CategoryPolicy:
                    return Tongue.Text("{=TL41}your category policy excludes it");
                case Block.BelowMargin:
                    return Tongue.Text("{=TL42}prices here miss your margin");
                case Block.BelowBestMarket:
                    return Tongue.Text("{=TL85}you are holding this cargo for a better market");
                case Block.BudgetSpent:
                    return Tongue.Text("{=TL43}your purse or spending caps are spent");
                case Block.ItemCountCap:
                case Block.ItemValueCap:
                    return Tongue.Text("{=TL391}you have bought as many of these as your buy cap per item allows");
                case Block.CarryWeight:
                    return Tongue.Text("{=TL44}there is no room to carry more");
                case Block.HerdFull:
                    return Tongue.Text("{=TL86}your party cannot drive any more livestock");
                case Block.MerchantTillEmpty:
                    return Tongue.Text("{=TL46}the merchant has run out of gold");
                case Block.NoResaleMarket:
                    return Tongue.Text("{=TL47}there is nowhere in reach to resell it");
                case Block.TradedHereAlready:
                    return Tongue.Text("{=TL48}you already traded these on this visit");
                case Block.VillageLastUnit:
                    return Tongue.Text("{=TL83}the village is down to its last of each good");
                case Block.HeldEnough:
                    return Tongue.Text("{=TL93}you are already carrying as many of these as you allow");
                case Block.Smeltable:
                    return Tongue.Text("{=TL99}you are keeping what the smithy can break down");
                case Block.NeverList:
                    return Tongue.Text("{=TL381}it is on your never-sell or never-buy list");
                case Block.GrainSwitch:
                    return Tongue.Text("{=TL388}grain is left alone, since it fills the cargo for little return");
                case Block.Locked:
                    return Tongue.Text("{=TL382}it is locked in your inventory");
                case Block.Protected:
                    return Tongue.Text("{=TL383}your unique and crafted protection holds it");
                case Block.QuestGoods:
                    return Tongue.Text("{=TL384}a quest of yours may be waiting on it");
                case Block.FoodReserve:
                    return Tongue.Text("{=TL386}your food reserve holds it back");
                default:
                    return Tongue.Text("{=TL45}this market has nothing worth trading");
            }
        }
    }
}
