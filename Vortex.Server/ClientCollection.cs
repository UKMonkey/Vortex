using System.Collections.Generic;
using System.Linq;
using Lidgren.Network;
using Vortex.Interface.Net;
using Vortex.Net;

namespace Vortex.Server
{
    internal class ClientCollection : List<NetConnection>
    {
        private readonly NetServer _server;

        internal ClientCollection(NetServer server)
        {
            _server = server;
        }

        private bool HasPotentialRecipients { get { return Count > 0; } }

        public void SendMessage(NetOutgoingMessage message, RemotePlayer except, DeliveryMethod method, int channel)
        {
            if (!HasPotentialRecipients)
                return;

            var recipients =
                this.Where(c => c.RemoteUniqueIdentifier != except.Connection.RemoteUniqueIdentifier).ToList();

            if (recipients.Count == 0)
                return;

            _server.SendMessage(
                message,
                recipients,
                DeliveryMethodMapper.Map(method), 
                channel);
        }

        public void SendMessage(NetOutgoingMessage message, DeliveryMethod deliveryMethod, int channel)
        {
            if (!HasPotentialRecipients)
                return;

            _server.SendMessage(message, this, DeliveryMethodMapper.Map(deliveryMethod), channel);
        }
    }
}