using System;
using SlimMath;
using Vortex.Interface.EntityBase;

namespace Vortex.Interface.Audio
{
    public interface IAudioEngine: IDisposable
    {
        IAudioChannel CreateChannel(int channelId, int maxVoices = 4);
        IAudioChannel GetChannel(int channelId, bool useDefaultIfInvalidChannelId);
        IAudioSample Precache(string filename);

        void UpdateListenerPosition(Entity target);
        void UpdateListenerPosition(Vector3 position, Vector3 rotation);

        void Play(string filename, int channelId = 0);
        void Play(string filename, Vector3 source, int channelId = 0);

        float MasterVolume { get; set; }
    }
}