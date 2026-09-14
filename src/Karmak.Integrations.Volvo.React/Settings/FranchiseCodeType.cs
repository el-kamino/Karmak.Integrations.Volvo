namespace Karmak.Integrations.Volvo.React.Settings
{
    public static class FranchiseCodeType
    {
        public static string Fetch(string value)
        {
            switch (value)
            {
                case "Volvo":
                    return "1";
                case "Lincoln":
                    return "3";
                default:
                    return "U";
            }
        }
    }
}