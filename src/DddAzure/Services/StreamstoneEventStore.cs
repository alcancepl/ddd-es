using Ddd.Domain;
using Ddd.Events;
using Ddd.Messages;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Streamstone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Services
{
	/// <summary>
	/// EventStore implementation on top of Azure Table Storeage.
	/// </summary>
	/// <remarks>This implementation is missing event publishing. Look at the in-memory implementation of SaveAsync().</remarks>
	/// <see cref="https://github.com/yevhen/Streamstone"/>
	/// <seealso cref="https://github.com/yevhen/Streamstone.m-r"/>
	public class StreamstoneEventStore : IEventStore
	{
		private readonly CloudTableClient tableClient;
		private readonly IDictionary<IAggregateIdentity, Stream> openStreams;
		private readonly IMessageBus bus;	

		public StreamstoneEventStore(IMessageBus bus, string storageAccountConnectionString)
		{
			this.bus = bus;

			var account = string.IsNullOrWhiteSpace(storageAccountConnectionString)
				? CloudStorageAccount.DevelopmentStorageAccount
				: CloudStorageAccount.Parse(storageAccountConnectionString);
			tableClient = account.CreateCloudTableClient();
			openStreams = new Dictionary<IAggregateIdentity, Stream>(100);            
		}        

		public async Task SaveAsync(Type aggregateType, IAggregateIdentity aggregateId, ICollection<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (events.Count == 0)
				return;

			var serialized = events.Select(ToEventData).ToArray();

			Stream stream;
			if (!openStreams.TryGetValue(aggregateId, out stream))
			{
				var aggregateTypeName = aggregateType.Name;
				var partition = GetPartition(aggregateTypeName, aggregateId);
				stream = new Stream(partition);
				openStreams.Add(aggregateId, stream);
			}		

			// Save the events
			try
			{                
				var result = await StreamWriteAsync(stream, serialized);
				openStreams[aggregateId] = result.Stream;
			}
			catch (ConcurrencyConflictException ex)
			{
				throw new Ddd.Domain.Exceptions.ConcurrencyException(aggregateType, aggregateId, ex);
			}

			// Publish events (sync the read side)
			await bus.PublishAsync(events, default(CancellationToken));			
		}

		private Partition GetPartition(string aggregateTypeName, IAggregateIdentity aggregateId)
		{
			var table = tableClient.GetTableReference(aggregateTypeName);
			//return new Partition(table, string.Format("{0}|{1}", typeof(TAggregate).Name, aggId.ToString()));
			return new Partition(table, aggregateId.Value);
		}

		private async Task<StreamWriteResult> StreamWriteAsync(Stream stream, EventData[] events)
		{
			try
			{
				return await Stream.WriteAsync(stream, events);
			}
			catch (Microsoft.WindowsAzure.Storage.StorageException ex)
			{
				if (ex.RequestInformation.ExtendedErrorInformation.ErrorCode != "TableNotFound")
					throw ex;

				await stream.Partition.Table.CreateAsync();
				return await Stream.WriteAsync(stream, events);
			}
		}

		public async Task<ICollection<IEvent>> GetAsync(Type aggregateType, IAggregateIdentity aggregateId, int fromVersion = 1, CancellationToken cancellationToken = default(CancellationToken))
		{

			var aggregateTypeName = aggregateType.Name;

			tableClient.GetTableReference(aggregateTypeName).CreateIfNotExists();
			var partition = GetPartition(aggregateTypeName, aggregateId);

			//Stream stream;

			// Open the stream and keep a reference to it for fufure writes of the same aggregate.
			if (!openStreams.ContainsKey(aggregateId))
			{
				var existent = await Stream.TryOpenAsync(partition);
				if (!existent.Found)
				{
					openStreams.Add(aggregateId, new Stream(partition));
					return new List<IEvent>(0);
				}
				openStreams.Add(aggregateId, existent.Stream);
			}

			StreamSlice<EventEntity> slice;
			var nextSliceStart = fromVersion;
			var events = new List<IEvent>(1000);

			do
			{
				slice = await Stream.ReadAsync<EventEntity>(partition, nextSliceStart, sliceSize: 1);
				nextSliceStart = nextSliceStart = slice.HasEvents
					? slice.Events.Last().Version + 1
					: -1;
				events.AddRange(slice.Events.Select(DeserializeEvent));
			}
			while (!slice.IsEndOfStream);

			return events;
		}

		static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
		{
			PreserveReferencesHandling = PreserveReferencesHandling.None,
			NullValueHandling = NullValueHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.Ignore,
			MissingMemberHandling = MissingMemberHandling.Ignore,
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
			//Culture = CultureInfo.GetCultureInfo("en-US"),
			DateFormatHandling = DateFormatHandling.IsoDateFormat,
			TypeNameHandling = TypeNameHandling.None,
			FloatParseHandling = FloatParseHandling.Decimal,
			Formatting = Formatting.None
		};

		static IEvent DeserializeEvent(EventEntity eventEntity)
		{            
			var eventType = Type.GetType(TypeNameToQuallifiedName(eventEntity.Type), true);
			return (IEvent)JsonConvert.DeserializeObject(eventEntity.Data, eventType, SerializerSettings);
		}

		private static string TypeNameToQuallifiedName(string typeName)
		{
			int nthPointIndex = -1;
			for (int i = 0; i < 3; i++)
			{
				nthPointIndex = typeName.IndexOf('.', nthPointIndex + 1);
				if (nthPointIndex == -1) break;
			}
			var assemblyName = nthPointIndex == -1
				? typeName
				: typeName.Substring(0, nthPointIndex);
			return string.Concat(typeName, ", ", assemblyName);
		}

		static EventData ToEventData(IEvent @event)
		{
			var id = Guid.NewGuid().ToString("D");

			var properties = new EventEntity
			{
				Id = id,
				Type = @event.GetType().FullName,
				Data = JsonConvert.SerializeObject(@event, SerializerSettings)
			};

			return new EventData(EventId.From(id), EventProperties.From(properties));
		}

		/// <summary>
		/// Event stored in the store
		/// </summary>
		class EventEntity
		{
			public string Id { get; set; }
			public string Type { get; set; }
			public string Data { get; set; }
			public int Version { get; set; }
		}

	}
}
