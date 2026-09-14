namespace Karmak.Integrations.Volvo.React.Comments
{
    internal class CommentsServiceOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TokenUri { get; set; }
        public string Resource { get; set; }
        public string CommentsBaseUrl { get; set; }
        public string CommentsReceivePath { get; set; }
    }
}