using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Domain
{
    public interface IReadOnlyRepository
    {
        Task<TAggregate> GetByIdAsync<TAggregate, TAggregateIdentity>(TAggregateIdentity id, CancellationToken cancellationToken = default(CancellationToken))
            where TAggregate : class, IAggregate<TAggregateIdentity>, new()
            where TAggregateIdentity : IAggregateIdentity;
    }
}
