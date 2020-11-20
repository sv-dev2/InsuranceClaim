using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class GrossWrittenPremiumReportModels
    {
        public int? Id { get; set; }
        public string Customer_Name { get; set; }
        public string Policy_Number { get; set; }
        public string Policy_endate { get; set; }
        public string Policy_startdate { get; set; }
        public string Transaction_date { get; set; }
        public string Vehicle_makeandmodel { get; set; }
        public string Payment_Mode { get; set; }
        public string Payment_Term { get; set; }
        public decimal Annual_Premium { get; set; }
        public decimal Stamp_duty { get; set; }
        public decimal ZTSC_Levy { get; set; }
        public decimal? Net_Premium { get; set; }
        public decimal Premium_due { get; set; }
        public decimal GrandPremium { get; set; }
        public decimal Comission_percentage { get; set; }
        public decimal Comission_Amount { get; set; }
        public decimal Sum_Insured { get; set; }
        public decimal? RadioLicenseCost { get; set; }

        public string CoverType { get; set; }

        public decimal? Zinara_License_Fee { get; set; }

        public string PolicyCreatedBy { get; set; }

        public DateTime PolicyRenewalDate { get; set; }
        public bool? IsLapsed { get; set; }
        public bool? IsActive { get; set; }
        public string Currency { get; set; }

        public string RenewPolicyNumber {get; set;}

        public string ALMId { get; set; }

        public string CoverNoteNum { get; set; }

        public string BranchName { get; set; }

        public string BusinessSourceName { get; set; }

        public string SourceDetailName { get; set; }

        public int SummaryDetailId { get; set; } 

        public int PaymentMethodId { get; set; }

        public int CreatedBy { get; set; }

        public int AgentActualPolicy { get; set; }

        public decimal AgentTargetPolicy { get; set; }

        public decimal AgentTargetGwp { get; set; }

        public decimal AgentActualGwp { get; set; }

        public decimal TransactionPerformance { get; set; }

        public decimal TransactionVariance { get; set; }

        public decimal GwpPerformance { get; set; }

        public decimal GwpVariance { get; set; }

        public string WorkDesc { get; set; }

    }

    public class ReportType
    {
        public int Id { get; set; }
        public string ReportName { get; set; }
    }
    public class ListGrossWrittenPremiumReportModels
    {
        public List<GrossWrittenPremiumReportModels> ListGrossWrittenPremiumReportdata { get; set; }
    }
    public class GrossWrittenPremiumReportSearchModels
    {
        
        public List<int> BranchId { get; set; }

        public int ReportTypeId { get; set; }
        public List<BranchModel> ListBranchModelData { get; set; }
        public List<GrossWrittenPremiumReportModels> ListGrossWrittenPremiumReportdata { get; set; }
        
        [Required(ErrorMessage = "Please Enter Start Date.")]
        public string FormDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }

    }
    public class ALMParnterSearchModels
    {

        public int PartnerId { get; set; }

        public int ReportTypeId { get; set; }
        public List<PartnerModel> ListPartnerModelData { get; set; }
        public List<ALMParnterSearchModelsData> ListReportdata { get; set; }

        [Required(ErrorMessage = "Please Enter Start Date.")]
        public string FormDate { get; set; }
        [Required(ErrorMessage = "Please Enter End Date.")]
        public string EndDate { get; set; }

    }

    public class ALMParnterSearchModelsData
    {
        public string BranchName { get; set; }
        public string PolicyNumber { get; set; }
        public string CoverNoteNumber { get; set; }
        public string PolicyDate { get; set; }
        public string PaymentDetail { get; set; }
        public decimal GrossPremium { get; set; }
        public decimal CommissionAmount { get; set; } 
        public decimal Totals { get; set; }
    }


}

