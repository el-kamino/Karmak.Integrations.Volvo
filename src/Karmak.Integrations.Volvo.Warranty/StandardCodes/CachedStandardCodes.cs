using System;
using System.Collections.Generic;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.StandardCodes
{
    public class CachedStandardCodes
    {
        public IEnumerable<StandardCode> Codes { get; set; }
        public DateTimeOffset CachedAt { get; set; }
    }
}
