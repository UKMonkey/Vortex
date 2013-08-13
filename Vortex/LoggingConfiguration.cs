using System;
using Psy.Core.Configuration;
using Psy.Core.Console;
using Psy.Core.Logging;
using Psy.Core.Logging.Loggers;

namespace Vortex
{
    public static class LoggingConfiguration
    {
        private static LoggerLevel GetLogLevel(string level)
        {
            LoggerLevel result;
            if (Enum.TryParse(level, true, out result))
                return result;

            return LoggerLevel.Error;
        }

        private static void PrepareLogging()
        {
            var fileLevel = StaticConfigurationManager.ConfigurationManager.GetString("Logging.File.Level");
            var cmdPromptLevel = StaticConfigurationManager.ConfigurationManager.GetString("Logging.CommandPrompt.Level");
            var consoleLevel = StaticConfigurationManager.ConfigurationManager.GetString("Logging.Console.Level");

            if (!string.IsNullOrEmpty(fileLevel))
            {
                Logger.Add(new FileLogger { LoggerLevel = GetLogLevel(fileLevel) });    
            }

            if (!string.IsNullOrEmpty(cmdPromptLevel))
            {
                Logger.Add(new CommandPromptLogger { LoggerLevel = GetLogLevel(cmdPromptLevel) });    
            }

            if (!string.IsNullOrEmpty(consoleLevel))
            {
                Logger.Add(new ConsoleLogger { LoggerLevel = GetLogLevel(consoleLevel) });
            }
        }

        public static void Initialize()
        {
            PrepareLogging();
        }
    }
}