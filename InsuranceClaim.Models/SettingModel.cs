using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class SettingModel
    {
        public int Id { get; set; }
        [Display(Name = "Key Name")]
        [Required(ErrorMessage = "Please Enter Key Name.")]
        public string keyname { get; set; }
        [Display(Name = "Value")]
        [Required(ErrorMessage = "Please Enter Value.")]
        public decimal? value { get; set; }
        [Display(Name = "Value Type")]
        [Required(ErrorMessage = "Please Select Value Type.")]
        public int? ValueType { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


    }
}
