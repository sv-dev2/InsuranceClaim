using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class ClaimReportModel
    {

        public int Id { get; set; }
        public string InsuredName { get; set; }
        public string PolicyNumber { get; set; }
        public string PolicyStartDate { get; set; }
        public string PolicyEndDate { get; set; }
        public int? ClaimNumber { get; set; }
        public string ClaimantName { get; set; }
        public string ClaimStatus { get; set; }
        public string DateOfLoss { get; set; }
        public string DateOfNotification { get; set; }
        public string DescripationOfLoss { get; set; }
        public string CoverType { get; set; }
        public decimal SumInsured { get; set; }
        public string VehicleDescription { get; set; }
        public decimal EstimatedLoss { get; set; }
        public string VRN { get; set; }

        public string ProductName { get; set; }
        //public string ClassOfInsurance { get; set; }
    }

    public class ClaimReportList
    {
        public List<ClaimReportModel> ClaimReportModelData { get; set; }
    }
    public class ClaimSearchReport
    {
        public List<ClaimReportModel> ClaimReportModelData { get; set; }
        [Required(ErrorMessage = "Please Enter Start Date.")]
        public string FromDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }
    }
}
