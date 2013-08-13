using System;
using OpenTK.Audio.OpenAL;
using Vortex.Interface.Audio;

namespace Vortex.Client.Audio.OpenAL
{
    public class OpenALAudioSample : IAudioSample
    {
        public string Filename { get; private set; }
        internal int Buffer { get; set; }

        public OpenALAudioSample(string filename)
        {
            Filename = filename;
            Buffer = AL.GenBuffer();

            var sample = OpenALOggLoader.Load(filename);

            AL.BufferData(Buffer, ALFormat.Mono16, sample.RawBuffer, sample.RawBuffer.Length, 44100);
        }

        public void Dispose()
        {
            AL.DeleteBuffer(Buffer);
        }
    }
}