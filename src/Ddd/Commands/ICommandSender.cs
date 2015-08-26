using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Commands
{
    /// <summary>
    /// A bus that only sends messages (commands). It does not publish messages (events).   
    /// </summary>   
    public interface ICommandSender
    {
        Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand;
    }
}
