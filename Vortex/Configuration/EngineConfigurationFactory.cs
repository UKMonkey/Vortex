using Psy.Core.Configuration.Sources;

namespace Vortex.Configuration
{
    public static class EngineConfigurationFactory
    {
         public static FileConfigurationSource Create()
         {
             var ecf = new FileConfigurationSource("engine.cfg");

             ecf
                 .AddConfiguration("Debug", true)
                 .AddConfiguration("LogDir", @"VortexEngine\Logs")
                 .AddConfiguration("Net.ConnectTimeout", 10)
                 .AddConfiguration("Net.MaxConnections", 5);

             return ecf;
         }
    }
}