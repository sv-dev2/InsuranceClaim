using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class IceCashModel
    {
        public string partner_id { get; set; }

        public decimal amount { get; set; }

        public Guid client_reference { get; set; }

       // public string IceCashRequestUrl { get; set;  }

        public string success_url { get; set; }

        public string failed_url { get; set; }

        public string results_url { get; set; }

        public string details { get; set; }

        public string check_hash { get; set; }

        public string status { get; set; }

        public string message { get; set; }

        public string technical_reason { get; set; }


    }
}
