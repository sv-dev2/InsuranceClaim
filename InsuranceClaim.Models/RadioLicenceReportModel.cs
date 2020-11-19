using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
 public class RadioLicenceReportModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Policy_Number { get; set; }
        public string Transaction_date { get; set; }
        public decimal? RadioLicenseCost { get; set; }

        public string Currency { get; set; }
             
    }
    public class ListRadioLicenceReport
    {
        public List<RadioLicenceReportModel> RadioLicence { get; set; }
    }
    public class RadioLicenceSearchReportModel
    {
        public List<RadioLicenceReportModel> RadioLicence { get; set; }
        [Required(ErrorMessage = "Please Enter Start Date.")]
        public string FromDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }
    }
}
