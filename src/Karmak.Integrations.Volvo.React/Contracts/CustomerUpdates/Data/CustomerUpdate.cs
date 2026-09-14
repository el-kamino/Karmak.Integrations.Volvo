using System;
using System.Collections.Generic;
using Karmak.Integrations.Volvo.React.Contracts.Common;

namespace Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Data;

public class CustomerUpdate 
{
    public DealerInfo DealerInfo { get; set; }
    public PayloadMetadata Metadata { get; set; }
    public bool ForceTransmission { get; set; }
    public DateTime? AddDate { get; set; }
    public Customer Customer { get; set; }
    public string Source { get; set; }
    public decimal? TimeZone { get; set; }
    public int VolvoPassRewardID { get; set; }
    public bool SentToVolvo { get; set; }
    public IList<string> CurrentVINs { get; set; }
    public string AddedVIN { get; set; }
    public string RemovedVIN { get; set; }

    public CustomerUpdate()
    {
        CurrentVINs = new List<string>();
    }
}