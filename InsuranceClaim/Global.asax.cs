using AutoMapper;
using Insurance.Domain;
using Insurance.Domain.Code;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace InsuranceClaim
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RunMgr.Instance.Init();
            Map();
           
        }
        private void Map()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Customer, CustomerModel>().ReverseMap();
                cfg.CreateMap<PolicyDetailModel, PolicyDetail>().ReverseMap();
                cfg.CreateMap<RiskDetailModel, VehicleDetail>().ReverseMap();

                cfg.CreateMap<VehicleModel, ClsVehicleModel>().ReverseMap();
                cfg.CreateMap<Product, ProductModel>().ReverseMap();
                cfg.CreateMap<AgentCommission, AgentCommissionModel>().ReverseMap();
                cfg.CreateMap<PolicyInsurer, PolicyInsurerModel>().ReverseMap();
                cfg.CreateMap<CoverType, CovertypeModel>().ReverseMap();
                cfg.CreateMap<VehicleUsage, VehicleUsageModel>().ReverseMap();
                cfg.CreateMap<InflationFactorModel, InflationFactor>().ReverseMap();
                cfg.CreateMap<InsurerModel, PolicyInsurer>().ReverseMap();
                cfg.CreateMap<SummaryDetail, SummaryDetailModel>().ReverseMap();
                cfg.CreateMap<SmsLog, SmsLogModel>().ReverseMap();
                cfg.CreateMap<Setting, SettingModel>().ReverseMap();
                cfg.CreateMap<UserManagementView, UserManagementViewModel>().ReverseMap();
                cfg.CreateMap<SummaryVehicleDetail, SummaryVehicleDetailsModel>().ReverseMap();
                cfg.CreateMap<ReinsuranceBroker, ReinsuranceBrokerModel>().ReverseMap();
                cfg.CreateMap<VehicleMake, VehiclesMakeModel>().ReverseMap();
                cfg.CreateMap<EndorsementRiskDetailModel, EndorsementVehicleDetail>().ReverseMap();

                //Endorsement mapper//

                cfg.CreateMap<EndorsementPolicyDetail, PolicyDetail>().ReverseMap();
                cfg.CreateMap<EndorsementSummaryDetail, SummaryDetail>().ReverseMap();
                cfg.CreateMap<EndorsementPolicyDetailModel, PolicyDetail>().ReverseMap();
                cfg.CreateMap<EndorsementPolicyDetail, EndorsementPolicyDetailModel>().ReverseMap();
                cfg.CreateMap<Customer, EndorsementCustomerModel>().ReverseMap();
                cfg.CreateMap<EndorsementCustomer, EndorsementCustomerModel>().ReverseMap();

                //Receipt Module
                cfg.CreateMap<SummaryDetail, ReceiptModuleModel>().ReverseMap();

                //Second Phase Work 
                cfg.CreateMap<ClaimNotification, ClaimNotificationModel>().ReverseMap();
                cfg.CreateMap<ServiceProvider, ServiceProviderModel>().ReverseMap();
                cfg.CreateMap<ClaimRegistration, ClaimRegistrationModel>().ReverseMap();
                cfg.CreateMap<ClaimAdjustment, ClaimAdjustmentModel>().ReverseMap();
                cfg.CreateMap<ClaimDetailsProvider, ClaimDetailsProviderModel>().ReverseMap();
                cfg.CreateMap<ClaimSetting, ClaimSettingModel>().ReverseMap();
                cfg.CreateMap<EndorsementVehicleDetail, EndorsementRiskDetailModel>().ReverseMap();
                cfg.CreateMap<VehicleDetail, EndorsementRiskDetailModel>().ReverseMap();
                cfg.CreateMap<BirthdayMessage, BirthdayMessageModel>().ReverseMap();
                cfg.CreateMap<EndorsementSummaryDetail, EndorsementSummaryDetailModel>().ReverseMap();
                cfg.CreateMap<SummaryDetail, EndorsementSummaryDetailModel>().ReverseMap();
                cfg.CreateMap<EndorsementSummaryVehicleDetail, EndorsementSummaryVehicleDetailModel>().ReverseMap();
                cfg.CreateMap<SourceDetail, SourceDetailModel>().ReverseMap();

                cfg.CreateMap<VehicleDetail, EndorsementVehicleDetail>();

                //30Jan D

                cfg.CreateMap<EndorsementCustomerModel, Customer>().ReverseMap();
                //08/May D
                cfg.CreateMap<QRCode, QRCode>().ReverseMap();

                cfg.CreateMap<VehicleTaxClass, VehicleTaxClassModel>().ReverseMap();

                // domestic
                cfg.CreateMap<DomesticRiskDetailModel, Domestic_Vehicle>().ReverseMap();
                cfg.CreateMap<DomesticSummaryModel, DomesticSummaryDetail>().ReverseMap();         
            });
        }
    }
}
