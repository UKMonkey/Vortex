using System;
using System.IO;
using System.Threading;
using Psy.Core;
using Psy.Core.Configuration;
using Psy.Core.Console;
using Psy.Core.FileSystem;
using Psy.Core.Logging;
using Psy.Core.Logging.Loggers;
using Psy.Core.Tasks;
using Vortex.Configuration;
using Vortex.Interface;
using Vortex.Server.Configuration;

namespace Vortex.Server
{
    static class Program
    {
        private static bool _errored;

        public static void Main(string[] args)
        {
            var startArguments = StartArguments.ParseFrom(args);

            StaticConfigurationManager.Initialize();
            var configurationManager = StaticConfigurationManager.ConfigurationManager;
            configurationManager.RegisterSource(ServerConfigurationFactory.Create());
            configurationManager.RegisterSource(EngineConfigurationFactory.Create());

            var modName = string.IsNullOrEmpty(startArguments.ModName)
                              ? configurationManager.GetString("DefaultMod")
                              : startArguments.ModName;
            Lookup.AddPath(Path.Combine("Mods", modName), true);
            startArguments.ModName = modName;

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                _errored = true;
                Logger.WriteException((Exception) eventArgs.ExceptionObject);
            };

            AppDomain.CurrentDomain.ProcessExit += ProcessExit;

            configurationManager.RegisterSource(ModConfigurationFactory.Create(startArguments));

            if (!string.IsNullOrEmpty(StaticConfigurationManager.ConfigurationManager.GetString("DataDirectory")))
            {
                Lookup.AddPath(StaticConfigurationManager.ConfigurationManager.GetString("DataDirectory"), true);
            }

            StaticConsole.Initialize();

            LoggingConfiguration.Initialize();

            FileLogger.GlobalSource = "Server";

            var engine = new Server(startArguments);
            engine.AttachModule();

            var platform = Platform.GetExecutingPlatform();

            while (engine.Running)
            {
                if (platform == PlatformType.Windows)
                {
                    Thread.Yield();
                }
                else
                {
                    Thread.Sleep(1);
                }
                StaticTaskQueue.TaskQueue.ProcessAll();
            }

            engine.Dispose();
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            if (!_errored)
                StaticConfigurationManager.ConfigurationManager.WriteSources();
        }
    }
}
