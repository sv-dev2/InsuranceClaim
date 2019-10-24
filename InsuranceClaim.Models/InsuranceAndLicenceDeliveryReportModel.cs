using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class InsuranceAndLicenceDeliveryReportModel
    {
       
        public string PaymentMethod { get; set; }
        public string TransactionReference { get; set; }
        public string CustomerName { get; set; }
        public string PolicyNo { get; set; }
        public string Courier { get; set; }

        public string ContactDetails { get; set; }
        public string AddressOfCustomer { get; set; }
        public string DeliveryAddress { get; set; }


        public DateTime DateDeliverd { get; set; }
        public string TimeofDelivery { get; set; }
        public int Receiptno { get; set; }
        public decimal? ReceiptAmount { get; set; }

        public string SignaturePath { get; set; }




    }
    public class ListInsuranceAndLicenceDeliveryReport
    {
        public List<InsuranceAndLicenceDeliveryReportModel> InsuranceAndLicense { get; set; }
    }
    public class InsuraceAndLicenseSearchReportModel
    {
        public List<InsuranceAndLicenceDeliveryReportModel> InsuranceAndLicense { get; set; }
        [Required(ErrorMessage = "Please Enter Start Date.")]
        public string FromDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }
    }
}


