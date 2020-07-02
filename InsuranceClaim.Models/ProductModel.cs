using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class ProductModel
    {

        public int Id { get; set; }
        [Display(Name = "Product Name")]
        [Required(ErrorMessage = "Please Enter Product Name.")]
        public string ProductName { get; set; }
        [Display(Name = "Product Code")]
        [Required(ErrorMessage = "Please Enter Product Code.")]
        public string ProductCode { get; set; }

        public bool? Active { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }
}