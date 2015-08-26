using Ddd.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd.Domain
{
    public abstract class AggregateRoot : IAggregate
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

        public Guid Id { get; protected set; }

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
            if (transitions.TryGetValue(eventType, out transition))
            {
                transition(@event);
            }
            lock (uncommitedEvents)
            {
                Version++;
            }
        }


    }
}
