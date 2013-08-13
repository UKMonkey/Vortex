using Lidgren.Network;

namespace Vortex.Interface.Net
{
    public class RemotePlayer
    {
        public string PlayerName;
        public readonly ushort ClientId;
        public readonly NetConnection Connection;
        public int? EntityId { get; set; }

        public RemotePlayer(ushort clientId, string playerName)
        {
            ClientId = clientId;
            PlayerName = playerName;
            Connection = null;
        }

        public RemotePlayer(ushort clientId, string playerName, NetConnection connection)
        {
            ClientId = clientId;
            PlayerName = playerName;
            Connection = connection;
        }
    }
}
