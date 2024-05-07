using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melbeez.Business.Models
{
    public class CountryViewModel
    {
        public long Id { get; set; }

        [MaxLength(256)]
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public string CurrencyCode { get; set; }
    }
}
