using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;

namespace InsuranceClaim.Controllers
{
    
    public class CovertypeController : Controller
    {
        // GET: Covertype
        public ActionResult Index()
        {
            InsuranceClaim.Models.CovertypeModel obj = new InsuranceClaim.Models.CovertypeModel();
            List<Insurance.Domain.CoverType> objList = new List<Insurance.Domain.CoverType>();
            objList = InsuranceContext.CoverTypes.All().ToList();
            return View(obj);
        }

        [HttpPost]
        public ActionResult SaveCover(CovertypeModel model)
        {
            var dbModel = Mapper.Map<CovertypeModel, CoverType>(model);
            InsuranceContext.CoverTypes.Insert(dbModel);

            return RedirectToAction("CoverList");
        }

        [Authorize(Roles = "Staff,Administrator")]
        public ActionResult CoverList()
        {
            var db = InsuranceContext.CoverTypes.All(where: "IsActive = 'True' or IsActive is null").ToList();


            return View(db);
        }
        public ActionResult EditCovertype(int Id)
        {
            var record = InsuranceContext.CoverTypes.All(where: $"Id ={Id}").FirstOrDefault();
            var model = Mapper.Map<CoverType, CovertypeModel>(record);
            return View(model);
        }
        [HttpPost]
        public ActionResult EditCovertype(CovertypeModel model)
        {

            if (ModelState.IsValid)
            {

                var data = Mapper.Map<CovertypeModel, CoverType>(model);
                InsuranceContext.CoverTypes.Update(data);
            }
            return RedirectToAction("CoverList");
        }
        public ActionResult DeleteCovertype(int Id)
        {
            string query = $"update CoverType set IsActive = 0 where Id = {Id}";
            InsuranceContext.CoverTypes.Execute(query);

            return RedirectToAction("CoverList");
        }
    }
}
