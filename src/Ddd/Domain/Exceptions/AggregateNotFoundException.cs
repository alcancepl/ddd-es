using System;

namespace Ddd.Domain.Exceptions
{
    public class AggregateNotFoundException : AggregateException
    {
        public AggregateNotFoundException(Type aggregateType, IAggregateIdentity id)
            : base(string.Format("Aggregate {0} with id {1} was not found.", aggregateType.FullName, id))
        {
        }

        public AggregateNotFoundException(string message)
            : base(message)
        {
        }

        public AggregateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
