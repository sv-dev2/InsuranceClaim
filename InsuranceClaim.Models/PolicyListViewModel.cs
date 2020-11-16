using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class PolicyListViewModel
    {

        public List<VehicleReinsuranceViewModel> Vehicles { get; set; }
        public string PolicyNumber { get; set; }
        public int PolicyId { get; set; }
        public int CustomerId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal TotalSumInsured { get; set; }
        public decimal TotalPremium { get; set; }
        public int SummaryId { get; set; }
        public DateTime createdOn { get; set; }
        public bool  IsActive { get; set; }
        public string CustomerEmail { get; set; }
        public string PolicyStatus { get; set; }
        public string CustomerName { get; set; }

        public string AgentName { get; set; }

        public string CustomerContactNumber { get; set; }

        public string CoverTypeName { get; set; }
        public string PaymentTerm { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string RegisterationNumber { get; set; }

        public string startdate { get; set; }
        public string enddate { get; set; }
        public string RenewalDate { get; set; }

        public DateTime TransactionDate { get; set; }

        public string Currency { get; set; }

        public string PaymentMethod { get; set; }

        public string VehicleName { get; set; }

        public decimal VehicleSumInsured { get; set; }

        public int VehicleDetailId { get; set; }

        public int SummaryDetailId { get; set; }

        public DateTime CoverStartDate { get; set; }

        public DateTime CoverEndDate { get; set; }

        public DateTime RenewalDate1 { get; set; }
        public string LicExpiryDate { get; set; }

        public bool isLapsed { get; set; }

        public string RenewPolicyNumber { get; set; }

        //ReinsuranceCommission,ReinsurancePremium, ReinsuranceAmount

        public decimal BrokerCommission { get; set; }
        public decimal FacultativeCommission { get; set; }
        public decimal AutoFacPremium { get; set; }
        public decimal FacPremium { get; set; }
        public decimal AutoFacReinsuranceAmount { get; set; }
        public decimal FacReinsuranceAmount { get; set; }

        public string PaymentStatus { get; set; }

        public string LicExpDate { get; set; }

        public string ModifiedOn { get; set; }

    }

    public class ListPolicy
    {
        public List<PolicyListViewModel> listpolicy { get; set; }


        [Required(ErrorMessage = "Please Enter Start Date.")]
        public string FromDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }
    }

    public class VehicleReinsuranceViewModel
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
        public string RegistrationNo { get; set; }
        

        public string Make { get; set; }
        public string Model { get; set; }
        public string CoverTypeName { get; set; }
        public string PaymentTerm { get; set; }

        public string currency { get; set; }

        public string Vehicle { get; set; }

    }
}
