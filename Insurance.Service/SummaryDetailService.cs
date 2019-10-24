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
                return  "USD";
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
            if(tokenDetails!=null)
            {
                token = tokenDetails.Token;
            }
            return token;
        }

        public static void UpdateToken( ICEcashTokenResponse tokenObject)
        {
            string format = "yyyyMMddHHmmss";
            var IceDateNowtime = DateTime.Now;
            var IceExpery = DateTime.ParseExact(tokenObject.Response.ExpireDate, format, CultureInfo.InvariantCulture);

            var tokenDetails = InsuranceContext.TokenRequests.Single();

            if(tokenDetails!=null)
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


       

        public void WriteLog(string error)
        {
            string message = string.Format("Error Time: {0}", DateTime.Now);
            message += error;
            message += "-----------------------------------------------------------";

            message += Environment.NewLine;




            string path = System.Web.HttpContext.Current.Server.MapPath("~/LogFile.txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(message);
                writer.Close();
            }
        }










    }
}
