using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class BranchController : Controller
    {
        // GET: Branch
        public ActionResult Index()
        {
            return View(InsuranceContext.Branches.All());
        }

        // GET: Home/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Branch branch = InsuranceContext.Branches.Single(id);
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
        public ActionResult Create([Bind(Include = "Id,BranchName")] Branch branch)
        {
            if (ModelState.IsValid)
            {
                branch.AlmId = GetALMId();
                InsuranceContext.Branches.Insert(branch);
                return RedirectToAction("Index");
            }

            return View(branch);
        }



        public string GetALMId()
        {

            string almId = "";

            var getcustomerdetail = InsuranceContext.Query(" select top 1 AlmId  from [dbo].[Branch] where AlmId is not null order by id desc ")
         .Select(x => new Customer()
         {
             ALMId = x.AlmId
         }).ToList().FirstOrDefault();


            if (getcustomerdetail != null && getcustomerdetail.ALMId != null)
            {
                string number = getcustomerdetail.ALMId.Split('K')[1];
                long pernumer = Convert.ToInt64(number) + 1;
                string policyNumbera = string.Empty;
                int lengths = 3;
                lengths = lengths - pernumer.ToString().Length;
                for (int i = 0; i < lengths; i++)
                {
                    policyNumbera += "0";
                }
                policyNumbera += pernumer;
                //  customer.ALMId = "GENE-SSK" + policyNumbera;
                almId = "GENE-SSK" + policyNumbera;
            }
            else
            {
                almId = "GENE-SSK003";
            }

            return almId;
        }




        // GET: Home/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Branch branch = InsuranceContext.Branches.Single(id);
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
        public ActionResult Edit( Branch branch)
        {
            if (ModelState.IsValid)
            {
              //  branch.AlmId = GetALMId();
                InsuranceContext.Branches.Update(branch);
                return RedirectToAction("Index");
            }
            return View(branch);
        }

        // GET: Home/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Branch branch = InsuranceContext.Branches.Single(id);
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
            Branch branch = InsuranceContext.Branches.Single(id);
            InsuranceContext.Branches.Delete(branch);
 
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