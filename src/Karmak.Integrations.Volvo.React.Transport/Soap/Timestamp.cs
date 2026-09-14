using System;
using System.Xml.Schema;
using System.Xml.Serialization;
using Karmak.Integrations.Volvo.React.Transport.Utils;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    [XmlRoot(Namespace = XmlNamespaces.WsSecurityUtilityUrl)]
    public class Timestamp {
        [XmlAttribute(Form = XmlSchemaForm.Qualified)]
        public string Id { get; set; }

        [XmlElement]
        public string Created { get; set; }

        [XmlElement]
        public string Expires { get; set; }

        public Timestamp() {
            Id = Guid.NewGuid().ToString();
            Created = DateTime.UtcNow.InTimestampFormat();
            Expires = DateTime.UtcNow.AddMinutes(5).InTimestampFormat();
        }
    }
}