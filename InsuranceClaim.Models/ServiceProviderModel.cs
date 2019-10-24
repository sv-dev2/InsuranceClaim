using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace InsuranceClaim.Models
{
    public class ServiceProviderModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please select Provider Type")]
        [Display(Name = "Service Provider Type")]
        public string ServiceProviderType { get; set; }

        [Required(ErrorMessage = "Please Enter Provider Name")]
        [Display(Name = "Service Provider Name")]
        public string ServiceProviderName { get; set; }

        [Required(ErrorMessage = "Please Enter Contact Details")]
        [Display(Name = "Service Provider Contact Details")]
        public string ServiceProviderContactDetails { get; set; }

        [Required(ErrorMessage = "Please Enter Provider Fees")]
        [Display(Name = "Service Provider Fees")]
        public decimal ServiceProviderFees { get; set; }

        public DateTime CreatedOn { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

        public int ProviderTypeId { get; set; }

        public int ClaimRegistrationId { get; set; }
        public decimal? OtherServiceProvider { get; set; }
        public decimal RepairersProviderFees { get; set; }
        public string Currency { get; set; }
    }
}
