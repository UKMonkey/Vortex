using Psy.Core.Configuration.Sources;

namespace Vortex.Client.Configuration
{
    public static class PlayerConfigurationFactory
    {
         public static FileConfigurationSource Create()
         {
             var playerConfiguration = new FileConfigurationSource("player.cfg");

             playerConfiguration
                 .AddConfiguration("PlayerName", "Zombie Killer")
                 .AddConfiguration("Maximize", false)
                 .AddConfiguration("Sound.Enabled", true)
                 .AddConfiguration("Screen.Width", 1024)
                 .AddConfiguration("Screen.Height", 768);

             return playerConfiguration;
         }
    }
}