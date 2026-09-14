namespace Karmak.Integrations.Volvo.Common.Settings.Exceptions;

public class NoSettingsFoundException : Exception
{
    public NoSettingsFoundException() 
        : base("No settings found for context.") 
    {
    }
}