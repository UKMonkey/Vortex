using System.Collections.Generic;
using System.Linq;
using Vortex.Interface.Net;
using Vortex.Interface.World.Triggers;

namespace Vortex.World.Triggers
{
    public static class MessageSerialisationExtensions
    {
        public static void Write(this IOutgoingMessageStream msg, List<ITrigger> triggers)
        {
            var tweeakedTriggers = triggers.Where(trigger => trigger.SendToClient).ToList();
            msg.WriteByte((byte)tweeakedTriggers.Count);

            foreach (var trigger in tweeakedTriggers)
            {
                //msg.Write(trigger);
            }
        }

        public static List<ITrigger> ReadTriggers(this IIncomingMessageStream msg)
        {
            var count = msg.ReadInt32();
            var ret = new List<ITrigger>(count);

            for (var i=0; i<count; ++i)
            {
                // TODO - fix this after the one below!
                var tmp = ReadTrigger(msg);
                //ret.Add();
            }
            return ret;
        }

        /******************/

        public static void Write(this IOutgoingMessageStream msg, ITrigger trigger)
        {
            msg.Write(trigger.Name);
            //msg.Write(trigger.Key);
            msg.Write(trigger.Location);
            //msg.Write(trigger.Configuration);
        }

        public static ITrigger ReadTrigger(this IIncomingMessageStream msg)
        {
            var name = msg.ReadString();
            var key = msg.ReadTriggerKey();
            var location = msg.ReadVector();
            var properties = msg.ReadStringKeyValue();

            var trigger = StaticTriggerFactory.Instance.GetTrigger(name, key, location);
            trigger.SetProperties(key, location, properties);
            return trigger;
        }

        /******************/

        public static void Write(this IOutgoingMessageStream msg, TriggerKey key)
        {
            msg.Write(key.ChunkLocation);
            msg.WriteInt16(key.Id);
        }

        public static TriggerKey ReadTriggerKey(this IIncomingMessageStream msg)
        {
            var chunkKey = msg.ReadChunkKey();
            var id = msg.ReadInt16();

            return new TriggerKey(chunkKey, id);
        }

        /******************/

        public static void Write(this IOutgoingMessageStream msg, IEnumerable<KeyValuePair<string, string>> data)
        {
            msg.WriteInt32(data.Count());
            foreach (var item in data)
            {
                msg.Write(item.Key);
                msg.Write(item.Value);
            }
        }

        public static List<KeyValuePair<string, string>> ReadStringKeyValue(this IIncomingMessageStream msg)
        {
            var count = msg.ReadInt32();
            var ret = new List<KeyValuePair<string, string>>(count);

            for (var i = 0; i < count; ++i)
            {
                var key = msg.ReadString();
                var value = msg.ReadString();
                ret.Add(new KeyValuePair<string, string>(key, value));
            }
            return ret;
        }
    }
}
