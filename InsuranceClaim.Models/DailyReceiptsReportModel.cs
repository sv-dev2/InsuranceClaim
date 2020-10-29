using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class DailyReceiptsReportModel
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Contact { get; set; }
        public string Product { get; set; }
        public string PolicyNumber { get; set; }
        public string TransactionDate{ get; set; }
        public string ReceiptNumber { get; set; }
        public decimal? PremiumDue { get; set; }
        public string AmountPaid { get; set; }
        public string Balance { get; set; }
        public string PaymentType { get; set; }

    }
    public class ListDailyReceiptsReport
    {
        public List<DailyReceiptsReportModel> DailyReceiptsReport { get; set; }
    }
    public class DailyReceiptsSearchReportModel
    {
        public List<PreviewReceiptListModel> DailyReceiptsReport { get; set; }
        [Required(ErrorMessage = "Please Enter Start Date.")]
        public string FromDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }

        public int? BranchId { get; set; }
    }
}
