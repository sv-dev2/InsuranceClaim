using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class EndorsementSummaryDetailModel
    {
        public int Id { get; set; }
        public int SummaryId { get; set; }
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

        public bool? IsCompleted { get; set; }
        public string InvoiceNumber { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int? EndorsementPolicyId { get; set; }
        public int? EndorsementCustomerId { get; set; }
        public int? PrimarySummaryId { get; set; }
        public int? CustomerId { get; set; }
        public int? VehicleDetailId { get; set; }
        public int? EndorsementVehicleId { get; set; }
        public decimal PayableAmount { get; set; }


    }
}
