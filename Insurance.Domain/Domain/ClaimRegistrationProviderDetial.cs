using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Domain
{
    public class ClaimRegistrationProviderDetial : Entity<ClaimRegistrationProviderDetial>
    {
        public int Id { get; set; }

        public int ClaimRegistrationId { get; set; }

        public int ServiceProviderTypeId { get; set; }

        public int ServiceProviderId { get; set; }

        public decimal ServiceProviderFee { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsActive { get; set; }

        public bool IsSaved { get; set; }

    }
}
