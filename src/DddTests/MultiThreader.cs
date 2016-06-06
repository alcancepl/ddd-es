using System;
using System.Linq;
using System.Threading.Tasks;

namespace DddTest
{
    public static class MultiTrheader
    {
        public static T[] ExecuteMultithread<T>(this Func<int, Task<T>> work, int threadCount = 10, TimeSpan timeout = default(TimeSpan))
        {
            if (timeout == default(TimeSpan))
                timeout = TimeSpan.FromSeconds(60);

            var tasks = Enumerable.Range(1, threadCount).Select(threadNr => Task.Run(() => work(threadNr))).ToArray();

            if (!Task.WaitAll(tasks, timeout))
                throw new TimeoutException($"Timeout after {timeout.TotalSeconds} seconds.");

            return tasks.Select(t => t.Result).ToArray();
        }
    }
}
