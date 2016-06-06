using Ddd.Events;

namespace Ddd.Sagas
{
    /// <summary>
    /// Use this interface to signify that when a message of the given type is
    /// received, if a saga cannot be found by an <see cref="IFindSagas{TSagaData,TEvent}"/>
    /// the saga will be created.
    /// </summary>
    public interface ISagaStartedBy<TEvent> : IHandles<TEvent> where TEvent : class, IEvent
    {
    }
}
