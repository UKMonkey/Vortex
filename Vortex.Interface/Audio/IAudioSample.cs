using System;

namespace Vortex.Interface.Audio
{
    public interface IAudioSample: IDisposable
    {
        string Filename { get; }
    }
}