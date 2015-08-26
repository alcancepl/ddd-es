using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd.Events
{
    /// <summary>
    /// Represents a map of domain event types to read database names.
    /// </summary>
    public interface IReadSyncMap
    {
        string[] EventTypeToReadDbName(Type eventType);
    }

    public class ReadSyncMap : IReadSyncMap
    {
        /// <summary>
        /// Parses passed types and maps implemented IHandle<TEvent> to names specified in the applied ReadSync attribute(s).
        /// </summary>
        /// <param name="eventHandlers"></param>
        /// <returns>a sync map</returns>
        public static IReadSyncMap Create(IEnumerable<Type> eventHandlers)
        {
            return new ReadSyncMap(eventHandlers);
        }

        /// <summary>
        /// Key = type of the event, Values = list of read dbs names (views)
        /// </summary>
        private readonly IDictionary<Type, List<string>> map;
        private ReadSyncMap(IEnumerable<Type> eventHandlers)
        {
            map = new Dictionary<Type, List<string>>();
            foreach (var eventHandler in eventHandlers)
            {
                var names = eventHandler.GetCustomAttributes(typeof(ReadSyncAttribute), true)
                    .Cast<ReadSyncAttribute>()
                    .SelectMany(attrib => attrib.Names)
                    .Distinct()
                    .ToList();
                if (names.Count == 0)
                    continue; // not marked with attribute [ReadSync("")]
                var interfaces = eventHandler.GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    if (!@interface.IsGenericType 
                        || @interface.GetGenericTypeDefinition() != typeof(IHandles<>)
                        || @interface.GenericTypeArguments.Length != 1
                        || !typeof(IEvent).IsAssignableFrom(@interface.GenericTypeArguments[0]))
                        continue; // implemented interface is not IHandles<TEvent> where TEvent: IEvent
                    
                    var eventType = @interface.GenericTypeArguments[0];

                    List<string> eventTypeNames;
                    if (!map.TryGetValue(eventType, out eventTypeNames))
                    {
                        eventTypeNames = new List<string>();
                        map.Add(eventType, eventTypeNames);
                    }
                    eventTypeNames.AddRange(names);
                }                
            }
        }
        public string[] EventTypeToReadDbName(Type eventType)
        {
            List<string> names;
            if (!map.TryGetValue(eventType, out names))
                return new string[0];
            return names.ToArray();
        }
    }

}
