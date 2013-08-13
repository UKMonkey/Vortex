using Psy.Core.Configuration;
using Psy.Core.Configuration.Sources;
using Vortex.Interface;

namespace Vortex.Client.Configuration
{
    public class ClientConfiguration : IClientConfiguration
    {
        public ConfigurationSource ModConfiguration { get; private set; }
        public ConfigurationSource EngineConfiguration { get; private set; }
        public ConfigurationSource PlayerConfiguration { get; private set; }
        public ConfigurationSource VortexClientConfiguration { get; private set; }

        public ClientConfiguration(ConfigurationSource modConfiguration, 
            ConfigurationSource playerConfiguration, 
            ConfigurationSource clientConfiguration,
            ConfigurationSource engineConfiguration)
        {
            ModConfiguration = modConfiguration;
            PlayerConfiguration = playerConfiguration;
            VortexClientConfiguration = clientConfiguration;
            EngineConfiguration = engineConfiguration;
        }
    }
}