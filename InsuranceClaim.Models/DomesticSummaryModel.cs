using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   
    public class DomesticSummaryModel
    {
        //public SummaryDetailModel()
        //{
        //    IceCashModel = new IceCashModel();
        //}

        public int Id { get; set; }
        public int? PaymentTermId { get; set; }
        public int? PaymentMethodId { get; set; }

        public decimal Premium { get; set; }
        public decimal? TotalCoverAmount { get; set; }
        public decimal? TotalPremium { get; set; }
        public decimal? TotalStampDuty { get; set; }
 
        public string DebitNote { get; set; }
        public string ReceiptNumber { get; set; }
        public bool SMSConfirmation { get; set; }

        public decimal? Discount { get; set; }
        [Required(ErrorMessage = "Please Enter Amount to be paid")]
        public decimal AmountPaid { get; set; }
        public decimal? MaxAmounttoPaid { get; set; }
        public decimal? MinAmounttoPaid { get; set; }
        public DateTime BalancePaidDate { get; set; }
        public string Notes { get; set; }
        public bool isQuotation { get; set; }

        public int CarInsuredCount { get; set; }

        public string InvoiceNumber { get; set; }

        public string Currency { get; set; }

        // risk cover
        public string Error { get; set; }
        
        public int CustomSumarryDetilId { get; set; }
        public string ProductName { get; set; }
        public string CoverName { get; set; }
        public string RiskItem { get; set; }

        //  public IceCashModel IceCashModel { get; set; }

    }





}
