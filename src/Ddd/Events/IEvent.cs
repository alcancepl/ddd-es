using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd.Events
{
    /// <summary>
    /// A domain event, that has happend in the context of an aggregate.
    /// </summary>
    public interface IEvent: IMessage
    {
        /// <summary>
        /// Aggregate Root Id this event belongs to.
        /// </summary>
        Guid Id { get; }
        //int Version { get; set; }
    }
}
