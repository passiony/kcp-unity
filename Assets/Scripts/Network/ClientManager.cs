using System;
using System.Net;
using System.Threading;

namespace UNetwork
{
    /// <summary>
    /// 客户端的 业务逻辑 管理类
    /// </summary>
    public class ClientManager : MonoSingleton<ClientManager>, INetworkManager
    {
        public AService Service { get; private set; }
        public Session Session { get; private set; }

        public IMessagePacker MessagePacker { get; set; }
        public IMessageDispatcher MessageDispatcher { get; set; }

        public Action<int> OnConnect { get; set; }
        public Action<int> OnError { get; set; }
        public Action<byte[]> OnMessage { get; set; }


        public override void Init()
        {
            SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);
        }

        //clinet
        public void InitService(NetworkProtocol protocol, int packetSize = Packet.PacketSizeLength4)
        {
            switch (protocol)
            {
                case NetworkProtocol.KCP:
                    this.Service = new KService() { };
                    break;
                case NetworkProtocol.TCP:
                    this.Service = new TService(packetSize) { };
                    break;
                case NetworkProtocol.WebSocket:
                    this.Service = new WService() { };
                    break;
            }
        }

        /// <summary>
        /// 创建一个新Session
        /// </summary>
        public void Connect(IPEndPoint ipEndPoint)
        {
            AChannel channel = this.Service.ConnectChannel(ipEndPoint);
            Session = new Session(channel);
            Session.Start(this);
        }

        /// <summary>
        /// 创建一个新Session
        /// </summary>
        public void Connect(string address)
        {
            AChannel channel = this.Service.ConnectChannel(address);
            Session = new Session(channel);
            Session.Start(this);
        }

        public void Connect(string ip, int port)
        {
            AChannel channel = this.Service.ConnectChannel(NetworkHelper.ToIPEndPoint(ip, port));
            Session = new Session(channel);
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