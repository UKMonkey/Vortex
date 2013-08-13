using SlimMath;
using Vortex.Interface.Audio;

namespace Vortex.Client.Audio.Null
{
    public class NullAudioChannel : IAudioChannel
    {
        public float ChannelVolume { get; set; }
        public void Play(IAudioSample audioSample) { }
        public void Play(IAudioSample audioSample, Vector3 source) {}
        public void Dispose()
        {
        }
    }
}