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

        public Guid GetOrAddUniqueValueKey(string valueGroup, string value, Guid requestedKey, bool ignoreCase = true)
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
                return requestedKey;
            }
            else
            {
                return res;
            }
        }

        public bool TryGetUniqueValueKey(string valueGroup, string value, out Guid key, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(valueGroup))
                throw new ArgumentNullException("valueGroup");
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("value");

            ConcurrentDictionary<string, Guid> groupValues;

            if (!groups.TryGetValue(valueGroup, out groupValues))
            {
                key = Guid.Empty;
                return false;
            }

            var res = groupValues.TryGetValue(value, out key);
            return res && key != Guid.Empty;
        }

        public bool TryRemoveUniqueValueKey(string valueGroup, string value, Guid key, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(valueGroup))
                throw new ArgumentNullException("valueGroup");
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("value");

            ConcurrentDictionary<string, Guid> groupValues;

            if (!groups.TryGetValue(valueGroup, out groupValues))
            {
                key = Guid.Empty;
                return false;
            }


            return groupValues.TryUpdate(value, Guid.Empty, key);
        }

        public void Clear()
        {
            groups.Clear();
        }
    }
}
