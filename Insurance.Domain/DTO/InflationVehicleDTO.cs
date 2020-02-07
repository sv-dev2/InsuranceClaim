using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Insurance.Domain
{
    public class InflationVehicleDTO
    {
        public IEnumerable<VehicleUsage> VehicleUsages { get; set; }
        public InflationFactor InflationFactor { get; set; }
    }

}
