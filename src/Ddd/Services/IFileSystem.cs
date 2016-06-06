using System.IO;
using System.Threading.Tasks;

namespace Ddd.Services
{
    public interface IFileSystem
    {        
        Task<bool> EnsureContainer(string containerName);        
        Task UploadFile(string containerName, string fileName, byte[] fileContent);        
        Task UploadFileChunk(string containerName, FileChunk fileChunk);        
        Task<uint> GetFileSize(string containerName, string fileName);        
        Task DeleteFile(string containerName, string fileName);        
        Task<bool> GetFile(string containerName, string fileName, Stream target);
    }

    public class FileChunk
    {
        public string FileName { get; set; }
        public string FullFileName { get; set; }
        public int ChunkNumber { get; set; }
        public int TotalChunksOfFile { get; set; }
        public byte[] Chunk { get; set; }

        public bool IsLastChunk
        {
            get
            {
                return ChunkNumber == TotalChunksOfFile - 1;
            }
        }
    }
}
