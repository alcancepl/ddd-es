using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Ddd.Services
{
    public class BlobStorageFileSystem : IFileSystem
    {
        private readonly CloudBlobClient blobClient;

        public BlobStorageFileSystem(string storageAccountConnectionString)
        {
            var account = CloudStorageAccount.Parse(storageAccountConnectionString);
            blobClient = account.CreateCloudBlobClient();
        }

        public async Task UploadFile(string containerName, string fileName, byte[] fileContent)
        {
            var container = blobClient.GetContainerReference(containerName);
            var blob = container.GetAppendBlobReference(fileName);
            await blob.CreateOrReplaceAsync();
            await blob.AppendFromByteArrayAsync(fileContent, 0, fileContent.Length);
        }

        public async Task UploadFileChunk(string containerName, FileChunk fileChunk)
        {
            var container = blobClient.GetContainerReference(containerName);
            var blob = container.GetAppendBlobReference(fileChunk.FullFileName);
            if (fileChunk.ChunkNumber == 0)
            {
                //replace blob if exists
                await blob.CreateOrReplaceAsync();
            }
            await blob.AppendFromByteArrayAsync(fileChunk.Chunk, 0, fileChunk.Chunk.Length);
        }

        public async Task<uint> GetFileSize(string containerName, string fileName)
        {
            var container = blobClient.GetContainerReference(containerName);
            var blob = container.GetAppendBlobReference(fileName);
            await blob.FetchAttributesAsync();
            return (uint)blob.Properties.Length;
        }

        public async Task<bool> EnsureContainer(string containerName)
        {
            var container = blobClient.GetContainerReference(containerName);
            return await container.CreateIfNotExistsAsync();
        }

        public async Task DeleteFile(string containerName, string fileName)
        {
            var container = blobClient.GetContainerReference(containerName);
            var blob = container.GetBlobReference(fileName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task<bool> GetFile(string containerName, string fileName, Stream target)
        {
            var container = blobClient.GetContainerReference(containerName);
            var blob = container.GetBlobReference(fileName);
            try
            {
                await blob.DownloadToStreamAsync(target);
            }
            catch (StorageException ex)
            {
                if ((HttpStatusCode)ex.RequestInformation.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    return false;  // exit the calling function
                }

                throw;
            }
            return true;
        }

    }
}
