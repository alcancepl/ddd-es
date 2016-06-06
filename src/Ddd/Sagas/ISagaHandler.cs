using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Sagas
{
    public interface ISagaHandler<TSagaData, TMessage>
        where TMessage : class, IMessage
        where TSagaData : class, ISagaData, new()
    {
        Task HandleAsync(Saga<TSagaData> saga, TMessage message, CancellationToken cancellationToken = default(CancellationToken));
    }
}
