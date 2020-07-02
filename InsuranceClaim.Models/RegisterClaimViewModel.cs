using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace InsuranceClaim.Models

{
    public class RegisterClaimViewModel
    {
    
        public int ClaimId { get; set; }
        public int Id { get; set; }
        public string PolicyNumber { get; set; }
        public string DateOfLoss { get; set; }
        public string DateOfNotifications { get; set; }
        [Required(ErrorMessage = "Please Enter Place Of Loss")]
        public string PlaceOfLoss { get; set; }
        [Required(ErrorMessage = "Please Enter Description Of Loss")]
        public string DescriptionOfLoss { get; set; }
        [Required(ErrorMessage = "Please Enter Estimated Value Of Loss")]
        public decimal? EstimatedValueOfLoss { get; set; }
        //[Required(ErrorMessage = "Please Enter Third Party Damage Value")]
        public decimal? ThirdPartyDamageValue { get; set; }
        public string Status { get; set; }
        public long Claimnumber { get; set; }
        //[Required(ErrorMessage = "Please Enter Rejected Reason")]
        public string RejectionStatus { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool isChecked { get; set; }
        public int checkId { get; set; }
        public string CheckBoxName { get; set; }
        public bool Claimsatisfaction { get; set; }
        public string checklistvalue { get; set; }
        public int? AssessorsProviderType { get; set; }
        public int? ValuersProviderType { get; set; }
        public int? LawyersProviderType { get; set; }
        public int? RepairersProviderType { get; set; }
        public decimal? TotalProviderFees { get; set; }
        public int? TownlyProviderType { get; set; }
        public int? MedicalProviderType { get; set; }
        public decimal? AssessorsProviderFees { get; set; }
        public decimal? ValuersProviderFees { get; set; }
        public decimal? LawyersProviderFees { get; set; }
        public decimal? RepairersProviderFees { get; set; }
        public decimal? TownlyProviderFees { get; set; }
        public decimal? MedicalProviderFees { get; set; }

        public int ProviderId { get; set; }

        public decimal ProviderFee { get; set; }

        //For Vehicle details 
        public List<ChecklistModel> chklist { get; set; }
        public List<RegisterClaimViewModel> chklistDetail { get; set; }
        public string names { get; set; }
        public List<RiskViewModel> RiskViewModel { get; set; }    
         
        public List<ServiceProviderModel> ProviderList { get; set; }

        public List<ClaimRegistrationProviderDetial> ClaimRegistrationProviderList { get; set; }

        public string ServiceProviderType { get; set; }
        public bool? ThirdPartyInvolvement { get; set; }
        public IEnumerable<HttpPostedFileBase> Files { get; set; }

        public List<RegistrationDocument> RegistrationDocumentList { get; set; }


    }





    //public class RegisterClaimViewModel
    //{

    //    public int ClaimId { get; set; }
    //    public int Id { get; set; }
    //    public string PolicyNumber { get; set; }
    //    public string DateOfLoss { get; set; }   
    //    public string DateOfNotifications { get; set; }
    //    [Required(ErrorMessage = "Please Enter Place Of Loss")]
    //    public string PlaceOfLoss { get; set; }
    //    [Required(ErrorMessage = "Please Enter Description Of Loss")]
    //    public string DescriptionOfLoss { get; set; }
    //    [Required(ErrorMessage = "Please Enter Estimated Value Of Loss")]
    //    public decimal? EstimatedValueOfLoss { get; set; }
    //    [Required(ErrorMessage = "Please Enter Third Party Damage Value")]
    //    public string ThirdPartyDamageValue { get; set; }
    //    public string Status { get; set; }
    //    public long Claimnumber { get; set; }
    //    //[Required(ErrorMessage = "Please Enter Rejected Reason")]
    //    public string RejectionStatus { get; set; }

    //    public string FirstName { get; set; }
    //    public string LastName { get; set; }
    //    public bool Claimsatisfaction { get; set; }

    //    //For Vehicle details 
    //    public List<ChecklistModel> chklist { get; set; }
    //    public string names { get; set; }
    //    public List<RiskViewModel> RiskViewModel { get; set; }

    //}

    public class RiskViewModel
    {
        public int CustomerId { get; set; }
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CoverStartDate { get; set; }
        public string CoverEndDate { get; set; }
        public decimal? Premium { get; set; }
        public decimal? StampDuty { get; set; }
        public decimal? ZTSCLevy { get; set; }
        public decimal? Discount { get; set; }
        public decimal? RadioLicenseCost { get; set; }
        public decimal? VehicleLicenceFee { get; set; }
        public int? PaymentTermId { get; set; }
        public int? ProductId { get; set; }
        public int? CoverTypeId { get; set; }
        public string Addthirdparty { get; set; }
        public string IncludeRadioLicenseCost { get; set; }
        public string IsLicenseDiskNeeded { get; set; }

        public int? NumberofPersons { get; set; }
        public int? VehicleNumber { get; set; }
        public string RegisterNumber { get; set; }
        public string paymentTerm { get; set; }
        public string Product { get; set; }
        public string VehUsage { get; set; }
        public string CoverType { get; set; }
        public string SuggestedValue { get; set; }
        public decimal? SumInsured { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int? VehicleYear { get; set; }
        public decimal? CubicCapacity { get; set; }
        public string EngineNumber { get; set; }
        public string ChasisNumber { get; set; }
        public decimal? AddThirdPartyAmount { get; set; }
        public decimal Excess { get; set; }
        public string ExcessType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string PassengerAccidentCover { get; set; }
        public string ExcessBuyBack { get; set; }
        public string RoadsideAssistance { get; set; }
        public string MedicalExpenses { get; set; }
       
    }

    }
