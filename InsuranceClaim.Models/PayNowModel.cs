using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class PayNowModel
    {
        public string TransactionId { get; set; }

        public bool IsSuccessPayment { get; set; }

        public string Message { get; set; }

        public string PollUrl { get; set; }

        public string ReturnUrl { get; set; }

    }


    public class EcoCashModel
    {
        [Required()]
        public int SummaryId { get; set; }
        [Required()]
        public int PaymentMethodId { get; set; }
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage = "OTP Required")]
        public string OTP { get; set; }
    }

    




}
