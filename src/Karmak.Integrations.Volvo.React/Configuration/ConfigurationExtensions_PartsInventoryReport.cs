using Karmak.Integrations.Volvo.React.Splitting;
using Karmak.Integrations.Volvo.React.Transport.Soap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Xml.Linq;

namespace Karmak.Integrations.Volvo.React.Configuration
{
    public static partial class ConfigurationExtensions
    {
        private static IServiceCollection AddPartsInventoryReportProcessing(this IServiceCollection services, IConfiguration config)
        {
            int maxTransmisionSize = int.Parse(config["Volvo:React:MaxTransmissionSizeBytes"]);
            string xpath = $"//{XmlNamespaces.Star.Prefix}:PartsInventoryLine";

            services.AddTransient<IVolvoRequestSplitter, VolvoRequestSplitter>(sp =>
            {
                var splitter = new VolvoRequestSplitter(
                   new LogicalOr<XDocument>(
                       new HasNElementsAtXPath(xpath, 0),
                       new HasNElementsAtXPath(xpath, 1),
                       new IsBelowTransmissionSizeWhenSerialized(maxTransmisionSize)),
                   new SplitXDocumentOnXPath(xpath));
                return splitter;
            });

            return services;
        }
    }
}
