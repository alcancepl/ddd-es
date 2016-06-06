using System;
using System.Runtime.Serialization;

namespace Ddd.Sagas
{
    [Serializable]
    internal class SagaNotFoundException : Exception
    {
        public SagaNotFoundException()
        {
        }

        public SagaNotFoundException(string message) : base(message)
        {
        }

        public SagaNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected SagaNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}