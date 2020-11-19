using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class EndorsementPolicyDetailModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Please Enter Policy Number.")]
        [MaxLength(25, ErrorMessage = "Policy number must be less than 25 characters long.")]
        public string PolicyNumber { get; set; }
        [Display(Name = "Insurer")]
        public int? InsurerId { get; set; }
       
        public int PolicyStatusId { get; set; }
       
        public int CurrencyId { get; set; }
        [Display(Name = "Start Date")]
        [Required(ErrorMessage = "Please Enter Policy Start Date")]
        public DateTime? StartDate { get; set; }
        //[Display(Name = "End Date")]
        [Required(ErrorMessage = "Please Enter Policy End Date.")]
        public DateTime? EndDate { get; set; }
        //[Display(Name = "Renewal Date")]
        public DateTime? RenewalDate { get; set; }
        [Display(Name = "Transaction Date")]
        public DateTime? TransactionDate { get; set; }
        [Display(Name = "Business Source")]
        public int BusinessSourceId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public bool? IsActive { get; set; }
        public int PolicyTerm { get; set; }
        public int? EndorsementCustomerId { get; set; }
        public int? PrimaryPolicyId { get; set; }
        public int? CustomerId { get; set; }
        public string PolicyName { get; set; }
    }

}
