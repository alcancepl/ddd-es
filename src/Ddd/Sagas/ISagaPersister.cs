using System.Threading.Tasks;

namespace Ddd.Sagas
{
    /// <summary>
    /// Marker interface for saga persisters.
    /// </summary>
    public interface ISagaPersister
    { }

    /// <summary>
    /// Defines the basic functionality of a persister for storing a saga.	
    /// </summary>
    public interface ISagaSaver<TSagaData>: ISagaPersister
        where TSagaData : class, ISagaData, new()
    {
        /// <summary>
        /// Saves the saga data to the persistence store.
        /// </summary>
        /// <param name="sagaData">The saga data to save.</param>        
        Task SaveAsync(TSagaData sagaData);

        ///// <summary>
        ///// Updates an existing saga entity in the persistence store.
        ///// </summary>
        ///// <param name="sagaData">The saga entity to updated.</param>
        ///// <param name="context">The current pipeline context.</param>
        //Task UpdateAsync(ISagaData sagaData);

        ///// <summary>
        ///// Gets a saga entity from the persistence store by its Id.
        ///// </summary>
        ///// <param name="sagaId">The Id of the saga entity to get.</param>
        ///// <param name="context">The current pipeline context.</param>
        //Task<TSagaData> GetAsync<TSagaData>(Guid sagaId) where TSagaData : class, ISagaData, new();

        ///// <summary>
        ///// Looks up a saga entity by a given property.
        ///// </summary>
        ///// <param name="propertyName">From the data store, analyze this property.</param>
        ///// <param name="propertyValue">From the data store, look for this value in the identified property.</param>
        ///// <param name="context">The current pipeline context.</param>
        //Task<TSagaData> FindByEventAsync<TSagaData, TEvent>(TEvent @event)
        //    where TSagaData : class, ISagaData, new()
        //    where TEvent : class, Events.IEvent;

        /// <summary>
        /// Sets a saga as completed and removes it from the active saga list
        /// in the persistence store.
        /// </summary>
        /// <param name="saga">The saga to complete.</param>
        /// <param name="context">The current pipeline context.</param>
        Task CompleteAsync(TSagaData sagaData);
    }
}
