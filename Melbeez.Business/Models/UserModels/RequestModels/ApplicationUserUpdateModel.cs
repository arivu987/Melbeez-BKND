namespace Melbeez.Business.Models.UserModels.RequestModels
{
    public class ApplicationUserUpdateModel
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; }
    }
}
