using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using InsuranceClaim.Models;
using AutoMapper;

namespace InsuranceClaim.Controllers
{
    public class PolicyInsurerController : Controller
    {
        // GET: Policy
        public ActionResult Index()
        {
            InsuranceClaim.Models.PolicyInsurerModel obj = new InsuranceClaim.Models.PolicyInsurerModel();
            List<Insurance.Domain.PolicyInsurer> objList = new List<Insurance.Domain.PolicyInsurer>();
            objList = InsuranceContext.PolicyInsurers.All().ToList();
            return View(obj);
        }
        [HttpPost]
        public ActionResult PolicySave(PolicyInsurerModel model)
        {
            var data = Mapper.Map<PolicyInsurerModel, PolicyInsurer>(model);
            InsuranceContext.PolicyInsurers.Insert(data);
            return RedirectToAction("PolicyInsurerList");
        }
        [Authorize(Roles = "Staff,Administrator")]
        public ActionResult PolicyInsurerList()
        {
            var db = InsuranceContext.PolicyInsurers.All(where:"IsActive = 'True' or IsActive is null").ToList();

            return View(db);
        }
        public ActionResult EditPolicy(int Id)
        {
            var record = InsuranceContext.PolicyInsurers.All(where: $"Id ={Id}").FirstOrDefault();
            var data = Mapper.Map<PolicyInsurer,PolicyInsurerModel>(record);
            return View(data);
        }
            [HttpPost]
        public ActionResult EditPolicy(PolicyInsurerModel model)
        {

            if (ModelState.IsValid)
            {

                var data = Mapper.Map<PolicyInsurerModel, PolicyInsurer>(model);
                InsuranceContext.PolicyInsurers.Update(data);
            }
            return RedirectToAction("PolicyInsurerList");
        }
        public ActionResult DeletePolicy( int Id)
        {
            string query = $"update PolicyInsurer set IsActive = 0 where Id = {Id}";
            InsuranceContext.PolicyInsurers.Execute(query);

            return RedirectToAction("PolicyInsurerList");
        }
    }

}

