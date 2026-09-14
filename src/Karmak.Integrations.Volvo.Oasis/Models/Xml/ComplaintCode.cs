using System.Xml.Serialization;

namespace Karmak.Integrations.Volvo.Oasis.Models.Xml
{
    public class ComplaintCode
    {
        [XmlElement("ComplaintCode", IsNullable = true)]
        public string Code { get; set; }

        [XmlElement("ComplaintCodeType", IsNullable = true)]
        public string CodeType { get; set; }

        [XmlElement("ComplaintDescription", IsNullable = true)]
        public string Description { get; set; }
    }
}
