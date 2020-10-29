using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace InsuranceClaim.Models
{
    public class PreviewReceiptListModel
    {

        public List<PreviewReceiptListModel> listReceipt { get; set; }
        public int Id { get; set; }
        public int PolicyId { get; set; }
        public int VehicleId { get; set; }
        public int CustomerId { get; set; }
        public string PolicyNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string PolicyNo { get; set; }
        public string PaymentDetails { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNumber { get; set; }
        public int? PaymentMethodId { get; set; }
        public decimal? AmountDue { get; set; }
        public string Date { get; set; }
        public decimal? AmountPaid { get; set; }
        public int TotalPremium { get; set; }
        public string Balance { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime TransactionDate { get; set; }
        public string filepath { get; set; }
        public string paymentMethodType { get; set; }

        public string TransactionReference { get; set; }

        public string PolicyCreatedBy { get; set; }

        public int CreatedBy { get; set; }

        public string Currency { get; set; }

        public DateTime? CreatedOn { get; set;}

        public decimal ZinaraFee { get; set; }

        public decimal RadioCost { get; set; }

        public decimal TenderedAmount { get; set; }

        public string BranchName { get; set; }

    }
    public class ListReceiptModule
    {
        public List<PreviewReceiptListModel> listReceipt { get; set; }
    }
}
