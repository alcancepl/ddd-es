using Ddd.Domain;

namespace Ddd.Events
{
    /// <summary>
    /// A domain event, that has happend in the context of an aggregate.
    /// </summary>
    public interface IEvent: IMessage
    {
        /// <summary>
        /// Aggregate Root Id this event belongs to.
        /// </summary>
        IAggregateIdentity Id { get; }
        //int Version { get; set; }
    }
}
