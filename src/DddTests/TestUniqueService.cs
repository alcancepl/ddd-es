using Ddd;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DddTest
{
    public class TestUniqueService : IUniqueService
    {
        static TestUniqueService()
        {
            groups = new ConcurrentDictionary<string, ConcurrentDictionary<string, Guid>>();
        }

        private static ConcurrentDictionary<string, ConcurrentDictionary<string, Guid>> groups;

        public Task<Guid> GetOrAddUniqueValueKey(string valueGroup, string value, Guid requestedKey, bool ignoreCase = true)
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
                (vg) => new ConcurrentDictionary<string, Guid>(
                    new[] { new KeyValuePair<string, Guid>(value, requestedKey) },
                    comparer));

            var res = groupValues.GetOrAdd(value, requestedKey);
            if (res == Guid.Empty)
            {
                groupValues.TryUpdate(value, requestedKey, Guid.Empty);
                return Task.FromResult(requestedKey);
            }
            else
            {
                return Task.FromResult(res);
            }
        }

        public Task<Tuple<bool, Guid>> TryGetUniqueValueKey(string valueGroup, string value, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(valueGroup))
                throw new ArgumentNullException("valueGroup");
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("value");

            ConcurrentDictionary<string, Guid> groupValues;

			Guid key;
			if (!groups.TryGetValue(valueGroup, out groupValues))
            {
                key = Guid.Empty;
                return Task.FromResult(new Tuple<bool, Guid>(false, key));
            }

            var res = groupValues.TryGetValue(value, out key);
            return Task.FromResult(new Tuple<bool, Guid>(res && key != Guid.Empty, key));
        }

        public Task<bool> TryRemoveUniqueValueKey(string valueGroup, string value, Guid key, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(valueGroup))
                throw new ArgumentNullException("valueGroup");
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("value");

            ConcurrentDictionary<string, Guid> groupValues;

            if (!groups.TryGetValue(valueGroup, out groupValues))
            {
                key = Guid.Empty;
                return Task.FromResult(false);
            }


            return Task.FromResult(groupValues.TryUpdate(value, Guid.Empty, key));
        }

        public void Clear()
        {
            groups.Clear();
        }
    }
}
