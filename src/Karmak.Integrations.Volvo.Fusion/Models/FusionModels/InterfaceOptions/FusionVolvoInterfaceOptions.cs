namespace Karmak.Integrations.Volvo.Fusion.Models.FusionModels.InterfaceOptions;

public class FusionVolvoInterfaceOptions
{
    public string PaCode { get; set; }
    public string FranchiseCode { get; set; }
    public string SalesZoneCode { get; set; }
    public bool? ReactEnabled { get; set; }
    public bool? WarrantyEnabled { get; set; }
    public bool AllowOasisRetrieval { get; set; }
    public int[] SubletPartsCharges { get; set; }
    public int[] SubletLaborCharges { get; set; }
    public int[] WarrantyDeductibleCharges { get; set; }
    public int[] EspDeductibleCharges { get; set; }
    public int[] BodyShopDepartments { get; set; }
    public int[] DealerPrepDepartments { get; set; }
    public int[] MobileServiceDepartments { get; set; }
    public int[] SatelliteServiceDepartments { get; set; }
    public int[] QuickLaneDepartments { get; set; }
    public int[] ServiceDepartments { get; set; }
    public int[] ShippingCharges { get; set; }
    public string[] AwaCustomers { get; set; }
    public string[] InternalPolicyCustomers { get; set; }
    public string[] WarrantyCustomers { get; set; }
    public string[] EspCustomers { get; set; }
    public OEMUserCrossReference[] OEMUserCrossReferences { get; set; }
    public string[] ECommercePartsOrderSource { get; set; }
    public string[] WalkInPartsOrderSource { get; set; }
    public string[] WarrantyEmailAddresses { get; set; }
    public string[] ReactEmailAddresses { get; set; }
}