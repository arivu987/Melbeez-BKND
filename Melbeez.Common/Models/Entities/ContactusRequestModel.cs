namespace Melbeez.Common.Models.Entities
{
    public class ContactusRequestModel
    {
        public long Id { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string Image { get; set; }
        public string AttachImageName { get; set; }
    }
}
