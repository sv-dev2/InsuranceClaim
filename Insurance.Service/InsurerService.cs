using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Insurance.Domain;
using InsuranceClaim.Models;
using AutoMapper;
namespace Insurance.Service
{
    public class InsurerService
    {

        string _superUser = "fe19c887-f8a9-4353-939f-65e19afe0D5L";

        public List<InsurerModel> GetInsurers()
        {
            var list = InsuranceContext.PolicyInsurers.All().ToList();
            var model = Mapper.Map<List<PolicyInsurer>,List<InsurerModel>>(list);
            return model;
        }


        


    }
}
