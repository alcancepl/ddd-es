using Ddd.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd.Domain
{
    public interface IAggregate
    {
        Guid Id { get; }
        int Version { get; }

        void ApplyEvent(IEvent @event);

        IEnumerable<IEvent> UncommitedEvents { get; }
        void ClearUncommitedEvents();
        
        // IMemento GetSnapshot();        
    }
}
