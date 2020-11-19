using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class CertSerialNoModel
    {
        public int Id { get; set; }
        public string PolicyNumber { get; set; }
        public string VRN { get; set; }
        public string CertSerialNo { get; set; }
        public string AgentName { get; set; }
        public int ALMBranchId { get; set; }
        public string BranchName { get; set; }
        public string CreatedOn { get; set; }
            
    }


    public class CertSerialNoReportModel
    {
        public string FromDate { get; set; }

        public string EndDate { get; set; }
     
        public int? BranchId { get; set; }

        public List<CertSerialNoModel> List { get; set; }
    }






}
