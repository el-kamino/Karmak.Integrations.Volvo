using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Transport.Utils;
using Karmak.Integrations.Volvo.Warranty.Configuration;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Karmak.Integrations.Volvo.Warranty.StandardCodes
{
    public class OWSStandardCodesClient : IStandardCodesClient
    {
        private const string CustomerConcernCodesType = "CustomerConcernCode";
        private const string GetCustomerConcernCodes = "GetCustomerConcernCodes";
        private const string CustomerConcernServiceMessage = "CUSTOMER CONCERN";
        private const string ConditionCodeType = "ConditionCode";
        private const string GetConditionCodes = "GetConditionCodes";
        private const string ConditionServiceMessage = "CONDITION";
        private const string DamageCodesType = "DamageCode";
        private const string GetDamageCodes = "GetDamageCodes";
        private const string DamageServiceMessage = "DAMAGE";
        private const string VolvoStarServicesUrnV1 = "urn:volvo/star/services/v1/";

        private readonly IVolvoClient _volvoClient;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<OWSStandardCodesClient> _logger;

        private readonly StandardCodesClientOptions _options;

        public OWSStandardCodesClient(
            IVolvoClient volvoClient,
            IDateTimeProvider dateTimeProvider,
            ILogger<OWSStandardCodesClient> logger,
            IOptions<StandardCodesClientOptions> options)
        {
            _volvoClient = volvoClient;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;

            _options = options.Value;
        }

        public async Task<IEnumerable<StandardCode>> FetchCodesAsync(StandardCodesSettings standardCodesSettings)
        {
            try
            {
                _logger.LogInformation("Attempting to fetch Standard Codes from OWS.");
                var aggregateResponse = new OWSStandardCodeResponse
                {
                    ConditionCodes = await FetchStandardCodesAsync(GetConditionCodes, ConditionServiceMessage, standardCodesSettings),
                    DamageCodes = await FetchStandardCodesAsync(GetDamageCodes, DamageServiceMessage, standardCodesSettings),
                    CustomerConcernCodes = StripCategoricalCustomerConcernCodes(
                        await FetchStandardCodesAsync(GetCustomerConcernCodes, CustomerConcernServiceMessage, standardCodesSettings)
                    )
                };

                return ToStandardCodesList(aggregateResponse);
            }
            catch (OWSStandardCodesException e)
            {
                _logger.LogError(e, e.Message);

                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);

                throw new OWSStandardCodesException(e.Message, "Unknown");
            }
        }

        // categorical concern codes contain many code values that are references to other codes
        // ADAM systems filters out any line item that has code values, we will do the same here
        private List<StandardCodesLineItemsType> StripCategoricalCustomerConcernCodes(List<StandardCodesLineItemsType> codes) =>
            codes.Where(code => code.Values == null || code.Values.Length == 0).ToList();

        private async Task<List<StandardCodesLineItemsType>> FetchStandardCodesAsync(string toAction, string serviceMessage, StandardCodesSettings standardCodesSettings)
        {
            var envelope = BuildSoapEnvelope(toAction, serviceMessage, standardCodesSettings);
            var request = new VolvoSoapRequest(envelope);
            var response = await _volvoClient.OAuthSendSoapAsync(request, new Dictionary<string, string>());

            switch (response)
            {
                case SoapResult.Success successResponse: return ExtractCodesFromResponse(successResponse.Response.OuterXml);
                case SoapResult.Failure failureResponse:
                    ExtractSoapFault(failureResponse.Response.OuterXml);

                    break;
                case SoapResult.InvalidSignature invalidResponse: throw new OWSStandardCodesException(invalidResponse.Response.OuterXml, "Invalid-Signature");
                case SoapResult.Error errorResponse: throw new OWSStandardCodesException(errorResponse.Response, "Error");
                default: return new List<StandardCodesLineItemsType>();
            }

            throw new OWSStandardCodesException("Unexpected SOAP result", "Unknown");
        }

        private SoapEnvelope BuildSoapEnvelope(string toAction, string serviceMessage, StandardCodesSettings standardCodesSettings)
        {
            return new SoapEnvelope
            {
                Header = new Header
                {
                    To = VolvoStarServicesUrnV1 + toAction,
                    Action = "http://www.starstandards.org/webservices/2005/10/transport/operations/ProcessMessage",
                    MessageID = Guid.NewGuid().ToString(),
                    Security = new Security
                    {
                        Timestamp = new Timestamp
                        {
                            Id = Guid.NewGuid().ToString(),
                            Created = _dateTimeProvider.Now().InTimestampFormat(),
                            Expires = _dateTimeProvider.Now().AddMinutes(15).InTimestampFormat()
                        }
                    },
                    PayloadManifest = new React.Transport.Soap.PayloadManifest
                    {
                        Element = "GetStandardCodes",
                        Version = "5.2.4",
                        NamespaceUri = XmlNamespaces.StarUrl
                    },
                    VolvoDealerIdentity = new VolvoDealerIdentity
                    {
                        SiteCode = standardCodesSettings.CountryCode + standardCodesSettings.PACode
                    }
                },
                Body = new Body
                {
                    Id = Guid.NewGuid().ToString(),
                    Value = ToXml.Element(new ProcessMessage
                    {
                        Payload = new Payload
                        {
                            Content = new Content
                            {
                                Value = ToXml.Element(new GetStandardCodesType
                                {
                                    languageCode = GetLanguageCodeEnum(standardCodesSettings.LanguageCode),
                                    releaseID = "5.2.4",
                                    systemEnvironmentCode = _options.VolvoEnvironment,
                                    ApplicationArea = new ApplicationAreaType
                                    {
                                        CreationDateTime = _dateTimeProvider.Now(),
                                        Sender = new SenderType
                                        {
                                            TaskID = new IdentifierType
                                            {
                                                Value = "GetStandardCodes"
                                            },
                                            SenderNameCode = new CodeType
                                            {
                                                Value = _options.SenderNameCode
                                            },
                                            CreatorNameCode = new TextType
                                            {
                                                Value = _options.CreatorNameCode
                                            },
                                            DealerNumberID = new IdentifierType
                                            {
                                                Value = $"{standardCodesSettings.PACode}!"
                                            },
                                            DealerCountryCode = GetCountryCountryCodeEnum(standardCodesSettings.CountryCode),
                                            DealerCountryCodeSpecified = true,
                                            LanguageCode = standardCodesSettings.LanguageCode
                                        },
                                        Destination = new DestinationType
                                        {
                                            DestinationNameCode = new CodeType
                                            {
                                                Value = "FM"
                                            },
                                            DestinationSoftwareCode = new TextType
                                            {
                                                Value = "OWS"
                                            },
                                            ServiceMessageID = new IdentifierType
                                            {
                                                Value = $"OWS Standard Codes {serviceMessage} CODE"
                                            }
                                        }
                                    },
                                    GetStandardCodesDataArea = new GetStandardCodesDataAreaType
                                    {
                                        Get = new GetType
                                        {
                                            Expression = new[] {
                                                new ExpressionType {
                                                    expressionLanguage = "token",
                                                    Value = "token"
                                                }
                                            }
                                        },
                                        StandardCodes = new[] {
                                            new StandardCodesType {
                                                StandardCodesHeader = new[] {
                                                    new StandardCodesHeaderType {
                                                        LanguageCode = standardCodesSettings.LanguageCode,
                                                        DocumentIdentificationGroup = new DocumentIdentificationGroupType {
                                                            DocumentIdentification = new DocumentIdentificationType {
                                                                DocumentID = new IdentifierType {
                                                                    Value = "Default"
                                                                }
                                                            }
                                                        },
                                                        TableName = new TextType {
                                                            Value = $"OWS Standard Codes {serviceMessage} CODE"
                                                        }
                                                    }
                                                },
                                                StandardCodesLineItems = new[] {
                                                    new StandardCodesLineItemsType()
                                                }
                                            }
                                        }
                                    }
                                })
                            }
                        }
                    })
                }
            };
        }

        private static LanguageEnumeratedType GetLanguageCodeEnum(string languageCode)
        {
            var code = languageCode?.ToLower();

            switch (code)
            {
                case "fr-ca": return LanguageEnumeratedType.frCA;
                case "es-mx": return LanguageEnumeratedType.esMX;
                case "en-us": return LanguageEnumeratedType.enUS;
                default: return LanguageEnumeratedType.enUS;
            }
        }

        private static CountryEnumeratedType GetCountryCountryCodeEnum(string countryCode)
        {
            var code = countryCode?.ToLower();

            switch (code)
            {
                case "usa": return CountryEnumeratedType.US;
                case "can": return CountryEnumeratedType.CA;
                case "mex": return CountryEnumeratedType.MX;
                default: return CountryEnumeratedType.US;
            }
        }

        private static List<StandardCodesLineItemsType> ExtractCodesFromResponse(string soapResponseXml)
        {
            const string PayloadXPath = "/tran:payload";
            const string ContentXPath = "/tran:content";
            const string ShowStandardCodesXpath = "/ns:ShowStandardCodes";

            var soapEnvelope = Deserialize<SoapEnvelope>(soapResponseXml);
            var nsManager = XmlNamespaces.GetManager();

            if (soapEnvelope.Body.Value.Name.EndsWith("Fault"))
            {
                ExtractSoapFault(soapEnvelope.Body.Value, nsManager);
            }

            var standardCodesXml = soapEnvelope.Body.Value.SelectSingleNode($"{PayloadXPath}{ContentXPath}{ShowStandardCodesXpath}", nsManager) ?? soapEnvelope.Body.Value.SelectSingleNode($"{PayloadXPath}{ShowStandardCodesXpath}", nsManager);

            if (standardCodesXml == null)
            {
                throw new OWSStandardCodesException("Unrecognized response returned", "ExtractCodesError");
            }

            var standardCodes = Deserialize<ShowStandardCodesType>(standardCodesXml.OuterXml);

            return new List<StandardCodesLineItemsType>(standardCodes.ShowStandardCodesDataArea.StandardCodes.SelectMany(standardCodesType =>
            {
                return standardCodesType.StandardCodesLineItems;
            }));
        }

        private static void ExtractSoapFault(string envelopeXml)
        {
            var soapEnvelope = Deserialize<SoapEnvelope>(envelopeXml);
            var nsManager = XmlNamespaces.GetManager();
            ExtractSoapFault(soapEnvelope.Body.Value, nsManager);
        }

        private static void ExtractSoapFault(XmlElement body, XmlNamespaceManager nsManager)
        {
            const string SoapFaultCodeXPath = "/faultcode";
            const string SoapFaultStringXPath = "/faultstring";
            const string SoapFaultDetailsXPath = "/detail/ErrorMessage";

            var code = body.SelectSingleNode(SoapFaultCodeXPath, nsManager)?.InnerText;
            var message = body.SelectSingleNode(SoapFaultStringXPath, nsManager)?.InnerText;
            var detailsNode = body.SelectSingleNode(SoapFaultDetailsXPath, nsManager);
            var details = detailsNode != null
                ? $", Details: {detailsNode.InnerText}"
                : "";

            var errorMessage = $"Fault Code: {code}, Message: {message}, {details}";

            throw new OWSStandardCodesException(errorMessage, "SoapFault");
        }

        private static T Deserialize<T>(string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var reader = new StringReader(xmlString);

            return (T)xmlSerializer.Deserialize(reader);
        }

        private static List<StandardCode> ToStandardCodesList(OWSStandardCodeResponse owsStandardCodeResponse)
        {
            var standardCodes = owsStandardCodeResponse.ConditionCodes.Select(lineItem => new StandardCode
            {
                CodeType = ConditionCodeType,
                Code = lineItem.Code.Value,
                Description = GetDescription(lineItem)
            })
                .ToList();

            standardCodes.AddRange(owsStandardCodeResponse.DamageCodes.Select(lineItem => new StandardCode
            {
                CodeType = DamageCodesType,
                Code = lineItem.Code.Value,
                Description = GetDescription(lineItem)
            }));

            standardCodes.AddRange(owsStandardCodeResponse.CustomerConcernCodes.Select(lineItem => new StandardCode
            {
                CodeType = CustomerConcernCodesType,
                Code = lineItem.Code.Value,
                Description = GetDescription(lineItem)
            }));

            return standardCodes;
        }

        private static string GetDescription(StandardCodesLineItemsType lineItem)
        {
            var description = string.Join(", ", lineItem.CodeDescription.Select(cd => cd.Value));

            if (lineItem.Values == null || !lineItem.Values.Any())
            {
                return description;
            }

            var supplemental = lineItem.Values.Where(v => v.CodeSupplementalDescription != null).Select(v => string.Join(", ", v.CodeSupplementalDescription.Select(d => d.Value))).ToList();

            if (!supplemental.Any())
            {
                return description;
            }

            var additionalDesc = string.Join(", ", supplemental);

            return $"{description} ({additionalDesc})";
        }
    }
}
