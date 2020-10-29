using Insurance.Domain;
using InsuranceClaim.Models;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Insurance.Service
{
    public static class MiscellaneousService
    {

        public static void UpdateBalanceForVehicles(decimal amountPaid, int SummaryID, decimal totalPremium, bool isRenew, int renewVehicleID = 0)
        {
            List<SummaryVehicleDetail> _SummaryVehicleDetails = new List<SummaryVehicleDetail>();

            _SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={SummaryID}").ToList();

            if (amountPaid <= totalPremium)
            {
                if (isRenew)
                {

                }
                else
                {
                    var balanceFromAmountPaid = amountPaid;
                    var listVehicles = new List<VehicleDetail>();
                    foreach (var item in _SummaryVehicleDetails)
                    {
                        var vehicle = InsuranceContext.VehicleDetails.Single(where: $" Id='{item.VehicleDetailsId}' and IsActive<>0");
                        if (vehicle != null)
                        {
                            listVehicles.Add(vehicle);
                        }

                    }

                    if (listVehicles != null && listVehicles.Count > 0)
                    {
                        listVehicles = listVehicles.OrderBy(x => x.Premium).ToList();

                        foreach (var _item in listVehicles)
                        {

                            var vehicletotalPremium = _item.Premium + _item.StampDuty + _item.ZTSCLevy + (Convert.ToBoolean(_item.IncludeRadioLicenseCost) ? _item.RadioLicenseCost : 0.00m);
                            if (balanceFromAmountPaid > 0.00m)
                            {
                                if (balanceFromAmountPaid >= vehicletotalPremium)
                                {
                                    balanceFromAmountPaid = Convert.ToDecimal(balanceFromAmountPaid - vehicletotalPremium);
                                    _item.BalanceAmount = 0.00m;
                                }
                                else
                                {
                                    _item.BalanceAmount = vehicletotalPremium - balanceFromAmountPaid;
                                    balanceFromAmountPaid = 0.00m;
                                }
                            }
                            else
                            {
                                _item.BalanceAmount = vehicletotalPremium;
                            }

                            InsuranceContext.VehicleDetails.Update(_item);
                        }
                    }

                }
            }
        }

        public static string EmailPdf(string MotorBody, int custid, string policynumber, string filename, int vehcleId = 0)
        {
            StringReader sr = new StringReader(MotorBody.ToString());
            string path = "";
            try
            {

                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                string vehiclefolderpath = "";


                //  filename = Guid.NewGuid() + "," + filename;
                // string file = Convert.ToString(DateTime.Now.ToString("ddMMyyyy"));
                string file = Convert.ToString(DateTime.Now.ToString("yyyyMMddHHmmss"));

                filename = file + "," + filename;
                //  string[] nfilename=filename.Split(",");
                //  filename = DateTime.Now.ToString("dd/MM/yyyy") + "" + filename;
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, memoryStream);
                    pdfDoc.Open();
                    htmlparser.Parse(sr);
                    pdfDoc.Close();
                    byte[] bytes = memoryStream.ToArray();
                    memoryStream.Close();

                    string custfolderpath = HttpContext.Current.Server.MapPath("~/Documents/" + custid + "/");
                    string policyfolderpath = HttpContext.Current.Server.MapPath("~/Documents/" + custid + "/" + policynumber + "/");
                    // string Backuppath = HttpContext.Current.Server.MapPath("~/F:/PolicyDetails/" + custid + "/" + policynumber + "/");

                    if (vehcleId > 0)
                    {
                        vehiclefolderpath = HttpContext.Current.Server.MapPath("~/Documents/" + custid + "/" + policynumber + "/" + vehcleId + "/");
                    }


                    if (!Directory.Exists(custfolderpath))
                    {
                        Directory.CreateDirectory(custfolderpath);
                        Directory.CreateDirectory(policyfolderpath);
                        // Directory.CreateDirectory(Backuppath);
                    }
                    else
                    {
                        if (!Directory.Exists(policyfolderpath))
                        {
                            Directory.CreateDirectory(policyfolderpath);
                            if (vehcleId > 0)
                            {
                                Directory.CreateDirectory(vehiclefolderpath);
                                //     Directory.CreateDirectory(Backuppath);
                            }


                        }
                        else
                        {
                            if (vehcleId > 0)
                            {
                                if (!Directory.Exists(vehiclefolderpath))
                                {
                                    Directory.CreateDirectory(vehiclefolderpath);
                                    //       Directory.CreateDirectory(Backuppath);
                                }
                            }

                        }

                    }
                    if (vehcleId > 0)
                    {

                        System.IO.File.WriteAllBytes(vehiclefolderpath + filename + ".pdf", memoryStream.ToArray());
                        //    System.IO.File.WriteAllBytes(Backuppath + filename + ".pdf", memoryStream.ToArray());
                        path = "~/Documents/" + custid + "/" + policynumber + "/" + vehcleId + "/" + filename + ".pdf";

                    }
                    else
                    {
                        System.IO.File.WriteAllBytes(policyfolderpath + filename + ".pdf", memoryStream.ToArray());
                        //    System.IO.File.WriteAllBytes(Backuppath + filename + ".pdf", memoryStream.ToArray());
                        //    path = "http://" + HttpContext.Current.Request.Url.Authority + "/" + "~/Documents/" + custid + "/" + policynumber + "/" + filename + ".pdf";
                        path = "~/Documents/" + custid + "/" + policynumber + "/" + filename + ".pdf";
                    }
                }
            }
            catch (Exception ex)
            {
                Insurance.Service.EmailService service1 = new Insurance.Service.EmailService();
                service1.WriteLog("email pdf: "+ ex.Message);
            }

            sr.Close();
            return path;
        }

        public static string LicensePdf(string body, string vehicleId)
        {
            StringReader sr = new StringReader(body.ToString());
            string path = "";

            try
            {
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                HTMLWorker htmlparser = new HTMLWorker(pdfDoc);
                string vehiclefolderpath = "";

                vehiclefolderpath = HttpContext.Current.Server.MapPath("~/Documents/License/");
                if (!Directory.Exists(vehiclefolderpath))
                {
                    Directory.CreateDirectory(vehiclefolderpath);
                }

                string filename = vehicleId;

                byte[] bytes = Convert.FromBase64String(body);

                System.IO.File.WriteAllBytes(vehiclefolderpath + filename + ".pdf", bytes);
                // path = vehiclefolderpath + filename + ".pdf";
                path = "/Documents/License/" + filename + ".pdf";

            }
            catch (Exception ex)
            {

            }

            sr.Close();
            return path;
        }

        //public void ConvertBase64ToPdf(string body)
        //{


        //    byte[] bytes = Convert.FromBase64String(body);
        //    File.WriteAllBytes(@"FolderPath\pdfFileName.pdf", bytes);
        //}




        public static string GetCustomerNamebyID(int id)
        {
            var list = InsuranceContext.Customers.Single(id);
            if (list != null)
            {
                return list.FirstName + " " + list.LastName;
            }
            return "";

        }

        public static string GetPaymentMethodNamebyID(int id)
        {
            var list = InsuranceContext.PaymentMethods.Single(id);
            if (list != null)
            {
                return list.Name;
            }
            return "";

        }

        public static string GetCoverTypeNamebyID(int id)
        {
            var list = InsuranceContext.CoverTypes.Single(id);
            if (list != null)
            {
                return list.Name;
            }
            return "";

        }

        public static string GetMakeNamebyMakeCode(string code)
        {
            var list = InsuranceContext.VehicleMakes.Single(where: $"MakeCode='{code}'");
            if (list != null)
            {
                return list.MakeDescription;
            }
            return "";

        }

        public static string GetModelNamebyModelCode(string code)
        {
            var list = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{code}'");
            if (list != null)
            {
                return list.ModelDescription;
            }
            return "";

        }

        public static string GetReinsuranceBrokerNamebybrokerid(int id)
        {
            var list = InsuranceContext.ReinsuranceBrokers.Single(id);
            if (list != null)
            {
                return list.ReinsuranceBrokerName;
            }
            return "";

        }

        public static string AddLoyaltyPoints(int CustomerId, int PolicyId, RiskDetailModel vehicle, string email = "", string filepath = "")
        {
            string CurrencyName = "";
            var loaltyPointsSettings = InsuranceContext.Settings.Single(where: $"keyname='Points On Renewal'");
            var loyaltyPoint = 0.00m;
            switch (vehicle.PaymentTermId)
            {
                case 1:
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        loyaltyPoint = ((Convert.ToDecimal(vehicle.AnnualRiskPremium) * Convert.ToDecimal(loaltyPointsSettings.value)) / 100);
                    }
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        loyaltyPoint = Convert.ToDecimal(loaltyPointsSettings.value);
                    }
                    break;
                case 3:
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        loyaltyPoint = ((Convert.ToDecimal(vehicle.QuaterlyRiskPremium) * Convert.ToDecimal(loaltyPointsSettings.value)) / 100);
                    }
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        loyaltyPoint = Convert.ToDecimal(loaltyPointsSettings.value);
                    }
                    break;
                case 4:
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        loyaltyPoint = ((Convert.ToDecimal(vehicle.TermlyRiskPremium) * Convert.ToDecimal(loaltyPointsSettings.value)) / 100);
                    }
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        loyaltyPoint = Convert.ToDecimal(loaltyPointsSettings.value);
                    }
                    break;
            }

            LoyaltyDetail objLoyaltydetails = new LoyaltyDetail();
            objLoyaltydetails.CustomerId = CustomerId;
            objLoyaltydetails.IsActive = true;
            objLoyaltydetails.PolicyId = PolicyId;
            objLoyaltydetails.PointsEarned = loyaltyPoint;
            objLoyaltydetails.CreatedBy = CustomerId;
            objLoyaltydetails.CreatedOn = DateTime.Now;

            InsuranceContext.LoyaltyDetails.Insert(objLoyaltydetails);

            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            //08 May D

            SummaryDetailService detailService = new SummaryDetailService();
            var currencylist = detailService.GetAllCurrency();
            CurrencyName = detailService.GetCurrencyName(currencylist, vehicle.CurrencyId);

            var policy = InsuranceContext.PolicyDetails.Single(PolicyId);
            var customer = InsuranceContext.Customers.Single(CustomerId);




            var TotalLoyaltyPoints = InsuranceContext.LoyaltyDetails.All(where: $"CustomerId={CustomerId}").Sum(x => x.PointsEarned);
            string ReminderEmailPath = "/Views/Shared/EmaiTemplates/LoyaltyPoints.cshtml";
            string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(ReminderEmailPath));
            var body = EmailBody2.Replace("##FirstName##", customer.FirstName).Replace("##path##", filepath).Replace("##LastName##", customer.LastName)
                 .Replace("##currencyName##", CurrencyName)
                .Replace("##CreditedWalletAmount##", Convert.ToString(loyaltyPoint)).Replace("##TotalWalletBalance##", Convert.ToString(TotalLoyaltyPoints));
            // var yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
            var attacheMentPath = MiscellaneousService.EmailPdf(body, policy.CustomerId, policy.PolicyNumber, "LoyaltyPoints");

            List<string> attachements = new List<string>();
            attachements.Add(attacheMentPath);
            //if (!userLoggedin)
            //{
            //    attachements.Add(yAtter);
            //    objEmailService.SendEmail(email, "", "", "Loyalty Reward | Points Credited to your Wallet", body, attachements);

            //}

            objEmailService.SendEmail(email, "", "", "Loyalty Reward | Points Credited to your Wallet", body, attachements);

            return "";
        }


        public static string AddAgentLoyaltyPoints(int CustomerId, int PolicyId, RiskDetailModel vehicle, string email = "", string filepath = "", Customer agentDetials = null, AgentLogo agentLogo = null, string agentEmail = "")
        {
            string CurrencyName = "";
            var loaltyPointsSettings = InsuranceContext.Settings.Single(where: $"keyname='Points On Renewal'");
            var loyaltyPoint = 0.00m;
            switch (vehicle.PaymentTermId)
            {
                case 1:
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        loyaltyPoint = ((Convert.ToDecimal(vehicle.AnnualRiskPremium) * Convert.ToDecimal(loaltyPointsSettings.value)) / 100);
                    }
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        loyaltyPoint = Convert.ToDecimal(loaltyPointsSettings.value);
                    }
                    break;
                case 3:
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        loyaltyPoint = ((Convert.ToDecimal(vehicle.QuaterlyRiskPremium) * Convert.ToDecimal(loaltyPointsSettings.value)) / 100);
                    }
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        loyaltyPoint = Convert.ToDecimal(loaltyPointsSettings.value);
                    }
                    break;
                case 4:
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                    {
                        loyaltyPoint = ((Convert.ToDecimal(vehicle.TermlyRiskPremium) * Convert.ToDecimal(loaltyPointsSettings.value)) / 100);
                    }
                    if (loaltyPointsSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                    {
                        loyaltyPoint = Convert.ToDecimal(loaltyPointsSettings.value);
                    }
                    break;
            }

            LoyaltyDetail objLoyaltydetails = new LoyaltyDetail();
            objLoyaltydetails.CustomerId = CustomerId;
            objLoyaltydetails.IsActive = true;
            objLoyaltydetails.PolicyId = PolicyId;
            objLoyaltydetails.PointsEarned = loyaltyPoint;
            objLoyaltydetails.CreatedBy = CustomerId;
            objLoyaltydetails.CreatedOn = DateTime.Now;

            InsuranceContext.LoyaltyDetails.Insert(objLoyaltydetails);

            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            //08 May D

            SummaryDetailService detailService = new SummaryDetailService();
            var currencylist = detailService.GetAllCurrency();
            CurrencyName = detailService.GetCurrencyName(currencylist, vehicle.CurrencyId);

            var policy = InsuranceContext.PolicyDetails.Single(PolicyId);
            var customer = InsuranceContext.Customers.Single(CustomerId);




            var TotalLoyaltyPoints = InsuranceContext.LoyaltyDetails.All(where: $"CustomerId={CustomerId}").Sum(x => x.PointsEarned);
            string ReminderEmailPath = "/Views/Shared/EmaiTemplates/AgentLoyalityPoints.cshtml";
            string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(ReminderEmailPath));
            var body = EmailBody2.Replace("##FirstName##", customer.FirstName).Replace("##path##", filepath + agentLogo.LogoPath).Replace("##LastName##", customer.LastName)
                 .Replace("##currencyName##", CurrencyName)
                   .Replace("#AgentFirstName#", agentDetials.FirstName).Replace("#AgentLastName#", agentDetials.LastName)
                 .Replace("#AgentAddress1#", agentDetials.AddressLine1).Replace("#AgentCity#", agentDetials.City)
                  .Replace("#AgentPhone#", agentDetials.PhoneNumber).Replace("#AgentWhatsapp#", agentDetials.AgentWhatsapp)
                  .Replace("#AgentEmail#", agentEmail)
                .Replace("##CreditedWalletAmount##", Convert.ToString(loyaltyPoint)).Replace("##TotalWalletBalance##", Convert.ToString(TotalLoyaltyPoints));
            // var yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
            var attacheMentPath = MiscellaneousService.EmailPdf(body, policy.CustomerId, policy.PolicyNumber, "LoyaltyPoints");

            List<string> attachements = new List<string>();
            attachements.Add(attacheMentPath);
            //if (!userLoggedin)
            //{
            //    attachements.Add(yAtter);
            //    objEmailService.SendEmail(email, "", "", "Loyalty Reward | Points Credited to your Wallet", body, attachements);

            //}

            objEmailService.SendEmail(email, "", "", "Loyalty Reward | Points Credited to your Wallet", body, attachements);


            return "";
        }

        public static void SendEmailNewPolicy(string customerName, string address1, string address2, string policyNumber, decimal? premium, int? paymentTermId, string paymentMethod, List<VehicleDetail> vehicle, string renew = "")
        {
            string paymentTerm = GetPaymentTerm(paymentTermId);
            string subject = "";
            if (renew != "")
                subject = "Renew Policy-" + policyNumber;
            else
                subject = "New Policy-" + policyNumber;
            // string paymentMethod = GetPaymentType(paymentMethodId);
            string vrn = "";
            if (vehicle != null)
                vrn = vehicle[0].RegistrationNo;
            if (vehicle.Count > 1)
                vrn += "," + vehicle[0].RegistrationNo;

            string QuotationEmailPath = "/Views/Shared/EmaiTemplates/NewPolicy.cshtml";
            string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(QuotationEmailPath));

            var body = MotorBody.Replace("#CustomerName#", customerName).Replace("#PolicyNumber#", policyNumber)
                .Replace("#Address1#", address1)
                .Replace("#Address2#", address2)
                .Replace("#Premium#", premium.ToString()).Replace("#PaymentTerm#", paymentTerm).Replace("#PaymentMethod#", paymentMethod).Replace("#vrn#", vrn);

            //webclientsemail
            var webclientsemail = System.Configuration.ConfigurationManager.AppSettings["webclientsemail"];
            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
            objEmailService.SendEmail(webclientsemail, "", "", subject, body, new List<string>());

        }

        public static string GetPaymentTerm(int? paymentTermId)
        {
            var paymentTermVehicel = InsuranceContext.PaymentTerms.Single(paymentTermId);
            string paymentTerm = "";

            if (paymentTermVehicel != null)
            {
                if (paymentTermVehicel.Id == 1)
                    paymentTerm = "Annual";
                else if (paymentTermVehicel.Id == 4)
                    paymentTerm = "Termly";
                else
                    paymentTerm = paymentTermVehicel.Name + " Months";
            }


            return paymentTerm;
        }

    }
}
