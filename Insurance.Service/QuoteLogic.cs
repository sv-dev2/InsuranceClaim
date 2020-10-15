using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain;
using InsuranceClaim.Models;
using System.Configuration;


namespace Insurance.Service
{

    public class QuoteLogic
    {
        public decimal Premium { get; set; }
        public decimal StamDuty { get; set; }
        public decimal ZtscLevy { get; set; }
        public bool Status { get; set; } = true;
        public string Message { get; set; }
        public decimal ExcessBuyBackAmount { get; set; }
        public decimal RoadsideAssistanceAmount { get; set; }
        public decimal MedicalExpensesAmount { get; set; }
        public decimal PassengerAccidentCoverAmount { get; set; }
        public decimal PassengerAccidentCoverAmountPerPerson { get; set; }
        public decimal ExcessBuyBackPercentage { get; set; }
        public decimal RoadsideAssistancePercentage { get; set; }
        public decimal MedicalExpensesPercentage { get; set; }
        public decimal ExcessAmount { get; set; }
        public decimal AnnualRiskPremium { get; set; }
        public decimal TermlyRiskPremium { get; set; }
        public decimal QuaterlyRiskPremium { get; set; }
        public decimal Discount { get; set; }

        public QuoteLogic CalculatePremium(int vehicleUsageId, decimal sumInsured, eCoverType coverType, eExcessType excessType, decimal excess, int PaymentTermid, decimal? AddThirdPartyAmount, int NumberofPersons, Boolean Addthirdparty, Boolean PassengerAccidentCover, Boolean ExcessBuyBack, Boolean RoadsideAssistance, Boolean MedicalExpenses, decimal? RadioLicenseCost, Boolean IncludeRadioLicenseCost, Boolean isVehicleRegisteredonICEcash, string BasicPremiumICEcash, string StampDutyICEcash, string ZTSCLevyICEcash, int ProductId = 0, string vehicleStartDate = "", string vehicleEndDate = "", string manufacturerYear = "", bool isAgentStaff = false, bool IsEndorsment = false, int currencyId = 6)
        {
            var vehicleUsage = InsuranceContext.VehicleUsages.Single(vehicleUsageId);
            var Setting = InsuranceContext.Settings.All();
            var DiscountOnRenewalSettings = Setting.Where(x => x.keyname == "Discount On Renewal").FirstOrDefault();

            decimal ratingPremium = 0;


            var additionalchargeatp = 0.0m;
            var additionalchargepac = 0.0m;
            var additionalchargeebb = 0.0m;
            var additionalchargersa = 0.0m;
            var additionalchargeme = 0.0m;
            float? InsuranceRate = 0;
            decimal? InsuranceMinAmount = 0;
            this.AnnualRiskPremium = 0.00m;
            this.QuaterlyRiskPremium = 0.00m;
            this.TermlyRiskPremium = 0.00m;
            this.Discount = 0.00m;


            if (coverType == eCoverType.Comprehensive)
            {
                //17 jun 2020  
                decimal? minAmount = 0;
                float incrementRate = Convert.ToSingle(0.5);
                // InsuranceRate = vehicleUsage.ComprehensiveRate;


                //decimal InflationFactorAmt = 25;
                //decimal premiumRate = Convert.ToDecimal((vehicleUsage.ComprehensiveRate * 90) / 100);
                //minAmount = ((vehicleUsage.USDBenchmark * InflationFactorAmt) * premiumRate) / 100;
                ////InsuranceMinAmount = vehicleUsage.MinCompAmount;
                //InsuranceMinAmount = minAmount;

                incrementRate = Convert.ToSingle(0.5);
                InsuranceRate = vehicleUsage.ComprehensiveRate + incrementRate;



                if (currencyId == (int)currencyType.USD)
                {
                    // incrementRate = Convert.ToSingle(0.5);
                    //  InsuranceRate = vehicleUsage.ComprehensiveRate + incrementRate;
                }
                else
                {
                    InsuranceMinAmount = vehicleUsage.MinCompAmount;
                }

            }



            else if (coverType == eCoverType.ThirdParty)
            {
                InsuranceRate = vehicleUsage.AnnualTPAmount == null ? 0 : (float)vehicleUsage.AnnualTPAmount;
                InsuranceMinAmount = vehicleUsage.MinThirdAmount;
            }
            else if (coverType == eCoverType.FullThirdParty)
            {
                InsuranceRate = (float)vehicleUsage.FTPAmount;
                InsuranceMinAmount = vehicleUsage.FTPAmount;
            }


            var premium = 0.00m;

            if (coverType == eCoverType.ThirdParty)
            {
                premium = (decimal)InsuranceRate;
            }
            else if (coverType == eCoverType.FullThirdParty)
            {
                premium = (decimal)InsuranceRate;
            }
            else
            {
                premium = (sumInsured * Convert.ToDecimal(InsuranceRate)) / 100;
            }


            // For ratinglist for AgentMotor
            // Session["CustomerDataModal"]

            if (isAgentStaff == true && coverType == eCoverType.Comprehensive && vehicleStartDate != "" && vehicleEndDate != "" && manufacturerYear != "")
            {
                if (System.Web.HttpContext.Current.Session["CustomerDataModal"] != null)
                {
                    var customer = (CustomerModel)System.Web.HttpContext.Current.Session["CustomerDataModal"];

                    // var customerAge = (DateTime.Now - customer.DateOfBirth);
                    var customerAge = CalculateInYear(customer.DateOfBirth.Value, DateTime.Now);

                    //  var AgeOfLicense = DateTime.Now - Convert.ToDateTime(manufacturerYear);

                    //  var AgeOfLicense = DateTime.Now - Convert.ToDateTime(manufacturerYear);

                    if (manufacturerYear == "")
                        manufacturerYear = DateTime.Now.ToShortDateString();

                    var AgeOfLicense = CalculateInYear(Convert.ToDateTime(manufacturerYear), DateTime.Now);

                    // var AgeOfVehicle = Convert.ToDateTime(vehicleEndDate) - Convert.ToDateTime(vehicleStartDate);

                    var AgeOfVehicle = CalculateInYear(Convert.ToDateTime(vehicleStartDate), Convert.ToDateTime(vehicleEndDate));

                    ratingPremium = RatingListCalcualation(premium, ProductId, customer.Gender, customerAge, AgeOfLicense, AgeOfVehicle);
                }
            }


            // if (premium < InsuranceMinAmount && coverType == eCoverType.Comprehensive && currencyId!=1) // 1 represent to usd, in case of USD min and max should not allowed
            if (premium < InsuranceMinAmount && coverType == eCoverType.Comprehensive)
            {
                Status = false;
                premium = InsuranceMinAmount.Value;
                this.Message = "Insurance minimum amount $" + InsuranceMinAmount + " Charge is applicable.";
            }


            var settingAddThirdparty = Convert.ToDecimal(Setting.Where(x => x.keyname == "Addthirdparty").Select(x => x.value).FirstOrDefault());

            if (Addthirdparty)
            {
                var AddThirdPartyAmountADD = AddThirdPartyAmount;

                if (AddThirdPartyAmountADD > 10000)
                {
                    var Amount = AddThirdPartyAmountADD - 10000;
                    premium += Convert.ToDecimal((Amount * settingAddThirdparty) / 100);
                }
            }


            int day = 0;
            double calulateTerm = 0;
            switch (PaymentTermid)
            {
                case 3:
                    premium = premium / 4;
                    break;
                case 4:
                    premium = premium / 3;
                    break;
                case 5:
                    day = 5 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 6:
                    day = 6 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 7:
                    day = 7 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 8:
                    day = 8 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 9:
                    day = 9 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 10:
                    day = 10 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 11:
                    day = 11 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
            }




            decimal PassengerAccidentCoverAmountPerPerson = Convert.ToInt32(Setting.Where(x => x.keyname == "PassengerAccidentCover").Select(x => x.value).FirstOrDefault());
            decimal ExcessBuyBackPercentage = Convert.ToInt32(Setting.Where(x => x.keyname == "ExcessBuyBack").Select(x => x.value).FirstOrDefault());
            decimal RoadsideAssistancePercentage = Convert.ToDecimal(Setting.Where(x => x.keyname == "RoadsideAssistance").Select(x => x.value).FirstOrDefault());
            decimal MedicalExpensesPercentage = Convert.ToDecimal(Setting.Where(x => x.keyname == "MedicalExpenses").Select(x => x.value).FirstOrDefault());
            var StampDutySetting = Setting.Where(x => x.keyname == "Stamp Duty").FirstOrDefault();
            var ZTSCLevySetting = Setting.Where(x => x.keyname == "ZTSC Levy").FirstOrDefault();




            if (PassengerAccidentCover)
            {
                int totalAdditionalPACcharge = NumberofPersons * Convert.ToInt32(PassengerAccidentCoverAmountPerPerson);
                additionalchargepac = totalAdditionalPACcharge;
            }

            if (ExcessBuyBack)
            {
                additionalchargeebb = (sumInsured * ExcessBuyBackPercentage) / 100;
            }

            if (RoadsideAssistance)
            {
                if ((coverType == eCoverType.ThirdParty || coverType == eCoverType.FullThirdParty) && ProductId == 1) // for private car
                {
                    var roadsideAssistanceDetails = Setting.Where(x => x.keyname == "third party private cars roadside assistance").FirstOrDefault();
                    if (roadsideAssistanceDetails != null)
                    {
                        additionalchargersa = Math.Round(Convert.ToDecimal(roadsideAssistanceDetails.value), 2);
                    }
                }
                else if ((coverType == eCoverType.ThirdParty || coverType == eCoverType.FullThirdParty) && (ProductId == 3 || ProductId == 11)) // Commercial vehicles
                {
                    var roadsideAssistanceDetails = Setting.Where(x => x.keyname == "third party commercial vehicle roadside assistance").FirstOrDefault();
                    if (roadsideAssistanceDetails != null)
                    {
                        additionalchargersa = Math.Round(Convert.ToDecimal(roadsideAssistanceDetails.value), 2);
                    }
                }
                else
                {
                    additionalchargersa = (sumInsured * RoadsideAssistancePercentage) / 100;

                    //switch (PaymentTermid)
                    //{
                    //    case 3:
                    //        additionalchargersa = 4.5m / 4;
                    //        break;
                    //    case 4:
                    //        additionalchargersa = 4.5m / 3;
                    //        break;
                    //    case 5:
                    //        additionalchargersa = 4.5m / 5;
                    //        break;
                    //    case 6:
                    //        additionalchargersa = 4.5m / 6;
                    //        break;
                    //    case 7:
                    //        additionalchargersa = 4.5m / 7;
                    //        break;
                    //    case 8:
                    //        additionalchargersa = 4.5m / 8;
                    //        break;
                    //    case 9:
                    //        additionalchargersa = 4.5m / 9;
                    //        break;
                    //    case 10:
                    //        additionalchargersa = 4.5m / 10;
                    //        break;
                    //    case 11:
                    //        additionalchargersa = 4.5m / 11;
                    //        break;
                    //    case 1:
                    //        additionalchargersa = 4.5m;
                    //        break;
                    //}


                }
            }


            if (MedicalExpenses)
            {
                additionalchargeme = (sumInsured * MedicalExpensesPercentage) / 100;
            }

            if (excessType == eExcessType.FixedAmount && excess > 0)
            {
                this.ExcessAmount = excess;
            }

            //  this.Premium = premium;// 04_sep_2019




            if (!isVehicleRegisteredonICEcash && coverType != eCoverType.Comprehensive)
                this.Premium = premium;   //premium * (5*2)
            else
                this.Premium = premium;


            this.PassengerAccidentCoverAmount = Math.Round(additionalchargepac, 2);
            this.PassengerAccidentCoverAmountPerPerson = Math.Round(PassengerAccidentCoverAmountPerPerson, 2);
            this.RoadsideAssistanceAmount = Math.Round(additionalchargersa, 2);

            this.RoadsideAssistancePercentage = Math.Round(RoadsideAssistancePercentage, 2);
            this.MedicalExpensesAmount = Math.Round(additionalchargeme, 2);
            this.MedicalExpensesPercentage = Math.Round(MedicalExpensesPercentage, 2);
            this.ExcessBuyBackAmount = Math.Round(additionalchargeebb, 2);
            this.ExcessBuyBackPercentage = Math.Round(ExcessBuyBackPercentage, 2);

            //premium = premium + stampDuty + ztscLevy;
            //premium = premium + additionalchargeebb + additionalchargeme + additionalchargepac + additionalchargersa + (IncludeRadioLicenseCost ? Convert.ToDecimal(RadioLicenseCost) : 0.00m );// + Convert.ToDecimal(AgentCommission);
            //if (excessType == eExcessType.Percentage && excess > 0)
            //{
            //    InsuranceRate = InsuranceRate + float.Parse(excess.ToString());
            //}

            if (excessType == eExcessType.Percentage && excess > 0)
            {
                this.ExcessAmount = (sumInsured * excess) / 100;
            }


            //decimal totalPremium = 0;
            //decimal calCulatePremiumWithOptional = 0;

            //if (coverType == eCoverType.Comprehensive)
            //{
            //    calCulatePremiumWithOptional = this.Premium + this.PassengerAccidentCoverAmount + this.RoadsideAssistanceAmount + this.MedicalExpensesAmount + this.ExcessBuyBackAmount + this.ExcessAmount;
            //}
            //else
            //{
            //    calCulatePremiumWithOptional = (isVehicleRegisteredonICEcash ? Convert.ToDecimal(BasicPremiumICEcash) : this.Premium) + this.PassengerAccidentCoverAmount + this.RoadsideAssistanceAmount + this.MedicalExpensesAmount + this.ExcessBuyBackAmount + this.ExcessAmount;
            //}




            //switch (PaymentTermid)
            //{
            //    case 3:
            //        premium = calCulatePremiumWithOptional / 4;
            //        break;
            //    case 4:
            //        premium = calCulatePremiumWithOptional / 3;
            //        break;
            //}


            var discountField = this.PassengerAccidentCoverAmount + this.RoadsideAssistanceAmount + this.MedicalExpensesAmount + this.ExcessBuyBackAmount + this.ExcessAmount;

            switch (PaymentTermid)
            {
                case 3:
                    discountField = discountField / 4;
                    break;
                case 4:
                    discountField = discountField / 3;
                    break;
                case 5:
                    day = 5 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 6:
                    day = 6 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 7:
                    day = 7 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 8:
                    day = 8 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 9:
                    day = 9 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 10:
                    day = 10 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
                case 11:
                    day = 11 * 30;
                    discountField = Math.Round(Convert.ToDecimal((double)day / 365) * discountField, 2);
                    break;
            }




            switch (PaymentTermid)
            {
                case 1:
                    this.AnnualRiskPremium = premium + discountField;
                    if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive))
                    {
                        this.AnnualRiskPremium = Convert.ToDecimal(BasicPremiumICEcash);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        this.Discount = Math.Round(((this.AnnualRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100), 2);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        this.Discount = Math.Round(Convert.ToDecimal(DiscountOnRenewalSettings.value), 2);
                    }
                    break;
                case 3:
                    this.QuaterlyRiskPremium = premium + discountField;
                    if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive))
                    {
                        this.QuaterlyRiskPremium = Convert.ToDecimal(BasicPremiumICEcash);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        this.Discount = Math.Round(((this.QuaterlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100), 2);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        this.Discount = Math.Round(Convert.ToDecimal(DiscountOnRenewalSettings.value), 2);
                    }
                    break;
                case 4:
                    this.TermlyRiskPremium = premium + discountField;
                    if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive))
                    {
                        this.TermlyRiskPremium = BasicPremiumICEcash == "" ? 0 : Convert.ToDecimal(BasicPremiumICEcash);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        this.Discount = Math.Round(((this.TermlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100), 2);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        this.Discount = Math.Round(Convert.ToDecimal(DiscountOnRenewalSettings.value), 2);
                    }
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    this.AnnualRiskPremium = premium + discountField;
                    if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive))
                    {
                        this.AnnualRiskPremium = Convert.ToDecimal(BasicPremiumICEcash);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        this.Discount = Math.Round(((this.AnnualRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100), 2);
                    }
                    if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        this.Discount = Math.Round(Convert.ToDecimal(DiscountOnRenewalSettings.value), 2);
                    }
                    break;

            }

            if (!isVehicleRegisteredonICEcash && coverType != eCoverType.Comprehensive)
            {
                this.Discount = this.Discount; //this.Discount * (5*2);
            }

            //if(currencyId==(int)currencyType.USD) // 17th jun 2020 // updated on 24 july 2020
            //{
            //    this.Discount = 0;
            //}

            // totalPremium = premium - this.Discount;
            decimal totalPremium = 0;

            if (coverType == eCoverType.Comprehensive)
            {
                totalPremium = (this.Premium + discountField) - this.Discount;
            }
            else
            {
                totalPremium = ((isVehicleRegisteredonICEcash ? Convert.ToDecimal(BasicPremiumICEcash) : this.Premium) + discountField) - this.Discount;
            }



            //  premium = (decimal)totalPremium;


            this.Premium = Math.Round(totalPremium, 2);


            var stampDuty = 0.00m;

            var totalPremiumForStampDuty = (this.Premium + discountField + this.Discount);

            if (StampDutySetting.ValueType == Convert.ToInt32(eSettingValueType.percentage))
            {
                stampDuty = (totalPremiumForStampDuty * Convert.ToDecimal(StampDutySetting.value)) / 100;
                // stampDuty = (totalPremium * Convert.ToDecimal(StampDutySetting.value)) / 100;
            }
            else
            {
                stampDuty = totalPremiumForStampDuty + Convert.ToDecimal(StampDutySetting.value);
                // stampDuty = totalPremium + Convert.ToDecimal(StampDutySetting.value);
            }


            //if (stampDuty > 2000000)
            //{
            //    Status = false;
            //    this.Message = "Stamp Duty must not exceed $2,000,000";
            //}


            var ztscLevy = 0.00m;

            decimal totalPremiumForZtscLevy = 0;

            //if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive))
            //{
            //    totalPremiumForZtscLevy = (isVehicleRegisteredonICEcash ? Convert.ToDecimal(BasicPremiumICEcash) : this.Premium) + this.PassengerAccidentCoverAmount + this.RoadsideAssistanceAmount + this.MedicalExpensesAmount + this.ExcessBuyBackAmount + this.ExcessAmount;
            //}
            //else
            //{
            //    totalPremiumForZtscLevy = (this.Premium) + this.PassengerAccidentCoverAmount + this.RoadsideAssistanceAmount + this.MedicalExpensesAmount + this.ExcessBuyBackAmount + this.ExcessAmount;
            //}


            if (coverType == eCoverType.Comprehensive)
            {
                // totalPremiumForZtscLevy = (this.Premium + discountField);
                totalPremiumForZtscLevy = (this.Premium + discountField + this.Discount);
            }
            else
            {
                // totalPremiumForZtscLevy = (isVehicleRegisteredonICEcash ? Convert.ToDecimal(BasicPremiumICEcash) : this.Premium) + discountField;
                totalPremiumForZtscLevy = (isVehicleRegisteredonICEcash ? Convert.ToDecimal(BasicPremiumICEcash) : this.Premium) + discountField + this.Discount;  // need to discuss BasicPremiumICEcash with Client
            }


            if (ZTSCLevySetting.ValueType == Convert.ToInt32(eSettingValueType.percentage))
            {
                // ztscLevy = (totalPremium * Convert.ToDecimal(ZTSCLevySetting.value)) / 100;
                ztscLevy = (totalPremiumForZtscLevy * Convert.ToDecimal(ZTSCLevySetting.value)) / 100;
            }
            else
            {
                // ztscLevy = totalPremium + Convert.ToDecimal(ZTSCLevySetting.value);
                ztscLevy = totalPremiumForZtscLevy + Convert.ToDecimal(ZTSCLevySetting.value);
            }


            this.StamDuty = Math.Round(stampDuty, 2);
            this.ZtscLevy = Math.Round(ztscLevy, 2);

            // if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive) && totalPremium == Convert.ToDecimal(BasicPremiumICEcash)) // by ash 11 apr 2019


            if (StampDutyICEcash == "") // if iceCash is not working
            {
                this.StamDuty = Math.Round(stampDuty, 2);
                StampDutyICEcash = Math.Round(stampDuty, 2).ToString();
            }

            if (isVehicleRegisteredonICEcash && !(coverType == eCoverType.Comprehensive))
            {
                this.StamDuty = Math.Round(Convert.ToDecimal(StampDutyICEcash), 2);
                this.ZtscLevy = Math.Round(Convert.ToDecimal(ZTSCLevyICEcash), 2);
            }
            else
            {

                double maxZTSC = 1100;
                //  double maxZTSC = 10.80 ; // default ProductId=1;
                //double maxZTSC = 10.80 * (5*2); // default ProductId=1;
                //            // ztsc new calucation will be also aply for comprehensive


                if (coverType == eCoverType.Comprehensive)
                {
                    if (ProductId == 3 || ProductId == 11) // Commercial Commuter Omnibus and Commercial Vehicle
                    {
                        //  maxZTSC = 22.00;             
                        maxZTSC = 22.00 * (5 * 2);
                    }

                    if (currencyId == (int)currencyType.USD) // for usd
                    {
                        maxZTSC = 10.80;
                        if (ProductId == 3 || ProductId == 11) // Commercial Commuter Omnibus and Commercial Vehicle
                        {
                            maxZTSC = 22.80;
                        }
                    }
                }

                switch (PaymentTermid)
                {
                    case 1:
                        if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                        {
                            this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                        }
                        break;
                    case 3:
                        maxZTSC = maxZTSC * 4 / 12;
                        if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                        {
                            this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                        }
                        break;
                    case 4:
                        maxZTSC = maxZTSC / 3;
                        if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                        {
                            this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                        }
                        break;
                    case 5:
                        maxZTSC = maxZTSC * 5 / 12;
                        if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                        {
                            this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                        }
                        break;
                    case 6:
                        maxZTSC = maxZTSC * 6 / 12;
                        if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                        {
                            this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                        }
                        break;
                    case 7:
                        maxZTSC = maxZTSC * 7 / 12;
                        if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                        {
                            this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                        }
                        break;
                    case 8:
                        maxZTSC = maxZTSC * 8 / 12;
                        if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                        {
                            this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                        }
                        break;
                    case 9:
                        maxZTSC = maxZTSC * 9 / 12;
                        if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                        {
                            this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                        }
                        break;
                    case 10:
                        maxZTSC = maxZTSC * 10 / 12;
                        if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                        {
                            this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                        }
                        break;
                    case 11:
                        maxZTSC = maxZTSC * 11 / 12;
                        if (Convert.ToDouble(this.ZtscLevy) > maxZTSC)
                        {
                            this.ZtscLevy = Math.Round(Convert.ToDecimal(maxZTSC), 2);
                        }
                        break;
                }
            }


            if (!string.IsNullOrEmpty(StampDutyICEcash) && Convert.ToDecimal(StampDutyICEcash) > 100000)
            {
                this.StamDuty = 100000 * 2;
            }
            else if (coverType != eCoverType.Comprehensive && Convert.ToDecimal(StampDutyICEcash) < Convert.ToDecimal(7.50)) // minimum stamp duty
            {
                this.StamDuty = Convert.ToDecimal(7.50 * 2);
            }


            //else if (Convert.ToDecimal(StampDutyICEcash) < 2) // minimum stamp duty
            //{
            //    this.StamDuty = 2;
            //}


            this.Premium = this.Premium + ratingPremium;

            if (IsEndorsment && coverType == eCoverType.Comprehensive)
                this.Premium = CalculatPremiumAccourdingDay(premium, PaymentTermid, vehicleEndDate);

            //if (coverType == eCoverType.Comprehensive && currencyId==(int)currencyType.USD)
            //{
            //    RiskDetailModel modelUsd = new RiskDetailModel();
            //    modelUsd= GetCalculationDetailForUsd(PaymentTermid, this.Premium);
            //    if(modelUsd.Premium!=null && modelUsd.Premium!=0)
            //    {
            //        this.Premium = Convert.ToDecimal(modelUsd.Premium);
            //        this.StamDuty = Convert.ToDecimal(modelUsd.StampDuty);
            //        this.ZtscLevy = Convert.ToDecimal(modelUsd.ZTSCLevy);
            //    }
            //}

            return this;
        }


        public RiskDetailModel GetCalculationDetailForUsd(int paymentTermId, decimal calculatedPremium)
        {

            RiskDetailModel model = new RiskDetailModel();

            //decimal premium = 300;
            decimal premium = 210;
            decimal StamDuty = 18;
            decimal ZtscLevy = 12;


            switch (paymentTermId)
            {
                case 1:

                    if (calculatedPremium < premium)
                    {
                        model.Premium = premium;
                        model.StampDuty = StamDuty;
                        model.ZTSCLevy = ZtscLevy;
                    }

                    break;
                case 3:
                    premium = (premium / 12) * 3;
                    if (calculatedPremium < premium)
                    {
                        model.Premium = premium;
                        model.StampDuty = (StamDuty / 12) * 3;
                        model.ZTSCLevy = (ZtscLevy / 12) * 3;
                    }

                    break;
                case 4:
                    premium = (premium / 12) * 4;
                    if (calculatedPremium < premium)
                    {
                        model.Premium = premium;
                        model.StampDuty = (StamDuty / 12) * 4;
                        model.ZTSCLevy = (ZtscLevy / 12) * 4;
                    }
                    break;
                case 5:
                    premium = (premium / 12) * 5;
                    if (calculatedPremium < premium)
                    {
                        model.Premium = premium;
                        model.StampDuty = (StamDuty / 12) * 5;
                        model.ZTSCLevy = (ZtscLevy / 12) * 5;
                    }
                    break;
                case 6:
                    premium = (premium / 12) * 6;
                    if (calculatedPremium < premium)
                    {
                        model.Premium = premium;
                        model.StampDuty = (StamDuty / 12) * 6;
                        model.ZTSCLevy = (ZtscLevy / 12) * 6;
                    }
                    break;
                case 7:
                    premium = (premium / 12) * 7;
                    if (calculatedPremium < premium)
                    {
                        model.Premium = premium;
                        model.StampDuty = (StamDuty / 12) * 7;
                        model.ZTSCLevy = (ZtscLevy / 12) * 7;
                    }
                    break;
                case 8:
                    premium = (premium / 12) * 8;
                    if (calculatedPremium < premium)
                    {
                        model.Premium = premium;
                        model.StampDuty = (StamDuty / 12) * 8;
                        model.ZTSCLevy = (ZtscLevy / 12) * 8;
                    }
                    break;
                case 9:
                    premium = (premium / 12) * 9;
                    if (calculatedPremium < premium)
                    {
                        model.Premium = premium;
                        model.StampDuty = (StamDuty / 12) * 9;
                        model.ZTSCLevy = (ZtscLevy / 12) * 9;
                    }
                    break;
                case 10:
                    premium = (premium / 12) * 10;
                    if (calculatedPremium < premium)
                    {
                        model.Premium = premium;
                        model.StampDuty = (StamDuty / 12) * 10;
                        model.ZTSCLevy = (ZtscLevy / 12) * 10;
                    }
                    break;
                case 11:
                    premium = (premium / 12) * 11;
                    if (calculatedPremium < premium)
                    {
                        model.Premium = premium;
                        model.StampDuty = (StamDuty / 12) * 11;
                        model.ZTSCLevy = (ZtscLevy / 12) * 11;
                    }
                    break;
            }

            return model;
        }





        public QuoteLogic CalculateDomesticPremium(decimal InsuranceRate, decimal coverAmount, int PaymentTermid)
        {

            var Setting = InsuranceContext.Settings.All();
            var premium = 0.00m;
            int day = 0;


            // double calulateTerm = 0;           
            premium = (coverAmount * Convert.ToDecimal(InsuranceRate)) / 100;

            switch (PaymentTermid)
            {
                case 3:
                    premium = premium / 4;
                    break;
                case 4:
                    premium = premium / 3;
                    break;
                case 5:
                    day = 5 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 6:
                    day = 6 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 7:
                    day = 7 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 8:
                    day = 8 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 9:
                    day = 9 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 10:
                    day = 10 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
                case 11:
                    day = 11 * 30;
                    premium = Math.Round(Convert.ToDecimal((double)day / 365) * premium, 2);
                    break;
            }

            var StampDutySetting = Setting.Where(x => x.keyname == "Stamp Duty").FirstOrDefault();
            this.Premium = Math.Round(premium, 2);
            decimal totalPremium = this.Premium;

            var stampDuty = 0.00m;
            if (StampDutySetting.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                stampDuty = (totalPremium * Convert.ToDecimal(StampDutySetting.value)) / 100;
            else
                stampDuty = totalPremium + Convert.ToDecimal(StampDutySetting.value);



            this.StamDuty = Math.Round(stampDuty, 2);

            if (this.StamDuty > 100000)
            {
                this.StamDuty = 100000;
            }
            else if (this.StamDuty < 2) // minimum stamp duty
            {
                this.StamDuty = 2;
            }

            return this;
        }





        public decimal CalculatPremiumAccourdingDay(decimal premium, int paymentTermId, string vehicleEndDate)
        {
            // 4 month 30
            //120n day is 30

            if (paymentTermId == 1)
                paymentTermId = 12;


            //int totalDayOfVehicle = Convert.ToInt32((Convert.ToDateTime(vehicleEndDate) - vehicleStartDate).TotalDays);
            //int policyDaySpent = Convert.ToInt32((DateTime.Now - vehicleDetails.CoverStartDate).TotalDays);
            //// decimal amount = 12/policyDaySpent;
            //double refundAmount = ((double)policyDaySpent / totalDayOfVehicle) * (double)vehicleDetails.Premium;
            ////30 / 122 X 500


            var everyDayPremium = premium / (paymentTermId * 30);
            int day = Convert.ToInt32((Convert.ToDateTime(vehicleEndDate) - DateTime.Now).TotalDays);
            decimal calculatedPremium = Math.Round(Convert.ToDecimal(everyDayPremium * day), 2);
            return calculatedPremium;
        }


        public int CalculateInYear(DateTime startDate, DateTime endDate)
        {
            DateTime zeroTime = new DateTime(1, 1, 1);

            DateTime a = new DateTime(2007, 1, 1);
            DateTime b = new DateTime(2008, 1, 1);

            TimeSpan span = b - a;
            // Because we start at year 1 for the Gregorian
            // calendar, we must subtract a year here.
            int years = (zeroTime + span).Year - 1;
            return years;
        }


        public decimal RatingListCalcualation(decimal baseRate, int prodcutId, string Gender, int DriverAge, int AgeOfLicense, int AgeOfVehicle)
        {
            decimal calcuateRatingListPremium = 0;

            // Usage Presonal
            decimal businessLoadig = 50;

            // Driver Gender
            decimal femalDriverLoading = 10;

            // Driver Age
            decimal driverAgeBelow25Loading = 20;
            decimal driverAgeBelow25to35Loading = 10;
            decimal driverAgeAbove35Discount = 10;

            // Age of license
            decimal ageOfLicense0to3Loading = 20;
            decimal ageOfLicense3to5Loading = 10;
            decimal ageOfLicenseAbove5Discount = 10;

            // Age Of Vehicle
            decimal ageOfVehicle0to5Discount = 10;
            decimal ageOfVehicle5to10Loading = 10;
            decimal ageOfVehileAbove10Loading = 20;

            // decimal 

            decimal usageAmount = 0;
            decimal genderAmount = 0;
            decimal driverAgeAmount = 0;
            decimal driverAgeOfLicenseAmount = 0;
            decimal AgeOfVehicleAmount = 0;

            // USAGE
            if (prodcutId == 3 && prodcutId == 11) // for comercial vehicle
            {
                usageAmount = baseRate * businessLoadig / 100;
                calcuateRatingListPremium = calcuateRatingListPremium + usageAmount;
            }



            // Driver Gender
            if (Gender == "Female")
            {
                genderAmount = baseRate * femalDriverLoading / 100;
                calcuateRatingListPremium = calcuateRatingListPremium + genderAmount;
            }


            // Driver Age
            if (DriverAge < 25)
            {
                driverAgeAmount = baseRate * driverAgeBelow25Loading / 100;
                calcuateRatingListPremium = calcuateRatingListPremium + driverAgeAmount;
            }
            else if (DriverAge > 25 && DriverAge < 35)
            {
                driverAgeAmount = baseRate * driverAgeBelow25to35Loading / 100;
                calcuateRatingListPremium = calcuateRatingListPremium + driverAgeAmount;
            }
            else if (DriverAge > 35)
            {
                // discount
                driverAgeAmount = baseRate * driverAgeAbove35Discount / 100;
                calcuateRatingListPremium = calcuateRatingListPremium - driverAgeAmount;

            }

            // Age Of License
            if (AgeOfLicense > 0 && AgeOfLicense < 3)
            {
                driverAgeOfLicenseAmount = baseRate * ageOfLicense0to3Loading / 100;
                calcuateRatingListPremium = calcuateRatingListPremium + driverAgeOfLicenseAmount;
            }
            else if (AgeOfLicense > 3 && AgeOfLicense < 5)
            {
                driverAgeOfLicenseAmount = baseRate * ageOfLicense3to5Loading / 100;
                calcuateRatingListPremium = calcuateRatingListPremium + driverAgeOfLicenseAmount;
            }
            else if (AgeOfLicense > 5)
            {
                // discount
                driverAgeOfLicenseAmount = baseRate * ageOfLicenseAbove5Discount / 100;
                calcuateRatingListPremium = calcuateRatingListPremium - driverAgeOfLicenseAmount;
            }

            // Age of Vehicle


            if (AgeOfVehicle > 0 && AgeOfVehicle < 5)
            {
                //discount
                AgeOfVehicleAmount = baseRate * ageOfVehicle0to5Discount / 100;
                calcuateRatingListPremium = calcuateRatingListPremium - AgeOfVehicleAmount;
            }
            else if (AgeOfLicense > 5 && AgeOfLicense < 10)
            {
                AgeOfVehicleAmount = baseRate * ageOfVehicle5to10Loading / 100;
                calcuateRatingListPremium = calcuateRatingListPremium + AgeOfVehicleAmount;
            }
            else if (AgeOfLicense > 10)
            {
                // discount
                AgeOfVehicleAmount = baseRate * ageOfVehileAbove10Loading / 100;
                calcuateRatingListPremium = calcuateRatingListPremium + AgeOfVehicleAmount;
            }

            return calcuateRatingListPremium;
        }







        //public QuoteLogic CalculatePremium(int vehicleUsageId, decimal sumInsured, eCoverType coverType, eExcessType excessType, decimal excess, int PaymentTermid, decimal? AddThirdPartyAmount, int NumberofPersons, Boolean Addthirdparty, Boolean PassengerAccidentCover, Boolean ExcessBuyBack, Boolean RoadsideAssistance, Boolean MedicalExpenses)
        //{
        //    var vehicleUsage = InsuranceContext.VehicleUsages.Single(vehicleUsageId);
        //    var Setting = InsuranceContext.Settings.All();
        //    var additionalchargeatp = 0.0m;
        //    var additionalchargepac = 0.0m;
        //    var additionalchargeebb = 0.0m;
        //    var additionalchargersa = 0.0m;
        //    var additionalchargeme = 0.0m;
        //    float? InsuranceRate = 0;
        //    decimal? InsuranceMinAmount = 0;
        //    if (coverType == eCoverType.Comprehensive)
        //    {
        //        InsuranceRate = vehicleUsage.ComprehensiveRate;
        //        InsuranceMinAmount = vehicleUsage.MinCompAmount;
        //    }
        //    else if (coverType == eCoverType.ThirdParty)
        //    {
        //        InsuranceRate = (float)vehicleUsage.AnnualTPAmount;
        //        InsuranceMinAmount = vehicleUsage.MinThirdAmount;
        //    }
        //    if (excessType == eExcessType.Percentage && excess > 0)
        //    {
        //        InsuranceRate = InsuranceRate + float.Parse(excess.ToString());
        //    }

        //    var premium = 0.00m;

        //    if (coverType == eCoverType.ThirdParty)
        //    {
        //        premium = (decimal)InsuranceRate;
        //    }
        //    else
        //    {
        //        premium = (sumInsured * Convert.ToDecimal(InsuranceRate)) / 100;
        //    }



        //    if (Addthirdparty)
        //    {
        //        var AddThirdPartyAmountADD = AddThirdPartyAmount;

        //        if (AddThirdPartyAmountADD > 10000)
        //        {
        //            var settingAddThirdparty = Convert.ToDecimal(Setting.Where(x => x.key == "Addthirdparty").Select(x => x.value).FirstOrDefault());
        //            var Amount = AddThirdPartyAmountADD - 10000;
        //            premium += Convert.ToDecimal((Amount * settingAddThirdparty) / 100);

        //        }
        //    }
        //    if (PassengerAccidentCover)
        //    {
        //        int additionalAmountPerPerson = Convert.ToInt32(Setting.Where(x => x.key == "PassengerAccidentCover").Select(x => x.value).FirstOrDefault());

        //        int totalAdditionalPACcharge = NumberofPersons * additionalAmountPerPerson;

        //        additionalchargepac = totalAdditionalPACcharge;

        //    }
        //    if (ExcessBuyBack)
        //    {

        //        int additionalAmountExcessBuyBack = Convert.ToInt32(Setting.Where(x => x.key == "ExcessBuyBack").Select(x => x.value).FirstOrDefault());


        //        additionalchargeebb = (premium * additionalAmountExcessBuyBack) / 100;


        //    }
        //    if (RoadsideAssistance)
        //    {
        //        decimal additionalAmountRoadsideAssistance = Convert.ToDecimal(Setting.Where(x => x.key == "RoadsideAssistance").Select(x => x.value).FirstOrDefault());


        //        additionalchargersa = (premium * additionalAmountRoadsideAssistance) / 100;


        //    }
        //    if (MedicalExpenses)
        //    {

        //        decimal additionalAmountMedicalExpenses = Convert.ToDecimal(Setting.Where(x => x.key == "MedicalExpenses").Select(x => x.value).FirstOrDefault());


        //        additionalchargeme = (premium * additionalAmountMedicalExpenses) / 100;

        //    }




        //    //if (sumInsured > 10000)
        //    //{
        //    //    var extraamount = sumInsured - 10000m;
        //    //    var additionalcharge = ((0.5 * (double)extraamount) / 100);
        //    //    premium = premium + (decimal)additionalcharge;
        //    //}
        //    if (premium < InsuranceMinAmount && coverType == eCoverType.Comprehensive)
        //    {
        //        Status = false;
        //        //premium = premium + InsuranceMinAmount.Value;
        //        premium = InsuranceMinAmount.Value;
        //        this.Message = "Insurance minimum amount $" + InsuranceMinAmount + " Charge is applicable.";
        //    }

        //    //if (premium < InsuranceMinAmount)
        //    //{
        //    //    Status = false;
        //    //    //premium = premium + InsuranceMinAmount.Value;
        //    //    premium = InsuranceMinAmount.Value;
        //    //    this.Message = "Insurance minimum amount $" + InsuranceMinAmount + " Charge is applicable.";
        //    //}



        //    if (excessType == eExcessType.FixedAmount && excess > 0)
        //    {
        //        premium = premium + excess;
        //    }

        //    this.Premium = premium;
        //    var stampDuty = (premium * 5) / 100;
        //    if (stampDuty > 2000000)
        //    {

        //        Status = false;
        //        this.Message = "Stamp Duty must not exceed $2,000,000";
        //    }

        //    var ztscLevy = (premium * 12) / 100;
        //    this.StamDuty = Math.Round(stampDuty, 2);
        //    this.ZtscLevy = Math.Round(ztscLevy, 2);



        //    premium = premium + stampDuty + ztscLevy ;

        //    switch (PaymentTermid)
        //    {
        //        case 3:
        //            premium = premium / 4;
        //            break;
        //        case 4:
        //            premium = premium / 3;
        //            break;
        //    }

        //    premium = premium + additionalchargeebb + additionalchargeme + additionalchargepac + additionalchargersa;

        //    this.Premium = Math.Round(premium, 2);





        //    return this;
        //}
    }


}
