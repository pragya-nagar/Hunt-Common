using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Synergy.Common.FileStorage.Abstraction;

namespace Synergy.Common.FileStorage.AmazonS3
{
    public class AmazonS3Storage : IFileStorage
    {
        private const string AwsCustomMetadataKeyPrefix = "x-amz-meta-";

        private const int ReadUrlLifetimeMinutes = 2;

        private const int WriteUrlLifetimeMinutes = 10;

        private readonly string _bucketName;

        private readonly IAmazonS3 _amazonS3;

        static AmazonS3Storage()
        {
            AWSConfigsS3.UseSignatureVersion4 = true;
        }

        public AmazonS3Storage(string bucketName)
            : this(bucketName, new AmazonS3Client())
        {
        }

        public AmazonS3Storage(string bucketName, IAmazonS3 amazonS3)
        {
            this._bucketName = bucketName;
            this._amazonS3 = amazonS3;
        }

        public async Task DeleteAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new DeleteObjectRequest
            {
                BucketName = this._bucketName,
                Key = fileName,
            };

            await this._amazonS3.DeleteObjectAsync(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<byte[]> GetAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var stream = await new TransferUtility(this._amazonS3)
                .OpenStreamAsync(this._bucketName, fileName, cancellationToken)
                .ConfigureAwait(false))
            {
                using (var s = new MemoryStream(32 * 1024))
                {
                    await stream.CopyToAsync(s).ConfigureAwait(false);
                    return s.ToArray();
                }
            }
        }

        public async Task<Stream> GetStreamAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var stream = await new TransferUtility(this._amazonS3)
                .OpenStreamAsync(this._bucketName, fileName, cancellationToken)
                .ConfigureAwait(false))
            {
                var s = new MemoryStream(32 * 1024);
                await stream.CopyToAsync(s).ConfigureAwait(false);
                return s;
            }
        }

        public async Task<string> GetUploadUrlAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await this.GetObjectAccessAsync(path, HttpVerb.PUT, DateTime.Now.AddMinutes(WriteUrlLifetimeMinutes), cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<ObjectAccessModel>> GetAccessAsync(string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            var keysRequest = new ListObjectsRequest
            {
                BucketName = this._bucketName,
                Prefix = path,
                MaxKeys = 10,
            };

            var list = new List<ObjectAccessModel>();

            try
            {
                ListObjectsResponse response;
                do
                {
                    response = await this._amazonS3.ListObjectsAsync(keysRequest, cancellationToken).ConfigureAwait(false);
                    foreach (var obj in response.S3Objects.Where(x => x.Size > 0))
                    {
                        var url = await this.GetObjectAccessAsync(obj.Key, HttpVerb.GET, DateTime.Now.AddMinutes(ReadUrlLifetimeMinutes), cancellationToken).ConfigureAwait(false);
                        var info = await this._amazonS3.GetObjectMetadataAsync(new GetObjectMetadataRequest { BucketName = this._bucketName, Key = obj.Key }, cancellationToken).ConfigureAwait(false);

                        var contentTypeKey = info.Metadata.Keys.FirstOrDefault(x => x.Equals("Content-Type", StringComparison.OrdinalIgnoreCase));
                        list.Add(new ObjectAccessModel
                        {
                            Path = obj.Key,
                            ContentType = string.IsNullOrWhiteSpace(contentTypeKey) ? "application/octet-stream" : info.Metadata[contentTypeKey],
                            Length = obj.Size,
                            ETag = obj.ETag,
                            ModifiedOn = obj.LastModified,
                            Url = url,
                        });
                    }

                    keysRequest.Marker = response.NextMarker;
                }
                while (response.IsTruncated);
            }
            catch (AmazonS3Exception)
            {
                throw;
            }

            return list;
        }

        public async Task SaveAsync(byte[] content, string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            await new TransferUtility(this._amazonS3)
                        .UploadAsync(new MemoryStream(content), this._bucketName, fileName, cancellationToken)
                        .ConfigureAwait(false);
        }

        public async Task<Dictionary<string, string>> GetMetadataAsync(string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var metadataResponse = await this._amazonS3.GetObjectMetadataAsync(new GetObjectMetadataRequest()
            {
                BucketName = this._bucketName,
                Key = fileName,
            }, cancellationToken).ConfigureAwait(false);

            return metadataResponse.Metadata.Keys.Where(metaKey => metaKey.StartsWith(AwsCustomMetadataKeyPrefix, StringComparison.OrdinalIgnoreCase))
                                                 .ToDictionary(metaKey => metaKey.Substring(AwsCustomMetadataKeyPrefix.Length),
                                                               metaKey => metadataResponse.Metadata[metaKey]);
        }

        public async Task SetMetadataAsync(string fileName, Dictionary<string, string> metadata, CancellationToken cancellationToken = default(CancellationToken))
        {
            var metadataResponse = await this._amazonS3.GetObjectMetadataAsync(new GetObjectMetadataRequest()
            {
                BucketName = this._bucketName,
                Key = fileName,
            }, cancellationToken).ConfigureAwait(false);

            var requst = new CopyObjectRequest
            {
                SourceBucket = this._bucketName,
                DestinationBucket = this._bucketName,

                SourceKey = fileName,
                DestinationKey = fileName,

                MetadataDirective = S3MetadataDirective.REPLACE,
            };

            var systemMetadataKeys = metadataResponse.Metadata.Keys
                .Where(metaKey => metaKey.StartsWith(AwsCustomMetadataKeyPrefix, StringComparison.OrdinalIgnoreCase) == false);

            foreach (var key in systemMetadataKeys)
            {
                requst.Metadata.Add(key, metadataResponse.Metadata[key]);
            }

            foreach (var metaPair in metadata)
            {
                requst.Metadata.Add(AwsCustomMetadataKeyPrefix + metaPair.Key, metaPair.Value);
            }

            await this._amazonS3.CopyObjectAsync(requst, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> GetObjectAccessAsync(string key, HttpVerb accessType, DateTime expires, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var protocol = Protocol.HTTPS;
            if (Uri.TryCreate(this._amazonS3.Config.ServiceURL, UriKind.RelativeOrAbsolute, out var serviceUri) == true && string.Equals(serviceUri.Scheme, "http", StringComparison.OrdinalIgnoreCase))
            {
                protocol = Protocol.HTTP;
            }

            var url = this._amazonS3.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = this._bucketName,
                Key = key,
                Verb = accessType,
                Expires = expires,
                Protocol = protocol,
            });

            return await Task.FromResult(url).ConfigureAwait(false);
        }
    }
}
