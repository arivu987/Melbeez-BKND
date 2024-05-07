using Melbeez.Domain.Entities.Identity;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class CreateUserResponseModel
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public ApplicationUser User { get; set; }
    }
}
