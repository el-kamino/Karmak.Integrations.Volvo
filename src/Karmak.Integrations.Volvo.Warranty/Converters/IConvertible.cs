namespace Karmak.Integrations.Volvo.Warranty.Converters
{
    public interface IConvertible<in TSource, out TDestination>
    {
        TDestination Convert(TSource source);
    }
}
