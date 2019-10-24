using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class PolicyRenewReminderSettingViewModel
    {
        public int Id { get; set; }
        public int NoOfDays { get; set; }
        public int NotificationType { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public bool SMS { get; set; }
        public bool Email { get; set; }
    }
}
