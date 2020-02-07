using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Service
{
    public class InflationFactorService
    {
        public List<InflationFactor> getAll()
        {
            var inflationFactors = InsuranceContext.InflationFactors.
                 All(where: "IsActive is not null").
                OrderByDescending(x => x.CreatedOn).ToList();
            return inflationFactors;
        }

        public InflationFactor getActiveInflationfactor()
        {
            var inflationFactor = InsuranceContext.InflationFactors.All(where: "IsActive = 1").FirstOrDefault();

            return inflationFactor;

        }
    }

}
