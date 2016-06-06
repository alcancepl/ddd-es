using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Ddd.Services
{
    public class SequenceEntity : TableEntity
    {
        public long LastNr { get; set; }
        public string SequenceData { get; set; }

        public static SequenceEntity Create<TSequenceData>(string sequence, long lastNr, TSequenceData sequenceData)
        {
            return new SequenceEntity()
            {
                PartitionKey = FormatPartitionKey(sequence),
                RowKey = FormatRowKey(),
                LastNr = lastNr,
                SequenceData = Tools.SerializeData(sequenceData)
            };
        }

        public static string FormatPartitionKey(string sequence) => Tools.Sanitize(sequence);
        public static string FormatRowKey() => "SEQUENCE-HEAD";

        internal TSequenceData GetData<TSequenceData>() where TSequenceData : class
            => Tools.DeserializeData<TSequenceData>(SequenceData);


        internal void SetData<TSequenceData>(TSequenceData data)
        {
            SequenceData = Tools.SerializeData(data);
        }
    }

    public class IdEntity : TableEntity
    {
        public long Nr { get; set; }

        public static IdEntity Create(string sequence, string aggregateId, long sequenceNr)
        {
            return new IdEntity()
            {
                PartitionKey = FormatPartitionKey(sequence),
                RowKey = FormatRowKey(aggregateId),
                Nr = sequenceNr
            };
        }

        public static string FormatPartitionKey(string sequence) => Tools.Sanitize(sequence);
        public static string FormatRowKey(string aggregateId) => Tools.Sanitize($"ID-{aggregateId}");
    }

    public class NrEntity : TableEntity
    {
        public string Data { get; set; }

        public static NrEntity Create<TNrData>(string sequence, string aggregateId, long nr, TNrData nrData)
        {
            return new NrEntity()
            {
                PartitionKey = FormatPartitionKey(sequence),
                RowKey = FormatRowKey(nr),
                Id = aggregateId,
                Data = Tools.SerializeData(nrData)
            };
        }

        public static string FormatPartitionKey(string sequence) => Tools.Sanitize(sequence);
        public static string FormatRowKey(long sequenceNr) => $"NR-{sequenceNr:000000000000000000}";

        public string Id { get; set; }

        internal TNrData GetData<TNrData>() where TNrData : class
            => Tools.DeserializeData<TNrData>(Data);

        internal void SetData<TNrData>(TNrData data)
        {
            Data = Tools.SerializeData(data);
        }
    }

    static class Tools
    {
        /// <summary>
        /// Sanitizes Azure table storage key properties.
        /// </summary>
        /// <param name="s">string to sanitize</param>
        /// <returns>a valid azure storage key property value</returns>
        internal static string Sanitize(string s)
        {
            var rgx = new System.Text.RegularExpressions.Regex(@"[^a-zA-Z0-9\-_]");
            var n = rgx.Replace(s, "");
            return n;
        }

        internal static string SerializeData<TData>(TData data)
        {
            if (data == null)
                return null;
            else
                return JsonConvert.SerializeObject(data);
        }

        internal static TData DeserializeData<TData>(string serializedData) where TData : class
        {
            if (string.IsNullOrEmpty(serializedData))
                return null;
            else
                return JsonConvert.DeserializeObject<TData>(serializedData);
        }
    }

}
