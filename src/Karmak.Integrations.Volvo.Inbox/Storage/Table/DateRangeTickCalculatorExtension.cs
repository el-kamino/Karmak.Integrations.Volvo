namespace Karmak.Integrations.Volvo.Inbox.Storage.Table;

public static class DateRangeTickCalculatorExtension
{
    public static long CalculateEarliestTicks(this TimeProvider provider, int daysToAdd)
    {
        return GetTicks(provider.GetUtcNow().Date, daysToAdd);
    }
    
    public static long CalculateLatestTicks(this TimeProvider provider)
    {
        return GetTicks(provider.GetUtcNow().DateTime, 1);
    }

    private static long GetTicks(DateTime currentDate, int daysToAdd)
    {
        return DateTime.MaxValue.Ticks - currentDate.AddDays(daysToAdd).Ticks;
    }
}