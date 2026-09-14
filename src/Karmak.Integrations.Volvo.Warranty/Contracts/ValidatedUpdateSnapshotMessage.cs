using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public class ValidatedUpdateSnapshotMessage
    {
        public IEnumerable<string> Errors { get; set; }
        public UpdateSnapshotMessage UpdateSnapshotMessage { get; set; }
    }
}
