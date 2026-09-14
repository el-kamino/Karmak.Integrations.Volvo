namespace Karmak.Integrations.Volvo.Inbox.Storage.Table;

public static class StringExtensions
{
    public static string ToCapitalized(this string value)
    {
        if (value.Length < 1)
            return value;

        var chars = value.ToArray();
        chars[0] = char.ToUpper(chars[0]);
        
        return new string(chars);
    }
}