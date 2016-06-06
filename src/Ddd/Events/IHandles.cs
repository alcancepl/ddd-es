using Ddd.Messages;

namespace Ddd.Events
{
    /// <summary>
    /// A handler of a specific type of domain events.
    /// </summary>
    /// <typeparam name="TEvent">the type of the handled domain events</typeparam>    
    public interface IHandles<in TEvent>: IHandler<TEvent> where TEvent : class, IEvent
    {        
    }
}
