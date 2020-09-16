using AutoMapper;
using Insurance.Domain;
using Insurance.Service;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using static InsuranceClaim.Controllers.CustomerRegistrationController;

namespace InsuranceClaim.Controllers
{
    public class EndorsementController : Controller
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

        // GET: Endorsement
        public ActionResult EndorsementDetials(int summaryId = 0, int? endorback = 0)
        {
            if (endorback == 0)
            {
                // RemoveSession();
                RemoveEndorsementSession();
            }

            EndorsementCustomerModel endorcustom = new EndorsementCustomerModel();
            string path = Server.MapPath("~/Content/Countries.txt");
            var countries = System.IO.File.ReadAllText(path);
            var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
            ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));
            ViewBag.Cities = InsuranceContext.Cities.All();
            if (endorback == 0)
            {
                if (summaryId != 0)
                {
                    //var endorseSummaryDetail = InsuranceContext.EndorsementSummaryDetails.All(where: $"SummaryId={sumaryid}").FirstOrDefault();

                    var summaryDetail = InsuranceContext.SummaryDetails.Single(summaryId);
                    var Cusotmer = InsuranceContext.Customers.Single(summaryDetail.CustomerId);
                    endorcustom = Mapper.Map<Customer, EndorsementCustomerModel>(Cusotmer);

                    if (Cusotmer != null)
                    {
                        var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == Cusotmer.UserID);
                        if (dbUser != null)
                        {
                            endorcustom.EmailAddress = dbUser.Email; ;
                            endorcustom.PrimeryCustomerId = Cusotmer.Id;
                            endorcustom.SummaryId = summaryId;
                            endorcustom.UserEmail = dbUser.Email;

                        }
                    }
                    endorcustom.Id = 0; // for endersoment customer
                    endorcustom.PrimeryCustomerId = Cusotmer.Id;

                    Session["EnSummaryDetailIdView"] = summaryId;

                }
                return View(endorcustom);
            }

            else
            {
                var customer = (EndorsementCustomer)Session["EnCustomerDetail"];
                var endorsuser = Session["Enuser"];


                EndorsementCustomerModel obj = new EndorsementCustomerModel();
                obj.ZipCode = "00263";
                if (customer != null)
                {
                    var User = UserManager.FindById(customer.UserID);
                    obj.FirstName = customer.FirstName;
                    obj.LastName = customer.LastName;
                    obj.AddressLine1 = customer.AddressLine1;
                    obj.AddressLine2 = customer.AddressLine2;
                    obj.City = customer.City;
                    obj.Id = customer.Id;
                    obj.Country = customer.Country;
                    obj.ZipCode = customer.ZipCode;
                    obj.Gender = customer.Gender;
                    obj.NationalIdentificationNumber = customer.NationalIdentificationNumber;
                    obj.CountryCode = customer.Countrycode;
                    obj.DateOfBirth = Convert.ToString(customer.DateOfBirth);
                    obj.EmailAddress = User.Email;
                    obj.IsCustomEmail = customer.IsCustomEmail;
                    obj.PhoneNumber = customer.PhoneNumber;
                    obj.PrimeryCustomerId = customer.PrimeryCustomerId;
                    obj.UserEmail = User.Email;
                    obj.UserID = customer.UserID;

                    var summaryidDeailId = Session["EnSummaryDetailIdView"];
                    if (summaryidDeailId != null)
                    {
                        obj.SummaryId = Convert.ToInt32(summaryidDeailId);
                    }
                }
                return View(obj);
            }

        }

        [HttpPost]
        public JsonResult GetCustomerId()
        {
            var dbCustomer = InsuranceContext.UniqueCustomers.All(orderBy: "CreatedOn desc").FirstOrDefault();


            int uniqueId = 0;
            string customerUserId = "";

            if (dbCustomer != null)
            {
                uniqueId = Convert.ToInt32(dbCustomer.UniqueCustomerId);

                uniqueId = uniqueId + 1;

                customerUserId = "Guest-" + uniqueId + "@gmail.com";

                var uniquCustomer = new UniqueCustomer { UniqueCustomerId = uniqueId, CreatedOn = DateTime.Now };

                InsuranceContext.UniqueCustomers.Insert(uniquCustomer);
            }
            else
            {
                uniqueId = 1000;
                customerUserId = "Guest-" + uniqueId + "@gmail.com";
                var uniquCustomer = new UniqueCustomer { UniqueCustomerId = uniqueId, CreatedOn = DateTime.Now };

                InsuranceContext.UniqueCustomers.Insert(uniquCustomer);
            }

            return Json(customerUserId, JsonRequestBehavior.AllowGet);


        }
        public async Task<JsonResult> UpdateCustomerData(EndorsementCustomerModel model, string buttonUpdate)
        {
            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (userLoggedin)
                {

                    if (model.IsEmailUpdated)
                    {
                        var isAssigned = UserManager.Users.Any(x => x.Email == model.EmailAddress);
                        if (isAssigned)
                        {
                            return Json(new { IsError = true, error = "Email already exist. Please Enter new Email" }, JsonRequestBehavior.AllowGet);
                        }
                      
                    }

                    var summaryDetails = InsuranceContext.SummaryDetails.Single(model.SummaryId);

                    if (summaryDetails != null)
                    {
                        if (summaryDetails.CustomerId != null)
                        {
                            var customerDetails = InsuranceContext.EndorsementCustomers.Single(where: $"Id= '{model.Id}'");
                            if (customerDetails == null)
                            {
                                var customerdata = Mapper.Map<EndorsementCustomerModel, EndorsementCustomer>(model);

                                customerdata.CustomerId = summaryDetails.CustomerId.Value;
                                customerdata.PrimeryCustomerId = model.PrimeryCustomerId;
                                customerdata.UserID = model.UserID;
                                customerdata.ZipCode = model.ZipCode;
                                customerdata.CreatedOn = DateTime.Now;
                                customerdata.IsCompleted = false;
                                customerdata.DateOfBirth = Convert.ToDateTime(model.DateOfBirth);
                                InsuranceContext.EndorsementCustomers.Insert(customerdata);
                                Session["EnCustomerDetail"] = customerdata;
                                var user = UserManager.FindById(model.UserID);
                                //if (user.Email != model.EmailAddress)
                                //{
                                AspNetUsersUpdate obj = new AspNetUsersUpdate();
                                obj.Email = user.Email;
                                obj.UserName = user.UserName;
                                obj.CreatedOn = DateTime.Now;
                                obj.UpdatedEmail = model.EmailAddress;
                                obj.PhoneNumber = user.PhoneNumber;
                                obj.UserId = user.Id;
                                InsuranceContext.AspNetUsersUpdates.Insert(obj);
                                //Update AspnetUser Table
                                if (model.IsEmailUpdated)
                                {
                                    var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == model.UserID);
                                    dbUser.Id = model.UserID;
                                    dbUser.Email = model.EmailAddress;
                                    dbUser.UserName = model.EmailAddress;
                                    UserManager.Update(dbUser);
                                    //Update Customer Table
                                    var custom = InsuranceContext.Customers.Single(where: $"Id = '{summaryDetails.CustomerId}'");
                                    //Customer data = new Customer();
                                    if (custom != null)
                                    {
                                        var endorcustom = Mapper.Map<EndorsementCustomerModel, Customer>(model);
                                        custom.IsCustomEmail = model.IsCustomEmail;
                                        InsuranceContext.Customers.Update(custom);
                                    }
                                }



                            }
                            else
                            {
                                if (model.IsEmailUpdated)
                                {
                                    var isAssigned = UserManager.Users.Any(x => x.Email == model.EmailAddress);
                                    if (isAssigned)
                                    {
                                        return Json(new { IsError = false, error = "Email already exist" }, JsonRequestBehavior.AllowGet);
                                    }
                                    else
                                    {
                                        var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == model.UserID);
                                        dbUser.Id = model.UserID;
                                        dbUser.Email = model.EmailAddress;
                                        dbUser.UserName = model.EmailAddress;
                                        UserManager.Update(dbUser);
                                    }
                                    var custom = InsuranceContext.Customers.Single(where: $"Id = '{model.PrimeryCustomerId}'");
                                    if (custom != null)
                                    {
                                       // Customer cudata = new Customer();
                                        var data = Mapper.Map<EndorsementCustomerModel, Customer>(model);
                                        custom.IsCustomEmail = model.IsCustomEmail;
                                        InsuranceContext.Customers.Update(custom);
                                    }


                                }


                                var _customerdata = Mapper.Map<EndorsementCustomerModel, EndorsementCustomer>(model);
                                _customerdata.CustomerId = summaryDetails.CustomerId.Value;
                                _customerdata.PrimeryCustomerId = model.PrimeryCustomerId;
                                _customerdata.UserID = model.UserID;
                                _customerdata.ZipCode = model.ZipCode;
                                _customerdata.CreatedOn = _customerdata.CreatedOn;
                                _customerdata.DateOfBirth = Convert.ToDateTime(model.DateOfBirth);
                                _customerdata.IsCompleted = false;
                                InsuranceContext.EndorsementCustomers.Update(_customerdata);
                                Session["EnCustomerDetail"] = _customerdata;
                                var _user = UserManager.FindById(model.UserID);
                                var aspupdate = InsuranceContext.AspNetUsersUpdates.All(where: $"UserId = '{_user.Id}'", orderBy: "Id desc", top: 1).FirstOrDefault();


                                if (aspupdate != null)
                                {
                                    AspNetUsersUpdate _obj = new AspNetUsersUpdate();
                                    aspupdate.Id = aspupdate.Id;
                                    aspupdate.Email = _user.Email;
                                    aspupdate.UserName = _user.UserName;
                                    aspupdate.CreatedOn = aspupdate.CreatedOn;
                                    aspupdate.UpdatedEmail = model.EmailAddress;
                                    aspupdate.PhoneNumber = _user.PhoneNumber;
                                    aspupdate.ModifiedOn = DateTime.Now;
                                    InsuranceContext.AspNetUsersUpdates.Update(aspupdate);

                                }
                                //var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == model.UserID);
                                //dbUser.Email = model.EmailAddress;
                                //dbUser.UserName = model.EmailAddress;
                                //UserManager.Update(dbUser);
                            }
                        }
                    }

                    return Json(new { IsError = false, error = "Your email have been updated sucessfully" }, JsonRequestBehavior.AllowGet);



                }
            }
            return Json(new { IsError = false, error = TempData["ErrorMessage"].ToString() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EndorsementInsertRiskDetails(int? id = 1)
        {
            var viewModel = new EndorsementRiskDetailModel();

            if (Session["EnSummaryDetailIdView"] != null)
            {

                InsertEndersoment(Convert.ToInt32(Session["EnSummaryDetailIdView"]));

                var Endorsesummartid = (EndorsementSummaryDetail)Session["EnsummaryId"];


                SetEndorsementValueIntoSession(Endorsesummartid.Id);

            }

            return RedirectToAction("EndorsementRiskDetails");
        }

        public void InsertEndersoment(int summaryId)
        {

            var summaryDetail = InsuranceContext.SummaryDetails.Single(summaryId);
            var paymeninfo = InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId = '{summaryId}'");
            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summaryId}").ToList();
            var Endorsmentcutom = (EndorsementCustomer)Session["EnCustomerDetail"];

            int vehicalId = 0;
            if (SummaryVehicleDetails.Count > 0)
            {
                vehicalId = SummaryVehicleDetails[0].VehicleDetailsId;
            }

            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            var _customerData = new Customer();

            ///Insertt into PolicyDetail//
            if (paymeninfo != null)
            {
                var Policy = InsuranceContext.PolicyDetails.Single(where: $"Id= '{paymeninfo.PolicyId}'");
                if (Policy != null)
                {
                    var endorsement = (EndorsementPolicyDetail)Session["PolicyDataView"];
                    if (endorsement != null)
                    {
                        var endorsementpolicy = InsuranceContext.EndorsementPolicyDetails.Single(where: $"Id = '{endorsement.Id}'");
                        if (endorsementpolicy != null)
                        {
                            var _endorsement = (EndorsementPolicyDetail)Session["PolicyDataView"];
                            // EndorsementPolicyDetail model = new EndorsementPolicyDetail();
                            endorsementpolicy.Id = _endorsement.Id;
                            endorsementpolicy.CustomerId = _endorsement.CustomerId;
                            endorsementpolicy.PolicyName = _endorsement.PolicyName;
                            endorsementpolicy.InsurerId = _endorsement.InsurerId;
                            endorsementpolicy.PolicyStatusId = _endorsement.PolicyStatusId;
                            endorsementpolicy.CurrencyId = _endorsement.CurrencyId;
                            endorsementpolicy.StartDate = _endorsement.StartDate;
                            endorsementpolicy.EndDate = _endorsement.EndDate;
                            endorsementpolicy.RenewalDate = _endorsement.RenewalDate;
                            endorsementpolicy.TransactionDate = _endorsement.TransactionDate;
                            endorsementpolicy.BusinessSourceId = _endorsement.BusinessSourceId;
                            endorsementpolicy.CreatedBy = _endorsement.CreatedBy;
                            endorsementpolicy.CreatedOn = _endorsement.CreatedOn;
                            endorsementpolicy.ModifiedBy = _endorsement.ModifiedBy;
                            endorsementpolicy.ModifiedOn = _endorsement.ModifiedOn;
                            endorsementpolicy.IsActive = _endorsement.IsActive;
                            

                            endorsementpolicy.PrimaryPolicyId = _endorsement.PrimaryPolicyId;

                            

                            InsuranceContext.EndorsementPolicyDetails.Update(endorsementpolicy);
                            Session["PolicyDataView"] = endorsementpolicy;
                        }
                        else
                        {
                            var obj = Mapper.Map<PolicyDetail, EndorsementPolicyDetailModel>(Policy);
                            var data = Mapper.Map<EndorsementPolicyDetailModel, EndorsementPolicyDetail>(obj);
                            data.EndorsementCustomerId = Endorsmentcutom.Id;
                            data.CustomerId = obj.CustomerId;
                            data.PrimaryPolicyId = obj.Id;
                            InsuranceContext.EndorsementPolicyDetails.Insert(data);
                            Session["PolicyDataView"] = data;
                        }
                    }
                    else
                    {
                        var obj = Mapper.Map<PolicyDetail, EndorsementPolicyDetailModel>(Policy);
                        var data = Mapper.Map<EndorsementPolicyDetailModel, EndorsementPolicyDetail>(obj);
                        data.EndorsementCustomerId = Endorsmentcutom.Id;
                        data.CustomerId = obj.CustomerId;
                        data.PrimaryPolicyId = obj.Id;
                        InsuranceContext.EndorsementPolicyDetails.Insert(data);
                        Session["PolicyDataView"] = data;
                    }

                }
            }


            ///Insert in EndorsementSummaryDetail////
            ///

            if (Session["EnsummaryId"] == null && Session["PolicyDataView"]!=null)
            {
                var _Endorsmentpolicy = (EndorsementPolicyDetail)Session["PolicyDataView"];
                EndorsementSummaryDetail endorsementSummaryDetail = new EndorsementSummaryDetail();
                endorsementSummaryDetail.PrimarySummaryId = summaryDetail.Id;
                endorsementSummaryDetail.EndorsementPolicyId = _Endorsmentpolicy.Id;
                endorsementSummaryDetail.VehicleDetailId = summaryDetail.VehicleDetailId;
                endorsementSummaryDetail.EndorsementCustomerId = Endorsmentcutom.Id;
                //
                endorsementSummaryDetail.CustomerId = summaryDetail.CustomerId;
                endorsementSummaryDetail.PaymentTermId = summaryDetail.PaymentTermId;
                endorsementSummaryDetail.PaymentMethodId = summaryDetail.PaymentMethodId;
                endorsementSummaryDetail.TotalSumInsured = summaryDetail.TotalSumInsured;
                endorsementSummaryDetail.TotalPremium = summaryDetail.TotalPremium;
                endorsementSummaryDetail.TotalStampDuty = summaryDetail.TotalStampDuty;
                endorsementSummaryDetail.TotalZTSCLevies = summaryDetail.TotalZTSCLevies;
                endorsementSummaryDetail.TotalRadioLicenseCost = summaryDetail.TotalRadioLicenseCost;
                endorsementSummaryDetail.AmountPaid = summaryDetail.AmountPaid;
                endorsementSummaryDetail.DebitNote = summaryDetail.DebitNote;
                endorsementSummaryDetail.ReceiptNumber = summaryDetail.ReceiptNumber;
                endorsementSummaryDetail.SMSConfirmation = summaryDetail.SMSConfirmation;
                endorsementSummaryDetail.CreatedOn = DateTime.Now;
                if (_userLoggedin)
                {
                    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                    _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();
                    endorsementSummaryDetail.CreatedBy = _customerData.Id;
                }
                endorsementSummaryDetail.ModifiedOn = summaryDetail.ModifiedOn;
                endorsementSummaryDetail.ModifiedBy = summaryDetail.ModifiedBy;
                endorsementSummaryDetail.IsActive = summaryDetail.IsActive;
                endorsementSummaryDetail.BalancePaidDate = summaryDetail.BalancePaidDate;

                endorsementSummaryDetail.Notes = summaryDetail.Notes;
                endorsementSummaryDetail.isQuotation = summaryDetail.isQuotation;
                endorsementSummaryDetail.IsCompleted = false;
                InsuranceContext.EndorsementSummaryDetails.Insert(endorsementSummaryDetail);
                Session["EnsummaryId"] = endorsementSummaryDetail;
            }
            else
            {
                // To do Update by session 
                var sessionvalue = (EndorsementSummaryDetail)Session["EnsummaryId"];

                var EnSummarydetail = InsuranceContext.EndorsementSummaryDetails.Single(where: $"Id = '{sessionvalue.Id}'");

                EnSummarydetail.Id = sessionvalue.Id;
                EnSummarydetail.SummaryId = sessionvalue.SummaryId;
                EnSummarydetail.VehicleDetailId = sessionvalue.VehicleDetailId;
                EnSummarydetail.CustomerId = sessionvalue.CustomerId;
                EnSummarydetail.PaymentMethodId = sessionvalue.PaymentMethodId;
                EnSummarydetail.PaymentTermId = sessionvalue.PaymentTermId;
                EnSummarydetail.TotalPremium = sessionvalue.TotalPremium;
                EnSummarydetail.TotalSumInsured = sessionvalue.TotalSumInsured;
                EnSummarydetail.TotalStampDuty = sessionvalue.TotalStampDuty;
                EnSummarydetail.TotalZTSCLevies = sessionvalue.TotalZTSCLevies;
                EnSummarydetail.TotalRadioLicenseCost = sessionvalue.TotalRadioLicenseCost;
                EnSummarydetail.DebitNote = sessionvalue.DebitNote;
                EnSummarydetail.ReceiptNumber = sessionvalue.ReceiptNumber;
                EnSummarydetail.SMSConfirmation = sessionvalue.SMSConfirmation;
                EnSummarydetail.CreatedBy = sessionvalue.CreatedBy;
                EnSummarydetail.CreatedOn = sessionvalue.CreatedOn;
                EnSummarydetail.ModifiedBy = sessionvalue.ModifiedBy;
                EnSummarydetail.ModifiedOn = sessionvalue.ModifiedOn;
                EnSummarydetail.IsActive = sessionvalue.IsActive;
                EnSummarydetail.AmountPaid = sessionvalue.AmountPaid;
                EnSummarydetail.BalancePaidDate = sessionvalue.BalancePaidDate;
                EnSummarydetail.Notes = sessionvalue.Notes;
                EnSummarydetail.isQuotation = sessionvalue.isQuotation;
                EnSummarydetail.IsCompleted = sessionvalue.IsCompleted;
                EnSummarydetail.PrimarySummaryId = sessionvalue.PrimarySummaryId;
                EnSummarydetail.EndorsementCustomerId = sessionvalue.EndorsementCustomerId;
                EnSummarydetail.EndorsementPolicyId = sessionvalue.EndorsementPolicyId;
                EnSummarydetail.EndorsementVehicleId = sessionvalue.EndorsementVehicleId;
                InsuranceContext.EndorsementSummaryDetails.Update(EnSummarydetail);
                Session["EnsummaryId"] = EnSummarydetail;

            }

            if (Session["EnViewlistVehicles"] == null)
            {
                /// Insert into EndorsementVehicleDetail///
                List<EndorsementVehicleDetail> listriskdetailmodel = new List<EndorsementVehicleDetail>();
                foreach (var item in SummaryVehicleDetails)
                {
                    var vehicle = InsuranceContext.VehicleDetails.Single(where: $"Id={item.VehicleDetailsId} and IsActive=1 ");

                    if (vehicle == null)
                        continue;

                    var Endorsmentpolicy = (EndorsementPolicyDetail)Session["PolicyDataView"];

                    var vehicleInsert = new EndorsementVehicleDetail();
                    vehicleInsert.NoOfCarsCovered = vehicle.NoOfCarsCovered;
                    vehicleInsert.PolicyId = vehicle.PolicyId;
                    vehicleInsert.RegistrationNo = vehicle.RegistrationNo;
                    //Endor//
                    vehicleInsert.PrimaryVehicleId = vehicle.Id;
                    vehicleInsert.EndorsementPolicyId = Endorsmentpolicy.Id;
                    vehicleInsert.EndorsementCustomerId = Endorsmentcutom.Id;
                    //
                    vehicleInsert.MakeId = vehicle.MakeId;
                    vehicleInsert.ModelId = vehicle.ModelId;
                   // vehicleInsert.ModelId = vehicle.ModelId;
                    vehicleInsert.CubicCapacity = vehicle.CubicCapacity;
                    vehicleInsert.VehicleYear = vehicle.VehicleYear;
                    vehicleInsert.EngineNumber = vehicle.EngineNumber;
                    vehicleInsert.ChasisNumber = vehicle.ChasisNumber;
                    vehicleInsert.VehicleColor = vehicle.VehicleColor;
                    vehicleInsert.VehicleUsage = vehicle.VehicleUsage;
                    vehicleInsert.CoverTypeId = vehicle.CoverTypeId;
                    //vehicleInsert.CoverTypeId = vehicle.CoverTypeId;
                    vehicleInsert.CoverStartDate = vehicle.CoverStartDate;
                    vehicleInsert.CoverEndDate = vehicle.CoverEndDate;
                    vehicleInsert.SumInsured = vehicle.SumInsured;
                    vehicleInsert.Premium = vehicle.Premium;
                    vehicleInsert.AgentCommissionId = vehicle.AgentCommissionId;
                    vehicleInsert.Rate = vehicle.Rate;
                    vehicleInsert.StampDuty = vehicle.StampDuty;
                    vehicleInsert.ZTSCLevy = vehicle.ZTSCLevy;
                    vehicleInsert.RadioLicenseCost = vehicle.RadioLicenseCost;
                    vehicleInsert.OptionalCovers = vehicle.OptionalCovers;
                    vehicleInsert.Excess = vehicle.Excess;
                    vehicleInsert.CoverNoteNo = vehicle.CoverNoteNo;
                    vehicleInsert.ExcessType = vehicle.ExcessType;
                    //vehicleInsert.ExcessType = vehicle.ExcessType;
                    vehicleInsert.CreatedOn = DateTime.Now;
                    if (_userLoggedin)
                    {
                        var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                        _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();
                        vehicleInsert.CreatedBy = _customerData.Id;
                    }

                    vehicleInsert.IsActive = vehicle.IsActive;
                    vehicleInsert.Addthirdparty = vehicle.Addthirdparty;
                  //  vehicleInsert.Addthirdparty = vehicle.Addthirdparty;
                    vehicleInsert.AddThirdPartyAmount = vehicle.AddThirdPartyAmount;

                    vehicleInsert.PassengerAccidentCoverAmount = vehicle.PassengerAccidentCoverAmount == null ? 0 : vehicle.PassengerAccidentCoverAmount;
                   
                    vehicleInsert.ExcessBuyBackAmount = vehicle.ExcessBuyBackAmount == null ? 0 : vehicle.ExcessBuyBackAmount;

                    vehicleInsert.NumberofPersons = vehicle.NumberofPersons;
                    vehicleInsert.IsLicenseDiskNeeded = vehicle.IsLicenseDiskNeeded;

                    // boolean property
                    vehicleInsert.PassengerAccidentCover = vehicle.PassengerAccidentCover;
                    vehicleInsert.ExcessBuyBack = vehicle.ExcessBuyBack;
                    vehicleInsert.RoadsideAssistance = vehicle.RoadsideAssistance;
                    vehicleInsert.MedicalExpenses = vehicle.MedicalExpenses;

                    vehicleInsert.PassengerAccidentCoverAmountPerPerson = vehicle.PassengerAccidentCoverAmountPerPerson == null ? 0 : vehicle.PassengerAccidentCoverAmountPerPerson;
                    vehicleInsert.ExcessBuyBackPercentage = vehicle.ExcessBuyBackPercentage == null ? 0 : vehicle.ExcessBuyBackPercentage;
                   
                    vehicleInsert.ExcessAmount = vehicle.ExcessAmount;

                    vehicleInsert.PaymentTermId = vehicle.PaymentTermId;
                    vehicleInsert.ProductId = vehicle.ProductId;
                    vehicleInsert.RoadsideAssistanceAmount = vehicle.RoadsideAssistanceAmount == null ? 0 : vehicle.RoadsideAssistanceAmount;
                    vehicleInsert.MedicalExpensesAmount = vehicle.MedicalExpensesAmount == null ? 0 : vehicle.MedicalExpensesAmount;

                   

                    vehicleInsert.TransactionDate = DateTime.Now;
                    vehicleInsert.RenewalDate = vehicle.RenewalDate;
                    vehicleInsert.IncludeRadioLicenseCost = vehicle.IncludeRadioLicenseCost;
                    vehicleInsert.InsuranceId = vehicle.InsuranceId;

                    vehicleInsert.AnnualRiskPremium = vehicle.AnnualRiskPremium == null ? 0 : vehicle.AnnualRiskPremium;
                    vehicleInsert.TermlyRiskPremium = vehicle.TermlyRiskPremium == null ? 0 : vehicle.TermlyRiskPremium;
                    vehicleInsert.QuaterlyRiskPremium = vehicle.QuaterlyRiskPremium == null ? 0 : vehicle.QuaterlyRiskPremium;
                    vehicleInsert.Discount = vehicle.Discount;

                    vehicleInsert.isLapsed = vehicle.isLapsed;
                    vehicleInsert.BalanceAmount = vehicle.BalanceAmount;
                    vehicleInsert.VehicleLicenceFee = vehicle.VehicleLicenceFee;
                    vehicleInsert.BusinessSourceId = vehicle.BusinessSourceDetailId;
                    vehicleInsert.CurrencyId = vehicle.CurrencyId;

                    vehicleInsert.RoadsideAssistancePercentage = vehicle.RoadsideAssistancePercentage == null ? 0 : vehicle.RoadsideAssistancePercentage;
                    vehicleInsert.MedicalExpensesPercentage = vehicle.MedicalExpensesPercentage == null ? 0 : vehicle.MedicalExpensesPercentage;
                    vehicleInsert.IsCompleted = false;
                    vehicleInsert.TaxClassId = vehicle.TaxClassId;
                    vehicleInsert.CombinedID = vehicle.CombinedID;

                    InsuranceContext.EndorsementVehicleDetails.Insert(vehicleInsert);
                    //var vehicleId = vehicleInsert;
                    //Session["vehicleId"] = vehicleInsert;
                    listriskdetailmodel.Add(vehicleInsert);
                    Session["EnViewlistVehicles"] = listriskdetailmodel;
                }

                //// insert into EndorsementSummaryVehicleDetails///
                var _Endorsmentvehicle = (List<EndorsementVehicleDetail>)Session["EnViewlistVehicles"];
                List<EndorsementSummaryVehicleDetail> listsummaryvehiclemodel = new List<EndorsementSummaryVehicleDetail>();
                if (_Endorsmentvehicle != null)
                {
                    foreach (var item in _Endorsmentvehicle)
                    {

                        EndorsementSummaryVehicleDetail summaryVehicalDetials = new EndorsementSummaryVehicleDetail();
                        var endorsesummayid = (EndorsementSummaryDetail)Session["EnsummaryId"];
                        summaryVehicalDetials.SummaryDetailId = summaryId;
                        summaryVehicalDetials.VehicleDetailsId = Convert.ToInt32(item.PrimaryVehicleId);
                        summaryVehicalDetials.CreatedOn = DateTime.Now;
                        summaryVehicalDetials.CreatedBy = _customerData.Id;
                        summaryVehicalDetials.ModifiedOn = DateTime.Now;
                        summaryVehicalDetials.ModifiedBy = _customerData.Id;
                        summaryVehicalDetials.IsCompleted = false;
                        //endorse//
                        summaryVehicalDetials.EndorsementVehicleId = item.Id;
                        summaryVehicalDetials.EndorsementSummaryId = endorsesummayid.Id;
                        ////
                        InsuranceContext.EndorsementSummaryVehicleDetails.Insert(summaryVehicalDetials);
                        listsummaryvehiclemodel.Add(summaryVehicalDetials);
                        Session["EndorsementSummaryVehiclrDetail"] = listsummaryvehiclemodel;

                    }
                }

            }
            else
            {
                var summarysessionvalue = (List<EndorsementRiskDetailModel>)Session["EnViewlistVehicles"];
                List<EndorsementVehicleDetail> _listriskdetailmodel = new List<EndorsementVehicleDetail>();
                foreach (var item in summarysessionvalue)
                {
                    var _vehicleInsert = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id = '{item.Id}'");

                    _vehicleInsert.Id = item.Id;
                    _vehicleInsert.VehicleId = item.VehicleId;
                    _vehicleInsert.NoOfCarsCovered = item.NoOfCarsCovered;
                    _vehicleInsert.PolicyId = item.PolicyId;
                    _vehicleInsert.RegistrationNo = item.RegistrationNo;
                    _vehicleInsert.CustomerId = item.CustomerId;
                    _vehicleInsert.MakeId = item.MakeId;
                    _vehicleInsert.ModelId = item.ModelId;
                    _vehicleInsert.CubicCapacity = item.CubicCapacity;
                    _vehicleInsert.VehicleYear = item.VehicleYear;
                    _vehicleInsert.EngineNumber = item.EngineNumber;
                    _vehicleInsert.ChasisNumber = item.ChasisNumber;
                    _vehicleInsert.VehicleColor = item.VehicleColor;
                    _vehicleInsert.VehicleUsage = item.VehicleUsage;
                    _vehicleInsert.CoverTypeId = item.CoverTypeId;
                    _vehicleInsert.CoverStartDate = item.CoverStartDate;
                    _vehicleInsert.CoverEndDate = item.CoverEndDate;
                    _vehicleInsert.SumInsured = item.SumInsured;
                    _vehicleInsert.Premium = item.Premium;
                    _vehicleInsert.AgentCommissionId = item.AgentCommissionId;
                    _vehicleInsert.Rate = item.Rate;
                    _vehicleInsert.StampDuty = item.StampDuty;
                    _vehicleInsert.ZTSCLevy = item.ZTSCLevy;
                    _vehicleInsert.RadioLicenseCost = item.RadioLicenseCost;
                    _vehicleInsert.OptionalCovers = item.OptionalCovers;
                    _vehicleInsert.Excess = item.Excess;
                    _vehicleInsert.CoverNoteNo = item.CoverNoteNo;
                    _vehicleInsert.CreatedBy = item.CreatedBy;
                    _vehicleInsert.CreatedOn = item.CreatedOn;
                    _vehicleInsert.ModifiedBy = item.ModifiedBy;
                    _vehicleInsert.ModifiedOn = item.ModifiedOn;
                    _vehicleInsert.IsActive = item.IsActive;
                    _vehicleInsert.Addthirdparty = item.Addthirdparty;
                    _vehicleInsert.AddThirdPartyAmount = item.AddThirdPartyAmount;
                    _vehicleInsert.PassengerAccidentCover = item.PassengerAccidentCover;
                    _vehicleInsert.ExcessBuyBack = item.ExcessBuyBack;
                    _vehicleInsert.RoadsideAssistance = item.RoadsideAssistance;
                    _vehicleInsert.MedicalExpenses = item.MedicalExpenses;
                    _vehicleInsert.NumberofPersons = item.NumberofPersons;
                    _vehicleInsert.IsLicenseDiskNeeded = item.IsLicenseDiskNeeded;
                    _vehicleInsert.PassengerAccidentCoverAmount = item.PassengerAccidentCoverAmount;
                    _vehicleInsert.ExcessBuyBackAmount = item.ExcessBuyBackAmount;
                    _vehicleInsert.PaymentTermId = item.PaymentTermId;
                    _vehicleInsert.ProductId = item.ProductId;
                    _vehicleInsert.RoadsideAssistanceAmount = item.RoadsideAssistanceAmount;
                    _vehicleInsert.MedicalExpensesAmount = item.MedicalExpensesAmount;
                    _vehicleInsert.PassengerAccidentCoverAmountPerPerson = item.PassengerAccidentCoverAmountPerPerson;
                    _vehicleInsert.ExcessBuyBackPercentage = item.ExcessBuyBackPercentage;
                    _vehicleInsert.RoadsideAssistancePercentage = item.RoadsideAssistancePercentage;
                    _vehicleInsert.MedicalExpensesPercentage = item.MedicalExpensesPercentage;
                    _vehicleInsert.ExcessAmount = item.ExcessAmount;
                    _vehicleInsert.RenewalDate = item.RenewalDate;
                    _vehicleInsert.TransactionDate = item.TransactionDate;
                    _vehicleInsert.IncludeRadioLicenseCost = item.IncludeRadioLicenseCost;
                    _vehicleInsert.InsuranceId = item.InsuranceId;
                    _vehicleInsert.AnnualRiskPremium = item.AnnualRiskPremium;
                    _vehicleInsert.TermlyRiskPremium = item.TermlyRiskPremium;
                    _vehicleInsert.QuaterlyRiskPremium = item.QuaterlyRiskPremium;
                    _vehicleInsert.Discount = item.Discount;
                    _vehicleInsert.isLapsed = Convert.ToBoolean(item.isLapsed);
                    _vehicleInsert.BalanceAmount = item.BalanceAmount;
                    _vehicleInsert.VehicleLicenceFee = item.VehicleLicenceFee;
                    _vehicleInsert.BusinessSourceId = item.BusinessSourceId;
                    _vehicleInsert.IsCompleted = item.IsCompleted;
                    _vehicleInsert.EndorsementCustomerId = item.EndorsementCustomerId;
                    _vehicleInsert.EndorsementPolicyId = item.EndorsementPolicyId;
                    _vehicleInsert.PrimaryVehicleId = item.PrimaryVehicleId;
                    _vehicleInsert.TaxClassId = item.TaxClassId;
                    InsuranceContext.EndorsementVehicleDetails.Update(_vehicleInsert);
                    _listriskdetailmodel.Add(_vehicleInsert);
                    Session["EnViewlistVehicles"] = _listriskdetailmodel;

                }
                //// Update into EndorsementSummaryVehicleDetails///
                var Endorsmentvehicle = (List<EndorsementVehicleDetail>)Session["EnViewlistVehicles"];
                List<EndorsementSummaryVehicleDetail> _listsummaryvehiclemodel = new List<EndorsementSummaryVehicleDetail>();
                if (Endorsmentvehicle != null)
                {
                    foreach (var item in Endorsmentvehicle)
                    {

                        EndorsementSummaryVehicleDetail summaryVehicalDetials = new EndorsementSummaryVehicleDetail();
                        var endorsesummayid = (EndorsementSummaryDetail)Session["EnsummaryId"];
                        summaryVehicalDetials.Id = item.Id;
                        summaryVehicalDetials.SummaryDetailId = summaryId;
                        summaryVehicalDetials.VehicleDetailsId = Convert.ToInt32(item.PrimaryVehicleId);
                        summaryVehicalDetials.CreatedOn = DateTime.Now;
                        summaryVehicalDetials.CreatedBy = _customerData.Id;
                        summaryVehicalDetials.ModifiedOn = DateTime.Now;
                        summaryVehicalDetials.ModifiedBy = _customerData.Id;
                        summaryVehicalDetials.IsCompleted = false;
                        //endorse//
                        summaryVehicalDetials.EndorsementVehicleId = item.Id;
                        summaryVehicalDetials.EndorsementSummaryId = endorsesummayid.Id;
                        ////
                        InsuranceContext.EndorsementSummaryVehicleDetails.Update(summaryVehicalDetials);
                        _listsummaryvehiclemodel.Add(summaryVehicalDetials);
                        Session["EndorsementSummaryVehiclrDetail"] = _listsummaryvehiclemodel;
                    }
                }

            }
        }

        public void SetEndorsementValueIntoSession(int Endorsementsummaryid)
        {
            //Session["SummaryDetailIdView"] = Endorsementsummaryid;

            var endorsesummaryDetail = InsuranceContext.EndorsementSummaryDetails.Single(where: $"Id={Endorsementsummaryid}");
            var endorseSummaryVehicleDetails = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"EndorsementSummaryId={Endorsementsummaryid}").ToList();


            var endorsevehicle = InsuranceContext.EndorsementVehicleDetails.All(where: $"Id={endorseSummaryVehicleDetails[0].EndorsementVehicleId}").FirstOrDefault();
            var endorsepolicy = InsuranceContext.EndorsementPolicyDetails.Single(endorsevehicle.EndorsementPolicyId);
            //var endorseproduct = InsuranceContext.Products.Single(Convert.ToInt32(endorsepolicy.PolicyName));
            Session["PolicyDataView"] = endorsepolicy;

            List<EndorsementRiskDetailModel> listRiskDetail = new List<EndorsementRiskDetailModel>();
            foreach (var item in endorseSummaryVehicleDetails)
            {
                var _vehicle = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id={item.EndorsementVehicleId}");
                EndorsementRiskDetailModel riskDetail = Mapper.Map<EndorsementVehicleDetail, EndorsementRiskDetailModel>(_vehicle);
                riskDetail.PrimaryVehicleId = Convert.ToInt32(_vehicle.PrimaryVehicleId);
                listRiskDetail.Add(riskDetail);
            }
            Session["EnViewlistVehicles"] = listRiskDetail;

            EndorsementSummaryDetailModel summarymodel = Mapper.Map<EndorsementSummaryDetail, EndorsementSummaryDetailModel>(endorsesummaryDetail);
            summarymodel.Id = endorsesummaryDetail.Id;
            Session["ENViewSummaryDetail"] = endorsesummaryDetail;


        }
        public ActionResult EndorsementRiskDetails(int? id = 1)
        {
            var viewModel = new EndorsementRiskDetailModel();
            var ensummertdetail = (EndorsementSummaryDetail)Session["EnsummaryId"];
            viewModel.SummaryId = ensummertdetail.PrimarySummaryId;
            var eExcessTypeData = from eExcessType e in Enum.GetValues(typeof(eExcessType))
                                  select new
                                  {
                                      ID = (int)e,
                                      Name = e.ToString()
                                  };

            ViewBag.eExcessTypeData = new SelectList(eExcessTypeData, "ID", "Name");

            ViewBag.Products = InsuranceContext.Products.All().ToList();

            ViewBag.Currencies = InsuranceContext.Currencies.All();

            ViewBag.TaxClass = InsuranceContext.VehicleTaxClasses.All().ToList();

            ViewBag.VehicleLicensePaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is Null").ToList();
            ViewBag.RadioLicensePaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is Null").ToList();



            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm))
                                   select new
                                   {
                                       ID = (int)e,
                                       Name = e.ToString()
                                   };

            ViewBag.ePaymentTermData = new SelectList(ePaymentTermData, "ID", "Name");
            int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var PolicyData = (EndorsementPolicyDetail)Session["PolicyDataView"];
            // Id is policyid from Policy detail table

            var service = new VehicleService();

            ViewBag.VehicleUsage = service.GetAllVehicleUsage();
            viewModel.NumberofPersons = 0;
            viewModel.AddThirdPartyAmount = 0.00m;
            viewModel.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType();
            ViewBag.AgentCommission = service.GetAgentCommission();
            ViewBag.Sources = InsuranceContext.BusinessSources.All();
            ViewBag.Makers = makers;
            viewModel.isUpdate = false;
            viewModel.VehicleUsage = 0;

            viewModel.CurrencyId = 6; // default "RTGS$" selected

            var _list = "";
            //  TempData["Policy"] = service.GetPolicy(id);
            TempData["Policy"] = PolicyData;
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;
            }

            if (TempData["ViewModel"] != null)
            {

                viewModel = (EndorsementRiskDetailModel)TempData["ViewModel"];
                return View(viewModel);
            }



            viewModel.NoOfCarsCovered = 1;

            if (Session["EnViewlistVehicles"] != null)
            {
                var list = (List<EndorsementRiskDetailModel>)Session["EnViewlistVehicles"];
                if (list.Count > 0)
                {
                    viewModel.NoOfCarsCovered = list.Count + 1;
                }
            }
            if (id > 0)
            {
                var list = (List<EndorsementRiskDetailModel>)Session["EnViewlistVehicles"];
                if (list != null && list.Count > 0 && (list.Count >= id))
                {
                    var data = (EndorsementRiskDetailModel)list[Convert.ToInt32(id - 1)];
                    if (data != null)
                    {
                        viewModel.AgentCommissionId = data.AgentCommissionId;
                        viewModel.ChasisNumber = data.ChasisNumber;
                        viewModel.CoverEndDate = data.CoverEndDate;
                        viewModel.CoverNoteNo = data.CoverNoteNo;
                        viewModel.CoverStartDate = data.CoverStartDate;
                        viewModel.CoverTypeId = data.CoverTypeId;
                        viewModel.CubicCapacity = data.CubicCapacity == null ? 0 : (int)Math.Round(data.CubicCapacity.Value, 0);
                        viewModel.CustomerId = data.CustomerId;
                        viewModel.EngineNumber = data.EngineNumber;
                        // viewModel.Equals = data.Equals;
                        viewModel.Excess = (int)Math.Round(data.Excess, 0);
                        viewModel.ExcessType = data.ExcessType;
                        viewModel.MakeId = data.MakeId;
                        viewModel.ModelId = data.ModelId;
                        viewModel.NoOfCarsCovered = id;
                        viewModel.OptionalCovers = data.OptionalCovers;
                        viewModel.PolicyId = data.PolicyId;
                        viewModel.Premium = data.Premium;
                        viewModel.RadioLicenseCost = (int)Math.Round(data.RadioLicenseCost == null ? 0 : data.RadioLicenseCost.Value, 0);
                        viewModel.Rate = data.Rate;
                        viewModel.RegistrationNo = data.RegistrationNo;
                        viewModel.StampDuty = data.StampDuty;
                        viewModel.SumInsured = (int)Math.Round(data.SumInsured == null ? 0 : data.SumInsured.Value, 0);
                        viewModel.VehicleColor = data.VehicleColor;
                        viewModel.VehicleUsage = data.VehicleUsage;
                        viewModel.VehicleYear = data.VehicleYear;
                        viewModel.Id = data.Id;    
                        viewModel.ZTSCLevy = data.ZTSCLevy;
                        viewModel.NumberofPersons = data.NumberofPersons;
                        viewModel.PassengerAccidentCover = data.PassengerAccidentCover;
                        viewModel.IsLicenseDiskNeeded = data.IsLicenseDiskNeeded;
                        viewModel.ExcessBuyBack = data.ExcessBuyBack;
                        viewModel.RoadsideAssistance = data.RoadsideAssistance;
                        viewModel.MedicalExpenses = data.MedicalExpenses;
                        viewModel.Addthirdparty = data.Addthirdparty;
                        viewModel.AddThirdPartyAmount = data.AddThirdPartyAmount;
                        viewModel.ExcessAmount = data.ExcessAmount;
                        viewModel.ProductId = data.ProductId;
                        viewModel.PaymentTermId = data.PaymentTermId;
                        viewModel.IncludeRadioLicenseCost = data.IncludeRadioLicenseCost;
                        viewModel.Discount = data.Discount;
                        viewModel.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);
                        viewModel.BusinessSourceId = data.BusinessSourceId;
                        viewModel.VehicleUsage = data.VehicleUsage;
                        viewModel.isUpdate = true;
                        viewModel.vehicleindex = Convert.ToInt32(id);
                        viewModel.EndorsementVehicleId = data.Id;
                        // viewModel.VehicleId = data.VehicleId;
                        viewModel.VehicleId = data.PrimaryVehicleId.Value;
                        viewModel.EndorsementCustomerId = data.EndorsementCustomerId;
                        viewModel.EndorsementPolicyId = data.EndorsementPolicyId;
                        viewModel.PrimaryVehicleId = data.PrimaryVehicleId;
                        viewModel.BalanceAmount = data.BalanceAmount;
                        viewModel.TaxClassId = data.TaxClassId;
                        viewModel.CombinedID = data.CombinedID;


                        viewModel.IncludeLicenseFee = data.IncludeLicenseFee;
                        viewModel.IncludeRadioLicenseCost = data.IncludeRadioLicenseCost;
                        viewModel.ZinaraLicensePaymentTermId = data.ZinaraLicensePaymentTermId;
                        viewModel.RadioLicensePaymentTermId = data.RadioLicensePaymentTermId;
                        



                        viewModel.Id = data.Id;
                        var ser = new VehicleService();
                        var model = ser.GetModel(data.MakeId);
                        ViewBag.Model = model;
                    }
                }
            }
            return View(viewModel);
        }

        [HttpGet]
        public JsonResult GetEndorsementLicenseAddress()
        {
            var customerData = (EndorsementCustomerModel)Session["EnCustomerDetail"];
            //LicenseAddress licenseAddress = new LicenseAddress();
            EndorsementRiskDetailModel riskDetailModel = new EndorsementRiskDetailModel();
            riskDetailModel.LicenseAddress1 = customerData.AddressLine1;
            riskDetailModel.LicenseAddress2 = customerData.AddressLine2;
            riskDetailModel.LicenseCity = customerData.City;
            return Json(riskDetailModel, JsonRequestBehavior.AllowGet);
        }
        public JsonResult getEndorsementVehicleList(int EndorsementsummaryDetailId = 0)
        {

            try
            {

                SummaryDetailService summaryDetailService = new SummaryDetailService();
                var currencyList = summaryDetailService.GetAllCurrency();

                if (EndorsementsummaryDetailId != 0)
                {
                    if (Session["EnViewlistVehicles"] != null)
                    {
                        var list = (List<EndorsementRiskDetailModel>)Session["EnViewlistVehicles"];
                        List<VehicleListModel> vehiclelist = new List<VehicleListModel>();


                        foreach (var item in list)
                        {
                            VehicleListModel obj = new VehicleListModel();
                            obj.make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'").ShortDescription;
                            obj.model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ShortDescription;
                            obj.covertype = InsuranceContext.CoverTypes.Single(item.CoverTypeId).Name;
                            obj.premium = item.Premium.ToString();
                            obj.suminsured = item.SumInsured.ToString();
                            obj.ZTSCLevy = item.ZTSCLevy == null ? "0" : item.ZTSCLevy.ToString();
                            obj.Id = item.Id;
                            obj.CurrencyName = summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);


                            vehiclelist.Add(obj);
                        }

                        return Json(vehiclelist, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (Session["EnViewlistVehicles"] != null)
                {
                    var _list = (List<EndorsementRiskDetailModel>)Session["EnViewlistVehicles"];
                    List<VehicleListModel> _vehiclelist = new List<VehicleListModel>();
                    foreach (var item in _list)
                    {
                        VehicleListModel obj = new VehicleListModel();
                        obj.make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'").ShortDescription;
                        obj.model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ShortDescription;
                        obj.covertype = InsuranceContext.CoverTypes.Single(item.CoverTypeId).Name;
                        obj.premium = item.Premium.ToString();
                        obj.suminsured = item.SumInsured.ToString();
                        obj.ZTSCLevy = item.ZTSCLevy == null ? "0" : item.ZTSCLevy.ToString();
                        obj.RegistrationNo = item.RegistrationNo;
                        obj.excess = item.ExcessAmount == null ? "0" : item.ExcessAmount.ToString();
                        obj.vehicle_license_fee = item.VehicleLicenceFee == 0 ? "0" : item.VehicleLicenceFee.ToString();
                        obj.stampDuty = item.StampDuty == null ? "0" : item.StampDuty.ToString();
                        obj.Id = item.Id;

                        obj.CurrencyName = summaryDetailService.GetCurrencyName(currencyList, item.CurrencyId);


                        if (item.IncludeRadioLicenseCost == true)
                        {
                            obj.radio_license_fee = item.RadioLicenseCost == null ? "0" : item.RadioLicenseCost.ToString();
                        }
                        else
                        {
                            obj.radio_license_fee = "0";
                        }
                        decimal? radioLicenseCost = 0;
                        if (item.IncludeRadioLicenseCost)
                        {
                            radioLicenseCost = item.RadioLicenseCost;
                        }

                        // var calculationAmount = item.Premium + radioLicenseCost + item.Excess + item.VehicleLicenceFee + item.StampDuty + item.ZTSCLevy;

                        var calculationAmount = item.Premium + radioLicenseCost + item.VehicleLicenceFee + item.StampDuty + item.ZTSCLevy;


                        obj.total = calculationAmount.ToString();
                        obj.ZTSCLevy = item.ZTSCLevy == null ? "0" : Convert.ToString(item.ZTSCLevy);

                        _vehiclelist.Add(obj);
                    }
                    return Json(_vehiclelist, JsonRequestBehavior.AllowGet);
                }
                {

                }
                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult SaveEndorsementRiskDetails(EndorsementRiskDetailModel model)
        {

            VehicleService _service = new VehicleService();
            var validationMsg = _service.EndorsmentValidationMessage(model);

            if (validationMsg != "")
            {
                model.ErrorMessage = validationMsg;
                TempData["ViewModel"] = model;

                // if (User.IsInRole("Staff"))
                return RedirectToAction("EndorsementRiskDetails", new { id = 1 });
            }



            var dbVehicle = InsuranceContext.VehicleDetails.Single(where: $"Id={model.VehicleId}");

            //var endorsementvehicle = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id = '{model.Id}'");
            var EnderSomentVehical = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id={model.EndorsementVehicleId}");

          //  var EnderSomentVehical = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id={model.vehicleindex}");

            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;


            var vehicleUpdate = Mapper.Map<EndorsementRiskDetailModel, EndorsementVehicleDetail>(model);

            EnderSomentVehical.PrimaryVehicleId = model.PrimaryVehicleId;
            // EnderSomentVehical.Id = model.Id;
            EnderSomentVehical.Id = model.EndorsementVehicleId;
            EnderSomentVehical.EndorsementCustomerId = model.EndorsementCustomerId;
            EnderSomentVehical.EndorsementPolicyId = model.EndorsementPolicyId;
            EnderSomentVehical.CurrencyId = model.CurrencyId;
            EnderSomentVehical.NoOfCarsCovered = vehicleUpdate.NoOfCarsCovered;
            EnderSomentVehical.PolicyId = vehicleUpdate.PolicyId;
            EnderSomentVehical.RegistrationNo = vehicleUpdate.RegistrationNo;
            EnderSomentVehical.CustomerId = model.CustomerId;
            EnderSomentVehical.MakeId = vehicleUpdate.MakeId;
            EnderSomentVehical.ModelId = vehicleUpdate.ModelId;
            EnderSomentVehical.CubicCapacity = vehicleUpdate.CubicCapacity;
            EnderSomentVehical.VehicleYear = vehicleUpdate.VehicleYear;
            EnderSomentVehical.EngineNumber = vehicleUpdate.EngineNumber;
            EnderSomentVehical.ChasisNumber = vehicleUpdate.ChasisNumber;
            EnderSomentVehical.VehicleColor = vehicleUpdate.VehicleColor;
            EnderSomentVehical.VehicleUsage = vehicleUpdate.VehicleUsage;
            EnderSomentVehical.CoverTypeId = vehicleUpdate.CoverTypeId;
            EnderSomentVehical.CoverStartDate = vehicleUpdate.CoverStartDate;
            EnderSomentVehical.CoverEndDate = vehicleUpdate.CoverEndDate;
            EnderSomentVehical.SumInsured = vehicleUpdate.SumInsured;
            EnderSomentVehical.Premium = vehicleUpdate.Premium;
            EnderSomentVehical.AgentCommissionId = vehicleUpdate.AgentCommissionId;
            EnderSomentVehical.Rate = vehicleUpdate.Rate;
            EnderSomentVehical.StampDuty = vehicleUpdate.StampDuty;
            EnderSomentVehical.ZTSCLevy = vehicleUpdate.ZTSCLevy;
            EnderSomentVehical.RadioLicenseCost = vehicleUpdate.RadioLicenseCost;
            EnderSomentVehical.OptionalCovers = vehicleUpdate.OptionalCovers;
            EnderSomentVehical.Excess = vehicleUpdate.Excess;
            EnderSomentVehical.CoverNoteNo = vehicleUpdate.CoverNoteNo;
            EnderSomentVehical.ExcessType = vehicleUpdate.ExcessType;

            EnderSomentVehical.CreatedOn = DateTime.Now;
            //EnderSomentVehical.CreatedOn = vehicleUpdate.CreatedOn;
            //EnderSomentVehical.CreatedBy = vehicleUpdate.CreatedBy;
            EnderSomentVehical.ModifiedOn = DateTime.Now;
            EnderSomentVehical.ModifiedBy = vehicleUpdate.ModifiedBy;
            if (_userLoggedin)
            {
                var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

                EnderSomentVehical.ModifiedBy = _customerData.Id;
                EnderSomentVehical.CreatedBy = _customerData.Id;
            }
            EnderSomentVehical.IsActive = model.IsActive;
            EnderSomentVehical.Addthirdparty = vehicleUpdate.Addthirdparty;
            EnderSomentVehical.AddThirdPartyAmount = vehicleUpdate.AddThirdPartyAmount;
            EnderSomentVehical.PassengerAccidentCover = vehicleUpdate.PassengerAccidentCover;
            EnderSomentVehical.ExcessBuyBack = vehicleUpdate.ExcessBuyBack;
            EnderSomentVehical.RoadsideAssistance = vehicleUpdate.RoadsideAssistance;
            EnderSomentVehical.MedicalExpenses = vehicleUpdate.MedicalExpenses;
            EnderSomentVehical.NumberofPersons = vehicleUpdate.NumberofPersons;
            EnderSomentVehical.IsLicenseDiskNeeded = vehicleUpdate.IsLicenseDiskNeeded;
            EnderSomentVehical.PassengerAccidentCoverAmount = vehicleUpdate.PassengerAccidentCoverAmount == null ? 0 : vehicleUpdate.PassengerAccidentCoverAmount;
            EnderSomentVehical.ExcessBuyBackAmount = vehicleUpdate.ExcessBuyBackAmount == null ? 0 : vehicleUpdate.ExcessBuyBackAmount;
            EnderSomentVehical.PaymentTermId = vehicleUpdate.PaymentTermId;
            EnderSomentVehical.ProductId = vehicleUpdate.ProductId;
            EnderSomentVehical.RoadsideAssistanceAmount = vehicleUpdate.RoadsideAssistanceAmount == null ? 0 : vehicleUpdate.RoadsideAssistanceAmount;
            EnderSomentVehical.MedicalExpensesAmount = vehicleUpdate.MedicalExpensesAmount == null ? 0 : vehicleUpdate.MedicalExpensesAmount;
            EnderSomentVehical.PassengerAccidentCoverAmountPerPerson = vehicleUpdate.PassengerAccidentCoverAmountPerPerson == null ? 0 : vehicleUpdate.PassengerAccidentCoverAmountPerPerson;
            EnderSomentVehical.ExcessBuyBackPercentage = vehicleUpdate.ExcessBuyBackPercentage == null ? 0 : vehicleUpdate.ExcessBuyBackPercentage;
            EnderSomentVehical.RoadsideAssistancePercentage = vehicleUpdate.RoadsideAssistancePercentage == null ? 0 : vehicleUpdate.RoadsideAssistancePercentage;
            EnderSomentVehical.MedicalExpensesPercentage = vehicleUpdate.MedicalExpensesPercentage == null ? 0 : vehicleUpdate.MedicalExpensesPercentage;
            EnderSomentVehical.ExcessAmount = vehicleUpdate.ExcessAmount;
            EnderSomentVehical.RenewalDate = vehicleUpdate.RenewalDate;
            EnderSomentVehical.TransactionDate = DateTime.Now;
            EnderSomentVehical.IncludeRadioLicenseCost = vehicleUpdate.IncludeRadioLicenseCost;
            EnderSomentVehical.InsuranceId = vehicleUpdate.InsuranceId;
            EnderSomentVehical.AnnualRiskPremium = vehicleUpdate.AnnualRiskPremium == null ? 0 : vehicleUpdate.AnnualRiskPremium;
            EnderSomentVehical.TermlyRiskPremium = vehicleUpdate.TermlyRiskPremium == null ? 0 : vehicleUpdate.TermlyRiskPremium;
            EnderSomentVehical.QuaterlyRiskPremium = vehicleUpdate.QuaterlyRiskPremium == null ? 0 : vehicleUpdate.QuaterlyRiskPremium;
            EnderSomentVehical.Discount = vehicleUpdate.Discount;
            EnderSomentVehical.isLapsed = vehicleUpdate.isLapsed;
            EnderSomentVehical.BalanceAmount = model.BalanceAmount;
            EnderSomentVehical.VehicleLicenceFee = vehicleUpdate.VehicleLicenceFee;
            EnderSomentVehical.BusinessSourceId = vehicleUpdate.BusinessSourceId;
            EnderSomentVehical.IsCompleted = true;
           // EnderSomentVehical.CreatedOn = model.CreatedOn;
           // EnderSomentVehical.CreatedBy = model.CreatedBy;
            EnderSomentVehical.VehicleLicenceFee = model.VehicleLicenceFee;


            InsuranceContext.EndorsementVehicleDetails.Update(EnderSomentVehical);

            if(!string.IsNullOrEmpty(model.CertSerialNo))
            {
                SetCertificateDetailsInSession(EnderSomentVehical, model);
            }


            Session.Remove("EnViewlistVehicles");
            return RedirectToAction("EndorsementSummaryDetail", "Endorsement");

        }


        public void SetCertificateDetailsInSession(EndorsementVehicleDetail EnderSomentVehical, EndorsementRiskDetailModel model)
        {
           


           
            CertSerialNoDetail serialNumber = new CertSerialNoDetail()
            {
                PolicyId = EnderSomentVehical.PolicyId,
                VRN = EnderSomentVehical.RegistrationNo,
                CertSerialNo = model.CertSerialNo,
                PolicyType = Enum.GetName(typeof(PolicyType), PolicyType.Endorsement),
                EndorsmentVehicleId = EnderSomentVehical.Id,
                CreatedBy = Convert.ToInt32(EnderSomentVehical.CreatedBy),
                CreatedOn = DateTime.Now
            };

            Session["CertSerialNumDetail"] = serialNumber;


           // _riskDetailService.SaveCertSerialNoDetails(serialNumber);
        }


        public ActionResult EndorsementSummaryDetail(int? Id = 0)
        {
            ViewBag.SummaryDetailId = Id;
            var _model = new EndorsementSummaryDetailModel();
            var EnorsesummaryDetail = (EndorsementSummaryDetail)Session["ENViewSummaryDetail"];
            List<EndorsementVehicleDetail> _listriskdetailmodel = new List<EndorsementVehicleDetail>();
            List<EndorsementVehicleDetail> listriskdetailmodels = new List<EndorsementVehicleDetail>();
            List<EndorsementRiskDetailModel> RiskDetalModel = new List<EndorsementRiskDetailModel>();
            //var listVehicles = Session["ViewlistVehicles"];
            //var vehicle = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];// summary.GetVehicleInformation(id);
            bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;


            var endorsesummaryDetail = InsuranceContext.EndorsementSummaryDetails.All(where: $"Id={EnorsesummaryDetail.Id}").FirstOrDefault();
            var endorseSummaryVehicleDetails = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"EndorsementSummaryId={EnorsesummaryDetail.Id}").ToList();


            SummaryDetailService detailService = new SummaryDetailService();
            var currencyList = detailService.GetAllCurrency();


            if (endorseSummaryVehicleDetails != null)
            {
                foreach (var item in endorseSummaryVehicleDetails)
                {
                    var envehicle = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id={item.EndorsementVehicleId}");
                    var model = Mapper.Map<EndorsementVehicleDetail, EndorsementRiskDetailModel>(envehicle);
                    EndorsementRiskDetailModel obj = new EndorsementRiskDetailModel();


                    // var vehicleInsert = new EndorsementVehicleDetail();
                    obj.NoOfCarsCovered = envehicle.NoOfCarsCovered;
                    obj.PolicyId = envehicle.PolicyId;
                    obj.RegistrationNo = envehicle.RegistrationNo;
                    //Endor//
                    obj.PrimaryVehicleId = envehicle.PrimaryVehicleId;
                   // obj.EndorsementPolicyId = envehicle.Id; // 03 sep 2019
                    obj.EndorsementCustomerId = envehicle.EndorsementCustomerId;
                    obj.EndorsementPolicyId = envehicle.EndorsementPolicyId;  
                    //
                    obj.MakeId = envehicle.MakeId;
                    obj.ModelId = envehicle.ModelId;
                    obj.ModelId = envehicle.ModelId;
                    obj.CubicCapacity = envehicle.CubicCapacity;
                    obj.VehicleYear = envehicle.VehicleYear;
                    obj.EngineNumber = envehicle.EngineNumber;
                    obj.ChasisNumber = envehicle.ChasisNumber;
                    obj.VehicleColor = envehicle.VehicleColor;
                    obj.VehicleUsage = envehicle.VehicleUsage;
                    obj.CoverTypeId = envehicle.CoverTypeId;
                    obj.CoverTypeId = envehicle.CoverTypeId;
                    obj.CoverStartDate = envehicle.CoverStartDate;
                    obj.CoverEndDate = envehicle.CoverEndDate;
                    obj.SumInsured = envehicle.SumInsured;
                    obj.Premium = envehicle.Premium;
                    obj.AgentCommissionId = envehicle.AgentCommissionId;
                    obj.Rate = envehicle.Rate;
                    obj.StampDuty = envehicle.StampDuty;
                    obj.ZTSCLevy = envehicle.ZTSCLevy;
                    obj.RadioLicenseCost = envehicle.RadioLicenseCost;
                    obj.OptionalCovers = envehicle.OptionalCovers;
                    obj.VehicleLicenceFee = envehicle.VehicleLicenceFee==null? 0 : envehicle.VehicleLicenceFee.Value;
                    obj.Excess = envehicle.Excess;
                    obj.CoverNoteNo = envehicle.CoverNoteNo;
                    obj.ExcessType = envehicle.ExcessType;
                    obj.ExcessType = envehicle.ExcessType;
                    obj.CreatedOn = DateTime.Now;
                    obj.CreatedBy = envehicle.CreatedBy;


                    obj.IsActive = envehicle.IsActive;
                    obj.Addthirdparty = envehicle.Addthirdparty;
                    obj.Addthirdparty = envehicle.Addthirdparty;
                    obj.AddThirdPartyAmount = envehicle.AddThirdPartyAmount;

                    obj.PassengerAccidentCover = envehicle.PassengerAccidentCover;
                    obj.ExcessBuyBack = envehicle.ExcessBuyBack;
                    obj.RoadsideAssistance = envehicle.RoadsideAssistance;
                    obj.MedicalExpenses = envehicle.MedicalExpenses;

                    obj.PassengerAccidentCoverAmount = envehicle.PassengerAccidentCoverAmount == null ? 0 : envehicle.PassengerAccidentCoverAmount;
                    obj.ExcessBuyBackAmount = envehicle.ExcessBuyBackAmount == null ? 0 : envehicle.ExcessBuyBackAmount;

                    obj.NumberofPersons = envehicle.NumberofPersons;
                    obj.IsLicenseDiskNeeded = Convert.ToBoolean(envehicle.IsLicenseDiskNeeded);


                    obj.PassengerAccidentCoverAmountPerPerson = envehicle.PassengerAccidentCoverAmountPerPerson == null ? 0 : envehicle.PassengerAccidentCoverAmountPerPerson;
                    obj.ExcessBuyBackPercentage = envehicle.ExcessBuyBackPercentage == null ? 0 : envehicle.ExcessBuyBackPercentage;

                    obj.PaymentTermId = envehicle.PaymentTermId;
                    obj.ProductId = envehicle.ProductId;
                    obj.RoadsideAssistanceAmount = envehicle.RoadsideAssistanceAmount == null ? 0 : envehicle.RoadsideAssistanceAmount;
                    obj.MedicalExpensesAmount = envehicle.MedicalExpensesAmount == null ? 0 : envehicle.MedicalExpensesAmount;

                    obj.ExcessAmount = envehicle.ExcessAmount;

                    obj.TransactionDate = DateTime.Now;
                    obj.RenewalDate = Convert.ToDateTime(envehicle.RenewalDate);
                    obj.IncludeRadioLicenseCost = Convert.ToBoolean(envehicle.IncludeRadioLicenseCost);
                    obj.InsuranceId = envehicle.InsuranceId;
                    obj.InsuranceId = envehicle.InsuranceId;
                    obj.AnnualRiskPremium = envehicle.AnnualRiskPremium == null ? 0 : envehicle.AnnualRiskPremium;
                    obj.TermlyRiskPremium = envehicle.TermlyRiskPremium == null ? 0 : envehicle.TermlyRiskPremium;
                    obj.QuaterlyRiskPremium = envehicle.QuaterlyRiskPremium == null ? 0 : envehicle.QuaterlyRiskPremium;
                    obj.Discount = envehicle.Discount;

                    obj.isLapsed = envehicle.isLapsed;
                    obj.BalanceAmount = envehicle.BalanceAmount;
                    obj.VehicleLicenceFee = Convert.ToDecimal(envehicle.VehicleLicenceFee);
                    obj.BusinessSourceId = envehicle.BusinessSourceId;
                    obj.CurrencyId = envehicle.CurrencyId;

                    obj.RoadsideAssistancePercentage = envehicle.RoadsideAssistancePercentage == null ? 0 : envehicle.RoadsideAssistancePercentage;
                    obj.MedicalExpensesPercentage = envehicle.MedicalExpensesPercentage == null ? 0 : envehicle.MedicalExpensesPercentage;
                    obj.IsCompleted = envehicle.IsCompleted;

                    obj.Currency = detailService.GetCurrencyName(currencyList, envehicle.CurrencyId);
                    obj.TaxClassId = envehicle.TaxClassId;
                     
                    //InsuranceContext.EndorsementVehicleDetails.Insert(vehicleInsert);
                    //var vehicleId = vehicleInsert;
                    //Session["vehicleId"] = vehicleInsert;

                    obj.Id = envehicle.Id;
                    RiskDetalModel.Add(obj);
                    Session["EnViewlistVehicles"] = RiskDetalModel;
                }

            }

            var smrydetail = (List<EndorsementRiskDetailModel>)Session["EnViewlistVehicles"];

            EndorsementSummaryDetailService endorsementService = new EndorsementSummaryDetailService();

            if (smrydetail != null)
            {
                //var model = Mapper.Map<EndorsementSummaryDetail, EndorsementSummaryDetailModel>(smrydetail);
                _model.CarInsuredCount = smrydetail.Count;
                _model.DebitNote = "INV" + Convert.ToString(endorsementService.getNewDebitNote());
                _model.PaymentMethodId = 1;
                _model.PaymentTermId = 1;
                _model.ReceiptNumber = "";
                _model.SMSConfirmation = false;
                _model.AmountPaid = 0.00m;
                _model.TotalPremium = 0.00m;
                _model.TotalRadioLicenseCost = 0.00m;
                _model.Discount = 0.00m;
                _model.Id = EnorsesummaryDetail.Id;
                foreach (var item in smrydetail)
                {
                    _model.TotalPremium += item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee;
                    if (Convert.ToBoolean(item.IncludeRadioLicenseCost))
                    {
                        _model.TotalPremium += item.RadioLicenseCost;
                        _model.TotalRadioLicenseCost += item.RadioLicenseCost;
                    }
                }

                _model.AmountPaid = Convert.ToDecimal(_model.TotalPremium);
                _model.TotalStampDuty = smrydetail.Sum(item => item.StampDuty);
                _model.TotalSumInsured = smrydetail.Sum(item => item.SumInsured);
                _model.TotalZTSCLevies = smrydetail.Sum(item => item.ZTSCLevy);
                _model.ExcessBuyBackAmount = smrydetail.Sum(item => item.ExcessBuyBackAmount);
                _model.MedicalExpensesAmount = smrydetail.Sum(item => item.MedicalExpensesAmount);
                _model.PassengerAccidentCoverAmount = smrydetail.Sum(item => item.PassengerAccidentCoverAmount);
                _model.RoadsideAssistanceAmount = smrydetail.Sum(item => item.RoadsideAssistanceAmount);
                _model.ExcessAmount = smrydetail.Sum(item => item.ExcessAmount);
                _model.Discount = smrydetail.Sum(item => item.Discount);
                _model.isQuotation = false;
                _model.IsCompleted = false;
                _model.CreatedOn = DateTime.Now;
                if (_userLoggedin)
                {
                    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                    var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();
                    _model.CreatedBy = _customerData.Id;
                }
                _model.PrimarySummaryId = EnorsesummaryDetail.PrimarySummaryId;
                _model.EndorsementCustomerId = EnorsesummaryDetail.EndorsementCustomerId;
                _model.EndorsementPolicyId = EnorsesummaryDetail.EndorsementPolicyId;
                _model.CustomerId = EnorsesummaryDetail.CustomerId;
                if (Session["PolicyDataView"] != null)
                {
                    var PolicyData = (EndorsementPolicyDetail)Session["PolicyDataView"];
                    _model.InvoiceNumber = PolicyData.PolicyNumber;
                }

                decimal paybale = Convert.ToDecimal(endorsesummaryDetail.TotalPremium - _model.TotalPremium);
                
                if(paybale!=0)
                    _model.PayableAmount = paybale;


            }

            return View(_model);
        }
        public ActionResult SaveEndorsementSummaryDetails(EndorsementSummaryDetailModel model)
        {
            var _Endorsepolicy = (EndorsementPolicyDetail)Session["PolicyDataView"];
            var Endorcustomer = (EndorsementCustomer)Session["EnCustomerDetail"];
            //var enduser = Session["Enuser"];
            var ensummerydetail = InsuranceContext.EndorsementSummaryDetails.Single(where: $"Id = '{model.Id}'");
            if (ensummerydetail != null)
            {
                Session.Remove("EnsummaryId");
                var EndorseSummery = Mapper.Map<EndorsementSummaryDetailModel, EndorsementSummaryDetail>(model);
                //EndorsementSummaryDetailModel endorsemodel = new EndorsementSummaryDetailModel();
                ensummerydetail.SummaryId = model.SummaryId;
                ensummerydetail.VehicleDetailId = model.VehicleDetailId;
                ensummerydetail.CustomerId = EndorseSummery.CustomerId;
                ensummerydetail.PaymentTermId = EndorseSummery.PaymentTermId;
                ensummerydetail.PaymentMethodId = EndorseSummery.PaymentMethodId;
                ensummerydetail.TotalSumInsured = EndorseSummery.TotalSumInsured;
                ensummerydetail.TotalPremium = EndorseSummery.TotalPremium;
                ensummerydetail.TotalStampDuty = EndorseSummery.TotalStampDuty;
                ensummerydetail.TotalZTSCLevies = EndorseSummery.TotalZTSCLevies;
                ensummerydetail.TotalRadioLicenseCost = EndorseSummery.TotalRadioLicenseCost;
                ensummerydetail.DebitNote = EndorseSummery.DebitNote;
                ensummerydetail.ReceiptNumber = EndorseSummery.ReceiptNumber;
                ensummerydetail.SMSConfirmation = Convert.ToBoolean(EndorseSummery.SMSConfirmation);
                ensummerydetail.CreatedBy = ensummerydetail.CreatedBy;
                ensummerydetail.CreatedOn = ensummerydetail.CreatedOn;
                // ensummerydetail.ModifiedBy = ensummerydetail.ModifiedBy;
                ensummerydetail.ModifiedOn = DateTime.Now;
                ensummerydetail.IsActive = ensummerydetail.IsActive;
                ensummerydetail.AmountPaid = model.AmountPaid;
                ensummerydetail.BalancePaidDate = DateTime.Now;
                ensummerydetail.Notes = model.Notes;
                ensummerydetail.isQuotation = model.isQuotation;
                ensummerydetail.IsCompleted = false;

                ensummerydetail.PrimarySummaryId = model.PrimarySummaryId;
                ensummerydetail.EndorsementCustomerId = model.EndorsementCustomerId;

                ensummerydetail.EndorsementPolicyId = model.EndorsementPolicyId;
                ensummerydetail.EndorsementVehicleId = model.EndorsementVehicleId;

                ensummerydetail.PayableAmount = model.PayableAmount; // for pro-amount


                InsuranceContext.EndorsementSummaryDetails.Update(ensummerydetail);

                Session["EnsummaryId"] = ensummerydetail;
                if (model.PaymentMethodId == 1)
                {
                    return RedirectToAction("SaveEndorsementDetailList", "Endorsement", new { id = model.Id, invoiceNumer = model.InvoiceNumber });
                }
                else if (model.PaymentMethodId == 2)
                {
                    return RedirectToAction("EndorsementPaymentDetail", new { id = model.Id });
                }
                else if (model.PaymentMethodId == 3)
                {
                    TempData["PaymentMethodId"] = model.PaymentMethodId;
                    return RedirectToAction("makepayment", "Endorsement", new { id = model.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid) });
                    //return RedirectToAction("InitiatePaynowTransaction", "Endorsement", new { id = model.Id, TotalPremiumPaid = Convert.ToString(model.AmountPaid), PolicyNumber = _Endorsepolicy.PolicyNumber, Email = enduser });
                }
            }

            return RedirectToAction("MyPolicies", "Account");
        }

        public ActionResult EndorsementPaymentDetail(int id, string erroMsg = null)
        {

            var cardDetails = (CardDetailModel)Session["EndorsementCardDetail"];
            if (cardDetails == null)
            {
                cardDetails = new CardDetailModel();
            }
            cardDetails.EndorsementSummaryId = id;

            TempData["ErrorMsg"] = erroMsg;
            return View(cardDetails);
        }

        public ActionResult PaymentWithCreditCard(CardDetailModel model)
        {
            if (!isValid(model.ExpiryDate))
            {
                ModelState.AddModelError("PaymentError", "Card expire date is not valid");
                return RedirectToAction("EndorsementPaymentDetail", "Endorsement", new { id = model.EndorsementSummaryId, erroMsg = "Card expire date is not valid." });
            }


            Session["EndorsementCardDetail"] = model;

            //create and item for which you are taking payment
            //if you need to add more items in the list
            //Then you will need to create multiple item objects or use some loop to instantiate object
            var EndoeSummaryId = InsuranceContext.EndorsementSummaryDetails.Single(model.EndorsementSummaryId);

            var EndorseSummaryVehicleDetails = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"EndorsementSummaryId={model.EndorsementSummaryId}").ToList();
            var Endodevehicle = InsuranceContext.EndorsementVehicleDetails.Single(EndorseSummaryVehicleDetails[0].EndorsementVehicleId);
            var _policy = InsuranceContext.PolicyDetails.Single(Endodevehicle.PolicyId);
            var _Endorecustomer = InsuranceContext.EndorsementCustomers.Single(EndoeSummaryId.EndorsementCustomerId);

            var product = InsuranceContext.Products.Single(Convert.ToInt32(Endodevehicle.ProductId));
            var currency = InsuranceContext.Currencies.Single(_policy.CurrencyId);

            double totalPremium = Convert.ToDouble(EndoeSummaryId.TotalPremium);
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
            foreach (var vehicledetail in EndorseSummaryVehicleDetails.ToList())
            {
                var _vehicle = InsuranceContext.EndorsementVehicleDetails.Single(vehicledetail.EndorsementVehicleId);
                //Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
                VehicleModel _model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_vehicle.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{_vehicle.MakeId}'");

                Item item = new Item();
                item.name = make.MakeDescription + "/" + _model.ModelDescription;
                item.currency = "USD";
                item.price = Convert.ToString((_vehicle.Premium + _vehicle.StampDuty + _vehicle.ZTSCLevy + _vehicle.VehicleLicenceFee + (Convert.ToBoolean(_vehicle.IncludeRadioLicenseCost) ? _vehicle.RadioLicenseCost : 0.00m)) - _vehicle.BalanceAmount);
                item.quantity = "1";
                item.sku = _vehicle.RegistrationNo;

                itms.Add(item);
            }
            Session["EndoesementitemData"] = itms;
            ItemList itemLists = new ItemList();
            itemLists.items = itms;


            Address billingAddress = new Address();
            billingAddress.city = _Endorecustomer.City;
            billingAddress.country_code = "US";
            billingAddress.line1 = _Endorecustomer.AddressLine1 == string.Empty ? _Endorecustomer.AddressLine2 : _Endorecustomer.AddressLine1;
            billingAddress.line2 = _Endorecustomer.AddressLine2 == string.Empty ? _Endorecustomer.AddressLine1 : _Endorecustomer.AddressLine2;
            if (_Endorecustomer.ZipCode == null)
            {
                billingAddress.postal_code = "00263";
            }
            else
            {
                billingAddress.postal_code = _Endorecustomer.ZipCode;
            }

            billingAddress.state = _Endorecustomer.NationalIdentificationNumber;


            PayPal.Api.CreditCard crdtCard = new PayPal.Api.CreditCard();
            crdtCard.billing_address = billingAddress;
            crdtCard.cvv2 = model.CVC;
            crdtCard.expire_month = Convert.ToInt32(model.ExpiryDate.Split('/')[0]);
            crdtCard.expire_year = Convert.ToInt32(model.ExpiryDate.Split('/')[1]);

            var _name = model.NameOnCard.Split(' ');

            if (_name.Length == 1)
            {
                crdtCard.first_name = _name[0];
                crdtCard.last_name = null;
            }
            if (_name.Length == 2)
            {
                crdtCard.first_name = _name[0];
                crdtCard.last_name = _name[1];
            }
            crdtCard.number = model.CardNumber; //use some other test number if it fails
            crdtCard.type = CreditCardUtility.GetTypeName(model.CardNumber).ToLower();

            Details details = new Details();
            details.tax = "0";
            details.shipping = "0";
            details.subtotal = (EndoeSummaryId.AmountPaid.ToString().IndexOf('.') > -1 ? EndoeSummaryId.AmountPaid.ToString() : EndoeSummaryId.AmountPaid.ToString() + zeros);


            Amount amont = new Amount();
            amont.currency = "USD";
            amont.total = (EndoeSummaryId.AmountPaid.ToString().IndexOf('.') > -1 ? EndoeSummaryId.AmountPaid.ToString() : EndoeSummaryId.AmountPaid.ToString() + zeros);
            amont.details = details;


            Transaction tran = new Transaction();
            tran.amount = amont;
            tran.description = "trnx desc";
            tran.item_list = itemLists;

            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(tran);

            FundingInstrument fundInstrument = new FundingInstrument();
            fundInstrument.credit_card = crdtCard;


            List<FundingInstrument> fundingInstrumentList = new List<FundingInstrument>();
            fundingInstrumentList.Add(fundInstrument);

            var User = UserManager.FindById(_Endorecustomer.UserID);
            PayerInfo pi = new PayerInfo();
            pi.email = User.Email;
            pi.first_name = _Endorecustomer.FirstName;
            pi.last_name = _Endorecustomer.LastName;
            pi.shipping_address = new ShippingAddress
            {
                city = _Endorecustomer.City,
                country_code = "US",
                line1 = _Endorecustomer.AddressLine1 == string.Empty ? _Endorecustomer.AddressLine2 : _Endorecustomer.AddressLine1,
                line2 = _Endorecustomer.AddressLine2 == string.Empty ? _Endorecustomer.AddressLine1 : _Endorecustomer.AddressLine2,
                postal_code = _Endorecustomer.ZipCode,
                state = _Endorecustomer.NationalIdentificationNumber,
            };


            Payer payr = new Payer();
            payr.funding_instruments = fundingInstrumentList;
            payr.payment_method = "credit_card";
            payr.payer_info = pi;

            Payment pymnt = new Payment();
            pymnt.intent = "sale";
            pymnt.payer = payr;
            pymnt.transactions = transactions;


            try
            {

                //getting context from the paypal, basically we are sending the clientID and clientSecret key in this function 
                //to the get the context from the paypal API to make the payment for which we have created the object above.

                //Code for the configuration class is provided next

                // Basically, apiContext has a accesstoken which is sent by the paypal to authenticate the payment to facilitator account. An access token could be an alphanumeric string

                APIContext apiContext = Configuration.GetAPIContext();

                // Create is a Payment class function which actually sends the payment details to the paypal API for the payment. The function is passed with the ApiContext which we received above.

                Payment createdPayment = pymnt.Create(apiContext);

                // condition for failure

                if (createdPayment != null && string.IsNullOrEmpty(createdPayment.id))
                {
                    ModelState.AddModelError("PaymentError", "Payment not approved");
                    return RedirectToAction("EndorsementPaymentDetail", "Endorsement", new { id = model.EndorsementSummaryId });
                }

                decimal amount = 0;





                if (createdPayment != null && createdPayment.transactions.Count() > 0)
                {
                    var trasactinList = createdPayment.transactions;

                    foreach (var item in trasactinList)
                    {
                        if (item.amount != null)
                        {
                            amount = Convert.ToDecimal(item.amount.total);
                            break;
                        }

                    }

                    if (amount < 1)
                    {
                        ModelState.AddModelError("PaymentError", "Payment not approved");
                        return RedirectToAction("EndorsementPaymentDetail", "Endorsement", new { id = model.EndorsementSummaryId, erroMsg = "Payment not approved" });
                    }
                }
                Session["EndorsementPaymentId"] = createdPayment.id;

                //if the createdPayment.State is "approved" it means the payment was successfull else not

                creatInvoice(User, _Endorecustomer);

                // ApproveVRNToIceCash(model.SummaryDetailId);


                if (createdPayment.state.ToLower() != "approved")
                {
                    ModelState.AddModelError("PaymentError", "Payment not approved");
                    return RedirectToAction("EndorsementPaymentDetail", "Endorsement", new { id = model.EndorsementSummaryId, erroMsg = "Payment not approved" });
                }
            }
            catch (PayPal.PayPalException ex)
            {

                Logger.Log("Error: " + ex.Message);
                ModelState.AddModelError("PaymentError", ex.Message);
                TempData["ErrorMessage"] = ex.Message;
                JavaScriptSerializer json_serializer = new JavaScriptSerializer();

                var error = json_serializer.DeserializeObject(((PayPal.ConnectionException)ex).Response);
                return RedirectToAction("EndorsementPaymentDetail", "Endorsement", new { id = model.EndorsementSummaryId, erroMsg = "Payment not approved" });
            }
            return RedirectToAction("SaveEndorsementDetailList", "Endorsement", new { id = model.EndorsementSummaryId });

        }



        //public async Task<ActionResult> InitiatePaynowTransaction(Int32 id, string TotalPremiumPaid, string PolicyNumber, string Email)
        //{
        //    var summaryDetail = InsuranceContext.SummaryDetails.Single(id);
        //    var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={id}").ToList();
        //    //var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
        //    var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
        //    var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
        //    var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));

        //    List<Item> itms = new List<Item>();




        //    foreach (var vehicledetail in SummaryVehicleDetails.ToList())
        //    {
        //        var _vehicle = InsuranceContext.VehicleDetails.Single(vehicledetail.VehicleDetailsId);
        //        Insurance.Service.VehicleService obj = new Insurance.Service.VehicleService();
        //        VehicleModel _model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{_vehicle.ModelId}'");
        //        VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{_vehicle.MakeId}'");

        //        Item item = new Item();
        //        item.name = make.MakeDescription + "/" + _model.ModelDescription;
        //        item.currency = "USD";
        //        item.price = Convert.ToString(_vehicle.Premium);
        //        item.quantity = "1";
        //        item.sku = _vehicle.RegistrationNo;

        //        itms.Add(item);
        //    }

        //    //Item item = new Item();
        //    //item.name = product.ProductName;
        //    //item.currency = "USD";
        //    //item.price = vehicle.Premium.ToString();
        //    //item.quantity = "1";
        //    //item.sku = "sku";
        //    //item.currency = "USD";



        //    Session["itemData"] = itms;

        //    Insurance.Service.PaynowService paynowservice = new Insurance.Service.PaynowService();
        //    PaynowResponse paynowresponse = new PaynowResponse();

        //    paynowresponse = await paynowservice.initiateTransaction(Convert.ToString(id), TotalPremiumPaid, PolicyNumber, Email);

        //    if (paynowresponse.status == "Ok")
        //    {
        //        string strScript = "location.href = '" + paynowresponse.browserurl + "';";
        //        ViewBag.strScript = "<script type='text/javascript'>$(document).ready(function(){" + strScript + "});</script>";
        //    }
        //    else
        //    {
        //        ViewBag.strScript = "<script type='text/javascript'>$(document).ready(function(){$('#errormsg').text('" + paynowresponse.error + "');});</script>";
        //    }



        //    return View();
        //}




        public ActionResult makepayment(Int32 id, decimal TotalPremiumPaid)
        {
            Dictionary<string, dynamic> responseData;
            string data = "authentication.userId=8a8294175698883c01569ce4c4212119" +
                "&authentication.password=Mc2NMzf8jM" +
                "&authentication.entityId=8a8294175698883c01569ce4c3972115" +
                "&amount=" + TotalPremiumPaid + "" +
                "&currency=USD" +
                "&paymentType=DB";
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
                ViewBag.checkoutIds = Convert.ToString(responseData["id"]);

                TempData["ID"] = id;
                return View();
            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        public ActionResult Endorsementreturnurl()
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
                int EndorsementSummaryid = Convert.ToInt32(TempData["ID"]);
                string PaymentId = Convert.ToString(TempData["PaymentMethodId"]);
                //var result = new PaypalController().SaveDetailList(Summaryid, InvoiceId);
                return RedirectToAction("SaveEndorsementDetailList", "Endorsement", new { id = EndorsementSummaryid, invoiceNumer = InvoiceId, PaymentId = PaymentId });
            }
            else
            {
                return RedirectToAction("PaymentFailure");
            }

        }
        public async Task<ActionResult> SaveEndorsementDetailList(Int32 id, string invoiceNumer, string Paymentid = "")
        {
            string PaymentMethod = "";
            if (Paymentid == "1")
            {
                PaymentMethod = "CASH";
            }
            else if (Paymentid == "2")
            {
                PaymentMethod = "MasterCard";
            }
            else if (Paymentid == "3")
            {
                PaymentMethod = "paynow";
            }
            else if (Paymentid == "")
            {
                PaymentMethod = "CASH";
            }

            var endorsementsummay = InsuranceContext.EndorsementSummaryDetails.Single(id);
            EndorsementSummaryDetail objEndorsementsummary = new EndorsementSummaryDetail();
            if (endorsementsummay != null && endorsementsummay.isQuotation)
            {
                endorsementsummay.isQuotation = false;
            }
            var EndorsementSummaryVehicleDetails = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"EndorsementSummaryId='{id}'").ToList();
            var endorsevehicle = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id='{EndorsementSummaryVehicleDetails[0].EndorsementVehicleId}'");
            var endorsepolicy = InsuranceContext.EndorsementPolicyDetails.Single(endorsevehicle.EndorsementPolicyId);
            var endorsementCustomer = InsuranceContext.EndorsementCustomers.Single(endorsementsummay.EndorsementCustomerId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(endorsevehicle.ProductId));
            SummaryDetailService detailDervice = new SummaryDetailService();
            var currencyList = detailDervice.GetAllCurrency();
            var currency = detailDervice.GetCurrencyName(currencyList, endorsevehicle.CurrencyId);

            // var currency = InsuranceContext.Currencies.Single(endorsepolicy.CurrencyId);
            var paymentInformations = InsuranceContext.PaymentInformations.SingleCustome(id);

            var user = UserManager.FindById(endorsementCustomer.UserID);
            var DebitNote = endorsementsummay.DebitNote;
            EndorsementPaymentInformation objSaveDetailListModel = new EndorsementPaymentInformation();
          //  objSaveDetailListModel.CurrencyId = EndorsementSummaryVehicleDetails.;
            objSaveDetailListModel.PrimaryPolicyId = endorsepolicy.PrimaryPolicyId;
            objSaveDetailListModel.PrimaryCustomerId = endorsementsummay.CustomerId.Value;
            objSaveDetailListModel.PrimaryVehicleDetailId = endorsevehicle.PrimaryVehicleId;
            objSaveDetailListModel.PrimarySummaryDetailId = endorsementsummay.PrimarySummaryId;

            objSaveDetailListModel.EndorsementSummaryId = id;
            objSaveDetailListModel.EndorsementPolicyId = endorsevehicle.EndorsementPolicyId;
            objSaveDetailListModel.EndorsementCustomerId = endorsementsummay.EndorsementCustomerId.Value;
            objSaveDetailListModel.EndorsementVehicleId = endorsevehicle.Id;


            objSaveDetailListModel.DebitNote = endorsementsummay.DebitNote;
            objSaveDetailListModel.ProductId = product.Id;

            objSaveDetailListModel.PaymentId = PaymentMethod;
            objSaveDetailListModel.InvoiceId = invoiceNumer;
            objSaveDetailListModel.CreatedBy = endorsementCustomer.Id;
            objSaveDetailListModel.CreatedOn = DateTime.Now;
            objSaveDetailListModel.InvoiceNumber = endorsepolicy.PolicyNumber;
            List<EndorsementVehicleDetail> ListOfVehicles = new List<EndorsementVehicleDetail>();



            string filepath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];
            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();

            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            var dbEndorsemenetPaymentInformation = InsuranceContext.EndorsementPaymentInformations.Single(where: $"EndorsementSummaryId='{id}'");
            if (dbEndorsemenetPaymentInformation == null)
            {
                InsuranceContext.EndorsementPaymentInformations.Insert(objSaveDetailListModel);
            }
            else
            {
                objSaveDetailListModel.Id = dbEndorsemenetPaymentInformation.Id;
                InsuranceContext.EndorsementPaymentInformations.Update(objSaveDetailListModel);
            }
            endorsementsummay.IsCompleted = true;
            InsuranceContext.EndorsementSummaryDetails.Update(endorsementsummay);

            //Update New Email
            // var data =  Session["Enuser"];

            //ApproveVRNToIceCash(id);
            string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/EndorsementUserPayment.cshtml";
            string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));
            var Body2 = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString()).Replace("#PayableAmount#", endorsementsummay.PayableAmount.ToString()).Replace("##path##", filepath).Replace("#FirstName#", endorsementCustomer.FirstName).Replace("#Currency#", currency).Replace("#LastName#", endorsementCustomer.LastName).Replace("#AccountName#", endorsementCustomer.FirstName + ", " + endorsementCustomer.LastName).Replace("#Address1#", endorsementCustomer.AddressLine1).Replace("#Address2#", endorsementCustomer.AddressLine2).Replace("#Amount#", Convert.ToString(endorsementsummay.AmountPaid)).Replace("#PaymentDetails#", "Endorsement Premium").Replace("#ReceiptNumber#", endorsepolicy.PolicyNumber).Replace("#PaymentType#", (endorsementsummay.PaymentMethodId == 1 ? "Cash" : (endorsementsummay.PaymentMethodId == 2 ? "PayPal" : "PayNow")));

          //  var attachementFile = MiscellaneousService.EmailPdf(Body2, Convert.ToInt32(endorsepolicy.EndorsementCustomerId), endorsepolicy.PolicyNumber, "Reciept Payment");

            var attachementFile = MiscellaneousService.EmailPdf(Body2, Convert.ToInt32(endorsepolicy.EndorsementCustomerId), endorsepolicy.PolicyNumber, "EndorsementInvoice");


            List<string> attachements = new List<string>();
            attachements.Add(attachementFile);


            if (endorsementCustomer.IsCustomEmail) // if customer has custom email
            {
                objEmailService.SendEmail(LoggedUserEmail(), "", "", "EndorsementInvoice", Body2, attachements);
            }
            else
            {
                objEmailService.SendEmail(user.Email, "", "", "EndorsementInvoice", Body2, attachements);
            }

            foreach (var item in EndorsementSummaryVehicleDetails)
            {
                var itemVehicle = InsuranceContext.EndorsementVehicleDetails.Single(item.EndorsementVehicleId);
                ListOfVehicles.Add(itemVehicle);
            }


            string Summeryofcover = "";
            var RoadsideAssistanceAmount = 0.00m;
            var MedicalExpensesAmount = 0.00m;
            var ExcessBuyBackAmount = 0.00m;
            var PassengerAccidentCoverAmount = 0.00m;
            var ExcessAmount = 0.00m;
            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm)) select new { ID = (int)e, Name = e.ToString() };

            foreach (var item in ListOfVehicles)
            {
                VehicleModel model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'");
                VehicleMake make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'");
                string vehicledescription = model.ModelDescription + " / " + make.MakeDescription;

                RoadsideAssistanceAmount = RoadsideAssistanceAmount + Convert.ToDecimal(item.RoadsideAssistanceAmount);
                MedicalExpensesAmount = MedicalExpensesAmount + Convert.ToDecimal(item.MedicalExpensesAmount);
                ExcessBuyBackAmount = ExcessBuyBackAmount + Convert.ToDecimal(item.ExcessBuyBackAmount);
                PassengerAccidentCoverAmount = PassengerAccidentCoverAmount + Convert.ToDecimal(item.PassengerAccidentCoverAmount);
                ExcessAmount = ExcessAmount + Convert.ToDecimal(item.ExcessAmount);

                var paymentTermVehicel = ePaymentTermData.FirstOrDefault(p => p.ID == item.PaymentTermId);

                decimal totalpaymentdue = 0.00m;
                string paymentTermsName = "";
                if (item.PaymentTermId == 1)
                    paymentTermsName = "Annual";
                else if (item.PaymentTermId == 4)
                    paymentTermsName = "Termly";
                else
                    paymentTermsName = paymentTermVehicel.Name + " Months";


                string policyPeriod = item.CoverStartDate.Value.ToString("dd/MM/yyyy") + " - " + item.CoverEndDate.Value.ToString("dd/MM/yyyy");
                Summeryofcover += "<tr><td style='padding: 7px 10px; font - size:15px;'>" + item.RegistrationNo + " </td> <td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + vehicledescription + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + currency + item.SumInsured + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + (item.CoverTypeId == 4 ? eCoverType.Comprehensive.ToString() : eCoverType.ThirdParty.ToString()) + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + InsuranceContext.VehicleUsages.All(Convert.ToString(item.VehicleUsage)).Select(x => x.VehUsage).FirstOrDefault() + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + policyPeriod + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + paymentTermsName + "</font></td><td style='padding: 7px 10px; font - size:15px;'><font size='2'>" + currency + Convert.ToString(item.Premium) + "</font></td></tr>";

            }
            var paymentTerm = ePaymentTermData.FirstOrDefault(p => p.ID == endorsevehicle.PaymentTermId);
            string SeheduleMotorPath = "/Views/Shared/EmaiTemplates/EndorsementSehduleMotor.cshtml";
            string MotorBody = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(SeheduleMotorPath));


            var Bodyy = MotorBody.Replace("##PolicyNo##", endorsepolicy.PolicyNumber).Replace("##paht##", filepath).Replace("##Cellnumber##", user.PhoneNumber).Replace("##FirstName##", endorsementCustomer.FirstName).Replace("##LastName##", endorsementCustomer.LastName).Replace("##Email##", user.Email).Replace("##BirthDate##", endorsementCustomer.DateOfBirth.Value.ToString("dd/MM/yyyy")).Replace("##Address1##", endorsementCustomer.AddressLine1).Replace("##Address2##", endorsementCustomer.AddressLine2).Replace("##Renewal##", endorsevehicle.RenewalDate.Value.ToString("dd/MM/yyyy")).Replace("##InceptionDate##", endorsevehicle.CoverStartDate.Value.ToString("dd/MM/yyyy")).Replace("##package##", paymentTerm.Name).Replace("##Summeryofcover##", Summeryofcover).Replace("##PaymentTerm##", (endorsevehicle.PaymentTermId == 1 ? paymentTerm.Name + "(1 Year)" : paymentTerm.Name + "(" + endorsevehicle.PaymentTermId.ToString() + "Months)")).Replace("##TotalPremiumDue##", Convert.ToString(endorsementsummay.TotalPremium)).Replace("##PayableAmount##", Convert.ToString(endorsementsummay.PayableAmount)).Replace("##Currency##",currency).Replace("##StampDuty##", Convert.ToString(endorsementsummay.TotalStampDuty)).Replace("##MotorLevy##", Convert.ToString(endorsementsummay.TotalZTSCLevies)).Replace("##PremiumDue##", Convert.ToString(endorsementsummay.TotalPremium - endorsementsummay.TotalStampDuty - endorsementsummay.TotalZTSCLevies - endorsementsummay.TotalRadioLicenseCost - ListOfVehicles.Sum(x => x.VehicleLicenceFee) + ListOfVehicles.Sum(x => x.Discount))).Replace("##PostalAddress##", endorsementCustomer.ZipCode).Replace("##ExcessBuyBackAmount##", Convert.ToString(ExcessBuyBackAmount)).Replace("##MedicalExpenses##", Convert.ToString(MedicalExpensesAmount)).Replace("##PassengerAccidentCover##", Convert.ToString(PassengerAccidentCoverAmount)).Replace("##RoadsideAssistance##", Convert.ToString(RoadsideAssistanceAmount)).Replace("##RadioLicence##", Convert.ToString(endorsementsummay.TotalRadioLicenseCost)).Replace("##Discount##", Convert.ToString(ListOfVehicles.Sum(x => x.Discount))).Replace("##ExcessAmount##", Convert.ToString(ExcessAmount)).Replace("##NINumber##", endorsementCustomer.NationalIdentificationNumber).Replace("##VehicleLicenceFee##", Convert.ToString(ListOfVehicles.Sum(x => x.VehicleLicenceFee)));

            var attacehmetnFile = MiscellaneousService.EmailPdf(Bodyy, Convert.ToInt32(endorsepolicy.EndorsementCustomerId), endorsepolicy.PolicyNumber, "Endorsement_Schedule-motor");
            var Atter = "~/Pdf/14809 Gene Insure Motor Policy Book.pdf";
            List<string> __attachements = new List<string>();
            __attachements.Add(attacehmetnFile);
            __attachements.Add(Atter);
            #region

            if (endorsementCustomer.IsCustomEmail) // if customer has custom email
            {
                objEmailService.SendEmail(LoggedUserEmail(), "", "", "Endorsement Schedule-motor", Bodyy, __attachements);
            }
            else
            {
                objEmailService.SendEmail(user.Email, "", "", "Endorsement Schedule-motor", Bodyy, __attachements);
            }


            #endregion

            #region  Remove All Sessions


            try
            {
                Session.Remove("PolicyDataView");
                Session.Remove("EnViewlistVehicles");
                Session.Remove("ENViewSummaryDetail");
                Session.Remove("EndoesementitemData");
                Session.Remove("EndorsementPaymentId");
                Session.Remove("EndoesementitemData");
                Session.Remove("EndorsementCardDetail");
                Session.Remove("EnsummaryId");
                Session.Remove("EnCustomerDetail");
                Session.Remove("Enuser");

            }
            catch (Exception ex)
            {

                Session.Remove("EnCustomerDetail");
                Session.Remove("EnsummaryId");
                Session.Remove("EndorsementCardDetail");
                Session.Remove("EndoesementitemData");
                Session.Remove("EndorsementPaymentId");
                Session.Remove("EndoesementitemData");
                Session.Remove("ENViewSummaryDetail");
                Session.Remove("EnViewlistVehicles");
                Session.Remove("PolicyDataView");
                Session.Remove("Enuser");
            }

            #endregion


            SaveCertSerialNoDetails();


            return RedirectToAction("MyPolicies", "Account");
            //return RedirectToAction("EndorsementThankYou");
        }

        private void SaveCertSerialNoDetails()
        {
            if(Session["CertSerialNumDetail"]!=null)
            {
                var serialNumberDetail = (CertSerialNoDetail)Session["CertSerialNumDetail"];
                RiskDetailService _riskDetailService = new RiskDetailService();
                _riskDetailService.SaveCertSerialNoDetails(serialNumberDetail);
            }     
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


        private bool isValid(string dateString)
        {

            DateTime dt1 = Convert.ToDateTime(dateString);
            DateTime dt2 = DateTime.Now.Date;
            int result = DateTime.Compare(dt1, dt2);


            if (result < 0)
                return false;
            else
                return true;

        }

        private ActionResult creatInvoice(ApplicationUser User, EndorsementCustomer _Endorecustomer)
        {

            APIContext apiContext = Configuration.GetAPIContext();

            var data = (List<Item>)Session["EndoesementitemData"];

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
                        line1 = _Endorecustomer.AddressLine1,
                        city = _Endorecustomer.AddressLine2,
                        state = _Endorecustomer.City + "/ " + _Endorecustomer.NationalIdentificationNumber,
                        postal_code = _Endorecustomer.ZipCode,
                        country_code = "US"

                    }
                },
                billing_info = new List<BillingInfo>()
                            {
                                new BillingInfo()
                                {

                                    email = User.Email,//"amit.kamal@kindlebit.com",
                                    first_name=_Endorecustomer.FirstName,
                                    last_name=_Endorecustomer.LastName
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
                    first_name = _Endorecustomer.FirstName,
                    last_name = _Endorecustomer.LastName,
                    business_name = "InsuranceClaim",
                    address = new InvoiceAddress()
                    {
                        //line1 = userdata.State.ToString(),
                        city = _Endorecustomer.City,
                        state = _Endorecustomer.City + "/" + _Endorecustomer.NationalIdentificationNumber,
                        postal_code = _Endorecustomer.ZipCode,
                        country_code = "US"
                    }
                }
            };
            var createdInvoice = invoice.Create(apiContext);
            Session["EndorsementInvoiceId"] = createdInvoice.id;
            createdInvoice.Send(apiContext);

            return null;
        }
        public ActionResult EndorsementThankYou()
        {
            return View();
        }


        //get SummeryList 
        public ActionResult GetEndorsementSummeryList()
        {
            return View();
        }


       


        //[HttpPost]
        //public JsonResult DeleteVehicle(int? index)
        //{

        //    try
        //    {
        //        if (Session["VehicleDetails"] != null)
        //        {
        //            var list = (List<RiskDetailModel>)Session["VehicleDetails"];

        //            list.RemoveAt(Convert.ToInt32(index) - 1);

        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }
        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception)
        //    {

        //        return Json(false, JsonRequestBehavior.AllowGet);
        //    }


        //}
        //Get Customer Id And Detail
        public string GetCustomerEmailbyCustomerID(int? customerId)
        {
            var customerDetial = InsuranceContext.Customers.Single(customerId);

            string email = "";

            if (customerDetial != null)
            {
                var user = UserManager.FindById(customerDetial.UserID);

                email = user.Email;
            }
            return email;

        }

        public ActionResult EndorsementDetail(string Policynumber)
        {
            ListEndorsementPolicy Endorpolicylist = new ListEndorsementPolicy();
            Endorpolicylist.listendorsementpolicy = new List<EndorsementPolicyListViewModel>();
            var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
            var role = UserManager.GetRoles(_User.Id.ToString()).FirstOrDefault();
            var custome = InsuranceContext.Customers.Single(where: $"UserId='{User.Identity.GetUserId().ToString()}'").Id;
            var EndorsementcustomerID = InsuranceContext.EndorsementCustomers.Single(where: $"PrimeryCustomerId='{custome}'");
            var policyinfo = InsuranceContext.EndorsementPolicyDetails.All(where: $"PolicyNumber = '{Policynumber}'");


            SummaryDetailService detialService = new SummaryDetailService();
            var currencyList = detialService.GetAllCurrency();

          

           

            var endorsementsummary = new List<EndorsementSummaryDetail>();

            foreach (var item in policyinfo)
            {
                if (role == "Staff" || role == "Administrator")
                {
                    var endorsementssummryinfo = InsuranceContext.EndorsementSummaryDetails.Single(where: $"CreatedBy ='{custome}'and IsCompleted = 'true'and EndorsementPolicyId = '{item.Id}'");
                    if (endorsementssummryinfo != null)
                    {
                        endorsementsummary.Add(endorsementssummryinfo);
                    }
                }
                else if (role == "Renewals")
                {
                    var endorsementssummryinfo = InsuranceContext.EndorsementSummaryDetails.Single(where: $"CreatedBy ='{custome}'and IsCompleted = 'true'and EndorsementPolicyId = '{item.Id}'");
                    if (endorsementssummryinfo != null)
                    {
                        endorsementsummary.Add(endorsementssummryinfo);
                    }
                }
            }

            foreach (var item in endorsementsummary)
            {
                var Endorsementsummaryvehicledetail = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"EndorsementSummaryId = '{item.Id}'").ToList();
                var Endorsementvehicle = InsuranceContext.EndorsementVehicleDetails.Single(Endorsementsummaryvehicledetail[0].EndorsementVehicleId);
                EndorsementPolicyListViewModel ListEndorsmentDetail = new EndorsementPolicyListViewModel();

                ListEndorsmentDetail.TotalPremium = Convert.ToDecimal(item.TotalPremium);
                ListEndorsmentDetail.TotalSumInsured = Convert.ToDecimal(item.TotalSumInsured);
                ListEndorsmentDetail.PaymentMethodId = Convert.ToInt32(item.PaymentMethodId);
                ListEndorsmentDetail.CustomerId = Convert.ToInt32(item.CustomerId);
                ListEndorsmentDetail.CustomerEmail = GetCustomerEmailbyCustomerID(item.CustomerId);

                var endCustomerDetail = InsuranceContext.EndorsementCustomers.Single(item.EndorsementCustomerId);

                if(endCustomerDetail!=null)
                    ListEndorsmentDetail.CustomerName = endCustomerDetail.FirstName + " " + endCustomerDetail.LastName;

                ListEndorsmentDetail.SummaryId = item.SummaryId;
                ListEndorsmentDetail.createdOn = Convert.ToDateTime(item.CreatedOn);
                ListEndorsmentDetail.EndorsementSummaryId = item.Id;
              

                if (Endorsementvehicle != null)
                {
                    var Endorsepolicy = InsuranceContext.EndorsementPolicyDetails.Single(Endorsementvehicle.EndorsementPolicyId);
                    var product = InsuranceContext.Products.Single(Convert.ToInt32(Endorsementvehicle.ProductId));

                    ListEndorsmentDetail.PolicyNumber = Endorsepolicy.PolicyNumber;

                    ListEndorsmentDetail.Currency = detialService.GetCurrencyName(currencyList, Endorsementvehicle.CurrencyId);

                    // int i = 0;

                }
                EndorsementVehicleReinsurance obj = new EndorsementVehicleReinsurance();
                var _Endorsementvehicle = InsuranceContext.EndorsementVehicleDetails.Single(Endorsementsummaryvehicledetail[0].EndorsementVehicleId);
                if (_Endorsementvehicle != null)
                {

                    //var _reinsurenaceTrans = InsuranceContext.ReinsuranceTransactions.All(where: $"SummaryDetailId={item.Id} and VehicleId={SummaryVehicleDetails[0].VehicleDetailsId}").ToList();



                    obj.CoverType = Convert.ToInt32(_Endorsementvehicle.CoverTypeId);
                    obj.isReinsurance = (_Endorsementvehicle.SumInsured > 100000 ? true : false);
                    obj.MakeId = _Endorsementvehicle.MakeId;
                    obj.ModelId = _Endorsementvehicle.ModelId;
                    obj.RegisterationNumber = _Endorsementvehicle.RegistrationNo;
                    obj.SumInsured = Convert.ToDecimal(_Endorsementvehicle.SumInsured);
                    obj.VehicleId = _Endorsementvehicle.Id;
                    obj.startdate = Convert.ToDateTime(_Endorsementvehicle.CoverStartDate);
                    obj.enddate = Convert.ToDateTime(_Endorsementvehicle.CoverEndDate);
                    obj.RenewalDate = Convert.ToDateTime(_Endorsementvehicle.RenewalDate);
                    obj.isLapsed = _Endorsementvehicle.isLapsed;
                    obj.BalanceAmount = Convert.ToDecimal(_Endorsementvehicle.BalanceAmount);
                    obj.isActive = Convert.ToBoolean(_Endorsementvehicle.IsActive);
                    obj.Premium = Convert.ToDecimal(_Endorsementvehicle.Premium + _Endorsementvehicle.StampDuty + _Endorsementvehicle.ZTSCLevy + (Convert.ToBoolean(_Endorsementvehicle.IncludeRadioLicenseCost) ? Convert.ToDecimal(_Endorsementvehicle.RadioLicenseCost) : 0.00m));

                   

                    //if (_reinsurenaceTrans != null && _reinsurenaceTrans.Count > 0)
                    //{
                    //    obj.BrokerCommission = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceCommission);
                    //    obj.AutoFacPremium = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsurancePremium);
                    //    obj.AutoFacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[0].ReinsuranceAmount);

                    //    if (_reinsurenaceTrans.Count > 1)
                    //    {
                    //        obj.FacultativeCommission = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceCommission);
                    //        obj.FacPremium = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsurancePremium);
                    //        obj.FacReinsuranceAmount = Convert.ToDecimal(_reinsurenaceTrans[1].ReinsuranceAmount);
                    //    }



                    //    policylistviewmodel.Vehicles.Add(obj);
                    //}
                    Endorpolicylist.listendorsementpolicy.Add(ListEndorsmentDetail);
                }
            }

            return View(Endorpolicylist);
        }

        public ActionResult ViewEndorsementCustomer(int id = 0)
        {

            EndorsementCustomerModel endorcustom = new EndorsementCustomerModel();
            if (id != 0)
            {
                var EndorsementSummery = InsuranceContext.EndorsementSummaryDetails.All(where: $"Id ='{id}'").FirstOrDefault();
                var Endorsenecustomer = InsuranceContext.EndorsementCustomers.Single(where: $"Id = '{EndorsementSummery.EndorsementCustomerId}'");
                endorcustom = Mapper.Map<EndorsementCustomer, EndorsementCustomerModel>(Endorsenecustomer);

                if (endorcustom != null)
                {
                    var dbUser = UserManager.Users.FirstOrDefault(c => c.Id == endorcustom.UserID);
                    if (dbUser != null)
                    {

                        endorcustom.EmailAddress = dbUser.Email; ;
                        endorcustom.PrimeryCustomerId = endorcustom.Id;
                        endorcustom.SummaryId = id;
                        endorcustom.DateOfBirth = Convert.ToDateTime(endorcustom.DateOfBirth).ToShortDateString();


                    }
                }

                string path = Server.MapPath("~/Content/Countries.txt");
                var countries = System.IO.File.ReadAllText(path);
                var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(countries);
                ViewBag.Countries = resultt.countries.OrderBy(x => x.code.Replace("+", ""));
                ViewBag.Cities = InsuranceContext.Cities.All();
                Session["EndorsummeryDetail"] = id;
            }
            else
            {
                return RedirectToAction("EndorsementDetail", "Endorsement");
            }
            return View(endorcustom);
        }

        [HttpPost]
        public async Task<JsonResult> SaveEnCustomerData(EndorsementCustomerModel model, string buttonUpdate)
        {
            ModelState.Remove("City");
            ModelState.Remove("CountryCode");

            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (userLoggedin)
                {

                    Session["EndorseCustomerDataModal"] = model;
                    return Json(new { IsError = true, error = "" }, JsonRequestBehavior.AllowGet);
                }


            }
            return Json(new { IsError = false, error = TempData["ErrorMessage"].ToString() }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EndorseProductDetail()
        {

            return RedirectToAction("EndorsementRiskDetail");
        }
        public ActionResult EndorsementRiskDetail(int? id = 1)
        {
            var endorsementRisk = new EndorsementRiskDetailModel();
            SetEndorseValueIntoSession(Convert.ToInt32(Session["EndorsummeryDetail"]));
            endorsementRisk.EndorsementSummaryId = Convert.ToInt32(Session["EndorsummeryDetail"]);


            ViewBag.Products = InsuranceContext.Products.All().ToList();
            ViewBag.Currencies = InsuranceContext.Currencies.All();
            var ePaymentTermData = from ePaymentTerm e in Enum.GetValues(typeof(ePaymentTerm))
                                   select new
                                   {
                                       ID = (int)e,
                                       Name = e.ToString()
                                   };

            ViewBag.ePaymentTermData = new SelectList(ePaymentTermData, "ID", "Name");
            int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var _PolicyData = (EndorsementPolicyDetail)Session["EnPolicyDataView"];
            // Id is policyid from Policy detail table

            var service = new VehicleService();

            ViewBag.VehicleUsage = service.GetAllVehicleUsage();


            ViewBag.VehicleLicensePaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is Null").ToList();
            ViewBag.RadioLicensePaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is Null").ToList();



            endorsementRisk.NumberofPersons = 0;
            endorsementRisk.AddThirdPartyAmount = 0.00m;
            endorsementRisk.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType();
            ViewBag.AgentCommission = service.GetAgentCommission();
            ViewBag.Makers = makers;
            endorsementRisk.isUpdate = false;
            endorsementRisk.VehicleUsage = 0;
            TempData["Policy"] = _PolicyData;
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;
            }

            if (TempData["ViewModel"] != null)
            {
                endorsementRisk = (EndorsementRiskDetailModel)TempData["ViewModel"];
                return View(endorsementRisk);
            }



            endorsementRisk.NoOfCarsCovered = 1;
            if (Session["EndorselistVehicles"] != null)
            {
                var list = (List<EndorsementRiskDetailModel>)Session["EndorselistVehicles"];
                if (list.Count > 0)
                {
                    endorsementRisk.NoOfCarsCovered = list.Count + 1;
                }
            }
            if (id > 0)
            {
                var list = (List<EndorsementRiskDetailModel>)Session["EndorselistVehicles"];
                if (list != null && list.Count > 0 && (list.Count >= id))
                {
                    var data = (EndorsementRiskDetailModel)list[Convert.ToInt32(id - 1)];
                    if (data != null)
                    {
                        endorsementRisk.AgentCommissionId = data.AgentCommissionId;
                        endorsementRisk.ChasisNumber = data.ChasisNumber;
                        endorsementRisk.CoverEndDate = data.CoverEndDate;
                        endorsementRisk.CoverNoteNo = data.CoverNoteNo;
                        endorsementRisk.CoverStartDate = data.CoverStartDate;
                        endorsementRisk.CoverTypeId = data.CoverTypeId;
                        endorsementRisk.CubicCapacity = (int)Math.Round(data.CubicCapacity.Value, 0);
                        endorsementRisk.CustomerId = data.CustomerId;
                        endorsementRisk.EngineNumber = data.EngineNumber;
                        // viewModel.Equals = data.Equals;
                        endorsementRisk.Excess = (int)Math.Round(data.Excess, 0);
                        endorsementRisk.ExcessType = data.ExcessType;
                        endorsementRisk.MakeId = data.MakeId;
                        endorsementRisk.ModelId = data.ModelId;
                        endorsementRisk.NoOfCarsCovered = id;
                        endorsementRisk.OptionalCovers = data.OptionalCovers;
                        endorsementRisk.PolicyId = data.PolicyId;
                        endorsementRisk.Premium = data.Premium;
                        endorsementRisk.RadioLicenseCost = (int)Math.Round(data.RadioLicenseCost == null ? 0 : data.RadioLicenseCost.Value, 0);
                        endorsementRisk.Rate = data.Rate;
                        endorsementRisk.RegistrationNo = data.RegistrationNo;
                        endorsementRisk.StampDuty = data.StampDuty;
                        endorsementRisk.SumInsured = (int)Math.Round(data.SumInsured == null ? 0 : data.SumInsured.Value, 0);
                        endorsementRisk.VehicleColor = data.VehicleColor;
                        endorsementRisk.VehicleUsage = data.VehicleUsage;
                        endorsementRisk.VehicleYear = data.VehicleYear;
                        endorsementRisk.Id = data.Id;
                        endorsementRisk.ZTSCLevy = data.ZTSCLevy;
                        endorsementRisk.NumberofPersons = data.NumberofPersons;
                        endorsementRisk.PassengerAccidentCover = data.PassengerAccidentCover;
                        endorsementRisk.IsLicenseDiskNeeded = data.IsLicenseDiskNeeded;
                        endorsementRisk.ExcessBuyBack = data.ExcessBuyBack;
                        endorsementRisk.RoadsideAssistance = data.RoadsideAssistance;
                        endorsementRisk.MedicalExpenses = data.MedicalExpenses;
                        endorsementRisk.Addthirdparty = data.Addthirdparty;
                        endorsementRisk.AddThirdPartyAmount = data.AddThirdPartyAmount;
                        endorsementRisk.ExcessAmount = data.ExcessAmount;
                        endorsementRisk.ProductId = data.ProductId;
                        endorsementRisk.PaymentTermId = data.PaymentTermId;
                        endorsementRisk.IncludeRadioLicenseCost = data.IncludeRadioLicenseCost;
                        endorsementRisk.Discount = data.Discount;
                        endorsementRisk.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);

                        endorsementRisk.VehicleUsage = data.VehicleUsage;
                        endorsementRisk.CustomerId = data.CurrencyId;

                        endorsementRisk.isUpdate = true;
                        endorsementRisk.vehicleindex = Convert.ToInt32(id);

                        var ser = new VehicleService();
                        var model = ser.GetModel(data.MakeId);
                        ViewBag.Model = model;
                    }
                }
            }
            return View(endorsementRisk);
        }
        public void SetEndorseValueIntoSession(int summaryId)
        {
            //Session["ICEcashToken"] = null;

            Session["EndorsummeryDetail"] = summaryId;

            var EnsummaryDetail = InsuranceContext.EndorsementSummaryDetails.Single(summaryId);
            var EnSummaryVehicleDetails = InsuranceContext.EndorsementSummaryVehicleDetails.All(where: $"EndorsementSummaryId={summaryId}").ToList();
            var Envehicle = InsuranceContext.EndorsementVehicleDetails.Single(EnSummaryVehicleDetails[0].EndorsementVehicleId);
            var Enpolicy = InsuranceContext.EndorsementPolicyDetails.Single(Envehicle.EndorsementPolicyId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(Enpolicy.PolicyName));


            Session["EnPolicyDataView"] = Enpolicy;

            List<EndorsementRiskDetailModel> listRiskDetail = new List<EndorsementRiskDetailModel>();
            foreach (var item in EnSummaryVehicleDetails)
            {


                var _vehicle = InsuranceContext.EndorsementVehicleDetails.Single(where: $"Id ='{item.EndorsementVehicleId}'");
                EndorsementRiskDetailModel _riskDetail = Mapper.Map<EndorsementVehicleDetail, EndorsementRiskDetailModel>(_vehicle);
                listRiskDetail.Add(_riskDetail);
            }
            // Session["VehicleDetails"] = listRiskDetail;
            Session["EndorselistVehicles"] = listRiskDetail;

            EndorsementSummaryDetailModel summarymodel = Mapper.Map<EndorsementSummaryDetail, EndorsementSummaryDetailModel>(EnsummaryDetail);
            summarymodel.Id = EnsummaryDetail.Id;

            Session["EndorsementSummaryDetail"] = EnsummaryDetail;

        }


        public JsonResult getEdorsementVehicle()
        {
            try
            {
                if (Session["EndorselistVehicles"] != null)
                {
                    var list = (List<EndorsementRiskDetailModel>)Session["EndorselistVehicles"];
                    List<VehicleListModel> vehiclelist = new List<VehicleListModel>();

                    SummaryDetailService serviceDetails = new SummaryDetailService();

                    var currencyList = serviceDetails.GetAllCurrency();

                    foreach (var item in list)
                    {
                        VehicleListModel obj = new VehicleListModel();
                        obj.make = InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{item.MakeId}'").ShortDescription;
                        obj.model = InsuranceContext.VehicleModels.Single(where: $"ModelCode='{item.ModelId}'").ShortDescription;
                        obj.covertype = InsuranceContext.CoverTypes.Single(item.CoverTypeId).Name;
                        obj.premium = item.Premium.ToString();
                        obj.suminsured = item.SumInsured.ToString();
                        obj.ZTSCLevy = item.ZTSCLevy == null ? "0" : item.ZTSCLevy.ToString();
                        obj.CurrencyName = serviceDetails.GetCurrencyName(currencyList, item.CurrencyId);
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

        public ActionResult EndorsementSummaryDetails(int? Id = 0)
        {

            var _model = new SummaryDetailModel();
            var summaryDetail = Session["EndorsementSummaryDetail"];
            var Endorsementvehicle = (List<EndorsementRiskDetailModel>)Session["EndorselistVehicles"];

            /* var Endorsevehicle = (List<EndorsementRiskDetailModel>)Session["ViewlistVehicles"];*/// summary.GetVehicleInformation(id);
            var summarydetail = (EndorsementSummaryDetail)Session["EndorsementSummaryDetail"];
            SummaryDetailService SummaryDetailServiceObj = new SummaryDetailService();

            if (summarydetail != null)
            {
                var model = Mapper.Map<EndorsementSummaryDetail, EndorsementSummaryDetailModel>(summarydetail);
                model.CarInsuredCount = Endorsementvehicle.Count;
                model.DebitNote = "INV" + Convert.ToString(SummaryDetailServiceObj.getNewDebitNote());
                model.PaymentMethodId = summarydetail.PaymentMethodId;
                model.PaymentTermId = 1;
                model.ReceiptNumber = "";
                model.SMSConfirmation = false;

                model.TotalPremium = Endorsementvehicle.Sum(item => item.Premium + item.ZTSCLevy + item.StampDuty + item.VehicleLicenceFee + (item.IncludeRadioLicenseCost ? item.RadioLicenseCost : 0.00m));// + vehicle.StampDuty + vehicle.ZTSCLevy;
                                                                                                                                                                                                             //model.TotalRadioLicenseCost = vehicle.Sum(item => item.RadioLicenseCost);
                model.TotalStampDuty = Endorsementvehicle.Sum(item => item.StampDuty);
                model.TotalSumInsured = Endorsementvehicle.Sum(item => item.SumInsured);
                model.TotalZTSCLevies = Endorsementvehicle.Sum(item => item.ZTSCLevy);
                model.ExcessBuyBackAmount = Endorsementvehicle.Sum(item => item.ExcessBuyBackAmount);
                model.MedicalExpensesAmount = Endorsementvehicle.Sum(item => item.MedicalExpensesAmount);
                model.PassengerAccidentCoverAmount = Endorsementvehicle.Sum(item => item.PassengerAccidentCoverAmount);
                model.RoadsideAssistanceAmount = Endorsementvehicle.Sum(item => item.RoadsideAssistanceAmount);
                model.ExcessAmount = Endorsementvehicle.Sum(item => item.ExcessAmount);
                model.Discount = Endorsementvehicle.Sum(item => item.Discount);
                decimal radio = 0.00m;
                foreach (var item in Endorsementvehicle)
                {
                    if (item.IncludeRadioLicenseCost)
                    {
                        radio += Convert.ToDecimal(item.RadioLicenseCost);
                    }
                }
                model.TotalRadioLicenseCost = radio;
                ViewBag.Endorsementid = model.Id;
                //var Model = Mapper.Map<SummaryDetailModel, SummaryDetail>(summarydetail);
                //InsuranceContext.SummaryDetails.Insert(Model);

                return View(model);
            }

            return View();
        }
        public JsonResult GetvalueSession()
        {
            var test = (EndorsementSummaryDetail)Session["EnsummaryId"];
            var result = test.Id;

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public JsonResult gotoEndorsementExit(int? id)
        {
            JsonResult jsonResult = new JsonResult();

            Session.Remove("PolicyDataView");
            Session.Remove("EnViewlistVehicles");
            Session.Remove("ENViewSummaryDetail");
            Session.Remove("EndoesementitemData");
            Session.Remove("EndorsementPaymentId");
            Session.Remove("EndoesementitemData");
            Session.Remove("EndorsementCardDetail");
            Session.Remove("EnsummaryId");
            Session.Remove("EnCustomerDetail");
            jsonResult.Data = 1;

            return jsonResult;
        }
        private void RemoveEndorsementSession()
        {
            Session.Remove("PolicyDataView");
            Session.Remove("EnViewlistVehicles");
            Session.Remove("ENViewSummaryDetail");
            Session.Remove("EndoesementitemData");
            Session.Remove("EndorsementPaymentId");
            Session.Remove("EndoesementitemData");
            Session.Remove("EndorsementCardDetail");
            Session.Remove("EnsummaryId");
            Session.Remove("EnCustomerDetail");

        }


    }

}