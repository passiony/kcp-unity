using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.IO;
using UnityEngine;

namespace UNetwork
{
	public sealed class TServiceServer : AService
	{
		private TChannelServer channel;

		public RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();
		
		public int PacketSizeLength { get; }

		public TServiceServer(int packetSizeLength)
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
			channel = new TChannelServer(ipEndPoint, this);
			return channel;
		}

		public override AChannel ConnectChannel(string address)
		{
			IPEndPoint ipEndPoint = NetworkHelper.ToIPEndPoint(address);
			return this.ConnectChannel(ipEndPoint);
		}
		public override void Update()
		{
			try
			{
				channel.Update();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}
	}
}