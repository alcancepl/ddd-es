using Rcs.Eurad.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Domain
{
    public class Session : ISession
    {
        private readonly IRepository repository;
        private readonly Dictionary<Guid, AggregateDescriptor> trackedAggregates;

        public Session(IRepository repository)
        {
            if (repository == null)
                throw new ArgumentNullException("repository");

            this.repository = repository;
            trackedAggregates = new Dictionary<Guid, AggregateDescriptor>();
        }

        public void Add<T>(T aggregate) where T : class, IAggregate
        {
            if (!IsTracked(aggregate.Id))
                trackedAggregates.Add(aggregate.Id,
                                       new AggregateDescriptor { Aggregate = aggregate, Version = aggregate.Version });
            else if (trackedAggregates[aggregate.Id].Aggregate != aggregate)
                throw new ConcurrencyException<T>(aggregate.Id);
        }

        public async Task<T> LoadAsync<T>(Guid id, int? expectedVersion = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class, IAggregate, new()
        {
            if (IsTracked(id))
            {
                var trackedAggregate = (T)trackedAggregates[id].Aggregate;
                if (expectedVersion != null && trackedAggregate.Version != expectedVersion)
                    throw new ConcurrencyException<T>(trackedAggregate.Id);
                return trackedAggregate;
            }

            var aggregate = await repository.GetByIdAsync<T>(id, cancellationToken);
            if (expectedVersion != null && aggregate.Version != expectedVersion)
                throw new ConcurrencyException<T>(id);
            Add(aggregate);

            return aggregate;
        }

        private bool IsTracked(Guid id)
        {
            return trackedAggregates.ContainsKey(id);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var eventsToSave = trackedAggregates.Select()

            foreach (var descriptor in trackedAggregates.Values)
            {
                await repository.SaveAsync(descriptor.Aggregate, descriptor.Version, cancellationToken);
            }
            trackedAggregates.Clear();
        }

        private class AggregateDescriptor
        {
            public IAggregate Aggregate { get; set; }
            public int Version { get; set; }
        }
    }
}
