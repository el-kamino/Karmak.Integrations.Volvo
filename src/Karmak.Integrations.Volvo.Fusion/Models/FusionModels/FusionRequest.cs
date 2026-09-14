namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels;

[Serializable]
public class FusionRequest<T>
{
    public DateTime CreatedDateTime { get; set; }
    public decimal CreatedTimeZone { get; set; }
    public string Action { get; set; }
    public string EntityType { get; set; }
    public int EntityId { get; set; }
    public T Payload { get; set; }
    public bool ForceProcessing { get; set; }
}