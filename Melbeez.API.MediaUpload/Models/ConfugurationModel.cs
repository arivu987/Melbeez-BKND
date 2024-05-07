
using System.Collections.Generic;

namespace Melbeez.MediaUpload.Models
{
    public class ConfugurationModel
    {
        public string _AccessKey { get; set; }
        public string _SecretKey { get; set; }
        public string _BucketName { get; set; }
        public string _UploadTo { get; set; }
        public List<string> _FileExtentions { get; set; }
    }
}
