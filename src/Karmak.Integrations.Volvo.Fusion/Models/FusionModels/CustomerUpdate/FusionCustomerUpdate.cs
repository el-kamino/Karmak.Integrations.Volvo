using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared;

namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.CustomerUpdate;

public class FusionCustomerUpdate
{
    public string DatabaseVersion { get; set; }
    public FusionCustomer Customer { get; set; }
    public int VolvoPassRewardID { get; set; }
    public bool SentToVolvo { get; set; }
    public IList<string> CurrentVINs { get; set; }
    public string AddedVIN { get; set; }
    public string RemovedVIN { get; set; }
}
