using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class ReceiptModuleHistoryModel
    {
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNumber { get; set; }
        public int? PaymentMethodId { get; set; }
        public decimal? AmountDue { get; set; }

        public decimal? AmountPaid { get; set; }
        public string Balance { get; set; }
        public DateTime DatePosted { get; set; }
    }


    public  class ReceiptDeliveryModule
    {
        public string customerFirstName { get; set; }
        public string customerLastName { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string city { get; set; }
        public string phoneNumber { get; set; }
        public string policyID { get; set; }
        public string policyTransactionDate { get; set; }
        public decimal policyAmount { get; set; }
        public string agentID { get; set; }
        public string agentName { get; set; }
        public string zoneName { get; set; }


    }
}
