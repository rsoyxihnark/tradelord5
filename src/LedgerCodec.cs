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
        public int WasBuyPrice;
        public int WasSellPrice;
        public float WasDay = NoEarlierReading;

        public const float NoEarlierReading = -1f;

        public bool SeenBefore => WasDay >= 0f;
    }

    public struct Batch
    {
        public int Unit;
        public int Count;
    }

    public struct TradeXpWaiting
    {
        public long Made;
        public long Allowed;
    }

    public class PurchaseRecord
    {
        public string ItemId;
        public int TotalPaid;
        public int Count;
        public int LastUnitPaid;
        public List<Batch> Batches = new List<Batch>();
    }

    public class PromiseRecord
    {
        public string TownId;
        public int Scored;
        public float Held;
    }

    public struct TradeNote
    {
        public string Where;
        public string What;
        public int Gold;
        public float Day;
    }

    public static class LedgerCodec
    {
        public const int FieldsAPriceNeeds = 5;

        public const int FieldsAPriceIsWrittenIn = 8;

        public const int FieldsAPurchaseNeeds = 4;

        public const int FieldsAPromiseNeeds = 3;

        public const int FieldsATradeNeeds = 4;

        public const int FieldsAWaitNeeds = 3;

        private const char FieldMark = '|';
        private const char RecordMark = ';';
        private const char BatchMark = ',';
        private const char BatchFieldMark = ':';

        public const char QualityMark = '@';

        public static string PaidKey(string itemId, string qualityId) =>
            string.IsNullOrEmpty(qualityId) ? itemId : itemId + QualityMark + qualityId;

        private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

        private static string Number(long value) => value.ToString(CultureInfo.InvariantCulture);

        private static string Number(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        private static bool Whole(string text, out int value) =>
            int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        private static bool Storable(string id) =>
            !string.IsNullOrEmpty(id) && id.IndexOf(FieldMark) < 0 && id.IndexOf(RecordMark) < 0;

        private static bool Storable(float day) =>
            !float.IsNaN(day) && !float.IsInfinity(day);

        private static string OneField(string said)
        {
            if (string.IsNullOrEmpty(said)) return "";
            return said.IndexOf(FieldMark) < 0 && said.IndexOf(RecordMark) < 0
                ? said
                : said.Replace(FieldMark, ' ').Replace(RecordMark, ' ');
        }

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
                      .Append(Number(o.CapturedDay)).Append(FieldMark)
                      .Append(Number(o.WasBuyPrice)).Append(FieldMark)
                      .Append(Number(o.WasSellPrice)).Append(FieldMark)
                      .Append(Number(o.WasDay));
                }
            }
            return sb.ToString();
        }

        public static Dictionary<string, List<PriceObservation>> ReadLedger(string text) =>
            ReadLedger(text, out _);

        public static Dictionary<string, List<PriceObservation>> ReadLedger(string text,
                                                                           out int unreadable)
        {
            var book = new Dictionary<string, List<PriceObservation>>();
            unreadable = 0;
            if (string.IsNullOrEmpty(text)) return book;
            string[] records = text.Split(RecordMark);
            for (int i = 0; i < records.Length; i++)
            {
                if (records[i].Length == 0) continue;
                string[] parts = records[i].Split(FieldMark);
                if (parts.Length < FieldsAPriceNeeds) { unreadable++; continue; }
                if (!Storable(parts[0]) || !Storable(parts[1])) { unreadable++; continue; }
                if (!Whole(parts[2], out int buy) || !Whole(parts[3], out int sell)) { unreadable++; continue; }
                if (!float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float day)) { unreadable++; continue; }
                if (!Storable(day)) { unreadable++; continue; }
                int wasBuy = 0, wasSell = 0;
                float wasDay = PriceObservation.NoEarlierReading;
                if (parts.Length >= FieldsAPriceIsWrittenIn && Whole(parts[5], out int earlierBuy) &&
                    Whole(parts[6], out int earlierSell) &&
                    float.TryParse(parts[7], NumberStyles.Float, CultureInfo.InvariantCulture,
                                   out float earlierDay) && Storable(earlierDay))
                {
                    wasBuy = earlierBuy;
                    wasSell = earlierSell;
                    wasDay = earlierDay;
                }
                if (!book.TryGetValue(parts[0], out var list))
                {
                    list = new List<PriceObservation>();
                    book[parts[0]] = list;
                }
                list.Add(new PriceObservation
                {
                    ItemId = parts[0], TownId = parts[1], BuyPrice = buy, SellPrice = sell,
                    CapturedDay = day, WasBuyPrice = wasBuy, WasSellPrice = wasSell, WasDay = wasDay
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
                if (rec.Batches == null || rec.Batches.Count == 0 || !TradeMath.BatchesAddUp(rec)) continue;
                sb.Append(FieldMark);
                for (int b = 0; b < rec.Batches.Count; b++)
                {
                    if (b > 0) sb.Append(BatchMark);
                    sb.Append(Number(rec.Batches[b].Unit)).Append(BatchFieldMark)
                      .Append(Number(rec.Batches[b].Count)).Append(BatchFieldMark)
                      .Append(Number(0));
                }
            }
            return sb.ToString();
        }

        public static List<Batch> ReadBatches(string text, int count)
        {
            var kept = new List<Batch>();
            if (string.IsNullOrEmpty(text) || count <= 0) return kept;
            long units = 0L;
            string[] batches = text.Split(BatchMark);
            if (batches.Length > TradeMath.MostBatchesKept) return kept;
            for (int i = 0; i < batches.Length; i++)
            {
                string[] parts = batches[i].Split(BatchFieldMark);
                if (parts.Length < 2 || !Whole(parts[0], out int unit) || !Whole(parts[1], out int many) ||
                    unit < 0 || many <= 0)
                    return new List<Batch>();
                units += many;
                kept.Add(new Batch { Unit = unit, Count = many });
            }
            if (units != count) return new List<Batch>();
            kept.Sort((x, y) => x.Unit.CompareTo(y.Unit));
            return kept;
        }

        public static string WritePromises(List<PromiseRecord> promises)
        {
            var sb = new StringBuilder();
            if (promises == null) return sb.ToString();
            for (int i = 0; i < promises.Count; i++)
            {
                PromiseRecord rec = promises[i];
                if (rec == null || !Storable(rec.TownId) || rec.Scored <= 0) continue;
                if (!Storable(rec.Held)) continue;
                if (sb.Length > 0) sb.Append(RecordMark);
                sb.Append(rec.TownId).Append(FieldMark)
                  .Append(Number(rec.Scored)).Append(FieldMark)
                  .Append(Number(rec.Held)).Append(FieldMark)
                  .Append(KeptUnderTheCap);
            }
            return sb.ToString();
        }

        public const string KeptUnderTheCap = "1";

        public static List<PromiseRecord> ReadPromises(string text) => ReadPromises(text, out _);

        public static List<PromiseRecord> ReadPromises(string text, out int setAside)
        {
            setAside = 0;
            var kept = new List<PromiseRecord>();
            if (string.IsNullOrEmpty(text)) return kept;
            string[] records = text.Split(RecordMark);
            for (int i = 0; i < records.Length; i++)
            {
                string[] parts = records[i].Split(FieldMark);
                if (parts.Length < FieldsAPromiseNeeds || !Storable(parts[0])) continue;
                if (!Whole(parts[1], out int scored) || scored <= 0) continue;
                if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture,
                                    out float held) || !Storable(held) || held < 0f) continue;
                if (parts.Length <= FieldsAPromiseNeeds || parts[FieldsAPromiseNeeds] != KeptUnderTheCap || held > scored)
                {
                    setAside++;
                    continue;
                }
                kept.Add(new PromiseRecord { TownId = parts[0], Scored = scored, Held = held });
            }
            return kept;
        }

        public static string WriteTrades(List<TradeNote> lately)
        {
            var sb = new StringBuilder();
            if (lately == null) return sb.ToString();
            for (int i = 0; i < lately.Count; i++)
            {
                TradeNote note = lately[i];
                if (!Storable(note.Day)) continue;
                if (sb.Length > 0) sb.Append(RecordMark);
                sb.Append(OneField(note.Where)).Append(FieldMark)
                  .Append(OneField(note.What)).Append(FieldMark)
                  .Append(Number(note.Gold)).Append(FieldMark)
                  .Append(Number(note.Day));
            }
            return sb.ToString();
        }

        public static List<TradeNote> ReadTrades(string text, int most)
        {
            var kept = new List<TradeNote>();
            if (string.IsNullOrEmpty(text) || most <= 0) return kept;
            string[] records = text.Split(RecordMark);
            for (int i = 0; i < records.Length && kept.Count < most; i++)
            {
                string[] parts = records[i].Split(FieldMark);
                if (parts.Length < FieldsATradeNeeds) continue;
                if (!Whole(parts[2], out int gold)) continue;
                if (!float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture,
                                    out float day) || !Storable(day)) continue;
                kept.Add(new TradeNote
                {
                    Where = parts[0], What = parts[1], Gold = gold, Day = day
                });
            }
            return kept;
        }

        public static string WriteTradeXpWaiting(Dictionary<string, TradeXpWaiting> waiting)
        {
            var sb = new StringBuilder();
            if (waiting == null) return sb.ToString();
            foreach (var kv in waiting)
            {
                if (!Storable(kv.Key) || (kv.Value.Made == 0L && kv.Value.Allowed == 0L)) continue;
                if (sb.Length > 0) sb.Append(RecordMark);
                sb.Append(kv.Key).Append(FieldMark)
                  .Append(Number(kv.Value.Made)).Append(FieldMark)
                  .Append(Number(kv.Value.Allowed));
            }
            return sb.ToString();
        }

        public static Dictionary<string, TradeXpWaiting> ReadTradeXpWaiting(string text)
        {
            var kept = new Dictionary<string, TradeXpWaiting>(System.StringComparer.Ordinal);
            if (string.IsNullOrEmpty(text)) return kept;
            string[] records = text.Split(RecordMark);
            for (int i = 0; i < records.Length; i++)
            {
                string[] parts = records[i].Split(FieldMark);
                if (parts.Length < FieldsAWaitNeeds || !Storable(parts[0])) continue;
                if (!long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out long made)) continue;
                if (!long.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out long allowed)) continue;
                kept[parts[0]] = new TradeXpWaiting { Made = made, Allowed = allowed < 0L ? 0L : allowed };
            }
            return kept;
        }

        public static List<PurchaseRecord> ReadPurchases(string text)
        {
            var kept = new List<PurchaseRecord>();
            if (string.IsNullOrEmpty(text)) return kept;
            string[] records = text.Split(RecordMark);
            for (int i = 0; i < records.Length; i++)
            {
                string[] parts = records[i].Split(FieldMark);
                if (parts.Length < FieldsAPurchaseNeeds || !Storable(parts[0])) continue;
                if (!Whole(parts[1], out int total) || !Whole(parts[2], out int count) ||
                    !Whole(parts[3], out int last)) continue;
                if (count <= 0) continue;
                kept.Add(new PurchaseRecord
                {
                    ItemId = parts[0], TotalPaid = total, Count = count, LastUnitPaid = last,
                    Batches = parts.Length > FieldsAPurchaseNeeds
                        ? ReadBatches(parts[FieldsAPurchaseNeeds], count)
                        : new List<Batch>()
                });
            }
            return kept;
        }
    }
}
