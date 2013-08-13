using Psy.Core.Configuration.Sources;

namespace Vortex.Interface
{
    public interface IServerConfiguration : IConfiguration
    {
        ConfigurationSource VortexServerConfiguration { get; }
    }
}