using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class LoyaltyPointsModel
    {
        public string CustomerName { get; set; }
        public string Address { get; set; }
        public string CellPhoneNumber { get; set; }
        public string LoyaltyPoints { get; set; }
        public decimal SumInsured { get; set; }
        public decimal PremiumPaid { get; set; }
        public string EmailAddress { get; set; }

        public int PolicyId { get; set; }

        public string Currency { get; set; }

        public DateTime TransactionDate { get; set; }
             
        

    }

    public class LoyaltyPointsReport
    {
        public List<LoyaltyPointsModel> LoyaltyPoints { get; set; }
    }

    public class LoyaltyPointsReportSeachModels
    {
        public List<LoyaltyPointsModel> LoyaltyPoints { get; set; }
        [Required(ErrorMessage = "Please Enter Start Date.")]
        public string FromDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }
    }
}
