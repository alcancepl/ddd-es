using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rcs.Eurad
{
    public abstract class DomainRepositoryBase : IDomainRepository
    {        
        protected int CalculateExpectedVersion<T>(IAggregate aggregate, List<T> events)
        {
            var expectedVersion = aggregate.Version - events.Count;
            return expectedVersion;
        }

        protected TAggregate BuildAggregate<TAggregate>(IEnumerable<IEvent> events) where TAggregate : IAggregate, new()
        {
            var result = new TAggregate();
            foreach (var @event in events)
            {
                result.ApplyEvent(@event);
            }
            return result;
        }

        public abstract Task<IEnumerable<IEvent>> SaveAsync<TAggregate>(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken)) where TAggregate : IAggregate;

        public abstract Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, CancellationToken cancellationToken = default(CancellationToken)) where TAggregate : IAggregate, new();
    }
}
