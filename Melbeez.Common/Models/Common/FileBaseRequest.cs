using System.Collections.Generic;

namespace Melbeez.Common.Models.Common
{
    public class FileBaseRequest
    {
        public long Id { get; set; }
        public string FileUrl { get; set; }
        public int FileSize { get; set; }
        public bool IsDefault { get; set; }
    }
}
