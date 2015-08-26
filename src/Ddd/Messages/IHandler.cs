using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Messages
{
    public interface IHandler<in TContext, in T>
        where TContext : class
        where T : IMessage        
    {
        Task HandleAsync(TContext context, T message, CancellationToken cancellationToken = default(CancellationToken));
    }
}
