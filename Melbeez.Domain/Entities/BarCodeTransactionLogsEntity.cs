using Melbeez.Domain.Common.BaseEntity;

namespace Melbeez.Domain.Entities
{
    public class BarCodeTransactionLogsEntity : AuditableEntity<long>
    {
        public string BarCode { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
}
