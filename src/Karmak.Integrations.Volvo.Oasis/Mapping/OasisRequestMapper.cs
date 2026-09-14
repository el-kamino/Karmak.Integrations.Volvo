using Karmak.Integrations.Volvo.Oasis.Models;
using Karmak.Integrations.Volvo.Oasis.Models.Xml;

namespace Karmak.Integrations.Volvo.Oasis.Mapping
{
    public class OasisRequestMapper : IOasisRequestMapper
    {
        public OasisRequest Map(OasisRequestRest source)
        {
            return new OasisRequest
            {
                Vin = source.Vin,
                Bcm = source.IncludeBroadcastMessages,
                VehicleInfo = source.IncludeVehicleInfo,
                Warranty = source.IncludeWarrantyData,
                FsaOnly = source.IncludeFsaData,
                MileageIn = source.MileageIn?.ToString(),
                CodesAndComments = MapComplaintCode(source.ComplaintCode),
                SymptomCode = MapSymptomCodeList(source.SymptomCodes)
            };
        }

        private static ComplaintCode MapComplaintCode(ComplaintCodeRest source)
        {
            if (source == null)
                return null;

            return new ComplaintCode
            {
                Code = source.Code,
                CodeType = source.CodeType,
                Description = source.Description
            };
        }

        private static SymptomCodeList MapSymptomCodeList(List<SymptomCodeRequestRest> source)
        {
            if (source == null || !source.Any())
                return null;

            return new SymptomCodeList
            {
                Codes = source.Select(MapSymptomCodeItem).ToList()
            };
        }

        private static SymptomCodeItem MapSymptomCodeItem(SymptomCodeRequestRest source)
        {
            return new SymptomCodeItem
            {
                Value = source.Code,
                OrderNumber = source.Order.ToString()
            };
        }
    }
}
