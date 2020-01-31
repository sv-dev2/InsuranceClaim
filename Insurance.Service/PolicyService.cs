using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace Insurance.Service
{
    public class PolicyService
    {
        public string GetLatestPolicyNumber()
        {
            string policyNumber = string.Empty;
            var objList = InsuranceContext.PolicyDetails.All(orderBy: "Id desc").FirstOrDefault();
            if (objList != null)
            {
                string number = objList.PolicyNumber.Split('-')[0].Substring(4, objList.PolicyNumber.Length - 6);
                long pNumber = Convert.ToInt64(number.Substring(2, number.Length - 2)) + 1;
                int length = 7;
                length = length - pNumber.ToString().Length;
                for (int i = 0; i < length; i++)
                {
                    policyNumber += "0";
                }
                policyNumber += pNumber;
                policyNumber = "GMCC" + DateTime.Now.Year.ToString().Substring(2, 2) + policyNumber + "-1";
            }
            else
            {
                policyNumber = ConfigurationManager.AppSettings["PolicyNumber"] + "-1";
            }
            return policyNumber;
        }
        public void SavePolicy(PolicyDetail policy)
        {
            try
            {
                InsuranceContext.PolicyDetails.Insert(policy);
            }
            catch(Exception ex)
            {

            }

           
        }
        public void UpdatePolicy(PolicyDetail policy)
        {
            InsuranceContext.PolicyDetails.Update(policy);
        }
        public PolicyDetail GetPolicyDetailById(int policyId)
        {
            return InsuranceContext.PolicyDetails.Single(policyId);
        }


        public System.Drawing.Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String.ToString());
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }

        public void SaveQRCode(QRCode qRCodes)
        {
            InsuranceContext.QRCodes.Insert(qRCodes);
        }


    
    }



}
