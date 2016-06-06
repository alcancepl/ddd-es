using Ddd.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Events
{
    public interface IEventStore
    {
        Task SaveAsync(Type aggregateType, IAggregateIdentity aggregateId, ICollection<IEvent> events, CancellationToken cancellationToken = default(CancellationToken));
        Task<ICollection<IEvent>> GetAsync(Type aggregateType, IAggregateIdentity aggregateId, int fromVersion = 1, CancellationToken cancellationToken = default(CancellationToken));
    }
}
