using System.IO;
using Psy.Core.Configuration.Sources;
using Vortex.Interface;

namespace Vortex.Configuration
{
    public static class ModConfigurationFactory
    {
         public static FileConfigurationSource Create(StartArguments startArguments)
         {
             var mcf = new FileConfigurationSource(Path.Combine("Mods", startArguments.ModName, "mod.cfg"));

             mcf
                 .AddConfiguration("DataDirectory", "")
                 .AddConfiguration("ModServerDLL", "")
                 .AddConfiguration("ModClientDLL", "")
                 .AddConfiguration("Net.AppIdent", "");

             return mcf;
         }
    }
}