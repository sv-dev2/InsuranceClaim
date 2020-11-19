using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Insurance.Domain;
using InsuranceClaim.Models;
using AutoMapper;

namespace InsuranceClaim.Controllers
{
    public class CommissionController : Controller
    {
        // GET: Commission
        public ActionResult Index()
        {
            InsuranceClaim.Models.AgentCommissionModel obj = new InsuranceClaim.Models.AgentCommissionModel();
            List<Insurance.Domain.AgentCommission> objList = new List<Insurance.Domain.AgentCommission>();
            objList = InsuranceContext.AgentCommissions.All().ToList();
            return View(obj);

    }
    [HttpPost]
        public ActionResult CommissionSave(AgentCommissionModel model)
        {

            var data = Mapper.Map<AgentCommissionModel, AgentCommission>(model);
            InsuranceContext.AgentCommissions.Insert(data);
            return RedirectToAction("CommissionList");
            
        }
        [Authorize(Roles = "Staff,Administrator")]
        public ActionResult CommissionList()
        {
            var db = InsuranceContext.AgentCommissions.All(where:"IsActive='True' Or IsActive is null").ToList();

            return View(db);
        }
        public ActionResult CommissionEdit(int Id)
        {
            var record = InsuranceContext.AgentCommissions.All(where: $"Id ={Id}").FirstOrDefault();
            var model = Mapper.Map<AgentCommission, AgentCommissionModel>(record);
            return View(model);
        }
        [HttpPost]
        public ActionResult CommissionEdit(AgentCommissionModel model )
        {
            if (ModelState.IsValid)
            {
                var data = Mapper.Map<AgentCommissionModel, AgentCommission>(model);
                InsuranceContext.AgentCommissions.Update(data);
            }

            return RedirectToAction("CommissionList");

        }
        public ActionResult DeleteCommission(int Id)
        {
            string query = $"update AgentCommission set IsActive = 0 where Id ={Id}";
            InsuranceContext.AgentCommissions.Execute(query);


            return RedirectToAction("CommissionList");
        }

    }
}
