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
	public class TableStorageUniqueServiceTests
	{
		string connString = ConfigurationManager.AppSettings["storageconnection"];

		[TestMethod()]
		public async Task UpdateUniqueValueKeyTest()
		{
			var tableName = RandomTableNameName();
			Console.WriteLine($"Table name for this test: {tableName}");

			var service = new TableStorageUniqueService(connString, tableName);
			service.Init();

			const string ContractorVatNrInCountryForCompanyAccount = "ContractorVatNrInCountryForCompanyAccount";

			var companyAccountId = Guid.NewGuid();
			var countryCode = "PL";
			var vatNumber = "8971649500";

			var contractorId = new TestId(Guid.NewGuid());

			var value = String.Format("{0}:{1}:{2}", companyAccountId.ToString("N"), countryCode, vatNumber);

			var id = await service.GetOrAddUniqueValueKey(ContractorVatNrInCountryForCompanyAccount, value, contractorId);

			if (!id.Equals(contractorId))
			{
				Assert.Fail();
			}

			var updatedValue = String.Format("{0}:{1}:{2}", companyAccountId.ToString("N"), countryCode, "8971649501");

			var id2 = await service.UpdateUniqueValueKey(ContractorVatNrInCountryForCompanyAccount, value, updatedValue, contractorId, 5);

			if (!id2.Equals(contractorId))
			{
				Assert.Fail();
			}


			var contractorId2 = new TestId(Guid.NewGuid());

			var task1 = service.UpdateUniqueValueKey(ContractorVatNrInCountryForCompanyAccount, updatedValue, value, contractorId, 5);
			var task2 = service.GetOrAddUniqueValueKey(ContractorVatNrInCountryForCompanyAccount, value, contractorId2);
			var t = await TaskEx.WhenAll(task1, task2);

			if (!t.Item1.Equals(contractorId))
			{
				Assert.Fail();
			}

			if (t.Item2.Equals(contractorId2))
			{
				Assert.Fail();
			}
		}



		private string RandomTableNameName(string prefix = "uniquetests")
		{
			var rgx = new System.Text.RegularExpressions.Regex(@"[^a-z0-9]");
			var random = rgx.Replace(Guid.NewGuid().ToString("N").ToLowerInvariant(), "");
			return $"{prefix}{random}";
		}

	}

	public class TestId : Ddd.Domain.IAggregateIdentity
	{
		public Guid Id { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public string Value
		{
			get
			{
				return Id.ToString("N");
			}
		}

		public TestId(Guid guid)
		{
			Id = guid;
		}
	}


}