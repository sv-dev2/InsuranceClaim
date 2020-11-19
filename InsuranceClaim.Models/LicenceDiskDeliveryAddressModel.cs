using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class LicenceDiskDeliveryAddressModel
    {
        public int Id { get; set; }
        [Display(Name = "Address1")]
        [Required(ErrorMessage = "Please Enter Address1.")]
        public string Address1 { get; set; }
        [Display(Name = "Address2")]
        [Required(ErrorMessage = "Please Enter Address2.")]
        public string Address2 { get; set; }
        [Display(Name = "City")]
        [Required(ErrorMessage = "Please Enter City.")]
        public string City { get; set; }
        public int VehicleId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }

    }
}
