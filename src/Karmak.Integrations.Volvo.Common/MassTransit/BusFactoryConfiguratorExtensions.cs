using MassTransit;

namespace Karmak.Integrations.Volvo.Common.MassTransit
{
    public static class BusFactoryConfiguratorExtensions
    {
        public static void UseImplicitElkContext<T>(this T configurator) where T : ISendPipelineConfigurator, IPublishPipelineConfigurator, IPipeConfigurator<ConsumeContext>
        {
            configurator.UseAddImplicitElkContextToPublishedMessages();
            configurator.UseAddImplicitElkContextToSentMessages();
            configurator.UseExtractImplicitElkContextFromReceivedAndSubscribedMessages();
        }

        private static void UseAddImplicitElkContextToSentMessages<T>(this T configurator) where T : ISendPipelineConfigurator
        {
            configurator.ConfigureSend(sendPipeConfigurator =>
            {
                sendPipeConfigurator.AddPipeSpecification(new AddImplicitElkContextToHeadersPipeSpecification<SendContext>());
            });
        }

        private static void UseAddImplicitElkContextToPublishedMessages<T>(this T configurator) where T : IPublishPipelineConfigurator
        {
            configurator.ConfigurePublish(publishPipeConfigurator =>
            {
                publishPipeConfigurator.AddPipeSpecification(new AddImplicitElkContextToHeadersPipeSpecification<PublishContext>());
            });
        }

        private static void UseExtractImplicitElkContextFromReceivedAndSubscribedMessages<T>(this T configurator) where T : IPipeConfigurator<ConsumeContext>
        {
            configurator.AddPipeSpecification(new ExtractImplicitElkContextFromMessagePipeSpecification<ConsumeContext>());
        }
    }
}