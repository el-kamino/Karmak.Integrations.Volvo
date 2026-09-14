using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Contracts;
using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.Warranty.Contracts;
using Karmak.Integrations.Volvo.Warranty.Contracts.Constants;
using Karmak.Integrations.Volvo.Warranty.Converters;
using ClaimAddress = Karmak.Integrations.Volvo.Warranty.Contracts.Address;
using ClaimCustomer = Karmak.Integrations.Volvo.Warranty.Contracts.Customer;
using ClaimDriver = Karmak.Integrations.Volvo.Warranty.Contracts.Driver;
using ClaimJob = Karmak.Integrations.Volvo.Warranty.Contracts.Job;
using ClaimUnit = Karmak.Integrations.Volvo.Warranty.Contracts.Unit;
using ClaimVehicle = Karmak.Integrations.Volvo.Warranty.Contracts.Vehicle;
using RepairOrderAddress = Karmak.Integrations.Volvo.React.Contracts.Common.Address;
using RepairOrderVehicle = Karmak.Integrations.Volvo.React.Contracts.Common.Vehicle;

namespace Karmak.Integrations.Volvo.Warranty.Mapping
{
    public class RepairOrderToClaimMapper : IRepairOrderToClaimMapper
    {
        private static readonly Regex TrimSpaces = new Regex(@"\s\s+");

        public Claim Map(RepairOrderSnapshot source, VolvoSettings settings, string customerRegion)
        {
            var subletLaborIdentifiers = settings?.InterfaceOptions?.SubletLaborCharges ?? Array.Empty<int>();

            var claim = new Claim
            {
                Id = Guid.NewGuid().ToString(),
                Type = ClaimTypes.VehicleCoverages,
                Status = ClaimStatus.New,
                Oem = new Company { Name = ConstantSettings.VolvoCompanyName },
                Identifier = source.RepairOrderNumber,
                InAppeal = false,
                Customer = MapCustomer(source),
                Driver = MapDriver(source.Driver),
                RepairOrder = MapRepairOrder(source, settings, subletLaborIdentifiers),
                Unit = MapUnit(source, customerRegion)
            };

            return claim;
        }

        private RepairOrder MapRepairOrder(RepairOrderSnapshot source, VolvoSettings settings, IEnumerable<int> subletLaborIdentifiers)
        {
            return new RepairOrder
            {
                Identifier = source.OriginalRepairOrderNumber ?? source.RepairOrderNumber,
                ExternalIdentifiers = source.OriginalRepairOrderExternalIdentifiers,
                SecondaryIdentifier = source.RepairOrderNumber,
                SecondaryExternalIdentifiers = source.ExternalIdentifiers,
                Advisor = ResolveAdvisor(source, settings),
                OpenedDate = Conversions.NormalizeTimeOfDateTime.Convert(source.OpenDate),
                CompletedDate = Conversions.NormalizeTimeOfDateTime.Convert(source.CompletionDate),
                InvoicedDate = Conversions.NormalizeTimeOfDateTime.Convert(source.InvoiceDate),
                InvoiceIdentifier = source.InvoiceIdentifier,
                Jobs = source.Tasks?.Select(task => MapJob(task, settings, subletLaborIdentifiers)).ToList()
            };
        }

        private ClaimJob MapJob(RepairOrderTask source, VolvoSettings settings, IEnumerable<int> subletLaborIdentifiers)
        {
            var job = new ClaimJob
            {
                Identifier = source.TaskNumber?.ToString(),
                CustomerNotes = source.ComplaintNotes,
                TechnicianNotes = source.TechnicianNotes,
                InternalDealerNotes = source.InternalDealerNotes,
                PartExpenses = ResolvePartExpenses(source, settings),
                LaborExpenses = ResolveLaborExpenses(source, settings, subletLaborIdentifiers),
                MiscellaneousExpenses = ResolveMiscellaneousExpenses(source, settings, subletLaborIdentifiers)
            };

            job.Expenses = job.PartExpenses.Concat<Expense>(job.LaborExpenses).Concat(job.MiscellaneousExpenses).ToList();
            return job;
        }

        private ClaimCustomer MapCustomer(RepairOrderSnapshot source)
        {
            var contact = source.OwningCustomer?.Contacts?.FirstOrDefault();
            var shippingAddress = source.Addresses?.FirstOrDefault(a =>
                a.AddressType == AddressType.SHIP_TO &&
                a.EntityType == ConstantSettings.OwningCustomerAddressEntityType);

            return new ClaimCustomer
            {
                Identifier = source.OwningCustomer?.CustomerKey,
                CompanyName = source.OwningCustomer?.CompanyName,
                ExternalIdentifiers = source.OwningCustomer?.ExternalIdentifiers,
                FirstName = contact?.FirstName,
                MiddleName = contact?.MiddleName?.First().ToString(),
                LastName = contact?.LastName,
                EmailAddress = contact?.Email,
                WorkPhoneNumber = contact?.Phones?.Where(p => p.Type == PhoneType.WORK).Select(p => p.Number).FirstOrDefault(),
                CellPhoneNumber = contact?.Phones?.Where(p => p.Type == PhoneType.CELL).Select(p => p.Number).FirstOrDefault(),
                PhysicalAddress = shippingAddress != null ? MapAddress(shippingAddress) : null
            };
        }

        private static ClaimAddress MapAddress(RepairOrderAddress source)
        {
            return new ClaimAddress
            {
                StreetAddress = source.Street1,
                SecondaryAddress = source.Street2,
                City = source.City,
                Region = source.Region,
                County = source.County,
                PostalCode = source.PostalCode,
                Country = Enum.TryParse(source.CountryCode, out CountryCode result) ? result : CountryCode.US
            };
        }

        private static ClaimDriver MapDriver(Contact source)
        {
            if (source == null) return null;

            var middleName = source.MiddleName?.Trim() ?? string.Empty;
            var middleInitial = string.IsNullOrEmpty(middleName) ? string.Empty : middleName.Substring(0, 1);
            var fullName = TrimSpaces.Replace(
                $"{source.FirstName?.Trim()} {middleInitial} {source.LastName?.Trim()}", " ");

            return new ClaimDriver
            {
                Identifier = source.LicenseNumber,
                FullName = fullName
            };
        }

        private ClaimUnit MapUnit(RepairOrderSnapshot source, string customerRegion)
        {
            return new ClaimUnit
            {
                Identifier = source.Vehicle?.UnitNumber,
                Vehicle = MapVehicle(source.Vehicle, source.InServiceDate, customerRegion),
                MeterReadings = ResolveMeterReadings(source),
                ExternalIdentifiers = new List<ExternalIdentifier>
                {
                    new ExternalIdentifier
                    {
                        ID = source.Vehicle?.UnitInventoryIdentifier,
                        ExternalSourceType = "FUSION"
                    }
                }
            };
        }

        private static ClaimVehicle MapVehicle(RepairOrderVehicle source, DateTime? inServiceDate, string customerRegion)
        {
            if (source == null) return null;

            return new ClaimVehicle
            {
                Identifier = source.VIN,
                Make = source.Make,
                Model = source.Model,
                Year = source.Year,
                InServiceDate = inServiceDate ?? DateTime.MinValue,
                License = new License
                {
                    State = customerRegion?.ToString()
                }
            };
        }

        #region Resolvers

        private static User ResolveAdvisor(RepairOrderSnapshot source, VolvoSettings settings)
        {
            var oemUserMappings = (settings?.InterfaceOptions?.OemUserMappings
                ?? ImmutableDictionary<string, string>.Empty)
                .ToImmutableDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

            var strippedUsername = source.ServiceWriterName?.Replace(".", string.Empty) ?? string.Empty;

            var identifier = oemUserMappings.ContainsKey(strippedUsername)
                ? oemUserMappings[strippedUsername]
                : source.ServiceWriterName;

            return new User
            {
                Identifier = identifier,
                Username = source.ServiceWriterName
            };
        }

        private static IEnumerable<MeterReading> ResolveMeterReadings(RepairOrderSnapshot source)
        {
            var isMeterParseable = Enum.TryParse<UnitOfMeasureType>(source.MeterType, true, out var meterType);
            var odometerReading = new Measurement
            {
                UnitOfMeasure = isMeterParseable ? meterType : UnitOfMeasureType.Miles,
                Value = source.MeterReading ?? default(decimal)
            };
            var meterReadings = new List<MeterReading>
            {
                new MeterReading
                {
                    Type = MeterReadingType.Odometer,
                    ReadingIn = odometerReading,
                    ReadingOut = odometerReading
                }
            };

            if (source.Vehicle?.Engine?.Hours != null)
            {
                meterReadings.Add(new MeterReading
                {
                    Type = MeterReadingType.EngineHours,
                    ReadingIn = new Measurement
                    {
                        UnitOfMeasure = UnitOfMeasureType.Hours,
                        Value = source.Vehicle.Engine.Hours ?? default(decimal)
                    }
                });
            }

            return meterReadings;
        }

        private static IList<PartExpense> ResolvePartExpenses(RepairOrderTask source, VolvoSettings settings)
        {
            var dealerRegionCurrency =
                Enum.TryParse(settings?.RegionSettings?.CurrencyCode, out CurrencyCode currencyCode)
                    ? currencyCode
                    : ConstantSettings.CurrencyCodeDefault;

            return source.Parts
                .Where(part => part.PartType != "Core")
                .Select(part => new PartExpense
                {
                    Identifier = part.PartNumber,
                    Prefix = ExtractPartNumberPrefix(part.PartNumber),
                    Number = ExtractPartNumber(part.PartNumber),
                    Suffix = ExtractPartNumberSuffix(part.PartNumber),
                    Description = part.Description,
                    UnitPrice = new Money
                    {
                        Currency = dealerRegionCurrency,
                        Value = part.UnitPrice ?? 0m
                    },
                    UnitCost = new Money
                    {
                        Currency = dealerRegionCurrency,
                        Value = part.UnitCost ?? 0m
                    },
                    Quantity = new Quantity
                    {
                        Type = UnitOfMeasureType.Count,
                        Value = part.Quantity ?? 0m
                    },
                    CorePrice = CalculateCorePrice(source.Parts, part, dealerRegionCurrency),
                    ThirdPartyInvoiceIdentifier = part.InvoiceIdentifier
                }).ToList();
        }

        private static IList<LaborExpense> ResolveLaborExpenses(RepairOrderTask source, VolvoSettings settings, IEnumerable<int> subletLaborIdentifiers)
        {
            var dealerRegionCurrency =
                Enum.TryParse(settings?.RegionSettings?.CurrencyCode, out CurrencyCode currencyCode)
                    ? currencyCode
                    : ConstantSettings.CurrencyCodeDefault;

            var oemUserMappings = settings?.InterfaceOptions?.OemUserMappings
                ?? ImmutableDictionary<string, string>.Empty;

            var laborExpenses = source.LaborOperations?
                .Select(laborOperation => new LaborExpense
                {
                    Identifier = laborOperation.Code,
                    Description = laborOperation.Description,
                    Quantity = new Quantity
                    {
                        Type = UnitOfMeasureType.Hours,
                        Value = laborOperation.Hours
                    },
                    UnitPrice = new Money
                    {
                        Currency = dealerRegionCurrency,
                        Value = GetLaborPrice(source)
                    },
                    Technician = new User
                    {
                        Identifier = GetOemUser(oemUserMappings,
                            source.LaborEntries.FirstOrDefault()?.TechnicianUsername,
                            source.LaborEntries.FirstOrDefault()?.TechnicianNumber.ToString()),
                        Username = source.LaborEntries.FirstOrDefault()?.TechnicianUsername
                    }
                }) ?? Enumerable.Empty<LaborExpense>();

            var subletLabor = GetSubletLaborExpenses(source, subletLaborIdentifiers, dealerRegionCurrency);

            return laborExpenses.Concat(subletLabor).ToList();
        }

        private static IEnumerable<LaborExpense> GetSubletLaborExpenses(RepairOrderTask source, IEnumerable<int> subletLaborIdentifiers, CurrencyCode dealerRegionCurrency)
        {
            return source.MiscCharges?
                .Where(miscCharge => miscCharge?.MiscellaneousChargeID != null
                    && subletLaborIdentifiers.Contains((int)miscCharge.MiscellaneousChargeID))
                .Select(miscCharge => new LaborExpense
                {
                    Description = miscCharge.Description,
                    Quantity = new Quantity
                    {
                        Type = UnitOfMeasureType.Hours,
                        Value = miscCharge.Quantity ?? 0m
                    },
                    UnitPrice = new Money
                    {
                        Currency = dealerRegionCurrency,
                        Value = miscCharge.UnitPrice ?? 0m
                    },
                    Technician = null,
                    IsSublet = true,
                    ThirdPartyInvoiceIdentifier = miscCharge.InvoiceIdentifier
                }) ?? Enumerable.Empty<LaborExpense>();
        }

        private static IList<MiscellaneousExpense> ResolveMiscellaneousExpenses(RepairOrderTask source, VolvoSettings settings, IEnumerable<int> subletLaborIdentifiers)
        {
            var dealerRegionCurrency =
                Enum.TryParse(settings?.RegionSettings?.CurrencyCode, out CurrencyCode currencyCode)
                    ? currencyCode
                    : ConstantSettings.CurrencyCodeDefault;

            return source.MiscCharges?
                .Where(miscCharge => IsNotSubletLaborCharge(miscCharge, subletLaborIdentifiers) && IsNotVarianceType(miscCharge))
                .Select(miscCharge => new MiscellaneousExpense
                {
                    Identifier = miscCharge.Name,
                    Description = miscCharge.Description,
                    Quantity = new Quantity
                    {
                        Type = UnitOfMeasureType.Days,
                        Value = miscCharge.Quantity ?? 0m
                    },
                    SecondQuantity = new Quantity
                    {
                        Type = UnitOfMeasureType.Hours,
                        Value = 0m
                    },
                    UnitPrice = new Money
                    {
                        Currency = dealerRegionCurrency,
                        Value = miscCharge.UnitPrice ?? 0m
                    },
                    ThirdPartyInvoiceIdentifier = miscCharge.InvoiceIdentifier
                }).ToList() ?? new List<MiscellaneousExpense>();
        }

        #endregion

        #region Helper Methods

        private static decimal GetLaborPrice(RepairOrderTask repairOrderTask)
        {
            if (!repairOrderTask.LaborEntries.Any())
            {
                return repairOrderTask.OverrideLaborRate != default
                    ? repairOrderTask.OverrideLaborRate
                    : repairOrderTask.LaborRate;
            }
            return repairOrderTask.LaborEntries.FirstOrDefault()?.UnitPrice ?? 0m;
        }

        private static string GetOemUser(IDictionary<string, string> oemUserMappings, string username, string technicianNumber)
        {
            if (oemUserMappings.ContainsKey(username ?? ""))
                return oemUserMappings[username];
            if (oemUserMappings.ContainsKey(technicianNumber ?? ""))
                return oemUserMappings[technicianNumber];
            return username;
        }

        private static bool IsNotVarianceType(MiscCharge miscCharge)
        {
            return miscCharge?.MiscellaneousChargeType == null || miscCharge.MiscellaneousChargeType.ToLower() != "variance";
        }

        private static bool IsNotSubletLaborCharge(MiscCharge miscCharge, IEnumerable<int> subletLaborIdentifiers)
        {
            return miscCharge?.MiscellaneousChargeID == null || !subletLaborIdentifiers.Contains((int)miscCharge.MiscellaneousChargeID);
        }

        private static Money CalculateCorePrice(IEnumerable<Part> parts, Part currentPart, CurrencyCode dealerRegionCurrency)
        {
            var corePart = parts.FirstOrDefault(part => part.PartNumber == currentPart.CorePartNumber);
            return corePart == null
                ? new Money { Currency = CurrencyCode.USD, Value = 0m }
                : new Money
                {
                    Currency = dealerRegionCurrency,
                    Value = (currentPart.CoreExtendedPrice ?? 0m) + (corePart.ExtendedPrice ?? 0m)
                };
        }

        private const int MaxLength = 22;
        private const int StartIndex = 0;
        private const int PrefixLength = 6;
        private const int PartNumberLength = 8;
        private const int SuffixStartIndex = 14;

        private static string CleanPartNumber(string value) =>
            ChompAtMaxSize(RemoveSpecialCharacters(value), MaxLength);

        private static string RemoveSpecialCharacters(string value) =>
            string.IsNullOrEmpty(value)
                ? value
                : Regex.Replace(value, @"[^0-9a-zA-Z:]+", string.Empty);

        private static string ChompAtMaxSize(string value, int max) =>
            string.IsNullOrEmpty(value)
                ? value
                : value.Substring(StartIndex, Math.Min(value.Length, max));

        private static bool CanExtractPartNumberSegments(string partNumber)
        {
            var cleanPartNumber = CleanPartNumber(partNumber);
            return !string.IsNullOrEmpty(cleanPartNumber) && cleanPartNumber.Length > PartNumberLength;
        }

        private static string ExtractPartNumberPrefix(string partNumber)
        {
            var cleanPartNumber = CleanPartNumber(partNumber);
            return CanExtractPartNumberSegments(cleanPartNumber)
                ? cleanPartNumber.Substring(StartIndex, PrefixLength)
                : cleanPartNumber;
        }

        private static string ExtractPartNumber(string partNumber)
        {
            var cleanPartNumber = CleanPartNumber(partNumber);
            return CanExtractPartNumberSegments(cleanPartNumber) && cleanPartNumber.Length > PrefixLength
                ? cleanPartNumber.Substring(PrefixLength, Math.Min(PartNumberLength, cleanPartNumber.Length - PrefixLength))
                : string.Empty;
        }

        private static string ExtractPartNumberSuffix(string partNumber)
        {
            var cleanPartNumber = CleanPartNumber(partNumber);
            return CanExtractPartNumberSegments(cleanPartNumber) && cleanPartNumber.Length >= SuffixStartIndex
                ? cleanPartNumber.Substring(SuffixStartIndex)
                : string.Empty;
        }

        #endregion
    }
}
