using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCS_Model.DTOs
{
    public class UnitConversionDTO
    {
        public int FromUnitId { get; set; }
        public int ToUnitId { get; set; }
        public decimal Factor { get; set; }
        public int Level { get; set; }
    }
}
