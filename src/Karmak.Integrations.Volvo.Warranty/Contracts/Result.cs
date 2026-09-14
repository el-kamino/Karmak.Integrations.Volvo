using System;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    public sealed class Result
    {

        private Result(string code = null, string reason = null, string details = null)
        {
            FaultReason = reason;
            FaultCode = code;
            FaultDetails = details;
        }

        public string FaultReason { get; }
        public string FaultCode { get; }
        public string FaultDetails { get; }

        public bool Succeeded =>
            string.IsNullOrWhiteSpace(FaultReason)
            && string.IsNullOrWhiteSpace(FaultCode)
            && string.IsNullOrWhiteSpace(FaultDetails);

        public static Result Success() => new Result();
        public static Result Fault(string code, string reason = null, string details = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException(nameof(code));
            return new Result(code, reason, details);
        }
    }
}
