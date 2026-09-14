using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Common;
using Karmak.Integrations.Volvo.React.Contracts.VehicleSales.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Core.Mappers.UsedVehicleSales.V5_14_4;
using Karmak.Integrations.Volvo.React.Gen.Helpers;
using Karmak.Integrations.Volvo.React.Mappers.Shared;
using Karmak.Integrations.Volvo.React.Utils;
using System;

namespace Karmak.Integrations.Volvo.React.UsedVehicleSales.V5_14_4
{
    internal class CreateProcessRetailDeliveryReportingTransmission
    {
        private const string RELEASE_ID = "5.10.2";
        private const string ONLY_SUPPORTED_SALE_CATEGORY = "R";

        private readonly ProcessorOptions _options;
        private readonly VolvoSettings _settings;
        private readonly VehicleSalesOrder _invoice;
        private readonly string _newOrHistorical;

        public CreateProcessRetailDeliveryReportingTransmission(
            ProcessorOptions options, 
            VolvoSettings settings, 
            VehicleSalesOrder invoice, 
            string newOrHistorical)
        {
            _options = options;
            _settings = settings;
            _invoice = invoice;
            _newOrHistorical = newOrHistorical;
        }

        public ProcessRetailDeliveryReportingType BuildMessage()
        {
            return new ProcessRetailDeliveryReportingType
            {
                releaseID = RELEASE_ID,
                systemEnvironmentCode = _options.Environment,
                languageCode = LanguageEnumeratedType.enUS,
                ApplicationArea = ApplicationAreaMapper.Map(_settings, _invoice.Metadata.FusionVersion, _newOrHistorical, _invoice.TimeZone),
                ProcessRetailDeliveryReportingDataArea = new ProcessRetailDeliveryReportingDataAreaType
                {
                    Process = ProcessTypeMapper.Map(),
                    RetailDeliveryReporting = new[] {
                        new RetailDeliveryReportingType {
                            RetailDeliveryReportingHeader = new RetailDeliveryReportingHeaderType {
                                DocumentDateTime = XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(
                                    DateTime.UtcNow.ToLocalTime(),
                                    _invoice.TimeZone),
                                DocumentDateTimeSpecified = true,
                                DocumentIdentificationGroup = DocumentIdentificationGroupMapper.Map(_invoice.InvoiceNumber),
                                SalesDate = _invoice.SalesDate.GetValueOrDefault(),
                                SalesDateSpecified = true,
                                SaleCategoryString = ONLY_SUPPORTED_SALE_CATEGORY,
                                SalesPersonParty = EmployeePartyMapper.Map(_invoice.SalesPersonId),
                                BuyerParty = BuyerPartyMapper.Map(_invoice.Customer, _settings),
                                TradeInVehicleCredit = VehicleMapper.MapTradeIn(_invoice.TradeInVehicles)
                            },
                            RetailDeliveryReportingVehicleLineItem = VehicleMapper.MapSale(_invoice.SoldVehicles)
                        }
                    }
                }
            };
        }
    }
}