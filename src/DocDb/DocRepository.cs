using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocDb
{
	public abstract class DocRepositorySpace : IDocRepositorySpace, IDisposable
	{
		private DocumentClient client;
		private Database database;
		private DocumentCollection collection;
		private readonly string databaseId;
		private readonly string collectionId;

		public DocRepositorySpace(string endpoint, string authKey, string databaseId, string collectionId)
		{
			client = new DocumentClient(new Uri(endpoint), authKey);
			this.databaseId = databaseId;
			this.collectionId = collectionId;
		}

		protected DocRepositorySpace()
		{
			client = null;
			database = null;
			collection = null;
		}

		public void Init()
		{
			var task = InitAsync();
			task.Wait();
		}
		private async Task InitAsync()
		{
			database = client.CreateDatabaseQuery()
				.Where(d => d.Id == databaseId)
				.AsEnumerable()
				.SingleOrDefault();

			if (database == null)
			{
				database = await client.CreateDatabaseAsync(new Database() { Id = databaseId });
			}

			collection = client.CreateDocumentCollectionQuery(database.SelfLink)
				.Where(c => c.Id == collectionId)
				.AsEnumerable()
				.SingleOrDefault();

			if (collection == null)
			{
				var newCollection = new DocumentCollection() { Id = collectionId };
				var indexingPolicy = newCollection.IndexingPolicy;
				indexingPolicy.IndexingMode = IndexingMode.Consistent;
				indexingPolicy.Automatic = true;
				indexingPolicy.IncludedPaths.Add(
					new IncludedPath
					{
						Path = "/*",
						Indexes = new Collection<Index> {
							new RangeIndex(DataType.Number) { Precision = -1 },
							new RangeIndex(DataType.String) { Precision = -1 },
							new SpatialIndex(DataType.Point)
						}
					});

				collection = await client.CreateDocumentCollectionAsync(database.SelfLink, newCollection);
			}
		}

		public DocumentClient Client => client;
		public Database Database => database;
		public DocumentCollection Collection => collection;
		public JsonSerializerSettings JsonSettings { get; } = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };

		public void Dispose()
		{
			if (client != null)
			{
				client.Dispose();
				client = null;
			}
		}
	}

	public class DocRepository<T, S> : IDocRepository<T, S>
		where T : class
		where S : IDocRepositorySpace
	{
		private readonly DocumentClient client;
		private readonly DocumentCollection collection;
		private readonly Database database;
		private readonly JsonSerializerSettings jsonSettings;

		public DocRepository(S space)
		{
			client = space.Client;
			database = space.Database;
			collection = space.Collection;
			jsonSettings = space.JsonSettings;
		}

		public async Task<DocumentWrap<T>> AddAsync(DocumentWrap<T> item)
		{
			var newDocument = new Document();
			var serializer = JsonSerializer.Create(jsonSettings);
			using (var ms = new MemoryStream())
			{
				using (var writer = new StreamWriter(ms, Encoding.UTF8, 4096, true))
				{
					serializer.Serialize(writer, item);
				}
				ms.Position = 0;
				using (var reader = new StreamReader(ms))
				{
					var jsonReader = new JsonTextReader(reader);
					newDocument.LoadFrom(jsonReader);
				}
			}
			var response = await client.CreateDocumentAsync(collection.SelfLink, newDocument, null, true);
			return item;
		}

		public async Task<DocumentWrap<T>> DeleteAsync(string id)
		{
			var docUri = UriFactory.CreateDocumentUri(database.Id, collection.Id, id);
			var response = await client.DeleteDocumentAsync(docUri);
			return null;
		}

		private T DocumentToItem(Document document)
		{
			var serializer = JsonSerializer.Create(jsonSettings);
			using (var ms = new MemoryStream())
			{
				using (var writer = new StreamWriter(ms, Encoding.UTF8, 4096, true))
				{
					serializer.Serialize(writer, document);
				}
				ms.Position = 0;
				using (var reader = new StreamReader(ms))
				using (var jsonReader = new JsonTextReader(reader))
				{
					while (jsonReader.Read())
					{
						if (jsonReader.TokenType == JsonToken.PropertyName && (string)jsonReader.Value == "document")
						{
							jsonReader.Read();
							return serializer.Deserialize<T>(jsonReader);
						}
					}
				}
			}
			return default(T);
		}
		private T DocumentWrapJsonToItem(string document)
		{
			var serializer = JsonSerializer.Create(jsonSettings);

			using (var reader = new StringReader(document))
			using (var jsonReader = new JsonTextReader(reader))
			{
				while (jsonReader.Read())
				{
					if (jsonReader.TokenType == JsonToken.PropertyName && (string)jsonReader.Value == "document")
					{
						jsonReader.Read();
						return serializer.Deserialize<T>(jsonReader);
					}
				}
			}
			return default(T);
		}

		public async Task<Result<T>> GetAsync(string id)
		{
			var documentLink = UriFactory.CreateDocumentUri(database.Id, collection.Id, id);
			try
			{
				var result = await client.ReadDocumentAsync(documentLink);
				return new Result<T>(DocumentToItem(result.Resource));
			}
			catch (DocumentClientException ex)
			{
				if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
					return new Result<T>(default(T));
				throw ex;
			}
		}

		public async Task<IList<T>> GetAsync(Func<IQueryable<DocumentWrap<T>>, IQueryable<T>> queryBuilder, int? maxItemCount = null)
		{
			var query = client.CreateDocumentQuery<DocumentWrap<T>>(collection.DocumentsLink, new FeedOptions() { MaxItemCount = maxItemCount });
			var idStartsWithTypeInfo = DocumentWrap<T>.TypeInfo + "|";
			var finalQuery = queryBuilder(query.Where(d => d.Id.StartsWith(idStartsWithTypeInfo)));

			var docQuery = finalQuery.AsDocumentQuery();

			var type = finalQuery.Expression.Type.GetGenericArguments().First();

			var result = new List<T>();
			var serializer = JsonSerializer.Create(jsonSettings);
			while (docQuery.HasMoreResults)
			{
				result.Capacity += 1000;
				var feed = await docQuery.ExecuteNextAsync();
				foreach (var item in feed)
				{
					result.Add(serializer.Deserialize<T>(item.ToString()));
				}
			}

			return result;
		}

		public async Task<IList<T>> GetAsync(Func<IQueryable<DocumentWrap<T>>, IQueryable<DocumentWrap<T>>> queryBuilder, int? maxItemCount = null)
		{
			var query = client.CreateDocumentQuery<DocumentWrap<T>>(collection.DocumentsLink, new FeedOptions() { MaxItemCount = maxItemCount });
			var idStartsWithTypeInfo = DocumentWrap<T>.TypeInfo + "|";
			var finalQuery = queryBuilder(query.Where(d => d.Id.StartsWith(idStartsWithTypeInfo)));

			var docQuery = finalQuery.AsDocumentQuery();

			var type = finalQuery.Expression.Type.GetGenericArguments().First();

			var result = new List<T>();
			while (docQuery.HasMoreResults)
			{
				result.Capacity += 1000;
				var feed = await docQuery.ExecuteNextAsync();
				foreach (var item in feed)
				{
					result.Add(DocumentWrapJsonToItem(item.ToString()));

					//var itemType = item.GetType();

					//if (itemType == typeof(JValue))
					//    result.Add(item);
					//else
					//    result.Add((T)JsonConvert.DeserializeObject(item.ToString(), type, jsonSettings));
				}
			}

			return result;
		}


		public async Task<DocumentWrap<T>> UpdateAsync(DocumentWrap<T> item)
		{
			//item.SetPropertyValue("Document", item.Document);
			Uri docUri = UriFactory.CreateDocumentUri(database.Id, collection.Id, item.Id);

			var replaceDocument = new Document();
			var serializer = JsonSerializer.Create(jsonSettings);
			using (var ms = new MemoryStream())
			{
				using (var writer = new StreamWriter(ms, Encoding.UTF8, 4096, true))
				{
					serializer.Serialize(writer, item);
				}
				ms.Position = 0;
				using (var reader = new StreamReader(ms))
				{
					var jsonReader = new JsonTextReader(reader);
					replaceDocument.LoadFrom(jsonReader);
				}
			}
			replaceDocument.SetPropertyValue("_self", docUri.OriginalString);
			var response = await client.ReplaceDocumentAsync(replaceDocument, null);
			return item;
		}

		public async Task<DocumentWrap<T>> UpsertAsync(DocumentWrap<T> item)
		{
			//item.SetPropertyValue("Document", item.Document);
			Uri docUri = UriFactory.CreateDocumentCollectionUri(database.Id, collection.Id);

			var replaceDocument = new Document();
			var serializer = JsonSerializer.Create(jsonSettings);
			using (var ms = new MemoryStream())
			{
				using (var writer = new StreamWriter(ms, Encoding.UTF8, 4096, true))
				{
					serializer.Serialize(writer, item);
				}
				ms.Position = 0;
				using (var reader = new StreamReader(ms))
				{
					var jsonReader = new JsonTextReader(reader);
					replaceDocument.LoadFrom(jsonReader);
				}
			}
			//replaceDocument.SetPropertyValue("_self", item.SelfLink);
			var response = await client.UpsertDocumentAsync(docUri, replaceDocument, null, true);
			//return response.Resource.GetPropertyValue<DocumentWrap<T>>("document"); // (DocumentWrap<T>)response.Resource;
			return item;
		}
	}
}
