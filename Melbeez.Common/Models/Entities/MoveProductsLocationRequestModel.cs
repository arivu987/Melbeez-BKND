using System.Collections.Generic;

namespace Melbeez.Common.Models.Entities
{
    public class MoveProductsLocationRequestModel
    {
        public List<long> ProductsId { get; set; }
        public long LocationId { get; set; }
    }
}
