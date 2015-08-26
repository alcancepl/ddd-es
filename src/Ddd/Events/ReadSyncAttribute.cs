using System;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Ddd.Events
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ReadSyncAttribute : Attribute
    {
        public ReadSyncAttribute(params string[] readDbNames)
        {
            Names = readDbNames;
        }

        /// <summary>
        /// Describes which read-side databases (or views) are updated by the synchronizator this attribute is applied to.
        /// </summary>
        public string[] Names { get; private set; }
    }
}