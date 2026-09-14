using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public sealed class UpdateStatus : IEquatable<UpdateStatus>
    {
        public string Code { get; set; }
        public string Description { get; set; }

        public bool Equals(UpdateStatus other)
        {
            var code = EqualityExtensions.Equals(Code, other?.Code);

            return code;
        }
    }
}
