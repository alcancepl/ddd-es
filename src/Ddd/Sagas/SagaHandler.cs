using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Sagas
{
    public class TransientSagaHandler<TMessage> : SagaHandler<TransientSagaData, TMessage>
        where TMessage : class, IMessage
    {
        public TransientSagaHandler()
            : base(new TransientSagaFinder<TMessage>(), new TransientSagaSaver())
        {
        }
    }

    public class SagaHandler<TSagaData, TMessage>//: ISagaHandler<TSagaData, TMessage>
        where TMessage : class, IMessage
        where TSagaData : class, ISagaData, new()
    {
        private readonly ISagaByMessageFinder<TSagaData, TMessage> finder;
        private readonly ISagaSaver<TSagaData> saver;

        public SagaHandler(ISagaByMessageFinder<TSagaData, TMessage> finder, ISagaSaver<TSagaData> saver)
        {
            this.finder = finder;
            this.saver = saver;
        }

        public async Task HandleAsync(Saga<TSagaData> saga, TMessage message, bool messageIsAllowedToStartTheSaga, CancellationToken cancellationToken = default(CancellationToken))
        {
            saga.Data = await GetSagaDataAsync(message, messageIsAllowedToStartTheSaga);
            await ((Messages.IHandler<TMessage>)saga).HandleAsync(message, cancellationToken);
            await SaveSagaData(saga);
        }

        private async Task SaveSagaData(Saga<TSagaData> saga)
        {
            if (saga.Completed)
            {
                await saver.CompleteAsync(saga.Data);
            }
            else
            {
                await saver.SaveAsync(saga.Data);
            }
        }

        private async Task<TSagaData> GetSagaDataAsync(TMessage message, bool messageIsAllowedToStartTheSaga)
        {
            var findResult = await finder.FindAsync(message);
            if (findResult.SagaFound)
            {
                // return existing saga
                return findResult.SagaData;
            }

            if (!messageIsAllowedToStartTheSaga)
            {
                throw new SagaNotFoundException($"Cannot find saga data {typeof(TSagaData).Name} with message '{message.ToString()}'.");
            }

            // start new saga
            return Activator.CreateInstance<TSagaData>();
        }
    }


}
