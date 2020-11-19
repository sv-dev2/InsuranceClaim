using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class BusinessSourceModel
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
