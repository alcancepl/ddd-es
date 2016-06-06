using Ddd.Domain;
using Ddd.Events;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Sagas.Tests
{
    public class MySaga : Saga<MySagaData>,
        ISagaStartedBy<StartSagaEvent>,
        IHandles<EndSagaEvent>

    {
        public Task HandleAsync(EndSagaEvent message, CancellationToken cancellationToken = default(CancellationToken))
        {
            Data.HasEnded = true;
            return Task.CompletedTask;
        }

        public Task HandleAsync(StartSagaEvent message, CancellationToken cancellationToken = default(CancellationToken))
        {
            Data.HasEnded = false;
            return Task.CompletedTask;
        }
    }

    public class MySagaData : SagaDataBase
    {
        public string Id { get; set; }
        public bool HasEnded { get; set; }

    }

    public class MySagaPersistence : ISagaSaver<MySagaData>, ISagaByMessageFinder<MySagaData, StartSagaEvent>, ISagaByMessageFinder<MySagaData, EndSagaEvent>
    {
        public Task CompleteAsync(MySagaData sagaData)
        {
            var data = ById(sagaData.Id);
            if (data != null)
            {
                db.Remove(data);
            }
            return Task.CompletedTask;
        }

        public Task<SagaFindResult<MySagaData>> FindAsync(EndSagaEvent message)
        {
            var data = db.SingleOrDefault(d => d.Id == message.Id.Value);
            return Task.FromResult(new SagaFindResult<MySagaData>(data));
        }

        public Task<SagaFindResult<MySagaData>> FindAsync(StartSagaEvent message)
        {
            var data = db.SingleOrDefault(d => d.Id == message.Id.Value);
            return Task.FromResult(new SagaFindResult<MySagaData>(data));
        }

        public Task SaveAsync(MySagaData sagaData)
        {
            var data = ById(sagaData.Id);
            if (data != null)
            {
                db.Remove(data);
            }
            db.Add(sagaData);
            return Task.CompletedTask;
        }

        private MySagaData ById(string id)
        {
            return db.SingleOrDefault(d => d.Id == id);
        }

        public static List<MySagaData> db = new List<MySagaData>();
    }

    public class StartSagaEvent : IEvent
    {
        public IAggregateIdentity Id { get; set; }
    }

    public class EndSagaEvent : IEvent
    {
        public IAggregateIdentity Id { get; set; }
    }
}
