using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Events
{
    public interface IEventStore
    {
        Task SaveAsync(Commands.CommandContext context, Type aggregateType, Guid aggregateId, ICollection<IEvent> events, CancellationToken cancellationToken = default(CancellationToken));
        Task<ICollection<IEvent>> GetAsync(Type aggregateType, Guid aggregateId, int fromVersion = 1, CancellationToken cancellationToken = default(CancellationToken));
    }
}
