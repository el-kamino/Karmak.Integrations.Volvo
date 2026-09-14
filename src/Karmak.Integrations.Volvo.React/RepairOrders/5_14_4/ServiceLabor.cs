using System;
using System.Collections.Generic;
using System.Linq;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.RepairOrders;
using Karmak.Integrations.Volvo.React.Utils;
using Karmak.Integrations.Volvo.React.RepairOrders.Extensions;
using Karmak.Integrations.Volvo.Common.Settings.Models;

namespace Karmak.Integrations.Volvo.React.Core.RepairOrders.V5_14_4
{
    public class ServiceLabor
    {
        private const string LOCAL = "local";
        private const string SUBLET_CODE = "Sublet";
        private const string ESP_DESCRIPTION_PREFIX = "DED-ESP";
        private const string WARRANTY_DESCRIPTION_PREFIX = "DED-WTY";

        private readonly RepairOrderSnapshot _repairOrderSnapshot;
        private readonly VolvoSettings _volvoSettings;
        public ServiceLabor(RepairOrderSnapshot repairOrderSnapshot, VolvoSettings volvoSettings)
        {
            _repairOrderSnapshot = repairOrderSnapshot;
            _volvoSettings = volvoSettings;
        }

        public ServiceLaborType[] Fetch(RepairOrderTask job, string serviceDepartment = ServiceDepartments.UNKNOWN_DEPARTMENT)
        {
            return new[] {
                new ServiceLaborType {
                    WorkshopCode = new TextType {
                        Value = serviceDepartment
                    },
                    LaborOperationID =
                        new IdentifierType {
                            Value = true == job.Warranty
                                ? job.SRTID
                                : job.RepairType
                        },
                    LaborOperationDescription = new[] {
                        new TextType {
                            Value = job.RepairTypeDescription
                        }
                    },
                    Pricing = FetchPricingAbieTypes(job),
                    LaborActualHoursNumeric = TotalLabourHours(job),
                    LaborActualHoursNumericSpecified = HasLabourHours(job),
                    ServiceTechnicianParty = FetchServiceTechnicianParty(job),
                    Sublet = FetchSublets(job)
                }
            };
        }

        private static PartyABIEType[] FetchServiceTechnicianParty(RepairOrderTask job)
        {
            if (!job.LaborEntries.Any())
            {
                return new PartyABIEType[0];
            }
            return new[] {
                new PartyABIEType {
                    Item = new PersonTypeStar {
                        ID = new[] {
                            new IdentifierType {
                                schemeID = LOCAL,
                                Value = job.LaborEntries[0].TechnicianNumber.ToString()
                            }
                        }
                    }
                }
            };
        }

        public IdentifierType FetchOperationIdFor(RepairOrderTask job)
        {
            if (string.IsNullOrWhiteSpace(job.AlternateBillingCustomerKey))
            {
                if (_repairOrderSnapshot.IsSecondaryRepairOrder())
                    return FetchOperationIdForSecondaryRO();
 
                return new IdentifierType
                {
                    Value = OperationIds.Customer
                };
            }

            if (IsWarranty(job))
            {
                return new IdentifierType
                {
                    Value = OperationIds.Warranty
                };
            }

            if (IsExtendedServicePlan(job))
            {
                return new IdentifierType
                {
                    Value = OperationIds.ExtendedServicePlan
                };
            }

            if (IsAfterWarrantyAssistance(job))
            {
                return new IdentifierType
                {
                    Value = OperationIds.AfterWarrantyAssistance
                };
            }

            if (IsInternalPolicyCustomer(job))
            {
                return new IdentifierType
                {
                    Value = OperationIds.InternalPolicyCustomer
                };
            }

            return new IdentifierType
            {
                Value = OperationIds.Unknown
            };
        }

        public IdentifierType FetchOperationIdForSecondaryRO()
        {
            if (string.IsNullOrWhiteSpace(_repairOrderSnapshot.BillingCustomer.CustomerKey))
            {
                return new IdentifierType
                {
                    Value = OperationIds.Customer
                };
            }

            if (IsWarranty())
            {
                return new IdentifierType
                {
                    Value = OperationIds.Warranty
                };
            }

            if (IsExtendedServicePlan())
            {
                return new IdentifierType
                {
                    Value = OperationIds.ExtendedServicePlan
                };
            }

            if (IsAfterWarrantyAssistance())
            {
                return new IdentifierType
                {
                    Value = OperationIds.AfterWarrantyAssistance
                };
            }

            if (IsInternalPolicyCustomer())
            {
                return new IdentifierType
                {
                    Value = OperationIds.InternalPolicyCustomer
                };
            }

            return new IdentifierType
            {
                Value = OperationIds.Unknown
            };
        }
        private PricingABIEType[] FetchPricingAbieTypes(RepairOrderTask job)
        {
            return new[] {
                new PricingABIEType {
                    Price = new[] {
                        new PriceABIETypeStar {
                            PriceCode = PriceEnumeratedType.Labor,
                            PriceCodeSpecified = true,
                            ChargeAmount = FetchChargeAmountForLabor(job)
                        }
                    }
                }
            };
        }

        private AmountType FetchChargeAmountForLabor(RepairOrderTask job)
        {
            if (IsWarranty(job))
            {
                var miscChargeForWarranty = MiscChargesForWarranty(job);
                var adjustedLaborChargeAfterWarranty = AdjustedLaborChargeAfterDeductible(job, miscChargeForWarranty);

                return new AmountType
                {
                    Value = adjustedLaborChargeAfterWarranty,
                    currencyID = _volvoSettings.RegionSettings.CurrencyCode
                };
            }

            if (IsExtendedServicePlan(job))
            {
                var miscChargeForExtendedServicePlan = MiscChargesForExtendedServicePlan(job);
                var adjustedLaborChargeAfterESP = AdjustedLaborChargeAfterDeductible(job, miscChargeForExtendedServicePlan);

                return new AmountType
                {
                    Value = adjustedLaborChargeAfterESP,
                    currencyID = _volvoSettings.RegionSettings.CurrencyCode
                };
            }

            return new AmountType
            {
                Value = job.LaborEntries.Sum(labor => labor.ExtendedPrice.GetValueOrDefault()).WithDecimalImplied(),
                currencyID = _volvoSettings.RegionSettings.CurrencyCode
            };
        }

        private SubletType[] FetchSublets(RepairOrderTask job)
        {
            return job.MiscCharges.Select(sublet => new SubletType
            {
                Pricing = new[] {
                    new PricingABIEType {
                        Price = new[] {
                            new PriceABIETypeStar {
                                PriceCode = FetchSubletPriceCode(sublet.MiscellaneousChargeID.GetValueOrDefault()),
                                PriceCodeSpecified = true,
                                ChargeAmount = FetchSubletChargeAmount(sublet)
                            }
                        }
                    }
                },
                SubletCode = new CodeType
                {
                    Value = SUBLET_CODE
                },
                AuthorizationNumberString = string.Empty,
                SubletWorkDescription = FetchSubletDescription(sublet)
            }).ToArray();
        }

        private AmountType FetchSubletChargeAmount(MiscCharge sublet)
        {
            decimal value = sublet.ExtendedPrice.GetValueOrDefault().WithDecimalImplied();
            int id = sublet.MiscellaneousChargeID.GetValueOrDefault();
            if (_volvoSettings.InterfaceOptions.WarrantyDeductibleCharges.NotNullAndContains(id) ||
                _volvoSettings.InterfaceOptions.EspDeductibleCharges.NotNullAndContains(id))
                value = Math.Abs(value);
            return new AmountType
            {
                Value = value,
                currencyID = _volvoSettings.RegionSettings.CurrencyCode
            };
        }

        private TextType[] FetchSubletDescription(MiscCharge miscCharge)
        {
            var description = miscCharge.Description;

            if (_repairOrderSnapshot.IsSecondaryRepairOrder())
            {
                if (IsWarrantyDeductible(miscCharge))
                {
                    description = $"{WARRANTY_DESCRIPTION_PREFIX} {miscCharge.Description}";
                }

                if (IsEspDeductible(miscCharge))
                {
                    description = $"{ESP_DESCRIPTION_PREFIX} {miscCharge.Description}";
                }
            }

            return new[] {
                new TextType {
                    Value = description.MaxLength(15)
                }
            };
        }

        private PriceEnumeratedType FetchSubletPriceCode(int id)
        {
            if (_volvoSettings.InterfaceOptions.SubletLaborCharges.NotNullAndContains(id))
                return PriceEnumeratedType.SubletLabor;

            if (IsWarranty() && _volvoSettings.InterfaceOptions.WarrantyDeductibleCharges.NotNullAndContains(id))
                return PriceEnumeratedType.SubletLabor;

            if (IsExtendedServicePlan() && _volvoSettings.InterfaceOptions.EspDeductibleCharges.NotNullAndContains(id))
                return PriceEnumeratedType.SubletLabor;

            if (_volvoSettings.InterfaceOptions.SubletPartsCharges.NotNullAndContains(id))
                return PriceEnumeratedType.SubletParts;

            return PriceEnumeratedType.Miscellaneous;
        }

        private IEnumerable<MiscCharge> MiscChargesForWarranty(RepairOrderTask job)
        {
            return (from id in _volvoSettings.InterfaceOptions.WarrantyDeductibleCharges
                    join miscCharge in job.MiscCharges on id equals miscCharge.MiscellaneousChargeID
                    select miscCharge).ToList();
        }

        private IEnumerable<MiscCharge> MiscChargesForExtendedServicePlan(RepairOrderTask job)
        {
            return (from id in _volvoSettings.InterfaceOptions.EspDeductibleCharges
                    join miscCharge in job.MiscCharges on id equals miscCharge.MiscellaneousChargeID
                    select miscCharge).ToList();
        }

        private static decimal AdjustedLaborChargeAfterDeductible(RepairOrderTask job, IEnumerable<MiscCharge> deductibleCharges)
        {
            var laborCharge = job.LaborEntries.Sum(labor => labor.ExtendedPrice.GetValueOrDefault());
            if (deductibleCharges == null)
            {
                return laborCharge;
            }
            var deductibleTotal = deductibleCharges.Sum(charge => Math.Abs(charge.ExtendedPrice.GetValueOrDefault()));
            return (laborCharge - deductibleTotal).WithDecimalImplied();
        }

        private bool IsWarranty(RepairOrderTask job)
        {
            return _volvoSettings.InterfaceOptions.WarrantyCustomers.NotNullAndContains(job.AlternateBillingCustomerKey)
                || (!string.IsNullOrWhiteSpace(_repairOrderSnapshot.OriginalRepairOrderNumber) && IsWarranty());
        }

        private bool IsWarranty()
        {
            return _volvoSettings.InterfaceOptions.WarrantyCustomers.NotNullAndContains(_repairOrderSnapshot.BillingCustomer.CustomerKey);
        }

        private bool IsWarrantyDeductible(MiscCharge charge)
        {
            return charge.MiscellaneousChargeID.HasValue &&
                   _volvoSettings.InterfaceOptions.WarrantyDeductibleCharges.NotNullAndContains(charge.MiscellaneousChargeID.Value);
        }

        private bool IsExtendedServicePlan(RepairOrderTask job)
        {
            return _volvoSettings.InterfaceOptions.EspCustomers.NotNullAndContains(job.AlternateBillingCustomerKey)
                || (!string.IsNullOrWhiteSpace(_repairOrderSnapshot.OriginalRepairOrderNumber) && IsExtendedServicePlan());
        }
        private bool IsExtendedServicePlan()
        {
            return _volvoSettings.InterfaceOptions.EspCustomers.NotNullAndContains(_repairOrderSnapshot.BillingCustomer.CustomerKey);
        }

        private bool IsEspDeductible(MiscCharge charge)
        {
            return charge.MiscellaneousChargeID.HasValue &&
                   _volvoSettings.InterfaceOptions.EspDeductibleCharges.NotNullAndContains(charge.MiscellaneousChargeID.Value);
        }

        private bool IsAfterWarrantyAssistance(RepairOrderTask job)
        {
            return _volvoSettings.InterfaceOptions.AwaCustomers.NotNullAndContains(job.AlternateBillingCustomerKey);
        }
        private bool IsAfterWarrantyAssistance()
        {
            return _volvoSettings.InterfaceOptions.AwaCustomers.NotNullAndContains(_repairOrderSnapshot.BillingCustomer.CustomerKey);
        }

        private bool IsInternalPolicyCustomer(RepairOrderTask job)
        {
            return _volvoSettings.InterfaceOptions.InternalPolicyCustomers.NotNullAndContains(job.AlternateBillingCustomerKey);
        }
        private bool IsInternalPolicyCustomer()
        {
            return _volvoSettings.InterfaceOptions.InternalPolicyCustomers.NotNullAndContains(_repairOrderSnapshot.BillingCustomer.CustomerKey);
        }

        private static decimal TotalLabourHours(RepairOrderTask job)
        {
            return job.LaborEntries.Sum(l => l.TotalHours.GetValueOrDefault()).WithDecimalImplied(1,DecimalExtensions.Round);
        }

        private static bool HasLabourHours(RepairOrderTask job)
        {
            return job.LaborEntries.NotNullAndAny(labor => labor.TotalHours.HasValue);
        }
    }
}