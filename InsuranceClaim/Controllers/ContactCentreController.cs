using AutoMapper;
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
    public class ContactCentreController : Controller
    {
        // GET: ContactCentre
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RiskDetail(int? id = 1)
        {
            // summaryDetailId: it's represent to Qutation edit
         
            if (Session["SummaryDetailId"] != null)
            {
                SetValueIntoSession(Convert.ToInt32(Session["SummaryDetailId"]));
                Session["SummaryDetailId"] = null;
            }


            if (Session["CustomerDataModal"] == null)
            {
                // return RedirectToAction("Index", "CustomerRegistration");
                return Redirect("/CustomerRegistration/Index");
            }


            var service = new VehicleService();
            var viewModel = new RiskDetailModel();
            ViewBag.VehicleUsage = service.GetAllVehicleUsage();
            ViewBag.Products = InsuranceContext.Products.All(where: "Active = 'True' or Active is Null").ToList();
            ViewBag.PaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is Null").ToList();
            ViewBag.VehicleLicensePaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is Null").ToList();
            ViewBag.RadioLicensePaymentTermId = InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is Null").ToList();


            ViewBag.TaxClass = InsuranceContext.VehicleTaxClasses.All().ToList();
            ViewBag.AgentCommission = service.GetAgentCommission();
            var makers = service.GetMakers();
            ViewBag.CoverType = service.GetCoverType();


            var data1 = (from p in InsuranceContext.BusinessSources.All().ToList()
                         join f in InsuranceContext.SourceDetails.All().ToList()
                         on p.Id equals f.BusinessId where f.IsDeleted == true
                         select new
                         {
                             Value = f.Id,
                             Text = p.Source
                         }).ToList();

            List<SelectListItem> listdata = new List<SelectListItem>();
            foreach (var item in data1)
            {
                SelectListItem sli = new SelectListItem();
                sli.Value = Convert.ToString(item.Value);
                sli.Text = item.Text;
                listdata.Add(sli);
            }
            ViewBag.Sources = new SelectList(listdata, "Value", "Text");
            //ViewBag.Sources = InsuranceContext.BusinessSources.All();
            ViewBag.Currencies = InsuranceContext.Currencies.All(where: $"IsActive = 'True'");
            // viewModel.CurrencyId = 7; // default "RTGS$" selected // for test server
             viewModel.CurrencyId = 6; // default "RTGS$" selected // for live server
            ViewBag.Makers = makers;

            if (makers.Count > 0)
            {
                var model = service.GetModel(makers.FirstOrDefault().MakeCode);
                ViewBag.Model = model;
            }


            var eExcessTypeData = from eExcessType e in Enum.GetValues(typeof(eExcessType))
                                  select new
                                  {
                                      ID = (int)e,
                                      Name = e.ToString()
                                  };

            ViewBag.eExcessTypeData = new SelectList(eExcessTypeData, "ID", "Name");
            if (TempData["ViewModel"] != null)
            {
                viewModel = (RiskDetailModel)TempData["ViewModel"];
                return View(viewModel);
            }



            int RadioLicenseCosts = 0;
          // int RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            var PolicyData = (PolicyDetail)Session["PolicyData"];
            //Id is policyid from Policy detail table
          //  var viewModel = new RiskDetailModel();
           
           

            viewModel.VehicleUsage = 0;
            viewModel.NumberofPersons = 0;
            viewModel.AddThirdPartyAmount = 0.00m;
            viewModel.RadioLicenseCost = Convert.ToDecimal(RadioLicenseCosts);
           
          
        
            viewModel.isUpdate = false;
            //TempData["Policy"] = service.GetPolicy(id);
            

            viewModel.NoOfCarsCovered = 1;
            if (Session["VehicleDetails"] != null)
            {
                var list = (List<RiskDetailModel>)Session["VehicleDetails"];
                viewModel.NoOfCarsCovered = list.Count + 1;

            }

            if (id > 0)
            {
                var list = (List<RiskDetailModel>)Session["VehicleDetails"];
                if (list != null && list.Count > 0 && (list.Count >= id))
                {
                    var data = (RiskDetailModel)list[Convert.ToInt32(id - 1)];
                    if (data != null)
                    {
                        viewModel.AgentCommissionId = data.AgentCommissionId;
                        viewModel.ChasisNumber = data.ChasisNumber;
                        viewModel.CoverEndDate = data.CoverEndDate;
                        viewModel.CoverNoteNo = data.CoverNoteNo;
                        viewModel.CoverStartDate = data.CoverStartDate;
                        viewModel.CoverTypeId = data.CoverTypeId;
                        viewModel.CubicCapacity = (int)Math.Round(data.CubicCapacity.Value, 0);
                        viewModel.CustomerId = data.CustomerId;
                        viewModel.EngineNumber = data.EngineNumber;
                        //viewModel.Equals = data.Equals;
                        viewModel.Excess = (int)Math.Round(data.Excess, 0);
                        viewModel.ExcessType = data.ExcessType;
                        viewModel.MakeId = data.MakeId;
                        viewModel.ModelId = data.ModelId;
                        viewModel.NoOfCarsCovered = id;
                        viewModel.OptionalCovers = data.OptionalCovers;
                        viewModel.PolicyId = data.PolicyId;
                        viewModel.Premium = data.Premium;
                        viewModel.PremiumWithDiscount = data.Premium + data.Discount;

                        viewModel.RadioLicenseCost = (int)Math.Round(data.RadioLicenseCost == null ? 0 : data.RadioLicenseCost.Value, 0);
                        viewModel.Rate = data.Rate;
                        viewModel.RegistrationNo = data.RegistrationNo;
                        viewModel.StampDuty = data.StampDuty;
                        viewModel.SumInsured = (int)Math.Round(data.SumInsured == null ? 0 : data.SumInsured.Value, 0);
                        viewModel.VehicleColor = data.VehicleColor;
                        viewModel.VehicleUsage = data.VehicleUsage;
                        viewModel.VehicleYear = data.VehicleYear;
                        viewModel.Id = data.Id;
                        viewModel.ZTSCLevy = data.ZTSCLevy;
                        viewModel.NumberofPersons = data.NumberofPersons;
                        viewModel.PassengerAccidentCover = data.PassengerAccidentCover;
                        viewModel.IsLicenseDiskNeeded = data.IsLicenseDiskNeeded;
                        viewModel.ExcessBuyBack = data.ExcessBuyBack;
                        viewModel.RoadsideAssistance = data.RoadsideAssistance;
                        viewModel.MedicalExpenses = data.MedicalExpenses;
                        viewModel.Addthirdparty = data.Addthirdparty;
                        viewModel.AddThirdPartyAmount = data.AddThirdPartyAmount;
                        viewModel.ExcessAmount = data.ExcessAmount;
                        viewModel.ExcessAmount = data.ExcessAmount;
                        viewModel.ExcessBuyBackAmount = data.ExcessBuyBackAmount;
                        viewModel.MedicalExpensesAmount = data.MedicalExpensesAmount;
                        viewModel.MedicalExpensesPercentage = data.MedicalExpensesPercentage;
                        viewModel.PassengerAccidentCoverAmount = data.PassengerAccidentCoverAmount;
                        viewModel.PassengerAccidentCoverAmountPerPerson = data.PassengerAccidentCoverAmountPerPerson;
                        viewModel.PaymentTermId = data.PaymentTermId;
                        viewModel.ProductId = data.ProductId;
                        viewModel.IncludeRadioLicenseCost = data.IncludeRadioLicenseCost;
                        viewModel.IncludeLicenseFee = data.IncludeLicenseFee;
                        viewModel.ZinaraLicensePaymentTermId = data.ZinaraLicensePaymentTermId;
                        viewModel.RadioLicensePaymentTermId = data.RadioLicensePaymentTermId;

                        viewModel.RenewalDate = data.RenewalDate;
                        viewModel.TransactionDate = data.TransactionDate;
                        viewModel.AnnualRiskPremium = data.AnnualRiskPremium;
                        viewModel.TermlyRiskPremium = data.TermlyRiskPremium;
                        viewModel.QuaterlyRiskPremium = data.QuaterlyRiskPremium;
                        viewModel.Discount = data.Discount;
                        viewModel.VehicleLicenceFee = Convert.ToDecimal(data.VehicleLicenceFee);
                        viewModel.InsuranceId = data.InsuranceId;
                        viewModel.BusinessSourceDetailId = data.BusinessSourceDetailId;
                        viewModel.CurrencyId = data.CurrencyId;

                        viewModel.isUpdate = true; //commented on "31 oct"
                        //viewModel.isUpdate = false; // 02_feb_2019
                 
                        viewModel.vehicleindex = Convert.ToInt32(id);
                        viewModel.TaxClassId = data.TaxClassId;

                        var ser = new VehicleService();
                        var model = ser.GetModel(data.MakeId);
                        ViewBag.Model = model;
                    }
                }
            }


            return View(viewModel);
        }

        public void SetValueIntoSession(int summaryId)
        {
            Session["ICEcashToken"] = null;
            Session["issummaryformvisited"] = true;

            Session["SummaryDetailId"] = summaryId;

            var summaryDetail = InsuranceContext.SummaryDetails.Single(summaryId);
            var SummaryVehicleDetails = InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summaryId}").ToList();
            var vehicle = InsuranceContext.VehicleDetails.Single(SummaryVehicleDetails[0].VehicleDetailsId);
            var policy = InsuranceContext.PolicyDetails.Single(vehicle.PolicyId);
            var product = InsuranceContext.Products.Single(Convert.ToInt32(policy.PolicyName));
  

            Session["PolicyData"] = policy;

            List<RiskDetailModel> listRiskDetail = new List<RiskDetailModel>();
            foreach (var item in SummaryVehicleDetails)
            {

                // var _vehicle = InsuranceContext.VehicleDetails.Single(item.VehicleDetailsId); after renew qutation
                var _vehicle = InsuranceContext.VehicleDetails.Single(where: "id=" + item.VehicleDetailsId + " and IsActive=1");

                if(_vehicle!=null)
                {
                    RiskDetailModel riskDetail = Mapper.Map<VehicleDetail, RiskDetailModel>(_vehicle);
                    listRiskDetail.Add(riskDetail);
                }
                
                
            }
            Session["VehicleDetails"] = listRiskDetail;

            SummaryDetailModel summarymodel = Mapper.Map<SummaryDetail, SummaryDetailModel>(summaryDetail);
            summarymodel.Id = summaryDetail.Id;
            Session["SummaryDetailed"] = summarymodel;

        }


        public ActionResult GetRadioLicenseCost(int? id, int productId)
        {
            JsonResult jsonResult = new JsonResult();
            //RadioLicenseCostCommercialvehicles

            jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            int RadioLicenseCosts = 0;
            if (productId == 3)// for  Commercial vehicles
                RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCostCommercialvehicles").Select(x => x.value).FirstOrDefault());
            else
                RadioLicenseCosts = Convert.ToInt32(InsuranceContext.Settings.All().Where(x => x.keyname == "RadioLicenseCost").Select(x => x.value).FirstOrDefault());
            


            //if (id == (int)ePaymentTerm.Annual)
            //{
            //    jsonResult.Data = RadioLicenseCosts;

            //}
            //if (id == (int)ePaymentTerm.Termly)
            //{
            //    jsonResult.Data = RadioLicenseCosts / 3;
            //}


            // defuault will be annual
            switch (id)
            {

                case 4:
                    RadioLicenseCosts = RadioLicenseCosts / 3;
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                    RadioLicenseCosts = (RadioLicenseCosts / 3) * 2;
                    break;
                //case 5:
                //    RadioLicenseCosts = RadioLicenseCosts / 3;
                //    break;
                //case 6:
                //    RadioLicenseCosts = RadioLicenseCosts / 3;
                //    break;
                //case 7:
                //    RadioLicenseCosts = RadioLicenseCosts / 3;
                //    break;
                case 9:
                case 10:
                case 11:
                    RadioLicenseCosts = RadioLicenseCosts;
                    break;        
            }


            jsonResult.Data = RadioLicenseCosts;


            return jsonResult;
        }

    }
}