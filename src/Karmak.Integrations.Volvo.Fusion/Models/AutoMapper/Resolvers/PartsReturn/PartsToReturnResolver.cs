using AutoMapper;
using Karmak.Integrations.Volvo.Dcds.Contracts;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.PartsReturn;

namespace Karmak.Integrations.Volvo.Fusion.Models.AutoMapper.Resolvers.PartsReturn;

public class PartsToReturnResolver : IValueResolver<PartPurchaseOrder, PartsReturnRequest, List<ReturnPartItem>>
{
    public List<ReturnPartItem> Resolve(PartPurchaseOrder source, PartsReturnRequest destination, List<ReturnPartItem> destMember, ResolutionContext context)
    {
        var returnType = source.Messages?.FirstOrDefault(message => message?.MessageType == "ReturnReason")?.MessageText;

        List<ReturnPartItem> returnedParts = new List<ReturnPartItem>();

        if (source.Parts != null)
        {
            foreach (PartPurchaseOrderDetail orderDetail in source.Parts)
            {
                returnedParts.Add(new ReturnPartItem
                {
                    BinNumber = orderDetail.BinLocation,
                    PartNumber = orderDetail.PartNumber,
                    QuantityToReturn = orderDetail.Quantity,
                    ReturnType = returnType
                });
            }
        }
        return returnedParts;
    }
}
