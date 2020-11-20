using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class NewReconcilationReportModel
    {

        [Required(ErrorMessage = "Please Enter Start Date.")]
        public string FromDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }

        public List<RecieptAndPaymentModel> listInvoiceAndReciept { get; set; }

    }
    public class RecieptAndPaymentModel
    {

        public string PolicyCreatedBy { get; set; }
        public string CustomerName { get; set; }
     
        public string Currency { get; set; }
        
        public string Contact { get; set; }
        public string Product { get; set; }
        public string PolicyNumber { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime PolicyCreatedOn { get; set; }
        public string ReceiptDate { get; set; }
        public string ReceiptNumber { get; set; }
        public decimal? PremiumDue { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal? Balance { get; set; }
        public string PaymentType { get; set; }

    }
}
