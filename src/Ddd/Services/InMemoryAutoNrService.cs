using Ddd.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddd.Services
{
	/// <summary>
	/// Thread-safe in-memory auto nr service implementation.
	/// </summary>
	public class InMemoryAutoNrService : IAutoNrService
	{
		static InMemoryAutoNrService()
		{
			sequences = new ConcurrentDictionary<string, Sequence>();
		}

		private static ConcurrentDictionary<string, Sequence> sequences;

		private class Sequence
		{
			public long LastSequenceNr { get; set; } = 0;
			private readonly Dictionary<string, long> generatedIds;
			private readonly Dictionary<long, Tuple<string, string>> generatedNrs;

			private Sequence()
			{
				generatedIds = new Dictionary<string, long>();
				generatedNrs = new Dictionary<long, Tuple<string, string>>();
			}

			public static Sequence Create<TSequenceData>(TSequenceData config) where TSequenceData : class
			{
				var ctx = new Sequence();
				ctx.SetConfig(config);
				return ctx;
			}

			public void AddNr(string aggregateId, long nr, object data)
			{
				generatedIds.Add(aggregateId, nr);
				try
				{
					generatedNrs.Add(nr, Tuple.Create(
						aggregateId,
						data == null? null : JsonConvert.SerializeObject(data)));
				}
				catch
				{
					generatedIds.Remove(aggregateId);
					throw;
				}
			}

			public void SetData(string aggregateId, long nr, object data)
			{
				generatedNrs[nr] = Tuple.Create(
					aggregateId, 
					data == null ? null : JsonConvert.SerializeObject(data));
			}

			public bool TryGet(string aggregateId, out long nr)
			{
				return generatedIds.TryGetValue(aggregateId, out nr);
			}

			private string configData;

			internal TSequenceData GetConfig<TSequenceData>() where TSequenceData : class
			{
				return JsonConvert.DeserializeObject<TSequenceData>(configData);
			}

			internal void SetConfig<TSequenceData>(TSequenceData config) where TSequenceData : class
			{
				this.configData = JsonConvert.SerializeObject(config);
			}

			internal TNrData GetAutoNrDataByNr<TNrData>(long nr) where TNrData : class
			{
				Tuple<string, string> idData;
				if (generatedNrs.TryGetValue(nr, out idData) && !string.IsNullOrEmpty(idData.Item2))
					return JsonConvert.DeserializeObject<TNrData>(idData.Item2);
				return null;
			}
		}


		public Task<TNrData> GetAutoNr<TSequenceData, TNrData>(string sequence, string aggregateId, AutoNrGenerator<TSequenceData, TNrData> generator)
			where TSequenceData : class
			where TNrData : class
		{
			Sequence contextLock = sequences.GetOrAdd(sequence, Sequence.Create<TSequenceData>(null));
			lock (contextLock) // only one thread per context
			{
				var context = sequences[sequence];
				long seqNr;
				if (context.TryGet(aggregateId, out seqNr))
				{
					return Task.FromResult(context.GetAutoNrDataByNr<TNrData>(seqNr));
				}
				else
				{
					var prev = context.GetAutoNrDataByNr<TNrData>(context.LastSequenceNr);
					var newSeqNr = context.LastSequenceNr + 1;
					var result = generator(newSeqNr, context.GetConfig<TSequenceData>(), prev);

					context.AddNr(aggregateId, newSeqNr, result.NrData);
					context.SetConfig(result.SequenceData);
					context.LastSequenceNr = newSeqNr;
					return Task.FromResult(result.NrData);
				}
			}

		}

		public Task<TNrData> UpdateAutoNr<TSequenceData, TNrData>(string sequence, string aggregateId, AutoNrUpdater<TSequenceData, TNrData> updater)
			where TSequenceData : class
			where TNrData : class
		{
			Sequence contextLock = sequences[sequence];
			if (contextLock == null)
			{
				throw new Exception($"Sequence not found: {sequence}");
			}

			lock (contextLock) // only one thread per context
			{
				var context = sequences[sequence];
				if (context == null)
				{
					throw new Exception($"Sequence not found: {sequence}");
				}
				long seqNr;
				if (!context.TryGet(aggregateId, out seqNr))
					throw new Exception($"Agreggate with id {aggregateId} not found in sequence {sequence}");

				var data = context.GetAutoNrDataByNr<TNrData>(seqNr);
				var prev = context.GetAutoNrDataByNr<TNrData>(seqNr - 1);
				var next = context.GetAutoNrDataByNr<TNrData>(seqNr + 1);
				var result = updater(data, context.GetConfig<TSequenceData>(), prev, next);
				context.SetData(aggregateId, seqNr, result.NrData);
				context.SetConfig(result.SequenceData);
				return Task.FromResult(result.NrData);
			}
		}

		public Task SetLastNr<TSequenceData, TNrData>(string sequence, long lastNr)
			where TSequenceData : class
			where TNrData : class
		{
			Sequence contextLock = sequences.GetOrAdd(sequence, Sequence.Create<TSequenceData>(null));

			lock (contextLock) // only one thread per context
			{
				var context = sequences[sequence];
				var prev = context.GetAutoNrDataByNr<TNrData>(lastNr - 1);
				if (prev != null)
					throw new Exception($"Sequence '{sequence}' already generated number and lastNr cannot be set.");

				context.LastSequenceNr = lastNr;
			}

			return Task.FromResult(0);
		}

	}
}
