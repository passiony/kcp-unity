using System;

namespace UNetwork
{
    public interface INetworkManager
    {
        public IMessagePacker MessagePacker { get; set; }

        public IMessageDispatcher MessageDispatcher { get; set; }

        public Action<int> OnConnect{ get; set; }
        public Action<int> OnError{ get; set; }
        public Action<byte[]> OnMessage{ get; set; }
    }
}