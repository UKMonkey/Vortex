using System;
using System.Collections.Generic;
using Psy.Core.Logging;
using Vortex.Interface;
using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;
using Vortex.Net.Messages;

namespace Vortex.Client.World.Providers
{
    public class NetworkChunkLoader : IChunkLoader
    {
        private readonly IEngine _engine;
        public event ChunkCallback OnChunkLoad;

        // Never called - don't bother registering anything with this
#pragma warning disable 67
        public event ChunkCallback OnChunksGenerated;
        public event ChunkKeyCallback OnChunksUnavailable;
#pragma warning restore 67

        public NetworkChunkLoader(IEngine engine)
        {
            _engine = engine;
            engine.RegisterMessageCallback(typeof(ServerChunkUpdatedMessage), HandleChunkMessage);
        }

        public void Dispose()
        {
            _engine.UnregisterMessageCallback(typeof(ServerChunkUpdatedMessage));
        }

        /** send a message to get the given chunk keys
         *  no need to do anything else, when we get the reply we'll know more...
         */
        public void LoadChunks(List<ChunkKey> chunkKeys)
        {
            if (chunkKeys.Count == 0)
                return;

            foreach (var item in chunkKeys)
            {
                Logger.Write(String.Format("Requesting chunk {0}, {1}", item.X, item.Y), LoggerLevel.Trace);
            }

            var msg = new ClientChunkRequestedMessage {ChunkKeys = chunkKeys};
            _engine.SendMessage(msg);
        }

        /** handle a message with a load of chunks
         */
        private void HandleChunkMessage(Message msg)
        {
            var message = (ServerChunkUpdatedMessage) msg;
            var chunk = message.Chunk;

            Logger.Write(String.Format("Got chunk {0}, {1}", chunk.Key.X, chunk.Key.Y), LoggerLevel.Trace);

            if (OnChunkLoad != null)
                OnChunkLoad(new List<Chunk>{chunk});
        }
    }
}
