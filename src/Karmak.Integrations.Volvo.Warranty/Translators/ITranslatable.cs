namespace Karmak.Integrations.Volvo.Warranty.Translators
{
    public interface ITranslatable<in TArguments, out TDestination>
    {
        TDestination Translate(TArguments args);
    }
}
