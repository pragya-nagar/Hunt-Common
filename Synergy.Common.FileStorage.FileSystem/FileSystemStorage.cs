using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Synergy.Common.FileStorage.Abstraction;

namespace Synergy.Common.FileStorage.FileSystem
{
    public class FileSystemStorage : IFileStorage
    {
        private readonly string _folderPath;

        public FileSystemStorage(string folderPath)
        {
            this._folderPath = folderPath;
        }

        public async Task DeleteAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            await Task.Run(() => File.Delete(Path.Combine(this._folderPath, fileName)), cancellationToken).ConfigureAwait(false);
        }

        public async Task<byte[]> GetAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var stream = new FileStream(Path.Combine(this._folderPath, fileName), FileMode.Open))
            {
                var content = new byte[stream.Length];
                await stream.ReadAsync(content, 0, (int)stream.Length, cancellationToken).ConfigureAwait(false);

                return content;
            }
        }

        public Task<Stream> GetStreamAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(new FileStream(Path.Combine(this._folderPath, fileName), FileMode.Open) as Stream);
        }

        public async Task<IEnumerable<ObjectAccessModel>> GetAccessAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
                var list = from fileName in Directory.GetFiles(this._folderPath, "*.*", SearchOption.AllDirectories)
                           let info = new FileInfo(fileName)
                           select new ObjectAccessModel
                           {
                               Path = info.FullName,
                               Length = info.Length,
                               ContentType = fileExtensionContentTypeProvider.TryGetContentType(fileName, out var contentType) ? contentType : "application/octet-stream",
                               ETag = $"{info.LastWriteTimeUtc:O}",
                               ModifiedOn = info.LastWriteTimeUtc,
                               Url = new Uri(fileName).AbsoluteUri,
                           };

                return await Task.FromResult(list).ConfigureAwait(false);
            }
            catch (DirectoryNotFoundException)
            {
                return await Task.FromResult(Enumerable.Empty<ObjectAccessModel>()).ConfigureAwait(false);
            }
        }

        public Task<string> GetUploadUrlAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fullPath = Path.Combine(this._folderPath, path);

            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            return Task.FromResult(new Uri(fullPath).AbsoluteUri);
        }

        public async Task SaveAsync(byte[] content, string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var fullPath = Path.Combine(this._folderPath, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            using (var stream = new FileStream(fullPath, FileMode.OpenOrCreate))
            {
                await stream.WriteAsync(content, 0, content.Length, cancellationToken).ConfigureAwait(false);
            }
        }

        public Task<Dictionary<string, string>> GetMetadataAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var metadataFilePath = Path.Combine(this._folderPath, fileName + ".meta");

            Directory.CreateDirectory(Path.GetDirectoryName(metadataFilePath));

            if (File.Exists(metadataFilePath) == false)
            {
                return Task.FromResult(new Dictionary<string, string>());
            }

            var result = new Dictionary<string, string>();

            var metadataContentLines = File.ReadAllLines(metadataFilePath);

            foreach (var metadataLine in metadataContentLines)
            {
                var delimiterIndex = metadataLine.IndexOf("=", StringComparison.Ordinal);

                if (delimiterIndex < 0)
                {
                    continue;
                }

                var key = metadataLine.Substring(0, delimiterIndex);
                var value = metadataLine.Substring(delimiterIndex + 1);

                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                result.Add(Uri.UnescapeDataString(key), value);
            }

            return Task.FromResult(result);
        }

        public Task SetMetadataAsync(string fileName, Dictionary<string, string> metadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            var metadataFilePath = Path.Combine(this._folderPath, fileName + ".meta");

            Directory.CreateDirectory(Path.GetDirectoryName(metadataFilePath));

            var metadataBuilder = new StringBuilder();

            foreach (var metaPair in metadata)
            {
                var key = Uri.EscapeDataString(metaPair.Key.ToLowerInvariant());
                var value = metaPair.Value;
                metadataBuilder.AppendLine($"{key}={value}");
            }

            File.WriteAllText(metadataFilePath, metadataBuilder.ToString());

            return Task.CompletedTask;
        }
    }
}
