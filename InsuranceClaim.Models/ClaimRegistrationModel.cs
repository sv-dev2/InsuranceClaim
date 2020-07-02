using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class ClaimRegistrationModel
    {
        public int Id { get; set; }
        public string PolicyNumber { get; set; }
        public string PaymentDetails { get; set; }
        //public string RiskDetails { get; set; }
        public long ClaimNumber { get; set; }
        public string Checklist { get; set; }
        public DateTime? DateOfLoss { get; set; }
        public DateTime? DateOfNotifications { get; set; }
        public string PlaceOfLoss { get; set; }
        public string DescriptionOfLoss { get; set; }
        public decimal? EstimatedValueOfLoss { get; set; }
        public decimal? ThirdPartyDamageValue { get; set; }
        public bool Claimsatisfaction { get; set; }
        public string ClaimStatus { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string RejectionStatus { get; set; }
        public string ServiceProvide { get; set; }
        public string AssessProviderName { get; set; }
        public string ValueProviderName { get; set; }
        public string LawyeProviderName { get; set; }
        public string RepairProviderName { get; set; }
        public string TownlyName { get; set; }
        public string MeadicalName { get; set; }
        public int VehicleDetailId { get; set; }
        public string RegistrationNo { get; set; }
        public decimal? TotalProviderFees { get; set; }

        public string ClaimValue { get; set; }
        public DateTime? ModifyOn { get; set; }
        public string ClaimantName { get; set; }
        public string MakeId { get; set; }
        public string ModelId { get; set; }
        public bool? ThirdPartyInvolvement { get; set; }

        public string ModelDescription { get; set; }

        public string MakeDescription { get; set; }
        public int ClaimNotificationId { get; set; }

        public List<ServiceProviderModel> ServiceProviderList { get; set; }
    }

    public class ClaimRegistrationModelNew
    {
        public string PolicyNumber { get; set; }
        public int Id { get; set; }
    }
    //public class ListClaimRegistrationModel
    //{



    //}

}
