using System;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class OTPOnEmailReportModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string OTP { get; set; }
        public DateTime TimeStamp { get; set; }
    }
    public class OTPOnSMSReportModel
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string OTP { get; set; }
        public DateTime TimeStamp { get; set; }
    }
    public class NewUsersReportModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsEmailConfimed { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsPhoneNumberConfimed { get; set; }
        public DateTime RegisteredDate { get; set; }
    }
    public class ActiveUsersReportModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Month { get; set; }
        public string Week { get; set; }
        public int Year { get; set; }
        public DateTime ActiveDate { get; set; }
    }
    public class BarCodeAPIReportModel
    {
        public string BarCode { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public string ScanBy { get; set; }
        public DateTime ScanOn { get; set; }
    }
}
