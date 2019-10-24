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



    }
}