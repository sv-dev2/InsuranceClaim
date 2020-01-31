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



    }
}