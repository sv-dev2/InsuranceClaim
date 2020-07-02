using AutoMapper;
using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class ServiceProviderTypeController : Controller
    {
        // GET: Branch
        public ActionResult Index()
        {
            InsuranceClaim.Models.ServiceProviderTypeModel obj = new InsuranceClaim.Models.ServiceProviderTypeModel();
            IEnumerable<Insurance.Domain.ServiceProviderType> objList = new List<Insurance.Domain.ServiceProviderType>();
            objList = InsuranceContext.ServiceProviderTypes.All().ToList();

            return View(objList);
        }

        // GET: Home/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceProviderType branch = InsuranceContext.ServiceProviderTypes.Single(id);
            if (branch == null)
            {
                return HttpNotFound();
            }
            return View(branch);
        }

        // GET: Home/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Home/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ServiceProviderTypeModel branch)
        {

            if (ModelState.IsValid)
            {
                var dbModel = Mapper.Map<ServiceProviderTypeModel, ServiceProviderType>(branch);
                InsuranceContext.ServiceProviderTypes.Insert(dbModel);
                return RedirectToAction("Index");
            }

            return View();
        }

        // GET: Home/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var branch = InsuranceContext.ServiceProviderTypes.All(where: $"Id ={id}").FirstOrDefault();
            if (branch == null)
            {
                return HttpNotFound();
            }
            return View(branch);
        }

        // POST: Home/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ServiceProviderTypeModel provider)
        {
            if (ModelState.IsValid)
            {

                var data = InsuranceContext.ServiceProviderTypes.Single(provider.Id);

                if(data!=null)
                {
                    data.ProviderType = provider.ProviderType;

                    InsuranceContext.ServiceProviderTypes.Update(data);
                }
            
             
                return RedirectToAction("Index");
            }
            return View();
        }

        // GET: Home/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ServiceProviderType branch = InsuranceContext.ServiceProviderTypes.Single(id);
            if (branch == null)
            {
                return HttpNotFound();
            }
            return View(branch);
        }

        // POST: Home/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ServiceProviderType branch = InsuranceContext.ServiceProviderTypes.Single(id);
            InsuranceContext.ServiceProviderTypes.Delete(branch);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //InsuranceContext.Dispose();
            }
            base.Dispose(disposing);
        }



    }
}