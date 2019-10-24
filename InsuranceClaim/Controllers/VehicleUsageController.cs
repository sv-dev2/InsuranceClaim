using AutoMapper;
using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class VehicleUsageController : Controller
    {
        // GET: VehicleUsage
        public ActionResult Index()
        {
            var obj = new InsuranceClaim.Models.VehicleUsageModel();
            var objList = InsuranceContext.VehicleUsages.All().ToList();
            ViewBag.Products = InsuranceContext.Products.All().ToList();
            return View(obj);
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
            var UserList = InsuranceContext.VehicleUsages.All(where: "IsActive='True' or IsActive is null").ToList();
            return View(UserList);

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
            if (ModelState.IsValid)
            {


                var data = Mapper.Map<VehicleUsageModel, VehicleUsage>(model);
                InsuranceContext.VehicleUsages.Update(data);
            }

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