using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.Oasis.Models.Xml
{
    public class SymptomCodeList
    {
        [XmlElement("Scode")]
        public List<SymptomCodeItem> Codes { get; set; } = new List<SymptomCodeItem>();
    }
}
