using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melbeez.Business.Models.UserModels.ResponseModels
{
    public class StatesResponseModel
    {
        public long Id { get; set; }
        public string StateName { get; set; }
        public long CountryId { get; set; }
        public string CountryName { get; set; }
    }
}
