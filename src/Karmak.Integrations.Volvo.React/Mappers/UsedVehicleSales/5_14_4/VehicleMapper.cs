using Karmak.Integrations.Volvo.React.Contracts.Common;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_10_2;
using Karmak.Integrations.Volvo.React.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.UsedVehicleSales.V5_14_4
{
    public static class VehicleMapper
    {
        private const decimal MAX_ODOMETER = 999_999m;
        private const string ONLY_SALE_TYPE = "Retail";
        private const string ONLY_VEHICLE_NOTE = "Used";
        private const int MAX_VEHICLES_ALLOWED = 2;

        public static TradeInVehicleCreditType[] MapTradeIn(IList<Vehicle> vehicles)
        {
            if(vehicles == null || vehicles.Count == 0)
            {
                return null;
            }
            else
            {
               return vehicles
                   .Take(MAX_VEHICLES_ALLOWED)
                   .Select(BuildVehicleCredit)
                   .ToArray();
            }
        }

        private static TradeInVehicleCreditType BuildVehicleCredit(Vehicle v)
        {
            return new TradeInVehicleCreditType
            {
                Vehicle = new VehicleABIEType
                {
                    VehicleID = new IdentifierType
                    {
                        Value = VinFormatter.Format(v.VIN)
                    }
                },
                DeliveryDistanceMeasure = OdometerFor(v)
            };
        }

        public static RetailDeliveryReportingVehicleLineItemType[] MapSale(IList<Vehicle> vehicles)
        {
            if (vehicles == null || vehicles.Count == 0)
            {
                return null;
            }

            var vehicle = vehicles.First();

            return new[] {
                new RetailDeliveryReportingVehicleLineItemType {
                    Vehicle = new VehicleABIEType {
                        VehicleID = new IdentifierType {
                            Value = VinFormatter.Format(vehicle.VIN)
                        },
                        VehicleNote = new[] {
                            new TextType {
                                Value = ONLY_VEHICLE_NOTE
                            }
                        }
                    },
                    DeliveryDistanceMeasure = OdometerFor(vehicle),
                    SaleTypeDescription = new TextType {
                        Value = ONLY_SALE_TYPE
                    }
                }
            };
        }

        private static LengthMeasureType OdometerFor(Vehicle vehicle)
        {
            var unitType = vehicle?.Odometer?.UnitType;

            return new LengthMeasureType
            {
                unitCode = unitType == OdometerUnitType.MILES || unitType == OdometerUnitType.UNKNOWN
                    ? LengthUnitsContentType.mile
                    : LengthUnitsContentType.kilometer,
                Value = Math.Truncate(vehicle?.Odometer?.Reading ?? 1m).OrMax(MAX_ODOMETER)
            };
        }
    }

}