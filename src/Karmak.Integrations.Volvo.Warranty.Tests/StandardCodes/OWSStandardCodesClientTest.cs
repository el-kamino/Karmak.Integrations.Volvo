using System.Net;
using System.Xml;
using System.Xml.Serialization;
using Karmak.Integrations.Volvo.React.Transport;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Transport.Utils;
using Karmak.Integrations.Volvo.Warranty.Configuration;
using Karmak.Integrations.Volvo.Warranty.Contracts.Exceptions;
using Karmak.Integrations.Volvo.Warranty.Contracts.OWS.V0_5;
using Karmak.Integrations.Volvo.Warranty.StandardCodes;
using Karmak.Integrations.Volvo.Warranty.Tests.TestUtils;
using Karmak.Integrations.Volvo.Warranty.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Karmak.Integrations.Volvo.Warranty.Tests.StandardCodes
{
    public class OWSStandardCodesClientTest
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IVolvoClient _volvoClient;

        private readonly StandardCodesClientOptions _options;
        private readonly StandardCodesSettings _settings;
        private readonly IStandardCodesClient _client;

        public OWSStandardCodesClientTest()
        {
            _dateTimeProvider = Substitute.For<IDateTimeProvider>();
            _dateTimeProvider
                .Now()
                .Returns(new DateTime(2019, 1, 1, 0, 0, 0));

            _volvoClient = Substitute.For<IVolvoClient>();

            _options = new StandardCodesClientOptions
            {
                SenderNameCode = "KM",
                CreatorNameCode = "Karmak",
                VolvoEnvironment = "foobar"
            };

            _settings = new StandardCodesSettings
            {
                CountryCode = "MEX",
                LanguageCode = "es-MX",
                PACode = "K1234"
            };

            var optionsSub = Substitute.For<IOptions<StandardCodesClientOptions>>();
            optionsSub.Value.Returns(_options);

            _client = new OWSStandardCodesClient(
                _volvoClient,
                _dateTimeProvider,
                Substitute.For<ILogger<OWSStandardCodesClient>>(),
                optionsSub);
        }

        [Fact]
        public async Task WhenFetchingStandardCodes_It_CallsVolvoClientWithASpecificRequest()
        {
            var now = new DateTime(2019, 1, 1, 0, 0, 0);
            _dateTimeProvider.Now().Returns(now);

            var requests = new List<VolvoSoapRequest>();
            _volvoClient
                .OAuthSendSoapAsync(Arg.Any<VolvoSoapRequest>(), Arg.Any<IDictionary<string, string>>())
                .Returns(default(SoapResult))
                .AndDoes(callInfo => requests.Add(callInfo.ArgAt<VolvoSoapRequest>(0)));

            await _client.FetchCodesAsync(_settings);

            Assert.Equal(3, requests.Count);
            Assert.Contains(requests, request => request.Body.Header.To == "urn:volvo/star/services/v1/GetCustomerConcernCodes");
            Assert.Contains(requests, request => request.Body.Header.To == "urn:volvo/star/services/v1/GetDamageCodes");
            Assert.Contains(requests, request => request.Body.Header.To == "urn:volvo/star/services/v1/GetConditionCodes");
            Assert.All(requests, request => Assert.Equal("http://www.starstandards.org/webservices/2005/10/transport/operations/ProcessMessage", request.Body.Header.Action));
            Assert.All(requests, request => Assert.NotNull(request.Body.Header.MessageID));
            Assert.All(requests, request => Assert.Equal("GetStandardCodes", request.Body.Header.PayloadManifest.Element));
            Assert.All(requests, request => Assert.Equal("5.2.4", request.Body.Header.PayloadManifest.Version));
            Assert.All(requests, request => Assert.Equal("http://www.starstandard.org/STAR/5", request.Body.Header.PayloadManifest.NamespaceUri));
            Assert.All(requests, request => Assert.Equal("MEXK1234", request.Body.Header.VolvoDealerIdentity.SiteCode));
            Assert.All(requests, request => Assert.Equal(now.InTimestampFormat(), request.Body.Header.Security.Timestamp.Created));
            Assert.All(requests, request => Assert.Equal(now.AddMinutes(15).InTimestampFormat(), request.Body.Header.Security.Timestamp.Expires));
            Assert.All(requests, request => Assert.NotNull(request.Body.Header.Security.Timestamp.Id));
            var soapRequestPayloads = requests.Select(r => Deserialize<ProcessMessage>(r.Body.Body.Value.OuterXml)).ToList();
            var getStandardCodesPayloads = soapRequestPayloads.Select(s => Deserialize<GetStandardCodesType>(s.Payload.Content.Value.OuterXml)).ToList();
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal(LanguageEnumeratedType.esMX, getStandardCodesPayload.languageCode));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal("5.2.4", getStandardCodesPayload.releaseID));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal(_options.VolvoEnvironment, getStandardCodesPayload.systemEnvironmentCode));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal("GetStandardCodes", getStandardCodesPayload.ApplicationArea.Sender.TaskID.Value));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal(now, getStandardCodesPayload.ApplicationArea.CreationDateTime));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal(_options.CreatorNameCode, getStandardCodesPayload.ApplicationArea.Sender.CreatorNameCode.Value));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal(_options.SenderNameCode, getStandardCodesPayload.ApplicationArea.Sender.SenderNameCode.Value));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal($"{_settings.PACode}!", getStandardCodesPayload.ApplicationArea.Sender.DealerNumberID.Value));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal(CountryEnumeratedType.MX, getStandardCodesPayload.ApplicationArea.Sender.DealerCountryCode));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal(_settings.LanguageCode, getStandardCodesPayload.ApplicationArea.Sender.LanguageCode));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal("FM", getStandardCodesPayload.ApplicationArea.Destination.DestinationNameCode.Value));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal("OWS", getStandardCodesPayload.ApplicationArea.Destination.DestinationSoftwareCode.Value));
            Assert.Contains(getStandardCodesPayloads, getStandardCodesPayload => getStandardCodesPayload.ApplicationArea.Destination.ServiceMessageID.Value == "OWS Standard Codes CUSTOMER CONCERN CODE");
            Assert.Contains(getStandardCodesPayloads, getStandardCodesPayload => getStandardCodesPayload.ApplicationArea.Destination.ServiceMessageID.Value == "OWS Standard Codes DAMAGE CODE");
            Assert.Contains(getStandardCodesPayloads, getStandardCodesPayload => getStandardCodesPayload.ApplicationArea.Destination.ServiceMessageID.Value == "OWS Standard Codes CONDITION CODE");
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Single(getStandardCodesPayload.GetStandardCodesDataArea.Get.Expression));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal("token", getStandardCodesPayload.GetStandardCodesDataArea.Get.Expression.First().expressionLanguage));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal("token", getStandardCodesPayload.GetStandardCodesDataArea.Get.Expression.First().Value));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Single(getStandardCodesPayload.GetStandardCodesDataArea.StandardCodes));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Single(getStandardCodesPayload.GetStandardCodesDataArea.StandardCodes.First().StandardCodesHeader));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal(_settings.LanguageCode, getStandardCodesPayload.GetStandardCodesDataArea.StandardCodes.First().StandardCodesHeader.First().LanguageCode));
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Equal("Default", getStandardCodesPayload.GetStandardCodesDataArea.StandardCodes.First().StandardCodesHeader.First().DocumentIdentificationGroup.DocumentIdentification.DocumentID.Value));
            Assert.Contains(getStandardCodesPayloads, getStandardCodesPayload => getStandardCodesPayload.GetStandardCodesDataArea.StandardCodes.First().StandardCodesHeader.First().TableName.Value == "OWS Standard Codes CUSTOMER CONCERN CODE");
            Assert.Contains(getStandardCodesPayloads, getStandardCodesPayload => getStandardCodesPayload.GetStandardCodesDataArea.StandardCodes.First().StandardCodesHeader.First().TableName.Value == "OWS Standard Codes DAMAGE CODE");
            Assert.Contains(getStandardCodesPayloads, getStandardCodesPayload => getStandardCodesPayload.GetStandardCodesDataArea.StandardCodes.First().StandardCodesHeader.First().TableName.Value == "OWS Standard Codes CONDITION CODE");
            Assert.All(getStandardCodesPayloads, getStandardCodesPayload => Assert.Single(getStandardCodesPayload.GetStandardCodesDataArea.StandardCodes.First().StandardCodesLineItems));
        }

        [Fact]
        public async Task WhenFetchingStandardCodes_It_ReturnsAListOfStandardCode()
        {
            _volvoClient.OAuthSendSoapAsync(Arg.Any<VolvoSoapRequest>(), Arg.Any<IDictionary<string, string>>())
                .Returns(callInfo =>
                {
                    var request = callInfo.ArgAt<VolvoSoapRequest>(0);
                    var doc = new XmlDocument();
                    var toAddress = request.Body.Header.To;

                    if (toAddress.EndsWith("GetCustomerConcernCodes"))
                    {
                        doc.LoadXml(OWSSoapResponseFixtures.CustomerConcernCodes);
                    }
                    else if (toAddress.EndsWith("GetConditionCodes"))
                    {
                        doc.LoadXml(OWSSoapResponseFixtures.ConditionCodes);
                    }
                    else if (toAddress.EndsWith("GetDamageCodes"))
                    {
                        doc.LoadXml(OWSSoapResponseFixtures.DamageCodes);
                    }

                    return new SoapResult.Success(doc);
                });

            var result = await _client.FetchCodesAsync(_settings);

            Assert.NotNull(result);
            Assert.Contains(result, c => c.CodeType == "CustomerConcernCode");
            Assert.Contains(result, c => c.CodeType == "ConditionCode");
            Assert.Contains(result, c => c.CodeType == "DamageCode");
            Assert.True(result.All(c => !string.IsNullOrWhiteSpace(c.Code) && !string.IsNullOrWhiteSpace(c.Description) && !string.IsNullOrWhiteSpace(c.CodeType)));
        }

        [Fact]
        public async Task WhenFetchingCustomerConcernCodes_It_RemovesCategoricalHeaderCodes()
        {
            _volvoClient.OAuthSendSoapAsync(Arg.Any<VolvoSoapRequest>(), Arg.Any<IDictionary<string, string>>())
                .Returns(callInfo =>
                {
                    var request = callInfo.ArgAt<VolvoSoapRequest>(0);
                    var doc = new XmlDocument();
                    var toAddress = request.Body.Header.To;

                    if (toAddress.EndsWith("GetCustomerConcernCodes"))
                    {
                        doc.LoadXml(OWSSoapResponseFixtures.CustomerConcernCodes);
                    }
                    else if (toAddress.EndsWith("GetConditionCodes"))
                    {
                        doc.LoadXml(OWSSoapResponseFixtures.ConditionCodes);
                    }
                    else if (toAddress.EndsWith("GetDamageCodes"))
                    {
                        doc.LoadXml(OWSSoapResponseFixtures.DamageCodes);
                    }

                    return new SoapResult.Success(doc);
                });

            var result = await _client.FetchCodesAsync(_settings);

            Assert.NotNull(result);
            Assert.Contains(result, c => c.CodeType == "CustomerConcernCode");
            Assert.Contains(result, c => c.Code == "F04" && c.CodeType == "CustomerConcernCode"); // "THIN/NO PAINT (EXCLUDES TRIM/BUMPER)"
            Assert.DoesNotContain(result, c => c.Code == "^296" && c.CodeType == "CustomerConcernCode"); // "Body"
        }

        [Fact]
        public async Task WhenOWSReturnsUnexpectedXML_It_ThrowsAnException()
        {
            _volvoClient.OAuthSendSoapAsync(Arg.Any<VolvoSoapRequest>(), Arg.Any<IDictionary<string, string>>())
                .Returns(callInfo =>
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(OWSSoapResponseFixtures.UnexpectedOWSResponse);

                    return new SoapResult.Success(doc);
                });

            var exception = await Assert.ThrowsAsync<OWSStandardCodesException>(() => _client.FetchCodesAsync(_settings));
            Assert.Contains("ExtractCodesError", exception.Message);
        }

        [Fact]
        public async Task WhenVolvoClientThrowsAnException_It_ThrowsAnOWSStandardCodesException()
        {
            _volvoClient.OAuthSendSoapAsync(Arg.Any<VolvoSoapRequest>(), Arg.Any<IDictionary<string, string>>()).Throws(new Exception("Exception from VolvoClient"));

            var exception = await Assert.ThrowsAsync<OWSStandardCodesException>(() => _client.FetchCodesAsync(_settings));
            Assert.Contains("Unknown", exception.Message);
        }

        [Fact]
        public async Task WhenOWSReturnsASoapFault_It_ThrowsAnOWSStandardCodesException()
        {
            _volvoClient.OAuthSendSoapAsync(Arg.Any<VolvoSoapRequest>(), Arg.Any<IDictionary<string, string>>())
                .Returns(callInfo =>
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(OWSSoapResponseFixtures.SoapFault);

                    return new SoapResult.Success(doc);
                });

            var exception = await Assert.ThrowsAsync<OWSStandardCodesException>(() => _client.FetchCodesAsync(_settings));
            Assert.Contains("SoapFault", exception.Message);
        }

        [Fact]
        public async Task WhenOWSReturnsASoapFailure_It_ThrowsAnOWSStandardCodesException()
        {
            _volvoClient.OAuthSendSoapAsync(Arg.Any<VolvoSoapRequest>(), Arg.Any<IDictionary<string, string>>())
                .Returns(callInfo =>
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(OWSSoapResponseFixtures.SoapFault);

                    return new SoapResult.Failure(HttpStatusCode.InternalServerError, doc);
                });

            var exception = await Assert.ThrowsAsync<OWSStandardCodesException>(() => _client.FetchCodesAsync(_settings));
            Assert.Contains("SoapFault", exception.Message);
        }

        [Fact]
        public async Task WhenOWSReturnsAnInvalidSignature_It_ThrowsAnOWSStandardCodesException()
        {
            _volvoClient.OAuthSendSoapAsync(Arg.Any<VolvoSoapRequest>(), Arg.Any<IDictionary<string, string>>())
                .Returns(callInfo =>
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(OWSSoapResponseFixtures.SoapFault);

                    return new SoapResult.InvalidSignature(doc);
                });

            var exception = await Assert.ThrowsAsync<OWSStandardCodesException>(() => _client.FetchCodesAsync(_settings));
            Assert.Contains("Invalid-Signature", exception.Message);
        }

        [Fact]
        public async Task WhenOWSReturnsAnError_It_ThrowsAnOWSStandardCodesException()
        {
            _volvoClient.OAuthSendSoapAsync(Arg.Any<VolvoSoapRequest>(), Arg.Any<IDictionary<string, string>>())
                .Returns(callInfo => new SoapResult.Error());

            var exception = await Assert.ThrowsAsync<OWSStandardCodesException>(() => _client.FetchCodesAsync(_settings));
            Assert.Contains("Error", exception.Message);
        }

        private static T Deserialize<T>(string xmlString)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var reader = new StringReader(xmlString);

            return (T)xmlSerializer.Deserialize(reader);
        }
    }
}
