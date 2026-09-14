using Elk.Integrations.Volvo.Core.Mappers.PartsSalesOrders.V5_14_4;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.PartSales.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Core.Mappers.PartsSalesOrders.V5_14_4;
using Karmak.Integrations.Volvo.React.Core.Mappers.Shared.V5_14_4;
using Karmak.Integrations.Volvo.React.Gen.Helpers;
using Microsoft.Extensions.Options;
using System;

namespace Karmak.Integrations.Volvo.React.PartsSalesOrders.V5_14_4
{
    internal class CreateProcessPartsInvoice
    {
        private const string RELEASE_ID = "5.14.4";
        private readonly ProcessorOptions _processorOptions;
        private readonly PartsSalesOrder _partsSalesOrder;
        private readonly string _newOrHistorical;
        private readonly VolvoSettings _settings;

        public CreateProcessPartsInvoice(
            IOptions<ProcessorOptions> options,
            VolvoSettings settings, 
            PartsSalesOrder partsSalesOrder, 
            string newOrHistorical = TransmissionType.New)
        {
            _processorOptions = options.Value;
            _settings = settings;
            _partsSalesOrder = partsSalesOrder;
            _newOrHistorical = newOrHistorical;
        }

        public ProcessPartsInvoiceType BuildMessage()
        {
            CodeType processCode = GetProcessCode(_partsSalesOrder.InvoiceDate, _partsSalesOrder.SalesOrderStatus);
            bool isCanceled = processCode.Value == "CANCELED";

            return new ProcessPartsInvoiceType
            {
                releaseID = RELEASE_ID,
                systemEnvironmentCode = _processorOptions.Environment,
                languageCode = LanguageEnumeratedType.enUS,
                ApplicationArea = ApplicationAreaMapper.Map(_settings, _partsSalesOrder.Metadata.FusionVersion, _newOrHistorical, _partsSalesOrder.TimeZone),
                ProcessPartsInvoiceDataArea = new ProcessPartsInvoiceDataAreaType
                {
                    Process = ProcessTypeMapper.Map(),
                    PartsInvoice = new[] {
                        new PartsInvoiceType {
                            PartsInvoiceHeader = new PartsInvoiceHeaderType {
                                DocumentDateTime =  GetFormattedDateTimeOffset(DateTime.UtcNow.ToLocalTime()),
                                ReferenceNumberString = string.IsNullOrEmpty(_partsSalesOrder.OriginalPartsOrderNumber)
                                    ? null
                                    : _partsSalesOrder.OriginalPartsOrderNumber,
                                DocumentDateTimeSpecified = true,
                                DocumentIdentificationGroup = DocumentIdentificationGroupMapper.Map(_partsSalesOrder.PartsOrderNumber, _partsSalesOrder.CustomerPurchaseOrderNumber),
                                PartsOrderReceivedDateTime = GetFormattedDateTimeOffset(_partsSalesOrder.OriginalInvoiceDate),
                                PartsOrderReceivedDateTimeSpecified = _partsSalesOrder.OriginalInvoiceDate != null,
                                InvoiceDateTime = GetFormattedDateTimeOffset(_partsSalesOrder.InvoiceDate),
                                InvoiceDateTimeSpecified = _partsSalesOrder.InvoiceDate != null,
                                OrderDateTime = GetFormattedDateTimeOffset(_partsSalesOrder.AddDate),
                                OrderDateTimeSpecified = _partsSalesOrder.AddDate != null,
                                Charges = ShippingChargesMapper.Map(_partsSalesOrder, _settings, isCanceled),
                                Tax = TaxMapper.Map(_partsSalesOrder.TaxTotal, _settings.RegionSettings.CurrencyCode, isCanceled),
                                PartsDiscountAmount = new AmountType {
                                    Value = 0m,
                                    currencyID = _settings.RegionSettings.CurrencyCode
                                },
                                SoldToParty = CustomerPartyMapper.MapBillingCustomer(_partsSalesOrder.BillingCustomer, _settings),
                                ShipToParty = CustomerPartyMapper.MapShippingCustomer(_partsSalesOrder.ShipToCustomer, _settings),
                                SoldByParty = SoldByPartyMapper.Map(_partsSalesOrder),
                                PartsInvoiceType = PartsInvoiceTypeMapper.Map(_partsSalesOrder.Source, _settings),
                                PartsCustomerTypeCode = PartsCustomerTypeCodeMapper.Map(_partsSalesOrder.BillingCustomer.CustomerTypeCode),
                                PartsInvoiceTypeDescription = PartsInvoiceTypeDescriptionMapper.Map(_partsSalesOrder.Source, _settings),
                                ProcessCode = processCode
                            },
                            PartsInvoiceLine = PartsMapper.Map(_partsSalesOrder.Parts, _partsSalesOrder.MiscCharges, isCanceled, _settings, _partsSalesOrder.TimeZone)
                        }
                    }
                }
            };
        }

        private CodeType GetProcessCode(DateTime? invoiceDate, string status)
        {
            string statusCode = "";
            if (status == "deleted")
            {
                statusCode = "CANCELED";
            }
            else
            {
                if (_partsSalesOrder.InvoiceDate != null)
                {
                    statusCode = "CLOSED";
                }
                else
                {
                    statusCode = "OPEN";
                }
            }


            return new CodeType
            {
                Value = statusCode
            };
        }

        private XmlSerializableDateTimeOffset GetFormattedDateTimeOffset(DateTime? date)
        {
            return XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(date, _partsSalesOrder.TimeZone);
        }
    }
}