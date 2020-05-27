using Insurance.Domain;
using Insurance.Service;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class InsuranceHomeController : Controller
    {
        // GET: InsuranceHome
        public ActionResult Index()
        {

            //ICEcashService obj = new ICEcashService();
            //obj.getToken();
            //obj.RequestQuote(objVehicles);
            //test    
            //List<string> _attachementss = new List<string>();

            //Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
            //objEmailService.SendEmail("chandan.kumar@kindlebit.com", "", "", "Receipt Module", "test mail", _attachementss);

           return View();
        }

        public ActionResult DownloadLogFile()
        {
            string path = Server.MapPath("/LogFile.txt"); 
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            string fileName = "LogFile.txt";
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }


        [HttpPost]
        public JsonResult SetValueIntoSession(string regNo, string nationalId)
        {
            Session["HomeRegNo"] = regNo;
            Session["HomeNationalId"] = nationalId;
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SendCallBackEmail(string UserName, string UserPhone)
        {
            string userRegisterationEmailPath = "/Views/Shared/EmaiTemplates/UserCallBack.cshtml";
            string EmailBody2 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(userRegisterationEmailPath));

            var Body = EmailBody2.Replace("#DATE#", DateTime.Now.ToShortDateString())
               .Replace("#Name#", UserName)
               .Replace("#Phone#", UserPhone);

            List<string> _attachements = new List<string>();
            _attachements.Add("");
            //callbackemail

            string email = System.Configuration.ConfigurationManager.AppSettings["callbackemail"];

            Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
            objEmailService.SendEmail(email, "", "", "Request for call me back", Body, _attachements);



            return Json("", JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SearchVrn(SearchVRNModel model)
        {
            if(!string.IsNullOrWhiteSpace(model.RegistrationNum))
            {
                return RedirectToAction("Index", "WebCustomer", new { reg = model.RegistrationNum, NationalId = model.NationalId });
            }
            else
            {
                return View("Index", model);
            }
          
        }



    }
}