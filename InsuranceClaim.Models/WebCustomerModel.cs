using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class WebCustomerModel
    {
        public CustomerModel Customer { get; set; }

        public RiskDetailModel RiskDetail { get; set; }

        public SummaryDetailModel SummaryDetail { get; set; }
    }

    public class SearchVRNModel
    {
        public string RegistrationNum { get; set; }
        public string NationalId { get; set; }
    }

}
