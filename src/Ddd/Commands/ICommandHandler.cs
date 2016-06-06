using Ddd.Messages;

namespace Ddd.Commands
{
    /// <summary>
    /// Handler for a specific command type.
    /// </summary>
    /// <typeparam name="TCommand">the type of handled commands</typeparam>
    public interface ICommandHandler<TCommand>: IHandler<TCommand> where TCommand: class, ICommand
    {        
    }
}
