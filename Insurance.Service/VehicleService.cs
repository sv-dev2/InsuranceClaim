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

       

        public Product GetVehicleTypeByProductId(int productId)
        {
            return InsuranceContext.Products.Single(productId);
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

        public List<Product> GetAllProducts()
        {
            return InsuranceContext.Products.All().ToList();
        }

        public List<Domestic_Product> GetDemosticProducts()
        {
            return InsuranceContext.Domestic_Products.All(where: "ProductName = 'Domestic All In One'").ToList();
        }

        public List<RiskCoverModel> Domestic_RiskCovers(int ProductId)
        {
            return InsuranceContext.Domestic_RiskCovers.All(where: $"ProductId='{ProductId}'").ToList().Select(x => new RiskCoverModel { Id = x.Id, RiskCover = x.CoverName }).ToList();     
        }

        public Domestic_RiskItem Domestic_RiskItem(int riskId)
        {
            return InsuranceContext.Domestic_RiskItems.Single(riskId);
        }

      
        public List<VehicleTaxClass> GetAllTaxClasses()
        {
            return InsuranceContext.VehicleTaxClasses.All().ToList();
        }

        public List<PaymentTerm> GetAllPaymentTerms()
        {
            return InsuranceContext.PaymentTerms.All(where: "IsActive = 'True' or IsActive is null").ToList();
        }

        public List<Currency> GetAllCurrency()
        {
            return InsuranceContext.Currencies.All(where: $"IsActive = 'True'").ToList();
        }

        public List<VehicleUsage> GetVehicleUsageByRiskId(string RiskCoverId)
        {
            var list = InsuranceContext.VehicleUsages.All(where: $"RiskCoverId='{RiskCoverId}'").ToList();
            return list;
        }


        public List<Domestic_RiskItem> GetRiskCoverItem(string RiskCoverId)
        {
            return InsuranceContext.Domestic_RiskItems.All(where: $"CoverId='{RiskCoverId}'").ToList();
        }



    }
}
