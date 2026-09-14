namespace Karmak.Integrations.Volvo.Warranty.Tests.TestUtils
{
    public class OWSSoapResponseFixtures
    {
        public const string CustomerConcernCodes = @"<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
  <soap:Header/>
  <soapenv:Body wsu:Id='Id-a6cf2eeb-4fb1-4334-aa31-40e1cdbdf9e4' xmlns:wsu='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd' xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'>
    <a:ProcessMessageResponse xmlns:a='http://www.starstandards.org/webservices/2005/10/transport'>
      <a:payload>
        <a:content id='Content01'>
          <star:ShowStandardCodes xmlns:ns2='urn:volvo/WarrantyClaim/Extended/RepairOrder/v1.0' xmlns:ns3='http://www.openapplications.org/oagis/9' xmlns:ns4='urn:volvo/WarrantyClaim/Extended/ServiceProcessingAdvisory/v1.0' xmlns:star='http://www.starstandard.org/STAR/5' languageCode='en-US' releaseID='5.2.4' systemEnvironmentCode='Test'>
            <star:ApplicationArea>
              <star:Sender>
                <star:TaskID>ShowStandardCodes</star:TaskID>
                <star:ConfirmationCode>Never</star:ConfirmationCode>
                <star:CreatorNameCode>FM</star:CreatorNameCode>
                <star:SenderNameCode xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:type='ns3:CodeType'>FM</star:SenderNameCode>
                <star:LanguageCode>en-US</star:LanguageCode>
                <star:ServiceID>OWS Standard Codes CUSTOMER CONCERN CODE</star:ServiceID>
              </star:Sender>
              <star:CreationDateTime>2020-01-23T18:52:37Z</star:CreationDateTime>
              <star:Destination>
                <star:DealerNumberID>K1234!</star:DealerNumberID>
                <star:DealerTargetCountry>US</star:DealerTargetCountry>
              </star:Destination>
            </star:ApplicationArea>
            <star:ShowStandardCodesDataArea>
              <star:Show></star:Show>
              <star:StandardCodes>
                <star:StandardCodesHeader>
                  <star:DocumentDateTime>2020-01-23T18:52:37Z</star:DocumentDateTime>
                  <star:DocumentIdentificationGroup>
                    <star:DocumentIdentification>
                      <star:DocumentID>Default</star:DocumentID>
                    </star:DocumentIdentification>
                  </star:DocumentIdentificationGroup>
                  <star:CodesAction>RA</star:CodesAction>
                  <star:LanguageCode>en-US</star:LanguageCode>
                  <star:TableName>OWS Standard Codes CUSTOMER CONCERN CODE</star:TableName>
                </star:StandardCodesHeader>
                <star:StandardCodesLineItems>
                  <star:Code>^296</star:Code>
                  <star:CodeDescription>Body</star:CodeDescription>
                  <star:Values>
                    <star:CodeValue>^300</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^301</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^302</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^303</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^304</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^305</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^306</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^307</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^308</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^309</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^310</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^311</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^312</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^313</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^314</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^315</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^316</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^317</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^318</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^319</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^458</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^459</star:CodeValue>
                  </star:Values>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>^297</star:Code>
                  <star:CodeDescription>Powertrain</star:CodeDescription>
                  <star:Values>
                    <star:CodeValue>^320</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^321</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^322</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^323</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^324</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^325</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^326</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^327</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^328</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^329</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^463</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^464</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^465</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^466</star:CodeValue>
                  </star:Values>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>^298</star:Code>
                  <star:CodeDescription>Chassis</star:CodeDescription>
                  <star:Values>
                    <star:CodeValue>^330</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^331</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^332</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^333</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^334</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^335</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^336</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^467</star:CodeValue>
                  </star:Values>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>^299</star:Code>
                  <star:CodeDescription>Electrical</star:CodeDescription>
                  <star:Values>
                    <star:CodeValue>^337</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^338</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^339</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^340</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^341</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^342</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^343</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^344</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>A79</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^457</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^460</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^461</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>^462</star:CodeValue>
                  </star:Values>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>A79</star:Code>
                  <star:CodeDescription>CONNECTOR TERMINAL ISSUES</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>^300</star:Code>
                  <star:CodeDescription>Paint Finish On Body Parts (not Including Trim)</star:CodeDescription>
                  <star:Values>
                    <star:CodeValue>F04</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F05</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F06</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F07</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F10</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F11</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F12</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F13</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F15</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F19</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F20</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F25</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>F30</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>12C</star:CodeValue>
                  </star:Values>
                  <star:Values>
                    <star:CodeValue>R50</star:CodeValue>
                  </star:Values>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F04</star:Code>
                  <star:CodeDescription>THIN/NO PAINT (EXCLUDES TRIM/BUMPER)</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F05</star:Code>
                  <star:CodeDescription>SAGS/RUNS IN PAINT (EXCLUDES TRIM/BUMPER)</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F06</star:Code>
                  <star:CodeDescription>PEELED PAINT (EXCLUDES TRIM/BUMPER)</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F07</star:Code>
                  <star:CodeDescription>BUBBLES/BLISTERS IN PAINT (EXCLUDES BUMPERS/TRIM)</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F10</star:Code>
                  <star:CodeDescription>PAINT SPRAY OVER BODY FINISH</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F11</star:Code>
                  <star:CodeDescription>BODY RUST/CORROSION (NOT PERFORATION, EXCL BUMPER)</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F12</star:Code>
                  <star:CodeDescription>STAINED/SPOTTED PAINT (EXCLUDES TRIM/BUMPER)</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F13</star:Code>
                  <star:CodeDescription>FADED/DULL PAINT (EXCLUDES TRIM/BUMPER)</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F15</star:Code>
                  <star:CodeDescription>DETAIL PAINT OR TAPE STRIPE TROUBLES</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F19</star:Code>
                  <star:CodeDescription>CHIPPED/SCRATCHES PAINT</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F20</star:Code>
                  <star:CodeDescription>DIRT IN PAINT (EXCLUDES TRIM/BUMPER)</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F25</star:Code>
                  <star:CodeDescription>RUST (PERFORATION)</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>F30</star:Code>
                  <star:CodeDescription>UNEVEN COLOR/COLOR DIFFERENT BETWEEN BODY PANELS</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>12C</star:Code>
                  <star:CodeDescription>Entertainment Systems</star:CodeDescription>
                </star:StandardCodesLineItems>
                <star:StandardCodesLineItems>
                  <star:Code>R50</star:Code>
                  <star:CodeDescription>UNDERBODY RUST/CORROSION</star:CodeDescription>
                </star:StandardCodesLineItems>
              </star:StandardCodes>
            </star:ShowStandardCodesDataArea>
          </star:ShowStandardCodes>
        </a:content>
      </a:payload>
    </a:ProcessMessageResponse>
  </soapenv:Body>
</soap:Envelope>";

        public const string DamageCodes = @"<?xml version='1.0' encoding='UTF-8'?>
<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
  <soap:Header/>
  <soapenv:Body wsu:Id='Id-87e3f8b6-6da1-437a-8544-e26c9f58fa54' xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:wsu='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'>
    <ProcessMessageResponse xmlns='http://www.starstandards.org/webservices/2005/10/transport'>
      <payload>
        <ns:ShowStandardCodes languageCode='en-US' releaseID='1.0' systemEnvironmentCode='Test' xmlns:ns='http://www.starstandard.org/STAR/5' xmlns:ns1='http://www.openapplications.org/oagis/9' xmlns:star='http://www.starstandard.org/STAR/5'>
          <ns:ApplicationArea>
            <ns:Sender>
              <ns:TaskID>ShowStandardCodes</ns:TaskID>
              <ns:ConfirmationCode>Never</ns:ConfirmationCode>
              <ns:CreatorNameCode>FM</ns:CreatorNameCode>
              <ns:SenderNameCode>FM</ns:SenderNameCode>
              <ns:LanguageCode>ab-GE</ns:LanguageCode>
              <ns:ServiceID>OWS Standard Codes Damage Codes</ns:ServiceID>
            </ns:Sender>
            <ns:CreationDateTime>2019-12-24T18:15:35Z</ns:CreationDateTime>
            <ns:Destination>
              <ns:DealerNumberID>K1234!</ns:DealerNumberID>
              <ns:DealerTargetCountry>US</ns:DealerTargetCountry>
            </ns:Destination>
          </ns:ApplicationArea>
          <ns:ShowStandardCodesDataArea>
            <ns:Show/>
            <ns:StandardCodes>
              <ns:StandardCodesHeader>
                <ns:DocumentDateTime>2017-11-25T00:15:05Z</ns:DocumentDateTime>
                <ns:DocumentIdentificationGroup>
                  <ns:DocumentIdentification>
                    <ns:DocumentID>Default</ns:DocumentID>
                  </ns:DocumentIdentification>
                </ns:DocumentIdentificationGroup>
                <ns:CodesAction>RA</ns:CodesAction>
                <ns:TableName>Customer Concern ns:Code</ns:TableName>
              </ns:StandardCodesHeader>
              <star:StandardCodesLineItems>
                <star:Code>00</star:Code>
                <star:CodeDescription>NO EXCEPTIONS</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE AREA</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>01</star:Code>
                <star:CodeDescription>ANTENNA</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE AREA</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>02</star:Code>
                <star:CodeDescription>BATTERY</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE AREA</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>03</star:Code>
                <star:CodeDescription>BUMPER/COVER/EXTENSION, FRONT</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE AREA</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>04</star:Code>
                <star:CodeDescription>BUMPER/COVER/EXTENSION, REAR</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE AREA</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>05</star:Code>
                <star:CodeDescription>BUMPER GUARD/STRIP, FRONT</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE AREA</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>06</star:Code>
                <star:CodeDescription>BUMPER GUARD/STRIP, REAR</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE AREA</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>00</star:Code>
                <star:CodeDescription>NO EXCEPTION</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE CONDITION</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>01</star:Code>
                <star:CodeDescription>BENT</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE CONDITION</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>02</star:Code>
                <star:CodeDescription>BROKEN (EXCEPT GLASS)</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE CONDITION</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>03</star:Code>
                <star:CodeDescription>CUT</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE CONDITION</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>04</star:Code>
                <star:CodeDescription>DENTED (PAINT BROKEN)</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE CONDITION</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>0</star:Code>
                <star:CodeDescription>NO EXCEPTION</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE SERVICE</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>1</star:Code>
                <star:CodeDescription>UP TO AND INCLUDING 1&quot; LENGTH/DIAMETER</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE SERVICE</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>2</star:Code>
                <star:CodeDescription>OVER 1&quot; UP TO AND INCLUDING 3&quot; LENGTH/DIAMETER</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE SERVICE</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>3</star:Code>
                <star:CodeDescription>OVER 3&quot; UP TO AND INCLUDING 6&quot; LENGTH/DIAMETER</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE SERVICE</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
              <star:StandardCodesLineItems>
                <star:Code>4</star:Code>
                <star:CodeDescription>OVER 6&quot; UP TO AND INCLUDING 12&quot; LENGTH/DIAMETER</star:CodeDescription>
                <star:Values>
                  <star:CodeSupplementalDescription>DAMAGE SERVICE</star:CodeSupplementalDescription>
                </star:Values>
              </star:StandardCodesLineItems>
            </ns:StandardCodes>
          </ns:ShowStandardCodesDataArea>
        </ns:ShowStandardCodes>
      </payload>
    </ProcessMessageResponse>
  </soapenv:Body>
</soap:Envelope>";

        public const string ConditionCodes = @"<?xml version='1.0' encoding='UTF-8'?>
<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
	<soap:Header/>
	<soapenv:Body wsu:Id='Id-a6e24877-3d29-4c75-831a-e26c9f5814b0' xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:wsu='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'>
		<ProcessMessageResponse xmlns='http://www.starstandards.org/webservices/2005/10/transport'>
			<payload>
				<tran:content id='Content0' xmlns:ns='http://www.starstandard.org/STAR/5' xmlns:tran='http://www.starstandards.org/webservices/2005/10/transport'>
					<ns:ShowStandardCodes languageCode='en-US' releaseID='1.0' systemEnvironmentCode='Production'>
						<ns:ApplicationArea>
							<ns:Sender>
								<ns:TaskID>ShowStandardCodes</ns:TaskID>
								<ns:ConfirmationCode>Never</ns:ConfirmationCode>
								<ns:CreatorNameCode>FM</ns:CreatorNameCode>
								<ns:SenderNameCode>FM</ns:SenderNameCode>
								<ns:LanguageCode>en_US</ns:LanguageCode>
								<ns:ServiceID>OWS Standard Codes Condition Codes</ns:ServiceID>
							</ns:Sender>
							<ns:CreationDateTime>2019-12-24T18:15:32Z</ns:CreationDateTime>
							<ns:Destination>
								<ns:DealerNumberID>K1234!</ns:DealerNumberID>
								<ns:DealerTargetCountry>US</ns:DealerTargetCountry>
							</ns:Destination>
						</ns:ApplicationArea>
						<ns:ShowStandardCodesDataArea>
							<ns:Show/>
							<ns:StandardCodes>
								<ns:StandardCodesHeader>
									<ns:DocumentDateTime>2017-11-25T00:15:05Z</ns:DocumentDateTime>
									<ns:DocumentIdentificationGroup>
										<ns:DocumentIdentification>
											<ns:DocumentID>Default</ns:DocumentID>
										</ns:DocumentIdentification>
									</ns:DocumentIdentificationGroup>
									<ns:CodesAction>RA</ns:CodesAction>
									<ns:LanguageCode>en-US</ns:LanguageCode>
									<ns:TableName>Condition Code</ns:TableName>
								</ns:StandardCodesHeader>
								<ns:StandardCodesLineItems>
									<ns:Code>A8</ns:Code>
									<ns:CodeDescription>STONE PECKING</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>B4</ns:Code>
									<ns:CodeDescription>PINCHED/DAMAGED WIRE</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>B5</ns:Code>
									<ns:CodeDescription>BATTERY ACID/FLUID DAMAGE</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>B5</ns:Code>
									<ns:CodeDescription>BATTERY ACID/FLUID DAMAGE</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>C8</ns:Code>
									<ns:CodeDescription>INDUSTRIAL/ENVIRONMENTAL FALLOUT</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>D1</ns:Code>
									<ns:CodeDescription>POROSITY</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>D4</ns:Code>
									<ns:CodeDescription>FLAW IN MATERIAL</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>D7</ns:Code>
									<ns:CodeDescription>CORROSION (PERFORATION)</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>D8</ns:Code>
									<ns:CodeDescription>FAILED GASKET/SEAL</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>D9</ns:Code>
									<ns:CodeDescription>OUT OF BALANCE</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>P1</ns:Code>
									<ns:CodeDescription>POLISH REPAIR (PAINT)</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>P2</ns:Code>
									<ns:CodeDescription>SPOT REPAIR (PAINT</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>P3</ns:Code>
									<ns:CodeDescription>SPRAY PANEL REPAIR (PAINT)</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>W6</ns:Code>
									<ns:CodeDescription>WHEEL ALIGNMENT OUT OF SPECIFICATION</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>X1</ns:Code>
									<ns:CodeDescription>POOR GROUND</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>X4</ns:Code>
									<ns:CodeDescription>DAMAGED TERMINAL</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>X7</ns:Code>
									<ns:CodeDescription>CROSSED WIRE (WIRE HARNESS)</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>01</ns:Code>
									<ns:CodeDescription>BROKEN/CRACKED</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>02</ns:Code>
									<ns:CodeDescription>BENT/BUCKLED/KINKED</ns:CodeDescription>
								</ns:StandardCodesLineItems>
								<ns:StandardCodesLineItems>
									<ns:Code>05</ns:Code>
									<ns:CodeDescription>POOR METAL FINISHING</ns:CodeDescription>
								</ns:StandardCodesLineItems>
							</ns:StandardCodes>
						</ns:ShowStandardCodesDataArea>
					</ns:ShowStandardCodes>
				</tran:content>
			</payload>
		</ProcessMessageResponse>
	</soapenv:Body>
</soap:Envelope>";

        public const string UnexpectedOWSResponse = @"<?xml version='1.0' encoding='UTF-8'?>
<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
	<soap:Header/>
	<soapenv:Body wsu:Id='Id-06578844-442d-492d-82d3-e26c9f58d954' xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:wsu='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'>
		<ProcessMessageResponse xmlns='http://www.starstandards.org/webservices/2005/10/transport'>
			<payload>
				<tran:content xmlns:star='http://www.starstandard.org/STAR/5' xmlns:tran='http://www.starstandards.org/webservices/2005/10/transport' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
					<star:ShowStandardCodesV2 languageCode='en-US' releaseID='5.2.4' systemEnvironmentCode='Production' versionID='1.0'>
						<star:ApplicationArea>
							<star:Sender>
								<star:TaskID>ShowStandardCodes</star:TaskID>
								<star:ConfirmationCode>Never</star:ConfirmationCode>
								<star:CreatorNameCode>FM</star:CreatorNameCode>
								<star:SenderNameCode>FM</star:SenderNameCode>
								<star:LanguageCode>en_US</star:LanguageCode>
								<star:ServiceID>02198!</star:ServiceID>
							</star:Sender>
							<star:CreationDateTime>2019-12-24T18:15:38Z</star:CreationDateTime>
							<star:Destination>
								<star:DealerNumberID>K1234!</star:DealerNumberID>
								<star:DealerTargetCountry>US</star:DealerTargetCountry>
							</star:Destination>
						</star:ApplicationArea>
						<star:ShowStandardCodesDataArea>
							<star:StandardCodes>
								<star:StandardCodesHeader>
									<star:DocumentDateTime>2010-04-29T13:14:49Z</star:DocumentDateTime>
									<star:DocumentIdentificationGroup>
										<star:DocumentIdentification>
											<star:DocumentID>Default</star:DocumentID>
										</star:DocumentIdentification>
									</star:DocumentIdentificationGroup>
									<star:CodesAction>RA</star:CodesAction>
									<star:LanguageCode>en-US</star:LanguageCode>
									<star:TableName>OWS Standard Codes CUSTOMER CONCERN CODE</star:TableName>
								</star:StandardCodesHeader>
								<star:StandardCodesLineItems>
									<star:Code>A01</star:Code>
									<star:CodeDescription>BUMPER SAGS/RUNS IN PAINT</star:CodeDescription>
									<star:Values>
										<star:CodeValue>B04</star:CodeValue>
									</star:Values>
									<star:Values>
										<star:CodeValue>H99</star:CodeValue>
									</star:Values>
									<star:Values>
										<star:CodeValue>M02</star:CodeValue>
									</star:Values>
								</star:StandardCodesLineItems>
							</star:StandardCodes>
						</star:ShowStandardCodesDataArea>
					</star:ShowStandardCodesV2>
				</tran:content>
			</payload>
		</ProcessMessageResponse>
	</soapenv:Body>
</soap:Envelope>";

        public const string SoapFault = @"<?xml version='1.0' encoding='UTF-8'?>
<soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
	<soap:Header/>
	<soapenv:Body wsu:Id='Id-93e08d4a-3fb9-4f9d-b19c-e26c9f584a6c' xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:wsu='http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd'>
		<soapenv:Fault>
			<faultcode>soapenv:Client</faultcode>
			<faultstring>Not valid Soap Header</faultstring>
			<detail>
				<ErrorMessage>NameSpace : urn:volvo/star/security/v1.0 is invalid</ErrorMessage>
			</detail>
		</soapenv:Fault>
	</soapenv:Body>
</soap:Envelope>
";
    }
}
