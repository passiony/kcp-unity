using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.IO;
using UnityEngine;

namespace UNetwork
{
	/// <summary>
	/// TChannle的一个封装管理
	/// </summary>
	public sealed class TService : AService
	{
		private TChannel channel;

		public RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();
		
		public int PacketSizeLength { get; }
		
		public TService(int packetSizeLength)
		{
			this.PacketSizeLength = packetSizeLength;
		}
		
		public override void Dispose()
		{
			this.channel.Dispose();
		}
		
		public override AChannel GetChannel()
		{
			return channel;
		}

		public override AChannel ConnectChannel(IPEndPoint ipEndPoint)
		{
			channel = new TChannel(ipEndPoint, this);
			return channel;
		}

		public override AChannel ConnectChannel(string address)
		{
			IPEndPoint ipEndPoint = NetworkHelper.ToIPEndPoint(address);
			return this.ConnectChannel(ipEndPoint);
		}

		public override void Update()
		{
			if (channel.IsSending)
			{
				return;
			}
			
			try
			{
				channel.StartSend();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}
	}
}