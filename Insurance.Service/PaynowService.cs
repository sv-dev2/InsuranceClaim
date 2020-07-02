using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Insurance.Service
{

    public class PaynowService
    {

        private static readonly HttpClient client = new HttpClient();
        // sendbox
       // private static String IntegrationID = "5623";
       // private static string IntegrationKey = "7c1cd190-5046-4292-806a-0dbb85b949f6";

        // live
        private static String IntegrationID = "6059";
        private static string IntegrationKey = "afef5b33-696c-4f32-b2f9-45de5eaa0eef";

        public async Task<InsuranceClaim.Models.PaynowResponse> initiateTransaction(string id, string amount, string additionalinfo, string authemail, bool isRenew = false)
        {
            InsuranceClaim.Models.PaynowResponse paynowresponse = new InsuranceClaim.Models.PaynowResponse();

            var response = new HttpResponseMessage();
            FormUrlEncodedContent content;
            Uri myuri = new Uri(System.Web.HttpContext.Current.Request.Url.AbsoluteUri);
            string pathQuery = myuri.PathAndQuery;
            string hostName = myuri.ToString().Replace(pathQuery, "");

            string PaymentId = "PAYNOW-" + Guid.NewGuid().ToString();
            HttpContext.Current.Session["PaymentId"] = PaymentId;

          //  authemail = "constantine@gene-insure.com";
            if (isRenew)
            {
                var values = new Dictionary<string, string>
            {

               { "resulturl", hostName + "/Renew/SaveDetailList/" + id},
               { "returnurl", hostName + "/Renew/SaveDetailList/" + id},
               { "reference", PaymentId },
               { "amount",Convert.ToString(amount)},
               { "id", IntegrationID },
               { "additionalinfo", additionalinfo},
               { "authemail", authemail },
               { "status", "Message" }
            };

                var generatedhash = GenerateTwoWayHash(values, new Guid(IntegrationKey));

                var _values = new Dictionary<string, string>
            {
               { "resulturl", hostName + "/Renew/SaveDetailList/" + id},
               { "returnurl", hostName + "/Renew/SaveDetailList/" + id},
               { "reference", PaymentId },
               { "amount",Convert.ToString(amount)},
               { "id", IntegrationID },
               { "additionalinfo", additionalinfo },
               { "authemail", authemail },
               { "status", "Message" },
               { "hash", generatedhash.ToUpper() }
            };

                paynowresponse.generatedhash = generatedhash.ToUpper();

                content = new FormUrlEncodedContent(_values);

                response = await client.PostAsync("https://www.paynow.co.zw/interface/initiatetransaction", content);
            }
            else
            {
                var values = new Dictionary<string, string>
            {

               { "resulturl", hostName + "/Paypal/SaveDetailList/" + id},
               { "returnurl", hostName + "/Paypal/SaveDetailList/" + id},
               { "reference", PaymentId },
               { "amount",Convert.ToString(amount)},
               { "id", IntegrationID },
               { "additionalinfo", additionalinfo },
               { "authemail", authemail },
               { "status", "Message" }
            };

                var generatedhash = GenerateTwoWayHash(values, new Guid(IntegrationKey));

                var _values = new Dictionary<string, string>
            {
               { "resulturl", hostName + "/Paypal/SaveDetailList/" + id},
               { "returnurl", hostName + "/Paypal/SaveDetailList/" + id},
               { "reference", PaymentId },
               { "amount",Convert.ToString(amount)},
               { "id", IntegrationID },
               { "additionalinfo", additionalinfo },
               { "authemail", authemail },
               { "status", "Message" },
               { "hash", generatedhash.ToUpper() }
            };

                paynowresponse.generatedhash = generatedhash.ToUpper();

                content = new FormUrlEncodedContent(_values);

                response = await client.PostAsync("https://www.paynow.co.zw/interface/initiatetransaction", content);
            }



            var responseString = await response.Content.ReadAsStringAsync();

            string decodedUrl = Uri.UnescapeDataString("http://dummyurl_to_decode_asURL.com?" + responseString);

            Uri responseUri = new Uri(decodedUrl);

            SmsLog objsmslog = new SmsLog()
            {
                Sendto = "PAYNOW",
                Body = content.ToString(),
                Response = decodedUrl
            };

            InsuranceContext.SmsLogs.Insert(objsmslog);

            if (decodedUrl.Contains("Status=Error"))
            {
                paynowresponse.status = HttpUtility.ParseQueryString(responseUri.Query).Get("status");
                paynowresponse.error = HttpUtility.ParseQueryString(responseUri.Query).Get("error");

                //log error
                //display error
                return paynowresponse;
            }
            else
            {
                paynowresponse.browserurl = HttpUtility.ParseQueryString(responseUri.Query).Get("browserurl");
                paynowresponse.pollurl = HttpUtility.ParseQueryString(responseUri.Query).Get("pollurl");
                paynowresponse.status = HttpUtility.ParseQueryString(responseUri.Query).Get("status");
                paynowresponse.hash = HttpUtility.ParseQueryString(responseUri.Query).Get("hash");

                //log success
                //display success
                //It is vital that the merchant site verify the hash value contained in the message before redirecting the
                //Customer to the browserurl. 
                return paynowresponse;
            }

        }

        public string GenerateTwoWayHash(Dictionary<string, string> items, Guid guid)
        {
            string concat = string.Join("", items.Select(c => (c.Value != null ? c.Value.Trim() : "")).ToArray());
            SHA512 check = SHA512.Create();
            byte[] resultArr = check.ComputeHash(Encoding.UTF8.GetBytes(concat + guid));
            return ByteArrayToString(resultArr);
        }

        public string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

    }
}
