using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class UserModel
    {

        [Required(ErrorMessage = "Please Enter Email.")]
        public string Email { get; set; }

        [Display(Name = "Current Password")]
        [Required(ErrorMessage = "Please Enter Current Password.")]
        public string CurrentPassword { get; set; }

        [Display(Name = "New Password")]
        [Required(ErrorMessage = "Please Enter New Password.")]
        public string NewPassword { get; set; }

        [Display(Name = "Confirm Password")]
        [System.Web.Mvc.Compare("NewPassword", ErrorMessage = "The NewPassword and ConfirmPassword Password do not Match.")]
        public string ConfirmPassword { get; set; }
        public string ErrorMsg { get; set; }
        
           
    }
}
