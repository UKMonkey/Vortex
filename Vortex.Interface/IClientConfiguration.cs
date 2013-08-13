using Psy.Core.Configuration.Sources;

namespace Vortex.Interface
{
    public interface IClientConfiguration : IConfiguration
    {
        ConfigurationSource PlayerConfiguration { get; }
        ConfigurationSource VortexClientConfiguration { get; }
    }
}