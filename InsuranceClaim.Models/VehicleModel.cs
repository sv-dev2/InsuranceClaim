using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class ClsVehicleModel
    {
        public int Id { get; set; }
        [Display(Name = "Model Description")]
        [Required(ErrorMessage = "Please Enter Model Description.")]
        public string ModelDescription { get; set; }
        [Display(Name = "Model Code")]
        [Required(ErrorMessage = "Please Enter Model Code.")]
        public string ModelCode { get; set; }
        [Display(Name = "Short Description")]
        [Required(ErrorMessage = "Please Enter Short Description.")]
        public string ShortDescription { get; set; }
        [Display(Name = "Make Code")]
        [Required(ErrorMessage = "Please Enter Make Code.")]
        public string MakeCode { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

        public string MakeDescription { get; set; }
    }
}
