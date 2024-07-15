using System;
using System.IO;
using System.Net;
using UnityEngine;

namespace UNetwork
{
    public sealed class SessionConnector
    {
        private AChannel channel;

        public SessionConnector(AChannel aChannel)
        {
            this.channel = aChannel;

            channel.ConnectCallback += OnConnect;
            channel.ErrorCallback += OnError;
            channel.ReadCallback += OnRead;
        }

        private void OnConnect(AChannel channel, int code)
        {
            Debug.Log("OnConnect" + code);
        }

        private void OnError(AChannel channel, int code)
        {
            Debug.LogError("OnError:" + code);
        }

        private void Run(MemoryStream memoryStream)
        {
            memoryStream.Seek(0, SeekOrigin.Begin);
            var bytes = new byte[memoryStream.Length];
            memoryStream.Read(bytes, 0, bytes.Length);

            ServerManager.Instance.OnMessage?.Invoke(bytes);
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
    }
}