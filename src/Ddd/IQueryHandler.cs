using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ddd
{
    public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Wykonuje asynchronicznie podane query.
        /// </summary>
        /// <param name="query">query, które należy wykonać</param>
        /// <param name="cancellationToken"></param>
        /// <returns>wynik query</returns>
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default(CancellationToken));
    }
}
