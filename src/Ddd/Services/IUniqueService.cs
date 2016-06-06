using Ddd.Domain;
using System;
using System.Threading.Tasks;

namespace Ddd.Services
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
        Task<TAggregateIdentity> GetOrAddUniqueValueKey<TAggregateIdentity>(string valueGroup, string value, TAggregateIdentity requestedKey, bool ignoreCase = true) where TAggregateIdentity : class, IAggregateIdentity;

        /// <summary>
        /// Attempts to get the key associated with the specified unique value.
        /// </summary>
        /// <param name="valueGroup">defines a scope for unique values</param>
        /// <param name="value">a unique value for which a key should be found</param>
        /// <param name="key">when this method returns, key contains the key for the unique value or Guid.Empty if the value was not found</param>
        /// <param name="ignoreCase"></param>
        /// <returns>a key if the value was found, null otherwise</returns>
        Task<TryGetUniqueValueKeyResult<TAggregateIdentity>> TryGetUniqueValueKey<TAggregateIdentity>(string valueGroup, string value, bool ignoreCase = true) where TAggregateIdentity : class, IAggregateIdentity;

        /// <summary>
        /// Attempts to remove value, key of the 
        /// </summary>
        /// <param name="valueGroup">defines a scope for unique values</param>        
        /// <param name="value">a value that should be unique in the valueGroup</param>
        /// <param name="requestedKey">a key that shouled be assigned to the unique value</param>
        /// <returns>true if value has been found and removed; otherwise false</returns>
        Task<bool> TryRemoveUniqueValueKey(string valueGroup, string value, IAggregateIdentity key, bool ignoreCase = true);

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TAggregateIdentity"></typeparam>
		/// <param name="valueGroup"></param>
		/// <param name="fromValue"></param>
		/// <param name="toValue"></param>
		/// <param name="key"></param>
		/// <param name="ignoreCase"></param>
		/// <returns></returns>
		Task<TAggregateIdentity> UpdateUniqueValueKey<TAggregateIdentity>(string valueGroup, string fromValue, string toValue, TAggregateIdentity key, int retries, bool ignoreCase = true) where TAggregateIdentity : class, IAggregateIdentity;
    }

    public class TryGetUniqueValueKeyResult<TAggregateIdentity> where TAggregateIdentity : class, IAggregateIdentity
    {
        public static TryGetUniqueValueKeyResult<TAggregateIdentity> WithResult(TAggregateIdentity result)
        {
            if (result == null)
                throw new ArgumentNullException("result");
            return new TryGetUniqueValueKeyResult<TAggregateIdentity> { HasValue = true, Value = result };
        }

        static TryGetUniqueValueKeyResult<TAggregateIdentity> noResult = new TryGetUniqueValueKeyResult<TAggregateIdentity> { HasValue = false, Value = default(TAggregateIdentity) };

        public static TryGetUniqueValueKeyResult<TAggregateIdentity> Empty => noResult;

        public bool HasValue { get; private set; }
        public TAggregateIdentity Value { get; private set; }
    }
}
