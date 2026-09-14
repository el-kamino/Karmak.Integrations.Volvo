using System;

namespace Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Messages;

public class RetransmitVehicleSales 
{
    public string BlobName { get; set; }
    public Guid RequestCorrelationGuid { get; set; }
    public int Index { get; set; }
    public int Total { get; set; }
}
