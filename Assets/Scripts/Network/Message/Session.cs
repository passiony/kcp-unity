using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ETModel
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
			channel.ErrorCallback += (c, e) =>
			{
				Debug.LogError("Error:"+e);
				Network.Session.Dispose();
			};
			channel.ReadCallback += this.OnRead;
		}
		
		public void Dispose()
		{
			Network.Session.Dispose();
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
			memoryStream.Seek(Packet.MessageIndex, SeekOrigin.Begin);
			ushort opcode = BitConverter.ToUInt16(memoryStream.GetBuffer(), Packet.OpcodeIndex);

			Debug.Log("receive msg："+opcode);
			
//			object message = this.Network.MessagePacker.DeserializeFrom(null, memoryStream);
//			Network.MessageDispatcher.Dispatch(this, opcode, message);
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
		
		public void Send(ushort opcode, object message=null)
		{
			Debug.Log("send message："+opcode);
			MemoryStream stream = this.Stream;
			
			stream.Seek(Packet.MessageIndex, SeekOrigin.Begin);
			stream.SetLength(Packet.MessageIndex);
//			this.Network.MessagePacker.SerializeTo(message, stream);
			stream.Seek(0, SeekOrigin.Begin);
			
			opcodeBytes.WriteTo(0, opcode);
			Array.Copy(opcodeBytes, 0, stream.GetBuffer(), 0, opcodeBytes.Length);

			this.Send(stream);
		}

		public void Send(MemoryStream stream)
		{
			channel.Send(stream);
		}
	}
}