using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class SummaryDetailModel
    {
        //public SummaryDetailModel()
        //{
        //    IceCashModel = new IceCashModel();
        //}

        public int Id { get; set; }
        public int? PaymentTermId { get; set; }
        public int? PaymentMethodId { get; set; }
        public decimal? TotalSumInsured { get; set; }
        public decimal? TotalPremium { get; set; }
        public decimal? TotalStampDuty { get; set; }
        public decimal? TotalZTSCLevies { get; set; }
        public decimal? TotalRadioLicenseCost { get; set; }
        public string DebitNote { get; set; }
        public string ReceiptNumber { get; set; }
        public bool SMSConfirmation { get; set; }
        public int? CarInsuredCount { get; set; }
        [Display(Name = "Excess Buy Back Amount")]
        public decimal? ExcessBuyBackAmount { get; set; }
        [Display(Name = "Roadside Assistance Amount")]
        public decimal? RoadsideAssistanceAmount { get; set; }
        [Display(Name = "Medical Expenses Amount")]
        public decimal? MedicalExpensesAmount { get; set; }
        [Display(Name = "Passenger Accident Cover Amount")]
        public decimal? PassengerAccidentCoverAmount { get; set; }
        public decimal? ExcessAmount { get; set; }
        public decimal? Discount { get; set; }
        [Required(ErrorMessage = "Please Enter Amount to be paid")]
        public decimal AmountPaid { get; set; }
        public decimal? MaxAmounttoPaid { get; set; }
        public decimal? MinAmounttoPaid { get; set; }
        public DateTime BalancePaidDate { get; set; }
        public string Notes { get; set; }
        public bool isQuotation { get; set; }

        public int CustomSumarryDetilId { get; set; }

        public string InsuranceId { get; set; }

        public string InvoiceNumber { get; set; }

        public string Currency { get; set; }

        // risk cover
        public string Error { get; set; }
        [Display(Name = "Basic Premium")]
        public decimal? AnnualRiskPremium { get; set; }
        public decimal? Premium { get; set; }
        [Display(Name = "Premium Due")]
        public decimal? PremiumDue { get; set; }

        //  public IceCashModel IceCashModel { get; set; }

    }
}
