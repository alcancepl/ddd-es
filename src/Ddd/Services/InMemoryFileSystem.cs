using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace Ddd.Services
{
    public class InMemoryFileSystem : IFileSystem
    {
        public static ConcurrentDictionary<string, ConcurrentDictionary<string, InMemoryFileInfo>> files = new ConcurrentDictionary<string, ConcurrentDictionary<string, InMemoryFileInfo>>();


        public Task UploadFile(string containerName, string fileName, byte[] fileContent)
        {
            var container = files[containerName];

            InMemoryFileInfo fileInfo;
            if (!container.TryGetValue(fileName, out fileInfo))
            {
                fileInfo = new InMemoryFileInfo();
                container.TryAdd(fileName, fileInfo);
            }
            fileInfo.Content.Write(fileContent, 0, fileContent.Length);
            return Task.FromResult(true);
        }

        public Task UploadFileChunk(string containerName, FileChunk fileChunk)
        {
            var container = files[containerName];

            InMemoryFileInfo fileInfo;
            if (!container.TryGetValue(fileChunk.FullFileName, out fileInfo))
            {
                fileInfo = new InMemoryFileInfo();
                container.TryAdd(fileChunk.FullFileName, fileInfo);
            }
            fileInfo.Content.Write(fileChunk.Chunk, 0, fileChunk.Chunk.Length);
            return Task.FromResult(true);
        }

        public Task<uint> GetFileSize(string containerName, string fileName)
        {
            var container = files[containerName];

            InMemoryFileInfo fileInfo;
            if (!container.TryGetValue(fileName, out fileInfo))
            {
                return Task.FromResult((uint)0);
            }
            return Task.FromResult((uint)fileInfo.Length);
        }

        public Task<bool> EnsureContainer(string containerName)
        {
            return Task.FromResult(files.TryAdd(containerName, new ConcurrentDictionary<string, InMemoryFileInfo>()));
        }

        public Task DeleteFile(string containerName, string fileName)
        {
            return Task.Run(() =>
            {
                var container = files[containerName];
                InMemoryFileInfo val;
                container.TryRemove(fileName, out val);
            });
        }

        public Task<bool> GetFile(string containerName, string fileName, Stream target)
        {
            var container = files[containerName];

            InMemoryFileInfo fileInfo;
            if (container.TryGetValue(fileName, out fileInfo))
            {
                fileInfo.Content.WriteTo(target);
            }
            else {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }

    public class InMemoryFileInfo
    {
        public InMemoryFileInfo()
        {
            Content = new MemoryStream();
        }
        public DateTime LastModified { get; set; }
        public MemoryStream Content { get; set; }
        public long Length
        {
            get
            {
                return Content.GetBuffer().Length;
            }
        }
    }
}
