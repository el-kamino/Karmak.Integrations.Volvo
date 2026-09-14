using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Core.Gen.Common.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.Gen.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.Gen.V6_0_0;
using Karmak.Integrations.Volvo.React.Core.RepairOrders.V6_0_0;
using Karmak.Integrations.Volvo.React.Mappers.RepairOrder._6_0_0;
using Karmak.Integrations.Volvo.React.Mappers.Shared;
using Karmak.Integrations.Volvo.React.Mappers.Shared._6_0_0;
using Karmak.Integrations.Volvo.React.Settings;
using Karmak.Integrations.Volvo.React.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.RepairOrders.V6_0_0
{
    internal class VolvoRepairOrderBuilder
    {
        private const decimal _defaultOdometer = 1m;
        private const decimal _maxOdometer = 999999m;
        private const string _resource = "urn:volvo/dealer/RepairOrder/v6.0";

        private readonly RepairOrderSnapshot _repairOrder;
        private readonly VolvoSettings _volvoSettings;
        private readonly VolvoRepairOrderState _roState;
        private readonly bool _isHistorical;

        public VolvoRepairOrderBuilder(VolvoSettings volvoSettings, RepairOrderSnapshot repairOrder, VolvoRepairOrderState roState, bool isHistorical)
        {
            _volvoSettings = volvoSettings;
            _repairOrder = repairOrder;
            _roState = roState;
            _isHistorical = isHistorical;
        }

        public string BuildSerializedPayload()
        {
            return SerializeROPayload(BuildPayload());
        }

        public VolvoRepairOrderTransmission BuildPayload()
        {
            return new VolvoRepairOrderTransmission
            {
                header = BuildROHeader(),
                sender = BuildROSender(),
                payload = BuildROPayload()
            };
        }

        public bool HasNoTasksAndIsNotDeletedOrClosed(VolvoRepairOrderTransmission payload)
        {
            //Volvo hack... we do not send tasks for "non Volvo warranty" so if we have no tasks and the RO is not canceled or closed,
            //  we need to mark this payload as empty so that we can handle it properly (not send it).
            //Note: ROs that are canceled, but have not been reported before are skipped earlier in the process (RepairOrderTriggersSend),
            //  so if we get to this stage and we have a canceled/closed RO with no tasks, it means that we have sent tasks in a previous update,
            //  so we want to send the Cancel/Closed
            return !payload.payload[0].job.Any() && payload.payload[0].statusText != VolvoRepairOrderStatus.CANCELED && payload.payload[0].statusText != VolvoRepairOrderStatus.CLOSED;
        }

        public bool HasNoTasks(VolvoRepairOrderTransmission payload)
        {
            return !payload.payload[0].job.Any();
        }

        public void RemoveAllTasksIfNeeded(ref VolvoRepairOrderTransmission payload)
        {
            //Volvo also requires that if the RO is in ARRIVED or CANCELED status, that we do not send any tasks,
            //  so we need to remove them from the payload (we built them empty in VolvoRepairOrderJobBuilder)
            if (payload.payload[0].statusText == VolvoRepairOrderStatus.ARRIVED || payload.payload[0].statusText == VolvoRepairOrderStatus.CANCELED)
            {
                payload.payload[0].job = null;
            }
        }
        public string SerializeROPayload(VolvoRepairOrderTransmission payload)
        {
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true
            };
            return JsonSerializer.Serialize(payload, options);
        }

        public string BuildPhantomVehicleArrival(string realPayloadJson)
        {
            //Using a copy of our RO, build a phantom payload with the following changes:
            //Set the RO Status to VEHICLE_ARRIVAL
            //Must have NO tasks, remove them 
            VolvoRepairOrderTransmission phantomPayload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(realPayloadJson);
            phantomPayload.payload[0].statusText = VolvoRepairOrderStatus.ARRIVED;
            phantomPayload.payload[0].job = null;
            return SerializeROPayload(phantomPayload);
        }
        public string BuildPhantomTechnicianAllocated(string realPayloadJson)
        {
            //Using a copy of our RO, build a phantom payload with the following changes:
            //Set the RO Status to TECHNICIAN_ALLOCATED
            //All tasks MUST be WORK_NOT_STARTED, update them
            VolvoRepairOrderTransmission phantomPayload = JsonSerializer.Deserialize<VolvoRepairOrderTransmission>(realPayloadJson);
            phantomPayload.payload[0].statusText = VolvoRepairOrderStatus.TECH_ALLOCATED;
            phantomPayload.payload[0].job.ForEach(job => job.jobStatusCode = VolvoRepairOrderTaskStatus.WORK_NOT_STARTED);
            return SerializeROPayload(phantomPayload);
        }

        private VolvoHeader BuildROHeader()
        {
            var countryCode = CountryID.ToAlpha3(_volvoSettings.RegionSettings.CountryCode) ?? DspConstants.DefaultCountryCode;

            return new VolvoHeader
            {
                partyId = DspConstants.Identifier,
                siteCode = $"{countryCode}{_volvoSettings.InterfaceOptions.PaCode}",
                resource = _resource,
                requestId = Guid.NewGuid().ToString(),
                dealerCountryCode = countryCode,
                dealerNumberId = _volvoSettings.InterfaceOptions.PaCode,
                roTotal = 1
            };
        }

        private VolvoSender BuildROSender()
        {
            return new VolvoSender
            {
                taskId = _isHistorical ? TransmissionType.Historical : TransmissionType.New,
                areaNumber = FranchiseCodeType.Fetch(_volvoSettings.InterfaceOptions.FranchiseCode),
                languageCode = _volvoSettings.RegionSettings.LanguageCode,
                currencyId = _volvoSettings.RegionSettings.CurrencyCode,
                systemVersion = _repairOrder.DatabaseVersion.MaxLength(9),
                creationDateTime = DateTimeUtility.ApplyCustomTimeZone(DateTime.UtcNow.ToLocalTime(), _repairOrder.TimeZone)
            };
        }

        private List<VolvoPayload> BuildROPayload()
        {
            bool isSecondaryRO = !string.IsNullOrWhiteSpace(_repairOrder.OriginalRepairOrderNumber);
            var partsBuilder = new VolvoRepairOrderPartsBuilder(_repairOrder.TimeZone);
            var laborBuilder = new VolvoRepairOrderLaborBuilder(_repairOrder, _volvoSettings);
            var jobBuilder = new VolvoRepairOrderJobBuilder(_repairOrder, _volvoSettings, _roState, partsBuilder, laborBuilder);
            var currentDateTime = DateTime.UtcNow.ToLocalTime();

            return new List<VolvoPayload>
            {
                new VolvoPayload
                {
                    documentDateTime = DateTimeUtility.ApplyCustomTimeZone(currentDateTime, _repairOrder.TimeZone),
                    documentId = _repairOrder.RepairOrderNumber.MaxLength(15),
                    customerPurchaseOrderNumber = _repairOrder.CustomerPONumber.MaxLength(20),
                    secondaryReferenceNumberString = string.IsNullOrWhiteSpace(_repairOrder.OriginalRepairOrderNumber) ? null : _repairOrder.OriginalRepairOrderNumber.MaxLength(15),
                    statusText = _roState.RepairOrderStatusState.VolvoRoStatus,
                    roTypeCode = _repairOrder.RepairOrderStatus.MaxLength(50),
                    roType = _repairOrder.RepairOrderStatus.MaxLength(50),
                    promisedRepairCompletionDateTime = DateTimeUtility.ApplyCustomTimeZone(_repairOrder.PromisedDate, _repairOrder.TimeZone),
                    appointmentScheduledDateTime = DateTimeUtility.ApplyCustomTimeZone(_repairOrder.Appointment?.ArrivalDateTime, _repairOrder.TimeZone),
                    vehicleHatNumber = _repairOrder.KeyTag.MaxLength(10),
                    repairOrderOpenedDateTime = DateTimeUtility.ApplyCustomTimeZone(_roState.OriginalRoOpenDate, _repairOrder.TimeZone),
                    repairOrderCompletedDateTime = DateTimeUtility.ApplyCustomTimeZone(GetCompletionDate(currentDateTime), _repairOrder.TimeZone),
                    repairOrderInvoiceDateTime = DateTimeUtility.ApplyCustomTimeZone(_repairOrder.InvoiceDate, _repairOrder.TimeZone),
                    inDistanceMeasure = BuildInMeter(_repairOrder),
                    departmentType = _repairOrder.BillingCustomer.CustomerTypeCode.DefaultIfNullOrEmpty(CustomerTypeCodes.VolvoServiceDefault),
                    dealerCustomerTypeCode = _repairOrder.BillingCustomer.IndustryType.DefaultIfNullOrEmpty(CustomerTypeCodes.DealerCustomerTypeCodeDefault).MaxLength(50),
                    dealerCustomerTypeName = _repairOrder.BillingCustomer.IndustryType.DefaultIfNullOrEmpty(CustomerTypeCodes.DealerCustomerTypeNameDefault).MaxLength(50),
                    taxAmount = _repairOrder.ROTaxAmountTotal.GetValueOrDefault().WithDecimalImplied(),
                    serviceAdvisorParty = BuildServiceAdvisorParty(_repairOrder.ServiceWriterName, _volvoSettings.InterfaceOptions?.OemUserMappings),
                    customerParty = BuildCustomerParties(_repairOrder, _volvoSettings.RegionSettings.LanguageCode),
                    vehicle = BuildVehicle(_repairOrder.Vehicle),
                    price = BuildPrice(),
                    job = jobBuilder.BuildVolvoJobs()
                }
            };
        }

        private DateTime? GetCompletionDate(DateTime documentDate)
        {
            if (_roState.RepairOrderStatusState.VolvoRoStatus != VolvoRepairOrderStatus.CLOSED)
            {
                return null;
            }
                
            if (_repairOrder.InvoiceDate.HasValue && _repairOrder.InvoiceDate > _repairOrder.OpenDate)
            {
                return _repairOrder.InvoiceDate.Value;
            }
            else if (_repairOrder.Tasks != null && _repairOrder.Tasks.Any())
            {
                var latestTaskCompletion = _repairOrder.Tasks.Max(task => task.LaborEntries.Max(labor => labor.DateTimeOut));
                if (latestTaskCompletion.HasValue && latestTaskCompletion > _repairOrder.OpenDate)
                {
                    return latestTaskCompletion;
                }
            }
            return documentDate;

        }

        private VolvoPrice BuildPrice()
        {
            decimal? roTotal = _repairOrder.Tasks
                .Where((RepairOrderTask repairOrderTask) =>
                    string.IsNullOrWhiteSpace(repairOrderTask.AlternateBillingCustomerKey) ||
                    repairOrderTask.AlternateBillingCustomerKey == _repairOrder.BillingCustomer.CustomerKey)
                .Sum((RepairOrderTask repairOrderTask) =>
                    (repairOrderTask.Parts?.Sum((Part part) => part.ExtendedPrice.GetValueOrDefault()) ?? 0) +
                    (repairOrderTask.Parts?.Sum((Part part) => part.CoreExtendedPrice.GetValueOrDefault()) ?? 0) +
                    (repairOrderTask.LaborEntries?.Sum((Labor labor) => labor.ExtendedPrice.GetValueOrDefault()) ?? 0) +
                    (repairOrderTask.MiscCharges?.Sum((MiscCharge misc) => misc.ExtendedPrice.GetValueOrDefault()) ?? 0) +
                    repairOrderTask.TaskTaxTotal);

            return new VolvoPrice { totalCustomerRepairOrderPrice = roTotal.GetValueOrDefault().WithDecimalImplied() };
        }

        private static VolvoDistanceMeasure BuildInMeter(RepairOrderSnapshot repairOrder)
        {
            return new VolvoDistanceMeasure
            {
                value = repairOrder.MeterReading.HasValue ? Math.Min(Math.Truncate(repairOrder.MeterReading.Value), _maxOdometer) : _defaultOdometer,
                unit = repairOrder.MeterType.EqualsIgnoreCase(FusionMeterType.Kilometers) ? VolvoMeterType.Kilometer : VolvoMeterType.Mile
            };
        }

        private static List<VolvoPartyIdentifier> BuildServiceAdvisorParty(string serviceWriterName, IDictionary<string, string> oemUserMappings)
        {
            if (string.IsNullOrEmpty(serviceWriterName))
                return null;

            if (oemUserMappings != null && oemUserMappings.Any())
            {
                var oemUserMappingsImmutable = oemUserMappings.ToImmutableDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
                serviceWriterName = oemUserMappingsImmutable.TryGetValue(serviceWriterName.Replace(".", string.Empty), out string value)
                    ? value : serviceWriterName;
            }

            return new List<VolvoPartyIdentifier>
            {
                new()
                {
                    id = serviceWriterName?.WafSanitize().MaxLength(10) ?? "",
                    type = CustomerTypeCodes.PartyIdentifierLocal,
                }
            };
        }

        private static VolvoVehicle BuildVehicle(Vehicle vehicle)
        {
            //vin must always be 17 chars - default and pad
            var vin = string.IsNullOrWhiteSpace(vehicle.VIN) ? "UNKNOWN" : vehicle.VIN;
            vin = vin.PadLeft(17, '0').MaxLength(17);

            int modelYear = 0;
            if (!string.IsNullOrWhiteSpace(vehicle.Year))
            {
                _ = int.TryParse(vehicle.Year, out modelYear);
            }

            return new VolvoVehicle()
            {
                modelYear = modelYear,
                makeString = vehicle.Make.WafSanitize().MaxLength(30),
                model = vehicle.Model.WafSanitize().MaxLength(30),
                vehicleId = vin,
                fleetVehicleId = vehicle.UnitNumber.WafSanitize().MaxLength(30)
            };
        }

        private static List<VolvoCustomerParty> BuildCustomerParties(RepairOrderSnapshot repairOrder, string languageCode)
        {
            var customerParties = new List<VolvoCustomerParty>();
            var ownerPartyAddress = repairOrder.Addresses?.FirstOrDefault(address => address.AddressType == AddressTypes.ShipTo && address.EntityType == AddressTypes.RepairOrderOwning);
            var driverPartyAddress = repairOrder.Driver?.Addresses?.FirstOrDefault();

            var ownerParty = VolvoCustomerPartyBuilder.BuildOwnerParty(repairOrder.OwningCustomer, ownerPartyAddress, languageCode);
            if (ownerParty != null)
            {
                customerParties.Add(ownerParty);
            }
            var driverParty = VolvoCustomerPartyBuilder.BuildDriverParty(repairOrder.Driver, driverPartyAddress, languageCode);
            if (driverParty != null)
            {
                customerParties.Add(driverParty);
            }
            return customerParties;
        }
    }
}
