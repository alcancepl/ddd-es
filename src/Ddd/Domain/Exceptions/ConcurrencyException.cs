using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd.Domain.Exceptions
{
    public class ConcurrencyException : System.Exception
    {
        public ConcurrencyException(Type aggregateType, Guid id)
            : base(string.Format("A different version than expected was found in aggregate {0} {1}", aggregateType.FullName, id))
        {            
        }

        public ConcurrencyException(Type aggregateType, Guid id, Exception innerException)
            : base(string.Format("A different version than expected was found in aggregate {0} {1}", aggregateType.FullName, id), innerException)
        {            
        }

        //public ConcurrencyException(Type aggregateType, Guid id, int expectedVersion, int currentVersion)
        //    : base(string.Format("A different version ({2}) than expected ({3}) was found in aggregate {0} {1}", aggregateType.FullName, id, currentVersion, expectedVersion))
        //{
        //    this.CrrentVersion = currentVersion;
        //    this.ExpectedVersion = expectedVersion;
        //}

        //public int? CrrentVersion { get; private set; }
        //public bool? ExpectedToBeNewButWasNot
        //{
        //    get
        //    {
        //        if (CrrentVersion.HasValue && ExpectedVersion.HasValue)
        //            return ExpectedVersion.Value < 0;
        //        return null;
        //    }
        //}
        //public int? ExpectedVersion { get; private set; }
    }
}
