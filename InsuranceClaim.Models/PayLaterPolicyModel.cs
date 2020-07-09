using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace InsuranceClaim.Models
{
    public class PayLaterPolicyDetail
    {
        public int PolicyId { get; set; }

        public int PaymentInformationId { get; set; }
        public int SummaryDetailId { get; set; }
        public string PolicyNumber { get; set; }
        public string CustomerName { get; set; }
        public string RegistrationNo { get; set; }
        public string MakeDescription { get; set; }

        public string ModelDescription { get; set; }
        public decimal TotalPremium { get; set; }
        public int PaymentTypeId { get; set; }

        public string Vehicle { get; set; }

        public int VehicleID { get; set; }

        public string ErrorMsg { get; set; }


    }

    public class PayLaterPolicyModel
    {
        public List<PayLaterPolicyDetail> PayLaterPolicyDetails { get; set; }

        public string FromDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }

    }


}
