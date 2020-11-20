using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Insurance.Domain;
using InsuranceClaim.Models;

namespace InsuranceClaim.Controllers
{
    public class MobileApiController : Controller
    {
        // GET: MobileApi
        public ActionResult Index()
        {

            return View();
        }

        public JsonResult GetVehicleDetails(string vrn)
        {
            PayLaterPolicyModel model = new PayLaterPolicyModel();

            string query = " select PolicyDetail.PolicyNumber, PolicyDetail.Id as PolicyId, Customer.FirstName + ' ' + Customer.LastName as CustomerName, ";
            query += " VehicleDetail.RegistrationNo as RegistrationNo, VehicleMake.MakeDescription, VehicleDetail.Id as VehicleID, ";
            query += " VehicleModel.ModelDescription, SummaryDetail.TotalPremium, SummaryDetail.Id as SummaryDetailId, ";
            query += " PaymentInformation.Id as PaymentInformationId,  case when ";
            query += " PaymentMethod.Name <> 'PayLater' then 'Paid' else 'PayLater' end as PaymentStatus ";
            query += " from PolicyDetail join Customer on PolicyDetail.CustomerId = Customer.Id ";
            query += " join VehicleDetail on VehicleDetail.PolicyId = PolicyDetail.Id   ";
            query += " join SummaryVehicleDetail on VehicleDetail.Id= SummaryVehicleDetail.VehicleDetailsId  ";
            query += " join SummaryDetail on SummaryVehicleDetail.SummaryDetailId = SummaryDetail.Id  ";
            query += " left join VehicleMake on VehicleDetail.MakeId = VehicleMake.MakeCode  left ";
            query += " join VehicleModel on VehicleDetail.ModelId = VehicleModel.ModelCode ";
            query += " join PaymentInformation on SummaryDetail.Id = PaymentInformation.SummaryDetailId ";
            query += " join PaymentMethod on SummaryDetail.PaymentMethodId = PaymentMethod.Id  where VehicleDetail.RegistrationNo=" + vrn + " and VehicleDetail.isactive=1";

            var result = InsuranceContext.Query(query).Select(c => new PayLaterPolicyDetail()
            {
                VehicleID = c.VehicleID,
                PolicyId = c.PolicyId,
                SummaryDetailId = c.SummaryDetailId,
                PaymentInformationId = c.PaymentInformationId,
                PolicyNumber = c.PolicyNumber,
                CustomerName = c.CustomerName,
                RegistrationNo = c.RegistrationNo,
                MakeDescription = c.MakeDescription,
                ModelDescription = c.ModelDescription,
                Vehicle = c.MakeDescription + ' ' + c.ModelDescription,
                TotalPremium = c.TotalPremium
            }).FirstOrDefault();


            if(result==null)
            {
                result = new PayLaterPolicyDetail();
                result.ErrorMsg = "Records not found";
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }



    }
}