using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
  public class ClaimDetailsProviderModel
    {
        public int Id { get; set; }
        [Display(Name = "Assign Assessors ")]
        [Required(ErrorMessage = "Please Select Assessors Provider.")]
        public int AssessorsProviderType { get; set; }
        [Display(Name = "Assign Valuers")]
        [Required(ErrorMessage = "Please Select Valuers Provider.")]
        public int ValuersProviderType { get; set; }
        [Display(Name = "Policy Number")]
        //[Required(ErrorMessage = "Please Select Assessors Provider Type.")]
        public string PolicyNumber { get; set; }
        [Display(Name = "Assign Lawyers ")]
        [Required(ErrorMessage = "Please Select Lawyers Provider.")]
        public int LawyersProviderType { get; set; }
        [Display(Name = "Assign Repairers ")]
        [Required(ErrorMessage = "Please Select Repairers Provider.")]
        public int RepairersProviderType { get; set; }

        //[Display(Name = "Assessors Provider Type")]
        //[Required(ErrorMessage = "Please Select Assessors Provider Type.")]
        public int ClaimNumber { get; set; }
        public string CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        public string ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

        public string Assessors_Type { get; set; }
        public string Towing_Type { get; set; }
        public string Medical_Type { get; set; }

        public string Valuers_Type { get; set; }
        public string Lawyers_Type { get; set; }
        public string Repairers_Type { get; set; }
        [Display(Name = "Townly Provider Type ")]
        [Required(ErrorMessage = "Please Select Townly Provider.")]
        public int TownlyProviderType { get; set; }
        [Display(Name = "Medical Provider Type ")]
        [Required(ErrorMessage = "Please Select Medical Provider.")]
        public int MedicalProviderType { get; set; }
        public decimal? AssessorsProviderFees { get; set; }
        public decimal? ValuersProviderFees { get; set; }
        public decimal? LawyersProviderFees { get; set; }
        public decimal? RepairersProviderFees { get; set; }
        public decimal? TownlyProviderFees { get; set; }
        public decimal? MedicalProviderFees { get; set; }

        public int ClaimRegistrationId { get; set; }

        public string ServiceProviderName { get; set; }

        public string ProviderType { get; set; }

    }
}
