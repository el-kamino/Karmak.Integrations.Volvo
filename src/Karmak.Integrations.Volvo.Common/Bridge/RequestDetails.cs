using System.Collections.Generic;

namespace Karmak.Integrations.Volvo.Common.Bridge
{
    public class RequestDetails
    {
        public Verb Verb { get; set; }
        public string Url { get; set; }
        public List<Header> Headers { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
    }

    public enum Verb
    {
        Get,
        Post,
        Put,
        Delete
    }

    public class Header
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
