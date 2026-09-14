using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Utils;

namespace Karmak.Integrations.Volvo.React.RepairOrders
{
    public static class ServiceDepartments
    {
        public const string BODY_SHOP_DEPARTMENT = "B";
        public const string DEALER_PREP_DEPARTMENT = "D";
        public const string MOBILE_SERVICE_DEPARTMENT = "M";
        public const string SATELLITE_SERVICE_DEPARTMENT = "O";
        public const string QUICK_LANE_DEPARTMENT = "Q";
        public const string SERVICE_DEPARTMENT = "S";
        public const string UNKNOWN_DEPARTMENT = "U";

        public static string GetDepartmentCode(int? departmentId, InterfaceOptions options)
        {
            if (!departmentId.HasValue)
            {
                return UNKNOWN_DEPARTMENT;
            }
            if (options.BodyShopDepartments.NotNullAndAny(dept => dept.Equals(departmentId)))
            {
                return BODY_SHOP_DEPARTMENT;
            }
            if (options.DealerPrepDepartments.NotNullAndAny(dept => dept.Equals(departmentId)))
            {
                return DEALER_PREP_DEPARTMENT;
            }
            if (options.MobileServiceDepartments.NotNullAndAny(dept => dept.Equals(departmentId)))
            {
                return MOBILE_SERVICE_DEPARTMENT;
            }
            if (options.SatelliteServiceDepartments.NotNullAndAny(dept => dept.Equals(departmentId)))
            {
                return SATELLITE_SERVICE_DEPARTMENT;
            }
            if (options.QuickLaneDepartments.NotNullAndAny(dept => dept.Equals(departmentId)))
            {
                return QUICK_LANE_DEPARTMENT;
            }
            if (options.ServiceDepartments.NotNullAndAny(dept => dept.Equals(departmentId)))
            {
                return SERVICE_DEPARTMENT;
            }
            return UNKNOWN_DEPARTMENT;
        }
    }
}
