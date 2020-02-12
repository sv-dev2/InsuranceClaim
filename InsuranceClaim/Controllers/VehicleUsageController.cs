using AutoMapper;
using Insurance.Domain;
using Insurance.Service;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class VehicleUsageController : Controller
    {
        private ApplicationUserManager _userManager;
        private InflationFactorService inflationFactorService = new InflationFactorService();

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

        // GET: VehicleUsage
        public ActionResult Index()
        {
            var obj = new InsuranceClaim.Models.VehicleUsageModel();
            var objList = InsuranceContext.VehicleUsages.All().ToList();
            ViewBag.Products = InsuranceContext.Products.All().ToList();
            return View(obj);
        }

        public ActionResult InflationFactorList()
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;


            var InflationFactorList = inflationFactorService.getAll();
            foreach (var item in InflationFactorList)
            {
                var id = item.CreatedBy;
                var customer = InsuranceContext.Customers.All(where: $"Id = '{id}'").FirstOrDefault();
                if(customer!=null)
                {
                    item.CreatedByName = textInfo.ToTitleCase(customer.FirstName.ToLower()) + " " + textInfo.ToTitleCase(customer.LastName.ToLower());
                }
                
            }
            return View(InflationFactorList);
        }

        [Authorize(Roles = "Staff, Administrator")]
        public ActionResult SaveInflationFactor()
        {
            var obj = new InsuranceClaim.Models.InflationFactorModel();
            return View(obj);
        }
        [HttpPost]
        public ActionResult SaveInflation(InflationFactorModel model)
        {

            var dbModel = Mapper.Map<InflationFactorModel, InflationFactor>(model);

            if (dbModel.IsActive == true)
            {
                var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{User.Identity.GetUserId().ToString()}'").FirstOrDefault();

                var inflationFactors = InsuranceContext.InflationFactors.All(where: "IsActive is not null").OrderByDescending(x => x.CreatedOn).ToList();

                foreach (var item in inflationFactors)
                {
                    item.IsActive = false;
                    item.DeActivatedOn = item.DeActivatedOn == null ? DateTime.Now : item.DeActivatedOn;
                    InsuranceContext.InflationFactors.Update(item);
                }

                dbModel.CreatedOn = DateTime.Now;
                dbModel.CreatedBy = _customerData.Id;
                InsuranceContext.InflationFactors.Insert(dbModel);
            }
            return RedirectToAction("InflationFactorList");
        }

        [HttpPost]
        public ActionResult SaveVehicleUsage(VehicleUsageModel model)
        {
            var dbModel = Mapper.Map<VehicleUsageModel, VehicleUsage>(model);
            InsuranceContext.VehicleUsages.Insert(dbModel);
            return RedirectToAction("VehicleUsageList");
        }
        [Authorize(Roles = "Staff,Administrator")]
        public ActionResult VehicleUsageList()
        {
            var inflationVehicle = new InflationVehicleDTO();
            var UserList = InsuranceContext.VehicleUsages.All(where: "IsActive='True' or IsActive is null").ToList();
            var inflationFactor = inflationFactorService.getActiveInflationfactor();
            inflationVehicle.VehicleUsages = UserList.ToList();
            inflationVehicle.InflationFactor = inflationFactor;

            return View(inflationVehicle);

        }
        public ActionResult EditVehicleUsage(int Id)
        {
            var record = InsuranceContext.VehicleUsages.All(where: $"Id ={Id}").FirstOrDefault();
            ViewBag.Products = InsuranceContext.Products.All().ToList();
            var data = Mapper.Map<VehicleUsage, VehicleUsageModel>(record);
            return View(data);
        }
        [HttpPost]
        public ActionResult EditVehicleUsage(VehicleUsageModel model)
        {

            var data = Mapper.Map<VehicleUsageModel, VehicleUsage>(model);
            InsuranceContext.VehicleUsages.Update(data);

/*            Debug.WriteLine(ModelState.IsValid);
            if (ModelState.IsValid)
            {


               
            }*/

            return RedirectToAction("VehicleUsageList");
        }
        public ActionResult DeleteVehicleUsage(int Id)
        {

            //var record = InsuranceContext.VehicleUsages.All(where: $"Id ={Id}").FirstOrDefault();

            string query = $"update VehicleUsage set IsActive=0 where Id={Id}";

            InsuranceContext.VehicleUsages.Execute(query);
            return RedirectToAction("VehicleUsageList");
        }
    }
}