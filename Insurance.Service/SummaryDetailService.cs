using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain;
using AutoMapper;
using System.IO;
using System.Web;
using System.Globalization;

namespace Insurance.Service
{
    public class SummaryDetailService
    {
        public VehicleDetail GetVehicleInformation(int vehicleId)
        {
            var vehicle = InsuranceContext.VehicleDetails.Single(vehicleId);
            return vehicle;
        }

        public Int32 getNewDebitNote()
        {
            var vehicle = InsuranceContext.SummaryDetails.Max("id");

            if (vehicle != null)
            {
                return Convert.ToInt32(vehicle) + 1;
            }
            else
            {
                return 1;
            }
        }


        public List<Currency> GetAllCurrency()
        {
            return InsuranceContext.Currencies.All().ToList();
        }


        public string GetCurrencyName(List<Currency> currenyList, int? currencyId)
        {
            var currencyDetails = currenyList.FirstOrDefault(c => c.Id == currencyId);
            if (currencyDetails != null)
                return currencyDetails.Name;
            else
                return "USD";
        }

        public ICEcashTokenResponse CheckSessionExpired()
        {
            Insurance.Service.ICEcashService ICEcashService = new Insurance.Service.ICEcashService();
            ICEcashTokenResponse tokenObject = new ICEcashTokenResponse();
            if (HttpContext.Current.Session["ICEcashToken"] != null)
            {
                var icevalue = (ICEcashTokenResponse)HttpContext.Current.Session["ICEcashToken"];
                string format = "yyyyMMddHHmmss";
                var IceDateNowtime = DateTime.Now;
                var IceExpery = DateTime.ParseExact(icevalue.Response.ExpireDate, format, CultureInfo.InvariantCulture);
                if (IceDateNowtime > IceExpery)
                {
                    ICEcashService.getToken();
                }
                tokenObject = (ICEcashTokenResponse)HttpContext.Current.Session["ICEcashToken"];
            }
            else
            {
                ICEcashService.getToken();
                tokenObject = (ICEcashTokenResponse)HttpContext.Current.Session["ICEcashToken"];
            }
            return tokenObject;
        }
        public static string GetLatestToken()
        {
            string token = "";
            var tokenDetails = InsuranceContext.TokenRequests.Single();
            if (tokenDetails != null)
            {
                token = tokenDetails.Token;
            }
            return token;
        }

        public static void UpdateToken(ICEcashTokenResponse tokenObject)
        {
            string format = "yyyyMMddHHmmss";
            var IceDateNowtime = DateTime.Now;
            var IceExpery = DateTime.ParseExact(tokenObject.Response.ExpireDate, format, CultureInfo.InvariantCulture);

            var tokenDetails = InsuranceContext.TokenRequests.Single();

            if (tokenDetails != null)
            {
                tokenDetails.Token = tokenObject.Response.PartnerToken;
                tokenDetails.ExpiryDate = IceExpery;
                tokenDetails.UpdatedOn = DateTime.Now;
                InsuranceContext.TokenRequests.Update(tokenDetails);
            }
            else
            {
                TokenRequest request = new TokenRequest { Token = tokenObject.Response.PartnerToken, ExpiryDate = IceExpery, UpdatedOn = DateTime.Now };
                InsuranceContext.TokenRequests.Insert(request);
            }





        }

        public static void WriteLog(string request, string response, string method, string vrn="")
        {
            string message = string.Format(" Time: {0}", DateTime.Now);
            message += Environment.NewLine;
            message += request;
            message += Environment.NewLine;
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";


            message += response;
            message += Environment.NewLine;
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += method;
            message += Environment.NewLine;
            message += Environment.NewLine;
            message += "-----------------------------------------------------------";
            message += vrn;



            string path = System.Web.HttpContext.Current.Server.MapPath("~/LogFile.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }

        //public static void WriteIceCashLog(string request, string response, string method, string vrn="")
        //{
        //    LogDetailTbl log = new LogDetailTbl();
        //    log.Request = request;
        //    log.Response = response;
        //    log.CreatedOn = DateTime.Now;
        //    log.Method = method;
        //    log.VRN = vrn;
        //    InsuranceContext.LogDetailTbls.Insert(log);
        //}

        public SummaryDetail GetSummaryDetail(int summaryId)
        {
            return InsuranceContext.SummaryDetails.Single(summaryId);
        }

        public List<SummaryVehicleDetail> GetSummaryVehicleList(int summaryId)
        {
            return InsuranceContext.SummaryVehicleDetails.All(where: $"SummaryDetailId={summaryId}").ToList();
        }

        public SummaryVehicleDetail GetSummaryVehicleDetails(int summaryId)
        {
            return InsuranceContext.SummaryVehicleDetails.Single(where: $"SummaryDetailId={summaryId}");
        }

        public List<SummaryVehicleDetail> GetSummaryVehicleDetailsByVehicle(int vehicleId)
        {
            return InsuranceContext.SummaryVehicleDetails.All(where: $"VehicleDetailsId={vehicleId}").ToList();
        }

        public int SaveSummaryDetails(SummaryDetail DbEntry)
        {
            int summaryId = 0;
            try
            {
                InsuranceContext.SummaryDetails.Insert(DbEntry);
                summaryId = DbEntry.Id;
            }
            catch (Exception ex)
            {

            }
            return summaryId;
        }


        public void UpdateSummaryDetail(SummaryDetail summaryDetail)
        {
            InsuranceContext.SummaryDetails.Update(summaryDetail);
        }

        public void DeleteSummaryVehicleDetails(SummaryVehicleDetail SummaryVehicleDetail)
        {
            InsuranceContext.SummaryVehicleDetails.Delete(SummaryVehicleDetail);
        }

        public void SaveSummaryVehicleDetails(SummaryVehicleDetail SummaryVehicleDetail)
        {
            InsuranceContext.SummaryVehicleDetails.Insert(SummaryVehicleDetail);
        }



    }
}
