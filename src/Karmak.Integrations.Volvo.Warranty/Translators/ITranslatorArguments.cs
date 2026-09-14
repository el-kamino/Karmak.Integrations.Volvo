namespace Karmak.Integrations.Volvo.Warranty.Translators
{
    public interface ITranslatorArguments<TSource>
    {
        TSource Source { get; set; }
    }
}
