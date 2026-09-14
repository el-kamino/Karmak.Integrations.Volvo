using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Common;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.CustomerUpdate.Extensions;
using Karmak.Integrations.Volvo.React.Gen.Helpers;
using Karmak.Integrations.Volvo.React.Mappers.CustomerUpdate;
using Karmak.Integrations.Volvo.React.Mappers.Shared;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Karmak.Integrations.Volvo.React.Utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using CustomerInfo = Karmak.Integrations.Volvo.React.Contracts.CustomerUpdates.Data.CustomerUpdate;
using SoapAddress = Karmak.Integrations.Volvo.React.Transport.Soap.V5_14_4.ProcessCustomerUpdateWebServiceAddress;

namespace Karmak.Integrations.Volvo.React.CustomerUpdate
{
    internal class CreateProcessCustomerUpdateTransmission
    {
        private const string RELEASE_ID = "5.10.2";
        private const string DELETED = "deleted";
        private const string INITIAL_LOAD = "initial load";

        private const int VOLVO_PASS_ID_MAX_LENGTH = 10;

        private readonly VolvoSettings _settings;
        private readonly CustomerInfo _customerInfo;
        private readonly SoapAddress _soapAddr;

        private readonly string _newOrHistorical;
        private readonly ProcessorOptions _options;
        private readonly ISoapRequestFactory _soapRequestFactory;

        public CreateProcessCustomerUpdateTransmission(
            IOptions<ProcessorOptions> options, 
            ISoapRequestFactory soapRequestFactory,
            VolvoSettings settings, 
            CustomerInfo customerInfo, 
            SoapAddress soapAddr, 
            string newOrHistorical = TransmissionType.New)
        {
            _options = options.Value;
            _soapRequestFactory = soapRequestFactory;

            _settings = settings;
            _customerInfo = customerInfo;
            _soapAddr = soapAddr;
            _newOrHistorical = newOrHistorical;
        }

        internal List<SoapEnvelope> BuildMessageList()
        {
            PartyABIEType party = ResolveCustomerParty();
            string transTypeCode = ResolveTransactionTypeCode();
            var msgList = new List<SoapEnvelope>();
            switch (transTypeCode)
            {
                case TransactionTypeCode.AddVin:
                    msgList.Add(BuildMessage(transTypeCode, party, _customerInfo.AddedVIN));
                    break;
                case TransactionTypeCode.RemoveVin:
                    msgList.Add(BuildMessage(transTypeCode, party, _customerInfo.RemovedVIN));
                    break;
                default:
                    if (_customerInfo.CurrentVINs.Any())
                    {
                        msgList.AddRange(_customerInfo.CurrentVINs.Select(
                            vin => BuildMessage(transTypeCode, party, vin)));
                    }
                    else if (_customerInfo.HasVolvoPassRewards())
                    {
                        msgList.Add(BuildMessage(transTypeCode, party));
                    }
                    break;
            }
            return msgList;
        }

        internal SoapEnvelope BuildMessage(string transTypeCode, PartyABIEType party, string vin = null)
        {
            var payload = new ProcessCustomerInformationType
            {
                releaseID = RELEASE_ID,
                systemEnvironmentCode = _options.Environment,
                languageCode = LanguageEnumeratedType.enUS,
                ApplicationArea = ApplicationAreaMapper.Map(_settings, _customerInfo.Metadata.FusionVersion, _newOrHistorical, _customerInfo.TimeZone),
                ProcessCustomerInformationDataArea = new ProcessCustomerInformationDataAreaType
                {
                    Process = ProcessTypeMapper.Map(),
                    CustomerInformation = BuildCustomerInfo(transTypeCode, party, vin)
                }
            };

            payload.ApplicationArea.Sender.ReferenceID = new IdentifierType
            {
                Value = Guid.NewGuid().ToString()
            };

            return _soapRequestFactory.CreateProcessRequest(_soapAddr, payload);
        }

        private CustomerInformationType[] BuildCustomerInfo(string transTypeCode, PartyABIEType party, string vin = null)
        {
            return new CustomerInformationType[] {
                new CustomerInformationType
                {
                    CustomerInformationHeader = new CustomerInformationHeaderType
                    {
                        DocumentDateTime = XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(
                            DateTime.UtcNow.ToLocalTime(),
                            _customerInfo.TimeZone),
                        DocumentDateTimeSpecified = true,
                        DocumentIdentificationGroup = new DocumentIdentificationGroupType
                        {
                            DocumentIdentification = new DocumentIdentificationType
                            {
                                DocumentID = new IdentifierType
                                {
                                    Value = "N/A"
                                }
                            }
                        },
                        TransactionTypeCode = new CodeType
                        {
                            Value = transTypeCode
                        }
                    },

                    CustomerInformationDetail = new CustomerInformationDetailType
                    {
                        CustomerRoleToVehicleCode = new CodeType{Value = "O"},
                        CustomerParty = party,
                        VehicleID = string.IsNullOrWhiteSpace(vin)
                        ? null
                        : new IdentifierType
                        {
                            Value = vin
                        }
                    }
                }
            };
        }

        private string ResolveTransactionTypeCode()
        {
            return _customerInfo.Source switch
            {
                DELETED => TransactionTypeCode.Delete,
                INITIAL_LOAD => TransactionTypeCode.InitialLoad,
                _ when !string.IsNullOrWhiteSpace(_customerInfo.AddedVIN) => TransactionTypeCode.AddVin,
                _ when !string.IsNullOrWhiteSpace(_customerInfo.RemovedVIN) => TransactionTypeCode.RemoveVin,
                _ => _customerInfo.SentToVolvo ? TransactionTypeCode.Update : TransactionTypeCode.New
            };
        }

        private PartyABIEType ResolveCustomerParty()
        {
            //use bill to
            var party = _customerInfo.Customer.BusinessStructure.EqualsIgnoreCase("Individual")
                ? new PartyABIEType
                {
                    Item = SpecifiedPersonMapper.Map(_customerInfo.Customer, _settings.RegionSettings.LanguageCode)
                }
                : new PartyABIEType
                {
                    Item = OrganizationMapper.Map(_customerInfo.Customer, _settings.RegionSettings.LanguageCode)
                };

            party.ManufacturerHouseholdID = _customerInfo.HasVolvoPassRewards()
                ? new IdentifierType { Value = _customerInfo.VolvoPassRewardID.ToString().MaxLength(VOLVO_PASS_ID_MAX_LENGTH) }
                : null;

            return party;
        }
    }
}
