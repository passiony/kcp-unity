using System;
using System.Net;
using System.Threading;
using  UnityEngine;

namespace Network
{
	public class ServerManager : MonoSingleton<ServerManager>
	{
		public AService Service { get; private set; }
		public Session Session { get; private set; }
		
		public IMessagePacker MessagePacker { get; set; }
		public IMessageDispatcher MessageDispatcher { get; set; }

		public Action<int> OnConnect{ get; set; }
		public Action<int> OnError{ get; set; }
		public Action<byte[]> ReceiveBytesHandle{ get; set; }
				
		
		public override void Init()
		{
			SynchronizationContext.SetSynchronizationContext(OneThreadSynchronizationContext.Instance);
		}
		
		//clinet
		public void InitService(NetworkProtocol protocol, int packetSize = Packet.PacketSizeLength4)
		{
			switch (protocol)
			{
				case NetworkProtocol.TCP:
					this.Service = new TServiceServer(packetSize) { };
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
			Session.Start();
		}

		/// <summary>
		/// 创建一个新Session
		/// </summary>
		public void Connect(string address)
		{
			AChannel channel = this.Service.ConnectChannel(address);
			Session = new Session(channel);
			Session.Start();
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
			Debug.Log("send message：" + data);

			Session.Send(data);
		}
		
		public void Dispose()
		{
			Session.Dispose();
		}
	}
}