using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class PartnerModel
    {
        public int Id { get; set; }
        [Display(Name = "Partner Name")]
        [Required(ErrorMessage = "Please Enter PartnerName")]
        public string PartnerName { get; set; }
        public bool Status { get; set; }



    }
}
