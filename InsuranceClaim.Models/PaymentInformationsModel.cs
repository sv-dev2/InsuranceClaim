using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
   public class PaymentInformationsModel
    {

        public int Id { get; set; }
        public int? SummaryDetailId { get; set; }
        public int VehicleDetailId { get; set; }
        public int PolicyId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int CurrencyId { get; set; }
        public string DebitNote { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? ModifiedBy { get; set; }

    }
}
