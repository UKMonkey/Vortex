using System;
using System.Text;
using Psy.Core;
using Psy.Core.Console;
using Psy.Core.Tasks;
using SlimMath;
using Vortex.Interface.EntityBase.Properties;

namespace Vortex.Client.Console
{
    partial class Commands
    {
        private readonly Client _engine;

        public Commands(Client engine)
        {
            _engine = engine;
        }

        public void BindConsoleCommands()
        {
            var commandBindings = StaticConsole.Console.CommandBindings;

            commandBindings.Bind("netinfo", "NetworkInfo", GetNetworkInfoCommand);
            commandBindings.Bind("gc", "garbage collector stats", ConsoleCommandGc);
            commandBindings.Bind("dbug", "debug information", ConsoleCommandDebug);
            commandBindings.Bind("ls", "debug information", ConsoleCommandDebug);
            commandBindings.Bind("players", "list connected players", ConsoleCommandListConnectedPlayers);
            commandBindings.Bind("vol", "vol [channel] [level 0-255]", VolumeCommandHandler);
            commandBindings.Bind("tasks", "List tasks and their statistics", StaticTaskQueue.TaskQueue.HandleTasksCommand);
        }

        private void VolumeCommandHandler(params string[] parameters)
        {
            int channelNumber;

            if (parameters.Length == 1)
            {
                return;
            }

            if (!int.TryParse(parameters[1], out channelNumber))
            {
                StaticConsole.Console.AddLine(string.Format("Could not parse channel number `{0}`", parameters[1]), Colours.Red);
                return;
            }

            var channel = _engine.AudioEngine.GetChannel(channelNumber, false);
            if (channel == null)
            {
                StaticConsole.Console.AddLine(string.Format("No such channel `{0}`", channelNumber), Colours.Red);
                return;
            }

            if (parameters.Length == 2)
            {
                StaticConsole.Console.AddLine(string.Format("Channel `{0}` volume = {1}", channelNumber, channel.ChannelVolume));
                return;
            }

            float volume;

            if (!float.TryParse(parameters[2], out volume))
            {
                StaticConsole.Console.AddLine(string.Format("Could not parse volume `{0}`", parameters[2]), Colours.Red);
                return;
            }

            channel.ChannelVolume = volume;

        }

        private static string GetEnumName(int entityPropertyId)
        {
            return entityPropertyId <= (int)(EntityPropertyEnum.MaxEngineEnumProperty) 
                ? ((EntityPropertyEnum) entityPropertyId).ToString() 
                : string.Format("__GameProp{0}", entityPropertyId);
        }

        private void ConsoleCommandGc(string[] parameters)
        {
            _engine.ConsoleText(string.Format("GC.TotalMemory = {0}", GC.GetTotalMemory(false)));

            var sb = new StringBuilder("Collections ");
            for (var i = 0; i <= GC.MaxGeneration; i++)
            {
                sb.Append(string.Format("G{0}:{1} ", i, GC.CollectionCount(i)));
            }
            _engine.ConsoleText(sb.ToString());

            _engine.ConsoleText(string.Format("Last update alloc = {0}", _engine.EngineWindow.LastUpdateMemoryAlloc));
        }

        private void ConsoleCommandListConnectedPlayers(string[] parameters)
        {
            foreach (var c in _engine.ConnectedClients)
            {
                _engine.ConsoleText(String.Format("{0} - {1}", c.ClientId, c.PlayerName));
            }
        }

        private void GetNetworkInfoCommand(string[] parameters)
        {
            var connectionStatistics = _engine.ConnectionStatistics;
            if (connectionStatistics == null)
            {
                _engine.ConsoleText("Unable to read connection statistics", new Color4(1.0f, 1.0f, 0.0f, 0.0f));
                return;
            }

            _engine.ConsoleText(
                String.Format("Recv Bytes: {0}", connectionStatistics.ReceivedBytes),
                new Color4(1.0f, 0.4f, 0.8f, 0.0f));

            _engine.ConsoleText(
                String.Format("Sent Bytes: {0}", connectionStatistics.SentBytes),
                new Color4(1.0f, 0.4f, 0.8f, 0.0f));

            _engine.ConsoleText(
                String.Format("Recv Packets: {0}", connectionStatistics.ReceivedPackets),
                new Color4(1.0f, 0.4f, 0.8f, 0.0f));

            _engine.ConsoleText(
                String.Format("Sent Packets: {0}", connectionStatistics.SentPackets),
                new Color4(1.0f, 0.4f, 0.8f, 0.0f));
        }
    }
}
