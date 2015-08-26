using Ddd.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Commands
{
    /// <summary>
    /// Handler for a specific command type.
    /// </summary>
    /// <typeparam name="TCommand">the type of handled commands</typeparam>
    public interface ICommandHandler<TCommand>: IHandler<CommandContext, TCommand> where TCommand: class, ICommand
    {        
    }
}
