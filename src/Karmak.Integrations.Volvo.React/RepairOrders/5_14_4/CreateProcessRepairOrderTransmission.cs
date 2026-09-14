using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Core.Mappers.RepairOrder.V5_14_4;
using Karmak.Integrations.Volvo.React.Core.Mappers.Shared.V5_14_4;
using System;
using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.RepairOrders;
using Karmak.Integrations.Volvo.React.Common;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.Settings;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Gen.Helpers;
using Karmak.Integrations.Volvo.React.RepairOrders.VolvoEvents;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.V5_14_4;

internal class CreateProcessRepairOrderTransmission
{
    private const decimal DEFAULT_ODOMETER_READING = 1m;
    private const decimal MAX_ODOMETER = 999999m;
    private const int MODEL_MAX_LENGTH = 30;
    private const int MAKE_STRING_MAX_LENGTH = 30;
    private const string RELEASE_ID = "5.14.4";
    private const string ORDER_INTERNAL_NOTES = "U";
    private const int MAX_FUSION_VERSION_LENGTH = 9;

    private readonly VolvoEvent _volvoEvent;
    private readonly VolvoSettings _settings;
    private readonly RepairOrderSnapshot _repairOrder;
    private readonly string _transmissionType;
    private readonly ProcessorOptions _options;

    public static CreateProcessRepairOrderTransmission New(ProcessorOptions options, VolvoSettings settings, RepairOrderSnapshot snapshot, VolvoEvent volvoEvent)
    {
        return new CreateProcessRepairOrderTransmission(options, settings, snapshot, volvoEvent, TransmissionType.New);
    }

    public static CreateProcessRepairOrderTransmission Historical(ProcessorOptions options, VolvoSettings settings, RepairOrderSnapshot snapshot, VolvoEvent volvoEvent)
    {
        return new CreateProcessRepairOrderTransmission(options, settings, snapshot, volvoEvent, TransmissionType.Historical);
    }

    private CreateProcessRepairOrderTransmission(ProcessorOptions options, VolvoSettings settings, RepairOrderSnapshot repairOrder, VolvoEvent volvoEvent, string transmissionType)
    {
        _repairOrder = repairOrder;
        _volvoEvent = volvoEvent;
        _settings = settings;
        _transmissionType = transmissionType;
        _options = options;
    }

    public ProcessRepairOrderType BuildMessage()
    {
        Enum.TryParse<CountryEnumeratedType>(_settings.RegionSettings.CountryCode, out var countryCode);
        // don't blame me, this addresses some stupid, arbitrary, undocumented rules.
        var canceledRODataArea = GetStrippedDownDataAreaForCancelled();

        return new ProcessRepairOrderType
        {
            releaseID = RELEASE_ID,
            systemEnvironmentCode = _options.Environment,
            languageCode = LanguageEnumeratedType.enUS,
            ApplicationArea = new ApplicationAreaTypeStar
            {
                Destination = new DestinationType(),
                Sender = new SenderTypeStar
                {
                    LanguageCode = _settings.RegionSettings.LanguageCode,
                    SystemVersion = _repairOrder.DatabaseVersion.MaxLength(MAX_FUSION_VERSION_LENGTH),
                    TaskID = new IdentifierType
                    {
                        Value = _transmissionType
                    },
                    CreatorNameCode = new TextType
                    {
                        Value = DspConstants.Code
                    },
                    SenderNameCode = new CodeType
                    {
                        Value = DspConstants.ShortCode
                    },
                    DealerNumberID = new IdentifierType
                    {
                        Value = _settings.InterfaceOptions.PaCode
                    },
                    AreaNumber = new TextType
                    {
                        Value = FranchiseCodeType.Fetch(_settings.InterfaceOptions.FranchiseCode)
                    },
                    DealerCountryCode = countryCode,
                    DealerCountryCodeSpecified = true,
                    PartyID = new IdentifierType
                    {
                        Value = DspConstants.Identifier
                    }
                },
                CreationDateTime = GetFormattedDateTimeOffset(DateTime.UtcNow.ToLocalTime()),
                BODID = new IdentifierType
                {
                    Value = Guid.NewGuid().ToString()
                }
            },
            ProcessRepairOrderDataArea = _volvoEvent.Status == Canceled.DESCRIPTION ? canceledRODataArea : new ProcessRepairOrderDataAreaType
            {
                Process = ProcessTypeMapper.Map(),
                RepairOrder = new[] {
                    new RepairOrderType {
                        RepairOrderHeader = new RepairOrderHeaderType {
                            SecondaryReferenceNumberString = string.IsNullOrWhiteSpace(_repairOrder.OriginalRepairOrderNumber)
                                ? null
                                : _repairOrder.OriginalRepairOrderNumber,
                            PrimaryDriver = PrimaryDriverMapper.From(_repairOrder.Driver, _settings.RegionSettings),
                            OrderNotes = new TextType {
                                Value = "U"
                            },
                            ServiceAdvisorParty = ServiceAdvisorPartyMapper.Map(_repairOrder.ServiceWriterName, _settings),
                            DocumentDateTime = GetFormattedDateTimeOffset(_volvoEvent.Timestamp),
                            DocumentDateTimeSpecified = true,
                            OrderInternalNotes = new TextType {
                                Value = ORDER_INTERNAL_NOTES
                            },
                            DepartmentType = new TextType {
                                Value = string.IsNullOrWhiteSpace(_repairOrder?.BillingCustomer?.CustomerTypeCode)
                                    ? CustomerTypeCodes.VolvoServiceDefault
                                    : _repairOrder.BillingCustomer.CustomerTypeCode
                            },
                            DocumentIdentificationGroup = DocumentIdentificationGroupMapper.Map(_repairOrder.RepairOrderNumber, _repairOrder.CustomerPONumber),
                            OwnerParty = OwnerParty.FetchOrNull(_repairOrder),
                            RepairOrderVehicleLineItem = new RepairOrderVehicleLineItemType {
                                Vehicle = new VehicleABIEType {
                                    Model = string.IsNullOrWhiteSpace(_repairOrder.Vehicle.Model)
                                        ? null
                                        : new TextType {
                                            Value = _repairOrder.Vehicle.Model.MaxLength(MODEL_MAX_LENGTH)
                                        },
                                    ModelYear = string.IsNullOrWhiteSpace(_repairOrder.Vehicle.Year)
                                        ? null
                                        : _repairOrder.Vehicle.Year,
                                    MakeString = string.IsNullOrWhiteSpace(_repairOrder.Vehicle.Make)
                                        ? null
                                        : _repairOrder.Vehicle.Make.MaxLength(MAKE_STRING_MAX_LENGTH),
                                    VehicleID = new IdentifierType {
                                        Value = VinFormatter.Format(_repairOrder.Vehicle.VIN)
                                    }
                                },
                                FleetAccount = string.IsNullOrEmpty(_repairOrder.Vehicle.UnitNumber) ? null : new FleetAccountType {
                                    FleetVehicleID = new IdentifierType {
                                        Value = _repairOrder.Vehicle.UnitNumber
                                    }
                                }
                            },
                            InDistanceMeasure = new LengthMeasureType {
                                Value = _repairOrder.MeterReading != null
                                    ? Math.Truncate(_repairOrder.MeterReading.OrMax(MAX_ODOMETER).GetValueOrDefault())
                                    : DEFAULT_ODOMETER_READING,
                                unitCode = FetchMeterType()
                            },
                            OutDistanceMeasure = new LengthMeasureType {
                                Value = _repairOrder.MeterReading != null
                                    ? Math.Truncate(_repairOrder.MeterReading.OrMax(MAX_ODOMETER).GetValueOrDefault())
                                    : DEFAULT_ODOMETER_READING,
                                unitCode = FetchMeterType()
                            },
                            Price = new[] {
                                new PriceABIETypeStar {
                                    ChargeAmount = new AmountType {
                                        Value = IsSplitRO()
                                        ? 0m
                                        : FetchNetCustomerPaidAmount().WithDecimalImplied(),
                                        currencyID = _settings.RegionSettings.CurrencyCode
                                    },
                                    PriceCode = PriceEnumeratedType.Total,
                                    PriceCodeSpecified = true
                                }
                            },
                            Tax = new[] {
                                new TaxType {
                                    TaxAmount = new AmountType {
                                        Value = _repairOrder.ROTaxAmountTotal.GetValueOrDefault().WithDecimalImplied(),
                                        currencyID = _settings.RegionSettings.CurrencyCode
                                    },
                                    TaxTypeCode = TaxTypeEnumeratedType.Total
                                }
                            },
                            RepairOrderStatus = new[] {
                                new ServiceContractStatusType {
                                    StatusText = new TextType {
                                        Value = _volvoEvent.Status
                                    }
                                }
                            },
                            PromisedRepairCompletionDateTime = GetFormattedDateTimeOffset(_repairOrder.PromisedDate),
                            PromisedRepairCompletionDateTimeSpecified = _repairOrder.PromisedDate != null,
                            RepairOrderOpenedDateTime = GetFormattedDateTimeOffset(_repairOrder.OpenDate),
                            RepairOrderOpenedDateTimeSpecified = _repairOrder.OpenDate != null,
                            RepairOrderCompletedDateTime = GetFormattedDateTimeOffset(_volvoEvent.Timestamp),
                            RepairOrderCompletedDateTimeSpecified = CompletedDateRequired(_volvoEvent),
                            AppointmentScheduledDateTime = GetFormattedDateTimeOffset(_repairOrder.Appointment?.ArrivalDateTime),
                            AppointmentScheduledDateTimeSpecified = NotNullOrDefault(_repairOrder.Appointment?.ArrivalDateTime),
                            RepairOrderInvoiceDateTime = GetFormattedDateTimeOffset(_repairOrder.InvoiceDate),
                            RepairOrderInvoiceDateTimeSpecified = _repairOrder.InvoiceDate != null,
                            DateAppointmentInitiated = GetFormattedDateTimeOffset(_repairOrder.Appointment?.AppointmentMadeDateTime),
                            DateAppointmentInitiatedSpecified = NotNullOrDefault(_repairOrder.Appointment?.AppointmentMadeDateTime)
                        },
                        Job = FetchJobs()
                    }
                }
            }
        };
    }

    private static bool CompletedDateRequired(VolvoEvent volvoEvent)
    {
        return volvoEvent.Matches(new Closed()) || volvoEvent.Matches(new Invoiced());
    }

    private decimal FetchNetCustomerPaidAmount()
    {
        return _repairOrder.Tasks.Where(repairOrderTask => string.IsNullOrWhiteSpace(repairOrderTask.AlternateBillingCustomerKey))
            .Sum(repairOrderTask => repairOrderTask.Parts.Sum(part => part.ExtendedPrice.GetValueOrDefault()) +
                                    repairOrderTask.Parts.Sum(part => part.CoreExtendedPrice.GetValueOrDefault()) +
                                    repairOrderTask.LaborEntries.Sum(labor => labor.ExtendedPrice.GetValueOrDefault()) +
                                    repairOrderTask.MiscCharges.Sum(misc => misc.ExtendedPrice.GetValueOrDefault()) +
                                    repairOrderTask.TaskTaxTotal);
    }

    private JobType[] FetchJobs()
    {
        var serviceLabor = new ServiceLabor(_repairOrder, _settings);
        var servicePart = new ServicePart(_settings, _repairOrder.TimeZone);
        var serviceDepartment = ServiceDepartments.GetDepartmentCode(_repairOrder.DepartmentID, _settings.InterfaceOptions);

        var jobs = new List<JobType>();
        bool isSplitRo = IsSplitRO();

        foreach (var job in _repairOrder.Tasks)
        {
            // Don't send tasks with status of "Quote Declined"
            if (job.RepairTaskStatus.EqualsIgnoreCase(RepairOrderTaskStatus.QUOTE_DECLINED))
                continue;

            bool altCustKeyWillNotSplitRo = job.AlternateBillingCustomerKey == _repairOrder.BillingCustomer.CustomerKey;
            var opId = serviceLabor.FetchOperationIdFor(job);
            if (isSplitRo || opId.Value == OperationIds.Customer || altCustKeyWillNotSplitRo)
            {
                var newJob = new JobType()
                {
                    JobNumberString = new JobNumberSchemeIDType
                    {
                        Value = "00"
                    },
                    OperationID = opId,
                    OperationName = new TextType
                    {
                        Value = "Operation"
                    },
                    CodesAndCommentsExpanded = new CodesAndCommentsExpandedType(),
                    ServiceTechnicianParty = new[] {
                        new PartyABIEType()
                    },
                    ServiceParts = servicePart.Fetch(job),
                    ServiceLabor = serviceLabor.Fetch(job, serviceDepartment),
                    TechnicianStartsJobSignInDateTime = GetFormattedDateTimeOffset(job.LaborEntries.Select(labor => labor.DateTimeIn).Min().GetValueOrDefault()),
                    TechnicianStartsJobSignInDateTimeSpecified = job.LaborEntries.Any(),
                    TechnicianFinishesJobSignOutDateTime = GetFormattedDateTimeOffset(job.LaborEntries.Select(labor => labor.DateTimeOut).Max().GetValueOrDefault()),
                    TechnicianFinishesJobSignOutDateTimeSpecified = job.LaborEntries.Any() && RepairOrderTaskStatus.CLOSED.EqualsIgnoreCase(job.RepairTaskStatus)
                };
                jobs.Add(newJob);
            }
        }
        return jobs.ToArray();
    }

    private LengthUnitsContentType FetchMeterType()
    {
        return _repairOrder.MeterType.EqualsIgnoreCase(FusionMeterType.Miles) || string.IsNullOrWhiteSpace(_repairOrder.MeterType)
            ? LengthUnitsContentType.mile
            : LengthUnitsContentType.kilometer;
    }

    /// <summary> Checks if this RepairOrder has a reference to an OriginalRepairOrder</summary>
    private bool IsSplitRO()
    {
        return !string.IsNullOrWhiteSpace(_repairOrder.OriginalRepairOrderNumber) &&
               !string.IsNullOrWhiteSpace(_repairOrder.BillingCustomer.CustomerKey);
    }

    private ProcessRepairOrderDataAreaType GetStrippedDownDataAreaForCancelled()
    {
        var canceledRODataArea = new ProcessRepairOrderDataAreaType
        {
            Process = ProcessTypeMapper.Map(),
            RepairOrder = new[] {
                new RepairOrderType {
                    RepairOrderHeader = new RepairOrderHeaderType {
                        SecondaryReferenceNumberString = string.IsNullOrWhiteSpace(_repairOrder.OriginalRepairOrderNumber)
                            ? null
                            : _repairOrder.OriginalRepairOrderNumber,
                        OrderNotes = new TextType {
                            Value = "U"
                        },
                        DocumentDateTime = GetFormattedDateTimeOffset(_volvoEvent.Timestamp),
                        DocumentDateTimeSpecified = true,
                        OrderInternalNotes = new TextType {
                            Value = ORDER_INTERNAL_NOTES
                        },
                        DepartmentType = new TextType {
                            Value = string.IsNullOrWhiteSpace(_repairOrder?.BillingCustomer?.CustomerTypeCode)
                                ? CustomerTypeCodes.VolvoServiceDefault
                                : _repairOrder.BillingCustomer.CustomerTypeCode
                        },
                        DocumentIdentificationGroup = DocumentIdentificationGroupMapper.Map(_repairOrder.RepairOrderNumber, _repairOrder.CustomerPONumber),
                        RepairOrderVehicleLineItem = new RepairOrderVehicleLineItemType {
                            Vehicle = new VehicleABIEType {
                                VehicleID = new IdentifierType {
                                    Value = VinFormatter.Format(_repairOrder.Vehicle.VIN)
                                }
                            },
                        },
                        InDistanceMeasure = new LengthMeasureType {
                            Value = _repairOrder.MeterReading != null
                                ? Math.Truncate(_repairOrder.MeterReading.OrMax(MAX_ODOMETER).GetValueOrDefault())
                                : DEFAULT_ODOMETER_READING,
                            unitCode = FetchMeterType()
                        },
                        RepairOrderStatus = new[] {
                            new ServiceContractStatusType {
                                StatusText = new TextType {
                                    Value = _volvoEvent.Status
                                }
                            }
                        },
                        PromisedRepairCompletionDateTimeSpecified = false,
                        RepairOrderOpenedDateTime = GetFormattedDateTimeOffset(_repairOrder.OpenDate.GetValueOrDefault()),
                        RepairOrderOpenedDateTimeSpecified = _repairOrder.OpenDate != null,
                        RepairOrderCompletedDateTimeSpecified = false,
                        AppointmentScheduledDateTimeSpecified = false,
                        RepairOrderInvoiceDateTimeSpecified = false,
                        DateAppointmentInitiatedSpecified = false
                    }
                }
            }
        };
        return canceledRODataArea;
    }

    private XmlSerializableDateTimeOffset GetFormattedDateTimeOffset(DateTime? date)
    {
        return XmlSerializableDateTimeOffset.GetFormattedDateTimeOffset(date, _repairOrder.TimeZone);
    }

    private static bool NotDefault(DateTime dateTime) =>
        dateTime != default;

    private static bool NotNullOrDefault(DateTime? dateTime) =>
        dateTime != null && NotDefault(dateTime.Value);
}