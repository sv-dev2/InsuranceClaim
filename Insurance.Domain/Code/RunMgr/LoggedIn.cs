using SV.Domain.Code;
using System;
using System.Linq;
using System.Web.Security;
using Insurance.Domain;
using WebMatrix.WebData;

namespace Insurance.Domain.Code
{
	public class LoggedIn
	{
		//private User LoggedInUser { get; set; }

		public void InitDb()
		{
			

			//foreach(var role in SvConstraints.Instance.SvRoles)
			//{
			//	if(!Roles.RoleExists(role.Name))
			//	{
			//		Roles.CreateRole(role.Name);
			//	}
			//}

			//CreateUser("admin@sevenverbs.com", "secret123", SvConstraints.Instance.SvRolesAdmin);

			
		}

		//public User CreateUser(string userId, string pw, string role)
		//{
		//	// Create Admin user
		//	if(!WebSecurity.IsConfirmed(userId))
		//	{
		//		WebSecurity.CreateUserAndAccount(userId, pw);
		//		Roles.AddUserToRole(userId, role);
		//	}

		//	return InsuranceContext.Users.FindUsersByUserId(userId);
		//}

		//public void AuthenticatedUser(User authenticatedUser, int? companyId)
		//{
		//	LoggedInUser = authenticatedUser;

		//	if(companyId == null)
		//		throw new Exception("Company Id can not be null for: " + authenticatedUser.UserName);

		//	Logger.Instance.Info("Logged In: " + LoggedInUser.UserId);
		//}
	}
}
