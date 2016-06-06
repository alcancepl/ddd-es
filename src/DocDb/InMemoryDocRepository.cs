using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocDb
{
	internal static class InMemoryCollections<S> where S : IDocRepositorySpace
	{
		internal static readonly ConcurrentDictionary<string, string> Documents = new ConcurrentDictionary<string, string>();
	}

	public class InMemoryDocRepository<T, S> : IDocRepository<T, S>
		where T : class
		where S : IDocRepositorySpace
	{
		static ConcurrentDictionary<string, string> Collection => InMemoryCollections<S>.Documents;

		static string Serialize(object value) => JsonConvert.SerializeObject(value, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
		static DocumentWrap<T> Deserialize(string document) => JsonConvert.DeserializeObject<DocumentWrap<T>>(document, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });

		//static ConcurrentDictionary<string, T> collection = new ConcurrentDictionary<string, T>();

		public Task<DocumentWrap<T>> AddAsync(DocumentWrap<T> item)
		{
			
			if (!Collection.TryAdd(item.Id, Serialize(item)))
				throw new InMemoryDocOptimisticConcurrencyException($"Cannot add {typeof(T).Name} with id {item.Id}.");
			return Task.FromResult(item);
		}

		public Task<DocumentWrap<T>> DeleteAsync(string id)
		{
			string removedItem;
			if (!Collection.TryRemove(id, out removedItem))
				throw new InMemoryDocOptimisticConcurrencyException($"Cannot remove {typeof(T).Name} with id {id}.");
			return Task.FromResult(Deserialize(removedItem));
		}

		public Task<Result<T>> GetAsync(string id)
		{
			string value;
			if (!Collection.TryGetValue(id, out value))
				return Task.FromResult(new Result<T>(null));
			return Task.FromResult(new Result<T>(Deserialize(value).Document));
		}

		public Task<IList<T>> GetAsync(Func<IQueryable<DocumentWrap<T>>, IQueryable<T>> queryBuilder, int? maxItemCount = default(int?))
		{
			var idStartsWithTypeInfo = DocumentWrap<T>.TypeInfo + "|"; // "|" jest potrzebne ponieważ ExplorerItem oraz ExplorerItemFolder zaczynają nazwę tak samo a my potrzebujemy tylko ExplorerItemFolder
			var query = Collection
				.Where(kv => kv.Key.StartsWith(idStartsWithTypeInfo))
				.Select(kv => Deserialize(kv.Value))
				.AsQueryable();

			var finalQuery = queryBuilder(query);
			var result = finalQuery.Take(maxItemCount ?? int.MaxValue).ToList();
			return Task.FromResult<IList<T>>(result);
		}

		public Task<IList<T>> GetAsync(Func<IQueryable<DocumentWrap<T>>, IQueryable<DocumentWrap<T>>> queryBuilder, int? maxItemCount = default(int?))
		{
			var idStartsWithTypeInfo = DocumentWrap<T>.TypeInfo + "|"; // "|" jest potrzebne ponieważ ExplorerItem oraz ExplorerItemFolder zaczynają nazwę tak samo a my potrzebujemy tylko ExplorerItemFolder
			var query = Collection
				.Where(kv => kv.Key.StartsWith(idStartsWithTypeInfo))
				.Select(kv => Deserialize(kv.Value))
				.AsQueryable();

			var finalQuery = queryBuilder(query);
			var result = finalQuery.Take(maxItemCount ?? int.MaxValue).ToList();
			var resultList = new List<T>();
			foreach (var item in result)
			{
				resultList.Add(item.Document);
			}
			return Task.FromResult<IList<T>>(resultList);
		}

		//public Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> unwrappedFilter, Expression<Func<DocumentWrap<T>, bool>> wrappedFilter)
		//{
		//    var wrappedFunc = wrappedFilter.Compile();
		//    var unwrappedFunc = unwrappedFilter.Compile();
		//    var result = collection
		//        .Where(kv => wrappedFunc(new DocumentWrap<T>(kv.Value, kv.Key)))
		//        .Where(kv => unwrappedFunc(kv.Value))
		//        .Select(kv => kv.Value);
		//    return Task.FromResult(result);
		//}

		public Task<DocumentWrap<T>> UpdateAsync(DocumentWrap<T> item)
		{
			var oldValue = Collection[item.Id];
			if (!Collection.TryUpdate(item.Id, Serialize(item), oldValue))
				throw new InMemoryDocOptimisticConcurrencyException($"Cannot update {typeof(T).Name} with id {item.Id}.");
			return Task.FromResult(item);
		}

		public Task<DocumentWrap<T>> UpsertAsync(DocumentWrap<T> item)
		{
			var document = Serialize(item);
			var doc = Collection.AddOrUpdate(item.Id, document, (id, val) => document);
			return Task.FromResult(Deserialize(doc));
		}

	}

	[Serializable]
	public class InMemoryDocOptimisticConcurrencyException : Exception
	{
		public InMemoryDocOptimisticConcurrencyException() { }
		public InMemoryDocOptimisticConcurrencyException(string message) : base(message) { }
		public InMemoryDocOptimisticConcurrencyException(string message, Exception inner) : base(message, inner) { }
		protected InMemoryDocOptimisticConcurrencyException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{ }
	}
}
