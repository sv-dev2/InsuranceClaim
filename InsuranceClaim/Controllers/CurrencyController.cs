using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class CurrencyController : Controller
    {
        // GET: Currency
        public ActionResult Index()
        {
          
            var currencyList = (from currency in InsuranceContext.Currencies.All().ToList()
                                select new CurrencyModel { CurrencyName = currency.Name, Description = currency.Description, CreatedOn = currency.CreatedOn, Id = currency.Id,IsActive=currency.IsActive }).ToList();
            return View(currencyList);
        }

        // GET: Currency/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Currency/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Currency/Create
        [HttpPost]
        public ActionResult Create(CurrencyModel model)
        {
            try
            {
                // TODO: Add insert logic here
                var currencyModel = InsuranceContext.Currencies.Single(where: $"Name='{model.CurrencyName}'");
                if (currencyModel == null)
                {
                
                    Currency currency = new Currency { Name=model.CurrencyName, Description= model.Description, CreatedOn=DateTime.Now};
                    InsuranceContext.Currencies.Insert(currency);


                }



                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Currency/Edit/5
        public ActionResult Edit(int id)
        {
            CurrencyModel model = new CurrencyModel();

            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                var currencyModel = InsuranceContext.Currencies.Single(where: $"Id='{id}'");
                if (currencyModel != null)
                {
                    model.CurrencyName = currencyModel.Name;
                    model.Description = currencyModel.Description;
                    model.Id = currencyModel.Id;
                }
            }

            return View(model);
        }

        // POST: Currency/Edit/5
        [HttpPost]
        public ActionResult Edit(CurrencyModel model)
        {
            try
            {
                // TODO: Add update logic here
                var currencyModel = InsuranceContext.Currencies.Single(where: $"Id='{model.Id}'");

                if (currencyModel != null)
                {
                    if(CheckCurrencyExist(currencyModel.Name, model.CurrencyName))
                    {
                        currencyModel.Name = model.CurrencyName;
                        currencyModel.Description = model.Description;
                        InsuranceContext.Currencies.Update(currencyModel);
                    }               
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }



        private bool CheckCurrencyExist(string oldCurrency, string newCurrency)
        {
            if (oldCurrency == newCurrency)
            {
                return true;
            }
            else
            {
                var dbCurrency = InsuranceContext.Currencies.Single(where: $"Name = '" + newCurrency + "'");

                if (dbCurrency != null)
                {
                    return false;
                }
            }
            return true;
        }

        // GET: Currency/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Currency/Delete/5
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
    }
}
