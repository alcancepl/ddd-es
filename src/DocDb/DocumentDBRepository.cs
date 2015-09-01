using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DocDb
{
	public static class DocumentDBRepository<T> where T : class
	{

		//Expose the "database" value from configuration as a property for internal use
		private static string databaseId;
		private static String DatabaseId
		{
			get
			{
				if (string.IsNullOrEmpty(databaseId))
				{
					databaseId = Microsoft.Azure.CloudConfigurationManager.GetSetting("documentDbDatabase");
				}

				return databaseId;
			}
		}

		//Expose the "collection" value from configuration as a property for internal use
		private static string collectionId;
		private static String CollectionId
		{
			get
			{
				if (string.IsNullOrEmpty(collectionId))
				{
					collectionId = Microsoft.Azure.CloudConfigurationManager.GetSetting("documentDbCollection");
				}

				return collectionId;
			}
		}

		//Use the ReadOrCreateDatabase function to get a reference to the database.
		private static Database database;
		private static Database Database
		{
			get
			{
				if (database == null)
				{
					database = Client.CreateDatabaseQuery()
							.Where(d => d.Id == DatabaseId)
							.AsEnumerable()
							.FirstOrDefault();
				}

				return database;
			}
		}

		//Use the ReadOrCreateCollection function to get a reference to the collection.
		private static DocumentCollection collection;
		private static DocumentCollection Collection
		{
			get
			{
				if (collection == null)
				{

					collection = Client.CreateDocumentCollectionQuery(Database.SelfLink)
										  .Where(c => c.Id == CollectionId)
										  .AsEnumerable()
										  .FirstOrDefault(); ;

				}

				return collection;
			}
		}

		//This property establishes a new connection to DocumentDB the first time it is used, 
		//and then reuses this instance for the duration of the application avoiding the
		//overhead of instantiating a new instance of DocumentClient with each request
		private static DocumentClient client;
		private static DocumentClient Client
		{
			get
			{
				if (client == null)
				{
					string endpoint = Microsoft.Azure.CloudConfigurationManager.GetSetting("documentDbEndpoint");
					string authKey = Microsoft.Azure.CloudConfigurationManager.GetSetting("documentDbAuthKey");
					Uri endpointUri = new Uri(endpoint);
					client = new DocumentClient(endpointUri, authKey);
				}

				return client;
			}
		}


		public static async Task<Document> CreateItemAsync(DocumentWrap<T> item)
		{
			var disableAutomaticIdGeneration = true;
			return await Client.CreateDocumentAsync(Collection.SelfLink, item, null, disableAutomaticIdGeneration);
		}

		public static async Task<DocumentWrap<T>> GetWrapedItem(Expression<Func<DocumentWrap<T>, bool>> predicate)
		{
			var query = Client.CreateDocumentQuery<DocumentWrap<T>>(Collection.DocumentsLink)
						.Where(predicate)
						.AsDocumentQuery();
			var feed = await query.ExecuteNextAsync();
			return feed.AsEnumerable().Single();
		}
		public static async Task<T> GetItem(Expression<Func<T, bool>> predicate)
		{
			var query = Client.CreateDocumentQuery<DocumentWrap<T>>(Collection.DocumentsLink)
				.Where(d => d.Id.StartsWith(String.Format("{0}|", typeof(T).Name)))
				.Select(d => d.Document)
				.Where(predicate)
				.AsDocumentQuery();

			var feed = await query.ExecuteNextAsync();
			return feed.AsEnumerable().Single();
		}

		public static async Task<IEnumerable<DocumentWrap<T>>> GetWrapedItems(Expression<Func<DocumentWrap<T>, bool>> predicate)
		{
			var query = Client.CreateDocumentQuery<DocumentWrap<T>>(Collection.DocumentsLink)
				.Where(predicate)
				.AsDocumentQuery();

			var feed = await query.ExecuteNextAsync<DocumentWrap<T>>();
			return feed.AsEnumerable();
		}

		public static async Task<IEnumerable<T>> GetItems(Expression<Func<T, bool>> predicate)
		{
			var query = Client.CreateDocumentQuery<DocumentWrap<T>>(Collection.DocumentsLink)
				.Where(d => d.Id.StartsWith(String.Format("{0}|", typeof(T).Name)))
				.Select(d => d.Document)
				.Where(predicate)
				.AsDocumentQuery();

			var feed = await query.ExecuteNextAsync<DocumentWrap<T>>();
			return feed.Select(f => f.Document).AsEnumerable();
		}

		public static async Task<T> GetById(string id)
		{
			var res = await TryGetById(id);
			if (res == null) {
				throw new KeyNotFoundException(String.Format("Document not found with id: {0}", DocumentWrapHelper.ConcateIds(typeof(T).Name, id)));
			}
			return res;
		}

		public static async Task<T> TryGetById(string id)
		{
			var formatedId = String.Format("{0}|{1}", typeof(T).Name, id);
			var document = await GetDocument(formatedId);
			if (document == null)
			{
				return default(T);
			}

			return document.GetPropertyValue<T>("Document");
		}


		public static async Task<Document> UpdateItemAsync(DocumentWrap<T> item)
		{
			item.SetPropertyValue("Document", item.Document);
			Uri docUri = UriFactory.CreateDocumentUri(DatabaseId, CollectionId, item.Id);
			return await Client.ReplaceDocumentAsync(docUri, item);
		}

		/// <summary>
		/// Delete document by it's id. 
		/// </summary>
		/// <param name="id">ID of the document without classname</param>
		/// <returns></returns>
		public static async Task DeleteItemById(string id) {
			await DeleteItemByDocumentId(DocumentWrapHelper.ConcateIds(typeof(T).Name, id));
		}

		private static async Task DeleteItemByDocumentId(string fullIdWithClassName)
		{
			Uri docUri = UriFactory.CreateDocumentUri(DatabaseId, CollectionId, fullIdWithClassName);
			await client.DeleteDocumentAsync(docUri);
		}

		public static async Task DeleteItem(DocumentWrap<T> document)
		{
			await DeleteItemByDocumentId(document.Id);
		}

		private static async Task<Document> GetDocument(string id)
		{
			var query = Client.CreateDocumentQuery(Collection.DocumentsLink)
				.Where(d => d.Id == id)
				.AsDocumentQuery();

			var feed = await query.ExecuteNextAsync();
			return feed.AsEnumerable().SingleOrDefault();
		}
	}
}
