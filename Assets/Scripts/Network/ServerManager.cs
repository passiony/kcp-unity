using System;
using System.Net;
using System.Threading;

namespace UNetwork
{
    /// <summary>
    /// Server 服务器的 业务逻辑 管理器
    /// </summary>
    public class ServerManager : MonoSingleton<ServerManager>, INetworkManager
    {
        public AService Service { get; private set; }
        public SessionServer Session { get; private set; }

        public IMessagePacker MessagePacker { get; set; }
        public IMessageDispatcher MessageDispatcher { get; set; }

        public Action<int> OnConnect { get; set; }
        public Action<int> OnError { get; set; }
        public Action<byte[]> OnMessage { get; set; }


        public override void Init()
        {
            SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);
        }

        //server
        public void InitService(NetworkProtocol protocol, int packetSize = Packet.PacketSizeLength4)
        {
            switch (protocol)
            {
                case NetworkProtocol.TCP:
                    this.Service = new TServiceServer(packetSize);
                    break;
            }
        }

        /// <summary>
        /// 创建一个新Session
        /// </summary>
        public void Connect(IPEndPoint ipEndPoint)
        {
            AChannel channel = this.Service.ConnectChannel(ipEndPoint);
            Session = new SessionServer(channel);
            Session.Start(this);
        }

        /// <summary>
        /// 创建一个新Session
        /// </summary>
        public void Connect(string address)
        {
            AChannel channel = this.Service.ConnectChannel(address);
            Session = new SessionServer(channel);
            Session.Start(this);
        }

        public void Connect(string ip, int port)
        {
            AChannel channel = this.Service.ConnectChannel(NetworkHelper.ToIPEndPoint(ip, port));
            Session = new SessionServer(channel);
            Session.Start(this);
        }

        public void Update()
        {
            OneThreadSynchronizationContext.Instance.Update();

            if (this.Service == null)
            {
                return;
            }

            this.Service.Update();
        }

        public void Send(byte[] data)
        {
            Session.Send(data);
        }

        public override void Dispose()
        {
            Session.Dispose();
        }
    }
}