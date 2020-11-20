using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class EndorsementPolicyListViewModel
    {
        public List<VehicleReinsuranceViewModel> Vehicles { get; set; }
        public string PolicyNumber { get; set; }
        public int CustomerId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal TotalSumInsured { get; set; }
        public decimal TotalPremium { get; set; }
        public int SummaryId { get; set; }
        public DateTime createdOn { get; set; }
        public bool IsActive { get; set; }
        public int? EndorsementSummaryId { get; set; }
        public int? EndorsementCustomerId { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerName { get; set; }

        public string Currency { get; set; }
    }
    public class ListEndorsementPolicy
    {
        public List<EndorsementPolicyListViewModel> listendorsementpolicy { get; set; }
    }
    public class EndorsementVehicleReinsurance
    {
        public int VehicleId { get; set; }
        public string MakeId { get; set; }
        public string ModelId { get; set; }
        public bool isReinsurance { get; set; }
        public decimal AutoFacReinsuranceAmount { get; set; }
        public decimal FacReinsuranceAmount { get; set; }
        public int ReinsurerBrokerId { get; set; }
        public decimal SumInsured { get; set; }
        public decimal AutoFacPremium { get; set; }
        public decimal FacPremium { get; set; }
        public string RegisterationNumber { get; set; }
        public int CoverType { get; set; }
        public DateTime startdate { get; set; }
        public DateTime enddate { get; set; }
        public DateTime RenewalDate { get; set; }
        public decimal BrokerCommission { get; set; }
        public bool isLapsed { get; set; }
        public decimal FacultativeCommission { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal Premium { get; set; }
        public bool isActive { get; set; }

      
    }
}
