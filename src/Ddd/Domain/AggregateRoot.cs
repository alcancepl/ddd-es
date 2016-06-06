using Ddd.Events;
using System;
using System.Collections.Generic;

namespace Ddd.Domain
{
    public abstract class AggregateRoot<TAggregateIdentity> : IAggregate<TAggregateIdentity> where TAggregateIdentity: IAggregateIdentity
    {
        private readonly List<IEvent> uncommitedEvents = new List<IEvent>();
        private Dictionary<Type, Action<IEvent>> transitions = new Dictionary<Type, Action<IEvent>>();

        protected AggregateRoot()
        {
            Version = -1;
        }

        public IEnumerable<IEvent> UncommitedEvents
        {
            get
            {
                lock (uncommitedEvents)
                {
                    return uncommitedEvents.ToArray();
                }
            }
        }        

        public void ClearUncommitedEvents()
        {
            lock (uncommitedEvents)
            {
                uncommitedEvents.Clear();
            }
        }

        public int Version { get; protected set; }

        public TAggregateIdentity Id { get; protected set; }

        protected void RegisterTransition<T>(Action<T> transition) where T : class, IEvent
        {
            transitions.Add(typeof(T), o => transition((T)o));
        }

        protected void RaiseEvent(IEvent @event)
        {
            ApplyEvent(@event);
            lock (uncommitedEvents)
            {
                uncommitedEvents.Add(@event);
            }
        }

        public void ApplyEvent(IEvent @event)
        {
            if (@event == null)
                throw new ArgumentNullException("event");

            var eventType = @event.GetType();
            Action<IEvent> transition;
            if (!transitions.TryGetValue(eventType, out transition))
            {
                throw new AggregateException($"Cannot apply event {eventType.Name} to aggregate {this}. Transition registration is missing. Event details: {@event}");
            }
            transition(@event);
            lock (uncommitedEvents)
            {
                Version++;
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}:{Id}";            
        }

    }
}
