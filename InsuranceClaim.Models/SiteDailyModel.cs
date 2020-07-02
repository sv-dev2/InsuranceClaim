using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsuranceClaim.Models
{
    public class SiteDailyModel
    {
        public string BranchName { get; set; }
        public int one { get; set; }
        public int two { get; set; }
        public int three { get; set; }
        public int four { get; set; }
        public int five { get; set; }
        public int six { get; set; }
        public int seven { get; set; }
        public int eight { get; set; }
        public int nine { get; set; }
        public int ten { get; set; }
        public int eleven { get; set; }
        public int twelve { get; set; }
        public int thirteen { get; set; }
        public int fourteen { get; set; }
        public int fifteen { get; set; }
        public int sixteen { get; set; }
        public int seventeen { get; set; }
        public int eighteen { get; set; }
        public int nineteen { get; set; }
        public int twenty { get; set; }
        public int twentyone { get; set; }
        public int twentytwo { get; set; }
        public int twentythree { get; set; }
        public int twentyfour { get; set; }
        public int twentyfive { get; set; }
        public int twentysix { get; set; }
        public int twentyseven { get; set; }
        public int twentyeight { get; set; }
        public int twentynine { get; set; }
        public int thirty { get; set; }
        public int thirtyone { get; set; }


    }

    public class SiteDailySearchModel
    {
        public List<SiteDailyModel> SiteDailyList { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MMM/yyyy}")]
        public int SelectedMonth
        {
            get;
            set;
        }
        public int SelectedYear
        {
            get;
            set;
        }
    }
}
