using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd.Domain.Exceptions
{
    public class AggregateNotFoundException : System.Exception
    {
        public AggregateNotFoundException(Type aggregateType, Guid id)
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
