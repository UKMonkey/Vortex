using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Psy.Core.Logging;
using Vortex.Interface.Net;

namespace Vortex.Net
{
    public class MessageIdFactory
    {
        private readonly Dictionary<Type, byte> _messageToMsgId;
        private readonly Dictionary<byte, Type> _msgIdToMessage;


        public MessageIdFactory()
        {
            _messageToMsgId = new Dictionary<Type, byte>();
            _msgIdToMessage = new Dictionary<byte, Type>();
            var msgs = DiscoverMessages();
            AssignMessages(msgs);
        }


        private void AssignMessages(List<Type> msgs)
        {
            _messageToMsgId.Clear();
            _msgIdToMessage.Clear();
            msgs.Sort((t1, t2) => String.CompareOrdinal(t1.Name, t2.Name));
            byte id = 0;

            foreach (var msg in msgs)
            {
                _messageToMsgId.Add(msg, id);
                _msgIdToMessage.Add(id, msg);
                Logger.Write(String.Format("Assigned id {0} to message {1}", id, msg.Name));
                ++id;
                if (id == 0)
                {
                    throw new InvalidProgramException("Unable to support more than 265 messages!");
                }
            }
        }


        public byte GetMessageId(Type tp)
        {
            byte value;

            if (!_messageToMsgId.TryGetValue(tp, out value))
            {
                throw new InvalidOperationException(String.Format("Type {0} not located in message id factory", tp.Name));
            }

            return value;
        }


        public Type GetMessageType(byte type)
        {
            Type value;

            if (!_msgIdToMessage.TryGetValue(type, out value))
            {
                throw new InvalidOperationException(String.Format("Type {0} not located in message id factory", type));
            }

            return value;
        }

        private List<Type> DiscoverMessages()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var availableMessages = new List<Type>();

            foreach (var assembly in loadedAssemblies)
            {
                availableMessages.AddRange(DiscoverMessagesInAssembly(assembly));
            }

            return availableMessages;
        }


        private IEnumerable<Type> DiscoverMessagesInAssembly(Assembly assembly)
        {
            var availableTypes = assembly.GetExportedTypes();
            return availableTypes.Where(item => item.IsSubclassOf(typeof(Message)));
        }
    }
}
