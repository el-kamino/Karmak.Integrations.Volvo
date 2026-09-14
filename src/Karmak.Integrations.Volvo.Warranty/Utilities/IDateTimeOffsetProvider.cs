using System;

namespace Karmak.Integrations.Volvo.Warranty.Utilities
{
    public interface IDateTimeOffsetProvider
    {
        DateTimeOffset Now { get; }
        DateTimeOffset UtcNow { get; }
    }
}
