using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class ReinsuranceReport
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PolicyNumber { get; set; }
        public string TransactionDate { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public decimal? SumInsured { get; set; }
        public string PhoneNumber { get; set; }
        public decimal? Premium { get; set; }
        public decimal AutoFacSumInsured { get; set; }
        public decimal AutoFacPremium { get; set; }
        public decimal AutoFacCommission { get; set; }
        public decimal FacSumInsured { get; set; }
        public decimal FacPremium { get; set; }
        public decimal FacCommission { get; set; }

        public string Currency { get; set; }

        public string Make { get; set; }
        public string Model { get; set; }

        public string RegistrationNumber { get; set; }
    }

    public class ListReinsuranceReport
    {
        public List<ReinsuranceReport> ReinsuranceReport { get; set; }
    }
    public class ReinsuranceSearchReport
    {
        public List<ReinsuranceReport> ReinsuranceReport { get; set; }
        [Required(ErrorMessage = "Please Enter Start Date.")]
        public string FromDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }
    }

}
