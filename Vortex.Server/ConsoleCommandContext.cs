using Vortex.Interface;
using Vortex.Interface.Net;

namespace Vortex.Server
{
    internal class ConsoleCommandContext : IConsoleCommandContext
    {
        public RemotePlayer Sender { get; set; }
    }
}