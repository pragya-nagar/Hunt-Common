using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Synergy.Common.FileStorage.Abstraction
{
    public interface IFileStorage
    {
        Task<byte[]> GetAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));

        Task<Stream> GetStreamAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));

        Task<IEnumerable<ObjectAccessModel>> GetAccessAsync(string path, CancellationToken cancellationToken = default(CancellationToken));

        Task<string> GetUploadUrlAsync(string path, CancellationToken cancellationToken = default(CancellationToken));

        Task SaveAsync(byte[] content, string fileName, CancellationToken cancellationToken = default(CancellationToken));

        Task DeleteAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));

        Task<Dictionary<string, string>> GetMetadataAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken));

        Task SetMetadataAsync(string fileName, Dictionary<string, string> metadata, CancellationToken cancellationToken = default(CancellationToken));
    }
}
