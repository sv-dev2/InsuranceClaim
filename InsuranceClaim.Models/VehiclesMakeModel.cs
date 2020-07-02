using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class VehiclesMakeModel
    {

        public int Id { get; set; }
        [Display(Name = "Make Description")]
        [Required(ErrorMessage = "Please Enter Make Description.")]
        public string MakeDescription { get; set; }
        [Display(Name = "Make Code")]
        [Required(ErrorMessage = "Please Enter Make Code.")]
        public string MakeCode { get; set; }
        [Display(Name = "Short Description")]
        [Required(ErrorMessage = "Please Enter Short Description.")]
        public string ShortDescription { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public int ModifiedBy { get; set; }
        public bool IsActive { get; set; }

    }
}
