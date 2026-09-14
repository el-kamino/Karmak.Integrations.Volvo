namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public interface IMoney
    {
        CurrencyCode Currency { get; }
        decimal Value { get; }
    }
}
