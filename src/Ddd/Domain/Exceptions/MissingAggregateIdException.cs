using System;

namespace Ddd.Domain.Exceptions
{
    public class MissingAggregateIdException : System.Exception 
    {
        public MissingAggregateIdException(Type aggregateType)
            : base(string.Format("An aggregate of type {0} has no id set.", aggregateType.FullName))
        {
        }
    }
}
