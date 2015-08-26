using Ddd.Commands;
using Ddd.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DddTest
{
    public class TestEventStore : IEventStore
    {
        private readonly IDictionary<Guid, List<IEvent>> store;
        private List<IEvent> lastEvents;

        public TestEventStore(Dictionary<Guid, IEnumerable<IEvent>> initData = null)
        {
            lastEvents = new List<IEvent>(0);
            store = new Dictionary<Guid, List<IEvent>>(100);
            if (initData != null)
            {
                foreach (var item in initData)
                {
                    store.Add(item.Key, item.Value.ToList());
                }
            }
        }

        public List<IEvent> GetLastEvents()
        {
            return new List<IEvent>(lastEvents);
        }

        public void Clear()
        {
            store.Clear();
            lastEvents.Clear();
        }

        public Task<ICollection<IEvent>> GetAsync(Type aggregateType, Guid aggregateId, int fromVersion = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<IEvent> list;
            if (!store.TryGetValue(aggregateId, out list))
                return Task.FromResult<ICollection<IEvent>>(new IEvent[0]);
            return Task.FromResult<ICollection<IEvent>>(list.Skip(fromVersion - 1).ToList());
        }

        public Task SaveAsync(CommandContext context, Type aggregateType, Guid aggregateId, ICollection<IEvent> events, CancellationToken cancellationToken = default(CancellationToken))
        {
            List<IEvent> list;
            if (store.TryGetValue(aggregateId, out list))
            {
                list.AddRange(events);
            }
            else
            {
                store.Add(aggregateId, new List<IEvent>(events));
            }
            lastEvents = events.ToList();
            return Task.CompletedTask;
        }
    }
}
