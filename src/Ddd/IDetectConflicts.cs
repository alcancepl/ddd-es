using Rcs.Eurad.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rcs.Eurad
{
    public interface IDetectConflicts
    {
        void Register<TUncommitted, TCommitted>(ConflictDelegate handler)
            where TUncommitted : class, IEvent
            where TCommitted : class, IEvent;

        bool ConflictsWith(IEnumerable<IEvent> uncommittedEvents, IEnumerable<IEvent> committedEvents);
    }

    public delegate bool ConflictDelegate(IEvent uncommitted, IEvent committed);
}
