using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rcs.Eurad.Exceptions
{    
    public class WrongExpectedVersionException : Exception
    {       

        public WrongExpectedVersionException(int expectedVersion, int currentversion)
            : base(string.Format("Expected version {0} but the version is {1}.", expectedVersion, currentversion))
        {
            ExpectedVersion = expectedVersion;
            Crrentversion = currentversion;
        }

        public int Crrentversion { get; private set; }
        public bool ExpectedToBeNewButWasNot { get { return ExpectedVersion < 0; } }
        public int ExpectedVersion { get; private set; }
    }
}
