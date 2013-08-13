using Vortex.Interface.Net;

namespace Vortex.Interface
{
    public interface IConsoleCommandContext
    {
        /// <summary>
        /// Sender of the console command
        /// </summary>
        RemotePlayer Sender { get; }
    }
}