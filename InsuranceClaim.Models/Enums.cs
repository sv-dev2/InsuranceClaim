using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public enum eCoverType
    {
        Comprehensive = 4,
        ThirdParty = 1,
        FullThirdParty = 2

        //Comprehensive = 1,
        //ThirdParty = 2,
        //FullThirdParty = 3
    }
    public enum eExcessType
    {
        Percentage = 1,
        FixedAmount = 2
    }
    public enum ePaymentTerm
    {
        Annual = 1,
        //Monthly = 2,
      //  Quarterly = 3,
        Termly = 4,
        Termly_5 =5,
        Termly_6 = 6,
        Termly_7 = 7,
        Termly_8 = 8,
        Termly_9 = 9,
        Termly_10 = 10,
        Termly_11 = 11
    }

    public enum eSettingValueType
    {
        percentage = 1,
        amount = 2
    }
    public enum eStatus
    {
        Quote = 1,
        InForce = 2,
        Lapsed = 3,
        NTU = 4,
        Renewed = 5
    }

    public enum ePolicyRenewReminderType
    {
        Email = 1,
        SMS = 2
    }
    public enum ePayeeBankDetails
    {
        Bank = 1,
        MobileMoney = 2,
        Cash = 3
    }
    public enum eVehicleType
    {
        Private = 1,
        Commercial = 2
    }

    public enum claimStatus
    {
        New = 1,
        Approved = 2,
        Rejected = 3
    }

    public enum paymentMethod
    {
        ecocash = 2,
        Zimswitch = 6,
        Cash = 1,
         PayLater = 6,
         PayNow = 3,
        //PayLater = 1008
    }

    public enum currencyType
    {
        USD = 1,
        RTGS=6
    }

    public enum PolicyType
    {
        New,
        Renew,
        Endorsement,
    }

    public enum ReportTypeEnum
    {
        Combined = 1,
        ALM = 2,
        CallCenter = 3
    }

    public enum ALMBranch
    {
        GeneCallCentre=6
    }

   


}
