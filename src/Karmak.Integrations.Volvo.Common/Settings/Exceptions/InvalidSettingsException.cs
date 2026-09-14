namespace Karmak.Integrations.Volvo.Common.Settings.Exceptions;

public class InvalidSettingsException : Exception
{
    public InvalidSettingsException()
    {
    }

    public InvalidSettingsException(string message) 
        : base(message) 
    {
    }
}