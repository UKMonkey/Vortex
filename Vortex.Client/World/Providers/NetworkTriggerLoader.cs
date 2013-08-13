using System;
using Vortex.Interface;
using Vortex.Interface.Net;
using Vortex.Interface.World.Chunks;
using Vortex.Interface.World.Triggers;
using Vortex.Net.Messages;

namespace Vortex.Client.World.Providers
{
    public class NetworkTriggerLoader : ITriggerLoader
    {
#pragma warning disable 67
        public event TriggerCallback OnTriggerLoaded;
        public event TriggerCallback OnTriggerGenerated;
        public event ChunkKeyCallback OnTriggersUnavailable;
#pragma warning restore 67

        private readonly IEngine _engine;

        public NetworkTriggerLoader(IEngine engine)
        {
            _engine = engine;
            engine.RegisterMessageCallback(typeof(TriggerUpdatedMessage), HandleTriggerMessage);
        }

        public void LoadTriggers(ChunkKey location)
        {
            //var msg = _engine.CreateNetworkMessage((byte)MessageType.TriggerRequested);
            //msg.Write(location);

            //_engine.SendMessage(msg);
        }

        private void HandleTriggerMessage(Message msg)
        {
            //var triggers = msg.ReadTriggers();
            //OnTriggerLoaded(triggers);
        }

        public void Dispose()
        {
            _engine.UnregisterMessageCallback(typeof(TriggerUpdatedMessage));
        }
    }
}
