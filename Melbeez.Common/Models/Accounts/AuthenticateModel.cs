using System;

namespace Melbeez.Common.Models.Accounts
{
    public class AuthenticateModel
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string CurrencyCode { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public bool IsSocialLogIn { get; set; }
        public bool IsFirstLoginAttempt { get; set; }
        public DateTime VerificationRemindedOn { get; set; }
        public int VerificationReminderCount { get; set; }
    }
}
