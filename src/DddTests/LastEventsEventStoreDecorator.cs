using Ddd.Domain;
using Ddd.Events;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DddTest
{
    public class LastEventsEventStoreDecorator : IEventStore
    {
        private readonly NewEvents newEvents;
        private readonly IEventStore decorated;
        private InitialEvents initialEvents;
        private static readonly object syncContext = new object();

        public LastEventsEventStoreDecorator(IEventStore decorated, InitialEvents initialEvents, NewEvents newEvents)
        {
            this.decorated = decorated;
            this.newEvents = newEvents;
            this.initialEvents = initialEvents;
        }

        private void ProcessAndClearInitialEvents()
        {
            lock (syncContext)
            {
                if (initialEvents != null)
                {
                    var aggregateEvents = initialEvents.Value;
                    initialEvents = null;
                    SaveAggregateEvents(aggregateEvents).Wait();
                }

            }
        }

        private async Task SaveAggregateEvents(IReadOnlyDictionary<Tuple<Type, IAggregateIdentity>, ICollection<IEvent>> aggregateEvents)
        {
            foreach (var initialAggregateEvents in aggregateEvents)
            {
                await decorated.SaveAsync(initialAggregateEvents.Key.Item1, initialAggregateEvents.Key.Item2, initialAggregateEvents.Value);
            }
        }

        public async Task<ICollection<IEvent>> GetAsync(Type aggregateType, IAggregateIdentity aggregateId, int fromVersion = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (initialEvents != null)
                ProcessAndClearInitialEvents();

            return await decorated.GetAsync(aggregateType, aggregateId, fromVersion, cancellationToken);
        }

        public async Task SaveAsync(Type aggregateType, IAggregateIdentity aggregateId, ICollection<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (initialEvents != null)
                ProcessAndClearInitialEvents();

            await decorated.SaveAsync(aggregateType, aggregateId, events, cancellationToken);
            newEvents.Set(aggregateType, aggregateId, events);
        }

        public class InitialEvents
        {
            private readonly Dictionary<Tuple<Type, IAggregateIdentity>, ICollection<IEvent>> events = new Dictionary<Tuple<Type, IAggregateIdentity>, ICollection<IEvent>>();
            public void AddEvents<TAggregate>(params IEvent[] events)
            {
                foreach (var @event in events)
                {
                    var key = Tuple.Create(typeof(TAggregate), @event.Id);
                    ICollection<IEvent> aggregateEvents;
                    if (!this.events.TryGetValue(key, out aggregateEvents))
                    {
                        aggregateEvents = new List<IEvent>();
                        this.events[key] = aggregateEvents;
                    }
                    aggregateEvents.Add(@event);
                }
            }

            public IReadOnlyDictionary<Tuple<Type, IAggregateIdentity>, ICollection<IEvent>> Value => events;
        }

        public class NewEvents
        {
            private readonly List<Tuple<Type, IAggregateIdentity, ICollection<IEvent>>> savedEvents = new List<Tuple<Type, IAggregateIdentity, ICollection<IEvent>>>();
            public IReadOnlyList<Tuple<Type, IAggregateIdentity, ICollection<IEvent>>> SavedEvents => savedEvents;
            public Tuple<Type, IAggregateIdentity, ICollection<IEvent>> LastEvents => savedEvents[savedEvents.Count - 1];
            public void Set(Type aggregateType, IAggregateIdentity aggregateId, ICollection<IEvent> events) => savedEvents.Add(Tuple.Create(aggregateType, aggregateId, events));
        }
    }


}
