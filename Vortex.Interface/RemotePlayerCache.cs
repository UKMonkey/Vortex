using System.Collections.Generic;
using Vortex.Interface.Net;

namespace Vortex.Interface
{
    // todo: consider moving this into Vortex
    public class RemotePlayerCache
    {
        private readonly Dictionary<ushort, RemotePlayer> _players;

        public RemotePlayerCache()
        {
            _players = new Dictionary<ushort, RemotePlayer>();
        }

        public RemotePlayer GetRemotePlayer(ushort id)
        {
            RemotePlayer result;
            _players.TryGetValue(id, out result);
            return result;
        }

        public void AddRemotePlayer(RemotePlayer player)
        {
            _players[player.ClientId] = player;
        }

        public RemotePlayer RemoveRemotePlayer(ushort id)
        {
            var player = _players[id];
            if (player != null)
                _players.Remove(id);
            return player;
        }

        public IEnumerable<RemotePlayer> GetPlayers()
        {
            return _players.Values;
        }

        public void Clear()
        {
            _players.Clear();
        }
    }
}
