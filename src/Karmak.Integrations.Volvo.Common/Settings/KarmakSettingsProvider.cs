using System.Net.Http.Headers;
using System.Text;
using Azure.Core;
using Karmak.ELK.Core.KICQ.Api.Core.Settings;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Common.Settings.Exceptions;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Karmak.Integrations.Volvo.Common.Settings
{
    public class KarmakSettingsProvider : ISettingsProvider
    {
        private const string VolvoSettingsExtentName = "Integrations.Volvo.Settings";
        private const string VolvoWarrantySettingsExtentName = "Integrations.Volvo.Warranty.Settings";
        private const string KicqSettingsExtentName = "Karmak.KICQ.Settings";

        private readonly HttpClient _httpClient;
        private readonly HybridCache _cache;
        private readonly KarmakSettingsProviderOptions _options;

        public KarmakSettingsProvider(HttpClient httpClient, HybridCache cache, IOptions<KarmakSettingsProviderOptions> options)
        {
            _httpClient = httpClient;
            _cache = cache;
            _options = options.Value;
        }

        public async Task<VolvoSettings> GetSettingsAsync()
        {
            var settings = await GetSettingsAsync<VolvoSettings>(VolvoSettingsExtentName);

            if (settings == null)
            {
                throw new NoSettingsFoundException();
            }

            return settings;
        }

        public async Task<KicqSettings> GetBridgeSettingsAsync()
        {
            var settings = await GetSettingsAsync<KicqSettings>(KicqSettingsExtentName);

            if (settings == null)
            {
                throw new NoSettingsFoundException();
            }

            return settings;
        }

        public async Task UpdateRecordAsync(SettingsRecord document)
        {
            string url = $"{_options.Url}/api/settings/record";

            using (var request = new HttpRequestMessage(HttpMethod.Put, url))
            {
                var dataObject = new JObject();
                dataObject["data"] = document.Data;
                dataObject["context_level"] = document.ContextLevel;
                dataObject["context_id"] = document.ContextId;
                dataObject["definition_id"] = document.RecordDefinitionId;

                var data = dataObject.ToString();
                request.Content = new StringContent(data);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                string token = await GetAuthTokenAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (HttpResponseMessage response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        private async Task<T> GetSettingsAsync<T>(string extentName)
        {
            var ctx = ToImmutableDictionary(ImplicitElkContext.Current);

            if (ctx == null || ctx.Count == 0)
            {
                throw new InvalidOperationException("No Elk context");
            }

            string cacheKey = BuildCacheKey(extentName, ctx);

            var settings = await _cache.GetOrCreateAsync(
                cacheKey,
                async ct => await RetrieveSettingsAsync<T>(extentName, ctx),
                new HybridCacheEntryOptions
                {
                    Expiration = _options.CacheTimeout ?? TimeSpan.FromMinutes(15)
                });

            return settings;
        }

        private async Task<T> RetrieveSettingsAsync<T>(string extent, Dictionary<ContextLevel, Guid> context)
        {
            string url = $"{_options.Url}/api/settings?extentQuery={extent}";
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                string elkContext = GetEncodedElkContext(context);

                request.Headers.Add("X-Voodoo-Context", elkContext);

                string token = await GetAuthTokenAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (HttpResponseMessage response = await _httpClient.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();

                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    using (var sr = new StreamReader(responseStream))
                    using (var jtr = new JsonTextReader(sr))
                    {
                        var serializer = new JsonSerializer();
                        var settings = serializer.Deserialize<T>(jtr);
                        return settings;
                    }
                }
            }
        }

        private async Task<string> GetAuthTokenAsync()
        {
            var requestContext = new TokenRequestContext([$"api://{_options.SettingsServiceResourceId}/.default"]);
            AccessToken response = await _options.Credential.GetTokenAsync(requestContext, CancellationToken.None);
            return response.Token;
        }

        private string GetEncodedElkContext(Dictionary<ContextLevel, Guid> elkContext)
        {
            var context = JsonConvert.SerializeObject(elkContext);
            var accountContextBytes = Encoding.UTF8.GetBytes(context);
            return Convert.ToBase64String(accountContextBytes);
        }

        private static string BuildCacheKey(string extent, Dictionary<ContextLevel, Guid> ctx)
        {
            var cacheKeyBuilder = new StringBuilder();
            cacheKeyBuilder.Append(extent);

            foreach (KeyValuePair<ContextLevel, Guid> entry in ctx.Where(x => x.Value != Guid.Empty))
            {
                cacheKeyBuilder.Append($":{entry.Value}");
            }

            return cacheKeyBuilder.ToString();
        }

        private static Dictionary<ContextLevel, Guid> ToImmutableDictionary(ElkContext elkContext)
        {
            var dict = new Dictionary<ContextLevel, Guid>();

            AddIfNotNullOrEmpty(dict, ContextLevel.Account, elkContext.Identity.Account);
            AddIfNotNullOrEmpty(dict, ContextLevel.User, elkContext.Identity.User);

            AddIfNotNullOrEmpty(dict, ContextLevel.Division, elkContext.ApplicationContext?.Division);
            AddIfNotNullOrEmpty(dict, ContextLevel.Company, elkContext.ApplicationContext?.Company);
            AddIfNotNullOrEmpty(dict, ContextLevel.Branch, elkContext.ApplicationContext?.Branch);
            AddIfNotNullOrEmpty(dict, ContextLevel.Department, elkContext.ApplicationContext?.Department);
            AddIfNotNullOrEmpty(dict, ContextLevel.Instance, elkContext.ApplicationContext?.Instance);

            return dict;
        }

        private static void AddIfNotNullOrEmpty(Dictionary<ContextLevel, Guid> dict, ContextLevel key, Guid? value)
        {
            if (value == null)
            {
                return;
            }

            if (value.Value == Guid.Empty)
            {
                return;
            }

            dict.Add(key, value.Value);
        }
    }
}
