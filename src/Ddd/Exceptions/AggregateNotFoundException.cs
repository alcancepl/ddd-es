﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd.Exceptions
{
    public class AggregateNotFoundException : Exception
    {
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
