using System;

namespace Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Messages;

public sealed class RetransmitCustomerUpdate 
{
    public string BlobName { get; set; }
    public Guid RequestCorrelationGuid { get; set; }
    public int Index { get; set; }
    public int Total { get; set; }
}
