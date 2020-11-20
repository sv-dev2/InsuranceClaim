using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
 public class ClaimDocumentModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string PolicyNumber { get; set; }
        public int ClaimNumber { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public int ServiceProvide { get; set; }
        public string ServiceProviderName { get; set; }
        //public DateTime ModifiedOn { get; set; }
        //public int ModifiedBy{ get; set; }
        public bool IsActive { get; set; }
    }
}
