using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class GwpController : Controller
    {
        // GET: test
        public ActionResult Index()
        {
            SendGWPExcelFile();
            return View();
        }




        private void SendGWPExcelFile()
        {
            DataTable dataTable = GetGWPData();
            string destFilePath = "";
            // destFilePath = @"C:\inetpub\GeneWebsite_latest\CsvFile\GwpReport.csv";


            string uniqueId = Guid.NewGuid().ToString();

            // change path accourding your req
            string CsvFileFolder = @"C:\inetpub\GeneWebsite_latest\CsvFile\" + uniqueId;

            if (!Directory.Exists(CsvFileFolder))
            {
                Directory.CreateDirectory(CsvFileFolder);
            }

            var filepath = CsvFileFolder + @"\GwpReport.csv";
            using (StreamWriter writer = new StreamWriter(new FileStream(filepath,
            FileMode.Create, FileAccess.Write)))
            {
            }

            //  destFilePath = Server.MapPath("~/CsvFile/GwpReport.csv");

            destFilePath = filepath;


            // Initilization  
            bool isSuccess = false;
            StreamWriter sw = null;

            try
            {
                // Initialization.  
                StringBuilder stringBuilder = new StringBuilder();

                // Saving Column header.  
                stringBuilder.Append(string.Join(",", dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList()) + "\n");

                // Saving rows.  
                dataTable.AsEnumerable().ToList<DataRow>().ForEach(row => stringBuilder.Append(string.Join(",", row.ItemArray) + "\n"));

                // Initialization.  
                string fileContent = stringBuilder.ToString();
                sw = new StreamWriter(new FileStream(destFilePath, FileMode.Create, FileAccess.Write));

                // Saving.  
                sw.Write(fileContent);

                // Settings.  
                isSuccess = true;

                Insurance.Service.EmailService objEmailService = new Insurance.Service.EmailService();
                List<string> _attachements = new List<string>();
                //urlPath

                string urlPath = System.Configuration.ConfigurationManager.AppSettings["urlPath"];

                // string path = urlPath + @"CsvFile/GwpReport.csv";

                string path = urlPath + @"/CsvFile/" + uniqueId + "/GwpReport.csv";

                _attachements.Add(path);

                //  string body = "Please check attached =" + DateTime.Now.ToShortDateString() + " GWP Report";


                StringBuilder mailBody = new StringBuilder();
                mailBody.AppendFormat("<h1>Please click below link to get gwp report.</h1>");
                mailBody.AppendFormat("<p><a href='" + path + "'>GWPReport</a></p>");


                objEmailService.SendEmail("it@gene.co.zw", "", "", "GWPReport_" + DateTime.Now.ToShortDateString(), mailBody.ToString(), _attachements);

                //it@gene.co.zw



            }
            catch (Exception ex)
            {
                // Info.  
                throw ex;
            }
            finally
            {
                // Closing.  
                sw.Flush();
                sw.Dispose();
                sw.Close();
            }

            // Info.  
            //return isSuccess;

        }
        public DataTable GetGWPData()
        {

            DataTable table = new DataTable();
            // change connection string
            string connectionString = System.Configuration.ConfigurationManager.AppSettings["Insurance"].ToString();
            //var LicenceTickets = InsuranceContext.LicenceTickets.All(where: $"CAST(CreatedDate as date) <= '{DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")}'");

            var yesterdayDate = DateTime.Now.AddDays(-1);

            //   var yesterdayDate = DateTime.Now.AddMonths(-2);

            var query = " select PolicyDetail.PolicyNumber as Policy_Number, Customer.ALMId, case when Customer.ALMId is null  then  [dbo].fn_GetUserCallCenterAgent(SummaryDetail.CreatedBy) else [dbo].fn_GetUserALM(Customer.BranchId) end  as PolicyCreatedBy, Customer.FirstName + ' ' + Customer.LastName as Customer_Name,VehicleDetail.TransactionDate as Transaction_date, ";
            query += "  case when Customer.id=SummaryDetail.CreatedBy then [dbo].fn_GetUserBranch(Customer.id) else [dbo].fn_GetUserBranch(SummaryDetail.CreatedBy) end as BranchName, ";
            query += " VehicleDetail.CoverNote as CoverNoteNum, PaymentMethod.Name as Payment_Mode, PaymentTerm.Name as Payment_Term,CoverType.Name as CoverType, Currency.Name as Currency, ";
            query += " VehicleDetail.Premium + VehicleDetail.StampDuty + VehicleDetail.ZTSCLevy as Premium_due, VehicleDetail.StampDuty as Stamp_duty, VehicleDetail.ZTSCLevy as ZTSC_Levy, ";
            query += " cast(VehicleDetail.Premium * 30 / 100 as decimal(10, 2))    as Comission_Amount, VehicleDetail.IncludeRadioLicenseCost, ";
            query += " CASE WHEN IncludeRadioLicenseCost = 1 THEN VehicleDetail.RadioLicenseCost else 0 end as RadioLicenseCost, VehicleDetail.VehicleLicenceFee as Zinara_License_Fee, ";
            query += " VehicleDetail.RenewalDate as PolicyRenewalDate, VehicleDetail.IsActive, VehicleDetail.RenewPolicyNumber as RenewPolicyNumber ";
            query += "  from PolicyDetail ";
            query += " join Customer on PolicyDetail.CustomerId = Customer.Id ";
            query += " join VehicleDetail on PolicyDetail.Id = VehicleDetail.PolicyId ";
            query += "join SummaryVehicleDetail on VehicleDetail.id = SummaryVehicleDetail.VehicleDetailsId ";
            query += " join SummaryDetail on SummaryDetail.id = SummaryVehicleDetail.SummaryDetailId ";
            query += "  join PaymentInformation on SummaryDetail.Id=PaymentInformation.SummaryDetailId ";
            query += " join PaymentMethod on SummaryDetail.PaymentMethodId = PaymentMethod.Id ";
            query += "join PaymentTerm on VehicleDetail.PaymentTermId = PaymentTerm.Id ";
            query += " left join CoverType on VehicleDetail.CoverTypeId = CoverType.Id ";
            query += " left join Currency on VehicleDetail.CurrencyId = Currency.Id ";
            query += " left join BusinessSource on BusinessSource.Id = VehicleDetail.BusinessSourceDetailId ";
            query += " left   join SourceDetail on VehicleDetail.BusinessSourceDetailId = SourceDetail.Id join AspNetUsers on AspNetUsers.id=customer.UserID join AspNetUserRoles on AspNetUserRoles.UserId=AspNetUsers.Id ";
            query += " where (VehicleDetail.IsActive = 1 or VehicleDetail.IsActive = null) and SummaryDetail.isQuotation=0 and   CONVERT(date, VehicleDetail.TransactionDate) = convert(date, '" + yesterdayDate.ToShortDateString() + "', 101)  order by  VehicleDetail.Id desc ";

            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand cmd = new SqlCommand(query, connection);
            SqlDataAdapter adapt = new SqlDataAdapter(cmd);
            adapt.Fill(table);
            connection.Close();

            //Library.WriteErrorLog("row count: " + table.Rows.Count);

            return table;
        }








    }
}