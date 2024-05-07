using Melbeez.Domain.Common.BaseEntity;
using System.ComponentModel.DataAnnotations;

namespace Melbeez.Domain.Entities
{
	public class UserNotificationPreferenceEntity : AuditableEntity<long> 
	{
		[Required]
        [MaxLength(256)]
		public string UserId { get; set; } = null!;
		[Required]
		public bool IsWarrantyExpireAlert { get; set; }
		[Required]
		public bool IsLocationUpdateAlert { get; set; }
		[Required]
		public bool IsProductUpdateAlert { get; set; }
		[Required]
		public bool IsDeviceActivationAlert { get; set; }
		[Required]
		public bool IsMarketingValueAlert { get; set; }
		[Required]
		public bool IsPushNotification { get; set; }
		[Required]
		public bool IsEmailNotification { get; set; }
		[Required]
		public bool IsTextNotification { get; set; }
		[Required]
		public bool IsThirdPartyServiceAllowed { get; set; }
		[Required]
		public bool IsBiometricAllowed { get; set; }
	}
}