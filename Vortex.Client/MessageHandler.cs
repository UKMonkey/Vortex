using System;
using Psy.Core.Configuration;
using SlimMath;
using Vortex.Interface.Net;
using Vortex.Net.Messages;

namespace Vortex.Client
{
    public class MessageHandler
    {
        private readonly Client _client;

        internal MessageHandler(Client client)
        {
            _client = client;
        }

        internal void RegisterHandlers()
        {
            _client.RegisterMessageCallback(typeof(ServerWeatherChangeMessage), HandleWeatherChangeMessage);
            _client.RegisterMessageCallback(typeof(ServerHandshakeMessage), HandleHandshakeMessage);
            _client.RegisterMessageCallback(typeof(ServerClientJoinMessage), HandleServerClientJoin);
            _client.RegisterMessageCallback(typeof(ServerClientLeaveMessage), HandleServerClientLeave);
            _client.RegisterMessageCallback(typeof(ServerConnectedClientsMessage), HandleServerConnectedClients);
            _client.RegisterMessageCallback(typeof(ServerEntityFocusMessage), HandleServerEntityFocus);
            _client.RegisterMessageCallback(typeof(ServerSayMessage), HandleServerSay);
            _client.RegisterMessageCallback(typeof(ServerMultiSayMessage), HandleServerMultiSay);
            _client.RegisterMessageCallback(typeof(ServerPlaySoundMessage), HandleServerSoundMessage);
            _client.RegisterMessageCallback(typeof(ServerPlaySoundDirectionMessage), HandleServerSoundPositionMessage);
            _client.RegisterMessageCallback(typeof(ServerPlaySoundAtEntityMessage), HandleServerSoundEntityMessage);
            _client.RegisterMessageCallback(typeof(ServerRejectMessage), HandleRejectionMessage);
        }

        private void HandleWeatherChangeMessage(Message msg)
        {
            var message = (ServerWeatherChangeMessage)msg;
            _client.IsRaining = message.IsRaining;
        }

        private void HandleHandshakeMessage(Message msg)
        {
            var newMsg = new ClientHandshakeMessage
            {
                // hack: Vortex shouldn't have to get PlayerName like this.
                PlayerName = StaticConfigurationManager.ConfigurationManager.GetString("PlayerName")
            };
            _client.SendMessage(newMsg);

            _client.LoadMap();
        }

        private void HandleServerClientJoin(Message msg)
        {
            var message = (ServerClientJoinMessage)msg;
            _client.OnClientJoin(message.ClientId, message.PlayerName);
        }

        private void HandleServerClientLeave(Message msg)
        {
            var message = (ServerClientLeaveMessage)msg;
            _client.OnClientLeave(message.ClientId);
        }

        private void HandleServerConnectedClients(Message msg)
        {
            var message = (ServerConnectedClientsMessage)msg;
            _client.UpdateRemotePlayerList(message.RemotePlayers);
        }

        private void HandleServerEntityFocus(Message msg)
        {
            var message = (ServerEntityFocusMessage)msg;
            _client.OnFocusChange(message.EntityId);
        }

        private void HandleServerSay(Message msg)
        {
            var message = (ServerSayMessage)msg;

            var clientId = message.ClientId;
            var txt = message.Text;

            if (clientId == 0)
            {
                // message from server.
                _client.ConsoleText(txt, new Color4(1.0f, 0.0f, 1.0f, 0.0f));
            }
            else
            {
                var client = _client.RemotePlayers.GetRemotePlayer(clientId);
                _client.ConsoleText(String.Format("<{0}>{1}", client.PlayerName, message));
            }
        }

        private void HandleServerMultiSay(Message msg)
        {
            var message = (ServerMultiSayMessage)msg;

            foreach (var txt in message.Text)
            {
                _client.ConsoleText(txt, new Color4(1.0f, 0.0f, 1.0f, 0.0f));
            }
        }

        private void HandleServerSoundMessage(Message msg)
        {
            var message = (ServerPlaySoundMessage)msg;
            _client.PlaySound(message.SoundId, message.SoundChannel);
        }

        private void HandleServerSoundPositionMessage(Message msg)
        {
            var message = (ServerPlaySoundDirectionMessage)msg;

            if (_client.Me == null)
                return;

            _client.PlaySound(message.SoundId, message.Position, message.SoundChannel);
        }

        private void HandleServerSoundEntityMessage(Message msg)
        {
            var message = (ServerPlaySoundAtEntityMessage)msg;

            if (_client.Me == null)
                return;

            var entity = _client.GetEntity(message.EntityId);
            _client.PlaySound(message.SoundId, entity.GetPosition(), message.SoundChannel);
        }

        private void HandleRejectionMessage(Message msg)
        {
            var message = (ServerRejectMessage) msg;
            _client.Game.OnClientRejected(message.Reason);
        }
    }
}