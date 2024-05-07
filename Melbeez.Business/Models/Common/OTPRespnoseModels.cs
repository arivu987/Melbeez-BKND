namespace Melbeez.Business.Models.Common
{
    public class OTPRespnoseModel
    {
        public bool IsSuccess { get; set; }
    }
    public class ResetPasswordOTPRespnoseModel : OTPRespnoseModel
    {
        public string ResetPasswordToken { get; set; }
    }
}
