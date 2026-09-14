using Karmak.Integrations.Volvo.Common.Settings.Models;
using Karmak.Integrations.Volvo.React.Core.Gen.V5_14_4;
using Karmak.Integrations.Volvo.React.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Karmak.Integrations.Volvo.React.Core.Mappers.Shared.V5_14_4
{
    public static class ServiceAdvisorPartyMapper
    {
        private const string ID_TYPE = "local";
        private const int MAX_ID_LENGTH = 10;

        public static PartyABIEType[] Map(string id, VolvoSettings volvoSettings)
        {
            return string.IsNullOrWhiteSpace(id)
                ? null
                : new[] {
                    new PartyABIEType {
                        Item = new PersonTypeStar {
                        ID = new[] {
                            new IdentifierType {
                                schemeID = ID_TYPE,
                                Value = GetOemUser(volvoSettings, id).MaxLength(MAX_ID_LENGTH)
                            }
                        }
                    }
                }
            };
        }

        public static string GetOemUser(VolvoSettings settings, string username)
        {
            var oemUserMappings = GetOemUserMappings(settings).ToImmutableDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);

            var strippedUsername = username.Replace(".", string.Empty);

            return oemUserMappings.ContainsKey(strippedUsername) ? oemUserMappings[strippedUsername]
                : username;
        }

        private static IDictionary<string, string> GetOemUserMappings(VolvoSettings settings)
        {
            return settings?.InterfaceOptions?.OemUserMappings
                    ?? ImmutableDictionary<string, string>.Empty;
        }
    }
}