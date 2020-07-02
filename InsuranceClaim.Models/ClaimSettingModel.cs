using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public  class ClaimSettingModel
    {
        public int Id { get; set; }
        [Display(Name = "Key Name")]
        [Required(ErrorMessage = "Please Enter Key Name.")]
        public string KeyName { get; set; }
        [Display(Name = "Value")]
        [Required(ErrorMessage = "Please Enter Value.")]
        public int Value { get; set; }
        [Display(Name = "Value Type")]
        [Required(ErrorMessage = "Please Enter Value Type.")]
        public int Valuetype { get; set; }
        [Display(Name = "Vehicle Type")]
        [Required(ErrorMessage = "Please Enter Vehicle Type.")]
        public int VehicleType { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
