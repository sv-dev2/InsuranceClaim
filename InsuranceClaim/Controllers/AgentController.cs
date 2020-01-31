using Insurance.Domain;
using Insurance.Service;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using static InsuranceClaim.Controllers.CustomerRegistrationController;

namespace InsuranceClaim.Controllers
{
    public class AgentController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        string AdminEmail = WebConfigurationManager.AppSettings["AdminEmail"];
        RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
        Insurance.Service.smsService objsmsService = new Insurance.Service.smsService();
        SummaryDetailService _summaryDetailService = new SummaryDetailService();

        string _superUser = "fe19c887-f8a9-4353-939f-65e19afe0D5L";
        string _agent = "bbbeffe0-94fa-41b7-bd8b-72d9ddc7JhKM";
        string _agentRoleName = "Agent";

        public AgentController()
        {

        }

        public AgentController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Agent
        public ActionResult Index()
        {
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            if (userLoggedin)
            {
                var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                var roles = UserManager.GetRoles(userid).FirstOrDefault();
                //if (roles != "SuperAdmin")
                //{
                //    return RedirectToAction("Index", "CustomerRegistration");
                //}
            }
            else
            {
                return RedirectToAction("Index", "Agent");
            }


            string url = HttpContext.Request.Url.Authority;
            // Authority	"localhost:49873"	string

            var branchList = InsuranceContext.Branches.All();

            var query = "select Customer. * , AspNetUsers.Email, AgentLogo.LogoPath from Customer ";
            query += "   join AspNetUsers on Customer.UserID = AspNetUsers.Id ";
            query += "  join AspNetUserRoles  on AspNetUserRoles.UserId = AspNetUsers.Id  left join  AgentLogo on Customer.Id = AgentLogo.CustomerId ";
            query += " where AspNetUserRoles.RoleId ='" + _agent + "' and (IsActive is null or IsActive=1) order by Customer.Id desc  ";


            //      var user1 = InsuranceContext.Query(query).Select

            var user = InsuranceContext.Query(query).Select(x => new CustomerModel()
            {
                CustomerId = x.CustomerId,
                UserID = x.UserID,
                FirstName = x.FirstName,
                LastName = x.LastName,
                AddressLine1 = x.AddressLine1,
                AddressLine2 = x.AddressLine2,
                City = x.City,
                NationalIdentificationNumber = x.NationalIdentificationNumber,
                Zipcode = x.Zipcode,
                Country = x.Country,
                DateOfBirth = x.DateOfBirth,
                Gender = x.Gender,
                IsWelcomeNoteSent = x.IsWelcomeNoteSent,
                IsPolicyDocSent = x.IsPolicyDocSent,
                IsLicenseDiskNeeded = x.IsLicenseDiskNeeded,
                IsOTPConfirmed = x.IsOTPConfirmed == null,
                CreatedBy = x.CreatedBy,
                ModifiedOn = x.ModifiedOn,
                ModifiedBy = x.ModifiedBy,
                IsActive = x.IsActive == null ? false : Convert.ToBoolean(x.IsActive),
                CountryCode = x.Countrycode,
                PhoneNumber = x.PhoneNumber,
                IsCustomEmail = x.IsCustomEmail == null ? false : Convert.ToBoolean(x.IsCustomEmail),
                EmailAddress = x.Email,
                AgentLogo = x.LogoPath,
                AgentWhatsapp =x.AgentWhatsapp,
                // CompanyName = x.CompanyName,
                // CompanyEmail = x.CompanyEmail,
                //  CompanyAddress = x.CompanyAddress,
                //   CompanyPhone = x.CompanyPhone,
                //  CompanyCity = x.CompanyCity,
                // CompanyBusinessId = x.CompanyBusinessId,
                //  IsCorporate = x.IsCorporate,
                //  BranchId = x.BranchId,
                //  ALMId = x.ALMId,
                Id = x.Id,
                CreatedOn = x.CreatedOn,
                Branch = GetAgentBranch(x.AgentBranch, branchList),
            }).ToList();



            ListUserViewModel lstUserModel = new ListUserViewModel();
            lstUserModel.ListUsers = user;

            return View(lstUserModel);
        }


        public string GetAgentBranch(string branchIds, IEnumerable<Branch> branches)
        {

            string branchName = "";

            if(branchIds!=null)
            {
                var splitBranch = branchIds.Split(',');

                foreach (var item in splitBranch)
                {
                    var branchDetails = branches.FirstOrDefault(c => c.Id == Convert.ToInt32(item));
                    if (branchDetails != null)
                    {
                        if (branchName == "")
                            branchName = branchDetails.BranchName;
                        else
                            branchName += "," + branchDetails.BranchName;

                    }

                }
            }
           

            return branchName;
        }



        // GET: Agent/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Agent/Create
        public ActionResult Create(int id = 0)
        {
            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            string path = Server.MapPath("~/Content/Countries.txt");
            var _countries = System.IO.File.ReadAllText(path);
            var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(_countries);
            ViewBag.Countries = resultt.countries;
            var userid = "";

            //string paths = Server.MapPath("~/Content/Cities.txt");
            //var _cities = System.IO.File.ReadAllText(paths);
            //var resultts = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObjects>(_cities);
            //ViewBag.Cities = resultts.cities;

            ViewBag.Cities = InsuranceContext.Cities.All();
            ViewBag.Branches = InsuranceContext.Branches.All();



            if (userLoggedin)
            {
                userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                // var role = UserManager.GetRoles(userid).FirstOrDefault();
                //if (role != "SuperAdmin")
                //{
                //    return RedirectToAction("Index", "CustomerRegistration");
                //}
            }
            else
            {
                return RedirectToAction("Index", "CustomerRegistration");
            }

            CustomerModel obj = new CustomerModel();
            List<IdentityRole> roles = roleManager.Roles.ToList();


            InsuranceClaim.Models.RoleManagementListViewModel _roles = new RoleManagementListViewModel();

            _roles.RoleList = roles;
            ViewBag.Adduser = _roles.RoleList;

            obj.Zipcode = "00263";



            var data = InsuranceContext.Customers.Single(id);
            //   var branchs = InsuranceContext.Branches.Single(data.BranchId) == null ? "" : InsuranceContext.Branches.Single(data.BranchId).BranchName;



            return View(obj);
        }

        // POST: Agent/Create
        [HttpPost]
        public async Task<ActionResult> Create(CustomerModel model, HttpPostedFileBase file, string Corporate)
        {
            try
            {

                string path = Server.MapPath("~/Content/Countries.txt");
                var _countries = System.IO.File.ReadAllText(path);
                var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(_countries);
                ViewBag.Countries = resultt.countries;
                ViewBag.Cities = InsuranceContext.Cities.All();
                ViewBag.Branches = InsuranceContext.Branches.All();


                if(Corporate=="")
                {
                    model.IsCorporate = true;
                    model.EmailAddress = model.CompanyEmail;
                    model.CompanyEmail = model.CompanyEmail;

                    model.FirstName = model.CompanyName;
                    model.CompanyName = model.CompanyName;

                    model.AddressLine1 = model.CompanyAddress;
                    model.CompanyAddress = model.CompanyAddress;

                    model.PhoneNumber = model.CompanyPhone;
                    model.CompanyPhone = model.CompanyPhone;

                    model.CompanyCity = model.CompanyCity;
                    model.City = model.CompanyCity;

                    model.CompanyBusinessId = model.CompanyBusinessId;
                    
                }




                decimal custId = 0.00m;
                var user = new ApplicationUser { UserName = model.EmailAddress, Email = model.EmailAddress, PhoneNumber = model.PhoneNumber };
                var result = await UserManager.CreateAsync(user, "Geninsure@123");
                if (result.Succeeded)
                {
                    var currentUser = UserManager.FindByName(user.UserName);
                    var roleresult = UserManager.AddToRole(currentUser.Id, _agentRoleName);

                    var objCustomer = InsuranceContext.Customers.All().OrderByDescending(x => x.Id).FirstOrDefault();
                    if (objCustomer != null)
                    {
                        custId = objCustomer.CustomerId + 1;
                    }
                    else
                    {
                        custId = Convert.ToDecimal(ConfigurationManager.AppSettings["CustomerId"]);
                    }

                    model.UserID = user.Id;
                    model.CustomerId = custId;

                    Customer cstmr = new Customer();
                    cstmr.Id = model.Id;
                    cstmr.CustomerId = model.CustomerId;
                    cstmr.AddressLine1 = model.AddressLine1;
                    cstmr.AddressLine2 = model.AddressLine2;
                    cstmr.City = model.City;           
                    cstmr.AgentBranch = model.HdnBrach;
                    cstmr.Countrycode = model.CountryCode;
                    cstmr.DateOfBirth = model.DateOfBirth == null? DateTime.Now : model.DateOfBirth;
                    cstmr.FirstName = model.FirstName;
                    cstmr.LastName = model.LastName;
                    cstmr.NationalIdentificationNumber = model.NationalIdentificationNumber;
                    cstmr.Zipcode = model.Zipcode;
                    cstmr.Gender = model.Gender;
                    cstmr.Country = model.Country;
                    cstmr.IsActive = model.IsActive;
                    cstmr.IsLicenseDiskNeeded = model.IsLicenseDiskNeeded;
                    cstmr.IsOTPConfirmed = model.IsOTPConfirmed;
                    cstmr.IsPolicyDocSent = model.IsPolicyDocSent;
                    cstmr.IsWelcomeNoteSent = model.IsWelcomeNoteSent;
                    cstmr.UserID = user.Id;
                    cstmr.PhoneNumber = model.PhoneNumber;
                    cstmr.IsActive = true;
                    cstmr.AgentWhatsapp = model.AgentWhatsapp;


                    if(model.IsCorporate)
                    {
                        cstmr.IsCorporate = true;
                        cstmr.CompanyName = model.CompanyName;
                        cstmr.CompanyEmail = model.CompanyEmail;
                        cstmr.CompanyAddress = model.CompanyAddress;
                        cstmr.CompanyPhone = model.CompanyPhone;
                        cstmr.CompanyCity = model.CompanyCity;
                        cstmr.CompanyBusinessId = model.CompanyBusinessId;
                        cstmr.LastName = " ";
                    }


                    InsuranceContext.Customers.Insert(cstmr);
                    string imagePath = SaveAgentLogoPath(file);
                    SaveAgentLogo(cstmr.Id, imagePath);
                }

            }
            catch (Exception ex)
            {
                model.ErrorMsg = ex.Message;
                return View(model);

            }


            return RedirectToAction("Index");

        }

        public void CreateAgent(CustomerModel model)
        {

        }



        public string SaveAgentLogoPath(HttpPostedFileBase file)
        {
            string path = "";
            if (file.ContentLength > 0)
            {
                string _FileName = Path.GetFileName(file.FileName);
                string logoPath = Path.Combine(Server.MapPath("~/AgentLogo"), _FileName);
                path = "/AgentLogo/"+_FileName;

                file.SaveAs(logoPath);
            }
            return path;
        }

        public void SaveAgentLogo(int customerId, string imagePath)
        {

            var agentLogoDetails = InsuranceContext.AgentLogos.Single(where : "CustomerId=" + customerId);

            if(agentLogoDetails == null)
            {
                AgentLogo agentLogo = new AgentLogo { CustomerId = customerId, LogoPath = imagePath, CreatedOn = DateTime.Now };
                InsuranceContext.AgentLogos.Insert(agentLogo);
            }
            else
            {
                agentLogoDetails.LogoPath = imagePath;
                InsuranceContext.AgentLogos.Update(agentLogoDetails);
            }


        }


        // GET: Agent/Edit/5
        public ActionResult Edit(int id)
        {

            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            string path = Server.MapPath("~/Content/Countries.txt");
            var _countries = System.IO.File.ReadAllText(path);
            var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(_countries);
            ViewBag.Countries = resultt.countries;


            //string paths = Server.MapPath("~/Content/Cities.txt");
            //var _cities = System.IO.File.ReadAllText(paths);
            //var resultts = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObjects>(_cities);
            //ViewBag.Cities = resultts.cities;

            ViewBag.Cities = InsuranceContext.Cities.All();
            ViewBag.Branches = InsuranceContext.Branches.All();



            if (userLoggedin)
            {
                var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();
                // var role = UserManager.GetRoles(userid).FirstOrDefault();
                //if (role != "SuperAdmin")
                //{
                //    return RedirectToAction("Index", "CustomerRegistration");
                //}
            }
            else
            {
                return RedirectToAction("Index", "Agent");
            }

            CustomerModel obj = new CustomerModel();
            List<IdentityRole> roles = roleManager.Roles.ToList();


            InsuranceClaim.Models.RoleManagementListViewModel _roles = new RoleManagementListViewModel();

            _roles.RoleList = roles;
            ViewBag.Adduser = _roles.RoleList;



            if (id != 0)
            {
                var data = InsuranceContext.Customers.Single(id);
                var branchs = InsuranceContext.Branches.Single(data.BranchId) == null ? "" : InsuranceContext.Branches.Single(data.BranchId).BranchName;
                var user = UserManager.FindById(data.UserID);
                var email = user.Email;
                var phone = user.PhoneNumber;
                var role = UserManager.GetRoles(data.UserID).FirstOrDefault();


                // for corporate

                if(data.IsCorporate)
                {
                    // data.EmailAddress = model.CompanyEmail;
                    obj.CompanyEmail = data.CompanyEmail;
                    obj.CompanyName = data.CompanyName;
                    obj.CompanyAddress = data.CompanyAddress;
                    obj.CompanyPhone = data.CompanyPhone;
                    obj.CompanyCity = data.CompanyCity;
                    obj.CompanyBusinessId = data.CompanyBusinessId;
                    obj.IsCorporate = data.IsCorporate;
                }


                obj.FirstName = data.FirstName;
                obj.LastName = data.LastName;
                obj.AddressLine1 = data.AddressLine1;
                obj.AddressLine2 = data.AddressLine2;
                obj.City = data.City;
                //  obj.Branch = Convert.ToString(data.BranchId);

                obj.AgentBranch = data.AgentBranch;
                obj.CountryCode = data.Countrycode;
                obj.Gender = data.Gender;
                obj.Id = data.Id;
                obj.DateOfBirth = data.DateOfBirth;
                obj.NationalIdentificationNumber = data.NationalIdentificationNumber;
                obj.Zipcode = data.Zipcode;
                obj.role = role;
                obj.PhoneNumber = Convert.ToString(phone);
                obj.EmailAddress = Convert.ToString(email);
                obj.IsActive = data.IsActive;
                obj.AgentWhatsapp = data.AgentWhatsapp;

            }
            return View(obj);



        }

        // POST: Agent/Edit/5
        [HttpPost]
        public ActionResult Edit(CustomerModel model, HttpPostedFileBase file, string Corporate)
        {
            try
            {
                // TODO: Add update logic here

                string path = Server.MapPath("~/Content/Countries.txt");
                var _countries = System.IO.File.ReadAllText(path);
                var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(_countries);
                ViewBag.Countries = resultt.countries;

                ViewBag.Cities = InsuranceContext.Cities.All();
                ViewBag.Branches = InsuranceContext.Branches.All();




                Customer ctems = InsuranceContext.Customers.Single(model.Id);
                var user = UserManager.FindById(ctems.UserID);
                var role = UserManager.GetRoles(ctems.UserID).FirstOrDefault();
                user.PhoneNumber = model.PhoneNumber;

                //if (role == null)
                //{
                //    UserManager.AddToRole(user.Id, model.role);
                //}
                //else if (role != model.role)
                //{
                //    UserManager.RemoveFromRole(user.Id, role);
                //    UserManager.AddToRole(user.Id, model.role);
                //    //update role                    
                //}



                if (Corporate == "")
                {
                    model.IsCorporate = true;
                    model.EmailAddress = model.CompanyEmail;
                    model.CompanyEmail = model.CompanyEmail;

                    model.FirstName = model.CompanyName;
                    model.CompanyName = model.CompanyName;

                    model.AddressLine1 = model.CompanyAddress;
                    model.CompanyAddress = model.CompanyAddress;

                    model.PhoneNumber = model.CompanyPhone;
                    model.CompanyPhone = model.CompanyPhone;

                    model.CompanyCity = model.CompanyCity;
                    model.City = model.CompanyCity;

                    model.CompanyBusinessId = model.CompanyBusinessId;

                }





                ctems.CustomerId = model.CustomerId;
                ctems.Id = ctems.Id;
                ctems.AddressLine1 = model.AddressLine1;
                ctems.AddressLine2 = model.AddressLine2;
                ctems.City = model.City;
                ctems.AgentBranch = model.HdnBrach;
                ctems.Countrycode = model.CountryCode;
                ctems.DateOfBirth = model.DateOfBirth;
                ctems.FirstName = model.FirstName;
                ctems.LastName = model.LastName;
                ctems.NationalIdentificationNumber = model.NationalIdentificationNumber;
                ctems.Zipcode = model.Zipcode;
                ctems.Gender = model.Gender;
                ctems.Country = model.Country;
                ctems.IsActive = model.IsActive;
                ctems.IsLicenseDiskNeeded = model.IsLicenseDiskNeeded;
                ctems.IsOTPConfirmed = model.IsOTPConfirmed;
                ctems.IsPolicyDocSent = model.IsPolicyDocSent;
                ctems.IsWelcomeNoteSent = model.IsWelcomeNoteSent;
                ctems.PhoneNumber = model.PhoneNumber;


                if (model.IsCorporate)
                {
                    ctems.IsCorporate = true;
                    ctems.CompanyName = model.CompanyName;
                    ctems.CompanyEmail = model.CompanyEmail;
                    ctems.CompanyAddress = model.CompanyAddress;
                    ctems.CompanyPhone = model.CompanyPhone;
                    ctems.CompanyCity = model.CompanyCity;
                    ctems.CompanyBusinessId = model.CompanyBusinessId;
                    ctems.LastName = " ";
                }



                ctems.AgentWhatsapp = model.AgentWhatsapp;
                InsuranceContext.Customers.Update(ctems);
                // UserManager.Update(user);

                if(file!=null)
                {
                    string imagePath = SaveAgentLogoPath(file);
                    SaveAgentLogo(ctems.Id, imagePath);
                }
              


            }
            catch (Exception ex)
            {

                model.ErrorMsg = ex.Message;

                return View(model);
            }

            return RedirectToAction("Index");
        }



        public ActionResult DeleteUserManagement(int id)
        {
            var data = InsuranceContext.Customers.Single(id);

            var userid = data.UserID;
            data.IsActive = false;
            InsuranceContext.Customers.Update(data);
            // InsuranceContext.Customers.Delete(data);

            //  var currentUser = UserManager.FindById(userid);
            //  UserManager.Delete(currentUser);

            return RedirectToAction("index");
        }



        // GET: Agent/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Agent/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
