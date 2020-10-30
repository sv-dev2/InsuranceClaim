using AutoMapper;
using Insurance.Domain;
using Insurance.Service;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class DomesticController : Controller
    {

        private ApplicationUserManager _userManager;

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

        int currencyId = 8; //RTGS$ 6 for live
        int policyStatusId = 1;
        int businessSourceId = 1; // broker
        int InsurerId = 1; //Zimnat Insurance Company

        UserService _userService = new UserService();
        PolicyService _policyService = new PolicyService();
        VehicleService _vehicleService = new VehicleService();
        DomesticService _domesticService = new DomesticService();

        SummaryDetailService _summaryDetailService = new SummaryDetailService();
        RiskDetailService _riskDetailService = new RiskDetailService();
        Insurance.Service.smsService objsmsService = new Insurance.Service.smsService();

        string AdminEmail = WebConfigurationManager.AppSettings["AdminEmail"];
        string ZimnatEmail = WebConfigurationManager.AppSettings["ZimnatEmail"];

        // GET: Domestic
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RegisterUser()
        {
            ClearSession();

            CustomerModel model = new CustomerModel();
            BindDropdown();
            model.Zipcode = "00263";
            return View(model);
        }


        [HttpPost]
        public ActionResult RegisterUser(CustomerModel model)
        {
            Session["CustomerDataModal"] = model;

            BindDropdown();
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
                    Session["CustomerDataModal"] = model;
                    return Json(new { IsError = true, error = "" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var AllUsers = UserManager.Users.ToList();//.FirstOrDefault(p=>p.Email== model.EmailAddress);
                    var isExist = AllUsers.Any(p => p.Email.ToLower() == model.EmailAddress.ToLower() || p.UserName.ToLower() == model.EmailAddress);
                    if (isExist)
                    {
                        return Json(new { IsError = false, error = "Email " + model.EmailAddress + " already exists." }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        Session["CustomerDataModal"] = model;
                        return Json(new { IsError = true, error = "" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(new { IsError = false, error = TempData["ErrorMessage"].ToString() }, JsonRequestBehavior.AllowGet);

            // return View(model);
        }

        public ActionResult ProductDetail()
        {
            var model = new PolicyDetailModel();
            model.CurrencyId = currencyId;
            model.PolicyStatusId = policyStatusId;
            model.BusinessSourceId = businessSourceId;
            //model.Products = InsuranceContext.Products.All().ToList();
            model.InsurerId = InsurerId;
            model.PolicyNumber = _policyService.GetLatestPolicyNumber();
            model.BusinessSourceId = businessSourceId;
            Session["PolicyData"] = Mapper.Map<PolicyDetailModel, PolicyDetail>(model);

            return RedirectToAction("RiskDetail");
        }

        public ActionResult RiskDetail(int? id = 1)
        {
            // id=1 for selecting VRN num details

            if (Session["CustomerDataModal"] == null)
            {
                return Redirect("/Domestic/RegisterUser");
            }

            // summaryDetailId: it's represent to Qutation edit

            if (Session["SummaryDetailId"] != null)
            {
                SetValueIntoSession(Convert.ToInt32(Session["SummaryDetailId"]));
                Session["SummaryDetailId"] = null;
            }

            ViewBag.Products = _vehicleService.GetDemosticProducts();
            ViewBag.TaxClass = _vehicleService.GetAllTaxClasses();
            // ViewBag.PaymentMethods = 


            ViewBag.PaymentTermId = _vehicleService.GetAllPaymentTerms();

            ViewBag.PaymentMethods = InsuranceContext.PaymentMethods.All(where: "name ='Cash' or name='Swipe' or name='Mobile' or name='Bank Transfer'").ToList();

            var PolicyData = (PolicyDetail)Session["PolicyData"];
            var viewModel = new DomesticRiskDetailModel();
            var service = new VehicleService();

            ViewBag.VehicleUsage = service.GetAllVehicleUsage();
            viewModel.NoOfCarsCovered = 1;
            if (Session["VehicleDetails"] != null)
            {
                var list = (List<DomesticRiskDetailModel>)Session["VehicleDetails"];
                viewModel.NoOfCarsCovered = list.Count + 1;
            }

            if (id > 0)
            {
                var list = (List<DomesticRiskDetailModel>)Session["VehicleDetails"];
                if (list != null && list.Count > 0 && (list.Count >= id))
                {
                    var data = (DomesticRiskDetailModel)list[Convert.ToInt32(id - 1)];
                    if (data != null)
                    {
                        viewModel.CoverEndDate = data.CoverEndDate;
                        viewModel.Notes = data.Notes;
                        viewModel.CoverStartDate = data.CoverStartDate;
                        viewModel.CustomerId = data.CustomerId;
                        viewModel.NoOfCarsCovered = id;
                        viewModel.PolicyId = data.PolicyId;
                        viewModel.BasicPremium = data.BasicPremium;
                        viewModel.Rate = data.Rate;

                        viewModel.StampDuty = Math.Round(Convert.ToDecimal(data.StampDuty), 2);

                        viewModel.CoverAmount = (int)Math.Round(data.CoverAmount == null ? 0 : data.CoverAmount.Value, 0);
                        viewModel.Id = data.Id;


                        viewModel.PaymentTermId = data.PaymentTermId;
                        viewModel.ProductId = data.ProductId;
                        viewModel.RenewDate = data.RenewDate;
                        viewModel.TransactionDate = data.TransactionDate;

                        viewModel.isUpdate = true; // commented on 31 oct
                        // viewModel.isUpdate = false;                        // commented on 02 feb 2019
                        viewModel.vehicleindex = Convert.ToInt32(id);

                        viewModel.CurrencyId = data.CurrencyId;

                    }
                }
            }
            return View(viewModel);
        }

        public ActionResult SummaryDetail(int summaryDetailId = 0, string paymentError = "")
        {
            if (Session["CustomerDataModal"] == null && summaryDetailId == 0)
            {
                return Redirect("/Domestic/Index");
            }
            if (Session["VehicleDetails"] == null && summaryDetailId == 0)
            {
                return Redirect("/Domestic/RiskDetail");
            }

            var model = new DomesticSummaryModel();
            try
            {
                Session["issummaryformvisited"] = true;
                var summarydetail = (DomesticSummaryModel)Session["SummaryDetailed"];
                SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();

                ViewBag.SummaryDetailId = summaryDetailId;
                var role = "";
                if (System.Web.HttpContext.Current.User.Identity.GetUserId() != null)
                {
                    role = UserManager.GetRoles(System.Web.HttpContext.Current.User.Identity.GetUserId()).FirstOrDefault();
                }

                ViewBag.CurrentUserRole = role;
                var summary = new SummaryDetailService();
                var vehicle = (List<DomesticRiskDetailModel>)Session["VehicleDetails"];// summary.GetVehicleInformation(id);
                List<DomesticRiskDetailModel> vehicleList = new List<DomesticRiskDetailModel>();
                if (summaryDetailId != 0)
                {
                    model.CustomSumarryDetilId = summaryDetailId;

                    var summaryVichalList = _domesticService.GetAllSummaryVehiclesById(summaryDetailId);

                    foreach (var item in summaryVichalList)
                    {
                        var vehicleDetails = _domesticService.GetVehicleById(item.VehicleDetailsId);
                        //GetVehicleById(int vehicleId)

                        if (vehicleDetails != null)
                        {
                            DomesticRiskDetailModel vehicleModel = Mapper.Map<Domestic_Vehicle, DomesticRiskDetailModel>(vehicleDetails);
                            var currency = _riskDetailService.GetCurrencyDetail(vehicleModel.CurrencyId);
                            vehicleList.Add(vehicleModel);
                        }
                    }
                    vehicle = vehicleList;
                    Session["VehicleDetails"] = vehicle;
                }

                model.CarInsuredCount = vehicle.Count;
                model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());
                //if (User.IsInRole("Staff") || User.IsInRole("Renewals")) // to do
                //    model.PaymentMethodId = 1;
                //else
                //    model.PaymentMethodId = 2;

           
                model.PaymentMethodId = 1;
              

                model.PaymentTermId = 1;
                model.ReceiptNumber = "";
                model.SMSConfirmation = false;
                model.TotalPremium = 0.00m;
                model.Discount = 0.00m;
                decimal temp = 0.00M;
                foreach (var item in vehicle)
                {

                    model.Premium += item.BasicPremium;
                    model.TotalPremium += item.BasicPremium + item.StampDuty;
                    var currency = _riskDetailService.GetCurrencyDetail(item.CurrencyId);
                    temp = (decimal)item.Premium;
                }
                // model.Premium = temp;
                model.TotalPremium = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
                model.TotalStampDuty = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.StampDuty)), 2);
                model.TotalCoverAmount = Math.Round(Convert.ToDecimal(vehicle.Sum(item => item.CoverAmount)), 2);

                model.MaxAmounttoPaid = Math.Round(Convert.ToDecimal(model.TotalPremium), 2);
                var vehiclewithminpremium = vehicle.OrderBy(x => x.BasicPremium).FirstOrDefault();

                if (vehiclewithminpremium != null)
                {
                    model.MinAmounttoPaid = Math.Round(Convert.ToDecimal(vehiclewithminpremium.BasicPremium + vehiclewithminpremium.StampDuty), 2);
                }
                model.AmountPaid = Convert.ToDecimal(model.TotalPremium);
                model.BalancePaidDate = DateTime.Now;
                model.Notes = "";

                if (Session["PolicyData"] != null)
                {
                    var PolicyData = (PolicyDetail)Session["PolicyData"];
                    model.InvoiceNumber = PolicyData.PolicyNumber;
                }

                if (summarydetail != null)
                {
                    model.Id = summarydetail.Id;
                }
            }
            catch (Exception ex)
            {
                // WriteLog(ex.Message);
                return View(model);
            }
            if (paymentError != "")
            {
                model.Error = "Error occurd during ecocash payment.";
                model.PaymentMethodId = (int)paymentMethod.ecocash;
            }
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitPlan(DomesticSummaryModel model, string btnSendQuatation = "")
        {
            SummaryDetailService servicedetail = new SummaryDetailService();
            try
            {
                if (model != null)
                {
                    //if (ModelState.IsValid && (model.AmountPaid >= model.MinAmounttoPaid && model.AmountPaid <= model.MaxAmounttoPaid))

                    int CustomerUniquId = 0;
                    if (User.IsInRole("Administrator"))
                    {
                        TempData["SucessMsg"] = "Admin can not create policy.";
                        return RedirectToAction("SummaryDetail");
                    }


                    TempData["ErroMsg"] = null;
                    if (User.IsInRole("Staff") && model.PaymentMethodId == 1 && btnSendQuatation == "")
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
                        List<DomesticRiskDetailModel> list = new List<DomesticRiskDetailModel>();
                        string PartnerToken = "";

                        #region update  TPIQuoteUpdate
                        var customerDetails = new Customer();

                        var policyDetils = new PolicyDetail();

                        var customerEmail = "";
                        var policyNum = "";
                        var InsuranceID = "";
                        var vichelDetails = new Domestic_Vehicle();

                        var summaryDetial = new DomesticVehicleSummary();

                        if (model.Id != 0)
                        {
                            model.CustomSumarryDetilId = model.Id;
                            summaryDetial = _domesticService.GetAllSummaryVehiclesById(model.CustomSumarryDetilId).FirstOrDefault();
                        }

                        // var summaryDetial = InsuranceContext.SummaryVehicleDetails.Single(where: $"SummaryDetailId = '" + model.CustomSumarryDetilId + "'");
                        // GetSummaryVehicleDetails


                        if (summaryDetial != null && summaryDetial.Id != 0 && btnSendQuatation == "") // while user come from qutation email
                        {
                            if (model.CustomSumarryDetilId != 0 && btnSendQuatation == "") // cehck if request is comming from agent email
                            {
                                if (model.PaymentMethodId == 1)
                                    return RedirectToAction("SaveDetailList", "Domestic", new { id = model.CustomSumarryDetilId, invoiceNumber = model.InvoiceNumber });
                                else if (model.PaymentMethodId == (int)paymentMethod.ecocash)
                                {
                                    TempData["PaymentMethodId"] = model.PaymentMethodId;
                                    return RedirectToAction("SaveDetailList", "Domestic", new { id = model.CustomSumarryDetilId, invoiceNumer = model.InvoiceNumber, Paymentid = model.PaymentMethodId.Value });
                                }
                                //else if (model.PaymentMethodId == 4)
                                //{
                                //    TempData["PaymentMethodId"] = model.PaymentMethodId;
                                //    return RedirectToAction("IceCashPayment", "Paypal", new { id = model.CustomSumarryDetilId, TotalPremiumPaid = Convert.ToString(model.AmountPaid) });
                                //}
                                else if (model.PaymentMethodId == (int)paymentMethod.Zimswitch)
                                {
                                    TempData["PaymentMethodId"] = model.PaymentMethodId;
                                    return RedirectToAction("IceCashPayment", "Domestic", new { id = model.CustomSumarryDetilId, amount = Convert.ToString(model.AmountPaid), Paymentid = model.PaymentMethodId.Value });
                                }
                                else
                                    return RedirectToAction("PaymentDetail", new { id = model.CustomSumarryDetilId, invoiceNumer = model.InvoiceNumber, Paymentid = model.PaymentMethodId.Value });
                            }
                        }
                        #endregion
                        #region Add All info to database

                        //var vehicle = (RiskDetailModel)Session["VehicleDetail"];
                        Session["SummaryDetailed"] = model;
                        string SummeryofReinsurance = "";
                        string SummeryofVehicleInsured = "";
                        bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                        var customer = (CustomerModel)Session["CustomerDataModal"];

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

                        if (role == "Staff" || role == "Renewals" || role == "Administrator")
                        {
                            // check if email id exist in user table

                            var user = UserManager.FindByEmail(customer.EmailAddress);

                            // if exist - get customer id from xcustomer table and set customer.Id in Customer object
                            if (user != null && user.Id != null)
                            {

                                //  var customerDetials = InsuranceContext.Customers.Single(where: $"UserID = '" + user.Id + "'");

                                var customerDetials = _userService.GetCustomerDetail(user.Id);

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
                                if (customer.Id == 0)
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
                                        //var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();

                                        var objCustomer = _userService.GetLastCustomerDetail();
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
                                        // InsuranceContext.Customers.Insert(customerdata);
                                        customer.Id = _userService.SaveCustomer(customerdata);
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

                                    // var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();

                                    var objCustomer = _userService.GetLastCustomerDetail();

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
                                    // InsuranceContext.Customers.Insert(customerdata);
                                    //customer.Id = customerdata.Id;
                                    customer.Id = _userService.SaveCustomer(customerdata);
                                }
                            }
                        }
                        else if (userLoggedin && userDetials != null && customer.Id == 0) //  when user is logged in
                        {
                            decimal custId = 0;

                            // var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();
                            //Query

                            var objCustomer = _userService.GetLastCustomerDetail();

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
                            //InsuranceContext.Customers.Insert(customerdata);
                            //customer.Id = customerdata.Id;

                            customer.Id = _userService.SaveCustomer(customerdata);

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
                                    // UserManager.Update(user);  // 13_june
                                }
                                // customer.UserID = User.Identity.GetUserId().ToString();
                                //  var customerDetials = InsuranceContext.Customers.Single(where: $"UserID = '" + user.Id + "'");

                                var customerDetials = _userService.GetCustomerDetail(user.Id);
                                if (customerDetials != null)
                                {
                                    // customer.UserID = user.Id;  // 13_june_2019
                                    customer.CustomerId = customerDetials.CustomerId;
                                    var customerdata = Mapper.Map<CustomerModel, Customer>(customer);

                                    if (customerdata.CustomerId == 0) // if exting record belong to 0
                                    {
                                        customerdata.CustomerId = customerdata.Id;
                                    }
                                    //   InsuranceContext.Customers.Update(customerdata); // 13_june_2019
                                }
                            }
                        }


                        var policy = (PolicyDetail)Session["PolicyData"];
                        // Genrate new policy number

                        policy.PolicyNumber = _policyService.GetLatestPolicyNumber();


                        if (policy != null)
                        {
                            if (policy.Id == 0)
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
                                policy.CurrencyId = 1;
                               // InsuranceContext.PolicyDetails.Insert(policy);
                                _policyService.SavePolicy(policy);

                                Session["PolicyData"] = policy;
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
                               // InsuranceContext.PolicyDetails.Update(policydata);
                               _policyService.UpdatePolicy(policy);
                            }
                        }


                        var vehicle = (List<DomesticRiskDetailModel>)Session["VehicleDetails"];

                        if (vehicle != null && vehicle.Count > 0)
                        {
                            foreach (var item in vehicle.ToList())
                            {
                                var _item = item;

                                
                              var   vehicelDetails = _domesticService.GetVehicleDetail(policy.Id);

                                if (vehicelDetails != null && vehicelDetails.Id!=0)
                                {
                                    item.Id = vehicelDetails.Id;
                                }

                                if (item.Id == 0)
                                {
                                    //   var service = new RiskDetailService();
                                    _item.CustomerId = customer.Id;
                                    _item.PolicyId = policy.Id;

                                    _item.Id = _domesticService.AddVehicleInformation(_item);
                                    var vehicles = (List<DomesticRiskDetailModel>)Session["VehicleDetails"];
                                    // vehicles[Convert.ToInt32(_item.NoOfCarsCovered) - 1] = _item;
                                    vehicles[0] = _item;
                                    Session["VehicleDetails"] = vehicles;

                                }
                                else
                                {

                                    Domestic_Vehicle Vehicledata = _domesticService.GetVehicleById(item.Id);
                                    Vehicledata.CoverEndDate = item.CoverEndDate.Value;
                                    Vehicledata.CoverStartDate = item.CoverStartDate.Value;

                                    Vehicledata.NoOfCarsCovered = item.NoOfCarsCovered.Value;
                                    Vehicledata.PolicyId = item.PolicyId;
                                    Vehicledata.BasicPremium = item.Premium;
                                    Vehicledata.Rate = item.Rate;
                                    Vehicledata.StampDuty = item.StampDuty;
                                    Vehicledata.TransactionDate = DateTime.Now;
                                    Vehicledata.CustomerId = customer.Id;
                                    _domesticService.UpdateVehicle(Vehicledata);

                                    var _summary = (DomesticSummaryModel)Session["SummaryDetailed"];

                                }
                            }
                        }

                        var summary = (DomesticSummaryModel)Session["SummaryDetailed"];
                        var DbEntry = Mapper.Map<DomesticSummaryModel, DomesticSummaryDetail>(model);


                        if (summary != null)
                        {
                            if (summary.Id == 0)
                            {
                                if (Session["VehicleDetails"] != null) // forcelly check because in some case summary details id is comming 0
                                {
                                    var vehicalDetailsForSummary = (List<DomesticRiskDetailModel>)Session["VehicleDetails"];
                                    if (vehicalDetailsForSummary[0].Id != 0)
                                    {
                                        var SummaryVehicalDetails = _domesticService.GetSummaryVehicleDetailsByVehicle(vehicalDetailsForSummary[0].Id);
                                        if (SummaryVehicalDetails.Count() > 0)
                                        {
                                            summary.Id = SummaryVehicalDetails[0].SummaryDetailId;
                                        }
                                    }
                                }
                            }

                            if (summary == null || summary.Id == 0)
                            {
                                DbEntry.CustomerId = customer.Id;
                                bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                                if (_userLoggedin)
                                {
                                    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                                    var _customerData = _userService.GetCustomerDetail(_User.Id);

                                    if (_customerData != null)
                                    {
                                        DbEntry.CreatedBy = _customerData.Id;
                                    }
                                }


                                DbEntry.CreatedOn = DateTime.Now;
                                if (DbEntry.BalancePaidDate.Year == 0001)
                                {
                                    DbEntry.BalancePaidDate = DateTime.Now;
                                }
                                if (DbEntry.Notes == null)
                                {
                                    DbEntry.Notes = "";
                                }

                                if (!string.IsNullOrEmpty(btnSendQuatation))
                                {
                                    DbEntry.isQuotation = true;
                                }

                                model.Id = _domesticService.SaveSummaryDetails(DbEntry);
                                Session["SummaryDetailed"] = model;
                            }
                            else
                            {
                                // SummaryDetail summarydata = InsuranceContext.SummaryDetails.All(summary.Id.ToString()).FirstOrDefault(); // on 05-oct for updatig qutation

                                var summarydata = Mapper.Map<DomesticSummaryModel, DomesticSummaryDetail>(model);
                                summarydata.Id = summary.Id;
                                summarydata.CreatedOn = DateTime.Now;

                                if (!string.IsNullOrEmpty(btnSendQuatation))
                                {
                                    summarydata.isQuotation = true;
                                }


                                //summarydata.PaymentTermId = Convert.ToInt32(Session["policytermid"]);
                                //summarydata.VehicleDetailId = vehicle[0].Id;


                                bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                                if (_userLoggedin)
                                {
                                    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());

                                    // var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

                                    var _customerData = _userService.GetCustomerDetail(_User.Id);

                                    if (_customerData != null)
                                    {
                                        summarydata.CreatedBy = _customerData.Id;
                                    }
                                }


                                summarydata.ModifiedBy = customer.Id;
                                summarydata.ModifiedOn = DateTime.Now;
                                if (summarydata.BalancePaidDate.Year == 0001)
                                {
                                    summarydata.BalancePaidDate = DateTime.Now;
                                }
                                if (DbEntry.Notes == null)
                                {
                                    summarydata.Notes = "";
                                }
                                //summarydata.CustomerId = vehicle[0].CustomerId;

                                summarydata.CustomerId = customer.Id;
                                _domesticService.UpdateSummaryDetail(summarydata);
                            }
                        }


                        if (vehicle != null && vehicle.Count > 0 && summary.Id != 0 && summary.Id > 0)
                        {
                            // var SummaryDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summary.Id}").ToList();
                            var SummaryDetails = _domesticService.GetSummaryVehicleList(summary.Id);

                            if (SummaryDetails != null && SummaryDetails.Count > 0)
                            {
                                foreach (var item in SummaryDetails)
                                {
                                    _domesticService.DeleteSummaryVehicleDetails(item);
                                }
                            }

                            foreach (var item in vehicle.ToList())
                            {
                                try
                                {
                                    var summarydetails = new DomesticVehicleSummary();
                                    summarydetails.SummaryDetailId = summary.Id;
                                    summarydetails.VehicleDetailsId = item.Id;
                                    summarydetails.CreatedBy = customer.Id;
                                    summarydetails.CreatedOn = DateTime.Now;
                                    _domesticService.SaveSummaryVehicleDetails(summarydetails);
                                }
                                catch (Exception ex)
                                {
                                    Insurance.Service.EmailService log = new Insurance.Service.EmailService();
                                    //log.WriteLog("exception during insert vehicel :" + ex.Message);

                                }
                            }
                        }

                        #endregion
                        //#region Quotation Email
                        //if (!string.IsNullOrEmpty(btnSendQuatation))
                        //{
                        //    List<Domestic_Vehicle> ListOfVehicles = new List<Domestic_Vehicle>();
                        //    var SummaryVehicleDetails = _domesticService.GetSummaryVehicleList(model.Id);

                        //    foreach (var itemSummaryVehicleDetails in SummaryVehicleDetails)
                        //    {
                        //        var itemVehicle = _domesticService.GetVehicleById(itemSummaryVehicleDetails.VehicleDetailsId);
                        //        ListOfVehicles.Add(itemVehicle);
                        //    }


                        //    var currencylist = servicedetail.GetAllCurrency();
                        //    string CurrencyName = "";

                        //    //List<VehicleDetail> ListOfVehicles = new List<VehicleDetail>();
                        //    string Summeryofcover = "";
                        //    var RoadsideAssistanceAmount = 0.00m;
                        //    var MedicalExpensesAmount = 0.00m;
                        //    var ExcessBuyBackAmount = 0.00m;
                        //    var PassengerAccidentCoverAmount = 0.00m;
                        //    var ExcessAmount = 0.00m;

                        //    var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };

                        //    string converType = "";

                        //    foreach (var item in ListOfVehicles)
                        //    {
                        //        Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();

                        //        string paymentTermsNmae = "";
                        //        var paymentTermVehicel = ePaymentTermData.FirstOrDefault(p => p.ID == item.PaymentTermId);


                        //        if (item.PaymentTermId == 1)
                        //            paymentTermsNmae = "Annual";
                        //        else if (item.PaymentTermId == 4)
                        //            paymentTermsNmae = "Termly";
                        //        else
                        //            paymentTermsNmae = paymentTermVehicel.Name + " Months";

                        //        // var vehicledetail = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);

                        //        var vehicledetail = _riskDetailService.GetVehicleDetails(SummaryVehicleDetails[0].VehicleDetailsId);

                        //        CurrencyName = servicedetail.GetCurrencyName(currencylist, vehicledetail.CurrencyId);
                        //        string policyPeriod = item.CoverStartDate.ToString("dd/MM/yyyy") + " - " + item.CoverEndDate.ToString("dd/MM/yyyy");

                        //        // to do
                        //      //  Summeryofcover += "<tr> <td style='padding: 7px 10px; font - size:15px;'>" + item.RegistrationNo + " </td> <td style='padding: 7px 10px; font - size:15px;'>" + vehicledescription + "</td><td style='padding: 7px 10px; font - size:15px;'>" + CurrencyName + item.CoverAmount + "</td><td style='padding: 7px 10px; font - size:15px;'>" + converType + "</td><td style='padding: 7px 10px; font - size:15px;'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</td> <td style='padding: 7px 10px; font - size:15px;'>" + policyPeriod + "</td><td style='padding: 7px 10px; font - size:15px;'>" + paymentTermsNmae + "</td><td style='padding: 7px 10px; font - size:15px;'>" + CurrencyName + Convert.ToString(item.PremiumDue) + "</td></tr>";
                        //    }


                        //    var summaryDetail = _domesticService.GetSummaryDetail(model.Id);

                        //    if (summaryDetail != null)
                        //    {
                        //        model.CustomSumarryDetilId = summaryDetail.Id;
                        //    }

                        //    string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
                        //    var customerQuotation = _userService.GetCustomerDetailById(summaryDetail.CustomerId);

                        //    var user = UserManager.FindById(customerQuotation.UserID);

                        //    var vehicleQuotation = _riskDetailService.GetVehicleDetails(SummaryVehicleDetails[0].VehicleDetailsId);

                        //    var policyQuotation = _policyService.GetPolicyDetailById(vehicleQuotation.PolicyId);

                        //    var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == vehicleQuotation.PaymentTermId);


                        //    Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();

                        //    string QuotationEmailPath = "/Views/Shared/EmaiTemplates/QuotationEmail.cshtml";


                        //    string urlPath = WebConfigurationManager.AppSettings["urlPath"];

                        //    string rootPath = urlPath + "/CustomerRegistration/SummaryDetail?summaryDetailId=" + summaryDetail.Id;

                        //    // need to do work

                        //    // Product name

                        //    string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(QuotationEmailPath));
                        //    var Bodyy = MotorBody.Replace("##PolicyNo##", policyQuotation.PolicyNumber).Replace("##path##", filepath).Replace("##Cellnumber##", user.PhoneNumber).
                        //        Replace("##FirstName##", customerQuotation.FirstName).Replace("##LastName##", customerQuotation.LastName).Replace("##Email##", user.Email).
                        //        Replace("##BirthDate##", customerQuotation.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", customerQuotation.AddressLine1).
                        //        Replace("##Address2##", customerQuotation.AddressLine2).Replace("##Renewal##", vehicleQuotation.RenewalDate.Value.ToString("dd/MM/yyyy")).
                        //        Replace("##InceptionDate##", vehicleQuotation.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name + " Months").
                        //        Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (vehicleQuotation.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + " Months")).
                        //        Replace("##TotalPremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty)).
                        //        Replace("##MotorLevy##", Convert.ToString(summaryDetail.TotalZTSCLevies)).
                        //        Replace("##PremiumDue##", Convert.ToString(summaryDetail.TotalPremium - summaryDetail.TotalStampDuty - summaryDetail.TotalZTSCLevies - summaryDetail.TotalRadioLicenseCost + ListOfVehicles.Sum(x => x.Discount) - ListOfVehicles.Sum(x => x.VehicleLicenceFee))).
                        //        Replace("##PostalAddress##", customerQuotation.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).
                        //        Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).
                        //        Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(summaryDetail.TotalRadioLicenseCost)).
                        //        Replace("##Discount##", Convert.ToString(ListOfVehicles.Sum(x => x.Discount)))
                        //        .Replace("##ExcessAmount##", Convert.ToString(ExcessAmount))
                        //        .Replace("##CurrencyNames##", CurrencyName).
                        //        Replace("##SummaryDetailsPath##", Convert.ToString(rootPath)).Replace("##insurance_period##", vehicleQuotation.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + vehicleQuotation.CoverEndDate.Value.ToString("dd/MM/yyyy")).
                        //        Replace("##NINumber##", customerQuotation.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));

                        //    #region Invoice PDF
                        //    var attacehmetn_File = MiscellaneousService.EmailPdf(Bodyy, policyQuotation.CustomerId, policyQuotation.PolicyNumber, "Quotation");
                        //    #endregion

                        //    #region Invoice EMail
                        //    //var _yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
                        //    List<string> _attachementss = new List<string>();
                        //    _attachementss.Add(attacehmetn_File);
                        //    //_attachementss.Add(_yAtter);


                        //    if (customer.IsCustomEmail)
                        //    {
                        //        objEmailService.SendEmail(LoggedUserEmail(), "", "", "Quotation", Bodyy, _attachementss);
                        //    }
                        //    else
                        //    {
                        //        objEmailService.SendEmail(user.Email, "", "", "Quotation", Bodyy, _attachementss);
                        //    }


                        //    #endregion

                        //    #region Send Quotation SMS
                        //    Insurance.Service.smsService objsmsService = new Insurance.Service.smsService();


                        //    // done
                        //    // string Recieptbody = "Hi " + customer.FirstName + "\nYour quote is" + "$" + Convert.ToString(summaryDetail.AmountPaid) + " for a " + converType+ " with GeneInsure. Please confirm your acceptance for policy activation. Thank you.";

                        //    string Recieptbody = "Dear " + customer.FirstName + "\nYour quote is" + "$" + Convert.ToString(summaryDetail.AmountPaid) + " for a " + converType + " with GeneInsure. Please confirm your acceptance for policy activation. Thank you.";


                        //    var Recieptresult = await objsmsService.SendSMS(customer.CountryCode.Replace("+", "") + user.PhoneNumber, Recieptbody);

                        //    SmsLog objRecieptsmslog = new SmsLog()
                        //    {
                        //        Sendto = user.PhoneNumber,
                        //        Body = Recieptbody,
                        //        Response = Recieptresult,
                        //        CreatedBy = customer.Id,
                        //        CreatedOn = DateTime.Now
                        //    };


                        //    _riskDetailService.SaveSmsLog(objRecieptsmslog);
                        //    #endregion


                        //    Session.Remove("CustomerDataModal");
                        //    Session.Remove("PolicyData");
                        //    Session.Remove("VehicleDetails");
                        //    Session.Remove("SummaryDetailed");
                        //    Session.Remove("CardDetail");
                        //    Session.Remove("issummaryformvisited");
                        //    Session.Remove("PaymentId");
                        //    Session.Remove("InvoiceId");


                        //    TempData["SucessMsg"] = "Quotation has been sent email sucessfully.";


                        //    bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                        //    if (_userLoggedin)
                        //    {
                        //        return RedirectToAction("QuotationList", "Account");
                        //    }
                        //    else
                        //    {
                        //        return Redirect("/CustomerRegistration/index");
                        //    }

                        //    // return RedirectToAction("SummaryDetail");
                        //}
                        //#endregion

                        // return RedirectToAction("InitiatePaynowTransaction", "Paypal", new { id = DbEntry.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid), PolicyNumber = policy.PolicyNumber, Email = customer.EmailAddress });

                        if (model.PaymentMethodId == 1)
                            return RedirectToAction("SaveDetailList", "Domestic", new { id = DbEntry.Id, invoiceNumer = model.InvoiceNumber, Paymentid = model.PaymentMethodId });
                        if (model.PaymentMethodId == (int)paymentMethod.ecocash)
                        {
                            TempData["PaymentMethodId"] = model.PaymentMethodId;
                            return RedirectToAction("SaveDetailList", "Domestic", new { id = DbEntry.Id, invoiceNumer = model.InvoiceNumber, Paymentid = model.PaymentMethodId.Value });
                        }
                        else if (model.PaymentMethodId == (int)paymentMethod.Zimswitch)
                        {
                            TempData["PaymentMethodId"] = model.PaymentMethodId;
                            return RedirectToAction("IceCashPayment", "Domestic", new { id = model.Id, amount = Convert.ToString(model.AmountPaid), Paymentid = model.PaymentMethodId.Value });
                        }
                        else
                            return RedirectToAction("PaymentDetail", new { id = DbEntry.Id, invoiceNumer = model.InvoiceNumber, Paymentid = model.PaymentMethodId.Value });
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

        public async Task<ActionResult> SaveDetailList(Int32 id, string invoiceNumber = "", string Paymentid = "")
        {
            var PaymentId = Session["PaymentId"];
            var InvoiceId = Session["InvoiceId"];
            if (id == 0)
            {
                id = Convert.ToInt32(TempData["SummaryId"]);
                if (id == 0)
                {
                    id = Convert.ToInt32(TempData["PaymentDetail"]);
                }
            }

            SummaryDetailService detailService = new SummaryDetailService();
            var currencylist = detailService.GetAllCurrency();
            string currencyName = "$";

            string PaymentMethod = "";
            if (Paymentid == "1")
                PaymentMethod = "CASH";
            else if (Paymentid == "2")
                PaymentMethod = "MasterCard";
            else if (Paymentid == "3")
                PaymentMethod = "EcoCash";
            else if (Paymentid == "6")
                PaymentMethod = "Zimswitch";
            else if (Paymentid == "")
                PaymentMethod = "CASH";



            var summaryDetail = _domesticService.GetSummaryDetail(id);
            if (summaryDetail != null && summaryDetail.isQuotation)
            {
                summaryDetail.isQuotation = false;
                _domesticService.UpdateSummaryDetail(summaryDetail);
            }

            var SummaryVehicleDetails = _domesticService.GetSummaryVehicleList(id);
            var vehicle = _domesticService.GetVehicleById(SummaryVehicleDetails[0].VehicleDetailsId);

            var policy = _policyService.GetPolicyDetailById(vehicle.PolicyId);
            // Generate QR Code
           // var path = SaveQRCode(policy.PolicyNumber);
            var customer = _userService.GetCustomerDetailById(summaryDetail.CustomerId);
            var product = _riskDetailService.GetProductDetailsById(vehicle.ProductId);
            var paymentInformations = _domesticService.GetPaymentInformationById(id);


            var user = UserManager.FindById(customer.UserID);

            var DebitNote = summaryDetail.DebitNote;
            DomesticPayment objSaveDetailListModel = new DomesticPayment();



            objSaveDetailListModel.CurrencyId = policy.CurrencyId;
            objSaveDetailListModel.PolicyId = vehicle.PolicyId;
            objSaveDetailListModel.CustomerId = summaryDetail.CustomerId;
            objSaveDetailListModel.SummaryDetailId = id;
            objSaveDetailListModel.DebitNote = summaryDetail.DebitNote;
            objSaveDetailListModel.ProductId = product.Id;
            objSaveDetailListModel.PaymentType = PaymentMethod;
           // objSaveDetailListModel.InvoiceId = Guid.Parse(invoiceNumber==null);
            objSaveDetailListModel.CreatedBy = customer.Id;
            objSaveDetailListModel.CreatedOn = DateTime.Now;
            objSaveDetailListModel.InvoiceNumber = policy.PolicyNumber;
            List<Domestic_Vehicle> ListOfVehicles = new List<Domestic_Vehicle>();

            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();



            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];

            string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

            var dbPaymentInformation = _domesticService.GetPaymentInformationById(id);


            if (dbPaymentInformation == null)
            {
                _domesticService.SavePaymentPaymentInformations(objSaveDetailListModel);
            }
            else
            {
                objSaveDetailListModel.Id = dbPaymentInformation.Id;
                _domesticService.UpdatePaymentInformation(objSaveDetailListModel);
            }

            if (Paymentid != Convert.ToString((int)paymentMethod.ecocash))
            {
                if (string.IsNullOrEmpty(Paymentid))
                    Paymentid = "1";

                // string res = ApproveVRNToIceCash(id, Convert.ToInt16(Paymentid));
            }




            if (!userLoggedin)
            {
                string emailTemplatePath = "/Views/Shared/EmaiTemplates/UserRegisteration.cshtml";
                string EmailBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(emailTemplatePath));
                var Body = EmailBody.Replace(" #PolicyNumber#", policy.PolicyNumber).Replace("##path##", filepath).Replace("#TodayDate#", DateTime.Now.ToShortDateString()).Replace("#FirstName#", customer.FirstName).Replace("#LastName#", customer.LastName).Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2).Replace("#Email#", user.Email).Replace("#change#", callbackUrl);
                var _yAtter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
                var attachementFile1 = MiscellaneousService.EmailPdf(Body, policy.CustomerId, policy.PolicyNumber, "WelCome Letter ");
                List<string> _attachements = new List<string>();
                _attachements.Add(attachementFile1);
                _attachements.Add(_yAtter);


                if (customer.IsCustomEmail) // if customer has custom email
                {
                    objEmailService.SendEmail(LoggedUserEmail(), "", "", "Account Creation", Body, _attachements);
                }
                else
                {
                    objEmailService.SendEmail(user.Email, "", "", "Account Creation", Body, _attachements);
                }

                string body = "Hello " + customer.FirstName + "\nWelcome to the GENE-INSURE." + " Policy number is : " + policy.PolicyNumber + "\nUsername is : " + user.Email + "\nYour Password : Geneinsure@123" + "\nPlease reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>" + "\nThank you.";
                var result = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber, body);

                SmsLog objsmslog = new SmsLog()
                {
                    Sendto = user.PhoneNumber,
                    Body = body,
                    Response = result,
                    CreatedBy = customer.Id,
                    CreatedOn = DateTime.Now
                };

                _riskDetailService.SaveSmsLog(objsmslog);

            }

            //var data = (List<Item>)Session["itemData"];
            //if (data != null)
            //{
            //    var totalprem = data.Sum(x => Convert.ToDecimal(x.price));

            //    string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/UserPaymentEmail.cshtml"; 24_jan_2019

            //   var currencyDetails = currencylist.FirstOrDefault(c => c.Id == vehicle.CurrencyId);
            //    if (currencyDetails != null)

            //        currencyName = detailService.GetCurrencyName(currencylist, vehicle.CurrencyId);

            string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/Reciept.cshtml";
            string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));
            var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString())
                .Replace("##path##", filepath).Replace("#FirstName#", customer.FirstName)
                .Replace("#LastName#", customer.LastName)
                .Replace("#AccountName#", customer.FirstName + ", " + customer.LastName)
                .Replace("#Address1#", customer.AddressLine1).Replace("#Address2#", customer.AddressLine2)
                .Replace("#currencyName#", currencyName)
                .Replace("#Amount#", Convert.ToString(summaryDetail.AmountPaid)).Replace("#PaymentDetails#", "New Premium").Replace("#ReceiptNumber#", policy.PolicyNumber).Replace("#PaymentType#", (summaryDetail.PaymentMethodId == 1 ? "Cash" : (summaryDetail.PaymentMethodId == 2 ? "PayPal" : "PayNow")));

            #region Payment Email
            var attachementFileInvoice = MiscellaneousService.EmailPdf(Body2, policy.CustomerId, policy.PolicyNumber, "Invoice");
            #region Payment Email
            #endregion

            List<string> attachements = new List<string>();
            attachements.Add(attachementFileInvoice);


            if (customer.IsCustomEmail) // if customer has custom email
            {
                objEmailService.SendEmail(LoggedUserEmail(), "", "", "Invoice", Body2, attachements);
            }
            else
            {
                objEmailService.SendEmail(user.Email, "", "", "Invoice", Body2, attachements);
            }

            #endregion
            #region Send Payment SMS
            // done
            // string Recieptbody = "Hello " + customer.FirstName + "\nWelcome to GeneInsure. Your payment of " + "$" + Convert.ToString(summaryDetail.AmountPaid) + " has been received. Policy number is : " + policy.PolicyNumber + "\n" + "\nThanks.";
            //string Recieptbody = "Hello " + customer.FirstName + "\nWelcome to GeneInsure. Please pay " + "$" + Convert.ToString(summaryDetail.AmountPaid) + " upon receiving your policy to merchant code 249341. Policy number is " + policy.PolicyNumber + "\n" + "\nThanks.";

            string Recieptbody = "Hello " + customer.FirstName + "\nWelcome to GeneInsure. Please pay " + "$" + Convert.ToString(summaryDetail.AmountPaid) + " upon receiving your policy to merchant code 249341. Policy number is " + policy.PolicyNumber + "\n" + "\nThanks.";
            var Recieptresult = await objsmsService.SendSMS(customer.Countrycode.Replace("+", "") + user.PhoneNumber, Recieptbody);

            SmsLog objRecieptsmslog = new SmsLog()
            {
                Sendto = user.PhoneNumber,
                Body = Recieptbody,
                Response = Recieptresult,
                CreatedBy = customer.Id,
                CreatedOn = DateTime.Now
            };

            _riskDetailService.SaveSmsLog(objRecieptsmslog);

            #endregion


            //foreach (var itemSummaryVehicleDetails in SummaryVehicleDetails)
            //{
            //    var itemVehicle = _domesticService.GetVehicleById(itemSummaryVehicleDetails.VehicleDetailsId);
            //    ListOfVehicles.Add(itemVehicle);
            //}

            //#region Payment PDF
            //MiscellaneousService.EmailPdf(Body2, policy.CustomerId, policy.PolicyNumber, "Reciept Payment");
            //#endregion


            //decimal totalpaymentdue = 0.00m;

            //if (vehicle.PaymentTermId == 1)
            //{
            //    totalpaymentdue = (decimal)summaryDetail.TotalPremium;
            //}
            //else if (vehicle.PaymentTermId == 4)
            //{
            //    totalpaymentdue = (decimal)summaryDetail.TotalPremium * 3;
            //}
            //else if (vehicle.PaymentTermId == 3)
            //{
            //    totalpaymentdue = (decimal)summaryDetail.TotalPremium * 4;
            //}

            //string Summeryofcover = "";
            //var RoadsideAssistanceAmount = 0.00m;
            //var MedicalExpensesAmount = 0.00m;
            //var ExcessBuyBackAmount = 0.00m;
            //var PassengerAccidentCoverAmount = 0.00m;
            //var ExcessAmount = 0.00m;
            //var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };


            //foreach (var item in ListOfVehicles)
            //{
            //    Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
            //    var paymentTermVehicel = ePaymentTermData.FirstOrDefault(p => p.ID == item.PaymentTermId);

            //    string paymentTermsName = (item.PaymentTermId == 1 ? paymentTermVehicel.Name + "(1 Year)" : paymentTermVehicel.Name + " Months");

            //    if (item.PaymentTermId == 1)
            //        paymentTermsName = "Annual";
            //    else if (item.PaymentTermId == 4)
            //        paymentTermsName = "Termly";
            //    else
            //        paymentTermsName = paymentTermVehicel.Name + " Months";

            //    string policyPeriod = item.CoverStartDate.ToString("dd/MM/yyyy") + " - " + item.CoverEndDate.ToString("dd/MM/yyyy");



            //    currencyName = detailService.GetCurrencyName(currencylist, item.CurrencyId);

            //  //  Summeryofcover += "<tr><td style='padding: 7px 10px; font - size:15px;'>" + item.RegistrationNo + " </td> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + vehicledescription + "</font></td> <td> " + item.CoverNote + " </td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + currencyName + item.SumInsured + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + (item.CoverTypeId == 4 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + policyPeriod + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + paymentTermsName + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + currencyName + Convert.ToString(item.Premium + item.Discount) + "</font></td></tr>";

            //}
            //for (int i = 0; i < SummaryVehicleDetails.Count; i++)
            //{
            //    var _vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[i].VehicleDetailsId);

            //}


            //var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == vehicle.PaymentTermId);
            //string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/SeheduleMotor.cshtml";
            //string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));

            //var Bodyy = MotorBody.Replace("##PolicyNo##", policy.PolicyNumber)
            //    .Replace("##currencyName##", currencyName)
            //    .Replace("##QRpath##", path) // save qrcode
            //    .Replace("##FirstName##", customer.FirstName).Replace("##LastName##", customer.LastName).Replace("##Email##", user.Email)
            //    .Replace("##BirthDate##", customer.DateOfBirth.Value.ToString("dd/MM/yyyy"))
            //    .Replace("##Address1##", customer.AddressLine1).Replace("##Address2##", customer.AddressLine2).Replace("##Renewal##", vehicle.RenewDate.ToString("dd/MM/yyyy"))
            //    .Replace("##InceptionDate##", vehicle.CoverStartDate.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover)
            //    .Replace("##PaymentTerm##", (vehicle.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + vehicle.PaymentTermId.ToString() + "Months)"))
            //    .Replace("##TotalPremiumDue##", Convert.ToString(summaryDetail.TotalPremium)).Replace("##StampDuty##", Convert.ToString(summaryDetail.TotalStampDuty))
            //    .Replace("##PremiumDue##", Convert.ToString(summaryDetail.TotalPremium))
            //    .Replace("##PostalAddress##", customer.Zipcode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount))
            //    .Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount));
                

            //#region Invoice PDF
            //var attacehmetnFile = MiscellaneousService.EmailPdf(Bodyy, policy.CustomerId, policy.PolicyNumber, "Schedule-motor");
            //var Atter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";


            //#endregion
            //List<string> __attachements = new List<string>();
            //__attachements.Add(attacehmetnFile);

            //__attachements.Add(Atter);


            #region Invoice EMail


            //if (customer.IsCustomEmail)
            //{
            //    objEmailService.SendEmail(LoggedUserEmail(), "", "", "Schedule-motor", Bodyy, __attachements);
            //}
            //else
            //{
            //    objEmailService.SendEmail(user.Email, "", "", "Schedule-motor", Bodyy, __attachements);
            //}

            #endregion
            #region Remove  All Sessions
            try
            {
                ClearSession();
            }
            catch (Exception ex)
            {
                ClearSession();
            }

            #endregion

            return RedirectToAction("ThankYou");
        }

        public void ClearSession()
        {
            Session.Remove("CustomerDataModal");
            Session.Remove("PolicyData");
            Session.Remove("VehicleDetails");
            Session.Remove("SummaryDetailed");
            Session.Remove("CardDetail");
            Session.Remove("issummaryformvisited");
            Session.Remove("PaymentId");
            Session.Remove("InvoiceId");
        }

        public ActionResult ThankYou()
        {
            return View();
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
                        _policyService.Base64ToImage(Convert.ToBase64String(ms.ToArray())).Save(Server.MapPath("~/QRCode/" + Policyno + ".jpg"));
                        path = urlPath + "/QRCode/" + Policyno + ".jpg";
                    }

                    Codes.PolicyNo = Policyno;
                    Codes.Qrcode = Policyno;
                    Codes.ReadBy = "";
                    Codes.Deliverto = "";
                    Codes.Createdon = DateTime.Now;
                    Codes.Comment = "";
                    _policyService.SaveQRCode(Codes);
                }

            }
            catch (Exception ex)
            {

            }

            return path;
        }


        public ActionResult GenerateQuote(DomesticRiskDetailModel model, string btnAddVehicle = "")
        {

            int selectedIndex = 0;
            //ModelState.Remove("SumInsured");

            if (model.isUpdate)
            {
                try
                {
                    model.Id = 0;
                    RemoveRiskDetail();
                    if (ModelState.IsValid)
                    {
                        List<DomesticRiskDetailModel> listriskdetailmodel = new List<DomesticRiskDetailModel>();
                        if (Session["VehicleDetails"] != null)
                        {
                            List<DomesticRiskDetailModel> listriskdetails = (List<DomesticRiskDetailModel>)Session["VehicleDetails"];
                            if (listriskdetails != null && listriskdetails.Count > 0)
                            {
                                listriskdetailmodel = listriskdetails;
                            }
                        }
                        model.Id = listriskdetailmodel[model.vehicleindex - 1].Id;
                        model.CustomerId = listriskdetailmodel[model.vehicleindex - 1].CustomerId;

                        listriskdetailmodel[model.vehicleindex - 1] = model;

                        Session["VehicleDetails"] = listriskdetailmodel;
                    }
                    else
                    {
                    }

                    if (btnAddVehicle == "")
                    {
                        return RedirectToAction("SummaryDetail");
                    }
                    else
                    {
                        // while click on updat button or submit buttton without add more.
                        if (User.IsInRole("Staff"))
                        {
                            return RedirectToAction("RiskDetail", new { id = 0 });
                        }
                        else
                        {
                            return RedirectToAction("RiskDetail", new { id = 0 });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // WriteLog(ex.Message);
                    return RedirectToAction("RiskDetail");
                }
            }
            else
            {
                try
                {
                    if (model.chkAddVehicles)
                    {
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
                        RemoveRiskDetail();
                        if (ModelState.IsValid)
                        {
                            model.Id = 0;

                            List<DomesticRiskDetailModel> listriskdetailmodel = new List<DomesticRiskDetailModel>();
                            if (Session["VehicleDetails"] != null) // 06 march
                            {
                                List<DomesticRiskDetailModel> listriskdetails = (List<DomesticRiskDetailModel>)Session["VehicleDetails"];
                                if (listriskdetails != null && listriskdetails.Count > 0)
                                {
                                    listriskdetailmodel = listriskdetails;
                                }
                            }

                            listriskdetailmodel.Add(model);

                            Session["VehicleDetails"] = listriskdetailmodel;

                            selectedIndex = listriskdetailmodel.Count();
                        }
                        return RedirectToAction("RiskDetail", new { id = 0 });
                    }
                    else
                    {
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
                        RemoveRiskDetail();
                        if (ModelState.IsValid)
                        {
                            model.Id = 0;
                            List<DomesticRiskDetailModel> listriskdetailmodel = new List<DomesticRiskDetailModel>();
                            if (Session["VehicleDetails"] != null)
                            {
                                //List<RiskDetailModel> listriskdetails = (List<RiskDetailModel>)Session["VehicleDetails"];
                                List<DomesticRiskDetailModel> listriskdetails = Session["VehicleDetails"] as List<DomesticRiskDetailModel>;
                                if (listriskdetails != null && listriskdetails.Count > 0)
                                {
                                    listriskdetailmodel = listriskdetails;
                                }
                            }
                            model.Id = 0;
                            listriskdetailmodel.Add(model);
                            Session["VehicleDetails"] = listriskdetailmodel;
                        }
                        return RedirectToAction("SummaryDetail");
                    }
                }
                catch (Exception ex)
                {
                    // WriteLog(ex.Message);
                    return RedirectToAction("SummaryDetail");
                }
            }
        }

        private void RemoveRiskDetail()
        {
            ModelState.Remove("CoverTypeId");
            ModelState.Remove("ChasisNumber");
            ModelState.Remove("EngineNumber");
            ModelState.Remove("ModelId");
            ModelState.Remove("VehicleYear");
            ModelState.Remove("CubicCapacity");
            ModelState.Remove("MakeId");
            ModelState.Remove("RegistrationNo");
        }

        public void SetValueIntoSession(int summaryId)
        {
            Session["ICEcashToken"] = null;
            Session["issummaryformvisited"] = true;
            Session["SummaryDetailId"] = summaryId;

            var summaryDetail = _summaryDetailService.GetSummaryDetail(summaryId);
            var SummaryVehicleDetails = _summaryDetailService.GetSummaryVehicleList(summaryId);
            var vehicle = _riskDetailService.GetVehicleDetails(SummaryVehicleDetails[0].VehicleDetailsId);
            var policy = _riskDetailService.GetPolicyDetails(vehicle.PolicyId);
            var product = _riskDetailService.GetProductDetails(policy.PolicyName);

            Session["PolicyData"] = policy;
            List<RiskDetailModel> listRiskDetail = new List<RiskDetailModel>();

            foreach (var item in SummaryVehicleDetails)
            {
                var _vehicle = _riskDetailService.GetVehicleDetails(item.VehicleDetailsId);
                RiskDetailModel riskDetail = Mapper.Map<VehicleDetail, RiskDetailModel>(_vehicle);
                listRiskDetail.Add(riskDetail);
            }

            Session["VehicleDetails"] = listRiskDetail;
            SummaryDetailModel summarymodel = Mapper.Map<SummaryDetail, SummaryDetailModel>(summaryDetail);
            summarymodel.Id = summaryDetail.Id;
            Session["SummaryDetailed"] = summarymodel;

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

        public void BindDropdown()
        {
            string path = Server.MapPath("~/Content/Countries.txt");
            var countries = System.IO.File.ReadAllText(path);
            var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
            ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));
            ViewBag.Cities = _userService.GetAllCity();
        }

        public JsonResult GetRiskCover(string ProductId = "0")
        {
            List<RiskCoverModel> model = _vehicleService.Domestic_RiskCovers(Convert.ToInt32(ProductId));
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetRiskItem(string RiskId)
        {
            var service = new VehicleService();
            // var model = service.GetVehicleUsageByRiskId(RiskId).Select(x => new VehicleUsageModel { VehUsage = x.VehUsage, Id = x.Id }).ToList();         
            // GetRiskCoverItem(string RiskCoverId)

            var model = service.GetRiskCoverItem(RiskId).Select(x => new RiskItemModel { Id = x.Id, RiskItem = x.RiskItem });

            JsonResult jsonResult = new JsonResult();
            jsonResult.Data = model;
            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return jsonResult;
        }
        public float? GetRate(string RiskId)
        {
            // var data = InsuranceContext.Domestic_RiskItems.Single(RiskId);
            var data = _vehicleService.Domestic_RiskItem(RiskId == null ? 0 : Convert.ToInt32(RiskId));
            if (data.Rate != 0)
                return (float)data.Rate;
            else
                return 0;
        }


        [HttpPost]
        public JsonResult CalculatePremiumCorporate(decimal InsuranceRate, decimal CoverAmount, int PaymentTermid)
        {

            JsonResult json = new JsonResult();
            var quote = new QuoteLogic();
            var premium = quote.CalculateDomesticPremium(InsuranceRate, CoverAmount, PaymentTermid);
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            json.Data = premium;
            return json;
        }




    }
}