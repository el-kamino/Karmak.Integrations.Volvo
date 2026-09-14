using Karmak.Integrations.Volvo.React.Contracts.Common;

namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class PhoneTypeExtensions
    {
        public static string ToChannelCode(this PhoneType type)
        {
            return type.ToString().ToLower();
        }
    }
}