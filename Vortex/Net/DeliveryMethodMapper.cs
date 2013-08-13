using System.Collections.Generic;
using Lidgren.Network;
using Vortex.Interface.Net;

namespace Vortex.Net
{
    public class DeliveryMethodMapper
    {
        private static readonly Dictionary<DeliveryMethod, NetDeliveryMethod>
            DeliveryMethod = 
                new Dictionary<DeliveryMethod, NetDeliveryMethod>
                {
                    {Interface.Net.DeliveryMethod.Unknown, NetDeliveryMethod.Unknown},
                    {Interface.Net.DeliveryMethod.UnreliableSequenced, NetDeliveryMethod.UnreliableSequenced},
                    {Interface.Net.DeliveryMethod.Unreliable, NetDeliveryMethod.Unreliable},
                    {Interface.Net.DeliveryMethod.ReliableSequenced, NetDeliveryMethod.ReliableSequenced},
                    {Interface.Net.DeliveryMethod.ReliableOrdered, NetDeliveryMethod.ReliableOrdered},
                    {Interface.Net.DeliveryMethod.ReliableUnordered, NetDeliveryMethod.ReliableUnordered}
                };

        public static NetDeliveryMethod Map(DeliveryMethod deliveryMethod)
        {
            return DeliveryMethod[deliveryMethod];
        }
    }
}