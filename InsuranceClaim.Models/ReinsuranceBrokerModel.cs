using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class ReinsuranceBrokerModel
    {

        public int Id { get; set; }
        [Display(Name = "Reinsurance Broker Code")]
        [Required(ErrorMessage = "Please Enter Reinsurance Broker Code.")]
        public string ReinsuranceBrokerCode { get; set; }
        [Display(Name = "Reinsurance Broker Name")]
        [Required(ErrorMessage = "Please Enter Reinsurance Broker Name.")]
        public string ReinsuranceBrokerName { get; set; }
        [Display(Name = "Commission")]
        [Required(ErrorMessage = "Please Enter Commission.")]
        public decimal? Commission { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }
}
