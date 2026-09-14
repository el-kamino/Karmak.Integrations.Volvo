using Karmak.Integrations.Elk.Identity.Extraction;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    internal class MassTransitHeaderImplicitElkContextKeySpecification : IImplicitElkContextKeySpecification<string>
    {
        public static readonly MassTransitHeaderImplicitElkContextKeySpecification Instance =
            new MassTransitHeaderImplicitElkContextKeySpecification();

        private MassTransitHeaderImplicitElkContextKeySpecification()
        {}

        public IEnumerable<string> IdentityAccount { get; } =
            new[] { "X-Elk-Identity-Account" };

        public IEnumerable<string> IdentityUser { get; } =
            new[] { "X-Elk-Identity-User" };

        public IEnumerable<string> SecurityProfileAccount { get; } =
            new[] { "X-Elk-SecurityProfile-Account" };

        public IEnumerable<string> SecurityProfileUser { get; } =
            new[] { "X-Elk-SecurityProfile-User" };

        public IEnumerable<string> ApplicationContextBranch { get; } =
            new[] { "X-Elk-ApplicationContext-Branch", "X-Elk-Identity-Branch" };

        public IEnumerable<string> ApplicationContextCompany { get; } =
            new[] { "X-Elk-ApplicationContext-Company", "X-Elk-Identity-Company" };

        public IEnumerable<string> ApplicationContextDepartment { get; } =
            new[] { "X-Elk-ApplicationContext-Department", "X-Elk-Identity-Department" };

        public IEnumerable<string> ApplicationContextDivision { get; } =
            new[] { "X-Elk-ApplicationContext-Division", "X-Elk-Identity-Division" };

        public IEnumerable<string> ApplicationContextInstance { get; } =
            new[] { "X-Elk-ApplicationContext-Instance", "X-Elk-Identity-Instance" };

        public IEnumerable<string> RoleId { get; } =
            new[] { "X-Elk-Role-Id", "X-Elk-Identity-Role" };
    }
}