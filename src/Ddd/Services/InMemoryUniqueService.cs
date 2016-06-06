using Ddd.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ddd.Services
{
	public class InMemoryUniqueService : IUniqueService
	{
		static InMemoryUniqueService()
		{
			groups = new ConcurrentDictionary<string, ConcurrentDictionary<string, IAggregateIdentity>>();
		}

		private static ConcurrentDictionary<string, ConcurrentDictionary<string, IAggregateIdentity>> groups;

		public Task<TAggregateIdentity> GetOrAddUniqueValueKey<TAggregateIdentity>(string valueGroup, string value, TAggregateIdentity requestedKey, bool ignoreCase = true) 
			where TAggregateIdentity : class, IAggregateIdentity
		{
			if (string.IsNullOrEmpty(valueGroup))
				throw new ArgumentNullException("valueGroup");
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException("value");

			var comparer = ignoreCase
					? StringComparer.InvariantCultureIgnoreCase
					: StringComparer.InvariantCulture;

			var groupValues = groups.GetOrAdd(
				valueGroup,
				(vg) => new ConcurrentDictionary<string, IAggregateIdentity>(
					new[] { new KeyValuePair<string, IAggregateIdentity>(value, requestedKey) },
					comparer));

			return Task.FromResult((TAggregateIdentity)groupValues.GetOrAdd(value, requestedKey));

			//var res = groupValues.GetOrAdd(value, requestedKey);
			//var result = res == Guid.Empty
			//    ? groupValues.TryUpdate(value, requestedKey, Guid.Empty)
			//        ? requestedKey
			//        : groupValues.GetOrAdd(value, requestedKey)
			//    : res;
			//return Task.FromResult(result);
		}

		public Task<TryGetUniqueValueKeyResult<TAggregateIdentity>> TryGetUniqueValueKey<TAggregateIdentity>(string valueGroup, string value, bool ignoreCase = true)
			where TAggregateIdentity : class, IAggregateIdentity
		{
			if (string.IsNullOrEmpty(valueGroup))
				throw new ArgumentNullException("valueGroup");
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException("value");

			ConcurrentDictionary<string, IAggregateIdentity> groupValues;
			IAggregateIdentity key;
			if (groups.TryGetValue(valueGroup, out groupValues) && groupValues.TryGetValue(value, out key))
			{
				return Task.FromResult(TryGetUniqueValueKeyResult<TAggregateIdentity>.WithResult((TAggregateIdentity)key));
			}
			return Task.FromResult(TryGetUniqueValueKeyResult<TAggregateIdentity>.Empty);            
		}

		public Task<bool> TryRemoveUniqueValueKey(string valueGroup, string value, IAggregateIdentity key, bool ignoreCase = true)
		{
			if (string.IsNullOrEmpty(valueGroup))
				throw new ArgumentNullException("valueGroup");
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException("value");

			ConcurrentDictionary<string, IAggregateIdentity> groupValues;

			if (!groups.TryGetValue(valueGroup, out groupValues))
			{                
				return Task.FromResult(false);
			}
			IAggregateIdentity removedKey;
			return Task.FromResult(groupValues.TryRemove(value, out removedKey));
		}

		public void Clear()
		{
			groups.Clear();
		}

		public Task<TAggregateIdentity> UpdateUniqueValueKey<TAggregateIdentity>(string valueGroup, string fromValue, string toValue, TAggregateIdentity key, int retries, bool ignoreCase) where TAggregateIdentity : class, IAggregateIdentity
		{
			throw new NotImplementedException();
		}
	}
}
