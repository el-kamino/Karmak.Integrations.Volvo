using Karmak.Integrations.Volvo.Api.Configuration;
using Karmak.Integrations.Volvo.Api.Models.Warranty;
using Karmak.Integrations.Volvo.Common.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Karmak.Integrations.Volvo.Api.Controllers.Warranty
{
    [ApiController]
    [Route("/api/AppService/v1/Warranty/settings")]
    [Authorize(AuthenticationSchemes = SecurityExtensions.VolvoCertScheme)]
    public class UISettingsController : ControllerBase
    {
        private readonly ISettingsProvider _client;

        public UISettingsController(ISettingsProvider client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<UISettings> GetUISettings()
        {
            var settings = await _client.GetSettingsAsync();
            return new UISettings
            {
                AllowOasisRetrieval = settings.InterfaceOptions.AllowOasisRetrieval,
                DealerCode = settings.InterfaceOptions.PaCode
            };
        }
    }
}
