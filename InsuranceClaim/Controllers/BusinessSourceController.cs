using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class BusinessSourceController : Controller
    {
        // GET: BusinessSource
        public ActionResult Index()
        {
            var result = (from business in InsuranceContext.BusinessSources.All().ToList()
                          select new BusinessSourceModel { Id = business.Id, Source = business.Source, CreatedOn = business.CreatedOn }).ToList();

            return View(result);
        }

        // GET: BusinessSource/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BusinessSource/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BusinessSource/Create
        [HttpPost]
        public ActionResult Create(BusinessSourceModel model)
        {
            try
            {
                // TODO: Add insert logic here

                ModelState.Remove("Id");

                if (ModelState.IsValid)
                {
                    var dbBusinessResource = InsuranceContext.BusinessSources.Single(where: $"Source ='" + model.Source + "'");

                    if (dbBusinessResource == null)
                    {
                        BusinessSource buesiness = new BusinessSource { Source = model.Source, CreatedOn = DateTime.Now };
                        InsuranceContext.BusinessSources.Insert(buesiness);
                    }
                    else
                    {
                        TempData["errorMsg"] = "Source alredy exist.";
                    }
                }


                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: BusinessSource/Edit/5
        public ActionResult Edit(int id)
        {
            BusinessSourceModel model = new BusinessSourceModel();

            var businessResource = InsuranceContext.BusinessSources.Single(id);

            if (businessResource != null)
            {
                model.Id = businessResource.Id;
                model.Source = businessResource.Source;
            }

            return View(model);
        }

        // POST: BusinessSource/Edit/5
        [HttpPost]
        public ActionResult Edit(BusinessSourceModel model)
        {
            try
            {
                // TODO: Add update logic here

                if (ModelState.IsValid)
                {
                    var businessResource = InsuranceContext.BusinessSources.Single(model.Id);

                  
                    if (businessResource != null)
                    {

                        if (CheckSourceExist(businessResource.Source, model.Source))
                        {
                            businessResource.Source = businessResource.Source;
                            InsuranceContext.BusinessSources.Update(businessResource);
                        }
                        else
                        {
                            TempData["errorMsg"] = "Source alredy exist.";
                        }


                        
                    }
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: BusinessSource/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: BusinessSource/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Commission()
        {
            List<AgentCommissionModel> list = (InsuranceContext.Query("select AgentCommission.Id, Source, CommissionName, CommissionAmount from AgentCommission join BusinessSource on AgentCommission.BusinessSourceId = BusinessSource.Id").Select(c => new AgentCommissionModel() { Source = c.Source, Id = c.Id, CommissionName = c.CommissionName, CommissionAmount = c.CommissionAmount })).ToList();
            return View(list);
        }

        public ActionResult AddCommission()
        {
            AgentCommissionModel model = new AgentCommissionModel();
            ViewBag.Sources = InsuranceContext.BusinessSources.All();
            return View(model);
        }

        public ActionResult AddCommision(AgentCommissionModel model)
        {
            ModelState.Remove("Id");
            ModelState.Remove("ManagementCommission");

            if (ModelState.IsValid)
            {
                var agentCommission = InsuranceContext.AgentCommissions.Single(where: $"BusinessSourceId =" + model.BusinessSourceId);

                if (agentCommission == null)
                {
                    AgentCommission agentCommision = new AgentCommission { BusinessSourceId = model.BusinessSourceId, CommissionName = model.CommissionName, CommissionAmount = model.CommissionAmount, CreatedOn = DateTime.Now };
                    InsuranceContext.AgentCommissions.Insert(agentCommision);
                }
                else
                {
                    TempData["errorMsg"] = "Agent commmission alredy exist for selected source.";
                }
            }

            return RedirectToAction("Commission");
        }

        public ActionResult EditCommission(int Id)
        {
            var detials = InsuranceContext.AgentCommissions.Single(Id);
            var agentCommissionModel = AutoMapper.Mapper.Map<AgentCommission, AgentCommissionModel>(detials);
            ViewBag.Sources = InsuranceContext.BusinessSources.All();
            return View(agentCommissionModel);
        }


        [HttpPost]
        public ActionResult EditCommission(AgentCommissionModel model)
        {
            ModelState.Remove("ManagementCommission");
            if (ModelState.IsValid)
            {
                var detials = InsuranceContext.AgentCommissions.Single(model.Id);

                if (CheckAgentExist(detials.BusinessSourceId, model.BusinessSourceId))
                {
                    detials.CommissionName = model.CommissionName;
                    detials.CommissionAmount = model.CommissionAmount;
                    detials.BusinessSourceId = model.BusinessSourceId;

                    InsuranceContext.AgentCommissions.Update(detials);
                }
                else
                {
                    TempData["errorMsg"] = "Agent commmission alredy exist for selected source.";
                }




                ViewBag.Sources = InsuranceContext.BusinessSources.All();

            }

            return RedirectToAction("Commission");

        }


        private bool CheckAgentExist(int oldSourceId, int newSourceId)
        {
            if (oldSourceId == newSourceId)
            {
                return true;
            }
            else
            {
                var dbAgentCommission = InsuranceContext.AgentCommissions.Single(where: $"BusinessSourceId='{newSourceId}'");

                if (dbAgentCommission != null)
                {
                    return false;
                }
            }
            return true;
        }


        private bool CheckSourceExist(string oldSourceName, string newSourceName)
        {
            if (oldSourceName == newSourceName)
            {
                return true;
            }
            else
            {
                var dbSource = InsuranceContext.BusinessSources.Single(where: $"Source='{newSourceName}'");

                if (dbSource != null)
                {
                    return false;
                }
            }
            return true;
        }




    }
}
