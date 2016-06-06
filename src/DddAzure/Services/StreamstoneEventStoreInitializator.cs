using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ddd.Services
{
    public class StreamstoneEventStoreInitializator
    {

        private readonly CloudTableClient tableClient;

        public StreamstoneEventStoreInitializator(string storageAccountConnectionString)
        {
            if (string.IsNullOrWhiteSpace(storageAccountConnectionString))
                throw new ArgumentNullException("storageAccountConnectionString");

            var account = CloudStorageAccount.Parse(storageAccountConnectionString);
            tableClient = account.CreateCloudTableClient();
        }

        public void Init(IEnumerable<string> tableNames)
        {
            foreach (var tableName in tableNames)
            {
                var tableRef = tableClient.GetTableReference(tableName);
                var tableCreated = tableRef.CreateIfNotExists();
                if (tableCreated)
                {
                    Trace.TraceInformation($"StreamstoneEventStoreInitializator created table {tableName} at {tableClient.BaseUri.PathAndQuery}. ");
                }
            }
        }
    }
}
