using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Insurance.Domain;
using AutoMapper;

namespace InsuranceClaim.Controllers
{
    public class SourceDetailController : Controller
    {
        // GET: SourceDetail
        public ActionResult Index()
        {
            return View();
        }
     
        public ActionResult AddSourceDetails()
        {
            ViewBag.BusinessSources = InsuranceContext.BusinessSources.All().ToList();
            return View();
        }

        [HttpPost]
        public ActionResult AddSourceDetails(SourceDetailModel model)
        {
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            string userid = "";
            if (ModelState.IsValid)
            {
                if (userLoggedin)
                {
                    userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    var customer = InsuranceContext.Customers.Single(where: $"UserId ='{userid}'");
                    var dbModel = Mapper.Map<SourceDetailModel, SourceDetail>(model);
                    dbModel.CreatedOn = DateTime.Now;
                    dbModel.CreatedBy = customer.Id;
                    dbModel.BusinessId =Convert.ToInt32(model.Source);
                    dbModel.IsDeleted = true;
                    InsuranceContext.SourceDetails.Insert(dbModel);
                    return RedirectToAction("SourceDetailsList");
                }
            }

            else {
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        var result = "";
                    }
                }
            }
            return View();
        }

        public ActionResult SourceDetailsList()
        {
                   var SourceDetailObjList = (from _Sd in InsuranceContext.SourceDetails.All().ToList()
                                               join BS in InsuranceContext.BusinessSources.All().ToList()
                                               on _Sd.BusinessId equals BS.Id
                                               where _Sd.IsDeleted == true
                                               select new SourceDetailModel
                                               {
                                                   Email = _Sd.Email,
                                                   FullName = _Sd.FirstName + " " + _Sd.LastName,
                                                   PhoneNumber = _Sd.PhoneNumber,
                                                   Id = _Sd.Id,
                                                   Address = _Sd.Address,
                                                   SourceName = BS.Source
                                               }
                                   ).ToList().OrderByDescending(c => c.Id);
                    return View(SourceDetailObjList);   
        }

        public ActionResult EditSourceDetail(int Id)
        {
            ViewBag.BusinessSources = InsuranceContext.BusinessSources.All().ToList();
            var record = InsuranceContext.SourceDetails.All(where: $"Id ={Id}").FirstOrDefault();

            var model= Mapper.Map<SourceDetail , SourceDetailModel>(record);
            model.BusinessId = record.BusinessId;
            return View(model); 
        }
        [HttpPost]
        public ActionResult EditSourceDetail(SourceDetailModel model)
        {
            if (ModelState.IsValid)
            {
                string userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                var customer = InsuranceContext.Customers.Single(where: $"UserId ='{userid}'");
                var data = Mapper.Map<SourceDetailModel, SourceDetail>(model);
                data.ModifiedOn = DateTime.Now;
                data.ModifiedBy = Convert.ToInt32(customer.Id);       
                data.IsDeleted = true;
                InsuranceContext.SourceDetails.Update(data);
                return RedirectToAction("SourceDetailsList");
            }
            return View();
        }

        public ActionResult DeleteSources(int Id)
        {
            string query = $"update SourceDetail set IsDeleted = 0 where Id = {Id}";
            InsuranceContext.SourceDetails.Execute(query);
            return RedirectToAction("SourceDetailsList");
        }
    }
}