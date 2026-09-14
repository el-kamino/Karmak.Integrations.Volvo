using Karmak.Integrations.Volvo.Oasis.Mapping;
using Karmak.Integrations.Volvo.Oasis.Models;

namespace Karmak.Integrations.Volvo.Oasis.Tests.Mapping
{
    public class OasisRequestMapperTest
    {
        private readonly OasisRequestMapper _mapper = new();

        [Fact]
        public void MapSetsVinFromSource()
        {
            var source = new OasisRequestRest { Vin = "1FAHP3F29CL123456" };

            var result = _mapper.Map(source);

            Assert.Equal("1FAHP3F29CL123456", result.Vin);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapSetsBcmFromIncludeBroadcastMessages(bool value)
        {
            var source = new OasisRequestRest { IncludeBroadcastMessages = value };

            var result = _mapper.Map(source);

            Assert.Equal(value, result.Bcm);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapSetsVehicleInfoFromIncludeVehicleInfo(bool value)
        {
            var source = new OasisRequestRest { IncludeVehicleInfo = value };

            var result = _mapper.Map(source);

            Assert.Equal(value, result.VehicleInfo);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapSetsWarrantyFromIncludeWarrantyData(bool value)
        {
            var source = new OasisRequestRest { IncludeWarrantyData = value };

            var result = _mapper.Map(source);

            Assert.Equal(value, result.Warranty);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MapSetsFsaOnlyFromIncludeFsaData(bool value)
        {
            var source = new OasisRequestRest { IncludeFsaData = value };

            var result = _mapper.Map(source);

            Assert.Equal(value, result.FsaOnly);
        }

        [Fact]
        public void MapSetsMileageInAsStringWhenNotNull()
        {
            var source = new OasisRequestRest { MileageIn = 50000 };

            var result = _mapper.Map(source);

            Assert.Equal("50000", result.MileageIn);
        }

        [Fact]
        public void MapSetsMileageInNullWhenSourceNull()
        {
            var source = new OasisRequestRest { MileageIn = null };

            var result = _mapper.Map(source);

            Assert.Null(result.MileageIn);
        }

        [Fact]
        public void MapSetsCodesAndCommentsFromComplaintCode()
        {
            var source = new OasisRequestRest
            {
                ComplaintCode = new ComplaintCodeRest
                {
                    Code = "A1",
                    CodeType = "DTC",
                    Description = "Engine misfire"
                }
            };

            var result = _mapper.Map(source);

            Assert.NotNull(result.CodesAndComments);
            Assert.Equal("A1", result.CodesAndComments.Code);
            Assert.Equal("DTC", result.CodesAndComments.CodeType);
            Assert.Equal("Engine misfire", result.CodesAndComments.Description);
        }

        [Fact]
        public void MapSetsCodesAndCommentsNullWhenSourceNull()
        {
            var source = new OasisRequestRest { ComplaintCode = null };

            var result = _mapper.Map(source);

            Assert.Null(result.CodesAndComments);
        }

        [Fact]
        public void MapSetsSymptomCodeListFromSymptomCodes()
        {
            var source = new OasisRequestRest
            {
                SymptomCodes = new List<SymptomCodeRequestRest>
                {
                    new() { Order = 1, Code = "SC001" },
                    new() { Order = 2, Code = "SC002" }
                }
            };

            var result = _mapper.Map(source);

            Assert.NotNull(result.SymptomCode);
            Assert.Equal(2, result.SymptomCode.Codes.Count);
            Assert.Equal("SC001", result.SymptomCode.Codes[0].Value);
            Assert.Equal("1", result.SymptomCode.Codes[0].OrderNumber);
            Assert.Equal("SC002", result.SymptomCode.Codes[1].Value);
            Assert.Equal("2", result.SymptomCode.Codes[1].OrderNumber);
        }

        [Fact]
        public void MapSetsSymptomCodeNullWhenSourceNull()
        {
            var source = new OasisRequestRest { SymptomCodes = null };

            var result = _mapper.Map(source);

            Assert.Null(result.SymptomCode);
        }

        [Fact]
        public void MapSetsSymptomCodeNullWhenSourceEmpty()
        {
            var source = new OasisRequestRest { SymptomCodes = new List<SymptomCodeRequestRest>() };

            var result = _mapper.Map(source);

            Assert.Null(result.SymptomCode);
        }

        [Fact]
        public void MapHandlesAllFieldsPopulated()
        {
            var source = new OasisRequestRest
            {
                Vin = "1FAHP3F29CL123456",
                IncludeBroadcastMessages = true,
                IncludeVehicleInfo = true,
                IncludeWarrantyData = false,
                IncludeFsaData = true,
                MileageIn = 75000,
                ComplaintCode = new ComplaintCodeRest
                {
                    Code = "B1",
                    CodeType = "SYMPTOM",
                    Description = "Brake noise"
                },
                SymptomCodes = new List<SymptomCodeRequestRest>
                {
                    new() { Order = 1, Code = "SYM01" },
                    new() { Order = 2, Code = "SYM02" },
                    new() { Order = 3, Code = "SYM03" }
                }
            };

            var result = _mapper.Map(source);

            Assert.Equal("1FAHP3F29CL123456", result.Vin);
            Assert.True(result.Bcm);
            Assert.True(result.VehicleInfo);
            Assert.False(result.Warranty);
            Assert.True(result.FsaOnly);
            Assert.Equal("75000", result.MileageIn);
            Assert.NotNull(result.CodesAndComments);
            Assert.Equal("B1", result.CodesAndComments.Code);
            Assert.Equal("SYMPTOM", result.CodesAndComments.CodeType);
            Assert.Equal("Brake noise", result.CodesAndComments.Description);
            Assert.NotNull(result.SymptomCode);
            Assert.Equal(3, result.SymptomCode.Codes.Count);
            Assert.Equal("SYM01", result.SymptomCode.Codes[0].Value);
            Assert.Equal("1", result.SymptomCode.Codes[0].OrderNumber);
            Assert.Equal("SYM03", result.SymptomCode.Codes[2].Value);
            Assert.Equal("3", result.SymptomCode.Codes[2].OrderNumber);
        }

        [Fact]
        public void MapHandlesDefaultValues()
        {
            var source = new OasisRequestRest();

            var result = _mapper.Map(source);

            Assert.Null(result.Vin);
            Assert.False(result.Bcm);
            Assert.False(result.VehicleInfo);
            Assert.False(result.Warranty);
            Assert.False(result.FsaOnly);
            Assert.Null(result.MileageIn);
            Assert.Null(result.CodesAndComments);
            Assert.Null(result.SymptomCode);
        }
    }
}
