using Ddd.Events;
using System.Threading;

namespace Ddd.Sagas
{
    /// <summary>
    /// Marker interface for a saga finder <see cref="IFindSagas{TSagaData}.Using{TEvent}"/>.
    /// </summary>
    public interface IFinder { }

    /// <summary>
    /// Interface indicating that implementers can find sagas of the given type.
    /// </summary>
    public abstract class IFindSagas<TSagaData> where TSagaData : class, ISagaData
    {
        /// <summary>
        /// Narrower interface indicating that implementers can find sagas
        /// of type TSagaData using messages of type TEvent.
        /// </summary>
        public interface Using<TEvent> : IFinder where TEvent : class, IEvent
        {
            /// <summary>
            /// Finds a saga data of the type TSagaData using a message of type TEvent.
            /// </summary>
            TSagaData FindByEventAsync(TEvent @event, CancellationToken cancellationToken = default(CancellationToken));
        }
    }
}
