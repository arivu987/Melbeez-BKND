namespace Melbeez.MediaUpload.Models
{
    public class FileUploadResponse
    {
        public bool IsSuccess { get; set; }
        public string FileUrl { get; set; } = null!;
        public int FileSize { get; set; }
        public string? Message { get; set; }
        public FileUploadResponse()
        {
            IsSuccess = false;
            FileSize = 0;
        }
    }
}
