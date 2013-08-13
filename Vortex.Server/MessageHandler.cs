using System;
using System.Collections.Generic;
using System.Linq;
using Psy.Core.Console;
using Vortex.Interface.Net;
using Vortex.Net.Messages;

namespace Vortex.Server
{
    internal class MessageHandler
    {
        private readonly Server _server;

        internal MessageHandler(Server server)
        {
            _server = server;
        }

        internal void RegisterHandlers()
        {
            _server.RegisterMessageCallback(typeof(ClientSayMessage), HandleClientSay);
            _server.RegisterMessageCallback(typeof(ClientConsoleCommandMessage), HandleClientConsoleCommand);
        }

        private void HandleClientSay(Message msg)
        {
            var inMessage = (ClientSayMessage)msg;
            var message = new ServerSayMessage { ClientId = msg.Sender.ClientId, Text = inMessage.Text };
            _server.SendMessage(message);
        }

        private void HandleClientConsoleCommand(Message msg)
        {
            var message = (ClientConsoleCommandMessage)msg;

            var rconPassword = message.Password;
            var rconCommand = message.Command;
            var rconCommandParts = rconCommand.Split(' ');

            var client = msg.Sender;

            if (rconCommand == "")
                return;

#if DEBUG
            var isAuthed = true;
#else
            var isAuthed = rconPassword == "letmein";
#endif

            if (!isAuthed)
                return;

            _server.GetConsoleCommandContext().Sender = client;

            var command = rconCommandParts[0];

            switch (command)
            {
                case "ssd":
                    RconCommandStateDump(client, rconCommandParts);
                    break;
                default:
                    StaticConsole.Console.Eval(rconCommand);
                    break;
            }
        }

        private void RconCommandStateDump(RemotePlayer player, string[] parameters)
        {
            var output = new List<string>
                             {
                                 "------ entity debug report ------",
                                 String.Format("Logical entity count = {0}", _server.Entities.Count()),
                                 "--- end of entity debug report --"
                             };
            var message = new ServerMultiSayMessage
                              {Text = output};
            _server.SendMessageToClient(message, player);
        }
    }
}