using System;
using System.Collections.Generic;
using System.Linq;

namespace Ddd.Sagas
{
    public class SagaMetadata
    {
        private Type sagaType;
        private Type sagaDataType;
        private Dictionary<Type, SagaMessage> associatedMessages;

        private SagaMetadata(Type sagaType, Type sagaDataType, List<SagaMessage> associatedMessages)
        {
            this.sagaType = sagaType;
            this.sagaDataType = sagaDataType;
            this.associatedMessages = associatedMessages.ToDictionary(m => m.MessageType);
        }

        private static Type genericSagaHandlerType = typeof(SagaHandler<,>);

        private static Func<Type, Type, Type> getSpecificSagaHandlerType = (sagaDataType, messageType) =>
            genericSagaHandlerType.MakeGenericType(sagaDataType, messageType);             

        /// <summary>
        /// Returns the list of messages that is associated with this saga.
        /// </summary>
        public IEnumerable<SagaMessage> AssociatedMessages => associatedMessages.Values;

        /// <summary>
        /// The type of the related saga data.
        /// </summary>
        public Type SagaDataType => sagaDataType;

        /// <summary>
        /// The type of the saga.
        /// </summary>
        public Type SagaType => sagaType;

        /// <summary>
        /// True if the specified message type is allowed to start the saga.
        /// </summary>
        public bool IsMessageAllowedToStartTheSaga(Type messageType)
        {
            SagaMessage sagaMessage;

            if (!associatedMessages.TryGetValue(messageType, out sagaMessage))
            {
                return false;
            }
            return sagaMessage.IsAllowedToStartSaga;
        }

        /// <summary>
        /// Creates a <see cref="SagaMetadata" /> from a specific Saga type.
        /// </summary>
        /// <param name="sagaType">A type representing a Saga. Must be a non-generic type inheriting from <see cref="Saga" />.</param>
        /// <returns>An instance of <see cref="SagaMetadata" /> describing the Saga.</returns>
        public static SagaMetadata Create(Type sagaType)
        {
            if (!IsSagaType(sagaType))
            {
                throw new Exception($"{sagaType.FullName} is not a saga");
            }

            var genericArguments = GetBaseSagaType(sagaType).GetGenericArguments();
            if (genericArguments.Length != 1)
            {
                throw new Exception($"'{sagaType}' saga type does not implement Saga<T>");
            }

            var sagaDataType = genericArguments.Single();

            var associatedMessages = GetSagaMessages(sagaDataType, sagaType)
                .ToList();

            return new SagaMetadata(sagaType, sagaDataType, associatedMessages);
        }

        //public bool TryGetFinderType(Type messageType, out Type finderType)
        //{
        //    finderType = typeof(IFindSagas<>).MakeGenericType(this.Typ)
        //}

        public static bool IsSagaType(Type t)
        {            
            return t.IsSubclassOfRawGeneric(typeof(Saga<>)) && t != typeof(Saga<>) && !t.IsGenericType;
        }

        static Type GetBaseSagaType(Type toCheck)
        {
            var theObjectType = typeof(object);

            while (toCheck != null && toCheck != theObjectType)
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (cur == typeof(Saga<>))                
                {
                    return toCheck;
                }
                toCheck = toCheck.BaseType;
            }

            throw new InvalidOperationException($"");
        }        

        

        static IEnumerable<SagaMessage> GetSagaMessages(Type sagaDataType, Type sagaType)
        {
            var result = GetMessagesCorrespondingToFilterOnSaga(sagaType, typeof(ISagaStartedBy<>))
                .Select(messageType => new SagaMessage(sagaDataType, messageType, true)).ToList();

            foreach (var messageType in GetMessagesCorrespondingToFilterOnSaga(sagaType, typeof(Events.IHandles<>)))
            {
                if (result.Any(m => m.MessageType == messageType))
                {
                    continue;
                }
                result.Add(new SagaMessage(sagaDataType, messageType, false));
            }            

            return result;
        }

        static IEnumerable<Type> GetMessagesCorrespondingToFilterOnSaga(Type sagaType, Type filter)
        {
            foreach (var interfaceType in sagaType.GetInterfaces())
            {
                if(!interfaceType.IsSubclassOfRawGeneric(filter))
                {
                    continue;
                }

                foreach (var argument in interfaceType.GetGenericArguments())
                {
                    var genericType = filter.MakeGenericType(argument);
                    var isOfFilterType = genericType == interfaceType;
                    if (!isOfFilterType)
                    {
                        continue;
                    }
                    yield return argument;
                }
            }
        }
    }
}
