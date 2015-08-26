using Ddd.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Domain
{
    public interface IRepository
    {
        Task SaveAsync<TAggregate>(Commands.CommandContext context, TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken)) where TAggregate : class, IAggregate;
        Task<TAggregate> GetByIdAsync<TAggregate>(Guid id, CancellationToken cancellationToken = default(CancellationToken)) where TAggregate : class, IAggregate, new();
    }
}
