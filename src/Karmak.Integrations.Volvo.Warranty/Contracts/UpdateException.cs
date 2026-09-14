using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public sealed class UpdateException : IEquatable<UpdateException>
    {
        public string Code { get; set; }
        public string Description { get; set; }

        public bool Equals(UpdateException other)
        {
            var code = EqualityExtensions.Equals(Code, other?.Code);
            var description = EqualityExtensions.Equals(Description, other?.Description);

            return code && description;
        }
    }
}
