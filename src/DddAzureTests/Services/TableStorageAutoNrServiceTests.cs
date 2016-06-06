using Ddd.Domain;
using Ddd.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Ddd.Services.Tests
{
	[TestClass()]
	public class TableStorageAutoNrServiceTests
	{
		string connString = ConfigurationManager.AppSettings["storageconnection"];

		[TestMethod()]
		public async Task GetAutoNrGeneratesUniqueNrsTest()
		{
			var tableName = RandomTableNameName();
			Console.WriteLine($"Table name for this test: {tableName}");

			var service = new TableStorageAutoNrService(connString, tableName);
			service.Init();

			var offers = Enumerable.Range(1, 30).Select(day => new TestOffer() { Date = new DateTime(2015, 11, day) }).ToArray();
			var nrs = new NrData[30];

			for (int i = 0; i < 30; i++)
			{
				var offer = offers[i];
				var generator = new AutoNrGenerator<SequenceData, NrData>((sequenceNewNr, config, prev) =>
				{
					var sequence = $"OFF-{offer.Date.Year}/{offer.Date.Month}/";
					var nr = $"{sequence}{sequenceNewNr:d5}";
					return new AutoNrResult<SequenceData, NrData>(
						config ?? new SequenceData(),
						new NrData() { DocumentNr = nr });
				});
				nrs[i] = await service.GetAutoNr("rcsoffers", offer.Id.Value, generator);
				//Console.WriteLine(nrs[i]);
			}
			for (int i = 0; i < 30; i++)
			{
				Assert.AreEqual($"OFF-2015/11/{i + 1:d5}", nrs[i].DocumentNr);
			}
		}

		[TestMethod()]
		public async Task ConcurentAutoNrGenerationWorksAsExpectedWhenConfigRowIsCreated()
		{
			var tableName = RandomTableNameName();
			Console.WriteLine($"Table name for this test: {tableName}");

			var service1 = new TableStorageAutoNrService(connString, tableName);
			var service2 = new TableStorageAutoNrService(connString, tableName);

			service1.Init();
			service2.Init();

			var offer1 = new TestOffer() { Date = new DateTime(2015, 11, 21) };
			var offer2 = new TestOffer() { Date = new DateTime(2015, 11, 22) };

			NrData nr1 = null;
			var nr2 = await service2.GetAutoNr<SequenceData, NrData>("rcsoffers", offer2.Id.Value,
				new AutoNrGenerator<SequenceData, NrData>((sequenceNewNr, config, prev) =>
				{
					nr1 = service2.GetAutoNr<SequenceData, NrData>("rcsoffers", offer1.Id.Value,
						new AutoNrGenerator<SequenceData, NrData>((sequenceNewNr1, config1, prev1) =>
						{
							var sequence1 = $"OFF-{offer1.Date.Year}/{offer1.Date.Month}/";
							var nr11 = $"{sequence1}{sequenceNewNr1:d5}";
							return new AutoNrResult<SequenceData, NrData>(
								config1 ?? new SequenceData(),
								new NrData() { DocumentNr = nr11 });
						})
					).Result;
					var sequence = $"OFF-{offer2.Date.Year}/{offer2.Date.Month}/";
					var nr = $"{sequence}{sequenceNewNr:d5}";
					return new AutoNrResult<SequenceData, NrData>(
						config ?? new SequenceData(),
						new NrData() { DocumentNr = nr }
						);
				})
			);

			Assert.AreEqual("OFF-2015/11/00001", nr1.DocumentNr);
			Assert.AreEqual("OFF-2015/11/00002", nr2.DocumentNr);
		}



		[TestMethod()]
		public async Task ConcurentAutoNrGenerationWorksAsExpectedWhenTheConfigRowIsModified()
		{
			var tableName = RandomTableNameName();
			Console.WriteLine($"Table name for this test: {tableName}");

			var service1 = new TableStorageAutoNrService(connString, tableName);
			var service2 = new TableStorageAutoNrService(connString, tableName);
			var service3 = new TableStorageAutoNrService(connString, tableName);

			service1.Init();
			service2.Init();
			service3.Init();

			var offer1 = new TestOffer() { Date = new DateTime(2015, 11, 21) };
			var offer2 = new TestOffer() { Date = new DateTime(2015, 11, 22) };
			var offer3 = new TestOffer() { Date = new DateTime(2015, 11, 23) };

			var nr1 = await service1.GetAutoNr<SequenceData, NrData>("rcsoffers", offer1.Id.Value,
				new AutoNrGenerator<SequenceData, NrData>((sequenceNewNr, config, prev) =>
				{
					var sequence = $"OFF-{offer1.Date.Year}/{offer1.Date.Month}/";
					var nr = $"{sequence}{sequenceNewNr:d5}";
					return new AutoNrResult<SequenceData, NrData>(
						config ?? new SequenceData(),
						new NrData() { DocumentNr = nr });
				}));
			NrData nr2 = null;
			var nr3 = await service3.GetAutoNr<SequenceData, NrData>("rcsoffers", offer3.Id.Value,
				new AutoNrGenerator<SequenceData, NrData>((sequenceNewNr, config, prev) =>
				{
					nr2 = service2.GetAutoNr<SequenceData, NrData>("rcsoffers", offer2.Id.Value,
						new AutoNrGenerator<SequenceData, NrData>((sequenceNewNr2, config2, prev2) =>
						{
							var sequence2 = $"OFF-{offer2.Date.Year}/{offer2.Date.Month}/";
							var nr11 = $"{sequence2}{sequenceNewNr2:d5}";
							return new AutoNrResult<SequenceData, NrData>(
								config2 ?? new SequenceData(),
								new NrData() { DocumentNr = nr11 });
						})
					).Result;
					var sequence = $"OFF-{offer3.Date.Year}/{offer3.Date.Month}/";
					var nr = $"{sequence}{sequenceNewNr:d5}";
					return new AutoNrResult<SequenceData, NrData>(
						config ?? new SequenceData(),
						new NrData() { DocumentNr = nr });
				})
			);

			Assert.AreEqual("OFF-2015/11/00001", nr1.DocumentNr);
			Assert.AreEqual("OFF-2015/11/00002", nr2.DocumentNr);
			Assert.AreEqual("OFF-2015/11/00003", nr3.DocumentNr);
		}


		[TestMethod()]
		public async Task ConcurentAutoNrGenerationForTheSameDocumentWorksAsExpectedWhenTwoServicesAsksForNumber()
		{
			var tableName = RandomTableNameName();
			Console.WriteLine($"Table name for this test: {tableName}");

			var service1 = new TableStorageAutoNrService(connString, tableName);
			var service2 = new TableStorageAutoNrService(connString, tableName);
			var service3 = new TableStorageAutoNrService(connString, tableName);

			service1.Init();
			service2.Init();
			service3.Init();

			var offer1 = new TestOffer() { Date = new DateTime(2015, 11, 21) };
			var offer2 = new TestOffer() { Date = new DateTime(2015, 11, 22) };

			var generator = new AutoNrGenerator<SequenceData, NrData>((sequenceNewNr, config, prev) =>
			{
				var sequence = $"OFF-{offer2.Date.Year}/{offer2.Date.Month}/";
				var nr = $"{sequence}{sequenceNewNr:d5}";
				return new AutoNrResult<SequenceData, NrData>(
					config ?? new SequenceData(),
					new NrData() { DocumentNr = nr });
			});

			var nr1 = await service1.GetAutoNr<SequenceData, NrData>(
				"rcsoffers",
				offer1.Id.Value,
				generator);
			NrData nr2 = null;

			var nr3 = await service3.GetAutoNr<SequenceData, NrData>(
				"rcsoffers",
				offer2.Id.Value,
				(sequenceNewNr, config, prev) =>
				{
					nr2 = service2.GetAutoNr<SequenceData, NrData>("rcsoffers", offer2.Id.Value, generator).Result;
					return generator(sequenceNewNr, config, prev);
				});

			Assert.AreEqual("OFF-2015/11/00001", nr1.DocumentNr);
			Assert.AreEqual(nr2.DocumentNr, nr3.DocumentNr);
			Assert.AreEqual("OFF-2015/11/00002", nr2.DocumentNr);
		}

		private string RandomTableNameName(string prefix = "autonrtests")
		{
			var rgx = new System.Text.RegularExpressions.Regex(@"[^a-z0-9]");
			var random = rgx.Replace(Guid.NewGuid().ToString("N").ToLowerInvariant(), "");
			return $"{prefix}{random}";
		}

		public class SequenceData
		{
		}

		public class NrData
		{
			public string DocumentNr { get; set; }
		}

		public class TestOffer : TestAggregate
		{
			public DateTime Date { get; set; }
		}

		public class TestId : IAggregateIdentity
		{
			public string Value { get; set; }
		}

		public class TestAggregate : IAggregate<TestId>
		{
			public TestAggregate()
			{
				Id = new TestId { Value = Guid.NewGuid().ToString("N") };
			}

			public TestId Id { get; private set; }


			public IEnumerable<IEvent> UncommitedEvents
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public int Version
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			public void ApplyEvent(IEvent @event)
			{
				throw new NotImplementedException();
			}

			public void ClearUncommitedEvents()
			{
				throw new NotImplementedException();
			}
		}
	}

}