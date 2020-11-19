using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class ClaimRegistrationProviderModel
    {
        public int Id { get; set; }

        public int ClaimRegistrationId { get; set; }

        public int ServiceProviderTypeId { get; set; }

        public int ServiceProviderId { get; set; }

        public decimal ServiceProviderFee { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsActive { get; set; }

        public bool IsSaved { get; set; }

        public string ServiceProviderName { get; set; }

        public string ServiceProviderType { get; set; }
        public string RegistrationNo { get; set; }
        public string  PolicyNumber{get; set;}
    public string ClaimantName { get; set; }

    }
}
