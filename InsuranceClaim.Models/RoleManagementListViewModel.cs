using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class RoleManagementListViewModel
    {
        public List<Microsoft.AspNet.Identity.EntityFramework.IdentityRole> RoleList { get; set; }
        
    }

    public class RoleViewModel
    {
        public string Id { get; set; }
        public string RoleName { get; set; }

    }
}
