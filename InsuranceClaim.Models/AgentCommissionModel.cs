using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class AgentCommissionModel
    {
        public int Id { get; set; }
        [Display(Name = "Commission Through")]
        [Required(ErrorMessage = "Please Enter Commission Name.")]
        public string CommissionName { get; set; }
        [Display(Name = "Commission Amount (%)")]
        [Required(ErrorMessage = "Please Enter Commission Amount.")]
        public double? CommissionAmount { get; set; }
        [Display(Name = "Management Commission")]
        [Required(ErrorMessage = "Please Enter Management Commission.")]
        public double? ManagementCommission { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public string Source { get; set; }
        public int BusinessSourceId { get; set; }
    }
}