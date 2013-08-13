using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Psy.Core.Logging;
using Timer = Psy.Core.Timer;

namespace Vortex.Interface.Net
{
    public abstract class Message
    {
        private static volatile int _nextId = 0;
        private static SpinLock _idLock = new SpinLock();

        public RemotePlayer Sender { get; set; }
        public DeliveryMethod DeliveryMethod { get; protected set; }
        public int Channel { get; protected set; }
        public int Id { get; set; }

        private double ExpireTime { get { return ExpiryDelay + TimeCreated; } }
        private const long DefaultExpireTime = 500;

        protected const long DoesNotExpire = -1;
        protected long ExpiryDelay { get; set; }
        private double TimeCreated { get; set; }

        protected abstract void DeserializeImpl(IIncomingMessageStream messageStream);
        protected abstract void SerializeImpl(IOutgoingMessageStream messageStream);

        private static int GetNextId()
        {
            var taken = false;
            while (!taken)
                _idLock.Enter(ref taken);

            var id = _nextId++;
            _idLock.Exit();

            return id;
        }

        protected Message()
        {
            TimeCreated = Timer.GetTime();
            DeliveryMethod = DeliveryMethod.ReliableOrdered;
            Channel = 0;

            // by default give each message an instant expiry
            // ie if you can't be processed, then throw it away...
            ExpiryDelay = DefaultExpireTime;
        }

        [Conditional("DEBUG")]
        private void DeerializeId(IIncomingMessageStream messageStream)
        {
            Id = messageStream.ReadInt32();
            Logger.Write(string.Format("Deserialised message {0} from {1}", Id, Sender == null ? "Server" : Sender.PlayerName), LoggerLevel.Trace);
        }

        [Conditional("DEBUG")]
        private void SerializeId(IOutgoingMessageStream messageStream)
        {
            Id = GetNextId();
            messageStream.WriteInt32(Id);

            Logger.Write(string.Format("Serialized message {0}", Id), LoggerLevel.Trace);
        }

        public void Deserialize(IIncomingMessageStream messageStream)
        {
            DeerializeId(messageStream);
            var hasExpiry = messageStream.ReadBoolean();
            if (hasExpiry)
            {
                ExpiryDelay = messageStream.ReadInt64();
            }
            DeserializeImpl(messageStream);
        }


        public void Serialize(IOutgoingMessageStream messageStream)
        {
            SerializeId(messageStream);
            if (Math.Abs(ExpireTime - DefaultExpireTime) < 0.0001f)
            {
                messageStream.WriteBool(false);
            }
            else
            {
                messageStream.WriteBool(true);
                messageStream.WriteInt64(ExpiryDelay);
            }
            SerializeImpl(messageStream);
        }

        public bool HasExpired()
        {
            return false;
            var now = Timer.GetTime();
            return HasExpired(now);
        }

        public bool HasExpired(double now)
        {
            return false;
            if (ExpiryDelay == DoesNotExpire)
                return false;
            return ExpireTime < now;
        }

        /// <summary>
        /// Return a list of entity Ids that this message requires to exist.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<int> EntityIds()
        {
            return null;
        }

        public virtual IEnumerable<Message> SubMessages()
        {
            return new List<Message>();
        }
    }
}