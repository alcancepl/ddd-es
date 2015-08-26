using Ddd.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Events
{
    /// <summary>
    /// A handler of a specific type of domain events.
    /// </summary>
    /// <typeparam name="TEvent">the type of the handled domain events</typeparam>    
    public interface IHandles<TEvent>: IHandler<Commands.CommandContext, TEvent> where TEvent : class, IEvent
    {        
    }
}
