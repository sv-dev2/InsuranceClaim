using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class CustomerListingReportModel
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public DateTime Dateofbirth { get; set; }
        public string NationalIdentificationNumber { get; set; }
        public string ContactNumber { get; set; }
        public string EmailAddress { get; set; }
        public string City { get; set; }
        public string Product { get; set; }
        public string VehicleMake { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleUsage { get; set; }
        public string PaymentTerm { get; set; }
        public string PaymentType { get; set; }
    

    }
    public class ListCustomerListingReport
    {
        public List<CustomerListingReportModel> CustomerListingReport { get; set; }
    }
    public class CustomerListingSearchReportModel
    {
        public List<CustomerListingReportModel> CustomerListingReport { get; set; }
        [Required(ErrorMessage = "Please Enter Start Date.")]
        public string FromDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }
    }
}
