using AutoMapper;
using Insurance.Domain;
using Insurance.Service;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using static InsuranceClaim.Controllers.CustomerRegistrationController;

namespace InsuranceClaim.Controllers
{
    public class WebCustomerController : Controller
    {

        private ApplicationUserManager _userManager;
        string AdminEmail = WebConfigurationManager.AppSettings["AdminEmail"];
        string ZimnatEmail = WebConfigurationManager.AppSettings["ZimnatEmail"];

        decimal _InflationFactorAmt = 25;

        public WebCustomerController()
        {
            // UserManager = userManager;


        }
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: WebCustomer
        public ActionResult Index(string reg = "", string NationalId = "")
        {
            WebCustomerModel model = new WebCustomerModel();
            model.Customer = new CustomerModel();
            model.RiskDetail = new RiskDetailModel();
            model.SummaryDetail = new SummaryDetailModel();

            ViewBag.PaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is null").ToList();
            ViewBag.TaxClass = InsuranceContext.VehicleTaxClasses.All().ToList();
            ViewBag.Products = InsuranceContext.Products.All(where: "Active = 'True' or Active is null").ToList();
            ViewBag.Cities = InsuranceContext.Cities.All();

            // ViewBag.Currencies = InsuranceContext.Currencies.All(where: $"IsActive = 'True'  and Id<>1 "); 
            model.RiskDetail.CurrencyId = 6;
            var service = new VehicleService();
            var makers = service.GetMakers();
            ViewBag.Makers = makers;
            ViewBag.VehicleUsage = service.GetAllVehicleUsage();

            if (makers.Count > 0)
            {
                var modelList = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = modelList;
            }

            model.Customer.NationalIdentificationNumber = NationalId;
            model.RiskDetail.RegistrationNo = reg;


            //if (Session["HomeNationalId"] != null)
            //{
            //    model.Customer.NationalIdentificationNumber = (string)Session["HomeNationalId"];
            //    Session["HomeNationalId"] = null;
            //}

            //if (Session["HomeRegNo"] != null)
            //{
            //    model.RiskDetail.RegistrationNo = (string)Session["HomeRegNo"];
            //    Session["HomeRegNo"] = null;
            //}

            return View(model);
        }

        [HttpPost]
        public JsonResult getPolicyDetailsFromICEcash(string regNo, string PaymentTerm, string SumInsured, int CoverTypeId, int VehicleType, bool VehilceLicense, bool RadioLicense, string firstName, string lastName, string email, string address, string phone, string nationalId, string radioLicensePaymentTerm, string zinaraLicensePaymentTerm, int vehilceUsage)
        {
            checkVRNwithICEcashResponse response = new checkVRNwithICEcashResponse();
            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //json.Data = "";

            try
            {
                Insurance.Service.ICEcashService ICEcashService = new Insurance.Service.ICEcashService();
                var tokenObject = new ICEcashTokenResponse();
                #region get ICEcash token
                string patnerToken = SummaryDetailService.GetLatestToken();

                if (patnerToken == "")
                {
                    tokenObject = ICEcashService.getToken();
                    SummaryDetailService.UpdateToken(tokenObject);
                }

                CustomerModel model = new CustomerModel
                {
                    FirstName = firstName,
                    LastName = lastName,
                    EmailAddress = email,
                    PhoneNumber = phone,
                    AddressLine1 = address,
                    NationalIdentificationNumber = nationalId,
                    AddressLine2 = ""
                };
                Session["CustomerDataModal"] = model;



                #endregion

                List<RiskDetailModel> objVehicles = new List<RiskDetailModel>();
                //objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo });
                objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo, PaymentTermId = Convert.ToInt32(PaymentTerm) });

                if (patnerToken != "")
                {
                    DateTime Cover_StartDate = DateTime.Now;
                    int numOfMonth = 12;
                    if (PaymentTerm == "1")
                        numOfMonth = 12;
                    else
                        numOfMonth = Convert.ToInt32(PaymentTerm);
                    DateTime Cover_EndDate = DateTime.Now.AddMonths(numOfMonth);


                    ResultRootObject quoteresponse = new ResultRootObject();

                    //  ResultRootObject quoteresponse = ICEcashService.RequestQuote(tokenObject.Response.PartnerToken, regNo, SumInsured, make, model, Convert.ToInt32(PaymentTerm), Convert.ToInt32(VehicleYear), CoverTypeId, VehicleUsage, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate);

                    VehicleService vehicleSerive = new VehicleService();
                    //var product = vehicleSerive.GetVehicleTypeByProductId(VehicleType);
                    //var tempVehicleType = VehicleType;
                    //if (product != null)
                    //    tempVehicleType = product.VehicleTypeId;

                    // quoteresponse = ICEcashService.TPILICQuoteWebUser(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, vehilceUsage, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1", VehilceLicense, RadioLicense, radioLicensePaymentTerm, zinaraLicensePaymentTerm);


                    //if (VehilceLicense)
                    //    quoteresponse = ICEcashService.TPILICQuoteWebUser(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, vehilceUsage, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1", VehilceLicense, RadioLicense, radioLicensePaymentTerm, zinaraLicensePaymentTerm);
                    //else
                    //    quoteresponse = ICEcashService.RequestQuote(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, vehilceUsage, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1");


                    if (VehilceLicense && RadioLicense)
                        quoteresponse = ICEcashService.TPILICQuote(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, vehilceUsage, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1", VehilceLicense, RadioLicense, zinaraLicensePaymentTerm, radioLicensePaymentTerm);
                    else if (VehilceLicense)
                        quoteresponse = ICEcashService.TPILICQuoteZinaraOnly(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, vehilceUsage, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1", VehilceLicense, RadioLicense, zinaraLicensePaymentTerm);
                    else
                        quoteresponse = ICEcashService.RequestQuote(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, vehilceUsage, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1");






                    // Invalid Partner Token. 

                    if (quoteresponse.Response != null && (quoteresponse.Response.Message.Contains("Partner Token has expired") || quoteresponse.Response.Message.Contains("Invalid Partner Token")))
                    {
                        tokenObject = ICEcashService.getToken();
                        SummaryDetailService.UpdateToken(tokenObject);

                        patnerToken = tokenObject.Response.PartnerToken;
                        //   tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
                        //tokenObject = service.CheckSessionExpired();
                        if (VehilceLicense && RadioLicense)
                            quoteresponse = ICEcashService.TPILICQuote(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, vehilceUsage, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1", VehilceLicense, RadioLicense, zinaraLicensePaymentTerm, radioLicensePaymentTerm);
                        else if (VehilceLicense)
                            quoteresponse = ICEcashService.TPILICQuoteZinaraOnly(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, vehilceUsage, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1", VehilceLicense, RadioLicense, zinaraLicensePaymentTerm);
                        else
                            quoteresponse = ICEcashService.RequestQuote(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, vehilceUsage, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1");


                    }

                    response.result = quoteresponse.Response.Result;
                    if (response.result == 0)
                    {
                        response.message = quoteresponse.Response.Quotes[0].Message;
                    }
                    else
                    {
                        response.Data = quoteresponse;
                    }

                    response.result = quoteresponse.Response.Result;
                }
                Session["CustomerDataModal"] = null;
                json.Data = response;

            }
            catch (Exception ex)
            {
                response.message = "Error occured.";

                json.Data = new ResultResponse();

            }
            return json;
        }


        [HttpPost]
        public async Task<ActionResult> SubmitPlan(WebCustomerModel model)
        {
            SummaryDetailService servicedetail = new SummaryDetailService();

            Insurance.Service.ICEcashService ICEcashService = new Insurance.Service.ICEcashService();
            List<RiskDetailModel> list = new List<RiskDetailModel>();
            string PartnerToken = "";




            #region Add All info to database

            //var vehicle = (RiskDetailModel)Session["VehicleDetail"];
            Session["SummaryDetailed"] = model;
            string SummeryofReinsurance = "";
            string SummeryofVehicleInsured = "";
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            var customer = model.Customer;

            // defualt value for customer
            customer.Zipcode = "00263";
            customer.Country = "+263";

            var userDetial = UserManager.FindByEmail(customer.EmailAddress);

            if (userDetial == null)
            {
                if (customer != null)
                {
                    decimal custId = 0;
                    var user = new ApplicationUser { UserName = customer.EmailAddress, Email = customer.EmailAddress, PhoneNumber = customer.PhoneNumber };
                    var result = await UserManager.CreateAsync(user, "Geninsure@123");
                    SaveUserPasswordDetails(user);
                    if (result.Succeeded)
                    {
                        var roleresult = UserManager.AddToRole(user.Id, "Web Customer"); // for web user
                        var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();
                        if (objCustomer != null)
                            custId = objCustomer.CustomerId + 1;
                        else
                            custId = Convert.ToDecimal(ConfigurationManager.AppSettings["CustomerId"]);

                        customer.UserID = user.Id;
                        customer.CustomerId = custId;
                        customer.CountryCode = "+263";
                        var customerdata = Mapper.Map<CustomerModel, Customer>(customer);
                        InsuranceContext.Customers.Insert(customerdata);
                        customer.Id = customerdata.Id;
                    }
                }
            }
            else
            {
                var customerDetails = InsuranceContext.Customers.Single(where: "UserID='" + userDetial + "'");
                customerDetails.FirstName = customer.FirstName;
                customerDetails.LastName = customer.LastName;
                customerDetails.PhoneNumber = customer.PhoneNumber;
                customerDetails.AddressLine1 = customerDetails.AddressLine1;
                InsuranceContext.Customers.Update(customerDetails); // 13_june_2019
                customer.Id = customerDetails.Id;
            }



            // get policy details
            var policy = ProductDetail();
            if (policy != null)
            {
                policy.CustomerId = customer.Id;
                policy.StartDate = null;
                policy.EndDate = null;
                policy.TransactionDate = null;
                policy.RenewalDate = null;
                policy.RenewalDate = null;
                policy.StartDate = null;
                policy.TransactionDate = null;
                policy.CreatedBy = customer.Id;
                policy.CreatedOn = DateTime.Now;
                InsuranceContext.PolicyDetails.Insert(policy);
            }

            var Id = 0;
            var listReinsuranceTransaction = new List<ReinsuranceTransaction>();

            // defualt value for vehicle
            model.RiskDetail.CurrencyId = 6; //default "RTGS$" select
            model.RiskDetail.NoOfCarsCovered = 1;
            model.RiskDetail.AgentCommissionId = 1;
            

            List<RiskDetailModel> vehilceList = new List<RiskDetailModel>();
            vehilceList.Add(model.RiskDetail);

            var vehicle = vehilceList;
            string format = "yyyyMMdd";
            int vehicleId = 0;

            if (vehicle != null && vehicle.Count > 0)
            {
                foreach (var item in vehicle.ToList())
                {
                    if (!string.IsNullOrEmpty(item.LicExpiryDate))
                    {
                        var LicExpiryDate = DateTime.ParseExact(item.LicExpiryDate, format, CultureInfo.InvariantCulture);
                        item.LicExpiryDate = LicExpiryDate.ToShortDateString();
                        if (item.VehicleLicenceFee > 0)
                            item.IceCashRequest = "InsuranceAndLicense";
                    }
                    else
                        item.IceCashRequest = "Insurance";

                    if (item.RadioLicenseCost > 0)  // for now 
                        item.IncludeRadioLicenseCost = true;

                    if(!string.IsNullOrEmpty(item.LicenseDeliveryWay))
                        item.IsLicenseDiskNeeded = true;
                    
                    

                    var _item = item;

                    if (_item.Id == 0)
                    {
                        var service = new RiskDetailService();
                        _item.CustomerId = customer.Id;
                        _item.PolicyId = policy.Id;
                        _item.CoverStartDate = DateTime.Now;
                        _item.Id = service.AddVehicleInformation(_item);
                        vehicleId = _item.Id;

                        // Delivery Address Save
                        var LicenseAddress = new LicenceDiskDeliveryAddress();
                        LicenseAddress.Address1 = _item.LicenseAddress1;
                        LicenseAddress.Address2 = _item.LicenseAddress2;
                        LicenseAddress.City = _item.LicenseCity;
                        LicenseAddress.VehicleId = _item.Id;
                        LicenseAddress.CreatedBy = customer.Id;
                        LicenseAddress.CreatedOn = DateTime.Now;
                        LicenseAddress.ModifiedBy = customer.Id;
                        LicenseAddress.ModifiedOn = DateTime.Now;

                        InsuranceContext.LicenceDiskDeliveryAddresses.Insert(LicenseAddress);

                        ///Licence Ticket
                        if (_item.IsLicenseDiskNeeded)
                        {
                            var LicenceTicket = new LicenceTicket();
                            var Licence = InsuranceContext.LicenceTickets.All(orderBy: "Id desc").FirstOrDefault();
                            if (Licence != null)
                            {
                                string number = Licence.TicketNo.Substring(3);

                                long tNumber = Convert.ToInt64(number) + 1;
                                string TicketNo = string.Empty;
                                int length = 6;
                                length = length - tNumber.ToString().Length;

                                for (int i = 0; i < length; i++)
                                {
                                    TicketNo += "0";
                                }
                                TicketNo += tNumber;
                                var ticketnumber = "GEN" + TicketNo;

                                LicenceTicket.TicketNo = ticketnumber;
                            }
                            else
                            {
                                var TicketNo = ConfigurationManager.AppSettings["TicketNo"];
                                LicenceTicket.TicketNo = TicketNo;
                            }

                            LicenceTicket.VehicleId = _item.Id;
                            LicenceTicket.CloseComments = "";
                            LicenceTicket.ReopenComments = "";
                            LicenceTicket.DeliveredTo = "";
                            LicenceTicket.CreatedDate = DateTime.Now;
                            LicenceTicket.CreatedBy = customer.Id;
                            LicenceTicket.IsClosed = false;
                            LicenceTicket.PolicyNumber = policy.PolicyNumber;
                            InsuranceContext.LicenceTickets.Insert(LicenceTicket);
                        }

                        ///Reinsurance                      

                        var ReinsuranceCases = InsuranceContext.Reinsurances.All(where: $"Type='Reinsurance'").ToList();
                        var ownRetention = InsuranceContext.Reinsurances.All().Where(x => x.TreatyCode == "OR001").Select(x => x.MaxTreatyCapacity).SingleOrDefault();
                        var ReinsuranceCase = new Reinsurance();

                        foreach (var Reinsurance in ReinsuranceCases)
                        {
                            if (Reinsurance.MinTreatyCapacity <= item.SumInsured && item.SumInsured <= Reinsurance.MaxTreatyCapacity)
                            {
                                ReinsuranceCase = Reinsurance;
                                break;
                            }
                        }

                        if (ReinsuranceCase != null && ReinsuranceCase.MaxTreatyCapacity != null)
                        {
                            var basicPremium = item.Premium;
                            var ReinsuranceBroker = InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{ReinsuranceCase.ReinsuranceBrokerCode}'");
                            var AutoFacSumInsured = 0.00m;
                            var AutoFacPremium = 0.00m;
                            var FacSumInsured = 0.00m;
                            var FacPremium = 0.00m;

                            if (ReinsuranceCase.MinTreatyCapacity > 200000)
                            {
                                var autofaccase = ReinsuranceCases.FirstOrDefault();
                                var autofacSumInsured = autofaccase.MaxTreatyCapacity - ownRetention;
                                var autofacReinsuranceBroker = InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{autofaccase.ReinsuranceBrokerCode}'");

                                var _reinsurance = new ReinsuranceTransaction();
                                _reinsurance.ReinsuranceAmount = autofacSumInsured;
                                AutoFacSumInsured = Convert.ToDecimal(_reinsurance.ReinsuranceAmount);
                                _reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((_reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                                AutoFacPremium = Convert.ToDecimal(_reinsurance.ReinsurancePremium);
                                _reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(autofacReinsuranceBroker.Commission);
                                _reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((_reinsurance.ReinsurancePremium * _reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                _reinsurance.VehicleId = item.Id;
                                _reinsurance.ReinsuranceBrokerId = autofacReinsuranceBroker.Id;
                                _reinsurance.TreatyName = autofaccase.TreatyName;
                                _reinsurance.TreatyCode = autofaccase.TreatyCode;
                                _reinsurance.CreatedOn = DateTime.Now;
                                _reinsurance.CreatedBy = customer.Id;

                                InsuranceContext.ReinsuranceTransactions.Insert(_reinsurance);

                                SummeryofReinsurance += "<tr><td>" + Convert.ToString(_reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(_reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(_reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                listReinsuranceTransaction.Add(_reinsurance);

                                var __reinsurance = new ReinsuranceTransaction();
                                __reinsurance.ReinsuranceAmount = _item.SumInsured - ownRetention - autofacSumInsured;
                                FacSumInsured = Convert.ToDecimal(__reinsurance.ReinsuranceAmount);
                                __reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((__reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                                FacPremium = Convert.ToDecimal(__reinsurance.ReinsurancePremium);
                                __reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                __reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((__reinsurance.ReinsurancePremium * __reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                __reinsurance.VehicleId = item.Id;
                                __reinsurance.ReinsuranceBrokerId = ReinsuranceBroker.Id;
                                __reinsurance.TreatyName = ReinsuranceCase.TreatyName;
                                __reinsurance.TreatyCode = ReinsuranceCase.TreatyCode;
                                __reinsurance.CreatedOn = DateTime.Now;
                                __reinsurance.CreatedBy = customer.Id;

                                InsuranceContext.ReinsuranceTransactions.Insert(__reinsurance);

                                //SummeryofReinsurance += "<tr><td>" + Convert.ToString(__reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(__reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(__reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                listReinsuranceTransaction.Add(__reinsurance);
                            }
                            else
                            {

                                var reinsurance = new ReinsuranceTransaction();
                                reinsurance.ReinsuranceAmount = _item.SumInsured - ownRetention;
                                AutoFacSumInsured = Convert.ToDecimal(reinsurance.ReinsuranceAmount);
                                reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((reinsurance.ReinsuranceAmount / item.SumInsured) * basicPremium), 2);
                                AutoFacPremium = Convert.ToDecimal(reinsurance.ReinsurancePremium);
                                reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((reinsurance.ReinsurancePremium * reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                reinsurance.VehicleId = item.Id;
                                reinsurance.ReinsuranceBrokerId = ReinsuranceBroker.Id;
                                reinsurance.TreatyName = ReinsuranceCase.TreatyName;
                                reinsurance.TreatyCode = ReinsuranceCase.TreatyCode;
                                reinsurance.CreatedOn = DateTime.Now;
                                reinsurance.CreatedBy = customer.Id;

                                InsuranceContext.ReinsuranceTransactions.Insert(reinsurance);

                                //SummeryofReinsurance += "<tr><td>" + Convert.ToString(reinsurance.Id) + "</td><td>" + ReinsuranceCase.TreatyCode + "</td><td>" + ReinsuranceCase.TreatyName + "</td><td>" + Convert.ToString(reinsurance.ReinsuranceAmount) + "</td><td>" + Convert.ToString(ReinsuranceBroker.ReinsuranceBrokerName) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(reinsurance.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(ReinsuranceBroker.Commission) + "</td></tr>";

                                listReinsuranceTransaction.Add(reinsurance);
                            }


                            Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                            VehicleModel vehiclemodel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                            VehicleMake vehiclemake = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'");

                            string vehicledescription = vehiclemodel.ModelDescription + " / " + vehiclemake.MakeDescription;

                            // SummeryofVehicleInsured += "<tr><td>" + vehicledescription + "</td><td>" + Convert.ToString(item.SumInsured) + "</td><td>" + Convert.ToString(item.Premium) + "</td><td>" + AutoFacSumInsured.ToString() + "</td><td>" + AutoFacPremium.ToString() + "</td><td>" + FacSumInsured.ToString() + "</td><td>" + FacPremium.ToString() + "</td></tr>";

                            SummeryofVehicleInsured += "<tr><td style='padding:7px 10px; font-size:14px'><font size='2'>" + vehicledescription + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + Convert.ToString(item.SumInsured) + " </font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + Convert.ToString(item.Premium) + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + AutoFacSumInsured.ToString() + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + AutoFacPremium.ToString() + "</ font ></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + FacSumInsured.ToString() + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + FacPremium.ToString() + "</font></td></tr>";

                        }

                    }

                }
            }

            // var summary = model.SummaryDetail;

            int? paymentMethod = model.SummaryDetail.PaymentMethodId;
            model.SummaryDetail = SetDataOnSummaryDetail(vehicle);
            model.SummaryDetail.InvoiceNumber = policy.PolicyNumber;
            model.SummaryDetail.PaymentMethodId = paymentMethod;

            var DbEntry = Mapper.Map<SummaryDetailModel, SummaryDetail>(model.SummaryDetail);

            if (model.SummaryDetail != null)
            {

                DbEntry.CustomerId = customer.Id;
                bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (_userLoggedin)
                {
                    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                    var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

                    if (_customerData != null)
                    {
                        DbEntry.CreatedBy = _customerData.Id;
                    }
                }


                DbEntry.CreatedOn = DateTime.Now;
                if (DbEntry.BalancePaidDate.Value.Year == 0001)
                {
                    DbEntry.BalancePaidDate = DateTime.Now;
                }
                if (DbEntry.Notes == null)
                {
                    DbEntry.Notes = "";
                }
                InsuranceContext.SummaryDetails.Insert(DbEntry);

            }



            if (DbEntry.Id > 0)
            {
                var SummaryDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={DbEntry.Id}").ToList();

                if (SummaryDetails != null && SummaryDetails.Count > 0)
                {
                    foreach (var item in SummaryDetails)
                    {
                        InsuranceContext.SummaryVehicleDetails.Delete(item);
                    }
                }


                var summarydetails = new SummaryVehicleDetail();
                summarydetails.SummaryDetailId = DbEntry.Id;
                summarydetails.VehicleDetailsId = vehicleId;
                summarydetails.CreatedBy = customer.Id;
                summarydetails.CreatedOn = DateTime.Now;
                InsuranceContext.SummaryVehicleDetails.Insert(summarydetails);

                MiscellaneousService.UpdateBalanceForVehicles(DbEntry.AmountPaid.Value, DbEntry.Id, Convert.ToDecimal(DbEntry.TotalPremium), false);
            }

            if (listReinsuranceTransaction != null && listReinsuranceTransaction.Count > 0)
            {
                string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                int _vehicleId = 0;
                int count = 0;
                bool MailSent = false;
                foreach (var item in listReinsuranceTransaction)
                {

                    count++;
                    if (_vehicleId == 0)
                    {
                        SummeryofReinsurance = "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                        _vehicleId = item.VehicleId;
                        MailSent = false;
                    }
                    else
                    {
                        if (_vehicleId == item.VehicleId)
                        {
                            SummeryofReinsurance += "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                            var user = UserManager.FindById(customer.UserID);
                            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                            var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == model.SummaryDetail.PaymentTermId);
                            string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/Reinsurance_Admin.cshtml";
                            string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                            var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", user.PhoneNumber)
                                .Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName)
                                .Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);

                            var attachementPath = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");


                            List<string> attachements = new List<string>();
                            attachements.Add(attachementPath);

                            objEmailService.SendEmail(ZimnatEmail, "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, attachements);
                            MailSent = true;
                        }
                        else
                        {
                            SummeryofReinsurance = "<tr><td>" + Convert.ToString(item.Id) + "</td><td>" + item.TreatyCode + "</td><td>" + item.TreatyName + "</td><td>" + Convert.ToString(item.ReinsuranceAmount) + "</td><td>" + MiscellaneousService.GetReinsuranceBrokerNamebybrokerid(item.ReinsuranceBrokerId) + "</td><td>" + Convert.ToString(Math.Round(Convert.ToDecimal(item.ReinsurancePremium), 2)) + "</td><td>" + Convert.ToString(item.ReinsuranceCommissionPercentage) + "%</td></tr>";
                            MailSent = false;
                        }
                        _vehicleId = item.VehicleId;
                    }


                    if (count == listReinsuranceTransaction.Count && !MailSent)
                    {
                        var user = UserManager.FindById(customer.UserID);
                        Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                        var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                        var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == model.SummaryDetail.PaymentTermId);
                        string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/Reinsurance_Admin.cshtml";
                        string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                        var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##paath##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);

                        var attacehMentFilePath = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case");

                        List<string> _attachements = new List<string>();
                        _attachements.Add(attacehMentFilePath);
                        objEmailService.SendEmail(ZimnatEmail, "", "", "Reinsurance Case: " + policy.PolicyNumber.ToString(), Body, _attachements);
                        //MiscellaneousService.ScheduleMotorPdf(Body, policy.CustomerId, policy.PolicyNumber, "Reinsurance Case- " + policy.PolicyNumber.ToString(), item.VehicleId);
                    }
                }
            }


            if (model.SummaryDetail.PaymentMethodId == (int)InsuranceClaim.Models.paymentMethod.PayNow)
            {
                CustomerRegistrationController customerController = new CustomerRegistrationController();


                Insurance.Service.EmailService log = new Insurance.Service.EmailService();
                var payNow = customerController.PayNow(DbEntry.Id, policy.PolicyNumber, model.SummaryDetail.PaymentMethodId.Value, Convert.ToDecimal(model.SummaryDetail.TotalPremium));
                if (payNow.IsSuccessPayment)
                {
                    Session["PollUrl"] = payNow.PollUrl;

                    Session["PayNowSummmaryId"] = DbEntry.Id;


                    return Redirect(payNow.ReturnUrl);
                }
                else
                {
                    return RedirectToAction("failed_url", "Paypal");
                }
            }
            else if (model.SummaryDetail.PaymentMethodId == (int)InsuranceClaim.Models.paymentMethod.ecocash)
            {
                return RedirectToAction("EcoCashPayment", "Paypal", new { id = DbEntry.Id, invoiceNumber = policy.PolicyNumber, Paymentid = model.SummaryDetail.PaymentMethodId.Value });
            }


            #endregion



            return View();
        }


        public SummaryDetailModel SetDataOnSummaryDetail(List<RiskDetailModel> vehicle)
        {

            SummaryDetailModel model = new SummaryDetailModel();

            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();

            model.CarInsuredCount = 1;
            model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());

            //default selection 
            
            model.ReceiptNumber = "";
            model.SMSConfirmation = false;
            //model.TotalPremium = vehicle.Sum(item => item.Premium + item.ZTSCLevy + item.StampDuty + item.RadioLicenseCost);
            model.TotalPremium = 0.00m;
            model.TotalRadioLicenseCost = 0.00m;
            model.Discount = 0.00m;
            foreach (var item in vehicle)
            {
                model.PaymentTermId = item.PaymentTermId;
                model.PaymentMethodId = item.PaymentMethodId;
                decimal penalitesAmt = Convert.ToDecimal(item.PenaltiesAmt);



                model.TotalPremium += Convert.ToDecimal(item.Premium) + Convert.ToDecimal(item.ZTSCLevy) + Convert.ToDecimal(item.StampDuty) + Convert.ToDecimal(item.VehicleLicenceFee) + Convert.ToDecimal(penalitesAmt);
                if (item.IncludeRadioLicenseCost)
                {
                    model.TotalPremium += Convert.ToDecimal(item.RadioLicenseCost);
                    model.TotalRadioLicenseCost += Convert.ToDecimal(item.RadioLicenseCost);
                }
                model.Discount += Convert.ToDecimal(item.Discount);


                //var currency = InsuranceContext.Currencies.Single(where: $" Id='{item.CurrencyId}' ");

                //if (currency != null)
                //    item. = currency.Name;
            }
            model.TotalRadioLicenseCost = Math.Round(Convert.ToDecimal(model.TotalRadioLicenseCost), 2);
            model.Discount = Math.Round(Convert.ToDecimal(model.Discount), 2);
            model.TotalPremium = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            model.TotalStampDuty = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.StampDuty)), 2);
            model.TotalSumInsured = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.SumInsured)), 2);
            model.TotalZTSCLevies = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ZTSCLevy)), 2);
            model.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ExcessBuyBackAmount)), 2);
            model.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.MedicalExpensesAmount)), 2);
            model.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.PassengerAccidentCoverAmount)), 2);
            model.RoadsideAssistanceAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.RoadsideAssistanceAmount)), 2);
            model.ExcessAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ExcessAmount)), 2);
            model.AmountPaid = 0.00m;
            model.MaxAmounttoPaid = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            var vehiclewithminpremium = vehicle.OrderBy(x => x.Premium).FirstOrDefault();

            if (vehiclewithminpremium != null)
            {
                model.MinAmounttoPaid = Math.Round(Convert.ToDecimal(vehiclewithminpremium.Premium + vehiclewithminpremium.StampDuty + vehiclewithminpremium.ZTSCLevy + (Convert.ToBoolean(vehiclewithminpremium.IncludeRadioLicenseCost) ? vehiclewithminpremium.RadioLicenseCost : 0.00m)), 2);
            }

            model.AmountPaid = Convert.ToDecimal(model.TotalPremium);
            model.BalancePaidDate = DateTime.Now;
            model.Notes = "";

            // model.InvoiceNumber = PolicyData.PolicyNumber;

            return model;
        }



        public PolicyDetail ProductDetail()
        {
            var model = new PolicyDetailModel();
            var InsService = new InsurerService();
            model.CurrencyId = InsuranceContext.Currencies.All().FirstOrDefault().Id;
            model.PolicyStatusId = InsuranceContext.PolicyStatuses.All().FirstOrDefault().Id;
            model.BusinessSourceId = InsuranceContext.BusinessSources.All().FirstOrDefault().Id;
            //model.Products = InsuranceContext.Products.All().ToList();
            model.InsurerId = InsService.GetInsurers().FirstOrDefault().Id;
            var objList = InsuranceContext.PolicyDetails.All(orderBy: "Id desc").FirstOrDefault();
            if (objList != null)
            {
                string number = objList.PolicyNumber.Split('-')[0].Substring(4, objList.PolicyNumber.Length - 6);
                long pNumber = Convert.ToInt64(number.Substring(2, number.Length - 2)) + 1;
                string policyNumber = string.Empty;
                int length = 7;
                length = length - pNumber.ToString().Length;
                for (int i = 0; i < length; i++)
                {
                    policyNumber += "0";
                }
                policyNumber += pNumber;
                ViewBag.PolicyNumber = "GMCC" + DateTime.Now.Year.ToString().Substring(2, 2) + policyNumber + "-1";
                model.PolicyNumber = ViewBag.PolicyNumber;
            }
            else
            {
                ViewBag.PolicyNumber = ConfigurationManager.AppSettings["PolicyNumber"] + "-1";
                model.PolicyNumber = ViewBag.PolicyNumber;
            }

            model.BusinessSourceId = 3;

            return Mapper.Map<PolicyDetailModel, PolicyDetail>(model);
        }



        private void SaveUserPasswordDetails(ApplicationUser user)
        {
            var userdetail = new AspNetUsersDetail { UserId = user.Id, CreatedOn = DateTime.Now, PasswordExpire = false };
            var data = Mapper.Map<AspNetUsersDetail, AspNetUsersDetail>(userdetail);
            InsuranceContext.AspNetUsersDetails.Insert(data);
        }


    }
}