using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ddd
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUniqueService
    {
        /// <summary>
        /// The first time a value is used, the requestedKey is assigned to this value and returned.
        /// Afterwards, the same key is always returned (regardles of what requestedKey is passed). 
        /// </summary>
        /// <param name="valueGroup">defines a scope for unique values</param>        
        /// <param name="value">a value that should be unique in the valueGroup</param>
        /// <param name="requestedKey">a key that shouled be assigned to the unique value</param>
        /// <returns>a key for the value</returns>
        Guid GetOrAddUniqueValueKey(string valueGroup, string value, Guid requestedKey, bool ignoreCase = true);

        /// <summary>
        /// Attempts to get the key associated with the specified unique value.
        /// </summary>
        /// <param name="valueGroup">defines a scope for unique values</param>
        /// <param name="value">a unique value for which a key should be found</param>
        /// <param name="key">when this method returns, key contains the key for the unique value or Guid.Empty if the value was not found</param>
        /// <param name="ignoreCase"></param>
        /// <returns>true if the value was found; otherwise, false</returns>
        bool TryGetUniqueValueKey(string valueGroup, string value, out Guid key, bool ignoreCase = true);

		/// <summary>
		/// Attempts to remove value, key of the 
		/// </summary>
		/// <param name="valueGroup">defines a scope for unique values</param>        
		/// <param name="value">a value that should be unique in the valueGroup</param>
		/// <param name="requestedKey">a key that shouled be assigned to the unique value</param>
		/// <returns>true if value has been found and removed; otherwise false</returns>
		bool TryRemoveUniqueValueKey(string valueGroup, string value, Guid key, bool ignoreCase = true);
    }
}
