using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Ddd.Domain
{
    public class Repository : IRepository
    {		
        private readonly Events.IEventStore store;        
                
		public Repository(Events.IEventStore store)
		{
            this.store = store;
		}

		public async Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, CancellationToken cancellationToken = default(CancellationToken)) where TAggregate : class, IAggregate, new()
        {
            var events = await store.GetAsync(typeof(TAggregate), id, 1, cancellationToken);
            if (events.Count == 0)
            {
                throw new Exceptions.AggregateNotFoundException(typeof(TAggregate), id);
            }

            var aggregate = new TAggregate();
            foreach (var @event in events)
            {
                aggregate.ApplyEvent(@event);
            }
            return aggregate;
		}

        public async Task SaveAsync<TAggregate>(Commands.CommandContext context, TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken)) where TAggregate : class, IAggregate
        {
			var uncomittedEvents = aggregate.UncommitedEvents.ToList();
            if (uncomittedEvents.Count == 0)
                return;

			//var expectedVersion = aggregate.Version - uncomittedEvents.Count() + 1;

            await store.SaveAsync(context, typeof(TAggregate), aggregate.Id, uncomittedEvents, cancellationToken);			

			aggregate.ClearUncommitedEvents();
		}	

	}
}
