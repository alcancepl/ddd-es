﻿using System;

namespace Ddd.Domain.Exceptions
{
    public class MissingEventIdException : System.Exception 
    {
        public MissingEventIdException(Type aggregateType, Type eventType)
            : base(string.Format("An event of type {0} was tried to save from {1} but the event has no id set.", eventType.FullName, aggregateType.FullName))
        {
        }
    }
}
