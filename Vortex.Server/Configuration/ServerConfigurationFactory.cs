using Psy.Core.Configuration.Sources;

namespace Vortex.Server.Configuration
{
    public static class ServerConfigurationFactory
    {
        public static FileConfigurationSource Create()
        {
            var scf = new FileConfigurationSource("server.cfg");

            scf
                .AddConfiguration("DefaultMod", "outbreak")
                .AddConfiguration("Net.Port", "9103");

            return scf;
        }
    }
}