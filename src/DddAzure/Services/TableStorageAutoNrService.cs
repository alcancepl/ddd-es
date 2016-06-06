using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace Ddd.Services
{
	/// <summary>
	/// Saves uniquie document numbers in Azure Table Storage. The format of the stored data is as follows:
	/// Row name     - PK           ;RK                 ;PROPERTIES
	/// ----------------------------------------------------------------------
	/// SEQUENCE ROW - "{sequence}" ;"SEQUENCE-HEAD"    ;LastNr        ; Data
	/// ID ROW       - "{sequence}" ;"ID-{aggregateId}" ;Nr            ;
	/// NR ROW       - "{sequence}" ;"NR-{sequenceNr}"  ;Id            ; Data
	/// </summary>
	public class TableStorageAutoNrService : IAutoNrService
	{
		private readonly CloudTable table;
		private const int MaxRetries = 10;

		public TableStorageAutoNrService(string storageAccountConnectionString, string serviceTableName = "autonrserivce")
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

		public async Task<TAutoNrData> GetAutoNr<TSequenceData, TAutoNrData>(string sequence, string aggregateId, AutoNrGenerator<TSequenceData, TAutoNrData> generator)
			where TSequenceData : class
			where TAutoNrData : class
		{
			return await GetAutoNrWithRetries(sequence, aggregateId, generator, MaxRetries);
		}

		public async Task<TAutoNrData> UpdateAutoNr<TSequenceData, TAutoNrData>(string sequence, string aggregateId, AutoNrUpdater<TSequenceData, TAutoNrData> updater)
			where TSequenceData : class
			where TAutoNrData : class
		{
			return await UpdateAutoNrWithRetries(sequence, aggregateId, updater, MaxRetries);
		}

		public async Task SetLastNr<TSequenceData, TNrData>(string sequence, long lastNr)
			where TSequenceData : class
			where TNrData : class
		{
			var sequenceEntity = await GetSequenceRow(sequence); // or null
			if (sequenceEntity != null)
			{
				var prevNrRow = await GetNrRow(sequence, lastNr - 1);
				if (prevNrRow != null)
					throw new Exception($"Sequence '{sequence}' already generated number and lastNr cannot be set.");
				sequenceEntity.LastNr = lastNr;
				await table.ExecuteAsync(TableOperation.Replace(sequenceEntity));
			}
			else {
				sequenceEntity = SequenceEntity.Create(sequence, lastNr, new object());
				await table.ExecuteAsync(TableOperation.Insert(sequenceEntity));
			}
		}


		private async Task<TAutoNrData> GetAutoNrWithRetries<TSequenceData, TAutoNrData>(string sequence, string aggregateId, AutoNrGenerator<TSequenceData, TAutoNrData> generator, int retries)
			where TSequenceData : class
			where TAutoNrData : class
		{
			var sequenceEntity = await GetSequenceRow(sequence); // or null
			if (sequenceEntity == null)
			{
				// No sequence row
				var newSequenceResult = generator(1L, null, null);

				var firstBatch = new TableBatchOperation();
				firstBatch.Insert(SequenceEntity.Create(sequence, 1L, newSequenceResult.SequenceData)); // OperationIndex 0
				firstBatch.Insert(NrEntity.Create(sequence, aggregateId, 1L, newSequenceResult.NrData)); // OperationIndex 1
				firstBatch.Insert(IdEntity.Create(sequence, aggregateId, 1L)); // OperationIndex 2

				try
				{
					await table.ExecuteBatchAsync(firstBatch);
				}
				catch (StorageException storageException)
				{
					int operationIndex;
					string errorCode;
					if (!TryParseStorageException(storageException, out operationIndex, out errorCode))
						throw;

					switch (operationIndex)
					{
						case 0: // Insert(SequenceEntity)                        
							if (errorCode == "EntityAlreadyExists")
							{
								if (retries > 0)
									return await GetAutoNrWithRetries(sequence, aggregateId, generator, retries - 1);
								throw new InvalidOperationException($"Optimistic lock failed because sequence row already exists. Sequence: {sequence}, nr:1.");
							}
							break;
						case 1: // Insert(NrEntity)
							if (errorCode == "EntityAlreadyExists")
							{
								throw new DuplicateDocumentNrException(sequence, 1L);
							}
							break;
						case 2: // Insert(IdEntity)
							if (errorCode == "EntityAlreadyExists")
							{
								var loadedIdRow = await GetIdRow(sequence, aggregateId);
								var loadedNrRow = await GetNrRow(sequence, loadedIdRow.Nr);
								return loadedNrRow.GetData<TAutoNrData>();
							}
							break;
						default:
							break;
					}

					throw;
				}

				return newSequenceResult.NrData;
			}

			// sequence row present
			var prevNrRow = await GetNrRow(sequence, sequenceEntity.LastNr);

			sequenceEntity.LastNr++;

			var result = generator(
				sequenceEntity.LastNr,
				sequenceEntity.GetData<TSequenceData>(),
				prevNrRow != null ? prevNrRow.GetData<TAutoNrData>() : null);

			sequenceEntity.SetData(result.SequenceData);

			var batch = new TableBatchOperation();
			batch.Add(TableOperation.Replace(sequenceEntity)); // OperationIndex 0
			batch.Add(TableOperation.Insert(NrEntity.Create(sequence, aggregateId, sequenceEntity.LastNr, result.NrData))); // OperationIndex 1
			batch.Add(TableOperation.Insert(IdEntity.Create(sequence, aggregateId, sequenceEntity.LastNr))); // OperationIndex 2
			if (prevNrRow != null)
				batch.Add(TableOperation.Merge(prevNrRow)); // OperationIndex 3

			try
			{
				await table.ExecuteBatchAsync(batch);
			}
			catch (StorageException storageException)
			{
				int operationIndex;
				string errorCode;
				if (!TryParseStorageException(storageException, out operationIndex, out errorCode))
					throw;

				switch (operationIndex)
				{
					case 0: // Replace(sequenceEntity)
					case 3: // Merge(prevNrRow)
						if (errorCode == "UpdateConditionNotSatisfied")
						{
							if (retries > 0)
								return await GetAutoNrWithRetries(sequence, aggregateId, generator, retries - 1);
							string reason = operationIndex == 0
								? "sequence row has changed"
								: "prev-nr row has changed";
							throw new AutoNrOptimisticException($"Optimistic lock failed because {reason}. Sequence: {sequence}, nr:{sequenceEntity.LastNr}.");
						}
						break;
					case 1: // Insert(NrEntity)
						if (errorCode == "EntityAlreadyExists")
						{
							throw new DuplicateDocumentNrException(sequence, sequenceEntity.LastNr);
						}
						break;
					case 2: // Insert(IdEntity)
						if (errorCode == "EntityAlreadyExists")
						{
							var loadedIdRow = await GetIdRow(sequence, aggregateId);
							var loadedNrRow = await GetNrRow(sequence, loadedIdRow.Nr);
							return loadedNrRow.GetData<TAutoNrData>();
						}
						break;
					default:
						break;
				}

				throw;
			}

			return result.NrData;
		}

		private async Task<TAutoNrData> UpdateAutoNrWithRetries<TSequenceData, TAutoNrData>(string sequence, string aggregateId, AutoNrUpdater<TSequenceData, TAutoNrData> updater, int retries)
			where TSequenceData : class
			where TAutoNrData : class
		{
			var sequenceRow = await GetSequenceRow(sequence); // or null
			if (sequenceRow == null)
				throw new Exception($"Sequence not found: {sequence}");

			var idRow = await GetIdRow(sequence, aggregateId);
			if (idRow == null)
				throw new Exception($"Agreggate with id {aggregateId} not found in sequence {sequence}");

			var nrRow = await GetNrRow(sequence, idRow.Nr);
			if (nrRow == null)
				throw new Exception($"Agreggate with id {aggregateId} not found in sequence {sequence}");

			var prevNrRow = await GetNrRow(sequence, idRow.Nr - 1);
			var nextNrRow = await GetNrRow(sequence, idRow.Nr + 1);

			var result = updater(
				nrRow.GetData<TAutoNrData>(),
				sequenceRow.GetData<TSequenceData>(),
				prevNrRow != null ? prevNrRow.GetData<TAutoNrData>() : null,
				nextNrRow != null ? nextNrRow.GetData<TAutoNrData>() : null);

			sequenceRow.SetData(result.SequenceData);
			nrRow.SetData(result.NrData);

			var batch = new TableBatchOperation();
			batch.Add(TableOperation.Replace(sequenceRow)); // OperationIndex 0
			batch.Add(TableOperation.Replace(nrRow)); // OperationIndex 1
			if (prevNrRow != null)
				batch.Add(TableOperation.Merge(prevNrRow)); // OperationIndex 2
			if (nextNrRow != null)
				batch.Add(TableOperation.Merge(nextNrRow)); // OperationIndex 2 or 3

			try
			{
				await table.ExecuteBatchAsync(batch);
			}
			catch (StorageException storageException)
			{
				int operationIndex;
				string errorCode;
				if (!TryParseStorageException(storageException, out operationIndex, out errorCode))
					throw;

				if (errorCode == "UpdateConditionNotSatisfied")
				{
					if (retries >= 0)
						return await UpdateAutoNrWithRetries(sequence, aggregateId, updater, retries - 1);
					var rowName = new string[] { "sequence", "auto-nr", "prev-auto-nr", "next-auto-nr" }[operationIndex];
					throw new AutoNrOptimisticException($"Optimistic lock failed because {rowName} has changed. Sequence: {sequence}, nr:{idRow.Nr}.");
				}

				throw;
			}

			return result.NrData;
		}

		private bool TryParseStorageException(StorageException storageException, out int operationIndex, out string errorCode)
		{
			operationIndex = -1;
			errorCode = storageException.RequestInformation.ExtendedErrorInformation.ErrorCode;
			var parts = storageException.RequestInformation.ExtendedErrorInformation.ErrorMessage.Split(new char[] { ':' }, 2);
			return parts.Length == 2 && int.TryParse(parts[0], out operationIndex);
		}

		async Task<SequenceEntity> GetSequenceRow(string sequence)
		{
			var op = TableOperation.Retrieve<SequenceEntity>(SequenceEntity.FormatPartitionKey(sequence), SequenceEntity.FormatRowKey());
			var tableResult = await table.ExecuteAsync(op);
			return tableResult.Result as SequenceEntity;
		}

		private async Task<NrEntity> GetNrRow(string sequence, long nr)
		{
			var op = TableOperation.Retrieve<NrEntity>(NrEntity.FormatPartitionKey(sequence), NrEntity.FormatRowKey(nr));
			var tableResult = await table.ExecuteAsync(op);
			return tableResult.Result as NrEntity;
		}

		private async Task<IdEntity> GetIdRow(string sequence, string aggregateId)
		{
			var op = TableOperation.Retrieve<IdEntity>(IdEntity.FormatPartitionKey(sequence), IdEntity.FormatRowKey(aggregateId));
			var tableResult = await table.ExecuteAsync(op);
			return tableResult.Result as IdEntity;
		}

	}

	[Serializable]
	public class AutoNrOptimisticException : Exception
	{
		public AutoNrOptimisticException() { }
		public AutoNrOptimisticException(string message) : base(message) { }
		public AutoNrOptimisticException(string message, Exception inner) : base(message, inner) { }
		protected AutoNrOptimisticException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}
