using Elk.Integrations.Volvo.Communications.Transport.Tests.Utils;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.React.Transport.ExtendedLogging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text;

namespace Elk.Integrations.Volvo.Communications.Transport.Tests.ExtendedLogging
{
    public class ExtendedLoggingClientTest
    {
        private const string Module = "FakeModule";
        private const string Application = "FakeApplication";
        private const string Content = "FakeContent";
        private const string Description = "Fake Description";

        private readonly StubExtendedLoggingService _internalService;
        private readonly IExtendedLoggingClient _client;

        public ExtendedLoggingClientTest()
        {
            _internalService = new StubExtendedLoggingService();
            _client = new ExtendedLoggingClient(NullLogger<ExtendedLoggingClient>.Instance, _internalService, Module, new SimpleApplicationNameResolver(Application));
        }

        [Fact]
        public async Task ItConstructsRequestAndSendsToInternalService()
        {
            await _client.Execute(Content, Description);

            Assert.Equal(1, _internalService.LogEvents.Count);
            Assert.Equal(Module, _internalService.LogEvents[0].Module);
            Assert.Equal(Application, _internalService.LogEvents[0].Application);
            Assert.Equal(Content, Encoding.Default.GetString(_internalService.LogEvents[0].Content));
        }

        [Fact]
        public async Task ItFormatsAndEscapesRequestMetadataBeforeSending()
        {
            var metadata = new Dictionary<string, string> {
                { "A.Foo", "https://example.com" },
                { "B.Bar/", "Baz" }
            };

            await _client.Execute(Content, Description, metadata);

            Assert.Equal(1, _internalService.LogEvents.Count);
            Assert.Equal("https%3A%2F%2Fexample.com", _internalService.LogEvents[0].Metadata["A_Foo"]);
            Assert.Equal("Baz", _internalService.LogEvents[0].Metadata["B_Bar%2F"]);
        }

        [Fact]
        public async Task ItAddsContentDescriptionToMetadata()
        {
            var metadata = new Dictionary<string, string> { { "A.Foo", "https://example.com" } };

            await _client.Execute(Content, Description, metadata);

            Assert.Equal(1, _internalService.LogEvents.Count);
            Assert.Equal(Uri.EscapeUriString(Description), _internalService.LogEvents[0].Metadata["ContentDescription"]);
        }

        [Fact]
        public async Task WhenCalledMultipleTimesItUpdatesContentDescription()
        {
            var metadata = new Dictionary<string, string> { { "A.Foo", "https://example.com" } };

            var originalDescription = "Original Content Description";
            var updatedDescription = "Updated Content Description";

            await _client.Execute(Content, originalDescription, metadata);
            await _client.Execute(Content, updatedDescription, metadata);

            Assert.Equal(2, _internalService.LogEvents.Count);
            Assert.Equal(Uri.EscapeUriString(originalDescription), _internalService.LogEvents[0].Metadata["ContentDescription"]);
            Assert.Equal(Uri.EscapeUriString(updatedDescription), _internalService.LogEvents[1].Metadata["ContentDescription"]);
        }

        [Fact]
        public async Task ItAddsContentType()
        {
            var stubContentType = "stubContentType";

            await _client.Execute(Content, Description, stubContentType);

            Assert.Equal(1, _internalService.LogEvents.Count);
            Assert.Equal(stubContentType, _internalService.LogEvents[0].ContentType);
        }

        [Fact]
        public async Task ItReplacesNullValueInMetadataWithNullString()
        {
            var metadata = new Dictionary<string, string> {
                { "Foo", "value" },
                { "Bar", null }
            };

            await _client.Execute(Content, Description, metadata);

            Assert.Equal(1, _internalService.LogEvents.Count);
            Assert.Equal("value", _internalService.LogEvents[0].Metadata["Foo"]);
            Assert.Equal(string.Empty, _internalService.LogEvents[0].Metadata["Bar"]);
        }

        [Fact]
        public async Task ItIncludesIdentityInMetadata()
        {
            var metadata = new Dictionary<string, string> {
                { "Foo", "value1" },
                { "Bar", "value2" }
            };

            var context = GenerateElkContext();
            await ImplicitElkContext.WithCurrentAsync(context, () => _client.Execute(Content, Description, metadata));


            Assert.Equal(1, _internalService.LogEvents.Count);
            var resultMetadata = _internalService.LogEvents[0].Metadata;
            Assert.True(resultMetadata["Foo"] == "value1");
            Assert.True(resultMetadata["Bar"] == "value2");
            Assert.True(resultMetadata["Elk_Identity_Account"] == context.Identity.Account.ToString());
            Assert.True(resultMetadata["Elk_Identity_User"] == context.Identity.User.ToString());
            Assert.True(resultMetadata["Elk_SecurityProfile_User"] == context.SecurityProfile.User.ToString());
            Assert.True(resultMetadata["Elk_SecurityProfile_Account"] == context.SecurityProfile.Account.ToString());
            Assert.True(resultMetadata["Elk_Role_Id"] == context.Role.Id.ToString());
            Assert.True(resultMetadata["Elk_ApplicationContext_Branch"] == context.ApplicationContext.Branch.ToString());
            Assert.True(resultMetadata["Elk_ApplicationContext_Instance"] == context.ApplicationContext.Instance.ToString());
            Assert.True(resultMetadata["Elk_ApplicationContext_Division"] == context.ApplicationContext.Division.ToString());
            Assert.True(resultMetadata["Elk_ApplicationContext_Company"] == context.ApplicationContext.Company.ToString());
            Assert.True(resultMetadata["Elk_ApplicationContext_Department"] == context.ApplicationContext.Department.ToString());
        }

        [Fact]
        public async Task ItIncludesIdentityInMetadataWhenIdentityIsNull()
        {
            var metadata = new Dictionary<string, string> {
                { "Foo", "value" },
                { "Bar", "value" }
            };

            await _client.Execute(Content, Description, metadata);

            Assert.Equal(1, _internalService.LogEvents.Count);
            var resultMetadata = _internalService.LogEvents[0].Metadata;
            Assert.False(resultMetadata.ContainsKey("Elk_Identity_Account"));
            Assert.False(resultMetadata.ContainsKey("Elk_Identity_User"));
            Assert.False(resultMetadata.ContainsKey("Elk_SecurityProfile_User"));
            Assert.False(resultMetadata.ContainsKey("Elk_SecurityProfile_Account"));
            Assert.False(resultMetadata.ContainsKey("Elk_Role_Id"));
            Assert.False(resultMetadata.ContainsKey("Elk_ApplicationContext_Branch"));
            Assert.False(resultMetadata.ContainsKey("Elk_ApplicationContext_Instance"));
            Assert.False(resultMetadata.ContainsKey("Elk_ApplicationContext_Division"));
            Assert.False(resultMetadata.ContainsKey("Elk_ApplicationContext_Company"));
            Assert.False(resultMetadata.ContainsKey("Elk_ApplicationContext_Department"));
        }

        private static ElkContext GenerateElkContext()
        {
            return new ElkContext.Builder()
            {
                ApplicationContext = new ElkApplicationContext.Builder
                {
                    Instance = Guid.NewGuid(),
                    Division = Guid.NewGuid(),
                    Company = Guid.NewGuid(),
                    Branch = Guid.NewGuid(),
                    Department = Guid.NewGuid()
                }.Build(),
                Identity = new ElkIdentity.Builder
                {
                    Account = Guid.NewGuid(),
                    User = Guid.NewGuid()
                }.Build(),
                SecurityProfile = new ElkIdentity.Builder
                {
                    Account = Guid.NewGuid(),
                    User = Guid.NewGuid()
                }.Build(),
                Role = new ElkRole.Builder
                {
                    Id = Guid.NewGuid()
                }.Build()
            }.Build();
        }
    }
}