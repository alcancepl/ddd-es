using System;

namespace Ddd.Domain.Exceptions
{
    [System.Serializable]
    public class AggregateException : Exception
    {
        public AggregateException() { }
        public AggregateException(string message) : base(message) { }
        public AggregateException(string message, Exception inner) : base(message, inner) { }
        protected AggregateException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context)
        { }
    }

}
