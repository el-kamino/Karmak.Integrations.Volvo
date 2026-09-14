namespace Karmak.Integrations.Volvo.Dcds.Api
{
    public class DcdsResponseContent<T>
    {
        [Newtonsoft.Json.JsonProperty("data")]
        public T? Data { get; set; }
    }

    public class DcdsFileInfo
    {
        [Newtonsoft.Json.JsonProperty("remote_file_id")]
        public string? FileId { get; set; }
        [Newtonsoft.Json.JsonProperty("statusCode")]
        public string? StatusCode { get; set; }
    }
}
