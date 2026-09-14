using System.Collections.Generic;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;

namespace Karmak.Integrations.Volvo.Warranty.StandardCodes
{
    public class OWSStandardCodeResponse
    {
        public List<StandardCodesLineItemsType> ConditionCodes { get; set; }
        public List<StandardCodesLineItemsType> DamageCodes { get; set; }
        public List<StandardCodesLineItemsType> CustomerConcernCodes { get; set; }
    }
}
