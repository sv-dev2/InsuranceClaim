using InsuranceClaim.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InsuranceClaim.Startup))]
namespace InsuranceClaim
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createRolesandUsers();
        }
        // In this method we will create default User roles and Admin user for login    
        private void createRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


            //// In Startup iam creating first Admin Role and creating a default Admin User     
            //if (!roleManager.RoleExists("SuperAdmin"))
            //{

            //    // first we create Admin rool    
            //    var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
            //    role.Name = "SuperAdmin";
            //    roleManager.Create(role);

            //    //Here we create a Admin super user who will maintain the website                   

            //    var user = new ApplicationUser();
            //    user.UserName = "admin@kindlebit.com";
            //    user.Email = "admin@kindlebit.com";

            //    string userPWD = "Kindle@123";

            //    var chkUser = UserManager.Create(user, userPWD);

            //    //Add default User to Role Admin    
            //    if (chkUser.Succeeded)
            //    {
            //        var result1 = UserManager.AddToRole(user.Id, "SuperAdmin");

            //    }
            //}

            // creating employee role     
            //if (!roleManager.RoleExists("Administrator"))
            //{
            //    var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
            //    role.Name = "AdministratorStaff";
            //    roleManager.Create(role);

            //}

            //// creating Agent role     
            //if (!roleManager.RoleExists("Staff"))
            //{
            //    var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
            //    role.Name = "Service_Provider";
            //    roleManager.Create(role);

            //}
            //// creating Customer role     
            //if (!roleManager.RoleExists("Customer"))
            //{
            //    var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
            //    role.Name = "Customer";
            //    roleManager.Create(role);

            //}
        }
    }
}
