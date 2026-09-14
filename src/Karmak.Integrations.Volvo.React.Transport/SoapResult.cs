using System;
using System.Net;
using System.Xml;

namespace Karmak.Integrations.Volvo.React.Transport
{
    public abstract class SoapResult {
        public class Success : SoapResult {
            public XmlDocument Response { get; }

            public Success(XmlDocument response) {
                Response = response;
            }
        }

        public class Failure : SoapResult {
            public HttpStatusCode HttpStatus { get; }
            public XmlDocument Response { get; }

            public Failure(HttpStatusCode httpStatus, XmlDocument response) {
                HttpStatus = httpStatus;
                Response = response;
            }
        }

        public class InvalidSignature : SoapResult {
            public XmlDocument Response { get; }

            public InvalidSignature(XmlDocument response) {
                Response = response;
            }
        }

        public class Error : SoapResult {
            public Exception Exception { get; set; }
            public string Response { get; set; }
        }
    }
}
