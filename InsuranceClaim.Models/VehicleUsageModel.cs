using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class VehicleUsageModel
    {
        public int Id { get; set; }
        [Display(Name = "Product Name")]
        public int ProductId { get; set; }
        [Display(Name = "Vehicle Usage")]
        [Required(ErrorMessage = "Please Enter Veh Usage.")]
        public string VehUsage { get; set; }
        [Display(Name = "Comprehensive Rate")]
        [Required(ErrorMessage = "Please Enter Comprehensive Rate.")]
        public Single? ComprehensiveRate { get; set; }
        [Display(Name = "Min Comp Amount")]
        [Required(ErrorMessage = "Please Enter Min Comp Amount.")]
        public decimal? MinCompAmount { get; set; }
        [Display(Name = "USD Minimum Benchmark")]
        [Required(ErrorMessage = "Please Enter USD Minimum Benchmark.")]
        public Single? USDBenchmark { get; set; }
        [Display(Name = "Third Party Rate")]
        [Required(ErrorMessage = "Please Enter Third Party Rate.")]
        public Single? ThirdPartyRate { get; set; }
        [Display(Name = "Min Third Amount")]
        [Required(ErrorMessage = "Please Enter Min Third Amount.")]
        public decimal? MinThirdAmount { get; set; }
        [Display(Name = "FTP Amount")]
        [Required(ErrorMessage = "Please Enter FTP Amount.")]
        public decimal? FTPAmount { get; set; }
        [Display(Name = "Annual TP Amount")]
        [Required(ErrorMessage = "Please Enter Annual TP Amount.")]
        public decimal? AnnualTPAmount { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }
    }
}