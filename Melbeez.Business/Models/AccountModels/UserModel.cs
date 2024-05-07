namespace Melbeez.Business.Models.AccountModels
{
    public class UserModel
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; }
        public string Password { get; set; } = null!;
        public string ConfirmPassword { get; set; } = null!;
    }
    public class UserViewModel
    {
        public long Id { get; set; }
        public string UserId { get; set; }
        public string AccountId { get; set; }
    }
}
