using AutoMapper;
using Karmak.Integrations.Elk.Identity;
using Karmak.Integrations.Elk.Identity.Context;
using Karmak.Integrations.Volvo.Common.Settings;
using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.Fusion.Models.AutoMapper;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels;
using Karmak.Integrations.Volvo.Fusion.Models.FusionModels.InterfaceOptions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FusionIdentity = Karmak.Integrations.Volvo.Fusion.Models.FusionModels.Shared.FusionIdentity;

namespace Karmak.Integrations.Volvo.Fusion.Common
{
    internal class InterfaceOptionsDispatch : IRequestDispatcher
    {
        public const string EntityType = "Volvo Interface Options";
        private const string BranchContext = "Branch";

        private readonly IMapper _mapper;
        private readonly ISettingsProvider _settingsProvider;
        private readonly InterfaceOptionsDispatchOptions _options;

        public InterfaceOptionsDispatch(
            IMapper mapper,
            ISettingsProvider settingsProvider,
            IOptions<InterfaceOptionsDispatchOptions> options)
        {
            _mapper = mapper;
            _settingsProvider = settingsProvider;
            _options = options.Value;
        }

        public async Task DispatchAsync(FusionRequest<JObject> request, FusionIdentity identity)
        {
            var requestPayload = request.Payload.ToObject<FusionVolvoInterfaceOptions>();
            var interfaceOptions = _mapper.Map<FusionVolvoInterfaceOptions, InterfaceOptions>(
                requestPayload,
                options =>
                {
                    options.Items[Constants.FusionIdentityKey] = identity;
                });

            var foundContext = ImplicitElkContext.TryGetCurrent(out ElkContext context);
            var contextId = context?.ApplicationContext?.Branch?.ToString();

            if (!foundContext || string.IsNullOrWhiteSpace(contextId))
            {
                throw new Exception("Failed to save Volvo Interface Options. No Elk Context found.");
            }

            var updatedRecord = new SettingsRecord
            {
                ContextId = contextId,
                ContextLevel = BranchContext,
                RecordDefinitionId = _options.VolvoInterfaceOptionsRecordDefinitionId,
                Data = JsonConvert.SerializeObject(interfaceOptions)
            };

            await _settingsProvider.UpdateRecordAsync(updatedRecord);
        }
    }
}
