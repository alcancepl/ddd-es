using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Domain
{
    public interface ISession
    {
        void Add<T>(T aggregate) where T : class, IAggregate;
        Task<T> LoadAsync<T>(Guid id, int? expectedVersion = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, IAggregate, new();
        Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
