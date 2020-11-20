using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class PolicyDocumentModels
    {
        public int id { get; set; }
        public string Title { get; set; }
        public string Decription { get; set; }
        public string FilePath { get; set; }
        public int CustomerId { get; set; }
        public string PolicyNumber { get; set; }

    }
}
