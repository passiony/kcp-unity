using System;
using System.IO;
using System.Net;
using UnityEngine;

namespace Network
{
	public enum ChannelType
	{
		Connect,
		Accept,
	}

	public abstract class AChannel
	{
		public ChannelType ChannelType { get; }

		public AService Service { get; }

		public abstract MemoryStream Stream { get; }
		
		public int Error { get; set; }

		public IPEndPoint RemoteAddress { get; protected set; }

		
		
		private Action<AChannel, int> connectCallback;

		public event Action<AChannel, int> ConnectCallback
		{
			add
			{
				this.connectCallback += value;
			}
			remove
			{
				this.connectCallback -= value;
			}
		}

		
		private Action<AChannel, int> errorCallback;

		public event Action<AChannel, int> ErrorCallback
		{
			add
			{
				this.errorCallback += value;
			}
			remove
			{
				this.errorCallback -= value;
			}
		}
		
		private Action<MemoryStream> readCallback;

		public event Action<MemoryStream> ReadCallback
		{
			add
			{
				this.readCallback += value;
			}
			remove
			{
				this.readCallback -= value;
			}
		}

		public void OnConnect(int code)
		{
			Debug.Log("connect success");
			this.connectCallback.Invoke(this, code);
		}
		
		protected void OnRead(MemoryStream memoryStream)
		{
			this.readCallback.Invoke(memoryStream);
		}

		protected void OnError(int e)
		{
			this.Error = e;
			this.errorCallback?.Invoke(this, e);
		}

		protected AChannel(AService service, ChannelType channelType)
		{
			this.ChannelType = channelType;
			this.Service = service;
		}

		public abstract void Start();
		
		public abstract void Send(MemoryStream stream);
		
		public void Dispose()
		{
			this.Service.Dispose();
		}
	}
}