using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Domain
{
    public class Repository : IRepository
	{		
		private readonly Events.IEventStore store;        
				
		public Repository(Events.IEventStore store)
		{
			this.store = store;
		}

		public async Task<TAggregate> GetByIdAsync<TAggregate, TAggregateIdentity>(TAggregateIdentity id, CancellationToken cancellationToken = default(CancellationToken))
			where TAggregate : class, IAggregate<TAggregateIdentity>, new()
			where TAggregateIdentity : IAggregateIdentity
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

		public async Task SaveAsync<TAggregate, TAggregateIdentity>(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken))
			where TAggregate : class, IAggregate<TAggregateIdentity>
			where TAggregateIdentity : IAggregateIdentity
		{
			var uncomittedEvents = aggregate.UncommitedEvents.ToList();
			if (uncomittedEvents.Count == 0)
				return;

			//var expectedVersion = aggregate.Version - uncomittedEvents.Count() + 1;

			await store.SaveAsync(typeof(TAggregate), aggregate.Id, uncomittedEvents, cancellationToken);			

			aggregate.ClearUncommitedEvents();
		}	

	}
}
