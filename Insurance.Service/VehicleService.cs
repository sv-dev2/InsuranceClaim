using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using InsuranceClaim.Models;

namespace Insurance.Service
{
    public class VehicleService
    {
        public List<VehicleMake> GetMakers()
        {
            var list = InsuranceContext.VehicleMakes.All().ToList();
            return list;
        }

        public List<ClsVehicleModel> GetModel(string makeCode)
        {
            var list = InsuranceContext.VehicleModels.All(where: $"MakeCode='{makeCode}'").ToList();
            
            var map = Mapper.Map<List<VehicleModel>, List<ClsVehicleModel>>(list);
            return map;

        }

        //VehicleTaxClassModel


        public List<VehicleTaxClassModel> GetVehicleTax(string VehicleType)
        {

            var product = InsuranceContext.Products.Single(where: $"Id='{VehicleType}'");

            int vehicleTypeId = 0;

            if(product!=null)
            {
                vehicleTypeId = product.VehicleTypeId;
            }

            var list = InsuranceContext.VehicleTaxClasses.All(where: $"VehicleType='{vehicleTypeId}'").ToList();

         //  var list = InsuranceContext.VehicleTaxClasses.All(where: $"VehicleUsageId='{VehicleType}'").ToList();


            var map = Mapper.Map<List<VehicleTaxClass>, List<VehicleTaxClassModel>>(list);
            return map;

        }


        public List<CoverType> GetCoverType()
        {
            var list = InsuranceContext.CoverTypes.All(where: $"IsActive=1").ToList();
            return list;
        }
        public List<AgentCommission> GetAgentCommission()
        {
            var list = InsuranceContext.AgentCommissions.All().ToList();
            return list;
        }
        public List<VehicleUsage> GetVehicleUsage(string PolicyName)
        {
            var list = InsuranceContext.VehicleUsages.All(where: $"ProductId='{PolicyName}'").ToList();
            return list;
        }
        public List<VehicleUsage> GetAllVehicleUsage()
        {
            var list = InsuranceContext.VehicleUsages.All().ToList();
            return list;
        }

        public PolicyDetail GetPolicy(int policyId)
        {
            var policy = InsuranceContext.PolicyDetails.Single(policyId);
            return policy;
        }
        public VehicleDetail GetVehicles(int policyId)
        {
            var Vehicleinfo = InsuranceContext.VehicleDetails.Single(policyId);
            return Vehicleinfo;
        }
    }
}
