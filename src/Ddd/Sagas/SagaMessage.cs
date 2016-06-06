using System;

namespace Ddd.Sagas
{
    /// <summary>
    /// Representation of a message that is related to a saga.
    /// </summary>
    public class SagaMessage
    {
        /// <summary>
        /// Creates a new instance of <see cref="SagaMessage"/>.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="isAllowedToStart"><code>true</code> if the message can start the saga, <code>false</code> otherwise.</param>
        public SagaMessage(Type sagaDataType, Type messageType, bool isAllowedToStart)
        {
            MessageType = messageType;
            IsAllowedToStartSaga = isAllowedToStart;
            SagaHandlerType = sagaDataType == typeof(TransientSagaData) 
                ? typeof(TransientSagaHandler<>).MakeGenericType(messageType)
                : typeof(SagaHandler<,>).MakeGenericType(sagaDataType, messageType);
        }

        /// <summary>
        /// The type of the message.
        /// </summary>
        public Type MessageType { get; private set; }

        /// <summary>
        /// True if the message can start the saga.
        /// </summary>
        public bool IsAllowedToStartSaga { get; private set; }

        /// <summary>
        /// The type of the SagaHandler{TSagaData,TMessage} for this message.
        /// </summary>
        public Type SagaHandlerType { get; private set; }
    }
}
