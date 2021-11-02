using System;

namespace Synergy.Common.FileStorage.Abstraction
{
    public class ObjectAccessModel
    {
        public string Path { get; set; }

        public string ContentType { get; set; }

        public long Length { get; set; }

        public string ETag { get; set; }

        public DateTime ModifiedOn { get; set; }

        public string Url { get; set; }
    }
}
