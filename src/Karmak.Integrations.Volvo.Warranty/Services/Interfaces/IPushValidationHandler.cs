using System.Collections.Generic;
using Karmak.Integrations.Volvo.Warranty.Contracts;

namespace Karmak.Integrations.Volvo.Warranty.Services.Interfaces
{
    public interface IPushValidationHandler
    {
        IEnumerable<ValidatedUpdateSnapshotMessage> Validate(IEnumerable<UpdateSnapshotMessage> updates);
    }
}
