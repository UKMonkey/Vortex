using Psy.Core.Configuration.Sources;

namespace Vortex.Client.Configuration
{
    public static class ClientConfigurationFactory
    {
         public static FileConfigurationSource Create()
         {
             var ccf = new FileConfigurationSource("client.cfg");

             ccf
                 .AddConfiguration("DefaultMod", "outbreak")
                 .AddConfiguration("Net.Hosts",
                                   "localhost:9103;tacgnol.psyogenix.co.uk:9103;ukmonkey.dyndns-server.com:9103;lan.psyogenix.co.uk:9103;jupiter:9103;78.86.4.108:9103");

             return ccf;
         }
    }
}