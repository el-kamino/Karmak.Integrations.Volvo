using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.Oasis.Models.Xml
{
    public class SymptomCodeItem
    {
        [XmlAttribute("num", DataType = "token")]
        public string OrderNumber { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}
