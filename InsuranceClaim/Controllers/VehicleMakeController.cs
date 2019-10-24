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
    public class VehicleMakeController : Controller
    {
        // GET: VehicleMake
        public ActionResult Index()
        {


            InsuranceClaim.Models.VehiclesMakeModel obj = new InsuranceClaim.Models.VehiclesMakeModel();
            List<Insurance.Domain.VehicleMake> objList = new List<Insurance.Domain.VehicleMake>();
            objList = InsuranceContext.VehicleMakes.All().ToList();

            return View(obj);
        }

        // GET: VehicleMake/Details/5
        [HttpPost]
        public ActionResult SaveVehicleMake(VehiclesMakeModel Model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dbVehicalMake = InsuranceContext.VehicleMakes.Single(where: $"MakeDescription = '" + Model.MakeDescription + "'");

                    if (dbVehicalMake == null)
                    {
                        var dbModel = Mapper.Map<VehiclesMakeModel, VehicleMake>(Model);
                        dbModel.CreatedOn = DateTime.Now;
                        dbModel.ModifiedOn = DateTime.Now;
                        dbModel.MakeDescription = Model.MakeDescription.ToUpper();
                        dbModel.MakeCode = Model.MakeCode;
                        dbModel.ShortDescription = Model.ShortDescription;

                        InsuranceContext.VehicleMakes.Insert(dbModel);
                    }

                }
                else
                {
                    return RedirectToAction("VehicleMakeList");
                }

            }
            catch (Exception ex)
            {
                TempData["ErroMsg"] = ex.Message;
                //  View(Model);
            }

            return RedirectToAction("VehicleMakeList");
        }

        // GET: VehicleMake/Create
        public ActionResult VehicleMakeList()
        {

          

            var makelist = InsuranceContext.VehicleMakes.All(where: "IsActive = 'True' or IsActive is Null").OrderByDescending(x => x.Id).ToList();
            return View(makelist);
        }

        public ActionResult VehicleMakeEdit(int Id)
        {
            var record = InsuranceContext.VehicleMakes.All(where: $"Id ={Id}").FirstOrDefault();

            var model = Mapper.Map<VehicleMake, VehiclesMakeModel>(record);
            return View(model);
        }
        [HttpPost]
        public ActionResult VehicleMakeEdit(VehiclesMakeModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var makeid = model.Id;
                    //var data = Mapper.Map<VehiclesMakeModel, VehicleMake>(model);
                    var data = InsuranceContext.VehicleMakes.Single(where: $"Id = {makeid}");

                    if(data!=null)
                    {
                        if(!CheckMakeExist(data.MakeDescription, model.MakeDescription))
                        {
                            TempData["errorMsg"] = "Make description already exist, please try again.";
                            return View(model);
                        }
                    }

                    data.MakeDescription = model.MakeDescription.ToUpper();
                    data.MakeCode = model.MakeCode;
                    data.ShortDescription = model.ShortDescription;
                    //data.CreatedOn = model.CreatedOn;
                    data.ModifiedOn = DateTime.Now;
                    InsuranceContext.VehicleMakes.Update(data);

                }

            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("VehicleMakeList");
        }

        private bool CheckMakeExist(string oldMake, string newMake)
        {

            if(oldMake== newMake)
            {
                return true;
            }
            else
            {

                var dbVehicalMake = InsuranceContext.VehicleMakes.Single(where: $"MakeDescription = '" + newMake + "'");

                if(dbVehicalMake!=null)
                {
                    return false;
                }


            }

            return true;
        }

        public ActionResult DeleteMake(int id)
        {
            var makeDetials = InsuranceContext.VehicleMakes.Single(id);

            if(makeDetials!=null)
            {
                var vehicelDetials = InsuranceContext.VehicleDetails.Single(where: $"MakeId='{makeDetials.MakeCode}'");

                if(vehicelDetials==null)
                {
                    string query = $"update VehicleMake set IsActive =0 where Id={id}";
                    InsuranceContext.VehicleMakes.Execute(query);
                }
                else
                {
                    TempData["errorMsg"] = "Vehicle exist for this Make.";
                }

            }

            

            return RedirectToAction("VehicleMakeList");
        }




    }
}
