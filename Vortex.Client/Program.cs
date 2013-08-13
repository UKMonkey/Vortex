using System;
using System.IO;
using Psy.Core;
using Psy.Core.Configuration;
using Psy.Core.Console;
using Psy.Core.FileSystem;
using Psy.Core.Logging;
using Psy.Core.Logging.Loggers;
using Psy.Windows;
using Vortex.Client.Configuration;
using Vortex.Configuration;
using Vortex.Interface;

namespace Vortex.Client
{
    static class Program
    {
        private static bool _errored;

        [STAThread]
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionEventHandler;
            AppDomain.CurrentDomain.ProcessExit += ProcessExit;

            var startArguments = StartArguments.ParseFrom(args);

            StaticConsole.Initialize();

            StaticConfigurationManager.Initialize();
            var configurationManager = StaticConfigurationManager.ConfigurationManager;

            var playerConfiguration = PlayerConfigurationFactory.Create();
            configurationManager.RegisterSource(playerConfiguration);

            var engineConfiguration = EngineConfigurationFactory.Create();
            configurationManager.RegisterSource(engineConfiguration);

            var clientConfiguration = ClientConfigurationFactory.Create();
            configurationManager.RegisterSource(clientConfiguration);

            var modName = string.IsNullOrEmpty(startArguments.ModName)
                              ? configurationManager.GetString("DefaultMod")
                              : startArguments.ModName;
            Lookup.AddPath(Path.Combine("Mods", modName), true);
            startArguments.ModName = modName;

            var modConfiguration = ModConfigurationFactory.Create(startArguments);
            configurationManager.RegisterSource(modConfiguration);

            LoggingConfiguration.Initialize();

            FileLogger.GlobalSource = "Client";

            Logger.Write(string.Format("Starting with mod `{0}`", startArguments.ModName), LoggerLevel.Info);

            var configuration = new ClientConfiguration(
                modConfiguration, playerConfiguration, 
                clientConfiguration, engineConfiguration);

            if (!string.IsNullOrEmpty(StaticConfigurationManager.ConfigurationManager.GetString("DataDirectory")))
            {
                Lookup.AddPath(StaticConfigurationManager.ConfigurationManager.GetString("DataDirectory"), true);
            }

            var windowAttributes = new WindowAttributes
                                   {
                                       Width = StaticConfigurationManager.ConfigurationManager.GetInt("Screen.Width"),
                                       Height = StaticConfigurationManager.ConfigurationManager.GetInt("Screen.Height"), 
                                       Title = "Outbreak",
                                       AllowResize = true
                                   };

            using (var window = new EngineWindow(startArguments, configuration, windowAttributes))
            {
                window.Run();
            }
        }

        private static void ProcessExit(object sender, EventArgs e)
        {
            if (_errored)
                return;

            StaticConfigurationManager.ConfigurationManager.WriteSources();
        }

        private static void UnhandledExceptionEventHandler(object sender, UnhandledExceptionEventArgs e)
        {
            _errored = true;

            var exception = e.ExceptionObject as Exception;
            if (exception == null)
                return;
            Logger.WriteException(exception);

            MessageBox.Error(exception.Message);
        }
    }
}
