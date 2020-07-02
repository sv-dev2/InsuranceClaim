using Insurance.Domain;
using Insurance.Service;
using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using static InsuranceClaim.Controllers.CustomerRegistrationController;

namespace InsuranceClaim.Controllers
{
    public class AgentStaffController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        string AdminEmail = WebConfigurationManager.AppSettings["AdminEmail"];
        RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
        Insurance.Service.smsService objsmsService = new Insurance.Service.smsService();
        SummaryDetailService _summaryDetailService = new SummaryDetailService();


        string _staff = "bbbeffe0-94fa-41b7-bd8b-72d9ddc7HGTR";
        string _agentStaff = "AgentStaff";



        public AgentStaffController()
        {

        }

        public AgentStaffController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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



        // GET: AgentStaff
        public ActionResult Index()
        {

            int loggedCustomerId = 0;

            bool userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;



            if (userLoggedin)
            {
                var userid = System.Web.HttpContext.Current.User.Identity.GetUserId();

                var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{userid}'").FirstOrDefault();

                if (_customerData != null)
                {
                    loggedCustomerId = _customerData.Id;
                }



                //  var roles = UserManager.GetRoles(userid).FirstOrDefault();
                //if (roles != "SuperAdmin")
                //{
                //    return RedirectToAction("Index", "CustomerRegistration");
                //}
            }
            else
            {
                return RedirectToAction("Index", "Agent");
            }

            var branchList = InsuranceContext.Branches.All();


            var query = "select Customer. *, AspNetUsers.Email  from Customer ";
            query += "   join AspNetUsers on Customer.UserID = AspNetUsers.Id ";
            query += "  join AspNetUserRoles  on AspNetUserRoles.UserId = AspNetUsers.Id ";
            query += " where AspNetUserRoles.RoleId ='" + _staff + "'  and CreatedBy=" + loggedCustomerId +" and (IsActive is null or IsActive=1) ";


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
                IsOTPConfirmed = x.IsOTPConfirmed==null ,
                CreatedBy = x.CreatedBy,
                ModifiedOn = x.ModifiedOn,
                ModifiedBy = x.ModifiedBy,
                IsActive = x.IsActive==null ? false : Convert.ToBoolean(x.IsActive),
                CountryCode = x.Countrycode,
                PhoneNumber = x.PhoneNumber,
                IsCustomEmail = x.IsCustomEmail==null ? false : Convert.ToBoolean(x.IsCustomEmail),
                EmailAddress = x.Email,
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
                Branch = branchList.FirstOrDefault(c => c.Id == x.BranchId) == null ? "" : branchList.FirstOrDefault(c => c.Id == x.BranchId).BranchName
            }).ToList();




            List<CustomerModel> ListUserViewModel = new List<CustomerModel>();


            ListUserViewModel lstUserModel = new ListUserViewModel();
            lstUserModel.ListUsers = user;

            return View(lstUserModel);
        }

        // GET: AgentStaff/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AgentStaff/Create
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
         //   List<IdentityRole> roles = roleManager.Roles.ToList();



            var customer = InsuranceContext.Customers.Single(where: "UserID='" + userid + "'");

            if (customer != null)
            {
                ViewBag.Branches = InsuranceContext.Branches.All(where: "id in ("+customer.AgentBranch+")");
            }
            else
            {
                ViewBag.Branches = InsuranceContext.Branches.All();
            }



            obj.Zipcode = "00263";



            return View(obj);
        }

        // POST: AgentStaff/Create
        [HttpPost]
        public async Task<ActionResult> Create(CustomerModel model)
        {
            try
            {


                string path = Server.MapPath("~/Content/Countries.txt");
                var _countries = System.IO.File.ReadAllText(path);
                var resultt = Newtonsoft.Json.JsonConvert.DeserializeObject<RootObject>(_countries);
                ViewBag.Countries = resultt.countries;
                ViewBag.Cities = InsuranceContext.Cities.All();
                ViewBag.Branches = InsuranceContext.Branches.All();


                decimal custId = 0.00m;
                var user = new ApplicationUser { UserName = model.EmailAddress, Email = model.EmailAddress, PhoneNumber = model.PhoneNumber };
                var result = await UserManager.CreateAsync(user, "Geninsure@123");
                if (result.Succeeded)
                {
                    var currentUser = UserManager.FindByName(user.UserName);
                    var roleresult = UserManager.AddToRole(currentUser.Id, _agentStaff);

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
                    cstmr.BranchId = Convert.ToInt32(model.Branch);
                    cstmr.Countrycode = model.CountryCode;
                    cstmr.DateOfBirth = model.DateOfBirth;
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

                    bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                    if (_userLoggedin)
                    {
                        var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                        var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();

                        if (_customerData != null)
                        {
                            cstmr.CreatedBy = _customerData.Id;
                        }
                    }



                    InsuranceContext.Customers.Insert(cstmr);

                }

            }
            catch (Exception ex)
            {
                model.ErrorMsg = ex.Message;
                return View(model);

            }


            return RedirectToAction("Index");

        }

        // GET: AgentStaff/Edit/5
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
            //  ViewBag.Branches = InsuranceContext.Branches.All();

            var userid = "";

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
                //  var branchs = InsuranceContext.Branches.Single(data.BranchId) == null ? "" : InsuranceContext.Branches.Single(data.BranchId).BranchName;

                //   userid = System.Web.HttpContext.Current.User.Identity.GetUserId();

                //    var branchs = InsuranceContext.Branches.Single(data.BranchId) == null ? "" : InsuranceContext.Branches.Single(data.BranchId).BranchName;


               // ViewBag.Branches= InsuranceContext.Branches.All(where: "Id=" + data.BranchId);


                var customer = InsuranceContext.Customers.Single(where: "UserID='" + userid + "'");

                if (customer != null)
                {
                    ViewBag.Branches = InsuranceContext.Branches.All(where: "id in (" + customer.AgentBranch + ")");
                }
                






                var user = UserManager.FindById(data.UserID);
                var email = user.Email;
                var phone = user.PhoneNumber;
                var role = UserManager.GetRoles(data.UserID).FirstOrDefault();

                obj.FirstName = data.FirstName;
                obj.LastName = data.LastName;
                obj.AddressLine1 = data.AddressLine1;
                obj.AddressLine2 = data.AddressLine2;
                obj.City = data.City;
                obj.Branch = Convert.ToString(data.BranchId);
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
            }


          



            return View(obj);



        }

        // POST: AgentStaff/Edit/5
        [HttpPost]
        public ActionResult Edit(CustomerModel model)
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


                ctems.CustomerId = model.CustomerId;
                ctems.Id = ctems.Id;
                ctems.AddressLine1 = model.AddressLine1;
                ctems.AddressLine2 = model.AddressLine2;
                ctems.City = model.City;
                ctems.BranchId = Convert.ToInt32(model.Branch);
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
                ctems.CreatedBy = ctems.CreatedBy;

                bool _userLoggedin = (System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
                if (_userLoggedin)
                {
                    var _User = UserManager.FindById(User.Identity.GetUserId().ToString());
                    var _customerData = InsuranceContext.Customers.All(where: $"UserId ='{_User.Id}'").FirstOrDefault();
                    if (_customerData != null)
                    {
                        ctems.ModifiedBy = _customerData.Id;
                    }
                }

                InsuranceContext.Customers.Update(ctems);
                //UserManager.Update(user);
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


        // GET: AgentStaff/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AgentStaff/Delete/5
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
