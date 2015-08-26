using Rcs.Eurad.Commands;
using Rcs.Eurad.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd
{
    public interface ISaga
    {
        Guid Id { get; }
        int Version { get; }

        void Transition(ICommand message);

        ICollection<IEvent> GetUncommittedEvents();
        void ClearUncommittedEvents();

        ICollection<ICommand> GetUndispatchedMessages();
        void ClearUndispatchedMessages();
    }
}
