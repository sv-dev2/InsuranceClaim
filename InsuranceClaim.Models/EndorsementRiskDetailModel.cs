using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
  public class EndorsementRiskDetailModel
    {

        public bool IncludeRadioLicenseCost { get; set; }
        public int Id { get; set; }

        public int EndorsementVehicleId { get; set; }
        public int VehicleId { get; set; }
        public int PolicyId { get; set; }
        //[Required(ErrorMessage = "Please Enter No Of Cars Covered")]
        //[DefaultValue(1)]
        public int? NoOfCarsCovered { get; set; }
        [Required(ErrorMessage = "Please Enter Registration No")]
        public string RegistrationNo { get; set; }
        public int? CustomerId { get; set; }
        [Required(ErrorMessage = "Please Select Vehicle Make")]
        public string MakeId { get; set; }
        [Required(ErrorMessage = "Please Select Vehicle Model")]
        public string ModelId { get; set; }
        [Required(ErrorMessage = "Please Enter Cubic Capacity")]
        public decimal? CubicCapacity { get; set; }
       // [Required(ErrorMessage = "Please Enter Vehicle Year")]
        public int? VehicleYear { get; set; }
        [Required(ErrorMessage = "Please Enter Engine Number")]
        public string EngineNumber { get; set; }
        [Required(ErrorMessage = "Please Enter Chassis Number")]
        public string ChasisNumber { get; set; }
        public string VehicleColor { get; set; }
        public int? VehicleUsage { get; set; }
        [Required(ErrorMessage = "Please Select Cover Type")]
        public int? CoverTypeId { get; set; }
        [Required(ErrorMessage = "Please Enter Cover Start Date")]
        public DateTime? CoverStartDate { get; set; }
        [Required(ErrorMessage = "Please Enter Cover End Date")]

        public DateTime? CoverEndDate { get; set; }


        public decimal? SumInsured { get; set; }
        [Required(ErrorMessage = "Please Enter Basic Premium")]
        public decimal? Premium { get; set; }
        public int? AgentCommissionId { get; set; }
        public decimal? Rate { get; set; }
        public decimal? StampDuty { get; set; }
        public decimal? ZTSCLevy { get; set; }
        public decimal? RadioLicenseCost { get; set; }
        public string OptionalCovers { get; set; }
        public int ExcessType { get; set; }
        public decimal Excess { get; set; }
        public string CoverNoteNo { get; set; }
        public bool IsLicenseDiskNeeded { get; set; }
        public Boolean Addthirdparty { get; set; }
        public decimal? AddThirdPartyAmount { get; set; }

        public Boolean PassengerAccidentCover { get; set; }
        public Boolean ExcessBuyBack { get; set; }
        public Boolean RoadsideAssistance { get; set; }
        public Boolean MedicalExpenses { get; set; }
        public int? NumberofPersons { get; set; }
        public bool chkAddVehicles { get; set; }
        public bool isUpdate { get; set; }
        public int vehicleindex { get; set; }
        public decimal? PassengerAccidentCoverAmount { get; set; }
        public decimal? ExcessBuyBackAmount { get; set; }
        public decimal? RoadsideAssistanceAmount { get; set; }
        public decimal? MedicalExpensesAmount { get; set; }
        public decimal? PassengerAccidentCoverAmountPerPerson { get; set; }
        public decimal? ExcessBuyBackPercentage { get; set; }
        public decimal? RoadsideAssistancePercentage { get; set; }
        public decimal? MedicalExpensesPercentage { get; set; }
        public decimal? ExcessAmount { get; set; }
        public DateTime RenewalDate { get; set; }
        public DateTime TransactionDate { get; set; }
        [Required(ErrorMessage = "Please Select Payment Term.")]
        public int PaymentTermId { get; set; }
        [Required(ErrorMessage = "Please Select A Product.")]
        public int ProductId { get; set; }
        public string InsuranceId { get; set; }
        public decimal? AnnualRiskPremium { get; set; }
        public decimal? TermlyRiskPremium { get; set; }
        public decimal? QuaterlyRiskPremium { get; set; }
        public decimal? Discount { get; set; }
        public decimal? BalanceAmount { get; set; }
        public string LicenseAddress1 { get; set; }
        public string LicenseAddress2 { get; set; }
        public string LicenseCity { get; set; }
        public bool isWebUser { get; set; }
        public decimal SuggestedValue { get; set; }
        public decimal VehicleLicenceFee { get; set; }

        public int? SummaryId { get; set; }

        public CustomerModel CustomerDetails { get; set; }
        public string VechicalMake { get; set; }

        public string VechicalModel { get; set; }

        public string CoverTypeName { get; set; }

        public string PolicyExpireDate { get; set; }

        public int BusinessSourceId { get; set; }

        public int CurrencyId { get; set; }
        public int? PrimaryVehicleId { get; set; }
        public int? EndorsementCustomerId { get; set; }
        public int? EndorsementPolicyId { get; set; }
        public int? EndorsementSummaryId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public bool? IsActive { get; set; }
        public bool? isLapsed { get; set; }
        public bool? IsCompleted { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public string Currency { get; set; }

        public int TaxClassId { get; set; }

        public bool IncludeLicenseFee { get; set; }

        public int? ZinaraLicensePaymentTermId { get; set; }
        public int? RadioLicensePaymentTermId { get; set; }

        public string CombinedID { get; set; }

        [Display(Name="Serial Number")]
        public string CertSerialNo { get; set; }

        public string ErrorMessage { get; set; }


        public string ArrearsAmt { get; set; }
        public decimal PenaltiesAmt { get; set; }

        public decimal TransactionAmt { get; set; }

        public decimal AdministrationAmt { get; set; }


    }
}
