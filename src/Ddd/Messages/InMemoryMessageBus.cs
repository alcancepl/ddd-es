using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Messages
{
    using Ddd;

    public partial class InMemoryMessageBus : IMessageBus// ICommandSender, IEventPublisher
    {
        //private readonly SemaphoreSlim semaphore;
        private readonly MessagesHandler handlerImplementation;

        public InMemoryMessageBus(MessagesHandler handlerImplementation)
        {
            //this.semaphore = new SemaphoreSlim(1, 1);
            this.handlerImplementation = handlerImplementation;
        }

        public async Task PublishAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default(CancellationToken))
        {
            //await semaphore.WaitAsync(cancellationToken);
            try
            {
                foreach (var message in messages)
                {
                    try
                    {
                        await handlerImplementation.HandleAsync(message, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceError("Handler for message {0} trew exception.", message);
                        System.Diagnostics.Trace.TraceError(ex.ToString());
                        throw;
                    }
                }
            }
            finally
            {
                //semaphore.Release();
            }
        }

        //public async Task SendAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default(CancellationToken)) where TMessage : class, IMessage
        public async Task SendAsync(IMessage message, CancellationToken cancellationToken = default(CancellationToken))
        //where TMessage : class, IMessage
        {
            try
            {
                await handlerImplementation.HandleAsync(message, cancellationToken);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("Handler for message {0} trew exception.", message);
                System.Diagnostics.Trace.TraceError(ex.ToString());
                throw;
            }
        }
    }
}
