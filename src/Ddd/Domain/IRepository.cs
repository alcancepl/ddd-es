using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Domain
{
    public interface IRepository : IReadOnlyRepository
    {
        Task SaveAsync<TAggregate, TAggregateIdentity>(TAggregate aggregate, CancellationToken cancellationToken = default(CancellationToken))
            where TAggregate : class, IAggregate<TAggregateIdentity>
            where TAggregateIdentity : IAggregateIdentity;
    }
}
