using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Messages
{
    public interface IHandler<in T>        
        where T : class, IMessage        
    {
        Task HandleAsync(T message, CancellationToken cancellationToken = default(CancellationToken));
    }
}
