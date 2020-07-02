using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
public class VehicleListModel
    {
        public string make { get; set; }
        public string model { get; set; }
        public string covertype { get; set; }
        public string suminsured { get; set; }
        public string premium { get; set; }
        public string radio_license_fee { get; set; }
        public string excess { get; set; }
        public string vehicle_license_fee { get; set; }
        public string stampDuty { get; set; }
        public string total { get; set; }
        public string RegistrationNo { get; set; }

        public string ZTSCLevy { get; set; }
        public int Id { get; set; }
        public string CurrencyName { get; set; }
        public decimal? Discount { get; set; }
    }
}
