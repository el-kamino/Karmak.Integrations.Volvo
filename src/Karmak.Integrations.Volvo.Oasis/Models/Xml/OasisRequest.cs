using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.Oasis.Models.Xml
{
    [XmlRoot("GRequest")]
    public class OasisRequest
    {
        [XmlElement("ApplicationID")]
        public string ApplicationId { get; set; }

        public string ApplicationType { get; set; }

        [XmlElement("IMSRegion")]
        public string ImsRegion { get; set; }

        [XmlIgnore]
        public bool XmlOnly
        {
            get => XmlOnlyTag == XmlConstants.BooleanYes;
            set => XmlOnlyTag = value ? XmlConstants.BooleanYes : XmlConstants.BooleanNo;
        }

        [XmlElement("XMLOnly")]
        public string XmlOnlyTag { get; set; }


        [XmlIgnore]
        public bool FsaOnly
        {
            get => FsaOnlyTag == XmlConstants.BooleanYes;
            set => FsaOnlyTag = value ? XmlConstants.BooleanYes : XmlConstants.BooleanNo;
        }

        [XmlElement("FSAOnly")]
        public string FsaOnlyTag { get; set; }

        [XmlElement("GSA")]
        public string Gsa { get; set; }

        [XmlElement("UserID")]
        public string UserId { get; set; }

        [XmlElement("PACode")]
        public string PaCode { get; set; }

        [XmlElement("SubDealerID", IsNullable = true)]
        public string SubDealerId { get; set; }

        public string LanguageCode { get; set; }

        [XmlIgnore]
        public bool Bcm
        {
            get => BcmTag == XmlConstants.BooleanYes;
            set => BcmTag = value ? XmlConstants.BooleanYes : XmlConstants.BooleanNo;
        }

        [XmlElement("BCM")]
        public string BcmTag { get; set; }

        [XmlIgnore]
        public bool VehicleInfo
        {
            get => VehicleInfoTag == XmlConstants.BooleanYes;
            set => VehicleInfoTag = value ? XmlConstants.BooleanYes : XmlConstants.BooleanNo;
        }

        [XmlElement("VehicleInfo")]
        public string VehicleInfoTag { get; set; }

        [XmlIgnore]
        public bool Warranty
        {
            get => WarrantyTag == XmlConstants.BooleanYes;
            set => WarrantyTag = value ? XmlConstants.BooleanYes : XmlConstants.BooleanNo;
        }

        [XmlElement("Warranty")]
        public string WarrantyTag { get; set; }

        [XmlElement("VIN", IsNullable = true)]
        public string Vin { get; set; }

        public SymptomCodeList SymptomCode { get; set; }

        [XmlElement(IsNullable = true)]
        public string MileageIn { get; set; }

        public ComplaintCode CodesAndComments { get; set; }

        public string ToXml()
        {
            return SerializationUtils.Serialize(this);
        }

        public static OasisRequest Build(string xml)
        {
            return SerializationUtils.Deserialize<OasisRequest>(xml);
        }
    }
}
