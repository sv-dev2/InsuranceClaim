using AutoMapper;
using Insurance.Domain;
using Insurance.Domain.Domain;
using Insurance.Service;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class ClaimAdjustmentController : Controller
    {

        decimal _excessAmount = 0;
        // GET: ClaimAdjustment
        public ActionResult Index(int? id)
        {

            //   string PolicyNumber, int? claimnumber
            var otherserviceprovider = 0.00m;
            var Assessorsprovider = 0.00m;
            var Towingprovider = 0.00m;
            var Valuersprovider = 0.00m;
            var Repairersprovider = 0.00m;

            var Lawyersprovider = 0.00m;
            var Medicalprovider = 0.00m;

           

            var ePaymentDetail = from ePayeeBankDetails e in Enum.GetValues(typeof(ePayeeBankDetails))
                                 select new
                                 {
                                     ID = (int)e,
                                     Name = e.ToString()
                                 };


            //for getting repairs providers value

            var query = "select ClaimRegistrationId, ProviderType,ServiceProviderName, ServiceProviderFee  from ClaimRegistrationProviderDetial ";
            query += " join ServiceProviderType on ClaimRegistrationProviderDetial.ServiceProviderTypeId = ServiceProviderType.Id ";
            query += " join ServiceProvider on ClaimRegistrationProviderDetial.ServiceProviderId = ServiceProvider.Id where ClaimRegistrationId =" + id + "and IsSaved=1";

            List<ServiceProviderModel> list = InsuranceContext.Query(query).Select(c => new ServiceProviderModel
            {
                ServiceProviderType = c.ProviderType,
                ServiceProviderName = c.ServiceProviderName,
                ServiceProviderFees = c.ServiceProviderFee,
                ClaimRegistrationId = c.ClaimRegistrationId
            }).ToList();

            //ViewBag.Providerslist = list;


            //for (int i=0; i< list.Count;i++)
            //{
            foreach (var Providers in list)
            {
                if (Providers.ServiceProviderType == "Repairers")
                {
                    Repairersprovider = Providers.ServiceProviderFees;
                }
                if (Providers.ServiceProviderType == "Assessors")
                {
                    Assessorsprovider = Providers.ServiceProviderFees;
                }

                if (Providers.ServiceProviderType == "Towing")
                {
                    Towingprovider = Providers.ServiceProviderFees;
                }

                if (Providers.ServiceProviderType == "Valuers")
                {
                    Valuersprovider = Providers.ServiceProviderFees;
                }
                if (Providers.ServiceProviderType == "Lawyers")
                {
                    Lawyersprovider = Providers.ServiceProviderFees;
                }
                if (Providers.ServiceProviderType == "Medical")
                {
                    Medicalprovider = Providers.ServiceProviderFees;
                }

            }

            otherserviceprovider = Assessorsprovider + Towingprovider + Valuersprovider + Lawyersprovider + Medicalprovider;


            //}





            ViewBag.ePaymentDetailData = new SelectList(ePaymentDetail, "ID", "Name");

            if (id != 0 && id != null)
            {
                var model = new ClaimAdjustmentModel();
                var data = InsuranceContext.ClaimRegistrations.Single(where: $"Id= '{id}'");
                if (data != null && data.Count() > 0)
                {

                    var policy = InsuranceContext.PolicyDetails.Single(where: $"PolicyNumber='{data.PolicyNumber}'");
                    var summery = InsuranceContext.SummaryDetails.Single(where: $"CustomerId = '{policy.CustomerId}'");

                    var custmo = InsuranceContext.Customers.Single(where: $"Id = '{policy.CustomerId}'");
                    model.EstimatedLoss = data.EstimatedValueOfLoss;
                    model.ClaimNumber = Convert.ToInt32(data.ClaimNumber);
                    model.PolicyNumber = data.PolicyNumber;
                    model.FirstName = custmo.FirstName;
                    model.LastName = custmo.LastName;
                    model.TotalSuminsure = Convert.ToDecimal(summery.TotalPremium);
                    model.PolicyholderName = custmo.FirstName + " " + custmo.LastName;
                    model.PayeeName = data.ClaimantName;
                    //model.AmountToPay = data.TotalProviderFees;
                    //model.FinalAmountToPaid = data.TotalProviderFees;
                    //model.FinalAmountToPaid = Repairersprovider+otherserviceprovider;
                    model.FinalAmountToPaid = 0.00m;
                    model.RepairCost = Repairersprovider;
                    model.ServiceProviderCost = otherserviceprovider;
                    model.ClaimRegisterationId = data.Id;
                    return View(model);

                }
            }
            return View();
        }


        [HttpGet]
        public ActionResult ClaimPayment(int id, string successMsg = "")
        {
            ClaimAdjustmentModel model = new ClaimAdjustmentModel();
            model.Id = id;
            var claimAdjustment = InsuranceContext.ClaimAdjustments.Single(where: "Id=" + id);



            TempData["Sucessmessage"] = successMsg;


            if (claimAdjustment != null)
            {
                model.PayeeName = claimAdjustment.PayeeName;
                model.PolicyholderName = claimAdjustment.PolicyholderName;
                model.FinalAmountToPaid = claimAdjustment.FinalAmountToPaid;
            }





            var query = "select ClaimRegistrationProviderDetial.Id, ProviderType, ServiceProviderName, ServiceProviderFee from ClaimRegistrationProviderDetial ";
            query += " join ServiceProvider on ClaimRegistrationProviderDetial.ServiceProviderId = ServiceProvider.Id ";
            query += "join ServiceProviderType on ClaimRegistrationProviderDetial.ServiceProviderTypeId = ServiceProviderType.id ";
            query += " where ClaimRegistrationProviderDetial.ClaimRegistrationId =" + claimAdjustment.ClaimRegisterationId;


            model.ServiceProviderList = InsuranceContext.Query(query).Select(c => new ClaimRegistrationProviderModel { Id = c.Id, ServiceProviderName = c.ServiceProviderName, ServiceProviderType = c.ProviderType, ServiceProviderFee = ServiceProviderFee(c.ProviderType, c.ServiceProviderFee, claimAdjustment.ExcessesAmount) }).ToList();

            var claimRegistrationProvider = InsuranceContext.ClaimRegistrationProviderDetials.All(where: "ClaimRegistrationId=" + claimAdjustment.ClaimRegisterationId).Select(c => c.ServiceProviderFee).Sum();




            model.TotalAmountLeftToPayed = Convert.ToString(claimRegistrationProvider- _excessAmount);

            // var providerList = InsuranceContext.ClaimRegistrationProviderDetials.All(where : "ClaimRegistrationId=" + claimAdjustment).ToList();

            return View(model);
        }


        public decimal ServiceProviderFee(string providerType, decimal providerFee, decimal excessAmount)
        {

            decimal fee = providerFee;

            if (providerType== "Repairers")
            {
                fee = providerFee - excessAmount;
                _excessAmount = excessAmount;
            }

            return fee;

        }


        public ActionResult ProviderPayment(int RegistrationProviderId, int ClaimAdjustmentId)
        {
            ClaimAdjustmentModel model = new ClaimAdjustmentModel();
            var ePaymentDetail = from ePayeeBankDetails e in Enum.GetValues(typeof(ePayeeBankDetails))
                                 select new
                                 {
                                     ID = (int)e,
                                     Name = e.ToString()
                                 };

            ViewBag.ePaymentDetailData = new SelectList(ePaymentDetail, "ID", "Name");

            model.RegistrationProviderId = RegistrationProviderId;
            model.Id = ClaimAdjustmentId;

            return View(model);
        }

        [HttpPost]
        public ActionResult ClaimPayment(ClaimAdjustmentModel model)
        {

            var claimAdjustment = InsuranceContext.ClaimAdjustments.Single(where: $"Id = '{model.Id}'");

            if (claimAdjustment != null)
            {
                claimAdjustment.PayeeName = model.PayeeName;
                claimAdjustment.PolicyholderName = model.PolicyholderName;
                claimAdjustment.PayeeBankDetails = model.PayeeBankDetails;
                claimAdjustment.PhoneNumber = model.PhoneNumber;
                InsuranceContext.ClaimAdjustments.Update(claimAdjustment);

            }

            var ePaymentDetail = from ePayeeBankDetails e in Enum.GetValues(typeof(ePayeeBankDetails))
                                 select new
                                 {
                                     ID = (int)e,
                                     Name = e.ToString()
                                 };
            ViewBag.ePaymentDetailData = new SelectList(ePaymentDetail, "ID", "Name");

            //  return RedirectToAction("ClaimRegistrationList" , "Claimant");

            return RedirectToAction("ProviderPayment", "Claimant", new { RegistrationProviderId = model.RegistrationProviderId, ClaimAdjustmentId = model.Id });

        }

        [HttpPost]
        public ActionResult ProviderPayment(ClaimAdjustmentModel model)
        {

            var claimAdjustment = InsuranceContext.ClaimAdjustments.Single(where: $"id = '{model.Id}'");

            var ClaimRegistrationProviderDetial = InsuranceContext.ClaimRegistrationProviderDetials.Single(where: "Id=" + model.RegistrationProviderId);


            decimal serviceProviderFee = 0;

            var claimRegistrationDetials = InsuranceContext.ClaimRegistrations.Single(where: $"id = '{claimAdjustment.ClaimRegisterationId}'");

            if (claimRegistrationDetials != null)
            {
                serviceProviderFee = ClaimRegistrationProviderDetial.ServiceProviderFee;

                decimal calCulationFee = claimRegistrationDetials.TotalProviderFees - ClaimRegistrationProviderDetial.ServiceProviderFee;

                claimRegistrationDetials.TotalProviderFees = calCulationFee;


                if (calCulationFee <= 0)
                    claimRegistrationDetials.ClaimStatus = (int)claimStatus.Approved;

                claimRegistrationDetials.CreatedOn = DateTime.Now;

                InsuranceContext.ClaimRegistrations.Update(claimRegistrationDetials);
            }


            if (ClaimRegistrationProviderDetial != null)
            {
                ClaimRegistrationProviderDetial.ServiceProviderFee = 0;
                InsuranceContext.ClaimRegistrationProviderDetials.Update(ClaimRegistrationProviderDetial);

                ServiceProviderPaymentHistory providerPaymentHistory = new ServiceProviderPaymentHistory { ClaimRegistrationId = claimAdjustment.ClaimRegisterationId, RegistrationDetialProviderId = model.RegistrationProviderId, PaidAmount = serviceProviderFee, CreatedOn = DateTime.Now };
                InsuranceContext.ServiceProviderPaymentHistories.Insert(providerPaymentHistory);


            }


            //06_june_2019
            claimAdjustment.PhoneNumber = model.PhoneNumber; 
            claimAdjustment.PayeeBankDetails = model.PayeeBankDetails;
            InsuranceContext.ClaimAdjustments.Update(claimAdjustment);




            var ePaymentDetail = from ePayeeBankDetails e in Enum.GetValues(typeof(ePayeeBankDetails))
                                 select new
                                 {
                                     ID = (int)e,
                                     Name = e.ToString()
                                 };
            ViewBag.ePaymentDetailData = new SelectList(ePaymentDetail, "ID", "Name");

            return RedirectToAction("ClaimPayment", "ClaimAdjustment", new { id = claimAdjustment.Id, successMsg = "Make another provider payment." });

            // return RedirectToAction("ClaimRegistrationList", "Claimant");
            //return View(model);
        }


        public ActionResult SaveClaimAdjustment(ClaimAdjustmentModel model)
        {
            ModelState.Remove("Id");
            ModelState.Remove("PayeeBankDetails");
            ModelState.Remove("PayeeName");


            if (model.IsDriverUnder25 == null)
            {
                ModelState.Remove("IsDriverUnder25");
            }
            if (model.DriverIsUnder21 == null)
            {
                ModelState.Remove("DriverIsUnder21");
            }


            //foreach (ModelState modelState in ViewData.ModelState.Values)
            //{
            //    foreach (ModelError error in modelState.Errors)
            //    {
            //        var res = "";
            //    }
            //}


            if (ModelState.IsValid)
            {
                string userid = "";
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;

                if (userLoggedin)
                {

                    var claimadjust = InsuranceContext.ClaimAdjustments.Single(where: $"ClaimRegisterationId = '{model.ClaimRegisterationId}'");

                    if (claimadjust == null)
                    {
                        userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                        var data = Mapper.Map<ClaimAdjustmentModel, ClaimAdjustment>(model);
                        data.CreatedOn = DateTime.Now;
                        data.CreatedBy = customer.Id;
                        data.IsActive = true;
                        InsuranceContext.ClaimAdjustments.Insert(data);
                        return RedirectToAction("ClaimPayment", new { id = data.Id });
                    }
                    else
                    {
                        userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                        var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                        var data = Mapper.Map<ClaimAdjustmentModel, ClaimAdjustment>(model);
                        data.Id = claimadjust.Id;
                        data.CreatedOn = DateTime.Now;
                        data.CreatedBy = customer.Id;
                        data.IsActive = true;
                        InsuranceContext.ClaimAdjustments.Update(data);
                        return RedirectToAction("ClaimPayment", new { id = claimadjust.Id });
                    }
                }
            }
            return View("Index");
        }
        public ActionResult ListClaimAdjustment()
        {

            var ListClaimAdjust = InsuranceContext.ClaimAdjustments.All(where: $"IsActive = 'True' or IsActive is null").OrderByDescending(x => x.Id);

            return View(ListClaimAdjust);
        }
        [HttpGet]
        public ActionResult EditClaimAdjustment(int Id)
        {
            var claimadjustdata = InsuranceContext.ClaimAdjustments.Single(where: $"Id ='{Id}'");
            var model = Mapper.Map<ClaimAdjustment, ClaimAdjustmentModel>(claimadjustdata);

            return View(model);
        }
        public ActionResult EditClaimAdjustment(ClaimAdjustmentModel model)
        {

            if (ModelState.IsValid)
            {
                string userid = "";
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                if (userLoggedin)
                {
                    userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                    var claimadjustdata = Mapper.Map<ClaimAdjustmentModel, ClaimAdjustment>(model);
                    var record = InsuranceContext.ClaimAdjustments.Single(where: $"Id = '{model.Id}'");
                    claimadjustdata.ModifiedOn = DateTime.Now;
                    claimadjustdata.ModifiedBy = customer.Id;
                    claimadjustdata.IsActive = true;
                    claimadjustdata.CreatedOn = record.CreatedOn;
                    InsuranceContext.ClaimAdjustments.Update(claimadjustdata);
                    return RedirectToAction("ListClaimAdjustment");
                }
            }
            return View("EditClaimAdjustment");
        }
        public ActionResult DeleteClaimAdjustment(int Id)
        {
            string query = $"update ClaimAdjustment set IsActive = 0 where Id={Id}";
            InsuranceContext.ClaimAdjustments.Execute(query);
            return RedirectToAction("ListClaimAdjustment");
        }
        [HttpPost]
        public JsonResult CalculateClaimPremium(decimal sumInsured, int? IsPartialLoss, int? IsLossInZimbabwe, int? IsStolen, int? Islicensedless60months, int? DriverIsUnder21, Boolean PrivateCar, Boolean CommercialCar, int? IsDriver25, int? IsSoundSystem, decimal? TotalAmount, decimal? FinalAmountToPaid, decimal repairercost, decimal? otherServiceProCost, decimal? TotalClaimCost)
        {
            JsonResult json = new JsonResult();
            var ClaimQuoteL = new ClaimQuoteLogic();
            var excess = ClaimQuoteL.CalculateClaimPremium(sumInsured, IsPartialLoss, IsLossInZimbabwe, IsStolen, Islicensedless60months, DriverIsUnder21, PrivateCar, CommercialCar, IsDriver25, IsSoundSystem, TotalAmount, FinalAmountToPaid, repairercost, otherServiceProCost, TotalClaimCost);
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            json.Data = excess;
            return json;
        }
    }
}