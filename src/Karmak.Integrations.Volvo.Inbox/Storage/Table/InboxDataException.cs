namespace Karmak.Integrations.Volvo.Inbox.Storage.Table;

public class InboxDataException : Exception
{
    public InboxDataException(string message) : base(message)
    {
    }

    public InboxDataException(string message, Exception innerException) : base(message, innerException)
    {
    }
}