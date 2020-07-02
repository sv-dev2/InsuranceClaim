using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public  class EndorsementPaymentInformationModel
    {

        public int Id { get; set; }
        public int? PrimarySummaryDetailId { get; set; }
        public int? PrimaryVehicleDetailId { get; set; }
        public int? PrimaryPolicyId { get; set; }
        public int? PrimaryCustomerId { get; set; }
        public int? ProductId { get; set; }
        public int? CurrencyId { get; set; }
        public string DebitNote { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public int? EndorsementSummaryId { get; set; }
        public int? EndorsementVehicleId { get; set; }
        public int? EndorsementCustomerId { get; set; }
        public int? EndorsementPolicyId { get; set; }
        public bool DeleverLicence { get; set; }
        public string PaymentId { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
    }
}
