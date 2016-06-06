using System.Threading.Tasks;

namespace Ddd.Sagas
{
    /// <summary>
    /// Marker interface for a saga finder <see cref="ISagaByMessageFinder{TSagaData,TMessage}"/>.
    /// </summary>
    public interface ISagaFinder
    { }

    public interface ISagaByMessageFinder<TSagaData, TMessage> : ISagaFinder
        where TSagaData : class, ISagaData, new()
        where TMessage : class, IMessage
    {
        Task<SagaFindResult<TSagaData>> FindAsync(TMessage message);
    }

    public class SagaFindResult<TSagaData> where TSagaData : class, ISagaData, new()
    {
        public SagaFindResult(TSagaData data): this()
        {
            SagaFound = data != null;
            SagaData = data;
        }

        private SagaFindResult()
        {
        }

        public static SagaFindResult<TSagaData> Found(TSagaData data)
        {
            return new SagaFindResult<TSagaData>() { SagaFound = true, SagaData = data };
        }

        public static SagaFindResult<TSagaData> NotFound()
        {
            return new SagaFindResult<TSagaData>() { SagaFound = false, SagaData = default(TSagaData) };
        }

        public bool SagaFound { get; private set; }
        public TSagaData SagaData { get; private set; }
    }
}
