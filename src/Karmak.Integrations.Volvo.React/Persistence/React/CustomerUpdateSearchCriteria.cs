using System;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Persistence.React
{
    public class CustomerUpdateSearchCriteria
    {
        public DateTime? WindowStart { get; set; }
        public DateTime? WindowEnd { get; set; }
        public IEnumerable<string> VolvoPassIds { get; set; }
        public IEnumerable<string> VINs { get; set; }

        public CustomerUpdateSearchCriteria()
        {
            VolvoPassIds = new List<string>();
            VINs = new List<string>();
        }

        public bool IsProperlyConstrained => IsDateConstrained || IsVolvoPassIdConstrained || IsVinConstrained;
        public bool IsDateConstrained => (WindowStart.HasValue && WindowEnd.HasValue);
        public bool IsVolvoPassIdConstrained => VolvoPassIds.Any();
        public bool IsVinConstrained => VINs.Any();
    }
}
