using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{


    public class ChecklistModel
    {
        public int Id { get; set; }
        public string ChecklistDetail { get; set; }

        public List<RegisterClaimViewModel> checklistData { get; set; }

        public bool IsChecked { get; set; }

    }
    //public class ChecklistModel
    //{
    //    public int Id { get; set; }
    //    public string ChecklistDetail { get; set; }

    //    public bool IsChecked { get; set; }

    //}
}
