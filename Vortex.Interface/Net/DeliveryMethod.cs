namespace Vortex.Interface.Net
{
    public enum DeliveryMethod
    {
        Unknown = 0,
        Unreliable = 1,
        UnreliableSequenced = 2,
        ReliableUnordered = 3,
        ReliableSequenced = 4,
        ReliableOrdered = 5
    }
}