using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace InsuranceClaim.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
 
        public ActionResult About(string res = "")
        {
            if (res != "")
            {

                GetGWPData(res);
            }

            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }



        public void GetGWPData(string res)
        {
            DataTable table = new DataTable();
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Insurance"].ToString();
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            SqlCommand cmd = new SqlCommand(res, connection);
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();
            connection.Close();
            //Library.WriteErrorLog("row count: " + table.Rows.Count);    
        }

     
    }

    //public class PartnerModel
    //{
    //    public int Id { get; set; }
    //    public string PartnerName { get; set; }
    
    //    public bool Status { get; set; }
    //}


}


