namespace Karmak.Integrations.Volvo.Oasis.Models
{
    public class OasisRequestRest
    {
        public bool IncludeFsaData { get; set; }

        public string KarmakAccountNumber { get; set; }

        public bool IncludeBroadcastMessages { get; set; }

        public bool IncludeVehicleInfo { get; set; }

        public bool IncludeWarrantyData { get; set; }

        public string Vin { get; set; }

        public List<SymptomCodeRequestRest> SymptomCodes { get; set; }

        public int? MileageIn { get; set; }

        public ComplaintCodeRest ComplaintCode { get; set; }
    }
}
