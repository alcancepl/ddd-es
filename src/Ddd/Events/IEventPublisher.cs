using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd.Events
{
    /// <summary>
    /// A bus that only publish messages (events). It does not send messages (commands).
    /// </summary>
    public interface IEventPublisher
    {
		Task PublishAsync<TEvent>(Commands.CommandContext context, IEnumerable<TEvent> @events, CancellationToken cancellationToken = default(CancellationToken)) where TEvent : class, IEvent;
	}
}
