using System;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Persistence.React
{
    public class PartsSalesOrderSearchCriteria
    {
        public DateTime? WindowStart { get; set; }
        public DateTime? WindowEnd { get; set; }
        public IEnumerable<string> InvoiceNumbers { get; set; }

        public PartsSalesOrderSearchCriteria()
        {
            InvoiceNumbers = new List<string>();
        }

        public bool IsProperlyConstrained => IsDateConstrained || IsInvoiceConstrained;
        public bool IsDateConstrained => (WindowStart.HasValue && WindowEnd.HasValue);
        public bool IsInvoiceConstrained => InvoiceNumbers.Any();
    }
}
