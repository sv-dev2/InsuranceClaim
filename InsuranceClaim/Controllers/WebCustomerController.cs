using Insurance.Domain;
using Insurance.Service;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public JsonResult getPolicyDetailsFromICEcash(string regNo, string PaymentTerm, string SumInsured, int CoverTypeId, int VehicleType, bool VehilceLicense, bool RadioLicense, string firstName, string lastName, string email, string address, string phone, string nationalId)
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

                    if (VehilceLicense)
                        quoteresponse = ICEcashService.TPILICQuote(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, 1, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1", VehilceLicense, RadioLicense);
                    else
                        quoteresponse = ICEcashService.RequestQuote(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, 1, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1");


                    // Invalid Partner Token. 

                    if (quoteresponse.Response != null && (quoteresponse.Response.Message.Contains("Partner Token has expired") || quoteresponse.Response.Message.Contains("Invalid Partner Token")))
                    {
                        tokenObject = ICEcashService.getToken();
                        SummaryDetailService.UpdateToken(tokenObject);

                        patnerToken = tokenObject.Response.PartnerToken;
                        //   tokenObject = (ICEcashTokenResponse)Session["ICEcashToken"];
                        //tokenObject = service.CheckSessionExpired();
                        if (VehilceLicense)
                            quoteresponse = ICEcashService.TPILICQuote(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, 1, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1", VehilceLicense, RadioLicense);
                        else
                            quoteresponse = ICEcashService.RequestQuote(patnerToken, regNo, SumInsured, "", "", Convert.ToInt32(PaymentTerm), Convert.ToInt32(DateTime.Now.Year), CoverTypeId, 1, tokenObject.PartnerReference, Cover_StartDate, Cover_EndDate, "1");

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








    }
}