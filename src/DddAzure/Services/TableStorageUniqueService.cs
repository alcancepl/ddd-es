using Ddd.Domain;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace Ddd.Services
{
	public class TableStorageUniqueService : IUniqueService
	{

		private readonly CloudTable table;

		public TableStorageUniqueService(string storageAccountConnectionString, string serviceTableName = "uniqueservice")
		{
			if (string.IsNullOrEmpty(storageAccountConnectionString))
				throw new ArgumentNullException("storageAccountConnectionString");

			var account = CloudStorageAccount.Parse(storageAccountConnectionString);
			var tableClient = account.CreateCloudTableClient();
			table = tableClient.GetTableReference(serviceTableName);
		}

		public void Init()
		{
			table.CreateIfNotExists();
		}

		public async Task<TAggregateIdentity> GetOrAddUniqueValueKey<TAggregateIdentity>(string valueGroup, string value, TAggregateIdentity requestedKey, bool ignoreCase = true)
			where TAggregateIdentity : class, IAggregateIdentity
		{
			TableOperation operation = TableOperation.Insert(new UniqueEntry(valueGroup, value, requestedKey));
			try
			{
				await table.ExecuteAsync(operation);
				return requestedKey;
			}
			catch (StorageException ex)
			{
				//table service error codes https://msdn.microsoft.com/en-us/library/azure/dd179438.aspx
				if (ex.RequestInformation == null || ex.RequestInformation.ExtendedErrorInformation == null || ex.RequestInformation.ExtendedErrorInformation.ErrorCode != "EntityAlreadyExists")
				{
					throw ex;
				}
				var result = await TryGetUniqueValueKey<TAggregateIdentity>(valueGroup, value);

				if (!result.HasValue)
				{
					throw new InvalidOperationException(String.Format("Unique service could not insert nor find unique combination: {0}|{1}", valueGroup, value));
				}

				return result.Value;
			}

		}

		public async Task<TryGetUniqueValueKeyResult<TAggregateIdentity>> TryGetUniqueValueKey<TAggregateIdentity>(string valueGroup, string value, bool ignoreCase = true)
			where TAggregateIdentity : class, IAggregateIdentity
		{
			var response = await table.ExecuteAsync(TableOperation.Retrieve<UniqueEntry>(valueGroup, value));
			var entry = response.Result as UniqueEntry;
			if (entry == null)
				return TryGetUniqueValueKeyResult<TAggregateIdentity>.Empty;
			return TryGetUniqueValueKeyResult<TAggregateIdentity>.WithResult(Newtonsoft.Json.JsonConvert.DeserializeObject<TAggregateIdentity>(entry.Id));
		}

		public Task<bool> TryRemoveUniqueValueKey(string valueGroup, string value, IAggregateIdentity key, bool ignoreCase = true)
		{
			throw new NotImplementedException();
		}

		public async Task<TAggregateIdentity> UpdateUniqueValueKey<TAggregateIdentity>(string valueGroup, string fromValue, string toValue, TAggregateIdentity key, int retries, bool ignoreCase = true) where TAggregateIdentity : class, IAggregateIdentity
		{
			var oldEntryTask = table.ExecuteAsync(TableOperation.Retrieve<UniqueEntry>(valueGroup, fromValue));
			var newEntryResponse = await table.ExecuteAsync(TableOperation.Retrieve<UniqueEntry>(valueGroup, toValue));
			var oldResponse = await oldEntryTask;
			var oldEntry = oldResponse.Result as UniqueEntry;
			var newEntry = newEntryResponse.Result as UniqueEntry;
			var batchOperation = new TableBatchOperation();
			if (oldEntry != null)
			{
				var oldEntryId = Newtonsoft.Json.JsonConvert.DeserializeObject<TAggregateIdentity>(oldEntry.Id);
				if (oldEntryId.Value == key.Value)
				{
					//var deleteOperation = TableOperation.Delete(oldEntry);
					batchOperation.Delete(oldEntry);
				}
			}
			if (newEntry == null)
			{
				batchOperation.Insert(new UniqueEntry(valueGroup, toValue, key));
			}
			else
			{
				var newEntryId = Newtonsoft.Json.JsonConvert.DeserializeObject<TAggregateIdentity>(newEntry.Id);
				if (newEntryId.Value != key.Value)
				{
					throw new UniqueEntityWithTheSameIdException();
				}
			}
			try
			{
				await table.ExecuteBatchAsync(batchOperation);
				return key;
			}
			catch (StorageException storageException)
			{
				int operationIndex;
				string errorCode;
				if (!TryParseStorageException(storageException, out operationIndex, out errorCode))
					throw;
				switch (operationIndex)
				{
					case 0: // Remove
						if (retries > 0)
							return await UpdateUniqueValueKey(valueGroup, fromValue, toValue, key, retries - 1, ignoreCase);
						throw new InvalidOperationException($"Could not remove entity!");
					case 1: // Insert
						if (errorCode == "EntityAlreadyExists")
						{
							if (retries > 0)
								return await UpdateUniqueValueKey(valueGroup, fromValue, toValue, key, retries - 1, ignoreCase);
							throw new InvalidOperationException($"Entity already exists!");
						}
						break;
					default:
						break;
				}
				throw;
			}
		}

		private bool TryParseStorageException(StorageException storageException, out int operationIndex, out string errorCode)
		{
			operationIndex = -1;
			errorCode = storageException.RequestInformation.ExtendedErrorInformation.ErrorCode;
			var parts = storageException.RequestInformation.ExtendedErrorInformation.ErrorMessage.Split(new char[] { ':' }, 2);
			return parts.Length == 2 && int.TryParse(parts[0], out operationIndex);
		}

	}

	public class UniqueEntry : TableEntity
	{
		public UniqueEntry(string valueGroup, string value, object id)
		{
			this.PartitionKey = valueGroup;
			this.RowKey = value;
			this.Id = Newtonsoft.Json.JsonConvert.SerializeObject(id);
		}

		public UniqueEntry() { }

		public string Id { get; set; }
	}
}
