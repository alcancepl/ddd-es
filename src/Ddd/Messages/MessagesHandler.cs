using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ddd.Messages
{
    using Sagas;
    using System.Collections;
    using System.Threading;
    using HandlerFactories = Dictionary<Type, Func<object>>;


    public class MessagesHandler
    {

        class SagaHandlerInfo
        {
            private readonly Type sagaHandlerType;
            private readonly bool messageIsAllowedToStartSaga;

            internal SagaHandlerInfo(Type sagaHandlerType, bool messageIsAllowedToStartSaga)
            {
                this.sagaHandlerType = sagaHandlerType;
                this.messageIsAllowedToStartSaga = messageIsAllowedToStartSaga;
            }

            public Type SagaHandlerType => sagaHandlerType;
            public bool MessageIsAllowedToStartSaga => messageIsAllowedToStartSaga;
        }

        private readonly HandlerFactories handlerFactories;
        private readonly IDictionary<Type, IDictionary<Type, SagaHandlerInfo>> sagaHandlersInfo;
        private readonly HandlerFactories sagaHandlerFactories;

        public MessagesHandler(HandlerFactories handlerFactories, HandlerFactories sagaHandlerFactories, SagaMetadata[] sagaMetadata)
        {
            this.handlerFactories = handlerFactories;
            this.sagaHandlerFactories = sagaHandlerFactories;

            sagaHandlersInfo = new Dictionary<Type, IDictionary<Type, SagaHandlerInfo>>();
            foreach (var sagaMeta in sagaMetadata)
            {
                foreach (var sagaMessage in sagaMeta.AssociatedMessages)
                {
                    IDictionary<Type, SagaHandlerInfo> sagaHandlersInfoForMessageType;
                    if (!sagaHandlersInfo.TryGetValue(sagaMessage.MessageType, out sagaHandlersInfoForMessageType))
                    {
                        sagaHandlersInfo[sagaMessage.MessageType] = sagaHandlersInfoForMessageType = new Dictionary<Type, SagaHandlerInfo>();
                    }
                    sagaHandlersInfoForMessageType[sagaMeta.SagaDataType] = new SagaHandlerInfo(sagaMessage.SagaHandlerType, sagaMessage.IsAllowedToStartSaga);
                }
            }
        }

        static Type genericSaga = typeof(Saga<>);
        static Type genericHandlerType = typeof(IHandler<>);
        static Type iEnumerableType = typeof(IEnumerable<>);

        /// <summary>
        /// Gets a type of a specific handler type collection <see cref="IEnumerable{IHandler{TMessage}}"/>.;
        /// </summary>
        static Func<Type, Type> getSpecificHandlerType = (messageType) =>
            iEnumerableType.MakeGenericType(genericHandlerType.MakeGenericType(messageType));

        public async Task HandleAsync(IMessage message, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (message == null)
                throw new ArgumentNullException("message");

            var messageType = message.GetType();

            var specificHandlerType = getSpecificHandlerType(messageType);

            Func<object> factory;
            if (handlerFactories.TryGetValue(specificHandlerType, out factory))
            {
                IDictionary<Type, SagaHandlerInfo> sagaHandlersInfoDict;
                if (!sagaHandlersInfo.TryGetValue(messageType, out sagaHandlersInfoDict))
                {
                    sagaHandlersInfoDict = new Dictionary<Type, SagaHandlerInfo>(0);
                }

                var tasks = new List<Task>();
                var sagaHandlers = new List<Func<Task>>();
                var handlers = (IEnumerable)factory();
                foreach (object handler in handlers)
                {
                    var handlerType = handler.GetType();
                    Type[] sagaTypeArguments;
                    if (handlerType.IsSubclassOfRawGeneric(genericSaga, out sagaTypeArguments))
                    {
                        if (sagaTypeArguments.Length != 1)
                            throw new InvalidOperationException($"Saga should have exactly one generic parameter, not {sagaTypeArguments.Length}.");
                        var sagaDataType = sagaTypeArguments[0];
                        var sagaHandlerInfo = sagaHandlersInfoDict[sagaDataType];
                        var sagaHandlerFactory = sagaHandlerFactories[sagaHandlerInfo.SagaHandlerType];
                        dynamic sagaHandler = sagaHandlerFactory();
                        dynamic saga = handler;
                        dynamic msg = message;
                        sagaHandlers.Add(new Func<Task>(() =>
                        {
                            return sagaHandler.HandleAsync(saga, msg, sagaHandlerInfo.MessageIsAllowedToStartSaga, cancellationToken);
                        }));
                    }
                    else
                    {
                        tasks.Add(((dynamic)handler).HandleAsync((dynamic)message, cancellationToken));
                    }
                }

                if (message is Commands.ICommand && tasks.Count == 0)
                    throw new NotImplementedException($"Cannot handle command {message.GetType().Name}. Command Handler was not found.");

                // Ordinary handlers first ...
                await Task.WhenAll(tasks).ConfigureAwait(false);
                // ... then Saga handlers
                await Task.WhenAll(sagaHandlers.Select(sagaHandler => sagaHandler.Invoke())).ConfigureAwait(false);
            }
            else
            {
                if (message is Commands.ICommand)
                    throw new NotImplementedException($"Cannot handle command {message.GetType().Name}. Command Handler of type {specificHandlerType.Name} was not found.");
                System.Diagnostics.Trace.TraceWarning($"There are no registered handlers of message type {message.GetType().Name}");
            }
        }

    }
}
