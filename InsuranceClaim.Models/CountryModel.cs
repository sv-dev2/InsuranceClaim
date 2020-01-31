using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class Country
    {
        public string code { get; set; }
        public string name { get; set; }
        public string DisplayName { get; set; }
    }

    public class RootObject
    {
        public List<Country> countries { get; set; }
    }



}
