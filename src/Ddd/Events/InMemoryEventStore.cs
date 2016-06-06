using Ddd.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Events
{
    using Domain;
    using ETag = Guid;

    public class InMemoryEventStore : IEventStore
    {
        // simulates the underlying storage db
        private static readonly IDictionary<string, Tuple<ETag, List<EventEntity>>> eventStore;

        private IDictionary<IAggregateIdentity, ETag> idToETag;

        private readonly IMessageBus bus;

        static InMemoryEventStore()
        {
            eventStore = new Dictionary<string, Tuple<ETag, List<EventEntity>>>(1000);
        }

        public InMemoryEventStore(IMessageBus bus)
        {
            this.bus = bus;
            idToETag = new Dictionary<IAggregateIdentity, ETag>(100);
        }

        /// <summary>
        /// For debug purposes
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, List<string>> GetUnderlyingStore()
        {
            lock (eventStore)
            {
                return eventStore.ToDictionary(k => k.Key, v => v.Value.Item2.Select(e => e.ToString()).ToList());
            }
        }

        public Task<ICollection<IEvent>> GetAsync(Type aggregateType, IAggregateIdentity aggregateId, int fromVersion = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            var partition = GetPartition(aggregateType, aggregateId);
            lock (eventStore)
            {
                Tuple<ETag, List<EventEntity>> eTaggedEvents;
                if (!eventStore.TryGetValue(partition, out eTaggedEvents))
                    return Task.FromResult<ICollection<IEvent>>(new IEvent[0]);

                var eTag = eTaggedEvents.Item1;
                var storedEvents = eTaggedEvents.Item2;
                idToETag[aggregateId] = eTag;

                var deserializedEvents = storedEvents
                    .Skip(fromVersion - 1)
                    .Select(DeserializeEvent)
                    .ToList();

                return Task.FromResult<ICollection<IEvent>>(deserializedEvents);
            }
        }

        public async Task SaveAsync(Type aggregateType, IAggregateIdentity aggregateId, ICollection<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (events.Count == 0)
                return;

            var partition = GetPartition(aggregateType, aggregateId);

            lock (eventStore)
            {
                Tuple<ETag, List<EventEntity>> eTaggedEvents;
                if (eventStore.TryGetValue(partition, out eTaggedEvents))
                {
                    // Check my last ETag against the current ETag
                    var dbETag = eTaggedEvents.Item1;
                    ETag myETag;
                    if (idToETag.TryGetValue(aggregateId, out myETag) && myETag != dbETag)
                    {
                        throw new Ddd.Domain.Exceptions.ConcurrencyException(aggregateType, aggregateId);
                    }

                    var eventsEntities = eTaggedEvents.Item2;
                    var lastVer = eventsEntities.Count;

                    eventsEntities.AddRange(events.Select(e => ToEventEntity(e, ++lastVer)));

                    var eTag = ETag.NewGuid();
                    eventStore[partition] = new Tuple<ETag, List<EventEntity>>(
                        eTag,
                        eventsEntities);
                    idToETag[aggregateId] = eTag; // Remeber current ETag
                }
                else
                {
                    var eTag = ETag.NewGuid();
                    var lastVer = 0;
                    var eventsEntities = events.Select(e => ToEventEntity(e, ++lastVer)).ToList();
                    eventStore.Add(partition, new Tuple<ETag, List<EventEntity>>(
                        eTag,
                        eventsEntities));
                    idToETag.Add(aggregateId, eTag); // Remeber current ETag
                }
            }
            
            await bus.PublishAsync(events);            
        }

        private static string GetPartition(Type aggregateType, IAggregateIdentity aggregateId)
        {
            return string.Concat(aggregateType.FullName, "|", aggregateId.ToString());
        }

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


        static EventEntity ToEventEntity(IEvent @event, int version)
        {
            var id = Guid.NewGuid().ToString("D");
            return new EventEntity
            {
                Id = id,
                Type = @event.GetType().FullName,
                Data = JsonConvert.SerializeObject(@event, SerializerSettings),
                Version = version
            };
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
            public override string ToString()
            {
                return string.Format("{0:000} {1} {2}", Version, Type, Data);
            }
        }
    }
}
