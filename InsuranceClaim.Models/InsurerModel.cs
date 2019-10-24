using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class InsurerModel
    {
        public int Id { get; set; }
        public string InsurerName { get; set; }
        public string InsurerCode { get; set; }
        public string InsurerAddress { get; set; }
    }
}
