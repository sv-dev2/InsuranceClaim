using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Configuration;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using InsuranceClaim.Models;
using Newtonsoft.Json;
using System.Web;
using Insurance.Domain;

namespace Insurance.Service
{
    public class ICEcashService
    {
        // SendBox
        public static string PSK = "127782435202916376850511";
        public static string LiveIceCashApi = "http://api-test.icecash.com/request/20523588";

        // Live
        //public static string PSK = "565205790573235453203546";
        //public static string LiveIceCashApi = "https://api.icecash.co.zw/request/20350763";

        private static string GetSHA512(string text)
        {
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            byte[] message = UE.GetBytes(text);
            SHA512Managed hashString = new SHA512Managed();
            string encodedData = Convert.ToBase64String(message);
            string hex = "";
            hashValue = hashString.ComputeHash(UE.GetBytes(encodedData));
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        public static string SHA512(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            using (var hash = System.Security.Cryptography.SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new System.Text.StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        public ICEcashTokenResponse getToken()
        {
            ICEcashTokenResponse json = null;
            //json = new ICEcashTokenResponse() { Date = "", PartnerReference = "", Version = "", Response = new TokenReposone() { Result = "1", Message = "", ExpireDate = "", Function = "", PartnerToken = "000000000374163" } };
            //return json;

            try
            {
                //string json = "%7B%20%20%20%22PartnerReference%22%3A%20%228eca64cb-ccf8-4304-a43f-a6eaef441918%22%2C%0A%20%20%20%20%22Date%22%3A%20%22201801080615165001%22%2C%0A%20%20%20%20%22Version%22%3A%20%222.0%22%2C%0A%20%20%20%20%22Request%22%3A%20%7B%0A%20%20%20%20%20%20%20%20%22Function%22%3A%20%22PartnerToken%22%7D%7D";
                //string PSK = "127782435202916376850511";
                string _json = "";//"{'PartnerReference':'" + Convert.ToString(Guid.NewGuid()) + "','Date':'" + DateTime.Now.ToString("yyyyMMddhhmmss") + "','Version':'2.0','Request':{'Function':'PartnerToken'}}";
                Arguments objArg = new Arguments();
                objArg.PartnerReference = Guid.NewGuid().ToString();
                objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
                objArg.Version = "2.0";
                objArg.Request = new FunctionObject { Function = "PartnerToken" };

                _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

                //string  = json.Reverse()
                string reversejsonString = new string(_json.Reverse().ToArray());
                string reversepartneridString = new string(PSK.Reverse().ToArray());

                string concatinatedString = reversejsonString + reversepartneridString;

                byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

                string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

                string GetSHA512encrypted = SHA512(returnValue);

                string MAC = "";

                for (int i = 0; i < 16; i++)
                {
                    MAC += GetSHA512encrypted.Substring((i * 8), 1);
                }

                MAC = MAC.ToUpper();


                ICERootObject objroot = new ICERootObject();
                objroot.Arguments = objArg;
                objroot.MAC = MAC;
                objroot.Mode = "SH";

                var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);


                JObject jsonobject = JObject.Parse(data);

                var client = new RestClient(LiveIceCashApi);
                //  var client = new RestClient(LiveIceCashApi);
                var request = new RestRequest(Method.POST);
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);

                json = JsonConvert.DeserializeObject<ICEcashTokenResponse>(response.Content);

                HttpContext.Current.Session["ICEcashToken"] = json;


                //   SummaryDetailService.WriteIceCashLog(data, response.Content, "PartnerToken_Web");
                SummaryDetailService.WriteLog(data, response.Content, "PartnerToken");


            }
            catch (Exception ex)
            {
                json = new ICEcashTokenResponse() { Date = "", PartnerReference = "", Version = "", Response = new TokenReposone() { Result = "0", Message = "A Connection Error Occured ! Please add manually", ExpireDate = "", Function = "", PartnerToken = "" } };
            }

            return json;
        }

        //public ResultRootObject checkVehicleExists(List<RiskDetailModel> listofvehicles, string PartnerToken, string PartnerReference)
        //{
        //    //string PSK = "127782435202916376850511";
        //    string _json = "";

        //    List<VehicleObject> obj = new List<VehicleObject>();

        //    var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];

        //    if (CustomerInfo == null)
        //    {
        //        CustomerInfo = (CustomerModel)HttpContext.Current.Session["ReCustomerDataModal"];
        //    }


        //    foreach (var item in listofvehicles)
        //    {
        //        obj.Add(new VehicleObject { VRN = item.RegistrationNo, DurationMonths = (item.PaymentTermId == 1 ? 12 : item.PaymentTermId), VehicleValue = 0, YearManufacture = 0, InsuranceType = 0, VehicleType = 0, TaxClass = 0, Make = "", Model = "", EntityType = "", Town = CustomerInfo.City, Address1 = CustomerInfo.AddressLine1, Address2 = CustomerInfo.AddressLine2, CompanyName = "", FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, IDNumber = CustomerInfo.NationalIdentificationNumber, MSISDN = CustomerInfo.CountryCode + CustomerInfo.PhoneNumber });
        //    }

        //    QuoteArguments objArg = new QuoteArguments();
        //    objArg.PartnerReference = Guid.NewGuid().ToString();
        //    objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
        //    objArg.Version = "2.0";
        //    objArg.PartnerToken = PartnerToken;
        //    objArg.Request = new QuoteFunctionObject { Function = "TPIQuote", Vehicles = obj };

        //    _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

        //    //string  = json.Reverse()
        //    string reversejsonString = new string(_json.Reverse().ToArray());
        //    string reversepartneridString = new string(PSK.Reverse().ToArray());

        //    string concatinatedString = reversejsonString + reversepartneridString;

        //    byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

        //    string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

        //    string GetSHA512encrypted = SHA512(returnValue);

        //    string MAC = "";

        //    for (int i = 0; i < 16; i++)
        //    {
        //        MAC += GetSHA512encrypted.Substring((i * 8), 1);
        //    }

        //    MAC = MAC.ToUpper();

        //    ICEQuoteRequest objroot = new ICEQuoteRequest();
        //    objroot.Arguments = objArg;
        //    objroot.MAC = MAC;
        //    objroot.Mode = "SH";

        //    var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);
        //    JObject jsonobject = JObject.Parse(data);


        //    var client = new RestClient(LiveIceCashApi);
        //    //var client = new RestClient(LiveIceCashApi);
        //    var request = new RestRequest(Method.POST);
        //    request.AddHeader("cache-control", "no-cache");
        //    request.AddHeader("content-type", "application/x-www-form-urlencoded");
        //    request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
        //    IRestResponse response = client.Execute(request);

        //    ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

        //    return json;
        //}

        public string check_hash(IceCashModel model)
        {

            string _json = "";//"{'PartnerReference':'" + Convert.ToString(Guid.NewGuid()) + "','Date':'" + DateTime.Now.ToString("yyyyMMddhhmmss") + "','Version':'2.0','Request':{'Function':'PartnerToken'}}";
            //Arguments objArg = new Arguments();

            PaymentArugument objArg = new PaymentArugument();

            objArg.partner_id = model.partner_id;
            objArg.amount = model.amount;
            objArg.client_reference = model.client_reference;
            objArg.success_url = model.success_url;
            objArg.failed_url = model.failed_url;
            objArg.results_url = model.results_url;
            objArg.details = model.details;

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.ToArray());


            string concatinatedString = reversejsonString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            //string GetSHA512encrypted = SHA512(returnValue);

            //string MAC = "";

            //for (int i = 0; i < 16; i++)
            //{
            //    MAC += GetSHA512encrypted.Substring((i * 8), 1);
            //}

            //MAC = MAC.ToUpper();


            //ICERootObject objroot = new ICERootObject();
            //objroot.Arguments = objArg;
            //objroot.MAC = MAC;
            //objroot.Mode = "SH";

            return returnValue;
        }

        public ResultRootObject checkVehicleExists(List<RiskDetailModel> listofvehicles, string PartnerToken, string PartnerReference)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";

            List<VehicleObject> obj = new List<VehicleObject>();

            var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];

            if (CustomerInfo == null)
            {
                CustomerInfo = (CustomerModel)HttpContext.Current.Session["ReCustomerDataModal"];
            }

            int durationMonth = 0;


            foreach (var item in listofvehicles)
            {
                if (item.PaymentTermId == 1)
                    durationMonth = 12;
                else
                    durationMonth = item.PaymentTermId;
                // durationMonth = GetMonthKey(item.PaymentTermId);
                // do


                if (item.RegistrationNo == "TBA")
                {
                    string MSISDN = "+263" + CustomerInfo.PhoneNumber;
                    obj.Add(new VehicleObject { VRN = item.RegistrationNo, IDNumber = CustomerInfo.NationalIdentificationNumber, FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, MSISDN = MSISDN, Address1 = UserService.ReplaceSpecialChracter(CustomerInfo.AddressLine1), Town = UserService.ReplaceSpecialChracter(CustomerInfo.AddressLine2), EntityType = "Corporate", DurationMonths = durationMonth, InsuranceType = item.CoverTypeId == null ? 0 : item.CoverTypeId.Value, VehicleType = item.ProductId, Make = item.MakeId, Model = item.ModelId, YearManufacture = item.VehicleYear == null ? 0 : item.VehicleYear.Value, TaxClass = item.TaxClassId.ToString() });
                }
                else
                {
                    obj.Add(new VehicleObject { VRN = item.RegistrationNo, IDNumber = CustomerInfo.NationalIdentificationNumber, FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, MSISDN = CustomerInfo.CountryCode + CustomerInfo.PhoneNumber, Address1 = CustomerInfo.AddressLine1, Town = CustomerInfo.AddressLine2, EntityType = "Personal", DurationMonths = durationMonth, InsuranceType = item.CoverTypeId == null ? 0 : item.CoverTypeId.Value, VehicleType = item.ProductId, Make = item.MakeId, Model = item.ModelId, TaxClass = item.TaxClassId.ToString(), YearManufacture = item.VehicleYear == null ? 0 : item.VehicleYear.Value });
                }
            }


            // need to uncomment
            //foreach (var item in listofvehicles)
            //{
            //    obj.Add(new VehicleObject { VRN = item.RegistrationNo, DurationMonths = (item.PaymentTermId == 1 ? 12 : item.PaymentTermId), VehicleValue = 0, YearManufacture = 0, InsuranceType = 0, VehicleType = 0, TaxClass = 0, Make = "", Model = "", EntityType = "", Town = CustomerInfo.City, Address1 = CustomerInfo.AddressLine1, Address2 = CustomerInfo.AddressLine2, CompanyName = "", FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, IDNumber = CustomerInfo.NationalIdentificationNumber, MSISDN = CustomerInfo.CountryCode + CustomerInfo.PhoneNumber });
            //}

            QuoteArguments objArg = new QuoteArguments();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = new QuoteFunctionObject { Function = "TPIQuote", Vehicles = obj };

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            ICEQuoteRequest objroot = new ICEQuoteRequest();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);
            JObject jsonobject = JObject.Parse(data);


            var client = new RestClient(LiveIceCashApi);
            //var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);
            // SummaryDetailService.WriteIceCashLog(data, response.Content, "TPIQuote_Web", listofvehicles[0].RegistrationNo);

            SummaryDetailService.WriteLog(data, response.Content, "TPIQuote");

            return json;
        }

        public ResultRootObject RequestQuote(string PartnerToken, string RegistrationNo, string suminsured, string make, string model, int PaymentTermId, int VehicleYear, int CoverTypeId, int VehicleUsage, string PartnerReference, DateTime CoverStartDate, DateTime CoverEndDate, string TaxClass)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";


            make = RemoveSpecialChars(make);
            model = RemoveSpecialChars(model);

            var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];

            if (CustomerInfo == null)
            {
                CustomerInfo = (CustomerModel)HttpContext.Current.Session["ReCustomerDataModal"];// if renew
            }

            List<VehicleObject> obj = new List<VehicleObject>();

            // int paymentTermId = GetMonthKey(PaymentTermId);


            //  TaxClass = 1,

            //foreach (var item in listofvehicles)
            //{

            //  obj.Add(new VehicleObject { VRN = RegistrationNo, DurationMonths = (PaymentTermId == 1 ? 12 : PaymentTermId), VehicleValue = Convert.ToInt32(suminsured), YearManufacture = Convert.ToInt32(VehicleYear), InsuranceType = Convert.ToInt32(CoverTypeId), VehicleType = Convert.ToInt32(VehicleUsage),  Make = make, Model = model, EntityType = "", Town = CustomerInfo.City, Address1 = CustomerInfo.AddressLine1, Address2 = CustomerInfo.AddressLine2, CompanyName = "", FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, IDNumber = CustomerInfo.NationalIdentificationNumber, MSISDN = CustomerInfo.CountryCode + CustomerInfo.PhoneNumber, StartDate= CoverStartDate, EndDate=CoverEndDate });

            obj.Add(new VehicleObject { VRN = RegistrationNo, DurationMonths = (PaymentTermId == 1 ? 12 : PaymentTermId), VehicleValue = Convert.ToInt32(suminsured), YearManufacture = Convert.ToInt32(VehicleYear), InsuranceType = Convert.ToInt32(CoverTypeId), VehicleType = Convert.ToInt32(VehicleUsage), Make = make, Model = model, EntityType = "", Town = CustomerInfo.City, Address1 = UserService.ReplaceSpecialChracter(CustomerInfo.AddressLine1), Address2 = UserService.ReplaceSpecialChracter(CustomerInfo.AddressLine2), CompanyName = "", FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, IDNumber = CustomerInfo.NationalIdentificationNumber, MSISDN = CustomerInfo.CountryCode + CustomerInfo.PhoneNumber, TaxClass = TaxClass });

            // obj.Add(new VehicleObject { VRN = RegistrationNo, DurationMonths = paymentTermId , VehicleValue = Convert.ToInt32(suminsured), YearManufacture = Convert.ToInt32(VehicleYear), InsuranceType = Convert.ToInt32(CoverTypeId), VehicleType = Convert.ToInt32(VehicleUsage), TaxClass = 1, Make = make, Model = model, EntityType = "", Town = CustomerInfo.City, Address1 = CustomerInfo.AddressLine1, Address2 = CustomerInfo.AddressLine2, CompanyName = "", FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, IDNumber = CustomerInfo.NationalIdentificationNumber, MSISDN = CustomerInfo.CountryCode + CustomerInfo.PhoneNumber });

            //}

            QuoteArguments objArg = new QuoteArguments();
            objArg.PartnerReference = Guid.NewGuid().ToString(); ;
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = new QuoteFunctionObject { Function = "TPIQuote", Vehicles = obj };

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            ICEQuoteRequest objroot = new ICEQuoteRequest();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient(LiveIceCashApi);
            //   var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

            SummaryDetailService.WriteLog(data, response.Content, "TPIQuote");



            if (json.Response.Quotes != null && json.Response.Quotes.Count > 0)
            {
                if (json.Response.Quotes[0].Policy != null)
                {
                    var Setting = InsuranceContext.Settings.All();
                    var DiscountOnRenewalSettings = Setting.Where(x => x.keyname == "Discount On Renewal").FirstOrDefault();
                    var premium = Convert.ToDecimal(json.Response.Quotes[0].Policy.CoverAmount);
                    switch (PaymentTermId)
                    {
                        case 1:
                            var AnnualRiskPremium = premium;
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                            {
                                json.LoyaltyDiscount = ((AnnualRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                            }
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                            {
                                json.LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                            }
                            break;
                        case 3:
                            var QuaterlyRiskPremium = premium;
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                            {
                                json.LoyaltyDiscount = ((QuaterlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                            }
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                            {
                                json.LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                            }
                            break;
                        case 4:
                            var TermlyRiskPremium = premium;
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                            {
                                json.LoyaltyDiscount = ((TermlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                            }
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                            {
                                json.LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                            }
                            break;
                    }
                }
            }
            return json;
        }

        public ResultRootObject TPILICQuote(string PartnerToken, string RegistrationNo, string suminsured, string make, string model, int PaymentTermId, int VehicleYear, int CoverTypeId, int VehicleType, string PartnerReference, DateTime CoverStartDate, DateTime CoverEndDate, string taxClassId, bool VehilceLicense, bool RadioLicense)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";
            make = RemoveSpecialChars(make);
            model = RemoveSpecialChars(model);

            var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];
            if (CustomerInfo == null)
            {
                CustomerInfo = (CustomerModel)HttpContext.Current.Session["ReCustomerDataModal"];// if renew
            }

            List<VehicleCombObject> obj = new List<VehicleCombObject>();

            int LicFrequencyTerm = GetMonthKey(PaymentTermId);
            string RadioTVUsage = "1"; // for private car
            string RadioTVFreeQuency = null;

            if (VehicleType == 0)
                RadioTVUsage = "1";
            else if (VehicleType == 3 || VehicleType == 11) // fr 
                RadioTVUsage = "2";

            string clientIdType = "1";
            if (CustomerInfo.IsCorporate)
                clientIdType = "2";

            if (!RadioLicense)
            {
                RadioTVUsage = null;
                RadioTVFreeQuency = null;
            }
            else
                RadioTVFreeQuency = LicFrequencyTerm.ToString();





            obj.Add(new VehicleCombObject
            {
                VRN = RegistrationNo,
                DurationMonths = (PaymentTermId == 1 ? 12 : PaymentTermId).ToString(),
                InsuranceType = CoverTypeId.ToString(),
                VehicleType = VehicleType.ToString(),
                Address1 = UserService.ReplaceSpecialChracter(CustomerInfo.AddressLine1),
                Address2 = UserService.ReplaceSpecialChracter(CustomerInfo.AddressLine2),
                FirstName = CustomerInfo.FirstName,
                LastName = CustomerInfo.LastName,
                IDNumber = UserService.ReplaceSpecialChracter(CustomerInfo.NationalIdentificationNumber),
                MSISDN = UserService.ReplaceSpecialChracter(CustomerInfo.CountryCode + CustomerInfo.PhoneNumber),
                LicFrequency = LicFrequencyTerm.ToString(),
                RadioTVUsage = RadioTVUsage,
                RadioTVFrequency = RadioTVFreeQuency,
                SuburbID = "1",
                ClientIDType = clientIdType,
                TaxClass = taxClassId
            });

            CombArguments objArg = new CombArguments();
            objArg.PartnerReference = Guid.NewGuid().ToString(); ;
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = new CombineFunctionObject { Function = "TPILICQuote", Vehicles = obj };

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            CombineQuoteRequest objroot = new CombineQuoteRequest();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient(LiveIceCashApi);
            //   var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

            SummaryDetailService.WriteLog(data, response.Content, "TPIQuote");



            if (json.Response.Quotes != null && json.Response.Quotes.Count > 0)
            {
                if (json.Response.Quotes[0].Policy != null)
                {
                    var Setting = InsuranceContext.Settings.All();
                    var DiscountOnRenewalSettings = Setting.Where(x => x.keyname == "Discount On Renewal").FirstOrDefault();
                    var premium = Convert.ToDecimal(json.Response.Quotes[0].Policy.CoverAmount);
                    switch (PaymentTermId)
                    {
                        case 1:
                            var AnnualRiskPremium = premium;
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                            {
                                json.LoyaltyDiscount = ((AnnualRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                            }
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                            {
                                json.LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                            }
                            break;
                        case 3:
                            var QuaterlyRiskPremium = premium;
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                            {
                                json.LoyaltyDiscount = ((QuaterlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                            }
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                            {
                                json.LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                            }
                            break;
                        case 4:
                            var TermlyRiskPremium = premium;
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.percentage))
                            {
                                json.LoyaltyDiscount = ((TermlyRiskPremium * Convert.ToDecimal(DiscountOnRenewalSettings.value)) / 100);
                            }
                            if (DiscountOnRenewalSettings.ValueType == Convert.ToInt32(eSettingValueType.amount))
                            {
                                json.LoyaltyDiscount = Convert.ToDecimal(DiscountOnRenewalSettings.value);
                            }
                            break;
                    }
                }
            }
            return json;
        }

        public string RemoveSpecialChars(string str)
        {
            // Create  a string array and add the special characters you want to remove
            // You can include / exclude more special characters based on your needs
            string[] chars = new string[] { ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "_", "(", ")", ":", "|", "[", "]" };
            //Iterate the number of times based on the String array length.
            for (int i = 0; i < chars.Length; i++)
            {
                if (str.Contains(chars[i]))
                {
                    str = str.Replace(chars[i], "");
                }
            }
            return str;
        }

        public static ResultRootObject TPIQuoteUpdate(Customer customer, VehicleDetail vehicleDetail, string PartnerToken, int? paymentMethod)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";

            List<VehicleObject> obj = new List<VehicleObject>();

            //   var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];

            var CustomerInfo = customer;

            var item = vehicleDetail;

            if (paymentMethod == null || paymentMethod == 0)
            {
                paymentMethod = 1;
            }


            if (paymentMethod == 2) // it's represent to visa
            {
                paymentMethod = 1;
            }


            // 

            // obj.Add(new VehicleObject { VRN = item.RegistrationNo, DurationMonths = (item.PaymentTermId == 1 ? 12 : item.PaymentTermId), VehicleValue = 0, YearManufacture = 0, InsuranceType = 0, VehicleType = 0, TaxClass = 0, Make = "", Model = "", EntityType = "", Town = CustomerInfo.City, Address1 = CustomerInfo.AddressLine1, Address2 = CustomerInfo.AddressLine2, CompanyName = "", FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, IDNumber = CustomerInfo.NationalIdentificationNumber, MSISDN = "01" + CustomerInfo.PhoneNumber });

            List<QuoteDetial> qut = new List<QuoteDetial>();

            qut.Add(new QuoteDetial { InsuranceID = item.InsuranceId, Status = "1" });

            var quotesDetial = new RequestTPIQuoteUpdate { Function = "TPIQuoteUpdate", PaymentMethod = Convert.ToString(paymentMethod), Identifier = "1", MSISDN = CustomerInfo.PhoneNumber, Quotes = qut };



            QuoteArgumentsTPIQuote objArg = new QuoteArgumentsTPIQuote();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = quotesDetial;



            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            ICEQuoteRequestTPIQuote objroot = new ICEQuoteRequestTPIQuote();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient(LiveIceCashApi);
            // var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

            SummaryDetailService.WriteLog(data, response.Content, "TPIQuoteUpdate");

            return json;
        }

        public static ResultRootObject LICQuoteUpdate(Customer customer, VehicleDetail vehicleDetail, string PartnerToken, int? paymentMethod)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";

            List<VehicleObject> obj = new List<VehicleObject>();

            //   var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];

            var CustomerInfo = customer;

            var item = vehicleDetail;

            if (paymentMethod == null || paymentMethod == 0)
            {
                paymentMethod = 1;
            }


            if (paymentMethod == 2) // it's represent to visa
            {
                paymentMethod = 1;
            }


            // 

            // obj.Add(new VehicleObject { VRN = item.RegistrationNo, DurationMonths = (item.PaymentTermId == 1 ? 12 : item.PaymentTermId), VehicleValue = 0, YearManufacture = 0, InsuranceType = 0, VehicleType = 0, TaxClass = 0, Make = "", Model = "", EntityType = "", Town = CustomerInfo.City, Address1 = CustomerInfo.AddressLine1, Address2 = CustomerInfo.AddressLine2, CompanyName = "", FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, IDNumber = CustomerInfo.NationalIdentificationNumber, MSISDN = "01" + CustomerInfo.PhoneNumber });

            List<LicQuoteDetial> qut = new List<LicQuoteDetial>();

            qut.Add(new LicQuoteDetial { LicenceID = item.LicenseId, Status = "1", DeliveryMethod = "3" });

            var quotesDetial = new RequestLicQuoteUpdate { Function = "LICQuoteUpdate", PaymentMethod = Convert.ToString(paymentMethod), Identifier = "1", MSISDN = CustomerInfo.PhoneNumber, Quotes = qut };



            QuoteArgumentsLicQuote objArg = new QuoteArgumentsLicQuote();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = quotesDetial;



            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            ICEQuoteRequestLivQuote objroot = new ICEQuoteRequestLivQuote();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient(LiveIceCashApi);
            // var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

            SummaryDetailService.WriteLog(data, response.Content, "LICQuoteUpdate");

            return json;
        }

        public static ResultRootObject TPILICUpdate(Customer customer, VehicleDetail vehicleDetail, string PartnerToken, int? paymentMethod)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";

            List<VehicleObject> obj = new List<VehicleObject>();

            //   var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];

            var CustomerInfo = customer;

            var item = vehicleDetail;

            if (paymentMethod == null || paymentMethod == 0)
            {
                paymentMethod = 1;
            }


            if (paymentMethod == 2) // it's represent to visa
            {
                paymentMethod = 1;
            }


            // 

            // obj.Add(new VehicleObject { VRN = item.RegistrationNo, DurationMonths = (item.PaymentTermId == 1 ? 12 : item.PaymentTermId), VehicleValue = 0, YearManufacture = 0, InsuranceType = 0, VehicleType = 0, TaxClass = 0, Make = "", Model = "", EntityType = "", Town = CustomerInfo.City, Address1 = CustomerInfo.AddressLine1, Address2 = CustomerInfo.AddressLine2, CompanyName = "", FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, IDNumber = CustomerInfo.NationalIdentificationNumber, MSISDN = "01" + CustomerInfo.PhoneNumber });

            List<CombineQuoteDetial> qut = new List<CombineQuoteDetial>();

            qut.Add(new CombineQuoteDetial { CombinedID = item.CombinedID, Status = "1", DeliveryMethod = "3", LicenceCert = "1" });

            var quotesDetial = new RequesCombineQuoteUpdate { Function = "TPILICUpdate", PaymentMethod = Convert.ToString(paymentMethod), Identifier = "1", MSISDN = CustomerInfo.PhoneNumber, Quotes = qut };



            QuoteArgumentsCombineQuote objArg = new QuoteArgumentsCombineQuote();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = quotesDetial;



            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            ICEQuoteRequestCombineQuote objroot = new ICEQuoteRequestCombineQuote();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient(LiveIceCashApi);
            // var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

            SummaryDetailService.WriteLog(data, response.Content, "TPILICUpdate");

            return json;
        }

        public static int GetMonthKey(int monthId)
        {
            int licFreequency = 0;
            switch (monthId)
            {
                case 1: // represent to 12 month
                    licFreequency = 3;
                    break;
                case 2:
                    Console.WriteLine("Case 2");
                    break;
                case 3:
                    Console.WriteLine("Case 1");
                    break;
                case 4:
                    licFreequency = 1;
                    break;
                case 5:
                    licFreequency = 4;
                    break;
                case 6:
                    licFreequency = 2;
                    break;
                case 7:
                    licFreequency = 5;
                    break;
                case 8:
                    licFreequency = 6;
                    break;
                case 9:
                    licFreequency = 7;
                    break;
                case 10:
                    licFreequency = 8;
                    break;
                case 11:
                    licFreequency = 9;
                    break;
                default:
                    licFreequency = 3;
                    break;
            }

            return licFreequency;
        }

        public static ResultRootObject TPIPolicy(VehicleDetail vehicleDetail, string PartnerToken)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";

            List<VehicleObject> obj = new List<VehicleObject>();

            //   var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];


            var item = vehicleDetail;


            // obj.Add(new VehicleObject { VRN = item.RegistrationNo, DurationMonths = (item.PaymentTermId == 1 ? 12 : item.PaymentTermId), VehicleValue = 0, YearManufacture = 0, InsuranceType = 0, VehicleType = 0, TaxClass = 0, Make = "", Model = "", EntityType = "", Town = CustomerInfo.City, Address1 = CustomerInfo.AddressLine1, Address2 = CustomerInfo.AddressLine2, CompanyName = "", FirstName = CustomerInfo.FirstName, LastName = CustomerInfo.LastName, IDNumber = CustomerInfo.NationalIdentificationNumber, MSISDN = "01" + CustomerInfo.PhoneNumber });

            //List<QuoteDetial> qut = new List<QuoteDetial>();

            TPIPolicyDetial qut = new TPIPolicyDetial { InsuranceID = item.InsuranceId, Function = "TPIPolicy" };

            // var quotesDetial = new RequestTPIQuoteUpdate { Function = "TPIQuoteUpdate", PaymentMethod = Convert.ToString(paymentMethod), Identifier = "1", MSISDN = "01" + CustomerInfo.PhoneNumber, Quotes = qut };



            QuoteArgumentsTPIPolicy objArg = new QuoteArgumentsTPIPolicy();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = qut;

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            ICEQuoteRequestTPIPolicy objroot = new ICEQuoteRequestTPIPolicy();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient(LiveIceCashApi);
            //  var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

            Insurance.Service.EmailService log = new Insurance.Service.EmailService();
            // log.WriteLog("TPIPolicy :" + response.Content);
            SummaryDetailService.WriteLog(data, response.Content, "TPIPolicy");

            return json;
        }

        public static ResultRootObject TPILICResult(VehicleDetail vehicleDetail, string PartnerToken)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";

            List<VehicleObject> obj = new List<VehicleObject>();
            var item = vehicleDetail;


            CombinePolicyDetial qut = new CombinePolicyDetial { CombinedID = item.CombinedID, Function = "TPILICResult" };

            QuoteArgumentsCombPolicy objArg = new QuoteArgumentsCombPolicy();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = qut;

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            ICEQuoteRequestCombPolicy objroot = new ICEQuoteRequestCombPolicy();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient(LiveIceCashApi);
            //  var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);


            Insurance.Service.EmailService log = new Insurance.Service.EmailService();
            // log.WriteLog("TPIPolicy :" + response.Content);
            SummaryDetailService.WriteLog(data, response.Content, "TPIPolicy");

            return json;
        }

        public static ResultRootObject LICQuote(string registrationNum, string paymentTerm, string PartnerToken)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";

            List<VehicleLicObject> obj = new List<VehicleLicObject>();
            var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];

            int paymentTermId = GetMonthKey(Convert.ToInt32(paymentTerm));

            //foreach (var item in listofvehicles)
            //{
            obj.Add(new VehicleLicObject
            {
                VRN = registrationNum,
                IDNumber = CustomerInfo.NationalIdentificationNumber,
                ClientIDType = "1",
                FirstName = CustomerInfo.FirstName,
                LastName = CustomerInfo.LastName,
                Address1 = CustomerInfo.AddressLine1,
                Address2 = CustomerInfo.AddressLine2,
                SuburbID = "1",
                LicFrequency = paymentTermId.ToString(),
                RadioTVUsage = null,
                RadioTVFrequency = null
            });
            //}

            LICQuoteArguments objArg = new LICQuoteArguments();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = new LICQuoteFunctionObject { Function = "LICQuote", Vehicles = obj };

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            LICQuoteRequest objroot = new LICQuoteRequest();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient(LiveIceCashApi);
            //var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

            SummaryDetailService.WriteLog(data, response.Content, "LICQuote");

            return json;

        }

        public static ResultRootObject TPILICUpdate(string registrationNum, string PartnerToken)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";

            List<VehicleLicInsuraceObject> obj = new List<VehicleLicInsuraceObject>();
            //  var CustomerInfo = (CustomerModel)HttpContext.Current.Session["CustomerDataModal"];

            var CustomerInfo = new CustomerModel();

            //foreach (var item in listofvehicles)
            //{
            obj.Add(new VehicleLicInsuraceObject
            {
                VRN = registrationNum,
                EntityType = "Personal",
                ClientIDType = "1",
                IDNumber = "12-123456A12",
                CompanyName = "",
                FirstName = "1",
                LastName = "",
                MSISDN = "",
                Email = "",
                Address1 = "",
                Address2 = "2",
                SuburbID = "3",
                InsuranceType = "",
                VehicleType = "",
                VehicleValue = "2",
                DurationMonths = "3",
                LicFrequency = "3",
                RadioTVUsage = "",
                RadioTVFrequency = "",

            });
            //}

            LICInsuranceQuoteArguments objArg = new LICInsuranceQuoteArguments();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = new LICInsuranceQuoteFunctionObject { Function = "TPILICQuote", Vehicles = obj };

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            LICInsuranceQuoteRequest objroot = new LICInsuranceQuoteRequest();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient(LiveIceCashApi);
            //var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

            SummaryDetailService.WriteLog(data, response.Content, "TPILICQuote");


            return json;

        }

        public static ResultRootObject LICCertConf(LicenseModel licenseModel, string PartnerToken)
        {
            //string PSK = "127782435202916376850511";
            string _json = "";

            List<VehicleLicConfObject> obj = new List<VehicleLicConfObject>();
            var CustomerInfo = new CustomerModel();

            obj.Add(new VehicleLicConfObject
            {
                VRN = licenseModel.VRN,
                CertSerialNo = licenseModel.SerialNumber,
                PrintResult = "1",
                LicenceID = licenseModel.LicenseId
            });

            LICConfArguments objArg = new LICConfArguments();
            objArg.PartnerReference = Guid.NewGuid().ToString();
            objArg.Date = DateTime.Now.ToString("yyyyMMddhhmmss");
            objArg.Version = "2.0";
            objArg.PartnerToken = PartnerToken;
            objArg.Request = new LICIConfFunctionObject { Function = "LICCertConf", LicenceID= licenseModel.LicenseId, CertSerialNo=licenseModel.SerialNumber, PrintResult="1" };

            _json = Newtonsoft.Json.JsonConvert.SerializeObject(objArg);

            //string  = json.Reverse()
            string reversejsonString = new string(_json.Reverse().ToArray());
            string reversepartneridString = new string(PSK.Reverse().ToArray());

            string concatinatedString = reversejsonString + reversepartneridString;

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(concatinatedString);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            string GetSHA512encrypted = SHA512(returnValue);

            string MAC = "";

            for (int i = 0; i < 16; i++)
            {
                MAC += GetSHA512encrypted.Substring((i * 8), 1);
            }

            MAC = MAC.ToUpper();

            LICConfRequest objroot = new LICConfRequest();
            objroot.Arguments = objArg;
            objroot.MAC = MAC;
            objroot.Mode = "SH";

            var data = Newtonsoft.Json.JsonConvert.SerializeObject(objroot);

            JObject jsonobject = JObject.Parse(data);

            var client = new RestClient(LiveIceCashApi);
            //var client = new RestClient(LiveIceCashApi);
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", jsonobject, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            ResultRootObject json = JsonConvert.DeserializeObject<ResultRootObject>(response.Content);

            SummaryDetailService.WriteLog(data, response.Content, "LICCertConf");
            return json;
        }


    }

    public class Arguments
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public FunctionObject Request { get; set; }
    }

    public class PaymentArugument
    {
        public string partner_id { get; set; }

        public decimal amount { get; set; }

        public Guid client_reference { get; set; }

        public string success_url { get; set; }

        public string failed_url { get; set; }

        public string results_url { get; set; }

        public string details { get; set; }
    }

    public class FunctionObject
    {
        public string Function { get; set; }
    }
    public class ICERootObject
    {
        public Arguments Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }
    public class VehicleObject
    {
        public string VRN { get; set; }
        public string IDNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MSISDN { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Town { get; set; }
        public string EntityType { get; set; }
        public string CompanyName { get; set; }
        public int DurationMonths { get; set; }
        public int VehicleValue { get; set; }
        public int InsuranceType { get; set; }
        public int VehicleType { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string TaxClass { get; set; }
        public int YearManufacture { get; set; }

        // public DateTime StartDate { get; set; }

        //  public DateTime EndDate { get; set; }
    }

    //LicFrequency


    public class VehicleCombObject
    {
        public string VRN { get; set; }
        public string IDNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MSISDN { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string SuburbID { get; set; }
        public string ClientIDType { get; set; }
        public string InsuranceType { get; set; }

        public string TaxClass { get; set; }

        public string VehicleType { get; set; }

        public string DurationMonths { get; set; }
        public string LicFrequency { get; set; }
        public string RadioTVUsage { get; set; }
        public string RadioTVFrequency { get; set; }

    }


    public class VehicleObjectWithNullable
    {
        public string VRN { get; set; }
        public string IDNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MSISDN { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Town { get; set; }
        public string EntityType { get; set; }
        public string CompanyName { get; set; }
        public string DurationMonths { get; set; }
        public string VehicleValue { get; set; }
        public string InsuranceType { get; set; }
        public string VehicleType { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string TaxClass { get; set; }
        public string YearManufacture { get; set; }
    }

    public class QuoteArguments
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public QuoteFunctionObject Request { get; set; }
    }

    //VehicleCombObject




    public class CombArguments
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public CombineFunctionObject Request { get; set; }
    }




    public class QuoteFunctionObject
    {
        public string Function { get; set; }
        public List<VehicleObject> Vehicles { get; set; }
    }


    public class CombineFunctionObject
    {
        public string Function { get; set; }
        public List<VehicleCombObject> Vehicles { get; set; }
    }



    public class ICEQuoteRequest
    {
        public QuoteArguments Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }

    public class CombineQuoteRequest
    {
        public CombArguments Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }


    public class TokenReposone
    {
        public string Function { get; set; }
        public string Result { get; set; }
        public string Message { get; set; }
        public string PartnerToken { get; set; }
        public string ExpireDate { get; set; }
    }
    public class ICEcashTokenResponse
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public TokenReposone Response { get; set; }

        public Quote Quotes { get; set; }
    }
    public class Quote
    {
        public string VRN { get; set; }
        public string InsuranceID { get; set; }
        public int Result { get; set; }
        public string Message { get; set; }
    }
    public class QuoteResponse
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public List<Quote> Quotes { get; set; }
    }
    public class ICEcashQuoteResponse
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public QuoteResponse Response { get; set; }
    }
    public class ResultPolicy
    {
        public string InsuranceType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string DurationMonths { get; set; }
        public string Amount { get; set; }
        public string StampDuty { get; set; }
        public string GovernmentLevy { get; set; }
        public string CoverAmount { get; set; }
        public string PremiumAmount { get; set; }
    }
    public class ResultClient
    {
        public string IDNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MSISDN { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Town { get; set; }
        public string EntityType { get; set; }
        public string CompanyName { get; set; }
    }
    public class ResultVehicle
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public string TaxClass { get; set; }
        public string YearManufacture { get; set; }
        public int VehicleType { get; set; }
        public string VehicleValue { get; set; }
    }
    public class ResultQuote
    {
        public string VRN { get; set; }
        public string InsuranceID { get; set; }
        public string PolicyNo { get; set; }
        public int Result { get; set; }
        public string Message { get; set; }

        public string LicenceID { get; set; }
        public string CombinedID { get; set; }

       // public string LicenceCert { get; set; }

        public string LicExpiryDate { get; set; }
        public string TotalLicAmt { get; set; }
        public string TotalAmount { get; set; }
        public ResultPolicy Policy { get; set; }
        public ResultClient Client { get; set; }
        public ResultVehicle Vehicle { get; set; }
        public Licence Licence { get; set; }

    }
    public class ResultResponse
    {
        public int Result { get; set; }
        public string Message { get; set; }
        public string PolicyNo { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public string Status { get; set; }
        public string LicenceCert { get; set; }
        public List<ResultQuote> Quotes { get; set; }
    }
    public class ResultRootObject
    {
        public decimal LoyaltyDiscount { get; set; }
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public ResultResponse Response { get; set; }
    }


    public class RequestTPIQuoteUpdate
    {
        public string Function { get; set; }
        public string PaymentMethod { get; set; }
        public string Identifier { get; set; }
        public string MSISDN { get; set; }
        public List<QuoteDetial> Quotes { get; set; }
    }

    public class RequesCombineQuoteUpdate
    {
        public string Function { get; set; }
        public string PaymentMethod { get; set; }
        public string Identifier { get; set; }
        public string MSISDN { get; set; }
        public List<CombineQuoteDetial> Quotes { get; set; }
    }



    //LicQuoteDetial

    public class RequestLicQuoteUpdate
    {
        public string Function { get; set; }
        public string PaymentMethod { get; set; }
        public string Identifier { get; set; }
        public string MSISDN { get; set; }
        public List<LicQuoteDetial> Quotes { get; set; }
    }




    public class QuoteDetial
    {
        public string InsuranceID { get; set; }

        public string Status { get; set; }

        public string LicenceID { get; set; }

    }

    public class CombineQuoteDetial
    {
        public string CombinedID { get; set; }
        public string Status { get; set; }
        public string DeliveryMethod { get; set; }
        public string LicenceCert { get; set; }

    }


    public class LicQuoteDetial
    {
        public string DeliveryMethod { get; set; }

        public string Status { get; set; }

        public string LicenceID { get; set; }

    }



    public class QuoteArgumentsTPIQuote
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public RequestTPIQuoteUpdate Request { get; set; }
    }


    public class QuoteArgumentsCombineQuote
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public RequesCombineQuoteUpdate Request { get; set; }
    }


    public class QuoteArgumentsLicQuote
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public RequestLicQuoteUpdate Request { get; set; }
    }

    //RequestLicQuoteUpdate


    public class ICEQuoteRequestTPIQuote
    {
        public QuoteArgumentsTPIQuote Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }

    public class ICEQuoteRequestCombineQuote
    {
        public QuoteArgumentsCombineQuote Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }


    public class ICEQuoteRequestLivQuote
    {
        public QuoteArgumentsLicQuote Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }



    public class TPIPolicyDetial
    {
        public string InsuranceID { get; set; }
        public string Function { get; set; }
    }


    public class CombinePolicyDetial
    {
        public string CombinedID { get; set; }
        public string Function { get; set; }
    }


    public class QuoteArgumentsTPIPolicy
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public TPIPolicyDetial Request { get; set; }
    }


    public class QuoteArgumentsCombPolicy
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public CombinePolicyDetial Request { get; set; }
    }


    public class ICEQuoteRequestTPIPolicy
    {
        public QuoteArgumentsTPIPolicy Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }


    public class ICEQuoteRequestCombPolicy
    {
        public QuoteArgumentsCombPolicy Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }


    public class VehicleLicObject
    {
        public string VRN { get; set; }
        public string IDNumber { get; set; }
        public string ClientIDType { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string SuburbID { get; set; }
        public string LicFrequency { get; set; }
        public string RadioTVUsage { get; set; }
        public string RadioTVFrequency { get; set; }

    }

    public class VehicleLicInsuraceObject
    {
        public string VRN { get; set; }
        public string EntityType { get; set; }
        public string ClientIDType { get; set; }
        public string IDNumber { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MSISDN { get; set; }
        public string Email { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }


        public string SuburbID { get; set; }
        public string InsuranceType { get; set; }
        public string VehicleType { get; set; }
        public string VehicleValue { get; set; }

        public string DurationMonths { get; set; }
        public string LicFrequency { get; set; }
        public string RadioTVUsage { get; set; }
        public string RadioTVFrequency { get; set; }

    }


    public class VehicleLicConfObject
    {
        public string LicenceID { get; set; }
        public string CertSerialNo { get; set; }
        public string VRN { get; set; }
        public string PrintResult { get; set; }

    }



    public class LICQuoteArguments
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public LICQuoteFunctionObject Request { get; set; }
    }

    public class LICInsuranceQuoteArguments
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public LICInsuranceQuoteFunctionObject Request { get; set; }
    }


    public class LICConfArguments
    {
        public string PartnerReference { get; set; }
        public string Date { get; set; }
        public string Version { get; set; }
        public string PartnerToken { get; set; }
        public LICIConfFunctionObject Request { get; set; }



    }



    public class LICQuoteFunctionObject
    {
        public string Function { get; set; }
        public List<VehicleLicObject> Vehicles { get; set; }
    }

    public class LICInsuranceQuoteFunctionObject
    {
        public string Function { get; set; }
        public List<VehicleLicInsuraceObject> Vehicles { get; set; }
    }


    public class LICIConfFunctionObject
    {
        public string Function { get; set; }
        public string LicenceID { get; set; }

        public string CertSerialNo { get; set; }
        public string VRN { get; set; }

        public string PrintResult { get; set; }
        // public List<VehicleLicConfObject> Vehicles { get; set; }
    }


    public class LICQuoteRequest
    {
        public LICQuoteArguments Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }


    public class LICInsuranceQuoteRequest
    {
        public LICInsuranceQuoteArguments Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }

    public class LICConfRequest
    {
        public LICConfArguments Arguments { get; set; }
        public string MAC { get; set; }
        public string Mode { get; set; }
    }



    public class Licence
    {
        public string LicFrequency { get; set; }
        public string RadioTVUsage { get; set; }
        public string RadioTVFrequency { get; set; }
        public string NettMass { get; set; }
        public string LicExpiryDate { get; set; }
        public string TransactionAmt { get; set; }
        public string ArrearsAmt { get; set; }
        public string PenaltiesAmt { get; set; }
        public string AdministrationAmt { get; set; }
        public string TotalLicAmt { get; set; }
        public string RadioTVAmt { get; set; }
        public string RadioTVArrearsAmt { get; set; }
        public string TotalRadioTVAmt { get; set; }
        public string TotalAmount { get; set; }

    }





}
