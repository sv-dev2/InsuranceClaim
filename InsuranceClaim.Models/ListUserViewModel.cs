using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace InsuranceClaim.Models
{
   public class ListUserViewModel
    {
        public List<CustomerModel> ListUsers { get; set; }

    }
    public class DataClass
    {
        public List<ListUserViewModel> Listuser { get; set; }

    }
}
