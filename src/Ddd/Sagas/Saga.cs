namespace Ddd.Sagas
{
    /// <summary>
    /// This class is used to define sagas containing data and handling messages.
    /// To handle message types, implement <see cref="IHandleMessages{T}"/>
    /// for the relevant types.
    /// To signify that the receipt of a message should start this saga,
    /// implement <see cref="ISagaStartedBy{TEvent}"/> for the relevant message type.
    /// </summary>
    public abstract class Saga<TSagaData>: ISaga where TSagaData : class, ISagaData, new()
    {
        //private readonly ISagaSaver<TSagaData> saver;

        //public Saga(ISagaSaver<TSagaData> saver)
        //{
        //    this.saver = saver;
        //}

        /// <summary>
        /// The saga's typed data.
        /// </summary>
        public TSagaData Data { get; set; }

        /// <summary>
        /// Indicates that the saga is complete.
        /// In order to set this value, use the <see cref="MarkAsComplete" /> method.
        /// </summary>
        public bool Completed { get; private set; }

        /// <summary>
        /// Marks the saga as complete.
        /// This may result in the sagas state being deleted by the persister.
        /// </summary>
        protected void MarkAsComplete()
        {
            Completed = true;
        }        

    }    

    public interface ISaga
    {

    }
}
