using System;

namespace Ddd.Events
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
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