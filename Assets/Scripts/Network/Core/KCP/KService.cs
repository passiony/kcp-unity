using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.IO;
using UnityEngine;

namespace ETModel
{
	public static class KcpProtocalType
	{
		public const byte SYN = 1;
		public const byte ACK = 2;
		public const byte FIN = 3;
		public const byte MSG = 4;
	}

	public sealed class KService : AService
	{
		public static KService Instance { get; private set; }

		private uint IdGenerater = 1000;

		// KService创建的时间
		public long StartTime;
		// 当前时间 - KService创建的时间
		public uint TimeNow { get; private set; }

		private Socket socket;

		private KChannel channel;
		
		private readonly byte[] cache = new byte[8192];

		public RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();

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

		public static void Output(IntPtr bytes, int count, IntPtr user)
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

		// 客户端channel很少,直接每帧update所有channel即可,这样可以消除TimerOut方法的gc
		public void AddToUpdateNextTime(long time, long id)
		{
		}
		
		public override void Update()
		{
			this.TimeNow = (uint) (TimeHelper.ClientNow() - this.StartTime);

			this.Recv();
			
			this.channel.Update();
		}
	}
}