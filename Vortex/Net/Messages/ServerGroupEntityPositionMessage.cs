using System;
using System.Collections.Generic;
using System.IO;
using SlimMath;
using Vortex.Interface.Net;

namespace Vortex.Net.Messages
{
    /** This message is a little more complex than the others
     * as it tries to compress the entity position messages
     * the saving is 25b/msg -> 17b/msg... so not insignificant.
     * plus it reduces the padding by sending bigger messages ... which can't be measured easily
     */
    public class ServerGroupEntityPositionMessage : Message
    {
        public List<ServerEntityPositionMessage> Messages { get; private set; }
        
        public ServerGroupEntityPositionMessage()
        {
            Messages = new List<ServerEntityPositionMessage>();
        }

        protected override void DeserializeImpl(IIncomingMessageStream messageStream)
        {
            foreach (var dataArray in ReadStreams(messageStream))
            {
                DecodeData(dataArray);
            }
        }

        protected override void SerializeImpl(IOutgoingMessageStream messageStream)
        {
            Messages.Sort((a, b) => a.EntityId.CompareTo(b.EntityId));
            var streams = new List<MemoryStream>();
            var last = 0;

            for (var i=0; i<Messages.Count; ++i)
            {
                last = WriteEntityData(Messages[i], last, streams);
            }

            WriteStreams(messageStream, streams);
        }

        private static int WriteEntityData(ServerEntityPositionMessage msg, int last, IList<MemoryStream> streams)
        {
            MemoryStream stream;
            var difference = (byte)(msg.EntityId - last);
            if (last == 0 || difference > 250)
            {
                stream = new MemoryStream();
                streams.Add(stream);
                stream.Write(BitConverter.GetBytes(msg.EntityId), 0, 4);
            }
            else
            {
                stream = streams[streams.Count - 1];
                stream.WriteByte(difference);
            }

            stream.Write(BitConverter.GetBytes(msg.Rotation), 0, 4);
            stream.Write(BitConverter.GetBytes(msg.Position.X), 0, 4);
            stream.Write(BitConverter.GetBytes(msg.Position.Y), 0, 4);
            stream.Write(BitConverter.GetBytes(msg.Position.Z), 0, 4);

            return msg.EntityId;
        }

        private void DecodeData(byte[] data)
        {
            var lastId = 0;
            var byteCount = data.Length;
            var processed = 0;

            while (processed != byteCount)
            {
                if (processed == 0)
                {
                    lastId = BitConverter.ToInt32(data, processed);
                    processed += 4;
                }
                else
                {
                    lastId += data[processed++];
                }

                var rotation = BitConverter.ToSingle(data, processed);
                var x = BitConverter.ToSingle(data, processed + 4);
                var y = BitConverter.ToSingle(data, processed + 8);
                var z = BitConverter.ToSingle(data, processed + 12);

                processed += 16;

                Messages.Add(new ServerEntityPositionMessage
                    {EntityId = lastId,
                    Position = new Vector3(x, y, z),
                    Rotation = rotation});
            }
        }

        private static void WriteStreams(IOutgoingMessageStream msgStream, List<MemoryStream> streams)
        {
            msgStream.WriteUInt16((ushort) (streams.Count));
            foreach (var stream in streams)
            {
                msgStream.WriteBytes(stream.GetBuffer(), (int)stream.Length);
            }
        }

        private static IEnumerable<byte[]> ReadStreams(IIncomingMessageStream msgStream)
        {
            var count = msgStream.ReadUint16();
            var list = new List<byte[]>(count);

            for (var i=0; i<count; ++i)
            {
                var data = msgStream.ReadBytes();
                list.Add(data);
            }

            return list;
        }

        public override IEnumerable<Message> SubMessages()
        {
            return Messages;
        }
    }
}
