using System.Collections.Generic;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerConnectedClientsMessage : Message
    {
        public List<RemotePlayer> RemotePlayers { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            RemotePlayers = messageStream.ReadRemotePlayers();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.Write(RemotePlayers);
        }
    }
}
