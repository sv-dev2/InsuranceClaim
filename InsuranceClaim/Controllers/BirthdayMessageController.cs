using AutoMapper;
using Insurance.Domain;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class BirthdayMessageController : Controller
    {
        // GET: BirthdayMessage
        public ActionResult Index()
        {
            return View();
        }
 
        [HttpGet]
        public ActionResult SendBirthdayMessage()
        {
            var record = InsuranceContext.BirthdayMessages.All().FirstOrDefault();

            if (record != null)
            {
                var model = Mapper.Map<BirthdayMessage, BirthdayMessageModel>(record);
                return View(model);
            }
            return View();
        }

        [HttpPost]
        public ActionResult SendBirthdayMessage(BirthdayMessageModel Model)
        {
            if (ModelState.IsValid)
            {
                bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                string userid = "";
                var recordExist = InsuranceContext.BirthdayMessages.All().FirstOrDefault();
                userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                var customer = InsuranceContext.Customers.Single(where: $"UserId = '{userid}'");
                if (recordExist != null)
                {
                    if (userLoggedin)
                    {
                        var dbModel = Mapper.Map<BirthdayMessageModel, BirthdayMessage>(Model);
                        var record = InsuranceContext.BirthdayMessages.Single(where: $"Id = '{recordExist.Id}'");
                        dbModel.ModifiedBy = customer.Id;
                        dbModel.ModifiedOn = DateTime.Now;
                        dbModel.Id = recordExist.Id;
                        dbModel.CreatedOn = Convert.ToDateTime(record.CreatedOn);
                        InsuranceContext.BirthdayMessages.Update(dbModel);
                        return RedirectToAction("SendBirthdayMessage");
                    }
                }

                else
                {
                    var dbModel = Mapper.Map<BirthdayMessageModel, BirthdayMessage>(Model);
                    dbModel.CreatedBy = customer.Id;
                    dbModel.CreatedOn = DateTime.Now;
                    InsuranceContext.BirthdayMessages.Insert(dbModel);
                    return RedirectToAction("SendBirthdayMessage");
                }
            }
            return View();
        }
    }
}