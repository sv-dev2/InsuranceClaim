using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class CardDetailModel
    {
        [Required(ErrorMessage = "Please Enter Card Number")]
        //[RegularExpression(@"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|6(?:011|5[0-9][0-9])[0-9]{12}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|(?:2131|1800|35\d{3})\d{11})$", ErrorMessage = "Not a Valid Card Number.")]
        public string CardNumber { get; set; }
        [Required(ErrorMessage = "Please Enter Name of Card")]
        public string NameOnCard { get; set; }
        [Required(ErrorMessage = "Please Enter Expiry Date ")]
        public string ExpiryDate { get; set; }
        [Required(ErrorMessage ="Please Enter CVC")]
        public string CVC { get; set; }
        public int SummaryDetailId { get; set; }
        public int? EndorsementSummaryId { get; set; }
    }


    public class IceCashCardDetailModel
    {
        public int SummaryId { get; set; }

        public decimal Amount { get; set; }

        public int PaymentMethod { get; set; }
    }


}
