using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocDb
{
    public interface IDocRepositorySpace
    {
        DocumentClient Client { get; }
        Database Database { get; }
        DocumentCollection Collection { get; }
        JsonSerializerSettings JsonSettings { get; }
    }

    public interface IReadOnlyDocRepository<T, in S>
        where T : class
        where S : IDocRepositorySpace
    {
        Task<Result<T>> GetAsync(string id);
        Task<IList<T>> GetAsync(Func<IQueryable<DocumentWrap<T>>, IQueryable<DocumentWrap<T>>> queryBuilder, int? maxItemCount = null);
		Task<IList<T>> GetAsync(Func<IQueryable<DocumentWrap<T>>, IQueryable<T>> queryBuilder, int? maxItemCount = default(int?));
	}

    public interface IDocRepository<T, in S> : IReadOnlyDocRepository<T, S>
        where T : class
        where S : IDocRepositorySpace
    {
        Task<DocumentWrap<T>> AddAsync(DocumentWrap<T> item);
        Task<DocumentWrap<T>> UpdateAsync(DocumentWrap<T> item);
        Task<DocumentWrap<T>> UpsertAsync(DocumentWrap<T> item);
        Task<DocumentWrap<T>> DeleteAsync(string id);
    }



    public class Result<T>
    {
        private readonly bool hasResult;
        private readonly T value;
        public Result(T value = default(T))
        {
            hasResult = value != null && !value.Equals(default(T));
            this.value = value;
        }
        public bool HasValue => hasResult;
        public T Value => value;
    }


}
