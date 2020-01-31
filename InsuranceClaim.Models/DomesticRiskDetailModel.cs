using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    
    public class DomesticRiskDetailModel
    {

        public int Id { get; set; }
        public int PolicyId { get; set; }
        //[Required(ErrorMessage = "Please Enter No Of Cars Covered")]
        //[DefaultValue(1)]
        public int? NoOfCarsCovered { get; set; }
        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "Please Enter Cover Start Date")]
        public DateTime? CoverStartDate { get; set; }
        [Required(ErrorMessage = "Please Enter Cover End Date")]
        public DateTime? CoverEndDate { get; set; }

        [Range(3500, int.MaxValue, ErrorMessage = "Minimum CoverAmount should be 3500.")]
        public decimal? CoverAmount { get; set; }
        [Required(ErrorMessage = "Please Enter Basic Premium")]
        public int ProductId { get; set; }
        public int RiskCoverId { get; set; }
        public int RiskItemId { get; set; }
        public string RiskAddress { get; set; }
        public int PaymentTermId { get; set; }
        public int PaymentTypeId { get; set; }
     
        public decimal Rate { get; set; }
        public string Notes { get; set; }
        public decimal BasicPremium { get; set; }
        public decimal StampDuty { get; set; }
        public decimal Premium { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime RenewDate { get; set; }

        public DateTime PolicyExpireDate { get; set; }
        public string RenewPolicyNumber { get; set; }
        public int vehicleindex { get; set; }

        public bool isUpdate { get; set; }

        public int CurrencyId { get; set; }

        public bool chkAddVehicles { get; set; }

    }


    public class RiskCoverModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string RiskCover { get; set; }
    }


    public class RiskItemModel
    {
        public int Id { get; set; }
        public int CoverId { get; set; }
        public string RiskItem { get; set; }
        public decimal Rate { get; set; }
    }



}
