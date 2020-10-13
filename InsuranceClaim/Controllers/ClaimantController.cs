using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using AutoMapper;
using Insurance.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Insurance.Service;
using System.IO;
using System.Globalization;

namespace InsuranceClaim.Controllers
{
    public class ClaimantController : Controller
    {
        public ActionResult SaveClaimant()
        {
            var service = new VehicleService();
            var makers = service.GetMakers();

            ViewBag.Makers = makers;
            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;
            }
            return View();
        }

        [HttpPost]
        public ActionResult SaveClaimant(ClaimNotificationModel model)
        {
            Insurance.Service.EmailService log = new Insurance.Service.EmailService();
            
           


            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                string userid = "";


              
                    if (userLoggedin)
                    {
                        userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        var customer = InsuranceContext.Customers.Single(where: $"UserId ='{userid}'");
                        var dbModel = Mapper.Map<ClaimNotificationModel, ClaimNotification>(model);
                        var policy = model.PolicyNumber;
                        var Detailofpolicy = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber='{policy}'");

                       // log.WriteLog("Detailofpolicy :");
                        var vehicalDetails = new VehicleDetail();

                        if (Detailofpolicy == null)
                        {
                            vehicalDetails = InsuranceContext.VehicleDetails.Single(where: $"RenewPolicyNumber='{policy}'");

                            if (vehicalDetails != null)
                            {
                                Detailofpolicy = InsuranceContext.PolicyDetails.Single(vehicalDetails.PolicyId);
                            }
                        }

                        //  var vehicalDetails = InsuranceContext.VehicleDetails.Single(where: $"PolicyId='{Detailofpolicy.Id}' and RegistrationNo='" + model.RegistrationNo + "'");

                        if (vehicalDetails != null)
                        {
                            dbModel.VehicleId = vehicalDetails.Id;
                        }
                        dbModel.PolicyId = Detailofpolicy.Id;
                        dbModel.CreatedBy = customer.Id;
                        dbModel.CreatedOn = DateTime.Now;
                        dbModel.IsDeleted = true;
                        dbModel.IsRegistered = false;
                        dbModel.PolicyNumber = Detailofpolicy.PolicyNumber;

       

                        dbModel.DateOfLoss = model.DateOfLoss;



                       // log.WriteLog("DateOfLoss :" + model.DateOfLoss);

                        InsuranceContext.ClaimNotifications.Insert(dbModel);
                    }
                

               

                return RedirectToAction("ClaimantList");




            }

            return View();
        }


        // GET: Claimant
        //[Authorize(Roles = "Staff")]
        [HttpGet]
        public ActionResult ClaimantList()
        {
            SummaryDetailService _summaryDetailService = new SummaryDetailService();

            var currenyList = _summaryDetailService.GetAllCurrency();

            List<ClaimNotificationModel> objList = new List<ClaimNotificationModel>();

            objList = (from j in InsuranceContext.ClaimNotifications.All().ToList()
                       join jt in InsuranceContext.PolicyDetails.All().ToList()
                       on j.PolicyId equals jt.Id
                       into rd
                       from rt in rd.DefaultIfEmpty()
                       join make in InsuranceContext.VehicleMakes.All() on j.ThirdPartyMakeId equals make.MakeCode into makes
                       from m in makes.DefaultIfEmpty()
                       join model in InsuranceContext.VehicleModels.All() on j.ThirdPartyModelId equals model.ModelCode into models
                       from mod in models.DefaultIfEmpty()
                       where j.IsDeleted == true && j.IsRegistered == false
                       select new ClaimNotificationModel
                       {

                           PolicyNumber = j.PolicyNumber,
                           DateOfLoss = j.DateOfLoss,
                           PlaceOfLoss = j.PlaceOfLoss,
                           DescriptionOfLoss = j.DescriptionOfLoss,
                           EstimatedValueOfLoss = j.EstimatedValueOfLoss,
                           ThirdPartyInvolvement = j.ThirdPartyInvolvement,
                           ClaimantName = j.ClaimantName,
                           RegistrationNo = j.RegistrationNo,
                           ThirdPartyName = j.ThirdPartyName,
                           ThirdPartyContactDetails = j.ThirdPartyContactDetails,
                           ThirdPartyMakeId = m == null ? "" : m.MakeDescription,
                           ThirdPartyModelId = mod == null ? "" : mod.ModelDescription,
                           CoverStartDate = Convert.ToDateTime(j.CoverStartDate),
                           CoverEndDate = Convert.ToDateTime(j.CoverEndDate),
                           ThirdPartyEstimatedValueOfLoss = j.ThirdPartyEstimatedValueOfLoss,
                           CustomerName = j.CustomerName,
                           ThirdPartyDamageValue = j.ThirdPartyDamageValue,
                           Id = j.Id,
                           IsExists = rt == null ? false : true,
                           Currency = _summaryDetailService.GetCurrencyName(currenyList, rt==null? 6: rt.CurrencyId),

                       }
                     ).OrderByDescending(c=>c.Id).ToList();

            return View(objList);
        }

        [HttpGet]
        public ActionResult EditClaimant(int Id)
        {
            var record = InsuranceContext.ClaimNotifications.All(where: $"Id ={Id}").FirstOrDefault();
            var model = Mapper.Map<ClaimNotification, ClaimNotificationModel>(record);
            var service = new VehicleService();
            var makers = service.GetMakers();
            ViewBag.Makers = makers;
            if (makers.Count > 0)
            {
                var _model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = _model;
            }
            model.ThirdPartyModelId = record.ThirdPartyModelId;
            return View(model);
        }


        [HttpPost]
        public ActionResult EditClaimant(ClaimNotificationModel model)
        {
            if (ModelState.IsValid)
            {
                string userid = "";
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                if (userLoggedin)
                {
                    userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId ='{userid}'");
                    var data = Mapper.Map<ClaimNotificationModel, ClaimNotification>(model);

                    var record = InsuranceContext.ClaimNotifications.Single(where: $"Id = '{model.Id}'");
                    data.ModifiedOn = DateTime.Now;
                    data.IsDeleted = true;
                    data.IsRegistered = false;
                    data.ModifiedBy = Convert.ToInt32(customer.Id);
                    data.CreatedOn = Convert.ToDateTime(record.CreatedOn);
                    InsuranceContext.ClaimNotifications.Update(data);
                    return RedirectToAction("ClaimantList");
                }
            }
            return View("EditClaimant");
        }
        public ActionResult DeleteClaimant(int Id)
        {
            string query = $"update ClaimNotification set IsDeleted = 0 where Id = {Id}";
            InsuranceContext.ClaimNotifications.Execute(query);
            return RedirectToAction("ClaimantList");
        }

        public ActionResult SaveUpdatedata(string PolicyNumber, int id)
        {

            var ClaimDetail = InsuranceContext.ClaimNotifications.All().SingleOrDefault(p => p.Id == id);
            //var VehicleDetail = InsuranceContext.VehicleDetails.All().Where(p => p.RegistrationNo == ClaimDetail.RegistrationNo).ToList();
            //var vehicleId = InsuranceContext.VehicleDetails.All().SingleOrDefault(p => p.RegistrationNo == ClaimDetail.RegistrationNo);

            var query = "select VehicleDetail.* from VehicleDetail join PolicyDetail on VehicleDetail.PolicyId = PolicyDetail.Id";
            query += " where VehicleDetail.IsActive=1 and PolicyDetail.PolicyNumber = '" + PolicyNumber + "' and VehicleDetail.RegistrationNo = '" + ClaimDetail.RegistrationNo + "'";


            var VehicleDetail = InsuranceContext.Query(query).Select(x => new VehicleDetail()
            {
                Id = x.Id,
                MakeId = x.MakeId,
                ModelId = x.ModelId

            }).FirstOrDefault();

            if(VehicleDetail==null)
            {
                query = "";
                 query = "select VehicleDetail.* from VehicleDetail join PolicyDetail on VehicleDetail.PolicyId = PolicyDetail.Id";
                query += " where VehicleDetail.IsActive=1 and RenewPolicyNumber = '" + PolicyNumber + "' and VehicleDetail.RegistrationNo = '" + ClaimDetail.RegistrationNo + "'";


                 VehicleDetail = InsuranceContext.Query(query).Select(x => new VehicleDetail()
                {
                    Id = x.Id,
                    MakeId = x.MakeId,
                    ModelId = x.ModelId

                }).FirstOrDefault();
            }


            //save in ClaimRegistration
            ClaimRegistrationModel model = new ClaimRegistrationModel();
            var claimNumber = GenerateClaimNumber();
            model.PolicyNumber = PolicyNumber;
            model.ClaimNumber = claimNumber;
            model.DateOfLoss = Convert.ToDateTime(ClaimDetail.DateOfLoss.ToShortDateString());
            model.DateOfNotifications = Convert.ToDateTime(ClaimDetail.CreatedOn.ToShortDateString());
            model.PlaceOfLoss = ClaimDetail == null ? null : ClaimDetail.PlaceOfLoss;
            model.DescriptionOfLoss = ClaimDetail == null ? null : ClaimDetail.DescriptionOfLoss;
            model.EstimatedValueOfLoss = ClaimDetail.EstimatedValueOfLoss;
            model.VehicleDetailId = VehicleDetail.Id;
            model.RegistrationNo = ClaimDetail.RegistrationNo;
            model.ThirdPartyDamageValue = ClaimDetail.ThirdPartyDamageValue;
            model.MakeId = VehicleDetail.MakeId;
            model.ModelId = VehicleDetail.ModelId;
            model.Claimsatisfaction = true;
            model.ClaimStatus = "1";
            model.CreatedOn = DateTime.Now;
            model.ClaimNotificationId = id;
            model.ClaimantName = ClaimDetail.ClaimantName;
            model.ThirdPartyInvolvement = ClaimDetail.ThirdPartyInvolvement;
            var dbModel = Mapper.Map<ClaimRegistrationModel, ClaimRegistration>(model);
            InsuranceContext.ClaimRegistrations.Insert(dbModel);

            // save in claimNotification


            var NotificationId = ClaimDetail.Id;
            var updateNotificationRecord = InsuranceContext.ClaimNotifications.Single(NotificationId);
            updateNotificationRecord.IsRegistered = true;
            InsuranceContext.ClaimNotifications.Update(updateNotificationRecord);

            return RedirectToAction("RegisterClaim", "Claimant", new { @id = id, @PolicyNumber = PolicyNumber, @dbmodel = dbModel.Id });
        }
        public ActionResult RegisterClaim(string PolicyNumber, int id, int dbmodel)
        {
            try
            {

                ViewBag.ClaimStatus = InsuranceContext.ClaimStatuss.All().ToList();
                var service = new VehicleService();
                if (PolicyNumber != "")
                {
                    var PolicyDetail = InsuranceContext.PolicyDetails.All().FirstOrDefault(p => p.PolicyNumber == PolicyNumber);
                    var ClaimDetail = InsuranceContext.ClaimNotifications.All().SingleOrDefault(p => p.Id == id);
                    var Policyid = PolicyDetail.Id;
                    var CustomerId = PolicyDetail.CustomerId;
                    
                    var VehicleDetail = InsuranceContext.VehicleDetails.All().Where(p => p.RegistrationNo == ClaimDetail.RegistrationNo && p.PolicyId == Policyid).ToList();


                    RegisterClaimViewModel VehicleDetailVM = new RegisterClaimViewModel();

                    List<RiskViewModel> VehicleData = new List<RiskViewModel>();
                    List<ChecklistModel> ChecklistModel = new List<ChecklistModel>();

                    var Checklist = InsuranceContext.Checklists.All().ToList();

                    ChecklistModel = Checklist.Select(p => new ChecklistModel()
                    {

                        Id = p.Id,
                        ChecklistDetail = p.ChecklistDetail,
                        IsChecked = false

                    }).ToList();

                    //get only Vehicle Detail
                    VehicleData = VehicleDetail.Select(p => new RiskViewModel()
                    {
                        Make = InsuranceContext.VehicleMakes.All().Where(q => q.MakeCode == p.MakeId).FirstOrDefault().MakeDescription,
                        Model = InsuranceContext.VehicleModels.All().Where(q => q.ModelCode == p.ModelId).FirstOrDefault().ModelDescription,
                        paymentTerm = InsuranceContext.PaymentTerms.All().Where(q => q.Id == p.PaymentTermId).FirstOrDefault().Name,
                        Product = InsuranceContext.Products.All().Where(q => q.Id == p.ProductId).FirstOrDefault().ProductName,
                        VehUsage = InsuranceContext.VehicleUsages.All().Where(q => q.Id == p.VehicleUsage).FirstOrDefault().VehUsage,
                        CoverType = InsuranceContext.CoverTypes.All().Where(q => q.Id == p.CoverTypeId).FirstOrDefault().Name,
                        VehicleYear = p.VehicleYear,
                        CubicCapacity = p.CubicCapacity,
                        EngineNumber = p.EngineNumber,
                        ChasisNumber = p.ChasisNumber,
                        AddThirdPartyAmount = p.AddThirdPartyAmount,
                        Excess = p.Excess,
                        ExcessType = Convert.ToString(p.ExcessType),
                        Premium = p.Premium,
                        StampDuty = p.StampDuty,
                        ZTSCLevy = p.ZTSCLevy,
                        Discount = p.Discount,

                        VehicleLicenceFee = p.VehicleLicenceFee,
                        Addthirdparty = p.Addthirdparty == true ? "yes" : "No",
                        IncludeRadioLicenseCost = p.IncludeRadioLicenseCost == true ? "yes" : "No",
                        IsLicenseDiskNeeded = p.IsLicenseDiskNeeded == true ? "yes" : "No",
                        FirstName = InsuranceContext.Customers.All().Where(q => q.Id == p.CustomerId).FirstOrDefault().FirstName,
                        LastName = InsuranceContext.Customers.All().Where(q => q.Id == p.CustomerId).FirstOrDefault().LastName,
                        RegisterNumber = p.RegistrationNo,
                        CoverStartDate = Convert.ToString(p.CoverStartDate.Value.ToShortDateString()),
                        CoverEndDate = Convert.ToString(p.CoverEndDate.Value.ToShortDateString()),
                        SumInsured = p.SumInsured,
                        RadioLicenseCost = p.RadioLicenseCost,
                        PassengerAccidentCover = p.PassengerAccidentCover == true ? "yes" : "No",
                        ExcessBuyBack = p.ExcessBuyBack == true ? "yes" : "No",
                        RoadsideAssistance = p.RoadsideAssistance == true ? "yes" : "No",
                        MedicalExpenses = p.MedicalExpenses == true ? "yes" : "No"
                    }).ToList();


                    VehicleDetailVM = new RegisterClaimViewModel()
                    {
                        PolicyNumber = PolicyDetail.PolicyNumber,
                        DateOfLoss = ClaimDetail.DateOfLoss.ToShortDateString(),
                        PlaceOfLoss = ClaimDetail == null ? null : ClaimDetail.PlaceOfLoss,
                        DescriptionOfLoss = ClaimDetail == null ? null : ClaimDetail.DescriptionOfLoss,
                        EstimatedValueOfLoss = ClaimDetail.EstimatedValueOfLoss,
                        ThirdPartyDamageValue = ClaimDetail.ThirdPartyDamageValue,
                        DateOfNotifications = ClaimDetail.CreatedOn.ToShortDateString(),
                        RiskViewModel = VehicleData,
                        chklist = ChecklistModel,
                        FirstName = InsuranceContext.Customers.All().Where(q => q.Id == CustomerId).FirstOrDefault().FirstName,
                        LastName = InsuranceContext.Customers.All().Where(q => q.Id == CustomerId).FirstOrDefault().LastName,

                    };
                    VehicleDetailVM.ThirdPartyInvolvement = ClaimDetail.ThirdPartyInvolvement;
                    var claimregistrationdetail = InsuranceContext.ClaimRegistrations.Single(where: $"Id = '{dbmodel}'");

                    VehicleDetailVM.ClaimId = dbmodel;
                    VehicleDetailVM.Claimnumber = claimregistrationdetail.ClaimNumber;
                    VehicleDetailVM.Claimsatisfaction = claimregistrationdetail.Claimsatisfaction;
                    return View(VehicleDetailVM);
                }
                return View();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult ClaimDetailServiceProvider(int id, string PolicyNumber, int claimNumber)
        {
            ClaimDetailsProviderModel model = new ClaimDetailsProviderModel();
            var claimRegisterdata = InsuranceContext.ClaimRegistrations.Single(id);
            var ServiceProvidersList = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();
            var claimdetail = InsuranceContext.ClaimDetailsProviders.Single(where: $"ClaimNumber = '{claimNumber}'");
            if (claimdetail != null && claimdetail.Count() > 0)
            {
                ViewBag.AssessorsType = ServiceProvidersList.Where(w => w.ServiceProviderType == 1).ToList();
                ViewBag.ValuersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 2).ToList();
                ViewBag.LawyersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 3).ToList();
                ViewBag.RepairersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 4).ToList();
                model.AssessorsProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == claimdetail.AssessorsProviderType).Id;
                model.ValuersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == claimdetail.ValuersProviderType).Id;
                model.LawyersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == claimdetail.LawyersProviderType).Id;
                model.RepairersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == claimdetail.RepairersProviderType).Id;
                model.PolicyNumber = claimdetail.PolicyNumber;
                model.ClaimNumber = claimdetail.ClaimNumber;
                return View(model);
            }
            else
            {
                var Providertype = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();
                ViewBag.AssessorsType = Providertype.Where(w => w.ServiceProviderType == 1).ToList();
                ViewBag.ValuersType = Providertype.Where(w => w.ServiceProviderType == 2).ToList();
                ViewBag.LawyersType = Providertype.Where(w => w.ServiceProviderType == 3).ToList();
                ViewBag.RepairersType = Providertype.Where(w => w.ServiceProviderType == 4).ToList();
                model.PolicyNumber = claimRegisterdata.PolicyNumber;
                model.ClaimNumber = Convert.ToInt32(claimRegisterdata.ClaimNumber);
            }
            return View(model);
        }
        public ActionResult SaveClaimDetails(ClaimDetailsProviderModel model)
        {


            if (ModelState.IsValid)
            {
                string customId = "";
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                var claimdata = InsuranceContext.ClaimDetailsProviders.Single(where: $"PolicyNumber = '{model.PolicyNumber}' and ClaimNumber = '{model.ClaimNumber}'");
                if (claimdata == null || claimdata.Count() == 0)
                {
                    if (userLoggedin)
                    {
                        var _userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        var customer = InsuranceContext.Customers.Single(where: $"UserId = '{_userid}'");
                        customId = Convert.ToString(customer.Id);

                        var data = Mapper.Map<ClaimDetailsProviderModel, ClaimDetailsProvider>(model);
                        data.CreatedBy = Convert.ToInt32(customId);
                        data.CreatedOn = DateTime.Now;
                        data.IsActive = true;
                        InsuranceContext.ClaimDetailsProviders.Insert(data);
                    }
                    return RedirectToAction("ClaimRegistrationList");
                }

                else
                {
                    if (userLoggedin)
                    {
                        var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                        customId = Convert.ToString(customer.Id);

                        //var claimDetailsdata = Mapper.Map<ClaimDetailsProviderModel, ClaimDetailsProvider>(model);
                        claimdata.AssessorsProviderType = model.AssessorsProviderType;
                        claimdata.ValuersProviderType = model.ValuersProviderType;
                        claimdata.LawyersProviderType = model.LawyersProviderType;
                        claimdata.RepairersProviderType = model.RepairersProviderType;
                        claimdata.PolicyNumber = model.PolicyNumber;
                        claimdata.ClaimNumber = model.ClaimNumber;
                        claimdata.ModifiedBy = Convert.ToInt32(customId);
                        claimdata.ModifiedOn = DateTime.Now;
                        claimdata.IsActive = true;
                        InsuranceContext.ClaimDetailsProviders.Update(claimdata);
                    }
                    return RedirectToAction("ClaimRegistrationList");
                }
            }
            TempData["Errormsg"] = "Exception Occur. Please Try Again";
            return RedirectToAction("ClaimDetailServiceProvider");
        }
        public ActionResult ClaimDetailsList()
        {

            var query = "select PolicyNumber, ClaimNumber, ClaimRegistration.Id as ClaimRegistrationId, ServiceProvider.ServiceProviderName, ";
            query += " ServiceProviderType.ProviderType, ClaimRegistrationProviderDetial.CreatedOn, ClaimRegistrationProviderDetial.Id from ClaimRegistrationProviderDetial ";
            query += " join ClaimRegistration on ClaimRegistrationProviderDetial.ClaimRegistrationId = ClaimRegistration.id ";
            query += " join ServiceProvider on ClaimRegistrationProviderDetial.ServiceProviderId = ServiceProvider.Id ";
            query += " join ServiceProviderType on ClaimRegistrationProviderDetial.ServiceProviderTypeId = ServiceProviderType.Id ";
            //Ds
            query += " where ClaimRegistrationProviderDetial.IsActive = 1";

            query += " order by ClaimRegistrationProviderDetial.Id desc";

            var ClaimDetailsProviderList = InsuranceContext.Query(query).Select(c => new ClaimDetailsProviderModel
            {
                PolicyNumber = c.PolicyNumber,
                ClaimNumber = c.ClaimNumber,
                ClaimRegistrationId = c.ClaimRegistrationId,
                ServiceProviderName = c.ServiceProviderName,
                ProviderType = c.ProviderType,
                CreatedOn = c.CreatedOn == null ? DateTime.MinValue.ToShortDateString() : Convert.ToString(c.CreatedOn.ToShortDateString()),
                Id = c.Id,
            }).ToList();


            //var claimList = InsuranceContext.ClaimDetailsProviders.All(where: $"IsActive = 'True' or IsActive is null").ToList();
            //var ServiceProvidersList = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();
            //var ClaimDetailsProviderList = new List<ClaimDetailsProviderModel>();
            //if (claimList != null && claimList.Count > 0)
            //{
            //    foreach (var item in claimList)
            //    {
            //        ClaimDetailsProviderModel model = new ClaimDetailsProviderModel();
            //        var asserorType = ServiceProvidersList.FirstOrDefault(c => c.Id == item.AssessorsProviderType);
            //        var valuerTypes = ServiceProvidersList.FirstOrDefault(c => c.Id == item.ValuersProviderType);
            //        var lawerType = ServiceProvidersList.FirstOrDefault(c => c.Id == item.LawyersProviderType);
            //        var repaireType = ServiceProvidersList.FirstOrDefault(c => c.Id == item.RepairersProviderType);
            //        var towingtype = ServiceProvidersList.FirstOrDefault(c => c.Id == item.TownlyProviderType);
            //        var medicaltype = ServiceProvidersList.FirstOrDefault(c => c.Id == item.MedicalProviderType);


            //        model.Assessors_Type = asserorType == null ? "" : asserorType.ServiceProviderName;
            //        model.Valuers_Type = valuerTypes == null ? "" : valuerTypes.ServiceProviderName;
            //        model.Lawyers_Type = lawerType == null ? "" : lawerType.ServiceProviderName;
            //        model.Repairers_Type = repaireType == null ? "" : repaireType.ServiceProviderName;
            //        model.Towing_Type = towingtype == null ? "" : towingtype.ServiceProviderName;
            //        model.Medical_Type = medicaltype == null ? "" : medicaltype.ServiceProviderName;
            //        model.PolicyNumber = item.PolicyNumber;
            //        model.ClaimNumber = item.ClaimNumber;
            //        model.CreatedOn = Convert.ToDateTime(item.CreatedOn).ToShortDateString();
            //        model.Id = item.Id;
            //        model.CreatedBy = item.CreatedBy;


            //        ClaimDetailsProviderList.Add(model);
            //    }
            //}



            return View(ClaimDetailsProviderList);
        }

        public ActionResult EditClaimDetailsProvider(int Id)
        {
            var record = InsuranceContext.ClaimDetailsProviders.Single(where: $"Id ={Id}");
            var ServiceProvidersList = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();
            var model = Mapper.Map<ClaimDetailsProvider, ClaimDetailsProviderModel>(record);
            if (model != null)
            {
                ViewBag.AssessorsType = ServiceProvidersList.Where(w => w.ServiceProviderType == 1).ToList();
                ViewBag.ValuersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 2).ToList();
                ViewBag.LawyersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 3).ToList();
                ViewBag.RepairersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 4).ToList();
                model.AssessorsProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == model.AssessorsProviderType).Id;
                model.ValuersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == model.ValuersProviderType).Id;
                model.LawyersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == model.LawyersProviderType).Id;
                model.RepairersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == model.RepairersProviderType).Id;

            }
            return View(model);
        }
        [HttpPost]
        public ActionResult EditClaimDetailsProvider(ClaimDetailsProviderModel model)
        {

            if (ModelState.IsValid)
            {
                string customId = "";
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                if (userLoggedin)
                {
                    var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                    customId = Convert.ToString(customer.Id);

                    var data = Mapper.Map<ClaimDetailsProviderModel, ClaimDetailsProvider>(model);
                    var record = InsuranceContext.ClaimDetailsProviders.Single(where: $"Id ={model.Id}");
                    data.ModifiedOn = DateTime.Now;
                    data.IsActive = true;
                    data.CreatedBy = record.CreatedBy;
                    data.CreatedOn = record.CreatedOn;
                    data.ModifiedBy = Convert.ToInt32(customId);
                    InsuranceContext.ClaimDetailsProviders.Update(data);
                    return RedirectToAction("ClaimDetailsList");
                }
            }
            return View("EditClaimDetailsProvider");
        }
        public ActionResult DeteteClaimDetailsProvider(int id)
        {

            string query = $"update ClaimRegistrationProviderDetial set IsActive = 0 where Id={id}";
            InsuranceContext.ClaimRegistrationProviderDetials.Execute(query);

            return RedirectToAction("ClaimDetailsList");
        }

        public long GenerateClaimNumber()
        {
            int number = 0;
            long cNumber = 0;
            var objList = InsuranceContext.ClaimRegistrations.All(orderBy: "Id desc").FirstOrDefault();
            if (objList != null)
            {
                number = Convert.ToInt32(objList.ClaimNumber);
                cNumber = Convert.ToInt64(number) + 1;
            }
            else
            {
                cNumber = 1001;
            }


            return cNumber;
        }

        [HttpPost]
        public ActionResult SaveRegisterClaim(RegisterClaimViewModel model, HttpPostedFileBase[] files)
        {

            if (ModelState.IsValid)
            {
                SaveCheckListDocument(files, model.Id);


                var caimNumber = model.Claimnumber;
                var claimStatis = model.Claimsatisfaction;
                var status = model.Status;
                var names = String.Join(",", model.chklist.Where(p => p.IsChecked).Select(p => p.Id));
                var updateRecord = InsuranceContext.ClaimRegistrations.Single(model.ClaimId);
                updateRecord.Checklist = names;
                if (model.PlaceOfLoss != "")
                {
                    updateRecord.PlaceOfLoss = model.PlaceOfLoss;
                }

                if (model.DescriptionOfLoss != "")
                {
                    updateRecord.DescriptionOfLoss = model.DescriptionOfLoss;
                }
                if (model.EstimatedValueOfLoss > 0)
                {
                    updateRecord.EstimatedValueOfLoss = model.EstimatedValueOfLoss;
                }
                if (model.RejectionStatus != "")
                {
                    updateRecord.RejectionStatus = model.RejectionStatus;
                }
                if (model.Status != "")
                {
                    updateRecord.ClaimStatus = Convert.ToInt32(model.Status);
                }
                updateRecord.ThirdPartyDamageValue = model.ThirdPartyDamageValue;
                updateRecord.Claimsatisfaction = claimStatis;
                InsuranceContext.ClaimRegistrations.Update(updateRecord);
                return RedirectToAction("ClaimRegistrationList");
            }
            return RedirectToAction("RegisterClaim");
        }


        public void SaveCheckListDocument(HttpPostedFileBase[] files, int RegisteredId)
        {
            var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
            // var RegisteredId = Request.Form["RegisterId"];
            foreach (HttpPostedFileBase file in files)
            {
                string fname;
                //Checking file is available to save.  
                if (file != null)
                {
                    //Get Guid 
                    Guid id = Guid.NewGuid();
                    var FileName = Path.GetFileName(file.FileName);
                    if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                    {
                        string[] testfiles = file.FileName.Split(new char[] { '\\' });
                        fname = id + "." + testfiles[testfiles.Length - 1].Split('.')[1];
                    }
                    else
                    {
                        fname = id + "." + file.FileName.Split('.')[1];
                    }
                    if (RegisteredId != 0)
                    {
                        string folderpath = @Server.MapPath("~/RegistrationDocument/" + RegisteredId + "/");
                        if (!Directory.Exists(folderpath))
                        {
                            Directory.CreateDirectory(folderpath);
                        }
                        fname = "/RegistrationDocument/" + RegisteredId + "/" + fname;
                        file.SaveAs(Server.MapPath(fname));



                        RegistrationDocument doc = new RegistrationDocument();
                        doc.Name = FileName;
                        doc.FilePath = fname;
                        doc.CreatedBy = customer.Id;
                        doc.ClaimRegistrationId = RegisteredId.ToString();
                        doc.CreatedOn = DateTime.Now;
                        InsuranceContext.RegistrationDocuments.Insert(doc);



                    }
                }
            }
        }

        public ActionResult ClaimRegistrationList()
        {
            ViewBag.servicename = InsuranceContext.ServiceProviderTypes.All();
            ViewBag.providername = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True'").ToList();
            var service = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True'").ToList();
            var claimList = InsuranceContext.ClaimDetailsProviders.All(where: $"IsActive = 'True' or IsActive is null").ToList();

            var Adjustments = InsuranceContext.ClaimAdjustments.All(where: $"IsActive = 'True'").ToList();

            //var list = (from _claimRegistration in InsuranceContext.ClaimRegistrations.All().ToList()
            //            join _clamdetail in InsuranceContext.ClaimDetailsProviders.All().ToList()
            //            on _claimRegistration.ClaimNumber equals _clamdetail.ClaimNumber into data
            //            from date in data.DefaultIfEmpty()
            //            join Claimstatusdata in InsuranceContext.ClaimStatuss.All().ToList()
            //            on _claimRegistration.ClaimStatus equals Claimstatusdata.Id
            //            join make in InsuranceContext.VehicleMakes.All() 
            //            on _claimRegistration.MakeId equals make.MakeCode into makes
            //            from _make in makes.DefaultIfEmpty()
            //            join model in InsuranceContext.VehicleModels.All() 
            //            on _claimRegistration.ModelId equals model.ModelCode into models
            //            from mode in models.DefaultIfEmpty()

            //            select new ClaimRegistrationModel
            //            {
            //                PolicyNumber = _claimRegistration.PolicyNumber,
            //                PaymentDetails = _claimRegistration.PaymentDetails,
            //                ClaimNumber = _claimRegistration.ClaimNumber,
            //                PlaceOfLoss = _claimRegistration.PlaceOfLoss,
            //                DescriptionOfLoss = _claimRegistration.DescriptionOfLoss,
            //                EstimatedValueOfLoss = _claimRegistration.EstimatedValueOfLoss,
            //                ThirdPartyDamageValue = _claimRegistration.ThirdPartyDamageValue,

            //                //AssessProviderName = GetProvider(date == null ? 0 : date.AssessorsProviderType, service),
            //                //LawyeProviderName = GetProvider(date == null ? 0 : date.LawyersProviderType, service),
            //                //ValueProviderName = GetProvider(date == null ? 0 : date.ValuersProviderType, service),
            //                //RepairProviderName = GetProvider(date == null ? 0 : date.RepairersProviderType, service),
            //                //TownlyName = GetProvider(date == null ? 0 : date.TownlyProviderType, service),
            //                //MeadicalName = GetProvider(date == null ? 0 : date.MedicalProviderType, service),



            //                ClaimStatus = Convert.ToString(Claimstatusdata.Status),
            //                TotalProviderFees = date == null ? 0 : date.TotalProviderFees,
            //                Id = _claimRegistration.Id,
            //                VehicleDetailId = _claimRegistration.VehicleDetailId,
            //                ClaimantName = _claimRegistration.ClaimantName,
            //                RegistrationNo = _claimRegistration.RegistrationNo,
            //                ClaimValue = GetClaimValue(_claimRegistration.ClaimNumber, Adjustments),
            //                MakeId = _make == null ? "" : _make.MakeDescription,
            //                ModelId = mode == null ? "" :  mode.ModelDescription
            //                //ThirdPartyMakeId = m == null ? "" : m.MakeDescription,
            //                //ThirdPartyModelId = mod == null ? "" : mod.ModelDescription,
            //                //ModifyOn = date = date.ModifiedOn


            //            }).ToList();

            var list = new List<ClaimRegistrationModel>();
            try
            {

                string query = "select  ClaimRegistration.Id, ClaimStatus.[Status], PolicyNumber,PaymentDetails, ClaimNumber, PlaceOfLoss, DescriptionOfLoss, ";
                query += " EstimatedValueOfLoss, ThirdPartyDamageValue, ClaimStatus, VehicleDetailId, ClaimantName, ";
                query += "RegistrationNo, MakeDescription, ModelDescription, TotalProviderFees, ClaimRegistration.CreatedOn from ClaimRegistration join ClaimStatus";
                query += " on ClaimRegistration.ClaimStatus = ClaimStatus.Id ";
                query += " left join VehicleMake on ClaimRegistration.MakeId = VehicleMake.MakeCode";
                query += " left join VehicleModel on ClaimRegistration.ModelId = VehicleModel.ModelCode";



                list = InsuranceContext.Query(query).Select(c => new ClaimRegistrationModel
                {
                    Id = c.Id,
                    PolicyNumber = c.PolicyNumber,
                    PaymentDetails = c.PaymentDetails,
                    ClaimNumber = c.ClaimNumber,
                    PlaceOfLoss = c.PlaceOfLoss,
                    DescriptionOfLoss = c.DescriptionOfLoss,
                    EstimatedValueOfLoss = c.EstimatedValueOfLoss,
                    ThirdPartyDamageValue = c.ThirdPartyDamageValue,
                    ClaimStatus = c.Status,
                    VehicleDetailId = c.VehicleDetailId,
                    ClaimantName = c.ClaimantName,
                    RegistrationNo = c.RegistrationNo,
                    MakeDescription = c.MakeDescription,
                    ModelDescription = c.ModelDescription,
                    ServiceProviderList = GetServiceProvider(c.Id),
                    TotalProviderFees = c.TotalProviderFees,
                    CreatedOn = c.CreatedOn
                }).ToList();

            }
            catch (Exception ex)
            {

            }


            return View(list.OrderByDescending(x => x.CreatedOn));
        }

        public List<ServiceProviderModel> GetServiceProvider(int claimRegistrationId)
        {

            var query = "select ClaimRegistrationId, ProviderType,ServiceProviderName, ServiceProviderFee  from ClaimRegistrationProviderDetial ";
            query += " join ServiceProviderType on ClaimRegistrationProviderDetial.ServiceProviderTypeId = ServiceProviderType.Id ";
            query += " join ServiceProvider on ClaimRegistrationProviderDetial.ServiceProviderId = ServiceProvider.Id where ClaimRegistrationId =" + claimRegistrationId + "and IsSaved=1";

            List<ServiceProviderModel> list = InsuranceContext.Query(query).Select(c => new ServiceProviderModel
            {
                ServiceProviderType = c.ProviderType,
                ServiceProviderName = c.ServiceProviderName,
                ServiceProviderFees = c.ServiceProviderFee,
                ClaimRegistrationId = c.ClaimRegistrationId
            }).ToList();



            return list;
        }



        public string GetClaimValue(long RClaimNumber, List<ClaimAdjustment> AdjustmentList)
        {
            string ClaimNumber = "";
            if (RClaimNumber != null && RClaimNumber != 0)
            {
                var details = AdjustmentList.FirstOrDefault(c => c.ClaimNumber == Convert.ToInt16(RClaimNumber));

                if (details != null)
                {
                    ClaimNumber = Convert.ToString(details.ClaimNumber);

                }
            }
            return ClaimNumber;
        }

        public string GetProvider(int? providerId, List<ServiceProvider> serviceList)
        {
            string provideName = "";

            if (providerId != null && providerId != 0)
            {
                var details = serviceList.FirstOrDefault(c => c.Id == Convert.ToInt16(providerId));

                if (details != null)
                {
                    provideName = details.ServiceProviderName;
                }
            }
            return provideName;
        }
        public ActionResult UploadFile()
        {
            if (Request.Files.Count > 0)
            {
                try
                {

                    var PolicyNumber = System.Web.HttpContext.Current.Request.Params["PolicyNumber"];
                    var ClaimNumber = System.Web.HttpContext.Current.Request.Params["ClaimNumber"];
                    var servicename = System.Web.HttpContext.Current.Request.Params["ServiceProvide"];
                    var serviceprovidername = System.Web.HttpContext.Current.Request.Params["ServiceProviderName"];

                    var Title = System.Web.HttpContext.Current.Request.Params["Title"];
                    var Description = System.Web.HttpContext.Current.Request.Params["Description"];

                    //Get Guid 

                    Guid id = Guid.NewGuid();

                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = id + "." + testfiles[testfiles.Length - 1].Split('.')[1];
                        }
                        else
                        {
                            fname = id + "." + file.FileName.Split('.')[1];
                        }


                        //if folder exist : folder name : customer id eg 1,2,3 etc
                        string policyfolderpath = @Server.MapPath("~/ClaimDocument/" + PolicyNumber + "/");
                        string Claimfolderpath = @Server.MapPath("~/ClaimDocument/" + PolicyNumber + "/" + ClaimNumber + "/");

                        if (!Directory.Exists(policyfolderpath))
                        {
                            Directory.CreateDirectory(policyfolderpath);
                            Directory.CreateDirectory(Claimfolderpath);
                        }
                        else
                        {
                            if (!Directory.Exists(Claimfolderpath))
                            {
                                Directory.CreateDirectory(policyfolderpath);
                                Directory.CreateDirectory(Claimfolderpath);
                            }
                        }

                        fname = "/ClaimDocument/" + PolicyNumber + "/" + ClaimNumber + "/" + fname;
                        file.SaveAs(Server.MapPath(fname));

                        ClaimDocument doc = new ClaimDocument();
                        doc.PolicyNumber = PolicyNumber;
                        doc.Title = Title;
                        doc.Description = Description;
                        doc.CreatedBy = Convert.ToInt32(ClaimNumber);
                        doc.CreatedOn = DateTime.Now;
                        doc.FilePath = fname;
                        doc.ClaimNumber = Convert.ToInt32(ClaimNumber);
                        doc.ServiceProvider = Convert.ToInt32(servicename);
                        doc.ServiceProviderName = Convert.ToInt32(serviceprovidername);
                        InsuranceContext.ClaimDocuments.Insert(doc);
                    }
                    // Returns message that successfully uploaded  
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            else
            {
                return Json("No files selected.");
            }

        }

        [HttpPost]
        public JsonResult GetUplodedFiles()
        {
            var PolicyNumber = System.Web.HttpContext.Current.Request.Params["PolicyNumber"];
            var ClaimNumber = System.Web.HttpContext.Current.Request.Params["ClaimNumber"];

            //string[] filePaths = Directory.GetFiles(Server.MapPath("~/Documents/" + CustomerId + "/" + PolicyNumber + "/" + vehicleId + "/"));

         
            var FileList = InsuranceContext.ClaimDocuments.All(where: $"PolicyNumber='{PolicyNumber}' and ClaimNumber='{ClaimNumber}'").ToList();
            var list = new List<InsuranceClaim.Models.ClaimDocumentModel>();
            foreach (var item in FileList)
            {
                var service = InsuranceContext.ServiceProviders.Single(where: $"Id = {item.ServiceProviderName}");
                var obj = new InsuranceClaim.Models.ClaimDocumentModel();
                obj.Title = item.Title;
                obj.Description = item.Description;
                obj.FilePath = item.FilePath;
                obj.Id = item.Id;
                obj.ServiceProviderName = service.ServiceProviderName;

                list.Add(obj);
            }

            return Json(list.OrderByDescending(x => x.Id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteDocument()
        {
            try
            {
                var docid = System.Web.HttpContext.Current.Request.Params["docid"];

                var document = InsuranceContext.ClaimDocuments.Single(Convert.ToInt32(docid));

                string filelocation = document.FilePath;

                if (System.IO.File.Exists(Server.MapPath(filelocation)))
                {
                    System.IO.File.Delete(Server.MapPath(filelocation));
                }

                InsuranceContext.ClaimDocuments.Delete(document);

                return Json(true, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                return Json(false, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost]
        public JsonResult getServiceProviderName(int? id)
        {
            JsonResult jsonResult = new JsonResult();

            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            var Providertype = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();
            if (id == 1)
            {
                jsonResult.Data = Providertype.Where(w => w.ServiceProviderType == 1).Select(y => new { y.Id, y.ServiceProviderName }).ToList();
            }
            if (id == 2)
            {
                jsonResult.Data = Providertype.Where(w => w.ServiceProviderType == 2).Select(y => new { y.Id, y.ServiceProviderName }).ToList();
            }
            if (id == 3)
            {
                jsonResult.Data = Providertype.Where(w => w.ServiceProviderType == 3).Select(y => new { y.Id, y.ServiceProviderName }).ToList();
            }
            if (id == 4)
            {
                jsonResult.Data = Providertype.Where(w => w.ServiceProviderType == 4).Select(y => new { y.Id, y.ServiceProviderName }).ToList();
            }
            return jsonResult;
        }
        public ActionResult ClaimSetting()
        {

            var eSettingValueTypedata = from eSettingValueType e in Enum.GetValues(typeof(eSettingValueType))
                                        select new
                                        {
                                            ID = (int)e,
                                            Name = e.ToString()
                                        };

            ViewBag.eSettingValueTypes = new SelectList(eSettingValueTypedata, "ID", "Name");

            var eSettingValuedata = from eVehicleType e in Enum.GetValues(typeof(eVehicleType))
                                    select new
                                    {
                                        ID = (int)e,
                                        Name = e.ToString()
                                    };

            ViewBag.eSettingValueType = new SelectList(eSettingValuedata, "ID", "Name");


            return View();
        }
        public ActionResult SaveSetting(ClaimSettingModel model)
        {

            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {

                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                string userid = "";
                if (userLoggedin)
                {
                    userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                    var dbModel = Mapper.Map<ClaimSettingModel, ClaimSetting>(model);
                    dbModel.CreatedBy = customer.Id;
                    dbModel.CreatedOn = DateTime.Now;
                    dbModel.IsActive = true;
                    InsuranceContext.ClaimSettings.Insert(dbModel);
                    return RedirectToAction("ClaimSettingList");
                }
            }
            return View("ClaimSetting");
        }
        public ActionResult ClaimSettingList()
        {
            var ClaimList = InsuranceContext.ClaimSettings.All(where: $"IsActive = 'True' or IsActive is null").OrderByDescending(x => x.Id).ToList();


            return View(ClaimList);
        }


        [HttpGet]
        public JsonResult GetAutoSuggestions()
        {

            List<ClaimNotificationModel> objList = new List<ClaimNotificationModel>();
            /*var Policylist = InsuranceContext.PolicyDetails.All().ToList();*/   ////  get data from database 
            var NotificationList = new List<ClaimNotificationModel>(); /// create list 

            //var result = (from Vehicle in InsuranceContext.VehicleDetails.All().ToList()
            //              join Policylist in InsuranceContext.PolicyDetails.All().ToList()
            //              on Vehicle.PolicyId equals Policylist.Id
            //              join customer in InsuranceContext.Customers.All()
            //              on Vehicle.CustomerId equals customer.Id
            //              where Vehicle.IsActive == true && Vehicle.CoverEndDate<=DateTime.Now
            //              select new ClaimNotificationModel
            //              {
            //                  PolicyNumber = Policylist.PolicyNumber,
                              
            //                  VRNNumber = Vehicle.RegistrationNo,
            //                  VehicleId = Vehicle.Id,
            //                  PolicyId = Vehicle.PolicyId,
            //                  CustomerName = customer.FirstName + " " + customer.LastName

            //              }).ToList().OrderByDescending(x => x.PolicyId).Take(500);

            var query = "select  top 500  Customer.FirstName + Customer.LastName as CustomerName, PolicyDetail.PolicyNumber, ";
            query += " VehicleDetail.RegistrationNo , VehicleDetail.RenewPolicyNumber, VehicleDetail.Id as VehicleId, PolicyDetail.Id as PolicyId, ";
            query += " VehicleDetail.RenewalDate from VehicleDetail join PolicyDetail on VehicleDetail.PolicyId = PolicyDetail.Id ";
            query += " join Customer on VehicleDetail.CustomerId = Customer.Id where VehicleDetail.RenewalDate >= getdate() order by RenewalDate desc";

            var list = InsuranceContext.Query(query).Select(x => new ClaimNotificationModel()
            {
                PolicyNumber = x.PolicyNumber,
                VRNNumber = x.RegistrationNo,
                VehicleId = x.VehicleId,
                PolicyId = x.PolicyId,
                CustomerName = x.CustomerName,
                RenewPolicyNumber = x.RenewPolicyNumber
            }).ToList();


            var policyList = new List<ClaimNotificationModel>();

            foreach(var item in list)
            {
                ClaimNotificationModel model = new ClaimNotificationModel();
                model = item;

                if (item.RenewPolicyNumber == null)
                    model.PolicyNumber = item.PolicyNumber;
                else if (item.RenewPolicyNumber.Contains("-1"))
                    model.PolicyNumber = item.PolicyNumber;
                else
                    model.PolicyNumber = item.RenewPolicyNumber;

                policyList.Add(model);
            }



            return Json(policyList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCustomername(string txtvalue)
        {
            ClaimNotificationModel model = new ClaimNotificationModel();
            try
            {

                var customerName = "";
                var policyNumber = "";
                var vrn = "";
                var policyAndRegistrationNumber = txtvalue; //Policy Number,VRN Number,Customer Name 
                var policyAndRegistrationNumberArray = policyAndRegistrationNumber.Split(',');

                var detail = new PolicyDetail();
                if (policyAndRegistrationNumberArray.Length > 0)
                {
                    policyNumber = policyAndRegistrationNumberArray[0]; //Policy Number
                    detail = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber='{policyNumber}'");
                }

                if(detail==null)
                {
                    // to get renew policy details
                    var vehicle = InsuranceContext.VehicleDetails.Single(where: $"RenewPolicyNumber = '{policyNumber}'");
                    detail = InsuranceContext.PolicyDetails.Single(where: $"Id='{vehicle.PolicyId}'");

                    if(detail!=null)
                    {
                        var customerdetail = InsuranceContext.Customers.Single(where: $"Id='{detail.CustomerId}'");
                        customerName = customerdetail.FirstName + " " + customerdetail.LastName;

                        model.CustomerName = customerName;
                        model.RegistrationNo = vrn;
                        model.PolicyNumber = policyNumber;

                        model.CoverStartDate = Convert.ToDateTime(vehicle.CoverStartDate);
                        model.CoverEndDate = Convert.ToDateTime(vehicle.CoverEndDate);
                        //model.CoverStartDate =Convert.ToDateTime(vehicle.CoverStartDate).ToShortDateString();
                        //model.CoverEndDate =Convert.ToDateTime(vehicle.CoverEndDate).ToShortDateString();

                        return Json(model, JsonRequestBehavior.AllowGet);
                    }
                
                }






                if (policyAndRegistrationNumberArray.Length > 1)
                {
                    policyNumber = policyAndRegistrationNumberArray[0]; //Policy Number
                    vrn = policyAndRegistrationNumberArray[1];//VRN Number
                }
                else
                {
                    policyNumber = policyAndRegistrationNumberArray[0];

                    if (detail != null)
                    {
                        var vehicle = InsuranceContext.VehicleDetails.Single(where: $"PolicyId = '{detail.Id}'");

                        vrn = vehicle.RegistrationNo == null ? "" : vehicle.RegistrationNo;
                    }
                }



                if (detail != null)
                {
                    var vehicle = InsuranceContext.VehicleDetails.Single(where: $"RegistrationNo = '{vrn}'");
                    var customerdetail = InsuranceContext.Customers.Single(where: $"Id='{detail.CustomerId}'");
                    customerName = customerdetail.FirstName + " " + customerdetail.LastName;

                    model.CustomerName = customerName;
                    model.RegistrationNo = vrn;
                    model.PolicyNumber = policyNumber;

                    model.CoverStartDate = Convert.ToDateTime(vehicle.CoverStartDate);
                    model.CoverEndDate = Convert.ToDateTime(vehicle.CoverEndDate);
                    //model.CoverStartDate =Convert.ToDateTime(vehicle.CoverStartDate).ToShortDateString();
                    //model.CoverEndDate =Convert.ToDateTime(vehicle.CoverEndDate).ToShortDateString();

                    return Json(model, JsonRequestBehavior.AllowGet);
                }


            }
            catch (Exception ex)
            {

            }


            return Json(model, JsonRequestBehavior.AllowGet);

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
        public JsonResult GetPolicydate(string Datelose, string StartDate, string EndDate)
        {

            bool result = false;

            if (string.IsNullOrEmpty(Datelose) && string.IsNullOrEmpty(StartDate) && string.IsNullOrEmpty(StartDate))
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }

            if (Convert.ToDateTime(Datelose) >= (Convert.ToDateTime(StartDate)))
            {
                if (Convert.ToDateTime(EndDate) >= Convert.ToDateTime(Datelose))
                {
                    result = true;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                else
                    result = false;
            }
            else
            {
                return Json(result, JsonRequestBehavior.AllowGet);

            }

            return Json(false, JsonRequestBehavior.AllowGet);
        }




        public ActionResult EditRegisterClaim(int id)
        {

            //string PolicyNumber;
            //int ClaimRegisterid;
            //int VehicleDetailId;
            //int ClaimNumer;

            var customerRegistration = InsuranceContext.ClaimRegistrations.Single(where: $"Id = '{id}'");

            try
            {

                ViewBag.ServiceProviderTypes = InsuranceContext.ServiceProviderTypes.All().ToList();

                var Providertype = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();

                //ViewBag.AssessorsType = (from res in Providertype.Where(w => w.ServiceProviderType == 1)
                //                         select new ServiceProvider { ServiceProviderName = res.ServiceProviderName + ", " + res.ServiceProviderFees, Id = res.Id }).ToList();

                //ViewBag.ValuersType = (from prov in Providertype.Where(w => w.ServiceProviderType == 2)
                //                       select new ServiceProvider { ServiceProviderName = prov.ServiceProviderName + ", " + prov.ServiceProviderFees, Id = prov.Id }).ToList();

                //ViewBag.LawyersType = (from lawe in Providertype.Where(w => w.ServiceProviderType == 3)
                //                       select new ServiceProvider { ServiceProviderName = lawe.ServiceProviderName + ", " + lawe.ServiceProviderFees, Id = lawe.Id }).ToList();

                //ViewBag.RepairersType = (from repa in Providertype.Where(w => w.ServiceProviderType == 4)
                //                         select new ServiceProvider { ServiceProviderName = repa.ServiceProviderName + ", " + repa.ServiceProviderFees, Id = repa.Id }).ToList();
                //ViewBag.TownlyType = (from repa in Providertype.Where(w => w.ServiceProviderType == 5)
                //                      select new ServiceProvider { ServiceProviderName = repa.ServiceProviderName + ", " + repa.ServiceProviderFees, Id = repa.Id }).ToList();
                //ViewBag.MedicalType = (from repa in Providertype.Where(w => w.ServiceProviderType == 6)
                //                       select new ServiceProvider { ServiceProviderName = repa.ServiceProviderName + ", " + repa.ServiceProviderFees, Id = repa.Id }).ToList();





                ViewBag.ClaimStatus = InsuranceContext.ClaimStatuss.All().ToList();
                var service = new VehicleService();
                if (customerRegistration.PolicyNumber != "")
                {
                    var PolicyDetail = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber = '{customerRegistration.PolicyNumber}'");

                    var ClaimDetail = InsuranceContext.ClaimNotifications.Single(where: $"VehicleId = '{customerRegistration.VehicleDetailId}' and PolicyId=" + PolicyDetail.Id);


                    // var RegisterDetail = InsuranceContext.ClaimRegistrations.All().SingleOrDefault(p => p.Id == ClaimRegisterid);

                    var claimregistrationdetail = InsuranceContext.ClaimRegistrations.Single(where: $"Id = '{customerRegistration.Id}'");

                    var ClaimProvider = InsuranceContext.ClaimDetailsProviders.Single(where: $"ClaimNumber = '{claimregistrationdetail.ClaimNumber}'");

                    var CustomerId = PolicyDetail.CustomerId;

                    var VehicleDetail = InsuranceContext.VehicleDetails.All(where: $"Id = '{customerRegistration.VehicleDetailId}' and PolicyId=" + PolicyDetail.Id).ToList();


                    RegisterClaimViewModel VehicleDetailVM = new RegisterClaimViewModel();


                    List<RiskViewModel> VehicleData = new List<RiskViewModel>();
                    List<ChecklistModel> ChecklistModel = new List<ChecklistModel>();

                    var Checklist = InsuranceContext.Checklists.All().ToList();


                    ChecklistModel = Checklist.Select(p => new ChecklistModel()
                    {
                        Id = p.Id,
                        ChecklistDetail = p.ChecklistDetail,
                        IsChecked = false,
                    }).ToList();


                    List<RegisterClaimViewModel> VehicleDetailList = new List<RegisterClaimViewModel>();


                    string[] getCheckList = null;

                    if (claimregistrationdetail.Checklist != null)
                    {
                        getCheckList = claimregistrationdetail.Checklist.Split(',');
                    }

                    for (var i = 0; i < ChecklistModel.Count(); i++)
                    {
                        RegisterClaimViewModel VehicleVM = new RegisterClaimViewModel();

                        var chekExist = false;

                        if (getCheckList != null)
                        {
                            chekExist = getCheckList.Contains(ChecklistModel[i].Id.ToString());
                        }


                        if (chekExist)
                        {
                            VehicleVM.isChecked = true;
                            VehicleVM.CheckBoxName = ChecklistModel[i].ChecklistDetail;
                            VehicleVM.checkId = ChecklistModel[i].Id;

                        }
                        else
                        {
                            VehicleVM.isChecked = false;
                            VehicleVM.CheckBoxName = ChecklistModel[i].ChecklistDetail;
                            VehicleVM.checkId = ChecklistModel[i].Id;

                        }

                        VehicleDetailList.Add(VehicleVM);
                    }


                    //get only Vehicle Detail
                    VehicleData = VehicleDetail.Select(p => new RiskViewModel()
                    {
                        Make = InsuranceContext.VehicleMakes.All().Where(q => q.MakeCode == p.MakeId).FirstOrDefault().MakeDescription,
                        Model = InsuranceContext.VehicleModels.All().Where(q => q.ModelCode == p.ModelId).FirstOrDefault().ModelDescription,
                        paymentTerm = InsuranceContext.PaymentTerms.All().Where(q => q.Id == p.PaymentTermId).FirstOrDefault().Name,
                        Product = InsuranceContext.Products.All().Where(q => q.Id == p.ProductId).FirstOrDefault().ProductName,
                        VehUsage = InsuranceContext.VehicleUsages.All().Where(q => q.Id == p.VehicleUsage).FirstOrDefault().VehUsage,
                        CoverType = InsuranceContext.CoverTypes.All().Where(q => q.Id == p.CoverTypeId).FirstOrDefault().Name,
                        VehicleYear = p.VehicleYear,
                        CubicCapacity = p.CubicCapacity,
                        EngineNumber = p.EngineNumber,
                        ChasisNumber = p.ChasisNumber,
                        AddThirdPartyAmount = p.AddThirdPartyAmount,
                        Excess = p.Excess,
                        ExcessType = Convert.ToString(p.ExcessType),
                        Premium = p.Premium,
                        StampDuty = p.StampDuty,
                        ZTSCLevy = p.ZTSCLevy,
                        Discount = p.Discount,

                        VehicleLicenceFee = p.VehicleLicenceFee,
                        Addthirdparty = p.Addthirdparty == true ? "yes" : "No",
                        IncludeRadioLicenseCost = p.IncludeRadioLicenseCost == true ? "yes" : "No",
                        IsLicenseDiskNeeded = p.IsLicenseDiskNeeded == true ? "yes" : "No",
                        FirstName = InsuranceContext.Customers.All().Where(q => q.Id == p.CustomerId).FirstOrDefault().FirstName,
                        LastName = InsuranceContext.Customers.All().Where(q => q.Id == p.CustomerId).FirstOrDefault().LastName,
                        RegisterNumber = p.RegistrationNo,
                        CoverStartDate = Convert.ToString(p.CoverStartDate.Value.ToShortDateString()),
                        CoverEndDate = Convert.ToString(p.CoverEndDate.Value.ToShortDateString()),
                        SumInsured = p.SumInsured,
                        RadioLicenseCost = p.RadioLicenseCost,
                        PassengerAccidentCover = p.PassengerAccidentCover == true ? "yes" : "No",
                        ExcessBuyBack = p.ExcessBuyBack == true ? "yes" : "No",
                        RoadsideAssistance = p.RoadsideAssistance == true ? "yes" : "No",
                        MedicalExpenses = p.MedicalExpenses == true ? "yes" : "No"
                    }).ToList();

                    VehicleDetailVM = new RegisterClaimViewModel()
                    {
                        PolicyNumber = PolicyDetail.PolicyNumber,
                        DateOfLoss = claimregistrationdetail.DateOfLoss.ToString(),// dispaly only date
                        PlaceOfLoss = claimregistrationdetail == null ? null : claimregistrationdetail.PlaceOfLoss,
                        DescriptionOfLoss = claimregistrationdetail == null ? null : claimregistrationdetail.DescriptionOfLoss,
                        EstimatedValueOfLoss = claimregistrationdetail.EstimatedValueOfLoss,
                        DateOfNotifications = claimregistrationdetail.CreatedOn.ToString(),// dispaly only date
                        RiskViewModel = VehicleData,
                        chklistDetail = VehicleDetailList,
                        checklistvalue = claimregistrationdetail.Checklist,
                        ThirdPartyDamageValue = claimregistrationdetail.ThirdPartyDamageValue,
                        FirstName = InsuranceContext.Customers.All().Where(q => q.Id == CustomerId).FirstOrDefault().FirstName,
                        LastName = InsuranceContext.Customers.All().Where(q => q.Id == CustomerId).FirstOrDefault().LastName,
                        Status = Convert.ToString(claimregistrationdetail.ClaimStatus),



                    };
                    //var ServiceProvidersList = InsuranceContext.ServiceProviders.All(where: $"IsDeleted = 'True' or IsDeleted is null ").ToList();
                    //var claimdetail = InsuranceContext.ClaimDetailsProviders.Single(where: $"ClaimNumber = '{ClaimNumer}'");
                    //if (claimdetail != null && claimdetail.Count() > 0)
                    //{
                    //    ViewBag.AssessorsType = ServiceProvidersList.Where(w => w.ServiceProviderType == 1).ToList();
                    //    ViewBag.ValuersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 2).ToList();
                    //    ViewBag.LawyersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 3).ToList();
                    //    ViewBag.RepairersType = ServiceProvidersList.Where(w => w.ServiceProviderType == 4).ToList();


                    //    VehicleDetailVM.ValuersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == claimdetail.ValuersProviderType).Id;
                    //    VehicleDetailVM.LawyersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == claimdetail.LawyersProviderType).Id;
                    //    VehicleDetailVM.RepairersProviderType = ServiceProvidersList.FirstOrDefault(c => c.Id == claimdetail.RepairersProviderType).Id;

                    //}

                    VehicleDetailVM.ClaimId = customerRegistration.Id;
                    if (ClaimProvider != null && ClaimProvider.Count() > 0)
                    {

                        VehicleDetailVM.AssessorsProviderType = ClaimProvider.AssessorsProviderType;
                        VehicleDetailVM.ValuersProviderType = ClaimProvider.ValuersProviderType;
                        VehicleDetailVM.RepairersProviderType = ClaimProvider.RepairersProviderType;
                        VehicleDetailVM.TotalProviderFees = ClaimProvider.TotalProviderFees;
                        VehicleDetailVM.LawyersProviderType = ClaimProvider.LawyersProviderType;
                        VehicleDetailVM.TownlyProviderType = ClaimProvider.TownlyProviderType;
                        VehicleDetailVM.MedicalProviderType = ClaimProvider.MedicalProviderType;
                        //Fees
                        VehicleDetailVM.AssessorsProviderFees = ClaimProvider.AssessorsProviderFees;
                        VehicleDetailVM.ValuersProviderFees = ClaimProvider.ValuersProviderFees;
                        VehicleDetailVM.RepairersProviderFees = ClaimProvider.RepairersProviderFees;
                        VehicleDetailVM.LawyersProviderFees = ClaimProvider.LawyersProviderFees;
                        VehicleDetailVM.TownlyProviderFees = ClaimProvider.TownlyProviderFees;
                        VehicleDetailVM.MedicalProviderFees = ClaimProvider.MedicalProviderFees;


                    }
                    VehicleDetailVM.ThirdPartyInvolvement = claimregistrationdetail.ThirdPartyInvolvement;
                    VehicleDetailVM.Claimnumber = claimregistrationdetail.ClaimNumber;
                    VehicleDetailVM.Claimsatisfaction = claimregistrationdetail.Claimsatisfaction;
                    VehicleDetailVM.Id = claimregistrationdetail.Id;

                    var query = "select ServiceProvider.Id, ServiceProvider.ServiceProviderName , ServiceProviderType.ProviderType, ServiceProviderType.Id as ProviderTypeId  from ServiceProvider join ServiceProviderType ";
                    query += " on ServiceProvider.ServiceProviderType = ServiceProviderType.Id";


                    VehicleDetailVM.ProviderList = InsuranceContext.Query(query).Select(c => new ServiceProviderModel { Id = c.Id, ServiceProviderName = c.ServiceProviderName, ServiceProviderType = c.ProviderType, ProviderTypeId = c.ProviderTypeId }).ToList();


                    var claimRegistrationProviders = InsuranceContext.ClaimRegistrationProviderDetials.All(where: "ClaimRegistrationId=" + customerRegistration.Id + " and IsSaved=0");

                    foreach (var item in claimRegistrationProviders)
                    {
                        InsuranceContext.ClaimRegistrationProviderDetials.Delete(item);
                    }


                    VehicleDetailVM.ClaimRegistrationProviderList = InsuranceContext.ClaimRegistrationProviderDetials.All(where: "ClaimRegistrationId=" + customerRegistration.Id + " and IsSaved=1").ToList();

                    VehicleDetailVM.RegistrationDocumentList = InsuranceContext.RegistrationDocuments.All(where: "ClaimRegistrationId=" + id).ToList();





                    return View(VehicleDetailVM);
                }
                return View();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost]
        public JsonResult GetProviderFee(string ProviderId, string claimRegistratoinId, string providerTypeId)
        {
            decimal fee = 0;
            var providerDetils = InsuranceContext.ServiceProviders.Single(where: $"Id='{ProviderId}'");


            if (providerDetils != null)
            {
                fee = providerDetils.ServiceProviderFees;
            }

            var ClaimRegistrationProviderDetial = InsuranceContext.ClaimRegistrationProviderDetials.Single(where: $"ClaimRegistrationId='{claimRegistratoinId}' and ServiceProviderTypeId= " + providerTypeId);

            if (ClaimRegistrationProviderDetial == null)
            {
                ClaimRegistrationProviderDetial detials = new ClaimRegistrationProviderDetial();
                detials.ClaimRegistrationId = Convert.ToInt32(claimRegistratoinId);
                detials.ServiceProviderId = ProviderId == null ? 0 : Convert.ToInt32(ProviderId);
                detials.ServiceProviderTypeId = Convert.ToInt32(providerTypeId);
                detials.ServiceProviderFee = fee;
                detials.CreatedOn = DateTime.Now;
                detials.IsActive = true;
                InsuranceContext.ClaimRegistrationProviderDetials.Insert(detials);
            }
            else
            {
                ClaimRegistrationProviderDetial.ClaimRegistrationId = Convert.ToInt32(claimRegistratoinId);
                ClaimRegistrationProviderDetial.ServiceProviderId = ProviderId == null ? 0 : Convert.ToInt32(ProviderId);
                ClaimRegistrationProviderDetial.ServiceProviderTypeId = Convert.ToInt32(providerTypeId);
                ClaimRegistrationProviderDetial.ServiceProviderFee = fee;
                ClaimRegistrationProviderDetial.CreatedOn = DateTime.Now;
                InsuranceContext.ClaimRegistrationProviderDetials.Update(ClaimRegistrationProviderDetial);
            }

            return Json(fee, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult DeleteClaimRegisterServiceProvider(string claimRegistratoinId, string providerTypeId)
        {
            var ClaimRegistrationProviderDetial = InsuranceContext.ClaimRegistrationProviderDetials.Single(where: $"ClaimRegistrationId='{claimRegistratoinId}' and ServiceProviderTypeId= " + providerTypeId);

            if (ClaimRegistrationProviderDetial != null)
            {
                InsuranceContext.ClaimRegistrationProviderDetials.Delete(ClaimRegistrationProviderDetial);
            }

            return Json("");
        }


        [HttpPost]
        public JsonResult UpdateProviderFee(string claimRegistratoinId, string providerTypeId, string fee)
        {

            var ClaimRegistrationProviderDetial = InsuranceContext.ClaimRegistrationProviderDetials.Single(where: $"ClaimRegistrationId='{claimRegistratoinId}' and ServiceProviderTypeId= " + providerTypeId);

            if (ClaimRegistrationProviderDetial != null)
            {
                ClaimRegistrationProviderDetial.ServiceProviderFee = fee == "" ? 0 : Convert.ToDecimal(fee);
                InsuranceContext.ClaimRegistrationProviderDetials.Update(ClaimRegistrationProviderDetial);
            }



            return Json("");
        }
        public bool GetcheckdValue(string ChecklistDetail)
        {
            bool IsChecked = false;
            if (ChecklistDetail == "Driverslicence")
            {
                IsChecked = true;
            }
            else if (ChecklistDetail == "ClaimantCopyOfID")
            {
                IsChecked = true;
            }
            else if (ChecklistDetail == "PoliceReport")
            {
                IsChecked = true;
            }

            else if (ChecklistDetail == "ClaimForm")
            {
                IsChecked = true;
            }

            return IsChecked;
        }

        public ActionResult updateRegisterDetail(RegisterClaimViewModel model, HttpPostedFileBase[] files)
        {

            //RemoveValidation();

            //if (ModelState.IsValid)
            //{

            SaveCheckListDocument(files, model.Id);


            var caimNumber = model.Claimnumber;
            var claimStatis = model.Claimsatisfaction;
            var status = model.Status;
            //  var names = String.Join(",", model.chklist.Where(p => p.IsChecked).Select(p => p.Id));
            var names = String.Join(",", model.chklistDetail.Where(p => p.isChecked).Select(p => p.checkId));
            var updateRecord = InsuranceContext.ClaimRegistrations.Single(model.ClaimId);
            updateRecord.Checklist = names;
            if (model.PlaceOfLoss != "")
            {
                updateRecord.PlaceOfLoss = model.PlaceOfLoss;
            }

            if (model.DescriptionOfLoss != "")
            {
                updateRecord.DescriptionOfLoss = model.DescriptionOfLoss;
            }
            if (model.EstimatedValueOfLoss > 0)
            {
                updateRecord.EstimatedValueOfLoss = model.EstimatedValueOfLoss;
            }
            if (model.RejectionStatus != "")
            {
                updateRecord.RejectionStatus = model.RejectionStatus;
            }
            if (model.Status != "")
            {
                updateRecord.ClaimStatus = Convert.ToInt32(model.Status);
            }
            updateRecord.ThirdPartyDamageValue = model.ThirdPartyDamageValue;
            updateRecord.Claimsatisfaction = claimStatis;
            updateRecord.ModifyOn = DateTime.Now;

            var ClaimRegistrationProviderDetials = InsuranceContext.ClaimRegistrationProviderDetials.All(where: "ClaimRegistrationId=" + model.Id).ToList();
            foreach (var item in ClaimRegistrationProviderDetials)
            {
                item.IsSaved = true;
                InsuranceContext.ClaimRegistrationProviderDetials.Update(item);
            }


            if (ClaimRegistrationProviderDetials.Count() > 0)
            {
                updateRecord.TotalProviderFees = ClaimRegistrationProviderDetials.Select(c => c.ServiceProviderFee).Sum();
            }

            InsuranceContext.ClaimRegistrations.Update(updateRecord);

            return RedirectToAction("ClaimRegistrationList");
            /////Insert


            //string customId = "";
            //bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            //var claimdata = InsuranceContext.ClaimDetailsProviders.Single(where: $"PolicyNumber = '{model.PolicyNumber}' and ClaimNumber = '{model.Claimnumber}'");
            //if (claimdata == null || claimdata.Count() == 0)
            //{
            //    if (userLoggedin)
            //    {
            //        var _userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
            //        var customer = InsuranceContext.Customers.Single(where: $"UserId = '{_userid}'");
            //        customId = Convert.ToString(customer.Id);

            //        ClaimDetailsProvider obj = new ClaimDetailsProvider();
            //        obj.LawyersProviderType = model.LawyersProviderType;
            //        obj.AssessorsProviderType = model.AssessorsProviderType;
            //        obj.RepairersProviderType = model.RepairersProviderType;
            //        obj.ValuersProviderType = model.ValuersProviderType;
            //        obj.TotalProviderFees = model.TotalProviderFees;
            //        obj.PolicyNumber = model.PolicyNumber;
            //        obj.ClaimNumber = Convert.ToInt32(model.Claimnumber);
            //        obj.TownlyProviderType = model.TownlyProviderType;
            //        obj.MedicalProviderType = model.MedicalProviderType;
            //        obj.AssessorsProviderFees = model.AssessorsProviderFees;
            //        obj.ValuersProviderFees = model.ValuersProviderFees;
            //        obj.RepairersProviderFees = model.RepairersProviderFees;
            //        obj.LawyersProviderFees = model.LawyersProviderFees;
            //        obj.TownlyProviderFees = model.TownlyProviderFees;
            //        obj.MedicalProviderFees = model.MedicalProviderFees;


            //        obj.CreatedBy = Convert.ToInt32(customId);
            //        obj.CreatedOn = DateTime.Now;
            //        obj.IsActive = true;
            //        InsuranceContext.ClaimDetailsProviders.Insert(obj);
            //    }
            //    return RedirectToAction("ClaimRegistrationList");
            //}

            //else
            //{
            //    if (userLoggedin)
            //    {
            //        var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
            //        var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
            //        customId = Convert.ToString(customer.Id);
            //        claimdata.AssessorsProviderType = model.AssessorsProviderType;
            //        claimdata.ValuersProviderType = model.ValuersProviderType;
            //        claimdata.LawyersProviderType = model.LawyersProviderType;
            //        claimdata.RepairersProviderType = model.RepairersProviderType;
            //        claimdata.TownlyProviderType = model.TownlyProviderType;
            //        claimdata.MedicalProviderType = model.MedicalProviderType;
            //        claimdata.PolicyNumber = model.PolicyNumber;
            //        claimdata.AssessorsProviderFees = model.AssessorsProviderFees;
            //        claimdata.ValuersProviderFees = model.ValuersProviderFees;
            //        claimdata.RepairersProviderFees = model.RepairersProviderFees;
            //        claimdata.LawyersProviderFees = model.LawyersProviderFees;
            //        claimdata.TownlyProviderFees = model.TownlyProviderFees;
            //        claimdata.MedicalProviderFees = model.MedicalProviderFees;
            //        claimdata.TotalProviderFees = model.TotalProviderFees;
            //        claimdata.ClaimNumber = Convert.ToInt32(model.Claimnumber);
            //        claimdata.ModifiedBy = Convert.ToInt32(customId);
            //        claimdata.ModifiedOn = DateTime.Now;
            //        claimdata.IsActive = true;
            //        InsuranceContext.ClaimDetailsProviders.Update(claimdata);
            //    }
            //    return RedirectToAction("ClaimRegistrationList");


            //    return RedirectToAction("RegisterClaim");
            //}

            //public void RemoveValidation()
            //{
            //    ModelState.Remove("PlaceOfLoss");

            //    ModelState.Remove("PlaceOfLoss");
            //    ModelState.Remove("DescriptionOfLoss");
            //    ModelState.Remove("EstimatedValueOfLoss");
            //    ModelState.Remove("ThirdPartyDamageValue");
            //}


        }
    }
}