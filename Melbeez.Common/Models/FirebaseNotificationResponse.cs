using System.Collections.Generic;

namespace Melbeez.Common.Models
{
    public class FirebaseNotificationResponse
    {
        public long multicast_id { get; set; }
        public int success { get; set; }
        public int failure { get; set; }
        public int canonical_ids { get; set; }
        public List<PushNotificationRootObjectResult> results { get; set; }
    }
    public class PushNotificationRootObjectResult
    {
        public string message_id { get; set; }
        public string error { get; set; }
    }
}
