using Newtonsoft.Json;

namespace Karmak.Integrations.Volvo.Oasis.Models
{
    public class CodeLookupRest
    {
        public string Code { get; set; }

        public string Description { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CodeType { get; set; }
    }
}
