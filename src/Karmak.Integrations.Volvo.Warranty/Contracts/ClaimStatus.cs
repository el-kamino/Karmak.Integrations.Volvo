using System.Runtime.Serialization;

namespace Karmak.Integrations.Volvo.Warranty.Contracts
{
    [DataContract]
    public class ClaimStatus
    {
        public static readonly ClaimStatus New = new ClaimStatus("New");
        public static readonly ClaimStatus Submitted = new ClaimStatus("Submitted");
        public static readonly ClaimStatus Failed = new ClaimStatus("Failed");
        private ClaimStatus() { }

        public ClaimStatus(string value, string code = null)
        {
            Value = value;
            Code = code;
        }

        [DataMember]
        public string Value
        {
            get;
            private set;
        }

        [DataMember]
        public string Code
        {
            get;
            private set;
        }
    }
}
