using Ddd.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DddTest
{
    public class TestCommandProcessor
    {
        static Type genericCommandHandlerType = typeof(ICommandHandler<>);
        private readonly Func<Type, dynamic> getInstance;

        public TestCommandProcessor(Func<Type, dynamic> getInstance)
        {
            this.getInstance = getInstance;
        }

        public async Task ProcessAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : class, ICommand
        {
            if (command == null)
                throw new ArgumentNullException("command");

            var iCommandHandlerOfCommandType = genericCommandHandlerType.MakeGenericType(command.GetType());
            var commandHandler = getInstance(iCommandHandlerOfCommandType);
            await commandHandler.HandleAsync((dynamic)command, cancellationToken);
        }
    }
}
