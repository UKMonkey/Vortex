using Vortex.Interface.Audio;

namespace Vortex.Client.Audio.Null
{
    public class NullAudioSample : IAudioSample
    {
        public string Filename { get; private set; }
        public void Dispose()
        {
        }
    }
}