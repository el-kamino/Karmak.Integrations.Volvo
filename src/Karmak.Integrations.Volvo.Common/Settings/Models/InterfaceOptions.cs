namespace Karmak.Integrations.Volvo.Common.Settings.Models;

public class InterfaceOptions
{
    public string PaCode { get; set; }

    public string FranchiseCode { get; set; }

    public string SalesZoneCode { get; set; }

    public bool AllowOasisRetrieval { get; set; }

    public bool? ReactEnabled { get; set; }

    public bool? WarrantyEnabled { get; set; }

    public int[] SubletPartsCharges { get; set; }

    public int[] SubletLaborCharges { get; set; }

    public int[] WarrantyDeductibleCharges { get; set; }

    public int[] EspDeductibleCharges { get; set; }

    public int[] ShippingCharges { get; set; }

    public int[] BodyShopDepartments { get; set; }

    public int[] DealerPrepDepartments { get; set; }

    public int[] MobileServiceDepartments { get; set; }

    public int[] SatelliteServiceDepartments { get; set; }

    public int[] QuickLaneDepartments { get; set; }

    public int[] ServiceDepartments { get; set; }

    public string[] AwaCustomers { get; set; }

    public string[] InternalPolicyCustomers { get; set; }

    public string[] WarrantyCustomers { get; set; }

    public string[] EspCustomers { get; set; }

    public IDictionary<string, string> OemUserMappings { get; set; }

    public string[] ECommercePartsOrderSource { get; set; }

    public string[] WalkInPartsOrderSource { get; set; }

    public string[] WarrantyEmailAddresses { get; set; }

    public string[] ReactEmailAddresses { get; set; }

    public InterfaceOptions()
    {
        SubletLaborCharges = Array.Empty<int>();
        SubletPartsCharges = Array.Empty<int>();
        WarrantyDeductibleCharges = Array.Empty<int>();
        EspDeductibleCharges = Array.Empty<int>();
        ShippingCharges = Array.Empty<int>();
        BodyShopDepartments = Array.Empty<int>();
        DealerPrepDepartments = Array.Empty<int>();
        MobileServiceDepartments = Array.Empty<int>();
        QuickLaneDepartments = Array.Empty<int>();
        ServiceDepartments = Array.Empty<int>();
        AwaCustomers = Array.Empty<string>();
        InternalPolicyCustomers = Array.Empty<string>();
        WarrantyCustomers = Array.Empty<string>();
        EspCustomers = Array.Empty<string>();
        SatelliteServiceDepartments = Array.Empty<int>();
        OemUserMappings = new Dictionary<string, string>();
        ECommercePartsOrderSource = Array.Empty<string>();
        WalkInPartsOrderSource = Array.Empty<string>();
        WarrantyEmailAddresses = Array.Empty<string>();
        ReactEmailAddresses = Array.Empty<string>();
    }
}
