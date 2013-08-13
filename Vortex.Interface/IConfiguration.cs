using Psy.Core.Configuration.Sources;

namespace Vortex.Interface
{
    public interface IConfiguration
    {
        ConfigurationSource ModConfiguration { get; }
        ConfigurationSource EngineConfiguration { get; }
    }
}