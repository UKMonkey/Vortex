using SlimMath;
using Vortex.Interface.Audio;
using Vortex.Interface.EntityBase;

namespace Vortex.Client.Audio.Null
{
    public class NullAudioEngine : IAudioEngine
    {
        public IAudioChannel CreateChannel(int channelId, int maxVoices = 4)
        {
            return new NullAudioChannel();
        }

        public IAudioChannel GetChannel(int channelId, bool useDefaultIfInvalidChannelId)
        {
            return new NullAudioChannel();
        }

        public IAudioSample Precache(string filename)
        {
            return new NullAudioSample();
        }

        public void UpdateListenerPosition(Entity target)
        {
        }

        public void UpdateListenerPosition(Vector3 position, Vector3 rotation)
        {
        }

        public void Play(string filename, int channelId = 0) { }
        public void Play(string filename, Vector3 source, int channelId = 0) {}
        public float MasterVolume { get; set; }
        public void Dispose()
        {
        }
    }
}