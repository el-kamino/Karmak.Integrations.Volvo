namespace Karmak.Integrations.Volvo.Dcds;

public class Utility
{
    public static DateTime AtOffsetInHours(DateTime dt, decimal? hours)
    {
        if (dt == default)
        {
            return dt;
        }

        if (hours == null)
        {
            return dt;
        }

        return dt.AddHours((double)hours);
    }
}
