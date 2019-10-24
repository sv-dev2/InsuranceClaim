using InsuranceClaim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Insurance.Domain;
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
                InsuranceContext.VehicleDetails.Insert(db);
                return db.Id;
            }
            catch (Exception ex)
            {

                return 0;
            }


        }
    }
}
