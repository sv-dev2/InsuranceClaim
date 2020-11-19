using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Insurance.Domain;
using AutoMapper;
using System.Configuration;
using System.Globalization;
using Insurance.Service;
using PayPal.Api;
using System.Web.Script.Serialization;
using System.Web.Configuration;
using System.Net;
using System.Text;
using System.IO;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using Webdev.Payments;

namespace InsuranceClaim.Controllers
{
    public class RenewController : Controller
    {
        private ApplicationUserManager _userManager;
        Insurance.Service.smsService objsmsService = new Insurance.Service.smsService();

        string AdminEmail = WebConfigurationManager.AppSettings["AdminEmail"];
        string ZimnatEmail = WebConfigurationManager.AppSettings["ZimnatEmail"];
        string _pdfPath = "";
        string _pdfCode = "";

        decimal _InflationFactorAmt = 25;

        int _currencId = 6; //RTGS$
        public RenewController()
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

        //public ActionResult Index(int? vehicleid)
        //{
        //    Session["RenewVehicleId"] = vehicleid;
        //    CustomerModel custdata = new CustomerModel();
        //    bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
        //    string path = Server.MapPath("~/Content/Countries.txt");
        //    var countries = System.IO.File.ReadAllText(path);
        //    var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
        //    ViewBag.Cities = InsuranceContext.Cities.All();
        //    ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));



        //    var vehicledetails = InsuranceContext.VehicleDetails.Single(where: $"Id = '{vehicleid}'");
        //    var customerdetail = InsuranceContext.Customers.Single(where: $"Id= '{vehicledetails.CustomerId}'");
        //    if (customerdetail != null)
        //    {

        //        custdata = Mapper.Map<Customer, CustomerModel>(customerdetail);
        //        var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == customerdetail.UserID);
        //        if (dbUser != null)
        //        {
        //            custdata.EmailAddress = dbUser.Email;
        //        }

        //    }

        //    return View(custdata);
        //}


        public ActionResult Index(int? vehicleid)
        {

            Session["RenewVehicleId"] = vehicleid;
            CustomerModel custdata = new CustomerModel();
            var GetUpdatedCustData = (CustomerModel)Session["ReCustomerDataModal"];
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            string path = Server.MapPath("~/Content/Countries.txt");
            var countries = System.IO.File.ReadAllText(path);
            var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
            ViewBag.Cities = InsuranceContext.Cities.All();
            ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));





            if (GetUpdatedCustData == null)
            {
                var vehicledetails = InsuranceContext.VehicleDetails.Single(where: $"Id = '{vehicleid}'");
                var customerdetail = InsuranceContext.Customers.Single(where: $"Id= '{vehicledetails.CustomerId}'");

                if (customerdetail != null)
                {

                    custdata = Mapper.Map<Customer, CustomerModel>(customerdetail);

                    // for approving 
                    //Session["ReCustomerDataModal"] = custdata;
                    //RenewApproveVRNToIceCash(customerdetail, vehicledetails);

                    var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == customerdetail.UserID);
                    if (dbUser != null)
                    {
                        custdata.EmailAddress = dbUser.Email;
                    }

                }
            }
            else
            {
                if (GetUpdatedCustData != null)
                {
                    var User = UserManager.FindById(GetUpdatedCustData.UserID);
                    custdata.AddressLine1 = GetUpdatedCustData.AddressLine1;
                    custdata.AddressLine2 = GetUpdatedCustData.AddressLine2;
                    custdata.City = GetUpdatedCustData.City;
                    custdata.Id = GetUpdatedCustData.Id;
                    custdata.Country = GetUpdatedCustData.Country;
                    custdata.Zipcode = GetUpdatedCustData.Zipcode;
                    custdata.Gender = GetUpdatedCustData.Gender;
                    custdata.PhoneNumber = GetUpdatedCustData.PhoneNumber;
                    custdata.NationalIdentificationNumber = GetUpdatedCustData.NationalIdentificationNumber;
                    custdata.DateOfBirth = GetUpdatedCustData.DateOfBirth;
                    custdata.EmailAddress = GetUpdatedCustData.EmailAddress;
                    custdata.FirstName = GetUpdatedCustData.FirstName;
                    custdata.LastName = GetUpdatedCustData.LastName;
                    custdata.CountryCode = GetUpdatedCustData.CountryCode;
                    custdata.IsCustomEmail = GetUpdatedCustData.IsCustomEmail;
                }
            }

            return View(custdata);
        }



        public void UpdateDeactiveVehilce(int? vehicleid)
        {
            if (vehicleid != 0)
            {
                var vehicleDetails = InsuranceContext.VehicleDetails.Single(where: "Id=" + vehicleid + "and IsActive=1");
                if (vehicleDetails != null)
                {
                    vehicleDetails.IsActive = false;
                    vehicleDetails.isLapsed = true;
                    InsuranceContext.VehicleDetails.Update(vehicleDetails);

                    var splitRenewPolicyNumber = vehicleDetails.RenewPolicyNumber.Replace("-2", "-3");

                    var renewVehicleDetails = InsuranceContext.VehicleDetails.Single(where: "RenewPolicyNumber='" + splitRenewPolicyNumber + "'");

                    if (renewVehicleDetails != null)
                    {
                        var summaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.Single(where: "VehicleDetailsId=" + renewVehicleDetails.Id);

                        if (summaryVehicleDetails != null)
                        {
                            var summaryDetails = InsuranceContext.SummaryDetails.Single(summaryVehicleDetails.SummaryDetailId);


                            if (summaryDetails != null)
                            {

                                decimal radioLicenseCost = 0;
                                if (renewVehicleDetails.IncludeRadioLicenseCost == true)
                                {
                                    radioLicenseCost = renewVehicleDetails.RadioLicenseCost.Value;
                                }

                                summaryDetails.TotalPremium = renewVehicleDetails.Premium + renewVehicleDetails.StampDuty + renewVehicleDetails.ZTSCLevy + renewVehicleDetails.VehicleLicenceFee + radioLicenseCost;
                                summaryDetails.TotalStampDuty = renewVehicleDetails.StampDuty;
                                summaryDetails.TotalZTSCLevies = renewVehicleDetails.ZTSCLevy;
                                summaryDetails.TotalRadioLicenseCost = renewVehicleDetails.RadioLicenseCost;

                                InsuranceContext.SummaryDetails.Update(summaryDetails);

                            }
                        }
                    }
                }
            }
        }

        public ActionResult BackToCustomerDetail(int id = 0)
        {
            var vehicleId = (Int32)Session["RenewVehicleId"];

            if (id != -1) // -1 use for getting session value when click on back button
            {
                //  RemoveSession();
                ClearRenewSession();
            }

            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            string path = Server.MapPath("~/Content/Countries.txt");
            var countries = System.IO.File.ReadAllText(path);
            var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
            ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));

            //string paths = Server.MapPath("~/Content/Cities.txt");
            //var cities = System.IO.File.ReadAllText(paths);
            //var resultts = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObjects>(cities);
            //ViewBag.Cities = resultts.cities;

            ViewBag.Cities = InsuranceContext.Cities.All();



            if (id > 0) // if staff try to edit Qutation
            {
                SetCustomerValueIntoSession(id); // here id represent to summardetialid during edit the Qutation
            }


            if (userLoggedin && id == 0)
            {
                var customerModel = new CustomerModel();
                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());


                var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{User.Identity.GetUserId().ToString()}'").FirstOrDefault();
                var customerData = (CustomerModel)Session["ReCustomerDataModal"];


                var role = UserManager.GetRoles(_User.Id.ToString()).FirstOrDefault();

                ViewBag.CurrentUserRole = role;

                if ((role != null && (role != "Staff" && role != "Renewals")))
                {
                    if (customerData != null)
                    {
                        var User = UserManager.FindById(customerData.UserID);
                        customerModel.AddressLine1 = customerData.AddressLine1;
                        customerModel.AddressLine2 = customerData.AddressLine2;
                        customerModel.City = customerData.City;
                        customerModel.Id = customerData.Id;
                        customerModel.Country = customerData.Country;
                        customerModel.Zipcode = customerData.Zipcode;
                        customerModel.Gender = customerData.Gender;
                        customerModel.PhoneNumber = customerData.PhoneNumber;
                        customerModel.NationalIdentificationNumber = customerData.NationalIdentificationNumber;
                        customerModel.DateOfBirth = customerData.DateOfBirth;
                        customerModel.EmailAddress = customerData.EmailAddress;
                        customerModel.FirstName = customerData.FirstName;
                        customerModel.LastName = customerData.LastName;
                        customerModel.CountryCode = customerData.CountryCode;
                        customerModel.IsCustomEmail = customerData.IsCustomEmail;
                    }
                    else
                    {
                        customerModel.AddressLine1 = _customerData.AddressLine1;
                        customerModel.AddressLine2 = _customerData.AddressLine2;
                        customerModel.City = _customerData.City;
                        customerModel.Id = _customerData.Id;
                        customerModel.Country = _customerData.Country;
                        customerModel.Zipcode = _customerData.Zipcode;
                        customerModel.Gender = _customerData.Gender;
                        customerModel.PhoneNumber = _User.PhoneNumber;
                        customerModel.NationalIdentificationNumber = _customerData.NationalIdentificationNumber;
                        customerModel.DateOfBirth = _customerData.DateOfBirth;
                        customerModel.EmailAddress = _User.Email;
                        customerModel.FirstName = _customerData.FirstName;
                        customerModel.LastName = _customerData.LastName;
                        customerModel.CountryCode = _customerData.Countrycode;
                        customerModel.CustomerId = _customerData.CustomerId;
                        customerModel.IsActive = _customerData.IsActive;
                        customerModel.UserID = _customerData.UserID;
                        customerModel.IsCustomEmail = _customerData.IsCustomEmail;
                    }
                    customerModel.Zipcode = "00263";

                    Session["CustomerDataModal"] = customerModel; // for admin
                }
                customerModel.Zipcode = "00263";
                //  RemoveSession();

                ClearRenewSession();

                return RedirectToAction("Index", "Renew", new { vehicleid = vehicleId });
            }
            else
            {
                var customerData = (CustomerModel)Session["ReCustomerDataModal"];

                //if (customerData==null)
                //{
                //    customerData = (CustomerModel)Session["ReCustomerDataModal"];
                //}

                var customerModel = new CustomerModel();
                customerModel.Zipcode = "00263";
                if (customerData != null)
                {
                    var User = UserManager.FindById(customerData.UserID);
                    customerModel.AddressLine1 = customerData.AddressLine1;
                    customerModel.AddressLine2 = customerData.AddressLine2;
                    customerModel.City = customerData.City;
                    customerModel.Id = customerData.Id;
                    customerModel.Country = customerData.Country;
                    customerModel.Zipcode = customerData.Zipcode;
                    customerModel.Gender = customerData.Gender;
                    customerModel.PhoneNumber = customerData.PhoneNumber;
                    customerModel.NationalIdentificationNumber = customerData.NationalIdentificationNumber;
                    customerModel.DateOfBirth = customerData.DateOfBirth;
                    customerModel.EmailAddress = customerData.EmailAddress;
                    customerModel.FirstName = customerData.FirstName;
                    customerModel.LastName = customerData.LastName;
                    customerModel.CountryCode = customerData.CountryCode;
                    customerModel.IsCustomEmail = customerData.IsCustomEmail;
                }
                //return View(customerModel);


                return RedirectToAction("Index", "Renew", new { vehicleid = vehicleId });
            }

        }

        public void SetCustomerValueIntoSession(int summaryId)
        {
            Session["ICEcashToken"] = null;
            Session["issummaryformvisited"] = true;

            Session["SummaryDetailId"] = summaryId;

            var summaryDetail = InsuranceContext.SummaryDetails.Single(summaryId);

            var Cusotmer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);

            CustomerModel custModel = AutoMapper.Mapper.Map<Customer, CustomerModel>(Cusotmer);

            if (Cusotmer != null)
            {
                var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == Cusotmer.UserID);
                if (dbUser != null)
                {
                    custModel.EmailAddress = dbUser.Email; ;
                }
            }

            Session["CustomerDataModal"] = custModel;
        }

        private void RemoveSession()
        {
            Session.Remove("CustomerDataModal");
            Session.Remove("PolicyData");
            Session.Remove("VehicleDetails");
            Session.Remove("SummaryDetailed");
            Session.Remove("CardDetail");
            Session.Remove("issummaryformvisited");
            Session.Remove("PaymentId");
            Session.Remove("InsuranceId");
        }


        [HttpPost]
        public async Task<JsonResult> SaveCustomerData(CustomerModel model, string buttonUpdate)
        {
            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (userLoggedin)
                {
                    var email = LoggedUserEmail();
                    if (email == model.EmailAddress)
                    {
                        return Json(new { IsError = false, error = "Staff and customer email can not be same" }, JsonRequestBehavior.AllowGet);
                    }

                    //After test  Change the Role(web User)
                    Session["ReCustomerDataModal"] = model;
                    return Json(new { IsError = true, error = "" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { IsError = false, error = TempData["ErrorMessage"].ToString() }, JsonRequestBehavior.AllowGet);
        }


        public string LoggedUserEmail()
        {
            string email = "";
            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (_userLoggedin)
            {
                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                email = _User.Email;
            }
            return email;
        }


        //public ActionResult RiskDetail(int? Id)
        //{

        //    var vehicleId = (Int32)Session["RenewVehicleId"];
        //    CustomerModel custdata = new CustomerModel();
        //    //get All PolicyDetail in session
        //    var policyid = InsuranceContext.VehicleDetails.Single(vehicleId).PolicyId;
        //    var policy = InsuranceContext.PolicyDetails.Single(policyid);
        //    Session["RenewVehiclePolicy"] = policy;
        //    //VehicleDetailId
        //    var lapsedvehicleid = (Int32)Session["RenewVehicleId"];
        //    var SummaryDetailId = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId = {lapsedvehicleid}").SummaryDetailId;
        //    var summary = InsuranceContext.SummaryDetails.Single(SummaryDetailId);
        //    int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
        //    var LapsedVehiclePolicy = (PolicyDetail)Session["RenewVehiclePolicy"];
        //    //Id is policyid from Policy detail table
        //    var viewModels = new RiskDetailModel();
        //    //HistoryVehicleDetailModel viewRenewModel = new HistoryVehicleDetailModel();
        //    var service = new VehicleService();

        //    ViewBag.VehicleUsage = service.GetAllVehicleUsage();

        //    viewModels.NumberofPersons = 0;
        //    viewModels.AddThirdPartyAmount = 0.00m;
        //    viewModels.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
        //    var makers = service.GetMakers();
        //    ViewBag.CoverType = service.GetCoverType();

        //    ViewBag.AgentCommission = service.GetAgentCommission();
        //    var data1 = (from p in InsuranceContext.BusinessSources.All().ToList()
        //                 join f in InsuranceContext.SourceDetails.All().ToList()
        //                 on p.Id equals f.BusinessId
        //                 select new
        //                 {
        //                     Value = f.Id,
        //                     Text = f.FirstName + " " + f.LastName + " - " + p.Source
        //                 }).ToList();

        //    List<SelectListItem> listdata = new List<SelectListItem>();
        //    foreach (var item in data1)
        //    {
        //        SelectListItem sli = new SelectListItem();
        //        sli.Value = Convert.ToString(item.Value);
        //        sli.Text = item.Text;
        //        listdata.Add(sli);
        //    }
        //    ViewBag.Sources = new SelectList(listdata, "Value", "Text");



        //    ViewBag.Currencies = InsuranceContext.Currencies.All();

        //    ViewBag.Makers = makers;
        //    viewModels.isUpdate = false;
        //    ViewBag.Products = InsuranceContext.Products.All(where: "Active = 'True' or Active is null").ToList();
        //    ViewBag.PaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is null").ToList();

        //    var eExcessTypeData = from eExcessType e in Enum.GetValues(typeof(eExcessType))
        //                          select new
        //                          {
        //                              ID = (int)e,
        //                              Name = e.ToString()
        //                          };

        //    ViewBag.eExcessTypeData = new SelectList(eExcessTypeData, "ID", "Name");
        //    if (makers.Count > 0)
        //    {
        //        var model = service.GetModel(makers.FirstOrDefault().MakeCode);
        //        ViewBag.Model = model;

        //    }

        //    viewModels.NoOfCarsCovered = 1;

        //    if (vehicleId > 0)
        //    {
        //        var data = InsuranceContext.VehicleDetails.Single(vehicleId);

        //        if (data != null)
        //        {
        //            viewModels.AgentCommissionId = data.AgentCommissionId;
        //            viewModels.ChasisNumber = data.ChasisNumber;
        //            viewModels.CoverEndDate = data.CoverEndDate;
        //            viewModels.CoverNoteNo = data.CoverNoteNo;
        //            viewModels.CoverStartDate = data.CoverStartDate;
        //            viewModels.CoverTypeId = data.CoverTypeId;
        //            viewModels.CubicCapacity = (int)Math.Round(data.CubicCapacity.Value, 0);
        //            viewModels.CustomerId = data.CustomerId;
        //            viewModels.EngineNumber = data.EngineNumber;
        //            viewModels.Excess = (int)Math.Round(data.Excess, 0);
        //            viewModels.ExcessType = data.ExcessType;
        //            viewModels.MakeId = data.MakeId;
        //            viewModels.ModelId = data.ModelId;

        //            viewModels.NoOfCarsCovered = data.NoOfCarsCovered;
        //            viewModels.OptionalCovers = data.OptionalCovers;
        //            viewModels.PolicyId = data.PolicyId;
        //            viewModels.Premium = data.Premium;
        //            viewModels.RadioLicenseCost = (int)Math.Round(data.RadioLicenseCost == null ? 0 : data.RadioLicenseCost.Value, 0);
        //            viewModels.Rate = data.Rate;
        //            viewModels.RegistrationNo = data.RegistrationNo;
        //            viewModels.StampDuty = data.StampDuty;
        //            viewModels.SumInsured = (int)Math.Round(data.SumInsured == null ? 0 : data.SumInsured.Value, 0);
        //            viewModels.VehicleColor = data.VehicleColor;
        //            viewModels.VehicleUsage = data.VehicleUsage;
        //            viewModels.VehicleYear = data.VehicleYear;


        //            viewModels.ZTSCLevy = data.ZTSCLevy;
        //            viewModels.NumberofPersons = data.NumberofPersons;
        //            viewModels.PassengerAccidentCover = data.PassengerAccidentCover;
        //            viewModels.IsLicenseDiskNeeded = Convert.ToBoolean(data.IsLicenseDiskNeeded);
        //            viewModels.ExcessBuyBack = data.ExcessBuyBack;
        //            viewModels.RoadsideAssistance = data.RoadsideAssistance;
        //            viewModels.MedicalExpenses = data.MedicalExpenses;
        //            viewModels.Addthirdparty = data.Addthirdparty;
        //            viewModels.InsuranceId = data.InsuranceId;

        //            viewModels.AddThirdPartyAmount = Math.Round(Convert.ToDecimal(data.AddThirdPartyAmount), 2);
        //            viewModels.ExcessAmount = Math.Round(Convert.ToDecimal(data.ExcessAmount), 2);
        //            viewModels.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(data.ExcessBuyBackAmount), 2);
        //            viewModels.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(data.MedicalExpensesAmount), 2);
        //            viewModels.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(data.PassengerAccidentCoverAmount), 2);
        //            viewModels.PassengerAccidentCoverAmountPerPerson = Math.Round(Convert.ToDecimal(data.PassengerAccidentCoverAmountPerPerson), 2);
        //            viewModels.RoadsideAssistanceAmount = Math.Round(Convert.ToDecimal(data.RoadsideAssistanceAmount), 2);

        //            //viewRenewModel.ExcessAmount = Math.Round(Convert.ToDecimal(data.ExcessAmount), 2);
        //            viewModels.MedicalExpensesPercentage = Math.Round(Convert.ToDecimal(data.MedicalExpensesPercentage), 2);
        //            viewModels.ExcessBuyBackPercentage = Math.Round(Convert.ToDecimal(data.ExcessBuyBackPercentage), 2);
        //            viewModels.RoadsideAssistancePercentage = Math.Round(Convert.ToDecimal(data.RoadsideAssistancePercentage), 2);
        //            viewModels.isUpdate = false;

        //            viewModels.vehicleindex = Convert.ToInt32(vehicleId);
        //            viewModels.PaymentTermId = data.PaymentTermId;
        //            viewModels.ProductId = data.ProductId;
        //            viewModels.IncludeRadioLicenseCost = Convert.ToBoolean(data.IncludeRadioLicenseCost);
        //            viewModels.RenewalDate = Convert.ToDateTime(data.RenewalDate);
        //            viewModels.CustomerId = data.CustomerId;
        //            viewModels.PolicyId = data.PolicyId;
        //            viewModels.AnnualRiskPremium = data.AnnualRiskPremium;
        //            viewModels.TermlyRiskPremium = data.TermlyRiskPremium;
        //            viewModels.QuaterlyRiskPremium = data.QuaterlyRiskPremium;
        //            viewModels.Discount = data.Discount;
        //            viewModels.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);
        //            viewModels.BalanceAmount = data.BalanceAmount;
        //            viewModels.TransactionDate = Convert.ToDateTime(data.TransactionDate);
        //            viewModels.Id = data.Id;

        //            viewModels.BusinessSourceDetailId = data.BusinessSourceDetailId;
        //            viewModels.CurrencyId = data.CurrencyId;

        //            var ser = new VehicleService();
        //            var model = ser.GetModel(data.MakeId);
        //            ViewBag.Model = model;
        //        }
        //    }


        //    return View(viewModels);
        //}


        public ActionResult RiskDetail(int? Id)
        {
            if (Session["RenewVehicleId"] == null)
            {
                return RedirectToAction("Index");
            }

            var vehicleId = (Int32)Session["RenewVehicleId"];
            CustomerModel custdata = new CustomerModel();
            //get All PolicyDetail in session
            var policyid = InsuranceContext.VehicleDetails.Single(vehicleId).PolicyId;
            var policy = InsuranceContext.PolicyDetails.Single(policyid);
            Session["RenewVehiclePolicy"] = policy;
            //VehicleDetailId
            var lapsedvehicleid = (Int32)Session["RenewVehicleId"];
            var SummaryDetailId = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId = {lapsedvehicleid}").SummaryDetailId;
            var summary = InsuranceContext.SummaryDetails.Single(SummaryDetailId);
            int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var LapsedVehiclePolicy = (PolicyDetail)Session["RenewVehiclePolicy"];
            //Id is policyid from Policy detail table
            var viewModels = new RiskDetailModel();
            //HistoryVehicleDetailModel viewRenewModel = new HistoryVehicleDetailModel();
            var service = new VehicleService();

            ViewBag.VehicleUsage = service.GetAllVehicleUsage();
            ViewBag.TaxClass = InsuranceContext.VehicleTaxClasses.All().ToList();

            viewModels.NumberofPersons = 0;
            viewModels.AddThirdPartyAmount = 0.00m;
            viewModels.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType();

            ViewBag.AgentCommission = service.GetAgentCommission();
            var data1 = (from p in InsuranceContext.BusinessSources.All().ToList()
                         join f in InsuranceContext.SourceDetails.All().ToList()
                         on p.Id equals f.BusinessId
                         select new
                         {
                             Value = f.Id,
                             Text = f.FirstName + " " + f.LastName + " - " + p.Source
                         }).ToList();

            List<SelectListItem> listdata = new List<SelectListItem>();
            foreach (var item in data1)
            {
                SelectListItem sli = new SelectListItem();
                sli.Value = Convert.ToString(item.Value);
                sli.Text = item.Text;
                listdata.Add(sli);
            }
            ViewBag.Sources = new SelectList(listdata, "Value", "Text");

            ViewBag.Currencies = InsuranceContext.Currencies.All(where: $"IsActive = 'True'");

            ViewBag.Makers = makers;
            viewModels.isUpdate = false;
            ViewBag.Products = InsuranceContext.Products.All(where: "Active = 'True' or Active is null").ToList();
            ViewBag.PaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is null").ToList();

            ViewBag.VehicleLicensePaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is Null").ToList();
            ViewBag.RadioLicensePaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is Null").ToList();



            var eExcessTypeData = from eExcessType e in Enum.GetValues(typeof(eExcessType))
                                  select new
                                  {
                                      ID = (int)e,
                                      Name = e.ToString()
                                  };

            ViewBag.eExcessTypeData = new SelectList(eExcessTypeData, "ID", "Name");
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;
            }

            if (TempData["ViewModel"] != null)
            {
                
                viewModels = (RiskDetailModel)TempData["ViewModel"];
                return View(viewModels);
            }




            viewModels.NoOfCarsCovered = 1;

            if (vehicleId > 0)
            {
                var data = InsuranceContext.VehicleDetails.Single(vehicleId);
                if (Session["RenewVehicleDetails"] != null)
                {
                    var RiskDetail = (RiskDetailModel)Session["RenewVehicleDetails"];

                    viewModels.AgentCommissionId = RiskDetail.AgentCommissionId;
                    viewModels.ChasisNumber = RiskDetail.ChasisNumber;
                    viewModels.CoverEndDate = RiskDetail.CoverEndDate;
                    viewModels.CoverNoteNo = RiskDetail.CoverNoteNo;
                    viewModels.CoverStartDate = RiskDetail.CoverStartDate;
                    viewModels.CoverTypeId = RiskDetail.CoverTypeId;
                    viewModels.CubicCapacity = (int)Math.Round(RiskDetail.CubicCapacity.Value, 0);
                    viewModels.CustomerId = RiskDetail.CustomerId;
                    viewModels.EngineNumber = RiskDetail.EngineNumber;
                    viewModels.Excess = (int)Math.Round(RiskDetail.Excess, 0);
                    viewModels.ExcessType = RiskDetail.ExcessType;
                    viewModels.MakeId = RiskDetail.MakeId;
                    viewModels.ModelId = RiskDetail.ModelId;

                    viewModels.NoOfCarsCovered = RiskDetail.NoOfCarsCovered;
                    viewModels.OptionalCovers = RiskDetail.OptionalCovers;
                    viewModels.PolicyId = RiskDetail.PolicyId;
                    viewModels.Premium = RiskDetail.Premium;
                    viewModels.PremiumWithDiscount = RiskDetail.Premium + RiskDetail.Discount;

                    viewModels.PenaltiesAmt = RiskDetail.PenaltiesAmt;

                    viewModels.RadioLicenseCost = (int)Math.Round(RiskDetail.RadioLicenseCost == null ? 0 : RiskDetail.RadioLicenseCost.Value, 0);
                    viewModels.Rate = RiskDetail.Rate;
                    viewModels.RegistrationNo = RiskDetail.RegistrationNo;
                    viewModels.StampDuty = RiskDetail.StampDuty;
                    viewModels.SumInsured = (int)Math.Round(RiskDetail.SumInsured == null ? 0 : RiskDetail.SumInsured.Value, 0);
                    viewModels.VehicleColor = RiskDetail.VehicleColor;
                    viewModels.VehicleUsage = RiskDetail.VehicleUsage;
                    viewModels.VehicleYear = RiskDetail.VehicleYear;
                    viewModels.SuggestedValue = RiskDetail.SuggestedValue;

                    viewModels.ZTSCLevy = RiskDetail.ZTSCLevy;
                    viewModels.NumberofPersons = RiskDetail.NumberofPersons;
                    viewModels.PassengerAccidentCover = RiskDetail.PassengerAccidentCover;
                    viewModels.IsLicenseDiskNeeded = Convert.ToBoolean(RiskDetail.IsLicenseDiskNeeded);
                    viewModels.ExcessBuyBack = RiskDetail.ExcessBuyBack;
                    viewModels.RoadsideAssistance = RiskDetail.RoadsideAssistance;
                    viewModels.MedicalExpenses = RiskDetail.MedicalExpenses;
                    viewModels.Addthirdparty = RiskDetail.Addthirdparty;
                    viewModels.InsuranceId = RiskDetail.InsuranceId;

                    viewModels.AddThirdPartyAmount = Math.Round(Convert.ToDecimal(RiskDetail.AddThirdPartyAmount), 2);
                    viewModels.ExcessAmount = Math.Round(Convert.ToDecimal(RiskDetail.ExcessAmount), 2);
                    viewModels.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(RiskDetail.ExcessBuyBackAmount), 2);
                    viewModels.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(RiskDetail.MedicalExpensesAmount), 2);
                    viewModels.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(RiskDetail.PassengerAccidentCoverAmount), 2);
                    viewModels.PassengerAccidentCoverAmountPerPerson = Math.Round(Convert.ToDecimal(RiskDetail.PassengerAccidentCoverAmountPerPerson), 2);
                    viewModels.RoadsideAssistanceAmount = Math.Round(Convert.ToDecimal(RiskDetail.RoadsideAssistanceAmount), 2);

                    //viewRenewModel.ExcessAmount = Math.Round(Convert.ToDecimal(data.ExcessAmount), 2);
                    viewModels.MedicalExpensesPercentage = Math.Round(Convert.ToDecimal(RiskDetail.MedicalExpensesPercentage), 2);
                    viewModels.ExcessBuyBackPercentage = Math.Round(Convert.ToDecimal(RiskDetail.ExcessBuyBackPercentage), 2);
                    viewModels.RoadsideAssistancePercentage = Math.Round(Convert.ToDecimal(RiskDetail.RoadsideAssistancePercentage), 2);
                    viewModels.isUpdate = false;

                    viewModels.vehicleindex = Convert.ToInt32(vehicleId);
                    viewModels.PaymentTermId = RiskDetail.PaymentTermId;
                    viewModels.ProductId = RiskDetail.ProductId;
                    viewModels.IncludeRadioLicenseCost = Convert.ToBoolean(RiskDetail.IncludeRadioLicenseCost);
                    viewModels.RenewalDate = Convert.ToDateTime(RiskDetail.RenewalDate);
                    viewModels.CustomerId = RiskDetail.CustomerId;
                    viewModels.PolicyId = RiskDetail.PolicyId;
                    viewModels.AnnualRiskPremium = RiskDetail.AnnualRiskPremium;
                    viewModels.TermlyRiskPremium = RiskDetail.TermlyRiskPremium;
                    viewModels.QuaterlyRiskPremium = RiskDetail.QuaterlyRiskPremium;
                    viewModels.Discount = RiskDetail.Discount;
                    viewModels.VehicleLicenceFee = Convert.ToDecimal(RiskDetail.VehicleLicenceFee);
                    viewModels.BalanceAmount = RiskDetail.BalanceAmount;
                    viewModels.TransactionDate = Convert.ToDateTime(RiskDetail.TransactionDate);
                    viewModels.Id = RiskDetail.Id;

                    viewModels.BusinessSourceDetailId = RiskDetail.BusinessSourceDetailId;
                    viewModels.CurrencyId = RiskDetail.CurrencyId;


                    viewModels.IsPolicyExpire = RiskDetail.IsPolicyExpire;
                    viewModels.TaxClassId = RiskDetail.TaxClassId;
                    viewModels.CombinedID = RiskDetail.CombinedID;

                    viewModels.IncludeLicenseFee = RiskDetail.IncludeLicenseFee;
                    viewModels.IncludeRadioLicenseCost = RiskDetail.IncludeRadioLicenseCost;
                    viewModels.ZinaraLicensePaymentTermId = RiskDetail.ZinaraLicensePaymentTermId;
                    viewModels.RadioLicensePaymentTermId = RiskDetail.RadioLicensePaymentTermId;
                    viewModels.TaxClassId = RiskDetail.TaxClassId;


                    var ser = new VehicleService();
                    var model = ser.GetModel(RiskDetail.MakeId);
                    ViewBag.Model = model;
                }
                else
                {
                    if (data != null)
                    {
                        viewModels.AgentCommissionId = data.AgentCommissionId;
                        viewModels.ChasisNumber = data.ChasisNumber;
                        viewModels.CoverEndDate = data.CoverEndDate;
                        viewModels.CoverNoteNo = data.CoverNoteNo;
                        // viewModels.CoverStartDate = data.CoverStartDate;
                        // viewModels.CoverStartDate = DateTime.Now; // added 05 sep_2019
                        viewModels.CoverStartDate = data.CoverEndDate.Value.AddDays(1);
                        //  viewModels.CoverStartDate = data.CoverEndDate.Value.AddDays(10);

                        if (data.CoverEndDate < DateTime.Now)
                            viewModels.IsPolicyExpire = true;
                        else if (DateTime.Now >= data.RenewalDate.Value.AddMonths(-2))
                        {
                            viewModels.CoverStartDate = data.CoverStartDate.Value.AddDays(1);
                        }



                        viewModels.CoverTypeId = data.CoverTypeId;
                        viewModels.CubicCapacity = data.CubicCapacity == null ? 0 : (int)Math.Round(data.CubicCapacity.Value, 0);
                        viewModels.CustomerId = data.CustomerId;
                        viewModels.EngineNumber = data.EngineNumber;
                        viewModels.Excess = (int)Math.Round(data.Excess, 0);
                        viewModels.ExcessType = data.ExcessType;
                        viewModels.MakeId = data.MakeId;
                        viewModels.ModelId = data.ModelId;

                        viewModels.NoOfCarsCovered = data.NoOfCarsCovered;
                        viewModels.OptionalCovers = data.OptionalCovers;
                        viewModels.PolicyId = data.PolicyId;
                        viewModels.Premium = data.Premium;
                        viewModels.PremiumWithDiscount = data.Premium + data.Discount;
                        viewModels.RadioLicenseCost = (int)Math.Round(data.RadioLicenseCost == null ? 0 : data.RadioLicenseCost.Value, 0);
                        viewModels.Rate = data.Rate;
                        viewModels.RegistrationNo = data.RegistrationNo;
                        viewModels.StampDuty = data.StampDuty;
                        viewModels.SumInsured = (int)Math.Round(data.SumInsured == null ? 0 : data.SumInsured.Value, 0);
                        viewModels.VehicleColor = data.VehicleColor;
                        viewModels.VehicleUsage = data.VehicleUsage;
                        viewModels.VehicleYear = data.VehicleYear;


                        viewModels.ZTSCLevy = data.ZTSCLevy;
                        viewModels.NumberofPersons = data.NumberofPersons;
                        viewModels.PassengerAccidentCover = data.PassengerAccidentCover;
                        viewModels.IsLicenseDiskNeeded = Convert.ToBoolean(data.IsLicenseDiskNeeded);
                        viewModels.ExcessBuyBack = data.ExcessBuyBack;
                        viewModels.RoadsideAssistance = data.RoadsideAssistance;
                        viewModels.MedicalExpenses = data.MedicalExpenses;
                        viewModels.Addthirdparty = data.Addthirdparty;
                        viewModels.InsuranceId = data.InsuranceId;

                        viewModels.AddThirdPartyAmount = Math.Round(Convert.ToDecimal(data.AddThirdPartyAmount), 2);
                        viewModels.ExcessAmount = Math.Round(Convert.ToDecimal(data.ExcessAmount), 2);
                        viewModels.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(data.ExcessBuyBackAmount), 2);
                        viewModels.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(data.MedicalExpensesAmount), 2);
                        viewModels.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(data.PassengerAccidentCoverAmount), 2);
                        viewModels.PassengerAccidentCoverAmountPerPerson = Math.Round(Convert.ToDecimal(data.PassengerAccidentCoverAmountPerPerson), 2);
                        viewModels.RoadsideAssistanceAmount = Math.Round(Convert.ToDecimal(data.RoadsideAssistanceAmount), 2);

                        //viewRenewModel.ExcessAmount = Math.Round(Convert.ToDecimal(data.ExcessAmount), 2);
                        viewModels.MedicalExpensesPercentage = Math.Round(Convert.ToDecimal(data.MedicalExpensesPercentage), 2);
                        viewModels.ExcessBuyBackPercentage = Math.Round(Convert.ToDecimal(data.ExcessBuyBackPercentage), 2);
                        viewModels.RoadsideAssistancePercentage = Math.Round(Convert.ToDecimal(data.RoadsideAssistancePercentage), 2);
                        viewModels.isUpdate = false;

                        viewModels.vehicleindex = Convert.ToInt32(vehicleId);
                        viewModels.PaymentTermId = data.PaymentTermId;
                        viewModels.ProductId = data.ProductId;
                        viewModels.IncludeRadioLicenseCost = Convert.ToBoolean(data.IncludeRadioLicenseCost);
                        viewModels.RenewalDate = Convert.ToDateTime(data.RenewalDate);
                        viewModels.CustomerId = data.CustomerId;
                        viewModels.PolicyId = data.PolicyId;
                        viewModels.AnnualRiskPremium = data.AnnualRiskPremium;
                        viewModels.TermlyRiskPremium = data.TermlyRiskPremium;
                        viewModels.QuaterlyRiskPremium = data.QuaterlyRiskPremium;
                        viewModels.Discount = data.Discount;
                        viewModels.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);
                        viewModels.BalanceAmount = data.BalanceAmount;
                        viewModels.TransactionDate = Convert.ToDateTime(data.TransactionDate);
                        viewModels.Id = data.Id;

                        viewModels.BusinessSourceDetailId = data.BusinessSourceDetailId;
                        viewModels.CurrencyId = data.CurrencyId;
                        viewModels.TaxClassId = data.TaxClassId;
                        viewModels.CombinedID = data.CombinedID;

                        var ser = new VehicleService();
                        var model = ser.GetModel(data.MakeId);
                        ViewBag.Model = model;
                    }
                }
                //}
            }
            return View(viewModels);
        }



        [HttpPost]
        public ActionResult GenerateQuote(RiskDetailModel model)
        {

            if (model.NumberofPersons == null)
            {
                model.NumberofPersons = 0;
            }

            if (model.AddThirdPartyAmount == null)
            {
                model.AddThirdPartyAmount = 0.00m;
            }

            ModelState.Remove("SumInsured");


            int vehicleUsage = model.VehicleUsage == null ? 0 : model.VehicleUsage.Value;
            decimal sumInsured = model.SumInsured == null ? 0 : model.SumInsured.Value;

            var miniumSumInsured = GetMinimumSumInsured(vehicleUsage, model.CurrencyId);

            if ((model.CoverTypeId == (int)eCoverType.Comprehensive) && (sumInsured < miniumSumInsured))
            {
                model.ErrorMessage = "Sum Insured amount should be greater or equal to " + miniumSumInsured;
                TempData["ViewModel"] = model;

                //if (User.IsInRole("Staff"))                  
                    return RedirectToAction("RiskDetail", new { id = 1 });
            }

            // for license payment term

            VehicleService _service = new VehicleService();
            // var validationMsg = _service.ValidationMessage(model);

            var validationMsg = "";
            if (validationMsg != "")
            {
                model.ErrorMessage = validationMsg;
                TempData["ViewModel"] = model;

               // if (User.IsInRole("Staff"))
                    return RedirectToAction("RiskDetail", new { id = 1 });
            }







            DateTimeFormatInfo usDtfi = new CultureInfo("en-US", false).DateTimeFormat;
            var service = new RiskDetailService();
            var startDate = Request.Form["CoverStartDate"];
            var endDate = Request.Form["CoverEndDate"];
            if (!string.IsNullOrEmpty(startDate))
            {
                ModelState.Remove("CoverStartDate");
                model.CoverStartDate = Convert.ToDateTime(startDate, usDtfi);
            }


            if (!string.IsNullOrEmpty(endDate))
            {
                ModelState.Remove("CoverEndDate");
                model.CoverEndDate = Convert.ToDateTime(endDate, usDtfi);
            }
            if (ModelState.IsValid)
            {
                // model.Id = 0;

                // List<RiskDetailModel> listriskdetailmodel = new List<RiskDetailModel>();
                if (Session["RenewVehicleDetails"] != null)
                {
                    //List<RiskDetailModel> listriskdetails = (List<RiskDetailModel>)Session["RenewVehicleDetails"];
                    //if (listriskdetails != null && listriskdetails.Count > 0)
                    //{
                    //    listriskdetailmodel = listriskdetails;
                    //}
                }
                //model.Id = 0;
                //listriskdetailmodel.Add(model);
                Session["RenewVehicleDetails"] = model;

            }
            return RedirectToAction("SummaryDetail");
        }

        public decimal GetMinimumSumInsured(int vehicleUsageId, int currencyId)
        {
            decimal amount = 0;
            RiskDetailService service = new RiskDetailService();
            var vehicleUsage = service.GetVehicleUsageById(vehicleUsageId);

            if (currencyId == (int)currencyType.USD)
            {
                if (vehicleUsage != null)
                    amount = vehicleUsage.USDBenchmark == null ? 2500 : Convert.ToDecimal(vehicleUsage.USDBenchmark);
                else
                    amount = 2500;
            }
            else
            {

                if (vehicleUsage != null)
                    amount = vehicleUsage.USDBenchmark == null ? 0 : vehicleUsage.USDBenchmark.Value * _InflationFactorAmt;
            }
            return amount;
        }


        public ActionResult SummaryDetail(int summaryDetailId = 0)
        {
            var Id = (Int32)Session["RenewVehicleId"];
            ViewBag.vehicleid = Id;
            var vehicle = (RiskDetailModel)Session["RenewVehicleDetails"];// summary.GetVehicleInformation(id);
            var model = new SummaryDetailModel();
            var summarydetail = (SummaryDetailModel)Session["ReSummaryDetailed"];
            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();

            ViewBag.SummaryDetailId = summaryDetailId;
            var role = "";

            if (System.Web.HttpContext.Current.User.Identity.GetUserId() != null)
            {
                role = UserManager.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).FirstOrDefault();
            }

            ViewBag.CurrentUserRole = role;

            var DiscountSettings = InsuranceContext.Settings.Single(where: $"keyname='Discount On Renewal'");
            model.CarInsuredCount = 1;
            model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());
            //default selection 

            model.PaymentMethodId = 1;

            if (User.IsInRole("Staff") || User.IsInRole("Renewals"))
            {
                model.PaymentMethodId = 1;
            }
            else
            {
                model.PaymentMethodId = 3;
            }

            model.PaymentTermId = 1;
            model.ReceiptNumber = "";
            model.SMSConfirmation = false;
            model.TotalPremium = 0.00m;
            model.TotalRadioLicenseCost = 0.00m;
            model.Discount = 0.00m;


            //foreach (var item in vehicle)
            //{
            //    model.TotalPremium += item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee;
            //    if (item.IncludeRadioLicenseCost)
            //    {
            //        model.TotalPremium += item.RadioLicenseCost;
            //        model.TotalRadioLicenseCost += item.RadioLicenseCost;
            //    }
            //    model.Discount += item.Discount;
            //}
            model.TotalPremium = vehicle.Premium + vehicle.ZTSCLevy + vehicle.StampDuty + vehicle.VehicleLicenceFee ;
            if (vehicle.IncludeRadioLicenseCost)
            {
                model.TotalPremium = model.TotalPremium + vehicle.RadioLicenseCost;
                model.TotalRadioLicenseCost = vehicle.RadioLicenseCost;
            }
            model.Discount = vehicle.Discount;
            model.TotalRadioLicenseCost = Math.Round(Convert.ToDecimal(model.TotalRadioLicenseCost), 2);
            model.Discount = Math.Round(Convert.ToDecimal(model.Discount), 2);
            model.TotalPremium = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);

            model.TotalStampDuty = vehicle.StampDuty;
            model.TotalSumInsured = vehicle.SumInsured;
            model.TotalZTSCLevies = vehicle.ZTSCLevy;
            model.ExcessBuyBackAmount = vehicle.ExcessBuyBackAmount;
            model.MedicalExpensesAmount = vehicle.MedicalExpensesAmount;
            model.PassengerAccidentCoverAmount = vehicle.PassengerAccidentCoverAmount;
            model.RoadsideAssistanceAmount = vehicle.RoadsideAssistanceAmount;
            model.ExcessAmount = vehicle.ExcessAmount;


            //model.TotalStampDuty = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.StampDuty)), 2);
            //model.TotalSumInsured = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.SumInsured)), 2);
            //model.TotalZTSCLevies = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ZTSCLevy)), 2);
            //model.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ExcessBuyBackAmount)), 2);
            //model.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.MedicalExpensesAmount)), 2);
            //model.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.PassengerAccidentCoverAmount)), 2);
            //model.RoadsideAssistanceAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.RoadsideAssistanceAmount)), 2);
            //model.ExcessAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.ExcessAmount)), 2);
            model.AmountPaid = 0.00m;
            model.MaxAmounttoPaid = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            //var vehiclewithminpremium = vehicle.OrderBy(x => x.Premium).FirstOrDefault();
            model.AmountPaid = Convert.ToDecimal(model.TotalPremium);
            model.BalancePaidDate = DateTime.Now;
            model.Notes = "";
            if (Session["RenewVehiclePolicy"] != null)
            {
                var PolicyData = (PolicyDetail)Session["RenewVehiclePolicy"];
                model.InvoiceNumber = PolicyData.PolicyNumber;
            }

            if (summarydetail != null)
            {
                model.Id = summarydetail.Id;
            }

            return View(model);
        }

        public static string CreateRandomPassword()
        {
            string _allowedChars = "0123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNOPQRSTUVWXYZ";
            Random randNum = new Random();
            char[] chars = new char[8];
            int allowedCharCount = _allowedChars.Length;
            for (int i = 0; i < 8; i++)
            {
                chars[i] = _allowedChars[(int)((_allowedChars.Length) * randNum.NextDouble())];
            }
            return new string(chars);
        }



        [HttpPost]
        public async Task<ActionResult> SubmitPlan1(SummaryDetailModel model, string btnSendQuatation)
        {
            SummaryDetailService servicedetail = new SummaryDetailService();

            try
            {
                if (model != null)
                {
                    //if (ModelState.IsValid && (model.AmountPaid >= model.MinAmounttoPaid && model.AmountPaid <= model.MaxAmounttoPaid))

                    int CustomerUniquId = 0;
                    //if (User.IsInRole("Administrator"))
                    //{
                    //    TempData["SucessMsg"] = "Admin can not create policy.";
                    //    return RedirectToAction("SummaryDetail");
                    //}


                    TempData["ErroMsg"] = null;
                    if (User.IsInRole("Staff") && model.PaymentMethodId == 1)
                    {
                        //  ModelState.Remove("InvoiceNumber");
                        if (string.IsNullOrEmpty(model.InvoiceNumber))
                        {
                            TempData["ErroMsg"] = "Please enter invoice number.";
                            return RedirectToAction("SummaryDetail");
                        }
                    }


                    if (ModelState.IsValid)
                    {
                        Insurance.Service.ICEcashService ICEcashService = new Insurance.Service.ICEcashService();
                        List<RiskDetailModel> list = new List<RiskDetailModel>();
                        string PartnerToken = "";

                        #region update  TPIQuoteUpdate
                        var customerDetails = new Customer();

                        var policyDetils = new PolicyDetail();

                        var customerEmail = "";
                        var policyNum = "";
                        var InsuranceID = "";
                        var vichelDetails = new VehicleDetail();


                        if (model.Id != 0)
                        {
                            model.CustomSumarryDetilId = model.Id;
                        }

                        //Ds31Jan

                        



                        var vehicleREnewid = (RiskDetailModel)Session["CheckRenewVehicleDetails"];
                        if (vehicleREnewid != null)
                        {
                            var summaryDetial = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId = '" + vehicleREnewid.Id + "'");
                            if (summaryDetial != null && btnSendQuatation == "") // while user come from qutation email
                            {
                                var vehicledetail = InsuranceContext.VehicleDetails.Single(where: $"Id = '{summaryDetial.VehicleDetailsId}'");
                                if (vehicledetail != null)
                                {
                                    if (model.CustomSumarryDetilId != 0) // cehck if request is comming from agent email
                                    {
                                        if (model.PaymentMethodId == 1)
                                            return RedirectToAction("SaveDetailList", "Renew", new { id = model.CustomSumarryDetilId, invoiceNumber = model.InvoiceNumber });
                                        if (model.PaymentMethodId == 3)
                                        {
                                            var payNow = PayNow(model.CustomSumarryDetilId, model.InvoiceNumber, model.PaymentMethodId.Value, Convert.ToDecimal(model.TotalPremium));
                                            if (payNow.IsSuccessPayment)
                                            {
                                                Response.Redirect(payNow.ReturnUrl);
                                                Session["PollUrl"] = payNow.PollUrl;
                                                return RedirectToAction("SaveDetailList", "Renew", new { id = model.CustomSumarryDetilId, invoiceNumer = model.InvoiceNumber, Paymentid = model.PaymentMethodId.Value });
                                            }
                                            else
                                            {
                                                return RedirectToAction("failed_url", "Paypal");
                                            }

                                            //TempData["PaymentMethodId"] = model.PaymentMethodId;
                                            //return RedirectToAction("makepayment", new { id = model.CustomSumarryDetilId, TotalPremiumPaid = Convert.ToString(model.AmountPaid) });

                                        }
                                        else
                                            return RedirectToAction("PaymentDetail", new { id = model.CustomSumarryDetilId });
                                    }
                                }
                            }
                        }


                        #endregion

                        #region Add All info to database

                        //var vehicle = (RiskDetailModel)Session["VehicleDetail"];
                        Session["ReSummaryDetailed"] = model;
                        string SummeryofReinsurance = "";
                        string SummeryofVehicleInsured = "";
                        bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                        var customer = (CustomerModel)Session["ReCustomerDataModal"];


                        var role = "";

                        if (System.Web.HttpContext.Current.User.Identity.GetUserId() != null)
                        {
                            role = UserManager.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).FirstOrDefault();

                        }

                        var userDetials = UserManager.FindByEmail(customer.EmailAddress);

                        if (userDetials == null)
                        {
                            customer.Id = 0;
                        }

                        //if user staff

                        if (role == "Staff" || role == "Renewals" || role == "Team Leaders" || role == "Administrator")
                        {
                            // check if email id exist in user table
                            var user = UserManager.FindByEmail(customer.EmailAddress);

                            // if exist - get customer id from xcustomer table and set customer.Id in Customer object
                            if (user != null && user.Id != null)
                            {
                                var customerDetials = InsuranceContext.Customers.Single(where: $"UserID = '" + user.Id + "'");

                                if (customerDetials != null)
                                {
                                    customer.Id = customerDetials.Id;
                                    CustomerUniquId = customerDetials.Id;

                                    // need to do work
                                    //if (btnSendQuatation != "" && model.Id != 0)
                                    //{
                                    //    var SummaryDetails = InsuranceContext.SummaryDetails.Single(where: $"CustomerId={customer.Id} and isQuotation = 'True'");
                                    //    if (SummaryDetails != null)
                                    //    {
                                    //        TempData["SucessMsg"] = customer.FirstName + " " + customer.LastName + " Quotation alredy exist, please edit existing.";
                                    //        return RedirectToAction("SummaryDetail");
                                    //    }
                                    //}
                                }
                            }
                        }


                        if (!userLoggedin)  // create new user without logged in
                        {
                            if (customer != null)
                            {
                                if (customer.Id == null || customer.Id == 0)
                                {
                                    decimal custId = 0;
                                    var user = new ApplicationUser { UserName = customer.EmailAddress, Email = customer.EmailAddress, PhoneNumber = customer.PhoneNumber };
                                    var result = await UserManager.CreateAsync(user, "Geninsure@123");
                                    if (result.Succeeded)
                                    {
                                        try
                                        {
                                            var roleresult = UserManager.AddToRole(user.Id, "Web Customer"); // for web user
                                        }
                                        catch (Exception ex)
                                        {
                                        }

                                        var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();
                                        if (objCustomer != null)
                                        {
                                            custId = objCustomer.CustomerId + 1;
                                        }
                                        else
                                        {
                                            custId = Convert.ToDecimal(ConfigurationManager.AppSettings["CustomerId"]);
                                        }

                                        customer.UserID = user.Id;
                                        customer.CustomerId = custId;
                                        var customerdata = Mapper.Map<CustomerModel, Customer>(customer);
                                        InsuranceContext.Customers.Insert(customerdata);
                                        customer.Id = customerdata.Id;
                                    }
                                }
                            }
                        }
                        else if (userLoggedin && userDetials == null) //  when user is logged in
                        {

                            if (customer.Id == null || customer.Id == 0)
                            {
                                decimal custId = 0;


                                var user = new ApplicationUser { UserName = customer.EmailAddress, Email = customer.EmailAddress, PhoneNumber = customer.PhoneNumber };
                                var result = await UserManager.CreateAsync(user, "Geninsure@123");
                                if (result.Succeeded)
                                {
                                    try
                                    {
                                        var roleresult = UserManager.AddToRole(user.Id, "Customer");
                                    }
                                    catch (Exception ex)
                                    {
                                    }

                                    var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();



                                    //Query
                                    if (objCustomer != null)
                                    {
                                        custId = objCustomer.CustomerId + 1;
                                    }
                                    else
                                    {
                                        custId = Convert.ToDecimal(ConfigurationManager.AppSettings["CustomerId"]);
                                    }

                                    customer.UserID = user.Id;
                                    customer.CustomerId = custId;
                                    var customerdata = Mapper.Map<CustomerModel, Customer>(customer);
                                    InsuranceContext.Customers.Insert(customerdata);
                                    customer.Id = customerdata.Id;
                                }
                            }
                        }
                        else if (userLoggedin && userDetials != null && customer.Id == 0) //  when user is logged in
                        {
                            decimal custId = 0;

                            var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();
                            //Query
                            if (objCustomer != null)
                            {
                                custId = objCustomer.CustomerId + 1;
                            }
                            else
                            {
                                custId = Convert.ToDecimal(ConfigurationManager.AppSettings["CustomerId"]);
                            }

                            customer.UserID = userDetials.Id;
                            customer.CustomerId = custId;
                            var customerdata = Mapper.Map<CustomerModel, Customer>(customer);
                            InsuranceContext.Customers.Insert(customerdata);
                            customer.Id = customerdata.Id;


                        }
                        else
                        {
                            //  var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                            var user = UserManager.FindByEmail(customer.EmailAddress);
                            //var objCustomer = InsuranceContext.Customers.Single(where: $"Userid=@0", parms: new object[] { User.Identity.GetUserId() });

                            if (user != null)
                            {
                                var number = user.PhoneNumber;
                                if (number != customer.PhoneNumber)
                                {
                                    user.PhoneNumber = customer.PhoneNumber;
                                    //  UserManager.Update(user); // 13 june _ 2019
                                }
                                // customer.UserID = User.Identity.GetUserId().ToString();

                                var customerDetials = InsuranceContext.Customers.Single(where: $"UserID = '" + user.Id + "'");

                                if (customerDetials != null)
                                {
                                    //   customer.UserID = user.Id;
                                    customer.CustomerId = customerDetials.CustomerId;
                                    var customerdata = Mapper.Map<CustomerModel, Customer>(customer);

                                    if (customerdata.CustomerId == 0) // if exting record belong to 0
                                    {
                                        customerdata.CustomerId = customerdata.Id;
                                    }


                                    // InsuranceContext.Customers.Update(customerdata);  // 13 june _ 2019
                                }

                            }
                        }


                        var policy = (PolicyDetail)Session["RenewVehiclePolicy"];


                        // Genrate new policy number


                        // end genrate policy number


                        if (policy != null)
                        {
                            if (policy.Id == null || policy.Id == 0)
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

                                Session["RenewVehiclePolicy"] = policy;
                            }
                            else
                            {
                                PolicyDetail policydata = InsuranceContext.PolicyDetails.All(policy.Id.ToString()).FirstOrDefault();
                                policydata.BusinessSourceId = policy.BusinessSourceId;
                                policydata.CurrencyId = policy.CurrencyId;
                                // policydata.CustomerId = policy.CustomerId;
                                policydata.CustomerId = customer.Id;
                                policydata.EndDate = null;
                                policydata.InsurerId = policy.InsurerId;
                                policydata.IsActive = policy.IsActive;
                                policydata.PolicyName = policy.PolicyName;
                                policydata.PolicyNumber = policy.PolicyNumber;
                                policydata.PolicyStatusId = policy.PolicyStatusId;
                                policydata.RenewalDate = null;
                                policydata.StartDate = null;
                                policydata.TransactionDate = null;
                                policy.ModifiedBy = customer.Id;
                                policy.ModifiedOn = DateTime.Now;
                                InsuranceContext.PolicyDetails.Update(policydata);
                            }

                        }
                        var Id = 0;
                        var listReinsuranceTransaction = new List<ReinsuranceTransaction>();
                        var vehicle = (RiskDetailModel)Session["RenewVehicleDetails"];
                        var summary = (SummaryDetailModel)Session["ReSummaryDetailed"];

                        string format = "yyyyMMdd";
                        if (vehicle != null)
                        {
                           // vehicle.CurrencyId = _currencId; //RTGS$ only  02 june 2020

                            if (!string.IsNullOrEmpty(vehicle.LicExpiryDate))
                            {
                                //var LicExpiryDate = DateTime.ParseExact(vehicle.LicExpiryDate, format, CultureInfo.InvariantCulture);
                                vehicle.LicExpiryDate = null;
                                if (vehicle.VehicleLicenceFee > 0)
                                    vehicle.IceCashRequest = "InsuranceAndLicense";
                            }
                            else
                                vehicle.IceCashRequest = "Insurance";

                            if(vehicle.RadioLicenseCost>0)
                            {
                                vehicle.IncludeRadioLicenseCost = true;
                            }

                            vehicle.IsMobile = false;

                            var _item = vehicle;

                            //List<RiskDetailModel> objVehicles = new List<RiskDetailModel>();
                            ////objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo });
                            //objVehicles.Add(new RiskDetailModel { RegistrationNo = _item.RegistrationNo, PaymentTermId = Convert.ToInt32(_item.PaymentTermId) });
                            //var  tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
                            //ResultRootObject quoteresponse = ICEcashService.checkVehicleExists(objVehicles, tokenObject.Response.PartnerToken, tokenObject.PartnerReference);

                            //if (quoteresponse.Response.Result == 0)
                            //{
                            //    response.message = quoteresponse.Response.Quotes[0].Message;
                            //}
                            //else
                            //{
                            //    response.Data = quoteresponse;
                            //}


                            //   var vehicelDetails = InsuranceContext.VehicleDetails.Single(where: $"policyid= '{policy.Id}' and RegistrationNo= '{_item.RegistrationNo}'");
                            var vehicelDetails = InsuranceContext.VehicleDetails.Single((Int32)Session["RenewVehicleId"]);

                            if (vehicelDetails != null)
                            {
                                // item.Id = vehicelDetails.Id;
                                vehicle.Id = 0;
                                vehicelDetails.IsActive = false;
                                vehicelDetails.RenewPolicyNumber = policy.PolicyNumber;
                                vehicelDetails.isLapsed = true;
                                InsuranceContext.VehicleDetails.Update(vehicelDetails);

                                var SummaryVehicalDetails = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId={vehicelDetails.Id}");
                                if (SummaryVehicalDetails != null)
                                    summary.Id = SummaryVehicalDetails.SummaryDetailId;

                            }


                            // Get renew policy number

                            int policyLastSequence = 0;
                            string[] splitPolicyNumber;
                            if (vehicelDetails.RenewPolicyNumber == null)
                            {
                                splitPolicyNumber = policy.PolicyNumber.Split('-');
                            }
                            else
                            {
                                // splitPolicyNumber = InsuranceContext.VehicleDetails.All(where: $"policyid= '{policy.Id}' and RegistrationNo= '{_item.RegistrationNo}'").OrderByDescending(c => c.Id).FirstOrDefault().RenewPolicyNumber.Split('-');
                                //  splitPolicyNumber = vehicelDetails.RenewPolicyNumber.Split('-');

                                splitPolicyNumber = GetHighestPolicyNumber(vehicelDetails.PolicyId).Split('-');
                            }


                            if (splitPolicyNumber.Length > 1)
                            {
                                policyLastSequence = Convert.ToInt32(splitPolicyNumber[1]);
                                policyLastSequence += 1;
                            }
                            string reNewPolicyNumber = splitPolicyNumber[0] + "-" + policyLastSequence;


                            if (vehicle.Id == 0)
                            {
                                var service = new RiskDetailService();
                                _item.CustomerId = customer.Id;
                                _item.PolicyId = policy.Id;
                                _item.RenewPolicyNumber = reNewPolicyNumber;

                                if (_item.IncludeRadioLicenseCost)
                                    _item.RadioLicenseCost = _item.RadioLicenseCost;
                                else
                                    _item.RadioLicenseCost = 0;



                                //   _item.InsuranceId = model.InsuranceId;
                                //if (model.AmountPaid < model.TotalPremium)
                                //{
                                //    _item.BalanceAmount = (_item.Premium + _item.ZTSCLevy + _item.StampDuty + (_item.IncludeRadioLicenseCost ? _item.RadioLicenseCost : 0.00m) - _item.Discount) - (model.AmountPaid / vehicle.Count);
                                //}

                                _item.Id = service.AddVehicleInformation(_item);
                                var vehicles = (RiskDetailModel)Session["RenewVehicleDetails"];



                               
                                // vehicles[Convert.ToInt32(_item.NoOfCarsCovered) - 1] = _item;
                                vehicles = _item;
                                Session["RenewVehicleDetails"] = vehicles;
                                Session["CheckRenewVehicleDetails"] = vehicles;


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
                                    if (Reinsurance.MinTreatyCapacity <= vehicle.SumInsured && vehicle.SumInsured <= Reinsurance.MaxTreatyCapacity)
                                    {
                                        ReinsuranceCase = Reinsurance;
                                        break;
                                    }
                                }

                                if (ReinsuranceCase != null && ReinsuranceCase.MaxTreatyCapacity != null)
                                {
                                    var basicPremium = vehicle.Premium;
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
                                        _reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((_reinsurance.ReinsuranceAmount / vehicle.SumInsured) * basicPremium), 2);
                                        AutoFacPremium = Convert.ToDecimal(_reinsurance.ReinsurancePremium);
                                        _reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(autofacReinsuranceBroker.Commission);
                                        _reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((_reinsurance.ReinsurancePremium * _reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        _reinsurance.VehicleId = vehicle.Id;
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
                                        __reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((__reinsurance.ReinsuranceAmount / vehicle.SumInsured) * basicPremium), 2);
                                        FacPremium = Convert.ToDecimal(__reinsurance.ReinsurancePremium);
                                        __reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                        __reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((__reinsurance.ReinsurancePremium * __reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        __reinsurance.VehicleId = vehicle.Id;
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
                                        reinsurance.ReinsurancePremium = Math.Round(Convert.ToDecimal((reinsurance.ReinsuranceAmount / vehicle.SumInsured) * basicPremium), 2);
                                        AutoFacPremium = Convert.ToDecimal(reinsurance.ReinsurancePremium);
                                        reinsurance.ReinsuranceCommissionPercentage = Convert.ToDecimal(ReinsuranceBroker.Commission);
                                        reinsurance.ReinsuranceCommission = Math.Round(Convert.ToDecimal((reinsurance.ReinsurancePremium * reinsurance.ReinsuranceCommissionPercentage) / 100), 2);
                                        reinsurance.VehicleId = vehicle.Id;
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
                                    VehicleModel vehiclemodel = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{vehicle.ModelId}'");
                                    VehicleMake vehiclemake = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{vehicle.MakeId}'");

                                    string vehicledescription = vehiclemodel.ModelDescription + " / " + vehiclemake.MakeDescription;

                                    // SummeryofVehicleInsured += "<tr><td>" + vehicledescription + "</td><td>" + Convert.ToString(item.SumInsured) + "</td><td>" + Convert.ToString(item.Premium) + "</td><td>" + AutoFacSumInsured.ToString() + "</td><td>" + AutoFacPremium.ToString() + "</td><td>" + FacSumInsured.ToString() + "</td><td>" + FacPremium.ToString() + "</td></tr>";

                                    SummeryofVehicleInsured += "<tr><td style='padding:7px 10px; font-size:14px'><font size='2'>" + vehicledescription + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + Convert.ToString(vehicle.SumInsured) + " </font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + Convert.ToString(vehicle.Premium) + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + AutoFacSumInsured.ToString() + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + AutoFacPremium.ToString() + "</ font ></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + FacSumInsured.ToString() + "</font></td><td style='padding:7px 10px; font-size:14px'><font size='2'>" + FacPremium.ToString() + "</font></td></tr>";

                                }
                            }
                        }



                        //   var item = vehicle;

                        try
                        {
                            var summarydetails = new SummaryVehicleDetail();
                            summarydetails.SummaryDetailId = summary.Id;
                            summarydetails.VehicleDetailsId = vehicle.Id;
                            summarydetails.CreatedBy = customer.Id;
                            summarydetails.CreatedOn = DateTime.Now;
                            InsuranceContext.SummaryVehicleDetails.Insert(summarydetails);
                        }
                        catch (Exception ex)
                        {
                            Insurance.Service.EmailService log = new Insurance.Service.EmailService();
                            log.WriteLog("exception during insert vehicel :" + ex.Message);

                        }


                        var DbEntry = Mapper.Map<SummaryDetailModel, SummaryDetail>(model);

                        if (summary != null)
                        {
                            // SummaryDetail summarydata = InsuranceContext.SummaryDetails.All(summary.Id.ToString()).FirstOrDefault(); // on 05-oct for updatig qutation

                            SummaryDetailsCalculation(summary);

                            var summarydata = Mapper.Map<SummaryDetailModel, SummaryDetail>(model);
                            summarydata.Id = summary.Id;
                            summarydata.CreatedOn = DateTime.Now;


                            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                            if (_userLoggedin)
                            {
                                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                                var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

                                if (_customerData != null)
                                {
                                    summarydata.CreatedBy = _customerData.Id;
                                }
                            }


                            summarydata.ModifiedBy = customer.Id;
                            summarydata.ModifiedOn = DateTime.Now;
                            if (summarydata.BalancePaidDate.Value.Year == 0001)
                            {
                                summarydata.BalancePaidDate = DateTime.Now;
                            }
                            if (DbEntry.Notes == null)
                            {
                                summarydata.Notes = "";
                            }

                            if (!string.IsNullOrEmpty(btnSendQuatation))
                            {
                                summarydata.isQuotation = true;
                                summarydata.IsRenewQutation = true;
                            }


                            //summarydata.CustomerId = vehicle[0].CustomerId;

                            summarydata.CustomerId = customer.Id;
                            InsuranceContext.SummaryDetails.Update(summarydata);



                            if (listReinsuranceTransaction != null && listReinsuranceTransaction.Count > 0)
                            {
                                foreach (var item in listReinsuranceTransaction)
                                {
                                    var InsTransac = InsuranceContext.ReinsuranceTransactions.Single(item.Id);
                                    InsTransac.SummaryDetailId = summary.Id;
                                    InsuranceContext.ReinsuranceTransactions.Update(InsTransac);
                                }
                            }
                        }

                        if (vehicle != null && summary.Id != null && summary.Id > 0)
                        {
                            //var SummaryDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summary.Id}").ToList();

                            //if (SummaryDetails != null && SummaryDetails.Count > 0)
                            //{
                            //    foreach (var item1 in SummaryDetails)
                            //    {
                            //        InsuranceContext.SummaryVehicleDetails.Delete(item1);
                            //    }
                            //}



                        }
                        MiscellaneousService.UpdateBalanceForVehicles(summary.AmountPaid, summary.Id, Convert.ToDecimal(summary.TotalPremium), false);


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
                                        var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
                                        string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/Reinsurance_Admin.cshtml";
                                        string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
                                        var Body = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##SummeryofVehicleInsured##", SummeryofVehicleInsured);

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
                                    var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
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

                        #endregion




                        #region Quotation Email
                        if (!string.IsNullOrEmpty(btnSendQuatation))
                        {
                            List<VehicleDetail> ListOfVehicles = new List<VehicleDetail>();
                            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={model.Id}").ToList();
                            foreach (var itemSummaryVehicleDetails in SummaryVehicleDetails)
                            {
                                var itemVehicle = InsuranceContext.VehicleDetails.Single(where : "id="+  itemSummaryVehicleDetails.VehicleDetailsId + " and IsActive=1");

                                if(itemVehicle!=null)
                                {
                                    ListOfVehicles.Add(itemVehicle);
                                }

                                
                            }

                            var currencylist = servicedetail.GetAllCurrency();
                            string CurrencyName = "";

                            //List<VehicleDetail> ListOfVehicles = new List<VehicleDetail>();
                            string Summeryofcover = "";
                            var RoadsideAssistanceAmount = 0.00m;
                            var MedicalExpensesAmount = 0.00m;
                            var ExcessBuyBackAmount = 0.00m;
                            var PassengerAccidentCoverAmount = 0.00m;
                            var ExcessAmount = 0.00m;

                            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };

                            string converType = "";

                            foreach (var item in ListOfVehicles)
                            {
                                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                                VehicleModel modell = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'");

                                string vehicledescription = modell.ModelDescription + " / " + make.MakeDescription;

                                RoadsideAssistanceAmount = RoadsideAssistanceAmount + Convert.ToDecimal(item.RoadsideAssistanceAmount);
                                MedicalExpensesAmount = MedicalExpensesAmount + Convert.ToDecimal(item.MedicalExpensesAmount);
                                ExcessBuyBackAmount = ExcessBuyBackAmount + Convert.ToDecimal(item.ExcessBuyBackAmount);
                                PassengerAccidentCoverAmount = PassengerAccidentCoverAmount + Convert.ToDecimal(item.PassengerAccidentCoverAmount);
                                ExcessAmount = ExcessAmount + Convert.ToDecimal(item.ExcessAmount);

                                if (item.CoverTypeId == 1)
                                {
                                    converType = eCoverType.ThirdParty.ToString();
                                }
                                if (item.CoverTypeId == 2)
                                {
                                    converType = eCoverType.FullThirdParty.ToString();
                                }

                                if (item.CoverTypeId == 4)
                                {
                                    converType = eCoverType.Comprehensive.ToString();
                                }

                                string paymentTermsNmae = "";
                                var paymentTermVehicel = ePaymentTermData.FirstOrDefault(p => p.ID == item.PaymentTermId);


                                if (item.PaymentTermId == 1)
                                    paymentTermsNmae = "Annual";
                                else
                                    paymentTermsNmae = item.PaymentTermId + " Months";

                                decimal? premiumDue = item.Premium + item.StampDuty + item.ZTSCLevy + item.VehicleLicenceFee + item.RadioLicenseCost;
                                var productDetail = InsuranceContext.Products.Single(Convert.ToInt32(item.ProductId));

                                var taxClassDetials = InsuranceContext.VehicleTaxClasses.Single(item.TaxClassId);

                                var vehicledetail = InsuranceContext.VehicleDetails.Single(where: "id=" + item.Id + " and IsActive=1" );
                                CurrencyName = servicedetail.GetCurrencyName(currencylist, vehicledetail.CurrencyId);
                                string policyPeriod = item.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + item.CoverEndDate.Value.ToString("dd/MM/yyyy");

                                // Summeryofcover += "<tr> <td style='padding: 7px 10px; font - size:15px;'>" + item.RegistrationNo + " </td> <td style='padding: 7px 10px; font - size:15px;'>" + vehicledescription + "</td><td style='padding: 7px 10px; font - size:15px;'>" + CurrencyName + item.SumInsured + "</td><td style='padding: 7px 10px; font - size:15px;'>" + converType + "</td><td style='padding: 7px 10px; font - size:15px;'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</td> <td style='padding: 7px 10px; font - size:15px;'>" + policyPeriod + "</td><td style='padding: 7px 10px; font - size:15px;'>" + paymentTermsNmae + "</td><td style='padding: 7px 10px; font - size:15px;'>" + CurrencyName + Convert.ToString(item.Premium + item.Discount) + "</td></tr>";

                                
                                // new
                                Summeryofcover += "<table border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse; width:100%;  border-color:#ffcc00; border-style:solid;' >";
                                Summeryofcover += "<tr> <th  bgcolor='#D3D3D3' colspan='2' style='background:#D3D3D3; color:#000;text-align: center; padding:10px;   word-break: break-all;'> <font size='1'>Gene-Insure</font> </th> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Cover Note #: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + item.CoverNote + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px; '> <font size='1'> Transaction Date: </font> </td> <td style='padding: 1px 10px; '> <font size='1'>" + item.TransactionDate.Value.ToShortDateString() + " </font> </td> </tr>";
                                Summeryofcover += " </table>";

                                Summeryofcover += "<table  border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse; width:100% border-color:#ffcc00; border-style:solid;' >";
                                Summeryofcover += "<tr> <th  bgcolor='#D3D3D3' colspan='2' style='background:#D3D3D3; color:#000;text-align: center; padding:1px;   word-break: break-all;'> <font size='1'>Certificate of Motor Insurance</font>  </th> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Insurance Type: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>Road Traffic Act </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle Type: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + (item.CoverTypeId == 4 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + " " + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Start Date: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + item.CoverStartDate.Value.ToString("dd/MM/yyyy") + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> End Date: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + item.CoverEndDate.Value.ToString("dd/MM/yyyy") + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Period: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + paymentTermsNmae + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Premium: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + item.Premium + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Gvt Levy: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + item.ZTSCLevy + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Stamp Duty: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + item.StampDuty + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Premium Due: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + Math.Round(Convert.ToDecimal(premiumDue), 2) + " </font> </td> </tr>";
                                Summeryofcover += " </table>";

                                // #ddd
                                Summeryofcover += "<table  border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse; width:100%;  border-color:#ffcc00; border-style:solid' >";
                                Summeryofcover += "<tr> <th  bgcolor='#D3D3D3' colspan='2' style='background:#D3D3D3; color:#000;text-align: center; padding:10px;  word-break: break-all;'> <font size='1'>Vehicle Details</font> </th> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle Reg. Number: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + item.RegistrationNo + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle Type: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + productDetail.ProductName + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Tax Class: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + taxClassDetials.Description + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px; font-size:15px;'> <font size='1'> Sum Insured: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + item.SumInsured + " </font> </td> </tr>";
                                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle: </font> </td> <td style='padding: 5px 10px;'> <font size='1'>" + vehicledescription + " </font> </td> </tr>";
                                Summeryofcover += " </table> ";

                            }


                            var summaryDetail = InsuranceContext.SummaryDetails.Single(model.Id);

                            if (summaryDetail != null)
                            {
                                model.CustomSumarryDetilId = summaryDetail.Id;
                            }

                            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                            var customerQuotation = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
                            var user = UserManager.FindById(customerQuotation.UserID);
                            //var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={model.Id}").ToList();
                            int QutationVehicleId = ListOfVehicles[0].Id;
     
                            var vehicleQuotation = InsuranceContext.VehicleDetails.Single(where: "id=" + QutationVehicleId + " and IsActive=1");
                            var policyQuotation = InsuranceContext.PolicyDetails.Single(vehicleQuotation.PolicyId);
                            //  var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
                            var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == vehicleQuotation.PaymentTermId);


                            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();

                            string QuotationEmailPath = "/Views/Shared/EmaiTemplates/QuotationEmail.cshtml";


                            string urlPath = WebConfigurationManager.AppSettings["urlPath"];

                            string rootPath = urlPath + "/CustomerRegistration/SummaryDetail?summaryDetailId=" + summaryDetail.Id;

                            // need to do work

                            // Product name

                            string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(QuotationEmailPath));
                            var Bodyy = MotorBody.Replace("##PolicyNo##", policyQuotation.PolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", user.PhoneNumber).
                                Replace("##FirstName##", customerQuotation.FirstName).Replace("##LastName##", customerQuotation.LastName).Replace("##Email##", user.Email).
                                Replace("##BirthDate##", customerQuotation.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customerQuotation.AddressLine1).
                                Replace("##Address2##", customerQuotation.AddressLine2).Replace("##Renewal##", vehicleQuotation.RenewalDate.Value.ToString("dd/MM/yyyy")).
                                Replace("##InceptionDate##", vehicleQuotation.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name + " Months").
                                Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (vehicleQuotation.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + " Months")).
                                Replace("##TotalPremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).
                                Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies)).
                                Replace("##PremiumDue##", Convert.ToString(ListOfVehicles.Sum(x => x.Premium) + ListOfVehicles.Sum(x => x.Discount))).
                                Replace("##PostalAddress##", customerQuotation.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).
                                Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).
                                Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaryDetail.TotalRadioLicenseCost)).
                                Replace("##Discount##", Convert.ToString(ListOfVehicles.Sum(x => x.Discount)))
                                 .Replace("##PenaltiesAmt##", Convert.ToString(ListOfVehicles.Sum(x => x.PenaltiesAmt)))
                                .Replace("##ExcessAmount##", Convert.ToString(ExcessAmount))
                                .Replace("##currencyName##", CurrencyName).
                                Replace("##SummaryDetailsPath##", Convert.ToString(rootPath)).Replace("##insurance_period##", vehicleQuotation.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + vehicleQuotation.CoverEndDate.Value.ToString("dd/MM/yyyy")).
                                Replace("##NINumber##", customerQuotation.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));

                            #region Invoice PDF
                            var attacehmetn_File = MiscellaneousService.EmailPdf(Bodyy, policyQuotation.CustomerId, policyQuotation.PolicyNumber, "Quotation");
                            #endregion

                            #region Invoice EMail
                            //var _yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
                            List<string> _attachementss = new List<string>();
                            _attachementss.Add(attacehmetn_File);
                            //_attachementss.Add(_yAtter);


                            if (customer.IsCustomEmail)
                                objEmailService.SendEmail(LoggedUserEmail(), "", "", "Renew Quotation", Bodyy, _attachementss);
                            else
                                objEmailService.SendEmail(user.Email, "", "", "Renew Quotation", Bodyy, _attachementss);
                            
                            #endregion
                            #region Send Quotation SMS
                            Insurance.Service.smsService objsmsService = new Insurance.Service.smsService();


                            // done
                            // string Recieptbody = "Hi " + customer.FirstName + "\nYour quote is" + "$" + Convert.ToString(summaryDetail.AmountPaid) + " for a " + converType+ " with GeneInsure. Please confirm your acceptance for policy activation. Thank you.";

                            string Recieptbody = "Dear " + customer.FirstName + "\nYour quote is" + "$" + Convert.ToString(summaryDetail.TotalPremium) + " for a " + converType + " with GeneInsure. Please confirm your acceptance for policy activation. Thank you.";


                            var Recieptresult = await objsmsService.SendSMS(customer.CountryCode.Replace("+", "") + user.PhoneNumber, Recieptbody);

                            SmsLog objRecieptsmslog = new SmsLog()
                            {
                                Sendto = user.PhoneNumber,
                                Body = Recieptbody,
                                Response = Recieptresult,
                                CreatedBy = customer.Id,
                                CreatedOn = DateTime.Now
                            };

                            InsuranceContext.SmsLogs.Insert(objRecieptsmslog);
                            #endregion


                            ClearRenewSession();


                            TempData["SucessMsg"] = "Quotation has been sent email sucessfully.";


                            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                            if (_userLoggedin)
                            {
                                return RedirectToAction("QuotationList", "Account");
                            }
                            else
                            {
                                return Redirect("/CustomerRegistration/index");
                            }

                            // return RedirectToAction("SummaryDetail");
                        }
                        #endregion


                        Session["RenewVehicleId"] = vehicle.Id;
                        // return RedirectToAction("InitiatePaynowTransaction", "Paypal", new { id = DbEntry.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid), PolicyNumber = policy.PolicyNumber, Email = customer.EmailAddress });

                       
                        if (model.PaymentMethodId == 1 || model.PaymentMethodId == (int)paymentMethod.PayLater)
                            return RedirectToAction("SaveDetailList", "Renew", new { id = summary.Id, invoiceNumer = model.InvoiceNumber, Paymentid= model.PaymentMethodId });
                        if (model.PaymentMethodId == 3)
                        {
                            var payNow = PayNow(DbEntry.Id, model.InvoiceNumber, model.PaymentMethodId.Value, Convert.ToDecimal(model.TotalPremium));
                            if (payNow.IsSuccessPayment)
                            {
                                Response.Redirect(payNow.ReturnUrl);
                                Session["PollUrl"] = payNow.PollUrl;
                                return RedirectToAction("SaveDetailList", "Renew", new { id = summary.Id, invoiceNumer = model.InvoiceNumber, Paymentid = model.PaymentMethodId.Value });
                            }
                            else
                            {
                                return RedirectToAction("failed_url", "Paypal");
                            }

                            //TempData["PaymentMethodId"] = model.PaymentMethodId;
                            //return RedirectToAction("makepayment", new { id = summary.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid) });

                        }
                        else
                            return RedirectToAction("PaymentDetail", new { id = summary.Id });
                    }
                    else
                    {
                        return RedirectToAction("SummaryDetail");
                    }
                }
                else
                {
                    return RedirectToAction("SummaryDetail");
                }

            }
            catch (Exception ex)
            {
                return RedirectToAction("SummaryDetail");
            }
        }


        protected PayNowModel PayNow(int summaryId, string invoiceNumber, int paymentId, decimal totalPremium)
        {
            PayNowModel payNowModel = new PayNowModel();
            Insurance.Service.EmailService log = new Insurance.Service.EmailService();
            try
            {

                string Integration_ID = ConfigurationManager.AppSettings["PayNowIntegration_ID"];
                string Integration_Key = ConfigurationManager.AppSettings["PayNowIntegration_Key"];
                string urlPath = ConfigurationManager.AppSettings["urlPath"];

                // log.WriteLog("PayNow Integration_ID:" + Integration_ID);

                // log.WriteLog("PayNow Integration_Key:" + Integration_Key);

                var paynow = new Paynow(Integration_ID, Integration_Key);
                paynow.ReturnUrl = urlPath + "/Renew/SaveDetailList?id=" + summaryId + "&invoiceNumer=" + invoiceNumber + "&Paymentid=" + paymentId;

               // log.WriteLog("PayNow execute:");

                var uniqueTransaction = InsuranceContext.Query("select top  1 * from UniqeTransaction order by id desc").
                Select(c => new UniqeTransaction()
                {
                    UniqueTransactionId = c.UniqueTransactionId
                }).FirstOrDefault();

                UniqeTransaction transaction = new UniqeTransaction { UniqueTransactionId = uniqueTransaction.UniqueTransactionId + 1, CreatedOn = DateTime.Now };
                InsuranceContext.UniqeTransactions.Insert(transaction);

                payNowModel.TransactionId = uniqueTransaction.UniqueTransactionId.ToString();


                var payment = paynow.CreatePayment(payNowModel.TransactionId);
                payment.Add("gene Insurance", totalPremium);
                //payment.Add("Apples", Convert.ToDecimal(3.4));

                paynow.Send(payment);

                var response = paynow.Send(payment);

                if (response.Success())
                {
                    payNowModel.IsSuccessPayment = true;
                    // Get the url to redirect the user to so they can make payment
                    // var link = response.RedirectLink();
                    payNowModel.ReturnUrl = response.RedirectLink();

                    // Get the poll url of the transaction
                    // var pollUrl = response.PollUrl();
                    payNowModel.PollUrl = response.PollUrl();

                    //Response.Redirect(link);
                }

            }
            catch (Exception ex)
            {
                log.WriteLog("PayNow execute:" + ex.Message);
            }

            return payNowModel;
        }


        public string GetHighestPolicyNumber(int policyId)
        {
            string renewPolicyNuber = "";

            var renewPolicyNumberDetials = InsuranceContext.Query("select  max(RenewPolicyNumber) RenewPolicyNumber  from VehicleDetail where PolicyId=" + policyId).Select(x => new RiskDetailModel()
            {
                RenewPolicyNumber = x.RenewPolicyNumber
            }).FirstOrDefault();


            if (renewPolicyNumberDetials != null)
            {
                renewPolicyNuber = renewPolicyNumberDetials.RenewPolicyNumber;
            }

            return renewPolicyNuber;
        }


        public ActionResult makepayment(Int32 id, decimal TotalPremiumPaid)
        {
            Dictionary<string, dynamic> responseData;
            string data = "authentication.userId=8a8294175698883c01569ce4c4212119" +
                "&authentication.password=Mc2NMzf8jM" +
                "&authentication.entityId=8a8294175698883c01569ce4c3972115" +
                "&amount=" + TotalPremiumPaid + "" +
                "&currency=USD" +
                "&paymentType=DB";


            //string data = "authentication.userId=8a8294175698883c01569ce4c4212119" +
            //   "&authentication.password=Mc2NMzf8jM" +
            //   "&authentication.entityId=8a8294175698883c01569ce4c3972115" +
            //   "&amount="+TotalPremiumPaid+"" +
            //   "&currency=USD" +
            //   "&paymentType=DB";
            string url = "https://test.oppwa.com/v1/checkouts";
            byte[] buffer = Encoding.ASCII.GetBytes(data);

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
           | SecurityProtocolType.Tls11
           | SecurityProtocolType.Tls12
           | SecurityProtocolType.Ssl3;


            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            Stream PostData = request.GetRequestStream();
            PostData.Write(buffer, 0, buffer.Length);
            PostData.Close();
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                var s = new JavaScriptSerializer();
                responseData = s.Deserialize<Dictionary<string, dynamic>>(reader.ReadToEnd());
                reader.Close();
                dataStream.Close();
            }

            if (responseData["result"]["description"].Contains("successfull"))
            {
                ViewBag.checkoutId = Convert.ToString(responseData["id"]);

                TempData["ID"] = id;
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }
        }


        public ActionResult returnurl()
        {
            var id = HttpContext.Request.QueryString["id"];
            Dictionary<string, dynamic> responseData;
            string data = "authentication.userId=8a8294175698883c01569ce4c4212119" +
                "&authentication.password=Mc2NMzf8jM" +
                "&authentication.entityId=8a8294175698883c01569ce4c3972115";
            string url = $"https://test.oppwa.com/v1/checkouts/{id}/payment?" + data;
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                var s = new JavaScriptSerializer();
                responseData = s.Deserialize<Dictionary<string, dynamic>>(reader.ReadToEnd());
                reader.Close();
                dataStream.Close();
            }

            if (responseData["result"]["description"].Contains("successfull"))
            {
                var InvoiceId = responseData["id"];
                int Summaryid = Convert.ToInt32(TempData["ID"]);
                string PaymentId = Convert.ToString(TempData["PaymentMethodId"]);
                //var result = new PaypalController().SaveDetailList(Summaryid, InvoiceId);
                return RedirectToAction("SaveDetailList", "Renew", new { id = Summaryid, invoiceNumber = InvoiceId, PaymentId = PaymentId });
            }
            else
            {
                return RedirectToAction("PaymentFailure");
            }
        }

        public ActionResult PaymentFailure()
        {
            return View();
        }


        public ActionResult VehicleHistory()
        {

            SummaryDetailService _summaryDetailService = new SummaryDetailService();

            var currenyList = _summaryDetailService.GetAllCurrency();


            //List<VehicleDetail> vehicles = new List<VehicleDetail>();
            //vehicles = InsuranceContext.VehicleDetails.All().Where(x => x.IsActive == false).ToList();


            //   var list = InsuranceContext.Query("select PolicyId, RegistrationNo,Premium, VehicleMake.MakeDescription as makeId, VehicleModel.modeldescription as modelId from vehicledetail join VehicleMake on VehicleDetail.MakeId = VehicleMake.Makecode join VehicleModel on vehicledetail.modelId = vehiclemodel.modelcode where vehicledetail.Isactive=0")
            var list = InsuranceContext.Query("select vehicledetail.CurrencyId, PolicyId, RegistrationNo,Premium, VehicleMake.MakeDescription as makeId, VehicleModel.modeldescription as modelId, PolicyDetail.PolicyNumber, Customer.FirstName,Customer.LastName from vehicledetail join VehicleMake on VehicleDetail.MakeId = VehicleMake.Makecode join VehicleModel on vehicledetail.modelId = vehiclemodel.modelcode join Policydetail on vehicledetail.PolicyId=Policydetail.Id join customer on vehicledetail.customerId=customer.Id where vehicledetail.Isactive=0")
            .Select(x => new VehicleDetail()
            {
                EngineNumber = x.PolicyNumber,
                ChasisNumber = x.FirstName + " " + x.LastName,
                RegistrationNo = x.RegistrationNo,
                MakeId = x.makeId,
                ModelId = x.modelId,
                Premium = x.Premium,
                Currency = _summaryDetailService.GetCurrencyName(currenyList, x.CurrencyId)

            }).ToList();

            return View(list);
        }
        public SummaryDetailModel SummaryDetailsCalculation(SummaryDetailModel model)
        {

            var summary = new SummaryDetailService();

            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();
            List<RiskDetailModel> vehicleList = new List<RiskDetailModel>();
            if (model.Id != 0)
            {
                model.CustomSumarryDetilId = model.Id;
                //vehicle = summary.GetVehicleInformation(id);
                var summaryVichalList = InsuranceContext.SummaryVehicleDetails.All(where: $" SummaryDetailId='{model.Id}'");

                foreach (var item in summaryVichalList)
                {
                    //  var vehicleDetails = InsuranceContext.VehicleDetails.Single(item.VehicleDetailsId);
                    var vehicleDetails = InsuranceContext.VehicleDetails.Single(where: $"Id='{ item.VehicleDetailsId }' and IsActive<>0 ");

                    if (vehicleDetails != null)
                    {
                        RiskDetailModel vehicleModel = Mapper.Map<VehicleDetail, RiskDetailModel>(vehicleDetails);
                        vehicleList.Add(vehicleModel);
                    }
                }
            }

            var DiscountSettings = InsuranceContext.Settings.Single(where: $"keyname='Discount On Renewal'");
            model.CarInsuredCount = vehicleList.Count;
            model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());

            model.PaymentMethodId = model.PaymentMethodId;

            //default selection 
            //if (User.IsInRole("Staff"))
            //{
            //    model.PaymentMethodId = 1;
            //}
            //else
            //{
            //    model.PaymentMethodId = 2;
            //}


            model.PaymentTermId = model.PaymentTermId;
            model.ReceiptNumber = "";
            model.SMSConfirmation = false;
            //model.TotalPremium = vehicle.Sum(item => item.Premium + item.ZTSCLevy + item.StampDuty + item.RadioLicenseCost);
            model.TotalPremium = 0.00m;
            model.TotalRadioLicenseCost = 0.00m;
            model.Discount = 0.00m;
            foreach (var item in vehicleList)
            {
                model.TotalPremium += item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee;
                if (item.IncludeRadioLicenseCost)
                {
                    model.TotalPremium += item.RadioLicenseCost;
                    model.TotalRadioLicenseCost += item.RadioLicenseCost;
                }
                model.Discount += item.Discount;

            }
            model.TotalRadioLicenseCost = Math.Round(Convert.ToDecimal(model.TotalRadioLicenseCost), 2);
            model.Discount = Math.Round(Convert.ToDecimal(model.Discount), 2);
            model.TotalPremium = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            model.TotalStampDuty = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.StampDuty)), 2);
            model.TotalSumInsured = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.SumInsured)), 2);
            model.TotalZTSCLevies = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.ZTSCLevy)), 2);
            model.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.ExcessBuyBackAmount)), 2);
            model.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.MedicalExpensesAmount)), 2);
            model.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.PassengerAccidentCoverAmount)), 2);
            model.RoadsideAssistanceAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.RoadsideAssistanceAmount)), 2);
            model.ExcessAmount = Math.Round(Convert.ToDecimal(vehicleList.Sum(item => item.ExcessAmount)), 2);
            model.AmountPaid = 0.00m;
            model.MaxAmounttoPaid = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
            var vehiclewithminpremium = vehicleList.OrderBy(x => x.Premium).FirstOrDefault();

            if (vehiclewithminpremium != null)
            {
                model.MinAmounttoPaid = Math.Round(Convert.ToDecimal(vehiclewithminpremium.Premium + vehiclewithminpremium.StampDuty + vehiclewithminpremium.ZTSCLevy + (Convert.ToBoolean(vehiclewithminpremium.IncludeRadioLicenseCost) ? vehiclewithminpremium.RadioLicenseCost : 0.00m)), 2);
            }

            model.AmountPaid = Convert.ToDecimal(model.TotalPremium);
            model.BalancePaidDate = DateTime.Now;
            model.Notes = "";
            model.Id = model.Id;

            if (Session["RePolicyData"] != null)
            {
                var PolicyData = (PolicyDetail)Session["RePolicyData"];
                model.InvoiceNumber = PolicyData.PolicyNumber;
            }

            return model;
        }


        [HttpPost]
        public JsonResult CalculatePremium(int vehicleUsageId, decimal sumInsured, int coverType, int excessType, decimal excess, decimal? AddThirdPartyAmount, int NumberofPersons, Boolean Addthirdparty, Boolean PassengerAccidentCover, Boolean ExcessBuyBack, Boolean RoadsideAssistance, Boolean MedicalExpenses, decimal? RadioLicenseCost, Boolean IncludeRadioLicenseCost, int policytermid)
        {
            //var policytermid = (int)Session["policytermid"];
            JsonResult json = new JsonResult();
            var quote = new QuoteLogic();
            var typeCover = eCoverType.Comprehensive;
            if (coverType == 2)
            {
                typeCover = eCoverType.ThirdParty;
            }
            if (coverType == 3)
            {
                typeCover = eCoverType.FullThirdParty;
            }
            var eexcessType = eExcessType.Percentage;
            if (excessType == 2)
            {
                eexcessType = eExcessType.FixedAmount;
            }
            var premium = quote.CalculatePremium(vehicleUsageId, sumInsured, typeCover, eexcessType, excess, policytermid, AddThirdPartyAmount, NumberofPersons, Addthirdparty, PassengerAccidentCover, ExcessBuyBack, RoadsideAssistance, MedicalExpenses, RadioLicenseCost, IncludeRadioLicenseCost, false, "", "", "");
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            json.Data = premium;
            return json;
        }

        public JsonResult GetVehicleModel(string makeCode)
        {
            var service = new VehicleService();
            var model = service.GetModel(makeCode);
            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = model;
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }

        public ActionResult PaymentDetail(int id, string erroMsg = null)
        {
            var cardDetails = (CardDetailModel)Session["CardDetail"];
            if (cardDetails == null)
            {
                cardDetails = new CardDetailModel();
            }
            cardDetails.SummaryDetailId = id;

            TempData["ErrorMsg"] = erroMsg;

            return View(cardDetails);
        }
        public async Task<ActionResult> SaveDetailList(Int32 id, string invoiceNumber = "", string paymentId = "")
        {

            if (Session["PollUrl"] != null)
            {
                string Integration_ID = System.Configuration.ConfigurationManager.AppSettings["PayNowIntegration_ID"];
                string Integration_Key = System.Configuration.ConfigurationManager.AppSettings["PayNowIntegration_Key"];

                var paynow = new Webdev.Payments.Paynow(Integration_ID, Integration_Key);
                var status = paynow.PollTransaction((string)Session["PollUrl"]);
                if (!status.Paid())
                {
                    return RedirectToAction("failed_url", "paypal");
                }
            }

            var vehicleId = (Int32)Session["RenewVehicleId"];
           // var PaymentId = Session["PaymentId"];
            var InvoiceId = Session["InvoiceId"];
            var summary = InsuranceContext.SummaryDetails.Single(id);
            var policy = (PolicyDetail)Session["RenewVehiclePolicy"];
            var DebitNote = summary.DebitNote;
            var vehicle = InsuranceContext.VehicleDetails.Single(vehicleId);
            PaymentInformation objSaveDetailListModel = new PaymentInformation();
            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];

            SummaryDetailService detailService = new SummaryDetailService();

            var email = "";
            var currencylist = detailService.GetAllCurrency();
            var currencyName = "";


            //Generate QR Code
            var path = SaveQRCode(vehicle.RenewPolicyNumber);

            var customer = InsuranceContext.Customers.Single(summary.CustomerId.Value);
            var user = UserManager.FindById(customer.UserID);

            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (Session["RenewVehicleDetails"] != null)
            {

                //vehicle.isLapsed = true;
                //InsuranceContext.VehicleDetails.Update(vehicle);

                //var summaryvehicledetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"SummaryDetailId={summary.Id} and VehicleDetailsId={vehicleId}");               
                //InsuranceContext.SummaryVehicleDetails.Delete(summaryvehicledetail);

                var _item = (RiskDetailModel)Session["RenewVehicleDetails"];
                //var product = InsuranceContext.Products.Single(Convert.ToInt32(_item.ProductId));
                currencyName = detailService.GetCurrencyName(currencylist, _item.CurrencyId);
                objSaveDetailListModel.CurrencyId = policy.CurrencyId;
                objSaveDetailListModel.PolicyId = policy.Id;
                objSaveDetailListModel.VehicleDetailId = _item.Id;
                objSaveDetailListModel.CustomerId = summary.CustomerId.Value;
                objSaveDetailListModel.SummaryDetailId = id;
                objSaveDetailListModel.DebitNote = summary.DebitNote;
                objSaveDetailListModel.ProductId = _item.ProductId;
                objSaveDetailListModel.PaymentId = paymentId;
                objSaveDetailListModel.InvoiceId = InvoiceId == null ? "" : InvoiceId.ToString();
                objSaveDetailListModel.InvoiceNumber = policy.PolicyNumber;
                objSaveDetailListModel.CreatedOn = DateTime.Now;

                if(Session["PollUrl"]!=null)
                    objSaveDetailListModel.PollURL = Convert.ToString(Session["PollUrl"]);
                
                

               
                if (_userLoggedin)
                {
                    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                    var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();
                    if (_customerData != null)
                    {
                        objSaveDetailListModel.CreatedBy = _customerData.Id;
                    }
                }


                if (customer.IsCustomEmail)
                    email = LoggedUserEmail();
                else
                    email = user.Email;


                InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);

                //  MiscellaneousService.AddLoyaltyPoints(summary.CustomerId.Value, policy.Id, _item, email); // disable 02_sep_2019
            }
            else
            {

                DateTime NewRenewalDate = DateTime.Now;

                switch (vehicle.PaymentTermId)
                {
                    case 1:
                        NewRenewalDate = vehicle.RenewalDate.Value.AddYears(1);
                        break;
                    case 3:
                        NewRenewalDate = vehicle.RenewalDate.Value.AddMonths(3);
                        break;
                    case 4:
                        NewRenewalDate = vehicle.RenewalDate.Value.AddMonths(4);
                        break;
                }

                vehicle.RenewalDate = NewRenewalDate;
                InsuranceContext.VehicleDetails.Update(vehicle);

                //var product = InsuranceContext.Products.Single(Convert.ToInt32(vehicle.ProductId));

                objSaveDetailListModel.CurrencyId = policy.CurrencyId;
                objSaveDetailListModel.PolicyId = policy.Id;
                objSaveDetailListModel.VehicleDetailId = vehicleId;
                objSaveDetailListModel.CustomerId = summary.CustomerId.Value;
                objSaveDetailListModel.SummaryDetailId = id;
                objSaveDetailListModel.DebitNote = summary.DebitNote;
                objSaveDetailListModel.ProductId = vehicle.ProductId;
                objSaveDetailListModel.PaymentId = paymentId ;
                objSaveDetailListModel.InvoiceId = InvoiceId == null ? "" : InvoiceId.ToString();
                InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);
                MiscellaneousService.AddLoyaltyPoints(summary.CustomerId.Value, policy.Id, Mapper.Map<VehicleDetail, RiskDetailModel>(vehicle));
            }


            ReceiptDeliveryModule detail = new ReceiptDeliveryModule();
            detail.customerFirstName = customer.FirstName;
            detail.customerLastName = customer.LastName;


            if (vehicle.IsLicenseDiskNeeded == true)
            {
                var licenseDelivery = InsuranceContext.LicenceDiskDeliveryAddresses.Single(where: "vehicleId=" + vehicle.Id);
                if (licenseDelivery != null)
                {
                    detail.addressLine1 = licenseDelivery.Address1;
                    detail.addressLine2 = licenseDelivery.Address2;
                    detail.zoneName = licenseDelivery.Address2;
                    detail.city = licenseDelivery.City;
                }
            }
            else
            {
                detail.addressLine1 = "Self";
                detail.addressLine2 = "Pick";
                detail.zoneName = "Pick";
                detail.city = "Harare";
            }


            detail.phoneNumber = customer.PhoneNumber;
            detail.policyID = policy.PolicyNumber;
            detail.policyTransactionDate = vehicle.TransactionDate.Value.ToShortDateString();
            detail.policyAmount = summary.TotalPremium.Value;

            if (_userLoggedin)
            {
                detail.agentID = summary.CreatedBy.ToString();
                var customerDetial = InsuranceContext.Customers.Single(summary.CreatedBy);
                detail.agentName = customerDetial.FirstName + ' ' + customerDetial.LastName;
            }

            VehicleService vehicleService = new VehicleService();
            vehicleService.SaveDeliveryAddress(detail);


            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();

            //var data = (List<Item>)Session["itemData"];
            //if (data != null)
            //{
            // var totalprem = data.Sum(x => Convert.ToDecimal(x.price));

            string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/Reciept.cshtml";
            string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));

            var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString())
                .Replace("#QRpath#", path).Replace("##path##", filepath).Replace("#FirstName#", customer.FirstName).Replace("#currencyName#", currencyName).Replace("#LastName#", customer.LastName).Replace("#AccountName#", customer.FirstName + ", " + customer.LastName).Replace("#Address1#", customer.AddressLine1).
                Replace("#Address2#", customer.AddressLine2).Replace("#Amount#", Convert.ToString(CalCulateVehicleTotalPremium(vehicle))).Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", policy.PolicyNumber).
                Replace("#PaymentType#", (summary.PaymentMethodId == 1 ? "Cash" : (summary.PaymentMethodId == 2 ? "PayPal" : "PayNow")));

            var attachementPath = MiscellaneousService.EmailPdf(Body2, policy.CustomerId, policy.PolicyNumber, "Renew Invoice ");

            List<string> _attachements = new List<string>();
            _attachements.Add(attachementPath);



            if (customer.IsCustomEmail) // if customer has custom email
            {
                objEmailService.SendEmail(LoggedUserEmail(), "", "", "Renew " + policy.PolicyNumber + " : Invoice", Body2, _attachements);
            }
            else
            {
                objEmailService.SendEmail(user.Email, "", "", "Renew " + policy.PolicyNumber + " : Invoice", Body2, _attachements);
            }


            #region Send Payment SMS

            // done

            

            string Recieptbody = "Hello " + customer.FirstName + "\nWelcome to GeneInsure. Please pay " + "$" + Convert.ToString(summary.TotalPremium) + " upon receiving your policy to merchant code 249341. Policy number is : " + policy.PolicyNumber + "\n" + "\nThanks.";
            var Recieptresult = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber, Recieptbody);

            SmsLog objRecieptsmslog = new SmsLog()
            {
                Sendto = user.PhoneNumber,
                Body = Recieptbody,
                Response = Recieptresult,
                CreatedBy = customer.Id,
                CreatedOn = DateTime.Now
            };

            InsuranceContext.SmsLogs.Insert(objRecieptsmslog);

            #endregion


            decimal totalpaymentdue = 0.00m;

            RenewApproveVRNToIceCash(customer, vehicle,  Convert.ToInt32(summary.PaymentMethodId)); // need to uncomment

            string Summeryofcover = "";

            var paymentMonths = vehicle.PaymentTermId.ToString() == "1" ? "Yearly" : vehicle.PaymentTermId + " Months";

            var productDetail = InsuranceContext.Products.Single(Convert.ToInt32(vehicle.ProductId));
            var taxClassDetials = InsuranceContext.VehicleTaxClasses.Single(vehicle.TaxClassId);
            decimal? premiumDue = vehicle.Premium + vehicle.StampDuty + vehicle.ZTSCLevy + vehicle.VehicleLicenceFee + vehicle.RadioLicenseCost;


            if (Session["RenewVehicleDetails"] != null)
            {
                var _vehicle = (RiskDetailModel)Session["RenewVehicleDetails"];
                var _Premium = (vehicle.Premium + vehicle.Discount) - vehicle.PassengerAccidentCoverAmount - vehicle.ExcessAmount - vehicle.ExcessBuyBackAmount - vehicle.MedicalExpensesAmount - vehicle.RoadsideAssistanceAmount;


                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_vehicle.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{_vehicle.MakeId}'");

                string vehicledescription = "";

                if (model != null && make != null)
                {
                    vehicledescription = model.ModelDescription + " / " + make.MakeDescription;
                }

                string coverType = "";

                if (vehicle.CoverTypeId == 1)
                    coverType = eCoverType.ThirdParty.ToString();
                if (vehicle.CoverTypeId == 2)
                    coverType = eCoverType.FullThirdParty.ToString();

                if (vehicle.CoverTypeId == 4)
                    coverType = eCoverType.Comprehensive.ToString();
              //  var paymentMonths = vehicle.PaymentTermId.ToString() == "1" ? "Yearly" : vehicle.PaymentTermId + " Months";


                string policyPeriod = vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + vehicle.CoverEndDate.Value.ToString("dd/MM/yyyy");
                currencyName = detailService.GetCurrencyName(currencylist, vehicle.CurrencyId);

                // new 

                Summeryofcover += "<table border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse; width:100%;  border-color:#ffcc00; border-style:solid;' >";
                Summeryofcover += "<tr> <th  bgcolor='#D3D3D3' colspan='2' style='background:#D3D3D3; color:#000;text-align: center; padding:10px;   word-break: break-all;'> <font size='1'>Gene-Insure</font> </th> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Cover Note #: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.CoverNote + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px; '> <font size='1'> Transaction Date: </font> </td> <td style='padding: 1px 10px; '> <font size='1'>" + vehicle.TransactionDate.Value.ToShortDateString() + " </font> </td> </tr>";
                Summeryofcover += " </table>";

                Summeryofcover += "<table  border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse; width:100% border-color:#ffcc00; border-style:solid;' >";
                Summeryofcover += "<tr> <th  bgcolor='#D3D3D3' colspan='2' style='background:#D3D3D3; color:#000;text-align: center; padding:1px;   word-break: break-all;'> <font size='1'>Certificate of Motor Insurance</font>  </th> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Insurance Type: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>Road Traffic Act </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle Type: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + (vehicle.CoverTypeId == 4 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + " " + InsuranceContext.VehicleUsages.All(Convert.ToString(vehicle.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Start Date: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy") + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> End Date: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.CoverEndDate.Value.ToString("dd/MM/yyyy") + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Period: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + paymentMonths + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Premium: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.Premium + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Gvt Levy: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.ZTSCLevy + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Stamp Duty: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.StampDuty + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Premium Due: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + Math.Round(Convert.ToDecimal(premiumDue), 2) + " </font> </td> </tr>";
                Summeryofcover += " </table>";

                // #ddd
                Summeryofcover += "<table  border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse; width:100%;  border-color:#ffcc00; border-style:solid' >";
                Summeryofcover += "<tr> <th  bgcolor='#D3D3D3' colspan='2' style='background:#D3D3D3; color:#000;text-align: center; padding:10px;  word-break: break-all;'> <font size='1'>Vehicle Details</font> </th> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle Reg. Number: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.RegistrationNo + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle Type: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + productDetail.ProductName + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Tax Class: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + taxClassDetials.Description + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px; font-size:15px;'> <font size='1'> Sum Insured: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.SumInsured + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle: </font> </td> <td style='padding: 5px 10px;'> <font size='1'>" + vehicledescription + " </font> </td> </tr>";
                Summeryofcover += " </table> ";




                // Summeryofcover += "<tr><td>" + vehicle.RegistrationNo + "</td> <td>" + vehicledescription + "</td> <td> " + vehicle.CoverNote + " </td>  <td>" + currencyName + vehicle.SumInsured + "</td><td>" + coverType + "</td><td>" + InsuranceContext.VehicleUsages.All(Convert.ToString(vehicle.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</td><td>" + policyPeriod + "</td><td>" + paymentMonths + " </td><td>" + currencyName + Convert.ToString(_Premium) + "</td></tr>";


            }
            else
            {
                var _Premium = vehicle.Premium - vehicle.PassengerAccidentCoverAmount - vehicle.ExcessAmount - vehicle.ExcessBuyBackAmount - vehicle.MedicalExpensesAmount - vehicle.RoadsideAssistanceAmount;
                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{vehicle.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{vehicle.MakeId}'");

                string vehicledescription = "";

                if (model != null && make != null)
                {
                    vehicledescription = model.ModelDescription + " / " + make.MakeDescription;
                }

                string coverType = "";

                if (vehicle.CoverTypeId == 1)
                    coverType = eCoverType.ThirdParty.ToString();
                if (vehicle.CoverTypeId == 2)
                    coverType = eCoverType.FullThirdParty.ToString();

                if (vehicle.CoverTypeId == 4)
                    coverType = eCoverType.Comprehensive.ToString();

                
                string policyPeriod = vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + vehicle.CoverEndDate.Value.ToString("dd/MM/yyyy");
                currencyName = detailService.GetCurrencyName(currencylist, vehicle.CurrencyId);


                Summeryofcover += "<table border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse; width:100%;  border-color:#ffcc00; border-style:solid;' >";
                Summeryofcover += "<tr> <th  bgcolor='#D3D3D3' colspan='2' style='background:#D3D3D3; color:#000;text-align: center; padding:10px;   word-break: break-all;'> <font size='1'>Gene-Insure</font> </th> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Cover Note #: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.CoverNote + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px; '> <font size='1'> Transaction Date: </font> </td> <td style='padding: 1px 10px; '> <font size='1'>" + vehicle.TransactionDate.Value.ToShortDateString() + " </font> </td> </tr>";
                Summeryofcover += " </table>";

                Summeryofcover += "<table  border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse; width:100% border-color:#ffcc00; border-style:solid;' >";
                Summeryofcover += "<tr> <th  bgcolor='#D3D3D3' colspan='2' style='background:#D3D3D3; color:#000;text-align: center; padding:1px;   word-break: break-all;'> <font size='1'>Certificate of Motor Insurance</font>  </th> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Insurance Type: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>Road Traffic Act </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle Type: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + (vehicle.CoverTypeId == 4 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + " " + InsuranceContext.VehicleUsages.All(Convert.ToString(vehicle.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Start Date: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy") + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> End Date: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.CoverEndDate.Value.ToString("dd/MM/yyyy") + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Period: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + paymentMonths + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Premium: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.Premium + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Gvt Levy: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.ZTSCLevy + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Stamp Duty: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.StampDuty + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Premium Due: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + Math.Round(Convert.ToDecimal(premiumDue), 2) + " </font> </td> </tr>";
                Summeryofcover += " </table>";

                // #ddd
                Summeryofcover += "<table  border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse; width:100%;  border-color:#ffcc00; border-style:solid' >";
                Summeryofcover += "<tr> <th  bgcolor='#D3D3D3' colspan='2' style='background:#D3D3D3; color:#000;text-align: center; padding:10px;  word-break: break-all;'> <font size='1'>Vehicle Details</font> </th> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle Reg. Number: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.RegistrationNo + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle Type: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + productDetail.ProductName + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Tax Class: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + taxClassDetials.Description + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px; font-size:15px;'> <font size='1'> Sum Insured: </font> </td> <td style='padding: 1px 10px;'> <font size='1'>" + vehicle.SumInsured + " </font> </td> </tr>";
                Summeryofcover += "<tr> <td style='padding: 1px 10px;'> <font size='1'> Vehicle: </font> </td> <td style='padding: 5px 10px;'> <font size='1'>" + vehicledescription + " </font> </td> </tr>";
                Summeryofcover += " </table> ";


                //  Summeryofcover += "<tr><td>" + vehicle.RegistrationNo + "</td> <td>" + vehicledescription + "</td><td>" + currencyName + vehicle.SumInsured + "</td><td>" + coverType + "</td><td>" + InsuranceContext.VehicleUsages.All(Convert.ToString(vehicle.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</td><td>" + policyPeriod + "</td><td>" + paymentMonths + " </td><td>" + currencyName + Convert.ToString(_Premium) + "</td></tr>";

            }


            string AgentDetials = "";

            //if (User.IsInRole("Staff") || User.IsInRole("Renewals"))
            //{

            //    VehicleService vehicleService = new VehicleService();
            //    var agentDetail = vehicleService.GetAgentDetails(summary, System.Web.HttpContext.Current.User.Identity.GetUserName());

            //    AgentDetials += "<table width='565' border='1' cellspacing='0' cellpadding='5' style='border - collapse:collapse; border: 1px solid #000; width:900px;'>";
            //    AgentDetials += "<tr> <td bgcolor='#000' colspan ='2' style = 'background:#000; color: white;text-align: center; padding:10px; font-size:16px;  word-break: break-all;' >< font size = '3' > INSURED PARTY DETAILS</ font ></td></tr>";
            //    AgentDetials += "<tr> <td style='padding: 7px 10px; font - size:15px;'> Name:  </td> <td style='padding: 7px 10px; font - size:15px;'>" + agentDetail.FirstName + " " + agentDetail.LastName + " </td></tr>";
            //    AgentDetials += "<tr> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>Email: </font></td> <td style='padding: 7px 10px; font - size:15px;'> " + agentDetail.EmailAddress + " </td> </tr>";
            //    AgentDetials += " <tr> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>Mobile: </font></td> <td style='padding: 7px 10px; font - size:15px;'> " + agentDetail.PhoneNumber + " </td> </tr>";
            //    AgentDetials += " <tr> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>Address: </font></td> <td style='padding: 7px 10px; font - size:15px;'> " + agentDetail.AddressLine1 + " " + agentDetail.City + " </td></tr>";
            //    AgentDetials += " <tr> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>ID Number: </font></td> <td style='padding: 7px 10px; font - size:15px;'> " + agentDetail.NationalIdentificationNumber + " </td></tr> </table> ";

            //}


            //var Premium = vehicle.Premium + vehicle.PassengerAccidentCoverAmount + vehicle.ExcessAmount + vehicle.ExcessBuyBackAmount + vehicle.MedicalExpensesAmount + vehicle.RoadsideAssistanceAmount ;

            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };
            var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == summary.PaymentTermId);
            string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/ScheduleMotorRenew.cshtml";
            string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));
            //var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (summary.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + summary.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(totalpaymentdue)).Replace("##StampDuty##", Convert.ToString(summary.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(summary.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(summary.TotalPremium)).Replace("##PostalAddress##", customer.Zipcode);

            var paymentTerms = summary.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + summary.PaymentTermId.ToString() + " Months)";


            decimal radioLicenseAmount = 0;
            if (vehicle.IncludeRadioLicenseCost.Value)
                radioLicenseAmount = vehicle.RadioLicenseCost.Value;

            var totalPremium = vehicle.Premium + vehicle.ZTSCLevy + vehicle.StampDuty + vehicle.VehicleLicenceFee + radioLicenseAmount;


            //  var Bodyy = MotorBody.Replace("##Summeryofcover##", Summeryofcover).Replace("##path##", filepath);

            var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber).Replace("##NINumber##", customer.NationalIdentificationNumber).Replace("##ReNewPolicyNo##", vehicle.RenewPolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", vehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).
                Replace("##PaymentTerm##", paymentTerms)
                .Replace("##currencyName##", currencyName)
               .Replace("##TransactionDate##", vehicle.TransactionDate.Value.ToShortDateString())
                .Replace("##AgentDetials##", AgentDetials)
                  .Replace("##ExcessAmount##", Convert.ToString(vehicle.ExcessAmount))
                .Replace("##Discount##", Convert.ToString(vehicle.Discount))
                .Replace("##RadioLicence##", Convert.ToString(vehicle.RadioLicenseCost))
                .Replace("##TotalPremiumDue##", Convert.ToString(totalPremium))
                .Replace("##StampDuty##", Convert.ToString(summary.TotalStampDuty))
                .Replace("##MotorLevy##", Convert.ToString(summary.TotalZTSCLevies))
                .Replace("##PremiumDue##", Convert.ToString(vehicle.Premium))
                .Replace("##PostalAddress##", customer.Zipcode)
                .Replace("##ExcessBuyBackAmount##", Convert.ToString(vehicle.ExcessBuyBackAmount))
                .Replace("##MedicalExpenses##", Convert.ToString(vehicle.MedicalExpensesAmount)).
                Replace("##PassengerAccidentCover##", Convert.ToString(vehicle.PassengerAccidentCoverAmount))
                .Replace("##VehicleLicenceFee##", Convert.ToString(vehicle.VehicleLicenceFee))
                .Replace("##PenaltiesAmt##", Convert.ToString(vehicle.PenaltiesAmt))
                .Replace("##QRpath##", path)
                .Replace("##RoadsideAssistance##", Convert.ToString(vehicle.RoadsideAssistanceAmount));

            var attachemetPath = MiscellaneousService.EmailPdf(Bodyy, policy.CustomerId, policy.PolicyNumber, "Renew Schedule-motor");
            List<string> attachements = new List<string>();

            attachements.Add(attachemetPath);

            var Atter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
            attachements.Add(Atter);


            if (customer.IsCustomEmail) // if customer has custom email
                objEmailService.SendEmail(LoggedUserEmail(), "", "", "Renew " + policy.PolicyNumber + " :Renew-Schedule-motor", Bodyy, attachements);
            else
                objEmailService.SendEmail(user.Email, "", "", "Renew " + policy.PolicyNumber + " :Renew-Schedule-motor", Bodyy, attachements);


            #region Renew policy

            var paymentType= summary.PaymentMethodId == 1 ? "Cash" : (summary.PaymentMethodId == 2 ? "PayPal" : "PayNow");
            List<VehicleDetail> ListOfVehicles = new List<VehicleDetail>();
            ListOfVehicles.Add(vehicle);
            //MiscellaneousService.SendEmailNewPolicy(customer.FirstName + " " + customer.LastName, customer.AddressLine1, customer.AddressLine2, policy.PolicyNumber, summary.TotalPremium, summary.PaymentTermId, paymentType, ListOfVehicles, "renew");

            #endregion

            //MiscellaneousService.ScheduleMotorPdf(Bodyy, policy.CustomerId, policy.PolicyNumber, "Renew-Schedule-motor");


            #region Send License PDFVerficationcode SMS

            var IswebCustomer = User.IsInRole("Web Customer");

            if (_pdfPath != "" && _pdfCode != "" && IswebCustomer==true)
            {
                string RecieptbodyPdf = "Hello " + customer.FirstName + "\nWelcome to GeneInsure. Your license pdf verifation code is: " + _pdfCode + "\n" + "\nThanks.";
                var RecieptresultPdf = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber, RecieptbodyPdf);
                SmsLog objRecieptsmslogPdf = new SmsLog()
                {
                    Sendto = user.PhoneNumber,
                    Body = RecieptbodyPdf,
                    Response = RecieptresultPdf,
                    CreatedBy = customer.Id,
                    CreatedOn = DateTime.Now
                };
                InsuranceContext.SmsLogs.Insert(objRecieptsmslog);
            }
            #endregion


            //Session.Remove("policytermid");
            //Session.Remove("RenewVehicleId");
            //Session.Remove("RenewPaymentId");
            //Session.Remove("RenewInvoiceId");
            //Session.Remove("RenewVehicleSummary");
            //Session.Remove("RenewVehiclePolicy");
            //Session.Remove("RenewVehicle");
            //Session.Remove("RenewVehicleDetails");
            //Session.Remove("RenewCardDetail");
            //Session.Remove("ReSummaryDetailed");
            //Session.Remove("CheckRenewVehicleDetails");

            ClearRenewSession();

           
            if (_pdfPath != "" && !IswebCustomer)
                ViewBag.file = System.Configuration.ConfigurationManager.AppSettings["urlPath"] + _pdfPath;


            var IsStaff = User.IsInRole("Staff");
            if (IsStaff && _pdfPath != "")
            {
                return RedirectToAction("CertificateSerialNumber", "CustomerRegistration", new { VehicleId = vehicle.Id });
            }


            return View(objSaveDetailListModel);
        }

        public decimal? CalCulateVehicleTotalPremium(VehicleDetail vehicleDetial)
        {

            decimal? vehilcleTotalPremium = 0;


            vehilcleTotalPremium = vehicleDetial.Premium + vehicleDetial.StampDuty + vehicleDetial.ZTSCLevy + vehicleDetial.VehicleLicenceFee + (vehicleDetial.IncludeRadioLicenseCost == false ? 0 : Convert.ToDecimal(vehicleDetial.RadioLicenseCost));

            return vehilcleTotalPremium;

        }

        public string SaveQRCode(string Policyno)
        {
            string path = "";
            try
            {

                var urlPath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];

                Insurance.Domain.QRCode Codes = new Insurance.Domain.QRCode();

                //var Policy =Convert.ToString (TempData["Registrationno"]);

                using (MemoryStream ms = new MemoryStream())
                {

                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(Policyno, QRCodeGenerator.ECCLevel.Q);
                    QRCoder.QRCode QrCode = new QRCoder.QRCode(qrCodeData);
                    using (Bitmap bitMap = QrCode.GetGraphic(6))
                    {
                        bitMap.Save(ms, ImageFormat.Png);
                        Base64ToImage(Convert.ToBase64String(ms.ToArray())).Save(Server.MapPath("~/QRCode/" + Policyno + ".jpg"));
                        //path = "/QRCode/" + Policyno + ".jpg";
                        path = urlPath + "/QRCode/" + Policyno + ".jpg";
                    }

                    //path = Request.Url.Scheme + System.Uri.SchemeDelimiter + "/" + Request.Url.Host + "/QRCode/" + Policyno + ".jpg";


                    //LinkedResource lr = new LinkedResource("path",MediaTypeNames.Image.Jpeg);
                    //lr.ContentId = "qrImage";
                    //path = Server.MapPath("~/QRCode/" + Policyno + ".jpg");
                    //LinkedResource lr = new LinkedResource(path, MediaTypeNames.Image.Jpeg);
                    //lr.ContentId = "image1";
                    //AlternateView av = AlternateView.CreateAlternateViewFromString(str, null, MediaTypeNames.Text.Html);
                    //lr.ContentId = "image1";
                    //av.LinkedResources.Add(lr);
                    //message.AlternateViews.Add(av);


                    // path = "https://gene.co.zw/QRCode/" + Policyno + ".jpg";
                    // path = Url.ss "http://geneinsureclaim.kindlebit.com/QRCode"  + Policyno + ".jpg";

                    //path = Request.Url.Authority+"/QRCode/" + Policyno + ".jpg";

                    // path = "/QRCode/" + Policyno + ".jpg";




                    Codes.PolicyNo = Policyno;
                    Codes.Qrcode = Policyno;
                    Codes.ReadBy = "";
                    Codes.Deliverto = "";
                    Codes.Createdon = DateTime.Now;
                    Codes.Comment = "";

                    //   var QRCodedata = Mapper.Map<QRCode, QRCode>(Codes);
                    InsuranceContext.QRCodes.Insert(Codes);
                }

            }
            catch (Exception ex)
            {

            }

            return path;
        }

        public System.Drawing.Image Base64ToImage(string base64String)
        {

            byte[] imageBytes = Convert.FromBase64String(base64String.ToString());
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }

        public JsonResult gotoExit(int? id)
        {
            JsonResult jsonResult = new JsonResult();
            Session.Remove("RenewVehicleId");
            Session.Remove("RenewPaymentId");
            Session.Remove("RenewInvoiceId");
            Session.Remove("RenewVehicleSummary");
            Session.Remove("RenewVehiclePolicy");
            Session.Remove("RenewVehicle");
            Session.Remove("RenewVehicleDetails");
            Session.Remove("RenewCardDetail");
            Session.Remove("ReSummaryDetailed");
            Session.Remove("CheckRenewVehicleDetails");
            Session.Remove("ReCustomerDataModal");

            jsonResult.Data = 1;

            return jsonResult;
        }

        private void ClearRenewSession()
        {
            Session.Remove("RenewVehicleId");
            Session.Remove("RenewPaymentId");
            Session.Remove("RenewInvoiceId");
            Session.Remove("RenewVehicleSummary");
            Session.Remove("RenewVehiclePolicy");
            Session.Remove("RenewVehicle");
            Session.Remove("RenewVehicleDetails");
            Session.Remove("RenewCardDetail");
            Session.Remove("ReSummaryDetailed");
            Session.Remove("CheckRenewVehicleDetails");
            Session.Remove("ReCustomerDataModal");
        }

        public JsonResult GetLicenseAddress()
        {
            var customerData = (CustomerModel)Session["CustomerDataModal"];
            //LicenseAddress licenseAddress = new LicenseAddress();
            RiskDetailModel riskDetailModel = new RiskDetailModel();
            riskDetailModel.LicenseAddress1 = customerData.AddressLine1;
            riskDetailModel.LicenseAddress2 = customerData.AddressLine2;
            riskDetailModel.LicenseCity = customerData.City;
            return Json(riskDetailModel, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> InitiatePaynowTransaction(Int32 id, string TotalPremiumPaid, string PolicyNumber, string Email)
        {
            var summaryDetail = InsuranceContext.SummaryDetails.Single(id);
            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={id}").ToList();
            //var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));

            List<Item> itms = new List<Item>();

            foreach (var vehicledetail in SummaryVehicleDetails.ToList())
            {
                var _vehicle = InsuranceContext.VehicleDetails.Single(vehicledetail.VehicleDetailsId);
                Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel _model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_vehicle.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{_vehicle.MakeId}'");

                Item item = new Item();
                item.name = make.MakeDescription + "/" + _model.ModelDescription;
                item.currency = "USD";
                item.price = Convert.ToString(_vehicle.Premium);
                item.quantity = "1";
                item.sku = _vehicle.RegistrationNo;

                itms.Add(item);
            }

            Session["itemData"] = itms;

            Insurance.Service.PaynowService paynowservice = new Insurance.Service.PaynowService();
            PaynowResponse paynowresponse = new PaynowResponse();

            paynowresponse = await paynowservice.initiateTransaction(Convert.ToString(id), TotalPremiumPaid, PolicyNumber, Email, true);

            if (paynowresponse.status == "Ok")
            {
                string strScript = "location.href = '" + paynowresponse.browserurl + "';";
                ViewBag.strScript = "<script type='text/javascript'>$(document).ready(function(){" + strScript + "});</script>";
            }
            else
            {
                ViewBag.strScript = "<script type='text/javascript'>$(document).ready(function(){$('#errormsg').text('" + paynowresponse.error + "');});</script>";
            }

            return View();
            //return RedirectToAction("SaveDetailList", "Paypal", new { id = id });
        }

        public ActionResult PaymentWithCreditCard(CardDetailModel model)
        {
            Session["RenewCardDetail"] = model;

            var Vehicle = new RiskDetailModel();
            if (Session["RenewVehicleDetails"] != null)
            {
                Vehicle = (RiskDetailModel)Session["RenewVehicleDetails"];
            }
            else
            {
                var Id = (Int32)Session["RenewVehicleId"];
                var _vehicle = InsuranceContext.VehicleDetails.Single(Id);
                Vehicle = Mapper.Map<VehicleDetail, RiskDetailModel>(_vehicle);
            }

            //create and item for which you are taking payment
            //if you need to add more items in the list
            //Then you will need to create multiple item objects or use some loop to instantiate object
            var summaryDetail = (SummaryDetail)Session["RenewVehicleSummary"];
           
            var customer = InsuranceContext.Customers.Single(Vehicle.CustomerId);
            
            double totalPremium = Convert.ToDouble(Vehicle.Premium + Vehicle.StampDuty + Vehicle.ZTSCLevy + (Convert.ToBoolean(Vehicle.IncludeRadioLicenseCost) ? Vehicle.RadioLicenseCost : 0.00m));

            //check if single decimal place
            string zeros = string.Empty;
            try
            {
                var percision = totalPremium.ToString().Split('.');
                var length = 2 - percision[1].Length;
                for (int i = 0; i < length; i++)
                {
                    zeros += "0";
                }
            }
            catch
            {
                zeros = ".00";

            }

            List<Item> itms = new List<Item>();

            Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
            VehicleModel _model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{Vehicle.ModelId}'");
            VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{Vehicle.MakeId}'");

            Item item = new Item();
            item.name = make.MakeDescription + "/" + _model.ModelDescription;
            item.currency = "USD";
            item.price = Convert.ToString(Vehicle.Premium + Vehicle.StampDuty + Vehicle.ZTSCLevy + (Convert.ToBoolean(Vehicle.IncludeRadioLicenseCost) ? Vehicle.RadioLicenseCost : 0.00m));
            item.quantity = "1";
            item.sku = Vehicle.RegistrationNo;

            itms.Add(item);


            Session["itemData"] = itms;

            ItemList itemList = new ItemList();
            itemList.items = itms;

            Address billingAddress = new Address();
            billingAddress.city = customer.City;
            billingAddress.country_code = "US";
            billingAddress.line1 = customer.AddressLine1 == string.Empty ? customer.AddressLine2 : customer.AddressLine1;
            billingAddress.line2 = customer.AddressLine2 == string.Empty ? customer.AddressLine1 : customer.AddressLine2;

            if (customer.Zipcode == null)
            {
                billingAddress.postal_code = "00263";
            }
            else
            {
                billingAddress.postal_code = customer.Zipcode;
            }

            billingAddress.state = customer.NationalIdentificationNumber;

            PayPal.Api.CreditCard crdtCard = new PayPal.Api.CreditCard();
            crdtCard.billing_address = billingAddress;
            crdtCard.cvv2 = model.CVC;
            crdtCard.expire_month = Convert.ToInt32(model.ExpiryDate.Split('/')[0]);
            crdtCard.expire_year = Convert.ToInt32(model.ExpiryDate.Split('/')[1]);

            //crdtCard.first_name = "fgdfg";
            //crdtCard.last_name = "rffd";

            var name = model.NameOnCard.Split(' ');
            if (name.Length == 1)
            {
                crdtCard.first_name = name[0];
                crdtCard.last_name = null;
            }
            if (name.Length == 2)
            {
                crdtCard.first_name = name[0];
                crdtCard.last_name = name[1];
            }

            crdtCard.number = model.CardNumber; //use some other test number if it fails
            crdtCard.type = CreditCardUtility.GetTypeName(model.CardNumber).ToLower();

            Details details = new Details();
            details.tax = "0";
            details.shipping = "0";
            details.subtotal = totalPremium.ToString() + zeros;

            Amount amont = new Amount();
            amont.currency = "USD";
            amont.total = totalPremium.ToString() + zeros;
            amont.details = details;

            Transaction tran = new Transaction();
            tran.amount = amont;
            tran.description = "trnx desc";
            tran.item_list = itemList;

            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(tran);

            FundingInstrument fundInstrument = new FundingInstrument();
            fundInstrument.credit_card = crdtCard;

            List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            fundingInstrumentList.Add(fundInstrument);

            var User = UserManager.FindById(customer.UserID);
            PayerInfo pi = new PayerInfo();
            pi.email = User.Email;
            pi.first_name = customer.FirstName;
            pi.last_name = customer.LastName;
            pi.shipping_address = new ShippingAddress
            {
                city = customer.City,
                country_code = "US",
                line1 = customer.AddressLine1 == string.Empty ? customer.AddressLine2 : customer.AddressLine1,
                line2 = customer.AddressLine2 == string.Empty ? customer.AddressLine1 : customer.AddressLine2,
                postal_code = customer.Zipcode,
                state = customer.NationalIdentificationNumber,
            };

            Payer payr = new Payer();
            payr.funding_instruments = fundingInstrumentList;
            payr.payment_method = "credit_card";
            payr.payer_info = pi;

            PayPal.Api.Payment pymnt = new PayPal.Api.Payment();
            pymnt.intent = "sale";
            pymnt.payer = payr;
            pymnt.transactions = transactions;

            try
            {
                //getting context from the paypal, basically we are sending the clientID and clientSecret key in this function 
                //to the get the context from the paypal API to make the payment for which we have created the object above.

                //Code for the configuration class is provided next

                // Basically, apiContext has a accesstoken which is sent by the paypal to authenticate the payment to facilitator account. An access token could be an alphanumeric string

                APIContext apiContext = InsuranceClaim.Models.Configuration.GetAPIContext();

                // Create is a Payment class function which actually sends the payment details to the paypal API for the payment. The function is passed with the ApiContext which we received above.

                PayPal.Api.Payment createdPayment = pymnt.Create(apiContext);
                //paymentInformations.PaymentTransctionId = createdPayment.id;
                Session["RenewPaymentId"] = createdPayment.id;

                //if the createdPayment.State is "approved" it means the payment was successfull else not
                creatInvoice(User, customer);
                if (createdPayment.state.ToLower() != "approved")
                {
                    ModelState.AddModelError("PaymentError", "Payment not approved");
                    return RedirectToAction("PaymentDetail", "CustomerRegistration", new { id = model.SummaryDetailId });
                }
            }
            catch (PayPal.PayPalException ex)
            {
                Logger.Log("Error: " + ex.Message);
                ModelState.AddModelError("PaymentError", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                var error = json_serializer.DeserializeObject(((PayPal.ConnectionException)ex).Response);
                return RedirectToAction("PaymentDetail", "CustomerRegistration", new { id = model.SummaryDetailId });
            }

            return RedirectToAction("SaveDetailList", "Renew", new { id = model.SummaryDetailId });
        }

        private ActionResult creatInvoice(ApplicationUser User, Customer customer)
        {
            APIContext apiContext = InsuranceClaim.Models.Configuration.GetAPIContext();

            var data = (List<Item>)Session["itemData"];

            var invoice = new Invoice()
            {

                merchant_info = new MerchantInfo()
                {
                    email = "ankit.dhiman-facilitator@kindlebit.com",
                    first_name = "Genetic Financial Services",
                    last_name = "ZB Centre, 4th Floor, South Wing, cnr First Street & Kwame Nkrumah Avenue, Harare",
                    business_name = "Insurance Claim",
                    website = "insuranceclaim.com",
                    //tax_id = "47-4559942",

                    phone = new Phone()
                    {
                        country_code = "001",
                        national_number = "08677007491"
                    },
                    address = new InvoiceAddress()
                    {
                        line1 = customer.AddressLine1,
                        city = customer.AddressLine2,
                        state = customer.City + "/ " + customer.NationalIdentificationNumber,
                        postal_code = customer.Zipcode,
                        country_code = "US"

                    }
                },

                billing_info = new List<BillingInfo>()
                            {
                                new BillingInfo()
                                {

                                    email = User.Email,//"amit.kamal@kindlebit.com",
                                    first_name=customer.FirstName,
                                    last_name=customer.LastName
                                }
                            },

                items = new List<InvoiceItem>()
                            {
                                new InvoiceItem()
                                {
                                    name = data[0].name,
                                    quantity = 1,
                                    unit_price = new PayPal.Api.Currency()
                                    {
                                        currency = "USD",
                                        value =data[0].price

                                    },
                                },
                            },
                note = "Your  Invoce has been created successfully.",

                shipping_info = new ShippingInfo()
                {
                    first_name = customer.FirstName,
                    last_name = customer.LastName,
                    business_name = "InsuranceClaim",
                    address = new InvoiceAddress()
                    {
                        //line1 = userdata.State.ToString(),
                        city = customer.City,
                        state = customer.City + "/" + customer.NationalIdentificationNumber,
                        postal_code = customer.Zipcode,
                        country_code = "US"
                    }
                }
            };
            var createdInvoice = invoice.Create(apiContext);
            Session["RenewInvoiceId"] = createdInvoice.id;
            createdInvoice.Send(apiContext);

            return null;
        }

        [HttpPost]
        public JsonResult checkVRNwithICEcash(string regNo, string PaymentTerm)
        {
            CustomerRegistrationController.checkVRNwithICEcashResponse response = new CustomerRegistrationController.checkVRNwithICEcashResponse();

            JsonResult json = new JsonResult();
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            //json.Data = "";

            Insurance.Service.ICEcashService ICEcashService = new Insurance.Service.ICEcashService();
            var tokenObject = new ICEcashTokenResponse();

            #region get ICE cash token
            //if (Session["ICEcashToken"] != null)
            //{
            //    ICEcashService.getToken();
            //    tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
            //}
            //else
            //{
            //    ICEcashService.getToken();
            //    tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
            //}

            //if (Session["ICEcashToken"] != null)
            //{
            //    var icevalue = (ICEcashTokenResponse)Session["ICEcashToken"];
            //    string format = "yyyyMMddHHmmss";
            //    var IceDateNowtime = DateTime.Now;
            //    var IceExpery = DateTime.ParseExact(icevalue.Response.ExpireDate, format, CultureInfo.InvariantCulture);
            //    if (IceDateNowtime > IceExpery)
            //    {
            //        ICEcashService.getToken();
            //    }

            //    tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
            //}
            //else
            //{
            //    ICEcashService.getToken();
            //    tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
            //}


            string parternToken = SummaryDetailService.GetLatestToken();

            #endregion

            List<RiskDetailModel> objVehicles = new List<RiskDetailModel>();
            //objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo });
            objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo, PaymentTermId = Convert.ToInt32(PaymentTerm) });



            //  objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo, PaymentTermId = Convert.ToInt32(PaymentTerm), CoverTypeId = Convert.ToInt32(CoverTypeId), ProductId = Convert.ToInt32(ProductId), MakeId = MakeId, ModelId = ModelId, TaxClassId = Convert.ToInt32(TaxClassId), VehicleYear = Convert.ToInt32(VehicleYear) });





            if (parternToken != "")
            {
                ResultRootObject quoteresponse = ICEcashService.checkVehicleExists(objVehicles, parternToken, tokenObject.PartnerReference);
                if (quoteresponse.Response.Message.Contains("Partner Token has expired"))
                {
                    tokenObject = ICEcashService.getToken();
                    SummaryDetailService.UpdateToken(tokenObject);

                    quoteresponse = ICEcashService.checkVehicleExists(objVehicles, tokenObject.Response.PartnerToken, tokenObject.PartnerReference);

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
            }

            json.Data = response;

            return json;
        }

        //[HttpPost]
        //public JsonResult getPolicyDetailsFromICEcash(string regNo, string PaymentTerm, string SumInsured, string make, string model, int VehicleYear, int CoverTypeId, int VehicleType, string CoverStartDate, string CoverEndDate, bool VehilceLicense, string taxClassId, bool RadioLicense)
        //{
        //    CustomerRegistrationController.checkVRNwithICEcashResponse response = new CustomerRegistrationController.checkVRNwithICEcashResponse();
        //    JsonResult json = new JsonResult();
        //    json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        //    //json.Data = "";

        //    try
        //    {
        //        Insurance.Service.ICEcashService ICEcashService = new Insurance.Service.ICEcashService();
        //        var tokenObject = new ICEcashTokenResponse();

        //        #region get ICE cash token
        //        //if (Session["ICEcashToken"] != null)
        //        //{
        //        //    ICEcashService.getToken();
        //        //    tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
        //        //}
        //        //else
        //        //{
        //        //    ICEcashService.getToken();
        //        //    tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
        //        //}


        //        string patnerToken = SummaryDetailService.GetLatestToken();

        //        if (patnerToken == "")
        //        {
        //            tokenObject = ICEcashService.getToken();
        //            SummaryDetailService.UpdateToken(tokenObject);
        //        }



        //        #endregion

        //        List<RiskDetailModel> objVehicles = new List<RiskDetailModel>();
        //        //objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo });
        //        objVehicles.Add(new RiskDetailModel { RegistrationNo = regNo, PaymentTermId = Convert.ToInt32(PaymentTerm) });


        //        DateTime Cover_StartDate = CoverStartDate == null ? DateTime.Now : Convert.ToDateTime(CoverStartDate);
        //        DateTime Cover_EndDate = CoverEndDate == null ? DateTime.Now : Convert.ToDateTime(CoverEndDate);

        //        ResultRootObject quoteresponse = new ResultRootObject();

        //        if (patnerToken != "")
        //        {
        //            if (VehilceLicense)
        //                quoteresponse = ICEcashService.TPILICQuote(patnerToken, regNo, SumInsured, make, model, Convert.ToInt32(PaymentTerm), Convert.ToInt32(VehicleYear), CoverTypeId, VehicleType, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, taxClassId, VehilceLicense, RadioLicense);
        //            else
        //                quoteresponse = ICEcashService.RequestQuote(patnerToken, regNo, SumInsured, make, model, Convert.ToInt32(PaymentTerm), VehicleYear, CoverTypeId, VehicleType, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, taxClassId);


        //            if (quoteresponse.Response != null && quoteresponse.Response.Message.Contains("Partner Token has expired"))
        //            {

        //                tokenObject = ICEcashService.getToken();
        //                SummaryDetailService.UpdateToken(tokenObject);
        //                //  tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];

        //                if (VehilceLicense)
        //                    quoteresponse = ICEcashService.TPILICQuote(patnerToken, regNo, SumInsured, make, model, Convert.ToInt32(PaymentTerm), Convert.ToInt32(VehicleYear), CoverTypeId, VehicleType, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, taxClassId, VehilceLicense, RadioLicense);
        //                else
        //                    quoteresponse = ICEcashService.RequestQuote(tokenObject.Response.PartnerToken, regNo, SumInsured, make, model, Convert.ToInt32(PaymentTerm), VehicleYear, CoverTypeId, VehicleType, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, taxClassId);



        //            }




        //            response.result = quoteresponse.Response.Result;
        //            if (response.result == 0)
        //            {
        //                response.message = quoteresponse.Response.Quotes[0].Message;
        //            }
        //            else
        //            {
        //                response.Data = quoteresponse;
        //            }
        //        }
        //        json.Data = response;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.message = "Error occured.";
        //        json.Data = new ResultResponse();
        //    }



        //    return json;
        //}


        [HttpGet]
        public JsonResult getVehicleList(int summaryDetailId = 0)
        {
            try
            {
                List<RiskDetailModel> vehicleList = new List<RiskDetailModel>();
                if (summaryDetailId != 0)
                {
                    //vehicle = summary.GetVehicleInformation(id);
                    var summaryVehicleList = InsuranceContext.SummaryVehicleDetails.All(where: $" SummaryDetailId='{summaryDetailId}'");

                    foreach (var item in summaryVehicleList)
                    {
                        var vehicleDetails = InsuranceContext.VehicleDetails.Single(where: $" Id='{item.VehicleDetailsId}'");
                        RiskDetailModel vehicleModel = Mapper.Map<VehicleDetail, RiskDetailModel>(vehicleDetails);

                        vehicleModel.ZTSCLevy = vehicleDetails.ZTSCLevy;
                        vehicleList.Add(vehicleModel);
                    }

                    Session["RenewVehicleDetails"] = vehicleList;
                }


                if (Session["RenewVehicleDetails"] != null)
                {
                    var list = (List<RiskDetailModel>)Session["RenewVehicleDetails"];
                    List<VehicleListModel> vehiclelist = new List<VehicleListModel>();

                    foreach (var item in list)
                    {
                        VehicleListModel obj = new VehicleListModel();
                        obj.make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'").MakeDescription;
                        obj.model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ModelDescription;
                        obj.covertype = InsuranceContext.CoverTypes.Single(item.CoverTypeId).Name;
                        obj.premium = item.Premium.ToString();
                        obj.suminsured = item.SumInsured.ToString();
                        obj.RegistrationNo = item.RegistrationNo;


                        if (item.IncludeRadioLicenseCost == true)
                        {
                            obj.radio_license_fee = item.RadioLicenseCost == null ? "0" : item.RadioLicenseCost.ToString();
                        }
                        else
                        {
                            obj.radio_license_fee = "0";
                        }


                        obj.excess = item.ExcessAmount == null ? "0" : item.ExcessAmount.ToString();
                        obj.vehicle_license_fee = item.VehicleLicenceFee == 0 ? "0" : item.VehicleLicenceFee.ToString();
                        obj.stampDuty = item.StampDuty == null ? "0" : item.StampDuty.ToString();

                        decimal? radioLicenseCost = 0;
                        if (item.IncludeRadioLicenseCost)
                        {
                            radioLicenseCost = item.RadioLicenseCost;
                        }

                        // var calculationAmount = item.Premium + radioLicenseCost + item.Excess + item.VehicleLicenceFee + item.StampDuty + item.ZTSCLevy;

                        var calculationAmount = item.Premium + radioLicenseCost + item.VehicleLicenceFee + item.StampDuty + item.ZTSCLevy;


                        obj.total = calculationAmount.ToString();
                        obj.ZTSCLevy = item.ZTSCLevy == null ? "0" : Convert.ToString(item.ZTSCLevy);


                        vehiclelist.Add(obj);
                    }

                    return Json(vehiclelist, JsonRequestBehavior.AllowGet);
                }

                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }


        public void RenewApproveVRNToIceCash(Customer customerDetails, VehicleDetail vichelDetails, int PaymentMethod)
        {
            #region update  TPIQuoteUpdate
            Insurance.Service.EmailService log = new Insurance.Service.EmailService();

            if(PaymentMethod!=3)
                PaymentMethod = 1;
            

            try
            {
                var tokenObject = new ICEcashTokenResponse();
                var PartnerToken = "";

                ICEcashService iceCash = new ICEcashService();
                PartnerToken = SummaryDetailService.GetLatestToken();

                var res = new ResultRootObject();

                if (vichelDetails != null && vichelDetails.CombinedID != null && vichelDetails.VehicleLicenceFee > 0)
                {
                    ResultRootObject quoteresponse = ICEcashService.TPILICUpdate(customerDetails, vichelDetails, PartnerToken, PaymentMethod);
                    if (quoteresponse.Response != null && quoteresponse.Response.Message.Contains("Partner Token has expired"))
                    {
                        tokenObject = iceCash.getToken();
                        SummaryDetailService.UpdateToken(tokenObject);
                        PartnerToken = tokenObject.Response.PartnerToken;
                        ICEcashService.TPILICUpdate(customerDetails, vichelDetails, PartnerToken, 1);
                    }


                    res = ICEcashService.TPILICResult(vichelDetails, PartnerToken);
                    if (res.Response != null && res.Response.Message.Contains("Partner Token has expired"))
                    {
                        tokenObject = iceCash.getToken();
                        //   tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
                        SummaryDetailService.UpdateToken(tokenObject);
                        // tokenObject = service.CheckSessionExpired();
                        PartnerToken = tokenObject.Response.PartnerToken;
                        res = ICEcashService.TPILICResult(vichelDetails, PartnerToken);
                 
                    }

                    if (res.Response != null && res.Response.LicenceCert != null)
                        _pdfPath = MiscellaneousService.LicensePdf(res.Response.LicenceCert, Convert.ToString(vichelDetails.Id));

                    string format = "yyyyMMdd";
                    if (res.Response != null && res.Response.LicExpiryDate!=null)
                    {
                        DateTime LicExpiryDate = DateTime.ParseExact(res.Response.LicExpiryDate, format, CultureInfo.InvariantCulture);
                        vichelDetails.LicExpiryDate = LicExpiryDate.ToShortDateString();
                    }
                }
                else if (!string.IsNullOrEmpty(vichelDetails.InsuranceId))
                {
                    ResultRootObject quoteresponse = ICEcashService.TPIQuoteUpdate(customerDetails, vichelDetails, PartnerToken, PaymentMethod);

                    // if partern token expire
                    if (quoteresponse.Response != null && quoteresponse.Response.Message.Contains("Partner Token has expired"))
                    {
                        tokenObject = iceCash.getToken();
                        SummaryDetailService.UpdateToken(tokenObject);
                        PartnerToken = tokenObject.Response.PartnerToken;
                        ICEcashService.TPIQuoteUpdate(customerDetails, vichelDetails, PartnerToken, PaymentMethod);
                    }

                    res = ICEcashService.TPIPolicy(vichelDetails, PartnerToken);
                    if (res.Response != null && (res.Response.Message.Contains("Partner Token has expired") || res.Response.Message.Contains("Invalid Partner Token")))
                    {
                        tokenObject = iceCash.getToken();
                        SummaryDetailService.UpdateToken(tokenObject);
                        PartnerToken = tokenObject.Response.PartnerToken;
                        res = ICEcashService.TPIPolicy(vichelDetails, PartnerToken);
                    }
                }


                if (res.Response != null && res.Response.Message.Contains("Policy Retrieved"))
                {
                    vichelDetails.InsuranceStatus = "Approved";
                    string format = "yyyyMMdd";
                    vichelDetails.CoverStartDate = DateTime.ParseExact(res.Response.StartDate, format, CultureInfo.InvariantCulture);
                    vichelDetails.CoverEndDate = DateTime.ParseExact(res.Response.EndDate, format, CultureInfo.InvariantCulture);
                    vichelDetails.RenewalDate = vichelDetails.CoverEndDate.Value.AddDays(1);
                    vichelDetails.CoverNote = res.Response.PolicyNo; // it's represent to Cover Note

                   
                    if (res.Response.Quotes != null && res.Response.Quotes[0].LicExpiryDate!=null)
                    {
                        try
                        {
                            DateTime LicExpiryDate = DateTime.ParseExact(res.Response.Quotes[0].LicExpiryDate, format, CultureInfo.InvariantCulture);
                            vichelDetails.LicExpiryDate = LicExpiryDate.ToShortDateString();
                        }
                        catch(Exception ex)
                        {

                        }

                        
                    }

                    if (res.Response.LicenceCert!=null && res.Response.LicenceCert.Length>1)
                    {
                        _pdfCode = "PD" + vichelDetails.Id + "" + DateTime.Now.Month;
                    }
                    

                    vichelDetails.PdfCode = _pdfCode;

                    InsuranceContext.VehicleDetails.Update(vichelDetails);
                }

            }
            catch (Exception ex)
            {
                // log.WriteLog("to approve");
            }

            #endregion
        }

        public class Country
        {
            public string code { get; set; }
            public string name { get; set; }
            public string DisplayName { get; set; }
        }



        public ActionResult customeRenew(int renewvehicle = 0)
        {
            Session["vehicleid"] = renewvehicle;
            CustomerModel _custdata = new CustomerModel();
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            string path = Server.MapPath("~/Content/Countries.txt");
            var countries = System.IO.File.ReadAllText(path);
            var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
            ViewBag.Cities = InsuranceContext.Cities.All();
            ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));

            var vehicledetail = InsuranceContext.VehicleDetails.Single(where: $"Id = '{renewvehicle}'");
            if (vehicledetail != null)
            {
                var customer = InsuranceContext.Customers.Single(where: $"Id = '{vehicledetail.CustomerId}'");
                if (customer != null)
                {
                    _custdata = Mapper.Map<Customer, CustomerModel>(customer);

                    // var User = UserManager.FindById(customerData.UserID);
                    _custdata.AddressLine1 = _custdata.AddressLine1;
                    _custdata.AddressLine2 = _custdata.AddressLine2;
                    _custdata.City = _custdata.City;
                    _custdata.Id = _custdata.Id;
                    _custdata.Country = _custdata.Country;
                    _custdata.Zipcode = _custdata.Zipcode;
                    _custdata.Gender = _custdata.Gender;
                    _custdata.PhoneNumber = _custdata.PhoneNumber;
                    _custdata.NationalIdentificationNumber = _custdata.NationalIdentificationNumber;
                    _custdata.DateOfBirth = _custdata.DateOfBirth;
                    //_custdata.EmailAddress = _custdata.EmailAddress;
                    _custdata.FirstName = _custdata.FirstName;
                    _custdata.LastName = _custdata.LastName;
                    _custdata.CountryCode = _custdata.CountryCode;
                    _custdata.IsCustomEmail = _custdata.IsCustomEmail;


                    var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == customer.UserID);
                    if (dbUser != null)
                    {
                        _custdata.EmailAddress = dbUser.Email;
                    }
                    return View(_custdata);
                }

            }
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> _SaveCustomerData(CustomerModel model, string buttonUpdate)
        {
            ModelState.Remove("City");
            ModelState.Remove("CountryCode");

            //foreach (ModelState modelState in ViewData.ModelState.Values)
            //{
            //    foreach (ModelError error in modelState.Errors)
            //    {
            //        var res = "";
            //    }
            //}
            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (userLoggedin)
                {


                    if (User.IsInRole("Staff") || User.IsInRole("Renewals"))
                    {
                        //if (buttonUpdate != null)
                        //{
                        //    AddOrUpdateCustomerInformation(model);

                        //    return Json(new { IsError = false, error = "Sucessfully update" }, JsonRequestBehavior.AllowGet);
                        //}

                        var email = LoggedUserEmail();

                        if (email == model.EmailAddress)
                        {
                            return Json(new { IsError = false, error = "Staff and customer email can not be same" }, JsonRequestBehavior.AllowGet);
                        }
                    }


                    Session["CustomModal"] = model;
                    return Json(new { IsError = true, error = "" }, JsonRequestBehavior.AllowGet);
                }


            }
            return Json(new { IsError = false, error = TempData["ErrorMessage"].ToString() }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Viewriskdetail()
        {
            Session["RenewVehicleView"] = null;

            ViewBag.Products = InsuranceContext.Products.All(where: "Active = 'True' or Active is null").ToList();
            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm))
                                   select new
                                   {
                                       ID = (int)e,
                                       Name = e.ToString()
                                   };

            ViewBag.ePaymentTermData = new SelectList(ePaymentTermData, "ID", "Name");
            var service = new VehicleService();
            ViewBag.VehicleUsage = service.GetAllVehicleUsage();


            var makers = service.GetMakers();


            ViewBag.CoverType = service.GetCoverType().ToList();
            ViewBag.AgentCommission = service.GetAgentCommission();
            var data1 = (from p in InsuranceContext.BusinessSources.All().ToList()
                         join f in InsuranceContext.SourceDetails.All().ToList()
                         on p.Id equals f.BusinessId
                         select new
                         {
                             Value = f.Id,
                             Text = f.FirstName + " " + f.LastName + " - " + p.Source
                         }).ToList();

            List<SelectListItem> listdata = new List<SelectListItem>();
            foreach (var item in data1)
            {
                SelectListItem sli = new SelectListItem();
                sli.Value = Convert.ToString(item.Value);
                sli.Text = item.Text;
                listdata.Add(sli);
            }
            ViewBag.Sources = new SelectList(listdata, "Value", "Text");



            ViewBag.Currencies = InsuranceContext.Currencies.All();

            ViewBag.Makers = makers;


            var eExcessTypeData = from eExcessType e in Enum.GetValues(typeof(eExcessType))
                                  select new
                                  {
                                      ID = (int)e,
                                      Name = e.ToString()
                                  };

            ViewBag.eExcessTypeData = new SelectList(eExcessTypeData, "ID", "Name");


            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;

            }


            var viewModel = new RiskDetailModel();

            var renvehicleid = Session["vehicleid"];
            var vehicledetailss = InsuranceContext.VehicleDetails.Single(where: $"Id = '{renvehicleid}'");

            if (vehicledetailss != null)
            {
                Session["RenewVehicleView"] = vehicledetailss;
                viewModel.NoOfCarsCovered = vehicledetailss.NoOfCarsCovered;
                viewModel.AgentCommissionId = vehicledetailss.AgentCommissionId;

                viewModel.ChasisNumber = vehicledetailss.ChasisNumber;

                viewModel.CoverEndDate = vehicledetailss.CoverEndDate;
                viewModel.CoverNoteNo = vehicledetailss.CoverNoteNo;
                viewModel.CoverStartDate = vehicledetailss.CoverStartDate;
                viewModel.CoverTypeId = vehicledetailss.CoverTypeId;
                viewModel.CubicCapacity = vehicledetailss.CubicCapacity;
                viewModel.CustomerId = vehicledetailss.CustomerId;
                viewModel.EngineNumber = vehicledetailss.EngineNumber;
                viewModel.Excess = (int)Math.Round(vehicledetailss.Excess, 0);
                viewModel.ExcessType = vehicledetailss.ExcessType;
                viewModel.MakeId = vehicledetailss.MakeId;

                viewModel.ModelId = vehicledetailss.ModelId;
                viewModel.OptionalCovers = vehicledetailss.OptionalCovers;
                viewModel.PolicyId = vehicledetailss.PolicyId;
                viewModel.Premium = vehicledetailss.Premium;
                viewModel.PremiumWithDiscount = vehicledetailss.Premium + vehicledetailss.Discount;
                viewModel.RadioLicenseCost = (int)Math.Round(vehicledetailss.RadioLicenseCost == null ? 0 : vehicledetailss.RadioLicenseCost.Value, 0);
                viewModel.Rate = vehicledetailss.Rate;
                viewModel.RegistrationNo = vehicledetailss.RegistrationNo;
                viewModel.StampDuty = vehicledetailss.StampDuty;
                viewModel.SumInsured = (int)Math.Round(vehicledetailss.SumInsured == null ? 0 : vehicledetailss.SumInsured.Value, 0);
                viewModel.VehicleColor = vehicledetailss.VehicleColor;
                viewModel.VehicleUsage = vehicledetailss.VehicleUsage;
                viewModel.VehicleYear = vehicledetailss.VehicleYear;
                viewModel.Id = vehicledetailss.Id;



                viewModel.ZTSCLevy = Math.Round(Convert.ToDecimal(vehicledetailss.ZTSCLevy), 2);
                viewModel.NumberofPersons = vehicledetailss.NumberofPersons;
                viewModel.PassengerAccidentCover = vehicledetailss.PassengerAccidentCover;
                viewModel.IsLicenseDiskNeeded = Convert.ToBoolean(vehicledetailss.IsLicenseDiskNeeded);
                viewModel.ExcessBuyBack = vehicledetailss.ExcessBuyBack;
                viewModel.RoadsideAssistance = vehicledetailss.RoadsideAssistance;
                viewModel.MedicalExpenses = vehicledetailss.MedicalExpenses;
                viewModel.Addthirdparty = vehicledetailss.Addthirdparty;
                viewModel.AddThirdPartyAmount = Math.Round(Convert.ToDecimal(vehicledetailss.AddThirdPartyAmount), 2);
                viewModel.ExcessAmount = Math.Round(Convert.ToDecimal(vehicledetailss.ExcessAmount), 2);
                viewModel.ExcessAmount = Math.Round(Convert.ToDecimal(vehicledetailss.ExcessAmount), 2);
                viewModel.ExcessBuyBackAmount = Math.Round(Convert.ToDecimal(vehicledetailss.ExcessBuyBackAmount), 2);
                viewModel.MedicalExpensesAmount = Math.Round(Convert.ToDecimal(vehicledetailss.MedicalExpensesAmount), 2);
                viewModel.MedicalExpensesPercentage = Math.Round(Convert.ToDecimal(vehicledetailss.MedicalExpensesPercentage), 2);
                viewModel.PassengerAccidentCoverAmount = Math.Round(Convert.ToDecimal(vehicledetailss.PassengerAccidentCoverAmount), 2);
                viewModel.PassengerAccidentCoverAmountPerPerson = Math.Round(Convert.ToDecimal(vehicledetailss.PassengerAccidentCoverAmountPerPerson), 2);
                viewModel.PaymentTermId = vehicledetailss.PaymentTermId;
                viewModel.ProductId = vehicledetailss.ProductId;
                viewModel.IncludeRadioLicenseCost = Convert.ToBoolean(vehicledetailss.IncludeRadioLicenseCost);
                viewModel.RenewalDate = Convert.ToDateTime(vehicledetailss.RenewalDate);
                viewModel.TransactionDate = Convert.ToDateTime(vehicledetailss.TransactionDate);
                viewModel.AnnualRiskPremium = Math.Round(Convert.ToDecimal(vehicledetailss.AnnualRiskPremium), 2);
                viewModel.TermlyRiskPremium = Math.Round(Convert.ToDecimal(vehicledetailss.TermlyRiskPremium), 2);
                viewModel.QuaterlyRiskPremium = Math.Round(Convert.ToDecimal(vehicledetailss.QuaterlyRiskPremium), 2);
                viewModel.Discount = Math.Round(Convert.ToDecimal(vehicledetailss.Discount), 2);
                viewModel.VehicleLicenceFee = Convert.ToDecimal(vehicledetailss.VehicleLicenceFee);
                viewModel.PenaltiesAmt = Convert.ToDecimal(vehicledetailss.PenaltiesAmt);
                //  viewModel.isUpdate = true; // commented on 31 oct
                viewModel.isUpdate = false;                         // viewModel.isUpdate = false; 
                viewModel.vehicleindex = Convert.ToInt32(vehicledetailss.Id);
                viewModel.BusinessSourceDetailId = vehicledetailss.BusinessSourceDetailId;
                viewModel.CurrencyId = vehicledetailss.CurrencyId;

                var ser = new VehicleService();
                var model = ser.GetModel(vehicledetailss.MakeId);
                ViewBag.Model = model;
                Session["RenewVehicleView"] = viewModel;
            }
            return View(viewModel);
        }
        public ActionResult ReneSummaryDetails(int? Id)
        {
            //var vehicleid = Session["vehicleid"];
            SummaryDetailModel Resummry = new SummaryDetailModel();

            var VehicleRdetails = (RiskDetailModel)Session["RenewVehicleView"];

            var dbVehicalDetials = InsuranceContext.VehicleDetails.Single(VehicleRdetails.Id);

            var Vsummmarydetail = InsuranceContext.SummaryVehicleDetails.Single(where: $"VehicleDetailsId = '{VehicleRdetails.Id}'");
            var summarydetails = InsuranceContext.SummaryDetails.Single(where: $"Id = '{Vsummmarydetail.SummaryDetailId}'");



            if (summarydetails != null && dbVehicalDetials.IsActive == true)
            {

                Resummry = Mapper.Map<SummaryDetail, SummaryDetailModel>(summarydetails);
                // Resummry.CarInsuredCount = VehicleRdetails.Count;
                Resummry.DebitNote = summarydetails.DebitNote;
                Resummry.PaymentMethodId = summarydetails.PaymentMethodId;
                Resummry.PaymentTermId = summarydetails.PaymentTermId;
                Resummry.ReceiptNumber = summarydetails.ReceiptNumber;

                Resummry.TotalPremium = Convert.ToDecimal(Math.Round(Convert.ToDouble(dbVehicalDetials.Premium + dbVehicalDetials.StampDuty + dbVehicalDetials.ZTSCLevy + dbVehicalDetials.VehicleLicenceFee + (Convert.ToBoolean(dbVehicalDetials.IncludeRadioLicenseCost) ? dbVehicalDetials.RadioLicenseCost : 0.00m)), 2)); ;
                Resummry.AmountPaid = Convert.ToDecimal(Math.Round(Convert.ToDouble(dbVehicalDetials.Premium + dbVehicalDetials.StampDuty + dbVehicalDetials.ZTSCLevy  + dbVehicalDetials.VehicleLicenceFee + (Convert.ToBoolean(dbVehicalDetials.IncludeRadioLicenseCost) ? dbVehicalDetials.RadioLicenseCost : 0.00m)), 2));



                Resummry.TotalStampDuty = VehicleRdetails.StampDuty;
                Resummry.TotalSumInsured = VehicleRdetails.SumInsured;
                Resummry.TotalZTSCLevies = VehicleRdetails.ZTSCLevy;
                Resummry.ExcessBuyBackAmount = VehicleRdetails.ExcessBuyBackAmount;
                Resummry.MedicalExpensesAmount = VehicleRdetails.MedicalExpensesAmount;
                Resummry.PassengerAccidentCoverAmount = VehicleRdetails.PassengerAccidentCoverAmount;
                Resummry.RoadsideAssistanceAmount = VehicleRdetails.RoadsideAssistanceAmount;
                Resummry.ExcessAmount = VehicleRdetails.ExcessAmount;
                Resummry.Discount = VehicleRdetails.Discount;

                if (Convert.ToBoolean(VehicleRdetails.IncludeRadioLicenseCost))
                {

                    Resummry.TotalRadioLicenseCost = VehicleRdetails.RadioLicenseCost;
                }
            }
            else
            {
                Resummry = Mapper.Map<SummaryDetail, SummaryDetailModel>(summarydetails);
                // Resummry.CarInsuredCount = VehicleRdetails.Count;
                Resummry.DebitNote = summarydetails.DebitNote;
                Resummry.PaymentMethodId = summarydetails.PaymentMethodId;
                Resummry.PaymentTermId = summarydetails.PaymentTermId;
                Resummry.ReceiptNumber = summarydetails.ReceiptNumber;

                Resummry.TotalPremium = Convert.ToDecimal(Math.Round(Convert.ToDouble(dbVehicalDetials.Premium + dbVehicalDetials.StampDuty + dbVehicalDetials.ZTSCLevy + dbVehicalDetials.VehicleLicenceFee  + (Convert.ToBoolean(dbVehicalDetials.IncludeRadioLicenseCost) ? dbVehicalDetials.RadioLicenseCost : 0.00m)), 2));
                Resummry.AmountPaid = Convert.ToDecimal(Math.Round(Convert.ToDouble(dbVehicalDetials.Premium + dbVehicalDetials.StampDuty + dbVehicalDetials.ZTSCLevy + dbVehicalDetials.VehicleLicenceFee  + (Convert.ToBoolean(dbVehicalDetials.IncludeRadioLicenseCost) ? dbVehicalDetials.RadioLicenseCost : 0.00m)), 2));

                Resummry.TotalStampDuty = VehicleRdetails.StampDuty;
                Resummry.TotalSumInsured = VehicleRdetails.SumInsured;
                Resummry.TotalZTSCLevies = VehicleRdetails.ZTSCLevy;
                Resummry.ExcessBuyBackAmount = VehicleRdetails.ExcessBuyBackAmount;
                Resummry.MedicalExpensesAmount = VehicleRdetails.MedicalExpensesAmount;
                Resummry.PassengerAccidentCoverAmount = VehicleRdetails.PassengerAccidentCoverAmount;
                Resummry.RoadsideAssistanceAmount = VehicleRdetails.RoadsideAssistanceAmount;
                Resummry.ExcessAmount = VehicleRdetails.ExcessAmount;
                Resummry.Discount = VehicleRdetails.Discount;

                if (Convert.ToBoolean(VehicleRdetails.IncludeRadioLicenseCost))
                {

                    Resummry.TotalRadioLicenseCost = VehicleRdetails.RadioLicenseCost;
                }
            }


            return View(Resummry);
        }

        public class RootObject
        {
            public List<Country> countries { get; set; }
        }
    }




}