using System.Text;

namespace Karmak.Integrations.Volvo.Dcds.FileUpload;

public static class StringBuilderExtensions
{
    public static void AppendFixedWidthPadLeft(this StringBuilder sb, string value, int width, char? padChar = null)
    {
        if (padChar == null)
            sb.Append(value.PadLeft(width));
        else
            sb.Append(value.PadLeft(width, padChar.Value));
    }

    public static void AppendFixedWidthPadRight(this StringBuilder sb, string value, int width, char? padChar = null)
    {
        if (padChar == null)
            sb.Append(value.PadRight(width));
        else
            sb.Append(value.PadRight(width, padChar.Value));
    }
}