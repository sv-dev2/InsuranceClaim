using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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


    public class LicenseModel
    {

        [Required]
        public string VRN { get; set; }
        public string CombinedID { get; set; }
        public string FilePath { get; set; }
        [Required]
        public string SerialNumber { get; set; }
        public int VehicleId { get; set; }
        public string LicenseId { get; set; }
    }
}
