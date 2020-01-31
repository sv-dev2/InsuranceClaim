using AutoMapper;
using Insurance.Domain;
using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Service
{
    public class DomesticService
    {
        public List<Domain.DomesticVehicleSummary> GetAllSummaryVehiclesById(int summaryId)
        {
            return InsuranceContext.DomesticVehicleSummaries.All(where: "SummaryDetailId=" + summaryId).ToList();
        }

        public Domestic_Vehicle GetVehicleDetail(int policyId)
        {
            return InsuranceContext.Domestic_Vehicles.Single(where: "PolicyId=" + policyId);
        }

        public Domestic_Vehicle GetVehicleById(int vehicleId)
        {
            return InsuranceContext.Domestic_Vehicles.Single(where: $" Id='{vehicleId}' and IsActive<>0");
        }


        public int AddVehicleInformation(DomesticRiskDetailModel model)
        {
            try
            {
                var db = Mapper.Map<DomesticRiskDetailModel, Domestic_Vehicle>(model);
                db.IsActive = true;

                if (db.RenewDate.Year == 1) // handling the exception
                {
                    //model.CoverStartDate = DateTime.Now;

                    if (model.PaymentTermId == 1)
                        db.CoverEndDate = model.CoverStartDate.Value.AddMonths(12);
                    else
                        db.CoverEndDate = model.CoverStartDate.Value.AddMonths(model.PaymentTermId);

                    db.RenewDate = model.CoverEndDate.Value.AddDays(1);
                    db.TransactionDate = DateTime.Now;
                    db.PolicyExpireDate = model.CoverEndDate.Value;
                }
                InsuranceContext.Domestic_Vehicles.Insert(db);
                return db.Id;
            }
            catch (Exception ex)
            {

                LogDetailTbl log = new LogDetailTbl();
                log.Request = ex.Message;
                string vehicleInfo = "Domestic_vehicle" + model.RenewPolicyNumber;

                log.Response = vehicleInfo;
                InsuranceContext.LogDetailTbls.Insert(log);

                return 0;
            }
        }


        public void UpdateVehicle(Domestic_Vehicle vehicle)
        {
            InsuranceContext.Domestic_Vehicles.Update(vehicle);
        }

        public List<DomesticVehicleSummary> GetSummaryVehicleDetailsByVehicle(int vehicleId)
        {
            return InsuranceContext.DomesticVehicleSummaries.All(where: $"VehicleDetailsId={vehicleId}").ToList();
        }

        public DomesticSummaryDetail GetSummaryDetail(int summaryId)
        {
            return InsuranceContext.DomesticSummaryDetails.Single(summaryId);
        }

        public int SaveSummaryDetails(DomesticSummaryDetail DbEntry)
        {
            int summaryId = 0;
            try
            {
                InsuranceContext.DomesticSummaryDetails.Insert(DbEntry);
                summaryId = DbEntry.Id;
            }
            catch (Exception ex)
            {

            }
            return summaryId;
        }

        public void UpdateSummaryDetail(DomesticSummaryDetail summaryDetail)
        {
            InsuranceContext.DomesticSummaryDetails.Update(summaryDetail);
        }

        public List<DomesticVehicleSummary> GetSummaryVehicleList(int summaryId)
        {
            return InsuranceContext.DomesticVehicleSummaries.All(where: $"SummaryDetailId={summaryId}").ToList();
        }

        public void DeleteSummaryVehicleDetails(DomesticVehicleSummary SummaryVehicleDetail)
        {
            InsuranceContext.DomesticVehicleSummaries.Delete(SummaryVehicleDetail);
        }

        public void SaveSummaryVehicleDetails(DomesticVehicleSummary SummaryVehicleDetail)
        {
            InsuranceContext.DomesticVehicleSummaries.Insert(SummaryVehicleDetail);
        }

        public DomesticPayment GetPaymentInformationById(int id)
        {
            return InsuranceContext.DomesticPayments.Single(id);
        }

        public void SavePaymentPaymentInformations(DomesticPayment objSaveDetailListModel)
        {
            InsuranceContext.DomesticPayments.Insert(objSaveDetailListModel);
        }

        public void UpdatePaymentInformation(DomesticPayment objSaveDetailListModel)
        {
            InsuranceContext.DomesticPayments.Update(objSaveDetailListModel);
        }



    }
}
