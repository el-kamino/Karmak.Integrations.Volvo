using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Karmak.Integrations.Volvo.React.Comments
{
    internal class EmailSender : IEmailSender
    {
        private readonly HttpClient _httpClient;
        private readonly string _alertUri;

        public EmailSender(HttpClient httpClient, IOptions<EmailSenderOptions> options)
        {
            _httpClient = httpClient;
            _alertUri = options.Value.AlertUri;
        }

        public async Task Send(List<string> toList, string body)
        {
            await Send(new Email(toList, body));
        }

        public async Task Send(List<string> toList, string body, string subject)
        {
            await Send(new Email(toList, body, subject));
        }

        private async Task Send(Email email)
        {
            if (string.IsNullOrWhiteSpace(_alertUri))
                throw new InvalidOperationException("Alert URL is not set, unable to send email");
            if (email is null)
                throw new ArgumentNullException(nameof(email));
            if (!email.IsValid(out var msg))
                throw new ArgumentException(msg);

            //send json
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, new Uri(_alertUri)))
            {
                string json = JsonConvert.SerializeObject(email);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                await _httpClient.SendAsync(request);
            }
        }

        private class Email
        {
            public List<string> To;
            public string Subject;
            public string Body;

            public const string DefaultSubject = "Failure Occurred While Processing Comments";

            public Email() : this(new List<string>(), null, null) { }

            public Email(List<string> to, string body) : this(to, body, null) { }

            public Email(List<string> to, string body, string subject)
            {
                To = new List<string>(to);
                Subject = !string.IsNullOrWhiteSpace(subject) ? subject : DefaultSubject;
                Body = body;
            }

            public bool IsValid(out string msg)
            {
                if (To?.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(Subject))
                    {
                        msg = "Email's Subject line must be set";
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(Body))
                    {
                        msg = "Email's Body must be populated with non-whitespace text";
                        return false;
                    }
                }
                else
                {
                    msg = "Email must have at least one recipient address";
                    return false;
                }
                msg = string.Empty;
                return true;
            }
        }
    }
}