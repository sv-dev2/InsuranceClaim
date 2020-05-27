using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Insurance.Domain;
using InsuranceClaim.Models;

namespace InsuranceClaim.Controllers
{
    public class CityController : Controller
    {
        // GET: City
        public ActionResult Index()
        {
            var list = (from city in InsuranceContext.Cities.All()
                        select new CltyModel { Id = city.Id, CityName = city.CityName, CreatedOn = city.CreatedOn }).ToList();

            return View(list);
        }

        // GET: City/Details/5
        public ActionResult Details(int id)
        {
            var cityDetails = InsuranceContext.Cities.Single(where: $"CityId = '" + id + "'");

            CltyModel model = new CltyModel();

            if (cityDetails != null)
            {
                model.Id = cityDetails.Id;
                model.CityName = cityDetails.CityName;
                model.CreatedOn = cityDetails.CreatedOn;
            }

            return View(model);
        }

        // GET: City/Create
        public ActionResult Create()
        {
            CltyModel model = new CltyModel();
            return View(model);
        }

        // POST: City/Create
        [HttpPost]
        public ActionResult Create(CltyModel model)
        {
            try
            {
                var cityDetails = InsuranceContext.Cities.Single(where: $"CityName = '" + model.CityName + "'");
                if (cityDetails != null)
                {
                    TempData["errorMsg"] = "City already exist.";
                    return View(model);
                }

                // TODO: Add insert logic here
                if (ModelState.IsValid)
                {
                    try
                    {
                        City city = new City { CityName = model.CityName, CreatedOn = DateTime.Now };
                        InsuranceContext.Cities.Insert(city);
                    }
                    catch (Exception ex)
                    {
                    }
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: City/Edit/5
        public ActionResult Edit(int id)
        {
            var cityDetails = InsuranceContext.Cities.Single(where: $"Id = '" + id + "'");

            CltyModel model = new CltyModel();

            if (cityDetails != null)
            {
                model.Id = cityDetails.Id;
                model.CityName = cityDetails.CityName;
                model.CreatedOn = cityDetails.CreatedOn;
            }

            return View(model);
        }

        // POST: City/Edit/5
        [HttpPost]
        public ActionResult Edit(CltyModel model)
        {
            try
            {
                // TODO: Add update logic here

                var cityDetails = InsuranceContext.Cities.Single(where: $"Id = '" + model.Id + "'");


                if (cityDetails != null)
                {
                   
                        if (!CheckCityExist(cityDetails.CityName, model.CityName))
                        {
                            TempData["errorMsg"] = "Make description already exist, please try again.";
                            return View(model);
                        }
                    



                    cityDetails.CityName = model.CityName;
                    InsuranceContext.Cities.Update(cityDetails);
                }


                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        private bool CheckCityExist(string oldCity, string newCity)
        {

            if (oldCity == newCity)
            {
                return true;
            }
            else
            {

                var dbVehicalMake = InsuranceContext.Cities.Single(where: $"CityName = '" + newCity + "'");

                if (dbVehicalMake != null)
                {
                    return false;
                }


            }

            return true;
        }



        // GET: City/Delete/5
        public ActionResult Delete(int id)
        {

            // var cityDetails = InsuranceContext.Cities.Single(where: $"CityId = '" + id + "'");
            //  InsuranceContext.Cities.Delete(cityDetails);

            return View();
        }

        // POST: City/Delete/5
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

        private void test()
        {
            // ddkdkkdkkdk dfkdskfsk kfdskfksj fdsfksk sfsdkfksdkf
            //  fdkfsdkf ksdfksdfj sdfkj dsk dfdsfkk dkfsdkfjds af sdkfjsdkf sdafksdjfsd f


        }


    }
}
