using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Insurance.Domain;
using System.Threading.Tasks;

namespace Insurance.Service
{
    public class PaymentInformationService
    {

        public Int32 Insert(PaymentInformation paymentinfo)
        {
            try
            {
                InsuranceContext.PaymentInformations.Insert(paymentinfo);
                return 1;
            }
            catch (Exception ex)
            {
                return 0;
            }
            
        }

        public PaymentInformation GetById(Int32 Id)
        {
            try
            {
              
               return InsuranceContext.PaymentInformations.SingleCustome(Id);
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}
