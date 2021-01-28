using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.IO;
using UnityEngine;

namespace Network
{
	public static class KcpProtocalType
	{
		public const short SYN = 101;
		public const short ACK = 102;
		public const short FIN = 103;
		public const short MSG = 104;
	}

	public sealed class KService : AService
	{
		public static KService Instance { get; private set; }

		// KService创建的时间
		public long StartTime{ get; private set; }
		
		// 当前时间 - KService创建的时间
		public uint TimeNow { get; private set; }

		private Socket socket;

		private KChannel channel;
		
		private readonly byte[] cache = new byte[8192];

		public readonly RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();

		private EndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 0);

		public KService()
		{
			this.StartTime = TimeHelper.ClientNow();
			this.TimeNow = (uint)(TimeHelper.ClientNow() - this.StartTime);
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			//this.socket.Blocking = false;
			this.socket.Bind(new IPEndPoint(IPAddress.Any, 0));

			Instance = this;
		}

		public override void Dispose()
		{
			this.channel.Dispose();
			this.socket.Close();
			this.socket = null;
			Instance = null;
		}

		public void Recv()
		{
			if (this.socket == null)
			{
				return;
			}

			while (socket != null && this.socket.Available > 0)
			{
				int messageLength = 0;
				try
				{
//					switch (flag)
//					{
//						case KcpProtocalType.ACK: // 连接
//							this.channel.OnConnect((int)SocketError.Success);
//							break;
//						case KcpProtocalType.FIN: // 断开
//							break;
//					}

					messageLength = this.socket.ReceiveFrom(this.cache, ref this.ipEndPoint);
					if (messageLength < 1)
					{
						continue;
					}
					
					
					this.channel.HandleRecv(this.cache, 0, messageLength);
				}
				catch (Exception e)
				{
					Debug.LogError(e);
				}
			}
		}

		public KChannel GetKChannel()
		{
			return (KChannel)channel;
		}

		public override AChannel GetChannel()
		{
			return this.channel;
		}

#if dynamic_kcp
		public static void Output(IntPtr bytes, int count, IntPtr user)
#else
		public static void Output(byte[] bytes, int count, object user)
#endif
		{
			if (Instance == null)
			{
				return;
			}
			KChannel kChannel = Instance.GetKChannel();
			if (kChannel == null)
			{
				Debug.LogError($"not found kchannel, {(uint)user}");
				return;
			}

			kChannel.Output(bytes, count);
		}

		public override AChannel ConnectChannel(IPEndPoint remoteEndPoint)
		{
			uint localConn = (uint)RandomHelper.RandomNumber(1000, int.MaxValue);
			this.channel = new KChannel(localConn, this.socket, remoteEndPoint, this);
			return channel;
		}

		public override AChannel ConnectChannel(string address)
		{
			IPEndPoint ipEndPoint2 = NetworkHelper.ToIPEndPoint(address);
			return this.ConnectChannel(ipEndPoint2);
		}

		public override void Update()
		{
			if (Instance == null)
			{
				return;
			}
			
			this.TimeNow = (uint) (TimeHelper.ClientNow() - this.StartTime);

			this.Recv();
			
			this.channel.Update();
		}
	}
}