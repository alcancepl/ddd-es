using Ddd.Events;
using System.Collections.Generic;

namespace Ddd.Domain
{
    public interface IAggregate<TAggregateIdentity> where TAggregateIdentity: IAggregateIdentity
    {
        TAggregateIdentity Id { get; }
        int Version { get; }

        void ApplyEvent(IEvent @event);

        IEnumerable<IEvent> UncommitedEvents { get; }
        void ClearUncommitedEvents();
        
        // IMemento GetSnapshot();        
    }
}
