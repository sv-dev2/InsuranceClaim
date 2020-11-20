using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class PaynowResponse
    {
        public String status { get; set; }
        public String hash { get; set; }
        public String pollurl { get; set; }
        public String browserurl { get; set; }
        public String error { get; set; }
        public string generatedhash { get; set; }

    }

    public class PaynowResponsefromModel
    {
        public bool success { get; set; }
        public string url { get; set; }
        public string error { get; set; }
    }
}
