using Karmak.Integrations.Volvo.Common.Logging;
using Karmak.Integrations.Volvo.React.Utils;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.React.Transport.Soap
{
    internal static class SoapResponseExtensions
    {
        /// This phrase is to be used to generate email alerts in Azure
        public static string ALERTPHRASE = "Alert SOAP Fault:";

        /// <summary>
        ///     Parse SOAP response from Volvo and emit telemetry that will trigger email alerts as needed.  
        /// </summary>
        /// <remarks>
        ///     Some mappings of faultcode and faultstring combinations to error types were required to have some active alerting
        ///     for CustomerUpdate certification.  
        ///     Given the lack of usefulness of some other error raising due to inflexibility and difficulty discerning between noise and 
        ///     crucial issues, this mechanism is intentionally left open-ended so we can add cases to quiet noisy items, etc.
        ///     The combinations specifically required by the certification testing were:
        ///         faultcode: "soap:Client" faultstring: "Invalid Request" => "Element or Attribute Missing"
        ///         faultcode: "soap:Client" faultstring: "Internal Error" => "InvalidContentID"
        ///         faultcode: "soap:Client" faultstring: "Access Denied" => "AuthenticationError"
        ///         faultcode: "Server.Unspecified" faultstring: "Internal server error" => "ServerDown"
        ///         faultcode: "Server.Processing" faultstring: "Request cannot be processed" => "MaxLimitMessageSize"
        ///
        ///     Other combinations are handled with generic messaging, but can easily be excluded from email alerting by removing the ALERTPHRASE
        /// </remarks>
        public static SoapResult RaiseAlertOnSoapFault(this SoapResult result, Dictionary<string, string> _metadata, ILogger logger)
        {
            string message = null;
            IDictionary<string, string> dict = new Dictionary<string, string>();
            switch (result)
            {
                /// For success, do nothing
                case SoapResult.Success _: break;
                /// This is related to certificate-based decryption, so probably won't hit it using OAuth
                case SoapResult.InvalidSignature i: message = $"{ALERTPHRASE} Invalid Signature!\n\n{i.Response.ToXDocument()}"; break;
                /// This catches actual exceptions in a couple places, but otherwise is a catch-all
                case SoapResult.Error e: message = $"{ALERTPHRASE} Error!\n\n{e.Response.ToXDocument()}\n\n{e.Exception}"; break;
                /// This is for processing responses that are non-success. 
                case SoapResult.Failure f:
                    foreach (var element in f.Response.ToXDocument().Elements())
                    {
                        dict.Add(element.Name.LocalName, element.Value);
                        foreach (var descendant in element.Descendants())
                            dict.Add(descendant.Name.LocalName, descendant.Value);
                    }

                    string faultcode = dict.ContainsKey("faultcode") ? dict["faultcode"] : "";
                    string faultstring = dict.ContainsKey("faultstring") ? dict["faultstring"] : "";

                    switch (faultcode.Trim())
                    {
                        case "soap:Client":
                            switch (faultstring.Trim())
                            {
                                case "Invalid Request": message = $"Element or Attribute Missing"; break;
                                case "Internal Error": message = $"InvalidContentID"; break;
                                case "Access Denied": message = $"AuthenticationError"; break;
                                default: message = $"Unrecognized faultstring \"{faultstring}\" WTF?"; break;
                            }
                            break;
                        case "Server.Unspecified":
                            switch (faultstring.Trim())
                            {
                                case "Internal server error": message = $"ServerDown"; break;
                                default: message = $"Unrecognized faultstring \"{faultstring}\" WTF?"; break;
                            }
                            break;
                        case "Server.Processing":
                            switch (faultstring.Trim())
                            {
                                case "Request cannot be processed": message = $"MaxLimitMessageSize"; break;
                                default: message = $"Unrecognized faultstring \"{faultstring}\" WTF?"; break;
                            }
                            break;
                        default:
                            message = $"Unrecognized faultcode \"{faultcode}\" WTF?";
                            break;
                    }
                    if (message != null)
                        message = $"{ALERTPHRASE} {message}\n\n{f.Response.ToXDocument()}";
                    break;
                default:
                    break;
            }

            if (message != null)
                logger.LogInformationWithMetadata(message, _metadata);

            return result;
        }

        public static void ThrowIfNotSuccessful(this SoapResult result)
        {
            switch (result)
            {
                case SoapResult.InvalidSignature _:
                    throw new VolvoIntegrationServiceException();
                case SoapResult.Error e:
                    throw new VolvoIntegrationServiceException(IsTransient(e));
                case SoapResult.Failure f:
                    throw new VolvoIntegrationServiceException(IsTransient(f));
                case SoapResult.Success _:
                default:
                    break;
            }

            bool IsTransient(SoapResult fault)
            {
                return true;
            } //NOSONAR for the time being all soap faults are assumed to be transient (forcing retry)
        }
    }
}
