using System;
using System.Runtime.Serialization;

namespace Ddd.Services
{
    [Serializable]
    public class TableStorageAutoNrServiceDataMismatchException : Exception
    {
        public TableStorageAutoNrServiceDataMismatchException()
        {
        }

        public TableStorageAutoNrServiceDataMismatchException(string message) : base(message)
        {
        }

        public TableStorageAutoNrServiceDataMismatchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TableStorageAutoNrServiceDataMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}