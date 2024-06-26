﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Microsoft.IO;
using UnityEngine;

namespace Network
{
	public sealed class TServiceServer : AService
	{
		private readonly SocketAsyncEventArgs innArgs = new SocketAsyncEventArgs();
		private TChannelServer channel;

		public RecyclableMemoryStreamManager MemoryStreamManager = new RecyclableMemoryStreamManager();
		
		public int PacketSizeLength { get; }
		
		private bool isSending;

		private bool isRecving;

		public TServiceServer(int packetSizeLength)
		{
			this.PacketSizeLength = packetSizeLength;
		}
		
		public override void Dispose()
		{
			this.channel.Dispose();
			this.innArgs.Dispose();
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
		public bool IsSending => this.isSending;
		public override void Update()
		{
			// if (channel.IsSending)
			// {
			// 	return;
			// }
			//
			// try
			// {
			// 	channel.StartSend();
			// }
			// catch (Exception e)
			// {
			// 	Debug.LogError(e);
			// }
		}
	}
}