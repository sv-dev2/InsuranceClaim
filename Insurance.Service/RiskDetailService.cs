using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Insurance.Domain;
using System.Configuration;

namespace Insurance.Service
{
    public class RiskDetailService
    {
        public int AddVehicleInformation(RiskDetailModel model)
        {
            try
            {

                var db = Mapper.Map<RiskDetailModel, VehicleDetail>(model);
                db.IsActive = true;

                if (db.RenewalDate.Value.Year == 1) // handling the exception
                {
                    //model.CoverStartDate = DateTime.Now;

                    if (model.PaymentTermId == 1)
                        db.CoverEndDate = model.CoverStartDate.Value.AddMonths(12);
                    else
                        db.CoverEndDate = model.CoverStartDate.Value.AddMonths(model.PaymentTermId);

                    db.RenewalDate = db.CoverEndDate.Value.AddDays(1);
                    db.TransactionDate = DateTime.Now;
                    db.PolicyExpireDate = db.CoverEndDate;
                }
                InsuranceContext.VehicleDetails.Insert(db);
                return db.Id;
            }
            catch (Exception ex)
            {

                LogDetailTbl log = new LogDetailTbl();
                log.Request = ex.Message;
                string vehicleInfo = model.RegistrationNo + "," + model.PaymentTermId + "," + model.CoverTypeId + "," + model.CoverStartDate + "," + model.CoverEndDate + "," + model.VehicleYear + "," + model.Premium + ",";
                vehicleInfo += model.StampDuty + "," + model.ZTSCLevy + "," + model.Discount + "," + model.IncludeRadioLicenseCost + "," + model.RadioLicenseCost + "," + model.VehicleLicenceFee + "," + model.PolicyId;
                log.Response = vehicleInfo;
                InsuranceContext.LogDetailTbls.Insert(log);

                return 0;
            }


        }

        public VehicleDetail GetVehicleDetails(int vehicleId)
        {
            return InsuranceContext.VehicleDetails.Single(vehicleId);
        }
        public PolicyDetail GetPolicyDetails(int PolicyId)
        {
            return InsuranceContext.PolicyDetails.Single(PolicyId);
        }
        public Product GetProductDetails(string ProductName)
        {
            return InsuranceContext.Products.Single(where: "ProductName='" + ProductName + "'");
        }

        public Product GetProductDetailsById(int Id)
        {
            return InsuranceContext.Products.Single(Id);
        }

        public Currency GetCurrencyDetail(int currencyId)
        {
            return InsuranceContext.Currencies.Single(currencyId);
        }

        public Setting GetSettingDetail()
        {
            return InsuranceContext.Settings.Single(where: $"keyname='Discount On Renewal'");
        }

        public VehicleDetail GetVehicleDetailsByVrnPolicyId(int policyId, string vrn)
        {
            return InsuranceContext.VehicleDetails.Single(where: $"policyid= '{policyId}' and RegistrationNo= '{vrn}'");
        }

        public void SaveLicenceDiskDeliveryAddresses(LicenceDiskDeliveryAddress LicenceAddress)
        {
            InsuranceContext.LicenceDiskDeliveryAddresses.Insert(LicenceAddress);
        }

        public LicenceTicket GetLastLicenceTicketsDetail()
        {
            return InsuranceContext.LicenceTickets.All(orderBy: "Id desc").FirstOrDefault();
        }

        public string GetTicketNo(LicenceTicket Licence)
        {
            string TicketNo = "";
            if (Licence != null)
            {
                string number = Licence.TicketNo.Substring(3);
                long tNumber = Convert.ToInt64(number) + 1;
                TicketNo = string.Empty;
                int length = 6;
                length = length - tNumber.ToString().Length;

                for (int i = 0; i < length; i++)
                {
                    TicketNo += "0";
                }
                TicketNo += tNumber;
                var ticketnumber = "GEN" + TicketNo;

                TicketNo = ticketnumber;
            }
            else
            {
                TicketNo = ConfigurationManager.AppSettings["TicketNo"];
            }

            return TicketNo;
        }

        public void SaveLicenceTickets(LicenceTicket LicenceTicket)
        {
            InsuranceContext.LicenceTickets.Insert(LicenceTicket);
        }

        public List<Reinsurance> GetAllReinsurances()
        {
            return InsuranceContext.Reinsurances.All(where: $"Type='Reinsurance'").ToList();
        }

        public decimal? GetReInsurancebyTreatCode()
        {
            return InsuranceContext.Reinsurances.All().Where(x => x.TreatyCode == "OR001").Select(x => x.MaxTreatyCapacity).SingleOrDefault();
        }

        public void SaveReinsuranceTransactions(ReinsuranceTransaction reinsurance)
        {
            InsuranceContext.ReinsuranceTransactions.Insert(reinsurance);
        }

        public VehicleModel GetVehicleModelDetail(string modelCode)
        {
            return InsuranceContext.VehicleModels.Single(where: $"ModelCode='{modelCode}'");
        }

        public VehicleMake GetVehicleMakeDetail(string makeCode)
        {
            return InsuranceContext.VehicleMakes.Single(where: $" MakeCode='{makeCode}'");
        }

        public void UpdateVehicle(VehicleDetail vehicle)
        {
            InsuranceContext.VehicleDetails.Update(vehicle);
        }

        public ReinsuranceBroker GetReinsuranceBrokerDetail(string ReinsuranceBrokerCode)
        {
            return InsuranceContext.ReinsuranceBrokers.Single(where: $"ReinsuranceBrokerCode='{ReinsuranceBrokerCode}'");
        }

        public ReinsuranceTransaction GetReinsuranceTransactionBySummaryId(int summaryId, int vehicleId)
        {
            return InsuranceContext.ReinsuranceTransactions.Single(where: $"SummaryDetailId={summaryId} and VehicleId={vehicleId}");
        }

        public void UpdateReinsuranceTransactions(ReinsuranceTransaction reinsuranceTransaction)
        {
            InsuranceContext.ReinsuranceTransactions.Update(reinsuranceTransaction);
        }

        public void DeleteReinsuranceTransactions(ReinsuranceTransaction reinsuranceTransactions)
        {
            InsuranceContext.ReinsuranceTransactions.Delete(reinsuranceTransactions);
        }

        public ReinsuranceTransaction GetReinsuranceTransactions(int Id)
        {
            return InsuranceContext.ReinsuranceTransactions.Single(Id);
        }

        public void SaveSmsLog(SmsLog objRecieptsmslog)
        {
            InsuranceContext.SmsLogs.Insert(objRecieptsmslog);
        }

        public PaymentInformation GetPaymentInformationById(int id)
        {
            return InsuranceContext.PaymentInformations.Single(id);
        }

        public PaymentInformation GetPaymentInformationBySummaryId(int id)
        {
            return InsuranceContext.PaymentInformations.Single(where: $"SummaryDetailId='{id}'");
        }

        public void SavePaymentPaymentInformations(PaymentInformation objSaveDetailListModel)
        {
            InsuranceContext.PaymentInformations.Insert(objSaveDetailListModel);
        }

        public void UpdatePaymentInformation(PaymentInformation objSaveDetailListModel)
        {
            InsuranceContext.PaymentInformations.Update(objSaveDetailListModel);
        }

        public VehicleUsage GetVehicleUsageById(int Id)
        {
            var vehicleUsage = InsuranceContext.VehicleUsages.Single(Id);
            return vehicleUsage;
        }

        public void SaveCertSerialNoDetails(CertSerialNoDetail model)
        {
            InsuranceContext.CertSerialNoDetails.Insert(model);
        }

    }

}
