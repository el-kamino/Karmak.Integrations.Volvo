namespace Karmak.Integrations.Volvo.Common.Sql.Models;

public abstract class ReactDataEntity<T>
{
    public string KAN { get; set; }
    public string PACode { get; set; }
    public T Entity { get; set; }

    public abstract string EntityType { get; }
    public abstract string EntityId { get; }
}
