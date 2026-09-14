using System;
using System.Collections.Generic;
using System.Text;

namespace Karmak.Integrations.Volvo.React.Contracts.Common
{
    public class Odometer {
        public decimal Reading { get; set; }
        public OdometerUnitType UnitType { get; set; }
    }
}
