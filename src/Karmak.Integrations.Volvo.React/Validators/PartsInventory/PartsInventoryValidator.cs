using FluentValidation;
using Karmak.Integrations.Volvo.React.Contracts.PartsInventory.Data;
using System;
using System.Linq;

namespace Karmak.Integrations.Volvo.React.Validators.PartsInventory
{
    public class PartsInventoryValidator : BaseValidator<PartsInventoryReport>
    {
        public PartsInventoryValidator()
        {
            RuleFor(inventoryReport => inventoryReport.Id)
                .NotNull()
                .WithMessage("Guid can not be null");

            RuleForEach(inventory => inventory.Parts)
                .SetValidator(new PartValidator())
                .When(report => report.Parts != null && report.Parts.Any());
            
            RuleFor(inventoryReport => inventoryReport.Type)
                .Must(t => Enum.IsDefined(typeof(ReportType), t))
                .WithMessage("inventory Report Type must be either Delta or Full");
        }
    }

    public static class PartsInventoryExtensions
    {
        public static bool IsValid(this PartsInventoryReport partsInventoryReport, out string errors)
        {
            return new PartsInventoryValidator().IsValid(partsInventoryReport, out errors);
        }

        public static bool HasParts(this PartsInventoryReport partsInventoryReport) => partsInventoryReport?.Parts?.Any() ?? false;
    }
}