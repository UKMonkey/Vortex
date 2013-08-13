using System;
using Psy.Core.Configuration;
using Psy.Core.Logging;
using Vortex.Client.Audio.Null;
using Vortex.Client.Audio.OpenAL;
using Vortex.Interface.Audio;

namespace Vortex.Client.Audio
{
    public static class AudioEngineFactory
    {
        public static IAudioEngine Create()
        {
            try
            {
                if (StaticConfigurationManager.ConfigurationManager.GetBool("Sound.Enabled"))
                {
                    Logger.Write("Sound enabled, using default sound factory", LoggerLevel.Critical);
                    return new OpenALAudioEngine();
                }
                Logger.Write("Sound disabled, using null sound factory", LoggerLevel.Critical);
            }
            catch (Exception e)
            {
                Logger.WriteException(e);
                Logger.Write("Failed to enable audio", LoggerLevel.Critical);
            }
            return new NullAudioEngine();
        }
    }
}