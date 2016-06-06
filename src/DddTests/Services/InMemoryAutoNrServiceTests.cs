using Ddd.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ddd.Services.Tests
{
	[TestClass()]
	public class InMemoryAutoNrServiceTests
	{
		[TestMethod()]
		public void GetAutoNrIsThreadSafe()
		{
			var threadCount = 10;
			var callsPerThread = 1000;
			Thread[] threads = new Thread[threadCount];
			TestAutoNrData[,] results = new TestAutoNrData[threadCount, callsPerThread];
			TestAggregate[] aggregates = new TestAggregate[callsPerThread];
			for (int i = 0; i < callsPerThread; i++)
			{
				aggregates[i] = new TestAggregate();
			}

			var service = new InMemoryAutoNrService();

			for (int i = 0; i < threadCount; i++)
			{
				var threadIndex = i;
				threads[i] = new Thread(new ThreadStart(() =>
				{                    
					for (int n = 0; n < callsPerThread; n++)
					{
						var aggregateId = aggregates[n].Id.Value;
						results[threadIndex, n] = service.GetAutoNr<TestAutoNrConfig, TestAutoNrData>(
							"single-context",
							aggregateId,
							(sequanceNr, config, prev) =>
							{
								var result = new AutoNrResult<TestAutoNrConfig, TestAutoNrData>(config, new TestAutoNrData() { DocumentNr = $"DOC-{sequanceNr:d5}" });
								Thread.Sleep(10);
								Console.WriteLine($"Thread-{threadIndex:d2} generated new NR = {result.NrData.DocumentNr} for aggregate id {aggregateId}");
								return result;
							}).Result;
						Thread.Sleep(0);
					}
				}));
			}



			for (int i = 0; i < threadCount; i++)
			{
				threads[i].Start();
			}
			var timeout = TimeSpan.FromSeconds(60);
			for (int i = 0; i < threadCount; i++)
			{
				threads[i].Join(timeout);
			}
			for (int i = 0; i < callsPerThread; i++)
			{
				var expected = $"DOC-{i + 1:d5}";
				for (int threadIndex = 0; threadIndex < threadCount; threadIndex++)
				{
					var actual = results[threadIndex, i];
					Assert.IsTrue(actual.DocumentNr == expected);
				}
			}
		}

		public class TestAutoNrConfig
		{            
		}

		public class TestAutoNrData
		{
			public string DocumentNr { get; set; }
		}

		public class TestId : IAggregateIdentity
		{
			public TestId(string value)
			{
				Value = value;
			}
			public string Value { get; private set; }
		}

		public class TestAggregate : AggregateRoot<TestId>
		{
			public TestAggregate()
			{
				Id = new TestId(Guid.NewGuid().ToString("N"));
			}            
			
		}
	}
}