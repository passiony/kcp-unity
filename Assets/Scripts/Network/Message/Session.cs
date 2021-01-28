using System;
using System.IO;
using System.Net;
using UnityEngine;

namespace Network
{
	public sealed class Session
	{
		private AChannel channel;
		
		private readonly byte[] opcodeBytes = new byte[2];

		public NetworkManager Network
		{
			get { return NetworkManager.Instance; }
		}

		public int Error
		{
			get
			{
				return this.channel.Error;
			}
			set
			{
				this.channel.Error = value;
			}
		}

		public Session(AChannel aChannel)
		{
			this.channel = aChannel;

			channel.ConnectCallback += OnConnect;
			channel.ErrorCallback += OnError;
			channel.ReadCallback += OnRead;
		}
		
		private void OnConnect(AChannel channel, int code)
		{
			if (Network.OnConnect != null)
			{
				Network.OnConnect.Invoke(0);
			}
			Debug.Log("OnConnect" + code);
		}
		
		private void OnError(AChannel channel, int code)
		{
			if (Network.OnError != null)
			{
				Network.OnError.Invoke(code);
			}
			Debug.LogError("OnError:" + code);
			this.Dispose();
		}

		public void Dispose()
		{
			int error = this.channel.Error;
			if (this.channel.Error != 0)
			{
				Debug.LogError($"session dispose: ErrorCode: {error}, please see ErrorCode.cs!");
			}
			
			this.channel.Dispose();
		}

		public void Start()
		{
			this.channel.Start();
		}

		public IPEndPoint RemoteAddress
		{
			get
			{
				return this.channel.RemoteAddress;
			}
		}

		public ChannelType ChannelType
		{
			get
			{
				return this.channel.ChannelType;
			}
		}

		public MemoryStream Stream
		{
			get
			{
				return this.channel.Stream;
			}
		}

		private void Run(MemoryStream memoryStream)
		{
			memoryStream.Seek(0, SeekOrigin.Begin);
			Network.MessageDispatcher.Dispatch(this, memoryStream.GetBuffer());
			Network.ReceiveBytesHandle?.Invoke(memoryStream.GetBuffer());
		}
		
		public void OnRead(MemoryStream memoryStream)
		{
			try
			{
				this.Run(memoryStream);
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}
		
		public void Send(ushort opcode)
		{
			MemoryStream stream = this.Stream;
			stream.Seek(0, SeekOrigin.Begin);
			stream.SetLength(Packet.MessageIndex);
			
			opcodeBytes.WriteTo(0, opcode);
			Array.Copy(opcodeBytes, 0, stream.GetBuffer(), 0, opcodeBytes.Length);
						
			this.Send(stream);
		}

		public void Send(byte[] buffers)
		{
			MemoryStream stream = this.Stream;
			stream.Seek(0, SeekOrigin.Begin);
			stream.SetLength(buffers.Length);
			
			Array.Copy(buffers, 0, stream.GetBuffer(), 0, buffers.Length);
			this.Send(stream);
		}
		
		public void Send(MemoryStream stream)
		{
			channel.Send(stream);
		}
	}
}