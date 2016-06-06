using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Messages
{
    public interface IMessageBus
    {
        Task SendAsync(IMessage message, CancellationToken cancellationToken = default(CancellationToken));
        Task PublishAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default(CancellationToken));
    }
}
