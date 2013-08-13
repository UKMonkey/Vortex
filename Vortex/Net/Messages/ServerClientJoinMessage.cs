using System;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    public class ServerClientJoinMessage : Message
    {
        public ServerClientJoinMessage() {}

        public ServerClientJoinMessage(RemotePlayer client)
        {
            ClientId = client.ClientId;
            PlayerName = client.PlayerName;
        }

        public ushort ClientId { get; set; }
        public String PlayerName { get; set; }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            ClientId = messageStream.ReadUint16();
            PlayerName = messageStream.ReadString();
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            messageStream.WriteUInt16(ClientId);
            messageStream.Write(PlayerName);
        }
    }
}
