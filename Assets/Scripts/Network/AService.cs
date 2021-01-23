using System;
using System.Net;

namespace Network
{
	public enum NetworkProtocol
	{
		KCP,
		TCP,
		WebSocket,
	}

	public abstract class AService
	{
		public abstract AChannel GetChannel();

		public abstract AChannel ConnectChannel(IPEndPoint ipEndPoint);
		
		public abstract AChannel ConnectChannel(string address);

		public abstract void Update();

		public abstract void Dispose();
	}
}