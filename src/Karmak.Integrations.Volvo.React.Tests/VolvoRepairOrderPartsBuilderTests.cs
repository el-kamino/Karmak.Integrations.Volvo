using Karmak.Integrations.Volvo.React.Constants.Shared;
using Karmak.Integrations.Volvo.React.Contracts.RepairOrders.Data;
using Karmak.Integrations.Volvo.React.Mappers.RepairOrder._6_0_0;
using Karmak.Integrations.Volvo.React.RepairOrders.Extensions;
using Xunit;

namespace Karmak.Integrations.Volvo.React.Tests.Mappers.RepairOrder._6_0_0
{
    public class VolvoRepairOrderPartsBuilderTests
    {
        private readonly VolvoRepairOrderPartsBuilder _builder;
        private const decimal TimeZone = -5.0m;

        public VolvoRepairOrderPartsBuilderTests()
        {
            _builder = new VolvoRepairOrderPartsBuilder(TimeZone);
        }

        #region BuildServiceParts Tests

        [Fact]
        public void BuildServiceParts_WithRegularParts_ReturnsCorrectServiceParts()
        {
            // Arrange
            var task = new RepairOrderTask
            {
                Parts = new List<Part>
                {
                    CreateRegularPart("PART001", "Regular Part 1", 2, 10.50m, 21.00m, 9.00m, 0m, 0m),
                    CreateRegularPart("PART002", "Regular Part 2", 1, 15.75m, 15.75m, 14.00m, 0m, 0m)
                }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].partsId.Count);
            Assert.Equal("PART00", result[0].partsId[0].itemId);
            Assert.Equal("Regular Part 1", result[0].itemIdDescription);
            Assert.Equal("PART00", result[1].partsId[0].itemId);
            Assert.Equal("Regular Part 2", result[1].itemIdDescription);
        }

        [Fact]
        public void BuildServiceParts_WithNegativeQuantityRegularPart_ForcesNegativeExtendedAmount()
        {
            // Arrange
            var task = new RepairOrderTask
            {
                Parts = new List<Part>
                {
                    CreateRegularPart("PART001", "Regular Part", -2, 10.50m, -21.00m, 9.00m, 0m, 0m)
                }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Single(result);
            Assert.NotNull(result[0].price.extendedAmount);
            Assert.Equal(-2100, result[0].price.extendedAmount);
        }

        [Fact]
        public void BuildServiceParts_WithExchangePart_CreatesExchangeAndCoreParts()
        {
            // Arrange
            var exchangePart = CreateExchangePart("EXCH001", "Exchange Part", 1, 100.00m, 100.00m, 90.00m, "CORE001", 18.00m, 5.00m);
            var returnedCore = CreateCorePart("CORE001", "Core Part", -1, -20.00m, -20.00m, -18.00m);
            var task = new RepairOrderTask
            {
                Parts = new List<Part> { exchangePart, returnedCore }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Equal(3, result.Count());
            Assert.Equal("Exchange Part", result[0].itemIdDescription);
            Assert.True(result[0].itemQuantity > 0);
            Assert.Contains(PartConstants.CORE_SALE, result[1].itemIdDescription);
            Assert.Equal("CORE", result[1].partsReturnDestinationCode);
            Assert.Equal(1800, result[1].price.extendedAmount);
            Assert.Equal(500, result[1].price.partCost);
            Assert.Contains(PartConstants.CORE_RETURN, result[2].itemIdDescription);
            Assert.Equal("CORE", result[2].partsReturnDestinationCode);
            Assert.Equal(-2000, result[2].price.extendedAmount);
            Assert.Equal(1800, result[2].price.partCost);
        }

        [Fact]
        public void BuildServiceParts_WithExchangePartAndNoReturnedCore_CreatesExchangeAndCorePart()
        {
            // Arrange
            var exchangePart = CreateExchangePart("EXCH001", "Exchange Part", 1, 100.00m, 100.00m, 90.00m, "CORE001", 18.00m, 5.00m);
            var task = new RepairOrderTask
            {
                Parts = new List<Part> { exchangePart }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("Exchange Part", result[0].itemIdDescription);
            Assert.True(result[0].itemQuantity > 0);
            Assert.Contains(PartConstants.CORE_SALE, result[1].itemIdDescription);
            Assert.Equal("CORE", result[1].partsReturnDestinationCode);
            Assert.Equal(1800, result[1].price.extendedAmount);
            Assert.Equal(500, result[1].price.partCost);
        }

        [Fact]
        public void BuildServiceParts_WithExchangeAndReturnedCore_CreatesThreeParts()
        {
            // Arrange
            var exchangePart = CreateExchangePart("EXCH001", "Exchange Part", 1, 100.00m, 100.00m, 90.00m, "CORE001", 18.00m, 5.00m);
            var returnedCore = CreateCorePart("CORE001", "Core Part", -1, -20.00m, -20.00m, -18.00m);
            var task = new RepairOrderTask
            {
                Parts = new List<Part> { exchangePart, returnedCore }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("Exchange Part", result[0].itemIdDescription);
            Assert.Contains(PartConstants.CORE_SALE, result[1].itemIdDescription);
            Assert.Contains(PartConstants.CORE_RETURN, result[2].itemIdDescription);
            Assert.Equal("CORE", result[2].partsReturnDestinationCode);
        }

        [Fact]
        public void BuildServiceParts_WithUnassociatedCore_CreatesCorePart()
        {
            // Arrange
            var unassociatedCore = CreateCorePart("CORE002", "Unassociated Core", -1, -15.00m, -15.00m, -13.00m);
            var task = new RepairOrderTask
            {
                Parts = new List<Part> { unassociatedCore }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Single(result);
            Assert.Contains(PartConstants.CORE_RETURN, result[0].itemIdDescription);
            Assert.Equal("CORE", result[0].partsReturnDestinationCode);
        }

        [Fact]
        public void BuildServiceParts_WithPartKit_CreatesKitAndAssemblyParts()
        {
            // Arrange
            var partKit = CreatePartKit("KIT001", "Part Kit", 1, 100.00m, 100.00m, 90.00m);
            partKit.AssemblyParts = new List<AssemblyPart>
            {
                new AssemblyPart
                {
                    PartNumber = "ASMPART001",
                    Description = "Assembly Part 1",
                    Quantity = 2,
                    UnitListPrice = 30.00m,
                    PartType = PartType.REGULAR
                },
                new AssemblyPart
                {
                    PartNumber = "ASMPART002",
                    Description = "Assembly Part 2",
                    Quantity = 1,
                    UnitListPrice = 40.00m,
                    PartType = PartType.REGULAR
                }
            };

            var task = new RepairOrderTask
            {
                Parts = new List<Part> { partKit }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(PartConstants.PART_KIT, result[0].itemIdDescription);
            Assert.Equal(0, result[0].price.extendedAmount);
            Assert.Equal(0, result[0].price.partCost);
            Assert.Contains(PartConstants.PART_KIT, result[1].itemIdDescription);
            Assert.Contains(PartConstants.PART_KIT, result[2].itemIdDescription);
        }

        [Fact]
        public void BuildServiceParts_WithPartKitAndAssemblyCharges_CreatesAllParts()
        {
            // Arrange
            var partKit = CreatePartKit("KIT001", "Part Kit", 1, 100.00m, 100.00m, 90.00m);
            partKit.AssemblyParts = new List<AssemblyPart>
            {
                new AssemblyPart
                {
                    PartNumber = "ASMPART001",
                    Description = "Assembly Part 1",
                    Quantity = 1,
                    UnitListPrice = 50.00m,
                    PartType = PartType.REGULAR
                }
            };
            partKit.AssemblyMiscCharges = new List<AssemblyMiscCharge>
            {
                new AssemblyMiscCharge
                {
                    Name = "CHARGE001",
                    Description = "Misc Charge",
                    Quantity = 1,
                    UnitListPrice = 50.00m,
                    UnitCost = 45.00m
                }
            };

            var task = new RepairOrderTask
            {
                Parts = new List<Part> { partKit }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(PartConstants.PART_KIT, result[0].itemIdDescription);
            Assert.Contains("Assembly Part 1", result[1].itemIdDescription);
            Assert.Contains("Misc Charge", result[2].itemIdDescription);
        }

        [Fact]
        public void BuildServiceParts_WithPartKit_AppliesDiscountCorrectly()
        {
            // Arrange
            var partKit = CreatePartKit("KIT001", "Part Kit", 1, 80.00m, 80.00m, 70.00m);
            partKit.AssemblyParts = new List<AssemblyPart>
            {
                new AssemblyPart
                {
                    PartNumber = "ASMPART001",
                    Description = "Assembly Part 1",
                    Quantity = 2,
                    UnitListPrice = 30.00m,
                    PartType = PartType.REGULAR
                },
                new AssemblyPart
                {
                    PartNumber = "ASMPART002",
                    Description = "Assembly Part 2",
                    Quantity = 1,
                    UnitListPrice = 40.00m,
                    PartType = PartType.REGULAR
                }
            };

            var task = new RepairOrderTask
            {
                Parts = new List<Part> { partKit }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            var totalExtendedAmount = result.Skip(1).Sum(p => p.price.extendedAmount);
            Assert.True(totalExtendedAmount > 0);
        }

        [Fact]
        public void BuildServiceParts_WithPartKit_AppliesDiscountRemainderCorrectly()
        {
            // Arrange
            var partKit = CreatePartKit("KIT001", "Part Kit", 1, 28.86m, 28.86m, 19.6477m);
            partKit.AssemblyParts = new List<AssemblyPart>
            {
                new AssemblyPart
                {
                    PartNumber = "ASMPART001",
                    Description = "Assembly Part 1",
                    Quantity = 2,
                    UnitListPrice = .435m,
                    UnitCost = .1236m,
                    PartType = PartType.REGULAR
                },
                new AssemblyPart
                {
                    PartNumber = "ASMPART002",
                    Description = "Assembly Part 2",
                    Quantity = 1,
                    UnitListPrice = .81m,
                    UnitCost = .24m,
                    PartType = PartType.REGULAR
                },
                new AssemblyPart
                {
                    PartNumber = "ASMPART003",
                    Description = "Assembly Part 3",
                    Quantity = 1,
                    UnitListPrice = 2.39m,
                    UnitCost = 1.0605m,
                    PartType = PartType.REGULAR
                },
                new AssemblyPart
                {
                    PartNumber = "ASMPART004",
                    Description = "Assembly Part 4",
                    Quantity = 1,
                    UnitListPrice = 72.4m,
                    UnitCost = 18.1m,
                    PartType = PartType.REGULAR
                }
            };

            var task = new RepairOrderTask
            {
                Parts = new List<Part> { partKit }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Equal(5, result.Count);
            Assert.Contains(PartConstants.PART_KIT, result[0].itemIdDescription);
            Assert.Contains(PartConstants.PART_KIT, result[1].itemIdDescription);
            Assert.Contains(PartConstants.PART_KIT, result[2].itemIdDescription);
            Assert.Contains(PartConstants.PART_KIT, result[3].itemIdDescription);
            Assert.Contains(PartConstants.PART_KIT, result[4].itemIdDescription);

            Assert.Equal(0, result[0].price.extendedAmount);
            Assert.Equal(0, result[0].price.partCost);

            Assert.Equal(32, result[1].price.extendedAmount);
            Assert.Equal(12, result[1].price.partCost);

            Assert.Equal(30, result[2].price.extendedAmount);
            Assert.Equal(24, result[2].price.partCost);

            Assert.Equal(90, result[3].price.extendedAmount);
            Assert.Equal(106, result[3].price.partCost);

            Assert.Equal(2734, result[4].price.extendedAmount);
            Assert.Equal(1810, result[4].price.partCost);

        }

        [Fact]
        public void BuildServiceParts_WithEmptyParts_ReturnsEmptyList()
        {
            // Arrange
            var task = new RepairOrderTask
            {
                Parts = new List<Part>()
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region BuildPartsId Tests

        [Fact]
        public void BuildPartsId_WithShortPartNumber_ReturnsSinglePrefix()
        {
            // Arrange
            var partNumber = "ABC12";
            var task = new RepairOrderTask
            {
                Parts = new List<Part>
                {
                    CreateRegularPart(partNumber, "Part", 1, 10.00m, 10.00m, 9.00m, 0m, 0m)
                }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Single(result[0].partsId);
            Assert.Equal("ABC12", result[0].partsId[0].itemId);
            Assert.Equal("Part Prefix", result[0].partsId[0].type);
        }

        [Fact]
        public void BuildPartsId_WithMediumPartNumber_ReturnsPrefixAndBase()
        {
            // Arrange
            var partNumber = "ABCDEFGHIJ";
            var task = new RepairOrderTask
            {
                Parts = new List<Part>
                {
                    CreateRegularPart(partNumber, "Part", 1, 10.00m, 10.00m, 9.00m, 0m, 0m)
                }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Equal(2, result[0].partsId.Count);
            Assert.Equal("ABCDEF", result[0].partsId[0].itemId);
            Assert.Equal("Part Prefix", result[0].partsId[0].type);
            Assert.Equal("GHIJ", result[0].partsId[1].itemId);
            Assert.Equal("Part Base", result[0].partsId[1].type);
        }

        [Fact]
        public void BuildPartsId_WithLongPartNumber_ReturnsPrefixBaseAndSuffix()
        {
            // Arrange
            var partNumber = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var task = new RepairOrderTask
            {
                Parts = new List<Part>
                {
                    CreateRegularPart(partNumber, "Part", 1, 10.00m, 10.00m, 9.00m, 0m, 0m)
                }
            };

            // Act
            var result = _builder.BuildServiceParts(task);

            // Assert
            Assert.Equal(3, result[0].partsId.Count);
            Assert.Equal("ABCDEF", result[0].partsId[0].itemId);
            Assert.Equal("Part Prefix", result[0].partsId[0].type);
            Assert.Equal("GHIJKLMN", result[0].partsId[1].itemId);
            Assert.Equal("Part Base", result[0].partsId[1].type);
            Assert.Equal("OPQRSTUV", result[0].partsId[2].itemId);
            Assert.Equal("Part Suffix", result[0].partsId[2].type);
        }

        #endregion

        #region Helper Methods

        private Part CreateRegularPart(string partNumber, string description, decimal quantity, decimal unitPrice, decimal extendedPrice, decimal unitCost, decimal coreExtendedPrice, decimal coreUnitCost)
        {
            return new Part
            {
                PartNumber = partNumber,
                Description = description,
                Quantity = quantity,
                UnitPrice = unitPrice,
                ExtendedPrice = extendedPrice,
                UnitCost = unitCost,
                AddDate = DateTime.UtcNow,
                AddUsername = "TestUser",
                CoreExtendedPrice = coreExtendedPrice,
                CoreUnitCost = coreUnitCost,
                PartType = PartType.REGULAR
            };
        }

        private Part CreateExchangePart(string partNumber, string description, decimal quantity, decimal unitPrice, decimal extendedPrice, decimal unitCost, string corePartNumber, decimal coreExtendedPrice, decimal coreUnitCost)
        {
            var part = CreateRegularPart(partNumber, description, quantity, unitPrice, extendedPrice, unitCost, coreExtendedPrice, coreUnitCost);
            part.PartType = PartType.EXCHANGE;
            part.CorePartNumber = corePartNumber;
            return part;
        }

        private Part CreateCorePart(string partNumber, string description, decimal quantity, decimal unitPrice, decimal extendedPrice, decimal unitCost)
        {
            var part = CreateRegularPart(partNumber, description, quantity, unitPrice, extendedPrice, unitCost, 0m, 0m);
            part.PartType = PartType.CORE;
            return part;
        }

        private Part CreatePartKit(string partNumber, string description, decimal quantity, decimal unitPrice, decimal extendedPrice, decimal unitCost)
        {
            var part = CreateRegularPart(partNumber, description, quantity, unitPrice, extendedPrice, unitCost, 0m, 0m);
            part.PartType = PartType.REGULAR;
            part.AssemblyParts = new List<AssemblyPart>();
            part.AssemblyMiscCharges = new List<AssemblyMiscCharge>();
            return part;
        }

        #endregion
    }
}