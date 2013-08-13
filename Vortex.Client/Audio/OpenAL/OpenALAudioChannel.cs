using OpenTK.Audio.OpenAL;
using SlimMath;
using Vortex.Interface.Audio;

namespace Vortex.Client.Audio.OpenAL
{
    public class OpenALAudioChannel : IAudioChannel
    {
        private readonly int[] _sourceIds;
        private byte _nextSource;
        public float ChannelVolume { get; set; }

        public OpenALAudioChannel()
        {
            _sourceIds = AL.GenSources(10);
        }

        public void Play(IAudioSample audioSample)
        {
            var sample = (OpenALAudioSample)audioSample;
            var sourceId = _sourceIds[(_nextSource++)%_sourceIds.Length];

            AL.SourceStop(sourceId);
            AL.SourceUnqueueBuffer(sourceId);
            AL.SourceQueueBuffer(sourceId, sample.Buffer);
            AL.SourcePlay(sourceId);
        }

        public void Play(IAudioSample audioSample, Vector3 source)
        {
            var sample = (OpenALAudioSample)audioSample;
            var sourceTkV = source.ToOpenTkVector();
            var sourceId = _sourceIds[(_nextSource++) % _sourceIds.Length];

            AL.Source(sourceId, ALSource3f.Position, ref sourceTkV);
            AL.Source(sourceId, ALSourceb.SourceRelative, true);

            AL.SourceStop(sourceId);
            AL.SourceUnqueueBuffer(sourceId);

            AL.SourceQueueBuffer(sourceId, sample.Buffer);
            AL.SourcePlay(sourceId);
        }

        public void Dispose()
        {
            AL.DeleteSources(_sourceIds);
        }
    }
}