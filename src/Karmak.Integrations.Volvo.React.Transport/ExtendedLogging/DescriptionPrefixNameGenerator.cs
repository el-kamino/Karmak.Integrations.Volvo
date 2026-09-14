using Elk.Core.ExtendedLogging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Karmak.Integrations.Volvo.React.Transport.ExtendedLogging
{
    public class DescriptionPrefixNameGenerator
        : IExtendedLogNameGenerator {
        private readonly IExtendedLogNameGenerator _bodyGenerator;

        public DescriptionPrefixNameGenerator(IExtendedLogNameGenerator bodyGenerator) {
            _bodyGenerator = bodyGenerator;
        }

        public string Generate(ExtendedLoggingRequest request) {
            if (!request.Metadata.TryGetValue("ContentDescription", out var description)) {
                return _bodyGenerator.Generate(request);
            } else {
                var name = Uri.UnescapeDataString(description).ToLowerInvariant().Trim().Replace(' ', '-');
                return $"{name}-{_bodyGenerator.Generate(request)}";
            }
        }
    }
}
