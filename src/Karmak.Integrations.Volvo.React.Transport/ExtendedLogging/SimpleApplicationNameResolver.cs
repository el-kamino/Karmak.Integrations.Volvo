namespace Karmak.Integrations.Volvo.React.Transport.ExtendedLogging
{
    public class SimpleApplicationNameResolver : IApplicationResolver {
        private readonly string _applicationName;

        public SimpleApplicationNameResolver(string applicationName) {
            _applicationName = applicationName;
        }
        public string Resolve() {
            return _applicationName;
        }
    }
}
