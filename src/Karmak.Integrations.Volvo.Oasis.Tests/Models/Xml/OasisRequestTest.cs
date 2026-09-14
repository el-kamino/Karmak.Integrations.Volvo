using System.Xml.Linq;
using Karmak.Integrations.Volvo.Oasis.Models.Xml;

namespace Karmak.Integrations.Volvo.Oasis.Tests.Models.Xml
{
    public class OasisRequestTest
    {
        [Fact]
        public void RequestSerializesToExpectedXmlWhenAllFieldsPopulated()
        {
            var request = BuildFullyPopulatedRequest();

            var requestXmlString = request.ToXml();
            var expectedXml = XElement.Parse(ExpectedXmlString).ToString();

            Assert.Equal(expectedXml, requestXmlString);
        }

        [Fact]
        public void RequestDeserializeFromXmlWithAllFieldsPopulatedAsExpected()
        {
            var expectedRequest = BuildFullyPopulatedRequest();
            var actualRequest = OasisRequest.Build(ExpectedXmlString);

            Assert.Equal(expectedRequest.ApplicationId, actualRequest.ApplicationId);
            Assert.Equal(expectedRequest.ApplicationType, actualRequest.ApplicationType);
            Assert.Equal(expectedRequest.ImsRegion, actualRequest.ImsRegion);
            Assert.Equal(expectedRequest.XmlOnly, actualRequest.XmlOnly);
            Assert.Equal(expectedRequest.FsaOnly, actualRequest.FsaOnly);
            Assert.Equal(expectedRequest.Gsa, actualRequest.Gsa);
            Assert.Equal(expectedRequest.UserId, actualRequest.UserId);
            Assert.Equal(expectedRequest.PaCode, actualRequest.PaCode);
            Assert.Equal(expectedRequest.SubDealerId, actualRequest.SubDealerId);
            Assert.Equal(expectedRequest.LanguageCode, actualRequest.LanguageCode);
            Assert.Equal(expectedRequest.Bcm, actualRequest.Bcm);
            Assert.Equal(expectedRequest.VehicleInfo, actualRequest.VehicleInfo);
            Assert.Equal(expectedRequest.Warranty, actualRequest.Warranty);
            Assert.Equal(expectedRequest.Vin, actualRequest.Vin);
            Assert.Equal(expectedRequest.MileageIn, actualRequest.MileageIn);
            Assert.Equal(expectedRequest.CodesAndComments.Code, actualRequest.CodesAndComments.Code);
            Assert.Equal(expectedRequest.CodesAndComments.CodeType, actualRequest.CodesAndComments.CodeType);
            Assert.Equal(expectedRequest.CodesAndComments.Description, actualRequest.CodesAndComments.Description);
            Assert.Contains(actualRequest.SymptomCode.Codes, c => c.OrderNumber == "1" && c.Value == "CODE1");
            Assert.Contains(actualRequest.SymptomCode.Codes, c => c.OrderNumber == "2" && c.Value == "CODE2");
        }

        [Fact]
        public void CanBuildExpectedBroadcastMessageRequest()
        {
            var request = new OasisRequest
            {
                ApplicationId = "ADM",
                ApplicationType = "GOASIS",
                ImsRegion = "TEST",
                XmlOnly = true,
                FsaOnly = false,
                Gsa = "USA",
                UserId = "j-doe12",
                PaCode = "DSP01",
                LanguageCode = "EN",
                Bcm = true,
                VehicleInfo = false,
                Warranty = false,
                SymptomCode = new SymptomCodeList()
            };
            request.SymptomCode.Codes.Add(new SymptomCodeItem { OrderNumber = "1", Value = "805000" });

            var requestXmlString = request.ToXml();
            var expectedXml = XElement.Parse(ADAMBroadcastMessageRequest).ToString();

            Assert.Equal(expectedXml, requestXmlString);
        }

        [Fact]
        public void CanBuildExpectedVinRequest()
        {
            var request = new OasisRequest
            {
                ApplicationId = "ADM",
                ApplicationType = "GOASIS",
                ImsRegion = "TEST",
                XmlOnly = true,
                FsaOnly = false,
                Gsa = "USA",
                UserId = "j-doe12",
                PaCode = "DSP01",
                LanguageCode = "EN",
                Bcm = false,
                VehicleInfo = true,
                Warranty = true,
                MileageIn = "48270",
                Vin = "1FMCU9J95DUA04123",
                SymptomCode = new SymptomCodeList(),
                CodesAndComments = new ComplaintCode
                {
                    Code = "A16",
                    Description = "CD PLAYER TROUBLES"
                }
            };

            request.SymptomCode.Codes.Add(new SymptomCodeItem { OrderNumber = "1", Value = "102000" });
            request.SymptomCode.Codes.Add(new SymptomCodeItem { OrderNumber = "2", Value = "100000" });
            request.SymptomCode.Codes.Add(new SymptomCodeItem { OrderNumber = "3", Value = "200000" });
            request.SymptomCode.Codes.Add(new SymptomCodeItem { OrderNumber = "4", Value = "203200" });
            request.SymptomCode.Codes.Add(new SymptomCodeItem { OrderNumber = "5", Value = "207000" });

            var requestXmlString = request.ToXml();
            var expectedXml = XElement.Parse(ADAMVinRequest).ToString();

            Assert.Equal(expectedXml, requestXmlString);
        }

        private static OasisRequest BuildFullyPopulatedRequest()
        {
            var request = new OasisRequest
            {
                ApplicationId = "Test.App",
                ApplicationType = "APP1",
                ImsRegion = "MIDWEST",
                XmlOnly = true,
                FsaOnly = false,
                Gsa = "USA",
                UserId = "v-hart1",
                PaCode = "1234",
                SubDealerId = "Sub1",
                LanguageCode = "EN",
                Bcm = true,
                VehicleInfo = true,
                Warranty = true,
                Vin = "123456789ABCDEFGH",
                MileageIn = "123456",
                SymptomCode = new SymptomCodeList(),
                CodesAndComments = new ComplaintCode
                {
                    Code = "COMPLAINT1",
                    CodeType = "TEST",
                    Description = "This is a test"
                }
            };

            request.SymptomCode.Codes.Add(new SymptomCodeItem { OrderNumber = "1", Value = "CODE1" });
            request.SymptomCode.Codes.Add(new SymptomCodeItem { OrderNumber = "2", Value = "CODE2" });
            return request;
        }

        private const string ExpectedXmlString =
            @"<GRequest>
				<ApplicationID>Test.App</ApplicationID>
				<ApplicationType>APP1</ApplicationType>
				<IMSRegion>MIDWEST</IMSRegion>
				<XMLOnly>Y</XMLOnly>
				<FSAOnly>N</FSAOnly>
				<GSA>USA</GSA>
				<UserID>v-hart1</UserID>
				<PACode>1234</PACode>
				<SubDealerID>Sub1</SubDealerID>
				<LanguageCode>EN</LanguageCode>
				<BCM>Y</BCM>
				<VehicleInfo>Y</VehicleInfo>
				<Warranty>Y</Warranty>
				<VIN>123456789ABCDEFGH</VIN>
				<SymptomCode>
				<Scode num=" + "\"1\"" + @">CODE1</Scode>
			        <Scode num=" + "\"2\"" + @">CODE2</Scode>
			    </SymptomCode>
				<MileageIn>123456</MileageIn>
			    <CodesAndComments>
				    <ComplaintCode>COMPLAINT1</ComplaintCode>
				    <ComplaintCodeType>TEST</ComplaintCodeType>
				    <ComplaintDescription>This is a test</ComplaintDescription>
			    </CodesAndComments>
			</GRequest>";

        private const string ADAMBroadcastMessageRequest =
            @"<GRequest>
				<ApplicationID>ADM</ApplicationID>
				<ApplicationType>GOASIS</ApplicationType>
				<IMSRegion>TEST</IMSRegion>
				<XMLOnly>Y</XMLOnly>
				<FSAOnly>N</FSAOnly>
				<GSA>USA</GSA>
				<UserID>j-doe12</UserID>
				<PACode>DSP01</PACode>
				<SubDealerID/>
				<LanguageCode>EN</LanguageCode>
				<BCM>Y</BCM>
				<VehicleInfo>N</VehicleInfo>
				<Warranty>N</Warranty>
				<VIN/>
				<SymptomCode>
					<Scode num=" + "\"1\"" + @">805000</Scode>
				</SymptomCode>
				<MileageIn/>
			</GRequest>";

        private const string ADAMVinRequest =
            @"<GRequest>
				<ApplicationID>ADM</ApplicationID>
				<ApplicationType>GOASIS</ApplicationType>
				<IMSRegion>TEST</IMSRegion>
				<XMLOnly>Y</XMLOnly>
				<FSAOnly>N</FSAOnly>
				<GSA>USA</GSA>
				<UserID>j-doe12</UserID>
				<PACode>DSP01</PACode>
				<SubDealerID/>
				<LanguageCode>EN</LanguageCode>
				<BCM>N</BCM>
				<VehicleInfo>Y</VehicleInfo>
				<Warranty>Y</Warranty>
				<VIN>1FMCU9J95DUA04123</VIN>
				<SymptomCode>
					<Scode num=" + "\"1\"" + @">102000</Scode>
					<Scode num=" + "\"2\"" + @">100000</Scode>
					<Scode num=" + "\"3\"" + @">200000</Scode>
					<Scode num=" + "\"4\"" + @">203200</Scode>
					<Scode num=" + "\"5\"" + @">207000</Scode>
				</SymptomCode>
				<MileageIn>48270</MileageIn>
				<CodesAndComments>
					<ComplaintCode>A16</ComplaintCode>
					<ComplaintCodeType/>
					<ComplaintDescription>CD PLAYER TROUBLES</ComplaintDescription>
				</CodesAndComments>
			</GRequest>";
    }
}
