using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace InsuranceClaim.Models
{
  public  class ClaimNotificationModel
    {
        public int Id { get; set; }  
        [Required(ErrorMessage = "Please Enter Policy Number")]
        [Display(Name = "Policy Number")]
        public string PolicyNumber { get; set; }

        //[Required(ErrorMessage = "Please Enter VRN Detail")]
        [Display(Name = "Search VRN/Policy Detail")]
        public string VRNDetail { get; set; }
        [Required(ErrorMessage = "Please Enter VRN Number")]
        [Display(Name = "VRN Number")]
        public string RegistrationNo { get; set; }
        [Required(ErrorMessage = "Please Enter Customer Name")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Please Enter Date")]
        [Display(Name = "Date Of Loss")]
        public DateTime DateOfLoss { get; set; }

        [Required(ErrorMessage = "Please Enter Place")]
        [Display(Name = "Place Of Loss")]
        public string PlaceOfLoss { get; set; }

        [Required(ErrorMessage = "Please Enter Description")]
        [Display(Name = "Description Of Loss")]
        public string DescriptionOfLoss { get; set; }

        [Required(ErrorMessage = "Please Enter Estimated Value")]
        [Display(Name = "Estimated Value Of Loss")]
        public decimal EstimatedValueOfLoss { get; set; }

        [Required(ErrorMessage = "This is required field")]
        [Display(Name = "Third Party Involvement")]
        public bool? ThirdPartyInvolvement { get; set; }
        //public int CreatedBy { get; set; }

       public string RenewPolicyNumber { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsRegistered { get; set; }
        
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool IsExists { get; set; }
        public string Currency { get; set; }

        [Required(ErrorMessage = "Please Enter Claimant Name")]
        [Display(Name = "Claimant Name")]
        public string ClaimantName { get; set; }
        public string VRNNumber { get; set; }
        public int? VehicleId { get; set; }
        public int? PolicyId { get; set; }
        [Display(Name = "Third Party Name")]
        public string ThirdPartyName { get; set; }
        [Display(Name = "Contact Details")]
        public string ThirdPartyContactDetails { get; set; }
        [Display(Name = "Make")]
        public string ThirdPartyMakeId { get; set; }
        [Display(Name = "Model")]
        public string ThirdPartyModelId { get; set; }
        [Display(Name = "Third Party Estimated Value Of Loss")]
        public decimal? ThirdPartyEstimatedValueOfLoss { get; set; }
        [Display(Name = "Cover Start Date")]
        [Required(ErrorMessage = "Please Cover Start Date")]
        public DateTime CoverStartDate { get; set; }
        [Required(ErrorMessage = "Please Cover Start Date")]
        [Display(Name = "Cover End Date")]
        public DateTime CoverEndDate { get; set; }
        [Display(Name = "Third Party Damage Value")]
        public decimal? ThirdPartyDamageValue { get; set; }


    }
}
