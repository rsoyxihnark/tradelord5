using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TradeLord
{
    public class PriceObservation
    {
        public string ItemId;
        public string TownId;
        public int BuyPrice;
        public int SellPrice;
        public float CapturedDay;
    }

    public class PurchaseRecord
    {
        public string ItemId;
        public int TotalPaid;
        public int Count;
        public int LastUnitPaid;
    }

    public static class LedgerCodec
    {
        private const char FieldMark = '|';
        private const char RecordMark = ';';

        private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

        private static string Number(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        private static bool Whole(string text, out int value) =>
            int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        private static bool Storable(string id) =>
            !string.IsNullOrEmpty(id) && id.IndexOf(FieldMark) < 0 && id.IndexOf(RecordMark) < 0;

        private static bool Storable(float day) =>
            !float.IsNaN(day) && !float.IsInfinity(day);

        public static string WriteLedger(Dictionary<string, List<PriceObservation>> ledger)
        {
            var sb = new StringBuilder();
            if (ledger == null) return sb.ToString();
            foreach (var kv in ledger)
            {
                if (!Storable(kv.Key) || kv.Value == null) continue;
                for (int i = 0; i < kv.Value.Count; i++)
                {
                    PriceObservation o = kv.Value[i];
                    if (o == null || !Storable(o.TownId) || !Storable(o.CapturedDay)) continue;
                    if (sb.Length > 0) sb.Append(RecordMark);
                    sb.Append(kv.Key).Append(FieldMark)
                      .Append(o.TownId).Append(FieldMark)
                      .Append(Number(o.BuyPrice)).Append(FieldMark)
                      .Append(Number(o.SellPrice)).Append(FieldMark)
                      .Append(Number(o.CapturedDay));
                }
            }
            return sb.ToString();
        }

        public static Dictionary<string, List<PriceObservation>> ReadLedger(string text)
        {
            var book = new Dictionary<string, List<PriceObservation>>();
            if (string.IsNullOrEmpty(text)) return book;
            string[] records = text.Split(RecordMark);
            for (int i = 0; i < records.Length; i++)
            {
                string[] parts = records[i].Split(FieldMark);
                if (parts.Length != 5 || !Storable(parts[0]) || !Storable(parts[1])) continue;
                if (!Whole(parts[2], out int buy) || !Whole(parts[3], out int sell)) continue;
                if (!float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float day)) continue;
                if (!Storable(day)) continue;
                if (!book.TryGetValue(parts[0], out var list))
                {
                    list = new List<PriceObservation>();
                    book[parts[0]] = list;
                }
                list.Add(new PriceObservation
                {
                    ItemId = parts[0], TownId = parts[1], BuyPrice = buy, SellPrice = sell, CapturedDay = day
                });
            }
            return book;
        }

        public static string WritePurchases(List<PurchaseRecord> purchases)
        {
            var sb = new StringBuilder();
            if (purchases == null) return sb.ToString();
            for (int i = 0; i < purchases.Count; i++)
            {
                PurchaseRecord rec = purchases[i];
                if (rec == null || !Storable(rec.ItemId) || rec.Count <= 0) continue;
                if (sb.Length > 0) sb.Append(RecordMark);
                sb.Append(rec.ItemId).Append(FieldMark)
                  .Append(Number(rec.TotalPaid)).Append(FieldMark)
                  .Append(Number(rec.Count)).Append(FieldMark)
                  .Append(Number(rec.LastUnitPaid));
            }
            return sb.ToString();
        }

        public static List<PurchaseRecord> ReadPurchases(string text)
        {
            var kept = new List<PurchaseRecord>();
            if (string.IsNullOrEmpty(text)) return kept;
            string[] records = text.Split(RecordMark);
            for (int i = 0; i < records.Length; i++)
            {
                string[] parts = records[i].Split(FieldMark);
                if (parts.Length != 4 || !Storable(parts[0])) continue;
                if (!Whole(parts[1], out int total) || !Whole(parts[2], out int count) ||
                    !Whole(parts[3], out int last)) continue;
                if (count <= 0) continue;
                kept.Add(new PurchaseRecord
                {
                    ItemId = parts[0], TotalPaid = total, Count = count, LastUnitPaid = last
                });
            }
            return kept;
        }
    }
}
