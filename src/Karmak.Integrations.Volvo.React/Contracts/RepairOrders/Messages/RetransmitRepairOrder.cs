using System;

namespace Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Messages;

public sealed class RetransmitRepairOrder
{
    public string BlobName { get; set; }
    public Guid RequestCorrelationGuid { get; set; }
    public int Index { get; set; }
    public int Total { get; set; }
}
