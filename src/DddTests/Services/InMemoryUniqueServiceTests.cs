using Ddd.Domain;
using DddTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ddd.Services.Tests
{
    [TestClass()]
    public class InMemoryUniqueServiceTests
    {
        [TestMethod()]
        public void GetOrAddUniqueValueKeyIsThreadSafe()
        {
            var count = 1000;
            var service = new InMemoryUniqueService();
            var data = Enumerable.Range(0, count).Select(i => Tuple.Create($"val-{i:d4}", new TestId(Guid.NewGuid().ToString("N")))).ToArray();

            var threadWork = new Func<int, Task<TestId[]>>(async (threadNr) =>
            {
                var res = new TestId[count];
                foreach (var i in Enumerable.Range(0, count).OrderBy(i => Guid.NewGuid()))
                {
                    res[i] = await service.GetOrAddUniqueValueKey("single-group", data[i].Item1, data[i].Item2);
                    //Console.WriteLine($"Key for value {data[i].Item1} is {res[i]} by thread {threadNr:d3}.");
                    await Task.Delay(i % 10);
                }
                return res;
            });
            var results = threadWork.ExecuteMultithread();

            for (int threadIndex = 0; threadIndex < results.Length; threadIndex++)
            {
                var threadResults = results[threadIndex];
                for (int result = 0; result < count; result++)
                {
                    var expected = data[result].Item2;
                    var actual = threadResults[result];
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        [TestMethod()]
        public void TryRemoveUniqueValueKeyIsThreadSafe()
        {
            var count = 1000;
            var service = new InMemoryUniqueService();
            var data = Enumerable.Range(0, count).Select(i => new Tuple<string, TestId>($"val-{i:d4}", new TestId(Guid.NewGuid().ToString("N")))).ToArray();

            Task.WaitAll(data.Select(d => service.GetOrAddUniqueValueKey("single-group", d.Item1, d.Item2)).ToArray());

            var threadWork = new Func<int, Task<bool[]>>(async (threadNr) =>
            {
                var res = new bool[count];
                foreach (var i in Enumerable.Range(0, count).OrderBy(i => Guid.NewGuid()))
                {
                    res[i] = service.TryRemoveUniqueValueKey("single-group", data[i].Item1, data[i].Item2).Result;
                    //Console.WriteLine($"Value index {i:d4} removeal is {res[i]} by thread {threadNr:d3}.");
                    await Task.Delay(i % 10);
                }
                return res;
            });
            var results = threadWork.ExecuteMultithread();

            for (int result = 0; result < count; result++)
            {
                var removedByCount = 0;
                for (int threadIndex = 0; threadIndex < results.Length; threadIndex++)
                {
                    var removed = results[threadIndex][result];
                    if (removed)
                    {
                        //Console.WriteLine($"Value index {result:d4} removed by thread {threadIndex:d3}.");
                        removedByCount++;
                    }
                }
                Assert.AreEqual(1, removedByCount);
            }
        }

        public class TestId : IAggregateIdentity
        {
            public TestId(string value)
            {
                Value = value;
            }
            public string Value { get; private set; }
        }
    }

    
}