using System;
using System.Collections.Generic;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using Psy.Core;
using Psy.Core.FileSystem;
using SlimMath;
using Vortex.Interface.Audio;
using Vortex.Interface.EntityBase;

namespace Vortex.Client.Audio.OpenAL
{
    public class OpenALAudioEngine : IAudioEngine
    {
        private readonly AudioContext _audioContext;
        private readonly Dictionary<int, IAudioChannel> _audioChannels;
        private IAudioChannel _defaultAudioChannel;
        private readonly Dictionary<string, OpenALAudioSample> _samples;
        private Vector3 _positionModifier;

        public OpenALAudioEngine()
        {
            _audioContext = new AudioContext();
            _audioChannels = new Dictionary<int, IAudioChannel>();
            _samples = new Dictionary<string, OpenALAudioSample>();
            _positionModifier = new Vector3(0,0,0);
        }

        public IAudioChannel CreateChannel(int channelId, int maxVoices = 4)
        {
            if (_audioChannels.ContainsKey(channelId))
            {
                throw new Exception("Audio channel already exists");
            }

            var channel = new OpenALAudioChannel();

            if (_defaultAudioChannel == null)
            {
                _defaultAudioChannel = channel;
            }

            _audioChannels[channelId] = channel;
            return channel;
        }

        public IAudioChannel GetChannel(int channelId, bool useDefaultIfInvalidChannelId)
        {
            if (!_audioChannels.ContainsKey(channelId))
            {
                return _defaultAudioChannel;
            }
            return _audioChannels[channelId];
        }

        public IAudioSample Precache(string filename)
        {
            filename = Lookup.GetAssetPath(filename);

            if (_samples.ContainsKey(filename))
            {
                return _samples[filename];
            }

            var sample = new OpenALAudioSample(filename);
            _samples[filename] = sample;

            return sample;
        }

        public void Play(string filename, int channelId = 0)
        {
            filename = Lookup.GetAssetPath(filename);

            var channel = GetChannel(channelId, false);
            channel.Play(Precache(filename));
        }

        public void Play(string filename, Vector3 source, int channelId = 0)
        {
            filename = Lookup.GetAssetPath(filename);
            source += _positionModifier;

            // todo: calculate source-listener volume
            var channel = GetChannel(channelId, false);
            channel.Play(Precache(filename), source);
        }

        private static Vector3 Up = new Vector3(0, 0, -1);

        public void UpdateListenerPosition(Entity target)
        {
            UpdateListenerPosition(
                target.GetPosition(),
                DirectionUtil.CalculateVector(target.GetRotation())
                );
        }

        public void UpdateListenerPosition(Vector3 position, Vector3 rotation)
        {
            //var listenerTkV = position.ToOpenTkVector();
            //var listenerOrtentation = rotation.ToOpenTkVector();
            //var listenerUp = Up.ToOpenTkVector();

            //AL.Listener(ALListener3f.Position, ref listenerTkV);
            //AL.Listener(ALListenerfv.Orientation, ref listenerOrtentation, ref listenerUp);

            _positionModifier = -position;
        }

        public float MasterVolume { get; set; }

        public void Dispose()
        {
            foreach (var channel in _audioChannels.Values)
                channel.Dispose();
            _audioChannels.Clear();
            
            foreach (var sample in _samples.Values)
                sample.Dispose();
            _samples.Clear();

            _audioContext.Dispose();
        }
    }
}