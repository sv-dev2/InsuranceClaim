using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class InflationFactorModel
    {
        public int Id { get; set; }
        [Display(Name = "Inflation Factor")]
        [Required(ErrorMessage = "Please Enter Inflation Factor.")]
        public int? InflationFact { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ActivatedOn { get; set; }
        public DateTime? DeActivatedOn { get; set; }
    }

}
