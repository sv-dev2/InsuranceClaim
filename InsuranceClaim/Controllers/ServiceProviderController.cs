using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using Insurance.Domain;
using AutoMapper;
using Insurance.Service;

namespace InsuranceClaim.Controllers
{
    public class ServiceProviderController : Controller
    {
        // GET: ServiceProvider
        [HttpGet]
        public ActionResult SaveServiceProviders()
        {
            ViewBag.ProviderTypes = InsuranceContext.ServiceProviderTypes.All().ToList();
            return View();
        }

        [HttpPost]
        public ActionResult SaveServiceProviders(ServiceProviderModel model)
        {
            if (ModelState.IsValid)
            {
                var dbModel = Mapper.Map<ServiceProviderModel, ServiceProvider>(model);
                dbModel.CreatedOn = DateTime.Now;
                dbModel.IsDeleted = true;
                InsuranceContext.ServiceProviders.Insert(dbModel);
                return RedirectToAction("ProvidersList");              
            }
            return View();
        }
        //[Authorize(Roles = "Staff")]
        [HttpGet]
        public ActionResult ProvidersList()
        {
            SummaryDetailService _summaryDetailService = new SummaryDetailService();

            var currenyList = _summaryDetailService.GetAllCurrency();
            //InsuranceClaim.Models.ServiceProviderModel obj = new InsuranceClaim.Models.ServiceProviderModel();
            //List<Insurance.Domain.ServiceProvider> objList = new List<Insurance.Domain.ServiceProvider>();
            // objList = InsuranceContext.ServiceProviders.All(where: "IsDeleted = 'True' or IsDeleted is null").ToList();

            //var servicetype = InsuranceContext.ServiceProviderTypes.All().ToList();

            var  objList = (from _service in InsuranceContext.ServiceProviders.All().ToList()
                        join _servicetype in InsuranceContext.ServiceProviderTypes.All().ToList()
                        on _service.ServiceProviderType equals _servicetype.Id
                        where _service.IsDeleted == true 
                           select new ServiceProviderModel
                        {

                            ServiceProviderName = _service.ServiceProviderName,
                            ServiceProviderType = Convert.ToString(_servicetype.ProviderType),
                            ServiceProviderContactDetails = _service.ServiceProviderContactDetails,
                            ServiceProviderFees = _service.ServiceProviderFees,
                           
                            Id = _service.Id


                        }
                         ).ToList().OrderByDescending(c=>c.Id);
            return View(objList);
        }

        [HttpGet]
        public ActionResult EditProviders(int Id)
        {
            ViewBag.ProviderTypes = InsuranceContext.ServiceProviderTypes.All().ToList();
            var record = InsuranceContext.ServiceProviders.All(where: $"Id ={Id}").FirstOrDefault();
            var model = Mapper.Map<ServiceProvider, ServiceProviderModel>(record);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditProviders(ServiceProviderModel model)
        {
            if (ModelState.IsValid)
            {
                var data = Mapper.Map<ServiceProviderModel, ServiceProvider>(model);
                data.CreatedOn = DateTime.Now;
                data.IsDeleted = true;
                InsuranceContext.ServiceProviders.Update(data);
                return RedirectToAction("ProvidersList");
            }
            return View();
        }

        public ActionResult DeleteProviders(int Id)
        {
            string query = $"update ServiceProvider set IsDeleted = 0 where Id = {Id}";
            InsuranceContext.ServiceProviders.Execute(query);
            return RedirectToAction("ProvidersList");
        }
    }
}
