using System;
using SlimMath;

namespace Vortex.Interface.Audio
{
    public interface IAudioChannel : IDisposable
    {
        float ChannelVolume { get; set; }
        void Play(IAudioSample audioSample);
        void Play(IAudioSample audioSample, Vector3 source);
    }
}